const { test, expect } = require('@playwright/test');

let createdLessonId = null;

test.afterAll(async ({ request }) => {
  if (createdLessonId) {
    await request.delete(`/api/lessons/${createdLessonId}`);
    createdLessonId = null;
  }
});

async function ensureLesson(page) {
  const response = await page.request.get('/api/lessons');
  if (!response.ok()) {
    throw new Error(`GET /api/lessons failed with ${response.status()}`);
  }
  const lessons = await response.json();
  if (Array.isArray(lessons) && lessons.length > 0) {
    return;
  }

  const payload = {
    title: `E2E Lesson ${Date.now()}`,
    difficulty: { case: 'A1' },
    contentType: { case: 'Words' },
    language: { case: 'French' },
    textContent: 'bonjour monde e2e test',
  };

  const createResponse = await page.request.post('/api/lessons', { data: payload });
  if (createResponse.status() !== 201) {
    throw new Error(`POST /api/lessons failed with ${createResponse.status()}`);
  }
  const created = await createResponse.json();
  createdLessonId = created.id || created.Id || createdLessonId;
}

async function openFirstLesson(page) {
  await ensureLesson(page);
  const lessonsResponse = page.waitForResponse((response) =>
    response.url().includes('/api/lessons') && response.status() === 200
  );
  await page.goto('/');
  await page.locator('.start-screen').waitFor();
  await lessonsResponse;
  await page.locator('.lesson-card').first().waitFor();
  await page.locator('.lesson-card').first().click();
  await page.locator('.typing-view').waitFor();
}

async function completeLesson(page) {
  await page.locator('.lesson-text').waitFor();
  const lessonText = (await page.locator('.lesson-text').textContent()) || '';
  expect(lessonText.length).toBeGreaterThan(0);

  await page.getByRole('button', { name: /^Start Typing$/ }).click();
  await page.locator('.typing-view').click();
  await page.keyboard.type(lessonText, { delay: 1 });

  await expect(page.getByText('Typing Complete!')).toBeVisible();
}

test('typing flow completes and submits', async ({ page }) => {
  await openFirstLesson(page);
  await completeLesson(page);

  await page.getByRole('button', { name: 'Submit Results' }).click();
  await expect(page.locator('.start-screen')).toBeVisible();
});

test('shows error when session submit fails', async ({ page }) => {
  await page.route('**/api/sessions', async (route) => {
    if (route.request().method() === 'POST') {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Server error' }),
      });
    } else {
      await route.continue();
    }
  });

  await openFirstLesson(page);
  await completeLesson(page);

  await page.getByRole('button', { name: 'Submit Results' }).click();
  await expect(page.locator('.alert-message')).toContainText('Server error');
});

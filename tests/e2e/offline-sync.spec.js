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
    textContent: 'offline sync test lesson',
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

test('offline submission stores locally and syncs when online', async ({ page, context }) => {
  await openFirstLesson(page);
  await completeLesson(page);

  await context.setOffline(true);
  await page.getByRole('button', { name: 'Submit Results' }).click();
  await expect(page.locator('.alert-message')).toContainText(/offline|Unable to reach the server/i);

  await page.getByRole('button', { name: 'Statistics' }).click();
  await expect(page.getByText(/Pending local sessions: 1/)).toBeVisible();

  await context.setOffline(false);
  await page.waitForTimeout(2200);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));

  await expect(page.getByText(/Pending local sessions:/)).toHaveCount(0);
});

const { test, expect } = require('@playwright/test');

const createdLessonIds = [];

async function createLesson(page, payload) {
  const response = await page.request.post('/api/lessons', { data: payload });
  if (response.status() !== 201) {
    throw new Error(`POST /api/lessons failed with ${response.status()}`);
  }
  const created = await response.json();
  const lessonId = created.id || created.Id;
  if (lessonId) {
    createdLessonIds.push(lessonId);
  }
  return created;
}

test.afterAll(async ({ request }) => {
  for (const lessonId of createdLessonIds) {
    await request.delete(`/api/lessons/${lessonId}`);
  }
});

test('training aids persist and hide keyboard when disabled', async ({ page }) => {
  await page.addInitScript(() => {
    if (!window.sessionStorage.getItem('e2e-storage-cleared')) {
      window.localStorage.clear();
      window.sessionStorage.setItem('e2e-storage-cleared', '1');
    }
  });

  const title = `E2E Training Aids ${Date.now()}`;
  await createLesson(page, {
    title,
    difficulty: { case: 'A1' },
    contentType: { case: 'Words' },
    language: { case: 'French' },
    textContent: 'asdf jkl; asdf jkl;'
  });

  await page.goto('/');
  await page.locator('.start-screen').waitFor();

  const colorToggle = page.getByRole('checkbox', { name: 'Color letters' });
  const showKeyboardToggle = page.getByRole('checkbox', { name: 'Show keyboard' });
  const highlightToggle = page.getByRole('checkbox', { name: 'Highlight next key' });

  await expect(colorToggle).toBeChecked();
  await expect(showKeyboardToggle).toBeChecked();
  await expect(highlightToggle).toBeChecked();

  await colorToggle.click();
  await showKeyboardToggle.click();

  await expect(colorToggle).not.toBeChecked();
  await expect(showKeyboardToggle).not.toBeChecked();
  await expect(highlightToggle).not.toBeChecked();
  await expect(highlightToggle).toBeDisabled();

  await page.reload();
  await page.locator('.start-screen').waitFor();

  await expect(colorToggle).not.toBeChecked();
  await expect(showKeyboardToggle).not.toBeChecked();
  await expect(highlightToggle).not.toBeChecked();
  await expect(highlightToggle).toBeDisabled();

  await page.locator('.lesson-card', { hasText: title }).click();
  await page.locator('.typing-view').waitFor();
  await page.getByRole('button', { name: /^Start Typing$/ }).click();
  await page.locator('.typing-view').click();

  await expect(page.locator('.lesson-text-display.no-letter-colors')).toBeVisible();
  await expect(page.locator('.keyboard')).toHaveCount(0);
});

test('next-key highlight includes Enter on newline', async ({ page }) => {
  await page.addInitScript(() => {
    if (!window.sessionStorage.getItem('e2e-storage-cleared')) {
      window.localStorage.clear();
      window.sessionStorage.setItem('e2e-storage-cleared', '1');
    }
  });

  const title = `E2E Keyboard Highlight ${Date.now()}`;
  await createLesson(page, {
    title,
    difficulty: { case: 'A1' },
    contentType: { case: 'Words' },
    language: { case: 'French' },
    textContent: 'a\nb'
  });

  await page.goto('/');
  await page.locator('.start-screen').waitFor();
  await page.locator('.lesson-card', { hasText: title }).click();
  await page.locator('.typing-view').waitFor();

  await page.getByRole('button', { name: /^Start Typing$/ }).click();
  await page.locator('.typing-view').click();

  const nextKey = page.locator('.keyboard .key-next');
  await expect(page.locator('.keyboard')).toBeVisible();
  await expect(nextKey).toHaveText('A');

  await page.keyboard.type('a');
  await expect(nextKey).toHaveText('Enter');
});

test('keyboard shows correct and error key states', async ({ page }) => {
  await page.addInitScript(() => {
    if (!window.sessionStorage.getItem('e2e-storage-cleared')) {
      window.localStorage.clear();
      window.sessionStorage.setItem('e2e-storage-cleared', '1');
    }
  });

  const title = `E2E Keyboard States ${Date.now()}`;
  await createLesson(page, {
    title,
    difficulty: { case: 'A1' },
    contentType: { case: 'Words' },
    language: { case: 'French' },
    textContent: 'abc'
  });

  await page.goto('/');
  await page.locator('.start-screen').waitFor();
  await page.locator('.lesson-card', { hasText: title }).click();
  await page.locator('.typing-view').waitFor();

  await page.getByRole('button', { name: /^Start Typing$/ }).click();
  await page.locator('.typing-view').click();

  await page.keyboard.type('x');
  await expect(page.locator('.keyboard .key-error')).toHaveText('X');

  await page.keyboard.type('b');
  await expect(page.locator('.keyboard .key-correct')).toHaveText('B');
});

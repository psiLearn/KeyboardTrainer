const { test, expect } = require('@playwright/test');

test('start screen shows error when lessons fail to load', async ({ page }) => {
  await page.route('**/api/lessons', async (route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Failed to retrieve lessons' }),
      });
    } else {
      await route.continue();
    }
  });

  await page.goto('/');
  await expect(page.locator('.alert-message')).toContainText('Failed to retrieve lessons');
  await expect(page.getByRole('button', { name: 'Retry' })).toBeVisible();
});

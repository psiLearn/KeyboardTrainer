const { test, expect } = require('@playwright/test');

test('metrics view shows empty state without sessions', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('button', { name: 'Statistics' }).click();

  await expect(page.getByRole('heading', { name: 'Your Statistics' })).toBeVisible();
  await expect(page.getByText('No Sessions Yet')).toBeVisible();
  await expect(page.getByText('Complete a typing lesson to see your statistics.')).toBeVisible();
});

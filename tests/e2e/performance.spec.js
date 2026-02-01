const { test, expect } = require('@playwright/test');

test('health endpoint handles a burst of requests', async ({ request }) => {
  test.skip(process.env.SKIP_PERF_TESTS === '1', 'Skipping perf checks by request.');

  const avgLimit = Number(process.env.PERF_AVG_MS || 2000);
  const maxLimit = Number(process.env.PERF_MAX_MS || 5000);
  const count = 20;
  const durations = await Promise.all(
    Array.from({ length: count }, async () => {
      const start = Date.now();
      const response = await request.get('/health');
      expect(response.ok()).toBeTruthy();
      await response.text();
      return Date.now() - start;
    })
  );

  const total = durations.reduce((sum, value) => sum + value, 0);
  const avg = total / count;
  const max = Math.max(...durations);

  expect(avg).toBeLessThan(avgLimit);
  expect(max).toBeLessThan(maxLimit);
});

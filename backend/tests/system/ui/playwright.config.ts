import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './specs',
  timeout: 30_000,
  use: {
    baseURL: process.env.HIRENIX_BASE_URL || 'http://localhost:5189',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  reporter: [['list'], ['html', { outputFolder: '../reports/ui/playwright-report', open: 'never' }]],
});

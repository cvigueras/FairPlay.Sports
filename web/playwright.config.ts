import { defineConfig } from '@playwright/test'

// End-to-end tests of the frontend. They only need the Vite dev server: every
// /api request is stubbed in e2e/support.ts, so no API and no database.
// The dev server is pinned to 5173 (vite.config.ts), which is what we wait for.
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  // The Vite dev server serves every module on demand; many workers at once
  // make the first page loads slow.
  workers: 2,
  timeout: 60_000,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  // The HTML report goes to playwright-report/. Locally it opens in the browser
  // after every run, pass or fail (the terminal stays busy until Ctrl+C; run with
  // `--reporter=list` to skip it). Reopen the last one: `npx playwright show-report`.
  reporter: process.env.CI
    ? [['github'], ['html', { open: 'never' }]]
    : [['list'], ['html', { open: 'always' }]],
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      // iPhone 13 with Safari's toolbars showing (stories 26, 33).
      name: 'mobile-390',
      testMatch: /.*\.mobile\.spec\.ts/,
      use: { viewport: { width: 390, height: 664 } },
    },
    {
      name: 'tablet-1024',
      testMatch: /.*\.layout\.spec\.ts/,
      use: { viewport: { width: 1024, height: 768 } },
    },
    {
      // Also runs the specs that set their own widths (breakpoint, contrast).
      name: 'desktop-1280',
      testMatch: /.*\.(layout|breakpoint)\.spec\.ts/,
      use: { viewport: { width: 1280, height: 800 } },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120_000,
  },
})

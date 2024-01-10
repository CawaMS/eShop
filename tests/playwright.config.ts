import { defineConfig, devices } from '@playwright/test';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
require('dotenv').config()

export default defineConfig({
  testDir: './tests',
  /* Maximum time one test can run for. */
  timeout: 60 * 1000,
  expect: {
    /**
     * Maximum time expect() should wait for the condition to be met.
     * For example in `await expect(locator).toHaveText();`
     */
    timeout: 6000,
  },
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: [
    ['list'],
    ['html'],
    ...(process.env.CI ? [['github'] as ['github']] : []),
  ],

  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* https://github.com/microsoft/playwright/issues/14440 - TODO - Investigate later */
    ignoreHTTPSErrors: true,
    /* Maximum time each action such as `click()` can take. Defaults to 0 (no limit). */
    actionTimeout: 0,
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL: process.env.BASEURL || 'https://cawatesteshopcache.azurewebsites.net/',
    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
  },

  projects: [
    // Setup project
    { name: 'setup', testMatch: /.*\.setup\.ts/ },
    // Test project that requires authentication
    {
      name: 'authenticated',
      testMatch: /.authenticated\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        // Use prepared auth state.
        storageState: '.auth/user.json',
      },
      dependencies: ['setup'],
    },
    // Test projects that don't require authentication
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome']
      },
    },
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox']
      },
    }
  ],
});
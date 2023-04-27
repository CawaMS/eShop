import { test as setup } from '@playwright/test';

const authFile = '.auth/user.json';

setup('authenticate', async ({ page }) => {
    await page.goto('/Identity/Account/Login');
    // Sign in using creds from env variables
    await page.getByPlaceholder('name@example.com').fill(process.env.TESTUSERNAME!);
    await page.getByPlaceholder('password').fill(process.env.TESTPASSWORD!);
    await page.getByRole('button', { name: 'Log in' }).click();
    // Save auth state to file (.gitignore'd)
    await page.context().storageState({ path: authFile });
}); 
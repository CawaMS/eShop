import { test, expect } from '@playwright/test';

test.describe('Account', () => {
    test('should be able to view account settings', async ({ page }) => {
        await page.goto('');
        await page.getByRole('link', { name: `Hello ${process.env.TESTUSERNAME!}` }).click();
        await page.getByRole('link', { name: 'Profile' }).click();
        await expect(page).toHaveURL('/Identity/Account/Manage');
        await page.getByRole('link', { name: 'Email' }).click();
        await expect(page).toHaveURL('/Identity/Account/Manage/Email');
        await page.getByRole('link', { name: 'Password' }).click();
        await expect(page).toHaveURL('/Identity/Account/Manage/ChangePassword');
        await page.getByRole('link', { name: 'Two-factor authentication' }).click();
        await expect(page).toHaveURL('/Identity/Account/Manage/TwoFactorAuthentication');
        await page.getByRole('link', { name: 'Personal data' }).click();
        await expect(page).toHaveURL('/Identity/Account/Manage/PersonalData');
        await page.getByRole('button', { name: 'Logout' }).click();
        await expect(page).toHaveURL('');
    });
});
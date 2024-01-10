import { test, expect } from '@playwright/test';

test.beforeEach(async ({ page }) => {
    await page.goto('/');
})

test.describe('Header', () => {
    test('should be able click all links', async ({ page }) => {
        await page.getByRole('link', { name: 'eShop' }).click();
        await expect(page).toHaveURL('/');
        await page.getByRole('link', { name: 'Home' }).click();
        await expect(page).toHaveURL('/');
        await page.getByRole('navigation').getByRole('link', { name: 'Privacy' }).click();
        await expect(page).toHaveURL('/Home/Privacy');
        await page.getByRole('link', { name: 'Shopping Cart' }).click();
        await expect(page).toHaveURL('/Carts');
        await page.getByRole('link', { name: 'Register' }).click();
        await expect(page).toHaveURL('/Identity/Account/Register');
        await page.getByRole('link', { name: 'Login' }).click();
        await expect(page).toHaveURL('/Identity/Account/Login');
    });
});

test.describe('Footer', () => {
    test('should display correct info', async ({ page }) => {
        const currentYear = new Date().getFullYear()
        await expect(page.getByText(`© ${currentYear} - eShop - Privacy`)).toBeVisible();
    });

    test('should display privacy link', async ({ page }) => {
        await page.getByRole('contentinfo').getByRole('link', { name: 'Privacy' }).click();
        await expect(page).toHaveURL('/Home/Privacy');
    });
});
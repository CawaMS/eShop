import { test, expect } from '@playwright/test';

// Product ID's to test
const products = process.env.PRODUCTIDS.split(',');

test.describe('Product Details', () => {
    // Loop through all productID's, view details, and add to cart
    for (const i of products) {
        test(`should be able to view and add product ${i} to cart`, async ({ page }) => {
            await page.goto(`/Home/Details/${i}`);
            await expect(page).toHaveURL(`/Home/Details/${i}`);
            await page.getByRole('button', { name: 'Add to cart' }).click();
            await expect(page).toHaveURL(`/Carts/Create`);
        });
    }
});

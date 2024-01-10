import { test, expect } from '@playwright/test';

// New product data
const name = 'test';
const price = '100.00';
const brand = 'Gucci';
const image = 'notreal.png';
const category = 'Purse'

test.describe('Admin - View | Edit', () => {
    test.skip(process.env.TESTUSERNAME !== 'admin@eshop.com', 'Admin only!');
    test('should be able to view product', async ({ page }) => {
        await page.goto('/products');
        await page.getByText('Details').first().click();
        await expect(page).toHaveURL('/Products/Details/1');
    });

    test('should be able to edit product', async ({ page }) => {
        await page.goto('/products');
        await page.getByText('Edit').first().click();
        await expect(page).toHaveURL('/Products/Edit/1');
        await page.getByText('Save').click();
    });
});

test.describe.serial('Admin - Create | Delete', () => {
    test.skip(process.env.TESTUSERNAME !== 'admin@eshop.com', 'Admin only!');
    test('should be able to create new product', async ({ page }) => {
        await page.goto('/products');
        await page.getByRole('link', { name: 'Create New' }).click();
        await page.getByLabel('Name').click();
        await page.getByLabel('Name').fill(name);
        await page.getByLabel('Price').click();
        await page.getByLabel('Price').fill(price);
        await page.getByLabel('Brand').click();
        await page.getByLabel('Brand').fill(brand);
        await page.getByLabel('Image').click();
        await page.getByLabel('Image').fill(image);
        await page.getByRole('button', { name: 'Create' }).click();
    });
    test('should be able to delete test product', async ({ page }) => {
        await page.goto('/products');
        await page.getByRole('row', { name: `${name} ¤${price} ${brand} ${image} ${category} Edit | Details | Delete` }).getByRole('link', { name: 'Delete' }).click();
        await page.getByRole('button', { name: 'Delete' }).click();
    });
});
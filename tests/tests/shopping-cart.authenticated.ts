import { test, expect, Page, TestInfo } from '@playwright/test';

test.describe('Shopping Cart', () => {
    test('should be able to checkout', async ({ page }) => {
        await page.goto('');
        await page.locator('.button').first().click();
        await page.getByText('Item added:').click();
        await expect(page).toHaveURL('/Carts/Create');
        await page.getByRole('link', { name: 'Shopping Cart' }).click();
        await page.getByRole('cell', { name: 'Items to purchase' }).click();
        await page.getByRole('cell', { name: 'Quantity' }).click();
        await page.getByRole('button', { name: 'Check Out' }).click();
    });
});

test.describe('Shopping Cart - Performance', () => {
    test('should be able to add items to cart then view shopping cart', async ({ page }, TestInfo) => {
        // Product ID's to test
        const products = process.env.PRODUCTIDS.split(',');
        // Loop through each product and add to cart
        for (const i of products) {
            await page.goto(`/Home/Details/${i}`);
            await page.getByRole('button', { name: 'Add to cart' }).click();
        }
        // Measure performance
        await measureCartPerformance(page, TestInfo);
    });

    test('should be able to add items to cart, edit product, then view shopping cart', async ({ page }, TestInfo) => {
        // Product ID's to test
        const products = process.env.PRODUCTIDS.split(',');
        // Loop through each product and add to cart
        for (const i of products) {
            await page.goto(`/Home/Details/${i}`);
            await page.getByRole('button', { name: 'Add to cart' }).click();
        }
        // Edit product to invalidate cache
        await page.goto('/products');
        await page.getByText('Edit').first().click();
        // generate random number for price
        const price = Math.floor(Math.random() * 1000) + 1;
        await page.getByLabel('Price').fill(`${price}.00`);
        await page.getByText('Save').click();
        // Measure performance
        await measureCartPerformance(page, TestInfo);
    });
});

async function measureCartPerformance(page: Page, TestInfo: TestInfo) {
    // Navigate to shopping cart
    await page.goto(`/Carts`);
    // Use Performance API to measure performance 
    // https://developer.mozilla.org/en-US/docs/Web/API/Performance/getEntriesByType
    const performance = await page.evaluate(() => performance.getEntriesByType('navigation'));
    // Get the first entry
    const performanceTiming = performance[0];
    // Get the start to load event end time
    const startToLoadEventEnd = performanceTiming.loadEventEnd - performanceTiming.startTime;
    // Add the performance annotation to the HTML report
    test.info().annotations.push({ type: 'Performance', description: `"${TestInfo.project.use.baseURL}" - Navigation start to load event end: ${startToLoadEventEnd}ms` });
    // Also output to console for debugging
    console.log(`${TestInfo.project.use.baseURL} - Navigation start to load event end: ${startToLoadEventEnd}ms`);
} 
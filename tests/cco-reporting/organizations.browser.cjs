// Controlled fixtures for the enabled and fail-closed Performance views.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const source = fs.readFileSync('AIS/wwwroot/js/cco-organizations.js', 'utf8');

(async () => {
    const browser = await chromium.launch({ channel: 'chrome', headless: true });
    try {
        for (const width of [1280, 375]) {
            const page = await browser.newPage({ viewport: { width, height: 800 } });
            let requests = 0;
            const rows = [
                { rowKind: 'GROUP', groupId: 1, groupName: 'Retail', totalCases: 2 },
                { rowKind: 'GROUP', groupId: 2, groupName: 'Wholesale', totalCases: 1 },
                { rowKind: 'DIVISION', groupId: 1, divisionId: 5, divisionName: '<img src=x>', totalCases: 2 },
                { rowKind: 'GM', groupId: 1, divisionId: 5, auditType: 'B', mappingQuality: 'Conflicting', totalCases: 2 },
                { rowKind: 'TOTAL', totalCases: 3, settlementRate: null }
            ];
            await page.route('https://ias.test/**', route => {
                const url = new URL(route.request().url());
                if (url.pathname !== '/api/performance') return route.fulfill({ contentType: 'text/html', body: '' });
                requests++;
                assert.equal(url.searchParams.get('startDate'), '2024-01-01');
                assert.equal(url.searchParams.get('endDate'), '2024-12-31');
                return route.fulfill({ json: { success: true, data: rows } });
            });
            await page.goto('https://ias.test/');
            const markup = enabled => `<section id="cco-organizations" data-enabled="${enabled}" data-url="/api/performance">
                <input id="cco-org-search"><button id="cco-expand-all">Expand</button><button id="cco-collapse-all">Collapse</button>
                <p id="cco-org-status"></p><div id="cco-org-results"></div></section>
                <input id="exec-start-date" value="2024-01-01"><input id="exec-end-date" value="2024-12-31"><select id="exec-entity"><option value="">All</option></select>`;
            await page.setContent(markup(false));
            await page.addScriptTag({ content: source });
            assert.equal(requests, 0);
            await page.setContent(markup(true));
            await page.addScriptTag({ content: source });
            await page.waitForSelector('tbody tr');
            assert.equal(requests, 1);
            assert.equal(await page.locator('tbody tr').count(), 3);
            await page.click('#cco-expand-all');
            assert.equal(await page.locator('tbody tr').count(), 5);
            assert.equal(await page.locator('img').count(), 0);
            assert.match(await page.locator('tbody').innerText(), /conflicting mapping/);
            assert.match(await page.locator('tbody tr').last().innerText(), /Grand Total[\s\S]*N\/A/);
            await page.fill('#cco-org-search', '<img');
            assert.equal(await page.locator('tbody tr').count(), 4);
            await page.locator('#exec-end-date').dispatchEvent('change');
            assert.equal(await page.locator('tbody tr').count(), 0);
            await page.close();
        }
        console.log('Performance browser fixtures passed at desktop and mobile widths.');
    } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });

const {chromium} = require('playwright'), fs = require('node:fs'), assert = require('node:assert/strict');
(async () => {
    const browser = await chromium.launch({channel: 'chrome', headless: true});
    try {
        const p = await browser.newPage();
        const rows = [
            {rowKind:'DETAIL', groupName:'B', divisionName:'<img src=x>', openingBalance:0, parasAdded:2, totalParas:2, settledParas:1, outstandingParas:3, settlementPercentage:50},
            {rowKind:'DETAIL', groupName:'A', divisionName:'Division', openingBalance:0, parasAdded:0, totalParas:0, settledParas:0, outstandingParas:1, settlementPercentage:null}
        ];
        let calls = 0, reply = {status:200, body:{success:true, data:rows}};
        await p.route('https://ias.test/**', async route => {
            if (route.request().url().includes('/api/CcoCompliance/GetFunctionPosition')) {
                calls++;
                const url = new URL(route.request().url());
                assert.equal(url.searchParams.get('startDate'), '2024-01-01');
                assert.equal(url.searchParams.get('endDate'), '2024-12-31');
                assert.equal(url.searchParams.get('maxRows'), '1000');
                return route.fulfill({status:reply.status, contentType:'application/json', body:JSON.stringify(reply.body)});
            }
            return route.fulfill({status:200, body:'<html></html>', contentType:'text/html'});
        });
        await p.goto('https://ias.test/ExecutiveOverview/CCO?view=org-position');
        await p.setContent('<section id="cco-function-position" data-api-url="/api/CcoCompliance/GetFunctionPosition"><p id="cco-function-status">Loading</p><div id="cco-function-results" hidden></div></section><input id="exec-start-date" value="2024-01-01"><input id="exec-end-date" value="2024-12-31">');
        const script = fs.readFileSync('AIS/wwwroot/js/cco-function-position.js','utf8');
        await p.addScriptTag({content:script});
        await p.locator('tbody tr').first().waitFor();
        assert.equal(calls, 1, 'one authorized API call');
        assert.deepEqual(await p.locator('th').allTextContents(), ['Group','Division','Opening balance','Para added','Total','Settled','Para outstanding','Settlement %']);
        assert.equal(await p.locator('tbody tr').first().locator('td').first().innerText(), 'A');
        assert.equal(await p.locator('img').count(), 0, 'XSS-safe text renderer');
        assert.match(await p.locator('tbody').innerText(), /N\/A/);
        assert.equal(await p.locator('tbody tr').last().locator('td').nth(6).innerText(), '3', 'independent outstanding retained');
        await p.locator('#exec-end-date').evaluate(e => {e.value='2024-01-02'; e.dispatchEvent(new Event('change'));});
        assert.equal(await p.locator('table').count(), 0);
        assert.match(await p.locator('#cco-function-status').innerText(), /Dates changed/);
        assert.equal(calls, 1, 'no request until Apply');
        await p.locator('#exec-end-date').evaluate(e => {e.value='2024-12-31';});
        reply = {status:403, body:{success:false}};
        await p.addScriptTag({content:script});
        await p.waitForFunction(() => document.getElementById('cco-function-status').textContent.includes('Access denied'));
        assert.equal(await p.locator('table').count(), 0, 'denial never leaks cached values');
        reply = {status:503, body:{success:false}};
        await p.addScriptTag({content:script});
        await p.waitForFunction(() => document.getElementById('cco-function-status').textContent.includes('unavailable'));
        assert.equal(await p.locator('table').count(), 0);
        reply = {status:200, body:{success:true, data:[]}};
        await p.addScriptTag({content:script});
        await p.waitForFunction(() => document.getElementById('cco-function-status').textContent.includes('No observations'));
        console.log('PASS: real scoped fetch, eight columns, stable totals, no injection, change clearing, 403/503, empty success');
    } finally { await browser.close(); }
})().catch(e => { console.error(e); process.exitCode=1; });

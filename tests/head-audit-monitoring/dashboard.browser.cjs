// Synthetic scoped pages exercise the actual production tab/export scripts in headless Edge.
// No Oracle, live IAS session or production data is used. NODE_PATH must include playwright.
const { chromium } = require('playwright');
const assert = require('node:assert/strict'), fs = require('node:fs'), path = require('node:path'), os = require('node:os'), http = require('node:http');
const root = path.resolve(__dirname, '../../AIS/wwwroot');
const ExcelJS = require('../../AIS/wwwroot/lib/dashboard-export/exceljs-4.4.0.min.js');
const titles = { progress: ['Audit plan progress', 'Audit office performance'], details: ['Audit plan details'], exceptions: ['Pending actions and exceptions'], register: ['Engagement register'] };
const requests = [], folder = fs.mkdtempSync(path.join(os.tmpdir(), 'ias-head-audit-'));
let authorized = true, contextKey = 'scope-a', errorStatus = 403;
function fixture(tab) {
    const query = name => '/head?auditPeriodId=77&positionAsOn=2026-10-22&officeId=9&statusId=4&exception=delayed&tab=' + name;
    return `<!doctype html><html><head><link rel="stylesheet" href="/css/engagement-workbench.css"></head><body>
    <section class="engagement-workbench wb-redesigned" data-context-key="scope-a" data-period="Audit period: FY2026 (IAS ID 77)" data-position="2026-10-22" data-scope="Authorized Office A" data-limitations="Current-state assignments; recorded workflow stages only">
    <header class="wb-hero"><h1>Head of Audit Dashboard</h1><div class="wb-hero-meta">${tab}</div></header>
    <form class="wb-dashboard-filters"><input name="tab" value="${tab}"><select id="head-period"><option value="77">FY2026</option><option value="66">FY2025</option></select><input id="head-position" type="date" value="2026-10-22" min="2026-01-01" max="2026-10-23"></form>
    <nav>${Object.keys(titles).map(name => `<a class="head-tab" ${name === tab ? 'aria-current="page"' : ''} href="${query(name)}">${name}</a>`).join(' ')}</nav>
    <div id="head-tab-content">${titles[tab].map(title => `<section data-filters="Office 9; status 4; delayed"><h2>${title}</h2><div class="wb-table"><table><thead><tr><th>Entity</th><th>Days</th><th>Completion</th></tr></thead><tbody><tr><td>Office A ${tab}</td><td>17</td><td>22 Oct 2026</td></tr><tr><td>=HYPERLINK(&quot;bad&quot;)</td><td>3</td><td>50.0%</td></tr></tbody>${tab === 'details' ? '<tfoot><tr><th>Total distinct plans: 2</th><td>20</td><td>Each PlanId once</td></tr></tfoot>' : ''}</table></div></section>`).join('')}</div></section>
    <script src="/js/head-audit-dashboard.js"></script><script src="/lib/dashboard-export/exceljs-4.4.0.min.js"></script><script src="/js/head-audit-export.js"></script></body></html>`;
}
const server = http.createServer((req, res) => {
    const url = new URL(req.url, 'http://localhost');
    if (url.pathname === '/head') {
        res.setHeader('X-Head-Audit-Context', contextKey);
        requests.push({ tab: url.searchParams.get('tab') || 'progress', check: url.searchParams.has('checkOnly') });
        if (!authorized) { res.writeHead(errorStatus); res.end('Denied'); return; }

        res.setHeader('Content-Type', 'text/html'); res.end(fixture(url.searchParams.get('tab') || 'progress')); return;
    }
    const file = path.resolve(root, '.' + url.pathname);
    if (!file.startsWith(root + path.sep) || !fs.existsSync(file)) { res.writeHead(404); res.end(); return; }
    res.setHeader('Content-Type', file.endsWith('.js') ? 'application/javascript' : 'text/css'); res.end(fs.readFileSync(file));
});
(async () => {
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    const browser = await chromium.launch({ channel: process.env.TEST_BROWSER || 'msedge', headless: true });
    try {
        const page = await browser.newPage({ viewport: { width: 1440, height: 1000 }, acceptDownloads: true });
        const errors = []; page.on('pageerror', e => errors.push(e.message));
        await page.goto(`http://127.0.0.1:${server.address().port}/head`);
        for (const tab of Object.keys(titles)) {
            if (tab !== 'progress') await page.locator('a.head-tab', { hasText: tab }).click();
            await page.waitForFunction(expected => document.querySelectorAll('#head-tab-content button').length === expected, titles[tab].length);
            assert.equal(await page.locator('#head-tab-content table').count(), titles[tab].length);
            for (let index = 0; index < titles[tab].length; index++) {
                const pending = page.waitForEvent('download');
                await page.locator('#head-tab-content button').nth(index).click();
                const download = await pending, file = path.join(folder, download.suggestedFilename()); await download.saveAs(file);
                const book = new ExcelJS.Workbook(); await book.xlsx.load(fs.readFileSync(file)); const sheet = book.worksheets[0];
                assert.equal(sheet.getCell('A1').value, titles[tab][index]);
                assert.match(sheet.getCell('A2').value, /IAS ID 77/); assert.match(sheet.getCell('A3').value, /2026-10-22/);
                assert.match(sheet.getCell('A5').value, /Office 9; status 4; delayed/);
                assert.equal(sheet.getCell('B8').value, 17); assert.ok(sheet.getCell('C8').value instanceof Date);
                assert.match(sheet.getCell('A9').value, /^'=/); assert.equal(sheet.getCell('A9').formula, undefined);
                if (tab === 'details') assert.equal(sheet.getCell('B10').value, 20);
            }
            assert.equal(new URL(page.url()).searchParams.get('tab') || 'progress', tab);
            assert.equal(await page.locator('.wb-dashboard-filters input[name="tab"]').inputValue(), tab);
        }
        assert.equal(requests.length, 9, 'Initial page, three tab reads and five fresh export reads');
        assert.ok(requests.every(r => !r.check), 'No cached authorization probes');
        await page.locator('a.head-tab', { hasText: 'progress' }).click();
        await page.waitForFunction(() => document.querySelector('.wb-hero-meta').textContent === 'progress');
        assert.equal(requests.length, 10, 'Revisiting initial tab must reload the dataset');
        assert.equal(requests.at(-1).check, false, 'Revisited tabs use secured data retrieval');
        await page.locator('#head-period').selectOption('66'); assert.equal(await page.locator('#head-position').inputValue(), '');
        contextKey = 'scope-b'; await page.locator('a.head-tab', { hasText: 'details' }).click();
        await page.waitForFunction(() => document.getElementById('head-tab-content').textContent.includes('assignment changed'));
        assert.equal(await page.locator('#head-tab-content table').count(), 0, 'Changed assignment must clear cached data');
        contextKey='scope-a'; authorized=true; await page.reload(); authorized=false;
        await page.locator('a.head-tab', { hasText: 'register' }).click();
        await page.waitForFunction(() => document.getElementById('head-tab-content').textContent.includes('assignment changed'));
        assert.equal(await page.locator('#head-tab-content table').count(), 0, 'Revoked access must clear data');
        for (const status of [401,403,400,503]) {
            authorized=true; await page.reload(); authorized=false; errorStatus=status;
            await page.locator('#head-tab-content button').first().click();
            await page.waitForFunction(() => !document.querySelector('#head-tab-content table'));
            assert.equal(await page.locator('#head-tab-content button').count(),0,'Export failure removes all protected controls');
            assert.equal(await page.locator('.wb-hero-meta').textContent(),'');
            assert.equal(await page.locator('.engagement-workbench').getAttribute('data-scope'),null);
            authorized=true; await page.locator('a.head-tab', {hasText:'details'}).click();
            await page.waitForFunction(() => !!document.querySelector('#head-tab-content table'));
        }
        // Hold a real fetch while a second tab wins: the old table disappears immediately.
        let release, reached;
        const held = new Promise(resolve => { reached=resolve; });
        await page.route('**/head?**tab=register', async route => {
            reached(); await new Promise(resolve => { release=resolve; });
            await route.fulfill({status:200,headers:{'X-Head-Audit-Context':'scope-a'},contentType:'text/html',body:fixture('register')}).catch(()=>{});
        });
        await page.locator('a.head-tab', {hasText:'register'}).click(); await held;
        assert.equal(await page.locator('#head-tab-content table').count(),0,'In-flight request hides previously authorized content');
        await page.locator('a.head-tab', {hasText:'progress'}).click();
        await page.waitForFunction(() => document.querySelector('.wb-hero-meta').textContent==='progress');
        release(); await page.unroute('**/head?**tab=register');
        assert.equal(await page.locator('#head-tab-content table').count(),2,'Aborted stale request cannot restore another tab');
        assert.deepEqual(errors, []);
        console.log('PASS: four tabs, five real browser XLSX downloads, exact displayed rows/filters, typed cells/totals, fresh repeated reads, 401/403/400/503 export clearing, and period change.');
        // Review the production modal/CSS using formatted synthetic content at desktop and narrow widths.
        const partial = fs.readFileSync(path.resolve(root, '../Views/ExecutiveOverview/_AuditParaSnapAnalysis.cshtml'), 'utf8');
        const modal = partial.match(/<dialog id="snap-para-dialog"[\s\S]*?<\/dialog>/)[0];
        await page.setContent('<link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css"><link rel="stylesheet" href="/css/audit-para-snap.css">' + modal);
        await page.evaluate(() => {
            document.getElementById('snap-para-text').innerHTML = '<h2>Critical Audit Observation</h2><p>First paragraph with <strong>bold text</strong>.</p><ul><li>Finding one</li><li>Finding two</li></ul><table><tr><th>Area</th><th>Amount</th></tr><tr><td>Review</td><td>25,000</td></tr></table>' + '<p>Long observation paragraph with readable spacing.</p>'.repeat(60);
            document.getElementById('snap-para-dialog').showModal();
        });
        for (const width of [1440, 390]) {
            await page.setViewportSize({ width, height: 900 });
            const bounds = await page.locator('#snap-para-dialog').boundingBox(); assert.ok(bounds.width >= width * .88 && bounds.width <= width * .92);
            assert.ok(bounds.height <= 810);
            assert.equal(await page.locator('.snap-para-body').evaluate(el => el.scrollHeight > el.clientHeight), true);
            assert.equal(await page.locator('#snap-para-text strong').textContent(), 'bold text');
            assert.equal(await page.locator('#snap-para-close').isVisible(), true);
            await page.screenshot({ path: path.join(folder, `para-${width}.png`) });
        }
        const hmSource = fs.readFileSync(path.resolve(root, '../Views/HM/old_paras_monitoring.cshtml'), 'utf8');
        const hmModal = await page.evaluate(source => new DOMParser().parseFromString(source, 'text/html').getElementById('viewMemoModel').outerHTML, hmSource);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.setContent('<link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css"><style>#viewMemoModel{display:block}</style>' + hmModal);
        await page.locator('#viewMemo_memo').evaluate(el => { el.innerHTML = '<h2>Critical Audit Observation</h2><p>First paragraph with <strong>bold text</strong>.</p><ul><li>Finding one</li><li>Finding two</li></ul><table class="table"><tr><th>Area</th><th>Amount</th></tr><tr><td>Review</td><td>25,000</td></tr></table>'; });
        await page.screenshot({ path: path.join(folder, 'hm-reference.png') });
        console.log('PASS: wide HM-style modal, formatted paragraphs/lists/table and long-body scrolling at desktop/mobile. Screenshots: ' + folder);
    } finally { await browser.close(); server.close(); }
})().catch(error => { console.error(error); server.close(); process.exitCode = 1; });


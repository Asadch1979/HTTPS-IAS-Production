const fs = require('fs'), path = require('path'), assert = require('assert/strict');
const { chromium } = require('playwright');
(async () => {
 const browser = await chromium.launch({channel: process.env.TEST_BROWSER || 'chrome',headless:true});
 try {
  const page=await browser.newPage();page.setDefaultTimeout(15000);
  const errors=[];page.on('pageerror',e=>errors.push(e.message));
  await page.route('http://ias.test/**', route => route.fulfill({contentType:'application/json',body:JSON.stringify({success:true,data:{rows:[]}})}));
  await page.goto('http://ias.test/');
  await page.setContent(`<section id="executive-dashboard" data-perspective="GROUP" data-scope-label="Authorized office">
  <form id="exec-filters"><input id="exec-start-date" name="startDate" type="date" value="2026-01-01"><input id="exec-end-date" name="endDate" type="date" value="2026-10-20"><input id="exec-report-selection" name="report" value="E"></form>
  <p id="exec-status"></p><section id="exec-reports" data-api-url="http://ias.test/api/" data-perspective="GROUP" data-group-root="123"><button data-report="E" data-title="Management settlement">Report E</button><article id="exec-report-panel"><h2 id="exec-report-title"></h2><p id="exec-report-period"></p><div id="exec-report-cautions"></div><div id="exec-report-content"></div></article></section></section>`);
  await page.addScriptTag({content:fs.readFileSync(path.resolve(__dirname,'../../AIS/wwwroot/js/executive-reports.js'),'utf8')});
  await page.waitForFunction(()=>document.getElementById('exec-status').textContent.includes('Report E loaded'));
  assert.equal(await page.locator('.exec-report-actions,.dashboard-export-control').count(),0);
  assert.deepEqual(errors,[]);
  console.log('PASS: ordinary-user report loads with export controls absent.');
 } finally {await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});

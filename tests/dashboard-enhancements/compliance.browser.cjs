const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs'), path = require('node:path'), http = require('node:http');
const script = fs.readFileSync(path.resolve(__dirname,'../../AIS/wwwroot/js/executive-dashboard.js'));
const perspective=process.env.TEST_PERSPECTIVE || 'CCO';
const actions=['GetKpiSummary','GetRiskSummary','GetAgingSummary','GetCycleSummary','GetSettlementTrend','GetCriticalItems'];
const fixture=`<html><body><section id="executive-dashboard" data-perspective="${perspective}" data-scope-label="Authorized test scope"
data-dashboard-type="${perspective}" data-group-root-id="123" data-view="glance" data-api-url="/api/" data-datasets="${actions.join(',')}">
<nav data-view-navigation><a href="/?view=risk">Risk</a></nav><form id="exec-filters">
<input type="date" id="exec-start-date" name="startDate" value="2024-01-01" max="2026-10-20" required>
<input type="date" id="exec-end-date" name="endDate" value="2024-02-29" max="2026-10-20" required>
<button id="exec-refresh" type="submit">Apply</button></form><p id="exec-status"></p>
<div id="exec-results" hidden><div id="exec-kpis" data-kpi-keys="openParas,settledMtd,settlementRatio"></div>
<div id="exec-risk"></div><div id="exec-aging"></div><div id="exec-cycle"></div><div id="exec-trend"></div>
<select id="exec-critical-risk"><option value="">All</option></select><p id="exec-critical-note"></p>
<table><tbody id="exec-critical"></tbody></table></div></section><script src="/client.js"></script></body></html>`;
(async()=>{
 let dated=true;const queries=[];
 const server=http.createServer((req,res)=>{
   if(req.url==='/'){res.setHeader('Content-Type','text/html');return res.end(fixture);}
   if(req.url==='/client.js'){res.setHeader('Content-Type','application/javascript');return res.end(script);}
   if(!req.url.startsWith('/api/')){res.statusCode=404;return res.end();}
   const url=new URL(req.url,'http://localhost');
   queries.push(url);res.setHeader('Content-Type','application/json');
   const data=url.pathname.endsWith('GetKpiSummary')
     ? {openParas:5,settledMtd:2,settlementRatio:20,criticalItems:0} : [];
   res.end(JSON.stringify({success:true,dateRangeApplied:dated,data}));
 }).listen(0,'127.0.0.1');
 await new Promise(resolve=>server.once('listening',resolve));
 const browser=await chromium.launch({channel:process.env.TEST_BROWSER||'chrome',headless:true});
 try{
   const page=await browser.newPage();page.setDefaultTimeout(15000);const errors=[];page.on('pageerror',e=>errors.push(e.message));
   await page.goto(`http://127.0.0.1:${server.address().port}`);
   await page.waitForFunction(()=>document.getElementById('executive-dashboard').getAttribute('aria-busy')==='false');
   assert.equal(queries.length,6);
   queries.forEach(url=>{
     assert.equal(url.searchParams.get('startDate'),'2024-01-01');
     assert.equal(url.searchParams.get('endDate'),'2024-02-29');
     assert.equal(url.searchParams.get('dashboardType'),perspective);
   });
   assert.equal(await page.locator('#exec-results').evaluate(el=>el.hidden),false);
   assert.match(await page.locator('#exec-kpis').innerText(),/Settled in Period/);
   assert.match(await page.locator('#exec-status').innerText(),/Selected period/);
   await page.fill('#exec-end-date','2024-03-31');await page.locator('#exec-end-date').dispatchEvent('change');
   assert.equal(await page.locator('#exec-results').evaluate(el=>el.hidden),true);
   assert.match(await page.locator('nav a').getAttribute('href'),/endDate=2024-03-31/);
   dated=false;await page.reload();
   await page.waitForFunction(()=>document.getElementById('exec-status').textContent.includes('date-aware'));
   assert.equal(await page.locator('#exec-results').evaluate(el=>el.hidden),true);
   assert.deepEqual(errors,[]);
   console.log('PASS: all six '+perspective+' requests carry dates, date-aware responses required, period labels and navigation persistence.');
 }finally{await browser.close();await new Promise(resolve=>server.close(resolve));}
})().catch(e=>{console.error(e);process.exitCode=1;});

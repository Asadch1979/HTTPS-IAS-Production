// Browser fixtures exercise the real shipped clients; not authenticated IAS staging.
const {chromium}=require('playwright');
const assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true});
 try {
  for(const [view,report] of [['field-office','B'],['gm-region','C'],['regional-rankings','D']]) {
   const page=await browser.newPage(),requests=[],errors=[];
   page.on('pageerror',e=>errors.push(e.message));
   await page.route('http://cco.test/**',async route=>{
    const url=new URL(route.request().url());
    if(url.pathname.startsWith('/api/')) {
     requests.push(url);
     return route.fulfill({contentType:'application/json',body:JSON.stringify({success:true,data:{rows:[]}})});
    }
    return route.fulfill({contentType:'text/html',body:`<section id="executive-dashboard" data-perspective="CCO" data-view="${view}" data-scope-label="Authorized fixture"><nav data-view-navigation><a href="/?view=overview">Overview</a><a href="/?view=gm-region">GM</a></nav><form id="exec-filters"><input name="view" value="${view}"><input id="exec-report-selection" name="report" value="${report}"><input type="date" id="exec-start-date" name="startDate" value="2024-01-01"><input type="date" id="exec-end-date" name="endDate" value="2024-02-29"></form><p id="exec-status"></p><section id="exec-reports" data-perspective="CCO" data-api-view="reports" data-api-url="/api/" data-needs-group="false"><button data-report="${report}" data-title="Fixture">Report</button><div id="exec-report-panel"><h2 id="exec-report-title"></h2><p id="exec-report-period"></p><div id="exec-report-cautions"></div><div id="exec-report-content"></div></div></section></section>`});
   });
   await page.goto('http://cco.test/');
   await page.addScriptTag({content:fs.readFileSync(path.resolve(__dirname,'../../AIS/wwwroot/js/executive-reports.js'),'utf8')});
   await page.waitForFunction(()=>document.getElementById('exec-status').textContent.includes('loaded'));
   assert.equal(requests.length,1,'Only assigned report loads');
   assert.equal(requests[0].pathname,'/api/GetReport'+report);
   assert.equal(requests[0].searchParams.get('view'),'reports');
   assert.equal(requests[0].searchParams.get('startDate'),'2024-01-01');
   assert.equal(requests[0].searchParams.get('endDate'),'2024-02-29');
   assert.equal(requests[0].searchParams.get('dashboardType'),'CCO');
   assert.equal(await page.locator('[id^="exec-report-export"]').count(),0);
   await page.fill('#exec-end-date','2024-03-31');await page.locator('#exec-end-date').dispatchEvent('change');
   assert.match(await page.locator('nav a').first().getAttribute('href'),/endDate=2024-03-31/);
   assert.equal(await page.locator('#exec-report-panel').evaluate(e=>e.hidden),true);
   assert.deepEqual(errors,[]);
   await page.close();
  }
  const overview=await browser.newPage(),seen=[],overviewErrors=[];
  overview.on('pageerror',e=>overviewErrors.push(e.message));
  await overview.route('http://overview.test/**',route=>{
   const action=new URL(route.request().url()).pathname.slice(1);seen.push(action);
   const data=action==='GetKpiLifecycle'?{openingOpenParas:10,paraAdded:2,parasSettled:3,closingOpenParas:9,openingEstimated:true,closingEstimated:true,highRiskOpenParas:1,over1YearParas:1,over3ComplianceCycles:2}:action==='GetKpiSummary'?{criticalItems:0,zeroCycleParas:0,over3CycleParas:2,settlementMtdRate:25}:[];
   return route.fulfill({contentType:'application/json',body:JSON.stringify({success:true,dateRangeApplied:true,data})});
  });
  await overview.goto('http://overview.test/');seen.length=0;
  await overview.setContent('<section id="executive-dashboard" data-perspective="CCO" data-view="overview" data-scope-label="Fixture" data-api-url="http://overview.test/" data-datasets="GetKpiLifecycle,GetKpiSummary,GetRiskSummary,GetAgingSummary,GetCycleSummary,GetSettlementTrend,GetCriticalItems"><nav data-view-navigation><a href="http://overview.test/?view=effectiveness">Effectiveness</a></nav><form id="exec-filters"><input id="exec-start-date" name="startDate" value="2024-01-01"><input id="exec-end-date" name="endDate" value="2024-02-29"><button id="exec-refresh"></button></form><p id="exec-status"></p><div id="exec-results" hidden><div id="cco-summary"></div><div id="exec-kpis" data-lifecycle="true" data-kpi-keys="openingOpenParas,paraAdded,parasSettled,closingOpenParas,highRiskOpenParas,over1YearParas,over3ComplianceCycles"></div><div id="exec-risk"></div><div id="exec-aging"></div><div id="exec-cycle"></div><div id="exec-trend"></div><select id="exec-critical-risk"></select><p id="exec-critical-note"></p><table><tbody id="exec-critical"></tbody></table></div></section>');
  for(const file of ['cco-compliance.js','executive-dashboard.js']) await overview.addScriptTag({content:fs.readFileSync(path.resolve(__dirname,'../../AIS/wwwroot/js',file),'utf8')});
  await overview.waitForFunction(()=>document.getElementById('executive-dashboard').getAttribute('aria-busy')==='false');
  assert.equal(seen.length,7);assert.equal(await overview.locator('#exec-results').evaluate(e=>e.hidden),false);
  assert.match(await overview.locator('#exec-kpis').innerText(),/Opening Open Paras/);
  assert.match(await overview.locator('#exec-status').innerText(),/historical values may be estimated/);
  assert.equal(await overview.locator('#cco-summary strong').count(),4);assert.deepEqual(overviewErrors,[]);
  await overview.close();
  const page=await browser.newPage();
  await page.setContent('<section id="executive-dashboard" data-perspective="CCO"><form id="exec-filters"></form><div id="cco-summary"></div></section>');
  await page.addScriptTag({content:fs.readFileSync(path.resolve(__dirname,'../../AIS/wwwroot/js/cco-compliance.js'),'utf8')});
  await page.evaluate(()=>document.getElementById('executive-dashboard').dispatchEvent(new CustomEvent('ias:executive-data',{detail:{datasets:{GetKpiSummary:{zeroCycleParas:0,over3CycleParas:3,settlementMtdRate:null,criticalItems:2}}}})));
  assert.equal(await page.locator('#cco-summary strong').allTextContents().then(x=>x.join(',')),'0,3,N/A,2');
  await page.locator('#exec-filters').dispatchEvent('change');assert.equal(await page.locator('#cco-summary strong').count(),0);
  console.log('PASS: Chrome fixtures B/C/D single assigned endpoint, canonical reports view, dates/navigation, export absence, invalidation and CCO summary zero/N/A. Not a live IAS smoke test.');
 } finally {await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});

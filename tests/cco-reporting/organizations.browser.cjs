// Review client only: no live activation, Oracle or permission changes.
const {chromium}=require('playwright');
const assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const script=fs.readFileSync(path.join(__dirname,'../../AIS/wwwroot/js/cco-organizations.js'),'utf8');
const markup=enabled=>`<input id="exec-start-date" value="2024-01-01"><input id="exec-end-date" value="2024-02-29"><section id="cco-organizations" data-enabled="${enabled}" data-action="GetOrgPosition" data-api-url="http://org.test/api/"><form id="cco-org-filters"><fieldset disabled><select id="cco-group-by"><option>GROUP</option></select>${['group','division','gm','entity'].map(id=>`<select id="cco-${id}"></select>`).join('')}<button>Apply</button></fieldset></form><p id="cco-org-breadcrumb"></p><p id="cco-org-status">Pending staging</p><div id="cco-org-results" hidden></div></section>`;
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true});
 try {
  const page=await browser.newPage();let requests=[],deny=false;
  await page.route('http://org.test/**',route=>{
   const url=new URL(route.request().url());requests.push(url);
   const data=url.pathname.endsWith('GetOrgFilters')?[{groupId:1,groupName:'Group One',divisionId:2,divisionName:'Division Two',gmOfficeId:3,gmOfficeName:'GM Three',entityId:4,isVerified:true},{groupId:5,groupName:'Group Five',divisionId:6,divisionName:'Division Six',gmOfficeId:7,entityId:8,isVerified:true}]:[{rowKind:'TOTAL',openingCases:10,paraAdded:2,totalCases:12,outstanding:9,settlementRate:null,historicalQuality:'Historical estimate'}];
   return route.fulfill({status:deny?403:200,contentType:'application/json',body:JSON.stringify({success:!deny,data,message:'Forbidden fixture'})});
  });
  await page.setContent(markup('false'));await page.addScriptTag({content:script});
  assert.equal(requests.length,0);assert.equal(await page.locator('fieldset').evaluate(e=>e.disabled),true);
  await page.setContent(markup('true'));await page.addScriptTag({content:script});
  await page.waitForFunction(()=>document.getElementById('cco-org-status').textContent.startsWith('Selected date'));
  assert.equal(requests.length,2);assert.equal(await page.locator('#cco-org-results').evaluate(e=>e.hidden),false);
  assert.match(await page.locator('#cco-org-results').innerText(),/N\/A/);
  await page.selectOption('#cco-group','1');await page.selectOption('#cco-division','2');await page.selectOption('#cco-gm','3');
  await page.selectOption('#cco-group','5');assert.equal(await page.locator('#cco-division').inputValue(),'');assert.equal(await page.locator('#cco-gm').inputValue(),'');
  await page.selectOption('#cco-division','6');assert.equal(await page.locator('#cco-gm option[value="3"]').count(),0);
  deny=true;await page.locator('button').click();await page.waitForFunction(()=>document.getElementById('cco-org-status').textContent.includes('Forbidden'));
  assert.equal(await page.locator('#cco-org-results').evaluate(e=>e.hidden),true);
  assert.equal(await page.locator('#cco-org-results table').count(),0);
  for(const q of requests){assert.equal(q.searchParams.get('startDate'),'2024-01-01');assert.equal(q.searchParams.has('groupRootId'),false);assert.equal(q.searchParams.has('snapDivision'),false);}
  console.log('PASS: dormant UI makes zero requests; enabled review fixture cascades, dates, unknown values and denial clearing. Not a live activation.');
 } finally {await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});

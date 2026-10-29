// Controlled fixtures only: does not verify Oracle or endpoint authorization.
const {chromium}=require('playwright');
const assert=require('node:assert/strict'),fs=require('node:fs');
(async()=>{const browser=await chromium.launch({channel:'chrome',headless:true});try{
 const page=await browser.newPage();let requests=0;page.on('request',()=>requests++);
 await page.setContent('<section id="cco-organizations" data-enabled="false"><input id="cco-org-search"><button id="cco-expand-all"></button><button id="cco-collapse-all"></button><div id="cco-org-results"></div></section><input id="exec-start-date" value="2024-01-01"><input id="exec-end-date" value="2024-12-31">');
 await page.addScriptTag({content:fs.readFileSync('AIS/wwwroot/js/cco-organizations.js','utf8')});
 const rows=[{entityId:1,entityName:'Group',entityCategory:'Group',openingCases:2,paraAdded:3,totalCases:5},{entityId:2,parentEntityId:1,parentEntityName:'Group',entityName:'<img src=x onerror=alert(1)>',entityCategory:'Division',hasTypeConflict:true}];
 const deliver=()=>page.evaluate(rows=>document.getElementById('cco-organizations').dispatchEvent(new CustomEvent('cco:authorized-rows',{detail:rows})),rows);
 await deliver();assert.equal(await page.locator('tbody tr').count(),1);assert.match(await page.locator('tbody').innerText(),/No verified/);
 await page.evaluate(()=>document.getElementById('cco-organizations').dataset.enabled='true');await deliver();
 assert.equal(await page.locator('tbody tr').count(),1);await page.click('#cco-expand-all');assert.equal(await page.locator('tbody tr').count(),2);assert.equal(await page.locator('img').count(),0);assert.match(await page.locator('tbody').innerText(),/Conflict/);
 await page.click('#cco-collapse-all');assert.equal(await page.locator('tbody tr').count(),1);
 await page.fill('#cco-org-search','<img');assert.equal(await page.locator('tbody tr').count(),1);assert.match(await page.locator('tbody').innerText(),/Division/);
 assert.equal(await page.inputValue('#exec-start-date'),'2024-01-01');await page.locator('#exec-end-date').dispatchEvent('change');assert.match(await page.locator('tbody').innerText(),/No verified/);assert.equal(requests,0);
 console.log('Organization fixture checks passed: gate, expansion, search, XSS, conflicts, date invalidation, no requests.');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});

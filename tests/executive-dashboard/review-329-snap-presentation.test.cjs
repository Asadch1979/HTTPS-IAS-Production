'use strict';
// Focused regression guard for PR #329 and subsequent dashboard presentation.
// Run: node --test tests/executive-dashboard/review-329-snap-presentation.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname, '../..');
const read = relative => fs.readFileSync(path.join(root,'AIS',relative),'utf8');

test('CIA displays eight but validates all ten source categories',()=>{
 const razor=read('Views/ExecutiveOverview/_CiaCommandCenter.cshtml');
 const js=read('wwwroot/js/cia-command-center.js');
 assert.equal((razor.match(/new \{ Code = "/g)||[]).length,8);
 assert.doesNotMatch(razor,/Code = "ACCOUNTS"/);
 assert.doesNotMatch(razor,/Code = "IS_AUDIT"/);
 assert.match(js,/rows.length !== 10/);
 assert.match(js,/hiddenCodes = new Set\(\['ACCOUNTS','IS_AUDIT'\]\)/);
 assert.match(js,/mapped \+ unmapped !== identified/);
 assert.match(js,/data-cia-hidden/);
});
test('Snap risk bar width is visible and detail table does not overlap charts',()=>{
 const css=read('wwwroot/css/audit-para-snap.css');
 const js=read('wwwroot/js/audit-para-snap.js');
 assert.match(css,/\.exec-snap \.exec-track > \.exec-fill \{display:block/);
 assert.match(css,/\.snap-chart-details \.snap-bucket-table \{table-layout:auto;min-width:0/);
 assert.match(js,/const details = element\('details',null,'snap-chart-details'\)/);
 assert.match(js,/const caption = element\('summary','View chart data'\)/);
 assert.match(js,/target.replaceChildren\(bars,details\)/);
 assert.match(css,/\.exec-snap\[data-view="risk"\] \.exec-charts/);
});
test('RLM, Group, CIA, CCO and TOP retain own Razor parent and CSS surfaces',()=>{
 for(const file of ['RLM','GROUP','CIA','CCO','TOP']){
   const page=read('Views/ExecutiveOverview/'+file+'.cshtml');
   assert.match(page,/executive-dashboard\.css/);
   assert.match(page,/executive-themes\.css/);
   assert.match(page,/executive-dashboard-parents\.css/);
 }
 assert.match(read('Views/ExecutiveOverview/RLM.cshtml'),/org exec exec-regional/);
 assert.match(read('Views/ExecutiveOverview/AuditParaSnap.cshtml'),/audit-para-snap\.css/);
 assert.match(read('Views/Shared/EngagementWorkbench.cshtml'),/Head Audit Department|HEAD AUDIT DEPARTMENT/);
 assert.match(read('Views/HeadAuditDepartment/Review.cshtml'),/READ-ONLY/);
});
test('Head Audit is supervisory, with its own secured GETs and no Team Lead PPNO substitution',()=>{
 const code=read('Controllers/HeadAuditDepartmentController.cs');
 const db=read('DBConnection.HeadAuditMonitoring.cs');
 const policy=read('Models/HeadAuditMonitoring.cs');
 const sql=read('Docs/sql/PKG_HEAD_AUDIT_MONITOR.sql');
 assert.match(code,/HasViewPermission/);
 assert.match(code,/HasApiPermissionForPath/);
 assert.match(code,/ResponseCache\(Duration = 0/);
 assert.match(db,/P_DEPT_ID/);
 assert.match(sql,/V_HEAD_AUDIT_ENG_SCOPE/);
 assert.match(policy,/PageId = 1000401/);
 assert.doesNotMatch(code,/\[HttpPost\]/);
});
test('all changed browser scripts parse (including CIA and Snap)',()=>{
 for(const file of ['cia-command-center','audit-para-snap']){
   assert.doesNotThrow(()=>new vm.Script(read('wwwroot/js/'+file+'.js')));
 }
});

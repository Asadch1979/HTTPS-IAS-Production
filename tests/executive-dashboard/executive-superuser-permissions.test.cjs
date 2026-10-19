'use strict';
// Static permission-contract checks. These do not run Oracle SQL.
// Run: node --test tests/executive-dashboard/executive-superuser-permissions.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = name => fs.readFileSync(path.join(root, name), 'utf8');
const pages = read('AIS/Docs/sql/executive_dashboard_permissions.sql');
const reports = read('AIS/Docs/sql/executive_reports_permissions.sql');
const executiveController = read('AIS/Controllers/ExecutiveDashboardController.cs');
const snapController = read('AIS/Controllers/AuditParaSnapController.cs');
const reportController = read('AIS/Controllers/ExecutiveReportsController.cs');

test('five executive parent pages explicitly grant only verified Super User', () => {
  for (const [id, name] of [
    [1000300,'TOP'],[1000301,'GROUP'],[1000302,'CCO'],
    [1000303,'CIA'],[1000304,'AuditParaSnap']
  ]) assert.ok(pages.includes("REGISTER_PAGE("+id+", '"+name+"'"), id);
  assert.match(pages, /DELETE FROM T_MENU_PAGES_GROUPMAP/);
  assert.match(pages, /GROUP_ID <> C_SUPER_USER/);
  assert.doesNotMatch(pages, /DELETE FROM T_MENU_PAGES_GROUPMAP[\s\S]{0,220}entity_wise_obs/);
  assert.match(pages, /RLM route ALIASES existing/);
});
test('all 20 executive/Snap actions registered once to the right controllers', () => {
  const records = [...pages.matchAll(/REGISTER_GET\('([^']+)', '([^']+)', (\d+)/g)];
  assert.equal(records.length,20);
  assert.equal(new Set(records.map(v=>v[1]+'/'+v[2])).size,20);
  for(const r of records){
    const controller = r[1]==='ExecutiveDashboard' ? executiveController : snapController;
    assert.match(controller,new RegExp('\\b'+r[2]+'\\('),r[2]);
    if(r[1]==='AuditParaSnap') assert.equal(Number(r[3]),1000304);
    if(['GetCiaCommandKpis','GetCiaIssueNature'].includes(r[2]))
      assert.equal(Number(r[3]),1000303);
  }
});
test('new master APIs disabled, existing master activations preserved', ()=>{
  for(const text of [pages,reports]){
    assert.match(text, /C_NEW_API_ACTIVE CONSTANT CHAR\(1\) := 'N'/);
    assert.doesNotMatch(text, /WHEN MATCHED THEN UPDATE SET\s+(?:m|a)\.IS_ACTIVE/i);
    assert.match(text, /SET IS_ACTIVE\s*=\s*'N'/);
    assert.match(text, /ROLE_ID\s*<>\s*C_SUPER_USER/);
    assert.match(text, /WHEN MATCHED THEN UPDATE SET p\.IS_ACTIVE\s*=\s*'Y'/);
  }
});
test('Report A-E grant only Super User and remain opt-in for new activation',()=>{
  const targets=[...reports.matchAll(/'GetReport[A-E]'/g)].map(x=>x[0]);
  assert.equal(targets.length,5);
  for(const action of ['GetReportA','GetReportB','GetReportC','GetReportD','GetReportE'])
    assert.match(reportController,new RegExp('\\b'+action+'\\('));
  assert.match(reports,/V_ACTIVATE_VERIFIED_REPORTS CHAR\(1\) := 'N'/);
  assert.match(reports,/IF V_ACTIVATE_VERIFIED_REPORTS='Y' THEN/);
  assert.match(reports,/WHERE API_ID=V_API_ID AND NVL\(IS_ACTIVE,'N'\)<>'Y'/);
  assert.doesNotMatch(reports,/V_GROUP_ROLE|V_TOP_ROLE|SELECT 38 FROM dual|SELECT 3 FROM dual/);
});
test('shared Organization Structure RLM page is not de-granted by script',()=>{
  assert.match(pages,/DO NOT REMOVE non-Super-User grants on that shared organization page/);
  assert.doesNotMatch(pages,/DELETE FROM T_MENU_PAGES_GROUPMAP\s+WHERE PAGE_ID = V_RLM_PAGE_ID/);
});

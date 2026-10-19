'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.join(__dirname, '..', '..');
const read = p => fs.readFileSync(path.join(root, p), 'utf8');
const permissions = read('AIS/Docs/sql/executive_dashboard_permissions.sql');
const routes = read('AIS/Controllers/ExecutiveOverviewController.cs');
const executive = read('AIS/Controllers/ExecutiveDashboardController.cs');
const snap = read('AIS/Controllers/AuditParaSnapController.cs');
test('distinct Razor parents match the page registrations', () => {
  for (const [id,route] of [['1000300','TOP'],['1000301','GROUP'],
    ['1000302','CCO'],['1000303','CIA'],['1000304','AuditParaSnap'],['1000305','RLM']]) {
    assert.match(permissions, new RegExp("REGISTER_PAGE\\("+id+",'"+route+"'"));
    assert.ok(fs.existsSync(path.join(root,'AIS/Views/ExecutiveOverview',route+'.cshtml')));
  }
  assert.match(routes, /return View\(auditParaSnap \? "AuditParaSnap" : scope\.DashboardType, new ExecutiveDashboardPageModel/);
  assert.match(permissions, /REGISTER_PAGE\(1000305,'RLM'/);
  assert.doesNotMatch(permissions, /GRANT_PAGE\(V_RLM_PAGE_ID/);
});
test('all 20 declared GET endpoints are covered exactly once', () => {
  const registrations = [...permissions.matchAll(/REGISTER_GET\('([^']+)','([^']+)',\d+,'[^']+'\);/g)];
  assert.equal(registrations.length, 20);
  assert.equal(new Set(registrations.map(r => r[1]+'/'+r[2])).size,20);
  for (const [ctrl,actions] of [
    ['ExecutiveDashboard',['GetKpiSummary','GetRiskSummary','GetAgingSummary','GetCycleSummary','GetSettlementTrend','GetCriticalItems','GetKpiLifecycle','GetCiaCommandKpis','GetCiaIssueNature','GetRoleEntityHeatmap','GetRoleUnitComparison','GetRoleFollowups']],
    ['AuditParaSnap',['GetKpis','GetDistribution','GetCriticalItems','GetReportA','GetReportB','GetReportC','GetReportD','GetReportE']]
  ]) {
    const controller = ctrl==='ExecutiveDashboard'?executive:snap;
    for (const a of actions) {
      assert.equal(registrations.filter(r=>r[1]===ctrl && r[2]===a).length,1,a);
      assert.match(controller,new RegExp('(?:IActionResult|Task<IActionResult>)\\s+'+a+'\\('),a);
    }
  }
});
test('re-execution preserves previously disabled endpoints and grants',()=>{
  assert.match(permissions,/V_NEW_API_ACTIVE CHAR\(1\) := 'N'/);
  assert.match(permissions,/V_NEW_GRANT_ACTIVE CHAR\(1\) := 'N'/);
  assert.doesNotMatch(permissions,/WHEN MATCHED THEN UPDATE SET/i);
  assert.match(permissions,/IF V_FOUND=0 THEN\s+INSERT INTO T_MENU_PAGES/);
  assert.match(permissions,/V_CONFLICTS>0 THEN\s+RAISE_APPLICATION_ERROR\(-20085/);
  assert.match(permissions,/LOCK TABLE T_AU_ROLE_API_PERMISSION IN EXCLUSIVE MODE/);
});
test('CIA/Snap sensitive registrations retain distinct page and fixed roles',()=>{
  assert.match(permissions,/REGISTER_GET\('ExecutiveDashboard','GetCiaCommandKpis',1000303,'CIA'\)/);
  assert.match(permissions,/REGISTER_GET\('ExecutiveDashboard','GetCiaIssueNature',1000303,'CIA'\)/);
  assert.match(permissions,/REGISTER_GET\('AuditParaSnap','GetKpis',1000304,'SNAP'\)/);
  assert.match(permissions,/P_CLASS='CIA' OR P_CLASS='SNAP'/);
  assert.match(permissions,/ROLE_ID NOT IN \(1,3\)/);
});
test('no legacy SQL package or report permission changes required by new parents',()=>{
  const pkg = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
  assert.match(pkg,/completed_entities AS/); // PR321 distinct-entity coverage
  assert.match(pkg,/AS issue_dropped/);
  assert.match(pkg,/CREATE OR REPLACE VIEW V_EXEC_PARA_BASE AS/);
  assert.match(read('AIS/Docs/sql/V_EXEC_KPI_LIFECYCLE.sql'),/issue_rectified_on/);
  assert.match(read('AIS/Docs/sql/V_EXEC_REPORT_SOURCE_image_rewrite.sql'),/report_movement_quality/);
});

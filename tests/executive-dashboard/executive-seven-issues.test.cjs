'use strict';
// Source contract QA for seven production screenshot findings.
// Run: node --test tests/executive-dashboard/executive-seven-issues.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname,'../..');
const read = rel => fs.readFileSync(path.join(root,rel),'utf8');
const views = read('AIS/Models/ExecutiveDashboardViews.cs');
const sql = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const main = read('AIS/wwwroot/js/executive-dashboard.js');
const rlm = read('AIS/Views/ExecutiveOverview/RLM.cshtml');
const group = read('AIS/Views/ExecutiveOverview/GROUP.cshtml');
const snap = read('AIS/Views/ExecutiveOverview/AuditParaSnap.cshtml');
const snapJs = read('AIS/wwwroot/js/audit-para-snap.js');
const regional = read('AIS/Views/ExecutiveOverview/_RegionalPulse.cshtml');
const mgmt = read('AIS/Views/ExecutiveOverview/_ManagementAudit.cshtml');
const execApi = read('AIS/Controllers/ExecutiveDashboardController.cs');
const snapApi = read('AIS/Controllers/AuditParaSnapController.cs');
const roleJs = read('AIS/wwwroot/js/executive-role-reporting.js');
const snapCss = read('AIS/wwwroot/css/audit-para-snap.css');

test('TOP Audit Para Position requests dated lifecycle values, not current totals',()=>{
  assert.match(views,/position\.UseLifecycleKpis = true/);
  assert.match(views,/position\.Sections = Array\.Empty<string>\(\)/);
  assert.match(views,/\bposition\.Kpis = new\[\] \{ "openingOpenParas"/);
  assert.match(views,/GetKpiLifecycle/);
  assert.match(main,/\.filter\(\(\[, key\]\) => \$\('kpis'\)\.dataset\.kpiKeys/);
  assert.match(main,/Audit Para Position for/);
});
test('Critical = OPEN High Risk, across current KPI list and Snap',()=>{
  assert.match(sql,/CASE WHEN c\.para_status = 8 AND c\.risk = 1\s+THEN 1 ELSE 0 END AS is_critical/);
  assert.match(sql,/NVL\(SUM\(p\.is_critical\), 0\) AS critical_items/);
  assert.match(sql,/AND p\.is_critical = 1/);
  const critical=sql.slice(sql.lastIndexOf('PROCEDURE P_GET_SNAP_CRITICAL('),
       sql.indexOf('END P_GET_SNAP_CRITICAL;',sql.lastIndexOf('PROCEDURE P_GET_SNAP_CRITICAL(')));
  assert.match(critical,/AND k\.risk_id = 1/);
  assert.doesNotMatch(critical,/AND \(k\.risk_id = 1 OR/);
  assert.match(snapJs,/Critical = open High-Risk paras \(ZTBL\)/);
});
test('Reports only CIA, Compliance and Group Management',()=>{
  assert.match(views,/"GROUP" => new\[\] \{ "E" \}/);
  assert.match(views,/"CCO" => new\[\] \{ "B", "C", "D" \}/);
  assert.match(views,/"CIA" => new\[\] \{ "A", "B", "C", "D", "E" \}/);
  assert.match(views,/views\.RemoveAll\(v => v\.Id == "reports"\)/);
});
test('RLM all tabs retain dark theme regardless of selected view',()=>{
  assert.match(rlm,/class="org exec exec-regional/);
  assert.match(rlm,/<link rel="stylesheet" href="~\/css\/regional-audit-pulse\.css"/);
  assert.doesNotMatch(rlm,/@if \(Model\.SelectedView\.Id == "regional"\) \{ <link rel="stylesheet" href="~\/css\/regional-audit-pulse\.css"/);
});
test('Group and Regional data loading uses IAS page/API grants, not environment feature switches',()=>{
  assert.match(mgmt,/<partial name="_RoleReporting"/);
  assert.match(regional,/<partial name="_RoleReporting"/);
  assert.match(group,/executive-role-reporting\.js/);
  assert.match(rlm,/executive-role-reporting\.js/);
  assert.doesNotMatch(roleJs,/app\.dataset\.roleEnabled !== 'true'/);
  assert.doesNotMatch(execApi,/Role reporting awaits verified Oracle deployment/);
  assert.match(execApi,/HasApiPermissionForPath/);
  assert.match(execApi,/HasViewPermission/);
});
test('Snap does not return environment-only not-deployed errors',()=>{
  assert.doesNotMatch(snap,/data-enabled="@\(Model\.AuditParaSnapEnabled/);
  assert.doesNotMatch(snap,/Oracle activation pending compilation/);
  assert.doesNotMatch(snapJs,/app\.dataset\.enabled !== 'true'/);
  assert.doesNotMatch(snapApi,/RoleDashboards:AuditParaSnapEnabled/);
  assert.match(snapApi,/pageId != AuditParaSnapPolicy.PageId/);
  assert.match(snapApi,/HasApiPermissionForPath/);
  assert.match(snap,/Model\.SelectedView\.Id=="reports"/);
});
test('Snap header aligned in separate eyebrow, title and scope',()=>{
  assert.match(snap,/<p class="snap-eyebrow">ZTBL · INTERNAL AUDIT REPORTING<\/p>/);
  assert.match(snap,/<h1>Audit Para Snap<\/h1>/);
  assert.match(snap,/<p class="snap-scope">/);
  assert.match(snapCss,/\.snap-branding h1/);
});
test('Unusual age is traceable to source, not guessed or fabricated',()=>{
  assert.match(main,/IAS Para Added On:/);
  assert.match(sql,/c\.para_added_on/);
  assert.match(read('AIS/Docs/sql/verify_executive_seven_issues_F8_20261019.sql'),/61581,61582/);
});

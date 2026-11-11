// Pin current merged Phase 1 baseline; intentional changes are narrowly reviewed.
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),cp=require('node:child_process');
const read=p=>fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n');
const baseline=p=>cp.execFileSync('git',['show','d3f2d144490d3744e766358c7b8beb1f3ecfc144:'+p],{encoding:'utf8'}).replace(/\r\n/g,'\n');
test('Phase 2 preserves existing APIs, permissions, report client, other parents and Oracle calculations',()=>{
 for(const p of ['AIS/Controllers/ExecutiveDashboardController.cs','AIS/Controllers/ExecutiveReportsController.cs','AIS/Controllers/AuditParaSnapController.cs','AIS/Models/ExecutiveDashboardFilters.cs','AIS/Models/ComplianceDashboardPolicy.cs',...['TOP','CIA','GROUP','RLM','AuditParaSnap'].map(v=>'AIS/Views/ExecutiveOverview/'+v+'.cshtml'),...['PKG_EXEC_DASHBOARD.sql','V_EXEC_KPI_LIFECYCLE.sql','executive_dashboard_permissions.sql'].map(v=>'AIS/Docs/sql/'+v)])assert.equal(read(p),baseline(p),p);
});
test('Audit Performance code is removed and remaining organizational endpoints stay fail-closed',()=>{
 const adapter=read('AIS/DBConnection.CcoOrganizationReporting.cs');
 const controller=read('AIS/Controllers/CcoComplianceController.cs');
 const definition=read('AIS/Models/CcoDashboardPresentation.cs');
 const page=read('AIS/Views/ExecutiveOverview/CCO.cshtml');
 assert.ok(!definition.includes('View("performance"'));
 assert.match(definition,/"performance" => "overview"/);
 assert.doesNotMatch(page,/cco-organizations\.js|Audit Performance Analysis/);
 assert.doesNotMatch(controller,/GetOrgPerformance|CcoReporting:PerformanceEnabled/);
 assert.doesNotMatch(adapter,/GetCcoPerformance|PKG_EXEC_CCO_PERFORMANCE/);
 assert.equal((adapter.match(/throw new CcoOrganizationUnavailableException/g)||[]).length,2);
 for(const path of [
   'AIS/Docs/sql/PKG_EXEC_CCO_PERFORMANCE.sql',
   'AIS/Views/ExecutiveOverview/_CcoOrganizations.cshtml',
   'AIS/wwwroot/js/cco-organizations.js'
 ]) assert.equal(fs.existsSync(path),false,path);
 const permissions=JSON.parse(read('AIS/Docs/sql/cco-review/permissions.review.json'));
 assert.equal(permissions.actions.includes('GetOrgPerformance'),false);
 assert.match(read('AIS/Views/ExecutiveOverview/_CcoCompliance.cshtml'),/_CcoFunctionPosition/);
 assert.match(read('AIS/Views/ExecutiveOverview/_CcoCompliance.cshtml'),/_Sections/);
});

test('CCO presentation changes retain canonical report APIs and safe text rendering',()=>{
 const client=read('AIS/wwwroot/js/executive-reports.js');
 assert.match(client,/root.dataset.perspective === 'CCO' \? selected.dataset.title/);
 assert.match(client,/query.set\('view', 'reports'\)/);
 assert.match(client,/GetReportC\?\$\{query\}/);
 assert.match(read('AIS/Views/ExecutiveOverview/_Reports.cshtml'),/Model.IsSuperUser/);
 assert.doesNotMatch(read('AIS/Views/ExecutiveOverview/CCO.cshtml'),/cco-organizations\.js/);
});
// Report E intentionally uses the approved Group/Division view; avoid pinning
// its package body to a pre-change snapshot while other protected SQL remains pinned.
test('Report E changes are limited to view-backed Group and Division resolution',()=>{
 const reports=read('AIS/Docs/sql/PKG_EXEC_REPORTS.sql');
 assert.match(reports,/FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r/);
 assert.match(reports,/group_report_map AS \(/);
 assert.match(reports,/LEFT JOIN group_report_map gr/);
 assert.match(reports,/P_REPORT_ID = 'E' AND s.audit_type = 'D'/);
});

// Pin current merged Phase 1 baseline; intentional changes are narrowly reviewed.
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),cp=require('node:child_process');
const read=p=>fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n');
const baseline=p=>cp.execFileSync('git',['show','d3f2d144490d3744e766358c7b8beb1f3ecfc144:'+p],{encoding:'utf8'}).replace(/\r\n/g,'\n');
test('Phase 2 preserves existing APIs, permissions, report client, other parents and Oracle calculations',()=>{
 for(const p of ['AIS/Controllers/ExecutiveDashboardController.cs','AIS/Controllers/ExecutiveReportsController.cs','AIS/Controllers/AuditParaSnapController.cs','AIS/Models/ExecutiveDashboardFilters.cs','AIS/Models/ComplianceDashboardPolicy.cs',...['TOP','CIA','GROUP','RLM','AuditParaSnap'].map(v=>'AIS/Views/ExecutiveOverview/'+v+'.cshtml'),...['PKG_EXEC_DASHBOARD.sql','PKG_EXEC_REPORTS.sql','V_EXEC_KPI_LIFECYCLE.sql','executive_dashboard_permissions.sql'].map(v=>'AIS/Docs/sql/'+v)])assert.equal(read(p),baseline(p),p);
});
test('new organizational adapters cannot be activated by a configuration toggle and permission requests remain inactive',()=>{
 const partial=read('AIS/Views/ExecutiveOverview/_CcoCompliance.cshtml');
 assert.ok(partial.indexOf('name="_Sections"')<partial.indexOf('id="cco-summary"'),'Main KPI cards precede supplementary cycle cards');
 const adapter=read('AIS/DBConnection.CcoOrganizationReporting.cs');
 assert.equal((adapter.match(/throw new CcoOrganizationUnavailableException/g)||[]).length,2);
 assert.doesNotMatch(adapter,/DatabaseConnection\(|CreateCommand\(|GetValue<bool>|ConnectionString/);
 const permissions=JSON.parse(read('AIS/Docs/sql/cco-review/permissions.review.json'));
 assert.equal(permissions.apiIsActive,'N');assert.equal(permissions.roleGrantIsActive,'N');assert.deepEqual(permissions.roles,[38,1]);assert.deepEqual(permissions.exports,[]);
 const sql=read('AIS/Docs/sql/cco-review/PKG_CCO_REPORTING.sql.review');
 assert.match(sql,/RAISE_APPLICATION_ERROR\(-20803,'CCO organizational reporting is not verified'\)/);
 assert.match(sql,/GROUPING SETS/);assert.match(sql,/V_CCO_AUTHORIZED_ENTITIES/);
 assert.match(read('AIS/Views/ExecutiveOverview/_CcoOrganizations.cshtml'),/data-enabled="false"/);
});

test('CCO presentation changes retain canonical report APIs and safe text rendering',()=>{
 const client=read('AIS/wwwroot/js/executive-reports.js');
 assert.match(client,/root.dataset.perspective === 'CCO' \? selected.dataset.title/);
 assert.match(client,/query.set\('view', 'reports'\)/);
 assert.match(client,/GetReportC\?\$\{query\}/);
 assert.match(read('AIS/Views/ExecutiveOverview/_Reports.cshtml'),/Model.IsSuperUser/);
 const tree=read('AIS/wwwroot/js/cco-organizations.js');
 assert.doesNotMatch(tree,/innerHTML|fetch\(/);
 assert.match(tree,/root.dataset.enabled!=='true'/);
});
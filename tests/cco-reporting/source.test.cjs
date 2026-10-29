const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),{execFileSync}=require('node:child_process');
const read=p=>fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n');
// Public PR base: https://github.com/Asadch1979/HTTPS-IAS-Production/commit/1239d510f913cef79c98ef91d2e7eaad090c480c
// Shallow checkouts: git fetch --depth=1 origin 1239d510f913cef79c98ef91d2e7eaad090c480c
const before=p=>execFileSync('git',['show','1239d510f913cef79c98ef91d2e7eaad090c480c:'+p],{encoding:'utf8'}).replace(/\r\n/g,'\n');
test('protected executive readers, APIs, permissions, other parents and Oracle sources unchanged',()=>{
 for(const p of ['AIS/Controllers/ExecutiveReportsController.cs','AIS/Controllers/ExecutiveDashboardController.cs','AIS/Controllers/AuditParaSnapController.cs','AIS/DBConnection.ExecutiveDashboard.cs','AIS/Models/ExecutiveDashboardFilters.cs','AIS/Models/ComplianceDashboardPolicy.cs',...['TOP','CIA','AuditParaSnap','GROUP','RLM'].map(r=>'AIS/Views/ExecutiveOverview/'+r+'.cshtml'),...['PKG_EXEC_DASHBOARD.sql','PKG_EXEC_REPORTS.sql','V_EXEC_KPI_LIFECYCLE.sql','executive_dashboard_permissions.sql'].map(f=>'AIS/Docs/sql/'+f)]) assert.equal(read(p),before(p),p);
 const p='AIS/Models/ExecutiveDashboardViews.cs';
 assert.equal(read(p).replace('            if (perspective == "CCO") return CcoDashboardPresentation.Definitions(views.Single(v => v.Id == "reports"));\n',''),before(p),'Other perspective definitions unchanged');
});
test('CCO projection reuses existing lifecycle values with no extra Oracle reads',()=>{
 const p='AIS/DBConnection.ComplianceDashboard.cs',source=read(p);
 assert.equal(source.replace('                OpeningCases = scope.DashboardType == "CCO" ? opening : (long?)null,\n                ParaAdded = scope.DashboardType == "CCO" ? added : (long?)null,\n',''),before(p));
 assert.match(source,/var total = opening \+ added/);
 assert.equal((source.match(/ReadExecutiveKpiLifecycle\(scope, filters\)/g)||[]).length,1);
});

test('CCO eight visible definitions hide canonical reports; pending tabs issue no queries',()=>{
 const page=read('AIS/Views/ExecutiveOverview/CCO.cshtml'),controller=read('AIS/Controllers/ExecutiveOverviewController.cs');
 assert.match(page,/else if \(!pending\)/);assert.match(page,/ReportForView\(Model.SelectedView.Id\)/);
 assert.match(controller,/views.RemoveAll\(v => v.Id == "reports"\)/);
 assert.ok(controller.indexOf('CcoDashboardPresentation.ResolveLegacy')>controller.indexOf('LogDenial("PagePermission"'));
 assert.match(page,/startDate = Model.Filters.StartDate, endDate = Model.Filters.EndDate/);
});

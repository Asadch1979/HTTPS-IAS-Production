const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),{execFileSync}=require('node:child_process');
const read=p=>fs.readFileSync(p,'utf8').replace(/\r\n/g,'\n');
const before=p=>execFileSync('git',['show','930394fe:'+p],{encoding:'utf8'}).replace(/\r\n/g,'\n');
test('protected executive readers, APIs, permissions, other parents and Oracle sources unchanged',()=>{
 for(const p of ['AIS/Controllers/ExecutiveReportsController.cs','AIS/Controllers/ExecutiveDashboardController.cs','AIS/Controllers/AuditParaSnapController.cs','AIS/DBConnection.ExecutiveDashboard.cs','AIS/DBConnection.ComplianceDashboard.cs','AIS/Models/ExecutiveDashboardFilters.cs','AIS/Models/ComplianceDashboardPolicy.cs','AIS/wwwroot/js/executive-dashboard.js',...['TOP','CIA','AuditParaSnap','GROUP','RLM'].map(r=>'AIS/Views/ExecutiveOverview/'+r+'.cshtml'),...['PKG_EXEC_DASHBOARD.sql','PKG_EXEC_REPORTS.sql','V_EXEC_KPI_LIFECYCLE.sql','executive_dashboard_permissions.sql'].map(f=>'AIS/Docs/sql/'+f)]) assert.equal(read(p),before(p),p);
 const p='AIS/Models/ExecutiveDashboardViews.cs';
 assert.equal(read(p).replace('            if (perspective == "CCO") return CcoDashboardPresentation.Definitions(views.Single(v => v.Id == "reports"));\n',''),before(p),'Other perspective definitions unchanged');
});
test('CCO seven visible definitions hide canonical reports; pending tabs issue no queries',()=>{
 const page=read('AIS/Views/ExecutiveOverview/CCO.cshtml'),controller=read('AIS/Controllers/ExecutiveOverviewController.cs');
 assert.match(page,/else if \(!pending\)/);assert.match(page,/ReportForView\(Model.SelectedView.Id\)/);
 assert.match(controller,/views.RemoveAll\(v => v.Id == "reports"\)/);
 assert.ok(controller.indexOf('CcoDashboardPresentation.ResolveLegacy')>controller.indexOf('LogDenial("PagePermission"'));
 assert.match(page,/startDate = Model.Filters.StartDate, endDate = Model.Filters.EndDate/);
});

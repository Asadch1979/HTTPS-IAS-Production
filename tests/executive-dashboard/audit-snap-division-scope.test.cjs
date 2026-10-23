'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname,'../..');
const read = p => fs.readFileSync(path.join(root,'AIS',p),'utf8');

test('Audit Para Snap supports All/FAD/MAD and verifies positive audited-by children',()=>{
 const policy=read('Models/AuditParaSnapData.cs');
 const filters=read('Models/ExecutiveDashboardFilters.cs');
 const map=read('Docs/sql/snap_audit_division_scope.sql');
 assert.match(policy,/filters.SnapDivision != "FAD" && filters.SnapDivision != "MAD"/);
 assert.match(policy,/filters.SnapDivision != "1"/);
 assert.match(policy,/filters.SnapDivision == "1" && filters.SnapDepartmentId.HasValue/);
 assert.match(policy,/filters.SnapDepartmentId\.HasValue/);
 assert.match(filters,/\["snapDivision"\] = SnapDivision/);
 assert.match(filters,/\["snapDepartmentId"\] = SnapDepartmentId/);
 assert.match(map,/P_GET_CHILDREN/);
 assert.match(map,/P_AUDITBY_ID IS NULL OR h\.ENTITY_ID = P_AUDITBY_ID/);
 assert.match(map,/V_IAS_ENTITY_REPORTING_NAMES h/);
 assert.match(map,/h\.REPORTING_1 = V_PARENT_ID/);
 assert.match(map,/WHEN 'FAD' THEN RETURN 112243/);
 assert.match(map,/WHEN 'MAD' THEN RETURN 113254/);
 assert.doesNotMatch(map,/\b(?:CREATE TABLE|INSERT INTO|JOIN|FROM) T_IAS_SNAP_AUDIT_SCOPE\b/);
});
test('All division filter includes parent auditby ID and descendants through reporting levels 1-6',()=>{
 const kpi=read('Docs/sql/PKG_EXEC_DASHBOARD.sql');
 const reports=read('Docs/sql/PKG_EXEC_REPORTS.sql');
 for(const text of [kpi,reports]){
   assert.match(text,/SELECT 112243 division_id, 'FAD' division_code FROM dual/);
   assert.match(text,/UNION ALL SELECT 113254, 'MAD' FROM dual/);
   assert.match(text,/P_SNAP_DIVISION_CODE = '1' OR d.division_code = P_SNAP_DIVISION_CODE/);
   for(const level of [1,2,3,4,5,6])
      assert.ok(text.toLowerCase().includes('h.reporting_'+level),'Missing reporting level '+level);
   assert.match(text,/OR EXISTS\s*\(\s*SELECT 1 FROM V_IAS_ENTITY_REPORTING_NAMES h/);
 }
 const body=read('Docs/sql/snap_audit_division_scope.sql');
 assert.match(body,/IF P_AUDITBY_ID IS NOT NULL/);
 assert.match(body,/h\.ENTITY_ID <> V_PARENT_ID/);
 assert.match(body,/REQUIRE_SELECTION\(P_DIVISION_CODE,NULL\)/);
});

test('both selectors persist on Snap tab navigation and reports',()=>{
 const parent=read('Views/ExecutiveOverview/_AuditParaSnapFilters.cshtml');
 const js=read('wwwroot/js/audit-para-snap.js');
 const reports=read('wwwroot/js/executive-reports.js');
 assert.match(parent,/name="snapDivision"/);
 assert.match(parent,/name="snapDepartmentId"/);
 assert.match(parent,/>Field Audit Division</);
 assert.match(parent,/>Management Audit Division</);
 assert.match(parent,/>All</);
 assert.match(parent,/<option value="1"[^>]*>All Divisions</);
 assert.match(js,/department.value = ''/);
 assert.match(js,/form.requestSubmit\(\)/);
 assert.match(reports,/'snapDivision','snapDepartmentId'/);
});
test('web AND API both reject unknown child before returning data',()=>{
 const page=read('Controllers/ExecutiveOverviewController.cs');
 const api=read('Controllers/AuditParaSnapController.cs');
 const db=read('DBConnection.AuditParaSnap.cs');
 assert.match(page,/database\.GetAuditParaSnapDepartments\(scope, filters.SnapDivision, filters.SnapDepartmentId\)/);
 assert.match(api,/database\.GetAuditParaSnapDepartments\(scope, filters.SnapDivision, filters.SnapDepartmentId\)/);
 assert.match(db,/Selected Department \/ Cluster does not belong to the chosen Division/);
 assert.match(api,/HasApiPermissionForPath/);
 assert.match(api,/pageId != AuditParaSnapPolicy.PageId/);
});
test('all IAS Snap KPI, risk, trend, critical and report cursors apply the mapped audit-portfolio scope',()=>{
 const kpi=read('Docs/sql/PKG_EXEC_DASHBOARD.sql');
 const reports=read('Docs/sql/PKG_EXEC_REPORTS.sql');
 const count=(s,x)=>(s.match(x)||[]).length;
 assert.equal(count(kpi,/P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL/g),10);
 assert.equal(count(kpi,/V_IAS_ENTITY_REPORTING_NAMES h/g),6);
 assert.equal(count(reports,/P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL/g),10);
 assert.equal(count(reports,/V_IAS_ENTITY_REPORTING_NAMES h/g),2);
 assert.doesNotMatch(kpi,/T_IAS_SNAP_AUDIT_SCOPE/);
 assert.doesNotMatch(reports,/T_IAS_SNAP_AUDIT_SCOPE/);
 assert.equal(count(reports,/IO_CURSOR, P_SNAP_DIVISION_CODE, P_SNAP_AUDITED_BY_ID\);/g),5);
 assert.match(read('DBConnection.ExecutiveDashboardReports.cs'),/P_SNAP_DIVISION_CODE/);
 assert.match(read('DBConnection.ExecutiveDashboard.cs'),/P_SNAP_DIVISION_CODE/);
 assert.match(read('DBConnection.AuditParaSnap.cs'),/P_SNAP_DIVISION_CODE/);
});
test('all scoped visualization JavaScript parses and Snap exposes table disclosure',()=>{
 for(const name of ['audit-para-snap','cia-command-center','executive-reports'])
   assert.doesNotThrow(()=>new vm.Script(read('wwwroot/js/'+name+'.js')));
 assert.match(read('wwwroot/css/audit-para-snap.css'),/\.exec-snap \.exec-track > \.exec-fill \{display:block/);
 assert.match(read('wwwroot/js/audit-para-snap.js'),/target.replaceChildren\(bars,details\)/);
});

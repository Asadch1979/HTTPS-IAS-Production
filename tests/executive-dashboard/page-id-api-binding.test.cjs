'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = p => fs.readFileSync(p, 'utf8');

const pages = new Map([
 [1000401,'HeadAuditDepartment/AuditTeamsMonitoring'],
 [1000304,'ExecutiveOverview/AuditParaSnap'],
 [1000303,'ExecutiveOverview/CIA'],
 [1000302,'ExecutiveOverview/CCO'],
 [1000301,'ExecutiveOverview/GROUP'],
 [1000300,'ExecutiveOverview/TOP'],
 [1000305,'ExecutiveOverview/RLM']
]);

test('Manifest uses existing two-column table and fails closed for unapproved or inactive-master APIs',()=>{
 const sql=read('AIS/Docs/sql/executive_dashboard_page_api_manifest.sql');
 for(const [id,path] of pages)
   assert.ok(sql.includes(`verify_page(${id},'${path}'`),id+' '+path);
 assert.match(sql,/MENU_ID=p_menu/);
 assert.match(sql,/v_count<>1 OR NVL\(v_active,'N'\)<>'Y'/);
 assert.match(sql,/Existing T_IAS_EXEC_PAGE_API|existing T_IAS_EXEC_PAGE_API/);
 assert.match(sql,/COLUMN_NAME='IS_ACTIVE'/); // only schema guard, never queried as a dependency flag
 assert.match(sql,/Status-enabled dependency schema requires separate reviewed migration/);
 assert.match(sql,/Unexpected page\/API dependency/);
 assert.match(sql,/approved\.EXISTS\(TO_CHAR\(unexpected\.PAGE_ID\)/);
 assert.match(sql,/WHEN NOT MATCHED THEN INSERT \(PAGE_ID,API_ID\)/);
 assert.doesNotMatch(sql,/d\.IS_ACTIVE|dependency\.IS_ACTIVE/);
 assert.match(sql,/Duplicate existing PAGE_ID\/API_ID dependencies/);
 assert.match(sql,/m\.IS_ACTIVE AS API_ACTIVE/);
 assert.doesNotMatch(sql,/MERGE INTO T_AU_ROLE_API_PERMISSION|UPDATE T_AU_ROLE_API_PERMISSION|DELETE FROM T_AU_ROLE_API_PERMISSION/);
 assert.match(sql,/link_get\(1000401,'HeadAuditDepartment','ResponseEvidence'\)/);
 assert.match(sql,/link_get\(1000303,'ExecutiveDashboard','GetCiaCommandKpis'\)/);
 assert.match(sql,/link_get\(1000302,'CcoCompliance','GetFunctionPosition'\)/);
 assert.match(sql,/link_get\(1000300,'ExecutiveDashboard','GetKpiLifecycle'\)/);
 assert.match(sql,/link_get\(1000305,'ExecutiveDashboard','GetRoleEntityHeatmap'\)/);
 assert.match(sql,/reports\(1000301,'E'\)/);
 assert.match(sql,/reports\(1000302,'BCD'\)/);
 assert.match(sql,/reports\(1000303,'ABCDE'\)/);
});

test('PKG_LG loads dashboard APIs from assigned active pages, not active role API grants',()=>{
 const pkg=read('AIS/Docs/sql/PKG_LG.sql');
 const start=pkg.indexOf('PROCEDURE p_GetApiPermissions(',10000);
 const end=pkg.indexOf('END p_GetApiPermissions;',start);
 const sql=pkg.slice(start,end);
 assert.ok(start>0&&end>start);
 assert.equal((sql.match(/JOIN T_IAS_EXEC_PAGE_API dependency/g)||[]).length,2);
 assert.equal((sql.match(/JOIN T_MENU_PAGES_GROUPMAP assigned/g)||[]).length,0);
 assert.equal((sql.match(/FROM T_MENU_PAGES_GROUPMAP assigned/g)||[]).length,2);
 assert.doesNotMatch(sql,/dependency\.IS_ACTIVE/);
 assert.equal((sql.match(/dependency.PAGE_ID=assigned.PAGE_ID/g)||[]).length,2);
 assert.equal((sql.match(/assigned.GROUP_ID=R_ID/g)||[]).length,2);
 assert.equal((sql.match(/AND NOT \(/g)||[]).length,2);
 for(const root of ['api/executivedashboard/%','api/executivereports/%',
                     'api/ccocompliance/%','api/auditparasnap/%','api/headauditdepartment/%'])
    assert.ok(sql.includes(root),root);
 assert.match(sql,/IF R_ID IN \(10, 27, 18, 28\) THEN/);
});

test('PKG_AD executive and Head Audit page assignment no longer changes role API permissions',()=>{
 const pkg=read('AIS/Docs/sql/PKG_AD.sql');
 const add=pkg.indexOf('PROCEDURE P_AddGroupMenuItemsAssignment(',20000);
 const remove=pkg.indexOf('PROCEDURE P_RemoveGroupMenuItemsAssignment(',add);
 const tail=pkg.indexOf('procedure P_GetAllMenuPages(',remove);
 assert.ok(add>0&&remove>add&&tail>remove);
 const body=pkg.slice(add,tail);
 assert.equal((body.match(/ELSIF \(PAGEID BETWEEN 1000300 AND 1000305\) OR PAGEID=1000401 THEN/g)||[]).length,2);
 assert.match(body,/Never auto-grant or reactivate a ROLE_ID\/API_ID row here/);
 assert.match(body,/Revoking the\s+-- page removes API eligibility/);
 assert.match(body,/IF PAGEID IN \(192,193\)/);
});

test('Server-side exact PAGE_ID plus live page assignment is checked independently of cached role/path',()=>{
 const helper=read('AIS/DBConnection.ExecutivePageApiPermissions.cs');
 assert.match(helper,/HasActiveExecutivePageApi\(int pageId, int assignedGroupId, string method, string requestPath\)/);
 assert.match(helper,/JOIN T_MENU_PAGES_GROUPMAP assigned/);
 assert.match(helper,/assigned.GROUP_ID=:group_id/);
 assert.match(helper,/d.PAGE_ID=:page_id/);
 assert.doesNotMatch(helper,/d\.IS_ACTIVE/);
 assert.match(helper,/m.API_ID=d.API_ID AND m.IS_ACTIVE='Y'/);
 assert.match(helper,/UPPER\(TRIM\(m.HTTP_METHOD\)\)='GET'/);
 assert.match(helper,/LOWER\(TRIM\(BOTH '\/' FROM m.API_PATH\)\)=LOWER\(:api_path\)/);
 assert.match(helper,/normalized.StartsWith\("api\/", StringComparison.OrdinalIgnoreCase\)/);
 assert.doesNotMatch(helper,/Role_API_PERMISSION|ROLE_API_PERMISSION/);
 const controllerBindings=[
  ['AIS/Controllers/ExecutiveDashboardController.cs', 'pageId, user.UserRoleID'],
  ['AIS/Controllers/ExecutiveReportsController.cs', 'pageId, user.UserRoleID'],
  ['AIS/Controllers/CcoComplianceController.cs', '1000302,user.UserRoleID'],
  ['AIS/Controllers/AuditParaSnapController.cs', 'pageId,user.UserRoleID'],
  ['AIS/Controllers/HeadAuditDepartmentController.cs', 'HeadAuditMonitoringPolicy.PageId, user.UserRoleID']
 ];
 for(const [file,args] of controllerBindings) {
   const src=read(file);
   assert.ok(src.includes('database.HasActiveExecutivePageApi('+args),file);
   assert.match(src,/HasApiPermissionForPath\(/);
 }
});

test('Deployment document requires DBA preflight, session refresh, scope checks and rollback',()=>{
 const doc=read('AIS/Docs/DASHBOARD_PAGE_ID_API_PRODUCTION.md');
 for(const part of ['1000401','1000304','1000303','1000302','1000301','1000300','1000305',
   'PKG_LG','PKG_AD','log out','No role-to-API activation','Rollback'])
   assert.ok(doc.includes(part),part);
});

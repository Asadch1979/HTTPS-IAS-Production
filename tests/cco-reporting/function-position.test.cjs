const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs');
const sql=fs.readFileSync('AIS/Docs/sql/cco-review/PKG_CCO_FUNCTION_POSITION.sql.review','utf8');
const report=fs.readFileSync('AIS/Docs/sql/PKG_EXEC_REPORTS.sql','utf8');
const f8=fs.readFileSync('AIS/Docs/sql/f8/PKG_EXEC_REPORTS/02.sql','utf8');
test('function SQL scopes facts and uses installed unique active Group/Division mappings',()=>{
 assert.match(sql,/s.audit_type='D'/);
 assert.match(sql,/FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r/);
 assert.match(sql,/m.entity_id=r.entity_id AND m.parent_id=r.parent_id/);
 assert.match(sql,/NVL\(m.status,'Y'\)='Y'/);
 assert.match(sql,/FROM mapping_candidates GROUP BY division_id/);
 assert.match(sql,/FROM V_EXEC_ENTITY_SCOPE GROUP BY entity_id/);
 assert.match(sql,/h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id/);
 assert.match(sql,/HAVING COUNT\(\*\)<>1 OR s.com_id IS NULL/);
 assert.match(sql,/GROUPING SETS/);
 assert.match(sql,/s.close_on IS NULL OR s.close_on>=P_END_DATE\+1/);
 assert.match(sql,/NULLIF\(SUM\(opening_balance\+paras_added\),0\)/);
 assert.match(sql,/V_CCO_AUTHORIZED_ENTITIES/);
 assert.equal((sql.match(/PKG_CCO_SCOPE.P_ASSERT_SCOPE/g)||[]).length,2);
});
test('Report E has no raw fact-multiplying mapping join, uses same view in F8',()=>{
 for(const s of [report,f8]){
  assert.match(s,/group_report_candidates AS/);
  assert.match(s,/FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r/);
  assert.match(s,/group_report_map AS \(/);
  assert.match(s,/LEFT JOIN group_report_map gr/);
  assert.match(s,/WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1/);
  assert.match(s,/ELSE 'Unmapped \/ conflicting Division'/);
  assert.match(s,/WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1[\s\S]*?THEN gr.group_id/);
  assert.match(s,/P_REPORT_ID = 'E' AND s.audit_type = 'D'/);
  assert.match(s,/GROUPING SETS/);
  for(const routine of ['P_ANNUAL_FIELD_PROGRESS','P_FIELD_OFFICE_POSITION','P_GM_REGION_POSITION','P_REGION_RANKINGS','P_MANAGEMENT_SETTLEMENT'])assert.match(s,new RegExp(routine));
 }
});
test('reader uses trusted transaction and UI performs authorized API requests',()=>{
 const adapter=fs.readFileSync('AIS/DBConnection.CcoFunctionPosition.cs','utf8');
 assert.match(adapter,/return ReadCcoFunctionPosition\(scope, filters\)/);
 assert.match(adapter,/PKG_CCO_SCOPE.P_BEGIN_SCOPE/);
 assert.match(adapter,/PKG_CCO_SCOPE.P_ASSERT_SCOPE/);
 assert.match(adapter,/PKG_CCO_SCOPE.P_END_SCOPE/);
 assert.match(adapter,/V_CCO_AUTHORIZED_ENTITIES|PKG_CCO_FUNCTION_POSITION.P_GET_POSITION/);
 assert.match(adapter,/sourceCount>filters.MaxRows/);
 assert.match(adapter,/OracleConnection.ClearPool/);
 assert.doesNotMatch(adapter,/GetExecutiveReportE|GetCcoOrganizationRows/);
 const partial=fs.readFileSync('AIS/Views/ExecutiveOverview/_CcoFunctionPosition.cshtml','utf8');
 const js=fs.readFileSync('AIS/wwwroot/js/cco-function-position.js','utf8');
 assert.doesNotMatch(partial,/data-enabled="false"/);
 assert.match(partial,/GetFunctionPosition/);
 assert.match(js,/credentials: 'same-origin'/);
 assert.match(js,/response\.ok/);
 assert.match(js,/textContent/);
 assert.doesNotMatch(js,/innerHTML|eval\(/);
});

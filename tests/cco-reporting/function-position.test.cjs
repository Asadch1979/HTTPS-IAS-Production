const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs');
const sql=fs.readFileSync('AIS/Docs/sql/cco-review/PKG_CCO_FUNCTION_POSITION.sql.review','utf8');
test('function SQL preserves population and avoids multiplying facts',()=>{
 assert.match(sql,/s.audit_type='D'/);assert.match(sql,/SELECT DISTINCT m.parent_id group_id,m.p_name group_name/);
 assert.match(sql,/NVL\(m.status,'Y'\)='Y'/);assert.match(sql,/m.parent_id NOT IN\(112215\)/);assert.match(sql,/m.entity_id NOT IN\(112221\)/);
 assert.match(sql,/FROM mapping_candidates GROUP BY division_id/);assert.match(sql,/FROM V_EXEC_ENTITY_SCOPE GROUP BY entity_id/);
 assert.match(sql,/h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id/);
 assert.match(sql,/HAVING COUNT\(\*\)<>1 OR s.com_id IS NULL/);assert.match(sql,/GROUPING SETS/);
 assert.match(sql,/s.close_on IS NULL OR s.close_on>=P_END_DATE\+1/);
 assert.match(sql,/NULLIF\(SUM\(opening_balance\+paras_added\),0\)/);
 assert.match(sql,/V_CCO_AUTHORIZED_ENTITIES/);assert.equal((sql.match(/PKG_CCO_SCOPE.P_ASSERT_SCOPE/g)||[]).length,2);
});
test('tab and independent API remain gated',()=>{
 assert.match(fs.readFileSync('AIS/Models/CcoDashboardPresentation.cs','utf8'),/View\("org-position", "Function Wise Position"/);
 assert.match(fs.readFileSync('AIS/Views/ExecutiveOverview/_CcoFunctionPosition.cshtml','utf8'),/data-enabled="false"/);
 const adapter=fs.readFileSync('AIS/DBConnection.CcoFunctionPosition.cs','utf8');assert.match(adapter,/throw new CcoOrganizationUnavailableException/);assert.match(adapter,/sourceCount>filters.MaxRows/);assert.match(adapter,/OracleConnection.ClearPool/);
});

'use strict';
// Static contract checks, not a substitute for Oracle compilation or UAT.
// Run: node --test tests/executive-dashboard/lifecycle-post-compliance.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const read = p => fs.readFileSync(path.resolve(__dirname,'../..',p),'utf8');
const lifecycle = read('AIS/Docs/sql/V_EXEC_KPI_LIFECYCLE.sql');
const pkg = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const reference = read('AIS/Docs/sql/R_IAS_PARA_POSITION.sql');

test('lifecycle KPI facts use post-compliance alone, with dimension-only joins',()=>{
  assert.match(lifecycle,/FROM ais_t_au_post_compliance p/);
  assert.doesNotMatch(lifecycle,/JOIN t_au_plan_eng|FROM t_au_observation|UNION ALL SELECT \* FROM v_exec_kpi_src_/i);
  assert.match(lifecycle,/LEFT JOIN period_lookup ap/);
  assert.match(lifecycle,/COUNT\(DISTINCT t\.auditperiodid\) = 1/);
  assert.match(lifecycle,/LEFT JOIN v_exec_entity_scope s/);
});
test('modern issue count is unique per observation, not duplicated per COM_ID',()=>{
  assert.match(lifecycle,/ROW_NUMBER\(\) OVER/);
  assert.match(lifecycle,/p\.new_para_id IS NOT NULL AND p\.issue_row_num = 1/);
  assert.match(lifecycle,/k\.record_type = 'PARA'/);
  assert.match(lifecycle,/WHERE p\.ind IN \('A', 'O', 'C'\)/);
});
test('rectified flags do not also count as para additions and settlements',()=>{
  assert.match(lifecycle,/NVL\(p\.issue_rectified, 0\) = 1/);
  assert.match(lifecycle,/p\.ind <> 'A' OR NVL\(p\.issue_rectified, 0\) <> 1/);
  assert.match(lifecycle,/THEN p\.para_added_on END AS issue_rectified_on/);
  assert.match(lifecycle,/AS excluded_pre_finalization/);
  assert.match(pkg,/k\.para_status = 8/);
  assert.match(pkg,/k\.raw_para_settled_on < V_FROM/);
  assert.match(pkg,/k\.raw_para_settled_on >= V_TO_EXCLUSIVE/);
  assert.match(pkg,/k\.para_added_on >= V_FROM/);
});
test('old unknown status/conclusion metadata never manufactured',()=>{
  for(const col of ['observation_status','report_concluded_on','eng_id','audit_startdate','audit_enddate'])
    assert.match(lifecycle,new RegExp('CAST\\(NULL AS (?:DATE|NUMBER)\\) AS '+col));
  assert.match(pkg,/CAST\(NULL AS NUMBER\) AS issue_dropped/);
  assert.doesNotMatch(pkg,/JOIN t_au_observation dropped_o/);
});
test('procedure signatures and cursor aliases consumed by DBConnection remain present',()=>{
  for(const proc of ['P_GET_KPI_LIFECYCLE','P_GET_CIA_ISSUES','P_GET_CIA_EXEC_KPIS','P_GET_CIA_ISSUE_NATURE','P_GET_SNAP_DISTRIBUTION','P_GET_SNAP_CRITICAL'])
    assert.equal((pkg.match(new RegExp('PROCEDURE '+proc+'\\(', 'g'))||[]).length,2,proc);
  for(const alias of ['issue_identified','issue_rectified','issue_dropped','para_added','paras_settled',
                      'opening_open_paras','closing_open_paras','high_risk_open_paras'])
    assert.match(pkg,new RegExp('AS '+alias+'\\b','i'));
  assert.match(reference,/AND issue_eligible = 1/);
});
test('authenticated-scope and risk checks are not weakened',()=>{
  assert.match(pkg,/PKG_AUDIT_SNAP_SCOPE\.REQUIRE_SELECTION/);
  assert.match(pkg,/REQUIRE_ROLE_DASHBOARD/);
  assert.match(pkg,/VALIDATE_REQUEST/);
  assert.match(pkg,/P_AUDITED_BY_ID IS NULL OR k\.audited_by_id = P_AUDITED_BY_ID/);
  assert.match(pkg,/k\.period_id = P_AUDIT_PERIOD_ID/);
});
test('legacy source views are para-only compatibility projections',()=>{
  for(const ind of ['A','O','C']){
    const sql=read('AIS/Docs/sql/V_EXEC_KPI_SRC_'+ind+'.sql');
    assert.match(sql,/FROM V_EXEC_KPI_LIFECYCLE/);
    assert.match(sql,/WHERE record_type = 'PARA'/);
    assert.match(sql,new RegExp("AND source_ind = '"+ind+"'"));
    assert.doesNotMatch(sql,/FROM t_au_observation|FROM ais_t_au_post_compliance/i);
  }
});

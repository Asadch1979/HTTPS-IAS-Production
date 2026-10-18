'use strict';
// Static source checks only. Oracle DDL and data reconciliation are manual.
// Run: node --test tests/executive-dashboard/cia-checklist-nature.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = p => fs.readFileSync(path.join(root,p),'utf8');
const pkg = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const rootView = read('AIS/Docs/sql/V_IAS_CIA_ISSUE_CHECKLIST_ROOT.sql');
const ddl = read('AIS/Docs/sql/T_IAS_CIA_ISSUE_NATURE_MAP.sql');
const migration = read('AIS/Docs/sql/cia_issue_nature_checklist_root_migration.sql');
const lifecycle = read('AIS/Docs/sql/V_EXEC_KPI_LIFECYCLE.sql');

test('CIA checklist dimension preserves outer-join path and one observation key', ()=>{
  assert.match(rootView, /o\.id AS observation_id/);
  assert.match(rootView, /d\.id = o\.checklistdetail_id/);
  assert.match(rootView, /s\.s_id = d\.s_id/);
  assert.match(rootView, /c\.t_id = s\.t_id/);
  assert.match(rootView, /LEFT JOIN t_audit_checklist_details/);
});
test('CIA procedure keeps 10 categories and reconciled ISSUE eligibility and scope',()=>{
  const body = pkg.slice(pkg.lastIndexOf('  PROCEDURE P_GET_CIA_ISSUE_NATURE('));
  const nature=body.slice(0,body.indexOf('END P_GET_CIA_ISSUE_NATURE;'));
  assert.equal((nature.match(/UNION ALL SELECT/g)||[]).length,9);
  assert.match(nature,/croot\.checklist_root_id/);
  assert.match(nature,/LEFT JOIN v_ias_cia_issue_checklist_root croot/);
  assert.match(nature,/m\.source_type = 'CHECKLIST_ROOT'/);
  assert.match(nature,/WHEN 'CHECKLIST_ROOT' THEN 0 ELSE 1/);
  assert.match(nature,/k\.issue_eligible = 1/);
  assert.match(nature,/k\.period_id = P_AUDIT_PERIOD_ID/);
  assert.match(nature,/t\.total_issues, t\.unmapped_issues/);
  assert.match(nature,/WHEN t\.unmapped_issues > 0 THEN 'PARTIAL'/);
});
test('fresh DDL and migration use exact eight verified category codes',()=>{
  assert.match(ddl,/CHECKLIST_ROOT/);
  const pairs=[
    [1,'AML_CFT'],[5,'LOAN_ADVANCES'],[6,'CASH_MANAGEMENT'],
    [8,'ACCOUNTING_REPORTING_RECON'],[9,'RECORD_MANAGEMENT'],
    [14,'MANAGEMENT_AUDIT'],[15,'BANKING_OPERATIONS'],
    [16,'BRANCH_GENERAL_CONTROLS']
  ];
  for(const [id,code] of pairs){
    assert.match(migration,new RegExp("(?:ROOT'|ROOT',)\\s*,"+id+"\\s*,\\s*'"+code+"'|SELECT '"+'CHECKLIST_ROOT'+"' source_type,"+id+" source_id,\\s*'"+code+"'"));
  }
  assert.doesNotMatch(migration,/CHECKLIST_ROOT',\s*\d+,\s*'IS_AUDIT'/);
  assert.doesNotMatch(migration,/CHECKLIST_ROOT',\s*\d+,\s*'ACCOUNTS'/);
});
test('existing nine lifecycle KPI source unchanged by CIA classification refactor',()=>{
  assert.match(lifecycle,/NVL\(o\.issue_rectified, 0\) = 1/);
  assert.match(lifecycle,/UNION ALL SELECT \* FROM v_exec_kpi_src_a/);
  assert.match(pkg,/PROCEDURE P_GET_KPI_LIFECYCLE\(/);
  assert.match(pkg,/PROCEDURE P_GET_SNAP_DISTRIBUTION\(/);
  assert.match(pkg,/PROCEDURE P_GET_CIA_EXEC_KPIS\(/);
});

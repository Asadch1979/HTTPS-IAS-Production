'use strict';
// Database-only regression checks for selected-date Audit Para Snap cycle views.
// Run: node --test tests/executive-dashboard/audit-snap-asof-cycles.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname,'../../AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql'),'utf8');
function procedure(name) {
  const start = source.lastIndexOf('PROCEDURE '+name+'(');
  const end = source.indexOf('END '+name+';',start);
  assert.ok(start >= 0 && end > start,name);
  return source.slice(start,end);
}
const distribution = procedure('P_GET_SNAP_DISTRIBUTION');
const critical = procedure('P_GET_SNAP_CRITICAL');

test('cycle buckets always use master COM_CYCLE while portfolio remains date-scoped',()=>{
  assert.match(distribution,/k\.com_cycle AS snap_cycle/);
  assert.doesNotMatch(distribution,/history_asof|recorded_cycle|LEFT JOIN ais_t_au_post_compliance_history/i);
  assert.match(distribution,/Unknown cycle/); // Genuine invalid master values only.
  assert.match(distribution,/snap_cycle IS NULL OR snap_cycle < 0/);
  assert.match(distribution,/FROM scoped WHERE snap_open = 1/);
  assert.match(distribution,/k\.para_added_on < V_TO/);
  assert.match(distribution,/k\.para_settled_on >= V_TO/);
  assert.doesNotMatch(distribution,/FROM scoped WHERE snap_open = 1 AND V_TODAY = 1/);
  assert.match(distribution,/WHEN V_TODAY = 1 THEN 'Y' ELSE 'E' END quality_code/);
});

test('critical cursor uses master COM_CYCLE and history only for movement date',()=>{
  assert.match(critical,/k\.com_cycle AS snap_cycle/);
  assert.match(critical,/FROM ais_t_au_post_compliance_history h/);
  assert.match(critical,/h\.comment_on < V_TO/);
  assert.match(critical,/MAX\(h\.comment_on\) AS last_movement_on/);
  assert.doesNotMatch(critical,/recorded_cycle|MAX\(h\.com_cycle\)/);
  assert.match(critical,/k\.snap_cycle AS com_cycle/);
  assert.match(critical,/TRUNC\(P_END_DATE\) - TRUNC\(k\.last_movement_on\)/);
  assert.match(critical,/END AS days_since_movement/);
  assert.match(critical,/CASE WHEN V_TODAY = 1 THEN 'Y' ELSE 'E' END quality_code,\s+days_since_movement/);
  assert.match(critical,/AND k\.risk_id = 1 -- ZTBL Critical = High-risk open/);
});

test('scope restrictions and API cursor input contracts remain in both procedures',()=>{
  for (const sql of [distribution, critical]) {
    assert.match(sql,/VALIDATE_REQUEST\(P_DASHBOARD_TYPE, P_ENTITY_ID\)/);
    assert.match(sql,/PKG_AUDIT_SNAP_SCOPE\.REQUIRE_SELECTION/);
    assert.match(sql,/P_AUDITED_BY_ID IS NULL OR k\.audited_by_id = P_AUDITED_BY_ID/);
    assert.match(sql,/P_REPORTING_OFFICE_ID IS NULL/);
    assert.match(sql,/P_ENTITY_ID IS NULL/);
    assert.match(sql,/P_START_DATE IS NULL OR P_END_DATE IS NULL/);
  }
});

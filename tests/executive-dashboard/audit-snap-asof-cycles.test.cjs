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

test('historical cycle bucket counts use as-of source events rather than blanking every past date',()=>{
  assert.match(distribution,/FROM ais_t_au_post_compliance_history h/);
  assert.match(distribution,/h\.comment_on < V_TO/);
  assert.match(distribution,/h\.com_cycle IS NOT NULL/);
  assert.match(distribution,/WHEN h\.recorded_cycle IS NOT NULL THEN h\.recorded_cycle/);
  assert.match(distribution,/WHEN k\.com_cycle = 0 THEN 0/);
  assert.match(distribution,/Unknown cycle/);
  assert.match(distribution,/FROM scoped WHERE snap_open = 1/);
  assert.doesNotMatch(distribution,/FROM scoped WHERE snap_open = 1 AND V_TODAY = 1/);
  assert.doesNotMatch(distribution,/dataset = 'cycle' AND V_TODAY = 0 THEN/);
  assert.match(distribution,/WHEN V_TODAY = 1 THEN 'Y' ELSE 'E' END quality_code/);
});

test('critical cursor retains existing fields and exposes last known cycle and movement days',()=>{
  assert.match(critical,/FROM ais_t_au_post_compliance_history h/);
  assert.match(critical,/h\.comment_on < V_TO/);
  assert.match(critical,/MAX\(h\.comment_on\) AS last_movement_on/);
  assert.match(critical,/DENSE_RANK LAST ORDER BY[\s\S]*?h\.com_cycle IS NOT NULL/);
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

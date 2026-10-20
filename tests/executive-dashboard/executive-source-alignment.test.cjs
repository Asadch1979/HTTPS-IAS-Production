'use strict';
// Source contract regression checks only; they do not certify Oracle compilation.
// Run: node --test tests/executive-dashboard/executive-source-alignment.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = p => fs.readFileSync(path.join(root, p), 'utf8');
const optionalRead = p => fs.existsSync(path.join(root, p)) ? read(p) : null;
const pkg = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const lifecycle = read('AIS/Docs/sql/V_EXEC_KPI_LIFECYCLE.sql');
const reports = optionalRead('AIS/Docs/sql/V_EXEC_REPORT_SOURCE_image_rewrite.sql');
const recon = optionalRead('AIS/Docs/sql/V_EXEC_ISSUE_RECTIFIED_RECON.sql');
test('canonical package still exposes existing CIA and lifecycle procedures', () => {
  for (const name of ['P_GET_KPI_LIFECYCLE','P_GET_CIA_EXEC_KPIS','P_GET_CIA_ISSUE_NATURE',
    'P_GET_SNAP_DISTRIBUTION','P_GET_SNAP_CRITICAL']) {
    assert.equal(pkg.match(new RegExp('PROCEDURE ' + name + '\\(', 'g'))?.length, 2, name);
  }
});
test('CIA audited-entity numerator is entity- and selected-period-based', () => {
  assert.match(pkg, /completed_entities AS/);
  assert.match(pkg, /e\.period_id = P_AUDIT_PERIOD_ID/);
  assert.match(pkg, /ON ce\.entity_id = p\.entity_id/);
  assert.doesNotMatch(pkg, /completed_plan_ids AS/);
  assert.match(pkg, /COUNT\(DISTINCT CASE WHEN ce\.entity_id IS NOT NULL/);
});
test('unavailable pre-transfer drops return NULL, not invented zero', () => {
  assert.match(pkg, /CAST\(NULL AS NUMBER\) AS issue_dropped/);
  assert.doesNotMatch(pkg, /JOIN t_au_observation dropped_o/);
});
test('valid lifecycle ISSUE rectification comes from post-compliance source flag', (t) => {
  assert.match(lifecycle, /NVL\(p\.issue_rectified, 0\) = 1/);
  assert.match(lifecycle, /p\.setteled_on END AS issue_rectified_on/);
  assert.match(pkg, /k\.record_type = 'ISSUE'\s+AND k\.issue_eligible = 1\s+AND k\.issue_rectified_on/);
  if (recon) assert.match(recon, /KEEP\s+\(DENSE_RANK LAST ORDER BY l\.id\)/);
  else t.diagnostic('Optional reconciliation view DDL is absent from this checkout.');
});
test('para base does not disguise missing or negative aging as zero-to-ninety days', () => {
  assert.match(pkg, /AS aging_days/);
  assert.match(pkg, /THEN 'UNKNOWN'\s+WHEN TRUNC/);
  assert.match(pkg, /END AS aging_quality/);
  assert.match(pkg, /END AS issue_rectified_copy/);
});
test('report source keeps existing cursor fields and exposes movement anomalies', (t) => {
  if (!reports) return t.skip('Optional report-source DDL is absent from this checkout.');
  for (const col of ['effective_added_on','close_on','history_close_stage',
    'report_movement_quality','issue_rectified_copy']) {
    assert.match(reports, new RegExp('AS '+ col));
  }
  assert.match(reports, /TRUNC\(h\.history_close_on\) = TRUNC\(k\.para_settled_on\)/);
  assert.match(reports, /k\.record_type = 'PARA'/);
});

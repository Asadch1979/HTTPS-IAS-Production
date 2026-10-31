'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = p => fs.readFileSync(path.join(root,p),'utf8');

const bodies = ['AIS/Docs/sql/PKG_EXEC_REPORTS.sql'];

test('Report C resolves direct GM, audited-by HO mapping and self GM without altering eligibility', () => {
  const source = read(bodies[0]);
  for (const file of bodies) {
    const sql = read(file);
    assert.match(sql, /report_c_direct_gm AS/);
    assert.match(sql, /COUNT\(DISTINCT m\.gm_office\) = 1/);
    assert.match(sql, /gm\.entity_id = m\.gm_office/);
    assert.match(sql, /gm\.type_id = 21/);
    assert.match(sql, /report_c_auditor_gm AS/);
    assert.match(sql, /COUNT\(DISTINCT m\.entity_id\) = 1/);
    assert.match(sql, /report_c_gm_auditor AS/);
    assert.match(sql, /gm_audited\.audited_by =/);
    assert.match(sql, /COALESCE\(md\.mapped_audit_by, f\.audited_by\)/);
    assert.match(sql, /NVL\(md\.ambiguous_audit_by, 0\) = 0/);
    assert.match(sql, /f\.entity_type_id NOT IN \(6, 21, 22, 28\)/);
    assert.match(sql, /COALESCE\(md\.mapped_gm_id, f\.gm_office_id\)/);
    assert.match(sql, /WHEN f\.entity_type_id = 21 THEN f\.entity_id/);
    assert.match(sql, /WHEN 'C' THEN f\.report_c_gm_id/);
    assert.match(sql, /WHEN 'C' THEN f\.report_c_item_id/);
    assert.match(sql, /WHEN 'C' THEN NVL\(report_c_gm\.name, 'Unassigned GM'\)/);
    assert.match(sql, /WHEN 'C' THEN NVL\(report_c_item\.name, 'Unassigned Region \/ H\.O\. Unit'\)/);
    assert.match(sql, /report_c_item\.entity_id = f\.report_c_item_id/);
    // No raw mapping joined to facts: only aggregated one-row-per-key sources.
    const cte = sql.slice(sql.indexOf('report_c_offices AS ('),sql.indexOf('dimensions AS ('));
    assert.doesNotMatch(cte,/JOIN t_auditee_entities_maping/);
    assert.match(sql,/P_SCOPE_ROOT_ID IN/);
    if (file !== 'AIS/Docs/sql/PKG_EXEC_REPORTS_image_rewrite_body.sql')
      assert.match(sql,/PKG_AUDIT_SNAP_SCOPE\.REQUIRE_SELECTION/);
    // Legacy image rewrite is for reference only; never deploy it in place of
    // the authenticated canonical and F8 package bodies.
    if (file !== 'AIS/Docs/sql/PKG_EXEC_REPORTS_image_rewrite_body.sql')
      assert.match(sql,/P_SNAP_AUDITED_BY_ID IS NULL OR s\.audited_by = P_SNAP_AUDITED_BY_ID/);
    assert.match(sql,/P_REPORT_ID = 'D' AND top_rank <= 5/);
  }
  assert.ok(source.includes('WHEN \'D\' THEN f.region_id'));
});

test('Report C browser and PDF name HO units instead of unnamed regions',()=>{
  assert.match(read('AIS/wwwroot/js/executive-reports.js'),/Region \/ H\.O\. unit/);
  assert.match(read('AIS/Services/ExecutiveReportsPdfGenerator.cs'),/GM \/ region and H\.O\. unit paras position/);
});

test('diagnostics expose type IDs, effective GM and ambiguous GM mappings without mutation',()=>{
  const diagnostic = read('AIS/Docs/sql/REPORT_C_GM_REGION_MAPPING_VERIFY.sql');
  assert.match(diagnostic,/COUNT\(DISTINCT m\.entity_id\) AS gm_candidates/);
  assert.match(diagnostic,/valid_mapped_gm/);
  assert.match(diagnostic,/mapping_quality/);
  assert.doesNotMatch(diagnostic,/^\s*(UPDATE|DELETE|MERGE|INSERT|ALTER|DROP|CREATE)\s+/im);
});

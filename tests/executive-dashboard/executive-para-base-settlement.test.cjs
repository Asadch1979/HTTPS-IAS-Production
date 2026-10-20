'use strict';
// DB-only regression checks. Run:
// node --test tests/executive-dashboard/executive-para-base-settlement.test.cjs
// Static checks are NOT Oracle compilation or live count validation.
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const read = p => fs.readFileSync(path.resolve(__dirname,'../..',p),'utf8');
const pkg = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const first = pkg.indexOf('CREATE OR REPLACE VIEW V_EXEC_PARA_BASE AS');
const last = pkg.indexOf('\n/\nCREATE OR REPLACE PACKAGE PKG_EXEC_DASHBOARD',first);
const view = pkg.slice(first,last);
const lifecycle = read('AIS/Docs/sql/V_EXEC_KPI_LIFECYCLE.sql');
const verification = read('AIS/Docs/sql/V_EXEC_PARA_BASE_settlement_verify.sql');

test('post-compliance executive view excludes ONLY pre-conclusion modern rectifications',()=>{
    assert.ok(first >= 0 && last > first);
    assert.match(view,/FROM ais_t_au_post_compliance c/i);
    assert.match(view,/WHERE CASE WHEN c\.ind = 'A' AND NVL\(c\.issue_rectified, 0\) = 1\s+THEN 0 ELSE 1 END = 1/);
    assert.match(view,/CASE WHEN c\.para_status = 8 THEN 1\s+ELSE 0\s+END AS is_open/);
    assert.match(view,/CASE WHEN c\.para_status = 9 THEN 1\s+ELSE 0\s+END AS is_settled/);
    assert.doesNotMatch(view,/TRUNC\(SYSDATE\).*?WHERE/i);
});

test('executive and role-dashboard existing cursor procedures consume the corrected view',()=>{
    for(const proc of ['P_GET_KPI_SUMMARY','P_GET_RISK_SUMMARY',
       'P_GET_AGING_SUMMARY','P_GET_CYCLE_SUMMARY',
       'P_GET_SETTLEMENT_TREND','P_GET_CRITICAL_ITEMS',
       'P_GET_ROLE_ENTITY_HEATMAP','P_GET_ROLE_UNIT_COMPARISON','P_GET_ROLE_FOLLOWUPS']){
        const found = pkg.lastIndexOf('PROCEDURE '+proc+'(');
        const end = pkg.indexOf('END '+proc+';',found);
        assert.ok(found > last && end > found, proc+' body is present');
        assert.match(pkg.slice(found,end),/V_EXEC_PARA_BASE/i,proc);
    }
    assert.match(pkg,/REQUIRE_ROLE_DASHBOARD/);
    assert.match(pkg,/PKG_AUDIT_SNAP_SCOPE\.REQUIRE_SELECTION/);
});

test('lifecycle distinct issue count and same-period settlements remain unchanged',()=>{
    assert.match(lifecycle,/ROW_NUMBER\(\) OVER/);
    assert.match(lifecycle,/p\.ind <> 'A' OR NVL\(p\.issue_rectified, 0\) <> 1/);
    assert.match(lifecycle,/p\.para_status = 9/);
    assert.match(verification,/expected_same_period/);
    assert.match(verification,/excluded_preconclusion_settled/);
    assert.match(verification,/verification_status/);
    assert.doesNotMatch(verification,/\b(?:DELETE|UPDATE|MERGE|TRUNCATE|DROP)\s+(?:TABLE|VIEW|ais_t_au)/i);
});

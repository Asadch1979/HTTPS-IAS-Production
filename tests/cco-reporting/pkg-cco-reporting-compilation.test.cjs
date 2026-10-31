'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = p => fs.readFileSync(p, 'utf8').replace(/\r\n/g, '\n');
const script = read('AIS/Docs/sql/PKG_CCO_REPORTING.sql');
const review = script;
const specStart = 'CREATE OR REPLACE PACKAGE PKG_CCO_REPORTING AS';
const bodyStart = 'CREATE OR REPLACE PACKAGE BODY PKG_CCO_REPORTING AS';

test('canonical CCO reporting package has spec and body', () => {
    assert.ok(script.includes(specStart));
    assert.ok(script.includes(bodyStart));
    assert.ok(script.indexOf(specStart)<script.indexOf(bodyStart));
});

test('T_AUDITEE_ENTITIES_MAPING uses confirmed AUDITEDBY without stale audit_by aliases', () => {
    assert.match(review, /COUNT\(DISTINCT m\.auditedby\) auditor_count/);
    assert.match(review, /m\.auditedby AS auditedby/);
    assert.match(review, /gm_map\.auditedby=m\.auditedby/);
    assert.match(review, /ag\.auditedby=m\.auditedby/);
    assert.doesNotMatch(review, /\baudit_by\b/i);
});

test('all joined report fact columns have unique named projections', () => {
    const query = review.slice(review.indexOf('WITH hierarchy AS'), review.indexOf('-- Cursor protocol'));
    for (const alias of ['r', 'q', 'g']) {
        assert.doesNotMatch(query, new RegExp('SELECT\\s+' + alias + '\\.\\*', 'i'));
    }
    for (const clause of [
        'r.com_id, r.entity_id, r.region_id, r.risk',
        'k.com_id AS lifecycle_id, k.com_cycle AS com_cycle',
        'q.com_id, q.entity_id, q.region_id, q.risk',
        'g.date_from, g.date_to, g.org_group',
        'g.outstanding-g.opening_cases AS net_backlog_change'
    ]) assert.ok(query.includes(clause), clause);
    assert.match(query, /GROUPING SETS/);
    assert.match(query, /V_CCO_AUTHORIZED_ENTITIES/);
});

test('security is unchanged and reporting remains fail-closed', () => {
    assert.match(review, /RAISE_APPLICATION_ERROR\(-20803,'CCO organizational reporting is not verified'\)/);
    assert.match(review, /RAISE_APPLICATION_ERROR\(-20803,'Verified CCO selector path reader pending'\)/);
    assert.match(review, /REQUIRE_VERIFIED_CONTEXT\(P_SCOPE_TOKEN\)/);
    assert.doesNotMatch(review, /^\s*(GRANT|REVOKE|ALTER SYSTEM|DROP TABLE|CREATE CONTEXT)\b/im);
});

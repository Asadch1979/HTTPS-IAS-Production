'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const root = path.join(__dirname, '..', '..');
const source = rel => fs.readFileSync(path.join(root, rel), 'utf8');

test('List/Lookup do not parse missing request ID or version', () => {
  const pkg = source('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
  const pos = pkg.indexOf('-- The five operations have distinct request DTOs.');
  const end = pkg.indexOf("IF p_operation = 'LOOKUP' THEN", pos);
  assert.ok(pos >= 0 && end > pos, 'Expected operation-specific input parsing block');
  const parsing = pkg.slice(pos, end);
  assert.match(parsing, /IF p_operation IN \('SUBMIT', 'DETAIL', 'DECIDE'\) THEN/);
  assert.match(parsing, /IF p_operation IN \('SUBMIT', 'DECIDE'\) THEN/);
  assert.match(parsing, /IF p_operation IN \('LOOKUP', 'LIST'\) THEN/);
  assert.match(parsing, /requestId' RETURNING NUMBER ERROR ON ERROR NULL ON EMPTY/);
  assert.match(parsing, /version' RETURNING NUMBER ERROR ON ERROR NULL ON EMPTY/);
  assert.doesNotMatch(parsing, /v_version\s*:=\s*JSON_VALUE\(p_input[^;]*ERROR ON ERROR\);/s);
});

test('Invalid detail/decision IDs and version mismatches still fail closed', () => {
  const pkg = source('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
  assert.match(pkg, /p_operation IN \('DETAIL', 'DECIDE'\) AND NVL\(v_id, 0\) <= 0/);
  assert.match(pkg, /p_operation = 'DECIDE' OR NVL\(v_id, 0\) > 0/);
  assert.match(pkg, /A valid request ID is required/);
  assert.match(pkg, /A valid request version is required/);
  assert.match(pkg, /Only the current returned request can be corrected/);
});

test('Database retains exact role/API/page grant and mapping scope checks', () => {
  const pkg = source('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
  assert.match(pkg, /JOIN T_IAS_LEGACY_PAGE_API dep/);
  assert.match(pkg, /rp\.role_id = v_role/);
  assert.match(pkg, /dep\.is_active = 'Y'/);
  assert.match(pkg, /'apicalls\/legacypara' \|\| LOWER\(p_operation\)/);
  assert.match(pkg, /e\.auditby_id = v_entity/);
  assert.match(pkg, /m\.auditedby <> e\.auditby_id/);
  assert.match(pkg, /v_get_parent_office/);
  assert.match(pkg, /FOR UPDATE/);
  assert.doesNotMatch(pkg, /COMMIT\s*;/i);
});

test('Controller supplies operation/page from internal enums and DTOs differ', () => {
  const db = source('AIS/DBConnection.LegacyParaRequests.cs');
  const models = source('AIS/Models/LegacyParaRequest.cs');
  assert.match(db, /operation\.ToString\(\)\.ToUpperInvariant\(\)/);
  assert.match(db, /Enum\.IsDefined\(typeof\(LegacyParaPage\), page\)/);
  const query = models.split('public sealed class LegacyParaQuery')[1].split('public sealed class LegacyParaLookup')[0];
  assert.match(query, /RequestId/);
  assert.doesNotMatch(query, /Version/);
  const lookup = models.split('public sealed class LegacyParaLookup')[1];
  assert.doesNotMatch(lookup, /RequestId/);
  assert.doesNotMatch(lookup, /Version/);
});

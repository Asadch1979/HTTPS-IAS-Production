'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = file => fs.readFileSync(path.join(root, file), 'utf8');

test('Legacy Para mirrors manage-audit-paras location labels and relationship source', () => {
  const manage = read('AIS/Views/Execution/manage_audit_paras.cshtml');
  const legacy = read('AIS/Views/Execution/Add_Legacy_Para.cshtml');
  const controller = read('AIS/Controllers/ExecutionController.cs');
  for (const label of ['Relationship Type', 'Controlling/Reporting Office', 'Place of Posting']) {
    assert.ok(manage.includes(label), 'Reference page missing ' + label);
    assert.ok(legacy.includes(label), 'Legacy page missing ' + label);
  }
  assert.match(legacy, /id="relationshipSelectField"/);
  assert.match(legacy, /id="reportingOfficeSelectField"/);
  assert.match(legacy, /id="entitySelectField"/);
  assert.doesNotMatch(legacy, /Relation Type \(Entity Type\)/);
  assert.doesNotMatch(legacy, /Reporting Office \(Audit By\)/);
  assert.doesNotMatch(legacy, /Audit Entity<\/label>/);
  assert.match(controller, /ViewData\["Userrelationship"\] = dBConnection\.GetrealtionshiptypeForParaPositionReport\(\)/);
});

test('place of posting is loaded only after scoped relationship and controlling office', () => {
  const js = read('AIS/wwwroot/js/Execution/Views_Execution_Add_Legacy_Para.js');
  assert.match(js, /lookup\('REPORTING_OFFICES',relation\)/);
  assert.match(js, /lookup\('POSTINGS',office,'',relation\)/);
  assert.match(js, /option\.dataset\.entityTypeId/);
  assert.match(js, /relationshipId:num\('relationshipSelectField'\)/);
  assert.match(js, /reportingOfficeId:num\('reportingOfficeSelectField'\)/);
  assert.match(js, /entityTypeId:num\('legacyEntityTypeId'\)/);
  assert.match(js, /lookup\('ENTITY_LOCATION',p\.entityId\)/);
  assert.doesNotMatch(js, /lookup\('ENTITIES',val\('entityTypeSelectField'\)/);
});

test('server and Oracle deny posting outside authorized relation/office/entity mapping', () => {
  const models = read('AIS/Models/LegacyParaRequest.cs');
  const pkg = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
  assert.match(models, /public int RelationshipId/);
  assert.match(models, /public int ReportingOfficeId/);
  assert.match(models, /REPORTING_OFFICES\|POSTINGS\|ENTITY_LOCATION/);
  assert.match(pkg, /FUNCTION relationship_allowed\(p_relationship NUMBER\) RETURN BOOLEAN/);
  assert.match(pkg, /PROCEDURE check_location\(p_entity NUMBER,p_relationship NUMBER,p_office NUMBER\)/);
  assert.match(pkg, /m\.entity_id=p_entity AND m\.parent_id=p_office/);
  assert.match(pkg, /m\.relation_type_id=p_relationship AND r\.status='Y'/);
  assert.match(pkg, /check_location\(v_data\.entity_id,v_relation,v_reporting_office\)/);
  assert.match(pkg, /check_location\(v_row\.entity_id,v_relation,v_reporting_office\)/);
  assert.match(pkg, /e\.auditby_id=v_entity AND e\.auditable='Y'/);
  assert.match(pkg, /v_kind='REPORTING_OFFICES'/);
  assert.match(pkg, /v_kind='POSTINGS'/);\n  const lookups = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA_LOOKUP.sql');\n  assert.match(lookups, /PROCEDURE P_GET_LOOKUP/);\n  assert.match(lookups, /p_kind='POSTINGS'/);
  assert.match(pkg, /v_kind='ENTITY_LOCATION'/);
});

test('HTTP 403 errors retain exact diagnostic codes without bypass or grant updates', () => {
  const js = read('AIS/wwwroot/js/Execution/legacy-para-requests.js');
  const diagnostic = read('AIS/Docs/sql/IAS_LEGACY_PARA_ACCESS_DIAGNOSTIC.sql');
  assert.match(js, /API_FORBIDDEN/);
  assert.match(js, /PAGE_FORBIDDEN/);
  assert.match(js, /DATABASE_FORBIDDEN/);
  assert.match(js, /CONTEXT_REQUIRED/);
  assert.doesNotMatch(js, /403: 'Your selected context does not have permission for this operation.'/);
  assert.match(diagnostic, /LEFT JOIN t_ias_legacy_page_api/);
  assert.match(diagnostic, /role_id = 15/);
  assert.doesNotMatch(diagnostic, /\bUPDATE\s+T_AU_ROLE_API_PERMISSION\b/i);
  assert.doesNotMatch(diagnostic, /\bCOMMIT\s*;/i);
});

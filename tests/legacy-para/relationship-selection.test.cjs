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

test('location dropdowns reuse the two existing Manage Audit Paras endpoints', () => {
  const ref = read('AIS/wwwroot/js/csp/Views_Execution_manage_audit_paras.js');
  const js = read('AIS/wwwroot/js/Execution/Views_Execution_Add_Legacy_Para.js');
  const view = read('AIS/Views/Execution/Add_Legacy_Para.cshtml');
  for (const endpoint of ['getparentrelForParaPositionReport','getpostplaceForParaPositionReport']) {
    assert.match(ref, new RegExp(endpoint));
    assert.match(js, new RegExp(endpoint));
  }
  assert.match(js, /ENTITY_REALTION_ID:relation/);
  assert.match(js, /E_R_ID:office/);
  assert.match(js, /item\.entitY_ID/);
  assert.match(js, /item\.description/);
  assert.match(js, /item\.c_NAME/);
  assert.match(js, /relationshipId:num\('relationshipSelectField'\)/);
  assert.match(js, /reportingOfficeId:num\('reportingOfficeSelectField'\)/);
  assert.match(js, /entityId:num\('entitySelectField'\)/);
  assert.match(view, /data-location-api=/);
  assert.doesNotMatch(js, /lookup\('REPORTING_OFFICES'/);
  assert.doesNotMatch(js, /lookup\('POSTINGS'/);
  assert.doesNotMatch(js, /legacyEntityTypeId/);
  assert.doesNotMatch(view, /legacyEntityTypeId/);
});

test('server and Oracle deny posting outside authorized relation/office/entity mapping', () => {
  const models = read('AIS/Models/LegacyParaRequest.cs');
  const pkg = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
  assert.match(models, /public int RelationshipId/);
  assert.match(models, /public int ReportingOfficeId/);
  assert.doesNotMatch(models, /public int EntityTypeId/);
  assert.doesNotMatch(pkg, /JSON_VALUE\(p_input,'\$\.entityTypeId'/);
  assert.match(pkg, /v_data\.entity_type_id:=v_type/);
  assert.match(models, /REPORTING_OFFICES\|POSTINGS\|ENTITY_LOCATION/);
  assert.match(pkg, /FUNCTION relationship_allowed\(p_relationship NUMBER\) RETURN BOOLEAN/);
  assert.match(pkg, /PROCEDURE check_location\(p_entity NUMBER,p_relationship NUMBER,p_office NUMBER\)/);
  assert.match(pkg, /m\.entity_id=p_entity AND m\.parent_id=p_office/);
  assert.match(pkg, /m\.relation_type_id=p_relationship AND r\.status='Y'/);
  assert.match(pkg, /check_location\(v_data\.entity_id,v_relation,v_reporting_office\)/);
  assert.match(pkg, /check_location\(v_row\.entity_id,v_relation,v_reporting_office\)/);
  assert.match(pkg, /e\.auditby_id=v_entity AND e\.auditable='Y'/);
  assert.match(pkg, /v_kind='REPORTING_OFFICES'/);
  assert.match(pkg, /v_kind='POSTINGS'/);
  const lookups = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA_LOOKUP.sql');
  assert.match(lookups, /PROCEDURE P_GET_LOOKUP/);
  assert.match(lookups, /p_kind='POSTINGS'/);
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

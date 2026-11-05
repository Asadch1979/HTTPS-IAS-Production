'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = file => fs.readFileSync(path.join(root, file), 'utf8');

test('maker page captures data and never initializes a request list', () => {
    const maker = read('AIS/Views/Execution/Add_Legacy_Para.cshtml');
    const checker = read('AIS/Views/Execution/Authorize_Adding_Legacy_Para.cshtml');
    const shared = read('AIS/wwwroot/js/Execution/legacy-para-requests.js');
    const capture = read('AIS/wwwroot/js/Execution/Views_Execution_Add_Legacy_Para.js');
    assert.doesNotMatch(maker, /_LegacyParaRequestQueue/);
    assert.match(checker, /_LegacyParaRequestQueue/);
    assert.match(shared, /if \(checker\) \{/);
    assert.doesNotMatch(capture, /app\.refresh\(\)/);
    assert.doesNotMatch(capture, /legacyCorrect/);
    assert.match(capture, /app\.api\('Submit',payload\)/);
});

test('read-only maker lookups and checker pending lists use separate procedures', () => {
    const controller = read('AIS/Controllers/ApiCallsController.LegacyPara.cs');
    const lookup = read('AIS/DBConnection.LegacyParaLookup.cs');
    const pending = read('AIS/DBConnection.LegacyParaPending.cs');
    const workflow = read('AIS/DBConnection.LegacyParaWorkflow.cs');
    assert.match(controller, /LegacyParaReferenceLookup\(\(LegacyParaLookup\)input, page\)/);
    assert.match(controller, /LegacyParaPending\(\(LegacyParaQuery\)input, page\)/);
    assert.match(lookup, /PKG_IAS_LEGACY_PARA_LOOKUP\.P_GET_LOOKUP/);
    assert.match(pending, /PKG_IAS_LEGACY_PARA_PENDING\.P_GET_PENDING/);
    assert.match(workflow, /LegacyParaOperation\.List or LegacyParaOperation\.Lookup/);
    assert.match(workflow, /throw new NotSupportedException/);
    assert.doesNotMatch(lookup, /JsonSerializer/);
    assert.doesNotMatch(pending, /JsonSerializer/);
    assert.equal(fs.existsSync(path.join(root,'AIS/DBConnection.LegacyParaRequests.cs')), false);
});

test('dedicated read procedures preserve page and exact selected role grants', () => {
    const lookup = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA_LOOKUP.sql');
    const pending = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA_PENDING.sql');
    assert.match(lookup, /p_page<>192/);
    assert.match(pending, /p_page <> 193/);
    assert.match(lookup, /JOIN T_IAS_LEGACY_PAGE_API dep/);
    assert.match(pending, /JOIN T_IAS_LEGACY_PAGE_API dep/);
    assert.match(lookup, /rp\.role_id=v_role/);
    assert.match(pending, /rp\.role_id=v_role/);
    assert.match(lookup, /'apicalls\/legacyparalookup'/);
    assert.match(pending, /'apicalls\/legacyparalist'/);
    assert.match(lookup, /e\.auditby_id=v_entity/);
    assert.match(pending, /r\.audited_by=v_entity/);
    assert.match(pending, /r\.status='PENDING'/);
    assert.doesNotMatch(lookup, /COMMIT\s*;/i);
    assert.doesNotMatch(pending, /COMMIT\s*;/i);
});

test('remaining workflow no longer parses missing list/lookup input fields', () => {
    const pkg=read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
    const start=pkg.indexOf('The request JSON schemas are operation-specific.');
    const end=pkg.indexOf("IF p_operation='LOOKUP' THEN",start);
    assert.ok(start>0 && end>start);
    const parser=pkg.slice(start,end);
    assert.match(parser, /NULL ON EMPTY/);
    assert.match(parser, /IF p_operation IN \('SUBMIT','DETAIL','DECIDE'\)/);
    assert.match(parser, /IF p_operation IN \('SUBMIT','DECIDE'\)/);
    assert.match(parser, /IF p_operation IN \('LOOKUP','LIST'\)/);
    assert.doesNotMatch(parser, /^    v_version:=JSON_VALUE/m);
});

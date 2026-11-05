'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const read = rel => fs.readFileSync(path.join(root, rel), 'utf8');

test('Legacy Para has five dedicated secure actions within ApiCalls', () => {
    const file = read('AIS/Controllers/ApiCallsController.LegacyPara.cs');
    for (const action of ['Submit', 'Lookup', 'List', 'Detail', 'Decide']) {
        assert.match(file, new RegExp('public IActionResult LegacyPara' + action + '\\('));
    }
    assert.equal((file.match(/ValidateAntiForgeryToken/g) || []).length, 5);
    assert.match(file, /sessionHandler\.TryGetAllowedViewIds/);
    assert.match(file, /sessionHandler\.HasApiPermissionForPath/);
    assert.match(file, /LegacyParaPage\.Maker/);
    assert.match(file, /LegacyParaPage\.Checker/);
    assert.match(file, /LegacyParaApiFailure\(403, "CONTEXT_REQUIRED"/);
    assert.match(file, /LegacyParaApiFailure\(403, "PAGE_FORBIDDEN"/);
    assert.match(file, /LegacyParaApiFailure\(403, "API_FORBIDDEN"/);
    assert.match(file, /LegacyParaApiFailure\(503, "MODULE_DISABLED"/);
    assert.match(file, /ApplicationAudit\("LEGACY_PARA_REQUEST_SUBMITTED"/);
    assert.match(file, /ApplicationAudit\("LEGACY_PARA_REQUEST_DECIDED"/);
    assert.equal(fs.existsSync(path.join(root, 'AIS/Controllers/LegacyParaRequestsController.cs')), false);
});

test('operation, page and active context are server-bound, never in request payload', () => {
    const model = read('AIS/Models/LegacyParaOperation.cs');
    const db = read('AIS/DBConnection.LegacyParaWorkflow.cs');
    assert.match(model, /enum LegacyParaOperation/);
    assert.match(model, /Maker = 192/);
    assert.match(model, /Checker = 193/);
    assert.match(db, /LegacyParaWorkflow\(LegacyParaOperation operation, object input, LegacyParaPage page\)/);
    assert.match(db, /Enum\.IsDefined\(typeof\(LegacyParaOperation\)/);
    assert.match(db, /Enum\.IsDefined\(typeof\(LegacyParaPage\)/);
    assert.match(db, /operation\.ToString\(\)\.ToUpperInvariant\(\)/);
    assert.match(db, /user\.UserContextAssignmentId\.Value/);
    assert.match(db, /user\.UserRoleID/);
    assert.match(db, /user\.UserGroupID/);
});

test('both views, Oracle permission contract and permission manifest use ApiCalls', () => {
    const maker = read('AIS/Views/Execution/Add_Legacy_Para.cshtml');
    const checker = read('AIS/Views/Execution/Authorize_Adding_Legacy_Para.cshtml');
    const pkg = read('AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql');
    const grants = read('AIS/Docs/sql/IAS_LEGACY_PARA_API_PERMISSIONS.sql');
    for (const view of [maker, checker]) {
        assert.match(view, /~\/ApiCalls\/LegacyPara/);
        assert.doesNotMatch(view, /~\/api\/LegacyParaRequests/);
    }
    assert.match(pkg, /'apicalls\/legacypara'\s*\|\|\s*LOWER\(p_operation\)/);
    assert.doesNotMatch(pkg, /'api\/legacypararequests\/'\|\|LOWER\(p_operation\)/);
    for (const action of ['Submit', 'Lookup', 'List', 'Detail', 'Decide']) {
        assert.match(grants, new RegExp("'ApiCalls','LegacyPara" + action + "','POST'"));
    }
    assert.match(grants, /register_api\(193,'ApiCalls','LegacyParaList','POST'\)/);
    assert.match(grants, /register_api\(193,'ApiCalls','LegacyParaDetail','POST'\)/);
});

test('migration preserves API identifiers and pre-existing role decisions', () => {
    const sql = read('AIS/Docs/sql/IAS_LEGACY_PARA_APICALLS_MIGRATION.sql');
    assert.match(sql, /UPDATE T_AU_API_MASTER/);
    assert.doesNotMatch(sql, /UPDATE T_AU_ROLE_API_PERMISSION/);
    assert.doesNotMatch(sql, /DELETE FROM T_AU_ROLE_API_PERMISSION/);
    assert.doesNotMatch(sql, /\bCOMMIT\s*;/i);
    assert.match(sql, /ROLLBACK TO legacy_para_api_migration/);
});

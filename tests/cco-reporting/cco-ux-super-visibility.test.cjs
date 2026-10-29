const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = path => fs.readFileSync(path, 'utf8');

test('CCO Function Wise Position omits disclaimer text and subgroup rows', () => {
    const partial = read('AIS/Views/ExecutiveOverview/_CcoFunctionPosition.cshtml');
    const client = read('AIS/wwwroot/js/cco-function-position.js');
    const policy = read('AIS/Models/CcoReportVisibility.cs');
    assert.doesNotMatch(partial, /Management Audit observations \(audit type D\)/);
    assert.doesNotMatch(client, /Authorized Management Audit \(D\) observations/);
    assert.match(policy, /!string\.Equals\(row\.RowKind, "SUBTOTAL"/);
    assert.match(policy, /row\.GroupId\.HasValue && row\.DivisionId\.HasValue/);
    assert.match(policy, /string\.Equals\(row\.RowKind, "TOTAL"/);
});

test('Unassigned details are removed in server APIs only for unverified super users', () => {
    const policy = read('AIS/Models/CcoReportVisibility.cs');
    const ccoApi = read('AIS/Controllers/CcoComplianceController.cs');
    const reportsApi = read('AIS/Controllers/ExecutiveReportsController.cs');
    assert.match(ccoApi, /CcoReportVisibility\.FunctionRows\(database\.GetCcoFunctionPosition\(scope,filters\),super\)/);
    assert.match(reportsApi, /scope\.DashboardType == "CCO" && !superUser/);
    assert.match(reportsApi, /CcoReportVisibility\.ExecutiveRows\(data\.Rows, false\)/);
    for (const label of ['Unassigned GM','Unassigned Region','Unmapped / conflicting'])
        assert.ok(policy.includes(label), label);
    assert.match(policy, /if \(isVerifiedSuperUser\) return rows\.ToList\(\)/);
    assert.match(policy, /"GRAND_TOTAL"/);
});

test('CCO Audit Position and GM/Region reports do not render nested tabs or cramped headers', () => {
    const report = read('AIS/Views/ExecutiveOverview/_Reports.cshtml');
    const css = read('AIS/wwwroot/css/executive-dashboard.css');
    const client = read('AIS/wwwroot/js/executive-reports.js');
    assert.match(report, /hidden="\@\(Model\.DashboardType == "CCO"\)"/);
    assert.match(css, /exec-report-selector\[hidden\]/);
    assert.match(css, /exec-report-table-wrap[\s\S]*?overflow-x:\s*auto/);
    assert.match(css, /exec-report-table[\s\S]*?table-layout:\s*auto/);
    assert.match(client, /root\.dataset\.perspective !== 'CCO'/);
    assert.match(client, /displayTitle} loaded/);
});

test('Group Head selector is authoritative, CCO-only and binds to scoped entity queries', () => {
    const page = read('AIS/Views/ExecutiveOverview/CCO.cshtml');
    const controller = read('AIS/Controllers/ExecutiveOverviewController.cs');
    const db = read('AIS/DBConnection.CcoOrganizationReporting.cs');
    const model = read('AIS/Models/ExecutiveDashboardPageModel.cs');
    const js = read('AIS/wwwroot/js/executive-dashboard.js');
    const partial = read('AIS/Views/ExecutiveOverview/_CcoOrganizations.cshtml');
    assert.match(page, /id="exec-entity" name="entityId"/);
    assert.match(page, /Model\.CcoGroupHeads/);
    assert.match(controller, /database\.GetCcoGroupHeads\(scope\)/);
    assert.match(controller, /!ccoGroupHeads\.Any\(group => group\.EntityId == entityId\.Value\)/);
    assert.match(db, /ValidateCcoOrganizationContext\(scope\)/);
    assert.match(db, /EntityTypeId == 18 && !n\.HasTypeConflict/);
    assert.match(model, /CcoGroupHeads/);
    assert.match(js, /!\['performance', 'effectiveness'\]\.includes/);
    assert.doesNotMatch(partial, /Search organization/);
    assert.match(partial, /data-enabled="false"/);
});

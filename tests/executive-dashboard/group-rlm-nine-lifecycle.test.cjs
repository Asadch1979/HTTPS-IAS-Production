'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = path => fs.readFileSync(path, 'utf8');

const nineKeys = [
    'openingOpenParas', 'issueIdentified', 'issueRectified',
    'paraAdded', 'netParaForCompliance', 'parasSettled', 'closingOpenParas',
    'highRiskOpenParas', 'mediumRiskOpenParas', 'lowRiskOpenParas'
];
const keysText = 'new[] { ' + nineKeys.map(key => '"' + key + '"').join(', ') + ' }';

test('TOP, GROUP management and RLM glance use the same exact ten lifecycle keys in order', () => {
    const source = read('AIS/Models/ExecutiveDashboardViews.cs');
    assert.match(source, /"TOP" => new\[\] \{ "openingOpenParas"/);
    assert.ok(source.includes('"GROUP" => ' + keysText));
    assert.ok(source.includes('"RLM" => ' + keysText));
    assert.ok(source.includes('new[] { "risk", "critical" }'), 'Audit Para’s remains available');
    assert.match(source, /View\("management", "Audit Para Position", "_ManagementAudit"/);
    assert.match(source, /glanceKpis, Array\.Empty<string>\(\)\)\)/);
    assert.match(source, /views\.RemoveAll\(v => v\.Id == "risk"\)/);
    assert.match(source, /views\.RemoveAll\(v => v\.Id == "position" \|\| v\.Id == "risk"\)/);
});

test('GROUP and RLM reuse existing page/API-scoped KPI summary and one approved lifecycle query', () => {
    const reader = read('AIS/DBConnection.ComplianceDashboard.cs');
    const model = read('AIS/Models/ExecutiveDashboardModels.cs');
    const api = read('AIS/Controllers/ExecutiveDashboardController.cs');
    const sqlPolicy = read('AIS/Models/ManagementDashboardPeriod.cs');
    assert.match(reader, /var k = ReadExecutiveKpiLifecycle\(scope, filters\)/);
    assert.equal((reader.match(/ReadExecutiveKpiLifecycle\(scope, filters\)/g) || []).length, 1);
    assert.match(reader, /Lifecycle = k/);
    assert.match(model, /public ExecutiveKpiLifecycle Lifecycle \{ get; set; \}/);
    assert.match(api, /HasApiPermissionForPath/);
    assert.match(api, /HasViewPermission/);
    assert.match(api, /if \(lifecycle && scope\.DashboardType != "TOP"\)/);
    assert.match(sqlPolicy, /perspective == "GROUP" \|\| perspective == "RLM"/);
    assert.match(sqlPolicy, /StartDate = period.Start/);
    assert.match(sqlPolicy, /EndDate = period.End/);
});

test('GROUP and RLM match TOP ten-card grid without requesting TOP-only lifecycle API', () => {
    const partial = read('AIS/Views/ExecutiveOverview/_Sections.cshtml');
    const css = read('AIS/wwwroot/css/executive-dashboard-parents.css');
    const js = read('AIS/wwwroot/js/executive-dashboard.js');
    const top = read('AIS/Views/ExecutiveOverview/TOP.cshtml');
    const group = read('AIS/Views/ExecutiveOverview/_ManagementAudit.cshtml');
    assert.match(partial, /managementNine/);
    assert.match(partial, /Model\.SelectedView\.UseLifecycleKpis \|\| managementNine/);
    assert.match(partial, /data-lifecycle="@\(Model\.SelectedView\.UseLifecycleKpis \? "true" : "false"\)"/);
    assert.match(css, /exec\.exec-management \.exec-lifecycle-kpis/);
    assert.match(css, /exec\.exec-regional \.exec-lifecycle-kpis/);
    assert.match(js, /app\.dataset\.perspective === 'GROUP' && app\.dataset\.view === 'management'/);
    assert.match(js, /app\.dataset\.perspective === 'RLM' && app\.dataset\.view === 'glance'/);
    assert.match(js, /lifecycleKpis\(data\.lifecycle\)/);
    assert.match(js, /if \(lifecycle && datasets\.GetKpiLifecycle\) lifecycleKpis/);
    assert.match(top, /Model\.SelectedView\.Partial/);
    assert.match(group, /<partial name="_Sections" model="Model"/);
    assert.doesNotMatch(group, /data-mgmt-ring/);
});

test('Ten stage labels are shared across perspectives and source quality flags remain visible', () => {
    const js = read('AIS/wwwroot/js/executive-dashboard.js');
    const requested = [
        'Opening Paras','Observation Raised by Audit','On Spot Rectification',
        'Para Added in Final Report','Post Compliance Settlement',
        'Outstanding Para','High Risk','Medium Risk',
        'Low Risk'
    ];
    for (const label of requested) assert.ok(js.includes(label), label);
    assert.match(js, /data\.historicalOpeningVerified/);
    assert.match(js, /data\.openingEstimated/);
    assert.match(js, /data\.closingAsofVerified/);
    assert.match(js, /data\.closingEstimated/);
    assert.match(js, /Historical value unavailable/);
});

test('Old RLM position and standalone GROUP/RLM high-risk bookmarks redirect only after authorization', () => {
    const route = read('AIS/Controllers/ExecutiveOverviewController.cs');
    assert.match(route, /scope\.DashboardType == "RLM" && view == "position"/);
    assert.match(route, /RedirectToAction\(nameof\(RLM\), new \{ view = "glance"/);
    assert.match(route, /scope\.DashboardType == "GROUP" \|\| scope\.DashboardType == "RLM"\) && view == "risk"/);
    assert.match(route, /RedirectToAction\(scope\.DashboardType, new \{ view = "audit-paras"/);
    assert.ok(route.indexOf('LogDenial("PagePermission"') < route.indexOf('scope.DashboardType == "RLM" && view == "position"'));
});

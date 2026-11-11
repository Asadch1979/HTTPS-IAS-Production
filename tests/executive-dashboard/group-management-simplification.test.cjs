const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = file => fs.readFileSync(file, 'utf8');

test('GROUP keeps the management view as Audit Para Position and CCM Reports, with no duplicate position tab', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const group = read('AIS/Views/ExecutiveOverview/GROUP.cshtml');
    const route = read('AIS/Controllers/ExecutiveOverviewController.cs');
    assert.match(views, /if \(perspective == "GROUP"\)/);
    assert.match(views, /views\.RemoveAll\(v => v\.Id == "position"\)/);
    assert.match(views, /views\.Single\(v => v\.Id == "reports"\)\.Title = "CCM Reports"/);
    assert.match(views, /perspective == "GROUP" \? "Audit Para Position" : "ZTBL \| Management Audit Dashboard"/);
    assert.match(route, /scope\.DashboardType == "GROUP" && view == "position"/);
    assert.match(route, /RedirectToAction\(nameof\(GROUP\), new \{ view = "management"/);
    assert.match(group, /Model\.SelectedView\.Id == "reports"/);
});

test('GROUP settlement attribution is Super User only in the UI AND the ordinary-user API payload', () => {
    const client = read('AIS/wwwroot/js/executive-reports.js');
    const api = read('AIS/Controllers/ExecutiveReportsController.cs');
    const report = read('AIS/Views/ExecutiveOverview/_Reports.cshtml');
    assert.match(report, /data-verified-super-user="@\(Model\.IsSuperUser \? "true" : "false"\)"/);
    assert.match(client, /!snap && !cco && \(!group \|\| verifiedSuperUser\)/);
    assert.match(api, /scope\.DashboardType == "GROUP" && reportId == "E" && !superUser/);
    const begin = api.indexOf('var publicRows = data.Rows.Select');
    const end = api.indexOf('var publicData = new', begin);
    assert.ok(begin > 0 && end > begin);
    const projection = api.slice(begin, end);
    for (const restricted of ['SettledByAudit', 'SettledByCompliance', 'SettledByUnknown',
        'ReconciliationDifference', 'ReconciliationAdjustment', 'ReopeningEvents',
        'ResettlementEvents', 'InitiallyClosedAdditions', 'AuditorAssignmentMismatch'])
        assert.doesNotMatch(projection, new RegExp('row\\.' + restricted));
    for (const required of ['OpeningBalance', 'ParasAdded', 'TotalParas', 'SettledParas',
        'OutstandingParas', 'SettlementPercentage'])
        assert.match(projection, new RegExp('row\\.' + required));
});

test('GROUP report removes redundant labels and notes without removing report records or failures', () => {
    const js = read('AIS/wwwroot/js/executive-reports.js');
    const report = read('AIS/Views/ExecutiveOverview/_Reports.cshtml');
    const dashboard = read('AIS/wwwroot/js/executive-dashboard.js');
    assert.match(report, /Model\.DashboardType == "GROUP" && Model\.SelectedView\.Reports\.Length == 1/);
    assert.match(report, /hidden="@\(Model\.DashboardType == "GROUP"\)"/);
    assert.match(js, /const displayTitle = group \? 'CCM Reports'/);
    assert.match(js, /if \(!cco && !group\)/);
    assert.match(js, /if \(group\) \$\('status'\)\.hidden = true/);
    assert.match(js, /\$\('status'\)\.hidden = false/);
    assert.match(dashboard, /if \(app\.dataset\.perspective === 'GROUP'\)/);
    assert.match(dashboard, /\$\('status'\)\.hidden = false/);
});

test('existing CCO, CIA, RLM and frozen Oracle dashboard implementation are untouched', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    assert.match(views, /"RLM"/);
    assert.match(views, /"CCO"/);
    assert.match(views, /"CIA"/);
    assert.match(views, /View\("position", "Audit Para Position"/);
    assert.match(views, /if \(perspective == "TOP"\)/);
    assert.match(read('AIS/Views/ExecutiveOverview/GROUP.cshtml'), /executive-role-reporting\.js/);
});

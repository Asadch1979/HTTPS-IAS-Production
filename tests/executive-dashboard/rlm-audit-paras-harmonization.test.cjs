const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = path => fs.readFileSync(path, 'utf8');

test('RLM removes management view and uses dated five-card At a Glance with risk distribution', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const reader = read('AIS/DBConnection.ComplianceDashboard.cs');
    const client = read('AIS/wwwroot/js/executive-dashboard.js');
    const route = read('AIS/Controllers/ExecutiveOverviewController.cs');
    const rlm = read('AIS/Views/ExecutiveOverview/RLM.cshtml');
    assert.match(views, /"RLM" => new\[\] \{ "openingCases", "paraAdded", "settledTotal", "openParas", "settlementRatio" \}/);
    assert.match(views, /perspective == "RLM" \? new\[\] \{ "risk" \}/);
    assert.doesNotMatch(views, /views\.Insert\(0, View\("management", perspective == "GROUP"/);
    assert.match(route, /scope\.DashboardType == "RLM" && view == "management"/);
    assert.match(route, /if \(scope\.DashboardType == "RLM"\) views\.RemoveAll\(v => v\.Id == "management"\)/);
    assert.match(reader, /scope\.DashboardType == "CCO" \|\| scope\.DashboardType == "RLM" \? opening/);
    assert.match(reader, /scope\.DashboardType == "CCO" \|\| scope\.DashboardType == "RLM" \? added/);
    for (const label of ['Opening Balance', 'Para Added', 'Closing Balance', 'Selected-Period Settlement Ratio'])
        assert.ok(client.includes(label), label);
    assert.doesNotMatch(rlm, /Model\.SelectedView\.Id == "management"/);
});

test('All authorized executive parents have Audit Para’s with risk graph before bounded High-Risk register', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const cco = read('AIS/Models/CcoDashboardPresentation.cs');
    const page = read('AIS/Views/ExecutiveOverview/_AuditParas.cshtml');
    const chart = read('AIS/Views/ExecutiveOverview/_Sections.cshtml');
    const client = read('AIS/wwwroot/js/executive-dashboard.js');
    assert.match(views, /View\("audit-paras", "Audit Para’s", "_AuditParas"/);
    assert.match(views, /Array\.Empty<string>\(\), new\[\] \{ "risk", "critical" \}/);
    assert.match(cco, /View\("audit-paras", "Audit Para’s", Array\.Empty<string>\(\), new\[\]\{"risk","critical"\}\)/);
    assert.match(cco, /"critical-observations" => "audit-paras"/);
    assert.match(chart, /Model\.SelectedView\.Sections\.Where\(s => s != "critical"\)/);
    assert.match(page, /<partial name="_Sections"/);
    assert.match(client, /datasets\.GetKpiSummary\?\.highRiskParas \?\? criticalRows\.length/);
    assert.match(client, /app\.dataset\.view === 'audit-paras'/);
});

test('High Risk KPI is never duplicated by Critical Items, and Aging never loads the critical list', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const cco = read('AIS/Models/CcoDashboardPresentation.cs');
    const critical = read('AIS/Views/ExecutiveOverview/_Critical.cshtml');
    const js = read('AIS/wwwroot/js/executive-dashboard.js');
    assert.match(views, /View\("risk", "High Risk"/);
    assert.match(views, /new\[\] \{ "highRiskParas" \}, new\[\] \{ "critical" \}/);
    assert.match(views, /View\("aging", "Aging & Compliance"/);
    assert.match(views, /new\[\] \{ "aging", "cycle" \}/);
    assert.doesNotMatch(views, /"highRiskParas", "criticalItems"/);
    assert.match(critical, /@if \(isCco\)/);
    assert.match(critical, /<h2>@title<\/h2>/);
    assert.match(js, /\$\('critical-risk'\)\?\.value \|\| ''/);
    assert.match(js, /if \(filter\) \{/);
    assert.match(cco, /new\[\]\{"risk","critical"\}/);
});

test('CIA Command Center and Snap dedicated readers remain isolated and accessible', () => {
    const cia = read('AIS/Views/ExecutiveOverview/CIA.cshtml');
    const route = read('AIS/Controllers/ExecutiveOverviewController.cs');
    const snap = read('AIS/Models/AuditParaSnapData.cs');
    const snapJs = read('AIS/wwwroot/js/audit-para-snap.js');
    assert.match(cia, /Model\.SelectedView\.Id == "command"/);
    assert.match(cia, /_CiaCommandCenter/);
    assert.match(cia, /"audit-paras"/);
    assert.match(cia, /_AuditParas/);
    assert.match(route, /views\.RemoveAll\(v => v\.Id != "command" && v\.Id != "audit-paras"\)/);
    assert.match(snap, /Definition\("risk","Audit Para’s"/);
    assert.match(snapJs, /document\.getElementById\('exec-critical-risk'\)\?\.value \|\| ''/);
});

test('CCO preserves its server-side ALL, High, Medium, Low, Unrated risk paging', () => {
    const controller = read('AIS/Controllers/ExecutiveDashboardController.cs');
    const critical = read('AIS/Views/ExecutiveOverview/_Critical.cshtml');
    const js = read('AIS/wwwroot/js/executive-dashboard.js');
    assert.match(controller, /scope\.DashboardType != "CCO" \|\| f\.View != "audit-paras"/);
    assert.match(js, /actionQuery\.set\('risk', \$\('critical-risk'\)\.value\)/);
    for (const risk of ['HIGH','MEDIUM','LOW','UNRATED','ALL'])
        assert.ok(critical.includes('<option value="' + risk + '"'));
    assert.match(controller, /database\.GetCcoObservationPage/);
});

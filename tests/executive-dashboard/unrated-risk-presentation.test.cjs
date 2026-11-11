'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = p => fs.readFileSync(p, 'utf8');

test('All executive risk charts display rated categories only, without changing the underlying source', () => {
    const main = read('AIS/wwwroot/js/executive-dashboard.js');
    const snap = read('AIS/wwwroot/js/audit-para-snap.js');
    assert.match(main, /rows = rows\.filter\(row => \[1,2,3\]\.includes\(Number\(row\.riskId\)\)/);
    assert.match(snap, /row\.bucketCode === '0' \|\| row\.bucketName === 'Unrated'/);
    assert.match(read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql'), /AS unrated/);
    assert.match(read('AIS/Models/RoleDashboardData.cs'), /public long Unrated \{ get; set; \}/);
});

test('RLM heatmap and charts present High Medium Low only; Oracle reconciliation remains intact', () => {
    const panel = read('AIS/Views/ExecutiveOverview/_RoleReporting.cshtml');
    const js = read('AIS/wwwroot/js/executive-role-reporting.js');
    const css = read('AIS/wwwroot/css/regional-audit-pulse.css');
    for (const label of ['High','Medium','Low']) assert.ok(panel.includes('>' + label + '</span>'), label);
    assert.doesNotMatch(panel, /Unrated|unrated/);
    assert.doesNotMatch(js, /name: 'Unrated'|role-risk-unrated/);
    assert.match(js, /count \/ open \* 100/);
    assert.match(js, /\+ Number\(item\.unrated \|\| 0\)/);
    assert.doesNotMatch(css, /role-risk-unrated|data-risk=unrated/);
    assert.match(js, /Risk classification totals require reconciliation/);
    assert.match(js, /mode === 'regional' \? 8 : 6/);
});

test('CCO Audit Para’s risk selector offers only High Medium Low; existing authorized backend contract not weakened', () => {
    const partial = read('AIS/Views/ExecutiveOverview/_Critical.cshtml');
    const validator = read('AIS/Models/CcoObservationFilter.cs');
    for (const risk of ['HIGH','MEDIUM','LOW'])
        assert.ok(partial.includes('option value="' + risk + '"'));
    assert.doesNotMatch(partial, /option value="UNRATED"|option value="ALL"/);
    assert.match(validator, /Risk != "HIGH"/);
    assert.match(validator, /RatedRiskId/);
});

test('Executive comparison, role followups and Team Lead do not label records as Unrated risk', () => {
    const comparison = read('AIS/wwwroot/js/executive-entity-comparison.js');
    const razor = read('AIS/Views/ExecutiveOverview/_EntityComparison.cshtml');
    const role = read('AIS/wwwroot/js/executive-role-reporting.js');
    const teamLead = read('AIS/Views/Shared/_TeamLeadOracle.cshtml');
    assert.doesNotMatch(comparison, /'unrated'|Unrated/);
    assert.doesNotMatch(razor, /Unrated/);
    assert.doesNotMatch(role, /\? 'Unrated'/);
    assert.doesNotMatch(teamLead, /"Unrated"/);
    assert.match(comparison, /'unknown'/);
});

test('Reports B/C and PDFs contain no visible Unrated Risk column, preserving model data for audit reconciliation', () => {
    const reports = read('AIS/wwwroot/js/executive-reports.js');
    const pdf = read('AIS/Services/ExecutiveReportsPdfGenerator.cs');
    const dto = read('AIS/Models/ExecutiveReportModels.cs');
    assert.doesNotMatch(reports, /Unrated risk|unratedRisk/);
    assert.doesNotMatch(pdf, /Unrated risk|"unrated"/);
    assert.match(dto, /public long UnratedRisk \{ get; set; \}/);
    for(const key of ['highRisk','mediumRisk','lowRisk']) assert.ok(reports.includes("'" + key + "'"));
});

test('Original RLM Regional Pulse visibility is GM Office 21 OR Region 5, never an unverified root', () => {
    const viewController = read('AIS/Controllers/ExecutiveOverviewController.cs');
    const api = read('AIS/Controllers/ExecutiveDashboardController.cs');
    assert.match(viewController, /root\?\.EntityTypeId == 21 \|\| root\?\.EntityTypeId == 5/);
    assert.match(api, /root\?\.EntityTypeId == 21 \|\| root\?\.EntityTypeId == 5/);
    assert.match(api, /new\[\] \{ 21, 5 \}\.Contains\(database\.GetExecutiveDashboardRoot\(scope\)\?\.EntityTypeId \?\? -1\)/);
});

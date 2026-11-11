'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = path => fs.readFileSync(path, 'utf8');

test('Regional Pulse is not a second RLM tab, and only validated GM Office roots may embed it', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const controller = read('AIS/Controllers/ExecutiveOverviewController.cs');
    const model = read('AIS/Models/ExecutiveDashboardPageModel.cs');
    const rlm = read('AIS/Views/ExecutiveOverview/RLM.cshtml');
    const glance = read('AIS/Views/ExecutiveOverview/_Glance.cshtml');
    const regional = read('AIS/Views/ExecutiveOverview/_RegionalPulse.cshtml');
    assert.doesNotMatch(views, /View\("regional", "Regional Audit Pulse"/);
    assert.match(controller, /gmOfficeSelected = scope\.DashboardType == "RLM" && root\?\.EntityTypeId == 21/);
    assert.doesNotMatch(controller, /gmOfficeSelected = [^;]*EntityTypeId == 5/);
    assert.match(controller, /if \(view == "regional"\)/);
    assert.match(controller, /if \(!gmOfficeSelected\).*StatusCode\(403\)/);
    assert.match(controller, /RedirectToAction\(nameof\(RLM\), new \{ view = "glance"/);
    assert.match(controller, /ShowRegionalPulse = gmOfficeSelected && view == "glance"/);
    assert.match(model, /public bool ShowRegionalPulse \{ get; set; \}/);
    assert.match(glance, /@if \(Model\.ShowRegionalPulse\)/);
    assert.match(glance, /<partial name="_RegionalPulse"/);
    assert.match(regional, /Model\.ShowRegionalPulse && Model\.SelectedView\.Id == "glance"/);
    assert.match(rlm, /@if \(Model\.ShowRegionalPulse\).*executive-role-reporting\.js/);
    assert.doesNotMatch(rlm, /SelectedView\.Id == "regional"/);
});

test('The GM-only RLM glance retains nine lifecycle cards and has the existing Pulse risk/aging/cycle/trend charts only at GM root', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const controller = read('AIS/Controllers/ExecutiveOverviewController.cs');
    const glance = read('AIS/Views/ExecutiveOverview/_Glance.cshtml');
    const regional = read('AIS/Views/ExecutiveOverview/_RegionalPulse.cshtml');
    assert.match(views, /"RLM" => new\[\] \{ "openingOpenParas", "issueIdentified", "issueRectified"/);
    assert.match(controller, /if \(gmOfficeSelected && view == "glance"\)\s+selectedView\.Sections = new\[\] \{ "risk", "aging", "cycle", "trend" \}/);
    assert.match(glance, /<partial name="_Sections"/);
    assert.match(regional, /<partial name="_RoleReporting"/);
    assert.doesNotMatch(regional, /<partial name="_Sections"/);
});

test('Role data endpoint requires GM type 21 and exact RLM glance, leaving GROUP management access intact', () => {
    const controller = read('AIS/Controllers/ExecutiveDashboardController.cs');
    assert.match(controller, /isGmPulse = scope\.DashboardType == "RLM" && root\?\.EntityTypeId == 21/);
    assert.match(controller, /gmGlanceView = isGmPulse && f\.View == "glance"/);
    assert.match(controller, /managementView = isManagement && f\.View == "management"/);
    assert.match(controller, /requiredPerspective == "regional" && !gmGlanceView/);
    assert.match(controller, /!managementView && !gmGlanceView/);
    assert.match(controller, /GetRoleEntityHeatmap/);
    assert.match(controller, /GetRoleFollowups/);
    assert.match(controller, /HasApiPermissionForPath/);
    assert.match(controller, /HasViewPermission/);
});

test('Only RLM glance declares the existing GM Pulse role datasets; TOP and GROUP do not gain them', () => {
    const views = read('AIS/Models/ExecutiveDashboardViews.cs');
    const api = read('AIS/Controllers/ExecutiveDashboardController.cs');
    assert.match(views, /public bool EnableGmPulseRoleDatasets \{ get; set; \}/);
    assert.match(views, /Id == "glance" && EnableGmPulseRoleDatasets/);
    assert.match(views, /views\.Single\(v => v\.Id == "glance"\)\.EnableGmPulseRoleDatasets = true/);
    assert.match(views, /new\[\] \{ "GetRoleEntityHeatmap", "GetRoleFollowups" \}/);
    assert.match(api, /definition\.RoleDatasets, action/);
    assert.match(api, /requiredPerspective == "regional" && !gmGlanceView/);
});

test('Existing Oracle reader already returns all four risk counts for scoped branches', () => {
    const adapter = read('AIS/DBConnection.RoleDashboard.cs');
    const model = read('AIS/Models/RoleDashboardData.cs');
    const sql = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
    for(const field of ['HIGH_RISK', 'MEDIUM_RISK', 'LOW_RISK', 'UNRATED'])
        assert.ok(adapter.includes('RoleLong(r, "' + field + '")'));
    for(const field of ['HighRisk','MediumRisk','LowRisk','Unrated','OpenParas'])
        assert.ok(model.includes('public long ' + field + ' { get; set; }'));
    for(const field of ['medium_risk','low_risk','unrated'])
        assert.match(sql, new RegExp('AS ' + field));
    assert.match(adapter, /ReadRoleDashboardCursor\("PKG_EXEC_DASHBOARD\.P_GET_ROLE_ENTITY_HEATMAP"/);
});

test('Heatmap shows each category percentage of branch open paras, with readable visual and table columns', () => {
    const role = read('AIS/Views/ExecutiveOverview/_RoleReporting.cshtml');
    const js = read('AIS/wwwroot/js/executive-role-reporting.js');
    const css = read('AIS/wwwroot/css/regional-audit-pulse.css');
    assert.match(role, /Medium-risk share/);
    assert.match(role, /Low-risk share/);
    assert.match(role, /Unrated share/);
    assert.match(role, /@\(regional \? "High-risk share"/);
    assert.match(js, /const share = \(count, open\) => open > 0 \? Math\.max\(0, count \/ open \* 100\) : 0/);
    for(const field of ['highRisk', 'mediumRisk', 'lowRisk', 'unrated'])
        assert.ok(js.includes('item.'+field));
    assert.match(js, /const mix = riskMix\(item\)/);
    assert.match(js, /stack\.setAttribute\('role', 'img'\)/);
    assert.match(js, /for \(const part of mix\)/);
    assert.match(js, /empty\(tbody, rows, mode === 'regional' \? 9 : 6\)/);
    assert.match(js, /Risk classification totals require reconciliation/);
    for(const color of ['high', 'medium', 'low', 'unrated'])
        assert.match(css, new RegExp('\\.role-risk-' + color));
});

test('Source presentation retains the 36-tile cap and the 250-row authorized reader limit', () => {
    const js = read('AIS/wwwroot/js/executive-role-reporting.js');
    const db = read('AIS/DBConnection.RoleDashboard.cs');
    assert.match(js, /\.slice\(0, 36\)/);
    assert.match(js, /GetRoleEntityHeatmap' : 'GetRoleUnitComparison'/);
    assert.match(db, /int maxRows = 250/);
    assert.match(db, /ReadType\(6\)/);
    assert.match(db, /ReadType\(28\)/);
});

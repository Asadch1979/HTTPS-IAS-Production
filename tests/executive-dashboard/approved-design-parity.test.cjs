'use strict';

// Source-level regression checks for approved visual composition. These tests
// deliberately do not assert live KPI values or imply Oracle deployment.
// Run: node --test tests/executive-dashboard/approved-design-parity.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const {readFileSync} = require('node:fs');
const {join} = require('node:path');
const {Script} = require('node:vm');

const root = join(__dirname, '..', '..');
const read = path => readFileSync(join(root, 'AIS', path), 'utf8');

test('TeamMate workbench preserves actual assignments and all four workflow lanes', () => {
    const razor = read('Views/Shared/EngagementWorkbench.cshtml');
    assert.match(razor,/My Engagement Workbench/);
    assert.match(razor,/new\[\] \{ "Planning", "Fieldwork", "Review", "Reporting" \}/);
    assert.match(razor,/stage == "Review" \|\| stage == "Reporting"/);
    assert.match(razor,/Stage verification pending/);
    assert.match(razor,/Url\.Action\(action, controller, new \{ engId = engagement\.EngagementId \}\)/);
    assert.match(razor,/Model\.TeamLeadSnapshot/);
    assert.match(read('wwwroot/css/engagement-workbench.css'),/\.wb-redesigned \.wb-stage-ribbon/);
});

test('Management and regional approved layouts stay in their respective authorized views', () => {
    const management = read('Views/ExecutiveOverview/_ManagementAudit.cshtml');
    const regional = read('Views/ExecutiveOverview/_RegionalPulse.cshtml');
    const role = read('Views/ExecutiveOverview/_RoleReporting.cshtml');
    assert.match(read('Views/ExecutiveOverview/GROUP.cshtml'),/Management Audit Dashboard/);
    assert.doesNotMatch(management,/mgmt-hero/);
    assert.match(management,/data-mgmt-ring="settlement"/);
    assert.match(management,/data-mgmt-ring="risk"/);
    assert.match(management,/data-mgmt-ring="aging"/);
    assert.match(management,/<partial name="_RoleReporting" model="Model" \/>/);
    assert.doesNotMatch(management,/Oracle activation pending|mgmt-pending-bars/);
    assert.match(read('Views/ExecutiveOverview/RLM.cshtml'),/Regional Audit Pulse/);
    assert.doesNotMatch(regional,/regional-hero/);
    assert.match(regional,/<partial name="_RoleReporting" model="Model" \/>/);
    assert.match(role,/data-region-heatmap/);
    assert.match(role,/data-management-bars/);
    assert.match(role,/data-role-followup-rows/);
});

test('All ten CIA issue categories render even without the Oracle feature flag', () => {
    const razor = read('Views/ExecutiveOverview/_CiaCommandCenter.cshtml');
    const expected = [
        'AML_CFT','ACCOUNTS','LOAN_ADVANCES','CASH_MANAGEMENT','RECORD_MANAGEMENT',
        'BANKING_OPERATIONS','BRANCH_GENERAL_CONTROLS','ACCOUNTING_REPORTING_RECON',
        'MANAGEMENT_AUDIT','IS_AUDIT'
    ];
    for (const category of expected) assert.ok(razor.includes('Code = "' + category + '"'), category);
    assert.equal((razor.match(/new \{ Code = "/g) || []).length, 10);
    assert.match(razor,/data-cia-category="@category\.Code"/);
    assert.match(razor,/data-cia-category-count>N\/A/);
    assert.match(razor,/data-cia-nature-ring/);
    const js = read('wwwroot/js/cia-command-center.js');
    assert.match(js,/mapped \+ unmapped === identified/);
    assert.match(js,/known\.get\(item\.categoryCode\)/);
    assert.match(js,/Issue classification unavailable/);
});

test('Charts are purely presentation over authorized IAS responses', () => {
    const main = read('wwwroot/js/executive-dashboard.js');
    const visuals = read('wwwroot/js/executive-role-visuals.js');
    const role = read('wwwroot/js/executive-role-reporting.js');
    assert.match(main,/ias:executive-data/);
    assert.match(visuals,/event\.detail\?\.datasets\?\.GetKpiSummary/);
    assert.doesNotMatch(visuals,/fetch\s*\(/);
    assert.match(role,/GetRoleEntityHeatmap/);
    assert.match(role,/GetRoleUnitComparison/);
    assert.match(role,/GetRoleFollowups/);
    assert.match(role,/share >= 20.*share >= 10/);
    assert.match(role,/rows\.slice\(\)\.sort/);
});

test('Every changed JavaScript file parses', () => {
    for (const path of [
        'wwwroot/js/executive-dashboard.js',
        'wwwroot/js/executive-role-reporting.js',
        'wwwroot/js/executive-role-visuals.js',
        'wwwroot/js/cia-command-center.js'
    ]) assert.doesNotThrow(() => new Script(read(path), {filename:path}));
});

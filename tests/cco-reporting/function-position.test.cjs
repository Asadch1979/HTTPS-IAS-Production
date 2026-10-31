'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = p => fs.readFileSync(p, 'utf8');
const report = read('AIS/Docs/sql/PKG_EXEC_REPORTS.sql');

test('the existing authenticated Report E SQL retains verified Group/Division mapping', () => {
  assert.match(report, /group_report_candidates AS/);
  assert.match(report, /FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r/);
  assert.match(report, /group_report_map AS \(/);
  assert.match(report, /LEFT JOIN group_report_map gr/);
  assert.match(report, /WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1/);
  assert.match(report, /ELSE 'Unmapped \/ conflicting Division'/);
  assert.match(report, /P_REPORT_ID = 'E' AND s.audit_type = 'D'/);
  assert.match(report, /GROUPING SETS/);
  assert.match(report, /PROCEDURE P_MANAGEMENT_SETTLEMENT/);
  assert.match(report, /OPEN_REPORT\('E', P_DASHBOARD_TYPE/);
});

test('Function Wise Position uses canonical authorized Report E, never the separate scope package', () => {
  const reader = read('AIS/DBConnection.CcoFunctionPosition.cs');
  const authorized = read('AIS/DBConnection.ExecutiveDashboardReports.cs');
  assert.match(reader, /GetExecutiveReportE\(scope, reportFilters\)/);
  assert.match(reader, /ValidateCcoOrganizationContext\(scope\)/);
  assert.match(reader, /ComplianceDashboardPolicy\.Validate\(reportFilters\)/);
  assert.match(reader, /report\.DashboardType != "CCO"/);
  assert.match(reader, /report\.Rows\.Count > filters\.MaxRows/);
  assert.match(reader, /DivisionId = source\.ItemId/);
  assert.match(reader, /DivisionName = source\.ItemName/);
  assert.doesNotMatch(reader, /PKG_CCO_SCOPE\.P_|PKG_CCO_FUNCTION_POSITION\.P_|V_CCO_AUTHORIZED_ENTITIES/);
  assert.doesNotMatch(reader, /V_EXEC_REPORT_SOURCE|GetCcoOrganizationRows/);
  assert.match(authorized, /PKG_EXEC_REPORTS\.P_MANAGEMENT_SETTLEMENT/);
  assert.match(authorized, /ValidateExecutiveDashboardSession\(scope\)/);
  assert.match(authorized, /GuardAgainstDynamicSql\(command\)/);
  assert.match(authorized, /command\.BindByName = true/);
});

test('the exact CCO endpoint keeps its role, page, API and exception visibility gates', () => {
  const controller = read('AIS/Controllers/CcoComplianceController.cs');
  const auth = read('AIS/DBConnection.CcoOrganizationReporting.cs');
  const visibility = read('AIS/Models/CcoReportVisibility.cs');
  assert.match(controller, /ResolvePageId\("\/ExecutiveOverview\/CCO"\)!=1000302/);
  assert.match(controller, /HasApiPermissionForPath/);
  assert.match(controller, /CcoReportVisibility\.FunctionRows\(database\.GetCcoFunctionPosition\(scope,filters\),super\)/);
  assert.match(auth, /user\.UserRoleID!=38/);
  assert.match(auth, /CreateSessionHandler\(\)\.IsSuperUser\(\)/);
  assert.match(visibility, /!string\.Equals\(row\.RowKind, "SUBTOTAL"/);
  assert.match(visibility, /row\.GroupId\.HasValue && row\.DivisionId\.HasValue/);
});

test('the Function Wise Position client safely renders the same eight columns', () => {
  const partial = read('AIS/Views/ExecutiveOverview/_CcoFunctionPosition.cshtml');
  const js = read('AIS/wwwroot/js/cco-function-position.js');
  assert.match(partial, /GetFunctionPosition/);
  assert.match(js, /credentials: 'same-origin'/);
  assert.match(js, /response\.ok/);
  assert.match(js, /textContent/);
  assert.doesNotMatch(js, /innerHTML|eval\(/);
  for (const col of ['groupName', 'divisionName', 'openingBalance', 'parasAdded',
                     'totalParas', 'settledParas', 'outstandingParas', 'settlementPercentage'])
    assert.ok(js.includes("'" + col + "'"), col);
});

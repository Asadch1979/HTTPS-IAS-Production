'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = file => fs.readFileSync(file, 'utf8');

test('Function Wise Position diagnoses authorized management reporting errors without scope package calls', () => {
  const c = read('AIS/DBConnection.CcoFunctionPosition.cs');
  for (const name of ['GetExecutiveReportE(scope, reportFilters)',
    'execute_authorized_management_report_e', 'validate_management_report_e_rows',
    'revalidate_ias_context', 'CcoFunctionPositionReadException']) {
    assert.ok(c.includes(name), name);
  }
  assert.doesNotMatch(c, /PKG_CCO_SCOPE\.P_|PKG_CCO_FUNCTION_POSITION\.P_/);
});

test('Controller reports database failures only to server logs, preserving exact grants', () => {
  const c = read('AIS/Controllers/CcoComplianceController.cs');
  assert.match(c, /catch \(CcoFunctionPositionReadException ex\)/);
  assert.match(c, /ex\.Stage, ex\.OracleNumber/);
  assert.match(c, /outcome="database_failure"/);
  assert.match(c, /HasApiPermissionForPath/);
  assert.match(c, /1000302/);
});

test('Browser shows only correlation, not Oracle or IAS session details', () => {
  const c = read('AIS/wwwroot/js/cco-function-position.js');
  assert.match(c, /rejected\?\.correlationId/);
  assert.match(c, /Request reference:/);
  assert.doesNotMatch(c, /innerHTML/);
});

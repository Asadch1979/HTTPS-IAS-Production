const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = file => fs.readFileSync(file, 'utf8');

test('Function Wise Position retains its scoped Oracle reader', () => {
  const c = read('AIS/DBConnection.CcoFunctionPosition.cs');
  for (const name of ['PKG_CCO_SCOPE.P_BEGIN_SCOPE', 'PKG_CCO_SCOPE.P_ASSERT_SCOPE',
    'PKG_CCO_FUNCTION_POSITION.P_GET_POSITION', 'PKG_CCO_SCOPE.P_END_SCOPE']) {
    assert.ok(c.includes(name), name);
  }
  assert.match(c, /CcoFunctionPositionReadException/);
  assert.match(c, /begin_trusted_cco_scope/);
  assert.match(c, /execute_function_position_package/);
  assert.match(c, /OracleConnection.ClearPool/);
});

test('Controller reports backend failures to server logs only', () => {
  const c = read('AIS/Controllers/CcoComplianceController.cs');
  assert.match(c, /catch \(CcoFunctionPositionReadException ex\)/);
  assert.match(c, /ex.Stage, ex.OracleNumber/);
  assert.match(c, /outcome="database_failure"/);
  assert.match(c, /HasApiPermissionForPath/);
  assert.match(c, /1000302/);
});

test('Response can show request correlation without exposing database details', () => {
  const c = read('AIS/wwwroot/js/cco-function-position.js');
  assert.match(c, /rejected\?\.correlationId/);
  assert.match(c, /Request reference:/);
  assert.doesNotMatch(c, /innerHTML/);
});

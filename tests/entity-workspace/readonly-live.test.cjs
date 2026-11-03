const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const base = path.resolve(__dirname, '../..');
function read(file) { return fs.readFileSync(path.join(base, file), 'utf8'); }
const endpoint = read('AIS/Controllers/AdministrationPanelController.EntityReads.cs');
const db = read('AIS/DBConnection.EntityManagement.cs');
const controller = read('AIS/Controllers/AdministrationPanelController.cs');
const view = read('AIS/Views/AdministrationPanel/Entity_Dashboard.cshtml');
const js = read('AIS/wwwroot/js/administration-entity-dashboard.js');

test('all entity reads are restricted to verified Super User and dashboard page permission', () => {
  assert.match(endpoint, /sessionHandler\.IsSuperUser\(\)/);
  assert.match(endpoint, /HasPageAccess\(user, "\/AdministrationPanel\/Entity_Dashboard"\)/);
  assert.match(controller, /ReadEntityDashboardData\(user, data, entityId/);
  assert.match(controller, /IsSessionRevoked\(user\)/);
  assert.match(view, /data-read-enabled/);
  assert.match(js, /root\.dataset\.readEnabled !== 'true'/);
});

test('package calls are fixed stored procedure names with bind variables and bounded rows', () => {
  for (const operation of ['P_SEARCH_ENTITIES','P_GET_ENTITY_PROFILE',
      'P_PREVIEW_ENTITY_CLOSURE','P_GET_ENTITY_CHANGE_LOG','P_GET_ENTITY_PARA_ARCHIVE'])
    assert.ok(db.includes('PKG_IAS_ENTITY_MGMT.' + operation));
  assert.match(db, /CommandType\.StoredProcedure/);
  assert.match(db, /BindByName = true/);
  assert.match(db, /result\.Rows\.Count >= maxRows/);
  assert.doesNotMatch(db, /ExecuteNonQuery|CommandType\.Text|INSERT INTO|UPDATE T_AUDITEE|DELETE FROM/);
});

test('writes remain disabled and unavailable aggregate cards are not fabricated', () => {
  assert.match(view, /disabled>Execute Closure/);
  assert.match(view, /disabled>Save Changes/);
  assert.match(view, /Approved aggregate reader required/);
  assert.match(endpoint, /data != "search" && data != "summary"/);
  assert.match(js, /method: 'GET'/);
  assert.doesNotMatch(js, /method: 'POST'|method: 'PUT'|method: 'DELETE'/);
});

test('read-only browser renders returned database values as text, not HTML', () => {
  assert.match(js, /td\.textContent =/);
  assert.match(js, /byId\('selectedEntityLabel'\)\.textContent/);
  assert.doesNotMatch(js, /innerHTML\s*=|eval\(/);
});

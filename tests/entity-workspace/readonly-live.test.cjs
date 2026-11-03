const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const base = path.resolve(__dirname, '../..');
const read = file => fs.readFileSync(path.join(base, file), 'utf8');
const endpoint = read('AIS/Controllers/AdministrationPanelController.EntityReads.cs');
const db = read('AIS/DBConnection.EntityManagement.cs');
const controller = read('AIS/Controllers/AdministrationPanelController.cs');
const view = read('AIS/Views/AdministrationPanel/Entity_Dashboard.cshtml');
const js = read('AIS/wwwroot/js/administration-entity-dashboard.js');

test('Super User and the Entity Dashboard page gate every live reader', () => {
    assert.match(endpoint, /sessionHandler\.IsSuperUser\(\)/);
    assert.match(endpoint, /HasPageAccess\(user, "\/AdministrationPanel\/Entity_Dashboard"\)/);
    assert.match(controller, /ReadEntityDashboardData\(user, data, entityId/);
    assert.match(controller, /IsSessionRevoked\(user\)/);
    assert.match(view, /data-read-enabled/);
    assert.match(js, /root\.dataset\.readEnabled !== 'true'/);
});
test('readers use bound Oracle procedures and bounded row counts', () => {
    for (const op of ['P_SEARCH_ENTITIES','P_GET_ENTITY_PROFILE',
        'P_PREVIEW_ENTITY_CLOSURE','P_GET_ENTITY_CHANGE_LOG','P_GET_ENTITY_PARA_ARCHIVE'])
        assert.ok(db.includes('PKG_IAS_ENTITY_MGMT.' + op));
    assert.match(db, /CommandType\.StoredProcedure/);
    assert.match(db, /BindByName = true/);
    assert.match(db, /result\.Rows\.Count >= maxRows/);
    assert.doesNotMatch(db, /ExecuteNonQuery|CommandType\.Text|INSERT INTO|UPDATE T_AUDITEE|DELETE FROM/);
});
test('new mutations are disabled; authorized legacy operations retain their own gate', () => {
    assert.match(view, /disabled>Execute closure/);
    assert.match(view, /disabled>Submit closure/);
    assert.match(view, /disabled>Restore entity/);
    assert.match(view, /Model\.VisibleSteps\.Any/);
    assert.match(view, /data-legacy-step="SETUP_ENGAGEMENT_REVERSAL"/);
    assert.match(js, /root\.dataset\.legacyReadUrl/);
    assert.match(js, /method: 'GET'/);
    assert.doesNotMatch(js, /method: '(POST|PUT|PATCH|DELETE)'/);
});
test('entity data and referenced labels render as text, not interpreted HTML', () => {
    assert.match(js, /el\.textContent = String\(content\)/);
    assert.match(js, /nameCache/);
    assert.match(js, /resolveEntityName/);
    assert.match(js, /types\.get\(String\(id\)\)/);
    assert.match(js, /host\.innerHTML = trustedServerPartial/);
    assert.match(js, /server-authorized IAS Razor partial/);
    assert.doesNotMatch(js, /eval\(/);
});

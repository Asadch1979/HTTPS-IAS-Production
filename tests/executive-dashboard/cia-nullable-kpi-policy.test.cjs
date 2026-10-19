'use strict';
// Source-level regression for production-first CIA evaluation.
// Run: node --test tests/executive-dashboard/cia-nullable-kpi-policy.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname,'../..');
const read = p => fs.readFileSync(path.join(root,p),'utf8');
const reader = read('AIS/DBConnection.CiaCommandCenter.cs');
const controller = read('AIS/Controllers/ExecutiveDashboardController.cs');
const js = read('AIS/wwwroot/js/cia-command-center.js');
const model = read('AIS/Models/CiaCommandCenterData.cs');
const view = read('AIS/Views/ExecutiveOverview/_CiaCommandCenter.cshtml');
const dev = read('AIS/appsettings.Development.json');
const prod = read('AIS/appsettings.Production.json');

test('Oracle workflow KPI NULL preserved as nullable; zero is not conflated with N/A',()=>{
  for(const [field,column] of [
    ['PlannedEntities','PLANNED_ENTITIES'],
    ['AuditedEntities','AUDITED_ENTITIES'],
    ['DelayedAuditReports','DELAYED_AUDIT_REPORTS'],
    ['FraudA1Paras','FRAUD_A1_PARAS'],
    ['AwaitingAuditeeResponse','AWAITING_AUDITEE_RESPONSE'],
    ['TeamsConductingAudit','TEAMS_CONDUCTING_AUDIT']
  ]) {
    assert.ok(reader.includes(field+' = RoleNullableLong(r, "'+column+'")'),field);
    assert.ok(model.includes('long? '+field),field);
  }
  assert.match(reader,/GetCiaCommandKpis\(/);
  assert.doesNotMatch(reader,/_configuration\.GetValue<bool>/);
  assert.match(js,/value == null \? 'N\/A'/);
  assert.match(js,/IAS procedure result · under production validation/);
  assert.doesNotMatch(js,/Verified IAS reporting deadline/);
});
test('both CIA environment switches are obsolete, while server permissions survive',()=>{
  for(const s of [reader,controller,js,view,dev,prod]){
    assert.doesNotMatch(s,/RoleDashboards:CiaWorkflowDefinitionsVerified/);
    assert.doesNotMatch(s,/RoleDashboards:CiaCommandCenterEnabled/);
  }
  for(const s of [dev,prod]){
    const settings = JSON.parse(s);
    assert.equal(settings.RoleDashboards?.CiaWorkflowDefinitionsVerified,undefined);
    assert.equal(settings.RoleDashboards?.CiaCommandCenterEnabled,undefined);
  }
  assert.match(controller,/permissions\.HasApiPermissionForPath/);
  assert.match(controller,/permissions\.HasViewPermission/);
  assert.match(controller,/session\.IsSuperUser\(\)/);
  assert.match(controller,/scope\.DashboardType != "CIA"/);
  assert.match(js,/if \(!selectedPeriod\)/);
});
test('diagnostic notes do not claim business validation from a returned number',()=>{
  assert.match(reader,/pending production validation/);
  assert.match(view,/under production|pending source reconciliation/);
  assert.match(reader,/items\[0\]\.DelayedAuditReports\.HasValue/);
  assert.match(reader,/items\[0\]\.TeamsConductingAudit\.HasValue/);
});

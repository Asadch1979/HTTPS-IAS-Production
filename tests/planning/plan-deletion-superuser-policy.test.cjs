// Static security-contract checks only; must be supplemented by Oracle 19c UAT.
// Run: node --test tests/planning/plan-deletion-superuser-policy.test.cjs
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const source = relativePath => fs.readFileSync(
  path.join(__dirname, '../..', relativePath), 'utf8'
);
const sql = source('AIS/Docs/sql/PKG_HD.sql');
const app = source('AIS/DBConnection.PlanDeletion.cs');
const controller = source('AIS/Controllers/PlanningController.PlanDeletion.cs');
const grants = source('AIS/Docs/sql/audit_plan_deletion/02_authorization_page.sql');

test('all three package procedures take authenticated IAS session and group', () => {
  assert.equal((sql.match(/P_SESSION_ID IN VARCHAR2/g) || []).length, 6);
  assert.equal((sql.match(/P_GROUP_ID IN NUMBER/g) || []).length, 6);
  assert.equal((sql.match(/REQUIRE_PLAN_DELETION_ACCESS\(P_ENT_ID,P_PPNO,P_ROLE_ID,/g) || []).length, 3);
});

test('active role, group, entity and session are verified in the database', () => {
  for (const required of [
    'S.SESSION_ID=P_SESSION_ID', 'S.USER_PP_NUMBER=U.PPNO',
    'S.GROUP_ID=P_GROUP_ID', 'S.ENTITY_ID=P_ENT_ID',
    "S.SESSION_ACTIVE='Y'", 'S.LOGGED_OUT_DATE IS NULL',
    "G.ROLE_ID=P_ROLE_ID", "G.STATUS='Y'",
    'MPG.GROUP_ID=P_GROUP_ID', "MP.STATUS='A'"
  ]) assert.ok(sql.includes(required), required);
  assert.ok(!sql.includes("IF P_ROLE_ID=1 THEN RETURN; END IF;"));
});

test('Super User portfolio elevation follows session verification and retains request separation', () => {
  assert.ok(sql.includes('IF P_ROLE_ID=1 THEN RETURN TRUE; END IF;'));
  assert.ok(sql.includes('P_ROLE_ID=1 OR D.AUDITEDBY=P_ENT_ID'));
  assert.ok(sql.includes('D.REQUESTED_BY <> P_PPNO'));
  assert.ok(sql.includes('IF L_REQUEST.REQUESTED_BY = P_PPNO THEN'));
  assert.ok(sql.includes('RAISE_APPLICATION_ERROR(-20219,'));
  assert.ok(controller.includes('case 20219:'));
});

test('identity comes from server session in both read and write paths', () => {
  assert.ok(app.includes('var user = CreateSessionHandler().GetUser();'));
  assert.equal((app.match(/cmd.Parameters.Add\("P_SESSION_ID"/g) || []).length, 2);
  assert.equal((app.match(/cmd.Parameters.Add\("P_CONTEXT_ID"/g) || []).length, 2);
  assert.ok(!app.includes('P_ROLE_ID", OracleDbType.Int32).Value = 1'));
});

test('Super User page grant follows verified active role 1 group, not group ID 1', () => {
  assert.ok(grants.includes("WHERE ROLE_ID=1 AND STATUS='Y'"));
  assert.ok(grants.includes('GROUP_ID=GR.GROUP_ID'));
  assert.ok(controller.includes('HasPageAccess(user, "/Planning/audit_plan_deletion_authorization")'));
});

test('approval and plan deletion stay atomic and engagement guarded', () => {
  assert.ok(sql.includes('REQUIRE_NO_PLAN_ENGAGEMENT(L_REQUEST.PLAN_ID);'));
  assert.ok(sql.includes('DELETE FROM T_AU_PLAN WHERE ID = L_REQUEST.PLAN_ID'));
  assert.ok(sql.includes("SET STATUS='AUTHORIZED',AUTHORIZED_BY=P_PPNO"));
  assert.ok(sql.includes('ROLLBACK;'));
});

'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const db = fs.readFileSync('AIS/DBConnection.ExecutiveDashboard.cs','utf8');
const sql = fs.readFileSync('AIS/Docs/sql/executive_dashboard_permissions.sql','utf8');
test('Roles 4 and 14 resolve GROUP before optional configuration with restricted assigned root',()=>{
 const block=db.slice(db.indexOf('if (user.UserRoleID == 4 || user.UserRoleID == 14)'),db.indexOf('var mappings = _configuration'));
 assert.match(block,/requested != null && requested != "GROUP"/);
 assert.match(block,/groupRootId.HasValue \|\| user.UserEntityID.GetValueOrDefault\(\) <= 0/);
 assert.match(block,/RequireExecutivePosting\(user.UserEntityID.Value, user.UserRoleID == 4 \? 18 : 3\)/);
 assert.match(block,/new ExecutiveDashboardScope\(user, "GROUP", "\/ExecutiveOverview\/GROUP",\s*user.UserEntityID, user.UserEntityID\)/);
 assert.match(db,/n.EntityTypeId == 18 && !n.HasTypeConflict/);
});
test('verified SQL variable grants page and GET permissions without activation',()=>{
 assert.match(sql,/V_GROUP_ROLE NUMBER := 14/);
 assert.match(sql,/REGISTER_PAGE\(1000301,'GROUP',[^\n]+V_GROUP_ROLE\)/);
 assert.doesNotMatch(sql,/GRANT_API\(14,/);
 assert.equal((sql.match(/GRANT_API\(V_GROUP_ROLE,V_API_ID\)/g)||[]).length,2);
 assert.match(sql,/V_NEW_GRANT_ACTIVE CHAR\(1\) := 'N'/);
});

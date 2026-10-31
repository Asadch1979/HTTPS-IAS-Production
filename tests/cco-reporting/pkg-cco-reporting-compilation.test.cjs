'use strict';
const test=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs');
const read=p=>fs.readFileSync(p,'utf8');

test('retired CCO scope and unused report package scripts cannot be redeployed from canonical SQL',()=>{
 for(const f of ['PKG_CCO_REPORTING.sql','PKG_CCO_FUNCTION_POSITION.sql'])
  assert.equal(fs.existsSync('AIS/Docs/sql/'+f),false,f);
});

test('CCO Function Wise Position uses the supported IAS Report E path',()=>{
 const reader=read('AIS/DBConnection.CcoFunctionPosition.cs');
 const api=read('AIS/Controllers/CcoComplianceController.cs');
 const sql=read('AIS/Docs/sql/PKG_EXEC_REPORTS.sql');
 assert.match(reader,/GetExecutiveReportE\(scope, reportFilters\)/);
 assert.doesNotMatch(reader,/PKG_CCO_SCOPE\.P_|PKG_CCO_FUNCTION_POSITION\.P_/);
 assert.match(api,/HasApiPermissionForPath/);
 assert.match(sql,/PROCEDURE P_MANAGEMENT_SETTLEMENT/);
});

test('legacy Oracle retirement is gated by DBA dependencies and disabled by default',()=>{
 const sql=read('AIS/Docs/sql/CCO_SCOPE_RETIREMENT.sql');
 assert.match(sql,/APPLY_CCO_CLEANUP = NO/);
 assert.match(sql,/FROM dba_dependencies/i);
 assert.match(sql,/FROM dba_context/i);
 assert.match(sql,/FROM dba_source/i);
 assert.match(sql,/FROM dba_synonyms/i);
 assert.match(sql,/DROP PACKAGE/);
 assert.match(sql,/DROP CONTEXT IAS_CCO_CTX/);
 assert.match(sql,/WHENEVER SQLERROR EXIT SQL.SQLCODE/);
});

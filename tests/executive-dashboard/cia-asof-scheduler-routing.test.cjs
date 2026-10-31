'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const read = name => fs.readFileSync(name,'utf8');
const full = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const client = read('AIS/wwwroot/js/cia-command-center.js');
const scheduler = read('AIS/Docs/sql/verify_oracle_ias_db_scheduler_readonly.sql');
const worker = read('AIS/Docs/sql/PKG_IAS_DB_JOB_WORKER.sql');
const installed = read('AIS/Docs/sql/deploy_ias_scheduler_package_only.sql');

function kpi(sql) {
    const b=sql.indexOf('PROCEDURE P_GET_CIA_EXEC_KPIS(',sql.indexOf('CREATE OR REPLACE PACKAGE BODY'));
    const e=sql.indexOf('END P_GET_CIA_EXEC_KPIS;',b);
    assert.ok(b>0&&e>b,'CIA KPI body found');
    return sql.slice(b,e+'END P_GET_CIA_EXEC_KPIS;'.length).replace(/\s+/g,' ').trim();
}

test('CIA selected date drives engagement statuses and all workflow cutoffs', () => {
    const sql=kpi(full);
    assert.match(sql,/NVL\(l\.last_updated_on,l\.created_on\) < V_TO_EXCLUSIVE/);
    assert.match(sql,/position_status IN \(13,14\)/);
    assert.match(sql,/e\.position_status BETWEEN 4 AND 12/);
    assert.match(sql,/TRUNC\(e\.audit_enddate\) \+ 15 < V_TO_EXCLUSIVE/);
    assert.match(sql,/e\.position_status BETWEEN 4 AND 11/);
    assert.match(sql,/o\.entereddate < V_TO_EXCLUSIVE/);
    assert.match(sql,/e\.audit_startdate/);
    assert.doesNotMatch(sql,/WHERE e\.status\b|AND e\.status\b|e\.status BETWEEN/);
    assert.doesNotMatch(sql,/e\.audit_enddate \+ 15 < TRUNC\(SYSDATE\)/);
    assert.match(sql,/V_POSITION AS position_as_on/i);
});
test('CIA browser invalidates old position when the date changes', () => {
    assert.match(client,/getElementById\('cia-position'\)\?\.addEventListener\('change'/);
    assert.match(client,/url\.searchParams\.set\('positionAsOn', position\.value\)/);
    assert.match(client,/history\.replaceState\(null, '', url\)/);
    assert.match(client,/load\(\);/);
    assert.doesNotMatch(client,/Coverage and workflow KPIs are current-state measures/);
});
test('Scheduler preflight is read-only; Oracle worker has a fixed DATABASE allowlist',()=>{
    assert.match(scheduler,/P_CLAIM_DUE_JOB/);
    assert.match(scheduler,/IAS_DATABASE_JOB_POLLER/);
    assert.match(scheduler,/USER_SCHEDULER_JOBS/i);
    assert.match(scheduler,/t_ias_job_execution/i);
    assert.doesNotMatch(scheduler,/^\s*(CREATE|DROP|ALTER|DELETE|UPDATE|INSERT|EXECUTE|GRANT|REVOKE)\s+/im);
    assert.match(worker,/P_EXECUTION_TYPE => 'DATABASE'/);
    assert.match(worker,/PKG_DAILY\.SESSION_KILL_DAY_END/);
    assert.match(installed,/P_EXECUTION_TYPE IN VARCHAR2 DEFAULT 'APPLICATION'/);
});

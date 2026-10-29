const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const draftFiles = ['csp/Views_Execution_manage_observations.js', 'csp/Views_Execution_manage_observations_branches.js',
    'csp/Views_Execution_manage_draft_report_paras.js', 'csp/Views_Execution_manage_draft_report_paras_branch.js',
    'fieldAudit/manageObservationBranchesReplica.js'];
function sandbox(file) {
    const requests = [], alerts = [], modals = [], reloads = [];
    function $(selector) {
        const chain = new Proxy({}, { get: (_, key) => {
            if (key === 'ready') return () => chain;
            if (key === 'val') return () => selector === '#entitySelectField' ? '42' : '7';
            if (key === 'attr') return () => 'true';
            if (key === 'modal') return (...args) => { modals.push(args); return chain; };
            return () => chain;
        }});
        return chain;
    }
    $.trim = x => String(x || '').trim();
    $.ajax = options => requests.push(options);
    $.fn = { DataTable: { isDataTable: () => false } };
    const context = vm.createContext({ $, console, document: { getElementById: () => null },
        window: { addEventListener() {} }, alert: x => alerts.push(x), g_asiBaseURL: '',
        showApiAlert() {}, showApiAlertFromXhr() {}, getErrorReferenceIdFromXhr() {},
        onAlertCallback: fn => reloads.push(fn) });
    vm.runInContext(fs.readFileSync(path.join(root, 'AIS/wwwroot/js', file), 'utf8'), context);
    context.syncEngagementContext = () => 42;
    context.getSelectedEngagementId = () => 42;
    context.isCurrentEngagementTeamLead = () => true;
    return { context, requests, alerts, modals, reloads };
}
for (const file of draftFiles) {
    const { context: c, requests } = sandbox(file);
    c.g_obsId = 17; c.g_newStatusId = 5;
    for (const status of [0, 1, 2, 4, 5, 8, 9, 23]) {
        c.g_currentStatus = status; c.g_obsList = [{ obS_ID: 17, obS_STATUS_ID: status }];
        c.finalCommentsButtonSave();
        c.updateObservationStatus(17, 5, 1);
        assert.equal(requests.length, 0, file + ' must reject status ' + status);
    }
    c.g_currentStatus = 3; c.g_obsList = [{ obS_ID: 17, obS_STATUS_ID: 3 }];
    c.finalCommentsButtonSave(); c.finalCommentsButtonSave();
    assert.equal(requests.length, 1, file + ' suppresses duplicate clicks');
    assert.equal(Number(requests[0].data.EngagementId), 42);
    assert.ok(requests[0].url.endsWith('/ApiCalls/AddObservationToDraft'));
    requests[0].complete();
    c.g_currentStatus = 5; c.g_obsList[0].obS_STATUS_ID = 5;
    c.finalCommentsButtonSave();
    assert.equal(requests.length, 1, file + ' rejects refreshed ineligible status');
    console.log('PASS: status eligibility and duplicate-click checks: ' + file);
}
for (const file of ['csp/Views_Execution_pre_concluding_audit.js', 'csp/Views_Execution_pre_concluding_audit_ho.js']) {
    const { context: c, requests, modals, reloads } = sandbox(file);
    c.g_obsId = 17;
    c.saveObservationGistandRecommendation();
    assert.equal(requests.length, 1);
    assert.equal(modals.length, 0, 'Retain entered text while saving');
    requests[0].success({ Status: false, Message: 'Rejected' });
    assert.equal(modals.length, 0); assert.equal(reloads.length, 0);
    requests[0].success({ status: true, message: 'Saved' });
    assert.equal(modals.length, 1); assert.equal(reloads.length, 1);
    assert.equal(typeof requests[0].error, 'function');
    console.log('PASS: rejected gist saves retain input: ' + file);
}
const scheduler = fs.readFileSync(path.join(root, 'AIS/Docs/sql/PKG_IAS_SCHEDULER.sql'), 'utf8').replace(/\r\n/g, '\n');
const packageOnly = fs.readFileSync(path.join(root, 'AIS/Docs/sql/deploy_ias_scheduler_package_only.sql'), 'utf8').replace(/\r\n/g, '\n');
const spec = 'CREATE OR REPLACE PACKAGE PKG_IAS_SCHEDULER';
assert.equal(scheduler.slice(scheduler.indexOf(spec)).trim(), packageOnly.slice(packageOnly.indexOf(spec)).trim(), 'Scheduler package copies must remain identical');
console.log('PASS: scheduler package-only replacement matches canonical package');

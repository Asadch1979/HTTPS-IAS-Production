const fs = require('fs');
const vm = require('vm');
const assert = require('assert');
const elements = new Map();
let calls = [];
function $(selector) {
    if (!elements.has(selector)) elements.set(selector, {
        classes: new Set(), attributes: {}, value: '',
        addClass(c) { this.classes.add(c); return this; },
        removeClass(c) { this.classes.delete(c); return this; },
        attr(k, v) { if (v === undefined) return this.attributes[k]; this.attributes[k] = v; return this; },
        val(v) { if (v === undefined) return this.value; this.value = v; return this; },
        one(_, callback) { this.callback = callback; return this; },
        modal(action) { if (action === 'hide' && this.callback) { const cb = this.callback; this.callback = null; cb(); } return this; }
    });
    return elements.get(selector);
}
$.ajax = options => calls.push(options);
const context = { $, console, window: { addEventListener() {} },
    document: { readyState: 'loading', getElementById() { return null; } }, alert() {}, g_asiBaseURL: '' };
vm.createContext(context);
vm.runInContext(fs.readFileSync('AIS/wwwroot/js/fieldAudit/manageObservationBranchesReplica.js', 'utf8'), context);
context.preserveTablePosition = () => {};
for (const lead of [true, false]) {
    context.isSelectedEngagementTeamLead = () => lead;
    context.g_currentStatus = 3;
    context.g_riskId = 3;
    $('#fieldAuditManageObservationBranchesReplica').attr('data-can-add-to-draft', 'true');
    context.showActionButtons();
    assert(!$('#addDraftButton_update').classes.has('d-none'), `Assigned ${lead ? 'lead' : 'member'} sees draft`);
    assert.equal($('#settleButton_update').classes.has('d-none'), !lead, 'Settlement remains lead-only');
    context.updateObservationStatus(42, 5, 3);
    $('#draftNoInCommentsBox').val('7');
    $('#commentAreaInCommentsBox').val('Draft remarks');
    context.finalCommentsButtonSave();
    assert.equal(calls.at(-1).url, '/ApiCalls/AddObservationToDraft');
    assert.equal(calls.at(-1).data.ObservationId, 42);
    assert.equal(calls.at(-1).data.DraftParaNumber, '7');
}
const accepted = calls.length;
for (const [allowed, status] of [[false, 3], [true, 1], [true, 5], [true, 8], [true, 9]]) {
    $('#fieldAuditManageObservationBranchesReplica').attr('data-can-add-to-draft', String(allowed));
    context.g_currentStatus = status;
    context.showActionButtons();
    assert($('#addDraftButton_update').classes.has('d-none'));
    context.updateObservationStatus(42, 5, 3);
    context.finalCommentsButtonSave();
    assert.equal(calls.length, accepted, 'Unassigned users and other workflow states cannot submit draft');
}
console.log('PASS: assigned lead/member draft button and submission; unassigned/state restrictions; settlement unchanged');

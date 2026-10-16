// Run: node tests/ias-para-position/report.test.cjs
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const elements = new Map();
const document = {};
function $(selector) {
    if (selector === document) return { ready: callback => callback() };
    if (!elements.has(selector) || selector === '<span>') {
        elements.set(selector, {
            value: '', content: '', markup: '', disabled: false, classes: new Set(),
            val() { return this.value; },
            text(value) { if (value === undefined) return this.content; this.content = String(value); return this; },
            html() { return this.content.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'); },
            empty() { this.markup = ''; return this; },
            append(value) { this.markup += value; return this; },
            data() { return this.reportingOffice; },
            select2() {},
            addClass(value) { this.classes.add(value); return this; },
            removeClass(value) { this.classes.delete(value); return this; },
            prop(name, value) { this[name] = value; return this; }
        });
    }
    return elements.get(selector);
}
let request;
let initializations = 0;
$.ajax = options => { request = options; };
$.each = (rows, callback) => rows.forEach((row, index) => callback(index, row));
const context = vm.createContext({ $, document, g_asiBaseURL: '/ias',
    destroyDatatable() {}, initializeDataTable() { initializations++; } });
vm.runInContext(fs.readFileSync(path.join(__dirname, '../../AIS/wwwroot/js/csp/Views_Reports_IAS_PARA_POSITION.js'), 'utf8'), context);
$('#entityTypeSelectionField').value = '2';
$('#periodStartModalField').value = '2026-10-01';
$('#periodEndModalField').value = '2026-10-16';
$('#iasParaPositionGrid').reportingOffice = true;
context.entityTypeSelectionChangeEvent();
assert.equal(request.url, '/ias/ApiCalls/get_ias_para_position_for_entity_type_id');
assert.equal(request.type, 'POST');
assert.equal($('#iasParaPositionSearch').disabled, true);
request.success([
    { reportinG_OFFICE: '<office>', chilD_CODE: '1', placE_OF_POSTING: '<img src=x>',
      openinG_BALANCE: '1,000', issuE_IDENTIFIED: 10, issuE_RECTIFIED: 4, parA_ADDED: 6,
      settleD_AUDIT: 2, settleD_COM: 3, outstanding: 1001, r1: 500, r2: 400, r3: 101 },
    { chilD_CODE: '2', placE_OF_POSTING: 'Issues only', issuE_IDENTIFIED: 5, issuE_RECTIFIED: 1 }
]);
request.complete();
assert.equal($('#totalIssueIdentified').text(), '15');
assert.equal($('#totalIssueRectified').text(), '5');
assert.equal($('#totalParaAdded').text(), '6');
assert.equal($('#totalCombined').text(), '1,006'); // Issue counts must not inflate para balance.
assert.equal($('#totalOutstanding').text(), '1,001');
assert.equal($('#iasParaPositionSearch').disabled, false);
assert.equal(initializations, 1);
const markup = $('#iasParaPositionGrid tbody').markup;
assert.equal((markup.match(/<td(?: |>)/g) || []).length, 30); // 15 per row, including reporting office.
assert.ok(markup.includes('&lt;office&gt;'));
assert.ok(markup.includes('&lt;img src=x&gt;'));
assert.ok(!markup.includes('<img src=x>'));

$('#iasParaPositionGrid').reportingOffice = false;
context.entityTypeSelectionChangeEvent();
request.success([{ chilD_CODE: '1' }]);
request.complete();
assert.equal(($('#iasParaPositionGrid tbody').markup.match(/<td(?: |>)/g) || []).length, 14);
assert.equal($('#totalIssueIdentified').text(), '0');
assert.equal($('#totalIssueRectified').text(), '0');
context.entityTypeSelectionChangeEvent();
request.success([]);
request.complete();
assert.equal($('#iasParaPositionGrid tbody').markup, '');
assert.equal($('#totalCombined').text(), '0');
context.entityTypeSelectionChangeEvent();
request.error();
request.complete();
assert.equal($('#iasParaPositionError').classes.has('d-none'), false);
assert.equal($('#iasParaPositionSearch').disabled, false);

request = undefined;
$('#periodEndModalField').value = '2026-09-30';
context.entityTypeSelectionChangeEvent();
assert.equal(request, undefined);
$('#periodEndModalField').value = '';
context.entityTypeSelectionChangeEvent();
assert.equal(request, undefined);
console.log('IAS Para Position: column alignment, totals, escaping, empty results, errors and date validation passed.');

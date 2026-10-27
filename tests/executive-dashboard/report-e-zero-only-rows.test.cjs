'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname, '../..');
const read = relative => fs.readFileSync(path.join(root, relative), 'utf8');

test('Report E Oracle variants discard only five-metric zero rows', () => {
    for (const file of [
        'AIS/Docs/sql/PKG_EXEC_REPORTS.sql',
        'AIS/Docs/sql/f8/PKG_EXEC_REPORTS/02.sql',
        'AIS/Docs/sql/PKG_EXEC_REPORTS_image_rewrite_body.sql'
    ]) {
        const sql = read(file);
        assert.match(sql, /P_REPORT_ID <> 'E' OR a.row_kind <> 'SUBTOTAL'/);
        for (const field of ['opening_balance','paras_added','total_paras','settled_paras','outstanding_paras'])
            assert.match(sql, new RegExp('a\\.' + field + ' <> 0'));
        assert.match(sql, /P_REPORT_ID <> 'D'/); // Other reports retain existing behavior.
    }
});

test('browser Report E preserves meaningful zero values and hides zero-only rows', () => {
    const js = read('AIS/wwwroot/js/executive-reports.js');
    const start = js.indexOf('            const measures = [');
    const end = js.indexOf('            if (!visibleRows.length)', start);
    assert.ok(start >= 0 && end > start);
    const code = js.slice(start, end);
    const rows = [
        {itemName:'Empty', openingBalance:0, parasAdded:0, totalParas:0, settledParas:0, outstandingParas:0},
        {itemName:'Opening', openingBalance:12, parasAdded:0, totalParas:12, settledParas:0, outstandingParas:12},
        {itemName:'Activity', openingBalance:0, parasAdded:4, totalParas:4, settledParas:4, outstandingParas:0},
        {itemName:'Correction', openingBalance:0, parasAdded:0, totalParas:0, settledParas:-1, outstandingParas:-1}
    ];
    const output = vm.runInNewContext(code + '\nvisibleRows.map(row => row.itemName);', {rows});
    assert.deepEqual(Array.from(output), ['Opening','Activity','Correction']);
    assert.match(js, /querySelectorAll\('tbody tr'\)/); // CSV uses the visible table.
});

test('PDF Report E applies the same five-metric rule only in its E branch', () => {
    const pdf = read('AIS/Services/ExecutiveReportsPdfGenerator.cs');
    const branch = pdf.slice(pdf.indexOf('html.Append("<h2>Division-wise Management Audit Settlement</h2>")'));
    for (const field of ['OpeningBalance','ParasAdded','TotalParas','SettledParas','OutstandingParas'])
        assert.match(branch, new RegExp('r\\.' + field + ' != 0'));
    assert.match(branch, /AddTable\(html, "Function \/ Division", visibleRows/);
});

test('Unknown-cycle diagnostic and Report C listing remain read-only', () => {
    const sql = read('AIS/Docs/sql/SNAP_UNKNOWN_CYCLE_REPORT_C_UNASSIGNED_DIAGNOSTICS.sql');
    assert.match(sql, /k\.record_type = 'PARA'/);
    assert.match(sql, /s\.snapshot_cycle IS NULL OR s\.snapshot_cycle < 0/);
    assert.match(sql, /c\.gm_office_id IS NULL OR c\.gm_name IS NULL/);
    assert.match(sql, /c\.region_id IS NULL OR c\.region_name IS NULL/);
    assert.doesNotMatch(sql, /^\s*(UPDATE|DELETE|MERGE|INSERT|ALTER|DROP|CREATE)\s+/im);
});

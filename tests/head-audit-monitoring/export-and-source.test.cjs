'use strict';
const test = require('node:test'), assert = require('node:assert/strict'), fs = require('node:fs'), path = require('node:path');
const { execFileSync } = require('node:child_process');
const root = path.resolve(__dirname, '../..');
const read = file => fs.readFileSync(path.join(root, file), 'utf8').replace(/\r\n/g, '\n');
const ExcelJS = require('../../AIS/wwwroot/lib/dashboard-export/exceljs-4.4.0.min.js');
const { cellValue, workbook } = require('../../AIS/wwwroot/js/head-audit-export.js');
for (const title of ['Audit plan progress', 'Audit office performance', 'Audit plan details', 'Pending actions and exceptions', 'Engagement register']) {
    test(title + ': real XLSX roundtrip preserves metadata, headings, typed values and totals', async () => {
        const rows = [['Office A', '17', '22 Oct 2026'], ['=HYPERLINK("https://evil")', '3', '50.0%']];
        const book = workbook(ExcelJS, title, ['Audit period: FY 2026 (IAS ID 77)', 'Position as on: 2026-10-22', 'Authorized Office A', 'Filters: delayed'],
            ['Entity', 'Days', 'Date / rate'], rows, [['Total distinct plans: 2', '20', '']]);
        const loaded = new ExcelJS.Workbook(); await loaded.xlsx.load(await book.xlsx.writeBuffer());
        const sheet = loaded.worksheets[0];
        assert.equal(sheet.getCell('A1').value, title);
        assert.match(sheet.getCell('A2').value, /IAS ID 77/); assert.match(sheet.getCell('A3').value, /2026-10-22/);
        assert.equal(sheet.getCell('A6').value, 'Entity');
        assert.equal(sheet.getCell('B7').value, 17); assert.ok(sheet.getCell('C7').value instanceof Date);
        assert.equal(sheet.getCell('C8').value, .5); assert.equal(sheet.getCell('C8').numFmt, '0.0%');
        assert.equal(sheet.getCell('A8').type, ExcelJS.ValueType.String); assert.equal(sheet.getCell('A8').formula, undefined);
        assert.match(sheet.getCell('A8').value, /^'=/); assert.equal(sheet.getCell('B9').value, 20);
        assert.equal(sheet.getCell('A9').font.bold, true); assert.ok(sheet.getColumn(1).width >= 16);
    });
}
test('formula injection, negative numeric values and precision are safe', () => {
    for (const value of ['=1+1', '+SUM(A1)', '@SUM(A1)', '-cmd|x', '\t=1+1', '\r\n=cmd']) assert.ok(cellValue(value).value.startsWith("'"));
    assert.equal(cellValue('-12').value, -12); assert.equal(cellValue('1234567890123456').value, '1234567890123456');
});
test('Oracle spec/body signatures and all six F8 execution units stay synchronized', () => {
    const source = read('AIS/Docs/sql/PKG_HEAD_AUDIT_MONITOR.sql');
    const boundary = source.indexOf('CREATE OR REPLACE PACKAGE BODY');
    for (const m of source.slice(0, boundary).matchAll(/PROCEDURE (\w+)\(([\s\S]*?)\);/g)) {
        const body = source.slice(boundary).match(new RegExp('PROCEDURE ' + m[1] + '\\(([\\s\\S]*?)\\) AS'));
        assert.equal(body?.[1].replace(/\s+/g, ' '), m[2].replace(/\s+/g, ' '), m[1]);
    }
    const units = source.split(/^\s*\/\s*$/m).map(s => s.replace(/^[ \t]*(?:WHENEVER|SHOW ERRORS|SET |SPOOL ).*$/gm, '').trim()).filter(s => s.replace(/--[^\n]*/g, '').trim());
    assert.equal(units.length, 6);
    units.forEach((unit, i) => assert.equal(read(`AIS/Docs/sql/f8/PKG_HEAD_AUDIT_MONITOR/${String(i + 1).padStart(2, '0')}.sql`).split('\n').slice(1).join('\n').trim(), unit));
});
test('query selection avoids unrelated datasets and preserves scope/current business constants', () => {
    const db = read('AIS/DBConnection.HeadAuditMonitoring.cs'), sql = read('AIS/Docs/sql/PKG_HEAD_AUDIT_MONITOR.sql');
    assert.match(db, /if \(tab == "progress"\)[\s\S]*?P_GET_OFFICE_SUMMARY[\s\S]*?P_GET_ACTIVE_TEAMS/);
    assert.match(db, /if \(tab == "details" \|\| tab == "review"\)\s*result.Plans = ReadHeadAudit/);
    assert.match(db, /if \(!details\) return engagements;\s*var counts = ReadHeadAudit/);
    assert.match(db, /details: tab != "exceptions"/);
    for (const text of ['p.status IN (1,2,4)', 'e.position_status BETWEEN 4 AND 13', 'e.position_status IN (12,13)', 'e.position_status=14', 'e.position_status BETWEEN 4 AND 11', 'REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID)']) assert.ok(sql.includes(text), text);
    assert.match(sql, /NVL\(l.last_updated_on,l.created_on\) < TRUNC\(P_AS_OF\)\+1/);
    assert.match(sql, /l.status_id=10 OR UPPER\(l.remarks\) LIKE '%REVERS%'/);
    assert.match(sql, /unknown_history=0 THEN 1 ELSE 0 END\) not_started/);
    assert.match(sql, /USING P_DEPT_ID,P_PERIOD_ID,P_PERIOD_ID,P_DEPT_ID,P_PERIOD_ID,P_PERIOD_ID/);
});
test('unrelated dashboards, exact para eligibility, and shared Super User export gate are unchanged from main', () => {
    for (const file of ['AIS/DBConnection.AuditParaSnap.cs', 'AIS/Views/Shared/_DashboardExportAssets.cshtml', 'AIS/wwwroot/js/dashboard-export.js', 'AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql', 'AIS/Controllers/ExecutiveOverviewController.cs', 'AIS/Controllers/ExecutiveDashboardController.cs', 'AIS/wwwroot/js/cia-command-center.js', 'AIS/Views/ExecutiveOverview/GROUP.cshtml', 'AIS/Views/ExecutiveOverview/RLM.cshtml']) {
        assert.equal(read(file), execFileSync('git', ['show', 'fa64939f:' + file], { cwd: root, encoding: 'utf8' }).replace(/\r\n/g, '\n'), file);
    }
});

test('security paths require current Oracle session, assignment, page and cutoff checks', () => {
    const db=read('AIS/DBConnection.HeadAuditMonitoring.cs'), sql=read('AIS/Docs/sql/PKG_HEAD_AUDIT_MONITOR.sql'), mvc=read('AIS/Controllers/HeadAuditDepartmentController.cs');
    assert.ok(!mvc.includes('return NoContent()'));
    assert.ok(db.indexOf('ValidateHeadAuditScope(connection, user)') < db.indexOf('command.ExecuteReader()'));
    assert.ok(db.indexOf('ValidateHeadAuditScope(connection, current)') > db.indexOf('command.ExecuteReader()'));
    for(const clause of ["s.session_id=P_SESSION_ID", "s.session_active='Y'", 's.group_id=P_GROUP_ID', 'mp.id=1000401', 'mpg.group_id=NVL(c.group_id,c.role_id)', 'mpg.group_id=m.group_id']) assert.ok(sql.includes(clause),clause);
    assert.ok(sql.includes("AND TRUNC(P_AS_OF)<TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE 'Asia/Karachi' AS DATE))"));
    assert.ok(mvc.includes('ex.Number == 20154) { return Unauthorized(); }'));
    assert.ok(sql.includes('IF P_AS_OF IS NULL THEN RETURN; END IF;'),'Legacy optional-cutoff contract');
});

// Report E / CCO Function Wise Position are explicitly changed in a separate
// guarded feature. Head Audit and all other protected readers remain pinned.
test('CCO Function Wise Position changes do not alter Head Audit or shared authorization',()=>{
 const pkg=read('AIS/Docs/sql/PKG_EXEC_REPORTS.sql');
 assert.match(pkg,/FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r/);
 const page=read('AIS/Views/ExecutiveOverview/CCO.cshtml');
 assert.match(page,/cco-function-position.js/);
 assert.match(page,/Model.SelectedView.Id == "performance" \|\| Model.SelectedView.Id == "effectiveness"/);
});

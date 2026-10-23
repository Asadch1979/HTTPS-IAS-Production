/* Head Audit page-specific exports. No data endpoint and no shared export-policy changes. */
(() => {
    'use strict';
    const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    function cellValue(text) {
        const value = String(text ?? '').trim();
        if (/^-?\d+(?:\.\d+)?$/.test(value) && value.replace(/[-.]/g, '').length <= 15)
            return { value: Number(value), numFmt: Number.isInteger(Number(value)) ? '#,##0' : '#,##0.00' };
        if (/^\d+(?:\.\d+)?%$/.test(value)) return { value: Number(value.slice(0, -1)) / 100, numFmt: '0.0%' };
        const date = /^(\d{2}) ([A-Z][a-z]{2}) (\d{4})$/.exec(value);
        if (date && months.includes(date[2])) {
            const parsed = new Date(Date.UTC(+date[3], months.indexOf(date[2]), +date[1]));
            if (parsed.getUTCDate() === +date[1]) return { value: parsed, numFmt: 'dd mmm yyyy' };
        }
        // ExcelJS strings are not formula objects; prefix dangerous spreadsheet text as defense in depth.
        return { value: /^[\s\u0000-\u001f]*[=+@-]/.test(value) ? "'" + value : value };
    }
    function workbook(ExcelJS, title, metadata, headings, rows, totals) {
        const book = new ExcelJS.Workbook();
        book.creator = 'IAS';
        const sheet = book.addWorksheet('Head Audit');
        const columns = headings.length;
        for (const line of [title, ...metadata]) {
            const row = sheet.addRow([cellValue(line).value]);
            if (columns > 1) sheet.mergeCells(row.number, 1, row.number, columns);
            row.alignment = { wrapText: true, vertical: 'top' };
            row.height = Math.max(24, Math.ceil(String(line).length / 110) * 15);
        }
        const header = sheet.addRow(headings.map(h => cellValue(h).value));
        header.font = { bold: true, color: { argb: 'FFFFFFFF' } };
        header.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF173F56' } };
        for (const values of [...rows, ...totals]) {
            const row = sheet.addRow(values.map(v => cellValue(v).value));
            values.forEach((v, i) => { const format = cellValue(v).numFmt; if (format) row.getCell(i + 1).numFmt = format; });
            row.alignment = { wrapText: true, vertical: 'top' };
        }
        if (totals.length) sheet.lastRow.font = { bold: true };
        sheet.getRow(1).font = { bold: true, size: 16 };
        sheet.views = [{ state: 'frozen', ySplit: header.number }];
        sheet.autoFilter = { from: { row: header.number, column: 1 }, to: { row: header.number + rows.length, column: columns } };
        sheet.columns.forEach((column, index) => {
            const lengths = [headings, ...rows, ...totals].map(row => String(row[index] ?? '').length);
            column.width = Math.min(60, Math.max(16, ...lengths.map(n => Math.min(60, n + 2))));
        });
        return book;
    }
    if (typeof module !== 'undefined' && module.exports) { module.exports = { cellValue, workbook }; return; }
    const root = document.querySelector('.engagement-workbench');
    if (!root) return;
    function values(row) {
        return Array.from(row.cells).flatMap(cell => {
            const copy = cell.cloneNode(true);
            copy.querySelectorAll('small,br').forEach(node => node.before(document.createTextNode('\n')));
            return [copy.textContent.trim(), ...Array(Math.max(0, cell.colSpan - 1)).fill('')];
        });
    }
    function setup() {
        document.querySelectorAll('#head-tab-content table').forEach(table => {
            if (table.dataset.headExport) return;
            table.dataset.headExport = 'true';
            const title = table.closest('section')?.querySelector('h2')?.textContent || 'Head Audit table';
            const button = document.createElement('button'); button.type = 'button'; button.className = 'wb-table-action';
            button.textContent = 'Export Excel (.xlsx)'; button.setAttribute('aria-label', 'Export ' + title + ' as Excel');
            const feedback = document.createElement('span'); feedback.setAttribute('role', 'status');
            table.before(button, feedback);
            button.addEventListener('click', async () => {
                button.disabled = true; feedback.textContent = '';
                try {
                    const check = new URL(location.href); check.searchParams.set('checkOnly', 'true');
                    const response = await fetch(check, { credentials: 'same-origin', cache: 'no-store', redirect: 'error' });
                    if (!response.ok) throw new Error('An active authorized Head Audit session is required.');
                    if (!window.ExcelJS) throw new Error('The local Excel library could not be loaded.');
                    const metadata = [root.dataset.period, 'Position as on: ' + root.dataset.position + ' (Pakistan, end of day)',
                        root.dataset.scope, 'Applied register filters: ' + (table.closest('[data-filters]')?.dataset.filters || 'Not applicable'), root.dataset.limitations];
                    const book = workbook(window.ExcelJS, title, metadata, values(table.tHead.rows[0]),
                        Array.from(table.tBodies[0].rows, values), table.tFoot ? Array.from(table.tFoot.rows, values) : []);
                    const blob = new Blob([await book.xlsx.writeBuffer()], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    const url = URL.createObjectURL(blob), link = document.createElement('a');
                    link.href = url; link.download = 'Head_Audit_' + root.dataset.position + '_' + title.replace(/[^a-z0-9]+/gi, '_') + '.xlsx';
                    document.body.append(link); link.click(); link.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000);
                } catch (error) { feedback.textContent = error.message; }
                finally { button.disabled = false; }
            });
        });
    }
    setup(); document.addEventListener('head-audit-tab-loaded', setup);
})();

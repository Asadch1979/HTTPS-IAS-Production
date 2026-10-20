(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard') || document.querySelector('.engagement-workbench');
    if (!app) return;
    const status = document.createElement('p');
    status.className = 'dashboard-export-status';
    status.setAttribute('role', 'status');
    document.body.append(status);
    let revision = 0, exporting = false;
    app.addEventListener('change', () => ++revision);
    const observer = new MutationObserver(records => {
        if (records.some(r => !r.target.closest?.('.dashboard-export-control'))) ++revision;
        attach();
    });
    function save(blob, name) {
        const url = URL.createObjectURL(blob), link = document.createElement('a');
        link.href = url; link.download = name; link.click();
        setTimeout(() => URL.revokeObjectURL(url), 30000);
    }
    function rows(target) {
        // The DOM contains only the already authorized response. Include chart
        // data in collapsed details, but never deliberately hidden table columns.
        const included = table => {
            for (let node = table; node; node = node.parentElement) {
                if (node.hidden || getComputedStyle(node).display === 'none') return false;
                if (node === target) break;
            }
            return true;
        };
        return Array.from(target.querySelectorAll('table')).filter(included).map(table =>
            Array.from(table.rows).map(row => Array.from(row.cells)
                .filter(cell => !cell.hidden && getComputedStyle(cell).display !== 'none')
                .map(cell => cell.textContent.trim())));
    }
    async function capture(target) {
        await document.fonts.ready;
        const key = target.dataset.dashboardExport;
        return html2canvas(target, {
            backgroundColor: getComputedStyle(app).backgroundColor === 'rgba(0, 0, 0, 0)' ? '#ffffff' : getComputedStyle(app).backgroundColor,
            scale: Math.min(2, window.devicePixelRatio || 1),
            useCORS: false, allowTaint: false, logging: false,
            ignoreElements: node => node.matches?.('.dashboard-export-control,.exec-report-actions'),
            onclone: doc => {
                const copy = doc.querySelector(`[data-dashboard-export="${key}"]`);
                // Expand scroll containers only in the detached rendering document.
                // Leave collapsed details and hidden/unauthorized content hidden.
                const originals = [target, ...target.querySelectorAll('*')].filter(node => !node.closest('.dashboard-export-control'));
                const copies = [copy, ...copy.querySelectorAll('*')].filter(node => !node.closest('.dashboard-export-control'));
                originals.forEach((node, i) => {
                    const clone = copies[i], style = getComputedStyle(node);
                    if (!clone || node.hidden || style.display === 'none') return;
                    if (/(auto|scroll)/.test(style.overflow + style.overflowX + style.overflowY)) {
                        clone.style.overflow = 'visible';
                        clone.style.maxHeight = 'none';
                        clone.style.height = 'auto';
                        if (node.scrollWidth > node.clientWidth) clone.style.minWidth = node.scrollWidth + 'px';
                    }
                });
                copy.style.height = 'auto';
                copy.style.maxHeight = 'none';
                copy.style.width = Math.max(target.getBoundingClientRect().width, copy.scrollWidth) + 'px';
                copy.style.maxWidth = 'none';
            }
        });
    }
    async function exportSection(target, format) {
        if (exporting) return;
        if (app.getAttribute('aria-busy') === 'true' || target.hidden ||
            document.getElementById('exec-results')?.hidden ||
            (app.dataset.view === 'reports' && document.getElementById('exec-report-panel')?.hidden)) {
            status.textContent = 'Apply the selected filters and wait for the dashboard to finish loading.';
            return;
        }
        exporting = true;
        const selectedRevision = revision;
        status.textContent = 'Preparing section export…';
        try {
            const tables = rows(target);
            const canvas = await capture(target);
            if (!canvas.width || !canvas.height) throw new Error('The section is too large to capture.');
            if (revision !== selectedRevision) throw new Error('The dashboard changed. Export the refreshed section again.');
            const name = (target.querySelector('h1,h2,h3')?.textContent || target.getAttribute('aria-label') || 'IAS dashboard')
                .trim().replace(/[^a-z0-9 -]/gi, '').slice(0, 90) || 'IAS dashboard';
            if (format === 'png') {
                const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/png'));
                if (!blob) throw new Error('The section is too large to capture.');
                if (revision !== selectedRevision) throw new Error('The dashboard changed. Export the refreshed section again.');
                save(blob, name + '.png');
            } else if (format === 'pdf') {
                const pdf = new jspdf.jsPDF({ orientation: canvas.width > canvas.height ? 'landscape' : 'portrait', unit: 'pt', format: 'a4' });
                const width = pdf.internal.pageSize.getWidth() - 32;
                const height = pdf.internal.pageSize.getHeight() - 32;
                const sliceHeight = Math.max(1, Math.floor(height * canvas.width / width));
                for (let y = 0; y < canvas.height; y += sliceHeight) {
                    if (y) pdf.addPage();
                    const slice = document.createElement('canvas');
                    slice.width = canvas.width; slice.height = Math.min(sliceHeight, canvas.height - y);
                    slice.getContext('2d').drawImage(canvas, 0, y, slice.width, slice.height, 0, 0, slice.width, slice.height);
                    pdf.addImage(slice.toDataURL('image/png'), 'PNG', 16, 16, width, slice.height * width / canvas.width);
                }
                pdf.save(name + '.pdf');
            } else {
                const workbook = new ExcelJS.Workbook();
                const snapshot = workbook.addWorksheet('Visual snapshot');
                const image = workbook.addImage({ base64: canvas.toDataURL('image/png'), extension: 'png' });
                snapshot.addImage(image, { tl: { col: 0, row: 0 }, ext: { width: canvas.width, height: canvas.height } });
                tables.forEach((data, index) => {
                    const sheet = workbook.addWorksheet('Table ' + (index + 1));
                    // Literal strings prevent spreadsheet formula injection.
                    data.forEach(row => sheet.addRow(row.map(value =>
                        /^-?\d+(?:\.\d+)?$/.test(value.replace(/,/g, '')) ? Number(value.replace(/,/g, '')) : value)));
                    sheet.getRow(1).font = { bold: true };
                    sheet.columns.forEach(column => { column.width = 24; });
                });
                const cards = target.querySelectorAll('.exec-kpi,.wb-stat,.cia-kpi-card');
                if (cards.length) {
                    const sheet = workbook.addWorksheet('KPIs');
                    sheet.addRow(['KPI', 'Value', 'Context']);
                    cards.forEach(card => sheet.addRow([
                        card.querySelector('span,h3,h4')?.textContent.trim() || '',
                        card.querySelector('strong')?.textContent.trim() || '',
                        card.querySelector('small')?.textContent.trim() || ''
                    ]));
                    sheet.columns.forEach(column => { column.width = 32; });
                }
                const buffer = await workbook.xlsx.writeBuffer();
                if (revision !== selectedRevision) throw new Error('The dashboard changed. Export the refreshed section again.');
                save(new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }), name + '.xlsx');
            }
            status.textContent = 'Section exported.';
        } catch (error) {
            status.textContent = 'Export failed: ' + error.message;
        } finally { exporting = false; }
    }
    let nextId = 0;
    function attach() {
        for (const target of [app, ...app.querySelectorAll('.exec-panel,.exec-kpis,.cia-command-scorecard,.mgmt-panel,.wb-kpis,.wb-overview,.wb-process')]) {
            if (target.querySelector(':scope > .dashboard-export-control')) continue;
            if (!target.dataset.dashboardExport) target.dataset.dashboardExport = String(++nextId);
            const control = document.createElement('details');
            control.className = 'dashboard-export-control';
            control.setAttribute('data-html2canvas-ignore', 'true');
            const summary = document.createElement('summary'); summary.textContent = 'Export';
            const menu = document.createElement('div');
            for (const [format, label] of [['png', 'Picture (PNG)'], ['xlsx', 'Excel (.xlsx)'], ['pdf', 'PDF']]) {
                const button = document.createElement('button'); button.type = 'button'; button.textContent = label;
                button.addEventListener('click', () => { control.open = false; exportSection(target, format); });
                menu.append(button);
            }
            control.append(summary, menu); target.append(control);
        }
    }
    attach();
    observer.observe(app, { childList: true, subtree: true, characterData: true });
})();

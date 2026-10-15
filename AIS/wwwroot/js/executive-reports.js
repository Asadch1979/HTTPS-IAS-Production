(() => {
    'use strict';
    const root = document.getElementById('exec-reports');
    if (!root) return;
    const app = document.getElementById('executive-dashboard');
    const $ = id => document.getElementById(`exec-${id}`);
    const form = $('filters'), selection = $('report-selection');
    const buttons = [...root.querySelectorAll('[data-report]')];
    const node = (tag, value, className) => {
        const result = document.createElement(tag);
        if (value != null) result.textContent = value;
        if (className) result.className = className;
        return result;
    };
    const number = value => value == null ? 'N/A' : Number(value).toLocaleString('en-GB');
    const percent = value => value == null ? 'N/A' : `${Number(value).toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}%`;
    const date = value => {
        const parsed = new Date(`${String(value).slice(0, 10)}T00:00:00Z`);
        return Number.isNaN(parsed.getTime()) ? 'N/A' : new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: 'short', year: 'numeric', timeZone: 'UTC' }).format(parsed).replaceAll(' ', '-');
    };
    const kind = row => String(row.rowKind || '').toUpperCase();
    const detail = row => ['DETAIL', 'ANNUAL'].includes(kind(row));
    const total = row => ['TOTAL', 'GRAND_TOTAL'].includes(kind(row));
    const column = (label, key, format = number) => ({ label, key, format, numeric: format === number || format === percent });
    const plain = value => value == null || value === '' ? 'N/A' : String(value);
    const position = [column('Opening balance', 'openingBalance'), column('Paras added', 'parasAdded'), column('Total paras', 'totalParas'),
        column('Settled', 'settledParas'), column('Settlement %', 'settlementPercentage', percent), column('Outstanding', 'outstandingParas'),
        column('Low risk', 'lowRisk'), column('Medium risk', 'mediumRisk'), column('High risk', 'highRisk'), column('Unrated risk', 'unratedRisk'),
        column('Settled by Compliance', 'settledByCompliance'), column('Settled by Audit', 'settledByAudit'), column('Settled by Unknown', 'settledByUnknown')];
    const rankingColumns = [column('Rank', 'rankPosition'), column('Region', 'itemName', plain), column('Total paras', 'totalParas'),
        column('Settlement %', 'settlementPercentage', percent), column('Outstanding paras', 'outstandingParas')];
    function table(target, title, columns, rows, grouped = false) {
        const wrap = node('div', null, 'exec-table-wrap exec-report-table-wrap');
        wrap.tabIndex = 0; wrap.setAttribute('role', 'region'); wrap.setAttribute('aria-label', title);
        const table = node('table', null, 'exec-table exec-report-table');
        table.append(node('caption', title));
        const head = node('thead'), header = node('tr'), body = node('tbody');
        columns.forEach(col => { const th = node('th', col.label, col.numeric ? 'exec-number' : ''); th.scope = 'col'; header.append(th); });
        head.append(header);
        let previousGroup;
        rows.forEach(row => {
            const groupKey = `${row.groupId ?? ''}:${row.groupName ?? ''}`;
            if (grouped && !total(row) && groupKey !== previousGroup) {
                const group = node('tr', null, 'exec-report-group'), label = node('th', row.groupName || 'Unassigned group');
                label.colSpan = columns.length; label.scope = 'rowgroup'; group.append(label); body.append(group); previousGroup = groupKey;
            }
            const tr = node('tr', null, total(row) ? 'exec-report-total' : kind(row) === 'SUBTOTAL' ? 'exec-report-subtotal' : '');
            columns.forEach((col, index) => {
                const value = index === 0 && total(row) ? 'Grand total' : index === 0 && kind(row) === 'SUBTOTAL' ? `Subtotal · ${row.groupName || 'Unassigned group'}` : col.format(row[col.key]);
                const cell = node(index === 0 ? 'th' : 'td', value, col.numeric ? 'exec-number' : '');
                if (index === 0) cell.scope = 'row'; tr.append(cell);
            });
            body.append(tr);
        });
        if (!rows.length) { const tr = node('tr'), cell = node('td', 'No records for the selected scope and reporting period.'); cell.colSpan = columns.length; tr.append(cell); body.append(tr); }
        table.append(head, body); wrap.append(table); target.append(wrap);
    }
    function bars(target, title, rows, series, label) {
        const section = node('section', null, 'exec-report-chart'); section.append(node('h3', title));
        const legend = node('p', series.map(s => s.label).join(' · '), 'exec-note'); section.append(legend);
        const countMaximum = Math.max(1, ...rows.flatMap(row => series.filter(s => !s.percent).map(s => Number(row[s.key]) || 0)));
        rows.forEach(row => {
            const group = node('div', null, 'exec-report-bar-group'); group.append(node('h4', label(row)));
            series.forEach((s, index) => {
                const value = row[s.key], line = node('div', null, 'exec-report-bar-row');
                const track = node('div', null, 'exec-track'), fill = node('div', null, `exec-fill exec-report-series-${index}`);
                // Always use a 0–100% axis for percentage bars, across TOP and BOTTOM charts.
                const axisMaximum = s.percent ? 100 : countMaximum;
                fill.style.width = `${Math.min(100, Math.max(0, (Number(value) || 0) / axisMaximum * 100))}%`;
                line.title = `${label(row)} · ${s.label}: ${s.percent ? percent(value) : number(value)}`;
                track.append(fill); line.append(node('span', s.label), track, node('strong', s.percent ? percent(value) : number(value))); group.append(line);
            });
            section.append(group);
        });
        if (!rows.length) section.append(node('p', 'No ranked or annual records for this period.', 'exec-empty'));
        target.append(section);
    }
    function render(id, rows) {
        const target = $('report-content'); target.replaceChildren();
        if (!rows.length) { target.append(node('p', 'No records for the selected scope and reporting period.', 'exec-empty')); return; }
        if (id === 'A') {
            const annual = rows.filter(detail);
            const year = row => `${String(row.periodStart).slice(0, 4)}${row.isPartialYear ? ` · Partial year (${date(row.periodStart)} – ${date(row.periodEnd)})` : ''}`;
            const charts = node('div', null, 'exec-report-charts');
            bars(charts, 'Annual field audit comparison', annual, [{key:'settledParas',label:'Total paras settled'}, {key:'parasAdded',label:'Paras added'}, {key:'outstandingParas',label:'Paras outstanding'}], year);
            bars(charts, 'Annual settlement percentage', annual, [{key:'settlementPercentage',label:'Settlement %',percent:true}], year);
            target.append(charts);
            table(target, 'Annual field audit summary', [column('Reporting year', 'yearLabel', plain), column('Total paras settled','settledParas'), column('Settlement %','settlementPercentage',percent), column('Paras added','parasAdded'), column('Paras outstanding','outstandingParas')], rows.map(row => ({...row,yearLabel:year(row)})));
        } else if (id === 'B' || id === 'C') {
            const identity = id === 'B' ? [column('Field office', 'itemName', plain)] : [column('General Manager', 'groupName', plain), column('Region name', 'itemName', plain)];
            table(target, id === 'B' ? 'Field office paras position' : 'GM / region paras position', [...identity, ...position], rows, true);
        } else if (id === 'D') {
            const charts = node('div', null, 'exec-report-charts');
            for (const [group, title] of [['TOP','Top five regions'],['BOTTOM','Bottom five regions']]) {
                // Oracle determines membership and rank, including ties; never re-rank or truncate here.
                const ranked = rows.filter(row => detail(row) && row.rankingGroup === group && row.rankPosition != null).sort((a,b) => a.rankPosition - b.rankPosition);
                const panel = node('section');
                bars(panel, title, ranked, [{key:'settlementPercentage',label:'Settlement %',percent:true}], row => `${row.rankPosition}. ${row.itemName || 'Unassigned region'}`);
                table(panel, title, rankingColumns, ranked); charts.append(panel);
            }
            target.append(charts);
            const unranked = rows.filter(row => detail(row) && (!['TOP','BOTTOM'].includes(row.rankingGroup) || row.rankPosition == null));
            if (unranked.length) table(target, 'Unranked regions (excluded from ranked charts)', rankingColumns, unranked);
            const summaries = rows.filter(row => !detail(row));
            if (summaries.length) table(target, 'Oracle summary records (excluded from ranked charts)', rankingColumns, summaries);
        } else {
            table(target, 'Division-wise management audit settlement', [column('Relevant function / division','itemName',plain), column('Opening balance','openingBalance'), column('Para added','parasAdded'), column('Total','totalParas'), column('Para settled','settledParas'), column('Para outstanding','outstandingParas')], rows);
        }
        const sources = node('details', null, 'exec-data'); sources.open = true; sources.append(node('summary', 'Settlement attribution and reconciliation details'));
        table(sources, 'Oracle settlement attribution and reconciliation', [column('Office / region / year', 'itemName',plain), column('Compliance','settledByCompliance'),column('Audit','settledByAudit'),column('Unknown','settledByUnknown'),column('Reopenings','reopeningEvents', () => 'Not measured'),column('Resettlements','resettlementEvents', () => 'Not measured'),column('Initially closed additions','initiallyClosedAdditions', () => 'Not measured'),column('Reconciliation difference','reconciliationDifference'),column('Auditor assignment mismatch','auditorAssignmentMismatch')], rows);
        target.append(sources);
    }
    // Only a successful response from an authorized report API is exportable.
    const exportCsvButton = $('report-export-csv'), exportPrintButton = $('report-export-print');
    let request, version = 0, loadedReport = null;
    function setExportReady(ready) {
        exportCsvButton.disabled = !ready;
        exportPrintButton.disabled = !ready;
    }
    function csvEscape(value) {
        const text = String(value ?? '').replace(/\s+/g, ' ').trim();
        // Prevent formula execution when a recipient opens exported text in Excel.
        // Preserve genuine negative numeric values as numbers.
        const safe = /^[=+@\-]/.test(text) && !/^-?\d+(?:,\d{3})*(?:\.\d+)?%?$/.test(text) ? "'" + text : text;
        return '"' + safe.replace(/"/g, '""') + '"';
    }
    function exportCsv() {
        if (!loadedReport) return;
        const lines = [];
        const add = cells => lines.push(cells.map(csvEscape).join(','));
        add([loadedReport.title]);
        add(['Reporting period', loadedReport.period]);
        add(['Report', loadedReport.id]);
        $('report-cautions').querySelectorAll('p').forEach(p => add(['Note', p.textContent]));
        const tables = [...$('report-content').querySelectorAll('table')];
        tables.forEach(table => {
            lines.push('');
            add([table.querySelector('caption')?.textContent || 'Report table']);
            add([...table.querySelectorAll('thead th')].map(th => th.textContent));
            table.querySelectorAll('tbody tr').forEach(tr => add([...tr.children].map(cell => cell.textContent)));
        });
        const blob = new Blob(['\uFEFF', lines.join('\r\n')], { type: 'text/csv;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'IAS_Report_' + loadedReport.id + '_' + loadedReport.start + '_to_' + loadedReport.end + '.csv';
        document.body.append(link);
        link.click();
        link.remove();
        window.setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
    exportCsvButton.addEventListener('click', exportCsv);
    exportPrintButton.addEventListener('click', () => { if (loadedReport) window.print(); });
    function invalidate() {
        request?.abort(); ++version; loadedReport = null; setExportReady(false);
        $('report-panel')?.setAttribute('hidden',''); app.setAttribute('aria-busy','false');
    }
    function validDates() {
        const start = $('start-date'), end = $('end-date');
        end.setCustomValidity(!start.value || !end.value || start.value > end.value ? 'Provide both dates, with Start Date no later than End Date.' : '');
        return form.checkValidity();
    }
    function navigation() {
        const values = new FormData(form);
        app.querySelectorAll('[data-view-navigation] a').forEach(link => {
            const url = new URL(link.href);
            for (const key of ['startDate','endDate','selectBy','dimensionId','reportingOfficeId','entityId']) {
                const value = values.get(key); if (value) url.searchParams.set(key,value); else url.searchParams.delete(key);
            }
            if (url.searchParams.get('view') === 'reports' && selection.value) url.searchParams.set('report',selection.value);
            link.href = url.href;
        });
        for (const name of ['startDate','endDate']) {
            const hidden = app.querySelector(`form:not(#exec-filters) input[name="${name}"]`);
            if (hidden) hidden.value = values.get(name) || '';
        }
    }
    async function load() {
        if (root.dataset.needsGroup === 'true' || !buttons.length) return;
        if (!validDates()) { $('status').textContent = 'Choose a valid reporting period and apply filters.'; return; }
        const selected = buttons.find(button => button.dataset.report === selection.value) || buttons[0];
        selection.value = selected.dataset.report;
        buttons.forEach(button => button.setAttribute('aria-pressed', String(button === selected)));
        invalidate(); const current = version; request = new AbortController();
        const query = new URLSearchParams(new FormData(form)); query.delete('report');
        query.set('dashboardType', root.dataset.perspective);
        if (root.dataset.groupRoot) query.set('groupRootId', root.dataset.groupRoot);
        $('report-title').textContent = `Report ${selection.value} · ${selected.dataset.title}`;
        $('report-period').textContent = `${date(query.get('startDate'))} – ${date(query.get('endDate'))} · ${$('entity').selectedOptions[0].textContent}`;
        $('report-content').replaceChildren(); $('report-cautions').replaceChildren(); $('report-panel').hidden = false;
        $('status').textContent = `Loading Report ${selection.value}…`; app.setAttribute('aria-busy','true');
        try {
            const response = await fetch(`${root.dataset.apiUrl}GetReport${selection.value}?${query}`, { signal: request.signal, credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json','X-Requested-With':'XMLHttpRequest'} });
            if (response.redirected || response.status === 401) throw new Error('Your session has expired. Sign in again to view reports.');
            if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('The report endpoint returned an unexpected response. Reload the page or sign in again.');
            const result = await response.json();
            if (!response.ok || result.success !== true) throw new Error(result.message || 'The report could not be loaded.');
            if (!result.data || !Array.isArray(result.data.rows)) throw new Error('The report response is incomplete.');
            if (current !== version) return;
            const cautions = $('report-cautions');
            if (result.historicalBalancesEstimated) cautions.append(node('p','Historical balances are estimates and must not be treated as certified results.'));
            if (result.currentOrganizationMapping) cautions.append(node('p','Historical activity is presented using the current authorized organizational portfolio and hierarchy.'));
            if (result.settlementAttributionEstimated) cautions.append(node('p','Settlement attribution is estimated. Unknown attribution remains included; see settlement attribution details below.'));
            cautions.append(node('p','Reopenings, resettlements and initially closed additions are not yet measured. The Oracle package returns placeholder zeros for these fields; they are shown as Not measured.'));
            if (result.data.settlementMethod) cautions.append(node('p',result.data.settlementMethod));
            if (result.data.auditorBasis) cautions.append(node('p',`Auditor basis: ${result.data.auditorBasis}`));
            render(selection.value,result.data.rows);
            loadedReport = {
                id: selection.value,
                title: $('report-title').textContent,
                period: $('report-period').textContent,
                start: query.get('startDate'),
                end: query.get('endDate')
            };
            setExportReady(true);
            $('status').textContent = `Report ${selection.value} loaded · ${number(result.data.rows.length)} Oracle records · Selected dates and organizational filters applied.`;
        } catch (error) {
            if (current !== version || error.name === 'AbortError') return;
            $('status').textContent = `${error.message} Choose Apply to retry.`;
            $('report-content').append(node('p','Report unavailable for this request.', 'exec-empty'));
        } finally { if (current === version) app.setAttribute('aria-busy','false'); }
    }
    buttons.forEach(button => button.addEventListener('click', () => {
        selection.value = button.dataset.report;
        const url = new URL(location.href); url.searchParams.set('report',selection.value); history.replaceState(null,'',url);
        navigation(); load();
    }));
    form.addEventListener('submit', event => { if (!validDates()) { event.preventDefault(); form.reportValidity(); } });
    for (const [id, children] of [['select-by',['dimension','office','entity']],['dimension',['office','entity']],['office',['entity']]]) {
        $(id)?.addEventListener('change', () => {
            invalidate(); children.forEach(child => { if ($(child)) $(child).replaceChildren(new Option('All matching options','')); });
            navigation(); if (validDates()) form.requestSubmit(); else form.reportValidity();
        });
    }
    for (const id of ['start-date','end-date','entity']) $(id)?.addEventListener('change', () => {
        invalidate(); validDates(); navigation(); $('status').textContent = 'Filters changed. Choose Apply to load this reporting scope.';
    });
    navigation(); load();
})();

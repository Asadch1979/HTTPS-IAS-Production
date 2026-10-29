(() => {
    'use strict';
    const root = document.getElementById('exec-reports');
    if (!root) return;
    const app = document.getElementById('executive-dashboard');
    const snap=app?.classList.contains('exec-snap') === true;
    const settlementSources=new Set(['settledByCompliance','settledByAudit','settledByUnknown']);
    const $ = id => document.getElementById(`exec-${id}`);
    const form = $('filters'), selection = $('report-selection');
    const buttons = [...root.querySelectorAll('[data-report]')];
    // RLM has no report selector/export panel until a scoped report is authorized.
    if (!buttons.length) return;
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
    const plain = value => value == null || value === '' ? '' : String(value);
    const groupLabel = value => value === 'GM Field Offices' ? 'Field Audit Division'
        : value === 'Other Field Offices' ? 'Management Audit Division' : (value || 'Unassigned group');
    const position = [column('Opening balance', 'openingBalance'), column('Paras added', 'parasAdded'), column('Total paras', 'totalParas'),
        column('Settled', 'settledParas'), column('Settlement %', 'settlementPercentage', percent), column('Outstanding', 'outstandingParas'),
        column('Low risk', 'lowRisk'), column('Medium risk', 'mediumRisk'), column('High risk', 'highRisk'), column('Unrated risk', 'unratedRisk'),
        column('Settled by Compliance', 'settledByCompliance'), column('Settled by Audit', 'settledByAudit'), column('Settled by Unknown', 'settledByUnknown')];
    const rankingColumns = [column('Rank', 'rankPosition'), column('Region', 'itemName', plain), column('Total paras', 'totalParas'),
        column('Settlement %', 'settlementPercentage', percent), column('Outstanding paras', 'outstandingParas')];
    function table(target, title, columns, rows, grouped = false) {
        if(snap) columns=columns.filter(col=>!settlementSources.has(col.key));
        const wrap = node('div', null, 'exec-table-wrap exec-report-table-wrap');
        wrap.tabIndex = 0; wrap.setAttribute('role', 'region'); wrap.setAttribute('aria-label', title);
        const table = node('table', null, 'exec-table exec-report-table');
        table.append(node('caption', title));
        const head = node('thead'), header = node('tr'), body = node('tbody');
        columns.forEach(col => {
            const th = node('th', col.label, col.numeric ? 'exec-number' : '');
            if (col.key === 'rankPosition') th.classList.add('exec-rank');
            th.scope = 'col'; header.append(th);
        });
        head.append(header);
        let previousGroup;
        rows.forEach(row => {
            const groupKey = `${row.groupId ?? ''}:${row.groupName ?? ''}`;
            if (grouped && !total(row) && groupKey !== previousGroup) {
                const group = node('tr', null, 'exec-report-group'), label = node('th', groupLabel(row.groupName));
                label.colSpan = columns.length; label.scope = 'rowgroup'; group.append(label); body.append(group); previousGroup = groupKey;
            }
            const tr = node('tr', null, total(row) ? 'exec-report-total' : kind(row) === 'SUBTOTAL' ? 'exec-report-subtotal' : '');
            columns.forEach((col, index) => {
                const value = index === 0 && total(row) ? 'Grand total' : index === 0 && kind(row) === 'SUBTOTAL' ? `Subtotal · ${groupLabel(row.groupName)}`
                    : kind(row) === 'SUBTOTAL' && (row[col.key] == null || row[col.key] === '') ? '' : col.format(row[col.key]);
                const cell = node(index === 0 ? 'th' : 'td', value, col.numeric ? 'exec-number' : '');
                if (col.key === 'rankPosition') cell.classList.add('exec-rank');
                if (col.key === 'settlementPercentage' && value !== 'N/A' && detail(row)) {
                    cell.classList.add('exec-report-settlement');
                    // Visual data bar, not a performance target or status classification.
                    cell.style.setProperty('--settlement-width', `${Math.max(0, Math.min(100, Number(row.settlementPercentage) || 0))}%`);
                }
                if (['lowRisk','mediumRisk','highRisk'].includes(col.key)) cell.classList.add('exec-risk-' + col.key);
                if (value === 'Not measured') cell.classList.add('exec-not-measured');
                if (index === 0) cell.scope = 'row'; tr.append(cell);
            });
            body.append(tr);
        });
        if (!rows.length) { const tr = node('tr'), cell = node('td', 'No records for the selected scope and reporting period.'); cell.colSpan = columns.length; tr.append(cell); body.append(tr); }
        table.append(head, body); wrap.append(table); target.append(wrap);
    }
    function bars(target, title, rows, series, label, options = {}) {
        const section = node('section', null, 'exec-report-chart');
        if (options.semantic) section.classList.add('exec-report-' + options.semantic);
        section.append(node('h3', title));
        if (options.benchmark != null) section.append(node('p', `Overall settlement average: ${percent(options.benchmark)} (dashed reference line)`, 'exec-note'));
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
                track.append(fill);
                if (options.benchmark != null && s.percent) {
                    const marker = node('i', null, 'exec-report-benchmark');
                    marker.style.left = `${Math.max(0, Math.min(100, Number(options.benchmark)))}%`;
                    track.append(marker);
                }
                line.append(node('span', s.label), track, node('strong', s.percent ? percent(value) : number(value))); group.append(line);
            });
            section.append(group);
        });
        if (!rows.length) section.append(node('p', 'No ranked or annual records for this period.', 'exec-empty'));
        target.append(section);
    }
    function annualChart(annual, year) {
        const wrap = node('section', null, 'exec-report-annual-visual');
        wrap.append(node('h3', 'Annual settled vs added; outstanding trend'));
        const svg = (tag, attrs = {}, value) => {
            const element = document.createElementNS('http://www.w3.org/2000/svg', tag);
            Object.entries(attrs).forEach(([name, val]) => element.setAttribute(name, String(val)));
            if (value != null) element.textContent = value;
            return element;
        };
        const graph = svg('svg', {viewBox:'0 0 760 285',role:'img', 'aria-label':'Annual settled and added counts shown as grouped columns; outstanding stock shown as an amber line'});
        const maxFlow = Math.max(1, ...annual.map(r => Math.max(Number(r.settledParas) || 0, Number(r.parasAdded) || 0)));
        const maxStock = Math.max(1, ...annual.map(r => Number(r.outstandingParas) || 0));
        const xAt = i => 75 + (i + .5) * (620 / Math.max(1, annual.length));
        const y = v => 238 - Math.max(0, v) / maxFlow * 183;
        const sy = v => 238 - Math.max(0, v) / maxStock * 183;
        for (let t=0;t<=4;t++) {
            const gy = 238 - t * 45.75;
            graph.append(svg('line',{x1:64,y1:gy,x2:700,y2:gy,stroke:'#e2e8f0'}));
            graph.append(svg('text',{x:60,y:gy+4,'text-anchor':'end','font-size':10,fill:'#64748b'},number(maxFlow*t/4)));
        }
        const path = [];
        annual.forEach((row,i) => {
            const x=xAt(i), settled=Number(row.settledParas)||0, added=Number(row.parasAdded)||0, stock=Number(row.outstandingParas)||0;
            graph.append(svg('rect',{x:x-25,y:y(settled),width:22,height:238-y(settled),rx:2,fill:'#0f5c4d'}));
            graph.append(svg('rect',{x:x+3,y:y(added),width:22,height:238-y(added),rx:2,fill:'#75b5a2'}));
            graph.append(svg('text',{x,y:258,'text-anchor':'middle','font-size':11,fill:'#1f2933'},String(row.periodStart).slice(0,4)));
            path.push(`${i?'L':'M'} ${x} ${sy(stock)}`);
        });
        if (path.length) {
            graph.append(svg('path',{d:path.join(' '),fill:'none',stroke:'#e39b1b','stroke-width':3}));
            annual.forEach((row,i)=>{
                const x=xAt(i), stock=Number(row.outstandingParas)||0;
                graph.append(svg('circle',{cx:x,cy:sy(stock),r:4,fill:'#e39b1b'}));
                graph.append(svg('text',{x:708,y:sy(stock)+4,'font-size':9,fill:'#8b5d08'},i===annual.length-1?number(stock):''));
            });
        }
        wrap.append(graph);
        wrap.append(node('p','■ Settled (dark green)  ■ Added (light green)  — Outstanding (amber, right-hand scale)', 'exec-note'));
        return wrap;
    }
    function sortedWithinGroup(rows) {
        // Preserve Oracle group/subtotal/grand-total boundaries; sort only
        // detail offices/regions inside each GM or division by open volume.
        const output = [], pending = [];
        const flush = () => {
            pending.sort((a,b) => (Number(b.outstandingParas)||0) - (Number(a.outstandingParas)||0));
            output.push(...pending); pending.length = 0;
        };
        rows.forEach(row => {
            if (detail(row)) pending.push(row);
            else { flush(); output.push(row); }
        });
        flush(); return output;
    }
    function render(id, rows, benchmark = null) {
        const target = $('report-content'); target.replaceChildren();
        if (!rows.length) { target.append(node('p', 'No records for the selected scope and reporting period.', 'exec-empty')); return; }
        if (id === 'A') {
            const annual = rows.filter(detail);
            const year = row => `${String(row.periodStart).slice(0, 4)}${row.isPartialYear ? ` · Partial year (${date(row.periodStart)} – ${date(row.periodEnd)})` : ''}`;
            const layout = node('div', null, 'exec-report-annual-layout');
            layout.append(annualChart(annual, year));
            const summary = node('div', null, 'exec-report-annual-summary');
            table(summary, 'Annual field audit summary', [column('Reporting year', 'yearLabel', plain), column('Total paras settled','settledParas'), column('Settlement %','settlementPercentage',percent), column('Paras added','parasAdded'), column('Paras outstanding','outstandingParas')], rows.map(row => ({...row,yearLabel:year(row)})));
            layout.append(summary); target.append(layout);
        } else if (id === 'B' || id === 'C') {
            const dynamicPosition = position.filter(col => col.key !== 'unratedRisk' || rows.some(row => (Number(row.unratedRisk)||0) !== 0));
            if (id === 'C') {
                const gmRows = rows.filter(row => kind(row) === 'SUBTOTAL')
                    .map(row => ({...row, itemName: groupLabel(row.groupName), rowKind:'DETAIL'}));
                const grandTotal = rows.find(total);
                if (grandTotal) gmRows.push(grandTotal);
                table(target, 'GM paras position', [column('GM Office','itemName',plain), ...dynamicPosition], gmRows);
            }
            table(target, id === 'B' ? 'Field office paras position' : 'GM / region and H.O. unit paras position',
                [column(id === 'B' ? 'Field office' : 'Region / H.O. unit','itemName',plain), ...dynamicPosition], sortedWithinGroup(rows), true);
        } else if (id === 'D') {
            const charts = node('div', null, 'exec-report-charts');
            for (const [group, title] of [['TOP','Top five regions'],['BOTTOM','Bottom five regions']]) {
                // Oracle determines membership and rank, including ties; never re-rank or truncate here.
                const ranked = rows.filter(row => detail(row) && row.rankingGroup === group && row.rankPosition != null).sort((a,b) => a.rankPosition - b.rankPosition);
                const panel = node('section');
                bars(panel, title, ranked, [{key:'settlementPercentage',label:'Settlement %',percent:true}], row =>
                    `${row.rankPosition}. ${row.itemName || 'Unassigned region'} · ${number(row.outstandingParas)} open / ${number(row.totalParas)} total`,
                    {semantic: group === 'TOP' ? 'top' : 'bottom', benchmark});
                table(panel, title, rankingColumns, ranked); charts.append(panel);
            }
            target.append(charts);
            const unranked = rows.filter(row => detail(row) && (!['TOP','BOTTOM'].includes(row.rankingGroup) || row.rankPosition == null));
            if (unranked.length) table(target, 'Unranked regions (excluded from ranked charts)', rankingColumns, unranked);
            const summaries = rows.filter(row => !detail(row));
            if (summaries.length) table(target, 'Oracle summary records (excluded from ranked charts)', rankingColumns, summaries);
        } else {
            // Report E: do not show functions whose opening, movement and closing
            // counts are all zero. Keep meaningful zeros in non-empty rows.
            // CSV exports these same rendered rows; other reports are untouched.
            const measures = ['openingBalance', 'parasAdded', 'totalParas', 'settledParas', 'outstandingParas'];
            const visibleRows = rows.filter(row => measures.some(key => row[key] != null && Number(row[key]) !== 0));
            if (!visibleRows.length) {
                target.append(node('p', 'No management audit position or movement for the selected period.', 'exec-empty'));
            } else {
                table(target, 'Division-wise management audit settlement', [column('Group','groupName',plain), column('Division','itemName',plain), column('Opening balance','openingBalance'), column('Para added','parasAdded'), column('Total','totalParas'), column('Settled','settledParas'), column('Para outstanding','outstandingParas'), column('Settlement %','settlementPercentage', percent)], visibleRows);
            }
        }
        if (!snap) {
        const sources = node('details', null, 'exec-data'); sources.open = false;
        sources.append(node('summary', 'IAS Settlement Attribution and Reconciliation'));
        table(sources, 'IAS Settlement Attribution and Reconciliation', [
            column('Office / region / year', 'itemName', plain),
            column('Compliance', 'settledByCompliance'),
            column('Audit', 'settledByAudit'),
            column('Unknown', 'settledByUnknown')
        ], rows);
        target.append(sources);
        }
    }
    // Only a successful response from an authorized report API is exportable.
    const exportCsvButton = $('report-export-csv'), exportPrintButton = $('report-export-print');
    let request, version = 0, loadedReport = null;
    function setExportReady(ready) {
        if (exportCsvButton) exportCsvButton.disabled = !ready;
        if (exportPrintButton) exportPrintButton.disabled = !ready;
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
    exportCsvButton?.addEventListener('click', exportCsv);
    exportPrintButton?.addEventListener('click', async () => {
        if (!loadedReport) return;
        const snapshot = loadedReport;
        exportPrintButton.disabled = true;
        try {
            const response = await fetch(snapshot.pdfUrl, {credentials:'same-origin', cache:'no-store'});
            if (!response.ok || !(response.headers.get('content-type') || '').includes('application/pdf'))
                throw new Error('PDF export was unavailable. Check session and report API permissions.');
            if (loadedReport !== snapshot) return;
            const blob = await response.blob();
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url; link.download = `IAS_Report_${snapshot.id}_${snapshot.start}_to_${snapshot.end}.pdf`;
            document.body.append(link); link.click(); link.remove();
            window.setTimeout(() => URL.revokeObjectURL(url), 30000);
        } catch (error) { $('status').textContent = error.message || 'Unable to export report PDF.'; }
        finally { exportPrintButton.disabled = !loadedReport; }
    });
    function invalidate() {
        request?.abort(); ++version; loadedReport = null; setExportReady(false);
        $('report-panel')?.setAttribute('hidden',''); app.setAttribute('aria-busy','false');
    }
    function validDates() {
        const start = $('start-date'), end = $('end-date');
        end.setCustomValidity(!start.value || !end.value || start.value > end.value ? 'Provide both dates, with Start Date no later than End Date.' : '');
        if (app.dataset.perspective === 'CCO' && start.value && end.value) {
            const a = start.value.split('-').map(Number), b = end.value.split('-').map(Number);
            if ((b[0] - a[0]) * 12 + b[1] - a[1] >= 1200)
                end.setCustomValidity('Select fewer than 1200 calendar months.');
        }
        return form.checkValidity();
    }
    function navigation() {
        const values = new FormData(form);
        app.querySelectorAll('[data-view-navigation] a').forEach(link => {
            const url = new URL(link.href);
            for (const key of ['startDate','endDate','selectBy','dimensionId','reportingOfficeId','entityId','snapDivision','snapDepartmentId']) {
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
        root.dataset.activeReport = selection.value;
        buttons.forEach(button => button.setAttribute('aria-pressed', String(button === selected)));
        invalidate(); const current = version; request = new AbortController();
        const query = new URLSearchParams(new FormData(form)); query.delete('report');
        if (root.dataset.apiView === 'reports' && root.dataset.perspective === 'CCO') query.set('view', 'reports');
        query.set('dashboardType', root.dataset.perspective);
        if (root.dataset.groupRoot) query.set('groupRootId', root.dataset.groupRoot);
        const displayTitle = root.dataset.perspective === 'CCO' ? selected.dataset.title : `Report ${selection.value} · ${selected.dataset.title}`;
        $('report-title').textContent = displayTitle;
        $('report-period').textContent = `${date(query.get('startDate'))} – ${date(query.get('endDate'))} · ${$('entity')?.selectedOptions[0]?.textContent || app.dataset.scopeLabel}`;
        $('report-content').replaceChildren(); $('report-cautions').replaceChildren(); $('report-panel').hidden = false;
        $('status').textContent = `Loading ${root.dataset.perspective === 'CCO' ? displayTitle : 'Report ' + selection.value}…`; app.setAttribute('aria-busy','true');
        try {
            const response = await fetch(`${root.dataset.apiUrl}GetReport${selection.value}?${query}`, { signal: request.signal, credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json','X-Requested-With':'XMLHttpRequest'} });
            if (response.redirected || response.status === 401) throw new Error('Your session has expired. Sign in again to view reports.');
            if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('The report endpoint returned an unexpected response. Reload the page or sign in again.');
            const result = await response.json();
            if (!response.ok || result.success !== true) throw new Error(result.message || 'The report could not be loaded.');
            if (!result.data || !Array.isArray(result.data.rows)) throw new Error('The report response is incomplete.');
            if (current !== version) return;
            const cautions = $('report-cautions');
            // Replace the old six-paragraph cream disclaimer with one quiet,
            // accurate data note. Estimates must still not appear certified.
            cautions.append(node('p', 'ⓘ Data notes · Historical balances and settlement attribution are indicative; current organization mapping applies.', 'exec-note'));
            let benchmark = null;
            if (selection.value === 'D') {
                // Read overall benchmark from the same authorized Field Audit source.
                // Never hardcode the 54.25% observed in one reference screenshot.
                try {
                    const benchmarkResponse = await fetch(`${root.dataset.apiUrl}GetReportC?${query}`,
                        {signal:request.signal,credentials:'same-origin',cache:'no-store'});
                    if (benchmarkResponse.ok) {
                        const benchmarkJson = await benchmarkResponse.json();
                        benchmark = benchmarkJson.data?.rows?.find(total)?.settlementPercentage ?? null;
                    }
                } catch (error) { if (error.name === 'AbortError') throw error; }
            }
            if (current !== version) return;
            render(selection.value,result.data.rows,benchmark);
            loadedReport = {
                id: selection.value,
                title: $('report-title').textContent,
                period: $('report-period').textContent,
                start: query.get('startDate'),
                end: query.get('endDate'),
                pdfUrl: `${root.dataset.apiUrl}GetReport${selection.value}?${query}&format=pdf`
            };
            setExportReady(true);
            $('status').textContent = `${root.dataset.perspective === 'CCO' ? displayTitle : 'Report ' + selection.value} loaded · ${number(result.data.rows.length)} Oracle records · Selected dates and organizational filters applied.`;
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

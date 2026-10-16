(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    if (!app) return;
    const $ = id => document.getElementById(`exec-${id}`);
    const lifecycle = $('kpis')?.dataset.lifecycle === 'true';
    const ciaIssues = $('kpis')?.dataset.ciaIssues === 'true';
    const ciaCommand = !!$('cia-kpis');
    const ciaActions = ['GetCiaAuditYears', 'GetCiaExecKpis', 'GetCiaIssueNature'];
    const prettyDate = value => {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(value || '')) return 'selected date';
        const [year, month, day] = value.split('-').map(Number);
        return new Date(Date.UTC(year, month - 1, day)).toLocaleDateString('en-GB',
            {day: '2-digit', month: 'short', year: 'numeric', timeZone:'UTC'}).replaceAll(' ', '-');
    };
    const number = value => Number(value).toLocaleString();
    const el = (tag, text, className) => {
        const node = document.createElement(tag);
        if (text !== undefined && text !== null) node.textContent = text;
        if (className) node.className = className;
        return node;
    };
    const svg = (tag, attributes = {}) => {
        const node = document.createElementNS('http://www.w3.org/2000/svg', tag);
        Object.entries(attributes).forEach(([key, value]) => node.setAttribute(key, value));
        return node;
    };
    const chart = label => {
        const node = svg('svg', { viewBox: '0 0 560 260', role: 'img', 'aria-label': label, class: 'exec-chart' });
        const title = svg('title'); title.textContent = label; node.append(title);
        return node;
    };
    const text = (parent, x, y, value, attributes = {}) => {
        const node = svg('text', { x, y, fill: '#435f50', 'font-size': 12, ...attributes });
        node.textContent = value; parent.append(node);
    };
    function dataTable(target, labels, rows) {
        const details = el('details', null, 'exec-data');
        details.append(el('summary', 'View chart data'));
        const table = el('table'), head = el('thead'), header = el('tr'), body = el('tbody');
        labels.forEach(label => { const cell = el('th', label); cell.scope = 'col'; header.append(cell); });
        head.append(header);
        rows.forEach(row => { const tr = el('tr'); row.forEach(value => tr.append(el('td', value))); body.append(tr); });
        table.append(head, body); details.append(table); target.append(details);
    }
    function bars(id, rows) {
        const target = $(id); if (!target) return; target.replaceChildren();
        // Report age bucket thresholds in days, consistent with AGING_DAYS.
        // The Oracle bucket codes retain their original boundary calculations.
        const ageLabel = row => id !== 'aging' ? row.bucketName
            : row.bucketCode === '1_3_YEARS' ? '366–1,095 days'
            : row.bucketCode === 'OVER_3_YEARS' ? 'Over 1,095 days'
            : row.bucketName;
        const total = rows.reduce((sum, row) => sum + row.paraCount, 0);
        if (!rows.length || !total) target.append(el('p', 'No open observations in this scope.', 'exec-empty'));
        else {
            const maximum = Math.max(...rows.map(row => row.paraCount));
            const graphic = chart(`${id === 'aging' ? 'Aging' : 'Compliance cycle'} distribution of open observations`);
            rows.forEach((row, index) => {
                const y = 20 + index * (210 / rows.length);
                text(graphic, 0, y + 15, ageLabel(row));
                graphic.append(svg('rect', { x: 145, y, width: 330, height: 20, rx: 4, fill: '#edf2ef' }));
                graphic.append(svg('rect', { x: 145, y, width: 330 * row.paraCount / maximum, height: 20, rx: 4, fill: '#51937b' }));
                text(graphic, 550, y + 15, number(row.paraCount), { 'text-anchor': 'end' });
            });
            target.append(graphic);
        }
        dataTable(target, ['Category', 'Open paras', 'High risk'], rows.map(row => [ageLabel(row), number(row.paraCount), number(row.highRiskCount)]));
    }
    function risk(rows) {
        const target = $('risk'); if (!target) return; target.replaceChildren();
        const total = rows.reduce((sum, row) => sum + row.paraCount, 0);
        if (!total) target.append(el('p', 'No open observations in this scope.', 'exec-empty'));
        else {
            const graphic = el('div', null, 'exec-risk-stack');
            rows.forEach(row => {
                const share = row.paraCount / total * 100;
                const color = /high/i.test(row.riskName) ? '#b42318' : /medium/i.test(row.riskName) ? '#b65c00' : '#2457be';
                const segment = el('div'); segment.style.width = `${share}%`; segment.style.backgroundColor = color;
                segment.title = `${row.riskName}: ${number(row.paraCount)} (${share.toFixed(1)}%)`;
                graphic.append(segment);
                const label = el('p', segment.title, 'exec-risk-label'); label.style.borderLeftColor = color; target.append(label);
            });
            target.prepend(graphic);

        }
        dataTable(target, ['Risk', 'Open paras', 'Percentage'], rows.map(row => [row.riskName, number(row.paraCount), `${number(row.percentage)}%`]));
    }
    function trend(rows) {
        const target = $('trend'); if (!target) return; target.replaceChildren();
        if (!rows.length || !rows.some(row => row.settledParas > 0))
            target.append(el('p', 'No settlements in the returned calendar months.', 'exec-empty'));
        if (rows.length) {
            const graphic = chart('Monthly settled paras and high-risk settlements');
            const rawMaximum = Math.max(1, ...rows.map(row => Math.max(row.settledParas, row.highRiskSettled)));
            const magnitude = Math.pow(10, Math.floor(Math.log10(rawMaximum / 2)));
            const step = Math.max(1, Math.ceil(rawMaximum / 2 / magnitude) * magnitude);
            const maximum = Math.ceil(rawMaximum / step) * step;
            Array.from({ length: Math.round(maximum / step) + 1 }, (_, i) => i * step / maximum).forEach(fraction => {
                const y = 215 - fraction * 180;
                graphic.append(svg('line', { x1: 50, x2: 540, y1: y, y2: y, stroke: '#e1e8e3', 'stroke-dasharray': '4 4' }));
                text(graphic, 43, y + 4, number(Math.round(maximum * fraction)), { 'text-anchor': 'end' });
            });
            const x = index => 60 + index * 470 / Math.max(1, rows.length - 1);
            [['settledParas', '#2457be'], ['highRiskSettled', '#b42318']].forEach(([key, color]) => {
                const points = rows.map((row, index) => `${x(index)},${215 - row[key] / maximum * 180}`).join(' ');
                graphic.append(svg('polyline', { points, fill: 'none', stroke: color, 'stroke-width': 3, 'stroke-dasharray': key === 'highRiskSettled' ? '7 5' : 'none' }));
                rows.forEach((row, index) => {
                    const point = svg('circle', { cx: x(index), cy: 215 - row[key] / maximum * 180, r: 5, fill: color, tabindex: 0 });
                    const label = `${row.settlementMonth}: ${key === 'settledParas' ? 'All settled' : 'High-risk settled'} ${number(row[key])}`;
                    const hint = svg('title'); hint.textContent = label; point.append(hint); point.setAttribute('aria-label', label); graphic.append(point);
                });
            });
            rows.forEach((row, index) => { if (index % 3 === 0 || index === rows.length - 1) text(graphic, x(index), 241, row.settlementMonth, { 'text-anchor': 'middle', 'font-size': 10 }); });
            target.append(el('p', '━━ All settled (blue) · ┄┄ High-risk settled (red)', 'exec-legend'), graphic);
        }
        dataTable(target, ['Month', 'Settled paras', 'High-risk settled'], rows.map(row => [row.settlementMonth, number(row.settledParas), number(row.highRiskSettled)]));
    }
    function kpis(data) {
        const target = $('kpis'); if (!target) return; target.replaceChildren();
        [['Total Cases', 'totalCases'], ['Settled Total', 'settledTotal'], ['Open Paras', 'openParas'], ['High Risk', 'highRiskParas'],
            ['Issue Identified', 'issueIdentified'], ['Issue Rectified', 'issueRectified'], ['Settled Month-to-Date', 'settledMtd'],
            ['Settlement Ratio', 'settlementRatio'], ['Critical Items', 'criticalItems'], ['Zero Cycle', 'zeroCycleParas'],
            ['Over 3 Cycles', 'over3CycleParas'], ['Over 1 Year', 'over1YearParas']].filter(([, key]) => target.dataset.kpiKeys.split(',').includes(key)).forEach(([label, key]) => {
            const card = el('div', null, 'exec-kpi');
            card.append(el('span', label), el('strong', number(data[key]) + (key === 'settlementRatio' ? '%' : '')));
            if (ciaIssues && ['issueIdentified', 'issueRectified'].includes(key)) {
                const dates = new FormData($('filters'));
                card.append(el('small', dates.get('startDate') && dates.get('endDate')
                    ? 'Selected Date Range' : 'All eligible IAS issue history'));
                target.append(card);
                return;
            }
            const denominator = ['totalCases', 'openParas', 'settledTotal'].includes(key) ? data.totalCases : key === 'settledMtd' ? data.settledTotal : data.openParas;
            card.append(el('small', key === 'settlementRatio' ? 'Settled as a share of all cases' : denominator > 0 ? `${(data[key] / denominator * 100).toFixed(1)}% of ${key === 'settledMtd' ? 'all settled' : ['totalCases', 'openParas', 'settledTotal'].includes(key) ? 'all cases' : 'open paras'}` : 'Comparison unavailable'));
            target.append(card);
        });
    }
    function lifecycleKpis(data) {
        const cards = document.createDocumentFragment();
        const openingQuality = data.historicalOpeningVerified ? 'verified' : data.openingEstimated ? 'estimated' : 'unavailable';
        const closingQuality = data.closingAsofVerified ? 'verified' : data.closingEstimated ? 'estimated' : 'unavailable';
        const startLabel = prettyDate($('start-date')?.value);
        const endLabel = prettyDate($('end-date')?.value);
        [['Opening Open Paras', 'openingOpenParas', openingQuality],
            ['Issue Identified', 'issueIdentified', 'period'],
            ['Issue Rectified', 'issueRectified', 'period'],
            ['Para Added', 'paraAdded', 'period'],
            ['Paras Settled', 'parasSettled', 'period'],
            ['Closing Open Paras', 'closingOpenParas', closingQuality],
            ['High-Risk Open Paras', 'highRiskOpenParas', closingQuality],
            ['Over 1 Year', 'over1YearParas', closingQuality],
            ['Over 3 Compliance Cycles', 'over3ComplianceCycles', closingQuality]]
            .forEach(([label, key, quality]) => {
                const card = el('div', null, 'exec-kpi');
                const available = quality !== 'unavailable' && data[key] !== null && data[key] !== undefined;
                card.append(el('span', label), el('strong',
                    available ? (quality === 'estimated' ? '≈ ' : '') + number(data[key]) : 'N/A'));
                const note = key === 'openingOpenParas' ? `Position as on ${startLabel}`
                    : key === 'closingOpenParas' ? `Position as on ${endLabel}`
                    : !available ? 'Historical value unavailable'
                    : quality === 'period' ? 'Selected Date Range'
                    : quality === 'estimated'
                        ? (key === 'over3ComplianceCycles' ? 'Estimated · current cycle data' : 'Estimated · historical changes not fully traced')
                        : 'Verified current snapshot';
                card.append(el('small', note));
                // Opening is a baseline, not its own 100% KPI. Rectification and
                // identification may involve different issue cohorts in this interval.
                if (key !== 'openingOpenParas' && key !== 'issueRectified') {
                    const base = ['highRiskOpenParas', 'over1YearParas', 'over3ComplianceCycles'].includes(key) ? data.closingOpenParas : data.openingOpenParas;
                    card.append(el('small', available && base > 0 ? `${(data[key] / base * 100).toFixed(1)}% of ${['highRiskOpenParas', 'over1YearParas', 'over3ComplianceCycles'].includes(key) ? 'closing open paras' : 'opening open paras'}` : 'Comparison unavailable'));
                }
                cards.append(card);
            });
        $('kpis').replaceChildren(cards);
    }
    let criticalRows = [], criticalCount = 0;
    function critical(rows, count) {
        const target = $('critical'); if (!target) return; target.replaceChildren();
        const selection = $('critical-risk').value;
        rows = rows.filter(row => !selection || row.riskName === selection);
        $('critical-note').textContent = `Showing ${number(rows.length)} of ${number(count)} critical items. At most 100 rows, ordered by high risk, age and case ID. Expand an observation to read its text.`;
        if (!rows.length) {
            const tr = el('tr'), cell = el('td', 'No critical observations in this scope.'); cell.colSpan = 7; tr.append(cell); target.append(tr);
        }
        rows.forEach(row => {
            const tr = el('tr');
            [row.entityName, `${row.auditPeriod ?? 'N/A'} / ${row.paraNo ?? 'N/A'}`, row.riskName,
                row.comCycle].forEach(value => tr.append(el('td', value ?? 'N/A')));
            const age = el('td', row.agingDays == null ? 'N/A' : `${number(row.agingDays)} days`);
            if (row.agingDays != null) age.title = `${number(row.agingDays)} days`;
            tr.append(age, el('td', row.daysSinceLastMovement == null ? 'No movement' : number(row.daysSinceLastMovement)));
            const cell = el('td'), details = el('details');
            details.append(el('summary', `Observation ${row.comId}`), el('p', row.gistOfParas || 'Observation text is unavailable.'));
            cell.append(details); tr.append(cell); target.append(tr);
        });
    }
    function ciaKpis(data, yearLabel = 'Selected audit year') {
        const target = $('cia-kpis');
        const count = value => value == null ? 'N/A' : number(value);
        target.replaceChildren();
        const annual = `Annual · ${yearLabel}`;
        const current = data ? `Current state · ${prettyDate(data.currentStateAsOf?.slice(0, 10))} · All audit years` : 'Current state · All audit years';
        const cards = [
            ['Audit Plan Coverage', data ? `${count(data.plannedEntities)} / ${count(data.auditedEntities)}` : 'N/A', annual,
                `Planned / Audited entities · ${!data ? 'Coverage unavailable' : data.planCoveragePct == null ? 'Coverage N/A (no denominator)' : `${number(data.planCoveragePct)}% coverage (audited ÷ planned)`}`],
            ['Delayed Audit Reports', count(data?.delayedAuditReports), current, 'Audit end +15 days passed; no status-14 milestone'],
            ['Fraud Detected (A1 Paras)', count(data?.fraudA1Paras), annual, 'Reported A1 paras, statuses 8/9; not a determination of fraud'],
            ['Awaiting Auditee Response', count(data?.awaitingAuditeeResponse), current, 'Observations with STATUS = 2'],
            ['Teams Conducting Audit', count(data?.teamsConductingAudit), current, 'Distinct teams in commenced, unconcluded audit stages 4–12']
        ];
        cards.forEach(([label, value, period, note]) => {
            const card = el('article', null, 'exec-kpi');
            card.append(el('span', label), el('strong', value), el('small', period, 'exec-cia-period'), el('small', note));
            target.append(card);
        });
    }
    function ciaNature(rows) {
        const target = $('cia-nature'); target.replaceChildren();
        if (!rows.length) { target.append(el('p', 'No issue-category data was returned. Classification is unavailable.', 'exec-empty')); return; }
        const first = rows[0];
        const validCount = value => Number.isSafeInteger(value) && value >= 0;
        if (rows.length !== 10 || new Set(rows.map(row => row.categoryCode)).size !== 10 ||
            !validCount(first.totalIssues) || !validCount(first.unmappedIssues) ||
            !['COMPLETE', 'PARTIAL', 'NO_ISSUES'].includes(first.mappingStatus) ||
            rows.some(row => !row.categoryCode || !row.categoryName || !validCount(row.issueCount) ||
                row.totalIssues !== first.totalIssues || row.unmappedIssues !== first.unmappedIssues || row.mappingStatus !== first.mappingStatus) ||
            rows.reduce((sum, row) => sum + row.issueCount, 0) + first.unmappedIssues !== first.totalIssues ||
            (first.mappingStatus === 'COMPLETE' && first.unmappedIssues !== 0) ||
            (first.mappingStatus === 'NO_ISSUES' && first.totalIssues !== 0))
            throw new Error('Issue classification data is incomplete or does not reconcile. Refresh or contact IAS support.');
        const partial = first.mappingStatus === 'PARTIAL' || first.unmappedIssues > 0;
        target.append(el('p', partial
            ? `Incomplete classification · ${number(first.unmappedIssues)} unmapped of ${number(first.totalIssues)} identified issues. Bars show mapped issues only; zero means no mapped issues in that category.`
            : first.totalIssues === 0 ? 'No eligible issues identified for this audit year and scope.'
            : `Complete classification · ${number(first.totalIssues)} identified issues; 0 unmapped.`, partial ? 'exec-cia-warning' : 'exec-note'));
        const maximum = Math.max(1, ...rows.map(row => row.issueCount));
        const graphic = el('div', null, 'exec-cia-nature-bars');
        rows.sort((a, b) => a.displayOrder - b.displayOrder).forEach(row => {
            const bar = el('div', null, 'exec-cia-nature-row');
            const label = `${row.categoryName}: ${number(row.issueCount)} ${partial ? 'mapped ' : ''}issues`;
            const track = el('div', null, 'exec-track'), fill = el('div', null, 'exec-fill');
            fill.style.width = `${row.issueCount / maximum * 100}%`;
            track.append(fill); track.setAttribute('aria-hidden', 'true');
            bar.setAttribute('aria-label', label);
            bar.append(el('span', row.categoryName), track, el('strong', number(row.issueCount)));
            graphic.append(bar);
        });
        target.append(graphic);
        dataTable(target, ['Category', partial ? 'Mapped issues' : 'Identified issues'], rows.map(row => [row.categoryName, number(row.issueCount)]));
    }
    async function ciaRequest(action, query, signal) {
        const response = await fetch(`${app.dataset.apiUrl}${action}?${query}`, { signal, credentials: 'same-origin', cache: 'no-store',
            headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' } });
        if (response.status === 401 || response.status === 403 || response.redirected)
            throw new Error('CIA session or API permission is unavailable. Reload or ask the IAS administrator to verify the CIA API grants.');
        if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('Unexpected CIA response. Reload or sign in again.');
        const result = await response.json();
        if (!response.ok || result.success !== true) throw new Error(result.message || 'CIA dataset is temporarily unavailable.');
        return result.data;
    }
    async function refreshCia(query, signal, current) {
        ciaKpis(null);
        $('cia-kpi-status').textContent = 'Loading CIA executive KPIs…';
        $('cia-nature').replaceChildren(el('p', 'Loading issue categories…', 'exec-empty'));
        const year = $('audit-year');
        try {
            const years = await ciaRequest('GetCiaAuditYears', query, signal);
            if (current !== version) return;
            if (!Array.isArray(years) || years.some(y => !Number.isSafeInteger(y.auditPeriodId) || y.auditPeriodId <= 0 || !y.startDate || !y.endDate))
                throw new Error('Audit-year lookup is incomplete.');
            const requested = year.value || year.dataset.selectedId;
            year.replaceChildren(new Option('Select audit year', ''), ...years.map(y => new Option(y.description || `${y.startDate.slice(0, 10)} – ${y.endDate.slice(0, 10)}`, y.auditPeriodId)));
            const now = new Date();
            const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
            const preferred = years.find(y => y.startDate.slice(0, 10) <= today && y.endDate.slice(0, 10) >= today)
                || years.find(y => y.startDate.slice(0, 10) <= today) || years[0];
            year.value = requested || (preferred ? String(preferred.auditPeriodId) : '');
            if (!year.value) throw new Error(years.length ? 'Select a valid audit year and choose Apply.' : 'No valid audit years are available.');
            updateNavigation();
            const annualQuery = new URLSearchParams(query);
            annualQuery.set('auditPeriodId', year.value);
            const yearLabel = year.selectedOptions[0].textContent;
            $('cia-nature-period').textContent = `Annual · ${yearLabel} · Uses audit-period dates, independently of optional Start/End Dates.`;
            // Independent failures must not suppress the other dataset or existing dashboard features.
            await Promise.allSettled([
                (async () => {
                    try {
                        const data = await ciaRequest('GetCiaExecKpis', annualQuery, signal);
                        if (current !== version) return;
                        if (data != null && (typeof data !== 'object' || Array.isArray(data) || data.auditPeriodId !== Number(year.value) ||
                            !data.auditPeriodFrom || !data.auditPeriodTo || !data.currentStateAsOf ||
                            ['plannedEntities', 'auditedEntities', 'delayedAuditReports', 'fraudA1Paras', 'awaitingAuditeeResponse', 'teamsConductingAudit'].some(key =>
                                data[key] !== null && (!Number.isSafeInteger(data[key]) || data[key] < 0)) ||
                            (data.planCoveragePct !== null && (!Number.isFinite(data.planCoveragePct) || data.planCoveragePct < 0 || data.planCoveragePct > 100))))
                            throw new Error('CIA executive KPI response is incomplete.');
                        ciaKpis(data, yearLabel);
                        $('cia-kpi-status').textContent = data
                            ? `Annual measures: ${prettyDate(data.auditPeriodFrom.slice(0, 10))} to ${prettyDate(data.auditPeriodTo.slice(0, 10))}. Operational cards are current-state as of ${prettyDate(data.currentStateAsOf.slice(0, 10))}, including prior-year work.`
                            : 'No executive KPI row was returned. Values are unavailable (N/A).';
                    } catch (error) {
                        if (current === version && error.name !== 'AbortError') $('cia-kpi-status').textContent = `${error.message} Choose Apply to retry.`;
                    }
                })(),
                (async () => {
                    try {
                        const rows = await ciaRequest('GetCiaIssueNature', annualQuery, signal);
                        if (current !== version) return;
                        if (!Array.isArray(rows)) throw new Error('Issue-category response is incomplete.');
                        ciaNature(rows);
                    } catch (error) {
                        if (current === version && error.name !== 'AbortError') $('cia-nature').replaceChildren(el('p', `${error.message} Choose Apply to retry.`, 'exec-cia-warning'));
                    }
                })()
            ]);
        } catch (error) {
            if (current !== version || error.name === 'AbortError') return;
            if (year.options.length === 1 && !year.value) year.replaceChildren(new Option('Audit years unavailable — choose Apply to retry', ''));
            $('cia-kpi-status').textContent = `${error.message} Executive KPIs are unavailable (N/A).`;
            $('cia-nature').replaceChildren(el('p', `${error.message} Choose Apply to retry.`, 'exec-cia-warning'));
        }
    }
    let activeRequest, version = 0;
    async function refresh() {
        if (app.dataset.dashboardType === 'GROUP' && !app.dataset.groupRootId) {
            $('status').textContent = 'Select a Group Head entity and choose Load Group to view its dashboard.';
            $('results').hidden = true;
            return;
        }
        const current = ++version;
        activeRequest?.abort(); activeRequest = new AbortController();
        const signal = activeRequest.signal;
        const query = new URLSearchParams();
        // Only Super User pages carry a route-derived perspective. Ordinary
        // accounts continue deriving their perspective/root from the IAS session.
        if (app.dataset.dashboardType) query.set('dashboardType', app.dataset.dashboardType);
        if (app.dataset.groupRootId) query.set('groupRootId', app.dataset.groupRootId);
        if ($('entity').value) query.set('entityId', $('entity').value);
        // These organizational dimensions are independently applied by the Oracle
        // dashboard package, including when the Entity selector is "All".
        const filterValues = new FormData($('filters'));
        for (const key of ['selectBy', 'dimensionId', 'reportingOfficeId']) {
            const value = filterValues.get(key);
            if (value) query.set(key, value);
        }
        // Start/end dates are not applied to the six current-state summary APIs.
        query.set('view', app.dataset.view);
        $('results').hidden = !ciaCommand; app.setAttribute('aria-busy', 'true');
        if (ciaCommand) $('existing-results').hidden = true;
        $('status').textContent = 'Loading authorized dashboard data…';
        $('refresh').disabled = true;
        let ciaTask;
        try {
            ciaTask = ciaCommand ? refreshCia(new URLSearchParams(query), signal, current) : null;
            const actions = app.dataset.datasets.split(',').filter(action => action && !ciaActions.includes(action));
            const data = await Promise.all(actions.map(async action => {
                const actionQuery = new URLSearchParams(query);
                if ((lifecycle && action === 'GetKpiLifecycle') ||
                    (ciaIssues && action === 'GetKpiSummary')) {
                    const start = filterValues.get('startDate');
                    const end = filterValues.get('endDate');
                    // CIA with neither date applies no period predicates in Oracle.
                    if (start && end) {
                        actionQuery.set('startDate', start);
                        actionQuery.set('endDate', end);
                    }
                }
                const response = await fetch(`${app.dataset.apiUrl}${action}?${actionQuery}`, { signal, credentials: 'same-origin', cache: 'no-store',
                    headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' } });
                if (response.status === 401 || response.status === 403 || response.redirected)
                    throw new Error('Your session or dashboard access has changed. Reload or sign in again.');
                if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('Unexpected response. Reload the page or sign in again.');
                const result = await response.json();
                if (!response.ok || result.success !== true) throw new Error(result.message || 'Dashboard data is temporarily unavailable.');
                if (['GetKpiSummary', 'GetKpiLifecycle'].includes(action) ? !result.data || typeof result.data !== 'object' || Array.isArray(result.data) : !Array.isArray(result.data))
                    throw new Error('Dashboard response is incomplete. Please refresh.');
                return [action, result.data];
            }));
            if (current !== version) return;
            const datasets = Object.fromEntries(data);
            if (lifecycle && datasets.GetKpiLifecycle) lifecycleKpis(datasets.GetKpiLifecycle);
            else if (datasets.GetKpiSummary) kpis(datasets.GetKpiSummary);
            if (datasets.GetRiskSummary) risk(datasets.GetRiskSummary);
            if (datasets.GetAgingSummary) bars('aging', datasets.GetAgingSummary);
            if (datasets.GetCycleSummary) bars('cycle', datasets.GetCycleSummary);
            if (datasets.GetSettlementTrend) trend(datasets.GetSettlementTrend);
            if (datasets.GetCriticalItems) {
                criticalRows = datasets.GetCriticalItems; criticalCount = datasets.GetKpiSummary.criticalItems;
                const filter = $('critical-risk'), selected = filter.value;
                filter.replaceChildren(new Option('All risks', ''), ...[...new Set(criticalRows.map(row => row.riskName).filter(Boolean))].sort().map(name => new Option(name, name)));
                if ([...filter.options].some(option => option.value === selected)) filter.value = selected;
                critical(criticalRows, criticalCount);
            }
            $('results').hidden = false;
            if (ciaCommand) $('existing-results').hidden = false;
            const entity = $('entity').selectedOptions[0].textContent;
            $('status').textContent = `${entity} · Current organizational filters applied · Updated ${new Date().toLocaleTimeString()}.`;
            if (lifecycle) $('status').textContent = `${entity} · KPI period: ${filterValues.get('startDate')} to ${filterValues.get('endDate')} · Charts and critical observations use current-state data, without the selected date range · Updated ${new Date().toLocaleTimeString()}.`;
            if (ciaIssues) $('status').textContent = `${entity} · CIA issue events: ${filterValues.get('startDate') && filterValues.get('endDate') ? `${filterValues.get('startDate')} to ${filterValues.get('endDate')}` : 'all eligible history'} · Other cards/charts show current-state information · Updated ${new Date().toLocaleTimeString()}.`;
            if (ciaCommand) {
                $('status').textContent += ' Audit Year applies separately to coverage, A1 paras and issue nature.';
                await ciaTask;
            }
        } catch (error) {
            if (current !== version || error.name === 'AbortError') return;
            if (!ciaCommand) activeRequest.abort();
            $('status').textContent = `${error.message} Use Apply to retry.`;
            if (ciaCommand) await ciaTask;
        } finally {
            if (current === version) { $('refresh').disabled = false; app.setAttribute('aria-busy', 'false'); }
        }
    }
    const form = $('filters');
    function validateDates() {
        const start = $('start-date'), end = $('end-date');
        if (!start || !end) return form.checkValidity();
        end.setCustomValidity((lifecycle || start.value || end.value) && (!start.value || !end.value || start.value > end.value)
            ? 'Provide both dates, with Start Date no later than End Date.' : '');
        return form.checkValidity();
    }
    function updateNavigation() {
        const values = new FormData(form);
        for (const key of ['startDate', 'endDate']) {
            const input = app.querySelector(`form[action] input[type=hidden][name=${key}]`);
            if (input) input.value = values.get(key) || '';
        }
        app.querySelectorAll('[data-view-navigation] a, .exec-next a').forEach(link => {
            const url = new URL(link.href);
            for (const key of ['startDate', 'endDate', 'selectBy', 'dimensionId', 'reportingOfficeId', 'entityId', ...(ciaCommand ? ['auditPeriodId'] : [])]) {
                const value = values.get(key);
                if (value) url.searchParams.set(key, value); else url.searchParams.delete(key);
            }
            link.href = url.href;
        });
    }
    form.addEventListener('submit', event => {
        if (!validateDates()) { event.preventDefault(); form.reportValidity(); }
    });
    // Parent changes reset children before asking the server for the next scoped options.
    for (const [id, children] of [['select-by', ['dimension', 'office', 'entity']], ['dimension', ['office', 'entity']], ['office', ['entity']]]) {
        if (!$(id)) continue; // Reporting Line has no Select By / Dimension controls.
        $(id)?.addEventListener('change', () => {
            activeRequest?.abort(); ++version; $('refresh').disabled = false; app.setAttribute('aria-busy', 'false'); $('results').hidden = true;
            for (const child of children) {
                if (!$(child)) continue;
                $(child).value = '';
                const pending = el('option', 'Choose Apply to reload options'); pending.value = '';
                $(child).replaceChildren(pending);
            }
            updateNavigation();
            if (validateDates()) form.requestSubmit();
            else { $('status').textContent = 'Correct the date range and choose Apply to load authorized filter options.'; form.reportValidity(); }
        });
    }
    for (const id of ['start-date', 'end-date', 'entity', 'audit-year']) $(id)?.addEventListener('change', () => {
        activeRequest?.abort(); ++version; $('refresh').disabled = false; app.setAttribute('aria-busy', 'false');
        if (app.dataset.view !== 'reports') $('results').hidden = true;
        $('status').textContent = 'Filter selections changed. Choose Apply to load this scope.';
        validateDates(); updateNavigation();
    });
    app.querySelectorAll('[data-view-navigation] a, .exec-next a').forEach(link => link.addEventListener('click', event => {
        if (!validateDates()) { event.preventDefault(); form.reportValidity(); }
    }));
    $('critical-risk')?.addEventListener('change', () => critical(criticalRows, criticalCount));
    updateNavigation();
    if (app.dataset.view !== 'reports') refresh();
})();

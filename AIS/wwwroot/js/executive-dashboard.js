(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    if (!app) return;
    const $ = id => document.getElementById(`exec-${id}`);
    const lifecycle = $('kpis')?.dataset.lifecycle === 'true';
    const ciaIssues = $('kpis')?.dataset.ciaIssues === 'true';
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
        // The chart intentionally includes only High/Medium/Low risk.
        // Unknown/unrated values remain in Oracle for separate validation.
        rows = rows.filter(row => [1,2,3].includes(Number(row.riskId)) ||
            ['high','medium','low'].includes(String(row.riskName).toLowerCase()));
        const total = rows.reduce((sum, row) => sum + row.paraCount, 0);
        if (!total) target.append(el('p', 'No rated open observations in this scope.', 'exec-empty'));
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
        dataTable(target, ['Risk', 'Open paras', 'Share of rated paras'],
            rows.map(row => [row.riskName, number(row.paraCount),
                total ? (row.paraCount / total * 100).toFixed(1) + '%' : 'N/A']));
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
        [...(app.dataset.perspective === 'CCO' ? [['Opening Cases', 'openingCases'], ['Para Added', 'paraAdded']] : []), ['Total Cases', 'totalCases'], ['Settled Total', 'settledTotal'], ['Open Paras', 'openParas'], ['High Risk', 'highRiskParas'],
            ['Issue Identified', 'issueIdentified'], ['Issue Rectified', 'issueRectified'], ['Settled Month-to-Date', 'settledMtd'],
            ['Overall Settlement Ratio', 'settlementRatio'], ['MTD Settlement Rate', 'settlementMtdRate'], ['Critical Items', 'criticalItems'], ['Zero Cycle', 'zeroCycleParas'],
            ['Over 3 Cycles', 'over3CycleParas'], ['Over 1 Year', 'over1YearParas']].filter(([, key]) => target.dataset.kpiKeys.split(',').includes(key)).forEach(([label, key]) => {
            const compliance = ['CCO', 'GROUP', 'RLM'].includes(app.dataset.perspective);
            if (compliance) label = ({ totalCases: 'Opening Balance + Para Added', settledTotal: 'Settled in Period',
                settledMtd: 'Settled in Period', settlementRatio: 'Selected-Period Settlement Ratio',
                settlementMtdRate: 'Selected-Period Settlement Rate', openParas: 'Closing Open Paras' })[key] || label;
            if (app.dataset.perspective === 'CCO' && key === 'totalCases') label = 'Total Cases';
            const card = el('div', null, 'exec-kpi');
            const value = data[key];
            card.append(el('span', label), el('strong',
                value == null ? 'N/A' : number(value) +
                (['settlementRatio','settlementMtdRate'].includes(key) ? '%' : '')));
            if (ciaIssues && ['issueIdentified', 'issueRectified'].includes(key)) {
                const dates = new FormData($('filters'));
                card.append(el('small', dates.get('startDate') && dates.get('endDate')
                    ? 'Selected Date Range' : 'All eligible IAS issue history'));
                target.append(card);
                return;
            }
            if (app.dataset.perspective === 'CCO' && ['openingCases','paraAdded','totalCases'].includes(key)) {
                const dates = new FormData($('filters'));
                card.append(el('small', key === 'openingCases'
                    ? `Position at start of ${dates.get('startDate')} · Historical estimate`
                    : `Selected period: ${dates.get('startDate')} to ${dates.get('endDate')}`));
                target.append(card);
                return;
            }
            const denominator = ['totalCases', 'openParas', 'settledTotal'].includes(key) ? data.totalCases : key === 'settledMtd' ? data.settledTotal : data.openParas;
            card.append(el('small', compliance
                ? (['settlementRatio','settlementMtdRate'].includes(key) ? 'Period settled ÷ (opening + period additions)'
                    : ['totalCases','settledTotal','settledMtd'].includes(key) ? 'Selected reporting period' : 'As of End Date')
                : key === 'settlementMtdRate'
                ? 'This month: settled ÷ (opening outstanding + new paras)'
                : key === 'settlementRatio' ? 'All settled ÷ (open + settled)' : denominator > 0 ? `${(data[key] / denominator * 100).toFixed(1)}% of ${key === 'settledMtd' ? 'all settled' : ['totalCases', 'openParas', 'settledTotal'].includes(key) ? 'all cases' : 'open paras'}` : 'Comparison unavailable'));
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
            .filter(([, key]) => $('kpis').dataset.kpiKeys.split(',').includes(key))
            .forEach(([label, key, quality]) => {
                const card = el('div', null, 'exec-kpi');
                const available = quality !== 'unavailable' && data[key] !== null && data[key] !== undefined;
                card.title = quality === 'estimated'
                    ? 'Historical snapshot reconstructed from the available IAS master and events; not a complete historical movement ledger.'
                    : 'Source: IAS Oracle lifecycle';
                card.append(el('span', label), el('strong',
                    available ? number(data[key]) : 'N/A'));
                const note = key === 'openingOpenParas' ? `Position as on ${startLabel}`
                    : key === 'closingOpenParas' ? `Position as on ${endLabel}`
                    : !available ? 'Historical value unavailable'
                    : quality === 'period' ? 'Selected Date Range'
                    : quality === 'estimated'
                        ? 'As of selected date'
                        : 'Current IAS position';
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
        $('critical-note').textContent = `Showing ${number(rows.length)} of ${number(count)} critical paras · Risk: High.`;
        if (!rows.length) {
            const tr = el('tr'), cell = el('td', 'No critical observations in this scope.'); cell.colSpan = app.dataset.perspective === 'CCO' ? 8 : 7; tr.append(cell); target.append(tr);
        }
        rows.forEach(row => {
            const tr = el('tr');
            if (app.dataset.perspective === 'CCO') tr.append(el('td', row.reportingOfficeName || 'Unmapped reporting office'));
            [row.entityName, `${row.auditPeriod ?? 'N/A'} / ${row.paraNo ?? 'N/A'}`, row.riskName,
                row.comCycle].forEach(value => tr.append(el('td', value ?? 'N/A')));
            const age = el('td', row.agingDays == null ? 'N/A' : `${number(row.agingDays)} days`);
            // Expose source chronology instead of guessing corrected dates.
            // Unexpected ages must be reconciled with PARA_ADDED_ON in Oracle.
            age.title = row.paraAddedOn
                ? `IAS Para Added On: ${String(row.paraAddedOn).slice(0, 10)} · Age calculated from this source date`
                : 'IAS Para Added On is missing; source-date review required.';
            tr.append(age, el('td', row.daysSinceLastMovement == null ? 'No movement' : number(row.daysSinceLastMovement)));
            const cell = el('td'), details = el('details');
            details.append(el('summary', `Observation ${row.comId}`), el('p', row.gistOfParas || 'Observation text is unavailable.'));
            cell.append(details); tr.append(cell); target.append(tr);
        });
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
        if ($('entity')?.value) query.set('entityId', $('entity').value);
        // These organizational dimensions are independently applied by the Oracle
        // dashboard package, including when the Entity selector is "All".
        const filterValues = new FormData($('filters'));
        for (const key of ['selectBy', 'dimensionId', 'reportingOfficeId']) {
            const value = filterValues.get(key);
            if (value) query.set(key, value);
        }
        // Compliance applies dates to every dataset; other perspectives retain their contracts.
        query.set('view', app.dataset.view);
        $('results').hidden = true; app.setAttribute('aria-busy', 'true');
        $('status').textContent = 'Loading authorized dashboard data…';
        $('refresh').disabled = true;
        try {
            const actions = app.dataset.datasets.split(',').filter(Boolean);
            const data = await Promise.all(actions.map(async action => {
                const actionQuery = new URLSearchParams(query);
                if (['CCO', 'GROUP', 'RLM'].includes(app.dataset.perspective) || (lifecycle && action === 'GetKpiLifecycle') ||
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
                if (['CCO', 'GROUP', 'RLM'].includes(app.dataset.perspective) && result.dateRangeApplied !== true)
                    throw new Error('Compliance requires a date-aware response for every dataset.');
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
                if (app.dataset.perspective !== 'CCO') filter.replaceChildren(new Option('All risks', ''), ...[...new Set(criticalRows.map(row => row.riskName).filter(Boolean))].sort().map(name => new Option(name, name)));
                if ([...filter.options].some(option => option.value === selected)) filter.value = selected;
                critical(criticalRows, criticalCount);
            }
            // Share the already-authorized Oracle response with role-specific
            // presentation only. No duplicate KPI API request or new access path.
            app.dispatchEvent(new CustomEvent('ias:executive-data', {
                detail: { datasets }
            }));
            $('results').hidden = false;
            const entity = $('entity')?.selectedOptions[0]?.textContent || app.dataset.scopeLabel;
            $('status').textContent = `${entity} · Current-state data; selected date range is not applied · Updated ${new Date().toLocaleTimeString()}.`;
            if (lifecycle) $('status').textContent = app.dataset.view === 'position'
                ? `${entity} · Audit Para Position for ${filterValues.get('startDate')} to ${filterValues.get('endDate')} · Historical opening/closing may be estimated · Updated ${new Date().toLocaleTimeString()}.`
                : `${entity} · KPI period: ${filterValues.get('startDate')} to ${filterValues.get('endDate')} · Charts and critical observations use current-state data, without the selected date range · Updated ${new Date().toLocaleTimeString()}.`;
            if (ciaIssues) $('status').textContent = `${entity} · CIA issue events: ${filterValues.get('startDate') && filterValues.get('endDate') ? `${filterValues.get('startDate')} to ${filterValues.get('endDate')}` : 'all eligible history'} · Other cards/charts show current-state information · Updated ${new Date().toLocaleTimeString()}.`;
            if (['CCO', 'GROUP', 'RLM'].includes(app.dataset.perspective)) $('status').textContent =
                `${entity} · Selected period: ${filterValues.get('startDate')} to ${filterValues.get('endDate')} · Balances and distributions as of End Date; historical values may be estimated.`;
        } catch (error) {
            if (current !== version || error.name === 'AbortError') return;
            activeRequest.abort();
            $('status').textContent = `${error.message} Use Apply to retry.`;
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
        if (['CCO', 'GROUP', 'RLM'].includes(app.dataset.perspective) && start.value && end.value) {
            const a = start.value.split('-').map(Number), b = end.value.split('-').map(Number);
            if ((b[0] - a[0]) * 12 + b[1] - a[1] >= 1200)
                end.setCustomValidity('Select fewer than 1200 calendar months.');
        }
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
            for (const key of ['startDate', 'endDate', 'selectBy', 'dimensionId', 'reportingOfficeId', 'entityId']) {
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
    for (const id of ['start-date', 'end-date', 'entity']) $(id)?.addEventListener('change', () => {
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

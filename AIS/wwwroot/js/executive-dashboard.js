(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    if (!app) return;
    const $ = id => document.getElementById(`exec-${id}`);
    const number = value => Number(value).toLocaleString();
    const colors = ['#c56560', '#d7b16b', '#51937b', '#889890'];
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
        const target = $(id); target.replaceChildren();
        const total = rows.reduce((sum, row) => sum + row.paraCount, 0);
        if (!rows.length || !total) target.append(el('p', 'No open observations in this scope.', 'exec-empty'));
        else {
            const maximum = Math.max(...rows.map(row => row.paraCount));
            const graphic = chart(`${id === 'aging' ? 'Aging' : 'Compliance cycle'} distribution of open observations`);
            rows.forEach((row, index) => {
                const y = 20 + index * (210 / rows.length);
                text(graphic, 0, y + 15, row.bucketName);
                graphic.append(svg('rect', { x: 145, y, width: 330, height: 20, rx: 4, fill: '#edf2ef' }));
                graphic.append(svg('rect', { x: 145, y, width: 330 * row.paraCount / maximum, height: 20, rx: 4, fill: '#51937b' }));
                text(graphic, 550, y + 15, number(row.paraCount), { 'text-anchor': 'end' });
            });
            target.append(graphic);
        }
        dataTable(target, ['Category', 'Open paras', 'High risk'], rows.map(row => [row.bucketName, number(row.paraCount), number(row.highRiskCount)]));
    }
    function risk(rows) {
        const target = $('risk'); target.replaceChildren();
        const total = rows.reduce((sum, row) => sum + row.paraCount, 0);
        if (!total) target.append(el('p', 'No open observations in this scope.', 'exec-empty'));
        else {
            const graphic = chart('Risk distribution of open observations');
            let offset = 0;
            const circumference = 2 * Math.PI * 85;
            rows.forEach((row, index) => {
                const length = row.paraCount / total * circumference;
                graphic.append(svg('circle', { cx: 155, cy: 130, r: 85, fill: 'none', stroke: colors[index % colors.length],
                    'stroke-width': 35, 'stroke-dasharray': `${length} ${circumference - length}`,
                    'stroke-dashoffset': -offset, transform: 'rotate(-90 155 130)' }));
                offset += length;
                graphic.append(svg('circle', { cx: 290, cy: 60 + index * 38, r: 6, fill: colors[index % colors.length] }));
                text(graphic, 310, 65 + index * 38, `${row.riskName}: ${number(row.paraCount)} (${number(row.percentage)}%)`);
            });
            text(graphic, 155, 132, number(total), { 'text-anchor': 'middle', 'font-size': 24 });
            text(graphic, 155, 153, 'Open paras', { 'text-anchor': 'middle' });
            target.append(graphic);
        }
        dataTable(target, ['Risk', 'Open paras', 'Percentage'], rows.map(row => [row.riskName, number(row.paraCount), `${number(row.percentage)}%`]));
    }
    function trend(rows) {
        const target = $('trend'); target.replaceChildren();
        if (!rows.length || !rows.some(row => row.settledParas > 0))
            target.append(el('p', 'No settlements in the returned calendar months.', 'exec-empty'));
        if (rows.length) {
            const graphic = chart('Monthly settled paras and high-risk settlements');
            const maximum = Math.max(1, ...rows.map(row => row.settledParas));
            [0, 0.5, 1].forEach(fraction => {
                const y = 215 - fraction * 180;
                graphic.append(svg('line', { x1: 50, x2: 540, y1: y, y2: y, stroke: '#e1e8e3', 'stroke-dasharray': '4 4' }));
                text(graphic, 43, y + 4, number(maximum * fraction), { 'text-anchor': 'end' });
            });
            const x = index => 60 + index * 470 / Math.max(1, rows.length - 1);
            [['settledParas', '#51937b'], ['highRiskSettled', '#c56560']].forEach(([key, color]) => {
                const points = rows.map((row, index) => `${x(index)},${215 - row[key] / maximum * 180}`).join(' ');
                graphic.append(svg('polyline', { points, fill: 'none', stroke: color, 'stroke-width': 3 }));
                rows.forEach((row, index) => graphic.append(svg('circle', { cx: x(index), cy: 215 - row[key] / maximum * 180, r: 3, fill: color })));
            });
            rows.forEach((row, index) => { if (index % 3 === 0 || index === rows.length - 1) text(graphic, x(index), 241, row.settlementMonth, { 'text-anchor': 'middle', 'font-size': 10 }); });
            target.append(graphic, el('p', 'Green: all settled paras · Red: high-risk settled paras', 'exec-note'));
        }
        dataTable(target, ['Month', 'Settled paras', 'High-risk settled'], rows.map(row => [row.settlementMonth, number(row.settledParas), number(row.highRiskSettled)]));
    }
    function kpis(data) {
        const target = $('kpis'); target.replaceChildren();
        [['Open Paras', 'openParas'], ['High Risk', 'highRiskParas'], ['Settled MTD', 'settledMtd'],
            ['Settlement Ratio', 'settlementRatio'], ['Critical Items', 'criticalItems'], ['Zero Cycle', 'zeroCycleParas'],
            ['Over 3 Cycles', 'over3CycleParas'], ['Over 1 Year', 'over1YearParas']].forEach(([label, key]) => {
            const card = el(key === 'criticalItems' ? 'button' : 'div', null, 'exec-kpi');
            card.append(el('span', label), el('strong', number(data[key]) + (key === 'settlementRatio' ? '%' : '')));
            if (key === 'criticalItems') {
                card.type = 'button'; card.append(el('small', 'View authorized critical observations ↓'));
                card.addEventListener('click', () => { $('critical-panel').scrollIntoView({ behavior: 'smooth', block: 'start' }); $('critical-panel').focus({ preventScroll: true }); });
            }
            target.append(card);
        });
    }
    function critical(rows, count) {
        const target = $('critical'); target.replaceChildren();
        $('critical-note').textContent = `Showing ${number(rows.length)} of ${number(count)} critical items. At most 100 rows, ordered by high risk, age and case ID. Expand an observation to read its text.`;
        if (!rows.length) {
            const tr = el('tr'), cell = el('td', 'No critical observations in this scope.'); cell.colSpan = 7; tr.append(cell); target.append(tr);
        }
        rows.forEach(row => {
            const tr = el('tr');
            [row.entityName, `${row.auditPeriod ?? '—'} / ${row.paraNo ?? '—'}`, row.riskName,
                row.comCycle, row.agingDays, row.daysSinceLastMovement].forEach(value => tr.append(el('td', value ?? '—')));
            const cell = el('td'), details = el('details');
            details.append(el('summary', `Observation ${row.comId}`), el('p', row.gistOfParas || 'Observation text is unavailable.'));
            cell.append(details); tr.append(cell); target.append(tr);
        });
    }
    let activeRequest, version = 0;
    async function refresh() {
        const current = ++version;
        activeRequest?.abort(); activeRequest = new AbortController();
        const signal = activeRequest.signal;
        const query = new URLSearchParams();
        if ($('entity').value) query.set('entityId', $('entity').value);
        if ($('period').value) query.set('auditPeriod', $('period').value);
        $('results').hidden = true; app.setAttribute('aria-busy', 'true');
        $('status').textContent = 'Loading authorized dashboard data…';
        $('refresh').disabled = true;
        try {
            const actions = ['GetKpiSummary', 'GetRiskSummary', 'GetAgingSummary', 'GetCycleSummary', 'GetSettlementTrend', 'GetCriticalItems'];
            const data = await Promise.all(actions.map(async action => {
                const response = await fetch(`${app.dataset.apiUrl}${action}?${query}`, { signal, credentials: 'same-origin', cache: 'no-store',
                    headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' } });
                if (response.status === 401 || response.status === 403 || response.redirected)
                    throw new Error('Your session or dashboard access has changed. Reload or sign in again.');
                if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('Unexpected response. Reload the page or sign in again.');
                const result = await response.json();
                if (!response.ok || result.success !== true) throw new Error(result.message || 'Dashboard data is temporarily unavailable.');
                if (action === 'GetKpiSummary' ? !result.data || typeof result.data !== 'object' : !Array.isArray(result.data))
                    throw new Error('Dashboard response is incomplete. Please refresh.');
                return result.data;
            }));
            if (current !== version) return;
            kpis(data[0]); risk(data[1]); bars('aging', data[2]); bars('cycle', data[3]); trend(data[4]); critical(data[5], data[0].criticalItems);
            $('results').hidden = false;
            const entity = $('entity').selectedOptions[0].textContent;
            const period = $('period').selectedOptions[0].textContent;
            $('status').textContent = `${entity} · ${period} · Updated ${new Date().toLocaleTimeString()}.`;
        } catch (error) {
            if (current !== version || error.name === 'AbortError') return;
            activeRequest.abort();
            $('status').textContent = `${error.message} Use Refresh to retry.`;
        } finally {
            if (current === version) { $('refresh').disabled = false; app.setAttribute('aria-busy', 'false'); }
        }
    }
    $('filters').addEventListener('submit', event => { event.preventDefault(); refresh(); });
    $('entity').addEventListener('change', refresh);
    $('period').addEventListener('change', refresh);
    refresh();
})();

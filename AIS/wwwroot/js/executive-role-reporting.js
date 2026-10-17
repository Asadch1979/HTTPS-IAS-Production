(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    const host = app?.querySelector('[data-role-reporting]');
    const form = document.getElementById('exec-filters');
    if (!app || !host || !form || app.dataset.roleEnabled !== 'true') return;

    const mode = host.dataset.roleMode;
    const number = new Intl.NumberFormat('en-GB');
    const element = (tag, text, className) => {
        const node = document.createElement(tag);
        if (text != null) node.textContent = String(text);
        if (className) node.className = className;
        return node;
    };
    const cell = (row, value, className, risk) => {
        const td = element('td', value == null || value === '' ? 'N/A' : value, className);
        if (risk) td.dataset.risk = risk;
        row.append(td);
        return td;
    };
    function status(selector, value) {
        const node = host.querySelector(selector);
        if (node) node.textContent = value;
    }
    async function read(action, maxRows) {
        const qs = new URLSearchParams(new FormData(form));
        if (app.dataset.dashboardType) qs.set('dashboardType', app.dataset.dashboardType);
        if (app.dataset.groupRootId) qs.set('groupRootId', app.dataset.groupRootId);
        if (maxRows) qs.set('maxRows', String(maxRows));
        const result = await fetch(`${app.dataset.apiUrl}${action}?${qs}`, {
            credentials: 'same-origin', cache: 'no-store',
            headers: {Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest'}
        });
        if (result.redirected || !(result.headers.get('content-type') || '').includes('application/json'))
            throw new Error('Access or IAS session changed. Reload and sign in.');
        const body = await result.json();
        if (!result.ok || body.success !== true || !Array.isArray(body.data))
            throw new Error(body.message || 'Scoped reporting query is unavailable.');
        return body.data;
    }
    function empty(tbody, rows) {
        if (rows.length) return;
        const tr = element('tr');
        const td = cell(tr, 'No matching records within your authorized scope.', 'role-empty');
        td.colSpan = 6; tbody.append(tr);
    }
    function visualEmpty(target, message) {
        if (!target) return;
        target.replaceChildren(element('p', message, 'role-visual-empty'));
    }
    function managementBars(rows) {
        const target = host.querySelector('[data-management-bars]');
        if (!target) return;
        target.replaceChildren();
        const ranked = rows.slice().sort((a, b) =>
            Number(b.openParas || 0) - Number(a.openParas || 0)).slice(0, 8);
        if (!ranked.length) return visualEmpty(target, 'No subordinate unit data for the selected authorized scope.');
        const max = Math.max(1, ...ranked.map(row => Number(row.openParas || 0)));
        for (const item of ranked) {
            const row = element('div', null, 'role-rank-row');
            const label = element('div', null, 'role-rank-label');
            label.append(element('span', item.unitName || 'Unnamed reporting unit'));
            label.append(element('small', `ID ${item.unitId} · ${number.format(item.highRiskOpen || 0)} high-risk open`));
            const track = element('div', null, 'role-rank-track');
            const bar = element('span', null, 'role-rank-bar');
            const width = Math.max(0, Math.min(100, Number(item.openParas || 0) / max * 100));
            bar.style.width = width + '%';
            track.append(bar);
            const value = element('strong', number.format(item.openParas || 0));
            row.append(label, track, value); target.append(row);
        }
        target.append(element('p', 'Top eight units by open-para count; common width scale = largest selected unit. Individual reporting units may contain nested descendants.', 'role-chart-note'));
    }
    function regionHeatmap(rows) {
        const target = host.querySelector('[data-region-heatmap]');
        if (!target) return;
        target.replaceChildren();
        if (!rows.length) return visualEmpty(target, 'No verified descendant branches match the selected IAS filters.');
        const ranked = rows.slice().sort((a,b) =>
            Number(b.highRisk || 0) - Number(a.highRisk || 0) ||
            Number(b.agedOver365 || 0) - Number(a.agedOver365 || 0) ||
            Number(b.openParas || 0) - Number(a.openParas || 0)).slice(0, 36);
        for (const item of ranked) {
            const open = Number(item.openParas || 0);
            const high = Number(item.highRisk || 0);
            const share = open > 0 ? Math.min(100, Math.max(0, high / open * 100)) : 0;
            const band = open === 0 ? 'neutral' : share >= 20 ? 'strong' : share >= 10 ? 'watch' : 'low';
            const tile = element('div', null, 'regional-heat-tile regional-heat-' + band);
            tile.setAttribute('role', 'group');
            tile.setAttribute('aria-label', `${item.entityName || 'Branch ' + item.entityId}: ${high} high-risk of ${open} open paras, ${share.toFixed(1)} percent`);
            tile.append(element('span', item.entityName || ('Branch ' + item.entityId), 'regional-heat-name'));
            tile.append(element('strong', number.format(high), 'regional-heat-count'));
            tile.append(element('small', `${share.toFixed(1)}% high risk · ${number.format(open)} open`));
            const dots = element('div', null, 'regional-heat-extras');
            dots.append(element('span', `${number.format(item.agedOver365 || 0)} >365d`));
            dots.append(element('span', `${number.format(item.over3Cycles || 0)} >3 cycles`));
            tile.append(dots); target.append(tile);
        }
        target.after(); // Heatmap is an overview of returned rows, not a total across nested roots.
        status('[data-role-units-status]', `${rows.length} authorized branch rows returned (displaying the 36 highest ranked exposures).`);
    }
    async function units() {
        const tbody = host.querySelector('[data-role-unit-rows]');
        const visual = host.querySelector(mode === 'regional' ? '[data-region-heatmap]' : '[data-management-bars]');
        try {
            const rows = await read(mode === 'regional' ? 'GetRoleEntityHeatmap' : 'GetRoleUnitComparison',
                mode === 'regional' ? 250 : null);
            tbody.replaceChildren();
            for (const item of rows) {
                const tr = element('tr');
                const name = item.entityName || item.unitName || 'Unnamed entity';
                const id = item.entityId ?? item.unitId;
                cell(tr, `${name} (ID ${id})`, 'role-entity-name');
                cell(tr, number.format(item.openParas || 0));
                const high = mode === 'regional' ? item.highRisk : item.highRiskOpen;
                cell(tr, number.format(high || 0), '', Number(high || 0) > 0 ? 'high' : '');
                cell(tr, number.format(item.agedOver365 || 0));
                cell(tr, number.format(item.over3Cycles || 0));
                cell(tr, mode === 'regional' ? Number(item.highRiskSharePct || 0).toFixed(1) + '%' :
                    number.format(item.settledMtd || 0));
                tbody.append(tr);
            }
            empty(tbody,rows);
            if (mode === 'regional') regionHeatmap(rows);
            else {
                managementBars(rows);
                status('[data-role-units-status]', `${rows.length} authorized organizational units returned.`);
            }
        } catch (error) {
            tbody.replaceChildren();
            visualEmpty(visual, 'Live comparison unavailable: ' + error.message);
            status('[data-role-units-status]', 'Comparison could not be loaded. Retry after checking IAS access.');
        }
    }
    async function followups() {
        const tbody = host.querySelector('[data-role-followup-rows]');
        try {
            const rows = await read('GetRoleFollowups',100);
            tbody.replaceChildren();
            for (const item of rows) {
                const tr = element('tr');
                cell(tr, `${item.entityName || 'Unknown entity'} · ${item.paraNo || 'Para not numbered'}`, 'role-entity-name');
                const risk = item.riskName || (item.riskId == null ? 'Unrated' : String(item.riskId));
                cell(tr,risk,'',item.riskId === 1 ? 'high' : item.riskId === 2 ? 'medium' : '');
                cell(tr,item.agingDays == null ? 'Unknown' : number.format(item.agingDays));
                cell(tr,number.format(item.comCycle || 0));
                cell(tr,(item.priorityReason || 'NORMAL').replaceAll('_',' '),'role-priority');
                cell(tr,(item.gistOfParas || '').slice(0,240));
                tbody.append(tr);
            }
            empty(tbody,rows);
            status('[data-role-followups-status]', `${rows.length} priority items returned (capped at 100; not a complete outstanding balance).`);
        } catch (error) {
            tbody.replaceChildren();
            status('[data-role-followups-status]', 'Follow-up register unavailable: ' + error.message);
        }
    }
    units(); followups();
})();

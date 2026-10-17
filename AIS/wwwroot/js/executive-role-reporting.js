(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    const host = app?.querySelector('[data-role-reporting]');
    const filterForm = document.getElementById('exec-filters');
    if (!app || !host || !filterForm || app.dataset.roleEnabled !== 'true') return;

    const mode = host.dataset.roleMode;
    const nf = new Intl.NumberFormat();
    const cell = (row, value, cls, risk) => {
        const td = document.createElement('td');
        if (cls) td.className = cls;
        if (risk) td.dataset.risk = risk;
        td.textContent = value == null || value === '' ? 'N/A' : String(value);
        row.append(td);
        return td;
    };
    const msg = (selector, value) => {
        const el = host.querySelector(selector);
        if (el) el.textContent = value;
    };
    async function read(action, maxRows) {
        const qs = new URLSearchParams(new FormData(filterForm));
        // These are informational server-side hints; API re-resolves user and scope.
        if (app.dataset.dashboardType) qs.set('dashboardType', app.dataset.dashboardType);
        if (app.dataset.groupRootId) qs.set('groupRootId', app.dataset.groupRootId);
        if (maxRows) qs.set('maxRows', String(maxRows));
        const response = await fetch(`${app.dataset.apiUrl}${action}?${qs}`, {
            credentials: 'same-origin', cache: 'no-store',
            headers: {Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest'}
        });
        if (response.redirected || !(response.headers.get('content-type') || '').includes('application/json'))
            throw new Error('Access or session changed. Reload and sign in again.');
        const json = await response.json();
        if (!response.ok || json.success !== true || !Array.isArray(json.data))
            throw new Error(json.message || 'Reporting query is unavailable.');
        return json.data;
    }
    function empty(tbody, count) {
        if (count) return;
        const tr = document.createElement('tr');
        const td = cell(tr, 'No matching records in the selected authorized scope.', 'role-empty');
        td.colSpan = 6;
        tbody.append(tr);
    }
    async function units() {
        const tbody = host.querySelector('[data-role-unit-rows]');
        try {
            const rows = await read(mode === 'regional' ? 'GetRoleEntityHeatmap' : 'GetRoleUnitComparison', mode === 'regional' ? 250 : null);
            tbody.replaceChildren();
            for (const item of rows) {
                const tr = document.createElement('tr');
                const name = item.entityName || item.unitName || 'Unnamed entity';
                const code = item.entityId ?? item.unitId;
                cell(tr, `${name} (ID ${code})`, 'role-entity-name');
                cell(tr, nf.format(item.openParas || 0));
                cell(tr, nf.format(mode === 'regional' ? item.highRisk || 0 : item.highRiskOpen || 0),
                    '', (mode === 'regional' ? item.highRisk : item.highRiskOpen) > 0 ? 'high' : '');
                cell(tr, nf.format(item.agedOver365 || 0));
                cell(tr, nf.format(item.over3Cycles || 0));
                cell(tr, mode === 'regional' ? `${Number(item.highRiskSharePct || 0).toFixed(1)}%` : nf.format(item.settledMtd || 0));
                tbody.append(tr);
            }
            empty(tbody, rows.length);
            msg('[data-role-units-status]', `${rows.length} verified ${mode === 'regional' ? 'branches (maximum 250)' : 'organizational units'} returned.`);
        } catch (error) {
            tbody.replaceChildren();
            msg('[data-role-units-status]', `Unit comparisons unavailable: ${error.message}`);
        }
    }
    async function followups() {
        const tbody = host.querySelector('[data-role-followup-rows]');
        try {
            const rows = await read('GetRoleFollowups', 100);
            tbody.replaceChildren();
            for (const item of rows) {
                const tr = document.createElement('tr');
                cell(tr, `${item.entityName || 'Unknown entity'} · ${item.paraNo || 'Para not numbered'}`, 'role-entity-name');
                cell(tr, item.riskName || (item.riskId == null ? 'Unrated' : String(item.riskId)),
                    '', item.riskId === 1 ? 'high' : item.riskId === 2 ? 'medium' : '');
                cell(tr, item.agingDays == null ? 'Unknown' : nf.format(item.agingDays));
                cell(tr, nf.format(item.comCycle || 0));
                cell(tr, (item.priorityReason || 'NORMAL').replaceAll('_', ' '));
                cell(tr, (item.gistOfParas || '').slice(0, 240));
                tbody.append(tr);
            }
            empty(tbody, rows.length);
            msg('[data-role-followups-status]', `${rows.length} priority items returned (maximum 100, not a total outstanding balance).`);
        } catch (error) {
            tbody.replaceChildren();
            msg('[data-role-followups-status]', `Follow-ups unavailable: ${error.message}`);
        }
    }
    // The main dashboard already manages the selected filter form and its reload.
    // This sidecar reads the same parameters without altering chart/API contracts.
    units();
    followups();
})();

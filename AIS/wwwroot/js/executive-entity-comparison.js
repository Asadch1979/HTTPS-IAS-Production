(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    const panel = app?.querySelector('[data-entity-comparison]');
    if (!panel) return;
    const button = panel.querySelector('[data-load-entities]');
    const tiles = [...panel.querySelectorAll('[data-entity-id]')];
    let position = 0;
    const number = value => Number(value).toLocaleString();
    async function dataset(action, entityId) {
        const query = new URLSearchParams(new FormData(document.getElementById('exec-filters')));
        query.set('entityId', entityId);
        if (app.dataset.dashboardType) query.set('dashboardType', app.dataset.dashboardType);
        if (app.dataset.groupRootId) query.set('groupRootId', app.dataset.groupRootId);
        const response = await fetch(`${app.dataset.apiUrl}${action}?${query}`, {credentials: 'same-origin', headers: {'Accept': 'application/json'}});
        if (!response.ok) throw new Error(`Request failed (${response.status})`);
        const result = await response.json();
        if (!result.success || result.data == null) throw new Error('Data unavailable');
        return result.data;
    }
    button?.addEventListener('click', async () => {
        button.disabled = true;
        const batch = tiles.slice(position, position + 6);
        let failures = 0;
        // Two requests at a time; never fan out across the entire hierarchy.
        for (const tile of batch) {
            tile.hidden = false;
            const target = tile.querySelector('[data-entity-values]');
            target.textContent = 'Loading verified summaries…';
            try {
                const [kpi, risk] = await Promise.all([dataset('GetKpiSummary', tile.dataset.entityId), dataset('GetRiskSummary', tile.dataset.entityId)]);
                const count = id => risk.filter(r => r.riskId === id).reduce((sum, r) => sum + r.paraCount, 0);
                const high = count(1), medium = count(2), low = count(3);
                const level = !kpi.openParas ? 'none' : high ? 'high' : medium ? 'medium' : low ? 'low' : 'unrated';
                tile.dataset.risk = level;
                target.textContent = `${level === 'none' ? 'No open paras' : level.toUpperCase() + ' risk'} · Open ${number(kpi.openParas)} · High ${number(high)} · Medium ${number(medium)} · Low ${number(low)} · High-risk share ${kpi.openParas ? (high / kpi.openParas * 100).toFixed(1) + '%' : 'N/A'} · Over 365 days ${number(kpi.over1YearParas)} · Settled balance ${number(kpi.settledTotal)} · Settled month-to-date ${number(kpi.settledMtd)} · Settlement ratio ${number(kpi.settlementRatio)}%`;
            } catch (error) {
                target.textContent = `${error.message}. Reload the page to retry. No figures available.`;
                failures++;
            }
        }
        position += batch.length;
        panel.querySelector('[data-comparison-status]').textContent = `${position} of ${tiles.length} entities requested${failures ? ` · ${failures} unavailable in this batch` : ''}.`;
        button.disabled = false;
        button.hidden = position >= tiles.length;
        button.textContent = 'Load next six';
    });
})();

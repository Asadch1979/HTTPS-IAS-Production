(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    const target = document.getElementById('cco-summary');
    if (!app || !target || app.dataset.perspective !== 'CCO') return;
    app.addEventListener('ias:executive-data', event => {
        const data = event.detail.datasets.GetKpiSummary;
        if (!data) return;
        target.replaceChildren();
        [['Settlement rate', 'settlementMtdRate']].forEach(([label, key]) => {
            const card = document.createElement('div'); card.className = 'exec-kpi';
            const title = document.createElement('span'); title.textContent = label;
            const value = document.createElement('strong');
            value.textContent = data[key] == null ? 'N/A' : Number(data[key]).toLocaleString() + (key === 'settlementMtdRate' ? '%' : '');
            card.append(title, value); target.append(card);
        });
    });
    document.getElementById('exec-filters')?.addEventListener('change', () => target.replaceChildren());
})();

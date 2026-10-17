(() => {
    'use strict';
    // Scope: only the approved management view. Values come from the EXISTING
    // authorized KPI cursor response, never a parallel or client-root query.
    const app = document.getElementById('executive-dashboard');
    if (!app || app.dataset.view !== 'management') return;
    const rings = app.querySelectorAll('[data-mgmt-ring]');
    if (!rings.length) return;
    function renderRing(name, numerator, denominator, override) {
        const ring = app.querySelector('[data-mgmt-ring="' + name + '"]');
        const value = app.querySelector('[data-mgmt-ring-value="' + name + '"]');
        if (!ring || !value) return;
        const result = override != null ? Number(override) :
            Number(denominator) > 0 ? Number(numerator) / Number(denominator) * 100 : null;
        if (!Number.isFinite(result)) {
            ring.style.setProperty('--ring-progress', '0%');
            ring.setAttribute('aria-label', name + ': no verified denominator available');
            value.textContent = 'N/A';
            return;
        }
        const percent = Math.max(0, Math.min(100, result));
        ring.style.setProperty('--ring-progress', percent.toFixed(2) + '%');
        value.textContent = percent.toFixed(1) + '%';
        ring.setAttribute('aria-label', name + ': ' + percent.toFixed(1) + ' percent');
    }
    app.addEventListener('ias:executive-data', event => {
        const data = event.detail?.datasets?.GetKpiSummary;
        if (!data) return;
        const open = Number(data.openParas || 0);
        // IAS already computes settlement ratio on its authorized full scope.
        renderRing('settlement', null, null, data.settlementRatio);
        renderRing('risk', data.highRiskParas, open);
        renderRing('aging', data.over1YearParas, open);
    });
})();

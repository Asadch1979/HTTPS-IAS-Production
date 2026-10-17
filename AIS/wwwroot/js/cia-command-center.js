(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    const main = app?.querySelector('.cia-command-center');
    const form = document.getElementById('exec-filters');
    const period = document.getElementById('cia-period');
    const status = document.getElementById('exec-status');
    const results = document.getElementById('exec-results');
    if (!main || !form || !status || !results) return;
    results.hidden = false;
    if (main.dataset.commandEnabled !== 'true' || !period?.value) {
        status.textContent = main.dataset.commandEnabled !== 'true'
            ? 'CIA Command Center · Oracle activation pending verification.'
            : 'No authorized audit period is available.';
        return;
    }
    let controller = null, version = 0;
    const number = value => value == null ? 'N/A' : Number(value).toLocaleString('en-GB');
    const decimal = value => value == null ? 'N/A' : Number(value).toFixed(1) + '%';
    async function query(action, params, signal) {
        const response = await fetch(`${app.dataset.apiUrl}${action}?${params}`, {
            credentials: 'same-origin', cache: 'no-store', signal,
            headers: {Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest'}
        });
        if (response.redirected || !String(response.headers.get('content-type') || '').includes('application/json'))
            throw new Error('IAS session or dashboard access changed.');
        const data = await response.json();
        if (!response.ok || data.success !== true) throw new Error(data.message || 'Oracle reporting unavailable.');
        return data.data;
    }
    function renderKpis(data) {
        for (const field of ['delayedAuditReports','fraudA1Paras','awaitingAuditeeResponse','teamsConductingAudit'])
            main.querySelector('[data-cia-kpi="' + field + '"]').textContent = number(data[field]);
        main.querySelector('[data-cia-kpi="planCoveragePct"]').textContent = decimal(data.planCoveragePct);
        main.querySelector('[data-cia-plan]').textContent =
            `${number(data.auditedEntities)} audited of ${number(data.plannedEntities)} planned entities`;
        main.querySelector('[data-cia-as-of]').textContent =
            `Audit period: ${String(data.auditPeriodFrom || '').slice(0,10)} – ${String(data.auditPeriodTo || '').slice(0,10)} · Current operational position as of ${String(data.currentStateAsOf || '').slice(0,10)}.`;
    }
    function renderNature(rows) {
        if (rows.length !== 10) throw new Error('Issue nature must return exactly ten categories.');
        const holder = main.querySelector('[data-cia-nature-bars]');
        holder.replaceChildren();
        const totals = rows[0], complete = rows.every(r =>
            r.mappingStatus === 'COMPLETE' || r.mappingStatus === 'NO_ISSUES');
        const identified = Number(totals.totalIssues || 0);
        const unmapped = Number(totals.unmappedIssues || 0);
        const mapped = rows.reduce((sum, r) => sum + Number(r.issueCount || 0), 0);
        const quality = main.querySelector('[data-cia-mapping-status]');
        quality.textContent = complete && mapped + unmapped === identified
            ? `Classification verified · ${number(identified)} issues`
            : `Partial mapping · ${number(unmapped)} unmapped of ${number(identified)} issues`;
        quality.dataset.complete = String(complete && mapped + unmapped === identified);
        const denominator = Math.max(identified, 1);
        for (const item of rows) {
            const row = document.createElement('div'); row.className = 'cia-nature-row';
            const label = document.createElement('span'); label.textContent = item.categoryName;
            const track = document.createElement('div'); track.className = 'cia-nature-track';
            const fill = document.createElement('span'); fill.className = 'cia-nature-fill';
            fill.style.width = (Number(item.issueCount || 0) / denominator * 100).toFixed(2) + '%';
            track.append(fill);
            const value = document.createElement('strong'); value.textContent = number(item.issueCount);
            row.append(label, track, value);
            holder.append(row);
        }
        main.querySelector('[data-cia-unmapped]').textContent =
            !complete || mapped + unmapped !== identified
                ? `Only ${number(mapped)} mapped observations are classified; ${number(unmapped)} remain unmapped or require reconciliation. This is not a complete distribution.`
                : 'All eligible issues are classified in the ten approved categories.';
    }
    async function load() {
        if (!period.value) return;
        controller?.abort(); controller = new AbortController();
        const current = ++version, selectedPeriod = period.value;
        const qs = new URLSearchParams(new FormData(form));
        qs.set('view','command'); qs.set('auditPeriodId',selectedPeriod);
        if (app.dataset.dashboardType) qs.set('dashboardType',app.dataset.dashboardType);
        if (app.dataset.groupRootId) qs.set('groupRootId',app.dataset.groupRootId);
        status.textContent = 'Loading CIA Command Center…';
        app.setAttribute('aria-busy', 'true');
        try {
            const [kpis, categories] = await Promise.all([
                query('GetCiaCommandKpis',qs,controller.signal),
                query('GetCiaIssueNature',qs,controller.signal)
            ]);
            if (current !== version || !kpis || !Array.isArray(categories))
                throw new Error('Incomplete CIA command data.');
            renderKpis(kpis);
            renderNature(categories);
            status.textContent = 'CIA Command Center · Approved IAS audit period · Updated ' +
                new Date().toLocaleTimeString();
        } catch (error) {
            if (current !== version || error.name === 'AbortError') return;
            status.textContent = 'CIA Command Center data unavailable: ' + error.message;
        } finally {
            if (current === version) app.setAttribute('aria-busy', 'false');
        }
    }
    period.addEventListener('change', () => {
        const hidden = form.querySelector('[name=auditPeriodId]');
        if (hidden) hidden.value = period.value;
        const url = new URL(location.href);
        url.searchParams.set('auditPeriodId',period.value);
        history.replaceState(null,'',url);
        load();
    });
    form.addEventListener('submit', () => {
        const hidden = form.querySelector('[name=auditPeriodId]');
        if (hidden) hidden.value = period.value;
    });
    load();
})();

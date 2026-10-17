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
        // Preserve the approved ten labels in Razor even when Oracle is disabled
        // or fails. Render only verified numeric results from the CIA cursor.
        if (rows.length !== 10) throw new Error('The Nature of Issue cursor did not return all ten categories.');
        const cards = [...main.querySelectorAll('[data-cia-category]')];
        if (cards.length !== 10) throw new Error('The CIA nature-of-issue design is incomplete.');
        const known = new Map(cards.map(card => [card.dataset.ciaCategory, card]));
        const seen = new Set();
        const totals = rows[0];
        const identified = Number(totals.totalIssues);
        const unmapped = Number(totals.unmappedIssues);
        if (!Number.isSafeInteger(identified) || identified < 0 || !Number.isSafeInteger(unmapped) ||
            unmapped < 0 || unmapped > identified)
            throw new Error('Invalid IAS issue-classification totals.');
        let mapped = 0;
        for (const item of rows) {
            const card = known.get(item.categoryCode);
            const count = Number(item.issueCount);
            if (!card || seen.has(item.categoryCode) || !Number.isSafeInteger(count) || count < 0 ||
                Number(item.totalIssues) !== identified || Number(item.unmappedIssues) !== unmapped)
                throw new Error('IAS category mapping returned inconsistent or unknown rows.');
            seen.add(item.categoryCode);
            mapped += count;
        }
        if (seen.size !== known.size) throw new Error('A required nature-of-issue category is missing.');

        for (const item of rows) {
            const card = known.get(item.categoryCode);
            const share = identified > 0 ? item.issueCount / identified * 100 : 0;
            card.querySelector('[data-cia-category-count]').textContent = number(item.issueCount);
            card.querySelector('[data-cia-category-fill]').style.width = Math.min(100, share).toFixed(2) + '%';
            card.querySelector('[data-cia-category-share]').textContent =
                identified > 0 ? share.toFixed(1) + '% of issues identified' : 'No identified issues';
            card.setAttribute('aria-label', `${item.categoryName}: ${number(item.issueCount)} identified issues`);
        }
        const complete = rows.every(r => r.mappingStatus === 'COMPLETE' || r.mappingStatus === 'NO_ISSUES') &&
            mapped + unmapped === identified && unmapped === 0;
        const consistent = mapped + unmapped === identified;
        const quality = main.querySelector('[data-cia-mapping-status]');
        quality.textContent = !consistent
            ? 'Classification totals require reconciliation'
            : complete ? 'All issues categorized' :
                'Partial mapping · ' + number(unmapped) + ' unmapped';
        quality.dataset.complete = String(complete);
        const pct = identified > 0 ? mapped / identified * 100 : null;
        const ring = main.querySelector('[data-cia-nature-ring]');
        ring.style.setProperty('--cia-mapped-percent', pct == null ? '0%' : Math.min(100,pct).toFixed(2) + '%');
        ring.setAttribute('aria-label', pct == null ? 'No identified issues to classify' :
            pct.toFixed(1) + ' percent of identified IAS issues assigned to an approved category');
        main.querySelector('[data-cia-mapped-pct]').textContent = pct == null ? 'N/A' : pct.toFixed(1) + '%';
        main.querySelector('[data-cia-total]').textContent = number(identified);
        main.querySelector('[data-cia-mapped]').textContent = number(mapped);
        main.querySelector('[data-cia-unmapped-total]').textContent = number(unmapped);
        main.querySelector('[data-cia-unmapped]').textContent = !consistent
            ? 'Category counts do not reconcile to the IAS identified-issue total; review the Oracle mapping.'
            : unmapped > 0
                ? `${number(unmapped)} issues remain unmapped. The bars exclude these records and must not be treated as a complete classification.`
                : identified === 0
                    ? 'No eligible issues were identified in the selected IAS audit period.'
                    : 'All identified issues are assigned to one of the ten approved IAS categories.';
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
            const quality = main.querySelector('[data-cia-mapping-status]');
            if (quality) {
                quality.textContent = 'Issue classification unavailable';
                quality.dataset.complete = 'false';
            }
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

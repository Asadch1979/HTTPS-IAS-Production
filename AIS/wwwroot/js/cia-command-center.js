(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    const main = app?.querySelector('.cia-command-center');
    const form = document.getElementById('exec-filters');
    const period = document.getElementById('cia-period');
    const status = document.getElementById('exec-status');
    const results = document.getElementById('exec-results');
    if (!main || !form || !status || !results || !period) return;
    results.hidden = false;
    let controller = null, version = 0;
    const states = { kpi: 'idle', nature: 'idle' };
    const number = value => value == null ? 'N/A' : Number(value).toLocaleString('en-GB');
    const decimal = value => value == null ? 'N/A' : Number(value).toFixed(1) + '%';
    async function query(action, params, signal) {
        const response = await fetch(`${app.dataset.apiUrl}${action}?${params}`, {
            credentials: 'same-origin', cache: 'no-store', signal,
            headers: {Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest'}
        });
        const reference = response.headers.get('X-CIA-Request-Id');
        const suffix = reference ? ' Request: ' + reference + '.' : '';
        const messages = {
            400: 'The selected audit period or filters were rejected.',
            401: 'Your IAS session expired. Sign in again.',
            403: 'CIA page or API permission is missing.',
            500: 'The server could not load this section.',
            503: 'Reporting is unavailable or disabled in this environment.'
        };
        if (response.redirected) throw new Error('IAS session changed. Sign in and reload.' + suffix);
        const failure = messages[response.status] || 'Reporting request failed.';
        if (!String(response.headers.get('content-type') || '').includes('application/json'))
            throw new Error('HTTP ' + response.status + ' · ' + failure + ' Expected a JSON response.' + suffix);
        let data;
        try { data = await response.json(); }
        catch { throw new Error('HTTP ' + response.status + ' · Invalid JSON response.' + suffix); }
        if (!response.ok || data?.success !== true)
            throw new Error('HTTP ' + response.status + ' · ' + (data?.message || failure) + suffix);
        if (data.data == null) throw new Error('The section returned no data.' + suffix);
        return data.data;
    }
    function renderKpis(data, selectedPeriod) {
        if (!data || Number(data.auditPeriodId) !== Number(selectedPeriod))
            throw new Error('The KPI result does not match the selected IAS period.');
        for (const field of ['plannedEntities','auditedEntities','delayedAuditReports','fraudA1Paras','awaitingAuditeeResponse','teamsConductingAudit']) {
            if (!(field in data) || data[field] != null && (!Number.isSafeInteger(data[field]) || data[field] < 0))
                throw new Error('Invalid KPI count returned by IAS.');
        }
        if (!('planCoveragePct' in data) || data.planCoveragePct != null &&
            (!Number.isFinite(data.planCoveragePct) || data.planCoveragePct < 0 || data.planCoveragePct > 100))
            throw new Error('Invalid plan coverage returned by IAS.');
        for (const field of ['delayedAuditReports','fraudA1Paras','awaitingAuditeeResponse','teamsConductingAudit'])
            main.querySelector('[data-cia-kpi="' + field + '"]').textContent = number(data[field]);
        main.querySelector('[data-cia-kpi="planCoveragePct"]').textContent = decimal(data.planCoveragePct);
        main.querySelector('[data-cia-plan]').textContent =
            `${number(data.auditedEntities)} audited of ${number(data.plannedEntities)} planned entities`;
        for (const note of main.querySelectorAll('[data-cia-workflow-note]')) {
            note.textContent = data.delayedAuditReports == null || data.teamsConductingAudit == null
                ? 'N/A · IAS deadline and active-stage verification pending.'
                : 'Current state · Verified IAS reporting deadline and audit stages.';
            note.title = data.workflowVerificationNote || 'Workflow definitions require verification.';
        }
        main.querySelector('[data-cia-as-of]').textContent =
            `Audit period: ${String(data.auditPeriodFrom || '').slice(0,10)} – ${String(data.auditPeriodTo || '').slice(0,10)} · Current operational position as of ${String(data.currentStateAsOf || '').slice(0,10)}.`;
    }
    function renderNature(rows) {
        // Oracle still returns ten source categories; the business presentation
        // intentionally excludes ACCOUNTS and IS_AUDIT. All ten counts remain
        // in reconciliation and mapping-coverage totals.
        if (!Array.isArray(rows) || rows.length !== 10) throw new Error('The Nature of Issue cursor did not return all ten categories.');
        const cards = [...main.querySelectorAll('[data-cia-category]')];
        if (cards.length !== 8) throw new Error('Expected eight visible CIA nature categories.');
        const hiddenCodes = new Set(['ACCOUNTS','IS_AUDIT']);
        const known = new Map(cards.map(card => [card.dataset.ciaCategory, card]));
        const fullCodes = new Set([...known.keys(),...hiddenCodes]);
        if (fullCodes.size !== 10) throw new Error('CIA category configuration is invalid.');
        const seen = new Set();
        const totals = rows[0];
        const identified = totals.totalIssues;
        const unmapped = totals.unmappedIssues;
        if (!Number.isSafeInteger(identified) || identified < 0 || !Number.isSafeInteger(unmapped) ||
            unmapped < 0 || unmapped > identified)
            throw new Error('Invalid IAS issue-classification totals.');
        let mapped = 0, hiddenCount = 0;
        for (const item of rows) {
            const card = known.get(item.categoryCode);
            const count = item.issueCount;
            if (!fullCodes.has(item.categoryCode) || seen.has(item.categoryCode) ||
                !Number.isSafeInteger(count) || count < 0 ||
                item.totalIssues !== identified || item.unmappedIssues !== unmapped)
                throw new Error('IAS category mapping returned inconsistent or unknown rows.');
            seen.add(item.categoryCode);
            mapped += count;
            if (hiddenCodes.has(item.categoryCode)) hiddenCount += count;
        }
        if (seen.size !== fullCodes.size) throw new Error('A required IAS nature category is missing.');
        if (mapped + unmapped !== identified) throw new Error('IAS category counts do not reconcile.');

        for (const item of rows) {
            const card = known.get(item.categoryCode);
            if (!card) continue; // hidden category stays in full totals
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
        main.querySelector('[data-cia-hidden]').textContent = number(hiddenCount);
        main.querySelector('[data-cia-unmapped-total]').textContent = number(unmapped);
        main.querySelector('[data-cia-unmapped]').textContent = !consistent
            ? 'Category counts do not reconcile to the IAS identified-issue total; review the Oracle mapping.'
            : unmapped > 0
                ? `${number(unmapped)} issues remain unmapped. The bars exclude these records and must not be treated as a complete classification.`
                : identified === 0
                    ? 'No eligible issues were identified in the selected IAS audit period.'
                    : 'All identified issues are classified; the chart shows eight of the ten source categories.';
    }
    async function load() {
        controller?.abort(); controller = new AbortController();
        const request = controller;
        const current = ++version, selectedPeriod = period.value;
        if (main.dataset.commandEnabled !== 'true' || !selectedPeriod) {
            const reason = main.dataset.commandEnabled !== 'true'
                ? 'Data loading is disabled in this environment.'
                : 'Select an IAS audit period to load results.';
            clearKpis(reason); clearNature(reason);
            setSectionState('kpi','unavailable',reason);
            setSectionState('nature','unavailable',reason);
            app.setAttribute('aria-busy','false');
            return;
        }
        const qs = new URLSearchParams(new FormData(form));
        qs.set('view','command'); qs.set('auditPeriodId',selectedPeriod);
        if (app.dataset.dashboardType) qs.set('dashboardType',app.dataset.dashboardType);
        if (app.dataset.groupRootId) qs.set('groupRootId',app.dataset.groupRootId);
        clearKpis('Loading selected IAS period…');
        clearNature('Loading issue classification…');
        setSectionState('kpi','loading','Loading KPI cards…');
        setSectionState('nature','loading','Loading issue categories…');
        app.setAttribute('aria-busy', 'true');
        async function section(name, action, render) {
            try {
                const data = await query(action,qs,request.signal);
                if (current !== version || request.signal.aborted) return;
                render(data,selectedPeriod);
                const empty = name === 'nature' && data[0].totalIssues === 0;
                setSectionState(name,empty ? 'empty' : 'success',
                    empty ? 'No eligible issues in this audit period.' : name === 'kpi' ? 'KPI cards loaded' : 'Issue categories loaded');
            } catch (error) {
                if (current !== version || request.signal.aborted || error.name === 'AbortError') return;
                if (name === 'kpi') clearKpis('KPI data unavailable.');
                else {
                    clearNature('Issue classification unavailable');
                    main.querySelector('[data-cia-mapping-status]').textContent = 'Issue classification unavailable';
                }
                setSectionState(name,'error',(name === 'kpi' ? 'KPI cards: ' : 'Issue categories: ') + error.message);
            }
        }
        await Promise.allSettled([
            section('kpi','GetCiaCommandKpis',renderKpis),
            section('nature','GetCiaIssueNature',renderNature)
        ]);
        if (current === version) app.setAttribute('aria-busy','false');
    }
    function setSectionState(name, state, message) {
        states[name] = state;
        const label = main.querySelector('[data-cia-' + name + '-status]');
        label.textContent = message; label.dataset.state = state;
        const section = name === 'kpi' ? main.querySelector('[data-cia-kpi-section]') : main.querySelector('.cia-command-nature');
        section.dataset.state = state;
        section.setAttribute('aria-busy',String(state === 'loading'));
        const describe = (key, title) => title + (states[key] === 'loading' ? ' loading' :
            states[key] === 'success' || states[key] === 'empty' ? ' loaded' : ' unavailable');
        status.textContent = describe('kpi','KPI cards') + ' · ' + describe('nature','Issue categories');
    }
    function clearKpis(note) {
        for (const field of main.querySelectorAll('[data-cia-kpi]'))
            field.textContent = 'N/A';
        main.querySelector('[data-cia-plan]').textContent = note;
        main.querySelector('[data-cia-as-of]').textContent = note;
    }
    function clearNature(note) {
        for (const field of main.querySelectorAll('[data-cia-category-count], [data-cia-total], [data-cia-mapped], [data-cia-hidden], [data-cia-unmapped-total], [data-cia-mapped-pct]'))
            field.textContent = 'N/A';
        for (const fill of main.querySelectorAll('[data-cia-category-fill]')) fill.style.width = '0%';
        for (const label of main.querySelectorAll('[data-cia-category-share]')) label.textContent = note;
        for (const card of main.querySelectorAll('[data-cia-category]'))
            card.setAttribute('aria-label',card.dataset.ciaCategory + ': ' + note);
        main.querySelector('[data-cia-unmapped]').textContent = note;
        const quality = main.querySelector('[data-cia-mapping-status]');
        quality.textContent = note; quality.dataset.complete = 'false';
        const ring = main.querySelector('[data-cia-nature-ring]');
        ring.style.setProperty('--cia-mapped-percent','0%');
        ring.setAttribute('aria-label',note);
    }
    period.addEventListener('change', () => {
        const hidden = form.querySelector('[name=auditPeriodId]');
        if (hidden) hidden.value = period.value;
        const selected = app.querySelector('[data-cia-selected-period]');
        if (selected) selected.textContent = 'Selected: ' + (period.selectedOptions[0]?.textContent || '');
        const reset = app.querySelector('[data-cia-reset]');
        if (reset) {
            const resetUrl = new URL(reset.href);
            resetUrl.searchParams.set('auditPeriodId',period.value);
            reset.href = resetUrl.href;
        }
        const url = new URL(location.href);
        url.searchParams.set('auditPeriodId',period.value);
        history.replaceState(null,'',url);
        load();
    });
    form.addEventListener('submit', () => {
        const hidden = form.querySelector('[name=auditPeriodId]');
        if (hidden) hidden.value = period.value;
    });
    for (const [id,children] of [
        ['exec-select-by',['exec-dimension','exec-office','exec-entity']],
        ['exec-dimension',['exec-office','exec-entity']],
        ['exec-office',['exec-entity']]
    ]) document.getElementById(id)?.addEventListener('change', () => {
        for (const child of children) document.getElementById(child)?.replaceChildren(new Option('All matching options',''));
        form.requestSubmit();
    });
    load();
})();

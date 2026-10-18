(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    if (!app?.classList.contains('exec-snap')) return;
    const form = document.getElementById('exec-filters');
    const start = document.getElementById('exec-start-date');
    const end = document.getElementById('exec-end-date');
    const status = document.getElementById('exec-status');
    const reports = app.dataset.view === 'reports';
    const number = value => value == null ? 'N/A' : Number(value).toLocaleString('en-GB');
    const element = (tag, value, cls) => {
        const node = document.createElement(tag);
        if (value != null) node.textContent = value;
        if (cls) node.className = cls;
        return node;
    };
    function validDates() {
        end.setCustomValidity('');
        if (!start.value || !end.value) return false;
        if (start.value > end.value) end.setCustomValidity('End Date must be on or after Start Date.');
        else if (end.value > end.max) end.setCustomValidity('End Date cannot be in the future.');
        else {
            const a = start.value.split('-').map(Number), b = end.value.split('-').map(Number);
            if ((b[0] - a[0]) * 12 + b[1] - a[1] >= 1200)
                end.setCustomValidity('Select fewer than 1200 calendar months.');
        }
        return form.checkValidity();
    }
    function navigation() {
        for (const anchor of app.querySelectorAll('[data-view-navigation] a')) {
            const url = new URL(anchor.href);
            for (const [key,value] of new FormData(form)) {
                if (key !== 'view' && key !== 'report') {
                    if (value) url.searchParams.set(key,value); else url.searchParams.delete(key);
                }
            }
            anchor.href = url;
        }
    }
    let request = null;
    function resetResults(note) {
        for (const card of app.querySelectorAll('[data-snap-card]')) {
            card.querySelector('[data-snap-kpi]').textContent = 'N/A';
            card.querySelector('[data-snap-kpi-note]').textContent = note;
        }
        for (const name of ['risk','aging','cycle','trend']) {
            const target = document.getElementById('exec-' + name);
            if (target) target.replaceChildren(element('p',note,'exec-empty'));
        }
        document.getElementById('exec-critical')?.replaceChildren();
        const criticalNote = document.getElementById('exec-critical-note');
        if (criticalNote) criticalNote.textContent = note;
    }
    form.addEventListener('submit', event => {
        if (!validDates()) { event.preventDefault(); form.reportValidity(); }
    });
    // The approved report client handles its own invalidation and cascades.
    // This handler only shares Snap's extra date rules and updates tab dates.
    form.addEventListener('change', () => {
        validDates(); navigation();
        const period = app.querySelector('[data-snap-period]');
        period.textContent = 'Requested period: ' + start.value + ' – ' + end.value + ' · Apply to refresh results.';
        if (!reports) {
            request?.abort();
            app.setAttribute('aria-busy','false');
            resetResults('Filters changed. Apply to load this scope and period.');
            status.textContent = 'Filters changed. Choose Apply.';
        }
    });
    navigation();
    if (app.dataset.enabled !== 'true') {
        resetResults('N/A · Oracle activation pending compilation and approval.');
        return;
    }
    if (reports) return;
    async function read(action, signal) {
        const query = new URLSearchParams(new FormData(form));
        const response = await fetch(app.dataset.apiUrl + action + '?' + query, {
            signal, credentials:'same-origin', cache:'no-store',
            headers:{Accept:'application/json','X-Requested-With':'XMLHttpRequest'}
        });
        if (response.redirected || !(response.headers.get('content-type') || '').includes('application/json'))
            throw new Error('IAS session or reporting access changed.');
        const body = await response.json();
        if (!response.ok || body.success !== true || body.dateRangeApplied !== true ||
            body.startDate !== start.value || body.endDate !== end.value)
            throw new Error(body.message || 'A consistent dated result was not returned.');
        return body.data;
    }
    function renderKpis(data) {
        for (const card of app.querySelectorAll('[data-snap-card]')) {
            const key = card.dataset.snapCard;
            const opening = key === 'openingOpenParas';
            const position = opening || ['closingOpenParas','highRiskOpenParas','over1YearParas','over3ComplianceCycles'].includes(key);
            const verified = opening ? data.historicalOpeningVerified === true : data.closingAsofVerified === true;
            const estimated = opening ? data.openingEstimated === true : data.closingEstimated === true;
            const available = !position || verified || estimated;
            const value = available ? data[key] : null;
            if (value != null && (!Number.isSafeInteger(value) || value < 0)) throw new Error('Invalid lifecycle count.');
            card.querySelector('[data-snap-kpi]').textContent = (value != null && position && estimated ? '≈ ' : '') + number(value);
            card.querySelector('[data-snap-kpi-note]').textContent = value == null ? 'N/A · Historical evidence unavailable'
                : position ? estimated ? key === 'over3ComplianceCycles'
                    ? 'Estimated · present cycles on estimated past open population'
                    : 'Estimated · latest master dates/status; present risk and organization'
                    : 'Verified current position · as of now'
                : 'Qualifying event dates within selected range';
        }
    }
    function renderDistribution(rows) {
        if (!Array.isArray(rows)) throw new Error('Invalid distribution cursor.');
        for (const name of ['risk','aging','cycle','trend']) {
            const target = document.getElementById('exec-' + name);
            if (!target) continue;
            const subset = rows.filter(row => row.dataset === name);
            if (!subset.length) throw new Error('Missing ' + name + ' analysis.');
            const table = element('table',null,'exec-table snap-bucket-table');
            const head = element('thead'), header = element('tr'), body = element('tbody');
            header.append(element('th', name === 'trend' ? 'Month' : 'Category'),element('th','Paras'),element('th','Evidence'));
            for (const th of header.children) th.scope = 'col';
            head.append(header);
            for (const row of subset) {
                if (!['Y','E','N'].includes(row.qualityCode) || row.paraCount != null &&
                    (!Number.isSafeInteger(row.paraCount) || row.paraCount < 0)) throw new Error('Invalid dated analysis.');
                const tr = element('tr'), label = element('th',row.bucketName); label.scope = 'row';
                tr.append(label,element('td',row.qualityCode === 'N' ? 'N/A' : number(row.paraCount)),
                    element('td',row.qualityCode === 'N' ? 'History unavailable' : row.qualityCode === 'E' ? 'Estimated' : 'Current as of now','snap-quality'));
                body.append(tr);
            }
            table.append(head,body);
            const bars = element('div',null,'exec-bars');
            const maximum = Math.max(1,...subset.map(row => Number(row.paraCount) || 0));
            for (const row of subset) {
                const line = element('div',null,'exec-bar-row'), track = element('div',null,'exec-track');
                const fill = element('span',null,'exec-fill');
                fill.style.width = row.qualityCode === 'N' ? '0%' : Math.max(0,Number(row.paraCount) / maximum * 100) + '%';
                track.append(fill);
                line.append(element('span',row.bucketName),track,element('strong',row.qualityCode === 'N' ? 'N/A' : number(row.paraCount)));
                bars.append(line);
            }
            const note = name === 'cycle' ? 'Past cycle histories are not verified. No current distribution is substituted.'
                : name === 'trend' ? 'Selected months and inclusive dates only. Latest qualifying settlement dates can omit earlier settlements after a reopening.'
                : 'Historical position is estimated; risk and organization use present assignments. Age is calculated at End Date.';
            target.replaceChildren(bars,table,element('p',note,'exec-note'));
        }
    }
    let criticalRows = [];
    function renderCritical() {
        const target = document.getElementById('exec-critical');
        const selectedRisk = document.getElementById('exec-critical-risk').value;
        target.replaceChildren();
        for (const row of criticalRows.filter(row => !selectedRisk || row.riskName === selectedRisk)) {
            const tr = element('tr'), entity = element('td');
            entity.textContent = row.entityName || 'Unknown entity';
            tr.append(entity,element('td',row.paraNo || 'Unnumbered'),element('td',row.riskName),
                element('td',number(row.comCycle)),element('td',number(row.agingDays)),
                element('td','N/A'),element('td',row.gistOfParas || ''));
            target.append(tr);
        }
        if (!target.children.length) {
            const tr = element('tr'), cell = element('td','No matching critical items in the returned list.');
            cell.colSpan = 7; tr.append(cell); target.append(tr);
        }
    }
    function loadCritical(rows) {
        if (!Array.isArray(rows) || rows.length > 250) throw new Error('Invalid critical-item cursor.');
        criticalRows = rows;
        const risk = document.getElementById('exec-critical-risk');
        risk.replaceChildren(new Option('All risks',''));
        for (const value of [...new Set(rows.map(row => row.riskName))]) risk.add(new Option(value,value));
        renderCritical();
        document.getElementById('exec-critical-note').textContent =
            rows.length + ' items returned (maximum 100). Past lists are Estimated using present risk; historical cycles and movement dates are N/A. ' +
            'Past critical criteria cover high risk or age >365 days; cycle-only historical criticality cannot be reconstructed.';
    }
    document.getElementById('exec-critical-risk').addEventListener('change',renderCritical);
    async function load() {
        if (!validDates()) { resetResults('Select valid dates and Apply.'); return; }
        request = new AbortController();
        app.setAttribute('aria-busy','true');
        status.textContent = 'Loading all selected-period results…';
        const tasks = [['GetKpis',renderKpis],['GetDistribution',renderDistribution],['GetCriticalItems',loadCritical]];
        const results = await Promise.allSettled(tasks.map(async ([action,render]) => {
            const data = await read(action,request.signal);
            if (!request.signal.aborted) render(data);
        }));
        if (request.signal.aborted) return;
        const failures = results.filter(item => item.status === 'rejected');
        if (failures.length) {
            // Do not leave a mixture of refreshed and failed dataset values.
            resetResults('N/A · One or more dated datasets could not be loaded.');
            status.textContent = 'Audit Para Snap unavailable: ' + failures[0].reason.message;
        } else status.textContent = 'Selected dates applied to all results · Historical estimates and unavailable histories are disclosed.';
        app.setAttribute('aria-busy','false');
    }
    load();
})();

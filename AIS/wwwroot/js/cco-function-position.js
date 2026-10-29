(() => {
    'use strict';
    const root = document.getElementById('cco-function-position');
    if (!root) return;
    const target = document.getElementById('cco-function-results');
    const status = document.getElementById('cco-function-status');
    const columns = [
        ['Group', 'groupName'], ['Division', 'divisionName'],
        ['Opening balance', 'openingBalance'], ['Para added', 'parasAdded'],
        ['Total', 'totalParas'], ['Settled', 'settledParas'],
        ['Para outstanding', 'outstandingParas'], ['Settlement %', 'settlementPercentage']
    ];
    let activeRequest = null;
    const element = (tag, value) => {
        const e = document.createElement(tag);
        if (value != null) e.textContent = String(value);
        return e;
    };
    function clear() {
        target.replaceChildren();
        target.hidden = true;
    }
    function render(rows) {
        clear();
        if (!Array.isArray(rows) || rows.length > 1000) throw Error('Invalid function position response.');
        if (!rows.length) {
            status.textContent = 'No observations in the selected period.';
            return;
        }
        const table = element('table');
        table.className = 'exec-table';
        const head = element('thead');
        const header = element('tr');
        const body = element('tbody');
        columns.forEach(([title]) => {
            const th = element('th', title);
            th.scope = 'col';
            header.append(th);
        });
        head.append(header);
        const rank = row => row.rowKind === 'TOTAL' ? 2 : row.rowKind === 'SUBTOTAL' ? 1 : 0;
        [...rows].sort((a, b) =>
            rank(a) === 2 ? (rank(b) === 2 ? 0 : 1) :
            rank(b) === 2 ? -1 :
            String(a.groupName || '').localeCompare(String(b.groupName || '')) ||
            rank(a) - rank(b) ||
            String(a.divisionName || '').localeCompare(String(b.divisionName || ''))
        ).forEach(row => {
            const tr = element('tr');
            columns.forEach(([, key]) => tr.append(element('td', row[key] == null ? 'N/A' : row[key])));
            body.append(tr);
        });
        table.append(head, body);
        target.append(table);
        target.hidden = false;
        status.textContent = 'Authorized Management Audit (D) observations. Historical estimates; unresolved mappings are included.';
    }
    function queryUrl() {
        const start = document.getElementById('exec-start-date')?.value;
        const end = document.getElementById('exec-end-date')?.value;
        const isoDate = /^\d{4}-\d{2}-\d{2}$/;
        if (!isoDate.test(start || '') || !isoDate.test(end || '') || start > end) {
            throw Error('Select a valid Start Date and End Date, then Apply.');
        }
        const url = new URL(root.dataset.apiUrl, window.location.href);
        url.searchParams.set('startDate', start);
        url.searchParams.set('endDate', end);
        url.searchParams.set('maxRows', '1000');
        return url;
    }
    function failureMessage(code) {
        if (code === 401) return 'Your session has expired. Sign in again to view this report.';
        if (code === 403) return 'Access denied. CCO page and exact API permissions are required.';
        if (code === 400) return 'Invalid report date range. Please correct the dates and Apply.';
        if (code === 503) return 'Function Wise Position is unavailable. Database scope, report package or permissions require administrator verification.';
        return 'Unable to load Function Wise Position.';
    }
    async function load() {
        if (activeRequest) activeRequest.abort();
        clear();
        const controller = new AbortController();
        activeRequest = controller;
        try {
            const url = queryUrl();
            status.textContent = 'Loading authorized Function Wise Position data…';
            const response = await fetch(url, {
                method: 'GET',
                credentials: 'same-origin',
                cache: 'no-store',
                headers: { 'Accept': 'application/json' },
                signal: controller.signal
            });
            if (!response.ok) {
                // Show only the server-generated request correlation identifier,
                // never raw SQL/Oracle errors or authenticated scope details.
                const rejected = await response.json().catch(() => null);
                const reference = typeof rejected?.correlationId === 'string' &&
                    /^[a-zA-Z0-9:._-]{1,100}$/.test(rejected.correlationId)
                    ? rejected.correlationId : null;
                throw Error(failureMessage(response.status) +
                    (reference ? ' Request reference: ' + reference : ''));
            }
            const payload = await response.json();
            if (activeRequest !== controller) return;
            if (payload?.success !== true || !Array.isArray(payload.data)) {
                throw Error('Invalid function position response.');
            }
            render(payload.data);
        } catch (error) {
            if (controller.signal.aborted || activeRequest !== controller) return;
            clear();
            status.textContent = error.message || 'Unable to load Function Wise Position.';
        } finally {
            if (activeRequest === controller) activeRequest = null;
        }
    }
    ['exec-start-date', 'exec-end-date'].forEach(id => {
        document.getElementById(id)?.addEventListener('change', () => {
            if (activeRequest) activeRequest.abort();
            activeRequest = null;
            clear();
            status.textContent = 'Dates changed. Select Apply to load the revised period.';
        });
    });
    void load();
})();

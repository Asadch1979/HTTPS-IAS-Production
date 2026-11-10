(() => {
    'use strict';
    const root = document.getElementById('cco-organizations');
    if (root?.dataset.enabled !== 'true') return;
    const target = document.getElementById('cco-org-results');
    const search = document.getElementById('cco-org-search');
    const status = document.getElementById('cco-org-status');
    const expanded = new Set();
    const start = document.getElementById('exec-start-date');
    const end = document.getElementById('exec-end-date');
    const groupSelect = document.getElementById('exec-entity');
    const request = new AbortController();
    const columns = [
        ['Group', 'groupName'], ['Division', 'divisionName'], ['GM Office', 'gmOfficeName'],
        ['Reporting entity / Department', 'reportingEntityName'], ['Population', 'population'], ['Opening Balance', 'openingCases'], ['Para Added', 'paraAdded'],
        ['Total Paras', 'totalCases'], ['Paras Settled', 'parasSettled'], ['Outstanding', 'outstanding'],
        ['Settlement %', 'settlementRate'], ['Net Backlog Change', 'netBacklogChange'],
        ['High-Risk Outstanding', 'highRiskOpen'], ['Reconciliation Difference', 'reconciliationDifference'],
        ['Mapping Exceptions', 'mappingExceptions']
    ];
    const number = new Intl.NumberFormat('en-GB');
    const percent = new Intl.NumberFormat('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const node = (tag, text) => {
        const element = document.createElement(tag);
        if (text != null) element.textContent = text;
        return element;
    };
    const identityColumns = { GROUP: 0, DIVISION: 1, GM: 2, DETAIL: 3, POPULATION: 4, TOTAL: 0 };
    const populationName = { B: 'Field Audit (B)', D: 'Management Audit (D)' };
    let groups = [], populationTotals = [], grandTotal, ready = false;
    let sortIndex = 0, direction = 1;

    function buildTree(rows) {
        const branches = new Map();
        function branch(kind, ids, names, parent) {
            const key = JSON.stringify([kind, ...ids]);
            if (!branches.has(key)) {
                const entry = { key, row: { rowKind: kind, ...names }, children: [] };
                branches.set(key, entry);
                parent?.children.push(entry);
            }
            return branches.get(key);
        }
        for (const source of rows) {
            const { rowKind, groupId, divisionId, gmOfficeId, auditType } = source;
            if (!['GROUP', 'DIVISION', 'GM', 'DETAIL'].includes(rowKind)) continue;
            const groupName = source.groupName || 'Unassigned Group';
            const group = branch('GROUP', [groupId ?? null], { groupName });
            if (rowKind === 'GROUP') {
                group.row = { ...source, groupName };
                continue;
            }
            const divisionName = source.divisionName || 'Unassigned Division';
            const division = branch('DIVISION', [groupId ?? null, divisionId ?? null], { divisionName }, group);
            if (rowKind === 'DIVISION') {
                division.row = { ...source, divisionName };
                continue;
            }
            const population = populationName[auditType];
            let parent = division;
            if (auditType === 'B') {
                const gmOfficeName = source.gmOfficeName || 'Unassigned GM Office';
                parent = branch('GM', [groupId ?? null, divisionId ?? null, gmOfficeId ?? null],
                    { gmOfficeName, population }, division);
                if (rowKind === 'GM') {
                    parent.row = { ...source, gmOfficeName, population };
                    continue;
                }
            }
            if (rowKind !== 'DETAIL') continue;
            const name = source.reportingEntityName ||
                (auditType === 'D' ? 'Unassigned Department' : 'Unassigned Reporting Entity');
            const status = source.mappingQuality === 'Conflicting' ? ' (conflicting mapping)' :
                source.mappingQuality === 'Unmapped' ? ' (unmapped path)' : '';
            parent.children.push({ row: { ...source, population, reportingEntityName: name + status }, children: [] });
        }
        groups = [...branches.values()].filter(entry => entry.row.rowKind === 'GROUP');
        populationTotals = rows.filter(row => row.rowKind === 'POPULATION')
            .map(row => ({ ...row, population: populationName[row.auditType] + ' total' }));
        grandTotal = rows.find(row => row.rowKind === 'TOTAL');
    }

    function render() {
        if (!ready) return;
        const term = search.value.trim().toLocaleLowerCase();
        const matches = row => columns.slice(0, 5).some(([, key]) =>
            String(row[key] || '').toLocaleLowerCase().includes(term));
        const sorted = branches => [...branches].sort((a, b) => {
            const key = columns[sortIndex][1], left = a.row[key], right = b.row[key];
            if (left == null || right == null) return Number(left == null) - Number(right == null);
            return direction * (sortIndex < 5 ? String(left).localeCompare(right) : left - right);
        });
        const table = node('table');
        table.className = 'exec-table exec-report-table';
        const head = node('thead'), header = node('tr'), body = node('tbody');
        columns.forEach(([label], index) => {
            const th = node('th'), button = node('button', label);
            th.scope = 'col';
            if (index >= 5) th.className = 'exec-number';
            if (sortIndex === index) th.setAttribute('aria-sort', direction === 1 ? 'ascending' : 'descending');
            button.type = 'button';
            button.addEventListener('click', () => {
                direction = sortIndex === index ? -direction : 1;
                sortIndex = index;
                render();
                target.querySelectorAll('thead button')[index].focus();
            });
            th.append(button);
            header.append(th);
        });
        head.append(header);
        function appendRow(row, branch) {
            const identity = identityColumns[row.rowKind];
            const tr = node('tr');
            if (row.rowKind === 'TOTAL') tr.className = 'exec-report-total';
            else if (row.rowKind !== 'DETAIL') tr.className = 'exec-report-subtotal';
            columns.forEach(([, key], index) => {
                const value = row[key];
                const cell = node(index === identity ? 'th' : 'td');
                if (index === identity) cell.scope = 'row';
                if (index >= 5) cell.className = 'exec-number';
                if (index === identity && branch?.children.length && !term) {
                    const open = expanded.has(branch.key);
                    const button = node('button', open ? '−' : '+');
                    button.type = 'button';
                    button.dataset.branch = branch.key;
                    button.setAttribute('aria-label', (open ? 'Collapse ' : 'Expand ') + value);
                    button.setAttribute('aria-expanded', String(open));
                    button.addEventListener('click', () => {
                        if (open) expanded.delete(branch.key); else expanded.add(branch.key);
                        render();
                        [...target.querySelectorAll('[data-branch]')].find(el => el.dataset.branch === branch.key)?.focus();
                    });
                    cell.append(button, ' ');
                }
                cell.append(index < 5 ? (index === identity || index === 4 ? value ?? '' : '')
                    : value == null ? 'N/A' : key === 'settlementRate' ? percent.format(value) + '%' : number.format(value));
                tr.append(cell);
            });
            body.append(tr);
        }
        function visibleBranch(branch, ancestorMatches = false) {
            const include = ancestorMatches || matches(branch.row);
            const children = branch.children.map(child => visibleBranch(child, include)).filter(Boolean);
            return include || children.length ? { ...branch, children } : null;
        }
        function appendBranches(branches) {
            for (const branch of sorted(branches)) {
                appendRow(branch.row, branch);
                if (term || expanded.has(branch.key)) appendBranches(branch.children);
            }
        }
        const visible = term ? groups.map(branch => visibleBranch(branch)).filter(Boolean) : groups;
        appendBranches(visible);
        populationTotals.forEach(row => appendRow(row));
        if (grandTotal && groups.length) appendRow({ ...grandTotal, groupName: 'Grand Total' });
        status.textContent = !groups.length ? 'No performance activity for the selected scope and dates.'
            : !visible.length ? 'No matching organizations. Grand Total covers the selected scope.'
            : term ? 'Showing search matches. Subtotals and Grand Total cover the selected scope.' : 'Performance analysis loaded.';
        table.append(head, body);
        target.replaceChildren(...(groups.length ? [table] : []));
    }

    search.addEventListener('input', render);
    document.getElementById('cco-expand-all').addEventListener('click', () => {
        function expand(branches) {
            for (const branch of branches) {
                if (!branch.children.length) continue;
                expanded.add(branch.key);
                expand(branch.children);
            }
        }
        expand(groups);
        render();
    });
    document.getElementById('cco-collapse-all').addEventListener('click', () => { expanded.clear(); render(); });

    for (const input of [start, end, groupSelect]) {
        input.addEventListener('change', () => {
            request.abort();
            ready = false;
            target.replaceChildren();
            status.textContent = 'Filters changed. Choose Apply to load performance data.';
        });
    }

    async function load() {
        const startDate = start.value, endDate = end.value, groupId = groupSelect.value;
        const query = new URLSearchParams({ startDate, endDate, maxRows: 1000 });
        if (groupId) query.set('groupId', groupId);
        root.setAttribute('aria-busy', 'true');
        try {
            const response = await fetch(root.dataset.url + '?' + query, {
                credentials: 'same-origin', cache: 'no-store', signal: request.signal,
                headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (response.redirected || response.status === 401 || response.status === 403) {
                status.textContent = 'Your session or reporting access has changed. Sign in again or contact your administrator.';
                return;
            }
            if (response.status === 422) {
                status.textContent = 'The result exceeds the row limit. Select one Group or a shorter period.';
                return;
            }
            if (!response.ok || !(response.headers.get('content-type') || '').includes('application/json'))
                throw new Error('Performance data unavailable.');
            const result = await response.json();
            if (result.success !== true || !Array.isArray(result.data))
                throw new Error('Incomplete performance response.');
            if (request.signal.aborted) return;
            buildTree(result.data);
            ready = true;
            render();
        } catch {
            if (request.signal.aborted) return;
            status.textContent = 'Unable to load performance data. Check the selected filters and choose Apply to retry.';
        } finally {
            root.setAttribute('aria-busy', 'false');
        }
    }
    load();
})();

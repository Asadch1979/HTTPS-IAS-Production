(function () {
    'use strict';
    var root = document.getElementById('entityWorkspace');
    if (!root) return;

    function byId(id) { return document.getElementById(id); }
    var tabs = Array.from(root.querySelectorAll('[data-entity-tab]'));
    function activate(index, focus) {
        tabs.forEach(function (tab, n) {
            tab.setAttribute('aria-selected', String(n === index));
            tab.tabIndex = n === index ? 0 : -1;
            var panel = byId(tab.getAttribute('aria-controls'));
            if (panel) panel.hidden = n !== index;
        });
        if (focus) tabs[index].focus();
    }
    tabs.forEach(function (tab, index) {
        tab.addEventListener('click', function () { activate(index, false); });
        tab.addEventListener('keydown', function (event) {
            var next = index;
            if (event.key === 'ArrowRight') next = (index + 1) % tabs.length;
            else if (event.key === 'ArrowLeft') next = (index + tabs.length - 1) % tabs.length;
            else if (event.key === 'Home') next = 0;
            else if (event.key === 'End') next = tabs.length - 1;
            else return;
            event.preventDefault();
            activate(next, true);
        });
    });

    // Draft fields are local only. No endpoint can submit or apply them.
    root.querySelectorAll('[data-change-field]').forEach(function (toggle) {
        toggle.addEventListener('change', function () {
            var input = byId(toggle.getAttribute('data-change-field'));
            if (!input) return;
            input.disabled = !toggle.checked;
            if (!toggle.checked) input.value = '';
        });
    });

    // The deployed Oracle search package is currently unscoped. Its reader is
    // intentionally available only to authenticated Super Users with dashboard
    // access. Never enable it for other users from client-side state alone.
    if (!root.dataset || root.dataset.readEnabled !== 'true') return;

    var offset = 0, pageSize = 50, total = 0, searchVersion = 0, selectionVersion = 0;
    function value(row, key) {
        if (!row) return '';
        var match = Object.keys(row).find(function (k) { return k.toUpperCase() === key.toUpperCase(); });
        var result = match ? row[match] : null;
        return result == null ? '' : String(result);
    }
    function tableMessage(id, cols, message) {
        var body = byId(id);
        if (!body) return;
        var tr = document.createElement('tr'), td = document.createElement('td');
        td.colSpan = cols;
        td.className = 'text-muted';
        td.textContent = message;
        tr.appendChild(td);
        body.replaceChildren(tr);
    }
    function addCell(row, content) {
        var td = document.createElement('td');
        td.textContent = content == null || content === '' ? '—' : String(content);
        row.appendChild(td);
    }
    function keyValueRows(id, record, keys) {
        var body = byId(id);
        if (!body) return;
        body.replaceChildren();
        keys.forEach(function (key) {
            var tr = document.createElement('tr'), th = document.createElement('th');
            th.scope = 'row';
            th.textContent = key.replaceAll('_', ' ').toLowerCase().replace(/\b\w/g, function (m) { return m.toUpperCase(); });
            tr.appendChild(th);
            addCell(tr, value(record, key));
            body.appendChild(tr);
        });
    }
    function error(message) {
        var box = byId('entityReadError');
        if (!box) return;
        box.textContent = message;
        box.classList.remove('d-none');
    }
    function clearError() {
        var box = byId('entityReadError');
        if (box) box.classList.add('d-none');
    }
    function endpoint(operation, params) {
        var url = new URL(root.dataset.readUrl, window.location.href);
        url.searchParams.set('data', operation);
        Object.keys(params || {}).forEach(function (k) {
            if (params[k] != null && params[k] !== '') url.searchParams.set(k, String(params[k]));
        });
        return url.toString();
    }
    async function getData(operation, params) {
        var response = await fetch(endpoint(operation, params), {
            method: 'GET',
            credentials: 'same-origin',
            cache: 'no-store',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (!response.ok) {
            if (response.status === 401) throw new Error('Session expired. Sign in again.');
            if (response.status === 403) throw new Error('Entity information is not authorized for this session.');
            if (response.status === 503) throw new Error('Oracle reader unavailable. Check the application database connection and package.');
            throw new Error('Unable to load Oracle entity data (HTTP ' + response.status + ').');
        }
        return response.json();
    }
    async function loadSummary() {
        try {
            var s = await getData('summary');
            byId('entityMetric-active').textContent = String(s.activeEntities ?? 'Unavailable');
            byId('entityMetric-inactive').textContent = String(s.inactiveEntities ?? 'Unavailable');
        } catch (e) {
            byId('entityMetric-active').textContent = 'Unavailable';
            byId('entityMetric-inactive').textContent = 'Unavailable';
            error(e.message);
        }
    }
    async function loadTypes() {
        var select = byId('entityTypeFilter');
        if (!select) return;
        try {
            var result = await getData('types');
            (result.rows || []).forEach(function (type) {
                var option = document.createElement('option');
                option.value = String(type.id);
                option.textContent = type.name || ('Entity Type ' + type.id);
                select.appendChild(option);
            });
        } catch (_) {
            // Search and status remain usable without optional type metadata.
            select.title = 'Entity Type lookup unavailable; search by ID or name still works.';
        }
    }
    async function search(reset) {
        if (reset) offset = 0;
        var version = ++searchVersion;
        var params = {
            query: byId('entityQuery').value.trim(),
            typeId: byId('entityTypeFilter').value,
            status: byId('entityStatusFilter').value,
            offset: offset,
            pageSize: pageSize
        };
        tableMessage('entityRegisterRows', 7, 'Loading entities from Oracle...');
        byId('entityPrevious').disabled = true;
        byId('entityNext').disabled = true;
        clearError();
        try {
            var result = await getData('search', params);
            if (version !== searchVersion) return;
            var rows = result.rows || [];
            total = Number(result.totalRows || 0);
            var body = byId('entityRegisterRows');
            body.replaceChildren();
            if (!rows.length) tableMessage('entityRegisterRows', 7, 'No entities matched your search.');
            rows.forEach(function (item) {
                var tr = document.createElement('tr');
                var id = Number(value(item, 'ENTITY_ID'));
                addCell(tr, id);
                addCell(tr, value(item, 'CODE'));
                addCell(tr, value(item, 'NAME'));
                addCell(tr, value(item, 'TYPE_ID'));
                addCell(tr, value(item, 'AUDITBY_ID'));
                addCell(tr, value(item, 'ACTIVE') === 'Y' ? 'Active' : 'Inactive');
                var cell = document.createElement('td'), button = document.createElement('button');
                button.type = 'button';
                button.className = 'btn btn-sm btn-outline-success';
                button.textContent = 'View';
                button.addEventListener('click', function () { selectEntity(id, value(item, 'NAME')); });
                cell.appendChild(button);
                tr.appendChild(cell);
                body.appendChild(tr);
            });
            byId('entityRegisterStatus').textContent = total === 0 ? 'No matching entities' :
                'Showing ' + (offset + 1) + '–' + (offset + rows.length) + ' of ' + total;
            byId('entityPrevious').disabled = offset === 0;
            byId('entityNext').disabled = offset + rows.length >= total;
        } catch (e) {
            if (version !== searchVersion) return;
            tableMessage('entityRegisterRows', 7, e.message);
            byId('entityRegisterStatus').textContent = 'Unable to retrieve entities';
            error(e.message);
        }
    }
    function renderMappings(rows) {
        var body = byId('entityMappingRows');
        body.replaceChildren();
        var mapped = rows.filter(function (r) {
            return value(r, 'PARENT_ID') !== '' || value(r, 'MAPPING_AUDITEDBY') !== '';
        });
        if (!mapped.length) { tableMessage('entityMappingRows', 10, 'No current mapping row found.'); return; }
        mapped.forEach(function (r) {
            var tr = document.createElement('tr');
            ['PARENT_ID', 'P_NAME', 'C_NAME', 'P_TYPE_ID', 'C_TYPE_ID',
                'MAPPING_AUDITEDBY', 'GM_OFFICE', 'REPORTING', 'DIV_OFFICE', 'B_GROUP']
                .forEach(function (key) { addCell(tr, value(r, key)); });
            body.appendChild(tr);
        });
    }
    function renderRecordRows(id, rows, keys, empty) {
        var body = byId(id);
        body.replaceChildren();
        if (!rows.length) {
            tableMessage(id, keys.length, empty);
            return;
        }
        rows.forEach(function (record) {
            var tr = document.createElement('tr');
            keys.forEach(function (key) { addCell(tr, value(record, key)); });
            body.appendChild(tr);
        });
    }
    async function selectEntity(id, name) {
        if (!Number.isInteger(id) || id <= 0) return;
        var version = ++selectionVersion;
        byId('selectedEntityLabel').textContent = name + ' (ID ' + id + ')';
        tableMessage('entityOverviewRows', 2, 'Loading master profile...');
        tableMessage('entityMasterRows', 2, 'Loading master fields...');
        tableMessage('entityMappingRows', 10, 'Loading mappings...');
        tableMessage('entityClosureRows', 2, 'Loading closure preview...');
        tableMessage('entityHistoryRows', 7, 'Loading audit history...');
        tableMessage('entityArchiveRows', 8, 'Loading historical para archive...');
        var params = { entityId: id };
        getData('profile', params).then(function (result) {
            if (version !== selectionVersion) return;
            var rows = result.rows || [];
            if (!rows.length) { tableMessage('entityMasterRows', 2, 'Entity not found.'); return; }
            var keys = ['ENTITY_ID', 'CODE', 'NAME', 'DESCRIPTION', 'TYPE_ID', 'AUDITBY_ID',
                'ACTIVE', 'AUDITABLE', 'AUDITOR', 'IAD', 'RISK_ID', 'SIZE_ID',
                'ADDRESS', 'TELEPHONE', 'EMAIL_ADDRESS'];
            keyValueRows('entityMasterRows', rows[0], keys);
            keyValueRows('entityOverviewRows', rows[0], ['ENTITY_ID', 'CODE', 'NAME', 'TYPE_ID', 'ACTIVE', 'AUDITABLE', 'AUDITBY_ID']);
            renderMappings(rows);
            if (result.truncated) error('The entity has more mapping rows than the configured display limit.');
        }).catch(function (e) {
            if (version !== selectionVersion) return;
            tableMessage('entityMasterRows', 2, e.message);
            tableMessage('entityOverviewRows', 2, e.message);
            tableMessage('entityMappingRows', 10, e.message);
        });
        getData('closure', params).then(function (result) {
            if (version !== selectionVersion) return;
            var rows = result.rows || [];
            if (!rows.length) { tableMessage('entityClosureRows', 2, 'No closure-preview information.'); return; }
            keyValueRows('entityClosureRows', rows[0], [
                'TOTAL_POST_COMPLIANCE', 'OPEN_PARAS', 'SETTLED_PARAS', 'ALREADY_ZERO_PARAS',
                'STATUS_6_PARAS', 'STATUS_28_PARAS', 'OTHER_PARA_STATUSES',
                'ALL_ENGAGEMENTS', 'IN_PROGRESS_ENGAGEMENTS', 'ENGAGEMENTS_NOT_FINAL_14',
                'OTHER_ENGAGEMENT_STATES_FOR_REVIEW', 'ACTIVE_CHILD_MAPPINGS',
                'MAPPING_ROWS', 'PREVIOUS_ARCHIVE_ROWS'
            ]);
        }).catch(function (e) { if (version === selectionVersion) tableMessage('entityClosureRows', 2, e.message); });
        getData('history', params).then(function (result) {
            if (version !== selectionVersion) return;
            renderRecordRows('entityHistoryRows', result.rows || [],
                ['RECORDED_ON', 'ACTION_CODE', 'EVENT_STATUS', 'ACTOR_PPNO', 'CHANGE_REASON', 'REQUEST_ID', 'BATCH_ID'],
                'No change-log events recorded for this entity.');
            if (result.truncated) error('Change history is limited to the first 100 rows for this preview.');
        }).catch(function (e) { if (version === selectionVersion) tableMessage('entityHistoryRows', 7, e.message); });
        getData('archive', params).then(function (result) {
            if (version !== selectionVersion) return;
            renderRecordRows('entityArchiveRows', result.rows || [],
                ['COM_ID', 'PARA_NO', 'PARA_STATUS', 'PARA_ADDED_ON', 'SETTELED_ON', 'COM_CYCLE', 'ARCHIVED_ON', 'ENTITY_LOG_ID'],
                'No archived para snapshots recorded for this entity.');
            if (result.truncated) error('Para archive display is limited to the first 100 rows for this preview.');
        }).catch(function (e) { if (version === selectionVersion) tableMessage('entityArchiveRows', 8, e.message); });
    }

    byId('entitySearchButton').addEventListener('click', function () { search(true); });
    byId('entityQuery').addEventListener('keydown', function (event) {
        if (event.key === 'Enter') { event.preventDefault(); search(true); }
    });
    byId('entityStatusFilter').addEventListener('change', function () { search(true); });
    byId('entityTypeFilter').addEventListener('change', function () { search(true); });
    byId('entityPrevious').addEventListener('click', function () {
        offset = Math.max(0, offset - pageSize); search(false);
    });
    byId('entityNext').addEventListener('click', function () {
        if (offset + pageSize < total) { offset += pageSize; search(false); }
    });

    loadSummary();
    loadTypes();
    search(true);
}());

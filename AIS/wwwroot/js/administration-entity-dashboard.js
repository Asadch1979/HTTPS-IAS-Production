(function () {
    'use strict';

    const root = document.getElementById('entityWorkspace');
    if (!root) return;
    const byId = function (id) { return document.getElementById(id); };
    const tabs = Array.from(root.querySelectorAll('[data-entity-tab]'));
    let activeArea = 0;
    let currentEntity = null;
    let currentProfile = [];
    let registerRows = [];
    let offset = 0, total = 0;
    const pageSize = 12;
    let searchVersion = 0, selectionVersion = 0, legacyVersion = 0;
    const types = new Map();
    const nameCache = new Map();

    function activate(index, focus) {
        activeArea = index;
        tabs.forEach(function (tab, n) {
            const selected = n === index;
            tab.setAttribute('aria-selected', String(selected));
            tab.tabIndex = selected ? 0 : -1;
            const panel = byId(tab.getAttribute('aria-controls'));
            if (panel) panel.hidden = !selected;
        });
        if (focus && tabs[index]) tabs[index].focus();
    }
    tabs.forEach(function (tab, index) {
        tab.addEventListener('click', function () { activate(index, false); });
        tab.addEventListener('keydown', function (event) {
            let next = index;
            if (event.key === 'ArrowRight') next = (index + 1) % tabs.length;
            else if (event.key === 'ArrowLeft') next = (index + tabs.length - 1) % tabs.length;
            else if (event.key === 'Home') next = 0;
            else if (event.key === 'End') next = tabs.length - 1;
            else return;
            event.preventDefault();
            activate(next, true);
        });
    });

    function dataValue(row, field) {
        if (!row) return '';
        const key = Object.keys(row).find(function (k) { return k.toUpperCase() === field.toUpperCase(); });
        const value = key ? row[key] : null;
        return value == null ? '' : String(value);
    }
    function setText(id, value) {
        const element = byId(id);
        if (element) element.textContent = value == null || value === '' ? 'Not assigned' : String(value);
    }
    function fieldLabel(id) {
        if (!id) return 'Type not assigned';
        return types.get(String(id)) || 'Type description unavailable';
    }
    function clearError() {
        const error = byId('entityReadError');
        if (error) error.classList.add('d-none');
    }
    function showError(message) {
        const error = byId('entityReadError');
        if (!error) return;
        error.textContent = message;
        error.classList.remove('d-none');
    }
    function emptyRow(tbodyId, columnCount, message) {
        const body = byId(tbodyId);
        if (!body) return;
        const row = document.createElement('tr');
        const cell = document.createElement('td');
        cell.colSpan = columnCount;
        cell.className = 'entity-empty';
        cell.textContent = message;
        row.appendChild(cell);
        body.replaceChildren(row);
    }
    function make(tag, className, content) {
        const el = document.createElement(tag);
        if (className) el.className = className;
        if (content != null) el.textContent = String(content);
        return el;
    }
    function replaceWithFields(id, pairs) {
        const host = byId(id);
        if (!host) return;
        host.replaceChildren();
        pairs.forEach(function (pair) {
            const line = make('div');
            line.appendChild(make('dt', '', pair[0]));
            line.appendChild(make('dd', '', pair[1] == null || pair[1] === '' ? 'Not assigned' : pair[1]));
            host.appendChild(line);
        });
    }
    function endpoint(action, params) {
        const url = new URL(root.dataset.readUrl, window.location.href);
        url.searchParams.set('data', action);
        Object.keys(params || {}).forEach(function (key) {
            const value = params[key];
            if (value != null && value !== '') url.searchParams.set(key, String(value));
        });
        return url.toString();
    }
    async function getData(action, params) {
        const response = await fetch(endpoint(action, params), {
            method: 'GET', credentials: 'same-origin', cache: 'no-store',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (!response.ok) {
            if (response.status === 401) throw new Error('Session expired. Please sign in again.');
            if (response.status === 403) throw new Error('Your session is not authorized to read this entity information.');
            if (response.status === 404) throw new Error('The requested entity was not found.');
            if (response.status === 503) throw new Error('The Oracle entity reader is unavailable. Check the configured database and package.');
            throw new Error('Unable to retrieve entity data (HTTP ' + response.status + ').');
        }
        return response.json();
    }
    async function loadSummary() {
        try {
            const response = await getData('summary');
            setText('entityMetric-active', response.activeEntities);
            setText('entityMetric-inactive', response.inactiveEntities);
        } catch (e) {
            setText('entityMetric-active', 'Unavailable');
            setText('entityMetric-inactive', 'Unavailable');
            showError(e.message);
        }
    }
    async function loadTypes() {
        try {
            const data = await getData('types');
            const filter = byId('entityTypeFilter');
            (data.rows || []).forEach(function (type) {
                if (!type || !type.id) return;
                types.set(String(type.id), String(type.name || 'Unknown type'));
                if (filter) {
                    const option = make('option', '', type.name || 'Unknown type');
                    option.value = String(type.id);
                    filter.appendChild(option);
                }
            });
            renderRegister();
            root.dispatchEvent(new CustomEvent('entity-workspace:types', {
                detail: { types: (data.rows || []).map(function (x) { return { id: x.id, name: x.name }; }) }
            }));
            if (currentProfile.length) renderSelectedProfile(currentProfile, selectionVersion);
        } catch (e) {
            // A missing supplementary type reader must not disable master-data search.
            const filter = byId('entityTypeFilter');
            if (filter) filter.title = 'Type descriptions are unavailable in this session.';
        }
    }
    function renderRegister() {
        const body = byId('entityRegisterRows');
        if (!body) return;
        body.replaceChildren();
        if (!registerRows.length) {
            emptyRow('entityRegisterRows', 4, 'No entities match these filters.');
            return;
        }
        registerRows.forEach(function (entity) {
            const id = Number(dataValue(entity, 'ENTITY_ID'));
            const name = dataValue(entity, 'NAME');
            const active = dataValue(entity, 'ACTIVE') === 'Y';
            const row = make('tr', currentEntity && currentEntity.id === id ? 'is-selected' : '');
            const nameCell = make('td');
            nameCell.appendChild(make('strong', '', name || 'Unnamed entity'));
            const code = dataValue(entity, 'CODE');
            if (code) {
                const codeSpan = make('span', 'entity-reference', ' · ' + code);
                nameCell.appendChild(codeSpan);
            }
            row.appendChild(nameCell);
            row.appendChild(make('td', '', fieldLabel(dataValue(entity, 'TYPE_ID'))));
            const statusCell = make('td');
            statusCell.appendChild(make('span', 'entity-inline-status' + (active ? '' : ' inactive'), active ? 'Active' : 'Inactive'));
            row.appendChild(statusCell);
            const actionCell = make('td');
            const button = make('button', 'entity-choose-btn', 'Select');
            button.type = 'button';
            button.addEventListener('click', function () { selectEntity(id, name, active); });
            actionCell.appendChild(button);
            row.appendChild(actionCell);
            body.appendChild(row);
        });
    }
    async function search(reset) {
        if (reset) offset = 0;
        const current = ++searchVersion;
        emptyRow('entityRegisterRows', 4, 'Loading entities...');
        byId('entityPrevious').disabled = true;
        byId('entityNext').disabled = true;
        clearError();
        try {
            const result = await getData('search', {
                query: byId('entityQuery').value.trim(),
                typeId: byId('entityTypeFilter').value,
                status: byId('entityStatusFilter').value,
                offset: offset, pageSize: pageSize
            });
            if (current !== searchVersion) return;
            registerRows = result.rows || [];
            total = Number(result.totalRows || 0);
            renderRegister();
            setText('entityRegisterStatus', total ? 'Showing ' + (offset + 1) + '–' + (offset + registerRows.length) + ' of ' + total : 'No matching records');
            byId('entityPrevious').disabled = offset === 0;
            byId('entityNext').disabled = offset + registerRows.length >= total;
        } catch (e) {
            if (current !== searchVersion) return;
            registerRows = [];
            emptyRow('entityRegisterRows', 4, e.message);
            setText('entityRegisterStatus', 'Unable to load entities');
            showError(e.message);
        }
    }
    async function resolveEntityName(referenceId) {
        const id = Number(referenceId);
        if (!Number.isInteger(id) || id <= 0) return 'Not assigned';
        if (nameCache.has(id)) return nameCache.get(id);
        try {
            const result = await getData('profile', { entityId: id });
            const name = dataValue((result.rows || [])[0], 'NAME') || 'Reference name unavailable';
            nameCache.set(id, name);
            return name;
        } catch (_) {
            return 'Reference name unavailable';
        }
    }
    // Mapping.GM_OFFICE takes precedence. A fallback from mapping AUDITEDBY
    // is used only if that referenced entity has one unambiguous GM_OFFICE.
    // AUDITEDBY itself is not assumed to be a GM office.
    async function resolveGmName(mapping) {
        const direct = dataValue(mapping, 'GM_OFFICE');
        if (direct) return resolveEntityName(direct);
        const auditOfficeId = Number(dataValue(mapping, 'MAPPING_AUDITEDBY'));
        if (!Number.isInteger(auditOfficeId) || auditOfficeId <= 0) return 'Not assigned';
        try {
            const lookup = await getData('profile', { entityId: auditOfficeId });
            const candidates = Array.from(new Set((lookup.rows || [])
                .map(function (row) { return dataValue(row, 'GM_OFFICE'); })
                .filter(Boolean)));
            if (candidates.length !== 1) return 'Not assigned / ambiguous fallback';
            const gmName = await resolveEntityName(candidates[0]);
            return gmName === 'Reference name unavailable' ? gmName : gmName + ' (via audit office)';
        } catch (_) { return 'GM fallback unavailable'; }
    }
    function mappingRows(profileRows) {
        return (profileRows || []).filter(function (row) {
            return dataValue(row, 'PARENT_ID') || dataValue(row, 'P_NAME') ||
                dataValue(row, 'GM_OFFICE') || dataValue(row, 'B_GROUP') ||
                dataValue(row, 'MAPPING_AUDITEDBY');
        });
    }
    async function renderSelectedProfile(profileRows, version) {
        if (!profileRows.length || version !== selectionVersion) return;
        const master = profileRows[0];
        const mappings = mappingRows(profileRows);
        const preferred = mappings[0] || {};
        const parentId = dataValue(preferred, 'PARENT_ID');
        let parent = dataValue(preferred, 'P_NAME');
        const groupId = dataValue(preferred, 'B_GROUP');
        const [resolvedParent, gmName, groupName, auditedByName] = await Promise.all([
            parent ? Promise.resolve(parent) : resolveEntityName(parentId),
            resolveGmName(preferred),
            resolveEntityName(groupId),
            resolveEntityName(dataValue(master, 'AUDITBY_ID'))
        ]);
        if (version !== selectionVersion) return;
        parent = resolvedParent;
        const entityType = fieldLabel(dataValue(master, 'TYPE_ID'));
        setText('identityName', dataValue(master, 'NAME'));
        setText('identityParent', parent);
        setText('identityType', entityType);
        setText('identityGm', gmName);
        setText('identityGroup', groupName);
        setText('identityActive', dataValue(master, 'ACTIVE') === 'Y' ? 'Active' : 'Inactive');
        setText('selectedEntityLabel', dataValue(master, 'NAME'));
        setText('selectedEntityReference', 'Entity ID ' + dataValue(master, 'ENTITY_ID') +
            (dataValue(master, 'CODE') ? ' · Code ' + dataValue(master, 'CODE') : ''));
        const flag = byId('selectedEntityStatus');
        if (flag) flag.textContent = dataValue(master, 'ACTIVE') === 'Y' ? 'Active' : 'Inactive';
        setText('entityMappingNotice', mappings.length > 1
            ? 'Review required: ' + mappings.length + ' mapping records exist. The summary shows the first returned mapping; all records are listed below.'
            : 'Current entity context is shared across the four areas. Reference names are resolved from the authorized entity master.');
        replaceWithFields('entityMasterDetails', [
            ['Entity name', dataValue(master, 'NAME')],
            ['Code', dataValue(master, 'CODE')],
            ['Entity type', entityType],
            ['Active status', dataValue(master, 'ACTIVE') === 'Y' ? 'Active' : 'Inactive'],
            ['Auditable', dataValue(master, 'AUDITABLE') === 'Y' ? 'Yes' : 'No'],
            ['Audit cluster', auditedByName],
            ['Description', dataValue(master, 'DESCRIPTION')],
            ['Address', dataValue(master, 'ADDRESS')],
            ['Telephone', dataValue(master, 'TELEPHONE')],
            ['Email', dataValue(master, 'EMAIL_ADDRESS')]
        ]);
        const mappingHost = byId('entityMappingDetails');
        if (mappingHost) {
            mappingHost.replaceChildren();
            if (!mappings.length) {
                mappingHost.appendChild(make('p', '', 'No current mapping record found.'));
            }
            const limit = Math.min(mappings.length, 50);
            for (let i = 0; i < limit; i++) {
                const item = mappings[i];
                const box = make('div', 'entity-mapping-record');
                box.appendChild(make('strong', '', 'Mapping ' + (i + 1)));
                const itemParent = dataValue(item, 'P_NAME') || await resolveEntityName(dataValue(item, 'PARENT_ID'));
                const [itemGm, itemGroup] = await Promise.all([
                    resolveGmName(item),
                    resolveEntityName(dataValue(item, 'B_GROUP'))
                ]);
                if (version !== selectionVersion) return;
                box.appendChild(make('p', '', 'Parent: ' + itemParent));
                box.appendChild(make('p', '', 'GM Office: ' + itemGm + ' · Group: ' + itemGroup));
                const mappingStatus = dataValue(item, 'MAPPING_STATUS');
                if (mappingStatus) box.appendChild(make('p', '', 'Mapping status: ' + mappingStatus));
                mappingHost.appendChild(box);
            }
            if (mappings.length > limit) mappingHost.appendChild(make('p', 'entity-mapping-warning', 'Additional mappings exist and require an approved paged reader.'));
        }
        renderRegister();
    }
    function renderClosure(result) {
        const row = (result.rows || [])[0];
        if (!row) {
            replaceWithFields('entityClosureDetails', [['Closure preview', 'No result returned']]);
            return;
        }
        const items = [
            ['Total observations', 'TOTAL_POST_COMPLIANCE'],
            ['Open paras', 'OPEN_PARAS'],
            ['Settled paras', 'SETTLED_PARAS'],
            ['Other para statuses', 'OTHER_PARA_STATUSES'],
            ['Total engagements', 'ALL_ENGAGEMENTS'],
            ['In-progress engagements', 'IN_PROGRESS_ENGAGEMENTS'],
            ['Engagements not final-issued', 'ENGAGEMENTS_NOT_FINAL_14'],
            ['Mapped child entities', 'ACTIVE_CHILD_MAPPINGS'],
            ['Existing mapping records', 'MAPPING_ROWS'],
            ['Previously archived records', 'PREVIOUS_ARCHIVE_ROWS']
        ];
        replaceWithFields('entityClosureDetails', items.map(function (it) { return [it[0], dataValue(row, it[1]) || '0']; }));
    }
    function renderHistory(id, rows, limit, emptyMessage, recordLine) {
        const host = byId(id);
        if (!host) return;
        host.replaceChildren();
        if (!rows.length) {
            host.appendChild(make('p', '', emptyMessage));
            return;
        }
        (rows || []).slice(0, limit).forEach(function (r) {
            const box = make('div', 'entity-history-record');
            box.appendChild(make('strong', '', recordLine(r)));
            const secondLine = id === 'entityHistoryDetails'
                ? [dataValue(r, 'RECORDED_ON'), dataValue(r, 'CHANGE_REASON'), dataValue(r, 'ACTOR_PPNO')].filter(Boolean).join(' · ')
                : [dataValue(r, 'PARA_ADDED_ON'), dataValue(r, 'SETTELED_ON'), dataValue(r, 'ARCHIVED_ON')].filter(Boolean).join(' · ');
            box.appendChild(make('p', '', secondLine));
            host.appendChild(box);
        });
    }
    function renderParaStatus(code) {
        return code === '8' ? 'Open' : code === '9' ? 'Settled' : code ? 'Other status (' + code + ')' : 'Unknown status';
    }
    function selectEntity(id, name, active) {
        if (!Number.isInteger(id) || id <= 0) return;
        const version = ++selectionVersion;
        currentEntity = { id: id, name: name, active: active };
        currentProfile = [];
        // Invalidate any partially loaded operation belonging to a different entity.
        ++legacyVersion;
        const engagementHost = byId('entityEngagementLegacy');
        if (engagementHost) engagementHost.replaceChildren();
        root.dispatchEvent(new CustomEvent('entity-workspace:selected', {
            detail: { id: id, name: name, active: active }
        }));
        setText('selectedEntityLabel', name || 'Loading entity...');
        setText('selectedEntityStatus', active ? 'Active' : 'Inactive');
        setText('selectedEntityReference', 'Entity ID ' + id);
        ['identityName', 'identityParent', 'identityType', 'identityGm', 'identityGroup', 'identityActive'].forEach(function (field) {
            setText(field, 'Loading...');
        });
        replaceWithFields('entityMasterDetails', [['Entity profile', 'Loading current Oracle record...']]);
        byId('entityMappingDetails').replaceChildren(make('p', '', 'Loading mapping records...'));
        replaceWithFields('entityClosureDetails', [['Closure impact', 'Loading preliminary counts...']]);
        byId('entityHistoryDetails').replaceChildren(make('p', '', 'Loading change history...'));
        byId('entityArchiveDetails').replaceChildren(make('p', '', 'Loading archived paras...'));
        clearError();
        renderRegister();
        const params = { entityId: id };
        getData('profile', params).then(function (r) {
            if (version !== selectionVersion) return;
            currentProfile = r.rows || [];
            if (!currentProfile.length) throw new Error('No master profile was returned.');
            root.dispatchEvent(new CustomEvent('entity-workspace:profile', {
                detail: { id: id, profile: currentProfile }
            }));
            return renderSelectedProfile(currentProfile, version);
        }).catch(function (e) {
            if (version !== selectionVersion) return;
            setText('identityName', 'Unavailable');
            replaceWithFields('entityMasterDetails', [['Entity profile', e.message]]);
            byId('entityMappingDetails').replaceChildren(make('p', '', e.message));
            showError(e.message);
        });
        getData('closure', params).then(function (r) {
            if (version === selectionVersion) renderClosure(r);
        }).catch(function (e) { if (version === selectionVersion) replaceWithFields('entityClosureDetails', [['Closure impact', e.message]]); });
        getData('history', params).then(function (r) {
            if (version !== selectionVersion) return;
            renderHistory('entityHistoryDetails', r.rows || [], 100, 'No change events have been recorded for this entity.',
                function (row) { return (dataValue(row, 'ACTION_CODE') || 'Change') + ' · ' + (dataValue(row, 'EVENT_STATUS') || 'Recorded'); });
        }).catch(function (e) { if (version === selectionVersion) byId('entityHistoryDetails').replaceChildren(make('p', '', e.message)); });
        getData('archive', params).then(function (r) {
            if (version !== selectionVersion) return;
            renderHistory('entityArchiveDetails', r.rows || [], 100, 'No archived para snapshots were found for this entity.',
                function (row) { return 'Para ' + (dataValue(row, 'PARA_NO') || dataValue(row, 'COM_ID') || '—') + ' · ' + renderParaStatus(dataValue(row, 'PARA_STATUS')); });
        }).catch(function (e) { if (version === selectionVersion) byId('entityArchiveDetails').replaceChildren(make('p', '', e.message)); });
    }

    function appPath(url) {
        const value = String(url || '').trim();
        const base = String(window.g_asiBaseURL || '').replace(/\/+$/, '');
        if (value.startsWith('~/')) return base + value.substring(1);
        if (value.startsWith('/') && base && value !== base && !value.startsWith(base + '/')) return base + value;
        return value;
    }
    async function executeServerScripts(container, version) {
        const scripts = Array.from(container.querySelectorAll('script'));
        for (const oldScript of scripts) {
            if (version !== legacyVersion) return;
            await new Promise(function (resolve, reject) {
                const script = document.createElement('script');
                Array.from(oldScript.attributes).forEach(function (attribute) {
                    if (attribute.name !== 'src') script.setAttribute(attribute.name, attribute.value);
                });
                const src = oldScript.getAttribute('src');
                if (src) {
                    script.src = appPath(src);
                    script.async = false;
                    script.onload = resolve;
                    script.onerror = reject;
                } else {
                    script.textContent = oldScript.textContent;
                }
                oldScript.parentNode.replaceChild(script, oldScript);
                if (!src) resolve();
            });
        }
    }
    async function loadLegacyOperation(step, target) {
        const host = byId(target);
        if (!host) return;
        const current = ++legacyVersion;
        Array.from(root.querySelectorAll('.entity-legacy-host')).forEach(function (container) {
            container.replaceChildren();
        });
        host.appendChild(make('p', '', 'Loading authorized administration operation...'));
        const url = new URL(root.dataset.legacyReadUrl, window.location.href);
        url.searchParams.set('stepKey', step);
        url.searchParams.set('_', String(Date.now()));
        try {
            const response = await fetch(url.toString(), {
                method: 'GET', credentials: 'same-origin', cache: 'no-store',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (current !== legacyVersion) return;
            if (!response.ok) {
                if (response.status === 401) throw new Error('Your session has expired. Please sign in again.');
                if (response.status === 403) throw new Error('You do not have permission for this administration operation.');
                throw new Error('Unable to open this operation. HTTP ' + response.status);
            }
            const trustedServerPartial = await response.text();
            if (current !== legacyVersion) return;
            // This markup is a server-authorized IAS Razor partial from the existing
            // endpoint, not entity/master data. Keep all user data textContent-only.
            host.innerHTML = trustedServerPartial;
            host.classList.add('entity-embedded');
            await executeServerScripts(host, current);
            if (current === legacyVersion) {
                root.dispatchEvent(new CustomEvent('entity-workspace:operation-loaded', {
                    detail: { step: step, target: target }
                }));
            }
        } catch (e) {
            if (current === legacyVersion) host.replaceChildren(make('p', 'entity-mapping-warning', e.message));
        }
    }
    root.querySelectorAll('[data-legacy-step]').forEach(function (button) {
        button.addEventListener('click', function () {
            if (button.hasAttribute('data-unified-edit') && !currentEntity) {
                showError('Select an entity before editing.');
                return;
            }
            loadLegacyOperation(button.getAttribute('data-legacy-step'),
                button.getAttribute('data-legacy-target'));
        });
    });
    const without = byId('entityWithoutParas');
    if (without) without.addEventListener('click', function () {
        ++legacyVersion;
        byId('entityMovementLegacy').replaceChildren();
        byId('entityClosureSection').hidden = true;
        setText('entityMovementMessage', 'Mapping-only shifting requires an approved transactional definition. It must not be treated as para settlement or silently reuse the para-shifting process.');
    });
    const closure = byId('entityShowClosure');
    if (closure) closure.addEventListener('click', function () {
        ++legacyVersion;
        byId('entityMovementLegacy').replaceChildren();
        byId('entityClosureSection').hidden = false;
        setText('entityMovementMessage', 'Preliminary closure impact only. Outstanding paras remain outstanding, and maker-checker authorization is required before any future execution.');
    });
    root.querySelectorAll('[data-legacy-step="ENTITY_SHIFTING"]').forEach(function (button) {
        button.addEventListener('click', function () { byId('entityClosureSection').hidden = true; });
    });

    if (!root.dataset || root.dataset.readEnabled !== 'true') return;
    byId('entitySearchButton').addEventListener('click', function () { search(true); });
    byId('entityQuery').addEventListener('keydown', function (event) {
        if (event.key === 'Enter') { event.preventDefault(); search(true); }
    });
    byId('entityStatusFilter').addEventListener('change', function () { search(true); });
    byId('entityTypeFilter').addEventListener('change', function () { search(true); });
    byId('entityPrevious').addEventListener('click', function () {
        offset = Math.max(0, offset - pageSize);
        search(false);
    });
    byId('entityNext').addEventListener('click', function () {
        if (offset + pageSize < total) { offset += pageSize; search(false); }
    });
    loadSummary();
    loadTypes();
    search(true);
}());

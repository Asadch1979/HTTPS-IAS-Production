(function () {
    'use strict';
    const root = document.getElementById('entityWorkspace');
    if (!root) return;
    const byId = id => document.getElementById(id);
    const readEnabled = root.dataset && root.dataset.readEnabled === 'true';
    const typeList = [];
    const latest = { entity: null, profile: null };
    const pickerVersions = new Map();
    const groupTypes = [];
    const gmTypes = [];
    let additionsReady = false;
    let editVersion = 0;

    function value(record, name) {
        if (!record) return '';
        const key = Object.keys(record).find(k => k.toUpperCase() === name.toUpperCase());
        return key && record[key] != null ? String(record[key]) : '';
    }
    function setMessage(message, error) {
        const box = byId('entityAdditionLookupNotice');
        if (box) {
            box.textContent = message;
            box.classList.toggle('entity-mapping-warning', !!error);
        }
    }
    function addOption(select, id, label) {
        const option = document.createElement('option');
        option.value = String(id);
        option.textContent = String(label || 'Unnamed entity');
        select.appendChild(option);
    }
    function baseOptions(select, message) {
        select.replaceChildren();
        addOption(select, '', message);
    }
    function url(action, args) {
        const endpoint = new URL(root.dataset.readUrl, window.location.href);
        endpoint.searchParams.set('data', action);
        Object.entries(args || {}).forEach(([k, v]) => {
            if (v != null && v !== '') endpoint.searchParams.set(k, String(v));
        });
        return endpoint.toString();
    }
    async function read(action, args) {
        const response = await fetch(url(action, args), {
            method: 'GET', credentials: 'same-origin', cache: 'no-store',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (!response.ok) throw new Error('Unable to retrieve authorized ' + action + ' information (HTTP ' + response.status + ').');
        return response.json();
    }
    function showLegacyError(target, message) {
        const host = byId(target);
        if (host) {
            const p = document.createElement('p');
            p.className = 'entity-mapping-warning';
            p.textContent = message;
            host.prepend(p);
        }
    }
    function currentSelected() {
        return latest.entity && latest.profile && Number(value(latest.profile[0], 'ENTITY_ID')) === Number(latest.entity.id)
            ? latest.entity : null;
    }

    root.addEventListener('entity-workspace:selected', event => {
        latest.entity = event.detail || null;
        latest.profile = null;
        editVersion++;
        const editButton = byId('entityUnifiedEditButton');
        if (editButton) editButton.disabled = true;
        const host = byId('entityEngagementLegacy');
        if (host) host.replaceChildren();
        const context = byId('entityEngagementContextNote');
        if (context) context.textContent = 'Selected entity: ' + (latest.entity && latest.entity.name || '') +
            '. Open the required engagement operation to load its current records without choosing it again.';
    });
    root.addEventListener('entity-workspace:profile', event => {
        if (!event.detail || !latest.entity || Number(event.detail.id) !== Number(latest.entity.id)) return;
        latest.profile = event.detail.profile || [];
        const button = byId('entityUnifiedEditButton');
        if (button) button.disabled = !currentSelected();
    });

    // Reuse the existing Add Entity update modal: no old UPDATE_AUDITEE_ENTITIES
    // page and no manual ID lookup. The original, separately authorized update
    // endpoint and validation continue to handle the actual save.
    async function openSelectedAdditionUpdate() {
        const selected = currentSelected();
        if (!selected) return showLegacyError('entityMasterLegacy', 'Select an entity and load its master profile before editing.');
        const generation = ++editVersion;
        const host = byId('entityMasterLegacy');
        if (!host || !host.querySelector('#updateEntityModal')) return;
        try {
            const res = await read('profile', { entityId: selected.id });
            if (generation !== editVersion || !currentSelected() || Number(currentSelected().id) !== Number(selected.id)) return;
            const record = (res.rows || [])[0];
            if (!record || Number(value(record, 'ENTITY_ID')) !== Number(selected.id))
                throw new Error('The selected entity does not match the current database record.');
            const set = (field, fieldValue) => {
                const element = host.querySelector('#' + field);
                if (!element) throw new Error('Entity Addition update field ' + field + ' is unavailable.');
                element.value = fieldValue == null ? '' : String(fieldValue);
                if (element.tagName === 'SELECT' && element.value !== String(fieldValue))
                    throw new Error('The existing Entity Addition selector does not contain the entity\'s ' + field + ' value.');
                return element;
            };
            set('entTypeField', value(record, 'TYPE_ID'));
            set('entCodeField', value(record, 'CODE'));
            set('entNameField', value(record, 'NAME'));
            set('entDescField', value(record, 'DESCRIPTION'));
            set('entAuditableField', value(record, 'AUDITABLE') === 'Y' ? 'Y' : 'N');
            set('entAuditbyField', value(record, 'AUDITBY_ID'));
            const status = value(record, 'ACTIVE') === 'Y' ? 'Y' : 'N';
            const active = set('entActiveField', status);
            // Entity inactivation is a material closure event. The legacy
            // PKG_AD updater may delete mapping when ACTIVE='N'; prevent that
            // operation being initiated through this new edit route.
            active.disabled = true;
            const save = host.querySelector('#updateEntityModal [data-onclick*="saveUpdateChangesInAISEntity"]');
            if (save) save.disabled = status !== 'Y';
            window.g_entIdToUpdate = selected.id;
            if (!window.jQuery || !window.jQuery.fn || typeof window.jQuery.fn.modal !== 'function')
                throw new Error('Existing Entity Addition modal is not initialized.');
            window.jQuery(host.querySelector('#updateEntityModal')).modal('show');
            if (status !== 'Y') showLegacyError('entityMasterLegacy',
                'This entity is inactive. The old update procedure could delete historical mappings; saving from this workspace is blocked.');
        } catch (e) {
            if (generation === editVersion) showLegacyError('entityMasterLegacy', e.message);
        }
    }

    // Only the source entity selectors are replaced by the central selection.
    // The destination selectors, request validations, API URLs, CSRF, and
    // original reversal/shifting functions are unchanged.
    function bindEngagementSource(step, target) {
        const selected = currentSelected();
        if (!selected) return showLegacyError(target, 'Please select an entity in the register before opening engagement details.');
        const host = byId(target);
        if (!host) return;
        const shifting = step === 'ENGAGEMENT_SHIFTING';
        const selectorId = shifting ? 'engShiftCurrentEntity' : 'childposting';
        const picker = host.querySelector('#' + selectorId);
        const action = shifting ? 'engShiftGetEngagements' : 'getEngagementDetails';
        if (!picker || typeof window[action] !== 'function')
            return showLegacyError(target, 'The original ' + action + ' script is not ready. Open the operation again.');
        baseOptions(picker, 'Select current entity');
        addOption(picker, selected.id, selected.name);
        picker.value = String(selected.id);
        const relationship = host.querySelector('#' + (shifting ? 'engShiftRelationshipField' : 'RelationshipField'));
        const parent = host.querySelector('#' + (shifting ? 'engShiftControllingOffice' : 'controlingsearch'));
        [relationship, parent, picker].forEach(element => {
            if (!element) return;
            const row = element.closest('.row.col-md-12.mt-1');
            if (row) row.hidden = true;
        });
        const heading = host.querySelector('h3');
        const summary = document.createElement('div');
        summary.className = 'entity-operation-selection';
        summary.textContent = 'Using selected entity: ' + selected.name + ' (ID ' + selected.id +
            '). Source selection is controlled above; destination selection remains available where required.';
        if (heading) heading.insertAdjacentElement('afterend', summary);
        else host.prepend(summary);
        // This invokes the untouched business reader with precisely the same
        // ENTITY_ID parameter that the original dropdown passed.
        window[action]();
    }

    root.addEventListener('entity-workspace:operation-loaded', event => {
        const detail = event.detail || {};
        if (detail.step === 'SETUP_ENGAGEMENT_REVERSAL' || detail.step === 'ENGAGEMENT_SHIFTING')
            bindEngagementSource(detail.step, detail.target);
        if (detail.step === 'ENTITY_ADDITION' && detail.target === 'entityMasterLegacy')
            openSelectedAdditionUpdate();
    });

    // Combined creation is a controlled draft until a single transactional
    // Oracle procedure can insert master + mapping + Group + GM together.
    function fillTypes() {
        for (const target of ['newEntityType', 'newEntityParentType']) {
            const select = byId(target);
            if (!select) continue;
            baseOptions(select, target === 'newEntityType' ? 'Select entity type' : 'Select parent type');
            typeList.forEach(t => addOption(select, t.id, t.name));
        }
        groupTypes.splice(0, groupTypes.length, ...typeList.filter(t => /\bgroup\b/i.test(t.name)));
        gmTypes.splice(0, gmTypes.length, ...typeList.filter(t => /\b(gm|general manager)\b/i.test(t.name)));
    }
    root.addEventListener('entity-workspace:types', event => {
        typeList.splice(0, typeList.length, ...((event.detail && event.detail.types) || []).filter(x => Number(x.id) > 0));
        fillTypes();
        if (!byId('entityUnifiedAddPanel')?.hidden) initializeEntityPickers();
    });

    async function populateChoices(target, typeId, search) {
        const select = byId(target);
        if (!select) return;
        const token = (pickerVersions.get(target) || 0) + 1;
        pickerVersions.set(target, token);
        baseOptions(select, 'Loading authorized choices...');
        select.disabled = true;
        if (!Number(typeId)) {
            baseOptions(select, 'Select a valid entity type first');
            return;
        }
        try {
            const results = await read('search', { typeId: Number(typeId), query: search || '',
                status: 'ACTIVE', offset: 0, pageSize: 200 });
            if (pickerVersions.get(target) !== token) return;
            baseOptions(select, 'Choose an entity');
            for (const item of (results.rows || [])) {
                const id = value(item, 'ENTITY_ID'), name = value(item, 'NAME');
                if (Number(id) > 0) addOption(select, id, name);
            }
            select.disabled = !(results.rows || []).length;
            if (Number(results.totalRows || 0) > (results.rows || []).length)
                setMessage('Only the first 200 matching entities are shown. Narrow the name search to find the correct authorized entity.', true);
            else setMessage('Select a Parent Type, then its Parent. GM Office and Group are loaded from their verified entity-type categories.', false);
        } catch (e) {
            if (pickerVersions.get(target) !== token) return;
            baseOptions(select, 'Authorized lookup unavailable');
            setMessage(e.message, true);
        }
    }
    function uniqueCategory(list, target) {
        const select = byId(target);
        if (!select) return;
        if (list.length !== 1) {
            baseOptions(select, list.length
                ? 'Multiple matching categories; verify type mapping'
                : 'Category type not identified');
            select.disabled = true;
            setMessage('The Group/GM type must be verified in the entity type catalog before a reference can be chosen.', true);
            return;
        }
        populateChoices(target, list[0].id, byId(target + 'Search')?.value.trim());
    }
    function initializeEntityPickers() {
        if (!typeList.length) {
            setMessage('Loading entity-type definitions. If unavailable, verify the authorized Oracle type reader.', true);
            return;
        }
        fillTypes();
        uniqueCategory(groupTypes, 'newEntityGroup');
        uniqueCategory(gmTypes, 'newEntityGm');
        // Audit clusters use the same existing approved PKG_AD reader.
        if (!additionsReady) {
            additionsReady = true;
            read('auditby').then(data => {
                const select = byId('newEntityAuditBy');
                if (!select) return;
                baseOptions(select, 'Select audit cluster');
                for (const item of (data.rows || [])) {
                    if (Number(item.id) > 0) addOption(select, item.id, item.name);
                }
            }).catch(e => setMessage('Audit cluster lookup: ' + e.message, true));
        }
    }

    const add = byId('entityUnifiedAddButton');
    if (add) add.addEventListener('click', () => {
        const panel = byId('entityUnifiedAddPanel');
        if (!panel) return;
        panel.hidden = false;
        const legacy = byId('entityMasterLegacy');
        if (legacy) legacy.replaceChildren();
        initializeEntityPickers();
        byId('newEntityName')?.focus();
    });
    const close = byId('entityUnifiedAddClose');
    if (close) close.addEventListener('click', () => {
        const panel = byId('entityUnifiedAddPanel');
        if (panel) panel.hidden = true;
    });
    const edit = byId('entityUnifiedEditButton');
    if (edit) edit.addEventListener('click', () => {
        const panel = byId('entityUnifiedAddPanel');
        if (panel) panel.hidden = true;
    });

    const parentType = byId('newEntityParentType');
    if (parentType) parentType.addEventListener('change', () =>
        populateChoices('newEntityParent', parentType.value, byId('newEntityParentSearch')?.value.trim()));

    for (const [id, choices, category] of [
        ['newEntityParentSearch', 'newEntityParent', null],
        ['newEntityGmSearch', 'newEntityGm', gmTypes],
        ['newEntityGroupSearch', 'newEntityGroup', groupTypes]
    ]) {
        const control = byId(id);
        if (!control) continue;
        let timer;
        control.addEventListener('input', () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                if (category) uniqueCategory(category, choices);
                else populateChoices(choices, byId('newEntityParentType')?.value, control.value.trim());
            }, 250);
        });
    }

    if (!readEnabled) {
        const addBtn = byId('entityUnifiedAddButton');
        if (addBtn) addBtn.disabled = true;
    }
}());

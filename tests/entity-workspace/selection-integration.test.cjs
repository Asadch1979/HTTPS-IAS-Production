const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

const base = path.resolve(__dirname, '../..');
const bridge = fs.readFileSync(path.join(base, 'AIS/wwwroot/js/entity-workspace-selection.js'), 'utf8');
const main = fs.readFileSync(path.join(base, 'AIS/wwwroot/js/administration-entity-dashboard.js'), 'utf8');
const view = fs.readFileSync(path.join(base, 'AIS/Views/AdministrationPanel/Entity_Workspace.cshtml'), 'utf8');
const addView = fs.readFileSync(path.join(base, 'AIS/Views/AdministrationPanel/DashboardPartials/_EntityUnifiedAddition.cshtml'), 'utf8');

function make(tag) {
    return {
        tagName: tag.toUpperCase(), children: [], style: {}, classList: { add() {}, remove() {}, toggle() {} },
        replaceChildren(...items) { this.children = items; },
        appendChild(child) { this.children.push(child); return child; },
        prepend(child) { this.children.unshift(child); },
        insertAdjacentElement(_where, child) { this.inserted = child; },
        closest() { return this.row || null; },
        querySelector() { return null; },
        addEventListener() {},
        set textContent(value) { this.text = value; },
        get textContent() { return this.text; }
    };
}

function setup() {
    const listeners = {};
    const root = {
        dataset: { readEnabled: 'true', readUrl: '/entity-read' },
        addEventListener(event, callback) { listeners[event] = callback; },
        dispatch(type, detail) { if (listeners[type]) listeners[type]({ detail }); }
    };
    const heading = make('h3'), sourceRow = make('div');
    const relationRow = make('div');
    const childposting = make('select'), relationship = make('select'), office = make('select');
    childposting.row = sourceRow; office.row = sourceRow; relationship.row = relationRow;
    const currentEntity = make('select'), shiftRelationship = make('select'), shiftOffice = make('select');
    currentEntity.row = sourceRow; shiftOffice.row = sourceRow; shiftRelationship.row = relationRow;
    const reversalHost = make('div');
    const bySelector = {
        '#childposting': childposting, '#RelationshipField': relationship, '#controlingsearch': office,
        '#engShiftCurrentEntity': currentEntity, '#engShiftRelationshipField': shiftRelationship,
        '#engShiftControllingOffice': shiftOffice, 'h3': heading
    };
    reversalHost.querySelector = selector => bySelector[selector] || null;
    const controls = { entityWorkspace: root, entityEngagementLegacy: reversalHost, entityUnifiedEditButton: { disabled: true, addEventListener() {} } };
    let reversalCalls = 0, shiftCalls = 0;
    const sandbox = {
        document: { getElementById: id => controls[id] || null, createElement: make },
        window: { getEngagementDetails() { reversalCalls++; }, engShiftGetEngagements() { shiftCalls++; } },
        fetch() { throw Error('No network needed for source-selection test'); },
        URL, Map, Set, clearTimeout, setTimeout
    };
    vm.runInNewContext(bridge, sandbox);
    return { root, get reversalCalls() { return reversalCalls; },
        get shiftCalls() { return shiftCalls; }, childposting, currentEntity, relationRow, sourceRow };
}

test('engagement reversal uses the same selected entity without separate source dropdowns', () => {
    const s = setup();
    s.root.dispatch('entity-workspace:selected', { id: 112206, name: 'CHIEF FINANCIAL OFFICER', active: true });
    s.root.dispatch('entity-workspace:profile', { id: 112206, profile: [{ ENTITY_ID: '112206', NAME: 'CHIEF FINANCIAL OFFICER' }] });
    s.root.dispatch('entity-workspace:operation-loaded', { step: 'SETUP_ENGAGEMENT_REVERSAL', target: 'entityEngagementLegacy' });
    assert.equal(s.reversalCalls, 1);
    assert.equal(s.childposting.value, '112206');
    assert.equal(s.relationRow.hidden, true);
    assert.equal(s.sourceRow.hidden, true);
});

test('engagement shifting reuses selected source and leaves existing destination selection logic untouched', () => {
    const s = setup();
    s.root.dispatch('entity-workspace:selected', { id: 112206, name: 'CHIEF FINANCIAL OFFICER', active: true });
    s.root.dispatch('entity-workspace:profile', { id: 112206, profile: [{ ENTITY_ID: '112206' }] });
    s.root.dispatch('entity-workspace:operation-loaded', { step: 'ENGAGEMENT_SHIFTING', target: 'entityEngagementLegacy' });
    assert.equal(s.shiftCalls, 1);
    assert.equal(s.currentEntity.value, '112206');
    assert.doesNotMatch(bridge, /engShiftDestRelationshipField.*hidden|engShiftNewEntity.*hidden/);
});

test('no selected entity means no legacy engagement API is invoked', () => {
    const s = setup();
    s.root.dispatch('entity-workspace:operation-loaded', { step: 'SETUP_ENGAGEMENT_REVERSAL', target: 'entityEngagementLegacy' });
    assert.equal(s.reversalCalls, 0);
});

test('edit uses Entity Addition update with fresh profile; old manual update workflow is not used', () => {
    assert.match(view, /id="entityUnifiedEditButton" data-legacy-step="ENTITY_ADDITION"/);
    assert.doesNotMatch(view, /data-legacy-step="UPDATE_AUDITEE_ENTITIES"/);
    assert.match(bridge, /read\('profile', \{ entityId: selected\.id \}\)/);
    assert.match(bridge, /window\.g_entIdToUpdate = selected\.id/);
    assert.match(bridge, /#updateEntityModal/);
    assert.match(bridge, /active\.disabled = true/);
    assert.match(bridge, /save\.disabled = status !== 'Y'/);
    assert.match(main, /entity-workspace:profile/);
    assert.match(main, /entity-workspace:operation-loaded/);
});

test('new master and mapping are displayed as one draft but cannot submit a partial transaction', () => {
    assert.match(addView, /newEntityGroup/);
    assert.match(addView, /newEntityGm/);
    assert.match(addView, /newEntityParentType/);
    assert.match(addView, /newEntityParent/);
    assert.match(addView, /disabled title="Requires an approved atomic/);
    assert.match(bridge, /pageSize: 200/);
    assert.match(bridge, /populateChoices/);
    assert.doesNotMatch(bridge, /method:\s*'(POST|PUT|PATCH|DELETE)'/);
});

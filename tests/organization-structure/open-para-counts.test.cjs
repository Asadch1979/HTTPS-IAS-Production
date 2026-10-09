// Focused DOM harness: node tests/organization-structure/open-para-counts.test.cjs
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const elements = new Map();
class Element {
    constructor(tag = 'div') {
        this.tagName = tag; this.children = []; this.dataset = {}; this.style = {};
        this.textContent = ''; this.value = ''; this.hidden = false; this.disabled = false;
        this.scrollTop = 0; this.scrollLeft = 0; this.clientHeight = 0; this.clientWidth = 800;
        this.attributes = {}; this.offsetWidth = 1000; this.offsetHeight = 500; this.className = ''; this.listeners = {};
        this.classList = { contains: name => this.className.split(' ').includes(name), toggle: (name, force) => { const names = new Set(this.className.split(' ').filter(Boolean)); if (force ?? !names.has(name)) names.add(name); else names.delete(name); this.className = [...names].join(' '); }, add: () => {}, remove: () => {} };
    }
    set id(value) { elements.set(value, this); }
    append(...items) { items.forEach(item => { item.parentElement = this; this.children.push(item); }); }
    replaceChildren(...items) { this.children = []; this.append(...items); }
    showModal() { this.open = true; }
    close() { this.open = false; }
    setAttribute(name, value) { this.attributes[name] = value; }
    getBoundingClientRect() { return { left: 0, top: 0, width: 228, height: 153 }; }
    addEventListener(name, handler) { this.listeners[name] = handler; }
    get childElementCount() { return this.children.length; }
    querySelectorAll(selector) {
        const matches = [];
        const visit = node => {
            if (selector.startsWith('.') && node.className.split(' ').includes(selector.slice(1))) matches.push(node);
            if (selector === '[data-selected]' && node.dataset.selected) matches.push(node);
            node.children.forEach(visit);
        };
        this.children.forEach(visit); return matches;
    }
    querySelector(selector) { return this.querySelectorAll(selector)[0] || null; }
}
const app = new Element();
app.dataset = { nodesUrl: '/nodes', countsUrl: '/counts', pathUrl: '/path', canMove: 'true', previewUrl: '/preview', moveUrl: '/move' };
elements.set('organization-structure', app);
const get = id => {
    if (!elements.has(id)) elements.set(id, new Element());
    return elements.get(id);
};
// Synthetic fixtures matching the supplied observations; never production constants.
const nodes = [
    { entityId: 112222, entityName: 'Recovery and SAM Division', entityTypeId: 3, directChildCount: 2, isRoot: true },
    { entityId: 112250, entityName: 'Recovery Department', entityTypeId: 4, parentEntityId: 112222, directChildCount: 0 },
    { entityId: 112267, entityName: 'Non Performing Loans Department', entityTypeId: 4, parentEntityId: 112222, directChildCount: 0 }
];
const fixture = [
    { entityId: 112222, ownOpenParas: 0, subordinateOpenParas: 10, totalOpenParas: 10 },
    { entityId: 112250, ownOpenParas: 5, subordinateOpenParas: 0, totalOpenParas: 5 },
    { entityId: 112267, ownOpenParas: 5, subordinateOpenParas: 0, totalOpenParas: 5 }
];
let nextCounts = fixture, countsCalls = 0, releaseCounts, moveCalls = 0, releaseMove, postedMove;
const context = vm.createContext({
    document: { getElementById: get, createElement: tag => new Element(tag) },
    location: { href: 'https://example.test/org', pathname: '/org', search: '', hash: '' },
    history: { replaceState() {} }, URL, URLSearchParams,
    FormData: class extends Map { constructor() { super([['__RequestVerificationToken', 'test-token']]); } },
    requestAnimationFrame() {}, cancelAnimationFrame() {}, ResizeObserver: class { observe() {} },
    matchMedia: () => ({ matches: false }), setTimeout, clearTimeout,
    fetch: async (url, options) => {
        let result;
        if (new URL(url, 'https://example.test').pathname === '/nodes') result = { nodes, rootEntityId: 112222, isRestricted: true };
        else if (new URL(url, 'https://example.test').pathname === '/preview') {
            result = { entityId: 112250, entityName: 'Recovery Department', currentParentId: 112222,
                currentParentName: 'Recovery and SAM Division', newParentId: 112267,
                newParentName: 'Non Performing Loans Department', descendantEntities: 0 };
        } else if (url === '/move') {
            moveCalls++; postedMove = options;
            await new Promise(resolve => { releaseMove = resolve; }); result = { moveId: 'test-move' };
        } else if (url === '/counts') {
            countsCalls++;
            if (releaseCounts) await new Promise(resolve => { releaseCounts.resolve = resolve; });
            if (nextCounts instanceof Error) throw nextCounts;
            result = nextCounts;
        } else result = [{ entityId: 112222, entityName: nodes[0].entityName }];
        return { ok: true, status: 200, headers: { get: () => 'application/json' }, json: async () => result };
    }
});
let source = fs.readFileSync(path.join(__dirname, '../../AIS/wwwroot/js/organization-structure.js'), 'utf8');
source = source.replace(/    load\(\);\s*\}\)\(\);\s*$/, `
    globalThis.testApi = {
        load, loadCounts, select, fitChart, zoom, openMove,
        scale() { return scale; },
        seedView() { scale = 1.3; expanded.add(112250); pages.set(112222, 1); },
        state() { return JSON.stringify({ selected, scale, expanded: [...expanded], pages: [...pages], navExpanded: [...navExpanded] }); }
    };
})();`);
vm.runInContext(source, context);
const api = context.testApi;
const tick = () => new Promise(resolve => setImmediate(resolve));
const labels = () => get('org-chart').querySelectorAll('.org-card-count').map(item => item.textContent);
const details = () => get('org-detail-counts').children.map(row => row.children.map(item => item.textContent));

(async () => {
    releaseCounts = {};
    const loading = api.load(); await tick();
    assert.equal(countsCalls, 1, 'one scoped request on page load');
    assert(labels().every(label => label === 'Open paras: Loading…'));
    releaseCounts.resolve(); releaseCounts = null; await loading;
    assert.deepEqual(labels().sort(), ['Open paras: 10', 'Open paras: 5', 'Open paras: 5'].sort());
    assert.deepEqual(details(), [['Own open paras', '0'], ['Subordinate open paras', '10'], ['Total open paras', '10']]);
    assert.equal(get('org-retry').hidden, true, 'chart initialization succeeds');

    api.seedView();
    get('org-viewport').scrollTop = 123; get('org-viewport').scrollLeft = 456;
    get('org-tree').scrollTop = 99; get('org-search').value = 'Recovery'; get('org-filter').value = '4';
    const before = api.state(), cardNodes = get('org-chart').querySelectorAll('.org-card-count');
    nextCounts = fixture.map(row => ({ ...row, totalOpenParas: row.entityId === 112222 ? 12 : row.totalOpenParas }));
    await api.loadCounts();
    assert.equal(api.state(), before, 'refresh preserves selection, zoom, branches and pagination');
    assert.equal(get('org-viewport').scrollTop, 123); assert.equal(get('org-viewport').scrollLeft, 456);
    assert.equal(get('org-tree').scrollTop, 99); assert.equal(get('org-search').value, 'Recovery');
    assert.equal(get('org-filter').value, '4');
    assert.deepEqual(get('org-chart').querySelectorAll('.org-card-count'), cardNodes, 'cards stay in place');
    assert(labels().includes('Open paras: 12'), 'use returned total without adding child totals');

    nextCounts = [fixture[0], { ...fixture[1], totalOpenParas: 0 }];
    await api.loadCounts();
    assert(labels().includes('Open paras: 0'), 'a returned zero is displayed');
    assert(labels().includes('Open paras: Unavailable'), 'missing entity is not zero');
    assert.equal(get('org-counts-retry').hidden, false);
    api.select(112267);
    assert(details().every(row => row[1] === 'Unavailable'), 'new selection uses loaded counts map');

    nextCounts = new Error('database unavailable');
    await api.loadCounts();
    assert(labels().every(label => label === 'Open paras: Unavailable'));
    assert.equal(get('org-workspace').hidden, false, 'chart remains usable on failure');
    assert.equal(get('org-refresh-counts').disabled, false);
    nextCounts = fixture;
    await get('org-counts-retry').listeners.click();
    assert.equal(get('org-counts-retry').hidden, true, 'retry recovers');
    assert.deepEqual(details(), [['Own open paras', '5'], ['Subordinate open paras', '0'], ['Total open paras', '5']]);

    releaseCounts = {};
    const running = api.loadCounts(), calls = countsCalls;
    await api.loadCounts(); assert.equal(countsCalls, calls, 'duplicate refresh is suppressed');
    releaseCounts.resolve(); releaseCounts = null; await running;
    get('org-viewport').clientHeight = 500;
    get('org-chart').offsetWidth = 4000;
    api.fitChart();
    assert(api.scale() < .25, 'fit can go below 75 percent');
    const overview = api.scale();
    get('org-zoom-in').listeners.click();
    assert(api.scale() > overview && api.scale() < .3, 'manual zoom does not jump from overview');
    get('org-actual').listeners.click(); assert.equal(api.scale(), 1);
    api.fitChart(); get('org-focus').listeners.click(); assert.equal(api.scale(), 1);
    get('org-details-toggle').listeners.click();
    assert.equal(get('org-details').hidden, true);
    assert.equal(get('org-details-toggle').attributes['aria-expanded'], 'false');
    get('org-expand-chart').listeners.click(); assert.equal(get('org-navigator').hidden, true);
    assert.equal(get('org-expand-chart').attributes['aria-pressed'], 'true');
    get('org-expand-chart').listeners.click();
    assert.equal(get('org-navigator').hidden, false);
    assert.equal(get('org-details').hidden, true, 'expansion restores prior panels');
    get('org-reset').listeners.click();
    assert.equal(JSON.parse(api.state()).selected, 112222);
    assert.equal(get('org-search').value, ''); assert.equal(get('org-filter').value, '');
    assert.equal(get('org-back').disabled, true);
    assert.equal(api.scale(), 1);
    // A response from the previous hierarchy must never overwrite refreshed counts.
    nextCounts = fixture; releaseCounts = {};
    const stale = api.loadCounts(); const releaseOld = releaseCounts.resolve; releaseCounts = null;
    await api.load(true);
    nextCounts = fixture.map(row => ({ ...row, totalOpenParas: 999 }));
    releaseOld(); await stale;
    assert(!labels().includes('Open paras: 999'), 'late count response is ignored');
    nextCounts = fixture;
    api.select(112250); api.openMove();
    assert.equal(get('org-move-confirm').disabled, true);
    get('org-move-destination').value = '112267';
    await get('org-move-preview').listeners.click();
    assert.equal(get('org-move-confirm').disabled, true, 'reason is mandatory');
    get('org-move-reason').value = 'Fixture reason'; get('org-move-reason').listeners.input();
    assert.equal(get('org-move-confirm').disabled, false);
    get('org-move-destination').listeners.change();
    assert.equal(get('org-move-confirm').disabled, true, 'destination invalidates preview');
    await get('org-move-preview').listeners.click();
    const moving = get('org-move-form').listeners.submit({ preventDefault() {} });
    await get('org-move-form').listeners.submit({ preventDefault() {} });
    assert.equal(moveCalls, 1, 'duplicate move suppressed');
    assert.equal(postedMove.method, 'POST');
    assert.equal(postedMove.body.get('ExpectedParentId'), 112222);
    assert.equal(postedMove.body.get('__RequestVerificationToken'), 'test-token');
    assert.equal(get('org-move-cancel').disabled, true);
    releaseMove(); await moving;
    assert.equal(get('org-move-dialog').open, false);
    assert.equal(JSON.parse(api.state()).selected, 112250, 'moved selection preserved');
    console.log('PASS: move preview, mandatory reason, stale preview, duplicate suppression, authenticated POST token; controls, overview zoom, reset, panel restoration, late response protection;  scoped request, totals, details, loading, zero, missing result, failure, retry and preserved view');
})().catch(error => { console.error(error); process.exitCode = 1; });

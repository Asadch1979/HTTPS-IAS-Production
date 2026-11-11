const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const base = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(base, 'AIS/wwwroot/js/administration-entity-dashboard.js'), 'utf8');
const view = fs.readFileSync(path.join(base, 'AIS/Views/AdministrationPanel/Entity_Workspace.cshtml'), 'utf8');

function setup() {
    const panels = Array.from({ length: 4 }, (_, i) => ({ hidden: i !== 0 }));
    const tabs = panels.map((_, i) => ({
        attrs: { 'aria-controls': String(i) }, events: {},
        setAttribute(k, v) { this.attrs[k] = v; },
        getAttribute(k) { return this.attrs[k]; },
        addEventListener(k, f) { this.events[k] = f; },
        focus() { this.focused = true; }
    }));
    const root = {
        querySelectorAll(q) { return q === '[data-entity-tab]' ? tabs : []; },
        dataset: { readEnabled: 'false' },
        addEventListener() {}
    };
    const document = {
        getElementById(id) {
            if (id === 'entityWorkspace') return root;
            const m = String(id).match(/^entity-panel-(\d+)$/);
            return m ? panels[Number(m[1])] : null;
        }
    };
    // In the real view tab aria-controls is 'entity-panel-N'.
    tabs.forEach((tab, i) => { tab.attrs['aria-controls'] = 'entity-panel-' + i; });
    vm.runInNewContext(source, { document, window: {}, Map, URL, fetch() { throw Error('Unexpected request'); } });
    return { panels, tabs };
}
test('single workspace contains exactly four core management areas', () => {
    assert.equal((view.match(/data-entity-tab="@i"/g) || []).length, 1);
    assert.match(view, /Search, add, edit and mapping/);
    assert.match(view, /With paras, without paras, closure/);
    assert.match(view, /Reversal and engagement operations/);
    assert.match(view, /Requests, decisions and history/);
});
test('tab selection leaves one visible panel and accessible keyboard stop', () => {
    const { tabs, panels } = setup();
    tabs[2].events.click();
    assert.deepEqual(panels.map(p => p.hidden), [true, true, false, true]);
    assert.equal(tabs.filter(t => t.tabIndex === 0).length, 1);
    assert.equal(tabs[2].attrs['aria-selected'], 'true');
});
test('keyboard navigation wraps and supports Home and End', () => {
    const { tabs } = setup();
    let prevented = false;
    tabs[0].events.keydown({ key: 'ArrowLeft', preventDefault() { prevented = true; } });
    assert.equal(prevented, true);
    assert.equal(tabs[3].focused, true);
    tabs[3].events.keydown({ key: 'Home', preventDefault() {} });
    assert.equal(tabs[0].tabIndex, 0);
    tabs[0].events.keydown({ key: 'End', preventDefault() {} });
    assert.equal(tabs[3].tabIndex, 0);
});
test('Oracle data uses bounded read-only GET; all write actions remain gated', () => {
    assert.match(source, /root\.dataset\.readEnabled !== 'true'/);
    assert.match(source, /getData\('search'/);
    assert.match(source, /getData\('profile'/);
    assert.match(source, /getData\('closure'/);
    assert.match(source, /method: 'GET'/);
    assert.doesNotMatch(source, /method: '(POST|PUT|PATCH|DELETE)'|eval\(/);
    assert.match(view, /disabled>Execute closure/);
    assert.match(view, /disabled>Submit closure/);
});

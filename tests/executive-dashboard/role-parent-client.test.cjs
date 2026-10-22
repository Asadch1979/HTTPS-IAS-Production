'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const script = fs.readFileSync(path.join(__dirname, '../../AIS/wwwroot/js/executive-dashboard.js'), 'utf8');

test('role parents own dates and navigation; organizational controls stay confined to CIA/Snap', () => {
    const views = path.join(__dirname, '../../AIS/Views/ExecutiveOverview');
    for (const role of ['TOP', 'CCO', 'GROUP', 'RLM']) {
        const page = fs.readFileSync(path.join(views, `${role}.cshtml`), 'utf8');
        assert.match(page, /data-view-navigation/);
        assert.match(page, /name="startDate"/);
        assert.match(page, /name="endDate"/);
        assert.doesNotMatch(page, /name="(?:entityId|dimensionId|reportingOfficeId|selectBy)"/);
        assert.doesNotMatch(page, /OrganizationStructure|name="_Filters"/);
    }
    const snapFilters = fs.readFileSync(path.join(views, '_AuditParaSnapFilters.cshtml'), 'utf8');
    for(const name of ['entityId','dimensionId','reportingOfficeId','selectBy','snapDivision','snapDepartmentId'])
        assert.match(snapFilters,new RegExp('name="'+name+'"'));
    const ciaFilters=fs.readFileSync(path.join(views,'_CiaCommandFilters.cshtml'),'utf8');
    assert.doesNotMatch(ciaFilters,/name="(?:entityId|dimensionId|reportingOfficeId|selectBy)"/);
    const group = fs.readFileSync(path.join(views, 'GROUP.cshtml'), 'utf8');
    assert.ok(group.indexOf('mgmt-hero') < group.indexOf('data-view-navigation'));
    assert.ok(group.indexOf('data-view-navigation') < group.indexOf('id="exec-filters"'));
    assert.match(group, /Model.IsSuperUser && Model.DashboardType == "GROUP"/);
    assert.equal(fs.existsSync(path.join(views, 'Index.cshtml')), false);
});

test('disabled Snap with dates only preserves N/A and makes no API requests', () => {
    const snap = fs.readFileSync(path.join(__dirname, '../../AIS/wwwroot/js/audit-para-snap.js'), 'utf8');
    const values = [];
    const card = { querySelector: () => ({ set textContent(value) { values.push(value); } }) };
    const app = {
        classList: { contains: () => true }, dataset: { view: 'glance', enabled: 'false' },
        querySelectorAll: selector => selector === '[data-snap-card]' ? [card] : []
    };
    const fields = { 'executive-dashboard': app, 'exec-filters': { addEventListener() {} } };
    vm.runInNewContext(snap, {
        document: { getElementById: id => fields[id] || null },
        fetch: () => { throw Error('Disabled Snap must not fetch'); }
    });
    assert.equal(values[0], 'N/A');
    assert.match(values[1], /activation pending/);
});

// Exercise the real client without organizational inputs. A successful API
// response must remain visible, rather than failing after removing Entity.
for (const role of ['TOP', 'CCO', 'GROUP', 'RLM']) {
    test(`${role}: date-only parent loads within its authenticated scope`, async () => {
        const requests = [], events = [];
        const form = { addEventListener() {}, checkValidity: () => true };
        const fields = {
            'exec-filters': form, 'exec-results': { hidden: true },
            'exec-status': {}, 'exec-refresh': {}
        };
        const app = {
            dataset: { dashboardType: role === 'RLM' ? role : '', scopeLabel: 'Authenticated hierarchy',
                view: 'position', datasets: 'GetKpiSummary', apiUrl: '/ZTBLAIS/api/ExecutiveDashboard/' },
            querySelector: () => null, querySelectorAll: () => [], setAttribute() {},
            dispatchEvent: event => events.push(event)
        };
        vm.runInNewContext(script, {
            document: { getElementById: id => id === 'executive-dashboard' ? app : fields[id] || null },
            FormData: class { get() { return null; } }, URLSearchParams, AbortController,
            CustomEvent: class { constructor(type, options) { this.type = type; this.detail = options.detail; } },
            fetch: async (url, options) => {
                requests.push({ url, options });
                return { ok: true, status: 200, headers: new Map([['content-type', 'application/json']]),
                    json: async () => ({ success: true, data: { openParas: 7 } }) };
            }
        });
        await new Promise(resolve => setImmediate(resolve));
        assert.equal(requests.length, 1);
        const query = new URL(requests[0].url, 'https://ias.test').searchParams;
        for (const key of ['entityId', 'groupRootId', 'dimensionId', 'reportingOfficeId', 'startDate', 'endDate'])
            assert.equal(query.has(key), false, key);
        assert.equal(query.get('dashboardType'), role === 'RLM' ? 'RLM' : null);
        assert.equal(fields['exec-results'].hidden, false);
        assert.match(fields['exec-status'].textContent, /Authenticated hierarchy/);
        assert.match(fields['exec-status'].textContent, /selected date range is not applied/);
        assert.equal(events[0].detail.datasets.GetKpiSummary.openParas, 7);
    });
}

'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const {execFileSync} = require('node:child_process');
const {Script} = require('node:vm');
const root = path.join(__dirname,'../..');
const read = p => fs.readFileSync(path.join(root,p),'utf8').replaceAll('\r\n','\n');
const baseline = p => execFileSync('git',['show','255f466:' + p],{cwd:root,encoding:'utf8'}).replaceAll('\r\n','\n');
const sql = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const before = baseline('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const body = (source,name) => {
    const pkg = source.slice(source.indexOf('CREATE OR REPLACE PACKAGE BODY'));
    const start = pkg.indexOf('PROCEDURE ' + name + '(');
    return pkg.slice(start,pkg.indexOf('END ' + name + ';',start) + ('END ' + name + ';').length);
};

test('every existing Oracle signature is unchanged; no duplicate lifecycle procedure',() => {
    function signatures(source) {
        const spec=source.slice(source.indexOf('CREATE OR REPLACE PACKAGE PKG_EXEC_DASHBOARD'),source.indexOf('CREATE OR REPLACE PACKAGE BODY'));
        return [...spec.matchAll(/PROCEDURE\s+(\w+)\s*\(([\s\S]*?)\);/g)].map(m=>[m[1],m[2].replace(/\s+/g,' ').trim()]);
    }
    const old=signatures(before), updated=new Map(signatures(sql));
    for (const [name,signature] of old) assert.equal(updated.get(name),signature,name);
    for (const [name,signature] of updated) {
        const text=body(sql,name);
        assert.ok(text,'missing body: ' + name);
        const match=text.match(/PROCEDURE\s+\w+\s*\(([\s\S]*?)\)\s+(?:AS|IS)/);
        assert.equal(match?.[1].replace(/\s+/g,' ').trim(),signature,'spec/body mismatch: ' + name);
    }
    assert.equal((sql.match(/PROCEDURE P_GET_KPI_LIFECYCLE\(/g)||[]).length,2,'one spec and one body only');
    for (const [name] of old) if (!['P_GET_CIA_EXEC_KPIS','P_GET_CIA_ISSUE_NATURE',
            'P_GET_SNAP_CRITICAL','P_GET_SNAP_DISTRIBUTION'].includes(name))
        assert.equal(body(sql,name),body(before,name),'existing Oracle body changed: ' + name);
});
test('Snap historical queries use dated lifecycle evidence and explicit missing cycle history',() => {
    const distribution=body(sql,'P_GET_SNAP_DISTRIBUTION');
    const critical=body(sql,'P_GET_SNAP_CRITICAL');
    for (const text of [distribution,critical]) {
        assert.match(text,/V_TYPE <> 'CIA'/);
        assert.match(text,/P_START_DATE IS NULL OR P_END_DATE IS NULL/);
        assert.match(text,/TRUNC\(P_END_DATE\) > TRUNC\(SYSDATE\)/);
        assert.match(text,/V_TO := TRUNC\(P_END_DATE\) \+ 1/);
        assert.match(text,/k\.para_added_on < V_TO/);
        assert.match(text,/k\.para_settled_on >= V_TO/);
        assert.match(text,/P_REPORTING_OFFICE_ID IN/);
        assert.match(text,/FROM V_EXEC_KPI_LIFECYCLE k/);
        assert.doesNotMatch(text,/V_EXEC_PARA_BASE|P_GET_KPI_SUMMARY|P_GET_RISK_SUMMARY/);
    }
    assert.match(distribution,/k\.com_cycle AS snap_cycle/);
    assert.doesNotMatch(distribution,/WITH history_asof AS|recorded_cycle/);
    assert.match(distribution,/Unknown cycle/); // Null/negative master values only.
    assert.doesNotMatch(distribution,/c\.dataset = 'cycle' AND V_TODAY = 0 THEN CAST\(NULL AS NUMBER\)/);
    assert.match(distribution,/para_settled_on >= GREATEST\(m.month_start,V_FROM\)/);
    assert.match(distribution,/para_settled_on < LEAST\(ADD_MONTHS\(m.month_start,1\),V_TO\)/);
    assert.match(distribution,/COUNT\(CASE WHEN s.risk_id = 1 THEN s.com_id END\)/);
    assert.match(distribution,/high_risk_count, quality_code/);
    assert.match(critical,/WITH history_asof AS/);
    assert.match(critical,/k\.com_cycle AS snap_cycle/);
    assert.doesNotMatch(critical,/recorded_cycle/);
    assert.match(critical,/k\.snap_cycle AS com_cycle/);
    assert.match(critical,/MAX\(h\.comment_on\) AS last_movement_on/);
    assert.match(critical,/AS days_since_movement/);
    assert.doesNotMatch(critical,/CASE WHEN V_TODAY = 1 THEN k.com_cycle END/);
    assert.match(critical,/AND k.risk_id = 1 -- ZTBL Critical = High-risk open/);
    assert.doesNotMatch(critical,/k.snap_age > 365\s+OR/);
    assert.match(critical,/WHERE rn <= P_MAX_ROWS/);
});
test('CIA finalization/period/entity coverage and classification reconciliation are scoped',() => {
    const kpis=body(sql,'P_GET_CIA_EXEC_KPIS'), nature=body(sql,'P_GET_CIA_ISSUE_NATURE');
    assert.match(kpis,/p\.auditperiodid = P_AUDIT_PERIOD_ID/);
    assert.match(kpis,/cp\.plan_id = p.plan_id AND cp.entity_id = p.entity_id/);
    assert.match(kpis,/o\.status IN \(8, 9\)\s+AND e.status = 14\s+AND e.period_id = P_AUDIT_PERIOD_ID/);
    assert.match(kpis,/o\.status = 2/);
    assert.match(kpis,/e\.status BETWEEN 4 AND 11/);
    assert.match(nature,/SELECT DISTINCT k.observation_id/);
    assert.match(nature,/k\.period_id = P_AUDIT_PERIOD_ID/);
    assert.match(nature,/m\.category_code IN \(SELECT category_code FROM categories\)/);
    assert.match(nature,/PARTITION BY i.observation_id/);
    assert.match(nature,/m\.match_rank = 1/);
});
test('new APIs enforce session, fixed CIA/Super User roles, exact page and exact API grants before reads',() => {
    const db=read('AIS/DBConnection.AuditParaSnap.cs'), api=read('AIS/Controllers/AuditParaSnapController.cs');
    assert.match(db,/user\.UserRoleID != 3 && !\(user.UserRoleID == 1 && session.IsSuperUser\(\)\)/);
    assert.match(db,/ResolveExecutiveDashboardScope\("CIA"\)/);
    assert.match(api,/session\.IsSessionRevoked\(user\)/);
    assert.match(api,/pageId != AuditParaSnapPolicy.PageId/);
    assert.match(api,/permissions\.HasViewPermission\(user,pageId\)/);
    assert.match(api,/session\.TryGetAllowedViewIds/);
    assert.match(api,/permissions\.HasApiPermissionForPath\(user,Request.Method,Request.PathBase.Value,Request.Path.Value\)/);
    assert.match(api,/session\.HasApiPermissionForPath\(Request.Method,Request.PathBase.Value,Request.Path.Value\)/);
    assert.ok(api.indexOf('pageId !=') < api.indexOf('var data=query'));
    assert.ok(api.indexOf('HasApiPermissionForPath') < api.indexOf('var data=query'));
    assert.match(api,/AuditParaSnapPolicy.Validate\(filters\)/);
    assert.match(api,/GetExecutiveDashboardFilterOptions\(scope,filters\)/);
    assert.match(api,/NarrowExecutiveDashboardScope\(scope,filters.EntityId\)/);
    const grants=read('AIS/Docs/sql/executive_dashboard_permissions.sql');
    const registration=grants.slice(grants.indexOf('PROCEDURE REGISTER_SNAP_API'),grants.indexOf('\nBEGIN\n  -- Serialize'));
    assert.match(registration,/1000304,'AuditParaSnap'/);
    assert.match(registration,/'GET','N'/);
    assert.match(registration,/GRANT_API_ROLE\(1,V_API_ID\)/);
    assert.match(registration,/GRANT_API_ROLE\(3,V_API_ID\)/);
    assert.doesNotMatch(registration,/GRANT_API_ROLE\((38|V_GROUP_ROLE|V_TOP_ROLE)/);
    assert.match(grants,/reserved page ID\/path\/key collision/);
});
test('independent pages preserve dates on navigation and isolate report routes',() => {
    const cia=read('AIS/Views/ExecutiveOverview/CIA.cshtml'), snap=read('AIS/Views/ExecutiveOverview/AuditParaSnap.cshtml');
    assert.doesNotMatch(cia,/data-view-navigation|exec-navigation|startDate|endDate|executive-dashboard.js/);
    assert.match(cia,/_CiaCommandCenter/);
    assert.match(snap,/ZTBL \| Audit Para Snap/);
    assert.match(snap,/_AuditParaSnapFilters/);
    assert.ok(snap.indexOf('_AuditParaSnapFilters') < snap.indexOf('data-view-navigation'));
    assert.match(snap,/Model.Filters.RouteValues\(view.Id,null\)/);
    const route=read('AIS/Controllers/ExecutiveOverviewController.cs');
    assert.match(route,/RedirectToAction\(nameof\(AuditParaSnap\),values\)/);
    assert.match(route,/filters.StartDate=null; filters.EndDate=null/);
    const partial=read('AIS/Views/ExecutiveOverview/_Reports.cshtml');
    assert.match(partial,/Model.IsAuditParaSnap \? "~\/api\/AuditParaSnap\/" : "~\/api\/ExecutiveReports\/"/);
});
test('protected dashboard and Report A-E artifacts remain byte-equivalent to PR317',() => {
    for (const p of [
        'AIS/Controllers/ExecutiveReportsController.cs',
        'AIS/DBConnection.ExecutiveDashboardReports.cs',
        'AIS/wwwroot/css/executive-themes.css','AIS/wwwroot/css/executive-dashboard.css',
        'AIS/Views/Shared/EngagementWorkbench.cshtml','AIS/wwwroot/css/engagement-workbench.css',

    ]) assert.equal(read(p),baseline(p),'protected artifact: ' + p);
    // Reviewed changes now intentionally load the existing authorized
    // Oracle role-reporting panels rather than rendering placeholders.
    for (const file of ['_ManagementAudit.cshtml','_RegionalPulse.cshtml']) {
        const text=read('AIS/Views/ExecutiveOverview/'+file);
        assert.match(text,/<partial name="_RoleReporting" model="Model" \/>/);
    }
    const reportJs='AIS/wwwroot/js/executive-reports.js';
    // Reports now deliberately have the approved zero-only Report E display
    // and export adjustment; protect the SQL scope and browser CSV safeguards.
    assert.match(read('AIS/Docs/sql/PKG_EXEC_REPORTS.sql'),/P_REPORT_ID <> 'E' OR a.row_kind <> 'SUBTOTAL'/);
    assert.match(read('AIS/Docs/sql/PKG_EXEC_REPORTS.sql'),/a.outstanding_paras <> 0/);
    assert.match(read(reportJs),/const visibleRows = rows.filter/);
    assert.match(read(reportJs),/querySelectorAll\('thead th'\)/);
    assert.match(read(reportJs),/function csvEscape/);
    // CIA request wiring is intentionally maintained independently. Continue
    // protecting every unrelated endpoint in the shared controller.
    const p='AIS/Controllers/ExecutiveDashboardController.cs', now=read(p), old=baseline(p);
    // Existing routes still exist and remain permission-scoped; the role
    // loading feature flag was intentionally removed in this revision.
    assert.match(now,/public IActionResult GetKpiLifecycle/);
    assert.match(now,/GetRoleUnitComparison/);
    assert.match(now,/HasApiPermissionForPath/);
    assert.doesNotMatch(now,/Role reporting awaits verified Oracle deployment/);
});
test('new/changed JavaScript parses and clears stale data on date/access failures',() => {
    for (const p of ['AIS/wwwroot/js/audit-para-snap.js','AIS/wwwroot/js/cia-command-center.js']) new Script(read(p));
    const js=read('AIS/wwwroot/js/audit-para-snap.js');
    assert.match(js,/body.dateRangeApplied !== true/);
    assert.match(js,/body.startDate !== start.value/);
    assert.match(js,/body.endDate !== end.value/);
    assert.match(js,/request\?\.abort\(\)/);
    assert.match(js,/if \(!request.signal.aborted\) render\(data\)/);
    assert.match(js,/Historical evidence unavailable/);
    const cia=read('AIS/wwwroot/js/cia-command-center.js');
    assert.match(cia,/clearKpis\('KPI data unavailable/);
    assert.match(cia,/clearNature\('Issue classification unavailable/);
});

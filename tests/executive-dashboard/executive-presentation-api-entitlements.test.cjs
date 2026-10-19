'use strict';
// IAS: presentation + executive dashboard API dependency contracts.
// Static source tests; DB compilation and authenticated user testing remain mandatory.
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname,'../..');
const read = p => fs.readFileSync(path.join(root,p),'utf8');
const snap = read('AIS/Models/AuditParaSnapData.cs');
const partial = read('AIS/Views/ExecutiveOverview/_AuditParaSnapAnalysis.cshtml');
const snapJs = read('AIS/wwwroot/js/audit-para-snap.js');
const dashboardJs = read('AIS/wwwroot/js/executive-dashboard.js');
const sql = read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql');
const model = read('AIS/Models/ExecutiveDashboardModels.cs');
const reader = read('AIS/DBConnection.ExecutiveDashboard.cs');
const snapReader = read('AIS/DBConnection.AuditParaSnap.cs');
const manifest = read('AIS/Docs/sql/executive_dashboard_page_api_manifest.sql');
const permissions = read('AIS/Docs/sql/PKG_AD.sql');
const page = read('AIS/Services/PageIdResolver.cs');
const route = read('AIS/DBConnection.ExecutiveDashboard.cs');

test('Snap tabs do not repeat all nine At-a-Glance KPIs and fetch only visible datasets',()=>{
  assert.match(snap,/Definition\("position","Audit Para Position",\s*new\[\]\{"openingOpenParas","paraAdded","parasSettled","closingOpenParas"\}/);
  assert.match(snap,/Definition\("settlement","Settlement & Performance",\s*new\[\]\{"parasSettled","periodSettlementRate"\},new\[\]\{"trend"\}/);
  assert.match(snap,/Definition\("aging","Aging & Compliance",\s*new\[\]\{"over1YearParas","over3ComplianceCycles"\}/);
  assert.match(partial,/Model.SelectedView.Kpis.Contains\(card.Key\)/);
  assert.match(partial,/Model.SelectedView.Id == "risk" \|\| Model.SelectedView.Id == "glance"/);
  assert.match(snapJs,/if \(app.querySelector\('\[data-snap-card\]'\)\)/);
  assert.match(snapJs,/if \(document.getElementById\('exec-critical'\)\)/);
});
test('selected-period settlement chart returns all-settled and high-risk, as SVG lines',()=>{
  const dist=sql.slice(sql.lastIndexOf('PROCEDURE P_GET_SNAP_DISTRIBUTION('),
    sql.indexOf('END P_GET_SNAP_DISTRIBUTION;',sql.lastIndexOf('PROCEDURE P_GET_SNAP_DISTRIBUTION(')));
  assert.match(dist,/COUNT\(CASE WHEN s.risk_id = 1 THEN s.com_id END\)/);
  assert.match(dist,/high_risk_count, quality_code/);
  assert.match(snapReader,/HighRiskCount=ExecutiveNullableCount\(r,"HIGH_RISK_COUNT"\)/);
  assert.match(snapJs,/function renderMonthlyTrend\(rows\)/);
  assert.match(snapJs,/node\('polyline'/);
  assert.match(snapJs,/highRiskCount/);
});
test('MTD settlement rate is separate from lifetime settlement ratio',()=>{
  assert.match(sql,/AS settlement_mtd_rate/);
  assert.match(sql,/AS settlement_ratio/);
  assert.match(sql,/p\.setteled_on >= TRUNC\(SYSDATE,'MM'\)/);
  assert.match(model,/decimal\? SettlementMtdRate/);
  assert.match(reader,/GetDecimal\(reader.GetOrdinal\("SETTLEMENT_MTD_RATE"\)\)/);
  const views=read('AIS/Models/ExecutiveDashboardViews.cs');
  assert.match(views,/new\[\] \{ "settledMtd", "settlementMtdRate" \}/);
  assert.match(dashboardJs,/MTD Settlement Rate/);
  assert.match(dashboardJs,/opening outstanding \+ new paras/);
});
test('Risk charts hide Unrated without deleting or relabeling source records',()=>{
  assert.match(dashboardJs,/rows = rows.filter\(row => \[1,2,3\].includes/);
  assert.match(dashboardJs,/Share of rated paras/);
  assert.match(snapJs,/name === 'risk' && \(row.bucketCode === '0'/);
  assert.match(sql,/SELECT 0, 'Unrated', 4/);
});
test('Critical definition and verbose disclaimer clean-up retain source metadata',()=>{
  assert.match(read('AIS/Views/ExecutiveOverview/_Critical.cshtml'),/Critical = open High-Risk paras/);
  assert.doesNotMatch(read('AIS/Views/ExecutiveOverview/AuditParaSnap.cshtml'),
    /Activity uses qualifying lifecycle event dates\. Historical balances/);
  assert.match(snapJs,/card.title = periodRate/);
  assert.match(dashboardJs,/card.title = quality === 'estimated'/);
});
test('dedicated RLM page is not the Organization Structure shared permission',()=>{
  assert.match(page,/\["\/ExecutiveOverview\/RLM"\] = 1000305/);
  assert.doesNotMatch(page,/AddAlias\(updated, "\/ExecutiveOverview\/RLM"/);
  assert.match(route,/new ExecutiveDashboardScope\(user, "RLM", "\/ExecutiveOverview\/RLM"/);
  assert.match(read('AIS/Controllers/ExecutiveOverviewController.cs'),/pageIds.ResolvePageId\("\/ExecutiveOverview\/RLM"\)/);
  assert.match(manifest,/ExecutiveOverview\/RLM/);
});
test('each executive page has explicit API dependencies and assignments preserve sharing',()=>{
  const entries=[...manifest.matchAll(/SELECT (100030\d) page_id,'(\w+)' controller_name,'(\w+)' action_name FROM dual/g)];
  assert.equal(entries.length,88,'44-page/API dependencies appear in preflight and MERGE');
  const unique=new Set(entries.map(x=>x[1]+':'+x[2]+':'+x[3]));
  assert.equal(unique.size,44);
  for(const id of [1000300,1000301,1000302,1000303,1000304,1000305])
    assert.ok(entries.some(x=>Number(x[1])===id),'no API mapping for '+id);
  assert.match(permissions,/IF PAGEID BETWEEN 1000300 AND 1000305 THEN/);
  assert.match(permissions,/JOIN T_AU_API_MASTER m ON m.API_ID=d.API_ID/);
  assert.match(permissions,/WHEN MATCHED THEN UPDATE SET p.IS_ACTIVE='Y'/);
  assert.match(permissions,/NOT EXISTS\s+\(SELECT 1 FROM T_MENU_PAGES_GROUPMAP g/);
  assert.match(permissions,/-- Non-executive IAS page assignment unchanged/);
  assert.match(manifest,/This script DOES NOT activate API masters/);
});
test('both JavaScript files are syntax valid',()=>{
  for(const p of ['AIS/wwwroot/js/audit-para-snap.js','AIS/wwwroot/js/executive-dashboard.js'])
    assert.doesNotThrow(()=>new vm.Script(read(p),{filename:p}));
});

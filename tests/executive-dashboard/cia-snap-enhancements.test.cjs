'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const {execFileSync}=require('node:child_process');
const root=path.resolve(__dirname,'../..');
const read=p=>fs.readFileSync(path.join(root,p),'utf8').replace(/\r\n/g,'\n');
const baseline=p=>execFileSync('git',['show','8f18681:'+p],{cwd:root,encoding:'utf8'}).replace(/\r\n/g,'\n');
const body=(s,name)=>{const p=s.slice(s.indexOf('CREATE OR REPLACE PACKAGE BODY'));const start=p.indexOf('PROCEDURE '+name+'(');return p.slice(start,p.indexOf('END '+name+';',start)+('END '+name+';').length);};
test('unrelated Oracle procedure bodies and lifecycle calculations remain unchanged',()=>{
 const file='AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql',before=baseline(file),after=read(file);
 const allowed=new Set(['P_GET_CIA_EXEC_KPIS','P_GET_CIA_ISSUE_NATURE','P_GET_KPI_LIFECYCLE','P_GET_SNAP_DISTRIBUTION','P_GET_SNAP_CRITICAL']);
 for(const match of before.slice(before.indexOf('CREATE OR REPLACE PACKAGE BODY')).matchAll(/PROCEDURE (\w+)\(/g))
  if(!allowed.has(match[1]))assert.equal(body(after,match[1]),body(before,match[1]),match[1]);
 const calculations=s=>{const p=body(s,'P_GET_KPI_LIFECYCLE');return p.slice(p.indexOf('\n      SELECT'),p.indexOf('FROM V_EXEC_KPI_LIFECYCLE'));};
 assert.equal(calculations(after),calculations(before));
 for(const file of ['AIS/Docs/sql/V_EXEC_KPI_LIFECYCLE.sql','AIS/Models/ExecutiveDashboardModels.cs','AIS/Models/ExecutiveReportModels.cs'])
  assert.equal(file.endsWith('ExecutiveDashboardModels.cs')
    ? read(file).replace('        // Populated only by the authorized CCO summary projection.\n        public long? OpeningCases { get; set; }\n        public long? ParaAdded { get; set; }\n','')
    : read(file),baseline(file),file);
});
test('Oracle extended signatures match their package bodies and exact-COM_ID precedes ranking',()=>{
 for(const file of ['AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql','AIS/Docs/sql/PKG_EXEC_REPORTS.sql']){
  const source=read(file),spec=source.slice(source.indexOf('CREATE OR REPLACE PACKAGE PKG_'),source.indexOf('CREATE OR REPLACE PACKAGE BODY'));
  for(const m of spec.matchAll(/PROCEDURE (\w+)\(([\s\S]*?)\);/g)){
   const definition=body(source,m[1]).match(/^PROCEDURE \w+\(([\s\S]*?)\)\s+(?:AS|IS)/);
   assert.equal(definition?.[1].replace(/\s+/g,' ').trim(),m[2].replace(/\s+/g,' ').trim(),m[1]);
  }
 }
 const critical=body(read('AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql'),'P_GET_SNAP_CRITICAL');
 assert.ok(critical.indexOf('k.com_id = P_COM_ID')<critical.indexOf('ROW_NUMBER()'));
 assert.match(critical,/P_SNAP_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_SNAP_AUDITED_BY_ID/);
});
test('nine glance KPI keys, two charts and nature reconciliation remain intact',()=>{
 const source=read('AIS/Models/AuditParaSnapData.cs'),before=baseline('AIS/Models/AuditParaSnapData.cs');
 const glance=s=>s.slice(s.indexOf('Definition("glance"'),s.indexOf('Definition("position"'));
 assert.equal(glance(source),glance(before));
 const partial=read('AIS/Views/ExecutiveOverview/_AuditParaSnapAnalysis.cshtml');
 assert.match(partial,/@if \(Model.SelectedView.Id == "risk"\)/);
 assert.doesNotMatch(partial,/Id == "risk" \|\|/);
 assert.match(source,/Definition\("position","Audit Performance Analysis",\s*Array.Empty<string>\(\)/);
 assert.match(source,/Definition\("risk","Critical Audit Observations"[^\n]*Array.Empty<string>\(\)/);
 const js=read('AIS/wwwroot/js/cia-command-center.js');assert.match(js,/mapped \+ unmapped !== identified/);
});
class Element {
 constructor(tag='div'){this.tagName=tag;this.children=[];this.dataset={};this.style={};this.events={};this.classList={add(){},contains:()=>false};this.value='';this._text='';}
 set textContent(value){this._text=String(value);this._html='';this.children=[];}get textContent(){return this._text+this.children.map(c=>c.textContent??c).join('');}
 set innerHTML(value){this._html=value;this._text='';this.children=[];}get innerHTML(){return this._html||'';}
 append(...items){this.children.push(...items);}replaceChildren(...items){this._text='';this._html='';this.children=items;}
 addEventListener(name,fn){this.events[name]=fn;}setAttribute(){}querySelectorAll(){return [];}checkValidity(){return true;}
 showModal(){this.open=true;}close(){this.open=false;this.events.close?.();}
}
function client(view,reply){
 const ids={},app=ids['executive-dashboard']=new Element();app.classList.contains=n=>n==='exec-snap';app.dataset={view,apiUrl:'/api/AuditParaSnap/'};
 const form=ids['exec-filters']=new Element();ids['exec-start-date']=Object.assign(new Element(),{value:'2026-01-01'});ids['exec-end-date']=Object.assign(new Element(),{value:'2026-10-21'});ids['exec-status']=new Element();
 if(view==='aging')for(const id of ['exec-aging','exec-cycle'])ids[id]=new Element();
 if(view==='risk')for(const id of ['snap-para-dialog','snap-para-text','snap-para-close'])ids[id]=new Element();
 app.querySelector=()=>null;
 const context={document:{getElementById:id=>ids[id]??null,createElement:tag=>new Element(tag)},URL,URLSearchParams,AbortController,
  FormData:class{*[Symbol.iterator](){yield ['startDate','2026-01-01'];yield ['endDate','2026-10-21'];yield ['view',view];}},
  fetch:async url=>{context.requestUrl=url;return {ok:reply?.success!==false,redirected:false,headers:{get:()=> 'application/json'},json:async()=>({success:true,dateRangeApplied:true,startDate:'2026-01-01',endDate:'2026-10-21',...reply})};}};
 const script=read('AIS/wwwroot/js/audit-para-snap.js').replace(/    load\(\);\n\}\)\(\);\s*$/,'    globalThis.api={renderKpis,renderDistribution,viewPara,resetResults};\n})();');
 vm.runInNewContext(script,context);return {...context,ids,get requestUrl(){return context.requestUrl;}};
}
test('actual aging renderer shows percentages only with reconciled scoped closing stock',()=>{
 const c=client('aging'),rows=[{dataset:'aging',bucketName:'Over three years',paraCount:2,qualityCode:'E'},{dataset:'aging',bucketName:'Other',paraCount:8,qualityCode:'E'},{dataset:'cycle',bucketName:'Zero cycles',paraCount:10,qualityCode:'E'}];
 c.api.renderKpis({closingOpenParas:10,closingEstimated:true});c.api.renderDistribution(rows);
 assert.match(c.ids['exec-aging'].textContent,/20.00%/);assert.match(c.ids['exec-cycle'].textContent,/100.00%/);
 c.api.renderKpis({closingOpenParas:11,closingEstimated:true});assert.doesNotMatch(c.ids['exec-aging'].textContent,/20.00%/);assert.match(c.ids['exec-aging'].textContent,/N\/A/);
 rows[2].qualityCode='N';c.api.renderKpis({closingOpenParas:10,closingEstimated:true});c.api.renderDistribution(rows);assert.match(c.ids['exec-cycle'].textContent,/N\/A/);
});
test('actual para modal renders secured sanitized HTML and clears on filter invalidation',async()=>{
 const html='<p>Readable <strong>finding</strong></p>',c=client('risk',{data:{comId:42,html}});
 await c.api.viewPara(42);assert.equal(c.ids['snap-para-dialog'].open,true);assert.equal(c.ids['snap-para-text'].innerHTML,html);assert.equal(c.ids['snap-para-text'].children.length,0);
 assert.match(c.requestUrl,/GetParaText\?/);assert.match(c.requestUrl,/comId=42/);
 c.api.resetResults('Changed scope');assert.equal(c.ids['snap-para-dialog'].open,false);assert.equal(c.ids['snap-para-text'].textContent,'');
});
test('actual para modal never displays another COM_ID or an API denial payload',async()=>{
 const mismatch=client('risk',{data:{comId:99,html:'OTHER PARA'}});await mismatch.api.viewPara(42);assert.doesNotMatch(mismatch.ids['snap-para-text'].textContent+mismatch.ids['snap-para-text'].innerHTML,/OTHER PARA/);
 const denied=client('risk',{success:false,message:'Denied',data:{comId:42,html:'SECRET'}});await denied.api.viewPara(42);assert.doesNotMatch(denied.ids['snap-para-text'].textContent+denied.ids['snap-para-text'].innerHTML,/SECRET/);
});
test('actual report table removes settlement sources only for Snap, including the CSV DOM source',()=>{
 const source=read('AIS/wwwroot/js/executive-reports.js');
 const fn=source.slice(source.indexOf('    function table('),source.indexOf('    function bars('));
 for(const snap of [true,false]){
  const target=new Element(),context={snap,settlementSources:new Set(['settledByCompliance','settledByAudit','settledByUnknown']),
   node:(tag,value)=>{const e=new Element(tag);if(value!=null)e.textContent=value;return e;},
   total:()=>false,kind:()=> 'DETAIL',groupLabel:x=>x,target,
   columns:[{label:'Office',key:'itemName',format:String},{label:'Settled by Compliance',key:'settledByCompliance',format:String},{label:'Settled by Audit',key:'settledByAudit',format:String},{label:'Settled by Unknown',key:'settledByUnknown',format:String}],
   rows:[{itemName:'Office A',settledByCompliance:911,settledByAudit:922,settledByUnknown:933}]};
  vm.runInNewContext(fn+'\ntable(target,"Report",columns,rows);',context);
  if(snap){assert.doesNotMatch(target.textContent,/Settled by|911|922|933/);assert.match(target.textContent,/Office A/);}
  else {assert.match(target.textContent,/Settled by Compliance/);assert.match(target.textContent,/933/);}
 }
 assert.match(source,/report-content'\)\.querySelectorAll\('table'\)/);
 assert.match(source,/table.querySelectorAll\('thead th'\)/);
});

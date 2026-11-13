const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),vm=require('node:vm');
const read=p=>fs.readFileSync('AIS/'+p,'utf8');
class Node {constructor(tag,text){this.tag=tag;this.textContent=text??'';this.children=[];this.dataset={};this.style={};} append(...n){this.children.push(...n)} replaceChildren(...n){this.children=n} setAttribute(){} }
function harness(name,end,context){const s=read('wwwroot/js/executive-dashboard.js');return vm.runInNewContext(s.slice(s.indexOf('    function '+name+'('),s.indexOf(end,s.indexOf('    function '+name+'(')))+';'+name,context)}
const texts=n=>[n.textContent,...n.children.flatMap(texts)].join('|');
test('actual aging renderer includes unknown in total but excludes category and uses split two-year buckets',()=>{
 const target=new Node('div');let table;
 const fn=harness('bars','    function risk',{$:()=>target,el:(t,s)=>new Node(t,s),number:String,chart:()=>new Node('svg'),svg:()=>new Node('rect'),text(){},dataTable:(t,l,r)=>table=r});
 fn('aging',[['1_2_YEARS',2],['2_3_YEARS',3],['OVER_3_YEARS',4],['UNKNOWN',5]].map(([bucketCode,paraCount])=>({bucketCode,bucketName:bucketCode,paraCount,highRiskCount:0})));
 assert.match(texts(target),/Total Outstanding Paras: 14/);assert.match(texts(target),/1 Year old: 9/);assert.match(texts(target),/2 Year old: 7/);assert.equal(table.length,3);assert.match(texts(target),/5 paras have unknown/);
});
test('risk measures share the lifecycle closing cutoff and exact risk IDs',()=>{
 const sql=read('Docs/sql/PKG_EXEC_DASHBOARD.sql');for(const [name,id] of [['medium',2],['low',3]]){const end=sql.indexOf('END AS '+name+'_risk_open_paras');const block=sql.slice(sql.lastIndexOf('CASE WHEN V_TODAY_SNAPSHOT',end),end);assert.match(block,new RegExp('k.risk_id = '+id));assert.match(block,/k.para_settled_on >= V_TO_EXCLUSIVE/);assert.match(block,/k.current_open/);}
 assert.match(read('Models/ExecutiveDashboardModels.cs'),/NetParaForCompliance => OpeningOpenParas \+ ParaAdded/);
});
test('strict aging boundaries and unknown invalid dates retained in both Oracle sources',()=>{const sql=read('Docs/sql/PKG_EXEC_DASHBOARD.sql');assert.match(sql,/<= 730\s+THEN '1-2 YEARS'/);assert.match(sql,/snap_age <= 730 THEN '4' WHEN snap_age <= 1095 THEN '6'/);assert.match(sql,/snap_age IS NULL OR snap_age < 0 THEN '0'/);});
test('office mapping fails visibly on conflicting or missing evidence without multiplying pagination rows',()=>{const sql=read('Docs/sql/PKG_EXEC_DASHBOARD.sql');assert.match(sql,/COUNT\(DISTINCT h.reporting_1\)>1 THEN 'CONFLICTING'/);assert.match(sql,/COUNT\(h.reporting_1\)<>COUNT\(\*\) THEN 'MISSING'/);assert.match(sql,/ro.name_count > 1/);assert.match(sql,/FROM V_IAS_ENTITY_REPORTING_IDS h GROUP BY h.entity_id/);assert.match(sql,/p.rn > P_CCO_OFFSET AND p.rn <= P_CCO_OFFSET \+ P_MAX_ROWS/);assert.match(read('DBConnection.AuditParaSnap.cs'),/ValidateCcoOrganizationContext\(scope\)/);});
test('gist remains text, with explicit missing indicator and no observation-number substitution',()=>{const s=read('wwwroot/js/executive-dashboard.js');assert.match(s,/node.textContent = text/);assert.match(s,/gist.slice\(0, 180\)/);assert.match(s,/Para gist unavailable/);assert.doesNotMatch(s,/`Observation \$\{row.comId\}`/);});

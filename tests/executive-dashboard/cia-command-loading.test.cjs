'use strict';
// In-memory DOM/fetch fixtures exercise the actual browser client. No app data.
const test = require('node:test');
const assert = require('node:assert/strict');
const {readFileSync} = require('node:fs');
const {join} = require('node:path');
const {runInNewContext} = require('node:vm');
const source = readFileSync(join(__dirname,'../../AIS/wwwroot/js/cia-command-center.js'),'utf8');
const codes = ['AML_CFT','ACCOUNTS','LOAN_ADVANCES','CASH_MANAGEMENT','RECORD_MANAGEMENT',
    'BANKING_OPERATIONS','BRANCH_GENERAL_CONTROLS','ACCOUNTING_REPORTING_RECON','MANAGEMENT_AUDIT','IS_AUDIT'];
const visibleCodes = codes.filter(code => code !== 'ACCOUNTS' && code !== 'IS_AUDIT');
const kpis = (period=12,count=7) => ({auditPeriodId:period,plannedEntities:10,auditedEntities:5,planCoveragePct:50,
    delayedAuditReports:null,fraudA1Paras:count,awaitingAuditeeResponse:3,teamsConductingAudit:null,
    auditPeriodFrom:'2026-01-01',auditPeriodTo:'2026-12-31',currentStateAsOf:'2026-10-18'});
const nature = () => codes.map(code=>({categoryCode:code,categoryName:code,issueCount:1,totalIssues:10,
    unmappedIssues:0,mappingStatus:'COMPLETE'}));
const flush = () => new Promise(resolve=>setImmediate(resolve));
class Element {
    constructor() {
        this.textContent=''; this.dataset={}; this.attrs={}; this.events={};
        this.style={setProperty:(key,value)=>{this.style[key]=value;}};
    }
    setAttribute(key,value) { this.attrs[key]=value; }
    addEventListener(name,fn) { (this.events[name]??=[]).push(fn); }
    fire(name) { for(const fn of this.events[name]||[]) fn({preventDefault(){}}); }
}
function fixture({enabled=true,periodValue='12'}={}) {
    const fields=new Map();
    const field=key=> { if(!fields.has(key)) fields.set(key,new Element()); return fields.get(key); };
    const cards=visibleCodes.map(code=>{
        const card=new Element(); card.dataset.ciaCategory=code;
        card.querySelector=key=>field(code+key);return card;
    });
    const kpiFields=['planCoveragePct','delayedAuditReports','fraudA1Paras','awaitingAuditeeResponse','teamsConductingAudit'];
    const nodesFor=selector=>selector.split(',').flatMap(raw=>{
        const key=raw.trim();
        if(key==='[data-cia-kpi]') return kpiFields.map(k=>field('[data-cia-kpi="'+k+'"]'));
        if(key==='[data-cia-category]') return cards;
        if(key==='[data-cia-workflow-note]') return [field('workflow1'),field('workflow2')];
        if(['[data-cia-category-count]','[data-cia-category-fill]','[data-cia-category-share]'].includes(key))
            return visibleCodes.map(code=>field(code+key));
        return [field(key)];
    });
    const main=new Element();main.dataset.commandEnabled=String(enabled);
    main.querySelector=field;main.querySelectorAll=nodesFor;
    const form=new Element(), hidden={value:periodValue};
    form.querySelector=()=>hidden;
    form.requestSubmit=()=>form.fire('submit');
    const period=new Element();period.value=periodValue;period.selectedOptions=[{textContent:'2026'}];
    const status=new Element(), results=new Element(), selected=new Element(), reset=new Element();
    reset.href='https://ias.test/ZTBLAIS/ExecutiveOverview/CIA?auditPeriodId=12';
    const app=new Element();
    app.dataset={apiUrl:'/ZTBLAIS/api/ExecutiveDashboard/',dashboardType:'CIA'};
    app.querySelector=key=>key==='.cia-command-center' ? main : key==='[data-cia-reset]' ? reset : selected;
    const doc={getElementById:id=>({'executive-dashboard':app,'exec-filters':form,'cia-period':period,
        'exec-status':status,'exec-results':results}[id]||null)};
    class FormDataFixture { *[Symbol.iterator]() { yield ['view','command'];yield ['auditPeriodId',hidden.value];yield ['entityId','123']; } }
    const requests=[];
    const fetch=(url,options)=>new Promise(resolve=>requests.push({url,options,resolve}));
    runInNewContext(source,{document:doc,fetch,FormData:FormDataFixture,URLSearchParams,URL,AbortController,
        location:{href:'https://ias.test/ZTBLAIS/ExecutiveOverview/CIA?entityId=123'},
        history:{replaceState(){}},console});
    const reply=(index,data,statusCode=200,{invalidJson=false,contentType='application/json',redirected=false}={})=>{
        requests[index].resolve({status:statusCode,ok:statusCode>=200&&statusCode<300,redirected,
            headers:{get:name=>name==='content-type'?contentType:name==='X-CIA-Request-Id'?'test-trace':null},
            json:async()=>{if(invalidJson)throw new SyntaxError('invalid');return {success:statusCode===200,data,message:statusCode===200?undefined:'Test section failure'};}});
    };
    return {app,main,form,period,hidden,status,requests,field,cards,reply,selected,reset,
        changePeriod(value){period.value=value;period.selectedOptions=[{textContent:value}];period.fire('change');}};
}
test('authorized development client sends both correctly scoped requests with PathBase',()=>{
    const f=fixture();
    assert.equal(f.requests.length,2);
    for(const request of f.requests) {
        assert.match(request.url,/^\/ZTBLAIS\/api\/ExecutiveDashboard\/GetCia/);
        const q=new URL(request.url,'https://ias.test').searchParams;
        assert.equal(q.get('view'),'command');assert.equal(q.get('auditPeriodId'),'12');
        assert.equal(q.get('dashboardType'),'CIA');assert.equal(q.get('entityId'),'123');
        assert.equal(request.options.credentials,'same-origin');
    }
});
for(const code of [400,401,403,500,503]) test('nature HTTP '+code+' cannot erase successful KPI cards',async()=>{
    const f=fixture();f.reply(0,kpis());await flush();
    assert.equal(f.field('[data-cia-kpi="fraudA1Paras"]').textContent,'7');
    assert.equal(f.field('[data-cia-kpi="delayedAuditReports"]').textContent,'N/A');
    f.reply(1,null,code);await flush();
    assert.equal(f.field('[data-cia-kpi="fraudA1Paras"]').textContent,'7');
    assert.match(f.field('[data-cia-nature-status]').textContent,new RegExp('HTTP '+code));
    assert.match(f.field('[data-cia-nature-status]').textContent,/test-trace/);
    assert.equal(f.app.attrs['aria-busy'],'false');
});
test('successful nature data remains when the KPI request fails',async()=>{
    const f=fixture();f.reply(1,nature());await flush();f.reply(0,null,500);await flush();
    assert.equal(f.cards[0].querySelector('[data-cia-category-count]').textContent,'1');
    assert.equal(f.field('[data-cia-total]').textContent,'10');
    assert.equal(f.field('[data-cia-hidden]').textContent,'2');
    assert.match(f.field('[data-cia-kpi-status]').textContent,/HTTP 500/);
});
test('invalid JSON and HTML redirects are section-local failures',async()=>{
    const f=fixture();f.reply(0,kpis());f.reply(1,null,200,{invalidJson:true});await flush();
    assert.match(f.field('[data-cia-nature-status]').textContent,/Invalid JSON/);
    assert.equal(f.field('[data-cia-kpi="fraudA1Paras"]').textContent,'7');
    f.changePeriod('13');
    f.reply(2,kpis(13));f.reply(3,null,200,{redirected:true,contentType:'text/html'});await flush();
    assert.match(f.field('[data-cia-nature-status]').textContent,/session changed/);
    assert.equal(f.field('[data-cia-kpi="fraudA1Paras"]').textContent,'7');
});
test('period changes abort old requests, ignore late results and preserve the selected period in filters/reset',async()=>{
    const f=fixture();f.changePeriod('13');
    assert.equal(f.requests[0].options.signal.aborted,true);
    assert.equal(f.hidden.value,'13');assert.match(f.reset.href,/auditPeriodId=13/);
    f.reply(2,kpis(13,9));f.reply(3,nature());await flush();
    f.reply(0,kpis(12,88));f.reply(1,null,403);await flush();
    assert.equal(f.field('[data-cia-kpi="fraudA1Paras"]').textContent,'9');
    assert.equal(f.field('[data-cia-total]').textContent,'10');
    assert.equal(f.field('[data-cia-nature-status]').dataset.state,'success');
});
test('mismatched period and unreconciled categories never display as valid results',async()=>{
    const f=fixture();const rows=nature();rows[0].issueCount=9;
    f.reply(0,kpis(99));f.reply(1,rows);await flush();
    assert.equal(f.field('[data-cia-kpi="fraudA1Paras"]').textContent,'N/A');
    assert.equal(f.field('[data-cia-total]').textContent,'N/A');
    assert.match(f.field('[data-cia-kpi-status]').textContent,/does not match/);
    assert.match(f.field('[data-cia-nature-status]').textContent,/do not reconcile/);
});
test('empty results show real zero counts and an empty section state',async()=>{
    const f=fixture();const rows=nature().map(row=>({...row,issueCount:0,totalIssues:0,mappingStatus:'NO_ISSUES'}));
    f.reply(0,kpis());f.reply(1,rows);await flush();
    assert.equal(f.field('[data-cia-total]').textContent,'0');
    assert.equal(f.field('[data-cia-nature-status]').dataset.state,'empty');
});
test('disabled environments make no requests; a deliberately selected future period still loads',()=>{
    const disabled=fixture({enabled:false});assert.equal(disabled.requests.length,0);
    const future=fixture({periodValue:''});assert.equal(future.requests.length,0);
    future.changePeriod('2027');
    assert.equal(future.requests.length,2);assert.match(future.requests[0].url,/auditPeriodId=2027/);
});

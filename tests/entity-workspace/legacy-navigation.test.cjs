const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),vm=require('node:vm'),path=require('node:path');
const source=fs.readFileSync(path.join(__dirname,'../../AIS/wwwroot/js/administration-entity-dashboard-legacy.js'),'utf8');
function setup(stored=''){
 const requests=[],attrs={'data-step-key':'A','data-load-url':'/base/load','data-base-url':'/base/dashboard'};
 const host={innerHTML:'',getAttribute:k=>attrs[k],setAttribute:(k,v)=>attrs[k]=v,querySelectorAll:()=>[],addEventListener(){}};
 const stepper={querySelectorAll:()=>[]};let click;
 const window={location:{search:'?stepKey=A'},sessionStorage:{getItem:()=>stored,setItem(){}},history:{replaceState(){}},dispatchEvent(){},fieldAuditStepperTheme:{render(o){click=o.onStepClick;}},entityDashboardStepData:[{stepCode:'A',stepNo:1},{stepCode:'B',stepNo:2}]};
 vm.runInNewContext(source,{window,document:{title:'IAS',getElementById:id=>id==='entityDashboardStepHost'?host:id==='entityDashboardStepper'?stepper:null,querySelector:()=>null},URLSearchParams,Date,fetch(url,options){return new Promise((resolve,reject)=>requests.push({url,options,resolve,reject}));},CustomEvent:function(){}});
 return {host,attrs,requests,go(key){click({getAttribute:k=>k==='data-step-code'?key:'2'});}};
}
const flush=()=>new Promise(r=>setImmediate(r));
function respond(r,html,status=200){r.resolve({status,ok:status===200,text:()=>Promise.resolve(html)});}
test('revisiting a step displays fresh records, never restored database HTML',async()=>{
 const s=setup();respond(s.requests[0],'old records');await flush();s.go('B');respond(s.requests[1],'B records');await flush();s.go('A');respond(s.requests[2],'new records');await flush();assert.equal(s.host.innerHTML,'new records');assert.equal(s.requests[2].options.cache,'no-store');
});
test('slower response cannot overwrite a newer selected step',async()=>{
 const s=setup();s.go('B');respond(s.requests[1],'B latest');await flush();respond(s.requests[0],'A stale');await flush();assert.equal(s.host.innerHTML,'B latest');assert.equal(s.attrs['data-step-key'],'B');
});
test('stale network failure cannot replace current records',async()=>{
 const s=setup();s.go('B');respond(s.requests[1],'B latest');await flush();s.requests[0].reject(Error('late failure'));await flush();assert.equal(s.host.innerHTML,'B latest');
});
test('forbidden response replaces old operational records',async()=>{
 const s=setup();respond(s.requests[0],'sensitive records');await flush();s.go('A');respond(s.requests[1],'access denied',403);await flush();assert.equal(s.host.innerHTML,'access denied');
});
test('expired session cannot restore previous data',async()=>{
 const s=setup();respond(s.requests[0],'old');await flush();s.go('B');respond(s.requests[1],'login',401);await flush();assert.match(s.host.innerHTML,/Unable to load/);assert.doesNotMatch(s.host.innerHTML,/old/);
});
test('explicit step link overrides remembered navigation',()=>{
 const s=setup('B');assert.match(s.requests[0].url,/stepKey=A/);
});

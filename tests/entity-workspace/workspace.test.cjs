const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const base = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(base, 'AIS/wwwroot/js/administration-entity-dashboard.js'), 'utf8');
function setup() {
 const panels = Array.from({length:8}, (_,i)=>({hidden:i!==0}));
 const tabs = panels.map((_,i)=>({attrs:{'aria-controls':String(i)},events:{},setAttribute(k,v){this.attrs[k]=v;},getAttribute(k){return this.attrs[k];},addEventListener(k,f){this.events[k]=f;},focus(){this.focused=true;}}));
 const input={disabled:true,value:'old'};
 const toggle={checked:false,addEventListener(k,f){this.change=f;},getAttribute(){return 'proposal';}};
 const root={querySelectorAll(q){return q==='[data-entity-tab]'?tabs:[toggle];},addEventListener(){}};
 vm.runInNewContext(source,{document:{getElementById(id){return id==='entityWorkspace'?root:id==='proposal'?input:panels[Number(id)];}},window:{addEventListener(){}}});
 return {tabs,panels,toggle,input};
}
test('tab selection has one visible panel and one keyboard stop',()=>{
 const {tabs,panels}=setup(); tabs[4].events.click();
 assert.deepEqual(panels.map(p=>p.hidden),[true,true,true,true,false,true,true,true]);
 assert.equal(tabs.filter(t=>t.tabIndex===0).length,1);
 assert.equal(tabs[4].attrs['aria-selected'],'true');
});
test('keyboard navigation wraps and supports Home and End',()=>{
 const {tabs}=setup(); let prevented=false;
 tabs[0].events.keydown({key:'ArrowLeft',preventDefault(){prevented=true;}});
 assert.equal(prevented,true);assert.equal(tabs[7].focused,true);
 tabs[7].events.keydown({key:'Home',preventDefault(){}});assert.equal(tabs[0].tabIndex,0);
 tabs[0].events.keydown({key:'End',preventDefault(){}});assert.equal(tabs[7].tabIndex,0);
});
test('unchanged bulk fields cannot retain a stale proposal',()=>{
 const {toggle,input}=setup();toggle.checked=true;toggle.change();assert.equal(input.disabled,false);
 toggle.checked=false;toggle.change();assert.equal(input.disabled,true);assert.equal(input.value,'');
});
test('Oracle calls are gated read-only GET operations',()=>{
 assert.match(source,/readEnabled/);
 assert.match(source,/getData\('search'/);
 assert.match(source,/getData\('profile'/);
 assert.match(source,/getData\('closure'/);
 assert.match(source,/method:\s*'GET'/);
 assert.doesNotMatch(source,/method:\s*'(POST|PUT|PATCH|DELETE)'|\.ajax\(|eval\(/);
});

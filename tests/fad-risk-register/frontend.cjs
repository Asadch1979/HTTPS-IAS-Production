const fs = require('node:fs');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const source = fs.readFileSync('AIS/wwwroot/js/fad-risk-control-panel.js','utf8');
const fn = source.slice(source.indexOf('    function safeAttr('), source.indexOf('    function valueOrNull('));
const context = { $: () => ({ text(v) { this.v=String(v); return this; }, html() {
 return this.v.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); } }) };
vm.createContext(context); vm.runInContext(fn,context);
assert.equal(context.safeAttr('" onmouseover="alert(1)'), '&quot; onmouseover=&quot;alert(1)');
assert.equal(context.safeAttr("'><img src=x>"),'&#39;&gt;&lt;img src=x&gt;');
assert.equal(context.safeAttr(null),'');
assert.ok(source.includes("['KPI_SUB_CODE', 'KPI_CODE']"));
assert.ok(source.includes("['KPI_SUB_NAME', 'KPI_NAME']"));
console.log('PASS: attribute escaping and canonical/legacy Sub KPI contracts');

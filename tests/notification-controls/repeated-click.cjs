const fs = require('fs');
const vm = require('vm');
const assert = require('assert');
let calls = [], disabled = false;
const element = { ready() {}, hasClass() { return false; }, val() { return 'Reason'; }, prop(k,v) { disabled=v; return this; } };
function $(s) { return element; }
$.trim = s => s.trim(); $.each = (a,f) => a.forEach(f); $.ajax = o => calls.push(o);
const context = { $, document:{}, console, crypto:require('crypto').webcrypto, g_asiBaseURL:'', alert() {} };
vm.createContext(context);
vm.runInContext(fs.readFileSync('AIS/wwwroot/js/csp/Views_PostCompliance_post_compliance_review.js','utf8'),context);
context.validateComplianceRemarksLength = () => true;
context.g_reviewRequestId = '11111111-1111-4111-8111-111111111111';
context.PublishCompliance('D'); context.PublishCompliance('D');
assert.equal(calls.length,1); assert.equal(disabled,true);
calls[0].complete(); assert.equal(disabled,false);
context.PublishCompliance('D');
assert.equal(calls.length,2); assert.equal(calls[0].data.REQUEST_ID,calls[1].data.REQUEST_ID);
console.log('PASS: repeated clicks send once while pending; retry retains the server idempotency key');

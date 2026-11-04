const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const cp = require('node:child_process');
const read = p => fs.readFileSync(p, 'utf8');
const names = ['P_add_legacy_Para', 'P_Get_legacy_Para_to_authorize',
    'P_Authorize_legacy_Para_addition', 'P_referedback_Del_para'];
test('SQL changes retire only the four unsafe bodies; all other source is preserved', () => {
    const path = 'AIS/Docs/sql/PKG_HD.sql';
    const before = cp.execFileSync('git', ['show', 'HEAD:' + path], {encoding:'utf8', maxBuffer:20000000});
    const after = read(path);
    function normalize(s) {
        for (const name of names) {
            const start = s.toLowerCase().indexOf('  procedure ' + name.toLowerCase() + '(', s.toLowerCase().indexOf('create or replace package body'));
            const end = s.toLowerCase().indexOf('end ' + name.toLowerCase() + ';', start);
            assert.ok(start > 0 && end > start);
            const begin = start + s.slice(start, end).search(/\bbegin\b/i);
            if (s === after) assert.match(s.slice(begin,end), /^begin\s+raise_application_error\(-20870,[^;]+;\s+$/);
            s = s.slice(0,begin) + '<RETIRED BODY>' + s.slice(end);
        }
        return s.replace(/\r\n/g,'\n');
    }
    assert.equal(normalize(after), normalize(before));
});
test('retired application paths cannot execute the old Oracle procedures', () => {
    const db = read('AIS/DBConnection.HD.cs');
    for (const name of names) assert.ok(!db.toLowerCase().includes('pkg_hd.' + name.toLowerCase()));
    const api = read('AIS/Controllers/ApiCallsController.cs');
    for (const name of ['add_new_legacy_para','get_add_legacy_paras_autorize','Authorize_Legacy_Para_addition','Delete_Legacy_Para_addition_request'])
        assert.match(api, new RegExp('public IActionResult ' + name + '\\([^)]*\\)\\s*\\{\\s*return LegacyParaAdditionUnavailable\\(\\);\\s*\\}'));
    const guard = read('AIS/Controllers/ApiCallsController.LegacyParaAddition.cs');
    assert.match(guard,/StatusCode\(503/);
    assert.match(guard,/Status = false/);
});
test('menu restoration cannot activate the unsafe feature', () => {
    assert.match(read('AIS/Docs/sql/restore_legacy_para_execution_menu.sql'), /BEGIN\s+RAISE_APPLICATION_ERROR\(-20870/);
});

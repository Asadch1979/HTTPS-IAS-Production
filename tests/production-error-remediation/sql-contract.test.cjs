const fs = require('node:fs');
const cp = require('node:child_process');
const path = require('node:path');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../..');
const normalize = s => s.replace(/\r\n/g, '\n');
const read = p => normalize(fs.readFileSync(path.join(root, p), 'utf8'));
function body(text, name) {
    const start = text.toLowerCase().lastIndexOf('  procedure ' + name.toLowerCase() + '(');
    assert.ok(start >= 0, 'Missing body ' + name);
    const end = text.toLowerCase().indexOf('  end ' + name.toLowerCase() + ';', start);
    assert.ok(end > start, 'Missing end ' + name);
    return { start, end: end + name.length + 7, text: text.slice(start, end + name.length + 7) };
}
function maskBody(text, name) {
    const b = body(text, name);
    return text.slice(0, b.start) + '-- Reviewed procedure: ' + name + text.slice(b.end);
}
function maskDraftSpec(text) {
    const start = text.indexOf('  PROCEDURE P_Add_Observation_To_Draft(');
    const end = text.indexOf(');', start) + 2;
    assert.ok(start >= 0 && end > start);
    return text.slice(0, start) + '-- Reviewed draft signature' + text.slice(end);
}
for (const [pkg, names] of [
    ['AR', ['P_Add_Observation_To_Draft', 'P_Update_Memo_Draft_Para_No']],
    ['HD', ['P_audit_pre_Concluding', 'P_Finalise_para']]
]) {
    const file = 'AIS/Docs/sql/PKG_' + pkg + ' - prod.sql';
    let original = normalize(cp.execFileSync('git', ['show', '75493b215372712adc7bf505374c7b8d06c603e9:' + file], { cwd: root, encoding: 'utf8' }));
    let changed = read(file);
    const canonical = read('AIS/Docs/sql/PKG_' + pkg + '.sql');
    for (const name of names) {
        if (name !== 'P_Finalise_para') {
            assert.equal(body(changed, name).text.replace(/\s+/g, ''), body(canonical, name).text.replace(/\s+/g, ''), 'Corrected canonical/production procedure parity: ' + name);
        }
        original = maskBody(original, name); changed = maskBody(changed, name);
    }
    if (pkg === 'AR') { original = maskDraftSpec(original); changed = maskDraftSpec(changed); }
    const stripTerminator = s => s.trimEnd().replace(/\n\/\s*$/, '').trimEnd();
    assert.equal(stripTerminator(changed), stripTerminator(original), 'Unrelated production package content must remain unchanged: ' + pkg);
    console.log('PASS: production ' + pkg + ' preserves all unrelated procedures and corrected interfaces match');
}
for (const file of ['PKG_HD.sql', 'PKG_HD - prod.sql']) {
    const text = body(read('AIS/Docs/sql/' + file), 'P_audit_pre_Concluding').text;
    assert.equal((text.match(/\bCOMMIT\s*;/gi) || []).length, 1, 'Related writes have only one commit');
    assert.ok(text.indexOf('FOR UPDATE') < text.indexOf('MERGE INTO'));
    assert.ok(text.includes('SEQ_AU_OBSERVATION_GIST.NEXTVAL') && text.includes('SEQ_AU_OBS_FINAL_RECOMM.NEXTVAL'));
    assert.ok(!/MAX\(p.id\)/i.test(text));
    assert.ok(text.includes('ROLLBACK;') && text.includes('RAISE;'));
}
console.log('PASS: both pre-concluding procedures retain locking, atomic commit and error propagation');
const deployment = read('AIS/Docs/sql/deploy_production_error_remediation_packages.sql');
assert.ok(!/@@"PKG_IAS_SCHEDULER.sql"/.test(deployment));
assert.ok(deployment.includes('@@"deploy_ias_scheduler_package_only.sql"'));
assert.equal((deployment.match(/V_VALID <> 2 OR V_ERRORS <> 0/g) || []).length, 4);
for (const [, filename] of deployment.matchAll(/@@"([^"\n]+)"/g)) {
    const sql = read('AIS/Docs/sql/' + filename);
    assert.ok(!/^\s*(CREATE TABLE|DROP TABLE|TRUNCATE TABLE|CREATE SEQUENCE|DBMS_SCHEDULER\.)/im.test(sql), 'Package replacement must exclude schema/job DDL: ' + filename);
    assert.ok(/\/\s*$/.test(sql), 'SQL client must execute the final package body: ' + filename);
}
console.log('PASS: deployment replaces packages only and stops on compilation errors');

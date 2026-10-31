// Generate untracked, on-demand Oracle execution units outside AIS/Docs. Does not connect to a database.
const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../AIS/Docs/sql');
const outputRoot = path.resolve(__dirname, '../generated/f8');
const sources = ['PKG_HEAD_AUDIT_MONITOR','snap_audit_division_scope','PKG_EXEC_DASHBOARD',
    'PKG_EXEC_REPORTS','PKG_CCO_REPORTING','head_audit_dashboard_menu'];
for (const name of sources) {
    const folder=path.join(outputRoot,name);fs.mkdirSync(folder,{recursive:true});
    const source=fs.readFileSync(path.join(root,name+'.sql'),'utf8').replace(/\r\n/g,'\n');
    const units=source.split(/^\s*\/\s*$/m).map(unit=>unit
        .replace(/^[ \t]*(?:WHENEVER|SHOW ERRORS|SET |SPOOL ).*$/gm,'').trim())
        .filter(unit=>unit.replace(/--[^\n]*/g,'').trim());
    units.forEach((unit,i)=>{
        if (/^\s*(?:WHENEVER|SHOW ERRORS|SET |SPOOL |\/\s*$)/m.test(unit)) throw Error('Client directive remains');
        fs.writeFileSync(path.join(folder,String(i+1).padStart(2,'0')+'.sql'),
            '-- F8: execute this file alone in PL/SQL Developer SQL Window.\n'+unit+'\n');
    });
    console.log(name+': '+units.length+' execution units');
}

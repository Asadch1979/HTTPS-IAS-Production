(() => {
    'use strict';
    const root = document.getElementById('cco-organizations');
    if (!root) return;
    const target = document.getElementById('cco-org-results');
    const search = document.getElementById('cco-org-search');
    const expanded = new Set();
    let rows = [];
    const node = (tag, text) => { const n = document.createElement(tag); if (text != null) n.textContent = text; return n; };
    function render() {
        const table = node('table'); table.className = 'exec-table';
        const head = node('thead'), header = node('tr'), body = node('tbody');
        ['Organization Level','Reporting Office','Entity','Opening Cases','Para Added','Total Cases','Settled','Outstanding','Settlement %','Mapping Status'].forEach(label => { const th=node('th',label); th.scope='col'; header.append(th); });
        head.append(header);
        const byId = new Map(rows.map(r => [r.entityId,r]));
        const term=(search?.value || '').trim().toLocaleLowerCase();
        for (const row of rows) {
            const chain=[], seen=new Set([row.entityId]); let parent=byId.get(row.parentEntityId), cycle=false;
            while(parent) { if(seen.has(parent.entityId)) {cycle=true;break;} seen.add(parent.entityId);chain.push(parent);parent=byId.get(parent.parentEntityId); }
            if (term ? ![row.entityName,row.parentEntityName,row.entityCategory].some(v=>String(v||'').toLocaleLowerCase().includes(term)) : !cycle && chain.some(p=>!expanded.has(p.entityId))) continue;
            const tr=node('tr'), level=node('td');
            for(let i=0;i<chain.length;i++)level.append(node('span','　'));
            if(rows.some(r=>r.parentEntityId===row.entityId)) { const button=node('button',expanded.has(row.entityId)?'−':'+');button.type='button';button.setAttribute('aria-label','Expand or collapse '+row.entityName);button.setAttribute('aria-expanded',String(expanded.has(row.entityId)));button.addEventListener('click',()=>{expanded.has(row.entityId)?expanded.delete(row.entityId):expanded.add(row.entityId);render();});level.append(button); }
            level.append(node('span',row.organizationLevel||row.entityCategory||'Unmapped')); tr.append(level);
            const quality=cycle||row.hasTypeConflict?'Conflict':row.isOrphan?'Unmapped':row.mappingQuality||'Unverified';
            [row.parentEntityName||'Unmapped reporting office',row.entityName,...['openingCases','paraAdded','totalCases','parasSettled','outstanding','settlementRate'].map(k=>row[k]),quality].forEach(v=>tr.append(node('td',v==null?'N/A':String(v))));
            body.append(tr);
        }
        if(!body.children.length){const tr=node('tr'),cell=node('td',rows.length?'No matching organizations.':'No verified organizational figures available.');cell.colSpan=10;tr.append(cell);body.append(tr);}
        table.append(head,body);target.replaceChildren(table);
    }
    search?.addEventListener('input',render);
    document.getElementById('cco-expand-all')?.addEventListener('click',()=>{rows.forEach(r=>expanded.add(r.entityId));render();});
    document.getElementById('cco-collapse-all')?.addEventListener('click',()=>{expanded.clear();render();});
    // Only a separately verified adapter may supply data. No directory requests.
    root.addEventListener('cco:authorized-rows',event=>{
        if(root.dataset.enabled!=='true')return;
        const data=event.detail;
        if(!Array.isArray(data)||data.length>500||new Set(data.map(r=>r.entityId)).size!==data.length)throw Error('Invalid organizational response');
        rows=data;expanded.clear();render();
    });
    ['exec-start-date','exec-end-date'].forEach(id=>document.getElementById(id)?.addEventListener('change',()=>{rows=[];expanded.clear();render();}));
    // No organizational requests while the trusted reporting adapter is disabled.
    if (root.dataset.enabled === 'true') render();
})();

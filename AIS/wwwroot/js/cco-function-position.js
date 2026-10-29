(() => {
    'use strict';
    const root=document.getElementById('cco-function-position');if(!root)return;
    const target=document.getElementById('cco-function-results'),status=document.getElementById('cco-function-status');
    const columns=[['Group','groupName'],['Division','divisionName'],['Opening balance','openingBalance'],['Para added','parasAdded'],['Total','totalParas'],['Settled','settledParas'],['Para outstanding','outstandingParas'],['Settlement %','settlementPercentage']];
    const element=(tag,value)=>{const e=document.createElement(tag);if(value!=null)e.textContent=String(value);return e;};
    function clear(){target.replaceChildren();target.hidden=true;}
    function render(rows){
        clear();if(!Array.isArray(rows)||rows.length>1000)throw Error('Invalid function position response.');
        if(!rows.length){status.textContent='No observations in the selected period.';return;}
        const table=element('table');table.className='exec-table';const head=element('thead'),header=element('tr'),body=element('tbody');
        columns.forEach(([title])=>{const th=element('th',title);th.scope='col';header.append(th);});head.append(header);
        const rank=r=>r.rowKind==='TOTAL'?2:r.rowKind==='SUBTOTAL'?1:0;
        [...rows].sort((a,b)=>(a.rowKind==='TOTAL')-(b.rowKind==='TOTAL')||String(a.groupName||'').localeCompare(String(b.groupName||''))||rank(a)-rank(b)||String(a.divisionName||'').localeCompare(String(b.divisionName||''))).forEach(row=>{
            const tr=element('tr');columns.forEach(([,key])=>tr.append(element('td',row[key]==null?'N/A':row[key])));body.append(tr);
        });table.append(head,body);target.append(table);target.hidden=false;status.textContent='Selected period · Management Audit (D) · historical estimates. Unresolved mappings remain included.';
    }
    root.addEventListener('cco:function-rows',event=>{if(root.dataset.enabled!=='true')return;try{render(event.detail);}catch(error){clear();status.textContent=error.message;}});
    ['exec-start-date','exec-end-date'].forEach(id=>document.getElementById(id)?.addEventListener('change',()=>{clear();status.textContent='Dates changed. Authorized data must be reloaded.';}));
})();

(() => {
    'use strict';
    const root = document.getElementById('cco-organizations');
    if (!root || root.dataset.enabled !== 'true') return; // No live activation in this review.
    const $ = id => document.getElementById(`cco-${id}`);
    const form = $('org-filters'), target = $('org-results'), status = $('org-status');
    let paths = [], request, version = 0;
    const ids = ['group','division','gm','entity'];
    const fields = {group:'groupId', division:'divisionId', gm:'gmOfficeId', entity:'entityId'};
    const invalidate = () => {request?.abort(); ++version; target.replaceChildren(); target.hidden=true;};
    const element = (tag, value) => {const e=document.createElement(tag); if(value!=null)e.textContent=value;return e;};
    function query() {
        const params=new URLSearchParams();
        params.set('startDate',document.getElementById('exec-start-date').value);
        params.set('endDate',document.getElementById('exec-end-date').value);
        params.set('groupBy',$('group-by').value);params.set('maxRows','500');
        ids.forEach(id=>{if($(id).value)params.set(fields[id],$(id).value);});
        return params;
    }
    function options() {
        const keys={group:'groupId',division:'divisionId',gm:'gmOfficeId',entity:'entityId'};
        const names={group:'groupName',division:'divisionName',gm:'gmOfficeName',entity:'entityName'};
        ids.forEach((id,index)=>{
            const selected=$(id).value, choices=new Map();
            paths.filter(p=>p.isVerified && ids.slice(0,index).every(parent=>!$(parent).value || String(p[keys[parent]])===$(parent).value))
                .forEach(p=>{if(p[keys[id]]!=null)choices.set(String(p[keys[id]]),p[names[id]]||String(p[keys[id]]));});
            $(id).replaceChildren(new Option('All authorized '+id+' options',''),...[...choices].map(([key,label])=>new Option(label,key)));
            if(choices.has(selected))$(id).value=selected;
            $(id).disabled=(id==='division'&&!$('group').value)||(id==='gm'&&(!$('group').value||!$('division').value));
        });
        $('org-breadcrumb').textContent=['All authorized Groups',...ids.filter(id=>$(id).value).map(id=>$(id).selectedOptions[0].textContent)].join(' › ');
    }
    async function get(action,params,signal) {
        const response=await fetch(`${root.dataset.apiUrl}${action}?${params}`,{signal,credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}});
        if(response.redirected)throw Error('The IAS session must be renewed.');
        const result=await response.json();
        if(!response.ok || result.success!==true)throw Error(result.message||'Organizational data unavailable.');
        return result.data;
    }
    function render(rows) {
        if(!Array.isArray(rows)||rows.length>500)throw Error('Invalid or unbounded organizational response.');
        const table=element('table');table.className='exec-table';
        const columns=[['Period','periodStart'],['Row','rowKind'],['Group','groupName'],['Division','divisionName'],['GM Office','gmOfficeName'],
            ['Opening Cases','openingCases'],['Para Added','paraAdded'],['Total Cases','totalCases'],['Paras Settled','parasSettled'],['Closing Outstanding','outstanding'],
            ['Settlement %','settlementRate'],['Net Backlog Change','netBacklogChange'],['High Risk','highRiskOpen'],['Zero Cycles (current)','zeroCycleOpen'],
            ['Over 3 Cycles (current)','over3CycleOpen'],['Over 365 Days','agedOver365Open'],['Reconciliation Difference','reconciliationDifference'],['Mapping Quality','mappingQuality'],['Historical Quality','historicalQuality']];
        table.append(element('caption','Authorized auditee organization · selected dates · historical estimates'));
        const head=element('thead'),tr=element('tr'),body=element('tbody');
        columns.forEach(([label])=>{const th=element('th',label);th.scope='col';tr.append(th);});head.append(tr);
        rows.forEach(row=>{const r=element('tr');columns.forEach(([,key])=>r.append(element('td',row[key]==null?'N/A':String(row[key]))));body.append(r);});
        if(!rows.length){const r=element('tr'),cell=element('td','No data in selected period.');cell.colSpan=columns.length;r.append(cell);body.append(r);}
        table.append(head,body);target.replaceChildren(table);target.hidden=false;
    }
    async function load(selectors=false) {
        invalidate();const current=version;request=new AbortController();status.textContent='Loading authorized organization…';
        try {
            if(selectors){paths=await get('GetOrgFilters',query(),request.signal);if(current!==version)return;
                if(!Array.isArray(paths)||paths.length>1000)throw Error('Invalid organizational selector response.');options();}
            const rows=await get(root.dataset.action,query(),request.signal);if(current!==version)return;
            render(rows);status.textContent='Selected date range applied. Historical estimates and current compliance cycles remain qualified.';
        } catch(error) {if(current!==version||error.name==='AbortError')return;target.replaceChildren();target.hidden=true;status.textContent=error.message;}
    }
    ids.forEach((id,index)=>$(id).addEventListener('change',()=>{invalidate();ids.slice(index+1).forEach(child=>$(child).value='');options();status.textContent='Organization changed. Choose Apply organization.';}));
    $('group-by').addEventListener('change',()=>{invalidate();status.textContent='Aggregation changed. Choose Apply organization.';});
    form.addEventListener('submit',event=>{event.preventDefault();load();});
    ['exec-start-date','exec-end-date'].forEach(id=>document.getElementById(id).addEventListener('change',()=>{invalidate();paths=[];ids.forEach(id=>$(id).value='');options();status.textContent='Dates changed. Apply dates to reload the verified organization.';}));
    form.querySelector('fieldset').disabled=false;
    load(true);
})();

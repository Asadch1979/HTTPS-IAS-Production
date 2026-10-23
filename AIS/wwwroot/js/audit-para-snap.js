(() => {
    'use strict';
    const app = document.getElementById('executive-dashboard');
    if (!app?.classList.contains('exec-snap')) return;
    const form = document.getElementById('exec-filters');
    const start = document.getElementById('exec-start-date');
    const end = document.getElementById('exec-end-date');
    const status = document.getElementById('exec-status');
    const reports = app.dataset.view === 'reports';
    const number = value => value == null ? 'N/A' : Number(value).toLocaleString('en-GB');
    const element = (tag, value, cls) => {
        const node = document.createElement(tag);
        if (value != null) node.textContent = value;
        if (cls) node.className = cls;
        return node;
    };
    function validDates() {
        end.setCustomValidity('');
        if (!start.value || !end.value) return false;
        if (start.value > end.value) end.setCustomValidity('End Date must be on or after Start Date.');
        else if (end.value > end.max) end.setCustomValidity('End Date cannot be in the future.');
        else {
            const a = start.value.split('-').map(Number), b = end.value.split('-').map(Number);
            if ((b[0] - a[0]) * 12 + b[1] - a[1] >= 1200)
                end.setCustomValidity('Select fewer than 1200 calendar months.');
        }
        return form.checkValidity();
    }
    function navigation() {
        for (const anchor of app.querySelectorAll('[data-view-navigation] a')) {
            const url = new URL(anchor.href);
            for (const [key,value] of new FormData(form)) {
                if (key !== 'view' && key !== 'report') {
                    if (value) url.searchParams.set(key,value); else url.searchParams.delete(key);
                }
            }
            anchor.href = url;
        }
    }
    let request = null;
    function resetResults(note) {
        performance = null;
        agingPosition=null; agingDistribution=null;
        paraRequest?.abort();
        document.getElementById('snap-para-dialog')?.close();
        document.getElementById('snap-para-text')?.replaceChildren();
        for (const card of app.querySelectorAll('[data-snap-card]')) {
            card.querySelector('[data-snap-kpi]').textContent = 'N/A';
            card.querySelector('[data-snap-kpi-note]').textContent = note;
        }
        for (const name of ['risk','aging','cycle','trend']) {
            const target = document.getElementById('exec-' + name);
            if (target) target.replaceChildren(element('p',note,'exec-empty'));
        }
        for (const id of ['snap-clearance','snap-movement','snap-rankings','snap-exposure','snap-critical-departments']) document.getElementById(id)?.replaceChildren(element('p',note,'exec-empty'));
        document.getElementById('snap-performance')?.replaceChildren(element('p',note,'exec-empty'));
        document.getElementById('exec-critical')?.replaceChildren();
        const criticalNote = document.getElementById('exec-critical-note');
        if (criticalNote) criticalNote.textContent = note;
    }
    form.addEventListener('submit', event => {
        if (!validDates()) { event.preventDefault(); form.reportValidity(); }
    });
    // The approved report client handles its own invalidation and cascades.
    // This handler only shares Snap's extra date rules and updates tab dates.
    form.addEventListener('change', () => {
        validDates(); navigation();
        const period = app.querySelector('[data-snap-period]');
        period.textContent = 'Requested period: ' + start.value + ' – ' + end.value + ' · Apply to refresh results.';
        if (!reports) {
            request?.abort();
            app.setAttribute('aria-busy','false');
            resetResults('Filters changed. Apply to load this scope and period.');
            status.textContent = 'Filters changed. Choose Apply.';
        }
    });
    // Changing Division discards an incompatible Department / Cluster and
    // refreshes the server-authorized child list with "All" selected.
    const division = document.getElementById('snap-division');
    const department = document.getElementById('snap-department');
    division?.addEventListener('change', () => {
        if (department) { department.value = ''; department.disabled = division.value === '1'; }
        navigation();
        if (validDates()) form.requestSubmit();
    });
    if (!reports) for (const [id,children] of [
        ['exec-select-by',['exec-dimension','exec-office','exec-entity']],
        ['exec-dimension',['exec-office','exec-entity']], ['exec-office',['exec-entity']]
    ]) document.getElementById(id)?.addEventListener('change', () => {
        for (const child of children) document.getElementById(child)?.replaceChildren(new Option('All matching options',''));
        navigation(); if (validDates()) form.requestSubmit();
    });
    navigation();
    if (reports) return;
    async function read(action, signal, extra = {}) {
        const query = new URLSearchParams(new FormData(form));
        for (const [key,value] of Object.entries(extra)) query.set(key,value);
        const response = await fetch(app.dataset.apiUrl + action + '?' + query, {
            signal, credentials:'same-origin', cache:'no-store',
            headers:{Accept:'application/json','X-Requested-With':'XMLHttpRequest'}
        });
        if (response.redirected || !(response.headers.get('content-type') || '').includes('application/json'))
            throw new Error('IAS session or reporting access changed.');
        const body = await response.json();
        if (!response.ok || body.success !== true || body.dateRangeApplied !== true ||
            body.startDate !== start.value || body.endDate !== end.value)
            throw new Error(body.message || 'A consistent dated result was not returned.');
        return body.data;
    }
    let agingPosition=null, agingDistribution=null;
    function renderKpis(data) {
        if(app.dataset.view==='aging') {
            agingPosition=data;
            if(agingDistribution)renderDistribution(agingDistribution);
        }
        const clearance=document.getElementById('snap-clearance');
        if(clearance) {
            const comparable=data.paraAdded>0 && data.parasSettled!=null && data.openingOpenParas!=null && data.closingOpenParas!=null &&
                data.closingOpenParas===data.openingOpenParas+data.paraAdded-data.parasSettled;
            clearance.textContent=comparable ? (data.parasSettled/data.paraAdded*100).toFixed(2)+'% · estimated' : 'N/A · unavailable or incomparable denominator';
        }
        for (const card of app.querySelectorAll('[data-snap-card]')) {
            const key = card.dataset.snapCard;
            const opening = key === 'openingOpenParas';
            const periodRate = key === 'periodSettlementRate';
            const position = opening || periodRate ||
                ['closingOpenParas','highRiskOpenParas','over1YearParas','over3ComplianceCycles'].includes(key);
            const verified = opening ? data.historicalOpeningVerified === true : data.closingAsofVerified === true;
            const estimated = opening ? data.openingEstimated === true : data.closingEstimated === true;
            const available = !position || verified || estimated;
            let value = available ? data[key] : null;
            if (periodRate) {
                // The date-range rate is distinct from MTD and lifetime ratio.
                // Do not invent a rate when the reconstructed opening is
                // unavailable or the denominator is inconsistent with events.
                const baseline = data.openingOpenParas;
                const additions = data.paraAdded;
                const settled = data.parasSettled;
                const denominator = baseline == null || additions == null ? null : baseline + additions;
                value = denominator > 0 && Number.isSafeInteger(settled) &&
                    settled >= 0 && settled <= denominator
                    ? Math.round(settled / denominator * 10000) / 100 : null;
            }
            if (value != null && (!Number.isFinite(value) || value < 0 ||
                (!periodRate && !Number.isSafeInteger(value)))) throw new Error('Invalid lifecycle count.');
            card.querySelector('[data-snap-kpi]').textContent =
                value == null ? 'N/A' : number(value) + (periodRate ? '%' : '');
            card.title = periodRate
                ? 'Selected-period settlements ÷ (estimated opening open paras + paras added in selected period). N/A if unavailable or inconsistent.'
                : position && estimated
                ? 'As-of value reconstructed from the latest available IAS records; historical status changes may not be complete.'
                : position && verified ? 'Current IAS position.' : 'IAS event dates within the selected range.';
            card.querySelector('[data-snap-kpi-note]').textContent =
                value == null ? 'N/A · No reliable history' :
                position ? 'As of selected date' : 'Within selected dates';
        }
    }

    // Selected-period IAS monthly settlement trend. The Oracle cursor returns
    // BOTH settled and high-risk-settled observations for the SAME month.
    function renderMonthlyTrend(rows) {
        const panel = element('div',null,'snap-monthly-trend');
        const legend = element('p','━━ All settled (blue) · ┄┄ High-risk settled (red)','exec-legend');
        const svg = document.createElementNS('http://www.w3.org/2000/svg','svg');
        const ns = 'http://www.w3.org/2000/svg';
        svg.setAttribute('viewBox','0 0 640 270');
        svg.setAttribute('role','img');
        svg.setAttribute('aria-label','Monthly settled paras and high-risk settled paras over the selected IAS period');
        svg.setAttribute('class','snap-trend-chart');
        const title = document.createElementNS(ns,'title');
        title.textContent = 'Monthly settlement trend'; svg.append(title);
        const maxCount = Math.max(1,...rows.map(row => Math.max(row.paraCount ?? 0, row.highRiskCount ?? 0)));
        const ceil = Math.ceil(maxCount / 3 / Math.pow(10,Math.floor(Math.log10(maxCount / 3)))) *
            Math.pow(10,Math.floor(Math.log10(maxCount / 3)));
        const maximum = Math.max(3,ceil*3);
        const x = i => 55 + i * 560 / Math.max(1,rows.length-1);
        const y = value => 222 - value / maximum * 178;
        const node = (tag,attrs) => {
            const el = document.createElementNS(ns,tag);
            Object.entries(attrs).forEach(([k,v]) => el.setAttribute(k,String(v)));
            return el;
        };
        for (let k=0;k<=3;k++) {
            const val = maximum*k/3, top=y(val);
            svg.append(node('line',{x1:54,x2:618,y1:top,y2:top,stroke:'#dee7e5','stroke-dasharray':'3 5'}));
            const lab = node('text',{x:46,y:top+4,'text-anchor':'end','font-size':11,fill:'#496574'});
            lab.textContent = number(val); svg.append(lab);
        }
        for (const [key,color,dash] of [['paraCount','#265cc8',null],['highRiskCount','#b32a24','7 5']]) {
            const vals = rows.map(row => row[key]);
            if (vals.some(value => !Number.isSafeInteger(value) || value < 0))
                throw new Error('Invalid monthly settlement count from IAS.');
            const polyline = node('polyline',{
                points:vals.map((value,i) => x(i)+','+y(value)).join(' '),
                fill:'none',stroke:color,'stroke-width':3
            });
            if (dash) polyline.setAttribute('stroke-dasharray',dash);
            svg.append(polyline);
            vals.forEach((value,i) => {
                const point=node('circle',{cx:x(i),cy:y(value),r:3.3,fill:color});
                const desc=node('title',{});desc.textContent =
                    rows[i].bucketName + ': ' + (key==='paraCount'?'Settled ':'High risk settled ') + number(value);
                point.append(desc);svg.append(point);
            });
        }
        const labelStep = Math.max(1,Math.ceil(rows.length/7));
        rows.forEach((row,i) => {
            if (i % labelStep!==0 && i!==rows.length-1) return;
            const lab=node('text',{x:x(i),y:251,'text-anchor':'middle','font-size':11,fill:'#496574'});
            lab.textContent=row.bucketName;svg.append(lab);
        });
        const table = element('table',null,'exec-table snap-bucket-table');
        const head = element('thead'), header=element('tr'),body=element('tbody');
        for (const label of ['Month','Settled','High-risk settled']) {
            const th=element('th',label);th.scope='col';header.append(th);
        }
        head.append(header);
        rows.forEach(row => {
            const tr=element('tr'), th=element('th',row.bucketName);th.scope='row';
            tr.title=row.qualityCode==='E'?'Derived from latest qualifying IAS settlement events':'IAS settlement events';
            tr.append(th,element('td',number(row.paraCount)),element('td',number(row.highRiskCount)));
            body.append(tr);
        });
        table.append(head,body);
        const details=element('details',null,'exec-data');
        details.append(element('summary','View chart data'),table);
        panel.append(legend,svg,details);
        return panel;
    }

    function renderMovement(rows) {
        const target=document.getElementById('snap-movement');if(!target)return;
        const subset=rows.filter(r=>r.dataset==='movement');
        if(!subset.length)throw new Error('Monthly additions/settlements unavailable.');
        const adapted=subset.map(r=>({...r,paraCount:r.parasAdded,highRiskCount:r.parasSettled}));
        const panel=renderMonthlyTrend(adapted);
        panel.querySelector('.exec-legend').textContent='━━ Paras Added (blue) · ┄┄ Paras Settled (red)';
        const svg=panel.querySelector('svg');svg.setAttribute('aria-label','Monthly additions versus settlements');
        svg.querySelector('title').textContent='Monthly additions versus settlements';
        for(const node of svg.querySelectorAll('circle title'))node.textContent=node.textContent.replace('High risk settled ','Paras settled ').replace(': Settled ',': Paras added ');
        const headings=panel.querySelectorAll('th');headings[1].textContent='Paras Added';headings[2].textContent='Paras Settled';
        target.replaceChildren(panel);
    }
    function renderRankings(data) {
        const target=document.getElementById('snap-rankings');
        const rows=data.rows.filter(r=>r.rowKind==='department' && r.settlementPercentage!=null && r.arithmeticDifference===0);
        if(!rows.length){target.replaceChildren(element('p','N/A · No departments have a comparable denominator.','exec-empty'));return;}
        const table=element('table',null,'exec-table'),thead=element('thead'),head=element('tr'),body=element('tbody');
        for(const label of ['Ranking','Division / Department','Settlement %','Paras Added','Paras Settled','Closing Outstanding'])head.append(element('th',label));
        thead.append(head);
        for(const [label,items] of [['Top',rows.slice().sort((a,b)=>b.settlementPercentage-a.settlementPercentage).slice(0,5)],['Lowest',rows.slice().sort((a,b)=>a.settlementPercentage-b.settlementPercentage).slice(0,5)]])
            for(const row of items){const tr=element('tr');for(const text of [label,row.division+' / '+row.department,row.settlementPercentage.toFixed(2)+'% · estimated',number(row.position.paraAdded),number(row.position.parasSettled),number(row.position.closingOpenParas)])tr.append(element('td',text));body.append(tr);}
        table.append(thead,body);target.replaceChildren(table);
    }
    function renderDistribution(rows) {
        if(app.dataset.view==='aging')agingDistribution=rows;
        renderMovement(rows);
        const exposure=document.getElementById('snap-exposure');
        if(exposure) {
            const entries=rows.filter(r=>r.dataset==='exposure');
            if(entries.length!==3)throw new Error('Missing critical exposure analysis.');
            exposure.replaceChildren(...entries.map(r=>element('p',r.bucketName+': '+(r.qualityCode==='N'?'N/A · insufficient history':number(r.paraCount)))));
        }
        if (!Array.isArray(rows)) throw new Error('Invalid distribution cursor.');
        for (const name of ['risk','aging','cycle','trend']) {
            const target = document.getElementById('exec-' + name);
            if (!target) continue;
            // Unrated is retained in Oracle for data-quality diagnostics,
            // but the visible distribution is explicitly for rated risks.
            const subset = rows.filter(row => row.dataset === name &&
                !(name === 'risk' && (row.bucketCode === '0' || row.bucketName === 'Unrated')));
            if (!subset.length) throw new Error('Missing ' + name + ' analysis.');
            if (name === 'trend') {
                target.replaceChildren(renderMonthlyTrend(subset));
                continue;
            }
            const table = element('table',null,'exec-table snap-bucket-table');
            const head = element('thead'), header = element('tr'), body = element('tbody');
            const aging=app.dataset.view==='aging';
            const closing=agingPosition?.closingOpenParas;
            const comparable=aging && (agingPosition?.closingAsofVerified || agingPosition?.closingEstimated) && closing>0 &&
                subset.every(row=>row.qualityCode!=='N' && Number.isSafeInteger(row.paraCount)) &&
                subset.reduce((sum,row)=>sum+row.paraCount,0)===closing;
            header.append(element('th','Category'),element('th','Paras'));
            if(aging)header.append(element('th','% of closing outstanding · estimated'));
            for (const th of header.children) th.scope = 'col';
            head.append(header);
            for (const row of subset) {
                if (!['Y','E','N'].includes(row.qualityCode) || row.paraCount != null &&
                    (!Number.isSafeInteger(row.paraCount) || row.paraCount < 0)) throw new Error('Invalid dated analysis.');
                const tr = element('tr'), label = element('th',row.bucketName); label.scope = 'row';
                tr.title = row.qualityCode === 'N' ? 'Historical source evidence unavailable'
                    : row.qualityCode === 'E' ? 'Calculated from available IAS history' : 'Current IAS status';
                tr.append(label,element('td',row.qualityCode === 'N' ? 'N/A' : number(row.paraCount)));
                if(aging)tr.append(element('td',comparable?(row.paraCount/closing*100).toFixed(2)+'%':'N/A'));
                body.append(tr);
            }
            table.append(head,body);
            const bars = element('div',null,'exec-bars');
            const maximum = Math.max(1,...subset.map(row => Number(row.paraCount) || 0));
            for (const row of subset) {
                const line = element('div',null,'exec-bar-row'), track = element('div',null,'exec-track');
                const fill = element('span',null,'exec-fill');
                if (name === 'risk') fill.classList.add('snap-risk-' + String(row.bucketCode));
                fill.style.width = row.qualityCode === 'N' ? '0%' : Math.max(0,Number(row.paraCount) / maximum * 100) + '%';
                track.append(fill);
                line.append(element('span',row.bucketName),track,element('strong',row.qualityCode === 'N' ? 'N/A' : number(row.paraCount)));
                bars.append(line);
            }
            const details = element('details',null,'snap-chart-details');
            const caption = element('summary','View chart data');
            details.append(caption,table);
            target.replaceChildren(bars,details);
            if (name === 'cycle' && subset.every(row => row.qualityCode === 'N'))
                target.append(element('p','Historical cycle breakdown unavailable.','exec-empty'));
        }
    }
    let performance = null;
    function renderPerformance(data) {
        if (!data || !Array.isArray(data.rows) || data.rows.length > 205) throw new Error('Invalid department comparison.');
        performance = data;
        const target = document.getElementById('snap-performance');
        const table = element('table',null,'exec-table'), head = element('thead'), header=element('tr'),body=element('tbody');
        for (const label of ['Audit Division','Audit Department / Cluster','Opening Balance','Paras Added','Paras Settled','Closing Balance','Settlement %','Net Change','Reconciliation Difference']) {
            const th=element('th',label);th.scope='col';header.append(th);
        }
        head.append(header);
        const mode=document.getElementById('snap-performance-sort').value;
        const value=row => mode==='rate' ? row.settlementPercentage : mode==='change' ? row.netChange : row.position.closingOpenParas;
        const rows=[];
        for (const division of ['FAD','MAD']) {
            rows.push(...data.rows.filter(r=>r.rowKind==='department' && r.division===division)
                .sort((a,b)=>(value(b) ?? -Infinity)-(value(a) ?? -Infinity) || a.department.localeCompare(b.department)));
            rows.push(...data.rows.filter(r=>r.rowKind==='subtotal' && r.division===division));
        }
        rows.push(...data.rows.filter(r=>r.rowKind==='total'));
        for (const row of rows) {
            const k=row.position;
            if (!k) throw new Error('Missing lifecycle comparison.');
            const tr=element('tr');
            if (row.rowKind!=='department') tr.className='exec-total';
            const fields=[row.division==='1'?'All Divisions':row.division,row.department,
                number(k.openingOpenParas),number(k.paraAdded),number(k.parasSettled),number(k.closingOpenParas),
                row.settlementPercentage==null?'N/A':Number(row.settlementPercentage).toFixed(2)+'%',
                number(row.netChange),number(row.arithmeticDifference)];
            for (const text of fields) tr.append(element('td',text));
            body.append(tr);
        }
        table.append(head,body); target.replaceChildren(table);
        const notes=[data.limitations];
        for (const row of data.rows.filter(r=>r.rowKind==='subtotal')) {
            const departments=data.rows.filter(r=>r.rowKind==='department' && r.division===row.division);
            for (const key of ['openingOpenParas','paraAdded','parasSettled','closingOpenParas']) {
                if (row.position[key]!=null && departments.every(d=>d.position[key]!=null)) {
                    const difference=row.position[key]-departments.reduce((sum,d)=>sum+d.position[key],0);
                    if (difference!==0) notes.push(row.division+' '+key+': portfolio total minus listed department sum = '+number(difference)+'.');
                }
            }
        }
        document.getElementById('snap-performance-note').textContent=notes.join(' ');
    }
    document.getElementById('snap-performance-sort')?.addEventListener('change',()=>{if(performance)renderPerformance(performance);});
    function renderCriticalDepartments(data) {
        const table=element('table',null,'exec-table'),head=element('thead'),tr=element('tr'),body=element('tbody');
        for(const label of ['Audit Division','Department / Cluster','High-risk Open'])tr.append(element('th',label));head.append(tr);
        for(const row of data.rows){const tr=element('tr');for(const value of [row.division,row.department,number(row.position.highRiskOpenParas)])tr.append(element('td',value));body.append(tr);}
        table.append(head,body);document.getElementById('snap-critical-departments').replaceChildren(table);
    }
    let paraRequest=null;
    const dialog=document.getElementById('snap-para-dialog');
    dialog?.addEventListener('close',()=>{paraRequest?.abort();document.getElementById('snap-para-text').replaceChildren();});
    document.getElementById('snap-para-close')?.addEventListener('click',()=>dialog.close());
    async function viewPara(comId) {
        paraRequest?.abort();paraRequest=new AbortController();const active=paraRequest;
        const text=document.getElementById('snap-para-text');text.textContent='Loading authorized para text…';dialog.showModal();
        try {
            const data=await read('GetParaText',active.signal,{comId});
            if(!active.signal.aborted){if(data.comId!==comId || typeof data.text!=='string')throw new Error('Invalid para text response.');text.textContent=data.text || 'No para text is recorded.';}
        } catch(error){if(!active.signal.aborted)text.textContent='Para text unavailable: '+error.message;}
    }
    let criticalRows = [];
    function renderCritical() {
        const target = document.getElementById('exec-critical');
        const selectedRisk = document.getElementById('exec-critical-risk').value;
        target.replaceChildren();
        for (const row of criticalRows.filter(row => !selectedRisk || row.riskName === selectedRisk)) {
            const tr = element('tr'), entity = element('td');
            entity.textContent = row.entityName || 'Unknown entity';
            tr.append(entity,element('td',(row.auditPeriod || 'Unknown audit period')+' / '+(row.paraNo || 'Unnumbered')),element('td',row.riskName),
                element('td',number(row.comCycle)),element('td',number(row.agingDays)),
                element('td',number(row.daysSinceLastMovement)),element('td',row.gistOfParas || ''));
            const view=element('button','View Para','org-button');view.type='button';
            view.addEventListener('click',()=>viewPara(row.comId));tr.lastElementChild.append(' ',view);
            target.append(tr);
        }
        if (!target.children.length) {
            const tr = element('tr'), cell = element('td','No matching critical items in the returned list.');
            cell.colSpan = 7; tr.append(cell); target.append(tr);
        }
    }
    function loadCritical(rows) {
        if (!Array.isArray(rows) || rows.length > 250) throw new Error('Invalid critical-item cursor.');
        criticalRows = rows;
        const risk = document.getElementById('exec-critical-risk');
        risk.replaceChildren(new Option('All risks',''));
        for (const value of [...new Set(rows.map(row => row.riskName))]) risk.add(new Option(value,value));
        renderCritical();
        document.getElementById('exec-critical-note').textContent =
            'Critical = open High-Risk paras (ZTBL). Showing ' + rows.length + ' returned (limit 100). This capped list is not the entire population.';
    }
    document.getElementById('exec-critical-risk')?.addEventListener('change',renderCritical);
    async function load() {
        if (!validDates()) { resetResults('Select valid dates and Apply.'); return; }
        request = new AbortController();
        app.setAttribute('aria-busy','true');
        status.textContent = 'Loading all selected-period results…';
        const tasks = [];
        if (app.querySelector('[data-snap-card]')) tasks.push(['GetKpis',renderKpis]);
        if (['risk','aging','cycle','trend'].some(name => document.getElementById('exec-' + name)))
            tasks.push(['GetDistribution',renderDistribution]);
        if (document.getElementById('snap-performance')) tasks.push(['GetPerformance',renderPerformance]);
        if (document.getElementById('snap-rankings')) tasks.push(['GetPerformance',renderRankings]);
        if (document.getElementById('snap-movement') || document.getElementById('snap-exposure')) tasks.push(['GetDistribution',renderDistribution]);
        if (document.getElementById('snap-critical-departments')) tasks.push(['GetPerformance',renderCriticalDepartments]);
        if (document.getElementById('exec-critical')) tasks.push(['GetCriticalItems',loadCritical]);
        const results = await Promise.allSettled(tasks.map(async ([action,render]) => {
            const data = await read(action,request.signal);
            if (!request.signal.aborted) render(data);
        }));
        if (request.signal.aborted) return;
        const failures = results.filter(item => item.status === 'rejected');
        if (failures.length) {
            // Do not leave a mixture of refreshed and failed dataset values.
            resetResults('N/A · One or more dated datasets could not be loaded.');
            status.textContent = 'Audit Para Snap unavailable: ' + failures[0].reason.message;
        } else status.textContent = 'IAS data loaded for the selected dates.';
        app.setAttribute('aria-busy','false');
    }
    load();
})();

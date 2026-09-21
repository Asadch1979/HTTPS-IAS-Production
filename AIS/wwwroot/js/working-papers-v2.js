(function () {
    'use strict';

    const root = document.getElementById('wpWorkspace');
    if (!root) return;

    const paperNames = { LCF: 'Loan Case File', VCH: 'Voucher Checking', AOF: 'Account Opening', FAS: 'Fixed Assets', CCT: 'Cash Count' };
    const schemas = {
        LCF: [field('loanNumber','Loan / facility number'),field('productType','Product / facility type'),field('currencyCode','Currency','text','PKR'),field('outstandingAmount','Outstanding amount','number'),field('classification','Classification'),field('overdueDays','Days past due','number'),field('approvalAuthority','Approval authority'),field('requiredSecurityValue','Required security value','number'),field('eligibleSecurityValue','Eligible security value','number'),field('recordedProvision','Recorded provision','number'),field('expectedProvision','Expected provision','number'),field('exceptionSummary','Exception summary','textarea')],
        VCH: [field('voucherNumber','Voucher number'),field('postingDate','Posting date','date'),field('voucherType','Voucher type'),field('currencyCode','Currency','text','PKR'),field('amount','Posted amount','number'),field('recalculatedAmount','Recalculated amount','number'),field('debitAccount','Debit account'),field('creditAccount','Credit account'),field('payee','Payee / vendor'),field('approvalResult','Approval result'),field('supportResult','Supporting documents result'),field('cutoffResult','Cut-off result'),field('exceptionSummary','Exception summary','textarea')],
        AOF: [field('accountNumber','Account number'),field('openDate','Open date','date'),field('customerType','Customer type'),field('accountType','Account type'),field('riskRating','Customer risk rating'),field('screeningResult','Sanctions / PEP screening'),field('beneficialOwnerResult','Beneficial owner verification'),field('documentsResult','Mandatory documents'),field('approvalResult','Approval result'),field('kycReviewDue','KYC review due','date'),field('exceptionSummary','Exception summary','textarea')],
        FAS: [field('assetTag','Asset tag / register ID'),field('description','Description'),field('assetClass','Asset class'),field('recordedLocation','Recorded location'),field('observedLocation','Observed location'),field('exists','Physically exists','select',['Yes','No']),field('condition','Condition'),field('recordedNetBookValue','Recorded net book value','number'),field('expectedNetBookValue','Expected net book value','number'),field('custodianResult','Custodian confirmation'),field('exceptionSummary','Exception summary','textarea')],
        CCT: [field('countPoint','Vault / till / count point'),field('countTimestamp','Count date and time','datetime-local'),field('currencyCode','Currency','text','PKR'),field('denomination','Denomination','number'),field('physicalQuantity','Physical quantity','number'),field('registerQuantity','Register quantity','number'),field('ledgerBalance','Ledger balance','number'),field('custodian','Custodian'),field('witness','Independent witness'),field('dualControl','Dual control','select',['Yes','No']),field('exceptionSummary','Variance explanation / exception','textarea')]
    };
    const descriptions = {
        LCF:'Test approval, documentation, security, performance, classification and provisioning for each sampled facility.',
        VCH:'Test authorization, support, accounting, tax, duplication and cut-off for each sampled transaction.',
        AOF:'Test identity, ownership, screening, risk, documentation and approval for each sampled account.',
        FAS:'Test existence, completeness, location, ownership, movement, condition and valuation for each sampled asset.',
        CCT:'Count by currency and denomination and reconcile physical cash to the register and ledger.'
    };
    let state = { engagementId: Number(root.dataset.engagementId), paperType: root.dataset.paperType, workspace: null, item: null };
    const token = document.querySelector('#wpCsrf input[name="__RequestVerificationToken"]')?.value || '';
    const dialogElement = document.getElementById('wpDialog');
    const dialog = window.bootstrap ? new bootstrap.Modal(dialogElement) : null;

    function field(key, label, type, value) { return { key, label, type: type || 'text', value: value || '' }; }
    function byId(id) { return document.getElementById(id); }
    function value(id) { return byId(id)?.value ?? ''; }
    function numberOrNull(id) { const raw = value(id); return raw === '' ? null : Number(raw); }
    function escapeText(value) { const span = document.createElement('span'); span.textContent = value ?? ''; return span.innerHTML; }

    async function api(url, options) {
        const config = options || {};
        config.headers = Object.assign({ 'Content-Type':'application/json', 'RequestVerificationToken':token }, config.headers || {});
        const response = await fetch((window.g_asiBaseURL || '') + url, config);
        const data = await response.json().catch(() => ({ message: response.statusText }));
        if (!response.ok) throw new Error(data.message || (data.errors ? Object.values(data.errors).flat().join(' ') : 'Request failed.'));
        return data;
    }

    function notify(message, kind) {
        const alert = byId('wpAlert'); alert.className = `alert alert-${kind || 'success'}`; alert.textContent = message; alert.classList.remove('d-none');
        window.setTimeout(() => alert.classList.add('d-none'), 5000);
    }

    async function load() {
        setBusy(true);
        try {
            const query = new URLSearchParams({ engId:String(state.engagementId), paperType:state.paperType });
            state.workspace = await api(`/WorkingPapers/api/workspace?${query}`, { method:'GET' });
            await loadSummary();
            render();
        } catch (error) {
            state.workspace = emptyWorkspace(); render();
            notify('No redesigned paper exists yet. Create one after assigning an independent reviewer.', 'warning');
        } finally { setBusy(false); }
    }

    async function loadSummary() {
        const papers = await api(`/WorkingPapers/api/summary?engId=${state.engagementId}`, { method:'GET' });
        const host = byId('wpSummary');
        if (!host) return;
        host.replaceChildren();
        papers.forEach(paper => {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = `wp-summary-item${paper.paperType === state.paperType ? ' active' : ''}`;
            const title = document.createElement('strong'); title.textContent = paperNames[paper.paperType];
            const status = document.createElement('span'); status.textContent = (paper.status || 'NOT_STARTED').replaceAll('_',' ');
            const progress = document.createElement('progress'); progress.max = 100; progress.value = paper.progress || 0;
            const percent = document.createElement('small'); percent.textContent = `${paper.progress || 0}% complete`;
            button.append(title, status, progress, percent);
            button.addEventListener('click', async () => { state.paperType = paper.paperType; root.dataset.paperType = paper.paperType; await load(); });
            host.appendChild(button);
        });
    }

    function emptyWorkspace() { return { workingPaperId:0, engagementId:state.engagementId, paperType:state.paperType, status:'NOT_CREATED', rowVersion:0, plan:{}, conclusion:{}, items:[], evidence:[], exceptions:[], reviewNotes:[], history:[], legacyRecords:[] }; }
    function setBusy(busy) { root.setAttribute('aria-busy', busy ? 'true' : 'false'); }

    function render() {
        const w = state.workspace;
        byId('wpTitle').textContent = paperNames[state.paperType];
        byId('wpStatus').textContent = (w.status || 'NOT_CREATED').replaceAll('_',' ');
        byId('wpMeta').innerHTML = `<span>Engagement ${state.engagementId}</span><span>${escapeText(w.referenceNo || 'Not created')}</span><span>Version ${w.versionNo || 1}</span><span>Preparer ${escapeText(w.preparerPpno || '-')}</span><span>Reviewer ${escapeText(w.reviewerPpno || '-')}</span>`;
        document.querySelectorAll('.wp-paper-nav a').forEach(a => a.classList.toggle('active', a.dataset.paper === state.paperType));
        byId('testSheetDescription').textContent = descriptions[state.paperType];
        fillPlan(w.plan || {}); fillConclusion(w.conclusion || {}); renderItems(); renderLists(); renderWorkflow(); setEditable(['DRAFT','RETURNED'].includes(w.status));
        if (!w.workingPaperId) promptCreate();
    }

    function fillPlan(p) {
        const map = { planObjectives:p.objectives, planRisks:p.risks, planControls:p.controls, planScope:p.scope, populationSource:p.populationSource, populationAsOf:dateInput(p.populationAsOf), populationCount:p.populationCount, populationValue:p.populationValue, planCurrency:p.currencyCode, samplingMethod:p.samplingMethod, sampleSize:p.sampleSize, samplingRationale:p.samplingRationale, auditProgramReference:p.auditProgramReference };
        Object.entries(map).forEach(([id,v]) => { if (byId(id)) byId(id).value = v ?? ''; });
    }
    function fillConclusion(c) { byId('objectiveAssessment').value=c.objectiveAssessment||'PENDING'; byId('residualRisk').value=c.residualRisk||''; byId('unresolvedIssues').value=c.unresolvedIssues||''; byId('overallConclusion').value=c.overallConclusion||''; }
    function dateInput(v) { return v ? String(v).slice(0,10) : ''; }

    function renderItems() {
        const fields = schemas[state.paperType].slice(0,5);
        byId('itemHead').innerHTML = `<tr><th>Reference</th>${fields.map(f=>`<th>${escapeText(f.label)}</th>`).join('')}<th>Result</th><th>Exceptions</th></tr>`;
        const body = byId('itemBody'); body.replaceChildren();
        (state.workspace.items || []).forEach(item => {
            const tr=document.createElement('tr'); tr.dataset.id=item.itemId;
            const details=item.details||{}; const exceptionCount=(state.workspace.exceptions||[]).filter(x=>x.itemId===item.itemId).length;
            [item.itemReference,...fields.map(f=>details[f.key]),item.result,exceptionCount].forEach(v=>{const td=document.createElement('td');td.textContent=v??'';tr.appendChild(td);});
            tr.addEventListener('click',()=>openItem(item)); body.appendChild(tr);
        });
        if (!body.children.length) body.innerHTML='<tr><td colspan="8" class="wp-empty">No test items recorded.</td></tr>';
        const counts=(state.workspace.items||[]).reduce((a,x)=>(a[x.result]=(a[x.result]||0)+1,a),{});
        byId('conclusionMetrics').textContent=`${state.workspace.items.length} items; ${counts.PASS||0} passed; ${counts.EXCEPTION||0} exceptions; ${(state.workspace.exceptions||[]).filter(x=>x.status!=='RESOLVED').length} unresolved.`;
    }

    function openItem(item) {
        state.item=item||{itemId:0,itemReference:'',details:{},result:'PENDING',auditorComment:'',rowVersion:0};
        byId('itemEditor').classList.remove('d-none'); byId('itemEditorTitle').textContent=state.item.itemId?'Edit test item':'New test item';
        const host=byId('specializedFields'); host.replaceChildren();
        const ref=makeInput({key:'itemReference',label:'Item reference'},state.item.itemReference); host.appendChild(ref);
        schemas[state.paperType].forEach(f=>host.appendChild(makeInput(f,state.item.details?.[f.key]??f.value)));
        byId('itemResult').value=state.item.result||'PENDING'; byId('itemComment').value=state.item.auditorComment||'';
    }

    function makeInput(def, current) {
        const label=document.createElement('label'); label.textContent=def.label; if(def.type==='textarea') label.classList.add('span-2');
        let input;
        if(def.type==='select'){input=document.createElement('select');(def.value||[]).forEach(v=>{const o=document.createElement('option');o.value=v;o.textContent=v;input.appendChild(o);});}
        else if(def.type==='textarea'){input=document.createElement('textarea');input.rows=2;}
        else {input=document.createElement('input');input.type=def.type||'text';if(def.type==='number'){input.step='0.01';input.min='0';}}
        input.dataset.field=def.key; input.value=current??''; label.appendChild(input); return label;
    }

    async function saveItem() {
        const details={}; document.querySelectorAll('#specializedFields [data-field]').forEach(el=>{if(el.dataset.field!=='itemReference') details[el.dataset.field]=el.type==='number'?(el.value===''?null:Number(el.value)):el.value;});
        const request={workingPaperId:state.workspace.workingPaperId,itemId:state.item.itemId,itemReference:document.querySelector('[data-field="itemReference"]').value,details,result:value('itemResult'),auditorComment:value('itemComment'),rowVersion:state.item.rowVersion||0};
        try { await api(`/WorkingPapers/api/item?paperType=${state.paperType}`,{method:'POST',body:JSON.stringify(request)}); notify('Test item saved.'); byId('itemEditor').classList.add('d-none'); await load(); } catch(e){notify(e.message,'danger');}
    }

    function renderLists() {
        renderList('evidenceList',state.workspace.evidence,e=>({title:e.title,rows:[['Evidence ID',e.existingEvidenceId],['Type',e.evidenceType],['Source',e.source],['Date',dateInput(e.evidenceDate)]]}));
        renderList('exceptionList',state.workspace.exceptions,e=>({title:`${e.riskRating} - ${e.status}`,rows:[['Condition',e.condition],['Impact',e.impact],['Owner',e.ownerPpno],['Observation',e.observationId||'Not linked']]}));
        renderList('reviewList',state.workspace.reviewNotes,e=>({title:`${e.sectionKey} - ${e.status}`,rows:[['Note',e.noteText],['Response',e.responseText],['Raised by',e.raisedBy],['Raised on',e.raisedOn]]}));
        renderList('legacyList',state.workspace.legacyRecords,e=>({title:e.displayReference||e.sourceId,rows:[['Source',e.sourceTable],['Source ID',e.sourceId],['Legacy values',e.legacyValuesJson]]}));
        const history=byId('historyList'); history.replaceChildren(); (state.workspace.history||[]).forEach(h=>{const article=document.createElement('article');const strong=document.createElement('strong');strong.textContent=h.action;const div=document.createElement('div');div.className='wp-meta';div.textContent=`${h.actorPpno} / ${new Date(h.actionOn).toLocaleString()}`;const p=document.createElement('p');p.textContent=h.details||'';article.append(strong,div,p);history.appendChild(article);});
        if(!history.children.length) history.innerHTML='<div class="wp-empty">No history recorded.</div>';
    }
    function renderList(id,items,map){const host=byId(id);host.replaceChildren();(items||[]).forEach(item=>{const v=map(item);const article=document.createElement('article');article.className='wp-list-item';const h=document.createElement('h3');h.textContent=v.title;const dl=document.createElement('dl');v.rows.forEach(([k,val])=>{const dt=document.createElement('dt');dt.textContent=k;const dd=document.createElement('dd');dd.textContent=val??'-';dl.append(dt,dd);});article.append(h,dl);host.appendChild(article);});if(!host.children.length)host.innerHTML='<div class="wp-empty">Nothing recorded.</div>';}

    function setEditable(editable){document.querySelectorAll('#savePlan,#addItem,#saveConclusion,#addEvidence,#addException').forEach(el=>el.disabled=!editable);document.querySelectorAll('.wp-panel input,.wp-panel textarea,.wp-panel select').forEach(el=>el.disabled=!editable && !el.closest('#tab-review'));}
    function renderWorkflow(){const btn=byId('wpWorkflow'),s=state.workspace.status;const map={DRAFT:['Submit for review','SUBMIT'],RETURNED:['Resubmit for review','SUBMIT'],IN_REVIEW:['Complete review','REVIEW'],REVIEWED:['Approve and lock','APPROVE']};if(map[s]){btn.textContent=map[s][0];btn.dataset.action=map[s][1];btn.classList.remove('d-none');}else btn.classList.add('d-none');}
    function validateWorkspace(){const issues=[];const p=state.workspace.plan||{};['objectives','risks','controls','scope','populationSource','samplingMethod'].forEach(k=>{if(!p[k])issues.push(k);});if(!(state.workspace.items||[]).length)issues.push('test items');if(!state.workspace.conclusion?.overallConclusion)issues.push('overall conclusion');notify(issues.length?`Incomplete: ${issues.join(', ')}.`:'All core sections are complete.',issues.length?'warning':'success');return !issues.length;}

    async function savePlan(){const plan={objectives:value('planObjectives'),risks:value('planRisks'),controls:value('planControls'),scope:value('planScope'),populationSource:value('populationSource'),populationAsOf:value('populationAsOf')||null,populationCount:numberOrNull('populationCount'),populationValue:numberOrNull('populationValue'),currencyCode:value('planCurrency').toUpperCase(),samplingMethod:value('samplingMethod'),sampleSize:numberOrNull('sampleSize'),samplingRationale:value('samplingRationale'),auditProgramReference:value('auditProgramReference')};try{await api('/WorkingPapers/api/plan',{method:'POST',body:JSON.stringify({workingPaperId:state.workspace.workingPaperId,rowVersion:state.workspace.rowVersion,plan})});notify('Plan saved.');await load();}catch(e){notify(e.message,'danger');}}
    async function saveConclusion(){const conclusion={objectiveAssessment:value('objectiveAssessment'),overallConclusion:value('overallConclusion'),unresolvedIssues:value('unresolvedIssues'),residualRisk:value('residualRisk')};try{await api('/WorkingPapers/api/conclusion',{method:'POST',body:JSON.stringify({workingPaperId:state.workspace.workingPaperId,rowVersion:state.workspace.rowVersion,conclusion})});notify('Conclusion saved.');await load();}catch(e){notify(e.message,'danger');}}
    async function workflow(){const action=byId('wpWorkflow').dataset.action;if(action==='SUBMIT'&&!validateWorkspace())return;const remarks=window.prompt('Workflow remarks (optional):','')??null;if(remarks===null)return;try{await api('/WorkingPapers/api/workflow',{method:'POST',body:JSON.stringify({workingPaperId:state.workspace.workingPaperId,action,remarks,rowVersion:state.workspace.rowVersion})});notify('Workflow updated.');await load();}catch(e){notify(e.message,'danger');}}

    function showDialog(title,html,onSave){byId('dialogTitle').textContent=title;byId('dialogBody').innerHTML=html;byId('dialogSave').onclick=onSave;dialog?.show();}
    function promptCreate(){showDialog(`Create ${paperNames[state.paperType]}`,`<div class="wp-form-grid"><label>Independent reviewer PPNO<input id="createReviewer" type="text" maxlength="30"></label><label>Due date<input id="createDue" type="date"></label></div>`,async()=>{try{await api('/WorkingPapers/api/create',{method:'POST',body:JSON.stringify({engagementId:state.engagementId,paperType:state.paperType,reviewerPpno:value('createReviewer'),dueDate:value('createDue')||null})});dialog?.hide();await load();}catch(e){notify(e.message,'danger');}});}
    function addEvidence(){showDialog('Link existing IAS evidence',`<div class="wp-form-grid"><label>Evidence ID<input id="evId"></label><label>Title<input id="evTitle"></label><label>Type<input id="evType"></label><label>Source<input id="evSource"></label><label>Evidence date<input id="evDate" type="date"></label></div>`,async()=>{try{await api('/WorkingPapers/api/evidence',{method:'POST',body:JSON.stringify({workingPaperId:state.workspace.workingPaperId,existingEvidenceId:value('evId'),title:value('evTitle'),evidenceType:value('evType'),source:value('evSource'),evidenceDate:value('evDate')||null})});dialog?.hide();await load();}catch(e){notify(e.message,'danger');}});}
    function addException(){const opts=(state.workspace.items||[]).map(x=>`<option value="${x.itemId}">${escapeText(x.itemReference)}</option>`).join('');showDialog('Add exception',`<div class="wp-form-grid"><label>Test item<select id="exItem">${opts}</select></label><label>Risk<select id="exRisk"><option>LOW</option><option>MEDIUM</option><option>HIGH</option><option>CRITICAL</option></select></label><label class="span-2">Criteria<textarea id="exCriteria"></textarea></label><label class="span-2">Condition<textarea id="exCondition"></textarea></label><label class="span-2">Cause<textarea id="exCause"></textarea></label><label class="span-2">Impact<textarea id="exImpact"></textarea></label><label>Owner PPNO<input id="exOwner"></label><label>Due date<input id="exDue" type="date"></label><label>Existing IAS observation ID<input id="exObs" type="number"></label></div>`,async()=>{try{await api('/WorkingPapers/api/exception',{method:'POST',body:JSON.stringify({workingPaperId:state.workspace.workingPaperId,itemId:Number(value('exItem')),criteria:value('exCriteria'),condition:value('exCondition'),cause:value('exCause'),impact:value('exImpact'),riskRating:value('exRisk'),ownerPpno:value('exOwner'),dueDate:value('exDue')||null,observationId:numberOrNull('exObs'),rowVersion:0})});dialog?.hide();await load();}catch(e){notify(e.message,'danger');}});}
    function addReviewNote(){showDialog('Add supervisory review note',`<div class="wp-form-grid"><label>Section<select id="rnSection"><option>Plan</option><option>Test Sheet</option><option>Evidence</option><option>Exceptions</option><option>Conclusion</option></select></label><label class="span-2">Review note<textarea id="rnText" rows="4"></textarea></label></div>`,async()=>{try{await api('/WorkingPapers/api/review-note',{method:'POST',body:JSON.stringify({workingPaperId:state.workspace.workingPaperId,sectionKey:value('rnSection'),noteText:value('rnText'),status:'OPEN'})});dialog?.hide();await load();}catch(e){notify(e.message,'danger');}});}

    document.querySelectorAll('.wp-paper-nav a').forEach(a=>a.addEventListener('click',async()=>{state.paperType=a.dataset.paper;root.dataset.paperType=state.paperType;await load();}));
    document.querySelectorAll('.wp-tabs button').forEach(b=>b.addEventListener('click',()=>{document.querySelectorAll('.wp-tabs button,.wp-panel').forEach(x=>x.classList.remove('active'));b.classList.add('active');byId(`tab-${b.dataset.tab}`).classList.add('active');}));
    byId('savePlan').addEventListener('click',savePlan);byId('saveConclusion').addEventListener('click',saveConclusion);byId('addItem').addEventListener('click',()=>openItem(null));byId('saveItem').addEventListener('click',saveItem);byId('closeItem').addEventListener('click',()=>byId('itemEditor').classList.add('d-none'));byId('addEvidence').addEventListener('click',addEvidence);byId('addException').addEventListener('click',addException);byId('addReviewNote').addEventListener('click',addReviewNote);byId('wpValidate').addEventListener('click',validateWorkspace);byId('wpWorkflow').addEventListener('click',workflow);
    load();
})();

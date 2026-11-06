(function (window, $) {
    'use strict';
    $(function () {
    const app=window.legacyPara; if (!app || !app.enabled) return;
    const annexes=JSON.parse(document.getElementById('legacy-page-data').textContent).AnnexList || [];
    let persons=[], staged=[], editing=null, files=[], restoring=false, auditBy=Number(document.getElementById('legacyParaWorkspace').dataset.auditBy)||null, entityLoad=0;
    const run=fn => Promise.resolve().then(fn).catch(e => app.message(e.message,true));
    const val=id => $('#' + id).val();
    const num=id => Number(val(id));
    const lookup=(kind,parentId,search,relationshipId) => app.api('Lookup',{kind,parentId:Number(parentId)||0,search:search||'',relationshipId:Number(relationshipId)||0});
    async function options(id,kind,parent) { const select=document.getElementById(id); select.replaceChildren(new Option('Select an option','')); if (Number(parent)>0) (await lookup(kind,parent)).forEach(row => select.add(new Option(row.label,row.id))); }
    function risk() { const a=annexes.find(a => String(a.ID)===String(val('updatedAnnexlist'))); $('#viewMemo_risk_display').val(a ? a.RISK : ''); }
    function renderPersons() {
        const body=document.querySelector('#listofRespPersons tbody'); body.replaceChildren();
        persons.forEach((p,index) => { const tr=app.node('tr'); [index+1,p.ppNo,p.employeeName,p.loanCase,p.loanAmount,p.accountNumber,p.accountAmount].forEach(v => tr.append(app.node('td',v)));
            ['Update','Delete'].forEach(action => { const td=app.node('td'),b=app.node('button',action,'btn btn-sm btn-outline-secondary'); b.type='button'; b.onclick=() => { if (action==='Delete') { persons.splice(index,1); renderPersons(); } else { editing=index; staged=[{...p}]; fillPerson(p); renderStaged(); bootstrap.Modal.getOrCreateInstance(document.getElementById('ResponsiblePPModel')).show(); } };td.append(b);tr.append(td); });body.append(tr); });
    }
    function fillPerson(p) { $('#responsiblePPNoEntryField').val(p.ppNo); $('#responsibleLoanNumberEntryField').val(p.loanCase); $('#responsibleLoanAmountEntryField').val(p.loanAmount); $('#responsibleAccountNumberEntryField').val(p.accountNumber); $('#responsibleAccountAmountEntryField').val(p.accountAmount); }
    function renderStaged() { const target=document.querySelector('#respPendingTable tbody'); target.replaceChildren(); staged.forEach((p,index) => { const tr=app.node('tr'); [p.role,p.ppNo,p.employeeName,p.loanCase,p.loanAmount,p.accountNumber,p.accountAmount].forEach(v => tr.append(app.node('td',v))); const td=app.node('td'),b=app.node('button','Remove','btn btn-sm btn-outline-danger');b.type='button';b.onclick=() => { staged.splice(index,1);renderStaged(); };td.append(b);tr.append(td);target.append(tr); }); }
    window.openResponsiblePPs=function () { editing=null;staged=[];fillPerson({});renderStaged();$('#responsibleLCNoEntryField,#responsibleBrCodeEntryField').val('');$('#updateResponsibleButton,#deleteResponsibleButton').addClass('d-none');$('#addResponsibleButton').removeClass('d-none');bootstrap.Modal.getOrCreateInstance(document.getElementById('ResponsiblePPModel')).show(); };
    window.getMatchedPP=function () { run(async () => { const generation=entityLoad;const rows=await lookup('PERSON',val('responsiblePPNoEntryField'));if(generation!==entityLoad) return; if (!rows.length) throw new Error('Employee not found.'); const p=rows[0];staged=[{ppNo:String(p.id),employeeName:p.label,role:'',loanCase:val('responsibleLoanNumberEntryField')||'',loanAmount:num('responsibleLoanAmountEntryField')||0,accountNumber:val('responsibleAccountNumberEntryField')||'',accountAmount:num('responsibleAccountAmountEntryField')||0}];renderStaged(); }); };
    window.getLCDetails=function () { run(async () => { const generation=entityLoad;const entity=num('entitySelectField');if (!entity) throw new Error('Select the audit entity first.');const rows=await lookup('LC',entity,val('responsibleLCNoEntryField'));if(generation!==entityLoad || entity!==num('entitySelectField')) return;staged=[];rows.forEach(r => { [['MCO','mco_ppno','mco_name'],['Manager','manager_ppno','manager_name'],['RGM','rgm_ppno','rgm_name'],['CAD Reviewer','cad_reviewer','cad_name'],['CAD Authorizer','cad_authorizer','cad_authorizer_name']].forEach(([role,pp,name]) => { if (r[pp]) staged.push({ppNo:String(r[pp]),employeeName:r[name],role,loanCase:r.loan_case_no,loanAmount:r.outstanding_amount||0,accountNumber:'',accountAmount:0}); }); });if (!staged.length) throw new Error('No responsible persons found for this entity and loan case.');renderStaged(); }); };
    window.addResponsibilityToMainTable=function () { if (!staged.length) return; if (editing!==null) persons.splice(editing,1,...staged);else persons.push(...staged);renderPersons();staged=[];editing=null;bootstrap.Modal.getInstance(document.getElementById('ResponsiblePPModel')).hide(); };
    window.div_risksubcategoryShowHide=function () { run(async () => { $('#div_risksubcategory').show();$('#div_activityContainer').hide();$('#riskActivitiesSelectBox').empty();await options('riskSubGroupSelectBox','SUBCHECKLIST',val('riskGroupSelectBox')); }); };
    window.div_activityContainerShowHide=function () { run(async () => { $('#div_activityContainer').show();await options('riskActivitiesSelectBox','CHECKLIST',val('riskSubGroupSelectBox')); }); };
    // CAU template provider currently returns no templates; keep entered text when changing a checklist.
    window.getAuditObservationTemplate=function () {};
    // Bind the shared markup directly; the legacy editor needs no evaluated handlers.
    document.querySelectorAll('#legacyParaWorkspace [data-onclick],#legacyParaWorkspace [data-onchange]').forEach(element => {
        ['data-onclick','data-onchange'].forEach(attribute => {
            const expression=element.getAttribute(attribute);if (!expression) return;
            element.removeAttribute(attribute);
            const match=expression.match(/^([A-Za-z0-9_]+)\(/);
            const allowed=['openResponsiblePPs','getLCDetails','getMatchedPP','addResponsibilityToMainTable','div_risksubcategoryShowHide','div_activityContainerShowHide','getAuditObservationTemplate'];
            if (match && allowed.includes(match[1])) element.addEventListener(attribute==='data-onclick'?'click':'change',() => window[match[1]]());
        });
    });
    const editorOptions = {bold:true,italic:true,underline:true,leftAlign:true,centerAlign:true,rightAlign:true,justify:true,ol:true,ul:true,heading:true,fonts:true,fontColor:true,fontSize:true,table:true,removeStyles:true,code:true,imageUpload:false,fileUpload:false,videoEmbed:false,urls:false};
    let $source = $('#template_box'), $editable;
    function setObservationHtml(html) {
        // This plugin CLONES the initial textarea. Rebuild its wrapper to reset
        // undo/redo and code-view state, then reacquire the actual cloned input.
        const $next = $('<textarea>', {id:'template_box',class:'content',name:'example'}).val(html == null ? '' : html);
        const $old = $source.closest('.richText');
        if ($old.length) $old.replaceWith($next); else $source.replaceWith($next);
        $next.richText(editorOptions);
        $source = $('#template_box');
        $editable = $source.siblings('.richText-editor');
    }
    function observationHtml() {
        // Code view edits the textarea; WYSIWYG edits the contenteditable sibling.
        const html = $source.is(':visible') ? $source.val() : $editable.html();
        $source.val(html == null ? '' : html);
        return $source.val();
    }
    setObservationHtml($source.val());
    $('#amount_inv_field').attr({min:0,step:'0.01'});$('#no_instances_field').attr({min:0,step:1});
    $('#updatedAnnexlist').on('change',risk);
    function clearEntityData() { ++entityLoad;persons=[];staged=[];editing=null;renderPersons();renderStaged();fillPerson({});$('#responsibleLCNoEntryField,#responsibleBrCodeEntryField').val(''); }
    function clearOffices() {
        document.getElementById('reportingOfficeSelectField').replaceChildren(new Option('--Select Controlling/Reporting Office--',''));
    }
    function clearPostings() {
        document.getElementById('entitySelectField').replaceChildren(new Option('--Select Place of Posting--',''));
        $('#legacyEntityTypeId').val('');
    }
    $('#relationshipSelectField').on('change',() => {
        clearEntityData(); clearOffices(); clearPostings();
        const generation=entityLoad, relation=num('relationshipSelectField');
        if (!relation) return;
        run(async () => {
            const offices=await lookup('REPORTING_OFFICES',relation);
            if (generation!==entityLoad || relation!==num('relationshipSelectField')) return;
            const select=document.getElementById('reportingOfficeSelectField');
            offices.forEach(r => select.add(new Option(r.label,r.id)));
        });
    });
    $('#reportingOfficeSelectField').on('change',() => {
        clearEntityData(); clearPostings();
        const generation=entityLoad, relation=num('relationshipSelectField'), office=num('reportingOfficeSelectField');
        if (!relation || !office) return;
        run(async () => {
            const postings=await lookup('POSTINGS',office,'',relation);
            if (generation!==entityLoad || relation!==num('relationshipSelectField') || office!==num('reportingOfficeSelectField')) return;
            const select=document.getElementById('entitySelectField');
            postings.forEach(r => {
                const option=new Option(r.label,r.id);
                option.dataset.entityTypeId=String(r.entity_type_id);
                select.add(option);
            });
        });
    });
    $('#entitySelectField').on('change',() => {
        clearEntityData();
        const selected=document.getElementById('entitySelectField').selectedOptions[0];
        $('#legacyEntityTypeId').val(selected && selected.dataset.entityTypeId || '');
    });
    // Audit By belongs to the authenticated selected context; it is not a
    // user-editable controlling office or a routing hint.
    if (!auditBy) run(async () => {
        const context=await lookup('CONTEXT');
        if (!Array.isArray(context) || context.length!==1 || !(Number(context[0].id)>0))
            throw new Error('Select an active authorized audit context.');
        auditBy=Number(context[0].id);
    });
    if ($.fn.select2) $('#updatedAnnexlist,#riskActivitiesSelectBox').select2();
    if (window.initObservationReference) window.initObservationReference('#observationReferenceSection',{forceReload:true,initialRefId:null});
    // The selected audit entity is the trusted loan-lookup scope; no free branch-code routing.
    $('#responsibleBrCodeEntryField').prop('disabled',true).attr('placeholder','Uses selected audit entity');
    const renderFiles=() => app.attachments(document.getElementById('legacyAttachmentList'),files,index => {files.splice(index,1);renderFiles();});
    $('#legacyAttachments').on('change',() => run(async () => { const incoming=Array.from(document.getElementById('legacyAttachments').files); if (files.length+incoming.length>10) throw new Error('Select at most ten attachments.'); const additions=[];let total=files.reduce((sum,f) => sum+Math.floor(f.base64.length*3/4),0);for(const f of incoming) {total+=f.size;if(f.size>5000000 || total>10000000) throw new Error('Attachments allow 5 MB per file and 10 MB total.');const base64=await new Promise((resolve,reject) => {const reader=new FileReader();reader.onload=() => resolve(String(reader.result).split(',')[1]);reader.onerror=reject;reader.readAsDataURL(f);});additions.push({name:f.name,contentType:f.type||'application/octet-stream',base64});}files.push(...additions);$('#legacyAttachments').val('');renderFiles(); }));
    function clear() { ++entityLoad;clearEntityData();$('#relationshipSelectField,#reportingOfficeSelectField,#entitySelectField,#legacyEntityTypeId,#natureSelectField,#yearSelectField').val('');clearOffices();clearPostings();$('#legacyRequestId,#legacyVersion').val(0);$('#paraNoField,#viewMemo_heading,#amount_inv_field,#no_instances_field').val('');setObservationHtml('');persons=[];files=[];renderPersons();renderFiles();$('#legacySubmit').text('Submit for Authorization');$('#updatedAnnexlist,#riskGroupSelectBox').val('0').trigger('change');$('#riskSubGroupSelectBox,#riskActivitiesSelectBox').empty();$('#div_risksubcategory,#div_activityContainer').hide();$('#observationReferenceId').val('');if(window.initObservationReference)window.initObservationReference('#observationReferenceSection',{forceReload:true,initialRefId:null}); }
    $('#legacyNewRequest').on('click',clear);
    $('#legacySubmit').on('click',() => run(async () => {
        if (restoring) return;
        if (!num('relationshipSelectField') || !num('reportingOfficeSelectField') || !num('entitySelectField') || !num('legacyEntityTypeId'))
            throw new Error('Select Relationship Type, Controlling/Reporting Office and Place of Posting.');
        const reference=window.getSelectedObservationReference ? window.getSelectedObservationReference('#observationReferenceSection') : null;
        const payload={requestId:num('legacyRequestId'),version:num('legacyVersion'),relationshipId:num('relationshipSelectField'),reportingOfficeId:num('reportingOfficeSelectField'),entityTypeId:num('legacyEntityTypeId'),entityId:num('entitySelectField'),natureId:num('natureSelectField'),auditYear:num('yearSelectField'),paraNo:val('paraNoField'),gist:val('viewMemo_heading'),annexureDetails:null,annexureId:num('updatedAnnexlist'),categoryId:num('riskGroupSelectBox'),subChecklistId:num('riskSubGroupSelectBox'),checklistId:num('riskActivitiesSelectBox'),heading:val('viewMemo_heading'),observationHtml:observationHtml(),amount:num('amount_inv_field'),instances:num('no_instances_field'),referenceId:Number(reference && reference.refId || val('observationReferenceId'))||null,responsiblePersons:persons,attachments:files};
        $('#legacySubmit').prop('disabled',true);try {const rows=await app.api('Submit',payload);app.message('Request '+rows[0].request_id+' submitted for authorization.');clear();}finally{$('#legacySubmit').prop('disabled',false);}
    }));
    });
})(window,window.jQuery);

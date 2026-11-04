(function (window, $) {
    'use strict';
    $(function () {
    const root = document.getElementById('legacyParaWorkspace');
    if (!root) return;
    const checker = root.dataset.mode === 'checker', enabled = root.dataset.enabled === 'true';
    let offset = 0, selected = null;
    const node = (tag, text, cls) => { const el = document.createElement(tag); if (text != null) el.textContent = text; if (cls) el.className = cls; return el; };
    function message(text, error) { const box = document.getElementById('legacyMessage'); box.textContent = text; box.className = 'alert ' + (error ? 'alert-danger' : 'alert-success'); }
    async function api(action, input) {
        if (!enabled) throw new Error('Legacy para addition is not activated.');
        const response = await fetch(root.dataset.api + action, { method: 'POST', credentials: 'same-origin', cache: 'no-store', headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': root.querySelector('[name="__RequestVerificationToken"]').value }, body: JSON.stringify(input) });
        let result; try { result = await response.json(); } catch (_) { throw new Error('Unable to process request. Your session or permission may have changed.'); }
        if (!response.ok || !(result.status ?? result.Status)) throw new Error(result.message || result.Message || 'Request failed.');
        return result.data || result.Data || [];
    }
    function table(headers, rows) {
        const t = node('table', null, 'table table-sm table-bordered'), head = node('tr'), thead = node('thead');
        headers.forEach(h => head.append(node('th', h))); thead.append(head); t.append(thead);
        const body = node('tbody'); rows.forEach(row => { const tr = node('tr'); row.forEach(value => tr.append(node('td', value ?? ''))); body.append(tr); }); t.append(body); return t;
    }
    function attachments(target, files, removable) {
        target.replaceChildren();
        files.forEach((file, index) => {
            const line = node('div', null, 'd-flex gap-2 my-1'), download = node('button', file.name, 'btn btn-link p-0'); download.type = 'button';
            download.onclick = () => { const bytes = Uint8Array.from(atob(file.base64), c => c.charCodeAt(0)), url = URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' })), a = node('a'); a.href = url; a.download = file.name; document.body.append(a); a.click(); a.remove(); window.setTimeout(() => URL.revokeObjectURL(url), 1000); }; line.append(download);
            if (removable) { const button = node('button', 'Remove', 'btn btn-sm btn-outline-danger'); button.type = 'button'; button.onclick = () => removable(index); line.append(button); }
            target.append(line);
        });
    }
    async function refresh() {
        const rows = await api('List', { queue: checker, search: $('#legacySearch').val(), offset });
        const body = document.getElementById('legacyRequests'); body.replaceChildren();
        rows.forEach(row => {
            const tr = node('tr'); [row.request_id,row.entity,row.audit_year,row.para_no,row.heading,row.risk,(row.requested_by_name || '') + ' (' + row.requested_by + ')',row.submitted_on,row.status].forEach(v => tr.append(node('td', v)));
            const td = node('td'), button = node('button', row.status === 'RETURNED' && !checker ? 'View / Correct' : 'View', 'btn btn-sm btn-outline-primary'); button.type = 'button'; button.onclick = () => preview(row.request_id).catch(e => message(e.message,true)); td.append(button); tr.append(td); body.append(tr);
        });
        if (!rows.length) { const tr = node('tr'), td = node('td','No requests found.'); td.colSpan = 10; tr.append(td); body.append(tr); }
        $('#legacyPrevious').prop('disabled',offset === 0); $('#legacyNext').prop('disabled',rows.length < 50);
    }
    async function preview(id) {
        const rows = await api('Detail', { requestId: id, queue: checker });
        const record = rows.find(r => r.record_kind === 'REQUEST'); if (!record) throw new Error('Request is unavailable.');
        const data = JSON.parse(record.payload_json); selected = { record, data };
        const list = document.getElementById('legacyPreviewAttributes'); list.replaceChildren();
        const values = { 'Request': record.request_id,'Status': record.status,'Entity': record.entity,'Audit Year': data.auditYear,'Audit Nature': record.nature,'Original Para Number': data.paraNo,'Heading': data.heading,'Annexure': record.annexure,'Risk': record.risk,'Violation Category': record.category,'Violation Nature': record.subchecklist,'Checklist Detail': record.checklist,'Reference': record.reference_label || 'None selected','Amount Involved': data.amount,'Instances': data.instances,'Remarks': record.remarks || '','Legacy Reference': record.legacy_ref_p || '','Compliance Reference': record.compliance_id || '' };
        Object.entries(values).forEach(([label,value]) => { list.append(node('dt',label,'col-sm-3')); list.append(node('dd',value,'col-sm-9')); });
        document.getElementById('legacyObservationPreview').srcdoc = record.observation_html;
        document.getElementById('legacyPreviewPersons').replaceChildren(table(['PP No','Name','Role','Loan Case','Loan Amount','Account','Account Amount'],(data.responsiblePersons || []).map(p => [p.ppNo,p.employeeName,p.role,p.loanCase,p.loanAmount,p.accountNumber,p.accountAmount])));
        attachments(document.getElementById('legacyPreviewAttachments'),data.attachments || []);
        document.getElementById('legacyPreviewHistory').replaceChildren(table(['Action','Version','Actor PP No','Date','Remarks'],JSON.parse(record.history_json || '[]').map(h => [h.status,h.version_no,h.actor,h.action_on,h.remarks])));
        $('#legacyDecisionPanel').toggleClass('d-none',!checker || record.status !== 'PENDING'); $('#legacyCorrect').toggleClass('d-none',checker || record.status !== 'RETURNED'); $('#legacyRemarks').val('');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('legacyPreview')).show();
    }
    async function decide(action) {
        if (!selected) return;
        const remarks = $('#legacyRemarks').val().trim(); if (action !== 'AUTHORIZE' && !remarks) throw new Error('Enter remarks for return or rejection.');
        $('#legacyDecisionPanel button').prop('disabled',true);
        try { await api('Decide',{ requestId:selected.record.request_id,version:selected.record.version_no,action,remarks }); bootstrap.Modal.getInstance(document.getElementById('legacyPreview')).hide(); message('Request action completed.'); await refresh(); }
        finally { $('#legacyDecisionPanel button').prop('disabled',false); }
    }
    window.legacyPara = { api,message,refresh,node,table,attachments,enabled,checker,get selected() { return selected; } };
    $('#legacyRefresh').on('click',() => { offset=0; refresh().catch(e => message(e.message,true)); });
    $('#legacySearch').on('keydown',e => { if (e.key === 'Enter') $('#legacyRefresh').trigger('click'); });
    $('#legacyPrevious').on('click',() => { offset=Math.max(0,offset-50); refresh().catch(e => message(e.message,true)); });
    $('#legacyNext').on('click',() => { offset+=50; refresh().catch(e => message(e.message,true)); });
    [['legacyAuthorize','AUTHORIZE'],['legacyReturn','RETURN'],['legacyReject','REJECT']].forEach(([id,action]) => $('#' + id).on('click',() => decide(action).catch(e => message(e.message,true))));
    if (enabled) refresh().catch(e => message(e.message,true)); else $('#legacyRefresh,#legacyNext,#legacyPrevious,#legacySearch').prop('disabled',true);
    });
})(window,window.jQuery);

(function (window, $) {
    'use strict';
    $(function () {
    const root = document.getElementById('legacyParaWorkspace');
    if (!root) return;
    const checker = root.dataset.mode === 'checker', enabled = root.dataset.enabled === 'true';
    let offset = 0, selected = null;
    const node = (tag, text, cls) => { const el = document.createElement(tag); if (text != null) el.textContent = text; if (cls) el.className = cls; return el; };
    function message(text, error) { const box = document.getElementById('legacyMessage'); box.textContent = text; box.className = 'alert ' + (error ? 'alert-danger' : 'alert-success'); }
    const failureMessages = {
        400: 'The request is invalid or its security token expired. Refresh the page and review the entered values.',
        401: 'Your session has expired or you are signed out. Sign in again.',
        403: 'Access was denied. Verify the selected role, assigned page and approved API route permissions.',
        404: 'The requested resource or API is unavailable.',
        409: 'The request changed, conflicts with another para, or cannot be processed in its current state. Refresh and review it.',
        500: 'The server could not process the request. Use the reference below when reporting the problem.',
        503: 'Legacy para addition or posting is currently unavailable.'
    };
    function safeReference(value) {
        return typeof value === 'string' && /^[A-Za-z0-9_.:-]{1,128}$/.test(value) ? value : '';
    }
    function apiFailure(operation, response, reference, reason) {
        const status = response ? response.status : 0;
        const responseType = response ? (response.headers.get('content-type') || '').split(';')[0].toLowerCase() : '';
        // No URLs, cookies, tokens, observation text, request bodies or exceptions.
        console.warn('Legacy para API failure', { operation, status, responseType,
            redirected: !!(response && response.redirected), reference: reference || undefined, reason });
        const text = response && response.redirected
            ? 'The API unexpectedly redirected. Your session may have expired; sign in again.'
            : failureMessages[status] || (status === 0 ? 'The server could not be reached. Try again when the connection is available.' : 'An unexpected API response was received. Refresh the page.');
        return new Error(text + (reference ? ' Reference: ' + reference : ''));
    }
    async function api(action, input) {
        if (!enabled) throw new Error('Legacy para addition is not activated.');
        let response;
        try {
            response = await fetch(root.dataset.api + action, { method: 'POST', credentials: 'same-origin', cache: 'no-store',
                headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': root.querySelector('[name="__RequestVerificationToken"]').value }, body: JSON.stringify(input) });
        } catch (_) { throw apiFailure(action, null, '', 'network'); }
        const status = response.status;
        const successful = status >= 200 && status < 300;
        const referenceHeader = safeReference(response.headers.get('X-Trace-Id'));
        const contentType = (response.headers.get('content-type') || '').split(';')[0].trim().toLowerCase();
        // Inspect transport status and headers first. Never parse or display HTML,
        // including a followed login redirect that happens to return HTTP 200.
        if (response.redirected || response.type === 'opaqueredirect')
            throw apiFailure(action, response, referenceHeader, 'redirect');
        if (contentType !== 'application/json' && contentType !== 'application/problem+json')
            throw apiFailure(action, response, referenceHeader, 'non-json');
        let result;
        try { result = await response.json(); }
        catch (_) { throw apiFailure(action, response, referenceHeader, 'invalid-json'); }
        const reference = referenceHeader || safeReference(result && (result.reference || result.Reference || result.traceId));
        const publicErrors={
            UNAUTHENTICATED:'Your session has expired. Sign in again.',
            CONTEXT_REQUIRED:'Select an active authorized user context.',
            PAGE_FORBIDDEN:'The selected group is not authorized for this Legacy Para page.',
            API_FORBIDDEN:'The selected role does not have an active grant for this API route. Check the ApiCalls registration and sign in again after approved updates.',
            DATABASE_FORBIDDEN:'Oracle denied this operation. Check the active role grant, page dependency and selected organizational scope.',
            CONTEXT_FORBIDDEN:'The selected user context is unavailable or no longer authorized.',
            MODULE_DISABLED:'Legacy para addition is not activated.',
            OPERATION_CONTRACT_MISMATCH:'The deployed application and Oracle package operation contract do not match. Report this reference.',
            CAD_HO_UNSUPPORTED:'CAD/HO historical posting is disabled pending an approved authorization workflow.',
            ENTITY_SCOPE_CHANGED:'Entity scope cannot be established for this Audit By office. Organizational reassignment requires controlled resolution.',
            HISTORICAL_FIELDS_REQUIRED:'Enter the historical gist and review annexure details before resubmission.'
        };
        const code=result && (result.code || result.Code);
        if (!successful && Object.prototype.hasOwnProperty.call(publicErrors,code)) throw new Error(publicErrors[code]+' ('+code+')'+(reference?' Reference: '+reference:''));
        if (!successful || !result || typeof result !== 'object' || !(result.status ?? result.Status))
            throw apiFailure(action, response, reference, 'rejected');
        // Server messages are intentionally not shown: error text can be an HTML
        // status page or an internal exception from an upstream component.
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
            const tr = node('tr'); [row.request_id,row.entity,row.audit_year,row.para_no,row.heading,row.risk,(row.requested_by_name || '') + ' (' + row.requested_by + ')',row.submitted_on,row.status+(row.scope_notice?' — '+row.scope_notice:'')].forEach(v => tr.append(node('td', v)));
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
        const values = { 'Request': record.request_id,'Status': record.status,'Entity': record.entity,'Audit Year': data.auditYear,'Audit Nature': record.nature,'Original Para Number': data.paraNo,'Gist of Para':data.gist || 'Not supplied','Historical Annexure Details':data.annexureDetails || '','Audit By ID':record.audited_by,'Scope Notice':record.scope_notice || '','Heading': data.heading,'Annexure': record.annexure,'Risk': record.risk,'Violation Category': record.category,'Violation Nature': record.subchecklist,'Checklist Detail': record.checklist,'Reference': record.reference_label || 'None selected','Amount Involved': data.amount,'Instances': data.instances,'Remarks': record.remarks || '','Legacy Reference': record.legacy_ref_p || '','Compliance Reference': record.compliance_id || '' };
        Object.entries(values).forEach(([label,value]) => { list.append(node('dt',label,'col-sm-3')); list.append(node('dd',value,'col-sm-9')); });
        document.getElementById('legacyObservationPreview').srcdoc = record.observation_html;
        document.getElementById('legacyPreviewPersons').replaceChildren(table(['PP No','Name','Role','Loan Case','Loan Amount','Account','Account Amount'],(data.responsiblePersons || []).map(p => [p.ppNo,p.employeeName,p.role,p.loanCase,p.loanAmount,p.accountNumber,p.accountAmount])));
        attachments(document.getElementById('legacyPreviewAttachments'),data.attachments || []);
        document.getElementById('legacyPreviewHistory').replaceChildren(table(['Action','Version','Actor PP No','Date','Remarks'],JSON.parse(record.history_json || '[]').map(h => [h.status,h.version_no,h.actor,h.action_on,h.remarks])));
        $('#legacyDecisionPanel').toggleClass('d-none',!checker || record.status !== 'PENDING'); $('#legacyCorrect').toggleClass('d-none',checker || record.status !== 'RETURNED' || !!record.scope_notice); $('#legacyRemarks').val('');
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
    // The maker page is data capture only. It must not query LIST or DETAIL
    // on load, and it does not expose the request queue or preview UI.
    // Only the separate checker page may initialize the review queue.
    if (checker) {
        if (enabled) refresh().catch(e => message(e.message,true));
        else $('#legacyRefresh,#legacyNext,#legacyPrevious,#legacySearch').prop('disabled',true);
    }
    });
})(window,window.jQuery);

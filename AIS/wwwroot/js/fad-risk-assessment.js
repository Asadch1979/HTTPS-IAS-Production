(function () {
    'use strict';
    const root = document.getElementById('riskAssessment');
    if (!root) return;
    const el = id => document.getElementById('risk' + id);
    let header, rows = [], userId, busy = false, dirty = false, mustReload = false, permissions = {};
    const text = (tag, value) => { const n = document.createElement(tag); n.textContent = value == null ? '' : String(value); return n; };
    const message = value => { el('Message').textContent = value; };
    async function api(name, data, post) {
        const url = new URL(root.dataset[name], location.origin);
        if (!post) Object.entries(data || {}).forEach(([k, v]) => url.searchParams.set(k, v));
        const response = await fetch(url, { method: post ? 'POST' : 'GET', credentials: 'same-origin',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest', ...(post ? {
                'Content-Type': 'application/json', 'RequestVerificationToken': root.querySelector('[name="__RequestVerificationToken"]').value } : {}) },
            body: post ? JSON.stringify(data) : undefined });
        const body = await response.json().catch(() => null);
        if (!response.ok || !body || !body.success) throw new Error(body?.message || 'The request failed. Check your session and permissions.');
        return body.data;
    }
    async function run(work) {
        if (busy) return;
        busy = true; buttons();
        try { await work(); } catch (e) { message(e.message); }
        finally { busy = false; buttons(); }
    }
    function buttons() {
        const edit = !mustReload && permissions.canSave && header && (!header.assessment_id || (Number(header.preparer_id) === userId && ['DRAFT', 'RETURNED'].includes(header.status)));
        el('Fields').disabled = busy || !edit;
        el('Save').disabled = busy || !edit;
        el('New').disabled = busy || !permissions.canSave;
        el('Submit').disabled = busy || dirty || !edit || !permissions.canSubmit || !header?.assessment_id;
        const review = !mustReload && permissions.canReview && header?.status === 'SUBMITTED' && Number(header.preparer_id) !== userId;
        el('Approve').disabled = el('Return').disabled = busy || !review;
    }
    async function catalog() {
        const data = await api('catalog'); userId = Number(data.currentUserId); permissions = data;
        for (const [id, values, valueKey, label] of [
            ['Engagement', data.engagements, 'engagementId', x => `${x.engagementId} | ${x.branch} | ${x.period}`],
            ['Version', data.catalog.versions, 'version_id', x => x.version_name]]) {
            const select = el(id), old = select.value; select.replaceChildren(new Option('Select', ''));
            values.forEach(x => select.add(new Option(label(x), x[valueKey]))); select.value = old;
        }
        el('List').replaceChildren();
        data.catalog.assessments.forEach(a => {
            const tr = document.createElement('tr');
            [`${a.eng_id} | ${a.branch_name}`, a.version_id, a.status, a.rating].forEach(v => tr.append(text('td', v)));
            const td = document.createElement('td'), button = text('button', 'Open'); button.type = 'button'; button.className = 'btn btn-sm btn-outline-primary';
            button.onclick = () => run(async () => { if (!dirty || confirm('Discard unsaved changes?')) await open(a.eng_id, a.assessment_id, a.version_id); });
            td.append(button); tr.append(td); el('List').append(tr);
        });
    }
    async function open(engagementId, assessmentId, versionId) {
        const data = await api('read', { engagementId, assessmentId, versionId });
        if (data.header.length !== 1) throw new Error('Assessment header unavailable.');
        header = data.header[0]; rows = data.details; dirty = false; mustReload = false;
        el('Editor').hidden = false;
        el('Heading').textContent = `${header.branch_name} | ${header.audit_period}`;
        el('Metadata').textContent = `${header.version_name} | ${header.status || 'New'} | Revision ${header.revision || 0} | Preparer: ${header.preparer_name || 'Not saved'} | Reviewer: ${header.reviewer_name || 'Not reviewed'}`;
        el('Final').textContent = header.rating ? `Final rating: ${header.rating} | Score: ${header.score} | Fraud instances: ${header.fraud_count}` : 'Save the complete matrix to calculate the final rating.';
        el('Size').value = header.branch_size || ''; el('Portfolio').value = header.portfolio ?? ''; el('Reason').value = header.size_reason || '';
        el('Matrix').replaceChildren();
        rows.forEach(row => {
            const tr = document.createElement('tr'); row.inputs = {};
            tr.append(text('td', `${row.main_kpi} > ${row.sub_kpi} > ${row.process_name} > ${row.sub_code} | ${row.item_code}: ${row.description}`));
            tr.append(text('td', `${row.gravity ?? ''} ${row.criterion}`));
            const count = ['instances', 'fraud_override'].includes(row.criterion);
            for (const key of ['instances', 'base', 'actual', 'observation', 'evidence']) {
                const td = document.createElement('td');
                const applicable = key === 'instances' ? count : key === 'base' ? !count && row.criterion !== 'profit' : key === 'actual' ? !count : true;
                if (applicable) {
                    const input = document.createElement('input'); input.className = 'form-control form-control-sm'; input.setAttribute('aria-label', `${row.item_code} ${key}`);
                    input.type = ['instances', 'base', 'actual'].includes(key) ? 'number' : 'text';
                    if (input.type === 'number') { input.step = key === 'instances' ? '1' : 'any'; input.required = true; if (row.criterion !== 'profit') input.min = '0'; }
                    else input.maxLength = key === 'evidence' ? 21 : 18;
                    input.value = row[key === 'base' ? 'base_value' : key === 'actual' ? 'actual_value' : key] ?? (key === 'instances' ? 0 : '');
                    row.inputs[key] = input; td.append(input);
                }
                tr.append(td);
            }
            el('Matrix').append(tr);
        });
        el('Results').replaceChildren();
        data.results.forEach(r => { const tr = document.createElement('tr'); [r.description, r.raw_marks, r.weight, r.denominator, r.capped_marks].forEach(v => tr.append(text('td', v))); el('Results').append(tr); });
        el('History').replaceChildren();
        data.history.forEach(h => {
            const details = document.createElement('details'); details.append(text('summary', `${h.recorded_on} | ${h.action} | User ${h.actor_id} | Revision ${h.revision}`));
            details.append(text('p', h.remarks));
            const pre = text('pre', `Inputs: ${h.input_snapshot || ''}\nResults: ${h.result_snapshot || ''}`); pre.style.whiteSpace = 'pre-wrap'; details.append(pre); el('History').append(details);
        });
        message('Assessment loaded.'); buttons();
    }
    el('Form').addEventListener('input', () => { dirty = true; buttons(); });
    el('Refresh').onclick = () => run(async () => {
        if (dirty && !confirm('Discard unsaved changes?')) return;
        await catalog();
        if (header?.assessment_id) await open(header.eng_id, header.assessment_id, header.version_id);
        else { mustReload = false; message('Assessment list refreshed. Open any saved assessment before retrying a creation.'); }
    });
    el('New').onclick = () => run(async () => {
        if (!el('Engagement').value || !el('Version').value) throw new Error('Select an engagement and published configuration.');
        if (!dirty || confirm('Discard unsaved changes?')) await open(el('Engagement').value, 0, el('Version').value);
    });
    el('Form').onsubmit = event => {
        event.preventDefault();
        run(async () => {
            const details = rows.map(row => {
                const result = { code: row.item_code };
                ['instances', 'base', 'actual', 'observation', 'evidence'].forEach(k => { const value = row.inputs[k]?.value.trim(); result[k] = !value ? null : ['instances', 'base', 'actual'].includes(k) ? Number(value) : value; });
                return result;
            });
            mustReload = true; // An interrupted response may still have committed; reload before another write.
            const data = await api('save', { engagementId: header.eng_id, assessmentId: header.assessment_id || 0, versionId: header.version_id,
                revision: header.revision || 0, branchSize: el('Size').value, portfolio: Number(el('Portfolio').value), sizeReason: el('Reason').value, details }, true);
            dirty = false;
            // Never blindly retry a write if the following refresh fails.
            header.assessment_id = data.assessmentId;
            await open(header.eng_id, data.assessmentId, header.version_id); await catalog(); message('Assessment saved and calculated.');
        });
    };
    for (const action of ['Submit', 'Approve', 'Return']) el(action).onclick = () => run(async () => {
        if (!confirm(`${action} this assessment?`)) return;
        mustReload = true;
        await api('transition', { engagementId: header.eng_id, assessmentId: header.assessment_id, revision: header.revision, action: action.toUpperCase(), remarks: el('Remarks').value }, true);
        await open(header.eng_id, header.assessment_id, header.version_id); await catalog(); message(`Assessment workflow updated.`);
    });
    run(catalog);
})();

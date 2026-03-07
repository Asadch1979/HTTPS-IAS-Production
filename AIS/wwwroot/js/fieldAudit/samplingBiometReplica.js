(function () {
    var host = document.getElementById('fieldAuditSamplingBiomet');
    if (!host) return;
    var engId = host.dataset.engId || '0';
    var sampleId = host.dataset.sampleId || '0';
    var loanStatus = host.dataset.loanStatus || '0';
    var sampleType = host.dataset.sampleType || '';
    function rowHtml(item, i) {
        return '<tr><td>' + (i + 1) + '</td><td>' + (item.accounT_NO || '') + '</td><td>' + (item.accounT_TITLE || '') + '</td><td>' + (item.customeR_NAME || '') + '</td><td>' + (item.dob_DISP || item.doB_DISP || item.dob || '') + '</td><td>' + (item.phonE_CELL || '') + '</td><td>' + (item.cnic || '') + '</td><td>' + (item.cniC_EXPIRY_DATE_DISP || item.cniC_EXPIRY_DATE || '') + '</td><td>' + (item.accounT_TYPE || '') + '</td><td>' + (item.accounT_CATEGORY || '') + '</td><td><button class="btn btn-primary btn-sm" data-doc="' + (item.accounT_NO || '') + '">Documents</button></td><td><button class="btn btn-success btn-sm" data-txn="' + (item.accounT_NO || '') + '">Transactions</button></td></tr>';
    }
    fetch((window.g_asiBaseURL || '') + '/ApiCalls/get_biomet_sampling_details', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }, credentials: 'same-origin', body: 'ENG_ID=' + encodeURIComponent(engId) })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            var b = document.querySelector('#fieldAuditBiometSampleList tbody');
            b.innerHTML = '';
            (data || []).forEach(function (x, i) { b.insertAdjacentHTML('beforeend', rowHtml(x, i)); });
            b.querySelectorAll('button[data-doc]').forEach(function (x) { x.onclick = function () { window.fieldAuditDashboard && window.fieldAuditDashboard.loadNestedView('SAMPLING_ACCOUNT_DOCUMENT', { acNo: this.dataset.doc, sampleId: sampleId, loanStatus: loanStatus, sampleType: sampleType }); }; });
            b.querySelectorAll('button[data-txn]').forEach(function (x) { x.onclick = function () { window.fieldAuditDashboard && window.fieldAuditDashboard.loadNestedView('SAMPLING_ACCOUNT_TRANSACTION', { acNo: this.dataset.txn, sampleId: sampleId, loanStatus: loanStatus, sampleType: sampleType }); }; });
            if (typeof initializeDataTable === 'function') initializeDataTable('fieldAuditBiometSampleList');
        });
})();

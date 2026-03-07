(function () {
    var host = document.getElementById('fieldAuditSamplingLoans');
    if (!host) return;
    var engId = host.dataset.engId || '0';
    var sampleId = host.dataset.sampleId || '0';
    var loanStatus = host.dataset.loanStatus || '0';
    var sampleType = host.dataset.sampleType || '';
    var label = document.getElementById('fieldAuditLoanSampleType'); if (label) label.textContent = sampleType;
    function fmt(a) { if (isNaN(a) || a == null) return '₨0.00'; return parseFloat(a).toLocaleString('en-PK', { style: 'currency', currency: 'PKR' }); }
    fetch((window.g_asiBaseURL || '') + '/ApiCalls/get_loan_samples', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }, credentials: 'same-origin', body: 'ENG_ID=' + encodeURIComponent(engId) + '&INDICATOR=L&STATUS_ID=' + encodeURIComponent(loanStatus) + '&SAMPLE_ID=' + encodeURIComponent(sampleId) })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            var b = document.querySelector('#fieldAuditLoanSampleList tbody'); b.innerHTML = '';
            (data || []).forEach(function (item, i) {
                b.insertAdjacentHTML('beforeend', '<tr><td>' + (i + 1) + '</td><td>' + (item.type || '') + '</td><td>' + (item.scheme || '') + '</td><td>' + (item.l_PURPOSE || '') + '</td><td>' + (item.lC_NO || '') + '</td><td>' + (item.cnic || '') + '</td><td>' + (item.customername || '') + '</td><td>' + (item.apP_DATE_DISP || item.app_DATE_DISP || '') + '</td><td>' + (item.disB_DATE_DISP || item.disb_DATE_DISP || '') + '</td><td>' + fmt(item.deV_AMOUNT) + '</td><td>' + fmt(item.outstanding) + '</td><td><button class="btn btn-link p-0" data-tr="' + (item.loaN_DISB_ID || '') + '">View Transactions</button></td><td><button class="btn btn-link p-0" data-doc="' + (item.loaN_DISB_ID || '') + '">View Documents</button></td></tr>');
            });
            b.querySelectorAll('button[data-tr]').forEach(function (x) { x.onclick = function () { window.fieldAuditDashboard && window.fieldAuditDashboard.loadNestedView('SAMPLING_LOAN_TRANSACTION', { disbId: this.dataset.tr, sampleId: sampleId, loanStatus: loanStatus, sampleType: sampleType }); }; });
            b.querySelectorAll('button[data-doc]').forEach(function (x) { x.onclick = function () { window.fieldAuditDashboard && window.fieldAuditDashboard.loadNestedView('SAMPLING_LOAN_DOCUMENT', { disbId: this.dataset.doc, sampleId: sampleId, loanStatus: loanStatus, sampleType: sampleType }); }; });
            if (typeof initializeDataTable === 'function') initializeDataTable('fieldAuditLoanSampleList');
        });
})();

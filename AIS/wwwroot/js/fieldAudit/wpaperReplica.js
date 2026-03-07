(function () {
    var host = document.getElementById('fieldAuditWPaperReplica');
    if (!host) {
        return;
    }

    var engId = parseInt(host.getAttribute('data-eng-id') || document.getElementById('fieldAuditWPaperEngId')?.value || '0', 10);
    var tableBody = document.querySelector('#fieldAuditWPaperTable tbody');
    var openButton = document.getElementById('openLoanCaseFileBtn');
    var addButton = document.getElementById('addLoanCaseFileBtn');
    var voucherLink = document.getElementById('voucherCheckingLink');
    var modalElement = document.getElementById('newLoanCaseFileDialog');

    if (!engId || engId <= 0 || !tableBody) {
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center">A valid engagement is required.</td></tr>';
        }
        return;
    }

    function resetTable() {
        tableBody.innerHTML = '';
    }

    function showNoData() {
        tableBody.innerHTML = '<tr><td colspan="7" class="text-center">No data found.</td></tr>';
    }

    function getModalInstance() {
        if (!modalElement || !window.bootstrap || !window.bootstrap.Modal) {
            return null;
        }

        return window.bootstrap.Modal.getOrCreateInstance(modalElement);
    }

    function getLoanCaseFiles() {
        resetTable();

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Get_Working_Paper_Loan_Cases', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: 'ENGID=' + encodeURIComponent(engId),
            credentials: 'same-origin'
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data || !data.length) {
                    showNoData();
                    return;
                }

                data.forEach(function (item, index) {
                    var row = document.createElement('tr');
                    row.innerHTML = '<td>' + (index + 1) + '</td>'
                        + '<td>' + (item.lC_NUMBER || '') + '</td>'
                        + '<td>' + (item.amount || '') + '</td>'
                        + '<td>' + (item.disB_DATE || '') + '</td>'
                        + '<td>' + (item.category || '') + '</td>'
                        + '<td>' + (item.observation || '') + '</td>'
                        + '<td>' + (item.parA_NO || '') + '</td>';
                    tableBody.appendChild(row);
                });
            })
            .catch(showNoData);
    }

    function addNewLoanCaseFile() {
        var loanCaseNumber = (document.getElementById('loancasenumber_txtField') || {}).value || '';
        var amount = (document.getElementById('osamount_txtField') || {}).value || '';
        var disbursementDate = (document.getElementById('dateofdisb_txtField') || {}).value || '';
        var category = (document.getElementById('lccategory_selectField') || {}).value || '';
        var observation = (document.getElementById('ob_txtField') || {}).value || '';
        var paraNo = (document.getElementById('pno_txtField') || {}).value || '';

        if (!loanCaseNumber) {
            alert('Provide LC Number to proceed');
            return;
        }

        if (!amount) {
            alert('Provide LC Amount to proceed');
            return;
        }

        if (!disbursementDate) {
            alert('Provide Disb Date to proceed');
            return;
        }

        if (!category) {
            alert('Provide Loan Category to proceed');
            return;
        }

        var payload = new URLSearchParams();
        payload.append('ENGID', engId.toString());
        payload.append('LCNUMBER', loanCaseNumber);
        payload.append('LCAMOUNT', amount);
        payload.append('DISBDATE', disbursementDate);
        payload.append('LCAT', category);
        payload.append('OBS', observation);
        payload.append('PARA_NO', paraNo);

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Add_Working_Paper_Loan_Cases', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: payload.toString(),
            credentials: 'same-origin'
        })
            .then(function (res) { return res.text(); })
            .then(function (data) {
                if (window.showApiAlert) {
                    showApiAlert(data);
                }

                var modal = getModalInstance();
                if (modal) {
                    modal.hide();
                }

                getLoanCaseFiles();
            })
            .catch(function () {
                if (window.showApiAlert) {
                    showApiAlert('{"Status":false,"Message":"Unable to save working paper record."}');
                }
            });
    }

    if (openButton) {
        openButton.addEventListener('click', function () {
            var modal = getModalInstance();
            if (modal) {
                modal.show();
            }
        });
    }

    if (addButton) {
        addButton.addEventListener('click', addNewLoanCaseFile);
    }

    if (voucherLink) {
        voucherLink.addEventListener('click', function () {
            if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
                window.fieldAuditDashboard.loadNestedView('WP_VOUCHER');
            }
        });
    }

    getLoanCaseFiles();
})();

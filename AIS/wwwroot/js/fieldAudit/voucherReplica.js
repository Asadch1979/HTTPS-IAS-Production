(function () {
    var host = document.getElementById('fieldAuditVoucherReplica');
    if (!host) {
        return;
    }

    var engId = host.dataset.engId || '0';
    var tableId = 'fieldAuditVoucherTable';
    var tableBody = document.querySelector('#' + tableId + ' tbody');
    var modalElement = document.getElementById('fieldAuditVoucherCheckingDialog');
    var openButton = document.getElementById('openVoucherCheckingReplicaBtn');
    var addButton = document.getElementById('addVoucherCheckingReplicaBtn');
    var form = document.getElementById('fieldAuditVoucherForm');

    function destroyDataTable() {
        if (window.$ && $.fn && $.fn.DataTable && $.fn.DataTable.isDataTable('#' + tableId)) {
            $('#' + tableId).DataTable().clear().destroy();
        }
    }

    function getModalInstance() {
        if (!modalElement || !window.bootstrap || !window.bootstrap.Modal) {
            return null;
        }

        return window.bootstrap.Modal.getOrCreateInstance(modalElement);
    }

    function renderNoData(message) {
        if (!tableBody) {
            return;
        }

        destroyDataTable();
        tableBody.innerHTML = '<tr><td colspan="4" class="text-center">' + message + '</td></tr>';
    }

    function renderRows(data) {
        if (!tableBody) {
            return;
        }

        destroyDataTable();
        tableBody.innerHTML = '';

        if (!data || !data.length) {
            renderNoData('No data found.');
            return;
        }

        (data || []).forEach(function (item, index) {
            tableBody.insertAdjacentHTML('beforeend',
                '<tr><td>' + (index + 1) + '</td><td>' + (item.v_NUMBER || '') + '</td><td>' + (item.observation || '') + '</td><td>' + (item.parA_NO || '') + '</td></tr>');
        });

        if (typeof initializeDataTable === 'function') {
            initializeDataTable(tableId);
        }
    }

    function refreshCurrentView() {
        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
            window.fieldAuditDashboard.loadNestedView('WP_VOUCHER');
            return;
        }

        loadVoucherChecking();
    }

    function loadVoucherChecking() {
        if (!engId || engId === '0') {
            renderNoData('A valid engagement is required.');
            return;
        }

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Get_Working_Paper_Voucher_Checking', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: 'ENGID=' + encodeURIComponent(engId),
            credentials: 'same-origin'
        })
            .then(function (response) { return response.json(); })
            .then(renderRows)
            .catch(function () { renderNoData('No data found.'); });
    }

    function resetForm() {
        if (form) {
            form.reset();
        }
    }

    function addVoucherChecking() {
        var voucherNumber = (document.getElementById('fieldAuditVoucherNumber') || {}).value || '';
        var observation = (document.getElementById('fieldAuditVoucherObservation') || {}).value || '';
        var paraNo = (document.getElementById('fieldAuditVoucherParaNo') || {}).value || '';

        if (!voucherNumber) {
            alert('Provide Voucher Number to proceed');
            return;
        }

        var payload = new URLSearchParams();
        payload.append('ENGID', engId);
        payload.append('VNUMBER', voucherNumber);
        payload.append('OBS', observation);
        payload.append('PARA_NO', paraNo);

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Add_Working_Paper_Voucher_Checking', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: payload.toString(),
            credentials: 'same-origin'
        })
            .then(function (response) { return response.text(); })
            .then(function (data) {
                if (window.showApiAlert) {
                    showApiAlert(data);
                }

                var modal = getModalInstance();
                if (modal) {
                    modal.hide();
                }

                resetForm();
                refreshCurrentView();
            })
            .catch(function () {
                if (window.showApiAlert) {
                    showApiAlert('{"Status":false,"Message":"Unable to save voucher checking record."}');
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
        addButton.addEventListener('click', addVoucherChecking);
    }

    loadVoucherChecking();
})();

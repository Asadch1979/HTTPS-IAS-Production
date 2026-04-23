(function () {
    var host = document.getElementById('fieldAuditAccountOpeningReplica');
    if (!host) {
        return;
    }

    var engId = host.dataset.engId || '0';
    var tableId = 'fieldAuditAccountOpeningTable';
    var tableBody = document.querySelector('#' + tableId + ' tbody');
    var modalElement = document.getElementById('fieldAuditAccountOpeningDialog');
    var openButton = document.getElementById('openAccountOpeningReplicaBtn');
    var addButton = document.getElementById('addAccountOpeningReplicaBtn');
    var form = document.getElementById('fieldAuditAccountOpeningForm');

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
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center">' + message + '</td></tr>';
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
                '<tr><td>' + (index + 1) + '</td><td>' + (item.v_NUMBER || '') + '</td><td>' + (item.a_NATURE || '') + '</td><td>' + (item.observation || '') + '</td><td>' + (item.parA_NO || '') + '</td></tr>');
        });

        if (typeof initializeDataTable === 'function') {
            initializeDataTable(tableId);
        }
    }

    function refreshCurrentView() {
        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
            window.fieldAuditDashboard.loadNestedView('WP_ACCOUNT_OPENING');
            return;
        }

        loadAccountOpenings();
    }

    function loadAccountOpenings() {
        if (!engId || engId === '0') {
            renderNoData('A valid engagement is required.');
            return;
        }

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Get_Working_Paper_Account_Opening', {
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

    function addAccountOpening() {
        var accountNumber = (document.getElementById('fieldAuditAccountNumber') || {}).value || '';
        var accountNature = (document.getElementById('fieldAuditAccountNature') || {}).value || '';
        var observation = (document.getElementById('fieldAuditAccountObservation') || {}).value || '';
        var paraNo = (document.getElementById('fieldAuditAccountParaNo') || {}).value || '';

        if (!accountNumber) {
            alert('Provide Voucher Number to proceed');
            return;
        }

        if (!accountNature) {
            alert('Provide Account Nature to proceed');
            return;
        }

        var payload = new URLSearchParams();
        payload.append('ENGID', engId);
        payload.append('VNUMBER', accountNumber);
        payload.append('ANATURE', accountNature);
        payload.append('OBS', observation);
        payload.append('PARA_NO', paraNo);

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Add_Working_Paper_Account_Opening', {
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
                    showApiAlert('{"Status":false,"Message":"Unable to save account opening record."}');
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
        addButton.addEventListener('click', addAccountOpening);
    }

    loadAccountOpenings();
})();

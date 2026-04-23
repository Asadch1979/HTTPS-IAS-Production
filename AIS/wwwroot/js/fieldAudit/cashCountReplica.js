(function () {
    var host = document.getElementById('fieldAuditCashCountReplica');
    if (!host) {
        return;
    }

    var engId = host.dataset.engId || '0';
    var tableId = 'fieldAuditCashCountTable';
    var tableBody = document.querySelector('#' + tableId + ' tbody');
    var modalElement = document.getElementById('fieldAuditCashCountDialog');
    var openButton = document.getElementById('openCashCountReplicaBtn');
    var addButton = document.getElementById('addCashCountReplicaBtn');
    var form = document.getElementById('fieldAuditCashCountForm');

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
        tableBody.innerHTML = '<tr><td colspan="8" class="text-center">' + message + '</td></tr>';
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
                '<tr><td>' + (index + 1) + '</td><td>' + (item.denominatioN_VAULT || '') + '</td><td>' + (item.nO_CURRENCY_NOTES_VAULT || '') + '</td><td>' + (item.totaL_AMOUNT_VAULT || '') + '</td><td>' + (item.denominatioN_SAFE_REGISTER || '') + '</td><td>' + (item.nO_CURRENCY_NOTES_SAFE_REGISTER || '') + '</td><td>' + (item.totaL_AMOUNT_SAFE_REGISTER || '') + '</td><td>' + (item.difference || '') + '</td></tr>');
        });

        if (typeof initializeDataTable === 'function') {
            initializeDataTable(tableId);
        }
    }

    function refreshCurrentView() {
        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
            window.fieldAuditDashboard.loadNestedView('WP_CASH_COUNT');
            return;
        }

        loadCashCount();
    }

    function loadCashCount() {
        if (!engId || engId === '0') {
            renderNoData('A valid engagement is required.');
            return;
        }

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Get_Working_Paper_Cash_Counter', {
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

    function addCashCount() {
        var vaultDenomination = (document.getElementById('fieldAuditVaultDenomination') || {}).value || '';
        var vaultNotes = (document.getElementById('fieldAuditVaultCurrencyNotes') || {}).value || '';
        var vaultTotal = (document.getElementById('fieldAuditVaultTotalAmount') || {}).value || '';
        var safeDenomination = (document.getElementById('fieldAuditSafeDenomination') || {}).value || '';
        var safeNotes = (document.getElementById('fieldAuditSafeCurrencyNotes') || {}).value || '';
        var safeTotal = (document.getElementById('fieldAuditSafeTotalAmount') || {}).value || '';
        var difference = (document.getElementById('fieldAuditCashCountDifference') || {}).value || '';

        if (!vaultDenomination) {
            alert('Provide Denomination as per Vault to proceed');
            return;
        }

        if (!vaultNotes) {
            alert('Provide Currency Notes as Per Vault to proceed');
            return;
        }

        if (!vaultTotal) {
            alert('Provide Total Amount as per Vault to proceed');
            return;
        }

        if (!safeDenomination) {
            alert('Provide Denomination as per Safe Register to proceed');
            return;
        }

        if (!safeNotes) {
            alert('Provide Currency Notes as Per Safe Register to proceed');
            return;
        }

        if (!safeTotal) {
            alert('Provide Total Amount as per Safe Register to proceed');
            return;
        }

        if (!difference) {
            alert('Provide Difference to proceed');
            return;
        }

        var payload = new URLSearchParams();
        payload.append('ENGID', engId);
        payload.append('DVAULT', vaultDenomination);
        payload.append('NOVAULT', vaultNotes);
        payload.append('TOTVAULT', vaultTotal);
        payload.append('DSR', safeDenomination);
        payload.append('NOSR', safeNotes);
        payload.append('TOTSR', safeTotal);
        payload.append('DIFF', difference);

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Add_Working_Paper_Cash_Counter', {
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
                    showApiAlert('{"Status":false,"Message":"Unable to save cash count record."}');
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
        addButton.addEventListener('click', addCashCount);
    }

    loadCashCount();
})();

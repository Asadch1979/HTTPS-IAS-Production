(function () {
    var host = document.getElementById('fieldAuditFixedAssetsReplica');
    if (!host) {
        return;
    }

    var engId = host.dataset.engId || '0';
    var tableId = 'fieldAuditFixedAssetsTable';
    var tableBody = document.querySelector('#' + tableId + ' tbody');
    var modalElement = document.getElementById('fieldAuditFixedAssetsDialog');
    var openButton = document.getElementById('openFixedAssetsReplicaBtn');
    var addButton = document.getElementById('addFixedAssetsReplicaBtn');
    var form = document.getElementById('fieldAuditFixedAssetsForm');

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
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center">' + message + '</td></tr>';
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
                '<tr><td>' + (index + 1) + '</td><td>' + (item.asseT_NAME || '') + '</td><td>' + (item.physicaL_EXISTANCE || '') + '</td><td>' + (item.locatioN_AS_PER_FAR || '') + '</td><td>' + (item.difference || '') + '</td><td>' + (item.remarks || '') + '</td></tr>');
        });

        if (typeof initializeDataTable === 'function') {
            initializeDataTable(tableId);
        }
    }

    function refreshCurrentView() {
        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
            window.fieldAuditDashboard.loadNestedView('WP_FIXED_ASSETS');
            return;
        }

        loadFixedAssets();
    }

    function loadFixedAssets() {
        if (!engId || engId === '0') {
            renderNoData('A valid engagement is required.');
            return;
        }

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Get_Working_Paper_Fixed_Assets', {
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

    function addFixedAssets() {
        var assetName = (document.getElementById('fieldAuditAssetName') || {}).value || '';
        var physicalExistence = (document.getElementById('fieldAuditAssetPhysical') || {}).value || '';
        var farLocation = (document.getElementById('fieldAuditAssetFar') || {}).value || '';
        var difference = (document.getElementById('fieldAuditAssetDifference') || {}).value || '';
        var remarks = (document.getElementById('fieldAuditAssetRemarks') || {}).value || '';

        if (!assetName) {
            alert('Provide Assets Name to proceed');
            return;
        }

        if (!physicalExistence) {
            alert('Provide Physical Existance to proceed');
            return;
        }

        if (!farLocation) {
            alert('Provide Location as per Fixed Asset Register to proceed');
            return;
        }

        if (!difference) {
            alert('Provide Difference to proceed');
            return;
        }

        if (!remarks) {
            alert('Provide Remarks to proceed');
            return;
        }

        var payload = new URLSearchParams();
        payload.append('ENGID', engId);
        payload.append('A_NAME', assetName);
        payload.append('PHY_EX', physicalExistence);
        payload.append('FAR', farLocation);
        payload.append('DIFF', difference);
        payload.append('REM', remarks);

        fetch((window.g_asiBaseURL || '') + '/ApiCalls/Add_Working_Paper_Fixed_Assets', {
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
                    showApiAlert('{"Status":false,"Message":"Unable to save fixed asset record."}');
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
        addButton.addEventListener('click', addFixedAssets);
    }

    loadFixedAssets();
})();

(function () {
    var host = document.getElementById('fieldAuditSamplesReplica');
    if (!host) {
        return;
    }

    var hiddenEngId = host.querySelector('.field-audit-eng-id');
    var engId = parseInt(host.getAttribute('data-eng-id') || (hiddenEngId ? hiddenEngId.value : '0') || '0', 10);
    var tableId = 'fieldAuditSamplesTable';

    if (!engId || engId <= 0) {
        var tableBody = document.querySelector('#' + tableId + ' tbody');
        if (tableBody) {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center">A valid engagement is required.</td></tr>';
        }
        return;
    }

    function renderRows(data) {
        if (typeof destroyDatatable === 'function') {
            destroyDatatable(tableId);
        }

        var tableBody = document.querySelector('#' + tableId + ' tbody');
        if (!tableBody) {
            return;
        }

        tableBody.innerHTML = '';

        if (!data || !data.length) {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center">No data found.</td></tr>';
            return;
        }

        data.forEach(function (item, index) {
            var row = document.createElement('tr');
            row.innerHTML = '<td>' + (index + 1) + '</td>'
                + '<td>' + (item.samplE_TYPE || '') + '</td>'
                + '<td class="text-center">' + (item.samplE_PERCENTAGE || 0) + '%</td>'
                + '<td class="text-center">' + (item.totaL_COUNT || 0) + '</td>'
                + '<td class="text-center">' + (item.samplE_COUNT || 0) + '</td>'
                + '<td class="text-center"><button class="btn btn-danger btn-sm field-audit-sample-view">View</button></td>';

            var btn = row.querySelector('.field-audit-sample-view');
            if (btn) {
                btn.addEventListener('click', function () {
                    var indicator = item.samplE_INDICATOR;
                    var loanStatus = item.loaN_STATUS;
                    var sampleType = encodeURIComponent(item.samplE_TYPE || '');
                    if (indicator === 'A') {
                        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
                            window.fieldAuditDashboard.loadNestedView('SAMPLING_BIOMET', { sampleId: item.samplE_ID, loanStatus: loanStatus, sampleType: item.samplE_TYPE || '' });
                        }
                    } else if (indicator === 'L') {
                        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
                            window.fieldAuditDashboard.loadNestedView('SAMPLING_LOANS', { sampleId: item.samplE_ID, loanStatus: loanStatus, sampleType: item.samplE_TYPE || '' });
                        }
                    }
                });
            }

            tableBody.appendChild(row);
        });

        if (window.jQuery && $.fn && $.fn.DataTable) {
            if ($.fn.DataTable.isDataTable('#' + tableId)) {
                $('#' + tableId).DataTable().clear().destroy();
            }

            $('#' + tableId).DataTable({
                dom: '<"top">rt<"bottom"ip><"clear">',
                autoWidth: true,
                ordering: false,
                searching: false,
                lengthChange: false,
                lengthMenu: [
                    [10, 50, 100, -1],
                    [10, 50, 100, 'All']
                ]
            });
        }
    }

    fetch((window.g_asiBaseURL || '') + '/ApiCalls/get_list_of_samples', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
        body: 'ENG_ID=' + encodeURIComponent(engId),
        credentials: 'same-origin'
    })
        .then(function (res) { return res.json(); })
        .then(renderRows)
        .catch(function () { renderRows([]); });
})();

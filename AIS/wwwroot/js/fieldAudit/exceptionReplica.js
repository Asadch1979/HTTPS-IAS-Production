(function () {
    var host = document.getElementById('fieldAuditExceptionReplica');
    if (!host) {
        return;
    }

    var hiddenEngId = host.querySelector('.field-audit-eng-id');
    var engId = parseInt(host.getAttribute('data-eng-id') || (hiddenEngId ? hiddenEngId.value : '0') || '0', 10);
    var tableId = 'fieldAuditExceptionTable';

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
            var reportingPeriod = item.reportingPeriod || item.ReportingPeriod || item.REPORTING_PERIOD || '';
            var exceptionCount = item.exceptionCount || item.ExceptionCount || item.EXCEPTION_COUNT || item.EXC_COUNT || 0;

            var row = document.createElement('tr');
            row.innerHTML = '<td>' + (index + 1) + '</td>'
                + '<td>' + (item.reporT_TITLE || '') + '</td>'
                + '<td class="text-center">' + (item.discription || '') + '</td>'
                + '<td>' + reportingPeriod + '</td>'
                + '<td class="text-center">' + exceptionCount + '</td>'
                + '<td class="text-center"><button class="btn btn-danger btn-sm field-audit-report-view">View</button></td>';

            var btn = row.querySelector('.field-audit-report-view');
            if (btn) {
                btn.addEventListener('click', function () {
                    var indicator = item.reporT_INDICATOR;
                    var loanStatus = item.loaN_STATUS;
                    var title = encodeURIComponent(item.reporT_TITLE || '');
                    var desc = encodeURIComponent(item.discription || '');

                    if (indicator === 'A') {
                        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
                            window.fieldAuditDashboard.loadNestedView('EXCEPTION_ACCOUNT', { reportId: item.reporT_ID, loanStatus: loanStatus, title: item.reporT_TITLE || '', desc: item.discription || '' });
                        }
                    } else if (indicator === 'L') {
                        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.loadNestedView === 'function') {
                            window.fieldAuditDashboard.loadNestedView('EXCEPTION_LOAN', { reportId: item.reporT_ID, loanStatus: loanStatus, title: item.reporT_TITLE || '', desc: item.discription || '' });
                        }
                    }
                });
            }

            tableBody.appendChild(row);
        });

        if (typeof initializeDataTable === 'function') {
            initializeDataTable(tableId);
        }
    }

    fetch((window.g_asiBaseURL || '') + '/ApiCalls/get_list_of_reports', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
        body: 'ENG_ID=' + encodeURIComponent(engId),
        credentials: 'same-origin'
    })
        .then(function (res) { return res.json(); })
        .then(renderRows)
        .catch(function () { renderRows([]); });
})();

(function () {
    'use strict';

    var page = document.getElementById('exceptionMonitoringPage');
    if (!page) {
        return;
    }

    var defaultEngagementId = parseInt(page.getAttribute('data-default-engagement-id'), 10) || 0;
    var g_engID = 0;

    $(document).ready(function () {
        var url = new URL(window.location);
        g_engID = parseInt(url.searchParams.get('engId'), 10) || defaultEngagementId;

        loadExceptionEntities();
        $('#entitySelectField').on('change', loadExceptionDetails);

        $(document).on('click', '.btn-view-exc', handleViewException);
        $(document).on('click', '.btn-regen-exc', handleRegenerateException);
    });

    function loadExceptionEntities() {
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_exception_monitor_entities',
            type: 'GET',
            cache: false,
            success: function (data) {
                var dropdown = $('#entitySelectField');
                dropdown.empty();
                dropdown.append('<option value="0" id="0" selected>--Select Entity Name--</option>');

                if (data && data.length) {
                    data.forEach(function (item) {
                        var engId = parseInt(getValue(item, ['engId', 'ENG_ID', 'eng_id']), 10) || 0;
                        var entName = getValue(item, ['entName', 'ENT_NAME', 'ent_name']);
                        dropdown.append('<option id="' + engId + '" value="' + engId + '">' + (entName || '') + '</option>');
                    });
                }

                if (g_engID > 0) {
                    dropdown.val(g_engID);
                }

                if (dropdown.val() && dropdown.val() !== '0') {
                    loadExceptionDetails();
                }
            },
            error: function (xhr) {
                showToast(xhr.responseText || 'Unable to load entities.', false);
            }
        });
    }

    function loadExceptionDetails() {
        var engId = $('#entitySelectField').val();
        g_engID = parseInt(engId, 10) || g_engID;
        var tableBody = $('#exceptionDetailsTable tbody');
        if (typeof destroyDatatable === 'function') {
            destroyDatatable('exceptionDetailsTable');
        }
        tableBody.empty();

        if (!engId || engId === '0') {
            tableBody.append('<tr><td colspan="7" class="text-center">Please select an entity to view exception data.</td></tr>');
            return;
        }

        $('#wait').show();
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_exception_monitor_details',
            type: 'GET',
            cache: false,
            data: {
                eng_id: engId
            },
            success: function (data) {
                if (data && data.length) {
                    data.forEach(function (item, index) {
                        tableBody.append(buildExceptionRow(item, index));
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No exception data found for this entity.</td></tr>');
                }

                if (typeof initializeDataTable === 'function') {
                    initializeDataTable('exceptionDetailsTable');
                }
            },
            error: function (xhr) {
                showToast(xhr.responseText || 'An error occurred while fetching exception details.', false);
            },
            complete: function () {
                $('#wait').hide();
            }
        });
    }

    function buildExceptionRow(item, index) {
        var erId = parseInt(getValue(item, ['erId', 'ER_ID', 'er_id']), 10) || 0;
        var loanStatus = parseInt(getValue(item, ['loanStatus', 'LOAN_STATUS', 'loan_status']), 10) || 0;
        var indicator = getValue(item, ['reportIndicator', 'ReportIndicator', 'REPORT_INDICATOR', 'reportType', 'ReportType', 'REPORT_TYPE', 'indicator', 'Indicator', 'INDICATOR', 'ind', 'IND']) || '';

        var executionDates = getValue(item, ['executionDates', 'ExecutionDates', 'EXECUTION_DATES', 'execution_dates']) || '';
        var reportingPeriod = getValue(item, ['reportingPeriod', 'ReportingPeriod', 'REPORTING_PERIOD', 'reporting_period']) || '';

        var reportTitle = getValue(item, ['reportTitle', 'REPORT_TITLE', 'report_title']) || '';
        var reportDescription = getValue(item, ['description', 'reportDescription', 'REPORT_DESCRIPTION', 'report_description', 'REPORT_DESC']) || '';
        var exceptionCount = parseInt(getValue(item, ['exceptionCount', 'EXC_COUNT', 'excCount']), 10) || 0;

        return '<tr>'
            + '<td>' + (index + 1) + '</td>'
            + '<td>' + htmlEncode(reportTitle) + '</td>'
            + '<td>' + htmlEncode(executionDates) + '</td>'
            + '<td>' + htmlEncode(reportingPeriod) + '</td>'
            + '<td class="text-center">' + exceptionCount + '</td>'
            + '<td class="text-center">'
            + '    <button type="button" class="btn btn-danger btn-sm btn-view-exc"'
            + '        data-er-id="' + erId + '"'
            + '        data-loan-status="' + loanStatus + '"'
            + '        data-indicator="' + (indicator || '').toString() + '"'
            + '        data-title="' + htmlEncode(reportTitle) + '"'
            + '        data-desc="' + htmlEncode(reportDescription) + '">'
            + '        View'
            + '    </button>'
            + '</td>'
            + '<td class="text-center">'
            + '    <button type="button" class="btn btn-danger btn-sm btn-regen-exc" data-er-id="' + erId + '">Regenerate</button>'
            + '</td>'
            + '</tr>';
    }

    function getValue(item, keys) {
        for (var i = 0; i < keys.length; i++) {
            if (item[keys[i]] !== undefined && item[keys[i]] !== null) {
                return item[keys[i]];
            }
        }
        return '';
    }

    function normalizeIndicator(indicator) {
        var normalized = (indicator || '').toString().trim().toUpperCase();
        if (normalized === 'L') {
            return 'L';
        }
        return 'A';
    }

    function htmlEncode(value) {
        return $('<div/>').text(value == null ? '' : value).html();
    }

    function regenerateException(engId, erId, btn) {
        var $button = $(btn);
        var originalHtml = $button.html();
        $button.prop('disabled', true);
        $button.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');

        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/regenerate_exception',
            type: 'POST',
            cache: false,
            data: {
                eng_id: engId,
                er_id: erId
            },
            success: function (data) {
                if (data && (data.success || data.Status)) {
                    showToast('Exception regenerated successfully', true);
                    loadExceptionDetails();
                } else {
                    var message = extractApiMessage(data, 'Failed to regenerate exception');
                    showToast(message, false);
                }
            },
            error: function (xhr) {
                showToast(extractApiMessageFromXhr(xhr, 'An error occurred while regenerating the exception'), false);
            },
            complete: function () {
                $button.prop('disabled', false);
                $button.html(originalHtml);
            }
        });
    }

    function showToast(message, isSuccess) {
        var toastContainer = $('#exception-toast-container');
        var alertClass = isSuccess ? 'alert-success' : 'alert-danger';
        var toast = $('<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">'
            + message
            + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        toastContainer.append(toast);
        setTimeout(function () {
            toast.alert('close');
        }, 4000);
    }

    function handleViewException() {
        var $btn = $(this);
        var erId = parseInt($btn.data('er-id'), 10) || 0;
        var loanStatus = parseInt($btn.data('loan-status'), 10) || 0;
        var indicator = normalizeIndicator($btn.data('indicator'));
        var reportTitle = $btn.data('title') || '';
        var reportDescription = $btn.data('desc') || '';

        if (!erId) {
            showToast('Invalid report selection.', false);
            return;
        }

        if (indicator === 'L') {
            redirectToLoan(erId, loanStatus, reportTitle, reportDescription);
        } else {
            redirectToAccount(erId, loanStatus, reportTitle, reportDescription);
        }
    }

    function handleRegenerateException() {
        var $btn = $(this);
        var erId = parseInt($btn.data('er-id'), 10) || 0;
        if (!erId || !g_engID) {
            showToast('Please select a valid entity before regenerating.', false);
            return;
        }

        regenerateException(g_engID, erId, $btn);
    }

    function redirectToAccount(reportId, loanStatus, title, desc) {
        window.location.href = g_asiBaseURL
            + '/Sampling/Account_exception?engId=' + g_engID
            + '&report_id=' + reportId
            + '&loan_status=' + loanStatus
            + '&title=' + encodeURIComponent(title)
            + '&desc=' + encodeURIComponent(desc);
    }

    function redirectToLoan(reportId, loanStatus, title, desc) {
        window.location.href = g_asiBaseURL
            + '/Sampling/loans_exception?engId=' + g_engID
            + '&reporT_ID=' + reportId
            + '&loaN_STATUS=' + loanStatus
            + '&title=' + encodeURIComponent(title)
            + '&desc=' + encodeURIComponent(desc);
    }
})();

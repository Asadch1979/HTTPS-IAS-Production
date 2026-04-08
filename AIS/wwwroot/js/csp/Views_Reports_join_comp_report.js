$(document).ready(function () {
    if ($.fn.select2) {
        $('#auditDepartmentSelectBox').select2();
    }
});

function getJoiningCompletionValue(row, keys, defaultValue) {
    var fallback = defaultValue === undefined ? '' : defaultValue;

    if (!row) {
        return fallback;
    }

    for (var i = 0; i < keys.length; i++) {
        var key = keys[i];
        if (Object.prototype.hasOwnProperty.call(row, key) && row[key] !== null && row[key] !== undefined) {
            return row[key];
        }
    }

    return fallback;
}

function formatJoiningCompletionDate(value) {
    if (value === null || value === undefined) {
        return '';
    }

    var rawValue = String(value).trim();
    if (rawValue.length === 0) {
        return '';
    }

    var datePart = rawValue;
    if (datePart.indexOf('T') !== -1) {
        datePart = datePart.split('T')[0];
    } else if (datePart.indexOf(' ') !== -1) {
        datePart = datePart.split(' ')[0];
    }

    var dateMatch = datePart.match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (!dateMatch) {
        return rawValue;
    }

    return dateMatch[3] + '-' + dateMatch[2] + '-' + dateMatch[1].substring(2, 4);
}

function getJoiningCompletionExportOptions() {
    return $.extend(true, {}, getSafeExportFormatOptions(), {
        columns: function (idx, data, node) {
            return !$(node).hasClass('hide-export');
        }
    });
}

function buildJoiningCompletionPdfButton() {
    return $.extend(true, {}, getPdfExportButtonConfig(), {
        text: 'Export to PDF',
        className: 'btn btn-danger',
        exportOptions: getJoiningCompletionExportOptions()
    });
}

function buildJoiningCompletionExcelButton() {
    return $.extend(true, {}, getExcelExportButtonConfig('Export to Excel'), {
        className: 'btn btn-success',
        exportOptions: getJoiningCompletionExportOptions()
    });
}

function buildJoiningCompletionCsvButton() {
    return $.extend(true, {}, getCsvExportButtonConfig('Export to CSV'), {
        className: 'btn btn-primary',
        exportOptions: getJoiningCompletionExportOptions()
    });
}

function initializeJoiningCompletionGrid() {
    if ($.fn.DataTable.isDataTable('#JoiningCompletionGrid')) {
        $('#JoiningCompletionGrid').DataTable().clear().destroy();
    }

    $('#JoiningCompletionGrid').DataTable({
        dom: '<"top"lfB>rt<"bottom"ip><"clear">',
        autoWidth: true,
        ordering: false,
        buttons: [
            buildJoiningCompletionPdfButton(),
            buildJoiningCompletionExcelButton(),
            buildJoiningCompletionCsvButton()
        ],
        lengthMenu: [
            [10, 50, 100, -1],
            [10, 50, 100, 'All']
        ]
    });
}

function appendJoiningCompletionRow(index, row) {
    var auditBy = getJoiningCompletionValue(row, ['AUDIT_BY', 'audiT_BY', 'audit_BY']);
    var reportingOffice = getJoiningCompletionValue(row, ['Reporting', 'reporting']);
    var auditeeCode = getJoiningCompletionValue(row, ['CODE', 'code']);
    var auditeeName = getJoiningCompletionValue(row, ['AUDITEE_NAME', 'auditeE_NAME', 'auditee_NAME']);
    var risk = getJoiningCompletionValue(row, ['Risk', 'risk']);
    var startDate = formatJoiningCompletionDate(getJoiningCompletionValue(row, ['START_DATE', 'starT_DATE', 'start_DATE']));
    var endDate = formatJoiningCompletionDate(getJoiningCompletionValue(row, ['END_DATE', 'enD_DATE', 'end_DATE']));
    var status = getJoiningCompletionValue(row, ['STATUS', 'status']);
    var issuanceDate = formatJoiningCompletionDate(getJoiningCompletionValue(row, ['Issuancedate', 'issuancedate']));

    var $tableRow = $('<tr/>');
    $tableRow.append($('<td/>', { text: index, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: auditBy, 'class': 'text-start' }));
    $tableRow.append($('<td/>', { text: reportingOffice, 'class': 'text-start' }));
    $tableRow.append($('<td/>', { text: auditeeCode, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: auditeeName, 'class': 'text-start' }));
    $tableRow.append($('<td/>', { text: risk, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: startDate, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: endDate, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: status, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: issuanceDate, 'class': 'text-center' }));

    $('#JoiningCompletionGrid tbody').append($tableRow);
}

function FindJoiningCompletionData() {
    var departmentId = $('#auditDepartmentSelectBox').val();
    var auditStartDate = $('#auditStartDateField').val();
    var auditEndDate = $('#auditEndDateField').val();

    if (auditStartDate === '') {
        alert('Select Audit Plan Start Date to proceed');
        return;
    }

    if (auditEndDate === '') {
        alert('Select Audit Plan End Date to proceed');
        return;
    }

    if ($.fn.DataTable.isDataTable('#JoiningCompletionGrid')) {
        $('#JoiningCompletionGrid').DataTable().clear().destroy();
    }

    $('#JoiningCompletionGrid tbody').empty();

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/get_joining_completion',
        type: 'POST',
        data: {
            DEPT_ID: departmentId,
            AUDIT_STARTDATE: auditStartDate,
            AUDIT_ENDDATE: auditEndDate
        },
        cache: false,
        success: function (data) {
            $.each(data || [], function (index, row) {
                appendJoiningCompletionRow(index + 1, row);
            });

            initializeJoiningCompletionGrid();
        },
        error: function () {
            initializeJoiningCompletionGrid();
            alert('Unable to load the joining/completion report right now. Please try again.');
        },
        dataType: 'json'
    });
}

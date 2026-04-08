$(document).ready(function () {
    if ($.fn.select2) {
        $('#auditDepartmentSelectBox').select2();
    }

    resetJoiningCompletionFooter();
});

function resetJoiningCompletionFooter() {
    $('#JOINING_COMPLETION_TOTAL_HIGH').text('0');
    $('#JOINING_COMPLETION_TOTAL_MEDIUM').text('0');
    $('#JOINING_COMPLETION_TOTAL_LOW').text('0');
}

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

function toJoiningCompletionNumber(value) {
    var numeric = parseInt(value, 10);
    if (isNaN(numeric)) {
        return 0;
    }

    return numeric;
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
        footer: true,
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
    var auditeeName = getJoiningCompletionValue(row, ['AUDITEE_NAME', 'auditeE_NAME', 'auditee_NAME']);
    var risk = getJoiningCompletionValue(row, ['Risk', 'risk']);
    var joiningDate = formatJoiningCompletionDate(getJoiningCompletionValue(row, ['JOINING_DATE', 'joininG_DATE', 'joining_DATE']));
    var completionDate = formatJoiningCompletionDate(getJoiningCompletionValue(row, ['COMPLETION_DATE', 'completioN_DATE', 'completion_DATE']));
    var status = getJoiningCompletionValue(row, ['STATUS', 'status']);
    var issuanceDate = formatJoiningCompletionDate(getJoiningCompletionValue(row, ['Issuancedate', 'issuancedate']));
    var high = toJoiningCompletionNumber(getJoiningCompletionValue(row, ['High', 'high'], 0));
    var medium = toJoiningCompletionNumber(getJoiningCompletionValue(row, ['Medium', 'medium'], 0));
    var low = toJoiningCompletionNumber(getJoiningCompletionValue(row, ['Low', 'low'], 0));

    var $tableRow = $('<tr/>');
    $tableRow.append($('<td/>', { text: index, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: auditBy, 'class': 'text-start' }));
    $tableRow.append($('<td/>', { text: auditeeName, 'class': 'text-start' }));
    $tableRow.append($('<td/>', { text: risk, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: joiningDate, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: completionDate, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: status, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: issuanceDate, 'class': 'text-center' }));
    $tableRow.append($('<td/>', { text: high, 'class': 'text-end' }));
    $tableRow.append($('<td/>', { text: medium, 'class': 'text-end' }));
    $tableRow.append($('<td/>', { text: low, 'class': 'text-end' }));

    $('#JoiningCompletionGrid tbody').append($tableRow);

    return {
        high: high,
        medium: medium,
        low: low
    };
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
    resetJoiningCompletionFooter();

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
            var totalHigh = 0;
            var totalMedium = 0;
            var totalLow = 0;

            $.each(data || [], function (index, row) {
                var rowTotals = appendJoiningCompletionRow(index + 1, row);
                totalHigh += rowTotals.high;
                totalMedium += rowTotals.medium;
                totalLow += rowTotals.low;
            });

            $('#JOINING_COMPLETION_TOTAL_HIGH').text(totalHigh);
            $('#JOINING_COMPLETION_TOTAL_MEDIUM').text(totalMedium);
            $('#JOINING_COMPLETION_TOTAL_LOW').text(totalLow);

            initializeJoiningCompletionGrid();
        },
        error: function () {
            resetJoiningCompletionFooter();
            initializeJoiningCompletionGrid();
            alert('Unable to load the joining/completion report right now. Please try again.');
        },
        dataType: 'json'
    });
}

    $(document).ready(function () {
        initSystemLogDatePickers();
        $('#loadSystemLogs').on('click', function () {
            fetchSystemLogs();
        });

        $('#deleteOldLogs').on('click', function () {
            deleteOldSystemLogs();
        });
    });

    function initSystemLogDatePickers() {
        if (typeof flatpickr === 'undefined') {
            return;
        }

        $('.system-log-datetime').flatpickr({
            enableTime: true,
            dateFormat: 'Y-m-d h:i K',
            time_24hr: false,
            allowInput: true
        });
    }

    function fetchSystemLogs() {
        $('#systemLogsMessage').text('');

        $.ajax({
            url: g_asiBaseURL + "/AdministrationPanel/GetSystemLogs",

            type: "POST",
            dataType: "json",
            data: {
                // Controller expects: start, end (NOT startTime/endTime)
                start: $('#logStartTime').val(),
                end: $('#logEndTime').val(),
                logLevel: $('#logLevel').val(),
                module: $('#logModule').val(),
                userPpno: $('#logUserPpno').val(),
                engId: $('#logEngId').val()
            },
            cache: false,
            success: function (response) {
                if (!response || response.status !== true) {
                    $('#systemLogsMessage').text('Unable to load system logs.');
                    renderSystemLogs([]);
                    return;
                }

                // response.data is already the array
                renderSystemLogs(response.data || []);
            },
            error: function (xhr) {
                var statusMessage = xhr && xhr.status ? (' (HTTP ' + xhr.status + ')') : '';
                $('#systemLogsMessage').text('Unable to load system logs.' + statusMessage);
                renderSystemLogs([]);
            }
        });
    }


    function deleteOldSystemLogs() {
        $('#systemLogsDeleteMessage').removeClass('text-success text-danger').text('');

        var cutoffValue = $('#deleteLogCutoffTime').val();
        var daysValue = $('#deleteLogDays').val();
        var cutoffTime = '';

        if (daysValue) {
            var parsedDays = Number(daysValue);
            if (Number.isNaN(parsedDays) || parsedDays <= 0) {
                $('#systemLogsDeleteMessage').addClass('text-danger').text('Please enter a valid number of days.');
                return;
            }
            var cutoffDate = new Date();
            cutoffDate.setDate(cutoffDate.getDate() - parsedDays);
            cutoffTime = formatDateTime(cutoffDate);
        } else if (cutoffValue) {
            cutoffTime = cutoffValue;
        }

        if (!cutoffTime) {
            $('#systemLogsDeleteMessage').addClass('text-danger').text('Please select a cutoff datetime or enter days.');
            return;
        }

        if (!confirm('Delete logs older than the selected cutoff? This cannot be undone.')) {
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/AdministrationPanel/DeleteSystemLogs",
            type: "POST",
            data: {
                cutoffTime: cutoffTime
            },
            cache: false,
            success: function (resp) {
                $('#systemLogsDeleteMessage').addClass('text-success').text('Deleted ' + (resp.deletedCount || 0) + ' log(s).');
                fetchSystemLogs();
            },
            error: function () {
                $('#systemLogsDeleteMessage').addClass('text-danger').text('Unable to delete old logs.');
            }
        });
    }

    function formatDateTime(date) {
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');
        var hours = date.getHours();
        var minutes = String(date.getMinutes()).padStart(2, '0');
        var ampm = hours >= 12 ? 'PM' : 'AM';
        var displayHours = hours % 12;
        displayHours = displayHours ? displayHours : 12;
        var hourString = String(displayHours).padStart(2, '0');
        return year + '-' + month + '-' + day + ' ' + hourString + ':' + minutes + ' ' + ampm;
    }

    function renderSystemLogs(logs) {
        var tbody = $('#systemLogsTable tbody');
        tbody.empty();

        if (!logs || logs.length === 0) {
            tbody.append('<tr><td colspan="10" class="text-center">No logs found.</td></tr>');
            return;
        }

        $.each(logs, function (index, log) {
            var row = $('<tr></tr>');
            row.append($('<td></td>').text(formatLogTime(readLogField(log, 'LOGTIME'))));
            row.append($('<td></td>').text(readLogField(log, 'LOGLEVEL')));
            row.append($('<td></td>').text(readLogField(log, 'MODULE')));
            row.append($('<td></td>').text(readLogField(log, 'CONTROLLER')));
            row.append($('<td></td>').text(readLogField(log, 'ACTION')));
            row.append($('<td></td>').text(readLogField(log, 'MESSAGE')));
            row.append($('<td></td>').text(readLogField(log, 'TECHDETAILS')));
            row.append($('<td></td>').text(readLogField(log, 'PAGEID')));
            row.append($('<td></td>').text(readLogField(log, 'ENGID')));
            row.append($('<td></td>').text(readLogField(log, 'USERPPNO')));

            tbody.append(row);
        });
    }

    function readLogField(log, field) {
        if (!log) {
            return '';
        }

        var candidates = [
            field,
            field.charAt(0).toLowerCase() + field.slice(1),
            field.toLowerCase().replace(/_([a-z])/g, function (match, letter) { return letter.toUpperCase(); }),
            field.toLowerCase()
        ];

        for (var i = 0; i < candidates.length; i++) {
            var key = candidates[i];
            if (log[key] !== undefined && log[key] !== null) {
                return log[key];
            }
        }
        return '';
    }

    function formatLogTime(value) {
        if (!value) {
            return '';
        }

        var parsed = new Date(value);
        if (isNaN(parsed.getTime())) {
            return value;
        }

        return parsed.toLocaleString();
    }

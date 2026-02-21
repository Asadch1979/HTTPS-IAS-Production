(function (window) {
    function esc(v) { return $('<div/>').text(v || '').html(); }

    function field(row, keys) {
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            if (row[key] !== undefined && row[key] !== null) { return row[key]; }
            var lower = key.charAt(0).toLowerCase() + key.slice(1);
            if (row[lower] !== undefined && row[lower] !== null) { return row[lower]; }
            var upper = key.toUpperCase();
            if (row[upper] !== undefined && row[upper] !== null) { return row[upper]; }
        }
        return '';
    }

    function showAlert(msg, type) {
        $('#monitoringAlertHost').html('<div class="alert alert-' + (type || 'danger') + '">' + esc(msg || 'Unexpected error') + '</div>');
    }

    function renderTable(rows) {
        destroyDatatable('iidMonitoringTable');
        var $body = $('#iidMonitoringTable tbody');
        $body.empty();

        (rows || []).forEach(function (row) {
            var complaintId = field(row, ['complaintId', 'ComplaintId', 'COMPLAINT_ID']);
            var html = '<tr>' +
                '<td>' + esc(field(row, ['complaintNo', 'ComplaintNo', 'COMPLAINT_NO'])) + '</td>' +
                '<td>' + esc(field(row, ['complainantName', 'ComplainantName', 'COMPLAINANT_NAME'])) + '</td>' +
                '<td>' + esc(field(row, ['nature', 'Nature', 'NATURE'])) + '</td>' +
                '<td>' + esc(field(row, ['source', 'Source', 'SOURCE'])) + '</td>' +
                '<td>' + esc(field(row, ['unitName', 'UnitName', 'UNIT_NAME'])) + '</td>' +
                '<td>' + esc(field(row, ['status', 'Status', 'STATUS'])) + '</td>' +
                '<td><a class="btn btn-sm btn-outline-primary" href="' + (window.g_asiBaseURL || '') + '/IID/InquiryReportReadOnly?complaintId=' + encodeURIComponent(complaintId || 0) + '">View</a></td>' +
                '</tr>';
            $body.append(html);
        });

        initializeDataTable('iidMonitoringTable');
    }

    function loadActiveComplaints() {
        $.ajax({
            url: (window.g_asiBaseURL || '') + '/ApiCalls/GetIidActiveComplaints',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ page: 1, pageSize: 200, filters: {} })
        }).done(function (resp) {
            if (resp && resp.ok === false) {
                showAlert(resp.message || 'Could not load active complaints.');
                return;
            }
            renderTable(resp || []);
        }).fail(function () {
            showAlert('Failed to load active complaints.');
        });
    }

    $(function () { loadActiveComplaints(); });
}(window));

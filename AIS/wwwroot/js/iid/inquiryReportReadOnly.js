(function (window) {
    function esc(v) { return $('<div/>').text(v || '').html(); }
    function pick(obj, keys, fallback) {
        obj = obj || {};
        for (var i = 0; i < keys.length; i++) {
            var k = keys[i];
            if (obj[k] !== undefined && obj[k] !== null) { return obj[k]; }
            var lower = k.charAt(0).toLowerCase() + k.slice(1);
            if (obj[lower] !== undefined && obj[lower] !== null) { return obj[lower]; }
            var upper = k.toUpperCase();
            if (obj[upper] !== undefined && obj[upper] !== null) { return obj[upper]; }
        }
        return fallback || '';
    }

    function toList(payload, keys) {
        for (var i = 0; i < keys.length; i++) {
            var val = pick(payload, [keys[i]], null);
            if (Array.isArray(val)) { return val; }
        }
        return [];
    }

    function showAlert(msg, type) {
        $('#iidReadOnlyAlertHost').html('<div class="alert alert-' + (type || 'danger') + '">' + esc(msg || 'Unexpected error') + '</div>');
    }

    function bindRows(tableId, rows, columns) {
        var $body = $(tableId + ' tbody');
        $body.empty();
        if (!rows || !rows.length) {
            $body.append('<tr><td colspan="' + columns.length + '" class="text-muted">No data available.</td></tr>');
            return;
        }
        rows.forEach(function (row) {
            var tds = columns.map(function (c) { return '<td>' + esc(pick(row, c, '')) + '</td>'; }).join('');
            $body.append('<tr>' + tds + '</tr>');
        });
    }

    function setNarrative(id, value) {
        var text = (value || '').toString();
        var html = esc(text).replace(/\n/g, '<br/>');
        $(id).html(html || '<span class="text-muted">N/A</span>');
    }

    function bindSummary(data) {
        var pairs = [
            ['Complaint No', pick(data, ['complaintNo', 'ComplaintNo'])],
            ['Nature', pick(data, ['nature', 'Nature'])],
            ['Source', pick(data, ['source', 'Source'])],
            ['I&I Unit', pick(data, ['unitName', 'UnitName'])],
            ['Status', pick(data, ['status', 'Status'])],
            ['Complainant', pick(data, ['complainantName', 'ComplainantName'])]
        ];
        var html = pairs.map(function (x) {
            return '<div class="col-md-4"><div class="small text-muted">' + esc(x[0]) + '</div><div class="fw-semibold">' + esc(x[1] || 'N/A') + '</div></div>';
        }).join('');
        $('#summaryGrid').html(html);
    }

    function bindAll(payload) {
        var header = pick(payload, ['complaintHeader', 'header', 'summary'], {}) || {};
        bindSummary(header);

        bindRows('#tblAccused', toList(payload, ['accusedList', 'accused']), [['name', 'employeeName', 'accusedName'], ['designation', 'employeeDesignation'], ['ppNo', 'ppno']]);
        bindRows('#tblAccusations', toList(payload, ['accusations', 'accusedAccusations']), [['accusation', 'title', 'details']]);
        bindRows('#tblRecords', toList(payload, ['recordsScrutinized', 'records']), [['recordTitle', 'title'], ['details', 'recordDetail']]);
        bindRows('#tblStatements', toList(payload, ['statementRegister', 'statements']), [['name', 'personName'], ['role', 'designation'], ['statementDate', 'date'], ['mode', 'recordingMode']]);
        bindRows('#tblEvidence', toList(payload, ['evidenceFiles', 'evidence']), [['fileName', 'name'], ['fileType', 'type'], ['uploadedOn', 'date']]);
        bindRows('#tblViolations', toList(payload, ['violationsList', 'violations']), [['category'], ['detail', 'details'], ['reference'], ['recommendation']]);
        bindRows('#tblDsa', toList(payload, ['dsaList', 'dsa']), [['person', 'name'], ['designation'], ['ppNo', 'ppno'], ['status']]);
        bindRows('#tblApprovals', toList(payload, ['finalApprovals', 'approvals', 'comments']), [['stage'], ['approvedBy', 'by'], ['comments', 'comment'], ['approvedOn', 'date']]);

        var report = pick(payload, ['inquiryReport', 'report'], payload);
        setNarrative('#txtGist', pick(report, ['gist', 'Gist']));
        setNarrative('#txtProceedings', pick(report, ['proceedings', 'Proceedings']));
        setNarrative('#txtFindings', pick(report, ['findings', 'Findings']));
        setNarrative('#txtRecommendation', pick(report, ['recommendation', 'Recommendation']));
    }

    function loadReadOnlyData(complaintId) {
        $.ajax({
            url: (window.g_asiBaseURL || '') + '/ApiCalls/GetIidInquiryReportReadOnlyData',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ complaintId: complaintId })
        }).done(function (resp) {
            if (resp && resp.ok === false) {
                showAlert(resp.message || 'Could not load inquiry report data.');
                return;
            }
            bindAll(resp || {});
        }).fail(function () {
            showAlert('Failed to load inquiry report read-only data.');
        });
    }

    function extractErrorMessage(xhr) {
        var fallback = 'Failed to export PDF.';
        if (!xhr) { return fallback; }

        var responseText = xhr.responseText;
        if (!responseText && xhr.response instanceof Blob) {
            return fallback;
        }

        if (responseText) {
            try {
                var parsed = JSON.parse(responseText);
                return parsed.message || parsed.error || fallback;
            } catch (e) {
                return responseText || fallback;
            }
        }

        return fallback;
    }

    function exportPdf(complaintId) {
        var $btn = $('#btnExportInquiryPdf');
        if ($btn.prop('disabled')) { return; }

        var original = $btn.text();
        $btn.prop('disabled', true).text('Exporting...');

        $.ajax({
            url: (window.g_asiBaseURL || '') + '/ApiCalls/ExportIidInquiryReportPdf',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ complaintId: complaintId }),
            xhrFields: { responseType: 'blob' }
        }).done(function (blob, status, xhr) {
            var fileName = 'InquiryReport_' + complaintId + '.pdf';
            var disposition = xhr.getResponseHeader('Content-Disposition') || '';
            var match = disposition.match(/filename\*?=(?:UTF-8''|\")?([^\";]+)/i);
            if (match && match[1]) {
                fileName = decodeURIComponent(match[1].replace(/\"/g, '').trim());
            }
            var url = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        }).fail(function (xhr) {
            showAlert(extractErrorMessage(xhr));
        }).always(function () {
            $btn.prop('disabled', false).text(original);
        });
    }

    $(function () {
        var complaintId = Number($('#iidReadOnlyRoot').data('complaint-id')) || 0;
        if (!complaintId) {
            showAlert('Complaint id is missing from request.');
            return;
        }

        loadReadOnlyData(complaintId);
        $('#btnExportInquiryPdf').on('click', function () { exportPdf(complaintId); });
    });
}(window));

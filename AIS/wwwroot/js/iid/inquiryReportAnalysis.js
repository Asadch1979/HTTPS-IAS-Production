(function (window) {
    var inquiryPageId = 408;

    function esc(v) { return $('<div/>').text(v || '').html(); }

    function pick(obj, keys, fallback) {
        obj = obj || {};
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            if (obj[key] !== undefined && obj[key] !== null) { return obj[key]; }
            var lower = key.charAt(0).toLowerCase() + key.slice(1);
            if (obj[lower] !== undefined && obj[lower] !== null) { return obj[lower]; }
            var upper = key.toUpperCase();
            if (obj[upper] !== undefined && obj[upper] !== null) { return obj[upper]; }
        }
        return fallback || '';
    }

    function toList(payload, keys) {
        for (var i = 0; i < keys.length; i++) {
            var value = pick(payload, [keys[i]], null);
            if (Array.isArray(value)) {
                return value;
            }
        }
        return [];
    }

    function showAlert(message, type) {
        var safe = (typeof sanitizeAlertMessageText === 'function')
            ? sanitizeAlertMessageText(message)
            : ((message || 'Unexpected error').toString().trim());
        var text = safe || 'Unexpected error';
        var $alert = $('<div/>', { 'class': 'alert alert-' + (type || 'danger') + ' text-prewrap' });
        $alert.text(text);
        $('#iidAnalysisAlertHost').empty().append($alert);
    }

    function bindRows(tableId, rows, columns) {
        var $body = $(tableId + ' tbody');
        $body.empty();
        if (!rows || !rows.length) {
            $body.append('<tr><td colspan="' + columns.length + '" class="text-muted">No data available.</td></tr>');
            return;
        }

        rows.forEach(function (row) {
            var tds = columns.map(function (column) {
                return '<td>' + esc(pick(row, column, '')) + '</td>';
            }).join('');
            $body.append('<tr>' + tds + '</tr>');
        });
    }

    function setNarrative(id, value, fallback) {
        var text = (value || '').toString();
        var html = esc(text).replace(/\n/g, '<br/>');
        $(id).html(html || '<span class="text-muted">' + esc(fallback || 'N/A') + '</span>');
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

        var html = pairs.map(function (pair) {
            return '<div class="col-md-4"><div class="small text-muted">' + esc(pair[0]) + '</div><div class="fw-semibold">' + esc(pair[1] || 'N/A') + '</div></div>';
        }).join('');
        $('#summaryGrid').html(html);
    }

    function applyFinalizeState(isFinalized, finalizeState) {
        var $button = $('#btnFinalizeInquiryReport');
        var normalizedState = (finalizeState || '').toString().trim().toUpperCase();

        if (isFinalized || normalizedState === 'Y') {
            $('#finalizeStatusText').removeClass('text-muted').addClass('text-success fw-semibold').text('This inquiry report is finalized.');
            $button.prop('disabled', true).text('Finalized');
            return;
        }

        if (normalizedState === 'A') {
            $('#finalizeStatusText').removeClass('text-success').addClass('text-muted').text('This inquiry report is submitted for analysis and ready to finalize.');
        } else {
            $('#finalizeStatusText').removeClass('text-success').addClass('text-muted').text('This inquiry report is not finalized yet.');
        }

        $button.prop('disabled', false).text('Finalize Report');
    }

    function bindAll(payload) {
        var header = pick(payload, ['complaintHeader', 'header', 'summary'], {}) || {};
        var report = pick(payload, ['inquiryReport', 'report'], payload) || {};
        var evidenceStep = pick(payload, ['evidenceStep'], {}) || {};
        var reportedInAuditReport = pick(report, ['reportedInAuditReport', 'ReportedInAuditReport'], '');

        bindSummary(header);

        bindRows('#tblAccused', toList(payload, ['accusedList', 'accused']), [['name', 'personName', 'employeeName', 'accusedName'], ['designation', 'employeeDesignation'], ['ppNo', 'ppno', 'ppnoNumber']]);
        bindRows('#tblAccusations', toList(payload, ['accusations', 'accusedAccusations']), [['accusation', 'accusationText', 'title', 'details']]);
        bindRows('#tblRecords', toList(payload, ['recordsScrutinized', 'records']), [['recordTitle', 'title'], ['details', 'recordDetails', 'recordDetail']]);
        bindRows('#tblProceedings', toList(payload, ['inquiryProceedings', 'proceedings']), [['noticeReference'], ['visitDate'], ['placeVisited'], ['participantsDetail'], ['missingParticipantsReason']]);
        bindRows('#tblStatements', toList(payload, ['statementRegister', 'statements']), [['name', 'personName'], ['role', 'roleType', 'designation'], ['statementDate', 'statementDatetime', 'date'], ['mode', 'modeType', 'recordingMode'], ['keyPoints', 'KeyPoints']]);
        bindRows('#tblEvidence', toList(payload, ['evidenceFiles', 'evidence']), [['fileName', 'name'], ['fileType', 'fileExt', 'evidenceType', 'type'], ['uploadedOn', 'date']]);
        bindRows('#tblViolations', toList(payload, ['violationsList', 'violations']), [['category'], ['detail', 'violationDetail', 'details'], ['reference', 'referenceText'], ['recommendation']]);
        bindRows('#tblDsa', toList(payload, ['dsaList', 'dsa']), [['person', 'personName', 'name'], ['designation'], ['ppNo', 'ppno', 'ppnoNumber'], ['status', 'dsaStatus']]);
        bindRows('#tblApprovals', toList(payload, ['finalApprovals', 'approvals', 'comments']), [['stage'], ['approvedBy', 'by'], ['comments', 'comment'], ['approvedOn', 'date']]);

        setNarrative('#txtProceedings', pick(report, ['proceedings', 'Proceedings']));
        setNarrative('#txtFindings', pick(report, ['findings', 'Findings', 'findingText', 'FindingText']));
        setNarrative('#txtRecommendation', pick(report, ['recommendation', 'Recommendation', 'recommendationText', 'RecommendationText']));
        setNarrative('#txtRootCause', pick(report, ['conclusion', 'Conclusion']));
        setNarrative('#txtReportedInAuditReport', reportedInAuditReport || 'Not reported');
        setNarrative('#txtAuditReportReferenceDetail', pick(report, ['auditReportReferenceDetail', 'AuditReportReferenceDetail']));
        setNarrative('#txtMaterialEvidenceDetail', pick(evidenceStep, ['materialEvidenceDetail', 'MaterialEvidenceDetail']));
        setNarrative('#txtCircumstantialEvidenceDetail', pick(evidenceStep, ['circumstantialEvidenceDetail', 'CircumstantialEvidenceDetail']));

        $('#auditReportReferenceDetailWrap').toggle((reportedInAuditReport || '').toString().toLowerCase() === 'yes');
        applyFinalizeState(pick(header, ['isFinalized', 'IsFinalized'], false), pick(header, ['finalizeState', 'FinalizeState'], ''));
    }

    function requestPdf(complaintId, regenerate) {
        if (!complaintId) { return; }
        var endpoint = regenerate ? '/IidInquiryReportPdf/RegeneratePdf' : '/IidInquiryReportPdf/GeneratePdf';
        var url = (window.g_asiBaseURL || '') + endpoint + '?complaintId=' + encodeURIComponent(complaintId);
        window.open(url, '_blank');
    }

    function viewExistingPdf(complaintId) {
        if (!complaintId) { return; }
        window.open((window.g_asiBaseURL || '') + '/IidInquiryReportPdf/ViewPdf?complaintId=' + encodeURIComponent(complaintId), '_blank');
    }

    function loadAnalysisData(complaintId) {
        return $.ajax({
            url: (window.g_asiBaseURL || '') + '/ApiCalls/GetIidInquiryReportReadOnlyData',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ complaintId: complaintId })
        }).done(function (response) {
            if (response && response.ok === false) {
                showAlert(response.message || 'Could not load inquiry report data.');
                return;
            }

            bindAll(response || {});
        }).fail(function (xhr) {
            showAlert(extractApiMessageFromXhr ? extractApiMessageFromXhr(xhr, 'Failed to load analysis data.') : 'Failed to load analysis data.');
        });
    }

    function loadComplaintDropdown(selectedComplaintId) {
        return $.post((window.g_asiBaseURL || '') + '/ApiCalls/GetComplaintsDropdown', { pageId: inquiryPageId })
            .done(function (list) {
                var $dd = $('#ddlInquiryComplaint');
                $dd.empty().append('<option value="0">--Select Inquiry--</option>');
                (list || []).forEach(function (item) {
                    var selected = Number(item.complaintId) === Number(selectedComplaintId) ? ' selected' : '';
                    $dd.append('<option value="' + esc(item.complaintId) + '"' + selected + '>' + esc(item.displayText) + '</option>');
                });
            });
    }

    function finalizeInquiryReport(complaintId) {
        if (!complaintId) {
            showAlert('ComplaintId is required for finalization.');
            return;
        }

        var $button = $('#btnFinalizeInquiryReport');
        $button.prop('disabled', true).text('Finalizing...');

        $.ajax({
            url: (window.g_asiBaseURL || '') + '/ApiCalls/FinalizeIidInquiryReport',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ complaintId: complaintId })
        }).done(function (response) {
            if (!response || response.ok === false) {
                showAlert((response && response.message) || 'Finalization failed.');
                $button.prop('disabled', false).text('Finalize Report');
                return;
            }

            showAlert(response.message || 'Inquiry report finalized successfully.', 'success');
            loadAnalysisData(complaintId);
        }).fail(function (xhr) {
            showAlert(extractApiMessageFromXhr ? extractApiMessageFromXhr(xhr, 'Finalization failed.') : 'Finalization failed.');
            $button.prop('disabled', false).text('Finalize Report');
        });
    }

    $(function () {
        var complaintId = Number($('#iidAnalysisRoot').data('complaint-id')) || 0;
        var dashboardMode = ($('#iidAnalysisRoot').data('dashboard-mode') || '').toString().toLowerCase() === 'true';

        if (!dashboardMode) {
            loadComplaintDropdown(complaintId).fail(function () {
                showAlert('Failed to load inquiry dropdown.');
            });

            $('#ddlInquiryComplaint').on('change', function () {
                var selectedId = Number($(this).val()) || 0;
                if (!selectedId) { return; }
                window.location.href = (window.g_asiBaseURL || '') + '/IID/Analysis?complaintId=' + encodeURIComponent(selectedId);
            });
        }

        if (complaintId) {
            loadAnalysisData(complaintId);
        }

        $('#btnGenerateInquiryPdf').on('click', function () { requestPdf(complaintId, false); });
        $('#btnRegenerateInquiryPdf').on('click', function () { requestPdf(complaintId, true); });
        $('#btnViewInquiryPdf').on('click', function () { viewExistingPdf(complaintId); });
        $('#btnFinalizeInquiryReport').on('click', function () { finalizeInquiryReport(complaintId); });
    });
}(window));

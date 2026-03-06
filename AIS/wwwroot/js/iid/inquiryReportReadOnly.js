(function (window) {
    var inquiryPageId = 5;
    var lastPayload = null;

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
        var safe = (typeof sanitizeAlertMessageText === 'function')
            ? sanitizeAlertMessageText(msg)
            : ((msg || 'Unexpected error').toString().trim());
        var text = safe || 'Unexpected error';
        var $alert = $('<div/>', { 'class': 'alert alert-' + (type || 'danger') + ' text-prewrap' });
        $alert.text(text);
        $('#iidReadOnlyAlertHost').empty().append($alert);
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
        lastPayload = payload || {};
        var header = pick(payload, ['complaintHeader', 'header', 'summary'], {}) || {};
        bindSummary(header);

        bindRows('#tblAccused', toList(payload, ['accusedList', 'accused']), [['name', 'personName', 'employeeName', 'accusedName'], ['designation', 'employeeDesignation'], ['ppNo', 'ppno', 'ppnoNumber']]);
        bindRows('#tblAccusations', toList(payload, ['accusations', 'accusedAccusations']), [['accusation', 'accusationText', 'title', 'details']]);
        bindRows('#tblRecords', toList(payload, ['recordsScrutinized', 'records']), [['recordTitle', 'title'], ['details', 'recordDetails', 'recordDetail']]);
        bindRows('#tblStatements', toList(payload, ['statementRegister', 'statements']), [['name', 'personName'], ['role', 'roleType', 'designation'], ['statementDate', 'statementDatetime', 'date'], ['mode', 'modeType', 'recordingMode']]);
        bindRows('#tblEvidence', toList(payload, ['evidenceFiles', 'evidence']), [['fileName', 'name'], ['fileType', 'fileExt', 'evidenceType', 'type'], ['uploadedOn', 'date']]);
        bindRows('#tblViolations', toList(payload, ['violationsList', 'violations']), [['category'], ['detail', 'violationDetail', 'details'], ['reference', 'referenceText'], ['recommendation']]);
        bindRows('#tblDsa', toList(payload, ['dsaList', 'dsa']), [['person', 'personName', 'name'], ['designation'], ['ppNo', 'ppno', 'ppnoNumber'], ['status', 'dsaStatus']]);
        bindRows('#tblApprovals', toList(payload, ['finalApprovals', 'approvals', 'comments']), [['stage'], ['approvedBy', 'by'], ['comments', 'comment'], ['approvedOn', 'date']]);

        var report = pick(payload, ['inquiryReport', 'report'], payload);
        setNarrative('#txtGist', pick(report, ['gist', 'Gist', 'findingText', 'FindingText']));
        setNarrative('#txtProceedings', pick(report, ['proceedings', 'Proceedings']));
        setNarrative('#txtFindings', pick(report, ['findings', 'Findings', 'findingText', 'FindingText']));
        setNarrative('#txtRecommendation', pick(report, ['recommendation', 'Recommendation', 'recommendationText', 'RecommendationText']));
    }

    function formatDateTitlePart(value) {
        if (!value) { return new Date().toISOString().slice(0, 10); }
        var dt = new Date(value);
        if (isNaN(dt.getTime())) {
            var cleaned = value.toString().trim().replace(/[\s/\\:]+/g, '-');
            return cleaned || new Date().toISOString().slice(0, 10);
        }
        return dt.toISOString().slice(0, 10);
    }

    function cleanFilePart(value, fallback) {
        var text = (value || fallback || '').toString().trim();
        return (text || fallback || 'NA').replace(/[^A-Za-z0-9._-]+/g, '_').replace(/^_+|_+$/g, '');
    }

    function formatRowsParagraph(rows, mapFn) {
        if (!rows || !rows.length) { return 'N/A'; }
        return rows.map(function (row, idx) {
            return (idx + 1) + '. ' + mapFn(row);
        }).join('\n');
    }

    function fillPrintTemplate(payload) {
        var header = pick(payload, ['complaintHeader', 'header', 'summary'], {}) || {};
        var report = pick(payload, ['inquiryReport', 'report'], payload) || {};
        var accused = toList(payload, ['accusedList', 'accused']);
        var accusations = toList(payload, ['accusations', 'accusedAccusations']);
        var records = toList(payload, ['recordsScrutinized', 'records']);
        var statements = toList(payload, ['statementRegister', 'statements']);
        var evidence = toList(payload, ['evidenceFiles', 'evidence']);
        var violations = toList(payload, ['violationsList', 'violations']);
        var dsa = toList(payload, ['dsaList', 'dsa']);

        var complaintNo = pick(header, ['complaintNo', 'ComplaintNo'], 'N/A');
        var branchName = pick(header, ['branchName', 'BranchName', 'branch', 'Branch', 'unitName', 'UnitName'], 'N/A');
        var regionName = pick(header, ['regionName', 'RegionName', 'region', 'Region'], 'N/A');
        var reportDate = pick(header, ['reportDate', 'ReportDate', 'updatedOn', 'UpdatedOn'], new Date().toISOString().slice(0, 10));

        $('#ptComplaintNo').text(complaintNo);
        $('#ptBranchTop,#ptBranchName').text(branchName);
        $('#ptRegionName').text(regionName);
        $('#ptReportDate').text(reportDate);
        $('#ptComplaintDetails').text(pick(report, ['gist', 'Gist', 'findingText', 'FindingText'], 'N/A'));
        $('#ptComplainantDetails').text([pick(header, ['complainantName', 'ComplainantName'], ''), pick(header, ['complainantCnic', 'ComplainantCNIC', 'cnic', 'CNIC'], '')].filter(Boolean).join(', ') || 'N/A');
        $('#ptReferenceDetails').text(pick(header, ['source', 'Source', 'nature', 'Nature'], 'N/A'));
        $('#ptAccusationsPara').text(formatRowsParagraph(accusations, function (row) {
            return pick(row, ['accusation', 'accusationText', 'title', 'details'], 'N/A');
        }));

        var mainAccused = accused.filter(function (x) {
            var role = (pick(x, ['roleType', 'role', 'accusedRole'], '') || '').toString().toLowerCase();
            return role.indexOf('main') >= 0 || role.indexOf('primary') >= 0;
        });
        var coAccused = accused.filter(function (x) {
            return mainAccused.indexOf(x) < 0;
        });

        function accusedLine(row) {
            return [pick(row, ['name', 'personName', 'employeeName', 'accusedName'], ''), pick(row, ['designation', 'employeeDesignation'], ''), pick(row, ['ppNo', 'ppno', 'ppnoNumber'], '')].filter(Boolean).join(', ');
        }

        $('#ptMainAccused').text(formatRowsParagraph(mainAccused.length ? mainAccused : accused, accusedLine));
        $('#ptCoAccused').text(formatRowsParagraph(coAccused, accusedLine));
        $('#ptProceedings').text(pick(report, ['proceedings', 'Proceedings'], 'N/A'));
        $('#ptRecordsPara').text(formatRowsParagraph(records, function (row) {
            return [pick(row, ['recordTitle', 'title'], ''), pick(row, ['details', 'recordDetails', 'recordDetail'], '')].filter(Boolean).join(' - ');
        }));

        var complainantSt = statements.filter(function (s) {
            var role = (pick(s, ['role', 'roleType', 'designation'], '') || '').toString().toLowerCase();
            return role.indexOf('complain') >= 0;
        });
        var accusedSt = statements.filter(function (s) {
            return complainantSt.indexOf(s) < 0;
        });
        function statementLine(s) {
            return [pick(s, ['name', 'personName'], ''), pick(s, ['statementDate', 'statementDatetime', 'date'], ''), pick(s, ['mode', 'modeType', 'recordingMode'], '')].filter(Boolean).join(', ');
        }
        $('#ptStatementsComplainant').text(formatRowsParagraph(complainantSt, statementLine));
        $('#ptStatementsAccused').text(formatRowsParagraph(accusedSt, statementLine));
        $('#ptEvidencePara').text(formatRowsParagraph(evidence, function (row) { return pick(row, ['fileName', 'name'], 'N/A'); }));
        $('#ptFindingsPara').text(pick(report, ['findings', 'Findings', 'findingText', 'FindingText'], 'N/A'));
        $('#ptRecommendationsPara').text(pick(report, ['recommendation', 'Recommendation', 'recommendationText', 'RecommendationText'], 'N/A'));
        $('#ptDsaPara').text(formatRowsParagraph(dsa, function (row) {
            return [pick(row, ['person', 'personName', 'name'], ''), pick(row, ['designation'], ''), pick(row, ['ppNo', 'ppno', 'ppnoNumber'], '')].filter(Boolean).join(', ');
        }));
        $('#ptViolationsPara').text(formatRowsParagraph(violations, function (row) {
            return [pick(row, ['detail', 'violationDetail', 'details'], ''), pick(row, ['reference', 'referenceText'], ''), pick(row, ['recommendation'], '')].filter(Boolean).join(' | ');
        }));

        return {
            complaintNo: complaintNo,
            branchName: branchName,
            reportDate: reportDate
        };
    }

    function generatePdf(complaintId) {
        var $btn = $('#btnGenerateInquiryPdf');
        if ($btn.prop('disabled') || !complaintId || !lastPayload) { return; }

        var originalBtnText = $btn.text();
        var originalTitle = document.title;
        $btn.prop('disabled', true).text('Preparing...');

        var titleParts = fillPrintTemplate(lastPayload);
        var fileName = [
            'InquiryReport',
            cleanFilePart(titleParts.complaintNo, complaintId),
            cleanFilePart(titleParts.branchName, 'Branch'),
            cleanFilePart(formatDateTitlePart(titleParts.reportDate), new Date().toISOString().slice(0, 10))
        ].join('_');

        document.title = fileName;
        document.body.classList.add('printing-inquiry-report');

        setTimeout(function () {
            window.print();
            setTimeout(function () {
                document.body.classList.remove('printing-inquiry-report');
                document.title = originalTitle;
                $btn.prop('disabled', false).text(originalBtnText);
            }, 300);
        }, 100);
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

    function loadComplaintDropdown(selectedComplaintId) {
        return $.post((window.g_asiBaseURL || '') + '/ApiCalls/GetComplaintsDropdown', { pageId: inquiryPageId })
            .done(function (list) {
                var $dd = $('#ddlInquiryComplaint');
                $dd.empty().append('<option value="0">--Select Inquiry--</option>');
                (list || []).forEach(function (x) {
                    var selected = Number(x.complaintId) === Number(selectedComplaintId) ? ' selected' : '';
                    $dd.append('<option value="' + esc(x.complaintId) + '"' + selected + '>' + esc(x.displayText) + '</option>');
                });
            });
    }

    $(function () {
        var complaintId = Number($('#iidReadOnlyRoot').data('complaint-id')) || 0;

        loadComplaintDropdown(complaintId).fail(function () {
            showAlert('Failed to load inquiry dropdown.');
        });

        $('#ddlInquiryComplaint').on('change', function () {
            var selectedId = Number($(this).val()) || 0;
            if (!selectedId) { return; }
            window.location.href = (window.g_asiBaseURL || '') + '/IID/InquiryReportReadOnly?complaintId=' + encodeURIComponent(selectedId);
        });

        if (complaintId) {
            loadReadOnlyData(complaintId);
        }

        $('#btnGenerateInquiryPdf').on('click', function () { generatePdf(complaintId); });
    });
}(window));

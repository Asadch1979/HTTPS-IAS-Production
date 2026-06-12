    function getParaDetailValue(source, names) {
        if (!source || typeof source !== 'object') {
            return '';
        }

        for (var i = 0; i < names.length; i++) {
            var value = source[names[i]];
            if (value !== undefined && value !== null && String(value).trim() !== '') {
                return value;
            }
        }

        return '';
    }

    function setParaDetailHtml(selector, value) {
        $(selector).html(value && String(value).trim() !== '' ? value : '<span class="text-muted">N/A</span>');
    }

    function resolveAuditeeResponse(source) {
        return getParaDetailValue(source, ['AUDITEE_RESPONSE', 'auditeE_RESPONSE', 'auditeeResponse', 'BRANCH_REPLY', 'branchReply']);
    }

    function resolveAuditRecommendation(source) {
        return getParaDetailValue(source, ['AUDIT_RECOMMENDATION', 'auditRecommendation', 'AUDITOR_RECOMMENDATION', 'auditoR_RECOMMENDATION', 'RECOMMENDATION', 'recommendation', 'CAU_INSTRUCTION', 'cauInstruction']);
    }
    $(document).on('click', '.js-old-paras-find', function (event) {
        event.preventDefault();
        getEmployeeName();
    });

    $(document).on('click', '.js-view-para', function (event) {
        event.preventDefault();
        viewParaText($(this).data('comId'), $(this).data('ind'));
    });

    function getEntityObservation() {
        destroyDatatable('manageObsPanel');
        $('#manageObsPanel tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_old_paras_for_monitoring_ppno",
            type: "POST",
            data: {
                'ppno': $('#ppnoSearchField').val()
            },
            cache: false,
            success: function (data) {
                if (!Array.isArray(data)) {
                    alert('Unexpected response from server.');
                    return;
                }

                $.each(data, function (i, v) {
                    $('#manageObsPanel tbody').append(
                        '<tr id="assignedObRow_' + (v.oldParaId ?? i) + '"><td>' +
                        (i + 1) + '</td><td>' + (v.entityName || '') + '</td><td>' + (v.auditPeriod || '') + '</td><td>' + (v.annex || '') + '</td><td>' + (v.paraNo || '') + '</td><td>' + (v.gistOfParas || '') + '</td><td>' + (v.amount || '') + '</td><td><a href="#" class="text-danger text-center js-view-para" style="cursor:pointer;" data-com-id="' + (v.comId || '') + '" data-ind="' + (v.ind || '') + '">View Para</a></td></tr>'
                    );
                });

                initializeDataTable('manageObsPanel');
            },
            error: function (jqXHR, textStatus) {
                handlePageAjaxError(jqXHR, textStatus);
            },
            dataType: "json"
        });
    }

    function getEmployeeName() {
        var ppNo = ($('#ppnoSearchField').val() || '').trim();
        if (!ppNo || ppNo === "0") {
            alert('Please enter a valid PPNO.');
            return;
        }

        $('#employeename').val('');

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_user_name",
            type: "POST",
            data: {
                'PPNUMBER': ppNo
            },
            cache: false,
            success: function (data) {
                if (!data || typeof data !== 'object') {
                    alert('Unexpected response from server.');
                    return;
                }

                $('#employeename').val(data.Message || '');
                getEntityObservation();
            },
            error: function (jqXHR, textStatus) {
                handlePageAjaxError(jqXHR, textStatus);
            },
            dataType: "json"
        });
    }

    // Backward compatibility for existing callers.
    function getemployeename() {
        getEmployeeName();
    }

    function viewParaText(comid, ind) {
        if (!comid) {
            return;
        }

        $('#viewMemoModel').modal('show');
        $('#viewMemo_memo').html("");
        setParaDetailHtml('#viewMemo_auditeeResponse', '');
        setParaDetailHtml('#viewMemo_auditRecommendation', '');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getallparatext",
            type: "GET",
            data: { comId: comid },
            cache: false,
            dataType: "json",
            success: function (data) {
                var para = null;
                if (Array.isArray(data)) {
                    para = data.find(function (p) {
                        return String(p.IND ?? p.ind ?? '') === String(ind ?? '');
                    });
                } else {
                    alert('Unexpected response from server.');
                    return;
                }

                var text = para ? (para.ParaText || para.paraText || "") : "";
                $('#viewMemo_memo').html(text);
                setParaDetailHtml('#viewMemo_auditeeResponse', resolveAuditeeResponse(para));
                setParaDetailHtml('#viewMemo_auditRecommendation', resolveAuditRecommendation(para));
            },
            error: function (jqXHR, textStatus) {
                handlePageAjaxError(jqXHR, textStatus);
            }
        });
    }

    function handlePageAjaxError(jqXHR, textStatus) {
        var status = jqXHR && jqXHR.status ? jqXHR.status : 0;

        if (status === 401) {
            alert('Session expired. Please sign in again.');
            return;
        }

        if (status === 403) {
            alert('No permission.');
            return;
        }

        var contentType = jqXHR && typeof jqXHR.getResponseHeader === 'function'
            ? (jqXHR.getResponseHeader('content-type') || '')
            : '';

        if (textStatus === 'parsererror' || contentType.indexOf('text/html') !== -1) {
            alert('Unexpected response from server.');
            return;
        }

        alert('Request failed. Please try again.');
    }

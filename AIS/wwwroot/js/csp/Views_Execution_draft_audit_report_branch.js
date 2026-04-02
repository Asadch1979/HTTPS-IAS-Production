window.addEventListener("error", function (e) {
    console.error("JS error:", e.message, e.filename, e.lineno, e.colno);
});
window.addEventListener("unhandledrejection", function (e) {
    console.error("Promise rejection:", e.reason);
});

function getPageData() {
    const input = document.getElementById("page-data-json");
    const script = document.getElementById("page-data");
    const rawValue = input && input.value
        ? input.value
        : (script ? (script.textContent || "") : "");
    if (!rawValue) return {};
    try {
        return JSON.parse(rawValue || "{}");
    } catch (err) {
        console.error("Failed to parse execution page data JSON:", err);
        return {};
    }
}

    var g_obsId = 0;
    var g_newStatusId = 0;
    var g_entityID = 0;
    var g_riskId = 0;
    var g_obsList = [];
    var g_procId = 0;
    var g_subProcId=0;
    var g_procDetailId=0;
    var g_selectedRiskId = 0;
    var g_statusId = 0;
    var respSection = null;
    var pageData = {};
    var g_annexList = [];
    var g_riskList = [];
    var g_boDraftReadOnlyMode = false;

    function refreshDraftPageData() {
        pageData = getPageData();
        g_annexList = pageData.AnnexList || [];
        g_riskList = pageData.RiskList || [];
    }

    function parseDraftNumber(value) {
        var parsed = parseInt(value, 10);
        return Number.isNaN(parsed) ? 0 : parsed;
    }

    function getDraftFieldValue(item, keys) {
        if (!item) {
            return '';
        }

        for (var i = 0; i < keys.length; i += 1) {
            var key = keys[i];
            if (item[key] !== undefined && item[key] !== null && item[key] !== '') {
                return item[key];
            }
        }

        return '';
    }

    function getDraftRiskMetaByAnnexId(annexId) {
        var targetAnnexId = parseDraftNumber(annexId);
        var match = null;

        $.each(g_annexList, function (i, annex) {
            if (parseDraftNumber(getDraftFieldValue(annex, ['ID', 'id'])) === targetAnnexId) {
                match = {
                    riskId: parseDraftNumber(getDraftFieldValue(annex, ['RISK_ID', 'riskId', 'riskid', 'risK_ID'])),
                    riskName: getDraftFieldValue(annex, ['RISK', 'risk', 'DESCRIPTION', 'description'])
                };
                return false;
            }
        });

        return match;
    }

    function getDraftRiskMetaByRiskId(riskId) {
        var targetRiskId = parseDraftNumber(riskId);
        var match = null;

        $.each(g_riskList, function (i, risk) {
            if (parseDraftNumber(getDraftFieldValue(risk, ['R_ID', 'r_ID', 'id', 'ID'])) === targetRiskId) {
                match = {
                    riskId: targetRiskId,
                    riskName: getDraftFieldValue(risk, ['DESCRIPTION', 'description', 'RISK', 'risk'])
                };
                return false;
            }
        });

        return match;
    }

    function applyDraftRiskDisplay(riskName) {
        var displayValue = (riskName || '').toString();
        $('#viewMemo_risk_display').val(displayValue);

        var color = '';
        var normalizedRisk = displayValue.trim().toLowerCase();
        if (normalizedRisk === 'high') {
            color = 'red';
        } else if (normalizedRisk === 'medium') {
            color = 'gold';
        } else if (normalizedRisk === 'low') {
            color = 'green';
        }

        $('#viewMemo_risk_display').css('color', color);
    }

    function getDraftReferenceContainerSelector() {
        return '#viewMemoDetailsModel #boObservationReferenceSection';
    }

    function setDraftMemoContent(html) {
        var content = html || '';
        $('#viewMemo_memo_ObSent').val(content).trigger('change');
        var $editor = $('#viewMemoDetailsModel .richText-editor').first();
        if ($editor.length) {
            $editor.html(content);
        }
    }

    function getDraftMemoContent() {
        var $editor = $('#viewMemoDetailsModel .richText-editor').first();
        if ($editor.length) {
            return $editor.html();
        }

        return $('#viewMemo_memo_ObSent').val();
    }

    function getDraftObservationReferenceId(detail) {
        if (!detail) {
            return null;
        }

        var rawValue = detail.referenceId;
        if (rawValue === undefined || rawValue === null || rawValue === '') {
            rawValue = detail.REFERENCE_ID;
        }
        if (rawValue === undefined || rawValue === null || rawValue === '') {
            rawValue = detail.referencE_ID;
        }
        if (rawValue === undefined || rawValue === null || rawValue === '') {
            rawValue = detail.reference_ID;
        }

        var parsed = parseInt(rawValue, 10);
        return Number.isNaN(parsed) ? null : parsed;
    }

    function getDraftSelectedReferenceId() {
        var containerSelector = getDraftReferenceContainerSelector();
        var selectedReference = typeof window.getSelectedObservationReference === 'function'
            ? window.getSelectedObservationReference(containerSelector)
            : null;
        var currentReference = typeof window.getCurrentObservationReference === 'function'
            ? window.getCurrentObservationReference(containerSelector)
            : null;
        var rawValue = selectedReference && selectedReference.refId
            ? selectedReference.refId
            : (currentReference && currentReference.refId
                ? currentReference.refId
                : $(containerSelector + ' #observationReferenceId').val());
        var parsed = parseInt(rawValue, 10);
        return Number.isNaN(parsed) ? null : parsed;
    }

    function resetDraftObservationReference() {
        var containerSelector = getDraftReferenceContainerSelector();
        var $section = $(containerSelector);
        if (!$section.length || typeof window.initObservationReference !== 'function') {
            return;
        }

        $section.find('#observationReferenceId').val('');
        window.initObservationReference(containerSelector, {
            editMode: !g_boDraftReadOnlyMode,
            readOnly: g_boDraftReadOnlyMode,
            allowClear: false,
            forceReload: true,
            currentReferenceLabel: 'Saved Reference',
            emptyCurrentText: 'No reference selected yet.',
            initialRefId: null
        });
    }

    function initDraftObservationReference(detail) {
        var containerSelector = getDraftReferenceContainerSelector();
        if (!$(containerSelector).length || typeof window.initObservationReference !== 'function') {
            return;
        }

        window.initObservationReference(containerSelector, {
            editMode: !g_boDraftReadOnlyMode,
            readOnly: g_boDraftReadOnlyMode,
            allowClear: false,
            forceReload: true,
            currentReferenceLabel: 'Saved Reference',
            emptyCurrentText: 'No reference selected yet.',
            initialRefId: getDraftObservationReferenceId(detail)
        });
    }

    function scheduleDraftReferenceInit(detail) {
        var callback = function () {
            if (detail) {
                initDraftObservationReference(detail);
                return;
            }

            resetDraftObservationReference();
        };

        if (window.requestAnimationFrame) {
            window.requestAnimationFrame(callback);
            return;
        }

        window.setTimeout(callback, 0);
    }

    function commitDraftObservationReference() {
        if (typeof window.commitObservationReferenceSelection === 'function') {
            window.commitObservationReferenceSelection(getDraftReferenceContainerSelector());
        }
    }

    function saveDraftObservationReferenceUpdate(obsId) {
        if (g_boDraftReadOnlyMode) {
            return;
        }

        var targetObsId = obsId || g_obsId;
        if (!targetObsId) {
            alert('Observation is not selected.');
            return;
        }

        return $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_audit_para_for_finalization",
            type: "POST",
            data: {
                'OBS_ID': targetObsId,
                'ANNEX_ID': $('#viewMemo_annex_ObSent').val(),
                'PROCESS_ID': $('#viewMemo_process_ObSent').val(),
                'SUB_PROCESS_ID': $('#viewMemo_subprocess_ObSent').val() || 0,
                'PROCESS_DETAIL_ID': $('#viewMemo_checklist_ObSent').val() || 0,
                'RISK_ID': g_selectedRiskId,
                'FINAL_PARA_NO': 0,
                'GIST_OF_PARA': $('#viewMemo_heading_ObSent').val(),
                'TEXT_PARA': getDraftMemoContent(),
                'AMOUNT_INV': $('#viewMemo_amount_ObSent').val() || 0,
                'NO_INST': $('#viewMemo_inst_ObSent').val() || 0,
                'REFERENCE_ID': getDraftSelectedReferenceId()
            },
            cache: false,
            dataType: "json"
        }).done(function (data) {
            commitDraftObservationReference();
            showApiAlert(data);
        });
    }

    function initializeDraftReportUi() {
        console.log("Loaded draft_audit_report_branch JS", { g_obsId, g_entityID });

        if (!$('#viewMemoDetailsModel').length) {
            return;
        }

        refreshDraftPageData();

        if ($('#entitySelectField').length && !$('#entitySelectField').hasClass('select2-hidden-accessible')) {
            $('#entitySelectField').select2();
        }

        var entName = $('#manageObsPanel tbody .entity_name_field:first').text();
        $('#entityNameField').val(entName);
        var periodName = $('#manageObsPanel tbody .period_name_field:first').text();
        $('#auditPeriodNameField').val(periodName);

        if (!document.querySelector('#viewMemoDetailsModel .richText-editor')) {
            $('#viewMemo_memo_ObSent').richText({
                imageUpload: false,
                fileUpload: false,
                videoEmbed: false,
                urls: false
            });
        }

        $('#viewMemo_annex_ObSent').off('change.draftReport').on('change.draftReport', updateRiskDisplay);
        $('#viewMemoDetailsModel').off('hidden.bs.modal.boDraftReference').on('hidden.bs.modal.boDraftReference', resetDraftObservationReference);
        $('#obsReferenceSaveUpdateBtn').off('click.boDraftReference').on('click.boDraftReference', function () {
            saveDraftObservationReferenceUpdate(g_obsId);
        });

        respSection = initResponsibilitySection({
            tableSelector: '#update_listofRespPersons',
            changesTableSelector: '#c_update_listofRespPersons',
            modalSelector: '#ResponsiblePPModel',
            status: 4,
            directSaveMode: false,
            afterSave: function () {
                respSection.reload();
                viewObservationDetails(g_obsId, g_statusId);
            }
        });

        var amountField = document.getElementById('viewMemo_amount_ObSent');
        if (amountField && !amountField.dataset.draftAmountBound) {
            amountField.addEventListener('input', function () {
                this.value = this.value.replace(/\D|^0(?=\d)/g, '');
            });
            amountField.dataset.draftAmountBound = '1';
        }

        scheduleDraftReferenceInit(null);
    }

    $(document).ready(function () {
        initializeDraftReportUi();
    });
    function reloadLocation() {
        getEntityObservation();
    }

    function fieldAuditBoLoadDraftReport(engId, readOnly) {
        if (!engId) {
            return;
        }

        g_boDraftReadOnlyMode = !!readOnly;
        initializeDraftReportUi();

        var boEngId = String(engId);
        var selector = $('#entitySelectField');
        if (!selector.length) {
            return;
        }

        if (selector.find('option[value="' + boEngId + '"]').length === 0) {
            selector.append($('<option>', {
                value: boEngId,
                text: boEngId
            }));
        }

        selector.val(boEngId);
        selector.prop('disabled', true);
        $('#engIdHidden').val(boEngId);
        getEntityObservation();
    }

    window.fieldAuditBoLoadDraftReport = fieldAuditBoLoadDraftReport;

    function getEntityObservation() {

        $('#entitySelectField').attr('disabled', 'disabled');
        $('#manageObsPanel tbody').empty();
        var selectedEngId = $('#entitySelectField option:selected').val();
        $('#engIdHidden').val(selectedEngId);
        if (respSection) {
            respSection.updateContext({ engId: parseInt(selectedEngId || 0) });
        }
        if (selectedEngId != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_finalized_observations_draft_branch",
                type: "POST",
                data: {
                    'ENG_ID': selectedEngId
                },
                cache: false,
                success: function (data) {

                    $('#entitySelectField').removeAttr('disabled');
                    g_obsList = data;
                    var isbranch = false;
                    $.each(data, function (i, v) {
                          g_entityID=v.entitY_ID;
                        $('#auditPeriodNameField').val(v.period);
                        if (v.violation == null && v.nature == null) {
                            isbranch = true;
                        }
                        if (v.obS_STATUS_ID == 5)
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.heading + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td><a href="#" data-onclick="viewObservationDetails(' + v.obS_ID + ', '+v.obS_STATUS_ID+');" class="text-hover text-success ml-5px"><small>View Details</small></a></td></tr>');
                            //$('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.process + '</td><td class="branchfield">' + v.suB_PROCESS + '</td><td class="branchfield">' + v.checklist_Details + '</td><td class="branchfield">' + v.heading + '</td><td class="text-center"><a data-onclick="event.preventDefault();ViewObservation(' + v.obS_ID + ');" href="#" class="text-primary">View Observation</a></td><td class="obs_reply"><a data-onclick="ViewObservationResponse(' + v.obS_ID + ');" href="#" class="text-primary">View Response</a></td><td>' + v.auD_REPLY + '</td><td>' + v.heaD_REPLY + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="text-center"><a href="#" data-onclick="updateObservationStatus(' + v.obS_ID + ', 9,' + v.obS_RISK_ID + ');" class="text-hover text-danger mr-5px"><small>Settle</small></a></td><td><a href="#" data-onclick="updateObservationStatus(' + v.obS_ID + ',8,' + v.obS_RISK_ID + ');" class="text-hover text-primary ml-5px"><small>Add to Final Report</small></a></td></tr>');
                        else
                            $('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.heading + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td><a href="#" data-onclick="viewObservationDetails(' + v.obS_ID + ', '+v.obS_STATUS_ID+');" class="text-hover text-success ml-5px"><small>View Details</small></a></td></tr>');
                            //$('#manageObsPanel tbody').append('<tr id="' + v.obS_ID + '"><td class="text-center">' + v.memO_NO + '</td><td class="text-center">' + v.drafT_PARA_NO + '</td><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.process + '</td><td class="branchfield">' + v.suB_PROCESS + '</td><td class="branchfield">' + v.checklist_Details + '</td><td class="branchfield">' + v.heading + '</td><td class="text-center"><a data-onclick="event.preventDefault();ViewObservation(' + v.obS_ID + ');" href="#" class="text-primary">View Observation</a></td><td class="obs_reply"><a data-onclick="ViewObservationResponse(' + v.obS_ID + ');" href="#" class="text-primary">View Response</a></td><td>' + v.auD_REPLY + '</td><td>' + v.heaD_REPLY + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td></td><td></td></tr>');

                    });



                    setTimeout(function () {
                        if (g_obsId != 0) {
                            var rowpos = $('#manageObsPanel tbody tr#' + g_obsId).position();
                            $('html').scrollTop(rowpos.top);
                        }
                    }, 200)


                },
                dataType: "json",
            });
            getReportSummary();
        }
    }
    function getReportSummary() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/draft_report_summary",
            type: "POST",
            data: {
                'ENG_ID': $('#entitySelectField option:selected').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $('#totalObsLabel').text(data.total);
                $('#highObsLabel').text(data.high);
                $('#mediumObsLabel').text(data.medium);
                $('#lowObsLabel').text(data.low);
                $('#resolvedObsLabel').text(data.settled);
                $('#addToDraftLabel').text(data.addtoDraft);
                //  $('#downloadReport').html('<a  target="_blank" href="/Audit_Reports/' + data.reportName+'">Download Draft Report</a>');


            },
            dataType: "json",
        });
    }
    function finalCommentsButtonSave() {
        var svpComments = "";

        if ($('#finalNoInCommentsBox').val() == "") {
            alert("Please enter Final Para No to proceed");
            return;
        }


        svpComments = $('#commentAreaInCommentsBox').val();
        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Please enter the Comments First");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_status",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'DRAFT_PARA_NO': $('#finalNoInCommentsBox').val(),
                'RISK_ID': g_riskId,
                'AUDITOR_COMMENT': svpComments
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
                $('#commentsBox').modal('hide');
            },
            dataType: "json",
        });

    }
    function updateObservationStatus(obs_id, new_status_id, risk_id) {
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $('#commentsBox').modal('show');
        if (g_newStatusId == 9) {
            $('#finalNoInCommentsBox').val(0);
            $('#finalNoInCommentsBox').attr("disabled", true);

        } else {
            $('#finalNoInCommentsBox').val('');
            $('#finalNoInCommentsBox').attr("disabled", false);
        }
    }
    function submitPreConcluding() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_pre_concluding",
            type: "POST",
            data: {
                'ENG_ID': $('#entitySelectField option:selected').val()

            },
            cache: false,
            success: function (data) {

                showApiAlert(data);
            },
            dataType: "json",
        });
    }
    function viewObservationDetails(obsId,status_id){
        g_obsId=obsId;
        g_statusId = status_id;
        $('#viewMemoDetailsModel').off('shown.bs.modal.boDraftReferenceInit').one('shown.bs.modal.boDraftReferenceInit', function () {
            scheduleDraftReferenceInit(null);
        });
        $('#viewMemoDetailsModel').modal('show');

        if(g_boDraftReadOnlyMode || status_id !=5){
            $('#update_audit_obs_button').addClass("d-none");
            $('#un_settle_audit_obs_button').addClass("d-none");
            $('#settle_audit_obs_button').addClass("d-none");
            $('#dsa_audit_obs_button').addClass("d-none");

        }else{
            $('#update_audit_obs_button').removeClass("d-none");
            $('#un_settle_audit_obs_button').removeClass("d-none");
            $('#settle_audit_obs_button').removeClass("d-none");
            $('#dsa_audit_obs_button').removeClass("d-none");
        }

         $('#viewMemo_heading_ObSent').val('');
         setDraftMemoContent('');
         $('#viewMemo_response_ObSent').html('');
         $('#viewMemo_aud_reply_ObSent').html('');
         $('#viewMemo_head_reply_ObSent').html('');
         $('#viewMemo_annex_ObSent').val(0);
        $('#viewMemo_risk_display').val('');
        g_selectedRiskId = 0;
        $('#viewMemo_process_ObSent').val(0);
        $('#viewMemo_amount_ObSent').val(0);
        $('#viewMemo_inst_ObSent').val(0);
        g_procId = 0;
        g_subProcId = 0;
        g_procDetailId = 0;
        scheduleDraftReferenceInit(null);

          $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_obs_details_by_id",
            type: "POST",
            data: {
                'OBS_ID': g_obsId
            },
            cache: false,
            success: function (data) {

                if(data.dsA_ISSUED=="Y"){
                     $('#settle_audit_obs_button').addClass("d-none");
                }else{
                        $('#settle_audit_obs_button').removeClass("d-none");
                }
         $('#viewMemo_heading_ObSent').val(data.heading);
         setDraftMemoContent(data.observatioN_TEXT);
         $('#viewMemo_response_ObSent').html(data.auditeE_REPLY);
         ViewAuditeeAttachedEvidences();
         $('#viewMemo_aud_reply_ObSent').html(data.auditoR_RECOM);
         $('#viewMemo_annex_ObSent').val(data.annexurE_ID);
         g_selectedRiskId = parseDraftNumber(data.riskmodeL_ID || data.RISKMODEL_ID);
         updateRiskDisplay(g_selectedRiskId);
         $('#viewMemo_process_ObSent').val(data.procesS_ID);
         $('#viewMemo_amount_ObSent').val(data.amounT_INVOLVED);
         $('#viewMemo_inst_ObSent').val(data.nO_OF_INSTANCES);
         g_procId = data.procesS_ID;
        g_subProcId = data.subchecklisT_ID;
        g_procDetailId = data.checklistdetaiL_ID;
        getSubCheckListOfProcess();
        if (respSection && typeof respSection.updateContext === 'function') {
            respSection.updateContext({
                newParaId: data.neW_PARA_ID || g_obsId,
                oldParaId: data.olD_PARA_ID || 0,
                indicator: data.indicator || '',
                comId: 0,
                engId: parseInt($('#engIdHidden').val() || 0),
                readOnly: g_boDraftReadOnlyMode
            });
        }
               scheduleDraftReferenceInit(data);
               },
            dataType: "json",
        });
    }
    function getSubCheckListOfProcess() {
        $('#viewMemo_subprocess_ObSent').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
            type: "POST",
            data: {
                'PROCESS_ID': $('#viewMemo_process_ObSent').val()
            },
            cache: false,
            success: function (data) {
                $('#viewMemo_subprocess_ObSent').append('<option value="0">--Select Sub-Checklist--</option>');
                $.each(data, function (i, v) {
                    $('#viewMemo_subprocess_ObSent').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                });
                if(g_subProcId!=0)
                    {
                        $('#viewMemo_subprocess_ObSent').val(g_subProcId);
                      getCheckListDetailOfSubProcess();
                    }


            },
            dataType: "json",
        });

    }
    function getCheckListDetailOfSubProcess() {
        var processId = $('#viewMemo_process_ObSent').val();
        var subProcessId = $('#viewMemo_subprocess_ObSent').val();
        if (processId != 0 && subProcessId !=0) {
              $('#viewMemo_checklist_ObSent').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_audit_checklist_detail",
                type: "POST",
                data: {
                    'SUB_PROCESS_ID': subProcessId
                },
                cache: false,
                success: function (data) {
                     $('#viewMemo_checklist_ObSent').append('<option value="0">--Select Checklist Detail--</option>');
                $.each(data, function (i, v) {
                    $('#viewMemo_checklist_ObSent').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                });
                if (g_procDetailId != 0)
                    $('#viewMemo_checklist_ObSent').val(g_procDetailId);

                },
                dataType: "json",
            });
        }

    }
    function updateRiskDisplay(riskIdOverride) {
        var annexId = $('#viewMemo_annex_ObSent').val();
        var riskMeta = getDraftRiskMetaByAnnexId(annexId);
        var fallbackRiskId = parseDraftNumber(riskIdOverride);

        if (!riskMeta && fallbackRiskId > 0) {
            riskMeta = getDraftRiskMetaByRiskId(fallbackRiskId) || {
                riskId: fallbackRiskId,
                riskName: ''
            };
        }

        g_selectedRiskId = riskMeta ? parseDraftNumber(riskMeta.riskId) : fallbackRiskId;
        applyDraftRiskDisplay(riskMeta ? riskMeta.riskName : '');
    }
    function ViewAuditeeAttachedEvidences() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_responded_obs_evidences",
            type: "POST",
            data: {
                'OBS_ID': g_obsId
            },
            cache: false,
            success: function (data) {

                $('#complianceCycleEvidences').empty();
                if (data.length > 0) {
                    $.each(data, function (i, pp) {

                        var extension = pp.imagE_NAME.split('.').pop().toLowerCase();
                        const contentType = getContentType(extension);

                        // Create and append the attachment item
                        const container = document.createElement('div');
                        container.className = 'evidence-link';

                        // Add icon
                        const icon = document.createElement('i');
                        icon.className = getIconClass(extension) + ' evidence-icon mr-1';
                        container.appendChild(icon);

                        // Add label
                        const label = document.createElement('span');
                        label.innerText = pp.imagE_NAME;
                        label.classList.add('text-primary');

                        // Add cursor style to make it look like a link
                        label.style.cursor = 'pointer';
                        container.appendChild(label);

                        // Add click event to download file on selection
                        container.addEventListener('click', function () {
                            downloadFile(pp.filE_ID);
                        });

                        $('#complianceCycleEvidences').append(container);
                    });
                }
                else {
                    $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                }

            },
            dataType: "json",
        });
    }
    function downloadFile(id) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_auditee_evidence_data",
            type: "POST",
            data: {
                'FILE_ID': id,
            },
            cache: false,
            success: function (data) {
                var extension = data.imagE_NAME.split('.').pop().toLowerCase();
                const contentType = getContentType(extension);

                const blob = base64ToBlob(data.imagE_DATA, contentType);
                const link = document.createElement('a');
                link.href = URL.createObjectURL(blob);
                link.download = data.imagE_NAME;
                link.click(); // Trigger the download

            },
            dataType: "json",
        });


    }
    function getFileExtension(file) {
        var fileName = file.name;
        var extension = fileName.substring(fileName.lastIndexOf('.') + 1).toLowerCase();
        return extension;
    }
    function getIconClass(extension) {
        switch (extension) {
            case 'pdf': return 'fa fa-file-pdf';
            case 'zip': return 'fa fa-file-archive';
            case 'png':
            case 'jpg':
            case 'jpeg':
            case 'bmp': return 'fa fa-file-image';
            case 'doc':
            case 'docx': return 'fa fa-file-word';
            default: return 'fa fa-file';
        }
    }
    function getContentType(extension) {
        switch (extension) {
            case 'pdf': return 'application/pdf';
            case 'zip': return 'application/zip';
            case 'png': return 'image/png';
            case 'doc': return 'application/msword';
            case 'docx': return 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
            default: return 'application/octet-stream';
        }
    }
    function base64ToBlob(base64, contentType) {
        const byteCharacters = atob(base64);
        const byteArrays = [];

        for (let offset = 0; offset < byteCharacters.length; offset += 512) {
            const slice = byteCharacters.slice(offset, offset + 512);

            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        const blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }
    function openResponsiblePPs() {
        $('#ResponsiblePPModel').modal('show');
    }
    function updateObservationDetails(obsId){

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_audit_para_for_finalization",
            type: "POST",
            data: {
                'OBS_ID': obsId,
                'ANNEX_ID': $('#viewMemo_annex_ObSent').val(),
                'PROCESS_ID': $('#viewMemo_process_ObSent').val(),
                'SUB_PROCESS_ID': $('#viewMemo_subprocess_ObSent').val(),
                'PROCESS_DETAIL_ID': $('#viewMemo_checklist_ObSent').val(),
                'RISK_ID': g_selectedRiskId,
                'GIST_OF_PARA': $('#viewMemo_heading_ObSent').val(),
                'TEXT_PARA': getDraftMemoContent(),
                'AMOUNT_INV': $('#viewMemo_amount_ObSent').val(),
                'NO_INST': $('#viewMemo_inst_ObSent').val(),
                'REFERENCE_ID': getDraftSelectedReferenceId()

            },
            cache: false,
            success: function (data) {
                commitDraftObservationReference();
                showApiAlert(data);
                onAlertCallback(getEntityObservation);
            },
            dataType: "json",
        });
        return;
    }

    function dsaObservationDetails(obs_id){
        $('#viewMemoDetailsModel').modal('hide');
        $('#DSAModel').modal('show');
         $('#DSAModel').modal('show');
            $.each(g_obsList, function(i,v){
                if(v.obS_ID=obs_id){
                    $('#dsaHeading').val(v.heading);
                       $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observation_text_branches",
                type: "POST",
                data: {
                    'OBS_ID': obs_id
                },
                cache: false,
                success: function (data) {
                    $('#dsaContent').html(data[0].obS_TEXT);
                    $('#dsaResponsibles tbody').empty();
                    if (data[0].responsiblE_PPs.length > 0) {
                        $.each(data[0].responsiblE_PPs, function (j, pp) {
                            var srNo = $('#dsaResponsibles tbody tr').length;
                            srNo++;
                            $('#dsaResponsibles tbody').append('<tr id="tr_' + pp.pP_NO + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td><td><input class="chk_dsaissued" resp_row_id="'+pp.resP_ROW_ID+'" id="'+pp.pP_NO+'" type="checkbox"  /></td></tr>');
                        });
                    }


                },
                dataType: "json",
            });
                }
            })
    }

     function submitObservationToAuditeeAfterDSAIssuance(){
        var dsaArr=[];

        $.each($('.chk_dsaissued'), function(i,v){
            if($(v).is(":checked"))
            {
                dsaArr.push({"RESP_ROW_ID":$(v).attr("resp_row_id"),"RESP_PP_NO":$(v).attr("id")});
            }
    });

    if(dsaArr.length==0){
        alert("Please select at least one responsible from the list to issue DSA");
        return false;
    }

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/submit_dsa_to_auditee",
                type: "POST",
                data: {
                    'OBS_ID': g_obsId,
                    'ENTITY_ID': g_entityID,
                    'ENG_ID': $('#entitySelectField').val(),
                    "RespDSAModel":dsaArr
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                   onAlertCallback(submissionOfDSA);

                },
                dataType: "json",
            });

    }

    function submissionOfDSA(){
          $('#DSAModel').modal('hide');
                    $('#viewMemoDetailsModel').modal('show');

    }

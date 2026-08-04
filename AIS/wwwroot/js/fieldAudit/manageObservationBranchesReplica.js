window.addEventListener("error", function (e) {
    console.error("JS error:", e.message, e.filename, e.lineno, e.colno);
});
window.addEventListener("unhandledrejection", function (e) {
    console.error("Promise rejection:", e.reason);
});

function getPageData() {
    const el = document.getElementById("page-data");
    if (!el) return {};
    try {
        return JSON.parse(el.textContent || "{}");
    } catch (err) {
        console.error("Failed to parse #page-data JSON:", err);
        return {};
    }
}

    var g_obsId = 0;
    var g_entityID = 0;
    var g_newStatusId = 0;
    var g_riskId = 0;
    var g_annexId = 0;
    var g_currentStatus = 0;
    var g_obsList = [];
    var g_selectedRiskId = 0;
    var pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];
    var g_processId = 0;
    var g_subProcessId = 0;
    var g_checklistId = 0;
    var respSectionUpdate = null;
    var g_dsa = "";
    var g_tablePage = 0;
    var g_scrollPos = 0;
    var OBSERVATION_HEADING_VALIDATION_MESSAGE = 'Observation Heading/Title can contain only alphabets, numbers, space, &, ?, and comma.';

    function escapeHtml(value) {
        return $('<div>').text(value || '').html();
    }

    function getUpdateObservationHeading() {
        return $.trim($('#updateMemo_heading').val() || '');
    }

    function setObservationHeadingValidation(isValid) {
        var $heading = $('#updateMemo_heading');
        var $message = $('#updateMemoHeadingValidation');
        if (!$heading.length) {
            return;
        }

        if (isValid) {
            $heading.removeClass('is-invalid');
            $message.text(OBSERVATION_HEADING_VALIDATION_MESSAGE);
            return;
        }

        $heading.addClass('is-invalid');
        $message.text(OBSERVATION_HEADING_VALIDATION_MESSAGE);
    }

    function validateUpdateObservationHeading(showMessage) {
        var isValid = false;
        if (window.CommonValidation && CommonValidation.isAlnumOk) {
            isValid = CommonValidation.isAlnumOk('#updateMemo_heading', {
                allowAmp: true,
                allowQuestion: true,
                allowComma: true,
                allowSpace: true,
                required: true,
                rejectInvalid: true
            });
        }
        setObservationHeadingValidation(isValid);

        if (!isValid && showMessage) {
            alert(OBSERVATION_HEADING_VALIDATION_MESSAGE);
        }

        return isValid;
    }

    function bindObservationHeadingRestriction() {
        if (window.CommonValidation && CommonValidation.attachAlnumOnly) {
            CommonValidation.attachAlnumOnly('#updateMemo_heading', {
                allowAmp: true,
                allowQuestion: true,
                allowComma: true,
                allowSpace: true,
                maxLen: 200
            });
            return;
        }

        window.setTimeout(bindObservationHeadingRestriction, 50);
    }

    function resolveCurrentEngagementId() {
        var hiddenValue = $('#engIdHidden').val();
        var localValue = $('#fieldAuditManageObservationBranchesReplica').attr('data-eng-id');
        var hostValue = $('#fieldAuditStepHost').attr('data-eng-id');
        var rawValue = hiddenValue || localValue || hostValue || pageData.EngagementId || 0;
        var parsed = parseInt(rawValue, 10);
        return Number.isNaN(parsed) ? 0 : parsed;
    }

    function syncEngagementContext() {
        var engId = resolveCurrentEngagementId();
        $('#engIdHidden').val(engId || '');
        $('#fieldAuditManageObservationBranchesReplica').attr('data-eng-id', engId || '');
        return engId;
    }

    function getSelectedEngagementState() {
        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.getSelectedEngagementState === 'function') {
            return window.fieldAuditDashboard.getSelectedEngagementState() || {};
        }

        var selector = document.getElementById('engagementSelector');
        if (!selector || selector.selectedIndex < 0) {
            return {};
        }

        var selectedOption = selector.options[selector.selectedIndex];
        return {
            statusId: parseInt((selectedOption && selectedOption.getAttribute('data-status-id')) || '0', 10) || 0,
            isTeamLead: ((selectedOption && selectedOption.getAttribute('data-is-team-lead')) || 'N').toUpperCase()
        };
    }

    function isSelectedEngagementTeamLead() {
        return ((getSelectedEngagementState().isTeamLead) || 'N').toUpperCase() === 'Y';
    }

    function applyObservationEditMode() {
        var canEdit = isSelectedEngagementTeamLead();
        $('#updateMemo_process, #updateMemo_subprocess, #updateMemo_violation, #updateMemo_heading, #updateMemo_annex, #updateMemoContent').prop('disabled', !canEdit);
        $('#updateMemoModel .richText-editor').attr('contenteditable', canEdit ? 'true' : 'false');
        $('#updateMemoModel .richText-toolbar').toggleClass('d-none', !canEdit);
        $('#updateMemoContent_submit').toggleClass('d-none', !canEdit);
        $('#updateObservationReferenceSection').find('input, select, button').prop('disabled', !canEdit);
        $('#obsReferenceChangeBtn, #obsReferenceCancelEditBtn, #obsReferenceSearchBtn, #obsReferenceSaveUpdateBtn').toggleClass('d-none', !canEdit);
        $('#update_listofRespPersons').closest('.form-group').find('button[data-onclick="openResponsiblePPs();"]').toggleClass('d-none', !canEdit);
    }

    function canModifySelectedObservation() {
        return isSelectedEngagementTeamLead();
    }

    function preserveTablePosition() {
        g_scrollPos = $('html').scrollTop();
        if ($.fn.DataTable.isDataTable('#manageObsPanel')) {
            g_tablePage = $('#manageObsPanel').DataTable().page();
        }
    }

    function getObservationReferenceId(detail) {
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

    function getStep6SelectedReferenceId() {
        var selectedReference = typeof window.getSelectedObservationReference === 'function'
            ? window.getSelectedObservationReference('#updateObservationReferenceSection')
            : null;
        var currentReference = typeof window.getCurrentObservationReference === 'function'
            ? window.getCurrentObservationReference('#updateObservationReferenceSection')
            : null;
        var rawValue = selectedReference && selectedReference.refId
            ? selectedReference.refId
            : (currentReference && currentReference.refId ? currentReference.refId : $('#updateObservationReferenceSection #observationReferenceId').val());
        var parsed = parseInt(rawValue, 10);
        return Number.isNaN(parsed) ? null : parsed;
    }

    function saveObservationReferenceUpdate() {
        if (!canModifySelectedObservation()) {
            return;
        }

        if (!g_obsId || g_obsId <= 0) {
            alert('Observation is not selected.');
            return;
        }

        if (!validateUpdateObservationHeading(true)) {
            return;
        }

        var observationHeading = getUpdateObservationHeading();
        var referenceId = getStep6SelectedReferenceId();
        if (!referenceId) {
            alert('Please select a reference before saving.');
            return;
        }

        updateRiskDisplay();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_text",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'OBS_TITLE': observationHeading,
                'OBS_TEXT': $('.richText-editor').html(),
                'ANNEXURE_ID': $('#updateMemo_annex').val() || g_annexId,
                'RISK_ID': g_selectedRiskId || g_riskId,
                'PROCESS_ID': $('#updateMemo_process').val() || g_processId,
                'SUBPROCESS_ID': $('#updateMemo_subprocess').val() || g_subProcessId,
                'CHECKLIST_ID': $('#updateMemo_violation').val() || g_checklistId,
                'REFERENCE_ID': referenceId
            },
            cache: false,
            success: function (data) {
                if (typeof window.commitObservationReferenceSelection === 'function') {
                    window.commitObservationReferenceSelection('#updateObservationReferenceSection');
                }
                showApiAlert(data);
            },
            dataType: "json",
        });
    }

    function resetStep6ObservationReference() {
        var $section = $('#updateObservationReferenceSection');
        if (!$section.length) {
            return;
        }

        $section.find('#observationReferenceId').val('');

        if (typeof window.initObservationReference === 'function') {
            window.initObservationReference('#updateObservationReferenceSection', {
                editMode: true,
                allowClear: false,
                forceReload: true,
                initialRefId: null
            });
        }
    }

    function initManageObservationBranches() {
        var root = document.getElementById('fieldAuditManageObservationBranchesReplica');
        if (!root) {
            return;
        }

        if (root.getAttribute('data-initialized') === '1') {
            bindObservationHeadingRestriction();
            syncEngagementContext();
            return;
        }

        root.setAttribute('data-initialized', '1');
        console.log("Loaded manage_observations_branches JS", { g_obsId, g_entityID });
        var entName = $('#manageObsPanel tbody .entity_name_field:first').text();
        $('#entityNameField').val(entName);
        var periodName = $('#manageObsPanel tbody .period_name_field:first').text();
        $('#auditPeriodNameField').val(periodName);

        if (!document.querySelector('#updateMemoModel .richText-editor')) {
            $('#updateMemoContent').richText({
                imageUpload: false,
                fileUpload: false,
                videoEmbed: false,
                urls: false
            });
        }

        $('#updateMemo_annex').off('change.manageObservationBranches').on('change.manageObservationBranches', updateRiskDisplay);
        $('#obsReferenceSaveUpdateBtn').off('click.manageObservationBranches').on('click.manageObservationBranches', saveObservationReferenceUpdate);
        $('#updateMemoModel').off('hidden.bs.modal.manageObservationBranchesReference').on('hidden.bs.modal.manageObservationBranchesReference', resetStep6ObservationReference);
        bindObservationHeadingRestriction();
        $(document).off('fieldAudit:engagement-state-changed.manageObservationBranches').on('fieldAudit:engagement-state-changed.manageObservationBranches', function () {
            if ($('#updateMemoModel').hasClass('show')) {
                showActionButtons();
                applyObservationEditMode();
            }
        });
        var engId = syncEngagementContext();
        respSectionUpdate = initResponsibilitySection({
            tableSelector: '#update_listofRespPersons',
            changesTableSelector: '#c_update_listofRespPersons',
            modalSelector: '#ResponsiblePPModel',
            status: 1,
            directSaveMode: false,
            afterSave: function () {
                ObservationUpdatePanel(g_obsId);
            },
            engId: engId
        });

        getEntityObservation();
    }
    function reloadLocation() {
        getEntityObservation();
    }

    function reloadCurrentStepLocally() {
        $('#DSAModel').modal('hide');
        $('#commentsBox').modal('hide');
        $('#updateMemoModel').modal('hide');

        if (window.fieldAuditDashboard && typeof window.fieldAuditDashboard.reloadCurrentStepContent === 'function') {
            window.fieldAuditDashboard.reloadCurrentStepContent();
            return;
        }

        reloadLocation();
    }
    var MANAGE_OBS_COLUMNS = 7;

    function getManageObsColumnCount() {
        return $('#manageObsPanel thead th').length || MANAGE_OBS_COLUMNS;
    }

    function rebuildManageObservationTableBody() {
        var $table = $('#manageObsPanel');
        if (!$table.length) {
            return;
        }

        if ($.fn && $.fn.DataTable && $.fn.DataTable.isDataTable('#manageObsPanel')) {
            $table.DataTable().clear().destroy(true);
        }

        $table.find('tbody').remove();
        $table.append('<tbody></tbody>');
    }

    function initializeManageObservationTable() {
        if (!$.fn || !$.fn.DataTable) {
            return initializeDataTable('manageObsPanel');
        }

        if ($.fn.DataTable.isDataTable('#manageObsPanel')) {
            $('#manageObsPanel').DataTable().clear().destroy(true);
        }

        return $('#manageObsPanel').DataTable({
            dom: 'rt<"bottom"ip><"clear">',
            autoWidth: true,
            ordering: false,
            searching: false,
            lengthChange: false,
            columns: new Array(getManageObsColumnCount()).fill({ orderable: false }),
            lengthMenu: [
                [10, 50, 100, -1],
                [10, 50, 100, 'All']
            ]
        });
    }

    function renderMessageRow(message, cssClass) {
        var columnCount = getManageObsColumnCount();
        var rowCells = ['<td class="text-center">' + message + '</td>'];
        for (var i = 1; i < columnCount; i++) {
            rowCells.push('<td></td>');
        }

        $('#manageObsPanel tbody').append('<tr class="' + (cssClass || 'manage-obs-message') + '">' + rowCells.join('') + '</tr>');
    }

    function renderNoDataRow() {
        renderMessageRow('No observations found.', 'manage-obs-empty');
    }

    function getEntityObservation() {
        rebuildManageObservationTableBody();
        renderMessageRow('Please wait...', 'manage-obs-loading');

        var selectedEngId = syncEngagementContext();
        if (respSectionUpdate) {
            respSectionUpdate.updateContext({ engId: selectedEngId });
        }

        if (!selectedEngId) {
            renderNoDataRow();
            initializeManageObservationTable();
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_branches",
            type: "POST",
            data: {
                'ENG_ID': selectedEngId
            },
            cache: false,
            success: function (data) {
                g_obsList = data || [];
                $('#manageObsPanel tbody').empty();
                if (!g_obsList.length) {
                    renderNoDataRow();
                }

                $.each(g_obsList, function (i, v) {
                    var obsId = parseInt(v.obS_ID, 10) || 0;
                    g_entityID = v.entitY_ID;
                    $('#auditPeriodNameField').val(v.period);
                    var statusText = (v.obS_STATUS || '').toString().trim().toLowerCase();
                    var actionText = isSelectedEngagementTeamLead() ? 'Manage' : 'View';
                    var actionHtml = '<a data-onclick="ObservationUpdatePanel(' + obsId + ')" href="#" class="text-hover">' + escapeHtml(actionText) + '</a>';
                    if (statusText === 'submitted to auditee') {
                        actionHtml += '<span class="text-muted mx-2">|</span><a data-onclick="printObservation(' + obsId + ')" href="#" class="text-hover">Print</a>';
                    }

                    $('#manageObsPanel tbody').append('<tr id="' + obsId + '"><td class="text-center">' + escapeHtml(v.memO_NO) + '</td><td class="text-center">' + escapeHtml(v.annexurE_CODE) + '</td><td>' + escapeHtml(v.heading) + '</td><td>' + escapeHtml(v.nO_OF_INSTANCES) + '</td><td>' + escapeHtml(v.obS_RISK) + '</td><td>' + escapeHtml(v.obS_STATUS) + '</td><td class="text-center">' + actionHtml + '</td></tr>');
                });

                var tbl = initializeManageObservationTable();
                if (tbl && typeof tbl.page === 'function') {
                    tbl.page(g_tablePage).draw('page');
                }

                setTimeout(function () {
                    if ($('#manageObsPanel tbody tr#' + g_obsId).length > 0) {
                        var rowpos = $('#manageObsPanel tbody tr#' + g_obsId).position();
                        $('html').scrollTop(rowpos.top);
                    } else {
                        $('html').scrollTop(g_scrollPos);
                    }
                }, 200)
            },
            error: function () {
                $('#manageObsPanel tbody').empty();
                renderMessageRow('Unable to load observations right now.', 'manage-obs-error');
                initializeManageObservationTable();
            },
            dataType: "json",
        });
    }

    function printObservation(obsId) {
        if (!obsId || obsId <= 0) {
            alert('Observation id is required.');
            return;
        }

        var engId = syncEngagementContext();
        if (!engId || engId <= 0) {
            alert('Engagement id is required.');
            return;
        }

        var url = g_asiBaseURL + '/Observation/GeneratePdf?obsId=' + obsId + '&engId=' + engId;
        window.open(url, '_blank');
    }
    function getSubProcessList() {
        if ($('#updateMemo_process option:selected').val() == 0) {
            $('#updateMemo_subprocess').empty();
            $('#updateMemo_violation').empty();
        }
        else {

            $('#updateMemo_subprocess').empty();
            $('#updateMemo_violation').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/sub_checklist",
                type: "POST",
                data: {
                    'T_ID': $('#updateMemo_process option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#updateMemo_subprocess').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        $('#updateMemo_subprocess').append("<option value=\"" + escapeHtml(item.s_ID) + "\"> " + escapeHtml(item.heading) + " </option>");
                    });

                },
                dataType: "json",
            });
        }
    }
    function getSubProcessViolationList() {
        if ($('#updateMemo_subprocess option:selected').val() == 0)
            $('#updateMemo_violation').empty();
        else {
            $('#updateMemo_violation').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/checklist_details",
                type: "POST",
                data: {
                    'S_ID': $('#updateMemo_subprocess option:selected').val(),
                },
                cache: false,
                timeout: 300000,
                success: function (data) {
                    $('#updateMemo_violation').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        $('#updateMemo_violation').append("<option value=\"" + escapeHtml(item.id) + "\"> " + escapeHtml(item.heading) + "</option>");
                    });

                },
                error: function (xhr, textStatus) {
                    if (textStatus === "timeout") {
                        alert('Request taking longer than usual, please wait or refine search.');
                        return;
                    }
                    alert('Request failed. Please try again.');
                },
                dataType: "json",
            });
        }


    }
    function updateRiskDisplay() {
        var annexId = $('#updateMemo_annex').val();
        var riskName = '';
        g_selectedRiskId = 0;
        $.each(g_annexList, function (i, v) {
            var id = v.ID || v.id;
            if (id == annexId) {
                riskName = v.RISK || v.risk;
                g_selectedRiskId = v.RISK_ID || v.risK_ID;
            }
        });
        $('#updateMemo_risk_display').val(riskName);
        var color = '';
        if (riskName.toLowerCase() === 'high') {
            color = 'red';
        } else if (riskName.toLowerCase() === 'medium') {
            color = 'gold';
        } else if (riskName.toLowerCase() === 'low') {
            color = 'green';
        }
        $('#updateMemo_risk_display').css('color', color);
    }
    function ObservationUpdatePanel(obs_id) {
        g_obsId = obs_id;
        resetStep6ObservationReference();
        $.each(g_obsList, function (i, v) {
            if (v.obS_ID == obs_id) {
                g_currentStatus = v.obS_STATUS_ID;
                g_riskId = v.obS_RISK_ID;
                g_annexId = v.annexurE_ID;
                g_dsa = v.dsa;
            }
        });
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_text_branches",
            type: "POST",
            data: {
                'OBS_ID': obs_id
            },
            cache: false,
            success: function (data) {
                $('#updateMemoModel').modal('show');
                $('#updateMemoContent').val(data[0].obS_TEXT).trigger('change');
                $('#updateMemo_heading').val(data[0].heading);
                $('#updateMemo_process').val(data[0].procesS_ID);
                $('#updateMemo_annex').val(data[0].annexurE_ID);
                g_selectedRiskId = data[0].obS_RISK_ID;
                g_riskId = data[0].obS_RISK_ID;
                g_annexId = data[0].annexurE_ID;
                g_processId = data[0].procesS_ID;
                g_subProcessId = data[0].suB_PROCESS_ID;
                g_checklistId = data[0].checklist_Details_Id;
                updateRiskDisplay();
                $('#updateMemo_subprocess').empty();
                $('#updateMemo_subprocess').append('<option value="' + escapeHtml(g_subProcessId) + '">' + escapeHtml(data[0].suB_PROCESS) + '</option>');
                $('#updateMemo_violation').empty();
                $('#updateMemo_violation').append('<option value="' + escapeHtml(g_checklistId) + '">' + escapeHtml(data[0].checklist_Details) + '</option>');

                $('#updateMemo_response').html(data[0].obS_REPLY || '');
                $('#updateMemo_evidences').empty();
                if (data[0].attacheD_EVIDENCES && data[0].attacheD_EVIDENCES.length > 0) {
                    $.each(data[0].attacheD_EVIDENCES, function (i, pp) {
                        var extension = pp.imagE_NAME.split('.').pop().toLowerCase();
                        const container = document.createElement('div');
                        container.className = 'evidence-link';

                        const icon = document.createElement('i');
                        icon.className = getIconClass(extension) + ' evidence-icon mr-1';
                        container.appendChild(icon);

                        const label = document.createElement('span');
                        label.innerText = pp.imagE_NAME;
                        label.classList.add('text-primary');
                        label.style.cursor = 'pointer';
                        container.appendChild(label);

                        container.addEventListener('click', function () {
                            downloadFile(pp.filE_ID);
                        });

                        $('#updateMemo_evidences').append(container);
                    });
                }
                else {
                    $('#updateMemo_evidences').append('<i>No evidence is attached </i>');
                }

                if (typeof window.initObservationReference === 'function') {
                    window.initObservationReference('#updateObservationReferenceSection', {
                        editMode: true,
                        allowClear: false,
                        forceReload: true,
                        initialRefId: getObservationReferenceId(data[0])
                    });
                }
                var engId = syncEngagementContext();
                respSectionUpdate.updateContext({ newParaId: obs_id, engId: engId });
                showActionButtons();
                applyObservationEditMode();
            },
            dataType: "json",
        });

    }
    function showActionButtons() {
        $('#dropButton_update').addClass('d-none');
        $('#submitAuditeeButton_update').addClass('d-none');
        $('#addDraftButton_update').addClass('d-none');
        $('#settleButton_update').addClass('d-none');

        if (g_currentStatus == 1) {
            if (isSelectedEngagementTeamLead()) {
                $('#dropButton_update').removeClass('d-none');
                $('#submitAuditeeButton_update').removeClass('d-none');
            }
        } else if (g_currentStatus == 3) {
            if (isSelectedEngagementTeamLead()) {
                $('#addDraftButton_update').removeClass('d-none');
            }
            if (isSelectedEngagementTeamLead() && g_riskId == 3) {
                $('#settleButton_update').removeClass('d-none');
            }
        }
    }
    function finalCommentsButtonSave() {
        if (!canModifySelectedObservation()) {
            return;
        }

        preserveTablePosition();
        if (g_newStatusId == 5 && $('#draftNoInCommentsBox').val() == "") {
            alert("Please enter Draft Para No to proceed");
            return;
        }
        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Auditor Comments are Mandatory");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + (g_newStatusId == 5 ? "/ApiCalls/AddObservationToDraft" : "/ApiCalls/update_observation_status"),
            type: "POST",
            data: g_newStatusId == 5 ? {
                'ObservationId': g_obsId,
                'DraftParaNumber': $('#draftNoInCommentsBox').val(),
                'Remarks': $('#commentAreaInCommentsBox').val()
            } : {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'DRAFT_PARA_NO': $('#draftNoInCommentsBox').val(),
                'RISK_ID': g_riskId,
                'AUDITOR_COMMENT': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                if (data && (data.Status === true || data.status === true)) {
                    reloadCurrentStepLocally();
                    $('#commentsBox').modal('hide');
                    $('#updateMemoModel').modal('hide');
                }
            },
            error: function (xhr) {
                xhr.__iasSafetyHandled = true;
                showApiAlertFromXhr(xhr, xhr.status, getErrorReferenceIdFromXhr(xhr), "Unable to update observation status.");
            },
            dataType: "json",
        });
    }
    function updateObservationStatus(obs_id, new_status_id, risk_id) {
        if (!canModifySelectedObservation()) {
            return;
        }

        preserveTablePosition();
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $('#updateMemoModel').one('hidden.bs.modal', function () {
            $('#commentsBox').modal('show');
            $('#commentAreaInCommentsBox').val('');
            if (g_newStatusId == 4) {
                $('#draftNoInCommentsBox').val(0);
                $('#draftNoInCommentsBox').attr("disabled", true);
            } else {
                $('#draftNoInCommentsBox').val('');
                $('#draftNoInCommentsBox').attr("disabled", false);
            }
        }).modal('hide');
    }
    function dropObservation(obs_id, new_status_id, risk_id) {
        if (!canModifySelectedObservation()) {
            return;
        }

        preserveTablePosition();
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/drop_observation",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'NEW_STATUS_ID': g_newStatusId,
                'RISK_ID': g_riskId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                reloadCurrentStepLocally();
            },
            dataType: "json",
        });
    }
    function submitObservationToAuditee(obs_id, new_status_id, risk_id) {
        if (!canModifySelectedObservation()) {
            return;
        }

        preserveTablePosition();
        g_obsId = obs_id;
        g_newStatusId = new_status_id;
        g_riskId = risk_id;

        if (g_dsa === "Y" || g_annexId == 1) {
            // Close the viewer modal and, once completely hidden,
            // open the DSA modal to avoid multiple backdrops and
            // ensure the DSA modal has focus.
            $('#updateMemoModel').one('hidden.bs.modal', function () {
                $('#DSAModel').modal('show');
            }).modal('hide');

            $.each(g_obsList, function (i, v) {
                if (v.obS_ID == obs_id) {
                    $('#dsaHeading').val(v.heading || '');

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

                            if (data[0].responsiblE_PPs && data[0].responsiblE_PPs.length > 0) {
                                $.each(data[0].responsiblE_PPs, function (j, pp) {
                                    var srNo = $('#dsaResponsibles tbody tr').length + 1;
                                    var ppNo = parseInt(pp.pP_NO, 10) || 0;
                                    var respRowId = parseInt(pp.resP_ROW_ID, 10) || 0;
                                    $('#dsaResponsibles tbody').append('<tr id="tr_' + ppNo + '"><td>' + srNo + '</td><td>' + escapeHtml(pp.pP_NO) + '</td><td>' + escapeHtml(pp.emP_NAME) + '</td><td>' + escapeHtml(pp.loaN_CASE) + '</td><td>' + escapeHtml(pp.lC_AMOUNT) + '</td><td>' + escapeHtml(pp.accounT_NUMBER) + '</td><td>' + escapeHtml(pp.acC_AMOUNT) + '</td><td><input class="chk_dsaissued" resp_row_id="' + respRowId + '" id="' + ppNo + '" type="checkbox" /></td></tr>');
                                });
                            }
                        },
                        dataType: "json",
                    });
                }
            });
        } else {
            finalSubmissionParasToAuditee();
        }
    }
    function submitObservationToAuditeeAfterDSAIssuance(){
        if (!canModifySelectedObservation()) {
            return;
        }

        var dsaArr=[];

        $.each($('.chk_dsaissued'), function(i,v){
            if($(v).is(":checked"))
            {
                dsaArr.push({"RESP_ROW_ID":$(v).attr("resp_row_id"),"RESP_PP_NO":$(v).attr("id")});
            }
    });

    if(dsaArr.length==0){
        alert("This Observation is marked with A-1 annexure, therefore, please select at least one responsible from the list to issue DSA");
        return false;
    }

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/submit_dsa_to_auditee",
                type: "POST",
                data: {
                    'OBS_ID': g_obsId,
                    'ENTITY_ID': g_entityID,
                    'ENG_ID': syncEngagementContext(),
                    "RespDSAModel":dsaArr
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    finalSubmissionParasToAuditee();
                    $('#DSAModel').modal('hide');
                    $('#updateMemoModel').modal('hide');
                    $('#submitAuditeeButton_update').removeAttr('disabled');
                },
                dataType: "json",
            });

    }
    function finalSubmissionParasToAuditee(){
         if (!canModifySelectedObservation()) {
             return;
         }

         preserveTablePosition();
         $('#submitAuditeeButton_update').attr('disabled', 'disabled');
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/submit_observation_to_auditee",
                type: "POST",
                data: {
                    'OBS_ID': g_obsId,
                    'NEW_STATUS_ID': g_newStatusId,
                    'RISK_ID': g_riskId
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    reloadCurrentStepLocally();
                    $('#DSAModel').modal('hide');
                    $('#updateMemoModel').modal('hide');
                    $('#submitAuditeeButton_update').removeAttr('disabled');
                },
                dataType: "json",
            });
    }
    function finalUpdateMemoContent(obs_id) {
        if (!canModifySelectedObservation()) {
            return;
        }

        preserveTablePosition();
        g_obsId = obs_id;
        updateRiskDisplay();
        if (!validateUpdateObservationHeading(true)) {
            return;
        }

        var observationHeading = getUpdateObservationHeading();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_text",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'OBS_TITLE': observationHeading,
                'OBS_TEXT': $('.richText-editor').html(),
                'ANNEXURE_ID': $('#updateMemo_annex').val() || g_annexId,
                'RISK_ID': g_selectedRiskId || g_riskId,
                'PROCESS_ID': $('#updateMemo_process').val() || g_processId,
                'SUBPROCESS_ID': $('#updateMemo_subprocess').val() || g_subProcessId,
                'CHECKLIST_ID': $('#updateMemo_violation').val() || g_checklistId,
                'REFERENCE_ID': getStep6SelectedReferenceId()
            },
            cache: false,
            success: function (data) {
                if (typeof window.commitObservationReferenceSelection === 'function') {
                    window.commitObservationReferenceSelection('#updateObservationReferenceSection');
                }
                showApiAlert(data);
                reloadCurrentStepLocally();
            },
            dataType: "json",
        });

    }
    function openResponsiblePPs() {
        if (!canModifySelectedObservation()) {
            return;
        }

        $('#ResponsiblePPModel').modal('show');
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

    window.fieldAuditStepInitializers = window.fieldAuditStepInitializers || {};
    window.fieldAuditStepInitializers.MANAGE_OBSERVATION_BRANCHES = initManageObservationBranches;

    if (document.readyState !== 'loading') {
        initManageObservationBranches();
    } else {
        $(initManageObservationBranches);
    }

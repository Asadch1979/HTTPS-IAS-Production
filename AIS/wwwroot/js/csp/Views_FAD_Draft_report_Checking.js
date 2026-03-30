(function () {
    if (typeof g_boDraftReadOnlyMode !== 'undefined') {
        g_boDraftReadOnlyMode = true;
    }

    function lockDraftReportReadOnlyUi() {
        if (typeof g_boDraftReadOnlyMode !== 'undefined') {
            g_boDraftReadOnlyMode = true;
        }

        $('button[data-onclick="submitPreConcluding();"], #finalCommentsButtonSave, #update_audit_obs_button, #un_settle_audit_obs_button, #settle_audit_obs_button, #dsa_audit_obs_button, #addResponsibleButton, #updateResponsibleButton, #deleteResponsibleButton').addClass('d-none');
        $('#viewMemoDetailsModel [data-onclick="openResponsiblePPs();"]').addClass('d-none');
        $('#ResponsiblePPModel [data-onclick="respSection.getLCDetails();"], #ResponsiblePPModel [data-onclick="respSection.getMatchedPP();"]').addClass('d-none');
        $('#DSAModel button[data-onclick="submitObservationToAuditeeAfterDSAIssuance();"]').addClass('d-none');

        $('#update_listofRespPersons th:nth-child(9), #update_listofRespPersons th:nth-child(10), #update_listofRespPersons td:nth-child(9), #update_listofRespPersons td:nth-child(10)').hide();

        $('#viewMemoDetailsModel input:not([type="hidden"]), #viewMemoDetailsModel textarea, #commentsBox input:not([type="hidden"]), #commentsBox textarea').prop('readonly', true).prop('disabled', true);
        $('#viewMemoDetailsModel select, #commentsBox select').prop('disabled', true);
        $('#viewMemoDetailsModel .richText-editor').attr('contenteditable', 'false');

        $('#ResponsiblePPModel input:not([type="hidden"]), #ResponsiblePPModel textarea, #DSAModel input:not([type="hidden"]), #DSAModel textarea').prop('readonly', true).prop('disabled', true);
        $('#ResponsiblePPModel select, #DSAModel select').prop('disabled', true);
        $('#DSAModel input[type="checkbox"]').prop('disabled', true);
        $('#ResponsiblePPModel button').not('[data-bs-dismiss="modal"]').not('.btn-close').addClass('d-none');
    }

    $(function () {
        lockDraftReportReadOnlyUi();

        $('#viewMemoDetailsModel, #ResponsiblePPModel, #commentsBox, #DSAModel').on('shown.bs.modal', function () {
            window.setTimeout(lockDraftReportReadOnlyUi, 0);
            window.setTimeout(lockDraftReportReadOnlyUi, 200);
        });

        $(document).ajaxComplete(function () {
            if ($('#viewMemoDetailsModel').hasClass('show') || $('#ResponsiblePPModel').hasClass('show') || $('#commentsBox').hasClass('show') || $('#DSAModel').hasClass('show')) {
                window.setTimeout(lockDraftReportReadOnlyUi, 0);
            }
        });
    });
})();

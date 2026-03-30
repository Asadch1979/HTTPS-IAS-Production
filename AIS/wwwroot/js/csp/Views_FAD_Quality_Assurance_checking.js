(function () {
    if (typeof g_boPreConReadOnlyMode !== 'undefined') {
        g_boPreConReadOnlyMode = true;
    }

    function lockQualityAssuranceReadOnlyUi() {
        if (typeof g_boPreConReadOnlyMode !== 'undefined') {
            g_boPreConReadOnlyMode = true;
        }

        $('#preConcludingActionHandler, #gist_recom_inc_pre_con, #update_audit_obs_button, #saveBtn, #addResponsibleButton, #updateResponsibleButton, #deleteResponsibleButton').addClass('d-none');
        $('#viewMemoDetailsModel [data-onclick="openResponsiblePPs();"]').addClass('d-none');
        $('#ResponsiblePPModel [data-onclick="respSection.getLCDetails();"], #ResponsiblePPModel [data-onclick="respSection.getMatchedPP();"]').addClass('d-none');

        $('#update_listofRespPersons th:nth-child(9), #update_listofRespPersons th:nth-child(10), #update_listofRespPersons td:nth-child(9), #update_listofRespPersons td:nth-child(10)').hide();

        $('#viewMemoDetailsModel input:not([type="hidden"]), #viewMemoDetailsModel textarea').prop('readonly', true).prop('disabled', true);
        $('#viewMemoDetailsModel select').prop('disabled', true);
        $('#viewMemoDetailsModel .richText-editor').attr('contenteditable', 'false');

        $('#ResponsiblePPModel input:not([type="hidden"]), #ResponsiblePPModel textarea').prop('readonly', true).prop('disabled', true);
        $('#ResponsiblePPModel select').prop('disabled', true);
        $('#ResponsiblePPModel button').not('[data-bs-dismiss="modal"]').not('.btn-close').addClass('d-none');
    }

    $(function () {
        lockQualityAssuranceReadOnlyUi();

        $('#viewMemoDetailsModel, #ResponsiblePPModel').on('shown.bs.modal', function () {
            window.setTimeout(lockQualityAssuranceReadOnlyUi, 0);
            window.setTimeout(lockQualityAssuranceReadOnlyUi, 200);
        });

        $(document).ajaxComplete(function () {
            if ($('#viewMemoDetailsModel').hasClass('show') || $('#ResponsiblePPModel').hasClass('show')) {
                window.setTimeout(lockQualityAssuranceReadOnlyUi, 0);
            }
        });
    });
})();

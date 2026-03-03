    var g_trId = 0;
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfProcTransactions tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
    function reloadLocation() {
        window.location = window.location.pathname;
    }

    function recommendReferProcTransaction(id) {
        g_trId = id;
        $('#recommendReferProcTransactionModal').modal('show');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_checklist_detail_comparison_by_Id",
            type: "POST",
            data: {
                'CHECKLIST_DETAIL_ID': id
            },
            cache: false,
            success: function (data) {
                var tr = data[0];
                $('#viewerMainProcNameModalField').val(tr.process);
                $('#viewerNameModalField').val(tr.neW_SUB_PROCESS);
                $('#viewerDescModalField').val(tr.neW_PROCESS_DETAIL);
                $('#viewerViolationNameModalField').val(tr.neW_VIOLATION);
                $('#viewerControlModalField').val(tr.neW_FUNCTIONAL_RESP);
                $('#viewerDivModalField').val(tr.neW_ROLE_RESP);
                $('#viewerRiskModalField').val(tr.neW_RISK);
                $('#viewerAnnexModalField').val(tr.neW_ANNEXURE);

                $('#oldProcessLabel').val(tr.process);
                $('#oldViolationLabel').val(tr.violation);
                $('#oldProcessOwnerLabel').val(tr.functionaL_RESP);
                $('#oldRoleRespLabel').val(tr.rolE_RESP);
                $('#oldRiskLabel').val(tr.risk);
                $('#oldAnnexureLabel').val(tr.annexure);
                $('#oldProcessDetailLabel').val(tr.procesS_DETAIL);
                $('#oldSubProcessLabel').val(tr.suB_PROCESS);

            },
            dataType: "json",
        });
    }
    function recommendProcTransaction() {
        $('#commentsBox').modal('show');
        $('#finalCommentsButtonSave').attr('onclick', 'finalRecommendProcessTransaction()');

    }
    function refferedBackProcTransaction() {
        $('#commentsBox').modal('show');
        $('#finalCommentsButtonSave').attr('onclick', 'finalRefferedBackProcessTransaction()');

    }
    function finalRecommendProcessTransaction() {

        $.ajax({
            url: g_asiBaseURL + "/Setup/authorize_process_transaction_by_authorizer",
            type: "POST",
            data: {
                'T_ID': g_trId,
                'COMMENTS': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                $('#recommendReferProcTransactionModal').modal('hide'); 
                $('#commentsBox').modal('hide');
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    
    function finalRefferedBackProcessTransaction() {
        $.ajax({
            url: g_asiBaseURL + "/Setup/reffered_back_process_transaction_by_authorizer",
            type: "POST",
            data: {
                'T_ID': g_trId,
                'COMMENTS': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                $('#recommendReferProcTransactionModal').modal('hide');
                $('#commentsBox').modal('hide');
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }

    var g_trId = 0;
    var g_sId = 0;
    var g_dId = 0;
    var g_pId = 0;
    var g_selectedChecklistDetailRecord = {};
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfProcTransactions tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });


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
                g_sId = tr.n_S_ID;
                g_selectedChecklistDetailRecord = data[0];
                 $('#viewerMainProcNameModalField').val(tr.process);
                $('#viewerSubProcNameModalField').val(tr.neW_SUB_PROCESS);
                $('#processDetailNewField').val(tr.neW_PROCESS_DETAIL);  

                $('#updatedViolationlist').val(tr.n_V_ID);
                $('#updatedProcOwnerlist').val(tr.n_OWNER_ID);
                $('#updatedRoleResplist').val(tr.n_ROLE_RESP_ID);
                $('#updatedRisklist').val(tr.n_RISK_ID);
                $('#updatedAnnexlist').val(tr.n_ANNEX_ID);    
                
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
        function toInt(value) {
            var parsed = parseInt(value, 10);
            return isNaN(parsed) ? 0 : parsed;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/recommend_process_transaction_by_reviewer",
            type: "POST",
            data: {
                'T_ID': toInt(g_trId),
                'COMMENTS': $('#commentAreaInCommentsBox').val(),               
                'SUB_PROCESS_ID': toInt(g_sId || g_selectedChecklistDetailRecord.n_S_ID),
                'PROCESS_DETAIL_ID': toInt(g_selectedChecklistDetailRecord.n_D_ID),
                'HEADING': $('#processDetailNewField').val(),
                'V_ID': toInt($('#updatedViolationlist').val()),
                'CONTROL_ID': toInt($('#updatedProcOwnerlist').val()),
                'ROLE_ID': toInt($('#updatedRoleResplist').val()),
                'RISK_ID': toInt($('#updatedRisklist').val()),
                'ANNEX_CODE': $('#updatedAnnexlist').val()
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
            url: g_asiBaseURL + "/ApiCalls/reffered_back_process_transaction_by_reviewer",
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

    function reloadLocation(){
        window.location = window.location.pathname;
    }

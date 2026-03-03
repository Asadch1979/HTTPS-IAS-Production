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
            url: g_asiBaseURL + "/ApiCalls/get_sub_checklist_comparison_by_Id",
            type: "POST",
            data: {
                'SUB_PROCESS_ID': id
            },
            cache: false,
            success: function (data) {

                var v = data[0];
                $('#oldProcessLabel').val(v.procesS_NAME);
                $('#oldSubProcessLabel').val(v.suB_PROCESS_NAME);

                $('#viewerMainProcNameModalField').val(v.neW_SUB_PROCESS_NAME);
                $('#viewerNameModalField').val(v.neW_PROCESS_NAME);
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
            url: g_asiBaseURL + "/ApiCalls/authorize_sub_process_by_authorizer",
            type: "POST",
            data: {
                'T_ID': g_trId,
                'COMMENTS': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                $('#recommendReferProcTransactionModal').modal('hide');
                showApiAlert(data);
                onAlertCallback(reloadLocation);
               
            },
            dataType: "json",
        });
    }
    function finalRefferedBackProcessTransaction() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/reffered_back_sub_process_by_authorizer",
            type: "POST",
            data: {
                'T_ID': g_trId,
                'COMMENTS': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                $('#recommendReferProcTransactionModal').modal('hide');
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }

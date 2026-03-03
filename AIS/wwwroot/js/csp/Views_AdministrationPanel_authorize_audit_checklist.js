    var g_trId = 0;
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
            url: g_asiBaseURL + "/Setup/process_transactions",
            type: "POST",
            data: {
                'transactionId': id
            },
            cache: false,
            success: function (data) {
                var tr = data[0];
                $('#viewerMainProcNameModalField').val(tr.procesS_NAME);
                $('#viewerNameModalField').val(tr.suB_PROCESS_NAME);
                $('#viewerDescModalField').val(tr.description);
                $('#viewerControlModalField').val(tr.controL_OWNER);
                $('#viewerDivModalField').val(tr.diV_NAME);
                $('#viewerActionModalField').val(tr.action);
                if (tr.risK_WEIGHTAGE == 3)
                    $('#viewerRiskModalField').val('High');
                if (tr.risK_WEIGHTAGE == 3)
                    $('#viewerRiskModalField').val('Medium');
                else
                    $('#viewerRiskModalField').val('Low');
                $('#viewerRiskMaxModalField').val(tr.risK_MAX_NUMBER);
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
            url: g_asiBaseURL + "/Setup/recommend_process_transaction_by_authorizer",
            type: "POST",
            data: {
                'T_ID': g_trId,
                'COMMENTS': $('#commentAreaInCommentsBox').val()
            },
            cache: false,
            success: function (data) {
                $('#recommendReferProcTransactionModal').modal('hide');
                //console.log(data);
                window.location = window.location.pathname;
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
                //console.log(data);
                window.location = window.location.pathname;
            },
            dataType: "json",
        });
    }

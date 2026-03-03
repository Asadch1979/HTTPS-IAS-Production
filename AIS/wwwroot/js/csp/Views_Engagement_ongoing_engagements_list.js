    var g_engId=0;
    $('#document').ready(function () {
    });
    function referredBackEngagementPlan(engId) {
        g_engId = engId;
        $('#commentsBox').modal('show');
    }
      function reloadLocation() {
        location.reload();
    }
    function finalReferredBackEngagementPlan() {
         if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Enter Comments to Proceed"); return false;
        }
        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/reject_engagement_plan",
                type: "POST",
                data: {
                    'ENG_ID': g_engId,
                    'COMMENTS': $('#commentAreaInCommentsBox').val()
                },
                cache: false,
                success: function (data) {
                    alert("Successfully rejected Engagement Plan");
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });

    }
    function approveEngagementPlan(engId) {
        g_engId=engId;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/approve_engagement_plan",
                type: "POST",
                data: {
                    'ENG_ID': g_engId
                },
                cache: false,
                success: function (data) {
                    alert("Successfully approved Engagement Plan");
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
    }

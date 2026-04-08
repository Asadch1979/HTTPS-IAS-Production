    var g_engId=0;
    $('#document').ready(function () {
    });
    function referredBackEngagementPlan(engId) {
        g_engId = engId;
        $('#commentsBox').modal('show');
    }
      function reloadLocation() {
        if (window.planningDashboard && typeof window.planningDashboard.reloadCurrentStep === 'function') {
            window.planningDashboard.reloadCurrentStep();
            return;
        }

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
                    createSampleRecord();
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
    }
    function createSampleRecord(){
          $.ajax({
            url: g_asiBaseURL + "/ApiCalls/create_engagement_sample_data",
                type: "POST",
                data: {
                    'ENG_ID': g_engId
                },
                cache: false,
                success: function (data) {
                  createExceptionRecord();
                  onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
    }

        function createExceptionRecord(){
          $.ajax({
            url: g_asiBaseURL + "/ApiCalls/create_engagement_Exception_data",
                type: "POST",
                data: {
                    'ENG_ID': g_engId
                },
                cache: false,
                success: function (data) {

                },
                dataType: "json",
            });
    }

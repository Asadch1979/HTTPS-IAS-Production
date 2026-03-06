  
    var g_preCreatedPlans=[];
    var g_planId=0;
   
    $(document).ready(function () {
        getSavedSpecialAuditPlan();
    });

    function getSavedSpecialAuditPlan() {
        $('#auditCriteriaListBox tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_saved_special_audit_plans",
            type: "POST",
            data: {

            },

            cache: false,
            success: function (data) {
                g_preCreatedPlans=data;
                $.each(data, function (index, row) {
                    $('#auditCriteriaListBox tbody').append("<tr id=" + row.plaN_ID + "><td>" + ++index + "</td><td>" + row.nature + "</td><td>" + row.audiT_PERIOD + "</td><td>" + row.reportinG_OFFICE + "</td><td>" + row.entitY_NAME + "</td><td>" + row.nO_DAYS + "</td><td>" + row.auditeD_BY + "</td><td><a href=\"#\" class=\"text-danger\" data-click=\"event.preventDefault();ReferredBackCriteriaRecordFromGrid(" + row.plaN_ID + ");\">Referred Back</a></td><td><a href=\"#\" class=\"text-success\" data-click=\"event.preventDefault();ApprovalCriteriaRecordFromGrid(" + row.plaN_ID + ");\">Approve</a></td></tr>");
                });
            },
            dataType: "json",
        });

    }
  
    function ApprovalCriteriaRecordFromGrid(pId) 
    {
        g_planId=pId;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/approve_special_audit_plan",
            type: "POST",
            data: {
                'PLAN_ID': g_planId,
                'INDICATOR':'A'
            },
            cache: false,
            success: function (data) {
                  showApiAlert(data);
                getSavedSpecialAuditPlan();
            },

            dataType: "json",
        });
    }
    function ReferredBackCriteriaRecordFromGrid(pId) {
        g_planId=pId;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/referred_back_special_audit_plan",
            type: "POST",
            data: {
                'PLAN_ID': g_planId,
                'INDICATOR':'R'
            },
            cache: false,
            success: function (data) {
                 showApiAlert(data);
                getSavedSpecialAuditPlan();
            },

            dataType: "json",
        });
    }

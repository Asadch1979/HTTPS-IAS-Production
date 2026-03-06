    var g_status = 'Create';
    var g_preCreatedPlans=[];
    var g_planId=0;
    var g_indicator='N';
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

                    $('#auditCriteriaListBox tbody').append("<tr id=" + row.plaN_ID + "><td>" + ++index + "</td><td>" + row.nature + "</td><td>" + row.audiT_PERIOD + "</td><td>" + row.reportinG_OFFICE + "</td><td>" + row.entitY_NAME + "</td><td>" + row.nO_DAYS + "</td><td>" + row.auditeD_BY + "</td><td><a href=\"#\" class=\"text-primary\" data-onclick=\"event.preventDefault();UpdateCriteriaRecordFromGrid(" + row.plaN_ID + ");\">Edit</a></td><td><a href=\"#\" class=\"text-danger\" data-onclick=\"event.preventDefault();DeleteCriteriaRecordFromGrid(" + row.plaN_ID + ");\">Delete</a></td><td><a href=\"#\" class=\"text-success\" data-onclick=\"event.preventDefault();SubmitCriteriaRecordFromGrid(" + row.plaN_ID + ");\">Submit</a></td></tr>");
                });
            },
            dataType: "json",
        });

    }
    function getAuditableEntities(userEntityId = 0) {
        $('#auditCriteriaEntField').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: {
                'E_R_ID': $('#auditCriteriaRepField option:selected').val()
            },


            cache: false,
            success: function (data) {
                $('#auditCriteriaEntField').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
                $.each(data, function (index, gpp) {

                    var selected = '';
                    if (gpp.entitY_ID == userEntityId)
                        selected = 'selected="selected"';
                    $('#auditCriteriaEntField').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>')
                });
            },
            dataType: "json",
        });

    }
    function addRecordToauditCriteriaListBox() {
        var nature = 0;
        if ($('#auditNatureEntityField option:selected').val() != 0)
            nature = $('#auditNatureEntityField option:selected').val();

        var period = 0;
        if ($('#auditCriteriaPeriodField option:selected').val() != 0)
            period = $('#auditCriteriaPeriodField option:selected').val();

        var entityId = 0;
        if ($('#auditCriteriaEntField option:selected').val() != 0) {
            entityId = $('#auditCriteriaEntField option:selected').val();
        }

        var days = '';
        if ($('#auditCriteriaDaysField').val() != '')
            days = $('#auditCriteriaDaysField').val();

        if (nature == 0) {
            alert('Select Audit Nature to proceed');
            return;
        }

        if (period == 0) {
            alert('Select Audit Period to proceed');
            return;
        }
        if (entityId == 0) {
            alert('Select Audit Entity to proceed');
            return;
        }
        if (days == '') {
            alert('Enter No. of days to proceed');
            return;
        }

        var plan_id=0;
        var ind='N';

        if(g_status=="Update"){
            plan_id=g_planId;
            ind=g_indicator;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_special_audit_plan",
            type: "POST",
            data: {

                'NATURE': $('#auditNatureEntityField').val(),
                'PERIOD': $('#auditCriteriaPeriodField').val(),
                'ENTITY_ID': $('#auditCriteriaEntField').val(),
                'NO_DAYS': $('#auditCriteriaDaysField').val(),
                'PLAN_ID': plan_id,
                'INDICATOR': ind
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                g_status='Create';
                $('#saveButtonSpecialPlan').html('Save');
                getSavedSpecialAuditPlan();
                   clearPlanFields();
            },

            dataType: "json",
        });


    }

    function clearPlanFields(){
        $('#auditNatureEntityField').val(0);
                 $('#auditCriteriaPeriodField').val(0);
                 $('#auditCriteriaRepField').val(0);
                 getAuditableEntities();
                 $('#auditCriteriaDaysField').val(0);
    }
    function UpdateCriteriaRecordFromGrid(pId) {
        g_planId=pId;
        g_status="Update";
        g_indicator="E";
        $('#saveButtonSpecialPlan').html('Update');
        $.each(g_preCreatedPlans,function(i,v){
            if(v.plaN_ID==g_planId){
                $('#auditNatureEntityField').val(v.naturE_ID);
                 $('#auditCriteriaPeriodField').val(v.audiT_PERIOD_ID);
                 $('#auditCriteriaRepField').val(v.reportinG_OFFICE_ID);
                 getAuditableEntities(v.entitY_ID);
                 $('#auditCriteriaDaysField').val(v.nO_DAYS);
            }
        })

    }


    function DeleteCriteriaRecordFromGrid(pId) 
    {
        g_planId=pId;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/delete_special_audit_plan",
            type: "POST",
            data: {
                'PLAN_ID': g_planId,
                'INDICATOR':'D'
            },
            cache: false,
            success: function (data) {
                  showApiAlert(data);
                g_status='Create';
                $('#saveButtonSpecialPlan').html('Save');
                getSavedSpecialAuditPlan();
                   clearPlanFields();
            },

            dataType: "json",
        });
    }
     function SubmitCriteriaRecordFromGrid(pId) 
    {
        g_planId=pId;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_special_audit_plan",
            type: "POST",
            data: {
                'PLAN_ID': g_planId,
                'INDICATOR':'S'
            },
            cache: false,
            success: function (data) {
                 showApiAlert(data);
                g_status='Create';
                $('#saveButtonSpecialPlan').html('Save');
                getSavedSpecialAuditPlan();
                   clearPlanFields();
            },

            dataType: "json",
        });
    }

    function reloadLocation() {
        location.reload();
    }

    function addAuditEntitiesModal() {
        $('#setupAuditEntitiesModal').modal('show');
    }

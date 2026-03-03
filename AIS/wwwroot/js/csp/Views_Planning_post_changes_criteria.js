    var g_trId = 0;
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listofAuditCriterias tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });


    function recommendPostChangesAuditCriterias(id, period_id, ent_id, size_id, risk_id, freq_id, visit, nodays,entId,entName) {
        g_trId = id;


        $('#recommendPostChangesAuditCriteriasModal').modal('show');

        if (ent_id == 25) {
            $('#auditCriteriaCADHUBField').append('<option selected="selected" id="' + entId + '">' + entName + '</option>');
            $('#CADHUBPanel').removeClass('d-none');
            $('#nonCADHUBPanel').addClass('d-none');
        } else {
            $('#CADHUBPanel').addClass('d-none');
            $('#nonCADHUBPanel').removeClass('d-none');
        }
        $('#auditCriteriaPeriodField').val(period_id);
        $('#auditCriteriaEntityField').val(ent_id);
        $('#auditCriteriaRiskField').val(risk_id);
        $('#auditCriteriaSizeField').val(size_id);
        $('#auditCriteriaFreqField').val(freq_id)
        $('#auditCriteriaDaysField').val(nodays);
        if (visit == 'Y')
            $('#auditCriteriaVisitField').attr('checked', true);
        else
            $('#auditCriteriaVisitField').attr('checked', false);
    }
    function recommendProcTransaction() {
        $('#commentsBox').modal('show');
        $('#finalCommentsButtonSave').attr('onclick', 'finalRecommendProcessTransaction()');
    }

    function reloadLocation() {
        location.reload();
    }

    function finalRecommendProcessTransaction() {
        if ($('#commentAreaInCommentsBox').val() == "") {
            alert("Enter Comments to Proceed"); return false;
        }

        var entity = '';
        if ($('#auditCriteriaEntityField option:selected').val() != 0)
            entity = $('#auditCriteriaEntityField option:selected').val();

        var period = '';
        if ($('#auditCriteriaPeriodField option:selected').val() != 0)
            period = $('#auditCriteriaPeriodField option:selected').val();

        var days = $('#auditCriteriaDaysField').val();
        var risk = '';
        if ($('#auditCriteriaRiskField option:selected').val() != 0)
            risk = $('#auditCriteriaRiskField option:selected').val();

        var freq = '';
        if ($('#auditCriteriaFreqField option:selected').val() != 0)
            freq = $('#auditCriteriaFreqField option:selected').val();

        var size = '';
        if ($('#auditCriteriaSizeField option:selected').val() != 0)
            size = $('#auditCriteriaSizeField option:selected').val();

        var visit = 'N';
        if ($('#auditCriteriaVisitField').is(':checked'))
            visit = "Y";
        var criteria_details = [];
        criteria_details.push(g_trId);
        criteria_details.push(period);
        criteria_details.push(entity);
        criteria_details.push(risk);
        criteria_details.push(freq);
        criteria_details.push(size);
        criteria_details.push(days);
        criteria_details.push(visit);
        criteria_details.push($('#commentAreaInCommentsBox').val());

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/PostChangesAuditCriteria",
            type: "POST",
            data: {
                'CRITERIA_LIST': criteria_details
            },
            cache: false,
            success: function (data) {
                $('#recommendPostChangesAuditCriteriasModal').modal('hide');
                alert('Post Changes in Audit Criteria successfuly done');
                onAlertCallback(reloadLocation);
                
            },
            dataType: "json",
        });
    }

    function generatePlanAuditCriterias(id) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GeneratePlanAuditCriteria",
            type: "POST",
            data: {
                'CRITERIA_ID': id
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }

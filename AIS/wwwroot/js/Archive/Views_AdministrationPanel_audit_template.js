    $('#document').ready(function () {
        $('#template_box').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
    });

    $(document).ready(function () {

        $('#sidebar_policy').hide();
    });

    function div_risksubcategoryShowHide() {
        if ($('#riskGroupSelectBox option:selected').val() == 0) {
            $('#div_risksubcategory').hide();
            $('#div_activityContainer').hide();
        }
        else {
            $('#div_risksubcategory').show();
            $('#div_activityContainer').hide();
            $('#riskSubGroupSelectBox').empty();
            $.ajax({
                url: g_asiBaseURL + "/Execution/risk_sub_group",
                type: "POST",
                data: {
                    'GR_ID': $('#riskGroupSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#riskSubGroupSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        $('#riskSubGroupSelectBox').append("<option value=\"" + item.s_GR_ID + "\"> " + item.description + "</option>");
                    });

                },
                dataType: "json",
            });
        }
    }
    function div_activityContainerShowHide() {
        if ($('#riskSubGroupSelectBox option:selected').val() == 0)
            $('#div_activityContainer').hide();
        else
            $('#div_activityContainer').show();
        $('#riskActivitiesSelectBox').empty();
        $.ajax({
            url: g_asiBaseURL + "/Execution/risk_activities",
            type: "POST",
            data: {
                'S_GR_ID': $('#riskSubGroupSelectBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                $('#riskActivitiesSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                $.each(data, function (index, item) {
                    $('#riskActivitiesSelectBox').append("<option value=\"" + item.activitY_ID + "\"> " + item.description + "</option>");
                });

            },
            dataType: "json",
        });
    }
    function getAuditObservationTemplate() {
        if ($('#riskActivitiesSelectBox option:selected').val() == 0)
            $('#template_box').val('').trigger('change');
        else {
            $('#template_box').val('').trigger('change');
            $.ajax({
                url: g_asiBaseURL + "/Execution/audit_observation_template",
                type: "POST",
                data: {
                    'ACTIVITY_ID': $('#riskActivitiesSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, item) {
                        $('#template_box').val(item.obS_TEMPLATE).trigger('change');
                    });

                },
                dataType: "json",
            });
        }
    }

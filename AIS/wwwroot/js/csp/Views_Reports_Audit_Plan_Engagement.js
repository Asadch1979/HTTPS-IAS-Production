    var g_engArr = [];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#observation_panel tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    })


    function engagementPlanDetail(engId) {
        $('#engagementPlanDetailModel').modal('show');
        $('#engID_UpdateModelField').val('');
        $('#entName_UpdateModelField').val('');
        $('#RepOffice_UpdateModelField').val('');
        $('#audStart_UpdateModelField').val('');
        $('#audTeam_UpdateModelField').val('');
        $('#audEnd_UpdateModelField').val('');
        $('#opStart_UpdateModelField').val('');
        $('#opEnd_UpdateModelField').val('');
        $('#engStatus_UpdateModelField').val('');
        $('#discussionDays_UpdateModelField').val('');
        $('#weekendDays_UpdateModelField').val('');
        $('#revRecDays_UpdateModelField').val('');
        $('#travelDays_UpdateModelField').val('');
        $('#totalDays_UpdateModelField').val('');
        $.each(g_engArr, function (i, v) {
            if (v.enG_ID == engId) {
                $('#engID_UpdateModelField').val(v.enG_ID);
                $('#entName_UpdateModelField').val(v.entitY_NAME);
                $('#RepOffice_UpdateModelField').val(v.reportinG_OFFICE);
                $('#audTeam_UpdateModelField').val(v.audiT_TEAM);
                $('#audStart_UpdateModelField').val(v.audiT_START_DATE.split(" ")[0]);
                $('#audEnd_UpdateModelField').val(v.audiT_END_DATE.split(" ")[0]);
                $('#opStart_UpdateModelField').val(v.oP_START_DATE.split(" ")[0]);
                $('#opEnd_UpdateModelField').val(v.oP_END_DATE.split(" ")[0]);
                $('#engStatus_UpdateModelField').val(v.enG_STATUS);
                $('#travelDays_UpdateModelField').val(v.traveL_DAYS);
                $('#discussionDays_UpdateModelField').val(v.discussioN_DAYS);
                $('#weekendDays_UpdateModelField').val(v.weekenD_DAYS);
                $('#revRecDays_UpdateModelField').val(v.revenuE_RECORD_DAYS);
                $('#totalDays_UpdateModelField').val(v.totaL_DAYS);
            }
        });

    }

    function getAuditEngagementDetials() {

        if ($('#auditedByField').val() == "-1") {
            alert("Select Audited By Department");
            return;
        }

        if ($('#auditPeriodField').val() == 0) {
            alert("Select Audit Period");
            return;
        }

        $('#observation_panel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_plan_engagement_detailed_report",
            type: "POST",
            data: {
                'AUDITED_BY': $('#auditedByField').val(),
                'PERIOD_ID': $('#auditPeriodField').val()
            },
            cache: false,
            success: function (data) {
                g_engArr = data;
                $.each(data, function (i, v) {
                    $('#observation_panel tbody').append('<tr><td align="center"> ' + ++i + '</td><td  align="left">' + v.enG_ID + '</td><td  align="left">' + v.reportinG_OFFICE + '</td><td align="left">' + v.entitY_NAME + '</td><td align="left">' + v.audiT_START_DATE.split(" ")[0] + '</td><td align="left">' + v.audiT_END_DATE.split(" ")[0] + '</td><td align="left">' + v.totaL_DAYS + '</td><td align="left">' + v.delaY_DAYS + '</td><td align="left">' + v.enG_STATUS + '</td><td><small data-onclick="event.preventDefault();engagementPlanDetail(' + v.enG_ID + ')" class="text-danger cursor-pointer">Details</small></td></tr>');

                });

            },
            dataType: "json",
        });


    }

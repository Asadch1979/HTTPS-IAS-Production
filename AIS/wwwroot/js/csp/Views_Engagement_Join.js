    var g_joinRes = null;

    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var eng_id = url.searchParams.get("engId");

        $.ajax({
            url: g_asiBaseURL + "/Engagement/get_joining_details",
            type: "POST",
            data: {
                engId: eng_id
            },
            cache: false,
            dataType: "json",
            success: function (data) {
                g_joinRes = data;

                $('#ent_name_field').html(data.entitY_NAME);
                $('#ent_risk_field').html(data.risk);
                $('#ent_size_field').html(data.size);

                var today = new Date().toISOString().split("T")[0];
                var jDate = today.split('-')[2] + "/" + today.split('-')[1] + "/" + today.split('-')[0];

                var sDate = data.starT_DATE.split('T')[0];
                var sfromat = sDate.split('-')[2] + "/" + sDate.split('-')[1] + "/" + sDate.split('-')[0];

                var eDate = data.enD_DATE.split('T')[0];
                var efromat = eDate.split('-')[2] + "/" + eDate.split('-')[1] + "/" + eDate.split('-')[0];

                $('#ent_start_field').html(sfromat);
                $('#ent_end_field').html(efromat);
                $('#ent_team_name_field').html(data.teaM_NAME);
                $('#ent_period_field').html(data.audiT_PERIOD);

                $.each(data.teaM_DETAILS, function (i, v) {
                    $('#teamDetailsPanel tbody').append(
                        '<tr>' +
                        '<td>' + v.emP_NAME + '</td>' +
                        '<td>' + v.pP_NO + '</td>' +
                        '<td id="joiningDateFieldValue">' + jDate + '</td>' +
                        '<td id="completionDateFieldValue">' + efromat + '</td>' +
                        '<td>' + v.iS_TEAM_LEAD + '</td>' +
                        '</tr>'
                    );
                });
            }
        });
    });

    function toIsoDate(dmy) {
        if (!dmy) return null;

        var parts = dmy.split('/');
        if (parts.length !== 3) return null;

        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

    function publishJoinReport() {
        $.ajax({
            url: g_asiBaseURL + "/Engagement/add_joining_report",
            type: "POST",
            data: {
                ID: 0,
                ENG_PLAN_ID: g_joinRes.enG_PLAN_ID,
                TEAM_MEM_PPNO: g_joinRes.teaM_DETAILS[0].pP_NO,
                JOINING_DATE: toIsoDate($('#joiningDateFieldValue').text()),
                COMPLETION_DATE: toIsoDate($('#completionDateFieldValue').text())
            },
            cache: false,
            dataType: "json",
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(redirectToTaskList);
            }
        });
    }

    function redirectToTaskList() {
        window.location.href = g_asiBaseURL + "/Engagement/task_list";
    }

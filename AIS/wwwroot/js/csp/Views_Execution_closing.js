    var g_teamMembers = [];
    var g_engId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var eng_id = url.searchParams.get("engId");
        if (typeof eng_id != 'undefined')
            g_engId = eng_id;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/closing_draft_report_status",
            type: "POST",
            data: {
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {
                if(data.length>0)
                    $('#entityNameField').html(data[0].entitY_NAME);
                $.each(data, function (i, v) {
                    $('#joiningAuditorDetailsTable tbody').append('<tr><td>' + v.teaM_MEM_PPNO + '</td><td>' + v.membeR_NAME + '</td><td>' + v.joininG_DATE.split('T')[0] + '</td><td>' + v.completioN_DATE.split('T')[0] + '</td></tr>');
                    $('#auditorWiseProgessTable tbody').append('<tr><td class="text-center">' + v.membeR_NAME + '</td><td class="text-center">' + v.isteamlead + '</td><td class="text-center">' + v.totaL_NO_OB + '</td><td class="text-center">' + v.dropped + '</td><td class="text-center">' + v.submitteD_TO_AUDITEE + '</td><td class="text-center">' + v.responded + '</td><td class="text-center">' + v.resolved + '</td><td class="text-center">' + v.addeD_TO_DRAFT + '</td></tr>');
                });   
                },
            dataType: "json",
        });
        });
    function reloadLocationToUrl() {
        window.location.href = g_asiBaseURL + "/Engagement/task_list"
    }
    function closeDraftAudit() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/close_draft_audit",
            type: "POST",
            data: {
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocationToUrl);
            },
            dataType: "json",
        });
    }

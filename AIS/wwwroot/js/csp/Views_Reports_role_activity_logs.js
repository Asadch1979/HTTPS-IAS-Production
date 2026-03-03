    function getActivityLog() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_role_actuserivity_log",
            type: "POST",
            data: {
                'ROLE_ID': $('#roleGroupField').val(),
                'DEPT_ID': $('#deptField').val(),
                'AZ_ID': $('#azField').val(),
            },
            cache: false,
            success: function (data) {
                $('#userWiseActivityLog tbody').empty();
                var sr = 1;
                $.each(data, function (i, v) {
                    $('#userWiseActivityLog tbody').append('<tr><td>' + sr + '</td><td>' + v.useR_PP_NUMBER + '</td><td>' + v.useR_NAME + '</td><td>' + v.acT_DATE + '</td><td>' + v.activity + '</td><td>' + v.actions + '</td><td>' + v.duration + '</td></tr>');
                    sr++;
                });

            },
            dataType: "json",
        });
    }

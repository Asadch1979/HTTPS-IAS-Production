    function normalizeRequiredInt(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return 0;
        }
        var number = parseInt(trimmed, 10);
        return Number.isNaN(number) ? 0 : number;
    }

    function getActivityLog() {
         destroyDatatable('userWiseActivityLog');

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_user_activity_log",
            type: "POST",
            data: {
                'PP_NO': normalizeRequiredInt($('#ppNumField').val())
            },
            cache: false,
            success: function (data) {
                $('#userWiseActivityLog tbody').empty();
                var sr = 1;
                $.each(data, function (i, v) {
                    $('#userWiseActivityLog tbody').append('<tr><td>' + sr + '</td><td>' + v.useR_PP_NUMBER + '</td><td>' + v.useR_NAME + '</td><td>' + v.starT_DATE + '</td><td>' + v.enD_DATE + '</td><td>' + v.actions + '</td><td>' + v.duration + '</td></tr>');
                    sr++;
                });
                 initializeDataTable('userWiseActivityLog');

            },
            dataType: "json",
        });
    }

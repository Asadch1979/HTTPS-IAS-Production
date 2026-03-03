    $(document).ready(function () {
        GetCurrentActiveSessions();
    });
    function GetCurrentActiveSessions() {
        $('#CurrentActiveSessionsGrid tbody').empty();
        if ($('#auditDepartmentSelectBox').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_active_users",
                type: "POST",
                data: {
                   
                },
                cache: false,
                success: function (data) {
                    var sr = 1;
                    $.each(data, function (i, v) {
                        v.sessioN_TIME = v.sessioN_TIME.replace('.', ':');
                        v.sessioN_TIME = v.sessioN_TIME.split('.')[0];
                        v.sessioN_TIME = v.sessioN_TIME.split(':')[0] + ':' + v.sessioN_TIME.split(':')[1] + ':' + v.sessioN_TIME.split(':')[2];
                        v.loggeD_IN_DATE = v.loggeD_IN_DATE.replace('T', ' ');

                        $('#CurrentActiveSessionsGrid tbody').append('<tr><td class="text-center">' + sr + '</td><td>' + v.departmenT_NAME + '</td><td>' + v.name + '</td><td class="text-center">' + v.pP_NUMBER + '</td><td>' + v.loggeD_IN_DATE + '</td><td class="text-center">' + v.sessioN_TIME + '</td></tr>');
                        sr++;

                    });
                },
                dataType: "json",
            });
        }
        
       
    }

    function GetCurrentAuditProgress() {
        $('#CurrentAuditProgressGrid tbody').empty();
        if ($('#auditDepartmentSelectBox').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_current_audit_progress",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#auditDepartmentSelectBox').val()
                },
                cache: false,
                success: function (data) {
                    var sr = 1;
                    $.each(data, function (i, v) {

                        $('#CurrentAuditProgressGrid tbody').append('<tr><td>' + sr + '</td><td>' + v.name + '</td><td>' + v.area + '</td><td>' + v.obS_COUNT + '</td></tr>');
                        sr++;

                    });
                },
                dataType: "json",
            });
        }
        
       
    }

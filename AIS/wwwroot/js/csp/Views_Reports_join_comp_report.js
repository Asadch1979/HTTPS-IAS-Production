    function FindJoiningCompletionData() {
        $('#JoiningCompletionGrid tbody').empty();

        if ($('#auditDepartmentSelectBox').val()==0) {
            alert("Select Department/ Audit Cluster to proceed");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_joining_completion",
            type: "POST",
            data: {

                'DEPT_ID': $('#auditDepartmentSelectBox').val(),
                'AUDIT_STARTDATE': $('#auditStartDateField').val(),
                'AUDIT_ENDDATE': $('#auditEndDateField').val()
                },
            cache: false,
            success: function (data) {
                var sr = 1;
                $.each(data, function (i, v) {
                    v.joininG_DATE = v.joininG_DATE.split('T')[0];
                    var d, m, y, jd;

                    d = v.joininG_DATE.split('-')[2];
                    m = v.joininG_DATE.split('-')[1];
                    y = v.joininG_DATE.split('-')[0];
                    y = y.substring(2,4);
                    jd = d + '-' + m + '-' + y;               

                    v.completioN_DATE = v.completioN_DATE.split('T')[0];
                    var cd, cm, cy, cdd;
                    cd = v.completioN_DATE.split('-')[2];
                    cm = v.completioN_DATE.split('-')[1];
                    cy = v.completioN_DATE.split('-')[0];
                    cy = cy.substring(2, 4);
                    cdd = cd + '-' + cm + '-' + cy;
                    $('#JoiningCompletionGrid tbody').append('<tr><td>' + sr + '</td><td>' + v.audiT_BY + '</td><td>' + v.auditeE_NAME + '</td><td>' + v.teaM_NAME + '</td><td>' + v.ppno + '</td><td>' + v.name + '</td><td>' + v.teaM_LEAD + '</td><td>' + jd + '</td><td>' + cdd + '</td><td>' + v.status+'</td></tr>'); 
                    sr++;

                });
            },
            dataType: "json",
        });
    }

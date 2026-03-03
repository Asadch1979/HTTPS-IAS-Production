        var g_teamMembers = [];
        var g_engId = 0;
        var g_type_id = 0;

        $(document).ready(function(){

            $('#closeAuditReportButtonHandler').addClass('d-none');

        });

        function reloadLocationToUrl() {
            window.location.href = g_asiBaseURL + "/Engagement/task_list"
        }


        function getaddress() {

            $('#auditorWiseProgessTable tbody').empty();
            $('#joiningAuditorDetailsTable tbody').empty();
            g_engId = $('#entitySelectField').val();
            g_type_id = $('#entitySelectField option:selected').attr("ent_type_id");

            if(g_engId==0){
                $('#closeAuditReportButtonHandler').addClass('d-none');
                return;
            }

            $('#closeAuditReportButtonHandler').removeClass('d-none');

            $('#conoffice').empty();
            $('#EntN').empty();
            $('#ADD').empty();
            $('#a_s_d').empty();
            $('#a_e_d').empty();
            $('#o_s_d').empty();
            $('#o_e_d').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_address",
                type: "POST",
                data: {
                    'ENT_ID': g_engId
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                        console.log(v);
                        g_teamMembers.push(v.teaM_MEM_PPNO);
                        $('#conoffice').html(v.p_NAME);
                        $('#EntN').html(v.name);
                        $('#ADD').html(v.address);
                        $('#a_s_d').html(v.audiT_STARTDATE.split('T')[0]);
                        $('#a_e_d').html(v.audiT_ENDDATE.split('T')[0]);
                        $('#o_s_d').html(v.operatioN_STARTDATE.split('T')[0]);
                        $('#o_e_d').html(v.operatioN_ENDDATE.split('T')[0]);

                        $('#highTotalMemos').html(v.high);
                        $('#highTotalSettledMemos').html(v.settleD_HIGH);
                        $('#highTotalOpenMemos').html(v.opeN_HIGH);

                        $('#mediumTotalMemos').html(v.medium);
                        $('#mediumTotalSettledMemos').html(v.settleD_MEDIUM);
                        $('#mediumTotalOpenMemos').html(v.opeN_MEDIUM);

                        $('#lowTotalMemos').html(v.low);
                        $('#lowTotalSettledMemos').html(v.settleD_LOW);
                        $('#lowTotalOpenMemos').html(v.opeN_LOW);

                    });
                },
                dataType: "json",
            });

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetTeamDetails",
                type: "POST",
                data: {
                    'ENG_ID': g_engId
                },
                cache: false,
                success: function (data) {
                    console.log(data);
                    $.each(data, function (i, v) {
                        g_teamMembers.push(v.teaM_MEM_PPNO);
                        $('#joiningAuditorDetailsTable tbody').append('<tr><td class="text-center" >' + v.membeR_PPNO + '</td><td class="text-center" >' + v.membeR_NAME + '</td><td class="text-center" >' + v.isteamlead + '</td><td class="text-center" >' + v.audiT_START_DATE + '</td><td class="text-center" >' + v.audiT_END_DATE + '</td></tr>');

                    });

                },
                dataType: "json",
            });


        }

        function reloadLocation(){
            window.location.reload();
        }

      function closeDraftAudit() {

           $.ajax({
            url: g_asiBaseURL + "/ApiCalls/conclude_draft_audit",
            type: "POST",
            data: {
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);

            },
            dataType: "json",
        });
    }

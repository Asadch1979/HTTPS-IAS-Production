    var g_teamMembers = [];
    var g_engId = 0;
    var g_type_id = 0;
    $(document).ready(function () {
        $('#reportheading1').empty;
        $('#reportheading2').empty;

      

      
    });
    function reloadLocationToUrl() {
        window.location.href = g_asiBaseURL + "/Engagement/task_list"
    }



    function getaddress() {
        
        $('#auditorWiseProgessTable tbody').empty();
        $('#joiningAuditorDetailsTable tbody').empty();
        //getParas();
        g_engId = $('#entitySelectField').val();
        g_type_id = $('#entitySelectField option:selected').attr("ent_type_id");
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
                    // $('#joiningAuditorDetailsTable tbody').append('<tr><td class="text-center" style="font-size:xx-small">' + v.teaM_MEM_PPNO + '</td><td class="text-center" style="font-size:xx-small">' + v.membeR_NAME + '</td><td class="text-center" style="font-size:xx-small">' + v.joininG_DATE.split('T')[0] + '</td><td class="text-center" style="font-size:xx-small">' + (new Date().toISOString()).split("T")[0] + '</td></tr>');
                    $('#conoffice').html(v.p_NAME);
                    $('#EntN').html(v.name);
                    $('#ADD').html(v.address);
                    $('#a_s_d').html(v.audiT_STARTDATE.split('T')[0]);
                    $('#a_e_d').html(v.audiT_ENDDATE.split('T')[0]);
                    $('#o_s_d').html(v.operatioN_STARTDATE.split('T')[0]);
                    $('#o_e_d').html(v.operatioN_ENDDATE.split('T')[0]);



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
                    $('#joiningAuditorDetailsTable tbody').append('<tr><td class="text-center" style="font-size:xx-small">' + v.membeR_PPNO + '</td><td class="text-center" style="font-size:xx-small">' + v.membeR_NAME + '</td><td class="text-center" style="font-size:xx-small">' + v.audiT_START_DATE + '</td><td class="text-center" style="font-size:xx-small">' + v.audiT_END_DATE + '</td></tr>');
                 // 
                });

             



            },
            dataType: "json",
        });

  
  
    }

    function auditeereport() {
        g_engId = $('#entitySelectField').val();

        $('#conoffice').html();
        $('#EntN').html();
        $('#ADD').html();
        $('#a_s_d').html();
        $('#a_e_d').html();
        $('#o_s_d').html();
        $('#o_e_d').html();

       // if (g_type_id == 6)
        window.location.href = g_asiBaseURL + '/Execution/auditee_observations_report?engid=' + g_engId + '&conOffice=' + $('#conoffice').html() + '&entName=' + $('#EntN').html() + '&startDate=' + $('#a_s_d').html() + '&endDate=' + $('#a_e_d').html() + '&ostart=' + $('#o_s_d').html() + '&oend=' + $('#o_e_d').html();
      //  else
       //     window.location.href = '/Execution/auditee_observations_report_cau?engid=' + g_engId + '&conOffice=' + $('#conoffice').html() + '&entName=' + $('#EntN').html() + '&startDate=' + $('#a_s_d').html() + '&endDate=' + $('#a_e_d').html() + '&ostart=' + $('#o_s_d').html() + '&oend=' + $('#o_e_d').html();


       
            

    }

    function getreportstatus()
    {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_report_status",
            type: "POST",
            data: {
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {
                if (data[0].reF_OUT == "0") {
                 //   auditeereport();
                   // onAlertCallback(auditeereport);
                   if (g_engId == 0)
                        alert("Audit Entity Not Selected");

                  
                    else {
                    showApiAlert((data && data[0]) ? data[0] : data, 'Request completed.');
                    return;
                }
                }
                else {
                    if (g_engId == 0)
                        alert("Audit Entity Not Selected");

                    else {
                        showApiAlert((data && data[0]) ? data[0] : data, 'Request completed.');
                        onAlertCallback(auditeereport);
                        //auditeereport();
                    }


                }


            },
            dataType: "json",
        });

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
            
            },
            dataType: "json",
        });
    }

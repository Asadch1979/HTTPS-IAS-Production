    var g_teamMembers = [];
    var g_engId = 0;
    $(document).ready(function () {
        $('#reportheading1').empty;
        $('#reportheading2').empty;

        var url_string = window.location;
        var url = new URL(url_string);
        var eng_id = url.searchParams.get("engId");
        if (typeof eng_id != 'undefined')
            g_engId = eng_id;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_team_details",
            type: "POST",
            data: {
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    g_teamMembers.push(v.teaM_MEM_PPNO);
                    $('#joiningAuditorDetailsTable tbody').append('<tr><td class="text-center" style="font-size:xx-small">' + v.teaM_MEM_PPNO + '</td><td class="text-center" style="font-size:xx-small">' + v.membeR_NAME + '</td><td class="text-center" style="font-size:xx-small">' + v.joininG_DATE.split('T')[0] + '</td><td class="text-center" style="font-size:xx-small">' + (new Date().toISOString()).split("T")[0] + '</td></tr>');
                    // $('#conoffice').html(v.teaM_MEM_PPNO);
                });

                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/closing_draft_report_status",
                    type: "POST",
                    data: {
                        'ENG_ID': g_engId
                    },
                    cache: false,
                    success: function (data) {
                        g_obsList = data;
                        // g_engId = g_obsList[0].engplanid;
                        $.each(g_teamMembers, function (i, v) {
                            var totalObs = 0;
                            var sendToAuditee = 0;
                            var noresByAuditee = 0;
                            var settled = 0;
                            var addToDraft = 0;
                            var teamLead = 'N';
                            $.each(g_obsList, function (k, ob) {
                                if (ob.enteredby == v) {
                                    totalObs++;
                                    sendToAuditee++;
                                    if (ob.status == 4)
                                        settled++;
                                    if (ob.status == 5)
                                        addToDraft++;
                                    if (ob.status == 2)
                                        noresByAuditee++;
                                    if (ob.teaM_LEAD == 'Y')
                                        teamLead = 'Y';
                                }

                            });
                            $('#auditorWiseProgessTable tbody').append('<tr><td class="text-center" style="font-size:xx-small">' + v + '</td><td class="text-center" style="font-size:xx-small" >' + teamLead + '</td><td class="text-center" style="font-size:xx-small">' + totalObs + '</td><td class="text-center" style="font-size:xx-small">' + sendToAuditee + '</td><td class="text-center" style="font-size:xx-small">' + (totalObs - noresByAuditee) + '</td><td class="text-center" style="font-size:xx-small">' + settled + '</td><td class="text-center" style="font-size:xx-small">' + addToDraft + '</td></tr>')
                        });

                    },
                    dataType: "json",
                });



            },
            dataType: "json",
        });
    });
    function reloadLocationToUrl() {
        window.location.href = g_asiBaseURL + "/Engagement/task_list"
    }



    function getaddress() {

        //getParas();
        var eng_id = $('#entitySelectField').val();

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
                'ENT_ID': eng_id
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    console.log(v);
                    //  g_teamMembers.push(v.teaM_MEM_PPNO);
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
        getParas();
    }

    function getParas() {

        var eng_id = $('#entitySelectField').val();

        $('#conoffice').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_report_paras",
            type: "POST",
            data: {
                'ENG_ID': eng_id
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {



                    $('#obs_report_panel').append('<tr><td>' + v.memO_NUMBER + '</td><td>' + v.text + '</td><td>' + v.reply + '</td ><td>' + v.recommendation + '</td><td>' + v.headremarks + '</td></tr>')
                    $('#obs_report_panel2').append('<tr><td>' + v.memO_NUMBER + '</td><td>' + v.text + '</td><td>' + v.reply + '</td ><td>' + v.recommendation + '</td><td>' + v.headremarks + '</td></tr>')



                });


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
            success: function () {
                alert("Audit Closed Successfully");
                onAlertCallback(reloadLocationToUrl);
            },
            dataType: "json",
        });
    }

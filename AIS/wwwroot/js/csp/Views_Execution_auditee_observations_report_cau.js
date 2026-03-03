    var g_teamMembers = [];
    var g_engId = 0;
    var g_entName = '';
    var g_startDate = '';
    var g_endDate = '';
    var g_ostartDate = '';
    var g_oendDate = '';
    var g_conoffice = '';
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engid");
        g_entName = url.searchParams.get("entName");
        g_startDate = url.searchParams.get("startDate");
        g_endDate = url.searchParams.get("endDate");
        g_ostartDate = url.searchParams.get("ostart");
        g_oendDate = url.searchParams.get("oend");
        g_conoffice = url.searchParams.get("conOffice");

        getParas();

        $('.conOffice').html(g_conoffice);
        $('.entNameField').html(g_entName);
        $('#startField').html(g_startDate);
        $('#endField').html(g_endDate);
        $('#ostartField').html(g_ostartDate);
        $('#oendField').html(g_oendDate);
    });

    function CreatePDFfromHTML() {
        $('#wait').show()
        var HTML_Width = $(".WordSection1").width();
        var HTML_Height = $(".WordSection1").height();
        var top_left_margin = 15;
        var PDF_Width = HTML_Width + (top_left_margin * 2);
        var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
        var canvas_image_width = HTML_Width;
        var canvas_image_height = HTML_Height;

        var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

        html2canvas($(".WordSection1")[0]).then(function (canvas) {
            var imgData = canvas.toDataURL("image/jpeg", 1.0);
            var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
            pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
            for (var i = 1; i <= totalPDFPages; i++) {
                pdf.addPage(PDF_Width, PDF_Height);
                pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
            }
            pdf.save("branchReport.pdf");
            $('#wait').hide()
            //  $(".WordSection1").hide();
        });
    }

    /* $('#reportheading1').empty;
     $('#reportheading2').empty;

     var url_string = window.location;
     var url = new URL(url_string);
     var eng_id =
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
     window.location.href = "/Engagement/task_list"
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
 */
    function getParas() {

        //  var eng_id = $('#entitySelectField').val();

        $('.conOffice').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_report_paras",
            type: "POST",
            data: {
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {

                $('.conOffice').html(data.length > 0 ? data[0].name : '');
                $.each(data, function (i, v) {
                    if (v.assignedto == "CAU")
                        $('#obs_report_panel').append('<tr ><td style="text-align:center">' + v.memO_NUMBER + '</td><td style="text-align:center">' + v.name + '</td><td style="padding:50px !important;text-align:justify"><div style="width:1000px; max-width:1000px; overflow-y:auto">' + v.text + '</div></td><td style="text-align:center">' + v.reply + '</td ><td style="text-align:center">' + v.recommendation + '</td><td style="text-align:center">' + v.headremarks + '</td></tr>')
                    else
                        $('#obs_report_panel2').append('<tr ><td style="text-align:center">' + v.memO_NUMBER + '</td><td style="text-align:center">' + v.name + '</td><td style="padding:50px !important;text-align:justify"><div style="width:1000px; max-width:1000px; overflow-y:auto">' + v.text + '</div></td><td style="text-align:center">' + v.reply + '</td ><td style="text-align:center">' + v.recommendation + '</td><td style="text-align:center">' + v.headremarks + '</td></tr>')



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

    function CreatePDFfromHTML() {
        $('#wait').show

        var HTML_Width = $(".reportsec").width();
        var HTML_Height = $(".reportsec").height();
        var top_left_margin = 15;
        var PDF_Width = HTML_Width + (top_left_margin * 2);
        var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
        var canvas_image_width = HTML_Width;
        var canvas_image_height = HTML_Height;

        var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

        html2canvas($(".reportsec")[0]).then(function (canvas) {
            var imgData = canvas.toDataURL("image/jpeg", 1.0);
            var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
            pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
            for (var i = 1; i <= totalPDFPages; i++) {
                pdf.addPage(PDF_Width, PDF_Height);
                pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
            }
            pdf.save("Report.pdf");
            $('#wait').hide()
            //  $(".WordSection1").hide();
        });
    }

    function getauditplanreport() {
        var sr = 1;

        var entityid = $("#entitySelectField option:selected").attr("id");
        var zoneid = $("#ZoneSelectField option:selected").attr("id");
        var risk = $('#RiskSelectField').val();
        var size =$("#SizeSelectField option:selected").attr("id");

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetFADAuditPlan",
            type: "POST",
            data: {
                'ENT_ID': entityid,
                'Z_ID': zoneid,
                'RISK': risk,
                'SIZE': size
            },
            cache: false,
            success: function (data) {


                $.each(data, function (i, v) {
                    console.log(data);
                  //  g_teamMembers.push(v.teaM_MEM_PPNO);
                    
                    $('#tentative_plan_list tbody').append('<tr><td class="text-center" style="font-size:xx-small">' + sr + '</td><td class="text-center" style="font-size:xx-small">' + v.auditeename + '</td><td class="text-center" style="font-size:xx-small">' + v.entitycode + '</td><td class="text-center" style="font-size:xx-small">' + v.auditedby + '</td><td class="text-center" style="font-size:xx-small">' + v.parrentoffice + '</td><td class="text-center" style="font-size:xx-small">' + v.lastauditopsendate + '</td><td class="text-center" style="font-size:xx-small">' + v.entityrisk + '</td><td class="text-center" style="font-size:xx-small">' + v.entitysize + '</td><td class="text-center" style="font-size:xx-small">' + v.normaldays + '</td><td class="text-center" style="font-size:xx-small">' + v.revenuedays + '</td><td class="text-center" style="font-size:xx-small">' + v.travelday + '</td><td class="text-center" style="font-size:xx-small">' + v.discussionday + '</td><td class="text-center" style="font-size:xx-small">' + v.auditstartdate + '</td><td class="text-center" style="font-size:xx-small">' + v.auditenddate + '</td><td class="text-center" style="font-size:xx-small">' + v.tname + '</td><td class="text-center" style="font-size:xx-small">' + v.teamlead + '</td></tr>');
                    //
                    sr++;
                });





            },
            dataType: "json",
        });


    }

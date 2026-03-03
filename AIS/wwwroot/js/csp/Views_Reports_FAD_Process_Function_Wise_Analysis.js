    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#zoneiwseperformance tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function getReportContents(){
        $('#zoneiwseperformance tbody').empty();
        if ($('#ZoneSelectField option:selected').val() != -1) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_fad_new_old_para_performance",
                type: "POST",
                data: {
                    'AUDIT_ZONE_ID': $('#ZoneSelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                  
                    $.each(data, function (i, v) {
                        $('#zoneiwseperformance tbody').append(' <tr><td class="text-center">' + v.Checklist + '</td><td>' + v.Total_Paras + '</td><td>' + v.nature + '</td><td class="d-none entity_name_field">' + v.entitY_NAME + '</td><td class="d-none period_name_field">' + v.period + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="text-center"><a onclick="event.preventDefault();ObservationViewerPanel(' + v.obS_ID + ',' + v.obS_STATUS_ID + ', ' + v.obS_RISK_ID + ')" href="#" class="text-hover">View Memo</a></td></tr>');
                    });
                  


                },
                dataType: "json",
            });

        }
    }

    function CreatePDFfromHTML() {
        $('#wait').show();

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

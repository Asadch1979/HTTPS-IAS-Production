    $(document).ready(function () {

    })

    function getComplianceProgressReport() {
        destroyDatatable('progressReport_panel');
        if($('#roleSelectionField').val()=="0"){
            return false;
        }
        if ($('#roleSelectionField').val() == "R") {
            $('#theadTitle').html("Recommended for Settlement");
        } else if ($('#roleSelectionField').val() == "A") {
            $('#theadTitle').html("Settled");
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_progress_report",
            type: "POST",
            data: {
                "ROLE_TYPE": $('#roleSelectionField').val()
            },
            cache: false,
            success: function (data) {

                var t_c=0;
                var t_rb=0;
                var t_s=0;
                var t_p=0;
                $.each(data, function (i, v) {
                    t_c+= parseInt(v.total);
                    t_rb += parseInt(v.referreD_BACK);
                    t_s += parseInt(v.recommended);
                    t_p += parseInt(v.pending);
                     $('#progressReport_panel tbody').append('<tr><td class="text-center"> ' + ++i + '</td><td class="text-center">' + v.ppno + '</td><td class="text-left" >' + v.name + '</td><td class="text-right">' + v.total + '</td><td class="text-right">' + v.referreD_BACK + '</td><td class="text-right">' + v.recommended + '</td><td class="text-right">' + v.pending + '</td>><td class="text-right">' + v.lasT_LOGIN_ON + '</td><td class="text-center"><a href="#" data-onclick="event.preventDefault();getComplianceProgressDetails(' + v.ppno + ');">Details</a></td></tr>');

                    });

                $('#progressReport_panel tbody').append('<tr><td></td><td></td><td><b>Total</b></td><td class="text-right">' + t_c + '</td><td class="text-right">' + t_rb + '</td><td class="text-right">' + t_s + '</td><td class="text-right">' + t_p + '</td><td class="text-right"></td><td class="text-center"></td></tr>');
                initializeDataTable('progressReport_panel');

            },
            dataType: "json",
        });
    }

    function getComplianceProgressDetails(ppno){
        $('#complianceProgressReportDetailModel').modal('show');
        destroyDatatable('progressReportdetails_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_progress_report_details",
            type: "POST",
            data: {
                "ROLE_TYPE": $('#roleSelectionField').val(),
                    "PP_NO": ppno
            },
            cache: false,
            success: function (data) {
                var t_c = 0;
                var t_rb = 0;
                var t_s = 0;
                var t_p = 0;
                $.each(data, function (i, v) {
                    t_c += parseInt(v.total);
                    t_rb += parseInt(v.referreD_BACK);
                    t_s += parseInt(v.recommended);
                    t_p += parseInt(v.pending);
                    $('#progressReportdetails_panel tbody').append('<tr><td class="text-center"> ' + ++i + '</td><td class="text-left">' + v.parenT_NAME + '</td><td class="text-center">' + v.entitY_CODE + '</td><td class="text-left">' + v.entitY_NAME + '</td><td class="text-right">' + v.total + '</td><td class="text-right">' + v.referreD_BACK + '</td><td class="text-right">' + v.recommended + '</td><td class="text-right">' + v.pending + '</td></tr>');

                });
                initializeDataTable('progressReportdetails_panel');

            },
            dataType: "json",
        });

    }

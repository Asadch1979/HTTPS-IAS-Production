    $(document).ready(function () {

       
    });

    function getBACAnalysis() {

        $('#bacanalysis_panel tbody').empty();

        if ($('#BACAnalysisOptionBox option:selected').val() == "-1")
            return;

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_bac_analysis",
            type: "POST",
            data: {
                'PROCESS_ID': $('#BACAnalysisOptionBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var rowspan = 1;
                var mergeRow = 1;
                var currentT = 0;
                var legacyT = 0;
                var totalT = 0;
                var heading_check = "";
                var rowSpArr = [];
                $.each(data, function (index, item) 
                {
                    if (item.heading == heading_check) {
                        rowspan++;
                       
                    } else {
                        if(rowspan>1)
                            rowSpArr.push({ 'heading': heading_check, 'rowspan': rowspan });
                        rowspan = 1;
                        heading_check = item.heading;
                    }

                });
                rowSpArr.push({ 'heading': heading_check, 'rowspan': rowspan });

                console.log(rowSpArr);
                rowspan = 1;
                var heading_check = "";
                $.each(data, function (index, item) {
                    currentT += parseInt(item.newcount);
                    legacyT += parseInt(item.oldcount);
                    totalT += parseInt(item.count);
                    if (item.heading != heading_check) {
                        $.each(rowSpArr, function (i, v) {
                            if (v.heading == item.heading) {
                                mergeRow = v.rowspan;
                            }
                        });

                        if(item.indicator=="Y")
                            $('#bacanalysis_panel tbody').append("<tr><td>" + sr + "</td><td class=\"heading_field\">" + item.heading + "</td><td>" + item.annex + "</td><td class=\"text-right\">" + item.newcount + "</td><td class=\"text-right\">" + item.oldcount + "</td><td class=\"text-right\">" + item.count + "</td><td style=\"vertical-align: middle;\" rowspan=\"" + mergeRow + "\">" + item.auditcomments + "</td><td class=\"actionsCol\"><a href=\"#\" data-onclick=\"getParaViewerDetails(" + item.id + ");\">View Detail</a></td><td class=\"actionsCol\"><a  href=\"#\" data-onclick=\"getParaSummaryDetails(" + item.id + ");\">View Summary</a></td></tr>");
                        else
                            $('#bacanalysis_panel tbody').append("<tr><td>" + sr + "</td><td class=\"heading_field\">" + item.heading + "</td><td>" + item.annex + "</td><td class=\"text-right\">" + item.newcount + "</td><td class=\"text-right\">" + item.oldcount + "</td><td class=\"text-right\">" + item.count + "</td><td style=\"vertical-align: middle;\" rowspan=\"" + mergeRow + "\">" + item.auditcomments + "</td><td class=\"actionsCol\"></td><td class=\"actionsCol\"></td></tr>");
                        heading_check = item.heading;
                    }
                    else{
                        if (item.indicator == "Y")
                            $('#bacanalysis_panel tbody').append("<tr><td>" + sr + "</td><td class=\"heading_field\">" + item.heading + "</td><td>" + item.annex + "</td><td class=\"text-right\">" + item.newcount + "</td><td class=\"text-right\">" + item.oldcount + "</td><td class=\"text-right\">" + item.count + "</td><td class=\"actionsCol\"><a href=\"#\" data-onclick=\"getParaViewerDetails(" + item.id + ");\">View Detail</a></td><td class=\"actionsCol\"><a href=\"#\" data-onclick=\"getParaSummaryDetails(" + item.id + ");\">View Summary</a></td></tr>");
                        else
                            $('#bacanalysis_panel tbody').append("<tr><td>" + sr + "</td><td class=\"heading_field\">" + item.heading + "</td><td>" + item.annex + "</td><td class=\"text-right\">" + item.newcount + "</td><td class=\"text-right\">" + item.oldcount + "</td><td class=\"text-right\">" + item.count + "</td><td class=\"actionsCol\" ></td><td class=\"actionsCol\"></td></tr>");

                         heading_check = item.heading;
                    }
                    sr++;                   
                });              

                if ($('#BACAnalysisOptionBox option:selected').val() == "0")
                {
                    $('#bacanalysis_panel tbody').append("<tr><td colspan=\"3\"><b>Total</b></td><td class=\"text-right text-bold\"><b>" + currentT + "</b></td><td class=\"text-right text-bold\"><b>" + legacyT + "</b></td><td class=\"text-right text-bold\"><b>" + totalT + "</b></td><td></td><td class=\"actionsCol\"></td><td class=\"actionsCol\"></td></tr>");
                    $('.heading_field').show();
                    $('.actionsCol').hide();

                }                
                else
                {
                    $('.heading_field').hide();
                    $('.actionsCol').show();
                    $('#bacanalysis_panel tbody').append("<tr><td colspan=\"2\"><b>Total</b></td><td class=\"text-right text-bold\"><b>" + currentT + "</b></td><td class=\"text-right text-bold\"><b>" + legacyT + "</b></td><td class=\"text-right text-bold\"><b>" + totalT + "</b></td><td></td><td class=\"actionsCol\"></td><td class=\"actionsCol\"></td></tr>");
                }   
                


            },
            dataType: "json",
        });

    }

    function getParaText(id, pc) {
        $('#paraTextViewerModel').modal("show");
        $('#paraTextDivField').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_functional_observation_text",
            type: "POST",
            data: {
                'PARA_ID': id,
                'PARA_CATEGORY': pc
            },
            cache: false,
            success: function (data) {
                $('#paraTextDivField').html(data);
            }
        });
    }

    function getParaViewerDetails(id) {
        $('#paraViewerModel').modal('show');
        $('#entitywise_panel tbody').empty();
         $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_analysis_detail_paras",
            type: "POST",
            data: {
                'PROCESS_ID': id
            },
            cache: false,
            success: function (data) {
                var sr = 1;

                $.each(data, function (index, item) {

                    $('#entitywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.name + "</td><td>" + item.audiT_PERIOD + "</td> <td class=\"text-right\">" + item.parA_NO + "</td><td><a href=\"#\" data-onclick=\"getParaText(" + item.id + ", '"+item.parA_CATEGORY+"');\">View Para Text</a></td></tr>");
                    sr++;
                });


            },
            dataType: "json",
        });
    }

    function getParaSummaryDetails(id) {
        $('#paraSummaryModel').modal('show');
        $('#summarywise_panel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_analysis_summary_paras",
            type: "POST",
            data: {
                'PROCESS_ID': id
            },
            cache: false,
            success: function (data) {
                var sr = 1;

                $.each(data, function (index, item) {

                    $('#summarywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.p_NAME + "</td><td>" + item.name + "</td><td>" + item.audiT_PERIOD + "</td> <td class=\"text-right\">" + item.parA_NO + "</td></tr>");
                    sr++;
                });


            },
            dataType: "json",
        });
    }

    $(document).ready(function () {
       
    });

    function getBACSubProcess() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_bac_analysis",
            type: "POST",
            data: {
                'PROCESS_ID': $('#BACAnalysisOptionBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var currentT = 0;
                var legacyT = 0;
                var totalT = 0;
                $.each(data, function (index, item) {
                    currentT += parseInt(item.newcount);
                    legacyT += parseInt(item.oldcount);
                    totalT += parseInt(item.count);
                    $('#bacanalysis_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.heading + "</td><td class=\"text-right\">" + item.newcount + "</td><td class=\"text-right\">" + item.oldcount + "</td><td class=\"text-right\">" + item.count + "</td><td>" + item.auditcomments + "</td></tr>");
                    sr++;
                });

                $('#bacanalysis_panel tbody').append("<tr><td colspan=\"2\">Total</td><td class=\"text-right text-bold\">" + currentT + "</td><td class=\"text-right text-bold\">" + legacyT + "</td><td class=\"text-right text-bold\">" + totalT + "</td><td></td></tr>");

            },
            dataType: "json",
        });

    }

    function getBACAnalysis(){

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_bac_analysis",
            type: "POST",
            data: {
                'PROCESS_ID': $('#BACAnalysisOptionBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var currentT = 0;
                var legacyT = 0;
                var totalT = 0;
                $.each(data, function (index, item) {
                    currentT += parseInt(item.newcount);
                    legacyT += parseInt(item.oldcount);
                    totalT += parseInt(item.count);
                    $('#bacanalysis_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.heading + "</td><td class=\"text-right\">" + item.newcount + "</td><td class=\"text-right\">" + item.oldcount + "</td><td class=\"text-right\">" + item.count + "</td><td>" + item.auditcomments + "</td></tr>");
                    sr++;
                });

                $('#bacanalysis_panel tbody').append("<tr><td colspan=\"2\">Total</td><td class=\"text-right text-bold\">" + currentT + "</td><td class=\"text-right text-bold\">" + legacyT + "</td><td class=\"text-right text-bold\">" + totalT + "</td><td></td></tr>");

            },
            dataType: "json",
        });

    }

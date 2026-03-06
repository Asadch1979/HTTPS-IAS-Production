    var dTable;
    $(document).ready(function () {
        getEntityWiseObservations();
    });

    function getEntityWiseObservations() {
        destroyDatatable('entitywise_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entity_wise_observations",
            type: "POST",
            data: {
           },
            cache: false,
            success: function (data) {
                var sr = 1;
                var rowspan = 1;
                var mergeRow = 1;
                var currentT = 0;
                var legacyT = 0;
                var totalT = 0;
                var r1 = 0;
                var r2 = 0;
                var r3 = 0;
                $.each(data, function (index, item) {
                    currentT += parseInt(item.neW_TOTAL);
                    legacyT += parseInt(item.olD_TOTAL);
                    totalT += parseInt(item.total);
                    r1 += parseInt(item.r1);
                    r2 += parseInt(item.r2);
                    r3 += parseInt(item.r3);
                    $('#entitywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.reportinG_OFFICE + "</td><td>" + item.entitY_NAME + "</td><td class=\"text-right\">" + item.neW_TOTAL + "</td><td class=\"text-right\">" + item.olD_TOTAL + "</td><td class=\"text-right\">" + item.total + "</td><td class=\"text-right\" style=\"background-color: #ff968f; \">" + item.r1 + "</td><td class=\"text-right\" style=\"background-color: #f9e10a6b; \">" + item.r2 + "</td><td class=\"text-right\" style=\"background-color:#82f386;\">" + item.r3 + "</td><td><a href=\"#\" data-onclick=\"getDetails("+item.entitY_ID+");\" class=\"text-danger\">Details</a></td></tr>");
                    sr++;
                });
               $('#entitywise_panel tbody').append("<tr><td></td><td></td><td><b>Total</b></td><td class=\"text-right text-bold\"><b>" + currentT + "</b></td><td class=\"text-right text-bold\"><b>" + legacyT + "</b></td><td class=\"text-right text-bold\"><b>" + totalT + "</b></td><td class=\"text-right text-bold\"><b>" + r1 + "</b></td><td class=\"text-right text-bold\"><b>" + r2 + "</b></td><td class=\"text-right text-bold\"><b>" + r3 + "</b></td><td></td></tr>");
                initializeDataTable('entitywise_panel');

            },
            dataType: "json",
        });
    }

    function getDetails(id) {
        window.location.href = g_asiBaseURL + "/Dashboard/entity_wise_obs_detail?entId=" + id;
    }

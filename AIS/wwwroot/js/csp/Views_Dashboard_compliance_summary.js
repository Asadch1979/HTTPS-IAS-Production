    $(document).ready(function () {
        getEntityWiseObservations(0);
    });

    function getEntityWiseObservations(entityId) {
        
        destroyDatatable('entitywise_panel_mainGrid');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_summary",
            type: "POST",
            data: {
                ENTITY_ID: entityId
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var totalP = 0;
                var totalC = 0;
                var atRepo = 0;
                var underCon = 0;
                var settled = 0;
                var rejected = 0;
                var roleID = 0;
                $.each(data, function (index, item) {
                    roleID = item.rolE_ID;
                    totalP += parseInt(item.totaL_PARA);
                    totalC += parseInt(item.totaL_COMPLIANCE);
                    atRepo += parseInt(item.aT_REPORTING);
                    underCon += parseInt(item.undeR_CONSIDERATION);
                    settled += parseInt(item.settled);
                    rejected += parseInt(item.rejected);
                    $('#entitywise_panel_mainGrid tbody').append("<tr><td>" + sr + "</td><td>" + item.name + "</td><td class=\"text-right\">" + item.totaL_COMPLIANCE + "</td><td class=\"text-right\">" + item.aT_REPORTING + "</td><td class=\"text-right\">" + item.undeR_CONSIDERATION + "</td><td class=\"text-right\">" + item.settled + "</td><td class=\"text-right\">" + item.rejected + "</td><td>" + item.totaL_PARA + "</td><td class=\"actionsCol col_hide\"><a href=\"#\" data-click=\"getComplianceSummaryDetails(" + item.id + ");\">View Details</a></td></tr>");
                    sr++;
                });
                $('#entitywise_panel_mainGrid tbody').append("<tr><td></td><td><b>Total</b></td><td class=\"text-right text-bold\"><b>" + totalC + "</b></td><td class=\"text-right text-bold\"><b>" + atRepo + "</b></td><td class=\"text-right text-bold\"><b>" + underCon + "</b></td><td class=\"text-right text-bold\"><b>" + settled + "</b></td><td class=\"text-right text-bold\"><b>" + rejected + "</b></td><td class=\"text-right text-bold\"><b>" + totalP + "</b></td><td class=\"col_hide\"></td></tr>");
                if (roleID == 12 || roleID == 13 || roleID == 14 )
                    $('.col_hide').remove();
                initializeDataTable('entitywise_panel_mainGrid');

            },
            dataType: "json",
        });
    }

    function getComplianceSummaryDetails(id) {
        $('#paraSummaryModel').modal('show');        
        destroyDatatable('summarywise_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_summary",
            type: "POST",
            data: {
                ENTITY_ID: id
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var totalP = 0;
                var totalC = 0;
                var atRepo = 0;
                var underCon = 0;
                var settled = 0;
                var rejected = 0;

                $.each(data, function (index, item) {

                    totalP += parseInt(item.totaL_PARA);
                    totalC += parseInt(item.totaL_COMPLIANCE);
                    atRepo += parseInt(item.aT_REPORTING);
                    underCon += parseInt(item.undeR_CONSIDERATION);
                    settled += parseInt(item.settled);
                    rejected += parseInt(item.rejected);

                    $('#summarywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.name + "</td><td class=\"text-right\">" + item.totaL_COMPLIANCE + "</td><td class=\"text-right\">" + item.aT_REPORTING + "</td><td class=\"text-right\">" + item.undeR_CONSIDERATION + "</td><td class=\"text-right\">" + item.settled + "</td><td class=\"text-right\">" + item.rejected + "</td><td>" + item.totaL_PARA + "</td></tr>");
                    sr++;
                });
                $('#summarywise_panel tbody').append("<tr><td></td><td><b>Total</b></td><td class=\"text-right text-bold\"><b>" + totalC + "</b></td><td class=\"text-right text-bold\"><b>" + atRepo + "</b></td><td class=\"text-right text-bold\"><b>" + underCon + "</b></td><td class=\"text-right text-bold\"><b>" + settled + "</b></td><td class=\"text-right text-bold\"><b>" + rejected + "</b></td><td class=\"text-right text-bold\"><b>" + totalP + "</b></td></tr>");
                initializeDataTable('summarywise_panel');

            },
            dataType: "json",
        });
    }

    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrel",
            type: "POST",
            data: {
                'ENTITY_REALTION_ID': $('#RelationshipField option:selected').val()
            },


            cache: false,
            success: function (data) {


                $('#controlingsearch').append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
                $.each(data, function (index, contof) {

                    var selected = '';
                    if (contof.entitY_ID == parentEntityId)
                        selected = 'selected="selected"';

                    $('#controlingsearch').append('<option ' + selected + ' value="' + contof.entitY_ID + '" id="' + contof.entitY_REALTION_ID + '">' + contof.description + '</option>')
                });
                if (userEntityId != 0)
                    getplacepost(userEntityId)

                // console.log(data);

            },
            dataType: "json",
        });



    }

    function getplacepost(userEntityId = 0) {
        $('#childposting').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: {
                'E_R_ID': $('#controlingsearch option:selected').val()
            },


            cache: false,
            success: function (data) {
                $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
                $.each(data, function (index, gpp) {

                    var selected = '';
                    if (gpp.entitY_ID == userEntityId)
                        selected = 'selected="selected"';
                    $('#childposting').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>')
                });
            },
            dataType: "json",
        });
        //  getrelation();

    }

    function getAuditEngagementPlans() {
        $('#engagementList').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_auditee_engagement_plan",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting option:selected').val(),
                'PERIOD': $('#periodSelectionBox option:selected').val()
            },

            cache: false,
            success: function (data) {
                $('#engagementList').append('<option id="0" value="0" selected="selected">--Select Engagements--</option>');
                $.each(data, function (index, gpp) {
                    $('#engagementList').append('<option value="' + gpp.enG_ID + '" >' + gpp.status + '</option>')
                });
            },
            dataType: "json",
        });
    }

    function getAuditeeRisk() {
        $('#auditeeRiskAreaPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_auditee_risk",
            type: "POST",
            data: {
                'ENG_ID': $('#engagementList option:selected').val()
                
            },

            cache: false,
            success: function (data) {
                var sr = 1;
                var total_count_mx = 0;
                var total_count_m = 0;
                var m = 0;
                $.each(data, function (index, v) {
                    total_count_mx += parseInt(v.maX_NUMBER);
                    total_count_m += parseFloat(v.marks);
                    m = parseFloat(v.marks);

                    $('#auditeeRiskAreaPanel tbody').append('<tr><td align="center">' + sr + '</td><td>' + v.risK_AREAS + '</td><td align="right">' + v.maX_NUMBER + '</td><td align="right">' + m.toFixed(2) + '</td></tr>')
                    sr++
                });

                $('#auditeeRiskAreaPanel tbody').append('<tr><td colspan="2">Total</td><td align="right">' + total_count_mx + '</td><td align="right" >' + total_count_m.toFixed(2) + '</td></tr>')
            },
            dataType: "json",
        });
        getAuditeeRiskDetails();
    }

    function getAuditeeRiskDetails() {
        $('#auditeeRiskAreaPanel_details tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_auditee_risk_details",
            type: "POST",
            data: {
                'ENG_ID': $('#engagementList option:selected').val()
            },

            cache: false,
            success: function (data) {
                var sr = 1;
                var total_count_mx = 0;
                var total_count_m = 0;
                var total_count_o = 0;
                var total_count_avg = 0;
                var total_count_gr = 0;
                var total_count_wavg = 0;
                var marks = 0;
                var avg = 0;
                $.each(data, function (index, v) {
                    total_count_mx += parseFloat(v.maX_NUMBER);
                    total_count_m += parseInt(v.risK_MARKS);
                    total_count_o += parseInt(v.nO_OBS);
                    total_count_avg += parseFloat(v.avG_MARKS);
                    total_count_gr += parseInt(v.g_RISK);
                    total_count_wavg += parseFloat(v.w_AVG);
                    marks = parseFloat(v.maX_NUMBER);
                    avg = parseFloat(v.avG_MARKS);
                    wavg = parseFloat(v.w_AVG);
                    
                    $('#auditeeRiskAreaPanel_details tbody').append('<tr><td align="center">' + sr + '</td><td>' + v.risK_AREAS + '</td><td align="right">' + marks + '</td><td align="right">' + wavg.toFixed(2) + '</td><td align="right">' + v.g_RISK + '</td><td align="right">' + v.nO_OBS + '</td><td align="right">' + v.risK_MARKS + '</td><td align="right">' + avg.toFixed(2) + '</td></tr>')
                    sr++
                });

                $('#auditeeRiskAreaPanel_details tbody').append('<tr><td colspan="2">Total</td><td align="right">' + total_count_mx + '</td><td align="right">' + total_count_wavg.toFixed(2) + '</td><td align="right">' + total_count_gr + '</td><td align="right">' + total_count_o + '</td><td align="right">' + total_count_m + '</td><td align="right">' + total_count_avg.toFixed(2) + '</td></tr>')
            },
            dataType: "json",
        });

    }

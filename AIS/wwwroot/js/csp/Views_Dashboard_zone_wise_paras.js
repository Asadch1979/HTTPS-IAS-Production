    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrelForDashboardPanel",
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
            },
            dataType: "json",
        });



    }
    function getplacepost(userEntityId = 0) {
        $('#childposting').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplaceForDashboardPanel",
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

    }
    function getLegacyEntityObservation() {
        destroyDatatable('observation_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_relation_legacy_observation_for_dashboard",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting option:selected').val(),
            },
            cache: false,
            success: function (data) {
                g_response = data;
                var sr = 1;
                var t_tparas = 0;
                var t_sparas = 0;
                var t_uparas = 0;
                var t_rparas = 0;
                var t_hparas = 0;
                var t_mparas = 0;
                var t_lparas = 0;
                $.each(data, function (i, v) {
                    t_tparas += parseInt(v.total_Paras);
                    t_sparas += parseInt(v.setteled_Para);
                    t_uparas += parseInt(v.unsetteled_Para);
                    t_rparas += parseFloat(v.ratio);
                    t_hparas += parseInt(v.r1);
                    t_mparas += parseInt(v.r2);
                    t_lparas += parseInt(v.r3);
                    t_rparas = parseFloat(t_rparas).toFixed(2);
                    $('#observation_panel tbody').append('  <tr id="' + v.id + '"><td align="center"> ' + sr + '</td> <td align="left">' + v.process + '</td> <td align="right">' + v.total_Paras + '</td> <td align="right">' + v.setteled_Para + '</td> <td align="right">' + v.unsetteled_Para + '</td> <td align="right">' + v.ratio + '</td> <td align="right" style="background-color: #ff968f;">' + v.r1 + '</td> <td align="right" style="background-color:#f9e10a6b;">' + v.r2 + '</td> <td align="right" style="background-color:#82f386;">' + v.r3 + '</td> </tr>');
                    sr++;
                });
                var finalRatio = (parseFloat(t_sparas / t_tparas) * 100).toFixed(2);
                $('#observation_panel tbody').append('<tr><td></td><td align="right"><b>Total</b></td><td align="right">' + t_tparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + finalRatio + '%</td><td align="right">' + t_hparas + '</td><td align="right">' + t_mparas + '</td><td align="right">' + t_lparas + '</td></tr>')
                initializeDataTable('observation_panel');
            },
            dataType: "json",
        });
       
      
    }
    function getAISEntityObservation() {
        destroyDatatable('observation_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_relation_ais_observation_for_dashboard",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting option:selected').val(),
            },
            cache: false,
            success: function (data) {
                g_response = data;
                var sr = 1;
                var t_tparas = 0;
                var t_sparas = 0;
                var t_uparas = 0;
                var t_rparas = 0;
                var t_hparas = 0;
                var t_mparas = 0;
                var t_lparas = 0;
                $.each(data, function (i, v) {
                    t_tparas += parseInt(v.total_Paras);
                    t_sparas += parseInt(v.setteled_Para);
                    t_uparas += parseInt(v.unsetteled_Para);
                    t_rparas += parseFloat(v.ratio);
                    t_hparas += parseInt(v.r1);
                    t_mparas += parseInt(v.r2);
                    t_lparas += parseInt(v.r3);
                    t_rparas = parseFloat(t_rparas).toFixed(2);
                    $('#observation_panel tbody').append('  <tr id="' + v.id + '"><td align="center"> ' + sr + '</td> <td align="left">' + v.process + '</td> <td align="right">' + v.total_Paras + '</td> <td align="right">' + v.setteled_Para + '</td> <td align="right">' + v.unsetteled_Para + '</td> <td align="right">' + v.ratio + '</td> <td align="right" style="background-color: #ff968f;">' + v.r1 + '</td> <td align="right" style="background-color:#f9e10a6b;">' + v.r2 + '</td> <td align="right" style="background-color:#82f386;">' + v.r3 + '</td> </tr>');
                    sr++;
                });
                var finalRatio = (parseFloat(t_sparas / t_tparas) * 100).toFixed(2);
                $('#observation_panel tbody').append('<tr><td></td><td align="right"><b>Total</b></td><td align="right">' + t_tparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + finalRatio + '%</td><td align="right">' + t_hparas + '</td><td align="right">' + t_mparas + '</td><td align="right">' + t_lparas + '</td></tr>')
                initializeDataTable('observation_panel');
            },
            dataType: "json",
        });


    }
    function getEntityObservation() {
        destroyDatatable('observation_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_relation_observation_for_dashboard",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting option:selected').val(),
            },
            cache: false,
            success: function (data) {
                g_response = data;
                var sr = 1;
                var t_tparas = 0;
                var t_sparas = 0;
                var t_uparas = 0;
                var t_rparas = 0;
                var t_hparas = 0;
                var t_mparas = 0;
                var t_lparas = 0;
                $.each(data, function (i, v) {
                    t_tparas += parseInt(v.total_Paras);
                    t_sparas += parseInt(v.setteled_Para);
                    t_uparas += parseInt(v.unsetteled_Para);
                    t_rparas += parseFloat(v.ratio);
                    t_hparas += parseInt(v.r1);
                    t_mparas += parseInt(v.r2);
                    t_lparas += parseInt(v.r3);
                    t_rparas = parseFloat(t_rparas).toFixed(2);
                    $('#observation_panel tbody').append('  <tr id="' + v.id + '"><td align="center"> ' + sr + '</td> <td align="left">' + v.process + '</td> <td align="right">' + v.total_Paras + '</td> <td align="right">' + v.setteled_Para + '</td> <td align="right">' + v.unsetteled_Para + '</td> <td align="right">' + v.ratio + '</td> <td align="right" style="background-color: #ff968f;">' + v.r1 + '</td> <td align="right" style="background-color:#f9e10a6b;">' + v.r2 + '</td> <td align="right" style="background-color:#82f386;">' + v.r3 + '</td> </tr>');
                    sr++;
                });
                var finalRatio = (parseFloat(t_sparas / t_tparas) * 100).toFixed(2);
                $('#observation_panel tbody').append('<tr><td></td><td  align="right"><b>Total</b></td><td align="right">' + t_tparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + finalRatio + '%</td><td align="right">' + t_hparas + '</td><td align="right">' + t_mparas + '</td><td align="right">' + t_lparas + '</td></tr>')
                initializeDataTable('observation_panel');
            },
            dataType: "json",
        });


    }

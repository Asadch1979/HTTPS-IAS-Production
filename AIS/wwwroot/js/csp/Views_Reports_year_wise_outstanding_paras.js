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
   
    function getAISEntityObservation() {
        destroyDatatable('observation_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_year_wise_outstanding_observations",
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
                var t_hparas = 0;
                var t_mparas = 0;
                var t_lparas = 0;
                $.each(data, function (i, v) {
                    t_tparas += parseInt(v.totaL_PARAS);
                    t_sparas += parseInt(v.settleD_PARA);
                    t_uparas += parseInt(v.uN_SETTLED_PARA);
                    t_hparas += parseInt(v.r1);
                    t_mparas += parseInt(v.r2);
                    t_lparas += parseInt(v.r3);
                    $('#observation_panel tbody').append('  <tr id="' + v.id + '"><td align="center"> ' + sr + '</td> <td align="left">' + v.audiT_PERIOD + '</td> <td align="right">' + v.totaL_PARAS + '</td> <td align="right">' + v.settleD_PARA + '</td> <td align="right">' + v.uN_SETTLED_PARA + '</td> <td align="right" style="background-color: #ff968f;">' + v.r1 + '</td> <td align="right" style="background-color:#f9e10a6b;">' + v.r2 + '</td> <td align="right" style="background-color:#82f386;">' + v.r3 + '</td><td align="center"><a href="#" data-onclick="getEntityWiseDetails('+v.audiT_PERIOD+');">Details</a></td></tr>');
                    sr++;
                });
                $('#observation_panel tbody').append('<tr><td></td><td align="right"><b>Total</b></td><td align="right">' + t_tparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + t_hparas + '</td><td align="right">' + t_mparas + '</td><td align="right">' + t_lparas + '</td><td></td></tr>')
                initializeDataTable('observation_panel');
            },
            dataType: "json",
        });


    }


       function getEntityWiseDetails(period) {
             $('#viewObservationDetailsModel').modal('show');

           destroyDatatable('observation_detail_model');
          $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_year_wise_outstanding_observations_detials",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#childposting option:selected').val(),
                    'AUDIT_PERIOD': period
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                        $('#observation_detail_model tbody').append('<tr id="assignedObRow_' + v.id + '"><td>' + ++i + '</td><td>' + v.entitY_NAME + '</td><td>' + v.audiT_PERIOD + '</td><td>' + v.memO_NO + '</td><td>' + v.gisT_OF_PARAS + '</td><td><a hre="#" class="text-danger text-center" style="cursor:pointer;" data-onclick="event.preventDefault();viewParaText(\'' + v.coM_ID + '\')"> View Para</a></td></tr>');

                    });
                    initializeDataTable('observation_detail_model');
                },
                dataType: "json",
            });

        
    }

    function viewParaText(comId) {
        if (!comId || comId === "0") return;
        $('#viewMemoModel').modal('show');
        $('#viewMemo_memo').html("");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
            type: "POST",
            data: {
                'COM_ID': comId
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $('#viewMemo_memo').html(data);
            }
        });
    }

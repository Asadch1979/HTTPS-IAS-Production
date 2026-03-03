    function getFunctionalOwenerList() {
        if ($('#deptSelectBox').val() != 0) {
            $('#functionalOwnerPanel').show();
            $('#functionalGroupSelectBox').empty();
            $('#functionalGroupSelectBox').append('<option value=0>--Select Functional Owner---</option>');

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_functional_owner_area_for_functional_responsibility_wise_paras_ho",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#deptSelectBox').val()
                },
                cache: false,
                success: function (data) {

                    $.each(data, function (i, v) {
                        $('#functionalGroupSelectBox').append('<option value="' + v.p_ID + '">' + v.p_NAME + '</option>');

                    });
                },
                dataType: "json",
            });

        }
        else {
            $('#functionalOwnerPanel').hide();
        }
    }

    function getEntityObservation() {
        if ($('#functionalGroupSelectBox').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_total_para_details_ho",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#deptSelectBox').val()
                },
                cache: false,
                success: function (data) {
                    g_obsList = data;
                    $('#observation_panel tbody').empty();
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
                        $('#observation_panel tbody').append('  <tr id="' + v.id + '"><td align="center"> ' + sr + '</td> <td align="left">' + v.process + '</td> <td align="right">' + v.total_Paras + '</td> <td align="right">' + v.setteled_Para + '</td> <td align="right">' + v.unsetteled_Para + '</td> <td align="right">' + parseFloat(v.ratio).toFixed(2) + '%</td><td align="right" style="background-color: #ff968f;">' + v.r1 + '</td> <td align="right" style="background-color:#f9e10a6b;">' + v.r2 + '</td> <td align="right" style="background-color:#82f386;">' + v.r3 + '</td> </tr>');
                        sr++;
                    });
                    var finalRatio = (parseFloat(t_sparas / t_tparas) * 100).toFixed(2);

                    $('#observation_panel tbody').append('<tr><td colspan="2" align="right">Total</td><td align="right">' + t_tparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + finalRatio + '%</td><td align="right">' + t_hparas + '</td><td align="right">' + t_mparas + '</td><td align="right">' + t_lparas + '</td></tr>');

                },
                dataType: "json",
            });
        }else{
            $('#observation_panel tbody').empty();
        }
    }

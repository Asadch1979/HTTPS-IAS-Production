    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#observation_panel tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        getParaPosition();
    })

    function getParaPosition() {
        destroyDatatable('observation_panel_ho');
        destroyDatatable('observation_panel_az');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_audit_para_reconsillation",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_response = data;
                var sr = 1;
                var t_tparas = 0;
                var t_sparas = 0;
                var t_slparas = 0;
                var t_uparas = 0;
                var t_opara = 0;
                var t_percentage = 0;
                var t_aparas = 0;                
                var t_r1 = 0;
                var t_r2 = 0;
                var t_r3 = 0;
                if (g_response.length > 0) {
                    if (g_response[0].indicator == "Z" || g_response[0].indicator == "" ) {
                        $('#observation_panel_az').removeClass('d-none');
                        $.each(data, function (i, v) {

                            var percentage = 0;

                            t_opara += parseInt(v.opeN_BALANCE);
                            t_aparas += parseInt(v.added);
                            t_tparas += parseInt(v.total);
                            t_slparas += parseInt(v.settleD_LEGACY);
                            t_sparas += parseInt(v.settleD_NEW_PARA);
                            t_uparas += parseInt(v.uN_SETTLED);
                            t_r1 += parseInt(v.r1);
                            t_r2 += parseInt(v.r2);
                            t_r3 += parseInt(v.r3);
                             $('#observation_panel_az tbody').append('<tr><td align="center"> ' + sr + '</td><td  align="left">' + v.entitY_TYPE_DESC + '</td><td class="visibCol" align="left">' + v.reportinG_OFFICE + '</td><td class="visibCol" align="left">' + v.entitY_NAME + '</td><td align="right">' + v.opeN_BALANCE + '</td><td align="right">' + v.added + '</td><td align="right">' + v.total + '</td><td align="right">' + v.settleD_LEGACY + '</td><td align="right">' + v.settleD_NEW_PARA + '</td><td align="right">' + v.uN_SETTLED + '</td><td align="right">' + v.r1 + '</td><td align="right">' + v.r2 + '</td><td align="right">' + v.r3 + '</td><td align="right">' + v.percentage + '%</td></tr>');
                            sr++;
                        });
                        t_percentage = (((t_sparas + t_slparas) / t_tparas) * 100).toFixed(2);
                        $('#observation_panel_az tbody').append('<tr><td align="center"></td><td align="center"></td><td align="left"></td><td align="right"><Total</td><td align="right">' + t_opara + '</td><td align="right">' + t_aparas + '</td><td align="right">' + t_tparas + '</td><td align="right">' + t_slparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + t_r1 + '</td><td align="right">' + t_r2 + '</td><td align="right">' + t_r3 + '</td><td align="right">' + t_percentage + '%</td></tr>')
                       
                        initializeDataTable('observation_panel_az');

                    }
                    else if (g_response[0].indicator == "A") {
                        $('#observation_panel_ho').removeClass('d-none');
                        $.each(data, function (i, v) {

                            var percentage = 0;

                            t_opara += parseInt(v.opeN_BALANCE);
                            t_aparas += parseInt(v.added);
                            t_tparas += parseInt(v.total);
                            t_slparas += parseInt(v.settleD_LEGACY);
                            t_sparas += parseInt(v.settleD_NEW_PARA);
                            t_uparas += parseInt(v.uN_SETTLED);
                            t_r1 += parseInt(v.r1);
                            t_r2 += parseInt(v.r2);
                            t_r3 += parseInt(v.r3);
                            $('#observation_panel_ho tbody').append('<tr><td align="center"> ' + sr + '</td><td  align="left">' + v.audiT_ZONE + '</td><td align="right">' + v.opeN_BALANCE + '</td><td align="right">' + v.added + '</td><td align="right">' + v.total + '</td><td align="right">' + v.settleD_LEGACY + '</td><td align="right">' + v.settleD_NEW_PARA + '</td><td align="right">' + v.uN_SETTLED + '</td><td align="right">' + v.r1 + '</td><td align="right">' + v.r2 + '</td><td align="right">' + v.r3 + '</td><td align="right">' + v.percentage + '%</td></tr>');
                            sr++;
                        });
                        t_percentage = (((t_sparas + t_slparas) / t_tparas) * 100).toFixed(2);
                        $('#observation_panel_ho tbody').append('<tr><td align="left"></td><td align="right"><Total</td><td align="right">' + t_opara + '</td><td align="right">' + t_aparas + '</td><td align="right">' + t_tparas + '</td><td align="right">' + t_slparas + '</td><td align="right">' + t_sparas + '</td><td align="right">' + t_uparas + '</td><td align="right">' + t_r1 + '</td><td align="right">' + t_r2 + '</td><td align="right">' + t_r3 + '</td><td align="right">' + t_percentage + '%</td></tr>')

                        initializeDataTable('observation_panel_ho');
                    }
                }
            },
            dataType: "json",
        });


    }

    function getDetails( indicator, parent_ent_id, annex_ind ){
        $('#DetailsA1Paras').modal('show');
          destroyDatatable('paraDetailsGrid');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_serious_entities_details",
            type: "POST",
            data: {
                'INDICATOR': indicator,
                'PARENT_ENT_ID': parent_ent_id,
                'ANNEX_IND': annex_ind
            },
            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    $('#paraDetailsGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.p_NAME + '</td><td>' + v.c_NAME + '</td><td>' + v.audiT_PERIOD + '</td><td>' + v.parA_NO + '</td><td>' + v.anneX_HEADING + '</td><td>' + v.risk + '</td><td>' + v.gisT_OF_PARAS + '</td><td>' + v.amounT_INVOLVED + '</td></tr>');
                });

                   initializeDataTable('paraDetailsGrid');
            },
            dataType: "json",
        });


    }

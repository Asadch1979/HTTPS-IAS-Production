    var g_cnic = 0;
    $('#document').ready(function () {
    });   
    function findParaByGistKeyword() {        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_report_para_by_gist_keyword",
            type: "POST",
            data: {
                'GIST': $('#gistkeywordSearch').val(),
            },
            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    $('#gldetailtab tbody').append('<tr><td class="text-center">' + ++i + '</td><td class="text-center">' + v.audiT_ZONE + '</td><td class="text-center">' + v.region + '</td><td class="text-center">' + v.brancH_CODE + '</td><td class="text-center">' + v.branch + '</td><td class="text-right">' + v.parA_NO+ '</td><td class="text-center">' + v.e_DATE.split(" ")[0] + '</td><td class="text-left">' + v.gisT_OF_PARAS + '</td><td class="text-right">' + v.nO_OF_INSTANCES + '</td><td class="text-right">' + v.amounT_INVOLVED + '</td></tr>')

                });
            },
            dataType: "json",
        });
    }

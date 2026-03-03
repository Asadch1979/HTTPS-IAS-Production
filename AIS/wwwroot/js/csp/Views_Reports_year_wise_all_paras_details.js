    $(document).ready(function () {

    })

    function getAuditParas() {
        destroyDatatable('year_wise_paras_grid');
        if($('#auditPeriodSelectBox').val()=="0"){
            return false;
        }
        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_year_wise_all_audit_paras",
            type: "POST",
            data: {
                "AUDIT_PERIOD": $('#auditPeriodSelectBox').val()
            },
            cache: false,
            success: function (data) {
                   $.each(data, function (i, v) {
        $('#year_wise_paras_grid tbody').append(
            '<tr>' +
                '<td class="text-center">' + (i + 1) + '</td>' +
                '<td class="text-center">' + (v.entitY_TYPE) + '</td>' +
                '<td class="text-center">' + (v.reportinG_OFFICE) + '</td>' +
                '<td class="text-center">' + (v.entitY_NAME) + '</td>' +
                '<td class="text-center">' + (v.entitY_RISK_LEVEL) + '</td>' +
                '<td class="text-center">' + (v.audiT_ZONE) + '</td>' +
                '<td class="text-center">' + (v.annexure) + '</td>' +
                '<td class="text-center">' + (v.parA_NO) + '</td>' +
                '<td class="text-center">' + (v.risk) + '</td>' +
                '<td class="text-center">' + (v.gisT_OF_PARAS) + '</td>' +
                '<td class="text-center">' + (v.nO_OF_INSTANCES) + '</td>' +
                '<td class="text-center">' + (v.amounT_INVOLVED) + '</td>' +
                '<td class="text-center">' + (v.functioN_RESP) + '</td>' +
                '<td class="text-center">' + (v.parA_STATUS) + '</td>' +
               
            '</tr>'
        );
    });

                initializeDataTable('year_wise_paras_grid');

            },
            dataType: "json",
        });
    }

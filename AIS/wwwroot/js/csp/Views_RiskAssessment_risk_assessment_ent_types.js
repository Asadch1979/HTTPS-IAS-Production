  

    function getAuditeeRiskForEntTypes() {
        $('#auditeeRiskAreaPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_auditee_risk_for_entity_types",
            type: "POST",
            data: {
                'ENT_TYPE_ID': $('#entTypesList option:selected').val(),
                    'PERIOD': $('#periodSelectionBox option:selected').val()
                
            },

            cache: false,
            success: function (data) {
                var sr = 1;
                var total_count_mx = 0;
                var total_count_m = 0;
                $.each(data, function (index, v) {
                    total_count_mx += parseInt(v.maX_NUMBER);
                    total_count_m += parseFloat(v.marks);
                    m = parseFloat(v.marks);

                    $('#auditeeRiskAreaPanel tbody').append('<tr><td>' + sr + '</td><td>' + v.parenT_OFFICE + '</td><td>' + v.name + '</td><td align="right">' + v.brancH_CODE + '</td><td align="right">' + v.risK_RATING + '</td><td>' + v.risK_CATEGORY + '</td></tr>')
                    sr++
                });

                
            },
            dataType: "json",
        });
        getAuditeeRiskDetails();
    }

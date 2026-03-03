    $(document).ready(function(){
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#observation_panel tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        getEntityObservation();
    });

    
    function getEntityObservation() {
        destroyDatatable('observation_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_risk_base_plan_for_dashboard",
            type: "POST",
            data: {
                'PROCESS_ID': $('#riskGroupSelectBox option:selected').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                destroyDatatable('observation_panel');
                var sr = 1;
                var total_sum = 0;
                $.each(data, function (i, v) {
                    total_sum += v.entitY_NO;
                    $('#observation_panel tbody').append('  <tr><td align="center"> ' + sr + '</td> <td align="left">' + v.entitY_NAME + '</td> <td align="left">' + v.entitY_DESC + '</td> <td align="left">' + v.entitY_RISK + '</td> <td align="left">' + v.entitY_SIZE + '</td> <td align="right">' + v.entitY_NO + '</td></tr>');
                    sr++;
                });
                $('#observation_panel tbody').append('  <tr><td></td><td></td><td></td><td></td><td><b>Total</b></td><td align="right">' + total_sum + '</td></tr>');
               initializeDataTable('observation_panel');
            },
            dataType: "json",
        });
    }

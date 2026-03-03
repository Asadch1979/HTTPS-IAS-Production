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
        $('#observation_panel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_annexure_exercise_status",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_response = data;

                var sr = 1;
                var t_tparas = 0;
                var t_pparas = 0;
                var t_cparas = 0;
                var t_percentage = 0;
                
                if (g_response.length > 0) {
                    $.each(data, function (i, v) {

                   
                        t_tparas += parseInt(v.total);
                        t_pparas += parseInt(v.pending);
                        t_cparas += parseInt(v.completed);
                        t_percentage = ((parseInt(v.completed) / parseInt(v.total)) * 100).toFixed(2);
                        //  v.percentage = 
                        $('#observation_panel tbody').append('<tr><td align="center"> ' + sr + '</td><td align="left">' + v.ppno + '</td><td align="left">' + v.name + '</td><td align="left">' + v.audiT_ZONE + '</td><td align="right">' + v.total + '</td><td align="right">' + v.pending + '</td><td align="right">' + v.completed + '</td><td align="right">' + t_percentage + '%</td></tr>');
                        sr++;
                    });
                  
                }

                t_percentage = ((parseInt(t_cparas) / parseInt(t_tparas)) * 100).toFixed(2);
                $('#observation_panel tbody').append('<tr><td colspan="4" align="right">Total</td><td align="right">' + t_tparas + '</td><td align="right">' + t_pparas + '</td><td align="right">' + t_cparas + '</td><td align="right">' + t_percentage + '%</td></tr>')

            },
            dataType: "json",
        });


    }

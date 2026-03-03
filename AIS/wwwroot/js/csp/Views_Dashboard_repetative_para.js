    function getSubProcess(){
        $('#riskSubGroupSelectBox').empty();
        $('#riskSubGroupSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
        $('#riskGroupDetailSelectBox').empty();
        $('#riskGroupDetailSelectBox').append("<option value=\"0\" id=\"0\">--Select Checklist Detail--</option>");
        
        $.ajax({
                url: g_asiBaseURL + "/Setup/process_details",
                type: "POST",
                data: {
                    'ProcessId': $('#riskGroupSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, item) {
                        $('#riskSubGroupSelectBox').append("<option value=\"" + item.id + "\"> " + item.title + " </option > ");
                    });
                },
                dataType: "json",
            });   
    }

    function getProcessDetail() {
       
        $('#riskGroupDetailSelectBox').empty();
        $('#riskGroupDetailSelectBox').append("<option value=\"0\" id=\"0\">--Select Checklist Detail--</option>");

        $.ajax({
            url: g_asiBaseURL + "/Setup/process_transactions",
            type: "POST",
            data: {
                'ProcessDetailId': $('#riskSubGroupSelectBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                $.each(data, function (index, item) {
                    $('#riskGroupDetailSelectBox').append("<option value=\"" + item.id + "\"> " + item.description + "</option>");
                });

            },
            dataType: "json",
        });
    
    }
       
    function getEntityObservation() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_repetative_paras_for_dashboard",
            type: "POST",
            data: {
                'PROCESS_ID': $('#riskGroupSelectBox option:selected').val(),
                'SUB_PROCESS_ID': $('#riskSubGroupSelectBox option:selected').val(),
                'PROCESS_DETAIL_ID': $('#riskGroupDetailSelectBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                destroyDatatable('observation_panel');
                var sr = 1;
                var t_2023 = 0;
                var t_2022 = 0;
                var t_2021 = 0;
                var t_2020 = 0;
                var t_2019 = 0;
                var t_2018 = 0;
                var t_2017 = 0;
                var t_2016 = 0;
                var t_2015 = 0;
                var t_2014 = 0;
                var t_2013 = 0;
                var t_2012 = 0;
                var t_2011 = 0;
                var t_2010 = 0;
                $.each(data, function (i, v) {
                    t_2023 += parseInt(v.y2023);
                    t_2022 += parseInt(v.y2022);
                    t_2021 += parseInt(v.y2021);
                    t_2020 += parseInt(v.y2020);
                    t_2019 += parseInt(v.y2019);
                    t_2018 += parseInt(v.y2018);
                    t_2017 += parseInt(v.y2017);
                    t_2016 += parseInt(v.y2016);
                    t_2015 += parseInt(v.y2015);
                    t_2014 += parseInt(v.y2014);
                    t_2013 += parseInt(v.y2013);
                    t_2012 += parseInt(v.y2012);
                    t_2011 += parseInt(v.y2011);
                    t_2010 += parseInt(v.y2010);
                    $('#observation_panel tbody').append('<tr id="' + v.id + '"><td align="center"> ' + sr + '</td> <td align="left">' + v.procesS_DETAIL + '</td><td align="right">' + v.y2023 + '</td> <td align="right">' + v.y2022 + '</td><td align="right">' + v.y2021 + '</td><td align="right">' + v.y2020 + '</td><td align="right">' + v.y2019 + '</td><td align="right">' + v.y2018 + '</td><td align="right">' + v.y2017 + '</td><td align="right">' + v.y2016 + '</td><td align="right">' + v.y2015 + '</td><td align="right">' + v.y2014 + '</td><td align="right">' + v.y2013 + '</td><td align="right">' + v.y2012 + '</td><td align="right">' + v.y2011 + '</td><td align="right">' + v.y2010 + '</td></tr>');
                    sr++;
                });
                $('#observation_panel tbody').append('<tr><td></td><td align="right">Total</td><td align="right">' + t_2023 + '</td><td align="right">' + t_2022 + '</td><td align="right">' + t_2021 + '</td><td align="right">' + t_2020 + '</td><td align="right">' + t_2019 + '</td><td align="right">' + t_2018 + '</td><td align="right">' + t_2017 + '</td><td align="right">' + t_2016 + '</td><td align="right">' + t_2015 + '</td><td align="right">' + t_2014 + '</td><td align="right">' + t_2013 + '</td><td align="right">' + t_2012 + '</td><td align="right">' + t_2011 + '</td><td align="right">' + t_2010 + '</td></tr>');
                initializeDataTable('observation_panel');
            },
            dataType: "json",
        });
    }

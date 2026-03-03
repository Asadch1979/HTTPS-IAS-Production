    $('#document').ready(function () {

        var currentDate = new Date();
        // Set the start date to the start of the current month
        var startOfMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
        var startOfMonthString = startOfMonth.toISOString().split('T')[0];
        $('#loanDisbStartDate').val(startOfMonthString);
        // Set the end date to today's date
        var todayDateString = currentDate.toISOString().split('T')[0];
        $('#loanDisbtEndDate').val(todayDateString);
        $('#loanGLSubField').select2();      
       
    });

    function getZoneOffice() {
        $('#loanZoneField').empty();
        $('#loanZoneField').append('<option value="0">--Select Zone Office--</option>');
        if ($('#loanGMField').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_region_zone_office",
                type: "POST",
                data: {
                    'RGM_ID': $('#loanGMField').val()
                },
                cache: false,
                success: function (data) {

                    $.each(data, function (i, v) {
                        $('#loanZoneField').append('<option value="'+v.code+'">'+v.name+'</option>');
                    });
                },
                dataType: "json",
            });
        }
       

    }

    function getLoanDetail(){

        if ($('#loanZoneField').val() == 0) {
            alert("Select Zone Office to proceed");
            return;
        }
        $('#gldetailtab tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_loan_detail_report",
            type: "POST",
            data: {
                'GLSUBID': $('#loanGLSubField').val(),
                'STATUSID': $('#loanStatusField').val(),
                'START_DATE': $('#loanDisbStartDate').val(),
                'END_DATE': $('#loanDisbtEndDate').val(),
                'ENT_ID': $('#loanZoneField').val(),
            },
            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    $('#gldetailtab tbody').append('<tr><td class="text-center"><a class="text-primary" href = "'+g_asiBaseURL+'/Reports/cnic_loan_report?cnic=' + v.cnic + '"> ' + v.cnic + '</a></td><td class="text-right">' + v.loaN_CASE_NO + '</td > <td>' + v.customername + ' </td>  <td class="text-left">' + v.glsubname + '</td> <td class="text-right"> ' + v.disB_DATE.split(" ")[0] + ' </td> <td class="text-center">' + v.lasT_TRANSACTION_DATE.split(" ")[0] + '</td> <td class="text-right" > ' + v.disB_STATUSID + ' </td><td class="text-right">' + v.lasT_RECOVERY_AMOUNT + '</td> <td class="text-center"> ' + v.principle + ' </td><td class="text-center">' + v.markup + '</td> </tr>')
                    
                });
                initializeDataTable('gldetailtab');
            },
            dataType: "json",
        });
    }

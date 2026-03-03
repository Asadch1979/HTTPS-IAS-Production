    function populateTable() {
        var selectedValue = $('#auditPeriodSelectBox').val();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/audit_periods",
            type: "POST",
            data: {
                'AUDIT_PERIOD_ID': selectedValue
            },
            cache: false,
            success: function (data) {
                console.log(data);
                //$.each(data, function (index, row) {
                //    $('#settled_paras tbody').append('<tr><td>' + ++index + '</td><td>' + row.reportinG_OFFICE + '</td><td>' + row.placE_OF_POSTING + '</td> <td>' + row.audiT_PERIOD + '</td><td>' + row.parA_NO + '</td><td>' + row.gist + '</td><td>' + row.auditeD_BY + '</td><td>' + row.settleD_ON + '</td></tr>');

                //});

               

            },
            dataType: "json",
        });
       

        alert(selectedValue);
    }

    var g_cnic = 0;
    $('#document').ready(function () {

        var url_string = window.location;
        var url = new URL(url_string);
        g_cnic = url.searchParams.get("cnic");
        getCnicLoanDetail();

    });

   
    function getCnicLoanDetail() {

        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_cnic_loan_detail_report",
            type: "POST",
            data: {
                'CNIC': g_cnic,              
            },
            cache: false,
            success: function (data) {


                $.each(data, function (i, v) {

                    $('#gldetailtab tbody').append('<tr><td class="text-center">' + v.cnic + '</td><td class="text-right">' + v.loaN_CASE_NO + '</td><td>' + v.customername + '</td>  <td class="text-left">' + v.glsubname + '</td><td class="text-right">' + v.disB_DATE.split(" ")[0] + '</td> <td class="text-center">' + v.lasT_TRANSACTION_DATE.split(" ")[0] + '</td><td class="text-right">' + v.disB_STATUSID + '</td><td class="text-right">' + v.lasT_RECOVERY_AMOUNT + '</td><td class="text-center">' + v.principle + '</td><td class="text-center">' + v.markup + '</td><td><a href="'+g_asiBaseURL+'/Reports/cnic_default_loan_report?cnic=' + g_cnic + '&disb_id=' + v.loaN_DISB_ID + '">Details</a></td></tr>')

                });
            },
            dataType: "json",
        });
    }

    var g_cnic = 0;
    var g_loan_disb_id = 0;
    $('#document').ready(function () {

        var url_string = window.location;
        var url = new URL(url_string);
        g_cnic = url.searchParams.get("cnic");
        g_loan_disb_id = url.searchParams.get("disb_id");
        getCnicLoanDetail();

    });

   
    function getCnicLoanDetail() {

        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_default_cnic_loan_detail_report",
            type: "POST",
            data: {
                'CNIC': g_cnic,
                'LOAN_DISB_ID': g_loan_disb_id,
              
            },
            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    $('#gldetailtab tbody').append('<tr><td class="text-center">' + v.cnic + '</td><td class="text-center">' + v.transactioN_DATE.split(" ")[0] + '</td><td class="text-center">' + v.currenT_STATUS + '</td>  <td class="text-right">' + v.outstandinG_PRINCIPAL_TOTAL + '</td><td class="text-right">' + v.outstandinG_MARKUP_TOTAL + '</td><td class="text-right">' + v.defaulT_PRINCIPAL + '</td><td class="text-right">' + v.defaulT_MARKUP + '</td></tr>')

                });
            },
            dataType: "json",
        });
    }

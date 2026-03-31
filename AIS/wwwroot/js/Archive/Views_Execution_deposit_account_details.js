    $('#document').ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var cat_id = url.searchParams.get("acccatid");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetDepositAccountcatdetails",
            type: "POST",
            data: {
                'catid': cat_id
            },
            cache: false,
            success: function (data) {

                console.log(data);
                $('#b_d_det tbody').empty();
                $.each(data, function (i, v) {
                    $('#b_d_det tbody').append('<tr><td >' + v.brancH_NAME + ' ' + '</td><td >' + v.customername + '</td><td >' + v.title + '</td><td>' + v.acC_NUMBER + '</td> <td>' + v.accountcategory + '</td><td>' + v.accountstatus + '</td><td>' + v.cnic + ' ' + '</td><td>' + v.cnicexpirydate.split('T')[0] + '</td><td>' + v.bmvS_VERIFIED + '</td><td>' + v.openingdate + '</td>  <td>' + v.lasttransactiondate.split('T')[0] + '</td> </tr>')
                });
            },
            dataType: "json",
        });
    });

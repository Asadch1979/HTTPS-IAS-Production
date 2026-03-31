    $(document).ready(function () {

        getb_depositinfo();

        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#b_d_det tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });


    });


    function getb_depositinfo() {

        var bid = $('#b_n').val();
        if (bid != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetDepositAccountSubdetails",
                type: "POST",
                data: {
                    'b_name': bid
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
        }
    }

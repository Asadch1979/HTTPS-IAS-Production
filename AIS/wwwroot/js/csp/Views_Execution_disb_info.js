    $(document).ready(function () {

        getb_depositinfo();

        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#b_d_T tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });


    });

    function getb_depositinfo() {

        var bid = $('#b_i').val();
        if (bid != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetBranchDesbursementaccountdetails",
                type: "POST",
                data: {
                    'b_id': bid
                },
                cache: false,
                success: function (data) {
                    console.log(data);
                    $('#b_d_T tbody').empty();
                    $.each(data, function (i, v) {
                        $('#b_d_T tbody').append('<tr><td>' + v.loaN_CASE_NO + '</td><td class="pr-2">' + v.cnic + '</td><td>' + v.customername + '</td><td>' + v.disburseD_AMOUNT + '</td><td>' + v.prin + '</td><td>' + v.markup + '</td><td>' + v.disB_DATE.split('T')[0] + '</td>  <td>' + v.disB_STATUSID + '</td> </tr>')
                    });
                },
                dataType: "json",
            });
        }
    }

    $(document).ready(function () {

        getb_depositinfo();

        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#l_c_d_T tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });


    });

    function getb_depositinfo() {

     
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/Getloancasedocuments",
                type: "POST",
                data: {
                 //   'b_id': bid
                },
                cache: false,
                success: function (data) {
                    console.log(data);
                    $('#l_c_d_T tbody').empty();
                    $.each(data, function (i, v) {
                        $('#l_c_d_T tbody').append('<tr><td>' + v.branchcode + '</td><td>' + v.loaN_APP_ID + '</td><td class="pr-2">' + v.cnic + '</td><td>' + v.loaN_CASE_NO + '</td><td>' + v.glsubcode + '</td><td>' + v.customername + '</td><td>' + v.loaN_DISB_ID + '</td><td>' + v.documents + '</td>  <td>' + v.images+ '</td> </tr>')
                    });
                },
                dataType: "json",
            });
        
    }

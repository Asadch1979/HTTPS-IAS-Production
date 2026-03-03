    $('#document').ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var gl_sub_id = url.searchParams.get("glsubcode");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Glhead_Transaction_Details",
            type: "POST",
            data: {
                'GLTYPEID': gl_id
            },
            cache: false,
            success: function (data) {

                $.each(data.gL_SUBDETAILS, function (i, v) {

                    console.log(data);

                    $('#gldetailtab tbody').append('<tr><td class="text-right">' + v.glsubcode + '</td><td>' + v.description + '</td><td>' + v.glsubname + '</td>  <td class="text-right">' + v.credit + '</td><td class="text-right">' + v.debit + '</td> <td class="text-right">' + v.balance + '</td></td> <td>  <a href="#">Details</a></td> </tr>')
                });
            },
            dataType: "json",
        });
    });

    $('#document').ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var gl_id = url.searchParams.get("glcode");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Glhead_Sub_Details",
            type: "POST",
            data: {
                'GLTYPEID': gl_id
            },
            cache: false,
            success: function (data) {

                $.each(data.gL_SUBDETAILS, function (i, v) {

                    console.log(data);

                    /**/
                    $('#gldetailtab tbody').append('<tr><td class="text-right">' + v.glsubcode + '</td><td>' + v.description + '</td><td>' + v.glsubname + '</td>  <td class="text-right">' + v.credit + '</td><td class="text-right">' + v.debit + '</td> <td class="text-right">' + v.balance + '</td></td> <td>  <a href="'+g_asiBaseURL+'/Reports/glhead_transaction_details?glsubcode=' + v.glsubcode + '">Details</a></td> </tr>')
    /**/
});
},
dataType: "json",
});
});

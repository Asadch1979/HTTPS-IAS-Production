    $('#document').ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var gl_id = url.searchParams.get("GLCODE");
        $.ajax({
            url: g_asiBaseURL + "/Reports/Get_Glhead_Sub_Details",
            type: "POST",
            data: {
                'gl_code': gl_id
            },
            cache: false,
            success: function (data) {
                //v = data;

                // console.log(data);

                $.each(data.gL_SUBDETAILS, function (i, v) {

                    $('#gldetailtab tbody').append('<tr><td>' + v.glcode + '</td><td>' + v.glsubname + '</td><td>' + v.datetime.split('T')[0] + '</td>  <td>' + v.runninG_CR + '</td><td>' + v.runninG_DR + '</td> <td>' + v.balance + '</td> </tr>')
                });
            },
            dataType: "json",
        });
    });

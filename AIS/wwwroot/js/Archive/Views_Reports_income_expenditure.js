    var g_engId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var eng_id = url.searchParams.get("engId");
        if (typeof eng_id != 'undefined')
            g_engId = eng_id;
        get_gl_details();

        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#b_d_T tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });


    });

    function get_gl_details() {

        var bid = $('#b_i').val();
        if (bid != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetIncomeExpenceDetails",
                type: "POST",
                data: {
                    'b_id': bid,
                    'ENG_ID':g_engId
                },
                cache: false,
                success: function (data) {
                    console.log(data);
                    $('#b_d_T tbody').empty();
                    console.log(data);
                    $.each(data, function (i, v) {
                        console.log(v);
                        $('#b_d_T tbody').append('<tr><td>' + v.name + '</td><td>' + v.glsubname + '</td><td class="pr-2">' + v.glsubcode + '</td><td>' + v.debit + '</td><td>' + v.credit + '</td> </tr>')
                    });
                },
                dataType: "json",
            });
        }
    }

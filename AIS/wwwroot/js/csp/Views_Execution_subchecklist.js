    var g_endId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var checklist_id = url.searchParams.get("id");
        g_endId = url.searchParams.get("engId");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/sub_checklist",
            type: "POST",
            data: {
                'T_ID': checklist_id,
                'ENG_ID': g_endId
            },
            cache: false,
            success: function (data) {
                console.log('subhcekclist', data);
                $('#subchecklistPanel tbody').empty();
                var sr = 1;
               $.each(data, function (i, v) {
                    $('#subchecklistPanel tbody').append('<tr id="' + v.s_ID + '"><td>' + sr + '</td><td>' + v.t_NAME + '</td><td>' + v.heading + '</td><td class="text-center"><span class="badge text-bg-danger rounded-pill d-inline">' + v.status + '</span></td><td class="text-center"><a href="#" onclick="event.preventDefault();proceedToChecklistDetails(' + v.s_ID + ')">Checklist Details</a></td></tr>');
                   sr++;
                });

            },
            dataType: "json",
        });
    });
    function proceedToChecklistDetails(id) {
        window.location.href = g_asiBaseURL + '/Execution/checklist_details?engId=' + g_endId + '&id=' + id;
    }

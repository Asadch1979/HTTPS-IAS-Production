   
    $(document).ready(function () {

        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#group_wise_pages_grid tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        getGroupWisePages();
     
    });

   
    function getGroupWisePages() {

        $('#group_wise_pages_grid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_group_wise_pages",
            type: "POST",
            data: {
                "GROUP_ID":$('#groupSelectionBox').val()
                },
            cache: false,
            success: function (data) {

                var srNo = 1;
                $.each(data, function (index, row) {
                  
                    $('#group_wise_pages_grid tbody').append('<tr><td>' + srNo + '</td><td>' + row.g_NAME+ '</td><td>' + row.p_NAME+ '</td></tr>');
                    srNo++;
                });

            },
            dataType: "json",
        });


    }

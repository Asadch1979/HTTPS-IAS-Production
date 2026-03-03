   
    $(document).ready(function () {

        $("#searchTableRecord").on("keyup", function ()        
        
        {
            var value = $(this).val().toLowerCase();
            $("#group_wise_user_grid tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        getGroupWiseUsersCount();
     
    }); 

    function getGroupWiseUsersCount() {

        $('#group_wise_user_grid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_group_wise_users_count",
            type: "POST",
            data: {
               
            },
            cache: false,
            success: function (data) {

                var srNo = 1;
                $.each(data, function (index, row) {
                  
                    $('#group_wise_user_grid tbody').append('<tr><td>' + srNo + '</td><td>' + row.g_NAME+ '</td><td>' + row.u_COUNT+ '</td></tr>');
                    srNo++;
                });

            },
            dataType: "json",
        });


    }

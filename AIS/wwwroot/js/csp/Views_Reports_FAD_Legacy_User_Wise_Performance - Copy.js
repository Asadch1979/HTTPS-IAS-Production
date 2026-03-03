    
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#useriwseperformance tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        getLegacyUserWisePerformance();
    });


    function getLegacyUserWisePerformance(){
        
        $('#useriwseperformance tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_user_wise_performance",
            type: "POST",
            data: {
                'FILTER_DATE': $('#updateDateFilter').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (index, child) {
                    var srNo=++index;
                    var onlyDate = "";
                    if (child.date!="")
                    {
                        onlyDate = child.date.split(' ')[0];
                    }
                    $('#useriwseperformance tbody').append('<tr><td><p class="fw-normal mb-1">' + srNo + '</p></td><td><p class="fw-normal mb-1">' + child.zonename + '</p></td><td><p class="fw-normal mb-1">' + child.ppno + '</p></td><td><p class="fw-normal mb-1">' + child.emP_NAME + '</p></td><td><p class="fw-normal mb-1">' + onlyDate + '</p></td><td><p class="fw-normal mb-1">' + child.parA_ENTERED + '</p></td></tr>')
                });

            },

            dataType: "json",
        });
    }

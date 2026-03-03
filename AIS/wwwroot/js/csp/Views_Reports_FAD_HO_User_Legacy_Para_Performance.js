    
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
            url: g_asiBaseURL + "/ApiCalls/get_fad_ho_user_legacy_para_user_wise_performance",
            type: "POST",
            data: {
                'FILTER_DATE': $('#updateDateFilter').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (index, child) {
                    var srNo=++index;
                    
                    $('#useriwseperformance tbody').append('<tr><td><p class="fw-normal mb-1">' + srNo + '</p></td><td><p class="fw-normal mb-1">' + child.emP_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.pP_NO + '</p></td><td><p class="fw-normal mb-1">' + child.parA_REVIEWED + '</p></td><td><p class="fw-normal mb-1">' + child.parA_UPDATED + '</p></td><td><p class="fw-normal mb-1">' + child.parA_UPDATED_WITHOUT_CHANGE + '</p></td><td><p class="fw-normal mb-1">' + child.parA_REFERRED_BACK + '</p></td></tr>')
                });

            },

            dataType: "json",
        });
    }

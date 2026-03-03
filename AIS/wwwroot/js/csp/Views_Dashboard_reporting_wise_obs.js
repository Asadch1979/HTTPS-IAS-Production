    $(document).ready(function () {

        getEntityWiseObservations();
    });

    function getEntityWiseObservations() {
      
        destroyDatatable('entitywise_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_reporting_wise_observations",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var currentT = 0, legacyT = 0, totalT = 0;
                var r1 = 0, r2 = 0, r3 = 0;

             

                $.each(data, function (index, item) {
                    // Accumulate totals
                    currentT += parseInt(item.neW_TOTAL, 10);
                    legacyT += parseInt(item.olD_TOTAL, 10);
                    totalT += parseInt(item.total, 10);
                    r1 += parseInt(item.r1, 10);
                    r2 += parseInt(item.r2, 10);
                    r3 += parseInt(item.r3, 10);

                    // Append row data
                    $('#entitywise_panel tbody').append(
                        `<tr>
                        <td>${sr}</td>
                        <td>${item.reportinG_OFFICE}</td>
                        <td class="text-right">${item.neW_TOTAL}</td>
                        <td class="text-right">${item.olD_TOTAL}</td>
                        <td class="text-right">${item.total}</td>
                        <td class="text-right" style="background-color: #ff968f;">${item.r1}</td>
                        <td class="text-right" style="background-color: #f9e10a6b;">${item.r2}</td>
                        <td class="text-right" style="background-color:#82f386;">${item.r3}</td>
                    </tr>`
                    );
                    sr++;
                });

                $('#entitywise_panel tbody').append(
                    `<tr>
                            <td></td>
                            <td><b>Total</b></td>
                                    <td class="text-right">${currentT}</td>
                                <td class="text-right">${legacyT}</td>
                                <td class="text-right">${totalT}</td>
                                <td class="text-right" style="background-color: #ff968f;">${r1}</td>
                                <td class="text-right" style="background-color: #f9e10a6b;">${r2}</td>
                                <td class="text-right" style="background-color:#82f386;">${r3}</td>
                        </tr>`
                );
                 initializeDataTable('entitywise_panel');
               
                
            },
            dataType: "json",
        });
    }

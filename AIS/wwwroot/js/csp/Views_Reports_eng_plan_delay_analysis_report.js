    $(document).ready(function () {
        getSettledParas();
    });

    function getSettledParas() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_engagement_plan_delay_analysis_report",
            type: "POST",
            data: {
             
            },
            cache: false,
            success: function (data) {

                $.each(data, function (index, row) {
                    if (row.audiT_START_DATE != "")
                        row.audiT_START_DATE = row.audiT_START_DATE.split(" ")[0];
                    if (row.audiT_END_DATE != "")
                        row.audiT_END_DATE = row.audiT_END_DATE.split(" ")[0];
                        
                    $('#settled_paras tbody').append('<tr><td>' + ++index + '</td><td>' + row.reportinG_OFFICE + '</td><td>' + row.placE_OF_POSTING + '</td><td>' + row.entitY_NAME + '</td><td>' + row.audiT_START_DATE + '</td><td>' + row.audiT_END_DATE + '</td><td>' + row.status + '</td><td>' + row.delaY_DAYS + '</td></tr>');

                });

                $('#settled_paras').DataTable({
                    dom: '<"top"lfB>rt<"bottom"ip><"clear">',
                    buttons: [
                        {
                            extend: 'excelHtml5',
                            text: 'Export to Excel',
                            className: 'btn btn-success',
                            exportOptions: {
                                columns: function (idx, data, node) {
                                    // Exclude columns with the class 'hide-export'
                                    return !$(node).hasClass('hide-export');
                                }
                            }
                        },
                        getPdfExportButtonConfig({
                            text: 'Export To PDF',
                            className: 'btn btn-danger',
                            exportOptions: {
                                columns: function (idx, data, node) {
                                    // Exclude columns with the class 'hide-export'
                                    return !$(node).hasClass('hide-export');
                                }
                            }
                        })
                    ],
                    lengthMenu: [
                        [10, 50, 100, -1],
                        [10, 50, 100, "All"]
                    ]
                });

            },
            dataType: "json",
        });


    }

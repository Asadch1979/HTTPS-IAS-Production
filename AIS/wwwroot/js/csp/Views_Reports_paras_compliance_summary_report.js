    $(document).ready(function () {

        getSettledParas();

    });

    function getSettledParas() {
        if ($.fn.DataTable.isDataTable('#compliance_paras')) {
            $('#compliance_paras').DataTable().clear().destroy();
        }
        $('#compliance_paras tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_paras_for_compliance_summary_report",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {

                $.each(data, function (index, row) {

                    row.settlemenT_PERCENTAGE = parseFloat((parseInt(row.totaL_SETTLED_PARAS) / parseInt(row.totaL_PARAS)) * 100).toFixed(2);
                    row.outstandinG_PERCENTAGE = parseFloat((parseInt(row.totaL_OUTSTANDING_PARAS) / parseInt(row.totaL_PARAS)) * 100).toFixed(2);
                    row.compliancE_PENDING_OS_PARAS = parseInt(row.totaL_OUTSTANDING_PARAS) - parseInt(row.zerO_COMPLIANCE_PARAS);
                    $('#compliance_paras tbody').append('<tr><td>' + ++index + '</td><td>' + row.reportinG_OFFICE + '</td><td>' + row.entitY_NAME + '</td><td>' + row.totaL_PARAS + '</td><td>' + row.totaL_SETTLED_PARAS + '</td><td>' + row.totaL_OUTSTANDING_PARAS + '</td><td>' + row.settlemenT_PERCENTAGE + '%</td><td>' + row.outstandinG_PERCENTAGE + '%</td><td>' + row.compliancE_PENDING_OS_PARAS + '</td><td>' + row.zerO_COMPLIANCE_PARAS + '</td></tr>');

                });
               

                $('#compliance_paras').DataTable({
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

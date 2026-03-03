    $(document).ready(function () {

        $('#entitySelectField').select2();

    });

    function getSettledParas() {
        if ($('#selectFromDate').val() == "") {
            alert("Please select From Date to proceed");
            return;
        }
        if ($('#selectEndDate').val() == "") {
            alert("Please select End Date to proceed");
            return;
        }
        if ($.fn.DataTable.isDataTable('#settled_paras')) {
            $('#settled_paras').DataTable().clear().destroy();
        }
        $('#settled_paras tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_settled_paras_for_compliance_report",
            type: "POST",
            data: {
                "ENTITY_TYPE_ID": $('#entitySelectField').val(),
                "DATE_FROM": $('#selectFromDate').val(),
                "DATE_TO": $('#selectEndDate').val()
            },
            cache: false,
            success: function (data) {

                $.each(data, function (index, row) {
                    if (row.settleD_ON != "")
                        row.settleD_ON = row.settleD_ON.split(" ")[0];
                    $('#settled_paras tbody').append('<tr><td>' + ++index + '</td><td>' + row.reportinG_OFFICE + '</td><td>' + row.placE_OF_POSTING + '</td> <td>' + row.audiT_PERIOD + '</td><td>' + row.parA_NO + '</td><td>' + row.gist + '</td><td>' + row.auditeD_BY + '</td><td>' + row.settleD_ON + '</td></tr>');

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

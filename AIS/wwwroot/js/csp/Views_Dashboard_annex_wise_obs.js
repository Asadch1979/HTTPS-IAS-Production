    $(document).ready(function () {
        var roleId = parseInt($("#RoleIdHidden").val(), 10) || 0;
        var showDatePicker = roleId === 1 || roleId === 5;
        var $positionDateInput = $('#annexPositionDate');
        var $validationMessage = $('#annexPositionDateValidation');
        var $searchButton = $('#annexSearchButton');

        if (showDatePicker) {
            $searchButton.prop('disabled', false).removeClass('d-none');
            $positionDateInput.on('change blur', function () {
                if ($(this).val()) {
                    $validationMessage.addClass('d-none');
                }
            });
        } else {
            $searchButton.prop('disabled', true).addClass('d-none');
            getEntityWiseObservations(getTodayDate());
        }

        $searchButton.on('click', function () {
            var referenceDate;

            if (showDatePicker) {
                referenceDate = $positionDateInput.val();
                if (!referenceDate) {
                    $validationMessage.removeClass('d-none');
                    $positionDateInput.focus();
                    return;
                }

                $validationMessage.addClass('d-none');
            } else {
                referenceDate = getTodayDate();
            }

            getEntityWiseObservations(referenceDate);
        });
    });

    function getTodayDate() {
        return new Date().toISOString().split('T')[0];
    }

    function getEntityWiseObservations(positionDate) {
        if (!positionDate) {
            return;
        }

        destroyDatatable('entitywise_panel_mainGrid');
        $('#entitywise_panel_mainGrid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_annex_wise_observations",
            type: "POST",
            data: {
                'P_REF_DATE': positionDate
            },
            cache: false,
            success: function (data) {
                var sr = 1;
                var rowspan = 1;
                var mergeRow = 1;
                var currentT = 0;
                var legacyT = 0;
                var totalT = 0;
                var r1 = 0;
                var r2 = 0;
                var r3 = 0;

                if (Array.isArray(data) && data.length > 0) {
                    $.each(data, function (index, item) {
                        currentT += parseInt(item.neW_TOTAL);
                        legacyT += parseInt(item.olD_TOTAL);
                        totalT += parseInt(item.total);
                        r1 += parseInt(item.r1);
                        r2 += parseInt(item.r2);
                        r3 += parseInt(item.r3);
                        $('#entitywise_panel_mainGrid tbody').append("<tr><td>" + sr + "</td><td>" + item.heading + "</td><td>" + item.annex + "</td><td class=\"text-right\">" + item.neW_TOTAL + "</td><td class=\"text-right\">" + item.olD_TOTAL + "</td><td class=\"text-right\">" + item.total + "</td><td class=\"text-right\" style=\"background-color: #ff968f; \">" + item.r1 + "</td><td class=\"text-right\" style=\"background-color: #f9e10a6b; \">" + item.r2 + "</td><td class=\"text-right\" style=\"background-color:#82f386;\">" + item.r3 + "</td><td class=\"actionsCol\"><a href=\"#\" data-click=\"getParaViewerDetails(" + item.id + ");\">View Detail</a></td><td class=\"actionsCol\"><a href=\"#\" data-click=\"getParaSummaryDetails(" + item.id + ");\">View Summary</a></td></tr>");
                        sr++;
                    });

                    $('#entitywise_panel_mainGrid tbody').append("<tr><td></td><td></td><td><b>Total</b></td><td class=\"text-right text-bold\"><b>" + currentT + "</b></td><td class=\"text-right text-bold\"><b>" + legacyT + "</b></td><td class=\"text-right text-bold\"><b>" + totalT + "</b></td><td class=\"text-right text-bold\"><b>" + r1 + "</b></td><td class=\"text-right text-bold\"><b>" + r2 + "</b></td><td class=\"text-right text-bold\"><b>" + r3 + "</b></td><td></td><td></td></tr>");
                }
                else {
                    $('#entitywise_panel_mainGrid tbody').append('<tr><td colspan="11" class="text-center">No data available for the selected date.</td></tr>');
                }
                initializeDataTable('entitywise_panel_mainGrid');


            },
            dataType: "json",
        });
    }


    function getParaText(id, pc) {
        $('#paraTextViewerModel').modal("show");
        $('#paraTextDivField').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_functional_observation_text",
            type: "POST",
            data: {
                'PARA_ID': id,
                'PARA_CATEGORY': pc
            },
            cache: false,
            success: function (data) {
                $('#paraTextDivField').html(data);
            }
        });
    }

    function getParaViewerDetails(id) {
        $('#paraViewerModel').modal('show');
        destroyDatatable('entitywise_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_analysis_detail_paras",
            type: "POST",
            data: {
                'PROCESS_ID': id
            },
            cache: false,
            success: function (data) {
                var sr = 1;

                $.each(data, function (index, item) {
                    $('#entitywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.name + "</td><td>" + item.audiT_PERIOD + "</td> <td class=\"text-right\">" + item.parA_NO + "</td><td><a href=\"#\" data-click=\"getParaText(" + item.id + ", '" + item.parA_CATEGORY + "');\">View Para Text</a></td></tr>");
                    sr++;
                });
                initializeDataTable('entitywise_panel');

            },
            dataType: "json",
        });
    }

    function getParaSummaryDetails(id) {
        $('#paraSummaryModel').modal('show');
        destroyDatatable('summarywise_panel');
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_analysis_summary_paras",
            type: "POST",
            data: {
                'PROCESS_ID': id
            },
            cache: false,
            success: function (data) {
                var sr = 1;

                $.each(data, function (index, item) {

                    $('#summarywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.p_NAME + "</td><td>" + item.name + "</td><td>" + item.audiT_PERIOD + "</td> <td class=\"text-right\">" + item.parA_NO + "</td></tr>");
                    sr++;
                });
                initializeDataTable('summarywise_panel');

            },
            dataType: "json",
        });
    }

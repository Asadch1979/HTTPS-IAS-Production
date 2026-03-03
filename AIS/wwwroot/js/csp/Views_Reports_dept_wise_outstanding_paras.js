    const storageKeys = {
        refDate: 'deptOutstandingParasRefDate',
        useTrunc: 'deptOutstandingParasUseTrunc'
    };

    function getTodayIsoString() {
        var today = new Date();
        var timezoneOffset = today.getTimezoneOffset();
        var localDate = new Date(today.getTime() - (timezoneOffset * 60000));
        return localDate.toISOString().split('T')[0];
    }

    function updateReportSummary(refDate, useTrunc) {
        var hasDate = refDate && refDate.trim() !== '';
        var summaryDate = hasDate ? refDate : '-';
        var truncOn = useTrunc === 1 || useTrunc === '1' || useTrunc === true;
        var truncLabel = truncOn ? 'On' : 'Off';

        $('#reportRefDate').text(summaryDate);
        $('#reportTruncMode').text(truncLabel);
        $('#entityOutstandingParasCaption').text('Position as on: ' + summaryDate + ' | TRUNC mode: ' + truncLabel);
    }

    $(document).ready(function () {
        $('#entityTypeSelectionField').select2();

        var storedRefDate = sessionStorage.getItem(storageKeys.refDate);
        var todayIso = getTodayIsoString();
        if (storedRefDate && storedRefDate.trim() !== '') {
            $('#refDateField').val(storedRefDate);
        } else {
            $('#refDateField').val(todayIso);
            sessionStorage.setItem(storageKeys.refDate, todayIso);
        }

        var storedUseTrunc = sessionStorage.getItem(storageKeys.useTrunc);
        if (storedUseTrunc !== null) {
            $('#useTruncField').prop('checked', storedUseTrunc === '1');
        } else {
            $('#useTruncField').prop('checked', false);
            sessionStorage.setItem(storageKeys.useTrunc, '0');
        }

        updateReportSummary($('#refDateField').val(), $('#useTruncField').is(':checked') ? 1 : 0);

        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#entity_oustanding_paras_grid tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

        $('#refDateField').on('change blur', function () {
            var value = $(this).val();
            if (value && value.trim() !== '') {
                sessionStorage.setItem(storageKeys.refDate, value);
                $('#refDateValidationMessage').addClass('d-none');
            } else {
                sessionStorage.removeItem(storageKeys.refDate);
            }
            updateReportSummary(value, $('#useTruncField').is(':checked') ? 1 : 0);
        });

        $('#useTruncField').on('change', function () {
            var value = $(this).is(':checked') ? '1' : '0';
            sessionStorage.setItem(storageKeys.useTrunc, value);
            updateReportSummary($('#refDateField').val(), value === '1' ? 1 : 0);
        });

        $('#searchOutstandingParasButton').on('click', function () {
            entityTypeSelectionChangeEvent();
        });

    })

    var slabKeys = [
        'SLAB_1_CURRENT_YEAR',
        'SLAB_2_Age_1_to_3',
        'SLAB_3_Age_4_to_6',
        'SLAB_4_Age_7_to_9',
        'SLAB_5_Age_10_to_12',
        'SLAB_6_Age_12_plus'
    ];

    function resetFooterTotals() {
        $.each(slabKeys, function (_, key) {
            $('#FOOTER_' + key).html('0');
        });
        $('#FOOTER_GRAND_TOTAL').html('0');
    }

    function toNumber(value) {
        if (value === null || value === undefined) {
            return 0;
        }

        var numeric = parseFloat(value.toString().replace(/,/g, '').trim());
        if (isNaN(numeric)) {
            numeric = 0;
        }

        return numeric;
    }

    function entityTypeSelectionChangeEvent() {
        if ($('#entityTypeSelectionField').val() != 0) {
            var refDateValue = $('#refDateField').val();
            if (!refDateValue || refDateValue.trim() === '') {
                $('#refDateValidationMessage').removeClass('d-none');
                $('#refDateField').focus();
                return;
            }

            $('#refDateValidationMessage').addClass('d-none');

            var useTruncValue = $('#useTruncField').is(':checked') ? 1 : 0;
            sessionStorage.setItem(storageKeys.refDate, refDateValue);
            sessionStorage.setItem(storageKeys.useTrunc, useTruncValue.toString());
            updateReportSummary(refDateValue, useTruncValue);

            // Clear the existing table
            // Clear and destroy the existing DataTable instance
            if ($.fn.DataTable.isDataTable('#entity_oustanding_paras_grid')) {
                $('#entity_oustanding_paras_grid').DataTable().clear().destroy();
            }

            // Clear existing rows from tbody
            $('#entity_oustanding_paras_grid tbody').empty();
            resetFooterTotals();

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_outstanding_paras_for_entity_type_id",
                type: "POST",
                data: {
                    "ENTITY_TYPE_ID": $('#entityTypeSelectionField').val(),
                    "P_REF_DATE": refDateValue,
                    "P_USE_TRUNC": useTruncValue
                },
                cache: false,
                success: function (data) {
                    resetFooterTotals();
                    if (data.length > 0) {
                        var entityId = null;
                        var totalParasCount = 0;
                        var sr = 1;
                        var globalTotals = {
                            'SLAB_1_CURRENT_YEAR': 0,
                            'SLAB_2_Age_1_to_3': 0,
                            'SLAB_3_Age_4_to_6': 0,
                            'SLAB_4_Age_7_to_9': 0,
                            'SLAB_5_Age_10_to_12': 0,
                            'SLAB_6_Age_12_plus': 0,
                            'GRAND_TOTAL': 0
                        };

                        $.each(data, function (i, v) {
                            if (entityId === null || v.entitY_ID !== entityId) {
                                if (entityId !== null) {
                                    $('#TOTAL_' + entityId).html(totalParasCount);
                                }

                                entityId = v.entitY_ID;
                                totalParasCount = 0;

                                $('#entity_oustanding_paras_grid tbody').append(
                                    generateTableRow(sr, v.entitY_ID, v.entitY_NAME)
                                );
                                sr++;
                            }

                            var recordValue = toNumber(v.totaL_PARAS);

                            var cellSelector = '#' + v.age + '_' + entityId;
                            var existingValue = toNumber($(cellSelector).text());

                            var updatedValue = existingValue + recordValue;
                            $(cellSelector).html(updatedValue);

                            totalParasCount += recordValue;

                            if (!globalTotals.hasOwnProperty(v.age)) {
                                globalTotals[v.age] = 0;
                            }
                            globalTotals[v.age] += recordValue;
                            globalTotals['GRAND_TOTAL'] += recordValue;
                        });

                        if (entityId !== null) {
                            $('#TOTAL_' + entityId).html(totalParasCount);
                        }

                        $.each(slabKeys, function (_, key) {
                            if (globalTotals.hasOwnProperty(key)) {
                                $('#FOOTER_' + key).html(globalTotals[key]);
                            }
                        });
                        $('#FOOTER_GRAND_TOTAL').html(globalTotals['GRAND_TOTAL']);

                        // Re-initialize DataTable after the table content is updated
                        $('#entity_oustanding_paras_grid').DataTable({
                            dom: '<"top"lfB>rt<"bottom"ip><"clear">',
                            buttons: [
                                {
                                    extend: 'excelHtml5',
                                    text: 'Export to Excel',
                                    className: 'btn btn-success',
                                    exportOptions: {
                                        columns: function (idx, data, node) {
                                            return !$(node).hasClass('hide-export');
                                        },
                                        footer: true
                                    }
                                },
                                getPdfExportButtonConfig({
                                    text: 'Export To PDF',
                                    className: 'btn btn-danger',
                                    exportOptions: {
                                        columns: function (idx, data, node) {
                                            return !$(node).hasClass('hide-export');
                                        },
                                        footer: true
                                    }
                                })
                            ],
                            lengthMenu: [
                                [10, 50, 100, -1],
                                [10, 50, 100, "All"]
                            ]
                        });
                    }
                },
                dataType: "json",
            });
        }
    }

    // Helper function to generate table rows
    function generateTableRow(sr, entityId, entityName) {
        return '<tr id="row_' + entityId + '">' +
            '<td>' + sr + '</td>' + // Sr No.
            '<td>' + entityName + '</td>' + // Entity Name
            '<td class="text-end" id="SLAB_1_CURRENT_YEAR_' + entityId + '">0</td>' + // Current Year Paras
            '<td class="text-end" id="SLAB_2_Age_1_to_3_' + entityId + '">0</td>' + // Age 1 to 3 years Paras
            '<td class="text-end" id="SLAB_3_Age_4_to_6_' + entityId + '">0</td>' + // Age 4 to 6 years Paras
            '<td class="text-end" id="SLAB_4_Age_7_to_9_' + entityId + '">0</td>' + // Age 7 to 9 years Paras
            '<td class="text-end" id="SLAB_5_Age_10_to_12_' + entityId + '">0</td>' + // Age 10 to 12 years Paras
            '<td class="text-end" id="SLAB_6_Age_12_plus_' + entityId + '">0</td>' + // Age 12+ years Paras
            '<td class="text-end" id="TOTAL_' + entityId + '">0</td>' + // Grand Total
            '</tr>';
    }

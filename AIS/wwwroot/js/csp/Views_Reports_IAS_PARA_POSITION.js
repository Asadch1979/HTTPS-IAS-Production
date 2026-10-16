    $(document).ready(function () {
        $('#entityTypeSelectionField').select2();

    });

    function toNumber(value) {
        if (value === null || value === undefined) {
            return 0;
        }

        if (typeof value === 'string') {
            value = value.replace(/,/g, '').trim();
        }

        var numeric = parseFloat(value);
        return isNaN(numeric) ? 0 : numeric;
    }

    function formatNumber(value) {
        if (value === 0) {
            return '0';
        }

        return value % 1 === 0
            ? value.toLocaleString('en-IN')
            : value.toLocaleString('en-IN', { minimumFractionDigits: 0, maximumFractionDigits: 2 });
    }

    function resetTotals() {
        $('#totalIssueIdentified').text('0');
        $('#totalIssueRectified').text('0');
        $('#totalOpeningBalance').text('0');
        $('#totalParaAdded').text('0');
        $('#totalCombined').text('0');
        $('#totalSettledAudit').text('0');
        $('#totalSettledCompliance').text('0');
        $('#totalOutstanding').text('0');
        $('#totalHighRisk').text('0');
        $('#totalMediumRisk').text('0');
        $('#totalLowRisk').text('0');
    }

    function updateTotals(totals) {
        $('#totalIssueIdentified').text(formatNumber(totals.issueIdentified));
        $('#totalIssueRectified').text(formatNumber(totals.issueRectified));
        $('#totalOpeningBalance').text(formatNumber(totals.openingBalance));
        $('#totalParaAdded').text(formatNumber(totals.paraAdded));
        $('#totalCombined').text(formatNumber(totals.totalCombined));
        $('#totalSettledAudit').text(formatNumber(totals.settledAudit));
        $('#totalSettledCompliance').text(formatNumber(totals.settledCompliance));
        $('#totalOutstanding').text(formatNumber(totals.outstanding));
        $('#totalHighRisk').text(formatNumber(totals.highRisk));
        $('#totalMediumRisk').text(formatNumber(totals.mediumRisk));
        $('#totalLowRisk').text(formatNumber(totals.lowRisk));
    }

    function escapeCell(value) {
        return $('<span>').text(value == null ? '' : String(value)).html();
    }

    function entityTypeSelectionChangeEvent() {
        var errorBox = $('#iasParaPositionError').addClass('d-none').text('');
        var startDate = $('#periodStartModalField').val();
        var endDate = $('#periodEndModalField').val();
        if (!$('#entityTypeSelectionField').val() || $('#entityTypeSelectionField').val() === '0' ||
            !startDate || !endDate || startDate > endDate) {
            errorBox.removeClass('d-none').text('Select an entity type and a valid report period; From must not exceed To.');
            return;
        }
        if ($('#entityTypeSelectionField').val() != 0) {
            var showReportingOffice = $('#iasParaPositionGrid').data('show-reporting-office') === true;
            destroyDatatable('iasParaPositionGrid');

            $('#iasParaPositionGrid tbody').empty();
            resetTotals();
            $('#iasParaPositionSearch').prop('disabled', true);

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_ias_para_position_for_entity_type_id",
                type: "POST",
                data: {
                    "ENT_TYPE_ID": $('#entityTypeSelectionField').val(),
                    "S_DATE": $('#periodStartModalField').val(),
                    "E_DATE": $('#periodEndModalField').val()
                },
                cache: false,
                success: function (data) {
                    var totals = {
                        openingBalance: 0,
                        issueIdentified: 0,
                        issueRectified: 0,
                        paraAdded: 0,
                        totalCombined: 0,
                        settledAudit: 0,
                        settledCompliance: 0,
                        outstanding: 0,
                        highRisk: 0,
                        mediumRisk: 0,
                        lowRisk: 0
                    };

                    $.each(data, function (index, record) {
                        var rowNumber = index + 1;
                        var openingBalance = toNumber(record.openinG_BALANCE);
                        var issueIdentified = toNumber(record.issuE_IDENTIFIED);
                        var issueRectified = toNumber(record.issuE_RECTIFIED);
                        var paraAdded = toNumber(record.parA_ADDED);
                        var totalCombined = openingBalance + paraAdded;
                        var settledAudit = toNumber(record.settleD_AUDIT);
                        var settledCompliance = toNumber(record.settleD_COM);
                        var outstanding = toNumber(record.outstanding);
                        var highRisk = toNumber(record.r1);
                        var mediumRisk = toNumber(record.r2);
                        var lowRisk = toNumber(record.r3);

                        totals.openingBalance += openingBalance;
                        totals.issueIdentified += issueIdentified;
                        totals.issueRectified += issueRectified;
                        totals.paraAdded += paraAdded;
                        totals.totalCombined += totalCombined;
                        totals.settledAudit += settledAudit;
                        totals.settledCompliance += settledCompliance;
                        totals.outstanding += outstanding;
                        totals.highRisk += highRisk;
                        totals.mediumRisk += mediumRisk;
                        totals.lowRisk += lowRisk;

                        var reportingOfficeCell = showReportingOffice
                            ? '<td>' + escapeCell(record.reportinG_OFFICE) + '</td>'
                            : '';

                        $('#iasParaPositionGrid tbody').append(
                            '<tr>' +
                            '<td>' + rowNumber + '</td>' +
                            reportingOfficeCell +
                            '<td>' + escapeCell(record.chilD_CODE) + '</td>' +
                            '<td>' + escapeCell(record.placE_OF_POSTING) + '</td>' +
                            '<td class="text-end">' + formatNumber(openingBalance) + '</td>' +
                            '<td class="text-end">' + formatNumber(issueIdentified) + '</td>' +
                            '<td class="text-end">' + formatNumber(issueRectified) + '</td>' +
                            '<td class="text-end">' + formatNumber(paraAdded) + '</td>' +
                            '<td class="text-end">' + formatNumber(totalCombined) + '</td>' +
                            '<td class="text-end">' + formatNumber(settledAudit) + '</td>' +
                            '<td class="text-end">' + formatNumber(settledCompliance) + '</td>' +
                            '<td class="text-end">' + formatNumber(outstanding) + '</td>' +
                            '<td class="text-end">' + formatNumber(highRisk) + '</td>' +
                            '<td class="text-end">' + formatNumber(mediumRisk) + '</td>' +
                            '<td class="text-end">' + formatNumber(lowRisk) + '</td>' +
                            '</tr>'
                        );
                    });

                    updateTotals(totals);
                    initializeDataTable('iasParaPositionGrid');
                },
                error: function () {
                    errorBox.removeClass('d-none').text('Unable to load IAS Para Position. Please try again.');
                },
                complete: function () {
                    $('#iasParaPositionSearch').prop('disabled', false);
                },
                dataType: "json",
            });
        }
    }


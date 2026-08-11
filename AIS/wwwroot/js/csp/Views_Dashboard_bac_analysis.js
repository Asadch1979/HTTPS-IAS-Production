(function () {
    'use strict';
    var bacDsaRows = [];

    function value(row, name) {
        if (row[name] !== undefined) return row[name];
        var normalizedName = name.replace(/_/g, '').toLowerCase();
        var matchingKey = Object.keys(row).find(function (key) {
            return key.replace(/_/g, '').toLowerCase() === normalizedName;
        });
        return matchingKey === undefined ? undefined : row[matchingKey];
    }

    function text(input) {
        return $('<div>').text(input == null ? '' : input).html();
    }

    function number(input) {
        var parsed = Number(input);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function formatNumber(input) {
        return number(input).toLocaleString('en-US', { maximumFractionDigits: 2 });
    }

    function formatDate(dateValue) {
        if (!dateValue) return '';
        var parts = dateValue.split('-');
        var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        return parts[2] + '-' + months[Number(parts[1]) - 1] + '-' + parts[0];
    }

    function showMessage(message) {
        $('#bacAnalysisMessage').text(message).removeClass('d-none');
    }

    function exportOptions(numericColumns) {
        return {
            columns: ':visible:not(.hide-export)',
            format: {
                body: function (data, row, column) {
                    if (column === 0) return row + 1;
                    return numericColumns.indexOf(column) >= 0 ? String(data).replace(/,/g, '') : data;
                }
            }
        };
    }

    function exportButtons(title, numericColumns) {
        return [
            { extend: 'excelHtml5', text: 'Export to Excel', className: 'btn btn-success', title: title, footer: true, exportOptions: exportOptions(numericColumns) },
            getPdfExportButtonConfig({
                text: 'Export to PDF', className: 'btn btn-danger', title: title,
                orientation: 'landscape', pageSize: 'LEGAL', footer: true, exportOptions: exportOptions(numericColumns)
            })
        ];
    }

    function continuousSerialDraw() {
        var api = this.api();
        var start = api.page.info().start;
        api.rows({ page: 'current' }).nodes().each(function (row, index) {
            $('td:first', row).text(start + index + 1);
        });
    }

    function validate() {
        var fromDate = $('#bacFromDate').val();
        var toDate = $('#bacToDate').val();
        if (!fromDate) { showMessage('From Date is mandatory.'); return false; }
        if (!toDate) { showMessage('To Date is mandatory.'); return false; }
        if (fromDate > toDate) { showMessage('From Date cannot be greater than To Date.'); return false; }
        return true;
    }

    function render(rows) {
        if ($.fn.DataTable && $.fn.DataTable.isDataTable('#bacAnalysisTable')) {
            $('#bacAnalysisTable').DataTable().clear().destroy();
        }
        var body = $('#bacAnalysisTable tbody').empty();
        var totals = { issues: 0, rectified: 0, open: 0, dsa: 0, amount: 0 };

        rows.forEach(function (row, index) {
            totals.issues += number(value(row, 'ISSUES_IDENTIFIED'));
            totals.rectified += number(value(row, 'RECTIFIED'));
            totals.open += number(value(row, 'OPEN'));
            totals.dsa += number(value(row, 'DSA'));
            totals.amount += number(value(row, 'AMOUNT_INVOLVED'));
            body.append('<tr><td class="text-end">' + (index + 1) + '</td><td>' + text(value(row, 'PROCESS')) + '</td><td>' + text(value(row, 'ANNEXURE_CODE')) +
                '</td><td>' + text(value(row, 'ANNEXURE')) + '</td><td class="text-end">' + formatNumber(value(row, 'ISSUES_IDENTIFIED')) +
                '</td><td class="text-end">' + formatNumber(value(row, 'RECTIFIED')) + '</td><td class="text-end">' + formatNumber(value(row, 'OPEN')) +
                '</td><td>' + text(value(row, 'AFFECTED_ENTITIES')) + '</td>' +
                '<td class="text-end">' + formatNumber(value(row, 'DSA')) + '</td><td class="text-end">' + formatNumber(value(row, 'AMOUNT_INVOLVED')) + '</td><td><button type="button" class="btn btn-link p-0 bac-view-detail"' +
                ' data-annex-id="' + number(value(row, 'ANNEX_ID')) + '" data-annex-code="' + text(value(row, 'ANNEXURE_CODE')) +
                '" data-annexure="' + text(value(row, 'ANNEXURE')) + '">View</button></td></tr>');
        });

        $('#bacAnalysisTable tfoot').html('<tr><td colspan="4">Total</td><td class="text-end">' + formatNumber(totals.issues) +
            '</td><td class="text-end">' + formatNumber(totals.rectified) + '</td><td class="text-end">' + formatNumber(totals.open) +
            '</td><td></td><td class="text-end">' + formatNumber(totals.dsa) + '</td><td class="text-end">' + formatNumber(totals.amount) + '</td><td></td></tr>');

        if ($.fn.DataTable) {
            $('#bacAnalysisTable').DataTable({
                dom: '<"top"lfB>rt<"bottom"ip><"clear">',
                ordering: true,
                order: [[1, 'asc'], [2, 'asc']],
                columnDefs: [{ targets: 10, orderable: false }],
                drawCallback: continuousSerialDraw,
                buttons: exportButtons('BAC Analysis', [4, 5, 6, 8, 9]),
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
                pageLength: 10
            });
        }
    }

    $(function () {
        if ($.fn.select2) $('#bacRisk').select2({ width: '100%' });
        $('#bacSearch').on('click', function () {
            $('#bacAnalysisMessage').addClass('d-none').empty();
            if (!validate()) return;
            var button = $(this).prop('disabled', true);
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/get_bac_analysis_report', type: 'POST', cache: false, dataType: 'json',
                data: { FROM_DATE: $('#bacFromDate').val(), TO_DATE: $('#bacToDate').val(), RISK_ID: $('#bacRisk').val() }
            }).done(render).fail(function (xhr) {
                showMessage((xhr.responseJSON && xhr.responseJSON.message) || 'Unable to load BAC Analysis. Please try again.');
            }).always(function () { button.prop('disabled', false); });
        });

        $('#bacAnalysisTable').on('click', '.bac-view-detail', function () {
            var button = $(this);
            var fromDate = $('#bacFromDate').val();
            var toDate = $('#bacToDate').val();
            $('#bacDetailTitle').text(button.data('annex-code') + ' - ' + button.data('annexure'));
            $('#bacDetailPeriod').text(formatDate(fromDate) + ' to ' + formatDate(toDate));
            $('#bacDetailMessage').addClass('d-none').empty();
            if ($.fn.DataTable && $.fn.DataTable.isDataTable('#bacDetailTable')) {
                $('#bacDetailTable').DataTable().clear().destroy();
            }
            $('#bacDetailTable tbody, #bacDetailTable tfoot').empty();
            $('#bacDetailModal').modal('show');
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/get_bac_analysis_detail', type: 'POST', dataType: 'json',
                data: { FROM_DATE: fromDate, TO_DATE: toDate, ANNEX_ID: button.data('annex-id'), RISK_ID: $('#bacRisk').val() }
            }).done(function (rows) {
                var instances = 0, amount = 0, dsa = 0, body = $('#bacDetailTable tbody');
                rows.forEach(function (row, index) {
                    instances += number(value(row, 'NO_OF_INSTANCES')); amount += number(value(row, 'AMOUNT'));
                    dsa += number(value(row, 'DSA'));
                    body.append('<tr data-observation-id="' + number(value(row, 'OBSERVATION_ID')) + '"><td class="text-end">' + (index + 1) + '</td><td>' + text(value(row, 'REPORTING_OFFICE')) +
                        '</td><td>' + text(value(row, 'ENTITY')) + '</td><td>' + text(value(row, 'GIST')) + '</td><td class="text-end">' +
                        formatNumber(value(row, 'NO_OF_INSTANCES')) + '</td><td class="text-end">' + formatNumber(value(row, 'AMOUNT')) +
                        '</td><td>' + text(value(row, 'PARA_STATUS')) + '</td><td class="text-end">' + formatNumber(value(row, 'DSA')) +
                        '</td><td><button type="button" class="btn btn-link p-0 bac-view-para">View Para</button></td>' +
                        '<td><button type="button" class="btn btn-link p-0 bac-view-dsa">View DSA</button></td></tr>');
                });
                $('#bacDetailTable tfoot').html('<tr><td colspan="4">Total</td><td class="text-end">' + formatNumber(instances) +
                    '</td><td class="text-end">' + formatNumber(amount) + '</td><td></td><td class="text-end">' + formatNumber(dsa) + '</td><td></td><td></td></tr>');
                if ($.fn.DataTable) {
                    $('#bacDetailTable').DataTable({
                        dom: '<"top"lfB>rt<"bottom"ip><"clear">', ordering: true, order: [[1, 'asc'], [2, 'asc']],
                        columnDefs: [{ targets: [8, 9], orderable: false }], drawCallback: continuousSerialDraw,
                        buttons: exportButtons('BAC Annexure Detail', [4, 5, 7]),
                        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']], pageLength: 10
                    });
                }
            }).fail(function (xhr) {
                $('#bacDetailMessage').text((xhr.responseJSON && xhr.responseJSON.message) || 'Unable to load Annexure details.').removeClass('d-none');
            });
        });

        $('#bacDetailTable').on('click', '.bac-view-para', function () {
            var observationId = $(this).closest('tr').data('observation-id');
            $('#bacParaHeading').text('Para Text'); $('#bacParaText').empty(); $('#bacParaMessage').addClass('d-none').empty();
            $.ajax({ url: g_asiBaseURL + '/ApiCalls/get_bac_para_text', type: 'POST', dataType: 'json', data: { OBSERVATION_ID: observationId } })
                .done(function (row) {
                    $('#bacParaHeading').text(value(row, 'HEADING') || 'Para Text');
                    $('#bacParaText').html(value(row, 'PARA_TEXT') || '');
                    $('#bacParaModal').modal('show');
                }).fail(function (xhr) {
                    $('#bacParaMessage').text((xhr.responseJSON && xhr.responseJSON.message) || 'Unable to load para text.').removeClass('d-none');
                    $('#bacParaModal').modal('show');
                });
        });

        $('#bacDetailTable').on('click', '.bac-view-dsa', function () {
            var observationId = $(this).closest('tr').data('observation-id');
            if ($.fn.DataTable && $.fn.DataTable.isDataTable('#bacDsaTable')) {
                $('#bacDsaTable').DataTable().clear().destroy();
            }
            bacDsaRows = [];
            $('#bacDsaTable tbody').empty(); $('#bacDsaMessage').addClass('d-none').empty();
            $('#bacDsaModal').modal('show');
            $.ajax({ url: g_asiBaseURL + '/ApiCalls/get_bac_dsa_details', type: 'POST', dataType: 'json', data: { OBSERVATION_ID: observationId } })
                .done(function (rows) {
                    bacDsaRows = rows;
                    var body = $('#bacDsaTable tbody');
                    rows.forEach(function (row, index) {
                        body.append('<tr><td class="text-end">' + (index + 1) + '</td><td>' + text(value(row, 'PPNO')) + '</td><td>' +
                            text(value(row, 'EMP_NAME')) + '</td><td><button type="button" class="btn btn-link p-0 bac-view-dsa-text" data-dsa-index="' +
                            index + '">View DSA</button></td></tr>');
                    });
                    if ($.fn.DataTable) {
                        $('#bacDsaTable').DataTable({
                            dom: '<"top"lfB>rt<"bottom"ip><"clear">', ordering: true, order: [[1, 'asc']],
                            columnDefs: [{ targets: 3, orderable: false }], drawCallback: continuousSerialDraw,
                            buttons: exportButtons('BAC DSA Details', []),
                            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']], pageLength: 10
                        });
                    }
                }).fail(function (xhr) {
                    $('#bacDsaMessage').text((xhr.responseJSON && xhr.responseJSON.message) || 'Unable to load DSA details.').removeClass('d-none');
                });
        });

        $('#bacDsaTable').on('click', '.bac-view-dsa-text', function () {
            var row = bacDsaRows[Number($(this).data('dsa-index'))];
            if (!row) return;
            $('#bacDsaTextHeading').text((value(row, 'EMP_NAME') || 'DSA') + ' - ' + (value(row, 'PPNO') || ''));
            $('#bacDsaText').html(value(row, 'DSA_TEXT') || '');
            $('#bacDsaTextModal').modal('show');
        });
    });
}());

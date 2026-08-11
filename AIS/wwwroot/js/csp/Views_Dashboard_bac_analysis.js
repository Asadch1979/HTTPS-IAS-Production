(function () {
    'use strict';

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

    function showMessage(message) {
        $('#bacAnalysisMessage').text(message).removeClass('d-none');
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
        var totals = { issues: 0, rectified: 0, open: 0, amount: 0 };
        var hasOpenEntities = rows.some(function (row) { return value(row, 'HAS_OPEN_ENTITIES_COLUMN') === true; });
        $('.bac-open-entities').toggleClass('d-none', !hasOpenEntities);

        rows.forEach(function (row) {
            totals.issues += number(value(row, 'ISSUES_IDENTIFIED'));
            totals.rectified += number(value(row, 'RECTIFIED'));
            totals.open += number(value(row, 'OPEN'));
            totals.amount += number(value(row, 'AMOUNT_INVOLVED'));
            body.append('<tr><td>' + text(value(row, 'PROCESS')) + '</td><td>' + text(value(row, 'ANNEXURE_CODE')) +
                '</td><td>' + text(value(row, 'ANNEXURE')) + '</td><td class="text-end">' + formatNumber(value(row, 'ISSUES_IDENTIFIED')) +
                '</td><td class="text-end">' + formatNumber(value(row, 'RECTIFIED')) + '</td><td class="text-end">' + formatNumber(value(row, 'OPEN')) +
                '</td><td>' + text(value(row, 'AFFECTED_ENTITIES')) + '</td>' +
                '<td class="bac-open-entities' + (hasOpenEntities ? '' : ' d-none') + '">' + text(value(row, 'OPEN_ENTITIES')) + '</td>' +
                '<td class="text-end">' + formatNumber(value(row, 'AMOUNT_INVOLVED')) + '</td></tr>');
        });

        $('#bacAnalysisTable tfoot').html('<tr><td colspan="3">Total</td><td class="text-end">' + formatNumber(totals.issues) +
            '</td><td class="text-end">' + formatNumber(totals.rectified) + '</td><td class="text-end">' + formatNumber(totals.open) +
            '</td><td></td><td class="bac-open-entities' + (hasOpenEntities ? '' : ' d-none') + '"></td><td class="text-end">' + formatNumber(totals.amount) + '</td></tr>');

        if ($.fn.DataTable) {
            $('#bacAnalysisTable').DataTable({
                dom: '<"top"lfB>rt<"bottom"ip><"clear">',
                ordering: true,
                order: [[0, 'asc'], [1, 'asc']],
                columnDefs: [{ targets: 7, visible: hasOpenEntities }],
                buttons: [
                    {
                        extend: 'excelHtml5',
                        text: 'Export to Excel',
                        className: 'btn btn-success',
                        footer: true,
                        exportOptions: { columns: ':visible' }
                    },
                    getPdfExportButtonConfig({
                        text: 'Export to PDF',
                        className: 'btn btn-danger',
                        orientation: 'landscape',
                        pageSize: 'LEGAL',
                        footer: true,
                        exportOptions: { columns: ':visible' }
                    })
                ],
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
    });
}());

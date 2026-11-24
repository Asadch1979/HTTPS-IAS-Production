(function ($) {
    'use strict';

    $(function () {
        var root = $('#riskEngine');
        if (!root.length) return;

        var period = $('#enginePeriod');
        var status = $('#engineStatus');
        var busy = false;
        var lastRequest = 0;

        function show(message) {
            status.text(message || '');
        }

        function fail(xhr) {
            var response = xhr && xhr.responseJSON;
            var ref = xhr && xhr.getResponseHeader &&
                xhr.getResponseHeader('X-Error-Reference-Id');
            var message = (response && response.message) ||
                (xhr.status === 403
                    ? 'Super User permission or Engine API activation is missing.'
                    : xhr.status === 401
                        ? 'Your IAS session has expired.'
                        : 'Request failed. Review the IAS error log.');
            show(message + (ref ? ' Reference: ' + ref : ''));
        }

        function display(value) {
            return value == null || value === '' ? '—' : String(value);
        }

        function fetchResults(params, onData) {
            $.getJSON(root.attr('data-results'), params)
                .done(function (response) {
                    if (!response || response.success !== true || !response.data) {
                        show('The result service returned an invalid response.');
                        return;
                    }
                    onData(response.data);
                })
                .fail(fail);
        }

        function detail(resultId) {
            if (!resultId) return;
            fetchResults({ period: period.val(), result: resultId }, function (data) {
                var container = $('#engineDetails').empty();
                (data.exceptions || []).forEach(function (item) {
                    container.append($('<p>').text(item.code + ': ' + item.message));
                });

                var table = $('<table>').addClass('table table-sm table-bordered');
                var heading = $('<tr>');
                ['Level', 'Component', 'Annexure', 'Observations', 'Instances',
                    'Marks', 'Status'].forEach(function (name) {
                    heading.append($('<th>').text(name));
                });
                table.append($('<thead>').append(heading));
                var body = $('<tbody>');
                (data.detail || []).forEach(function (item) {
                    var row = $('<tr>');
                    [item.node_level, item.process_code || item.procedure_code,
                        item.source_code || item.annex_id,
                        item.observations, item.instances,
                        item.score, item.status].forEach(function (value) {
                        row.append($('<td>').text(display(value)));
                    });
                    body.append(row);
                });
                if (!body.children().length) {
                    body.append($('<tr>').append($('<td>')
                        .attr('colspan', 7).text('No component detail recorded.')));
                }
                container.append(table.append(body));
            });
        }

        function render(rows, id, emptyMessage) {
            var body = $(id).empty();
            if (!Array.isArray(rows) || !rows.length) {
                body.append($('<tr>').append($('<td>')
                    .attr('colspan', 10).text(emptyMessage)));
                return;
            }

            rows.forEach(function (item) {
                var tr = $('<tr>');
                var calculated = item.status === 'CALCULATED';
                [
                    item.period_id, item.entity_name, item.eng_id,
                    item.observations, item.instances,
                    calculated ? item.score : null,
                    calculated ? item.risk_rating : null,
                    display(item.status).replace(/_/g, ' '),
                    item.calculated_on
                ].forEach(function (value) {
                    tr.append($('<td>').text(display(value)));
                });
                var details = $('<td>');
                if (item.result_id != null) {
                    details.append($('<button>').attr('type', 'button')
                        .addClass('btn btn-sm btn-link')
                        .text('Components / inputs')
                        .on('click', function () { detail(item.result_id); }));
                } else {
                    details.text('Not executed');
                }
                tr.append(details);
                body.append(tr);
            });
        }

        function load() {
            var auditPeriod = period.val();
            if (!auditPeriod) {
                show('Select an audit period.');
                return;
            }
            var request = ++lastRequest;
            fetchResults({ period: auditPeriod }, function (data) {
                if (request !== lastRequest) return;
                render(data.current, '#engineCurrentRows',
                    'No audited branches found for the selected period.');
                render(data.cursor, '#engineRows',
                    'No previous risk calculations for this period.');
                show((data.current || []).length +
                    ' branch engagement(s) available for Audit Period ' +
                    auditPeriod + '.');
            });
        }

        function calculate() {
            var auditPeriod = period.val();
            if (busy || !auditPeriod) return;
            if (!window.confirm('Calculate Excel-model risk ratings for ALL ' +
                'audited branches (Entity Type 6) in Audit Period ' +
                auditPeriod + '? A new results history will be saved.')) return;

            var token = root.find('input[name="__RequestVerificationToken"]').val();
            if (!token) {
                show('Antiforgery token missing. Reload the page.');
                return;
            }

            busy = true;
            root.find('button,select').prop('disabled', true);
            show('Calculating branch risk ratings...');
            $.post(root.attr('data-bulk'), {
                period: auditPeriod, __RequestVerificationToken: token
            }).done(function (response) {
                if (!response || response.success !== true || !response.data) {
                    show('Execution response is incomplete. Check history before retrying.');
                    return;
                }
                var r = response.data;
                show('Run ' + r.runId + ': ' + r.calculated +
                    ' rated, ' + r.pending + ' pending inputs, ' +
                    r.failed + ' failed.');
                load();
            }).fail(function (xhr) {
                fail(xhr);
                // A timeout may have committed; inspect saved history before retry.
            }).always(function () {
                busy = false;
                root.find('button,select').prop('disabled', false);
            });
        }

        $('#engineLoad').on('click', load);
        $('#engineBulk').on('click', calculate);
        period.empty().append($('<option>').val('')
            .text('Loading audit periods...'));
        root.find('button').prop('disabled', true);

        $.getJSON(root.attr('data-catalog')).done(function (response) {
            var rows = response && response.data && response.data.cursor;
            if (!response || response.success !== true ||
                !Array.isArray(rows)) {
                show('Audit period service returned unexpected data.');
                return;
            }

            period.empty().append($('<option>').val('')
                .text('Select audit period'));
            rows.forEach(function (item) {
                var id = item.auditperiodid;
                if (id == null) return;
                period.append($('<option>').val(String(id))
                    .text(display(item.description) + ' (' + id + ')'));
            });
            if (!rows.length) {
                show('No audit periods found in T_AU_PERIOD.');
                return;
            }
            if (rows.some(function (item) {
                return Number(item.auditperiodid) === 5;
            })) period.val('5');
            else period.prop('selectedIndex', 1);

            root.find('button').prop('disabled', false);
            load();
        }).fail(fail);
    });
})(jQuery);

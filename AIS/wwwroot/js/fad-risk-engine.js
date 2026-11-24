(function ($) {
    'use strict';

    $(function () {
        var root = $('#riskEngine');
        if (!root.length) return;

        var period = $('#enginePeriod');
        var type = $('#engineType');
        var entity = $('#engineEntity');
        var status = $('#engineStatus');
        var catalog = [];
        var busy = false;
        var requestNumber = 0;

        function message(value) {
            status.text(value || '');
        }

        function errorMessage(xhr) {
            var detail = xhr && xhr.responseJSON && xhr.responseJSON.message;
            var code = xhr && xhr.status;
            var ref = xhr && xhr.getResponseHeader && xhr.getResponseHeader('X-Error-Reference-Id');
            var fallback = code === 401 ? 'Your session has expired. Sign in again.'
                : code === 403 ? 'Access denied. Verify Super User access and the four engine API grants.'
                : 'The request failed. Review the IAS system error log.';
            return (detail || fallback) + (ref ? ' Request trace: ' + ref : '');
        }

        function fail(xhr) {
            message(errorMessage(xhr));
        }

        function keyOf(value) {
            return value == null ? '' : String(value);
        }

        function fill(select, rows, key, label, prompt, allowAll, preferred) {
            var seen = {};
            select.empty().append($('<option>').val('').text(prompt));
            if (allowAll) select.append($('<option>').val('0').text('All entities'));
            (rows || []).forEach(function (row) {
                var value = keyOf(row[key]);
                if (!value || seen[value]) return;
                seen[value] = true;
                select.append($('<option>').val(value).text(keyOf(row[label]) || value));
            });
            var choice = keyOf(preferred);
            if (choice && select.find('option').filter(function () { return this.value === choice; }).length) {
                select.val(choice);
            } else if (allowAll) {
                select.val('0');
            } else if (select.find('option').length > 1) {
                select.prop('selectedIndex', 1);
            } else {
                select.val('');
            }
        }

        function selected() {
            return { period: period.val() || '', type: type.val() || '',
                entity: entity.val() || '0' };
        }

        function buttons() {
            var values = selected();
            root.find('button').prop('disabled', busy);
            $('#engineLoad').prop('disabled', busy || !values.period);
            $('#engineBulk').prop('disabled', busy || !values.period || !values.type);
            $('#engineSingle').prop('disabled', busy || !values.period || Number(values.entity) <= 0);
            root.find('select').prop('disabled', busy);
        }

        period.on('change', function () {
            var oldType = type.val();
            var rows = catalog.filter(function (row) {
                return keyOf(row.period_id) === period.val();
            });
            fill(type, rows, 'type_id', 'type_id', 'Select entity type', false, oldType);
            type.trigger('change');
            buttons();
        });

        type.on('change', function () {
            var oldEntity = entity.val();
            var rows = catalog.filter(function (row) {
                return keyOf(row.period_id) === period.val()
                    && keyOf(row.type_id) === type.val();
            });
            fill(entity, rows, 'entity_id', 'name', 'Select entity', true, oldEntity);
            buttons();
        });

        function getResults(params, callback) {
            return $.getJSON(root.attr('data-results'), params).done(function (response) {
                if (!response || response.success !== true || !response.data) {
                    message('The results service returned an invalid response.');
                    return;
                }
                callback(response.data);
            }).fail(fail);
        }

        function details(resultId) {
            var params = selected();
            params.result = resultId;
            getResults(params, function (data) {
                var box = $('#engineDetails').empty();
                (data.exceptions || []).forEach(function (item) {
                    box.append($('<p>').text(item.code + ': ' + item.message));
                });
                var table = $('<table>').addClass('table table-sm');
                var head = $('<tr>');
                ['Level', 'Source code', 'Observation', 'Instances', 'Status'].forEach(function (label) {
                    head.append($('<th>').text(label));
                });
                table.append($('<thead>').append(head));
                var body = $('<tbody>');
                (data.detail || []).forEach(function (item) {
                    var tr = $('<tr>');
                    [item.node_level, item.source_code || item.procedure_code ||
                        item.process_code || item.package_code || item.main_code,
                        item.observation_id, item.instances, item.status].forEach(function (value) {
                        tr.append($('<td>').text(value == null ? '—' : value));
                    });
                    body.append(tr);
                });
                if (!body.children().length && !(data.exceptions || []).length) {
                    box.append($('<p>').text('No details are recorded for this result.'));
                }
                box.append(table.append(body));
            });
        }

        function render(rows, target, emptyText) {
            var body = $(target).empty();
            if (!rows.length) {
                body.append($('<tr>').append($('<td>').attr('colspan', 9).text(emptyText)));
                return;
            }
            rows.forEach(function (row) {
                var tr = $('<tr>');
                var state = row.status || 'UNKNOWN';
                var pending = state === 'PENDING_INPUT';
                var rating = pending ? 'Pending Input' : row.risk_rating;
                var score = pending ? 'Pending Input' : row.score;
                [row.period_id, row.entity_name, row.eng_id, row.observations, score,
                    rating, state.replace(/_/g, ' '), row.calculated_on].forEach(function (value) {
                    tr.append($('<td>').text(value == null ? '—' : value));
                });
                var cell = $('<td>');
                if (row.result_id) {
                    cell.append($('<button>').attr('type', 'button')
                        .addClass('btn btn-sm btn-link')
                        .text('Run ' + row.run_id + ' / details')
                        .on('click', function () { details(row.result_id); }));
                    if (row.processed != null) {
                        cell.append($('<div>').text(
                            row.processed + ' processed; ' + row.calculated + ' calculated; ' +
                            row.pending + ' pending; ' + row.skipped + ' skipped; ' +
                            row.failed + ' failed.'));
                    }
                } else {
                    cell.text('Not executed');
                }
                tr.append(cell);
                body.append(tr);
            });
        }

        function load() {
            var params = selected();
            if (!params.period) {
                message('Select an audit period first.');
                return;
            }
            var number = ++requestNumber;
            getResults(params, function (data) {
                if (number !== requestNumber) return;
                if (!Array.isArray(data.current) || !Array.isArray(data.cursor)) {
                    message('The results service returned an unexpected data format.');
                    return;
                }
                render(data.current, '#engineCurrentRows', 'No engagements for these filters.');
                render(data.cursor, '#engineRows', 'No previous execution history.');
                message(data.current.length + ' engagement(s) loaded.');
            });
        }

        function run(bulk) {
            if (busy) return;
            var values = selected();
            if (!values.period || (bulk && !values.type) ||
                (!bulk && Number(values.entity) <= 0)) {
                message(bulk ? 'Select audit period and entity type.'
                    : 'Select audit period and one branch.');
                return;
            }
            var question = bulk
                ? 'Execute all eligible engagements for the selected period and entity type?'
                : 'Recalculate all eligible engagements for this branch and period?';
            if (!window.confirm(question)) return;

            var token = root.find('input[name="__RequestVerificationToken"]').val();
            if (!token) {
                message('Security token is missing. Reload the page.');
                return;
            }
            var payload = { period: values.period, __RequestVerificationToken: token };
            if (bulk) payload.type = values.type;
            else payload.entity = values.entity;

            busy = true;
            buttons();
            message('Executing risk register. Do not resubmit the same request.');
            $.post(root.attr(bulk ? 'data-bulk' : 'data-single'), payload)
                .done(function (response) {
                    if (!response || response.success !== true || !response.data) {
                        message('Execution response was incomplete. Refresh results before retrying.');
                        return;
                    }
                    var r = response.data;
                    message('Run ' + r.runId + ': ' + r.processed + ' processed, ' +
                        r.calculated + ' calculated, ' + r.pending + ' pending, ' +
                        r.skipped + ' skipped, ' + r.failed + ' failed.');
                    load();
                })
                .fail(function (xhr) {
                    fail(xhr);
                    // A timed-out write may still have committed; review history before retrying.
                })
                .always(function () {
                    busy = false;
                    buttons();
                });
        }

        $('#engineLoad').on('click', load);
        $('#engineBulk').on('click', function () { run(true); });
        $('#engineSingle').on('click', function () { run(false); });
        fill(period, [], 'period_id', 'period_id', 'Loading audit periods...', false);
        fill(type, [], 'type_id', 'type_id', 'Select entity type', false);
        fill(entity, [], 'entity_id', 'name', 'Select entity', true);
        buttons();

        $.getJSON(root.attr('data-catalog'))
            .done(function (response) {
                var rows = response && response.data && response.data.cursor;
                if (!response || response.success !== true || !Array.isArray(rows)) {
                    message('The engine catalog returned an unexpected response.');
                    return;
                }
                catalog = rows;
                if (!catalog.length) {
                    fill(period, [], 'period_id', 'period_id', 'No audit periods found', false);
                    message('No audit engagements were returned. Check the engine catalog and its Super User API permission.');
                    buttons();
                    return;
                }
                fill(period, catalog, 'period_id', 'period_id',
                    'Select audit period', false,
                    catalog.some(function (row) { return keyOf(row.period_id) === '5'; }) ? '5' : '');
                period.trigger('change');
                load();
            })
            .fail(fail);
    });
})(jQuery);

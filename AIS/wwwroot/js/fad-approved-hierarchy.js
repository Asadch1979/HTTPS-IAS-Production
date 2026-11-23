(function ($) {
    'use strict';
    $(function () {
        var root = $('#fadApprovedRegister');
        if (!root.length) return;
        var rows = [];
        var main = $('#registerMain'), pack = $('#registerPackage');
        var process = $('#registerProcess'), procedure = $('#registerProcedure');
        var status = $('#registerStatus');
        function options(select, source, id, code, name, all) {
            select.empty();
            if (all) select.append($('<option>').val('').text('All procedures'));
            var seen = new Set();
            source.forEach(function (r) {
                var key = String(r[id]);
                if (seen.has(key)) return;
                seen.add(key);
                select.append($('<option>').val(key).text(r[code] + ' — ' + r[name]));
            });
            select.prop('disabled', !source.length);
        }
        function subset() {
            return rows.filter(function (r) {
                return String(r.main_code) === main.val() && String(r.package_code) === pack.val()
                    && String(r.process_code) === process.val();
            });
        }
        function reconciliationLabel(value) {
            return { MATCHED: 'Matched', PENDING_RECONCILIATION: 'Pending reconciliation',
                CONFLICTING_IAS_CODE: 'Pending reconciliation: duplicate IAS code',
                INVALID_IAS_ID: 'Pending reconciliation: missing IAS ID',
                CONFLICTING_IAS_ID: 'Pending reconciliation: duplicate IAS ID' }[value] || 'Pending reconciliation';
        }
        function render() {
            var selected = subset().filter(function (r) { return !procedure.val() || String(r.procedure_code) === procedure.val(); });
            var body = $('#registerRows').empty();
            selected.forEach(function (r) {
                var tr = $('<tr>');
                ['procedure_code', 'procedure_name', 'source_annexure_code', 'annexure_code', 'annexure_id', 'violation_description', 'risk_classification', 'reconciliation_status'].forEach(function (key) {
                    var value = r[key];
                    if (key === 'reconciliation_status') value = reconciliationLabel(value);
                    if (key === 'annexure_id' && value == null) value = 'Unresolved';
                    tr.append($('<td>').addClass('text-break').text(value == null ? '' : value));
                });
                body.append(tr);
            });
            var pending = selected.filter(function (r) { return r.reconciliation_status !== 'MATCHED'; }).length;
            var totalPending = rows.filter(function (r) { return r.reconciliation_status !== 'MATCHED'; }).length;
            status.text(selected.length + ' Annexures displayed; ' + pending + ' pending in this selection. '
                + rows.length + ' total source Annexures; ' + totalPending + ' pending reconciliation overall.');
        }
        main.on('change', function () {
            options(pack, rows.filter(function (r) { return String(r.main_code) === main.val(); }), 'package_code', 'package_code', 'package_name');
            pack.trigger('change');
        });
        pack.on('change', function () {
            options(process, rows.filter(function (r) { return String(r.main_code) === main.val() && String(r.package_code) === pack.val(); }), 'process_code', 'process_code', 'process_name');
            process.trigger('change');
        });
        process.on('change', function () {
            options(procedure, subset(), 'procedure_code', 'procedure_code', 'procedure_name', true);
            render();
        });
        procedure.on('change', render);
        $.getJSON(root.attr('data-source-url')).done(function (response) {
            if (!response || response.success !== true || !Array.isArray(response.data)) {
                status.text('The approved hierarchy could not be loaded.'); return;
            }
            rows = response.data.map(function (row) {
                var normalized = {};
                Object.keys(row).forEach(function (key) { normalized[key.toLowerCase()] = row[key]; });
                return normalized;
            });
            var pendingRows = rows.filter(function (r) { return r.reconciliation_status !== 'MATCHED'; });
            var pendingList = $('#registerPending').empty();
            pendingRows.forEach(function (r) {
                pendingList.append($('<li>').text(r.source_annexure_code + ' — ' + r.procedure_code
                    + ' — ' + r.procedure_name + ' — ' + reconciliationLabel(r.reconciliation_status)));
            });
            $('#registerPendingPanel').prop('hidden', pendingRows.length === 0);
            options(main, rows, 'main_code', 'main_code', 'main_name');
            main.trigger('change');
        }).fail(function (xhr) {
            status.text(xhr.status === 401 ? 'Your session has expired. Sign in again.' : xhr.status === 403 ? 'You do not have permission to view this register.' : 'The approved hierarchy is unavailable. Contact the administrator.');
        });
    });
})(jQuery);

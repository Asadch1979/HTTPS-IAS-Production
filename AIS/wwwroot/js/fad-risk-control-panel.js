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
                return String(r.kpi_main_id) === main.val() && String(r.package_id) === pack.val()
                    && String(r.process_id) === process.val();
            });
        }
        function render() {
            var selected = subset().filter(function (r) { return !procedure.val() || String(r.procedure_id) === procedure.val(); });
            var body = $('#registerRows').empty();
            selected.forEach(function (r) {
                var tr = $('<tr>');
                ['procedure_code', 'procedure_name', 'source_annexure_code', 'annexure_code', 'annexure_id', 'violation_description', 'risk_classification'].forEach(function (key) {
                    tr.append($('<td>').addClass('text-break').text(r[key] == null ? '' : r[key]));
                });
                body.append(tr);
            });
            status.text(selected.length ? selected.length + ' Annexure mappings displayed.' : 'No approved mappings available for this selection.');
        }
        main.on('change', function () {
            options(pack, rows.filter(function (r) { return String(r.kpi_main_id) === main.val(); }), 'package_id', 'package_code', 'package_name');
            pack.trigger('change');
        });
        pack.on('change', function () {
            options(process, rows.filter(function (r) { return String(r.kpi_main_id) === main.val() && String(r.package_id) === pack.val(); }), 'process_id', 'process_code', 'process_name');
            process.trigger('change');
        });
        process.on('change', function () {
            options(procedure, subset(), 'procedure_id', 'procedure_code', 'procedure_name', true);
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
            options(main, rows, 'kpi_main_id', 'main_code', 'main_name');
            main.trigger('change');
        }).fail(function (xhr) {
            status.text(xhr.status === 401 ? 'Your session has expired. Sign in again.' : xhr.status === 403 ? 'You do not have permission to view this register.' : 'The approved hierarchy is unavailable. Contact the administrator.');
        });
    });
})(jQuery);

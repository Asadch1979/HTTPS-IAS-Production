(function () {
    var loadedEntities = [];

    function isBlank(value) {
        return !value || value.toString().trim() === '';
    }

    function hideValidation() {
        $('#auditDepartmentValidationMessage').addClass('d-none');
        $('#executionFromValidationMessage').addClass('d-none');
        $('#executionToValidationMessage').addClass('d-none');
        $('#dateRangeValidationMessage').addClass('d-none');
    }

    function showMessage(message, type) {
        var $message = $('#outstandingParasMessage');
        $message
            .removeClass('d-none alert-info alert-success alert-warning alert-danger')
            .addClass('alert-' + (type || 'info'))
            .text(message || '');
    }

    function hideMessage() {
        $('#outstandingParasMessage').addClass('d-none').text('');
    }

    function validateFilters() {
        hideValidation();

        var isValid = true;
        var auditDepartmentId = $('#auditDepartmentField').val();
        var executionFromDate = $('#executionFromDateField').val();
        var executionToDate = $('#executionToDateField').val();

        if (auditDepartmentId === '0' || isBlank(auditDepartmentId)) {
            $('#auditDepartmentValidationMessage').removeClass('d-none');
            isValid = false;
        }

        if (isBlank(executionFromDate)) {
            $('#executionFromValidationMessage').removeClass('d-none');
            isValid = false;
        }

        if (isBlank(executionToDate)) {
            $('#executionToValidationMessage').removeClass('d-none');
            isValid = false;
        }

        if (!isBlank(executionFromDate) && !isBlank(executionToDate) && executionFromDate > executionToDate) {
            $('#dateRangeValidationMessage').removeClass('d-none');
            isValid = false;
        }

        return isValid;
    }

    function buildLoadEntitiesUrl() {
        var query = $.param({
            auditDepartmentId: $('#auditDepartmentField').val(),
            executionStartDate: $('#executionFromDateField').val(),
            executionEndDate: $('#executionToDateField').val()
        });

        return g_asiBaseURL + '/OutstandingParasPdf/LoadEntities?' + query;
    }

    function buildZipUrl() {
        return g_asiBaseURL + '/OutstandingParasPdf/GenerateSelectedZip';
    }

    function buildSummaryPdfUrl() {
        var query = $.param({
            auditDepartmentId: $('#summaryAuditDepartmentField').val() || 0,
            risk: $('#summaryRiskField').val() || 'All'
        });

        return g_asiBaseURL + '/OutstandingParasPdf/GenerateSummaryPdf?' + query;
    }

    function getTableExportColumns(idx) {
        return idx > 0;
    }

    function getTableExportOptions() {
        var safeOptions = typeof getSafeExportFormatOptions === 'function'
            ? getSafeExportFormatOptions()
            : {};

        safeOptions.columns = getTableExportColumns;
        return safeOptions;
    }

    function destroyOutstandingParasTable() {
        if ($.fn.DataTable.isDataTable('#outstandingParasEntitiesTable')) {
            $('#outstandingParasEntitiesTable').DataTable().clear().destroy();
        }
    }

    function initializeOutstandingParasTable() {
        var exportOptions = getTableExportOptions();
        var pdfButton = typeof getPdfExportButtonConfig === 'function'
            ? getPdfExportButtonConfig()
            : { extend: 'pdfHtml5', orientation: 'landscape', pageSize: 'A4' };

        pdfButton.text = 'Export PDF';
        pdfButton.className = 'btn btn-danger';
        pdfButton.title = 'Outstanding Audit Paras';
        pdfButton.exportOptions = exportOptions;

        $('#outstandingParasEntitiesTable').DataTable({
            dom: '<"top d-flex flex-wrap gap-2 mb-2"B>rt<"bottom"i><"clear">',
            paging: false,
            searching: false,
            ordering: false,
            info: false,
            buttons: [
                pdfButton,
                {
                    extend: 'excelHtml5',
                    text: 'Export Excel',
                    className: 'btn btn-success',
                    title: 'Outstanding Audit Paras',
                    exportOptions: exportOptions
                },
                {
                    extend: 'csvHtml5',
                    text: 'Export CSV',
                    className: 'btn btn-primary',
                    title: 'Outstanding Audit Paras',
                    exportOptions: exportOptions
                }
            ]
        });
    }

    function encodeText(value) {
        return $('<div/>').text(value === null || value === undefined || value === '' ? '-' : value).html();
    }

    function setEmptyTableMessage(message) {
        destroyOutstandingParasTable();
        $('#outstandingParasEntitiesTable tbody').html(
            '<tr><td colspan="10" class="text-center text-muted">' + encodeText(message) + '</td></tr>'
        );
    }

    function renderEntities(entities) {
        destroyOutstandingParasTable();
        var $tbody = $('#outstandingParasEntitiesTable tbody');
        loadedEntities = entities || [];
        $('#selectAllOutstandingParasEntities').prop('checked', false);

        if (loadedEntities.length === 0) {
            setEmptyTableMessage('No engagements/entities found for the selected filters.');
            updateExportButtonState();
            return;
        }

        var rows = loadedEntities.map(function (item, index) {
            return '<tr>' +
                '<td class="text-center"><input type="checkbox" class="outstanding-entity-checkbox" data-index="' + index + '" /></td>' +
                '<td>' + encodeText(item.entityName) + '</td>' +
                '<td>' + encodeText(item.entityCode) + '</td>' +
                '<td>' + encodeText(item.auditDepartment) + '</td>' +
                '<td>' + encodeText(item.auditPeriod) + '</td>' +
                '<td>' + encodeText(item.executionStartDate) + '</td>' +
                '<td>' + encodeText(item.executionEndDate) + '</td>' +
                '<td>' + encodeText(item.teamLead) + '</td>' +
                '<td>' + encodeText(item.teamMembers) + '</td>' +
                '<td class="text-end">' + encodeText(item.outstandingParasCount) + '</td>' +
                '</tr>';
        });

        $tbody.html(rows.join(''));
        initializeOutstandingParasTable();
        updateExportButtonState();
    }

    function getSelectedEntities() {
        var selected = [];
        $('.outstanding-entity-checkbox:checked').each(function () {
            var index = parseInt($(this).attr('data-index'), 10);
            if (!isNaN(index) && loadedEntities[index]) {
                selected.push(loadedEntities[index]);
            }
        });

        return selected;
    }

    function updateExportButtonState() {
        var selectedCount = getSelectedEntities().length;
        $('#exportSelectedOutstandingParasPdfsButton').prop('disabled', selectedCount === 0);

        var totalCount = $('.outstanding-entity-checkbox').length;
        $('#selectAllOutstandingParasEntities').prop('checked', totalCount > 0 && selectedCount === totalCount);
    }

    function getZipFileName(response) {
        var disposition = response.headers.get('content-disposition') || '';
        var encodedMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (encodedMatch && encodedMatch[1]) {
            return decodeURIComponent(encodedMatch[1].replace(/"/g, ''));
        }

        var match = disposition.match(/filename="?([^"]+)"?/i);
        if (match && match[1]) {
            return match[1].replace(/;$/, '');
        }

        var now = new Date();
        var stamp = now.getFullYear().toString() +
            String(now.getMonth() + 1).padStart(2, '0') +
            String(now.getDate()).padStart(2, '0') + '_' +
            String(now.getHours()).padStart(2, '0') +
            String(now.getMinutes()).padStart(2, '0') +
            String(now.getSeconds()).padStart(2, '0');
        return 'Outstanding_Audit_Reports_' + stamp + '.zip';
    }

    function downloadBlob(blob, fileName) {
        var url = window.URL.createObjectURL(blob);
        var link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.setTimeout(function () {
            window.URL.revokeObjectURL(url);
        }, 1000);
    }

    function getPdfFileName(response) {
        var disposition = response.headers.get('content-disposition') || '';
        var encodedMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (encodedMatch && encodedMatch[1]) {
            return decodeURIComponent(encodedMatch[1].replace(/"/g, ''));
        }

        var match = disposition.match(/filename="?([^"]+)"?/i);
        if (match && match[1]) {
            return match[1].replace(/;$/, '');
        }

        return 'Consolidated_Outstanding_Audit_Paras_Summary.pdf';
    }

    async function generateSummaryPdf() {
        $('#generateOutstandingParasSummaryPdfButton').prop('disabled', true);
        showMessage('Generating consolidated summary PDF. This may take a few minutes.', 'info');

        try {
            var response = await fetch(buildSummaryPdfUrl(), {
                method: 'GET'
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to generate consolidated summary PDF. Please try again.');
            }

            var blob = await response.blob();
            downloadBlob(blob, getPdfFileName(response));
            showMessage('Consolidated summary PDF generated successfully.', 'success');
        } catch (error) {
            showMessage(error.message || 'Unable to generate consolidated summary PDF. Please try again.', 'danger');
        }

        $('#generateOutstandingParasSummaryPdfButton').prop('disabled', false);
    }

    async function exportSelectedPdfs() {
        var selected = getSelectedEntities();
        if (selected.length === 0) {
            showMessage('Please select at least one engagement/entity before export.', 'warning');
            return;
        }

        $('#loadOutstandingParasEntitiesButton, #exportSelectedOutstandingParasPdfsButton').prop('disabled', true);
        showMessage('Preparing ZIP export for ' + selected.length + ' selected engagement(s). This may take a few minutes.', 'info');

        try {
            var response = await fetch(buildZipUrl(), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    engagementIds: selected.map(function (item) {
                        return item.engagementId;
                    })
                })
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to generate ZIP export. Please try again.');
            }

            var blob = await response.blob();
            downloadBlob(blob, getZipFileName(response));
            showMessage('ZIP export completed for ' + selected.length + ' selected engagement(s).', 'success');
        } catch (error) {
            showMessage(error.message || 'Unable to generate ZIP export. Please try again.', 'danger');
        }

        $('#loadOutstandingParasEntitiesButton').prop('disabled', false);
        updateExportButtonState();
    }

    function loadEntities() {
        if (!validateFilters()) {
            return;
        }

        hideMessage();
        setEmptyTableMessage('Loading engagements/entities...');
        $('#loadOutstandingParasEntitiesButton').prop('disabled', true);
        $('#exportSelectedOutstandingParasPdfsButton').prop('disabled', true);

        $.ajax({
            url: buildLoadEntitiesUrl(),
            method: 'GET',
            dataType: 'json'
        }).done(function (response) {
            renderEntities(response && response.data ? response.data : []);
            showMessage('Loaded ' + loadedEntities.length + ' engagement/entity row(s).', loadedEntities.length > 0 ? 'success' : 'warning');
        }).fail(function (xhr) {
            loadedEntities = [];
            setEmptyTableMessage('Unable to load engagements/entities.');
            showMessage(xhr.responseText || 'Unable to load engagements/entities. Please try again.', 'danger');
        }).always(function () {
            $('#loadOutstandingParasEntitiesButton').prop('disabled', false);
            updateExportButtonState();
        });
    }

    $(document).ready(function () {
        $('#auditDepartmentField').select2();
        $('#summaryAuditDepartmentField').select2();
        $('#summaryRiskField').select2();

        $('#auditDepartmentField, #executionFromDateField, #executionToDateField').on('change blur', function () {
            validateFilters();
            loadedEntities = [];
            setEmptyTableMessage('Select filters and click Search / Load.');
            $('#selectAllOutstandingParasEntities').prop('checked', false);
            updateExportButtonState();
            hideMessage();
        });

        $('#loadOutstandingParasEntitiesButton').on('click', loadEntities);

        $('#selectAllOutstandingParasEntities').on('change', function () {
            $('.outstanding-entity-checkbox').prop('checked', $(this).is(':checked'));
            updateExportButtonState();
        });

        $('#outstandingParasEntitiesTable').on('change', '.outstanding-entity-checkbox', updateExportButtonState);

        $('#exportSelectedOutstandingParasPdfsButton').on('click', function () {
            exportSelectedPdfs();
        });

        $('#generateOutstandingParasSummaryPdfButton').on('click', function () {
            generateSummaryPdf();
        });
    });
})();

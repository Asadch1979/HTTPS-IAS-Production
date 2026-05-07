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

    function hideSummaryDownloadLinks() {
        $('#summaryPdfDownloadLinks').addClass('d-none').html('');
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

    function buildDeleteSummaryBatchUrl() {
        return g_asiBaseURL + '/OutstandingParasPdf/DeleteSummaryBatch';
    }

    function buildStoredSummaryPdfsUrl() {
        return g_asiBaseURL + '/OutstandingParasPdf/ListSummaryPdfs';
    }

    function buildDeleteSummaryPdfUrl() {
        return g_asiBaseURL + '/OutstandingParasPdf/DeleteSummaryPdf';
    }

    function buildStoredSummaryPdfsZipUrl() {
        return g_asiBaseURL + '/OutstandingParasPdf/ExportSelectedSummaryPdfsZip';
    }

    function buildSummaryPdfDownloadUrl(pdfId) {
        return g_asiBaseURL + '/OutstandingParasPdf/DownloadSummaryPdf?pdfId=' + encodeURIComponent(pdfId);
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

    function destroyStoredSummaryPdfsTable() {
        if ($.fn.DataTable.isDataTable('#storedSummaryPdfsTable')) {
            $('#storedSummaryPdfsTable').DataTable().clear().destroy();
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

    function setStoredSummaryPdfsMessage(message) {
        destroyStoredSummaryPdfsTable();
        $('#selectAllStoredSummaryPdfs').prop('checked', false);
        updateStoredSummaryPdfSelectionState();
        $('#storedSummaryPdfsTable tbody').html(
            '<tr><td colspan="12" class="text-center text-muted">' + encodeText(message) + '</td></tr>'
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

    function buildDownloadUrl(url) {
        if (!url) {
            return '#';
        }

        if (/^https?:\/\//i.test(url)) {
            return url;
        }

        return (g_asiBaseURL || '') + url;
    }

    function formatBytes(sizeBytes) {
        var value = parseInt(sizeBytes, 10);
        if (isNaN(value) || value <= 0) {
            return '';
        }

        if (value < 1024) {
            return value + ' B';
        }

        if (value < 1024 * 1024) {
            return (value / 1024).toFixed(1) + ' KB';
        }

        return (value / (1024 * 1024)).toFixed(1) + ' MB';
    }

    function renderStoredSummaryPdfs(records) {
        destroyStoredSummaryPdfsTable();
        $('#selectAllStoredSummaryPdfs').prop('checked', false);
        var rows = records || [];
        if (rows.length === 0) {
            setStoredSummaryPdfsMessage('No previously generated CIA summary PDFs found.');
            return;
        }

        var html = rows.map(function (item) {
            var pdfId = parseInt(item.pdfId, 10);
            var canDownload = pdfId > 0 && String(item.status || '').toUpperCase() === 'GENERATED';
            var downloadUrl = canDownload ? buildSummaryPdfDownloadUrl(pdfId) : '#';
            return '<tr data-pdf-id="' + encodeText(pdfId) + '">' +
                '<td class="text-center"><input type="checkbox" class="stored-summary-pdf-checkbox" data-pdf-id="' + encodeText(pdfId) + '"' + (canDownload ? '' : ' disabled') + ' /></td>' +
                '<td>' + encodeText(item.batchId) + '</td>' +
                '<td>' + encodeText(item.auditDepartmentName) + '</td>' +
                '<td>' + encodeText(item.entityName) + '</td>' +
                '<td>' + encodeText(item.risk) + '</td>' +
                '<td class="text-end">' + encodeText(item.partNo) + '</td>' +
                '<td>' + encodeText(item.fileName) + '</td>' +
                '<td class="text-end">' + encodeText(formatBytes(item.fileSize)) + '</td>' +
                '<td>' + encodeText(item.generatedOn) + '</td>' +
                '<td>' + encodeText(item.status) + '</td>' +
                '<td class="text-center">' +
                (canDownload ? '<a class="btn btn-sm btn-outline-danger" href="' + encodeText(downloadUrl) + '">Download</a>' : '<span class="text-muted">-</span>') +
                '</td>' +
                '<td class="text-center"><button type="button" class="btn btn-sm btn-outline-secondary delete-stored-summary-pdf-button" data-pdf-id="' + encodeText(pdfId) + '">Delete</button></td>' +
                '</tr>';
        }).join('');

        $('#storedSummaryPdfsTable tbody').html(html);
        $('#storedSummaryPdfsTable').DataTable({
            dom: 'rt<"bottom"ip><"clear">',
            pageLength: 10,
            ordering: false,
            searching: false,
            lengthChange: false
        });
        updateStoredSummaryPdfSelectionState();
    }

    function getSelectedStoredSummaryPdfIds() {
        var selected = [];
        var $checkboxes = getStoredSummaryPdfCheckboxes().filter(':checked');
        $checkboxes.each(function () {
            var pdfId = parseInt($(this).attr('data-pdf-id'), 10);
            if (!isNaN(pdfId) && pdfId > 0) {
                selected.push(pdfId);
            }
        });

        return selected;
    }

    function updateStoredSummaryPdfSelectionState() {
        var $checkboxes = getStoredSummaryPdfCheckboxes().filter(':not(:disabled)');
        var selectedCount = $checkboxes.filter(':checked').length;
        $('#exportSelectedStoredSummaryPdfsButton').prop('disabled', selectedCount === 0);
        $('#selectAllStoredSummaryPdfs').prop('checked', $checkboxes.length > 0 && selectedCount === $checkboxes.length);
    }

    function getStoredSummaryPdfCheckboxes() {
        if ($.fn.DataTable.isDataTable('#storedSummaryPdfsTable')) {
            return $($('#storedSummaryPdfsTable').DataTable().rows().nodes()).find('.stored-summary-pdf-checkbox');
        }

        return $('.stored-summary-pdf-checkbox');
    }

    async function loadStoredSummaryPdfs() {
        setStoredSummaryPdfsMessage('Loading previously generated PDFs...');
        $('#refreshStoredSummaryPdfsButton').prop('disabled', true).text('Refreshing...');

        try {
            var response = await fetch(buildStoredSummaryPdfsUrl(), {
                method: 'GET'
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to load previously generated PDFs.');
            }

            var result = await response.json();
            renderStoredSummaryPdfs(result && result.data ? result.data : []);
        } catch (error) {
            setStoredSummaryPdfsMessage(error.message || 'Unable to load previously generated PDFs.');
        }

        $('#refreshStoredSummaryPdfsButton').prop('disabled', false).text('Refresh');
    }

    function renderSummaryDownloadLinks(response) {
        var files = response && response.files ? response.files : [];
        var $container = $('#summaryPdfDownloadLinks');
        $container.removeClass('d-none');

        if (files.length === 0) {
            $container.prepend('<div class="alert alert-warning mb-2">No files were generated for Batch ID: ' + encodeText(response && response.batchId ? response.batchId : '-') + '</div>');
            return;
        }

        var items = files.map(function (file, index) {
            var badgeClass = file.isFailureFile ? 'badge badge-warning' : 'badge badge-success';
            var badgeText = file.isFailureFile ? 'Failed set' : 'PDF';
            var size = formatBytes(file.sizeBytes);
            var href = file.pdfId ? buildSummaryPdfDownloadUrl(file.pdfId) : buildDownloadUrl(file.url);
            return '<li class="list-group-item d-flex justify-content-between align-items-center flex-wrap gap-2">' +
                '<div class="d-flex align-items-center flex-wrap gap-2">' +
                '<input type="checkbox" class="summary-download-checkbox me-2" data-index="' + index + '" checked />' +
                '<span class="' + badgeClass + ' me-2">' + badgeText + '</span>' +
                '<a class="summary-download-link" href="' + encodeText(href) + '" target="_blank" rel="noopener" download>' + encodeText(file.fileName) + '</a>' +
                (size ? '<span class="text-muted small ms-2">' + encodeText(size) + '</span>' : '') +
                '</div>' +
                '</li>';
        });

        var batchHtml =
            '<div class="summary-batch-card border rounded p-2 mb-3" data-batch-id="' + encodeText(response.batchId) + '">' +
                '<div class="alert alert-success mb-2 d-flex justify-content-between align-items-center flex-wrap gap-2">' +
                '<span>Generated ' + encodeText(response.successCount || 0) + ' PDF file(s)' +
                (response.failureCount ? ' with ' + encodeText(response.failureCount) + ' failed set(s)' : '') +
                '. Batch ID: ' + encodeText(response.batchId) + '</span>' +
                '</div>' +
                '<div class="d-flex align-items-center flex-wrap gap-2 mb-2">' +
                '<label class="mb-0"><input type="checkbox" class="select-all-summary-download-links" checked /> Select all</label>' +
                '<button type="button" class="btn btn-sm btn-danger download-selected-summary-links-button">Download Selected</button>' +
                '</div>' +
                '<ul class="list-group">' + items.join('') + '</ul>' +
            '</div>';

        $container.prepend(batchHtml);
    }

    function updateSummaryDownloadSelectionState($batch) {
        var $targetBatch = $batch && $batch.length ? $batch : $('#summaryPdfDownloadLinks');
        var $checkboxes = $targetBatch.find('.summary-download-checkbox');
        var selectedCount = $checkboxes.filter(':checked').length;
        $targetBatch.find('.select-all-summary-download-links').prop('checked', $checkboxes.length > 0 && selectedCount === $checkboxes.length);
        $targetBatch.find('.download-selected-summary-links-button').prop('disabled', selectedCount === 0);
    }

    function downloadSelectedSummaryLinks($batch) {
        $batch.find('.summary-download-checkbox:checked').each(function () {
            var $link = $(this).closest('li').find('.summary-download-link').first();
            if ($link.length > 0 && $link.attr('href')) {
                var link = document.createElement('a');
                link.href = $link.attr('href');
                link.download = $link.text();
                link.target = '_blank';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        });
    }

    async function removeSummaryBatch($batch) {
        var batchId = $batch.attr('data-batch-id');
        if (!batchId) {
            $batch.remove();
            return;
        }

        var $button = $batch.find('.remove-summary-batch-button').first();
        $button.prop('disabled', true).text('Removing...');

        try {
            var response = await fetch(buildDeleteSummaryBatchUrl(), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ batchId: batchId })
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to remove the generated batch.');
            }

            $batch.remove();
            if ($('#summaryPdfDownloadLinks').children().length === 0) {
                hideSummaryDownloadLinks();
            }
            showMessage('Generated PDF batch removed.', 'success');
        } catch (error) {
            $button.prop('disabled', false).text('Remove');
            showMessage(error.message || 'Unable to remove the generated batch.', 'danger');
        }
    }

    async function generateSummaryPdf() {
        $('#generateOutstandingParasSummaryPdfButton').prop('disabled', true);
        $('#summaryPdfDownloadLinks').removeClass('d-none');
        showMessage('Generating consolidated summary PDF files. Existing generated batches remain available below.', 'info');

        try {
            var response = await fetch(buildSummaryPdfUrl(), {
                method: 'GET'
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to generate consolidated summary PDF. Please try again.');
            }

            var result = await response.json();
            renderSummaryDownloadLinks(result);
            loadStoredSummaryPdfs();
            showMessage('Consolidated summary PDF files generated successfully.', 'success');
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

    async function deleteStoredSummaryPdf(pdfId, $button) {
        if (!pdfId || pdfId <= 0) {
            showMessage('A valid PDF ID is required.', 'warning');
            return;
        }

        $button.prop('disabled', true).text('Deleting...');
        try {
            var response = await fetch(buildDeleteSummaryPdfUrl(), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ pdfId: pdfId })
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to delete the selected PDF.');
            }

            showMessage('Selected CIA summary PDF deleted.', 'success');
            loadStoredSummaryPdfs();
        } catch (error) {
            $button.prop('disabled', false).text('Delete');
            showMessage(error.message || 'Unable to delete the selected PDF.', 'danger');
        }
    }

    async function exportSelectedStoredSummaryPdfsZip() {
        var selectedPdfIds = getSelectedStoredSummaryPdfIds();
        if (selectedPdfIds.length === 0) {
            showMessage('Please select at least one CIA summary PDF before export.', 'warning');
            return;
        }

        $('#exportSelectedStoredSummaryPdfsButton').prop('disabled', true).text('Preparing ZIP...');
        showMessage('Preparing ZIP export for ' + selectedPdfIds.length + ' selected CIA summary PDF(s).', 'info');

        try {
            var response = await fetch(buildStoredSummaryPdfsZipUrl(), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ pdfIds: selectedPdfIds })
            });

            if (!response.ok) {
                var errorText = await response.text();
                throw new Error(errorText || 'Unable to generate selected CIA summary PDFs ZIP.');
            }

            var blob = await response.blob();
            downloadBlob(blob, getStoredSummaryZipFileName(response));
            showMessage('ZIP export completed for ' + selectedPdfIds.length + ' selected CIA summary PDF(s).', 'success');
        } catch (error) {
            showMessage(error.message || 'Unable to generate selected CIA summary PDFs ZIP.', 'danger');
        }

        $('#exportSelectedStoredSummaryPdfsButton').text('Export Selected ZIP');
        updateStoredSummaryPdfSelectionState();
    }

    function getStoredSummaryZipFileName(response) {
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
        return 'CIA_Summary_PDFs_' + stamp + '.zip';
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

        $('#summaryAuditDepartmentField, #summaryRiskField').on('change', function () {
            hideMessage();
        });

        $('#refreshStoredSummaryPdfsButton').on('click', function () {
            loadStoredSummaryPdfs();
        });

        $('#selectAllStoredSummaryPdfs').on('change', function () {
            getStoredSummaryPdfCheckboxes().filter(':not(:disabled)').prop('checked', $(this).is(':checked'));
            updateStoredSummaryPdfSelectionState();
        });

        $('#storedSummaryPdfsTable').on('change', '.stored-summary-pdf-checkbox', updateStoredSummaryPdfSelectionState);

        $('#exportSelectedStoredSummaryPdfsButton').on('click', function () {
            exportSelectedStoredSummaryPdfsZip();
        });

        $('#storedSummaryPdfsTable').on('click', '.delete-stored-summary-pdf-button', function () {
            var $button = $(this);
            var pdfId = parseInt($button.attr('data-pdf-id'), 10);
            deleteStoredSummaryPdf(pdfId, $button);
        });

        $('#summaryPdfDownloadLinks').on('change', '.select-all-summary-download-links', function () {
            var $batch = $(this).closest('.summary-batch-card');
            $batch.find('.summary-download-checkbox').prop('checked', $(this).is(':checked'));
            updateSummaryDownloadSelectionState($batch);
        });

        $('#summaryPdfDownloadLinks').on('change', '.summary-download-checkbox', function () {
            updateSummaryDownloadSelectionState($(this).closest('.summary-batch-card'));
        });

        $('#summaryPdfDownloadLinks').on('click', '.download-selected-summary-links-button', function () {
            downloadSelectedSummaryLinks($(this).closest('.summary-batch-card'));
        });

        $('#summaryPdfDownloadLinks').on('click', '.remove-summary-batch-button', function () {
            removeSummaryBatch($(this).closest('.summary-batch-card'));
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

        loadStoredSummaryPdfs();
    });

    window.addEventListener('pageshow', function (event) {
        if (event.persisted) {
            loadStoredSummaryPdfs();
        }
    });
})();

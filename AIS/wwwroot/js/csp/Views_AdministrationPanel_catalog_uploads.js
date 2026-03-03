        var catalogTokens = {};

        function validateCatalog(type) {
            var fileInput = type === 'api' ? $('#apiCatalogFile')[0] : $('#pageCatalogFile')[0];
            if (!fileInput.files || fileInput.files.length === 0) {
                alert("Select a file to validate.");
                return;
            }

            var formData = new FormData();
            formData.append('file', fileInput.files[0]);
            formData.append('catalogType', type);

            $.ajax({
                url: g_asiBaseURL + "/Administration/Catalog/Validate",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (!response || !response.success) {
                        showApiAlert(response, "Unable to validate catalog.");
                        return;
                    }

                    var preview = response.preview;
                    catalogTokens[type] = preview.token;
                    renderCatalogPreview(type, preview);
                    toggleApplyButton(type, true);
                },
                error: function () {
                    alert("Unable to validate catalog.");
                }
            });
        }

        function applyCatalog(type) {
            if (!catalogTokens[type]) {
                alert("Validate the catalog before applying.");
                return;
            }

            var formData = new FormData();
            formData.append('catalogType', type);
            formData.append('token', catalogTokens[type]);

            $.ajax({
                url: g_asiBaseURL + "/Administration/Catalog/Apply",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (!response || !response.success) {
                        showApiAlert(response, "Unable to apply catalog.");
                        return;
                    }

                    alert("Catalog sync completed.");
                    toggleApplyButton(type, false);
                },
                error: function () {
                    alert("Unable to apply catalog.");
                }
            });
        }

        function toggleApplyButton(type, enabled) {
            var button = type === 'api' ? $('#apiCatalogApplyBtn') : $('#pageCatalogApplyBtn');
            button.prop('disabled', !enabled);
        }

        function renderCatalogPreview(type, preview) {
            var summaryTarget = type === 'api' ? $('#apiCatalogSummary') : $('#pageCatalogSummary');
            var tableBody = type === 'api' ? $('#apiCatalogPreviewTable tbody') : $('#pageCatalogPreviewTable tbody');
            tableBody.empty();

            summaryTarget.html(
                '<span class="badge bg-success me-2">New: ' + preview.newCount + '</span>'
                + '<span class="badge bg-info text-dark me-2">Updated: ' + preview.updatedCount + '</span>'
                + '<span class="badge bg-secondary">Inactive: ' + preview.inactiveCount + '</span>'
            );

            if (type === 'api') {
                appendApiPreviewRows(tableBody, preview.newApiRecords, 'New');
                appendApiPreviewRows(tableBody, preview.updatedApiRecords, 'Update');
                appendApiPreviewRows(tableBody, preview.inactiveApiRecords, 'Inactive');
            } else {
                appendPagePreviewRows(tableBody, preview.newPageRecords, 'New');
                appendPagePreviewRows(tableBody, preview.updatedPageRecords, 'Update');
                appendPagePreviewRows(tableBody, preview.inactivePageRecords, 'Inactive');
            }

            if (tableBody.children().length === 0) {
                tableBody.append('<tr><td colspan="4" class="text-muted">No changes detected.</td></tr>');
            }
        }

        function appendApiPreviewRows(target, records, label) {
            if (!records || records.length === 0) {
                return;
            }

            $.each(records, function (i, item) {
                target.append('<tr>'
                    + '<td>' + label + '</td>'
                    + '<td>' + (item.apiName || '') + '</td>'
                    + '<td>' + (item.apiPath || '') + '</td>'
                    + '<td>' + (item.httpMethod || '') + '</td>'
                    + '</tr>');
            });
        }

        function appendPagePreviewRows(target, records, label) {
            if (!records || records.length === 0) {
                return;
            }

            $.each(records, function (i, item) {
                target.append('<tr>'
                    + '<td>' + label + '</td>'
                    + '<td>' + (item.pageId || '') + '</td>'
                    + '<td>' + (item.pageName || '') + '</td>'
                    + '<td>' + (item.pagePath || '') + '</td>'
                    + '</tr>');
            });
        }

        var apiMasterData = [];

        $(document).ready(function () {
            loadApiMasterList();
        });

        function loadApiMasterList() {
            $.ajax({
                url: g_asiBaseURL + "/Administration/ApiMaster/List",
                type: "GET",
                cache: false,
                success: function (response) {
                    if (!response || !response.success) {
                        showApiAlert(response, "Unable to load API master list.");
                        return;
                    }

                    apiMasterData = response.data || [];
                    renderApiMasterTable();
                },
                error: function () {
                    alert("Unable to load API master list.");
                }
            });
        }

        function renderApiMasterTable() {
            var tbody = $('#apiMasterTable tbody');
            tbody.empty();

            if (!apiMasterData || apiMasterData.length === 0) {
                tbody.append('<tr><td colspan="5" class="text-muted">No API definitions found.</td></tr>');
                return;
            }

            $.each(apiMasterData, function (i, item) {
                var isActiveRaw = item.isActive || '';
                var statusLabel = isActiveRaw.toUpperCase() === 'N' ? 'Disabled' : 'Enabled';
                var apiId = item.apiId;

                var row = '<tr>'
                    + '<td>' + (item.apiName || '') + '</td>'
                    + '<td>' + (item.apiPath || '') + '</td>'
                    + '<td>' + (item.httpMethod || '') + '</td>'
                    + '<td>' + statusLabel + '</td>'
                    + '<td>'
                    + '<button class="btn btn-sm btn-outline-primary me-2" type="button" data-onclick="openApiMasterModal(' + apiId + ');">Edit</button>'
                    + '<button class="btn btn-sm btn-outline-secondary" type="button" data-onclick="disableApiMaster(' + apiId + ');">Disable</button>'
                    + '</td>'
                    + '</tr>';

                tbody.append(row);
            });
        }

        function openApiMasterModal(apiId) {
            var modal = new bootstrap.Modal(document.getElementById('apiMasterModal'));
            var item = apiMasterData.find(function (entry) {
                return entry.apiId == apiId;
            });

            $('#apiMasterId').val(apiId || 0);
            $('#apiNameInput').val(item ? item.apiName : '');
            $('#apiPathInput').val(item ? item.apiPath : '');
            $('#apiMethodInput').val(item ? item.httpMethod : 'POST');
            $('#apiIsActiveInput').val(item ? (item.isActive || 'Y') : 'Y');

            modal.show();
        }

        function saveApiMaster() {
            var apiId = parseInt($('#apiMasterId').val() || 0);
            var payload = {
                apiId: apiId,
                apiName: $('#apiNameInput').val(),
                apiPath: $('#apiPathInput').val(),
                httpMethod: $('#apiMethodInput').val(),
                isActive: $('#apiIsActiveInput').val(),
                actionInd: apiId > 0 ? 'U' : 'A'
            };

            $.ajax({
                url: g_asiBaseURL + "/Administration/ApiMaster/Save",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                cache: false,
                success: function (response) {
                    if (!response || !response.success) {
                        showApiAlert(response, "Unable to save API master entry.");
                        return;
                    }

                    $('#apiMasterModal').modal('hide');
                    loadApiMasterList();
                },
                error: function () {
                    alert("Unable to save API master entry.");
                }
            });
        }

        function disableApiMaster(apiId) {
            var item = apiMasterData.find(function (entry) {
                return entry.apiId == apiId;
            });

            if (!item) {
                return;
            }

            var payload = {
                apiId: apiId,
                apiName: item.apiName,
                apiPath: item.apiPath,
                httpMethod: item.httpMethod,
                isActive: 'N',
                actionInd: 'D'
            };

            $.ajax({
                url: g_asiBaseURL + "/Administration/ApiMaster/Save",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                cache: false,
                success: function (response) {
                    if (!response || !response.success) {
                        showApiAlert(response, "Unable to disable API.");
                        return;
                    }

                    loadApiMasterList();
                },
                error: function () {
                    alert("Unable to disable API.");
                }
            });
        }

        var apiMasterData = [];

        $(document).ready(function () {
            loadApiMasterList();
        });

        $(document).on('input change', '#controllerNameInput', function () {
            clearApiMasterFieldValidation($(this), $('#controllerNameValidation'));
        });

        function normalizeApiMasterItem(item) {
            item = item || {};
            return {
                ApiId: item.ApiId || item.apiId || 0,
                ApiName: item.ApiName || item.apiName || '',
                ControllerName: item.ControllerName || item.controllerName || '',
                ApiPath: item.ApiPath || item.apiPath || '',
                HttpMethod: item.HttpMethod || item.httpMethod || 'POST',
                IsActive: item.IsActive || item.isActive || 'Y'
            };
        }

        function clearApiMasterFieldValidation($input, $message) {
            $input.removeClass('is-invalid');
            if ($message && $message.length) {
                $message.text('Controller Name is required.');
            }
        }

        function validateApiMasterForm() {
            var $controllerNameInput = $('#controllerNameInput');
            var $controllerNameValidation = $('#controllerNameValidation');
            var controllerName = $.trim($controllerNameInput.val());

            clearApiMasterFieldValidation($controllerNameInput, $controllerNameValidation);

            if (!controllerName) {
                $controllerNameInput.addClass('is-invalid');
                $controllerNameValidation.text('Controller Name is required.');
                return false;
            }

            return true;
        }

        $(document).on('click', '.js-api-master-add', function (event) {
            event.preventDefault();
            openApiMasterModal(0);
        });

        $(document).on('click', '.js-api-master-save', function (event) {
            event.preventDefault();
            saveApiMaster();
        });

        $(document).on('click', '.js-api-master-edit', function (event) {
            event.preventDefault();
            var apiId = $(this).data('apiId');
            openApiMasterModal(apiId);
        });

        $(document).on('click', '.js-api-master-disable', function (event) {
            event.preventDefault();
            var apiId = $(this).data('apiId');
            disableApiMaster(apiId);
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

                    apiMasterData = $.map(response.data || [], function (entry) {
                        return normalizeApiMasterItem(entry);
                    });
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
                var isActiveRaw = item.IsActive || '';
                var statusLabel = isActiveRaw.toUpperCase() === 'N' ? 'Disabled' : 'Enabled';
                var apiId = item.ApiId;

                var row = '<tr>'
                    + '<td>' + (item.ApiName || '') + '</td>'
                    + '<td>' + (item.ApiPath || '') + '</td>'
                    + '<td>' + (item.HttpMethod || '') + '</td>'
                    + '<td>' + statusLabel + '</td>'
                    + '<td>'
                    + '<button class="btn btn-sm btn-outline-primary me-2 js-api-master-edit" type="button" data-api-id="' + apiId + '">Edit</button>'
                    + '<button class="btn btn-sm btn-outline-secondary js-api-master-disable" type="button" data-api-id="' + apiId + '">Disable</button>'
                    + '</td>'
                    + '</tr>';

                tbody.append(row);
            });
        }

        function openApiMasterModal(apiId) {
            var modal = new bootstrap.Modal(document.getElementById('apiMasterModal'));
            var item = apiMasterData.find(function (entry) {
                return entry.ApiId == apiId;
            });

            $('#apiMasterId').val(apiId || 0);
            $('#apiNameInput').val(item ? item.ApiName : '');
            $('#controllerNameInput').val(item ? item.ControllerName : '');
            $('#apiPathInput').val(item ? item.ApiPath : '');
            $('#apiMethodInput').val(item ? item.HttpMethod : 'POST');
            $('#apiIsActiveInput').val(item ? (item.IsActive || 'Y') : 'Y');
            clearApiMasterFieldValidation($('#controllerNameInput'), $('#controllerNameValidation'));

            modal.show();
        }

        function saveApiMaster() {
            if (!validateApiMasterForm()) {
                return;
            }

            var apiId = parseInt($('#apiMasterId').val() || 0);
            var payload = {
                ApiId: apiId,
                ApiName: $.trim($('#apiNameInput').val()),
                ControllerName: $.trim($('#controllerNameInput').val()),
                ApiPath: $.trim($('#apiPathInput').val()),
                HttpMethod: $('#apiMethodInput').val(),
                IsActive: $('#apiIsActiveInput').val(),
                ActionInd: apiId > 0 ? 'U' : 'A'
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
                return entry.ApiId == apiId;
            });

            if (!item) {
                return;
            }

            var payload = {
                ApiId: apiId,
                ApiName: item.ApiName,
                ControllerName: item.ControllerName || '',
                ApiPath: item.ApiPath,
                HttpMethod: item.HttpMethod,
                IsActive: 'N',
                ActionInd: 'D'
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

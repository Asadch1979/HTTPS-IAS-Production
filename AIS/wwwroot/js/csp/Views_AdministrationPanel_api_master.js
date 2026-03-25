        var apiMasterData = [];
        var apiMasterControllers = [];
        var apiMasterPages = [];
        var editingPageId = 0;

        $(document).ready(function () {
            loadControllerOptions();
            loadAllPages();
            loadApiMasterList();
        });

        $(document).on('input change', '#controllerNameInput', function () {
            clearApiMasterFieldValidation($(this), $('#controllerNameValidation'), 'Controller Name is required.');
        });

        $(document).on('change', '#apiMenuInput', function () {
            clearApiMasterFieldValidation($('#apiPageIdInput'), $('#apiPageValidation'), 'Page is required.');
            populateSubMenuDropdown(parseInt($(this).val() || 0, 10), 0);
            populatePageDropdown(parseInt($(this).val() || 0, 10), 0, 0);
        });

        $(document).on('change', '#apiSubMenuInput', function () {
            clearApiMasterFieldValidation($('#apiPageIdInput'), $('#apiPageValidation'), 'Page is required.');
            var menuId = parseInt($('#apiMenuInput').val() || 0, 10);
            var subMenuId = parseInt($(this).val() || 0, 10);
            populatePageDropdown(menuId, subMenuId, 0);
        });

        $(document).on('change', '#apiPageIdInput', function () {
            clearApiMasterFieldValidation($(this), $('#apiPageValidation'), 'Page is required.');
        });

        function normalizeApiMasterItem(item) {
            item = item || {};
            return {
                ApiId: item.ApiId || item.apiId || 0,
                ApiName: item.ApiName || item.apiName || '',
                ControllerName: item.ControllerName || item.controllerName || '',
                ApiPath: item.ApiPath || item.apiPath || '',
                HttpMethod: item.HttpMethod || item.httpMethod || 'POST',
                IsActive: item.IsActive || item.isActive || 'Y',
                PageId: parseInt(item.PageId || item.pageId || 0, 10) || 0
            };
        }

        function clearApiMasterFieldValidation($input, $message, defaultMessage) {
            $input.removeClass('is-invalid');
            if ($message && $message.length && defaultMessage) {
                $message.text(defaultMessage);
            }
        }

        function validateApiMasterForm() {
            var isValid = true;
            var $controllerNameInput = $('#controllerNameInput');
            var $controllerNameValidation = $('#controllerNameValidation');
            var controllerName = $.trim($controllerNameInput.val());

            var $pageInput = $('#apiPageIdInput');
            var $pageValidation = $('#apiPageValidation');
            var pageId = parseInt($pageInput.val() || 0, 10);

            clearApiMasterFieldValidation($controllerNameInput, $controllerNameValidation, 'Controller Name is required.');
            clearApiMasterFieldValidation($pageInput, $pageValidation, 'Page is required.');

            if (!controllerName) {
                $controllerNameInput.addClass('is-invalid');
                $controllerNameValidation.text('Controller Name is required.');
                isValid = false;
            }

            if (!pageId) {
                $pageInput.addClass('is-invalid');
                $pageValidation.text('Page is required.');
                isValid = false;
            }

            return isValid;
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

        function loadControllerOptions() {
            $.ajax({
                url: g_asiBaseURL + "/Administration/ApiMaster/ControllerOptions",
                type: "GET",
                cache: false,
                success: function (response) {
                    if (!response || !response.success) {
                        return;
                    }

                    apiMasterControllers = $.map(response.data || [], function (entry) {
                        return $.trim(entry || '');
                    }).filter(function (entry, index, self) {
                        return entry && self.indexOf(entry) === index;
                    });

                    bindControllerDropdown('');
                }
            });
        }

        function bindControllerDropdown(selectedController) {
            var controller = $.trim(selectedController || '');
            var ddl = $('#controllerNameInput');
            ddl.empty();
            ddl.append('<option value="">-- Select Controller --</option>');

            $.each(apiMasterControllers, function (i, value) {
                ddl.append('<option value="' + safeAttr(value) + '">' + safeText(value) + '</option>');
            });

            ddl.val(controller);
        }

        function loadAllPages() {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_menu_pages_for_admin_panel",
                type: "POST",
                data: {
                    M_ID: 0,
                    SM_ID: 0
                },
                cache: false,
                success: function (data) {
                    apiMasterPages = $.map(data || [], function (entry) {
                        return {
                            PageId: parseInt(entry.p_ID || entry.P_ID || 0, 10) || 0,
                            MenuId: parseInt(entry.m_ID || entry.M_ID || 0, 10) || 0,
                            SubMenuId: parseInt(entry.sM_ID || entry.SM_ID || 0, 10) || 0,
                            PageName: entry.p_NAME || entry.P_NAME || '',
                            PagePath: entry.p_PATH || entry.P_PATH || ''
                        };
                    }).filter(function (entry) {
                        return entry.PageId > 0;
                    });
                }
            });
        }

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
                    + '<td>' + safeText(item.ApiName || '') + '</td>'
                    + '<td>' + safeText(item.ApiPath || '') + '</td>'
                    + '<td>' + safeText(item.HttpMethod || '') + '</td>'
                    + '<td>' + safeText(statusLabel) + '</td>'
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

            editingPageId = item ? (item.PageId || 0) : 0;

            $('#apiMasterId').val(apiId || 0);
            $('#apiNameInput').val(item ? item.ApiName : '');
            bindControllerDropdown(item ? item.ControllerName : '');
            $('#apiPathInput').val(item ? item.ApiPath : '');
            $('#apiMethodInput').val(item ? item.HttpMethod : 'POST');
            $('#apiIsActiveInput').val(item ? (item.IsActive || 'Y') : 'Y');

            resetPageSelection(item ? item.PageId : 0);
            clearApiMasterFieldValidation($('#controllerNameInput'), $('#controllerNameValidation'), 'Controller Name is required.');
            clearApiMasterFieldValidation($('#apiPageIdInput'), $('#apiPageValidation'), 'Page is required.');

            modal.show();
        }

        function resetPageSelection(selectedPageId) {
            var pageId = parseInt(selectedPageId || 0, 10);
            var pageDetail = findPageDetail(pageId);
            var menuId = pageDetail ? pageDetail.MenuId : 0;
            var subMenuId = pageDetail ? pageDetail.SubMenuId : 0;

            $('#apiMenuInput').val(menuId || 0);
            populateSubMenuDropdown(menuId, subMenuId);
            populatePageDropdown(menuId, subMenuId, pageId);
        }

        function findPageDetail(pageId) {
            if (!pageId) {
                return null;
            }

            for (var i = 0; i < apiMasterPages.length; i++) {
                if (apiMasterPages[i].PageId === pageId) {
                    return apiMasterPages[i];
                }
            }

            return null;
        }

        function populateSubMenuDropdown(menuId, selectedSubMenuId) {
            var ddl = $('#apiSubMenuInput');
            ddl.empty();
            ddl.append('<option value="0">-- Select Sub Menu --</option>');

            if (!menuId) {
                return;
            }

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_sub_menu_for_admin_panel",
                type: "POST",
                data: {
                    M_ID: menuId
                },
                cache: false,
                success: function (data) {
                    $.each(data || [], function (i, entry) {
                        var id = parseInt(entry.suB_MENU_ID || entry.SUB_MENU_ID || 0, 10) || 0;
                        var name = entry.suB_MENU_NAME || entry.SUB_MENU_NAME || '';
                        if (id > 0) {
                            ddl.append('<option value="' + id + '">' + safeText(name) + '</option>');
                        }
                    });

                    ddl.val(selectedSubMenuId || 0);
                }
            });
        }

        function populatePageDropdown(menuId, subMenuId, selectedPageId) {
            var ddl = $('#apiPageIdInput');
            ddl.empty();
            ddl.append('<option value="">-- Select Page --</option>');

            if (!menuId) {
                return;
            }

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_menu_pages_for_admin_panel",
                type: "POST",
                data: {
                    M_ID: menuId,
                    SM_ID: subMenuId || 0
                },
                cache: false,
                success: function (data) {
                    $.each(data || [], function (i, entry) {
                        var pageId = parseInt(entry.p_ID || entry.P_ID || 0, 10) || 0;
                        var pageName = entry.p_NAME || entry.P_NAME || '';
                        var pagePath = entry.p_PATH || entry.P_PATH || '';

                        if (pageId > 0) {
                            var label = pageName;
                            if (pagePath) {
                                label += ' (' + pagePath + ')';
                            }

                            ddl.append('<option value="' + pageId + '">' + safeText(label) + '</option>');
                        }
                    });

                    ddl.val(selectedPageId || '');
                }
            });
        }

        function saveApiMaster() {
            if (!validateApiMasterForm()) {
                return;
            }

            var apiId = parseInt($('#apiMasterId').val() || 0, 10);
            var payload = {
                ApiId: apiId,
                ApiName: $.trim($('#apiNameInput').val()),
                ControllerName: $.trim($('#controllerNameInput').val()),
                ApiPath: $.trim($('#apiPathInput').val()),
                HttpMethod: $('#apiMethodInput').val(),
                IsActive: $('#apiIsActiveInput').val(),
                PageId: parseInt($('#apiPageIdInput').val() || 0, 10),
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
                PageId: item.PageId || 0,
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

        function safeText(value) {
            return $('<div/>').text(value || '').html();
        }

        function safeAttr(value) {
            return safeText(value).replace(/"/g, '&quot;');
        }

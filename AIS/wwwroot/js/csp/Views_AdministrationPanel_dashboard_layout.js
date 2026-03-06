    var dashboardLayoutData = [];
    var availableDashboardPages = [];
    var availableDashboardPagesAll = [];

    function loadDashboardLayout() {
        var roleId = $('#dashboardRoleSelect').val();
        if (!roleId || roleId === '0') {
            $('#dashboardLayoutSection').addClass('d-none');
            $('#dashboardRolePrompt').removeClass('d-none');
            resetDashboardPageFilters();
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/Administration/DashboardLayout/GetByRole",
            type: "GET",
            data: { roleId: roleId },
            cache: false,
            success: function (response) {
                if (!response || !response.success) {
                    showApiAlert(response, "Unable to load dashboard layout.");
                    return;
                }

                dashboardLayoutData = response.data || [];
                renderDashboardLayoutTable();
                loadAvailableDashboardPages(roleId);
                $('#dashboardLayoutSection').removeClass('d-none');
                $('#dashboardRolePrompt').addClass('d-none');
            },
            error: function () {
                alert("Unable to load dashboard layout.");
            }
        });
    }

    function loadAvailableDashboardPages(roleId) {
        $.ajax({
            url: g_asiBaseURL + "/Administration/DashboardLayout/GetAvailablePages",
            type: "GET",
            data: { roleId: roleId },
            cache: false,
            success: function (response) {
                if (!response || !response.success) {
                    showApiAlert(response, "Unable to load available pages.");
                    return;
                }

                availableDashboardPagesAll = response.data || [];
                availableDashboardPages = availableDashboardPagesAll;
                populateDashboardPageSelect();
                setDefaultDashboardOrder();
            },
            error: function () {
                alert("Unable to load available pages.");
            }
        });
    }

    function getDashboardSubMenus() {
        var menuId = $('#dashboardMenuDropDownField').val();
        $('#dashboardSubMenuDropDownField').empty();
        $('#dashboardSubMenuDropDownField').append('<option value="0">--All--</option>');

        if (!menuId || menuId === '0') {
            availableDashboardPages = availableDashboardPagesAll;
            populateDashboardPageSelect();
            return;
        }

        // TODO: Replace ApiCalls endpoint with a consolidated dashboard layout API.
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_sub_menu_for_admin_panel",
            type: "POST",
            data: { "M_ID": menuId },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#dashboardSubMenuDropDownField').append('<option value="' + v.suB_MENU_ID + '">' + v.suB_MENU_NAME + '</option>');
                });
            },
            dataType: "json"
        });
    }

    function loadDashboardMenuPages() {
        var roleId = $('#dashboardRoleSelect').val();
        var menuId = $('#dashboardMenuDropDownField').val();
        var subMenuId = $('#dashboardSubMenuDropDownField').val();

        if (!roleId || roleId === '0') {
            alert("Select a role to continue.");
            return;
        }

        if (!menuId || menuId === '0') {
            alert("Select a menu to continue.");
            return;
        }

        // TODO: Replace ApiCalls endpoint with a consolidated dashboard layout API.
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_menu_pages_for_admin_panel",
            type: "POST",
            data: {
                "M_ID": menuId,
                "SM_ID": subMenuId
            },
            cache: false,
            success: function (data) {
                var configuredPages = dashboardLayoutData.map(function (item) { return item.pageId; });
                availableDashboardPages = $.map(data || [], function (item) {
                    if ($.inArray(item.p_ID, configuredPages) !== -1) {
                        return null;
                    }
                    return {
                        pageId: item.p_ID,
                        pageName: item.p_NAME
                    };
                });

                populateDashboardPageSelect();
                setDefaultDashboardOrder();
            },
            dataType: "json"
        });
    }

    function populateDashboardPageSelect() {
        var select = $('#dashboardPageSelect');
        select.empty();
        select.append('<option value="0">-- Select Page --</option>');

        if (!availableDashboardPages || availableDashboardPages.length === 0) {
            select.append('<option value="0" disabled>No available pages</option>');
            return;
        }

        $.each(availableDashboardPages, function (i, item) {
            select.append('<option value="' + item.pageId + '">' + item.pageName + '</option>');
        });
    }

    function normalizeDashboardOrder(order, pageId) {
        var normalizedOrder = parseInt(order || 1);
        if (isNaN(normalizedOrder) || normalizedOrder < 1) {
            normalizedOrder = 1;
        }

        var existingOrders = dashboardLayoutData
            .filter(function (item) { return item.pageId !== pageId; })
            .map(function (item) { return parseInt(item.dashboardOrder || 0); });

        while ($.inArray(normalizedOrder, existingOrders) !== -1) {
            normalizedOrder += 1;
        }

        return normalizedOrder;
    }

    function resetDashboardPageFilters() {
        $('#dashboardMenuDropDownField').val('0');
        $('#dashboardSubMenuDropDownField').empty();
        $('#dashboardSubMenuDropDownField').append('<option value="0">--All--</option>');
        availableDashboardPages = availableDashboardPagesAll;
        populateDashboardPageSelect();
    }

    function setDefaultDashboardOrder() {
        var maxOrder = 0;
        $.each(dashboardLayoutData, function (i, item) {
            if (item.dashboardOrder && item.dashboardOrder > maxOrder) {
                maxOrder = item.dashboardOrder;
            }
        });

        $('#dashboardOrderInput').val(maxOrder + 1);
    }

    function renderDashboardLayoutTable() {
        var tbody = $('#dashboardLayoutTable tbody');
        tbody.empty();

        if (!dashboardLayoutData || dashboardLayoutData.length === 0) {
            tbody.append('<tr><td colspan="4" class="text-muted">No pages configured for this role.</td></tr>');
            return;
        }

        $.each(dashboardLayoutData, function (i, item) {
            var isActiveLabel = (item.isActive || '').toUpperCase() === 'N' ? 'Disabled' : 'Enabled';
            var toggleAction = (item.isActive || '').toUpperCase() === 'N' ? 'Enable' : 'Disable';
            var toggleStatus = (item.isActive || '').toUpperCase() === 'N' ? 'Y' : 'N';

            var row = '<tr>'
                + '<td>' + item.pageName + '</td>'
                + '<td><input class="form-control form-control-sm" type="number" min="1" value="' + (item.dashboardOrder || '') + '" data-page-id="' + item.pageId + '" /></td>'
                + '<td>' + isActiveLabel + '</td>'
                + '<td>'
                + '<button class="btn btn-sm btn-outline-primary me-2" type="button" data-onclick="updateDashboardOrder(' + item.pageId + ');">Update Order</button>'
                + '<button class="btn btn-sm btn-outline-secondary" type="button" data-onclick="toggleDashboardStatus(' + item.pageId + ', \'' + toggleStatus + '\');">' + toggleAction + '</button>'
                + '</td>'
                + '</tr>';

            tbody.append(row);
        });
    }

    function addDashboardPage() {
        var roleId = $('#dashboardRoleSelect').val();
        var pageId = $('#dashboardPageSelect').val();
        var order = $('#dashboardOrderInput').val();
        var selectedPageName = $('#dashboardPageSelect option:selected').text();

        if (!roleId || roleId === '0') {
            alert("Select a role to continue.");
            return;
        }

        if (!pageId || pageId === '0') {
            alert("Select a page to continue.");
            return;
        }

        var normalizedOrder = normalizeDashboardOrder(order, null);
        $('#dashboardOrderInput').val(normalizedOrder);

        var newItem = {
            roleId: parseInt(roleId),
            pageId: parseInt(pageId),
            pageName: selectedPageName,
            dashboardOrder: normalizedOrder,
            isActive: 'Y',
            actionInd: 'A'
        };

        dashboardLayoutData.push(newItem);
        renderDashboardLayoutTable();

        submitDashboardLayout([newItem]);
    }

    function updateDashboardOrder(pageId) {
        var roleId = $('#dashboardRoleSelect').val();
        var input = $('#dashboardLayoutTable input[data-page-id="' + pageId + '"]');
        var order = input.val();
        var item = dashboardLayoutData.find(function (entry) { return entry.pageId === pageId; });
        var isActive = item ? item.isActive : 'Y';
        var normalizedOrder = normalizeDashboardOrder(order, pageId);

        input.val(normalizedOrder);
        if (item) {
            item.dashboardOrder = normalizedOrder;
        }

        submitDashboardLayout([
            {
                roleId: parseInt(roleId),
                pageId: parseInt(pageId),
                dashboardOrder: normalizedOrder,
                isActive: isActive,
                actionInd: 'U'
            }
        ]);
    }

    function toggleDashboardStatus(pageId, isActive) {
        var roleId = $('#dashboardRoleSelect').val();
        var input = $('#dashboardLayoutTable input[data-page-id="' + pageId + '"]');
        var order = input.val();
        var action = isActive === 'N' ? 'D' : 'U';
        var normalizedOrder = normalizeDashboardOrder(order, pageId);

        input.val(normalizedOrder);
        var item = dashboardLayoutData.find(function (entry) { return entry.pageId === pageId; });
        if (item) {
            item.dashboardOrder = normalizedOrder;
        }

        submitDashboardLayout([
            {
                roleId: parseInt(roleId),
                pageId: parseInt(pageId),
                dashboardOrder: normalizedOrder,
                isActive: isActive,
                actionInd: action
            }
        ]);
    }

    function submitDashboardLayout(payload) {
        $.ajax({
            url: g_asiBaseURL + "/Administration/DashboardLayout/Save",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            cache: false,
            success: function (response) {
                if (!response || !response.success) {
                    showApiAlert(response, "Unable to save dashboard layout.");
                    return;
                }

                loadDashboardLayout();
            },
            error: function () {
                alert("Unable to save dashboard layout.");
            }
        });
    }

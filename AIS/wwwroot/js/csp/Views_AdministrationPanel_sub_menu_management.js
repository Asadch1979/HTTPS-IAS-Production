    var g_menuId = 0;
    var g_subMenuId = 0;
    var g_subMenuArr = [];
    function getSubMenus() {

        if ($('#menuDropDownField').val() == 0) {
            $('#listofSubMenus tbody').empty();
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_sub_menu_for_admin_panel",
            type: "POST",
            data: {
                "M_ID": $('#menuDropDownField').val()
            },
            cache: false,
            success: function (data) {
                g_subMenuArr = data;
                $('#listofSubMenus tbody').empty();
                $.each(data, function (i, v) {
                    $('#listofSubMenus tbody').append('<tr><td>' + ++i + '</td><td>' + v.suB_MENU_NAME + '</td><td>' + v.description + '</td><td>' + v.suB_MENU_ORDER + '</td><td>' + v.status + '</td><td><a href="#" data-onclick="event.preventDefault();updateSubMenuAssignment(' + v.menU_ID + ', ' + v.suB_MENU_ID + ');">Update</a></td></tr>');

                });
            },
            dataType: "json",
        });

    }

    function setupNewMenuPage() {
        g_pageId = 0;
        $('#subMenuDetailModel').modal('show');
        $('#sub_menu_modal_menu_dd').val(0);
        $('#page_menu_sub_field').val(0);
        $('#sub_menu_modal_submenu_tb').val("");
        $('#page_path_field').val("");
        $('#sub_menu_modal_submenu_order_tb').val("");
        $('#sub_menu_modal_submenu_status_tb').val("");
        $('#sub_menu_modal_submenu_desc_tb').val(0);

    }

    function updateSubMenuAssignment(menuId, menuSubId) {
        $('#subMenuDetailModel').modal('show');
        g_menuId = menuId;
        g_subMenuId = menuSubId;
        $.each(g_subMenuArr, function (i, v) {
            if (v.suB_MENU_ID == g_subMenuId && v.menU_ID == g_menuId) {
                $('#sub_menu_modal_menu_dd').val(v.menU_ID);
                $('#sub_menu_modal_submenu_tb').val(v.suB_MENU_NAME);
                $('#sub_menu_modal_submenu_order_tb').val(v.suB_MENU_ORDER);
                $('#sub_menu_modal_submenu_status_tb').val(v.status);
                $('#sub_menu_modal_submenu_desc_tb').val(v.description);

            }
        })
    }
    function reloadLocation() {
        getSubMenus();
    }

    function publishUpdateSubMenuChanges() {

        if ($('#sub_menu_modal_menu_dd').val() == 0) {
            alert("Select Menu to proceed");
            return;
        }
        if ($('#sub_menu_modal_submenu_tb').val() == "") {
            alert("Enter Sub Menu Name to proceed");
            return;
        }
      
        if ($('#sub_menu_modal_submenu_order_tb').val() == "") {
            alert("Enter Order to proceed");
            return;
        }
        if ($('#sub_menu_modal_submenu_status_tb').val() == "") {
            alert("Enter Status to proceed");
            return;
        }
        if (g_subMenuId == 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_sub_menu_for_admin_panel",
                type: "POST",
                data: {
                    'MENU_ID': $('#sub_menu_modal_menu_dd').val(),
                    'SUB_MENU_ID': g_subMenuId,
                    'SUB_MENU_NAME': $('#sub_menu_modal_submenu_tb').val(),
                    'SUB_MENU_ORDER': $('#sub_menu_modal_submenu_order_tb').val(),
                    'STATUS': $('#sub_menu_modal_submenu_status_tb').val(),
                    'DESCRIPTION': $('#sub_menu_modal_submenu_desc_tb').val()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        } else {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_sub_menu_for_admin_panel",
                type: "POST",
                data: {
                    'MENU_ID': $('#sub_menu_modal_menu_dd').val(),
                    'SUB_MENU_ID': g_subMenuId,
                    'SUB_MENU_NAME': $('#sub_menu_modal_submenu_tb').val(),
                    'SUB_MENU_ORDER': $('#sub_menu_modal_submenu_order_tb').val(),
                    'STATUS': $('#sub_menu_modal_submenu_status_tb').val(),
                    'DESCRIPTION': $('#sub_menu_modal_submenu_desc_tb').val()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }


    }
    function getSubMenusForModel() {

        if ($('#sub_menu_modal_menu_dd').val() == 0) {
            $('#page_sub_menu_field').empty();
            $('#page_sub_menu_field').append('<option value="0">--All--</option>');
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_sub_menu_for_admin_panel",
            type: "POST",
            data: {
                "M_ID": $('#sub_menu_modal_menu_dd').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#page_sub_menu_field').append('<option value="' + v.suB_MENU_ID + '">' + v.suB_MENU_NAME + '</option>');

                });

                if (g_subMenuId != 0) {
                    $('#page_sub_menu_field').val(g_subMenuId);
                }

            },
            dataType: "json",
        });

    }

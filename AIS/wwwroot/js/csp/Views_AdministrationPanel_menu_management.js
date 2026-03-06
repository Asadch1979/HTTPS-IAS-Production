    var g_menuId = 0;  
    var g_menuArr = [];

    $(document).ready(function () {
        getMenus();
    });
  
    function getMenus() {

       

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_menu_pages_for_admin_panel",
            type: "POST",
            data: {
                "M_ID":$('#menuDropDownField').val(),
                "SM_ID":$('#subMenuDropDownField').val()
            },
            cache: false,
            success: function (data) {
                g_menuArr = data;
                $('#listOfMenuPages tbody').empty();
                $.each(data, function (i, v) {
                    $('#listOfMenuPages tbody').append('<tr><td>' + ++i + '</td><td>' + v.sM_NAME + '</td><td>' + v.p_NAME + '</td><td>' + v.p_PATH + '</td><td>' + v.p_ORDER + '</td><td>' + v.p_STATUS + '</td><td>' + v.p_HIDE_MENU + '</td><td><a href="#" data-click="event.preventDefault();updatePageAssignment(' + v.p_ID + ', ' + v.m_ID + ', ' + v.sM_ID + ');">Update</a></td></tr>');
                   
                });

            },
            dataType: "json",
        });

    }
    function setupNewMenu() {
        g_pageId = 0;
        $('#menuDetailModel').modal('show');
        $('#page_menu_field').val(0);
        $('#page_menu_sub_field').val(0);
        $('#menu_name_field').val("");
        $('#menu_desc_field').val("");
        $('#menu_order_field').val("");
        $('#menu_status_field').val("");
        $('#page_hide_menu_field').val(0);       

    }

    function updatePageAssignment(pageId, menuId, menuSubId) {
        $('#menuDetailModel').modal('show');
        g_pageId = pageId;
        g_menuId = menuId;
        $.each(g_pageArr,function(i,v){
            if(v.p_ID==g_pageId){
                $('#page_menu_field').val(v.m_ID);
                $('#page_menu_sub_field').val(v.sM_ID);
                $('#menu_name_field').val(v.p_NAME);
                $('#menu_desc_field').val(v.p_PATH);
                $('#menu_order_field').val(v.p_ORDER);
                $('#menu_status_field').val(v.p_STATUS);
                $('#page_hide_menu_field').val(v.p_HIDE_MENU);
                g_subMenuId = v.sM_ID;
                getSubMenusForModel();

            }
        })
    }
    function reloadLocation() {
        location.reload();
    }
    
    function publishUpdateMenuChanges() {

       
        if ($('#menu_name_field').val() == "") {
            alert("Enter Menu Name to proceed");
            return;
        }
       
        if ($('#menu_order_field').val() == "") {
            alert("Enter Menu Order to proceed");
            return;
        }
        if ($('#menu_status_field').val() == "") {
            alert("Enter Menu status to proceed");
            return;
        }
        if (g_menuId == 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_menu_page_for_admin_panel",
                type: "POST",
                data: {
                    'M_ID': $('#page_menu_field').val(),
                    'SM_ID': $('#page_menu_sub_field').val(),
                    'P_ID': g_pageId,
                    'P_NAME': $('#menu_name_field').val(),
                    'P_PATH': $('#menu_desc_field').val(),
                    'P_ORDER': $('#menu_order_field').val(),
                    'P_STATUS': $('#menu_status_field').val(),
                    'P_HIDE_MENU': $('#page_hide_menu_field').val()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }else
        {
            $.ajax({
                        url: g_asiBaseURL + "/ApiCalls/update_menu_page_for_admin_panel",
                        type: "POST",
                        data: {
                            'M_ID': $('#page_menu_field').val(),
                            'SM_ID': $('#page_menu_sub_field').val(),
                            'P_ID': g_pageId,
                            'P_NAME': $('#menu_name_field').val(),
                            'P_PATH': $('#menu_desc_field').val(),
                            'P_ORDER': $('#menu_order_field').val(),
                            'P_STATUS': $('#menu_status_field').val(),
                            'P_HIDE_MENU': $('#page_hide_menu_field').val()
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

        if ($('#page_menu_field').val() == 0) {
            $('#page_sub_menu_field').empty();
            $('#page_sub_menu_field').append('<option value="0">--All--</option>');
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_sub_menu_for_admin_panel",
            type: "POST",
            data: {
                "M_ID": $('#page_menu_field').val()
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#page_sub_menu_field').append('<option value="' + v.suB_MENU_ID + '">' + v.suB_MENU_NAME + '</option>');

                });

                if (g_subMenuId!=0){
                    $('#page_sub_menu_field').val(g_subMenuId);
                }

            },
            dataType: "json",
        });

    }

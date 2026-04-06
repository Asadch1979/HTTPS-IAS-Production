    var g_pageId = 0;
    var g_menuId = 0;  
    var g_subMenuId = 0;  
    var g_pageArr = [];
    function getSubMenus() {

        if ($('#menuDropDownField').val() == 0) {
            $('#listOfMenuPages tbody').empty();
            $('#subMenuDropDownField').empty();
            $('#subMenuDropDownField').append('<option value="0">--All--</option>');
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
                $.each(data, function (i, v) {
                    $('#subMenuDropDownField').append('<option value="' + v.suB_MENU_ID + '">' + v.suB_MENU_NAME + '</option>');

                });

            },
            dataType: "json",
        });

    }
    function getMenuPages() {

        if($('#menuDropDownField').val()==0){
            
            $('#listOfMenuPages tbody').empty();
            return;

        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_menu_pages_for_admin_panel",
            type: "POST",
            data: {
                "M_ID":$('#menuDropDownField').val(),
                "SM_ID":$('#subMenuDropDownField').val()
            },
            cache: false,
            success: function (data) {
                g_pageArr = data;
                $('#listOfMenuPages tbody').empty();
                $.each(data, function (i, v) {
                    $('#listOfMenuPages tbody').append('<tr><td>' + ++i + '</td><td>' + v.sM_NAME + '</td><td>' + v.p_NAME + '</td><td>' + (v.p_KEY || '') + '</td><td>' + (v.p_URL || '') + '</td><td>' + v.p_PATH + '</td><td>' + v.p_ORDER + '</td><td>' + v.p_STATUS + '</td><td>' + v.p_HIDE_MENU + '</td><td><a href="#" data-onclick="event.preventDefault();updatePageAssignment(' + v.p_ID + ', ' + v.m_ID + ', ' + v.sM_ID + ');">Update</a></td></tr>');
                   
                });

            },
            dataType: "json",
        });

    }
    function setupNewMenuPage() {
        g_pageId = 0;
        g_subMenuId = 0;
        $('#menuPageDetailModel').modal('show');
        $('#page_menu_field').val(0);
        $('#page_menu_sub_field').val(0);
        $('#page_name_field').val("");
        $('#page_key_field').val("");
        $('#page_url_field').val("");
        $('#page_path_field').val("");
        $('#page_order_field').val("");
        $('#page_status_field').val("");
        $('#page_hide_menu_field').val(0);       

    }

    function updatePageAssignment(pageId, menuId, menuSubId) {
        $('#menuPageDetailModel').modal('show');
        g_pageId = pageId;
        g_menuId = menuId;
        $.each(g_pageArr,function(i,v){
            if(v.p_ID==g_pageId){
                $('#page_menu_field').val(v.m_ID);
                $('#page_menu_sub_field').val(v.sM_ID);
                $('#page_name_field').val(v.p_NAME);
                $('#page_key_field').val(v.p_KEY || "");
                $('#page_url_field').val(v.p_URL || "");
                $('#page_path_field').val(v.p_PATH);
                $('#page_order_field').val(v.p_ORDER);
                $('#page_status_field').val(v.p_STATUS);
                $('#page_hide_menu_field').val(v.p_HIDE_MENU);
                g_subMenuId = v.sM_ID;
                getSubMenusForModel();

            }
        })
    }
    function reloadLocation() {
        $('#menuPageDetailModel').modal('hide');
        getMenuPages();
    }
    
    function publishUpdateMenuPageChanges() {

        if ($('#page_menu_field').val()==0){
            alert("Select Menu to proceed");
            return;
        }
        if ($('#page_name_field').val() == "") {
            alert("Enter Page Name to proceed");
            return;
        }
        if ($('#page_key_field').val() == "") {
            alert("Enter Page Key to proceed");
            return;
        }
        if ($('#page_url_field').val() == "") {
            alert("Enter Page URL to proceed");
            return;
        }
        if ($('#page_path_field').val() == "") {
            alert("Enter Page Path to proceed");
            return;
        }
        if ($('#page_order_field').val() == "") {
            alert("Enter Page Order to proceed");
            return;
        }
        if ($('#page_status_field').val() == "") {
            alert("Enter Page Status to proceed");
            return;
        }
        if(g_pageId==0){
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_menu_page_for_admin_panel",
                type: "POST",
                data: {
                    'M_ID': $('#page_menu_field').val(),
                    'SM_ID': $('#page_menu_sub_field').val(),
                    'P_ID': g_pageId,
                    'P_NAME': $('#page_name_field').val(),
                    'P_KEY': $('#page_key_field').val(),
                    'P_URL': $('#page_url_field').val(),
                    'P_PATH': $('#page_path_field').val(),
                    'P_ORDER': $('#page_order_field').val(),
                    'P_STATUS': $('#page_status_field').val(),
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
                    'P_NAME': $('#page_name_field').val(),
                    'P_KEY': $('#page_key_field').val(),
                    'P_URL': $('#page_url_field').val(),
                    'P_PATH': $('#page_path_field').val(),
                    'P_ORDER': $('#page_order_field').val(),
                    'P_STATUS': $('#page_status_field').val(),
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
            $('#page_menu_sub_field').empty();
            $('#page_menu_sub_field').append('<option value="0">--All--</option>');
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
                 $('#page_menu_sub_field').empty();
                $('#page_menu_sub_field').append('<option value="0">--All--</option>');
                $.each(data, function (i, v) {
                    $('#page_menu_sub_field').append('<option value="' + v.suB_MENU_ID + '">' + v.suB_MENU_NAME + '</option>');

                });

                if (g_subMenuId!=0){
                    $('#page_menu_sub_field').val(g_subMenuId);
                }

            },
            dataType: "json",
        });

    }

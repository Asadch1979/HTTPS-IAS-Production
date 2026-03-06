   
    $(document).ready(function () {
        
    });  
    
  
    function getNewUsersInAIS() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/admin_get_new_users",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                $('#newUsersGrid tbody').empty();
                $.each(data, function (i, v) {
                    if (v.entitY_ID.trim().length > 0)
                        $('#newUsersGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.ppno + '</td><td>' + v.emP_NAME + '</td><td>' + v.designatioN_CODE + '</td><td>' + v.designation + '</td><td>' + v.employeE_TYPE + '</td><td>' + v.postinG_TYPE + '</td><td>' + v.code + '</td><td>' + v.entitY_NAME + '</td><td>' + v.entitY_ID + '</td><td><a href="#" data-click="event.preventDefault();AddUserDetails(' + v.designatioN_CODE + ',\'' + v.designation + '\');" class="text-danger">Update User</a></td><td><input type="checkbox" ppno="' + v.ppno + '" class="selectBoxForUpdate" /></td></tr>');
                    else
                        $('#newUsersGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.ppno + '</td><td>' + v.emP_NAME + '</td><td>' + v.designatioN_CODE + '</td><td>' + v.designation + '</td><td>' + v.employeE_TYPE + '</td><td>' + v.postinG_TYPE + '</td><td>' + v.code + '</td><td>' + v.entitY_NAME + '</td><td>' + v.entitY_ID + '</td><td><a href="#" data-click="event.preventDefault();addNewEntityInAIS(\'' + v.code + '\',\'' + v.entitY_NAME + '\');" class="text-danger">Add New Entity</a></td><td></td></tr>');
                                               
                });

                if (data.length > 0) 
                    $('#updateButtonAtEnd').removeClass('d-none');
                else
                    $('#updateButtonAtEnd').addClass('d-none');
              
            },
            dataType: "json",
        });
    }

    function viewUserDetails(){
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/admin_get_user_details",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('.step1').show();
                $('#deleteUser').hide();

            },
            dataType: "json",
        });
    }
    function AddUserDetails(code,name) {

        $('#addUserModel').modal('show');
        $('#modalDesignationCode').val(code);
        $('#modalDesignationName').val(name);
        $('#modalRoleName').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/admin_get_user_details",
            type: "POST",
            data: {
                "DESIGNATION_CODE": code
            },
            cache: false,
            success: function (data) {
                $('#modalRoleName').append('<option value="0">--Select Group--</option>');
                var selIndex = 0;
                if (data.length == 1)
                    selIndex = 0;
                else
                    selIndex = data.length;
                $.each(data, function (i, v) {
                    var selected = "";
                    if (selIndex==i && selIndex==0)
                        selected = "selected=\"selected\"";
                    $('#modalRoleName').append('<option ' + selected + ' value=' + v.grouP_ID + '>' + v.grouP_NAME + '</option>');
                });
               
            },
            dataType: "json",
        });
    }

    function addNewEntityInAIS(code,name) {
        window.location.href = g_asiBaseURL + "/AdministrationPanel/entity_addition?code=" + code + "&name=" + name;
    }

    function updateAllSelectedUsers(){
        var ppNos = [];
        $.each($('.selectBoxForUpdate'),function (i,v) {
            if ($(v).is(':checked')) {
                ppNos.push($(v).attr('ppno'));

            }
        });

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_new_user_admin_panel",
            type: "POST",
            data: {
                'PPNOArr': ppNos
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
            },
            dataType: "json",
        });
    }

    function selectAllUserRows(){

        if ($('#selectAllChkBox').is(':checked'))
        {
            $('.selectBoxForUpdate').prop('checked', true);

        } else {
            $('.selectBoxForUpdate').prop('checked', false);
        }
        

    }

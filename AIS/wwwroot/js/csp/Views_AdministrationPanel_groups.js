    $(document).ready(function () {
        var g_groupId = 0;
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfGroups tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newGroupSetup() {
        g_groupId = 0;
        $('#setupGroupModal').modal('show');
        $('#groupNameModalField').val('');
        $('#groupDescModalField').val('');
    }

    function setupGroup(name, description, active, grpId) {
        g_groupId = grpId;
        $('#groupNameModalField').val(name);
        $('#groupDescModalField').val(description);
       
        if (active == "Y")
            $('#groupActiveModalField').click();
        else 
            $('#groupInactiveModalField').click();

        $('#setupGroupModal').modal('show');
    }

    function publishGroupChanges() {

        var name = $('#groupNameModalField').val();
        var desc = $('#groupDescModalField').val();
        var status;
        var badge;
        if ($('#groupActiveModalField').is(':checked')) {
            status = 'Y';
            badge = 'text-bg-success ';
        }
        else {
            status = 'N';
            badge = 'text-bg-danger ';
        }
        $.ajax({
            url: g_asiBaseURL + "/AdministrationPanel/group_add",
            type: "POST",
            data: {
                'GROUP_ID': g_groupId,
                'GROUP_NAME': name,
                'GROUP_DESCRIPTION': desc,
                'ISACTIVE': status
            },
            cache: false,
            success: function (data) {
                $('#setupGroupModal').modal('hide');
                //console.log(data);
                window.location = window.location.pathname;

            },
            dataType: "json",
        });
    }

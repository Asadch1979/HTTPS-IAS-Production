    $(document).ready(function () {
        var g_divId = 0;
        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfDivision tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newDivSetup() {
        g_divId = 0;
        $('#setupDivModal').modal('show');
        $('#divCodeModalField').val('');
        $('#divNameModalField').val('');
        $('#divAddressModalField').val('');
    }

    function setupDivision(code, name, desc, status, id) {
        g_divId = id;
        //console.log(code, name, desc, status,id);
        $('#divCodeModalField').val(code);
        $('#divNameModalField').val(name);
        $('#divAddressModalField').val(desc);
        if (status == "Active")
            $('#divActiveModalField').click();
        else if (status == "InActive")
            $('#divInactiveModalField').click();

        $('#setupDivModal').modal('show');
    }

    function publishDivisionChanges() {
               
        var code=$('#divCodeModalField').val();
        var name= $('#divNameModalField').val();
        var desc = $('#divAddressModalField').val();
        var status;
        var badge;
        if ($('#divActiveModalField').is(':checked')) {
            status = 'Active';
            badge = 'text-bg-success ';
        }
        else {
            status = 'InActive';
            badge = 'text-bg-danger ';
        }
        if (g_divId == 0)
            var row = "<tr id=\"div_" + g_divId + "\"><td class=\"div_code\"><p class=\"fw-normal mb-1\">" + code + "</p></td><td class=\"div_name\"><p class=\"fw-normal mb-1\">" + name + "</p></td ><td class=\"div_desc\"><p class=\"fw-normal mb-1\">" + desc + "</p></td><td class=\"div_status\"><span class=\"badge " + badge+" rounded-pill d-inline\">" + status + "</span></td><td class=\"div_action\"><button type=\"button\" class=\"btn btn-link text-danger btn-sm btn-rounded\" data-click=\"setupDivision('" + code + "','" + name + "','" + desc + "','" + status + "','" + g_divId + "');\">Edit</button></td></tr>";
        else
            $('#div_' + g_divId).html("<td class=\"div_code\"><p class=\"fw-normal mb-1\">" + code + "</p></td><td class=\"div_name\"><p class=\"fw-normal mb-1\">" + name + "</p></td ><td class=\"div_desc\"><p class=\"fw-normal mb-1\">" + desc + "</p></td><td class=\"div_status\"><span class=\"badge " + badge +" rounded-pill d-inline\">" + status + "</span></td><td class=\"div_action\"><button type=\"button\" class=\"btn btn-link text-danger btn-sm btn-rounded\" data-click=\"setupDivision('" + code + "','" + name + "','" + desc + "','" + status + "','" + g_divId + "');\">Edit</button></td>");
        $('#listOfDivision tbody').append(row);
        $('#setupDivModal').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/division_add",
            type: "POST",
            data: {
                'id':g_divId,
                'code': code,
                'name': name,
                'description': desc,
                'status': status
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                window.location = window.location.pathname;
                
            },
            dataType:"json",
        });
    }

    $(document).ready(function () {
        var g_divId = 0;
        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfIC tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newICSetup() {
        g_divId = 0;
        $('#setupICModal').modal('show');
        $('#ICCodeModalField').val('');
        $('#ICNameModalField').val('');
        $('#ICAddressModalField').val('');
    }

    function setupIC(code, name, desc, status, id) {
        g_ICId = id;
        //console.log(code, name, desc, status, id);
        $('#ICCodeModalField').val(code);
        $('#ICNameModalField').val(name);
        $('#ICAddressModalField').val(desc);
        if (status == "Active")
            $('#ICActiveModalField').click();
        else if (status == "InActive")
            $('#ICInactiveModalField').click();

        $('#setupICModal').modal('show');
    }

    function publishICChanges() {

        var code = $('#ICCodeModalField').val();
        var name = $('#ICNameModalField').val();
        var desc = $('#ICAddressModalField').val();
        var status;
        var badge;
        if ($('#ICActiveModalField').is(':checked')) {
            status = 'Active';
            badge = 'text-bg-success ';
        }
        else {
            status = 'InActive';
            badge = 'text-bg-danger ';
        }
        if (g_divId == 0)
            var row = "<tr id=\"IC_" + g_ICId + "\"><td class=\"IC_code\"><p class=\"fw-normal mb-1\">" + code + "</p></td><td class=\"IC_name\"><p class=\"fw-normal mb-1\">" + name + "</p></td ><td class=\"IC_desc\"><p class=\"fw-normal mb-1\">" + desc + "</p></td><td class=\"IC_status\"><span class=\"badge " + badge + " rounded-pill d-inline\">" + status + "</span></td><td class=\"IC_action\"><button type=\"button\" class=\"btn btn-link text-danger btn-sm btn-rounded\" data-onclick=\"setupIC('" + code + "','" + name + "','" + desc + "','" + status + "','" + g_ICId + "');\">Edit</button></td></tr>";
        else
            $('#div_' + g_ICId).html("<td class=\"IC_code\"><p class=\"fw-normal mb-1\">" + code + "</p></td><td class=\"IC_name\"><p class=\"fw-normal mb-1\">" + name + "</p></td ><td class=\"IC_desc\"><p class=\"fw-normal mb-1\">" + desc + "</p></td><td class=\"IC_status\"><span class=\"badge " + badge + " rounded-pill d-inline\">" + status + "</span></td><td class=\"IC_action\"><button type=\"button\" class=\"btn btn-link text-danger btn-sm btn-rounded\" data-onclick=\"setupIC('" + code + "','" + name + "','" + desc + "','" + status + "','" + g_ICId + "');\">Edit</button></td>");
        $('#listOfIC tbody').append(row);
        $('#setupICModal').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/Inspection_Units_add",
            type: "POST",
            data: {
                'id': g_ICId,
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
            dataType: "json",
        });
    }

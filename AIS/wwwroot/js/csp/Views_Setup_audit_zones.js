    $(document).ready(function () {
        var g_divId = 0;
        $('#sidebar_policy').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfAuditZone tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newAZSetup() {
        g_divId = 0;
        $('#setupAZModal').modal('show');
        $('#AZCodeModalField').val('');
        $('#AZNameModalField').val('');
        $('#AZAddressModalField').val('');
    }

    function setupAuditZone(code, name, desc, status, id) {
        g_AZId = id;
        //console.log(code, name, desc, status, id);
        $('#AZCodeModalField').val(code);
        $('#AZNameModalField').val(name);
        $('#AZAddressModalField').val(desc);
        if (status == "Active")
            $('#AZActiveModalField').click();
        else if (status == "InActive")
            $('#AZInactiveModalField').click();

        $('#setupAZModal').modal('show');
    }

    function publishAuditZoneChanges() {

        var code = $('#AZCodeModalField').val();
        var name = $('#AZNameModalField').val();
        var desc = $('#AZAddressModalField').val();
        var status;
        var badge;
        if ($('#AZActiveModalField').is(':checked')) {
            status = 'Active';
            badge = 'text-bg-success ';
        }
        else {
            status = 'InActive';
            badge = 'text-bg-danger ';
        }
        if (g_divId == 0)
            var row = "<tr id=\"AZ_" + g_AZId + "\"><td class=\"AZ_code\"><p class=\"fw-normal mb-1\">" + code + "</p></td><td class=\"AZ_name\"><p class=\"fw-normal mb-1\">" + name + "</p></td ><td class=\"AZ_desc\"><p class=\"fw-normal mb-1\">" + desc + "</p></td><td class=\"AZ_status\"><span class=\"badge " + badge + " rounded-pill d-inline\">" + status + "</span></td><td class=\"AZ_action\"><button type=\"button\" class=\"btn btn-link text-danger btn-sm btn-rounded\" data-onclick=\"setupAuditZone('" + code + "','" + name + "','" + desc + "','" + status + "','" + g_AZId + "');\">Edit</button></td></tr>";
        else
            $('#div_' + g_AZId).html("<td class=\"AZ_code\"><p class=\"fw-normal mb-1\">" + code + "</p></td><td class=\"AZ_name\"><p class=\"fw-normal mb-1\">" + name + "</p></td ><td class=\"AZ_desc\"><p class=\"fw-normal mb-1\">" + desc + "</p></td><td class=\"AZ_status\"><span class=\"badge " + badge + " rounded-pill d-inline\">" + status + "</span></td><td class=\"AZ_action\"><button type=\"button\" class=\"btn btn-link text-danger btn-sm btn-rounded\" data-onclick=\"setupAuditZone('" + code + "','" + name + "','" + desc + "','" + status + "','" + g_AZId + "');\">Edit</button></td>");
        $('#listOfAuditZone tbody').append(row);
        $('#setupAZModal').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/AuditZone_add",
            type: "POST",
            data: {
                'id': g_AZId,
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

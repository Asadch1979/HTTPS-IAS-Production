    $(document).ready(function () {
        var g_entityId = 0;
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listofSubEntities tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newSubEntitySetup() {
        g_entityId = 0;
        $('#setupSubEntityModal').modal('show');
        $('#divCodeModalField').val('0');
        $('#entityNameModalField').val('');
        $('#deptCodeModalField').val('0');
    }

    function setupSubEntity(divCode, deptCode,entName, status, entId) {
        g_entityId = entId;
        $('#divCodeModalField').val(divCode);
        $('#entityNameModalField').val(entName);
        $('#deptCodeModalField').val(deptCode);
        if (status == "Active")
            $('#activeModalField').click();
        else if (status == "InActive")
            $('#inActiveModalField').click();

        $('#setupSubEntityModal').modal('show');
    }

    function publishSubEntityChanges() {

        var divCode = $('#divCodeModalField option:selected').val();
        var divName = $('#divCodeModalField option:selected').text();
        var entityName = $('#entityNameModalField').val();
        var deptCode = $('#deptCodeModalField option:selected').val();
        var deptName = $('#deptCodeModalField option:selected').text();
        
        var status;
        var badge;
        if ($('#activeModalField').is(':checked')) {
            status = 'Active';
            badge = 'text-bg-success ';
        }
        else {
            status = 'InActive';
            badge = 'text-bg-danger ';
        }
        if (g_entityId == 0)
            var row = "<tr id=\"div_" + g_entityId + " \"><td><p class=\"fw - normal mb - 1\">" + divName + "</p></td><td><p class=\"fw - normal mb - 1\">" + deptName + "</p></td><td><p class=\"fw - normal mb - 1\">" + entityName + "</p></td><td><span class=\"badge " + badge + " rounded - pill d - inline\">" + status + "</span></td><td><button type=\"button\" class=\"btn btn - link text - danger btn - sm btn - rounded\" data-click=\"setupSubEntity('" + divCode + "', '" + deptCode + "', '" + entityName + "', '" + status + "', '" + g_entityId + "'); \" > Edit</button></td ></tr >";
        else
            $('#div_' + g_entityId).html("<td><p class=\"fw - normal mb - 1\">" + divName + "</p></td><td><p class=\"fw - normal mb - 1\">" + deptName + "</p></td><td><p class=\"fw - normal mb - 1\">" + entityName + "</p></td><td><span class=\"badge " + badge + " rounded - pill d - inline\">" + status + "</span></td><td><button type=\"button\" class=\"btn btn - link text - danger btn - sm btn - rounded\" data-click=\"setupSubEntity('" + divCode + "', '" + deptCode + "', '" + entityName + "', '" + status + "','" + g_entityId + "'); \" > Edit</button></td >");
        $('#listofSubEntities tbody').append(row);
        $('#setupSubEntityModal').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/add_sub_entity",
            type: "POST",
            data: {
                'ID': g_entityId,
                'NAME': entityName,
                'DIV_ID': divCode,
                'DEP_ID': deptCode,
                'STATUS': status
                
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                window.location = window.location.pathname;
            },
            dataType: "json",
        });
    }

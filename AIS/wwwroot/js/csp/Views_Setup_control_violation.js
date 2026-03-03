    $(document).ready(function () {
        var g_controlViolationId = 0;
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listofControlViolations tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

    function newControlViolationSetup() {
        g_controlViolationId = 0;
        $('#setupControlViolationModal').modal('show');
        $('#controlViolationNameModelField').val('');
        $('#controlViolationMaxNumberModelField').val(0);
        $('#controlViolationActiveModelField').attr('checked', true);
        $('#controlViolationInactiveModelField').attr('checked', false);      
    }

    function setupControlViolation(name, number, status, id ) {
        g_controlViolationId = id;
        $('#controlViolationNameModelField').val(name);
        $('#controlViolationMaxNumberModelField').val(number);
      if (status == "Y")
            $('#controlViolationActiveModelField').click();
        else 
            $('#controlViolationInactiveModelField').click();

        $('#setupControlViolationModal').modal('show');
    }

    function publishControlViolationChanges() {

        var name = $('#controlViolationNameModelField').val();
        var max_number = $('#controlViolationMaxNumberModelField').val();
        var status;
        var badge;
        if ($('#controlViolationActiveModelField').is(':checked')) {
            status = 'Y';
            badge = 'text-bg-success ';
        }
        else {
            status = 'N';
            badge = 'text-bg-danger ';
        }
        
        $.ajax({
            url: g_asiBaseURL + "/Setup/add_control_violation",
            type: "POST",
            data: {
                'ID': g_controlViolationId,
                'V_NAME': name,
                'MAX_NUMBER': max_number,
                'STATUS': status
            },
            cache: false,
            success: function (data) {
                $('#setupControlViolationModal').modal('hide');
                //console.log(data);
                window.location = window.location.pathname;

            },
            dataType: "json",
        });
    }

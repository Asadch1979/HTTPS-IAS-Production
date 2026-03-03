       



    var g_statusId = 0;
    var g_statusRecord = [];


    $(document).ready(function () {


        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#manageObservationStatusGrid tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        getObservationStatus();

    });

    function getObservationStatus() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_maange_obs_status",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_statusRecord = data;
                $('#manageObservationStatusGrid tbody').empty();
                $.each(data, function (i, v) {

                    $('#manageObservationStatusGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.statuS_ID + '</td><td>' + v.statuS_NAME + '</td><td>' + v.iS_ACTIVE + '</td><td>' + v.code + '</td><td>' + v.satisfied + '</td><td><a href="#" onclick="event.preventDefault();UpdateManageObservation(' + v.statuS_ID + ');" class="text-danger">Update</a></td></tr>');

                });

            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#UpdateManageObservation').modal('hide');
      //  getEntityTypes();
        getObservationStatus();
    }

    function addNewObsStatus(){
        g_statusId = 0;
        $('#updateManageObservationStatus').modal('show');
        $('#modalStatusName').val('');
        $('#modalIsActive').val('');
        $('#modalCode').val('');
        $('#modalSatisfied').val('Y');
    }

    function UpdateManageObservation(statusId) {
        g_statusId = statusId;
        $('#updateManageObservationStatus').modal('show');

       
        $('#modalStatusName').val('');
        $('#modalIsActive').val('');
        $('#modalCode').val('');
        $('#modalSatisfied').val('Y');
     
        $.each(g_statusRecord, function (i, v) {
            if (v.statuS_ID == g_statusId) {

          
                $('#modalStatusName').val(v.statuS_NAME);
                $('#modalIsActive').val(v.iS_ACTIVE);
                $('#modalCode').val(v.code);
                $('#modalSatisfied').val(v.satisfied);
              

            }
        })

    }
    function saveChangesManageObservationStatus() {
        var manageObservationStatusModel = {

            'STATUS_ID': g_statusId,
            'STATUS_NAME': $('#modalStatusName').val(),
            'IS_ACTIVE': $('#modalIsActive').val(),
            'CODE': $('#modalCode').val(),
            'SATISFIED': $('#modalSatisfied').val(),

        }
        if (g_statusId == 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_manage_observatiton_status",
                type: "POST",
                data: {
                    OBS_STATUS_MODEL: manageObservationStatusModel
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
                url: g_asiBaseURL + "/ApiCalls/update_manage_observatiton_status",
                type: "POST",
                data: {
                    OBS_STATUS_MODEL: manageObservationStatusModel
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

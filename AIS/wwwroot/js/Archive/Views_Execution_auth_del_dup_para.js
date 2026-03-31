    var g_np_id = 0;
    var g_op_id = 0;
    var g_ind = "";
    var g_allObs = [];
    $(document).ready(function () {
       getEntityObservation();
    });
    function reloadLocation() {
        getEntityObservation();
    }
    function getEntityObservation() {
        destroyDatatable('manageObsPanel');
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_duplicate_paras_for_authorize",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    g_allObs = data;
                    $.each(data, function (i, v) {
                        $('#manageObsPanel tbody').append('<tr><td class="text-center">' + ++i + '</td><td class="text-center">' + v.entityName + '</td><td class="text-center">' + v.auditPeriod + '</td><td>' + v.paraNo + '</td><td>' + v.annex + '</td><td>' + v.risk + '</td><td>' + v.paraGist + '</td><td>' + v.remarks + '</td><td class="text-center"><a data-onclick="event.preventDefault();rejectDeleteDuplicateRequest(\'' + v.dId + '\')" href="#" class="text-hover">Reject</a></td><td class="text-center"><a data-onclick="event.preventDefault();authorizeDeleteDuplicateRequest(\'' + v.dId + '\')" href="#" class="text-hover">Authorize</a></td></tr>');
                    });

                    initializeDataTable('manageObsPanel');
                },
                dataType: "json",
            });
        }
    }
    function rejectDeleteDuplicateRequest(did) {
           $.ajax({
                url: g_asiBaseURL + "/ApiCalls/reject_delete_duplicate_para",
                type: "POST",
                data: {
                    'D_ID':did
                },
                cache: false,
                success: function (data) {
                  showApiAlert(data);
                      onAlertCallback(getEntityObservation)
                },
                dataType: "json",
            });

    }

      function authorizeDeleteDuplicateRequest(did) {

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/authorize_delete_duplicate_para",
                type: "POST",
                data: {
                    'D_ID':did
                },
                cache: false,
                success: function (data) {
                  showApiAlert(data);
                      onAlertCallback(getEntityObservation)
                },
                dataType: "json",
            });


    }

    var g_paraId = 0;
    var refp = 0;
    var g_obsList = [];
    var g_action = "A";
    var g_ind = "";

    $(document).ready(function () {
        getLegacyPara();
    });

    function getLegacyPara() {

        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_current_paras_for_status_change_request_authorize",
            type: "POST",
            data: {
                'ENTITY_ID': $('#entitySelectField').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;

                $.each(data, function (index, child) {
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p class="fw-normal mb-1">' + child.amounT_INVOLVED + '</p></td><td><p class="fw-normal mb-1">' + child.parA_RISK + '</p></td><td class="col-md-1"><p  class="fw-normal mb-1" >' + child.parA_STATUS + '</p></td><td class="col-md-1"><p  class="fw-normal mb-1" >' + child.makeR_REMARKS + '</p></td><td class="col-md-1"><p  class="fw-normal mb-1" >' + child.revieweR_REMARKS + '</p></td><td class="text-center"><a class="text-center text-primary" style="cursor:pointer;" data-onclick="event.preventDefault();paraText(\'' + child.id + '\');">View Para Text</a></td><td><a class="text-danger" style="cursor:pointer;" data-onclick="parastatuschange(\'' + child.id + '\',\'' + child.ind + '\');">Authorize</a> | <a class="text-danger" style="cursor:pointer;" data-onclick="rejectPara(\'' + child.id + '\',\'' + child.ind + '\');">Reject</a></td></tr>');
                });

            },

            dataType: "json",
        });

    }
    function parastatuschange(id, ind) {
        g_action = 'A';
        refp = id;
        g_ind = ind;
        $('#process_detail').modal('show');
    }

    function rejectPara(id, ind) {
        g_action = 'R';
        refp = id;
        g_ind = ind;
        $('#process_detail').modal('show');
    }

    function Publishchange() {

        if ($('#Reason_Unsettle').val() == "") {
            alert("Please enter Reply");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Old_Para_Change_status_Authorize",
            type: "POST",
            data: {
                'REFID': refp,
                'REMARKS': $('#Reason_Unsettle').val(),
                'Action_IND': g_action,
                'IND': g_ind
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
                $('#process_detail').modal('hide');
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        getLegacyPara();
    }

    function paraText(id) {
        $('#paraTextDisplayModel').modal('show');
        $('#paraTextModelPanel').empty();

        $.get(g_asiBaseURL + "/ApiCalls/GetIASPARATEXT", { comId: id }, function (data) {
            $('#paraTextModelPanel').html(data);
        });

    }

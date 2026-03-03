    var g_paraId = 0;
    var g_obsList = [];
    
    var g_paraRef = "";
    var g_paraObsId = 0;
    var g_ind = "";
    var g_action = "A";
    $(document).ready(function () {
        getLegacyPara();

        $('#1').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#PublishParaText').on('click', function () {
            publishResponseChanges();
        });
    });
    function getLegacyPara() {
       
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_settled_paras_autorize",
            type: "POST",
            data: {
                
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {
                    var status_text= child.parA_STATUS;
                    var status_change_text = child.parA_CHANGE_REQUEST_STATUS;
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p class="fw-normal mb-1">' + child.parA_RISK + '</p></td><td><p class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td><p class="fw-normal mb-1">' + status_text + '</p></td><td><p class="fw-normal mb-1">' + status_change_text + '</p></td><td class="col-md-1"><p  class="fw-normal mb-1" >' + child.remarks + '</p></td><td class="text-center"><a class="text-center text-primary" style="cursor:pointer;" onclick="event.preventDefault();paraText(\'' + child.reF_P + '\',\'' + child.aU_OBS_ID + '\');">Para Text</a></td><td class="text-center"><a class="text-danger" style="cursor:pointer;" onclick="event.preventDefault();parastatuschange(\'' + child.reF_P + '\',\'' + child.aU_OBS_ID + '\',\'' + child.ind + '\');">Authorize</a> | <a class="text-danger" style="cursor:pointer;" onclick="event.preventDefault();rejectPara(\'' + child.reF_P + '\',\'' + child.aU_OBS_ID + '\',\'' + child.ind + '\');">Reject</a></td></tr>');
                });
            },

            dataType: "json",
        });

    }
    function parastatuschange(refp, obsId, ind) {

        g_action = 'A';
        g_paraRef = refp;
        g_paraObsId = obsId;
        g_ind = ind;
        confirmAlert("Do you confirm to authorize this Para Status Change Request");
        onconfirmAlertCallback(Publishchange);

    }

    function rejectPara(refp, obsId, ind) {
        g_action = 'R';
        g_paraRef = refp;
        g_paraObsId = obsId;
        g_ind = ind;
        confirmAlert("Do you confirm to reject this Para Status Change Request");
        onconfirmAlertCallback(Publishchange);
    }


    function Publishchange() {

       
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Authorization_Old_Para_Change_status",
            type: "POST",
            data: {
                'REFID': g_paraRef,
                'OBS_ID': g_paraObsId,
                'Action_IND': g_action,
                'IND': g_ind
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        getLegacyPara();
    }

    function paraText(refp, obsId) {
        $('#paraTextDisplayModel').modal('show');
        $('#paraTextModelPanel').empty();

        $.get(g_asiBaseURL + "/ApiCalls/GetIASPARATEXT", { comId: obsId }, function (data) {
            $('#paraTextModelPanel').html(data);
        });

    }

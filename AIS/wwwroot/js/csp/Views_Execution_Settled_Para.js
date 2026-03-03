    var g_paraId = 0;
    var g_paraRef = 0;
    var g_paraObsId = 0;
    var g_ind = "";
    var g_obsList = [];

    function getLegacyPara() {

        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_settled_paras",
            type: "POST",
            data: {
                'ENTITY_ID': $('#entitySelectField').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p class="fw-normal mb-1">' + child.amounT_INVOLVED + '</p></td><td class="col-md-1" style="text-align:center"><p  class="fw-normal mb-1" >' + child.voL_I_II + '</p></td><td class="col-md-1" style="text-align:center"><p  class="fw-normal mb-1" >' + child.parA_STATUS + '</p></td><td class="text-center"><a class="text-center text-danger" style="cursor:pointer;" onclick="event.preventDefault();parastatuschange(\'' + child.reF_P + '\',\'' + child.id + '\',\'' + child.ind + '\');">Change Status</a></td></tr>')
                });
            },

            dataType: "json",
        });

    }
    function parastatuschange(refP, id, ind) {
        g_paraRef = refP;
        g_paraObsId = id;
        g_ind = ind;
        $('#process_detail').modal('show');
        $('#Reason_Unsettle').val('');
        $('#checklistDetailField').empty();
        if (ind == "O") {
            $('#checklistDetailField').append('<option value="6">Settle</option>');
            $('#checklistDetailField').append('<option value="8">Un-settle</option>');

        } else {
            $('#checklistDetailField').append('<option value="9">Settle</option>');
            $('#checklistDetailField').append('<option value="8">Un-settle</option>');

        }

    }

    function Publishchange() {

        if ($('#Reason_Unsettle').val() == "") {
            alert("Please enter Reply");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Old_Para_Change_status",
            type: "POST",
            data: {
                'REFID': g_paraRef,
                'OBS_ID': g_paraObsId,
                'INDICATOR': g_ind,
                'NEW_STATUS': $('#checklistDetailField').val(),
                'REMARKS': $('#Reason_Unsettle').val()
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

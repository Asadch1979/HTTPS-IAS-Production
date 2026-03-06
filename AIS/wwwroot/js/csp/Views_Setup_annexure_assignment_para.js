    var g_refP = "";
    var g_obsId = "";
    var g_pc = "";
    $('#document').ready(function () {

    });

    function getZoneBranches() {
        $('#manageObsPanel tbody').empty();
        $('#branchSelectField').empty();

        if ($('#zoneSelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_zone_Branches_for_Annexure_Assignment",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#zoneSelectField option:selected').val()
                },
                cache: false,
                success: function (data) {

                    $('#branchSelectField').append('<option value="0" id="0">--Select Branch--</option>');
                    $.each(data, function (i, v) {
                        $('#branchSelectField').append('<option value="' + v.branchid + '" id="' + v.branchid + '">' + v.branchname + '</option>');
                    })

                },
                dataType: "json",
            });

        }
    }

    function getEntityObservation() {
        $('#manageObsPanel tbody').empty();
        if ($('#branchSelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_all_paras_for_annexure_assignment",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#branchSelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                        $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.anneX_ID + '"><td>' + v.audiT_PERIOD + '</td><td class="text-right">' + v.parA_NO + '</td><td class="text-right">' + v.anneX_CODE + '</td><td>' + v.annexure + '</td><td>' + v.gisT_OF_PARAS + '</td><td><a hre="#" class="text-danger text-center" style="cursor:pointer;" data-onclick="event.preventDefault();viewParaText(\'' + v.parA_CATEGORY + '\', \'' + v.obS_ID + '\',\'' + v.reF_P + '\',\'' + v.anneX_ID + '\')"> View Para</a></td></tr>');

                    });
                },
                dataType: "json",
            });

        }
    }

    function viewParaText(pc, obsId, ref_p, annex_id) {
        g_refP = ref_p;
        g_obsId = obsId;
        g_pc = pc;


        $('#viewMemoModel').modal('show');
        $('#updatedAnnexlist').val(annex_id);
        $('#viewMemo_memo').html("");
        if (pc == "A") {
            $.get(g_asiBaseURL + "/ApiCalls/GetIASPARATEXT", { comId: obsId }, function (data) {
                $('#viewMemo_memo').html(data);
            });
        } else {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_para_text",
                type: "POST",
                data: {
                    'ref_p': ref_p,
                    'OBS_ID': obsId
                },
                cache: false,
                success: function (data) {
                    $('#viewMemo_memo').html(data);
                }
            });
        }
    }
    function assignAnnexureWithPara() {

        if ($('#updatedAnnexlist').val() == 0) {
            alert("Select Annexure to proceed");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/assign_annexure_with_para",
            type: "POST",
            data: {
                'REF_P': g_refP,
                'OBS_ID': g_obsId,
                'PARA_CATEGORY': g_pc,
                'ANNEX_ID': $('#updatedAnnexlist').val()
            },
            cache: false,
            success: function (data) {

                showApiAlert(data);
                onAlertCallback(getEntityObservation);
                $('#viewMemoModel').modal('hide');

            },
            dataType: "json"
        });
    }

    function reloadLocation() {
        window.location.reload();
    }

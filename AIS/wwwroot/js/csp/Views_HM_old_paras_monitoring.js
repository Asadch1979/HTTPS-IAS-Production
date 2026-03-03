    function getZoneBranches() {
       
        destroyDatatable('manageObsPanel');
        $('#branchSelectField').empty();

        if ($('#zoneSelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_zone_Branches",
                type: "POST",
                data: {
                    'ZONEID': $('#zoneSelectField option:selected').val()
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
        
           destroyDatatable('manageObsPanel');
        if ($('#branchSelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_old_paras_for_monitoring",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#branchSelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (i, v) {
                        $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.id + '"><td>' + ++i + '</td><td>' + v.entitY_NAME + '</td><td>' + v.audiT_PERIOD + '</td><td>' + v.memO_NO + '</td><td>' + v.parA_RISK + '</td><td>' + v.gisT_OF_PARAS + '</td><td><a hre="#" class="text-danger text-center" style="cursor:pointer;" onclick="event.preventDefault();viewParaText(\'' + v.coM_ID + '\')"> View Para</a></td></tr>');

                    });
                    initializeDataTable('manageObsPanel');
                },
                dataType: "json",
            });

        }
    }

    function viewParaText(comId) {
        if (!comId || comId === "0") return;
        $('#viewMemoModel').modal('show');
        $('#viewMemo_memo').html("");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
            type: "POST",
            data: {
                'COM_ID': comId
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $('#viewMemo_memo').html(data);
            }
        });
    }

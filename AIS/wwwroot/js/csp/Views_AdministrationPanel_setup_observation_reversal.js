    var g_memoStatusReversalIds = [];
    var g_engId = 0;

    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrel",
            type: "POST",
            data: {
                'ENTITY_REALTION_ID': $('#RelationshipField option:selected').val()
            },


            cache: false,
            success: function (data) {


                $('#controlingsearch').append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
                $.each(data, function (index, contof) {

                    var selected = '';
                    if (contof.entitY_ID == parentEntityId)
                        selected = 'selected="selected"';

                    $('#controlingsearch').append('<option ' + selected + ' value="' + contof.entitY_ID + '" id="' + contof.entitY_REALTION_ID + '">' + contof.description + '</option>')
                });
                if (userEntityId != 0)
                    getplacepost(userEntityId)

                // console.log(data);

            },
            dataType: "json",
        });



    }

    function getplacepost(userEntityId = 0) {
        $('#childposting').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: {
                'E_R_ID': $('#controlingsearch option:selected').val()
            },


            cache: false,
            success: function (data) {
                $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
                $.each(data, function (index, gpp) {

                    var selected = '';
                    if (gpp.entitY_ID == userEntityId)
                        selected = 'selected="selected"';
                    $('#childposting').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>')
                });
            },
            dataType: "json",
        });
        //  getrelation();

    }

    function getEngagementDetails() {
        $('#engsListPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_engagements_details_for_status_reversal",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting option:selected').val()
            },

            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    if (v.audiT_START_DATE != null)
                        v.audiT_START_DATE = v.audiT_START_DATE.split(' ')[0];

                    if (v.audiT_END_DATE != null)
                        v.audiT_END_DATE = v.audiT_END_DATE.split(' ')[0];

                    if (v.oP_START_DATE != null)
                        v.oP_START_DATE = v.oP_START_DATE.split(' ')[0];

                    if (v.oP_END_DATE != null)
                        v.oP_END_DATE = v.oP_END_DATE.split(' ')[0];

                    i = i + 1;

                    $('#engsListPanel').append('<tr><td>' + i + '</td><td>' + v.enG_ID + '</td><td>' + v.teaM_NAME + '</td><td>' + v.audiT_START_DATE + '</td><td>' + v.audiT_END_DATE + '</td><td>' + v.oP_START_DATE + '</td><td>' + v.oP_END_DATE + '</td><td>' + v.status + '</td><td class="text-center"><a class="text-primary text-center" data-onclick="event.preventDefault();getObservationsForStatusReversal(' + v.enG_ID + ');" href="#">Status Reversal</a></td><td><a href="#" data-onclick="getObservationsForAssignmentReversal(' + v.enG_ID + ');" class="text-danger text-center">Assignment Reversal</a></td><td class="text-center"><a class="text-primary text-center" data-onclick="getObservationsForReNumbering(' + v.enG_ID + ');" href="#">Change Observation Number</a></td></tr>');
                })

            },
            dataType: "json",
        });

    }

    function getObservationsForStatusReversal(engId) {
        g_engId = engId;

        $('#statusReversalModal').modal('show');
        $('#engObsListPanel tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_details_for_status_reversal",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {

                    if (v.memO_DATE != null)
                        v.memO_DATE = v.memO_DATE.split(' ')[0];

                    $('#engObsListPanel').append('<tr><td class="text-center">' + ++i + '</td><td>' + v.memO_NO + '</td><td>' + v.memO_DATE + '</td><td>' + v.assigneD_TO + '</td><td>' + v.status + '</td><td class="text-center"><input class="rev_chk_obs" id="' + v.id + '"  type="checkbox" /></td></tr>');
                })
            },
            dataType: "json",
        });
    }
    function getObservationsForReNumbering(engId) {

        $('#numberChangeModal').modal('show');
        $('#engObsListPanel_changeNo tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_details_for_status_reversal",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {

                    if (v.memO_DATE != null)
                        v.memO_DATE = v.memO_DATE.split(' ')[0];

                    i = i + 1;

                    $('#engObsListPanel_changeNo').append('<tr><td class="text-center">' + i + '</td><td>' + v.memO_NO + '</td><td>' + v.memO_DATE + '</td><td>' + v.assigneD_TO + '</td><td>' + v.status + '</td><td class="text-center"><input name="memonoradio" id="rev_checkbox_"+' + v.id + ' data-onclick="event.preventDefault();memoIdsToNumberChange(this,' + v.id + ');" type="radio" /></td></tr>');
                })
            },
            dataType: "json",
        });
    }

    function getObservationsForAssignmentReversal(engId) {

        $('#assignmentReversalModal').modal('show');
    }    

    function updateMemoStatuses() {
        $('#newStatusReversalModel').modal('show');
    }

    function updateNewStatusRequest() {
        var obsIds = [];

        $('.rev_chk_obs').each(function (i, v) {
            if ($(v).is(':checked'))
                obsIds.push($(v).attr('id'));
        });

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_status_for_reversal",
            type: "POST",
            data: {
                'OBS_IDS': obsIds,
                'NEW_STATUS_ID': $('#newStatusReservalSelectionBox').val(),
                'ENG_ID': g_engId
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
            },
            dataType: "json",
        });

    }

    function updateMemoNumber() {
        $('#memoNumberChangeModal').modal('show');
    }

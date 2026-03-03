    var g_memoStatusReversalIds = [];
    var g_engID = 0;
    var g_planID = 0;
    var g_obsID = 0;
    var g_auditedByID = 0;
    var g_obsIdsArr = [];

    $(document).ready(function () {
      
    });

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

                    $('#engsListPanel').append('<tr><td>' + i + '</td><td>' + v.enG_ID + '</td><td>' + v.teaM_NAME + '</td><td>' + v.audiT_START_DATE + '</td><td>' + v.audiT_END_DATE + '</td><td>' + v.oP_START_DATE + '</td><td>' + v.oP_END_DATE + '</td><td>' + v.status + '</td><td><a class="text-sucess text-center" onclick="event.preventDefault();getObservationsForStatusReversal(' + v.enG_ID + ');" href="#">Status Reversal</a></td><td><a href="#" onclick="getObservationsForAssignmentReversal(' + v.enG_ID + ',' + v.plaN_ID + ');" class="text-sucess text-center">Assignment Reversal At Planning Stage</a></td><td><a href="#" onclick="proceedToChangeInAuditTeam(' + v.auditeD_BY_ID + ',' + v.enG_ID + ');" class="text-sucess text-center">Post Changes in Team</a></td><td><a href="#" onclick="proceedToChangeEndDate(' + v.enG_ID + ',\'' + v.audiT_START_DATE + '\',\'' + v.audiT_END_DATE + '\');" class="text-sucess text-center">Change Dates</a></td><td class="text-center"><a class="text-sucess text-center" onclick="getObservationsForReNumbering(' + v.enG_ID + ');" href="#">Change Observation Number</a></td></tr>');
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

                    $('#engObsListPanel_changeNo').append('<tr><td class="text-center">' + i + '</td><td>' + v.memO_NO + '</td><td>' + v.memO_DATE + '</td><td>' + v.heading + '</td><td>' + v.assigneD_TO + '</td><td>' + v.status + '</td><td class="text-center"><button onclick="event.preventDefault();obsChangeNumber(' + v.id + ');" class="btn btn-small btn-primary">Update Number</button></td></tr>');
                })
            },
            dataType: "json",
        });
    }

    function obsChangeNumber(obsId) {
        g_obsID = obsId;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_numbers_for_status_reversal",
            type: "POST",
            data: {
                'OBS_ID': obsId
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {

                    $('#memo_no_old').html(v.memO_NUMBER);
                    $('#draft_no_old').html(v.drafT_PARA_NUMBER);
                    $('#final_no_old').html(v.finaL_PARA_NUMBER);


                    $('#memonumber_input').html(v.memO_NUMBER);
                    $('#draftnumber_input').html(v.drafT_PARA_NUMBER);
                    $('#finalnumber_input').html(v.finaL_PARA_NUMBER);

                });
            },
            dataType: "json",
        });

        $('#memoNumberChangeModal').modal('show');
    }

    function getObservationsForStatusReversal(engId) {
        g_engID = engId;
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

                    i = i + 1;

                    $('#engObsListPanel').append('<tr><td class="text-center">' + i + '</td><td>' + v.memO_NO + '</td><td>' + v.memO_DATE + '</td><td>' + v.assigneD_TO + '</td><td>' + v.status + '</td><td class="text-center"><input class="statusselected" id="' + v.id + '"  type="checkbox" /></td></tr>');
                })
            },
            dataType: "json",
        });
    }

    function getObservationsForAssignmentReversal(engId, planId) {
        $('#engStatusReversalNewStatus').empty();
        $('#engStatusReversalNewStatus').append('<option value="0">--Select Engagement Status--</option>');
        g_engID = engId;
        g_planID = planId;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_engagement_status_for_reversal",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#engStatusReversalNewStatus').append('<option value="' + v.statuS_ID + '">' + v.statuS_NAME + '</option>');
                });
                $('#assignmentReversalModal').modal('show');
            },
            dataType: "json",
        });

    }

    function proceedToChangeInAuditTeam(audById, engId) {
        g_auditedByID = audById;
        g_engID = engId;
        $('#setupAuditTeam').modal('show');
        $('#listOfEmployeeTeam tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_team_memeber_details_for_post_changes_team_eng_reversal",
            type: "POST",
            data: {
                'AUDITED_BY_DEPT': audById
            },
            cache: false,
            success: function (data) {
                console.log('auditteams', data);
                var teamId = 0;
                var srNo = 1;
                var teamMembers = [];
                $.each(data, function (index, team) {

                    if (team.iS_TEAMLEAD == "Y") {
                        if (team.status == 'Y')
                            team.status = 'Active';
                        else
                            team.status = 'Inactive';
                        if (team.status == 'Active')
                            $('#listOfEmployeeTeam tbody').append('<tr id=teamcode_' + team.code + '><td class="searchable"><p class="fw-normal mb-1">' + srNo + '</p></td><td class="tName_Col"><p class="fw-normal mb-1">' + team.name + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + team.employeename + ' (' + team.teammembeR_ID + ') </p></td><td class="empMembers"></td><td><input name="newTeamChange" class="newTeamChange" teamName="' + team.name + '" teamCode="' + team.code + '" teamId="' + team.t_ID + '" id="team_' + team.t_ID + '" type="radio" /></td></tr>');

                        srNo++;
                    } else {
                        teamMembers.push(team);
                        if (team.code != teamId) {
                            teamId = team.code;
                            srNo++;
                        }
                    }
                });
                $.each(teamMembers, function (index, team) {
                    if (team.iS_TEAMLEAD != "Y") {
                        prevText = $('#listOfEmployeeTeam tbody #teamcode_' + team.code + ' .empMembers').html();
                        if (prevText != '')
                            prevText += ", ";
                        $('#listOfEmployeeTeam tbody #teamcode_' + team.code + ' .empMembers').text(prevText + team.employeename + '(' + team.teammembeR_ID + ')');
                    }
                });
            },
            dataType: "json",
        });


    }
    function formatDateToISO(dateString) {
        // Attempt to parse the date string
        var date = new Date(dateString);

        // Check if the date is valid
        if (isNaN(date.getTime())) {
            return null;
        }

        // Format the date to yyyy-MM-dd
        var year = date.getFullYear();
        var month = ('0' + (date.getMonth() + 1)).slice(-2); // Months are 0-based in JavaScript
        var day = ('0' + date.getDate()).slice(-2);

        return [year, month, day].join('-');
    }


    function proceedToChangeEndDate(engId, sdate, edate){    
        g_engID = engId;
        $('#engagementDatesUpdationModel').modal('show');
        $('#selectDateInputField').val(formatDateToISO(sdate));
        $('#selectEndDateInputField').val(formatDateToISO(edate));
    }

    function updateMemoStatuses() {
        g_obsIdsArr = [];
        $.each($('.statusselected'), function (i, v) {
            if ($(v).is(":checked")) {
                g_obsIdsArr.push($(v).attr('id'));
            }

        });

        if (g_obsIdsArr.length > 0) {

            $('#newStatusReversalModel').modal('show');
        }
        else {
            alert("Please select atleast one observation to update status");
            return;
        }

    }
    function updateNewMemoNumber() {

        
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_observation_numbers_for_status_reversal",
            type: "POST",
            data: {
                'OBS_ID': g_obsID,
                'MEMO_NUMBER': $('#memonumber_input').val(),
                'DRAFT_PARA_NUMBER': $('#draftnumber_input').val(),
                'FINAL_PARA_NUMBER': $('#finalnumber_input').val()
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#memoNumberChangeModal').modal('hide');
            },
            dataType: "json",
        });


    }
    function updateNewStatusRequest() {

        if ($('#newStatusReservalSelectionBox').val() == 0) {
            alert("Please select new Status to proceed");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/audit_engagement_obs_status_reversal",
            type: "POST",
            data: {
                'NEW_STATUS_ID': $('#newStatusReservalSelectionBox').val(),
                'OBS_IDs': g_obsIdsArr,
                'ENG_ID': g_engID

            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#newStatusReversalModel').modal('hide');
            },
            dataType: "json",
        });


    }

    function updateNewEngagementStatus() {

        if ($('#engStatusReversalNewStatus').val() == 0) {
            alert("Please select new Engagement Status to proceed");
            return;
        }

        if ($('#engStatusReversalComments').val() == 0) {
            alert("Please enter to commetns to proceed");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/audit_engagement_status_reversal",
            type: "POST",
            data: {
                'NEW_STATUS_ID': $('#engStatusReversalNewStatus').val(),
                'COMMENTS': $('#engStatusReversalComments').val(),
                'ENG_ID': g_engID,
                'PLAN_ID': g_planID
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#assignmentReversalModal').modal('hide');
            },
            dataType: "json",
        });


    }

    function publishNewTeamChanges() {

        if ($('.newTeamChange:checked').length == 0) {
            alert("Please select Team to proceed");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_new_team_id_for_post_changes_team_eng_reversal",
            type: "POST",
            data: {
                'TEAM_ID': $('.newTeamChange:checked').attr('teamid'),
                'TEAM_NAME': $('.newTeamChange:checked').attr('teamname'),
                'ENG_ID': g_engID,
                'AUDITED_BY_ID': g_auditedByID
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#setupAuditTeam').modal("hide");
                //onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });

    }

    function finalChangeDatesOfEngagement() {


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_engagement_dates_for_status_reversal",
            type: "POST",
            data: {
                'ENG_ID': g_engID,
                'START_DATE': $('#selectDateInputField').val(),
                'END_DATE': $('#selectEndDateInputField').val()
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#engagementDatesUpdationModel').modal('hide');
            },
            dataType: "json",
        });


    }

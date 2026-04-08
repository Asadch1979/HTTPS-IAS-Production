    $(document).ready(function () {
        $("#searchTeamFormation").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfEmployeeTeam tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        
    });

    function addNewTeam() {
        
        $('#teamNameModalField').val('');
        $('#teamCodeModalField').val('');
        $.each($('.participantcheckboxes'), function (index, member) { $(member).attr('checked', false); });
        $.each($('.teamleadradio'), function (index, member) { $(member).attr('checked', false); });
       
        $('#setupAuditTeam').modal('show');
    }

    function reloadLocation() {
        if (window.planningDashboard && typeof window.planningDashboard.reloadCurrentStep === 'function') {
            window.planningDashboard.reloadCurrentStep();
            return;
        }

        location.reload();
    }

    function publishNewTeamChanges() {
        
       
      /*  var teamCode = $('#teamCodeModalField').val()
        if (teamCode == '') {
            alert('Enter Team Code');
            return false;
        }
        */
        var teamName = $('#teamNameModalField').val();

        if (document.querySelectorAll('input.alnum-only.is-invalid').length > 0) {
            Swal.fire({ icon: "error", title: "Validation error", text: "Please correct highlighted fields." });
            return false;
        }

        if (teamName == '') {
            alert('Enter Team Name');
            return false;
        }     

        if (!$('.participantcheckboxes').is(':checked')) {
            alert('Select Team Participants');
            return false;
        }
        if (!$('.teamleadradio').is(':checked')) {
            alert('Select Team Lead');
            return false;
        }
        var participants = [];
        $.each($('.participantcheckboxes'), function (index, member) {
            if ($(member).is(':checked')) {
                var obj = {
                    'T_CODE': '',
                    'T_NAME':teamName,
                    'PPNO': $(member).attr('memberid'),
                    'NAME': $(member).attr('memberfname') +" "+$(member).attr('memberlname'),
                    'PLACEOFPOSTING': $(member).attr('memberplaceofposting'),
                    'ISTEAMLEAD': 'N',
                    'STATUS': 'Y',
                }
                participants.push(obj);
            }
        });
        var teamLeadID = 0;
        var teamLeadCheck = false;

        $.each($('.teamleadradio'), function (index, lead) {
            if ($(lead).is(':checked')) {
                teamLeadID = $(lead).attr('memberid');                
            }
        });
        $.each(participants, function (i, p) {
            if (p.PPNO == teamLeadID) {
                p.ISTEAMLEAD = 'Y'
                teamLeadCheck = 1;
            }
        });
        if (!teamLeadCheck) {
            alert('Select Team Lead among Team Participants');
            return false;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/AddAuditTeam",
            type: "POST",
            data: {
                'AUDIT_TEAM': participants
            },
            cache: false,
            success: function (data) {
                reloadLocation();
            },
            dataType: "json",
        });
        

    }
    function deleteTeam(teamCode) {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/DeleteAuditTeam",
            type: "POST",
            data: {
                'T_CODE': teamCode
            },
            cache: false,
            success: function (data) {
                reloadLocation();
            },
            dataType: "json",
        });


    }

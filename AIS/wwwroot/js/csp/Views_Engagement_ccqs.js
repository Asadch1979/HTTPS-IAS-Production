    $(document).ready(function () {
        ShowEmployeeContainer();
        $("#searchTeamFormation").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfEmployeeTeam tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });

    });

    function ShowEmployeeContainer() {
        //console.log($('#deptSelectionBox option:selected').val());
        if ($('#deptSelectionBox option:selected').val() == 0)
            $('#listOfEmployeeTeam tbody').empty();
        else {
            $('#listOfEmployeeTeam tbody').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetAuditTeam",
                type: "POST",
                data: {
                    'dept_code': $('#deptSelectionBox option:selected').val()
                },
                cache: false,
                success: function (data) {
                    //console.log(data);
                    var teamId = 0;
                    var srNo = 1;
                    var teamMembers = [];
                    $.each(data, function (index, team) {

                        if (team.iS_TEAMLEAD == "Y") {
                            $('#listOfEmployeeTeam tbody').append('<tr id=teamcode_' + team.code + '><td class="searchable"><p class="fw-normal mb-1">' + srNo + '</p></td><td class="searchable"><p class="fw-normal mb-1">' + team.name + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + team.employeename + ' </p></td><td class="empMembers"></td><td> <small data-click="event.preventDefault();deleteTeam(\'' + team.code + '\');" class="text-danger deleteTeam">Delete</small></td></tr>');
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
                            $('#listOfEmployeeTeam tbody #teamcode_' + team.code + ' .empMembers').text(prevText + team.employeename);
                        }
                    });
                },
                dataType: "json",
            });
        }
    }
    function addccq() {
        $('#teamNameModalField').val('');
        $('#teamCodeModalField').val('');
        $.each($('.participantcheckboxes'), function (index, member) { $(member).attr('checked', false); });
        $.each($('.teamleadradio'), function (index, member) { $(member).attr('checked', false); });

        $('#setupAuditTeam').modal('show');
    }
    function publishNewTeamChanges() {


        var teamCode = $('#teamCodeModalField').val()
        if (teamCode == '') {
            alert('Enter Sr. No.');
            return false;
        }
        var Entity = $('#EntityModalField').val();

        if (Entity == '') {
            alert('Enter Entity');
            return false;
        }
        var Question = $('#QuestionModalField').val();
        if (Question == '') {
            alert('Enter Question');
            return false;
        }
        var ControlViolation = $('#ControlViolationModalField').val();
                if (ControlViolation == '') {
            alert('Enter Control Violation');
            return false;
        }
        var Risk = $('#RiskModalField').val();
                if (Risk == '') {
            alert('Enter Risk');
            return false;
        }
                var Status = $('#StatusModalField').val();
                if (Status == '') {
            alert('Enter Status');
            return false;
        }
        ;





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
                window.location = window.location.pathname;
            },
            dataType: "json",
        });


    }

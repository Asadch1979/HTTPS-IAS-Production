    var g_teams = [];
    var g_branches = [];
    var g_code = 0;
    var g_planId = 0;
    var g_periodId = 0;
    var g_entityType = 0;
    var g_zoneId = 0;
    var g_entityId = 0;
    var g_entityName = '';
    var g_public_days = []; // Will be filled from DB as MM-DD


     function loadPublicDays(year, cb) {
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_all_public_holidays',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ year: year }),
            success: function (data) {
                data.forEach(function (item) {
                    var dateObj = new Date(item.holidaY_DATE);
                    // Convert to MM-DD for comparison
                    var mmdd = ("0" + (dateObj.getMonth() + 1)).slice(-2) + '-' + ("0" + dateObj.getDate()).slice(-2);
                    g_public_days.push(mmdd);
                });
                if (cb) cb();
            }
        });
    }

        // Helper: Check if a date is a public day (from DB)
    function isPublicDay(dateObj) {
        var mmdd = ("0" + (dateObj.getMonth() + 1)).slice(-2) + '-' + ("0" + dateObj.getDate()).slice(-2);
        return g_public_days.indexOf(mmdd) !== -1;
    }



    function readPlanningContextValue(url, name, hiddenId) {
        var hidden = document.getElementById(hiddenId);
        if (hidden && hidden.value) {
            return hidden.value;
        }
        return url.searchParams.get(name);
    }

    $(document).ready(function () {
         var url = new URL(window.location.href);
         var periodName = readPlanningContextValue(url, "period", "planningPeriodName");
         var risk = readPlanningContextValue(url, "risk", "planningRisk");
         var size = readPlanningContextValue(url, "size", "planningSize");
         var entityName = readPlanningContextValue(url, "name", "planningEntityName");
         g_entityName = entityName;
         var freq = readPlanningContextValue(url, "freq", "planningFrequency");
         var days = readPlanningContextValue(url, "days", "planningDays");
         g_planId = readPlanningContextValue(url, "planId", "planningPlanId");
         g_code = readPlanningContextValue(url, "code", "planningCode");
         g_periodId = readPlanningContextValue(url, "periodId", "planningPeriodId");
         g_entityType = readPlanningContextValue(url, "entityType", "planningEntityType");
         g_zoneId = readPlanningContextValue(url, "zoneId", "planningZoneId");
         g_entityId = readPlanningContextValue(url, "entityId", "planningEntityId");

        // Public holidays will be fetched on demand when calculating end date

         if (g_entityType == 25 || g_entityType == 6|| g_entityType == 28) {
             $('#travellingDayWrapper').removeClass('d-none');
             $('#revenueCollWrapper').removeClass('d-none');
             $('#discussionDayWrapper').removeClass('d-none');
         } else {
             $('#travellingDayWrapper').addClass('d-none');
             $('#revenueCollWrapper').addClass('d-none');
             $('#discussionDayWrapper').addClass('d-none');
         }

         $('#auditperiod_label').text(periodName);
         $('#auditentity_label').text(entityName);
         $('#risk_label').text(risk);
         $('#size_label').text(size);
         $('#frequency_label').text(freq);
         $('#days_label').text(days);
         $('#btnCalcEndDate').on('click', calculateEndDate);
     });

    function isWeekend(dateObj) {
        var saturdayWorking = [5, 6, 17, 21, 22, 25];
        if (saturdayWorking.includes(parseInt(g_entityType)))
            return dateObj.getDay() === 0;
        return dateObj.getDay() === 6 || dateObj.getDay() === 0;
    }

    function calculateEndDate() {
        var startVal = $('#startplan_date').val();
        if (!startVal) {
            alert('Select Audit Start Date');
            return;
        }
        var year = new Date(startVal).getFullYear();
        g_public_days = [];
        loadPublicDays(year, function () {
            loadPublicDays(year + 1, getAutoEndDate);
        });
    }

    function getAutoEndDate() {
        var numAdd = parseInt($('#days_label').text());
        if (g_entityType == 25 || g_entityType == 6 || g_entityType == 28) {
            var addDays = parseInt($('#discussionDaysSelectField').val()) + parseInt($('#revenueDaysSelectField').val()) + parseInt($('#travellingDaysSelectField').val());
            numAdd = parseInt(numAdd) + addDays;
        }

        var curDate = new Date($('#startplan_date').val());
        if (isNaN(curDate)) return;

        if (isWeekend(curDate) || isPublicDay(curDate)) {
            alert('Start date cannot be a weekend or public holiday');
            return;
        }

        var added = 1;
        while (added < numAdd) {
            curDate.setDate(curDate.getDate() + 1);
            if (isWeekend(curDate) || isPublicDay(curDate)) {
                continue;
            }
            added++;
        }

        // Set End Date field
        var month = (curDate.getMonth() + 1);
        var day = curDate.getDate();
        if (month < 10)
            month = "0" + month;
        if (day < 10)
            day = "0" + day;
        var today = curDate.getFullYear() + '-' + month + '-' + day;
        $('#endplan_date').val(today);

        // Set operational period end date (last day of previous month)
        var startVal = $('#startplan_date').val();
        if (!startVal) return;
        var date = new Date(startVal);
        const lastDayPrevMonth = new Date(date.getFullYear(), date.getMonth(), 0);
        var month2 = (lastDayPrevMonth.getMonth() + 1);
        var day2 = lastDayPrevMonth.getDate();
        if (month2 < 10)
            month2 = "0" + month2;
        if (day2 < 10)
            day2 = "0" + day2;
        var prevMonthDay = lastDayPrevMonth.getFullYear() + '-' + month2 + '-' + day2;
        $('#endop_date').val(prevMonthDay);

        // Fetch operational start date from server as before
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetOperationalStartDate",
            type: "POST",
            data: {
                'periodId': g_periodId,
                'entityCode': g_code
            },
            cache: false,
            success: function (data) {
                if (data != "" && data.length > 0) {
                    var date = new Date(data);
                    date.setDate(date.getDate() + 1)
                    var month = (date.getMonth() + 1);
                    var day = date.getDate();
                    if (month < 10)
                        month = "0" + month;
                    if (day < 10)
                        day = "0" + day;
                    var today = date.getFullYear() + '-' + month + '-' + day;
                    $('#startop_date').val(today);
                }
            }
        });
    }
   
    function previewAuditPlan() {

        $('#previewAuditPlan').modal('show');
        $('#auditorDept').text($('#deptSelectionBox option:selected').text());
        $('#auditorPlan').text($('#periodSelectionBox option:selected').text());
        $('#descModal_field').html($('#entitySelectionBox option:selected').text());
        if ($('#deptSelectionBox option:selected').val() == '473') {
            $('#divzone_field').text($('#auditZoneSelectionBox option:selected').text());
            $('#deptBranch_field').text($('#branchSelectionBox option:selected').text());

        } else {
            $('#divzone_field').text($('#divSelectionBox option:selected').text());
            $('#deptBranch_field').text($('#divDeptSelectionBox option:selected').text());

        }
        $('#exeFrom_field').html($('#executionPeriodFromField').val());
        $('#exeTo_field').html($('#executionPeriodToField').val());
        $('#operationalFrom_field').html($('#auditPeriodFromField').val());
        $('#operationalTo_field').html($('#auditPeriodToField').val());
        //
        if ($('#isTravelingRequiredField').is(":checked"))
            $('#travelingReq_field').html('Yes');
        else
            $('#travelingReq_field').html('No');
        $('#remarksAddtn_field').html($('#remarksAdditionalInfoField').val());
        $('#teamName_field').text($('#teamSelectionBox option:selected').text());
        //
        var teamMembersFields = "";
        $.each(g_teams, function (index, team) {
            if (team.name == $('#teamSelectionBox option:selected').text()) {
                if (team.iS_TEAMLEAD == "Y")
                    teamMembersFields += team.employeename + " " + team.teammembeR_ID + " (L)<br>";
                else
                    teamMembersFields += team.employeename + " " + team.teammembeR_ID + " (M)<br>";
            }
        });
        $('#teamMembers_field').html(teamMembersFields);

    }
    function publishNewAuditPlanChanges() {

        if ($('#auditTeam_box').val() == 0) {
            alert('Select audit team');
            return;
        }


         if ($('#startop_date').val() == '') {
            alert('Select Operational Start Date');
            return;
        }

        if ($('#endop_date').val() == '') {
            alert('Select Operational End Date');
            return;
        }

        if ($('#startplan_date').val() == '') {
            alert('Select Audit Start Date');
            return;
        }
        if ($('#endplan_date').val() == '') {
            alert('Select Audit End Date');
            return;
        }


        var status = 1;
        var desc = $('#descriptionAuditPlanField').val();


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/AddEngagementPlan",
            type: "POST",
            data: {
                'PERIOD_ID': g_periodId,
                'ENTITY_TYPE': g_entityType,
                'PLAN_ID': g_planId,
                'ENTITY_CODE': g_code,
                'ENTITY_ID': g_entityId,
                'ENTITY_NAME': g_entityName,
                'AUDITBY_ID': g_zoneId,
                'AUDIT_STARTDATE': $('#startplan_date').val(),
                'AUDIT_ENDDATE': $('#endplan_date').val(),
                'OP_STARTDATE': $('#startop_date').val(),
                'OP_ENDDATE': $('#endop_date').val(),
                'TRAVELDAY': $('#travellingDaysSelectField').val(),
                'RRDAY': $('#revenueDaysSelectField').val(),
                'D_Day': $('#discussionDaysSelectField').val(),
                'TEAM_NAME': $('#auditTeam_box option:selected').text(),
                'STATUS': 1,
                'TEAM_ID': $('#auditTeam_box').val(),                
            },
            cache: false,
            success: function (data) {
                alert(data.remarkS_OUT);
                if(data.iS_SUCCESS=="Yes")
                    onAlertCallback(redirectToLocation);
                
            },
            dataType: "json",
        });
    }
    function redirectToLocation() {
        if (window.planningDashboard && typeof window.planningDashboard.loadStep === 'function') {
            window.planningDashboard.loadStep('AUDIT_PLAN', '5');
        }
    }

    function previewSelectedTeaM() {
        if ($('#auditTeam_box').val() == 0) {
            $('#teamPreview').addClass('d-none');
            $('#listOfEmployeeTeam tbody').empty();
        } else {
            $('#teamPreview').removeClass('d-none');
            $('#listOfEmployeeTeam tbody').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetAuditTeam",
                type: "POST",
                data: {
                   
                },
                cache: false,
                success: function (data) {
                    var teamId = 0;
                    var teamMembers = [];
                    $.each(data, function (index, team) {
                        if (team.code == $('#auditTeam_box').val()) {

                            if (team.iS_TEAMLEAD == "Y") {
                                $('#listOfEmployeeTeam tbody').append('<tr id=teamcode_' + team.code + '><td class="searchable"><p class="fw-normal mb-1">' + team.name + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + team.employeename + ' (' + team.teammembeR_ID + ') </p></td><td class="empMembers"></td></tr>');
                             } else {
                                teamMembers.push(team);
                                if (team.code != teamId) {
                                    teamId = team.code;
                                }
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
        

    }

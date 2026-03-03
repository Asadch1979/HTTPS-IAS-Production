    $(document).ready(function () {
        $('#sidebar_policy').hide();
        $('#listOfAuditPeriodsContainer').hide();
    });
    function ShowDepartmentAuditPeriods() {
        if ($('#deptSelectionBox option:selected').val() == 0)
            $('#listOfAuditPeriodsContainer').hide();
        else {
            $('#listOfAuditPeriodsContainer').show();

            $.ajax({
                url: g_asiBaseURL + "/Planning/audit_periods",
                type: "POST",
                data: {
                    'dept_code': $('#deptSelectionBox option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $('#listOfAuditPeriods tbody').empty();
                    //console.log(data);
                    var srNo = 1;
                    $.each(data, function (index, period) {
                        var status = 'Approved';
                        period.starT_DATE = period.starT_DATE.split('T')[0];
                        period.enD_DATE = period.enD_DATE.split('T')[0];
                        $('#listOfAuditPeriods tbody').append('<tr id=teamcode_' + period.id + '><td class="searchable"><p class="fw-normal mb-1">' + srNo + '</p></td><td class="searchable"><p class="fw-normal mb-1">' + period.description + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.starT_DATE + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.enD_DATE + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + status + ' </p></td><td><small onclick="event.preventDefault();getAuditPlan(' + period.id + ')" class="text-danger planDetails">Details</small></td></tr>');
                        srNo++;
                    });

                },
                dataType: "json",
            });
        }
    }
    function setupNewAuditPeriod() {
        $('#setupAuditPeriod').modal('show');
        $('#periodDescriptionModalField').val('');
        $('#periodStartModalField').val('');
        $('#periodEndModalField').val('');
        $.each($('.deptCheckBoxes'), function (index, chkbox) {
            if ($(chkbox).is(':checked')) {
                $(chkbox).click();
            }
        });

    }
    function publishNewAuditPeriodChanges() {
        var desc=$('#periodDescriptionModalField').val();
        var stDate=$('#periodStartModalField').val();
        var edDate = $('#periodEndModalField').val();
        var deptIds = [];
        $.each($('.deptCheckBoxes'), function (index, chkbox) {
            if ($(chkbox).is(':checked')) {
                deptIds.push($(chkbox).attr('dept-id'));
            }
        });
        if (desc == "" || stDate == "" || edDate == "" || deptIds.length == 0) {
            alert("Missing Mandatory fields");
            return;
        }

        //var date = '21/01/2015';
        var ds = new Date(stDate.split("/").reverse().join("-"));
        var dd = ds.getDate();
        var mm = ds.getMonth() + 1;
        var yy = ds.getFullYear();
        var startDate = ("0" + mm).slice(-2) + "/" + dd + "/" + yy;

        var de = new Date(edDate.split("/").reverse().join("-"));
        var dd = de.getDate();
        var mm = de.getMonth() + 1;
        var yy = de.getFullYear();
        var endDate = ("0" + mm).slice(-2) + "/" + dd + "/" + yy;
        //console.log(desc, stDate, edDate, deptIds);
        $.ajax({
            url: g_asiBaseURL + "/Planning/add_audit_period",
            type: "POST",
            data: {
                'DESCRIPTION': desc,
                'STARTDATE': startDate,
                'ENDDATE': endDate,
                'DEPARTMENT_IDS': deptIds
            },
            cache: false,
            success: function (data) {
                location.reload();
            },
            dataType: "json",
        });
    }
    function getAuditPlan(periodId) {
        window.location.href = g_asiBaseURL + "/Planning/audit_plan?dept=" + $('#deptSelectionBox option:selected').val() + "&periodId=" + periodId;
    }

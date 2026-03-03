    $(document).ready(function () {
        $('#sidebar_policy').hide();
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
                        $('#listOfAuditPeriods tbody').append('<tr id=teamcode_' + period.id + '><td class="searchable"><p class="fw-normal mb-1">' + srNo + '</p></td><td class="searchable"><p class="fw-normal mb-1">' + period.description + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.starT_DATE + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.enD_DATE + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + status + ' </p></td><td><small onclick="event.preventDefault();getAuditPlan(' + period.id + ')" class="text-danger deleteTeam">Details</small></td></tr>');
                        srNo++;
                    });

                },
                dataType: "json",
            });
        }
    }
    function setupNewAuditPlanItem() {
        var url = window.location.href;
        var newUrl = url.replace('audit_plan', 'tentative_audit_plan');
        window.location.href = newUrl;
    }

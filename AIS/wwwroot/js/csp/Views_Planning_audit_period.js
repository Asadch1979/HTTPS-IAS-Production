    var g_periodId = 0;
    $(document).ready(function () {
        ShowDepartmentAuditPeriods();
    });
    function ShowDepartmentAuditPeriods() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/audit_periods",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {
                $('#listOfAuditPeriods tbody').empty();
                //console.log(data);
                var srNo = 1;
                $.each(data, function (index, period) {
                    
                    period.starT_DATE = period.starT_DATE.split('T')[0];
                    period.enD_DATE = period.enD_DATE.split('T')[0];
                    $('#listOfAuditPeriods tbody').append('<tr id=teamcode_' + period.auditperiodid + '><td class="searchable"><p class="fw-normal mb-1">' + srNo + '</p></td><td class="searchable"><p class="fw-normal mb-1">' + period.description + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.starT_DATE + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.enD_DATE + ' </p></td><td class="searchable"><p class="empName fw-normal mb-1">' + period.status + ' </p></td><td><small data-onclick="event.preventDefault();updateAuditPeriod(' + period.auditperiodid + ',' + period.statuS_ID + ')" class="text-danger cursor-pointer">Update</small></td></tr>');
                    srNo++;
                });

            },
            dataType: "json",
        });

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

    function updateAuditPeriod(periodId, statusId) {
        $('#updateAuditPeriodModel').modal('show');
        $('#auditPeriodStatus_forUpdate').val(statusId);
        g_periodId = periodId;


    }
    function reloadLocation() {
        location.reload();
    }
    function publishNewAuditPeriodChanges() {
        var desc = $('#periodDescriptionModalField').val();
        if (document.querySelectorAll('input.alnum-only.is-invalid').length > 0) {
            Swal.fire({ icon: "error", title: "Validation error", text: "Please correct highlighted fields." });
            return;
        }
        var stDate = $('#periodStartModalField').val();
        var edDate = $('#periodEndModalField').val();
        var deptIds = [];
        $.each($('.deptCheckBoxes'), function (index, chkbox) {
            if ($(chkbox).is(':checked')) {
                deptIds.push($(chkbox).attr('dept-id'));
            }
        });
        if (desc == "" || stDate == "" || edDate == "") {
            alert("Missing Mandatory fields");
            return;
        }

        //var date = '21/01/2015';
        var ds = new Date(stDate.split("/").reverse().join("-"));
        var dd = ds.getDate();
        var mm = ds.getMonth() + 1;
        var yy = ds.getFullYear();
        var startDate = ("0" + mm).slice(-2) + "/" + ("0" + dd).slice(-2) + "/" + yy;

        var de = new Date(edDate.split("/").reverse().join("-"));
        var dd = de.getDate();
        var mm = de.getMonth() + 1;
        var yy = de.getFullYear();
        var endDate = ("0" + mm).slice(-2) + "/" + ("0" + dd).slice(-2) + "/" + yy;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_audit_period",
            type: "POST",
            data: {
                'DESCRIPTION': desc,
                'STARTDATE': startDate,
                'ENDDATE': endDate,
                'STATUS_ID': 1,
                'DEPARTMENT_IDS': deptIds
            },
            cache: false,
            success: function (data) {

                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function publishUpdateAuditPeriodChanges() {
        if ($('#auditPeriodStatus_forUpdate').val() == 0) {
            alert("Please select Audit Period to proceed");
            return;
        }     

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_audit_period",
            type: "POST",
            data: {
                'AUDITPERIODID': g_periodId,
                'STATUS_ID': $('#auditPeriodStatus_forUpdate').val(),
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function getAuditPlan(periodId) {
        window.location.href = g_asiBaseURL + "/Planning/tentative_audit_plan?dept=" + $('#deptSelectionBox option:selected').val() + "&periodId=" + periodId;
    }

    var g_comKey = 0;
    var g_revPP = 0;
    var g_authPP = 0;
    var g_entityID = 0;
    $(document).ready(function () {
        getComplianceHierarchy();
    });

    function getComplianceHierarchy() {
        $('#listOfComplianceHierarchy tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_compliance_hierarchy",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#listOfComplianceHierarchy tbody').append('<tr><td>' + ++i + '</td><td>' + v.compliancE_UNIT + '</td><td>' + v.approveR_NAME + ' ( ' + v.approveR_PPNO + ')</td><td>' + v.revieweR_NAME + ' ( ' + v.revieweR_PPNO + ')</td><td><a href="#" onclick="event.preventDefault();updateComplianceHierarchy(' + v.entitY_ID + ', ' + v.revieweR_PPNO + ', ' + v.approveR_PPNO + ', \'' + v.coM_KEY + '\', \'' + v.revieweR_NAME + '\', \'' + v.approveR_NAME + '\');">Update</a></td></tr>');

                });

            },
            dataType: "json",
        });

    }
    function setupNewComplianceHierarchy(){
        $('#addNewcomplianceUnit').modal('show');
        $('#addNewComplianceUnitsDropDown').val('0');
        $('#addnew_settlementPPNo').val('');
        $('#addnew_inchargePPNo').val('');
        $('#addnew_inchargeEmpName').html();
        $('#addnew_settlementEmpName').html();
    }
    function updateComplianceHierarchy(entID, revPP, authPP, comKey, revName, authName) {
        g_revPP = revPP;
        g_entityID = entID;
        g_authPP = authPP;
        g_comKey = comKey;
        $('#complianceUnit').modal('show');
        $('#inchargePPNo').val(authPP);
        $('#settlementPPNo').val(revPP);
        $('#inchargeEmpName').html(authName + ' (' + authPP + ')');
        $('#settlementEmpName').html(revName + ' (' + revPP + ')');

    }
    function publishAddNewComplianceHierarchy() {

        if ($('#addNewComplianceUnitsDropDown').val() == "0") {
            alert("Please select Compliance Unit to proceed");
            return;
        }


        if ($('#addnew_inchargePPNo').val() == "") {
            alert("Please enter Incharge Compliance Unit PP No. to proceed");
            return;
        }

        if ($('#addnew_settlementPPNo').val() == "") {
            alert("Please enter Settlement Officer PP No. to proceed");
            return;
        }     


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_compliance_hierarchy",
            type: "POST",
            data: {
                'ENTITY_ID': $('#addNewComplianceUnitsDropDown').val(),
                'REVIEWER_PP': $('#addnew_settlementPPNo').val(),
                'AUTHORIZER_PP': $('#addnew_inchargePPNo').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function publishUpdateComplianceHierarchy() {

        if ($('#inchargePPNo').val() == "") {
            alert("Please enter Incharge Compliance Unit PP No. to proceed");
            return;
        }
        if ($('#settlementPPNo').val() == "") {
            alert("Please enter Settlement Officer PP No. to proceed");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_compliance_hierarchy",
            type: "POST",
            data: {
                'ENTITY_ID': g_entityID,
                'REVIEWER_PP': $('#settlementPPNo').val(),
                'AUTHORIZER_PP': $('#inchargePPNo').val(),
                'COMPLIANCE_KEY': g_comKey,
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function normalizeRequiredInt(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return 0;
        }
        var number = parseInt(trimmed, 10);
        return Number.isNaN(number) ? 0 : number;
    }

    function getMatchedPP(ppno, labelId) {
        $('#matchedPPNoPanels').empty();
        g_respUser = [];
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_employee_name_from_pp",
            type: "POST",
            data: {
                'PP_NO': normalizeRequiredInt(ppno)
            },
            cache: false,
            success: function (data) {
                g_respUser.push(data);
                if (data.ppNumber > 0) {
                    $('#' + labelId).html(data.name + ' (' + data.ppNumber + ')');
                }
                else
                    $('#' + labelId).html('No record found..');
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#complianceUnit').modal('hide');
        $('#addNewcomplianceUnit').modal('hide');
        getComplianceHierarchy();
    }

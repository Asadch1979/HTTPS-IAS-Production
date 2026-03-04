window.addEventListener("error", function (e) {
    console.error("JS error:", e.message, e.filename, e.lineno, e.colno);
});
window.addEventListener("unhandledrejection", function (e) {
    console.error("Promise rejection:", e.reason);
});

function getPageData() {
    var el = document.getElementById("page-data");
    if (!el) return {};
    try { return JSON.parse(el.textContent || "{}"); } catch (error) {
        console.error("Invalid #page-data JSON", error);
        return {};
    }
}

    var pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];
    var g_procId = 0;
    var g_annexId = 0;
    var g_subProcList = [];
    $(document).ready(function () {
        console.log("Loaded Views_Setup_manage_annexure.js JS");
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#auditeeEntitiesList tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
    /*    function getAnnexures() {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_annexures",
                type: "POST",
                data: {

                },
                cache: false,
                success: function (data) {
                    $('#annexureContainerGrid tbody').empty();
                    var mxTotal = 0;
                    var gravTotal = 0;
                    var weightTotal = 0;
                    $.each(data, function (i, v) {
                        if (v.gravity != "")
                            gravTotal += v.gravity;
                        if (v.maX_NUMBER != "")
                            mxTotal += v.maX_NUMBER;
                        if (v.weightage != "")
                            weightTotal += v.weightage;
                        $('#annexureContainerGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.process + '</td><td>' + v.code + '</td><td>' + v.heading + '</td><td>' + v.functioN_OWNER + '</td><td>' + v.risk + '</td><td>' + v.maX_NUMBER + '</td><td>' + v.weightage + '</td><td>' + v.gravity + '</td><td>  <a class="text-danger" onclick="openUpdateAnnexure( '+v.id+', '+v.risK_ID+', '+v.risK_MODEL_ID+', '+v.procesS_ID +', '+ v.functioN_OWNER_ID +', '+ v.heading+', '+ v.code+'   'item.RISK_ID'', 'item.RISK_MODEL_ID', 'item.PROCESS_ID','item.FUNCTION_OWNER_ID', 'item.HEADING', 'item.CODE', 'item.MAX_NUMBER', 'item.WEIGHTAGE', 'item.GRAVITY' )">Update</a></td></tr>');
                    })

                },
                dataType: "json",
            });
        }
        */
    function openUpdateAnnexure(id) {
        g_annexId = id;
        $('#updateAnnexureModel').modal('show');
        $.each(g_annexList, function (i,v) { 
            if (id == v.id) {
                $('#annexProcField').val(v.procesS_ID);
                $('#annexFuncOwnerField').val(v.functioN_OWNER_ID);
                $('#annexCoFuncField1').val(v.functioN_ID_1);
                $('#annexCoFuncField2').val(v.functioN_ID_2);
                $('#annexCodeField').val(v.code);
                $('#annexRiskField').val(v.risK_ID);
                $('#annexHeadingField').val(v.heading);
                $('#annexMaxNumberField').val(v.maX_NUMBER);
                $('#annexGravityField').val(v.gravity);
                $('#annexWeightageField').val(v.weightage);
                $('#annexCodeField').attr("disabled", true);
            }        
        });
    }
    function reloadLocation() {
        window.location.reload();

    }

    function updateAnnexure() {

        if (document.querySelectorAll('input.alnum-only.is-invalid').length > 0) {
            Swal.fire({ icon: "error", title: "Validation error", text: "Please correct highlighted fields." });
            return;
        }

        if ($('#annexProcField').val() == "") {
            alert("Please select Process  to proceed");
            return;
        }

        if ($('#annexFuncOwnerField').val() == "") {
            alert("Please select Function Owner to proceed");
            return;
        }

        if ($('#annexRiskField').val() == "0") {
            alert("Please select Risk to proceed");
            return;
        }

        if ($('#annexHeadingField').val() == "") {
            alert("Please enter Heading to proceed");
            return;
        }

        if ($('#annexMaxNumberField').val() == "") {
            alert("Please enter Max Number to proceed");
            return;
        }


        if ($('#annexGravityField').val() == "") {
            alert("Please enter Gravity to proceed");
            return;
        }


        if ($('#annexWeightageField').val() == "") {
            alert("Please enter Weightage to proceed");
            return;
        }

        if (g_annexId != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_annexure",
                type: "POST",
                data: {
                    'ANNEX_ID': g_annexId,
                    'PROCESS_ID': $('#annexProcField').val(),
                    'FUNCTION_OWNER_ID': $('#annexFuncOwnerField').val(),
                    'FUNCTION_ID_1': $('#annexCoFuncField1').val(),
                    'FUNCTION_ID_2': $('#annexCoFuncField2').val(),
                    'RISK_ID': $('#annexRiskField').val(),
                    'MAX_NUMBER': $('#annexMaxNumberField').val(),
                    'WEIGHTAGE': $('#annexWeightageField').val(),
                    'GRAVITY': $('#annexGravityField').val(),
                    'HEADING': $('#annexHeadingField').val()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        } else {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_annexure",
                type: "POST",
                data: {
                    'ANNEX_CODE': $('#annexCodeField').val(),
                    'PROCESS_ID': $('#annexProcField').val(),
                    'FUNCTION_OWNER_ID': $('#annexFuncOwnerField').val(),
                    'FUNCTION_ID_1': $('#annexCoFuncField1').val(),
                    'FUNCTION_ID_2': $('#annexCoFuncField2').val(),
                    'RISK_ID': $('#annexRiskField').val(),
                    'MAX_NUMBER': $('#annexMaxNumberField').val(),
                    'WEIGHTAGE': $('#annexWeightageField').val(),
                    'GRAVITY': $('#annexGravityField').val(),
                    'HEADING': $('#annexHeadingField').val()
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }

    }

    function addNewAnnexure(id) {
        g_annexId = id;
        $('#updateAnnexureModel').modal('show');
        $('#annexProcField').val('');
        $('#annexFuncOwnerField').val('');
        $('#annexCoFuncField1').val('');
        $('#annexCoFuncField2').val('');
        $('#annexRiskField').val(0);
        $('#annexHeadingField').val('');
        $('#annexCodeField').val('');
        $('#annexMaxNumberField').val('');
        $('#annexGravityField').val('');
        $('#annexWeightageField').val('');
        $('#annexCodeField').attr("disabled", false);
    }

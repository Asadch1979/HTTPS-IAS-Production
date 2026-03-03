    $(document).ready(function () {
    })
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
    function getStatusWiseCompliance() {
        var aud_id = 0;
        var R_C = "N";
        if ($('#controlingsearch').val() != 0) {

            aud_id = $('#controlingsearch').val();
            R_C = "P";
        }

        if ($('#childposting').val() != 0) {
            aud_id = $('#childposting').val();
            R_C = "C";
        }
        var s_d = "";
        var e_d = "";
        if ($('#startDateField').val() != "") {
            var stDate = $('#startDateField').val();
            var ds = new Date(stDate.split("/").reverse().join("-"));
            var dd = ds.getDate();
            var mm = ds.getMonth() + 1;
            var yy = ds.getFullYear();
            s_d = ("0" + mm).slice(-2) + "/" + ("0" + dd).slice(-2) + "/" + yy;
        } else {
            alert("Select Start Date to proceed");
            return false;
        }
        if ($('#endDateField').val() != "") {
            var edDate = $('#endDateField').val();
            var de = new Date(edDate.split("/").reverse().join("-"));
            var dd = de.getDate();
            var mm = de.getMonth() + 1;
            var yy = de.getFullYear();
            e_d = ("0" + mm).slice(-2) + "/" + ("0" + dd).slice(-2) + "/" + yy;
        }
        else {
            alert("Select End Date to proceed");
            return false;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_status_wise_compliance",
            type: "POST",
            data: {
                'AUDITEE_ID': aud_id,
                'START_DATE': s_d,
                'END_DATE': e_d,
                'RELATION_CHECK': R_C
            },
            cache: false,
            success: function (data) {
                $('#statusWiseComplianceTable tbody').empty();
                var Complaince_Submitted = 0;
                var Complaince_received_at_Incharge_implementation = 0;
                var Referredback_by_Controlling_office = 0;
                var Complaince_Submitted_To_Incharge_AZ = 0;
                var Complaince_Referred_back_by_Incharge_Implementation = 0;
                var Para_settled_by_Incharge_AZ = 0;
                var Complaince_Referred_back_by_Incharge_AZ = 0;
                $.each(data, function (i, v) {
                    Complaince_Submitted += parseInt(v.complaince_Submitted);
                    Complaince_received_at_Incharge_implementation += parseInt(v.complaince_received_at_Incharge_implementation);
                    Referredback_by_Controlling_office += parseInt(v.referredback_by_Controlling_office);
                    Complaince_Submitted_To_Incharge_AZ += parseInt(v.complaince_Submitted_To_Incharge_AZ);
                    Complaince_Referred_back_by_Incharge_Implementation += parseInt(v.complaince_Referred_back_by_Incharge_Implementation);
                    Para_settled_by_Incharge_AZ += parseInt(v.para_settled_by_Incharge_AZ);
                    Complaince_Referred_back_by_Incharge_AZ += parseInt(v.complaince_Referred_back_by_Incharge_AZ);
                    $('#statusWiseComplianceTable tbody').append('<tr><td>' + ++i + '</td><td>' + v.parent_Office + '</td><td>' + v.entity_name + '</td><td>' + v.complaince_Submitted + '</td><td>' + v.complaince_received_at_Incharge_implementation + '</td><td>' + v.referredback_by_Controlling_office + '</td><td>' + v.complaince_Submitted_To_Incharge_AZ + '</td><td>' + v.complaince_Referred_back_by_Incharge_Implementation + '</td><td>' + v.para_settled_by_Incharge_AZ + '</td><td>' + v.complaince_Referred_back_by_Incharge_AZ + '</td></tr>');
                });

                $('#statusWiseComplianceTable tbody').append('<tr><td colspan="3">Total</td><td>' + Complaince_Submitted + '</td><td>' + Complaince_received_at_Incharge_implementation + '</td><td>' + Referredback_by_Controlling_office + '</td><td>' + Complaince_Submitted_To_Incharge_AZ + '</td><td>' + Complaince_Referred_back_by_Incharge_Implementation + '</td><td>' + Para_settled_by_Incharge_AZ + '</td><td>' + Complaince_Referred_back_by_Incharge_AZ + '</td></tr>');
            },
            dataType: "json",
        });
    }

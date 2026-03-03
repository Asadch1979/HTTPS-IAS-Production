    $(document).ready(function () {

    });

    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting tbody').empty();
        $('#complianceUnitArea').addClass('d-none');
        $('#AZOfficeArea').addClass('d-none');
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

            },
            dataType: "json",
        });



    }
    function getplacepost(userEntityId = 0) {
        $('#childposting tbody').empty();
        $('#complianceUnitArea').addClass('d-none');
        $('#AZOfficeArea').addClass('d-none');

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: {
                'E_R_ID': $('#controlingsearch option:selected').val()
            },


            cache: false,
            success: function (data) {
                $.each(data, function (index, gpp) {

                    $('#childposting tbody').append('<tr><td>' + ++index + '</td><td>' + gpp.c_NAME + '</td><td>' + gpp.complicE_BY + '</td><td>' + gpp.audiT_BY + '</td><td class="text-center"><input type="checkbox" class="selectallMini" entId="' + gpp.entitY_ID + '" name="selectall" /></td></tr>');
                });
                if (data.length > 0) {
                    $('#complianceUnitArea').removeClass('d-none');
                    $('#AZOfficeArea').removeClass('d-none');
                }
                    
            },
            dataType: "json",
        });


    }
    function updateComplianceOffice() {

        var entIdArr = [];
        $.each($('.selectallMini'), function (i, v) {
            if ($(v).is(':checked')) {
                entIdArr.push($(v).attr("entId"));
            }
        });

        if (entIdArr.length == 0) {
            alert("please select entity to proceed");
            return false;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_compliance_office",
            type: "POST",
            data: {
                'COMP_ID': $('#ComplianceUnitField option:selected').val(),
                'AUD_ID': $('#AZOfficeField option:selected').val(),
                'ENT_ID_ARR': entIdArr
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                //onAlertCallback(reloadToLocation);

            },
            dataType: "json",
        });
    }

    function selectAllCheckboxes() {

        if ($('#selectAllMainBox').is(':checked')) {
            $('.selectallMini').prop("checked", true);
        }
        else {
            $('.selectallMini').prop("checked", false)
        }
    }
    function reloadToLocation() {

    }

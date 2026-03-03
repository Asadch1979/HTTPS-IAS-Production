    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrelForDashboardPanel",
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
            url: g_asiBaseURL + "/ApiCalls/getpostplaceForDashboardPanel",
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

    }


    function getObservations() {
        
        destroyDatatable('entitywise_panel');
        var entId = 0;
        if ($('#childposting').val() == 0)
            entId = $('#controlingsearch').val();
        else
            entId = $('#childposting').val();

        if ($('#newAnnexlist').val() == 0) {
            alert("Select Annexure to Proceed");
            return;
        }
        if (entId == 0) {
            alert("Select Reporting Office or Place of Posting to Proceed");
            return;
        }
      
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_functional_observations",
            type: "POST",
            data: {
                'ANNEX_ID': $('#newAnnexlist').val(),
                'ENTITY_ID': entId
            },
            cache: false,
            success: function (data) {
                var sr = 1;

                $.each(data, function (index, item) {
                    $('#entitywise_panel tbody').append("<tr><td>" + sr + "</td><td>" + item.name + "</td><td>" + item.audiT_PERIOD + "</td> <td class=\"text-right\">" + item.parA_NO + "</td><td><a href=\"#\" onclick=\"getParaText('" + item.coM_ID + "');\">View Para Text</a></td></tr>");
                    sr++;
                });
              initializeDataTable('entitywise_panel');              

            },
            dataType: "json",
        });
    }

    function getParaText(comID) {
        $('#paraTextViewerModel').modal("show");
        $('#paraTextDivField').empty();

        if (!comID) {
            $('#paraTextDivField').html('<p class="text-center">Para information is unavailable.</p>');
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_all_para_text",
            type: "POST",
            data: {
                'COM_ID': comID
            },
            cache: false,
            success: function (data) {
                if (data) {
                    $('#paraTextDivField').html(data);
                } else {
                    $('#paraTextDivField').html('<p class="text-center">No para text available.</p>');
                }
            }
        });
    }

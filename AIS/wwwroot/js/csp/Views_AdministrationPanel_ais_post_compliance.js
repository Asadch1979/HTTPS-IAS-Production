    var postComplianceData = [];
    var tableFields = [
        "COMID",
        "AUDITPERIOD",
        "GISTOFPARAS",
        "AUDITEDBY",
        "ENTITYTYPEID",
        "COMCYCLE",
        "COMSTATUS",
        "COMSTAGE",
        "PARASTATUS",
        "PARANO",
        "IND",
        "RISK"
    ];
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
    function getPostCompliance() {
        destroyDatatable('manageObsPanel');
        $('#manageObsPanel tbody').empty();
        if ($('#childposting option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_ais_post_compliance_details",
                type: "POST",
                data: {
                    'ENT': $('#childposting option:selected').val()
                },
                cache: false,
                success: function (data) {
                    postComplianceData = data;
                    $.each(data, function (idx, v) {
                        var row = '<tr data-index="' + idx + '"><td>' + (idx + 1) + '</td>';
                        $.each(tableFields, function(_, f){
                            var val = v[f];
                            if (val === undefined) val = v[f.toLowerCase()];
                            if (val === undefined) val = v[f.toUpperCase()];
                            row += '<td>' + (val !== undefined ? val : '') + '</td>';
                        });
                        row += '<td><button class="btn btn-sm btn-primary" data-onclick="openUpdateModal(' + idx + ');">Update</button></td></tr>';
                        $('#manageObsPanel tbody').append(row);
                    });
                    initializeDataTable('manageObsPanel');
                },
                dataType: "json",
            });
        }
    }
    function openUpdateModal(index) {
        var d = postComplianceData[index];
        $('#rowIndex').val(index);
        $.each(tableFields, function (_, f) {
            var val = d[f] || d[f.toLowerCase()] || d[f.toUpperCase()] || d[f.replace(/_/g, '')];
            var id = '#upd_' + f;
            if ($(id).length)
                $(id).val(val || '');
        });
        $('#updateModal').modal('show');
    }
    function submitUpdate() {
        var idx = $('#rowIndex').val();
        var payload = {};
        $('#updateModal input').each(function(){
            payload[$(this).attr('name')] = $(this).val();
        });
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_ais_post_compliance",
            type: "POST",
            data: payload,
            cache: false,
            success: function (resp) {
                showApiAlert(resp);
                $('#updateModal').modal('hide');
                getPostCompliance();
            },
            dataType: "json",
        });
    }

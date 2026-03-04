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
    function getAISEntities(){
        $('#auditeeEntitiesList tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getAISEntities",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting').val(),
                'TYPE_ID': $('#EntTypeField').val()
            },

            cache: false,
            success: function (data) {
                var sr = 0;
                $.each(data, function (i, v) {
                    sr++;
                    $('#auditeeEntitiesList tbody').append('<tr><td>' + sr + '</td><td>' + v.entitY_ID + '</td><td>' + v.code + '</td><td>' + v.typE_NAME + '</td><td>' + v.cosT_CENTER + '</td><td>' + v.name + '</td><td>' + v.auditbY_NAME + '</td><td>' + v.status + '</td></tr>');

                });

            },
            dataType: "json",
        });
    }

    function getEntities() {

        getCBASEntities();
        getERPEntities();
        getHREntities();
    
    }
    function getCBASEntities() {
        $('#auditeeEntitiesList tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getCBASEntities",
            type: "POST",
            data: {
                'E_ID': $('#childposting').val(),
                'E_CODE': $('#childposting option:selected').text()
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    sr++;
                    $('#auditeeEntitiesList tbody').append('<tr><td>' + sr + '</td><td>' + v.entitY_ID + '</td><td>' + v.name + '</td><td><a class="text-danger" data-onclick="event.preventDefault();updateAuditeeEntities(' + v.entitY_ID + ')">Update</a></td></tr>');

                });
              
            },
            dataType: "json",
        });
        //  getrelation();

    }
    function getERPEntities() {
        $('#auditeeEntitiesList_erp tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getERPEntities",
            type: "POST",
            data: {
                'E_ID': $('#childposting').val(),
                'E_CODE': $('#childposting option:selected').text()
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    sr++;
                    $('#auditeeEntitiesList_erp tbody').append('<tr><td>' + sr + '</td><td>' + v.entitY_ID + '</td><td>' + v.name + '</td><td><a class="text-danger" data-onclick="event.preventDefault();updateAuditeeEntities(' + v.entitY_ID + ')">Update</a></td></tr>');

                });

            },
            dataType: "json",
        });
        //  getrelation();

    }
    function getHREntities() {
        $('#auditeeEntitiesList_hr tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getHREntities",
            type: "POST",
            data: {
                'E_ID': $('#childposting').val(),
                'E_CODE': $('#childposting option:selected').text()
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    sr++;
                    $('#auditeeEntitiesList_hr tbody').append('<tr><td>' + sr + '</td><td>' + v.entitY_ID + '</td><td>' + v.name + '</td><td><a class="text-danger" data-onclick="event.preventDefault();updateAuditeeEntities(' + v.entitY_ID + ')">Update</a></td></tr>');

                });

            },
            dataType: "json",
        });
        //  getrelation();

    }

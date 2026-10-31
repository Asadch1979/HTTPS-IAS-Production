   
  
    function getEntity() {
        $('#entitySelectField').empty();
        $('#entitySelectField').append('<option value="0">--Select Audit Entity--</option>');
        if ($('#entityTypeSelectField').val()!=0){           
            
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_auditee_entities_by_entity_type_id",
                type: "POST",
                data: {
                    'ENTITY_TYPE_ID': $('#entityTypeSelectField').val()
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, v) {
                        $('#entitySelectField').append('<option value="' + v.typE_ID + '">' + v.entitytypedesc + '</option>');
                    });
                },

                dataType: "json",
            });
        }
     

    }

    function resetLegacyParaFields(){
        $('#entityTypeSelectField').val(0);
        $('#entitySelectField').val(0);
        $('#natureSelectField').val(0);
        $('#yearSelectField').val(0);
        $('#paraNoField').val('');
        $('#paragistField').val('');
        $('#amountField').val('');
        $('#annexureField').val('');
        $('#VolumeSelectField').val(0);
        $('#instancesField').val('');

    }

    function addLegacyParaToDB(){
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_new_legacy_para",
            type: "POST",
            data: {
                'ENTITY_TYPE_ID': $('#entityTypeSelectField').val(),
                'ENTITY_ID': $('#entitySelectField').val(),
                'NATURE_ID': $('#natureSelectField').val(),
                'AUDIT_YEAR': $('#yearSelectField').val(),
                'PARA_NO': $('#paraNoField').val(),
                'GIST_OF_PARA': $('#paragistField').val(),
                'AMOUNT': $('#amountField').val(),
                'ANNEXURE': $('#annexureField').val(),
                'VOL_I_II': $('#VolumeSelectField').val(),
                'NO_OF_INSTANCES': $('#instancesField').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                resetLegacyParaFields();
            },

            dataType: "json",
        });
    }
  
    function reloadLocation() {
        getLegacyPara();
    }

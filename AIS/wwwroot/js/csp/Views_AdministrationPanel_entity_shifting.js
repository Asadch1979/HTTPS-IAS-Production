    var g_aisEntitiesRec = [];
    var g_entIdToUpdate = 0;
    var g_entIdtoUpdateMapping = 0;
    var g_mappingExists = [];
    var url_string = window.location;
    var url = new URL(url_string);
    var ent_code = url.searchParams.get("code");
    var ent_name = url.searchParams.get("name");
    var g_uploadedAttachment="";
    var g_fromEntityDetail = null;
    var g_toEntityDetail = null;

    function entitySelectionButtons(entityId, entityName) {
        var safeName = String(entityName || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
        return '<td><button type="button" class="btn btn-sm btn-outline-primary" data-onclick="selectEntityForShifting(' + entityId + ',\'' + safeName + '\',\'from\');">Use as From</button></td>'
            + '<td><button type="button" class="btn btn-sm btn-outline-success" data-onclick="selectEntityForShifting(' + entityId + ',\'' + safeName + '\',\'to\');">Use as To</button></td>';
    }

    function applyStoredEntitySelection(selection) {
        selection = selection || {};
        if (selection.from && selection.from.id) {
            $('#inputFromEntityId').val(selection.from.id);
        }
        if (selection.to && selection.to.id) {
            $('#inputToEntityId').val(selection.to.id);
        }
    }

    function selectEntityForShifting(entityId, entityName, target) {
        window.entityDashboardSelectEntity(entityId, entityName, target);
        applyStoredEntitySelection(window.entityDashboardGetShiftingSelection());
    }

    $(document).ready(function () {
        $('#summaryPanel').addClass('d-none');
        $('#entityNameTextBar').val(ent_name);
        $('#entityCodeTextBar').val(ent_code);
        $('#AISentityNameTextBar').val(ent_name);
        $('#AISentityCodeTextBar').val(ent_code);
        updateEntityShiftMode();
        if (window.entityDashboardGetShiftingSelection) {
            applyStoredEntitySelection(window.entityDashboardGetShiftingSelection());
        }
        $(window)
            .off('entity-dashboard-shifting-selection.entityShifting')
            .on('entity-dashboard-shifting-selection.entityShifting', function (event) {
                var originalEvent = event.originalEvent || event;
                applyStoredEntitySelection(originalEvent.detail);
            });


         $('#circularRefField').on('change', function (e) {
        var file = e.target.files[0];

        if (file) {
          // Use FileReader to read the file and get base64 encoding
          var reader = new FileReader();

          reader.onload = function (readerEvent) {
            g_uploadedAttachment = readerEvent.target.result;

          };

          // Read the file as Data URL (base64)
          reader.readAsDataURL(file);
        }
      });


    });


    function getEntityDetailsByName() {


        $('#entity_find_panel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ais_entities_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_NAME': $('#entityNameTextBar').val(),
                'ENTITY_CODE': ""
            },
            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    $('#entity_find_panel tbody').append('<tr><td>' + ++i + '</td><td>' + v.entitY_ID + '</td><td>' + v.entitY_CODE + '</td><td>' + v.entitY_NAME + '</td><td>' + v.typE_ID + '</td><td>' + v.audiT_BY + '</td><td>' + v.auditable + '</td><td>' + v.status + '</td>' + entitySelectionButtons(v.entitY_ID, v.entitY_NAME) + '</tr>');
                });


            },
            dataType: "json",
        });



    }

    function getEntityDetailsByCode() {

        $('#entity_find_panel tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ais_entities_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_CODE': $('#entityCodeTextBar').val(),
                'ENTITY_NAME': ""
            },
            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    $('#entity_find_panel tbody').append('<tr><td>' + ++i + '</td><td>' + v.entitY_ID + '</td><td>' + v.entitY_CODE + '</td><td>' + v.entitY_NAME + '</td><td>' + v.typE_ID + '</td><td>' + v.audiT_BY + '</td><td>' + v.auditable + '</td><td>' + v.status + '</td>' + entitySelectionButtons(v.entitY_ID, v.entitY_NAME) + '</tr>');
                });

            },
            dataType: "json",
        });


    }

    //////////////////////////////////////////////////


    function getAISEntityDetailsByName() {
        $('#ais_entity_find_panel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ais_entities_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_NAME': $('#AISentityNameTextBar').val(),
                'ENTITY_CODE': ""
            },
            cache: false,
            success: function (data) {
                g_aisEntitiesRec = data;
                $.each(data, function (i, v) {

                    $('#ais_entity_find_panel tbody').append('<tr><td>' + ++i + '</td><td>' + v.entitY_ID + '</td><td>' + v.entitY_CODE + '</td><td>' + v.entitY_NAME + '</td><td>' + v.typE_ID + '</td><td>' + v.audiT_BY + '</td><td>' + v.auditable + '</td><td>' + v.status + '</td><td><a data-onclick="event.preventDefault();updateAISEntity(' + v.entitY_ID + ');" href="#">Update Entity</a></td><td><a data-onclick="updateAISEntityMapping(' + v.entitY_ID + ');" href="#">Update Entity Mapping</a></td></tr>');
                });



            },
            dataType: "json",
        });
    }

    function getAISEntityDetailsByCode() {

        $('#ais_entity_find_panel tbody').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ais_entities_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_CODE': $('#AISentityCodeTextBar').val(),
                'ENTITY_NAME': ""
            },
            cache: false,
            success: function (data) {
                g_aisEntitiesRec = data;
                $.each(data, function (i, v) {

                    $('#ais_entity_find_panel tbody').append('<tr><td>' + ++i + '</td><td>' + v.entitY_ID + '</td><td>' + v.entitY_CODE + '</td><td>' + v.entitY_NAME + '</td><td>' + v.typE_ID + '</td><td>' + v.audiT_BY + '</td><td>' + v.auditable + '</td><td>' + v.status + '</td><td><a data-onclick="event.preventDefault();updateAISEntity(' + v.entitY_ID + ');" href="#">Update Entity</a></td><td><a data-onclick="updateAISEntityMapping(' + v.entitY_ID + ');" href="#">Update Entity Mapping</a></td></tr>');
                });

            },
            dataType: "json",
        });


    }



    ///////////////////////////////////////////////////
    function getFromEntityDetail() {

        if ($('#inputFromEntityId').val() == "") {
            alert("Enter Old Entity ID to proceed");
            return;
        }
        if ($('#inputToEntityId').val() == "") {
            alert("Enter New Entity ID to proceed");
            return;
        }
        if ($('#inputFromEntityId').val() == $('#inputToEntityId').val()) {
            alert("Old/From and New/To entities must be different");
            return;
        }


        getToEntityDetail();
    }
    function getToEntityDetail() {
        $('#summaryPanel').removeClass('d-none');
        g_fromEntityDetail = null;
        g_toEntityDetail = null;
        updateEntityShiftMode();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entity_shifting_details",
            type: "POST",
            data: {
                'ENTITY_ID': $('#inputFromEntityId').val()
            },
            cache: false,
            success: function (data) {
                if (data.length > 0) {
                    var val = data[0];
                    g_fromEntityDetail = val;
                    $('#entityNameFrom').html(val.name);
                    $('#entityTypeFrom').text((val.typE_ID || '') + (val.audiT_TYPE ? ' (' + val.audiT_TYPE + ')' : ''));
                    $('#entitySizeFrom').html(val.e_SIZE);
                    $('#entityRiskFrom').html(val.risk);
                    $('#engIDFrom').html(val.enG_ID);
                    $('#engStartFrom').html(val.starT_DATE);
                    $('#engEndFrom').html(val.enD_DATE);
                    $('#totParaFrom').html(val.totaL_PARA);
                    $('#totLegFrom').html(val.legacY_PARA);
                    $('#totLegOpenFrom').html(val.legacY_OPEN);
                    $('#totLegCloseFrom').html(val.legacY_CLOSE);
                    $('#totAISFrom').html(val.aiS_PARA);
                    $('#totAISOpenFrom').html(val.aiS_OPEN);
                    $('#totAISCloseFrom').html(val.aiS_CLOSE);
                    $('#compFrom').html(val.comP_SUB);
                } else {
                    g_fromEntityDetail = null;
                    $('#entityNameFrom').html('');
                    $('#entityTypeFrom').html('');
                    $('#entitySizeFrom').html('');
                    $('#entityRiskFrom').html('');
                    $('#engIDFrom').html('');
                    $('#engStartFrom').html('');
                    $('#engEndFrom').html('');
                    $('#totParaFrom').html('');
                    $('#totLegFrom').html('');
                    $('#totLegOpenFrom').html('');
                    $('#totLegCloseFrom').html('');
                    $('#totAISFrom').html('');
                    $('#totAISOpenFrom').html('');
                    $('#totAISCloseFrom').html('');
                    $('#compFrom').html('');
                }
                updateEntityShiftMode();
            },
            dataType: "json",
        });
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entity_shifting_details",
            type: "POST",
            data: {
                'ENTITY_ID': $('#inputToEntityId').val()
            },
            cache: false,
            success: function (data) {
                if (data.length > 0) {
                    var val = data[0];
                    g_toEntityDetail = val;

                    $('#entityNameTo').html(val.name);
                    $('#entityTypeTo').text((val.typE_ID || '') + (val.audiT_TYPE ? ' (' + val.audiT_TYPE + ')' : ''));
                    $('#entitySizeTo').html(val.e_SIZE);
                    $('#entityRiskTo').html(val.risk);
                    $('#engIDTo').html(val.enG_ID);
                    $('#engStartTo').html(val.starT_DATE);
                    $('#engEndTo').html(val.enD_DATE);
                    $('#totParaTo').html(val.totaL_PARA);
                    $('#totLegTo').html(val.legacY_PARA);
                    $('#totLegOpenTo').html(val.legacY_OPEN);
                    $('#totLegCloseTo').html(val.legacY_CLOSE);
                    $('#totAISTo').html(val.aiS_PARA);
                    $('#totAISOpenTo').html(val.aiS_OPEN);
                    $('#totAISCloseTo').html(val.aiS_CLOSE);
                    $('#compTo').html(val.comP_SUB);
                } else {
                    g_toEntityDetail = null;
                    $('#entityNameTo').html('');
                    $('#entityTypeTo').html('');
                    $('#entitySizeTo').html('');
                    $('#entityRiskTo').html('');
                    $('#engIDTo').html('');
                    $('#engStartTo').html('');
                    $('#engEndTo').html('');
                    $('#totParaTo').html('');
                    $('#totLegTo').html('');
                    $('#totLegOpenTo').html('');
                    $('#totLegCloseTo').html('');
                    $('#totAISTo').html('');
                    $('#totAISOpenTo').html('');
                    $('#totAISCloseTo').html('');
                    $('#compTo').html('');
                }
                updateEntityShiftMode();
            },
            dataType: "json",
        });
    }

    function updateEntityShiftMode() {
        var ready = !!g_fromEntityDetail && !!g_toEntityDetail;
        $('#proceedEntityShiftButton').prop('disabled', !ready);
        var branchFlow = ready
            && parseInt(g_fromEntityDetail.typE_ID, 10) === 6
            && parseInt(g_toEntityDetail.typE_ID, 10) === 6;
        $('#convertIslamicButton').toggleClass('d-none', !branchFlow);
    }


    function updateAISEntity(entId) {
        g_entIdToUpdate = entId;
        $('#updateEntityModal').modal('show');
        $.each(g_aisEntitiesRec, function (i, v) {
            if (v.entitY_ID == entId) {
                if (v.auditable == "")
                    v.auditable = 'N';
                if (v.status == "")
                    v.status = 'N';
                $('#entTypeField').val(v.typE_ID);
                $('#entCodeField').val(v.entitY_CODE);
                $('#entNameField').val(v.entitY_NAME);
                $('#entDescField').val(v.description);
                $('#entActiveField').val(v.status);
                $('#entAuditableField').val(v.auditable);
                $('#entAuditbyField').val(v.audiT_BY_ID);
            }
        })

    }

    function AddNewAISEntity(name, code) {

        $('#addEntityModal').modal('show');
        $('#addentTypeField').val('');
        $('#addentCodeField').val(code);
        $('#addentNameField').val(name);

        $('#addentDescField').val('');
        $('#addentActiveField').val('');
        $('#addentAuditableField').val('');
        $('#addentAuditbyField').val('');

    }


    function saveUpdateChangesInAISEntity() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_ais_entity_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_ID': g_entIdToUpdate,
                'ENTITY_NAME': $('#entNameField').val(),
                'ENTITY_CODE': $('#entCodeField').val(),
                'ENT_DESC': $('#entDescField').val(),
                'AUDITABLE': $('#entAuditableField').val(),
                'AUDIT_BY_ID': $('#entAuditbyField').val(),
                'ENTITY_TYPE_ID': $('#entTypeField').val(),
                'STATUS': $('#entActiveField').val()

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);

            },
            dataType: "json",
        });
    }

    function saveAddInAISEntity() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_ais_entity_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_ID': 0,
                'ENTITY_NAME': $('#addentNameField').val(),
                'ENTITY_CODE': $('#addentCodeField').val(),
                'ENT_DESC': $('#addentDescField').val(),
                'AUDITABLE': $('#addentAuditableField').val(),
                'AUDIT_BY_ID': $('#addentAuditbyField').val(),
                'ENTITY_TYPE_ID': $('#addentTypeField').val(),
                'STATUS': $('#addentActiveField').val()

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);

            },
            dataType: "json",
        });
    }

    function clearHREntitiesPanel() {
        $('#entity_find_panel tbody').empty();
    }

    function clearAISEntitiesPanel() {
        $('#ais_entity_find_panel tbody').empty();
    }


    function getEntityExistingMapping(entId) {
        g_entIdToUpdate = entId;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ais_entity_existing_mapping_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_ID': entId,

            },
            cache: false,
            success: function (data) {
                g_mappingExists = data;
                if (data.length > 0) {
                    data = data[0];
                    $('#mapentTypeField').val(data.parenT_TYPE_ID);
                    $('#mapentRelationshipField').val(data.relatioN_TYPE_ID);
                    $('#entity_mapping_panel tbody').empty();
                    $('#entity_mapping_panel tbody').append('<tr><td>' + data.parenT_ID + '</td><td>' + data.parenT_TYPE_ID + '</td><td>' + data.parenT_CODE + '</td><td>' + data.parenT_NAME + '</td><td>' + data.chilD_ID + '</td><td>' + data.chilD_TYPE_ID + '</td><td>' + data.chilD_CODE + '</td><td>' + data.chilD_NAME + '</td><td>' + data.auditeD_BY + '</td><td>' + data.relatioN_TYPE_ID + '</td></tr>');

                    getEntitiesListByTypeId(data.parenT_TYPE_ID, data.parenT_ID);
                } else {
                    $('#entity_mapping_panel tbody').empty();
                    $('#mapentTypeField').val(0);
                    $('#mapentParentField').val(0);
                    $('#mapentRelationshipField').val(0);
                }


            },
            dataType: "json",
        });

    }

    function getEntitiesListByTypeId(typeId, parentId) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_ais_entities_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENT_TYPE_ID': typeId,

            },
            cache: false,
            success: function (data) {

                $('#mapentParentField').empty();
                $('#mapentParentField').append('<option value="0">--Select Parent Entity--</option>');

                $.each(data, function (i, v) {
                    var selected = "";
                    if (v.entitY_ID == parentId)
                        selected = "selected=\"selected\"";
                    $('#mapentParentField').append('<option ' + selected + ' value="' + v.entitY_ID + '">' + v.entitY_NAME + '</option>');
                });

            },
            dataType: "json",
        });
    }

    function updateAISEntityMapping(entId) {
        $('#updateMappingEntityModal').modal('show');
        getEntityExistingMapping(entId);
    }

    function getOnChageList() {

        getEntitiesListByTypeId($('#mapentTypeField').val(), 0);
    }

    function saveInsertAISEntityMapping() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_ais_entity_mapping_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_ID': g_entIdToUpdate,
                'P_ENTITY_ID': $('#mapentParentField').val(),
                'RELATION_TYPE_ID': $('#mapentRelationshipField').val()

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);

            },
            dataType: "json",
        });


    }
    function saveUpdateAISEntityMapping() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_ais_entity_mapping_for_admin_panel_entity_addition",
            type: "POST",
            data: {
                'ENTITY_ID': g_entIdToUpdate,
                'P_ENTITY_ID': $('#mapentParentField').val(),
                'RELATION_TYPE_ID': $('#mapentRelationshipField').val()

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);

            },
            dataType: "json",
        });

    }

    function proceedToEntityShifting() {
        if (!g_fromEntityDetail || !g_toEntityDetail) {
            alert('Load valid old/from and new/to entity summary details first.');
            return;
        }

        var fromType = parseInt(g_fromEntityDetail.typE_ID, 10);
        var toType = parseInt(g_toEntityDetail.typE_ID, 10);
        if (fromType !== toType) {
            alert('Old/from and new/to entities must have the same entity type.');
            return;
        }
        $('#proceedToEntityShift').modal('show');
    }

    function proceedConvToIslamic(){
      
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_entity_conv_to_islamic_from_admin_panel",
            type: "POST",
            data: {
               'FROM_ENT_ID': $('#inputFromEntityId').val(),
                'TO_ENT_ID': $('#inputToEntityId').val(),

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);

            },
            dataType: "json",
        });

    }

    function saveInsertAISEntityShifting() {
        if (!g_fromEntityDetail || !g_toEntityDetail) {
            alert('Load valid entity summary details before shifting.');
            return;
        }
        if (!$('#circularRefNoField').val()) {
            alert('Enter Circular Reference No.');
            return;
        }
        if (!$('#circularRefDateField').val()) {
            alert('Select Circular Reference Date.');
            return;
        }

        onconfirmAlertCallback(function () {
            submitValidatedEntityShifting();
        });
        confirmAlert('Shift open paras, observations, compliance, and relevant entity mapping from '
            + (g_fromEntityDetail.name || $('#inputFromEntityId').val()) + ' to '
            + (g_toEntityDetail.name || $('#inputToEntityId').val()) + '?');
    }

    function submitValidatedEntityShifting() {
        var isBranchFlow = parseInt(g_fromEntityDetail.typE_ID, 10) === 6;
        $.ajax({
            url: g_asiBaseURL + (isBranchFlow
                ? "/ApiCalls/submit_entity_shifting_from_admin_panel"
                : "/ApiCalls/submit_department_entity_shifting_from_admin_panel"),
            type: "POST",
            data: {
                'FROM_ENT_ID': $('#inputFromEntityId').val(),
                'TO_ENT_ID': $('#inputToEntityId').val(),
                'CIR_REF': $('#circularRefNoField').val() ,
                'CIR_DATE': $('#circularRefDateField').val(),
                'CIR': g_uploadedAttachment

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                $('#proceedToEntityShift').modal('hide');
                getToEntityDetail();

            },
            error: function () {
                alert('Entity shifting could not be completed.');
            },
            dataType: "json",
        });

    }

    window.entityDashboardStepStateAdapters = window.entityDashboardStepStateAdapters || {};
    window.entityDashboardStepStateAdapters.ENTITY_SHIFTING = {
        capture: function () {
            return {
                uploadedAttachment: g_uploadedAttachment,
                fromEntityDetail: g_fromEntityDetail,
                toEntityDetail: g_toEntityDetail
            };
        },
        restore: function (state) {
            state = state || {};
            g_uploadedAttachment = state.uploadedAttachment || '';
            g_fromEntityDetail = state.fromEntityDetail || null;
            g_toEntityDetail = state.toEntityDetail || null;
            updateEntityShiftMode();
        }
    };

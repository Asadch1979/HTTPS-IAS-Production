    g_status = 'Created';
    var previousAuditCriteriaEntityTypeId = 0;

    function getSelectedAuditCriteriaEntityTypeId() {
        var rawValue = $('#auditCriteriaEntityField option:selected').val();
        var parsedValue = parseInt(rawValue, 10);
        return Number.isNaN(parsedValue) ? 0 : parsedValue;
    }

    function isBranchAuditCriteriaEntity(entityTypeId) {
        return entityTypeId === 6 || entityTypeId === 28;
    }

    function isCreditAdministrationUnit(entityTypeId) {
        return entityTypeId === 25;
    }

    function addRecordToauditCriteriaListBox() {

        var entityName = '';
        var entityTypeId = getSelectedAuditCriteriaEntityTypeId();
        var isBranchEntity = isBranchAuditCriteriaEntity(entityTypeId);
        var isCreditAdministrationEntity = isCreditAdministrationUnit(entityTypeId);
        var riskId = $('#auditCriteriaRiskField option:selected').val() || '0';
        var sizeId = $('#auditCriteriaSizeField option:selected').val() || '0';
        var frequencyId = $('#auditCriteriaFreqField option:selected').val() || '0';

        if (entityTypeId !== 0)
        {
            entityName = $('#auditCriteriaEntityField option:selected').text();
        }
        var period = '';
        if ($('#auditCriteriaPeriodField option:selected').val() != 0)
            period = $('#auditCriteriaPeriodField option:selected').text();
        var days = 0;
        if ($('#auditCriteriaDaysField').val() != 0)
            days = $('#auditCriteriaDaysField').val();
        var risk = '';
        if (riskId != '0')
            risk = $('#auditCriteriaRiskField option:selected').text();

        var freq = '';
        if (frequencyId != '0')
            freq = $('#auditCriteriaFreqField option:selected').text();
        var size = '';
        if (sizeId != '0')
            size = $('#auditCriteriaSizeField option:selected').text();

        var visit = 'No';
        if ($('#auditCriteriaVisitField').is(':checked'))
            visit = "Yes";


        if (period == '') {
            alert('Audit Period Not Selected');
            return;
        }
        if (entityTypeId == 0) {
            alert('Entity Type Not Selected');
            return;
        }

        if (isBranchEntity) {
            if (risk == '') {
                alert('Risk Category Not Selected');
                return;
            }
            if (size == '') {
                alert('Branch Size Not Selected');
                return;
            }
            if (freq == '') {
                alert('Audit Frequency Not Selected');
                return;
            }
        } else {
            riskId = '1';
            sizeId = '1';
            risk = '';
            size = '';
            entityName = $('#auditCriteriaCADHUBField  option:selected').text();
        }

        if (isCreditAdministrationEntity) {
            frequencyId = '1';
            freq = $('#auditCriteriaFreqField option[value="1"]').text() || '';
        }

        if (days == 0) {
            alert('Number Of Days Not Selected');
            return;
        }
       
        data = '-';
        $('#auditCriteriaListBox tbody').append('<tr class="new"><td value="' + $('#auditCriteriaPeriodField option:selected').val() + '">' + period + '</td><td value="' + $('#auditCriteriaEntityField option:selected').val() + '"  data-value="' + $('#auditCriteriaCADHUBField option:selected').val() + '">' + entityName + '</td><td value="' + riskId + '">' + risk + '</td><td value="' + frequencyId + '">' + freq + '</td><td value="' + sizeId + '">' + size + '</td><td value="' + days + '">' + days + '</td><td value="' + visit + '">' + visit + '</td><td class="entCountField">' + data + '</td><td><a data-onclick=CountCriteriaRecordFromGrid(this,' + $('#auditCriteriaPeriodField option:selected').val() + ',' + $('#auditCriteriaEntityField option:selected').val() + ',' + riskId + ',' + sizeId + ',' + frequencyId + '); class="text-hover text-primary">Entities Count</a><a data-onclick=DeleteCriteriaRecordFromGrid(this,' + $('#auditCriteriaPeriodField option:selected').val() + ',' + $('#auditCriteriaEntityField option:selected').val() + ',' + riskId + ',' + sizeId + ',' + frequencyId + '); class="text-hover text-danger pl-3">Delete</a></td></tr>');
           
        submitAuditCriteria();
    }
    function CountCriteriaRecordFromGrid(e, cId, entity_type, risk, size, freq) {


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/GetAuditEntitiesCount",
            type: "POST",
            data: {
                'CRITERIA_ID': cId
            },
            cache: false,
            success: function (data) {
                console.log('count of entities', data);
                $(e).parent().parent().find('td.entCountField').eq(0).html(data);

            },

            dataType: "json",
        });
    }
   
    function DeleteCriteriaRecordFromGrid(e, criteria_id) {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/DeletePendingCriteria",
            type: "POST",
            data: {
                'CID': criteria_id
            },
            cache: false,
            success: function (data) {
                reloadLocation();
            },

            dataType: "json",
        });
    }

    function reloadLocation() {
        if (window.planningDashboard && typeof window.planningDashboard.reloadCurrentStep === 'function') {
            window.planningDashboard.reloadCurrentStep();
            return;
        }

        location.reload();
    }

    function addAuditEntitiesModal() {
        $('#setupAuditEntitiesModal').modal('show');
    }

    function mapAuditCriteriaRows(response) {
        if (response && Array.isArray(response.rows)) {
            return response.rows;
        }

        if (Array.isArray(response)) {
            return $.map(response, function (item, index) {
                return {
                    rowIndex: index + 1,
                    auditPeriod: item[0] || '',
                    entityName: item[1] || '',
                    risk: item[2] || '',
                    size: item[3] || '',
                    frequency: item[4] || '',
                    message: item[5] || '',
                    success: /success/i.test(item[5] || '')
                };
            });
        }

        return [];
    }

    function buildAuditCriteriaAlertMessage(response) {
        var rows = mapAuditCriteriaRows(response);
        if (rows.length > 0) {
            return $.map(rows, function (row) {
                return "Criteria = "
                    + (row.auditPeriod || '')
                    + " | " + (row.entityName || '')
                    + " | " + (row.risk || '')
                    + " | " + (row.size || '')
                    + " | " + (row.frequency || '')
                    + " " + (row.message || '');
            }).join('\n');
        }

        if (response && response.message) {
            return response.message;
        }

        return "Audit criteria request completed.";
    }

    function hasSuccessfulAuditCriteriaRow(response) {
        return $.grep(mapAuditCriteriaRows(response), function (row) {
            return row && row.success === true;
        }).length > 0;
    }

    function submitAuditCriteria() {

        var criteria_list = [];
        $.each($('#auditCriteriaListBox tbody tr.new'), function (index, row) {
            var criteria = [];
            $.each($(row).find('td'), function (i, d) {
                if (typeof $(d).attr('value') != "undefined" && $(d).attr('value') != null) {
                    criteria.push($(d).attr('value'));
                }
            });
            criteria.push($($(row).find('td').eq(0)).html()); //[period]
            criteria.push($($(row).find('td').eq(1)).html()); //[entname]
            criteria.push($($(row).find('td').eq(2)).html()); //[risk]
            criteria.push($($(row).find('td').eq(4)).html()); //[size]
            criteria.push($($(row).find('td').eq(3)).html()); //[freq]
            criteria.push($($(row).find('td').eq(1)).attr('data-value')); //[entityId]
            criteria_list.push(criteria);
        });
        console.log(criteria_list);

        if (criteria_list.length > 0) {
            $.ajax({
                url: g_asiBaseURL + "/Engagement/add_audit_criteria",
                type: "POST",
                data: {
                    'CRITERIA_LIST': criteria_list
                },
                cache: false,
                success: function (data) {
                    var alertMessage = buildAuditCriteriaAlertMessage(data);
                    if (hasSuccessfulAuditCriteriaRow(data)) {
                        $('#auditCriteriaListBox tbody tr').removeClass('new');
                        if (typeof onAlertCallback === 'function') {
                            onAlertCallback(reloadLocation);
                        }
                    }

                    alert(alertMessage);
                },
                error: function (xhr) {
                    alert(extractApiMessageFromXhr(xhr, "Unable to add audit criteria."));
                },

                dataType: "json",
            });
        }
        else {

            alert('No Audit Criteria Defined');
            return;
        }
    }
    function FinalsubmitAuditCriteria() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/submit_audit_criterias",
            type: "POST",
            data: {
                'PERIOD_ID': 0
            },
            cache: false,
            success: function (data) {

                alert("Audit Criteria Submitted Successfully");
                onAlertCallback(reloadLocation);
            },

            dataType: "json",
        });

    }


    function publishAuditeeEntity() {
        var code = $('#auditeeEntTypesCodeModelField').val();
        var desc = $('#auditeeEntTypesDescModelField').val();
        var isactive = $('#auditeeEntTypesIsActiveModelField').is(':checked');
        var isauditable = $('#auditeeEntTypesIsAuditableModelField').is(':checked');
        if (isactive)
            isactive = 'Y';
        else
            isactive = 'N';

        if (isauditable)
            isauditable = 'A';
        else
            isauditable = 'N';

        $.ajax({
            url: g_asiBaseURL + "/Engagement/add_auditee_entity",
            type: "POST",
            data: {
                'ENTITYCODE': code,
                'ENTITYTYPEDESC': desc,
                'ACTIVE': isactive,
                'AUDITABLE': isauditable
            },
            cache: false,
            success: function (data) {
                alert('Entity Added Successfully');
                onAlertCallback(reloadLocation);

            },
            dataType: "json",
        });
    }

    function setSizeEnableForBranches() {
        var entityTypeId = getSelectedAuditCriteriaEntityTypeId();
        var isBranchEntity = isBranchAuditCriteriaEntity(entityTypeId);
        var isCreditAdministrationEntity = isCreditAdministrationUnit(entityTypeId);

        if (isCreditAdministrationEntity) {
            $('#auditCriteriaFreqField').val('1');
            $('#auditCriteriaFreqField').attr('disabled', true);
            $('#freqPanel').addClass('d-none');
        } else {
            if (previousAuditCriteriaEntityTypeId === 25) {
                $('#auditCriteriaFreqField').val('0');
            }
            $('#auditCriteriaFreqField').attr('disabled', false).show();
            $('#freqPanel').removeClass('d-none');
        }

        if (!isBranchEntity) {
            $('#auditCriteriaRiskField').val('1');
            $('#auditCriteriaSizeField').val('1');
            $('#auditCriteriaSizeField').attr('disabled', true);
            $('#auditCriteriaRiskField').attr('disabled', true);
            $('#nonCADHUBPanel').addClass('d-none');
        } else {
            if (!isBranchAuditCriteriaEntity(previousAuditCriteriaEntityTypeId)) {
                $('#auditCriteriaRiskField').val('0');
                $('#auditCriteriaSizeField').val('0');
            }
            $('#auditCriteriaRiskField').attr('disabled', false);
            $('#auditCriteriaSizeField').attr('disabled', false);
            $('#nonCADHUBPanel').removeClass('d-none');
        }
        $('#auditCriteriaCADHUBField').empty();
        $('#auditCriteriaCADHUBField').append('<option value="0">-- Select Auditable Entity--</option>');

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_entities_parent_ent_type_id",
            type: "POST",
            data: {
                'ENTITY_TYPE_ID': $('#auditCriteriaEntityField').val()
            },
            cache: false,
            success: function (data) {
                $.each(data,function(i,v){
                    $('#auditCriteriaCADHUBField').append('<option value="'+v.entitY_ID+'">'+v.name+'</option>');
                });
                


            },

            dataType: "json",
        });

        previousAuditCriteriaEntityTypeId = entityTypeId;
    }

    $(document).ready(function () {
        setSizeEnableForBranches();
    });

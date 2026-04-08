    g_status = 'Created';

    function addRecordToauditCriteriaListBox() {

        var entityName = '';
        var entityTypeId = 0;
        if ($('#auditCriteriaEntityField option:selected').val() != 0)
        {
            entityName = $('#auditCriteriaEntityField option:selected').text();
            entityTypeId = $('#auditCriteriaEntityField option:selected').val();
        }
        var period = '';
        if ($('#auditCriteriaPeriodField option:selected').val() != 0)
            period = $('#auditCriteriaPeriodField option:selected').text();
        var days = 0;
        if ($('#auditCriteriaDaysField').val() != 0)
            days = $('#auditCriteriaDaysField').val();
        var risk = '';
        if ($('#auditCriteriaRiskField option:selected').val() != 0)
            risk = $('#auditCriteriaRiskField option:selected').text();

        var freq = '';
        if ($('#auditCriteriaFreqField option:selected').val() != 0)
            freq = $('#auditCriteriaFreqField option:selected').text();
        var size = '';
        if ($('#auditCriteriaSizeField option:selected').val() != 0)
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
       
       
        if ($('#auditCriteriaEntityField option:selected').attr("d-risk") == "Y") {
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
            entityName = $('#auditCriteriaCADHUBField  option:selected').text();
        }
        if (days == 0) {
            alert('Number Of Days Not Selected');
            return;
        }
      
        data = '-';
        $('#auditCriteriaListBox tbody').append('<tr class="new"><td value="' + $('#auditCriteriaPeriodField option:selected').val() + '">' + period + '</td><td value="' + $('#auditCriteriaEntityField option:selected').val() + '"  data-value="' + $('#auditCriteriaCADHUBField option:selected').val() + '">' + entityName + '</td><td value="' + $('#auditCriteriaRiskField option:selected').val() + '">' + risk + '</td><td value="' + $('#auditCriteriaFreqField option:selected').val() + '">' + freq + '</td><td value="' + $('#auditCriteriaSizeField option:selected').val() + '">' + size + '</td><td value="' + days + '">' + days + '</td><td value="' + visit + '">' + visit + '</td><td class="entCountField">' + data + '</td><td><a data-onclick=CountCriteriaRecordFromGrid(this,' + $('#auditCriteriaPeriodField option:selected').val() + ',' + $('#auditCriteriaEntityField option:selected').val() + ',' + $('#auditCriteriaRiskField option:selected').val() + ',' + $('#auditCriteriaSizeField option:selected').val() + ',' + $('#auditCriteriaFreqField option:selected').val() + '); class="text-hover text-primary">Entities Count</a><a data-onclick=DeleteCriteriaRecordFromGrid(this,' + $('#auditCriteriaPeriodField option:selected').val() + ',' + $('#auditCriteriaEntityField option:selected').val() + ',' + $('#auditCriteriaRiskField option:selected').val() + ',' + $('#auditCriteriaSizeField option:selected').val() + ',' + $('#auditCriteriaFreqField option:selected').val() + '); class="text-hover text-danger pl-3">Delete</a></td></tr>');
           
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
                    var row = "";
                    $.each(data, function (i, v) {
                        row += "Criteria = " + v[0] + " | " + v[1] + " | " + v[2] + " | " + v[3] + " | " + v[4] + "  " + v[5] + "";
                    });
                    $('#auditCriteriaListBox tbody tr').removeClass('new');
                    alert(row);
                    onAlertCallback(reloadLocation);
                    
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
        var risk_check="N";
    $.each($('#auditCriteriaEntityField option'), function (e, v)
    {
            if ($(v).val() == $('#auditCriteriaEntityField option:selected').val())
                {risk_check=$(v).attr("d-risk");}
        });

        if ($('#auditCriteriaEntityField option:selected').val() == '25') {
            $('#auditCriteriaFreqField').val('1');
            $('#auditCriteriaFreqField').attr('disabled', true).hide();
        } else {
            $('#auditCriteriaFreqField').attr('disabled', false).show();
        }
        if ($('#auditCriteriaEntityField option:selected').val() != 6 && $('#auditCriteriaEntityField option:selected').val() != 28) {
            $('#auditCriteriaSizeField').val('1');
            $('#auditCriteriaSizeField').attr('disabled', true);

        } else {
            $('#auditCriteriaSizeField').attr('disabled', false);
        }

        if (risk_check == "Y") {
            $('#nonCADHUBPanel').removeClass('d-none');

        } else {
            $('#auditCriteriaRiskField').val("3"); //Setting RISK to LOW
            $('#nonCADHUBPanel').addClass('d-none');
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

    }

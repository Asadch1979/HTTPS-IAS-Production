    g_status = 'Created';

    function addRecordToauditCriteriaListBox() {
        var entityName = '';
        if ($('#auditCriteriaEntityField option:selected').val() != 0)
            entityName = $('#auditCriteriaEntityField option:selected').text();

        var period = '';
        if ($('#auditCriteriaPeriodField option:selected').val() != 0)
            period = $('#auditCriteriaPeriodField option:selected').text();

        var days = $('#auditCriteriaDaysField').val();
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

        $('#auditCriteriaListBox tbody').append('<tr><td value="' + $('#auditCriteriaPeriodField option:selected').val() + '">' + period + '</td><td value="' + $('#auditCriteriaEntityField option:selected').val() + '">' + entityName + '</td><td value="' + $('#auditCriteriaRiskField option:selected').val() + '">' + risk + '</td><td value="' + $('#auditCriteriaFreqField option:selected').val() + '">' + freq + '</td><td value="' + $('#auditCriteriaSizeField option:selected').val() + '">' + size + '</td><td value="' + days + '">' + days + '</td><td value="' + visit + '">' + visit + '</td><td><a class="text-hover text-danger">' + g_status + '</a></td><td><a data-onclick=DeleteCriteriaRecordFromGrid(this); class="text-hover text-danger">Delete</a></td></tr>')

    }
    function DeleteCriteriaRecordFromGrid(e) {
        $(e).parent().parent().remove();
    }
    function enitityChangeEvent() {
        if ($('#auditCriteriaEntityField option:selected').val() == 6) {
            // $('.brField').removeClass('d-none');
            $('#auditCriteriaSizeField option:selected').val(0);
        } else
            // $('.brField').addClass('d-none');
            $('#auditCriteriaSizeField option:selected').val(0);
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

    function submitAuditCriteria() {

        var criteria_list = [];
        $.each($('#auditCriteriaListBox tbody tr'), function (index, row) {
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
            criteria_list.push(criteria);
        });
        //console.log(criteria_list);
        //  return;
        if (criteria_list.length > 0) {
            $.ajax({
                url: g_asiBaseURL + "/Engagement/add_audit_criteria",
                type: "POST",
                data: {
                    'CRITERIA_LIST': criteria_list
                },
                cache: false,
                success: function (data) {
                    alert(buildAuditCriteriaAlertMessage(data));
                },
                error: function (xhr) {
                    alert(extractApiMessageFromXhr(xhr, "Unable to add audit criteria."));
                },
                dataType: "json",
            });
        }
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
                location.reload();

            },
            dataType: "json",
        });
    }

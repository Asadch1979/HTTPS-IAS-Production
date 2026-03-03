    var g_paraId = 0;
    var g_obsList = [];
    $(document).ready(function () {
        getLegacyPara();

        $('#responseAuditee').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#complianceAuditee').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#complianceAuditeeRemarks').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        //
        $('#PublishParaText').on('click', function () {
            publishResponseChanges();
        });
      
    });
    function getLegacyPara() {

        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_old_para_management",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {
                    child.audiT_PERIOD = child.audiT_PERIOD.split(' ')[0];
                    $('#manageObsPanel tbody').append('<tr id="' + child.parA_ID + '"><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td class="text-center"><a class="text-center text-danger" style="cursor:pointer;" onclick="event.preventDefault();processdetails(' + child.parA_ID + ');">Add Para Text</a></td><td class="text-center"><a class="text-center text-danger" style="cursor:pointer;" onclick="viewCompliance(' + child.parA_ID + ');">Add Compliance</a></td></tr>')
                });
                setTimeout(function () {
                    if (g_paraId != 0) {
                        var rowpos = $('#' + g_paraId).position();
                        $('html').scrollTop(rowpos.top);
                    }
                }, 200)

            },

            dataType: "json",
        });

    }
    function div_risksubcategoryShowHide() {
        if ($('#riskGroupSelectBox option:selected').val() == 0) {
            $('#div_risksubcategory').hide();
            $('#div_activityContainer').hide();
        }
        else {
            $('#div_risksubcategory').show();
            $('#div_activityContainer').hide();
            $('#riskSubGroupSelectBox').empty();
            $.ajax({
                url: g_asiBaseURL + "/Execution/sub_voilation",
                type: "POST",
                data: {
                    'V_ID': $('#riskGroupSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#riskSubGroupSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        $('#riskSubGroupSelectBox').append("<option value=\"" + item.id + "\"> " + item.suB_V_NAME + "</option> ");
                    });

                },
                dataType: "json",
            });
        }
    }
    function div_activityContainerShowHide() {
        if ($('#riskSubGroupSelectBox option:selected').val() == 0)
            $('#div_activityContainer').hide();
        else
            $('#div_activityContainer').show();
        $('#riskActivitiesSelectBox').empty();
        $.ajax({
            url: g_asiBaseURL + "/Execution/risk_activities",
            type: "POST",
            data: {
                'S_GR_ID': $('#riskSubGroupSelectBox option:selected').val(),
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                $('#riskActivitiesSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                $.each(data, function (index, item) {
                    $('#riskActivitiesSelectBox').append("<option value=\"" + item.activitY_ID + "\"> " + item.description + "</option>");
                });

            },
            dataType: "json",
        });
    }
    function processdetails(id) {
        g_paraId = id;
        $.each(g_obsList, function (i, v) {
            if (v.id == g_paraId) {
                console.log(v);
                $('#processField').val(v.procesS_DES);
                $('#subprocessField').val(v.suB_PROCESS_DES);
                $('#checklistDetailField').val(v.procesS_DETAIL_DES);
                $('#observation').html(v.gisT_OF_PARAS);
            }
        });
        $('#process_detail').modal('show');
        $('#responseAuditee').val('').trigger('change');
    }

    function viewCompliance(id) {
        g_paraId = id;
        $('#view_compliance_model').modal('show');
        $('#manageCompliancePanel tbody').empty();
    }


    function publishResponseChanges() {
        if ($('#riskGroupSelectBox').val() == 0) {
            alert('Select Violation Category');
            return;
        }
        if ($('#riskSubGroupSelectBox').val() == 0) {
            alert('Select Violation Nature');
            return;
        }
        if ($('#auditCriteriaRiskField').val() == 0) {
            alert('Select Risk');
            return;
        }

        if ($('#process_detail .richText-editor').html() == "") {
            alert("Please enter Reply");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_legacy_para_cad_reply",
            type: "POST",
            data: {
                'ID': g_paraId,
                'V_CAT_ID': $('#riskGroupSelectBox').val(),
                'V_CAT_NATURE_ID': $('#riskSubGroupSelectBox').val(),
                'RISK_ID': $('#auditCriteriaRiskField').val(),
                'REPLY': $('#process_detail .richText-editor').html()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function publishCompliance() {

        var COMPLIANCE_LIST = [];
        $.each($('#manageCompliancePanel tbody tr'), function (i, v)
        {
            COMPLIANCE_LIST.push({
                'ParaRef': g_paraId,
                'ComplianceDate': $(v).find('td').eq(0).html(),
                'AuditeeCompliance': $(v).find('td').eq(1).html(),
                'AuditorRemarks': $(v).find('td').eq(2).html(),
                'CnIRecommendation': $(v).find('td').eq(3).html()
            });

        });

        if (COMPLIANCE_LIST.length > 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_legacy_para_cad_compliance",
                type: "POST",
                data: {
                    'COMPLIANCE_LIST': COMPLIANCE_LIST
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }

      
    }
    function reloadLocation() {
        getLegacyPara();
    }

    function openComplianceModal() {
        $('#submit_compliance_model').modal('show');

    }
    function deleteRespRow(e) {
        $(e).parent().parent().remove();
    }

    function addComplianceToComplianceGrid() {
        $('#manageCompliancePanel tbody').append('<tr id="tr_' + $('#complianceReceivedOn').val() + '"><td>' + $('#complianceReceivedOn').val() + '</td><td><div>' + $('#complianceAuditee').val() + '</div></td><td><div>' + $('#complianceAuditeeRemarks').val() + '</div></td><td>' + $('#CnIRecommendations').val() + '</td><td class="text-center"><a href="#" onclick="event.preventDefault();deleteRespRow(this);">Delete</a></td></tr>');

        $('#submit_compliance_model').modal('hide');

    }

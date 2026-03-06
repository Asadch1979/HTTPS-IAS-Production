window.addEventListener("error", function (e) {
    console.error("JS error:", e.message, e.filename, e.lineno, e.colno);
});
window.addEventListener("unhandledrejection", function (e) {
    console.error("Promise rejection:", e.reason);
});

function getPageData() {
    const el = document.getElementById("page-data");
    if (!el) return {};
    try {
        return JSON.parse(el.textContent || "{}");
    } catch (err) {
        console.error("Failed to parse #page-data JSON:", err);
        return {};
    }
}

        var g_com_id = 0;
        var g_np_id = 0;
        var g_op_id = 0;
        var g_ind = "";
        var g_allObs = [];
        var g_index = 0;
        var auditParaSection = null;
        var respSectionUpdate = null;
        const pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];
        var g_selectedRiskId = 0;

        $(document).ready(function () {
            console.log("Loaded manage_audit_paras JS", { annexCount: g_annexList.length });
            $('#entitySelectField').select2();
            $('#paraTextViewer').richText({
                imageUpload: false,
                fileUpload: false,
                videoEmbed: false,
                urls: false
            });
                const currentYear = new Date().getFullYear();
    for (var i = 1970; i <= currentYear; i++) {
        $('#auditPara_Period').append(
            '<option value="' + i + '">Audit Year ' + i + '</option>'
        );
    }
            auditParaSection = initFieldAuditParaSection({
                containerSelector: '#viewMemoModel',
                annexList: g_annexList,
                readOnly: false
            });
            respSectionUpdate = initResponsibilitySection({
                tableSelector: '#listofRespPersons',
                changesTableSelector: '#c_listofRespPersons',
                modalSelector: '#ResponsiblePPModel',
                status: 1,
                indicator: 'O',
                directSaveMode: false
            });

        });
        function getrelation(parentEntityId = 0, userEntityId = 0) {


            $('#controlingsearch').empty();
            $('#childposting').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/getparentrelForParaPositionReport",
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
                url: g_asiBaseURL + "/ApiCalls/getpostplaceForParaPositionReport",
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
        function reloadLocation() {
            getEntityObservation();
        }
        function getEntityObservation() {
            destroyDatatable('manageObsPanel');
            if ($('#childposting option:selected').val() != 0) {
                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/get_observations_for_manage_paras",
                    type: "POST",
                    data: {
                        'ENTITY_ID': $('#childposting option:selected').val()
                    },
                    cache: false,
                    success: function (data) {
                        g_allObs = data;
                        $.each(data, function (i, v) {
                            var row = '<tr index="' + i + '"><td class="text-center">' + (i + 1) + '</td><td class="text-center">' + v.audiT_PERIOD + '</td><td>' + v.parA_NO + '</td><td>' + v.annex + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_GIST + '</td><td class="text-center"><a data-onclick="event.preventDefault();ObservationViewerPanel(\'' + i + '\')" href="#" class="text-hover">Update Para Details</a></td><td class="text-center"><a data-onclick="event.preventDefault();DeleteDuplicatePara(\'' + v.neW_PARA_ID + '\',\'' + v.olD_PARA_ID + '\', \'' + v.p_TYPE_IND + '\')" href="#" class="text-hover">Delete Duplicate Para</a></td>';
                            if($('#userGroupField').val()==1)
                                row += '<td><input type="checkbox" class="selectedParasToShift"/></td>';
                            row += '</tr>';
                            $('#manageObsPanel tbody').append(row);
                        });
                        initializeDataTable('manageObsPanel');
                    },
                    dataType: "json",
                });
            }
        }

        function selectAllParasShift(){
            if($('#selectAllChkBox').is("checked"))
            {
                $('.selectedParasToShift').attr("checked",true);
            }else{
                $('.selectedParasToShift').attr("checked",false);
            }

        }

        function ObservationViewerPanel(index) {
            g_index = index;
            var item = g_allObs[index];
            g_com_id = item.coM_ID || item.COM_ID || item.com_id;
            $('#viewMemoModel').modal('show');
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_observations_details_for_manage_paras",
                type: "POST",
                data: { 'COM_ID': g_com_id },
                cache: false,
                success: function (v) {
                    g_np_id = v.neW_PARA_ID;
                    g_op_id = v.olD_PARA_ID;
                    g_ind = v.indicator;
                    $('#auditPara_Period').val(v.audiT_PERIOD);
                    $('#auditPara_ParaNO').val(v.parA_NO);
                    $('#auditPara_Annex').val(v.anneX_ID || v.annex);
                    updateRiskDisplay();
                    $('#auditPara_Gist').val(v.obS_GIST);
                    $('#paraTextViewer').val(v.parA_TEXT).trigger('change');
                    $('#auditPara_AmountInv').val(v.amounT_INV);
                    $('#auditPara_InstNO').val(v.nO_INSTANCES);
                    if (respSectionUpdate) {
                        respSectionUpdate.updateContext({ comId: g_com_id, newParaId: g_np_id, oldParaId: g_op_id, indicator: 'O' });
                    }
                    if (window.ReferenceSection) {
                        ReferenceSection.init({ comId: g_com_id });
                    }
                },
                dataType: "json"
            });
        }

        // reference search handlers removed


    function updateObservationStatus() {
            if (g_ind != "N") {
                if ($('#auditPara_Period').val() == "") {
                    alert("Please enter Audit Period");
                    return false;
                }
            }

            if (!g_selectedRiskId || g_selectedRiskId == 0) {
                alert("Please select Audit Risk");
                return false;
            }
            if ($('#auditPara_Annex').val() == "") {
                alert("Please select Annexure");
                return false;
            }
            if ($('#auditPara_ParaNO').val() == "") {
                alert("Please enter Para No");
                return false;
            }
            if ($('#auditPara_Gist').val() == "") {
                alert("Please enter Para Gist");
                return false;
            }
            const payload = {
              COM_ID: Number(g_com_id) || 0,
              NEW_PARA_ID: Number(g_np_id) || 0,
              OLD_PARA_ID: Number(g_op_id) || 0,

              INDICATOR: g_ind || "",
              AUDIT_PERIOD: ($('#auditPara_Period').val() || "").trim(),

              OBS_GIST: $('#auditPara_Gist').val() || "",
              PARA_TEXT: $('#paraTextViewer').val() || "",
              PARA_NO: ($('#auditPara_ParaNO').val() || "").trim(),

              OBS_RISK_ID: Number(g_selectedRiskId) || 0,
              ANNEX_ID: Number($('#auditPara_Annex').val()) || 0,

              AMOUNT_INV: $('#auditPara_AmountInv').val() || "0",
              NO_INSTANCES: $('#auditPara_InstNO').val() || "0"
            };

            $.ajax({
              url: g_asiBaseURL + "/ApiCalls/update_para_for_manage_audit_paras",
              type: "POST",
              contentType: "application/json; charset=utf-8",
              dataType: "json",
              data: JSON.stringify(payload),
              cache: false,
              success: function (res) {
                $('#viewMemoModel').modal('hide');
                showApiAlert(res);
                onAlertCallback(getEntityObservation);
              },
              error: function (xhr) {
                console.log("update_para_for_manage_audit_paras failed", xhr.responseText);
                alert("Update failed. Check console for details.");
              }
            });



        }


        function DeleteDuplicatePara(np_id, op_id, ind) {
              g_np_id = np_id;
                g_op_id = op_id;
                g_ind = ind;
                $('#DuplicateParaModel').modal('show');


        }

        function ProceedDeleteDuplicatePara() {
              $.ajax({
                url: g_asiBaseURL + "/ApiCalls/request_delete_duplicate_para",
                type: "POST",
                data: {
                    'COM_ID': g_com_id,
                    'NEW_PARA_ID': g_np_id,
                    'OLD_PARA_ID': g_op_id,
                    'INDICATOR': g_ind,
                    'REMARKS': $('#textarea_justification').val()
                },
                cache: false,
                success: function (data) {
                    $("#DuplicateParaModel").modal('hide');
                    showApiAlert(data);
                    onAlertCallback(getEntityObservation);
                },
                dataType: "json",
            });
        }

        function openResponsiblePPs() {
            $('#ResponsiblePPModel').modal('show');
        }
              function updateRiskDisplay() {
                var annexId = $('#auditPara_Annex').val();
                var riskName = '';
                g_selectedRiskId = 0;
                $.each(g_annexList, function (i, v) {
                    var id = v.ID || v.id;
                    if (id == annexId) {
                        riskName = v.RISK || v.risk;
                        g_selectedRiskId = v.RISK_ID || v.risK_ID;
                    }
                });
                $('#viewMemo_risk_display').val(riskName);
                var color = '';
                if (riskName.toLowerCase() === 'high') {
                    color = 'red';
                } else if (riskName.toLowerCase() === 'medium') {
                    color = 'gold';
                } else if (riskName.toLowerCase() === 'low') {
                    color = 'green';
                }
                $('#viewMemo_risk_display').css('color', color);
            }

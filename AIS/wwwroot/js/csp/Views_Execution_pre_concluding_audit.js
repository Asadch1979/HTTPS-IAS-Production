window.addEventListener("error", function (e) {
    console.error("JS error:", e.message, e.filename, e.lineno, e.colno);
});
window.addEventListener("unhandledrejection", function (e) {
    console.error("Promise rejection:", e.reason);
});

function getPageData() {
    const input = document.getElementById("page-data-json");
    if (!input || !input.value) return {};
    try {
        return JSON.parse(input.value || "{}");
    } catch (err) {
        console.error("Failed to parse #page-data-json JSON:", err);
        return {};
    }
}

    var g_observationId = 0;
    var g_obsId = 0;
    var g_engId = 0;
    var g_respUsersArr = [];
    var g_procId = 0;
    var g_subProcId = 0;
    var g_procDetailId = 0;
    var g_selectedRiskId = 0;
    var respSection = null;
    var g_boBootstrapEngId = null;
    var pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];

    $(document).ready(function () {
        console.log("Loaded pre_concluding_audit JS", { g_obsId, g_engId });
        $('#preConcludingActionHandler').addClass("d-none");
            $('#viewMemo_memo_ObSent').richText({
             imageUpload: false,
             fileUpload: false,
             videoEmbed: false,
             urls: false
         });
        $('#viewMemo_annex_ObSent').on('change', updateRiskDisplay);
        respSection = initResponsibilitySection({
            tableSelector: '#update_listofRespPersons',
            changesTableSelector: '#c_update_listofRespPersons',
            modalSelector: '#ResponsiblePPModel',
            status: 7,
            directSaveMode: false,
            afterSave: function () {
                respSection.reload();
                viewObservationDetails(g_obsId, $('#checklistDetailsPanel tbody tr#obs_' + g_obsId + ' .preconcludingStatusField').text());
            }
        });



            document.getElementById('viewMemo_amount_ObSent').addEventListener('input', function (e) {
        this.value = this.value.replace(/\D|^0(?=\d)/g, ''); // Removes decimals and leading zeros
    });

     });

     function getEntityObservations() {
          var flag = 0;
         if($('#entitySelectField').val()==0){
              $('#checklistDetailsPanel tbody').empty();
             $('#preConcludingActionHandler').addClass("d-none");
              return;
         }
         g_engId = $('#entitySelectField').val();
         $('#engIdHidden').val(g_engId);
        // showBoApiDebugAlert('get_obs_for_pre_concluding', g_engId);
         if (respSection) {
             respSection.updateContext({ engId: parseInt(g_engId || 0) });
         }
         $.ajax({
             url: g_asiBaseURL + "/ApiCalls/get_obs_for_pre_concluding",
             type: "POST",
             data: {
                 'ENG_ID': g_engId
             },
             cache: false,
             success: function (data) {
                 $('#checklistDetailsPanel tbody').empty();
                 var sr = 1;
                 $.each(data, function (i, v) {

                     if (v.status.toLowerCase() == "completed")
                     {
                        $('#checklistDetailsPanel tbody').append('<tr id="obs_' + v.id + '"><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.heading + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="preconcludingStatusField">' + v.status + '</td><td><a href="#" data-onclick="viewObservationDetails(' + v.obS_ID + ', \''+v.status+'\');" class="text-hover text-success ml-5px"><small>View Details</small></a></td></tr>');
                     }
                     else{
                        $('#checklistDetailsPanel tbody').append('<tr id="obs_' + v.id + '"><td class="text-center">' + v.finaL_PARA_NO + '</td><td class="branchfield">' + v.heading + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_STATUS + '</td><td class="preconcludingStatusField">' + v.status + '</td><td><a href="#" data-onclick="viewObservationDetails(' + v.obS_ID + ', \''+v.status+'\');" class="text-hover text-success ml-5px"><small>View Details</small></a></td></tr>');
                        flag++;
                     }
                     sr++;
                 });

                 if (flag > 0) {
                     $('#preConcludingActionHandler').addClass("d-none");
                 } else if(flag==0  && data.length >0)  {
                     $('#preConcludingActionHandler').removeClass("d-none");
                 }
                 else{
                     $('#preConcludingActionHandler').addClass("d-none");
                 }
             },
             dataType: "json",
         });
     }

     function fieldAuditBoLoadPreConcluding(engId, readOnly) {
         if (!engId) {
             return;
         }

         var boEngId = String(engId);
         var selector = $('#entitySelectField');
         if (!selector.length) {
             return;
         }

         if (selector.find('option[value="' + boEngId + '"]').length === 0) {
             selector.append($('<option>', {
                 value: boEngId,
                 text: boEngId
             }));
         }

         selector.val(boEngId);
         selector.prop('disabled', true);
         $('#engIdHidden').val(boEngId);
         g_boBootstrapEngId = boEngId;

         if (readOnly) {
             $('#preConcludingActionHandler').addClass('d-none');
         }

         getEntityObservations();
     }

     window.fieldAuditBoLoadPreConcluding = fieldAuditBoLoadPreConcluding;

     function reloadLocation() {
         getEntityObservations();
     }

      function viewObservationDetails(obsId, status){
          g_obsId=obsId;
          $('#viewMemoDetailsModel').modal('show');

          if(status=="Pending"){
              $('#update_audit_obs_button').removeClass("d-none");
              $('#gist_recom_inc_pre_con').removeClass("d-none");

          }else{
              $('#update_audit_obs_button').addClass("d-none");
              $('#gist_recom_inc_pre_con').addClass("d-none");
          }

           $('#viewMemo_heading_ObSent').val('');
           $('#viewMemo_memo_ObSent').val('').trigger('change');
           $('#viewMemo_response_ObSent').html('');
           $('#viewMemo_aud_reply_ObSent').html('');
            $('#viewMemo_head_reply_ObSent').html('');
           $('#viewMemo_head_reply_ObSent').html('');
           $('#viewMemo_annex_ObSent').val(0);
           $('#viewMemo_risk_display').val('');
           g_selectedRiskId = 0;
           $('#viewMemo_process_ObSent').val(0);
            $('#viewMemo_amount_ObSent').val(0);
           $('#viewMemo_inst_ObSent').val(0);

             $('#gistPara_response_ObSent').val('');
           $('#audRecommend_response_ObSent').val('');

            $.ajax({
               url: g_asiBaseURL + "/ApiCalls/get_obs_details_by_id_pre_con",
              type: "POST",
              data: {
                  'OBS_ID': g_obsId
              },
              cache: false,
              success: function (data) {
           $('#viewMemo_heading_ObSent').val(data.heading);
           $('#viewMemo_memo_ObSent').val(data.observatioN_TEXT).trigger('change');
           $('#viewMemo_response_ObSent').html(data.auditeE_REPLY);
           ViewAuditeeAttachedEvidences();
           $('#viewMemo_aud_reply_ObSent').html(data.auditoR_RECOM);
            $('#viewMemo_head_reply_ObSent').html(data.heaD_RECOM);
           $('#viewMemo_annex_ObSent').val(data.annexurE_ID);
           g_selectedRiskId = data.riskmodeL_ID;
           updateRiskDisplay();
           $('#viewMemo_process_ObSent').val(data.procesS_ID);
           $('#viewMemo_paraNo_ObSent').val(data.finaL_PARA_NO);
           $('#viewMemo_amount_ObSent').val(data.amounT_INVOLVED);
           $('#viewMemo_inst_ObSent').val(data.nO_OF_INSTANCES);

           $('#gistPara_response_ObSent').val(data.qA_GIST);
           $('#audRecommend_response_ObSent').val(data.qA_RECOM);

           g_procId=data.procesS_ID;
           g_subProcId=data.subchecklisT_ID;
        g_procDetailId=data.checklistdetaiL_ID;
        getSubCheckListOfProcess();
        respSection.updateContext({
            newParaId: data.neW_PARA_ID || g_obsId,
            oldParaId: data.olD_PARA_ID || 0,
            indicator: data.indicator || '',
            comId: 0,
            engId: parseInt($('#engIdHidden').val() || 0),
            readOnly: false
        });
        //respSection.reload();
                 initReferenceSection(g_obsId, false, '#viewMemoDetailsModel #referenceSection');
                 },
              dataType: "json",
          });
      }
      function getSubCheckListOfProcess() {
          $('#viewMemo_subprocess_ObSent').empty();
          $.ajax({
              url: g_asiBaseURL + "/ApiCalls/get_audit_sub_checklist",
              type: "POST",
              data: {
                  'PROCESS_ID': $('#viewMemo_process_ObSent').val()
              },
              cache: false,
              success: function (data) {
                  $('#viewMemo_subprocess_ObSent').append('<option value="0">--Select Sub-Checklist--</option>');
                  $.each(data, function (i, v) {
                      $('#viewMemo_subprocess_ObSent').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                  });
                  if(g_subProcId!=0)
                      {
                          $('#viewMemo_subprocess_ObSent').val(g_subProcId);
                        getCheckListDetailOfSubProcess();
                      }


              },
              dataType: "json",
          });

      }
      function getCheckListDetailOfSubProcess() {
          var processId = $('#viewMemo_process_ObSent').val();
          var subProcessId = $('#viewMemo_subprocess_ObSent').val();
          if (processId != 0 && subProcessId !=0) {
                $('#viewMemo_checklist_ObSent').empty();
              $.ajax({
                  url: g_asiBaseURL + "/ApiCalls/get_audit_checklist_detail",
                  type: "POST",
                  data: {
                      'SUB_PROCESS_ID': subProcessId
                  },
                  cache: false,
                  success: function (data) {
                       $('#viewMemo_checklist_ObSent').append('<option value="0">--Select Checklist Detail--</option>');
                  $.each(data, function (i, v) {
                      $('#viewMemo_checklist_ObSent').append('<option value="' + v.s_ID + '">' + v.heading + '</option>');
                  });
                  if(g_procDetailId !=0)
                      $('#viewMemo_checklist_ObSent').val(g_procDetailId);

                  },
                  dataType: "json",
              });
          }

      }

      function updateRiskDisplay() {
          var annexId = $('#viewMemo_annex_ObSent').val();
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
        function ViewAuditeeAttachedEvidences() {
          $.ajax({
              url: g_asiBaseURL + "/ApiCalls/get_responded_obs_evidences",
              type: "POST",
              data: {
                  'OBS_ID': g_obsId
              },
              cache: false,
              success: function (data) {

                  $('#complianceCycleEvidences').empty();
                  if (data.length > 0) {
                      $.each(data, function (i, pp) {

                          var extension = pp.imagE_NAME.split('.').pop().toLowerCase();
                          const contentType = getContentType(extension);

                          // Create and append the attachment item
                          const container = document.createElement('div');
                          container.className = 'evidence-link';

                          // Add icon
                          const icon = document.createElement('i');
                          icon.className = getIconClass(extension) + ' evidence-icon mr-1';
                          container.appendChild(icon);

                          // Add label
                          const label = document.createElement('span');
                          label.innerText = pp.imagE_NAME;
                          label.classList.add('text-primary');

                          // Add cursor style to make it look like a link
                          label.style.cursor = 'pointer';
                          container.appendChild(label);

                          // Add click event to download file on selection
                          container.addEventListener('click', function () {
                              downloadFile(pp.filE_ID);
                          });

                          $('#complianceCycleEvidences').append(container);
                      });
                  }
                  else {
                      $('#complianceCycleEvidences').append("<i>No evidence is attached </i>");
                  }

              },
              dataType: "json",
          });
      }
      function downloadFile(id) {
          $.ajax({
              url: g_asiBaseURL + "/ApiCalls/get_auditee_evidence_data",
              type: "POST",
              data: {
                  'FILE_ID': id,
              },
              cache: false,
              success: function (data) {
                  var extension = data.imagE_NAME.split('.').pop().toLowerCase();
                  const contentType = getContentType(extension);

                  const blob = base64ToBlob(data.imagE_DATA, contentType);
                  const link = document.createElement('a');
                  link.href = URL.createObjectURL(blob);
                  link.download = data.imagE_NAME;
                  link.click(); // Trigger the download

              },
              dataType: "json",
          });


      }
      function getFileExtension(file) {
          var fileName = file.name;
          var extension = fileName.substring(fileName.lastIndexOf('.') + 1).toLowerCase();
          return extension;
      }
      function getIconClass(extension) {
          switch (extension) {
              case 'pdf': return 'fa fa-file-pdf';
              case 'zip': return 'fa fa-file-archive';
              case 'png':
              case 'jpg':
              case 'jpeg':
              case 'bmp': return 'fa fa-file-image';
              case 'doc':
              case 'docx': return 'fa fa-file-word';
              default: return 'fa fa-file';
          }
      }
      function getContentType(extension) {
          switch (extension) {
              case 'pdf': return 'application/pdf';
              case 'zip': return 'application/zip';
              case 'png': return 'image/png';
              case 'doc': return 'application/msword';
              case 'docx': return 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
              default: return 'application/octet-stream';
          }
      }
    function base64ToBlob(base64, contentType) {
        const byteCharacters = atob(base64);
        const byteArrays = [];

          for (let offset = 0; offset < byteCharacters.length; offset += 512) {
              const slice = byteCharacters.slice(offset, offset + 512);

              const byteNumbers = new Array(slice.length);
              for (let i = 0; i < slice.length; i++) {
                  byteNumbers[i] = slice.charCodeAt(i);
              }

              const byteArray = new Uint8Array(byteNumbers);
              byteArrays.push(byteArray);
          }

        const blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }
    function openResponsiblePPs() {
        $('#ResponsiblePPModel').modal('show');
    }
      function updateObservationDetails(obsId){

          $.ajax({
              url: g_asiBaseURL + "/ApiCalls/update_audit_para_for_finalization",
              type: "POST",
              data: {
                  'OBS_ID': obsId,
                  'ANNEX_ID': $('#viewMemo_annex_ObSent').val(),
                  'PROCESS_ID': $('#viewMemo_process_ObSent').val(),
                  'SUB_PROCESS_ID': $('#viewMemo_subprocess_ObSent').val(),
                  'PROCESS_DETAIL_ID': $('#viewMemo_checklist_ObSent').val(),
                  'RISK_ID': g_selectedRiskId,
                  'FINAL_PARA_NO': $('#viewMemo_paraNo_ObSent').val(),
                  'GIST_OF_PARA': $('#viewMemo_heading_ObSent').val(),
                  'TEXT_PARA': $('#viewMemo_memo_ObSent').val(),
                  'AMOUNT_INV': $('#viewMemo_amount_ObSent').val(),
                  'NO_INST': $('#viewMemo_inst_ObSent').val()

              },
              cache: false,
              success: function (data) {
                  showApiAlert(data);
                   onAlertCallback(reloadModel);
              },
              dataType: "json",
          });
          return;
      }

      function updateObservationDetailsWithReference(obsId){
          updateObservationDetails(obsId);
      }

      function reloadModel(){
          getEntityObservations();
          viewObservationDetails(g_obsId);
      }
     function saveObservationGistandRecommendation(){

         if ($('#gistPara_response_ObSent').val() == 0) {
             alert("Please enter Title of the Para to proceed");
             return;
         }

         if ($('#audRecommend_response_ObSent').val() == 0) {
             alert("Please enter Remarks / Root Cause/Corrective Measures to proceed");
              return;
         }

         $('#viewMemoModel_ObSent').modal('hide');

         $.ajax({
             url: g_asiBaseURL + "/ApiCalls/add_observation_gist_and_recommendation",
             type: "POST",
             data: {
                 'OBS_ID': g_obsId,
                 'AUDITOR_RECOMMENDATION': $('#audRecommend_response_ObSent').val(),
                 'GIST_OF_PARA': $('#gistPara_response_ObSent').val()

             },
             cache: false,
             success: function (data) {
                showApiAlert(data);
                   onAlertCallback(reloadModel);
             },
             dataType: "json",
         });

     }
     function submitPreConcluding() {

         var flag = 0;
         $('.preconcludingStatusField').each(function (i, v) {

             if ($(v).html() == "Pending")
                 flag++;
         });

         if (flag > 0) {
             alert("Please add Audit Observation Gist and Recommendation for all observations first to proceed");
             return;
         }

         $.ajax({
             url: g_asiBaseURL + "/ApiCalls/submit_pre_concluding",
             type: "POST",
             data: {
                 'ENG_ID': $('#entitySelectField option:selected').val()

             },
             cache: false,
             success: function (data) {
                 showApiAlert(data);
                  onAlertCallback(reloadLocation);

             },
             dataType: "json",
         });
     }

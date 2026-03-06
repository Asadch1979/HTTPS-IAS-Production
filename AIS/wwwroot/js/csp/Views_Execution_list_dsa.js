    var g_dsaId=0;
    $(document).ready(function () {
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_draft_dsa_list",
        type: "POST",
        data: {}, // Add data here if necessary
        cache: false,
        success: function (data) {
          if (data.length > 0) {
            var tbody = $('#listofRespPersons tbody');
            tbody.empty();
            destroyDatatable("listofRespPersons");
            $.each(data, function (index, item) {
              if ($('#hiddenUserRoleField').val() == 15) {
                var row = `
                  <tr>
                    <td>${index + 1}</td>
                    <td>${item.audiT_PERIOD || ''}</td>
                    <td>${item.reportinG_OFFICE || ''}</td>
                    <td>${item.entitY_NAME || ''}</td>
                    <td>${item.dsA_NO || ''}</td>
                    <td>${item.heading || ''}</td>
                    <td>${item.resP_PP_NO || ''}</td>
                    <td>${item.emP_NAME || ''}</td>
                    <td>${item.loaN_CASE || ''}</td>
                    <td>${item.lC_AMOUNT || ''}</td>
                    <td>${item.dsA_STATUS || ''}</td>
                    <td>
                      <a data-click="viewDSAContent(${item.id},1);" href="#" class="text-primary">View DSA Content</a>
                    </td>
                     <td>
                      <a data-click="updateDSAContent(${item.id},1);" href="#" class="text-primary">Update DSA Heading</a>
                    </td>
                    <td>
                      <a href="#" data-click="forwardSubmissionOfDSAToFAD(${item.id});" class="text-danger">${item.statuS_UP || ''}</a>
                    </td>
                  </tr>`;
                tbody.append(row);
              } else if ($('#hiddenUserRoleField').val() == 5) {
                var row = `
                  <tr>
                    <td>${index + 1}</td>
                    <td>${item.audiT_PERIOD || ''}</td>
                    <td>${item.aZ_NAME || ''}</td>
                    <td>${item.reportinG_OFFICE || ''}</td>
                    <td>${item.entitY_NAME || ''}</td>
                    <td>${item.dsA_NO || ''}</td>
                    <td>${item.heading || ''}</td>
                    <td>${item.resP_PP_NO || ''}</td>
                    <td>${item.emP_NAME || ''}</td>
                    <td>${item.loaN_CASE || ''}</td>
                    <td>${item.lC_AMOUNT || ''}</td>
                    <td>${item.dsA_STATUS || ''}</td>
                    <td>
                      <a data-click="viewDSAContent(${item.id},0);" href="#" class="text-primary">View Content</a>
                    </td>
                    <td>
                      <a href="#" data-click="refferedBackDSAToAZ(${item.id});" class="text-danger">${item.statuS_DOWN || ''}</a>
                    </td>
                    <td>
                      <a href="#" data-click="forwardSubmissionOfDSAToDPD(${item.id});" class="text-success">${item.statuS_UP || ''}</a>
                    </td>
                  </tr>`;
                tbody.append(row);
              }
              else if ($('#hiddenUserRoleField').val() == 12 && $('#hiddenUserEntityField').val() == "112259") {
                var row = `
                  <tr>
                    <td>${index + 1}</td>
                    <td>${item.audiT_PERIOD || ''}</td>
                    <td>${item.aZ_NAME || ''}</td>                    
                    <td>${item.reportinG_OFFICE || ''}</td>
                    <td>${item.entitY_NAME || ''}</td>
                    <td>${item.dsA_NO || ''}</td>
                    <td>${item.heading || ''}</td>
                    <td>${item.resP_PP_NO || ''}</td>
                    <td>${item.emP_NAME || ''}</td>
                    <td>${item.loaN_CASE || ''}</td>
                    <td>${item.lC_AMOUNT || ''}</td>
                    <td>${item.dsA_STATUS || ''}</td>
                    <td>
                      <a data-click="viewDSAContent(${item.id},0);" href="#" class="text-primary">View Content</a>
                    </td>
                    <td>
                      <a href="#" data-click="refferedBackDSAToHeadFAD(${item.id});" class="text-danger">${item.statuS_DOWN || ''}</a>
                    </td>
                    <td>
                      <a href="#" data-click="AcknowledgeDSAByDPD(${item.id});" class="text-success">${item.statuS_UP || ''}</a>
                    </td>
                  </tr>`;
                tbody.append(row);
              }
            });
            initializeDataTable("listofRespPersons");
          }
        },
        error: function (xhr, status, error) {
          console.error("Error fetching data: ", error);
        },
        dataType: "json",
      });
    });

    function viewDSAContent(id) {
        g_dsaId=id;
      $('#ViewDSAContent').modal('show');
      $('#updateHeadingButtonHandler').hide();
      $('#dsaHeading').attr("readonly",true);
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_dsa_content",
        type: "POST",
        data: { 'DSA_ID': id },
        cache: false,
        success: function (data) {
          $('#dsa_no_field').val(data.no);
          $('#dsaHeading').val(data.heading);
          $('#dsaContent').html(data.text);
        },
        dataType: "json",
      });
    }
      function updateDSAContent(id,editRight) {
          g_dsaId=id;
      $('#ViewDSAContent').modal('show');
      $('#updateHeadingButtonHandler').hide();
      $('#dsaHeading').attr("readonly",true);
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/get_dsa_content",
        type: "POST",
        data: { 'DSA_ID': g_dsaId },
        cache: false,
        success: function (data) {
          $('#dsa_no_field').val(data.no);
          $('#dsaHeading').val(data.heading);
          if(editRight==1){
              $('#dsaHeading').attr("readonly",false);
              $('#updateHeadingButtonHandler').show();
          }

          $('#dsaContent').html(data.text);
        },
        dataType: "json",
      });
    }
    //SVP AZ ACTION
    function forwardSubmissionOfDSAToFAD(id) {
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/submit_dsa_to_head_fad",
        type: "POST",
        data: { 'DSA_ID': id },
        cache: false,
        success: function (data) {
          showApiAlert(data);
          onAlertCallback(reloadLocation);
        },
        dataType: "json",
      });
    }
    //HEAD FAD ACTION
    function forwardSubmissionOfDSAToDPD(id) {
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/submit_dsa_to_dpd",
        type: "POST",
        data: { 'DSA_ID': id },
        cache: false,
        success: function (data) {
          showApiAlert(data);
          onAlertCallback(reloadLocation);
        },
        dataType: "json",
      });
    }

    function refferedBackDSAToAZ(id) {
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/reffered_back_by_head_fad",
        type: "POST",
        data: { 'DSA_ID': id },
        cache: false,
        success: function (data) {
          showApiAlert(data);
          onAlertCallback(reloadLocation);
        },
        dataType: "json",
      });
    }
    
    //SVP DPD
    function AcknowledgeDSAByDPD(id) {
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/acknowledge_dsa_by_dpd",
        type: "POST",
        data: { 'DSA_ID': id },
        cache: false,
        success: function (data) {
          showApiAlert(data);
          onAlertCallback(reloadLocation);
        },
        dataType: "json",
      });
    }

    function refferedBackDSAToHeadFAD(id) {
      $.ajax({
        url: g_asiBaseURL + "/ApiCalls/reffered_back_by_dpd",
        type: "POST",
        data: { 'DSA_ID': id },
        cache: false,
        success: function (data) {
          showApiAlert(data);
          onAlertCallback(reloadLocation);
        },
        dataType: "json",
      });
    }

    function updateDSAHeading(){
   
     $.ajax({
        url: g_asiBaseURL + "/ApiCalls/update_dsa_heading",
        type: "POST",
        data: {
            'DSA_ID': g_dsaId,
            'DSA_HEADING':  $('#dsaHeading').val()
        },
        cache: false,
        success: function (data) {
    $('#ViewDSAContent').modal('hide');
          showApiAlert(data);
          onAlertCallback(reloadLocation);
        },
        dataType: "json",
      });
    }

    function reloadLocation() {
      window.location.reload();
    }

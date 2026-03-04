    var g_engId=0;
    var g_respUser=[];
    function normalizeRequiredInt(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return 0;
        }
        var number = parseInt(trimmed, 10);
        return Number.isNaN(number) ? 0 : number;
    }

    function normalizeNullableInt(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return null;
        }
        var number = parseInt(trimmed, 10);
        return Number.isNaN(number) ? null : number;
    }
    $('#document').ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engId");
        $('#omcontent_field').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        $('#updatedAnnexlist').select2();

         document.getElementById('amount_inv_field').addEventListener('input', function (e) {
        this.value = this.value.replace(/\D|^0(?=\d)/g, ''); // Removes decimals and leading zeros
    });


    });


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
                url: g_asiBaseURL + "/ApiCalls/sub_checklist",
                type: "POST",
                data: {
                    'T_ID': $('#riskGroupSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $('#riskSubGroupSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                    $.each(data, function (index, item) {
                        $('#riskSubGroupSelectBox').append("<option value=\"" + item.s_ID + "\"> " + item.heading + " </option > ");
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
            url: g_asiBaseURL + "/ApiCalls/checklist_details",
            type: "POST",
            data: {
                'S_ID': $('#riskSubGroupSelectBox option:selected').val(),
            },
            cache: false,
            timeout: 300000,
            success: function (data) {
                $('#riskActivitiesSelectBox').append("<option value=\"0\" id=\"0\">--Select Sub Group--</option>");
                $.each(data, function (index, item) {
                    $('#riskActivitiesSelectBox').append("<option value=\"" + item.id + "\"> " + item.heading + "</option>");
                });

            },
            error: function (xhr, textStatus) {
                if (textStatus === "timeout") {
                    alert('Request taking longer than usual, please wait or refine search.');
                    return;
                }
                alert('Request failed. Please try again.');
            },
            dataType: "json",
        });
    }
    function getAuditObservationTemplate() {
        if ($('#riskActivitiesSelectBox option:selected').val() == 0)
            $('#omcontent_field').val('').trigger('change');
        else {
            $('#omcontent_field').val('').trigger('change');
            $.ajax({
                url: g_asiBaseURL + "/Execution/audit_observation_template",
                type: "POST",
                data: {
                    'ACTIVITY_ID': $('#riskActivitiesSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, item) {
                        $('#omcontent_field').val(item.obS_TEMPLATE).trigger('change');
                    });

                },
                dataType: "json",
            });
        }
    }

    function div_zoneBranchesShowHide() {
        if ($('#zoneSearchField option:selected').val() == 0) {
            $('#brSearchField').empty();
            $('#brSearchField').append('<option value="0" id="0">--Select Branch--</option>');
        }
        else {
            $('#brSearchField').empty();
            $('#brSearchField').append('<option value="0" id="0">--Select Branch--</option>')
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetZoneBranches",
                type: "POST",
                data: {
                    'zone_code': $('#zoneSearchField option:selected').val(),
                    'session_check':false
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, branch) {
                        $('#brSearchField').append('<option value="' + branch.branchcode + '" id="' + branch.branchcode + '">' + branch.description + '</option>')
                    });
                },
                dataType: "json",
            });
        }
    }

    function submitObservationToAuditee() {
        if ($('#updatedAnnexlist').val() == 0) {
            alert('Select Annexure');
            return;
        }
        if ($('#riskGroupSelectBox').val() == 0) {
            alert('Select Violation Category');
            return;
        }
        if ($('#riskSubGroupSelectBox').val() == 0) {
            alert('Select Violation Nature');
            return;
        }
        if ($('#riskActivitiesSelectBox').val() == 0) {
            alert('Select Sub Checklist Detail');
            return;
        }


        if ($('#viewMemo_risk').val() == 0) {
           alert('Select Risk');
           return;
        }
        if ($('#viewMemo_heading').val() == 0) {
            alert('Please Enter Para Heading');
            return;
        }

        if($('#amount_inv_field').val()  == ""){
            alert('Please Enter Amount Involved, in case of blank please enter 0');

            return;
        }

         if($('#no_instances_field').val()  ==""){
            alert('Please Enter No. of Instances, in case of blank please enter 0');
            return;
        }
        var resP = [];
        $.each($('#listofRespPersons tbody tr'), function (i, v) {
            resP.push({
                'PP_NO': normalizeRequiredInt($(v).attr('id').split('tr_')[1]),
                'EMP_NAME': $(v).find('td').eq(2).html(),
                'LOAN_CASE': normalizeNullableInt($(v).find('td').eq(3).html()),
                'LC_AMOUNT': normalizeNullableInt($(v).find('td').eq(4).html()),
                'ACCOUNT_NUMBER': normalizeRequiredInt($(v).find('td').eq(5).html()),
                'ACC_AMOUNT': normalizeRequiredInt($(v).find('td').eq(6).html())
            });
        });
        var g_memoObj = [];
        var memo = {
            'MEMO': $('.richText-editor').html(),
            'ID': 'obs_0',
            'HEADING': $('#viewMemo_heading').val(),
            'RISK': $('#viewMemo_risk').val(),
            'AMOUNT_INVOLVED': $('#amount_inv_field').val(),
            'NO_OF_INSTANCES': $('#no_instances_field').val(),
            'DAYS': $('#viewMemo_replydays option:selected').val(),
            'LOANCASE': '',
            'RESPONSIBLE_PPNO': resP,
            'ATTACHMENTS': ''
        };
        g_memoObj.push(memo);
        $('#submitCAUobBtn').attr('disabled', true);
        var payload = {
            'LIST_OBS': g_memoObj,
            'ENG_ID': g_engId,
            'BRANCH_ID': $('#brSearchField').val(),
            'SUB_CHECKLISTID': $('#riskSubGroupSelectBox').val(),
            'CHECKLIST_ID': $('#riskActivitiesSelectBox').val(),
            'ANNEXURE_ID': $('#updatedAnnexlist').val()

        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/save_observations_cau",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            cache: false,
            success: function (data) {
                $('#submitCAUobBtn').attr('disabled', false);
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });

    }

    function reloadLocation() {
        window.location.reload();
    }

    function openResponsiblePPs() {
        $('#ResponsiblePPModel').modal('show');
        $('#matchedPPNoPanels').empty();
        $('#matchedPPNoPanelsBYPP').empty();
      //  $('#findBYPPNOPanel').addClass('d-none');
        return false;
    }
  
     function getMatchedPP() {
          $('#matchedPPNoPanelsBYPP').empty();
          if($('#responsiblePPNoEntryField')==""){
              alert("Please enter PP Number to proceed");
              return;
          }
          g_respUser = [];
          $.ajax({
              url: g_asiBaseURL + "/ApiCalls/get_employee_name_from_pp",
              type: "POST",
              data: {
                  'PP_NO': normalizeRequiredInt($('#responsiblePPNoEntryField').val())
              },
              cache: false,
              success: function (data) {
                  g_respUser.push(data);
                  if (data.ppNumber > 0) {
                       $('#matchedPPNoPanelsBYPP').append(`
                        <!-- Responsible Persons Heading -->
            <div class="row col-md-12 mt-2">
                <div class="col-sm-1 font-weight-bold">P.P. No</div>
                <div class="col-sm-3 font-weight-bold">Name</div>
                <div class="col-sm-2 font-weight-bold">Acc No.</div>
                <div class="col-sm-2 font-weight-bold">Acc Amount</div>
                <div class="col-sm-1 font-weight-bold">LC No.</div>
                <div class="col-sm-2 font-weight-bold">LC Amount</div>
                <div class="col-sm-1 font-weight-bold">Action</div>
            </div>
            <hr class="row col-md-12 mt-3" />
          <div class="row col-md-12 mt-2">
           <div class="col-sm-1"><span>${$('#responsiblePPNoEntryField').val()}</span></div>
              <div class="col-sm-3"><span>${data.name}</span></div>
              <div class="col-sm-2"><span>${$('#responsibleAccountNumberEntryField').val()}</span></div>
              <div class="col-sm-2"><span>${$('#responsibleAccountAmountEntryField').val()}</span></div>
              <div class="col-sm-1"><span>${$('#responsibleLoanNumberEntryField').val()}</span></div>
              <div class="col-sm-2"><span>${$('#responsibleLoanAmountEntryField').val()}</span></div>
              <div class="col-sm-1">
                  <input style="margin-left:10px;" class="respCheckBOXBYPP" type="checkbox" />
              </div>
          </div>
      `);                  }

              },
              dataType: "json",
          });
      }

      function getLCDetails(){
         $('#matchedPPNoPanels').empty();
        // $('#findBYPPNOPanel').removeClass('d-none');
        g_respUser = [];
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_lc_details",
            type: "POST",
            data: {
                'LC_NO': $('#responsibleLCNoEntryField').val(),
                'BR_CODE': $('#responsibleBrCodeEntryField').val()
            },
            cache: false,
            success: function (data) {
                var response =data;

                      response.forEach((data) => {
        const responsiblePersons = [
            { label: "MCO", ppno: data.mcoPPNo, name:data.mcoName },
            { label: "Manager", ppno: data.managerPPNo, name:data.managerName },
            { label: "RGM", ppno: data.rgmPPNo, name:data.rgmName },
            { label: "CAD Reviewer", ppno: data.cadReviewerPPNo, name:data.cadReviewerName },
            { label: "CAD Authorizer", ppno: data.cadAuthorizerPPNo , name:data.cadAuthorizerName }
        ].filter(person => person.ppno);
          const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        const [year, month, day] = dateString.split("T")[0].split("-");
        return `${day}/${month}/${year}`;
    };

        $('#matchedPPNoPanels').append(`
        <hr class="row col-md-12 mt-1"/> <div class="row loan-case-panel">
            <!-- Loan Case Details -->
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>Name</label></div>
                <div class="col-md-8"><input class="form-control" type="text" value="${data.name}" readonly /></div>
            </div>
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>CNIC</label></div>
                <div class="col-md-8"><input class="form-control" type="text" value="${data.cnic}" readonly /></div>
            </div>
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>Loan Case No</label></div>
                <div class="col-md-8"><input id="resp_loan_case" class="form-control" type="text" value="${data.loanCaseNo}" readonly /></div>
            </div>

            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>Application Date</label></div>
                <div class="col-md-8"><input class="form-control" type="text" value="${formatDate(data.appDate)}" readonly /></div>
            </div>
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>CAD Receive Date</label></div>
                <div class="col-md-8"><input class="form-control" type="text" value="${formatDate(data.cadReceiveDate)}" readonly /></div>
            </div>
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>Sanction Date</label></div>
                <div class="col-md-8"><input class="form-control" type="text" value="${formatDate(data.sanctionDate)}" readonly /></div>
            </div>          
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>Disbursed Amount</label></div>
                <div class="col-md-8"><input  class="form-control" type="text" value="${data.disbursedAmount}" readonly /></div>
            </div>
            <div class="row col-md-12 mt-2">
                <div class="col-md-4"><label>Outstanding Amount</label></div>
                <div class="col-md-8"><input id="resp_loan_amount" class="form-control" type="text" value="${data.outstandingAmount}" readonly /></div>
            </div>
            <hr class="row col-md-12 mt-3" />
            <!-- Responsible Persons Heading -->
            <div class="row col-md-12 mt-2">
                <div class="col-sm-3 font-weight-bold">Role</div>
                <div class="col-sm-3 font-weight-bold">P.P. No</div>
                <div class="col-sm-3 font-weight-bold">Name</div>
                <div class="col-sm-3 font-weight-bold">Action</div>
            </div>
            <hr class="row col-md-12 mt-3" />
            <!-- Responsible Persons -->
            ${responsiblePersons.map(person => `
                <div class="row col-md-12 mt-2">
                    <div class="col-sm-3"><label>${person.label}</label></div>
                    <div class="col-sm-3"><span>${person.ppno}</span></div>
                    <div class="col-sm-3"><span>${person.name}</span></div>
                    <div class="col-sm-3">
                        <input style="margin-left:10px;" class="respCheckBOX" type="checkbox" />
                    </div>
                </div>
            `).join('')}
        </div>
    `);
    });
            },
            dataType: "json",
        });
    }

    function deleteRespRow(e) {
        $(e).parent().parent().remove();
    }
    
      function addResponsibilityToMainTable() {
        g_respUser = []; // Clear the existing user array for fresh addition

        // Loop through each checked checkbox
        $('.respCheckBOX:checked').each(function() {
            const row = $(this).closest('.row'); // Get the closest row to the checkbox
            const ppNumber = row.find('span').eq(0).text(); // Get P.P. No (second span)
            const name = row.find('span').eq(1).text(); // Get Name (third span)

            var srNo = $('#listofRespPersons tbody tr').length + 1; // Increment Serial Number
                $('#listofRespPersons tbody').append(`
                    <tr id="tr_${ppNumber}">
                        <td>${srNo}</td>
                        <td>${ppNumber}</td>
                        <td>${name}</td>
                        <td>${$('#resp_loan_case').val()}</td>
                        <td>${$('#resp_loan_amount').val()}</td>
                        <td></td>
                        <td></td>
                        <td class="text-center">
                            <a href="#" data-onclick="event.preventDefault(); deleteRespRow(this);">Delete</a>
                        </td>
                    </tr>
                `);
                g_respUser.push({ ppNumber, name }); // Store the user details if needed

        });

         $('.respCheckBOXBYPP:checked').each(function() {
            const row = $(this).closest('.row');
            const ppNumber = row.find('span').eq(0).text();
            const name = row.find('span').eq(1).text();
            const acc_no = row.find('span').eq(2).text();
            const acc_amt = row.find('span').eq(3).text();
            const lc_no = row.find('span').eq(4).text();
            const lc_amt = row.find('span').eq(5).text();
            var srNo = $('#listofRespPersons tbody tr').length + 1; // Increment Serial Number
                $('#listofRespPersons tbody').append(`
                    <tr id="tr_${ppNumber}">
                        <td>${srNo}</td>
                        <td>${ppNumber}</td>
                        <td>${name}</td>
                        <td>${lc_no}</td>
                        <td>${lc_amt}</td>
                         <td>${acc_no}</td>
                        <td>${acc_amt}</td>
                        <td class="text-center">
                            <a href="#" data-onclick="event.preventDefault(); deleteRespRow(this);">Delete</a>
                        </td>
                    </tr>
                `);
                g_respUser.push({ ppNumber, name }); // Store the user details if needed

        });
    }

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

    var g_engId = 0;
    var g_respUser = [];
    var g_stagedResp = [];
    var g_selectedRespRow = null;
    var g_selectedRiskId = 0;
    const pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];

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
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engId = url.searchParams.get("engId");
        console.log("Loaded cau_observation JS", { g_engId });
        $('#template_box').richText({
            bold: true,
            italic: true,
            underline: true,
            leftAlign: true,
            centerAlign: true,
            rightAlign: true,
            justify: true,
            ol: true,
            ul: true,
            heading: true,
            fonts: true,
            fontColor: true,
            fontSize: true,
            table: true,
            removeStyles: true,
            code: true,
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });

        $('#updatedAnnexlist').select2();
        $('#riskActivitiesSelectBox').select2();
        $('#updatedAnnexlist').on('change', updateRiskDisplay);

         document.getElementById('amount_inv_field').addEventListener('input', function (e) {
        this.value = this.value.replace(/\D|^0(?=\d)/g, ''); // Removes decimals and leading zeros
    });

    });

    function buildRespKey(ppNo, role, loanCase, accountNumber) {
        return `${ppNo || ''}|${role || ''}|${loanCase || ''}|${accountNumber || ''}`;
    }

    function renderPendingGrid() {
        var $tbody = $('#respPendingTable tbody');
        $tbody.empty();
        g_stagedResp.forEach(function (item) {
            $tbody.append(`
                <tr>
                    <td>${item.role || ''}</td>
                    <td>${item.ppNo || ''}</td>
                    <td>${item.empName || ''}</td>
                    <td>${item.loanCase || ''}</td>
                    <td>${item.lcAmount || ''}</td>
                    <td>${item.accountNumber || ''}</td>
                    <td>${item.accAmount || ''}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-danger respPendingRemove" data-key="${item.key}">Remove</button>
                    </td>
                </tr>
            `);
        });
    }

    function stagePending(item) {
        if (!item) {
            return;
        }
        var key = item.key || buildRespKey(item.ppNo, item.role, item.loanCase, item.accountNumber);
        item.key = key;
        var exists = g_stagedResp.some(function (entry) { return entry.key === key; });
        if (exists) {
            alert('This responsibility is already added.');
            return;
        }
        g_stagedResp.push(item);
    }

    function clearPending() {
        g_stagedResp = [];
        renderPendingGrid();
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

                if ($.fn.select2) {
                    if ($('#riskActivitiesSelectBox').data('select2')) {
                        $('#riskActivitiesSelectBox').select2('destroy');
                    }
                    $('#riskActivitiesSelectBox').select2();
                }
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
            $('#template_box').val('').trigger('change');
        else {
            $('#template_box').val('').trigger('change');
            $.ajax({
                url: g_asiBaseURL + "/Execution/audit_observation_template",
                type: "POST",
                data: {
                    'ACTIVITY_ID': $('#riskActivitiesSelectBox option:selected').val(),
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, item) {
                        $('#template_box').val(item.obS_TEMPLATE).trigger('change');
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


        updateRiskDisplay();
        if (g_selectedRiskId == 0) {
            alert('Risk not available for selected Annexure');
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
                'PP_NO': normalizeRequiredInt($(v).data('pp')),
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
            'RISK': g_selectedRiskId,
            'ANNEXURE_ID': $('#updatedAnnexlist').val(),
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

    function updateRiskDisplay() {
        var annexId = $('#updatedAnnexlist').val();
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

    function openResponsiblePPs() {
        $('#ResponsiblePPModel').modal('show');
        clearPending();
        g_selectedRespRow = null;
        $('#responsibleLCNoEntryField').val('');
        $('#responsibleBrCodeEntryField').val('');
        $('#responsiblePPNoEntryField').val('');
        $('#responsibleLoanNumberEntryField').val('');
        $('#responsibleLoanAmountEntryField').val('');
        $('#responsibleAccountNumberEntryField').val('');
        $('#responsibleAccountAmountEntryField').val('');
        $('#addResponsibleButton').removeClass("d-none");
        $('#updateResponsibleButton').addClass("d-none");
        $('#deleteResponsibleButton').addClass("d-none");
        return false;
    }
  
     function getMatchedPP() {
          if ($.trim($('#responsiblePPNoEntryField').val()) === "") {
              alert("Please enter PP Number to proceed");
              return;
          }
          g_respUser = [];
          $.ajax({
              url: g_asiBaseURL + "/ApiCalls/get_responsible_by_pp",
              type: "POST",
              data: {
                  'PP_NO': normalizeRequiredInt($('#responsiblePPNoEntryField').val())
              },
              cache: false,
              success: function (data) {
                  var items = Array.isArray(data) ? data : (data ? [data] : []);
                  if (!items.length) {
                      alert("No record found..");
                      return;
                  }
                  items.forEach(function (item) {
                      var ppNo = item.ppNo || item.PP_NO || item.ppNumber || $('#responsiblePPNoEntryField').val();
                      var empName = item.name || item.empName || item.EMP_NAME || '';
                      var role = item.role || item.ROLE || item.respRole || '';
                      var loanCase = item.loanCase || item.LOAN_CASE || item.loanCaseNo || $('#responsibleLoanNumberEntryField').val();
                      var lcAmount = item.lcAmount || item.LC_AMOUNT || item.loanAmount || $('#responsibleLoanAmountEntryField').val();
                      var accountNumber = item.accountNumber || item.ACCOUNT_NUMBER || $('#responsibleAccountNumberEntryField').val();
                      var accAmount = item.accAmount || item.ACC_AMOUNT || $('#responsibleAccountAmountEntryField').val();
                      stagePending({
                          role: role,
                          ppNo: ppNo,
                          empName: empName,
                          loanCase: loanCase,
                          lcAmount: lcAmount,
                          accountNumber: accountNumber,
                          accAmount: accAmount
                      });
                  });
                  renderPendingGrid();
              },
              dataType: "json",
          });
      }

      function getLCDetails(){
        g_respUser = [];
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_responsible_by_lc",
            type: "POST",
            data: {
                'LC_NO': $('#responsibleLCNoEntryField').val(),
                'BR_CODE': $('#responsibleBrCodeEntryField').val()
            },
            cache: false,
            success: function (data) {
                var response = Array.isArray(data) ? data : (data ? [data] : []);
                if (!response.length) {
                    alert("No record found..");
                    return;
                }
                response.forEach(function (item) {
                    var ppNo = item.ppNo || item.PP_NO || item.ppNumber || '';
                    var empName = item.name || item.empName || item.EMP_NAME || '';
                    var role = item.role || item.ROLE || item.respRole || '';
                    var loanCase = item.loanCase || item.LOAN_CASE || item.loanCaseNo || $('#responsibleLCNoEntryField').val();
                    var lcAmount = item.lcAmount || item.LC_AMOUNT || item.outstandingAmount || '';
                    var accountNumber = item.accountNumber || item.ACCOUNT_NUMBER || item.accountNo || '';
                    var accAmount = item.accAmount || item.ACC_AMOUNT || item.accountAmount || '';
                    stagePending({
                        role: role,
                        ppNo: ppNo,
                        empName: empName,
                        loanCase: loanCase,
                        lcAmount: lcAmount,
                        accountNumber: accountNumber,
                        accAmount: accAmount
                    });
                });
                renderPendingGrid();
            },
            dataType: "json",
        });
    }

    function deleteRespRow(e) {
        if (!confirm('Are you sure you want to delete this responsibility?')) {
            return;
        }
        $(e).closest('tr').remove();
    }

    function updateRespRow(e) {
        var $row = $(e).closest('tr');
        g_selectedRespRow = $row;
        $('#ResponsiblePPModel').off('shown.bs.modal.update').on('shown.bs.modal.update', function () {
            $('#addResponsibleButton').addClass("d-none");
            $('#updateResponsibleButton').removeClass("d-none");
            $('#deleteResponsibleButton').removeClass("d-none");
            var lcNo = $.trim($row.data('loan') || $row.children('td').eq(3).text()) || '0';
            var lcAmt = $.trim($row.data('lcamount') || $row.children('td').eq(4).text()) || '0';
            var accNo = $.trim($row.data('account') || $row.children('td').eq(5).text()) || '0';
            var accAmt = $.trim($row.data('accamount') || $row.children('td').eq(6).text()) || '0';
            $('#responsiblePPNoEntryField').val($row.data('pp') || $row.children('td').eq(1).text());
            $('#responsibleLoanNumberEntryField').val(lcNo);
            $('#responsibleLoanAmountEntryField').val(lcAmt);
            $('#responsibleAccountNumberEntryField').val(accNo);
            $('#responsibleAccountAmountEntryField').val(accAmt);
            clearPending();
            stagePending({
                role: '',
                ppNo: $row.data('pp') || $row.children('td').eq(1).text(),
                empName: $row.data('emp') || $row.children('td').eq(2).text(),
                loanCase: lcNo,
                lcAmount: lcAmt,
                accountNumber: accNo,
                accAmount: accAmt
            });
            renderPendingGrid();
            $(this).off('shown.bs.modal.update');
        }).modal('show');
    }
    
      function addResponsibilityToMainTable(action) {
        if (!g_stagedResp.length && action !== 'D') {
            alert('Select at least one responsible');
            return;
        }
        if (action === 'U') {
            if (!g_selectedRespRow) {
                alert('Please select a responsibility to update.');
                return;
            }
            var item = g_stagedResp[0];
            $(g_selectedRespRow)
                .attr('data-pp', item.ppNo || '')
                .attr('data-loan', item.loanCase || '')
                .attr('data-account', item.accountNumber || '')
                .attr('data-lcamount', item.lcAmount || '')
                .attr('data-accamount', item.accAmount || '')
                .attr('data-emp', item.empName || '')
                .children('td').eq(1).text(item.ppNo || '').end()
                .children('td').eq(2).text(item.empName || '').end()
                .children('td').eq(3).text(item.loanCase || '').end()
                .children('td').eq(4).text(item.lcAmount || '').end()
                .children('td').eq(5).text(item.accountNumber || '').end()
                .children('td').eq(6).text(item.accAmount || '');
            g_selectedRespRow = null;
        } else if (action === 'D') {
            if (!g_selectedRespRow) {
                alert('Please select a responsibility to delete.');
                return;
            }
            if (confirm('Are you sure you want to delete this responsibility?')) {
                $(g_selectedRespRow).remove();
                g_selectedRespRow = null;
            }
        } else {
            g_stagedResp.forEach(function (item) {
                var srNo = $('#listofRespPersons tbody tr').length + 1;
                $('#listofRespPersons tbody').append(`
                    <tr data-pp="${item.ppNo || ''}" data-loan="${item.loanCase || ''}" data-account="${item.accountNumber || ''}" data-lcamount="${item.lcAmount || ''}" data-accamount="${item.accAmount || ''}" data-emp="${item.empName || ''}">
                        <td>${srNo}</td>
                        <td>${item.ppNo || ''}</td>
                        <td>${item.empName || ''}</td>
                        <td>${item.loanCase || ''}</td>
                        <td>${item.lcAmount || ''}</td>
                        <td>${item.accountNumber || ''}</td>
                        <td>${item.accAmount || ''}</td>
                        <td class="text-center">
                            <a href="#" data-onclick="event.preventDefault(); updateRespRow(this);">Update</a>
                        </td>
                        <td class="text-center">
                            <a href="#" class="text-danger" data-onclick="event.preventDefault(); deleteRespRow(this);">Delete</a>
                        </td>
                    </tr>
                `);
            });
        }
        clearPending();
        $('#addResponsibleButton').removeClass('d-none');
        $('#updateResponsibleButton').addClass('d-none');
        $('#deleteResponsibleButton').addClass('d-none');
        $('#ResponsiblePPModel').modal('hide');
    }

    $(document).on('click', '.respPendingRemove', function () {
        var key = $(this).data('key');
        g_stagedResp = g_stagedResp.filter(function (entry) { return entry.key !== key; });
        renderPendingGrid();
    });

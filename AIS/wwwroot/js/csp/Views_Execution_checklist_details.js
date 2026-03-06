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

    var g_respUser = [];
    var g_memoObj = [];
    var g_observationId = 0;
    var g_engId = 0;
    var S_ID = 0;
    var g_respUsersArr = [];
    var g_stagedResp = [];
    var g_selectedRespRow = null;

    var g_selectedRiskId = 0;
    const pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var checklistsub_id = url.searchParams.get("id");
        S_ID = checklistsub_id;
        g_engId = url.searchParams.get("engId");
        console.log("Loaded checklist_details JS", { S_ID, g_engId });
        $('#updatedAnnexlist').select2({ dropdownParent: $('#viewMemoModel') });
        $('#updatedAnnexlist').on('change', updateRiskDisplay);


        $(document).on('change', '.checklistaction', function () {
            showObservationArea($(this).val(), $(this).data('observation-id'));
        });

        $(document).on('click', '.js-view-created-memo', function (event) {
            event.preventDefault();
            viewCreatedMemo(this, $(this).data('observation-id'));
        });

        $(document).on('click', '.js-update-resp-row', function (event) {
            event.preventDefault();
            updateRespRow(this);
        });

        $(document).on('click', '.js-delete-resp-row', function (event) {
            event.preventDefault();
            deleteRespRow(this);
        });



        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/checklist_details",
            type: "POST",
            data: {
                'S_ID': checklistsub_id
            },
            cache: false,
            timeout: 300000,
            success: function (data) {
                console.log('subhcekclist', data);
                $('#checklistDetailsPanel tbody').empty();
                var sr = 1;
                $.each(data, function (i, v) {
                    $('#checklistDetailsPanel tbody').append('<tr id="obs_' + v.id + '"><td>' + sr + '</td><td>' + v.s_NAME + '</td><td>' + v.v_NAME + '</td><td>' + v.heading + '</td><td><select id="checklistaction_' + v.id + '" class="checklistaction form-select form-control" data-observation-id="obs_' + v.id + '" aria-label="Default select example"><option value="-1" id="-1" selected>--Please Select--</option><option value="0" id="0">No</option><option value="1" id="1">Yes</option></select></td><td id="actionTd_' + v.id + '" class="text-center"><a href="#" class="text-center text-danger js-view-created-memo" data-observation-id="obs_' + v.id + '">View Memo</a></td></tr>');
                    sr++;
                });
                getSubCheckListStatus();
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
        $('#viewMemo_memo').richText({
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
            document.getElementById('viewMemo_amountInv').addEventListener('input', function (e) {
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
    function showObservationArea(value, id) {
        g_observationId = id;
        if (value == 1) {
            $('#viewMemoModel').modal('show');
            $('.richText-editor').html('');
            $('#viewMemo_heading').val('');
            $('#updatedAnnexlist').val('0');
            g_selectedRiskId = 0;
            $('#viewMemo_risk_display').val('');
            $('#viewMemo_replydays').val(1);
            $('#viewMemo_loancase').val('');
            $('#listofRespPersons tbody').empty();
            $('#viewMemo_attachments').val('');
        }
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

    function saveMemoContent() {
        if ($('#updatedAnnexlist').val() == 0) {
            alert("Please select Annexure");
            return false;
        }
        if ($('#viewMemo_heading').val()==""){
            alert("Please Enter Para Heading");
            return false;
        }
        updateRiskDisplay();
        if (g_selectedRiskId == 0) {
            alert("Risk not available for selected Annexure");
            return false;
        }
         if($('#viewMemo_amountInv').val()  == ""){
            alert('Please Enter Amount Involved, in case of blank please enter 0');

            return;
        }

         if($('#viewMemo_noinstances').val()  ==""){
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
        var memo = {
            'HEADING': $('#viewMemo_heading').val(),
            'RISK': g_selectedRiskId,
            'ANNEXURE_ID': $('#updatedAnnexlist').val(),
            'MEMO': $('.richText-editor').html(),
            'ID': g_observationId,
            'DAYS': $('#viewMemo_replydays option:selected').val(),
            'NO_OF_INSTANCES': $('#viewMemo_noinstances').val(),
            'AMOUNT_INVOLVED': $('#viewMemo_amountInv').val(),
            'LOANCASE': $('#viewMemo_loancase').val(),
            'ATTACHMENTS': $('#viewMemo_attachments').val(),
            'RESPONSIBLE_PPNO': resP,
            'RESP_TABLE_ROWS': g_respUsersArr
        };
        var isFound = false;
        $.each(g_memoObj, function (i, v) {
            if (v.ID == g_observationId) {
                isFound = true;
                g_memoObj[i].MEMO = memo.MEMO;
                g_memoObj[i].HEADING = memo.HEADING;
                g_memoObj[i].RISK = memo.RISK;
                g_memoObj[i].ANNEXURE_ID = memo.ANNEXURE_ID;
                g_memoObj[i].ID = memo.ID;
                g_memoObj[i].LOANCASE = memo.LOANCASE;
                g_memoObj[i].NO_OF_INSTANCES = memo.NO_OF_INSTANCES;
                g_memoObj[i].AMOUNT_INVOLVED = memo.AMOUNT_INVOLVED;
                g_memoObj[i].DAYS = memo.DAYS;
                g_memoObj[i].ATTACHMENTS = memo.ATTACHMENTS;
                g_memoObj[i].RESPONSIBLE_PPNO = memo.RESPONSIBLE_PPNO;
                g_memoObj[i].RESP_TABLE_ROWS = memo.RESP_TABLE_ROWS;
            }
        });

        if (!isFound)
            g_memoObj.push(memo);

        $('#viewMemoModel').modal('hide');
        var payload = {
            'LIST_OBS': [memo],
            'ENG_ID': g_engId,
            'S_ID': S_ID
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/save_observations",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }

    function viewCreatedMemo(e, id) {
        g_observationId = id;
        var value = $($(e).parent().parent().find('.checklistaction').eq(0)).val();

        var tempobj = {
            ID: '0',
            'MEMO': '',
            'ANNEXURE_ID': '0',
            'HEADING': '',
            'RISK': '',
            'DAYS': '',
            'LOANCASE': '',
            'NO_OF_INSTANCES': '0',
            'AMOUNT_INVOLVED': '0',
            'ATTACHMENTS': '',
            'RESPONSIBLE_PPNO': [],
            'RESP_TABLE_ROWS': [],
        };
        if (value == 1) {
            $.each(g_memoObj, function (i, v) {
                if (v.ID == id) {
                    tempobj = v;
                }
            });
        }
        if (tempobj.ID != 0) {
            $('#viewMemoModel').modal('show');
            $('.richText-editor').html(tempobj.MEMO);
            $('#viewMemo_heading').val(tempobj.HEADING);
            $('#updatedAnnexlist').val(tempobj.ANNEXURE_ID).trigger('change');
            $('#viewMemo_replydays').val(tempobj.DAYS);
            $('#viewMemo_loancase').val(tempobj.LOANCASE);
            $('#viewMemo_noinstances').val(tempobj.NO_OF_INSTANCES);
            $('#viewMemo_amountInv').val(tempobj.AMOUNT_INVOLVED);
            $('#viewMemo_attachments').val(tempobj.ATTACHMENTS);
            if (tempobj.RESPONSIBLE_PPNO.length > 0) {
                $.each(tempobj.RESPONSIBLE_PPNO, function (j, pp) {
                    var srNo = $('#listofRespPersons tbody tr').length;
                    srNo++;
                    $('#listofRespPersons tbody').append('<tr data-pp="' + (pp.PP_NO || '') + '" data-loan="' + (pp.LOAN_CASE || '') + '" data-account="' + (pp.ACCOUNT_NUMBER || '') + '" data-lcamount="' + (pp.LC_AMOUNT || '') + '" data-accamount="' + (pp.ACC_AMOUNT || '') + '" data-emp="' + (pp.EMP_NAME || '') + '"><td>' + srNo + '</td><td>' + pp.PP_NO + '</td><td>' + pp.EMP_NAME + '</td><td>' + pp.LOAN_CASE + '</td><td>' + pp.LC_AMOUNT + '</td><td>' + pp.ACCOUNT_NUMBER + '</td><td>' + pp.ACC_AMOUNT + '</td><td class="text-center"><a href="#" class="js-update-resp-row">Update</a></td><td class="text-center"><a href="#" class="text-danger js-delete-resp-row">Delete</a></td></tr>');
                });
            }
        } else {
            alert("Please select Yes to create Observation");
            return;
        }
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
                            <a href="#" class="js-update-resp-row">Update</a>
                        </td>
                        <td class="text-center">
                            <a href="#" class="text-danger js-delete-resp-row">Delete</a>
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
    function getSubCheckListStatus() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_subchecklist_status",
            type: "POST",
            data: {
                'ENG_ID': g_engId,
                'S_ID': S_ID
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    if (v.status == 'Y') {
                        $('#checklistaction_' + v.cD_ID).val(1);
                        $('#checklistaction_' + v.cD_ID).attr('disabled', true);
                        $('#actionTd_'+v.cD_ID).empty();
                        $('#actionTd_' + v.cD_ID).append('<a class="text-center text-danger" data-onclick= "ObservationViewerPanel(' + v.obS_ID + ')"> View Memo </a>');

                    }
                    else if (v.status == 'N') {
                        $('#checklistaction_' + v.cD_ID).val(0);
                        $('#checklistaction_' + v.cD_ID).attr('disabled', false);
                    }
                    else {
                        $('#checklistaction_' + v.cD_ID).val(-1);
                        $('#checklistaction_' + v.cD_ID).attr('disabled', false);
                    }
                });
            },
            dataType: "json",
        });
    }
    function ObservationViewerPanel(obs_id) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_text_branches",
            type: "POST",
            data: {
                'OBS_ID': obs_id,
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {

                $('#viewMemoModel_ObSent').modal('show');
                $('#viewMemo_memo_ObSent').html(data[0].obS_TEXT);
                $('#viewMemo_response_ObSent').html(data[0].obS_REPLY);
                $('#viewMemo_process_ObSent').html(data[0].process);
                $('#viewMemo_annex_ObSent').val(data[0].annexurE_ID);
                $('#viewMemo_subprocess_ObSent').html(data[0].suB_PROCESS);
                $('#viewMemo_violation_ObSent').html(data[0].checklist_Details);
                $('#viewMemo_risk_ObSent').val(data[0].obS_RISK_ID);
                $('#listofRespPersons_ObSent tbody').empty();
                  if(data[0].responsiblE_PPs != null){
                       if (data[0].responsiblE_PPs.length > 0) {
                    $.each(data[0].responsiblE_PPs, function (j, pp) {
                        var srNo = $('#listofRespPersons_ObSent tbody tr').length;
                        srNo++;
                        $('#listofRespPersons_ObSent tbody').append('<tr data-pp="' + (pp.pP_NO || '') + '" data-loan="' + (pp.loaN_CASE || '') + '" data-account="' + (pp.accounT_NUMBER || '') + '"><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                    });
                }
                }
            },
            dataType: "json",
        });

    }

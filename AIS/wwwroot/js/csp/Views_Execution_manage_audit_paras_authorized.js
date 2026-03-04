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
        var g_p_id = 0;
        var g_ind = "";
        var g_allObs = [];
        var g_respUser = [];
        var g_respUsersArr = [];
        var g_stagedResp = [];
        var g_index = 0;
        var g_ele = null;
        const pageData = getPageData();
    var g_annexList = pageData.AnnexList || [];
        var g_selectedRiskId = 0;
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

        function buildKey(ppNo, role, loanCase, accountNumber) {
            return `${ppNo || ''}|${role || ''}|${loanCase || ''}|${accountNumber || ''}`;
        }
        function stagePending(item) {
            if (!item) {
                return;
            }
            var key = item.key || buildKey(item.ppNo, item.role, item.loanCase, item.accountNumber);
            item.key = key;
            var exists = g_stagedResp.some(function (entry) { return entry.key === key; });
            if (!exists) {
                g_stagedResp.push(item);
            } else {
                alert('This responsibility is already added.');
            }
        }
        function renderPendingGrid() {
            var $tableBody = $('#respPendingTable tbody');
            if (!$tableBody.length) {
                return;
            }
            $tableBody.empty();
            g_stagedResp.forEach(function (item) {
                $tableBody.append(`
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
        function clearPending() {
            g_stagedResp = [];
            renderPendingGrid();
        }
        $(document).ready(function () {
            console.log("Loaded manage_audit_paras_authorized JS", { annexCount: g_annexList.length });
            $('#paraTextViewer').richText({
                imageUpload: false,
                fileUpload: false,
                videoEmbed: false,
                urls: false
            });
            $('#p_paraTextViewer').richText({
                imageUpload: false,
                fileUpload: false,
                videoEmbed: false,
                urls: false
            });
            const currentYear = new Date().getFullYear();
            for (var i = 1970; i <= currentYear; i++) {
                $('#auditPara_Period').append('<option value="' + i + '">' + i + '</option>');
                $('#p_auditPara_Period').append('<option value="' + i + '">' + i + '</option>');
            }


            $('#p_auditPara_Annex').on('change', updateRiskDisplay);

            getEntityObservation();

        });
        function reloadLocation() {
            getEntityObservation();
        }
        function getEntityObservation() {
            destroyDatatable('manageObsPanel');
            if ($('#entitySelectField option:selected').val() != 0) {
                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/get_observations_for_manage_paras_auth",
                    type: "POST",
                    data: {
                    },
                    cache: false,
                    success: function (data) {
                        g_allObs = data;
                        $.each(data, function (i, v) {
                                $('#manageObsPanel tbody').append('<tr><td class="text-center">' + (i + 1) + '</td><td class="text-center">' + v.auditee + '</td><td class="text-center">' + v.audiT_PERIOD + '</td><td>' + v.parA_NO + '</td><td>' + v.annex + '</td><td>' + v.obS_RISK + '</td><td>' + v.obS_GIST + '</td><td>' + v.updateD_BY + '</td><td>' + v.updateD_ON + '</td><td class="text-center"><a data-onclick="event.preventDefault();ObservationViewerPanel(\'' + i + '\')" href="#" class="text-hover">View Para Details</a></td></tr>');
                        });
                        initializeDataTable('manageObsPanel');
                    },
                    dataType: "json",
                });
            }
        }

        function ObservationViewerPanel(index) {
            g_index = index;
            $('#viewMemoModel').modal('show');
            var v = g_allObs[index];
            g_com_id = v.coM_ID;
            g_np_id = v.neW_PARA_ID;
            g_op_id = v.olD_PARA_ID;
            g_p_id = v.parA_ID;
            g_ind = v.indicator;

            $('#auditPara_Period').val(v.audiT_PERIOD);
            $('#auditPara_ParaNO').val(v.parA_NO);
            $('#auditPara_Annex').val(v.anneX_ID);
            $('#auditPara_Gist').val(v.obS_GIST);
            $('#auditPara_Risk').val(v.obS_RISK);
            $('#auditPara_AmountInv').val(v.amounT_INV);
            $('#auditPara_InstNO').val(v.nO_INSTANCES);
            $('#paraTextViewer').val(v.parA_TEXT).trigger('change');
            ObservationResponsibles(g_index);
            setupResponsibilityForOld(g_com_id);
            if (window.ReferenceSection) {
                ReferenceSection.init({ comId: g_com_id });
            }

            //get Proposed Changes
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_proposed_changes_in_manage_paras_auth",
                type: "POST",
                data: {

                       'COM_ID': v.coM_ID
                },
                cache: false,
                success: function (data) {
                    if (data.length > 0) {
                        var v = data[0];
                        $('#p_auditPara_Period').val(v.audiT_PERIOD);
                        $('#p_auditPara_ParaNO').val(v.parA_NO);
                        $('#p_auditPara_Annex').val(v.anneX_ID || v.annex);
                        updateRiskDisplay();
                        $('#p_auditPara_Gist').val(v.obS_GIST);
                        g_selectedRiskId = parseInt(v.obS_RISK_ID || 0);
                        $('#p_auditPara_Risk').val(v.obS_RISK).prop('readonly', true);
                        var color = '';
                        if ((v.obS_RISK || '').toLowerCase() === 'high') {
                            color = 'red';
                        } else if ((v.obS_RISK || '').toLowerCase() === 'medium') {
                            color = 'gold';
                        } else if ((v.obS_RISK || '').toLowerCase() === 'low') {
                            color = 'green';
                        }
                        $('#p_auditPara_Risk').css('color', color);
                        $('#p_auditPara_AmountInv').val(v.amounT_INV);
                        $('#p_auditPara_InstNO').val(v.nO_INSTANCES);
                        $('#p_paraTextViewer').val(v.parA_TEXT).trigger('change');


                    }
                },
                dataType: "json",
            });


        }
        function responsibleCallback() {
            $('#ResponsiblePPModel').modal('hide');
            ObservationResponsibles(g_index);
        }


    function updateObservationStatus(type) {
            if(type=='A'){
                if ($('#p_auditPara_Period').val() == "") {
                alert("Please enter Audit Period");
                return false;
            }

            if ($('#p_auditPara_Risk').val() == "0") {
                alert("Please select Audit Risk");
                return false;
            }
            if ($('#p_auditPara_Annex').val() == "") {
                alert("Please select Annexure");
                return false;
            }
            if ($('#p_auditPara_ParaNO').val() == "") {
                alert("Please enter Para No");
                return false;
            }
            if ($('#p_auditPara_Gist').val() == "") {
                alert("Please enter Para Gist");
                return false;
            }
            if ($('#p_paraTextViewer').val() == "") {
                alert("Please enter Para Text");
                return false;
            }
            if ($('#p_auditPara_AmountInv').val() == "") {
                alert("Please enter Amount Involved or enter 0 in case of no amount involved");
                return false;
            }
            if ($('#p_auditPara_InstNO').val() == "") {
                alert("Please enter No. of Instances or enter 0 in case of no instance ");
                return false;
            }

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/authorize_para_for_manage_audit_paras",
                type: "POST",
                data: {
                     'COM_ID' : g_com_id,
                    'NEW_PARA_ID': g_np_id,
                    'OLD_PARA_ID': g_op_id,
                    'PARA_ID': g_p_id,
                    'INDICATOR': g_ind,
                    'AUDIT_PERIOD': $('#p_auditPara_Period').val(),
                    'OBS_GIST': $('#p_auditPara_Gist').val(),
                    'OBS_RISK_ID': g_selectedRiskId,
                    'PARA_NO': $('#p_auditPara_ParaNO').val(),
                    'PARA_TEXT': $('#p_paraTextViewer').val(),
                    'ANNEX_ID': $('#p_auditPara_Annex').val(),
                    'AMOUNT_INV': $('#p_auditPara_AmountInv').val(),
                    'NO_INSTANCES': $('#p_auditPara_InstNO').val(),
                    'P_DECISION': 'A',
                },
                cache: false,
                success: function (data) {
                    $('#viewMemoModel').modal('hide');
                    showApiAlert(data);
                    onAlertCallback(getEntityObservation);
                },
                dataType: "json",
            });

            }
            else {
                 $.ajax({
                url: g_asiBaseURL + "/ApiCalls/referredback_para_for_manage_audit_paras",
                type: "POST",
                data: {
                     'COM_ID' : g_com_id,
                    'NEW_PARA_ID': g_np_id,
                    'OLD_PARA_ID': g_op_id,
                    'PARA_ID': g_p_id,
                    'INDICATOR': g_ind,
                    'AUDIT_PERIOD': $('#p_auditPara_Period').val(),
                    'OBS_GIST': $('#p_auditPara_Gist').val(),
                    'OBS_RISK_ID': g_selectedRiskId,
                    'PARA_NO': $('#p_auditPara_ParaNO').val(),
                    'PARA_TEXT': $('#p_paraTextViewer').val(),
                    'ANNEX_ID': $('#p_auditPara_Annex').val(),
                    'AMOUNT_INV': $('#p_auditPara_AmountInv').val(),
                    'NO_INSTANCES': $('#p_auditPara_InstNO').val(),
                    'P_DECISION': 'R',
                },
                cache: false,
                success: function (data) {
                    $('#viewMemoModel').modal('hide');
                    showApiAlert(data);
                    onAlertCallback(getEntityObservation);
                },
                dataType: "json",
            });
            }


        }


             function updateRiskDisplay() {
                var annexId = $('#p_auditPara_Annex').val();
                var riskName = '';
                g_selectedRiskId = 0;
                $.each(g_annexList, function (i, v) {
                    var id = v.ID || v.id;
                    if (id == annexId) {
                        riskName = v.RISK || v.risk;
                        g_selectedRiskId = v.RISK_ID || v.risK_ID;
                    }
                });
                $('#p_auditPara_Risk').val(riskName);
                var color = '';
                if (riskName.toLowerCase() === 'high') {
                    color = 'red';
                } else if (riskName.toLowerCase() === 'medium') {
                    color = 'gold';
                } else if (riskName.toLowerCase() === 'low') {
                    color = 'green';
                }
                $('#p_auditPara_Risk').css('color', color);
            }

        function DeleteDuplicatePara(np_id, op_id, ind,c_id) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/request_delete_duplicate_para",
                type: "POST",
                data: {
                    'COM_ID': c_id,
                    'NEW_PARA_ID': np_id,
                    'OLD_PARA_ID': op_id,
                    'INDICATOR': ind
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(getEntityObservation);
                },
                dataType: "json",
            });
        }

        function openResponsiblePPs() {
            $('#ResponsiblePPModel').modal('show');
            $('#matchedPPNoPanels').empty();
            $('#matchedPPNoPanelsBYPP').empty();
            g_ele = null;
            $('#addResponsibleButton').removeClass("d-none");
            $('#updateResponsibleButton').addClass("d-none");
            $('#deleteResponsibleButton').addClass("d-none");
            $('#responsiblePPNoEntryField').val('');
            $('#responsibleLCNoEntryField').val('');
            $('#responsibleBrCodeEntryField').val('');
            $('#responsibleLoanNumberEntryField').val('');
            $('#responsibleLoanAmountEntryField').val('');
            $('#responsibleAccountNumberEntryField').val('');
            $('#responsibleAccountAmountEntryField').val('');
            clearPending();
            return false;
        }
        function openResponsibleGridModel() {
            $('#respGridTable tbody').html($('#p_listofRespPersons tbody').html());
            $('#ResponsibleGridModel').modal('show');
        }
        function getMatchedPP() {
            $('#matchedPPNoPanelsBYPP').empty();
            $('#matchedPPNoPanels').empty();
            var ppNo = $.trim($('#responsiblePPNoEntryField').val());
            if (ppNo === "") {
                alert('Please enter PP Number to proceed');
                return;
            }
            g_respUser = [];
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_responsible_by_pp",
                type: "POST",
                data: { 'PP_NO': normalizeRequiredInt(ppNo) },
                cache: false,
                success: function (data) {
                    var items = Array.isArray(data) ? data : (data ? [data] : []);
                    if (!items.length || !items[0]) {
                        alert('No record found..');
                        return;
                    }
                    var item = items[0];
                    g_respUser.push(item);
                    stagePending({
                        role: item.role || item.ROLE || '',
                        ppNo: item.ppNo || item.PP_NO || ppNo,
                        empName: item.empName || item.EMP_NAME || item.name || '',
                        loanCase: $('#responsibleLoanNumberEntryField').val() || $('#loanCaseNumber').val() || '',
                        lcAmount: $('#responsibleLoanAmountEntryField').val() || $('#loanCaseAmount').val() || '',
                        accountNumber: $('#responsibleAccountNumberEntryField').val() || '',
                        accAmount: $('#responsibleAccountAmountEntryField').val() || ''
                    });
                    renderPendingGrid();
                },
                dataType: 'json'
            });
        }

        function getLCDetails() {
            $('#matchedPPNoPanels').empty();
            g_respUser = [];
            var lcNo = $.trim($('#responsibleLCNoEntryField').val());
            var brCode = $.trim($('#responsibleBrCodeEntryField').val());
            if (lcNo === '' || brCode === '') {
                alert('Please enter LC Number and Branch Code to proceed');
                return;
            }
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_responsible_by_lc",
                type: "POST",
                data: {
                    'LC_NO': lcNo,
                    'BR_CODE': brCode
                },
                cache: false,
                success: function (data) {
                    var response = Array.isArray(data) ? data : (data ? [data] : []);
                    if (!response.length) {
                        alert('No record found..');
                        return;
                    }
                    response.forEach(function (item) {
                        var ppNo = item.ppNo || item.PP_NO || item.ppNumber || '';
                        var role = item.role || item.ROLE || item.respRole || '';
                        var empName = item.empName || item.EMP_NAME || item.name || '';
                        var loanCase = item.loanCase || item.LOAN_CASE || item.loanCaseNo || lcNo;
                        var lcAmount = item.lcAmount || item.LC_AMOUNT || item.outstandingAmount || '';
                        var accountNumber = item.accountNumber || item.ACCOUNT_NUMBER || item.accountNo || '';
                        var accAmount = item.accAmount || item.ACC_AMOUNT || item.accountAmount || '';
                        $('#loanCaseNumber').val(loanCase || '');
                        $('#loanCaseAmount').val(lcAmount || '');
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

        var respSectionUpdate = {
            getLCDetails: getLCDetails,
            getMatchedPP: getMatchedPP
        };
        function deleteRespRow(e) {
            if (!confirm('Are you sure you want to delete this responsibility?')) {
                return;
            }
            var $row = $(e).closest('tr');
            var payload = {
                PP_NO: normalizeRequiredInt($row.data('pp') || $row.children('td').eq(1).text()),
                LOAN_CASE: normalizeNullableInt($row.data('loan') || $row.children('td').eq(3).text()),
                LC_AMOUNT: normalizeNullableInt($row.data('lcamount') || $row.children('td').eq(4).text()),
                ACCOUNT_NUMBER: normalizeRequiredInt($row.data('account') || $row.children('td').eq(5).text()),
                ACC_AMOUNT: normalizeRequiredInt($row.data('accamount') || $row.children('td').eq(6).text())
            };
            fetchWithPageId(`${g_asiBaseURL}/ApiCalls/add_responsible_for_old_paras?IND_Action=D`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...payload, COM_ID: g_com_id })
            }).then(function () {
                if (typeof ObservationResponsibles === 'function') {
                    ObservationResponsibles(g_index);
                }
            });
        }
        function updateRespRow(e) {
            g_ele = e;
            var $row = $(e).closest('tr');
            $('#ResponsiblePPModel').off('shown.bs.modal.update').on('shown.bs.modal.update', function () {
                $('#addResponsibleButton').addClass("d-none");
                $('#updateResponsibleButton').removeClass("d-none");
                $('#deleteResponsibleButton').removeClass("d-none");
                var ppNo = $row.data('pp') || $row.children('td').eq(1).text();
                var lcNo = $.trim($row.data('loan') || $row.children('td').eq(3).text()) || '0';
                var lcAmt = $.trim($row.data('lcamount') || $row.children('td').eq(4).text()) || '0';
                var accNo = $.trim($row.data('account') || $row.children('td').eq(5).text()) || '0';
                var accAmt = $.trim($row.data('accamount') || $row.children('td').eq(6).text()) || '0';
                $('#responsiblePPNoEntryField').val(ppNo);
                $('#loanCaseNumber').val(lcNo);
                $('#loanCaseAmount').val(lcAmt);
                $('#responsibleAccountNumberEntryField').val(accNo);
                $('#responsibleAccountAmountEntryField').val(accAmt);
                clearPending();
                stagePending({
                    role: '',
                    ppNo: ppNo,
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

        $(document).on('click', '.respPendingRemove', function () {
            var key = $(this).data('key');
            g_stagedResp = g_stagedResp.filter(function (entry) { return entry.key !== key; });
            renderPendingGrid();
        });

        function ObservationResponsibles(index) {
            g_index = index;
            var v = g_allObs[index];
            g_np_id = v.neW_PARA_ID;
            g_op_id = v.olD_PARA_ID;
            g_ind = v.indicator;
            g_com_id = v.coM_ID || v.COM_ID || g_com_id;
            $('#listofRespPersons tbody').empty();
            $('#p_listofRespPersons tbody').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_responsible_person_list",
                type: "POST",
                data: {
                    'PARA_ID': g_np_id != "" ? g_np_id : g_op_id,
                    'INDICATOR': g_ind
                },
                cache: false,
                success: function (data) {
                    var sr = 1;
                    $.each(data, function (i, v) {
                        $('#listofRespPersons tbody').append('<tr id="tr_' + v.pP_NO + '"><td>' + sr + '</td><td>' + v.pP_NO + '</td><td>' + v.emP_NAME + '</td><td>' + v.loaN_CASE + '</td><td>' + v.lC_AMOUNT + '</td><td>' + v.accounT_NUMBER + '</td><td>' + v.acC_AMOUNT + '</td><td>' + v.remarks + '</td></tr>');
                        sr++;
                    });
                },
                dataType: "json",
            });

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_responsibility_for_authorize",
                type: "POST",
                data: {
                    'COM_ID': g_com_id
                },
                cache: false,
                success: function (data) {
                    var sr_c = 1;
                    $.each(data, function (i, v) {
                        var act = v.action || v.ACTION || '';
                        $('#p_listofRespPersons tbody').append(
                            '<tr data-pp="' + (v.pP_NO || '') + '" data-loan="' + (v.loaN_CASE || '') + '" data-account="' + (v.accounT_NUMBER || '') + '" data-lcamount="' + (v.lC_AMOUNT || '') + '" data-accamount="' + (v.acC_AMOUNT || '') + '" data-emp="' + (v.emP_NAME || '') + '" data-action="' + act + '" data-remarks="' + (v.remarks || '') + '">' +
                            '<td>' + sr_c + '</td>' +
                            '<td>' + v.pP_NO + '</td>' +
                            '<td>' + v.emP_NAME + '</td>' +
                            '<td>' + v.loaN_CASE + '</td>' +
                            '<td>' + v.lC_AMOUNT + '</td>' +
                            '<td>' + v.accounT_NUMBER + '</td>' +
                            '<td>' + v.acC_AMOUNT + '</td>' +
                            '<td>' + act + '</td>' +
                            '<td>' + v.remarks + '</td>' +
                            '<td class="text-center"><a href="#" data-onclick="event.preventDefault();updateRespRow(this);">Update</a></td>' +
                            '<td class="text-center"><a href="#" class="text-danger" data-onclick="event.preventDefault();deleteRespRow(this);">Delete</a></td>' +
                            '</tr>'
                        );
                        sr_c++;
                    });
                    $('#respGridTable tbody').html($('#p_listofRespPersons tbody').html());
                },
                dataType: "json",
            });
        }

            function setupResponsibilityForOld(comId) {
                const $tbody = $('#update_listofRespPersons tbody');

                function render(list) {
                    if (!$tbody.length) return;
                    $tbody.empty();
                    list.forEach(function (v, i) {
                        const id = v.resP_ROW_ID || v.RESP_ROW_ID;
                        const row = `<tr data-id="${id}"><td>${i + 1}</td>
                                        <td>${v.pP_NO || v.PP_NO || ''}</td>
                                        <td>${v.emP_NAME || v.EMP_NAME || ''}</td>
                                        <td>${v.loaN_CASE || v.LOAN_CASE || ''}</td>
                                        <td>${v.lC_AMOUNT || v.LC_AMOUNT || ''}</td>
                                        <td>${v.accounT_NUMBER || v.ACCOUNT_NUMBER || ''}</td>
                                        <td>${v.acC_AMOUNT || v.ACC_AMOUNT || ''}</td>
                                        <td>${v.remarkS || v.REMARKS || ''}</td>
                                        <td class="text-center"><button type="button" class="btn btn-sm btn-danger btn-delete" data-id="${id}">&times;</button></td></tr>`;
                        $tbody.append(row);
                    });
                }

                function load() {
                    fetchWithPageId(`${g_asiBaseURL}/ApiCalls/GetResponsiblePPNOforoldPara?COM_ID=${comId}`)
                        .then(r => r.json())
                        .then(render);
                }

                async function save(action, data) {
                    const payload = data || {};
                    await fetchWithPageId(`${g_asiBaseURL}/ApiCalls/add_responsible_for_old_paras?IND_Action=${action}`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ ...payload, COM_ID: comId })
                    });
                    load();
                    if (typeof ObservationResponsibles === 'function') {
                        ObservationResponsibles(g_index);
                    }
                }

                $tbody.off('click', '.btn-delete').on('click', '.btn-delete', function () {
                    if (!confirm('Are you sure you want to delete this responsibility?')) {
                        return;
                    }
                    const tr = this.closest('tr');
                    const data = {
                        PP_NO: normalizeRequiredInt(tr.children[1].textContent),
                        LOAN_CASE: normalizeNullableInt(tr.children[3].textContent),
                        LC_AMOUNT: normalizeNullableInt(tr.children[4].textContent),
                        ACCOUNT_NUMBER: normalizeRequiredInt(tr.children[5].textContent),
                        ACC_AMOUNT: normalizeRequiredInt(tr.children[6].textContent)
                    };
                    save('D', data);
                });

                load();

                window.addResponsibilityToMainTable = async function (action) {
                    if (!g_stagedResp.length) {
                        alert('Select at least one responsible');
                        return;
                    }
                    for (const item of g_stagedResp) {
                        const data = {
                            PP_NO: normalizeRequiredInt(item.ppNo),
                            ACCOUNT_NUMBER: normalizeRequiredInt(item.accountNumber),
                            ACC_AMOUNT: normalizeRequiredInt(item.accAmount),
                            LOAN_CASE: normalizeNullableInt(item.loanCase),
                            LC_AMOUNT: normalizeNullableInt(item.lcAmount)
                        };
                        await save(action, data);
                    }
                    clearPending();
                    if (window.$) {
                        $('#ResponsibleGridModel').modal('hide');
                        $('#ResponsiblePPModel').modal('hide');
                    }
                };
            }

            document.addEventListener('DOMContentLoaded', function () {
                setupResponsibilityForOld(0);
            });

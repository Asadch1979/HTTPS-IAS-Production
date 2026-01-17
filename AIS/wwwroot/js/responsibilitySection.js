function initResponsibilitySection(config) {
    var opts = $.extend({
        tableSelector: '#listofRespPersons',
        changesTableSelector: '#c_listofRespPersons',
        modalSelector: '#ResponsiblePPModel',
        comId: 0,
        newParaId: 0,
        oldParaId: 0,
        indicator: '',
        readOnly: false,
        status: 0,
        directSaveMode: true,
        afterSave: null,
        engId: 0
    }, config || {});

    var table = $(opts.tableSelector);
    var changesTable = $(opts.changesTableSelector);
    var modal = $(opts.modalSelector);
    var respUser = [];
    var selectedRow = null;
    var isSaving = false;

    function getInputValue(selectors) {
        for (var i = 0; i < selectors.length; i++) {
            var $el = $(selectors[i]);
            if ($el.length) {
                var val = $el.val();
                if (val !== undefined && val !== null) {
                    return val;
                }
            }
        }
        return '';
    }

    function normalizeNumericValue(value) {
        if (value === undefined || value === null) {
            return 0;
        }
        var trimmed = String(value).trim();
        if (!trimmed) {
            return 0;
        }
        var numeric = Number(trimmed);
        return isNaN(numeric) ? 0 : numeric;
    }

    function load() {
        if (!table.length) return;
        table.find('tbody').empty();
        if (changesTable.length) changesTable.find('tbody').empty();
        if (opts.indicator === 'O') {
            if (!opts.comId) return;
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetResponsiblePPNOforoldPara',
                type: 'GET',
                data: { 'COM_ID': opts.comId },
                dataType: 'json',
                success: function (data) {
                    var sr = 1; var sr_c = 1;
                    $.each(data, function (i, v) {
                        var pp = v.pP_NO || v.PP_NO || v.pp_no;
                        var baseRow = '<tr id="tr_' + pp + '"><td>' + (v.indicator === 'O' ? sr : sr_c) + '</td><td>' + pp + '</td><td>' + (v.emP_NAME || v.EMP_NAME || v.emp_name) + '</td><td>' + (v.loaN_CASE || v.LOAN_CASE || v.loancase) + '</td><td>' + (v.lC_AMOUNT || v.LC_AMOUNT || v.lcamount) + '</td><td>' + (v.accounT_NUMBER || v.ACCOUNT_NUMBER || v.accnumber) + '</td><td>' + (v.acC_AMOUNT || v.ACC_AMOUNT || v.acamount) + '</td><td>' + (v.remarkS || v.REMARKS || '') + '</td>';
                        if (!opts.readOnly && (v.indicator === 'O' || v.indicator === undefined || v.indicator === null))
                            baseRow += '<td class="text-center"><a href="#" class="updateResp">Update / delete</a></td>';
                        baseRow += '</tr>';
                        if (v.indicator === 'O' || v.indicator === undefined || v.indicator === null) {
                            table.find('tbody').append(baseRow); sr++;
                        } else if (changesTable.length) {
                            changesTable.find('tbody').append(baseRow); sr_c++;
                        }
                    });
                }
            });
            return;
        }
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_responsible_person_list',
            type: 'POST',
            data: {
                'PARA_ID': opts.newParaId ? opts.newParaId : opts.oldParaId,
                'INDICATOR': opts.indicator,
                'COM_ID': opts.comId,
                'ENG_ID': opts.engId
            },
            dataType: 'json',
            success: function (data) {
                var sr = 1; var sr_c = 1;
                $.each(data, function (i, v) {
                    var row = '<tr id="tr_' + v.pP_NO + '"><td>' + sr + '</td><td>' + v.pP_NO + '</td><td>' + v.emP_NAME + '</td><td>' + v.loaN_CASE + '</td><td>' + v.lC_AMOUNT + '</td><td>' + v.accounT_NUMBER + '</td><td>' + v.acC_AMOUNT + '</td><td>' + v.remarks + '</td>';
                    if (!opts.readOnly && v.indicator === 'O')
                        row += '<td class="text-center"><a href="#" class="updateResp">Update / delete</a></td>';
                    row += '</tr>';
                    if (v.indicator === 'O') {
                        table.find('tbody').append(row); sr++; }
                    else if (changesTable.length) {
                        changesTable.find('tbody').append('<tr id="tr_' + v.pP_NO + '"><td>' + sr_c + '</td><td>' + v.pP_NO + '</td><td>' + v.emP_NAME + '</td><td>' + v.loaN_CASE + '</td><td>' + v.lC_AMOUNT + '</td><td>' + v.accounT_NUMBER + '</td><td>' + v.acC_AMOUNT + '</td><td>' + v.remarks + '</td></tr>'); sr_c++; }
                });
            }
        });
    }

    function getMatchedPP() {
        // prefer new style panel if available
        if ($('#matchedPPNoPanelsBYPP').length) {
            $('#matchedPPNoPanelsBYPP').empty();
            if ($('#responsiblePPNoEntryField').val() === "") {
                alert('Please enter PP Number to proceed');
                return;
            }
            respUser = [];
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/get_employee_name_from_pp',
                type: 'POST',
                data: { 'PP_NO': $('#responsiblePPNoEntryField').val() },
                cache: false,
                success: function (data) {
                    respUser.push(data);
                    if (data.ppNumber > 0) {
                        $('#matchedPPNoPanelsBYPP').append(`
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
                                <div class="col-sm-1"><span>${$('#loanCaseNumber').val() || $('#responsibleLoanNumberEntryField').val()}</span></div>
                                <div class="col-sm-2"><span>${$('#loanCaseAmount').val() || $('#responsibleLoanAmountEntryField').val()}</span></div>
                                <div class="col-sm-1">
                                    <input style="margin-left:10px;" class="respCheckBOXBYPP" type="checkbox" />
                                </div>
                            </div>
                        `);
                    }
                },
                dataType: 'json'
            });
        } else {
            $('#matchedPPNoPanels').empty();
            respUser = [];
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/get_employee_name_from_pp',
                type: 'POST',
                data: { 'PP_NO': $('#responsiblePPNoEntryField').val() },
                dataType: 'json',
                success: function (data) {
                    respUser.push(data);
                    if (data.ppNumber > 0) {
                        $('#matchedPPNoPanels').append('<div class="row"><div class="row col-md-12 mt-2"><div class="col-sm-4"><label>Responsible</label></div><div class="col-sm-8"><span>' + data.name + ' (' + data.ppNumber + ')</span></div></div><div class="row col-md-12 mt-2"><div class="col-md-4"><label> Loan Case </label></div><div class="col-md-8"><input id="resp_loan_case" class="form-control" type="number" /></div></div><div class="row col-md-12 mt-2"><div class="col-md-4"><label> LC Amount </label></div><div class="col-md-8"><input id="resp_loan_amount" class="form-control" type="number" /></div></div><div class="row col-md-12 mt-2"><div class="col-md-4"><label> Account Number </label></div><div class="col-md-8"><input id="resp_account_number" class="form-control" type="number" /></div></div><div class="row col-md-12 mt-2"><div class="col-md-4"><label>ACC Amount </label></div><div class="col-md-8"><input id="resp_account_amount" class="form-control" type="number" /></div></div><div class="row col-md-12 mt-2"><div class="col-md-4"><label>Remarks/Reason</label></div><div class="col-md-8"><textarea id="resp_remarks" class="form-control" rows="3"></textarea></div></div></div>');
                        if (selectedRow) {
                            $('#resp_loan_case').val($(selectedRow).parent().parent().children('td').eq(3).text());
                            $('#resp_loan_amount').val($(selectedRow).parent().parent().children('td').eq(4).text());
                            $('#resp_account_number').val($(selectedRow).parent().parent().children('td').eq(5).text());
                            $('#resp_account_amount').val($(selectedRow).parent().parent().children('td').eq(6).text());
                            $('#resp_remarks').val('');
                        }
                    } else {
                        $('#matchedPPNoPanels').append('<div class="row"><span>No record found..</span></div>');
                    }
                }
            });
        }
    }

    function getLCDetails() {
        $('#matchedPPNoPanels').empty();
        respUser = [];
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_lc_details',
            type: 'POST',
            data: {
                'LC_NO': $('#responsibleLCNoEntryField').val(),
                'BR_CODE': $('#responsibleBrCodeEntryField').val()
            },
            cache: false,
            success: function (data) {
                var response = data;
                response.forEach(function (d) {
                    var responsiblePersons = [
                        { label: 'MCO', ppno: d.mcoPPNo, name: d.mcoName },
                        { label: 'Manager', ppno: d.managerPPNo, name: d.managerName },
                        { label: 'RGM', ppno: d.rgmPPNo, name: d.rgmName },
                        { label: 'CAD Reviewer', ppno: d.cadReviewerPPNo, name: d.cadReviewerName },
                        { label: 'CAD Authorizer', ppno: d.cadAuthorizerPPNo, name: d.cadAuthorizerName }
                    ].filter(function (p) { return p.ppno; });

                    var formatDate = function (dateString) {
                        if (!dateString) return 'N/A';
                        var parts = dateString.split('T')[0].split('-');
                        return parts[2] + '/' + parts[1] + '/' + parts[0];
                    };

                      $('#matchedPPNoPanels').append(`
                        <hr class="row col-md-12 mt-1"/>
                        <div class="row loan-case-panel">
                            <div class="row col-md-12 mt-2">
                                <div class="col-md-4"><label>Name</label></div>
                                <div class="col-md-8"><input class="form-control" type="text" value="${d.name}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                <div class="col-md-4"><label>CNIC</label></div>
                                <div class="col-md-8"><input class="form-control" type="text" value="${d.cnic}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                  <div class="col-md-4"><label>Loan Case No</label></div>
                                  <div class="col-md-8"><input id="resp_loan_case" class="form-control" type="text" value="${d.loanCaseNo}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                <div class="col-md-4"><label>Application Date</label></div>
                                <div class="col-md-8"><input class="form-control" type="text" value="${formatDate(d.appDate)}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                <div class="col-md-4"><label>CAD Receive Date</label></div>
                                <div class="col-md-8"><input class="form-control" type="text" value="${formatDate(d.cadReceiveDate)}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                <div class="col-md-4"><label>Sanction Date</label></div>
                                <div class="col-md-8"><input class="form-control" type="text" value="${formatDate(d.sanctionDate)}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                <div class="col-md-4"><label>Disbursed Amount</label></div>
                                <div class="col-md-8"><input class="form-control" type="text" value="${d.disbursedAmount}" readonly /></div>
                            </div>
                            <div class="row col-md-12 mt-2">
                                  <div class="col-md-4"><label>Outstanding Amount</label></div>
                                  <div class="col-md-8"><input id="resp_loan_amount" class="form-control" type="text" value="${d.outstandingAmount}" readonly /></div>
                            </div>
                            <hr class="row col-md-12 mt-3" />
                            <div class="row col-md-12 mt-2">
                                <div class="col-sm-3 font-weight-bold">Role</div>
                                <div class="col-sm-3 font-weight-bold">P.P. No</div>
                                <div class="col-sm-3 font-weight-bold">Name</div>
                                <div class="col-sm-3 font-weight-bold">Action</div>
                            </div>
                            <hr class="row col-md-12 mt-3" />
                            ${responsiblePersons.map(function (person) {
                                return `
                                    <div class="row col-md-12 mt-2">
                                        <div class="col-sm-3"><label>${person.label}</label></div>
                                        <div class="col-sm-3"><span>${person.ppno}</span></div>
                                        <div class="col-sm-3"><span>${person.name}</span></div>
                                        <div class="col-sm-3">
                                            <input style="margin-left:10px;" class="respCheckBOX" type="checkbox" />
                                        </div>
                                    </div>
                                `;
                            }).join('')}
                        </div>
                      `);
                      $('#loanCaseNumber').val(d.loanCaseNo);
                      $('#loanCaseAmount').val(d.outstandingAmount);
                });
            },
            dataType: 'json'
        });
    }

    function saveResp(action) {
        if (isSaving) return;
        isSaving = true;
        var $btns = $('#addResponsibleButton,#updateResponsibleButton,#deleteResponsibleButton').prop('disabled', true);

        if (!respUser.length || respUser[0].ppNumber <= 0) {
            isSaving = false;
            $btns.prop('disabled', false);
            return;
        }
        var lcRaw = getInputValue(['#resp_loan_case', '#loanCaseNumber', '#responsibleLoanNumberEntryField', '.js-responsible-loan-case']);
        var accRaw = getInputValue(['#resp_account_number', '#responsibleAccountNumberEntryField', '.js-responsible-account-number']);
        if ($.trim(lcRaw) === '' && $.trim(accRaw) === '') {
            alert('Please enter Either Loan Case Or Account Number to Proceed');
            isSaving = false;
            $btns.prop('disabled', false);
            return;
        }
        var lcValue = normalizeNumericValue(lcRaw);
        var lcAmountValue = normalizeNumericValue(getInputValue(['#resp_loan_amount', '#loanCaseAmount', '#responsibleLoanAmountEntryField', '.js-responsible-loan-amount']));
        var accValue = normalizeNumericValue(accRaw);
        var accAmountValue = normalizeNumericValue(getInputValue(['#resp_account_amount', '#responsibleAccountAmountEntryField', '.js-responsible-account-amount']));
        if ((opts.indicator !== 'O' && (!opts.engId || opts.engId <= 0)) ||
            (opts.indicator === 'O' && (!opts.comId || opts.comId <= 0))) {
            alert('Context missing: please ensure ENG_ID (for new obs) or COM_ID (for old paras) is set.');
            isSaving = false;
            $btns.prop('disabled', false);
            return;
        }
        var ajaxOpts;
        if (opts.indicator === 'O') {
            ajaxOpts = {
                url: g_asiBaseURL + '/ApiCalls/add_responsible_for_old_paras?IND_Action=' + action,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    'PP_NO': respUser[0].ppNumber,
                    'LOAN_CASE': lcValue,
                    'LC_AMOUNT': lcAmountValue,
                    'ACCOUNT_NUMBER': accValue,
                    'ACC_AMOUNT': accAmountValue,
                    'COM_ID': opts.comId
                }),
                dataType: 'json'
            };
        } else {
            ajaxOpts = {
                url: g_asiBaseURL + '/ApiCalls/add_responsible_to_observation',
                type: 'POST',
                data: {
                    'PP_NO': respUser[0].ppNumber,
                    'LOAN_CASE': lcValue,
                    'LC_AMOUNT': lcAmountValue,
                    'ACCOUNT_NUMBER': accValue,
                    'ACC_AMOUNT': accAmountValue,
                    'EMP_NAME': respUser[0].name,
                    'REMARKS': $('#resp_remarks').val(),
                    'NEW_PARA_ID': opts.newParaId,
                    'OLD_PARA_ID': opts.oldParaId,
                    'ENG_ID': opts.engId,
                    'INDICATOR': action,
                    'COM_ID': opts.comId,
                    'ACTION': action,
                    'PARA_STATUS': opts.status
                },
                dataType: 'json'
            };
        }
        $.ajax(ajaxOpts).done(function (data) {
                var msg = data.Message || data.message || 'Operation completed';
                alert(msg);
                onAlertCallback(function () {
                    modal.modal('hide');
                    load();
                    if (typeof opts.afterSave === 'function') {
                        opts.afterSave();
                    }
                });
                $('#matchedPPNoPanels').empty();
                $('#matchedPPNoPanelsBYPP').empty();
                $('#responsiblePPNoEntryField').val('');
                $('#loanCaseNumber').val('');
                $('#loanCaseAmount').val('');
                $('#responsibleLoanNumberEntryField').val('');
                $('#responsibleLoanAmountEntryField').val('');
                $('#responsibleAccountNumberEntryField').val('');
                $('#responsibleAccountAmountEntryField').val('');
            }).fail(function (xhr) {
                var msg = 'Error occurred';
                if (xhr.responseJSON) {
                    msg = xhr.responseJSON.Message || xhr.responseJSON.message || msg;
                }
                alert(msg);
            }).always(function () {
            isSaving = false;
            $btns.prop('disabled', false);
        });
    }

    table.off('click.resp', '.updateResp').on('click.resp', '.updateResp', function (e) {
        e.preventDefault();

        // show the modal (this will clear previous values via the show handler)
        modal.modal('show');

        // set the current row after modal.show has reset state
        selectedRow = this;

        // toggle button visibility for update/delete mode
        $('#addResponsibleButton').addClass('d-none');
        $('#updateResponsibleButton').removeClass('d-none');
        $('#deleteResponsibleButton').removeClass('d-none');

        // populate entry fields from the selected row
        var $row = $(this).closest('tr');
        $('#matchedPPNoPanels').empty();
        $('#responsiblePPNoEntryField').val($row.attr('id').split('tr_')[1]);
        $('#loanCaseNumber, #responsibleLoanNumberEntryField').val($row.children('td').eq(3).text());
        $('#loanCaseAmount, #responsibleLoanAmountEntryField').val($row.children('td').eq(4).text());
        $('#responsibleAccountNumberEntryField').val($row.children('td').eq(5).text());
        $('#responsibleAccountAmountEntryField').val($row.children('td').eq(6).text());

        // build the matched PP panel with the populated values
        getMatchedPP();
    });

    modal.find('form').off('submit.resp').on('submit.resp', function (e) { e.preventDefault(); });

    if (!opts.readOnly) {
        $('#addResponsibleButton').off('click.resp').on('click.resp', function () { saveResp('A'); });
        $('#updateResponsibleButton').off('click.resp').on('click.resp', function () { saveResp('U'); });
        $('#deleteResponsibleButton').off('click.resp').on('click.resp', function () {
            if (confirm('Are you sure you want to delete this responsibility?')) {
                saveResp('D');
            }
        });
        $('#responsiblePPNoEntryField').off('keypress.resp').on('keypress.resp', function (e) { if (e.which === 13) { e.preventDefault(); getMatchedPP(); } });
        modal.off('show.bs.modal.resp').on('show.bs.modal.resp', function () {
            $('#matchedPPNoPanels').empty();
            $('#matchedPPNoPanelsBYPP').empty();
            selectedRow = null;
            respUser = [];
            $('#responsiblePPNoEntryField').val('');
            $('#loanCaseNumber').val('');
            $('#loanCaseAmount').val('');
            $('#responsibleLoanNumberEntryField').val('');
            $('#responsibleLoanAmountEntryField').val('');
            $('#responsibleAccountNumberEntryField').val('');
            $('#responsibleAccountAmountEntryField').val('');
            $('#addResponsibleButton').removeClass('d-none');
            $('#updateResponsibleButton').addClass('d-none');
            $('#deleteResponsibleButton').addClass('d-none');
        });
    }

    function updateContext(c) {
        opts = $.extend(opts, c || {});
        load();
    }

    load();

    return {
        reload: load,
        updateContext: updateContext,
        getMatchedPP: getMatchedPP,
        getLCDetails: getLCDetails,
        saveResp: saveResp
    };
}

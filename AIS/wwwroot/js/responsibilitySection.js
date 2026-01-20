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
    var stagedResp = [];
    var selectedRow = null;
    var isSaving = false;

    function getFirstValue(selectors) {
        for (var i = 0; i < selectors.length; i++) {
            var $el = $(selectors[i]);
            if ($el.length) {
                var val = $el.val();
                if (val !== undefined && val !== null && $.trim(val) !== '') {
                    return val;
                }
            }
        }
        return '';
    }

    function normalizeNumericValue(value) {
        var trimmed = $.trim(value);
        if (!trimmed) {
            return 0;
        }
        var number = Number(trimmed);
        return Number.isNaN(number) ? 0 : number;
    }

    function getBranchCodeValue(value) {
        return $.trim(value || '');
    }

    function buildKey(ppNo, role, loanCase, accountNumber, branchCode) {
        return `${ppNo || ''}|${role || ''}|${loanCase || ''}|${accountNumber || ''}|${branchCode || ''}`;
    }

    function applyDigitsOnly($container) {
        var $fields = $container.find('.digits-only');
        $fields.off('.digitsOnly');
        $fields.on('input.digitsOnly', function () {
            this.value = this.value.replace(/\D/g, '');
        });
        $fields.on('paste.digitsOnly', function () {
            var input = this;
            setTimeout(function () {
                input.value = input.value.replace(/\D/g, '');
            }, 0);
        });
        $fields.on('keydown.digitsOnly', function (e) {
            var controlKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
            if (controlKeys.indexOf(e.key) !== -1) {
                return;
            }
            if ((e.ctrlKey || e.metaKey) && ['a', 'c', 'v', 'x'].indexOf(e.key.toLowerCase()) !== -1) {
                return;
            }
            if (/^\d$/.test(e.key)) {
                return;
            }
            e.preventDefault();
        });
    }

    function renderPendingGrid() {
        var $tableBody = modal.find('#respPendingTable tbody');
        if (!$tableBody.length) {
            return;
        }
        $tableBody.empty();
        stagedResp.forEach(function (item) {
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

    function stageItem(item) {
        if (!item) {
            return;
        }
        var branchCode = getBranchCodeValue(item.branchCode || item.BR_CODE || item.brCode);
        if (!branchCode) {
            alert('Please enter Branch Code to proceed');
            return;
        }
        item.branchCode = branchCode;
        var key = item.key || buildKey(item.ppNo, item.role, item.loanCase, item.accountNumber, branchCode);
        item.key = key;
        var exists = stagedResp.some(function (entry) { return entry.key === key; });
        if (!exists) {
            stagedResp.push(item);
        } else {
            alert('This responsibility is already added.');
        }
    }

    function unstageItem(key) {
        stagedResp = stagedResp.filter(function (entry) { return entry.key !== key; });
    }

    function clearPending() {
        stagedResp = [];
        renderPendingGrid();
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
                    var loanCaseValue = v.loaN_CASE || v.LOAN_CASE || v.loancase;
                    var accountValue = v.accounT_NUMBER || v.ACCOUNT_NUMBER || v.accnumber;
                    var branchCodeValue = v.bR_CODE || v.BR_CODE || v.br_code || v.brCode;
                    var baseRow = '<tr data-pp="' + (pp || '') + '" data-branch="' + (branchCodeValue || '') + '" data-loan="' + (loanCaseValue || '') + '" data-account="' + (accountValue || '') + '"><td>' + (v.indicator === 'O' ? sr : sr_c) + '</td><td>' + pp + '</td><td>' + (v.emP_NAME || v.EMP_NAME || v.emp_name) + '</td><td>' + (branchCodeValue || '') + '</td><td>' + (loanCaseValue || '') + '</td><td>' + (v.lC_AMOUNT || v.LC_AMOUNT || v.lcamount) + '</td><td>' + (accountValue || '') + '</td><td>' + (v.acC_AMOUNT || v.ACC_AMOUNT || v.acamount) + '</td><td>' + (v.remarkS || v.REMARKS || '') + '</td>';
                    if (!opts.readOnly && (v.indicator === 'O' || v.indicator === undefined || v.indicator === null)) {
                        baseRow += '<td class="text-center"><a href="#" class="updateResp">Update</a></td>';
                        baseRow += '<td class="text-center"><a href="#" class="btn-resp-delete">Delete</a></td>';
                    } else {
                        baseRow += '<td></td><td></td>';
                    }
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
                    var row = '<tr data-pp="' + (v.pP_NO || '') + '" data-branch="' + (v.BR_CODE || v.br_code || '') + '" data-loan="' + (v.loaN_CASE || '') + '" data-account="' + (v.accounT_NUMBER || '') + '"><td>' + sr + '</td><td>' + v.pP_NO + '</td><td>' + v.emP_NAME + '</td><td>' + (v.BR_CODE || v.br_code || '') + '</td><td>' + v.loaN_CASE + '</td><td>' + v.lC_AMOUNT + '</td><td>' + v.accounT_NUMBER + '</td><td>' + v.acC_AMOUNT + '</td><td>' + v.remarks + '</td>';
                    if (!opts.readOnly && v.indicator === 'O') {
                        row += '<td class="text-center"><a href="#" class="updateResp">Update</a></td>';
                        row += '<td class="text-center"><a href="#" class="btn-resp-delete">Delete</a></td>';
                    } else {
                        row += '<td></td><td></td>';
                    }
                    row += '</tr>';
                    if (v.indicator === 'O') {
                        table.find('tbody').append(row); sr++; }
                    else if (changesTable.length) {
                        changesTable.find('tbody').append('<tr data-pp="' + (v.pP_NO || '') + '" data-branch="' + (v.BR_CODE || v.br_code || '') + '" data-loan="' + (v.loaN_CASE || '') + '" data-account="' + (v.accounT_NUMBER || '') + '"><td>' + sr_c + '</td><td>' + v.pP_NO + '</td><td>' + v.emP_NAME + '</td><td>' + (v.BR_CODE || v.br_code || '') + '</td><td>' + v.loaN_CASE + '</td><td>' + v.lC_AMOUNT + '</td><td>' + v.accounT_NUMBER + '</td><td>' + v.acC_AMOUNT + '</td><td>' + v.remarks + '</td><td></td><td></td></tr>'); sr_c++; }
                });
            }
        });
    }

    function getMatchedPP() {
        if ($('#responsiblePPNoEntryField').val() === "") {
            alert('Please enter PP Number to proceed');
            return;
        }
        respUser = [];
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_responsible_by_pp',
            type: 'POST',
            data: { 'PP_NO': $('#responsiblePPNoEntryField').val() },
            cache: false,
            success: function (data) {
                var items = Array.isArray(data) ? data : (data ? [data] : []);
                if (!items.length) {
                    alert('No record found..');
                    return;
                }
                items.forEach(function (item) {
                    if (!item) {
                        return;
                    }
                    var ppNo = item.ppNo || item.PP_NO || item.ppNumber || $('#responsiblePPNoEntryField').val();
                    var empName = item.name || item.empName || item.EMP_NAME || '';
                    var role = item.role || item.ROLE || item.respRole || '';
                    var loanCase = item.loanCase || item.LOAN_CASE || item.loanCaseNo || $('#loanCaseNumber').val() || $('#responsibleLoanNumberEntryField').val();
                    var lcAmount = item.lcAmount || item.LC_AMOUNT || item.loanAmount || $('#loanCaseAmount').val() || $('#responsibleLoanAmountEntryField').val();
                    var accountNumber = item.accountNumber || item.ACCOUNT_NUMBER || $('#responsibleAccountNumberEntryField').val();
                    var accAmount = item.accAmount || item.ACC_AMOUNT || $('#responsibleAccountAmountEntryField').val();
                    var branchCode = getBranchCodeValue(item.branchCode || item.BR_CODE || $('#responsibleBrCodeEntryField').val());
                    stageItem({
                        role: role,
                        ppNo: ppNo,
                        empName: empName,
                        branchCode: branchCode,
                        loanCase: loanCase,
                        lcAmount: lcAmount,
                        accountNumber: accountNumber,
                        accAmount: accAmount
                    });
                });
                renderPendingGrid();
            },
            dataType: 'json'
        });
    }

    function getLCDetails() {
        respUser = [];
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_responsible_by_lc',
            type: 'POST',
            data: {
                'LC_NO': $('#responsibleLCNoEntryField').val(),
                'BR_CODE': $('#responsibleBrCodeEntryField').val()
            },
            cache: false,
            success: function (data) {
                var response = Array.isArray(data) ? data : (data ? [data] : []);
                if (!response.length) {
                    alert('No record found..');
                    return;
                }
                response.forEach(function (d) {
                    var ppNo = d.ppNo || d.PP_NO || d.ppNumber || '';
                    var role = d.role || d.ROLE || d.respRole || '';
                    var name = d.name || d.empName || d.EMP_NAME || '';
                    var loanCase = d.loanCase || d.LOAN_CASE || d.loanCaseNo || $('#responsibleLCNoEntryField').val();
                    var lcAmount = d.lcAmount || d.LC_AMOUNT || d.outstandingAmount || '';
                    var accountNumber = d.accountNumber || d.ACCOUNT_NUMBER || d.accountNo || '';
                    var accAmount = d.accAmount || d.ACC_AMOUNT || d.accountAmount || '';
                    var branchCode = getBranchCodeValue(d.branchCode || d.BR_CODE || $('#responsibleBrCodeEntryField').val());
                    stageItem({
                        role: role,
                        ppNo: ppNo,
                        empName: name,
                        branchCode: branchCode,
                        loanCase: loanCase,
                        lcAmount: lcAmount,
                        accountNumber: accountNumber,
                        accAmount: accAmount
                    });
                });
                renderPendingGrid();
            },
            dataType: 'json'
        });
    }

    function saveStaged(action) {
        if (isSaving) return;
        if (!stagedResp.length) {
            alert('Select at least one responsible');
            return;
        }
        if ((opts.indicator !== 'O' && (!opts.engId || opts.engId <= 0)) ||
            (opts.indicator === 'O' && (!opts.comId || opts.comId <= 0))) {
            alert('Context missing: please ensure ENG_ID (for new obs) or COM_ID (for old paras) is set.');
            return;
        }
        isSaving = true;
        var $btns = $('#addResponsibleButton,#updateResponsibleButton,#deleteResponsibleButton').prop('disabled', true);
        var index = 0;
        var successCount = 0;

        function handleDone() {
            alert('Responsibilities added: ' + successCount);
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
            clearPending();
            isSaving = false;
            $btns.prop('disabled', false);
        }

        function handleFail(xhr) {
            var msg = 'Error occurred';
            if (xhr && xhr.responseJSON) {
                msg = xhr.responseJSON.Message || xhr.responseJSON.message || msg;
            }
            alert(msg);
            isSaving = false;
            $btns.prop('disabled', false);
        }

        function saveNext() {
            if (index >= stagedResp.length) {
                handleDone();
                return;
            }
            var item = stagedResp[index];
            index += 1;
            var rawBranchCode = getBranchCodeValue(item.branchCode);
            if (!rawBranchCode) {
                alert('Please enter Branch Code to proceed');
                isSaving = false;
                $btns.prop('disabled', false);
                return;
            }
            var rawLoanCase = $.trim(item.loanCase || '');
            var rawAccountNumber = $.trim(item.accountNumber || '');
            if (rawLoanCase === '' && rawAccountNumber === '') {
                alert('Please enter Either Loan Case Or Account Number to Proceed');
                isSaving = false;
                $btns.prop('disabled', false);
                return;
            }
            var loanCaseValue = normalizeNumericValue(rawLoanCase);
            var loanAmountValue = normalizeNumericValue(item.lcAmount || '');
            var accountNumberValue = normalizeNumericValue(rawAccountNumber);
            var accountAmountValue = normalizeNumericValue(item.accAmount || '');
            var ajaxOpts;
            if (opts.indicator === 'O') {
                ajaxOpts = {
                    url: g_asiBaseURL + '/ApiCalls/add_responsible_for_old_paras?IND_Action=' + action,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        'PP_NO': item.ppNo,
                        'LOAN_CASE': loanCaseValue,
                        'BR_CODE': rawBranchCode,
                        'LC_AMOUNT': loanAmountValue,
                        'ACCOUNT_NUMBER': accountNumberValue,
                        'ACC_AMOUNT': accountAmountValue,
                        'COM_ID': opts.comId
                    }),
                    dataType: 'json'
                };
            } else {
                ajaxOpts = {
                    url: action === 'D' ? g_asiBaseURL + '/ApiCalls/delete_responsible_from_observation' : g_asiBaseURL + '/ApiCalls/add_responsible_to_observation',
                    type: 'POST',
                    data: {
                        'PP_NO': item.ppNo,
                        'LOAN_CASE': loanCaseValue,
                        'BR_CODE': rawBranchCode,
                        'LC_AMOUNT': loanAmountValue,
                        'ACCOUNT_NUMBER': accountNumberValue,
                        'ACC_AMOUNT': accountAmountValue,
                        'EMP_NAME': item.empName || '',
                        'REMARKS': $('#resp_remarks').val() || '',
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
            $.ajax(ajaxOpts).done(function () {
                successCount += 1;
                saveNext();
            }).fail(handleFail);
        }

        saveNext();
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
        var rawLoanCase = getFirstValue(['#resp_loan_case', '#loanCaseNumber', '#responsibleLoanNumberEntryField', '.js-resp-loan-case']);
        var rawAccountNumber = getFirstValue(['#resp_account_number', '#responsibleAccountNumberEntryField', '.js-resp-account-number']);
        var rawBranchCode = getBranchCodeValue(getFirstValue(['#responsibleBrCodeEntryField', '.js-resp-branch-code']));
        if ($.trim(rawLoanCase) === '' && $.trim(rawAccountNumber) === '') {
            alert('Please enter Either Loan Case Or Account Number to Proceed');
            isSaving = false;
            $btns.prop('disabled', false);
            return;
        }
        if (!rawBranchCode) {
            alert('Please enter Branch Code to proceed');
            isSaving = false;
            $btns.prop('disabled', false);
            return;
        }
        var loanCaseValue = normalizeNumericValue(rawLoanCase);
        var loanAmountValue = normalizeNumericValue(getFirstValue(['#resp_loan_amount', '#loanCaseAmount', '#responsibleLoanAmountEntryField', '.js-resp-loan-amount']));
        var accountNumberValue = normalizeNumericValue(rawAccountNumber);
        var accountAmountValue = normalizeNumericValue(getFirstValue(['#resp_account_amount', '#responsibleAccountAmountEntryField', '.js-resp-account-amount']));
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
                    'LOAN_CASE': loanCaseValue,
                    'BR_CODE': rawBranchCode,
                    'LC_AMOUNT': loanAmountValue,
                    'ACCOUNT_NUMBER': accountNumberValue,
                    'ACC_AMOUNT': accountAmountValue,
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
                    'LOAN_CASE': loanCaseValue,
                    'BR_CODE': rawBranchCode,
                    'LC_AMOUNT': loanAmountValue,
                    'ACCOUNT_NUMBER': accountNumberValue,
                    'ACC_AMOUNT': accountAmountValue,
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
        var $row = $(this).closest('tr');
        selectedRow = $row;

        // toggle button visibility for update/delete mode
        $('#addResponsibleButton').addClass('d-none');
        $('#updateResponsibleButton').removeClass('d-none');
        $('#deleteResponsibleButton').removeClass('d-none');

        // populate entry fields from the selected row
        $('#responsiblePPNoEntryField').val($row.data('pp') || $row.children('td').eq(1).text());
        $('#responsibleBrCodeEntryField').val($row.data('branch') || $row.children('td').eq(3).text());
        $('#loanCaseNumber, #responsibleLoanNumberEntryField').val($row.data('loan') || $row.children('td').eq(4).text());
        $('#loanCaseAmount, #responsibleLoanAmountEntryField').val($row.children('td').eq(5).text());
        $('#responsibleAccountNumberEntryField').val($row.data('account') || $row.children('td').eq(6).text());
        $('#responsibleAccountAmountEntryField').val($row.children('td').eq(7).text());
        clearPending();
        stageItem({
            role: '',
            ppNo: $row.data('pp') || $row.children('td').eq(1).text(),
            empName: $row.children('td').eq(2).text(),
            branchCode: $row.data('branch') || $row.children('td').eq(3).text(),
            loanCase: $row.data('loan') || $row.children('td').eq(4).text(),
            lcAmount: $row.children('td').eq(5).text(),
            accountNumber: $row.data('account') || $row.children('td').eq(6).text(),
            accAmount: $row.children('td').eq(7).text()
        });
        renderPendingGrid();
    });

    table.off('click.resp', '.btn-resp-delete').on('click.resp', '.btn-resp-delete', function (e) {
        e.preventDefault();
        var $row = $(this).closest('tr');
        var ppNo = $row.data('pp') || '';
        var loanCase = $row.data('loan') || '';
        var branchCode = $row.data('branch') || '';
        var accountNumber = $row.data('account') || '';
        if (!branchCode) {
            alert('Please enter Branch Code to proceed');
            return;
        }
        if (!confirm('Are you sure to delete responsibility?')) {
            return;
        }
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/delete_responsible_from_observation',
            type: 'POST',
            data: {
                'ENG_ID': opts.engId,
                'NEW_PARA_ID': opts.newParaId,
                'OLD_PARA_ID': opts.oldParaId,
                'PP_NO': ppNo,
                'LOAN_CASE': loanCase,
                'BR_CODE': branchCode,
                'ACCOUNT_NUMBER': accountNumber
            },
            dataType: 'json'
        }).done(function (data) {
            var msg = data && (data.Message || data.message);
            if (msg) {
                alert(msg);
            }
            $row.remove();
        }).fail(function (xhr) {
            var msg = 'Error occurred';
            if (xhr && xhr.responseJSON) {
                msg = xhr.responseJSON.Message || xhr.responseJSON.message || msg;
            }
            alert(msg);
        });
    });

    modal.find('form').off('submit.resp').on('submit.resp', function (e) { e.preventDefault(); });

    if (!opts.readOnly) {
        $('#addResponsibleButton').off('click.resp').on('click.resp', function () {
            saveStaged('A');
        });
        $('#updateResponsibleButton').off('click.resp').on('click.resp', function () { saveStaged('U'); });
        $('#deleteResponsibleButton').off('click.resp').on('click.resp', function () {
            if (confirm('Are you sure you want to delete this responsibility?')) {
                saveStaged('D');
            }
        });
        $('#responsiblePPNoEntryField').off('keypress.resp').on('keypress.resp', function (e) { if (e.which === 13) { e.preventDefault(); getMatchedPP(); } });
        modal.off('show.bs.modal.resp').on('show.bs.modal.resp', function () {
            $('#matchedPPNoPanels').empty();
            $('#matchedPPNoPanelsBYPP').empty();
            selectedRow = null;
            respUser = [];
            $('#responsiblePPNoEntryField').val('');
            $('#responsibleBrCodeEntryField').val('');
            $('#loanCaseNumber').val('');
            $('#loanCaseAmount').val('');
            $('#responsibleLoanNumberEntryField').val('');
            $('#responsibleLoanAmountEntryField').val('');
            $('#responsibleAccountNumberEntryField').val('');
            $('#responsibleAccountAmountEntryField').val('');
            $('#addResponsibleButton').removeClass('d-none');
            $('#updateResponsibleButton').addClass('d-none');
            $('#deleteResponsibleButton').addClass('d-none');
            clearPending();
        });
    }

    function updateContext(c) {
        opts = $.extend(opts, c || {});
        load();
    }

    modal.off('click.resp', '.respPendingRemove').on('click.resp', '.respPendingRemove', function () {
        var key = $(this).data('key');
        unstageItem(key);
        renderPendingGrid();
    });

    applyDigitsOnly(modal);

    load();

    return {
        reload: load,
        updateContext: updateContext,
        getMatchedPP: getMatchedPP,
        getLCDetails: getLCDetails,
        saveResp: saveResp,
        saveStaged: saveStaged
    };
}

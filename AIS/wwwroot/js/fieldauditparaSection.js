function initFieldAuditParaSection(config) {
    var opts = $.extend({
        containerSelector: '#fieldAuditParaSection',
        readOnly: false,
        annexList: []
    }, config || {});

    var container = $(opts.containerSelector);

    var annexureSel = container.find('#auditPara_Annex');
    var riskDisplay = container.find('#viewMemo_risk_display');
    var processSel = container.find('#auditPara_Process');
    var subProcessSel = container.find('#auditPara_SubProcess');
    var checklistSel = container.find('#auditPara_Checklist');
    var gistField = container.find('#auditPara_Gist');
    var paraTextField = container.find('#paraTextViewer');

    var selectedRiskId = 0;

    function updateRiskDisplay() {
        var annexId = annexureSel.val();
        var riskName = '';
        selectedRiskId = 0;
        $.each(opts.annexList, function (i, v) {
            var id = v.ID || v.id;
            if (id == annexId) {
                riskName = v.RISK || v.risk;
                selectedRiskId = v.RISK_ID || v.risK_ID;
            }
        });
        riskDisplay.val(riskName);
        var color = '';
        if (riskName && riskName.toLowerCase() === 'high') {
            color = 'red';
        } else if (riskName && riskName.toLowerCase() === 'medium') {
            color = 'gold';
        } else if (riskName && riskName.toLowerCase() === 'low') {
            color = 'green';
        }
        riskDisplay.css('color', color);
    }

    function loadSubProcess() {
        if (!subProcessSel.length) return;
        subProcessSel.empty().append('<option value="0" id="0">--Select Sub-Process--</option>');
        if (processSel.length && processSel.val() != 0) {
            $.ajax({
                url: g_asiBaseURL + '/Setup/process_details',
                type: 'POST',
                data: { 'ProcessId': processSel.val() },
                dataType: 'json',
                success: function (data) {
                    $.each(data, function (index, pid) {
                        subProcessSel.append('<option value="' + pid.id + '" id="' + pid.id + '">' + pid.title + '</option>');
                    });
                }
            });
        }
    }

    function loadChecklist() {
        if (!checklistSel.length) return;
        checklistSel.empty().append('<option value="0" id="0">--Select Checklist Detail--</option>');
        if (subProcessSel.length && subProcessSel.val() != 0) {
            $.ajax({
                url: g_asiBaseURL + '/Setup/process_transactions',
                type: 'POST',
                data: { 'ProcessDetailId': subProcessSel.val() },
                dataType: 'json',
                success: function (data) {
                    $.each(data, function (index, clid) {
                        checklistSel.append('<option value="' + clid.id + '" id="' + clid.id + '">' + clid.description + '</option>');
                    });
                }
            });
        }
    }

    function setReadOnly(val) {
        opts.readOnly = val;
        var fields = [annexureSel, processSel, subProcessSel, checklistSel, gistField, paraTextField];
        $.each(fields, function (i, f) {
            if (f.length) f.prop('disabled', val);
        });
    }

    function getData() {
        return {
            annexureId: annexureSel.val(),
            riskId: selectedRiskId,
            processId: processSel.val(),
            subProcessId: subProcessSel.val(),
            checklistId: checklistSel.val(),
            gist: gistField.val(),
            paraText: paraTextField.val()
        };
    }

    function loadData(d) {
        if (!d) return;
        annexureSel.val(d.annexureId);
        updateRiskDisplay();
        if (processSel.length) processSel.val(d.processId);
        loadSubProcess();
        if (subProcessSel.length) subProcessSel.val(d.subProcessId);
        loadChecklist();
        if (checklistSel.length) checklistSel.val(d.checklistId);
        gistField.val(d.gist);
        paraTextField.val(d.paraText).trigger('change');
    }

    annexureSel.off('change.fap').on('change.fap', updateRiskDisplay);
    processSel.off('change.fap').on('change.fap', loadSubProcess);
    subProcessSel.off('change.fap').on('change.fap', loadChecklist);

    updateRiskDisplay();
    setReadOnly(opts.readOnly);

    return {
        updateRiskDisplay: updateRiskDisplay,
        reloadSubProcess: loadSubProcess,
        reloadChecklist: loadChecklist,
        setReadOnly: setReadOnly,
        getData: getData,
        loadData: loadData
    };
}


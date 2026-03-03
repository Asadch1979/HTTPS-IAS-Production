    var g_proc_id = 0;
    var g_sub_proc_id = 0;
   
    function getProcessChilds(procId) {
        if (!$('#procItem_' + procId + ' >span').hasClass('caret-down')) {

            $.ajax({
                url: g_asiBaseURL + "/Setup/process_details",
                type: "POST",
                data: {
                    'ProcessId': procId
                },
                cache: false,
                success: function (data) {
                    $.each(data, function (index, child) {
                        $('#procItem_' + procId).append('<ul class="childLevel"><li id="procTrans_' + child.id + '"><span onclick="event.preventDefault();getProcessTransactions(' + child.id + ')"  class="caret">' + child.title + '</span></li></ul>');

                    });
                    $('#procItem_' + procId + ' >span').after('<ul class="childLevel"><li><span onclick="event.preventDefault();addNewSubProcess(' + procId + ')" class="newitemaddmenu text-success" >-- Add New Sub Process--</span></li></ul>')
                    $('#procItem_' + procId + ' >span').addClass('caret-down');

                },
                dataType: "json",
            });
        } else {
            $('#procItem_' + procId + ' >span').removeClass('caret-down');
            // $('#procItem_' + procId + ' >span').addClass('caret');
            $('#procItem_' + procId + ' .childLevel').remove();
        }
    }
    function getProcessTransactions(procDetailId) {
        if (!$('#procTrans_' + procDetailId + ' >span').hasClass('caret-down')) {

            $.ajax({
                url: g_asiBaseURL + "/Setup/process_transactions",
                type: "POST",
                data: {
                    'ProcessDetailId': procDetailId
                },
                cache: false,
                success: function (data) {

                    console.log('tranc', data);
                    $.each(data, function (index, child) {
                        $('#procTrans_' + procDetailId).append('<ul class="grandchildLevel"><li  class="listItems"><span id="transactionspan_' + child.id + '" onclick="event.preventDefault();showTransactionDetail(this);" class="icon">' + child.description + '</span></li></ul>');

                    });
                    $('#procTrans_' + procDetailId + ' >span').after('<ul class="grandchildLevel"><li><span onclick="event.preventDefault();addNewSubProcessTransaction(' + procDetailId + ');" class="newitemaddmenu text-success" >-- Add New Transaction--</span></li></ul>')
                    $('#procTrans_' + procDetailId + ' >span').addClass('caret-down');

                },
                dataType: "json",
            });
        } else {
            $('#procTrans_' + procDetailId + ' >span').removeClass('caret-down');
            $('.grandchildLevel').remove();
        }
    }

    function showTransactionDetail(e) {
        var trId = $(e).attr('id');
        trId = trId.replace('transactionspan_', '')

        if ($('#' + $(e).attr('id')).hasClass('listItems-selected')) {
            $('.icon').removeClass('listItems-selected');
            $('#processTransactionViewer').hide();
        } else {
            $('.icon').removeClass('listItems-selected');
            $('#' + $(e).attr('id')).addClass('listItems-selected');
            $('#processTransactionViewer').show();
            $.ajax({
                url: g_asiBaseURL + "/Setup/process_transactions",
                type: "POST",
                data: {
                    'transactionId': trId
                },
                cache: false,
                success: function (data) {
                    g_trList = data;
                    var tr = data[0];
                    $('#viewerNameModalField').val(tr.procesS_NAME);
                    $('#viewerViolationNameModalField').val(tr.violatioN_NAME);
                    $('#viewerDescModalField').val(tr.description);
                    $('#viewerControlModalField').val(tr.controL_OWNER);
                    $('#viewerDivModalField').val(tr.diV_NAME);
                   // $('#viewerActionModalField').val(tr.action);
                    $('#viewerRiskModalField').val(tr.risk);
                   // $('#viewerRiskMaxModalField').val(tr.risK_MAX_NUMBER);
                },
                dataType: "json",
            });
        }
    }
    function addNewProcess() {
        $('#addNewProcess').modal('show');
        $('#procNameModalField').val('');
        $('#procRiskModalField').val(0)
    }
    function publishNewProcess() {
        var name = $('#procNameModalField').val();
        var risk = $('#procRiskModalField option:selected').val();
        if (name == "" || risk == 0 ) {
            alert('Please provide all the details');
            return false;
        }
        $('#addNewProcess').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/process_add",
            type: "POST",
            data: {
                'P_NAME': name,
                'RISK_ID': risk
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                window.location = window.location.pathname;

            },
            dataType: "json",
        });
    }
    function addNewSubProcess(procId) {
        g_proc_id = procId;
        $('#addNewSubProcess').modal('show');
        $('#subProcNameModalField').val($('#procItem_' + procId+'>span').text());
        $('#subProcTitleModalField').val('');
    }
    function publishNewSubProcess() {
        var title = $('#subProcTitleModalField').val();
        var ent_type = $('#subProcRiskModalField option:selected').val();
        if (title == "" || ent_type==0) {
            alert('Please provide all the details');
            return false;
        }
        $('#addNewSubProcess').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/sub_process_add",
            type: "POST",
            data: {
                'TITLE': title,
                'P_ID': g_proc_id,
                'ENTITY_TYPE': $('#subProcRiskModalField option:selected').val()
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                window.location = window.location.pathname;

            },
            dataType: "json",
        });
    }
    function addNewSubProcessTransaction(subProcId) {
        g_sub_proc_id = subProcId;
        $('#addNewProcessTransaction').modal('show');
        $('#tranSubProcNameModalField').val($('#procTrans_' + subProcId + '>span').text());
        $('#tranDescModalField').val('');
        $('#tranControlOwnerModalField').val(0);
        $('#tranDivModalField').val(0);
        $('#tranActionModalField').val(0);
        $('#tranRiskModalField').val(0);
        $('#tranRiskMaxNumberModalField').val(0);
    }
    function publishNewSubProcessTransaction() {
        var desc = $('#tranDescModalField').val();
        var control = $('#tranControlOwnerModalField').val();
        var div_id = $('#tranDivModalField option:selected').val();
        var action = $('#tranActionModalField').val();
        var div_name = $('#tranDivModalField option:selected').text();
        var risk = $('#tranRiskModalField option:selected').val();
        if (control == 0|| action == 0 || risk == 0 || div_id == 0) {
            alert('Please provide all the details');
            return false;
        }
        $('#addNewProcessTransaction').modal('hide');
        $.ajax({
            url: g_asiBaseURL + "/Setup/sub_process_transaction_add",
            type: "POST",
            data: {
                'PD_ID': g_sub_proc_id,
                'V_ID': control,
                'DESCRIPTION': desc,
                'CONTROL_OWNER': div_id,
                'DIV_ID': div_id,
                'DIV_NAME': div_name,
                'ACTION': action,
                'RISK_WEIGHTAGE': risk
            },
            cache: false,
            success: function (data) {
                //console.log(data);
                window.location = window.location.pathname;

            },
            dataType: "json",
        });
    }
    function getControlViolationMaxNumber() {
        if ($('#tranControlOwnerModalField option:selected').val() == 0) {
            $('#tranRiskMaxNumberModalField').val(0);
        } else {
            $('#tranRiskMaxNumberModalField').val($('#tranControlOwnerModalField option:selected').attr('max-number'));
        }
    }

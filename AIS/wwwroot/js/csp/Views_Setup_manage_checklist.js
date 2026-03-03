    var g_procId = 0;
    var g_subProcId = 0;
    var g_subProcList=[];
    $(document).ready(function () {
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfDepartment tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });


    
    function openUpdateSubCheclist(id, text, seq, weight) {
        g_subProcId=id;
        $('#updateEntityModal').modal('show');
        $('#entCodeField').val(text);
        $('#entSeqField').val(seq);
        $('#entWeightField').val(weight);
        
    }
    function reloadLocation(){
        window.location.reload();
    }

    function updateSubProcess() {
        if (document.querySelectorAll('input.alnum-only.is-invalid').length > 0) {
            Swal.fire({ icon: "error", title: "Validation error", text: "Please correct highlighted fields." });
            return;
        }
        if ($('#entCodeField').val()==""){
            alert("Please enter Checklist Title");
            return;
        }

        if ($('#entSeqField').val() == "") {
            alert("Please enter Risk Sequence");
            return;
        }

        if ($('#entWeightField').val() == "") {
            alert("Please enter Risk Weightage");
            return;
        }
      
        if (g_subProcId !=0){
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/update_audit_checklist",
                type: "POST",
                data: {
                    'PROCESS_ID': g_subProcId,
                    'HEADING': $('#entCodeField').val(),
                    'RISK_SEQUENCE': $('#entSeqField').val(),
                    'RISK_WEIGHTAGE': $('#entWeightField').val(),
                    'ACTIVE': $('#entActiveField').val() 
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }else{
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/add_audit_checklist",
                type: "POST",
                data: {
                    'HEADING': $('#entCodeField').val(),
                    'ENTITY_TYPE_ID': 6, //Hardcoded 6 for Branch,
                    'RISK_SEQUENCE': $('#entSeqField').val(),
                    'RISK_WEIGHTAGE': $('#entWeightField').val(),
                    'ACTIVE': $('#entActiveField').val(),
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    onAlertCallback(reloadLocation);
                },
                dataType: "json",
            });
        }
      
    }

    function addNewCheckList(id) {
        g_subProcId = id;
        $('#updateEntityModal').modal('show');
        $('#entCodeField').val('');
        $('#entSeqField').val('');
        $('#entWeightField').val('');
    }

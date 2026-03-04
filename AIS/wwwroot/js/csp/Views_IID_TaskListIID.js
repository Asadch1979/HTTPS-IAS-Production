    $(function(){
        var statuses = {
            approvedForInquiry: 'APPROVED_FOR_INQUIRY',
            planDrafted: 'PLAN_DRAFTED',
            planApproved: 'PLAN_APPROVED',
            reportDrafted: 'REPORT_DRAFTED',
            qcReturned: 'QC_RETURNED',
            qcCleared: 'QC_CLEARED'
        };

        function resolveValue(row, upperKey, lowerKey, pascalKey){
            return row[lowerKey] ?? row[pascalKey] ?? row[upperKey] ?? "";
        }

        function buildActions(row){
            var status = resolveValue(row, 'STATUS', 'status', 'Status');
            var complaintIdRaw = resolveValue(row, 'COMPLAINT_ID', 'complaintId', 'ComplaintId');
            var complaintId = parseInt(complaintIdRaw, 10);
            if(isNaN(complaintId)){ complaintId = 0; }

            var planId = resolveValue(row, 'PLAN_ID', 'planId', 'PlanId');
            var planExists = planId && Number(planId) > 0;

            var actions = [];

            if(status === statuses.approvedForInquiry && !planExists){
                actions.push('<a href="' + g_asiBaseURL + '/IID/InvestigationPlan?complaintId=' + complaintId + '">Create Plan</a>');
            }

            if(status === statuses.planDrafted){
                actions.push('<span class="text-muted">Plan Pending Approval</span>');
            }

            if(status === statuses.planApproved){
                actions.push('<a href="' + g_asiBaseURL + '/IID/InquiryReport?complaintId=' + complaintId + '">Draft Inquiry Report</a>');
            }

            if(status === statuses.qcCleared){
                actions.push('<a href="' + g_asiBaseURL + '/IID/InquiryReport?complaintId=' + complaintId + '">Submit Final Report</a>');
            }

            if(complaintId > 0){
                actions.push('<a href="' + g_asiBaseURL + '/IID/InquiryReport?complaintId=' + complaintId + '">Inquiry Report</a>');
            }

            actions.push('<a href="' + g_asiBaseURL + '/sampling/list_reports?engId=' + complaintId + '">Exception Reports</a>');

            if(actions.length === 0){
                actions.push('<span class="text-muted">-</span>');
            }

            return actions.join(' | ');
        }

        function loadTasks(){
            $.get(g_asiBaseURL + '/ApiCalls/GetIidTaskList', function(d){
                destroyDatatable('iidTaskListTable');
                var body = $('#iidTaskListTable tbody');
                body.empty();

                $.each(d, function(i, row){
                    var complaintNo = resolveValue(row, 'COMPLAINT_NO', 'complaintNo', 'ComplaintNo');
                    var complainantName = resolveValue(row, 'COMPLAINANT_NAME', 'complainantName', 'ComplainantName');
                    var assignedOn = resolveValue(row, 'ASSIGNED_ON', 'assignedOn', 'AssignedOn');
                    var status = resolveValue(row, 'STATUS', 'status', 'Status');
                    var actions = buildActions(row);

                    var html = '<tr>' +
                        '<td>' + (complaintNo || '') + '</td>' +
                        '<td>' + (complainantName || '') + '</td>' +
                        '<td>' + (assignedOn || '') + '</td>' +
                        '<td>' + (status || '') + '</td>' +
                        '<td>' + actions + '</td>' +
                        '</tr>';
                    body.append(html);
                });
                initializeDataTable('iidTaskListTable');
            });
        }

        loadTasks();
    });

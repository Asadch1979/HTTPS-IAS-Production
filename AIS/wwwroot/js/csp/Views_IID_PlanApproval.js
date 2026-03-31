    $(function(){
        var pageRoot = document.getElementById('iidPlanApprovalRoot');
        var complaintId = pageRoot ? (parseInt(pageRoot.getAttribute('data-complaint-id') || '0', 10) || 0) : 0;
        var planId = pageRoot ? (parseInt(pageRoot.getAttribute('data-plan-id') || '0', 10) || 0) : 0;
        var dashboardMode = !!(pageRoot && pageRoot.getAttribute('data-dashboard-mode') === 'true');
        var pageId = 349;

        function parsePositiveInt(value){
            var parsed = parseInt(value, 10);
            return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
        }

        function showIidAlert(title, message){
            $('#iidAlertTitle').text(title || 'Message');
            $('#iidAlertBody').text(message || '');
            $('#iidAlertModal').modal('show');
        }

        function hideIidLoader(){
            $('#iidLoading, .iid-loading, .loading').hide();
        }


        function loadComplaintDropdown(pageId){
            return $.post(g_asiBaseURL + '/ApiCalls/GetComplaintsDropdown', { pageId: pageId });
        }

        var submittedPlanFields = [
            { key: 'planId', label: 'Plan Id' },
            { key: 'complaintId', label: 'Complaint Id' },
            { key: 'planDetails', label: 'Plan Details' },
            { key: 'submittedBy', label: 'Submitted By' },
            { key: 'submittedOn', label: 'Submitted On' },
            { key: 'status', label: 'Status' },
            { key: 'planTitle', label: 'Plan Title' },
            { key: 'investigationRisk', label: 'Investigation Risk' },
            { key: 'investigationSize', label: 'Investigation Size' },
            { key: 'noOfDays', label: 'No Of Days' },
            { key: 'travellingDays', label: 'Travelling Days' },
            { key: 'startDate', label: 'Start Date' },
            { key: 'teamLead', label: 'Team Lead' },
            { key: 'teamMembers', label: 'Team Members' },
            { key: 'activitiesText', label: 'Activities Text' }
        ];

        function renderSubmittedPlanGrid(planRow){
            var $body = $('#submittedPlanGrid tbody');
            $body.empty();

            if(!planRow){
                $body.append('<tr><td colspan="2" class="text-muted">Plan details not available.</td></tr>');
                return;
            }

            submittedPlanFields.forEach(function(field){
                var value = planRow[field.key];
                if(value === null || typeof value === 'undefined'){
                    value = '';
                }

                $body.append('<tr><td>' + $('<div/>').text(field.label).html() + '</td><td class="text-break">' + $('<div/>').text(String(value)).html() + '</td></tr>');
            });
        }

        function loadPlanDetailsByComplaintId(selectedComplaintId){
            return $.get(g_asiBaseURL + '/ApiCalls/GetIidPlanDetails', { complaintId: selectedComplaintId }, function(d){
                var planRow = Array.isArray(d) ? d[0] : d;
                renderSubmittedPlanGrid(planRow);

                if(!planRow){
                    planId = 0;
                    $('#approvalForm [name="PlanId"]').val('');
                    return;
                }

                planId = parsePositiveInt(planRow.planId);
                $('#approvalForm [name="PlanId"]').val(planId > 0 ? String(planId) : '');
                $('#assessmentText').text(planRow.ASSESSMENT || planRow.assessment || $('#assessmentText').text());
                $('#assessmentRecommendation').text(planRow.RECOMMENDATION || planRow.recommendation || $('#assessmentRecommendation').text());
                $('#assessmentUnit').text(planRow.ASSIGNED_UNIT_ID || planRow.assignedUnitId || $('#assessmentUnit').text());
            }).fail(function(){
                renderSubmittedPlanGrid(null);
                showIidAlert('Failed', 'Failed to load plan details.');
            }).always(function(){
                hideIidLoader();
            });
        }

        function loadComplaintDetails(selectedComplaintId){
            return $.post(g_asiBaseURL + '/ApiCalls/GetComplaint', { complaintId: selectedComplaintId })
                .done(function(d){
                    if(d && d.ok === false){
                        showIidAlert('Failed', d.message || 'Operation failed.');
                        return;
                    }

                    var normalized = iidNormalizeComplaint(d);
                    iidRenderComplaintDetails('#iidComplaintDetailsRoot', normalized);

                    $('#assessmentText').text(normalized.Assessment || '');
                    $('#assessmentRecommendation').text(normalized.Recommendation || '');
                    $('#assessmentUnit').text(normalized.AssignedUnit || '');
                })
                .fail(function(){
                    $('#iidComplaintDetailsRoot').html('<div class="text-danger">Failed to load complaint details.</div>');
                });
        }

        function loadPlanApprovalByComplaintId(selectedComplaintId){
            complaintId = selectedComplaintId;
            loadComplaintDetails(complaintId);
            loadPlanDetailsByComplaintId(complaintId);
        }

        $('#approvalForm').on('submit', function(e){
            e.preventDefault();

            var $form = $(this);
            var ok = true;
            $form.find('[required]').each(function(){
                if (!$(this).val() || !$(this).val().toString().trim()) { ok = false; $(this).focus(); return false; }
            });
            if (!ok) { showIidAlert('Required', 'Please fill all mandatory fields.'); return; }

            var selectedComplaintId = $('#selectedComplaintId').val();
            if(!selectedComplaintId || selectedComplaintId === '0'){
                showIidAlert('Required', 'Please select a complaint.');
                return;
            }

            var currentPlanId = parsePositiveInt($form.find('[name="PlanId"]').val()) || planId;
            if(!currentPlanId){
                showIidAlert('Required', 'Valid Plan ID is required before approval can be submitted.');
                return;
            }

            var payload = {
                PlanId: currentPlanId,
                IsApproved: ($form.find('[name="IsApproved"]').val() || '').trim(),
                EditedPlan: $form.find('[name="EditedPlan"]').val() || '',
                FurtherActions: $form.find('[name="FurtherActions"]').val() || ''
            };

            $.post(g_asiBaseURL + '/ApiCalls/AddPlanApproval', payload)
                .done(function(resp){
                    if(resp && resp.ok){
                        showIidAlert('Success', 'Action recorded. Approval ID: ' + resp.id);
                        loadPlanApprovalByComplaintId(parsePositiveInt(selectedComplaintId));
                        return;
                    }

                    showIidAlert('Failed', (resp && resp.message) ? resp.message : 'Error saving approval.');
                })
                .fail(function(xhr){
                    showIidAlert('Failed', (xhr && xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'Error saving approval.');
                });
        });

        if (dashboardMode) {
            if (complaintId > 0) {
                $('#selectedComplaintId').val(String(complaintId));
                loadPlanApprovalByComplaintId(complaintId);
            }
            return;
        }

        loadComplaintDropdown(pageId)
            .done(function(resp){
                if(resp && resp.ok === false){
                    showIidAlert('Failed', resp.message || 'Operation failed.');
                    return;
                }

                var $dd = $('#iidComplaintSelect');
                $dd.empty().append('<option value="">-- Select Complaint --</option>');

                (resp || []).forEach(function(x){
                    $dd.append('<option value="' + x.complaintId + '">' + x.displayText + '</option>');
                });

                $dd.off('change.iidPlanApproval').on('change.iidPlanApproval', function(){
                    var id = $(this).val();
                    $('#selectedComplaintId').val(id);
                    if(!id || id === '0'){
                        $('#iidComplaintDetailsRoot').html('');
                        $('#approvalForm [name="PlanId"]').val('');
                        renderSubmittedPlanGrid(null);
                        return;
                    }
                    loadPlanApprovalByComplaintId(parseInt(id, 10));
                });

                if(complaintId > 0){
                    $dd.val(String(complaintId));
                    $('#selectedComplaintId').val(String(complaintId));
                    loadPlanApprovalByComplaintId(complaintId);
                }
            })
            .fail(function(){
                showIidAlert('Failed', 'Failed to load complaints dropdown.');
            })
            .always(function(){
                hideIidLoader();
            });
    });

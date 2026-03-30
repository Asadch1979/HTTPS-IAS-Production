    $(function(){
        var complaintId = '@complaintId';
        var pageId = 348;
        var todayDate = new Date();
        todayDate.setHours(0, 0, 0, 0);
        var minInquiryDate = new Date(todayDate);
        minInquiryDate.setDate(minInquiryDate.getDate() + 1);
        var minInquiryDateStr = minInquiryDate.toISOString().split('T')[0];

        $('#TenureFrom').attr('min', minInquiryDateStr);

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

        function loadComplaintSummary(selectedComplaintId){
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetComplaint',
                type: 'POST',
                data: { complaintId: selectedComplaintId },
                success: function(d){
                    if(d && d.ok === false){
                        showIidAlert('Failed', d.message || 'Operation failed.');
                        return;
                    }
                    var normalized = iidNormalizeComplaint(d);
                    iidRenderComplaintDetails('#iidComplaintDetailsRoot', normalized);
                    $('#assessmentText').text(normalized.Assessment || '');
                    $('#assessmentRecommendation').text(normalized.Recommendation || '');
                    $('#assessmentUnit').text(normalized.AssignedUnit || '');
                },
                error: function(){
                    showIidAlert('Failed', 'Failed to load complaint details.');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        function loadInvestigationPlanByComplaintId(selectedComplaintId){
            complaintId = selectedComplaintId;
            $('#planForm [name="ComplaintId"]').val(complaintId);
            loadComplaintSummary(complaintId);

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetLatestPlanByComplaintId',
                type: 'POST',
                data: { complaintId: complaintId },
                success: function(resp){
                    if(resp && resp.ok === false){
                        $('#PlanText').val('');
                        showIidAlert('Failed', resp.message || 'Operation failed.');
                        return;
                    }
                    var planNote = (resp.planDetails || '').trim();
                    $('#PlanText').val(planNote);
                    $('#PlanNote').val(planNote);
                },
                error: function(){
                    showIidAlert('Failed', 'Failed to load investigation plan.');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        $('#planForm').on('submit', function (e) {
            e.preventDefault();

            var token = $('input[name="__RequestVerificationToken"]', this).val();

            var acts = [];
            $('.act:checked').each(function(){ acts.push($(this).val()); });
            var otherText = ($('#OtherActivityText').val() || '').trim();
            if (acts.indexOf('Other') >= 0 && otherText) {
                acts[acts.indexOf('Other')] = 'Other: ' + otherText;
            }

            var planDetails = ($('#PlanNote').val() || '').trim();
            $('#PlanText').val(planDetails);

            var payload = {
                complaintId: parseInt(($('input[name="ComplaintId"]').val() || '0'), 10),
                planDetails: planDetails,
                status: 'Plan Drafted',
                planTitle: 'Investigation Plan',
                investigationRisk: ($('#InvestigationRisk').val() || '').trim(),
                investigationSize: ($('#InvestigationSize').val() || '').trim(),
                noOfDays: $('#NoOfDays').val() ? parseInt($('#NoOfDays').val(), 10) : null,
                travellingDays: $('#TravellingDay').val() ? parseInt($('#TravellingDay').val(), 10) : null,
                startDate: $('#TenureFrom').val() ? $('#TenureFrom').val() : null,
                teamLead: ($('#TeamLead').val() || '').trim(),
                teamMembers: ($('#TeamMember').val() || '').trim(),
                activitiesText: acts.join(', ')
            };

            console.log('payload', payload);

            if (!payload.complaintId || payload.complaintId <= 0) { alert('ComplaintId is required.'); return; }
            if (!payload.investigationRisk || !payload.investigationSize || !payload.noOfDays
                || !payload.startDate || !payload.teamLead || !payload.teamMembers) {
                alert('Please fill all mandatory fields.'); return;
            }
            if (!payload.activitiesText) { alert('Please select at least one Investigation Activity.'); return; }

            if (payload.startDate && payload.startDate <= todayDate.toISOString().split('T')[0]) {
                alert('Inquiry Conduction Date must be a future date.');
                return;
            }

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/AddInvestigationPlan',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(payload),
                headers: { 'RequestVerificationToken': token },
                success: function(resp){
                    if (resp && resp.ok) {
                        showIidAlert('Success', 'Plan submitted. Plan ID: ' + resp.id);
                        $('#iidAlertModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
                            location.reload();
                        });
                    }
                    else {
                        alert((resp && resp.message) ? resp.message : 'Failed.');
                    }
                },
                error: function(xhr){
                    console.log('HTTP', xhr.status, xhr.responseText);
                    alert('Error submitting plan. Check console.');
                }
            });
        });

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

                $dd.off('change').on('change', function(){
                    var id = $(this).val();
                    $('#selectedComplaintId').val(id);
                    if(!id || id === '0'){
                        $('#iidComplaintDetailsRoot').html('');
                        return;
                    }
                    loadInvestigationPlanByComplaintId(parseInt(id));
                });

                if(complaintId){
                    $dd.val(complaintId).trigger('change');
                }
            })
            .fail(function(){
                showIidAlert('Failed', 'Failed to load complaints dropdown.');
            })
            .always(function(){
                hideIidLoader();
            });
    });

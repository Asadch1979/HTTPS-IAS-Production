    $(function(){
        var complaintId = '@complaintId';
        var currentAssessmentId = '@assessmentId';
        var assignedUnitId = 0;
        var pageId = 2;

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
        function loadUnits(){
            $('#AssignedToUnit').empty().append('<option value="">--Select--</option>');
            return $.get(g_asiBaseURL + '/ApiCalls/GetIiUnits', function(d){
                $.each(d, function(i, unit){
                    $('#AssignedToUnit').append('<option value="'+unit.unitId+'">'+unit.unitName+'</option>');
                });
            }).fail(function(){
                showIidAlert('Failed', 'Failed to load inspection units.');
            });
        }


        function loadPageDataByComplaintId(selectedComplaintId){
            complaintId = selectedComplaintId;
            $('#reviewForm [name="ComplaintId"]').val(complaintId);
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetComplaint',
                type: 'POST',
                data: { complaintId: complaintId },
                success: function(d){
                    if(d && d.ok === false){
                        showIidAlert('Failed', d.message || 'Operation failed.');
                        return;
                    }
                    var normalized = iidNormalizeComplaint(d);
                    iidRenderComplaintDetails('#iidComplaintDetailsRoot', normalized);
                    $('#assessmentText').text(normalized.Assessment || '');
                    $('#assessmentRecommendation').text(normalized.Recommendation || '');
                    currentAssessmentId = d.assessmentId || d.AssessmentId || currentAssessmentId || 0;
                    $('#AssessmentId').val(currentAssessmentId);

                    assignedUnitId = d.assignedUnitId || d.AssignedToUnit || d.assignedToUnit || d.AssignedUnitId || 0;
                    $('#AssignedToUnit').val(String(assignedUnitId || '')).trigger('change');
                },
                error: function(){
                    showIidAlert('Failed', 'Failed to load complaint details.');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        function updateAssignedOn(){
            var today = new Date();
            $('#assignedOnDisplay').val(today.toISOString().split('T')[0]);
        }

        $('#reviewForm').on('submit', function(e){
            e.preventDefault();

            var action = $('#reviewForm [name="Action"]:checked').val();
            var complaintId = $('#reviewForm [name="ComplaintId"]').val();
            var assignedToUnit = $('#reviewForm [name="AssignedToUnit"]').val();

            if(!complaintId || complaintId === '0'){
                showIidAlert('Required', 'Please select a complaint.');
                return;
            }

            if(action === 'APPROVE' && !assignedToUnit){
                showIidAlert('Failed', 'Assigned unit is required.');
                return;
            }

            var assessmentIdValue = $('#AssessmentId').val() || currentAssessmentId || '@assessmentId';
            $('#AssessmentId').val(assessmentIdValue);

            var data = $(this).serialize();
            if(!/AssessmentId=/.test(data) || /AssessmentId=(?:0)?(?:&|$)/.test(data)){
                data = data.replace(/(^|&)AssessmentId=[^&]*/,'').replace(/^&/,'');
                data += (data ? '&' : '') + 'AssessmentId=' + encodeURIComponent(assessmentIdValue);
            }

            $.post(g_asiBaseURL + '/ApiCalls/AddHeadReview', data, function(response){
                if(response && response.ok === false){
                    showIidAlert('Failed', response.message || 'Operation failed.');
                    return;
                }

                showIidAlert('Success', 'Saved successfully.');
                $('#iidAlertModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
                    location.reload();
                });
            }).fail(function(){
                showIidAlert('Failed', 'Failed to submit decision.');
            });
        });

        loadUnits().then(function(){
            if(assignedUnitId){
                $('#AssignedToUnit').val(String(assignedUnitId)).trigger('change');
            }
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
                    loadPageDataByComplaintId(parseInt(id));
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
        updateAssignedOn();
    });

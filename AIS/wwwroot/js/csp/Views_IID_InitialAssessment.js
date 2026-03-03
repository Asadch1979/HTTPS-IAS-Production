    $(function(){
        var complaintId = '@complaintId';
        var referredBackStatus = 'REFERRED_BACK';
        var pageId = 1;

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

        function loadPageDataByComplaintId(selectedComplaintId){
            complaintId = selectedComplaintId;
            $('#assessmentForm [name="ComplaintId"]').val(complaintId);
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

                    if (normalized.Status === referredBackStatus) {
                        $('#assessmentForm [name="Assessment"]').val(normalized.Assessment || '');
                        $('#assessmentForm [name="Recommendation"]').val(normalized.Recommendation || '');
                        if (d.assignedUnitId || d.AssignedUnitId) {
                            $('#AssignedUnitId').val(d.assignedUnitId || d.AssignedUnitId);
                        }
                    }
                },
                error: function(){
                    showIidAlert('Failed', 'Failed to load complaint details. Please refresh.');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        function loadUnits(){
            $('#AssignedUnitId').empty().append('<option value="">--Select--</option>');
            $.get(g_asiBaseURL + '/ApiCalls/GetIiUnits', function(d){
                $.each(d, function(i, unit){
                    $('#AssignedUnitId').append('<option value="'+unit.unitId+'">'+unit.unitName+'</option>');
                });
            }).fail(function(){
                showIidAlert('Failed', 'Failed to load inspection units.');
            });
        }

        $('#assessmentForm').on('submit', function(e){
            e.preventDefault();
            var data = {
                ComplaintId: $('#assessmentForm [name="ComplaintId"]').val(),
                Assessment: $('#assessmentForm [name="Assessment"]').val(),
                Recommendation: $('#assessmentForm [name="Recommendation"]').val(),
                AssignedUnitId: $('#assessmentForm [name="AssignedUnitId"]').val()
            };

            if(!data.AssignedUnitId){
                showIidAlert('Failed', 'Assigned I&I Unit is required.');
                return;
            }

            if(!data.ComplaintId || data.ComplaintId === '0'){
                showIidAlert('Required', 'Please select a complaint.');
                return;
            }

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/AddAssessment',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function(response){
                    if(response && response.ok === false){
                        showIidAlert('Failed', response.message || 'Operation failed.');
                        return;
                    }
                    showIidAlert('Success', 'Assessment saved successfully.');
                    $('#iidAlertModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
                        location.reload();
                    });
                },
                error: function(){
                    showIidAlert('Failed', 'Could not save assessment.');
                }
            });
        });

        loadUnits();
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
    });

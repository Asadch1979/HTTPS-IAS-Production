    $(function(){
        var pageRoot = document.getElementById('iidCaseStudyRoot');
        var complaintId = pageRoot ? (parseInt(pageRoot.getAttribute('data-complaint-id') || '0', 10) || 0) : 0;
        var dashboardMode = !!(pageRoot && pageRoot.getAttribute('data-dashboard-mode') === 'true');
        var pageId = 342;

        function showIidAlert(message, type){
            type = type || 'danger';
            $('#iidAlertHost').html(
                '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
                (message || 'Unexpected error') +
                '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                '</div>'
            );
        }

        function hideIidLoader(){
            $('#iidLoading, .iid-loading, .loading').hide();
        }

        function loadComplaintDropdown(pageId){
            return $.post(g_asiBaseURL + '/ApiCalls/GetComplaintsDropdown', { pageId: pageId });
        }

        function loadPageDataByComplaintId(selectedComplaintId){
            complaintId = selectedComplaintId;
            $('#caseForm [name="ComplaintId"]').val(complaintId);
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetComplaint',
                type: 'POST',
                data: { complaintId: complaintId },
                success: function(d){
                    if(d && d.ok === false){
                        showIidAlert(d.message, 'danger');
                    }
                },
                error: function(){
                    showIidAlert('Failed to load complaint details.', 'danger');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        $('#caseForm').on('submit', function(e){
            e.preventDefault();
            var data = {
                ComplaintId: $('#caseForm [name="ComplaintId"]').val(),
                OriginProcessOwner: $('#caseForm [name="OriginProcessOwner"]').val(),
                NameComplainant: $('#caseForm [name="NameComplainant"]').val(),
                Branch: $('#caseForm [name="Branch"]').val(),
                Gist: $('#caseForm [name="Gist"]').val(),
                Outcome: $('#caseForm [name="Outcome"]').val(),
                ModusOperandi: $('#caseForm [name="ModusOperandi"]').val(),
                Gaps: $('#caseForm [name="Gaps"]').val(),
                RootCause: $('#caseForm [name="RootCause"]').val(),
                PolicyGapsIdentified: $('#caseForm [name="PolicyGapsIdentified"]').val(),
                ControlViolations: $('#caseForm [name="ControlViolations"]').val(),
                RiskIdentified: $('#caseForm [name="RiskIdentified"]').val(),
                RegulatoryComplianceFailure: $('#caseForm [name="RegulatoryComplianceFailure"]').val(),
                ActionsRec: $('#caseForm [name="ActionsRec"]').val(),
                Status: $('#caseForm [name="Status"]').val()
            };
            if(!data.ComplaintId || data.ComplaintId === '0'){
                showIidAlert('Required', 'Please select a complaint.');
                return;
            }

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/AddCaseStudy',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function(response){
                    if(response && response.ok === false){
                        showIidAlert(response.message, 'danger');
                        return;
                    }
                    showIidAlert('Case study added', 'success');
                    location.reload();
                },
                error: function(){
                    showIidAlert('Failed to add case study.', 'danger');
                }
            });
        });

        if (dashboardMode) {
            if (complaintId > 0) {
                $('#selectedComplaintId').val(String(complaintId));
                loadPageDataByComplaintId(complaintId);
            }
            return;
        }

        loadComplaintDropdown(pageId)
            .done(function(resp){
                if(resp && resp.ok === false){
                    showIidAlert(resp.message, 'danger');
                    return;
                }

                var $dd = $('#iidComplaintSelect');
                $dd.empty().append('<option value="">-- Select Complaint --</option>');

                (resp || []).forEach(function(x){
                    $dd.append('<option value="' + x.complaintId + '">' + x.displayText + '</option>');
                });

                $dd.off('change.iidCaseStudy').on('change.iidCaseStudy', function(){
                    var id = $(this).val();
                    $('#selectedComplaintId').val(id);
                    if(!id || id === '0'){
                        return;
                    }
                    loadPageDataByComplaintId(parseInt(id, 10));
                });

                if(complaintId > 0){
                    $dd.val(String(complaintId)).trigger('change');
                }
            })
            .fail(function(){
                showIidAlert('Failed to load complaints dropdown.', 'danger');
            })
            .always(function(){
                hideIidLoader();
            });
    });

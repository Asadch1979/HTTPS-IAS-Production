    $(function(){
        var complaintId = '@complaintId';
        var reportId = '@reportId';
        var pageId = 343;

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

        function loadReportFilesByReportId(id){
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetInquiryReportFiles',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ ReportId: id }),
                success: function(d){
                    if(d && d.ok === false){
                        showIidAlert(d.message, 'danger');
                        return;
                    }
                    var links = [];
                    if(d && d.uploadedReport){
                        links.push('<a href="'+g_asiBaseURL+'/Uploads/'+d.uploadedReport+'" target="_blank">Report</a>');
                    }
                    if(d && d.uploadedEvidence){
                        var evidences = d.uploadedEvidence.split(';');
                        $.each(evidences, function(i,f){
                            if(f){
                                links.push('<a href="'+g_asiBaseURL+'/Uploads/'+f+'" target="_blank">Evidence '+(i+1)+'</a>');
                            }
                        });
                    }
                    if(d && d.uploadedDsa){
                        links.push('<a href="'+g_asiBaseURL+'/Uploads/'+d.uploadedDsa+'" target="_blank">DSA</a>');
                    }
                    $('#reportLinks').html(links.length ? links.join('<br/>') : 'No files available.');
                },
                error: function(){
                    showIidAlert('Failed to load report files.', 'danger');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        function loadPageDataByComplaintId(selectedComplaintId){
            complaintId = selectedComplaintId;
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

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetLatestInquiryReportByComplaintId',
                type: 'POST',
                data: { complaintId: complaintId },
                success: function(resp){
                    if(resp && resp.ok === false){
                        showIidAlert(resp.message, 'danger');
                        $('#reportLinks').text('No files available.');
                        return;
                    }
                    reportId = resp.reportId;
                    $('#analysisForm [name="ReportId"]').val(reportId);
                    var links = [];
                    if(resp && resp.uploadedReport){
                        links.push('<a href="'+g_asiBaseURL+'/Uploads/'+resp.uploadedReport+'" target="_blank">Report</a>');
                    }
                    if(resp && resp.uploadedEvidence){
                        var evidences = resp.uploadedEvidence.split(';');
                        $.each(evidences, function(i,f){
                            if(f){
                                links.push('<a href="'+g_asiBaseURL+'/Uploads/'+f+'" target="_blank">Evidence '+(i+1)+'</a>');
                            }
                        });
                    }
                    if(resp && resp.uploadedDsa){
                        links.push('<a href="'+g_asiBaseURL+'/Uploads/'+resp.uploadedDsa+'" target="_blank">DSA</a>');
                    }
                    $('#reportLinks').html(links.length ? links.join('<br/>') : 'No files available.');
                },
                error: function(){
                    showIidAlert('Failed to load inquiry report details.', 'danger');
                },
                complete: function(){
                    hideIidLoader();
                }
            });
        }

        function toggleDecision(){
            if($('#Decision').val() === 'REFER_BACK'){
                $('#referBackDiv').removeClass('d-none');
            } else {
                $('#referBackDiv').addClass('d-none');
                $('#referBackDiv textarea').val('');
            }
        }

        if(reportId){
            $('#analysisForm [name="ReportId"]').val(reportId);
            loadReportFilesByReportId(reportId);
        }
        $('#Decision').on('change', toggleDecision);
        toggleDecision();

        $('#analysisForm').on('submit', function(e){
            e.preventDefault();
            var data = {
                ReportId: $('#analysisForm [name="ReportId"]').val(),
                PolicyGaps: $('#analysisForm [name="PolicyGaps"]').val(),
                ControlGaps: $('#analysisForm [name="ControlGaps"]').val(),
                ProceduralViolations: $('#analysisForm [name="ProceduralViolations"]').val(),
                ForwardTo: $('#analysisForm [name="ForwardTo"]').val(),
                Comments: $('#analysisForm [name="Comments"]').val(),
                Decision: $('#analysisForm [name="Decision"]').val(),
                ReferBackComments: $('#analysisForm [name="ReferBackComments"]').val()
            };
            var selectedComplaintId = $('#selectedComplaintId').val();
            if(!selectedComplaintId || selectedComplaintId === '0'){
                showIidAlert('Required', 'Please select a complaint.');
                return;
            }

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/AddAnalysis',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function(response){
                    if(response && response.ok === false){
                        showIidAlert(response.message, 'danger');
                        return;
                    }
                    showIidAlert('Analysis submitted successfully', 'success');
                    location.reload();
                },
                error: function(){
                    showIidAlert('Failed to submit analysis.', 'danger');
                }
            });
        });

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

                $dd.off('change').on('change', function(){
                    var id = $(this).val();
                    $('#selectedComplaintId').val(id);
                    if(!id || id === '0'){
                        return;
                    }
                    loadPageDataByComplaintId(parseInt(id));
                });

                if(complaintId){
                    $dd.val(complaintId).trigger('change');
                }
            })
            .fail(function(){
                showIidAlert('Failed to load complaints dropdown.', 'danger');
            })
            .always(function(){
                hideIidLoader();
            });
    });

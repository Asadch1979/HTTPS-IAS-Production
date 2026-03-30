    $(function(){
        var pageId = 350;
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

        function loadRegions(){
            $.post(g_asiBaseURL + '/ApiCalls/get_region_zone_office', { RGM_ID: 0 }, function(d){
                $('#RegionId').empty().append('<option value="">Region</option>');
                $.each(d, function(i,v){
                    var code = v.code || v.CODE;
                    var name = v.name || v.NAME;
                    $('#RegionId').append('<option value="'+code+'">'+name+'</option>');
                });
            }).fail(function(){
                showIidAlert('Failed to load regions.', 'danger');
            });
        }

        function loadHoUnitTypes(){
            $.post(g_asiBaseURL + '/ApiCalls/get_ho_unit_types', {}, function(d){
                $('#HOUnitTypeId').empty().append('<option value="">HO Unit Type</option>');
                $.each(d, function(i,v){
                    var id = v.divisionid || v.DIVISIONID;
                    var name = v.name || v.NAME;
                    $('#HOUnitTypeId').append('<option value="'+id+'">'+name+'</option>');
                });
            }).fail(function(){
                showIidAlert('Failed to load HO unit types.', 'danger');
            });
        }

        function loadHoUnits(){
            var typeId = $('#HOUnitTypeId').val();
            $('#HOUnitId').empty().append('<option value="">HO Unit</option>');
            if(!typeId){
                return;
            }
            $.post(g_asiBaseURL + '/ApiCalls/get_ho_units', { divisionId: typeId }, function(d){
                $.each(d, function(i,v){
                    var id = v.id || v.ID;
                    var name = v.name || v.NAME;
                    $('#HOUnitId').append('<option value="'+id+'">'+name+'</option>');
                });
            }).fail(function(){
                showIidAlert('Failed to load HO units.', 'danger');
            });
        }

        function togglePertainsTo(){
            var pertainsTo = $('#PertainsTo').val();
            if(pertainsTo === 'HO'){
                $('#HOUnitTypeId,#HOUnitId').prop('disabled', false);
                $('#RegionId,#BranchId').prop('disabled', true).val('');
            } else if(pertainsTo === 'FIELD'){
                $('#RegionId,#BranchId').prop('disabled', false);
                $('#HOUnitTypeId,#HOUnitId').prop('disabled', true).val('');
            } else {
                $('#HOUnitTypeId,#HOUnitId,#RegionId,#BranchId').prop('disabled', false);
            }
        }

        function getFieldValue(row, keys){
            for(var i = 0; i < keys.length; i++){
                var key = keys[i];
                if(row[key] !== undefined && row[key] !== null){
                    return row[key];
                }
                var lowerKey = key.toLowerCase();
                if(row[lowerKey] !== undefined && row[lowerKey] !== null){
                    return row[lowerKey];
                }
            }
            return '';
        }

        loadRegions();
        loadHoUnitTypes();
        togglePertainsTo();

        $('#HOUnitTypeId').on('change', loadHoUnits);
        $('#PertainsTo').on('change', togglePertainsTo);
        $('#RegionId').on('change', function(){
            if($(this).val()){
                $('#BranchId').empty().append('<option value="">Branch</option>');
                $.post(g_asiBaseURL + '/ApiCalls/get_zone_Branches', { ZONEID: $(this).val() }, function(d){
                    $.each(d, function(i,v){
                        $('#BranchId').append('<option value="'+v.branchid+'">'+v.branchname+'</option>');
                    });
                }).fail(function(){
                    showIidAlert('Failed to load branches.', 'danger');
                });
            }
        });

        $('#filterForm').on('submit', function(e){
            e.preventDefault();
            var selectedComplaintId = $('#selectedComplaintId').val();
            var data = {
                DateFrom: $('#filterForm [name="DateFrom"]').val(),
                DateTo: $('#filterForm [name="DateTo"]').val(),
                Nature: $('#filterForm [name="Nature"]').val(),
                Source: $('#filterForm [name="Source"]').val(),
                Category: $('#filterForm [name="Category"]').val(),
                PertainsTo: $('#filterForm [name="PertainsTo"]').val(),
                RegionId: $('#filterForm [name="RegionId"]').val(),
                BranchId: $('#filterForm [name="BranchId"]').val(),
                HOUnitTypeId: $('#filterForm [name="HOUnitTypeId"]').val(),
                HOUnitId: $('#filterForm [name="HOUnitId"]').val(),
                Status: $('#filterForm [name="Status"]').val(),
                ComplaintId: selectedComplaintId ? parseInt(selectedComplaintId) : null
            };
            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/GetReports',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function(d){
                    if(d && d.ok === false){
                        showIidAlert(d.message, 'danger');
                        return;
                    }
                    var body = $('#resultTable tbody');
                    body.empty();
                    $.each(d, function(i,v){
                        var complaintId = getFieldValue(v, ['ComplaintId','COMPLAINT_ID']);
                        var submittedOn = getFieldValue(v, ['SubmittedOn','SUBMITTED_ON']);
                        var source = getFieldValue(v, ['Source','SOURCE']);
                        var category = getFieldValue(v, ['Category','CATEGORY']);
                        var pertainsTo = getFieldValue(v, ['PertainsToSummary','PERTAINS_TO_SUMMARY','PERTAINS_TO']);
                        var status = getFieldValue(v, ['Status','STATUS']);
                        body.append('<tr><td>'+complaintId+'</td><td>'+submittedOn+'</td><td>'+source+'</td><td>'+category+'</td><td>'+pertainsTo+'</td><td>'+status+'</td></tr>');
                    });
                },
                error: function(){
                    showIidAlert('Failed to load reports.', 'danger');
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
                    loadReports();
                });
            })
            .fail(function(){
                showIidAlert('Failed to load complaints dropdown.', 'danger');
            })
            .always(function(){
                hideIidLoader();
            });
    });

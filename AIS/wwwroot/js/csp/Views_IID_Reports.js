$(function(){
    var pageRoot = document.getElementById('iidReportsRoot');
    if(!pageRoot){
        return;
    }

    var dashboardMode = String(pageRoot.getAttribute('data-dashboard-mode') || '').toLowerCase() === 'true';
    var complaintId = parseInt(pageRoot.getAttribute('data-complaint-id') || '0', 10) || 0;
    var tableId = 'iidReportsTable';
    var pageId = 350;
    var reportRows = [];

    function parsePositiveInt(value){
        var parsed = parseInt(value, 10);
        return isNaN(parsed) || parsed <= 0 ? null : parsed;
    }

    function showIidAlert(message, type){
        var safe = (typeof sanitizeAlertMessageText === 'function')
            ? sanitizeAlertMessageText(message)
            : ((message || 'Unexpected error').toString().trim());
        $('#iidReportsAlertHost').html(
            '<div class="alert alert-' + (type || 'danger') + ' alert-dismissible fade show" role="alert">' +
            $('<div/>').text(safe || 'Unexpected error').html() +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
            '</div>'
        );
    }

    function htmlEncode(value){
        return $('<div/>').text(value == null ? '' : value).html();
    }

    function loadRegions(){
        return $.post(g_asiBaseURL + '/ApiCalls/get_region_zone_office', { RGM_ID: 0 }, function(d){
            var $region = $('#RegionId');
            $region.empty().append('<option value="">All</option>');
            $.each(d || [], function(_, v){
                var code = v.code || v.CODE;
                var name = v.name || v.NAME;
                $region.append('<option value="' + htmlEncode(code) + '">' + htmlEncode(name) + '</option>');
            });
        }).fail(function(){
            showIidAlert('Failed to load regions.', 'danger');
        });
    }

    function loadHoUnitTypes(){
        return $.post(g_asiBaseURL + '/ApiCalls/get_ho_unit_types', {}, function(d){
            var $types = $('#HOUnitTypeId');
            $types.empty().append('<option value="">All</option>');
            $.each(d || [], function(_, v){
                var id = v.divisionid || v.DIVISIONID;
                var name = v.name || v.NAME;
                $types.append('<option value="' + htmlEncode(id) + '">' + htmlEncode(name) + '</option>');
            });
        }).fail(function(){
            showIidAlert('Failed to load HO unit types.', 'danger');
        });
    }

    function loadHoUnits(selectedValue){
        var typeId = $('#HOUnitTypeId').val();
        var $units = $('#HOUnitId');
        $units.empty().append('<option value="">All</option>');
        if(!typeId){
            return $.Deferred().resolve().promise();
        }
        return $.post(g_asiBaseURL + '/ApiCalls/get_ho_units', { divisionId: typeId }, function(d){
            $.each(d || [], function(_, v){
                var id = v.id || v.ID;
                var name = v.name || v.NAME;
                $units.append('<option value="' + htmlEncode(id) + '">' + htmlEncode(name) + '</option>');
            });
            if(selectedValue){
                $units.val(String(selectedValue));
            }
        }).fail(function(){
            showIidAlert('Failed to load HO units.', 'danger');
        });
    }

    function loadBranches(regionId, selectedValue){
        var $branches = $('#BranchId');
        $branches.empty().append('<option value="">All</option>');
        if(!regionId){
            return $.Deferred().resolve().promise();
        }
        return $.post(g_asiBaseURL + '/ApiCalls/get_zone_Branches', { ZONEID: regionId }, function(d){
            $.each(d || [], function(_, v){
                $branches.append('<option value="' + htmlEncode(v.branchid) + '">' + htmlEncode(v.branchname) + '</option>');
            });
            if(selectedValue){
                $branches.val(String(selectedValue));
            }
        }).fail(function(){
            showIidAlert('Failed to load branches.', 'danger');
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
            $('#HOUnitId').empty().append('<option value="">All</option>');
        } else {
            $('#HOUnitTypeId,#HOUnitId,#RegionId,#BranchId').prop('disabled', false);
        }
    }

    function getSelectedText(selector){
        return $(selector + ' option:selected').text() || '';
    }

    function buildFilterPayload(){
        var regionText = getSelectedText('#RegionId');
        var branchText = getSelectedText('#BranchId');
        var hoUnitText = getSelectedText('#HOUnitId');
        return {
            DateFrom: $('#filterForm [name="DateFrom"]').val(),
            DateTo: $('#filterForm [name="DateTo"]').val(),
            ComplaintId: parsePositiveInt($('#filterForm [name="ComplaintId"]').val()),
            Complaint: $('#filterForm [name="Complaint"]').val(),
            Nature: $('#filterForm [name="Nature"]').val(),
            Source: $('#filterForm [name="Source"]').val(),
            Category: $('#filterForm [name="Category"]').val(),
            PertainsTo: $('#filterForm [name="PertainsTo"]').val(),
            HOUnitTypeId: parsePositiveInt($('#filterForm [name="HOUnitTypeId"]').val()),
            HOUnitId: parsePositiveInt($('#filterForm [name="HOUnitId"]').val()),
            RegionId: parsePositiveInt($('#filterForm [name="RegionId"]').val()),
            BranchId: parsePositiveInt($('#filterForm [name="BranchId"]').val()),
            Accused: $('#filterForm [name="Accused"]').val(),
            Unit: $('#filterForm [name="Unit"]').val() || (hoUnitText !== 'All' ? hoUnitText : ''),
            Region: regionText !== 'All' ? regionText : '',
            Branch: branchText !== 'All' ? branchText : '',
            Status: $('#filterForm [name="Status"]').val()
        };
    }

    function getValue(row, keys){
        var keyIndex;
        var keysInRow = Object.keys(row || {});
        for(var i = 0; i < keys.length; i++){
            var wanted = keys[i];
            if(row[wanted] !== undefined && row[wanted] !== null){
                return row[wanted];
            }
            for(keyIndex = 0; keyIndex < keysInRow.length; keyIndex++){
                if(keysInRow[keyIndex].toLowerCase() === String(wanted).toLowerCase()){
                    return row[keysInRow[keyIndex]];
                }
            }
        }
        return '';
    }

    function shouldHideColumn(columnName){
        return /(^|_)(exceptioncount|exception_count|exc_count)(_|$)/i.test(String(columnName || '').replace(/\s+/g, '_'));
    }

    function buildPreferredColumns(rows){
        var preferred = [
            'COMPLAINT_ID', 'COMPLAINT_NO', 'NATURE', 'SOURCE', 'CATEGORY', 'PERTAINS_TO_SUMMARY',
            'REGION', 'BRANCH', 'ASSIGNED_UNIT', 'ACCUSED', 'STATUS', 'SUBMITTED_ON'
        ];
        var seen = {};
        var available = [];

        (rows || []).forEach(function(row){
            Object.keys(row || {}).forEach(function(key){
                if(shouldHideColumn(key)){ return; }
                if(!seen[key.toLowerCase()]){
                    seen[key.toLowerCase()] = key;
                    available.push(key);
                }
            });
        });

        var ordered = [];
        preferred.forEach(function(name){
            var match = available.find(function(key){ return key.toLowerCase() === name.toLowerCase(); });
            if(match){ ordered.push(match); }
        });

        available.forEach(function(key){
            if(ordered.indexOf(key) < 0){ ordered.push(key); }
        });

        return ordered;
    }

    function toHeaderLabel(columnName){
        var normalized = String(columnName || '')
            .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
            .replace(/_/g, ' ')
            .replace(/\s+/g, ' ')
            .trim();
        return normalized.replace(/\b\w/g, function(ch){ return ch.toUpperCase(); });
    }

    function updateSummaryCards(rows){
        var statuses = {};
        var coverage = {};
        (rows || []).forEach(function(row){
            var status = getValue(row, ['STATUS', 'Status']);
            var region = getValue(row, ['REGION', 'Region']);
            var branch = getValue(row, ['BRANCH', 'Branch']);
            var unit = getValue(row, ['ASSIGNED_UNIT', 'UNIT', 'Unit']);
            if(status){ statuses[String(status).trim().toUpperCase()] = true; }
            [region, branch, unit].forEach(function(value){
                var text = String(value || '').trim();
                if(text){ coverage[text.toUpperCase()] = true; }
            });
        });

        $('#iidReportsCount').text((rows || []).length);
        $('#iidReportsStatuses').text(Object.keys(statuses).length);
        $('#iidReportsCoverage').text(Object.keys(coverage).length);
        $('#iidReportsResultMeta').text((rows || []).length ? ((rows || []).length + ' report row(s) loaded.') : 'No report rows matched the current filters.');
    }

    function buildViewLink(row){
        var rowComplaintId = parsePositiveInt(getValue(row, ['COMPLAINT_ID', 'ComplaintId']));
        if(!rowComplaintId){
            return '<span class="text-muted">N/A</span>';
        }
        var href = dashboardMode
            ? (g_asiBaseURL + '/IID/IID_Dashboard?complaintId=' + encodeURIComponent(rowComplaintId) + '&utilityCode=READ_ONLY_REPORT')
            : (g_asiBaseURL + '/IID/InquiryReportReadOnly?complaintId=' + encodeURIComponent(rowComplaintId));
        return '<a class="btn btn-danger btn-sm" href="' + href + '">View</a>';
    }

    function renderTable(rows){
        reportRows = rows || [];
        updateSummaryCards(reportRows);

        if(typeof destroyDatatable === 'function'){
            destroyDatatable(tableId);
        }

        var columns = buildPreferredColumns(reportRows);
        var headerHtml = '<tr>';
        if(!columns.length){
            $('#iidReportsHeader').html('<tr><th>Result</th></tr>');
            $('#iidReportsBody').html('<tr><td class="text-center text-muted">No report rows matched the current filters.</td></tr>');
            return;
        }

        columns.forEach(function(column){
            headerHtml += '<th>' + htmlEncode(toHeaderLabel(column)) + '</th>';
        });
        headerHtml += '<th class="text-center">View</th></tr>';
        $('#iidReportsHeader').html(headerHtml);

        if(!reportRows.length){
            $('#iidReportsBody').html('<tr><td colspan="' + (columns.length + 1) + '" class="text-center text-muted">No report rows matched the current filters.</td></tr>');
            return;
        }

        var bodyHtml = '';
        reportRows.forEach(function(row){
            bodyHtml += '<tr>';
            columns.forEach(function(column){
                var value = row[column];
                bodyHtml += '<td>' + htmlEncode(value == null ? '' : value) + '</td>';
            });
            bodyHtml += '<td class="text-center">' + buildViewLink(row) + '</td>';
            bodyHtml += '</tr>';
        });
        $('#iidReportsBody').html(bodyHtml);

        if(typeof initializeDataTable === 'function'){
            initializeDataTable(tableId);
        }
    }

    function loadReports(){
        var payload = buildFilterPayload();
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/GetReports',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function(resp){
                if(resp && resp.ok === false){
                    showIidAlert(resp.message, 'danger');
                    renderTable([]);
                    return;
                }
                renderTable(Array.isArray(resp) ? resp : []);
            },
            error: function(){
                showIidAlert('Failed to load reports.', 'danger');
                renderTable([]);
            }
        });
    }

    function resetFilters(){
        $('#filterForm')[0].reset();
        $('#HOUnitId').empty().append('<option value="">All</option>');
        $('#BranchId').empty().append('<option value="">All</option>');
        if(complaintId > 0){
            $('#filterForm [name="ComplaintId"]').val(String(complaintId));
        }
        togglePertainsTo();
        renderTable([]);
    }

    loadRegions();
    loadHoUnitTypes();
    togglePertainsTo();

    $('#HOUnitTypeId').on('change', function(){
        loadHoUnits();
    });

    $('#RegionId').on('change', function(){
        loadBranches($(this).val());
    });

    $('#PertainsTo').on('change', function(){
        togglePertainsTo();
    });

    $('#btnResetFilters').on('click', function(){
        resetFilters();
    });

    $('#filterForm').on('submit', function(e){
        e.preventDefault();
        loadReports();
    });

    if(complaintId > 0){
        $('#filterForm [name="ComplaintId"]').val(String(complaintId));
        loadReports();
    }
});

    function getZoneBranches() {
        destroyDatatable('manageObsPanel');
        $('#branchSelectField').empty();
        if ($('#zoneSelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_zone_Branches",
                type: "POST",
                data: {
                    'ZONEID': $('#zoneSelectField option:selected').val()
    },
                    cache: false,
                    success: function (data) {
                        $('#branchSelectField').append('<option value="0" id="0">--Select Branch--</option>');
                        $.each(data, function (i, v) {
                            $('#branchSelectField').append('<option value="' + v.branchid + '" id="' + v.branchid + '">' + v.branchname + '</option>');
                        })
                    },
                    dataType: "json",
                });
            }
        }

        function getEntityObservation() {
            destroyDatatable('manageObsPanel');
            if ($('#branchSelectField option:selected').val() != 0) {
                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/get_old_paras_for_monitoring",
                    type: "POST",
                    data: {
                        'ENTITY_ID': $('#branchSelectField option:selected').val()
                    },
                    cache: false,
                    success: function (data) {
                        $.each(data, function (i, v) {
                            $('#manageObsPanel tbody').append('<tr data-category="' + v.parA_CATEGORY + '" id="assignedObRow_' + v.id + '"><td>' + (++i) + '</td><td>' + v.entitY_NAME + '</td><td>' + v.audiT_PERIOD + '</td><td>' + v.memO_NO + '</td><td>' + v.gisT_OF_PARAS + '</td><td class="text-center"><input type="checkbox" class="shift-check" value="' + v.obS_ID + '"></td></tr>');
                        });
                        initializeDataTable('manageObsPanel');
                    },
                    dataType: "json",
                });
            }
        }

        function getShiftZoneBranches() {
            $('#shiftBranchSelect').empty();
            if ($('#shiftZoneSelect option:selected').val() != 0) {
                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/get_zone_Branches",
                    type: "POST",
                    data: {
                        'ZONEID': $('#shiftZoneSelect option:selected').val()
                    },
                    cache: false,
                    success: function (data) {
                        $('#shiftBranchSelect').append('<option value="0" id="0">--Select Branch--</option>');
                        $.each(data, function (i, v) {
                            $('#shiftBranchSelect').append('<option value="' + v.branchid + '" id="' + v.branchid + '">' + v.branchname + '</option>');
                        })
                    },
                    dataType: "json",
                });
            }
        }

        function shiftParas() {
            var selected = $('.shift-check:checked');
            if (selected.length === 0) {
                alert('Please select at least one para to shift');
                return;
            }
            var ids = [];
            var categories = [];
            selected.each(function () {
                ids.push(parseInt($(this).val()));
                categories.push($(this).closest('tr').data('category'));
            });
            if ($('#shiftBranchSelect option:selected').val() == 0) {
                alert('Please select entity to shift para');
                return;
            }
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/Para_Shifted_To",
                type: "POST",
                traditional: true,
                data: {
                    'OBS_IDS': ids,
                    'NEW_ENT_ID': $('#shiftBranchSelect option:selected').val(),
                    'OLD_ENT_ID': $('#branchSelectField option:selected').val(),
                    'P_INDS': categories
                },
                cache: false,
                success: function (data) {
                    showApiAlert(data);
                    getEntityObservation();
                },
                dataType: "json",
            });
        }

        $(document).on('change', '#selectAllShift', function () {
            $('.shift-check').prop('checked', $(this).prop('checked'));
        });

        $(document).on('change', '.shift-check', function () {
            $('#selectAllShift').prop('checked', $('.shift-check').length === $('.shift-check:checked').length);
        });

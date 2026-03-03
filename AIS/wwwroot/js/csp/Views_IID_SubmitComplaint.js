    $(function(){
        function showIidAlert(title, message){
            $('#iidAlertTitle').text(title || 'Message');
            $('#iidAlertBody').text(message || '');
            $('#iidAlertModal').modal('show');
        }

        function getTodayDate(){
            var now = new Date();
            var localDate = new Date(now.getTime() - (now.getTimezoneOffset() * 60000));
            return localDate.toISOString().split('T')[0];
        }

        function toggleSourceManual(){
            var selectedText = $('#Source option:selected').text() || '';
            var $manualDiv = $('#sourceOtherDiv');
            var $manualInput = $('#SourceOtherText');
            var $manualLabel = $('#sourceOtherLabel');
            var requiresManual = selectedText.indexOf('(Specify Manually)') !== -1;

            if(requiresManual){
                $manualDiv.removeClass('d-none');
                $manualInput.prop('required', true);

                if(selectedText.indexOf('HO Department') !== -1){
                    $manualLabel.text('HO Department Name');
                    $manualInput.attr('placeholder', 'Enter HO Department Name');
                } else {
                    $manualLabel.text('Other Source Name');
                    $manualInput.attr('placeholder', 'Enter Source Name');
                }
            } else {
                $manualDiv.addClass('d-none');
                $manualInput.val('').prop('required', false).attr('placeholder', '');
                $manualLabel.text('Specify Source');
            }
        }

        function onSourceChange(){
            var sourceValue = ($('#Source').val() || '').trim();
            var requiresFfr = sourceValue === 'SQ&CMD';
            $('#FfrFile').prop('required', requiresFfr);
            if(!requiresFfr){
                $('#ffrValidationMessage').addClass('d-none');
            }
        }

        function togglePertainsTo(){
            var pertainsTo = $('#PertainsTo').val();
            $('#fieldTypeDiv,#hoUnitTypeDiv,#hoUnitDiv,#regionDiv,#branchDiv').addClass('d-none');
            $('#FieldType,#HOUnitTypeId,#HOUnitId,#RegionId,#BranchId');

            if(pertainsTo === 'FIELD'){
                $('#fieldTypeDiv').removeClass('d-none');
                $('#FieldType');
                toggleFieldType();
            }
        }

        function toggleFieldType(){
            var fieldType = $('#FieldType').val();
            $('#hoUnitTypeDiv,#hoUnitDiv,#regionDiv,#branchDiv').addClass('d-none');
            $('#HOUnitTypeId,#HOUnitId,#RegionId,#BranchId');

            if(fieldType === 'HO_UNIT'){
                $('#hoUnitTypeDiv,#hoUnitDiv').removeClass('d-none');
                $('#HOUnitTypeId,#HOUnitId');
                loadHoUnitTypes();
            } else if(fieldType === 'BRANCH'){
                $('#regionDiv,#branchDiv').removeClass('d-none');
                $('#RegionId,#BranchId');
            }
        }

        function loadHoUnitTypes(){
            if($('#HOUnitTypeId option').length > 1){
                return;
            }
            $.post(g_asiBaseURL + '/ApiCalls/get_ho_unit_types', {}, function(d){
                $('#HOUnitTypeId').empty().append('<option value="">--Select HO Unit Type--</option>');
                $.each(d, function(i,v){
                    var id = v.divisionid || v.DIVISIONID;
                    var name = v.name || v.NAME;
                    $('#HOUnitTypeId').append('<option value="'+id+'">'+name+'</option>');
                });
            });
        }

        function loadHoUnits(){
            var typeId = $('#HOUnitTypeId').val();
            $('#HOUnitId').empty().append('<option value="">--Select HO Unit--</option>');
            if(!typeId){
                return;
            }
            $.post(g_asiBaseURL + '/ApiCalls/get_ho_units', { divisionId: typeId }, function(d){
                $.each(d, function(i,v){
                    var id = v.id || v.ID;
                    var name = v.name || v.NAME;
                    $('#HOUnitId').append('<option value="'+id+'">'+name+'</option>');
                });
            });
        }

        $('#Source').on('change', function(){
            toggleSourceManual();
            onSourceChange();
        });
        $('#PertainsTo').on('change', togglePertainsTo);
        $('#FieldType').on('change', toggleFieldType);
        $('#HOUnitTypeId').on('change', loadHoUnits);

        $('#RegionId').on('change', function(){
            if($(this).val()){
                $('#BranchId').empty().append('<option value="">--Select Branch--</option>');
                $.post(g_asiBaseURL + '/ApiCalls/get_zone_Branches', { ZONEID: $(this).val() }, function(d){
                    $.each(d, function(i,v){
                        $('#BranchId').append('<option value="'+v.branchid+'">'+v.branchname+'</option>');
                    });
                });
            }
        });


        toggleSourceManual();
        onSourceChange();
        togglePertainsTo();

        var today = getTodayDate();
        $('#ReceivedOn').attr('max', today);

        $('#complaintForm').on('submit', function(e){
            e.preventDefault();

            // Hard check: all required fields must be filled before API call
            var isValid = true;
            var firstInvalidName = '';

            // Check all required inputs/selects/textareas (including file inputs)
            $('#complaintForm').find('input, select, textarea').each(function(){
                var $el = $(this);

                if ($el.prop('disabled')) return;

                var isRequired = $el.prop('required');
                if (!isRequired) return;

                var tag = ($el.prop('tagName') || '').toLowerCase();
                var type = ($el.attr('type') || '').toLowerCase();

                if (type === 'file') {
                    if (!this.files || this.files.length === 0) {
                        isValid = false;
                        firstInvalidName = $el.attr('name') || 'file';
                        return false;
                    }
                } else if (tag === 'select') {
                    if (!$el.val()) {
                        isValid = false;
                        firstInvalidName = $el.attr('name') || 'select';
                        return false;
                    }
                } else {
                    if (!$el.val() || !$el.val().toString().trim()) {
                        isValid = false;
                        firstInvalidName = $el.attr('name') || 'field';
                        return false;
                    }
                }
            });

            if (!isValid) {
                showIidAlert('Failed', 'Please fill all mandatory fields before submitting.');
                // Optionally focus first invalid field
                var $first = $('#complaintForm').find('[name="'+firstInvalidName+'"]');
                if ($first.length) $first.focus();
                return;
            }

            var fd = new FormData(this);
            var receivedOnDate = $('#ReceivedOn').val();
            var selectedSource = ($('#Source').val() || '').trim();
            var hasFfr = $('#FfrFile')[0] && $('#FfrFile')[0].files && $('#FfrFile')[0].files.length > 0;

            if(selectedSource === 'SQ&CMD' && !hasFfr){
                $('#ffrValidationMessage').removeClass('d-none');
                showIidAlert('Failed', 'FFR upload is mandatory when Source is SQ&CMD.');
                $('#FfrFile').focus();
                return;
            }

            if(selectedSource !== 'SQ&CMD'){
                $('#ffrValidationMessage').addClass('d-none');
            }

            if(receivedOnDate && receivedOnDate > today){
                showIidAlert('Failed', 'Received On date cannot be a future date.');
                $('#ReceivedOn').focus();
                return;
            }

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/SubmitComplaint',
                type: 'POST',
                data: fd,
                processData:false,
                contentType:false,
                success: function(d){
                    if(d && d.ok === false){
                        showIidAlert('Failed', d.message || 'Error submitting complaint.');
                        return;
                    }
                    showIidAlert('Success', 'Complaint saved successfully.');
                    $('#iidAlertModal').off('hidden.bs.modal').on('hidden.bs.modal', function () { location.reload(); });
                },
                error: function(xhr){
                    showIidAlert('Failed', (xhr && xhr.responseText) || 'Error submitting complaint.');
                }
            });
        });
    });

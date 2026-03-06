    var g_paraId = 0;
    $(document).ready(function () {

        $('#observation').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#PublishParaText').on('click', function () {
            console.log('asdasdas');
            publishResponseChanges();
        });
    });
    function getLegacyPara() {
        if ($('#legacypara_branch option:selected').val() == 0) {
            alert('Please Select Your Branch');
            return;
        }
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_para",
            type: "POST",
            data: {
                'AUDITED_BY': $('#legacypara_branch option:selected').val(),
                'AUDIT_YEAR': $('#legacypara_year option:selected').val(),
            },
            cache: false,
            success: function (data) {
                $.each(data, function (index, child) {
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p class="fw-normal mb-1">' + child.amounT_INVOLVED + '</p></td><td><p  class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td class="text-center"><a class="text-center text-danger" style="cursor:pointer;" data-onclick="event.preventDefault();processdetails(' + child.id + ');">Add Para Text</a></td></tr>')
                });

            },

            dataType: "json",
        });

    }
    function getProcessChilds() {

        var select = document.getElementById('processbox');
        var option = select.options[select.selectedIndex];
        var p_id = option.value;
        $('#sub_process').empty();
        $('#sub_process').append('<option value="0" id="0">--Select Sub-Process--</option>');
        if (p_id != 0) {
            $.ajax({
                url: g_asiBaseURL + "/Setup/process_details",
                type: "POST",
                data: {
                    'ProcessId': p_id,
                },

                cache: false,
                success: function (data) {
                    
                    $.each(data, function (index, pid) {
                        $('#sub_process').append('<option value="' + pid.id + '" id="' + pid.id + '" >' + pid.title + '</option>');
                    });
                },

                dataType: "json",
            });
        }

    }
    function getchecklist() {
        var select = document.getElementById('sub_process');
        var option = select.options[select.selectedIndex];
        var s_p_id = option.value;
        $('#checklist_box').empty();
        $('#checklist_box').append('<option value="0" id="0">--Select Checklist Detail--</option>');
        if (s_p_id != 0) {
            $.ajax({
                url: g_asiBaseURL + "/Setup/process_transactions",
                type: "POST",
                data: {
                    'ProcessDetailId': s_p_id,
                },
                cache: false,
                success: function (data) {                    
                    $.each(data, function (index, clid) {
                        $('#checklist_box').append('<option value="' + clid.id + '" id="' + clid.id + '" >' + clid.description + '</option>');
                    });
                },

                dataType: "json",
            });
        }

    }
    function processdetails(id) {
        g_paraId = id;
        $('#process_detail').modal('show');
        $('#observation').val('').trigger('change');
    }

    function publishResponseChanges() {
        if ($('#processbox option:selected').val() == 0) {
            alert("Please select Process to proceed");
            return;
        }

        if ($('#sub_process option:selected').val() == 0) {
            alert("Please select Sub-Process to proceed");
            return;
        }
        if ($('#checklist_box option:selected').val() == 0) {
            alert("Please select Checklist Detail to proceed");
            return;
        }

    
        if ($('.richText-editor').html() == "") {
            alert("Please enter Para Text");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_legacy_para_observation_text",
            type: "POST",
            data: {
                'ID': g_paraId,
                'PROCESS': $('#processbox option:selected').val(),
                'SUB_PROCESS': $('#sub_process option:selected').val(),
                'PROCESS_DETAIL': $('#checklist_box option:selected').val(),
                'PARA_TEXT': $('.richText-editor').html(),
                'RESPONSIBLE_PP_NO': $('#multiplePPNumberFields').val()
            },
            cache: false,
            success: function (data) {
                alert("Para text Successfully added");
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        window.location.reload();
    }

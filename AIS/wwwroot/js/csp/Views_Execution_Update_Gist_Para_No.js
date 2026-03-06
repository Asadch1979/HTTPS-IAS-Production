    var g_paraId = 0;
    var g_obsList = [];
    var g_procId = 0;
    var g_subprocId = 0;
    var g_procDetailId = 0;
    var g_entType="";
  
    var g_paraRef="";

    $(document).ready(function () {

        $('#entitySelectField').select2();
        $('#responseAuditee').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });      

    });
    
   function getLegacyPara()
    {
        $('#process_detail').modal('hide');
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_paras_for_gist_update",
            type: "POST",
            data: {
                'ENTITY_ID': $('#entityReportSelectField').val()
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $('#entityNameField').html(data.length > 0 ? data[0].name : '');
                $.each(data, function (index, child) {

                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.audiT_PERIOD.split(' ')[0] + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p  class="fw-normal mb-1">' + child.amounT_INVOLVED + '</p></td><td><p  class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td class="text-center"><a href="#" data-onclick="event.preventDefault();updateGistParaNo(\'' + child.reF_P + '\', \'' + child.parA_NO + '\', \'' + child.id + '\' );" class="text-hover text-danger mr-5px"><small>Update Gist & Para No</small></a></td></tr>')
                });
            },

            dataType: "json",
        });

    }
    function updateGistParaNo(ref_p, memo_no, paraId) {

        g_paraId=paraId;
        g_paraRef=ref_p;
        $('#process_detail').modal('show');
        $('#listofRespPersons tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_legacy_paras_for_gist_update",
            type: "POST",
            data: {
                'ENTITY_ID': $('#entitySelectField').val(),
                'PARA_REF': ref_p,
                'PARA_ID': paraId
            },
            cache: false,
            success: function (data) {
              
                if (data.length > 0) {
                    var v = data[0];                    
                    $('#paraNofield').val(v.parA_NO);
                    $('#paraGistField').val(v.gisT_OF_PARAS);
                    
                }
            },

            dataType: "json",
        });


    }
  
    function reloadLocation() {
        getLegacyPara();
    }

    function submitLegacyParaUpdates(){

        if($('#paraNofield').val()==""){
            alert("Para No cannot be empty");
            return;
        }
        if ($('#paraGistField').val() == "") {
            alert("Para Gist cannot be empty");
            return;
        }        
        confirmAlert("Do you confirm to update Gist and Para No of this Legacy Para");
        onconfirmAlertCallback(confirm_submitLegacyParaUpdates);
    }
    function confirm_submitLegacyParaUpdates(){
        
        var add_leg_data = {
            'GIST_OF_PARA': $('#paraGistField').val(),
            'PARA_NO': $('#paraNoField').val(),
            'PARA_REF': g_paraRef
        };
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/update_legacy_para_gist_paraNo",
            type: "POST",
            data: add_leg_data,
            cache: false,
            success: function (data) {
                $('#PublishParaText').attr('disabled', false);
                showApiAlert(data);
                getLegacyPara();
            },

            dataType: "json",
        });
    }

    function getReportDropdownContents() {
        if ($('#entitySelectField').val() == 0) {
            $('#dropdownReportPanel').addClass('d-none');
        } else {
            $('#dropdownReportPanel').removeClass('d-none');
            $('#entityReportSelectField').empty();
            $('#entityReportSelectField').append('<option value="0">--Select Entity for Report</option>');

            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_legacy_report_dropdown_contents",
                type: "POST",
                data: {
                    'ENTITY_ID': $('#entitySelectField').val()
                },
                cache: false,
                success: function (data) {
                    g_obsList = data;
                    $('#entityNameField').html(data.length > 0 ? data[0].name : '');
                    $.each(data, function (index, v) {
                        $('#entityReportSelectField').append('<option value="' + v.id + '"> ' + v.entitY_NAME + '</option>');
                    });
                },

                dataType: "json",
            });
        }

    }

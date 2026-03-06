    var g_paraId = 0;
    var g_obsList = [];

    $(document).ready(function () {
        getLegacyPara();
        $('#viewMemo_compliance').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        g_imgLoader = $('[data-type=imagesloader]').imagesloader({
            maxFiles: 10,
            minSelect: 1,
            imagesToLoad: []
        });
    });

    function viewParaDetails(ref_p, memo_no, paraId) {
        g_paraId = ref_p;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_old_para_br_compliance_text_update",
            type: "POST",
            data: {
                'REF_P': ref_p
            },
            cache: false,
            success: function (data) {
                $('#viewMemoModel').modal('show');
                $('#viewMemo_memoNumber').val(memo_no);
                $('#viewMemo_process').val(data.checklist);
                $('#viewMemo_subprocess').val(data.subchecklist);
                $('#viewMemo_checklist_detail').val(data.checklistdetail);
                $('#viewMemo_memo').html(data.parA_TEXT);
                $('#viewMemo_compliance').val('').trigger('change');
            },

            dataType: "json",
        });
    }
    function getLegacyPara() {

        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_old_para_br_compliance_text_update",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $('#entityNameField').html(data.length > 0 ? data[0].name : '');
                $.each(data, function (index, child) {
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p  class="fw-normal mb-1">' + child.amount + '</p></td><td><p  class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td><p  class="fw-normal mb-1">' + child.revieweR_REMARKS + '</p></td><td class="text-center"><a href="#" data-onclick="event.preventDefault();viewParaDetails(\'' + child.reF_P + '\', \'' + child.parA_NO + '\', \'' + child.id + '\' );" class="text-hover text-danger mr-5px"><small>View Observation</small></a></td><td class="text-center"><a href="#" data-onclick="updateParaDetails(\'' + child.reF_P + '\', \'' + child.parA_NO + '\', \'' + child.id + '\' );" class="text-hover text-danger mr-5px"><small>Update Observation</small></a></td></tr>')
                });
            },

            dataType: "json",
        });

    }

    function PublishCompliance() {

        if ($('.richText-editor').html() == "") {
            alert("Please enter Reply");
            return;
        }
        var productImagesArr = [];
        g_imgFiles = g_imgLoader.data('format.imagesloader').AttachmentArray;
        $.each(g_imgFiles, function (i, v) {
            var ProductObject = {
                'OBS_ID': g_paraId,
                'OBS_TEXT_ID': 0,
                'IMAGE_NAME': v.FileName,
                'IMAGE_DATA': v.Base64,
                'IMAGE_TYPE': v.MimeType,
                'LENGTH': v.FileSizeInBytes,
                'COVER_IMAGE': i == 0 ? true : false,
                'SEQUENCE': i
            }
            productImagesArr.push(ProductObject);
        });


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_old_para_br_compliance_reply",
            type: "POST",
            data: {
                'Para_ID': g_paraId,
                'REPLY': $('.richText-editor').html(),
                'EVIDENCE_LIST': productImagesArr
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        $('#viewMemoModel').modal('hide');
        getLegacyPara();
    }

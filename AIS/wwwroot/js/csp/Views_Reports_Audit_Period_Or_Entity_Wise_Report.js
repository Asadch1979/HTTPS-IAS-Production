    var g_obsId = 0;
    var g_obsTextId = 0;
    var g_obsList = [];
    var g_imgFiles = null;
    var g_imgLoader = null;

    $(document).ready(function () {

        $('#viewMemo_reply').richText({
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
    function reloadLocation() {
        getEntityObservation();
      
    }
    function showMemo(obs_id, resp_id) {
        var obs_text_id, status, violation, nature, process, subprocess, checklist_detail,memo_number,canReply;

        $.each(g_obsList, function (i, v) {
            if (v.obS_ID == obs_id) {
                obs_text_id = v.obS_TEXT_ID;
                status = v.statuS_ID;
                violation = v.violation;
                nature = v.nature;
                process = v.process;
                subprocess = v.suB_PROCESS;
                checklist_detail = v.checklisT_DETAIL;
                memo_number = v.memO_NUMBER;
                canReply=v.caN_REPLY;
            }
        });

        g_obsId=obs_id;
        g_obsTextId = obs_text_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_text",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'RESP_ID': resp_id
            },
            cache: false,
            success: function (data) {

                $('#viewMemoModel').modal('show');
                $('#viewMemo_memo').html(data[0]);
                $('#viewMemo_memoNumber').val(memo_number);
                if (nature != null && violation != null) {
                    $('#viewMemo_nature').val(nature);
                    $('#viewMemo_violation').val(violation);
                    $('#viewMemo_process').parent().addClass('d-none');
                    $('#viewMemo_subprocess').parent().addClass('d-none');
                    $('#viewMemo_checklist_detail').parent().addClass('d-none');
                } else {
                    $('#viewMemo_nature').parent().addClass('d-none');
                    $('#viewMemo_violation').parent().addClass('d-none');

                    $('#viewMemo_process').val(process);
                    $('#viewMemo_subprocess').val(subprocess);
                    $('#viewMemo_checklist_detail').val(checklist_detail);
                }

                if (canReply==1){
                    if (status == 2) {
                        $('#replyButton_memoReply').removeClass('d-none');
                        $('#viewMemo_responded').parent().addClass('d-none');
                        $('#replyrichTextWrapper').removeClass('d-none');
                        $('#viewMemo_responded').html('');
                        $('#evidenceViewer').addClass('d-none');
                        $('#evidenceUploader').removeClass('d-none');

                    }
                    else {
                        $('#replyButton_memoReply').addClass('d-none');
                        $('#viewMemo_responded').parent().removeClass('d-none');
                        $('#replyrichTextWrapper').addClass('d-none');
                        $('#viewMemo_responded').html(data[1]);
                        $('#evidenceViewer').removeClass('d-none');
                        $('#evidenceUploader').addClass('d-none');
                    }

                }
                else {
                    $('#replyButton_memoReply').addClass('d-none');
                    $('#viewMemo_responded').parent().removeClass('d-none');
                    $('#replyrichTextWrapper').addClass('d-none');
                    $('#viewMemo_responded').html(data[1]);
                    $('#evidenceViewer').removeClass('d-none');
                    $('#evidenceUploader').addClass('d-none');
                }

                var auctionImages = [];
                $('#evidencePortal').empty();
                $.each(data[2], function (i, v) {
                    $('#evidencePortal').append('<div style="display:inline-block, height:190px; width:190px, border: 2px dashed; margin:5px;"><img style="height:184px; width:184px;" src="data: ' + v.imagE_TYPE + '; base64,' + v.imagE_DATA+'" /></div>')

                  
                });
              
            },
            dataType: "json",
        });
    }
    function replyMemo() {

        var productImagesArr = [];
        g_imgFiles = g_imgLoader.data('format.imagesloader').AttachmentArray;
        $.each(g_imgFiles, function (i, v) {
            var ProductObject = {
                'OBS_ID': g_obsId,
                'OBS_TEXT_ID': g_obsTextId,
                'IMAGE_NAME': v.FileName,
                'IMAGE_DATA': v.Base64,
                'IMAGE_TYPE': v.MimeType,
                'LENGTH': v.FileSizeInBytes,
                'COVER_IMAGE': i == 0 ? true : false,
                'SEQUENCE': i
            }
            productImagesArr.push(ProductObject);
        });


        var replyTxt = ($('#viewMemo_reply').val()).length;
        if (replyTxt > 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/reply_observation",
                type: "POST",
                data: {
                    'AU_OBS_ID': g_obsId,
                    'OBS_TEXT_ID': g_obsTextId,
                    'REPLY': $('#viewMemo_reply').val(),
                    'EVIDENCE_LIST': productImagesArr
                },
                cache: false,
                success: function (data) {
                    alert("Reply sent successfuly");
                    onAlertCallback(reloadLocation);

                },
                dataType: "json",
            });
        } else {
            alert("Provide your comments to proceed");
            return false;
        }

    }

    function getEntityObservation() {
        $('#manageObsPanel tbody').empty();
        if ($('#entitySelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_assigned_observation",
                type: "POST",
                data: {
                    'ENG_ID': $('#entitySelectField option:selected').val()
                },
                cache: false,
                success: function (data) {
                    g_obsList = data;
                    var sr = 1;
                    $.each(data, function (i, v) {
                        var sdate = v.memO_DATE.split(" ")[0];
                        var edate = v.memO_REPLY_DATE.split(" ")[0];
                        var opsdate = v.operatioN_STARTDATE.split("T")[0];
                        var opedate = v.operatioN_ENDDATE .split("T")[0];
                        if(v.caN_REPLY==1){
                            if(v.statuS_ID==2)
                                $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.obS_ID + '"><td>' + v.memO_NUMBER + '</td><td>' + v.audiT_YEAR + '</td><td>' + v.entitY_NAME + '</td><td class="text-center">' + sdate + '</td><td class="text-center">' + edate + '</td><td class="text-center">' + opsdate + '</td><td class="text-center">' + opedate + '</td><td class="text-center">' + v.status + '</td><td class="text-center"><a data-onclick="event.preventDefault();showMemo(' + v.obS_ID + ',' + v.resP_ID + ');" class="text-hover font-weight-bold text-success">Reply</a></td></tr>');
                        else
                                $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.obS_ID + '"><td>' + v.memO_NUMBER + '</td><td>' + v.audiT_YEAR + '</td><td>' + v.entitY_NAME + '</td><td class="text-center">' + sdate + '</td><td class="text-center">' + edate + '</td><td class="text-center">' + opsdate + '</td><td class="text-center">' + opedate + '</td><td class="text-center">' + v.status + '</td><td class="text-center"><a data-onclick="event.preventDefault();showMemo(' + v.obS_ID + ',' + v.resP_ID + ');" class="text-hover font-weight-bold text-success">View</a></td></tr>');
                        }else{
                            if(v.statuS_ID==2)
                                $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.obS_ID + '"><td>' + v.memO_NUMBER + '</td><td>' + v.audiT_YEAR + '</td><td>' + v.entitY_NAME + '</td><td class="text-center">' + sdate + '</td><td class="text-center">' + edate + '</td><td class="text-center">' + opsdate + '</td><td class="text-center">' + opedate + '</td><td class="text-center">' + v.status + '</td><td class="text-center"><a data-onclick="event.preventDefault();showMemo(' + v.obS_ID + ',' + v.resP_ID + ');" class="text-hover font-weight-bold text-success">View</a></td></tr>');
                        else
                                $('#manageObsPanel tbody').append('<tr id="assignedObRow_' + v.obS_ID + '"><td>' + v.memO_NUMBER + '</td><td>' + v.audiT_YEAR + '</td><td>' + v.entitY_NAME + '</td><td class="text-center">' + sdate + '</td><td class="text-center">' + edate + '</td><td class="text-center">' + opsdate + '</td><td class="text-center">' + opedate + '</td><td class="text-center">' + v.status + '</td><td class="text-center"><a data-onclick="event.preventDefault();showMemo(' + v.obS_ID + ',' + v.resP_ID + ');" class="text-hover font-weight-bold text-success">View</a></td></tr>');
                        }
                        
                        sr++;
                    });

                    setTimeout(function () {
                        if (g_obsId != 0) {
                            var rowpos = $('#assignedObRow_' + g_obsId).position();
                            $('html').scrollTop(rowpos.top);
                        }
                    }, 200)

                  

                },
                dataType: "json",
            });

        }
    }

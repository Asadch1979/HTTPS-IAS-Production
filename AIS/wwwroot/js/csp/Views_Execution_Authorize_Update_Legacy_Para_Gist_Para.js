    var g_paraId = 0;
    var g_obsList = [];
    $(document).ready(function () {
        getLegacyPara();


        $('#PublishParaText').on('click', function () {
            publishResponseChanges();
        });
    });
    function getLegacyPara() {

        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_update_gist_paraNo_legacy_paras_autorize",
            type: "POST",
            data: {

            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {

                    $('#manageObsPanel tbody').append('<tr id="div_' + child.parA_REF + '"><td><p class="fw-normal mb-1">' + child.e_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_YEAR + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.nature + '</p></td><td><p class="fw-normal mb-1">' + child.olD_GIST_OF_PARA + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARA + '</p></td><td class="text-center"><a class="text-center text-success" style="cursor:pointer;" data-onclick="event.preventDefault();parastatuschange(\'' + child.parA_REF + '\',\'' + child.parA_NO + '\');">Authorize</a></td></tr>')
                });

            },

            dataType: "json",
        });

    }
    function parastatuschange(id,  parano) {
        var gist = "";
        var oldgist = "";
        $.each(g_obsList, function (index, child) {

            if (child.parA_REF == id && child.parA_NO == parano) {
                gist = child.gisT_OF_PARA;
                oldgist = child.olD_GIST_OF_PARA;
            }

        });
        g_paraId = id;
        g_paraGist = gist;
        g_paraNo = parano;
        $('#process_detail').modal('show');

        $('#oldgistofParaTextArea').val(oldgist);
        $('#gistofParaTextArea').val(g_paraGist);
    }


    function Publishchange() {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Authorize_Legacy_Para_Gist_ParaNo",
            type: "POST",
            data: {
                'PARA_REF': g_paraId,
                'PARA_NO': g_paraNo,
                'GIST_OF_PARA':  $('#gistofParaTextArea').val()

            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
                $('#process_detail').modal('hide');
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        getLegacyPara();
    }

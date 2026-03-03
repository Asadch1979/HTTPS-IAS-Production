    var g_teamMembers = [];
    var g_obsId = 0;
    var g_engId = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        var obs_id = url.searchParams.get("obsId");
        var eng_id = url.searchParams.get("engId");
        $('#dsacontent_textarea').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        if (typeof obs_id != 'undefined')
            g_obsId = obs_id;
        if (typeof eng_id != 'undefined')
            g_engId = eng_id;
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_draft_dsa_guidelines",
            type: "POST",
            data: {
            },
            cache: false,
            success: function (data) {
                if (data.length > 0) {
                    $.each(data, function (j, p) {
                        var srNo = $('#dsaGuidelines tbody tr').length;
                        srNo++;
                        $('#dsaGuidelines tbody').append('<tr><td>' + srNo + '</td><td>' + p.particulars + '</td><td><input gid="'+p.id+'" type="checkbox" class="dsaAgreement" /></td></tr>');
                    });
                }
            },
            dataType: "json",
        });
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_observation_responsible_ppnos",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                'ENG_ID': g_engId
            },
            cache: false,
            success: function (data) {
                if (data.length > 0) {
                    $.each(data, function (j, pp) {
                        var srNo = $('#listofRespPersons tbody tr').length;
                        srNo++;
                        $('#listofRespPersons tbody').append('<tr id="tr_' + pp.pP_NO + '"><td><input ppno="' + pp.pP_NO + '" type="checkbox" class="respList" /></td><td>' + srNo + '</td><td>' + pp.pP_NO + '</td><td>' + pp.emP_NAME + '</td><td>' + pp.loaN_CASE + '</td><td>' + pp.lC_AMOUNT + '</td><td>' + pp.accounT_NUMBER + '</td><td>' + pp.acC_AMOUNT + '</td></tr>');
                    });
                }
            },
            dataType: "json",
        });
    });
    function reloadLocation() {
        window.location.reload();
    }
    function createDSA() {
        var allSigned = 0;
        var gidArr = [];
        $.each($('.dsaAgreement'), function (i, v) {
            if ($(v).is(":checked")) {
                gidArr.push($(v).attr("gid"));
                allSigned++;
            }
        });

        if (allSigned == 0) {
            alert("Please check all DSA guidelines to proceed");
            return;
        }

        var respPP = 0;
        var respArr = [];
        $.each($('.respList'), function (i, v) {
            if ($(v).is(":checked")) {
                respArr.push($(v).attr("ppno"));
                respPP++;
            }
        });

        if (respPP == 0) {
            alert("Please check at least one Responsible to proceed");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/draft_dsa",
            type: "POST",
            data: {
                'OBS_ID': g_obsId,
                "RESP_LIST": respArr,
                "GID_LIST": gidArr,
                "DSA_CONTENT":$('#dsacontent_textarea').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);

            },
            dataType: "json",
        });


    }

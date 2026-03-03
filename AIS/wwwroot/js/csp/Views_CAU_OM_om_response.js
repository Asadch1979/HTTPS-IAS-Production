    var g_oms = '';
    $('#document').ready(function () {
        $('#template_box').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/CAU_Get_OMs",
            type: "POST",
            cache: false,
            success: function (data) {
                g_oms = data;
               // $('#assignedOM_box').empty();
                $.each(g_oms, function (i, v) {
                    $('#assignedOM_box').append('<option id="' + v.id + '" value="' + v.oM_NO + '">' + v.oM_NO + '</option>');
                });
            },
            dataType: "json",
        });
    });

    function getOMContents() {
        var selectedOm = $('#assignedOM_box option:selected').attr('id');
        $.each(g_oms, function (i, v) {
            if (v.id == selectedOm) {
                $('#omContentAreaBox').html(v.contentS_OF_OM);
            }            
        });
    }
    function forwardOmToDept() {

    }

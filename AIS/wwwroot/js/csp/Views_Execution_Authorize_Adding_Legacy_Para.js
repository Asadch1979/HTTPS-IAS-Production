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
            url: g_asiBaseURL + "/ApiCalls/get_add_legacy_paras_autorize",
            type: "POST",
            data: {
                
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                $.each(data, function (index, child) {
                   
                    $('#manageObsPanel tbody').append('<tr id="div_' + child.parA_REF + '"><td><p class="fw-normal mb-1">' + child.e_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_YEAR + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARA + '</p></td><td><p class="fw-normal mb-1">' + child.amount + '</p></td><td><p class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td class="text-center"><a class="text-center text-danger" style="cursor:pointer;" onclick="event.preventDefault();PublishchangeDelete(\'' + child.parA_REF + '\');">Delete Request</a></td><td class="text-center"><a class="text-center text-success" style="cursor:pointer;" onclick="Publishchange(\'' + child.parA_REF + '\');">Authorize</a></td></tr>')
                });

            },

            dataType: "json",
        });

    }
    function parastatuschange(id) {
        g_paraId = id;       
        $('#process_detail').modal('show');     
    }


    function Publishchange(refp) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Authorize_Legacy_Para_addition",
            type: "POST",
            data: {
                'PARA_REF': refp
              
            },
            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function PublishchangeDelete(refp){
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Delete_Legacy_Para_addition_request",
            type: "POST",
            data: {
                'PARA_REF': refp

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
        getLegacyPara();
    }

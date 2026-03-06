    var g_paraId = 0;
    var g_obsList = [];
    var g_pageNo=1;
    var g_pageSize =10;
    $(document).ready(function () {
        g_pageSize=$('#totalPageSize').val();
        getLegacyPara();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#manageObsPanel tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
        $('#responseAuditee').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#PublishParaText').on('click', function () {
            publishResponseChanges();
        });
    });
    function getLegacyPara() {
       
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_manage_legacy_para",
            type: "POST",
            data: {
                
            },
            cache: false,
            success: function (data) {
                g_obsList = data;
                addRecordToGrid();

            },

            dataType: "json",
        });

    }

    function addRecordToGrid(){
        $('#manageObsPanel tbody').empty();
        g_pageSize = $('#totalPageSize').val();
        if(g_pageSize=='all')
        {
            g_pageSize=g_obsList.length;
        }
        var paginationRow = [];
        $.each(g_obsList,function(i,v){
            if (i >= (parseInt(g_pageNo - 1) * g_pageSize) && i < (parseInt(g_pageNo) * g_pageSize)) {
                paginationRow.push(v);
            }
        })
       // g_obsList.splice((g_pageNo-1 * 10), g_pageNo * 10); 

        

        $('#pageNoField').html('Page ' + g_pageNo + ' of ' + Math.ceil(g_obsList.length / g_pageSize));
        $('#totalRecordsNoField').html(' Total Records: '+g_obsList.length);
        $.each(paginationRow, function (index, child) {
            $('#manageObsPanel tbody').append('<tr id="div_' + child.id + '"><td><p class="fw-normal mb-1">' + child.entitY_NAME + '</p></td><td><p class="fw-normal mb-1">' + child.audiT_PERIOD + '</p></td><td><p class="fw-normal mb-1">' + child.parA_NO + '</p></td><td><p class="fw-normal mb-1">' + child.gisT_OF_PARAS + '</p></td><td><p class="fw-normal mb-1">' + child.amounT_INVOLVED + '</p></td><td><p  class="fw-normal mb-1">' + child.voL_I_II + '</p></td><td class="text-center"><a href="#" data-click="event.preventDefault();updateObservationStatus(' + child.id + ', 6);" class="text-hover text-danger mr-5px"><small>Settle</small></a>/<a href="#" data-click="updateObservationStatus(' + child.id + ',8);" class="text-hover text-primary ml-5px"><small>Unsettle</small></a></td></tr>')
        });
    }

    function nextPageClick(){
        g_pageNo++;
        if (g_pageNo > Math.ceil(g_obsList.length / g_pageSize))
            g_pageNo = Math.ceil(g_obsList.length / g_pageSize);
        addRecordToGrid();
    }

    function prevPageClick(){
        g_pageNo--;
        if (g_pageNo<1)
            g_pageNo = 1;
        addRecordToGrid();
    }

    function updateObservationStatus(id, new_status) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/set_manage_legacy_para_status",
            type: "POST",
            data: {
                'ID': id,
                'NEW_STATUS': new_status
            },
            cache: false,
            success: function (data) {
                alert("Observation Status Successfully updated");
                $('#manageObsPanel tbody tr#div_'+id).hide();               
                var newObsList=[];
                $.each(g_obsList,function(i,v){
                    if(v.id!=id)
                        newObsList.push(v);
                });
                g_obsList=newObsList;   
                addRecordToGrid();
            },

            dataType: "json",
        });

    }
    function processdetails(id) {
        g_paraId = id;
        $.each(g_obsList, function (i, v) {
            if (v.id == g_paraId) {
                $('#processField').val(v.procesS_DES);
                $('#subprocessField').val(v.suB_PROCESS_DES);
                $('#checklistDetailField').val(v.procesS_DETAIL_DES);
                $('#observation').html(v.gisT_OF_PARAS);
            }
        });
        $('#process_detail').modal('show');
        $('#responseAuditee').val('').trigger('change');
    }
  
    function publishResponseChanges() {
       
        if ($('.richText-editor').html() == "") {
            alert("Please enter Reply");
            return;
        }
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/add_legacy_para_reply",
            type: "POST",
            data: {
                'ID': g_paraId,
                'REPLY': $('.richText-editor').html()               
            },
            cache: false,
            success: function (data) {
                alert("Reply Successfully added");
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
    }
    function reloadLocation() {
        getLegacyPara();
    }

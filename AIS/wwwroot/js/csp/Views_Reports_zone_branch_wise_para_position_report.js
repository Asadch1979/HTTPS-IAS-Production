    function getZoneBranches() {
        $('#manageObsPanel tbody').empty();
        $('#branchSelectField').empty();

        if ($('#zoneSelectField option:selected').val() != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_zone_Branches",
                type: "POST",
                data: {
                    'ZONEID': $('#zoneSelectField option:selected').val()
                },
                cache: false,
                success: function (data) {

                    $('#branchSelectField').append('<option value="0" id="0">--Select Branch--</option>');
                    $.each(data, function (i, v) {
                        $('#branchSelectField').append('<option value="' + v.branchid + '" id="' + v.branchid + '">' + v.branchname + '</option>');
                    })

                },
                dataType: "json",
            });

        }
    }

    function getParaPositio() {

        var zId=0;
        var bId=0;
         if ($('#zoneSelectField option:selected').val()!=0)
        {
            
            zId = $('#zoneSelectField option:selected').attr('id');
        }
        if ($('#branchSelectField option:selected').val()!=0){
            bId=$('#branchSelectField option:selected').val();
        }
       
        if (bId==0 && zId==0) {
            alert("Please select either Zone or Branch to proceed");
            return;
        }

        var EntityId=bId==0?zId:bId;

        $('#manageObsPanel tbody').empty();

        
        $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_zone_brach_para_position",
                type: "POST",
                data: {
                'ENTITY_ID': EntityId
                },
                cache: false,
            success: function (data) 
            {
                $.each(data,function(i,v)
                {
                    $('#manageObsPanel tbody').append('<tr><td class="entity_name">' + v.entity_Name + '</td><td>' + v.total_Paras + '</td><td>' + v.settled_Paras + '</td><td>' + v.unsettled_Paras + '</td></tr>');
                })
                
                },
                dataType: "json",
            });
    }

    function viewParaText(ref_p) {
        $('#viewMemoModel').modal('show');
        $('#viewMemo_memo').html("");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_para_text",
            type: "POST",
            data: {
                'ref_p': ref_p
            },
            cache: false,
            success: function (data) {  
                console.log(data);

                $('#viewMemo_memo').html(data);     
            }
        });
    }

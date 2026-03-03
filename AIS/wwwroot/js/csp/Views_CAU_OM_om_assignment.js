    var g_omID=0;
    var g_paraID=0;
    $(document).ready(function () {
        $('#KeyMatchModel').modal({ backdrop: 'static', keyboard: false })
        $('#KeyMatchModel').modal('show');                
    });

    function bindEvents(){
        $('#template_box').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#template_box_reply').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#template_box_stage2').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#template_box_reply_stage2').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#template_box_stage3Model').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
        $('#template_box_stage4Model').richText({
            imageUpload: false,
            fileUpload: false,
            videoEmbed: false,
            urls: false
        });
    }
   

    function saveOMAssignment() {
        if ($('#division_box').val() == 0) {
            alert('Select Division');
            return;
        }
        if ($('#omNumber_box').val() == "") {
            alert('Please enter OM Number');
            return;
        }      
       
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/CAU_OM_assignment",
            type: "POST",
            data: {
                'ID': 0,
                'DIV_ID': $('#division_box').val(),
                'OM_NO': $('#omNumber_box').val(),
                'INS_YEAR': $('#inspectionYear_box').val(),
                'CONTENTS_OF_OM': $('#template_box').val(),
                'OM_REPLY': $('#template_box_reply').val(),
                'STATUS': 1

            },
            cache: false,
            success: function (data) {

                showApiAlert(data && data.response ? data.response : data);

                g_omID=data.id;
               // $('#saveOMAssignmentStage1').attr('disabled', true);
                $('#omNumber_box_stage2').val($('#omNumber_box').val());
            },
            dataType: "json",
        });

    }
    function saveOMAssignment_Stage2() {
       
        if ($('#paraNumber_box_stage2').val() == "") {
            alert('Please enter Para Number');
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/CAU_OM_assignmentAIR",
            type: "POST",
            data: {
                'ID': 0,
                'PARA_NO': $('#paraNumber_box_stage2').val(),
                'OM_NO': g_omID,
                'DIV_ID': $('#division_box').val(),
                'CONTENTS_OF_OM': $('#template_box_stage2').val(),
                'OM_REPLY': $('#template_box_reply_stage2').val(),
                'Key': $('#keyPasswordOMStage1').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data && data.response ? data.response : data);
                g_paraID=data.id;
                $('#paraNumber_box_stage3').val($('#paraNumber_box_stage2').val());

            },
            dataType: "json",
        });

    }
    function saveOMAssignment_Stage3() {

        var dacP = [];
        $.each($('#listofDACattached tbody tr'), function (i, v) {
            dacP.push({
                'PARA_ID': g_paraID,
                'REPORT_FREQUENCY': $(v).find('td').eq(3).html(),
                'DAC_DATES': $(v).find('td').eq(1).html(),
                'CONTENTS_OF_OM': $(v).find('td').eq(2).html()               
            });
        });

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/CAU_OM_assignmentPDP",
            type: "POST",
            data: {
                'DAC_LIST': dacP
            },
            cache: false,
            success: function (data) {
                showApiAlert(data && data.response ? data.response : data);
                //$('#saveOMAssignmentStage3').attr('disabled', true);
                $('#omNumber_box_stage4').val($('#omNumber_box_stage3').val());

            },
            dataType: "json",
        });

    }
    function saveOMAssignment_Stage4() {

        var dacP = [];
        $.each($('#listofPACattached tbody tr'), function (i, v) {
            dacP.push({
                'PARA_ID': g_paraID,
                'STATUS': $(v).find('td').eq(4).html(),
                'PAC_DATES': $(v).find('td').eq(1).html(),             
                'PRINTING_DATE': $(v).find('td').eq(2).attr('periodId'),
                'CONTENTS_OF_OM': $(v).find('td').eq(3).html()
            });
        });

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/CAU_OM_assignmentARPSE",
            type: "POST",
            data: {
                'PAC_LIST': dacP
            },
            cache: false,
            success: function (data) {
                showApiAlert(data && data.response ? data.response : data);                
            },
            dataType: "json",
        });

    }

    function gotoNextStage(source, destination) {
        $('#' + source).removeClass('show');
        $('#' + destination).addClass('show');
        
    }

    function openDACAssignments() {

        $('#viewModalDACAssignment').modal('show');
        $('#paraNumber_box_stage3Model').val($('#paraNumber_box_stage2').val());
        
    }
     function openPACAssignments() {

        $('#viewModalDACAssignment_stage4').modal('show');
        $('#paraNumber_box_stage4Model').val($('#paraNumber_box_stage2').val());
        
    }
    function deleteRespRow(e) {
        $(e).parent().parent().remove();
    }

    function addDACAssignmentRecordtoGrid(){
        var row = $('#listofDACattached tbody tr').length == 0 ? 1 : $('#listofDACattached tbody tr').length+1;        
        $('#listofDACattached tbody').append('<tr><td>' + row + '</td><td>' + $('#dacDates_box_stage3Model').val() + '</td><td>' + $('#template_box_stage3Model').val() + '</td><td>' + $('#reportFreq_box_stage3Model option:selected').val() + '</td><td class="text-center"><a href="#" onclick="event.preventDefault();deleteRespRow(this);">Delete</a></td></tr>');
    }
    function addPACAssignmentRecordtoGrid() {
        var row = $('#listofPACattached tbody tr').length == 0 ? 1 : $('#listofPACattached tbody tr').length + 1;
        $('#listofPACattached tbody').append('<tr><td>' + row + '</td><td>' + $('#dacDates_box_stage4Model').val() + '</td><td periodId="' + $('#printingDates_box_stage4Model').val() + '">' + $('#printingDates_box_stage4Model option:selected').text() + '</td><td>' + $('#template_box_stage4Model').val() + '</td><td>' + $('#reportFreq_box_stage4Model option:selected').val() + '</td><td class="text-center"><a href="#" onclick="event.preventDefault();deleteRespRow(this);">Delete</a></td></tr>');
    }
    function getPreAddedOM(){
        if ($('#omNumber_box').val() == "" && $('#inspectionYear_box').val() == "")
        {
            alert("Please enter OM No. and Inspection Year to search");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/CAU_get_Pre_Added_OM",
            type: "POST",
            data: {
                'OM_NO': $('#omNumber_box').val(),
                'INS_YEAR': $('#inspectionYear_box').val()
            },
            cache: false,
            success: function (data) {
                showApiAlert(data && data.response ? data.response : data);
            },
            dataType: "json",
        });
    }

    function crossCheckSecurityKey() {
        if ($('#protectedKey_input').val()=="123"){
            $('#KeyMatchModel').modal('hide');
            bindEvents();
            $('#mainPanelOfCAU').removeClass('d-none');

        }else{
            alert("You have entered Wrong Key");
            onAlertCallback(backToHome);
        }
        
    }
    function backToHome() {
        window.location.href = g_asiBaseURL + '/Home/Index';
        return;
    }

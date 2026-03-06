    var g_memoStatusReversalIds = [];
    var g_engID = 0;
    var g_planID = 0;
    var g_obsID = 0;
    var g_auditedByID = 0;
    var g_obsIdsArr = [];

    $(document).ready(function () {
      
    });

    function getrelation(parentEntityId = 0, userEntityId = 0) {


        $('#controlingsearch').empty();
        $('#childposting').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getparentrel",
            type: "POST",
            data: {
                'ENTITY_REALTION_ID': $('#RelationshipField option:selected').val()
            },


            cache: false,
            success: function (data) {


                $('#controlingsearch').append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
                $.each(data, function (index, contof) {

                    var selected = '';
                    if (contof.entitY_ID == parentEntityId)
                        selected = 'selected="selected"';

                    $('#controlingsearch').append('<option ' + selected + ' value="' + contof.entitY_ID + '" id="' + contof.entitY_REALTION_ID + '">' + contof.description + '</option>')
                });
                if (userEntityId != 0)
                    getplacepost(userEntityId)

                // console.log(data);

            },
            dataType: "json",
        });



    }

    function getplacepost(userEntityId = 0) {
        $('#childposting').empty();

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getpostplace",
            type: "POST",
            data: {
                'E_R_ID': $('#controlingsearch option:selected').val()
            },


            cache: false,
            success: function (data) {
                $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
                $.each(data, function (index, gpp) {

                    var selected = '';
                    if (gpp.entitY_ID == userEntityId)
                        selected = 'selected="selected"';
                    $('#childposting').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>')
                });
            },
            dataType: "json",
        });
    }
    function reloadLocation(){
        getEngagementDetails();
    }
    function getEngagementDetails() {
        $('#engsListPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_engagements_details_for_status_reversal",
            type: "POST",
            data: {
                'ENTITY_ID': $('#childposting option:selected').val()
            },

            cache: false,
            success: function (data) {

                $.each(data, function (i, v) {

                    if (v.audiT_START_DATE != null)
                        v.audiT_START_DATE = v.audiT_START_DATE.split(' ')[0];

                    if (v.audiT_END_DATE != null)
                        v.audiT_END_DATE = v.audiT_END_DATE.split(' ')[0];

                    if (v.oP_START_DATE != null)
                        v.oP_START_DATE = v.oP_START_DATE.split(' ')[0];

                    if (v.oP_END_DATE != null)
                        v.oP_END_DATE = v.oP_END_DATE.split(' ')[0];

                  

                    $('#engsListPanel').append('<tr><td>' + ++i + '</td><td>' + v.enG_ID + '</td><td>' + v.teaM_NAME + '</td><td>' + v.audiT_START_DATE + '</td><td>' + v.audiT_END_DATE + '</td><td>' + v.oP_START_DATE + '</td><td>' + v.oP_END_DATE + '</td><td>' + v.status + '</td><td><a class="text-sucess text-center" data-click="event.preventDefault();generateTraditionalRisk(' + v.enG_ID + ');" href="#">Generate Traditional Risk</a></td><td><a href="#" data-click="viewTraditionalRisk(' + v.enG_ID + ');" class="text-sucess text-center">View Traditional Risk</a></td><td><a href="#" data-click="generateAnnexureRisk(' + v.enG_ID + ');" class="text-sucess text-center">Generate Annexure Risk</a></td><td><a href="#" data-click="viewAnnexureRisk(' + v.enG_ID + ');" class="text-sucess text-center">View Annexure Risk</a></td></tr>');
                })

            },
            dataType: "json",
        });

    }
   function generateTraditionalRisk(engId){
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/generate_traditional_risk_rating_of_engagement",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
   }
    function hideTraditionalRiskRating(){
        $('#traditionalRiskRatingContainer').addClass('d-none');
    }
    function hideAnnexureRiskRating() {
        $('#annexureRiskRatingContainer').addClass('d-none');
    }
    function viewTraditionalRisk(engId) {

        $('#traditionalRiskRatingContainer').removeClass('d-none');
        $('#traditionalRiskRatingGrid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/view_traditional_risk_rating_of_engagement",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {                
                $.each(data,function(i,v){
                    $('#traditionalRiskRatingGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.maiN_PROCESS + '</td><td>' + v.risK_MODEL + '</td><td class="text-right">' + v.maX_NUMBER + '</td><td class="text-right">' + v.weightagE_AVERAGE + '</td><td class="text-right">' + v.gravitY_RISK + '</td><td class="text-right">' + v.nO_OF_OBSERVATIONS + '</td><td class="text-right">' + v.risK_BASED_MARKS + '</td><td class="text-right">' + v.weighteD_AVERAGE_MARKS + '</td><td class="text-right">' + v.ciA_MARKS + '</td></tr>');
                })
            },
            dataType: "json",
        });
   }
    function generateAnnexureRisk(engId) {
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/generate_annexure_risk_rating_of_engagement",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {
                showApiAlert(data);
                onAlertCallback(reloadLocation);
            },
            dataType: "json",
        });
   }
    function viewAnnexureRisk(engId) {

        $('#annexureRiskRatingContainer').removeClass('d-none');
        $('#annexureRiskRatingGrid tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/view_annexure_risk_rating_of_engagement",
            type: "POST",
            data: {
                'ENG_ID': engId
            },

            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#annexureRiskRatingGrid tbody').append('<tr><td>' + ++i + '</td><td>' + v.maiN_PROCESS + '</td><td>' + v.risK_MODEL + '</td><td class="text-right">' + v.maX_NUMBER + '</td><td class="text-right">' + v.weightagE_AVERAGE + '</td><td class="text-right">' + v.gravitY_RISK + '</td><td class="text-right">' + v.nO_OF_OBSERVATIONS + '</td><td class="text-right">' + v.risK_BASED_MARKS + '</td><td class="text-right">' + v.weighteD_AVERAGE_MARKS + '</td></tr>');
                })
            },
            dataType: "json",
        });
    }

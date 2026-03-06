    var g_paraId = 0;
    var g_engID = 0;
    $(document).ready(function () {

        var url_string = window.location;
        var url = new URL(url_string);
        g_engID = url.searchParams.get("engId");

        getLoanCaseFiles();

    });

    function openLoanCaseFile() {
        $('#newLoanCaseFileDialog').modal('show');
    }

    function getLoanCaseFiles() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Get_Working_Paper_Loan_Cases",
            type: "POST",
            data: {
                'ENGID': g_engID
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $.each(data,function(i,v){

                    $('#manageObsPanel').append('<tr><td>' + ++i + '</td><td>' + v.lC_NUMBER + '</td><td>' + v.amount + '</td><td>' + v.disB_DATE + '</td><td>' + v.category + '</td><td>' + v.observation + '</td><td>' + v.parA_NO + '</td><td><a href="#" data-click="event.preventDefault();updateLoanCaseFile(' + v.lC_ID + ');"></a></td></tr>');

                })

            },

            dataType: "json",
        });
    }

    function updateLoanCaseFile(lcId){

        $('#newLoanCaseFileDialog').modal('show');

    }

    function AddNewLoanCaseFile() {

        if($('#loancasenumber_txtField').val()==""){
            alert("Provide LC Number to proceed");
            return;
        }

        if ($('#osamount_txtField').val() == "") {
            alert("Provide LC Amount to proceed");
            return;
        }

        if ($('#dateofdisb_txtField').val() == "") {
            alert("Provide Disb Date to proceed");
            return;
        }


        if ($('#lccategory_selectField').val() == "") {
            alert("Provide Loan Category to proceed");
            return;
        }

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Working_Paper_Loan_Cases",
            type: "POST",
            data: {
                'ENGID': g_engID,
                'LCNUMBER': $('#loancasenumber_txtField').val(),
                'LCAMOUNT': $('#osamount_txtField').val(),
                'DISBDATE': $('#dateofdisb_txtField').val(),
                'LCAT': $('#lccategory_selectField').val(),
                'OBS': $('#ob_txtField').val(),
                'PARA_NO': $('#pno_txtField').val()
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
        window.location.reload();
    }

    function redirectToNexPage() {
        window.location.href = g_asiBaseURL + '/WorkingPaper/voucher_checking?engId=' + g_engID;
    }

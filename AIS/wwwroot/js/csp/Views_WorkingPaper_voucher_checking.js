    var g_paraId = 0;
    var g_engID = 0;
    $(document).ready(function () {

        var url_string = window.location;
        var url = new URL(url_string);
        g_engID = url.searchParams.get("engId");


        getVoucherChecking();
    });

    function openVoucherCheckingFile() {
        $('#newVoucherCheckingDialog').modal('show');
    }
    function getVoucherChecking() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Get_Working_Paper_Voucher_Checking",
            type: "POST",
            data: {
                'ENGID': g_engID
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $.each(data, function (i, v) {

                    $('#manageObsPanel').append('<tr><td>' + ++i + '</td><td>' + v.v_NUMBER + '</td><td>' + v.observation + '</td><td>' + v.parA_NO + '</td><td><a href="#" data-onclick="event.preventDefault();updateVoucherChecking(' + v.V_ID + ');"></a></td></tr>');

                })

            },

            dataType: "json",
        });
    }

    function updateVoucherChecking(vId) {

    }

    function AddNewVoucherChecking() {

        if ($('#accountnumber_txtField').val() == "") {
            alert("Provide Voucher Number to proceed");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Working_Paper_Voucher_Checking",
            type: "POST",
            data: {
                'ENGID':g_engID,
                'VNUMBER': $('#accountnumber_txtField').val(),
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
        window.location.href = g_asiBaseURL + '/WorkingPaper/account_opening?engId=' + g_engID;
    }

    function redirectToPreviousPage() {
        window.location.href = g_asiBaseURL + '/WorkingPaper/loan_case_file?engId=' + g_engID;
    }

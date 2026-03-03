    var g_paraId = 0;
    var g_engID = 0;
    $(document).ready(function () {

        var url_string = window.location;
        var url = new URL(url_string);
        g_engID = url.searchParams.get("engId");
        getFixedAssets();


    });

    function openFixedAssetModel() {
        $('#newFixedAssetDialog').modal('show');
    }

    function getFixedAssets() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Get_Working_Paper_Fixed_Assets",
            type: "POST",
            data: {
                'ENGID': g_engID
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $.each(data, function (i, v) {

                    $('#manageObsPanel').append('<tr><td>' + ++i + '</td><td>' + v.asseT_NAME + '</td><td>' + v.physicaL_EXISTANCE + '</td><td>' + v.locatioN_AS_PER_FAR + '</td><td>' + v.difference + '</td><td>' + v.remarks + '</td><td><a href="#" onclick="event.preventDefault();updateVoucherChecking(' + v.V_ID + ');"></a></td></tr>');

                })

            },

            dataType: "json",
        });
    }

    function updateFixedAssets(fId) {

    }

    function AddNewFixedAssets() {

        if ($('#assetName_txtField').val() == "") {
            alert("Provide Assets Name to proceed");
            return;
        }

        if ($('#pyex_txtField').val() == "") {
            alert("Provide Physical Existance to proceed");
            return;
        }

        if ($('#fixedAsset_txtField').val() == "") {
            alert("Provide Location as per Fixed Asset Register to proceed");
            return;
        }
        if ($('#diff_txtField').val() == "") {
            alert("Provide Difference to proceed");
            return;
        }

        if ($('#rem_txtField').val() == "") {
            alert("Provide Remarks to proceed");
            return;
        }


        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Working_Paper_Fixed_Assets",
            type: "POST",
            data: {
                'ENGID': g_engID,
                'A_NAME': $('#assetName_txtField').val(),
                'PHY_EX': $('#pyex_txtField').val(),
                'FAR': $('#fixedAsset_txtField').val(),
                'DIFF': $('#diff_txtField').val(),
                'REM': $('#rem_txtField').val()
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
        window.location.href = g_asiBaseURL + '/WorkingPaper/cash_count?engId=' + g_engID;
    }

    function redirectToPreviousPage() {
        window.location.href = g_asiBaseURL + '/WorkingPaper/account_opening?engId=' + g_engID;
    }

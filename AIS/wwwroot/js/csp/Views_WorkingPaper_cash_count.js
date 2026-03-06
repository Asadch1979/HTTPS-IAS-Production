    var g_paraId = 0;
    var g_engID = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engID = url.searchParams.get("engId");
        getCashCounters();
    });

    function openVoucherCheckingFile() {
        $('#newVoucherCheckingDialog').modal('show');
    }
    function getCashCounters() {

        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Get_Working_Paper_Cash_Counter",
            type: "POST",
            data: {
                'ENGID': g_engID
            },
            cache: false,
            success: function (data) {
                console.log(data);
                $.each(data, function (i, v) {
                    $('#manageObsPanel').append('<tr><td>' + ++i + '</td><td>' + v.denominatioN_VAULT + '</td><td>' + v.nO_CURRENCY_NOTES_VAULT + '</td><td>' + v.totaL_AMOUNT_VAULT + '</td><td>' + v.denominatioN_SAFE_REGISTER + '</td><td>' + v.nO_CURRENCY_NOTES_SAFE_REGISTER + '</td><td>' + v.totaL_AMOUNT_SAFE_REGISTER + '</td><td>' + v.difference + '</td><td><a href="#" data-click="event.preventDefault();updateVoucherChecking(' + v.V_ID + ');"></a></td></tr>');

                })

            },

            dataType: "json",
        });
    }

    function redirectToPreviousPage() {
        window.location.href = g_asiBaseURL + '/WorkingPaper/fixed_assets?engId=' + g_engID;
    }

    function updateFixedAssets(fId) {

    }

    function AddNewCashCounter() {

        if ($('#dVault_txtField').val() == "") {
            alert("Provide Denomination as per Vault to proceed");
            return;
        }

        if ($('#cVault_txtField').val() == "") {
            alert("Provide Currency Notes as Per Vault to proceed");
            return;
        }

        if ($('#tVault_txtField').val() == "") {
            alert("Provide Total Amount as per Vault to proceed");
            return;
        }

        if ($('#dSafe_txtField').val() == "") {
            alert("Provide Denomination as per Safe Register to proceed");
            return;
        }

        if ($('#cSafe_txtField').val() == "") {
            alert("Provide Currency Notes as Per Safe Register to proceed");
            return;
        }

        if ($('#tSafe_txtField').val() == "") {
            alert("Provide Total Amount as per Safe Register to proceed");
            return;
        }

        if ($('#diff_txtField').val() == "") {
            alert("Provide Difference to proceed");
            return;
        }

         
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/Add_Working_Paper_Cash_Counter",
            type: "POST",
            data: {
                'ENGID': g_engID,
                'DVAULT': $('#dVault_txtField').val(),
                'NOVAULT': $('#cVault_txtField').val(),
                'TOTVAULT': $('#tVault_txtField').val(),
                'DSR': $('#dSafe_txtField').val(),
                'NOSR': $('#cSafe_txtField').val(),
                'TOTSR': $('#tSafe_txtField').val(),
                'DIFF': $('#diff_txtField').val()             
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

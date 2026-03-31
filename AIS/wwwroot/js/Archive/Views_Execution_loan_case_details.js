    var g_loanType = '';
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_loanType = url.searchParams.get("type");          
          $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#Loancasetable tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
          });
        getloancase();
    });
    function getloancase() {

            var lid = $('#LCN').val();
            if (lid != 0) {
                $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/Loan_Case_Details",
                    type: "POST",
                    data: {
                        'Loan_case': lid,
                        'LOAN_TYPE': g_loanType
                    },
                    cache: false,
                    success: function (data) {

                        console.log(data);
                        $('#Loancasetable tbody').empty();
                        $.each(data, function (i, v) {
                            $('#Loancasetable tbody').append('<tr><td>' + v.loaN_CASE_NO + '</td><td>' + v.cnic + '</td><td>' + v.customername + '</td><td>' + v.disburseD_AMOUNT + '</td><td>' + v.prin + '</td><td>' + v.markup + '</td><td>' + v.disB_DATE.split('T')[0] + '</td>  <td>' + v.disB_STATUSID + '</td> </tr>')
                        });
                    },
                    dataType: "json",
                });
            }
        }

    
    var g_engID = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engID = url.searchParams.get("engId");
        listSamples();
     
    });
    function listSamples(){


        $.ajax({
             url: g_asiBaseURL + "/ApiCalls/get_list_of_reports",
            type: "POST",
               cache: false,

            data: { 
            'ENG_ID':g_engID
            },
            success: function (data) {
                if (data.length > 0) {
                    populateTable(data);
                } else {
                    destroyDatatable('listOfSamples');
                    $("#listOfSamples tbody").html('<tr><td colspan="6" class="text-center">No data found.</td></tr>');
                }
            },
            dataType: "json",
        });
    }
        function populateTable(data) {
            $('#wait').show();
             destroyDatatable('listOfSamples');
        let tableBody = $("#listOfSamples tbody");
                 

                data.forEach((item, index) => {
        var reportingPeriod =
            item.reportingPeriod ?? item.ReportingPeriod ?? item.REPORTING_PERIOD ?? '';

        var exceptionCount =
            item.exceptionCount ?? item.ExceptionCount ?? item.EXCEPTION_COUNT ?? item.EXC_COUNT ?? 0;

        let row = `<tr>
            <td>${index + 1}</td>
            <td>${item.reporT_TITLE}</td>
            <td class="text-center">${item.discription}</td>
            <td>${reportingPeriod}</td>
            <td class="text-center">${exceptionCount}</td>
            <td class="text-center">
                <button class="btn btn-danger btn-sm" data-click="viewSample(${item.reporT_ID}, ${item.loaN_STATUS}, '${item.reporT_INDICATOR}', '${item.reporT_TITLE.replace(/'/g, "\\'")}', '${item.discription.replace(/'/g, "\\'")}')">
                    View
                </button>
            </td>
        </tr>`;

        tableBody.append(row);
    });      

        // Reinitialize DataTable after populating data
        initializeDataTable("listOfSamples");
        $('#wait').hide();
    }

    function viewSample(reportId, loanStatus, indicator, reportTitle, reportDescription) {
        if(indicator=="A"){
            redirectToAccount(reportId, loanStatus, reportTitle, reportDescription);
        }else if(indicator=="L"){
            redirectToLoan(reportId, loanStatus, reportTitle, reportDescription);
        }
    }



    function redirectToAccount(reportId, loanStatus, title, desc){
        window.location.href = g_asiBaseURL + "/Sampling/Account_exception?engId="+g_engID+"&report_id="+reportId+"&loan_status="+loanStatus+"&title="+encodeURIComponent(title)+"&desc="+encodeURIComponent(desc);
    }
      function redirectToLoan(reportId, loanStatus, title, desc){
        window.location.href = g_asiBaseURL + "/Sampling/loans_exception?engId="+g_engID+"&reporT_ID="+reportId+"&loaN_STATUS="+loanStatus+"&title="+encodeURIComponent(title)+"&desc="+encodeURIComponent(desc);
    }

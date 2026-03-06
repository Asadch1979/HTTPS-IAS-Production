    
    var g_engID = 0;
    $(document).ready(function () {
        var url_string = window.location;
        var url = new URL(url_string);
        g_engID = url.searchParams.get("engId");
        listSamples();
     
    });
    function listSamples(){


        $.ajax({
             url: g_asiBaseURL + "/ApiCalls/get_list_of_samples",
            type: "POST",
               cache: false,

            data: { 
            'ENG_ID':g_engID
            },
            success: function (data) {
                if (data.length > 0) {
                    populateTable(data);
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
        let row = `<tr>
            <td>${index + 1}</td>
            <td>${item.samplE_TYPE}</td>
            <td class="text-center">${item.samplE_PERCENTAGE}%</td>
            <td class="text-center">${item.totaL_COUNT}</td>
            <td class="text-center">${item.samplE_COUNT}</td>
            <td class="text-center">
            <button class="btn btn-danger btn-sm" data-onclick="viewSample(${item.samplE_ID}, ${item.loaN_STATUS}, '${item.samplE_INDICATOR}', '${item.samplE_TYPE}')">                    View
                </button>
            </td>
        </tr>`;

        tableBody.append(row);
    });      

        // Reinitialize DataTable after populating data
        initializeDataTable("listOfSamples");
        $('#wait').hide();
    }

    function viewSample(sampleId, loanStatus, indicator, sampleType) {
        if(indicator=="A"){
            redirectToBiomet(sampleId,loanStatus,sampleType);
        }else if(indicator=="L"){
            redirectToLoan(sampleId,loanStatus,sampleType);
        }
    }


    function redirectToBiomet(sampleId,loanStatus,sampleType){

           window.location.href = g_asiBaseURL + "/Sampling/biomet?engId="+g_engID+"&sample_id="+sampleId+"&loan_status="+loanStatus+"&sample_type="+encodeURIComponent(sampleType);
       }
         function redirectToLoan(sampleId,loanStatus,sampleType){

           window.location.href = g_asiBaseURL + "/Sampling/loans?engId="+g_engID+"&sample_id="+sampleId+"&loan_status="+loanStatus+"&sample_type="+encodeURIComponent(sampleType);
       }

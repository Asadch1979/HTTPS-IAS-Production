    $(document).ready(function () {
        $('#sidebar_policy').hide();
        $('#listofEmployeesContainer').hide();
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfEmployee tbody tr").filter(function () {

                $(this).toggle($(this).find("td.searchable").text().toLowerCase().indexOf(value) > -1)
            });
        });
      
        
    });
    function bindEvents() {
        $('#listOfEmployee tbody .editRole').on('click', function () {
            $(this).hide();
            $(this).parent().find('.selectRole').show();
        });
        $('#listOfEmployee tbody .selectRole').on('change', function () {
            $(this).parent().parent().find('.userTypeField').text($(this).find('option:selected').text());
            $(this).hide();
            $(this).parent().find('.editRole').show();

        });

    }

    function ShowEmployeeContainer() {
        //console.log($('#deptSelectionBox option:selected').val());
        if ($('#deptSelectionBox option:selected').val() == 0)
            $('#listofEmployeesContainer').hide();
        else {
            $('#listofEmployeesContainer').show();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetAuditEmployees",
                type: "POST",
                data: {
                    'dept_code': $('#deptSelectionBox option:selected').val()
                },
                cache: false,
                success: function (data) {
                    $('#listOfEmployee tbody').empty();
                    //console.log(data);
                    $.each(data, function (index, emp) {
                        index++;
                        $('#listOfEmployee tbody').append('<tr><td class= "searchable"><p class="fw-normal mb-1">' + index+ '</p></td ><td class="searchable"><p class="fw-normal mb-1">' + emp.ppno + '</p></td><td class="searchable"><p class="fw-normal mb-1">' + emp.employeefirstname + ' ' + emp.employeelastname + '</p></td><td class="searchable"><p class="fw-normal mb-1 userTypeField">' + emp.fuN_DESIGNATION+'</p></td><td><small class="text-danger editRole">Edit</small><select class="selectRole"><option>Auditor</option><option>Implementation Officer</option><option>Planning & Development</option></select></td></tr >')
                    });
                    bindEvents();
                },
                dataType: "json",
            });
           
        }
    }
    function addNewTeam() {
        $('#setupAuditTeam').modal('show');
    }

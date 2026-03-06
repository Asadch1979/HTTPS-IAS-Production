    var g_entId = 0;
    var g_entList = [];

    function getVal(obj, prop) {
        return obj[prop] ?? obj[prop.toLowerCase()] ?? obj[prop.toUpperCase()] ?? obj[prop.replace(/_/g, '')];
    }
    $(document).ready(function () {
      
        $("#searchTableRecord").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#listOfDepartment tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });

  
    function updateAuditeeEntities(id) {

        g_entId = id;
        $.each(g_entList, function (i, v) {
             if (getVal(v, 'ENTITY_ID') == g_entId) {
                $('#modalEntityId').val(getVal(v, 'ENTITY_ID'));
                $('#modalCode').val(getVal(v, 'CODE'));
                $('#modalName').val(getVal(v, 'NAME'));
                $('#modalActive').val(getVal(v, 'ACTIVE'));
                $('#modalAuditBy').val(getVal(v, 'AUDITBY_ID'));
                $('#modalAuditable').val(getVal(v, 'AUDITABLE'));
                $('#modalAddress').val(getVal(v, 'ADDRESS'));
                $('#modalTelephone').val(getVal(v, 'TELEPHONE'));
                $('#modalEmail').val(getVal(v, 'EMAIL_ADDRESS'));
             }
        });

        $('#updateEntityModal').modal('show');
    }
  
    function ShowEntities() {
        g_entList = [];
        $('#auditeeEntitiesList tbody').empty();

        var typeId = $('#entityTypeSelectField').val();
        if (typeId != 0) {
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/GetAuditeeEntitiesByTypeId",
                type: "POST",
                data: {
                    'ENTITY_TYPE_ID': typeId
                },
                cache: false,
                success: function (data) {
                    g_entList = data;
                    var sr = 0;
                    $.each(data, function (i, v) {
                        sr++;
                        var id = getVal(v, 'ENTITY_ID');
                        $('#auditeeEntitiesList tbody').append('<tr><td>' + sr + '</td><td>' + id + '</td><td>' + getVal(v, 'CODE') + '</td><td>' + getVal(v, 'NAME') + '</td><td>' + getVal(v, 'ACTIVE') + '</td><td>' + getVal(v, 'AUDITBY_NAME') + '</td><td>' + getVal(v, 'AUDITABLE') + '</td><td>' + getVal(v, 'ADDRESS') + '</td><td>' + getVal(v, 'TELEPHONE') + '</td><td>' + getVal(v, 'EMAIL_ADDRESS') + '</td><td><a class="text-danger" data-onclick="event.preventDefault();updateAuditeeEntities(' + id + ')">Update</a></td></tr>');
                    });
                },
                dataType: "json",
            });
        }

    }
    function saveEntity() {
            var model = {
                'ENTITY_ID': $('#modalEntityId').val(),
                'CODE': $('#modalCode').val(),
                'NAME': $('#modalName').val(),
                'ACTIVE': $('#modalActive').val(),
                'AUDITBY_ID': $('#modalAuditBy').val(),
                'AUDITABLE': $('#modalAuditable').val(),
                'ADDRESS': $('#modalAddress').val(),
                'TELEPHONE': $('#modalTelephone').val(),
                'EMAIL_ADDRESS': $('#modalEmail').val(),
                'UP_STATUS': 'U'
            };
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/UpdateAuditeeEntity",
                type: "POST",
                data: { ENTITY_MODEL: model, IND: 'U' },
                cache: false,
                success: function (resp) {
                    showApiAlert(resp);
                    ShowEntities();
                },
                dataType: "json",
            });
        }

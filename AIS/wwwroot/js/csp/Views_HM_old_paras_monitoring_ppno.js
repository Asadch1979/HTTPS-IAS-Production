    function getEntityObservation() {
        destroyDatatable('manageObsPanel');
        $('#manageObsPanel tbody').empty();
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/get_old_paras_for_monitoring_ppno",
            type: "POST",
            data: {
                'ppno': $('#ppnoSearchField').val(),
            },
            cache: false,
            success: function (data) {
                $.each(data, function (i, v) {
                    $('#manageObsPanel tbody').append(
                        '<tr id="assignedObRow_' + (v.oldParaId ?? i) + '"><td>' +
                        (i + 1) + '</td><td>' + v.entityName + '</td><td>' + v.auditPeriod + '</td><td>' + v.annex + '</td><td>' + v.paraNo + '</td><td>' + v.gistOfParas + '</td><td>' + v.amount + '</td><td><a href="#" class="text-danger text-center" style="cursor:pointer;" data-onclick="event.preventDefault();viewParaText(\'' + v.comId + '\',\'' + v.ind + '\')"> View Para</a></td></tr>'
                    );
                });
                initializeDataTable('manageObsPanel');
            },
            dataType: "json",
        });
    }

    function getemployeename() {
        var whereCheck = 0;
        var entity_id = 0;
        if ($('#ppnoSearchField').val() == "0") {
            alert('Please Enter PPNO:');
            return;
        }
        else {
            $('#userListTable tbody').empty();
            $.ajax({
                url: g_asiBaseURL + "/ApiCalls/get_user_name",
                type: "POST",
                data: {
                    'PPNUMBER': $('#ppnoSearchField').val(),

                },
                cache: false,
                success: function (data) {
                    $('#employeename').val(data.Message);
                },
                dataType: "json",
            });

            getEntityObservation();
        }
    }

    function viewParaText(comid, ind) {
        $('#viewMemoModel').modal('show');
        $('#viewMemo_memo').html("");
        $.ajax({
            url: g_asiBaseURL + "/ApiCalls/getallparatext",
            type: "GET",
            data: { comId: comid },
            cache: false,
            dataType: "json",
            success: function (data) {
                var para = null;
                if (Array.isArray(data)) {
                    para = data.find(function (p) {
                        return (p.IND ?? p.ind) === ind;
                    });
                }
                var text = para ? (para.ParaText || para.paraText || "") : "";
                $('#viewMemo_memo').html(text);
            }
        });
    }

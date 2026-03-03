        var g_cnic = 0;
        $('#document').ready(function () {

        });
        function findParaByGistKeyword() {
                    destroyDatatable('searchtextTable');
            $.ajax({
                    url: g_asiBaseURL + "/ApiCalls/get_para_text_in_audit_report",
                type: "POST",
                data: {
                    'SEARCH_KEYWORD': $('#gistkeywordSearch').val(),
                },
                cache: false,
                success: function (data) {
                      var tableBody = $('#searchtextTable tbody');
                     $.each(data, function (i, v) {
                         tableBody.append(`
                                      <tr>
                                          <td class="text-center">${i + 1}</td>
                                          <td class="text-center">${v.audiT_ZONE }</td>
                                          <td class="text-center">${v.parenT_NAME }</td>
                                          <td class="text-center">${v.chilD_NAME }</td>
                                          <td class="text-center">${v.audiT_PERIOD }</td>
                                          <td class="text-center">${v.annexure }</td>
                                          <td class="text-center">${v.parA_NO }</td>
                                          <td class="text-left">${v.gisT_OF_PARAS }</td>
                                          <td class="text-left"><div style="max-width:500px !important; overflow-y:auto;">${v.text }</div></td>
                                      </tr>
                                  `);
                              });
                              initializeDataTable('searchtextTable');
                },
                dataType: "json",
            });
        }

    function loadHolidays() {
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_all_public_holidays',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ year: new Date().getFullYear() }),
            success: function (data) {
                var rows = '';
                data.forEach(function (item) {
                    rows += `<tr>
                        <td>${item.holidaY_DATE.substring(0,10)}</td>
                        <td>${item.iS_HOLIDAY === "Y" ? "Holiday" : item.iS_WEEKEND === "Y" ? "Weekend" : ""}</td>
                        <td>${item.holidaY_NAME || ''}</td>
                    </tr>`;
                });
                $('#holidaysTable tbody').html(rows);
            }
        });
    }

    $(document).ready(function () {
        loadHolidays();

        $('#holidayForm').submit(function (e) {
            e.preventDefault();
            var date = $('#holidayDate').val();
            var type = $('#holidayType').val();
            var name = $('#holidayName').val();

            var model = {
                HOLIDAY_DATE: new Date(date),
                HOLIDAY_YEAR: new Date(date).getFullYear(),
                IS_WEEKEND: type === 'Weekend' ? 'Y' : 'N',
                IS_HOLIDAY: type === 'Holiday' ? 'Y' : 'N',
                HOLIDAY_NAME: name
            };

            $.ajax({
                url: g_asiBaseURL + '/ApiCalls/add_public_holiday',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(model),
                success: function () {
                    loadHolidays();
                    $('#holidayForm')[0].reset();
                }
            });
        });
    });

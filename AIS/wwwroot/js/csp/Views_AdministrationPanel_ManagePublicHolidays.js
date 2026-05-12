    function escapeHtml(value) {
        return $('<div>').text(value || '').html();
    }

    function formatHolidayDate(value) {
        return value ? value.substring(0, 10) : '';
    }

    function getHolidayType(item) {
        return item.iS_HOLIDAY === "Y" ? "Holiday" : item.iS_WEEKEND === "Y" ? "Weekend" : "";
    }

    function resetHolidayForm() {
        $('#holidayForm')[0].reset();
        $('#holidayId').val('');
        $('#holidaySubmitButton').text('Add/Mark');
        $('#holidayCancelEditButton').addClass('d-none');
    }

    function loadHolidays() {
        $.ajax({
            url: g_asiBaseURL + '/ApiCalls/get_all_public_holidays',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ year: new Date().getFullYear() }),
            success: function (data) {
                var rows = '';
                data.forEach(function (item) {
                    var holidayDate = formatHolidayDate(item.holidaY_DATE);
                    var holidayType = getHolidayType(item);
                    var holidayName = item.holidaY_NAME || '';
                    var holidayId = item.id || item.ID || '';

                    rows += `<tr>
                        <td>${escapeHtml(holidayDate)}</td>
                        <td>${escapeHtml(holidayType)}</td>
                        <td>${escapeHtml(holidayName)}</td>
                        <td><a href="#" class="js-edit-holiday" data-id="${escapeHtml(holidayId)}" data-date="${escapeHtml(holidayDate)}" data-type="${escapeHtml(holidayType)}" data-name="${escapeHtml(holidayName)}">Edit</a></td>
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
                ID: $('#holidayId').val() ? parseInt($('#holidayId').val(), 10) : null,
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
                    resetHolidayForm();
                }
            });
        });

        $(document).on('click', '.js-edit-holiday', function (e) {
            e.preventDefault();
            $('#holidayId').val($(this).data('id'));
            $('#holidayDate').val($(this).data('date'));
            $('#holidayType').val($(this).data('type'));
            $('#holidayName').val($(this).data('name'));
            $('#holidaySubmitButton').text('Update Holiday');
            $('#holidayCancelEditButton').removeClass('d-none');
        });

        $('#holidayCancelEditButton').on('click', function () {
            resetHolidayForm();
        });
    });

// Helper functions for Circular search modal
// Provides common AJAX and DOM update logic so views
// only supply selectors and handle selected circulars.

function searchCirculars(text, tableSelector) {
    return $.ajax({
        url: g_asiBaseURL + "/ApiCalls/GetCirculars",
        type: "POST",
        data: { text: text },
        dataType: "json",
        cache: false
    }).done(function (data) {
        populateCircularResults(data, tableSelector);
    });
}

function populateCircularResults(data, tableSelector) {
    var tbody = $(tableSelector + ' tbody');
    tbody.empty();
    $.each(data, function (i, v) {
        var issue = v.issueDate ? v.issueDate.split('T')[0] : '';
        var row = $('<tr>');
        row.append('<td><input type="radio" name="circularSelect"></td>');
        row.append('<td>' + v.referenceNo + '</td>');
        row.append('<td>' + issue + '</td>');
        row.append('<td>' + v.displayText + '</td>');
        var division = v.division || v.Division || v.DIVISION || '';
        row.append('<td>' + division + '</td>');
        row.find('input').data('circular', v);
        tbody.append(row);
    });
}

function getSelectedCircular(tableSelector) {
    var selected = $(tableSelector + ' input[name=circularSelect]:checked');
    return selected.length ? selected.data('circular') : null;
}

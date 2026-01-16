
var metaBase = document.querySelector('meta[name="base-url"]');
var g_asiBaseURL = metaBase ? metaBase.getAttribute('content') : "";
var g_secretKey="";
var activeRequests = 0;
var accessDeniedNoticeVisible = false;

(function (global) {
    var base64Pattern = /^[A-Za-z0-9+/]+={0,2}$/;

    function normaliseBase64(value) {
        return (value || '').replace(/[\r\n\s]+/g, '').replace(/=+$/, '');
    }

    function isLikelyBase64(value) {
        if (typeof value !== 'string') {
            return false;
        }

        var sanitised = value.replace(/[\r\n\s]+/g, '');
        if (sanitised.length === 0 || sanitised.length % 4 !== 0) {
            return false;
        }

        return base64Pattern.test(sanitised);
    }

    function encodeWithCrypto(text) {
        if (!global.CryptoJS || !CryptoJS.enc || !CryptoJS.enc.Utf8 || !CryptoJS.enc.Base64) {
            return null;
        }

        try {
            var words = CryptoJS.enc.Utf8.parse(text);
            return CryptoJS.enc.Base64.stringify(words);
        } catch (error) {
            return null;
        }
    }

    function decodeWithCrypto(value) {
        if (!global.CryptoJS || !CryptoJS.enc || !CryptoJS.enc.Base64 || !CryptoJS.enc.Utf8) {
            return null;
        }

        try {
            var parsed = CryptoJS.enc.Base64.parse(value);
            return CryptoJS.enc.Utf8.stringify(parsed);
        } catch (error) {
            return null;
        }
    }

    function encodeWithWindow(text) {
        if (typeof global.btoa !== 'function') {
            return null;
        }

        try {
            return global.btoa(unescape(encodeURIComponent(text)));
        } catch (error) {
            return null;
        }
    }

    function decodeWithWindow(value) {
        if (typeof global.atob !== 'function') {
            return null;
        }

        try {
            return decodeURIComponent(escape(global.atob(value)));
        } catch (error) {
            return null;
        }
    }

    function encodeForComparison(text) {
        return encodeWithCrypto(text) || encodeWithWindow(text);
    }

    function tryDecodeBase64(value) {
        if (!isLikelyBase64(value)) {
            return null;
        }

        var decoded = decodeWithCrypto(value);
        if (decoded === null) {
            decoded = decodeWithWindow(value);
        }

        if (decoded === null || typeof decoded !== 'string') {
            return null;
        }

        var reEncoded = encodeForComparison(decoded);
        if (!reEncoded) {
            return null;
        }

        if (normaliseBase64(reEncoded) !== normaliseBase64(value)) {
            return null;
        }

        return decoded;
    }

    function ensureString(value) {
        if (value === null || value === undefined) {
            return '';
        }

        return typeof value === 'string' ? value : String(value);
    }

    function encryptText(value) {
        var text = ensureString(value);
        if (!text) {
            return '';
        }

        return encodeWithCrypto(text) || encodeWithWindow(text) || text;
    }

    function decryptText(value) {
        var text = ensureString(value);
        if (!text) {
            return '';
        }

        var decoded = tryDecodeBase64(text);
        return decoded !== null ? decoded : text;
    }

    global.encryptText = encryptText;
    global.decryptText = decryptText;
})(window);

function sendPageId() {
    var path = window.location.pathname;
    if (path.startsWith(g_asiBaseURL)) {
        path = path.substring(g_asiBaseURL.length);
    }
    path = path.replace(/^\//, "");
    $.ajax({
        url: g_asiBaseURL + "/Home/SetPageId",
        type: "POST",
        data: { 'page_path': path },
        cache: false
    });
}

function sanitizeCsvValue(value) {
    if (value === null || value === undefined) {
        return '';
    }

    var normalised = String(value).replace(/[\r\n]+/g, ' ').trim();
    if (normalised.length === 0) {
        return '';
    }

    var firstChar = normalised.charAt(0);
    var dangerousPrefixes = ['=', '+', '@', ':', '\\', '|', "'", '"'];

    if (dangerousPrefixes.indexOf(firstChar) !== -1) {
        return "'" + normalised;
    }

    return normalised;
}

function getSafeExportFormatOptions() {
    return {
        format: {
            body: function (data, row, column, node) {
                var textContent = '';
                if (node && typeof node.textContent === 'string') {
                    textContent = node.textContent;
                } else if (typeof data === 'string') {
                    textContent = data;
                } else if (data !== null && data !== undefined) {
                    textContent = String(data);
                }

                return sanitizeCsvValue(textContent);
            }
        }
    };
}

function getExcelExportButtonConfig(text) {
    return {
        extend: 'excelHtml5',
        text: text || 'Export to Excel',
        exportOptions: getSafeExportFormatOptions()
    };
}

function getCsvExportButtonConfig(text) {
    return {
        extend: 'csvHtml5',
        text: text || 'Export to CSV',
        exportOptions: getSafeExportFormatOptions()
    };
}

function getCurrentPageKey() {
    return "";
}

function getCurrentPageId() {
    if (typeof window.PAGE_ID === 'number') {
        return window.PAGE_ID;
    }

    var rawValue = document.body ? document.body.getAttribute('data-page-id') : "";
    var parsed = parseInt(rawValue || "0", 10);
    if (!isNaN(parsed)) {
        return parsed;
    }

    return 0;
}

function appendPageIdToUrl(url, pageId) {
    return url;
}

function appendPageIdToPayload(payload, pageId) {
    return payload;
}

function appendPageIdToJsonPayload(payload, pageId) {
    return payload;
}

function buildPageIdAwareFetchRequest(url, options) {
    var pageId = getCurrentPageId();
    return { url: url, options: options || {} };
}

function fetchWithPageId(url, options) {
    var request = buildPageIdAwareFetchRequest(url, options);
    return fetch(request.url, request.options)
        .then(function (response) {
            return handleAjaxLikeResponse(response).then(function () {
                return response;
            });
        })
        .catch(function (error) {
            showAjaxErrorAlert(0, null, 'Unable to reach the server. Please check your connection and retry.');
            throw error;
        });
}

$(document).ready(function () {
    // Override default options for all modals
    $.fn.modal.Constructor.Default.backdrop = 'static';
    $.fn.modal.Constructor.Default.keyboard = false;


    $('body').append('<div id="alertMessagesPopup" class="modal" tabindex="-1" role="dialog"><div class="modal-dialog" role="document">  <div class="modal-content">    <div class="modal-header">      <h5 class="modal-title">Alert</h5>      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>    </div>    <div class="modal-body">      <div id="content_alertMessagesPopup"></div>    </div>    <div class="modal-footer"><button type="button" class="btn btn-danger" data-bs-dismiss="modal">Close</button>    </div>  </div></div></div >');
    $('#alertMessagesPopup').on('hidden.bs.modal', function (e) {
        closeFuncCalled();
    });

    $('body').append('<div id="confirmAlertMessagesPopup" class="modal" tabindex="-1" role="dialog"><div class="modal-dialog" role="document">  <div class="modal-content">    <div class="modal-header">      <h5 class="modal-title">Confirmation Box</h5>      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>    </div>    <div class="modal-body">      <div id="content_confirmAlertMessagesPopup"></div>    </div>    <div class="modal-footer"><button type="button" onclick="event.preventDefault();onConfirmationCallback();" class="btn btn-danger" data-bs-dismiss="modal">Yes</button><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">No</button>    </div>  </div></div></div >');
    $('#confirmAlertMessagesPopup').on('hidden.bs.modal', function (e) {
        confirmAlertcloseFuncCalled();
    });

    $('.modal').on("hidden.bs.modal", function (e) { //fire on closing modal box
        if ($('.modal:visible').length) { // check whether parent modal is opend after child modal close
            $('body').addClass('modal-open'); // if open mean length is 1 then add a bootstrap css class to body of the page
        }
    });

    $('.modal').on('show.bs.modal', function (e) {
        if (!($('.modal.in').length)) {
            $('.modal-dialog').css({
                top: 0,
                left: 0
            });
        }
        $('.modal-dialog').draggable({
            handle: ".modal-header, .modal-footer"
        });
        //$('.modal-dialog').draggable();   
        $('.richText-editor').on('mousedown', function (event) {
            event.stopPropagation();
        });
    });
  
    var activeRequests = 0;

    $(document).ajaxStart(function () {
        if (activeRequests <1) {
            $("#wait").css("display", "block");
        }
        activeRequests++;
    });

    $(document).ajaxStop(function () {
        activeRequests--;
        activeRequests=activeRequests < 0? 0: activeRequests;
        if (activeRequests <1) {
            $("#wait").css("display", "none");
        }
    });

    $(document).ajaxError(function (event, jqxhr) {
        activeRequests--;
        activeRequests = activeRequests < 0 ? 0 : activeRequests;
        if (activeRequests < 1) {
            $("#wait").css("display", "none");
        }

        if (!jqxhr) {
            return;
        }

        var status = jqxhr.status;
        var errorRefId = getErrorReferenceIdFromXhr(jqxhr);

        if (status === 401) {
            showAjaxErrorAlert(status, errorRefId, 'Your session has expired. Please sign in again.');
            window.location = g_asiBaseURL + "/Login/Index";
            return;
        }

        if (status === 403) {
            showAjaxErrorAlert(status, errorRefId, "You don't have access. Please contact support if this continues.");
            return;
        }

        if (status === 400 || status === 415 || status === 500 || status === 503 || status === 302) {
            showAjaxErrorAlert(status, errorRefId);
        }

        if (jqxhr.responseJSON) {
            console.error("AJAX error response:", jqxhr.responseJSON);
        } else if (jqxhr.responseText) {
            try {
                var parsed = JSON.parse(jqxhr.responseText);
                console.error("AJAX error response:", parsed);
            } catch (e) {
                console.error("AJAX error response text:", jqxhr.responseText);
            }
        }
    });

    $(document).ajaxComplete(function (event, jqxhr) {
        if (!jqxhr) {
            return;
        }

        var status = jqxhr.status;
        var contentType = jqxhr.getResponseHeader('content-type') || '';
        var responseText = jqxhr.responseText || '';

        if (status === 200 && isHtmlResponse(contentType, responseText)) {
            showAjaxErrorAlert(302, getErrorReferenceIdFromXhr(jqxhr), 'Your session may have expired. Please sign in again.');
        }
    });
});



function alert(message) {
    $('#content_alertMessagesPopup').empty();
    $('#content_alertMessagesPopup').text(message);
    $('#alertMessagesPopup').modal('show');
}

function getErrorReferenceIdFromXhr(jqxhr) {
    if (!jqxhr || typeof jqxhr.getResponseHeader !== 'function') {
        return null;
    }

    return jqxhr.getResponseHeader('X-Error-Reference-Id');
}

function getErrorReferenceIdFromHeaders(headers) {
    if (!headers || typeof headers.get !== 'function') {
        return null;
    }

    return headers.get('X-Error-Reference-Id');
}

function buildAjaxErrorMessage(status, errorRefId, fallbackMessage) {
    var message = fallbackMessage || ('Request failed (' + status + '). Please retry. If it continues, contact support.');

    if (errorRefId) {
        message += ' Reference: ' + errorRefId;
    }

    return message;
}

function showAjaxErrorAlert(status, errorRefId, fallbackMessage) {
    var message = buildAjaxErrorMessage(status, errorRefId, fallbackMessage);
    alert(message);
}

function isHtmlResponse(contentType, responseText) {
    if (contentType && contentType.indexOf('text/html') !== -1) {
        return true;
    }

    if (!responseText) {
        return false;
    }

    var normalized = responseText.toLowerCase();
    return normalized.indexOf('login') !== -1 && normalized.indexOf('password') !== -1;
}

function handleAjaxLikeResponse(response) {
    if (!response) {
        return Promise.resolve();
    }

    var status = response.status;
    var errorRefId = getErrorReferenceIdFromHeaders(response.headers);
    var contentType = response.headers ? response.headers.get('content-type') : '';

    if (status === 401) {
        showAjaxErrorAlert(status, errorRefId, 'Your session has expired. Please sign in again.');
        return Promise.resolve();
    }

    if (status === 403) {
        showAjaxErrorAlert(status, errorRefId, "You don't have access. Please contact support if this continues.");
        return Promise.resolve();
    }

    if (response.redirected) {
        showAjaxErrorAlert(302, errorRefId, 'Your session may have expired. Please sign in again.');
        return Promise.resolve();
    }

    if (status === 400 || status === 415 || status === 500 || status === 503 || status === 302) {
        showAjaxErrorAlert(status, errorRefId);
        return Promise.resolve();
    }

    if (status === 200 && contentType && contentType.indexOf('text/html') !== -1) {
        showAjaxErrorAlert(302, errorRefId, 'Your session may have expired. Please sign in again.');
        return Promise.resolve();
    }

    if (status === 200 && (!contentType || contentType.indexOf('application/json') === -1)) {
        return response.clone().text().then(function (text) {
            if (isHtmlResponse(contentType, text)) {
                showAjaxErrorAlert(302, errorRefId, 'Your session may have expired. Please sign in again.');
            }
        }).catch(function () { });
    }

    return Promise.resolve();
}
function extractPlainText(clobContent) {
    // Implement your logic here to extract plain text from CLOB content
    // This might involve removing HTML tags or any other formatting

    // For example, a basic approach might involve removing HTML tags using a regular expression
    var plainText = clobContent.replace(/<[^>]+>/g, '');

    return plainText;
}
function onAlertCallback(funcToCall) {
    closeFuncCalled = funcToCall;
}
function closeFuncCalled() {

}
function confirmAlert(message) {
    $('#content_confirmAlertMessagesPopup').empty();
    $('#content_confirmAlertMessagesPopup').text(message);
    $('#confirmAlertMessagesPopup').modal('show');
}
function onconfirmAlertCallback(funcToCall) {
    onConfirmationCallback = funcToCall;
}
function onConfirmationCallback() {

}
function confirmAlertcloseFuncCalled() { }
function setCookie(name, value, daysToLive = undefined) {
    // Encode value in order to escape semicolons, commas, and whitespace
    var cookie = name + "=" + encodeURIComponent(value);

    if (typeof daysToLive === "number") {
        /* Sets the max-age attribute so that the cookie expires
        after the specified number of days */
        cookie += "; max-age=" + (daysToLive * 24 * 60 * 60);
    }

    document.cookie = cookie;
}

function showAccessDeniedFallback(message) {
    if (accessDeniedNoticeVisible) {
        return;
    }

    accessDeniedNoticeVisible = true;
    alert(message);

    $('.dataTables_empty').each(function () {
        $(this).text(message);
    });

    $('table').each(function () {
        var $table = $(this);
        var $tbody = $table.find('tbody');
        if ($tbody.length && $tbody.children().length === 0) {
            $tbody.append('<tr><td colspan="100%" class="text-center">' + message + '</td></tr>');
        }
    });

    setTimeout(function () {
        accessDeniedNoticeVisible = false;
    }, 1500);
}
function getCookie(name) {
    // Split cookie string and get all individual name=value pairs in an array
    var cookieArr = document.cookie.split(";");

    // Loop through the array elements
    for (var i = 0; i < cookieArr.length; i++) {
        var cookiePair = cookieArr[i].split("=");

        /* Removing whitespace at the beginning of the cookie name
        and compare it with the given string */
        if (name == cookiePair[0].trim()) {
            // Decode the cookie value and return
            return decodeURIComponent(cookiePair[1]);
        }
    }

    // Return null if not found
    return null;
}
function getBase64(file) {
    var reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = function () {
        return reader.result;
    };
    reader.onerror = function (error) {
        return "";
    };
}

function encryptPassword(password) {
    return btoa(password);
}
function destroyDatatable(id) {
    if ($.fn.DataTable.isDataTable('#' + id)) {
        $('#' + id).DataTable().clear().destroy();
    }
}
function getPdfExportButtonConfig() {
    return {
        extend: 'pdfHtml5',
        orientation: 'landscape',
        pageSize: 'A4',
        action: function (e, dt, button, config) {
            var pdfMake = window.pdfMake;
            var hasPdfMake = pdfMake &&
                typeof pdfMake.createPdf === 'function' &&
                pdfMake.vfs &&
                Object.keys(pdfMake.vfs).length > 0 &&
                !(pdfMake.version && pdfMake.version.indexOf('placeholder') === 0);
            if (!hasPdfMake) {
                console.error('PDF export failed: pdfMake is missing or invalid.');
                alert('PDF export is unavailable right now. Please refresh the page or contact support.');
                return;
            }

            var exportData = dt.buttons.exportData(config.exportOptions);
            var hasContent = exportData &&
                Array.isArray(exportData.body) &&
                exportData.body.some(function (row) {
                    return Array.isArray(row) && row.some(function (cell) {
                        return String(cell || '').trim().length > 0;
                    });
                });

            if (!hasContent) {
                console.error('PDF export failed: no data available for export.');
                alert('There is no data to export to PDF.');
                return;
            }

            $.fn.dataTable.ext.buttons.pdfHtml5.action.call(this, e, dt, button, config);
        }
    };
}
function initializeDataTable(id) {
    if ($.fn.DataTable.isDataTable('#' + id)) {
        $('#' + id).DataTable().clear().destroy();
    }

    // Re-initialize DataTable after the table content is updated
    var dTable=$('#' + id).DataTable({
        dom: '<"top"lfB>rt<"bottom"ip><"clear">',
        autoWidth: true,
        ordering: false,
        "buttons": [
            getPdfExportButtonConfig(),
            getExcelExportButtonConfig('Export to Excel'),
            getCsvExportButtonConfig('Export to CSV'),
            {
                extend: 'copyHtml5',
                text: 'Copy to Clipboard'
            }
        ],
        lengthMenu: [
            [10, 50, 100, -1],
            [10, 50, 100, "All"]
        ]
});
    return dTable;
}
function initializeDataTableWithoutExport(id) {
    if ($.fn.DataTable.isDataTable('#' + id)) {
        $('#' + id).DataTable().clear().destroy();
    }

    // Re-initialize DataTable after the table content is updated
    var dTable = $('#' + id).DataTable({
        dom: '<"top"lfB>rt<"bottom"ip><"clear">',
        autoWidth: true,
        ordering: false,  
        "buttons": [
            getPdfExportButtonConfig(),
            getExcelExportButtonConfig('Export to Excel'),
            getCsvExportButtonConfig('Export to CSV'),
            {
                extend: 'copyHtml5',
                text: 'Copy to Clipboard'
            }
        ],
        lengthMenu: [
            [10, 50, 100, -1],
            [10, 50, 100, "All"]
        ]
    });
    return dTable;
}

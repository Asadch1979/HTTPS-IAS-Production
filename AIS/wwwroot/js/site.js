
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
            return handleAjaxLikeResponse(response, request).then(function () {
                return response;
            });
        })
        .catch(function (error) {
            logClientAjaxIssue({
                reason: 'network_error',
                endpoint: request && request.url ? request.url : url,
                method: request && request.options && request.options.method ? request.options.method : null,
                status: 0
            });
            showAjaxErrorAlert(0, null, 'Unable to reach the server. Please check your connection and retry.');
            throw error;
        });
}

function decodeHtmlEntities(value) {
    if (!value || typeof value !== 'string') {
        return '';
    }

    var textarea = document.createElement('textarea');
    textarea.innerHTML = value;
    return textarea.value;
}

function sanitizeAlertMessageText(value) {
    var raw = value === null || value === undefined ? '' : String(value);
    if (!raw) {
        return '';
    }

    var normalized = raw.replace(/<br\s*\/?>/gi, '\n');
    normalized = decodeHtmlEntities(normalized);
    normalized = normalized.replace(/<[^>]+>/g, ' ');
    normalized = normalized.replace(/[ \t]+\n/g, '\n').replace(/\n[ \t]+/g, '\n');
    normalized = normalized.replace(/[ \t]{2,}/g, ' ');
    normalized = normalized.replace(/\n{3,}/g, '\n\n');

    return normalized.trim();
}

function containsDocumentHtmlMarkers(value) {
    if (!value || typeof value !== 'string') {
        return false;
    }

    var normalized = value.toLowerCase();
    if (normalized.indexOf('<!doctype html') !== -1) {
        return true;
    }

    return normalized.indexOf('<html') !== -1 && normalized.indexOf('<body') !== -1;
}

function isGenericFailureMessage(message) {
    if (!message || typeof message !== 'string') {
        return false;
    }

    var normalized = message.toLowerCase().trim();
    return normalized === 'error' ||
        normalized === 'error occurred' ||
        normalized === 'an error occurred' ||
        normalized === 'failed' ||
        normalized === 'request failed';
}

function getDefaultStatusMessage(status) {
    if (status === 401) {
        return 'Session expired. Please sign in again.';
    }

    if (status === 403) {
        return 'Access denied. Please contact support if this continues.';
    }

    if (status === 404) {
        return 'Requested resource was not found.';
    }

    if (status === 408) {
        return 'Request timed out. Please try again.';
    }

    if (status === 415) {
        return 'Unsupported request format. Please refresh and try again.';
    }

    if (status === 429) {
        return 'Too many requests. Please wait and try again.';
    }

    if (status >= 500) {
        return 'Server error, please try again or contact support.';
    }

    if (status === 0) {
        return 'Unable to reach the server. Please check your connection and retry.';
    }

    return '';
}

function buildClientAjaxContext(context) {
    var source = context || {};
    var payload = {
        reason: source.reason || 'ajax',
        endpoint: source.endpoint || '',
        method: source.method || '',
        status: typeof source.status === 'number' ? source.status : 0
    };

    if (source.errorRefId) {
        payload.errorRefId = source.errorRefId;
    }

    if (source.contentType) {
        payload.contentType = source.contentType;
    }

    if (source.sample) {
        payload.sample = source.sample;
    }

    return payload;
}

function updateServerErrorWatchlist(payload) {
    if (!payload || !payload.endpoint || payload.status < 500) {
        return;
    }

    try {
        if (!window.__iasServerErrorWatchlist) {
            window.__iasServerErrorWatchlist = {};
        }

        var endpoint = payload.endpoint || '';
        var method = (payload.method || 'GET').toUpperCase();
        var key = method + ' ' + endpoint;
        var current = window.__iasServerErrorWatchlist[key] || {
            endpoint: endpoint,
            method: method,
            count: 0,
            lastStatus: payload.status,
            lastSeenUtc: ''
        };

        current.count += 1;
        current.lastStatus = payload.status;
        current.lastSeenUtc = new Date().toISOString();

        if (payload.errorRefId) {
            current.lastErrorRefId = payload.errorRefId;
        }

        window.__iasServerErrorWatchlist[key] = current;
    } catch (watchError) {
        // ignore watchlist persistence issues
    }
}

function logClientAjaxIssue(context) {
    try {
        var payload = buildClientAjaxContext(context);
        updateServerErrorWatchlist(payload);
        console.error('Client AJAX issue:', payload);
    } catch (error) {
        console.error('Client AJAX issue');
    }
}

function expectsJsonFromAjaxSettings(settings) {
    if (!settings) {
        return false;
    }

    var dataType = (settings.dataType || '').toString().toLowerCase();
    return dataType.indexOf('json') !== -1;
}

function expectsJsonFromFetchOptions(options) {
    if (!options || !options.headers) {
        return false;
    }

    var headers = options.headers;
    var acceptValue = '';

    if (typeof headers.get === 'function') {
        acceptValue = headers.get('Accept') || headers.get('accept') || '';
    } else if (typeof headers === 'object') {
        acceptValue = headers.Accept || headers.accept || '';
    }

    return String(acceptValue || '').toLowerCase().indexOf('json') !== -1;
}

function invokeAjaxCallbacks(callbacks, callbackContext, args) {
    if (!callbacks) {
        return;
    }

    if (Array.isArray(callbacks)) {
        callbacks.forEach(function (callback) {
            if (typeof callback === 'function') {
                callback.apply(callbackContext, args);
            }
        });
        return;
    }

    if (typeof callbacks === 'function') {
        callbacks.apply(callbackContext, args);
    }
}

function getAjaxResponseText(jqxhr) {
    if (!jqxhr) {
        return '';
    }

    if (typeof jqxhr.responseText === 'string') {
        return jqxhr.responseText;
    }

    if (typeof jqxhr.responseJSON === 'string') {
        return jqxhr.responseJSON;
    }

    return '';
}

function getNormalizedAjaxPayload(jqxhr, rawData, responseText) {
    if (rawData && typeof rawData === 'object') {
        return rawData;
    }

    if (jqxhr && jqxhr.responseJSON && typeof jqxhr.responseJSON === 'object') {
        return jqxhr.responseJSON;
    }

    var candidateText = responseText || getAjaxResponseText(jqxhr);
    if (!candidateText) {
        return null;
    }

    var parsed = tryParseJson(candidateText);
    if (parsed && typeof parsed === 'object') {
        return parsed;
    }

    return null;
}

function handleCommonAjaxFailure(status, errorRefId, message, redirectToLogin) {
    showAjaxErrorAlert(status, errorRefId, message);
    if (redirectToLogin) {
        window.location = g_asiBaseURL + "/Login/Index";
    }
}

function shouldBlockAjaxSuccessPayload(jqxhr, settings, rawData) {
    if (!jqxhr) {
        return false;
    }

    var status = jqxhr.status || 0;
    var errorRefId = getErrorReferenceIdFromXhr(jqxhr);
    var endpoint = settings && settings.url ? settings.url : '';
    var method = settings && settings.type ? settings.type : '';
    var contentType = jqxhr.getResponseHeader('content-type') || '';
    var responseText = getAjaxResponseText(jqxhr);
    var expectsJson = expectsJsonFromAjaxSettings(settings);

    if (status === 401) {
        logClientAjaxIssue({
            reason: 'jquery_success_401',
            endpoint: endpoint,
            method: method,
            status: status,
            errorRefId: errorRefId
        });
        jqxhr.__iasSafetyHandled = true;
        handleCommonAjaxFailure(status, errorRefId, 'Session expired. Please sign in again.', true);
        return true;
    }

    if (status === 403) {
        logClientAjaxIssue({
            reason: 'jquery_success_403',
            endpoint: endpoint,
            method: method,
            status: status,
            errorRefId: errorRefId
        });
        jqxhr.__iasSafetyHandled = true;
        handleCommonAjaxFailure(status, errorRefId, 'Access denied. Please contact support if this continues.', false);
        return true;
    }

    if (isHtmlResponse(contentType, responseText) && (containsDocumentHtmlMarkers(responseText) || expectsJson || status >= 400)) {
        if (isProbablyLoginHtml(responseText)) {
            logClientAjaxIssue({
                reason: 'jquery_login_html',
                endpoint: endpoint,
                method: method,
                status: 401,
                errorRefId: errorRefId,
                contentType: contentType,
                sample: responseText.trim().slice(0, 200)
            });
            jqxhr.__iasSafetyHandled = true;
            handleCommonAjaxFailure(401, errorRefId, 'Session expired. Please sign in again.', true);
            return true;
        }

        logUnexpectedHtmlSnippet(responseText);
        logClientAjaxIssue({
            reason: 'jquery_unexpected_html_response',
            endpoint: endpoint,
            method: method,
            status: status,
            errorRefId: errorRefId,
            contentType: contentType,
            sample: responseText.trim().slice(0, 200)
        });
        jqxhr.__iasSafetyHandled = true;
        handleCommonAjaxFailure(status, errorRefId, 'Unexpected HTML response. Please try again.', false);
        return true;
    }

    if (expectsJson && responseText && !jqxhr.responseJSON) {
        var parsedText = tryParseJson(responseText);
        if (typeof parsedText === 'string') {
            logClientAjaxIssue({
                reason: 'jquery_unexpected_json_format',
                endpoint: endpoint,
                method: method,
                status: status,
                errorRefId: errorRefId,
                contentType: contentType,
                sample: responseText.trim().slice(0, 200)
            });
            jqxhr.__iasSafetyHandled = true;
            handleCommonAjaxFailure(status, errorRefId, 'Unexpected response format.', false);
            return true;
        }
    }

    var payload = getNormalizedAjaxPayload(jqxhr, rawData, responseText);
    if (payload && typeof payload === 'object' && payload.ok === false) {
        var payloadMessage = extractApiMessage(payload, getDefaultStatusMessage(status) || 'Request failed.');
        logClientAjaxIssue({
            reason: 'jquery_business_failure',
            endpoint: endpoint,
            method: method,
            status: status,
            errorRefId: errorRefId
        });
        jqxhr.__iasSafetyHandled = true;
        handleCommonAjaxFailure(status, errorRefId, payloadMessage, false);
        return true;
    }

    return false;
}

function applyGlobalAjaxDefaults() {
    if (!window.jQuery || !$.ajax || $.ajax.__iasWrapped) {
        return;
    }

    var originalAjax = $.ajax;

    $.ajax = function () {
        var args = Array.prototype.slice.call(arguments);
        if (!args.length) {
            return originalAjax.apply(this, args);
        }

        var settingsIndex = typeof args[0] === 'string' ? 1 : 0;
        var originalSettings = args[settingsIndex] || {};
        var safeSettings = $.extend({}, originalSettings);
        var originalSuccess = safeSettings.success;

        safeSettings.success = function (data, textStatus, jqxhr) {
            if (shouldBlockAjaxSuccessPayload(jqxhr, safeSettings, data)) {
                return;
            }

            invokeAjaxCallbacks(originalSuccess, this, arguments);
        };

        args[settingsIndex] = safeSettings;
        return originalAjax.apply(this, args);
    };

    $.ajax.__iasWrapped = true;
}

function isReportRoutePath(pathname) {
    if (!pathname) {
        return false;
    }

    var normalized = pathname.toLowerCase();
    return normalized.indexOf('/reports/') !== -1 ||
        normalized.indexOf('/fieldauditreport/') !== -1 ||
        normalized.indexOf('/manreport/') !== -1 ||
        normalized.indexOf('/fad/') !== -1;
}

function normalizeReportLayoutShell() {
    if (!window.jQuery || !isReportRoutePath(window.location.pathname || '')) {
        return;
    }

    var $main = $('.ias-main').first();
    if (!$main.length) {
        return;
    }

    $main.addClass('ias-report-page');

    var createdToolbar = false;
    var $toolbar = $main.find('.ias-report-toolbar, .report-toolbar').first();
    if ($toolbar.length) {
        $toolbar.addClass('ias-report-toolbar');
    } else {
        $toolbar = $('<div class="ias-report-toolbar"></div>');
        $main.prepend($toolbar);
        createdToolbar = true;
    }

    var $actionButtons = $main.find('button[style*="float:right"],a.btn[style*="float:right"],input[type="button"][style*="float:right"]')
        .filter(function () {
            return $(this).closest('.table, .modal, .dropdown-menu, .dataTables_wrapper, .ias-report-toolbar').length === 0;
        });

    $actionButtons.each(function () {
        var $btn = $(this);
        $btn.css('float', 'none');
        $btn.addClass('ias-report-action');
        $toolbar.append($btn);
    });

    if (createdToolbar && !$toolbar.children().length) {
        $toolbar.remove();
    }
}

function resolveSelect2DropdownParent($element) {
    if (!$element || !$element.closest) {
        return $(document.body);
    }

    var $modal = $element.closest('.modal.show');
    if (!$modal.length) {
        $modal = $element.closest('.modal');
    }

    if ($modal.length) {
        return $modal;
    }

    var $content = $element.closest('.ias-main, .container-fluid, .container').first();
    if ($content.length) {
        return $content;
    }

    return $(document.body);
}

function applyGlobalSelect2Defaults() {
    if (!window.jQuery || !$.fn || !$.fn.select2 || $.fn.select2.__iasSelect2Wrapped) {
        return;
    }

    var originalSelect2 = $.fn.select2;

    $.fn.select2 = function (options) {
        if (typeof options === 'string') {
            return originalSelect2.apply(this, arguments);
        }

        return this.each(function () {
            var $element = $(this);
            var safeOptions = $.extend(true, {}, options || {});

            if (!safeOptions.dropdownParent) {
                safeOptions.dropdownParent = resolveSelect2DropdownParent($element);
            }

            if (!safeOptions.width) {
                safeOptions.width = '100%';
            }

            originalSelect2.call($element, safeOptions);
        });
    };

    $.fn.select2.__iasSelect2Wrapped = true;
}

function applySafeHtmlInjectionGuard() {
    if (!window.jQuery || !$.fn || !$.fn.html || $.fn.html.__iasSafeHtmlWrapped) {
        return;
    }

    var originalHtml = $.fn.html;

    $.fn.html = function (value) {
        if (arguments.length === 1 && typeof value === 'string' && containsDocumentHtmlMarkers(value)) {
            logUnexpectedHtmlSnippet(value);
            logClientAjaxIssue({
                reason: 'blocked_document_html_injection',
                status: 200,
                sample: value.trim().slice(0, 200)
            });
            showAjaxErrorAlert(200, null, 'Unexpected HTML response. Please try again.');
            return this;
        }

        return originalHtml.apply(this, arguments);
    };

    $.fn.html.__iasSafeHtmlWrapped = true;
}

applyGlobalSelect2Defaults();
applyGlobalAjaxDefaults();
applySafeHtmlInjectionGuard();

$(document).ready(function () {
    // Override default options for all modals
    $.fn.modal.Constructor.Default.backdrop = 'static';
    $.fn.modal.Constructor.Default.keyboard = false;

    $('body').append('<div id="alertMessagesPopup" class="modal" tabindex="-1" role="dialog"><div class="modal-dialog" role="document">  <div class="modal-content">    <div class="modal-header">      <h5 class="modal-title">Alert</h5>      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>    </div>    <div class="modal-body">      <div id="content_alertMessagesPopup"></div>    </div>    <div class="modal-footer"><button type="button" class="btn btn-danger" data-bs-dismiss="modal">Close</button>    </div>  </div></div></div >');
    $('#content_alertMessagesPopup').addClass('text-prewrap');
    $('#alertMessagesPopup').on('hidden.bs.modal', function (e) {
        var callback = closeFuncCalled;
        closeFuncCalled = function () { };
        callback();
    });

    $('body').append('<div id="confirmAlertMessagesPopup" class="modal" tabindex="-1" role="dialog"><div class="modal-dialog" role="document">  <div class="modal-content">    <div class="modal-header">      <h5 class="modal-title">Confirmation Box</h5>      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>    </div>    <div class="modal-body">      <div id="content_confirmAlertMessagesPopup"></div>    </div>    <div class="modal-footer"><button type="button" class="btn btn-danger js-confirm-alert-yes" data-bs-dismiss="modal">Yes</button><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">No</button>    </div>  </div></div></div >');
    $('#content_confirmAlertMessagesPopup').addClass('text-prewrap');
    $('#confirmAlertMessagesPopup').on('hidden.bs.modal', function (e) {
        confirmAlertcloseFuncCalled();
    });
    $(document).on('click', '.js-confirm-alert-yes', function (event) {
        event.preventDefault();
        onConfirmationCallback();
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
    normalizeReportLayoutShell();

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

    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
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
        var endpoint = settings && settings.url ? settings.url : '';
        var method = settings && settings.type ? settings.type : '';

        logClientAjaxIssue({
            reason: 'jquery_ajax_error',
            endpoint: endpoint,
            method: method,
            status: status,
            errorRefId: errorRefId
        });

        if (status === 200 && expectsJsonFromAjaxSettings(settings) && jqxhr.responseText && !jqxhr.responseJSON) {
            logClientAjaxIssue({
                reason: 'jquery_parser_error',
                endpoint: endpoint,
                method: method,
                status: status,
                errorRefId: errorRefId,
                sample: jqxhr.responseText.trim().slice(0, 200)
            });
            showAjaxErrorAlert(status, errorRefId, 'Unexpected response format.');
            return;
        }

        if (status === 401) {
            showApiAlertFromXhr(jqxhr, status, errorRefId, 'Session expired. Please sign in again.');
            window.location = g_asiBaseURL + "/Login/Index";
            return;
        }

        if (status === 403) {
            showApiAlertFromXhr(jqxhr, status, errorRefId, "Access denied. Please contact support if this continues.");
            return;
        }

        if (status >= 400) {
            showApiAlertFromXhr(jqxhr, status, errorRefId, getDefaultStatusMessage(status));
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

    $(document).ajaxComplete(function (event, jqxhr, settings) {
        if (!jqxhr) {
            return;
        }

        if (jqxhr.__iasSafetyHandled) {
            return;
        }

        if (jqxhr.__iasSafetyHandled) {
            return;
        }

        var status = jqxhr.status;
        var contentType = jqxhr.getResponseHeader('content-type') || '';
        var responseText = jqxhr.responseText || '';
        var errorRefId = getErrorReferenceIdFromXhr(jqxhr);
        var endpoint = settings && settings.url ? settings.url : '';
        var method = settings && settings.type ? settings.type : '';
        var expectsJson = expectsJsonFromAjaxSettings(settings);

        if (status === 401) {
            showAjaxErrorAlert(status, errorRefId, 'Session expired. Please sign in again.');
            window.location = g_asiBaseURL + "/Login/Index";
            return;
        }

        if (status === 403) {
            showAjaxErrorAlert(status, errorRefId, 'Access denied. Please contact support if this continues.');
            return;
        }

        if (status === 200 && isHtmlResponse(contentType, responseText)) {
            if (isProbablyLoginHtml(responseText)) {
                showAjaxErrorAlert(401, errorRefId, 'Session expired. Please sign in again.');
                window.location = g_asiBaseURL + "/Login/Index";
                return;
            }
            logUnexpectedHtmlSnippet(responseText);
            logClientAjaxIssue({
                reason: 'unexpected_html_response',
                endpoint: endpoint,
                method: method,
                status: status,
                errorRefId: errorRefId,
                contentType: contentType,
                sample: responseText.trim().slice(0, 200)
            });
            showAjaxErrorAlert(status, errorRefId, 'Unexpected HTML response. Please try again.');
            return;
        }

        if (status === 200 && expectsJson && responseText && !jqxhr.responseJSON) {
            var parsed = tryParseJson(responseText);
            if (typeof parsed === 'string') {
                logClientAjaxIssue({
                    reason: 'unexpected_json_format',
                    endpoint: endpoint,
                    method: method,
                    status: status,
                    errorRefId: errorRefId,
                    contentType: contentType,
                    sample: responseText.trim().slice(0, 200)
                });
                showAjaxErrorAlert(status, errorRefId, 'Unexpected response format.');
                return;
            }

            if (parsed && typeof parsed === 'object' && parsed.ok === false) {
                var parsedMessage = extractApiMessage(parsed, getDefaultStatusMessage(status) || 'Request failed.');
                showAjaxErrorAlert(status, errorRefId, parsedMessage);
            }
        }
    });
});



function alert(message) {
    var safeMessage = sanitizeAlertMessageText(message);
    if (!safeMessage || isGenericFailureMessage(safeMessage)) {
        safeMessage = 'Request failed. Please retry. If it continues, contact support.';
    }

    $('#content_alertMessagesPopup').empty();
    $('#content_alertMessagesPopup').text(safeMessage);
    $('#alertMessagesPopup').modal('show');
}

function tryParseJson(value) {
    if (typeof value !== 'string') {
        return value;
    }

    var trimmed = value.trim();
    if (!trimmed) {
        return value;
    }

    try {
        return JSON.parse(trimmed);
    } catch (e) {
        return value;
    }
}

function unwrapApiPayload(payload) {
    var current = payload;
    for (var i = 0; i < 2; i++) {
        if (typeof current === 'string') {
            var parsed = tryParseJson(current);
            if (parsed !== current) {
                current = parsed;
                continue;
            }
        }
        break;
    }
    return current;
}

function extractMessageFromErrors(errors) {
    if (!errors) {
        return '';
    }

    if (Array.isArray(errors) && errors.length) {
        return errors[0];
    }

    if (typeof errors === 'object') {
        var keys = Object.keys(errors);
        for (var i = 0; i < keys.length; i++) {
            var entry = errors[keys[i]];
            if (Array.isArray(entry) && entry.length) {
                return entry[0];
            }
            if (typeof entry === 'string' && entry.trim()) {
                return entry.trim();
            }
        }
    }

    return '';
}

function extractApiMessage(payload, fallbackMessage) {
    if (!payload) {
        return (fallbackMessage || '').toString().trim();
    }

    if (typeof payload === 'string') {
        if (isHtmlResponse('', payload)) {
            return (fallbackMessage || '').toString().trim();
        }

        var parsedPayload = unwrapApiPayload(payload);
        if (parsedPayload !== payload) {
            return extractApiMessage(parsedPayload, fallbackMessage);
        }
        return payload.trim() || (fallbackMessage || '').toString().trim();
    }

    var normalized = unwrapApiPayload(payload);
    if (typeof normalized === 'string') {
        return normalized.trim() || (fallbackMessage || '').toString().trim();
    }

    if (normalized && typeof normalized === 'object') {
        var messageValue = normalized.Message || normalized.message || normalized.StatusMessage || normalized.statusMessage;
        if (messageValue && typeof messageValue === 'string') {
            return messageValue.trim();
        }

        var errorMessage = normalized.Error || normalized.error;
        if (errorMessage && typeof errorMessage === 'string') {
            return errorMessage.trim();
        }

        var nestedMessage = extractMessageFromErrors(normalized.Errors || normalized.errors);
        if (nestedMessage) {
            return nestedMessage.trim();
        }

        if (normalized.data) {
            return extractApiMessage(normalized.data, fallbackMessage);
        }
    }

    return (fallbackMessage || '').toString().trim();
}

function extractApiMessageFromXhr(jqxhr, fallbackMessage) {
    if (!jqxhr) {
        return (fallbackMessage || '').toString().trim();
    }

    if (jqxhr.responseJSON) {
        var jsonMessage = extractApiMessage(jqxhr.responseJSON, fallbackMessage);
        if (jsonMessage) {
            return jsonMessage;
        }
    }

    if (jqxhr.responseText) {
        var textMessage = extractApiMessage(jqxhr.responseText, fallbackMessage);
        if (textMessage) {
            return textMessage;
        }
    }

    if (jqxhr.statusText) {
        return jqxhr.statusText.toString().trim();
    }

    return (fallbackMessage || '').toString().trim();
}

function showApiAlert(payload, fallbackMessage) {
    var message = extractApiMessage(payload, fallbackMessage);
    if (!message) {
        message = (fallbackMessage || 'Request completed.').toString().trim();
    }
    alert(message);
}

function showApiAlertFromXhr(jqxhr, status, errorRefId, fallbackMessage) {
    var fallback = fallbackMessage || getDefaultStatusMessage(status);
    var message = extractApiMessageFromXhr(jqxhr, fallback);
    var alertMessage = buildAjaxErrorMessage(status, errorRefId, message);
    alert(alertMessage);
}

function showApiAlertFromResponse(response, status, errorRefId, fallbackMessage) {
    if (!response || typeof response.clone !== 'function') {
        var fallbackAlert = buildAjaxErrorMessage(status, errorRefId, fallbackMessage);
        alert(fallbackAlert);
        return Promise.resolve();
    }

    var contentType = response.headers ? (response.headers.get('content-type') || '') : '';
    var fallback = fallbackMessage || getDefaultStatusMessage(status);
    var expectsJson = isJsonContentType(contentType);

    return response.clone().json().then(function (json) {
        var message = extractApiMessage(json, fallback);
        alert(buildAjaxErrorMessage(status, errorRefId, message));
    }).catch(function () {
        if (expectsJson) {
            logClientAjaxIssue({
                reason: 'unexpected_json_format',
                endpoint: response.url || '',
                status: status,
                errorRefId: errorRefId,
                contentType: contentType
            });
            alert(buildAjaxErrorMessage(status, errorRefId, 'Unexpected response format.'));
            return Promise.resolve();
        }

        return response.clone().text().then(function (text) {
            var message = extractApiMessage(text, fallback);
            alert(buildAjaxErrorMessage(status, errorRefId, message));
        }).catch(function () {
            alert(buildAjaxErrorMessage(status, errorRefId, fallback));
        });
    });
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
    var normalizedStatus = typeof status === 'number' ? status : 0;
    var baseMessage = sanitizeAlertMessageText((fallbackMessage || '').toString().trim());
    if (isGenericFailureMessage(baseMessage)) {
        baseMessage = '';
    }

    var defaultStatusMessage = getDefaultStatusMessage(normalizedStatus);
    var message = baseMessage || defaultStatusMessage || ('Request failed (' + normalizedStatus + '). Please retry. If it continues, contact support.');

    if (errorRefId) {
        message += ' Reference: ' + errorRefId;
    }

    return message;
}

function showAjaxErrorAlert(status, errorRefId, fallbackMessage) {
    var message = buildAjaxErrorMessage(status, errorRefId, fallbackMessage);
    alert(message);
}

function isJsonContentType(contentType) {
    if (!contentType) {
        return false;
    }

    var normalized = contentType.toLowerCase();
    return normalized.indexOf('application/json') !== -1 || normalized.indexOf('+json') !== -1;
}

function isHtmlResponse(contentType, responseText) {
    if (contentType && contentType.toLowerCase().indexOf('text/html') !== -1) {
        return true;
    }

    if (!responseText) {
        return false;
    }

    var normalized = responseText.toLowerCase();
    return normalized.indexOf('<html') !== -1 || normalized.indexOf('<!doctype html') !== -1;
}

function isProbablyLoginHtml(responseText) {
    if (!responseText) {
        return false;
    }

    var normalized = responseText.toLowerCase();
    return normalized.indexOf('login') !== -1 && normalized.indexOf('password') !== -1;
}

function logUnexpectedHtmlSnippet(responseText) {
    if (!responseText) {
        return;
    }

    var snippet = responseText.trim().slice(0, 300);
    if (snippet) {
        console.error('Unexpected HTML response snippet:', snippet);
    }
}

function handleAjaxLikeResponse(response, requestContext) {
    if (!response) {
        return Promise.resolve();
    }

    var status = response.status || 0;
    var errorRefId = getErrorReferenceIdFromHeaders(response.headers);
    var contentType = response.headers ? (response.headers.get('content-type') || '') : '';
    var endpoint = requestContext && requestContext.url ? requestContext.url : (response.url || '');
    var method = requestContext && requestContext.options && requestContext.options.method ? requestContext.options.method : '';

    if (status === 401) {
        showAjaxErrorAlert(status, errorRefId, 'Session expired. Please sign in again.');
        window.location = g_asiBaseURL + "/Login/Index";
        return Promise.resolve();
    }

    if (status === 403) {
        showAjaxErrorAlert(status, errorRefId, 'Access denied. Please contact support if this continues.');
        return Promise.resolve();
    }

    return response.clone().text().then(function (text) {
        if (isHtmlResponse(contentType, text) && (containsDocumentHtmlMarkers(text) || isJsonContentType(contentType) || status >= 400)) {
            if (isProbablyLoginHtml(text)) {
                showAjaxErrorAlert(401, errorRefId, 'Session expired. Please sign in again.');
                window.location = g_asiBaseURL + "/Login/Index";
                return;
            }

            logUnexpectedHtmlSnippet(text);
            logClientAjaxIssue({
                reason: 'unexpected_html_response',
                endpoint: endpoint,
                method: method,
                status: status,
                errorRefId: errorRefId,
                contentType: contentType,
                sample: text.trim().slice(0, 200)
            });
            showAjaxErrorAlert(status, errorRefId, 'Unexpected HTML response. Please try again.');
            return;
        }

        if ((isJsonContentType(contentType) || expectsJsonFromFetchOptions(requestContext && requestContext.options)) && text) {
            var parsed = tryParseJson(text);
            if (typeof parsed === 'string') {
                logClientAjaxIssue({
                    reason: 'unexpected_json_format',
                    endpoint: endpoint,
                    method: method,
                    status: status,
                    errorRefId: errorRefId,
                    contentType: contentType,
                    sample: text.trim().slice(0, 200)
                });
                showAjaxErrorAlert(status, errorRefId, 'Unexpected response format.');
                return;
            }

            if (parsed && typeof parsed === 'object' && parsed.ok === false) {
                var payloadMessage = extractApiMessage(parsed, getDefaultStatusMessage(status) || 'Request failed.');
                showAjaxErrorAlert(status, errorRefId, payloadMessage);
                return;
            }
        }

        if (response.redirected) {
            showAjaxErrorAlert(401, errorRefId, 'Session expired. Please sign in again.');
            window.location = g_asiBaseURL + "/Login/Index";
            return;
        }

        if (!response.ok) {
            logClientAjaxIssue({
                reason: 'fetch_http_error',
                endpoint: endpoint,
                method: method,
                status: status,
                errorRefId: errorRefId,
                contentType: contentType,
                sample: (text || '').trim().slice(0, 200)
            });
            return showApiAlertFromResponse(response, status, errorRefId, getDefaultStatusMessage(status));
        }
    }).catch(function () {
        if (!response.ok) {
            return showApiAlertFromResponse(response, status, errorRefId, getDefaultStatusMessage(status));
        }
    });
}
function extractPlainText(clobContent) {
    // Implement your logic here to extract plain text from CLOB content
    // This might involve removing HTML tags or any other formatting

    // For example, a basic approach might involve removing HTML tags using a regular expression
    var plainText = clobContent.replace(/<[^>]+>/g, '');

    return plainText;
}
function onAlertCallback(funcToCall) {
    closeFuncCalled = typeof funcToCall === 'function' ? funcToCall : function () { };
}
function closeFuncCalled() {

}
function confirmAlert(message) {
    var safeMessage = sanitizeAlertMessageText(message);
    if (!safeMessage) {
        safeMessage = 'Please confirm this action.';
    }

    $('#content_confirmAlertMessagesPopup').empty();
    $('#content_confirmAlertMessagesPopup').text(safeMessage);
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

$(document).on('input', 'input.digits-only', function () {
    this.value = (this.value || '').replace(/[^0-9]/g, '');
});

$(document).on('paste', 'input.digits-only', function (e) {
    var text = (e.originalEvent.clipboardData || window.clipboardData).getData('text') || '';
    this.value = text.replace(/[^0-9]/g, '');
    e.preventDefault();
});

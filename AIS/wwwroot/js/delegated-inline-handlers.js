(function ($) {
    'use strict';

    if (!$ || !$.fn) {
        return;
    }

    function runHandler(element, event, attrName) {
        var script = element.getAttribute(attrName);
        if (!script) {
            return;
        }

        var fn = new Function('event', script);
        return fn.call(element, event);
    }

    $(document).on('click', '[data-onclick]', function (event) {
        return runHandler(this, event, 'data-onclick');
    });

    $(document).on('change', '[data-onchange]', function (event) {
        return runHandler(this, event, 'data-onchange');
    });
})(window.jQuery);

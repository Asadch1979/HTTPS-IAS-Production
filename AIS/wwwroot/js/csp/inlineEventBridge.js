(function () {
    function parseArg(token) {
        var value = (token || '').trim();
        if (!value.length) return undefined;
        if ((value.startsWith("'") && value.endsWith("'")) || (value.startsWith('"') && value.endsWith('"'))) {
            return value.slice(1, -1);
        }
        if (/^-?\d+$/.test(value)) return parseInt(value, 10);
        if (value === 'true') return true;
        if (value === 'false') return false;
        if (value === 'this') return '__THIS__';
        if (Object.prototype.hasOwnProperty.call(window, value)) return window[value];
        return value;
    }

    function invokeExpression(expression, context) {
        if (!expression) return;
        var match = expression.trim().match(/^([\w$.]+)\s*\((.*)\)$/);
        var fnName = expression.trim();
        var args = [];
        if (match) {
            fnName = match[1];
            var argsRaw = match[2].trim();
            if (argsRaw.length) {
                args = argsRaw.split(',').map(parseArg).map(function (arg) {
                    return arg === '__THIS__' ? context : arg;
                });
            }
        }

        var fn = fnName.split('.').reduce(function (obj, key) {
            return obj && obj[key];
        }, window);
        if (typeof fn === 'function') {
            fn.apply(window, args);
        }
    }

    document.addEventListener('click', function (event) {
        var target = event.target.closest('[data-click],[data-action],[data-resp-action]');
        if (!target) return;

        if (target.matches('a[href="#"]')) {
            event.preventDefault();
        }

        var action = target.getAttribute('data-action');
        if (action === 'history-back') { event.preventDefault(); history.back(); return; }
        if (action === 'open-responsible-pps' && typeof window.openResponsiblePPs === 'function') return window.openResponsiblePPs();
        if (action === 'save-memo-content' && typeof window.saveMemoContent === 'function') return window.saveMemoContent();
        if (action === 'get-lc-details' && typeof window.getLCDetails === 'function') return window.getLCDetails();
        if (action === 'get-matched-pp' && typeof window.getMatchedPP === 'function') return window.getMatchedPP();

        var respAction = target.getAttribute('data-resp-action');
        if (respAction && typeof window.addResponsibilityToMainTable === 'function') return window.addResponsibilityToMainTable(respAction);

        invokeExpression(target.getAttribute('data-click'), target);
    });

    document.addEventListener('change', function (event) {
        var target = event.target.closest('[data-change]');
        if (!target) return;
        invokeExpression(target.getAttribute('data-change'), target);
    });

    document.addEventListener('input', function (event) {
        var target = event.target.closest('[data-digits-only="true"]');
        if (!target) return;
        target.value = (target.value || '').replace(/[^0-9]/g, '');
    });

    document.addEventListener('paste', function (event) {
        var target = event.target.closest('[data-digits-only="true"]');
        if (!target) return;
        var text = (event.clipboardData || window.clipboardData).getData('text') || '';
        target.value = text.replace(/[^0-9]/g, '');
        event.preventDefault();
    });
})();

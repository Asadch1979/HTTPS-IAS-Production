(function () {
    function splitStatements(expression) {
        var statements = [];
        var current = '';
        var inSingle = false;
        var inDouble = false;

        for (var i = 0; i < expression.length; i++) {
            var ch = expression[i];
            var prev = i > 0 ? expression[i - 1] : '';

            if (ch === "'" && !inDouble && prev !== '\\') {
                inSingle = !inSingle;
            } else if (ch === '"' && !inSingle && prev !== '\\') {
                inDouble = !inDouble;
            }

            if (ch === ';' && !inSingle && !inDouble) {
                if (current.trim()) {
                    statements.push(current.trim());
                }
                current = '';
                continue;
            }

            current += ch;
        }

        if (current.trim()) {
            statements.push(current.trim());
        }

        return statements;
    }

    function splitArgs(argsRaw) {
        var args = [];
        var current = '';
        var inSingle = false;
        var inDouble = false;

        for (var i = 0; i < argsRaw.length; i++) {
            var ch = argsRaw[i];
            var prev = i > 0 ? argsRaw[i - 1] : '';

            if (ch === "'" && !inDouble && prev !== '\\') {
                inSingle = !inSingle;
            } else if (ch === '"' && !inSingle && prev !== '\\') {
                inDouble = !inDouble;
            }

            if (ch === ',' && !inSingle && !inDouble) {
                args.push(current.trim());
                current = '';
                continue;
            }

            current += ch;
        }

        if (current.trim()) {
            args.push(current.trim());
        }

        return args;
    }

    function parseArg(token, context, event) {
        var value = (token || '').trim();
        if (!value.length) return undefined;

        if (value === 'this') return context;
        if (value === 'event') return event;
        if (value === 'null') return null;
        if (value === 'undefined') return undefined;
        if (value === 'true') return true;
        if (value === 'false') return false;
        if (/^-?\d+$/.test(value)) return parseInt(value, 10);

        if ((value.startsWith("'") && value.endsWith("'")) || (value.startsWith('"') && value.endsWith('"'))) {
            return value.slice(1, -1);
        }

        if (Object.prototype.hasOwnProperty.call(window, value)) return window[value];
        return value;
    }

    function resolveFunction(functionPath) {
        return functionPath.split('.').reduce(function (obj, key) {
            return obj && obj[key];
        }, window);
    }

    function invokeCall(statement, context, event) {
        var match = statement.match(/^([\w$.]+)\s*\((.*)\)$/);
        if (!match) {
            var maybeFn = resolveFunction(statement);
            if (typeof maybeFn === 'function') {
                return maybeFn.call(window, event);
            }
            return;
        }

        var fn = resolveFunction(match[1]);
        if (typeof fn !== 'function') {
            return;
        }

        var argsRaw = (match[2] || '').trim();
        var args = [];
        if (argsRaw.length) {
            args = splitArgs(argsRaw).map(function (arg) {
                return parseArg(arg, context, event);
            });
        }

        return fn.apply(window, args);
    }

    function invokeExpression(expression, context, event) {
        if (!expression) return;
        var statements = splitStatements(expression.trim());

        for (var i = 0; i < statements.length; i++) {
            var statement = statements[i];
            if (statement === 'event.preventDefault()') {
                event.preventDefault();
                continue;
            }
            if (statement === 'event.stopPropagation()') {
                event.stopPropagation();
                continue;
            }

            invokeCall(statement, context, event);
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

        invokeExpression(target.getAttribute('data-click'), target, event);
    });

    document.addEventListener('change', function (event) {
        var target = event.target.closest('[data-change]');
        if (!target) return;
        invokeExpression(target.getAttribute('data-change'), target, event);
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

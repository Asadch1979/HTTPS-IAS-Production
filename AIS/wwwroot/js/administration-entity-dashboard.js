(function () {
    'use strict';
    var root = document.getElementById('entityWorkspace');
    if (!root) return;
    var tabs = Array.from(root.querySelectorAll('[data-entity-tab]'));
    function activate(index, focus) {
        tabs.forEach(function (tab, n) {
            tab.setAttribute('aria-selected', String(n === index));
            tab.tabIndex = n === index ? 0 : -1;
            document.getElementById(tab.getAttribute('aria-controls')).hidden = n !== index;
        });
        if (focus) tabs[index].focus();
    }
    tabs.forEach(function (tab, index) {
        tab.addEventListener('click', function () { activate(index, false); });
        tab.addEventListener('keydown', function (event) {
            var next = index;
            if (event.key === 'ArrowRight') next = (index + 1) % tabs.length;
            else if (event.key === 'ArrowLeft') next = (index + tabs.length - 1) % tabs.length;
            else if (event.key === 'Home') next = 0;
            else if (event.key === 'End') next = tabs.length - 1;
            else return;
            event.preventDefault(); activate(next, true);
        });
    });
    root.querySelectorAll('[data-change-field]').forEach(function (toggle) {
        toggle.addEventListener('change', function () {
            var input = document.getElementById(toggle.getAttribute('data-change-field'));
            input.disabled = !toggle.checked;
            if (!toggle.checked) input.value = '';
        });
    });
    var dirty = false;
    root.addEventListener('input', function () { dirty = true; });
    window.addEventListener('beforeunload', function (event) {
        if (dirty) { event.preventDefault(); event.returnValue = ''; }
    });
}());

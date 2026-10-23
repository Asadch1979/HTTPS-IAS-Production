/* Protected tabs are never cached. Every visit retrieves current Oracle-authorized data. */
(() => {
    'use strict';
    const content = document.getElementById('head-tab-content');
    const root = document.querySelector('.engagement-workbench');
    if (!content || !root) return;
    let request, generation = 0;
    function clear() {
        generation++;
        content.replaceChildren();
        document.querySelector('.wb-hero-meta')?.replaceChildren();
        document.querySelector('.dashboard-title p')?.replaceChildren();
        for (const key of ['period', 'position', 'scope', 'limitations']) delete root.dataset[key];
    }
    function fail(message) {
        request?.abort(); clear();
        const p = document.createElement('p'); p.textContent = message; content.append(p);
    }
    window.headAuditSecurity = { clear, fail, generation: () => generation };
    document.addEventListener('click', async event => {
        const link = event.target.closest('a.head-tab');
        if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        request?.abort();
        const current = new AbortController(); request = current;
        clear(); content.setAttribute('aria-busy', 'true');
        try {
            const response = await fetch(link.href, { signal: current.signal, credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' }, cache: 'no-store', redirect: 'error' });
            if (!response.ok || response.headers.get('X-Head-Audit-Context') !== root.dataset.contextKey)
                throw new Error('Dashboard access or assignment changed. Reload the page to continue.');
            const page = new DOMParser().parseFromString(await response.text(), 'text/html');
            const panel = page.getElementById('head-tab-content'), nextRoot = page.querySelector('.engagement-workbench'), hero = page.querySelector('.wb-hero-meta');
            if (!panel || !nextRoot || !hero) throw new Error('Invalid dashboard response.');
            if (current.signal.aborted) return;
            content.replaceChildren(...panel.childNodes);
            document.querySelector('.wb-hero-meta').replaceWith(hero);
            const scopeLabel = document.querySelector('.dashboard-title p');
            if (scopeLabel) scopeLabel.textContent = nextRoot.dataset.scope || ''; 
            for (const key of ['period', 'position', 'scope', 'limitations']) root.dataset[key] = nextRoot.dataset[key] || '';
            history.replaceState(null, '', link.href);
            document.querySelector('.wb-dashboard-filters input[name="tab"]').value = new URL(link.href).searchParams.get('tab');
            document.querySelectorAll('a.head-tab').forEach(a => {
                if (a === link) a.setAttribute('aria-current', 'page'); else a.removeAttribute('aria-current');
            });
            document.dispatchEvent(new Event('head-audit-tab-loaded'));
        } catch (error) {
            if (!current.signal.aborted) fail(error.message);
        } finally { if (request === current) content.removeAttribute('aria-busy'); }
    });
    const period = document.getElementById('head-period'), date = document.getElementById('head-position');
    period?.addEventListener('change', () => { date.value = ''; date.removeAttribute('min'); date.removeAttribute('max'); });
    window.addEventListener('pagehide', () => { request?.abort(); clear(); });
    window.addEventListener('pageshow', event => { if (event.persisted) { clear(); location.reload(); } });
})();

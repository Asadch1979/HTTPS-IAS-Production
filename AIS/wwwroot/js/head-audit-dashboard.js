/* Head Audit only. Cache visited DOM in memory; never persist portfolio data. */
(() => {
    'use strict';
    const content = document.getElementById('head-tab-content');
    if (!content) return;
    const cache = new Map();
    let activeUrl = document.querySelector('a.head-tab[aria-current="page"]')?.href || location.href, request;
    function remember() {
        cache.set(activeUrl, {
            nodes: Array.from(content.childNodes),
            hero: document.querySelector('.wb-hero-meta').cloneNode(true)
        });
    }
    remember();
    document.addEventListener('click', async event => {
        const link = event.target.closest('a.head-tab');
        if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        const url = link.href;
        request?.abort();
        const current = new AbortController(); request = current;
        content.setAttribute('aria-busy', 'true');
        try {
            const cached = cache.get(url);
            const readUrl = new URL(url);
            if (cached) readUrl.searchParams.set('checkOnly', 'true');
            const response = await fetch(readUrl, { signal: current.signal, credentials: 'same-origin', cache: 'no-store', redirect: 'error' });
            if (!response.ok || response.headers.get('X-Head-Audit-Context') !== document.querySelector('.engagement-workbench').dataset.contextKey)
                throw new Error('Dashboard access or assignment changed. Reload the page to continue.');
            let next = cached;
            if (!cached) {
                const page = new DOMParser().parseFromString(await response.text(), 'text/html');
                const panel = page.getElementById('head-tab-content');
                if (!panel) throw new Error('Invalid dashboard response.');
                next = { nodes: Array.from(panel.childNodes), hero: page.querySelector('.wb-hero-meta') };
            }
            if (current.signal.aborted) return;
            remember();
            content.replaceChildren(...next.nodes);
            document.querySelector('.wb-hero-meta').replaceWith(next.hero.cloneNode(true));
            activeUrl = url; cache.set(url, next);
            history.replaceState(null, '', url);
            const selected = new URL(url).searchParams.get('tab');
            document.querySelector('.wb-dashboard-filters input[name="tab"]').value = selected;
            document.querySelectorAll('a.head-tab').forEach(a => {
                if (a === link) a.setAttribute('aria-current', 'page'); else a.removeAttribute('aria-current');
            });
            document.dispatchEvent(new Event('head-audit-tab-loaded'));
        } catch (error) {
            if (current.signal.aborted) return;
            cache.clear(); content.replaceChildren();
            const message = document.createElement('p'); message.textContent = error.message;
            content.append(message);
        } finally { if (request === current) content.removeAttribute('aria-busy'); }
    });
    // A period change requests its own default cutoff; a manual cutoff remains validated server-side.
    const period = document.getElementById('head-period'), date = document.getElementById('head-position');
    period?.addEventListener('change', () => { date.value = ''; date.removeAttribute('min'); date.removeAttribute('max'); });
    window.addEventListener('pagehide', () => { request?.abort(); cache.clear(); });
})();


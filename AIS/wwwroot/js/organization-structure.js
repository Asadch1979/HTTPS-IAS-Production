(() => {
    'use strict';
    const app = document.getElementById('organization-structure');
    if (!app) return;
    const $ = id => document.getElementById(`org-${id}`);
    const PAGE_SIZE = 6, CARD_LIMIT = 40;
    const typeOrder = new Map([[18, 0], [3, 1], [4, 2], [21, 3]]);
    const compareEntities = (a, b) => (typeOrder.get(a.entityTypeId) ?? 4) - (typeOrder.get(b.entityTypeId) ?? 4) ||
        a.entityName.localeCompare(b.entityName) || a.entityId - b.entityId;
    const compareRoots = (a, b) => Number(b.entityTypeId === 2) - Number(a.entityTypeId === 2) || compareEntities(a, b);
    let nodes = new Map(), children = new Map(), roots = [], selected = null, home = null;
    let snapshot, scale = 1, pathVersion = 0, searchTimer, resultLimit = 50;
    let narrow = null, activeDrawer = null, fitted = false, layoutFrame = 0, panelState = null;
    const expanded = new Set(), navExpanded = new Set(), pages = new Map(), trail = [], paths = new Map();
    const statsCache = new Map();
    let counts = new Map(), countsLoading = true, countsRequestActive = false;
    const el = (tag, className, text) => {
        const item = document.createElement(tag);
        if (className) item.className = className;
        if (text !== undefined && text !== null) item.textContent = text;
        return item;
    };
    const button = (text, className, action, label) => {
        const item = el('button', className, text);
        item.type = 'button';
        if (label) item.setAttribute('aria-label', label);
        item.addEventListener('click', action);
        return item;
    };
    const childrenOf = id => children.get(id) || [];
    const notify = text => { $('status').textContent = text; };
    const categoryStyle = node => {
        if (node.hasTypeConflict) return 'department';
        switch (node.entityTypeId) {
            case 2: return 'executive';
            case 18: return 'group';
            case 3: return 'division';
            case 5: case 21: return 'office';
            case 6: return 'branch';
            default: return 'department';
        }
    };
    async function request(url) {
        const response = await fetch(url, { credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' } });
        if (response.status === 401 || response.status === 403 || response.redirected) throw new Error('Your session or access has changed. Reload the page or sign in again.');
        if (!response.ok) throw new Error('The organizational directory is temporarily unavailable. Please try again.');
        if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('Your session may have expired. Reload the page to continue.');
        return response.json();
    }

    async function load() {
        $('retry').hidden = true;
        $('workspace').hidden = true;
        notify('Loading your organizational directory…');
        try {
            snapshot = await request(app.dataset.nodesUrl);
            nodes = new Map(snapshot.nodes.map(node => [node.entityId, node]));
            children = new Map();
            paths.clear(); statsCache.clear(); navExpanded.clear(); trail.length = 0;
            for (const node of nodes.values()) {
                if (nodes.has(node.parentEntityId) && node.parentEntityId !== node.entityId) {
                    if (!children.has(node.parentEntityId)) children.set(node.parentEntityId, []);
                    children.get(node.parentEntityId).push(node);
                }
            }
            for (const entries of children.values()) entries.sort(compareEntities);
            roots = [...nodes.values()].filter(node => !nodes.has(node.parentEntityId) || node.parentEntityId === node.entityId).sort(compareRoots);
            // Keep disconnected cyclic components discoverable without inventing a parent.
            const reached = new Set();
            function mark(start) {
                const pending = [start];
                while (pending.length) {
                    const node = pending.pop();
                    if (reached.has(node.entityId)) continue;
                    reached.add(node.entityId);
                    pending.push(...childrenOf(node.entityId));
                }
            }
            roots.forEach(mark);
            for (const node of nodes.values()) if (!reached.has(node.entityId)) { roots.push(node); mark(node); }
            roots.sort(compareRoots);
            $('entity-count').textContent = `Entities in scope: ${nodes.size.toLocaleString()}`;
            $('scope').textContent = snapshot.isRestricted ? 'Your authorized organizational scope' : 'Full organizational directory';
            if (!nodes.size) { notify('No organizational entities are available in your permitted scope.'); return; }
            home = snapshot.rootEntityId ?? roots.find(n => n.isRoot && n.entityTypeId === 2)?.entityId ?? roots[0].entityId;
            const hashId = Number(new URLSearchParams(location.hash.slice(1)).get('entity'));
            selected = null;
            $('workspace').hidden = false;
            notify('');
            select(nodes.has(hashId) ? hashId : home, false);
            loadCounts();
        } catch (error) {
            notify(error.message);
            $('retry').hidden = false;
        }
    }

    function countValue(id, field) {
        if (countsLoading) return 'Loading…';
        const value = counts.get(id)?.[field];
        return Number.isSafeInteger(value) && value >= 0 ? value.toLocaleString() : 'Unavailable';
    }

    function updateCounts() {
        // Replace only count text: selection, branches, pagination, zoom and scroll stay intact.
        $('chart').querySelectorAll('.org-card-count').forEach(item => {
            item.textContent = `Open paras: ${countValue(Number(item.dataset.entityId), 'totalOpenParas')}`;
        });
        const host = $('detail-counts');
        if (host && selected !== null) renderDetailCounts(host);
    }

    async function loadCounts() {
        if (!nodes.size || countsRequestActive) return;
        countsRequestActive = true;
        countsLoading = true;
        $('refresh-counts').disabled = true;
        $('counts-retry').hidden = true;
        $('counts-status').textContent = 'Loading open para counts…';
        updateCounts();
        try {
            const rows = await request(app.dataset.countsUrl);
            counts = new Map(rows.filter(row => nodes.has(row.entityId)).map(row => [row.entityId, row]));
            const missing = [...nodes.keys()].some(id => ['ownOpenParas', 'subordinateOpenParas', 'totalOpenParas']
                .some(field => !Number.isSafeInteger(counts.get(id)?.[field]) || counts.get(id)[field] < 0));
            $('counts-status').textContent = missing ? 'Some open para counts are unavailable.' : '';
            $('counts-retry').hidden = !missing;
        } catch (error) {
            counts.clear();
            $('counts-status').textContent = `Open para counts unavailable. ${error.message}`;
            $('counts-retry').hidden = false;
        } finally {
            countsRequestActive = false;
            countsLoading = false;
            $('refresh-counts').disabled = false;
            updateCounts();
        }
    }

    function renderDetailCounts(host) {
        host.replaceChildren();
        for (const [field, label] of [['ownOpenParas', 'Own open paras'],
            ['subordinateOpenParas', 'Subordinate open paras'], ['totalOpenParas', 'Total open paras']]) {
            const row = el('div', 'org-para-stat');
            row.append(el('span', '', label), el('strong', '', countValue(selected, field)));
            host.append(row);
        }
    }

    function select(id, remember = true) {
        if (!nodes.has(id)) return;
        const changed = selected !== id;
        if (remember && selected !== null && selected !== id) trail.push(selected);
        selected = id;
        const selectedNode = nodes.get(id), query = $('search').value.trim().toLocaleLowerCase();
        if (query && !selectedNode.entityName.toLocaleLowerCase().includes(query) && !String(id).includes(query)) $('search').value = '';
        if ($('filter').value && Number($('filter').value) !== selectedNode.entityTypeId) $('filter').value = '';
        expanded.clear(); expanded.add(id); pages.clear();
        let ancestor = nodes.get(id);
        const visited = new Set();
        while (ancestor && !visited.has(ancestor.entityId)) {
            visited.add(ancestor.entityId); navExpanded.add(ancestor.entityId);
            ancestor = nodes.get(ancestor.parentEntityId);
        }
        history.replaceState(null, '', `${location.pathname}${location.search}#entity=${id}`);
        $('back').disabled = !trail.length;
        const parent = nodes.get(nodes.get(id).parentEntityId);
        $('up').disabled = !parent || parent.entityId === id;
        $('chart-title').textContent = nodes.get(id).entityName;
        scale = 1; fitted = false;
        if (activeDrawer) closeDrawer();
        renderNavigator(changed); renderChart(true); renderDetails(); renderPath(id);
    }

    function renderNavigator(selectionChanged = false) {
        const host = $('tree'), scrollTop = host.scrollTop; host.replaceChildren();
        const query = $('search').value.trim().toLocaleLowerCase();
        const filter = Number($('filter').value);
        if (query || filter) {
            const matches = [...nodes.values()].filter(n => (!filter || n.entityTypeId === filter) &&
                (!query || n.entityName.toLocaleLowerCase().includes(query) || String(n.entityId).includes(query))).sort(compareEntities);
            host.append(el('p', 'org-empty', `${matches.length} matching entities`));
            const list = el('ul');
            matches.slice(0, resultLimit).forEach(node => list.append(navItem(node, new Set(), false)));
            host.append(list);
            if (matches.length > resultLimit) host.append(button('Show more results', 'org-button', () => { resultLimit += 50; renderNavigator(); }));
        } else {
            const list = el('ul');
            const visited = new Set();
            roots.forEach(node => { const item = navItem(node, visited, true); if (item) list.append(item); });
            host.append(list);
        }
        host.scrollTop = scrollTop;
        const active = host.querySelector('.is-selected');
        if (active && host.clientHeight) {
            const row = active.getBoundingClientRect(), bounds = host.getBoundingClientRect();
            if (selectionChanged || row.top < bounds.top || row.bottom > bounds.bottom) {
                host.scrollTop += row.top < bounds.top || selectionChanged ? row.top - bounds.top : row.bottom - bounds.bottom;
            }
        }
    }

    function navItem(node, visited, hierarchical) {
        if (visited.has(node.entityId)) return null;
        visited.add(node.entityId);
        const item = el('li'), row = el('div', `org-tree-row${node.entityId === selected ? ' is-selected' : ''}${childrenOf(node.entityId).length ? ' is-parent' : ''}`);
        if (hierarchical && childrenOf(node.entityId).length) {
            const toggle = button(navExpanded.has(node.entityId) ? '−' : '+', 'org-tree-toggle', () => {
                if (navExpanded.has(node.entityId)) navExpanded.delete(node.entityId); else navExpanded.add(node.entityId);
                renderNavigator();
            }, `Expand or collapse ${node.entityName}`);
            toggle.setAttribute('aria-expanded', String(navExpanded.has(node.entityId)));
            row.append(toggle);
        } else row.append(el('span', 'org-tree-leaf', '·'));
        const choose = button('', 'org-tree-select', () => select(node.entityId));
        choose.append(el('span', '', node.entityName), el('small', '', node.directChildCount));
        choose.title = `${node.entityCategory} · ID ${node.entityId}`;
        if (node.entityId === selected) choose.setAttribute('aria-current', 'true');
        row.append(choose); item.append(row);
        if (hierarchical && navExpanded.has(node.entityId)) {
            const list = el('ul');
            for (const child of childrenOf(node.entityId)) { const nested = navItem(child, visited, true); if (nested) list.append(nested); }
            if (list.childElementCount) item.append(list);
        }
        return item;
    }

    function renderChart(center = false, anchor = null) {
        const host = $('chart'); host.replaceChildren();
        const rendered = new Set(); let limited = false, cyclic = false;
        const current = nodes.get(selected), parent = nodes.get(current.parentEntityId);
        function branch(node, authority = false) {
            if (rendered.has(node.entityId)) { cyclic = true; return null; }
            if (rendered.size >= CARD_LIMIT) { limited = true; return null; }
            rendered.add(node.entityId);
            const item = el('li', 'org-branch');
            const parentStack = el('div', 'org-parent');
            parentStack.append(card(node, authority));
            item.append(parentStack);
            const entries = authority ? [current] : childrenOf(node.entityId);
            if (authority || (expanded.has(node.entityId) && entries.length)) {
                const page = pages.get(node.entityId) || 0;
                const visible = authority ? entries : entries.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE);
                const list = el('ul', 'org-children');
                visible.forEach(child => { const nested = branch(child); if (nested) list.append(nested); });
                if (!authority && entries.length > PAGE_SIZE) {
                    const pager = el('div', 'org-pager');
                    pager.setAttribute('aria-label', `Direct offices of ${node.entityName}`);
                    pager.setAttribute('role', 'group');
                    const changePage = (nextPage, control) => {
                        const position = captureCard(node.entityId, control);
                        childrenOf(node.entityId).forEach(child => collapseBranch(child.entityId));
                        pages.set(node.entityId, nextPage); renderChart(false, position);
                    };
                    const previous = button('←', '', () => changePage(page - 1, 'previous'), `Previous direct offices of ${node.entityName}`);
                    const next = button('→', '', () => changePage(page + 1, 'next'), `Next direct offices of ${node.entityName}`);
                    previous.dataset.control = 'previous'; next.dataset.control = 'next';
                    previous.disabled = page === 0; next.disabled = (page + 1) * PAGE_SIZE >= entries.length;
                    pager.append(previous, el('span', '', `Direct offices: ${page * PAGE_SIZE + 1}–${Math.min((page + 1) * PAGE_SIZE, entries.length)} of ${entries.length}`), next);
                    parentStack.classList.add('has-pager'); parentStack.append(pager);
                }
                if (list.childElementCount) item.append(list);
            }
            return item;
        }
        const tree = el('ul', 'org-tree-chart');
        tree.append(branch(parent && parent.entityId !== selected ? parent : current, !!parent && parent.entityId !== selected));
        host.append(tree);
        $('chart-note').textContent = limited ? 'View limit reached. Select a deeper office to explore its subordinates.' :
            cyclic ? 'A repeated reporting relationship was omitted from this view.' :
                `${rendered.size} entities in view · Expand to reveal the next level`;
        requestAnimationFrame(() => {
            if (fitted) fitChart(); else sizeCanvas();
            if (anchor) restoreCard(anchor); else if (center) centerSelected();
        });
    }

    function collapseBranch(id) {
        const pending = [id], visited = new Set();
        while (pending.length) {
            const next = pending.pop();
            if (visited.has(next) || next === selected) continue;
            visited.add(next); expanded.delete(next); pages.delete(next);
            childrenOf(next).forEach(child => pending.push(child.entityId));
        }
    }

    function captureCard(id, control = 'expand') {
        const card = $('chart').querySelector(`[data-entity-id="${id}"]`);
        const rect = card?.getBoundingClientRect();
        return rect ? { id, x: rect.left, y: rect.top, control } : null;
    }

    function restoreCard(anchor) {
        const card = $('chart').querySelector(`[data-entity-id="${anchor.id}"]`);
        if (!card) return;
        const rect = card.getBoundingClientRect(), viewport = $('viewport');
        viewport.scrollLeft += rect.left - anchor.x;
        viewport.scrollTop += rect.top - anchor.y;
        const control = card.parentElement.querySelector(`[data-control="${anchor.control}"]:not(:disabled)`);
        (control || card.querySelector('button'))?.focus({ preventScroll: true });
    }

    function card(node, authority) {
        const item = el('article', `org-card org-card--${categoryStyle(node)}${node.entityId === selected ? ' is-selected' : ''}`);
        item.dataset.entityId = node.entityId;
        if (node.entityId === selected) item.dataset.selected = 'true';
        const main = button('', 'org-card-main', () => select(node.entityId), `Explore ${node.entityName}, entity ${node.entityId}`);
        main.title = `Select office: ${node.entityName}`;
        const category = el('span', 'org-card-category');
        const icon = el('i', `fa ${node.entityTypeId === 6 ? 'fa-building' : 'fa-sitemap'}`); icon.setAttribute('aria-hidden', 'true');
        category.append(icon, el('span', '', node.entityCategory));
        main.append(category, el('span', 'org-card-name', node.entityName), el('span', 'org-card-id', `ENTITY ${node.entityId}`));
        const count = el('span', 'org-card-count', `Open paras: ${countValue(node.entityId, 'totalOpenParas')}`);
        count.dataset.entityId = node.entityId;
        main.append(count);
        item.append(main);
        const footer = el('div', 'org-card-footer');
        footer.append(el('span', '', `${node.directChildCount} direct offices`));
        if (!authority && childrenOf(node.entityId).length) {
            const toggle = button(expanded.has(node.entityId) ? '− Collapse' : '+ Expand', 'org-expand', () => {
                const anchor = captureCard(node.entityId);
                if (expanded.has(node.entityId)) {
                    expanded.delete(node.entityId);
                    childrenOf(node.entityId).forEach(child => collapseBranch(child.entityId));
                } else {
                    const siblings = nodes.has(node.parentEntityId) ? childrenOf(node.parentEntityId) : roots;
                    siblings.filter(sibling => sibling.entityId !== node.entityId).forEach(sibling => collapseBranch(sibling.entityId));
                    expanded.add(node.entityId);
                }
                fitted = false; renderChart(false, anchor);
            }, `${expanded.has(node.entityId) ? 'Collapse' : 'Expand'} direct offices of ${node.entityName}`);
            toggle.dataset.control = 'expand';
            toggle.setAttribute('aria-expanded', String(expanded.has(node.entityId))); footer.append(toggle);
        } else footer.append(el('span', '', authority ? 'Authority' : 'Leaf office'));
        item.append(footer);
        if (node.isOrphan || node.isMissingName || node.hasTypeConflict || node.parentEntityId === node.entityId) {
            const note = el('span', 'org-record-note', '◦');
            note.title = [node.isOrphan && 'Reporting parent unavailable', node.isMissingName && 'Name supplied by directory fallback', node.hasTypeConflict && 'Conflicting office types', node.parentEntityId === node.entityId && 'Self-reporting record'].filter(Boolean).join(' · ');
            note.setAttribute('aria-label', note.title);
            item.append(note);
        }
        return item;
    }

    function statistics(id) {
        if (statsCache.has(id)) return statsCache.get(id);
        const seen = new Set([id]), pending = childrenOf(id).map(n => [n.entityId, 1]);
        let depth = 0, cycle = false;
        while (pending.length) {
            const [next, level] = pending.pop();
            if (seen.has(next)) { cycle = true; continue; }
            seen.add(next); depth = Math.max(depth, level);
            childrenOf(next).forEach(n => pending.push([n.entityId, level + 1]));
        }
        const result = { descendants: seen.size - 1, depth, cycle };
        statsCache.set(id, result); return result;
    }

    function renderDetails() {
        const node = nodes.get(selected), host = $('detail-content'), stats = statistics(selected);
        host.replaceChildren(el('h2', '', node.entityName),
            el('div', 'org-details-id', `ENTITY ID ${node.entityId}`), el('span', 'org-category-pill', node.entityCategory));
        const grid = el('div', 'org-stats');
        for (const [value, label] of [[node.directChildCount, 'Direct offices'], [stats.descendants, 'All subordinate offices']]) {
            const stat = el('div', 'org-stat'); stat.append(el('strong', '', value.toLocaleString()), el('span', '', label)); grid.append(stat);
        }
        host.append(grid, el('div', 'org-depth', stats.cycle ? 'Hierarchy depth unavailable · circular reporting record' : `${stats.depth} level${stats.depth === 1 ? '' : 's'} below this entity`));
        host.append(el('h3', '', 'Open paras'));
        const paraCounts = el('div', 'org-para-counts'); paraCounts.id = 'org-detail-counts';
        renderDetailCounts(paraCounts); host.append(paraCounts);
        host.append(el('h3', '', 'Reports to'));
        const parent = nodes.get(node.parentEntityId);
        if (parent && parent.entityId !== selected) host.append(button(parent.entityName, 'org-authority', () => select(parent.entityId)));
        else host.append(el('div', 'org-authority', node.isRoot ? 'Top-level organizational entity' : snapshot.isRestricted ? 'Reporting authority outside your scope' : node.parentEntityName || 'Reporting authority unavailable'));
        host.append(el('h3', '', 'Reporting chain'));
        const path = el('div'); path.id = 'org-detail-path'; path.textContent = 'Loading reporting chain…'; host.append(path);
        const notes = [];
        if (node.isOrphan) notes.push('The reporting parent is not present in the directory.');
        if (node.isMissingName) notes.push('The directory provides a fallback name for this entity.');
        if (node.hasTypeConflict) notes.push('Office type needs review; neutral styling is used.');
        if (node.parentEntityId === node.entityId) notes.push('This record reports to itself.');
        notes.push(snapshot.isRestricted ? 'Relationships and counts reflect your authorized organizational scope.' : 'Reporting relationships are provided by the organizational directory.');
        host.append(el('p', 'org-detail-note', notes.join(' ')));
    }

    async function renderPath(id) {
        const version = ++pathVersion;
        $('breadcrumbs').replaceChildren(el('span', '', 'Loading reporting path…'));
        try {
            let chain = paths.get(id);
            if (!chain) {
                const url = new URL(app.dataset.pathUrl, location.href); url.searchParams.set('entityId', id);
                chain = await request(url); paths.set(id, chain);
            }
            if (version !== pathVersion || selected !== id) return;
            const crumbs = $('breadcrumbs'), details = $('detail-path'); crumbs.replaceChildren(); details.replaceChildren();
            const list = el('ol', 'org-path-list');
            if (!chain.length) { crumbs.append(el('span', '', 'Reporting path unavailable')); details.append(el('p', 'org-empty', 'No reporting chain was returned for this entity.')); return; }
            chain.forEach((entry, index) => {
                if (index) crumbs.append(el('span', '', '›'));
                const choose = button(entry.entityName, '', () => select(entry.entityId));
                if (entry.entityId === id) choose.setAttribute('aria-current', 'page');
                crumbs.append(choose);
                const item = el('li'); item.append(button(entry.entityName, '', () => select(entry.entityId))); list.append(item);
                navExpanded.add(entry.entityId);
            });
            details.append(list); renderNavigator();
        } catch (error) {
            if (version !== pathVersion || selected !== id) return;
            $('breadcrumbs').replaceChildren(el('span', '', 'Reporting path unavailable'));
            $('detail-path').replaceChildren(el('p', 'org-empty', error.message), button('Retry reporting path', 'org-button', () => renderPath(id)));
        }
    }

    function sizeCanvas() {
        const chart = $('chart'), viewport = $('viewport'), space = $('chart-space');
        chart.style.minWidth = '0';
        const width = chart.offsetWidth, height = chart.offsetHeight;
        chart.style.transform = `scale(${scale})`;
        chart.style.left = `${Math.max(0, (viewport.clientWidth - width * scale) / 2)}px`;
        space.style.width = `${Math.max(viewport.clientWidth, width * scale)}px`;
        space.style.height = `${Math.max(viewport.clientHeight, height * scale)}px`;
        $('zoom-value').textContent = `${Math.round(scale * 100)}%`;
        $('zoom-out').disabled = scale <= .75; $('zoom-in').disabled = scale >= 1.5;
    }
    function centerSelected() {
        const card = $('chart').querySelector('[data-selected]');
        if (!card) return;
        const bounds = card.getBoundingClientRect(), viewport = $('viewport'), frame = viewport.getBoundingClientRect();
        viewport.scrollLeft += bounds.left - frame.left - (viewport.clientWidth - bounds.width) / 2;
        viewport.scrollTop += bounds.top - frame.top - Math.min(110, (viewport.clientHeight - bounds.height) / 2);
    }
    function zoom(value) { scale = Math.max(.75, Math.min(1.5, value)); sizeCanvas(); centerSelected(); }
    function toggleNavigator(show) {
        app.classList.toggle('nav-collapsed', !show);
        $('nav-toggle').setAttribute('aria-expanded', String(show));
        requestAnimationFrame(sizeCanvas);
    }
    $('nav-toggle').addEventListener('click', () => toggleNavigator(app.classList.contains('nav-collapsed')));
    $('retry').addEventListener('click', load);
    $('refresh-counts').addEventListener('click', loadCounts);
    $('counts-retry').addEventListener('click', loadCounts);
    $('search').addEventListener('input', () => { clearTimeout(searchTimer); searchTimer = setTimeout(() => { resultLimit = 50; renderNavigator(); }, 150); });
    $('filter').addEventListener('change', () => { resultLimit = 50; renderNavigator(); });
    $('home').addEventListener('click', () => { $('search').value = ''; $('filter').value = ''; select(home); });
    $('back').addEventListener('click', () => { if (trail.length) select(trail.pop(), false); });
    $('up').addEventListener('click', () => { const id = nodes.get(selected)?.parentEntityId; if (nodes.has(id)) select(id); });
    $('zoom-in').addEventListener('click', () => zoom(scale + .1));
    $('zoom-out').addEventListener('click', () => zoom(scale - .1));
    $('fit').addEventListener('click', () => {
        zoom(Math.min(($('viewport').clientWidth - 24) / $('chart').offsetWidth, ($('viewport').clientHeight - 24) / $('chart').offsetHeight, 1));
        if (scale === .75) $('chart-note').textContent = 'Readable fit · Scroll or drag to explore the wider structure.';
    });
    const viewport = $('viewport'); let drag = null;
    viewport.addEventListener('pointerdown', event => {
        if (event.pointerType === 'touch' || event.button !== 0 || event.target.closest('button')) return;
        drag = { x: event.clientX, y: event.clientY, left: viewport.scrollLeft, top: viewport.scrollTop };
        viewport.setPointerCapture(event.pointerId); viewport.classList.add('is-panning');
    });
    viewport.addEventListener('pointermove', event => {
        if (!drag) return;
        viewport.scrollLeft = drag.left - (event.clientX - drag.x); viewport.scrollTop = drag.top - (event.clientY - drag.y);
    });
    const stopDrag = () => { drag = null; viewport.classList.remove('is-panning'); };
    viewport.addEventListener('pointerup', stopDrag); viewport.addEventListener('pointercancel', stopDrag); viewport.addEventListener('lostpointercapture', stopDrag);
    new ResizeObserver(() => { if (selected !== null) sizeCanvas(); }).observe(viewport);
    if (matchMedia('(max-width:760px)').matches) toggleNavigator(false);
    load();
})();

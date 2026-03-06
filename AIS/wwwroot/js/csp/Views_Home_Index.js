(function(){
    const baseUrl = (document.querySelector('meta[name="base-url"]')?.getAttribute('content') || '').replace(/\/$/,'');
    const apiUrl = (path) => `${baseUrl}${path}`;

    const today = new Date();
    const todayPill = document.getElementById('iasTodayPill');
    if (todayPill){
        todayPill.textContent = today.toLocaleDateString(undefined, { weekday:'long', year:'numeric', month:'short', day:'numeric' });
    }

    let holidayMap = new Map(); // key: YYYY-MM-DD -> {name, raw}
    let calCursor = new Date(today.getFullYear(), today.getMonth(), 1);

    function pad2(n){ return String(n).padStart(2,'0'); }
    function toKey(d){ return `${d.getFullYear()}-${pad2(d.getMonth()+1)}-${pad2(d.getDate())}`; }

    function pick(obj, keys){
        for (const k of keys){
            if (obj && Object.prototype.hasOwnProperty.call(obj, k) && obj[k] != null) return obj[k];
            const found = obj ? Object.keys(obj).find(x => x.toLowerCase() === k.toLowerCase()) : null;
            if (found && obj[found] != null) return obj[found];
        }
        return null;
    }

    function parseHolidayDate(h){
        const v = pick(h, ['holiday_date','HolidayDate','HOLIDAY_DATE','date','Date','HOLIDAYDATE','PublicHolidayDate']);
        if (!v) return null;
        const d = new Date(v);
        if (!isNaN(d.getTime())) return d;

        if (typeof v === 'string'){
            const m = v.match(/^(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})$/);
            if (m){
                const dd = parseInt(m[1],10), mm = parseInt(m[2],10), yy = parseInt(m[3],10);
                const d2 = new Date(yy, mm-1, dd);
                if (!isNaN(d2.getTime())) return d2;
            }
        }
        return null;
    }

    function parseHolidayName(h){
        const v = pick(h, ['holiday_name','HolidayName','HOLIDAY_NAME','name','Name','HOLIDAY','holiday','description','Description','HOLIDAY_DESC']);
        return (v || 'Public Holiday').toString();
    }

    function setHolidayMap(holidays){
        holidayMap = new Map();
        (holidays || []).forEach(h => {
            const d = parseHolidayDate(h);
            if (!d) return;
            holidayMap.set(toKey(d), { name: parseHolidayName(h), raw: h });
        });
    }

    // Calendar
    const calTitle = document.getElementById('iasCalTitle');
    const calGrid = document.getElementById('iasCalGrid');

    function renderCalendar(){
        if (!calGrid) return;
        calGrid.innerHTML = '';

        const monthName = calCursor.toLocaleDateString(undefined, { month:'long', year:'numeric' });
        if (calTitle) calTitle.textContent = monthName;

        ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'].forEach(d => {
            const el = document.createElement('div');
            el.className = 'ias-cal-dow';
            el.textContent = d;
            calGrid.appendChild(el);
        });

        const first = new Date(calCursor.getFullYear(), calCursor.getMonth(), 1);
        const startDow = first.getDay();
        const start = new Date(first);
        start.setDate(first.getDate() - startDow);

        for (let i=0; i<42; i++){
            const d = new Date(start);
            d.setDate(start.getDate() + i);

            const cell = document.createElement('div');
            cell.className = 'ias-cal-cell';

            if (d.getMonth() !== calCursor.getMonth()) cell.classList.add('muted');

            const key = toKey(d);
            if (holidayMap.has(key)){
                cell.classList.add('holiday');
                cell.title = holidayMap.get(key).name;
            }

            if (d.getFullYear() === today.getFullYear() && d.getMonth() === today.getMonth() && d.getDate() === today.getDate()){
                cell.classList.add('today');
            }

            cell.textContent = d.getDate().toString();
            calGrid.appendChild(cell);
        }
    }

    // Holidays list
    const yearSel = document.getElementById('iasHolidayYear');
    const listEl = document.getElementById('iasHolidayList');
    const loadingEl = document.getElementById('iasHolidayLoading');
    const emptyEl = document.getElementById('iasHolidayEmpty');

    function setLoading(on){ if (loadingEl) loadingEl.style.display = on ? '' : 'none'; }
    function setEmpty(on){ if (emptyEl) emptyEl.style.display = on ? '' : 'none'; }

    function renderHolidayList(holidays){
        if (!listEl) return;
        listEl.innerHTML = '';

        const normalized = (holidays || [])
            .map(h => ({ raw:h, date: parseHolidayDate(h), name: parseHolidayName(h) }))
            .filter(x => x.date)
            .sort((a,b) => a.date - b.date);

        const now = new Date();
        const upcoming = normalized.filter(x => x.date >= new Date(now.getFullYear(), now.getMonth(), now.getDate()));
        const shown = (upcoming.length ? upcoming : normalized).slice(0, 10);

        if (shown.length === 0){
            setEmpty(true);
            return;
        }
        setEmpty(false);

        shown.forEach(x => {
            const li = document.createElement('li');

            const left = document.createElement('div');
            const name = document.createElement('p');
            name.className = 'name';
            name.textContent = x.name;

            const desc = document.createElement('p');
            desc.className = 'desc';
            desc.textContent = x.date.toLocaleDateString(undefined, { weekday:'short', year:'numeric', month:'short', day:'numeric' });

            left.appendChild(name);
            left.appendChild(desc);

            const badge = document.createElement('span');
            badge.className = 'ias-badge';
            badge.textContent = 'Holiday';

            li.appendChild(left);
            li.appendChild(badge);

            listEl.appendChild(li);
        });
    }

    async function fetchHolidays(year){
        setLoading(true);
        setEmpty(false);

        const payload = JSON.stringify({ year: Number(year) || 0 });

        let res = null;
        try{
            res = await fetchWithPageId(apiUrl('/ApiCalls/get_all_public_holidays'), {
                method: 'POST',
                headers: { 'Content-Type':'application/json' },
                body: payload,
                credentials: 'same-origin'
            });
        }catch(e){}

        if (!res || !res.ok){
            try{
                res = await fetchWithPageId(apiUrl('/ApiCalls/get_all_public_holidays'), {
                    method: 'GET',
                    headers: { 'Content-Type':'application/json' },
                    credentials: 'same-origin'
                });
            }catch(e){}
        }

        let data = [];
        try{
            if (res && res.ok) {
                const raw = await res.text();
                const parsed = tryParseJson(raw);

                if (Array.isArray(parsed)) {
                    data = parsed;
                } else if (parsed && typeof parsed === 'object' && Array.isArray(parsed.data)) {
                    data = parsed.data;
                }
            }
        }catch(e){ data = []; }

        setLoading(false);
        setHolidayMap(data);
        renderHolidayList(data);
        renderCalendar();
    }

    function initYears(){
        if (!yearSel) return;
        const y = today.getFullYear();
        [y-1, y, y+1].forEach(yy => {
            const opt = document.createElement('option');
            opt.value = String(yy);
            opt.textContent = String(yy);
            if (yy === y) opt.selected = true;
            yearSel.appendChild(opt);
        });
        yearSel.addEventListener('change', () => fetchHolidays(yearSel.value));
    }

    document.getElementById('iasCalPrev')?.addEventListener('click', () => { calCursor = new Date(calCursor.getFullYear(), calCursor.getMonth()-1, 1); renderCalendar(); });
    document.getElementById('iasCalNext')?.addEventListener('click', () => { calCursor = new Date(calCursor.getFullYear(), calCursor.getMonth()+1, 1); renderCalendar(); });
    document.getElementById('iasCalToday')?.addEventListener('click', () => { calCursor = new Date(today.getFullYear(), today.getMonth(), 1); renderCalendar(); });

    initYears();
    renderCalendar();
    fetchHolidays(today.getFullYear());
})();

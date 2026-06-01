import csv
import re
from collections import defaultdict
from pathlib import Path


REPO = Path(r"C:\Users\Asad\Documents\GitHub\HTTPS-IAS-Production")
AIS = REPO / "AIS"
OUT = Path(r"C:\Users\Asad\Documents\Codex\2026-06-01\review-and-upgrade-the-oracle-package\outputs")
OUT.mkdir(parents=True, exist_ok=True)

IGNORE_PARTS = {"Archive", "archive", "Backup", "backup", "Temp", "temp", "bin", "obj", ".vs"}


def rel(path: Path) -> str:
    return str(path.relative_to(REPO)).replace("/", "\\")


def active_path(path: Path) -> bool:
    return not bool(set(path.parts) & IGNORE_PARTS) and "Legacy" not in path.name


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="ignore")


def strip_js_comments(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", lambda m: "\n" * m.group(0).count("\n"), text, flags=re.S)
    text = re.sub(r"(?m)//.*$", "", text)
    return text


def line_no(text: str, idx: int) -> int:
    return text.count("\n", 0, idx) + 1


def normalize_js_src(src: str) -> str | None:
    if not src or src.startswith(("http://", "https://", "//")):
        return None
    src = src.split("?")[0].strip("\"'")
    src = src.replace("~", "").lstrip("/")
    if src.startswith("js/"):
        return "AIS\\wwwroot\\" + src.replace("/", "\\")
    if src.startswith("wwwroot/js/"):
        return "AIS\\" + src.replace("/", "\\")
    return None


def script_references():
    refs = defaultdict(list)
    inline_blocks = []
    script_src_re = re.compile(r"<script\b[^>]*\bsrc\s*=\s*[\"']([^\"']+)[\"'][^>]*>", re.I)
    script_block_re = re.compile(r"<script\b(?![^>]*\bsrc\s*=)[^>]*>(.*?)</script>", re.I | re.S)
    for view in (AIS / "Views").rglob("*.cshtml"):
        if not active_path(view):
            continue
        text = read(view)
        for m in script_src_re.finditer(text):
            normalized = normalize_js_src(m.group(1))
            if normalized:
                refs[normalized].append(f"{rel(view)}:{line_no(text, m.start())}")
        for m in script_block_re.finditer(text):
            content = m.group(1)
            if any(token in content for token in ("$.ajax", "$.get", "$.post", "fetch(")):
                inline_blocks.append({
                    "view": rel(view),
                    "line": line_no(text, m.start()),
                    "content": content,
                })
    return refs, inline_blocks


METHOD_RE = re.compile(
    r"(?m)^(?P<indent>\s*)(?P<attrs>(?:\[[^\]]+\]\s*)*)"
    r"(?P<access>public|private|protected|internal)\s+"
    r"(?:(?:static|virtual|override|async|sealed|new)\s+)*"
    r"(?P<ret>[\w<>\[\],\s\.\?]+?)\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*"
    r"\((?P<args>[^;{}]*)\)\s*(?:where[^{]+)?\{",
)


def iter_methods(path: Path):
    text = read(path)
    for m in METHOD_RE.finditer(text):
        open_idx = text.find("{", m.end() - 1)
        if open_idx < 0:
            continue
        depth = 0
        i = open_idx
        in_str = None
        escape = False
        while i < len(text):
            ch = text[i]
            if in_str:
                if escape:
                    escape = False
                elif ch == "\\":
                    escape = True
                elif ch == in_str:
                    in_str = None
            else:
                if ch in ('"', "'"):
                    in_str = ch
                elif ch == "{":
                    depth += 1
                elif ch == "}":
                    depth -= 1
                    if depth == 0:
                        yield {
                            "file": path,
                            "name": m.group("name"),
                            "access": m.group("access"),
                            "args": m.group("args"),
                            "attrs": m.group("attrs") or "",
                            "line": line_no(text, m.start()),
                            "body": text[open_idx:i + 1],
                            "prefix": text[max(0, m.start() - 1200):m.start()],
                        }
                        break
            i += 1


def parse_arg_names(args: str):
    params = []
    for arg in split_top_level(args):
        original = arg
        attrs = re.findall(r"\[([^\]]+)\]", arg)
        arg = re.sub(r"\[[^\]]+\]", "", arg).strip()
        if not arg:
            continue
        optional = "=" in arg
        arg_no_default = arg.split("=")[0].strip()
        bits = arg_no_default.split()
        if bits:
            name = bits[-1].lstrip("@")
            typ = bits[-2] if len(bits) >= 2 else ""
            simple = re.sub(r"\?$", "", typ) in {"string", "int", "long", "short", "bool", "DateTime", "decimal", "double", "float", "Guid"}
            params.append({
                "name": name,
                "type": typ,
                "optional": optional,
                "simple": simple,
                "from_body": any("FromBody" in a for a in attrs),
                "from_form": any("FromForm" in a for a in attrs),
                "raw": original.strip(),
            })
    return params


def split_top_level(text: str):
    out, cur = [], []
    depth = 0
    in_str = None
    for ch in text:
        if in_str:
            cur.append(ch)
            if ch == in_str:
                in_str = None
            continue
        if ch in ("'", '"'):
            in_str = ch
        elif ch in "([{":
            depth += 1
        elif ch in ")]}":
            depth = max(0, depth - 1)
        if ch == "," and depth == 0:
            out.append("".join(cur).strip())
            cur = []
        else:
            cur.append(ch)
    if "".join(cur).strip():
        out.append("".join(cur).strip())
    return out


def parse_controllers():
    actions = {}
    for path in (AIS / "Controllers").rglob("*.cs"):
        if not active_path(path):
            continue
        text = read(path)
        class_m = re.search(r"((?:\[[^\]]+\]\s*)*)\s*public\s+(?:partial\s+)?class\s+([A-Za-z_][A-Za-z0-9_]*)Controller\b", text)
        if not class_m:
            continue
        class_attrs = class_m.group(1) or ""
        controller = class_m.group(2)
        route_prefixes = [f"/{controller}"]
        route_attr = re.search(r'\[Route\("([^"]+)"\)\]', class_attrs)
        if route_attr:
            route_prefixes.append("/" + route_attr.group(1).replace("[controller]", controller).strip("/"))
        for method in iter_methods(path):
            if method["access"] != "public":
                continue
            attrs = method["attrs"]
            http_methods = []
            route_suffixes = [method["name"]]
            for hm in ("HttpGet", "HttpPost", "HttpPut", "HttpDelete"):
                for am in re.finditer(r"\[" + hm + r"(?:\((.*?)\))?\]", attrs, re.S):
                    http_methods.append(hm[4:].upper())
                    if am.group(1):
                        sm = re.search(r'"([^"]*)"', am.group(1))
                        if sm:
                            route_suffixes.append(sm.group(1).strip("/"))
            if not http_methods:
                http_methods = ["GET", "POST"]
            db_calls = sorted(set(re.findall(r"\b(?:dBConnection|dbConnection|_dbConnection|archiveDbConnection|archiveDbCon|db)\.([A-Za-z_][A-Za-z0-9_]*)\s*\(", method["body"])))
            info = {
                "controller": controller,
                "action": method["name"],
                "file": rel(path),
                "line": method["line"],
                "params": parse_arg_names(method["args"]),
                "http_methods": sorted(set(http_methods)),
                "db_calls": db_calls,
            }
            for prefix in route_prefixes:
                for suffix in route_suffixes:
                    route = (prefix.rstrip("/") + "/" + suffix.strip("/")).lower()
                    actions[route] = info
            actions[(f"/{controller}/{method['name']}").lower()] = info
    return actions


def parse_db_methods():
    methods = {}
    for path in AIS.glob("DBConnection*.cs"):
        if not active_path(path):
            continue
        for method in iter_methods(path):
            body = method["body"]
            procs = []
            for cmd in re.findall(r'CommandText\s*=\s*"([^"]+)"', body):
                if "." in cmd:
                    pkg, proc = cmd.split(".", 1)
                else:
                    pkg, proc = "", cmd
                procs.append((pkg, proc))
            if not procs:
                continue
            con_positions = [m.start() for m in re.finditer(r"DatabaseConnection\s*\(", body)]
            session_positions = [m.start() for m in re.finditer(r"GetUser\s*\(", body)]
            session_before_con = "N/A"
            if session_positions and con_positions:
                session_before_con = "Yes" if min(session_positions) < min(con_positions) else "No"
            methods[method["name"]] = {
                "file": rel(path),
                "line": method["line"],
                "packages": procs,
                "body": body,
                "stored_proc": "CommandType.StoredProcedure" in body,
                "bind_by_name": "BindByName = true" in body,
                "guard": "GuardAgainstDynamicSql(cmd)" in body,
                "using_connection": bool(re.search(r"using\s*(?:var|\()", body) and "DatabaseConnection" in body),
                "using_command": bool(re.search(r"using\s+(?:var\s+)?cmd\s*=\s*.*CreateCommand\s*\(", body) or re.search(r"using\s*\(\s*(?:OracleCommand|var)\s+\w+\s*=", body) or "using OracleCommand" in body),
                "using_reader": "ExecuteReader" not in body or bool(re.search(r"using\s+(?:var\s+)?(?:reader|rdr)\s*=\s*cmd\.ExecuteReader\s*\(", body) or "using OracleDataReader" in body or "using (OracleDataReader" in body),
                "session_before_con": session_before_con,
            }
    return methods


PROC_RE = re.compile(r"\bPROCEDURE\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:\((.*?)\))?\s*(?:AS|IS|;)", re.I | re.S)


def parse_packages():
    packages = defaultdict(lambda: {"spec": set(), "body": set()})
    for path in (AIS / "Docs" / "sql").glob("PKG_*.sql"):
        text = read(path)
        pkg = path.stem.lower()
        body_m = re.search(r"CREATE\s+OR\s+REPLACE\s+PACKAGE\s+BODY", text, re.I)
        split = body_m.start() if body_m else len(text)
        for name, _ in PROC_RE.findall(text[:split]):
            packages[pkg]["spec"].add(name.lower())
        for name, _ in PROC_RE.findall(text[split:]):
            packages[pkg]["body"].add(name.lower())
    return packages


def nearest_function(text: str, idx: int):
    prefix = text[:idx]
    patterns = [
        r"function\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(",
        r"(?:var|let|const)\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*function\s*\(",
        r"(?:var|let|const)\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*\([^)]*\)\s*=>",
        r"([A-Za-z_][A-Za-z0-9_]*)\s*:\s*function\s*\(",
    ]
    best = ("", -1)
    for pat in patterns:
        for m in re.finditer(pat, prefix):
            if m.start() > best[1]:
                best = (m.group(1), m.start())
    return best[0]


def extract_object_keys(text: str):
    keys = []
    for m in re.finditer(r"([A-Za-z_][A-Za-z0-9_]*|'[^']+'|\"[^\"]+\")\s*:", text):
        key = m.group(1).strip("'\"")
        if key not in {"url", "type", "method", "data", "success", "error", "cache", "contentType", "processData", "dataType", "headers"}:
            keys.append(key)
    return sorted(set(keys))


def clean_endpoint(expr: str, source_text: str, source_file: str):
    expr = expr.strip()
    string_parts = re.findall(r"['\"]([^'\"]+)['\"]", expr)
    if not string_parts:
        return "", "Dynamic endpoint expression"
    if not any("/" in p and not p.startswith("?") and not p.startswith("&") for p in string_parts):
        return "", "Dynamic endpoint expression"
    urlish = [p for p in string_parts if "/" in p and not p.startswith("?") and not p.startswith("&")]
    joined = urlish[-1] if urlish else "".join(p for p in string_parts if not p.startswith("?") and not p.startswith("&"))
    if joined.startswith("/"):
        endpoint = joined
    elif joined.startswith("ApiCalls/"):
        endpoint = "/" + joined
    elif joined and "/" in joined:
        endpoint = "/" + joined.strip("/")
    else:
        endpoint = joined
    if "apiBase" in expr and endpoint.startswith("/Get"):
        endpoint = "/ApiCalls" + endpoint
    if endpoint and not endpoint.startswith("/") and source_file.endswith(".cshtml"):
        endpoint = "/" + endpoint
    return endpoint, ""


def find_matching_block(text: str, start: int):
    brace = text.find("{", start)
    paren = text.find("(", start)
    open_idx = min([x for x in (brace, paren) if x >= 0], default=-1)
    if open_idx < 0:
        return text[start:start + 500]
    open_ch = text[open_idx]
    close_ch = "}" if open_ch == "{" else ")"
    depth = 0
    in_str = None
    escape = False
    for i in range(open_idx, min(len(text), open_idx + 8000)):
        ch = text[i]
        if in_str:
            if escape:
                escape = False
            elif ch == "\\":
                escape = True
            elif ch == in_str:
                in_str = None
            continue
        if ch in ("'", '"', "`"):
            in_str = ch
        elif ch == open_ch:
            depth += 1
        elif ch == close_ch:
            depth -= 1
            if depth == 0:
                return text[start:i + 1]
    return text[start:start + 2000]


def extract_calls_from_text(text: str, source_file: str, source_type: str):
    clean = strip_js_comments(text)
    calls = []
    # $.ajax({ url: ..., type/method: ..., data: {...} })
    for m in re.finditer(r"\$\.ajax\s*\(", clean):
        block = find_matching_block(clean, m.start())
        url_m = re.search(r"\burl\s*:\s*([^,\n]+(?:\+[^,\n]+)*)", block)
        if not url_m:
            continue
        endpoint, issue = clean_endpoint(url_m.group(1), clean, source_file)
        method_m = re.search(r"\b(?:type|method)\s*:\s*['\"]([^'\"]+)['\"]", block, re.I)
        data_m = re.search(r"\bdata\s*:\s*\{(.*?)\}", block, re.S)
        calls.append({
            "source_type": source_type,
            "source_file": source_file,
            "line": line_no(clean, m.start()),
            "function": nearest_function(clean, m.start()),
            "url": endpoint,
            "method": method_m.group(1).upper() if method_m else "GET",
            "js_params": extract_object_keys(data_m.group(1)) if data_m else [],
            "issue": issue,
        })
    # $.get / $.post
    for m in re.finditer(r"\$\.(get|post)\s*\(", clean, re.I):
        block = find_matching_block(clean, m.start())
        first_arg = split_top_level(block[block.find("(") + 1:-1])[0] if "(" in block else ""
        endpoint, issue = clean_endpoint(first_arg, clean, source_file)
        args = split_top_level(block[block.find("(") + 1:-1]) if "(" in block else []
        params = extract_object_keys(args[1]) if len(args) > 1 and "{" in args[1] else []
        calls.append({
            "source_type": source_type,
            "source_file": source_file,
            "line": line_no(clean, m.start()),
            "function": nearest_function(clean, m.start()),
            "url": endpoint,
            "method": "GET" if m.group(1).lower() == "get" else "POST",
            "js_params": params,
            "issue": issue,
        })
    # fetch(...)
    for m in re.finditer(r"\bfetch\s*\(", clean):
        block = find_matching_block(clean, m.start())
        args = split_top_level(block[block.find("(") + 1:-1]) if "(" in block else []
        first_arg = args[0] if args else ""
        endpoint, issue = clean_endpoint(first_arg, clean, source_file)
        method = "GET"
        if len(args) > 1:
            mm = re.search(r"\bmethod\s*:\s*['\"]([^'\"]+)['\"]", args[1], re.I)
            if mm:
                method = mm.group(1).upper()
        calls.append({
            "source_type": source_type,
            "source_file": source_file,
            "line": line_no(clean, m.start()),
            "function": nearest_function(clean, m.start()),
            "url": endpoint,
            "method": method,
            "js_params": [],
            "issue": issue,
        })
    return calls


def parse_all_js_calls(refs, inline_blocks):
    calls = []
    for js_rel, view_refs in refs.items():
        path = REPO / js_rel
        if path.exists() and active_path(path):
            for c in extract_calls_from_text(read(path), js_rel, "JS"):
                c["views"] = "; ".join(view_refs)
                c["referenced"] = "Yes"
                calls.append(c)
    for block in inline_blocks:
        for c in extract_calls_from_text(block["content"], f"{block['view']}:{block['line']}", "Inline View JS"):
            c["views"] = f"{block['view']}:{block['line']}"
            c["referenced"] = "Yes"
            calls.append(c)
    return calls


def map_action(actions, url: str):
    if not url:
        return None
    url = re.sub(r"\?.*$", "", url).rstrip("/")
    if not url.startswith("/"):
        url = "/" + url
    return actions.get(url.lower())


def status_for(call, action, db_infos, packages):
    issues = []
    fixes = []
    status = "Verified"
    if call["issue"]:
        issues.append(call["issue"])
        status = "Needs manual review"
    if not call["url"]:
        issues.append("API URL is dynamic or could not be statically resolved.")
        fixes.append("Verify runtime URL source in the referencing view.")
        return status, issues, fixes
    if not action:
        issues.append("API URL does not map to an active controller action.")
        fixes.append("Correct the JS endpoint or add/restore an active controller action only if this flow is still used.")
        return "Broken", issues, fixes
    if call["method"] not in action["http_methods"] and action["http_methods"] != ["GET", "POST"]:
        issues.append(f"HTTP method {call['method']} not declared by controller action ({'/'.join(action['http_methods'])}).")
        fixes.append("Align JS HTTP method with controller attribute.")
        status = "Broken"
    js_params = set(k.lower() for k in call["js_params"])
    action_params = action["params"]
    action_param_names = set(p["name"].lower() for p in action_params)
    has_complex_model = any((not p["simple"]) or p["from_body"] or p["from_form"] for p in action_params)
    required_simple = set(
        p["name"].lower()
        for p in action_params
        if p["simple"] and not p["optional"] and not p["from_body"] and not p["from_form"]
    )
    if js_params and action_params and not has_complex_model:
        missing = sorted(k for k in required_simple - js_params if k not in {"model", "request", "file", "files"})
        extra = sorted(k for k in js_params - action_param_names)
        if missing:
            issues.append("JS data is missing controller parameters: " + ", ".join(missing))
            fixes.append("Add the missing JS data keys or update the controller signature if model binding is intentional.")
            status = "Broken"
        if extra and len(extra) <= 8:
            issues.append("JS sends parameters not present in controller signature: " + ", ".join(extra))
            fixes.append("Remove unused JS data keys or add explicit controller parameters if expected.")
            status = "Needs manual review" if status == "Verified" else status
    elif js_params and has_complex_model:
        status = "Needs manual review" if status == "Verified" else status
        issues.append("Controller uses complex model binding; JS data keys require model-level verification.")
        fixes.append("Confirm JS data keys match the bound request/model properties.")
    if not action["db_calls"]:
        return status, issues, fixes
    for db in db_infos:
        if not db:
            issues.append("Controller calls a DBConnection method that is not active or not indexed.")
            fixes.append("Restore active DBConnection method or redirect controller to the replacement method.")
            status = "Broken"
            continue
        if not (db["stored_proc"] and db["bind_by_name"] and db["guard"]):
            missing = []
            if not db["stored_proc"]:
                missing.append("CommandType.StoredProcedure")
            if not db["bind_by_name"]:
                missing.append("BindByName = true")
            if not db["guard"]:
                missing.append("GuardAgainstDynamicSql(cmd)")
            issues.append("DBConnection hardening missing: " + ", ".join(missing))
            fixes.append("Add the missing DBConnection hardening statements around the OracleCommand.")
            status = "Broken"
        if not (db["using_connection"] and db["using_command"] and db["using_reader"]):
            issues.append("DBConnection disposal pattern needs review.")
            fixes.append("Use using blocks/declarations for connection, command, and reader.")
            status = "Needs manual review" if status == "Verified" else status
        for pkg, proc in db["packages"]:
            pkgdata = packages.get(pkg.lower())
            if not pkgdata or proc.lower() not in pkgdata["spec"] or proc.lower() not in pkgdata["body"]:
                issues.append(f"Package procedure missing from checked-in source: {pkg}.{proc}")
                fixes.append("Extract live DB package source or confirm obsolete/replaced flow before adding declarations.")
                status = "Broken"
    return status, issues, fixes


def main():
    refs, inline_blocks = script_references()
    actions = parse_controllers()
    db_methods = parse_db_methods()
    packages = parse_packages()
    calls = parse_all_js_calls(refs, inline_blocks)

    rows = []
    for call in calls:
        action = map_action(actions, call["url"])
        db_names = action["db_calls"] if action else []
        if not db_names:
            status, issues, fixes = status_for(call, action, [], packages)
            rows.append(row_for(call, action, "", None, "", "", status, issues, fixes))
        else:
            db_infos = [db_methods.get(name) for name in db_names]
            status, issues, fixes = status_for(call, action, db_infos, packages)
            for name, db in zip(db_names, db_infos):
                if db and db["packages"]:
                    for pkg, proc in db["packages"]:
                        rows.append(row_for(call, action, name, db, pkg, proc, status, issues, fixes))
                else:
                    rows.append(row_for(call, action, name, db, "", "", status, issues, fixes))

    all_js = {rel(p) for p in (AIS / "wwwroot" / "js").rglob("*.js") if active_path(p)}
    referenced = set(refs.keys())
    unref = sorted(all_js - referenced)

    fields = [
        "JS file",
        "View/layout using it",
        "JS function/event",
        "API URL",
        "HTTP method",
        "Controller/action",
        "Controller parameters",
        "JS parameters",
        "DBConnection method",
        "Package/procedure",
        "Package spec exists",
        "Package body exists",
        "DB hardening",
        "Status",
        "Issue found",
        "Recommended fix",
    ]
    register = OUT / "js_to_db_contract_register.csv"
    with register.open("w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(rows)

    broken = [r for r in rows if r["Status"] == "Broken"]
    missing_pkg = [r for r in rows if "Package procedure missing" in r["Issue found"]]
    write_csv(OUT / "broken_js_api_calls.csv", fields, broken)
    write_csv(OUT / "js_calls_missing_package_source.csv", fields, missing_pkg)
    write_csv(OUT / "unreferenced_active_js_files.csv", ["JS file"], [{"JS file": x} for x in unref])

    summary = OUT / "js_api_audit_summary.md"
    counts = defaultdict(int)
    for r in rows:
        counts[r["Status"]] += 1
    with summary.open("w", encoding="utf-8") as f:
        f.write("# JS API Audit Summary\n\n")
        js_files_with_calls = {
            r["JS file"].split(":")[0]
            for r in rows
            if r["JS file"].split(":")[0].lower().endswith(".js")
        }
        inline_rows = sum(1 for r in rows if not r["JS file"].split(":")[0].lower().endswith(".js"))
        f.write(f"- Referenced active JS files with API calls scanned: {len(js_files_with_calls)}\n")
        f.write(f"- Inline view script API trace rows scanned: {inline_rows}\n")
        f.write(f"- Register rows: {len(rows)}\n")
        for key in ("Verified", "Needs manual review", "Broken"):
            f.write(f"- {key}: {counts[key]}\n")
        f.write(f"- JS files under wwwroot/js not referenced by active views/layouts: {len(unref)}\n")
        f.write(f"- Broken JS API rows: {len(broken)}\n")
        f.write(f"- Rows hitting missing checked-in package source: {len(missing_pkg)}\n")
    print(summary)
    print(register)
    print(OUT / "broken_js_api_calls.csv")
    print(OUT / "js_calls_missing_package_source.csv")
    print(OUT / "unreferenced_active_js_files.csv")
    print(f"rows={len(rows)} verified={counts['Verified']} manual={counts['Needs manual review']} broken={counts['Broken']} unreferenced={len(unref)} missing_pkg={len(missing_pkg)}")


def row_for(call, action, db_name, db, pkg, proc, status, issues, fixes):
    spec = body = ""
    if pkg and proc:
        pdata = parse_packages_cache.get(pkg.lower())
        if pdata:
            spec = "Yes" if proc.lower() in pdata["spec"] else "No"
            body = "Yes" if proc.lower() in pdata["body"] else "No"
        else:
            spec = body = "No"
    db_hardening = ""
    if db:
        db_hardening = (
            f"StoredProcedure={yn(db['stored_proc'])}; "
            f"BindByName={yn(db['bind_by_name'])}; "
            f"Guard={yn(db['guard'])}; "
            f"UsingCon={yn(db['using_connection'])}; "
            f"UsingCmd={yn(db['using_command'])}; "
            f"UsingReader={yn(db['using_reader'])}; "
            f"SessionBeforeConnection={db['session_before_con']}"
        )
    return {
        "JS file": call["source_file"] + f":{call['line']}",
        "View/layout using it": call["views"],
        "JS function/event": call["function"],
        "API URL": call["url"],
        "HTTP method": call["method"],
        "Controller/action": f"{action['controller']}.{action['action']}" if action else "",
        "Controller parameters": ", ".join(p["name"] for p in action["params"]) if action else "",
        "JS parameters": ", ".join(call["js_params"]),
        "DBConnection method": db_name,
        "Package/procedure": f"{pkg}.{proc}" if pkg else "",
        "Package spec exists": spec,
        "Package body exists": body,
        "DB hardening": db_hardening,
        "Status": status,
        "Issue found": " | ".join(sorted(set(issues))),
        "Recommended fix": " | ".join(sorted(set(fixes))),
    }


def write_csv(path: Path, fields, rows):
    with path.open("w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(rows)


def yn(value):
    return "Yes" if value else "No"


parse_packages_cache = parse_packages()


if __name__ == "__main__":
    main()

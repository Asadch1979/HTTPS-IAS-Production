import csv
import os
import re
from collections import defaultdict
from pathlib import Path


REPO = Path(r"C:\Users\Asad\Documents\GitHub\HTTPS-IAS-Production")
AIS = REPO / "AIS"
OUT = Path(r"C:\Users\Asad\Documents\Codex\2026-06-01\review-and-upgrade-the-oracle-package\outputs")
OUT.mkdir(parents=True, exist_ok=True)

IGNORE_PARTS = {
    "bin",
    "obj",
    ".vs",
    "Archive",
    "archive",
    "Backup",
    "backup",
    "Temp",
    "temp",
}


def rel(path: Path) -> str:
    return str(path.relative_to(REPO)).replace("/", "\\")


def is_active_source(path: Path) -> bool:
    parts = set(path.parts)
    return not bool(parts & IGNORE_PARTS) and "Legacy" not in path.name


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="ignore")


def line_no(text: str, idx: int) -> int:
    return text.count("\n", 0, idx) + 1


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
        end = open_idx
        in_str = None
        escape = False
        i = open_idx
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
                        end = i + 1
                        break
            i += 1
        yield {
            "name": m.group("name"),
            "access": m.group("access"),
            "return": " ".join(m.group("ret").split()),
            "args": " ".join(m.group("args").split()),
            "start": m.start(),
            "end": end,
            "line": line_no(text, m.start()),
            "body": text[open_idx:end],
            "signature": text[m.start():open_idx].strip(),
            "file": path,
        }


def parse_controller_actions():
    actions = {}
    for path in (AIS / "Controllers").rglob("*.cs"):
        if not is_active_source(path):
            continue
        controller = path.stem
        if controller.endswith("Controller"):
            controller = controller[:-10]
        for method in iter_methods(path):
            if method["access"] != "public":
                continue
            body = method["body"]
            db_calls = sorted(set(re.findall(r"\b(?:dBConnection|dbConnection|_dbConnection|archiveDbConnection|archiveDbCon)\.([A-Za-z_][A-Za-z0-9_]*)\s*\(", body)))
            attrs = []
            prefix = read(path)[max(0, method["start"] - 500):method["start"]]
            for attr in re.findall(r"\[([^\]]+)\]", prefix):
                if any(x in attr for x in ("HttpGet", "HttpPost", "Route")):
                    attrs.append(attr.strip())
            route_names = {method["name"].lower()}
            for attr in attrs:
                mm = re.search(r'"([^"]+)"', attr)
                if mm:
                    route_names.add(mm.group(1).strip("/").split("/")[-1].lower())
            key = (controller.lower(), method["name"].lower())
            actions[key] = {
                "controller": controller,
                "action": method["name"],
                "file": rel(path),
                "line": method["line"],
                "db_calls": db_calls,
                "routes": sorted(route_names),
            }
            for rn in route_names:
                actions[(controller.lower(), rn)] = actions[key]
    return actions


def parse_frontend_endpoints():
    rows = []
    paths = list((AIS / "wwwroot").rglob("*.js")) + list((AIS / "Views").rglob("*.cshtml"))
    url_patterns = [
        re.compile(r'url\s*:\s*(?:g_asiBaseURL\s*\+\s*)?["\']([^"\']+)["\']', re.I),
        re.compile(r'fetch\s*\(\s*(?:g_asiBaseURL\s*\+\s*)?["\']([^"\']+)["\']', re.I),
        re.compile(r'\$\.(?:get|post)\s*\(\s*(?:g_asiBaseURL\s*\+\s*)?["\']([^"\']+)["\']', re.I),
        re.compile(r'Url\.Action\("([^"]+)"\s*,\s*"([^"]+)"', re.I),
        re.compile(r'asp-action="([^"]+)"(?:\s+asp-controller="([^"]+)")?', re.I),
    ]
    for path in paths:
        active = is_active_source(path)
        text = read(path)
        for idx, line in enumerate(text.splitlines(), start=1):
            for pat in url_patterns:
                for m in pat.finditer(line):
                    if "Url.Action" in pat.pattern:
                        action, controller = m.group(1), m.group(2)
                        url = f"/{controller}/{action}"
                    elif "asp-action" in pat.pattern:
                        action = m.group(1)
                        controller = m.group(2) or ""
                        url = f"/{controller}/{action}" if controller else action
                    else:
                        url = m.group(1)
                    rows.append({
                        "source_type": "JS" if path.suffix == ".js" else "View",
                        "source_file": rel(path),
                        "line": idx,
                        "endpoint": url,
                        "active_source": "Yes" if active else "No",
                    })
    return rows


def endpoint_to_controller_action(endpoint: str):
    ep = endpoint.strip().strip("'\"")
    ep = re.sub(r"\?.*$", "", ep)
    ep = ep.replace("~", "")
    parts = [p for p in ep.strip("/").split("/") if p and "{" not in p]
    if len(parts) >= 2:
        return parts[-2], parts[-1]
    if len(parts) == 1:
        return "", parts[0]
    return "", ""


def parse_db_methods():
    methods = {}
    for path in AIS.glob("DBConnection*.cs"):
        if path.name.endswith(".Legacy.cs"):
            continue
        text = read(path)
        for method in iter_methods(path):
            body = method["body"]
            procs = re.findall(r'CommandText\s*=\s*"([^"]+)"', body)
            proc_names = []
            for p in procs:
                if "." in p:
                    pkg, proc = p.split(".", 1)
                else:
                    pkg, proc = "", p
                proc_names.append((pkg.strip(), proc.strip()))
            params = sorted(set(re.findall(r'Parameters\.Add\s*\(\s*"([^"]+)"', body)))
            methods[method["name"]] = {
                "name": method["name"],
                "file": rel(path),
                "line": method["line"],
                "packages": proc_names,
                "params": params,
                "uses_stored_proc": "CommandType.StoredProcedure" in body,
                "bind_by_name": "BindByName = true" in body,
                "guard": "GuardAgainstDynamicSql(cmd)" in body,
                "execute_reader": "ExecuteReader" in body,
                "execute_nonquery": "ExecuteNonQuery" in body,
                "body": body,
            }
    return methods


PROC_DECL_RE = re.compile(
    r"\bPROCEDURE\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:\((.*?)\))?\s*(?:AS|IS|;)",
    re.I | re.S,
)


def parse_param_names(param_text: str):
    if not param_text:
        return []
    params = []
    depth = 0
    current = []
    for ch in param_text:
        if ch == "(":
            depth += 1
        elif ch == ")":
            depth -= 1
        if ch == "," and depth == 0:
            token = "".join(current).strip()
            if token:
                params.append(token)
            current = []
        else:
            current.append(ch)
    token = "".join(current).strip()
    if token:
        params.append(token)
    names = []
    for token in params:
        mm = re.match(r'"?([A-Za-z_][A-Za-z0-9_]*)"?\s+', token.strip(), re.I)
        if mm:
            names.append(mm.group(1))
    return names


def parse_packages():
    packages = defaultdict(lambda: {"spec": {}, "body": {}})
    for path in (AIS / "Docs" / "sql").glob("PKG_*.sql"):
        text = read(path)
        pkg = path.stem.lower()
        body_pos = re.search(r"CREATE\s+OR\s+REPLACE\s+PACKAGE\s+BODY", text, re.I)
        split = body_pos.start() if body_pos else len(text)
        spec_text = text[:split]
        body_text = text[split:]
        for name, params in PROC_DECL_RE.findall(spec_text):
            packages[pkg]["spec"][name.lower()] = {
                "name": name,
                "params": parse_param_names(params),
                "file": rel(path),
            }
        for name, params in PROC_DECL_RE.findall(body_text):
            packages[pkg]["body"][name.lower()] = {
                "name": name,
                "params": parse_param_names(params),
                "file": rel(path),
            }
    return packages


def main():
    actions = parse_controller_actions()
    front = parse_frontend_endpoints()
    db_methods = parse_db_methods()
    packages = parse_packages()

    active_endpoint_refs = defaultdict(list)
    rows = []
    for src in front:
        controller, action = endpoint_to_controller_action(src["endpoint"])
        action_info = None
        if controller:
            action_info = actions.get((controller.lower(), action.lower()))
        else:
            matches = [v for (c, a), v in actions.items() if a == action.lower()]
            action_info = matches[0] if len(matches) == 1 else None
            controller = action_info["controller"] if action_info else ""
        if src["active_source"] == "Yes" and action_info:
            active_endpoint_refs[(action_info["controller"], action_info["action"])].append(src)
        db_calls = action_info["db_calls"] if action_info else []
        if not db_calls:
            rows.append(make_row(src, controller, action, action_info, "", None, packages, ""))
        else:
            for db_call in db_calls:
                rows.append(make_row(src, controller, action, action_info, db_call, db_methods.get(db_call), packages, ""))

    internal_rows = []
    active_controller_db_refs = defaultdict(list)
    for key, action_info in actions.items():
        if key[1] != action_info["action"].lower():
            continue
        is_active_api = (action_info["controller"], action_info["action"]) in active_endpoint_refs
        for db_call in action_info["db_calls"]:
            active_controller_db_refs[db_call].append(action_info)
            src = {
                "source_type": "Controller",
                "source_file": action_info["file"],
                "line": action_info["line"],
                "endpoint": f"/{action_info['controller']}/{action_info['action']}",
                "active_source": "Yes" if is_active_api else "Unconfirmed",
            }
            internal_rows.append(make_row(src, action_info["controller"], action_info["action"], action_info, db_call, db_methods.get(db_call), packages, "Internal C# call"))

    all_rows = rows + internal_rows

    # DB methods with stored procedure calls but no controller reference.
    for name, info in db_methods.items():
        if not info["packages"]:
            continue
        if name in active_controller_db_refs:
            continue
        src = {
            "source_type": "Internal C# call",
            "source_file": "",
            "line": "",
            "endpoint": "",
            "active_source": "No",
        }
        all_rows.append(make_row(src, "", "", None, name, info, packages, "No active controller reference found"))

    csv_path = OUT / "full_api_to_db_contract_register.csv"
    fields = [
        "Source type",
        "Source file",
        "Function/button/event/action name",
        "URL/API endpoint called",
        "Controller name",
        "Controller action method",
        "DBConnection method called",
        "DBConnection file",
        "Package name",
        "Procedure name",
        "Procedure exists in package spec",
        "Procedure exists in package body",
        "Active usage confirmed",
        "Replacement method/procedure, if any",
        "Final action",
        "Remarks",
    ]
    with csv_path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        writer.writerows(all_rows)

    missing = [r for r in all_rows if r["Package name"] and (r["Procedure exists in package spec"] == "No" or r["Procedure exists in package body"] == "No")]
    missing_path = OUT / "missing_live_db_source_extraction_list.csv"
    with missing_path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        writer.writerows(missing)

    legacy_candidates = []
    for name, info in sorted(db_methods.items()):
        if not info["packages"]:
            continue
        refs = active_controller_db_refs.get(name, [])
        active_refs = [r for r in refs if (r["controller"], r["action"]) in active_endpoint_refs]
        if not refs:
            final = "Move to Legacy"
            reason = "No controller reference found in active controllers."
        elif not active_refs:
            final = "Needs manual review"
            reason = "Controller reference exists, but no active frontend endpoint reference was found."
        else:
            final = "Keep"
            reason = "Active controller/frontend reference found."
        legacy_candidates.append({
            "Method/action moved": name if final == "Move to Legacy" else "",
            "Old location": f"{info['file']}:{info['line']}",
            "New legacy file": legacy_file_for(info["file"]) if final == "Move to Legacy" else "",
            "Reason": reason,
            "Replacement method/procedure": "",
            "Active caller remaining": "Yes" if refs else "No",
            "Final action": final,
        })
    legacy_path = OUT / "legacy_movement_register.csv"
    legacy_fields = ["Method/action moved", "Old location", "New legacy file", "Reason", "Replacement method/procedure", "Active caller remaining", "Final action"]
    with legacy_path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=legacy_fields)
        writer.writeheader()
        writer.writerows(legacy_candidates)

    md_path = OUT / "end_to_end_cleanup_summary.md"
    with md_path.open("w", encoding="utf-8") as f:
        f.write("# AIS API to DB Contract Audit\n\n")
        f.write(f"- Frontend endpoint references scanned: {len(front)}\n")
        f.write(f"- Controller actions indexed: {len({id(v) for v in actions.values()})}\n")
        f.write(f"- DBConnection stored-procedure methods indexed: {sum(1 for v in db_methods.values() if v['packages'])}\n")
        f.write(f"- Register rows: {len(all_rows)}\n")
        f.write(f"- Missing package source rows: {len(missing)}\n")
        f.write(f"- DBConnection move-to-legacy candidates: {sum(1 for r in legacy_candidates if r['Final action'] == 'Move to Legacy')}\n\n")
        f.write("## Files\n\n")
        f.write(f"- Full API-to-DB register: `{csv_path}`\n")
        f.write(f"- Missing live DB source extraction list: `{missing_path}`\n")
        f.write(f"- Legacy movement register: `{legacy_path}`\n")
    print(md_path)
    print(csv_path)
    print(missing_path)
    print(legacy_path)
    print(f"rows={len(all_rows)} missing={len(missing)} legacy_candidates={sum(1 for r in legacy_candidates if r['Final action'] == 'Move to Legacy')}")


def make_row(src, controller, action, action_info, db_call, db_info, packages, remark_prefix):
    package_name = ""
    procedure_name = ""
    spec = ""
    body = ""
    db_file = ""
    final = "Keep"
    remarks = remark_prefix
    if action_info is None and src["endpoint"]:
        final = "Needs manual review"
        remarks = (remarks + "; " if remarks else "") + "Endpoint did not map to an indexed controller action."
    if db_call and db_info is None:
        final = "Needs manual review"
        remarks = (remarks + "; " if remarks else "") + "Controller calls a DBConnection method that was not indexed."
    if db_info:
        db_file = db_info["file"]
        if db_info["packages"]:
            pkg, proc = db_info["packages"][0]
            package_name = pkg
            procedure_name = proc
            pkg_data = packages.get(pkg.lower())
            if pkg_data:
                spec_info = pkg_data["spec"].get(proc.lower())
                body_info = pkg_data["body"].get(proc.lower())
                spec = "Yes" if spec_info else "No"
                body = "Yes" if body_info else "No"
                if spec == "No" or body == "No":
                    final = "Needs DB extraction"
                    remarks = (remarks + "; " if remarks else "") + "Procedure not fully present in checked-in package source."
            else:
                spec = "No"
                body = "No"
                final = "Needs DB extraction"
                remarks = (remarks + "; " if remarks else "") + "Package SQL source file not found."
        if db_info["packages"]:
            if not db_info["uses_stored_proc"] or not db_info["bind_by_name"] or not db_info["guard"]:
                final = "Needs manual review"
                missing_bits = []
                if not db_info["uses_stored_proc"]:
                    missing_bits.append("CommandType.StoredProcedure")
                if not db_info["bind_by_name"]:
                    missing_bits.append("BindByName")
                if not db_info["guard"]:
                    missing_bits.append("GuardAgainstDynamicSql")
                remarks = (remarks + "; " if remarks else "") + "Missing " + ", ".join(missing_bits)
    return {
        "Source type": src["source_type"],
        "Source file": f"{src['source_file']}:{src['line']}" if src.get("line") else src["source_file"],
        "Function/button/event/action name": action,
        "URL/API endpoint called": src["endpoint"],
        "Controller name": action_info["controller"] if action_info else controller,
        "Controller action method": action_info["action"] if action_info else action,
        "DBConnection method called": db_call,
        "DBConnection file": db_file,
        "Package name": package_name,
        "Procedure name": procedure_name,
        "Procedure exists in package spec": spec,
        "Procedure exists in package body": body,
        "Active usage confirmed": src["active_source"],
        "Replacement method/procedure, if any": "",
        "Final action": final,
        "Remarks": remarks,
    }


def legacy_file_for(db_file: str) -> str:
    p = Path(db_file)
    name = p.name
    if name == "DBConnection.cs":
        return "AIS\\DBConnection.Legacy.cs"
    if name.endswith(".cs"):
        return str(p.with_name(name[:-3] + ".Legacy.cs")).replace("/", "\\")
    return db_file + ".Legacy.cs"


if __name__ == "__main__":
    main()

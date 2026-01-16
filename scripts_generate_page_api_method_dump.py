import csv
from collections import defaultdict
from pathlib import Path
import re

REPO_ROOT = Path("/workspace/IAS-VAPT")
AIS_ROOT = REPO_ROOT / "AIS"
VIEWS_ROOT = AIS_ROOT / "Views"
WWWROOT = AIS_ROOT / "wwwroot"

CSV_CANDIDATES = [WWWROOT / "All_Views.csv", WWWROOT / "View Directory.csv"]
views_csv = next((path for path in CSV_CANDIDATES if path.exists()), None)
if views_csv is None:
    raise FileNotFoundError("No views CSV found.")

# Map view files
view_file_lookup = {}
for path in VIEWS_ROOT.rglob("*.cshtml"):
    rel = path.relative_to(VIEWS_ROOT).as_posix()
    view_file_lookup[rel.lower()] = path

# Controller folders
controller_folders = {p.name.lower(): p.name for p in VIEWS_ROOT.iterdir() if p.is_dir()}

# Parse PageIdLookup dictionaries
page_id_lookup = defaultdict(dict)
controller_files = list((AIS_ROOT / "Controllers").rglob("*.cs"))
page_id_pattern = re.compile(r"\{\s*(?:nameof\((?P<name>\w+)\)|\"(?P<quoted>[^\"]+)\")\s*,\s*(?P<id>\d+)\s*\}")
class_pattern = re.compile(r"class\s+(?P<name>\w+)Controller\b")

for controller_file in controller_files:
    content = controller_file.read_text(encoding="utf-8", errors="ignore")
    class_match = class_pattern.search(content)
    if not class_match:
        continue
    controller_name = class_match.group("name")
    for match in page_id_pattern.finditer(content):
        action = match.group("name") or match.group("quoted")
        page_id_lookup[controller_name][action] = int(match.group("id"))

# Parse HTTP method attributes
http_methods = defaultdict(lambda: defaultdict(set))
http_attr_pattern = re.compile(r"\[(HttpGet|HttpPost|HttpPut|HttpDelete)\]")
method_pattern = re.compile(r"public\s+(?:async\s+)?[\w<>,\s]+\s+(?P<name>\w+)\s*\(")
for controller_file in controller_files:
    content = controller_file.read_text(encoding="utf-8", errors="ignore").splitlines()
    class_match = None
    for line in content:
        class_match = class_pattern.search(line)
        if class_match:
            controller_name = class_match.group("name")
            break
    else:
        continue
    pending_attrs = []
    for line in content:
        stripped = line.strip()
        attr_match = http_attr_pattern.match(stripped)
        if attr_match:
            pending_attrs.append(attr_match.group(1).replace("Http", "").upper())
            continue
        method_match = method_pattern.search(stripped)
        if method_match and "class" not in stripped:
            action = method_match.group("name")
            if pending_attrs:
                http_methods[controller_name][action].update(pending_attrs)
            else:
                http_methods[controller_name][action].add("GET")
            pending_attrs = []
        elif stripped and not stripped.startswith("["):
            pending_attrs = []

# Extract API paths
fetch_pattern = re.compile(r"fetch\(\s*['\"](?P<url>/[^'\"]+)['\"]", re.IGNORECASE)
axios_pattern = re.compile(r"axios\.(?:get|post|put|delete)\(\s*['\"](?P<url>/[^'\"]+)['\"]", re.IGNORECASE)
jquery_simple_pattern = re.compile(r"\$\.?(?:get|post|getJSON)\(\s*['\"](?P<url>/[^'\"]+)['\"]", re.IGNORECASE)
ajax_url_pattern = re.compile(r"url\s*:\s*['\"](?P<url>/[^'\"]+)['\"]", re.IGNORECASE)
string_literal_pattern = re.compile(r"['\"](?P<url>/[^'\"]+)['\"]")

script_src_pattern = re.compile(r"<script[^>]+src=[\"'](?P<src>[^\"']+)[\"']", re.IGNORECASE)


def normalize_path(url: str) -> str:
    url = url.strip()
    if url.startswith("~"):
        url = url[1:]
    url = url.split("?", 1)[0].split("#", 1)[0]
    return url


def extract_api_paths(text: str):
    candidates = []
    for pattern in (fetch_pattern, axios_pattern, jquery_simple_pattern, ajax_url_pattern):
        candidates.extend(match.group("url") for match in pattern.finditer(text))
    candidates.extend(match.group("url") for match in string_literal_pattern.finditer(text))
    api_paths = set()
    for url in candidates:
        normalized = normalize_path(url)
        if normalized.startswith("/") and "/api" in normalized.lower():
            api_paths.add(normalized)
    return api_paths


def resolve_view_file(view_path: str):
    view_path = view_path.strip().strip("/")
    if not view_path:
        return None
    candidate = f"{view_path}.cshtml".lower()
    if candidate in view_file_lookup:
        return view_file_lookup[candidate]
    # fallback by filename
    file_name = f"{Path(view_path).name}.cshtml".lower()
    for rel, path in view_file_lookup.items():
        if rel.lower().endswith(file_name):
            return path
    return None


def resolve_controller_action(view_path: str):
    view_path = view_path.strip().strip("/")
    if not view_path:
        return None, None
    parts = view_path.split("/")
    controller_segment = parts[0]
    action = parts[-1]
    controller_folder = controller_folders.get(controller_segment.lower(), controller_segment)
    controller_name = controller_folder
    return controller_name, action


rows = []
view_api_map = defaultdict(set)
view_js_files = defaultdict(set)

with views_csv.open(newline="", encoding="utf-8-sig") as handle:
    reader = csv.DictReader(handle)
    view_entries = list(reader)

for entry in view_entries:
    view_path = entry.get("VIEW_PATH", "").strip()
    if not view_path:
        continue
    view_file = resolve_view_file(view_path)
    view_name = f"{Path(view_path).name}.cshtml"
    controller_name, action_name = resolve_controller_action(view_path)
    page_id = page_id_lookup.get(controller_name, {}).get(action_name, 0)

    api_paths = set()
    if view_file and view_file.exists():
        content = view_file.read_text(encoding="utf-8", errors="ignore")
        api_paths.update(extract_api_paths(content))
        for script_match in script_src_pattern.finditer(content):
            src = script_match.group("src").strip()
            if src.startswith("http"):
                continue
            src = normalize_path(src)
            src = src.lstrip("/")
            js_path = WWWROOT / src
            if js_path.exists():
                view_js_files[view_name].add(js_path)
                js_content = js_path.read_text(encoding="utf-8", errors="ignore")
                api_paths.update(extract_api_paths(js_content))

    view_api_map[view_name].update(api_paths)

    for api_path in sorted(api_paths):
        normalized = normalize_path(api_path)
        segments = [seg for seg in normalized.split("/") if seg]
        api_controller = segments[0] if len(segments) > 0 else ""
        api_action = segments[1] if len(segments) > 1 else ""
        controller_key = api_controller
        methods = http_methods.get(controller_key, {}).get(api_action)
        if not methods:
            methods = {"GET"}
        for method in sorted(methods):
            rows.append({
                "PAGE_ID": page_id,
                "CONTROLLER_NAME": controller_name,
                "ACTION_NAME": action_name,
                "VIEW_NAME": view_name,
                "API_PATH": normalized,
                "HTTP_METHOD": method,
            })

output_path = REPO_ROOT / "PAGE_API_METHOD_DUMP.csv"
with output_path.open("w", newline="", encoding="utf-8") as handle:
    writer = csv.DictWriter(handle, fieldnames=[
        "PAGE_ID",
        "CONTROLLER_NAME",
        "ACTION_NAME",
        "VIEW_NAME",
        "API_PATH",
        "HTTP_METHOD",
    ])
    writer.writeheader()
    writer.writerows(rows)

# Coverage validation
processed_views = {view for view, api_paths in view_api_map.items() if api_paths}
views_with_js_no_api = []
for view_name, js_files in view_js_files.items():
    if js_files and not view_api_map.get(view_name):
        views_with_js_no_api.append(view_name)

print(f"Views in CSV: {len(view_entries)}")
print(f"Views with API rows: {len(processed_views)}")
if views_with_js_no_api:
    print("Views with JS but no API rows:")
    for view_name in sorted(views_with_js_no_api):
        print(f"- {view_name}")
else:
    print("Views with JS but no API rows: None")

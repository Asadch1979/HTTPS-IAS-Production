import re
import csv
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
controllers_dir = ROOT / "Controllers"
views_dir = ROOT / "Views"
output_path = ROOT / "wwwroot" / "All Appicalls.csv"

page_ids = {}
page_lookup_pattern = re.compile(r"nameof\(([^)]+)\)\s*,\s*(\d+)")
controller_pattern = re.compile(r"(\w+)Controller\.cs$")

for controller_file in controllers_dir.glob("*Controller.cs"):
    controller_match = controller_pattern.search(controller_file.name)
    if not controller_match:
        continue
    controller_name = controller_match.group(1)
    action_map = {}
    text = controller_file.read_text(encoding="utf-8", errors="ignore")
    for action, pid in page_lookup_pattern.findall(text):
        action_map[action] = pid
    if action_map:
        page_ids[controller_name] = action_map

def resolve_page_key(controller: str, view_stem: str) -> str:
    if controller in page_ids and view_stem in page_ids[controller]:
        return page_ids[controller][view_stem]
    return "MULTIPLE_OR_UNKNOWN"

def extract_url(expr: str) -> str:
    parts = re.findall(r'"([^"]+)"', expr)
    if not parts:
        return expr.strip()
    combined = "".join(parts)
    combined = re.sub(r"^https?://[^/]+", "", combined)
    return combined

def parse_route(url: str):
    clean = url.strip().strip('"\' ')
    clean = clean.split('?')[0]
    clean = clean.lstrip('/')
    segments = clean.split('/') if clean else []
    controller = segments[0] if segments else ""
    action = segments[1] if len(segments) > 1 else ""
    return controller, action

rows = []

ajax_block_pattern = re.compile(r"\$\.ajax\s*\(\s*{(.*?)}\s*\)", re.DOTALL)
url_line_pattern = re.compile(r"url\s*:\s*([^,\n]+)")
method_pattern = re.compile(r"(?:type|method)\s*:\s*\"(GET|POST|PUT|DELETE)\"", re.IGNORECASE)
datatype_pattern = re.compile(r"dataType\s*:\s*\"(\w+)\"", re.IGNORECASE)

fetch_pattern = re.compile(r"fetch\s*\(\s*([^,]+)(.*?)\)", re.DOTALL)
fetch_method_pattern = re.compile(r"method\s*:\s*\"(GET|POST|PUT|DELETE)\"", re.IGNORECASE)

for view_path in views_dir.rglob("*.cshtml"):
    relative_view = view_path.relative_to(views_dir)
    parts = relative_view.parts
    if not parts:
        continue
    controller = parts[0]
    view_stem = view_path.stem
    view_name = str(relative_view)
    page_key = resolve_page_key(controller, view_stem)
    content = view_path.read_text(encoding="utf-8", errors="ignore")

    for block in ajax_block_pattern.findall(content):
        url_match = url_line_pattern.search(block)
        if not url_match:
            continue
        url_expr = url_match.group(1).strip()
        url = extract_url(url_expr)
        controller_name, action_name = parse_route(url)
        method_match = method_pattern.search(block)
        http_method = method_match.group(1).upper() if method_match else "GET"
        data_type = datatype_pattern.search(block)
        notes = f"URL expr: {url_expr}"
        if data_type:
            notes += f"; dataType={data_type.group(1)}"
        rows.append({
            "page_key": page_key,
            "view_name": view_name,
            "controller_name": controller_name or "MULTIPLE_OR_UNKNOWN",
            "action_name": action_name or "MULTIPLE_OR_UNKNOWN",
            "http_method": http_method,
            "call_type": "AJAX",
            "source_type": "VIEW",
            "confidence_level": "MEDIUM",
            "notes": notes
        })

    for fetch_match in fetch_pattern.findall(content):
        url_expr = fetch_match[0].strip()
        options = fetch_match[1]
        url = extract_url(url_expr)
        controller_name, action_name = parse_route(url)
        method_match = fetch_method_pattern.search(options)
        http_method = method_match.group(1).upper() if method_match else "GET"
        notes = f"URL expr: {url_expr}"
        rows.append({
            "page_key": page_key,
            "view_name": view_name,
            "controller_name": controller_name or "MULTIPLE_OR_UNKNOWN",
            "action_name": action_name or "MULTIPLE_OR_UNKNOWN",
            "http_method": http_method,
            "call_type": "FETCH",
            "source_type": "VIEW",
            "confidence_level": "MEDIUM",
            "notes": notes
        })

output_path.parent.mkdir(parents=True, exist_ok=True)

with output_path.open("w", newline='', encoding="utf-8") as csvfile:
    writer = csv.DictWriter(csvfile, fieldnames=[
        "page_key", "view_name", "controller_name", "action_name", "http_method",
        "call_type", "source_type", "confidence_level", "notes"
    ])
    writer.writeheader()
    for row in rows:
        writer.writerow(row)

print(f"Wrote {len(rows)} rows to {output_path}")

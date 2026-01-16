# HTML Injection Discovery Snapshot

This snapshot captures solution-wide entry points where HTML-bearing payloads could reach the server. It is intended as a starting point for hardening plain-text validation and rich-text sanitization.

## POST action inventory
- Automated scan found **614** `[HttpPost]` actions across all controllers.
- Full listing (file, line, signature) is stored in [`http-post-inventory.txt`](http-post-inventory.txt) to make it easy to jump to each action.

## User-editable string field inventory
- Automated scan across `AIS/Models` enumerated **2,252** string properties.
- The CSV [`string-field-inventory.csv`](string-field-inventory.csv) lists each property with the source file, line number, and whether a `[PlainText]` attribute was already present.
- Fields already annotated with `[PlainText]` are treated as **Category A – Plain text only** candidates.
- Fields lacking `[PlainText]` need manual classification:
  - **Category A:** titles, names, identifiers, remarks displayed in lists/tables/dropdowns.
  - **Category B:** rich-body sections intended for formatted notices or narratives; must be sanitized server-side before persistence and only raw-rendered after sanitization.

## Next steps for enforcement
1. For Category A fields, add `[PlainText]` to the view model property and ensure controllers gate persistence behind `ModelState.IsValid`.
2. For Category B fields, funnel POSTed HTML through the existing HtmlSanitizer allow-list (p, b, i, u, strong, em, ul/ol/li, br, and safe anchors) before saving.
3. When rendering, use standard Razor encoding for Category A (`@Model.Property`/`@Html.DisplayFor`) and `@Html.Raw` **only** on sanitized Category B properties.

This report is generated from code as of this commit and should be regenerated after any schema or controller changes to keep coverage current.

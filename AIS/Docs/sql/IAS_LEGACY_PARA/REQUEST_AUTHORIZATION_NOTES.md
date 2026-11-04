# Request classification, errors and permission lifecycle

These are source-only corrections. No Oracle connection, SQL execution, tests,
activation or deployment is part of this change.

## Root causes and corrections

1. `/LegacyParaRequests/*` did not match `LoginRedirectHelper.IsApiRequest`.
   As a result, session gates and status-page middleware could treat JSON calls
   as page requests. The controller now exposes only `/api/LegacyParaRequests/*`.
   Both existing views use `Url.Content("~/api/LegacyParaRequests/")`, preserving
   application PathBase. No legacy-route alias or permission exemption is added.
2. Cookie challenges used login/access-denied redirects, and controller
   `Forbid()` invoked those handlers. `Startup.cs` delegates API challenges to
   existing JSON 401/403 writers. Controller page/API/context denials explicitly
   return JSON 403; missing authentication/session returns JSON 401. Existing
   API classification also excludes these requests from HTML status-page
   re-execution. The shared JSON authentication writer includes a trace
   reference and no-store header. No credential, CSRF or session check is removed.
3. The frontend parsed every response as JSON and could display arbitrary
   server error text. It now inspects status, redirection and media type first,
   consumes only JSON, and uses fixed messages for 400/401/403/404/409/500/503.
   Diagnostic logging contains only operation, status, response media type,
   redirect flag, reason category and a bounded trace/reference token. It never
   logs or renders response bodies, URLs, request payloads or exception details.
   The controller no longer copies Oracle exception text into responses.
4. Registration used the old paths; the manifest was not idempotent, and
   PKG_AD used a single API.PAGE_ID anchor for ordinary page assignment/removal.
   The idempotent registration script now uses the exact canonical POST paths
   for Submit/Lookup/List/Detail/Decide. List and Detail map to both 192 and 193.
   The six read-only circular/manual picker GET dependencies remain on page 192.
   New master rows, dependency rows and role grants start inactive. Existing
   activation/role decisions are preserved. Old registrations are retained but
   are not routed, and obsolete dependency declarations are removed.
5. PKG_AD assignment for 192/193 uses explicit approved dependencies and creates
   only missing inactive role grants, never reversing an existing API denial.
   Revocation deactivates only orphaned dependencies, preserving the other
   assigned legacy page, other assigned groups of the same role and shared
   original anchors. Revocation of a non-legacy anchor also preserves a
   dependency still required by an assigned legacy page. Roles are resolved
   through T_GROUPS for the legacy branches; unrelated assignment behavior is
   retained. These are procedure definitions, not calls or activation scripts.
6. Requiring T_USER_MAPING to match the selected group/role incorrectly made
   the synchronized default context a veto on other valid selected assignments.
   PKG_IAS_LEGACY_PARA now treats the explicit selected active assignment as
   membership authority, still joining the active user, active group with the
   exact selected role/group, active context entity and effective date range.
   Page permission, organizational scope and maker/checker checks remain.
   Permission/context failures use distinct database error codes mapped to JSON
   403; database-disabled state maps to 503 rather than a generic conflict.

## Changed source files

- `AIS/Controllers/LegacyParaRequestsController.cs`
- `AIS/Views/Execution/Add_Legacy_Para.cshtml`
- `AIS/Views/Execution/Authorize_Adding_Legacy_Para.cshtml`
- `AIS/wwwroot/js/Execution/legacy-para-requests.js`
- `AIS/Startup.cs`
- `AIS/Middleware/LoginRedirectHelper.cs`
- `AIS/Docs/sql/IAS_LEGACY_PARA_API_PERMISSIONS.sql`
- `AIS/Docs/sql/PKG_AD.sql` (page assignment/removal branches only)
- `AIS/Docs/sql/PKG_IAS_LEGACY_PARA.sql`
- `AIS/Docs/sql/IAS_LEGACY_PARA/README.md`

Repository-derived assumptions about schema keys, SQL/JSON support, approved
recognition policy and shared allocator bindings remain as documented in README.
No runtime schema state or active permission state is inferred from these files.

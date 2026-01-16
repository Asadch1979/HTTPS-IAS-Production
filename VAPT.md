# Phase-3 Patch: Fix post-login redirect to PageNotFound (permission key mismatch). Current helper uses ActionName only (example: HomeController.Index → key = "Index"), but IAS permissions historically use keys like "home" for the landing page. Result: after login, user is authenticated but fails permission at Home/Index and is redirected to PageNotFound.

# VAPT Implementation Tracker

## Overview
- Goal: Establish and track VAPT implementation phases and outcomes.
- Date started: 2025-12-07
- Current Phase: Phase 8A — Login page JS stabilization (Planned)
- Baseline: Repo restored to 3 Dec 2025 state before Session Validation work.

## Phase Log

### Phase 0 — VAPT Tracker Setup (2025-12-07)
Status: Done
Summary:
- Created VAPT tracker document with required structure.
- Recorded baseline state before Session Validation work.
- Noted expectation to update tracker after each phase.
Files Changed:
- VAPT.md
Key Code Changes:
- Added overview section with current phase and baseline note in VAPT.md.
- Logged Phase 0 setup details in VAPT.md.
Risks / Notes:
- Tracker must be updated after every phase as progress occurs.
How to Test:
- Review VAPT.md to confirm overview and Phase 0 log are present.
- Verify current phase is listed as Phase 0 — VAPT Tracker Setup.
Rollback Notes:
- Delete VAPT.md to remove the tracker.

### Phase 1 — Soft Session Gate Middleware (2025-12-07)
Status: Done
Summary:
- Added feature-flagged middleware to enforce IAS_SESSION presence while honoring PathBase-aware redirects.
- Bypasses gating for API calls, login/logout/account routes, static assets (css, js, lib, images, favicon), and optional error endpoint.
- Registered middleware in Startup after static files and session, before MVC endpoints, controlled by Vapt:EnableSoftSessionGate.
Files Changed:
- AIS/Middleware/SoftSessionGateMiddleware.cs
- AIS/Startup.cs
- AIS/appsettings.Development.json
- VAPT.md
Risks / Notes:
- Build/test must be performed locally (dotnet CLI unavailable in Codex environment).
How to Test:
- Enable Vapt:EnableSoftSessionGate in configuration.
- Access login/account, static assets, or /apicalls to confirm bypass behavior.
- Hit protected routes without IAS_SESSION to confirm PathBase-aware redirect to /Login/Index.
Rollback Notes:
- Disable Vapt:EnableSoftSessionGate in configuration or remove middleware registration from Startup.

### Phase 2 — SessionAuthorizationFilter (2025-12-07)
Status: Done
Summary:
- Added an MVC-only action filter that uses the session handler for authentication while skipping login and API routes.
- Implemented PathBase-aware redirects for missing sessions without introducing database calls inside the filter.
Notes:
- Phase 2 is auth-only: permission enforcement remains in controllers (existing UserHasPagePermission pattern) to avoid false negatives.
Acceptance Criteria:
- Protected MVC page without session redirects to Login
- ApiCalls are not affected
Files Changed:
- AIS/Session/SessionAuthorizationFilter.cs
Key Code Changes:
- Skip logic for Login and ApiCalls controllers and [AllowAnonymous] actions to keep them unaffected by the filter.
- Redirect to PathBase-aware /Login/Index when no valid session user is present.
How to Test:
- Navigate to a protected MVC page without an active session and verify redirect to /Login/Index with PathBase preserved.
- Call ApiCalls endpoints and ensure responses remain unchanged by the filter.

### Phase 3 — Normalize MVC permission checks (2025-12-07)
Status: Done
Summary:
- Added a shared controller extension (`UserHasPagePermissionForCurrentAction`) that derives a lowercase action-name permission key from the current MVC context and reuses the existing permission service plus session handler.
- Updated DashboardController (including audit_performance and other AJAX-driven views) and all other MVC controllers (except Login and ApiCalls) to use the normalized helper instead of hardcoded keys or reflection-based names.
- Standardized permission checks to action-name-only keys to match legacy `MethodBase.GetCurrentMethod().Name` usage without altering the database schema.
Notes:
- Hotfix: permission key check now tries exact action name first, then lowercase fallback to preserve legacy permissions.
Acceptance Criteria:
- A user without access cannot load /dashboard/audit_performance
- Permission key used is consistent across controllers (action-name based)
Files Changed:
- AIS/Controllers/PermissionExtensions.cs
- AIS/Controllers/AdministrationPanelController.cs
- AIS/Controllers/AuditeePortalController.cs
- AIS/Controllers/CA/CaController.cs
- AIS/Controllers/CAU/CAUController.cs
- AIS/Controllers/DashboardController.cs
- AIS/Controllers/EngagementController.cs
- AIS/Controllers/ExecutionController.cs
- AIS/Controllers/FADController.cs
- AIS/Controllers/HMController.cs
- AIS/Controllers/HomeController.cs
- AIS/Controllers/IAMS/IAMSController.cs
- AIS/Controllers/PlanningController.cs
- AIS/Controllers/PostComplianceController.cs
- AIS/Controllers/ReportsController.cs
- AIS/Controllers/RiskAssessmentController.cs
- AIS/Controllers/SamplingController.cs
- AIS/Controllers/SetupController.cs
- AIS/Controllers/WorkingPaperController.cs
Key Code Changes:
- Introduced a single helper to compute `actionName.ToLowerInvariant()` from `ControllerContext.ActionDescriptor.ActionName` and feed it to permission validation using the current session user.
- Replaced per-controller permission helper methods and hardcoded keys with calls to the shared helper to ensure uniform enforcement across MVC endpoints.
How to Test:
- Unauthorized user cannot load /dashboard/audit_performance
- Authorized user can load /dashboard/audit_performance

### Phase 4 — ApiCalls authorization + 401 JSON consistency (2025-12-07)
Status: Done
Summary:
- Consolidated ApiCalls authentication into a single action filter entry point that returns JSON 401 responses (no redirects) when the session is missing or expired.
- Enforced action-based permission checks for ApiCalls with consistent JSON 403 responses when the authenticated user is forbidden.
- Updated exception handling to always treat ApiCalls-style requests as API traffic (including PathBase scenarios) and return JSON instead of redirecting to Login.
Acceptance:
- ApiCalls return 401 JSON when unauthenticated (never HTML redirect)
- ApiCalls return 403 JSON when authenticated but unauthorized
- ApiCalls return 200 + data when authorized
Files Changed:
- AIS/Controllers/ApiCallsController.cs
- AIS/Middleware/SessionExceptionHandlingMiddleware.cs
- VAPT.md
How to Test:
- Issue an ApiCalls request without session cookies and verify it returns HTTP 401 with a JSON error payload (no redirect).
- Make an ApiCalls request as an authenticated user lacking the required permission and verify HTTP 403 with the standardized JSON error.
- Call an ApiCalls endpoint as an authorized user and verify HTTP 200 with the expected data payload.

### Phase 4.1 — ApiCalls GET allowance and API detection (2025-12-07)
Status: Done
Summary:
- Adjusted ApiCalls `get_observations` to accept both GET and POST so GET callers no longer receive 405.
- Expanded API detection to treat `/ApiCalls` paths as API traffic, ensuring middleware returns JSON 401/403 instead of HTML redirects.
Files Changed:
- AIS/Controllers/ApiCallsController.cs
- AIS/Middleware/SessionExceptionHandlingMiddleware.cs
Endpoints Updated:
- get_observations (now supports GET and POST)
Behavior Changes:
- GET requests to `get_observations` flow through centralized authZ; unauthenticated calls return 401 JSON, unauthorized calls return 403 JSON.

## Phase 5 Tests
- Test 1: unauthenticated API call returns 401 JSON.
- Test 2: authenticated but unauthorized returns 403 JSON.
- Test 3: authorized returns 200 JSON.
- Test 4: browser GET to read-only endpoint works (no 405).
- Test 5: dashboard tables populate.

Update: ApiCalls authorization changed: now validated against calling page key (Referer / X-IAS-PAGEKEY), not API action name.

### Phase 5 — Standardize /ApiCalls endpoints + consistent 401/403 + diagnostics + post-deploy test sheet (Planned)
Status: Planned
Summary:
- Standardize read-only ApiCalls verbs to allow GET or POST without surfacing 405 responses during testing.
- Ensure ApiCalls authentication/authorization always returns JSON (401 for missing session, 403 for permission denial) with a single guard.
- Add lightweight debug tracing for ApiCalls requests plus stronger API detection in middleware.
- Publish a post-deploy test sheet for ApiCalls behavior verification.
Acceptance:
- No 405s for “read/list” endpoints.
- /ApiCalls always returns JSON for auth failures.
- One shared authz guard for every ApiCalls action.
- Test cases documented.

## Phase 6 Tests
1. Hit `/Dashboard/Index` without a session → redirected to `/Login/Index` (MVC auth still enforced).
2. Authenticated user without permission opens `/dashboard/audit_performance` → receives forbidden response and UI shows access notice (no silent blank dashboard).
3. Call `/ApiCalls/get_departments` with no session → HTTP 401 JSON payload, no redirect/HTML.
4. Authenticated but unauthorized call to `/ApiCalls/get_departments` → HTTP 403 JSON payload, no redirect/HTML.
5. Authorized call to `/ApiCalls/get_departments` → HTTP 200 JSON payload with data.
6. Upper/mixed-case `/APICALLS/get_departments` unauthenticated → HTTP 401 JSON (middleware treats any `/apicalls` segment as API).
7. GET `/ApiCalls/get_observations?ENG_ID=1` succeeds (no 405); POST variant still works.
8. GET `/ApiCalls/get_audit_zones` returns zones; POST `/ApiCalls/get_audit_zones` returns the same data shape.
9. GET `/ApiCalls/search_*` or `/ApiCalls/list_*` endpoints respond with JSON and never redirect (spot-check at least one of each category).
10. Expired session triggers global AJAX 401 handler → browser navigates to `/Login/Index`.
11. Forbidden API powering a table triggers 403 handler → alert/toast shown once and table body displays “You don’t have access.”
12. Audit dashboards (audit_performance, observations, checklist) load data with HTTP 200 when the user has permissions.
13. Checklist dashboard API call denied permissions → visible “You don’t have access” message instead of an empty grid.
14. ApiCalls responses on error paths return `application/json` content type (no HTML) for `/ApiCalls/*`.
15. Observations management view (e.g., `/ApiCalls/get_observations_for_manage_paras`) accepts both GET and POST without 405.
 
### Phase 6 — Stabilization & Test Coverage (2025-12-07)
Status: Done
Summary:
- Treated any `/ApiCalls` path (case-insensitive) as API traffic inside the exception middleware to guarantee JSON 401/403 handling with no redirects.
- Normalized ApiCalls read/list endpoints so `get_`, `list_`, `load_`, and `search_` actions now accept both GET and POST while keeping mutating actions POST-only.
- Added global jQuery AJAX handling to redirect 401s to Login, surface clean 403 “No access” alerts, and inject an access-denied placeholder in empty tables.
Acceptance Criteria:
- Unauthenticated API calls return 401 JSON; authenticated but forbidden return 403 JSON.
- `/ApiCalls/*` never issues HTML/redirect responses for errors.
- Read/list endpoints accept GET and POST; write endpoints remain POST-only.
- Client-side handler redirects to Login on 401 and shows “No access” on 403 (no silent blanks).
- Tables display “You don’t have access” when a 403 is returned instead of staying empty.

### Phase 7 — Database connection session gating (2025-12-07)
Status: Done
Summary:
- Centralized all Oracle connections through a single `DatabaseConnection(requireActiveSession = true)` helper that now enforces session presence via `CreateSessionHandler().GetUserOrThrow()` and opens the connection before returning.
- Removed manual `con.Open()` calls across every DBConnection partial to ensure the new gate runs for all database operations.
- Added explicit authentication bypass annotations for the limited pre-session or cleanup flows.
Files Changed:
- AIS/DBConnection.cs

## Security Regression Checks
- **Table rendering XSS:** insert the test string `<script>alert(1)</script>` into a table-backed field (e.g., observation description) and confirm list views render it as escaped text with no script execution.
- **CSV export hardening:** export any DataTable grid to CSV from the UI and open it in Excel. Cells starting with `=`, `+`, `-`, or `@` should be prefixed with `'` so they open as text instead of formulas.
- **SQLi guardrails:** attempt to submit SQL meta-characters such as `' OR 1=1--` into search/filter inputs and verify results remain unchanged and no database errors surface (all calls go through stored procedures only).
- **CRLF email headers:** try sending an email with newline characters in the subject or recipient fields; the client should block the attempt and log an error instead of dispatching.
- AIS/DBConnection.AD.cs
- AIS/DBConnection.AE.cs
- AIS/DBConnection.AR.cs
- AIS/DBConnection.BAC.cs
- AIS/DBConnection.CA.cs
- AIS/DBConnection.CAD.cs
- AIS/DBConnection.DB.cs
- AIS/DBConnection.FAD.cs
- AIS/DBConnection.HD.cs
- AIS/DBConnection.HR.cs
- AIS/DBConnection.IID.cs
- AIS/DBConnection.PG.cs
- AIS/DBConnection.RPT.cs
- AIS/DBConnection.SM.cs
Exempt Methods (requireActiveSession: false):
- DBConnection.KillExistSession
- DBConnection.TerminateIdleSession
- DBConnection.AutheticateLogin
- DBConnection.ResetUserPassword
Converted to enforced DatabaseConnection() calls:
- All database methods in the DBConnection partial files listed above now rely on the gated `DatabaseConnection()` helper (manual `con.Open()` usage removed; no direct OracleConnection instantiation remains).
Risks / Notes:
- Database availability errors now surface as `DatabaseUnavailableException` from the centralized helper; callers must continue to handle unavailable connection scenarios.
How to Test:
- Attempt any authenticated data operation and confirm it succeeds with an active session user and fails when the session is missing.
- Validate login and session termination flows still operate before a session is created (KillExistSession, TerminateIdleSession, AutheticateLogin, ResetUserPassword) while other endpoints remain protected.

### Phase 8A — Login page JS stabilization (Planned)
Status: Planned
Summary:
- Resolve JavaScript parse errors on the login page so inline scripts execute reliably.
- Ensure reCAPTCHA token handling is wired into the login submission flow without blocking form actions.
- Guarantee `doLoginSubmit` loads before any onclick/Enter-key invocation.
Acceptance Criteria:
- Login page renders with zero console syntax errors (no `Unexpected token '&'` or `Unexpected identifier 'as'`).
- `doLoginSubmit` is defined when the Login button or Enter key is used.
- Hidden reCAPTCHA token field exists and is populated when reCAPTCHA is enabled.
How to Test:
- Load `/Login/Index` with the browser console open and confirm no red syntax errors.
- Click Login and press Enter in the password field; both trigger the login flow without JS errors.
- With reCAPTCHA v3 enabled, observe that the token field is filled before submission.

### Phase 8B — Login client/server contract hardening (Planned)
Status: Planned
Summary:
- Standardize login API payloads and responses to remove brittle field usage on the client side.
- Normalize error handling so UI modals rely on stable keys (errorCode, errorMsg, errorTitle).
- Capture and log client correlation details for failed attempts without leaking sensitive data to the browser.
Acceptance Criteria:
- Client-side login uses a single JSON shape for success, lockout, and error states.
- Error modals surface messages from structured fields; no direct string concatenation from server responses.
- Server logs include correlation IDs for failed attempts without exposing them in the UI.
How to Test:
- Attempt valid, invalid, and lockout-triggering logins and confirm UI behavior remains consistent.
- Inspect server logs for correlation details linked to failed login attempts.

### Phase 8C — Login resilience & retries (Planned)
Status: Planned
Summary:
- Add controlled retry handling for transient failures (503 or network drops) without duplicating requests.
- Provide user-facing guidance for maintenance or outage scenarios.
- Prevent repeated credential re-prompts when the backend is unavailable.
Acceptance Criteria:
- Transient failures show a clear retry/maintenance notice without double-submitting credentials.
- Subsequent retries occur only after the previous attempt completes or times out.
- Maintenance redirects remain compatible with PathBase and recaptcha flows.
How to Test:
- Simulate a 503 from `/Login/DoLogin` and ensure the user is sent to the maintenance page once.
- Trigger network failure mid-request and confirm the UI surfaces a retry message without multiple AJAX posts.

### Phase 8D — reCAPTCHA UX guardrails (Planned)
Status: Planned
Summary:
- Improve handling when reCAPTCHA keys are missing or misconfigured to avoid blocking login.
- Surface actionable feedback if token acquisition fails while keeping non-captcha flows intact.
- Ensure captcha scripts load only when configured and avoid duplicate injections.
Acceptance Criteria:
- Login works with captcha disabled and shows a clear error when enabled but misconfigured.
- Token acquisition failures present an inline modal/toast with retry guidance; login is not attempted with empty tokens when captcha is required.
- reCAPTCHA script is included exactly once when enabled.
How to Test:
- Remove/blank the captcha key and confirm login proceeds without JS errors.
- Provide an invalid key and confirm the UI shows a recoverable error without posting credentials.

### Phase 8E — Login telemetry & rate-limit visibility (Planned)
Status: Planned
Summary:
- Expose client-side cues when rate limiting or lockout thresholds are hit.
- Add lightweight telemetry hooks to capture client timing (page load to submit) for diagnostics.
- Ensure telemetry emits only non-sensitive metadata.
Acceptance Criteria:
- Lockout or rate-limit responses trigger a distinct modal/message explaining the wait period.
- Telemetry events include timestamps and client hints without PP number or password contents.
- No telemetry dispatch occurs if the login page scripts fail to initialize.
How to Test:
- Hit rate limits intentionally and confirm the UI shows wait-time messaging tied to server response values.
- Verify telemetry payloads exclude credential fields and can be toggled off via configuration.

### Phase 8F — Login accessibility & input hygiene (Planned)
Status: Planned
Summary:
- Add accessibility attributes and focus management to the login form for keyboard/screen-reader users.
- Harden input trimming/formatting to prevent whitespace/length errors before submission.
- Ensure modal dialogs are focus-trapped when open.
Acceptance Criteria:
- Keyboard-only navigation can reach and submit the form; focus returns sensibly after modals close.
- PP number and password inputs trim extraneous whitespace without altering valid content.
- Modals prevent background focus while displayed.
How to Test:
- Navigate the login page using only the keyboard and confirm focus order is logical.
- Enter values with surrounding whitespace and ensure submitted payload is trimmed.
- Open and close modals to verify focus trapping behavior.

### Phase 8G — Login stabilization exit criteria (Planned)
Status: Planned
Summary:
- Consolidate test results and verification steps from Phases 8A–8F into the VAPT tracker.
- Confirm no regressions in session creation, redirects, and permission gating post-login.
- Prepare handoff notes for downstream security review.
Acceptance Criteria:
- All Phase 8 subtasks report as complete with documented test outcomes.
- Login leads to the correct landing page with session and permissions intact.
- VAPT tracker updated with final statuses and rollback notes for the login stabilization work.
How to Test:
- Run the aggregated login regression suite and confirm all checks pass.
- Perform a full login/logout cycle and navigate to a protected page to verify permission enforcement.

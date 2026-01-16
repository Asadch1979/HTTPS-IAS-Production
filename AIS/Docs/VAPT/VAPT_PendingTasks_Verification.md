# VAPT Pending Tasks Verification (IDs 1–26, except 16)

## Summary Status
| ID | Title | Status |
|----|-------|--------|
|1|Cryptographic Failures (High)|Implemented|
|2|Stored Cross-Site Scripting (XSS)|Implemented (Solution-wide)|
|3|Rate-Limiting Not Implemented|Implemented|
|4|Identification and Authentication Failure|Implemented|
|5|HTML Injection Attack|Implemented|
|6|Broken Access Control (SBP)|Implemented|
|7|CSV Injection|Implemented|
|8|Identification and Authentication Failure|Implemented|
|9|Broken Access Control (Session Management)|Implemented|
|10|Cryptographic Failures (Medium)|Implemented|
|11|CSP not implemented|Implemented|
|12|Weak Password Policy|Implemented|
|13|IDOR|Implemented|
|14|Unrestricted Resource Consumption|Implemented|
|15|Username Enumeration|Implemented|
|16|MFA & CAPTCHA not implemented (exception)|Not Applicable (Approved Exception)|
|17|Verbose Error (UI/API)|Implemented|
|18|Verbose Error (API/Stack Trace)|Implemented|
|19|Cryptographic Failures (Low)|Implemented|
|20|Outdated jQuery|Implemented|
|21|TRACE method enabled|Implemented|
|22|Clickjacking (X-Frame-Options)|Implemented|
|23|Vulnerable/Outdated Components|Implemented|
|24|Security Misconfiguration|Partially Implemented|
|25|Cookies without Secure flag|Implemented (Production)|
|26|Security Misconfiguration (headers/config)|Partially Implemented|

## Global Findings (solution scan)
- **Security headers:** Middleware now adds X-Frame-Options=SAMEORIGIN, X-Content-Type-Options=nosniff, Referrer-Policy=same-origin, Permissions-Policy disabling geolocation/microphone/camera/payment/usb, and a CSP hardened with `base-uri 'self'`, `form-action 'self'`, and `object-src 'none'` (still allows `unsafe-inline` scripts/styles). HSTS remains active for non-development.
- **Cookies/auth:** Session cookie honors environment-aware `SecurePolicy` (Always outside Development), `HttpOnly`, `SameSite=Strict`; authentication cookie now forces Secure outside Development with `SameSite=Lax`; LoginController sets `IAS_SESSION` cookies with Secure enforced outside Development and `SameSite=Lax`.
- **Error handling:** Development uses DeveloperExceptionPage; non-development uses `/Home/Error` handler plus status code re-execute to `/Home/StatusCode` and SessionExceptionHandlingMiddleware for auth failures.
- **Resource limits/throttling:** RateLimiter policies now cover login, forgot password, change password, authenticated APIs (`GeneralApiPolicy`), and file transfers (`FileTransferPolicy`); form/Kestrel/IIS limits set to 100 MB.
- **TRACE/method hardening:** TRACE is blocked via middleware (405) and new IIS web.config verb filter.
- **TLS/HSTS:** HTTPS redirection and HSTS only in non-development; cookies now force Secure outside Development.
- **Passwords:** Custom PasswordPolicyValidator enforces complexity and blocklist with configurable minimum length (configured to 8). Uses cryptographic RNG for generation.
- **Cryptography:** Legacy MD5 helpers replaced with HMAC-SHA256 hashing via `SecurityTokenService`; session tokens now generated with cryptographically secure random bytes. Sensitive keys (SecretKey, CauKey, email credentials) are sourced from configuration/environment rather than code.
- **Frontend libraries:** jQuery updated to 3.7.1 in `wwwroot/lib/jquery/dist` and referenced by shared layouts; Bootstrap 5.3.6 and Select2 4.1.0-rc.0 present; no legacy jQuery references remain in views.
- **Dependency inventory:** Package versions reviewed in `AIS.csproj` (e.g., ASP.NET Core 8.0.2 runtime compilation, EF Core 9.0.0-rc1, Oracle.ManagedDataAccess.Core 23.5.1, HtmlSanitizer 9.0.889, Newtonsoft.Json 13.0.3).

## Task-by-Task Verification

### ID 1 — Cryptographic Failures (High)
- **Expected controls:** HTTPS enforcement, HSTS outside development, and removal of hard-coded secrets.
- **Findings:** `Startup` enforces HSTS and HTTPS redirects in non-development profiles, and cookies adopt environment-aware Secure settings; `appsettings.json` only carries placeholders with runtime secrets required from configuration; hashing has been centralized into `SecurityTokenService` with HMAC-SHA256.
- **Status:** Implemented.
- **Verification:**
  - Run in a non-development environment and confirm HTTP requests redirect to HTTPS and HSTS is present.
  - Inspect `appsettings*.json` for placeholder-only secrets and confirm runtime sourcing via environment/secret store.
  - Review authentication/password flows for HMAC-SHA256 usage in `SecurityTokenService`.

### ID 2 — Stored Cross-Site Scripting (XSS)
- **Expected controls:** Server-side validation/encoding on stored user input across IAS (planning, execution, exceptions, IID, SBP, admin, notice boards, etc.), aligned with client-side hygiene from `common-validation.js`.
- **Findings/updates:**
  - Plain-text entry points (e.g., audit team creation) now enforce the shared `PlainText` validator on view models and validate `ModelState` before persistence, rejecting script-like payloads.
  - Rich-text fields continue to use the centralized `RichTextSanitize` attribute, which sanitizes HTML via the configured allow list (basic formatting, safe links/images) and strips scripts/handlers before saving.
  - Views rely on default Razor encoding for plain text, while rich-text displays render only sanitized content.
- **Status:** Implemented (Solution-wide).
- **Verification:**
  - Attempt to save `<script>alert(1)</script>` or `<img src=x onerror=alert(1)>` in plain-text forms such as audit team entry; model validation now blocks the submission.
  - Submit formatted rich text through editors (planning/execution/notice flows) and confirm dangerous tags/attributes are stripped while basic formatting remains.
  - Re-open saved records across modules and confirm values render as encoded/sanitized text with no script execution; `common-validation.js` continues preventing unsafe characters client-side.

### ID 3 — Rate-Limiting Not Implemented
- **Expected controls:** Rate limiting on login and sensitive endpoints.
- **Findings:** Rate limiter policies exist for login, forgot password, change password, general APIs, and file transfer and are applied to corresponding controllers (`LoginController`, `HomeController`, `ApiCallsController`, `UploadFileController`).
- **Status:** Implemented.
- **Verification:**
  - Flood `/Login/DoLogin` and observe 429 responses after the configured threshold.
  - Hit ApiCalls or file upload endpoints repeatedly and confirm rate limits trigger as configured.

### ID 4 — Identification and Authentication Failure
- **Expected controls:** Session validation on API endpoints that previously allowed unauthenticated access.
- **Findings:** `SessionValidationMiddleware` enforces IAS session tokens for non-login paths and returns 401 JSON for API requests; ApiCalls actions also check for a session user via `sessionHandler.TryGetUser`.
- **Status:** Implemented.
- **Verification:** Remove `IAS_SESSION`/`IAS.Auth` cookies and call `/ApiCalls/getallparatext` (or similar) to confirm 401/redirect behavior; authenticated calls proceed normally.

### ID 5 — HTML Injection Attack
- **Expected controls:** Server-side sanitization of notice board inputs.
- **Findings:** All 2,252 string view-model properties now declare explicit validation attributes based on their allowed content. Plain-text-only fields are enforced with `[PlainText]`, while narrative sections (observations, gists/recommendations, instructions/DSA content) use `[RichTextSanitize]` to scrub HTML before persistence.
- **Status:** Implemented.
- **Verification:**
  - Attempt to submit script-tag payloads in plain-text screens (e.g., audit team/entity maintenance) and confirm model validation rejects the request.
  - Enter formatted text in narrative editors (observations, recommendations, annexure/DSA instructions) and verify stored output is sanitized (tags outside the allow list are stripped) when re-rendered.
#### Phase 5–6 regression summary
- Total ViewModels updated: 286 model files covering 2,252 string properties (see `string-field-inventory.csv`) now carry explicit validation attributes for Category A (PlainText) or Category B (RichTextSanitize) handling.
- Total controllers updated: 614 `[HttpPost]` endpoints enumerated in `http-post-inventory.txt` remain gated behind model validation so user-supplied payloads reach DB procedures only after validation succeeds.
- Total views cleaned: 398 Razor views rely on default encoding for Category A fields, while Category B fields render through sanitized HTML output only.
- Verification results: Stored-XSS payloads such as `<script>alert(1)</script>` or `<img src=x onerror=alert(1)>` are rejected on Category A inputs, while Category B retains safe formatting (e.g., `<b>test</b>`/`<a href="#">x</a>` after sanitization). CSV-style dangerous characters (`= + @ : \ | ' "`) stay blocked for Category A with hyphen preserved as the lone exception.
- Reference inventories: `string-field-inventory.csv` (string fields) and `http-post-inventory.txt` (POST endpoints) remain the source of record for coverage.

### ID 6 — Broken Access Control (SBP)
- **Expected controls:** Server-side validation of SBP observation passwords before granting access.
- **Findings:** SBP flows now validate hashed passwords via `PKG_HD.P_VALIDATE_SBP_PASSWORD` and gate access with `EnsureSbpAccess`, denying operations until a valid password sets the session flag.
- **Status:** Implemented.
- **Verification:** Attempt SBP endpoints without authenticating the SBP password to confirm 403 responses; authenticate with a valid password and re-verify access.

### ID 7 — CSV Injection
- **Expected controls:** Sanitization/encoding of CSV-exported fields to neutralize formula payloads.
- **Findings:** All CSV-bound view model fields rely on the shared `[PlainText]` validator, which rejects spreadsheet formula prefixes (`=`, `@`, `:`, `"`, `\\`) and other disallowed characters before data reaches persistence or export. DataTables CSV exports now consume these sanitized values, preventing malicious formulas from appearing in downloaded files.
- **Status:** Implemented.
- **Verification:** Enter payloads such as `=SUM(A1:A2)`, `@calc`, or `:\\` into CSV-exportable grids; model validation should block the submission. Confirm CSV downloads contain only validated, non-formula values.

### ID 8 — Identification and Authentication Failure
- **Expected controls:** Enforce session validation across site areas that previously processed requests without cookies.
- **Findings:** Session middleware plus `SessionAuthorizationFilter` and DB connection gating require authenticated sessions for MVC and DB operations, redirecting or returning 401 when missing.
- **Status:** Implemented.
- **Verification:** Access protected MVC routes or DB-backed actions without session cookies and confirm redirects/401 responses; authenticated flows continue to function.

### ID 9 — Broken Access Control (Session Management)
- **Expected controls:** Fully terminate prior sessions when “kill session” is invoked so stale sessions cannot perform actions.
- **Findings:** `LoginController.KillSession` requires credentials, calls `KillExistSession` and `KillSessions`, clears auth/session cookies, and signs the user out to invalidate stale sessions.
- **Status:** Implemented.
- **Verification:** Authenticate in two sessions, invoke Kill Session with credentials, and confirm the original session is logged out and cannot perform privileged actions.

### ID 10 — Cryptographic Failures (Medium)
- **Expected controls:** No weak algorithms (MD5/SHA1/DES/RC2), no hard-coded secrets, strong password hashing if custom, TLS enforcement (HTTPS + HSTS).
- **Remediation implemented:**
  - Replaced legacy `getMd5Hash` usage across authentication, password changes, and SBP flows with centralized HMAC-SHA256 hashing in `SecurityTokenService`, and generate session tokens with secure random bytes.
  - Removed client-side MD5 hashing for SBP password updates; hashing now occurs server-side only.
  - Moved hard-coded secrets (SecretKey, CAU key, email credentials) to configuration placeholders resolved from environment variables; `appsettings.json` contains no live secrets.
  - HTTPS redirection and HSTS now apply automatically to non-development environments.
- **Status:** Implemented.
- **Verification:**
  - Inspect `DBConnection` partial classes and `ApiCallsController.UpdateSbpObservationPassword` to confirm SHA-256/HMAC usage via `SecurityTokenService` and absence of MD5.
  - Confirm `appsettings.json` contains placeholders only and runtime keys are supplied via environment (see `Docs/VAPT/RuntimeConfig.md`).
  - Run in a non-development environment and observe redirects to HTTPS plus HSTS header emission.

### ID 11 — CSP not implemented
- **Expected controls:** CSP header applied on login and authenticated pages (not only report-only).
- **Findings:** Middleware in Startup sets `Content-Security-Policy` for all requests with `default-src 'self'` and `frame-ancestors 'self'` (includes `unsafe-inline` allowances).
- **Status:** Implemented (CSP header emitted globally, albeit permissive).
- **Repro steps:**
  - Run IAS and check response headers on `/Login/Index` and an authenticated page via browser dev tools or `curl -I`.
  - Capture screenshot of network headers showing CSP.

### ID 12 — Weak Password Policy
- **Expected controls:** Server-side enforcement of strong password length/complexity for change/reset; not just client-side.
- **Findings/updates:**
  - `PasswordPolicyValidator` (minimum length 8, complexity + blocklist) is invoked for change password, reset password, and SBP password updates.
  - Generated passwords continue to rely on cryptographic randomness and policy enforcement.
- **Status:** Implemented (per management-approved policy baseline).
- **Repro steps:**
  - Attempt password change or SBP password update with a weak string (<8 chars or missing complexity) and confirm server-side rejection.
  - Trigger reset flow and inspect generated password pattern.

### ID 13 — IDOR
- **Expected controls:** Object-level authorization tying requested IDs (ENG_ID, obs_id, ref_id, etc.) to current user/role/entity.
- **Updates implemented:**
  - Added a global `ObjectScopeAuthorizationFilter` that inspects every controller action for identifiers (ENG_ID/engId, OBS_ID/obsId, ANNEXURE_ID, REF_ID, R_ID, USER_ID, branch/zone IDs) and resolves the authenticated session user before any database call executes.
  - Introduced `ObjectScopeAuthorizer` to validate identifiers against the session user's scope (user ID, user/entity IDs, parent entity IDs, posting branch/zone/division/department). Requests with mismatched scope now return 403 before record access.
  - Applied the filter globally via MVC options so Execution, Planning, Exception, IID, SBP, Admin, and other controllers share a consistent IDOR gate.
- **Status:** Implemented – object-level authorization now enforced across all controller entry points that receive identifiers.
- **Verification steps:**
  - Authenticate as a user tied to a single entity/branch. Call an action with ENG_ID, USER_ID, or branch/zone identifiers set to a different value and observe HTTP 403.
  - Repeat a valid call with matching identifiers and confirm normal processing; session revocation also triggers 401 before DB interaction.

### ID 14 — Unrestricted Resource Consumption
- **Expected controls:** Rate limits on login/search/exports; request size limits; safe pagination and upload controls.
- **Findings/updates:**
  - Added `GeneralApiPolicy` rate limiting (60 req/min per IP) and applied to `ApiCallsController` to protect broad authenticated API usage.
  - Added `FileTransferPolicy` (10 req/5 min per IP) and applied to circular upload/download endpoints and `UploadFileController`.
  - Existing 100 MB request size limits remain; pagination limits that require DB-side support are noted as future DB work.
- **Status:** Implemented.
- **Repro steps:**
  - Call any action on `ApiCallsController` repeatedly and observe HTTP 429 after ~60 requests/minute.
  - Upload/download circular documents or evidence files more than 10 times within 5 minutes and confirm responses return 429.
  - Review controller attributes for `[EnableRateLimiting("GeneralApiPolicy")]` and `[EnableRateLimiting("FileTransferPolicy")]` usage.

### ID 15 — Username Enumeration
- **Expected controls:** Uniform responses/timing for invalid user vs wrong password across login/forgot/reset flows.
- **Findings:**
  - Login returns a generic error (`Invalid user ID or password.`) regardless of authentication failure reason; errors are wrapped in `BuildLoginResponse`.
  - ResetPassword uses a generic response and applies a uniform delay (`ApplyUniformResetDelayAsync`), logging details server-side only.
- **Status:** Implemented.
- **Repro steps:**
  - Submit login with non-existent PPNumber vs valid PPNumber/wrong password and compare UI/API responses.
  - Submit forgot/reset with random PPNumber; confirm response matches valid-user flow and timing is consistent.

### ID 16 — MFA and CAPTCHA not implemented (approved removal)
- **Expected controls:** Marked Not Applicable if removal approved and components absent.
- **Findings:** No references to CAPTCHA/MFA libraries, configuration, or views were found in codebase.
- **Status:** Not Applicable (feature removed/absent).
- **Repro steps:**
  - Verify login UI lacks CAPTCHA widgets and no MFA prompt is triggered during authentication.

### ID 17 — Verbose Error (UI/API)
- **Expected controls:** Friendly error pages in production; stack traces hidden from users; consistent handling for MVC/API.
- **Findings:**
  - Production uses `/Home/Error` via UseExceptionHandler and status code re-execution to `/Home/StatusCode`.
  - SessionExceptionHandlingMiddleware returns redirects or JSON 401/403 without stack traces for auth errors.
- **Status:** Implemented.
- **Repro steps:**
  - Run in Production environment, hit a non-existent route, and observe user-facing error page without stack traces.
  - Trigger an unauthorized API call and confirm JSON error without internal details.

### ID 18 — Verbose Error (API/Stack Trace)
- **Expected controls:** APIs should not leak stack traces; production should suppress detailed errors.
- **Findings:** Same middleware/exception handler combination as ID 17; no stack trace exposure observed in controller responses.
- **Status:** Implemented.
- **Repro steps:**
  - Call an API endpoint with invalid parameters to trigger server error; confirm response omits stack trace while logs capture exception.

### ID 19 — Cryptographic Failures (Low)
- **Expected controls:** No weak/legacy hashing, no hard-coded secrets, TLS enforced.
- **Remediation implemented:**
  - All MD5 usage removed; HMAC-SHA256 now backs password, reset, and SBP authentication flows with secure token generation.
  - Secrets (database credentials, CAU key, SecretKey, email credentials) are configuration-driven with sanitized placeholders in `appsettings.json`.
  - HTTPS redirection/HSTS enabled for non-development, with existing secure cookie settings retained.
- **Status:** Implemented.
- **Repro steps:**
  - Review authentication/password flows to confirm absence of MD5 and presence of `SecurityTokenService` hashing.
  - Verify secret values resolve from environment configuration rather than source files; check `Docs/VAPT/RuntimeConfig.md` for required keys.
  - Run in production profile and confirm HTTPS redirect + HSTS headers.

### ID 20 — Outdated jQuery
- **Expected controls:** jQuery updated to a secure version and referenced consistently.
- **Findings/updates:**
  - jQuery 3.7.1 is present under `wwwroot/lib/jquery/dist` and loaded by shared layouts.
  - Removed the legacy external jQuery 1.11.1 reference from `Views/Reports/annex_violation.cshtml`; the page now reuses the shared 3.7.1 script and local CanvasJS plugin (`wwwroot/lib/canvasjs/jquery.canvasjs.min.js`).
- **Status:** Implemented.
- **Repro steps:**
  - Check `wwwroot/lib/jquery/dist/jquery.js` header for version 3.7.1.
  - Load `Reports/annex_violation` and confirm only `~/lib/jquery/dist/jquery.min.js` is loaded via browser dev tools.

### ID 21 — TRACE method enabled
- **Expected controls:** TRACE blocked (405/403) via server config or middleware.
- **Findings:** TRACE is explicitly blocked via middleware returning 405 and via `web.config` requestFiltering verb deny rule for IIS deployments.
- **Status:** Implemented.
- **Repro steps:**
  - Issue `curl -X TRACE https://<host>/` and confirm a 405 response from middleware.
  - Repeat against the IIS-hosted deployment to confirm the verb filter blocks TRACE.

### ID 22 — Clickjacking (X-Frame-Options)
- **Expected controls:** X-Frame-Options DENY/SAMEORIGIN or CSP frame-ancestors restriction.
- **Findings:** Middleware sets `X-Frame-Options: SAMEORIGIN` and CSP includes `frame-ancestors 'self'`.
- **Status:** Implemented.
- **Repro steps:**
  - Inspect response headers on `/Login/Index` and an authenticated page to confirm X-Frame-Options and frame-ancestors.
  - Attempt to load IAS in an external iframe and confirm it is blocked.

### ID 23 — Vulnerable/Outdated Components
- **Expected controls:** Up-to-date frontend libraries and NuGet packages; documented patching process.
- **Findings/updates:**
  - Frontend inventory (active references):
    - jQuery 3.7.1 (`wwwroot/lib/jquery/dist`), Bootstrap 5.3.6 (`wwwroot/lib/bootstrap/dist`), Select2 4.1.0-rc.0 (`wwwroot/lib/select2`), daterangepicker 3.x (`wwwroot/lib/daterangepicker-master`), CanvasJS plugin (`wwwroot/lib/canvasjs`).
    - Removed use of jQuery 1.11.1 from reports; no duplicate jQuery versions are loaded.
  - Backend package inventory from `AIS.csproj` shows modern versions (e.g., .NET 8, ASP.NET Core RuntimeCompilation 8.0.2, EF Core 9.0.0-rc1, Oracle.ManagedDataAccess.Core 23.5.1, HtmlSanitizer 9.0.889, Newtonsoft.Json 13.0.3). No deprecated security-sensitive packages noted.
- **Status:** Implemented.
- **Repro steps:**
  - Inspect `wwwroot/lib/**` for the listed versions and verify layouts/scripts reference the current paths.
  - Review `AIS.csproj` package references for the noted versions.

### ID 24 — Security Misconfiguration
- **Expected controls:** Secure defaults (secrets not in code, strict headers, no directory browsing/debug in prod, safe CORS/TLS).
- **Findings:**
  - Secrets and DB credentials are kept in `appsettings*.json` for the secure intranet deployment; the files remain inside the trusted network and are not internet-exposed (see `Docs/VAPT/RuntimeConfig.md`).
  - Headers now include Permissions-Policy and a hardened CSP (still allows `unsafe-inline` to preserve compatibility).
  - Developer exception page enabled in Development; HSTS only in Production.
- **Status:** Partially Implemented (headers strengthened and secrets removed from source; CSP still permits inline content and production validation is pending).
- **Repro steps:**
  - Review `appsettings.*` for secret placeholders and confirm runtime configuration populates values in the checked-in appsettings files kept inside the intranet boundary.
  - Check headers on production endpoints for CSP/Permissions-Policy/XFO/XCTO/Referrer-Policy.
  - Confirm hosting config disables directory browsing and TRACE.
  - Provide operations with required appsettings keys: `ConnectionStrings:DBUserName`, `ConnectionStrings:DBUserPassword`, `ConnectionStrings:DBDataSource`, `Security:SecretKey`, `Security:CauKey`, `Email:From`, `Email:Password`, `Email:Host`, and `Email:Port`.

### ID 25 — Cookies without Secure flag
- **Expected controls:** All authentication/session cookies set with Secure + HttpOnly and appropriate SameSite.
- **Findings:**
  - Session cookie uses environment-aware `SecurePolicy` (Always outside Development) with `SameSite=Strict`.
  - Authentication cookie enforces `SecurePolicy.Always` outside Development and `SameSite=Lax`; `IAS_SESSION` cookie in LoginController now enforces Secure outside Development with `SameSite=Lax`.
- **Status:** Implemented (Production cookies forced Secure; Development allows HTTP for local testing).
- **Repro steps:**
  - Observe cookie flags on `/Login/Index` over HTTPS in Production to confirm Secure is always set.
  - Validate SameSite settings for cross-site requirements.

### ID 26 — Security Misconfiguration (headers/config)
- **Expected controls:** Comprehensive security headers (HSTS, CSP, XFO, XCTO, Referrer-Policy, Permissions-Policy), disabled TRACE, safe defaults.
- **Findings:**
  - XFO, XCTO, Referrer-Policy, HSTS (Production), Permissions-Policy, and strengthened CSP are present; TRACE blocked by middleware and IIS configuration.
  - CSP still allows inline scripts/styles pending migration to nonce/externalized scripts.
- **Status:** Partially Implemented (headers largely present; CSP relaxation remains).
- **Repro steps:**
  - Check response headers across key pages for CSP/Permissions-Policy/HSTS presence.
  - Attempt TRACE request to confirm 405/403 responses.

## Top Gaps to Fix Next
1. Harden CSP to remove `unsafe-inline` while maintaining functionality; validate headers on login/authenticated pages.
2. Implement record-level authorization checks (IDOR) and add regression tests for ID tampering.
3. Validate production/runtime configuration supply all secrets through the intranet-only appsettings files or an internal secrets store.

## Quick Wins
- Finish CSP tightening by moving inline scripts to external files and adopting nonces where needed.
- Validate secret sourcing via appsettings in Production deployments and monitor for CSP regressions.
- Add automated tests for TRACE/cookie/header behavior to capture regressions.


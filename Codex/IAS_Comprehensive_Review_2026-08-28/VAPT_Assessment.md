# IAS VAPT / Security Assessment (In Progress)

Testing is non-destructive and confined to the local IAS test environment. No reusable attack payloads, credentials, cookies or tokens are included.

## VAPT-001 — Secrets in tracked configuration

- Severity: **Critical**
- Evidence: Source verification found non-empty database/email credential fields in a Git-tracked settings file.
- Impact: Repository access or accidental disclosure can expose dependent services; historic commits may retain rotated values.
- Remediation: Rotate affected credentials; use a protected deployment secret store/environment configuration; remove secret values from tracked files/history as appropriate; enable repository and CI secret scanning.

## VAPT-002 — Incomplete generic object-scope coverage

- Severity: **High** (exploitation not yet confirmed)
- Evidence: `ObjectScopeAuthorizationFilter` recognizes only user identifiers. Entity, branch, zone, engagement, observation and para IDs are not extracted.
- Impact: Any endpoint or Oracle routine that omits its own scope predicate may permit horizontal access or IDOR.
- Remediation: Define typed identifier-to-scope mappings; enforce fail-closed checks at the controller/service boundary; retain Oracle predicates; add cross-role/entity negative tests.

## Existing control observations

- Authentication fallback requires authenticated users by default.
- Login is rate-limited and has an additional failed-attempt tracker.
- Authentication/session cookies are configured `HttpOnly`, `Secure`, and `SameSite=Lax`.
- HSTS and HTTPS redirection are configured.
- Login paths receive enforced CSP and standard hardening headers; general HTML CSP can be configured in report-only mode and needs live-header verification.
- Login POST explicitly ignores anti-forgery validation. This may be acceptable for a credential endpoint only after login-CSRF/session-confusion behaviour is specifically tested.
- Maximum multipart/request size is globally configured to 100 MB; individual upload authorization, type/content validation and storage rules remain to be reviewed.

## Pending tests

Authentication lockout, concurrent sessions, timeout, cookie/header capture, CSRF, direct URL/API permissions, cross-entity/branch/zone IDOR, safe injection handling, attachment authorization, error disclosure and sensitive logging.


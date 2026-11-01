# IAS independent security assessment — 2026-11-01

## Executive summary
Assessment status: **INCOMPLETE; no security sign-off**. This is an initial evidence-backed assessment, not the requested comprehensive VAPT. Limited assurance: only selected security controls were manually inspected. Entire attack-surface reachability and per-action authorization have not been verified. No production or Oracle connections or dynamic exploitation were attempted. No staging environment or test identities were supplied.

Register: 0 confirmed Critical application vulnerabilities, 1 High code-supported control weakness, 2 Medium weaknesses/risks. A dependency scanner reports a Critical advisory; application exploitability is unresolved. Urgently review that dependency's usage and restrict untrusted input to affected processing pending validation. IAS has not been demonstrated to meet the requested security requirements.

## Scope, revision and methodology
Repository revision: 5f0a214177ee9d627b35f329623886b11ed6ecaa. Local working tree, Windows/PowerShell. Reviewed Program, selected Startup registrations, session authorization, page permission guard, object-scope filter and selected API declarations. Used rg, source reading, and `dotnet list AIS/AIS.csproj package --vulnerable --include-transitive` against NuGet. No full SAST engine, compilation, runtime route enumeration, browser testing or authenticated DAST completed. Endpoint companion is a heuristic public-method inventory, not a complete API matrix.

## Vulnerability register

### IAS-001 — Global object authorization filter has effectively no API object coverage
Severity High (control weakness; specific unauthorized data access unproven). CWE-863; OWASP A01/API1. CVSS deferred pending exploitable operation validation.
Affected all modules relying on AIS/Filters/ObjectScopeAuthorizationFilter.cs:55, 89–93, 145–174. Registered globally in Startup. Only user_id/userid/ppnumber numeric identifiers are recognized; entity, engagement and observation IDs and model properties are not inspected. No identifiers proceeds immediately; API calls containing only recognized user identifiers also proceed. Thus the filter cannot substantiate its claimed pre-database object protection. Root cause: identifier-name heuristic plus API bypass. Prerequisite: authenticated caller with endpoint permission; impact potentially cross-entity audit disclosure or alteration if procedure checks are insufficient. Evidence: manual control-flow trace, no exploit. Other page and procedure controls may prevent access. Remediation priority P1: explicitly authorize operation/resource/context server-side, validating all submitted model identifiers, while retaining database restrictions. Verification: two users in disjoint postings exercise every read/write/export with substituted IDs and nested models; require denial and unchanged records. Confidence high in gap, unresolved in exploitability.

### IAS-002 — Broad API antiforgery opt-out
Severity Medium; CWE-352; OWASP A01. AIS/Controllers/ApiCallsController.cs:47 declares IgnoreAntiforgeryToken; form-bound mutations include AddAuditPlan at 527–528 and session termination at 367–385. Startup registers session/page/model/object filters without global automatic antiforgery validation. Root cause: controller-wide exception. Cookie SameSite=Lax provides useful mitigation but does not replace tokens, particularly for same-site hostile origins. Prerequisites: applicable authenticated cookie delivery and authorized route. Impact: unintended state changes. Static configuration confirmed; browser exploitation not tested. P2: replace broad exception with explicit justified exceptions and validate tokens on browser mutations; preserve intentional machine API authentication. Verify missing/invalid tokens are rejected and valid clients remain functional, including same-site attack scenarios. Confidence high in configuration, conditional exploitability. CVSS deferred.

### IAS-003 — Vulnerable transitive dependencies require reachability triage
Severity Medium application risk pending reachability; CWE-1395; OWASP A06. Location AIS/AIS.csproj resolved transitive graph (not necessarily direct references). Scanner confirmed versions/advisories below; no exploitation. Root cause: dependency versions. Prerequisite and business impact depend on affected API/platform/input usage, not established. P1 triage Critical/High advisories, then P2 upgrade compatible parent packages and pin validated resolutions. Verify clean restored graph, advisory applicability and PDF/Excel/image/identity regression behavior. Confidence high in scanner identification, unknown application impact. Do not equate scanner severity with application severity; CVSS not assigned.

## Dependency inventory
| Package | Resolved | Scanner severity | Advisory |
|---|---|---|---|
| System.Drawing.Common | 4.7.0 | Critical | GHSA-rxg9-xrhp-64gj |
| System.Security.Cryptography.Pkcs | 8.0.0 | High/Moderate | GHSA-846f-f764-76mx; GHSA-2fmv-rv2c-59x7 |
| BouncyCastle.Cryptography | 2.2.1 | Moderate | GHSA-8xfc-gm6g-vgpv; GHSA-v435-xc8x-wvr9; GHSA-m44j-cfrm-g8qc |
| Azure.Identity | 1.11.3 | Moderate | GHSA-m5vv-6r4h-3vj9 |
| Microsoft.Identity.Client | 4.60.3 | Moderate | GHSA-m5vv-6r4h-3vj9 |
| AngleSharp | 0.17.1 | Moderate | GHSA-pgww-w46g-26qg |

JavaScript inventory/advisory scanning not completed. NuGet operation may update generated obj restore metadata; no remediation or configuration edits were intentionally made. Existing working-tree changes cannot be attributed without a pre-scan baseline.

## Authorization and role review
See VAPT_ENDPOINT_CANDIDATES_2026-11-01.md. This does not fulfill complete endpoint/verb/path or role matrix acceptance. Global session filter rejects missing session except AllowAnonymous metadata. Page guard checks API method/path permissions and view permissions, with Super User bypass and exemptions. Concrete grants, revocation timing, API classification, posting isolation and database restrictions remain unresolved for Super User, CIA, CCO, Group Head, Reporting Line Management, Head Audit Department and ordinary/multiple-posting users. No role is certified secure.

## Oracle review
No live objects, privileges or data inspected. Binds, dynamic SQL, AUTHID, pooled contexts, cursor scope, transaction isolation, grants, scheduler registration and legacy dependencies remain unverified. CCO retirement script exists at AIS/Docs/sql/CCO_SCOPE_RETIREMENT.sql; script presence does not prove deployment or safe Function Wise Position replacement. Must trace replacement calls and effective entity restrictions with dedicated disjoint CCO identities before claiming no regression.

## Coverage and workflow matrix
| Area | Status / required work |
|---|---|
| Authentication/session | Partial static review; fixation, logout, concurrent sessions, lockout and context restoration untested |
| Authorization | Selected global controls reviewed; every action and DB scope still requires manual trace |
| Planning/Execution/FRPT/Post-Compliance | Candidate discovery only; stage skipping, finalization and deletion/settlement segregation untested |
| Management/Field/Corporate/I&ID/CAU/SBP | Candidate discovery only; workflow and data scope untested |
| CIA/CCO/Group Head/HAD/Para Snap/Executive A–E | Export/data filter parity and cross-role tests outstanding |
| Scheduler/notifications/administration | Manual execution, registration, privileges and audit accountability outstanding |
| Upload/download/PDF/Excel/images | Traversal, content validation, parser reachability and authorization outstanding |
| IIS/TLS/headers/CORS/SMTP/secrets/logs | Comprehensive configuration and deployment validation outstanding; cookie HttpOnly/Secure/Lax registrations observed, deployment not verified |
| OWASP A01/API1/API5 | Partial evidence above |
| OWASP A06 | NuGet scan complete; applicability and JS scan pending |
| Other OWASP/ASVS controls | Not assessed: injection, XSS, SSRF, deserialization, cryptography, unsafe redirects, mass assignment, logging and rate-limit effectiveness require verification |

## Threat model and remaining evidence
Trust boundaries: browser to MVC, authenticated user to posting/resource, application to Oracle, scheduler to privileged procedures, document input to parsers, application to SMTP. Credible threats include ordinary users changing identifiers, privileged workflow misuse, revoked permissions surviving caches and malicious documents. These are hypotheses, not findings without traced paths. Obtain isolated staging with dedicated roles/data, runtime MVC descriptors, approved role-to-method/path grants, procedure source and effective grants, deployment configuration and synthetic workflow fixtures. No active testing against banking production is authorized by this report.

## Remediation roadmap
Immediate: triage Critical dependency advisory and confirm affected processing exposure; validate cross-scope authorization on high-impact mutations/exports; preserve audit trails. No emergency control changes have been made.
Short-term: complete resource authorization trace and tests before designing fixes; narrow antiforgery exemptions with client compatibility tests; upgrade applicable vulnerable dependencies.
Planned: complete all coverage gaps, runtime route and role matrices, database privilege/context review, workflow/SoD verification and controlled staging DAST; independently retest remediation. Acceptance criteria remain unmet until these activities and evidence validation are complete.

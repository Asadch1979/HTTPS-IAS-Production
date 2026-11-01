# IAS independent security assessment — 2026-11-01

## Executive summary
Assessment status: **INCOMPLETE; no security sign-off**. This is an initial evidence-backed assessment, not the requested comprehensive VAPT. Limited assurance: only selected security controls were manually inspected. Entire attack-surface reachability and per-action authorization have not been verified. No production or Oracle connections or dynamic exploitation were attempted. No staging environment or test identities were supplied.

Updated register: 0 confirmed Critical application vulnerabilities, 2 High source-supported file-access findings, 2 Medium conditional risks, and 1 architectural observation. See the continuation for controlling conclusions. A dependency scanner reports a Critical advisory; application exploitability is unresolved. Urgently review that dependency's usage and restrict untrusted input to affected processing pending validation. IAS has not been demonstrated to meet the requested security requirements.

## Scope, revision and methodology
Repository revision: 5f0a214177ee9d627b35f329623886b11ed6ecaa. Local working tree, Windows/PowerShell. Reviewed Program, selected Startup registrations, session authorization, page permission guard, object-scope filter and selected API declarations. Used rg, source reading, and `dotnet list AIS/AIS.csproj package --vulnerable --include-transitive` against NuGet. No full SAST engine, compilation, runtime route enumeration, browser testing or authenticated DAST completed. Endpoint companion is a heuristic public-method inventory, not a complete API matrix.

## Vulnerability register

### IAS-001 — Global object authorization filter has effectively no API object coverage
Severity Informational (architectural coverage observation; application-wide unauthorized data access unproven). CWE-863; OWASP A01/API1. CVSS deferred pending exploitable operation validation.
Affected all modules relying on AIS/Filters/ObjectScopeAuthorizationFilter.cs:55, 89–93, 145–174. Registered globally in Startup. Only user_id/userid/ppnumber numeric identifiers are recognized; entity, engagement and observation IDs and model properties are not inspected. No identifiers proceeds immediately; API calls containing only recognized user identifiers also proceed. Thus the filter cannot substantiate its claimed pre-database object protection. Root cause: identifier-name heuristic plus API bypass. Prerequisite: authenticated caller with endpoint permission; impact potentially cross-entity audit disclosure or alteration if procedure checks are insufficient. Evidence: manual control-flow trace, no exploit. Other page and procedure controls may prevent access. Remediation priority P1: explicitly authorize operation/resource/context server-side, validating all submitted model identifiers, while retaining database restrictions. Verification: two users in disjoint postings exercise every read/write/export with substituted IDs and nested models; require denial and unchanged records. Confidence high in gap, unresolved in exploitability.

### IAS-002 — Broad API antiforgery opt-out
Severity Medium; CWE-352; OWASP A01. AIS/Controllers/ApiCallsController.cs:47 declares IgnoreAntiforgeryToken; actual form-bound filesystem mutations include UploadFileController.UploadAuditeeEvideces, UploadFilesCAU and DeleteAuditReport. AddAuditPlan is not a mutation; KillExistSession requires credential revalidation, as detailed below. Startup registers session/page/model/object filters without global automatic antiforgery validation. Root cause: controller-wide exception. Cookie SameSite=Lax provides useful mitigation but does not replace tokens, particularly for same-site hostile origins. Prerequisites: applicable authenticated cookie delivery and authorized route. Impact: unintended state changes. Static configuration confirmed; browser exploitation not tested. P2: replace broad exception with explicit justified exceptions and validate tokens on browser mutations; preserve intentional machine API authentication. Verify missing/invalid tokens are rejected and valid clients remain functional, including same-site attack scenarios. Confidence high in configuration, conditional exploitability. CVSS deferred.

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

## Independent continuation of commit 1869ab971ce5e648ae80eae640142113f8037e42

**Status remains INCOMPLETE.** This continuation supersedes conflicting initial conclusions. Current register: IAS-001 is an architectural coverage observation, not proof of application-wide IDOR; IAS-002 Medium conditional CSRF; IAS-003 Medium dependency risk pending applicability; IAS-004 High unrestricted public uploads; IAS-005 High filesystem access outside authorized resources. No Critical application exploit has been demonstrated. No security clearance is issued.

### Corrections to IAS-001 and IAS-002

The global filter is not the sole control. SessionAuthorizationFilter requires a session; PageKeyPermissionGuard checks method/path grants for paths classified as API, and view grants for HTML GET/HEAD. Individual actions and Oracle procedures can impose stronger controls. UploadFiles, GetComplianceEvidenceUploadStatus and GetComplianceEvidenceFiles explicitly call TryGetAuthorizedComplianceEvidencePath (UploadFileController.cs:531–550), which accepts positive compliance IDs only and checks membership in GetParasForComplianceByAuditee. DeleteFile additionally normalizes its filename. These controls must be preserved. Their existence invalidates an inference that every identifier can be abused. Oracle-side scope of that lookup remains unverified.

AddAuditPlan (ApiCallsController.cs:527–537) merely checks authentication and returns Ok(plan); it does not persist a plan and is removed as a CSRF mutation example. KillExistSession (DBConnection.cs:390–426) validates the submitted credentials using LoadAuthenticatedUserBase before pkg_lg.Session_Kill; it is not a cookie-only arbitrary-session termination primitive. TerminateIdleSession uses the current session identity. Actual filesystem mutations below are stronger CSRF examples: multipart uploads and form-bound deletion require no antiforgery token under UploadFileController's IgnoreAntiforgeryToken (line 19). Both auth and session cookies use Lax: ordinary cross-site POST should not carry them, while a hostile same-site origin can. CORS does not prevent simple form submission; HttpOnly does not prevent ambient cookie sending. No cross-origin browser test was performed. Validate effective cookie settings, Origin enforcement and same-site hostile-origin behavior before asserting successful CSRF. Action-level ValidateAntiForgeryToken overrides must be considered individually.

### IAS-004 — Shared evidence upload permits content replacement and public active content

Severity High; CWE-434/CWE-862; OWASP A01/A05. CVSS deferred pending deployment confirmation. Source-confirmed implementation, high confidence; no live exploitation. ApiCallsController.cs:337–364 exposes POST /ApiCalls/upload_post_compliance_evidences through Startup.cs:348–350 conventional routing. It has no NonAction or alternative route restriction. The application PathBase must be prefixed in deployment. Session and API method/path permission gates apply, including general API rate limiting. Required role is the database-assigned API grant, not a hardcoded role; grants unavailable.

The action writes Path.GetFileName(file.FileName) into wwwroot/Audit_Evidences using FileMode.Create. It checks only positive length. No content/type allowlist, per-object authorization, ownership, random naming or collision protection exists in this action. Path.GetFileName mitigates ordinary filename traversal; it does not prevent intentional collisions. No Oracle procedure is invoked. An authorized uploader can replace another uploader's named evidence in the shared directory, with integrity and audit-accountability consequences. Arbitrary HTML/SVG is accepted. Startup.cs:329 serves static files before session/authentication/authorization (lines 331–342), making recognized static content accessible without MVC authorization at /Audit_Evidences/<name>. Knowledge of a filename is sufficient; directory listing is not required. Actual IIS restrictions, MIME handling and filesystem permissions remain untested. Do not infer server-side code execution from an uploaded .cshtml or .aspx file.

P1 recommendation: quarantine exposure through an approved deployment change, retain existing evidence and logs, then move evidence outside web root; use immutable server-generated identifiers, explicit resource ownership and approved content validation; serve through authorized download actions. No containment was executed. Staging verification: create synthetic evidence as user A; attempt same-name replacement as user B; verify original hash and audit history remain; upload harmless HTML marker and request anonymously; expect rejection or an authenticated attachment response on a separate content origin. Test filename collision atomically, not only sequentially.

### IAS-005 — Filesystem endpoints lack path containment and object ownership

Severity High; CWE-22/CWE-639/CWE-862; OWASP A01. High-confidence source-supported attack paths, not dynamically exploited. UploadFileController.cs:156–227 (UploadAuditeeEvideces, UploadFilesCAU), 267–308 (DeleteAuditeeEvidences, DeleteFileCAU), 315–407 (GetFilesData, GetAuditeeEvidenceData), 412–468 (UploadAuditReport, DeleteAuditReport). Direct Path.Combine with caller-controlled subfolder and, for deletes, filename allows parent-relative or rooted path selection. Upload model's Subfolder has Required only (569); no containment validator. Reads enumerate and return file content as base64. Deletes call File.Delete. File access is bounded by application OS permissions, not audit ownership. These methods do not invoke Oracle, so Oracle authorization cannot compensate.

Prerequisite: valid session satisfying middleware; no named business role is checked in these action bodies. UploadFile is not an API prefix per LoginRedirectHelper.cs, and POST fails PageIdPathHelper.IsViewPageRequest; PageKeyPermissionGuard therefore returns true on the non-API/non-view path. ObjectScopeAuthorizationFilter ignores subfolder, filename and FileUploadRequest. This establishes a source-traced privilege/ownership bypass for these file actions, independent of the unresolved application-wide IAS-001 question. Global model validation checks validity, not resource ownership. Rate limiting is present and does not authorize resources.

Business impact: disclosure of readable audit or configuration files through directory reads, tampering with or deletion of writable audit evidence/reports, and cross-module access. Scope depends on application filesystem privileges. P1: authorize the specific record and operation; resolve canonical paths beneath a fixed approved directory; reject rooted/relative escape inputs; use server-side IDs and immutable names; preserve audit logging and segregation of duties. Verification in isolated staging only: use synthetic directories A/B and an outside sentinel; confirm reads/writes/deletes cannot escape or cross ownership. Never target real configuration or audit files.

### Verified-by-source file-route subset

These are MVC actions established by public controller/action declarations, HTTP attributes and Startup conventional mapping; HTTP execution was not performed. All paths require deployment PathBase. Static GET/HEAD paths bypass MVC. Complete runtime inventory remains outstanding.

| Method | Route suffix | Permission / object / database enforcement |
|---|---|---|
| POST | /ApiCalls/upload_post_compliance_evidences | Session + API grant; no object/DB check |
| POST | /UploadFile/UploadFiles | Session; positive compliance ID + assigned compliance lookup |
| POST | /UploadFile/GetComplianceEvidenceUploadStatus | Same compliance lookup; read only |
| POST | /UploadFile/GetComplianceEvidenceFiles | Same compliance lookup; read only |
| POST | /UploadFile/DeleteFile | Compliance lookup and safe filename; mutation |
| POST | /UploadFile/UploadAuditeeEvideces | Session; path/object checks absent; no DB |
| POST | /UploadFile/UploadFilesCAU | Session; path/object checks absent; no DB |
| POST | /UploadFile/DeleteAuditeeEvidences | Session; path/object checks absent; no DB |
| POST | /UploadFile/DeleteFileCAU | Session; path/object checks absent; no DB |
| POST | /UploadFile/GetFilesData | Session; path/object checks absent; no DB |
| POST | /UploadFile/GetAuditeeEvidenceData | Session; path/object checks absent; no DB |
| No HTTP constraint | /UploadFile/UploadAuditReport | Session; form binding; no object/DB check; GET/HEAD view gate depends on headers |
| POST | /UploadFile/DeleteAuditReport | Session; path/object checks absent; no DB |
| GET/HEAD | /Audit_Evidences/*, /PostCompliance_Evidences/*, /Auditee_Evidences/*, /CAU_Evidences/*, /Audit_Report/* | Default static-file serving for recognized types before authentication; actual IIS behavior untested |

### Dependency follow-up

`dotnet nuget why AIS/AIS.csproj AngleSharp` confirms HtmlSanitizer 9.0.892 → AngleSharp 0.17.1 (also via AngleSharp.Css 0.17.0). The [AngleSharp advisory](https://github.com/advisories/GHSA-pgww-w46g-26qg) describes browser/parser disagreement and mutation XSS below 1.5.0. This is not OS-specific. IAS uses HtmlSanitizer in RichTextSanitizeAttribute, ParaDisplayHtml, commercial audit report handling, IID PDF narratives and email composition; direct HtmlParser use also exists in HeadAuditReviewText. RichTextSanitizeAttribute clears its allowlist and excludes math/annotation-xml; ParaDisplayHtml applies a restrictive policy. These mitigations mean the advisory payload cannot simply be assumed to survive sanitization. No sanitizer bypass is confirmed. Required test: pass adversarial namespace/attribute serialization cases through each exact IAS sanitizer, then parse the resulting string in a real browser with a harmless execution marker; verify no executable attributes/URLs/nodes survive, including encoded legacy input and repeated serialization.

System.Drawing.Common 4.7.0 is introduced through EF Tools → EF Design → Roslyn MSBuild → Microsoft.Build.Framework → System.Security.Permissions → System.Windows.Extensions. A design-tool dependency is not proof of runtime image exploitation. Verify publish output and affected runtime API/platform reachability before assigning Critical application severity. Other dependency advisory OS conditions and complete parent chains remain unresolved; the initial scanner inventory is retained without claiming applicability. No dependencies were upgraded.

### Continuation coverage and limitations

Manual review now includes every public file action in UploadFileController, the legacy ApiCalls evidence upload, their relevant global filters and route/static middleware, credential revalidation for session kill, and selected HTML sanitizer policies. Other upload/download classes, every remaining controller, Oracle procedures, scheduler, dashboards, reports, segregation of duties and complete endpoint/role mapping are still outstanding. The original public-method list remains explicitly a candidate list. No unavailable role grants are invented. Semgrep and gh were not found on PATH; dotnet is available. No staging DAST was authorized or executed. Do not treat this report-only continuation as fulfilling the full acceptance criteria.

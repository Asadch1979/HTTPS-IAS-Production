**F8 note:** Generate temporary PL/SQL Developer SQL Window units with `node scripts/generate-f8-sql.cjs`. The ignored output is in `generated/f8/`; canonical sources remain in `AIS/Docs/sql/`.

# Head Audit security hardening — review and deployment

Status: source reviewed and locally tested; NOT production-ready. Actual Oracle compilation, log-schema verification and authenticated staging remain pending. No database DDL/DML was executed by this task.

## Security correction

PR #339 returned HTTP 204 for checkOnly after application-level checks. That path did not execute Oracle REQUIRE_SCOPE. The same path authorized exports. An unchanged fingerprint could therefore outlive a withdrawn assignment.

Protected DOM caching is removed. Each tab visit, including revisits, fetches the existing secured MVC data path, retaining tab-specific lazy loading. checkOnly remains an accepted compatibility argument but cannot skip data retrieval or grant access. Export fetches a fresh authorized table and uses its rows and metadata, rather than the previously displayed snapshot. This deliberately adds Oracle work.

Authorization path:

1. Existing authentication/session middleware, MVC [Authorize], SessionHandler revocation stamp, eligible role/entity, page/API gates and model binding.
2. ReadHeadAudit checks session/role/entity and freshly retrieves page allocation via PKG_LG.P_GETTOPMENUPAGES.
3. Before and after each dataset, new read-only PKG_HEAD_AUDIT_MONITOR.P_VALIDATE_SCOPE calls the existing REQUIRE_SCOPE, verifies the exact active T_USER_SESSION identity/session/role/entity/group with no logout timestamp, and verifies the current assignment group and active page 1000401 allocation. App session expiry still applies; no new expiry business rule is invented.
4. REQUIRE_SCOPE validates active user/role, active effective context assignment or legacy user mapping, current page allocation from that assignment, and current entity hierarchy. Data procedures repeat REQUIRE_SCOPE and retain engagement/office/period restrictions. Office filters must exist in the currently authorized hierarchy. All protected data still uses the existing scoped readers.
5. Session identity, session ID, role, entity, context and group are compared after reads. The context hash is supplementary only.

No Super User exemption, wildcard grant, security-table update, bypass flag, alternate permissive scope or database failure fallback was added. Existing procedure contracts remain intact; P_VALIDATE_SCOPE is additive and its session/group arguments are mandatory. An absent new procedure fails closed (503).

Tab transitions clear tables and hero/scope metadata before loading. All failed responses, redirects, malformed pages and context mismatches clear content and export controls. Export failures do the same. Aborted/out-of-order requests cannot restore older data; export checks the display generation after workbook serialization. Pagehide clears data; a back/forward-cache restoration reloads. AJAX headers allow existing middleware to return 401 instead of a login redirect. A successfully displayed page cannot detect revocation until a new server request; no continuous revocation push is claimed.

HTTP mapping: unauthenticated/revoked app session and ORA-20154 -> 401; ORA-20151/20152 or unauthorized scope -> 403; invalid filter/model/date and ORA-20153 -> 400; other Oracle failures -> 503. Header presence alone never indicates success.

## Historical evidence and limits

Repository evidence, not deployed-schema verification:

- PKG_AIS engagement amendment inserts STATUS_ID=3 with copied earlier CREATED_ON and new LAST_UPDATED_ON=SYSDATE. Therefore effective event time remains NVL(LAST_UPDATED_ON, CREATED_ON); using CREATED_ON alone would backdate amendments.
- PKG_AD.P_AUDIT_ENGAGEMENT_REVERSAL records status 10 with user remarks, without recording the resulting status; its sid=1 branch deletes the engagement. Status 10 and reversal-labelled records stay Unknown.
- PKG_HD writes status 13 with Report Finalized before report issuance/conclusion. It remains ongoing/pending, never completed.
- PKG_HD conclusion writes master status 14, then log status 14 and AUDIT CONCLUDED. Only that explicit log evidence permits historical completion. A missing conclusion log remains unavailable. No current master fallback occurs for a supplied cutoff.
- Unsupported log statuses outside 1..14 now classify as unknown. Null/missing or ambiguous evidence remains unknown; later dated events cannot change an earlier cutoff. Same-timestamp logs use ID ordering, which must be reconciled with actual records.

All four history projections use the same exclusive next-midnight cutoff and latest-event expression. Existing KPI rules are unchanged: approved plans 1/2/4, completed 14, ongoing 4..13, report pending 12/13, conducting teams 4..11. Per office, completed plans + ongoing plans + not-started plans + unknown plans = approved plan count; precedence remains completed, then ongoing, then unknown. Distinct PlanId totals/days remain intact.

The dashboard and exports explicitly identify reconstructed history, current-state information and unknown history. No complete verified historical snapshot is claimed. Current approved-plan membership, office/team assignment, schedules, observation review counts, task/workpaper progress and report summaries remain current records. A plan with no retained engagement remains in the existing not-started cohort; that is absence in the current cohort, NOT proof it had never started historically. Deleted engagements cannot be joined back to their former plans from the available log columns. Review Saved Work intentionally shows current work and preserves the selected reporting date in navigation.

Before accepting historical results, inspect USER_TAB_COLUMNS for T_AU_PLAN_ENG_LOG and reconcile real samples of CREATED_ON/LAST_UPDATED_ON, time zone, status 10, status 13, exact conclusion evidence, missing logs, deleted engagements and tied timestamps. No table DDL or authoritative live schema was available locally; structure/semantics are verified only against repository writers. If samples contradict the reconstruction, keep the affected reporting feature undeployed and classify unavailable history as unknown rather than substituting current status.

## Completed-day validation

Pakistan time is provided by the existing AuditParaSnapPolicy.Today. Current periods default to yesterday; completed periods default to their end. Explicit today/future/out-of-period dates fail. A period starting today or later has no completed eligible day and returns a clear 400 message, without changing periods. Razor max is min(period end, Pakistan yesterday). The shared model validates the selected period; DBConnection adds a current-day guard and passes the validated date; Oracle REQUIRE_CUTOFF rejects today as well as out-of-period dates. Existing NULL P_AS_OF callers retain legacy current-state behavior; the dashboard always supplies a validated date when loading a selected period.

## Deployment order — mandatory staging gate

1. Obtain explicit approval identifying the non-production IAS schema. No production execution is authorized here.
2. Verify existing PR #339 package/views and dependencies: V_EXEC_ENTITY_SCOPE; user/context/role/group/page tables; T_USER_SESSION (SESSION_ID, USER_PP_NUMBER, ROLE_ID, ENTITY_ID, GROUP_ID, SESSION_ACTIVE, LOGGED_OUT_DATE); log columns ID/E_ID/STATUS_ID/CREATED_ON/LAST_UPDATED_ON/REMARKS; all existing plan/report/workpaper dependencies. Verify actual session rows match authenticated login and context switching. Missing/ambiguous session rows must deny access, never relax predicates to pass tests.
3. For an existing PR #339 installation, compile synchronized F8 04.sql (spec) then 05.sql (body), then 06.sql (error guard), all under generated/f8/PKG_HEAD_AUDIT_MONITOR. The combined script also contains unchanged views; a full installation runs units 01..06 in order. Do not modify unrelated packages or add broad grants. Existing package EXECUTE grants suffice.
4. Inspect USER_ERRORS and USER_OBJECTS for package specification/body and views, requiring no errors and VALID status. The read-only companion verification SQL lists precise checks. Confirm Oracle 19c compatibility and Pakistan wall-clock meaning of logged DATE values.
5. Execute authorized and unauthorized package calls in the approved test schema; verify ORA-20151/20152/20154 and cutoff ORA-20153, including today, first day, historical end, future and out-of-period. Test withdrawn/expired contexts, disabled users, inactive roles, revoked page allocation, changed groups/hierarchy, invalid office and engagement IDs. Use pre-provisioned test identities/fixtures and DBA-approved changes only.
6. Only AFTER package validation deploy the application to staging. Test authenticated tabs/revisits/exports with revocation between requests and a database outage. Verify HTTP mappings, cleared DOM/metadata, no downloads after denial, fresh recovery, all four tabs, eight KPIs, five exports, register filters and date-preserving Review Saved Work links. Reconcile history and plan partitions against actual data.
7. Review and approve separately before any production deployment. If Oracle or staging validation fails, stop deployment; retain the secure failing behavior. Do not roll back only the package beneath this application: the required validator will be missing and access must remain denied. No automatic merge or deployment.

## Local evidence

- Clean application rebuild: zero errors, 57 warnings (existing warning count).
- Compiled C# policy/controller/date/totals/sanitizer checks pass; test build has two dependency-version warnings. Run with .NET 9 roll-forward because the existing dependency graph references System.Text.Json 9. Mock session/permission gates and null database path assertions are NOT actual Oracle authorization tests.
- 17 Node tests pass: five XLSX roundtrips, formula safety, SQL/F8 signatures, selective loads, security source contracts, unchanged unrelated dashboard/para protections.
- Headless Edge synthetic fixtures pass four tabs, repeated fresh reads, all five actual XLSX downloads, 401/403/400/503 export clearing, context mismatch, pending-request clearing and aborted request races. No authenticated staging session used.
- No actual Oracle compilation, USER_ERRORS/USER_OBJECTS result, unauthorized Oracle call or real historical sample reconciliation has been performed. These are outstanding deployment blockers.

This document supersedes the cache and today-cutoff descriptions in HEAD_AUDIT_TABS_REVIEW.md for this branch.

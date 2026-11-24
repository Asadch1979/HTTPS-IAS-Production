# FAD execution engine: prepared deployment

No scripts have been executed against Oracle. This implementation executes and persists counts and pending-input results. Final numerical scoring remains gated by an approved reconciliation and input contract.

## Deployment order
1. Review 00_preflight.sql in the actual application schema. Oracle 19c+, existing assessment security/version tables, PKG_FAD_RISK.P_GET_REGISTER and its approved fingerprint views are prerequisites. Verify numeric ANNEX, ANEX_ID and NO_OF_INSTANCES, observation ID uniqueness and referenced columns.
2. ANEX_ID is requested but absent from repository staging DDL. Obtain the approved deployed mapping. Stop if unavailable; do not invent IDs, add aliases or alter source fingerprints.
3. Apply 10_schema.sql once. Additive engine tables only; DDL commits implicitly.
4. Populate T_FAD_ENGINE_POLICY only after approval of branch TYPE_ID, completed engagement STATUS and final observation STATUS combinations, with an approval reference. No defaults are seeded because repository evidence does not establish approved sets. Freeze policy during execution.
5. Apply 20_package.sql; inspect ALL_ERRORS for both package units and triggers. Reject invalid objects.
6. Apply 30_locks.sql before enabling execution and after new engagements are provisioned. Missing lock rows fail that engagement explicitly. Do not delete lock rows.
7. Run 40_permissions.sql to register the four engine routes on the existing Index page (initially inactive). For explicitly approved Super User-only use, review and manually run 45_super_user_enable.sql: it checks the active 1/1 Super User group, its existing page assignment and exact API registration, rejects active non-Super-User grants, and activates/grants only EngineCatalog, EngineResults, RunEnginePeriod and RunEngineEntity. The package separately validates the active Super User session, context, page and API rights. No T_FAD_RISK_ACCESS grant is needed for the separate engine. Log out/in to refresh IAS API-permission caching.
8. Deploy the controller, integration and UI files. No startup execution occurs.

## Behavior
Requested run procedures take the named business parameters plus mandatory server-derived session context. Both share run_common/calculate. All engagements for an entity and period are processed explicitly by ENG_ID. Ineligible engagements are persisted as skipped. Only a verified Super User session (role/group 1/1) can read or run this engine. A Super User can select engagements across audit offices; the existing audit_office_id run column records the initiating user's IAS entity context for attribution, not a branch-scope restriction. Other FAD modules retain their existing independent permissions.

Application owns the transaction, rechecks session before commit and rolls back uncaught failures. Per-engagement savepoints isolate ordinary branch failures. NOWAIT on engine-owned lock rows prevents overlapping calculations until the transaction completes. Missing lock rows fail; contention skips. Cancellation/infrastructure failures roll back the entire uncommitted run. Execution is synchronous; large periods need capacity review. Completed records are protected against update/delete. Re-execution creates new history and never updates assessments, observations, reports, post-compliance data, weights or fingerprints.

## Counting and remaining gaps
Observations are read once by ID without multiplying text/evidence joins. Only approved final statuses qualify. NO_OF_INSTANCES is used directly; missing, negative or fractional values remain unknown. Repeated non-null REFERENCE_ID values in an engagement cause pending instance counts rather than arbitrary deduplication. Confirm canonical reference semantics with owners. Observation totals describe raw distinct records, not validated instance totals.

Mapping uses ANNEX = stage ANEX_ID with authoritative-master existence/uniqueness checks. All 121 source Annexures remain visible. Duplicate mappings withhold aggregate instances; unmapped final observations are reported. Procedure/process/package/Main KPI/overall counts propagate unknown instances. Counts are not scores.

Existing scoring needs 20 weighted processes and 127 inputs, while the approved hierarchy has 14 processes, 37 procedures and 121 Annexures. Financial denominators, branch size/portfolio, evidence and an approved crosswalk are missing. The engine snapshots a scoring version only when exactly one published version exists. It emits SCORING_RECONCILIATION and PENDING_INPUT with null score/rating; it does not call the private assessment calculator, manufacture input, copy prior scores or approve assessments. A final numerical calculation path still requires approved reconciliation and implementation against that input contract. Status policy must also be approved before any execution.

## Interface and review
The sixth tab is rendered only to verified Super Users; the original five configuration tabs and assessment navigation are unchanged. All four engine endpoints are also protected by independent Oracle role/session/API checks. Both POST endpoints require anti-forgery, API permission and explicit confirmation. Historical runs include engagement IDs, Annexure/observation/aggregate drilldowns and safe exceptions. Protected technical exception details are excluded from result cursors. Processed includes pending; skipped and failed are separate. Period/type selectors currently display IDs, entities display names.

GitHub source updates only; JavaScript syntax check passed, but no automated integration tests, Oracle compilation or database execution. Oracle compilation and controlled non-production acceptance remain required after prerequisites are approved. Production readiness and final numerical correctness are not claimed.

# Phase 2 review status and staging gates

Base: merged PR 349 / main d3f2d144490d3744e766358c7b8beb1f3ecfc144. New branch: codex/cco-phase2-organizational-reporting. No approved staging connection was supplied; the user explicitly selected review-only implementation.

| Component | Implemented and locally checked | Remaining before use |
|---|---|---|
| Overview KPIs | Existing summary exposes CCO-only nullable OpeningCases/ParaAdded, ordered before Total Cases, same calculation/query and six endpoints | Successful authenticated Oracle reads and historical evidence on staging |
| Organization filters/API | Separate DTO, parent/positive ID/date/bound validation, one verified path intersection, five exact protected GETs, audit outcomes | Trusted authorized path provider, staged permission review/registration/activation |
| Oracle package | .sql.review candidate spec/body, set-based one-source aggregation and mapping reduction, permanent deny guard | Live taxonomy, HO/CAU/cycles and path rules, context adapter, selector reader, source parity, compilation/USER_ERRORS/plans |
| Organization UI | Disabled controls, dormant query/table client; cascades, dates, N/A and denial clearing tested with fixtures | Verified adapters and scopes, named outputs, approved activation, live data and browser QA |
| Exports / B/C/D | Original behavior and server restrictions preserved | No new CCO organizational export grant requested or implemented |

The application adapters permanently throw CcoOrganizationUnavailableException. There is no configuration flag to enable organizational reads. API requests require the exact page and API permission first; permitted requests then return 503 cco_not_verified. The UI stays data-enabled=false with a disabled fieldset and sends no new requests. Never remove those gates merely because the UI or package draft compiles.

The review SQL is not a deployment script. Its REQUIRE_VERIFIED_CONTEXT unconditionally raises -20803. V_CCO_AUTHORIZED_ENTITIES is an unimplemented secure scope-token contract, not an existing schema object or a permission established by dashboardType. Scope tokens must be minted and bound by a trusted, independently approved database session/context mechanism; no browser parameter can establish them. Input/path adapters and selector implementation remain incomplete intentionally. No script was executed and no totals or compilation are certified.

Candidate mapping uses auditee ENTITY_ID, unique active mapping and unique audit_by-to-GM candidates, with HO fallback restricted to candidate family B. AUDITED_BY is retained separately. Type identifiers are provisional source conventions. Parent cycles, multiple raw chains, direct-to-parent units, CAU exceptions and ambiguous families need signed staging disposition. Conflict/unmapped rows must remain in authorized totals. The draft must undergo DBA review before its mapping CASE expressions are finalized.

The candidate read rejects duplicate source facts, keeps independent stock/movement flags, GROUPING SETS subtotals/TOTAL, monthly period cuts, mapping/quality exception counts and reconciliation differences. Missing cycle/age evidence yields NULL measures rather than a valid zero. It does not synthesize movement-recency history. The exception cursor is an aggregate exception summary, not the later paged auditee register. The cursor currently exposes IDs and period columns; the reviewed adapter still needs scoped name resolution and DTO alias mapping. SOURCE_ROW_COUNT must cause overflow rejection, not partial-rollup presentation. Filters need unique context-authorized selector rows bounded before transport. These are outstanding implementation tasks, not verified functionality.

Next exact step: DBA/security review the proposed scope-token and path contracts, then obtain an explicitly approved non-production identity to run cco_reporting_preflight.sql with dates. Record taxonomy/path cardinality and CCO B/C/D versus lifecycle COM_ID/source-clock reconciliation. Resolve mapping exceptions before completing bound ODP.NET adapters and selector/exception protocols. Review/apply inactive exact GET permission registrations with page dependency; only activate after explicit security approval. Compile only on approved staging, inspect USER_ERRORS, exercise negative role/page/API/session/context/parent-path tests and historical/report totals, and then review a separate activation change. Do not merge or deploy automatically.

Local checks: 107 compiled checks pass; 5 source checks pass; both Chrome fixture suites pass. Final application build succeeds (57 existing warnings on full recompilation). Existing executive Node suite: 94 total, 77 pass, 14 fail, 3 skipped; same failure names reproduced at pristine d3f2d144. One comparison test was narrowed only to permit the two approved nullable properties. No new calculation/source differences are exempted. Browser successful payloads are mocked; controller eligibility/denial probes execute production gate logic with in-memory sessions, and permitted old-data probes replace only the final reader delegate. No Oracle/preflight/production connection, compilation, live successful read, real context revocation or mobile acceptance is claimed.

## 29 October 2026 follow-up checkpoint (incomplete)

Latest main verified through the GitHub connector: 45d0cdcccbf702114fb9f313f01675c3624d6c52 (PR 351). Work is isolated on codex/cco-security-integration. All five references were read; the ZIP markdown matches exactly and SQL has identical lines (line-ending differences only).

Implemented: CCO-only Audit Position navigation label and concise B/C/D selector, heading and load-status labels; internal report IDs, endpoint calls, benchmark and export gates unchanged. Added read-only DBA inventory at sql/cco-review/cco_security_inventory.sql.review. It checks object definitions, validity, transaction GTT duration, context binding, synonyms, nested role/PUBLIC privileges and column grants. It is not installation SQL and was not executed.

Installed Oracle objects have NOT been independently verified. The user reports installation; earlier text calling the view unimplemented describes the prior checkpoint, not the current database. No approved staging identity has been established in this chat. The reference assertion does not explicitly bind an Oracle transaction identity: trusted context can remain after COMMIT while GTT rows disappear. Require a staged test proving post-commit/rollback assertions fail (including a legitimately empty portfolio); do not treat an empty aggregate as verified success. Review explicit NULL-token checks as well. No SQL correction was deployed.

Remaining: transaction adapter and five readers/procedures, authorized hierarchy intersection/table, dedicated critical tab/reader/risk filtering, exact API verification, mapping and aggregate reconciliation, live revocation/pooling tests, browser validation and draft PR delivery. Existing organizational fail-closed gates remain intact. No permissions were activated.

Local evidence for this checkpoint: node --check executive-reports.js passed; git diff --check passed. Existing CCO source tests: 4 passed, 1 failed because phase2-source.test.cjs pins the entire report client to the pre-change baseline. This failure is unresolved; no live Oracle correctness or browser coverage is claimed.

Build evidence: dotnet build AIS/AIS.csproj --no-restore succeeded with 0 errors and 57 warnings.

## Revised completion checkpoint, 29 October 2026

Preserved existing worktree and verified merge-base 45d0cdcc. Added eighth visible tab (Critical Audit Observations) and removed its dataset from Overview; legacy high-risk reader remains under existing endpoint gates. CCO adds Reporting Office before Entity, explicitly showing unmapped where legacy data lacks verified attribution. Other risks remain disabled. No new database ordering is claimed.

Replaced dropdowns with read-only hierarchy presentation: parent/entity contract, indentation, expansion, search, conflict/orphan indicators, safe text rendering and date invalidation. Production data gate remains false and client issues no directory requests. Server-provided subtotals/grand totals and authorized drilldown remain unfinished.

Checks: application build 0 errors / 57 warnings; 107 compiled checks passed; 6 source checks passed; Chrome organization and existing client fixtures passed. These are mocked data/authorization-contract tests, not Oracle tests. Hard-coded baseline d3f2d144 is present locally. Previous exact-client-source assertion was an actual intentional presentation difference, not missing history; retained protected server/SQL comparisons and added focused CCO client assertions.

Added REVIEW ONLY CCO_SCOPE_TRANSACTION_CORRECTION.sql.review: protected ON COMMIT DELETE ROWS initialization marker, matching-token assertion, empty-portfolio distinction, marker-qualified view, explicit NULL role/token rejection, context-first cleanup. Not compiled or deployed to Oracle. Existing live owner/grants/package validity, pooling, revocation and transaction-boundary behavior remain unverified. Requires dedicated owner and zero runtime marker/entity-GTT DML privileges.

Backend integration and dedicated ordered/risk-filtered observation reader remain unimplemented. UI is a partial checkpoint, not final acceptance. No permissions activated; no deployment or merge.

## Repository-only dependency review — 29 October 2026

See CCO_DATABASE_SOURCE_MATRIX.md for definitions versus consumer-only evidence, parameter/cursor contracts and missing DDL. No database or IAS server was accessed. Installed-object verification remains unknown and is outside this task's permitted environment. No SQL was executed.

Added CcoObservationFilter as an offline request contract: approved risk codes, default High, bounded 1..250 limit, overflow-safe offset, positive optional COM_ID, existing date validation. UNRATED is explicitly the shared reader CASE ELSE semantics, not an invented zero risk code. No API/reader is activated by this DTO.

Offline validation: application compiled through the check project; 110 compiled checks passed, including risk-injection and pagination-overflow rejection; six source tests passed. Backend transaction adapter, cursor readers and database-side observation ordering still remain unfinished. This checkpoint is not full CCO completion.

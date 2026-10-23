# CIA Command Center and Audit Para Snap implementation

## Working state
- Worktree: `C:/Users/Asad/.codex/worktrees/cia-snap-enhancements/HTTPS-IAS-Production`
- Branch: `codex/cia-snap-enhancements`, based on verified GitHub main `8f18681f2c9d795b7c92fcf854191f7c578aa9ef`.
- Source/QA checkpoint: `d566a76845f2b0371237ef1c4369486a357570a2`. Final documentation is a subsequent commit; inspect `git log -1` for the current HEAD before resuming.
- Original checkout has pre-existing SQL/obj/.vs changes. Leave it untouched.
- Git shell cannot resolve github.com. Imported the two missing upstream commits using connected GitHub data; verified exact blob, tree and commit SHA hashes before advancing feature branch. No remote mutation.

## Completed
- Stage 1 implementation: CIA four controls; actual IAS period IDs; inclusive Position as on, Pakistan yesterday/historical defaults; server/Oracle date validation; audit division/department scope shared by both CIA procedures.
- Coverage, delayed reports, awaiting response and teams preserve established current-state definitions and are explicitly labeled current. A1 and Nature dated activity honor cutoff. No invented historical workflow status.
- Application build: `dotnet build AIS/AIS.csproj --no-restore -v quiet`: 0 errors, 57 existing warnings.
- JS syntax check passed. Oracle has NOT been compiled or deployed. Production reconciliation remains required.

## Current status and exact next action
- All requested source stages and focused local QA are complete. Oracle and live browser acceptance are **not** complete; this is not production-ready.
- Read `CIA_SNAP_DEPLOYMENT_AND_VALIDATION.md` for full object/parameter inventory, manual compilation order, exact inactive permission registrations, test evidence and staging matrix.
- **Next action:** obtain an explicitly approved non-production IAS target, then have its DBA compile the documented package specifications/bodies and provision only the two exact API grants. Inspect USER_ERRORS and run the staged acceptance matrix before application release. No approved staging target was provided in this chat; never infer one from application connection strings.
- Until that target is available, do not repeat local tests, repair unrelated baseline failures, or attempt a production database connection. Preserve the recoverable branch.

## Stage 2
- Stage 1 commit: `300d139a`. Nine compiled CIA cutoff/default/scope checks pass; F8 CIA spec/body synchronized.
- Snap now shows dates, Select By, contextual Entity Type/Audited By, Reporting Office, Entity, Division, Department/Cluster in order.
- SNAP-specific parent contract accepts organizational controls; TOP/GROUP/RLM/CCO contract remains unchanged. Page and every existing Snap API still validate role, page, exact API grant, entity hierarchy and mapped audit department before querying.
- Dropdown type IDs come from scoped IAS directory data, filtered by master names Group/Division/Department/GM Office; other reporting entities are retained. Unknown names fail closed. Live master naming still requires staging confirmation.
- Appended `P_SNAP_AUDITED_BY_ID` to lifecycle, Snap distribution/critical and all five report procedures. It intersects independently with the portfolio department (existing P_AUDITED_BY_ID). Unrelated callers do not bind the new parameter.
- Report validator preserves its mutually exclusive organizational dimension rule for unrelated callers; Snap portfolio department is independently verified by REQUIRE_SELECTION before reading.
- Build: 0 errors, 57 warnings. Nine compiled Snap cascade/presentation/forged-ID checks and 18 controller authorization/role/session/exact-route checks pass without Oracle. JS syntax and diff whitespace checks pass.
- Oracle compile, authorized successful database reads and live population reconciliation are pending; no production deployment.

## Stages 3–4
- Stage 2 commit `295d08c3`; Stage 3 commit `db78e27c` removes only the critical-register partial from glance. All nine glance KPI keys/charts/formulas unchanged.
- Stage 4: Audit Performance Analysis replaces position cards with division-grouped department comparison, sorting, independently queried subtotals/overall total, settlement rate, net change and explicit reconciliation differences.
- Reuses `P_GET_KPI_LIFECYCLE` per mapped office, bounded at 100 children per division; no new Oracle calculation/procedure. Typical approved mapping has 12 direct children. Multiple reads may observe live changes; this limitation is shown. Staging should assess performance before release.
- Division totals include its full approved portfolio, while direct-child rows can omit division-direct and other descendant audits. Differences are displayed, never silently forced to reconcile.
- New exact GET permission: `/api/AuditParaSnap/GetPerformance`, page `1000304`, SNAP grants only CIA (3) and verified Super User (1), via existing `REGISTER_GET` in `executive_dashboard_permissions.sql`. Register on staging before using the new tab; do not grant wildcard APIs.
- Build: 0 errors/57 existing warnings. 9 CIA + 9 Snap + 4 comparison policy checks and 20 controller access checks pass. New endpoint rejects reuse of GetKpis grant for CIA and Super User. JS syntax/diff checks pass.
- Pending database compilation, staging reads, live reconciliation and browser/session QA apply to all stages.

## Stage 5
- Settlement Effectiveness replaces the old tab title; total settled/selected-period rate retained, clearance ratio only when positive additions and stock/movement reconciliation permit it.
- Added monthly `movement` dataset to existing `P_GET_SNAP_DISTRIBUTION`; independent ParasAdded/ParasSettled output columns preserve existing trend/risk/aging/cycle datasets. Added SQL uses the same lifecycle Para Added status-8/status-9/excluded_pre_finalization rules and settlement timestamps as KPI reader.
- Department rankings reuse exact performance permission and source; zero/missing/inconsistent denominators and non-zero reconciliation differences are excluded. Rankings are explicitly indicative estimates.
- No confirmed-inactivity list: historical baseline is not complete; UI explains missing history cannot establish inactivity.
- Build 0 errors/57 existing warnings; 42 compiled policy/controller checks pass; JS syntax and diff whitespace checks pass. Oracle compile/live reconciliation pending.

## Stage 6
- Critical Audit Observations renamed, no repeated risk-distribution chart. Register is confined to this tab, shows audit period/para, recorded movement age and explicit 100-record limit.
- Exact new GET `/api/AuditParaSnap/GetParaText`, page 1000304, SNAP CIA/Super User grants added to permission registration script. Read-only dialog uses textContent, never stored HTML injection; closes/cancels/clears on filter changes.
- `P_GET_SNAP_CRITICAL` appends optional `P_COM_ID`; predicate applies before rank/cap. Text retrieval is only after this exact selected-scope eligibility check and rechecked afterward. Uses existing bound `PKG_HD.P_GET_ALL_PARA_TEXT`, never its legacy HTTP endpoint. Requires existing PKG_HD execute privilege, no broader privilege.
- High-risk-by-audit-office uses performance source; high-risk over-year/over-three-cycles uses new exposure distribution. Insufficient cycle evidence gives N/A.
- Build 0 errors/57 existing warnings. Exact para API denials tested for CIA and Super User without the new grant; live authorized/unauthorized COM_ID requests remain staging-only QA.

## Oracle deployment (draft, not production-ready)
1. Existing `V_IAS_ENTITY_REPORTING_NAMES`, lifecycle, para base, CIA nature/checklist/rectification dependency views must be valid.
2. `snap_audit_division_scope.sql` spec then body (existing approved mapping).
3. `PKG_EXEC_DASHBOARD.sql` spec then body, with existing dependent views retained.
4. Updated `PKG_EXEC_REPORTS` specification then body.
5. Inspect USER_ERRORS and reconcile known scopes on staging before deploying application.
- Amended CIA procedures: `P_GET_CIA_EXEC_KPIS`, `P_GET_CIA_ISSUE_NATURE`; append optional `P_POSITION_AS_ON`, `P_SNAP_DIVISION_CODE` after existing parameters.
- Existing exact CIA API grants retained; no new endpoint in Stage 1.

## Quota and continuation
- Official five-hour usage last observed after the host interruption/reset on 2026-10-23: 0% used / 100% remaining. No quota bypass or reset was requested. Earlier work remained below the 2% stopping threshold.
- Native same-chat heartbeat ACTIVE: `resume-ias-cia-and-snap-implementation`, every five hours. Checks official quota, reads this file and resumes first pending work. Requires local Codex host to remain available; scheduling does not guarantee execution or quota.
- Stop before 2% remaining; checkpoint near 5%. No unsupported polling.
- Restart prompt: `Continue the CIA/Snap task from AIS/Docs/CODEX_DASHBOARD_PROGRESS.md in the cia-snap-enhancements worktree; check official quota first.`

## Stages 7–9 and commit inventory
- Aging percentages are shown only when the selected distribution reconciles to scoped closing stock; unavailable/inconsistent denominators remain N/A. Unknown/zero-cycle and over-three-year buckets remain present.
- Snap-only browser/CSV/PDF column cleanup preserves underlying calculations/models and unrelated dashboard presentations. Exact report grants and Super User-only exports remain enforced.
- Final build: 0 errors, 57 existing warnings. 52 compiled policy/controller/PDF checks pass. 20 focused Node client/source checks pass. Broad suite: 86 tests, 70 pass, 13 pre-existing failures, 3 skips; verified upstream baseline has the same 13 failures. No new failures remain after updating feature-affected expectations.
- No Oracle procedure was compiled or deployed; no authenticated live COM_ID/portfolio reconciliation or PDF visual acceptance is claimed.

| Stage | Commit |
|---|---|
| CIA filters/cutoff | `300d139a` |
| Snap authorized filters | `295d08c3` |
| Glance register removal | `db78e27c` |
| Performance comparison | `b78d3e83` |
| Settlement effectiveness | `fd98069f` |
| Critical observations/secure para text | `167c413c` |
| Aging percentages | `40f8757d` |
| Report presentation cleanup | `d32041e4` |
| Focused regression/validation hardening | `d566a768` |

The original checkout, TOP/GROUP/RLM/CCO/Head Audit views and unrelated Oracle procedure bodies are unchanged. Feature scope uses shared readers only through optional Snap-specific arguments.

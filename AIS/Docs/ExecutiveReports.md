# Executive Reports A–E backend — staged deployment

Task 3 follows main `d721f7db`. All five requested repository PNGs were inspected.
Their columns informed the cursor contract, never the data. No Razor, chart, export,
filter-layout or existing Oracle package changes are included.

## Evidence and boundaries

The repository does **not** establish complete, immutable lifecycle history for all
observations, original risk snapshots, every transfer, or a durable Audit/Compliance
settlement-source field. This backend therefore requires a certified history contract;
it does not claim that existing comment history alone can reconstruct every report.
The new history tables deploy empty. Procedures reject a selected portfolio with
missing creation evidence, inadequate date coverage, duplicate originating identities,
or an inconsistent event chain. APIs default to disabled. Compiling the package alone
is insufficient to enable them.

Source evidence reviewed:

- `PKG_RPT.R_FAD_MONTHLY_REVIEW`: Opening + Added is the denominator; its current-state
  formulas and split by current `COM_STAGE IN (41,45)` are comparison definitions,
  **not** a reliable historical ledger. Its risk logic uses annex risk for some branch
  types and observation risk elsewhere. Certification must resolve these source rules
  to historical Low/Medium/High snapshots (3/2/1); unknown risk stays Unrated.
- `PKG_AE`, explicit settlement branch: `COM_STATUS=16` at stages 6/7/44/41 writes
  timestamped history and closes the master. `PKG_FAD` writes similar compliance history
  during administrative status authorization. They cannot be counted independently
  when they record the same action. The history rows do not contain PARA_STATUS.
- `AIS_T_PARA_STATUS_CHANGE_LOG`, `T_AU_PARAS_STATUS_CHANGE_LOG`, new/old para logs:
  authorized timestamps and old/new states support real settle/reopen events.
  Legacy FAD/HD writers sometimes update older rows' authorization dates together.
  Current snapshots of these rows alone do not prove their original chronology;
  reconcile with immutable audit trails/backups, and exclude rejected/pending actions.
- `PKG_HD` copies entity `AUDITBY_ID` into observation `AUDITED_BY` on creation;
  `PKG_AR` does likewise for imported old FAD paras. They share an entity-ID domain,
  but directory assignment is current and can diverge from a case's stored auditor.
  Report filtering/grouping uses observation-level `AUDITED_BY`. Counts of mismatches
  are returned; directory assignment is never used to rewrite historical auditors.
- Some PKG_HD imports are already settled, and can synthesize a master settlement date
  (`ENTEREDDATE + 3`). Neither that timestamp nor today's PARA_STATUS is accepted as
  historical proof. Creation open/closed state must have actual source evidence.

Audit vs Compliance classification needs evidence of the settlement action's business
source. Actor role, free-text remarks, current workflow stage and `SETTELED_BY` alone
are insufficient. Canonical close events carry AUDIT, COMPLIANCE or UNKNOWN, with source
object/key and verifier. Known Audit/Compliance counts plus Unknown equal total unique
settlements. Zero known counts with nonzero Unknown do **not** mean true zero settlement
by those sources. Verification must resolve Unknown before certifying source breakdowns.

## Canonical history contract

`exec_reports_history_contract.sql` creates:

- `T_EXEC_REPORT_CASE_HISTORY`: one evidenced creation timestamp, initial open state,
  initial historical risk, verified FIELD/MANAGEMENT family and complete-through bound
  per COM_ID. Do not infer family solely from a guessed audit-type code or initial
  status from the current master. A complete-through bound is exclusive: reporting an
  inclusive End Date requires coverage through at least End Date + 1.
- `T_EXEC_REPORT_HISTORY_EVENT`: real status/risk snapshots in chronological order.
  Before/AfterOpen must chain from the creation state. AfterRisk is the actual risk
  snapshot at that event, including NULL if unknown; use status-preserving events to
  record risk changes. EventOrder must reflect verified order for same-timestamp events.
  One business action must appear once even if several legacy logs recorded it.
  A stable original source key must identify each event; imports are idempotent by key.
- SQL object/collection types for shared period facts. No application DML privileges
  are granted to the canonical evidence tables. The owner imports only verified source
  records using controlled transactions; records must retain provenance. Do not create
  fixtures, inferred dates, default open states, or fabricated migration history.

`exec_reports_history_candidates.sql` supplies read-only queries against the established
sources and auditor mismatches. It deliberately does not bulk-import unverified events.
Coverage must include settle/reopen actions outside the selected interval as well as
creation/risk changes. If immutable evidence cannot establish a case's coverage, keep
reports for that portfolio unavailable and document the gap. No current-state fallback
or silent exclusion of uncovered observations is allowed.

## Calculation and scope contract

All procedures take the same bind-only parameters: `P_DASHBOARD_TYPE`, the independently
server-derived `P_SCOPE_ROOT_ID`, `P_START_DATE`, `P_END_DATE`, `P_ENTITY_TYPE_ID` OR
`P_AUDITED_BY_ID`, `P_REPORTING_OFFICE_ID`, `P_ENTITY_ID`, and `IO_CURSOR`.
Dates bind with OracleDbType.Date, at midnight. Events use >= Start and < End+1.
GROUP root is required, verified as an unambiguous category-18 entity, and remains an
AND restriction independently of Entity/type/auditor/office filters. Selected Entity
may only narrow that root. The package is for the application principal, not direct
end-user EXECUTE grants; IAS authenticates the perspective/root before binding them.

Organization attribution is the **current authorized portfolio** (`V_EXEC_ENTITY_SCOPE`),
not reconstructed historical ownership. Transfers and organizational changes do not
move old values into an unauthorized hierarchy. Each case's lifecycle is replayed in
its current authorized portfolio for both period boundaries. Historical organization
and auditor-transfer snapshots are not claimed. Nearest regional category 5 and GM
category 21 follow existing IAS organizational categories; divisions use the existing
scope's division with group/entity fallback. Null categories remain explicit Unassigned
rows rather than being dropped. This attribution must be disclosed in Task 4.

`F_FACTS` returns one row per unique COM_ID per interval. Opening is actual replayed
open state immediately before Start Date; Additions count evidenced creations in the
interval; Outstanding is replayed open state at inclusive End Date. Settled counts
unique observations with an effective close in the interval. If a case settles several
times, only its final in-period close determines its source, so source counts never
double-count it. Reopening and resettlement event counts are separate adjustments.

- Total = Opening + Added.
- Adjustment = ReopeningEvents − ResettlementEvents − InitiallyClosedAdditions.
- ReconciliationDifference = Outstanding − (Total − Settled + Adjustment).
- SettlementPercentage = 100 × Settled / Total, NULL for zero denominator. Reopening
  activity may produce unusual percentages; expose the adjustment, never clamp ratios.
- Low + Medium + High + Unrated = Outstanding, using certified closing risk snapshots.

A supplies dynamic annual slices within the requested interval (partial years must be
labeled with their actual dates). Both annual presentations use this one dataset.
Do not sum annual opening/outstanding snapshots into a multi-year balance.
B supplies field auditor offices and category subtotals; C supplies regions with GM
subtotals. Both share exactly the same FIELD facts and grand total. E supplies management
functions and a grand total. Do not sum detail + subtotal + grand-total rows together.
D ranks the same FIELD regional facts using the unrounded settlement ratio, then region
ID as deterministic tie-break. No minimum-case threshold. Zero denominators/unassigned
regions are emitted as explicit excluded rows. With fewer than ten eligible regions,
TOP/BOTTOM lists can overlap; that is disclosed rather than padded with invented regions.

## Deployment order and activation

1. Verify production columns and immutable evidence with the candidates/structure
   references. Confirm the family and historical risk/source definitions against IAS
   report owners. Ensure prior `V_EXEC_ENTITY_SCOPE`, `V_EXEC_PARA_BASE` and
   `PKG_ORG_STRUCTURE` exist and are valid. Do not recompile/replace PKG_RPT or
   PKG_EXEC_DASHBOARD as part of this deployment.
2. Run `exec_reports_history_contract.sql` once as owner (new tables/types).
3. Import certified creation/events with provenance from actual IAS records. Reconcile
   coverage, duplicate identities, same-time actions, imported already-settled cases,
   risk changes and transfer attribution. Never mark incomplete history as verified.
4. Run `exec_reports_portfolio_view.sql`, then `PKG_EXEC_REPORTS.sql`. Check USER_ERRORS
   and object status. Grant the application principal EXECUTE on PKG_EXEC_REPORTS and
   its SQL types, with required owner-level SELECT privileges for source views/tables.
   Do not grant application write access to the evidence tables.
5. Run `verify_exec_reports.sql` for at least one known interval and all intended
   perspectives/filters. Compare matching cohorts to `PKG_RPT.R_FAD_MONTHLY_REVIEW`
   manually; differences from current-stage history reconstruction must be explained.
   Its standard inputs are P_NO/ENT_ID/R_ID/R_T/S_DATE/E_DATE; retain actual authorized
   IAS values when comparing. Confirm source split from sampled actions, not just totals.
6. Run `executive_reports_permissions.sql` with default inactive registrations.
   Configure verified TOP/GROUP role IDs without guessing; ordinary accounts require
   their own perspective page grant and each report API grant. Controller catalogue
   further restricts GROUP to E and CCO to B/C/D, matching phase-1 backend definitions.
7. Only after owner compilation, coverage/source verification and reconciliation, activate
   the new API registrations/grants and set deployment configuration:

   ```json
   { "ExecutiveReports": { "Enabled": true, "HistoryVerified": true } }
   ```

   Both flags default false when omitted. No environment appsettings files were changed.
   The DBConnection entry points enforce both flags as well as the controller. Re-login
   after grants change. Rollback activation by disabling either flag and the new API
   registrations; leave all existing dashboard procedures/permissions intact.

New routes are `/api/ExecutiveReports/GetReportA` through `GetReportE`. They share the
existing dates/cascade query contract. `view` may be omitted or must equal `reports`.
Session, page, API, perspective/report catalogue and hierarchy validation all apply.
Oracle errors 20801/20802/20803 map to invalid request/forbidden/history unavailable.
SQL compilation/execution and real Oracle verification remain owner tasks: Codex has
no live Oracle connection and does not assert these scripts compiled on Oracle.

Legacy APIs no longer load the full directory for an unfiltered `selectBy` mode. They
reject non-null DimensionId/ReportingOfficeId with `unsupported_filter`, even when an
Entity is supplied, instead of returning broader figures. Existing Entity-hierarchy
and P_AUDIT_PERIOD behavior and dateRangeApplied=false remain intact. Dashboard layouts
are unchanged; rejected dimensional combinations show the existing error state.

## Local verification

One focused AIS net8.0 build passed with 0 errors and the same 57 existing warnings.
Source checks confirmed all five procedure/API mappings and typed binds, history-only
state calculation, independent GROUP root predicates, unknown settlement-source
handling, subtotal/ranking definitions, disabled deployment gates and legacy dimensional
filter rejection. No live database query, Oracle compilation, extensive tests or report
UI changes were performed. The owner must run the supplied verification script.

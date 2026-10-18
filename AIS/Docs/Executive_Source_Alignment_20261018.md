# Executive source alignment: scoped development corrections (18 Oct 2026)

## Scope and constraints

Source-only GitHub changes; no Oracle database connection, compilation, migration,
data updates, permission change or production release. Keep the existing
`PKG_EXEC_DASHBOARD`, `PKG_EXEC_REPORTS` and typed .NET cursor column names.
Review any differences against the **live installed Oracle definitions** before
compiling: the repository source might lag behind a locally edited package.

### Confirmed source defects corrected

1. `PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE.ISSUE_DROPPED` was always zero:
   it tested `ISSUE_RECTIFIED_ON` for status 23, while the lifecycle view
   correctly produces a rectification date only for statuses 4 and 9.
   The diagnostic now reads status-23 observation `STELLED_ON`, requiring
   an eligible concluded observation and a dated pre-conclusion disposition.
   **ISSUE_RECTIFIED remains unchanged; drops are not rectifications.**
2. `V_EXEC_PARA_BASE` assigned missing/negative ages to the valid
   `0-90 DAYS` bucket. It now returns `AGING_DAYS=NULL`,
   `AGING_BUCKET='UNKNOWN'` for missing settlement dates, invalid
   statuses, or end-before-add chronology. Added `AGING_QUALITY` and
   `ISSUE_RECTIFIED_COPY` at the **end** of the view, leaving existing
   field positions and current KPI counts for valid rows unchanged.
3. `P_GET_CIA_EXEC_KPIS` matched concluded engagements to approved
   plan records by `PLAN_ID` **and** entity. The approved business
   metric is the number of distinct *planned entities* with concluded
   engagements in the same IAS audit period, not the number of exact plan
   rows with an engagement. The revised join is entity + selected
   `T_AU_PERIOD.AUDITPERIODID`, and the numerator is still restricted to
   approved planned entities. This could explain 0/492, but **live data
   must prove the actual cause and corrected count**.

### Lifecycle and report-source stability

4. `V_EXEC_KPI_LIFECYCLE` now identifies the latest unreversed
   STATUS=14 event by the engagement-log ID's event order, not
   `MAX(CREATED_ON)`, which can select the wrong row after a backdated
   correction. `V_EXEC_ISSUE_RECTIFIED_RECON` uses the same rule.
   `V_EXEC_KPI_SRC_A` intentionally retains its *first* conclusion
   for original modern PARA recognition. Existing `ISSUE_ELIGIBLE`,
   ISSUE_RECTIFIED flag/status guard, and column positions are unchanged.
5. `V_EXEC_REPORT_SOURCE` exposes `HISTORY_CLOSE_STAGE` only when
   its history settlement day equals the current settled date (the
   existing Reports A-E package already required this match).
   Added read-only `REPORT_MOVEMENT_QUALITY` and
   `ISSUE_RECTIFIED_COPY` fields **at the end** of the view.
   Missing first conclusion, missing settle timestamp, settlement before
   effective recognition, and closing-history disagreements remain visible
   diagnostics. No invented settlement dates, statuses, fraud or amounts.
   The copied issue flag is **not** added to report activity, Para Settled,
   or Issue Rectified totals.

### Intentionally not rewritten without live business evidence

- Historical opening/closing snapshots cannot be certified from
  current para status, latest settlement and present organization.
  Reopenings, transfers and old risk/cycle values require a reliable
  historical ledger. Continue marking date-based estimates as estimates.
- `V_EXEC_PARA_BASE` is a current post-compliance position for all
  source IND values and statuses. `V_EXEC_KPI_LIFECYCLE` recognizes
  ISSUE plus modern/legacy A/O/C PARA entries; `V_EXEC_REPORT_SOURCE`
  additionally limits report rows to eligible/dated settled-or-open PARA.
  **Do not force these populations to equal by discarding rows**.
  Check duplicates and source exclusions before changing totals.
- CIA Nature of Issue still needs verified live master IDs populated into
  `T_IAS_CIA_ISSUE_NATURE_MAP`. These corrections do not turn unmapped
  observations into classified observations.
- Audit Para Snap feature flag/API authorization is a separate concern;
  this PR does not activate it.
- Workflow stage choices (delay +15 days, conducting stages) and
  `T_AU_PLAN.STATUS IN (1,2)` still need live owner validation.
- Time-zone mismatch between `SYSDATE` and Pakistan calendars can affect
  end-of-day boundaries. No unconfirmed environment override is introduced.

## Manual development compilation order

1. Confirm the current **development** tables include
   `T_AU_OBSERVATION.ISSUE_RECTIFIED` and
   `AIS_T_AU_POST_COMPLIANCE.ISSUE_RECTIFIED`. Do not rerun historical
   update/backfill scripts automatically.
2. Back up installed package/view DDL before replacing any object.
   `V_EXEC_ENTITY_SCOPE` is first in
   `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql`, followed by `V_EXEC_PARA_BASE`;
   if already valid, avoid replacing scope hierarchy unnecessarily.
3. Compile/read-only check `V_EXEC_KPI_SRC_A/O/C` if needed, then
   `V_EXEC_KPI_LIFECYCLE.sql`; compile
   `V_EXEC_ISSUE_RECTIFIED_RECON.sql` to align latest event semantics.
4. Compile the revised `V_EXEC_PARA_BASE` definition from
   `PKG_EXEC_DASHBOARD.sql`. Use SQL Developer F8 **per CREATE statement**,
   not F5 on the entire canonical file.
5. Compile `V_EXEC_REPORT_SOURCE_image_rewrite.sql`.
6. Compile revised `PKG_EXEC_DASHBOARD.sql` **package specification and
   package body independently**. Ensure the previous procedure signatures
   and select-field aliases remain intact. Recompile `PKG_EXEC_REPORTS`
   only if invalidated; compare local package versions first.
7. Run `verify_exec_view_alignment_20261018.sql` SELECTs independently.
   Especially compare audited-by-entity versus audited-by-planid and
   check duplicates, invalid ages, ISSUE_DROPPED and report
   movement-quality categories. No SQL write is needed for these checks.
8. Re-check `USER_ERRORS` and source-level tests in
   `tests/executive-dashboard/executive-source-alignment.test.cjs`.
   No live Oracle compilation or live test was run during authoring.

## Exit criteria

Package spec/body and all dependency views VALID, no USER_ERRORS,
expected one-record-per-`COM_ID` / one-issue-per-observation cardinality,
unchanged existing API aliases and non-CIA dashboard counts except
correctly diagnosed invalid-date effects. CIA audited-entity count reconciles
to distinct planned entities concluded in selected period. No production work.

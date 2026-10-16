# CIA Command Center SQL handoff (16 October 2026)

## Status and scope

**Feature branch:** `feature/cia-executive-kpis-issue-nature`

This branch contains SQL source changes only. The existing CIA, TOP, CCO, GROUP,
RLM, lifecycle, issue-count, and report procedures are preserved. No API,
Razor, JavaScript, production Oracle schema, or other dashboards have been
deployed or modified.

Added to the canonical `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql` package:

- `P_GET_CIA_EXEC_KPIS` (one row, five KPI domains)
- `P_GET_CIA_ISSUE_NATURE` (exactly ten rows, mapping diagnostics)

Both reject non-CIA perspectives using error -20041. This database check
does not replace authenticated role, selected-entity, and page/API grants
in the .NET service. Do not expose these procedures to an untrusted caller.

## Business definitions

| Output column | Rule | Time frame |
| --- | --- | --- |
| PLANNED_ENTITIES | Distinct `T_AU_PLAN.ENTITY_ID`, plan status 1 or 2 | Selected audit period ID |
| AUDITED_ENTITIES | Planned entities with an engagement reaching status 14 via current status or engagement log | Selected audit period ID |
| PLAN_COVERAGE_PCT | 100 * audited / planned; NULL when planned = 0 | Selected audit period ID |
| DELAYED_AUDIT_REPORTS | Distinct engagements with status 4..13, audit end +15 days already passed, and no status-14 milestone | Current state, all audit periods |
| FRAUD_A1_PARAS | Distinct `T_AU_OBSERVATION.ID`, annexure ID 1, observation status 8/9, `ENTEREDDATE` within selected period dates | Selected period event dates |
| AWAITING_AUDITEE_RESPONSE | Distinct `T_AU_OBSERVATION.ID` where **STATUS = 2**, scoped via audited engagement | Current state, all audit periods |
| TEAMS_CONDUCTING_AUDIT | Distinct assigned team IDs on engagements with status 4..12, commenced start date, without status-14 conclusion | Current state, all audit periods |

Current operational KPIs deliberately include outstanding work from prior
audit periods even when the user selects a different period for planning /
issue detection. `CURRENT_STATE_AS_OF` discloses the current-state date.

The following are working definitions that require real database validation
before the CIA frontend is enabled:
1. Annexure ID 1 means A1 for the modern `T_AU_OBSERVATION` table.
2. The audit-end +15 calendar-day threshold is the appropriate report
   deadline (reused from `PKG_RPT.P_DELAY_AUDITS`).
3. Engagement statuses 4..12 accurately represent teams still conducting
   audit, rather than teams in a separate reporting or approval stage.
4. Status-14 log/current engagement status is the agreed definition of
   "audited" for coverage.

The Fraud KPI is a reported A1 para count; the display must not assert
criminally established fraud or loss amounts.

## Ten-category implementation

`P_GET_CIA_ISSUE_NATURE` reuses the `ISSUE` population and
`ISSUE_ELIGIBLE = 1` from `V_EXEC_KPI_LIFECYCLE` and the same event date
meaning as the existing `P_GET_CIA_ISSUES`. It looks up source ID mappings
in `T_IAS_CIA_ISSUE_NATURE_MAP`:

- `VIOLATION_SUB`: observation `V_CAT_NATURE_ID`
- `ANNEX`: observation `ANNEX`
- `AUDIT_PORTFOLIO`: engagement `AUDITBY_ID`

These must be linked to the ten approved category codes after confirming
the live master records, including Management Audit and IS Audit portfolio
IDs. Lowest `MATCH_PRIORITY` wins when more than one rule matches.
Mapping is intentionally not guessed or auto-seeded.

For each category row, the cursor also returns `TOTAL_ISSUES`,
`UNMAPPED_ISSUES`, and `MAPPING_STATUS`. Do not show a ten-category chart
as a complete breakdown unless `MAPPING_STATUS = 'COMPLETE'` (or there
are no eligible issues). Always reconcile:

`SUM(ISSUE_COUNT) + UNMAPPED_ISSUES = TOTAL_ISSUES`.

Do not modify existing issue eligibility rules to force mapping totals.

## Controlled deployment

1. Back up the live package specification and body. Use an isolated Oracle
   test schema first if available.
2. Run once `AIS/Docs/sql/T_IAS_CIA_ISSUE_NATURE_MAP.sql`. Confirm table
   exists. It does not populate any mappings.
3. Compile the specification, then the body, of the updated canonical
   `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql`, as individual F8 statements in
   SQL Developer. Do not accidentally rerun the preceding view
   definitions. Ensure every pre-existing procedure remains present.
4. Run `AIS/Docs/sql/verify_exec_cia_command_F8.sql` as separate statements
   / anonymous blocks. Zero rows from `USER_ERRORS` is mandatory.
5. Populate only the user-approved nature mapping source IDs and rerun
   category reconciliation and bank-wide versus entity-scoped comparisons.
6. Validate plan coverage, late reports, A1 and active stages against real
   source figures. Do not release the new KPI UI for unapproved definitions.
7. Only after Oracle validation, add dedicated typed CIA-only C# methods and
   authorized API actions. Do not repurpose existing `GetKpiSummary`,
   `P_GET_CIA_ISSUES`, or other role perspectives. Merge only after review.

Environment limitation: SQL has been reviewed and committed to GitHub,
but no live Oracle compilation, data reconciliation, or production deployment
has been performed in this chat.

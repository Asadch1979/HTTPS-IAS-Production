# CIA Command Center — five KPI and ten issue-nature SQL procedures

**Status: Database implementation candidate; not merged or deployed.** The canonical edited file is `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql`; two standalone Oracle F8 scripts are provided. Existing procedures (`P_GET_KPI_SUMMARY`, `P_GET_KPI_LIFECYCLE`, `P_GET_CIA_ISSUES`, risk, aging, trends, critical items) and their signatures are **unchanged**. Existing CIA/TOP/CCO/GROUP/RLM frontends and API permissions are **unchanged**. The new SQL contains explicit *unverified* classifications and must not be treated as ready for production without audit-owner confirmation.

## New procedure 1: P_GET_CIA_COMMAND_KPIS

Inputs: `P_DASHBOARD_TYPE` (must be `CIA`), `P_ENTITY_ID` (authorized root or NULL only when the authenticated backend permits bank-wide CIA), mandatory integer `P_AUDIT_YEAR`, optional `P_ENTITY_TYPE_ID`, `P_AUDITED_BY_ID`, `P_REPORTING_OFFICE_ID`, and `IO_CURSOR`. The same authenticated scope must be derived server-side; SQL cannot replace IAS page and API authorization.

Cursor fields:

| Column | Source / exact current semantics | Data-quality |
|---|---|---|
| `ENTITIES_PLANNED` | Count **distinct entity IDs** on `T_AU_PLAN` for audit year based on `T_AU_PERIOD.START_DATE`; unlike an engagement-only query this includes scheduled entities without engagement rows | **Provisional** until cancelled/deleted plan statuses and approval/version rules are verified |
| `ENTITIES_AUDITED` | Distinct planned entities with `T_AU_PLAN_ENG.PLAN_ID` linked to plan ID, engagement current `STATUS=14` **and** logged `STATUS_ID=14` conclusion | **Provisional**; valid only if Status 14 completion is agreed to mean “audited” for plan coverage |
| `AUDIT_PLAN_COVERAGE_PCT` | `100 * ENTITIES_AUDITED / ENTITIES_PLANNED`, NULL where no plans exist | Provisional; source year is plan audit-period year, not necessarily calendar audit activity year |
| `DELAYED_AUDIT_REPORTS` | `NULL`, **not zero**. No confirmed approved issuance deadline, signed report issuance event or SLA in reviewed repository. Never derive delay by assuming `AUDIT_ENDDATE` or Status 14 is the issuance deadline | `NOT_VERIFIED_REPORT_ISSUE_DATE_AND_SLA` |
| `FRAUD_DETECTED_A1_PARAS` | Distinct `COM_ID` in approved source A post-compliance paras, with A1 annexure `T_AUDIT_CHECKLIST_ANNEXURE.CODE = 'A1'` (take post-compliance `ANNEX` first, with linked observation `ANNEX` as fallback) and *audit-period year* from `T_AU_PERIOD.START_DATE`. Excludes pre-Status-14 dropped/rectified issues through `PARA_ADDED_ON` source logic. **Not the outstanding A1 stock** | `MODERN_FINAL_SOURCE_A_ONLY`; legacy A1 O/C not added since audit-year attribution and dedupe not yet established |
| `AWAITING_AUDITEE_RESPONSE` | `NULL`, **not zero**. Current `COM_STAGE`/COM_STATUS aggregates do not establish that the next action owner is the auditee; response-owner/status must be verified | `NOT_VERIFIED_AUDITEE_OWNS_NEXT_ACTION` |
| `TEAMS_CONDUCTING_AUDIT` | Distinct current `T_AU_AUDIT_TEAMS.TEAM_ID` with approved team `STATUS=2`, an observed `T_AU_AUDIT_JOINING.JOINING_DATE`, engagement neither concluded (14) nor referred back (9), and scheduled audit dates inclusive of today | **Provisional**: a joining record does not prove ongoing physical fieldwork; current-state independent of selected audit year |
| `SNAPSHOT_DATE` | `TRUNC(SYSDATE)` | Current system date, not a past as-of status replay |

**Do not label null as 0 in C#.** Carry and display `PLAN_METRIC_STATUS`, `DELAYED_REPORT_METRIC_STATUS`, `FRAUD_METRIC_STATUS`, `RESPONSE_METRIC_STATUS`, and `TEAM_METRIC_STATUS`. Counts marked provisional should be qualified (e.g. `≈`) until validated.

### Unresolved decisions blocking two cards
1. **Delayed Audit Reports:** approved report issuance time limit (days after audit conclusion/end?), authoritative due date, approved report issuance milestone and status for stopped/exempt audits. A concluded audit need not have had its report issued.
2. **Awaiting Auditee Response:** the exact auditee-owned stage/status in draft observation/management para/post-compliance workflow; ensure no duplicate open para/observation, ignore responses already received, and distinguish reply pending from audit review pending.

## New procedure 2: P_GET_CIA_ISSUE_NATURE

Optional paired `P_START_DATE`/`P_END_DATE`; no dates counts **all historically eligible** issues just like `P_GET_CIA_ISSUES`. Eligible issue = `V_EXEC_KPI_LIFECYCLE` ISSUE row, `ISSUE_ELIGIBLE=1`, `ISSUE_IDENTIFIED_ON`, authorized entity hierarchy; date filters apply to ISSUE_IDENTIFIED_ON, end inclusive. Joins `T_AU_OBSERVATION.CHECKLISTDETAIL_ID → T_AUDIT_CHECKLIST_DETAILS → T_AUDIT_CHECKLIST_SUB → T_AUDIT_CHECKLIST.HEADING`.

Category rows in fixed order:

1. AML/CFT (also includes KYC).
2. Accounts (strictly `ACCOUNT` / `ACCOUNTS` to avoid collision with Accounting).
3. Loan and Advances.
4. Cash Management.
5. Record Management.
6. Banking Operations.
7. Branch General Controls.
8. Accounting/Reporting/Reconciliation.
9. Management Audit.
10. IS Audit.
11. Other / Unclassified — **mandatory reconciliation row**, not an eleventh approved category.

Returns `CATEGORY_ID`, `CATEGORY_NAME`, `ISSUE_COUNT`, `ISSUE_PERCENT`, `MAPPING_STATUS`, `START_DATE`, `END_DATE`. The 11 row totals (including unclassified) must equal `P_GET_CIA_ISSUES.ISSUE_IDENTIFIED` for the exact same date and organizational scope. The mapping uses uppercase **checklist headings**, not free-text heuristics on observations; all category mappings remain review-required until their actual IAS checklist titles are confirmed. In particular, `ACCOUNTS` and `IS AUDIT` are not yet confirmed separate configured headings. Never remap them silently.

## Safe deployment and rollback

1. On a non-production Oracle instance, execute `PKG_EXEC_DASHBOARD_CIA_command_spec_F8.sql` and then `PKG_EXEC_DASHBOARD_CIA_command_body_F8.sql` using SQL Developer **F8**. **Both source files include the entire existing package**, retaining all prior procedures.
2. `SELECT * FROM USER_ERRORS WHERE NAME='PKG_EXEC_DASHBOARD';` must return no errors.
3. Run `verify_cia_command_center_F8.sql` independent statements, confirm 2026 plan totals, A1 annex code, 11-category issue reconciliation, team status and portfolio filter behavior.
4. After data owners approve plan-year/status, A1 completeness, checklist category mappings, report delay SLA and auditee response ownership, add *separate authenticated CIA endpoints*. Do not use these SQL procedures directly from an anonymous or global controller.
5. Do not merge while validation is incomplete. No existing page or backend contracts change in this PR.

**Key structural notes:** Oracle DB packages do not authenticate the user. The existing ASP.NET executive controller/security must derive the CIA role, root entity, organizational dimensions and any selected period from session/authorized filters. A caller who can execute the package as the schema owner can pass the string 'CIA'; database-side string validation alone is not an IAS permission check.

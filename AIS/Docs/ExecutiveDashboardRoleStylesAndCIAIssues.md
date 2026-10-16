# Executive Dashboard perspective selection and KPI clarification (16-Oct-2026)

## Finalized style/role mapping

| Perspective | Design direction | Audience | Core content |
|---|---|---|---|
| Diligent HighBond style | Executive audit overview | CIA, Heads of Audit | Audit plan execution, high-risk issues, draft reports, audit performance, overdue remediation |
| TeamMate style | Auditor workbench | Audit Team Leaders | Assigned audits, engagement stages, fieldwork, review tasks, observations and deadlines |
| ZTBL Management Audit | Management perspective | Group Heads, Divisional Heads | Paras and risk exposure, division comparisons, settlement and pending decisions |
| Dark Dashboard (Design 3) | Compact operational view | General Managers and Regional Heads | Open paras, risk, aging, compliance cycles and branch/entity comparisons |

Design choices are approved **directions**, not a request to replace role mapping, route permission or implemented reports immediately. Implement role-specific layouts in subsequent scoped commits after current KPI corrections and user acceptance.

## Current KPI changes

**TOP**: `Opening Open Paras` is a historical baseline as on selected Start Date, not an activity rate. Date period captions use `Selected Date Range`. Current `Issue Rectified` query counts statuses 4 or 9 only, excluding status 23 (Drop by Team Lead); `ISSUE_DROPPED` remains available diagnostically. Default TOP End Date is yesterday when absent. The TOP charts/critical items remain current-state.

**CIA**: display two new separate issue events on At a Glance: `Issue Identified` (eligible `T_AU_OBSERVATION.ENTEREDDATE`) and `Issue Rectified` (pre-Status-14 date from `T_AU_OBSERVATION.STELLED_ON`, status 4/9 only). The CIA had **no** Para Added card in its original glance view, so there is no existing Para Added field to split; these are added independently to CIA.

CIA Start/End dates are **optional as a pair**. Both blank means all historically eligible issue records in the IAS lifecycle scope; both selected means issue events in the inclusive selected range. Other existing CIA para/current-state KPIs, charts and critical items continue to be **current state** and should not be described as date-filtered. For exact historical/current-state snapshots over a range, additional confirmed movement history is required.

Aging in critical-item list is shown as literal `AGING_DAYS`. `Settled MTD` expanded to `Settled Month-to-Date`.

## Deployment sequence and permission model

1. `PKG_EXEC_DASHBOARD_CIA_spec_F8.sql` and `PKG_EXEC_DASHBOARD_CIA_body_F8.sql` compile in authorized IAS Oracle schema using F8 (spec first).
2. Execute `verify_exec_cia_issues_F8.sql` independently, verify no user_errors, and compare filtered/full-history counts with source observations.
3. Only after the package is deployed, merge/release the corresponding CIA C# and JavaScript changes.
4. New CIA query uses the existing `GetKpiSummary` endpoint and its existing session/page/API permissions, with mandatory authenticated CIA scope. No new external API grant is required. Never weaken organizational filters to retrieve broad results for an unauthorized role.
5. TOP historical estimates remain qualified: the new Issue Rectified status 23 exclusion is a safe business correction, not a certification of historical snapshots.

Do not merge or publish before the user verifies the Oracle package and CIA counts.

# IAS executive dashboard — targeted presentation and permission corrections

**19 October 2026 — GitHub source candidate, NOT deployed to Oracle/IIS.**
This branch is based on merged PR #327. The earlier CIA-only loading patch
PR #326 and proposed Super-User-only permission hardening PR #325 remain
separate and must be reconciled before rollout.

## 1. Audit Para Snap: restore monthly trend and avoid repeated Glance cards

`AuditParaSnapPolicy.Definitions()` provides a **view-specific** card/section
set, not the nine common cards on all tabs:

| Snap tab | KPI cards | Chart/panel |
| --- | --- | --- |
| At a Glance | 9 date-aware lifecycle values | Monthly settlement trend + Risk |
| Audit Para Position | Opening, added, settled, closing | No repeated overview chart |
| Settlement & Performance | Settled, rate for selected date range | Monthly settlement trend |
| Risk | High-risk open | Risk distribution + critical list |
| Aging & Compliance | Over one year, over 3 cycles | Aging and compliance-cycle sections |

`P_GET_SNAP_DISTRIBUTION` returns an additional named cursor column
`HIGH_RISK_COUNT` for its `trend` rows, computed from the **same
selected-date event cohort** as `PARA_COUNT`. Existing column names and
procedure signature are unchanged. The client renders one SVG line chart
with **All settled (blue)** and **High-risk settled (red)**, plus a
collapsible detail table, instead of treating the trend as aging bars.

`AuditParaSnapAnalysis.cshtml` only creates each tab's relevant cards.
The client only requests visible datasets; this avoids unnecessary Oracle
reads for tabs not showing a list or chart.

## 2. Actual ratios — avoid mixing lifetime, MTD and arbitrary period

The old `SETTLEMENT_RATIO` in `PKG_EXEC_DASHBOARD.P_GET_KPI_SUMMARY`
is the *overall/lifetime* settled share of open-plus-settled cases.
It remains unchanged for other cards that need lifetime context.

For **Settlement & Performance** on the role dashboards, display the
separate **MTD Settlement Rate**:
`100 × settled in current month / (estimated open at month start +
paras added during current month)`. The monthly opening is
**reconstructed** using the currently available IAS master status,
para-added and settlement dates; it is not a fully audited movement
ledger. Zero denominator produces Oracle **NULL → N/A**; not a made-up
zero. New `SETTLEMENT_MTD_RATE` is appended to the existing KPI summary
cursor, with a nullable C# property; package spec unchanged.
The existing `SETTLEMENT_RATIO` and `SETTLED_MTD` fields remain intact.

For **Audit Para Snap Settlement & Performance** with freely selected
start/end dates, the period-specific rate is computed from the
existing dated lifecycle values:
`paras settled / (estimated opening open + paras added)`.
It displays N/A if the data are missing, the denominator is zero,
or settlements exceed that denominator. This is NOT automatically called
MTD unless the selected range actually covers the month.

These two new performance ratios must be **reconciled with the actual
production population before they are treated as approved banking KPIs**.
Do not describe reconstructed historical status as independently verified.

## 3. Risk distribution

Only High/Medium/Low appear in charts and percentage tables. "Unrated"
remains in the Oracle risk source and can be separately inspected for
data-quality purposes. Percentages use **rated-open** paras as denominator
to avoid hiding cases while misrepresenting total percentages.

## 4–6. Reduced visual clutter, factual critical definition

- Remove the long, repetitive date/historical caveat paragraphs from the
  executive parents and Snap analysis pages. Keep the IAS `QUALITY_CODE`
  and the opening/closing evidence flags in API data and short
  accessible tooltips. Avoid prominent "Estimated ..." boilerplate,
  but retain the ability to verify and disclose historical uncertainty.
- The Critical detail footer states:
  **"Critical = open High-Risk paras (ZTBL)."**
- Retain legacy charts and approved executive page themes; no RLM dark
  theme rollback.

## 7. Executive dashboard pages and API assignment from the IAS front end

**Confirmed defect:** The existing `PKG_AD.P_AddGroupMenuItemsAssignment`
uses only `T_AU_API_MASTER.PAGE_ID` to grant APIs to the group. However
`GetKpiSummary`, `GetRiskSummary`, `GetRoleFollowups`, etc. have
**one shared registration** anchored to TOP 1000300 even when another
parent page uses them. Assigning GROUP/CCO/RLM does not grant its APIs
under the old code.

**New design:**
- `T_IAS_EXEC_PAGE_API` contains **44 explicit page-to-GET API pairs**
  across TOP **1000300**, GROUP **1000301**, CCO **1000302**, CIA **1000303**,
  Audit Para Snap **1000304**, RLM **1000305**.
- RLM now has its own `/ExecutiveOverview/RLM` menu page (ID 1000305)
  instead of sharing the Organization Structure authorization page.
  Organization Structure grants are not altered.
- Existing shared API URL registrations remain singletons.
- `PKG_AD.P_AddGroupMenuItemsAssignment` grants/activates **only**
  manifest-listed GET APIs whose master `IS_ACTIVE='Y'` when an
  authorized administrator explicitly assigns the parent page.
- `PKG_AD.P_RemoveGroupMenuItemsAssignment` revokes those APIs when
  the page assignment is removed, **unless another still-assigned
  executive dashboard needs the same GET**.
- Other IAS page assignment and removal behavior are intentionally
  unchanged. No general authorization bypass, wildcard API grant or
  bank-wide impersonation is added.

### Role scope and current pilot restrictions

**Page assignment is necessary but NOT sufficient for privileged,
bank-wide dashboard access.** The existing application still validates:

- Authenticated IAS session and role.
- Authorized page assignment and exact HTTP method+API grant.
- CIA, CCO and TOP role/perspective eligibility, including existing
  `ExecutiveDashboard:RoleMappings` for optional roles.
- Group reporting root must be an actual verified Group Head; RLM
  derives an allowed entity from the IAS session, not browser input.
- All downstream Oracle hierarchy and filter validation.

Therefore, an ordinary user explicitly assigned RLM with an eligible
posting can view only their scoped descendant data, **not TOP/CIA
bank-wide analytics**. A role assigned a TOP/CIA/CCO page still requires
its verified server-side role mapping to access that perspective. This
is deliberate protection against privilege escalation through menu
editing. Expanding those verified bank-wide role mappings requires a
separate, explicit authorization decision.

**IMPORTANT:** The production plan is currently Super User-only.
Neither the manifest deployment nor this GitHub commit changes that:
the script itself creates **no role grants, no API master activations,
and no other users' menu assignments**. *Using the front-end grant
workflow later* can grant appropriately scoped API access, and must be
done only after owner approval.

## DBA dependency/implementation order (staging before production)

1. Back up target objects: `T_MENU_PAGES`,
   `T_MENU_PAGES_GROUPMAP`, `T_AU_API_MASTER`,
   `T_AU_ROLE_API_PERMISSION`, `PKG_AD` and
   `PKG_EXEC_DASHBOARD`. Record current role 1-only preview grants.
2. Ensure all executive GET endpoint registrations are present in
   `T_AU_API_MASTER`. Review/run
   `AIS/Docs/sql/executive_dashboard_page_api_manifest.sql` with
   SQL Developer F5/SQL*Plus in DEV/STAGING, **not F8**.
   It adds the table, seeds the mappings and registers dedicated
   RLM page; no role is assigned or master activated.
3. Compile the revised `PKG_AD` **body only**.
   Leave its package specification unchanged. Verify existing
   `P_AddGroupMenuItemsAssignment` and
   `P_RemoveGroupMenuItemsAssignment` signatures unchanged.
4. Compile the revised `PKG_EXEC_DASHBOARD` **body only**.
   Do NOT execute all view/package DDL in the canonical file;
   new cursor columns `SETTLEMENT_MTD_RATE` and
   `HIGH_RISK_COUNT` must be available before the application deploys.
5. Deploy the C#/Razor/JS patch to staging; invalidate the permission
   cache after any front-end group assignment and use fresh login sessions.
   Verify one assigned RLM role can see *only its own scope*,
   a withdrawn role gets 403, a role still assigned a different
   dashboard retains shared API access, and a CIA/CCO/TOP page
   assignment alone does **not** bypass bank-wide perspective checks.
6. Run each read-only SELECT from
   `AIS/Docs/sql/verify_exec_visuals_permissions_F8_20261019.sql`
   separately with **F8** in PL/SQL Developer. Compare the new MTD
   denominator to the exact production records, and both Snap trend
   series to the same selected dates and authorized entity scope.
7. Only then propose the production deployment window. Preserve the
   Super User-only pilot until the owner explicitly approves extension
   to management and other IAS roles.

### Integration caution

Unmerged PRs #325 and #326 also touch executive permission/controller
sources; coordinate the final merge order and resolve source conflicts.
Never run an old `executive_reports_permissions.sql` with default
`V_ACTIVE='N'` against currently enabled report endpoints; it can
deactivate them on repeated execution. Reconcile/report this risk
separately rather than bundling an unreviewed API activation change here.

**No live Oracle, IIS, production grants or real staff sessions were
changed by this branch. Runtime/Oracle correctness still needs staging
compilation and actual data reconciliation.**

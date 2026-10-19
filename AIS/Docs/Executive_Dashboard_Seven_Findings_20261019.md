# IAS Executive dashboard screenshot findings — 19 October 2026

**Status: GitHub source proposal only; not deployed to IAS production or Oracle.**
The reported screenshots are from the user's authenticated Super User IAS session.
Pilot access must remain **Super User only** until verified by the business owner.

## 1. Audit Para Position dates

Confirmed: `GetKpiSummary` returns the current position; the selected Start/End
dates were only retained in navigation. For **TOP → Audit Para Position**, this
patch replaces those cards with the existing scoped
`P_GET_KPI_LIFECYCLE` read-only API, showing **Opening Open, Para Added,
Paras Settled, Closing Open and High-Risk Open** for the chosen dates. The
historical opening/closing (and high-risk close) are still clearly identified
as **ESTIMATED** when the Oracle cursor reports quality E. No date is applied
to the old current-only Risk panel in the Position tab: it is removed to avoid
cross-period misrepresentation. TOP At a Glance retains its existing lifecycle
KPIs and current-only charts.

For **other perspectives**, old `position` (and other non-lifecycle
non-report views) remain *current-state*. This patch removes misleading
Start/End inputs on those current-state pages; it does **not** claim they
have been made historical. Extending robust date-based position to CCO, RLM and
GROUP requires separately verified event history, a narrowly authorized API,
and explicit business acceptance.

## 2. 29,146-day ages

Confirmed source: `V_EXEC_PARA_BASE.AGING_DAYS` subtracts recorded
`AIS_T_AU_POST_COMPLIANCE.PARA_ADDED_ON` from the current date for an open
para. Age 29,146 days suggests an extremely early stored date **if that is
the actual source row**, but the screenshots alone do not establish the
underlying value or justify a corrective UPDATE. The JavaScript now exposes
`PARA_ADDED_ON` in a tooltip on the Critical list, and the new read-only F8
script checks specific listed COM_IDs 61581/61582 (and reference legacy
COM_IDs) against stored dates, derived ages and audit periods.

**Not resolved until these source rows are confirmed.** We will not silently
clamp a legitimate historical age or overwrite legacy IAS evidence. DBA to
investigate missing or erroneous `PARA_ADDED_ON`, then agree a targeted source
correction *only when source documentation supports it*.

## 3. ZTBL: High Risk == Critical

Confirmed current SQL defect: `V_EXEC_PARA_BASE.IS_CRITICAL` was
`open AND (risk=1 OR cycles>3 OR age>365)`. Thus Critical 13,927 exceeded
High Risk 6,291 in the screenshot. The new definition is
`PARA_STATUS=8 AND RISK=1`. The existing
`P_GET_CRITICAL_ITEMS` filters `IS_OPEN=1 AND IS_CRITICAL=1`, so current
Critical count and High Risk open count will agree **within identical scope
and filters**, after the revised Oracle VIEW is compiled. The separate
`P_GET_SNAP_CRITICAL` is similarly restricted to high-risk open paras in
the selected dated snapshot (the past-date risk is still estimated from
present risk). Age/cycles remain their **own** indicators, not Critical.

## 4. Reports tab visibility

`ExecutiveDashboardViews.ForPerspective()` now offers Reports only to
CIA, CCO/Compliance and GROUP Management. TOP and RLM do not include the
Reports definition; server-side `ExecuteSummary` rejects invalid view
definitions and they no longer display a Reports tab. CIA legacy
`view=reports` redirects to its authorized Audit Para Snap reports as
previously implemented. Existing report reader permissions are unchanged.

## 5. RLM dark theme

The RLM Razor parent now loads `regional-audit-pulse.css` unconditionally and
applies `exec-regional` to **every** RLM tab. RLM charts, management tables
and nav controls thus inherit Dark Design 3 rather than returning to a
light theme outside `view=regional`. Group Management retains its green
design, and TOP/CCO retain their approved distinct themes.

## 6. Group unit comparisons

Confirmed frontend blocker: management `_RoleReporting` partial and JS
were not rendered when `RoleDashboards:OracleReadEnabled` was unset;
the backend then rejected role APIs with 503 before Oracle was called.
This patch loads the existing **page/API/scope-authorized**
`GetRoleUnitComparison`, `GetRoleFollowups` and regional heatmap, regardless
of an environment switch. The Oracle package's existing
`P_GET_ROLE_UNIT_COMPARISON` keeps current status, root hierarchy and
direct-child grouping unchanged until results can be checked against the full
production dataset. No fabricated comparisons will be shown.
**If the verified live query returns zero units or a 500/503, further
source/hierarchy investigation is required.** Group root selection is still
mandatory; no bank-wide bypass was added.

## 7. Audit Para Snap

The `AuditParaSnapEnabled` deployment switch is removed from the actual
request paths: JavaScript will fetch selected-date data, the API no longer
returns the environment-only `not_deployed` 503 and both KPI/reports scripts
are loaded on their authorized tabs. IAS session, page 1000304, exact GET API
grant, date validation, organizational scope and oracle exception handling
remain. The top header is now three separately aligned lines:
**ZTBL · INTERNAL AUDIT REPORTING / Audit Para Snap / Authorized scope**,
with responsive flex styling.

Historical balance and cycle limitations remain as estimated/N/A with
the source's factual history quality, not a fabricated value.

## Compiling safely

1. **Before deploying any app change**, verify that all five newly created
   executive pages and 25 GET endpoints have active privileges only for
   IAS Super User role/group 1. See separately prepared PR #325; it is NOT
   implied by this patch and must be independently confirmed in the live DB.
   Stale permission cache/session state requires fresh authorization testing.
2. DBA should compare the installed live Oracle view and package definitions
   to the canonical source file
   `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql` in this branch. **Do not execute the
   whole file**; doing so would replace unrelated dependencies. Compile just
   the revised `V_EXEC_PARA_BASE` view and `PKG_EXEC_DASHBOARD`
   **package body** according to the existing F8 compilation sequence, after
   backups. Package specification and procedure signatures are unchanged.
3. Validate `USER_ERRORS`, `USER_OBJECTS` and the read-only
   `verify_executive_seven_issues_F8_20261019.sql` queries in PL/SQL
   Developer (one SQL or block at a time). For TOP Position choose a short
   date range and compare with the same P_GET_KPI_LIFECYCLE cursor.
4. Deploy/recycle the ASP.NET application in an approved change window. Test
   all RLM view themes, Reports navigation (TOP/RLM direct URL must fail),
   management organization root, Snap period pages and PDF report access.
5. **Security negative test:** an ordinary CIA, Group, CCO, GM/Regional user
   must receive 403 on the newly restricted API actions, even if they can
   access legacy Organization Structure. Super User should access the pages
   and retrieve real data, with 0 vs NULL clearly differentiated.
6. Independently review age-source anomalies and historical reconciliation
   before opening access to any non-Super User role.

This PR is separate from the CIA Command Center environment/KPI patch
**PR #326**. If both are merged, check for conflicting changes in shared
controllers and re-run authorization/QA. Neither PR changes production
automatically.

# CIA Command Center and Audit Para Snap

Source delivery based on main after PR #317. No Oracle objects, permissions,
feature flags or production application have been deployed.

## Independent pages and authorization

- `/ExecutiveOverview/CIA`: approved navy/teal Command Center, five cards,
  ten issue categories, IAS audit-period ID selector and leadership/data notes.
  Shared executive tabs and Start/End dates are removed from this page.
- `/ExecutiveOverview/AuditParaSnap`: light slate/blue dashboard with six
  views: At a Glance, Audit Para Position, Settlement & Performance, Risk,
  Aging & Compliance and Reports. Page title: **ZTBL | Audit Para Snap**.
- Each page links to the other. Old CIA URLs with a recognized explicit
  `view=glance|position|settlement|risk|aging|reports` redirect locally to
  Snap, retaining valid dates, organizational filters and report selection.
  Invalid filters/views are rejected.
- Snap requires IAS role 3 or role 1 with the verified Super User flag,
  its own page grant **1000304**, and the exact GET API grant for each call.
  The Super User must pass page and API checks. Other role mappings cannot
  create Snap bank-wide access. Revoked sessions and incompatible hierarchy
  selections are rejected.

## Common date period

Start Date and End Date are required ISO calendar dates on every Snap view.
The default/reset is January 1 through yesterday, using Pakistan calendar
dates; on January 1 this means the previous complete calendar year.
The page explicitly displays the selected period. Apply performs a GET reload;
tab links, cascading entity filters, critical-item entity drill-downs, report
selection and exports retain the range. Changing inputs invalidates old
results until Apply. Snap dates are never used as a CIA audit-period ID.

Server and client reject reversed, partial and future ranges. Analysis is
bounded to fewer than 1200 months between the start/end months (at most 1200
trend rows). Oracle truncates calendar inputs and applies `>= StartDate`
and `< EndDate + 1`. Today is an as-of-now position, not completed end of day.
Confirm the Oracle server calendar agrees with the Pakistan reporting day.

## Oracle contracts and evidence

Canonical source: `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql`.
Compile its package specification and body separately after review. The
older standalone F8/snapshot package-body variants do not contain Snap.

**Existing `P_GET_KPI_LIFECYCLE` is unchanged**, including its signature,
nullable results and Y/E historical verification indicators. Snap uses it for
all nine required KPIs via a shared typed C# reader; TOP retains its original
entry-point restriction and behavior. No duplicate lifecycle procedure exists.

Corrected existing CIA procedures:

- `P_GET_CIA_EXEC_KPIS`: distinct planned/audited entity counts now also
  require engagement/plan entity agreement. A1 status 8/9 observations
  require concluded engagement status 14, the selected engagement period,
  and identification dates within that IAS period. Awaiting reply remains
  observation status 2, current state across periods.
- Delayed reports retain the existing `PKG_RPT.P_DELAY_AUDITS` proxy:
  stages 4–13, audit-end +15 calendar days, excluding concluded engagements.
  Conducting-team proxy uses distinct commenced teams in stages 4–11,
  excluding concluded engagements; stages 12/13 are draft/final reporting
  in `PKG_HD`. **These two values remain N/A until the live SLA and stage
  definitions are verified and explicitly approved.** If those definitions
  differ, correct the existing SQL before enabling the verification flag.
- `P_GET_CIA_ISSUE_NATURE`: selected-period eligible observations,
  distinct observation rows, approved mapping codes only and deterministic
  match priority. Invalid/unmapped mappings are disclosed, never guessed.
  All ten category cards always render; the client rejects inconsistent
  `SUM(ISSUE_COUNT) + UNMAPPED_ISSUES = TOTAL_ISSUES`.

Added non-KPI procedures only:

- `P_GET_SNAP_DISTRIBUTION`: selected-month settlements (including zero
  months), risk/aging distributions on the same population as lifecycle
  closing, and current-only cycle distributions. Past cycle counts return
  NULL/N. Risk/age positions return E for past dates, Y for current position;
  trend returns E because latest qualifying settlement dates are incomplete
  event history after reopenings.
- `P_GET_SNAP_CRITICAL`: same date/scoping basis; maximum 250 rows
  (UI requests 100). Past lists use high-risk or >365-day criteria and are
  Estimated. Historical cycles and last movement are N/A; cycle-only past
  criticality cannot be established. Today's list also includes >3 cycles.

All filters use authenticated scope and the existing reporting hierarchy.
No other executive procedure signature or body changed.

## Historical limitations

We inspected `V_EXEC_KPI_LIFECYCLE`, its modern/legacy source views,
engagement conclusion/re-conclusion milestones, and
`T_AU_PARAS_STATUS_CHANGE_LOG` requests versus `AUTHORIZED_ON` events.
`PKG_HD` demonstrates that reopening clears the latest settlement timestamp.
The supplied evidence does not establish complete initial-state, risk,
cycle or historical hierarchy histories. Compliance-history `COM_STATUS`
is not a para-status ledger.

Therefore historical opening/closing/risk/age and the lifecycle over-three-
cycle KPI remain explicitly Estimated. They use latest master status/dates
and present risk, cycles and organization as documented by lifecycle.
Missing dates, earlier settlements after reopening, transfers and changed
classification can alter past counts. Historical cycle distributions are
N/A. No current-state summary is silently presented as a past snapshot.
Reports A–E retain their existing independent SQL/data/PDF behavior and
indicative-history warnings; their totals need not reconcile exactly to
lifecycle because the existing report recognition/adjustment rules differ.

## Controlled activation

1. Review/compile canonical package spec/body in an approved Oracle test
   schema. Existing lifecycle/source views and approved
   `T_IAS_CIA_ISSUE_NATURE_MAP` must exist. No mapping IDs are seeded here.
2. Manually run `AIS/Docs/sql/verify_audit_para_snap.sql`. Resolve
   `USER_ERRORS`, reconcile live counts/scopes and approve workflow rules.
3. Review the permission script before DBA execution. It reserves page
   1000304 with ID/path/key collision checks, serializes registration and
   creates the eight new `/api/AuditParaSnap/*` GET APIs **inactive**.
   Only verified CIA/Super User roles 3/1 receive new grants. Existing Report
   A–E routes and their activation script remain unchanged.
4. After approval activate the intended new API records and set
   `RoleDashboards:AuditParaSnapEnabled=true`. Missing flags default false.
   Existing `RoleDashboards:CiaCommandCenterEnabled` remains separately
   controlled. Set `RoleDashboards:CiaWorkflowDefinitionsVerified=true`
   only after approving the delayed/conducting definitions.
5. In authenticated IAS test sessions verify both pages for CIA/Super User,
   denied roles/removed grants/revocation, all date/filter/tab/drill-down/
   report/PDF paths, real counts, responsive layout and existing dashboards.
   Production deployment and merging still require approval.

## Focused verification completed

- One `dotnet build AIS/AIS.csproj --no-restore -v minimal`: **passed,
  0 errors, 57 warnings** in existing code; Razor pages compile. Restore
  used installed/cached packages. Generated tracked obj changes were restored.
- `node --test tests/executive-dashboard/approved-design-parity.test.cjs`:
  **5 passed**.
- `node --test tests/executive-dashboard/audit-para-snap.test.cjs`:
  **7 passed**. Includes all Oracle spec/body signatures, byte-equivalent
  existing procedure bodies (except the two corrected CIA procedures),
  historical evidence rules, permission registration and protected artifacts.
- `tests/executive-dashboard/audit-para-snap-policy.ps1`: **23 compiled
  date/default/navigation checks and 18 compiled-controller access checks
  passed**, using the already-built AIS.dll. No extra project build or
  Oracle connection. Access tests exercise the real controller and session
  code with in-memory permissions/session; CIA/Super User authorization is
  confirmed through the date-validation gate, not a live database read.
- Changed JavaScript syntax checks and `git diff --check`: passed.

Pending environment checks: Oracle compilation/data reconciliation and
authenticated IAS page/browser/export verification. No live Oracle or IAS
test session was available. Source review is not an Oracle compiler result.

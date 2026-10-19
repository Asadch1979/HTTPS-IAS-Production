# CIA Command Center: remove environment gates; display nullable Oracle results

## Why this patch exists

On 19 October 2026 the production Command Center rendered
"Data loading is disabled in this environment." The independently
configured `RoleDashboards:CiaCommandCenterEnabled` flag was absent
from `appsettings.Production.json`, so ASP.NET Core evaluated it as
false. The Razor view emitted `data-command-enabled="false"` and the
JavaScript aborted before making Oracle API requests. A second
identical flag check in `ExecutiveDashboardController.ExecuteSummary`
would have returned HTTP 503 if the frontend check were bypassed.

This duplicate release switch was originally added to defer the Command
Center until its IAS package/view, classification mapping and grants
were reviewed. It is not an IAS business rule.

## What changes

- Delete `RoleDashboards:CiaCommandCenterEnabled` as a functional
  setting. It no longer appears in the page model, Razor attribute,
  JavaScript request gate, controller request gate or development JSON.
  Even if an old server environment variable remains, the code ignores it.
- The client loads both CIA endpoints whenever the selected IAS audit
  period is present; it does not use a client-controlled flag for access.
- The backend still validates authenticated/session state, verified
  CIA perspective, required view, selected audit period and scope,
  page permission and independent API permission before Oracle reads.
  AJAX error isolation and mapped/unmapped issue count checks remain.
- Remove the remaining CIA environment-based
  `RoleDashboards:CiaWorkflowDefinitionsVerified` mask. The Oracle cursor returns the CIA KPI values without environment masking.
  The C# reader uses nullable numeric conversion for **all six numeric
  counts** (planned, audited, delayed, A1, awaiting response, active teams):
  Oracle `NULL` displays as `N/A`; Oracle `0` displays as **0**; positive
  counts display unchanged. Plan coverage is already nullable. The UI labels these as **under production
  validation**, never automatically "verified" merely because populated.
- TOP lifecycle nine KPIs, Reports A-E, Audit Para Snap, GROUP/CCO/RLM
  parents and their separate loading logic are unchanged. A request
  to remove those other deployment switches would need a separate
  review and release; it is not hidden inside this urgent CIA fix.

## Security prerequisite for the planned Super User-only pilot

Removing the environment loading flag makes the Command Center available
to **any authenticated user who already has the CIA perspective plus
page 1000303 and matching GET API grants**. Therefore, before rolling
out this patch, verify the live IAS group/page and API grants are truly
Super User-only (role/group 1). See the separate Super User-only
permission script proposal **PR #325**, which may not yet be merged or
executed. This patch does **not** itself modify those permissions.

Both routes must be explicitly allowed only to Super User:
`/api/ExecutiveDashboard/GetCiaCommandKpis` and
`/api/ExecutiveDashboard/GetCiaIssueNature`. Check active master/API
grants AND current session permission caches. Test an ordinary CIA
role (3) in a fresh authenticated session and confirm **403**, not 200.

## Next maintenance window: verification

1. Review and merge this source-only patch after confirming Super User
   permissions; deploy the rebuilt application in the next approved window.
   No Oracle package/view/table change or data migration is required for
   this correction.
2. Verify `/ExecutiveOverview/CIA` renders without the
   `data-command-enabled` attribute. A selected `T_AU_PERIOD` should
   initiate both GET requests without an environment setting.
3. Verify HTTP 200 from both endpoints for authorized Super User;
   401/403 for a revoked session, missing permissions, or ordinary roles.
   500/503 now means a genuine service/Oracle problem rather than a
   feature-flag deployment gate; use IAS server logs and request IDs.
4. Verify CIA source data and audit-period totals,
   `SUM(ISSUE_COUNT)+UNMAPPED_ISSUES=TOTAL_ISSUES`,
   and partial checklist mapping alerts.
5. Confirm both workflow KPIs reflect the actual IAS cursor:
   `NULL` gives N/A, `0` gives 0, and positive values give their
   returned counts. Check this rule for all six numeric counts and
   nullable coverage percentage. Compare source engagement stages and
   deadline definitions with production data *before* expanding to other roles.
   Do not claim these business rules are validated merely because they
   returned a number.
6. Run focused frontend `cia-command-loading.test.cjs` and compiled
   `cia-command-policy.ps1` / `CiaCommandAccessChecks.cs` checks.
   Conduct an authenticated staging/smoke test before the production patch.

**GitHub source edit only.** Neither IIS, running production ASP.NET Core
process nor the live Oracle schema was changed by this commit.

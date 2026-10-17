# CIA Command Center / Audit Para Snap checkpoint

Branch: `codex/cia-audit-para-snap` (attached isolated worktree).
Latest committed baseline: `255f466f70e5ab5e512d8464b910c638c2b06f93` (PR #317), confirmed against GitHub main.
This file is committed with each milestone. Its recorded SHA identifies the preceding milestone; use `git log -1` for the commit containing this checkpoint (a commit cannot contain its own SHA).

## Completed
- Verified latest main and created a clean isolated feature branch. Original checkout's appsettings/build artifacts remain untouched.
- Inspected CIA cursors/readers, lifecycle and source views, permissions, shared Razor, reports and workflow evidence.
- Enabled supported hourly thread heartbeat `resume-cia-and-audit-para-snap-implementation`; persistent goal is active. It checks usage and checkpoint before continuing and must be disabled at completion.

## Remaining, in priority order
1. Correct CIA period/finalization and classification uniqueness; add dated Snap breakdown/critical cursors, preserving lifecycle signature.
2. Dedicated Snap models/readers/APIs with exact page/API checks and date validation; retain existing role APIs.
3. Independent CIA and Snap pages, six Snap views and common dates; legacy bookmark redirects, approved visuals and Report A–E protection.
4. Collision-safe page/API/role script and runtime page mapping.
5. One .NET build, JS syntax and focused boundary/permission/regression checks; manual SQL verification script.
6. Review, commit, push and open one PR; attach it. No merge or database deployment.

## Oracle modifications / errors
None yet. No Oracle compilation or deployment performed; no live Oracle session is available.

## Evidence / unresolved limits
- PKG_RPT.P_DELAY_AUDITS uses stages 4..13 and audit-end +15 days. Production deadline approval must still be verified before publishing.
- PKG_HD identifies stages 12/13 as draft/final reporting, so 4..12 is not a verified conducting-audit definition.
- V_EXEC_KPI_LIFECYCLE ISSUE eligibility requires engagement status 14; CIA A1 currently omits this and engagement period match.
- T_AU_PARAS_STATUS_CHANGE_LOG stores requests at CREATED_DATE and authorization at AUTHORIZED_ON. PKG_HD shows reopen clears latest settlement date. Completeness of historical state/risk/cycle/organization logs is unverified. Do not infer full history from compliance COM_STATUS.
- Lifecycle estimates ('E') use latest master status/risk/cycles and latest dates; retain and disclose these indicators.
- Shell Git fetch cannot connect to github.com; GitHub connector confirmed main SHA. Publication may need connectivity resolution.

## Review next
AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql; AIS/DBConnection.CiaCommandCenter.cs; AIS/DBConnection.ExecutiveDashboard.cs; AIS/Controllers/ExecutiveOverviewController.cs; AIS/Controllers/ExecutiveReportsController.cs; AIS/Services/PageIdResolver.cs; AIS/Docs/sql/executive_dashboard_permissions.sql.

## Tests
Completed: read-only contract/workflow review. Pending: all implementation checks, one build and manual Oracle verification.

## Next exact step
Update existing CIA procedures and add dated Snap non-KPI cursors to canonical PKG_EXEC_DASHBOARD.sql. Reuse P_GET_KPI_LIFECYCLE unchanged for nine KPI cards. Then commit this SQL milestone with checkpoint.

## Continuation / usage
Use supported get_usage_limits (primary window = 300 minutes); latest check 95% remaining. Prepare checkpoint at 5%; stop substantive work before 2%; commit all useful work. On continuation read this file, verify branch/HEAD, check allowance, and resume only the next incomplete task. Never deploy Oracle or merge.

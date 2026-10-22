# CIA Command Center and Audit Para Snap implementation

## Working state
- Worktree: `C:/Users/Asad/.codex/worktrees/cia-snap-enhancements/HTTPS-IAS-Production`
- Branch: `codex/cia-snap-enhancements`, based on verified GitHub main `8f18681f2c9d795b7c92fcf854191f7c578aa9ef`.
- Original checkout has pre-existing SQL/obj/.vs changes. Leave it untouched.
- Git shell cannot resolve github.com. Imported the two missing upstream commits using connected GitHub data; verified exact blob, tree and commit SHA hashes before advancing feature branch. No remote mutation.

## Completed
- Stage 1 implementation: CIA four controls; actual IAS period IDs; inclusive Position as on, Pakistan yesterday/historical defaults; server/Oracle date validation; audit division/department scope shared by both CIA procedures.
- Coverage, delayed reports, awaiting response and teams preserve established current-state definitions and are explicitly labeled current. A1 and Nature dated activity honor cutoff. No invented historical workflow status.
- Application build: `dotnet build AIS/AIS.csproj --no-restore -v quiet`: 0 errors, 57 existing warnings.
- JS syntax check passed. Oracle has NOT been compiled or deployed. Production reconciliation remains required.

## Pending (in user-specified order)
1. Finish focused CIA tests and synchronize F8 deployment artifacts; commit Stage 1.
2. Snap organizational selectors and independent audit-portfolio intersections through lifecycle/distribution/critical and Reports A–E, preserving scoped authorization.
3. Remove critical register from glance only.
4. Department comparison, then settlement analytics, secure para text, aging percentages, report-column cleanup.
5. Focused security/regression tests and final deployment documentation.

## Oracle deployment (draft, not production-ready)
1. Existing `V_IAS_ENTITY_REPORTING_NAMES`, lifecycle, para base, CIA nature/checklist/rectification dependency views must be valid.
2. `snap_audit_division_scope.sql` spec then body (existing approved mapping).
3. `PKG_EXEC_DASHBOARD.sql` spec then body, with existing dependent views retained.
4. Updated reports package when Stage 2 requires it.
5. Inspect USER_ERRORS and reconcile known scopes on staging before deploying application.
- Amended CIA procedures: `P_GET_CIA_EXEC_KPIS`, `P_GET_CIA_ISSUE_NATURE`; append optional `P_POSITION_AS_ON`, `P_SNAP_DIVISION_CODE` after existing parameters.
- Existing exact CIA API grants retained; no new endpoint in Stage 1.

## Quota and continuation
- Official five-hour usage last observed: 41% used / 59% remaining on 2026-10-22.
- Native same-chat heartbeat ACTIVE: `resume-ias-cia-and-snap-implementation`, every five hours. Checks official quota, reads this file and resumes first pending work. Requires local Codex host to remain available; scheduling does not guarantee execution or quota.
- Stop before 2% remaining; checkpoint near 5%. No unsupported polling.
- Restart prompt: `Continue the CIA/Snap task from AIS/Docs/CODEX_DASHBOARD_PROGRESS.md in the cia-snap-enhancements worktree; check official quota first.`

# IAS corrective review — 17 October 2026

Baseline: fetched `origin/main`, `665ef56e`, including PRs #310, #311 and #312. Corrective branch: `codex/ias-corrective-stabilization`. No Group/Divisional dashboard enhancements are included.

## Confirmed defects corrected

- All Entities (-1) now uses the existing report response/totals in a compact summary. Individual entity types retain the grid. Both layouts are hidden during loading, empty/error responses and selection changes; old DataTables are destroyed and stale AJAX responses ignored.
- `pkg_rpt.R_IAS_PARA_POSITION` is retained. The old standalone SQL is marked reference-only; the deployment instructions no longer direct users to compile it over production functionality. The packaged procedure is absent from the repository snapshot, so its live signature/result aliases still require verification.
- The historical migration no longer overwrites active/reopened audits using an old conclusion. Only currently concluded audits with one status-14 log and no recorded reversal are candidates; ambiguous histories are held for manual reconciliation.
- Finalization setters cannot re-settle an already concluded audit without an authorized reopen. They set ISSUE_RECTIFIED only during active audit stages 4–13. Applicable reversals clear source and copied flags. Conclusion copies only settled observations' flags and refreshes existing modern copies on re-conclusion.
- `P_audit_Concluding` previously inserted a status-14 log even when pending observations prevented conclusion. The log is moved inside the actual success branch. Historical false logs are not deleted automatically.
- ISSUE lifecycle rectification uses the latest conclusion after recorded reversal and the stored flag, excludes dropped status 23, and retains the existing strict timestamp cutoff. Modern PARA recognition deliberately keeps the original first-conclusion logic: re-conclusion must not re-date existing balances. Issue Identified and other executive KPI formulas remain unchanged.
- RLM now rejects invalid role/Super User context and ambiguous organizational roots. Its selectable entities match the six reporting levels used by the executive SQL. Existing database-backed page/API permissions remain mandatory; no guessed role IDs or new bank-wide access are added.
- Explicit `view=glance` remains accessible; only an omitted view defaults regional users to Regional Audit Pulse. The named regional view also validates the login GM/region posting in its APIs. Branch comparisons include existing IAS branch types 6/28. Reporting-office entity options now follow the same descendant semantics as RLM SQL; non-RLM filter behavior is retained.
- Team Leader workbench assignments are intersected with current active leader tasks before the workflow builder selects or writes active engagement context. Unauthorized/member/inactive selections are denied first; ordinary workflow selection remains unchanged.
- Comparison requests freeze their filter selection for each batch, prevent overlapping button runs and reject redirected/non-JSON responses. The intentional two requests (KPI + risk) per entity remain bounded; no new endpoint was introduced.
- Only the four accidental publish history/runtime-cache changes from `665ef56` are restored to the parent versions. `Page ID.xlsx` and the production DBConnection procedure name are preserved.

## Manual database deployment only

1. Run the read-only `sql/IAS_corrective_review_F8.sql` in the owning schema. Confirm deployed `PKG_RPT.R_IAS_PARA_POSITION` arguments, cursor aliases and package/view compilation errors. Do not deploy the legacy standalone report procedure.
2. Review backups and current deployed package source against the targeted diffs in `PKG_AR.sql`, `PKG_AD.sql` and `PKG_HD.sql`; compile approved package bodies only. The two ISSUE_RECTIFIED columns must exist first. Do not replace unrelated live package changes from an old repository snapshot.
3. Review the changed historical migration separately. Its DDL commits implicitly; historical DML remains uncommitted. Do not rerun it blindly if an earlier broad backfill has already been committed. Reopened/re-concluded, missing/failed-log, timestamp-tie and flag-mismatch cases need an approved observation-ID-specific reconciliation script.
4. After flag reconciliation, compile the revised `V_EXEC_KPI_LIFECYCLE` and verify dependent objects. `V_EXEC_KPI_SRC_A` has comments only; its PARA calculations are unchanged. The production report package body must be reconciled with the same issue rules without altering its other calculations.
5. Validate live first conclusion, blocked conclusion, post-conclusion finalization denial, authorized reopen/re-conclusion, observation reversal, dropped observations and report counts by observation/COM_ID. No database objects, historical rows or grants were changed by this review.

## Seven procedure compatibility findings — deferred integration

| Procedure | Compatibility / missing integration |
|---|---|
| PKG_TEAM_LEAD_DASHBOARD.P_GET_SUMMARY | P_PPNO + cursor; engagement/review counters do not match ExecutiveKpiSummary. No C# reader/API is wired. |
| P_GET_ENGAGEMENTS | P_PPNO + cursor; ENG_ID/ENGAGEMENT_STATUS_ID/TASK_STATUS_ID differ from the existing dropdown contract. Stage mapping remains unverified. |
| P_GET_WORKPAPERS | P_PPNO, optional P_ENG_ID + cursor; requires installed V2 tables and a new typed reader. Counts are not an approved completion denominator. |
| P_GET_OBSERVATIONS | P_PPNO, optional P_ENG_ID, P_MAX_ROWS + cursor; pending statuses 2/3 only, not draft/finalized/outstanding totals. |
| PKG_EXEC_DASHBOARD.P_GET_ROLE_ENTITY_HEATMAP | P_ROOT_ENTITY_ID, audit period, entity type, max rows + cursor; HIGH_RISK/MEDIUM_RISK/LOW_RISK/UNRATED need a dedicated mapping. Existing office/audited-by filters are not equivalent. |
| P_GET_ROLE_UNIT_COMPARISON | Root + audit period + cursor; new unit-level columns require a typed model. It does not accept all existing organizational filter parameters. |
| P_GET_ROLE_FOLLOWUPS | Root, audit period, max rows + cursor; overlaps critical-item columns but includes NORMAL open paras and extra status fields. It cannot replace the existing critical-only API unchanged. |

All seven are SQL source only and remain unconnected to the current dashboard readers/APIs. They must not be silently substituted. Working Papers V2 is disabled in WorkingPapersController; the workbench's Review/Reporting stage mapping and N/A review/finalization/completion metrics remain deferred. Existing CIA/TOP/CCO/GROUP/report procedures and API contracts are retained.

## Verification and remaining issues

Passed: `dotnet build AIS.sln --no-restore` after dependency restore (0 errors, 59 warnings: existing code warnings and scheduler integration-project assembly-version conflicts); JavaScript syntax; focused Para Position smoke checks for totals, summary/grid switching, DataTables lifecycle, stale results, and empty/error/malformed responses; source review of role/page/API/root/assignment checks; diff/whitespace checks. No test suites were run. An additional hierarchy smoke check was not run after execution approval was declined.

Live Oracle metadata access was attempted read-only using the existing configuration but failed (Oracle error 50000). No live signature, package compilation, count reconciliation or authenticated negative authorization tests are certified.

- **Critical:** No additional confirmed Critical defect remains in the corrected source; this is not production sign-off.
- **High:** DBA approval/compilation and deployed report signature/body reconciliation are outstanding. Existing ambiguous historical flags/logs need manual reconciliation. Strict timestamp ties and unlogged reopenings cannot be reconstructed reliably. Live denied-role, sibling/foreign entity, revoked-session and inactive/member assignment requests must be exercised before deployment.
- **Medium:** Seven procedure integrations, unverified stages/N/A metrics, live branch category reconciliation and disabled V2 workpapers remain deferred. Existing scheduler-test assembly-version warnings remain outside this corrective scope.

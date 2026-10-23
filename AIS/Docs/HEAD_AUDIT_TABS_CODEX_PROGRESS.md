# Head Audit tabs and para format progress

Branch: `codex/head-audit-tabs-and-para-format`
Base: `fa64939fe1764e7315407d836326da1676e2e20b`, latest remote main verified through the GitHub connector on 23 October 2026. Shell GitHub network fetch is unavailable.

## Completed checkpoints

| Commit | Work |
| --- | --- |
| dbaa44f3 | Reporting date, validation, workflow-log cutoff and explicit historical limitations |
| 4ede1fb1 | Four independently queried/rendered tabs and authorized memory caching |
| ea3477b4 | Distinct PlanId totals with no duplicate planned days |
| fc1bdf26 | Five scoped local ExcelJS exports, typed cells and formula injection protection |
| 649f6b9c | Allowlist sanitized HM-style read-only para document |

Regression work ready to commit: separate unknown-history plan counts; ambiguous status-10 handling; period-scoped workpaper counts; cheap authorized office options including offices without records; assignment-bound tab/export consistency checks; formatted modal QA; focused tests and review/deployment notes in HEAD_AUDIT_TABS_REVIEW.md.

## Tests

- Application builds: zero errors, 57 existing warnings.
- C# policy/date/totals/sanitizer checks passed, including plain, formatted, malformed, encoded, stored active content and long observations. Ten compiled Head Audit access/cache-probe gates passed without Oracle. Run under installed .NET 9 using `dotnet --roll-forward LatestMajor tests/head-audit-monitoring/bin/Debug/net8.0/HeadAuditMonitoring.Checks.dll` because the existing app dependency graph references System.Text.Json 9.
- 16 focused Node checks passed, including actual XLSX roundtrips for all five tables and SQL/F8/spec/body consistency.
- Headless Edge fixtures passed all four tab loads, five downloaded XLSX files, rows/metadata/totals, cache reuse, assignment/access failure clearing and period changes. HM/reference and corrected para desktop/mobile screenshots inspected. No authenticated live UAT claimed.
- Existing CIA/Snap policy script: 9 + 9 + 4 + 26 + 4 checks passed, including compiled Snap access gates.
- Broader executive suite: 14 failures and 3 skips reproduced on isolated unchanged main; no additional failures on this branch. Temporary baseline checkout removed.
- Unrelated dashboard/Oracle/export-gate and exact para eligibility source files verified unchanged against main.

## Validation boundaries / blockers

Oracle compilation, live query traces/timings, historical workflow sample reconciliation, equivalent-condition KPI comparison, live unauthorized office/engagement/COM_ID checks and authenticated Visual Studio UAT were not performed. They require the controlled IAS/Oracle environment. Package and F8 source are synchronized; compilation order and prerequisites are in HEAD_AUDIT_TABS_REVIEW.md. No database/application deployment or merge performed.

Pre-existing appsettings.json, NuGet obj changes and .vs files are excluded from commits and preserved.

## Scheduler and credit

One native heartbeat: `resume-head-audit-tabs-and-para-formatting`, currently ACTIVE every five hours. No old resume-ias-cia-and-snap-implementation schedule was present. Official allowance last checked at 86% remaining; 5% checkpoint / stop-before-2% rules remain applicable. Scheduler must be paused and verified before final handoff.

## Next exact step

Run final focused/browser/build checks after regression edits; commit source/tests/review notes. Pause the single heartbeat using supported controls and verify PAUSED, record final status here, commit final documentation, and hand off the local feature branch. Live Oracle/UAT remains an explicit reviewer prerequisite, not unattended work to fabricate.

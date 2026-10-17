# Dashboard delivery dependencies

## Phase 1 — Team Leader

Entry points: `/FieldAudit/AR_Dashboard?workbench=true` and `/ManagementAudit/MA_Dashboard?workbench=true`, linked from existing dashboards for assigned Team Leaders. Existing page grants and workflow step grants apply. No new API or database objects.

Sources: `pkg_ar.P_GET_AR_DASHBOARD_DROPDOWN` (session PP number, explicit `ISTEAMLEAD=Y`) and `pkg_ar.P_GetTaskList` (intersection with those assignments and the session PP number). Active means joined task status 2; status 1 is pre-joining. Upcoming means recorded audit end date today through seven days ahead, inclusive, in Pakistan time. Dates are engagement dates, not remediation due dates. Other task statuses remain in the assignment table rather than being guessed into workflow lanes.

Missing: verified Review/Reporting state mappings, workpaper completion denominator and submission/review ledger, observation draft/review/finalized counts and outstanding action semantics. These display N/A. Candidate sources to validate are `T_AU_PLAN_ENG`, `T_AU_AUDIT_TEAM_TASKLIST`, `T_AU_TEAM_MEMBERS` and their status lookup; workpaper/reviewer and observation status joins need DBA confirmation before proposing executable SQL. No unverified query is wired into the application.

Focused verification: build and whitespace checks; inspect explicit leader filtering, session revocation, foreign engagement rejection and existing permission-filter coverage. Live session/Oracle reconciliation remains a deployment check.

## Phase 2 — Regional Audit Pulse

`/ExecutiveOverview/RLM` defaults to the regional view only for an unambiguous login organizational type 21 (GM) or 5 (Region). The existing `/Dashboard/entity_wise_obs` page grant and each existing executive API grant remain mandatory. Other RLM organizational types keep their existing views. No browser root or bank-wide fallback is added.

The four KPIs, risk/aging/cycle/trend charts and bounded priority details reuse `PKG_EXEC_DASHBOARD`. Branch tiles select organizational type 6 from authorized descendants and the active filters. Each tile calls the existing, independently authorized KPI/risk APIs (six tiles per user request, two concurrent calls). No totals are inferred from the capped critical list. Highest recorded risk is explicitly labelled; high-risk share uses all open paras. Each tile drills down with the same filters. Nested entity scopes are never added together.

No new procedure is required for these comparisons. A future bulk cursor could reduce round trips; verify counts against the existing procedures before replacing this bounded implementation. Complete pending-response/decision queues remain unavailable. Exact ages are available only for the bounded critical-item list; aggregate aging bands cover all open paras. Existing procedure cycle bands are preserved.

Focused build passed. Live Oracle reconciliation and GM/region permission smoke checks require authenticated deployment sessions.

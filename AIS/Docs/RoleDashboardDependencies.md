# Dashboard delivery dependencies

## Phase 1 — Team Leader

Entry points: `/FieldAudit/AR_Dashboard?workbench=true` and `/ManagementAudit/MA_Dashboard?workbench=true`, linked from existing dashboards for assigned Team Leaders. Existing page grants and workflow step grants apply. No new API or database objects.

Sources: `pkg_ar.P_GET_AR_DASHBOARD_DROPDOWN` (session PP number, explicit `ISTEAMLEAD=Y`) and `pkg_ar.P_GetTaskList` (intersection with those assignments and the session PP number). Active means joined task status 2; status 1 is pre-joining. Upcoming means recorded audit end date today through seven days ahead, inclusive, in Pakistan time. Dates are engagement dates, not remediation due dates. Other task statuses remain in the assignment table rather than being guessed into workflow lanes.

Missing: verified Review/Reporting state mappings, workpaper completion denominator and submission/review ledger, observation draft/review/finalized counts and outstanding action semantics. These display N/A. Candidate sources to validate are `T_AU_PLAN_ENG`, `T_AU_AUDIT_TEAM_TASKLIST`, `T_AU_TEAM_MEMBERS` and their status lookup; workpaper/reviewer and observation status joins need DBA confirmation before proposing executable SQL. No unverified query is wired into the application.

Focused verification: build and whitespace checks; inspect explicit leader filtering, session revocation, foreign engagement rejection and existing permission-filter coverage. Live session/Oracle reconciliation remains a deployment check.

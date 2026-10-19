# Head Audit Department — Audit Teams Monitoring

Entry: `/HeadAuditDepartment/AuditTeamsMonitoring` (page **1000401**).
The existing blue workbench is now a supervisory page. Field Audit and Management Audit keep their operational dashboards; the `workbench=true` entry and links have been removed. Their workflow selection, assignment restrictions and approval actions are unchanged. The original `PKG_TEAM_LEAD_DASHBOARD` remains unchanged.

## Authorization and verified sources

The checked-in `Data_Structure/Complaince Summary.txt` identifies roles/groups **5** Head FAD, **6** Head Management Audit and **7** Head ISAD; **1** is Super User. `V_HEAD_AUDIT_ROLE_DEPT` additionally checks active role descriptions and actual department names in Oracle. FAD entity **112243** and its subordinate audit-office relationship are supported by existing `PKG_HD` and reporting queries. Management/ISAD entity IDs are resolved from exact department names rather than invented IDs. The registration script requires exactly one department match per role and equal verified role/group IDs; a mismatch stops registration for DBA investigation. No Head CAD role is identifiable in the checked-in catalogue: role 33 “Commercial Audit” is not assumed to be a departmental head. CAD/other roles remain excluded until separately verified.

Only the authenticated session supplies user ID, role ID, department entity and context assignment ID. The package rechecks the active user, active role, selected context/effective dates (or the legacy posting plus role assignment), then department/engagement membership on every read. FAD uses engagement `AUDITBY_ID` plus `T_AUDITEE_ENTITIES_MAPING_FAD`; Management/ISAD use their own `AUDITBY_ID`. Business reporting relationships never expand this portfolio. Ongoing statuses **4–13** follow existing `PKG_AR` query predicates; names come from `T_AU_PLAN_ENG_STATUS`.

Super User testing uses an explicitly authorized session context posted to a verified audit department. There is no request parameter to select or impersonate an arbitrary department or leader. An ordinary Super User context outside these departments receives 403; assign/select an approved test context through the existing context administration workflow.

Both page actions require the dedicated page grant. All five `/api/HeadAuditDepartment/` GET APIs (`Engagements`, `Work`, `Evidence`, `ResponseEvidence`, `Report`) additionally require their exact API grant. Authentication, session revocation, role eligibility, page grants and scope checks apply even to direct URLs. Responses are not cached. File downloads enforce both the selected engagement and file relationship; return attachments rather than rendering executable content inline. Review text is Razor-encoded.

## Saved work and metrics

- Plan/criteria: `T_AU_PLAN`, `T_AUDIT_CRITERIA` and its actual approval-status catalogue. Team/task records come from `T_AU_TEAM_MEMBERS` and `T_AU_AUDIT_TEAM_TASKLIST`, with actual task status descriptions.
- Five legacy working-paper sources are read without changing their workflow. When V2 tables are installed, saved latest headers/items (partitioned by engagement, entity and paper type), review notes and evidence references are also visible. Oracle table availability determines optional V2 loading; no environment or application feature switch gates authorized reads. Missing V2 sources show N/A; database read failures are not replaced with zero results.
- Observations include every saved status and text record, risk, reviewer/date and draft/final references. History includes engagement movements, auditor recommendations and auditee responses. Both legacy binary evidence and current response evidence are available through scoped download APIs. V2 evidence references show their recorded source/ID; no unverified external download URL is created.
- Draft report metadata/status, saved text blocks, para narratives and conclusions are readable before report finalization. Uploaded reports remain downloadable through the dedicated scoped API.
- Teams count distinct assigned team IDs. End-date KPI uses the actual scheduled end date from today through the next six days, computed by Oracle. Observation review count uses IAS status 3. V2 IN_REVIEW count and completed/pending saved item results use actual latest rows. These are saved-record counts, not percentages of all required audit work. Legacy workpaper review/completion, absent dates, unavailable report status and other unsupported metrics show N/A.

## Manual installation and focused verification

No SQL was applied by the application or during implementation. After approval, a DBA should inspect the actual target role/department/status catalogues with `sql/verify_head_audit_monitoring.sql`, review the mappings, then:

1. Run `sql/PKG_HEAD_AUDIT_MONITOR.sql`; verify both views/package are VALID and `USER_ERRORS` is empty.
2. Run `sql/head_audit_monitoring_permissions.sql`. It registers the dedicated menu/page and GET APIs, grants only verified roles 1/5/6/7, checks ID/path collisions and preserves existing deactivated API decisions on reruns. It never changes operational or executive page grants.
3. Refresh the approved test sessions' existing IAS permission caches (sign out/in). Verify own-department list/detail access, foreign engagement and foreign file denial, leader/member denial, expired/revoked context denial, and newly saved in-progress work visibility. The verification SQL includes read-only probes; its live Oracle checks remain pending.

Local validation: `dotnet build AIS/AIS.csproj --no-restore --nologo -v:q -p:BuildProjectReferences=false` and `dotnet run --project tests/head-audit-monitoring/HeadAuditMonitoring.Checks.csproj`. The latter checks role/entity eligibility, authentication/no-store attributes, GET-only actions and absence of authority parameters. It does not substitute for target Oracle compilation or live authorization probes. No merge, production SQL installation or deployment is included.

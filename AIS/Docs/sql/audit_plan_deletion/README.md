# Audit Plan Deletion Authorization

## Controlled deployment

**Not deployed to production.** Back up the original PKG_HD and PKG_PG specs/bodies and select a disposable UAT plan fixture. Do not execute any script as an application startup migration.

1. Confirm the actual Oracle 19c schema: `T_AU_PLAN` columns and `T_AU_PLAN_ENG.PLAN_ID`, IAS user/group/menu tables, `V_IAS_ENTITY_REPORTING_NAMES` (REPORTING_1..6), and potential foreign keys that reference `T_AU_PLAN`.
2. Execute `01_table.sql` **once** (table, unique PLAN_ID constraint, request sequence).
3. Execute `02_authorization_page.sql` after checking proposed PAGE_ID 1000416 is unused. The script fails on ID/path conflicts. Active groups belonging to the verified Super User role (role ID 1) are granted even where GROUP_ID differs from 1. The grant is limited to role 1. **Deliberately no ordinary approval groups are granted automatically.** Authorizing group/page allocation must be approved separately by IAS administrators.
4. Compile and deploy the updated `AIS/Docs/sql/PKG_HD.sql` (spec and body) and `AIS/Docs/sql/PKG_PG.sql` (spec and body), then validate `USER_ERRORS`. `PKG_PG.p_get_audit_plan` now emits `DELETION_PENDING`, so deploy the database before the application.
5. Deploy the .NET MVC files, JS and Razor views through the usual protected release pipeline. The GET page `/Planning/audit_plan_deletion_authorization` resolves its permission through `T_MENU_PAGES`. Only explicitly approved role/groups should be assigned to the authorizer page.
6. Run `03_verify_uat.sql` and the scenarios below. Do not merge/release without successful build, DB compilation and two-person UAT signoff.

## Security model

- Request page entitlement: existing `Planning/tentative_audit_plan` or `Planning/Planning` allocation.
- Authorize page entitlement: independent `Planning/audit_plan_deletion_authorization` allocation.
- Every mutation validates the current `T_USER_SESSION.SESSION_ID`, active IAS role/group/context, page allocation and auditee-entity scope in Oracle, not just the browser. The application binds the identity and session details from the authenticated `SessionUser` and never from submitted forms.
- Authenticated Super Users (IAS role 1) may review the full audit portfolio after validation of their session, group and independent approval-page grant. Ordinary users stay limited to the existing office/ancestor hierarchy. An approved Super User still cannot approve their own deletion request.
- `P_REQUEST_PLAN_DELETION` row-locks the plan and only accepts status 1 (tentative) with zero linked engagements. `T_AU_PLAN_DISABLED.PLAN_ID` is UNIQUE across all requests.
- `P_AUTHORIZE_PLAN_DELETION` row-locks request then plan; rejects the requester's own PPNO, non-pending/stale plans, any linked engagements and scope violations. Deletion and authorization update commit in one transaction; failures rollback.
- `PKG_PG.P_AddAuditEngagementPlan` also row-locks the plan and rejects plans with deletion requests to prevent a race against authorization. No existing permission or validation is disabled.
- HTTP POST forms use `[ValidateAntiForgeryToken]`; inputs come from the server-side user session. Stored reasons are displayed encoded in the authorization Razor view. Application audit events are emitted only after confirmed successful stored-procedure completion.
- Unauthorized/failed requests log server-side errors; database table retains permanent request and authorization evidence. The approval page adds no authorization remarks field.

## Acceptance scenarios (UAT fixtures)

1. Eligible tentative plan: request popup requires 10-2000 characters and saves one PENDING row. Original plan remains.
2. Another request for same PLAN_ID, including from a second browser/session: Oracle unique constraint prevents duplication.
3. While PENDING: UI shows Deletion Pending, hides Create; direct engagement stored-procedure call is rejected.
4. Same PPNO as requester (including Super User): cannot list own request in approval queue and cannot authorize even by direct POST/PLSQL (ORA-20219).
5. Authorized different PPNO within assigned group/scope: deletes exactly one plan, updates request to AUTHORIZED with person and timestamp. Original snapshot remains.
6. Second authorization and repeated submission are rejected without any extra DELETE.
7. Invalid page allocation, inactive user, wrong entity scope, invalid/stale role/group/context, forged POST/CSRF and missing or revoked IAS database session: denied without data changes.
8. Super User account with role 1 and a valid non-1 group ID: authorized only if its live session matches, its group is active, and the authorization page is granted. Lists pending requests across all audit offices except requests submitted by that same PPNO.
8. Create an engagement before the request or between approval phases: deletion refused. Confirm no orphaned engagement.
9. Modify any plan snapshot field or status after request: authorization refused and request remains pending for administrator investigation.
10. Cause a foreign-key or other DB failure at DELETE: original plan survives and request remains PENDING, showing transaction rollback.
11. Concurrent creation and deletion requests for one plan: exactly one operation may succeed. Verify no race produces an orphan or an approved deletion with a retained engagement.
12. Verify deployment permissions and that the CIA Command Center, Audit Para Snap, existing planning pages and reports are unchanged.

## Known validation limits

The feature branch is source-level implementation only. Oracle table metadata and live row samples were unavailable from GitHub, and no Oracle or .NET runtime was available for compilation here. DBA must verify SQL datatypes/foreign keys, compile both packages, run actual UAT scenarios, and check deployment order before merging.

## Rollback

Reverse the application deployment and restore the backed-up `PKG_PG` and `PKG_HD` binaries/specs. Retain `T_AU_PLAN_DISABLED` as evidence; do not silently drop it or reinstate deleted plans without an approved data recovery plan.

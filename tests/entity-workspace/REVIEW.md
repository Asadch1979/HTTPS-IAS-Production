# Entity Management foundation review — 3 November 2026

This branch is an incomplete, gated frontend foundation. It does not satisfy live-read, scoped selection, reversal selector migration or production acceptance. Do not deploy as the completed redesign.

## Reviewed sources and blockers

The six requested SQL references are absent in the local checkout. Read their current main-branch contents through the GitHub connector, including the authoritative package specification. No SQL files were copied or modified. PRODUCTION_REVIEW_2026-11-03 explicitly says the package has not been compiled/deployed. No target database deployment evidence or approved scope resolver was available. The search package has no scope input; filtering a loaded page cannot establish authorized paging/totals. Readers therefore remain unavailable, with no invented parameters, data or endpoints.

Existing AdministrationPanelController dashboard checks and LoadEntityDashboardStep remain unchanged. ApiCallsController, Manage Users selectors (page 43), OrganizationStructureController/service/DB integration, reversal Razor/JavaScript/procedures, and all legacy routes remain unchanged. No new API or database write path exists in this foundation. Existing API security has not been certified by this change.

## Old function mapping

| Existing screen / function | Workspace destination | Current behavior |
|---|---|---|
| entity_addition, setup_auditee_entities, update_auditee_entities | Entity Master | Disabled profile/edit boundary; legacy routes preserved |
| entity_relationship, entity_heirarchy, manage_entity_mapping, manage_entity_relations, manage_entity_type | Organization & Mapping | Read-only placeholders; legacy routes preserved |
| audit_comp_management, gm_repo_line_management, entity_gm_reporting_div_management | Bulk Management | Consolidated local field proposal controls; no selection or trusted preview |
| OrganizationStructure Index / move | Organization & Mapping; Movements | Link to unchanged implementation and checks |
| entity_shifting, EntityShiftingHistory | Movements & Engagements | Existing shifting screen linked; routes preserved |
| setup_engagement_reversal | Movements & Engagements | Established screen linked; requests/modals unchanged |
| Engagement shifting partial | Movements & Engagements | Existing reversal screen link; embedded partial loader retained server-side |
| authorize_auditee_entities_update | Authorizations & Audit Trail | New controls disabled; existing route preserved |
| New closure/archive | Closure & Para History | Disabled workflow, preliminary impact and archive boundaries |
| Mapping investigation | Data Quality | Defined categories; readers unavailable |

## Database integration matrix

| Capability | Status | Required evidence |
|---|---|---|
| P_SEARCH_ENTITIES | Unavailable | Valid deployed package + independently approved scope-aware search/paging |
| P_GET_ENTITY_PROFILE | Unavailable | Deployment + page/operation/entity authorization, preserve every mapping row |
| P_PREVIEW_ENTITY_CLOSURE | Unavailable | Deployment + archive availability + scoped authorization; never final approval |
| P_GET_ENTITY_CHANGE_LOG | Unavailable | Deployment + scoped history permission; payload format unspecified |
| P_GET_ENTITY_PARA_ARCHIVE | Unavailable | Deployment + archive table + scoped history permission |
| Summary metrics / extended filters / Data Quality | Awaiting Database API | Approved scoped aggregate/filter/investigation readers |
| Create / edit / mapping / bulk preview and apply | Awaiting Database API | Approved validated transaction and permission contracts |
| Closure / approval / reopening | Awaiting Database API | Maker-checker, immutable full archive, reporting parity, transactional procedures |
| Reversal and organization move | Existing integration unchanged | Manual regression against authorized staging environment |

Summary metric definitions for future approval: Active = master ACTIVE Y; Inactive = non-Y including null (package semantics); Mapping Exceptions = distinct entities classified by approved investigation categories; Unassigned GM = distinct entities without a valid established mapping GM assignment under approved fallback rules; Pending Approvals = authorized pending requests; Recent Changes = authorized events within a displayed, approved time window. No aggregate values or time windows are asserted now.

## Validation

- dotnet build AIS/AIS.csproj --no-restore -v quiet: succeeded, 0 errors, 57 existing warnings.
- node --test tests/entity-workspace/workspace.test.cjs: 4 passed; tabs, keyboard wrap/Home/End, explicit field opt-in/reset, no network/eval surface.
- No controller/API tests added because no routes were introduced. Future readers require permission, session revocation, object scope, validation, 400/401/403/404/503 and database-unavailable tests before enabling.
- Authenticated browser screenshots unavailable: no authenticated IAS session supplied. No fabricated before/after screenshots. Visual acceptance remains pending.

## Manual staging regression checklist (not executed)

- Login with authorized, unauthorized, revoked, ordinary, multi-posting and reporting-role sessions; verify dashboard/menu/redirect behavior and direct legacy-route checks.
- Verify all eight tabs, keyboard arrows/Home/End, mobile horizontal scrolling, no duplicate IDs or repeated event handlers; draft dirty warning and unchanged-field reset.
- Open setup_engagement_reversal directly and from workspace; exercise relationship/reporting/entity selection, engagements, each modal, validation failure, cancel, successful authorized reversal, failure/result handling and refresh. Compare requests and results with baseline; verify procedures unchanged.
- Check shifting, shifting history, entity addition/relationships, setup/update/authorization, both bulk legacy screens and Manage Users selectors retain existing behavior.
- Exercise Organization Structure authorized preview/move, stale preview, invalid move, forbidden session and rollback behavior with existing tests.
- Verify CIA, CCO, audit dashboards, reports, notifications and scheduler smoke checks remain unchanged.
- After approved readers: altered entity IDs must fail scope checks on every reader; page size 201 rejected; multiple profile mappings shown; unavailable archive differs from empty archive; filter changes reset page/selection safely.
- Future All Matching must resolve full authorized selection server-side; test rollback of entire batch on one failure and maker cannot approve own closure.

## Changed files

Entity_Dashboard.cshtml; DashboardPartials/_EntityMasterDraft.cshtml; _EntityBulkDraft.cshtml; _EntityClosureDraft.cshtml; wwwroot/css/entity-management.css; wwwroot/js/administration-entity-dashboard.js; tests/entity-workspace/workspace.test.cjs; this review document. Existing unrelated appsettings, obj and local files excluded.

# Entity route separation review

## Changes
- Entity_Dashboard.cshtml: restored the complete legacy view; removed the redundant Legacy filename.
- Entity_Workspace.cshtml: preserved the compact four-area view, changing only its data URL to Url.Action(Entity_Workspace).
- AdministrationPanelController.cs: legacy-only action; retains stepKey validation, authorized model and session revocation check.
- EntityWorkspace controller partial: independent action and explicit mapped page grant check, including Super Users.
- EntityReads controller partial: workspace-specific authorization; Oracle adapter, validation, pagination and error handling unchanged.
- Administration links: permission-gated workspace link, no preview flag.
- Tests: updated workspace view references and added route separation assertions.

## Preservation evidence
Git diff against merge 4d979db0 (PR #362) showed no changes to the legacy view or legacy JavaScript before separation. The restored view retains that complete implementation. LoadEntityDashboardStep, its independent permission checks, all business partials, engagement scripts, Oracle adapters and procedures are unchanged. PR #365's compact workspace is preserved with only its reader URL changed.

## Deployment dependency — fail closed
No approved new page ID or grants were supplied. Register /AdministrationPanel/Entity_Workspace through IAS page management, update the deployed wwwroot/Images/Page ID.xlsx mapping with the approved distinct ID, and explicitly grant intended users/roles through the normal process. Retain dashboard ID 970202. No mapping, grant or menu record is invented by this change. Missing mapping or explicit allowed-view grant denies workspace access even to Super Users. Unscoped readers additionally remain Super-User-only. Legacy operation permissions remain independently enforced by the unchanged loader. Activate a menu entry only after approval and UAT.

## Validation and outstanding acceptance checks
Executed: Node frontend regression suite and dotnet run --project tests/entity-workspace/EntityWorkspace.Checks.csproj --no-restore.
Not executed: authenticated Oracle-backed operations, browser two-tab isolation, visual before/after screenshots, live direct-access/session-revocation matrix, and root/non-root PathBase browser tests. No authenticated UAT session or approved workspace page registration was available. These remain required before acceptance/deployment; no successful live regression or screenshot comparison is claimed.

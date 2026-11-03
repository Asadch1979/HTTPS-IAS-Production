# Final correction evidence and permission migration proposal

## Fresh operational state and navigation

Every step visit now fetches fresh server-rendered data with no-store. Removed the HTML cache, generic control snapshot/restore and custom step-state adapters. No draft fields are currently approved for restoration, so none are restored. Explicit entity-shifting source/destination selection remains its existing identity-selection mechanism, not a database-record snapshot; destination/API validation is unchanged. Navigation versions guard response rendering, script-chain continuation, final navigation state and error rendering, so stale responses and failures cannot overwrite the latest step. An already executing legacy script retains its original behavior; authenticated browser regression remains required for modal/plugin lifecycle.

## Effective page mapping audit

Verified using the actual PageIdResolver against checked-in Page ID.xlsx plus Page ID - Archive.csv and the resolver's real precedence rules (active mappings win). These are configuration IDs, not proof of production grants.

| Original step | Effective page ID |
|---|---:|
| Entity Addition | 217 |
| Entity Relationship | 216 |
| Entity Shifting | 218 |
| Setup Auditee Entities | 41 |
| Update Auditee Entities | 323 |
| Authorize Auditee Entities Update | 324 |
| Setup Engagement Reversal | 225 |
| Engagement Shifting (compatibility) | 225 |

| Additional function | Effective page ID / disposition |
|---|---|
| entity_heirarchy | 0 — unmapped, excluded/fail closed |
| manage_entity_type | 0 — unmapped, excluded/fail closed |
| manage_entity_relations | 0 — unmapped, excluded/fail closed |
| manage_entity_mapping | 0 — unmapped, excluded/fail closed |
| audit_comp_management | 265 |
| gm_repo_line_management | 0 — unmapped, excluded/fail closed |
| entity_gm_reporting_div_management | 0 — unmapped, excluded/fail closed |
| EntityShiftingHistory | 448 (active entry takes precedence over archive 1000218) |

The earlier statement that every additional function was restored is corrected: six have no configured page ID and must remain inaccessible until the permission owner supplies approved mappings/grants. Do not alias them to arbitrary existing pages. Tests assert all 16 effective resolutions and visibility/denial, including dashboard-only access and direct denied partial/API requests.

## Dedicated Engagement Shifting permission proposal (not activated)

Proposed permission key: ADMIN_ENGAGEMENT_SHIFTING, with a dedicated page ID assigned by the permission owner (no invented numeric ID). The controller's CanAccessEngagementShifting is now the single policy seam used by dashboard visibility, function links, partial loading, GetShiftableEngagements and ShiftEngagementEntity. Today it deliberately delegates to existing page 225, preserving current access. Reversal continues independently on 225 and is unchanged.

Owner-approved migration sequence:
1. Inventory current page-225 users/roles, API operation grants and posting/entity scope in staging. Approve the dedicated page/path and separate view/execute operation grants; do not broaden object scope.
2. Provision the dedicated permission and equivalent approved grants outside this application PR. Verify every currently authorized shifting user has equivalent new access and reversal-only users are classified explicitly by the owner.
3. In a separately reviewed application change, use a bounded compatibility policy (legacy 225 OR the new dedicated grant) consistently through the shared seam and update the step's RequiredPermissionPageId/operation metadata. Existing global API permissions and entity-validation rules must still pass. Test both old-only and new-only access through dashboard, direct reader and mutation.
4. After grant parity and revocation tests, cut over to dedicated-only through the same seam. Require explicit deployment approval; expire compatibility mode on an approved date. Reversal must continue using 225 throughout. Keep a rollback plan that restores compatibility without losing grants.

No page configuration, production grants, Oracle procedures, reporting packages, reversal business logic or unrelated modules were changed. There is no automatic migration, permission fallback to ordinary users, or grant-writing endpoint in this PR.

## Remaining staging-only verification

- Confirm deployed page configuration matches the audited checked-in files; obtain approved mappings/grants for the six unmapped additional functions before claiming them restored.
- Exercise fresh records after switching away/back, rapid navigation under throttled network, permission revocation and expired sessions in an authenticated browser.
- Exercise all legacy plugin/modal initializers, DataTables teardown/reinitialization and form drafts; no draft retention is promised.
- Full Setup Engagement Reversal and Engagement Shifting success/failure/cancel validation against authorized staging entities; verify correct API operation grants and entity scope.
- Review and approve the dedicated-permission migration separately. Live Entity Management readers still require deployment/scope evidence as described in REVIEW.md.

The working dashboard remains the default. The eight-tab redesign remains an explicit incomplete preview. No merge or deployment is authorized by this evidence.

Automated evidence after final corrections: 39 C# checks passed (including all 16 real mapping resolutions); 10 JavaScript tests passed (including six legacy navigation regressions). Application build succeeded with 0 errors; incremental build reported 0 warnings. Commands: dotnet run --project tests/entity-workspace/EntityWorkspace.Checks.csproj --no-restore -v quiet; node --test tests/entity-workspace/*.test.cjs; dotnet build AIS/AIS.csproj --no-restore -v quiet.

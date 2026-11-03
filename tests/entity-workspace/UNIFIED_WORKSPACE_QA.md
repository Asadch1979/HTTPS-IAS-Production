# IAS Entity Management: unified four-area workspace review

## Why this change exists
The approved design was one compact **entity-centered** administration surface.
The prior eight-tab implementation scattered data among technical sections and
showed numeric TYPE_ID, AUDITBY_ID, GM_OFFICE, B_GROUP, PARENT_ID as if they
were user-facing descriptions. This branch reorganizes the preview, but does
not weaken the authorized legacy operations or enable unapproved writers.

## URL and scope
- Main existing working dashboard remains: `/AdministrationPanel/Entity_Dashboard`
- New redesigned preview: `/AdministrationPanel/Entity_Dashboard?workspacePreview=true`
- Existing Super-User-only read boundary remains, with Entity Dashboard page
  permission and revoked-session checks. Ordinary users cannot request entity data.
- No new Oracle packages, DML, mutation endpoints or role grants.

## Four coherent work areas
1. **Entity record:** search by name/code/ID; choose entity; display active
   status, name, parent name, entity type, GM Office and Group in one always-visible
   strip. Master and all mapping records are displayed together. Authorized
   legacy Add / Edit / Mapping partials load inside this same pane.
2. **Shifting & closure:** existing authorized Entity Shifting partial loads
   inline for shifting with paras; mapping-only shifting is visibly **not
   operational** until an approved separate transaction exists; closure
   displays existing preliminary para/engagement/child counts and archived
   para snapshots, without any executing/settlement capability.
3. **Engagement details:** the original authorized Setup Engagement Reversal
   partial and Engagement Shifting partial load in a shared workspace area.
   Existing client scripts, APIs, permission checks, validations and database
   procedures are untouched. The new shell styles the existing controls;
   no silent migration of legacy relationship selectors to entity type IDs.
4. **Authorizations:** existing authorization partial loads inline for users
   with that individual step permission; entity change history remains
   readable. New requests require an approved maker-checker backend.

## Descriptive labels and verified provenance
- Type: `GetAuditEntityTypes` authorized type reader, not raw TYPE_ID.
- Parent: `P_GET_ENTITY_PROFILE` mapping `P_NAME`, with an authorized
  profile lookup for PARENT_ID when P_NAME is unavailable.
- GM: current mapping `GM_OFFICE` first; if NULL, inspect the referenced
  `MAPPING_AUDITEDBY` entity's mapping and use its GM_OFFICE **only when there
  is exactly one distinct non-null candidate**. Never display AUDITEDBY itself
  as the GM, and never guess a GM on conflicting candidates.
- Group: mapping `B_GROUP` resolved through an authorized profile lookup
  to the referenced entity's NAME.
- Audit cluster: `AUDITBY_ID` similarly resolved to an entity name.
- Entity ID and Code remain visible as **secondary reference metadata**,
  not primary business descriptions.
- Multiple mapping rows are explicitly flagged. The summary displays the
  first returned mapping with that warning; all rows are shown in the
  mapping details below. This requires human review, not automatic repair.

The mapping lookup makes at most a few extra read-only calls on the *selected*
record. It does **not** fan out extra Oracle calls to enrich every search result.
Thus the register shows the entity Name, Type and Active status, while the
complete Parent / GM / Group context is shown when a row is selected.

## Required staging regression
1. Sign in as Super User authorized for the dashboard. Confirm initial
   counts (active/inactive) and compact three-by-two summary card arrangement.
2. Query `FINANCIAL`. Select CHIEF FINANCIAL OFFICER and verify that the
   shared profile displays **textual** Type/Parent/GM/Group labels, not
   numeric IDs. Compare these names to the actual mapping records; missing
   or ambiguous references should explicitly say so.
3. Switch through all four workflow cards. Selected entity must persist.
   Search / next / previous and Status / Entity Type filters must work.
4. Select an entity with two mapping rows. Verify both appear in Mapping
   details and summary warns that there is more than one.
5. Under Entity record, open Add, Edit and Mapping for authorized users.
   Exercise search and cancellation; verify modal and plugin lifecycle.
6. Under Shifting & Closure, invoke Shifting with paras and compare the
   original request/response behavior on an authorized staging entity.
   Shifting without paras must **not** execute any data change.
7. Under Engagement Details, open Reversal. Verify relationship/reporting/
   posting selectors, engagement results, every modal, cancel, success and
   failure handling. Test shifting separately. The legacy forms may still
   require their own selection; no unsupported ID translation was added.
8. Under Authorizations, open the existing update-authorization partial.
   Verify the right data and permissions, and that an unauthorized user
   cannot access the partial by changing its URL.
9. Verify direct `?data=profile&entityId=...` access for non-Super-Users
   returns 403 and does not leak data.
10. Verify all legacy administration routes and the normal Entity Dashboard
    remain unchanged. In particular, retain Engagement Reversal behavior
    and CIA/CCO/reporting/scheduler functionality.
11. Switch workflows repeatedly with browser throttling to check that
    legacy partial scripts initialize and modals close correctly.

## Pending, not misrepresented as complete
- Full Group/Division and GM filtering in the server-paged search needs
  an approved joined and scoped Oracle reader.
- Mapping exceptions, pending approvals, unassigned GM and recent changes
  aggregate definitions/readers are not approved; these cards say Pending.
- Entity addition/edit/mapping currently delegate to **existing** authorized
  applications. Their legacy forms are mounted inside the new area; they
  have not been rewritten as new transactional APIs.
- Mapping-only shift, closure, maker-checker approval, and any bulk write
  are unavailable without database transaction contracts.
- The existing Oracle package reads are unscoped and **must not** be opened
  to ordinary users until independent entity-scope filtering occurs before
  pagination.
- Running the new partials inside the redesigned panel requires live
  browser/regression testing. No successful Oracle or .NET build test has
  been performed by this review environment.

## Files changed
- `AIS/Views/AdministrationPanel/Entity_Dashboard.cshtml`
- `AIS/wwwroot/css/entity-management.css`
- `AIS/wwwroot/js/administration-entity-dashboard.js`
- `tests/entity-workspace/workspace.test.cjs`
- `tests/entity-workspace/readonly-live.test.cjs`
- This review checklist

No changes to `PKG_IAS_ENTITY_MGMT`, `PKG_AD`, legacy reversal screens,
legacy reversal JavaScript, `LoadEntityDashboardStep`, or production grants.

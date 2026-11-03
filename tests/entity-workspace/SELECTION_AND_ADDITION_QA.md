# Entity Management unified selection and addition: review/acceptance

## Implemented in this change

### 1. Engagement Details & Reversal; Shift an Engagement
- The selected entity from the central Entity Register is the source for **both** operations.
- `entity-workspace:selected` and `entity-workspace:profile` events retain
  the validated selected ID, name, status and full approved master profile.
- When the existing authorized partial is loaded, the wrapper inserts the
  selected entity as the sole source option of `#childposting` (Reversal)
  or `#engShiftCurrentEntity` (Engagement Shifting), and hides the now
  redundant **source** Relationship/Reporting/Entity selector rows.
- It calls the **original** `getEngagementDetails()` or
  `engShiftGetEngagements()`. Those unchanged functions call the same
  existing API using exactly the selected ENTITY_ID. Existing modals,
  destination selectors, audit dates, status changes, CSRF on shifting,
  validations, response handling and backend procedures remain untouched.
- Direct permission checks remain in the original Step loader and
  controller/API security pipeline. The central selection is *not* a new
  authorization bypass. Stage-test the effective object-scoping controls.
- Selecting a different entity clears any prior engagement partial so old
  entity records are not shown under a new selection.

### 2. Add Entity: GM, Group and parent mapping first
- A compact new form opens inside **Entity Record**. The first fields are
  **Group**, **GM Office**, **Parent Type**, **Parent Entity/Reporting Office**.
- Parent Type options come from the existing authorized Entity Types reader;
  Parent Entity candidates are loaded from `P_SEARCH_ENTITIES` using the
  selected type, status ACTIVE, bounded name search and server paging.
- Group/GM Type categories are inferred *only when a single unambiguous
  matching type label exists*. If missing/conflicting, the corresponding
  dropdown stays unavailable rather than choosing a potentially wrong ID.
- Audit cluster choices come from the existing authorized
  `PKG_AD.P_GET_AUDITED_BY` reader behind the same Super User + Entity
  Dashboard page gate; it is not sourced from Manage Users page-43 APIs.
- Candidate lists display **names**, never raw IDs as the selection label.
  With more than 200 matches, the form prompts for narrower name search
  rather than implying all candidate entities have been fetched.
- **Create Entity & Mapping remains DISABLED.** The current
  `PKG_AD.P_INSERTENTITIEES` and `P_ADD_ENTITIES_MAPPING` perform separate
  database transactions and do not support a safe all-or-nothing combined
  insertion. The system must not create a master record and silently leave
  its mapping missing if a subsequent mapping call fails.

## Oracle transaction contract required to enable one-step creation

Create a separately reviewed, authenticated and authorized Oracle writer
(e.g. `PKG_IAS_ENTITY_MGMT.P_CREATE_ENTITY_WITH_MAPPING`) with:

1. Inputs: code, name, description, entity type, active/auditable, audit office,
   approved parent type/parent ID, Group and GM Office IDs, reason, operator,
   request/authorization metadata as applicable.
2. Validate parent exists and matches the submitted Parent Type; reject
   inactive, conflicting or unauthorized parent/scope assignment. Validate
   Group and GM against approved hierarchy; derive or validate the correct
   relationship code and audit-office assignment on the server. Never treat
   client-selected type IDs alone as proof of authority.
3. Generate a race-safe unique ENTITY_ID with an approved sequence/
   concurrency migration. **Do not use MAX(ENTITY_ID)+1.**
4. Insert master and all required mapping fields (including Group and GM
   Office) as **one transaction**. Do not call the existing legacy procedures
   if their internal COMMIT/DELETE logic makes atomicity impossible.
5. Include an append-only change event with actor, request reference and
   effective reason. Implement independent maker-checker authorization
   if the organization requires it for creation.
6. Return created ID, success and reason from the database without
   claiming success until the entire transaction commits. Fail and
   rollback the complete transaction on any validation/writer error.
7. Enforce server-side page, action, role, entity scope and CSRF checks;
   preserve CSP and application audit logging. No client role metadata
   should authorize a write.

**No Oracle script or production table has been changed in this PR.**

### 3. Edit Entity from the Entity Addition update option
- "Edit selected entity" now loads the existing authorized
  `ENTITY_ADDITION` partial, not `UPDATE_AUDITEE_ENTITIES`.
- Fetches a **fresh** entity profile by selected ID, then prepopulates the
  existing `#updateEntityModal`: Type, Code, Name, Description, Auditable,
  Audit By and Active, with `window.g_entIdToUpdate` set to the validated
  selected ID.
- The old HR and AIS search grids are hidden **only** in this compact
  wrapper. The original existing Addition JavaScript, form submission
  and `PKG_AD.P_UPDATEENTITIEES` procedure are unchanged.
- The Active field is frozen when editing through this route. Saving
  an **inactive** entity is blocked because the old update procedure
  can delete mappings when ACTIVE='N'. Reopen/closure needs the new
  governed workflow. These are UI precautions, not a substitute for
  endpoint/object-level authorization.

## Test checklist
1. In Visual Studio check out this PR's branch. Build the .NET 8 application.
2. Sign in as authorized Super User. Open
   `/ZTBLAIS/AdministrationPanel/Entity_Dashboard?workspacePreview=true`.
3. Select a valid entity. Choose **Engagement Details & Reversal**.
   Confirm the selected entity is shown at top, the Relationship/
   Reporting Office/Place of Posting source dropdowns are hidden,
   and real engagements appear without another search.
   Exercise original actions, modals, cancel, success, failure.
4. Choose **Shift an engagement**. Confirm the same current entity,
   correct engagements and **destination** selectors still visible in
   the shift modal. Test authorized move and rollback/error outcomes.
5. Choose **Edit selected entity**. Confirm the existing Addition
   update modal opens with correct live values for every field, without
   first searching or entering an entity ID. Confirm that the manual
   Update Auditee Entities interface does not open.
6. For an inactive entity, confirm the Edit modal Save is disabled.
   For active entities, the Active status must not be editable here.
7. Choose **Add Entity**. Verify Group, GM Office, Parent Type and
   Parent Entity are presented first and show meaningful candidate
   names. Confirm Parent candidates update when Type/name search
   changes; candidate overflow is disclosed. Confirm **no Save**.
8. Verify non-Super-Users cannot access unscoped Oracle entity readers;
   unauthorized operations remain hidden/403. Test direct URLs.
9. Retest standard legacy Entity Addition, Engagement Reversal, Shifting,
   authorization workflow and all existing administration routes.
10. Confirm no changes to stored procedures, reporting, notifications,
    scheduler, or security configurations.

### Testing evidence and remaining gates
- The bridge JavaScript was syntax-parsed and its selected-entity
  Reversal/Shifting calls were exercised in a lightweight DOM mock.
- These are not full browser tests. No Visual Studio/.NET compilation,
  staging Oracle execution or authenticated modal regression has yet
  been performed by the authoring environment.
- Production merge awaits authenticated staging tests, and the
  combined Add save button awaits the separate Oracle write approval.

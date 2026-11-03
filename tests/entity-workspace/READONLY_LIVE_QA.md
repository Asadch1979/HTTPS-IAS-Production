# IAS Entity Management: Live Read-Only UAT

## Scope
This branch adds a **Super-User-only, read-only** Oracle-backed Entity Management preview.
The existing working Entity Dashboard stays the default and its legacy routes/reversal
procedures are unchanged. No Oracle DDL, DML, package modifications or grants are included.

**Prerequisite:** The database used by the running IIS / Visual Studio instance must
contain VALID PKG_IAS_ENTITY_MGMT (spec and body), T_IAS_ENTITY_CHANGE_LOG and
T_IAS_ENTITY_PARA_ARCHIVE. A production deployment check does **not** prove that a
localhost development connection targets that same database. Review the active
ConnectionStrings:DBDataSource outside source control. Do not paste credentials into tickets.

## Open in Visual Studio
1. Check out the feature branch, restore/build the .NET 8 solution, and sign in with a valid
   **Super User** account with Entity Dashboard permission.
2. Visit `/ZTBLAIS/AdministrationPanel/Entity_Dashboard?workspacePreview=true`.
   The base path may differ outside IIS virtual directory hosting.
3. Two KPI cards, **Active** and **Inactive**, should show exact real totals via
   P_SEARCH_ENTITIES with page size 1 and COUNT(*) OVER(). The other four cards
   explicitly remain unavailable pending approved Oracle aggregate readers.
4. Entity Register should load its first 50 results. Search by ID, code or name,
   filter by Entity Type (existing PKG_AD reader) and status, and use Next / Previous.
5. Click **View** to load master fields, every returned mapping record, closure-impact
   counts, change history (up to 100 rows), and archival history (up to 100 rows).
   Empty history means no events yet; an Oracle error is displayed separately.
6. Confirm no Save, Apply, Approve, Execute Closure or Restore action is enabled.

## Oracle reference queries (read-only)
```sql
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name IN ('PKG_IAS_ENTITY_MGMT',
                       'T_IAS_ENTITY_CHANGE_LOG',
                       'T_IAS_ENTITY_PARA_ARCHIVE')
 ORDER BY object_name, object_type;

SELECT active, COUNT(*) total
  FROM T_AUDITEE_ENTITIES
 GROUP BY active
 ORDER BY active;

SELECT entity_id, code, name, type_id, auditby_id, active
  FROM T_AUDITEE_ENTITIES
 WHERE entity_id = :ENTITY_ID;

SELECT entity_id, parent_id, auditedby, gm_office, reporting, div_office, b_group
  FROM T_AUDITEE_ENTITIES_MAPING
 WHERE entity_id = :ENTITY_ID;
```

The Active card counts `ACTIVE='Y'`; Inactive counts `ACTIVE IS NULL OR ACTIVE<>'Y'`.
Compare status totals to this definition, not simply all non-Y values grouped by ACTIVE
if the database contains NULL.

## Security and regression acceptance
- Ordinary users with Entity Dashboard permission must not receive JSON entity datasets.
  `?data=search` and `?data=profile&entityId=...` return 403 to non-Super-Users.
  A hidden/disabled frontend is NOT the authorization boundary.
- Invalid search status, negative offset, page size 201, and invalid IDs return 400.
- Logged-out/expired sessions return authorization errors and no data.
- Data is rendered via DOM textContent, not inserted as HTML.
- Repeated search and selecting a different entity must not show stale results.
- Retain separate valid workflow for Setup Engagement Reversal, Engagement Shifting
  and approved Organization Structure operations.
- User and reporting-role scope integration is NOT authorized by this feature. The
  deployed package's search cursor has no trusted entity-scope argument, so enabling
  it for ordinary users would expose cross-entity records. Future work needs an
  independently approved scope-aware search that filters BEFORE pagination.
- Histories are capped at 100 rows for this preview. Change-log CLOB display values
  are capped at 16,000 characters without altering database records.

## Implementation map
- Oracle calls: `AIS/DBConnection.EntityManagement.cs`
- Authorization and JSON response: `AIS/Controllers/AdministrationPanelController.EntityReads.cs`
- Existing authorized page route extended with `?data=`: `AdministrationPanelController.cs`
- Preview Razor: `AIS/Views/AdministrationPanel/Entity_Dashboard.cshtml`
- Browser logic: `AIS/wwwroot/js/administration-entity-dashboard.js`
- Client and boundary tests: `tests/entity-workspace/workspace.test.cjs` and
  `tests/entity-workspace/readonly-live.test.cjs`

## Deferred, not represented as complete
- Mapping exceptions, unassigned GM, pending approvals and recent changes KPI
  aggregates and their definitions.
- Group/Division/Reporting Office/GM Office search filters, which require trusted
  organization-aware server paging.
- Editing, bulk updates, approvals, closure, reopening or para mutations.
- End-to-end authenticated browser test and real target Oracle execution.

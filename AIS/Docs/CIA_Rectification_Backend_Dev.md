# CIA Command Center + ISSUE_RECTIFIED backend (development)

## Status
Source-only changes on an isolated branch. No Oracle execution, IAS activation,
production deployment, data update or historical backfill has been performed.

### Existing live logic to retain
- `PKG_HD.P_audit_Concluding` transfers the per-observation flag for modern
  `IND='A'` final statuses 8/9 into `AIS_T_AU_POST_COMPLIANCE`. Re-conclusion
  refreshes existing copied flags; no second para recognition is created.
- `V_EXEC_KPI_LIFECYCLE` already derives `ISSUE_RECTIFIED_ON` from
  `T_AU_OBSERVATION.ISSUE_RECTIFIED=1`, status 4/9, `STELLED_ON` before
  the current conclusion. Status 23 (dropped) is excluded.
- `PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE` already uses this view and must
  remain the single source of Audit Para Snap's nine lifecycle KPIs.
- The copied post-compliance flag must **not** generate a second ISSUE event
  or count as a settled para. Post-compliance `ISSUE_RECTIFIED=1` is evidence
  that rectification was carried forward from a source observation.

### New reconciliation functionality
1. `V_EXEC_ISSUE_RECTIFIED_RECON.sql`: one row per modern observation,
   pre-aggregates *all* `IND='A'` post-compliance copies and compares copied
   flag(s) against the intended conclusion rule (status 9 copies the
   source flag; other transferred statuses carry zero). Classifies
   NOT_TRANSFERRED, MATCH, MISMATCH and INCONSISTENT_COPIES. NOT_TRANSFERRED
   is normal for pre-conclusion status-4 issues and must not automatically
   be labeled a defect.
2. `PKG_EXEC_DASHBOARD.P_GET_CIA_RECTIFICATION_RECON`: new, read-only CIA
   scoped cursor for a selected IAS audit period. It reports rectified issues
   within that period, copied post-compliance flags, mismatched/inconsistent
   observation flags and final observations without transfer. The cursor
   neither modifies records nor replaces the existing five CIA KPI
   procedures/output. No HTTP endpoint is exposed for this diagnostic.
3. `executive_dashboard_permissions.sql`: map the two CIA APIs to page
   `1000303` instead of TOP page `1000300` and preserve previously
   disabled API-role grants on rerun.
4. `cia_command_debug_permissions.sql`: optional DBA-reviewed *development*
   activation for only `GET /api/ExecutiveDashboard/GetCiaCommandKpis`
   and `GetCiaIssueNature`. Restricts them to IAS Super User role/group
   1 and CIA role/group 3, checks existing page identity and fixes the old
   TOP-page mapping. Does not enable Audit Para Snap or other endpoints.

### Development database execution order, when you approve
1. Verify `ISSUE_RECTIFIED` exists on both observation and post-compliance
   tables and check current `PKG_HD` body matches the agreed copy logic.
   The existing `ISSUE_RECTIFIED_prod_migration.sql` contains DML/DDL;
   **do not rerun it casually or automatically**, in development or otherwise.
2. Compile `V_EXEC_ISSUE_RECTIFIED_RECON.sql` in the IAS development schema.
3. Compile the updated canonical `PKG_EXEC_DASHBOARD.sql` *spec first,
   then body*. Check `USER_ERRORS`, object statuses, and that the older
   nine lifecycle KPI output columns are unchanged.
4. Review and run `cia_command_debug_permissions.sql` in the
   **development database only**. Check role 1/3 grants and page 1000303.
5. Let Codex complete the separate C#/Razor/JavaScript/CSS changes,
   then use only the local development configuration to enable
   `RoleDashboards:CiaCommandCenterEnabled=true`.
6. Review the selected audit period: in October 2026 do not
   default to a future 2027 period. An authorized intentional future
   period may legitimately yield zero period-specific results.

### Read-only SQL checks
```sql
SELECT object_name, object_type, status FROM user_objects
 WHERE object_name IN ('V_EXEC_ISSUE_RECTIFIED_RECON',
                       'V_EXEC_KPI_LIFECYCLE','PKG_EXEC_DASHBOARD')
 ORDER BY object_name,object_type;
SELECT name, type, line, position, text FROM user_errors
 WHERE name IN ('V_EXEC_ISSUE_RECTIFIED_RECON','PKG_EXEC_DASHBOARD')
 ORDER BY name,sequence;

SELECT post_copy_status, COUNT(*) observations
  FROM V_EXEC_ISSUE_RECTIFIED_RECON
 GROUP BY post_copy_status ORDER BY post_copy_status;
```

Optional cursor call after choosing a **real** `T_AU_PERIOD.AUDITPERIODID`:

```sql
VARIABLE rc REFCURSOR
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_RECTIFICATION_RECON(
      P_DASHBOARD_TYPE => 'CIA',
      P_ENTITY_ID => NULL,
      P_AUDIT_PERIOD_ID => :selected_auditperiodid,
      IO_CURSOR => :rc);
END;
/
PRINT rc;
```

The `:selected_auditperiodid` bind must be supplied before running this block.

### Intentionally not changed
No signature or business rule of `P_GET_KPI_LIFECYCLE`,
`P_GET_CIA_EXEC_KPIS`, `P_GET_CIA_ISSUE_NATURE`, `PKG_HD`, original
report A–E procedures, historical source views, or other executive roles
was changed. Historical uncertainty remains. These edits have **not**
been compiled against an Oracle server or validated with live IAS data.

# IAS Entity Management Database: Phase 1 deliverables

Prepared: 3 November 2026. Production schema review added following the complete production preflight workbook. Based on supplied table definitions and read-only review of `Asadch1979/HTTPS-IAS-Production` GitHub default branch. **The uploaded production preflight has been reviewed; the new DDL and package have NOT been executed or Oracle-compiled against live data.** The available repository contains legacy code that may differ from production.

## Files / safe sequence

1. **Preferred export path: `00_PREFLIGHT_SINGLE_RESULT.sql`**: read-only, one SELECT returns every section as a single exportable result grid. In Oracle SQL Developer choose Execute Statement (Ctrl+Enter) and export the complete result grid to one XLSX sheet. Do not execute or export only the first SELECT within the UNION chain. This replaces multiple individual exports from the original script.
2. `00_PREFLIGHT.sql`: original read-only source-schema and data quality checks. **Run first, save and review the results.** Production confirmed `RELATION_TYPE_ID`, `COMPLICE_BY`, `COMPLIANCE_UNIT`, `ISSUE_RECTIFIED`, mapping source fields, and KPI/date baselines.
3. `01_FOUNDATION_DDL.sql`: staging-only DDL for `T_IAS_ENTITY_CHANGE_LOG` (event history) and `T_IAS_ENTITY_PARA_ARCHIVE` (source-table snapshot clone). Creates sequences, constraints and indexes. The archive clone includes all current source columns. Nothing is archived or closed by this script.
4. `02_READONLY_PACKAGE.sql`: read-only `PKG_IAS_ENTITY_MGMT` with search, full profile, first-pass closure preview, change-log browsing and para-archive browsing. It cannot change masters or paras.
5. `03_VALIDATE_FOUNDATION.sql`: object compilation and source/archival schema parity checks; samples read-only API. Run in staging after DDL and package creation.

6. `05_PRODUCTION_TARGETED_DIAGNOSTICS.sql`: **new**, read-only single-result deep dive for production anomaly records and key/ID checks. Run before deployment and export as XLSX.
7. `PRODUCTION_REVIEW_2026-11-03.md`: actual production metadata summary and blockers.
8. `04_ROLLBACK_EMPTY_FOUNDATION.sql`: optional staging rollback, refuses to run if either history table contains records.
9. `DB_CONTRACT.md`: precise read-only package contract and deferred database write requirements.

Run the **single-result diagnostic** with SQL Developer **Ctrl+Enter** to get one export grid. Only after staging approval, run the separate DDL/package scripts with **Run Script (F5)** or SQLcl/SQL*Plus as the IAS application-schema owner. There is intentionally no broad `GRANT EXECUTE`.

## Important design decisions

- Do not touch or drop the current `T_AUDITEE_ENTITIES`, `T_AUDITEE_ENTITIES_MAPING`, `AIS_T_AU_POST_COMPLIANCE` or the existing working Engagement Reversal procedures.
- Oracle `DATE` fields `PARA_ADDED_ON` and `SETTELED_ON` must not be set to numeric zero. Nulling those dates is deferred because the existing `V_EXEC_KPI_LIFECYCLE` and `R_IAS_PARA_POSITION` use them for historical counts. An entity's administrative closure must not become an audit para settlement.
- Do not modify legacy `PKG_AD.P_UpdateENTITIEES` to implement closure. The reviewed version deletes mapping rows when entities become inactive and commits between steps. The new closure operation must archive source rows and then update master, mapping and para statuses within **one** transaction, with rollback on error, only after full historical-reporting reconciliation.
- Do not assume mapping `ENTITY_ID` is unique: `00_PREFLIGHT.sql` measures duplicate rows. Do not derive `B_GROUP`/`DIV_OFFICE`/GM Office from the audit cluster without verifying the true hierarchy.
- Do not conflate `T_AUDITEE_ENTITIES.AUDITBY_ID` and `T_AUDITEE_ENTITIES_MAPING.AUDITEDBY` in the DB implementation without explicit validation. Both should be changed together in one transaction.
- Do not rebind Manage Users selector APIs (currently protected by the Manage Users page-ID 43) to entity management without an independently authorized reader. The new package read operations require controller/entity-scope checks, and its write operations must use securely established user context, not caller-supplied PPNO/role alone.
- `T_IAS_ENTITY_CHANGE_LOG` has an append-only **business convention**. Before granting write access, add database-level enforcement and an appropriate trusted writer package. Direct application DML must be prohibited. A secured writer should log individual REQUESTED/APPROVED/COMPLETED transitions as separate event rows under the same REQUEST_ID.

## Phase 2 DB changes blocked on preflight and acceptance tests

A fully executable master/mapping/closure transaction package is **not included in this foundation**. This is deliberate: the production metadata, ID generation/constraints, transaction semantics, session-scoping contract, `PKG_ORG_STRUCTURE` source, and reporting lineage must first be confirmed. Implementing such writes on guessed production assumptions would risk historical para/report corruption.

The next DB implementation needs `P_CREATE_ENTITY`, `P_UPDATE_ENTITY`, `P_PREVIEW_BULK_UPDATE`, `P_APPLY_BULK_UPDATE`, `P_PREVIEW_ENTITY_CLOSURE` (expanded), `P_CLOSE_ENTITY` and `P_REOPEN_ENTITY` (restricted). Add a controlled approval request workflow, tests for source-to-archive count/field parity, race-safe unique ID generation, multi-entity rollback, zero-date handling, and full historical KPI parity before and after closure. Audit cluster/GM/reporting/Group/Division updates must use exact current schema semantics.

## Not a production deployment package

The scripts are reviewable, executable foundations for a controlled staging environment. They have been statically inspected, but not Oracle-compiled here. Staging test results, DB metadata and sign-off are necessary before production migration. Oracle DDL commits implicitly; take an approved backup and plan a versioned rollback (drop only these new objects when empty and approved), not an imagined transactional rollback of DDL.

## Production workbook now reviewed (3 November 2026)

The latest full preflight returned 532 rows. See `PRODUCTION_REVIEW_2026-11-03.md`; the earlier export issue is resolved. No source-table DML or DDL has been applied. Important findings include absence of declared entity/master PKs and indexes, 26 unusual para statuses, 7 settled rows without dates, and engagement statuses outside 10–14. **Only `05_PRODUCTION_TARGETED_DIAGNOSTICS.sql` is recommended for production execution now.**

# Production preflight review | IAS Entity Management

**Source:** Uploaded `Results(3).xlsx`, first worksheet, 532 data rows, ZTBLAIS Oracle schema, 2026-11-03 12:23:26 database time. This is **production metadata**, not proof that new SQL was compiled or executed. No production DML/DDL has been performed by the assistant.

## Confirmed baseline

| Object | Count |
|---|---:|
| T_AUDITEE_ENTITIES | 907 |
| T_AUDITEE_ENTITIES_MAPING | 773 |
| AIS_T_AU_POST_COMPLIANCE | 105,434 |
| Duplicate mapping entity IDs | 0 |
| Mapping rows without master | 0 |
| Parent IDs without master | 0 |
| Self-parent mappings | 2 |
| Inactive entities retaining mapping | 6 |
| Mapping rows with null parent | 0 |

Production confirms all required master, mapping, post-compliance and engagement columns. `RELATION_TYPE_ID`, `COMPLICE_BY`, `COMPLIANCE_UNIT`, `ISSUE_RECTIFIED`, `GM_OFFICE` and `AUDITEDBY` all **exist**. All 11 examined package/view objects are VALID; there are no Phase 1 foundation object-name collisions. `MISC` quota is unlimited for the connected account.

## Material design blockers for database write operations

1. **No declared PK/index for either entity master or mapping table:** The preflight lists zero constraints and zero indexes for these two tables. `T_AUDITEE_ENTITIES.ENTITY_ID` and `T_AUDITEE_ENTITIES_MAPING.ENTITY_ID` are both reported nullable. Before new writes, validate master duplicate/null IDs, determine all legacy ID writers, create a safe uniqueness/sequence migration strategy, and test it against legacy `MAX+1` inserts. Do not add a primary key or sequence blindly.
2. **Production para statuses:** `8=17,392`, `9=85,159`, `6=2,740`, `28=116`, `0=1`; additionally **26 one-row statuses** with codes 10442–10470 (gaps present). `PARA_ADDED_ON` null = 0; `SETTELED_ON` null on settled status 9 = 7. These are evidence of existing status diversity, not proof of data corruption; investigate before designing closure status `0`.
3. **Engagement status diversity:** statuses `1:2`, `4:30`, `5:20`, `12:17`, `13:1`, `14:2328`. The prior preview checked only statuses 10–12, which is inadequate. Updated preview reports both engagement states 10–12 and **all engagements not yet at 14** as *review candidates*, not automatic closure blockers.
4. **High downstream coupling:** 327 direct dependency records (207 distinct referenced object names) from the three key tables; 64 dependency records specifically reference the post-compliance source. Nulling para dates or zeroing para statuses will affect historical snapshots unless downstream reporting uses closure-aware history.
5. **Root/inactive mapping exceptions:** Two self-parent and six inactive-with-mapping records need classification before setting hierarchy constraints or auto-deleting mapping.

## Implementation position

- `01_FOUNDATION_DDL.sql` is structurally aligned with the observed source columns. It creates new objects and copies no data. It is still **staging-only until Oracle compilation and rollback tests**.
- `02_READONLY_PACKAGE.sql` has been corrected to flag nonfinal engagement states and unusual para statuses. Its SQL has **not** been Oracle-compiled against production. Never run the package in production until staging validation.
- `05_PRODUCTION_TARGETED_DIAGNOSTICS.sql` is a new **read-only single-result** diagnostic to identify the precise 26 unusual status records, 7 missing settlement dates, one status-0 row, 2 self-parent mappings, 6 inactive mappings, entity-ID uniqueness and current non-final engagement counts.
- Do **not** run a closure update, set DATE fields to zero, delete mappings, or install a write package until audit-history rules, change authority, and master keys are finalized.

## Controlled sequence

1. Run `05_PRODUCTION_TARGETED_DIAGNOSTICS.sql` with Ctrl+Enter and export the single result grid; no changes are made.
2. Verify whether any duplicates or NULL primary identifiers exist and classify para status anomalies.
3. Validate `01_FOUNDATION_DDL.sql`, `02_READONLY_PACKAGE.sql`, `03_VALIDATE_FOUNDATION.sql` and empty-table rollback **in a staging copy**. Compare before/after counts of the three existing tables.
4. Build a dedicated trusted writer package with explicit row locking, maker/checker, sequence migration, append-only event logging, and closure-aware historical reports.
5. After tests, prepare a separate production deployment checklist and approval gate. Oracle DDL auto-commits and cannot be rolled back using `ROLLBACK`.

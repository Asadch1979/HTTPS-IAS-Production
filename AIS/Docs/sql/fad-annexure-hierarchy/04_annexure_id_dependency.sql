-- OPTIONAL historical FK report; NOT a prerequisite for the staging-backed display.
-- The register does not create a physical FK and reports duplicate/null IDs as pending.
-- READ ONLY. A unique index alone is not an Oracle referenced P/U constraint.
-- Never create a duplicate Annexure master, drop the FK, or alter the operational table here.
SET LINESIZE 250
SET PAGESIZE 50000
SPOOL fad-annexure-id-dependency.txt
SELECT COUNT(*) total_rows, COUNT(id) nonnull_ids, COUNT(DISTINCT id) distinct_ids
FROM T_AUDIT_CHECKLIST_ANNEXURE;
SELECT id,COUNT(*) occurrences FROM T_AUDIT_CHECKLIST_ANNEXURE
GROUP BY id HAVING id IS NULL OR COUNT(*)>1;
SELECT c.constraint_name,c.constraint_type,c.status,c.validated,cc.column_name,cc.position
FROM user_constraints c JOIN user_cons_columns cc ON cc.constraint_name=c.constraint_name
WHERE c.table_name='T_AUDIT_CHECKLIST_ANNEXURE' ORDER BY c.constraint_name,cc.position;
SELECT i.index_name,i.uniqueness,ic.column_name,ic.column_position
FROM user_indexes i JOIN user_ind_columns ic ON ic.index_name=i.index_name
WHERE i.table_name='T_AUDIT_CHECKLIST_ANNEXURE' ORDER BY i.index_name,ic.column_position;
SELECT CASE WHEN COUNT(*)>0 THEN 'EXISTING KEY SUPPORTS STRICT FK'
 ELSE 'BLOCKED: SEPARATE SCHEMA-OWNER KEY DECISION REQUIRED; NO TABLE ALTERATION AUTHORIZED'
 END dependency_status
FROM user_constraints c
WHERE c.table_name='T_AUDIT_CHECKLIST_ANNEXURE' AND c.constraint_type IN ('P','U')
 AND c.status='ENABLED' AND c.validated='VALIDATED'
 AND (SELECT COUNT(*) FROM user_cons_columns x WHERE x.constraint_name=c.constraint_name)=1
 AND EXISTS (SELECT 1 FROM user_cons_columns x WHERE x.constraint_name=c.constraint_name AND x.column_name='ID');
SPOOL OFF

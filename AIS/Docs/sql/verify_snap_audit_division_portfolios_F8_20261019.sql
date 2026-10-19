-- Audit Para Snap: existing ZTBL hierarchy preflight (READ ONLY).
-- Run EACH SELECT individually in PL/SQL Developer F8.
-- No new table, no DML, no changes to V_IAS_ENTITY_REPORTING_NAMES.
-- Parent IDs confirmed from user-supplied production results:
--   FAD: 112243 FIELD AUDIT DEPARTMENT
--   MAD: 113254 MANAGEMENT AUDIT DIVISION

-- 1. Direct Department / Cluster dropdown children. Expected: FAD 9 / MAD 3.
WITH divisions AS
(
  SELECT 'FAD' division_code, 112243 parent_id FROM dual
  UNION ALL SELECT 'MAD', 113254 FROM dual
)
SELECT p.division_code,p.parent_id,
       h.entity_id AS child_id,h.entity_name AS child_name,
       h.reporting_1,h.reporting_2
  FROM divisions p
  JOIN V_IAS_ENTITY_REPORTING_NAMES h
    ON h.reporting_1=p.parent_id
 WHERE h.entity_id<>p.parent_id
 ORDER BY p.division_code,h.entity_name,h.entity_id;

-- 2. Ensure each direct child is listed under one division only.
WITH divisions AS
(
  SELECT 'FAD' division_code,112243 parent_id FROM dual
  UNION ALL SELECT 'MAD',113254 FROM dual
)
SELECT h.entity_id,h.entity_name,
       COUNT(DISTINCT p.division_code) AS division_memberships
  FROM divisions p
  JOIN V_IAS_ENTITY_REPORTING_NAMES h ON h.reporting_1=p.parent_id
 WHERE h.entity_id<>p.parent_id
 GROUP BY h.entity_id,h.entity_name
HAVING COUNT(DISTINCT p.division_code)>1;

-- 3. Build the FULL eligible AUDITED_BY_ID set: division itself plus
-- all descendants along reporting levels 1..6 (deduplicated).
WITH divisions AS
(
  SELECT 'FAD' division_code,112243 parent_id FROM dual
  UNION ALL SELECT 'MAD',113254 FROM dual
), members AS
(
  SELECT p.division_code,p.parent_id AS audited_by_id FROM divisions p
  UNION
  SELECT p.division_code,h.entity_id
    FROM divisions p
    JOIN V_IAS_ENTITY_REPORTING_NAMES h
      ON p.parent_id IN
        (h.reporting_1,h.reporting_2,h.reporting_3,
         h.reporting_4,h.reporting_5,h.reporting_6)
)
SELECT m.division_code,m.audited_by_id,
       d.entity_name,
       COUNT(k.audited_by_id) AS lifecycle_rows
  FROM members m
  LEFT JOIN V_IAS_ENTITY_REPORTING_NAMES d
    ON d.entity_id=m.audited_by_id
  LEFT JOIN V_EXEC_KPI_LIFECYCLE k
    ON k.audited_by_id=m.audited_by_id
 GROUP BY m.division_code,m.audited_by_id,d.entity_name
 ORDER BY m.division_code,m.audited_by_id;

-- 4. Check real report AUDITED_BY keys against the same full membership.
WITH divisions AS
(
  SELECT 'FAD' division_code,112243 parent_id FROM dual
  UNION ALL SELECT 'MAD',113254 FROM dual
), members AS
(
  SELECT p.division_code,p.parent_id AS audited_by_id FROM divisions p
  UNION
  SELECT p.division_code,h.entity_id
    FROM divisions p JOIN V_IAS_ENTITY_REPORTING_NAMES h
      ON p.parent_id IN
        (h.reporting_1,h.reporting_2,h.reporting_3,
         h.reporting_4,h.reporting_5,h.reporting_6)
)
SELECT m.division_code,m.audited_by_id,
       COUNT(s.audited_by) AS report_source_rows
  FROM members m
  LEFT JOIN V_EXEC_REPORT_SOURCE s
    ON s.audited_by=m.audited_by_id
 GROUP BY m.division_code,m.audited_by_id
 ORDER BY m.division_code,m.audited_by_id;

-- 5. Ensure NO AUDITED_BY_ID is a descendant of BOTH divisions.
WITH divisions AS
(
  SELECT 'FAD' division_code,112243 parent_id FROM dual
  UNION ALL SELECT 'MAD',113254 FROM dual
), members AS
(
  SELECT p.division_code,p.parent_id AS audited_by_id FROM divisions p
  UNION
  SELECT p.division_code,h.entity_id
    FROM divisions p JOIN V_IAS_ENTITY_REPORTING_NAMES h
      ON p.parent_id IN
        (h.reporting_1,h.reporting_2,h.reporting_3,
         h.reporting_4,h.reporting_5,h.reporting_6)
)
SELECT audited_by_id,COUNT(DISTINCT division_code) divisions
  FROM members
 GROUP BY audited_by_id
HAVING COUNT(DISTINCT division_code)>1;

-- 6. If the old proposal was deployed separately, report its existence.
-- This correction does NOT automatically DROP any Oracle table.
SELECT table_name
  FROM user_tables
 WHERE table_name='T_IAS_SNAP_AUDIT_SCOPE';

-- 7. Confirm package/view dependencies after DBA recompilation.
SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN
 ('PKG_AUDIT_SNAP_SCOPE','PKG_EXEC_DASHBOARD','PKG_EXEC_REPORTS',
  'V_IAS_ENTITY_REPORTING_NAMES','V_EXEC_KPI_LIFECYCLE','V_EXEC_REPORT_SOURCE')
 ORDER BY object_name,object_type;

SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN ('PKG_AUDIT_SNAP_SCOPE','PKG_EXEC_DASHBOARD','PKG_EXEC_REPORTS')
 ORDER BY name,sequence;

-- Compare "All" with the sum of each child; include parent direct audits
-- when applicable. Validate identical date range/filter on all reports.
-- Do not infer that business audited-entity hierarchy implies AUDITED_BY_ID.

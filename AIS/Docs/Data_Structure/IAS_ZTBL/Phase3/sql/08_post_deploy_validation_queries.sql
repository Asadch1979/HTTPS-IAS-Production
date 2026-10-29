/*
  IAS_ZTBL Phase 3 post-deploy validation queries

  Purpose
  - validate that core objects compiled
  - validate lookup coverage
  - validate sequence/trigger creation
  - validate migration-support readiness
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

PROMPT === Invalid Objects ===
SELECT object_type, object_name, status
  FROM user_objects
 WHERE status <> 'VALID'
 ORDER BY object_type, object_name;

PROMPT === Missing Required Lookup Types ===
WITH required_types AS (
    SELECT 'SECURITY_STATUS' lookup_type_code FROM dual UNION ALL
    SELECT 'ENGAGEMENT_TYPE' FROM dual UNION ALL
    SELECT 'OBSERVATION_STATUS' FROM dual UNION ALL
    SELECT 'OBSERVATION_DETAIL_TYPE' FROM dual UNION ALL
    SELECT 'COMPLIANCE_STATUS' FROM dual UNION ALL
    SELECT 'COMPLIANCE_STAGE' FROM dual UNION ALL
    SELECT 'IID_CASE_STATUS' FROM dual UNION ALL
    SELECT 'COMMERCIAL_AUDIT_STATUS' FROM dual UNION ALL
    SELECT 'REPORT_STATE' FROM dual UNION ALL
    SELECT 'REFERENCE_TYPE' FROM dual UNION ALL
    SELECT 'RECIPIENT_TYPE' FROM dual UNION ALL
    SELECT 'ATTACHMENT_LINK_TYPE' FROM dual UNION ALL
    SELECT 'NOTIFICATION_QUEUE_STATUS' FROM dual UNION ALL
    SELECT 'DOCUMENT_TYPE' FROM dual UNION ALL
    SELECT 'PLAN_STATUS' FROM dual
)
SELECT r.lookup_type_code
  FROM required_types r
  LEFT JOIN tbl_lookup_type t
    ON t.lookup_type_code = r.lookup_type_code
 WHERE t.lookup_type_id IS NULL;

PROMPT === Missing Sequences For Tables ===
SELECT t.table_name
  FROM user_tables t
 WHERE t.table_name LIKE 'TBL\_%' ESCAPE '\'
   AND NOT EXISTS (
       SELECT 1
         FROM user_sequences s
        WHERE s.sequence_name = 'SEQ_' || SUBSTR(t.table_name, 5)
   )
 ORDER BY t.table_name;

PROMPT === Missing Before-Insert Triggers For Tables ===
SELECT t.table_name
  FROM user_tables t
 WHERE t.table_name LIKE 'TBL\_%' ESCAPE '\'
   AND NOT EXISTS (
       SELECT 1
         FROM user_triggers trg
        WHERE trg.trigger_name = 'TRG_' || SUBSTR(t.table_name, 5) || '_BIR'
   )
 ORDER BY t.table_name;

PROMPT === Core Table Inventory ===
SELECT COUNT(*) AS table_count
  FROM user_tables
 WHERE table_name LIKE 'TBL\_%' ESCAPE '\';

PROMPT === Core View Inventory ===
SELECT COUNT(*) AS view_count
  FROM user_views
 WHERE view_name LIKE 'VW\_%' ESCAPE '\';

PROMPT === Core Package Inventory ===
SELECT object_type, COUNT(*) AS object_count
  FROM user_objects
 WHERE object_name LIKE 'PKG\_%' ESCAPE '\'
 GROUP BY object_type
 ORDER BY object_type;

PROMPT === Migration Support Tables ===
SELECT table_name
  FROM user_tables
 WHERE table_name IN (
     'TBL_MIGRATION_BATCH',
     'TBL_LEGACY_KEY_MAP',
     'TBL_MIGRATION_ISSUE',
     'TBL_DOCUMENT_MIGRATION_QUEUE'
 )
 ORDER BY table_name;

PROMPT === Lookup Row Counts ===
SELECT lt.lookup_type_code,
       COUNT(lv.lookup_value_id) AS lookup_value_count
  FROM tbl_lookup_type lt
  LEFT JOIN tbl_lookup_value lv
    ON lv.lookup_type_id = lt.lookup_type_id
 GROUP BY lt.lookup_type_code
 ORDER BY lt.lookup_type_code;

PROMPT === Package Dependency Sanity Check ===
SELECT DISTINCT name, referenced_name, referenced_type
  FROM user_dependencies
 WHERE name LIKE 'PKG\_%' ESCAPE '\'
   AND referenced_name LIKE 'TBL\_%' ESCAPE '\'
 ORDER BY name, referenced_name;

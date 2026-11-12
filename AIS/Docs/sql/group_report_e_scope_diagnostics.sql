-- Read-only GROUP Report E preflight. Run in the IAS application schema.
-- Bind :root_id to the AUTHENTICATED UserEntityID from the affected session,
-- :role_id to its verified role, :entity_id / :office_id to request filters
-- (NULL when absent). Do not use an arbitrary browser root or infer it from role.
-- No DML, package execution, grant changes or mapping repairs occur here.

-- Missing/duplicate/conflicting active mappings, including null type values.
SELECT :root_id root_id, :role_id role_id, COUNT(m.entity_id) active_rows,
       COUNT(DISTINCT m.c_type_id) distinct_types,
       COUNT(m.entity_id)-COUNT(m.c_type_id) null_type_rows,
       MIN(m.c_type_id) min_type, MAX(m.c_type_id) max_type,
       CASE WHEN COUNT(m.entity_id)=0 THEN 'MISSING'
            WHEN COUNT(DISTINCT m.c_type_id)<>1 OR COUNT(m.entity_id)<>COUNT(m.c_type_id)
              THEN 'CONFLICT_OR_NULL'
            WHEN COUNT(m.entity_id)>1 THEN 'DUPLICATE_REVIEW_REQUIRED'
            WHEN (:role_id=4 AND MAX(m.c_type_id)=18) OR
                 (:role_id=14 AND MAX(m.c_type_id)=3) THEN 'EXPECTED_ROLE_TYPE'
            ELSE 'INCOMPATIBLE_ROLE_TYPE' END diagnostic
  FROM t_auditee_entities_maping m
 WHERE m.entity_id=:root_id AND NVL(m.status,'Y')='Y';
SELECT entity_id,c_type_id,status FROM t_auditee_entities_maping
 WHERE entity_id=:root_id ORDER BY c_type_id,status;

-- Both sources must contain exactly one root row. Zero rows or duplicates
-- require source investigation, never a broadening of the report scope.
SELECT 'V_IAS_ENTITY_REPORTING_IDS' source_name,COUNT(*) root_rows
 FROM V_IAS_ENTITY_REPORTING_IDS WHERE entity_id=:root_id
UNION ALL
SELECT 'V_EXEC_ENTITY_SCOPE',COUNT(*) FROM V_EXEC_ENTITY_SCOPE WHERE entity_id=:root_id;
SELECT h.* FROM V_IAS_ENTITY_REPORTING_IDS h WHERE h.entity_id=:root_id;
SELECT s.* FROM V_EXEC_ENTITY_SCOPE s WHERE s.entity_id=:root_id;

-- Reproduce current report-package root validation (expected count = 1).
-- A type-18 root is itself level zero in the repository view definition,
-- so BUSINESS_GROUP_ID must equal that root. A type-3 Division is independent
-- of its containing BUSINESS_GROUP_ID; its root is never promoted to that Group.
SELECT COUNT(*) report_root_matches FROM V_EXEC_ENTITY_SCOPE s
 WHERE s.entity_id=:root_id
   AND EXISTS (SELECT 1 FROM t_auditee_entities_maping m
     WHERE m.entity_id=s.entity_id AND NVL(m.status,'Y')='Y'
     GROUP BY m.entity_id
     HAVING COUNT(DISTINCT m.c_type_id)=1
       AND (MAX(m.c_type_id)=3 OR
            (MAX(m.c_type_id)=18 AND s.business_group_id=:root_id)));

-- Exact request filters: zero/duplicate matches also raise ORA-20802.
WITH requested AS (
 SELECT 'ENTITY' filter_name,:entity_id id FROM dual UNION ALL
 SELECT 'REPORTING_OFFICE',:office_id FROM dual)
SELECT r.filter_name,r.id,COUNT(s.entity_id) authorized_rows
 FROM requested r LEFT JOIN V_EXEC_ENTITY_SCOPE s
 ON s.entity_id=r.id AND (s.entity_id=:root_id OR
 :root_id IN (s.reporting_1,s.reporting_2,s.reporting_3,s.reporting_4,s.reporting_5,s.reporting_6))
 WHERE r.id IS NOT NULL GROUP BY r.filter_name,r.id;

-- Compare deployed definitions to repository. P_GET_NODES source is not
-- shipped in this repository: inspect its mapping status, type conflict,
-- root inclusion and duplicate-handling rules before proposing any repair.
SELECT name,type,line,text FROM USER_SOURCE
 WHERE name IN ('PKG_ORG_STRUCTURE','PKG_EXEC_REPORTS')
 ORDER BY name,type,line;
SELECT view_name,text FROM USER_VIEWS
 WHERE view_name IN ('V_EXEC_ENTITY_SCOPE','V_IAS_ENTITY_REPORTING_IDS');
SELECT object_name,object_type,status,last_ddl_time FROM USER_OBJECTS
 WHERE object_name IN ('PKG_ORG_STRUCTURE','PKG_EXEC_REPORTS',
                       'V_EXEC_ENTITY_SCOPE','V_IAS_ENTITY_REPORTING_IDS')
 ORDER BY object_name,object_type;

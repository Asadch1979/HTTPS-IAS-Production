/*
  IAS Organization Structure manual verification script.
  Run in SQL Developer as a script (F5) AFTER deploying PKG_ORG_STRUCTURE.sql.
  No updates, inserts or commits.
*/

-- Confirm both database objects compiled.
SELECT object_name, object_type, status
FROM user_objects
WHERE object_name = 'PKG_ORG_STRUCTURE'
ORDER BY object_type;

-- Inspect errors if either object is INVALID.
SELECT name, type, line, position, text
FROM user_errors
WHERE name = 'PKG_ORG_STRUCTURE'
ORDER BY type, sequence;

-- Check source view uniqueness and number of entities.
SELECT COUNT(*) AS total_rows,
       COUNT(DISTINCT entity_id) AS distinct_entities
FROM V_IAS_ENTITY_REPORTING_NAMES;

-- Check entity type conflicts in the mapping table.
SELECT entity_id, COUNT(DISTINCT c_type_id) AS type_count
FROM T_AUDITEE_ENTITIES_MAPING
GROUP BY entity_id
HAVING COUNT(DISTINCT c_type_id) > 1;

-- Expected chain: 112242 -> 113254 -> 112215 -> 112203 -> 112201.
VARIABLE org_cursor REFCURSOR;
EXEC PKG_ORG_STRUCTURE.P_GET_NODES(113254, :org_cursor);
PRINT org_cursor;

VARIABLE path_cursor REFCURSOR;
EXEC PKG_ORG_STRUCTURE.P_GET_ENTITY_PATH(112242, :path_cursor);
PRINT path_cursor;

-- All entities, including separate roots or detached data:
-- VARIABLE all_nodes REFCURSOR;
-- EXEC PKG_ORG_STRUCTURE.P_GET_NODES(NULL, :all_nodes);
-- PRINT all_nodes;

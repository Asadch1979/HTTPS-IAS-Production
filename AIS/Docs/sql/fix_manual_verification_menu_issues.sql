-- IAS manual-verification menu corrections
-- Review the preview queries before execution. Run once in a controlled release.

SET SERVEROUTPUT ON;

PROMPT Preview duplicate/malformed Authorize Delete Duplicate Para pages
SELECT id, page_name, '[' || page_path || ']' AS page_path, status
  FROM t_menu_pages
 WHERE LOWER(TRIM(page_path)) = 'execution/auth_del_dup_para'
 ORDER BY CASE WHEN page_path = TRIM(page_path) THEN 0 ELSE 1 END, id;

DECLARE
  v_canonical_page_id t_menu_pages.id%TYPE;
BEGIN
  SELECT id
    INTO v_canonical_page_id
    FROM (
      SELECT id
        FROM t_menu_pages
       WHERE LOWER(TRIM(page_path)) = 'execution/auth_del_dup_para'
       ORDER BY CASE WHEN page_path = TRIM(page_path) THEN 0 ELSE 1 END, id
    )
   WHERE ROWNUM = 1;

  UPDATE t_menu_pages
     SET page_path = 'Execution/auth_del_dup_para',
         page_name = 'Authorize Delete Duplicate Para'
   WHERE id = v_canonical_page_id;

  -- Preserve every existing group assignment on the canonical page.
  INSERT INTO t_menu_pages_groupmap (groupmap_id, group_id, page_id)
  SELECT (SELECT NVL(MAX(groupmap_id), 0) FROM t_menu_pages_groupmap) +
         ROW_NUMBER() OVER (ORDER BY source_group.group_id),
         source_group.group_id,
         v_canonical_page_id
    FROM (
      SELECT DISTINCT gm.group_id
        FROM t_menu_pages_groupmap gm
        JOIN t_menu_pages page_record ON page_record.id = gm.page_id
       WHERE LOWER(TRIM(page_record.page_path)) = 'execution/auth_del_dup_para'
    ) source_group
   WHERE NOT EXISTS (
      SELECT 1
        FROM t_menu_pages_groupmap existing_map
       WHERE existing_map.group_id = source_group.group_id
         AND existing_map.page_id = v_canonical_page_id
   );

  DELETE FROM t_menu_pages_groupmap
   WHERE page_id IN (
     SELECT id
       FROM t_menu_pages
      WHERE LOWER(TRIM(page_path)) = 'execution/auth_del_dup_para'
        AND id <> v_canonical_page_id
   );

  DELETE FROM t_menu_pages
   WHERE LOWER(TRIM(page_path)) = 'execution/auth_del_dup_para'
     AND id <> v_canonical_page_id;

  -- Remove duplicate mappings without changing the effective permission set.
  DELETE FROM t_menu_pages_groupmap duplicate_map
   WHERE duplicate_map.page_id = v_canonical_page_id
     AND duplicate_map.rowid NOT IN (
     SELECT MIN(canonical_map.rowid)
       FROM t_menu_pages_groupmap canonical_map
      WHERE canonical_map.page_id = v_canonical_page_id
      GROUP BY canonical_map.group_id, canonical_map.page_id
   );

  -- Display-label corrections only. Internal routes and permission keys remain unchanged.
  UPDATE t_menu_pages
     SET page_name = REPLACE(page_name, 'Reffered Back', 'Referred Back')
   WHERE INSTR(page_name, 'Reffered Back') > 0;

  UPDATE t_menu_pages
     SET page_name = REPLACE(page_name, 'Engagment', 'Engagement')
   WHERE INSTR(page_name, 'Engagment') > 0;

  UPDATE t_menu_pages
     SET page_name = 'Quality Assurance Checking'
   WHERE page_name = 'Quality_Assurance_checking';

  COMMIT;
  DBMS_OUTPUT.PUT_LINE('IAS menu corrections applied. Canonical page ID: ' || v_canonical_page_id);
EXCEPTION
  WHEN NO_DATA_FOUND THEN
    ROLLBACK;
    RAISE_APPLICATION_ERROR(-20001, 'Authorize Delete Duplicate Para page was not found. No changes applied.');
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/

PROMPT Verification
SELECT id, page_name, '[' || page_path || ']' AS page_path, page_key, status
  FROM t_menu_pages
 WHERE LOWER(TRIM(page_path)) = 'execution/auth_del_dup_para';

SELECT group_id, page_id, COUNT(*) AS mapping_count
  FROM t_menu_pages_groupmap
 GROUP BY group_id, page_id
HAVING COUNT(*) > 1;

SELECT id, page_name, '[' || page_path || ']' AS page_path
  FROM t_menu_pages
 WHERE page_path <> TRIM(page_path)
    OR page_name <> TRIM(page_name)
 ORDER BY id;

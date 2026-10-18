-- Development/debug ONLY: register and activate exactly two CIA Command Center
-- read-only APIs. This is not a production rollout. Run manually in the IAS
-- development database as the owning schema after reviewing current grants.
-- Neither Audit Para Snap nor other executive APIs are activated here.
-- Page 1000303 must already be the CIA page; do not invent page IDs or roles.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;
DECLARE
  v_found NUMBER;
  v_api_id NUMBER;
  v_incompatible NUMBER;
  v_page_path VARCHAR2(500);

  PROCEDURE grant_page(p_role NUMBER) IS
  BEGIN
    INSERT INTO T_MENU_PAGES_GROUPMAP (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    SELECT (SELECT NVL(MAX(GROUPMAP_ID),0)+1 FROM T_MENU_PAGES_GROUPMAP),
           p_role,1000303
      FROM dual
     WHERE NOT EXISTS
           (SELECT 1 FROM T_MENU_PAGES_GROUPMAP
             WHERE GROUP_ID=p_role AND PAGE_ID=1000303);
  END;

  PROCEDURE grant_api(p_role NUMBER,p_api NUMBER) IS
  BEGIN
    MERGE INTO T_AU_ROLE_API_PERMISSION tgt
    USING (SELECT p_role AS role_id,p_api AS api_id FROM dual) src
       ON (tgt.ROLE_ID=src.role_id AND tgt.API_ID=src.api_id)
    WHEN MATCHED THEN UPDATE SET tgt.IS_ACTIVE='Y'
    WHEN NOT MATCHED THEN INSERT
         (ROLE_ID,API_ID,IS_ACTIVE,CREATED_BY)
    VALUES (src.role_id,src.api_id,'Y',NULL);
  END;
BEGIN
  -- Serialize MAX+1 page-map allocation and API registrations.
  LOCK TABLE T_MENU_PAGES IN EXCLUSIVE MODE;
  LOCK TABLE T_MENU_PAGES_GROUPMAP IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_API_MASTER IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_ROLE_API_PERMISSION IN EXCLUSIVE MODE;

  SELECT COUNT(*),MIN(PAGE_PATH) INTO v_found,v_page_path
    FROM T_MENU_PAGES WHERE ID=1000303;
  IF v_found<>1 OR
     LOWER(TRIM(BOTH '/' FROM TRIM(v_page_path)))<>'executiveoverview/cia' THEN
    RAISE_APPLICATION_ERROR(-20051,
      'CIA page 1000303 is missing or mapped to an unexpected path.');
  END IF;

  -- IAS role/group IDs 1 (verified Super User) and 3 (CIA) only.
  grant_page(1);
  grant_page(3);

  FOR api IN
    (SELECT 'GetCiaCommandKpis' AS action_name FROM dual
     UNION ALL SELECT 'GetCiaIssueNature' FROM dual)
  LOOP
    SELECT COUNT(*),MIN(API_ID) INTO v_found,v_api_id
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
           LOWER('api/ExecutiveDashboard/'||api.action_name)
       AND UPPER(HTTP_METHOD)='GET';

    IF v_found>1 THEN
      RAISE_APPLICATION_ERROR(-20052,
        'Duplicate CIA API registrations: '||api.action_name);
    END IF;
    IF v_found=0 THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME,PAGE_ID,CONTROLLER_NAME,API_PATH,
         HTTP_METHOD,IS_ACTIVE,CREATED_ON)
      VALUES
        (api.action_name,1000303,'ExecutiveDashboard',
         '/api/ExecutiveDashboard/'||api.action_name,
         'GET','Y',SYSDATE);
      SELECT API_ID INTO v_api_id FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
             LOWER('api/ExecutiveDashboard/'||api.action_name)
         AND UPPER(HTTP_METHOD)='GET';
    ELSE
      SELECT COUNT(*) INTO v_incompatible
        FROM T_AU_API_MASTER
       WHERE API_ID=v_api_id
         AND (NVL(PAGE_ID,-1) NOT IN (1000300,1000303)
              OR NVL(CONTROLLER_NAME,'#')<>'ExecutiveDashboard'
              OR NVL(ACTION_NAME,'#')<>api.action_name);
      IF v_incompatible>0 THEN
        RAISE_APPLICATION_ERROR(-20053,
          'Existing CIA API metadata conflict: '||api.action_name);
      END IF;
      -- Fix the older script's incorrect TOP page mapping for CIA actions.
      UPDATE T_AU_API_MASTER
         SET PAGE_ID=1000303, IS_ACTIVE='Y'
       WHERE API_ID=v_api_id;
    END IF;

    -- No extra role can use these bank-wide CIA endpoints.
    UPDATE T_AU_ROLE_API_PERMISSION
       SET IS_ACTIVE='N'
     WHERE API_ID=v_api_id
       AND ROLE_ID NOT IN (1,3)
       AND NVL(IS_ACTIVE,'N')<>'N';

    grant_api(1,v_api_id);
    grant_api(3,v_api_id);
    DBMS_OUTPUT.PUT_LINE('Enabled CIA debug GET: '||api.action_name);
  END LOOP;
  COMMIT;
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/

-- Confirm only the two expected registered URLs and role grants.
SELECT a.API_ID,a.ACTION_NAME,a.PAGE_ID,a.IS_ACTIVE,
       p.ROLE_ID,p.IS_ACTIVE AS ROLE_GRANT_ACTIVE
  FROM T_AU_API_MASTER a
  LEFT JOIN T_AU_ROLE_API_PERMISSION p ON p.API_ID=a.API_ID
 WHERE LOWER(TRIM(BOTH '/' FROM TRIM(a.API_PATH))) IN
       ('api/executivedashboard/getciacommandkpis',
        'api/executivedashboard/getciaissuenature')
 ORDER BY a.API_ID,p.ROLE_ID;

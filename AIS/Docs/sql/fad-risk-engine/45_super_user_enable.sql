-- Controlled Super User-only activation for the FAD Risk Register Execution APIs.
-- Run AFTER fad-risk-engine/40_permissions.sql, PKG_FAD_RISK_ENGINE compilation,
-- and validation of the existing FADRiskControlPanel/Index page assignment.
-- Source only: never run from application startup. Commit requires DBA approval.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
SET SERVEROUTPUT ON
DECLARE
  v_page_id T_MENU_PAGES.ID%TYPE;
  v_api_id T_AU_API_MASTER.API_ID%TYPE;
  v_count NUMBER;
BEGIN
  SELECT COUNT(*),MAX(id) INTO v_count,v_page_id
    FROM T_MENU_PAGES
   WHERE LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/index'
     AND status='A';
  IF v_count<>1 THEN
    RAISE_APPLICATION_ERROR(-20103,'Active FAD Risk Control Panel Index page must be unique.');
  END IF;

  SELECT COUNT(*) INTO v_count FROM T_GROUPS
   WHERE group_id=1 AND role_id=1 AND status='Y';
  IF v_count<>1 THEN
    RAISE_APPLICATION_ERROR(-20103,'Verified Super User group/role 1/1 is unavailable.');
  END IF;

  SELECT COUNT(*) INTO v_count FROM T_MENU_PAGES_GROUPMAP
   WHERE group_id=1 AND page_id=v_page_id;
  IF v_count<>1 THEN
    RAISE_APPLICATION_ERROR(-20103,'Super User page assignment missing/duplicated. Review IAS administration.');
  END IF;

  -- Check all four routes before changing any permission.
  FOR a IN (
    SELECT 'EngineCatalog' action_name,'GET' verb FROM dual
    UNION ALL SELECT 'EngineResults','GET' FROM dual
    UNION ALL SELECT 'RunEnginePeriod','POST' FROM dual
    UNION ALL SELECT 'RunEngineEntity','POST' FROM dual
  ) LOOP
    SELECT COUNT(*),MAX(api_id) INTO v_count,v_api_id
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM api_path))=
           LOWER('FADRiskControlPanel/'||a.action_name)
       AND UPPER(http_method)=a.verb
       AND page_id=v_page_id;
    IF v_count<>1 THEN
      RAISE_APPLICATION_ERROR(-20103,
        'Missing, duplicated or wrongly anchored engine API: '||a.action_name);
    END IF;

    SELECT COUNT(*) INTO v_count
      FROM T_AU_ROLE_API_PERMISSION
     WHERE api_id=v_api_id AND role_id<>1 AND is_active='Y';
    IF v_count>0 THEN
      RAISE_APPLICATION_ERROR(-20103,
        'Active non-Super-User grant exists for '||a.action_name||
        '. Review before activation.');
    END IF;
  END LOOP;

  -- Activate only this four-route family and grant only verified role 1.
  FOR a IN (
    SELECT 'EngineCatalog' action_name,'GET' verb FROM dual
    UNION ALL SELECT 'EngineResults','GET' FROM dual
    UNION ALL SELECT 'RunEnginePeriod','POST' FROM dual
    UNION ALL SELECT 'RunEngineEntity','POST' FROM dual
  ) LOOP
    SELECT api_id INTO v_api_id
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM api_path))=
           LOWER('FADRiskControlPanel/'||a.action_name)
       AND UPPER(http_method)=a.verb
       AND page_id=v_page_id;

    UPDATE T_AU_API_MASTER SET is_active='Y'
     WHERE api_id=v_api_id AND NVL(is_active,'N')<>'Y';

    MERGE INTO T_AU_ROLE_API_PERMISSION p
    USING (SELECT 1 role_id,v_api_id api_id FROM dual) s
       ON (p.role_id=s.role_id AND p.api_id=s.api_id)
    WHEN MATCHED THEN UPDATE SET is_active='Y'
    WHEN NOT MATCHED THEN INSERT(role_id,api_id,is_active,created_by)
      VALUES(s.role_id,s.api_id,'Y',NULL);
    DBMS_OUTPUT.PUT_LINE('Super User engine API active: '||a.action_name||
      ' / '||a.verb);
  END LOOP;
  COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
-- Reauthenticate Super User so IAS session/API-permission cache is refreshed.

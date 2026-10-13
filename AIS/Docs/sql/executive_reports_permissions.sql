-- New report registrations only. Default INACTIVE. No existing grants/pages modified.
-- Set V_ACTIVE='Y' only after history/package verification and owner approval to enable.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
DECLARE
  V_ACTIVE CHAR(1) := 'N';
  V_TOP_ROLE NUMBER := NULL; -- Verify deployment-specific IDs; no guessed role grants.
  V_GROUP_ROLE NUMBER := NULL;
  V_API_ID NUMBER;
BEGIN
  IF V_ACTIVE NOT IN ('N','Y') OR V_TOP_ROLE <= 0 OR V_GROUP_ROLE <= 0 OR
     V_TOP_ROLE IN (1,3,38) OR V_GROUP_ROLE IN (1,3,38) OR V_TOP_ROLE = V_GROUP_ROLE THEN
    RAISE_APPLICATION_ERROR(-20810,'Invalid report registration settings');
  END IF;
  FOR r IN (SELECT 'GetReportA' action, 'A' report FROM dual UNION ALL
            SELECT 'GetReportB','B' FROM dual UNION ALL SELECT 'GetReportC','C' FROM dual UNION ALL
            SELECT 'GetReportD','D' FROM dual UNION ALL SELECT 'GetReportE','E' FROM dual) LOOP
    MERGE INTO T_AU_API_MASTER m
    USING (SELECT '/api/ExecutiveReports/' || r.action path FROM dual) s
       ON (LOWER(m.api_path) = LOWER(s.path) AND UPPER(m.http_method) = 'GET')
    WHEN MATCHED THEN UPDATE SET m.is_active = V_ACTIVE
    WHEN NOT MATCHED THEN INSERT (ACTION_NAME,PAGE_ID,CONTROLLER_NAME,API_PATH,HTTP_METHOD,IS_ACTIVE,CREATED_ON)
      VALUES (r.action,1000300,'ExecutiveReports',s.path,'GET',V_ACTIVE,SYSDATE);
    SELECT api_id INTO V_API_ID FROM T_AU_API_MASTER
      WHERE LOWER(api_path) = LOWER('/api/ExecutiveReports/' || r.action) AND UPPER(http_method) = 'GET';
    FOR role_row IN (SELECT 1 role_id FROM dual UNION SELECT 3 FROM dual
                    UNION SELECT 38 FROM dual WHERE r.report IN ('B','C','D')
                    UNION SELECT V_TOP_ROLE FROM dual WHERE V_TOP_ROLE IS NOT NULL
                    UNION SELECT V_GROUP_ROLE FROM dual WHERE V_GROUP_ROLE IS NOT NULL AND r.report = 'E') LOOP
      MERGE INTO T_AU_ROLE_API_PERMISSION p
      USING (SELECT role_row.role_id role_id, V_API_ID api_id FROM dual) s
        ON (p.role_id = s.role_id AND p.api_id = s.api_id)
      WHEN MATCHED THEN UPDATE SET p.is_active = V_ACTIVE
      WHEN NOT MATCHED THEN INSERT (ROLE_ID,API_ID,IS_ACTIVE,CREATED_BY) VALUES (s.role_id,s.api_id,V_ACTIVE,NULL);
    END LOOP;
  END LOOP;
  COMMIT;
END;
/

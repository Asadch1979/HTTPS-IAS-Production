-- Oracle 18c. Run using F8 only after reviewing the target roles and API grants.
-- Reporting Line Management uses the existing Dashboard/entity_wise_obs page permission.
-- Each API also checks the authenticated session entity; ordinary user requests
-- cannot claim TOP/GROUP/CCO/CIA without the existing verified role mapping.
-- This script does not grant any new page permission.
DECLARE
  V_PAGE_ID NUMBER;
  V_API_ID NUMBER;
  V_ENABLE CHAR(1) := 'N'; -- DBA: change to Y only after approval and testing.
BEGIN
  IF V_ENABLE NOT IN ('Y','N') OR V_ENABLE IS NULL THEN
    RAISE_APPLICATION_ERROR(-20861,'Set a valid activation value');
  END IF;
  SELECT ID INTO V_PAGE_ID
  FROM T_MENU_PAGES
  WHERE LOWER(TRIM(BOTH '/' FROM PAGE_PATH)) = 'dashboard/entity_wise_obs'
    AND ROWNUM = 1;

  IF V_ENABLE = 'Y' THEN
    FOR a IN (
      SELECT 'GetKpiSummary' action_name FROM DUAL UNION ALL
      SELECT 'GetRiskSummary' FROM DUAL UNION ALL
      SELECT 'GetAgingSummary' FROM DUAL UNION ALL
      SELECT 'GetCycleSummary' FROM DUAL UNION ALL
      SELECT 'GetSettlementTrend' FROM DUAL UNION ALL
      SELECT 'GetCriticalItems' FROM DUAL
    ) LOOP
      SELECT API_ID INTO V_API_ID
      FROM T_AU_API_MASTER
      WHERE LOWER(API_PATH) = LOWER('/api/ExecutiveDashboard/'||a.action_name)
        AND UPPER(HTTP_METHOD) = 'GET' AND IS_ACTIVE = 'Y';

      FOR role_row IN (
        SELECT DISTINCT GROUP_ID role_id
        FROM T_MENU_PAGES_GROUPMAP
        WHERE PAGE_ID = V_PAGE_ID AND GROUP_ID > 0
      ) LOOP
        MERGE INTO T_AU_ROLE_API_PERMISSION target
        USING (SELECT role_row.role_id role_id, V_API_ID api_id FROM DUAL) src
          ON (target.ROLE_ID = src.role_id AND target.API_ID = src.api_id)
        WHEN MATCHED THEN UPDATE SET IS_ACTIVE = 'Y'
        WHEN NOT MATCHED THEN
          INSERT (ROLE_ID,API_ID,IS_ACTIVE,CREATED_BY)
          VALUES (src.role_id,src.api_id,'Y',NULL);
      END LOOP;
    END LOOP;
  END IF;
END;
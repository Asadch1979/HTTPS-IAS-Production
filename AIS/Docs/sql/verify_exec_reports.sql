-- VERIFY DIRECT IAS SOURCES. SQL Developer: Run Script (F5).
-- Usage: @verify_exec_reports.sql 2026-01-01 2026-09-30 TOP 0
-- For GROUP, supply a real category-18 Group Head entity instead of 0.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SELECT object_name,object_type,status,last_ddl_time
  FROM user_objects
 WHERE object_name IN ('PKG_EXEC_REPORTS','V_EXEC_REPORT_SOURCE')
 ORDER BY object_name,object_type;
SELECT name,type,line,position,text FROM user_errors
 WHERE name IN ('PKG_EXEC_REPORTS','V_EXEC_REPORT_SOURCE')
 ORDER BY name,type,sequence;
SELECT COUNT(*) source_cases, COUNT(DISTINCT com_id) unique_cases,
       SUM(CASE WHEN para_status=9 AND setteled_on IS NULL THEN 1 ELSE 0 END) missing_settlement_dates,
       SUM(CASE WHEN para_status=8 AND setteled_on IS NOT NULL THEN 1 ELSE 0 END) open_with_old_settlement_date
  FROM AIS_T_AU_POST_COMPLIANCE;
DECLARE
  p_start DATE := TO_DATE('&1','YYYY-MM-DD');
  p_end DATE := TO_DATE('&2','YYYY-MM-DD');
  p_type VARCHAR2(10) := UPPER('&3');
  p_root NUMBER := NULLIF(TO_NUMBER('&4'),0);
  cur SYS_REFCURSOR;
  c INTEGER;
  cols DBMS_SQL.DESC_TAB2;
  ncols INTEGER;
  str VARCHAR2(4000);
  dt DATE;
  num NUMBER;
  ids NUMBER;
  total_paras NUMBER;
  opening NUMBER;
  added NUMBER;
  settled NUMBER;
  outstanding NUMBER;
  diff NUMBER;
  col VARCHAR2(128);
  rows_read NUMBER;
  PROCEDURE CHECK_REPORT(p_id VARCHAR2, p_rc IN OUT SYS_REFCURSOR) IS
  BEGIN
    c := DBMS_SQL.TO_CURSOR_NUMBER(p_rc);
    DBMS_SQL.DESCRIBE_COLUMNS2(c,ncols,cols);
    FOR i IN 1..ncols LOOP
      IF cols(i).col_type=2 THEN DBMS_SQL.DEFINE_COLUMN(c,i,num);
      ELSIF cols(i).col_type=12 THEN DBMS_SQL.DEFINE_COLUMN(c,i,dt);
      ELSE DBMS_SQL.DEFINE_COLUMN(c,i,str,4000);
      END IF;
    END LOOP;
    rows_read := 0;
    WHILE DBMS_SQL.FETCH_ROWS(c)>0 LOOP
      rows_read := rows_read + 1;
      total_paras := NULL; opening := NULL; added := NULL;
      settled := NULL; outstanding := NULL; diff := NULL;
      FOR i IN 1..ncols LOOP
        col := cols(i).col_name;
        IF cols(i).col_type=2 THEN
          DBMS_SQL.COLUMN_VALUE(c,i,num);
          CASE col
            WHEN 'TOTAL_PARAS' THEN total_paras := num;
            WHEN 'OPENING_BALANCE' THEN opening := num;
            WHEN 'PARAS_ADDED' THEN added := num;
            WHEN 'SETTLED_PARAS' THEN settled := num;
            WHEN 'OUTSTANDING_PARAS' THEN outstanding := num;
            WHEN 'RECONCILIATION_DIFFERENCE' THEN diff := num;
            ELSE NULL;
          END CASE;
        ELSIF cols(i).col_type=12 THEN DBMS_SQL.COLUMN_VALUE(c,i,dt);
        ELSE DBMS_SQL.COLUMN_VALUE(c,i,str);
        END IF;
      END LOOP;
      IF total_paras<>opening+added OR
         outstanding<>opening+added-settled+diff THEN
        RAISE_APPLICATION_ERROR(-20895,p_id||' report balance mismatch');
      END IF;
    END LOOP;
    DBMS_SQL.CLOSE_CURSOR(c);
    DBMS_OUTPUT.PUT_LINE('Report '||p_id||': '||rows_read||' rows; balance checks passed');
  EXCEPTION WHEN OTHERS THEN
    IF c IS NOT NULL AND DBMS_SQL.IS_OPEN(c) THEN DBMS_SQL.CLOSE_CURSOR(c); END IF;
    RAISE;
  END;
BEGIN
  PKG_EXEC_REPORTS.P_ANNUAL_FIELD_PROGRESS(p_type,p_root,p_start,p_end,NULL,NULL,NULL,NULL,cur);
  CHECK_REPORT('A',cur);
  PKG_EXEC_REPORTS.P_FIELD_OFFICE_POSITION(p_type,p_root,p_start,p_end,NULL,NULL,NULL,NULL,cur);
  CHECK_REPORT('B',cur);
  PKG_EXEC_REPORTS.P_GM_REGION_POSITION(p_type,p_root,p_start,p_end,NULL,NULL,NULL,NULL,cur);
  CHECK_REPORT('C',cur);
  PKG_EXEC_REPORTS.P_REGION_RANKINGS(p_type,p_root,p_start,p_end,NULL,NULL,NULL,NULL,cur);
  CHECK_REPORT('D',cur);
  PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT(p_type,p_root,p_start,p_end,NULL,NULL,NULL,NULL,cur);
  CHECK_REPORT('E',cur);
  DBMS_OUTPUT.PUT_LINE('Verify B/C grand totals, region ties, audit families, estimates and source split with IAS report owners.');
END;
/

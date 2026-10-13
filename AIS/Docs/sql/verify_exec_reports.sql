-- OWNER ONLY. Run after package compilation and certified evidence import.
-- Usage: @verify_exec_reports.sql START_YYYY-MM-DD END_YYYY-MM-DD TYPE GROUP_ROOT_OR_0 ENTITY_OR_0
-- Add intended dimension/office binds below and repeat. No screenshot/sample data inserted.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
DECLARE
  d_start DATE := TO_DATE('&1','YYYY-MM-DD');
  d_end DATE := TO_DATE('&2','YYYY-MM-DD');
  perspective VARCHAR2(20) := UPPER('&3');
  root_id NUMBER := NULLIF(TO_NUMBER('&4'),0);
  entity_id NUMBER := NULLIF(TO_NUMBER('&5'),0);
  entity_type_id NUMBER := NULL;
  audited_by_id NUMBER := NULL;
  office_id NUMBER := NULL;
  TYPE metric_map IS TABLE OF NUMBER INDEX BY VARCHAR2(100);
  b_totals metric_map; c_totals metric_map; other_totals metric_map;
  rc SYS_REFCURSOR;
  n NUMBER;
  PROCEDURE require(ok BOOLEAN, message VARCHAR2) IS
  BEGIN IF NOT ok OR ok IS NULL THEN RAISE_APPLICATION_ERROR(-20890,message); END IF; END;
  PROCEDURE check_cursor(report_id VARCHAR2, cursor_value IN OUT SYS_REFCURSOR, grand OUT metric_map) IS
    cursor_id INTEGER;
    count_columns INTEGER;
    columns DBMS_SQL.DESC_TAB2;
    text_value VARCHAR2(4000); date_value DATE; number_value NUMBER;
    row_kind VARCHAR2(30); rank_group VARCHAR2(30); group_key VARCHAR2(100);
    values_by_name metric_map; details metric_map; group_details metric_map;
    key_name VARCHAR2(100); group_metric VARCHAR2(100);
    rows_seen NUMBER := 0; top_count NUMBER := 0; bottom_count NUMBER := 0;
  BEGIN
    cursor_id := DBMS_SQL.TO_CURSOR_NUMBER(cursor_value);
    DBMS_SQL.DESCRIBE_COLUMNS2(cursor_id,count_columns,columns);
    FOR i IN 1..count_columns LOOP
      IF columns(i).col_type = 2 THEN DBMS_SQL.DEFINE_COLUMN(cursor_id,i,number_value);
      ELSIF columns(i).col_type = 12 THEN DBMS_SQL.DEFINE_COLUMN(cursor_id,i,date_value);
      ELSE DBMS_SQL.DEFINE_COLUMN(cursor_id,i,text_value,4000); END IF;
    END LOOP;
    WHILE DBMS_SQL.FETCH_ROWS(cursor_id) > 0 LOOP
      rows_seen := rows_seen + 1; row_kind := NULL; rank_group := NULL; group_key := '#NULL'; values_by_name.DELETE;
      FOR i IN 1..count_columns LOOP
        IF columns(i).col_type = 2 THEN
          DBMS_SQL.COLUMN_VALUE(cursor_id,i,number_value);
          values_by_name(columns(i).col_name) := number_value;
          IF columns(i).col_name = 'GROUP_ID' AND number_value IS NOT NULL THEN group_key := TO_CHAR(number_value); END IF;
        ELSIF columns(i).col_type = 12 THEN DBMS_SQL.COLUMN_VALUE(cursor_id,i,date_value);
        ELSE
          DBMS_SQL.COLUMN_VALUE(cursor_id,i,text_value);
          IF columns(i).col_name = 'ROW_KIND' THEN row_kind := text_value; END IF;
          IF columns(i).col_name = 'RANKING_GROUP' THEN rank_group := text_value; END IF;
        END IF;
      END LOOP;
      require(values_by_name('TOTAL_PARAS') = values_by_name('OPENING_BALANCE') + values_by_name('PARAS_ADDED'),'Opening/addition reconciliation');
      require(values_by_name('RECONCILIATION_DIFFERENCE') = 0,'Historical closing reconciliation');
      require(values_by_name('OUTSTANDING_PARAS') = values_by_name('TOTAL_PARAS') - values_by_name('SETTLED_PARAS')
        + values_by_name('REOPENING_EVENTS') - values_by_name('RESETTLEMENT_EVENTS') - values_by_name('INITIALLY_CLOSED_ADDITIONS'),'Closing/adjustment reconciliation');
      require(values_by_name('LOW_RISK') + values_by_name('MEDIUM_RISK') + values_by_name('HIGH_RISK')
        + values_by_name('UNRATED_RISK') = values_by_name('OUTSTANDING_PARAS'),'Closing risk reconciliation');
      require(values_by_name('SETTLED_AUDIT') + values_by_name('SETTLED_COMPLIANCE') + values_by_name('SETTLED_UNKNOWN')
        = values_by_name('SETTLED_PARAS'),'Unique settlement-source reconciliation');
      require(values_by_name('SETTLED_UNKNOWN') = 0,'Settlement sources remain unverified; do not enable source breakdowns');
      IF values_by_name('TOTAL_PARAS') = 0 THEN
        require(values_by_name('SETTLEMENT_PERCENTAGE') IS NULL,'Zero denominator must not be ranked at zero percent');
      ELSE
        require(values_by_name('SETTLEMENT_PERCENTAGE') = ROUND(100*values_by_name('SETTLED_PARAS')/values_by_name('TOTAL_PARAS'),2),'Consistent ratio denominator');
      END IF;
      IF report_id = 'D' THEN
        IF rank_group = 'TOP' THEN top_count := top_count + 1;
        ELSIF rank_group = 'BOTTOM' THEN bottom_count := bottom_count + 1;
        ELSE require(rank_group IN ('ZERO_DENOMINATOR','UNASSIGNED_REGION'),'Excluded region must disclose reason'); END IF;
        IF rank_group IN ('TOP','BOTTOM') THEN
          require(values_by_name('TOTAL_PARAS') > 0 AND values_by_name('ITEM_ID') IS NOT NULL,'Invalid ranking candidate');
          require(values_by_name('RANK_POSITION') BETWEEN 1 AND 5,'Invalid rank');
        END IF;
      ELSIF report_id IN ('B','C','E') THEN
        key_name := values_by_name.FIRST;
        WHILE key_name IS NOT NULL LOOP
          IF key_name NOT IN ('GROUP_ID','ITEM_ID','RANK_POSITION','SETTLEMENT_PERCENTAGE') THEN
            group_metric := group_key || ':' || key_name;
            IF row_kind = 'DETAIL' THEN
              IF NOT details.EXISTS(key_name) THEN details(key_name) := 0; END IF;
              IF NOT group_details.EXISTS(group_metric) THEN group_details(group_metric) := 0; END IF;
              details(key_name) := details(key_name) + values_by_name(key_name);
              group_details(group_metric) := group_details(group_metric) + values_by_name(key_name);
            ELSIF row_kind = 'SUBTOTAL' THEN
              require(group_details.EXISTS(group_metric),'Subtotal has no detail');
              require(group_details(group_metric) = values_by_name(key_name),'Subtotal/detail mismatch');
            ELSIF row_kind = 'TOTAL' THEN grand(key_name) := values_by_name(key_name);
            END IF;
          END IF;
          key_name := values_by_name.NEXT(key_name);
        END LOOP;
      END IF;
    END LOOP;
    DBMS_SQL.CLOSE_CURSOR(cursor_id);
    IF report_id IN ('B','C','E') THEN
      key_name := grand.FIRST;
      WHILE key_name IS NOT NULL LOOP
        IF details.EXISTS(key_name) THEN require(grand(key_name) = details(key_name),'Grand-total/detail mismatch');
        ELSE require(grand(key_name) = 0,'Nonzero total without detail'); END IF;
        key_name := grand.NEXT(key_name);
      END LOOP;
      require(grand.EXISTS('TOTAL_PARAS'),'Missing grand total');
    END IF;
    require(top_count <= 5 AND bottom_count <= 5,'Too many ranked regions');
    DBMS_OUTPUT.PUT_LINE('Report ' || report_id || ' passed row/totals checks; rows=' || rows_seen);
  EXCEPTION WHEN OTHERS THEN
    IF cursor_id IS NOT NULL AND DBMS_SQL.IS_OPEN(cursor_id) THEN DBMS_SQL.CLOSE_CURSOR(cursor_id); END IF;
    RAISE;
  END;
BEGIN
  SELECT COUNT(*) INTO n FROM USER_OBJECTS WHERE object_name IN
    ('PKG_EXEC_REPORTS','V_EXEC_REPORT_PORTFOLIO','EXEC_REPORT_FACT','EXEC_REPORT_FACTS') AND status <> 'VALID';
  require(n = 0,'Invalid report objects');
  SELECT COUNT(*) INTO n FROM USER_ERRORS WHERE name IN ('PKG_EXEC_REPORTS','V_EXEC_REPORT_PORTFOLIO','EXEC_REPORT_FACT','EXEC_REPORT_FACTS');
  require(n = 0,'Report compile errors');
  SELECT COUNT(*) INTO n FROM USER_OBJECTS WHERE object_name = 'PKG_EXEC_REPORTS' AND object_type = 'PACKAGE BODY' AND status = 'VALID';
  require(n = 1,'Missing compiled package body');
  PKG_EXEC_REPORTS.P_ANNUAL_FIELD_PROGRESS(perspective,root_id,d_start,d_end,entity_type_id,audited_by_id,office_id,entity_id,rc);
  check_cursor('A',rc,other_totals);
  PKG_EXEC_REPORTS.P_FIELD_OFFICE_POSITION(perspective,root_id,d_start,d_end,entity_type_id,audited_by_id,office_id,entity_id,rc);
  check_cursor('B',rc,b_totals);
  PKG_EXEC_REPORTS.P_GM_REGION_POSITION(perspective,root_id,d_start,d_end,entity_type_id,audited_by_id,office_id,entity_id,rc);
  check_cursor('C',rc,c_totals);
  DECLARE k VARCHAR2(100); BEGIN
    k := b_totals.FIRST;
    WHILE k IS NOT NULL LOOP require(c_totals(k) = b_totals(k),'Field office/GM grand totals differ'); k := b_totals.NEXT(k); END LOOP;
  END;
  PKG_EXEC_REPORTS.P_REGION_RANKINGS(perspective,root_id,d_start,d_end,entity_type_id,audited_by_id,office_id,entity_id,rc);
  check_cursor('D',rc,other_totals);
  PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT(perspective,root_id,d_start,d_end,entity_type_id,audited_by_id,office_id,entity_id,rc);
  check_cursor('E',rc,other_totals);
  BEGIN
    PKG_EXEC_REPORTS.P_FIELD_OFFICE_POSITION('GROUP',NULL,d_start,d_end,NULL,NULL,NULL,NULL,rc);
    RAISE_APPLICATION_ERROR(-20890,'Missing GROUP root was accepted');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE <> -20802 THEN RAISE; END IF; END;
  BEGIN
    PKG_EXEC_REPORTS.P_FIELD_OFFICE_POSITION('TOP',NULL,d_end+1,d_start,NULL,NULL,NULL,NULL,rc);
    RAISE_APPLICATION_ERROR(-20890,'Reversed dates were accepted');
  EXCEPTION WHEN OTHERS THEN IF SQLCODE <> -20801 THEN RAISE; END IF; END;
  DBMS_OUTPUT.PUT_LINE('SQL checks passed. Still verify actual source events, tie order and boundary timestamps manually.');
END;
/
-- Compare underlying evidence to real source IDs for a sample of cases, including:
-- creation before/on Start, end-day timestamps, already-settled imports, settle-reopen-
-- resettle, risk changes, duplicate source logs, transfers, unknown source/risk and
-- out-of-hierarchy Entity/Office. Repeat using genuine GROUP roots and forbidden IDs.
-- Keep API registrations/config disabled until these checks and report-owner comparison pass.
-- Current-state comparison is QA ONLY, not historical reconstruction. A mismatch
-- exposes incomplete evidence/certification; never fill events from current status.
WITH ordered_events AS (
  SELECT com_id, MAX(after_open) KEEP (DENSE_RANK LAST ORDER BY event_on,event_order) last_open
  FROM T_EXEC_REPORT_HISTORY_EVENT WHERE event_on <= SYSDATE GROUP BY com_id
)
SELECT h.com_id, h.initial_open, e.last_open, c.para_status current_master_status,
       h.history_complete_through
FROM T_EXEC_REPORT_CASE_HISTORY h
JOIN AIS_T_AU_POST_COMPLIANCE c ON c.com_id = h.com_id
LEFT JOIN ordered_events e ON e.com_id = h.com_id
WHERE h.history_complete_through >= SYSDATE
  AND ((c.para_status = 8 AND NVL(e.last_open,h.initial_open) <> 1)
    OR (c.para_status = 9 AND NVL(e.last_open,h.initial_open) <> 0));
-- No rows expected after certification. Confirm other master status categories manually.

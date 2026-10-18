-- CIA CHECKLIST_ROOT population based on verified IAS audit period 5 results
-- Development database only. DBA/operator review required before execution.
-- Requires T_IAS_CIA_ISSUE_NATURE_MAP (fresh DDL in
-- T_IAS_CIA_ISSUE_NATURE_MAP.sql, or existing table with old constraint).
--
-- Eight verified T_AUDIT_CHECKLIST.T_IDs:
-- 1=KYC/AML/CFT, 5=LOAN AND ADVANCES, 6=CASH MANAGEMENT,
-- 8=ACCOUNTING/REPORTING/RECONCILIATION, 9=RECORD MANAGEMENT,
-- 14=MANAGEMENT AUDIT, 15=BANKING OPERATIONS, 16=BRANCH GENERAL CONTROLS.
-- Accounts and IS Audit have NO verified checklist-root IDs; do not invent.
--
-- Execute as an entire script (F5/SQL*Plus), not F8 one-line chunks.
-- Oracle DDL implicitly commits. A wider replacement CHECK constraint is
-- added BEFORE the old narrow constraint is removed, so a failed run
-- cannot leave SOURCE_TYPE unconstrained. No production deployment.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;

-- Preflight: terminate before DDL/DML if the live master IDs changed.
DECLARE
  v_bad NUMBER;
  v_count NUMBER;
BEGIN
  WITH approved AS
  (
    SELECT 1 id,'KYC/AML/CFT' heading FROM dual UNION ALL
    SELECT 5,'LOANANDADVANCES' FROM dual UNION ALL
    SELECT 6,'CASHMANAGEMENT' FROM dual UNION ALL
    SELECT 8,'ACCOUNTING/REPORTING/RECONCILIATION' FROM dual UNION ALL
    SELECT 9,'RECORDMANAGEMENT' FROM dual UNION ALL
    SELECT 14,'MANAGEMENTAUDIT' FROM dual UNION ALL
    SELECT 15,'BANKINGOPERATIONS' FROM dual UNION ALL
    SELECT 16,'BRANCHGENERALCONTROLS' FROM dual
  )
  SELECT COUNT(*),NVL(SUM(CASE WHEN c.t_id IS NULL
         OR REGEXP_REPLACE(UPPER(TRIM(c.heading)),'[[:space:]]+','')
            <> a.heading THEN 1 ELSE 0 END),0)
    INTO v_count,v_bad
    FROM approved a LEFT JOIN t_audit_checklist c ON c.t_id=a.id;
  IF v_count<>8 OR v_bad>0 THEN
    RAISE_APPLICATION_ERROR(-20091,
      'Checklist master ID/heading validation failed. Review the eight IDs.');
  END IF;

  SELECT COUNT(*) INTO v_bad
    FROM t_ias_cia_issue_nature_map t
    JOIN
    (
      SELECT 1 id,'AML_CFT' code FROM dual UNION ALL
      SELECT 5,'LOAN_ADVANCES' FROM dual UNION ALL
      SELECT 6,'CASH_MANAGEMENT' FROM dual UNION ALL
      SELECT 8,'ACCOUNTING_REPORTING_RECON' FROM dual UNION ALL
      SELECT 9,'RECORD_MANAGEMENT' FROM dual UNION ALL
      SELECT 14,'MANAGEMENT_AUDIT' FROM dual UNION ALL
      SELECT 15,'BANKING_OPERATIONS' FROM dual UNION ALL
      SELECT 16,'BRANCH_GENERAL_CONTROLS' FROM dual
    ) a ON a.id=t.source_id
   WHERE t.source_type='CHECKLIST_ROOT'
     AND (t.category_code<>a.code OR t.is_active<>'Y');
  IF v_bad>0 THEN
    RAISE_APPLICATION_ERROR(-20092,
       'Existing CHECKLIST_ROOT records conflict or are intentionally inactive.');
  END IF;
  DBMS_OUTPUT.PUT_LINE('Preflight PASS: 8 authoritative checklist roots verified.');
END;
/

-- Upgrade only the SOURCE_TYPE check constraint, never the 10-category
-- CK_IAS_CIA_NATURE_CATEGORY list. Idempotent after partial execution.
DECLARE
  v_rule VARCHAR2(4000);
  v_new_rule VARCHAR2(4000);
  v_temp NUMBER;
BEGIN
  SELECT MAX(search_condition_vc)
    INTO v_rule
    FROM user_constraints
   WHERE table_name='T_IAS_CIA_ISSUE_NATURE_MAP'
     AND constraint_name='CK_IAS_CIA_NATURE_SOURCE'
     AND constraint_type='C';

  SELECT COUNT(*) INTO v_temp
    FROM user_constraints
   WHERE table_name='T_IAS_CIA_ISSUE_NATURE_MAP'
     AND constraint_name='CK_IAS_CIA_NATURE_SOURCE_V2'
     AND constraint_type='C';

  IF v_rule IS NOT NULL AND INSTR(UPPER(v_rule),'CHECKLIST_ROOT')>0 THEN
    DBMS_OUTPUT.PUT_LINE('CHECKLIST_ROOT already allowed by current constraint.');
  ELSE
    IF v_rule IS NOT NULL AND
       (INSTR(UPPER(v_rule),'VIOLATION_SUB')=0 OR
        INSTR(UPPER(v_rule),'AUDIT_PORTFOLIO')=0) THEN
      RAISE_APPLICATION_ERROR(-20093,
        'Unexpected source check definition; manual review required.');
    END IF;
    IF v_temp=0 THEN
      EXECUTE IMMEDIATE
        'ALTER TABLE T_IAS_CIA_ISSUE_NATURE_MAP ADD CONSTRAINT '||
        'CK_IAS_CIA_NATURE_SOURCE_V2 CHECK (SOURCE_TYPE IN '||
        '(''CHECKLIST_ROOT'',''VIOLATION_SUB'',''ANNEX'',''AUDIT_PORTFOLIO''))';
    END IF;
    IF v_rule IS NOT NULL THEN
      EXECUTE IMMEDIATE
        'ALTER TABLE T_IAS_CIA_ISSUE_NATURE_MAP '||
        'DROP CONSTRAINT CK_IAS_CIA_NATURE_SOURCE';
    END IF;
    EXECUTE IMMEDIATE
      'ALTER TABLE T_IAS_CIA_ISSUE_NATURE_MAP '||
      'RENAME CONSTRAINT CK_IAS_CIA_NATURE_SOURCE_V2 '||
      'TO CK_IAS_CIA_NATURE_SOURCE';
    DBMS_OUTPUT.PUT_LINE('CHECKLIST_ROOT source type enabled.');
  END IF;
END;
/

-- Only INSERT missing, approved records. Do not override an existing mapping,
-- activation choice or match priority. Repeatable without duplicate rows.
MERGE INTO T_IAS_CIA_ISSUE_NATURE_MAP t
USING
(
  SELECT 'CHECKLIST_ROOT' source_type,1 source_id,
         'AML_CFT' category_code,10 match_priority FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',5,'LOAN_ADVANCES',10 FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',6,'CASH_MANAGEMENT',10 FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',8,'ACCOUNTING_REPORTING_RECON',10 FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',9,'RECORD_MANAGEMENT',10 FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',14,'MANAGEMENT_AUDIT',10 FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',15,'BANKING_OPERATIONS',10 FROM dual
  UNION ALL SELECT 'CHECKLIST_ROOT',16,'BRANCH_GENERAL_CONTROLS',10 FROM dual
) s
ON (t.source_type=s.source_type AND t.source_id=s.source_id)
WHEN NOT MATCHED THEN
  INSERT (SOURCE_TYPE,SOURCE_ID,CATEGORY_CODE,MATCH_PRIORITY,IS_ACTIVE)
  VALUES (s.source_type,s.source_id,s.category_code,s.match_priority,'Y');

COMMIT;

SELECT source_type,source_id,category_code,match_priority,is_active
  FROM t_ias_cia_issue_nature_map
 WHERE source_type='CHECKLIST_ROOT'
 ORDER BY source_id;

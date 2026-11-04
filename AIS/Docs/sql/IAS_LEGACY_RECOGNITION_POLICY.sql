-- Source-only approved-policy manifest. Intentionally EMPTY: repository sources
-- establish PARA_ADDED_ON reporting semantics but contain no approved date rule.
-- Each reviewed record supplies: auditYear, recognitionOn (YYYY-MM-DD),
-- approvedBy (PP number), approvedOn (YYYY-MM-DD), approvalReference, rule.
-- No current-date fallback, date extrapolation or runtime user policy editing.
DECLARE
  v_approved_policy CLOB := '[]';
  v_count NUMBER;
  v_date DATE;
  v_approved DATE;
BEGIN
  FOR p IN (SELECT j.* FROM JSON_TABLE(v_approved_policy,'$[*]' COLUMNS (
    audit_year NUMBER PATH '$.auditYear' ERROR ON ERROR,
    recognition_on VARCHAR2(10) PATH '$.recognitionOn' ERROR ON ERROR,
    approved_by NUMBER PATH '$.approvedBy' ERROR ON ERROR,
    approved_on VARCHAR2(10) PATH '$.approvedOn' ERROR ON ERROR,
    approval_reference VARCHAR2(1000) PATH '$.approvalReference' ERROR ON ERROR,
    rule VARCHAR2(900) PATH '$.rule' ERROR ON ERROR)) j) LOOP
    IF p.audit_year IS NULL OR p.audit_year NOT BETWEEN 1900 AND 2025
      OR p.audit_year<>TRUNC(p.audit_year) OR p.recognition_on IS NULL
      OR p.approved_by IS NULL OR p.approved_on IS NULL
      OR TRIM(p.approval_reference) IS NULL OR TRIM(p.rule) IS NULL THEN
      RAISE_APPLICATION_ERROR(-20805,'Incomplete approved historical recognition policy.');
    END IF;
    v_date:=TO_DATE(p.recognition_on,'FXYYYY-MM-DD');
    v_approved:=TO_DATE(p.approved_on,'FXYYYY-MM-DD');
    SELECT COUNT(*) INTO v_count FROM T_AU_OLD_AUDIT_PERIOD WHERE audit_year=p.audit_year;
    IF v_count=0 THEN RAISE_APPLICATION_ERROR(-20805,'Policy audit year is not in the existing master.'); END IF;
    SELECT COUNT(*) INTO v_count FROM V_HR_EMPLOYEE_INFO_PPNO_NDC WHERE p_no=p.approved_by;
    IF v_count=0 THEN RAISE_APPLICATION_ERROR(-20805,'Policy approver is not an existing employee.'); END IF;
    -- Applied policy is immutable; an existing different value is not silently
    -- rewritten underneath already authorized requests or executive reporting.
    SELECT COUNT(*) INTO v_count FROM T_IAS_LEGACY_RECOGNITION
      WHERE audit_year=p.audit_year AND (recognition_on<>v_date OR approved_by<>p.approved_by
        OR approved_on<>v_approved OR rule_description<>p.approval_reference||': '||p.rule);
    IF v_count>0 THEN RAISE_APPLICATION_ERROR(-20805,'An existing historical policy differs from the manifest.'); END IF;
    INSERT INTO T_IAS_LEGACY_RECOGNITION(audit_year,recognition_on,approved_by,approved_on,rule_description)
      SELECT p.audit_year,v_date,p.approved_by,v_approved,p.approval_reference||': '||p.rule FROM dual
      WHERE NOT EXISTS (SELECT 1 FROM T_IAS_LEGACY_RECOGNITION WHERE audit_year=p.audit_year);
  END LOOP;
END;
/

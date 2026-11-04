-- Idempotent SOURCE ONLY. Canonical routes are application-relative; never include PathBase.
-- Repository-only registration source. No menu/group assignments, no activation.
-- Existing API anchors/activation and existing role decisions are never changed.
DECLARE
  v_exists NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_exists FROM user_tables WHERE table_name='T_IAS_LEGACY_PAGE_API';
  IF v_exists=0 THEN
    EXECUTE IMMEDIATE 'CREATE TABLE T_IAS_LEGACY_PAGE_API (
      PAGE_ID NUMBER NOT NULL CHECK (PAGE_ID IN (192,193)), API_ID NUMBER NOT NULL,
      IS_ACTIVE CHAR(1) DEFAULT ''N'' NOT NULL CHECK (IS_ACTIVE IN (''N'',''Y'')),
      CONSTRAINT PK_IAS_LEGACY_PAGE_API PRIMARY KEY(PAGE_ID,API_ID))';
  END IF;
END;
/
DECLARE
  v_id NUMBER;
  v_count NUMBER;
  PROCEDURE register_api(p_page NUMBER,p_controller VARCHAR2,p_action VARCHAR2,p_method VARCHAR2) IS
    v_path VARCHAR2(300):=CASE WHEN p_controller='LegacyParaRequests' THEN '/api/' ELSE '/' END||p_controller||'/'||p_action;
  BEGIN
    SELECT COUNT(*),MIN(api_id) INTO v_count,v_id FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM api_path))=LOWER(TRIM(BOTH '/' FROM v_path))
       AND UPPER(TRIM(http_method))=p_method;
    IF v_count>1 THEN RAISE_APPLICATION_ERROR(-20806,'Duplicate API registration: '||v_path); END IF;
    IF v_count=0 THEN
      -- API_ID uses the existing master default, as in executive_dashboard_permissions.sql.
      INSERT INTO T_AU_API_MASTER(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
        VALUES(p_action,p_page,p_controller,v_path,p_method,'N',SYSDATE);
      SELECT api_id INTO v_id FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM api_path))=LOWER(TRIM(BOTH '/' FROM v_path))
         AND UPPER(TRIM(http_method))=p_method;
    END IF;
    MERGE INTO T_IAS_LEGACY_PAGE_API t USING (SELECT p_page page_id,v_id api_id FROM dual) s
      ON (t.page_id=s.page_id AND t.api_id=s.api_id)
      WHEN NOT MATCHED THEN INSERT(page_id,api_id,is_active) VALUES(s.page_id,s.api_id,'N');
    -- Prepare only INACTIVE grants for roles of groups already assigned the page.
    -- Do not grant the page to any group, and do not update existing grants.
    MERGE INTO T_AU_ROLE_API_PERMISSION t
    USING (SELECT DISTINCT g.role_id,v_id api_id FROM t_menu_pages_groupmap m
      JOIN t_groups g ON g.group_id=m.group_id AND g.status='Y'
      WHERE m.page_id=p_page AND g.role_id IS NOT NULL) s
      ON (t.role_id=s.role_id AND t.api_id=s.api_id)
      WHEN NOT MATCHED THEN INSERT(role_id,api_id,is_active,created_by)
        VALUES(s.role_id,s.api_id,'N',NULL);
  END;
BEGIN
  -- Remove only obsolete dependency declarations. Old API registrations and
  -- role decisions are retained, but there is no old-route controller alias.
  DELETE FROM T_IAS_LEGACY_PAGE_API d WHERE d.page_id IN (192,193)
    AND EXISTS (SELECT 1 FROM T_AU_API_MASTER m WHERE m.api_id=d.api_id
      AND UPPER(TRIM(m.http_method))='POST'
      AND LOWER(TRIM(BOTH '/' FROM m.api_path)) IN
        ('legacypararequests/submit','legacypararequests/lookup','legacypararequests/list',
         'legacypararequests/detail','legacypararequests/decide'));
  register_api(192,'LegacyParaRequests','Submit','POST');
  register_api(192,'LegacyParaRequests','Lookup','POST');
  register_api(192,'LegacyParaRequests','List','POST');
  register_api(193,'LegacyParaRequests','List','POST');
  register_api(192,'LegacyParaRequests','Detail','POST');
  register_api(193,'LegacyParaRequests','Detail','POST');
  register_api(193,'LegacyParaRequests','Decide','POST');
  -- Exact observation picker dependencies from initObservationReference in
  -- obsreference.js. No SaveParaReferences or engagement/compliance write APIs.
  register_api(192,'ApiCalls','GetReferenceMasterDetail','GET');
  register_api(192,'ApiCalls','GetObservationManualMaster','GET');
  register_api(192,'ApiCalls','GetObservationManualSections','GET');
  register_api(192,'ApiCalls','GetObservationManualChapters','GET');
  register_api(192,'ApiCalls','GetObservationManualReferenceGrid','GET');
  register_api(192,'ApiCalls','GetReferenceDetailByRefId','GET');
END;
/

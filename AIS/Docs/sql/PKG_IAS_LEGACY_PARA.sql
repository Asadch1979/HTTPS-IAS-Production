CREATE OR REPLACE PACKAGE PKG_IAS_LEGACY_PARA AS
  -- Called with server-session identity only; scope/permissions are re-derived.
  PROCEDURE P_EXECUTE(p_operation IN VARCHAR2,
                      p_ppno      IN NUMBER,
                      p_context   IN NUMBER,
                      p_role      IN NUMBER,
                      p_group     IN NUMBER,
                      p_page      IN NUMBER,
                      p_input     IN CLOB,
                      io_cursor   OUT SYS_REFCURSOR);
END PKG_IAS_LEGACY_PARA;
/
CREATE OR REPLACE PACKAGE BODY PKG_IAS_LEGACY_PARA AS
  PROCEDURE P_EXECUTE(p_operation IN VARCHAR2, p_ppno IN NUMBER,
    p_context IN NUMBER, p_role IN NUMBER, p_group IN NUMBER, p_page IN NUMBER, p_input IN CLOB,
    io_cursor OUT SYS_REFCURSOR) IS
    v_entity NUMBER; v_group NUMBER; v_role NUMBER; v_enabled CHAR(1);
    v_id NUMBER; v_version NUMBER; v_count NUMBER; v_risk NUMBER;
    v_type NUMBER; v_code T_AUDITEE_ENTITIES.CODE%TYPE; v_auditor NUMBER;
    v_legacy NUMBER; v_text NUMBER; v_com NUMBER; v_resp NUMBER;
    v_ref T_AU_OLD_PARAS_FAD.REF_P%TYPE;
    v_gist T_AU_OLD_PARAS_FAD.GIST_OF_PARAS%TYPE;
    v_annexure T_AU_OLD_PARAS_FAD.ANNEXURE%TYPE;
    v_rec DATE; v_action VARCHAR2(12); v_remarks VARCHAR2(2000);
    v_row T_IAS_LEGACY_PARA_REQUEST%ROWTYPE;
    v_data T_IAS_LEGACY_PARA_REQUEST%ROWTYPE;
    v_kind VARCHAR2(30); v_parent NUMBER; v_search VARCHAR2(200);
    v_offset NUMBER; v_queue VARCHAR2(5);
    v_relation NUMBER; v_reporting_office NUMBER;
    v_json JSON_OBJECT_T;

    PROCEDURE fail(p_message VARCHAR2) IS
    BEGIN
      RAISE_APPLICATION_ERROR(-20801, p_message);
    END;
  
    PROCEDURE deny(p_message VARCHAR2) IS
    BEGIN
      RAISE_APPLICATION_ERROR(-20811, p_message);
    END;
  
    PROCEDURE entity_scope(p_entity NUMBER) IS
    BEGIN
      -- The selected Audit By office and master/mapping values must agree.
      -- The original historical-entity eligibility policy may admit
      -- an inactive entity only when its mapping corroborates ownership.
      SELECT e.type_id, e.code, e.auditby_id
        INTO v_type, v_code, v_auditor
        FROM t_auditee_entities e
       WHERE e.entity_id = p_entity
         AND e.auditable = 'Y'
         AND e.auditby_id = v_entity
         AND NOT EXISTS
             (SELECT 1 FROM t_auditee_entities_maping m
               WHERE m.entity_id = e.entity_id
                 AND m.auditedby IS NOT NULL
                 AND m.auditedby <> e.auditby_id)
         AND (e.active = 'Y' OR EXISTS
             (SELECT 1 FROM t_auditee_entities_maping m
               WHERE m.entity_id = e.entity_id AND m.auditedby = e.auditby_id))
         AND EXISTS
             (SELECT 1 FROM t_auditee_ent_types et
               WHERE et.autid = e.type_id
                 AND et.auditable = 'A'
                 AND ((v_entity IN (112242,112248)
                         AND et.audited_by_enitity = v_entity)
                   OR (v_entity NOT IN (112242,112248) AND EXISTS
                       (SELECT 1 FROM v_get_parent_office mp
                         WHERE mp.entity_id = v_entity
                           AND mp.parent_id = et.audited_by_enitity))));
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20814, 'Entity scope is not established for this Audit By office.');
    END;

    -- Mirror the report's role-scoped relationship codes, but do not use its
    -- broad, read-only parent/posting selectors for a write workflow.
    FUNCTION relationship_allowed(p_relationship NUMBER) RETURN BOOLEAN IS
      v_found NUMBER;
    BEGIN
      SELECT COUNT(*) INTO v_found FROM t_auditee_ent_relation r
       WHERE r.entity_realtion_id=p_relationship AND r.status='Y'
         AND ((v_role IN (15,16,17) AND r.id IN (5,9))
           OR (v_role=35 AND r.id IN (1,2))
           OR (v_role IN (1,3))
           OR (v_role IN (6,7) AND r.entity_realtion_id=3));
      RETURN v_found>0;
    END;

    PROCEDURE check_location(p_entity NUMBER,p_relationship NUMBER,p_office NUMBER) IS
    BEGIN
      IF p_relationship IS NULL OR p_office IS NULL
         OR p_relationship<=0 OR p_office<=0 OR NOT relationship_allowed(p_relationship) THEN
        deny('Relationship and reporting office are not authorized.');
      END IF;
      SELECT COUNT(*) INTO v_count FROM t_auditee_entities_maping m
        JOIN t_auditee_entities p ON p.entity_id=m.parent_id
        JOIN t_auditee_ent_relation r ON r.entity_realtion_id=m.relation_type_id
          AND r.parent_entity_typeid=m.p_type_id AND r.child_entity_typeid=m.c_type_id
       WHERE m.entity_id=p_entity AND m.parent_id=p_office
         AND m.relation_type_id=p_relationship AND r.status='Y';
      IF v_count=0 THEN deny('Selected posting does not belong to the reporting office and relationship.'); END IF;
    END;

    PROCEDURE duplicate_check(p_entity NUMBER,p_year NUMBER,p_nature NUMBER,p_para VARCHAR2,p_request NUMBER) IS
    BEGIN
      SELECT COUNT(*)
        INTO v_count
        FROM t_au_old_paras_fad f
       WHERE f.entity_id = p_entity
         AND TRIM(f.audit_period) = TO_CHAR(p_year, 'FM9999')
         AND f.nature_of_audit = p_nature
         AND UPPER(TRIM(f.para_no)) = UPPER(TRIM(p_para));
      IF v_count > 0 THEN
        fail('This legacy para already exists.');
      END IF;
      -- Preserve CAD/HO records; do not introduce the same historical observation again.
      SELECT COUNT(*)
        INTO v_count
        FROM T_AU_OBSERVATION_OLD_CAD_PARAS f
       WHERE f.entity_id = p_entity
         AND EXTRACT(YEAR FROM f.period) = p_year
         AND UPPER(TRIM(f.para_no)) = UPPER(TRIM(p_para));
      IF v_count > 0 THEN
        fail('This historical CAD/HO para already exists.');
      END IF;
      SELECT COUNT(*)
        INTO v_count
        FROM T_IAS_LEGACY_PARA_REQUEST r
       WHERE r.entity_id = p_entity
         AND r.audit_year = p_year
         AND r.nature_id = p_nature
         AND UPPER(TRIM(r.para_no)) = UPPER(TRIM(p_para))
         AND r.status <> 'REJECTED'
         AND r.request_id <> NVL(p_request, 0);
      IF v_count > 0 THEN
        fail('A request for this historical para already exists.');
      END IF;
    END;
  
    PROCEDURE history(p_action VARCHAR2) IS
    BEGIN
      INSERT INTO T_IAS_LEGACY_PARA_HISTORY
        (HISTORY_ID,
         REQUEST_ID,
         VERSION_NO,
         ACTION,
         STATUS,
         RISK_ID,
         LEGACY_PARA_ID,
         COMPLIANCE_ID,
         ACTOR_PPNO,
         CONTEXT_ID,
         REMARKS,
         PAYLOAD_JSON)
        SELECT SEQ_IAS_LEGACY_HISTORY.NEXTVAL,
               REQUEST_ID,
               VERSION_NO,
               p_action,
               STATUS,
               RISK_ID,
               LEGACY_PARA_ID,
               COMPLIANCE_ID,
               p_ppno,
               p_context,
               v_remarks,
               PAYLOAD_JSON
          FROM T_IAS_LEGACY_PARA_REQUEST
         WHERE REQUEST_ID = v_id;
    END;
  
    PROCEDURE check_attributes IS
    BEGIN
      IF v_data.audit_year IS NULL OR v_data.audit_year < 1900 OR
         v_data.audit_year > 2025 OR
         v_data.audit_year <> TRUNC(v_data.audit_year) THEN
        fail('Only eligible historical years through 2025 are allowed.');
      END IF;
      SELECT COUNT(*)
        INTO v_count
        FROM T_AU_OLD_AUDIT_PERIOD
       WHERE audit_year = v_data.audit_year;
      IF v_count = 0 THEN
        fail('Select an existing historical audit period.');
      END IF;
      SELECT COUNT(*)
        INTO v_count
        FROM T_AU_OLD_AUDIT_NATURE
       WHERE nid = v_data.nature_id;
      IF v_count <> 1 THEN
        fail('Invalid audit nature.');
      END IF;
      IF v_data.heading IS NULL OR LENGTH(v_data.heading) > 500 OR
         REGEXP_LIKE(v_data.heading, '[<>[:cntrl:]]') OR
         v_data.para_no IS NULL OR LENGTH(v_data.para_no) > 100 OR
         NOT REGEXP_LIKE(v_data.para_no, '^[[:alnum:] ._()/\-]+$') OR
         v_data.observation_html IS NULL OR
         DBMS_LOB.GETLENGTH(v_data.observation_html) > 2000000 OR
         v_data.amount_involved IS NULL OR v_data.amount_involved < 0 OR
         v_data.no_of_instances IS NULL OR v_data.no_of_instances < 0 OR
         v_data.no_of_instances <> TRUNC(v_data.no_of_instances) THEN
        fail('Invalid observation attributes.');
      END IF;
      SELECT COUNT(*)
        INTO v_count
        FROM t_audit_checklist c
        JOIN t_audit_checklist_sub s
          ON s.t_id = c.t_id
        JOIN t_audit_checklist_details d
          ON d.s_id = s.s_id
       WHERE c.t_id = v_data.category_id
         AND s.s_id = v_data.subchecklist_id
         AND d.id = v_data.checklist_id
         AND c.status = 'Y'
         AND s.status = 'Y'
         AND d.status = 'Y';
      IF v_count <> 1 THEN
        fail('Invalid checklist hierarchy.');
      END IF;
      SELECT a.risk
        INTO v_risk
        FROM T_AUDIT_CHECKLIST_ANNEXURE a
        JOIN t_risk r
          ON r.r_id = a.risk
       WHERE a.id = v_data.annexure_id;
      v_data.risk_id := v_risk;
      IF v_data.reference_id IS NOT NULL THEN
        SELECT COUNT(*)
          INTO v_count
          FROM vw_reference_master_detail
         WHERE ref_id = v_data.reference_id;
        IF v_count <> 1 THEN
          fail('Invalid circular or manual reference.');
        END IF;
      END IF;
      SELECT COUNT(*)
        INTO v_count
        FROM JSON_TABLE(v_data.payload_json,
                        '$.responsiblePersons[*]'
                        COLUMNS(pp VARCHAR2(12) PATH '$.ppNo',
                                lc NUMBER PATH '$.loanAmount',
                                ac NUMBER PATH '$.accountAmount')) j
       WHERE NOT REGEXP_LIKE(j.pp, '^[0-9]{1,12}$')
          OR j.pp IS NULL
          OR j.lc < 0
          OR j.ac < 0
          OR NOT EXISTS (SELECT 1
                FROM V_HR_EMPLOYEE_INFO_PPNO_NDC e
               WHERE e.p_no = TO_NUMBER(j.pp));
      IF v_count > 0 THEN
        fail('Invalid responsible employee or amount.');
      END IF;
    END;
  BEGIN
    SAVEPOINT legacy_request_start;
    IF p_operation NOT IN ('SUBMIT', 'LIST', 'DETAIL', 'LOOKUP', 'DECIDE') OR
       p_operation IS NULL OR p_page IS NULL OR p_page NOT IN (192, 193) THEN
      fail('Unsupported legacy request operation.');
    END IF;
    IF (p_operation IN ('SUBMIT', 'LOOKUP') AND p_page <> 192) OR
       (p_operation = 'DECIDE' AND p_page <> 193) THEN
      deny('Page permission is required for this action.');
    END IF;
    BEGIN
      SELECT a.entity_id, NVL(a.group_id, a.role_id), a.role_id
        INTO v_entity, v_group, v_role
      -- The selected active assignment is the membership authority. T_USER_MAPING
      -- stores the synchronized default and must not veto another active assignment.
        FROM T_USER_CONTEXT_ASSIGNMENT a
        JOIN t_user u
          ON u.userid = a.user_id
         AND u.ppno = a.ppno
        JOIN t_groups g
          ON g.group_id = NVL(a.group_id, a.role_id)
         AND g.role_id = a.role_id
         AND g.status = 'Y'
        JOIN t_auditee_entities cluster_entity
          ON cluster_entity.entity_id = a.entity_id
         AND cluster_entity.active = 'Y'
       WHERE a.user_context_id = p_context
         AND a.ppno = p_ppno
         AND a.is_active = 'Y'
         AND u.isactive = 'Y'
         AND a.role_id = p_role
         AND NVL(a.group_id, a.role_id) = p_group
         AND (a.effective_from IS NULL OR
             a.effective_from <= TRUNC(SYSDATE))
         AND (a.effective_to IS NULL OR a.effective_to >= TRUNC(SYSDATE));
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20810,
                                'The selected user context is no longer active.');
    END;
    SELECT COUNT(*)
      INTO v_count
      FROM t_menu_pages_groupmap g
      JOIN t_menu_pages p
        ON p.id = g.page_id
     WHERE g.group_id = v_group
       AND p.id = p_page
       AND p.status = 'A';
    IF v_count = 0 OR v_role IS NULL OR v_entity IS NULL THEN
      deny('Permission denied for the current page.');
    END IF;
    -- Do not rely on the generic API guard's Super User shortcut: require
    -- the selected active role, method, exact ApiCalls route and page 192/193
    -- dependency within the database as well.
    SELECT COUNT(*) INTO v_count
      FROM T_AU_ROLE_API_PERMISSION rp
      JOIN T_AU_API_MASTER am ON am.api_id = rp.api_id
      JOIN T_IAS_LEGACY_PAGE_API dep
        ON dep.api_id = am.api_id AND dep.page_id = p_page
     WHERE rp.role_id = v_role
       AND rp.is_active = 'Y'
       AND am.is_active = 'Y'
       AND dep.is_active = 'Y'
       AND UPPER(TRIM(am.http_method)) = 'POST'
       AND LOWER(TRIM(BOTH '/' FROM am.api_path)) =
           'apicalls/legacypara' || LOWER(p_operation);
    IF v_count = 0 THEN
      deny('Exact selected-role API permission is required.');
    END IF;
    -- This singleton also serializes new keys and corrected requests. No activation source is included.
    IF p_operation IN ('SUBMIT', 'DECIDE') THEN
      SELECT enabled
        INTO v_enabled
        FROM T_IAS_LEGACY_PARA_CONTROL
       WHERE control_id = 1
         FOR UPDATE;
    ELSE
      SELECT enabled
        INTO v_enabled
        FROM T_IAS_LEGACY_PARA_CONTROL
       WHERE control_id = 1;
    END IF;
    IF v_enabled<>'Y' THEN RAISE_APPLICATION_ERROR(-20812,'Legacy para addition is not activated.'); END IF;
    -- The request JSON schemas are operation-specific. Never extract
    -- requestId/version for LIST or LOOKUP: those JSON keys are absent.
    -- Preserve malformed-number errors while accepting optional missing keys.
    IF p_operation IN ('SUBMIT','DETAIL','DECIDE') THEN
      v_id := JSON_VALUE(p_input,'$.requestId'
                         RETURNING NUMBER ERROR ON ERROR NULL ON EMPTY);
      IF p_operation IN ('DETAIL','DECIDE') AND NVL(v_id,0)<=0 THEN
        fail('A positive request ID is required.');
      END IF;
    END IF;
    IF p_operation IN ('SUBMIT','DECIDE') THEN
      v_version := JSON_VALUE(p_input,'$.version'
                              RETURNING NUMBER ERROR ON ERROR NULL ON EMPTY);
      IF (p_operation='DECIDE' OR NVL(v_id,0)>0) AND NVL(v_version,0)<=0 THEN
        fail('A positive request version is required.');
      END IF;
    END IF;
    IF p_operation IN ('LOOKUP','LIST') THEN
      v_search:=TRIM(JSON_VALUE(p_input,'$.search'));
    END IF;

    IF p_operation='LOOKUP' THEN
      v_kind:=JSON_VALUE(p_input,'$.kind');
      v_parent:=JSON_VALUE(p_input,'$.parentId' RETURNING NUMBER ERROR ON ERROR);
      IF v_kind='CONTEXT' THEN
        OPEN io_cursor FOR SELECT entity_id AS id,NVL(description,name)||' ('||code||')' AS label
          FROM t_auditee_entities WHERE entity_id=v_entity;
      ELSIF v_kind='REPORTING_OFFICES' THEN
        IF NOT relationship_allowed(v_parent) THEN deny('Relationship is not available in the selected role.'); END IF;
        OPEN io_cursor FOR
          SELECT DISTINCT m.parent_id AS id, NVL(m.p_name,p.name) AS label
            FROM t_auditee_entities_maping m
            JOIN t_auditee_entities e ON e.entity_id=m.entity_id
            JOIN t_auditee_entities p ON p.entity_id=m.parent_id
            JOIN t_auditee_ent_relation r ON r.entity_realtion_id=m.relation_type_id
              AND r.parent_entity_typeid=m.p_type_id AND r.child_entity_typeid=m.c_type_id
            JOIN t_auditee_ent_types et ON et.autid=e.type_id
           WHERE m.relation_type_id=v_parent AND r.status='Y'
             AND e.auditby_id=v_entity AND e.auditable='Y' AND et.auditable='A'
             AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby IS NOT NULL AND cx.auditedby<>e.auditby_id)
             AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby=e.auditby_id))
             AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
               OR (v_entity NOT IN (112242,112248) AND EXISTS
                 (SELECT 1 FROM v_get_parent_office mp WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)))
           ORDER BY label;
      ELSIF v_kind='POSTINGS' THEN
        v_relation:=JSON_VALUE(p_input,'$.relationshipId' RETURNING NUMBER ERROR ON ERROR);
        IF NOT relationship_allowed(v_relation) THEN deny('Relationship is not available in the selected role.'); END IF;
        OPEN io_cursor FOR
          SELECT DISTINCT e.entity_id AS id, NVL(e.description,e.name)||' ('||e.code||')' AS label,
            e.type_id AS entity_type_id
            FROM t_auditee_entities_maping m
            JOIN t_auditee_entities e ON e.entity_id=m.entity_id
            JOIN t_auditee_ent_relation r ON r.entity_realtion_id=m.relation_type_id
              AND r.parent_entity_typeid=m.p_type_id AND r.child_entity_typeid=m.c_type_id
            JOIN t_auditee_ent_types et ON et.autid=e.type_id
           WHERE m.parent_id=v_parent AND m.relation_type_id=v_relation AND r.status='Y'
             AND e.auditby_id=v_entity AND e.auditable='Y' AND et.auditable='A'
             AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby IS NOT NULL AND cx.auditedby<>e.auditby_id)
             AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby=e.auditby_id))
             AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
               OR (v_entity NOT IN (112242,112248) AND EXISTS
                 (SELECT 1 FROM v_get_parent_office mp WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)))
           ORDER BY label;
      ELSIF v_kind='ENTITY_LOCATION' THEN
        -- Return all eligible mappings for older returned requests. Never guess
        -- if an entity has multiple relationships or reporting parents.
        OPEN io_cursor FOR
          SELECT DISTINCT m.relation_type_id AS relationship_id,
            m.parent_id AS reporting_office_id, e.type_id AS entity_type_id
            FROM t_auditee_entities_maping m
            JOIN t_auditee_entities p ON p.entity_id=m.parent_id
            JOIN t_auditee_entities e ON e.entity_id=m.entity_id
            JOIN t_auditee_ent_relation r ON r.entity_realtion_id=m.relation_type_id
              AND r.parent_entity_typeid=m.p_type_id AND r.child_entity_typeid=m.c_type_id
            JOIN t_auditee_ent_types et ON et.autid=e.type_id
           WHERE e.entity_id=v_parent AND e.auditby_id=v_entity
             AND e.auditable='Y' AND et.auditable='A' AND r.status='Y'
             AND ((v_role IN (15,16,17) AND r.id IN (5,9))
               OR (v_role=35 AND r.id IN (1,2))
               OR (v_role IN (1,3))
               OR (v_role IN (6,7) AND r.entity_realtion_id=3))
             AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby IS NOT NULL AND cx.auditedby<>e.auditby_id)
             AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby=e.auditby_id))
             AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
               OR (v_entity NOT IN (112242,112248) AND EXISTS
                 (SELECT 1 FROM v_get_parent_office mp WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)));
      ELSIF v_kind='ENTITY_TYPES' THEN
        OPEN io_cursor FOR SELECT DISTINCT et.autid AS id,et.entitytypedesc AS label FROM t_auditee_ent_types et
          WHERE et.auditable='A' AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
          OR (v_entity NOT IN (112242,112248) AND EXISTS (SELECT 1 FROM v_get_parent_office mp
            WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity))) ORDER BY label;
      ELSIF v_kind='ENTITIES' THEN
        OPEN io_cursor FOR SELECT e.entity_id AS id,NVL(e.description,e.name)||' ('||e.code||')' AS label
          FROM t_auditee_entities e WHERE e.auditby_id=v_entity
         AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping m WHERE m.entity_id=e.entity_id
           AND m.auditedby IS NOT NULL AND m.auditedby<>e.auditby_id)
         AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping m
           WHERE m.entity_id=e.entity_id AND m.auditedby=e.auditby_id)) AND e.auditable='Y' AND e.type_id=v_parent
          AND EXISTS (SELECT 1 FROM t_auditee_ent_types et WHERE et.autid=e.type_id AND et.auditable='A'
           AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
             OR (v_entity NOT IN (112242,112248) AND EXISTS (SELECT 1 FROM v_get_parent_office mp
               WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)))) ORDER BY label;
      ELSIF v_kind='SUBCHECKLIST' THEN
        OPEN io_cursor FOR SELECT s_id AS id,heading AS label FROM t_audit_checklist_sub
          WHERE t_id=v_parent AND status='Y' ORDER BY s_id;
      ELSIF v_kind='CHECKLIST' THEN
        OPEN io_cursor FOR SELECT id,heading AS label FROM t_audit_checklist_details
          WHERE s_id=v_parent AND status='Y' ORDER BY id;
      ELSIF v_kind='REFERENCES' THEN
        OPEN io_cursor FOR SELECT ref_id AS id,reference_source_type,title_or_heading AS label,
          section_text,chapter_no,sub_section_no FROM vw_reference_master_detail
          WHERE v_search IS NOT NULL AND INSTR(UPPER(title_or_heading),UPPER(v_search))>0
          ORDER BY title_or_heading FETCH FIRST 100 ROWS ONLY;
      ELSIF v_kind='PERSON' THEN
        OPEN io_cursor FOR SELECT DISTINCT p_no AS id,employee_name AS label FROM V_HR_EMPLOYEE_INFO_PPNO_NDC WHERE p_no=v_parent;
      ELSIF v_kind='LC' THEN
        entity_scope(v_parent);
        IF NOT REGEXP_LIKE(v_search, '^[0-9]{1,20}$') OR v_search IS NULL THEN
          fail('Enter a loan case number.');
        END IF;
        OPEN io_cursor FOR
          SELECT v.loan_case_no,
                 v.mco_ppno,
                 v.mco_name,
                 v.manager_ppno,
                 v.manager_name,
                 v.rgm_ppno,
                 v.rgm_name,
                 v.cad_reviewer,
                 v.cad_name,
                 v.cad_authorizer,
                 v.cad_authorizer_name,
                 d.outstanding_amount
            FROM v_loan_outstanding_amount d
            JOIN v_loan_sanction_process v
              ON d.loan_app_id = v.loan_app_id
             AND d.org_unitid = v.org_unitid
           WHERE v.loan_case_no = v_search
             AND v.code = v_code;
      ELSE
        fail('Unsupported lookup.');
      END IF;
      RETURN;
    END IF;
  
    IF p_operation = 'LIST' THEN
      v_offset := NVL(JSON_VALUE(p_input,
                                 '$.offset' RETURNING NUMBER ERROR ON ERROR),
                      0);
      IF v_offset < 0 OR v_offset > 100000 OR v_offset <> TRUNC(v_offset) THEN
        fail('Invalid page offset.');
      END IF;
      OPEN io_cursor FOR
        SELECT r.request_id,
               r.version_no,
               r.status,
               e.name         AS entity,
               r.audit_year,
               r.para_no,
               r.heading,
               k.description  AS risk,
               u.login_name   AS requested_by_name,
               r.requested_by,
               r.submitted_on,
               r.remarks
          FROM T_IAS_LEGACY_PARA_REQUEST r
          JOIN t_auditee_entities e
            ON e.entity_id = r.entity_id
          JOIN t_risk k
            ON k.r_id = r.risk_id
          LEFT JOIN t_user u
            ON u.ppno = r.requested_by
         WHERE r.audited_by = v_entity
           AND e.auditby_id = v_entity
           AND ((p_page = 192 AND r.requested_by = p_ppno) OR
               (p_page = 193 AND r.requested_by <> p_ppno))
           AND (v_search IS NULL OR
               INSTR(UPPER(e.name || ' ' || r.heading || ' ' || r.para_no || ' ' ||
                            r.status || ' ' || TO_CHAR(r.request_id)),
                      UPPER(v_search)) > 0)
         ORDER BY CASE
                    WHEN r.status = 'PENDING' THEN
                     0
                    ELSE
                     1
                  END,
                  r.request_id DESC OFFSET v_offset ROWS
         FETCH NEXT 50 ROWS ONLY;
      RETURN;
    END IF;
  
    IF p_operation IN ('DETAIL', 'DECIDE') OR
       (p_operation = 'SUBMIT' AND NVL(v_id, 0) > 0) THEN
      IF p_operation = 'DETAIL' THEN
        SELECT *
          INTO v_row
          FROM T_IAS_LEGACY_PARA_REQUEST
         WHERE request_id = v_id;
      ELSE
        SELECT *
          INTO v_row
          FROM T_IAS_LEGACY_PARA_REQUEST
         WHERE request_id = v_id
           FOR UPDATE;
      END IF;
      entity_scope(v_row.entity_id);
      IF v_row.audited_by <> v_entity OR
         (p_page = 192 AND v_row.requested_by <> p_ppno) OR
         (p_page = 193 AND v_row.requested_by = p_ppno) THEN
        deny('Request is not accessible in the selected context.');
      END IF;
    END IF;
  
    IF p_operation = 'DETAIL' THEN
      OPEN io_cursor FOR
        SELECT 'REQUEST' AS record_kind,
               r.request_id,
               r.version_no,
               r.status,
               r.payload_json,
               r.observation_html,
               r.requested_by AS actor,
               r.requested_on AS action_on,
               r.remarks,
               r.legacy_para_id,
               r.legacy_ref_p,
               r.compliance_id,
               e.name AS entity,
               k.description AS risk,
               a.code || ' ' || a.heading AS annexure,
               c.heading AS category,
               s.heading AS subchecklist,
               d.heading AS checklist,
               n.description AS nature,
               v.title_or_heading AS reference_label,
               (SELECT JSON_ARRAYAGG(JSON_OBJECT('status' VALUE h.action,
                                                 'version_no' VALUE
                                                 h.version_no,
                                                 'actor' VALUE h.actor_ppno,
                                                 'action_on' VALUE
                                                 h.action_on,
                                                 'remarks' VALUE h.remarks
                                                 RETURNING CLOB) ORDER BY
                                     h.history_id RETURNING CLOB)
                  FROM T_IAS_LEGACY_PARA_HISTORY h
                 WHERE h.request_id = r.request_id) AS history_json
          FROM T_IAS_LEGACY_PARA_REQUEST r
          JOIN t_auditee_entities e
            ON e.entity_id = r.entity_id
          JOIN t_risk k
            ON k.r_id = r.risk_id
          JOIN T_AUDIT_CHECKLIST_ANNEXURE a
            ON a.id = r.annexure_id
          JOIN t_audit_checklist c
            ON c.t_id = r.category_id
          JOIN t_audit_checklist_sub s
            ON s.s_id = r.subchecklist_id
          JOIN t_audit_checklist_details d
            ON d.id = r.checklist_id
          JOIN T_AU_OLD_AUDIT_NATURE n
            ON n.nid = r.nature_id
          LEFT JOIN vw_reference_master_detail v
            ON v.ref_id = r.reference_id
         WHERE r.request_id = v_id;
      RETURN;
    END IF;

    IF p_operation='SUBMIT' THEN
      IF NVL(v_id,0)>0 AND (v_row.status<>'RETURNED' OR v_row.version_no<>NVL(v_version,-1)) THEN fail('Only the current returned request can be corrected.'); END IF;
      IF v_entity IN (112242,112248) THEN RAISE_APPLICATION_ERROR(-20813,'CAD/HO posting is not supported.'); END IF;
      IF NVL(v_id,0)>0 THEN
        entity_scope(v_row.entity_id);
        -- Previous submissions may predate the three-level selector. Their
        -- original entity_scope validation remains mandatory; new requests
        -- additionally revalidate the recorded relationship and office.
        v_relation:=JSON_VALUE(v_row.payload_json,'$.relationshipId' RETURNING NUMBER ERROR ON ERROR);
        v_reporting_office:=JSON_VALUE(v_row.payload_json,'$.reportingOfficeId' RETURNING NUMBER ERROR ON ERROR);
        IF v_relation IS NOT NULL OR v_reporting_office IS NOT NULL THEN
          check_location(v_row.entity_id,v_relation,v_reporting_office);
        END IF;
        IF v_type<>v_row.entity_type_id THEN RAISE_APPLICATION_ERROR(-20814,'Entity type changed; correction is blocked.'); END IF;
      END IF;
      v_data.entity_id := JSON_VALUE(p_input,
                                     '$.entityId' RETURNING NUMBER ERROR ON
                                     ERROR);
      entity_scope(v_data.entity_id);
      v_relation:=JSON_VALUE(p_input,'$.relationshipId' RETURNING NUMBER ERROR ON ERROR);
      v_reporting_office:=JSON_VALUE(p_input,'$.reportingOfficeId' RETURNING NUMBER ERROR ON ERROR);
      check_location(v_data.entity_id,v_relation,v_reporting_office);
      v_data.entity_type_id:=v_type; v_data.audited_by:=v_auditor;
      IF NVL(JSON_VALUE(p_input,'$.entityTypeId' RETURNING NUMBER ERROR ON ERROR),-1)<>v_type THEN fail('Invalid entity type.'); END IF;
      v_data.audit_year:=JSON_VALUE(p_input,'$.auditYear' RETURNING NUMBER ERROR ON ERROR);
      v_data.nature_id:=JSON_VALUE(p_input,'$.natureId' RETURNING NUMBER ERROR ON ERROR);
      v_data.para_no:=TRIM(JSON_VALUE(p_input,'$.paraNo' RETURNING VARCHAR2(100) ERROR ON ERROR));
      v_data.heading:=TRIM(JSON_VALUE(p_input,'$.heading' RETURNING VARCHAR2(500) ERROR ON ERROR));
      v_json:=JSON_OBJECT_T.PARSE(p_input);
      v_data.observation_html:=v_json.GET_CLOB('observationHtml');
      v_data.gist:=v_json.GET_CLOB('gist');
      v_data.annexure_details:=v_json.GET_CLOB('annexureDetails');
      v_data.payload_json:=p_input;
      v_data.annexure_id:=JSON_VALUE(p_input,'$.annexureId' RETURNING NUMBER ERROR ON ERROR);
      v_data.category_id:=JSON_VALUE(p_input,'$.categoryId' RETURNING NUMBER ERROR ON ERROR);
      v_data.subchecklist_id:=JSON_VALUE(p_input,'$.subChecklistId' RETURNING NUMBER ERROR ON ERROR);
      v_data.checklist_id:=JSON_VALUE(p_input,'$.checklistId' RETURNING NUMBER ERROR ON ERROR);
      v_data.reference_id:=JSON_VALUE(p_input,'$.referenceId' RETURNING NUMBER ERROR ON ERROR);
      v_data.amount_involved:=JSON_VALUE(p_input,'$.amount' RETURNING NUMBER ERROR ON ERROR);
      v_data.no_of_instances:=JSON_VALUE(p_input,'$.instances' RETURNING NUMBER ERROR ON ERROR);
      check_attributes;
      duplicate_check(v_data.entity_id,
                      v_data.audit_year,
                      v_data.nature_id,
                      v_data.para_no,
                      v_id);
      IF NVL(v_id, 0) = 0 THEN
        v_id                      := SEQ_IAS_LEGACY_REQUEST.NEXTVAL;
        v_data.request_id         := v_id;
        v_data.version_no         := 1;
        v_data.status             := 'PENDING';
        v_data.requested_by       := p_ppno;
        v_data.request_context_id := p_context;
        v_data.requested_on       := SYSTIMESTAMP;
        v_data.submitted_on       := SYSTIMESTAMP;
        INSERT INTO T_IAS_LEGACY_PARA_REQUEST VALUES v_data;
        history('SUBMIT');
      ELSE
        UPDATE T_IAS_LEGACY_PARA_REQUEST
           SET entity_id        = v_data.entity_id,
               entity_type_id   = v_type,
               audited_by       = v_auditor,
               audit_year       = v_data.audit_year,
               nature_id        = v_data.nature_id,
               para_no          = v_data.para_no,
               heading          = v_data.heading,
               annexure_id      = v_data.annexure_id,
               category_id      = v_data.category_id,
               subchecklist_id  = v_data.subchecklist_id,
               checklist_id     = v_data.checklist_id,
               risk_id          = v_data.risk_id,
               reference_id     = v_data.reference_id,
               amount_involved  = v_data.amount_involved,
               no_of_instances  = v_data.no_of_instances,
               observation_html = v_data.observation_html,
               payload_json     = p_input,
               status           = 'PENDING',
               remarks          = NULL,
               decided_by       = NULL,
               decided_on       = NULL,
               submitted_on     = SYSTIMESTAMP,
               version_no       = version_no + 1
         WHERE request_id = v_id;
        history('RESUBMIT');
      END IF;
    ELSE
      v_action  := JSON_VALUE(p_input, '$.action');
      v_remarks := TRIM(JSON_VALUE(p_input,
                                   '$.remarks' RETURNING VARCHAR2(2000)
                                   ERROR ON ERROR));
      IF v_action NOT IN ('AUTHORIZE', 'RETURN', 'REJECT') OR
         v_action IS NULL THEN
        fail('Invalid action.');
      END IF;
      IF v_row.requested_by = p_ppno THEN
        deny('Maker cannot authorize or decide their own request.');
      END IF;
      IF v_row.status = 'AUTHORIZED' AND v_action = 'AUTHORIZE' THEN
        OPEN io_cursor FOR
          SELECT request_id,
                 version_no,
                 status,
                 legacy_para_id,
                 legacy_ref_p,
                 compliance_id
            FROM T_IAS_LEGACY_PARA_REQUEST
           WHERE request_id = v_id;
        RETURN;
      END IF;
      IF v_row.status <> 'PENDING' OR
         v_row.version_no <> NVL(v_version, -1) THEN
        fail('Request changed or is no longer pending.');
      END IF;
      IF v_action IN ('RETURN', 'REJECT') THEN
        IF v_remarks IS NULL THEN
          fail('Remarks are required.');
        END IF;
        UPDATE T_IAS_LEGACY_PARA_REQUEST
           SET status     = CASE v_action
                              WHEN 'RETURN' THEN
                               'RETURNED'
                              ELSE
                               'REJECTED'
                            END,
               remarks    = v_remarks,
               decided_by = p_ppno,
               decided_on = SYSTIMESTAMP,
               version_no = version_no + 1
         WHERE request_id = v_id;
      ELSE
        IF v_row.audited_by IN (112242,112248) THEN RAISE_APPLICATION_ERROR(-20813,'CAD/HO posting is not supported.'); END IF;
        entity_scope(v_row.entity_id);
        -- Revalidate the selected relationship/office at authorization as well
        -- as submission. Older pending requests retain their previous entity
        -- scope rule until returned and resubmitted through the new selector.
        v_relation:=JSON_VALUE(v_row.payload_json,'$.relationshipId' RETURNING NUMBER ERROR ON ERROR);
        v_reporting_office:=JSON_VALUE(v_row.payload_json,'$.reportingOfficeId' RETURNING NUMBER ERROR ON ERROR);
        IF v_relation IS NOT NULL OR v_reporting_office IS NOT NULL THEN
          check_location(v_row.entity_id,v_relation,v_reporting_office);
        END IF;
        IF v_type<>v_row.entity_type_id THEN RAISE_APPLICATION_ERROR(-20814,'Entity type changed; posting is blocked.'); END IF;
        v_data:=v_row; check_attributes;
        IF v_data.risk_id<>v_row.risk_id THEN fail('Annexure risk has changed; return this request for correction.'); END IF;
        BEGIN
          SELECT recognition_on
            INTO v_rec
            FROM T_IAS_LEGACY_RECOGNITION
           WHERE audit_year = v_row.audit_year;
        EXCEPTION
          WHEN NO_DATA_FOUND THEN
            fail('Approved historical recognition policy is not recorded for this audit year.');
        END;
        -- Shared allocator bindings are deliberately unconfigured by default.
        -- No operational table locks or per-workflow MAX(ID)+1 allocation.
        duplicate_check(v_row.entity_id,
                        v_row.audit_year,
                        v_row.nature_id,
                        v_row.para_no,
                        v_id);
        v_legacy := PKG_IAS_LEGACY_IDS.NEXT_ID('FAD');
        v_text   := PKG_IAS_LEGACY_IDS.NEXT_ID('TEXT');
        v_com    := PKG_IAS_LEGACY_IDS.NEXT_ID('COMPLIANCE');
        v_ref    := 'B' ||
                    TO_CHAR(v_legacy, 'FM9999999999999999999999999999');
        INSERT INTO T_AU_OLD_PARAS_FAD
          (id,
           ref_p,
           type_id,
           audit_period,
           para_no,
           gist_of_paras,
           annex,
           amount_involved,
           audited_by,
           entity_id,
           nature_of_audit,
           entered_by,
           entered_on,
           para_status,
           update_status,
           risk,
           process_detail,
           no_of_instances)
        VALUES
          (v_legacy,
           v_ref,
           v_type,
           TO_CHAR(v_row.audit_year, 'FM9999'),
           v_row.para_no,
           v_row.heading,
           v_row.annexure_id,
           v_row.amount_involved,
           v_auditor,
           v_row.entity_id,
           v_row.nature_id,
           v_row.requested_by,
           CAST(v_row.requested_on AS DATE),
           8,
           2,
           v_row.risk_id,
           v_row.checklist_id,
           v_row.no_of_instances);
        INSERT INTO T_AU_OLD_PARAS_FAD_TEXT
          (id, ref_p, para_text)
        VALUES
          (v_text, v_ref, v_row.observation_html);
        INSERT INTO AIS_T_AU_POST_COMPLIANCE
          (com_id,
           old_para_id,
           audit_period,
           entity_id,
           entity_code,
           audited_by,
           entity_type_id,
           com_cycle,
           com_status,
           com_stage,
           para_status,
           para_no,
           gist_of_paras,
           ind,
           para_added_on,
           risk,
           annex,
           amount,
           no_of_instances)
        VALUES
          (v_com,
           v_legacy,
           TO_CHAR(v_row.audit_year, 'FM9999'),
           v_row.entity_id,
           v_code,
           v_auditor,
           v_type,
           0,
           8,
           CASE WHEN v_type = 6 THEN 13 ELSE 21 END,
           8,
           v_row.para_no,
           v_row.heading,
           'O',
           v_rec,
           v_row.risk_id,
           v_row.annexure_id,
           v_row.amount_involved,
           v_row.no_of_instances);
        FOR person IN (SELECT j.*
                         FROM JSON_TABLE(v_row.payload_json,
                                         '$.responsiblePersons[*]'
                                         COLUMNS(pp NUMBER PATH '$.ppNo',
                                                 loan_case VARCHAR2(40) PATH
                                                 '$.loanCase',
                                                 account_number VARCHAR2(40) PATH
                                                 '$.accountNumber',
                                                 lc NUMBER PATH '$.loanAmount',
                                                 ac NUMBER PATH
                                                 '$.accountAmount')) j) LOOP
          v_resp := PKG_IAS_LEGACY_IDS.NEXT_ID('RESPONSIBILITY');
          INSERT INTO T_AU_OLD_PARAS_FAD_RESPONSIBILITY_ASSIGNED
            (id,
             obs_id,
             pp_no,
             assignedby,
             lastupdatedby,
             lastupdateddate,
             is_active,
             loan_case,
             account_number,
             lc_amount,
             ac_amount,
             ref_p)
          VALUES
            (v_resp,
             v_legacy,
             person.pp,
             v_entity,
             p_ppno,
             SYSDATE,
             'Y',
             person.loan_case,
             person.account_number,
             person.lc,
             person.ac,
             v_ref);
        END LOOP;
        INSERT INTO T_IAS_LEGACY_PARA_ATTRIBUTES
          (legacy_para_id, ref_p, request_id, reference_id, payload_json)
        VALUES
          (v_legacy, v_ref, v_id, v_row.reference_id, v_row.payload_json);
        UPDATE T_IAS_LEGACY_PARA_REQUEST
           SET status         = 'AUTHORIZED',
               decided_by     = p_ppno,
               decided_on     = SYSTIMESTAMP,
               remarks        = v_remarks,
               version_no     = version_no + 1,
               legacy_para_id = v_legacy,
               legacy_ref_p   = v_ref,
               compliance_id  = v_com,
               recognition_on = v_rec
         WHERE request_id = v_id;
      END IF;
      history(v_action);
    END IF;
    OPEN io_cursor FOR
      SELECT request_id,
             version_no,
             status,
             legacy_para_id,
             legacy_ref_p,
             compliance_id
        FROM T_IAS_LEGACY_PARA_REQUEST
       WHERE request_id = v_id;
  EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
      ROLLBACK TO legacy_request_start;
      RAISE_APPLICATION_ERROR(-20802,
                              'A request or para with this historical key already exists.');
    WHEN NO_DATA_FOUND THEN
      ROLLBACK TO legacy_request_start;
      RAISE_APPLICATION_ERROR(-20803,
                              'Request or required master data is unavailable.');
    WHEN OTHERS THEN
      ROLLBACK TO legacy_request_start;
      RAISE;
  END P_EXECUTE;
END PKG_IAS_LEGACY_PARA;

-- Dedicated reference lookups for Legacy Para data capturing.
-- No JSON payload, request list, request status update or audit-para posting.
-- Requires selected active role/group/context, page 192 and exact API grant.
CREATE OR REPLACE PACKAGE PKG_IAS_LEGACY_PARA_LOOKUP AS
  PROCEDURE P_GET_LOOKUP(
    p_ppno          IN NUMBER,
    p_context       IN NUMBER,
    p_role          IN NUMBER,
    p_group         IN NUMBER,
    p_page          IN NUMBER,
    p_kind          IN VARCHAR2,
    p_parent        IN NUMBER,
    p_relationship  IN NUMBER,
    p_search        IN VARCHAR2,
    io_cursor       OUT SYS_REFCURSOR);
END PKG_IAS_LEGACY_PARA_LOOKUP;
/
CREATE OR REPLACE PACKAGE BODY PKG_IAS_LEGACY_PARA_LOOKUP AS
  PROCEDURE P_GET_LOOKUP(
    p_ppno          IN NUMBER,
    p_context       IN NUMBER,
    p_role          IN NUMBER,
    p_group         IN NUMBER,
    p_page          IN NUMBER,
    p_kind          IN VARCHAR2,
    p_parent        IN NUMBER,
    p_relationship  IN NUMBER,
    p_search        IN VARCHAR2,
    io_cursor       OUT SYS_REFCURSOR) IS
    v_entity NUMBER;
    v_group NUMBER;
    v_role NUMBER;
    v_code T_AUDITEE_ENTITIES.CODE%TYPE;
    v_count NUMBER;
    v_enabled CHAR(1);
    FUNCTION relationship_allowed(p_relationship_id NUMBER) RETURN BOOLEAN IS
      v_found NUMBER;
    BEGIN
      SELECT COUNT(*) INTO v_found FROM t_auditee_ent_relation r
       WHERE r.entity_realtion_id=p_relationship_id AND r.status='Y'
         AND ((v_role IN (15,16,17) AND r.id IN (5,9))
           OR (v_role=35 AND r.id IN (1,2))
           OR (v_role IN (1,3))
           OR (v_role IN (6,7) AND r.entity_realtion_id=3));
      RETURN v_found>0;
    END;
    PROCEDURE fail(p_message VARCHAR2) IS
    BEGIN
      RAISE_APPLICATION_ERROR(-20801,p_message);
    END;
    PROCEDURE deny(p_message VARCHAR2) IS
    BEGIN
      RAISE_APPLICATION_ERROR(-20811,p_message);
    END;
  BEGIN
    IF p_page IS NULL OR p_page<>192 THEN
      deny('Maker page permission is required for lookup.');
    END IF;
    SELECT a.entity_id, NVL(a.group_id,a.role_id), a.role_id
      INTO v_entity, v_group, v_role
      FROM T_USER_CONTEXT_ASSIGNMENT a
      JOIN t_user u ON u.userid=a.user_id AND u.ppno=a.ppno AND u.isactive='Y'
      JOIN t_groups g ON g.group_id=NVL(a.group_id,a.role_id)
       AND g.role_id=a.role_id AND g.status='Y'
      JOIN t_auditee_entities ent ON ent.entity_id=a.entity_id AND ent.active='Y'
     WHERE a.user_context_id=p_context AND a.ppno=p_ppno
       AND a.role_id=p_role AND NVL(a.group_id,a.role_id)=p_group
       AND a.is_active='Y'
       AND (a.effective_from IS NULL OR a.effective_from<=TRUNC(SYSDATE))
       AND (a.effective_to IS NULL OR a.effective_to>=TRUNC(SYSDATE));
    SELECT COUNT(*) INTO v_count
      FROM t_menu_pages_groupmap gm JOIN t_menu_pages pg ON pg.id=gm.page_id
     WHERE gm.group_id=v_group AND gm.page_id=192 AND pg.status='A';
    IF v_count=0 THEN deny('Maker page is not assigned.'); END IF;

    SELECT COUNT(*) INTO v_count
      FROM T_AU_API_MASTER am
      JOIN T_AU_ROLE_API_PERMISSION rp ON rp.api_id=am.api_id
      JOIN T_IAS_LEGACY_PAGE_API dep ON dep.api_id=am.api_id AND dep.page_id=192
     WHERE rp.role_id=v_role AND rp.is_active='Y'
       AND am.is_active='Y' AND dep.is_active='Y'
       AND UPPER(TRIM(am.http_method))='POST'
       AND LOWER(TRIM(BOTH '/' FROM am.api_path))='apicalls/legacyparalookup';
    IF v_count=0 THEN deny('Selected role lacks maker lookup access.'); END IF;

    SELECT enabled INTO v_enabled FROM T_IAS_LEGACY_PARA_CONTROL WHERE control_id=1;
    IF v_enabled<>'Y' THEN
      RAISE_APPLICATION_ERROR(-20812,'Legacy para addition is not active.');
    END IF;
    IF p_kind IS NULL THEN fail('Select a lookup kind.'); END IF;
      IF p_kind='CONTEXT' THEN
        OPEN io_cursor FOR SELECT entity_id AS id,NVL(description,name)||' ('||code||')' AS label
          FROM t_auditee_entities WHERE entity_id=v_entity;
      ELSIF p_kind='REPORTING_OFFICES' THEN
        IF NOT relationship_allowed(p_parent) THEN deny('Relationship is not available in the selected role.'); END IF;
        OPEN io_cursor FOR
          SELECT DISTINCT m.parent_id AS id, NVL(m.p_name,p.name) AS label
            FROM t_auditee_entities_maping m
            JOIN t_auditee_entities e ON e.entity_id=m.entity_id
            JOIN t_auditee_entities p ON p.entity_id=m.parent_id
            JOIN t_auditee_ent_relation r ON r.entity_realtion_id=m.relation_type_id
              AND r.parent_entity_typeid=m.p_type_id AND r.child_entity_typeid=m.c_type_id
            JOIN t_auditee_ent_types et ON et.autid=e.type_id
           WHERE m.relation_type_id=p_parent AND r.status='Y'
             AND e.auditby_id=v_entity AND e.auditable='Y' AND et.auditable='A'
             AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby IS NOT NULL AND cx.auditedby<>e.auditby_id)
             AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby=e.auditby_id))
             AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
               OR (v_entity NOT IN (112242,112248) AND EXISTS
                 (SELECT 1 FROM v_get_parent_office mp WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)))
           ORDER BY label;
      ELSIF p_kind='POSTINGS' THEN
        IF NOT relationship_allowed(p_relationship) THEN deny('Relationship is not available in the selected role.'); END IF;
        OPEN io_cursor FOR
          SELECT DISTINCT e.entity_id AS id, NVL(e.description,e.name)||' ('||e.code||')' AS label,
            e.type_id AS entity_type_id
            FROM t_auditee_entities_maping m
            JOIN t_auditee_entities e ON e.entity_id=m.entity_id
            JOIN t_auditee_ent_relation r ON r.entity_realtion_id=m.relation_type_id
              AND r.parent_entity_typeid=m.p_type_id AND r.child_entity_typeid=m.c_type_id
            JOIN t_auditee_ent_types et ON et.autid=e.type_id
           WHERE m.parent_id=p_parent AND m.relation_type_id=p_relationship AND r.status='Y'
             AND e.auditby_id=v_entity AND e.auditable='Y' AND et.auditable='A'
             AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby IS NOT NULL AND cx.auditedby<>e.auditby_id)
             AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping cx
               WHERE cx.entity_id=e.entity_id AND cx.auditedby=e.auditby_id))
             AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
               OR (v_entity NOT IN (112242,112248) AND EXISTS
                 (SELECT 1 FROM v_get_parent_office mp WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)))
           ORDER BY label;
      ELSIF p_kind='ENTITY_LOCATION' THEN
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
           WHERE e.entity_id=p_parent AND e.auditby_id=v_entity
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
      ELSIF p_kind='ENTITY_TYPES' THEN
        OPEN io_cursor FOR SELECT DISTINCT et.autid AS id,et.entitytypedesc AS label FROM t_auditee_ent_types et
          WHERE et.auditable='A' AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
          OR (v_entity NOT IN (112242,112248) AND EXISTS (SELECT 1 FROM v_get_parent_office mp
            WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity))) ORDER BY label;
      ELSIF p_kind='ENTITIES' THEN
        OPEN io_cursor FOR SELECT e.entity_id AS id,NVL(e.description,e.name)||' ('||e.code||')' AS label
          FROM t_auditee_entities e WHERE e.auditby_id=v_entity
         AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping m WHERE m.entity_id=e.entity_id
           AND m.auditedby IS NOT NULL AND m.auditedby<>e.auditby_id)
         AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping m
           WHERE m.entity_id=e.entity_id AND m.auditedby=e.auditby_id)) AND e.auditable='Y' AND e.type_id=p_parent
          AND EXISTS (SELECT 1 FROM t_auditee_ent_types et WHERE et.autid=e.type_id AND et.auditable='A'
           AND ((v_entity IN (112242,112248) AND et.audited_by_enitity=v_entity)
             OR (v_entity NOT IN (112242,112248) AND EXISTS (SELECT 1 FROM v_get_parent_office mp
               WHERE mp.entity_id=v_entity AND mp.parent_id=et.audited_by_enitity)))) ORDER BY label;
      ELSIF p_kind='SUBCHECKLIST' THEN
        OPEN io_cursor FOR SELECT s_id AS id,heading AS label FROM t_audit_checklist_sub
          WHERE t_id=p_parent AND status='Y' ORDER BY s_id;
      ELSIF p_kind='CHECKLIST' THEN
        OPEN io_cursor FOR SELECT id,heading AS label FROM t_audit_checklist_details
          WHERE s_id=p_parent AND status='Y' ORDER BY id;
      ELSIF p_kind='REFERENCES' THEN
        OPEN io_cursor FOR SELECT ref_id AS id,reference_source_type,title_or_heading AS label,
          section_text,chapter_no,sub_section_no FROM vw_reference_master_detail
          WHERE p_search IS NOT NULL AND INSTR(UPPER(title_or_heading),UPPER(p_search))>0
          ORDER BY title_or_heading FETCH FIRST 100 ROWS ONLY;
      ELSIF p_kind='PERSON' THEN
        OPEN io_cursor FOR SELECT DISTINCT p_no AS id,employee_name AS label FROM V_HR_EMPLOYEE_INFO_PPNO_NDC WHERE p_no=p_parent;
      ELSIF p_kind='LC' THEN
        -- Loan lookup is bound to the selected audit entity and existing
        -- ownership/mapping checks, never arbitrary branch-code input.
        SELECT e.code INTO v_code
          FROM t_auditee_entities e
         WHERE e.entity_id=p_parent AND e.auditby_id=v_entity AND e.auditable='Y'
           AND NOT EXISTS (SELECT 1 FROM t_auditee_entities_maping m
              WHERE m.entity_id=e.entity_id AND m.auditedby IS NOT NULL
                AND m.auditedby<>e.auditby_id)
           AND (e.active='Y' OR EXISTS (SELECT 1 FROM t_auditee_entities_maping m
              WHERE m.entity_id=e.entity_id AND m.auditedby=e.auditby_id));
        IF NOT REGEXP_LIKE(p_search, '^[0-9]{1,20}$') OR p_search IS NULL THEN
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
           WHERE v.loan_case_no = p_search
             AND v.code = v_code;

      ELSE
        fail('Unsupported lookup.');
      END IF;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RAISE_APPLICATION_ERROR(-20810, 'Selected context or requested reference is unavailable.');
  END P_GET_LOOKUP;
END PKG_IAS_LEGACY_PARA_LOOKUP;
/

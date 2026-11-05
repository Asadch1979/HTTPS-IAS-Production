-- Review-only, dedicated maker/checker queue replacement.
-- This package is not used by the data-capture page. It exposes a single,
-- narrowly-scoped, read-only procedure for checker page 193.
-- Deploy with matching application changes; do not drop the existing
-- PKG_IAS_LEGACY_PARA until submission, lookup, detail and decision
-- operations have separately validated replacements.
CREATE OR REPLACE PACKAGE PKG_IAS_LEGACY_PARA_PENDING AS
  PROCEDURE P_GET_PENDING(
    p_ppno      IN NUMBER,
    p_context   IN NUMBER,
    p_role      IN NUMBER,
    p_group     IN NUMBER,
    p_page      IN NUMBER,
    p_search    IN VARCHAR2,
    p_offset    IN NUMBER,
    io_cursor   OUT SYS_REFCURSOR);
END PKG_IAS_LEGACY_PARA_PENDING;
/
CREATE OR REPLACE PACKAGE BODY PKG_IAS_LEGACY_PARA_PENDING AS
  PROCEDURE P_GET_PENDING(
    p_ppno      IN NUMBER,
    p_context   IN NUMBER,
    p_role      IN NUMBER,
    p_group     IN NUMBER,
    p_page      IN NUMBER,
    p_search    IN VARCHAR2,
    p_offset    IN NUMBER,
    io_cursor   OUT SYS_REFCURSOR) IS
    v_entity   NUMBER;
    v_group    NUMBER;
    v_role     NUMBER;
    v_count    NUMBER;
    v_enabled  CHAR(1);
  BEGIN
    -- The queue is available exclusively to the checker page.
    IF p_page IS NULL OR p_page <> 193 THEN
      RAISE_APPLICATION_ERROR(-20811, 'Checker page permission is required.');
    END IF;
    IF p_offset IS NULL OR p_offset < 0 OR p_offset > 100000
       OR p_offset <> TRUNC(p_offset) THEN
      RAISE_APPLICATION_ERROR(-20801, 'Invalid queue offset.');
    END IF;

    SELECT a.entity_id, NVL(a.group_id,a.role_id), a.role_id
      INTO v_entity, v_group, v_role
      FROM T_USER_CONTEXT_ASSIGNMENT a
      JOIN t_user u ON u.userid=a.user_id AND u.ppno=a.ppno AND u.isactive='Y'
      JOIN t_groups g ON g.group_id=NVL(a.group_id,a.role_id)
       AND g.role_id=a.role_id AND g.status='Y'
      JOIN t_auditee_entities e ON e.entity_id=a.entity_id AND e.active='Y'
     WHERE a.user_context_id=p_context AND a.ppno=p_ppno
       AND a.role_id=p_role AND NVL(a.group_id,a.role_id)=p_group
       AND a.is_active='Y'
       AND (a.effective_from IS NULL OR a.effective_from<=TRUNC(SYSDATE))
       AND (a.effective_to IS NULL OR a.effective_to>=TRUNC(SYSDATE));

    SELECT COUNT(*) INTO v_count
      FROM t_menu_pages_groupmap gm JOIN t_menu_pages pg ON pg.id=gm.page_id
     WHERE gm.group_id=v_group AND gm.page_id=193 AND pg.status='A';
    IF v_count=0 THEN
      RAISE_APPLICATION_ERROR(-20811, 'Checker page is not assigned.');
    END IF;

    SELECT COUNT(*) INTO v_count
      FROM T_AU_API_MASTER am
      JOIN T_AU_ROLE_API_PERMISSION rp ON rp.api_id=am.api_id
      JOIN T_IAS_LEGACY_PAGE_API dep ON dep.api_id=am.api_id AND dep.page_id=193
     WHERE rp.role_id=v_role AND rp.is_active='Y'
       AND am.is_active='Y' AND dep.is_active='Y'
       AND UPPER(TRIM(am.http_method))='POST'
       AND LOWER(TRIM(BOTH '/' FROM am.api_path))='apicalls/legacyparalist';
    IF v_count=0 THEN
      RAISE_APPLICATION_ERROR(-20811, 'Selected role lacks checker queue access.');
    END IF;

    SELECT enabled INTO v_enabled FROM T_IAS_LEGACY_PARA_CONTROL WHERE control_id=1;
    IF v_enabled<>'Y' THEN
      RAISE_APPLICATION_ERROR(-20812, 'Legacy para addition is not active.');
    END IF;

    OPEN io_cursor FOR
      SELECT r.request_id, r.version_no, r.status, e.name AS entity,
             r.audit_year, r.para_no, r.heading, k.description AS risk,
             u.login_name AS requested_by_name, r.requested_by,
             r.submitted_on, r.remarks
        FROM T_IAS_LEGACY_PARA_REQUEST r
        JOIN t_auditee_entities e ON e.entity_id=r.entity_id
        JOIN t_risk k ON k.r_id=r.risk_id
        LEFT JOIN t_user u ON u.ppno=r.requested_by
       WHERE r.audited_by=v_entity
         AND e.auditby_id=v_entity
         AND r.requested_by<>p_ppno
         AND r.status='PENDING'
         AND (p_search IS NULL OR
              INSTR(UPPER(e.name||' '||r.heading||' '||r.para_no||' '||r.status||
                          ' '||TO_CHAR(r.request_id)),UPPER(p_search))>0)
       ORDER BY r.request_id DESC
       OFFSET p_offset ROWS FETCH NEXT 50 ROWS ONLY;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RAISE_APPLICATION_ERROR(-20810, 'Selected context or configuration is unavailable.');
  END P_GET_PENDING;
END PKG_IAS_LEGACY_PARA_PENDING;
/

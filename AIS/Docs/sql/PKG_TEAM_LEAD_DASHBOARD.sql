-- IAS Team Leader engagement workbench: additive/read-only package.
-- Oracle 18c/19c compatible. Requires Working Papers V2 T_WP_HEADER/T_WP_ITEM.
-- Deploy only after table/column verification in the target DB.
-- P_PPNO MUST be resolved from the authenticated IAS session in the API;
-- never accept a caller-supplied PPNO or use this package for direct public access.
-- Team-lead authorization rechecked within each cursor using live assignments.

CREATE OR REPLACE PACKAGE PKG_TEAM_LEAD_DASHBOARD AS
  PROCEDURE P_GET_SUMMARY(
    P_PPNO IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ENGAGEMENTS(
    P_PPNO IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_WORKPAPERS(
    P_PPNO IN NUMBER, P_ENG_ID IN NUMBER DEFAULT NULL,
    IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_OBSERVATIONS(
    P_PPNO IN NUMBER, P_ENG_ID IN NUMBER DEFAULT NULL,
    P_MAX_ROWS IN NUMBER DEFAULT 100,
    IO_CURSOR OUT SYS_REFCURSOR);
END PKG_TEAM_LEAD_DASHBOARD;
/

CREATE OR REPLACE PACKAGE BODY PKG_TEAM_LEAD_DASHBOARD AS
  PROCEDURE REQUIRE_PPNO(P_PPNO IN NUMBER) AS
  BEGIN
    IF P_PPNO IS NULL OR P_PPNO <= 0 OR P_PPNO <> TRUNC(P_PPNO) THEN
      RAISE_APPLICATION_ERROR(-20101,'Valid session PPNO is required.');
    END IF;
  END REQUIRE_PPNO;

  PROCEDURE REQUIRE_ENGAGEMENT(P_PPNO IN NUMBER, P_ENG_ID IN NUMBER) AS
    V_COUNT NUMBER;
  BEGIN
    REQUIRE_PPNO(P_PPNO);
    IF P_ENG_ID IS NOT NULL THEN
      IF P_ENG_ID <= 0 OR P_ENG_ID <> TRUNC(P_ENG_ID) THEN
        RAISE_APPLICATION_ERROR(-20102,'Invalid engagement ID.');
      END IF;
      SELECT COUNT(*) INTO V_COUNT
        FROM T_AU_AUDIT_TEAM_TASKLIST t
        JOIN T_AU_TEAM_MEMBERS m
          ON m.t_id = t.team_id AND m.member_ppno = t.teammember_ppno
        JOIN T_AU_PLAN_ENG e ON e.eng_id = t.eng_plan_id
       WHERE t.eng_plan_id = P_ENG_ID
         AND t.teammember_ppno = P_PPNO
         AND m.isteamlead = 'Y'
         AND t.isactive = 'Y'
         AND e.status BETWEEN 4 AND 12;
      IF V_COUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20103,'Engagement is not assigned to this active Team Lead.');
      END IF;
    END IF;
  END REQUIRE_ENGAGEMENT;

  -- Counts and date windows are factual current-state values.
  -- Observation STATUS=3 means awaiting auditor review, not a final para.
  -- Workpaper review counts use only the latest version per type.
  PROCEDURE P_GET_SUMMARY(
    P_PPNO IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_PPNO(P_PPNO);
    OPEN IO_CURSOR FOR
      WITH lead_eng AS (
        SELECT DISTINCT e.eng_id, e.audit_enddate
          FROM T_AU_AUDIT_TEAM_TASKLIST t
          JOIN T_AU_TEAM_MEMBERS m
            ON m.t_id = t.team_id AND m.member_ppno = t.teammember_ppno
          JOIN T_AU_PLAN_ENG e ON e.eng_id = t.eng_plan_id
         WHERE t.teammember_ppno = P_PPNO
           AND m.isteamlead = 'Y'
           AND t.isactive = 'Y'
           AND e.status BETWEEN 4 AND 12
      ),
      wp_latest AS (
        SELECT h.eng_id,h.wp_id,h.status,h.reviewer_ppno,h.due_date,
               ROW_NUMBER() OVER (
                 PARTITION BY h.eng_id,h.paper_type
                 ORDER BY h.version_no DESC,h.wp_id DESC) AS rn
          FROM T_WP_HEADER h
          JOIN lead_eng e ON e.eng_id = h.eng_id
      )
      SELECT
        (SELECT COUNT(*) FROM lead_eng) AS active_engagements,
        (SELECT COUNT(*) FROM wp_latest h
          WHERE h.rn = 1 AND h.status = 'IN_REVIEW'
            AND TRIM(h.reviewer_ppno) = TO_CHAR(P_PPNO))
          AS awaiting_my_workpaper_review,
        (SELECT COUNT(*) FROM T_AU_OBSERVATION o
          JOIN lead_eng e ON e.eng_id = o.engplanid
          WHERE o.status = 3) AS observations_awaiting_auditor_review,
        (SELECT COUNT(*) FROM T_AU_OBSERVATION o
          JOIN lead_eng e ON e.eng_id = o.engplanid
          WHERE o.status = 2) AS observations_awaiting_auditee_response,
        (SELECT COUNT(*) FROM wp_latest h
          WHERE h.rn = 1 AND h.status NOT IN ('REVIEWED','APPROVED')
            AND h.due_date >= TRUNC(SYSDATE)
            AND h.due_date < TRUNC(SYSDATE) + 7)
          AS workpapers_due_next_7_days,
        (SELECT COUNT(*) FROM lead_eng e
          WHERE e.audit_enddate >= TRUNC(SYSDATE)
            AND e.audit_enddate < TRUNC(SYSDATE) + 7)
          AS engagements_ending_next_7_days,
        TRUNC(SYSDATE) AS current_state_as_of
      FROM dual;
  END P_GET_SUMMARY;

  -- Returns the underlying engagement and task-status IDs.
  -- Planning/Fieldwork/Review/Reporting stage mappings must be
  -- approved against the live status catalogue before UI grouping.
  PROCEDURE P_GET_ENGAGEMENTS(
    P_PPNO IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_PPNO(P_PPNO);
    OPEN IO_CURSOR FOR
      WITH lead_tasks AS (
        SELECT t.eng_plan_id,t.status_id,t.id,
               ROW_NUMBER() OVER (
                 PARTITION BY t.eng_plan_id ORDER BY t.id DESC) AS rn
          FROM T_AU_AUDIT_TEAM_TASKLIST t
          JOIN T_AU_TEAM_MEMBERS m
            ON m.t_id = t.team_id AND m.member_ppno = t.teammember_ppno
          JOIN T_AU_PLAN_ENG e ON e.eng_id = t.eng_plan_id
         WHERE t.teammember_ppno = P_PPNO
           AND m.isteamlead = 'Y'
           AND t.isactive = 'Y'
           AND e.status BETWEEN 4 AND 12
      )
      SELECT e.eng_id,e.plan_id,e.period_id,e.entity_id,
             ent.name AS entity_name,e.status AS engagement_status_id,
             t.status_id AS task_status_id,
             ts.description AS task_status_description,
             e.audit_startdate,e.audit_enddate,
             e.operation_startdate,e.operation_enddate,
             TRUNC(e.audit_enddate) - TRUNC(SYSDATE) AS days_to_audit_end,
             (SELECT COUNT(*) FROM T_AU_OBSERVATION o
               WHERE o.engplanid=e.eng_id AND o.status=3)
               AS awaiting_auditor_review,
             (SELECT COUNT(*) FROM T_AU_OBSERVATION o
               WHERE o.engplanid=e.eng_id AND o.status=2)
               AS awaiting_auditee_response
        FROM lead_tasks t
        JOIN T_AU_PLAN_ENG e ON e.eng_id = t.eng_plan_id
        LEFT JOIN T_AUDITEE_ENTITIES ent ON ent.entity_id=e.entity_id
        LEFT JOIN T_AU_AUDIT_TEAM_TASKLIST_STATUS ts
          ON ts.status_id=t.status_id
       WHERE t.rn=1
       ORDER BY e.audit_enddate NULLS LAST,e.eng_id;
  END P_GET_ENGAGEMENTS;

  -- V2 workpaper counts reflect actual T_WP_ITEM results. "Missing"
  -- paper types are not counted as existing or marked complete.
  PROCEDURE P_GET_WORKPAPERS(
    P_PPNO IN NUMBER, P_ENG_ID IN NUMBER DEFAULT NULL,
    IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_ENGAGEMENT(P_PPNO,P_ENG_ID);
    OPEN IO_CURSOR FOR
      WITH lead_eng AS (
        SELECT DISTINCT t.eng_plan_id
          FROM T_AU_AUDIT_TEAM_TASKLIST t
          JOIN T_AU_TEAM_MEMBERS m
            ON m.t_id=t.team_id AND m.member_ppno=t.teammember_ppno
          JOIN T_AU_PLAN_ENG e ON e.eng_id=t.eng_plan_id
         WHERE t.teammember_ppno=P_PPNO
           AND m.isteamlead='Y' AND t.isactive='Y'
           AND e.status BETWEEN 4 AND 12
           AND (P_ENG_ID IS NULL OR t.eng_plan_id=P_ENG_ID)
      ),
      latest AS (
        SELECT h.*,ROW_NUMBER() OVER (
          PARTITION BY h.eng_id,h.paper_type
          ORDER BY h.version_no DESC,h.wp_id DESC) AS rn
          FROM T_WP_HEADER h JOIN lead_eng l ON l.eng_plan_id=h.eng_id
      )
      SELECT h.wp_id,h.eng_id,h.entity_id,h.paper_type,h.reference_no,
             h.version_no,h.status,h.preparer_ppno,h.reviewer_ppno,
             h.due_date,h.reviewed_on,h.approved_on,
             (SELECT COUNT(*) FROM T_WP_ITEM i WHERE i.wp_id=h.wp_id) AS total_items,
             (SELECT COUNT(*) FROM T_WP_ITEM i
               WHERE i.wp_id=h.wp_id AND i.result='PASS') AS passed_items,
             (SELECT COUNT(*) FROM T_WP_ITEM i
               WHERE i.wp_id=h.wp_id AND i.result='EXCEPTION') AS exception_items,
             (SELECT COUNT(*) FROM T_WP_ITEM i
               WHERE i.wp_id=h.wp_id AND i.result IN ('PENDING','NOT_TESTED'))
               AS pending_items,
             TRUNC(h.due_date)-TRUNC(SYSDATE) AS days_to_due
        FROM latest h
       WHERE h.rn=1
       ORDER BY h.due_date NULLS LAST,h.eng_id,h.paper_type;
  END P_GET_WORKPAPERS;

  -- Pending statuses only. Status=2 is awaiting auditee response;
  -- status=3 is awaiting auditor review in existing AR workflow.
  PROCEDURE P_GET_OBSERVATIONS(
    P_PPNO IN NUMBER, P_ENG_ID IN NUMBER DEFAULT NULL,
    P_MAX_ROWS IN NUMBER DEFAULT 100,
    IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_ENGAGEMENT(P_PPNO,P_ENG_ID);
    IF P_MAX_ROWS IS NULL OR P_MAX_ROWS < 1 OR P_MAX_ROWS > 500
       OR P_MAX_ROWS <> TRUNC(P_MAX_ROWS) THEN
      RAISE_APPLICATION_ERROR(-20104,'Max rows must be between 1 and 500.');
    END IF;
    OPEN IO_CURSOR FOR
      SELECT * FROM (
        SELECT o.id AS observation_id,o.engplanid AS eng_id,
               o.status AS observation_status_id,
               o.severity AS risk_id,o.annex AS annex_id,
               o.entereddate
          FROM T_AU_OBSERVATION o
         WHERE o.status IN (2,3)
           AND (P_ENG_ID IS NULL OR o.engplanid=P_ENG_ID)
           AND EXISTS (
             SELECT 1 FROM T_AU_AUDIT_TEAM_TASKLIST t
             JOIN T_AU_TEAM_MEMBERS m
               ON m.t_id=t.team_id AND m.member_ppno=t.teammember_ppno
             JOIN T_AU_PLAN_ENG e ON e.eng_id=t.eng_plan_id
             WHERE t.eng_plan_id=o.engplanid
               AND t.teammember_ppno=P_PPNO
               AND m.isteamlead='Y' AND t.isactive='Y'
               AND e.status BETWEEN 4 AND 12)
         ORDER BY o.entereddate NULLS LAST,o.id
      ) WHERE ROWNUM <= P_MAX_ROWS;
  END P_GET_OBSERVATIONS;
END PKG_TEAM_LEAD_DASHBOARD;
/

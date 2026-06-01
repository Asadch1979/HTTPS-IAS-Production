CREATE OR REPLACE PACKAGE PKG_FRPT AS
  TYPE T_CURSOR IS REF CURSOR;

  FUNCTION F_IS_REPORT_FINAL(P_ENG_ID IN NUMBER) RETURN NUMBER;
    FUNCTION F_GET_ENTITY_TYPE_ID(P_ENG_ID IN NUMBER) RETURN NUMBER;
  PROCEDURE P_IS_REPORT_FINAL(P_ENG_ID IN NUMBER, O_IS_FINAL OUT NUMBER);

  Procedure P_GET_REPORT_ENTITY(P_USER_ENT_ID in number,
                                O_CURSOR      OUT T_CURSOR);

  Procedure P_GET_OBSERVATION_COUNT(P_ENG_ID IN NUMBER,
                                    O_CURSOR OUT T_CURSOR);

  Procedure P_GET_OBSERVATION_Details(P_ENG_ID IN NUMBER,
                                      O_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_REPORT_OVERVIEW(P_ENG_ID IN NUMBER,
                                  O_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_NARRATIVE_SECTIONS(P_ENG_ID IN NUMBER,
                                     O_CURSOR OUT T_CURSOR);

  PROCEDURE P_SAVE_PARA_NARRATIVE(P_ENG_ID           IN NUMBER,
                                  P_PARA_ID          IN NUMBER,
                                  P_IMPLICATIONS     IN CLOB,
                                  P_RECOMMENDATIONS  IN CLOB,
                                  P_MGMT_COMMENTS    IN CLOB,
                                  P_AUDITOR_COMMENTS IN CLOB,
                                  P_SVP_REMARKS      IN CLOB,
                                  P_ACTION           IN VARCHAR2,
                                  P_USER_ID          IN NUMBER,
                                  O_STATUS           OUT NUMBER,
                                  O_MESSAGE          OUT VARCHAR2);

  PROCEDURE P_SAVE_TEXT_BLOCK(P_ENG_ID       IN NUMBER,
                              P_SECTION_CODE IN VARCHAR2,
                              P_TEXT_BLOCK   IN CLOB);

  PROCEDURE P_GET_KPI_SNAPSHOT(P_ENG_ID IN NUMBER, O_CURSOR OUT T_CURSOR);
  PROCEDURE P_SAVE_KPI_SNAPSHOT(P_ENG_ID       IN NUMBER,
                                P_ENTITY_ID    IN NUMBER,
                                P_KPI_CODE     IN VARCHAR2,
                                P_KPI_LABEL    IN VARCHAR2,
                                P_PERIOD_END   IN DATE,
                                P_ACTUAL_VALUE IN NUMBER,
                                P_TARGET_VALUE IN NUMBER,
                                P_UNIT         IN VARCHAR2);

  PROCEDURE P_GET_NPL_SNAPSHOT(P_ENG_ID IN NUMBER, O_CURSOR OUT T_CURSOR);
  PROCEDURE P_SAVE_NPL_SNAPSHOT(P_ENG_ID             IN NUMBER,
                                P_CATEGORY           IN VARCHAR2,
                                P_PERIOD_END         IN DATE,
                                P_CASE_COUNT         IN NUMBER,
                                P_OUTSTANDING_AMOUNT IN NUMBER,
                                P_PROVISION_AMOUNT   IN NUMBER);

  PROCEDURE P_GET_STAFF_SNAPSHOT(P_ENG_ID IN NUMBER,
                                 O_CURSOR OUT SYS_REFCURSOR);
  PROCEDURE P_SAVE_STAFF_SNAPSHOT(P_ENG_ID      IN NUMBER,
                                  P_PP_NO       IN VARCHAR2,
                                  P_NAME        IN VARCHAR2,
                                  P_RANK        IN VARCHAR2,
                                  P_DESIGNATION IN VARCHAR2);

  PROCEDURE P_FINALIZE_REPORT(P_ENG_ID IN NUMBER, io_cursor OUT t_cursor);

  /* Exists in your DBConnection calls list (even if you’ll use later in UI) */
  PROCEDURE P_SAVE_PDF_STAT_REMARK(P_ENG_ID     IN NUMBER,
                                   P_RISK_LEVEL IN VARCHAR2,
                                   P_REMARKS    IN VARCHAR2);

  PROCEDURE P_GET_STAFF_DESIGNATIONS(P_ENG_ID IN NUMBER,
                                     O_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_KPI_OPTIONS(P_ENG_ID IN NUMBER,
                              O_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_NPL_CATEGORIES(P_ENG_ID IN NUMBER,
                                 O_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ALLOWED_PDF_ENGS(P_PP_NO  IN NUMBER,
                                   P_R_ID   IN NUMBER,
                                   P_ENT_ID IN NUMBER,
                                   O_CURSOR OUT SYS_REFCURSOR);

  -- In PKG_FRPT package spec
  PROCEDURE P_GET_PDF_HEADER(P_ENG_ID         IN NUMBER,
                             P_REPORT_VERSION IN NUMBER,
                             O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_PDF_REPORT_META(P_ENG_ID         IN NUMBER,
                                  P_REPORT_VERSION IN NUMBER,
                                  O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_PDF_SECTIONS(P_ENG_ID         IN NUMBER,
                               P_REPORT_VERSION IN NUMBER,
                               O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_PDF_KPI(P_ENG_ID         IN NUMBER,
                          P_REPORT_VERSION IN NUMBER,
                          O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_PDF_NPL(P_ENG_ID         IN NUMBER,
                          P_REPORT_VERSION IN NUMBER,
                          O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_PDF_STAFF(P_ENG_ID         IN NUMBER,
                            P_REPORT_VERSION IN NUMBER,
                            O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_PDF_STATISTICS(P_ENG_ID         IN NUMBER,
                                 P_REPORT_VERSION IN NUMBER,
                                 O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_PDF_STATISTICS(P_ENG_ID         IN NUMBER,
                                  P_REPORT_VERSION IN NUMBER,
                                  P_ROWS_JSON      IN CLOB,
                                  P_USER_PPNO      IN VARCHAR2);

  PROCEDURE P_GET_PDF_INCOME_LEAKAGE(P_ENG_ID         IN NUMBER,
                                     P_REPORT_VERSION IN NUMBER,
                                     O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_INCOME_LEAKAGE(P_ENG_ID         IN NUMBER,
                                 P_REPORT_VERSION IN NUMBER,
                                 O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_INCOME_LEAKAGE(P_ENG_ID         IN NUMBER,
                                  P_REPORT_VERSION IN NUMBER,
                                  P_ROWS_JSON      IN CLOB,
                                  P_USER_PPNO      IN VARCHAR2);

  PROCEDURE P_GET_OVERALL_CONCLUSION(P_ENG_ID         IN NUMBER,
                                     P_REPORT_VERSION IN NUMBER,
                                     O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_OVERALL_CONCLUSION(P_ENG_ID                  IN NUMBER,
                                      P_REPORT_VERSION          IN NUMBER,
                                      P_OVERALL_CONCLUSION_HTML IN CLOB,
                                      P_NON_ADDRESSABLE_HTML    IN CLOB,
                                      P_FRAUD_PRONE_HTML        IN CLOB,
                                      P_REGULATORY_HTML         IN CLOB,
                                      P_SAFETY_SECURITY_HTML    IN CLOB,
                                      P_USER_PPNO               IN VARCHAR2);

  procedure R_getauditeeParas(EngId    in number,
                              ENT_ID   in number,
                              P_NO     in number,
                              R_id     in number,
                              T_CURSOR OUT SYS_REFCURSOR);

  ------------------------Mangement Audit--------------

  /* =========================================================
  PKG_FRPT – SPEC (ADD THESE PROCEDURE DECLARATIONS)
  ---------------------------------------------------------
  IMPORTANT:
  - Do NOT replace your entire PKG_FRPT spec if it already
    exists with many procedures.
  - Instead, add/merge ONLY the declarations below into your
    existing PKG_FRPT specification.
  ========================================================= */

  -- Add these to PKG_FRPT package spec
  PROCEDURE P_GET_AUDIT_COVER(P_ENG_ID IN NUMBER,
                              T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MAN_OBJECTIVE_SCOPE(P_ENG_ID IN NUMBER,
                                      T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MAN_EXEC_SUMMARY(P_ENG_ID IN NUMBER,
                                   T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MAN_STAFF_SNAPSHOT(P_ENG_ID IN NUMBER,
                                     T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MAN_AUDIT_OBSERVATIONS(P_ENG_ID IN NUMBER,
                                         T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MAN_SETTLED_PARAS(P_ENG_ID IN NUMBER,
                                    T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MAN_TEAM_DETAILS(P_ENG_ID IN NUMBER,
                                   T_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ALLOWED_PDF_ENG_DETAILS(P_PP_NO  IN NUMBER,
                                          P_R_ID   IN NUMBER,
                                          P_ENT_ID IN NUMBER,
                                          o_CURSOR OUT SYS_REFCURSOR);

  ---PDF report of all outstanding paras

  PROCEDURE P_GET_AUDIT_DEPARTMENTS(O_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_OUTSTANDING_PARA_ENTITIES(P_AUDIT_DEPARTMENT_ID  IN NUMBER,
                                            P_EXECUTION_START_DATE IN DATE,
                                            P_EXECUTION_END_DATE   IN DATE,
                                            O_CURSOR               OUT T_CURSOR);

  PROCEDURE P_GET_OUTSTANDING_PARAS_FOR_PDF(P_AUDIT_DEPARTMENT_ID  IN NUMBER,
                                            P_EXECUTION_START_DATE IN DATE,
                                            P_EXECUTION_END_DATE   IN DATE,
                                            O_CURSOR               OUT T_CURSOR);

  PROCEDURE P_GET_OUTSTANDING_PARAS_SUMMARY_SETS(P_AUDIT_DEPARTMENT_ID IN NUMBER,
                                                 P_RISK                IN VARCHAR2,
                                                 O_CURSOR              OUT T_CURSOR);
                                                 
  PROCEDURE P_GET_OUTSTANDING_PARAS_SUMMARY_SET_PDF(P_AUDIT_DEPARTMENT_ID IN NUMBER,
                                                   P_ENTITY_ID           IN NUMBER,
                                                   P_RISK                IN VARCHAR2,
                                                   O_CURSOR              OUT T_CURSOR);                                                 

  PROCEDURE P_GET_OUTSTANDING_PARAS_SUMMARY_PDF(P_AUDIT_DEPARTMENT_ID IN NUMBER,
                                                P_RISK                IN VARCHAR2,
                                                O_CURSOR              OUT T_CURSOR);

  PROCEDURE P_GET_OUTSTANDING_PARA_ENTITY_BY_ENG_ID(P_ENG_ID IN NUMBER,
                                                    O_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_OUTSTANDING_PARAS_BY_ENG_ID(P_ENG_ID IN NUMBER,
                                              O_CURSOR OUT T_CURSOR);


END PKG_FRPT;

CREATE OR REPLACE PACKAGE BODY PKG_FRPT AS

  /* ============================================================
     Small helper: entity_id from engagement
  ============================================================ */
  FUNCTION F_GET_ENTITY_ID(P_ENG_ID IN NUMBER) RETURN NUMBER IS
    V_ENTITY_ID NUMBER;
  BEGIN
    SELECT EG.ENTITY_ID
      INTO V_ENTITY_ID
      FROM T_AU_PLAN_ENG EG
     WHERE EG.ENG_ID = P_ENG_ID;
  
    RETURN V_ENTITY_ID;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RETURN NULL;
  END;

  FUNCTION F_GET_ENTITY_TYPE_ID(P_ENG_ID IN NUMBER) RETURN NUMBER IS
    V_ENTITY_TYPE_ID NUMBER;
  BEGIN
    SELECT EG.ENTITY_TYPE
      INTO V_ENTITY_TYPE_ID
      FROM T_AU_PLAN_ENG EG
     WHERE EG.ENG_ID = P_ENG_ID;
  
    RETURN V_ENTITY_TYPE_ID;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RETURN NULL;
  END;

  -- put inside package body (private)
  FUNCTION F_TO_NUMBER_SAFE(P_VAL VARCHAR2) RETURN NUMBER IS
  BEGIN
    IF P_VAL IS NULL THEN
      RETURN NULL;
    END IF;
    IF REGEXP_LIKE(P_VAL, '^\d+$') THEN
      RETURN TO_NUMBER(P_VAL);
    END IF;
    RETURN NULL;
  END;

  FUNCTION F_HAS_TEXT(p_clob CLOB) RETURN NUMBER IS
    v_txt VARCHAR2(32767);
  BEGIN
    IF p_clob IS NULL THEN
      RETURN 0;
    END IF;
  
    v_txt := DBMS_LOB.SUBSTR(p_clob, 32767, 1);
    v_txt := REGEXP_REPLACE(v_txt, '[[:space:]]', '');
  
    IF v_txt IS NULL OR LENGTH(v_txt) = 0 THEN
      RETURN 0;
    END IF;
  
    RETURN 1;
  END;

  PROCEDURE P_SAVE_PARA_NARRATIVE(P_ENG_ID           IN NUMBER,
                                  P_PARA_ID          IN NUMBER,
                                  P_IMPLICATIONS     IN CLOB,
                                  P_RECOMMENDATIONS  IN CLOB,
                                  P_MGMT_COMMENTS    IN CLOB,
                                  P_AUDITOR_COMMENTS IN CLOB,
                                  P_SVP_REMARKS      IN CLOB,
                                  P_ACTION           IN VARCHAR2,
                                  P_USER_ID          IN NUMBER,
                                  O_STATUS           OUT NUMBER,
                                  O_MESSAGE          OUT VARCHAR2) IS
    v_exists    NUMBER;
    v_finalized NUMBER;
    v_upd_req   NUMBER;
    v_action    VARCHAR2(30) := UPPER(TRIM(P_ACTION));
  BEGIN
    O_STATUS  := 0;
    O_MESSAGE := 'FAILED';
  
    -- 1) Validate ENG_ID + PARA_ID is valid (IMPORTANT: fixes "select valid engagement" at DB level too)
    SELECT COUNT(*)
      INTO v_exists
      FROM t_au_observation P -- replace with your real para source
     WHERE P.ENGPLANID = P_ENG_ID
       AND P.Final_Para_No = P_PARA_ID;
  
    IF v_exists = 0 THEN
      O_STATUS  := 0;
      O_MESSAGE := 'Invalid engagement/para.';
      RETURN;
    END IF;
  
    -- 2) Ensure narrative row exists
    MERGE INTO T_FRPT_PARA_NARRATIVE N
    USING (SELECT P_ENG_ID AS ENG_ID, P_PARA_ID AS PARA_ID FROM DUAL) S
    ON (N.ENG_ID = S.ENG_ID AND N.PARA_ID = S.PARA_ID)
    WHEN NOT MATCHED THEN
      INSERT
        (ENG_ID,
         PARA_ID,
         IS_FINALIZED,
         UPDATED_REQUIRED,
         UPDATED_ON,
         UPDATED_BY)
      VALUES
        (P_ENG_ID, P_PARA_ID, 0, 0, SYSDATE, P_USER_ID);
  
    -- 3) Load current lock state
    SELECT IS_FINALIZED, UPDATED_REQUIRED
      INTO v_finalized, v_upd_req
      FROM T_FRPT_PARA_NARRATIVE
     WHERE ENG_ID = P_ENG_ID
       AND PARA_ID = P_PARA_ID
       FOR UPDATE;
  
    -- 4) Action: UPDATE_REQUIRED = unlock for editing (does not change text)
    IF v_action = 'UPDATE_REQUIRED' THEN
      UPDATE T_FRPT_PARA_NARRATIVE
         SET UPDATED_REQUIRED = 1,
             UPDATED_ON       = SYSDATE,
             UPDATED_BY       = P_USER_ID
       WHERE ENG_ID = P_ENG_ID
         AND PARA_ID = P_PARA_ID;
    
      O_STATUS  := 1;
      O_MESSAGE := 'Para unlocked for update.';
      RETURN;
    END IF;
  
    -- 5) If already finalized and not unlocked -> block changes
    IF v_finalized = 1 AND v_upd_req = 0 THEN
      O_STATUS  := 0;
      O_MESSAGE := 'Para is finalized. Click Update Required to edit.';
      RETURN;
    END IF;
  
    -- 6) For FINALIZE: enforce mandatory fields
    IF v_action = 'FINALIZE' THEN
      IF F_HAS_TEXT(P_IMPLICATIONS) = 0 THEN
        O_MESSAGE := 'IMPLICATIONS is mandatory.';
        RETURN;
      ELSIF F_HAS_TEXT(P_RECOMMENDATIONS) = 0 THEN
        O_MESSAGE := 'RECOMMENDATIONS is mandatory.';
        RETURN;
      ELSIF F_HAS_TEXT(P_MGMT_COMMENTS) = 0 THEN
        O_MESSAGE := 'MANAGEMENT / BRANCH COMMENTS is mandatory.';
        RETURN;
      ELSIF F_HAS_TEXT(P_AUDITOR_COMMENTS) = 0 THEN
        O_MESSAGE := 'AUDITOR’S FURTHER COMMENTS is mandatory.';
        RETURN;
      ELSIF F_HAS_TEXT(P_SVP_REMARKS) = 0 THEN
        O_MESSAGE := 'REMARKS OF SVP / INCHARGE is mandatory.';
        RETURN;
      END IF;
    END IF;
  
    -- 7) Save text (SAVE or FINALIZE)
    UPDATE T_FRPT_PARA_NARRATIVE
       SET IMPLICATIONS     = P_IMPLICATIONS,
           RECOMMENDATIONS  = P_RECOMMENDATIONS,
           MGMT_COMMENTS    = P_MGMT_COMMENTS,
           AUDITOR_COMMENTS = P_AUDITOR_COMMENTS,
           SVP_REMARKS      = P_SVP_REMARKS,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY       = P_USER_ID
     WHERE ENG_ID = P_ENG_ID
       AND PARA_ID = P_PARA_ID;
  
    -- 8) If FINALIZE: lock it again
    IF v_action = 'FINALIZE' THEN
      UPDATE T_FRPT_PARA_NARRATIVE
         SET IS_FINALIZED     = 1,
             UPDATED_REQUIRED = 0,
             FINALIZED_ON     = SYSDATE,
             FINALIZED_BY     = P_USER_ID,
             LOCK_VERSION     = NVL(LOCK_VERSION, 1) + 1
       WHERE ENG_ID = P_ENG_ID
         AND PARA_ID = P_PARA_ID;
    
      O_STATUS  := 1;
      O_MESSAGE := 'Para saved and finalized.';
      RETURN;
    END IF;
  
    -- Default SAVE (not finalized)
    UPDATE T_FRPT_PARA_NARRATIVE
       SET IS_FINALIZED = 0 -- optional: keep it 0 until finalized
     WHERE ENG_ID = P_ENG_ID
       AND PARA_ID = P_PARA_ID;
  
    O_STATUS  := 1;
    O_MESSAGE := 'Para saved.';
  EXCEPTION
    WHEN OTHERS THEN
      O_STATUS  := 0;
      O_MESSAGE := 'DB Error: ' || SUBSTR(SQLERRM, 1, 3500);
  END P_SAVE_PARA_NARRATIVE;

  /* ============================================================
     Helper: object exists? (table/view)
  ============================================================ */
  FUNCTION F_COL_EXISTS(P_TABLE IN VARCHAR2, P_COL IN VARCHAR2) RETURN NUMBER IS
    V_CNT NUMBER;
  BEGIN
    SELECT COUNT(*)
      INTO V_CNT
      FROM USER_TAB_COLS
     WHERE TABLE_NAME = UPPER(P_TABLE)
       AND COLUMN_NAME = UPPER(P_COL);
    RETURN CASE WHEN V_CNT > 0 THEN 1 ELSE 0 END;
  END;

  FUNCTION F_OBJ_EXISTS(P_NAME IN VARCHAR2) RETURN NUMBER IS
    V_CNT NUMBER;
  BEGIN
    SELECT COUNT(*)
      INTO V_CNT
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = UPPER(P_NAME)
       AND OBJECT_TYPE IN ('TABLE', 'VIEW');
    RETURN CASE WHEN V_CNT > 0 THEN 1 ELSE 0 END;
  END;

  /* =========================
     REPORT FINAL STATUS
  ========================= */
  FUNCTION F_IS_REPORT_FINAL(P_ENG_ID IN NUMBER) RETURN NUMBER IS
    V_CNT NUMBER := 0;
  BEGIN
    SELECT COUNT(*)
      INTO V_CNT
      FROM T_FRPT_REPORT_META M
     WHERE M.ENG_ID = P_ENG_ID
       AND NVL(M.REPORT_STATUS, 'DRAFT') = 'FINAL';
  
    RETURN CASE WHEN V_CNT > 0 THEN 1 ELSE 0 END;
  END;

  PROCEDURE P_IS_REPORT_FINAL(P_ENG_ID IN NUMBER, O_IS_FINAL OUT NUMBER) IS
  BEGIN
    O_IS_FINAL := F_IS_REPORT_FINAL(P_ENG_ID);
  END;

  Procedure P_GET_REPORT_ENTITY(P_USER_ENT_ID in number,
                                O_CURSOR      OUT T_CURSOR) IS
  begin
  
    OPEN O_CURSOR FOR
      SELECT e.eng_id AS ENG_ID,
             e.entity_id AS ENTITY_ID,
             e.entity_type as ENTITY_TYPE,
             t.audit_type as AUDIT_TYPE,
             a.description AS ENTITY_NAME,
             e.audit_startdate || ' - ' || e.audit_enddate AS AUDIT_PERIOD
        FROM t_au_plan_eng e
       inner join t_auditee_entities a
          on a.entity_id = e.entity_id
       inner join t_auditee_ent_types t
          on t.autid = a.type_id
      
       WHERE e.auditby_id = P_USER_ENT_ID
         AND e.status BETWEEN 10 AND 13;
  
  END P_GET_REPORT_ENTITY;

  /* =========================
     REPORT OVERVIEW
     C# expects columns:
     ENG_ID, ENTITY_ID, ENTITY_CODE, ENTITY_NAME,
     AUDIT_PERIOD, AUDIT_STARTDATE, AUDIT_ENDDATE,
     TEAM_NAME, VERSION_NO,
     GENERATED_ON, GENERATED_BY, FINALIZED_ON, FINALIZED_BY
  ========================= */
  PROCEDURE P_GET_REPORT_OVERVIEW(P_ENG_ID IN NUMBER,
                                  O_CURSOR OUT T_CURSOR) IS
    V_ENTITY_ID NUMBER;
  BEGIN
    V_ENTITY_ID := F_GET_ENTITY_ID(P_ENG_ID);
  
    OPEN O_CURSOR FOR
      SELECT E.ENG_ID    AS ENG_ID,
             E.ENTITY_ID AS ENTITY_ID,
             
             /* If you have a real code column, replace the '' with it */
             map.child_code AS ENTITY_CODE,
             
             /* From mapping table */
             NVL(MAP.C_NAME, '') AS ENTITY_NAME,
             nvl(MAP.p_Name, '') as REPORTING_OFFICE,
             
             /* From period table (your "Audit_year") */
             NVL(P.DESCRIPTION, '') AS AUDIT_PERIOD,
             
             /* From plan engagement */
             E.AUDIT_STARTDATE AS AUDIT_STARTDATE,
             E.AUDIT_ENDDATE   AS AUDIT_ENDDATE,
             
             e.operation_startdate as OPERATION_STARTDATE,
             e.operation_enddate   as OPERATION_ENDDATE,
             
             /* You were returning TEAM_NAME; your query has reporting office in MAP.P_NAME */
             (select tm.member_name
                from t_au_team_members tm
               where e.team_id = tm.t_id
                 and tm.isteamlead = 'Y') AS TEAM_NAME,
             
             (select count(*)
                from t_au_audit_team_tasklist t
               where e.eng_id = t.eng_plan_id) as Total_members,
             /* Version exists in meta; default to 1 */
             TO_CHAR(NVL(M.REPORT_VERSION, 1)) AS VERSION_NO,
             
             /* Meta fields (NULL if not generated yet) */
             M.GENERATED_ON,
             M.GENERATED_BY,
             M.FINALIZED_ON,
             M.FINALIZED_BY
      
        FROM T_AU_PLAN_ENG E
      
      /* keep meta optional but pinned to version 1 */
        LEFT JOIN T_FRPT_REPORT_META M
          ON M.ENG_ID = E.ENG_ID
         AND NVL(M.REPORT_VERSION, 1) = 1
      
       INNER JOIN T_AU_PERIOD P
          ON P.AUDITPERIODID = E.PERIOD_ID
      
       INNER JOIN T_AUDITEE_ENTITIES_MAPING MAP
          ON MAP.ENTITY_ID = E.ENTITY_ID
      
       WHERE E.ENG_ID = P_ENG_ID;
    --P_ENG_ID;
  
    /* If no row exists in META, return one row anyway */
    IF SQL%ROWCOUNT = 0 THEN
      OPEN O_CURSOR FOR
        SELECT P_ENG_ID AS ENG_ID,
               V_ENTITY_ID AS ENTITY_ID,
               '' AS ENTITY_CODE,
               '' AS ENTITY_NAME,
               '' AS AUDIT_PERIOD,
               CAST(NULL AS DATE) AS AUDIT_STARTDATE,
               CAST(NULL AS DATE) AS AUDIT_ENDDATE,
               '' AS TEAM_NAME,
               '1' AS VERSION_NO,
               CAST(NULL AS DATE) AS GENERATED_ON,
               CAST(NULL AS NUMBER) AS GENERATED_BY,
               CAST(NULL AS DATE) AS FINALIZED_ON,
               CAST(NULL AS NUMBER) AS FINALIZED_BY
          FROM DUAL;
    END IF;
  
  END;

  /* =========================
     NARRATIVE SECTIONS
     Master: T_FRPT_SECTION_MASTER
     Text:   T_FRPT_TEXT_BLOCKS.SECTION_TEXT
     C# expects: SECTION_CODE, SECTION_TITLE, DISPLAY_ORDER, IS_MANDATORY, TEXT_BLOCK
  ========================= */

  Procedure P_GET_OBSERVATION_COUNT(P_ENG_ID IN NUMBER,
                                    O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      select count(o.id) as no_of_paras
        from t_au_observation o
       where o.engplanid = P_ENG_ID
         and o.status = 8;
  
  end;

  PROCEDURE P_GET_OBSERVATION_Details(P_ENG_ID IN NUMBER,
                                      O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT o.final_para_no AS PARA_ID,
             TO_CHAR(o.final_para_no, 'FM00') AS PARA_NO,
             
             r.description AS risk,
             a.code AS Annexcode,
             a.heading AS Annexure,
             o.no_of_instances AS instances,
             o.amount_involved AS amount,
             'Outstanding' AS Rectification_Status,
             t.headings AS Gist_of_Para,
             t.text AS PARA_Detail,
             
             NVL(n.implications, EMPTY_CLOB()) AS IMPLICATIONS,
             NVL(f.recommendation, '-') AS RECOMMENDATIONS,
             DBMS_LOB.SUBSTR(ae.reply, 4000, 1) AS MANAGEMENT_COMMENTS,
             NVL(ar.recommendation, EMPTY_CLOB()) AS AUDITOR_COMMENTS,
             NVL(az.audit_reply, EMPTY_CLOB()) AS SVP_REMARKS,
             
             NVL(n.is_finalized, 0) AS IS_FINALIZED,
             NVL(n.updated_required, 0) AS UPDATED_REQUIRED,
             n.finalized_on,
             n.finalized_by,
             n.updated_on,
             n.updated_by
      
        FROM t_au_observation o
       INNER JOIN t_au_observation_text t
          ON t.observatsion_id = o.id
       INNER JOIN t_audit_checklist_annexure a
          ON a.id = o.annex
       INNER JOIN t_risk r
          ON r.rating = o.severity
       INNER JOIN t_au_observation_final_reccomendation f
          ON f.obs_id = o.id
       inner join t_au_observations_auditor_recommendation ar
          on ar.au_obs_id = o.id
       inner join t_au_observations_auditee_response ae
          on ae.au_obs_id = o.id
       inner join t_Au_Observations_Auditor_Reply az
          on az.au_obs_id = o.id
        LEFT JOIN t_frpt_para_narrative n
          ON n.eng_id = o.engplanid
         AND n.para_id = o.final_para_no
      
       WHERE o.engplanid = P_ENG_ID
         AND o.status = 8
       ORDER BY o.final_para_no;
  END;

  PROCEDURE P_GET_NARRATIVE_SECTIONS(P_ENG_ID IN NUMBER,
                                     O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT S.SECTION_CODE,
             S.SECTION_TITLE,
             S.DISPLAY_ORDER,
             CASE
               WHEN NVL(S.IS_MANDATORY, 'N') = 'Y' THEN
                'Y'
               ELSE
                'N'
             END AS IS_MANDATORY,
             NVL(T.SECTION_TEXT, '') AS TEXT_BLOCK
        FROM T_FRPT_SECTION_MASTER S
        LEFT JOIN (SELECT ENG_ID, SECTION_CODE, SECTION_TEXT
                     FROM (SELECT ENG_ID,
                                  SECTION_CODE,
                                  SECTION_TEXT,
                                  ROW_NUMBER() OVER(PARTITION BY ENG_ID, UPPER(SECTION_CODE) ORDER BY FRPT_TEXT_ID DESC) AS RN
                             FROM T_FRPT_TEXT_BLOCKS
                            WHERE ENG_ID = P_ENG_ID)
                    WHERE RN = 1) T
          ON T.ENG_ID = P_ENG_ID
         AND UPPER(T.SECTION_CODE) = UPPER(S.SECTION_CODE)
       WHERE NVL(S.IS_ACTIVE, 'Y') = 'Y'
       ORDER BY S.DISPLAY_ORDER, S.SECTION_CODE;
  END;

  PROCEDURE P_SAVE_TEXT_BLOCK(P_ENG_ID       IN NUMBER,
                              P_SECTION_CODE IN VARCHAR2,
                              P_TEXT_BLOCK   IN CLOB) IS
    V_ID NUMBER;
  BEGIN
    /* Update one existing row if present */
    SELECT MIN(FRPT_TEXT_ID)
      INTO V_ID
      FROM T_FRPT_TEXT_BLOCKS
     WHERE ENG_ID = P_ENG_ID
       AND UPPER(SECTION_CODE) = UPPER(TRIM(P_SECTION_CODE));
  
    IF V_ID IS NOT NULL THEN
      UPDATE T_FRPT_TEXT_BLOCKS
         SET SECTION_TEXT = P_TEXT_BLOCK
       WHERE FRPT_TEXT_ID = V_ID;
    ELSE
      INSERT INTO T_FRPT_TEXT_BLOCKS
        (FRPT_TEXT_ID, ENG_ID, SECTION_CODE, SECTION_TEXT)
      VALUES
        (SEQ_FRPT_TEXT_BLOCKS.NEXTVAL,
         P_ENG_ID,
         UPPER(TRIM(P_SECTION_CODE)),
         P_TEXT_BLOCK);
    END IF;
  
    COMMIT;
  END;

  /* =========================
     KPI SNAPSHOT
     Table: T_FRPT_KPI_SNAPSHOT
     C# expects: KPI_CODE, KPI_LABEL, PERIOD_END, ACTUAL_VALUE, TARGET_VALUE, UNIT
  ========================= */
  PROCEDURE P_GET_KPI_SNAPSHOT(P_ENG_ID IN NUMBER, O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT KPI_CODE,
             KPI_LABEL,
             PERIOD_END,
             ACTUAL_VALUE,
             TARGET_VALUE,
             UNIT
        FROM T_FRPT_KPI_SNAPSHOT
       WHERE ENG_ID = P_ENG_ID
       ORDER BY KPI_CODE, PERIOD_END;
  END;

  PROCEDURE P_SAVE_KPI_SNAPSHOT(P_ENG_ID       IN NUMBER,
                                P_ENTITY_ID    IN NUMBER,
                                P_KPI_CODE     IN VARCHAR2,
                                P_KPI_LABEL    IN VARCHAR2,
                                P_PERIOD_END   IN DATE,
                                P_ACTUAL_VALUE IN NUMBER,
                                P_TARGET_VALUE IN NUMBER,
                                P_UNIT         IN VARCHAR2) IS
    V_EXIST_ID NUMBER;
  BEGIN
    IF P_ENTITY_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20010,
                              'ENTITY_ID is required for KPI Snapshot save.');
    END IF;
  
    /* Find existing row by logical key */
    SELECT MIN(FRPT_KPI_ID)
      INTO V_EXIST_ID
      FROM T_FRPT_KPI_SNAPSHOT
     WHERE ENG_ID = P_ENG_ID
       AND NVL(TRIM(KPI_CODE), '') = NVL(TRIM(P_KPI_CODE), '')
       AND ((PERIOD_END = P_PERIOD_END) OR
           (PERIOD_END IS NULL AND P_PERIOD_END IS NULL));
  
    IF V_EXIST_ID IS NOT NULL THEN
      UPDATE T_FRPT_KPI_SNAPSHOT
         SET ENTITY_ID    = P_ENTITY_ID,
             KPI_LABEL    = P_KPI_LABEL,
             PERIOD_END   = P_PERIOD_END,
             ACTUAL_VALUE = P_ACTUAL_VALUE,
             TARGET_VALUE = P_TARGET_VALUE,
             UNIT         = P_UNIT
       WHERE FRPT_KPI_ID = V_EXIST_ID;
    ELSE
      INSERT INTO T_FRPT_KPI_SNAPSHOT
        (FRPT_KPI_ID,
         ENG_ID,
         ENTITY_ID,
         KPI_CODE,
         KPI_LABEL,
         PERIOD_END,
         ACTUAL_VALUE,
         TARGET_VALUE,
         UNIT)
      VALUES
        (SEQ_FRPT_KPI_SNAPSHOT.NEXTVAL,
         P_ENG_ID,
         P_ENTITY_ID,
         P_KPI_CODE,
         P_KPI_LABEL,
         P_PERIOD_END,
         P_ACTUAL_VALUE,
         P_TARGET_VALUE,
         P_UNIT);
    END IF;
  
    COMMIT;
  END;

  /* =========================
     PDF STATISTICS (COUNTS)
     No FRPT table exists in your provided structures, so:
     - If a table/view exists (T_FRPT_PDF_STATISTICS), return it
     - Else return 4 rows with zeros so UI remains stable
  ========================= */
  PROCEDURE P_GET_PDF_STATISTICS(P_ENG_ID         IN NUMBER,
                                 P_REPORT_VERSION IN NUMBER,
                                 O_CURSOR         OUT T_CURSOR) IS
    V_SQL CLOB;
  BEGIN
    IF F_OBJ_EXISTS('T_FRPT_PDF_STATISTICS') = 1 THEN
      V_SQL := 'SELECT RISK_LEVEL, REPORTED_COUNT, RECTIFIED_COUNT, OUTSTANDING_COUNT
         FROM T_FRPT_PDF_STATISTICS
        WHERE ENG_ID = :1 ';
    
      IF F_COL_EXISTS('T_FRPT_PDF_STATISTICS', 'REPORT_VERSION') = 1 THEN
        V_SQL := V_SQL || ' AND (:2 IS NULL OR REPORT_VERSION = :2) ';
        V_SQL := V_SQL || ' ORDER BY CASE RISK_LEVEL
                     WHEN ''FRAUD'' THEN 1
                     WHEN ''HIGH''  THEN 2
                     WHEN ''MEDIUM'' THEN 3
                     WHEN ''LOW''   THEN 4
                     ELSE 9
                   END';
        OPEN O_CURSOR FOR V_SQL
          USING P_ENG_ID, P_REPORT_VERSION;ELSE
        V_SQL := V_SQL ||
                 ' ORDER BY CASE RISK_LEVEL
                     WHEN ''FRAUD'' THEN 1
                     WHEN ''HIGH''  THEN 2
                     WHEN ''MEDIUM'' THEN 3
                     WHEN ''LOW''   THEN 4
                     ELSE 9
                   END';
        OPEN O_CURSOR FOR V_SQL
          USING P_ENG_ID;
      END IF;
    
    ELSE
      OPEN O_CURSOR FOR
        SELECT 'FRAUD' AS RISK_LEVEL,
               0 AS REPORTED_COUNT,
               0 AS RECTIFIED_COUNT,
               0 AS OUTSTANDING_COUNT
          FROM DUAL
        UNION ALL
        SELECT 'HIGH', 0, 0, 0
          FROM DUAL
        UNION ALL
        SELECT 'MEDIUM', 0, 0, 0
          FROM DUAL
        UNION ALL
        SELECT 'LOW', 0, 0, 0
          FROM DUAL;
    END IF;
  END;

  /* =========================
     NPL SNAPSHOT
     Table columns:
     NPL_BUCKET, CASES_COUNT, OUTSTANDING_AMT, PROVISION_AMT
     C# expects: CATEGORY, CASE_COUNT, OUTSTANDING_AMOUNT, PROVISION_AMOUNT, PERIOD_END
     NOTE: C# does NOT pass ENTITY_ID => we derive it.
  ========================= */
  PROCEDURE P_GET_NPL_SNAPSHOT(P_ENG_ID IN NUMBER, O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT NPL_BUCKET      AS CATEGORY,
             PERIOD_END,
             CASES_COUNT     AS CASE_COUNT,
             OUTSTANDING_AMT AS OUTSTANDING_AMOUNT,
             PROVISION_AMT   AS PROVISION_AMOUNT
        FROM T_FRPT_NPL_SNAPSHOT
       WHERE ENG_ID = P_ENG_ID
       ORDER BY NPL_BUCKET, PERIOD_END;
  END;

  PROCEDURE P_SAVE_NPL_SNAPSHOT(P_ENG_ID             IN NUMBER,
                                P_CATEGORY           IN VARCHAR2,
                                P_PERIOD_END         IN DATE,
                                P_CASE_COUNT         IN NUMBER,
                                P_OUTSTANDING_AMOUNT IN NUMBER,
                                P_PROVISION_AMOUNT   IN NUMBER) IS
    V_ENTITY_ID NUMBER;
    V_EXIST_ID  NUMBER;
  BEGIN
    V_ENTITY_ID := F_GET_ENTITY_ID(P_ENG_ID);
    IF V_ENTITY_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20011,
                              'ENTITY_ID could not be derived for NPL Snapshot.');
    END IF;
  
    SELECT MIN(FRPT_NPL_ID)
      INTO V_EXIST_ID
      FROM T_FRPT_NPL_SNAPSHOT
     WHERE ENG_ID = P_ENG_ID
       AND NVL(TRIM(NPL_BUCKET), '') = NVL(TRIM(P_CATEGORY), '')
       AND ((PERIOD_END = P_PERIOD_END) OR
           (PERIOD_END IS NULL AND P_PERIOD_END IS NULL));
  
    IF V_EXIST_ID IS NOT NULL THEN
      UPDATE T_FRPT_NPL_SNAPSHOT
         SET ENTITY_ID       = V_ENTITY_ID,
             NPL_BUCKET      = P_CATEGORY,
             PERIOD_END      = P_PERIOD_END,
             CASES_COUNT     = P_CASE_COUNT,
             OUTSTANDING_AMT = P_OUTSTANDING_AMOUNT,
             PROVISION_AMT   = P_PROVISION_AMOUNT
       WHERE FRPT_NPL_ID = V_EXIST_ID;
    ELSE
      INSERT INTO T_FRPT_NPL_SNAPSHOT
        (FRPT_NPL_ID,
         ENG_ID,
         ENTITY_ID,
         NPL_BUCKET,
         PERIOD_END,
         CASES_COUNT,
         OUTSTANDING_AMT,
         PROVISION_AMT)
      VALUES
        (SEQ_FRPT_NPL_SNAPSHOT.NEXTVAL,
         P_ENG_ID,
         V_ENTITY_ID,
         P_CATEGORY,
         P_PERIOD_END,
         P_CASE_COUNT,
         P_OUTSTANDING_AMOUNT,
         P_PROVISION_AMOUNT);
    END IF;
  
    COMMIT;
  END;

  /* =========================
     STAFF SNAPSHOT
     NOTE: C# does NOT pass ENTITY_ID => we derive it.
  ========================= */
  PROCEDURE P_GET_STAFF_SNAPSHOT(P_ENG_ID IN NUMBER,
                                 O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT FRPT_STAFF_ID,
             PP_NO,
             STAFF_NAME    AS NAME,
             STAFF_RANK    AS RANK,
             DESIGNATION
        FROM T_FRPT_STAFF_SNAPSHOT
       WHERE ENG_ID = P_ENG_ID
         AND NVL(IS_ACTIVE, 1) = 1
       ORDER BY FRPT_STAFF_ID;
  END;

  PROCEDURE P_SAVE_STAFF_SNAPSHOT(P_ENG_ID      IN NUMBER,
                                  P_PP_NO       IN VARCHAR2,
                                  P_NAME        IN VARCHAR2,
                                  P_RANK        IN VARCHAR2,
                                  P_DESIGNATION IN VARCHAR2) IS
    V_ENTITY_ID NUMBER;
    V_EXIST_ID  NUMBER;
    V_PP_NO     VARCHAR2(20);
  BEGIN
    V_PP_NO := TRIM(P_PP_NO);
  
    IF V_PP_NO IS NULL THEN
      RAISE_APPLICATION_ERROR(-20012,
                              'PP No is required for Staff Snapshot.');
    END IF;
  
    V_ENTITY_ID := F_GET_ENTITY_ID(P_ENG_ID);
    IF V_ENTITY_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20012,
                              'ENTITY_ID could not be derived for Staff Snapshot.');
    END IF;
  
    -- Find existing row for this ENG + PP_NO (active)
    SELECT MIN(FRPT_STAFF_ID)
      INTO V_EXIST_ID
      FROM T_FRPT_STAFF_SNAPSHOT
     WHERE ENG_ID = P_ENG_ID
       AND PP_NO = V_PP_NO
       AND NVL(IS_ACTIVE, 1) = 1;
  
    IF V_EXIST_ID IS NOT NULL THEN
      UPDATE T_FRPT_STAFF_SNAPSHOT
         SET ENTITY_ID   = V_ENTITY_ID,
             STAFF_NAME  = P_NAME,
             STAFF_RANK  = P_RANK,
             DESIGNATION = P_DESIGNATION,
             UPDATED_ON  = SYSDATE,
             UPDATED_BY  = V_PP_NO
       WHERE FRPT_STAFF_ID = V_EXIST_ID;
    ELSE
      INSERT INTO T_FRPT_STAFF_SNAPSHOT
        (FRPT_STAFF_ID,
         ENG_ID,
         ENTITY_ID,
         PP_NO,
         STAFF_NAME,
         STAFF_RANK,
         DESIGNATION,
         IS_ACTIVE,
         CREATED_ON,
         CREATED_BY)
      VALUES
        (SEQ_FRPT_STAFF_SNAPSHOT.NEXTVAL,
         P_ENG_ID,
         V_ENTITY_ID,
         V_PP_NO,
         P_NAME,
         P_RANK,
         P_DESIGNATION,
         1,
         SYSDATE,
         V_PP_NO);
    END IF;
  
    COMMIT;
  END;

  /* =========================
     FINALIZE REPORT
     Uses T_FRPT_REPORT_META with unique (ENG_ID, REPORT_VERSION)
  ========================= */
  PROCEDURE P_FINALIZE_REPORT(P_ENG_ID IN NUMBER,io_cursor OUT t_cursor) IS
    V_ENTITY_ID   NUMBER;
    V_EXIST_ID    NUMBER;
    V_ENTITY_TYPE NUMBER;
    V_KIP_EXIST   Varchar2(2);
  BEGIN
    V_ENTITY_ID   := F_GET_ENTITY_ID(P_ENG_ID);
    V_ENTITY_TYPE := F_GET_ENTITY_TYPE_ID(P_ENG_ID);
    IF V_ENTITY_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20013,
                              'ENTITY_ID could not be derived for Report Meta.');
    END IF;
    
    if(V_ENTITY_TYPE = 6) then
    SELECT CASE
             WHEN EXISTS (SELECT 1
                     FROM T_FRPT_KPI_SNAPSHOT f
                    WHERE f.eng_id = P_ENG_ID) AND EXISTS
              (SELECT 1
                     FROM T_FRPT_NPL_SNAPSHOT n
                    WHERE n.eng_id = P_ENG_ID) AND EXISTS
              (SELECT 1
                     FROM T_FRPT_STAFF_SNAPSHOT s
                    WHERE s.eng_id = P_ENG_ID) THEN
              'Y'
             ELSE
              'N'
           END
      INTO V_KIP_EXIST
      FROM dual; 
      else
        V_KIP_EXIST := 'Y';
      end if;
  If (V_KIP_EXIST = 'Y') then
    SELECT MIN(FRPT_ID)
      INTO V_EXIST_ID
      FROM T_FRPT_REPORT_META
     WHERE ENG_ID = P_ENG_ID
       AND NVL(REPORT_VERSION, 1) = 1;
  
    IF V_EXIST_ID IS NOT NULL THEN
      UPDATE T_FRPT_REPORT_META
         SET REPORT_STATUS = 'FINAL', FINALIZED_ON = SYSDATE
       WHERE FRPT_ID = V_EXIST_ID;
    ELSE
      INSERT INTO T_FRPT_REPORT_META
        (FRPT_ID,
         ENG_ID,
         ENTITY_ID,
         REPORT_VERSION,
         REPORT_STATUS,
         FINALIZED_ON)
      VALUES
        (SEQ_FRPT_REPORT_META.NEXTVAL,
         P_ENG_ID,
         V_ENTITY_ID,
         1,
         'FINAL',
         SYSDATE);
    END IF;  
    COMMIT;
          Open io_cursor for 
     Select 'Report Finalized'  as remarks from Dual;   
     
     else 
       Open io_cursor for 
     Select 'First Complete KPI/NPL/STAFF, then it can be Finalized '  as remarks from Dual;   
        
    end if;
  END;

  /* =========================
     SAVE PDF STAT REMARK
     Table not present in your FRPT structures, so compile-safe:
     - If table exists, do a merge via dynamic SQL
     - Else no-op
  ========================= */
  PROCEDURE P_SAVE_PDF_STAT_REMARK(P_ENG_ID     IN NUMBER,
                                   P_RISK_LEVEL IN VARCHAR2,
                                   P_REMARKS    IN VARCHAR2) IS
    V_SQL CLOB;
  BEGIN
    IF F_OBJ_EXISTS('T_FRPT_PDF_STAT_REMARKS') = 1 THEN
      V_SQL := 'MERGE INTO T_FRPT_PDF_STAT_REMARKS R
         USING (SELECT :1 ENG_ID, :2 RISK_LEVEL FROM DUAL) S
            ON (R.ENG_ID = S.ENG_ID AND R.RISK_LEVEL = S.RISK_LEVEL)
         WHEN MATCHED THEN UPDATE SET R.REMARKS = :3
         WHEN NOT MATCHED THEN INSERT (ENG_ID, RISK_LEVEL, REMARKS) VALUES (:1, :2, :3)';
      EXECUTE IMMEDIATE V_SQL
        USING P_ENG_ID, UPPER(TRIM(P_RISK_LEVEL)), P_REMARKS;
      COMMIT;
    ELSE
      NULL;
    END IF;
  END;

  PROCEDURE P_GET_STAFF_DESIGNATIONS(P_ENG_ID IN NUMBER,
                                     O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT CODE, LABEL
        FROM V_FRPT_STAFF_DESIGNATIONS
       ORDER BY SORT_ORDER;
  END;
  PROCEDURE P_GET_KPI_OPTIONS(P_ENG_ID IN NUMBER,
                              O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT KPI_CODE, KPI_LABEL, UNIT
        FROM V_FRPT_KPI_OPTIONS
       ORDER BY SORT_ORDER;
  END;
  PROCEDURE P_GET_NPL_CATEGORIES(P_ENG_ID IN NUMBER,
                                 O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT CODE, LABEL FROM V_FRPT_NPL_CATEGORIES ORDER BY SORT_ORDER;
  END;

  PROCEDURE P_GET_ALLOWED_PDF_ENGS(P_PP_NO  IN NUMBER,
                                   P_R_ID   IN NUMBER,
                                   P_ENT_ID IN NUMBER,
                                   O_CURSOR OUT SYS_REFCURSOR) IS
    V_USER_ID NUMBER;
  BEGIN
  
    -- 1) Admin / Super roles: allow all FINAL reports (adjust role IDs to your actual admin roles)
    IF P_R_ID IN (1, 2) THEN
      OPEN O_CURSOR FOR
        SELECT M.ENG_ID
          FROM T_FRPT_REPORT_META M
         WHERE M.REPORT_STATUS = 'FINAL'
         ORDER BY M.ENG_ID;
      -- 1) Department Head / Incharge: allow FINAL reports of their area (adjust role IDs to your actual admin roles)
    ELSIF P_R_ID in (15, 16) then
      OPEN O_CURSOR FOR
        SELECT DISTINCT M.ENG_ID
          FROM T_FRPT_REPORT_META M
          JOIN t_Au_Plan_Eng e
            ON e.eng_id = M.ENG_ID
        
         WHERE M.REPORT_STATUS = 'FINAL'
           and e.auditby_id = P_ENT_ID
         ORDER BY M.ENG_ID;
    END IF;
  
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      -- Return empty cursor instead of raising error (UI will show no engagements)
      OPEN O_CURSOR FOR
        SELECT CAST(NULL AS NUMBER) AS ENG_ID FROM DUAL WHERE 1 = 0;
  END P_GET_ALLOWED_PDF_ENGS;

  -- Put inside PKG_FRPT body (private helper)
  FUNCTION F_RESOLVE_VERSION(P_ENG_ID NUMBER, P_REPORT_VERSION NUMBER)
    RETURN NUMBER IS
    V_VER NUMBER;
  BEGIN
    IF P_REPORT_VERSION IS NOT NULL THEN
      RETURN P_REPORT_VERSION;
    END IF;
  
    -- Adjust column name if yours is VERSION_NO etc.
    SELECT MAX(REPORT_VERSION)
      INTO V_VER
      FROM T_FRPT_REPORT_META
     WHERE ENG_ID = P_ENG_ID;
  
    RETURN V_VER;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RETURN NULL;
  END;

  PROCEDURE P_GET_PDF_HEADER(P_ENG_ID         IN NUMBER,
                             P_REPORT_VERSION IN NUMBER,
                             O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT 'ZARAI TARAQIATI BANK LIMITED' AS BANK_NAME,
             'INTERNAL AUDIT DIVISION' AS INTERNAL_AUDIT_DIVISION,
             
             -- Adjust these joins/columns to your engagement/entity source
             Em.p_Name as Reporting_Office,
             E.NAME    AS BRANCH_NAME,
             E.CODE    AS BRANCH_CODE,
             
             p.description AS AUDIT_PERIOD,
             pe.operation_startdate || '-' || pe.operation_enddate as Operation_Period,
             pe.audit_startdate || '-' || pe.audit_enddate AS AUDIT_Executed,
             
             pe.audit_startdate as AUDIT_START_DATE,
             pe.audit_enddate as AUDIT_END_DATE,
             'High' as Risk,
             
             M.REPORT_STATUS AS REPORT_STATUS,
             TO_CHAR(M.REPORT_VERSION) AS VERSION_NO,
             E.NAME AS ENTITY_NAME
      
        FROM T_FRPT_REPORT_META M
        JOIN T_AU_PLAN_ENG PE
          ON PE.ENG_ID = M.ENG_ID
        join t_au_period p
          on pe.period_id = p.auditperiodid
        JOIN t_auditee_entities E
          ON E.ENTITY_ID = PE.ENTITY_ID
        Join t_auditee_entities_maping em
          on em.entity_id = e.entity_id
       WHERE M.ENG_ID = P_ENG_ID
         AND (V_VER IS NULL OR M.REPORT_VERSION = V_VER);
  END;

  PROCEDURE P_GET_PDF_REPORT_META(P_ENG_ID         IN NUMBER,
                                  P_REPORT_VERSION IN NUMBER,
                                  O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT M.REPORT_STATUS AS REPORT_STATUS,
             TO_CHAR(M.REPORT_VERSION) AS VERSION_NO,
             M.GENERATED_BY AS GENERATED_BY,
             M.GENERATED_ON AS GENERATED_ON,
             -- same entity source as header
             E.NAME        AS ENTITY_NAME,
             p.description AS AUDIT_PERIOD
        FROM T_FRPT_REPORT_META M
        JOIN T_AU_PLAN_ENG PE
          ON PE.ENG_ID = M.ENG_ID
        join t_au_period p
          on p.auditperiodid = pe.period_id
        JOIN t_Auditee_Entities E
          ON E.ENTITY_ID = PE.ENTITY_ID
       WHERE M.ENG_ID = P_ENG_ID
         AND (V_VER IS NULL OR M.REPORT_VERSION = V_VER);
  END;

  PROCEDURE P_GET_PDF_SECTIONS(P_ENG_ID         IN NUMBER,
                               P_REPORT_VERSION IN NUMBER,
                               O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT SM.SECTION_CODE AS SECTION_CODE,
             SM.SECTION_TITLE AS SECTION_TITLE,
             SM.DISPLAY_ORDER AS DISPLAY_ORDER,
             SM.IS_MANDATORY AS IS_MANDATORY,
             NVL(pn.section_text, '') AS TEXT_BLOCK
        FROM T_FRPT_SECTION_MASTER SM
        LEFT JOIN T_FRPT_TEXT_BLOCKS PN
          ON PN.ENG_ID = P_ENG_ID
         AND PN.SECTION_CODE = SM.SECTION_CODE
      
       WHERE NVL(SM.IS_ACTIVE, 'Y') = 'Y'
       ORDER BY SM.DISPLAY_ORDER;
  END;

  PROCEDURE P_GET_PDF_KPI(P_ENG_ID         IN NUMBER,
                          P_REPORT_VERSION IN NUMBER,
                          O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT KPI_CODE,
             KPI_LABEL,
             a.period_end as PERIOD_END_DATE,
             ACTUAL_VALUE,
             TARGET_VALUE,
             UNIT
        FROM T_FRPT_KPI_SNAPSHOT a
       WHERE ENG_ID = P_ENG_ID
      --AND (V_VER IS NULL OR REPORT_VERSION = V_VER)
       ORDER BY KPI_CODE;
  END;

  PROCEDURE P_GET_PDF_NPL(P_ENG_ID         IN NUMBER,
                          P_REPORT_VERSION IN NUMBER,
                          O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT a.npl_bucket      CATEGORY,
             a.period_end      PERIOD_END_DATE,
             a.cases_count     as CASE_COUNT,
             a.outstanding_amt as OUTSTANDING_AMOUNT,
             a.provision_amt   as PROVISION_AMOUNT
        FROM T_FRPT_NPL_SNAPSHOT a
       WHERE ENG_ID = P_ENG_ID
      -- AND (V_VER IS NULL OR REPORT_VERSION = V_VER)
       ORDER BY CATEGORY, PERIOD_END_DATE;
  END;

  PROCEDURE P_GET_PDF_STAFF(P_ENG_ID         IN NUMBER,
                            P_REPORT_VERSION IN NUMBER,
                            O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT s.pp_no,
             s.staff_name  as NAME,
             s.staff_rank  as RANK,
             s.designation
        FROM T_FRPT_STAFF_SNAPSHOT s
       WHERE ENG_ID = P_ENG_ID
      --AND (V_VER IS NULL OR REPORT_VERSION = V_VER)
       ORDER BY DESIGNATION;
  END;

  PROCEDURE P_GET_PDF_STATISTICS(P_ENG_ID         IN NUMBER,
                                 P_REPORT_VERSION IN NUMBER,
                                 O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
    -- Non-Fraud grouped by s.description + separate Fraud row
      SELECT x.RISK_LEVEL,
             x.REPORTED_COUNT,
             x.RECTIFIED_COUNT,
             x.OUTSTANDING_COUNT,
             '' AS REMARKS
        FROM (
              -- 1) Fraud: single row
              SELECT 'Fraud' AS RISK_LEVEL,
                      COUNT(o.id) AS REPORTED_COUNT,
                      SUM(CASE
                            WHEN o.status <> 8 THEN
                             1
                            ELSE
                             0
                          END) AS RECTIFIED_COUNT,
                      SUM(CASE
                            WHEN o.status = 8 THEN
                             1
                            ELSE
                             0
                          END) AS OUTSTANDING_COUNT
                FROM t_au_observation o
               INNER JOIN t_risk s
                  ON o.severity = s.rating
               WHERE o.engplanid = P_ENG_ID
                 AND o.annex = 1
              UNION ALL
              -- 2) Non-fraud: grouped only by s.description
              SELECT s.description AS RISK_LEVEL,
                      COUNT(o.id) AS REPORTED_COUNT,
                      SUM(CASE
                            WHEN o.status <> 8 THEN
                             1
                            ELSE
                             0
                          END) AS RECTIFIED_COUNT,
                      SUM(CASE
                            WHEN o.status = 8 THEN
                             1
                            ELSE
                             0
                          END) AS OUTSTANDING_COUNT
                FROM t_au_observation o
               INNER JOIN t_risk s
                  ON o.severity = s.rating
               WHERE o.engplanid = P_ENG_ID
                 AND o.annex <> 1
               GROUP BY s.description
              
              ) x
       order by x.risk_level;
  END;
  PROCEDURE P_SAVE_PDF_STATISTICS(P_ENG_ID         IN NUMBER,
                                  P_REPORT_VERSION IN NUMBER,
                                  P_ROWS_JSON      IN CLOB,
                                  P_USER_PPNO      IN VARCHAR2) IS
    V_VER      NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
    V_ENTITYID NUMBER;
    V_USER_NO  NUMBER := F_TO_NUMBER_SAFE(P_USER_PPNO);
  BEGIN
    SELECT PE.ENTITY_ID
      INTO V_ENTITYID
      FROM T_AU_PLAN_ENG PE
     WHERE PE.ENG_ID = P_ENG_ID;
  
    DELETE FROM T_FRPT_PDF_STATISTICS
     WHERE ENG_ID = P_ENG_ID
       AND (V_VER IS NULL OR REPORT_VERSION = V_VER);
  
    INSERT INTO T_FRPT_PDF_STATISTICS
      (ENG_ID,
       ENTITY_ID,
       REPORT_VERSION,
       RISK_LEVEL,
       REPORTED_COUNT,
       RECTIFIED_COUNT,
       OUTSTANDING_COUNT,
       REMARKS,
       CREATED_ON,
       CREATED_BY)
      SELECT P_ENG_ID,
             V_ENTITYID,
             V_VER,
             JT.NATURE,
             CASE
               WHEN REGEXP_LIKE(JT.REPORTED_TXT, '^\s*\d+\s*$') THEN
                TO_NUMBER(TRIM(JT.REPORTED_TXT))
             END,
             CASE
               WHEN REGEXP_LIKE(JT.RECTIFIED_TXT, '^\s*\d+\s*$') THEN
                TO_NUMBER(TRIM(JT.RECTIFIED_TXT))
             END,
             CASE
               WHEN REGEXP_LIKE(JT.OUT_TXT, '^\s*\d+\s*$') THEN
                TO_NUMBER(TRIM(JT.OUT_TXT))
             END,
             JT.REMARKS,
             SYSDATE,
             V_USER_NO
        FROM JSON_TABLE(P_ROWS_JSON,
                        '$[*]'
                        COLUMNS(NATURE VARCHAR2(50) PATH '$.Nature',
                                REPORTED_TXT VARCHAR2(50) PATH '$.Reported',
                                RECTIFIED_TXT VARCHAR2(50) PATH '$.Rectified',
                                OUT_TXT VARCHAR2(50) PATH '$.Outstanding',
                                REMARKS VARCHAR2(2000) PATH '$.Remarks')) JT;
  
  END;

  PROCEDURE P_GET_INCOME_LEAKAGE(P_ENG_ID         IN NUMBER,
                                 P_REPORT_VERSION IN NUMBER,
                                 O_CURSOR         OUT SYS_REFCURSOR) IS
  BEGIN
    P_GET_PDF_INCOME_LEAKAGE(P_ENG_ID, P_REPORT_VERSION, O_CURSOR);
  END;

  PROCEDURE P_GET_PDF_INCOME_LEAKAGE(P_ENG_ID         IN NUMBER,
                                     P_REPORT_VERSION IN NUMBER,
                                     O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT NVL(DESCRIPTION, '') AS DESCRIPTION,
             NVL(AREA, '') AS CASE_REFERENCE,
             AMOUNT AS AMOUNT
        FROM T_FRPT_INCOME_LEAKAGE
       WHERE ENG_ID = P_ENG_ID
         AND (V_VER IS NULL OR REPORT_VERSION = V_VER)
       ORDER BY NVL(LINE_NO, 999999), FRPT_LEAK_ID;
  END;

  PROCEDURE P_SAVE_INCOME_LEAKAGE(P_ENG_ID         IN NUMBER,
                                  P_REPORT_VERSION IN NUMBER,
                                  P_ROWS_JSON      IN CLOB,
                                  P_USER_PPNO      IN VARCHAR2) IS
    V_VER      NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
    V_ENTITYID NUMBER;
    V_USER_NO  NUMBER := F_TO_NUMBER_SAFE(P_USER_PPNO);
  BEGIN
    SELECT PE.ENTITY_ID
      INTO V_ENTITYID
      FROM T_AU_PLAN_ENG PE
     WHERE PE.ENG_ID = P_ENG_ID;
  
    DELETE FROM T_FRPT_INCOME_LEAKAGE
     WHERE ENG_ID = P_ENG_ID
       AND (V_VER IS NULL OR REPORT_VERSION = V_VER);
  
    INSERT INTO T_FRPT_INCOME_LEAKAGE
      (ENG_ID,
       ENTITY_ID,
       REPORT_VERSION,
       LINE_NO,
       DESCRIPTION,
       AREA,
       AMOUNT,
       CREATED_ON,
       CREATED_BY)
      SELECT P_ENG_ID,
             V_ENTITYID,
             V_VER,
             JT.LINE_NO,
             JT.DESCRIPTION,
             JT.AREA,
             CASE
               WHEN JT.AMOUNT_TXT IS NULL THEN
                NULL
               WHEN REGEXP_LIKE(TRIM(JT.AMOUNT_TXT), '^-?\d+(\.\d+)?$') THEN
                TO_NUMBER(TRIM(JT.AMOUNT_TXT))
             END,
             SYSDATE,
             V_USER_NO
        FROM JSON_TABLE(P_ROWS_JSON,
                        '$[*]'
                        COLUMNS(LINE_NO FOR ORDINALITY,
                                DESCRIPTION VARCHAR2(500) PATH
                                '$.Description',
                                AREA VARCHAR2(200) PATH '$.Area',
                                AMOUNT_TXT VARCHAR2(50) PATH '$.Amount')) JT;
  
  END;

  PROCEDURE P_GET_OVERALL_CONCLUSION(P_ENG_ID         IN NUMBER,
                                     P_REPORT_VERSION IN NUMBER,
                                     O_CURSOR         OUT SYS_REFCURSOR) IS
    V_VER NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
  BEGIN
    OPEN O_CURSOR FOR
      SELECT OVERALL_CONCLUSION_HTML AS OVERALL_CONCLUSION_HTML,
             NON_ADDRESSABLE_HTML    AS NON_ADDRESSABLE_HTML,
             FRAUD_PRONE_HTML        AS FRAUD_PRONE_HTML,
             REGULATORY_HTML         AS REGULATORY_HTML,
             SAFETY_SECURITY_HTML    AS SAFETY_SECURITY_HTML
        FROM T_FRPT_OVERALL_CONCLUSION
       WHERE ENG_ID = P_ENG_ID
         AND (V_VER IS NULL OR REPORT_VERSION = V_VER);
  END;

  PROCEDURE P_SAVE_OVERALL_CONCLUSION(P_ENG_ID                  IN NUMBER,
                                      P_REPORT_VERSION          IN NUMBER,
                                      P_OVERALL_CONCLUSION_HTML IN CLOB,
                                      P_NON_ADDRESSABLE_HTML    IN CLOB,
                                      P_FRAUD_PRONE_HTML        IN CLOB,
                                      P_REGULATORY_HTML         IN CLOB,
                                      P_SAFETY_SECURITY_HTML    IN CLOB,
                                      P_USER_PPNO               IN VARCHAR2) IS
    V_VER      NUMBER := F_RESOLVE_VERSION(P_ENG_ID, P_REPORT_VERSION);
    V_ENTITYID NUMBER;
    V_USER_NO  NUMBER := F_TO_NUMBER_SAFE(P_USER_PPNO);
  BEGIN
    SELECT PE.ENTITY_ID
      INTO V_ENTITYID
      FROM T_AU_PLAN_ENG PE
     WHERE PE.ENG_ID = P_ENG_ID;
  
    MERGE INTO T_FRPT_OVERALL_CONCLUSION T
    USING (SELECT P_ENG_ID ENG_ID, V_VER REPORT_VERSION FROM DUAL) S
    ON (T.ENG_ID = S.ENG_ID AND T.REPORT_VERSION = S.REPORT_VERSION)
    WHEN MATCHED THEN
      UPDATE
         SET T.ENTITY_ID               = V_ENTITYID,
             T.OVERALL_CONCLUSION_HTML = P_OVERALL_CONCLUSION_HTML,
             T.NON_ADDRESSABLE_HTML    = P_NON_ADDRESSABLE_HTML,
             T.FRAUD_PRONE_HTML        = P_FRAUD_PRONE_HTML,
             T.REGULATORY_HTML         = P_REGULATORY_HTML,
             T.SAFETY_SECURITY_HTML    = P_SAFETY_SECURITY_HTML,
             T.UPDATED_ON              = SYSDATE,
             T.UPDATED_BY              = V_USER_NO
    WHEN NOT MATCHED THEN
      INSERT
        (ENG_ID,
         ENTITY_ID,
         REPORT_VERSION,
         OVERALL_CONCLUSION_HTML,
         NON_ADDRESSABLE_HTML,
         FRAUD_PRONE_HTML,
         REGULATORY_HTML,
         SAFETY_SECURITY_HTML,
         CREATED_ON,
         CREATED_BY)
      VALUES
        (P_ENG_ID,
         V_ENTITYID,
         V_VER,
         P_OVERALL_CONCLUSION_HTML,
         P_NON_ADDRESSABLE_HTML,
         P_FRAUD_PRONE_HTML,
         P_REGULATORY_HTML,
         P_SAFETY_SECURITY_HTML,
         SYSDATE,
         V_USER_NO);
  
  END;

  procedure R_getauditeeParas(EngId    in number,
                              ENT_ID   in number,
                              P_NO     in number,
                              R_ID     in number,
                              T_CURSOR OUT SYS_REFCURSOR) is
  BEGIN
    OPEN T_CURSOR FOR
      SELECT o.final_para_no AS PARA_ID,
             TO_CHAR(o.final_para_no, 'FM00') AS PARA_NO,
             
             r.description AS risk,
             a.code AS ANNEXURE_CODE,
             a.heading AS ANNEXURE,
             o.no_of_instances AS INSTANCES,
             o.amount_involved AS AMOUNT,
             'Outstanding' AS Rectification_Status,
             t.headings AS V_HEADER,
             t.text AS V_DETAIL,
             'Outstanding' as NATURE,
             
             NVL(n.implications, EMPTY_CLOB()) AS IMPLICATIONS,
             NVL(f.recommendation, '-') AS RECOMMENDATION,
             NVL(ae.reply, EMPTY_CLOB()) AS MANAGEMENT_REPLY,
             NVL(ar.recommendation, EMPTY_CLOB()) AS AUDITOR_COMMENTS,
             NVL(az.audit_reply, EMPTY_CLOB()) AS SVP_REMARKS,
             
             NVL(n.is_finalized, 0) AS IS_FINALIZED,
             NVL(n.updated_required, 0) AS UPDATED_REQUIRED,
             n.finalized_on,
             n.finalized_by,
             n.updated_on,
             n.updated_by,
             1 as IS_SIGNIFICANT
        FROM t_au_observation o
       INNER JOIN t_au_observation_text t
          ON t.observatsion_id = o.id
       INNER JOIN t_audit_checklist_annexure a
          ON a.id = o.annex
       INNER JOIN t_risk r
          ON r.rating = o.severity
       INNER JOIN t_au_observation_final_reccomendation f
          ON f.obs_id = o.id
       inner join t_au_observations_auditor_recommendation ar
          on ar.au_obs_id = o.id
       inner join t_au_observations_auditee_response ae
          on ae.au_obs_id = o.id
       inner join t_Au_Observations_Auditor_Reply az
          on az.au_obs_id = o.id
        LEFT JOIN t_frpt_para_narrative n
          ON n.eng_id = o.engplanid
         AND n.para_id = o.final_para_no
       WHERE o.engplanid = EngId
         AND o.status = 8
       ORDER BY o.final_para_no;
  
  end R_getauditeeParas;

  ------------------------Mangement Audit--------------

  /* =========================================================
  PKG_FRPT – BODY (ADD THESE PROCEDURE IMPLEMENTATIONS)
  ---------------------------------------------------------
  IMPORTANT:
  - Do NOT replace your entire PKG_FRPT body if it already
    exists with many procedures.
  - Instead, paste/merge ONLY the procedure bodies below
    into your existing PKG_FRPT package body.
  ========================================================= */

  -- 1) Cover (as provided by you; included here for completeness)
  PROCEDURE P_GET_AUDIT_COVER(P_ENG_ID IN NUMBER,
                              T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT e.name AS Audited_by,
             mp.p_name AS reporting,
             mp.c_name AS entity_name,
             ep.audit_startdate || '-' || ep.audit_enddate AS Audited_on
        FROM t_au_plan_eng ep
       INNER JOIN t_auditee_entities_maping mp
          ON mp.entity_id = ep.entity_id
       INNER JOIN t_auditee_entities e
          ON e.entity_id = ep.auditby_id
       WHERE ep.eng_id = P_ENG_ID;
  END P_GET_AUDIT_COVER;

  -- 2) Objective & Scope page (entered by Audit Team, stored via P_SAVE_TEXT_BLOCK in T_FRPT_TEXT_BLOCKS)
  -- Keys used:
  -- MAN_OBJ_OBJECTIVE, MAN_OBJ_SCOPE, MAN_OBJ_METHODOLOGY, MAN_OBJ_DISCLAIMER, MAN_OBJ_INTRODUCTION
  PROCEDURE P_GET_MAN_OBJECTIVE_SCOPE(P_ENG_ID IN NUMBER,
                                      T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT MAX(CASE
                   WHEN UPPER(SECTION_CODE) = 'MAN_OBJ_OBJECTIVE' THEN
                    DBMS_LOB.SUBSTR(SECTION_TEXT, 4000, 1)
                 END) AS OBJECTIVE,
             MAX(CASE
                   WHEN UPPER(SECTION_CODE) = 'MAN_OBJ_SCOPE' THEN
                    DBMS_LOB.SUBSTR(SECTION_TEXT, 4000, 1)
                 END) AS SCOPE,
             MAX(CASE
                   WHEN UPPER(SECTION_CODE) = 'MAN_OBJ_METHODOLOGY' THEN
                    DBMS_LOB.SUBSTR(SECTION_TEXT, 4000, 1)
                 END) AS METHODOLOGY,
             MAX(CASE
                   WHEN UPPER(SECTION_CODE) = 'MAN_OBJ_DISCLAIMER' THEN
                    DBMS_LOB.SUBSTR(SECTION_TEXT, 4000, 1)
                 END) AS DISCLAIMER,
             MAX(CASE
                   WHEN UPPER(SECTION_CODE) = 'MAN_OBJ_INTRODUCTION' THEN
                    DBMS_LOB.SUBSTR(SECTION_TEXT, 4000, 1)
                 END) AS INTRODUCTION
        FROM T_FRPT_TEXT_BLOCKS
       WHERE ENG_ID = P_ENG_ID
         AND UPPER(SECTION_CODE) IN
             ('MAN_OBJ_OBJECTIVE',
              'MAN_OBJ_SCOPE',
              'MAN_OBJ_METHODOLOGY',
              'MAN_OBJ_DISCLAIMER',
              'MAN_OBJ_INTRODUCTION');
  END P_GET_MAN_OBJECTIVE_SCOPE;

  -- 3) Executive Summary (entered by Audit Team, stored via P_SAVE_TEXT_BLOCK)
  -- Key used: MAN_EXEC_SUMMARY
  PROCEDURE P_GET_MAN_EXEC_SUMMARY(P_ENG_ID IN NUMBER,
                                   T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT MAX(DBMS_LOB.SUBSTR(SECTION_TEXT, 4000, 1)) AS TEXT_BLOCK
        FROM T_FRPT_TEXT_BLOCKS
       WHERE ENG_ID = P_ENG_ID
         AND UPPER(SECTION_CODE) = 'MAN_EXEC_SUMMARY';
  END P_GET_MAN_EXEC_SUMMARY;

  -- 4) Staff Snapshot (stored in T_FRPT_STAFF_SNAPSHOT via P_SAVE_STAFF_SNAPSHOT)
  PROCEDURE P_GET_MAN_STAFF_SNAPSHOT(P_ENG_ID IN NUMBER,
                                     T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT s.pp_no, s.staff_name, s.staff_rank, s.designation
        FROM T_FRPT_STAFF_SNAPSHOT s
       WHERE ENG_ID = P_ENG_ID;
  
  END P_GET_MAN_STAFF_SNAPSHOT;

  -- 5) Audit Observations (procedure-driven; replace the SELECT source once you confirm IAS tables/procs)
  -- Required output columns:
  -- PARA_NO, TITLE, PARA_TEXT, RISK_CATEGORY, RECOMMENDATION, MANAGEMENT_REPLY, AUDIT_REPLY, STATUS
  PROCEDURE P_GET_MAN_AUDIT_OBSERVATIONS(P_ENG_ID IN NUMBER,
                                         T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT CAST(NULL AS NUMBER) AS PARA_NO,
             CAST(NULL AS VARCHAR2(4000)) AS TITLE,
             TO_CLOB(NULL) AS PARA_TEXT,
             CAST(NULL AS VARCHAR2(200)) AS RISK_CATEGORY,
             TO_CLOB(NULL) AS RECOMMENDATION,
             TO_CLOB(NULL) AS MANAGEMENT_REPLY,
             TO_CLOB(NULL) AS AUDIT_REPLY,
             CAST('Un-Settled' AS VARCHAR2(50)) AS STATUS
        FROM DUAL
       WHERE 1 = 0;
  END P_GET_MAN_AUDIT_OBSERVATIONS;

  -- 6) Paras settled during audit (procedure-driven; replace the SELECT source once confirmed)
  -- Required output columns:
  -- TITLE, PARA_TEXT, MANAGEMENT_REPLY
  PROCEDURE P_GET_MAN_SETTLED_PARAS(P_ENG_ID IN NUMBER,
                                    T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT CAST(NULL AS VARCHAR2(4000)) AS TITLE,
             TO_CLOB(NULL) AS PARA_TEXT,
             TO_CLOB(NULL) AS MANAGEMENT_REPLY
        FROM DUAL
       WHERE 1 = 0;
  END P_GET_MAN_SETTLED_PARAS;

  -- 7) Team details for cover bottom (wrapper; replace SELECT with your existing team query/proc)
  -- Required output columns (recommended shape):
  -- MEMBER_NAME, ROLE_TITLE, PP_NO
  PROCEDURE P_GET_MAN_TEAM_DETAILS(P_ENG_ID IN NUMBER,
                                   T_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT d.member_name, d.ROLE_TITLE, d.PP_NO
        FROM v_get_audit_team_details d
       WHERE d.eng_plan_id = P_ENG_ID;
  END P_GET_MAN_TEAM_DETAILS;
  
  PROCEDURE P_GET_ALLOWED_PDF_ENG_DETAILS (P_PP_NO IN NUMBER,
                                           P_R_ID IN NUMBER,
                                           P_ENT_ID IN NUMBER,
                                           o_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
    SELECT P.ENG_ID,
           E.P_NAME AS REPORTING_OFFICE,
           E.C_NAME as ENTITY_NAME,
           P.AUDIT_STARTDATE AS AUDIT_START_DATE,
           P.Audit_Enddate as AUDIT_END_DATE,
           f.report_status    REPORT_STATUS
    FROM T_AU_PLAN_ENG P
    INNER JOIN T_AUDITEE_ENTITIES_MAPING E
    ON P.ENTITY_ID = E.ENTITY_ID
    inner join T_FRPT_REPORT_META f
    on f.eng_id = p.eng_id
        where e.auditedby = 
    case when P_R_ID in (15,16) then P_ENT_ID
         when P_R_ID in (1,5) then e.auditedby end
           order by p.audit_startdate ;
    
    
END P_GET_ALLOWED_PDF_ENG_DETAILS;

 ----------------------------------------------------------------------
  -- Procedure: P_GET_AUDIT_DEPARTMENTS
  -- Purpose  : Returns Audit Departments for dropdown selection.
  -- Used By  : DBConnection.GetAuditDepartments()
  ----------------------------------------------------------------------
  PROCEDURE P_GET_AUDIT_DEPARTMENTS(O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT DISTINCT D.ENTITY_ID AS ID,
                      TO_CHAR(D.CODE) AS CODE,
                      D.NAME AS NAME,
                      NVL(D.ACTIVE, 'Y') AS STATUS
        FROM T_AUDITEE_ENTITIES D
       WHERE NVL(D.ACTIVE, 'Y') = 'Y'
         AND EXISTS (SELECT 1
                FROM T_AU_PLAN_ENG PE
               WHERE PE.AUDITBY_ID = D.ENTITY_ID)
       ORDER BY D.NAME;
  END P_GET_AUDIT_DEPARTMENTS;

  ----------------------------------------------------------------------
  -- Procedure: P_GET_OUTSTANDING_PARA_ENTITIES
  -- Purpose  : Returns one row per engagement/entity to build the
  --            entity-wise cover page in the consolidated PDF.
  -- Used By  : DBConnection.GetOutstandingParaEntitiesForPdf()
  ----------------------------------------------------------------------
  PROCEDURE P_GET_OUTSTANDING_PARA_ENTITIES(P_AUDIT_DEPARTMENT_ID  IN NUMBER,
                                            P_EXECUTION_START_DATE IN DATE,
                                            P_EXECUTION_END_DATE   IN DATE,
                                            O_CURSOR               OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT DISTINCT PE.ENG_ID AS ENG_ID,
                      ENT.ENTITY_ID AS ENTITY_ID,
                      NVL(MAP.C_NAME, ENT.NAME) AS ENTITY_NAME,
                      COALESCE(TO_CHAR(PE.ENTITY_CODE), TO_CHAR(ENT.CODE)) AS ENTITY_CODE,
                      DEPT.NAME AS AUDIT_DEPARTMENT,
                      PER.DESCRIPTION AS AUDIT_PERIOD,
                      PE.AUDIT_STARTDATE AS EXECUTION_START_DATE,
                      PE.AUDIT_ENDDATE AS EXECUTION_END_DATE,
                      (SELECT MAX(TM.MEMBER_NAME)
                         FROM T_AU_TEAM_MEMBERS TM
                        WHERE TM.T_ID = PE.TEAM_ID
                          AND NVL(TM.ISTEAMLEAD, 'N') = 'Y') AS TEAM_LEAD,
                      (SELECT LISTAGG(TM.MEMBER_NAME, ', ') WITHIN GROUP(ORDER BY TM.MEMBER_NAME)
                         FROM T_AU_TEAM_MEMBERS TM
                        WHERE TM.T_ID = PE.TEAM_ID
                          AND NVL(TM.ISTEAMLEAD, 'N') <> 'Y') AS TEAM_MEMBERS,
                          (SELECT Count(1)
                FROM T_AU_OBSERVATION O
               WHERE O.ENGPLANID = PE.ENG_ID
                 AND O.STATUS = 8) as OUTSTANDING_PARAS_COUNT
      
        FROM T_AU_PLAN_ENG PE
        JOIN T_AUDITEE_ENTITIES ENT
          ON ENT.ENTITY_ID = PE.ENTITY_ID
        LEFT JOIN T_AUDITEE_ENTITIES DEPT
          ON DEPT.ENTITY_ID = PE.AUDITBY_ID
        LEFT JOIN T_AU_PERIOD PER
          ON PER.AUDITPERIODID = PE.PERIOD_ID
        LEFT JOIN T_AUDITEE_ENTITIES_MAPING MAP
          ON MAP.ENTITY_ID = PE.ENTITY_ID
         AND MAP.AUDITEDBY = PE.AUDITBY_ID
      
       WHERE PE.AUDITBY_ID = P_AUDIT_DEPARTMENT_ID
         AND TRUNC(PE.AUDIT_STARTDATE) >= TRUNC(P_EXECUTION_START_DATE)
         AND TRUNC(PE.AUDIT_ENDDATE) <= TRUNC(P_EXECUTION_END_DATE)
         AND EXISTS (SELECT 1
                FROM T_AU_OBSERVATION O
               WHERE O.ENGPLANID = PE.ENG_ID
                 AND O.STATUS = 8)
       ORDER BY NVL(MAP.C_NAME, ENT.NAME), PE.AUDIT_STARTDATE, PE.ENG_ID;
  END P_GET_OUTSTANDING_PARA_ENTITIES;

  ----------------------------------------------------------------------
  -- Procedure: P_GET_OUTSTANDING_PARAS_FOR_PDF
  -- Purpose  : Returns all outstanding paras for the selected Audit
  --            Department and Execution Date Range.
  -- Used By  : DBConnection.GetOutstandingParasForPdf()
  ----------------------------------------------------------------------
  PROCEDURE P_GET_OUTSTANDING_PARAS_FOR_PDF(P_AUDIT_DEPARTMENT_ID  IN NUMBER,
                                            P_EXECUTION_START_DATE IN DATE,
                                            P_EXECUTION_END_DATE   IN DATE,
                                            O_CURSOR               OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      WITH LATEST_MANAGEMENT_RESPONSE AS
       (SELECT AU_OBS_ID, REPLY
          FROM (SELECT AR.AU_OBS_ID,
                       AR.REPLY,
                       ROW_NUMBER() OVER(PARTITION BY AR.AU_OBS_ID ORDER BY AR.REPLIEDDATE DESC NULLS LAST, AR.ID DESC) AS RN
                  FROM T_AU_OBSERVATIONS_AUDITEE_RESPONSE AR
                 WHERE NVL(AR.SUBMITTED, 'Y') = 'Y')
         WHERE RN = 1),
      LATEST_AUDIT_REMARKS AS
       (SELECT AU_OBS_ID, AUDIT_REPLY
          FROM (SELECT RP.AU_OBS_ID,
                       RP.AUDIT_REPLY,
                       ROW_NUMBER() OVER(PARTITION BY RP.AU_OBS_ID ORDER BY RP.REPLIEDDATE DESC NULLS LAST, RP.ID DESC) AS RN
                  FROM T_AU_OBSERVATIONS_AUDITOR_REPLY RP
                 WHERE NVL(RP.SUBMITTED, 'Y') = 'Y')
         WHERE RN = 1)
      SELECT PE.ENG_ID AS ENG_ID,
             ENT.ENTITY_ID AS ENTITY_ID,
             NVL(MAP.C_NAME, ENT.NAME) AS ENTITY_NAME,
             TO_CHAR(O.FINAL_PARA_NO) AS PARA_NO,
             OT.HEADINGS AS PARA_TITLE,
             R.DESCRIPTION AS RISK_CATEGORY,
             OT.TEXT AS OBSERVATION_TEXT,
             MR.REPLY AS LATEST_MANAGEMENT_RESPONSE,
             AU.AUDIT_REPLY AS AUDIT_REMARKS,
             NVL(OS.STATUSNAME, 'Outstanding') AS CURRENT_COMPLIANCE_STATUS
      
        FROM T_AU_PLAN_ENG PE
        JOIN T_AUDITEE_ENTITIES ENT
          ON ENT.ENTITY_ID = PE.ENTITY_ID
        LEFT JOIN T_AUDITEE_ENTITIES_MAPING MAP
          ON MAP.ENTITY_ID = PE.ENTITY_ID
         AND MAP.AUDITEDBY = PE.AUDITBY_ID
        JOIN T_AU_OBSERVATION O
          ON O.ENGPLANID = PE.ENG_ID
        JOIN T_AU_OBSERVATION_TEXT OT
          ON OT.OBSERVATSION_ID = O.ID
        LEFT JOIN T_RISK R
          ON R.RATING = O.SEVERITY
        LEFT JOIN T_AU_OBSERVATION_STATUS OS
          ON OS.STATUSID = O.STATUS
        LEFT JOIN LATEST_MANAGEMENT_RESPONSE MR
          ON MR.AU_OBS_ID = O.ID
        LEFT JOIN LATEST_AUDIT_REMARKS AU
          ON AU.AU_OBS_ID = O.ID
      
       WHERE PE.AUDITBY_ID = P_AUDIT_DEPARTMENT_ID
         AND TRUNC(PE.AUDIT_STARTDATE) >= TRUNC(P_EXECUTION_START_DATE)
         AND TRUNC(PE.AUDIT_ENDDATE) <= TRUNC(P_EXECUTION_END_DATE)
         AND O.STATUS = 8
       ORDER BY NVL(MAP.C_NAME, ENT.NAME),
                PE.AUDIT_STARTDATE,
                PE.ENG_ID,
                O.FINAL_PARA_NO,
                O.ID;
  END P_GET_OUTSTANDING_PARAS_FOR_PDF;

  ----------------------------------------------------------------------
  -- Procedure: P_GET_OUTSTANDING_PARAS_SUMMARY_PDF
  -- Purpose  : Returns one consolidated outstanding paras register for
  --            CIA review. This is independent of the detailed PDF/ZIP
  --            export procedures above.
  -- Used By  : DBConnection.GetOutstandingParasSummaryForPdf()
  ----------------------------------------------------------------------

  PROCEDURE P_GET_OUTSTANDING_PARAS_SUMMARY_SETS(P_AUDIT_DEPARTMENT_ID IN NUMBER,
                                                 P_RISK                IN VARCHAR2,
                                                 O_CURSOR              OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT m.entity_id AS ENTITY_ID,
             m.c_name AS ENTITY_NAME,
             r.description AS RISK,
             COUNT(1) AS ROW_COUNT
        FROM ais_t_au_post_compliance c
       INNER JOIN t_auditee_entities_maping m
          ON c.audited_by = m.entity_id
       INNER JOIN t_risk r
          ON r.rating = c.risk
       INNER JOIN v_get_all_para_text t
          ON t.com_id = c.com_id
       WHERE c.para_status = 8
         AND (NVL(P_AUDIT_DEPARTMENT_ID, 0) = 0 OR
             m.auditedby = P_AUDIT_DEPARTMENT_ID)
         AND (P_RISK IS NULL OR TRIM(P_RISK) IS NULL OR
             UPPER(TRIM(P_RISK)) = 'ALL' OR
             UPPER(TRIM(r.description)) = UPPER(TRIM(P_RISK)))
       GROUP BY m.entity_id, m.c_name, r.description
       ORDER BY m.c_name,
                DECODE(UPPER(r.description),
                       'HIGH',
                       1,
                       'MEDIUM',
                       2,
                       'LOW',
                       3,
                       4),
                r.description;
  END P_GET_OUTSTANDING_PARAS_SUMMARY_SETS;
  
  PROCEDURE P_GET_OUTSTANDING_PARAS_SUMMARY_SET_PDF(P_AUDIT_DEPARTMENT_ID IN NUMBER,
                                                   P_ENTITY_ID           IN NUMBER,
                                                   P_RISK                IN VARCHAR2,
                                                   O_CURSOR              OUT T_CURSOR) IS
BEGIN
  OPEN O_CURSOR FOR
    SELECT c.entity_id AS ENTITY_ID,
           az.name AS AUDIT_DEPARTMENT,
           m.p_name as REPORTING,
           m.c_name AS ENTITY_NAME,
           c.audit_period as AUDIT_PERIOD,
           TO_CHAR(c.para_no) AS PARA_NO,
           c.gist_of_paras as GIST_HEADING,
           r.description as risk,
           t.text PARA_TEXT,
           'Outstanding' AS CURRENT_COMPLIANCE_STATUS
      FROM ais_t_au_post_compliance c
     INNER JOIN t_auditee_entities_maping m
        ON c.entity_id = m.entity_id
     INNER JOIN t_auditee_entities az
        ON az.entity_id = m.auditedby
     INNER JOIN t_risk r
        ON r.rating = c.risk
     INNER JOIN v_get_all_para_text t
        ON t.com_id = c.com_id
     WHERE c.para_status = 8
       AND (NVL(P_AUDIT_DEPARTMENT_ID, 0) = 0 OR
           m.auditedby = P_AUDIT_DEPARTMENT_ID)
       AND c.audited_by = P_ENTITY_ID
       AND (P_RISK IS NULL OR TRIM(P_RISK) IS NULL OR
           UPPER(TRIM(P_RISK)) = 'ALL' OR
           UPPER(TRIM(r.description)) = UPPER(TRIM(P_RISK)))
     ORDER BY c.audit_period,
              TO_NUMBER(NULLIF(REGEXP_REPLACE(TO_CHAR(t.com_id), '[^0-9]', ''), '')) NULLS LAST,
              TO_CHAR(t.com_id);
END P_GET_OUTSTANDING_PARAS_SUMMARY_SET_PDF;

  PROCEDURE P_GET_OUTSTANDING_PARAS_SUMMARY_PDF(P_AUDIT_DEPARTMENT_ID IN NUMBER,
                                                P_RISK                IN VARCHAR2,
                                                O_CURSOR              OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
    
      Select az.name AS AUDIT_DEPARTMENT,
             c.para_no as Para_no,
             m.c_name AS ENTITY_NAME,
             c.audit_period as AUDIT_PERIOD,
             c.gist_of_paras as GIST_HEADING,
             r.description as risk,
             t.com_id,
             t.text PARA_TEXT,
             'Outstanding' AS CURRENT_COMPLIANCE_STATUS
      
        from ais_t_au_post_compliance c
       inner join t_auditee_entities_maping m
          on c.entity_id = m.entity_id
       inner join t_auditee_entities az
          on az.entity_id = m.auditedby
       inner join t_risk r
          on r.rating = c.risk
       inner join v_get_all_para_text t
          on t.com_id = c.com_id
       where c.para_status = 8
            
         AND (NVL(P_AUDIT_DEPARTMENT_ID, 0) = 0 OR
             m.auditedby = P_AUDIT_DEPARTMENT_ID)
         AND (P_RISK IS NULL OR TRIM(P_RISK) IS NULL OR
             UPPER(TRIM(P_RISK)) = 'ALL' OR
             UPPER(TRIM(R.DESCRIPTION)) = UPPER(TRIM(P_RISK)));
  END P_GET_OUTSTANDING_PARAS_SUMMARY_PDF;

  ----------------------------------------------------------------------
  -- Procedure: P_GET_OUTSTANDING_PARA_ENTITY_BY_ENG_ID
  -- Purpose  : Returns one engagement/entity row for single PDF export.
  -- Used By  : DBConnection.GetOutstandingParaEntityForPdfByEngId()
  ----------------------------------------------------------------------
  PROCEDURE P_GET_OUTSTANDING_PARA_ENTITY_BY_ENG_ID(P_ENG_ID IN NUMBER,
                                                    O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT DISTINCT PE.ENG_ID AS ENG_ID,
                      ENT.ENTITY_ID AS ENTITY_ID,
                      NVL(MAP.C_NAME, ENT.NAME) AS ENTITY_NAME,
                      COALESCE(TO_CHAR(PE.ENTITY_CODE), TO_CHAR(ENT.CODE)) AS ENTITY_CODE,
                      DEPT.NAME AS AUDIT_DEPARTMENT,
                      PER.DESCRIPTION AS AUDIT_PERIOD,
                      PE.AUDIT_STARTDATE AS EXECUTION_START_DATE,
                      PE.AUDIT_ENDDATE AS EXECUTION_END_DATE,
                      (SELECT MAX(TM.MEMBER_NAME)
                         FROM T_AU_TEAM_MEMBERS TM
                        WHERE TM.T_ID = PE.TEAM_ID
                          AND NVL(TM.ISTEAMLEAD, 'N') = 'Y') AS TEAM_LEAD,
                      (SELECT LISTAGG(TM.MEMBER_NAME, ', ') WITHIN GROUP(ORDER BY TM.MEMBER_NAME)
                         FROM T_AU_TEAM_MEMBERS TM
                        WHERE TM.T_ID = PE.TEAM_ID
                          AND NVL(TM.ISTEAMLEAD, 'N') <> 'Y') AS TEAM_MEMBERS,
                      (SELECT COUNT(1)
                         FROM T_AU_OBSERVATION OC
                        WHERE OC.ENGPLANID = PE.ENG_ID
                          AND OC.STATUS = 8) AS OUTSTANDING_PARAS_COUNT
      
        FROM T_AU_PLAN_ENG PE
        JOIN T_AUDITEE_ENTITIES ENT
          ON ENT.ENTITY_ID = PE.ENTITY_ID
         JOIN T_AUDITEE_ENTITIES DEPT
          ON DEPT.ENTITY_ID = PE.AUDITBY_ID
         JOIN T_AU_PERIOD PER
          ON PER.AUDITPERIODID = PE.PERIOD_ID
         JOIN T_AUDITEE_ENTITIES_MAPING MAP
          ON MAP.ENTITY_ID = PE.ENTITY_ID
         AND MAP.AUDITEDBY = PE.AUDITBY_ID
      
       WHERE PE.ENG_ID = P_ENG_ID
         AND EXISTS (SELECT 1
                FROM T_AU_OBSERVATION O
               WHERE O.ENGPLANID = PE.ENG_ID
                 AND O.STATUS = 8)
       ORDER BY NVL(MAP.C_NAME, ENT.NAME), PE.AUDIT_STARTDATE, PE.ENG_ID;
  END P_GET_OUTSTANDING_PARA_ENTITY_BY_ENG_ID;

  ----------------------------------------------------------------------
  -- Procedure: P_GET_OUTSTANDING_PARAS_BY_ENG_ID
  -- Purpose  : Returns outstanding paras for one engagement/entity PDF.
  -- Used By  : DBConnection.GetOutstandingParasForPdfByEngId()
  ----------------------------------------------------------------------
  PROCEDURE P_GET_OUTSTANDING_PARAS_BY_ENG_ID(P_ENG_ID IN NUMBER,
                                              O_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR     
      SELECT PE.ENG_ID AS ENG_ID,
             ENT.ENTITY_ID AS ENTITY_ID,
             NVL(MAP.C_NAME, ENT.NAME) AS ENTITY_NAME,
             TO_CHAR(O.FINAL_PARA_NO) AS PARA_NO,
             OT.HEADINGS AS PARA_TITLE,
             R.DESCRIPTION AS RISK_CATEGORY,
             OT.TEXT AS OBSERVATION_TEXT,
             MR.REPLY AS LATEST_MANAGEMENT_RESPONSE,
             AU.RECOMMENDATION AS AUDIT_REMARKS,
             NVL(OS.STATUSNAME, 'Outstanding') AS CURRENT_COMPLIANCE_STATUS
      
        FROM T_AU_PLAN_ENG PE
        JOIN T_AUDITEE_ENTITIES ENT
          ON ENT.ENTITY_ID = PE.ENTITY_ID
         JOIN T_AUDITEE_ENTITIES_MAPING MAP
          ON MAP.ENTITY_ID = PE.ENTITY_ID
         AND MAP.AUDITEDBY = PE.AUDITBY_ID
        JOIN T_AU_OBSERVATION O
          ON O.ENGPLANID = PE.ENG_ID
        JOIN T_AU_OBSERVATION_TEXT OT
          ON OT.OBSERVATSION_ID = O.ID
         JOIN T_RISK R
          ON R.RATING = O.SEVERITY
         JOIN T_AU_OBSERVATION_STATUS OS
          ON OS.STATUSID = O.STATUS
         JOIN t_Au_Observations_Auditee_Response MR
          ON MR.AU_OBS_ID = O.ID
         JOIN t_au_observation_final_reccomendation AU
          ON AU.OBS_ID = O.ID
      
       WHERE PE.ENG_ID = P_ENG_ID
         AND O.STATUS = 8
       ORDER BY NVL(MAP.C_NAME, ENT.NAME),
                PE.AUDIT_STARTDATE,
                PE.ENG_ID,
                O.FINAL_PARA_NO,
                O.ID;
  END P_GET_OUTSTANDING_PARAS_BY_ENG_ID;

END PKG_FRPT;


-- FAD Branch Risk Rating Engine (Excel risk model)
-- Phase 1: audit-period execution for T_AU_PLAN_ENG.ENTITY_TYPE = 6.
-- No legacy T_R_GROUP / T_R_SUB_GROUP scoring and no internal COMMIT.
-- Financial inputs / branch size are read only from existing APPROVED
-- T_FAD_RISK_ASSESSMENT and T_FAD_RISK_DETAIL records.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

CREATE OR REPLACE PACKAGE PKG_FAD_RISK_ENGINE AS
  PROCEDURE P_RUN_PERIOD(
    p_audit_period_id IN NUMBER, p_run_id OUT NUMBER,
    p_processed OUT NUMBER, p_calculated OUT NUMBER,
    p_pending OUT NUMBER, p_skipped OUT NUMBER, p_failed OUT NUMBER,
    p_user IN NUMBER, p_role IN NUMBER, p_entity IN NUMBER,
    p_group IN NUMBER, p_context IN NUMBER, p_session IN VARCHAR2);

  -- Phase 2: retained for later activation; not exposed on the Phase 1 UI.
  PROCEDURE P_RUN_ENTITY(
    p_audit_period_id IN NUMBER, p_ent_id IN NUMBER, p_run_id OUT NUMBER,
    p_processed OUT NUMBER, p_calculated OUT NUMBER,
    p_pending OUT NUMBER, p_skipped OUT NUMBER, p_failed OUT NUMBER,
    p_user IN NUMBER, p_role IN NUMBER, p_entity IN NUMBER,
    p_group IN NUMBER, p_context IN NUMBER, p_session IN VARCHAR2);

  PROCEDURE P_CATALOG(
    p_user IN NUMBER, p_role IN NUMBER, p_entity IN NUMBER,
    p_group IN NUMBER, p_context IN NUMBER, p_session IN VARCHAR2,
    o_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_RESULTS(
    p_user IN NUMBER, p_role IN NUMBER, p_entity IN NUMBER,
    p_group IN NUMBER, p_context IN NUMBER, p_session IN VARCHAR2,
    p_period IN NUMBER, p_type IN NUMBER, p_ent IN NUMBER, p_result IN NUMBER,
    o_current OUT SYS_REFCURSOR, o_cursor OUT SYS_REFCURSOR,
    o_detail OUT SYS_REFCURSOR, o_exceptions OUT SYS_REFCURSOR);
END PKG_FAD_RISK_ENGINE;
/

CREATE OR REPLACE PACKAGE BODY PKG_FAD_RISK_ENGINE AS

  PROCEDURE P_CHECK_SUPER_USER(
    p_user NUMBER,p_role NUMBER,p_entity NUMBER,
    p_group NUMBER,p_context NUMBER,p_session VARCHAR2) IS
    n NUMBER;
  BEGIN
    IF NVL(p_role,0)<>1 OR NVL(p_group,0)<>1 OR p_session IS NULL
       OR NVL(p_user,0)<=0 OR NVL(p_entity,0)<=0 THEN
      RAISE_APPLICATION_ERROR(-20043,'Super User session required.');
    END IF;
    SELECT COUNT(*) INTO n FROM T_USER u
      JOIN T_USER_SESSION s ON s.user_pp_number=u.ppno
      JOIN T_GROUPS g ON g.group_id=s.group_id AND g.role_id=s.role_id
     WHERE u.userid=p_user AND u.isactive='Y'
       AND g.group_id=1 AND g.role_id=1 AND g.status='Y'
       AND s.session_id=p_session AND s.session_active='Y'
       AND s.logged_out_date IS NULL AND s.entity_id=p_entity
       AND ((p_context IS NOT NULL AND s.user_context_id=p_context
             AND EXISTS (SELECT 1 FROM T_USER_CONTEXT_ASSIGNMENT c
                 WHERE c.user_context_id=p_context AND c.user_id=u.userid
                   AND c.role_id=1 AND c.entity_id=p_entity
                   AND NVL(c.group_id,c.role_id)=1 AND c.is_active='Y'
                   AND (c.effective_from IS NULL OR c.effective_from<=SYSDATE)
                   AND (c.effective_to IS NULL OR c.effective_to>=TRUNC(SYSDATE))))
         OR (p_context IS NULL AND s.user_context_id IS NULL
             AND u.entity_id=p_entity
             AND EXISTS (SELECT 1 FROM T_USER_MAPING m
                 WHERE m.userid=u.userid AND m.role_id=1 AND m.group_id=1)));
    IF n<>1 THEN RAISE_APPLICATION_ERROR(-20043,'Invalid Super User session.'); END IF;
  END P_CHECK_SUPER_USER;

  -- Common calculation used by both period and entity execution.
  PROCEDURE P_SCORE_BRANCH(p_result NUMBER,p_eng NUMBER,p_version NUMBER) IS
    v_size VARCHAR2(10);
    v_raw NUMBER;
    v_total NUMBER:=0;
    v_fraud NUMBER:=0;
    v_missing NUMBER:=0;
    v_invalid NUMBER;
    v_obs NUMBER;
    v_inst NUMBER;
    v_annex_id NUMBER;
    v_map_count NUMBER;
    v_base NUMBER;
    v_actual NUMBER;
    v_value NUMBER;
    v_marks NUMBER;
    v_process_missing NUMBER;
    v_obs_total NUMBER:=0;
    v_instance_total NUMBER:=0;
  BEGIN
    -- Existing IAS risk assessment is only a source for independently
    -- approved branch-size and quantitative inputs. Never change it here.
    BEGIN
      SELECT branch_size INTO v_size
        FROM T_FAD_RISK_ASSESSMENT
       WHERE eng_id=p_eng AND version_id=p_version AND status='APPROVED';
    EXCEPTION WHEN NO_DATA_FOUND THEN
      v_size:=NULL;
      v_missing:=v_missing+1;
      INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
      VALUES(p_result,'BRANCH_SIZE',
             'Approved branch size is unavailable for this engagement.');
    END;

    -- All 20 Excel-configured processes, including the zero-weight fraud rule.
    FOR p IN (
      SELECT process_code,main_kpi,sub_kpi,description,weight,denominator
        FROM T_FAD_RISK_PROCESS WHERE version_id=p_version
       ORDER BY process_code
    ) LOOP
      v_raw:=0;
      v_process_missing:=0;

      FOR sub IN (
        SELECT sub_code,description,gravity,criterion
          FROM T_FAD_RISK_SUBPROCESS
         WHERE version_id=p_version AND process_code=p.process_code
         ORDER BY sub_code
      ) LOOP
        IF sub.criterion IN ('instances','fraud_override') THEN
          FOR a IN (
            SELECT code,description,risk_level
              FROM T_FAD_RISK_ANNEX
             WHERE version_id=p_version AND sub_code=sub.sub_code
             ORDER BY code
          ) LOOP
            -- Stage is the approved source-code -> numeric IAS ANNEX bridge.
            SELECT COUNT(*),MAX(anex_id) INTO v_map_count,v_annex_id
              FROM T_FAD_RISK_MAP_STAGE
             WHERE REPLACE(UPPER(TRIM(annexure_code)),'-','') =
                   REPLACE(UPPER(TRIM(a.code)),'-','');
            IF v_map_count<>1 OR v_annex_id IS NULL THEN
              RAISE_APPLICATION_ERROR(-20101,'Annexure mapping missing: '||a.code);
            END IF;

            SELECT COUNT(*),
                   NVL(SUM(CASE WHEN no_of_instances>=0
                                    AND no_of_instances=TRUNC(no_of_instances)
                                THEN no_of_instances END),0),
                   COUNT(CASE WHEN no_of_instances IS NULL
                                 OR no_of_instances<0
                                 OR no_of_instances<>TRUNC(no_of_instances)
                              THEN 1 END)
              INTO v_obs,v_inst,v_invalid
              FROM T_AU_OBSERVATION
             WHERE engplanid=p_eng AND annex=v_annex_id;

            v_obs_total:=v_obs_total+v_obs;
            v_instance_total:=v_instance_total+v_inst;
            IF v_invalid>0 THEN
              v_process_missing:=1; v_missing:=v_missing+1;
              INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
              VALUES(p_result,'INSTANCE_INPUT',
                     'Invalid NO_OF_INSTANCES for Annexure '||a.code);
            END IF;

            IF sub.criterion='fraud_override' THEN
              v_fraud:=v_fraud+v_inst;
            ELSIF v_invalid=0 THEN
              v_raw:=v_raw+PKG_FAD_RISK_SCORE_V1.instance_marks(
                              sub.gravity,v_inst);
            END IF;

            INSERT INTO T_FAD_RISK_ENGINE_DETAIL(
              result_id,node_level,main_code,package_code,process_code,
              procedure_code,annex_id,source_code,description,
              risk_classification,observations,instances,score,status)
            VALUES(p_result,'ANNEXURE',p.main_kpi,p.sub_kpi,p.process_code,
                   sub.sub_code,v_annex_id,a.code,a.description,
                   a.risk_level,v_obs,
                   CASE WHEN v_invalid=0 THEN v_inst END,
                   CASE WHEN v_invalid=0 AND sub.criterion='instances'
                        THEN sub.gravity*v_inst END,
                   CASE WHEN v_invalid=0 THEN 'CALCULATED'
                        ELSE 'PENDING_INPUT' END);
          END LOOP;
        ELSE
          -- The Excel model uses quantitative inputs for achievement and
          -- profitability. Reuse approved IAS assessment data if available.
          BEGIN
            SELECT d.base_value,d.actual_value
              INTO v_base,v_actual
              FROM T_FAD_RISK_ASSESSMENT h
              JOIN T_FAD_RISK_DETAIL d ON d.assessment_id=h.assessment_id
               AND d.version_id=h.version_id
             WHERE h.eng_id=p_eng AND h.version_id=p_version
               AND h.status='APPROVED' AND d.item_code=sub.sub_code;

            CASE sub.criterion
              WHEN 'movement' THEN
                v_value:=PKG_FAD_RISK_SCORE_V1.movement_marks(v_base,v_actual);
              WHEN 'shortfall' THEN
                v_value:=PKG_FAD_RISK_SCORE_V1.shortfall_marks(v_base,v_actual);
              WHEN 'profit' THEN
                v_value:=PKG_FAD_RISK_SCORE_V1.profit_marks(v_actual);
            END CASE;
            v_raw:=v_raw+v_value;

            INSERT INTO T_FAD_RISK_ENGINE_DETAIL(
              result_id,node_level,main_code,package_code,process_code,
              procedure_code,source_code,description,score,status)
            VALUES(p_result,'INPUT',p.main_kpi,p.sub_kpi,p.process_code,
                   sub.sub_code,sub.sub_code,sub.description,
                   v_value,'CALCULATED');

          EXCEPTION WHEN NO_DATA_FOUND THEN
            v_process_missing:=1; v_missing:=v_missing+1;
            INSERT INTO T_FAD_RISK_ENGINE_DETAIL(
              result_id,node_level,main_code,package_code,process_code,
              procedure_code,source_code,description,status)
            VALUES(p_result,'INPUT',p.main_kpi,p.sub_kpi,p.process_code,
                   sub.sub_code,sub.sub_code,sub.description,'PENDING_INPUT');
            INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
            VALUES(p_result,'FINANCIAL_INPUT',
                   'Approved input unavailable for '||sub.sub_code);
          END;
        END IF;
      END LOOP;

      IF v_process_missing=0 THEN
        v_marks:=PKG_FAD_RISK_SCORE_V1.normalized_marks(
                   v_raw,p.weight,p.denominator);
        v_total:=v_total+v_marks;
      ELSE
        v_marks:=NULL;
      END IF;
      INSERT INTO T_FAD_RISK_ENGINE_DETAIL(
        result_id,node_level,main_code,package_code,process_code,
        description,score,status)
      VALUES(p_result,'PROCESS',p.main_kpi,p.sub_kpi,p.process_code,
             p.description,v_marks,
             CASE WHEN v_process_missing=0 THEN 'CALCULATED'
                  ELSE 'PENDING_INPUT' END);
    END LOOP;

    -- A source-model Annexure mismatch must never be silently ignored.
    SELECT COUNT(*) INTO v_invalid
      FROM T_AU_OBSERVATION o
     WHERE o.engplanid=p_eng
       AND NOT EXISTS (
          SELECT 1 FROM T_FAD_RISK_MAP_STAGE s
           WHERE s.anex_id=o.annex);
    IF v_invalid>0 THEN
      v_missing:=v_missing+1;
      INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
      VALUES(p_result,'UNMAPPED_OBSERVATION',
             'Observations exist with Annexure IDs outside the approved risk model.');
    END IF;

    IF v_missing=0 THEN
      UPDATE T_FAD_RISK_ENGINE_RESULT
         SET observations=v_obs_total,instances=v_instance_total,
             score=v_total,
             risk_rating=PKG_FAD_RISK_SCORE_V1.final_rating(
                           v_total,v_size,v_fraud),
             status='CALCULATED'
       WHERE result_id=p_result;
    ELSE
      -- Process-level marks remain visible, but an incomplete total is not
      -- published as a branch score or risk category.
      UPDATE T_FAD_RISK_ENGINE_RESULT
         SET observations=v_obs_total,instances=v_instance_total,
             score=NULL,risk_rating=NULL,status='PENDING_INPUT'
       WHERE result_id=p_result;
    END IF;
  END P_SCORE_BRANCH;

  PROCEDURE P_EXECUTE(
    p_period NUMBER,p_ent NUMBER,p_run_id OUT NUMBER,
    p_processed OUT NUMBER,p_calculated OUT NUMBER,
    p_pending OUT NUMBER,p_skipped OUT NUMBER,p_failed OUT NUMBER,
    p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,
    p_context NUMBER,p_session VARCHAR2) IS
    v_version NUMBER;
    v_count NUMBER;
    v_distinct NUMBER;
    v_weight NUMBER;
    v_ref SYS_REFCURSOR;
    v_result NUMBER;
    v_error VARCHAR2(2000);
    v_source_sha VARCHAR2(64);
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    IF NVL(p_period,0)<=0 OR (p_ent IS NOT NULL AND p_ent<=0) THEN
      RAISE_APPLICATION_ERROR(-20040,'Invalid audit period or entity.');
    END IF;
    SELECT COUNT(*) INTO v_count FROM T_AU_PERIOD
     WHERE auditperiodid=p_period;
    IF v_count<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Unknown audit period.'); END IF;

    SELECT COUNT(*) INTO v_count FROM T_AU_PLAN_ENG
     WHERE period_id=p_period AND entity_type=6
       AND (p_ent IS NULL OR entity_id=p_ent);
    IF v_count=0 THEN
      RAISE_APPLICATION_ERROR(-20040,'No branch engagements for this audit period.');
    END IF;

    -- Guard the same published 20-process workbook configuration.
    SELECT CASE WHEN COUNT(*)=1 THEN MIN(version_id) END INTO v_version
      FROM T_FAD_RISK_VERSION WHERE state='PUBLISHED'
        AND calculator='PKG_FAD_RISK_SCORE_V1';
    IF v_version IS NULL THEN
      RAISE_APPLICATION_ERROR(-20040,'A unique published Excel scoring version is required.');
    END IF;
    SELECT COUNT(*),NVL(SUM(weight),0) INTO v_count,v_weight
      FROM T_FAD_RISK_PROCESS WHERE version_id=v_version;
    IF v_count<>20 OR v_weight<>100 THEN
      RAISE_APPLICATION_ERROR(-20040,'Published process weights are incomplete.');
    END IF;
    SELECT COUNT(*) INTO v_count FROM T_FAD_RISK_ANNEX
     WHERE version_id=v_version;
    IF v_count<>121 THEN
      RAISE_APPLICATION_ERROR(-20040,'Published Annexure mapping is incomplete.');
    END IF;

    -- Fingerprint verification is already implemented in the existing package.
    PKG_FAD_RISK.P_GET_REGISTER(v_ref);
    CLOSE v_ref;
    SELECT COUNT(*),COUNT(DISTINCT s.anex_id),MAX(s.source_file_sha256)
      INTO v_count,v_distinct,v_source_sha FROM T_FAD_RISK_MAP_STAGE s;
    IF v_count<>121 OR v_distinct<>121 THEN
      RAISE_APPLICATION_ERROR(-20101,'IAS numeric Annexure mapping must be complete and unique.');
    END IF;
    SELECT COUNT(*) INTO v_count FROM T_FAD_RISK_ANNEX a
     WHERE a.version_id=v_version AND NOT EXISTS (
       SELECT 1 FROM T_FAD_RISK_MAP_STAGE s
       JOIN T_AUDIT_CHECKLIST_ANNEXURE m ON m.id=s.anex_id
        WHERE REPLACE(UPPER(TRIM(s.annexure_code)),'-','')=
              REPLACE(UPPER(TRIM(a.code)),'-','')
          AND REPLACE(UPPER(TRIM(m.code)),'-','')=
              REPLACE(UPPER(TRIM(a.code)),'-',''));
    IF v_count<>0 THEN
      RAISE_APPLICATION_ERROR(-20101,'Workbook Annexures do not reconcile to IAS Annexure IDs.');
    END IF;

    INSERT INTO T_FAD_RISK_ENGINE_RUN(
      period_id,entity_type_id,entity_id,audit_office_id,actor_id,
      status,engine_version,started_on)
    VALUES(p_period,6,p_ent,p_entity,p_user,
           'RUNNING','EXCEL-RATING-1',SYSTIMESTAMP)
    RETURNING run_id INTO p_run_id;

    FOR e IN (SELECT g.eng_id,g.entity_id,b.name
                FROM T_AU_PLAN_ENG g
                JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
               WHERE g.period_id=p_period AND g.entity_type=6
                 AND (p_ent IS NULL OR g.entity_id=p_ent)
               ORDER BY g.eng_id) LOOP
      INSERT INTO T_FAD_RISK_ENGINE_RESULT(
        run_id,eng_id,entity_id,entity_name,entity_type_id,
        version_id,source_sha256,status)
      VALUES(p_run_id,e.eng_id,e.entity_id,e.name,6,
             v_version,v_source_sha,'RUNNING')
      RETURNING result_id INTO v_result;

      SAVEPOINT branch_risk;
      BEGIN
        P_SCORE_BRANCH(v_result,e.eng_id,v_version);
      EXCEPTION WHEN OTHERS THEN
        v_error:=SUBSTR(SQLERRM||' '||DBMS_UTILITY.FORMAT_ERROR_BACKTRACE,1,2000);
        ROLLBACK TO branch_risk;
        UPDATE T_FAD_RISK_ENGINE_RESULT SET status='FAILED'
         WHERE result_id=v_result;
        INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
        VALUES(v_result,'CALCULATION_ERROR',v_error);
      END;
    END LOOP;

    SELECT COUNT(*),
           COUNT(CASE WHEN status='CALCULATED' THEN 1 END),
           COUNT(CASE WHEN status='PENDING_INPUT' THEN 1 END),
           COUNT(CASE WHEN status='SKIPPED' THEN 1 END),
           COUNT(CASE WHEN status='FAILED' THEN 1 END)
      INTO p_processed,p_calculated,p_pending,p_skipped,p_failed
      FROM T_FAD_RISK_ENGINE_RESULT WHERE run_id=p_run_id;
    UPDATE T_FAD_RISK_ENGINE_RUN SET status='COMPLETED',
      completed_on=SYSTIMESTAMP,processed=p_processed,
      calculated=p_calculated,pending=p_pending,
      skipped=p_skipped,failed=p_failed WHERE run_id=p_run_id;
  END P_EXECUTE;

  PROCEDURE P_RUN_PERIOD(
    p_audit_period_id IN NUMBER,p_run_id OUT NUMBER,
    p_processed OUT NUMBER,p_calculated OUT NUMBER,
    p_pending OUT NUMBER,p_skipped OUT NUMBER,p_failed OUT NUMBER,
    p_user IN NUMBER,p_role IN NUMBER,p_entity IN NUMBER,
    p_group IN NUMBER,p_context IN NUMBER,p_session IN VARCHAR2) IS
  BEGIN
    P_EXECUTE(p_audit_period_id,NULL,p_run_id,
      p_processed,p_calculated,p_pending,p_skipped,p_failed,
      p_user,p_role,p_entity,p_group,p_context,p_session);
  END P_RUN_PERIOD;

  PROCEDURE P_RUN_ENTITY(
    p_audit_period_id IN NUMBER,p_ent_id IN NUMBER,p_run_id OUT NUMBER,
    p_processed OUT NUMBER,p_calculated OUT NUMBER,
    p_pending OUT NUMBER,p_skipped OUT NUMBER,p_failed OUT NUMBER,
    p_user IN NUMBER,p_role IN NUMBER,p_entity IN NUMBER,
    p_group IN NUMBER,p_context IN NUMBER,p_session IN VARCHAR2) IS
  BEGIN
    P_EXECUTE(p_audit_period_id,p_ent_id,p_run_id,
      p_processed,p_calculated,p_pending,p_skipped,p_failed,
      p_user,p_role,p_entity,p_group,p_context,p_session);
  END P_RUN_ENTITY;

  -- Period selector uses the same source and columns as the IAS period reader,
  -- without its additional status_id=2 filter.
  PROCEDURE P_CATALOG(
    p_user IN NUMBER,p_role IN NUMBER,p_entity IN NUMBER,
    p_group IN NUMBER,p_context IN NUMBER,p_session IN VARCHAR2,
    o_cursor OUT SYS_REFCURSOR) IS
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    OPEN o_cursor FOR
      SELECT p.auditperiodid,p.description,p.start_date,p.end_date,
             p.status_id,s.status
        FROM T_AU_PERIOD p
        JOIN T_AU_PERIOD_STATUS s ON s.id=p.status_id
       ORDER BY p.auditperiodid ASC;
  END P_CATALOG;

  PROCEDURE P_RESULTS(
    p_user IN NUMBER,p_role IN NUMBER,p_entity IN NUMBER,
    p_group IN NUMBER,p_context IN NUMBER,p_session IN VARCHAR2,
    p_period IN NUMBER,p_type IN NUMBER,p_ent IN NUMBER,p_result IN NUMBER,
    o_current OUT SYS_REFCURSOR,o_cursor OUT SYS_REFCURSOR,
    o_detail OUT SYS_REFCURSOR,o_exceptions OUT SYS_REFCURSOR) IS
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    IF NVL(p_period,0)<=0 THEN
      RAISE_APPLICATION_ERROR(-20040,'Select an audit period.');
    END IF;
    OPEN o_current FOR
      SELECT g.period_id,g.eng_id,g.entity_id,6 entity_type_id,
             b.name entity_name,v.result_id,v.run_id,v.version_id,
             v.engine_version,v.observations,v.instances,v.score,
             v.risk_rating,NVL(v.status,'NOT_EXECUTED') status,v.calculated_on
        FROM T_AU_PLAN_ENG g
        JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
        LEFT JOIN V_FAD_RISK_ENGINE_CURRENT v
          ON v.period_id=g.period_id AND v.eng_id=g.eng_id
       WHERE g.period_id=p_period AND g.entity_type=6
         AND (p_ent IS NULL OR g.entity_id=p_ent)
       ORDER BY b.name,g.eng_id;

    OPEN o_cursor FOR
      SELECT v.*,u.processed,u.calculated,u.pending,u.skipped,u.failed
        FROM V_FAD_RISK_ENGINE_RESULTS v
        JOIN T_FAD_RISK_ENGINE_RUN u ON u.run_id=v.run_id
       WHERE v.period_id=p_period AND v.entity_type_id=6
         AND (p_ent IS NULL OR v.entity_id=p_ent)
       ORDER BY v.run_id DESC,v.eng_id;

    OPEN o_detail FOR
      SELECT d.* FROM T_FAD_RISK_ENGINE_DETAIL d
       WHERE d.result_id=p_result ORDER BY d.detail_id;

    OPEN o_exceptions FOR
      SELECT x.exception_id,x.result_id,x.code,x.message
        FROM T_FAD_RISK_ENGINE_EXCEPTION x
       WHERE x.result_id=p_result ORDER BY x.exception_id;
  END P_RESULTS;

END PKG_FAD_RISK_ENGINE;
/
SHOW ERRORS

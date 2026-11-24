-- FAD Risk Register execution: Super User only.
-- Follow IAS package conventions: direct SELECT/INSERT/UPDATE and ref cursors.
-- Caller owns COMMIT / ROLLBACK. No application-startup execution.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

CREATE OR REPLACE PACKAGE PKG_FAD_RISK_ENGINE AS
  PROCEDURE P_RUN_PERIOD(
    p_audit_period_id NUMBER, p_entity_type_id NUMBER, p_run_id OUT NUMBER,
    p_user NUMBER, p_role NUMBER, p_entity NUMBER,
    p_group NUMBER, p_context NUMBER, p_session VARCHAR2);

  PROCEDURE P_RUN_ENTITY(
    p_audit_period_id NUMBER, p_ent_id NUMBER, p_run_id OUT NUMBER,
    p_user NUMBER, p_role NUMBER, p_entity NUMBER,
    p_group NUMBER, p_context NUMBER, p_session VARCHAR2);

  PROCEDURE P_CATALOG(
    p_user NUMBER, p_role NUMBER, p_entity NUMBER,
    p_group NUMBER, p_context NUMBER, p_session VARCHAR2,
    o_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_RESULTS(
    p_user NUMBER, p_role NUMBER, p_entity NUMBER,
    p_group NUMBER, p_context NUMBER, p_session VARCHAR2,
    p_period NUMBER, p_type NUMBER, p_ent NUMBER, p_result NUMBER,
    o_current OUT SYS_REFCURSOR, o_cursor OUT SYS_REFCURSOR,
    o_detail OUT SYS_REFCURSOR, o_exceptions OUT SYS_REFCURSOR);
END PKG_FAD_RISK_ENGINE;
/

CREATE OR REPLACE PACKAGE BODY PKG_FAD_RISK_ENGINE AS

  -- A small database-side check is still needed: direct package calls must not
  -- be able to impersonate a Super User merely by passing p_role=1.
  -- The MVC RiskAction/PageKeyPermissionGuard enforces the exact API grant.
  PROCEDURE P_CHECK_SUPER_USER(
    p_user NUMBER, p_role NUMBER, p_entity NUMBER,
    p_group NUMBER, p_context NUMBER, p_session VARCHAR2) IS
    v_count NUMBER;
  BEGIN
    IF NVL(p_role,0)<>1 OR NVL(p_group,0)<>1
       OR p_session IS NULL OR NVL(p_entity,0)<=0 THEN
      RAISE_APPLICATION_ERROR(-20043,'Super User session required.');
    END IF;

    SELECT COUNT(*) INTO v_count
      FROM T_USER u
      JOIN T_USER_SESSION s ON s.user_pp_number=u.ppno
      JOIN T_GROUPS g ON g.group_id=s.group_id AND g.role_id=s.role_id
     WHERE u.userid=p_user AND u.isactive='Y'
       AND g.group_id=1 AND g.role_id=1 AND g.status='Y'
       AND s.session_id=p_session AND s.session_active='Y'
       AND s.logged_out_date IS NULL AND s.entity_id=p_entity
       AND ((p_context IS NOT NULL AND s.user_context_id=p_context
             AND EXISTS (
               SELECT 1 FROM T_USER_CONTEXT_ASSIGNMENT c
                WHERE c.user_context_id=p_context AND c.user_id=u.userid
                  AND c.role_id=1 AND c.entity_id=p_entity
                  AND NVL(c.group_id,c.role_id)=1 AND c.is_active='Y'
                  AND (c.effective_from IS NULL OR c.effective_from<=SYSDATE)
                  AND (c.effective_to IS NULL OR c.effective_to>=TRUNC(SYSDATE))))
         OR (p_context IS NULL AND s.user_context_id IS NULL
             AND u.entity_id=p_entity AND EXISTS (
               SELECT 1 FROM T_USER_MAPING m
                WHERE m.userid=u.userid AND m.role_id=1 AND m.group_id=1)));
    IF v_count<>1 THEN
      RAISE_APPLICATION_ERROR(-20043,'Super User session is not active.');
    END IF;
  END P_CHECK_SUPER_USER;

  -- Shared branch work. Period and single-entity procedures both call this
  -- routine, so the same ENG_ID produces the same counts and pending status.
  PROCEDURE P_PROCESS_ENGAGEMENT(
    p_run_id NUMBER, p_eng_id NUMBER, p_policy CLOB,
    p_version NUMBER, p_source_sha VARCHAR2) IS
    v_result_id NUMBER;
    v_entity_id NUMBER;
    v_entity_name VARCHAR2(500);
    v_entity_type NUMBER;
    v_eng_status NUMBER;
    v_count NUMBER;
    v_invalid NUMBER;
    v_instances NUMBER;
    v_rule VARCHAR2(30);
    v_message VARCHAR2(2000);
    v_technical VARCHAR2(2000);
  BEGIN
    SELECT g.entity_id,b.name,b.type_id,g.status
      INTO v_entity_id,v_entity_name,v_entity_type,v_eng_status
      FROM T_AU_PLAN_ENG g
      JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
     WHERE g.eng_id=p_eng_id;

    INSERT INTO T_FAD_RISK_ENGINE_RESULT
      (run_id,eng_id,entity_id,entity_name,entity_type_id,
       version_id,source_sha256,policy_snapshot,status)
    VALUES (p_run_id,p_eng_id,v_entity_id,v_entity_name,v_entity_type,
            p_version,p_source_sha,p_policy,'RUNNING')
    RETURNING result_id INTO v_result_id;

    SAVEPOINT fad_branch;

    SELECT COUNT(*) INTO v_count
      FROM JSON_TABLE(p_policy,'$[*]' COLUMNS(
        engagement_status NUMBER PATH '$.engagementStatus')) p
     WHERE p.engagement_status=v_eng_status;

    IF v_count=0 THEN
      UPDATE T_FAD_RISK_ENGINE_RESULT SET status='SKIPPED'
       WHERE result_id=v_result_id;
      INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
      VALUES(v_result_id,'INELIGIBLE',
             'Engagement is not at an approved completed-audit status.');
      RETURN;
    END IF;

    v_rule:=JSON_VALUE(p_policy,'$[0].referenceRule' RETURNING VARCHAR2(30));

    -- Record each qualifying final observation exactly once, using IAS ANNEX ID.
    INSERT INTO T_FAD_RISK_ENGINE_DETAIL
      (result_id,node_level,annex_id,observation_id,reference_id,
       observations,instances,status)
    SELECT v_result_id,'OBSERVATION',o.annex,o.id,TO_CHAR(o.reference_id),
           1,o.no_of_instances,
           CASE
             WHEN o.no_of_instances IS NULL OR o.no_of_instances<0
                  OR o.no_of_instances<>TRUNC(o.no_of_instances)
                  OR (v_rule='REQUIRE_UNIQUE' AND o.reference_id IS NOT NULL
                      AND COUNT(*) OVER(PARTITION BY o.reference_id)>1)
             THEN 'PENDING_INPUT' ELSE 'COUNTED'
           END
      FROM T_AU_OBSERVATION o
     WHERE o.engplanid=p_eng_id AND o.entity_id=v_entity_id
       AND EXISTS (
         SELECT 1 FROM JSON_TABLE(p_policy,'$[*]' COLUMNS(
           engagement_status NUMBER PATH '$.engagementStatus',
           observation_status NUMBER PATH '$.observationStatus')) p
         WHERE p.engagement_status=v_eng_status
           AND p.observation_status=o.status);

    -- Show all 121 approved Annexures, including those with zero observations.
    -- Do not group by source description because it can be a large text field.
    INSERT INTO T_FAD_RISK_ENGINE_DETAIL
      (result_id,node_level,main_code,package_code,process_code,procedure_code,
       source_row,annex_id,source_code,description,risk_classification,
       observations,instances,status)
    SELECT v_result_id,'ANNEXURE',v.main_code,v.package_code,
           v.process_code,v.procedure_code,v.excel_row,s.anex_id,
           v.annexure_code,v.violation_description,v.risk_classification,
           NVL(d.observations,0),
           CASE WHEN NVL(d.invalid_count,0)=0 THEN NVL(d.instances,0) END,
           'PENDING_INPUT'
      FROM V_FAD_RISK_APPROVED_SOURCE v
      JOIN T_FAD_RISK_MAP_STAGE s ON s.excel_row=v.excel_row
      LEFT JOIN (
        SELECT annex_id,COUNT(*) observations,
               SUM(instances) instances,
               COUNT(CASE WHEN status<>'COUNTED' THEN 1 END) invalid_count
          FROM T_FAD_RISK_ENGINE_DETAIL
         WHERE result_id=v_result_id AND node_level='OBSERVATION'
         GROUP BY annex_id
      ) d ON d.annex_id=s.anex_id;

    -- Aggregate up the approved source hierarchy. Counts are NOT risk scores.
    INSERT INTO T_FAD_RISK_ENGINE_DETAIL
      (result_id,node_level,main_code,package_code,process_code,procedure_code,
       observations,instances,status)
    SELECT v_result_id,
           CASE WHEN GROUPING(main_code)=1 THEN 'OVERALL'
                WHEN GROUPING(package_code)=1 THEN 'MAIN_KPI'
                WHEN GROUPING(process_code)=1 THEN 'PACKAGE'
                WHEN GROUPING(procedure_code)=1 THEN 'PROCESS'
                ELSE 'PROCEDURE' END,
           main_code,package_code,process_code,procedure_code,
           SUM(observations),
           CASE WHEN COUNT(*)=COUNT(instances) THEN SUM(instances) END,
           'PENDING_INPUT'
      FROM T_FAD_RISK_ENGINE_DETAIL
     WHERE result_id=v_result_id AND node_level='ANNEXURE'
     GROUP BY ROLLUP(main_code,package_code,process_code,procedure_code);

    SELECT COUNT(*),
           SUM(CASE WHEN status='COUNTED' THEN instances END),
           COUNT(CASE WHEN status<>'COUNTED'
                OR NOT EXISTS (SELECT 1 FROM T_FAD_RISK_MAP_STAGE s
                                WHERE s.anex_id=d.annex_id)
                THEN 1 END)
      INTO v_count,v_instances,v_invalid
      FROM T_FAD_RISK_ENGINE_DETAIL d
     WHERE result_id=v_result_id AND node_level='OBSERVATION';

    UPDATE T_FAD_RISK_ENGINE_RESULT
       SET observations=v_count,
           instances=CASE WHEN v_invalid=0 THEN NVL(v_instances,0) END,
           status='PENDING_INPUT',
           score=NULL,risk_rating=NULL
     WHERE result_id=v_result_id;

    IF v_invalid>0 THEN
      INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
      VALUES(v_result_id,'OBSERVATION_INPUT',
             'One or more observations have invalid instances, duplicate '||
             'references under the approved rule, or unmapped Annexure IDs.');
    END IF;

    IF v_count=0 THEN
      INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
      VALUES(v_result_id,'NO_FINAL_OBSERVATIONS',
             'No qualifying final observations were found; verify audit completeness.');
    END IF;

    -- There is no approved crosswalk from 14/121 source nodes to the existing
    -- 20/127 weighted calculator inputs. Never invent a numeric risk rating.
    INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message)
    VALUES(v_result_id,'SCORING_PENDING',
           'Approved scoring crosswalk, financial inputs and branch size are '||
           'required before calculating a numerical risk rating.');

  EXCEPTION
    WHEN OTHERS THEN
      v_message:=SUBSTR(SQLERRM,1,2000);
      v_technical:=SUBSTR(DBMS_UTILITY.FORMAT_ERROR_BACKTRACE,1,2000);
      IF v_result_id IS NULL THEN RAISE; END IF;
      ROLLBACK TO fad_branch;
      UPDATE T_FAD_RISK_ENGINE_RESULT SET status='FAILED'
       WHERE result_id=v_result_id;
      INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION
        (result_id,code,message,technical_details)
      VALUES(v_result_id,'EXECUTION_FAILED',
             'Engagement calculation failed; see protected technical details.',
             v_message||CHR(10)||v_technical);
  END P_PROCESS_ENGAGEMENT;

  -- Execute for all engagements of the selected entity type in one audit period.
  PROCEDURE P_RUN_PERIOD(
    p_audit_period_id NUMBER,p_entity_type_id NUMBER,p_run_id OUT NUMBER,
    p_user NUMBER,p_role NUMBER,p_entity NUMBER,
    p_group NUMBER,p_context NUMBER,p_session VARCHAR2) IS
    v_count NUMBER;
    v_unique NUMBER;
    v_matched NUMBER;
    v_policy_count NUMBER;
    v_policy_ok NUMBER;
    v_policy_versions NUMBER;
    v_rules NUMBER;
    v_policy_version VARCHAR2(100);
    v_policy CLOB;
    v_version NUMBER;
    v_source_sha VARCHAR2(64);
    v_cursor SYS_REFCURSOR;
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    IF NVL(p_audit_period_id,0)<=0 OR NVL(p_entity_type_id,0)<=0 THEN
      RAISE_APPLICATION_ERROR(-20040,'Audit period and entity type are required.');
    END IF;

    SELECT COUNT(*) INTO v_count
      FROM T_AU_PERIOD WHERE auditperiodid=p_audit_period_id;
    IF v_count<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Unknown audit period.'); END IF;

    SELECT COUNT(*) INTO v_count
      FROM T_AU_PLAN_ENG g JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
     WHERE g.period_id=p_audit_period_id AND b.type_id=p_entity_type_id;
    IF v_count=0 THEN RAISE_APPLICATION_ERROR(-20040,'No matching audited entities.'); END IF;

    SELECT COUNT(*),COUNT(DISTINCT policy_version),
           COUNT(DISTINCT reference_rule),
           COUNT(CASE WHEN approved_on IS NOT NULL
                        AND approved_on<=SYSTIMESTAMP
                        AND approval_ref IS NOT NULL
                        AND policy_version IS NOT NULL
                        AND reference_rule IN ('REQUIRE_UNIQUE','DISTINCT_OBSERVATIONS')
                      THEN 1 END),
           MAX(policy_version),
           JSON_ARRAYAGG(JSON_OBJECT(
             'engagementStatus' VALUE engagement_status,
             'observationStatus' VALUE observation_status,
             'referenceRule' VALUE reference_rule,
             'approval' VALUE approval_ref,
             'version' VALUE policy_version) RETURNING CLOB)
      INTO v_policy_count,v_policy_versions,v_rules,v_policy_ok,
           v_policy_version,v_policy
      FROM T_FAD_ENGINE_POLICY WHERE entity_type_id=p_entity_type_id;

    IF v_policy_count=0 OR v_policy_versions<>1
       OR v_rules<>1 OR v_policy_ok<>v_policy_count THEN
      RAISE_APPLICATION_ERROR(-20040,'Approved execution policy is incomplete.');
    END IF;

    -- Existing IAS package already verifies all 121 approved row fingerprints.
    PKG_FAD_RISK.P_GET_REGISTER(v_cursor);
    CLOSE v_cursor;
    SELECT COUNT(*),COUNT(DISTINCT s.anex_id),
           COUNT(CASE WHEN a.id IS NOT NULL AND
             REPLACE(UPPER(TRIM(a.code)),'-','')=
             REPLACE(UPPER(TRIM(s.annexure_code)),'-','')
             THEN 1 END),
           MAX(s.source_file_sha256)
      INTO v_count,v_unique,v_matched,v_source_sha
      FROM T_FAD_RISK_MAP_STAGE s
      LEFT JOIN T_AUDIT_CHECKLIST_ANNEXURE a ON a.id=s.anex_id;
    IF v_count<>121 OR v_unique<>121 OR v_matched<>121 THEN
      RAISE_APPLICATION_ERROR(-20101,'IAS Annexure ID mappings are incomplete or conflicting.');
    END IF;

    SELECT CASE WHEN COUNT(*)=1 THEN MIN(version_id) END
      INTO v_version FROM T_FAD_RISK_VERSION WHERE state='PUBLISHED';

    INSERT INTO T_FAD_RISK_ENGINE_RUN
      (period_id,entity_type_id,audit_office_id,actor_id,status,
       policy_snapshot,policy_version,snapshot_on,engine_version)
    VALUES (p_audit_period_id,p_entity_type_id,p_entity,p_user,'RUNNING',
            v_policy,v_policy_version,SYSTIMESTAMP,'FAD-ENGINE-3')
    RETURNING run_id INTO p_run_id;

    FOR eng IN (
      SELECT g.eng_id
        FROM T_AU_PLAN_ENG g
        JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
       WHERE g.period_id=p_audit_period_id AND b.type_id=p_entity_type_id
       ORDER BY g.eng_id
    ) LOOP
      P_PROCESS_ENGAGEMENT(p_run_id,eng.eng_id,v_policy,v_version,v_source_sha);
    END LOOP;

    UPDATE T_FAD_RISK_ENGINE_RUN
       SET status='COMPLETED',completed_on=SYSTIMESTAMP,
           processed=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                      WHERE run_id=p_run_id AND status IN ('PENDING_INPUT','CALCULATED')),
           calculated=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                       WHERE run_id=p_run_id AND status='CALCULATED'),
           pending=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                    WHERE run_id=p_run_id AND status='PENDING_INPUT'),
           skipped=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                    WHERE run_id=p_run_id AND status='SKIPPED'),
           failed=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                   WHERE run_id=p_run_id AND status='FAILED')
     WHERE run_id=p_run_id;
  END P_RUN_PERIOD;

  -- Execute for one audited entity. If the entity has multiple engagements
  -- in this audit period, retain one independent result per ENG_ID.
  PROCEDURE P_RUN_ENTITY(
    p_audit_period_id NUMBER,p_ent_id NUMBER,p_run_id OUT NUMBER,
    p_user NUMBER,p_role NUMBER,p_entity NUMBER,
    p_group NUMBER,p_context NUMBER,p_session VARCHAR2) IS
    v_type NUMBER;
    v_count NUMBER;
    v_unique NUMBER;
    v_matched NUMBER;
    v_policy_count NUMBER;
    v_policy_ok NUMBER;
    v_policy_versions NUMBER;
    v_rules NUMBER;
    v_policy_version VARCHAR2(100);
    v_policy CLOB;
    v_version NUMBER;
    v_source_sha VARCHAR2(64);
    v_cursor SYS_REFCURSOR;
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    IF NVL(p_audit_period_id,0)<=0 OR NVL(p_ent_id,0)<=0 THEN
      RAISE_APPLICATION_ERROR(-20040,'Audit period and entity ID are required.');
    END IF;

    SELECT COUNT(*),MAX(b.type_id) INTO v_count,v_type
      FROM T_AU_PLAN_ENG g JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
     WHERE g.period_id=p_audit_period_id AND g.entity_id=p_ent_id;
    IF v_count=0 OR v_type IS NULL THEN
      RAISE_APPLICATION_ERROR(-20040,'Entity has no engagement in this audit period.');
    END IF;

    SELECT COUNT(*),COUNT(DISTINCT policy_version),
           COUNT(DISTINCT reference_rule),
           COUNT(CASE WHEN approved_on IS NOT NULL
                        AND approved_on<=SYSTIMESTAMP
                        AND approval_ref IS NOT NULL
                        AND policy_version IS NOT NULL
                        AND reference_rule IN ('REQUIRE_UNIQUE','DISTINCT_OBSERVATIONS')
                      THEN 1 END),
           MAX(policy_version),
           JSON_ARRAYAGG(JSON_OBJECT(
             'engagementStatus' VALUE engagement_status,
             'observationStatus' VALUE observation_status,
             'referenceRule' VALUE reference_rule,
             'approval' VALUE approval_ref,
             'version' VALUE policy_version) RETURNING CLOB)
      INTO v_policy_count,v_policy_versions,v_rules,v_policy_ok,
           v_policy_version,v_policy
      FROM T_FAD_ENGINE_POLICY WHERE entity_type_id=v_type;

    IF v_policy_count=0 OR v_policy_versions<>1
       OR v_rules<>1 OR v_policy_ok<>v_policy_count THEN
      RAISE_APPLICATION_ERROR(-20040,'Approved execution policy is incomplete.');
    END IF;

    PKG_FAD_RISK.P_GET_REGISTER(v_cursor);
    CLOSE v_cursor;
    SELECT COUNT(*),COUNT(DISTINCT s.anex_id),
           COUNT(CASE WHEN a.id IS NOT NULL AND
             REPLACE(UPPER(TRIM(a.code)),'-','')=
             REPLACE(UPPER(TRIM(s.annexure_code)),'-','')
             THEN 1 END),
           MAX(s.source_file_sha256)
      INTO v_count,v_unique,v_matched,v_source_sha
      FROM T_FAD_RISK_MAP_STAGE s
      LEFT JOIN T_AUDIT_CHECKLIST_ANNEXURE a ON a.id=s.anex_id;
    IF v_count<>121 OR v_unique<>121 OR v_matched<>121 THEN
      RAISE_APPLICATION_ERROR(-20101,'IAS Annexure ID mappings are incomplete or conflicting.');
    END IF;

    SELECT CASE WHEN COUNT(*)=1 THEN MIN(version_id) END
      INTO v_version FROM T_FAD_RISK_VERSION WHERE state='PUBLISHED';

    INSERT INTO T_FAD_RISK_ENGINE_RUN
      (period_id,entity_id,entity_type_id,audit_office_id,actor_id,status,
       policy_snapshot,policy_version,snapshot_on,engine_version)
    VALUES (p_audit_period_id,p_ent_id,v_type,p_entity,p_user,'RUNNING',
            v_policy,v_policy_version,SYSTIMESTAMP,'FAD-ENGINE-3')
    RETURNING run_id INTO p_run_id;

    FOR eng IN (
      SELECT eng_id FROM T_AU_PLAN_ENG
       WHERE period_id=p_audit_period_id AND entity_id=p_ent_id
       ORDER BY eng_id
    ) LOOP
      P_PROCESS_ENGAGEMENT(p_run_id,eng.eng_id,v_policy,v_version,v_source_sha);
    END LOOP;

    UPDATE T_FAD_RISK_ENGINE_RUN
       SET status='COMPLETED',completed_on=SYSTIMESTAMP,
           processed=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                      WHERE run_id=p_run_id AND status IN ('PENDING_INPUT','CALCULATED')),
           calculated=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                       WHERE run_id=p_run_id AND status='CALCULATED'),
           pending=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                    WHERE run_id=p_run_id AND status='PENDING_INPUT'),
           skipped=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                    WHERE run_id=p_run_id AND status='SKIPPED'),
           failed=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT
                   WHERE run_id=p_run_id AND status='FAILED')
     WHERE run_id=p_run_id;
  END P_RUN_ENTITY;

  PROCEDURE P_CATALOG(
    p_user NUMBER,p_role NUMBER,p_entity NUMBER,
    p_group NUMBER,p_context NUMBER,p_session VARCHAR2,
    o_cursor OUT SYS_REFCURSOR) IS
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    OPEN o_cursor FOR
      SELECT DISTINCT g.period_id,b.type_id,b.entity_id,b.name
        FROM T_AU_PLAN_ENG g
        JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
       ORDER BY g.period_id DESC,b.name;
  END P_CATALOG;

  PROCEDURE P_RESULTS(
    p_user NUMBER,p_role NUMBER,p_entity NUMBER,
    p_group NUMBER,p_context NUMBER,p_session VARCHAR2,
    p_period NUMBER,p_type NUMBER,p_ent NUMBER,p_result NUMBER,
    o_current OUT SYS_REFCURSOR,o_cursor OUT SYS_REFCURSOR,
    o_detail OUT SYS_REFCURSOR,o_exceptions OUT SYS_REFCURSOR) IS
  BEGIN
    P_CHECK_SUPER_USER(p_user,p_role,p_entity,p_group,p_context,p_session);
    IF NVL(p_period,0)<=0 OR NVL(p_type,0)<0
       OR NVL(p_ent,0)<0 OR NVL(p_result,0)<0 THEN
      RAISE_APPLICATION_ERROR(-20040,'Invalid result selection.');
    END IF;

    OPEN o_current FOR
      SELECT g.period_id,g.eng_id,g.entity_id,b.type_id entity_type_id,
             b.name entity_name,v.result_id,v.run_id,v.version_id,
             v.engine_version,v.observations,v.instances,v.score,
             v.risk_rating,NVL(v.status,'NOT_EXECUTED') status,v.calculated_on
        FROM T_AU_PLAN_ENG g
        JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id
        LEFT JOIN V_FAD_RISK_ENGINE_CURRENT v
          ON v.period_id=g.period_id AND v.eng_id=g.eng_id
       WHERE g.period_id=p_period
         AND (p_type IS NULL OR b.type_id=p_type)
         AND (p_ent IS NULL OR g.entity_id=p_ent)
       ORDER BY g.eng_id;

    OPEN o_cursor FOR
      SELECT v.*,u.processed,u.calculated,u.skipped,u.pending,u.failed
        FROM V_FAD_RISK_ENGINE_RESULTS v
        JOIN T_FAD_RISK_ENGINE_RUN u ON u.run_id=v.run_id
       WHERE v.period_id=p_period
         AND (p_type IS NULL OR v.entity_type_id=p_type)
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

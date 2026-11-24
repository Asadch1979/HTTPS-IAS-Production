WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
CREATE OR REPLACE PACKAGE PKG_FAD_RISK_ENGINE AS
 PROCEDURE P_RUN_PERIOD(p_audit_period_id NUMBER,p_entity_type_id NUMBER,p_run_id OUT NUMBER,p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2);
 PROCEDURE P_RUN_ENTITY(p_audit_period_id NUMBER,p_ent_id NUMBER,p_run_id OUT NUMBER,p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2);
 PROCEDURE P_CATALOG(p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2,o_cursor OUT SYS_REFCURSOR);
 PROCEDURE P_RESULTS(p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2,p_period NUMBER,p_type NUMBER,p_ent NUMBER,p_result NUMBER,o_cursor OUT SYS_REFCURSOR,o_detail OUT SYS_REFCURSOR,o_exceptions OUT SYS_REFCURSOR);
END;
/
CREATE OR REPLACE PACKAGE BODY PKG_FAD_RISK_ENGINE AS
 PROCEDURE authorize(p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2,p_operation VARCHAR2,p_eng NUMBER DEFAULT NULL) IS
  n NUMBER; api_action VARCHAR2(40); verb VARCHAR2(5);
 BEGIN
  IF p_operation IS NULL OR p_operation NOT IN ('READ','CATALOG','SAVE','SUBMIT') THEN
   RAISE_APPLICATION_ERROR(-20043,'Unsupported assessment operation.'); END IF;
  api_action:=CASE p_operation WHEN 'READ' THEN 'EngineResults' WHEN 'CATALOG' THEN 'EngineCatalog'
    WHEN 'SAVE' THEN 'RunEnginePeriod' ELSE 'RunEngineEntity' END;
  verb:=CASE WHEN p_operation IN ('READ','CATALOG') THEN 'GET' ELSE 'POST' END;
  SELECT COUNT(*) INTO n FROM t_user u WHERE u.userid=p_user AND u.isactive='Y'
   AND EXISTS(SELECT 1 FROM t_user_session s WHERE s.user_pp_number=u.ppno AND s.session_id=p_session
    AND s.session_active='Y' AND s.logged_out_date IS NULL AND s.role_id=p_role
    AND s.entity_id=p_entity AND s.group_id=p_group
    AND ((p_context IS NULL AND s.user_context_id IS NULL)
      OR (p_context IS NOT NULL AND s.user_context_id=p_context)))
   AND EXISTS(SELECT 1 FROM t_groups g WHERE g.group_id=p_group AND g.role_id=p_role AND g.status='Y')
   AND ((p_context IS NOT NULL AND EXISTS(SELECT 1 FROM t_user_context_assignment c
     WHERE c.user_context_id=p_context AND c.user_id=u.userid AND c.role_id=p_role
      AND c.entity_id=p_entity AND NVL(c.group_id,c.role_id)=p_group AND c.is_active='Y'
      AND (c.effective_from IS NULL OR c.effective_from<=SYSDATE)
      AND (c.effective_to IS NULL OR c.effective_to>=TRUNC(SYSDATE))))
    OR (p_context IS NULL AND u.entity_id=p_entity AND EXISTS(SELECT 1 FROM t_user_maping m
     WHERE m.userid=u.userid AND m.role_id=p_role AND m.group_id=p_group)))
   AND EXISTS(SELECT 1 FROM t_au_api_master a JOIN t_au_role_api_permission r ON r.api_id=a.api_id
     JOIN t_menu_pages mp ON mp.id=a.page_id JOIN t_menu_pages_groupmap pg ON pg.page_id=mp.id
     WHERE r.role_id=p_role AND r.is_active='Y' AND a.is_active='Y' AND mp.status='A' AND pg.group_id=p_group
      AND LOWER(TRIM(BOTH '/' FROM a.api_path))=LOWER('FADRiskControlPanel/'||api_action)
      AND UPPER(a.http_method)=verb)
   AND EXISTS(SELECT 1 FROM t_fad_risk_access x WHERE x.group_id=p_group AND x.audit_entity_id=p_entity
    AND ((p_operation IN ('READ','CATALOG') AND (x.can_prepare='Y' OR x.can_review='Y'))
     OR (p_operation IN ('SAVE','SUBMIT') AND x.can_prepare='Y')
     OR (p_operation IN ('APPROVE','RETURN') AND x.can_review='Y')));
  IF n<>1 THEN RAISE_APPLICATION_ERROR(-20043,'Risk assessment access denied.'); END IF;
  IF p_eng IS NOT NULL THEN
   SELECT COUNT(*) INTO n FROM t_au_plan_eng WHERE eng_id=p_eng AND auditby_id=p_entity;
   IF n<>1 THEN RAISE_APPLICATION_ERROR(-20043,'Engagement outside authorized audit office.'); END IF;
  END IF;
 END;


 PROCEDURE issue(r NUMBER,c VARCHAR2,m VARCHAR2) IS BEGIN
 INSERT INTO T_FAD_RISK_ENGINE_EXCEPTION(result_id,code,message) VALUES(r,c,m); END;
 PROCEDURE calculate(r NUMBER,e NUMBER) IS
 n NUMBER; cur SYS_REFCURSOR; ver NUMBER;
 BEGIN
 -- Reuse the existing approved SHA and row fingerprint validation. No bypass.
 PKG_FAD_RISK.P_GET_REGISTER(cur); CLOSE cur;
 SELECT CASE WHEN COUNT(*)=1 THEN MIN(version_id) END INTO ver FROM T_FAD_RISK_VERSION WHERE state='PUBLISHED';
 UPDATE T_FAD_RISK_ENGINE_RESULT SET version_id=ver WHERE result_id=r;
 IF ver IS NULL THEN issue(r,'SCORING_VERSION','A unique published scoring version is unavailable.'); END IF;
 SELECT COUNT(*) INTO n FROM (SELECT DISTINCT main_code FROM V_FAD_RISK_REGISTER);
 IF n<>4 THEN RAISE_APPLICATION_ERROR(-20101,'Main KPI contract mismatch'); END IF;
 SELECT COUNT(*) INTO n FROM (SELECT DISTINCT package_code FROM V_FAD_RISK_REGISTER);
 IF n<>9 THEN RAISE_APPLICATION_ERROR(-20101,'Package contract mismatch'); END IF;
 SELECT COUNT(*) INTO n FROM (SELECT DISTINCT process_code FROM V_FAD_RISK_REGISTER);
 IF n<>14 THEN RAISE_APPLICATION_ERROR(-20101,'Process contract mismatch'); END IF;
 SELECT COUNT(*) INTO n FROM (SELECT DISTINCT procedure_code FROM V_FAD_RISK_REGISTER);
 IF n<>37 THEN RAISE_APPLICATION_ERROR(-20101,'Procedure contract mismatch'); END IF;
 -- One observation record per ID; never join text/evidence/reference tables that multiply counts.
 INSERT INTO T_FAD_RISK_ENGINE_DETAIL(result_id,node_level,annex_id,observation_id,reference_id,observations,instances,status)
 SELECT r,'OBSERVATION',o.annex,o.id,TO_CHAR(o.reference_id),1,o.no_of_instances,
 CASE WHEN o.no_of_instances IS NULL OR o.no_of_instances<0 OR o.no_of_instances<>TRUNC(o.no_of_instances)
 OR (o.reference_id IS NOT NULL AND COUNT(*) OVER(PARTITION BY o.reference_id)>1)
 THEN 'PENDING_INPUT' ELSE 'COUNTED' END
 FROM t_au_observation o JOIN t_au_plan_eng g ON g.eng_id=o.engplanid
 JOIN t_auditee_entities b ON b.entity_id=g.entity_id
 WHERE o.engplanid=e AND o.entity_id=g.entity_id AND EXISTS
 (SELECT 1 FROM T_FAD_ENGINE_POLICY p WHERE p.entity_type_id=b.type_id
 AND p.engagement_status=g.status AND p.observation_status=o.status);
 FOR s IN (SELECT v.*,s.anex_id mapped_id FROM V_FAD_RISK_REGISTER v
 JOIN T_FAD_RISK_MAP_STAGE s ON s.excel_row=v.excel_row) LOOP
 SELECT COUNT(*) INTO n FROM T_AUDIT_CHECKLIST_ANNEXURE a WHERE a.id=s.mapped_id;
 IF n<>1 OR s.mapped_id IS NULL THEN issue(r,'ANNEX_MAPPING','Missing or ambiguous IAS mapping for source row '||s.excel_row); END IF;
 INSERT INTO T_FAD_RISK_ENGINE_DETAIL(result_id,node_level,main_code,package_code,process_code,procedure_code,
 source_row,annex_id,source_code,description,risk_classification,observations,instances,status)
 SELECT r,'ANNEXURE',s.main_code,s.package_code,s.process_code,s.procedure_code,s.excel_row,s.mapped_id,
 s.source_annexure_code,s.violation_description,s.risk_classification,COUNT(*),
 CASE WHEN n=1 AND COUNT(CASE WHEN status='PENDING_INPUT' THEN 1 END)=0 THEN NVL(SUM(instances),0) END,'PENDING_INPUT'
 FROM T_FAD_RISK_ENGINE_DETAIL WHERE result_id=r AND node_level='OBSERVATION' AND annex_id=s.mapped_id;
 END LOOP;
 SELECT COUNT(*) INTO n FROM (SELECT anex_id FROM T_FAD_RISK_MAP_STAGE WHERE anex_id IS NOT NULL GROUP BY anex_id HAVING COUNT(*)>1);
 IF n>0 THEN issue(r,'DUPLICATE_MAPPING','Multiple source rows reference one IAS Annexure; aggregates withheld.');
 UPDATE T_FAD_RISK_ENGINE_DETAIL SET instances=NULL WHERE result_id=r AND node_level='ANNEXURE'; END IF;
 SELECT COUNT(*) INTO n FROM T_FAD_RISK_ENGINE_DETAIL d WHERE d.result_id=r AND d.node_level='OBSERVATION'
 AND (d.status='PENDING_INPUT' OR NOT EXISTS(SELECT 1 FROM T_FAD_RISK_MAP_STAGE s WHERE s.anex_id=d.annex_id));
 IF n>0 THEN issue(r,'OBSERVATION_INPUT','Repeated references, invalid instance counts or unmapped final observations require review.'); END IF;
 INSERT INTO T_FAD_RISK_ENGINE_DETAIL(result_id,node_level,main_code,package_code,process_code,procedure_code,observations,instances,status)
 SELECT r,CASE WHEN GROUPING(main_code)=1 THEN 'OVERALL' WHEN GROUPING(package_code)=1 THEN 'MAIN_KPI'
 WHEN GROUPING(process_code)=1 THEN 'PACKAGE' WHEN GROUPING(procedure_code)=1 THEN 'PROCESS' ELSE 'PROCEDURE' END,
 main_code,package_code,process_code,procedure_code,SUM(observations),
 CASE WHEN COUNT(*)=COUNT(instances) THEN SUM(instances) END,'PENDING_INPUT'
 FROM T_FAD_RISK_ENGINE_DETAIL WHERE result_id=r AND node_level='ANNEXURE'
 GROUP BY ROLLUP(main_code,package_code,process_code,procedure_code);
 issue(r,'SCORING_RECONCILIATION','14 source processes/121 Annexures are not an approved crosswalk to 20 weighted processes/127 inputs. Financial denominators, branch characteristics and evidence must be reconciled before final scoring.');
 UPDATE T_FAD_RISK_ENGINE_RESULT SET observations=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_DETAIL WHERE result_id=r AND node_level='OBSERVATION'),
 instances=(SELECT CASE WHEN COUNT(*)=COUNT(CASE WHEN status='COUNTED' THEN 1 END) THEN NVL(SUM(instances),0) END
 FROM T_FAD_RISK_ENGINE_DETAIL WHERE result_id=r AND node_level='OBSERVATION'),
 source_sha256='9e5f375ba0a8df2339edc3374307aa049ac2975580ea0f206d31bbb8090b2425',status='PENDING_INPUT'
 WHERE result_id=r;
 END;
 PROCEDURE run_common(period NUMBER,typ NUMBER,ent NUMBER,runid OUT NUMBER,p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2,op VARCHAR2) IS
 n NUMBER; rid NUMBER; locked NUMBER; err NUMBER; diagnostics VARCHAR2(2000); policies CLOB;
 BEGIN
 authorize(p_user,p_role,p_entity,p_group,p_context,p_session,op);
 IF period IS NULL OR period<=0 OR (typ IS NULL AND ent IS NULL) THEN RAISE_APPLICATION_ERROR(-20040,'Invalid selection'); END IF;
 SELECT COUNT(*) INTO n FROM t_au_period WHERE auditperiodid=period;
 IF n<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Unknown period'); END IF;
 SELECT COUNT(*) INTO n FROM T_FAD_ENGINE_POLICY WHERE (typ IS NULL OR entity_type_id=typ);
 IF n=0 THEN RAISE_APPLICATION_ERROR(-20040,'Approved completion and final-observation policy required'); END IF;
 SELECT JSON_ARRAYAGG(JSON_OBJECT('id' VALUE policy_id,'approval' VALUE approval_ref,'type' VALUE entity_type_id,
 'engagementStatus' VALUE engagement_status,'observationStatus' VALUE observation_status) RETURNING CLOB) INTO policies FROM T_FAD_ENGINE_POLICY;
 SELECT COUNT(*) INTO n FROM t_au_plan_eng g JOIN t_auditee_entities b ON b.entity_id=g.entity_id
 WHERE g.period_id=period AND g.auditby_id=p_entity AND (ent IS NULL OR g.entity_id=ent) AND (typ IS NULL OR b.type_id=typ);
 IF n=0 THEN RAISE_APPLICATION_ERROR(-20040,'No engagements in selected authorized scope'); END IF;
 INSERT INTO T_FAD_RISK_ENGINE_RUN(period_id,entity_type_id,entity_id,audit_office_id,actor_id,status)
 VALUES(period,typ,ent,p_entity,p_user,'RUNNING') RETURNING run_id INTO runid;
 FOR g IN (SELECT g.eng_id,g.entity_id,g.status,b.name,b.type_id FROM t_au_plan_eng g JOIN t_auditee_entities b ON b.entity_id=g.entity_id
 WHERE g.period_id=period AND g.auditby_id=p_entity AND (typ IS NULL OR b.type_id=typ) AND (ent IS NULL OR g.entity_id=ent) ORDER BY g.eng_id) LOOP
 INSERT INTO T_FAD_RISK_ENGINE_RESULT(run_id,eng_id,entity_id,entity_name,entity_type_id,status,policy_snapshot)
 VALUES(runid,g.eng_id,g.entity_id,g.name,g.type_id,'RUNNING',policies) RETURNING result_id INTO rid;
 SELECT COUNT(*) INTO n FROM T_FAD_ENGINE_POLICY WHERE entity_type_id=g.type_id AND engagement_status=g.status;
 IF n=0 THEN
 UPDATE T_FAD_RISK_ENGINE_RESULT SET status='SKIPPED' WHERE result_id=rid;
 issue(rid,'INELIGIBLE','Engagement has not reached an approved completion stage.');
 ELSE
 SAVEPOINT branch_start;
 BEGIN
 -- Lock rows are provisioned before execution; NOWAIT prevents overlap without changing IAS engagements.
 SELECT eng_id INTO locked FROM T_FAD_ENGINE_LOCK WHERE eng_id=g.eng_id FOR UPDATE NOWAIT;
 calculate(rid,g.eng_id);
 EXCEPTION WHEN OTHERS THEN
 err:=SQLCODE; diagnostics:=SUBSTR(DBMS_UTILITY.FORMAT_ERROR_STACK||CHR(10)||DBMS_UTILITY.FORMAT_ERROR_BACKTRACE,1,2000);
 ROLLBACK TO branch_start;
 UPDATE T_FAD_RISK_ENGINE_RESULT SET status=CASE WHEN err=-54 THEN 'SKIPPED' ELSE 'FAILED' END WHERE result_id=rid;
 issue(rid,'EXECUTION_FAILED',CASE WHEN err=-54 THEN 'Engagement is already being executed.' WHEN err=100 THEN 'Engine lock row is missing; administrator provisioning required.' ELSE 'Calculation failed; contact the administrator with the run and engagement IDs.' END);
 UPDATE T_FAD_RISK_ENGINE_EXCEPTION SET technical_details=diagnostics WHERE result_id=rid AND code='EXECUTION_FAILED';
 END;
 END IF;
 END LOOP;
 UPDATE T_FAD_RISK_ENGINE_RUN SET status='COMPLETED',completed_on=SYSTIMESTAMP,
 processed=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT WHERE run_id=runid AND status IN ('PENDING_INPUT','CALCULATED')),
 pending=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT WHERE run_id=runid AND status='PENDING_INPUT'),
 skipped=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT WHERE run_id=runid AND status='SKIPPED'),
 failed=(SELECT COUNT(*) FROM T_FAD_RISK_ENGINE_RESULT WHERE run_id=runid AND status='FAILED') WHERE run_id=runid;
 END;
 PROCEDURE P_RUN_PERIOD(p_audit_period_id NUMBER,p_entity_type_id NUMBER,p_run_id OUT NUMBER,p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2) IS BEGIN
 IF p_entity_type_id IS NULL OR p_entity_type_id<=0 THEN RAISE_APPLICATION_ERROR(-20040,'Entity type required'); END IF;
 run_common(p_audit_period_id,p_entity_type_id,NULL,p_run_id,p_user,p_role,p_entity,p_group,p_context,p_session,'SAVE'); END;
 PROCEDURE P_RUN_ENTITY(p_audit_period_id NUMBER,p_ent_id NUMBER,p_run_id OUT NUMBER,p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2) IS BEGIN
 IF p_ent_id IS NULL OR p_ent_id<=0 THEN RAISE_APPLICATION_ERROR(-20040,'Entity required'); END IF;
 run_common(p_audit_period_id,NULL,p_ent_id,p_run_id,p_user,p_role,p_entity,p_group,p_context,p_session,'SUBMIT'); END;
 PROCEDURE P_CATALOG(p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2,o_cursor OUT SYS_REFCURSOR) IS BEGIN
 authorize(p_user,p_role,p_entity,p_group,p_context,p_session,'CATALOG');
 OPEN o_cursor FOR SELECT DISTINCT g.period_id,b.type_id,b.entity_id,b.name FROM t_au_plan_eng g
 JOIN t_auditee_entities b ON b.entity_id=g.entity_id WHERE g.auditby_id=p_entity ORDER BY g.period_id DESC,b.name;
 END;
 PROCEDURE P_RESULTS(p_user NUMBER,p_role NUMBER,p_entity NUMBER,p_group NUMBER,p_context NUMBER,p_session VARCHAR2,p_period NUMBER,p_type NUMBER,p_ent NUMBER,p_result NUMBER,o_cursor OUT SYS_REFCURSOR,o_detail OUT SYS_REFCURSOR,o_exceptions OUT SYS_REFCURSOR) IS BEGIN
 authorize(p_user,p_role,p_entity,p_group,p_context,p_session,'READ');
 OPEN o_cursor FOR SELECT v.*,u.processed,u.skipped,u.pending,u.failed FROM V_FAD_RISK_ENGINE_RESULTS v
 JOIN T_FAD_RISK_ENGINE_RUN u ON u.run_id=v.run_id WHERE v.audit_office_id=p_entity AND v.period_id=p_period
 AND (p_type IS NULL OR v.entity_type_id=p_type) AND (p_ent IS NULL OR v.entity_id=p_ent) ORDER BY v.run_id DESC,v.eng_id;
 OPEN o_detail FOR SELECT d.* FROM T_FAD_RISK_ENGINE_DETAIL d JOIN V_FAD_RISK_ENGINE_RESULTS v ON v.result_id=d.result_id
 WHERE v.audit_office_id=p_entity AND v.result_id=p_result ORDER BY d.detail_id;
 OPEN o_exceptions FOR SELECT x.exception_id,x.result_id,x.code,x.message FROM T_FAD_RISK_ENGINE_EXCEPTION x JOIN V_FAD_RISK_ENGINE_RESULTS v ON v.result_id=x.result_id
 WHERE v.audit_office_id=p_entity AND v.result_id=p_result ORDER BY x.exception_id;
 END;
END;
/
SHOW ERRORS

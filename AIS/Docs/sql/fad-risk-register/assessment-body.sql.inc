 PROCEDURE authorize(@CTX@,p_operation VARCHAR2,p_eng NUMBER DEFAULT NULL) IS
  n NUMBER; api_action VARCHAR2(40); verb VARCHAR2(5);
 BEGIN
  IF p_operation IS NULL OR p_operation NOT IN ('READ','CATALOG','SAVE','SUBMIT','APPROVE','RETURN') THEN
   RAISE_APPLICATION_ERROR(-20043,'Unsupported assessment operation.'); END IF;
  api_action:=CASE p_operation WHEN 'READ' THEN 'ReadAssessment' WHEN 'CATALOG' THEN 'AssessmentCatalog'
    WHEN 'SAVE' THEN 'SaveAssessment' ELSE 'TransitionAssessment' END;
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

 @P_CATALOG@ IS
 BEGIN
  authorize(@ARGS@,'CATALOG');
  OPEN o_versions FOR SELECT version_id,version_name,source_sha256,calculator,published_on
   FROM t_fad_risk_version WHERE state='PUBLISHED' ORDER BY version_id DESC;
  OPEN o_assessments FOR SELECT a.assessment_id,a.eng_id,a.branch_name,a.audit_period,a.version_id,
   a.status,a.revision,a.score,a.rating,a.preparer_id,a.reviewer_id,a.updated_on
   FROM t_fad_risk_assessment a JOIN t_au_plan_eng e ON e.eng_id=a.eng_id
   WHERE e.auditby_id=p_entity ORDER BY a.updated_on DESC;
 END;

 @P_READ@ IS
  v NUMBER; n NUMBER;
 BEGIN
  authorize(@ARGS@,'READ',p_eng);
  IF p_id>0 THEN
   SELECT version_id INTO v FROM t_fad_risk_assessment WHERE assessment_id=p_id AND eng_id=p_eng;
  ELSE
   SELECT version_id INTO v FROM t_fad_risk_version WHERE version_id=p_version AND state='PUBLISHED';
   SELECT COUNT(*) INTO n FROM t_au_plan_eng WHERE eng_id=p_eng AND status BETWEEN 10 AND 13;
   IF n<>1 THEN RAISE_APPLICATION_ERROR(-20043,'Engagement is not open for preparation.'); END IF;
  END IF;
  OPEN o_header FOR
   SELECT a.assessment_id,e.eng_id,e.entity_id,NVL(a.branch_name,b.description) branch_name,
    NVL(a.audit_period,TO_CHAR(e.audit_startdate,'YYYY-MM-DD')||' - '||TO_CHAR(e.audit_enddate,'YYYY-MM-DD')) audit_period,
    a.branch_size,a.portfolio,a.size_reason,a.status,a.revision,a.preparer_id,a.reviewer_id,
    a.score,a.rating,a.fraud_count,v version_id,c.version_name,c.source_sha256,c.calculator,
    (SELECT login_name FROM t_user WHERE userid=a.preparer_id) preparer_name,
    (SELECT login_name FROM t_user WHERE userid=a.reviewer_id) reviewer_name
   FROM t_au_plan_eng e JOIN t_auditee_entities b ON b.entity_id=e.entity_id
    JOIN t_fad_risk_version c ON c.version_id=v
    LEFT JOIN t_fad_risk_assessment a ON a.assessment_id=p_id AND a.eng_id=e.eng_id WHERE e.eng_id=p_eng;
  OPEN o_details FOR
   WITH items AS (
    SELECT code item_code,sub_code,description,risk_level FROM t_fad_risk_annex WHERE version_id=v
    UNION ALL SELECT s.sub_code,s.sub_code,s.description,CAST(NULL AS VARCHAR2(20)) FROM t_fad_risk_subprocess s
     WHERE s.version_id=v AND s.criterion IN ('movement','shortfall','profit'))
   SELECT i.item_code,i.description,i.risk_level,s.sub_code,s.gravity,s.criterion,p.process_code,
    p.main_kpi,p.sub_kpi,p.description process_name,p.weight,p.denominator,
    d.instances,d.base_value,d.actual_value,d.observation,d.evidence
   FROM items i JOIN t_fad_risk_subprocess s ON s.version_id=v AND s.sub_code=i.sub_code
    JOIN t_fad_risk_process p ON p.version_id=v AND p.process_code=s.process_code
    LEFT JOIN t_fad_risk_detail d ON d.assessment_id=p_id AND d.item_code=i.item_code
   ORDER BY p.main_kpi,p.sub_kpi,p.process_code,s.sub_code,i.item_code;
  OPEN o_results FOR SELECT r.*,p.description FROM t_fad_risk_result r
   JOIN t_fad_risk_process p ON p.version_id=v AND p.process_code=r.process_code WHERE r.assessment_id=p_id ORDER BY r.process_code;
  OPEN o_history FOR SELECT history_id,revision,action,actor_id,recorded_on,remarks,
   input_snapshot,result_snapshot FROM t_fad_risk_history WHERE assessment_id=p_id ORDER BY history_id DESC;
 EXCEPTION WHEN NO_DATA_FOUND THEN RAISE_APPLICATION_ERROR(-20043,'Assessment or configuration unavailable.');
 END;

 PROCEDURE validate_evidence(p_eng NUMBER,p_entity NUMBER,kind VARCHAR2,obs VARCHAR2,ev VARCHAR2) IS
  n NUMBER;
 BEGIN
  IF obs IS NULL AND ev IS NULL THEN RETURN; END IF;
  IF kind IN ('movement','shortfall','profit') AND obs IS NULL AND REGEXP_LIKE(ev,'^WP:[0-9]{1,18}$') THEN
   -- Optional IAS V2 working-paper evidence source. Bound values only.
   EXECUTE IMMEDIATE 'SELECT COUNT(*) FROM t_wp_evidence_link f JOIN t_wp_header h ON h.wp_id=f.wp_id
    JOIN t_au_plan_eng e ON e.eng_id=h.eng_id WHERE f.evidence_link_id=:id
    AND h.eng_id=:eng AND h.entity_id=e.entity_id AND e.auditby_id=:office'
    INTO n USING TO_NUMBER(SUBSTR(ev,4)),p_eng,p_entity;
  ELSE
   IF obs IS NULL OR ev IS NULL OR NOT REGEXP_LIKE(obs,'^[0-9]{1,18}$') OR NOT REGEXP_LIKE(ev,'^[0-9]{1,18}$') THEN
    RAISE_APPLICATION_ERROR(-20040,'Invalid evidence source or observation reference.'); END IF;
   SELECT COUNT(*) INTO n FROM t_au_observation o JOIN t_au_observation_evidences f ON f.memoid=o.id
    JOIN t_au_plan_eng e ON e.eng_id=o.engplanid WHERE o.id=TO_NUMBER(obs) AND f.id=TO_NUMBER(ev)
     AND o.engplanid=p_eng AND o.entity_id=e.entity_id AND e.auditby_id=p_entity;
  END IF;
  IF n<>1 THEN RAISE_APPLICATION_ERROR(-20043,'Evidence outside authorized engagement.'); END IF;
 END;

 PROCEDURE require_complete(p_id NUMBER) IS
  a t_fad_risk_assessment%ROWTYPE; expected NUMBER; n NUMBER; total NUMBER; office NUMBER;
 BEGIN
  SELECT * INTO a FROM t_fad_risk_assessment WHERE assessment_id=p_id;
  SELECT auditby_id INTO office FROM t_au_plan_eng WHERE eng_id=a.eng_id AND entity_id=a.entity_id;
  SELECT COUNT(*) INTO expected FROM (
   SELECT code,sub_code FROM t_fad_risk_annex WHERE version_id=a.version_id
   UNION ALL SELECT sub_code,sub_code FROM t_fad_risk_subprocess WHERE version_id=a.version_id AND criterion IN ('movement','shortfall','profit'));
  SELECT COUNT(*) INTO n FROM t_fad_risk_detail WHERE assessment_id=p_id;
  IF n<>expected OR expected<>127 THEN RAISE_APPLICATION_ERROR(-20040,'Incomplete assessment details.'); END IF;
  SELECT COUNT(*) INTO n FROM t_fad_risk_detail d WHERE d.assessment_id=p_id AND
   (d.version_id<>a.version_id OR NOT EXISTS(SELECT 1 FROM (
    SELECT code,sub_code FROM t_fad_risk_annex WHERE version_id=a.version_id
    UNION ALL SELECT sub_code,sub_code FROM t_fad_risk_subprocess WHERE version_id=a.version_id AND criterion IN ('movement','shortfall','profit')) i
    WHERE i.code=d.item_code AND i.sub_code=d.sub_code));
  IF n<>0 THEN RAISE_APPLICATION_ERROR(-20040,'Invalid detail mapping.'); END IF;
  FOR d IN (SELECT d.*,s.criterion FROM t_fad_risk_detail d JOIN t_fad_risk_subprocess s
   ON s.version_id=d.version_id AND s.sub_code=d.sub_code WHERE d.assessment_id=p_id) LOOP
   IF d.criterion IN ('instances','fraud_override') AND
    (d.instances IS NULL OR d.instances<0 OR d.instances<>TRUNC(d.instances) OR (d.instances>0 AND d.observation IS NULL)) THEN
    RAISE_APPLICATION_ERROR(-20040,'Invalid stored instance input.'); END IF;
   IF d.criterion IN ('movement','shortfall') AND
    (d.base_value IS NULL OR d.base_value<=0 OR d.actual_value IS NULL OR d.actual_value<0) THEN
    RAISE_APPLICATION_ERROR(-20040,'Invalid stored financial input.'); END IF;
   IF d.criterion='profit' AND d.actual_value IS NULL THEN RAISE_APPLICATION_ERROR(-20040,'Missing profitability input.'); END IF;
   IF (d.criterion IN ('movement','shortfall','profit') OR d.instances>0) AND d.evidence IS NULL THEN
    RAISE_APPLICATION_ERROR(-20040,'Supporting evidence required.'); END IF;
   validate_evidence(a.eng_id,office,d.criterion,d.observation,d.evidence);
  END LOOP;
  SELECT COUNT(*),SUM(r.capped_marks) INTO n,total FROM t_fad_risk_result r JOIN t_fad_risk_process p
   ON p.version_id=a.version_id AND p.process_code=r.process_code WHERE r.assessment_id=p_id
   AND r.weight=p.weight AND r.denominator=p.denominator
   AND r.capped_marks=PKG_FAD_RISK_SCORE_V1.normalized_marks(r.raw_marks,p.weight,p.denominator);
  IF n<>20 OR a.score IS NULL OR total<>a.score OR a.rating IS NULL OR a.fraud_count IS NULL
   OR a.rating<>PKG_FAD_RISK_SCORE_V1.final_rating(a.score,a.branch_size,a.fraud_count) THEN
   RAISE_APPLICATION_ERROR(-20040,'Incomplete or invalid stored calculation.'); END IF;
  SELECT COUNT(*) INTO n FROM t_fad_risk_result WHERE assessment_id=p_id;
  IF n<>20 THEN RAISE_APPLICATION_ERROR(-20040,'Invalid process result count.'); END IF;
 END;

 PROCEDURE calculate(p_id NUMBER) IS
  total NUMBER:=0; fraud NUMBER:=0; raw NUMBER; marks NUMBER; v NUMBER; size_name VARCHAR2(10);
 BEGIN
  SELECT version_id,branch_size INTO v,size_name FROM t_fad_risk_assessment WHERE assessment_id=p_id;
  DELETE FROM t_fad_risk_result WHERE assessment_id=p_id;
  FOR p IN (SELECT * FROM t_fad_risk_process WHERE version_id=v) LOOP
   raw:=0;
   FOR d IN (SELECT d.*,s.gravity,s.criterion FROM t_fad_risk_detail d
    JOIN t_fad_risk_subprocess s ON s.version_id=d.version_id AND s.sub_code=d.sub_code
    WHERE d.assessment_id=p_id AND s.process_code=p.process_code) LOOP
    CASE d.criterion
     WHEN 'instances' THEN raw:=raw+PKG_FAD_RISK_SCORE_V1.instance_marks(d.gravity,d.instances);
     WHEN 'fraud_override' THEN fraud:=fraud+d.instances;
     WHEN 'movement' THEN raw:=raw+PKG_FAD_RISK_SCORE_V1.movement_marks(d.base_value,d.actual_value);
     WHEN 'shortfall' THEN raw:=raw+PKG_FAD_RISK_SCORE_V1.shortfall_marks(d.base_value,d.actual_value);
     WHEN 'profit' THEN raw:=raw+PKG_FAD_RISK_SCORE_V1.profit_marks(d.actual_value);
    END CASE;
   END LOOP;
   marks:=PKG_FAD_RISK_SCORE_V1.normalized_marks(raw,p.weight,p.denominator);
   INSERT INTO t_fad_risk_result VALUES(p_id,p.process_code,raw,p.weight,p.denominator,marks);
   total:=total+marks;
  END LOOP;
  UPDATE t_fad_risk_assessment SET score=total,fraud_count=fraud,
   rating=PKG_FAD_RISK_SCORE_V1.final_rating(total,size_name,fraud) WHERE assessment_id=p_id;
 END;

 PROCEDURE record_history(p_id NUMBER,p_actor NUMBER,p_action VARCHAR2,p_remarks VARCHAR2) IS
  inputs CLOB; results CLOB; rev NUMBER;
 BEGIN
  SELECT JSON_OBJECT('versionId' VALUE a.version_id,'branchSize' VALUE a.branch_size,
   'portfolio' VALUE a.portfolio,'sizeReason' VALUE a.size_reason,
   'details' VALUE (SELECT JSON_ARRAYAGG(JSON_OBJECT('code' VALUE d.item_code,'instances' VALUE d.instances,
    'base' VALUE d.base_value,'actual' VALUE d.actual_value,'observation' VALUE d.observation,'evidence' VALUE d.evidence
    RETURNING CLOB) ORDER BY d.item_code RETURNING CLOB) FROM t_fad_risk_detail d WHERE d.assessment_id=p_id) FORMAT JSON
   RETURNING CLOB),a.revision INTO inputs,rev FROM t_fad_risk_assessment a WHERE a.assessment_id=p_id;
  SELECT JSON_OBJECT('score' VALUE a.score,'rating' VALUE a.rating,'fraud' VALUE a.fraud_count,
   'components' VALUE (SELECT JSON_ARRAYAGG(JSON_OBJECT('process' VALUE r.process_code,'raw' VALUE r.raw_marks,
    'weight' VALUE r.weight,'denominator' VALUE r.denominator,'marks' VALUE r.capped_marks RETURNING CLOB)
    ORDER BY r.process_code RETURNING CLOB) FROM t_fad_risk_result r WHERE r.assessment_id=p_id) FORMAT JSON
   RETURNING CLOB) INTO results FROM t_fad_risk_assessment a WHERE a.assessment_id=p_id;
  INSERT INTO t_fad_risk_history(assessment_id,revision,action,actor_id,remarks,input_snapshot,result_snapshot)
   VALUES(p_id,rev,p_action,p_actor,p_remarks,inputs,results);
 END;

 @P_SAVE@ IS
  a t_fad_risk_assessment%ROWTYPE; n NUMBER; expected NUMBER; actual NUMBER; unique_items NUMBER;
  v_entity NUMBER; v_name VARCHAR2(500); v_period VARCHAR2(500); v_sub VARCHAR2(20); kind VARCHAR2(20);
  v_original_id NUMBER:=p_id;
 BEGIN
  SAVEPOINT fad_save;
  authorize(@ARGS@,'SAVE',p_eng);
  -- Lock the engagement so a concurrent closure cannot race an assessment edit.
  SELECT status,entity_id INTO n,v_entity FROM t_au_plan_eng WHERE eng_id=p_eng AND auditby_id=p_entity FOR UPDATE;
  IF n IS NULL OR n NOT BETWEEN 10 AND 13 THEN
   RAISE_APPLICATION_ERROR(-20044,'Engagement is not open for preparation.'); END IF;
  IF p_size IS NULL OR p_size NOT IN ('Small','Medium','Large') OR p_portfolio IS NULL OR p_portfolio<0
    OR TRIM(p_reason) IS NULL OR LENGTH(p_reason)>2000 THEN RAISE_APPLICATION_ERROR(-20040,'Branch size, portfolio and size basis are required.'); END IF;
  SELECT COUNT(*) INTO n FROM t_fad_risk_version WHERE version_id=p_version AND state='PUBLISHED'
   AND calculator='PKG_FAD_RISK_SCORE_V1';
  IF n<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Published supported configuration required.'); END IF;
  IF NVL(p_id,0)=0 THEN
   SELECT e.entity_id,b.description,TO_CHAR(e.audit_startdate,'YYYY-MM-DD')||' - '||TO_CHAR(e.audit_enddate,'YYYY-MM-DD')
    INTO v_entity,v_name,v_period FROM t_au_plan_eng e JOIN t_auditee_entities b ON b.entity_id=e.entity_id
    WHERE e.eng_id=p_eng AND e.auditby_id=p_entity AND e.status BETWEEN 10 AND 13;
   INSERT INTO t_fad_risk_assessment(eng_id,entity_id,version_id,branch_name,audit_period,branch_size,portfolio,size_reason,preparer_id)
    VALUES(p_eng,v_entity,p_version,v_name,v_period,p_size,p_portfolio,p_reason,p_user) RETURNING assessment_id INTO p_id;
  ELSE
   SELECT * INTO a FROM t_fad_risk_assessment WHERE assessment_id=p_id AND eng_id=p_eng FOR UPDATE;
   IF v_entity IS NULL OR a.entity_id<>v_entity THEN RAISE_APPLICATION_ERROR(-20043,'Engagement entity changed.'); END IF;
   IF a.version_id<>p_version OR a.preparer_id<>p_user THEN RAISE_APPLICATION_ERROR(-20043,'Only the preparer may edit this version.'); END IF;
   IF a.status NOT IN ('DRAFT','RETURNED') OR p_revision IS NULL OR a.revision<>p_revision THEN
    RAISE_APPLICATION_ERROR(-20044,'Assessment changed or is locked; reload.'); END IF;
   UPDATE t_fad_risk_assessment SET branch_size=p_size,portfolio=p_portfolio,size_reason=p_reason,
    revision=revision+1,updated_on=SYSTIMESTAMP WHERE assessment_id=p_id;
  END IF;
  SELECT (SELECT COUNT(*) FROM t_fad_risk_annex WHERE version_id=p_version)+
   (SELECT COUNT(*) FROM t_fad_risk_subprocess WHERE version_id=p_version AND criterion IN ('movement','shortfall','profit')) INTO expected FROM dual;
  SELECT COUNT(*),COUNT(DISTINCT code) INTO actual,unique_items FROM JSON_TABLE(p_details,'$[*]' ERROR ON ERROR
   COLUMNS(code VARCHAR2(50) PATH '$.code' ERROR ON ERROR));
  IF actual<>expected OR unique_items<>expected THEN RAISE_APPLICATION_ERROR(-20040,'All configuration items must appear exactly once.'); END IF;
  DELETE FROM t_fad_risk_detail WHERE assessment_id=p_id;
  FOR d IN (SELECT * FROM JSON_TABLE(p_details,'$[*]' ERROR ON ERROR COLUMNS(
   code VARCHAR2(50) PATH '$.code' ERROR ON ERROR,instances NUMBER PATH '$.instances' ERROR ON ERROR,
   base_value NUMBER PATH '$.base' ERROR ON ERROR,actual_value NUMBER PATH '$.actual' ERROR ON ERROR,
   observation VARCHAR2(2000) PATH '$.observation' ERROR ON ERROR,evidence VARCHAR2(2000) PATH '$.evidence' ERROR ON ERROR))) LOOP
   SELECT sub_code,criterion INTO v_sub,kind FROM (
    SELECT x.code,s.sub_code,s.criterion FROM t_fad_risk_annex x JOIN t_fad_risk_subprocess s
     ON s.version_id=x.version_id AND s.sub_code=x.sub_code WHERE x.version_id=p_version
    UNION ALL SELECT sub_code,sub_code,criterion FROM t_fad_risk_subprocess WHERE version_id=p_version
     AND criterion IN ('movement','shortfall','profit')) WHERE code=d.code;
   IF kind IN ('instances','fraud_override') THEN
    IF d.instances IS NULL OR d.instances<0 OR d.instances<>TRUNC(d.instances) OR d.base_value IS NOT NULL OR d.actual_value IS NOT NULL THEN
     RAISE_APPLICATION_ERROR(-20040,'Nonnegative whole instance counts required.'); END IF;
    IF d.instances>0 AND (TRIM(d.observation) IS NULL OR TRIM(d.evidence) IS NULL) THEN
     RAISE_APPLICATION_ERROR(-20040,'Positive instances require observation and evidence references.'); END IF;
   ELSE
    IF d.instances IS NOT NULL OR d.actual_value IS NULL OR (kind<>'profit' AND d.base_value IS NULL)
     OR (kind='profit' AND d.base_value IS NOT NULL) OR TRIM(d.evidence) IS NULL THEN
     RAISE_APPLICATION_ERROR(-20040,'Financial inputs and supporting evidence are required.'); END IF;
   END IF;
   validate_evidence(p_eng,p_entity,kind,d.observation,d.evidence);
   INSERT INTO t_fad_risk_detail VALUES(p_id,p_version,d.code,v_sub,d.instances,d.base_value,d.actual_value,d.observation,d.evidence);
  END LOOP;
  calculate(p_id);
  record_history(p_id,p_user,'SAVE',NULL);
 EXCEPTION WHEN OTHERS THEN ROLLBACK TO fad_save; p_id:=v_original_id; RAISE;
 END;

 @P_TRANSITION@ IS a t_fad_risk_assessment%ROWTYPE; v_entity NUMBER; v_status NUMBER;
 BEGIN
  SAVEPOINT fad_transition;
  IF p_action IS NULL OR p_action NOT IN ('SUBMIT','APPROVE','RETURN') THEN RAISE_APPLICATION_ERROR(-20040,'Invalid workflow action.'); END IF;
  authorize(@ARGS@,p_action,p_eng);
  SELECT entity_id,status INTO v_entity,v_status FROM t_au_plan_eng
   WHERE eng_id=p_eng AND auditby_id=p_entity FOR UPDATE;
  SELECT * INTO a FROM t_fad_risk_assessment WHERE assessment_id=p_id AND eng_id=p_eng FOR UPDATE;
  IF v_entity IS NULL OR a.entity_id<>v_entity THEN RAISE_APPLICATION_ERROR(-20043,'Engagement entity changed.'); END IF;
  IF p_action='SUBMIT' AND (v_status IS NULL OR v_status NOT BETWEEN 10 AND 13) THEN
   RAISE_APPLICATION_ERROR(-20044,'Engagement is not open for submission.'); END IF;
  IF p_action IN ('SUBMIT','APPROVE') THEN require_complete(p_id); END IF;
  IF p_revision IS NULL OR a.revision<>p_revision THEN RAISE_APPLICATION_ERROR(-20044,'Assessment changed; reload.'); END IF;
  IF p_action='SUBMIT' THEN
   IF a.status NOT IN ('DRAFT','RETURNED') OR a.preparer_id<>p_user THEN RAISE_APPLICATION_ERROR(-20043,'Only the preparer may submit a draft.'); END IF;
   calculate(p_id);
   UPDATE t_fad_risk_assessment SET status='SUBMITTED',reviewer_id=NULL,revision=revision+1,updated_on=SYSTIMESTAMP WHERE assessment_id=p_id;
  ELSE
   IF a.status<>'SUBMITTED' OR a.preparer_id=p_user THEN RAISE_APPLICATION_ERROR(-20043,'Independent reviewer required for a submitted assessment.'); END IF;
   IF TRIM(p_remarks) IS NULL OR LENGTH(p_remarks)>2000 THEN RAISE_APPLICATION_ERROR(-20040,'Reviewer remarks required.'); END IF;
   -- Record the exact stored calculation; no client marks or ratings are accepted.
   UPDATE t_fad_risk_assessment SET status=CASE p_action WHEN 'APPROVE' THEN 'APPROVED' ELSE 'RETURNED' END,
    reviewer_id=p_user,revision=revision+1,updated_on=SYSTIMESTAMP WHERE assessment_id=p_id;
  END IF;
  record_history(p_id,p_user,p_action,p_remarks);
 EXCEPTION WHEN OTHERS THEN ROLLBACK TO fad_transition; RAISE;
 END;

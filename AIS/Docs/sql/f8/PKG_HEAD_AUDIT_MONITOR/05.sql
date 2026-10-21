-- F8: execute this file alone in PL/SQL Developer SQL Window.
CREATE OR REPLACE PACKAGE BODY PKG_HEAD_AUDIT_MONITOR AS
  PROCEDURE REQUIRE_SCOPE(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,P_ENG_ID NUMBER DEFAULT NULL) AS
    n NUMBER;
  BEGIN
    SELECT COUNT(*) INTO n FROM t_user u
     WHERE u.userid=P_USER_ID AND u.isactive='Y'
       AND EXISTS (SELECT 1 FROM t_groups g WHERE g.role_id=P_ROLE_ID AND g.status='Y')
       AND ((P_CONTEXT_ID IS NOT NULL AND EXISTS (
         SELECT 1 FROM t_user_context_assignment c
          WHERE c.user_context_id=P_CONTEXT_ID AND c.user_id=u.userid
            AND c.role_id=P_ROLE_ID AND c.entity_id=P_DEPT_ID AND c.is_active='Y'
            AND (c.effective_from IS NULL OR c.effective_from<=SYSDATE)
            AND (c.effective_to IS NULL OR c.effective_to>=TRUNC(SYSDATE))))
         OR (P_CONTEXT_ID IS NULL AND u.entity_id=P_DEPT_ID AND EXISTS (
           SELECT 1 FROM t_user_maping m WHERE m.userid=u.userid AND m.role_id=P_ROLE_ID)))
       AND EXISTS (SELECT 1 FROM v_exec_entity_scope sc WHERE sc.entity_id=P_DEPT_ID);
    IF n<>1 THEN RAISE_APPLICATION_ERROR(-20151,'Unauthorized Head audit department context.'); END IF;
    IF P_ENG_ID IS NOT NULL THEN
      SELECT COUNT(*) INTO n FROM v_head_audit_eng_scope
       WHERE dept_id=P_DEPT_ID AND eng_id=P_ENG_ID;
      IF n<>1 THEN RAISE_APPLICATION_ERROR(-20152,'Engagement outside authorized ongoing audit portfolio.'); END IF;
    END IF;
  END;

  PROCEDURE REQUIRE_PERIOD(P_PERIOD_ID NUMBER) AS n NUMBER;
  BEGIN
    SELECT COUNT(*) INTO n FROM t_au_period WHERE auditperiodid=P_PERIOD_ID;
    IF n<>1 THEN RAISE_APPLICATION_ERROR(-20153,'Select a valid IAS audit period.'); END IF;
  END;

  PROCEDURE P_GET_PERIODS(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID);
    OPEN IO_CURSOR FOR SELECT auditperiodid,description,start_date,end_date
      FROM t_au_period ORDER BY start_date DESC,auditperiodid DESC;
  END;

  -- Current position within one audit-period cohort, not historical completion activity.
  -- Approved plan entries: T_AU_PLAN statuses 1/2 (existing CIA plan definition).
  -- Completed: current engagement status 14, written by PKG_HD at conclusion.
  -- 12 = Draft Report, 13 = Issue Final Report (PKG_HD dashboard labels).
  -- Neither 12 nor 13 is counted as completed; pending reports counts engagements,
  -- not report versions. Active engagement stages 4..13; active teams use 4..11.
  -- Each plan ID counts once even when it has multiple engagements. Completion
  -- takes precedence over ongoing in the plan progress partition. Distinct
  -- auditee entities are separate and must never be summed across office rows.
  -- Delays use only recorded AUDIT_ENDDATE; no invented report SLA/date.
  PROCEDURE P_GET_OFFICE_SUMMARY(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,P_PERIOD_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID);
    REQUIRE_PERIOD(P_PERIOD_ID);
    OPEN IO_CURSOR FOR
      WITH offices AS (
        SELECT office_id FROM v_head_audit_office_scope WHERE dept_id=P_DEPT_ID
      ), engagements AS (
        SELECT e.* FROM t_au_plan_eng e
         WHERE e.period_id=P_PERIOD_ID AND EXISTS
          (SELECT 1 FROM offices o WHERE o.office_id=e.auditby_id)
      ), plans AS (
        SELECT p.id,p.entity_id,p.auditedby office_id,
          CASE WHEN EXISTS (SELECT 1 FROM engagements e WHERE e.plan_id=p.id
            AND e.auditby_id=p.auditedby AND e.status=14) THEN 1 ELSE 0 END done,
          CASE WHEN EXISTS (SELECT 1 FROM engagements e WHERE e.plan_id=p.id
            AND e.auditby_id=p.auditedby AND e.status BETWEEN 4 AND 13) THEN 1 ELSE 0 END ongoing
        FROM t_au_plan p WHERE p.auditperiodid=P_PERIOD_ID AND p.status IN (1,2)
          AND EXISTS (SELECT 1 FROM offices o WHERE o.office_id=p.auditedby)
      ), plan_totals AS (
        SELECT office_id,COUNT(*) planned,COUNT(DISTINCT entity_id) planned_entities,
          SUM(done) completed_plans,SUM(CASE WHEN done=0 AND ongoing=1 THEN 1 ELSE 0 END) ongoing_plans,
          SUM(CASE WHEN done=0 AND ongoing=0 THEN 1 ELSE 0 END) not_started
        FROM plans GROUP BY office_id
      ), engagement_totals AS (
        SELECT e.auditby_id office_id,
          COUNT(CASE WHEN e.status=14 THEN 1 END) completed,
          COUNT(CASE WHEN e.status BETWEEN 4 AND 13 THEN 1 END) ongoing,
          COUNT(CASE WHEN e.status BETWEEN 4 AND 13 AND e.audit_enddate <
            TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE 'Asia/Karachi' AS DATE)) THEN 1 END) delayed,
          COUNT(CASE WHEN e.status IN (12,13) THEN 1 END) pending_reports,
          SUM((SELECT COUNT(*) FROM t_au_observation ob WHERE ob.engplanid=e.eng_id AND ob.status=3)) awaiting_review
        FROM engagements e GROUP BY e.auditby_id
      )
      SELECT o.office_id,a.name office_name,NVL(p.planned,0) planned,
        NVL(p.planned_entities,0) planned_entities,NVL(p.completed_plans,0) completed_plans,
        NVL(p.ongoing_plans,0) ongoing_plans,NVL(p.not_started,0) not_started,
        NVL(e.completed,0) completed,NVL(e.ongoing,0) ongoing,NVL(e.delayed,0) delayed,
        NVL(e.pending_reports,0) pending_reports,NVL(e.awaiting_review,0) awaiting_review
      FROM offices o LEFT JOIN t_auditee_entities a ON a.entity_id=o.office_id
      LEFT JOIN plan_totals p ON p.office_id=o.office_id
      LEFT JOIN engagement_totals e ON e.office_id=o.office_id
      WHERE o.office_id=P_DEPT_ID OR p.office_id IS NOT NULL OR e.office_id IS NOT NULL
      ORDER BY a.name,o.office_id;
  END;

  PROCEDURE P_GET_ENGAGEMENTS(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR,P_PERIOD_ID NUMBER DEFAULT NULL) AS
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID);
    IF P_PERIOD_ID IS NOT NULL THEN REQUIRE_PERIOD(P_PERIOD_ID); END IF;
    OPEN IO_CURSOR FOR
      SELECT e.eng_id,e.team_id,e.auditby_id office_id,e.period_id,e.operation_startdate,
        ent.name entity_name,NVL(d.deptname,office.name) department_name,
        e.team_name,p.description audit_period,e.status status_id,s.status status_name,
        (SELECT LISTAGG(m.member_name||CASE WHEN m.isteamlead='Y' THEN ' (Lead)' END, '; ' ON OVERFLOW TRUNCATE)
           WITHIN GROUP(ORDER BY m.isteamlead DESC,m.member_name) FROM t_au_team_members m WHERE m.t_id=e.team_id) team_members,
        e.audit_startdate,e.audit_enddate,TRUNC(e.audit_enddate)-TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE 'Asia/Karachi' AS DATE)) days_to_end,
        (SELECT COUNT(*) FROM t_au_observation o WHERE o.engplanid=e.eng_id AND o.status=3) awaiting_review,
        (SELECT LISTAGG(status_label||': '||cnt,'; ') WITHIN GROUP(ORDER BY status_id)
           FROM (SELECT t.eng_plan_id,t.status_id,NVL(ts.description,TO_CHAR(t.status_id)) status_label,COUNT(*) cnt
                   FROM t_au_audit_team_tasklist t LEFT JOIN t_au_audit_team_tasklist_status ts ON ts.status_id=t.status_id
                  WHERE t.isactive='Y' GROUP BY t.eng_plan_id,t.status_id,ts.description) q
          WHERE q.eng_plan_id=e.eng_id) task_progress,
        (SELECT LISTAGG(status_label||': '||cnt,'; ') WITHIN GROUP(ORDER BY status_id)
           FROM (SELECT o.engplanid,o.status status_id,NVL(os.statusname,TO_CHAR(o.status)) status_label,COUNT(*) cnt
                   FROM t_au_observation o LEFT JOIN t_au_observation_status os ON os.statusid=o.status
                  GROUP BY o.engplanid,o.status,os.statusname) q
          WHERE q.engplanid=e.eng_id) observation_counts,
        (SELECT COUNT(*) FROM t_audit_reports r WHERE r.eng_id=e.eng_id) report_count,
        (SELECT LISTAGG('v'||r.report_version||': '||r.report_status,'; ' ON OVERFLOW TRUNCATE)
          WITHIN GROUP(ORDER BY r.report_version) FROM t_frpt_report_meta r WHERE r.eng_id=e.eng_id) report_status
       FROM v_head_audit_eng_scope sc JOIN t_au_plan_eng e ON e.eng_id=sc.eng_id
       LEFT JOIN t_auditee_entities ent ON ent.entity_id=e.entity_id
       LEFT JOIN (SELECT entity_id, MAX(deptname) deptname
                    FROM t_audit_departments GROUP BY entity_id) d
         ON d.entity_id=e.auditby_id
       LEFT JOIN t_auditee_entities office ON office.entity_id=e.auditby_id
       LEFT JOIN t_au_period p ON p.auditperiodid=e.period_id
       LEFT JOIN t_au_plan_eng_status s ON s.id=e.status
      WHERE sc.dept_id=P_DEPT_ID AND (P_PERIOD_ID IS NULL OR e.period_id=P_PERIOD_ID) ORDER BY e.audit_enddate NULLS LAST,e.eng_id;
  END;

  PROCEDURE P_GET_WP_COUNTS(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR) AS
    n NUMBER;
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID);
    SELECT COUNT(*) INTO n FROM user_tables WHERE table_name IN ('T_WP_HEADER','T_WP_ITEM');
    IF n<>2 THEN
      OPEN IO_CURSOR FOR SELECT eng_id,CAST(NULL AS NUMBER) awaiting_review,
        CAST(NULL AS VARCHAR2(500)) item_progress FROM v_head_audit_eng_scope WHERE dept_id=P_DEPT_ID;
    ELSE
      OPEN IO_CURSOR FOR q'[
        WITH latest AS (
          SELECT h.*,ROW_NUMBER() OVER(PARTITION BY h.eng_id,h.entity_id,h.paper_type
            ORDER BY h.version_no DESC,h.wp_id DESC) rn FROM t_wp_header h
            JOIN v_head_audit_eng_scope sc ON sc.eng_id=h.eng_id WHERE sc.dept_id=:dept
        ), items AS (
          SELECT h.eng_id,COUNT(*) total,
            SUM(CASE WHEN i.result IN ('PASS','EXCEPTION','NOT_APPLICABLE') THEN 1 ELSE 0 END) completed,
            SUM(CASE WHEN i.result IN ('PENDING','NOT_TESTED') THEN 1 ELSE 0 END) pending
          FROM latest h JOIN t_wp_item i ON i.wp_id=h.wp_id WHERE h.rn=1 GROUP BY h.eng_id
        )
        SELECT sc.eng_id,(SELECT COUNT(*) FROM latest h WHERE h.eng_id=sc.eng_id AND h.rn=1 AND h.status='IN_REVIEW') awaiting_review,
          'V2 saved items: '||NVL(i.total,0)||' / completed results: '||NVL(i.completed,0)||' / pending: '||NVL(i.pending,0) item_progress
          FROM v_head_audit_eng_scope sc LEFT JOIN items i ON i.eng_id=sc.eng_id WHERE sc.dept_id=:dept2
      ]' USING P_DEPT_ID,P_DEPT_ID;
    END IF;
  END;

  PROCEDURE P_GET_WORK(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,P_ENG_ID NUMBER,P_SECTION VARCHAR2,IO_CURSOR OUT SYS_REFCURSOR) AS
    n NUMBER;
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID,P_ENG_ID);
    IF P_ENG_ID IS NULL THEN RAISE_APPLICATION_ERROR(-20152,'Engagement required.'); END IF;
    IF P_SECTION='Plan' THEN
      OPEN IO_CURSOR FOR
        SELECT p.id record_id,'Audit plan #'||p.id title,TO_CHAR(p.status) status_name,
          CAST(NULL AS VARCHAR2(100)) author,CAST(NULL AS DATE) saved_on,
          TO_CLOB('Criteria ID: '||p.criteria_id||CHR(10)||'Period ID: '||p.auditperiodid||CHR(10)||
            'Audit days: '||p.no_of_days||CHR(10)||'Risk ID: '||p.auditee_risk||CHR(10)||
            'Size ID: '||p.auditee_size||CHR(10)||'Nature ID: '||p.nature_id) content
         FROM t_au_plan_eng e JOIN t_au_plan p ON p.id=e.plan_id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=e.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND e.eng_id=P_ENG_ID;
    ELSIF P_SECTION='Criteria' THEN
      OPEN IO_CURSOR FOR
        SELECT c.id record_id,'Audit criteria #'||c.id title,NVL(cs.status,TO_CHAR(c.approval_status)) status_name,
          TO_CHAR(c.created_by) author,CAST(NULL AS DATE) saved_on,
          TO_CLOB('Criteria source: T_AUDIT_CRITERIA'||CHR(10)||'Audit days: '||c.no_of_days||CHR(10)||
            'Frequency ID: '||c.frequency_id||CHR(10)||'Visits: '||c.visit||CHR(10)||
            'Risk ID: '||c.risk_id||CHR(10)||'Size ID: '||c.size_id) content
         FROM t_au_plan_eng e JOIN t_au_plan p ON p.id=e.plan_id JOIN t_audit_criteria c ON c.id=p.criteria_id
         LEFT JOIN t_audit_criteria_status cs ON cs.id=c.approval_status
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=e.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND e.eng_id=P_ENG_ID;
    ELSIF P_SECTION='Team' THEN
      OPEN IO_CURSOR FOR
        SELECT m.id record_id,m.member_name title,m.status status_name,TO_CHAR(m.member_ppno) author,
          CAST(NULL AS DATE) saved_on,TO_CLOB('Team: '||m.team_name||CHR(10)||'Team Lead: '||m.isteamlead||CHR(10)||
            'Place of posting: '||m.place_of_posting) content
         FROM t_au_plan_eng e JOIN t_au_team_members m ON m.t_id=e.team_id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=e.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND e.eng_id=P_ENG_ID ORDER BY m.isteamlead DESC,m.member_name;
    ELSIF P_SECTION='Tasks' THEN
      OPEN IO_CURSOR FOR
        SELECT t.id record_id,'Assignment #'||t.id title,NVL(s.description,TO_CHAR(t.status_id)) status_name,
          TO_CHAR(t.teammember_ppno) author,CAST(NULL AS DATE) saved_on,
          TO_CLOB('Team ID: '||t.team_id||CHR(10)||'Sequence: '||t.sequence_no||CHR(10)||
            'Active: '||t.isactive||CHR(10)||'Entity: '||t.entity_name||CHR(10)||
            'Start: '||TO_CHAR(t.audit_start_date,'YYYY-MM-DD')||CHR(10)||
            'End: '||TO_CHAR(t.audit_end_date,'YYYY-MM-DD')) content
         FROM t_au_audit_team_tasklist t LEFT JOIN t_au_audit_team_tasklist_status s ON s.status_id=t.status_id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=t.eng_plan_id
        WHERE sc.dept_id=P_DEPT_ID AND t.eng_plan_id=P_ENG_ID ORDER BY t.id;
    ELSIF P_SECTION IN ('WorkpapersLoan','WorkpapersVoucher','WorkpapersAccount','WorkpapersAssets','WorkpapersCash') THEN
      -- Separate cursors preserve full CLOB text without Oracle set operators on LOBs.
      IF P_SECTION='WorkpapersLoan' THEN
        OPEN IO_CURSOR FOR SELECT w.lc_id record_id,'Loan case: '||w.lc_number title,
          CAST(NULL AS VARCHAR2(100)) status_name,w.entered_by author,CAST(w.entered_on AS DATE) saved_on,
          TO_CLOB('Amount: '||w.amount||CHR(10)||'Disbursed: '||TO_CHAR(w.disb_date,'YYYY-MM-DD')||CHR(10)||
            'Category: '||w.category||CHR(10)||'Para: '||w.para_no||CHR(10))||w.observation content
          FROM t_working_paper_loan_case_file w JOIN v_head_audit_eng_scope sc ON TO_CHAR(sc.eng_id)=TRIM(w.eng_id)
         WHERE sc.dept_id=P_DEPT_ID AND sc.eng_id=P_ENG_ID ORDER BY w.lc_id;
      ELSIF P_SECTION='WorkpapersVoucher' THEN
        OPEN IO_CURSOR FOR SELECT w.v_id record_id,'Voucher: '||w.v_number title,
          CAST(NULL AS VARCHAR2(100)) status_name,w.entered_by author,CAST(w.entered_on AS DATE) saved_on,
          TO_CLOB('Para: '||w.para_no||CHR(10))||w.observation content
          FROM t_working_paper_voucher_checking w JOIN v_head_audit_eng_scope sc ON TO_CHAR(sc.eng_id)=TRIM(w.eng_id)
         WHERE sc.dept_id=P_DEPT_ID AND sc.eng_id=P_ENG_ID ORDER BY w.v_id;
      ELSIF P_SECTION='WorkpapersAccount' THEN
        OPEN IO_CURSOR FOR SELECT w.a_id record_id,'Account opening: '||w.v_number title,
          CAST(NULL AS VARCHAR2(100)) status_name,w.entered_by author,CAST(w.entered_on AS DATE) saved_on,
          TO_CLOB('Nature: '||w.a_nature||CHR(10)||'Para: '||w.para_no||CHR(10))||w.observation content
          FROM t_working_paper_account_opening w JOIN v_head_audit_eng_scope sc ON TO_CHAR(sc.eng_id)=TRIM(w.eng_id)
         WHERE sc.dept_id=P_DEPT_ID AND sc.eng_id=P_ENG_ID ORDER BY w.a_id;
      ELSIF P_SECTION='WorkpapersAssets' THEN
        OPEN IO_CURSOR FOR SELECT w.fa_id record_id,'Fixed asset: '||w.asset_name title,
          CAST(NULL AS VARCHAR2(100)) status_name,w.entered_by author,CAST(w.entered_on AS DATE) saved_on,
          TO_CLOB('Physical existence: '||w.physical_existance||CHR(10)||'FAR location: '||w.location_as_per_far||CHR(10)||
            'Difference: '||w.difference||CHR(10))||w.remarks content
          FROM t_working_paper_fixed_assets w JOIN v_head_audit_eng_scope sc ON TO_CHAR(sc.eng_id)=TRIM(w.eng_id)
         WHERE sc.dept_id=P_DEPT_ID AND sc.eng_id=P_ENG_ID ORDER BY w.fa_id;
      ELSE
        OPEN IO_CURSOR FOR SELECT w.id record_id,'Cash count' title,
          CAST(NULL AS VARCHAR2(100)) status_name,w.entered_by author,CAST(w.entered_on AS DATE) saved_on,
          TO_CLOB('Vault denomination: '||w.denomination_vault||' / notes: '||w.no_currency_notes_vault||' / total: '||w.total_amount_vault||CHR(10)||
            'Safe register denomination: '||w.denomination_safe_register||' / notes: '||w.no_currency_notes_safe_register||
            ' / total: '||w.total_amount_safe_register||CHR(10)||'Difference: '||w.difference) content
          FROM t_working_paper_cash_count w JOIN v_head_audit_eng_scope sc ON TO_CHAR(sc.eng_id)=TRIM(w.eng_id)
         WHERE sc.dept_id=P_DEPT_ID AND sc.eng_id=P_ENG_ID ORDER BY w.id;
      END IF;
    ELSIF P_SECTION IN ('V2Headers','V2Items','V2Reviews','V2Evidence') THEN
      -- Optional installed V2 sources: no environment/configuration gate and no dependency
      -- on V2 being enabled in operational screens. Never hide database read errors.
      SELECT COUNT(*) INTO n FROM user_tables WHERE table_name IN
        ('T_WP_HEADER','T_WP_ITEM','T_WP_REVIEW_NOTE','T_WP_EVIDENCE_LINK');
      IF n<>4 THEN
        OPEN IO_CURSOR FOR SELECT 0 record_id,CAST(NULL AS VARCHAR2(100)) title,
          CAST(NULL AS VARCHAR2(100)) status_name,CAST(NULL AS VARCHAR2(100)) author,
          CAST(NULL AS DATE) saved_on,TO_CLOB('V2 working papers: N/A (not installed).') content FROM dual;
      ELSIF P_SECTION='V2Headers' THEN
        OPEN IO_CURSOR FOR q'[
          SELECT h.wp_id record_id,'V2 '||h.reference_no||' / version '||h.version_no title,
            h.status status_name,h.preparer_ppno author,CAST(h.updated_on AS DATE) saved_on,
            TO_CLOB('Plan: '||CHR(10))||h.plan_json||CHR(10)||'Conclusion: '||h.conclusion_json||CHR(10)||
              'Reviewer: '||h.reviewer_ppno||CHR(10)||'Reviewed: '||TO_CHAR(h.reviewed_on,'YYYY-MM-DD HH24:MI:SS')||CHR(10)||
              'Approved: '||TO_CHAR(h.approved_on,'YYYY-MM-DD HH24:MI:SS') content
           FROM t_wp_header h JOIN v_head_audit_eng_scope sc ON sc.eng_id=h.eng_id
          WHERE sc.dept_id=:dept AND h.eng_id=:eng
            AND NOT EXISTS (SELECT 1 FROM t_wp_header newer WHERE newer.eng_id=h.eng_id
              AND newer.entity_id=h.entity_id AND newer.paper_type=h.paper_type AND
              (newer.version_no>h.version_no OR (newer.version_no=h.version_no AND newer.wp_id>h.wp_id)))
          ORDER BY h.paper_type,h.entity_id]' USING P_DEPT_ID,P_ENG_ID;
      ELSIF P_SECTION='V2Items' THEN
        OPEN IO_CURSOR FOR q'[
          SELECT i.item_id record_id,'V2 '||h.reference_no||' / '||i.item_reference title,
            i.result status_name,i.updated_by author,CAST(i.updated_on AS DATE) saved_on,
            TO_CLOB('Details: '||CHR(10))||i.details_json||CHR(10)||'Calculations: '||i.calculations_json||CHR(10)||
              'Auditor comment: '||i.auditor_comment content
           FROM t_wp_item i JOIN t_wp_header h ON h.wp_id=i.wp_id JOIN v_head_audit_eng_scope sc ON sc.eng_id=h.eng_id
          WHERE sc.dept_id=:dept AND h.eng_id=:eng
            AND NOT EXISTS (SELECT 1 FROM t_wp_header newer WHERE newer.eng_id=h.eng_id
              AND newer.entity_id=h.entity_id AND newer.paper_type=h.paper_type AND
              (newer.version_no>h.version_no OR (newer.version_no=h.version_no AND newer.wp_id>h.wp_id)))
          ORDER BY h.wp_id,i.item_id]' USING P_DEPT_ID,P_ENG_ID;
      ELSIF P_SECTION='V2Reviews' THEN
        OPEN IO_CURSOR FOR q'[
          SELECT r.note_id record_id,'V2 review / '||h.reference_no||' / '||r.section_key title,
            r.status status_name,r.raised_by author,CAST(r.raised_on AS DATE) saved_on,
            TO_CLOB(r.note_text)||CHR(10)||'Response: '||r.response_text||CHR(10)||
              'Responded by: '||r.responded_by||CHR(10)||'Closed by: '||r.closed_by content
           FROM t_wp_review_note r JOIN t_wp_header h ON h.wp_id=r.wp_id JOIN v_head_audit_eng_scope sc ON sc.eng_id=h.eng_id
          WHERE sc.dept_id=:dept AND h.eng_id=:eng ORDER BY r.raised_on DESC,r.note_id DESC]' USING P_DEPT_ID,P_ENG_ID;
      ELSE
        OPEN IO_CURSOR FOR q'[
          SELECT f.evidence_link_id record_id,'V2 evidence / '||h.reference_no||' / '||f.title title,
            f.evidence_type status_name,f.linked_by author,CAST(f.linked_on AS DATE) saved_on,
            TO_CLOB('Evidence reference: '||f.existing_evidence_id||CHR(10)||'Source: '||f.source_name||CHR(10)||
              'Item: '||f.item_id||CHR(10)||'Evidence date: '||TO_CHAR(f.evidence_date,'YYYY-MM-DD')) content
           FROM t_wp_evidence_link f JOIN t_wp_header h ON h.wp_id=f.wp_id JOIN v_head_audit_eng_scope sc ON sc.eng_id=h.eng_id
          WHERE sc.dept_id=:dept AND h.eng_id=:eng ORDER BY f.linked_on DESC,f.evidence_link_id DESC]' USING P_DEPT_ID,P_ENG_ID;
      END IF;
    ELSIF P_SECTION='Observations' THEN
      OPEN IO_CURSOR FOR
        SELECT o.id record_id,'Observation #'||o.id||' / Risk: '||NVL(r.description,TO_CHAR(o.severity)) title,
          NVL(s.statusname,TO_CHAR(o.status)) status_name,TO_CHAR(o.enteredby) author,
          NVL(ot.lastupdateddate,ot.entereddate) saved_on,
          TO_CLOB('Memo: '||o.memo_number||CHR(10)||'Draft para: '||o.draft_para_no||CHR(10)||
            'Final para: '||o.final_para_no||CHR(10)||'Reviewed by: '||o.reviewed_by||CHR(10)||
            'Reviewed on: '||TO_CHAR(o.reviewed_on,'YYYY-MM-DD HH24:MI:SS')||CHR(10)||ot.headings||CHR(10))||ot.text content
         FROM t_au_observation o LEFT JOIN t_au_observation_status s ON s.statusid=o.status
         LEFT JOIN t_risk r ON r.r_id=o.severity
         LEFT JOIN t_au_observation_text ot ON ot.observatsion_id=o.id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
        WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID ORDER BY o.id,ot.id;
    ELSIF P_SECTION='History' THEN
      OPEN IO_CURSOR FOR
        SELECT l.id record_id,'Engagement workflow history' title,NVL(s.status,TO_CHAR(l.status_id)) status_name,
          TO_CHAR(l.createdby_id) author,l.created_on saved_on,
          TO_CLOB(l.remarks)||CHR(10)||l.comments content
         FROM t_au_plan_eng_log l LEFT JOIN t_au_plan_eng_status s ON s.id=l.status_id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=l.e_id
        WHERE sc.dept_id=P_DEPT_ID AND l.e_id=P_ENG_ID ORDER BY l.created_on DESC,l.id DESC;
    ELSIF P_SECTION='ObservationReviews' THEN
      OPEN IO_CURSOR FOR
        SELECT r.id record_id,'Observation #'||o.id||' recommendation' title,
          TO_CHAR(r.status) status_name,TO_CHAR(r.reco_by) author,r.reco_date saved_on,
          TO_CLOB('Role: '||r.reco_role||CHR(10)||'Submitted: '||r.submitted||CHR(10))||r.recommendation content
         FROM t_au_observations_auditor_recommendation r JOIN t_au_observation o ON o.id=r.au_obs_id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
        WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID ORDER BY r.reco_date DESC,r.id DESC;
    ELSIF P_SECTION='ObservationResponses' THEN
      OPEN IO_CURSOR FOR
        SELECT r.id record_id,'Observation #'||o.id||' response' title,
          TO_CHAR(r.status) status_name,TO_CHAR(r.repliedby) author,NVL(r.lastupdateddate,r.replieddate) saved_on,
          TO_CLOB('Role: '||r.reply_role||CHR(10)||'Submitted: '||r.submitted||CHR(10))||r.reply||CHR(10)||r.remarks content
         FROM t_au_observations_auditee_response r JOIN t_au_observation o ON o.id=r.au_obs_id
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
        WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID ORDER BY r.replieddate DESC,r.id DESC;
    ELSIF P_SECTION='ResponseEvidence' THEN
      OPEN IO_CURSOR FOR
        SELECT f.id record_id,f.file_name title,f.status status_name,TO_CHAR(f.enteredby) author,
          NVL(f.lastupdateddate,f.entereddate) saved_on,
          TO_CLOB('Observation ID: '||o.id||CHR(10)||'Response ID: '||f.respid||CHR(10)||'Bytes: '||f.length) content
         FROM t_au_observations_auditee_evidences f JOIN t_au_observation o ON o.id=f.memoid
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
        WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID ORDER BY f.id;
    ELSIF P_SECTION='Evidence' THEN
      OPEN IO_CURSOR FOR
        SELECT f.id record_id,f.file_name title,CAST(NULL AS VARCHAR2(100)) status_name,
          TO_CHAR(f.enteredby) author,NVL(f.lastupdateddate,f.entereddate) saved_on,
          TO_CLOB('Observation ID: '||o.id||CHR(10)||'Bytes: '||f.length||CHR(10)||f.description) content
         FROM t_au_observation_evidences f JOIN t_au_observation o ON o.id=f.memoid
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
        WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID ORDER BY f.id;
    ELSIF P_SECTION='DraftReportMeta' THEN
      OPEN IO_CURSOR FOR
        SELECT r.frpt_id record_id,'Report version '||r.report_version title,r.report_status status_name,
          TO_CHAR(r.generated_by) author,CAST(r.generated_on AS DATE) saved_on,
          TO_CLOB('Finalized by: '||r.finalized_by||CHR(10)||'Finalized on: '||TO_CHAR(r.finalized_on,'YYYY-MM-DD HH24:MI:SS')) content
         FROM t_frpt_report_meta r JOIN v_head_audit_eng_scope sc ON sc.eng_id=r.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND r.eng_id=P_ENG_ID ORDER BY r.report_version DESC;
    ELSIF P_SECTION='DraftReportText' THEN
      OPEN IO_CURSOR FOR
        SELECT r.frpt_text_id record_id,NVL(s.section_title,r.section_code) title,
          CAST(NULL AS VARCHAR2(100)) status_name,TO_CHAR(r.created_by) author,CAST(r.created_on AS DATE) saved_on,
          r.section_text content
         FROM t_frpt_text_blocks r LEFT JOIN t_frpt_section_master s ON s.section_code=r.section_code
         JOIN v_head_audit_eng_scope sc ON sc.eng_id=r.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND r.eng_id=P_ENG_ID ORDER BY s.display_order,r.frpt_text_id;
    ELSIF P_SECTION='DraftReportParas' THEN
      OPEN IO_CURSOR FOR
        SELECT r.para_id record_id,'Report para #'||r.para_id title,TO_CHAR(r.is_finalized) status_name,
          TO_CHAR(r.updated_by) author,CAST(r.updated_on AS DATE) saved_on,
          TO_CLOB('Implications: '||CHR(10))||r.implications||CHR(10)||'Recommendations: '||r.recommendations||CHR(10)||
            'Management comments: '||r.mgmt_comments||CHR(10)||'Auditor comments: '||r.auditor_comments||CHR(10)||'SVP remarks: '||r.svp_remarks content
         FROM t_frpt_para_narrative r JOIN v_head_audit_eng_scope sc ON sc.eng_id=r.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND r.eng_id=P_ENG_ID ORDER BY r.para_id;
    ELSIF P_SECTION='DraftReportConclusion' THEN
      OPEN IO_CURSOR FOR
        SELECT r.frpt_conc_id record_id,'Report conclusion / version '||r.report_version title,
          CAST(NULL AS VARCHAR2(100)) status_name,TO_CHAR(r.updated_by) author,CAST(r.updated_on AS DATE) saved_on,
          TO_CLOB('Overall conclusion: '||CHR(10))||r.overall_conclusion_html||CHR(10)||'Non-addressable: '||r.non_addressable_html||CHR(10)||
            'Fraud prone: '||r.fraud_prone_html||CHR(10)||'Regulatory: '||r.regulatory_html||CHR(10)||'Safety/security: '||r.safety_security_html content
         FROM t_frpt_overall_conclusion r JOIN v_head_audit_eng_scope sc ON sc.eng_id=r.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND r.eng_id=P_ENG_ID ORDER BY r.report_version DESC;
    ELSIF P_SECTION='Reports' THEN
      OPEN IO_CURSOR FOR
        SELECT r.id record_id,r.doc_name title,r.doc_type status_name,TO_CHAR(r.added_by) author,
          CAST(r.added_on AS DATE) saved_on,TO_CLOB('Saved report available for download. Approval status: N/A.') content
         FROM t_audit_reports r JOIN v_head_audit_eng_scope sc ON sc.eng_id=r.eng_id
        WHERE sc.dept_id=P_DEPT_ID AND r.eng_id=P_ENG_ID ORDER BY r.added_on DESC,r.id DESC;
    ELSE
      RAISE_APPLICATION_ERROR(-20152,'Unknown read-only section.');
    END IF;
  END;

  PROCEDURE P_GET_EVIDENCE(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,P_ENG_ID NUMBER,P_EVIDENCE_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID,P_ENG_ID);
    IF P_ENG_ID IS NULL THEN RAISE_APPLICATION_ERROR(-20152,'Engagement required.'); END IF;
    OPEN IO_CURSOR FOR
      SELECT f.file_data,f.file_name FROM t_au_observation_evidences f
       JOIN t_au_observation o ON o.id=f.memoid JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
       WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID AND f.id=P_EVIDENCE_ID;
  END;
  PROCEDURE P_GET_REPORT(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,P_ENG_ID NUMBER,P_EVIDENCE_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID,P_ENG_ID);
    IF P_ENG_ID IS NULL THEN RAISE_APPLICATION_ERROR(-20152,'Engagement required.'); END IF;
    OPEN IO_CURSOR FOR
      SELECT r.audit_report,r.doc_name file_name FROM t_audit_reports r JOIN v_head_audit_eng_scope sc ON sc.eng_id=r.eng_id
       WHERE sc.dept_id=P_DEPT_ID AND r.eng_id=P_ENG_ID AND r.id=P_EVIDENCE_ID;
  END;
  PROCEDURE P_GET_RESPONSE_EVIDENCE(P_USER_ID NUMBER,P_ROLE_ID NUMBER,P_DEPT_ID NUMBER,
    P_CONTEXT_ID NUMBER,P_ENG_ID NUMBER,P_EVIDENCE_ID NUMBER,IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_SCOPE(P_USER_ID,P_ROLE_ID,P_DEPT_ID,P_CONTEXT_ID,P_ENG_ID);
    IF P_ENG_ID IS NULL THEN RAISE_APPLICATION_ERROR(-20152,'Engagement required.'); END IF;
    OPEN IO_CURSOR FOR
      SELECT f.file_data,f.file_name FROM t_au_observations_auditee_evidences f
       JOIN t_au_observation o ON o.id=f.memoid JOIN v_head_audit_eng_scope sc ON sc.eng_id=o.engplanid
       WHERE sc.dept_id=P_DEPT_ID AND o.engplanid=P_ENG_ID AND f.id=P_EVIDENCE_ID;
  END;
END PKG_HEAD_AUDIT_MONITOR;

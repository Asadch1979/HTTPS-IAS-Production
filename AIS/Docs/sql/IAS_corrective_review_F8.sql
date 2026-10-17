-- Read-only corrective preflight. Run statements manually in the IAS schema (F8).
-- No package replacement, historical update, COMMIT or grant is performed here.

-- Resolve the deployed package owner before comparing arguments/overloads.
SELECT owner,synonym_name,table_owner,table_name
  FROM all_synonyms WHERE synonym_name='PKG_RPT';
SELECT owner,package_name,object_name,overload,position,argument_name,in_out,data_type
  FROM all_arguments
 WHERE package_name='PKG_RPT' AND object_name='R_IAS_PARA_POSITION'
 ORDER BY owner,overload,sequence;
-- Expected named IN arguments: P_NO NUMBER, ENT_ID NUMBER, R_ID NUMBER,
-- R_T NUMBER, S_DATE DATE, E_DATE DATE; IO_CURSOR OUT REF CURSOR.
-- Compare the actual signature and cursor aliases; repository PKG_RPT is older.

SELECT name,type,line,position,text FROM user_errors
 WHERE name IN ('PKG_RPT','PKG_AR','PKG_AD','PKG_HD',
                'V_EXEC_KPI_LIFECYCLE','V_EXEC_KPI_SRC_A',
                'PKG_EXEC_DASHBOARD','PKG_TEAM_LEAD_DASHBOARD')
 ORDER BY name,type,sequence;

-- Histories excluded from automatic backfill. Inspect underlying log rows and
-- observation settlements; status-14 logs historically include unsuccessful calls.
SELECT e.eng_id,e.status,
       SUM(CASE WHEN l.status_id=14 THEN 1 ELSE 0 END) AS conclusion_logs,
       SUM(CASE WHEN l.status_id=10 THEN 1 ELSE 0 END) AS reversal_logs,
       MIN(CASE WHEN l.status_id=14 THEN l.created_on END) AS first_conclusion,
       MAX(CASE WHEN l.status_id=14 THEN l.created_on END) AS latest_conclusion
  FROM t_au_plan_eng e JOIN t_au_plan_eng_log l ON l.e_id=e.eng_id
 GROUP BY e.eng_id,e.status
HAVING SUM(CASE WHEN l.status_id=14 THEN 1 ELSE 0 END)>1
    OR SUM(CASE WHEN l.status_id=10 THEN 1 ELSE 0 END)>0
    OR (e.status<>14 AND SUM(CASE WHEN l.status_id=14 THEN 1 ELSE 0 END)>0)
 ORDER BY e.eng_id;

-- Persisted flags and dropped/reversed cases must be reconciled by ID.
-- A source flag may be 1 during active finalization before a PARA is transferred.
SELECT o.id,o.engplanid,o.status,o.stelled_on,o.issue_rectified,
       c.com_id,c.para_status,c.issue_rectified AS copied_issue_rectified
  FROM t_au_observation o LEFT JOIN ais_t_au_post_compliance c
    ON c.ind='A' AND c.new_para_id=o.id
 WHERE (o.status NOT IN (4,9) AND o.issue_rectified<>0)
    OR (c.com_id IS NOT NULL AND c.issue_rectified<>o.issue_rectified)
 ORDER BY o.engplanid,o.id,c.com_id;

-- Seven additive procedures: verify actual argument contracts before integration.
SELECT package_name,object_name,overload,position,argument_name,in_out,data_type
  FROM user_arguments
 WHERE (package_name='PKG_EXEC_DASHBOARD' AND object_name IN
        ('P_GET_ROLE_ENTITY_HEATMAP','P_GET_ROLE_UNIT_COMPARISON','P_GET_ROLE_FOLLOWUPS'))
    OR (package_name='PKG_TEAM_LEAD_DASHBOARD' AND object_name IN
        ('P_GET_SUMMARY','P_GET_ENGAGEMENTS','P_GET_WORKPAPERS','P_GET_OBSERVATIONS'))
 ORDER BY package_name,object_name,overload,sequence;

-- Do NOT mass-update histories above from MAX alone. PKG_AD's closing-reversal
-- path does not log every reopen and old log dates can lose timestamp precision.
-- Back up data, reconcile the approved observation IDs and flags, and prepare an
-- ID-specific DML script with before/after checks for separate approval.

-- Read-only post-deployment validation for PKG_AR.
-- Run as the application schema owner after compiling the revised package.

SET PAGESIZE 500
SET LINESIZE 220

PROMPT === PACKAGE STATUS ===
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name = 'PKG_AR'
 ORDER BY object_type;

PROMPT === COMPILATION ERRORS ===
SELECT type, line, position, text
  FROM user_errors
 WHERE name = 'PKG_AR'
 ORDER BY type, sequence;

PROMPT === OBSERVATION ENTRY DATES AFTER ENGAGEMENT END ===
SELECT COUNT(*) AS invalid_count
  FROM t_au_observation o
  JOIN t_au_plan_eng e
    ON e.eng_id = o.engplanid
 WHERE o.entereddate > e.audit_enddate;

PROMPT === STATUS 8 WITH STALE SETTLEMENT METADATA ===
SELECT COUNT(*) AS invalid_count
  FROM t_au_observation o
 WHERE o.status = 8
   AND (o.stelled_on IS NOT NULL OR o.settled_by IS NOT NULL);

PROMPT === STATUS 9 WITHOUT SETTLEMENT METADATA ===
SELECT COUNT(*) AS invalid_count
  FROM t_au_observation o
 WHERE o.status = 9
   AND (o.stelled_on IS NULL OR o.settled_by IS NULL);

PROMPT === REQUIRED WORKFLOW NUMBERS MISSING ===
SELECT 'SUBMITTED_WITHOUT_MEMO' AS exception_type, COUNT(*) AS invalid_count
  FROM t_au_observation
 WHERE status = 2
   AND memo_number IS NULL
UNION ALL
SELECT 'DRAFT_WITHOUT_DRAFT_PARA', COUNT(*)
  FROM t_au_observation
 WHERE status = 5
   AND draft_para_no IS NULL
UNION ALL
SELECT 'FINAL_WITHOUT_FINAL_PARA', COUNT(*)
  FROM t_au_observation
 WHERE status = 8
   AND final_para_no IS NULL;

PROMPT === NUMBER/DATE PAIR CONTRADICTIONS ===
SELECT 'MEMO_PAIR' AS exception_type, COUNT(*) AS invalid_count
  FROM t_au_observation
 WHERE (memo_number IS NULL AND memo_date IS NOT NULL)
    OR (memo_number IS NOT NULL AND memo_date IS NULL)
UNION ALL
SELECT 'DRAFT_PAIR', COUNT(*)
  FROM t_au_observation
 WHERE (draft_para_no IS NULL AND draft_para_added_on IS NOT NULL)
    OR (draft_para_no IS NOT NULL AND draft_para_added_on IS NULL)
UNION ALL
SELECT 'FINAL_PAIR', COUNT(*)
  FROM t_au_observation
 WHERE (final_para_no IS NULL AND final_para_added_on IS NOT NULL)
    OR (final_para_no IS NOT NULL AND final_para_added_on IS NULL);

PROMPT === DUPLICATE ENGAGEMENT-SCOPED WORKFLOW NUMBERS ===
SELECT 'MEMO' AS number_type, COUNT(*) AS duplicate_groups
  FROM (
        SELECT engplanid, memo_number
          FROM t_au_observation
         WHERE memo_number IS NOT NULL
         GROUP BY engplanid, memo_number
        HAVING COUNT(*) > 1
       )
UNION ALL
SELECT 'DRAFT', COUNT(*)
  FROM (
        SELECT engplanid, draft_para_no
          FROM t_au_observation
         WHERE draft_para_no IS NOT NULL
         GROUP BY engplanid, draft_para_no
        HAVING COUNT(*) > 1
       )
UNION ALL
SELECT 'FINAL', COUNT(*)
  FROM (
        SELECT engplanid, final_para_no
          FROM t_au_observation
         WHERE final_para_no IS NOT NULL
         GROUP BY engplanid, final_para_no
        HAVING COUNT(*) > 1
       );

PROMPT === OBSERVATION/TEXT MEMO MISMATCH ===
SELECT COUNT(*) AS mismatch_count
  FROM t_au_observation o
  JOIN t_au_observation_text t
    ON t.observatsion_id = o.id
 WHERE NVL(o.memo_number, -1) <> NVL(t.memo_number, -1);

PROMPT === POST-COMPLIANCE STATUS/SETTLEMENT MONITORING ===
SELECT 'STATUS8_WITH_SETTLEMENT' AS exception_type, COUNT(*) AS invalid_count
  FROM ais_t_au_post_compliance
 WHERE para_status = 8
   AND (setteled_on IS NOT NULL OR setteled_by IS NOT NULL)
UNION ALL
SELECT 'STATUS9_WITHOUT_SETTLEMENT', COUNT(*)
  FROM ais_t_au_post_compliance
 WHERE para_status = 9
   AND (setteled_on IS NULL OR setteled_by IS NULL);


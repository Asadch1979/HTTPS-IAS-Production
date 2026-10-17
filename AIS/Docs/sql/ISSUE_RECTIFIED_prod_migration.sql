-- ISSUE_RECTIFIED production deployment
-- Required before compiling updated PKG_AR, PKG_AD and PKG_HD package bodies.
-- Oracle 18c/19c. Run in the IAS owning schema with an approved backup.
-- DDL implicitly commits; review DML verification before issuing COMMIT.

DECLARE
    v_exists NUMBER;
BEGIN
    SELECT COUNT(*)
      INTO v_exists
      FROM user_tab_columns
     WHERE table_name = 'T_AU_OBSERVATION'
       AND column_name = 'ISSUE_RECTIFIED';

    IF v_exists = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE T_AU_OBSERVATION ADD (ISSUE_RECTIFIED NUMBER(1) DEFAULT 0 NOT NULL)';
    END IF;

    SELECT COUNT(*)
      INTO v_exists
      FROM user_tab_columns
     WHERE table_name = 'AIS_T_AU_POST_COMPLIANCE'
       AND column_name = 'ISSUE_RECTIFIED';

    IF v_exists = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE AIS_T_AU_POST_COMPLIANCE ADD (ISSUE_RECTIFIED NUMBER(1) DEFAULT 0 NOT NULL)';
    END IF;
END;
/

-- Historical backfill: use the LATEST status-14 conclusion for each
-- concluded engagement, as agreed for the IAS Para Position report.
-- Include historical statuses 4 and 9; ongoing audits remain at default 0.
MERGE INTO t_au_observation target_obs
USING (
    SELECT o.id AS observation_id,
           CASE
             WHEN o.status IN (4, 9)
              AND o.stelled_on IS NOT NULL
              AND o.stelled_on < lc.concluded_on
             THEN 1 ELSE 0
           END AS expected_flag
      FROM t_au_observation o
      JOIN (
          SELECT e_id,
                 MAX(created_on) AS concluded_on
            FROM t_au_plan_eng_log
           WHERE status_id = 14
           GROUP BY e_id
      ) lc ON lc.e_id = o.engplanid
) source_obs
ON (target_obs.id = source_obs.observation_id)
WHEN MATCHED THEN
    UPDATE SET target_obs.issue_rectified = source_obs.expected_flag
     WHERE NVL(target_obs.issue_rectified, -1) <> source_obs.expected_flag;

-- Copy the historical flag into the existing modern post-compliance records.
-- IND = 'O'/'C' records retain the default zero.
MERGE INTO ais_t_au_post_compliance target_para
USING (
    SELECT id AS observation_id, issue_rectified
      FROM t_au_observation
) source_obs
ON (target_para.ind = 'A'
    AND target_para.new_para_id = source_obs.observation_id)
WHEN MATCHED THEN
    UPDATE SET target_para.issue_rectified = source_obs.issue_rectified
     WHERE NVL(target_para.issue_rectified, -1) <> source_obs.issue_rectified;

-- Verification (no COMMIT in this script):
SELECT
    (SELECT COUNT(*) FROM t_au_observation
      WHERE issue_rectified = 1) AS observation_rectified,
    (SELECT COUNT(*) FROM ais_t_au_post_compliance
      WHERE issue_rectified = 1) AS post_compliance_rectified,
    (SELECT COUNT(*)
       FROM ais_t_au_post_compliance c
       JOIN t_au_observation o ON o.id = c.new_para_id
      WHERE c.ind = 'A'
        AND c.issue_rectified <> o.issue_rectified) AS mismatched_flags
FROM dual;

-- Expected: MISMATCHED_FLAGS = 0. Inspect totals before COMMIT.

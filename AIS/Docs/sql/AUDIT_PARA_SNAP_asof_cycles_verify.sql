-- Oracle SQL Developer / read-only verification for Audit Para Snap
-- Critical Observations and Aging & Compliance cycles.
--
-- Manual inclusive reporting period: 01-Jan-2026 .. 20-Oct-2026.
-- This period uses exclusive upper bound 21-Oct-2026.
-- Audited By IDs 112242/112248 correspond to the previously checked
-- two departments, NOT an assumption that these are all FAD departments.
--
-- Historic cycle evidence is sourced from
-- AIS_T_AU_POST_COMPLIANCE_HISTORY.COM_CYCLE/COMMENT_ON/HIST_ID.
-- Historic fallback to 0 occurs ONLY if the CURRENT COM_CYCLE = 0,
-- not if an unknown past value was positive.
-- The result is an estimated historical snapshot: re-openings and
-- incomplete history prevent absolute as-of reconstruction.

WITH date_params AS
(
    SELECT DATE '2026-01-01' AS date_from,
           DATE '2026-10-21' AS date_to
      FROM dual
),
history_asof AS
(
    SELECT h.com_id,
           MAX(h.com_cycle) KEEP
             (DENSE_RANK LAST ORDER BY h.comment_on, h.hist_id)
             AS recorded_cycle,
           MAX(h.comment_on) AS last_movement_on
      FROM ais_t_au_post_compliance_history h
     WHERE h.comment_on < DATE '2026-10-21'
       AND h.com_cycle IS NOT NULL
     GROUP BY h.com_id
),
snapshot AS
(
    SELECT c.com_id,
           c.audited_by,
           c.risk,
           CASE WHEN h.recorded_cycle IS NOT NULL THEN h.recorded_cycle
                WHEN c.com_cycle = 0 THEN 0
           END AS snap_cycle,
           CASE WHEN h.last_movement_on IS NOT NULL THEN
                TRUNC(DATE '2026-10-20') - TRUNC(h.last_movement_on)
           END AS days_since_movement
      FROM ais_t_au_post_compliance c
      CROSS JOIN date_params d
      LEFT JOIN history_asof h ON h.com_id = c.com_id
     WHERE c.audited_by IN (112242,112248)
       AND c.ind IN ('A','O','C')
       AND NOT (c.ind = 'A' AND NVL(c.issue_rectified,0)=1)
       AND c.para_added_on < d.date_to
       AND (c.para_status = 8
            OR (c.para_status = 9 AND c.setteled_on >= d.date_to))
)
SELECT CASE WHEN GROUPING(audited_by)=1 THEN 'SELECTED FAD DEPARTMENTS'
            ELSE TO_CHAR(audited_by) END AS audit_department,
       COUNT(*) AS estimated_open_paras,
       COUNT(CASE WHEN snap_cycle = 0 THEN 1 END) AS zero_cycles,
       COUNT(CASE WHEN snap_cycle BETWEEN 1 AND 3 THEN 1 END) AS one_to_three_cycles,
       COUNT(CASE WHEN snap_cycle > 3 THEN 1 END) AS over_three_cycles,
       COUNT(CASE WHEN snap_cycle IS NULL OR snap_cycle < 0 THEN 1 END)
            AS unknown_cycles,
       COUNT(CASE WHEN risk = 1 THEN 1 END) AS high_risk_open_paras,
       COUNT(CASE WHEN risk = 1 AND snap_cycle IS NOT NULL
                  THEN 1 END) AS critical_with_cycle,
       COUNT(CASE WHEN risk = 1 AND days_since_movement IS NOT NULL
                  THEN 1 END) AS critical_with_movement_date
  FROM snapshot
 GROUP BY ROLLUP(audited_by)
 ORDER BY GROUPING(audited_by) DESC, audited_by;

-- After compiling PKG_EXEC_DASHBOARD.P_GET_SNAP_DISTRIBUTION, compare the
-- Cycle bucket totals with ZERO / ONE_TO_THREE / OVER_THREE / UNKNOWN
-- returned above, for exactly the same date, audited-by and entity scope.
-- The critical cursor also returns DAYS_SINCE_MOVEMENT (an additional column).
-- Current C# does not map that extra column and the critical table's
-- movement cell is hardcoded 'N/A' in audit-para-snap.js. A later frontend
-- change is required to SHOW it; this script makes no frontend change.

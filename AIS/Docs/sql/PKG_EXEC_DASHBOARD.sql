--Views
CREATE OR REPLACE VIEW V_EXEC_ENTITY_SCOPE AS
WITH entity_map AS
(
    SELECT
        entity_id,
        CASE WHEN COUNT(DISTINCT c_type_id) = 1 THEN MAX(c_type_id) END AS c_type_id
    FROM t_auditee_entities_maping
    WHERE NVL(status, 'Y') = 'Y'
    GROUP BY entity_id
),
hierarchy AS
(
    SELECT
        h.entity_id,
        h.reporting_1 AS parent_id,
        h.reporting_1,
        h.reporting_2,
        h.reporting_3,
        h.reporting_4,
        h.reporting_5,
        h.reporting_6
    FROM V_IAS_ENTITY_REPORTING_IDS h
    JOIN entity_map m
      ON m.entity_id = h.entity_id
),
hierarchy_levels AS
(
    SELECT
        h.entity_id,
        x.level_no,
        x.office_id,
        m.c_type_id
    FROM hierarchy h
    CROSS APPLY
    (
        SELECT 0 level_no, h.entity_id office_id FROM dual
        UNION ALL
        SELECT 1, h.reporting_1 FROM dual
        UNION ALL
        SELECT 2, h.reporting_2 FROM dual
        UNION ALL
        SELECT 3, h.reporting_3 FROM dual
        UNION ALL
        SELECT 4, h.reporting_4 FROM dual
        UNION ALL
        SELECT 5, h.reporting_5 FROM dual
        UNION ALL
        SELECT 6, h.reporting_6 FROM dual
    ) x
    LEFT JOIN entity_map m
      ON m.entity_id = x.office_id
    WHERE x.office_id IS NOT NULL
),
office_scope AS
(
    SELECT
        entity_id,

        MAX(CASE WHEN c_type_id = 21 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 21
                     THEN level_no ELSE 99 END)
          AS gm_office_id,

        MAX(CASE WHEN c_type_id = 3 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 3
                     THEN level_no ELSE 99 END)
          AS division_id,

        MAX(CASE WHEN c_type_id = 18 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 18
                     THEN level_no ELSE 99 END)
          AS business_group_id

    FROM hierarchy_levels
    GROUP BY entity_id
)
SELECT
    h.entity_id,
    h.parent_id,

    h.reporting_1 AS reporting_office_id,

    s.gm_office_id,
    s.division_id,
    s.business_group_id,

    h.reporting_1,
    h.reporting_2,
    h.reporting_3,
    h.reporting_4,
    h.reporting_5,
    h.reporting_6

FROM hierarchy h
LEFT JOIN office_scope s
       ON s.entity_id = h.entity_id;
/
CREATE OR REPLACE VIEW V_EXEC_PARA_BASE AS
SELECT
    c.com_id,
    c.old_para_id,
    c.new_para_id,
    c.ind AS para_source,

    c.audit_period,
    c.para_no,
    c.gist_of_paras,

    c.entity_id,
    c.entity_code,
    e.name AS entity_name,
    c.entity_type_id,
    et.entitytypedesc AS entity_type_name,

    s.reporting_office_id,
    ro.name AS reporting_office_name,

    s.reporting_1,
    s.reporting_2,
    s.reporting_3,
    s.reporting_4,
    s.reporting_5,
    s.reporting_6,

    s.gm_office_id,
    gm.name AS gm_office_name,

    s.division_id,
    dv.name AS division_name,

    s.business_group_id,
    bg.name AS business_group_name,

    c.audited_by,
    ab.name AS audited_by_name,

    c.risk AS risk_id,
    CASE c.risk
        WHEN 1 THEN 'High'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'Low'
        ELSE 'Unrated'
    END AS risk_name,

    c.para_status,
    c.com_status,
    c.com_stage,
    NVL(c.com_cycle, 0) AS com_cycle,

    c.para_added_on,
    c.setteled_on,

    h.last_movement_date,

    -- Missing settlement dates and negative/future ages are UNKNOWN,
    -- never misleadingly classified as a valid 0-90-day para.
    CASE
        WHEN c.para_added_on IS NOT NULL
         AND (c.para_status = 8 OR
              (c.para_status = 9 AND c.setteled_on IS NOT NULL))
         AND TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
             >= TRUNC(c.para_added_on)
        THEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
             - TRUNC(c.para_added_on)
    END AS aging_days,

    CASE
        WHEN c.para_added_on IS NULL
          OR c.para_status NOT IN (8, 9) OR c.para_status IS NULL
          OR (c.para_status = 9 AND c.setteled_on IS NULL)
          OR TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
             < TRUNC(c.para_added_on)
        THEN 'UNKNOWN'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 90
            THEN '0-90 DAYS'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 180
            THEN '91-180 DAYS'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 365
            THEN '181-365 DAYS'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 1095
            THEN '1-3 YEARS'
        ELSE 'OVER 3 YEARS'
    END AS aging_bucket,

    CASE
        WHEN h.last_movement_date IS NOT NULL
        THEN TRUNC(SYSDATE) - TRUNC(h.last_movement_date)
    END AS days_since_last_movement,

    CASE
        WHEN c.para_status = 8 THEN 1
        ELSE 0
    END AS is_open,

    CASE
        WHEN c.para_status = 9 THEN 1
        ELSE 0
    END AS is_settled,

    CASE
        WHEN c.para_status = 8
         AND NVL(c.com_cycle,0) = 0
        THEN 1 ELSE 0
    END AS is_zero_cycle,

    CASE
        WHEN c.para_status = 8
         AND NVL(c.com_cycle,0) > 3
        THEN 1 ELSE 0
    END AS is_over_3_cycle,

    CASE
        WHEN c.para_status = 8
         AND c.para_added_on IS NOT NULL
         AND TRUNC(SYSDATE) - TRUNC(c.para_added_on) > 365
        THEN 1 ELSE 0
    END AS is_over_1_year,

    -- ZTBL definition: an OPEN high-risk para (RISK=1) is Critical.
    -- Aging / repeat compliance cycles remain independent indicators.
    CASE WHEN c.para_status = 8 AND c.risk = 1
         THEN 1 ELSE 0 END AS is_critical,

    c.no_of_instances,
    -- Copy made at audit conclusion, NOT a new lifecycle ISSUE event.
    CASE WHEN c.ind = 'A' THEN NVL(c.issue_rectified, 0)
    END AS issue_rectified_copy,
    CASE
      WHEN c.para_added_on IS NULL THEN 'MISSING_ADDED_ON'
      WHEN c.para_status NOT IN (8, 9) OR c.para_status IS NULL
        THEN 'UNKNOWN_STATUS'
      WHEN c.para_status = 9 AND c.setteled_on IS NULL THEN 'MISSING_SETTLEMENT_DATE'
      WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
           < TRUNC(c.para_added_on) THEN 'NEGATIVE_AGE'
      ELSE 'OK'
    END AS aging_quality

FROM ais_t_au_post_compliance c

LEFT JOIN t_auditee_entities e
       ON e.entity_id = c.entity_id

LEFT JOIN t_auditee_ent_types et
       ON et.autid = c.entity_type_id

LEFT JOIN V_EXEC_ENTITY_SCOPE s
       ON s.entity_id = c.entity_id

LEFT JOIN t_auditee_entities ro
       ON ro.entity_id = s.reporting_office_id

LEFT JOIN t_auditee_entities gm
       ON gm.entity_id = s.gm_office_id

LEFT JOIN t_auditee_entities dv
       ON dv.entity_id = s.division_id

LEFT JOIN t_auditee_entities bg
       ON bg.entity_id = s.business_group_id

LEFT JOIN t_auditee_entities ab
       ON ab.entity_id = c.audited_by

LEFT JOIN (
    SELECT com_id,
           MAX(comment_on) AS last_movement_date
    FROM ais_t_au_post_compliance_history
    GROUP BY com_id
) h
       ON h.com_id = c.com_id;
/
CREATE OR REPLACE PACKAGE PKG_EXEC_DASHBOARD AS

  /*
    Dashboard types:
      TOP   = Bank-wide Top Management
      GROUP = Business Group scope
      CCO   = Bank-wide Compliance oversight
      CIA   = Bank-wide Internal Audit oversight

    The IAS backend must derive dashboard type and root entity from the
    authenticated user. This package applies hierarchy-based filtering but
    cannot authenticate privileges from dashboard type alone.
  */

  PROCEDURE P_GET_KPI_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                              P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                              P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                              P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                              P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                              P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                              IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_RISK_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                               P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                               P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                               P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                               P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                               P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                               IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_AGING_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_CYCLE_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_SETTLEMENT_TREND(P_DASHBOARD_TYPE IN VARCHAR2,
                                    P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                    P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                    P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                    P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                    IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_CRITICAL_ITEMS(P_DASHBOARD_TYPE IN VARCHAR2,
                                 P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                 P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                 P_MAX_ROWS       IN NUMBER DEFAULT 100,
                                 P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                 P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                 P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                 IO_CURSOR        OUT SYS_REFCURSOR);

  -------------------------------------------------------------------
  -- Lifecycle KPIs: activity by event date; existing APIs unaffected.
  -- This procedure uses the approved V_EXEC_KPI_LIFECYCLE view.
  -------------------------------------------------------------------
  PROCEDURE P_GET_KPI_LIFECYCLE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL);


  -- CIA issues: both dates optional, all eligible history when omitted.
  PROCEDURE P_GET_CIA_ISSUES(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE DEFAULT NULL,
      P_END_DATE IN DATE DEFAULT NULL,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);


  -------------------------------------------------------------------
  -- CIA Audit Command Center (isolated; existing cursors unchanged).
  -- The selected audit period defines plan and issue-event dates.
  -- Pending response / delayed reports / active teams are CURRENT
  -- bank-wide scoped states across all audit periods.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_EXEC_KPIS(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);

  -- Exactly ten categories, plus total/unmapped diagnostic columns.
  -- Requires T_IAS_CIA_ISSUE_NATURE_MAP (deploy its DDL first).
  PROCEDURE P_GET_CIA_ISSUE_NATURE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);



  -- Development reconciliation of persisted ISSUE_RECTIFIED flags.
  -- One row; no change to existing executive KPI cursor signatures.
  -- Source ISSUE events are authoritative; IND='A' compliance flags are copies.
  PROCEDURE P_GET_CIA_RECTIFICATION_RECON(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);

  -- Audit Para Snap: non-KPI date-aware analysis only.
  -- Nine KPIs MUST use the existing P_GET_KPI_LIFECYCLE.
  PROCEDURE P_GET_SNAP_DISTRIBUTION(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL);
  PROCEDURE P_GET_SNAP_CRITICAL(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      P_MAX_ROWS IN NUMBER DEFAULT 100,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL);

  -- Additional read-only cursors for Group, Division, GM and Region views.
  -- P_ROOT_ENTITY_ID must be a server-authorized hierarchy root.
  PROCEDURE P_GET_ROLE_ENTITY_HEATMAP(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD IN VARCHAR2 DEFAULT NULL,
    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
    P_MAX_ROWS IN NUMBER DEFAULT 250, IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ROLE_UNIT_COMPARISON(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD IN VARCHAR2 DEFAULT NULL,
    IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ROLE_FOLLOWUPS(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD IN VARCHAR2 DEFAULT NULL,
    P_MAX_ROWS IN NUMBER DEFAULT 100, IO_CURSOR OUT SYS_REFCURSOR);

END PKG_EXEC_DASHBOARD;
/
CREATE OR REPLACE PACKAGE BODY PKG_EXEC_DASHBOARD AS

  FUNCTION VALIDATE_REQUEST(P_DASHBOARD_TYPE IN VARCHAR2,
                            P_ENTITY_ID IN NUMBER) RETURN VARCHAR2 AS
    V_TYPE VARCHAR2(20) := UPPER(TRIM(P_DASHBOARD_TYPE));
  BEGIN
    IF V_TYPE IS NULL OR V_TYPE NOT IN ('TOP', 'GROUP', 'CCO', 'CIA', 'RLM') THEN
      RAISE_APPLICATION_ERROR(-20001, 'Unsupported executive dashboard type.');
    END IF;
    IF P_ENTITY_ID IS NOT NULL AND
       (P_ENTITY_ID <= 0 OR P_ENTITY_ID <> TRUNC(P_ENTITY_ID)) THEN
      RAISE_APPLICATION_ERROR(-20002, 'Entity ID must be a positive integer.');
    END IF;
    IF V_TYPE IN ('GROUP','RLM') AND P_ENTITY_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20003, 'Group dashboard needs an authorized entity.');
    END IF;
    RETURN V_TYPE;
  END VALIDATE_REQUEST;

  -------------------------------------------------------------------
  -- 1. KPI SUMMARY
  -------------------------------------------------------------------
  PROCEDURE P_GET_KPI_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                              P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                              P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                              P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                              P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                              P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                              IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN


    OPEN IO_CURSOR FOR
      SELECT COUNT(*) AS total_cases,

             NVL(SUM(p.is_open), 0) AS open_paras,

             NVL(SUM(CASE
                       WHEN p.is_open = 1 AND p.risk_id = 1 THEN
                        1
                       ELSE
                        0
                     END),
                 0) AS high_risk_paras,

             NVL(SUM(p.is_zero_cycle), 0) AS zero_cycle_paras,

             NVL(SUM(p.is_over_3_cycle), 0) AS over_3_cycle_paras,

             NVL(SUM(p.is_over_1_year), 0) AS over_1_year_paras,

             NVL(SUM(p.is_critical), 0) AS critical_items,

             NVL(SUM(p.is_settled), 0) AS settled_total,

             NVL(SUM(CASE
                       WHEN p.is_settled = 1 AND
                            p.setteled_on >= TRUNC(SYSDATE, 'MM') AND
                            p.setteled_on < ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1) THEN
                        1
                       ELSE
                        0
                     END),
                 0) AS settled_mtd,

             ROUND(CASE
                     WHEN NVL(SUM(p.is_open), 0) + NVL(SUM(p.is_settled), 0) = 0 THEN
                      0
                     ELSE
                      NVL(SUM(p.is_settled), 0) /
                      (NVL(SUM(p.is_open), 0) + NVL(SUM(p.is_settled), 0)) * 100
                   END,
                   2) AS settlement_ratio,

             -- Monthly settlement performance, deliberately separate from
             -- lifetime SETTLEMENT_RATIO. Workload = reconstructed open at
             -- month's start + newly added during this month.
             -- Current master status cannot reconstruct all historical
             -- reopenings; verify the denominator in production.
             ROUND(
               100 * NVL(SUM(CASE WHEN p.is_settled = 1
                          AND p.setteled_on >= TRUNC(SYSDATE,'MM')
                          AND p.setteled_on < ADD_MONTHS(TRUNC(SYSDATE,'MM'),1)
                          THEN 1 ELSE 0 END),0)
               / NULLIF(
                   NVL(SUM(CASE WHEN p.para_added_on < TRUNC(SYSDATE,'MM')
                           AND (p.is_open=1 OR
                                (p.is_settled=1 AND p.setteled_on >= TRUNC(SYSDATE,'MM')))
                           THEN 1 ELSE 0 END),0)
                   + NVL(SUM(CASE WHEN p.para_added_on >= TRUNC(SYSDATE,'MM')
                             AND p.para_added_on < ADD_MONTHS(TRUNC(SYSDATE,'MM'),1)
                             AND (p.is_open=1 OR p.is_settled=1)
                             THEN 1 ELSE 0 END),0),0)
               ,2) AS settlement_mtd_rate

        FROM V_EXEC_PARA_BASE p

       WHERE (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))

         AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)));

  END P_GET_KPI_SUMMARY;

  -------------------------------------------------------------------
  -- 2. OPEN PARAS BY RISK
  -------------------------------------------------------------------
  PROCEDURE P_GET_RISK_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                               P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                               P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                               P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                               P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                               P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                               IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN


    OPEN IO_CURSOR FOR

      WITH risk_categories AS
       (SELECT 1 AS risk_id, 'High' AS risk_name, 1 AS display_order
          FROM dual
        UNION ALL
        SELECT 2, 'Medium', 2
          FROM dual
        UNION ALL
        SELECT 3, 'Low', 3
          FROM dual
        UNION ALL
        SELECT 0, 'Unrated', 4
          FROM dual),
      risk_data AS
       (SELECT CASE
                 WHEN p.risk_id IN (1, 2, 3) THEN
                  p.risk_id
                 ELSE
                  0
               END AS risk_id,
               COUNT(*) AS para_count

          FROM V_EXEC_PARA_BASE p

         WHERE p.is_open = 1

           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))

           AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))

         GROUP BY CASE
                    WHEN p.risk_id IN (1, 2, 3) THEN
                     p.risk_id
                    ELSE
                     0
                  END),
      result_data AS
       (SELECT r.risk_id,
               r.risk_name,
               NVL(d.para_count, 0) AS para_count,
               r.display_order
          FROM risk_categories r
          LEFT JOIN risk_data d
            ON d.risk_id = r.risk_id)

      SELECT risk_id,
             risk_name,
             para_count,

             ROUND(CASE
                     WHEN SUM(para_count) OVER() = 0 THEN
                      0
                     ELSE
                      para_count / SUM(para_count) OVER() * 100
                   END,
                   2) AS percentage

        FROM result_data
       ORDER BY display_order;

  END P_GET_RISK_SUMMARY;

  -------------------------------------------------------------------
  -- 3. OPEN PARAS BY AGING
  -------------------------------------------------------------------
  PROCEDURE P_GET_AGING_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN


    OPEN IO_CURSOR FOR

      WITH age_categories AS
       (SELECT '0_90' AS bucket_code,
               '0-90 DAYS' AS bucket_name,
               1 AS display_order
          FROM dual

        UNION ALL
        SELECT '91_180', '91-180 DAYS', 2
          FROM dual

        UNION ALL
        SELECT '181_365', '181-365 DAYS', 3
          FROM dual

        UNION ALL
        SELECT '1_3_YEARS', '1-3 YEARS', 4
          FROM dual

        UNION ALL
        SELECT 'OVER_3_YEARS', 'OVER 3 YEARS', 5
          FROM dual

        UNION ALL
        SELECT 'UNKNOWN', 'UNKNOWN', 6
          FROM dual),
      age_data AS
       (SELECT p.aging_bucket,

               COUNT(*) AS para_count,

               SUM(CASE
                     WHEN p.risk_id = 1 THEN
                      1
                     ELSE
                      0
                   END) AS high_risk_count

          FROM V_EXEC_PARA_BASE p

         WHERE p.is_open = 1

           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))

           AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))

         GROUP BY p.aging_bucket)

      SELECT a.bucket_code,
             a.bucket_name,
             NVL(d.para_count, 0) AS para_count,
             NVL(d.high_risk_count, 0) AS high_risk_count

        FROM age_categories a
        LEFT JOIN age_data d
          ON d.aging_bucket = a.bucket_name

       ORDER BY a.display_order;

  END P_GET_AGING_SUMMARY;

  -------------------------------------------------------------------
  -- 4. OPEN PARAS BY COMPLIANCE CYCLE
  -------------------------------------------------------------------
  PROCEDURE P_GET_CYCLE_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN


    ---------------------------------------------------------------
    -- CCO and CIA require finer cycle breakup
    ---------------------------------------------------------------
    IF V_TYPE IN ('CCO', 'CIA') THEN

      OPEN IO_CURSOR FOR

        WITH cycle_categories AS
         (SELECT 'ZERO' AS bucket_code,
                 'Cycle 0' AS bucket_name,
                 1 AS display_order
            FROM dual

          UNION ALL
          SELECT 'ONE', 'Cycle 1', 2
            FROM dual

          UNION ALL
          SELECT 'TWO', 'Cycle 2', 3
            FROM dual

          UNION ALL
          SELECT 'THREE', 'Cycle 3', 4
            FROM dual

          UNION ALL
          SELECT 'FOUR_FIVE', 'Cycle 4-5', 5
            FROM dual

          UNION ALL
          SELECT 'OVER_FIVE', 'Over 5 Cycles', 6
            FROM dual),
        cycle_data AS
         (SELECT CASE
                   WHEN p.com_cycle = 0 THEN
                    'ZERO'
                   WHEN p.com_cycle = 1 THEN
                    'ONE'
                   WHEN p.com_cycle = 2 THEN
                    'TWO'
                   WHEN p.com_cycle = 3 THEN
                    'THREE'
                   WHEN p.com_cycle BETWEEN 4 AND 5 THEN
                    'FOUR_FIVE'
                   ELSE
                    'OVER_FIVE'
                 END AS bucket_code,

                 COUNT(*) AS para_count,

                 SUM(CASE
                       WHEN p.risk_id = 1 THEN
                        1
                       ELSE
                        0
                     END) AS high_risk_count

            FROM V_EXEC_PARA_BASE p

           WHERE p.is_open = 1

             AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
             AND (P_ENTITY_ID IS NULL OR
                  p.entity_id = P_ENTITY_ID OR
                  p.reporting_1 = P_ENTITY_ID OR
                  p.reporting_2 = P_ENTITY_ID OR
                  p.reporting_3 = P_ENTITY_ID OR
                  p.reporting_4 = P_ENTITY_ID OR
                  p.reporting_5 = P_ENTITY_ID OR
                  p.reporting_6 = P_ENTITY_ID)
           GROUP BY CASE
                      WHEN p.com_cycle = 0 THEN
                       'ZERO'
                      WHEN p.com_cycle = 1 THEN
                       'ONE'
                      WHEN p.com_cycle = 2 THEN
                       'TWO'
                      WHEN p.com_cycle = 3 THEN
                       'THREE'
                      WHEN p.com_cycle BETWEEN 4 AND 5 THEN
                       'FOUR_FIVE'
                      ELSE
                       'OVER_FIVE'
                    END)

        SELECT c.bucket_code,
               c.bucket_name,
               NVL(d.para_count, 0) AS para_count,
               NVL(d.high_risk_count, 0) AS high_risk_count

          FROM cycle_categories c
          LEFT JOIN cycle_data d
            ON d.bucket_code = c.bucket_code

         ORDER BY c.display_order;

      ---------------------------------------------------------------
      -- TOP and GROUP use simpler management breakup
      ---------------------------------------------------------------
    ELSE

      OPEN IO_CURSOR FOR

        WITH cycle_categories AS
         (SELECT 'ZERO' AS bucket_code,
                 'No Compliance / Cycle 0' AS bucket_name,
                 1 AS display_order
            FROM dual

          UNION ALL
          SELECT 'ONE', 'Cycle 1', 2
            FROM dual

          UNION ALL
          SELECT 'TWO_THREE', 'Cycle 2-3', 3
            FROM dual

          UNION ALL
          SELECT 'OVER_THREE', 'Over 3 Cycles', 4
            FROM dual),
        cycle_data AS
         (SELECT CASE
                   WHEN p.com_cycle = 0 THEN
                    'ZERO'
                   WHEN p.com_cycle = 1 THEN
                    'ONE'
                   WHEN p.com_cycle BETWEEN 2 AND 3 THEN
                    'TWO_THREE'
                   ELSE
                    'OVER_THREE'
                 END AS bucket_code,

                 COUNT(*) AS para_count,

                 SUM(CASE
                       WHEN p.risk_id = 1 THEN
                        1
                       ELSE
                        0
                     END) AS high_risk_count

            FROM V_EXEC_PARA_BASE p

           WHERE p.is_open = 1

             AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))

             AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))

           GROUP BY CASE
                      WHEN p.com_cycle = 0 THEN
                       'ZERO'
                      WHEN p.com_cycle = 1 THEN
                       'ONE'
                      WHEN p.com_cycle BETWEEN 2 AND 3 THEN
                       'TWO_THREE'
                      ELSE
                       'OVER_THREE'
                    END)

        SELECT c.bucket_code,
               c.bucket_name,
               NVL(d.para_count, 0) AS para_count,
               NVL(d.high_risk_count, 0) AS high_risk_count

          FROM cycle_categories c
          LEFT JOIN cycle_data d
            ON d.bucket_code = c.bucket_code

         ORDER BY c.display_order;

    END IF;

  END P_GET_CYCLE_SUMMARY;


  -------------------------------------------------------------------
  -- 5. SETTLED PARAS: last 12 calendar months (including zero months)
  -------------------------------------------------------------------
  PROCEDURE P_GET_SETTLEMENT_TREND(P_DASHBOARD_TYPE IN VARCHAR2,
                                    P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                    P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                    P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                    P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                    IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
    OPEN IO_CURSOR FOR
      WITH months AS
       (SELECT ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1 - LEVEL) month_start
          FROM dual
        CONNECT BY LEVEL <= 12),
      month_counts AS
       (SELECT TRUNC(p.setteled_on, 'MM') month_start,
               COUNT(*) AS settled_paras,
               SUM(CASE WHEN p.risk_id = 1 THEN 1 ELSE 0 END) AS high_risk_settled
          FROM V_EXEC_PARA_BASE p
         WHERE p.is_settled = 1
           AND p.setteled_on >= ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -11)
           AND p.setteled_on < ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1)
           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
           AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
             OR (P_ENTITY_ID IS NOT NULL AND
                 (p.entity_id = P_ENTITY_ID OR
                  p.reporting_1 = P_ENTITY_ID OR
                  p.reporting_2 = P_ENTITY_ID OR
                  p.reporting_3 = P_ENTITY_ID OR
                  p.reporting_4 = P_ENTITY_ID OR
                  p.reporting_5 = P_ENTITY_ID OR
                  p.reporting_6 = P_ENTITY_ID)))
         GROUP BY TRUNC(p.setteled_on, 'MM'))
      SELECT TO_CHAR(m.month_start, 'YYYY-MM') AS settlement_month,
             NVL(c.settled_paras, 0) AS settled_paras,
             NVL(c.high_risk_settled, 0) AS high_risk_settled
        FROM months m
        LEFT JOIN month_counts c ON c.month_start = m.month_start
       ORDER BY m.month_start;
  END P_GET_SETTLEMENT_TREND;


  -------------------------------------------------------------------
  -- 6. CRITICAL OPEN PARAS: bounded detail for executive drill-down
  -------------------------------------------------------------------
  PROCEDURE P_GET_CRITICAL_ITEMS(P_DASHBOARD_TYPE IN VARCHAR2,
                                 P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                 P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                 P_MAX_ROWS       IN NUMBER DEFAULT 100,
                                 P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                 P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                 P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                 IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
    IF P_MAX_ROWS IS NULL OR P_MAX_ROWS < 1 OR P_MAX_ROWS > 500 OR
       P_MAX_ROWS <> TRUNC(P_MAX_ROWS) THEN
      RAISE_APPLICATION_ERROR(-20004, 'Maximum rows must be between 1 and 500.');
    END IF;

    OPEN IO_CURSOR FOR
      SELECT *
        FROM (
          SELECT p.com_id,
                 p.entity_id,
                 p.entity_name,
                 p.division_id,
                 p.division_name,
                 p.business_group_id,
                 p.business_group_name,
                 p.audit_period,
                 p.para_no,
                 p.gist_of_paras,
                 p.risk_id,
                 p.risk_name,
                 p.com_cycle,
                 p.para_added_on,
                 p.aging_days,
                 p.days_since_last_movement
            FROM V_EXEC_PARA_BASE p
           WHERE p.is_open = 1
             AND p.is_critical = 1
             AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
             AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))
           ORDER BY CASE WHEN p.risk_id = 1 THEN 0 ELSE 1 END,
                    p.aging_days DESC NULLS LAST,
                    p.com_id
        )
       WHERE ROWNUM <= P_MAX_ROWS;
  END P_GET_CRITICAL_ITEMS;

  -------------------------------------------------------------------
  -- Lifecycle / At-a-Glance KPI preview.
  --
  -- Four activity counts use inclusive Start/End calendar dates
  -- and the tested Source A/O/C lifecycle business rules.
  -- Issue Rectified is sourced from post-compliance ISSUE_RECTIFIED=1.
  -- ISSUE_DROPPED is NULL: never-transferred drops cannot be reconstructed
  -- from the post-compliance register alone.
  --
  -- Historical opening/closing balances and associated end-date risk,
  -- aging and cycle counts are DATE-BASED ESTIMATES (E). A historical
  -- source status ledger is not complete enough to certify reopens,
  -- transfers, and historic changes in risk/COM_CYCLE.
  -- For End Date=today, closing snapshots use the current-state flags (Y).
  -- Do NOT mark older snapshots Y merely because they have a count.
  -- Estimate algorithm follows PKG_EXEC_REPORTS close-on convention,
  -- but uses approved lifecycle source A/O/C exclusions and scope.
  -- For modern source A, a para is not considered open before the
  -- known STATUS=14 conclusion event, even if PARA_ADDED_ON is copied
  -- from the much earlier observation ENTEREDDATE. When missing,
  -- the date-based estimate falls back to PARA_ADDED_ON.
  -------------------------------------------------------------------
  PROCEDURE P_GET_KPI_LIFECYCLE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
    V_TODAY_SNAPSHOT NUMBER(1);
  BEGIN
    IF P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR TRUNC(P_START_DATE) > TRUNC(P_END_DATE)
       OR TRUNC(P_END_DATE) >= DATE '9999-12-31' THEN
      RAISE_APPLICATION_ERROR(-20005,
        'Both KPI reporting dates are required; Start Date cannot exceed End Date.');
    END IF;

    V_FROM := TRUNC(P_START_DATE);
    V_TO_EXCLUSIVE := TRUNC(P_END_DATE) + 1;
    IF TRUNC(P_END_DATE) > TRUNC(SYSDATE) THEN
      RAISE_APPLICATION_ERROR(-20006, 'KPI End Date cannot be in the future.');
    END IF;

    V_TODAY_SNAPSHOT := CASE
      WHEN TRUNC(P_END_DATE) = TRUNC(SYSDATE) THEN 1 ELSE 0 END;

    IF P_SNAP_DIVISION_CODE IS NOT NULL THEN
      PKG_AUDIT_SNAP_SCOPE.REQUIRE_SELECTION(
        P_SNAP_DIVISION_CODE,P_AUDITED_BY_ID);
    END IF;
    OPEN IO_CURSOR FOR
      SELECT
        NVL(SUM(CASE
          WHEN k.record_type = 'ISSUE'
           AND k.issue_eligible = 1
           AND k.issue_identified_on >= V_FROM
           AND k.issue_identified_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS issue_identified,

        -- Rectified KPI: ISSUE_RECTIFIED=1, recognized in the
        -- original PARA_ADDED_ON issue cohort. ISSUE_RECTIFIED_ON is
        -- a compatibility alias for that date, not SETTELED_ON.
        -- No original observation status is required here.
        NVL(SUM(CASE
          WHEN k.record_type = 'ISSUE'
           AND k.issue_eligible = 1
           AND k.issue_rectified_on >= V_FROM
           AND k.issue_rectified_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS issue_rectified,

        -- The post-compliance register does not contain status-23
        -- observations never transferred at conclusion. Return unknown,
        -- not a misleading zero. Alias unchanged for existing consumers.
        CAST(NULL AS NUMBER) AS issue_dropped,

        -- Para Added: include status 8 (open) and status 9 (settled)
        -- if their PARA_ADDED_ON falls within the selected period.
        -- Same-period post-report settlements remain additions and
        -- are counted separately by the Paras Settled activity KPI.
        -- Modern pre-conclusion ISSUE_RECTIFIED=1 exclusions are
        -- already enforced in k.para_added_on.
        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.para_added_on >= V_FROM
           AND k.para_added_on < V_TO_EXCLUSIVE
           AND (k.para_status = 8
                OR (k.para_status = 9
                    AND k.excluded_pre_finalization = 0))
          THEN 1 ELSE 0 END), 0) AS para_added,

        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.para_settled_on >= V_FROM
           AND k.para_settled_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS paras_settled,

        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.excluded_pre_finalization = 1
           AND k.raw_para_added_on >= V_FROM
           AND k.raw_para_added_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS excluded_para_additions,

        -- Opening: outstanding immediately before selected Start Date.
        -- Historical ESTIMATE based on current master status/latest
        -- SETTELED_ON; cannot reconstruct unlogged reopenings.
        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.para_added_on < V_FROM
           AND (k.source_ind <> 'A'
                OR k.report_concluded_on IS NULL
                OR k.report_concluded_on < V_FROM)
           AND (k.para_status = 8
                OR (k.para_status = 9
                    AND k.para_settled_on >= V_FROM))
          THEN 1 ELSE 0 END), 0) AS opening_open_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_open ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
            THEN 1 ELSE 0 END), 0)
        END AS closing_open_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_high_risk_open ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
             AND k.risk_id = 1
            THEN 1 ELSE 0 END), 0)
        END AS high_risk_open_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_over_one_year ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
             AND TRUNC(P_END_DATE) - TRUNC(k.para_added_on) > 365
            THEN 1 ELSE 0 END), 0)
        END AS over_1_year_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_over_three_cycles ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
             AND NVL(k.com_cycle, 0) > 3
            THEN 1 ELSE 0 END), 0)
        END AS over_3_compliance_cycles,

        V_FROM AS start_date,
        V_TO_EXCLUSIVE - 1 AS end_date,
        -- E=Estimated (not verified); Y=verified current-state only.
        -- Keep existing column names for API compatibility.
        'E' AS historical_opening_verified,
        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN 'Y' ELSE 'E' END
          AS closing_asof_verified

      FROM V_EXEC_KPI_LIFECYCLE k
      WHERE (P_ENTITY_TYPE_ID IS NULL OR k.entity_type_id = P_ENTITY_TYPE_ID)
        AND (P_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_AUDITED_BY_ID)
           AND (P_SNAP_DIVISION_CODE IS NULL
                OR k.audited_by_id = (CASE P_SNAP_DIVISION_CODE
                        WHEN 'FAD' THEN 112243
                        WHEN 'MAD' THEN 113254 END)
                OR EXISTS
                  (SELECT 1
                     FROM V_IAS_ENTITY_REPORTING_NAMES h
                    WHERE h.ENTITY_ID = k.audited_by_id
                      AND (CASE P_SNAP_DIVISION_CODE
                        WHEN 'FAD' THEN 112243
                        WHEN 'MAD' THEN 113254 END) IN
                          (h.REPORTING_1,h.REPORTING_2,h.REPORTING_3,
                           h.REPORTING_4,h.REPORTING_5,h.REPORTING_6)))
        AND (P_REPORTING_OFFICE_ID IS NULL
             OR k.entity_id = P_REPORTING_OFFICE_ID
             OR P_REPORTING_OFFICE_ID IN
                (k.reporting_1, k.reporting_2, k.reporting_3,
                 k.reporting_4, k.reporting_5, k.reporting_6))
        AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
             OR (P_ENTITY_ID IS NOT NULL
                 AND (k.entity_id = P_ENTITY_ID
                      OR k.reporting_1 = P_ENTITY_ID
                      OR k.reporting_2 = P_ENTITY_ID
                      OR k.reporting_3 = P_ENTITY_ID
                      OR k.reporting_4 = P_ENTITY_ID
                      OR k.reporting_5 = P_ENTITY_ID
                      OR k.reporting_6 = P_ENTITY_ID)));
  END P_GET_KPI_LIFECYCLE;


  -------------------------------------------------------------------
  -- CIA-only ISSUE event counts. No dates = all eligible historical
  -- observations within the authenticated authorized IAS scope.
  -- Issue identified/rectified use their own event date and may differ.
  -- Status-23 drops never transferred at conclusion are unavailable.
  -- The source flag distinguishes rectification from para settlement.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_ISSUES(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE DEFAULT NULL,
      P_END_DATE IN DATE DEFAULT NULL,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
  BEGIN
    IF V_TYPE <> 'CIA' THEN
      RAISE_APPLICATION_ERROR(-20007, 'Issue count procedure requires CIA perspective.');
    END IF;
    IF (P_START_DATE IS NULL AND P_END_DATE IS NOT NULL)
       OR (P_START_DATE IS NOT NULL AND P_END_DATE IS NULL)
       OR (P_START_DATE IS NOT NULL AND
           TRUNC(P_START_DATE) > TRUNC(P_END_DATE)) THEN
      RAISE_APPLICATION_ERROR(-20008,
        'CIA issue dates must both be omitted or form a valid Start/End Date range.');
    END IF;
    IF P_START_DATE IS NOT NULL THEN
      V_FROM := TRUNC(P_START_DATE);
      V_TO_EXCLUSIVE := TRUNC(P_END_DATE) + 1;
    END IF;

    OPEN IO_CURSOR FOR
      SELECT NVL(SUM(CASE
                 WHEN k.issue_eligible = 1
                  AND k.issue_identified_on IS NOT NULL
                  AND (V_FROM IS NULL OR
                      (k.issue_identified_on >= V_FROM AND
                       k.issue_identified_on < V_TO_EXCLUSIVE))
                 THEN 1 ELSE 0 END), 0) AS issue_identified,
             NVL(SUM(CASE
                 WHEN k.issue_eligible = 1
                  AND k.issue_rectified_on IS NOT NULL
                  AND (V_FROM IS NULL OR
                      (k.issue_rectified_on >= V_FROM AND
                       k.issue_rectified_on < V_TO_EXCLUSIVE))
                 THEN 1 ELSE 0 END), 0) AS issue_rectified
        FROM V_EXEC_KPI_LIFECYCLE k
       WHERE k.record_type = 'ISSUE'
         AND (P_ENTITY_TYPE_ID IS NULL OR k.entity_type_id = P_ENTITY_TYPE_ID)
         AND (P_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_AUDITED_BY_ID)
         AND (P_REPORTING_OFFICE_ID IS NULL
              OR k.entity_id = P_REPORTING_OFFICE_ID
              OR P_REPORTING_OFFICE_ID IN
                 (k.reporting_1, k.reporting_2, k.reporting_3,
                  k.reporting_4, k.reporting_5, k.reporting_6))
         AND (P_ENTITY_ID IS NULL OR
              (k.entity_id = P_ENTITY_ID OR P_ENTITY_ID IN
                 (k.reporting_1, k.reporting_2, k.reporting_3,
                  k.reporting_4, k.reporting_5, k.reporting_6)));
  END P_GET_CIA_ISSUES;


  -------------------------------------------------------------------
  -- CIA Audit Command Center: shared period & perspective validation.
  -- Authorization is also mandatory in the authenticated IAS API.
  -------------------------------------------------------------------
  PROCEDURE RESOLVE_CIA_COMMAND_PERIOD(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_FROM OUT DATE,
      P_TO_EXCLUSIVE OUT DATE) AS
    V_TYPE VARCHAR2(20);
  BEGIN
    V_TYPE := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    IF V_TYPE <> 'CIA' THEN
      RAISE_APPLICATION_ERROR(-20041,
        'CIA Command Center procedures require the CIA perspective.');
    END IF;
    IF P_AUDIT_PERIOD_ID IS NULL OR P_AUDIT_PERIOD_ID <= 0
       OR P_AUDIT_PERIOD_ID <> TRUNC(P_AUDIT_PERIOD_ID) THEN
      RAISE_APPLICATION_ERROR(-20042,
        'Select a valid numeric audit period ID.');
    END IF;

    SELECT TRUNC(p.start_date), TRUNC(p.end_date) + 1
      INTO P_FROM, P_TO_EXCLUSIVE
      FROM t_au_period p
     WHERE p.auditperiodid = P_AUDIT_PERIOD_ID;

    IF P_FROM IS NULL OR P_TO_EXCLUSIVE IS NULL
       OR P_FROM >= P_TO_EXCLUSIVE THEN
      RAISE_APPLICATION_ERROR(-20043, 'Audit period dates are invalid.');
    END IF;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RAISE_APPLICATION_ERROR(-20044, 'Audit period was not found.');
  END RESOLVE_CIA_COMMAND_PERIOD;

  -------------------------------------------------------------------
  -- Reconcile rectification at conclusion without double counting events.
  -- V_EXEC_ISSUE_RECTIFIED_RECON aggregates IND='A' post-compliance copies
  -- before joining, preserving one observation per issue.
  -- Compile that view before compiling this package body.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_RECTIFICATION_RECON(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
  BEGIN
    RESOLVE_CIA_COMMAND_PERIOD(
      P_DASHBOARD_TYPE, P_ENTITY_ID, P_AUDIT_PERIOD_ID,
      V_FROM, V_TO_EXCLUSIVE);

    OPEN IO_CURSOR FOR
      SELECT P_AUDIT_PERIOD_ID AS audit_period_id,
             NVL(SUM(CASE
               WHEN r.rectified_before_conclusion = 1
                AND r.issue_rectified_on >= V_FROM
                AND r.issue_rectified_on < V_TO_EXCLUSIVE
               THEN 1 ELSE 0 END), 0) AS issue_rectified,
             NVL(SUM(CASE
               WHEN r.post_copy_count > 0 AND r.min_post_rectified = 1
                AND r.max_post_rectified = 1
               THEN 1 ELSE 0 END), 0) AS post_rectified_observations,
             NVL(SUM(CASE
               WHEN r.post_copy_status = 'MISMATCH' THEN 1 ELSE 0 END), 0)
               AS mismatch_observations,
             NVL(SUM(CASE
               WHEN r.post_copy_status = 'INCONSISTENT_COPIES'
               THEN 1 ELSE 0 END), 0) AS inconsistent_copy_observations,
             NVL(SUM(CASE
               WHEN r.engagement_status = 14
                AND r.observation_status IN (8, 9)
                AND r.post_copy_count = 0
               THEN 1 ELSE 0 END), 0) AS final_observations_not_transferred,
             NVL(SUM(r.post_copy_count), 0) AS modern_post_copy_rows,
             V_FROM AS audit_period_from,
             V_TO_EXCLUSIVE - 1 AS audit_period_to
        FROM V_EXEC_ISSUE_RECTIFIED_RECON r
        LEFT JOIN V_EXEC_ENTITY_SCOPE s ON s.entity_id = r.entity_id
       WHERE r.audit_period_id = P_AUDIT_PERIOD_ID
         AND (P_ENTITY_TYPE_ID IS NULL
              OR r.entity_type_id = P_ENTITY_TYPE_ID)
         AND (P_AUDITED_BY_ID IS NULL
              OR r.audited_by_id = P_AUDITED_BY_ID)
         AND (P_REPORTING_OFFICE_ID IS NULL
              OR r.entity_id = P_REPORTING_OFFICE_ID
              OR P_REPORTING_OFFICE_ID IN
                 (s.reporting_1,s.reporting_2,s.reporting_3,
                  s.reporting_4,s.reporting_5,s.reporting_6))
         AND (P_ENTITY_ID IS NULL OR r.entity_id = P_ENTITY_ID
              OR P_ENTITY_ID IN
                 (s.reporting_1,s.reporting_2,s.reporting_3,
                  s.reporting_4,s.reporting_5,s.reporting_6));
  END P_GET_CIA_RECTIFICATION_RECON;

  -------------------------------------------------------------------
  -- CIA EXECUTIVE KPIs
  --
  -- Planned/Audited: DISTINCT planned entities (statuses 1,2);
  --                  concluded by status-14 engagement log/current.
  -- Delayed reports: historical IAS report proxy: engagement stages
  --                  4..13, audit end +15 days, no status-14 log.
  -- Fraud A1:        final observation statuses 8,9 with ANNEX=1,
  --                  ENTEREDDATE inside selected audit period dates.
  -- Awaiting reply:  T_AU_OBSERVATION.STATUS=2, current state,
  --                  includes outstanding responses from prior years.
  -- Active teams:    distinct assigned teams in active stages 4..11,
  --                  work commenced and audit not concluded, all years.
  --
  -- NOTE: Delay SLA and active stage range must be reconciled with
  -- actual IAS workflow before front-end publication. This procedure
  -- never alters other executive KPI procedures or perspectives.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_EXEC_KPIS(
    P_DASHBOARD_TYPE IN VARCHAR2,
    P_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD_ID IN NUMBER,
    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
    P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
    P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
    IO_CURSOR OUT SYS_REFCURSOR
) AS
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
BEGIN
    -- Only the Audited By department controls audit portfolio scope.
    -- Retain obsolete parameters temporarily for C# compatibility.
    IF P_ENTITY_ID IS NOT NULL
       OR P_ENTITY_TYPE_ID IS NOT NULL
       OR P_REPORTING_OFFICE_ID IS NOT NULL THEN
        RAISE_APPLICATION_ERROR(
            -20048,
            'CIA KPI supports Audited By filtering only.'
        );
    END IF;

    RESOLVE_CIA_COMMAND_PERIOD(
        P_DASHBOARD_TYPE,
        NULL,
        P_AUDIT_PERIOD_ID,
        V_FROM,
        V_TO_EXCLUSIVE
    );

    OPEN IO_CURSOR FOR
    WITH plan_scope AS
    (
        SELECT p.id AS plan_id,
               p.entity_id,
               p.auditedby AS auditby_id
        FROM t_au_plan p
        INNER JOIN v_exec_entity_scope s
            ON s.entity_id = p.auditedby
        WHERE p.auditperiodid = P_AUDIT_PERIOD_ID
          AND p.status IN (1, 2)
          AND s.parent_id IN (112243, 113254)
          AND (
              P_AUDITED_BY_ID IS NULL
              OR s.entity_id = P_AUDITED_BY_ID
          )
    ),
    engagement_scope AS
    (
        SELECT e.eng_id,
               e.plan_id,
               e.period_id,
               e.entity_id,
               e.auditby_id,
               e.status,
               e.team_id,
               e.audit_startdate,
               e.audit_enddate,
               e.operation_startdate
        FROM t_au_plan_eng e
        INNER JOIN t_auditee_entities a
            ON a.entity_id = e.entity_id
        INNER JOIN v_exec_entity_scope s
            ON s.entity_id = e.auditby_id
        WHERE s.parent_id IN (112243, 113254)
          AND (
              P_AUDITED_BY_ID IS NULL
              OR s.entity_id = P_AUDITED_BY_ID
          )
    ),
    completed_entities AS
    (
        SELECT DISTINCT e.entity_id
        FROM engagement_scope e
        WHERE e.entity_id IS NOT NULL
          AND e.period_id = P_AUDIT_PERIOD_ID
          AND (
              e.status = 14
              OR EXISTS
              (
                  SELECT 1
                  FROM t_au_plan_eng_log l
                  WHERE l.e_id = e.eng_id
                    AND l.status_id = 14
              )
          )
    ),
    coverage AS
    (
        SELECT COUNT(DISTINCT p.entity_id)
                   AS planned_entities,
               COUNT(DISTINCT
                   CASE
                       WHEN ce.entity_id IS NOT NULL
                       THEN p.entity_id
                   END
               ) AS audited_entities
        FROM plan_scope p
        LEFT JOIN completed_entities ce
            ON ce.entity_id = p.entity_id
    ),
    delayed AS
    (
        SELECT COUNT(DISTINCT e.eng_id)
                   AS delayed_audit_reports
        FROM engagement_scope e
        WHERE e.status BETWEEN 4 AND 13
          AND e.audit_enddate + 15 < TRUNC(SYSDATE)
          AND NOT EXISTS
          (
              SELECT 1
              FROM t_au_plan_eng_log l
              WHERE l.e_id = e.eng_id
                AND l.status_id = 14
          )
    ),
    fraud AS
    (
        SELECT COUNT(DISTINCT o.id)
                   AS fraud_a1_paras
        FROM t_au_observation o
        INNER JOIN engagement_scope e
            ON e.eng_id = o.engplanid
        WHERE o.annex = 1
          AND o.status IN (8, 9)
          AND e.status = 14
          AND e.period_id = P_AUDIT_PERIOD_ID
          AND o.entereddate >= V_FROM
          AND o.entereddate < V_TO_EXCLUSIVE
    ),
    awaiting AS
    (
        SELECT COUNT(DISTINCT o.id)
                   AS awaiting_auditee_response
        FROM t_au_observation o
        INNER JOIN engagement_scope e
            ON e.eng_id = o.engplanid
        WHERE o.status = 2
    ),
    active_teams AS
    (
        SELECT COUNT(DISTINCT e.team_id)
                   AS teams_conducting_audit
        FROM engagement_scope e
        WHERE e.team_id IS NOT NULL
          AND e.status BETWEEN 4 AND 11
          AND TRUNC(
              NVL(e.operation_startdate, e.audit_startdate)
          ) <= TRUNC(SYSDATE)
          AND NOT EXISTS
          (
              SELECT 1
              FROM t_au_plan_eng_log l
              WHERE l.e_id = e.eng_id
                AND l.status_id = 14
          )
    )
    SELECT c.planned_entities,
           c.audited_entities,
           ROUND(
               CASE
                   WHEN c.planned_entities = 0 THEN NULL
                   ELSE c.audited_entities * 100
                        / c.planned_entities
               END, 2
           ) AS plan_coverage_pct,
           d.delayed_audit_reports,
           f.fraud_a1_paras,
           a.awaiting_auditee_response,
           t.teams_conducting_audit,
           P_AUDIT_PERIOD_ID AS audit_period_id,
           V_FROM AS audit_period_from,
           V_TO_EXCLUSIVE - 1 AS audit_period_to,
           TRUNC(SYSDATE) AS current_state_as_of
    FROM coverage c
    CROSS JOIN delayed d
    CROSS JOIN fraud f
    CROSS JOIN awaiting a
    CROSS JOIN active_teams t;

END P_GET_CIA_EXEC_KPIS;


  -------------------------------------------------------------------
  -- CIA Nature of Issue Identified (exactly ten rows).
  -- Eligibility, observation date and scope follow P_GET_CIA_ISSUES.
  -- Source categories are ID-based mappings: no free-text guessing.
  -- Mapping priority handles potentially overlapping match sources.
  -- An unmapped total is returned to prevent misleading zero charts.
  -------------------------------------------------------------------
 PROCEDURE P_GET_CIA_ISSUE_NATURE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
  BEGIN
    RESOLVE_CIA_COMMAND_PERIOD(
      P_DASHBOARD_TYPE, P_ENTITY_ID, P_AUDIT_PERIOD_ID,
      V_FROM, V_TO_EXCLUSIVE);

    OPEN IO_CURSOR FOR
      WITH categories AS
      (
        SELECT 1 display_order, 'AML_CFT' category_code,
               'AML/CFT' category_name FROM dual
        UNION ALL SELECT 2, 'ACCOUNTS', 'Accounts' FROM dual
        UNION ALL SELECT 3, 'LOAN_ADVANCES', 'Loan and Advances' FROM dual
        UNION ALL SELECT 4, 'CASH_MANAGEMENT', 'Cash Management' FROM dual
        UNION ALL SELECT 5, 'RECORD_MANAGEMENT', 'Record Management' FROM dual
        UNION ALL SELECT 6, 'BANKING_OPERATIONS', 'Banking Operations' FROM dual
        UNION ALL SELECT 7, 'BRANCH_GENERAL_CONTROLS',
                         'Branch General Controls' FROM dual
        UNION ALL SELECT 8, 'ACCOUNTING_REPORTING_RECON',
                         'Accounting / Reporting / Reconciliation' FROM dual
        UNION ALL SELECT 9, 'MANAGEMENT_AUDIT', 'Management Audit' FROM dual
        UNION ALL SELECT 10, 'IS_AUDIT', 'IS Audit' FROM dual
      ),
      cia_issue_rows AS
      (
        -- The IAS checklist root is the primary Nature of Issue.
        -- Missing checklist links remain NULL; preserve all ISSUE rows.
        SELECT DISTINCT k.observation_id, croot.checklist_root_id,
               o.annex AS annex_id,
               o.v_cat_nature_id AS violation_sub_id,
               k.audited_by_id
          FROM v_exec_kpi_lifecycle k
          LEFT JOIN t_au_observation o ON o.id = k.observation_id
          LEFT JOIN v_ias_cia_issue_checklist_root croot
            ON croot.observation_id = k.observation_id
         WHERE k.record_type = 'ISSUE'
           AND k.issue_eligible = 1
           AND k.period_id = P_AUDIT_PERIOD_ID
           AND k.issue_identified_on >= V_FROM
           AND k.issue_identified_on < V_TO_EXCLUSIVE
           AND (P_ENTITY_TYPE_ID IS NULL
                OR k.entity_type_id = P_ENTITY_TYPE_ID)
           AND (P_AUDITED_BY_ID IS NULL
                OR k.audited_by_id = P_AUDITED_BY_ID)
           AND (P_REPORTING_OFFICE_ID IS NULL
                OR k.entity_id = P_REPORTING_OFFICE_ID
                OR P_REPORTING_OFFICE_ID IN
                   (k.reporting_1, k.reporting_2, k.reporting_3,
                    k.reporting_4, k.reporting_5, k.reporting_6))
           AND (P_ENTITY_ID IS NULL OR k.entity_id = P_ENTITY_ID
                OR P_ENTITY_ID IN
                   (k.reporting_1, k.reporting_2, k.reporting_3,
                    k.reporting_4, k.reporting_5, k.reporting_6))
      ),
      nature_matches AS
      (
        SELECT i.observation_id, m.category_code,
               ROW_NUMBER() OVER
                 (PARTITION BY i.observation_id
                  -- Checklist-root decisions take precedence over legacy
                  -- fallback classifications for the same observation.
                  ORDER BY CASE m.source_type
                             WHEN 'CHECKLIST_ROOT' THEN 0 ELSE 1 END,
                           m.match_priority,
                           CASE m.source_type
                             WHEN 'VIOLATION_SUB' THEN 1
                             WHEN 'ANNEX' THEN 2
                             WHEN 'AUDIT_PORTFOLIO' THEN 3
                             ELSE 0 END,
                           m.source_id, m.category_code) AS match_rank
          FROM cia_issue_rows i
          JOIN t_ias_cia_issue_nature_map m
            ON m.is_active = 'Y'
           AND m.category_code IN (SELECT category_code FROM categories)
           AND ((m.source_type = 'CHECKLIST_ROOT'
                 AND m.source_id = i.checklist_root_id)
                OR (m.source_type = 'VIOLATION_SUB'
                 AND m.source_id = i.violation_sub_id)
                OR (m.source_type = 'ANNEX'
                    AND m.source_id = i.annex_id)
                OR (m.source_type = 'AUDIT_PORTFOLIO'
                    AND m.source_id = i.audited_by_id))
      ),
      nature_classified AS
      (
        SELECT i.observation_id, m.category_code
          FROM cia_issue_rows i
          LEFT JOIN nature_matches m
            ON m.observation_id = i.observation_id
           AND m.match_rank = 1
      ),
      nature_grouped AS
      (
        SELECT category_code, COUNT(*) AS issue_count
          FROM nature_classified
         WHERE category_code IS NOT NULL
         GROUP BY category_code
      ),
      totals AS
      (
        SELECT COUNT(*) AS total_issues,
               NVL(SUM(CASE WHEN category_code IS NULL
                            THEN 1 ELSE 0 END), 0) AS unmapped_issues
          FROM nature_classified
      )
      SELECT c.category_code, c.category_name, c.display_order,
             NVL(g.issue_count, 0) AS issue_count,
             t.total_issues, t.unmapped_issues,
             CASE WHEN t.total_issues = 0 THEN 'NO_ISSUES'
                  WHEN t.unmapped_issues > 0 THEN 'PARTIAL'
                  ELSE 'COMPLETE' END AS mapping_status
        FROM categories c
        LEFT JOIN nature_grouped g ON g.category_code = c.category_code
        CROSS JOIN totals t
       ORDER BY c.display_order;
  END P_GET_CIA_ISSUE_NATURE;




  -------------------------------------------------------------------
  -- Snap historical basis deliberately matches lifecycle estimates:
  -- latest settlement/master state, current risk and organization.
  -- T_AU_PARAS_STATUS_CHANGE_LOG has authorized events, but no proven
  -- complete baseline; COM_STATUS is a workflow stage, not para status.
  -- Historical cycle distributions are N/A, never today's distribution.
  -- Historical risk/age/critical lists are explicitly Estimated.
  -- Settlement trend uses qualifying lifecycle event timestamps.
  -------------------------------------------------------------------
  PROCEDURE P_GET_SNAP_DISTRIBUTION(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    V_FROM DATE;
    V_TO DATE;
    V_TODAY NUMBER;
  BEGIN
    IF V_TYPE <> 'CIA' THEN
      RAISE_APPLICATION_ERROR(-20045, 'Audit Para Snap requires CIA scope.');
    END IF;
    IF P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR TRUNC(P_START_DATE) > TRUNC(P_END_DATE)
       OR TRUNC(P_END_DATE) > TRUNC(SYSDATE)
       OR MONTHS_BETWEEN(TRUNC(P_END_DATE, 'MM'), TRUNC(P_START_DATE, 'MM')) >= 1200 THEN
      RAISE_APPLICATION_ERROR(-20046, 'Provide ordered non-future Snap dates spanning fewer than 1200 months.');
    END IF;
    V_FROM := TRUNC(P_START_DATE);
    V_TO := TRUNC(P_END_DATE) + 1;
    V_TODAY := CASE WHEN TRUNC(P_END_DATE) = TRUNC(SYSDATE) THEN 1 ELSE 0 END;

    IF P_SNAP_DIVISION_CODE IS NOT NULL THEN
      PKG_AUDIT_SNAP_SCOPE.REQUIRE_SELECTION(
        P_SNAP_DIVISION_CODE,P_AUDITED_BY_ID);
    END IF;
    OPEN IO_CURSOR FOR
      WITH scoped AS
      (
        SELECT k.*,
               CASE WHEN V_TODAY = 1 THEN k.current_open
                    WHEN k.para_added_on < V_TO
                     AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL
                          OR k.report_concluded_on < V_TO)
                     AND (k.para_status = 8 OR
                          (k.para_status = 9 AND k.para_settled_on >= V_TO))
                    THEN 1 ELSE 0 END AS snap_open,
               TRUNC(P_END_DATE) - TRUNC(k.para_added_on) AS snap_age
          FROM V_EXEC_KPI_LIFECYCLE k
         WHERE k.record_type = 'PARA'
           AND (P_ENTITY_TYPE_ID IS NULL OR k.entity_type_id = P_ENTITY_TYPE_ID)
           AND (P_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_AUDITED_BY_ID)
           AND (P_SNAP_DIVISION_CODE IS NULL
                OR k.audited_by_id = (CASE P_SNAP_DIVISION_CODE
                        WHEN 'FAD' THEN 112243
                        WHEN 'MAD' THEN 113254 END)
                OR EXISTS
                  (SELECT 1
                     FROM V_IAS_ENTITY_REPORTING_NAMES h
                    WHERE h.ENTITY_ID = k.audited_by_id
                      AND (CASE P_SNAP_DIVISION_CODE
                        WHEN 'FAD' THEN 112243
                        WHEN 'MAD' THEN 113254 END) IN
                          (h.REPORTING_1,h.REPORTING_2,h.REPORTING_3,
                           h.REPORTING_4,h.REPORTING_5,h.REPORTING_6)))
           AND (P_REPORTING_OFFICE_ID IS NULL OR k.entity_id = P_REPORTING_OFFICE_ID
                OR P_REPORTING_OFFICE_ID IN
                   (k.reporting_1,k.reporting_2,k.reporting_3,k.reporting_4,k.reporting_5,k.reporting_6))
           AND (P_ENTITY_ID IS NULL OR k.entity_id = P_ENTITY_ID
                OR P_ENTITY_ID IN
                   (k.reporting_1,k.reporting_2,k.reporting_3,k.reporting_4,k.reporting_5,k.reporting_6))
      ),
      categories AS
      (
        SELECT 'risk' dataset, '1' bucket_code, 'High' bucket_name, 1 ord FROM dual
        UNION ALL SELECT 'risk','2','Medium',2 FROM dual
        UNION ALL SELECT 'risk','3','Low',3 FROM dual
        UNION ALL SELECT 'risk','0','Unrated',4 FROM dual
        UNION ALL SELECT 'aging','1','0-90 days',1 FROM dual
        UNION ALL SELECT 'aging','2','91-180 days',2 FROM dual
        UNION ALL SELECT 'aging','3','181-365 days',3 FROM dual
        UNION ALL SELECT 'aging','4','1-3 years',4 FROM dual
        UNION ALL SELECT 'aging','5','Over 3 years',5 FROM dual
        UNION ALL SELECT 'aging','0','Unknown date',6 FROM dual
        UNION ALL SELECT 'cycle','0','Zero cycles',1 FROM dual
        UNION ALL SELECT 'cycle','1','1-3 cycles',2 FROM dual
        UNION ALL SELECT 'cycle','4','Over 3 cycles',3 FROM dual
      ),
      position_rows AS
      (
        SELECT 'risk' dataset,
               CASE WHEN risk_id IN (1,2,3) THEN TO_CHAR(risk_id) ELSE '0' END bucket_code
          FROM scoped WHERE snap_open = 1
        UNION ALL
        SELECT 'aging',
               CASE WHEN snap_age IS NULL THEN '0' WHEN snap_age <= 90 THEN '1'
                    WHEN snap_age <= 180 THEN '2' WHEN snap_age <= 365 THEN '3'
                    WHEN snap_age <= 1095 THEN '4' ELSE '5' END
          FROM scoped WHERE snap_open = 1
        UNION ALL
        SELECT 'cycle', CASE WHEN NVL(com_cycle,0) = 0 THEN '0'
                            WHEN com_cycle <= 3 THEN '1' ELSE '4' END
          FROM scoped WHERE snap_open = 1 AND V_TODAY = 1
      ),
      counts AS
      (
        SELECT dataset, bucket_code, COUNT(*) para_count FROM position_rows
         GROUP BY dataset, bucket_code
      ),
      months AS
      (
        SELECT ADD_MONTHS(TRUNC(V_FROM, 'MM'), LEVEL - 1) month_start
          FROM dual
        CONNECT BY LEVEL <= MONTHS_BETWEEN(TRUNC(P_END_DATE,'MM'),TRUNC(V_FROM,'MM')) + 1
      ),
      results AS
      (
        SELECT c.dataset, c.bucket_code, c.bucket_name, c.ord,
               CASE WHEN c.dataset = 'cycle' AND V_TODAY = 0 THEN CAST(NULL AS NUMBER)
                    ELSE NVL(n.para_count,0) END para_count,
               CAST(NULL AS NUMBER) AS high_risk_count,
               CASE WHEN c.dataset = 'cycle' AND V_TODAY = 0 THEN 'N'
                    WHEN V_TODAY = 1 THEN 'Y' ELSE 'E' END quality_code
          FROM categories c LEFT JOIN counts n
            ON n.dataset = c.dataset AND n.bucket_code = c.bucket_code
        UNION ALL
        SELECT 'trend', TO_CHAR(m.month_start,'YYYY-MM'), TO_CHAR(m.month_start,'YYYY-MM'),
               ROW_NUMBER() OVER(ORDER BY m.month_start),
               COUNT(s.com_id),
               COUNT(CASE WHEN s.risk_id = 1 THEN s.com_id END), 'E'
          FROM months m LEFT JOIN scoped s
            ON s.para_settled_on >= GREATEST(m.month_start,V_FROM)
           AND s.para_settled_on < LEAST(ADD_MONTHS(m.month_start,1),V_TO)
         GROUP BY m.month_start
      )
      SELECT dataset, bucket_code, bucket_name, para_count,
             high_risk_count, quality_code
        FROM results ORDER BY dataset, ord;
  END P_GET_SNAP_DISTRIBUTION;

  PROCEDURE P_GET_SNAP_CRITICAL(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      P_MAX_ROWS IN NUMBER DEFAULT 100,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    V_FROM DATE;
    V_TO DATE;
    V_TODAY NUMBER;
  BEGIN
    IF V_TYPE <> 'CIA' THEN
      RAISE_APPLICATION_ERROR(-20045, 'Audit Para Snap requires CIA scope.');
    END IF;
    IF P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR TRUNC(P_START_DATE) > TRUNC(P_END_DATE)
       OR TRUNC(P_END_DATE) > TRUNC(SYSDATE)
       OR MONTHS_BETWEEN(TRUNC(P_END_DATE, 'MM'), TRUNC(P_START_DATE, 'MM')) >= 1200 THEN
      RAISE_APPLICATION_ERROR(-20046, 'Provide ordered non-future Snap dates spanning fewer than 1200 months.');
    END IF;
    V_FROM := TRUNC(P_START_DATE);
    V_TO := TRUNC(P_END_DATE) + 1;
    V_TODAY := CASE WHEN TRUNC(P_END_DATE) = TRUNC(SYSDATE) THEN 1 ELSE 0 END;

    IF P_MAX_ROWS IS NULL OR P_MAX_ROWS < 1 OR P_MAX_ROWS > 250
       OR P_MAX_ROWS <> TRUNC(P_MAX_ROWS) THEN
      RAISE_APPLICATION_ERROR(-20047, 'Snap critical limit must be 1..250.');
    END IF;
    IF P_SNAP_DIVISION_CODE IS NOT NULL THEN
      PKG_AUDIT_SNAP_SCOPE.REQUIRE_SELECTION(
        P_SNAP_DIVISION_CODE,P_AUDITED_BY_ID);
    END IF;
    OPEN IO_CURSOR FOR
      WITH scoped AS
      (
        SELECT k.*,
               CASE WHEN V_TODAY = 1 THEN k.current_open
                    WHEN k.para_added_on < V_TO
                     AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL
                          OR k.report_concluded_on < V_TO)
                     AND (k.para_status = 8 OR
                          (k.para_status = 9 AND k.para_settled_on >= V_TO))
                    THEN 1 ELSE 0 END AS snap_open,
               TRUNC(P_END_DATE) - TRUNC(k.para_added_on) AS snap_age
          FROM V_EXEC_KPI_LIFECYCLE k
         WHERE k.record_type = 'PARA'
           AND (P_ENTITY_TYPE_ID IS NULL OR k.entity_type_id = P_ENTITY_TYPE_ID)
           AND (P_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_AUDITED_BY_ID)
           AND (P_SNAP_DIVISION_CODE IS NULL
                OR k.audited_by_id = (CASE P_SNAP_DIVISION_CODE
                        WHEN 'FAD' THEN 112243
                        WHEN 'MAD' THEN 113254 END)
                OR EXISTS
                  (SELECT 1
                     FROM V_IAS_ENTITY_REPORTING_NAMES h
                    WHERE h.ENTITY_ID = k.audited_by_id
                      AND (CASE P_SNAP_DIVISION_CODE
                        WHEN 'FAD' THEN 112243
                        WHEN 'MAD' THEN 113254 END) IN
                          (h.REPORTING_1,h.REPORTING_2,h.REPORTING_3,
                           h.REPORTING_4,h.REPORTING_5,h.REPORTING_6)))
           AND (P_REPORTING_OFFICE_ID IS NULL OR k.entity_id = P_REPORTING_OFFICE_ID
                OR P_REPORTING_OFFICE_ID IN
                   (k.reporting_1,k.reporting_2,k.reporting_3,k.reporting_4,k.reporting_5,k.reporting_6))
           AND (P_ENTITY_ID IS NULL OR k.entity_id = P_ENTITY_ID
                OR P_ENTITY_ID IN
                   (k.reporting_1,k.reporting_2,k.reporting_3,k.reporting_4,k.reporting_5,k.reporting_6))
      ),
      ranked AS
      (
        SELECT k.com_id, k.entity_id, e.name AS entity_name,
               p.para_no, SUBSTR(p.gist_of_paras,1,1000) AS gist_of_paras,
               CASE k.risk_id WHEN 1 THEN 'High' WHEN 2 THEN 'Medium'
                              WHEN 3 THEN 'Low' ELSE 'Unrated' END risk_name,
               k.snap_age AS aging_days,
               CASE WHEN V_TODAY = 1 THEN k.com_cycle END AS com_cycle,
               ROW_NUMBER() OVER(ORDER BY CASE WHEN k.risk_id = 1 THEN 0 ELSE 1 END,
                                          k.snap_age DESC NULLS LAST,k.com_id) rn
          FROM scoped k
          LEFT JOIN t_auditee_entities e ON e.entity_id = k.entity_id
          LEFT JOIN ais_t_au_post_compliance p ON p.com_id = k.com_id
         WHERE k.snap_open = 1
           AND k.risk_id = 1 -- ZTBL Critical = High-risk open
      )
      SELECT com_id, entity_id, entity_name, para_no, gist_of_paras,
             risk_name, aging_days, com_cycle,
             CASE WHEN V_TODAY = 1 THEN 'Y' ELSE 'E' END quality_code
        FROM ranked WHERE rn <= P_MAX_ROWS ORDER BY rn;
  END P_GET_SNAP_CRITICAL;

  -- These calls are read-only. The application must derive the root
  -- from the logged-in user's authorized org hierarchy, never from
  -- an untrusted browser request.
  PROCEDURE REQUIRE_ROLE_DASHBOARD(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER DEFAULT NULL) AS
    V_TYPE VARCHAR2(20);
  BEGIN
    V_TYPE := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ROOT_ENTITY_ID);
    IF V_TYPE NOT IN ('GROUP','RLM') THEN
      RAISE_APPLICATION_ERROR(-20061,'GROUP or RLM perspective required');
    END IF;
    IF P_MAX_ROWS IS NOT NULL AND
       (P_MAX_ROWS < 1 OR P_MAX_ROWS > 500 OR P_MAX_ROWS <> TRUNC(P_MAX_ROWS)) THEN
      RAISE_APPLICATION_ERROR(-20062,'Max rows must be between 1 and 500');
    END IF;
  END REQUIRE_ROLE_DASHBOARD;

  -- "Heatmap" shows observed open risk counts, not invented
  -- branch risk ratings. High-risk share denominator = open paras.
  PROCEDURE P_GET_ROLE_ENTITY_HEATMAP(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD IN VARCHAR2 DEFAULT NULL,
    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
    P_MAX_ROWS IN NUMBER DEFAULT 250, IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_ROLE_DASHBOARD(P_DASHBOARD_TYPE,P_ROOT_ENTITY_ID,P_MAX_ROWS);
    IF P_MAX_ROWS IS NULL THEN
      RAISE_APPLICATION_ERROR(-20062,'Max rows is required');
    END IF;
    OPEN IO_CURSOR FOR
      SELECT * FROM (
        SELECT p.entity_id, MAX(p.entity_name) AS entity_name,
               MAX(p.entity_type_id) AS entity_type_id,
               COUNT(*) AS open_paras,
               SUM(CASE WHEN p.risk_id = 1 THEN 1 ELSE 0 END) AS high_risk,
               SUM(CASE WHEN p.risk_id = 2 THEN 1 ELSE 0 END) AS medium_risk,
               SUM(CASE WHEN p.risk_id = 3 THEN 1 ELSE 0 END) AS low_risk,
               SUM(CASE WHEN p.risk_id IS NULL OR p.risk_id NOT IN (1,2,3)
                        THEN 1 ELSE 0 END) AS unrated,
               SUM(CASE WHEN p.aging_days > 365 THEN 1 ELSE 0 END) AS aged_over_365,
               SUM(CASE WHEN p.com_cycle > 3 THEN 1 ELSE 0 END) AS over_3_cycles,
               ROUND(100 * SUM(CASE WHEN p.risk_id = 1 THEN 1 ELSE 0 END)
                     / NULLIF(COUNT(*),0),2) AS high_risk_share_pct
          FROM V_EXEC_PARA_BASE p
         WHERE p.is_open = 1
           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
           AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
           AND (p.entity_id = P_ROOT_ENTITY_ID
                OR P_ROOT_ENTITY_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                        p.reporting_4,p.reporting_5,p.reporting_6))
         GROUP BY p.entity_id
         ORDER BY high_risk DESC, aged_over_365 DESC, open_paras DESC, p.entity_id
      ) WHERE ROWNUM <= P_MAX_ROWS;
  END P_GET_ROLE_ENTITY_HEATMAP;

  -- Compare direct children of selected org root. A record directly
  -- against the root is retained in its own group.
  PROCEDURE P_GET_ROLE_UNIT_COMPARISON(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD IN VARCHAR2 DEFAULT NULL,
    IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_ROLE_DASHBOARD(P_DASHBOARD_TYPE,P_ROOT_ENTITY_ID);
    OPEN IO_CURSOR FOR
      WITH scoped AS (
        SELECT p.*,
               CASE
                 WHEN p.entity_id = P_ROOT_ENTITY_ID THEN p.entity_id
                 WHEN p.reporting_1 = P_ROOT_ENTITY_ID THEN p.entity_id
                 WHEN p.reporting_2 = P_ROOT_ENTITY_ID THEN p.reporting_1
                 WHEN p.reporting_3 = P_ROOT_ENTITY_ID THEN p.reporting_2
                 WHEN p.reporting_4 = P_ROOT_ENTITY_ID THEN p.reporting_3
                 WHEN p.reporting_5 = P_ROOT_ENTITY_ID THEN p.reporting_4
                 WHEN p.reporting_6 = P_ROOT_ENTITY_ID THEN p.reporting_5
               END AS unit_id
          FROM V_EXEC_PARA_BASE p
         WHERE (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
           AND (p.entity_id = P_ROOT_ENTITY_ID
                OR P_ROOT_ENTITY_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                        p.reporting_4,p.reporting_5,p.reporting_6))
      )
      SELECT s.unit_id, MAX(u.name) AS unit_name,
             SUM(CASE WHEN s.is_open = 1 THEN 1 ELSE 0 END) AS open_paras,
             SUM(CASE WHEN s.is_open = 1 AND s.risk_id = 1 THEN 1 ELSE 0 END) AS high_risk_open,
             SUM(CASE WHEN s.is_open = 1 AND s.aging_days > 365 THEN 1 ELSE 0 END) AS aged_over_365,
             SUM(CASE WHEN s.is_open = 1 AND s.com_cycle > 3 THEN 1 ELSE 0 END) AS over_3_cycles,
             SUM(CASE WHEN s.is_settled = 1 THEN 1 ELSE 0 END) AS settled_total,
             SUM(CASE WHEN s.is_settled = 1
                       AND s.setteled_on >= TRUNC(SYSDATE,'MM')
                       AND s.setteled_on < ADD_MONTHS(TRUNC(SYSDATE,'MM'),1)
                      THEN 1 ELSE 0 END) AS settled_mtd
        FROM scoped s
        LEFT JOIN t_auditee_entities u ON u.entity_id = s.unit_id
       WHERE s.unit_id IS NOT NULL
       GROUP BY s.unit_id
       ORDER BY high_risk_open DESC, aged_over_365 DESC, open_paras DESC, s.unit_id;
  END P_GET_ROLE_UNIT_COMPARISON;

  -- Factual status IDs returned. Rejected/referred-back and response
  -- labels require separately verified workflow status mapping.
  PROCEDURE P_GET_ROLE_FOLLOWUPS(
    P_DASHBOARD_TYPE IN VARCHAR2, P_ROOT_ENTITY_ID IN NUMBER,
    P_AUDIT_PERIOD IN VARCHAR2 DEFAULT NULL,
    P_MAX_ROWS IN NUMBER DEFAULT 100, IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    REQUIRE_ROLE_DASHBOARD(P_DASHBOARD_TYPE,P_ROOT_ENTITY_ID,P_MAX_ROWS);
    IF P_MAX_ROWS IS NULL THEN
      RAISE_APPLICATION_ERROR(-20062,'Max rows is required');
    END IF;
    OPEN IO_CURSOR FOR
      SELECT * FROM (
        SELECT p.com_id, p.para_no, p.audit_period, p.gist_of_paras,
               p.entity_id, p.entity_name, p.division_id, p.division_name,
               p.business_group_id, p.business_group_name,
               p.risk_id, p.risk_name, p.para_status,
               p.com_status, p.com_stage, p.com_cycle,
               p.para_added_on, p.aging_days, p.last_movement_date,
               p.days_since_last_movement,
               CASE WHEN p.risk_id = 1 THEN 'HIGH_RISK'
                    WHEN p.aging_days > 365 THEN 'AGED_OVER_365'
                    WHEN p.com_cycle > 3 THEN 'REPEATED_COMPLIANCE'
                    ELSE 'NORMAL' END AS priority_reason
          FROM V_EXEC_PARA_BASE p
         WHERE p.is_open = 1
           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
           AND (p.entity_id = P_ROOT_ENTITY_ID
                OR P_ROOT_ENTITY_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                        p.reporting_4,p.reporting_5,p.reporting_6))
         ORDER BY CASE WHEN p.risk_id = 1 THEN 0 ELSE 1 END,
                  p.aging_days DESC NULLS LAST, p.com_cycle DESC, p.com_id
      ) WHERE ROWNUM <= P_MAX_ROWS;
  END P_GET_ROLE_FOLLOWUPS;

END PKG_EXEC_DASHBOARD;

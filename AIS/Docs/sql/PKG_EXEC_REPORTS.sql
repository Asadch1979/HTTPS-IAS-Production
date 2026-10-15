CREATE OR REPLACE PACKAGE PKG_EXEC_REPORTS AUTHID DEFINER AS
  PROCEDURE P_ANNUAL_FIELD_PROGRESS(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR);
  PROCEDURE P_FIELD_OFFICE_POSITION(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR);
  PROCEDURE P_GM_REGION_POSITION(P_DASHBOARD_TYPE      IN VARCHAR2,
                                 P_SCOPE_ROOT_ID       IN NUMBER,
                                 P_START_DATE          IN DATE,
                                 P_END_DATE            IN DATE,
                                 P_ENTITY_TYPE_ID      IN NUMBER,
                                 P_AUDITED_BY_ID       IN NUMBER,
                                 P_REPORTING_OFFICE_ID IN NUMBER,
                                 P_ENTITY_ID           IN NUMBER,
                                 IO_CURSOR             OUT SYS_REFCURSOR);
  PROCEDURE P_REGION_RANKINGS(P_DASHBOARD_TYPE      IN VARCHAR2,
                              P_SCOPE_ROOT_ID       IN NUMBER,
                              P_START_DATE          IN DATE,
                              P_END_DATE            IN DATE,
                              P_ENTITY_TYPE_ID      IN NUMBER,
                              P_AUDITED_BY_ID       IN NUMBER,
                              P_REPORTING_OFFICE_ID IN NUMBER,
                              P_ENTITY_ID           IN NUMBER,
                              IO_CURSOR             OUT SYS_REFCURSOR);
  PROCEDURE P_MANAGEMENT_SETTLEMENT(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR);
END PKG_EXEC_REPORTS;
/
CREATE OR REPLACE PACKAGE BODY PKG_EXEC_REPORTS AS
  PROCEDURE VALIDATE_REQUEST(P_REPORT_ID           IN VARCHAR2,
                             P_DASHBOARD_TYPE      IN VARCHAR2,
                             P_SCOPE_ROOT_ID       IN NUMBER,
                             P_START_DATE          IN DATE,
                             P_END_DATE            IN DATE,
                             P_ENTITY_TYPE_ID      IN NUMBER,
                             P_AUDITED_BY_ID       IN NUMBER,
                             P_REPORTING_OFFICE_ID IN NUMBER,
                             P_ENTITY_ID           IN NUMBER) IS
    n NUMBER;
  BEGIN
    IF P_REPORT_ID NOT IN ('A', 'B', 'C', 'D', 'E') OR P_REPORT_ID IS NULL OR
       P_DASHBOARD_TYPE NOT IN ('TOP', 'GROUP', 'CCO', 'CIA') OR
       P_DASHBOARD_TYPE IS NULL OR P_START_DATE IS NULL OR
       P_END_DATE IS NULL OR P_START_DATE <> TRUNC(P_START_DATE) OR
       P_END_DATE <> TRUNC(P_END_DATE) OR P_START_DATE > P_END_DATE OR
       P_END_DATE >= DATE '9999-12-31' THEN
      RAISE_APPLICATION_ERROR(-20801, 'Invalid report type or date range');
    END IF;
    IF (P_ENTITY_TYPE_ID IS NOT NULL AND P_AUDITED_BY_ID IS NOT NULL) OR
       (P_DASHBOARD_TYPE = 'GROUP' AND P_SCOPE_ROOT_ID IS NULL) OR
       (P_DASHBOARD_TYPE <> 'GROUP' AND P_SCOPE_ROOT_ID IS NOT NULL) THEN
      RAISE_APPLICATION_ERROR(-20802, 'Invalid reporting scope');
    END IF;
    FOR r IN (SELECT P_SCOPE_ROOT_ID id
                FROM dual
              UNION ALL
              SELECT P_ENTITY_TYPE_ID
                FROM dual
              UNION ALL
              SELECT P_AUDITED_BY_ID
                FROM dual
              UNION ALL
              SELECT P_REPORTING_OFFICE_ID
                FROM dual
              UNION ALL
              SELECT P_ENTITY_ID
                FROM dual) LOOP
      IF r.id IS NOT NULL AND (r.id <= 0 OR r.id <> TRUNC(r.id)) THEN
        RAISE_APPLICATION_ERROR(-20801,
                                'Entity IDs must be positive integers');
      END IF;
    END LOOP;
    IF P_DASHBOARD_TYPE = 'GROUP' THEN
      SELECT COUNT(*)
        INTO n
        FROM V_EXEC_ENTITY_SCOPE s
       WHERE s.entity_id = P_SCOPE_ROOT_ID
         AND s.business_group_id = P_SCOPE_ROOT_ID;
      IF n <> 1 THEN
        RAISE_APPLICATION_ERROR(-20802, 'GROUP root is invalid');
      END IF;
    END IF;
    FOR r IN (SELECT P_REPORTING_OFFICE_ID id
                FROM dual
              UNION ALL
              SELECT P_ENTITY_ID
                FROM dual) LOOP
      IF r.id IS NOT NULL THEN
        SELECT COUNT(*)
          INTO n
          FROM V_EXEC_ENTITY_SCOPE s
         WHERE s.entity_id = r.id
           AND (P_SCOPE_ROOT_ID IS NULL OR s.entity_id = P_SCOPE_ROOT_ID OR
               P_SCOPE_ROOT_ID IN (s.reporting_1,
                                    s.reporting_2,
                                    s.reporting_3,
                                    s.reporting_4,
                                    s.reporting_5,
                                    s.reporting_6));
        IF n <> 1 THEN
          RAISE_APPLICATION_ERROR(-20802,
                                  'Entity/office outside authorized scope');
        END IF;
      END IF;
    END LOOP;
  END VALIDATE_REQUEST;

  PROCEDURE OPEN_REPORT(P_REPORT_ID           IN VARCHAR2,
                        P_DASHBOARD_TYPE      IN VARCHAR2,
                        P_SCOPE_ROOT_ID       IN NUMBER,
                        P_START_DATE          IN DATE,
                        P_END_DATE            IN DATE,
                        P_ENTITY_TYPE_ID      IN NUMBER,
                        P_AUDITED_BY_ID       IN NUMBER,
                        P_REPORTING_OFFICE_ID IN NUMBER,
                        P_ENTITY_ID           IN NUMBER,
                        IO_CURSOR             OUT SYS_REFCURSOR) IS
  BEGIN
    VALIDATE_REQUEST(P_REPORT_ID,
                     P_DASHBOARD_TYPE,
                     P_SCOPE_ROOT_ID,
                     P_START_DATE,
                     P_END_DATE,
                     P_ENTITY_TYPE_ID,
                     P_AUDITED_BY_ID,
                     P_REPORTING_OFFICE_ID,
                     P_ENTITY_ID);
    -- A-D follow FAD's existing B/D audit-type set. E uses D (desk/HO) category:
    -- verify with management audit owner before treating E as a certified MAD portfolio.
    IF P_REPORT_ID = 'A' THEN
      OPEN IO_CURSOR FOR
        WITH years AS
         (SELECT ADD_MONTHS(TRUNC(P_START_DATE, 'YYYY'), 12 * (LEVEL - 1)) y
            FROM dual
          CONNECT BY LEVEL <= EXTRACT(YEAR FROM P_END_DATE) -
                     EXTRACT(YEAR FROM P_START_DATE) + 1),
        periods AS
         (SELECT GREATEST(y, P_START_DATE) period_start,
                 LEAST(ADD_MONTHS(y, 12) - 1, P_END_DATE) period_end
            FROM years),
        facts AS
         (SELECT p.period_start,
                 p.period_end,
                 CASE
                   WHEN s.para_added_on < p.period_start AND
                        (s.close_on IS NULL OR s.close_on >= p.period_start) THEN
                    1
                   ELSE
                    0
                 END opening_balance,
                 CASE
                   WHEN s.para_added_on >= p.period_start AND
                        s.para_added_on < p.period_end + 1 THEN
                    1
                   ELSE
                    0
                 END paras_added,
                 CASE
                   WHEN s.close_on >= p.period_start AND
                        s.close_on < p.period_end + 1 THEN
                    1
                   ELSE
                    0
                 END settled_paras,
                 CASE
                   WHEN s.para_added_on < p.period_end + 1 AND
                        (s.close_on IS NULL OR s.close_on >= p.period_end + 1) THEN
                    1
                   ELSE
                    0
                 END outstanding_paras,
                 CASE
                   WHEN s.para_added_on < p.period_end + 1 AND
                        (s.close_on IS NULL OR s.close_on >= p.period_end + 1) AND
                        s.risk = 3 THEN
                    1
                   ELSE
                    0
                 END low_risk,
                 CASE
                   WHEN s.para_added_on < p.period_end + 1 AND
                        (s.close_on IS NULL OR s.close_on >= p.period_end + 1) AND
                        s.risk = 2 THEN
                    1
                   ELSE
                    0
                 END medium_risk,
                 CASE
                   WHEN s.para_added_on < p.period_end + 1 AND
                        (s.close_on IS NULL OR s.close_on >= p.period_end + 1) AND
                        s.risk = 1 THEN
                    1
                   ELSE
                    0
                 END high_risk,
                 CASE
                   WHEN s.para_added_on < p.period_end + 1 AND
                        (s.close_on IS NULL OR s.close_on >= p.period_end + 1) AND
                        (s.risk IS NULL OR s.risk NOT IN (1, 2, 3)) THEN
                    1
                   ELSE
                    0
                 END unrated_risk,
                 CASE
                   WHEN s.close_on >= p.period_start AND
                        s.close_on < p.period_end + 1 AND
                        TRUNC(s.history_close_on) = TRUNC(s.close_on) AND
                        s.history_close_stage NOT IN (41, 45) THEN
                    1
                   ELSE
                    0
                 END settled_audit,
                 CASE
                   WHEN s.close_on >= p.period_start AND
                        s.close_on < p.period_end + 1 AND
                        TRUNC(s.history_close_on) = TRUNC(s.close_on) AND
                        s.history_close_stage IN (41, 45) THEN
                    1
                   ELSE
                    0
                 END settled_compliance,
                 CASE
                   WHEN s.close_on >= p.period_start AND
                        s.close_on < p.period_end + 1 AND
                        (s.history_close_on IS NULL OR
                        TRUNC(s.history_close_on) <> TRUNC(s.close_on) OR
                        s.history_close_stage IS NULL) THEN
                    1
                   ELSE
                    0
                 END settled_unknown,
                 0 reopening_events,
                 0 resettlement_events,
                 0 initially_closed_additions,
                 CASE
                   WHEN s.para_added_on < p.period_end + 1 THEN
                    (CASE
                      WHEN s.para_added_on < p.period_end + 1 AND
                           (s.close_on IS NULL OR s.close_on >= p.period_end + 1) THEN
                       1
                      ELSE
                       0
                    END) - (CASE
                      WHEN s.para_added_on < p.period_start AND
                           (s.close_on IS NULL OR s.close_on >= p.period_start) THEN
                       1
                      ELSE
                       0
                    END) - (CASE
                      WHEN s.para_added_on >= p.period_start AND
                           s.para_added_on < p.period_end + 1 THEN
                       1
                      ELSE
                       0
                    END) + (CASE
                      WHEN s.close_on >= p.period_start AND
                           s.close_on < p.period_end + 1 THEN
                       1
                      ELSE
                       0
                    END)
                   ELSE
                    0
                 END reconciliation_difference,
                 s.auditor_assignment_mismatch
            FROM periods p
            JOIN V_EXEC_REPORT_SOURCE s
              ON s.para_added_on < p.period_end + 1
           WHERE (P_SCOPE_ROOT_ID IS NULL OR s.entity_id = P_SCOPE_ROOT_ID OR
                 P_SCOPE_ROOT_ID IN (s.reporting_1,
                                      s.reporting_2,
                                      s.reporting_3,
                                      s.reporting_4,
                                      s.reporting_5,
                                      s.reporting_6))
             AND (P_ENTITY_ID IS NULL OR s.entity_id = P_ENTITY_ID OR
                 P_ENTITY_ID IN (s.reporting_1,
                                  s.reporting_2,
                                  s.reporting_3,
                                  s.reporting_4,
                                  s.reporting_5,
                                  s.reporting_6))
             AND (P_ENTITY_TYPE_ID IS NULL OR
                 s.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR s.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR
                 s.reporting_office_id = P_REPORTING_OFFICE_ID)
             AND ((P_REPORT_ID = 'E' AND s.audit_type = 'D') OR
                 (P_REPORT_ID <> 'E' AND s.audit_type IN ('B', 'D'))))
        SELECT 'ANNUAL' ROW_KIND,
               CAST(NULL AS NUMBER) GROUP_ID,
               CAST(NULL AS VARCHAR2(500)) GROUP_NAME,
               EXTRACT(YEAR FROM period_start) ITEM_ID,
               TO_CHAR(period_start, 'YYYY') ITEM_CODE,
               TO_CHAR(period_start, 'YYYY') ITEM_NAME,
               period_start PERIOD_START,
               period_end PERIOD_END,
               NVL(SUM(OPENING_BALANCE), 0) OPENING_BALANCE,
               NVL(SUM(PARAS_ADDED), 0) PARAS_ADDED,
               NVL(SUM(OPENING_BALANCE + PARAS_ADDED), 0) TOTAL_PARAS,
               NVL(SUM(SETTLED_PARAS), 0) SETTLED_PARAS,
               NVL(SUM(OUTSTANDING_PARAS), 0) OUTSTANDING_PARAS,
               ROUND(100 * SUM(SETTLED_PARAS) /
                     NULLIF(SUM(OPENING_BALANCE + PARAS_ADDED), 0),
                     2) SETTLEMENT_PERCENTAGE,
               NVL(SUM(LOW_RISK), 0) LOW_RISK,
               NVL(SUM(MEDIUM_RISK), 0) MEDIUM_RISK,
               NVL(SUM(HIGH_RISK), 0) HIGH_RISK,
               NVL(SUM(UNRATED_RISK), 0) UNRATED_RISK,
               NVL(SUM(SETTLED_AUDIT), 0) SETTLED_AUDIT,
               NVL(SUM(SETTLED_COMPLIANCE), 0) SETTLED_COMPLIANCE,
               NVL(SUM(SETTLED_UNKNOWN), 0) SETTLED_UNKNOWN,
               NVL(SUM(REOPENING_EVENTS), 0) REOPENING_EVENTS,
               NVL(SUM(RESETTLEMENT_EVENTS), 0) RESETTLEMENT_EVENTS,
               NVL(SUM(INITIALLY_CLOSED_ADDITIONS), 0) INITIALLY_CLOSED_ADDITIONS,
               NVL(SUM(RECONCILIATION_DIFFERENCE), 0) RECONCILIATION_DIFFERENCE,
               NVL(SUM(AUDITOR_ASSIGNMENT_MISMATCH), 0) AUDITOR_ASSIGNMENT_MISMATCH,
               CAST(NULL AS VARCHAR2(30)) RANKING_GROUP,
               CAST(NULL AS NUMBER) RANK_POSITION
          FROM facts
         GROUP BY period_start, period_end
         ORDER BY period_start;ELSE
      OPEN IO_CURSOR FOR
        WITH facts AS
         (SELECT s.com_id,
                 s.audited_by,
                 s.audited_by_name,
                 s.gm_office_id,
                 s.gm_name,
                 s.region_id,
                 re.name region_name,
                 TO_CHAR(re.code) region_code,
                 s.division_id,
                 s.division_name,
                 s.business_group_id,
                 s.group_name,
                 s.entity_type_id,
                 et.entitytypedesc entity_type_name,
                 
                 CASE
                   WHEN s.para_added_on <
                        P_START_DATE AND
                        (s.close_on IS NULL OR
                        s.close_on >=
                        P_START_DATE) THEN
                    1
                   ELSE
                    0
                 END opening_balance,
                 CASE
                   WHEN s.para_added_on >=
                        P_START_DATE AND
                        s.para_added_on <
                        P_END_DATE + 1 THEN
                    1
                   ELSE
                    0
                 END paras_added,
                 CASE
                   WHEN s.close_on >= P_START_DATE AND
                        s.close_on <
                        P_END_DATE + 1 THEN
                    1
                   ELSE
                    0
                 END settled_paras,
                 CASE
                   WHEN s.para_added_on <
                        P_END_DATE + 1 AND
                        (s.close_on IS NULL OR
                        s.close_on >=
                        P_END_DATE + 1) THEN
                    1
                   ELSE
                    0
                 END outstanding_paras,
                 CASE
                   WHEN s.para_added_on <
                        P_END_DATE + 1 AND
                        (s.close_on IS NULL OR
                        s.close_on >=
                        P_END_DATE + 1) AND
                        s.risk = 3 THEN
                    1
                   ELSE
                    0
                 END low_risk,
                 CASE
                   WHEN s.para_added_on <
                        P_END_DATE + 1 AND
                        (s.close_on IS NULL OR
                        s.close_on >=
                        P_END_DATE + 1) AND
                        s.risk = 2 THEN
                    1
                   ELSE
                    0
                 END medium_risk,
                 CASE
                   WHEN s.para_added_on <
                        P_END_DATE + 1 AND
                        (s.close_on IS NULL OR
                        s.close_on >=
                        P_END_DATE + 1) AND
                        s.risk = 1 THEN
                    1
                   ELSE
                    0
                 END high_risk,
                 CASE
                   WHEN s.para_added_on <
                        P_END_DATE + 1 AND
                        (s.close_on IS NULL OR
                        s.close_on >=
                        P_END_DATE + 1) AND
                        (s.risk IS NULL OR
                        s.risk NOT IN (1, 2, 3)) THEN
                    1
                   ELSE
                    0
                 END unrated_risk,
                 CASE
                   WHEN s.close_on >= P_START_DATE AND
                        s.close_on <
                        P_END_DATE + 1 AND
                        TRUNC(s.history_close_on) =
                        TRUNC(s.close_on) AND
                        s.history_close_stage NOT IN
                        (41, 45) THEN
                    1
                   ELSE
                    0
                 END settled_audit,
                 CASE
                   WHEN s.close_on >= P_START_DATE AND
                        s.close_on <
                        P_END_DATE + 1 AND
                        TRUNC(s.history_close_on) =
                        TRUNC(s.close_on) AND
                        s.history_close_stage IN
                        (41, 45) THEN
                    1
                   ELSE
                    0
                 END settled_compliance,
                 CASE
                   WHEN s.close_on >= P_START_DATE AND
                        s.close_on <
                        P_END_DATE + 1 AND
                        (s.history_close_on IS NULL OR
                        TRUNC(s.history_close_on) <>
                        TRUNC(s.close_on) OR
                        s.history_close_stage IS NULL) THEN
                    1
                   ELSE
                    0
                 END settled_unknown,
                 0 reopening_events,
                 0 resettlement_events,
                 0 initially_closed_additions,
                 CASE
                   WHEN s.para_added_on <
                        P_END_DATE + 1 THEN
                    (CASE
                      WHEN s.para_added_on <
                           P_END_DATE + 1 AND
                           (s.close_on IS NULL OR
                           s.close_on >=
                           P_END_DATE + 1) THEN
                       1
                      ELSE
                       0
                    END) - (CASE
                      WHEN s.para_added_on <
                           P_START_DATE AND
                           (s.close_on IS NULL OR
                           s.close_on >=
                           P_START_DATE) THEN
                       1
                      ELSE
                       0
                    END) - (CASE
                      WHEN s.para_added_on >=
                           P_START_DATE AND
                           s.para_added_on <
                           P_END_DATE + 1 THEN
                       1
                      ELSE
                       0
                    END) + (CASE
                      WHEN s.close_on >= P_START_DATE AND
                           s.close_on <
                           P_END_DATE + 1 THEN
                       1
                      ELSE
                       0
                    END)
                   ELSE
                    0
                 END reconciliation_difference,
                 s.auditor_assignment_mismatch
            FROM V_EXEC_REPORT_SOURCE s
            LEFT JOIN T_AUDITEE_ENTITIES re
              ON re.entity_id = s.region_id
            LEFT JOIN T_AUDITEE_ENT_TYPES et
              ON et.autid = s.entity_type_id
           WHERE s.para_added_on < P_END_DATE + 1
             AND (P_SCOPE_ROOT_ID IS NULL OR
                 s.entity_id = P_SCOPE_ROOT_ID OR
                 P_SCOPE_ROOT_ID IN
                 (s.reporting_1,
                   s.reporting_2,
                   s.reporting_3,
                   s.reporting_4,
                   s.reporting_5,
                   s.reporting_6))
             AND (P_ENTITY_ID IS NULL OR
                 s.entity_id = P_ENTITY_ID OR
                 P_ENTITY_ID IN
                 (s.reporting_1,
                   s.reporting_2,
                   s.reporting_3,
                   s.reporting_4,
                   s.reporting_5,
                   s.reporting_6))
             AND (P_ENTITY_TYPE_ID IS NULL OR
                 s.entity_type_id =
                 P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR
                 s.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR
                 s.reporting_office_id =
                 P_REPORTING_OFFICE_ID)
             AND ((P_REPORT_ID = 'E' AND
                 s.audit_type = 'D') OR
                 (P_REPORT_ID <> 'E' AND
                 s.audit_type IN ('B', 'D')))),
        dimensions AS
         (SELECT f.*,
                 CASE P_REPORT_ID
                   WHEN 'B' THEN
                    entity_type_id
                   WHEN 'C' THEN
                    gm_office_id
                 END gid,
                 CASE P_REPORT_ID
                   WHEN 'B' THEN
                    NVL(entity_type_name,
                        'Unassigned category')
                   WHEN 'C' THEN
                    NVL(gm_name, 'Unassigned GM')
                 END gname,
                 CASE P_REPORT_ID
                   WHEN 'B' THEN
                    audited_by
                   WHEN 'E' THEN
                    NVL(division_id,
                        business_group_id)
                   ELSE
                    region_id
                 END iid,
                 CASE
                   WHEN P_REPORT_ID IN ('C', 'D') THEN
                    region_code
                 END icode,
                 CASE P_REPORT_ID
                   WHEN 'B' THEN
                    NVL(audited_by_name,
                        'Unassigned audit office')
                   WHEN 'E' THEN
                    NVL(division_name,
                        NVL(group_name,
                            'Unassigned management function'))
                   ELSE
                    NVL(region_name,
                        'Unassigned region')
                 END iname
            FROM facts f),
        aggregated AS
         (SELECT CASE
                   WHEN GROUPING(gname) = 1 THEN
                    'TOTAL'
                   WHEN GROUPING(iname) = 1 THEN
                    'SUBTOTAL'
                   ELSE
                    'DETAIL'
                 END ROW_KIND,
                 gid GROUP_ID,
                 gname GROUP_NAME,
                 iid ITEM_ID,
                 icode ITEM_CODE,
                 iname ITEM_NAME,
                 P_START_DATE PERIOD_START,
                 P_END_DATE PERIOD_END,
                 NVL(SUM(OPENING_BALANCE), 0) OPENING_BALANCE,
                 NVL(SUM(PARAS_ADDED), 0) PARAS_ADDED,
                 NVL(SUM(OPENING_BALANCE +
                         PARAS_ADDED),
                     0) TOTAL_PARAS,
                 NVL(SUM(SETTLED_PARAS), 0) SETTLED_PARAS,
                 NVL(SUM(OUTSTANDING_PARAS), 0) OUTSTANDING_PARAS,
                 ROUND(100 * SUM(SETTLED_PARAS) /
                       NULLIF(SUM(OPENING_BALANCE +
                                  PARAS_ADDED),
                              0),
                       2) SETTLEMENT_PERCENTAGE,
                 NVL(SUM(LOW_RISK), 0) LOW_RISK,
                 NVL(SUM(MEDIUM_RISK), 0) MEDIUM_RISK,
                 NVL(SUM(HIGH_RISK), 0) HIGH_RISK,
                 NVL(SUM(UNRATED_RISK), 0) UNRATED_RISK,
                 NVL(SUM(SETTLED_AUDIT), 0) SETTLED_AUDIT,
                 NVL(SUM(SETTLED_COMPLIANCE), 0) SETTLED_COMPLIANCE,
                 NVL(SUM(SETTLED_UNKNOWN), 0) SETTLED_UNKNOWN,
                 NVL(SUM(REOPENING_EVENTS), 0) REOPENING_EVENTS,
                 NVL(SUM(RESETTLEMENT_EVENTS), 0) RESETTLEMENT_EVENTS,
                 NVL(SUM(INITIALLY_CLOSED_ADDITIONS),
                     0) INITIALLY_CLOSED_ADDITIONS,
                 NVL(SUM(RECONCILIATION_DIFFERENCE),
                     0) RECONCILIATION_DIFFERENCE,
                 NVL(SUM(AUDITOR_ASSIGNMENT_MISMATCH),
                     0) AUDITOR_ASSIGNMENT_MISMATCH
            FROM dimensions
           GROUP BY GROUPING SETS((gid, gname, iid, icode, iname),(gid, gname),())),
        rankable AS
         (SELECT a.*,
                 ROW_NUMBER() OVER (ORDER BY SETTLED_PARAS / NULLIF(TOTAL_PARAS, 0) DESC, ITEM_ID) top_rank,
                 ROW_NUMBER() OVER (ORDER BY SETTLED_PARAS / NULLIF(TOTAL_PARAS, 0) ASC, ITEM_ID) bottom_rank
            FROM aggregated a
           WHERE ROW_KIND = 'DETAIL'
             AND TOTAL_PARAS > 0
             AND ITEM_ID IS NOT NULL),
        report_result AS
         (SELECT a.*,
                 CAST(NULL AS VARCHAR2(30)) RANKING_GROUP,
                 CAST(NULL AS NUMBER) RANK_POSITION
            FROM aggregated a
           WHERE P_REPORT_ID <> 'D'
             AND (P_REPORT_ID <> 'E' OR
                 ROW_KIND <> 'SUBTOTAL')
          UNION ALL
          SELECT r.ROW_KIND,
                 r.GROUP_ID,
                 r.GROUP_NAME,
                 r.ITEM_ID,
                 r.ITEM_CODE,
                 r.ITEM_NAME,
                 r.PERIOD_START,
                 r.PERIOD_END,
                 r.OPENING_BALANCE,
                 r.PARAS_ADDED,
                 r.TOTAL_PARAS,
                 r.SETTLED_PARAS,
                 r.OUTSTANDING_PARAS,
                 r.SETTLEMENT_PERCENTAGE,
                 r.LOW_RISK,
                 r.MEDIUM_RISK,
                 r.HIGH_RISK,
                 r.UNRATED_RISK,
                 r.SETTLED_AUDIT,
                 r.SETTLED_COMPLIANCE,
                 r.SETTLED_UNKNOWN,
                 r.REOPENING_EVENTS,
                 r.RESETTLEMENT_EVENTS,
                 r.INITIALLY_CLOSED_ADDITIONS,
                 r.RECONCILIATION_DIFFERENCE,
                 r.AUDITOR_ASSIGNMENT_MISMATCH,
                 'TOP',
                 r.top_rank
            FROM rankable r
           WHERE P_REPORT_ID = 'D'
             AND r.top_rank <= 5
          UNION ALL
          SELECT r.ROW_KIND,
                 r.GROUP_ID,
                 r.GROUP_NAME,
                 r.ITEM_ID,
                 r.ITEM_CODE,
                 r.ITEM_NAME,
                 r.PERIOD_START,
                 r.PERIOD_END,
                 r.OPENING_BALANCE,
                 r.PARAS_ADDED,
                 r.TOTAL_PARAS,
                 r.SETTLED_PARAS,
                 r.OUTSTANDING_PARAS,
                 r.SETTLEMENT_PERCENTAGE,
                 r.LOW_RISK,
                 r.MEDIUM_RISK,
                 r.HIGH_RISK,
                 r.UNRATED_RISK,
                 r.SETTLED_AUDIT,
                 r.SETTLED_COMPLIANCE,
                 r.SETTLED_UNKNOWN,
                 r.REOPENING_EVENTS,
                 r.RESETTLEMENT_EVENTS,
                 r.INITIALLY_CLOSED_ADDITIONS,
                 r.RECONCILIATION_DIFFERENCE,
                 r.AUDITOR_ASSIGNMENT_MISMATCH,
                 'BOTTOM',
                 r.bottom_rank
            FROM rankable r
           WHERE P_REPORT_ID = 'D'
             AND r.bottom_rank <= 5
          UNION ALL
          SELECT a.*,
                 CASE
                   WHEN a.ITEM_ID IS NULL THEN
                    'UNASSIGNED_REGION'
                   ELSE
                    'ZERO_DENOMINATOR'
                 END,
                 CAST(NULL AS NUMBER)
            FROM aggregated a
           WHERE P_REPORT_ID = 'D'
             AND a.ROW_KIND = 'DETAIL'
             AND (a.TOTAL_PARAS = 0 OR
                 a.ITEM_ID IS NULL))
        SELECT *
          FROM report_result
         ORDER BY RANKING_GROUP,
                  RANK_POSITION,
                  GROUP_NAME,
                  GROUP_ID,
                  CASE ROW_KIND
                    WHEN 'DETAIL' THEN
                     0
                    WHEN 'SUBTOTAL' THEN
                     1
                    ELSE
                     2
                  END,
                  ITEM_NAME,
                  ITEM_ID;
    END IF;
  END OPEN_REPORT;
  PROCEDURE P_ANNUAL_FIELD_PROGRESS(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN_REPORT('A',
                P_DASHBOARD_TYPE,
                P_SCOPE_ROOT_ID,
                P_START_DATE,
                P_END_DATE,
                P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID,
                P_REPORTING_OFFICE_ID,
                P_ENTITY_ID,
                IO_CURSOR);
  END P_ANNUAL_FIELD_PROGRESS;

  PROCEDURE P_FIELD_OFFICE_POSITION(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN_REPORT('B',
                P_DASHBOARD_TYPE,
                P_SCOPE_ROOT_ID,
                P_START_DATE,
                P_END_DATE,
                P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID,
                P_REPORTING_OFFICE_ID,
                P_ENTITY_ID,
                IO_CURSOR);
  END P_FIELD_OFFICE_POSITION;

  PROCEDURE P_GM_REGION_POSITION(P_DASHBOARD_TYPE      IN VARCHAR2,
                                 P_SCOPE_ROOT_ID       IN NUMBER,
                                 P_START_DATE          IN DATE,
                                 P_END_DATE            IN DATE,
                                 P_ENTITY_TYPE_ID      IN NUMBER,
                                 P_AUDITED_BY_ID       IN NUMBER,
                                 P_REPORTING_OFFICE_ID IN NUMBER,
                                 P_ENTITY_ID           IN NUMBER,
                                 IO_CURSOR             OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN_REPORT('C',
                P_DASHBOARD_TYPE,
                P_SCOPE_ROOT_ID,
                P_START_DATE,
                P_END_DATE,
                P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID,
                P_REPORTING_OFFICE_ID,
                P_ENTITY_ID,
                IO_CURSOR);
  END P_GM_REGION_POSITION;

  PROCEDURE P_REGION_RANKINGS(P_DASHBOARD_TYPE      IN VARCHAR2,
                              P_SCOPE_ROOT_ID       IN NUMBER,
                              P_START_DATE          IN DATE,
                              P_END_DATE            IN DATE,
                              P_ENTITY_TYPE_ID      IN NUMBER,
                              P_AUDITED_BY_ID       IN NUMBER,
                              P_REPORTING_OFFICE_ID IN NUMBER,
                              P_ENTITY_ID           IN NUMBER,
                              IO_CURSOR             OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN_REPORT('D',
                P_DASHBOARD_TYPE,
                P_SCOPE_ROOT_ID,
                P_START_DATE,
                P_END_DATE,
                P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID,
                P_REPORTING_OFFICE_ID,
                P_ENTITY_ID,
                IO_CURSOR);
  END P_REGION_RANKINGS;

  PROCEDURE P_MANAGEMENT_SETTLEMENT(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN_REPORT('E',
                P_DASHBOARD_TYPE,
                P_SCOPE_ROOT_ID,
                P_START_DATE,
                P_END_DATE,
                P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID,
                P_REPORTING_OFFICE_ID,
                P_ENTITY_ID,
                IO_CURSOR);
  END P_MANAGEMENT_SETTLEMENT;
END PKG_EXEC_REPORTS;

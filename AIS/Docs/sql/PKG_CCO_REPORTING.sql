-- COMPILATION-CORRECTION CANDIDATE ONLY; no security activation or grants.
-- Run manually using an authorized database session after reviewing existing
-- ZTBLAIS_PROD package differences. This package still raises ORA-20803 on reads.
-- AUDITEDBY reflects the confirmed T_AUDITEE_ENTITIES_MAPING column.
CREATE OR REPLACE PACKAGE PKG_CCO_REPORTING AS
  PROCEDURE P_GET_ORG_FILTERS(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ORG_POSITION(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ORG_PERFORMANCE(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ORG_SETTLEMENT(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ORG_EXCEPTIONS(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR);
END PKG_CCO_REPORTING;
/

CREATE OR REPLACE PACKAGE BODY PKG_CCO_REPORTING AS
  PROCEDURE REQUIRE_VERIFIED_CONTEXT(P_SCOPE_TOKEN VARCHAR2) IS
  BEGIN
    -- Deliberately non-configurable fail-closed gate. Replace only after signed
    -- staging taxonomy, mapping/source reconciliation, secure adapter and grants.
    RAISE_APPLICATION_ERROR(-20803,'CCO organizational reporting is not verified');
  END;

  PROCEDURE VALIDATE_INPUT(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    REQUIRE_VERIFIED_CONTEXT(P_SCOPE_TOKEN);
    IF P_START_DATE IS NULL OR P_END_DATE IS NULL OR P_START_DATE>P_END_DATE
      OR P_START_DATE<>TRUNC(P_START_DATE) OR P_END_DATE<>TRUNC(P_END_DATE)
      OR P_END_DATE>TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE '+05:00' AS DATE))
      OR MONTHS_BETWEEN(P_END_DATE,P_START_DATE)>=1200
      OR P_GROUP_BY IS NULL OR P_GROUP_BY NOT IN ('GROUP','DIVISION','GM')
      OR P_MAX_ROWS IS NULL OR P_MAX_ROWS<1 OR P_MAX_ROWS>1000 OR P_MAX_ROWS<>TRUNC(P_MAX_ROWS)
      OR P_GROUP_ID<=0 OR P_DIVISION_ID<=0 OR P_GM_OFFICE_ID<=0 OR P_REGION_ID<=0 OR P_ENTITY_ID<=0
      OR P_GROUP_ID<>TRUNC(P_GROUP_ID) OR P_DIVISION_ID<>TRUNC(P_DIVISION_ID)
      OR P_GM_OFFICE_ID<>TRUNC(P_GM_OFFICE_ID) OR P_REGION_ID<>TRUNC(P_REGION_ID) OR P_ENTITY_ID<>TRUNC(P_ENTITY_ID)
      OR (P_DIVISION_ID IS NOT NULL AND P_GROUP_ID IS NULL)
      OR (P_GM_OFFICE_ID IS NOT NULL AND (P_GROUP_ID IS NULL OR P_DIVISION_ID IS NULL)) THEN
      RAISE_APPLICATION_ERROR(-20801,'Invalid CCO organization selection');
    END IF;
    -- REQUIRED ADAPTER CONTRACT: validate selected IDs against the unique,
    -- context-authorized path set BEFORE opening any cursor. No-match/conflict
    -- must raise -20802, never return a wider/empty substitute. Missing adapter
    -- remains blocked by REQUIRE_VERIFIED_CONTEXT, including selector reads.
  END;

  PROCEDURE READ_ORGANIZATIONS(P_MODE VARCHAR2, P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
    V_DUPLICATES NUMBER;
  BEGIN
    VALIDATE_INPUT(P_SCOPE_TOKEN,P_START_DATE,P_END_DATE,P_GROUP_BY,P_GROUP_ID,
      P_DIVISION_ID,P_GM_OFFICE_ID,P_REGION_ID,P_ENTITY_ID,P_MAX_ROWS,IO_CURSOR);
    -- Reject duplicate facts instead of selecting an arbitrary COM_ID row.
    SELECT COUNT(*) INTO V_DUPLICATES FROM (
      SELECT r.com_id FROM V_EXEC_REPORT_SOURCE r
      WHERE EXISTS (SELECT 1 FROM V_CCO_AUTHORIZED_ENTITIES a
        WHERE a.scope_token=P_SCOPE_TOKEN AND a.entity_id=r.entity_id)
      GROUP BY r.com_id HAVING COUNT(*)<>1 OR r.com_id IS NULL);
    IF V_DUPLICATES>0 THEN RAISE_APPLICATION_ERROR(-20803,'Duplicate authorized source facts'); END IF;
    SELECT COUNT(*) INTO V_DUPLICATES FROM (
      SELECT k.com_id FROM V_EXEC_KPI_LIFECYCLE k
      WHERE k.record_type='PARA' AND k.excluded_pre_finalization=0
        AND EXISTS (SELECT 1 FROM V_CCO_AUTHORIZED_ENTITIES a
          WHERE a.scope_token=P_SCOPE_TOKEN AND a.entity_id=k.entity_id)
      GROUP BY k.com_id HAVING COUNT(*)<>1 OR k.com_id IS NULL);
    IF V_DUPLICATES>0 THEN RAISE_APPLICATION_ERROR(-20803,'Duplicate lifecycle facts'); END IF;
    OPEN IO_CURSOR FOR
    WITH hierarchy AS (
      -- Keep one row per auditee, with explicit ambiguity. Never join raw maps
      -- or raw reporting-chain rows to para facts.
      SELECT entity_id, COUNT(*) path_rows,
        CASE WHEN COUNT(*)=1 THEN MAX(business_group_id) END group_id,
        CASE WHEN COUNT(*)=1 THEN MAX(division_id) END division_id,
        CASE WHEN COUNT(*)=1 THEN MAX(gm_office_id) END gm_id
      FROM V_EXEC_ENTITY_SCOPE GROUP BY entity_id
    ), active_map AS (
      SELECT m.entity_id, COUNT(DISTINCT m.c_type_id) type_count,
        COUNT(DISTINCT m.parent_id) parent_count, COUNT(DISTINCT m.gm_office) gm_count,
        COUNT(DISTINCT m.auditedby) auditor_count,
        CASE WHEN COUNT(DISTINCT m.auditedby)=1 THEN MAX(m.auditedby) END auditedby,
        CASE WHEN COUNT(DISTINCT m.gm_office)=1 AND MIN(CASE WHEN m.gm_office IS NULL OR g.type_id=21 THEN 1 ELSE 0 END)=1
          THEN MAX(m.gm_office) END direct_gm,
        MIN(CASE WHEN m.gm_office IS NULL OR g.type_id=21 THEN 1 ELSE 0 END) valid_gm
      FROM T_AUDITEE_ENTITIES_MAPING m LEFT JOIN T_AUDITEE_ENTITIES g ON g.entity_id=m.gm_office
      WHERE NVL(m.status,'Y')='Y' GROUP BY m.entity_id
    ), auditor_gm AS (
      SELECT m.auditedby AS auditedby, COUNT(DISTINCT m.entity_id) gm_candidates,
        CASE WHEN COUNT(DISTINCT m.entity_id)=1 THEN MAX(m.entity_id) END gm_id
      FROM T_AUDITEE_ENTITIES_MAPING m JOIN T_AUDITEE_ENTITIES g ON g.entity_id=m.entity_id AND g.type_id=21
      WHERE NVL(m.status,'Y')='Y' AND m.auditedby IS NOT NULL GROUP BY m.auditedby
    ), entity_family AS (
      SELECT r.entity_id,COUNT(DISTINCT r.audit_type) family_count,
        CASE WHEN COUNT(DISTINCT r.audit_type)=1 THEN MAX(r.audit_type) END audit_family
      FROM V_EXEC_REPORT_SOURCE r
      WHERE EXISTS (SELECT 1 FROM V_CCO_AUTHORIZED_ENTITIES a WHERE a.scope_token=P_SCOPE_TOKEN AND a.entity_id=r.entity_id)
      GROUP BY r.entity_id
    ), candidate_path AS (
      SELECT e.entity_id,h.group_id,h.division_id,h.path_rows,m.type_count,m.parent_count,
        m.gm_count,m.auditor_count,m.valid_gm,ef.family_count,
        CASE WHEN m.gm_count>1 OR m.auditor_count>1 OR m.valid_gm=0 THEN NULL
          WHEN e.type_id=21 THEN e.entity_id
          WHEN ef.audit_family='B' AND e.type_id NOT IN (6,21,22,28) THEN
            CASE WHEN m.auditedby IS NOT NULL AND gm_map.auditor_count=1 AND gm_map.auditedby=m.auditedby
                 THEN m.direct_gm
                 WHEN m.gm_count=0 AND ag.gm_candidates=1 THEN ag.gm_id END
          ELSE COALESCE(m.direct_gm,h.gm_id) END gm_id
      FROM T_AUDITEE_ENTITIES e LEFT JOIN hierarchy h ON h.entity_id=e.entity_id
      LEFT JOIN entity_family ef ON ef.entity_id=e.entity_id
      LEFT JOIN active_map m ON m.entity_id=e.entity_id
      LEFT JOIN active_map gm_map ON gm_map.entity_id=m.direct_gm
      LEFT JOIN auditor_gm ag ON ag.auditedby=m.auditedby
    ), paths AS (
      SELECT c.entity_id,c.group_id,c.division_id,
        CASE WHEN c.path_rows=1 AND gh.path_rows=1
          AND DECODE(c.group_id,gh.group_id,1,0)=1 AND DECODE(c.division_id,gh.division_id,1,0)=1
          THEN c.gm_id END gm_id,
        CASE WHEN c.path_rows<>1 OR c.family_count>1 OR c.type_count>1 OR c.parent_count>1 OR c.gm_count>1 OR c.auditor_count>1 OR c.valid_gm=0
          OR (c.gm_id IS NOT NULL AND (gh.path_rows IS NULL OR gh.path_rows<>1
             OR DECODE(c.group_id,gh.group_id,1,0)=0 OR DECODE(c.division_id,gh.division_id,1,0)=0))
          THEN 'CONFLICT'
          WHEN c.group_id IS NULL THEN 'UNMAPPED'
          WHEN c.gm_id IS NULL AND c.division_id IS NULL THEN 'DIRECT_TO_GROUP'
          WHEN c.gm_id IS NULL THEN 'DIRECT_TO_DIVISION' ELSE 'VERIFIED_CANDIDATE' END mapping_quality
      FROM candidate_path c LEFT JOIN hierarchy gh ON gh.entity_id=c.gm_id
    ), qualified AS (
      -- Auditee ENTITY_ID controls ownership. AUDITED_BY stays a separate dimension.
      -- Candidate B/C/D population requires reconciliation before acceptance.
      -- Project only required fact fields; r.* may duplicate lifecycle columns
      -- (for example COM_CYCLE) and cause ORA-00918 in the parent SQL.
      SELECT r.com_id, r.entity_id, r.region_id, r.risk,
        r.effective_added_on, r.close_on,
        p.group_id AS org_group, p.division_id AS org_division,
        p.gm_id AS org_gm, p.mapping_quality,
        k.com_id AS lifecycle_id, k.com_cycle AS com_cycle,
        k.para_added_on AS lifecycle_added_on
      FROM V_EXEC_REPORT_SOURCE r LEFT JOIN paths p ON p.entity_id=r.entity_id
      LEFT JOIN V_EXEC_KPI_LIFECYCLE k ON k.com_id=r.com_id AND k.record_type='PARA' AND k.excluded_pre_finalization=0
      WHERE EXISTS (SELECT 1 FROM V_CCO_AUTHORIZED_ENTITIES a WHERE a.scope_token=P_SCOPE_TOKEN AND a.entity_id=r.entity_id)
        AND r.audit_type IN ('B','D')
        AND (P_GROUP_ID IS NULL OR p.group_id=P_GROUP_ID)
        AND (P_DIVISION_ID IS NULL OR p.division_id=P_DIVISION_ID)
        AND (P_GM_OFFICE_ID IS NULL OR p.gm_id=P_GM_OFFICE_ID)
        AND (P_REGION_ID IS NULL OR r.region_id=P_REGION_ID)
        AND (P_ENTITY_ID IS NULL OR r.entity_id=P_ENTITY_ID)
    ), periods AS (
      SELECT CASE WHEN P_MODE='SETTLEMENT' THEN GREATEST(P_START_DATE,ADD_MONTHS(TRUNC(P_START_DATE,'MM'),LEVEL-1)) ELSE P_START_DATE END date_from,
        CASE WHEN P_MODE='SETTLEMENT' THEN LEAST(P_END_DATE+1,ADD_MONTHS(TRUNC(P_START_DATE,'MM'),LEVEL)) ELSE P_END_DATE+1 END date_to
      FROM dual CONNECT BY LEVEL<=CASE WHEN P_MODE='SETTLEMENT' THEN MONTHS_BETWEEN(TRUNC(P_END_DATE,'MM'),TRUNC(P_START_DATE,'MM'))+1 ELSE 1 END
    ), flags AS (
      SELECT q.com_id, q.entity_id, q.region_id, q.risk,
        q.effective_added_on, q.close_on, q.org_group, q.org_division,
        q.org_gm, q.mapping_quality, q.lifecycle_id, q.com_cycle,
        q.lifecycle_added_on, d.date_from, d.date_to,
        CASE WHEN q.effective_added_on<d.date_from AND (q.close_on IS NULL OR q.close_on>=d.date_from) THEN 1 ELSE 0 END opening_cases,
        CASE WHEN q.effective_added_on>=d.date_from AND q.effective_added_on<d.date_to THEN 1 ELSE 0 END para_added,
        CASE WHEN q.close_on>=d.date_from AND q.close_on<d.date_to AND q.effective_added_on<d.date_to THEN 1 ELSE 0 END settled,
        CASE WHEN q.effective_added_on<d.date_to AND (q.close_on IS NULL OR q.close_on>=d.date_to) THEN 1 ELSE 0 END outstanding
      FROM qualified q CROSS JOIN periods d
      WHERE q.effective_added_on<d.date_to OR q.effective_added_on IS NULL
    ), grouped AS (
      SELECT date_from,date_to,org_group,org_division,org_gm,GROUPING_ID(org_group,org_division,org_gm) grouping_id,
        SUM(opening_cases) opening_cases,SUM(para_added) para_added,SUM(opening_cases+para_added) total_cases,
        SUM(settled) paras_settled,SUM(outstanding) outstanding,
        SUM(CASE WHEN outstanding=1 AND risk=1 THEN 1 ELSE 0 END) high_risk_open,
        CASE WHEN SUM(CASE WHEN outstanding=1 AND (lifecycle_id IS NULL OR com_cycle IS NULL) THEN 1 ELSE 0 END)=0
          THEN SUM(CASE WHEN outstanding=1 AND com_cycle=0 THEN 1 ELSE 0 END) END zero_cycle_open,
        CASE WHEN SUM(CASE WHEN outstanding=1 AND (lifecycle_id IS NULL OR com_cycle IS NULL) THEN 1 ELSE 0 END)=0
          THEN SUM(CASE WHEN outstanding=1 AND com_cycle>3 THEN 1 ELSE 0 END) END over3_cycle_open,
        CASE WHEN SUM(CASE WHEN outstanding=1 AND lifecycle_added_on IS NULL THEN 1 ELSE 0 END)=0
          THEN SUM(CASE WHEN outstanding=1 AND date_to-1-lifecycle_added_on>365 THEN 1 ELSE 0 END) END aged_over365_open,
        SUM(CASE WHEN mapping_quality IS NULL OR mapping_quality IN ('UNMAPPED','CONFLICT') THEN 1 ELSE 0 END) mapping_exceptions,
        SUM(CASE WHEN lifecycle_id IS NULL OR effective_added_on IS NULL OR lifecycle_added_on<>effective_added_on OR
          (outstanding=1 AND (lifecycle_added_on IS NULL OR com_cycle IS NULL)) THEN 1 ELSE 0 END) quality_exceptions
      FROM flags GROUP BY date_from,date_to,GROUPING SETS ((org_group),(org_group,org_division),(org_group,org_division,org_gm),())
    )
    SELECT * FROM (
      SELECT g.date_from, g.date_to, g.org_group, g.org_division,
        g.org_gm, g.grouping_id, g.opening_cases, g.para_added,
        g.total_cases, g.paras_settled, g.outstanding,
        g.high_risk_open, g.zero_cycle_open, g.over3_cycle_open,
        g.aged_over365_open, g.mapping_exceptions, g.quality_exceptions,
        g.outstanding-g.opening_cases AS net_backlog_change,
        g.outstanding-g.opening_cases-g.para_added+g.paras_settled AS reconciliation_difference,
        CASE WHEN g.total_cases>0 AND g.outstanding-g.opening_cases-g.para_added+g.paras_settled=0 AND g.quality_exceptions=0
          THEN ROUND(100*g.paras_settled/g.total_cases,2) END AS settlement_rate,
        CASE WHEN g.grouping_id=7 THEN 'TOTAL'
          WHEN (P_GROUP_BY='GROUP' AND g.grouping_id=3) OR (P_GROUP_BY='DIVISION' AND g.grouping_id=1)
            OR (P_GROUP_BY='GM' AND g.grouping_id=0) THEN 'DETAIL' ELSE 'SUBTOTAL' END AS row_kind,
        'HISTORICAL_ESTIMATE; CURRENT_CYCLES; MOVEMENT_RECENCY_UNKNOWN' historical_quality,
        ROW_NUMBER() OVER (ORDER BY g.date_from,g.org_group NULLS LAST,g.org_division NULLS LAST,g.org_gm NULLS LAST,g.grouping_id) AS row_no,
        COUNT(*) OVER () source_row_count
      FROM grouped g
      WHERE (g.grouping_id=7 OR (P_GROUP_BY='GROUP' AND g.grouping_id=3)
        OR (P_GROUP_BY='DIVISION' AND g.grouping_id IN (1,3)) OR P_GROUP_BY='GM')
        AND (g.grouping_id=7 OR g.opening_cases<>0 OR g.para_added<>0 OR g.paras_settled<>0 OR g.outstanding<>0 OR g.mapping_exceptions<>0 OR g.quality_exceptions<>0)
        AND (P_MODE<>'EXCEPTIONS' OR g.mapping_exceptions>0 OR g.quality_exceptions>0 OR g.outstanding-g.opening_cases-g.para_added+g.paras_settled<>0)
    ) WHERE row_no<=P_MAX_ROWS;
    -- Cursor protocol returns SOURCE_ROW_COUNT so truncation cannot masquerade
    -- as a complete rollup. Final reader must reject overflow, not sum partial rows.
    REQUIRE_VERIFIED_CONTEXT(P_SCOPE_TOKEN);
  END;

  PROCEDURE P_GET_ORG_FILTERS(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    REQUIRE_VERIFIED_CONTEXT(P_SCOPE_TOKEN);
    -- Selector implementation is deliberately blocked until its authorized
    -- unique path reader is approved. No unscoped directory fallback.
    RAISE_APPLICATION_ERROR(-20803,'Verified CCO selector path reader pending');
  END P_GET_ORG_FILTERS;

  PROCEDURE P_GET_ORG_POSITION(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    READ_ORGANIZATIONS('POSITION',P_SCOPE_TOKEN,P_START_DATE,P_END_DATE,P_GROUP_BY,P_GROUP_ID,
      P_DIVISION_ID,P_GM_OFFICE_ID,P_REGION_ID,P_ENTITY_ID,P_MAX_ROWS,IO_CURSOR);
  END P_GET_ORG_POSITION;

  PROCEDURE P_GET_ORG_PERFORMANCE(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    READ_ORGANIZATIONS('PERFORMANCE',P_SCOPE_TOKEN,P_START_DATE,P_END_DATE,P_GROUP_BY,P_GROUP_ID,
      P_DIVISION_ID,P_GM_OFFICE_ID,P_REGION_ID,P_ENTITY_ID,P_MAX_ROWS,IO_CURSOR);
  END P_GET_ORG_PERFORMANCE;

  PROCEDURE P_GET_ORG_SETTLEMENT(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    READ_ORGANIZATIONS('SETTLEMENT',P_SCOPE_TOKEN,P_START_DATE,P_END_DATE,P_GROUP_BY,P_GROUP_ID,
      P_DIVISION_ID,P_GM_OFFICE_ID,P_REGION_ID,P_ENTITY_ID,P_MAX_ROWS,IO_CURSOR);
  END P_GET_ORG_SETTLEMENT;

  PROCEDURE P_GET_ORG_EXCEPTIONS(P_SCOPE_TOKEN IN VARCHAR2, P_START_DATE IN DATE, P_END_DATE IN DATE,
    P_GROUP_BY IN VARCHAR2, P_GROUP_ID IN NUMBER, P_DIVISION_ID IN NUMBER,
    P_GM_OFFICE_ID IN NUMBER, P_REGION_ID IN NUMBER, P_ENTITY_ID IN NUMBER,
    P_MAX_ROWS IN NUMBER, IO_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    READ_ORGANIZATIONS('EXCEPTIONS',P_SCOPE_TOKEN,P_START_DATE,P_END_DATE,P_GROUP_BY,P_GROUP_ID,
      P_DIVISION_ID,P_GM_OFFICE_ID,P_REGION_ID,P_ENTITY_ID,P_MAX_ROWS,IO_CURSOR);
  END P_GET_ORG_EXCEPTIONS;
END PKG_CCO_REPORTING;
/

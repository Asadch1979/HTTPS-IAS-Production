-- CCO_FUNCTION_POSITION_MANUAL_TEST.sql
-- PL/SQL Developer SQL Window: execute each SELECT separately with F8.
-- READ ONLY. Does not call PKG_CCO_SCOPE, set trusted context, modify
-- PKG_CCO_FUNCTION_POSITION, grant privileges or expose an unscoped API.
--
-- Dates match reported failing request: 01-Jan-2026 to 30-Oct-2026,
-- maxRows = 1000. Run ONLY under a DBA-approved, already-authorized
-- Oracle database account with appropriate SELECT rights.
-- WARNING: This diagnostic has no IAS application session scope.
-- Data returned may differ from the properly authorized CCO session;
-- do not use it as an application/production API substitute.
-- The report SQL below is derived from the canonical repository
-- PKG_CCO_FUNCTION_POSITION source. Scope-dependent predicates are
-- intentionally omitted only for this manual read-only diagnosis.

-- STEP 1 - Confirm schema
SELECT USER AS CONNECTED_USER,
       SYS_CONTEXT('USERENV','CURRENT_SCHEMA') AS CURRENT_SCHEMA
  FROM DUAL;

-- STEP 2 - Raw source cardinality / date nulls for the report period
SELECT COUNT(*) AS SOURCE_ROWS,
       COUNT(DISTINCT S.COM_ID) AS DISTINCT_COM_IDS,
       SUM(CASE WHEN S.COM_ID IS NULL THEN 1 ELSE 0 END) AS NULL_COM_IDS,
       SUM(CASE WHEN S.CLOSE_ON IS NULL THEN 1 ELSE 0 END) AS OPEN_OR_UNDATED_ROWS
  FROM V_EXEC_REPORT_SOURCE S
 WHERE S.AUDIT_TYPE = 'D'
   AND S.EFFECTIVE_ADDED_ON < DATE '2026-10-31';

-- STEP 3 - Exact duplicate source rejection from the report package.
-- If this returns ANY row, PKG_CCO_FUNCTION_POSITION may raise ORA-20803.
-- Unlike the live package, this checks all rows visible to your DB account.
SELECT S.COM_ID, COUNT(*) AS SOURCE_ROWS
  FROM V_EXEC_REPORT_SOURCE S
 WHERE S.AUDIT_TYPE = 'D'
   AND S.EFFECTIVE_ADDED_ON < DATE '2026-10-31'
 GROUP BY S.COM_ID
HAVING COUNT(*) <> 1 OR S.COM_ID IS NULL
 ORDER BY SOURCE_ROWS DESC
 FETCH FIRST 100 ROWS ONLY;

-- STEP 4 - Check repeated hierarchy paths independently of the facts.
SELECT H.ENTITY_ID, COUNT(*) AS HIERARCHY_PATHS,
       COUNT(DISTINCT H.BUSINESS_GROUP_ID) AS GROUPS,
       COUNT(DISTINCT H.DIVISION_ID) AS DIVISIONS
  FROM V_EXEC_ENTITY_SCOPE H
 GROUP BY H.ENTITY_ID
HAVING COUNT(*) > 1
 ORDER BY HIERARCHY_PATHS DESC
 FETCH FIRST 100 ROWS ONLY;

-- STEP 5 - Full Function Wise Position report projection.
-- Derived directly from PKG_CCO_FUNCTION_POSITION.P_GET_POSITION,
-- using the same Group/Division mapping, grouping sets and cursor fields.
-- Only this manual diagnostic omits protected CCO scope filtering.
-- Observe SOURCE_ROW_COUNT in every output row; >1000 would be
-- rejected by the C# adapter even if SQL itself executes successfully.

WITH mapping_candidates AS (
 SELECT DISTINCT r.parent_id group_id,r.p_name group_name,
                 r.entity_id division_id,r.c_name division_name
 FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r
 WHERE EXISTS (SELECT 1 FROM T_AUDITEE_ENTITIES_MAPING m
               WHERE m.entity_id=r.entity_id AND m.parent_id=r.parent_id
                 AND m.c_type_id=3 AND NVL(m.status,'Y')='Y'
                 AND m.p_name=r.p_name AND m.c_name=r.c_name)
 ), mappings AS (
 SELECT division_id,COUNT(*) candidates,MAX(group_id) group_id,MAX(group_name) group_name,MAX(division_name) division_name
 FROM mapping_candidates GROUP BY division_id
 ), hierarchy AS (
 SELECT entity_id,COUNT(*) paths,MAX(business_group_id) group_id,MAX(division_id) division_id
 FROM V_EXEC_ENTITY_SCOPE GROUP BY entity_id
 ), facts AS (
 SELECT s.com_id,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.group_id END group_id,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.group_name ELSE 'Unmapped / conflicting Group' END group_name,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.division_id END division_id,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.division_name ELSE 'Unmapped / conflicting Division' END division_name,
 CASE WHEN s.effective_added_on<DATE '2026-01-01' AND (s.close_on IS NULL OR s.close_on>=DATE '2026-01-01') THEN 1 ELSE 0 END opening_balance,
 CASE WHEN s.effective_added_on>=DATE '2026-01-01' AND s.effective_added_on<DATE '2026-10-30'+1 THEN 1 ELSE 0 END paras_added,
 CASE WHEN s.close_on>=DATE '2026-01-01' AND s.close_on<DATE '2026-10-30'+1 AND s.effective_added_on<DATE '2026-10-30'+1 THEN 1 ELSE 0 END settled_paras,
 CASE WHEN s.effective_added_on<DATE '2026-10-30'+1 AND (s.close_on IS NULL OR s.close_on>=DATE '2026-10-30'+1) THEN 1 ELSE 0 END outstanding_paras
 FROM V_EXEC_REPORT_SOURCE s LEFT JOIN hierarchy h ON h.entity_id=s.entity_id
 LEFT JOIN mappings m ON m.division_id=h.division_id
 WHERE s.audit_type='D' AND s.effective_added_on<DATE '2026-10-30'+1
 -- Manual diagnostic only: protected scope filter intentionally omitted.
 -- This reads only the rows visible to the connected Oracle account.
 ), aggregates AS (
 SELECT CASE WHEN GROUPING(group_id)=1 THEN 'TOTAL' WHEN GROUPING(division_id)=1 THEN 'SUBTOTAL' ELSE 'DETAIL' END row_kind,
 group_id,CASE WHEN GROUPING(group_id)=1 THEN 'Grand total' ELSE group_name END group_name,
 division_id,CASE WHEN GROUPING(group_id)=1 THEN 'All authorized functions' WHEN GROUPING(division_id)=1 THEN 'Group subtotal' ELSE division_name END division_name,
 NVL(SUM(opening_balance),0) opening_balance,NVL(SUM(paras_added),0) paras_added,
 NVL(SUM(opening_balance+paras_added),0) total_paras,NVL(SUM(settled_paras),0) settled_paras,
 NVL(SUM(outstanding_paras),0) outstanding_paras,
 ROUND(100*SUM(settled_paras)/NULLIF(SUM(opening_balance+paras_added),0),2) settlement_percentage
 FROM facts GROUP BY GROUPING SETS((group_id,group_name,division_id,division_name),(group_id,group_name),())
 ), meaningful AS (
 SELECT a.* FROM aggregates a WHERE opening_balance<>0 OR paras_added<>0 OR settled_paras<>0 OR outstanding_paras<>0
 ), counted AS (SELECT a.*,COUNT(*) OVER() source_row_count FROM meaningful a)
 SELECT * FROM counted ORDER BY CASE WHEN row_kind='TOTAL' THEN 1 ELSE 0 END,group_name,group_id,
 CASE WHEN row_kind='SUBTOTAL' THEN 1 ELSE 0 END,division_name,division_id
 FETCH FIRST 1000 ROWS ONLY;

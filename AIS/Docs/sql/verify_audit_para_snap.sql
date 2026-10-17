-- MANUAL ONLY, Oracle 18c+ test schema. Do not deploy from application startup.
-- Compile canonical PKG_EXEC_DASHBOARD specification/body separately first.
-- Existing V_EXEC_KPI_LIFECYCLE and source views and approved nature map required.
SET SERVEROUTPUT ON;
SELECT type, line, position, text FROM user_errors
 WHERE name = 'PKG_EXEC_DASHBOARD' ORDER BY sequence;
SELECT auditperiodid, description, start_date, end_date
 FROM t_au_period ORDER BY start_date DESC;
-- Reconcile live workflow before setting CiaWorkflowDefinitionsVerified=true:
SELECT id, status FROM t_au_plan_eng_status ORDER BY id;
SELECT id, code, heading FROM t_audit_checklist_annexure WHERE id = 1;
-- Inspect authorized versus pending/rejected state events, and missing baselines.
SELECT status, COUNT(*) records, MIN(authorized_on), MAX(authorized_on)
 FROM t_au_paras_status_change_log GROUP BY status;

-- Substitute an approved period ID and narrow entity before bank-wide execution.
VARIABLE period_id NUMBER;
VARIABLE entity_id NUMBER;
VARIABLE rc REFCURSOR;
-- EXEC :period_id := <verified AUDITPERIODID>;
-- EXEC :entity_id := <authorized entity ID>; -- NULL only for CIA/Super User bank-wide
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS('CIA',:entity_id,:period_id,IO_CURSOR=>:rc);
END;
/
PRINT rc;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE('CIA',:entity_id,:period_id,IO_CURSOR=>:rc);
END;
/
PRINT rc;
-- Expect exactly 10 distinct approved category codes; one shared TOTAL_ISSUES.
-- Verify SUM(ISSUE_COUNT)+UNMAPPED_ISSUES=TOTAL_ISSUES, even with empty mapping.
-- Confirm audited <= planned and no duplicate entity contribution from engagements.
-- Compare delayed to PKG_RPT.P_DELAY_AUDITS + conclusion exclusion.
-- Verify 15 calendar days and conducting stages 4..11 with business owner.

BEGIN
  PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE('CIA',:entity_id,
    DATE '2026-01-01',DATE '2026-09-30',IO_CURSOR=>:rc);
END;
/
PRINT rc;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_SNAP_DISTRIBUTION('CIA',:entity_id,
    DATE '2026-01-01',DATE '2026-09-30',IO_CURSOR=>:rc);
END;
/
PRINT rc;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_SNAP_CRITICAL('CIA',:entity_id,
    DATE '2026-01-01',DATE '2026-09-30',P_MAX_ROWS=>100,IO_CURSOR=>:rc);
END;
/
PRINT rc;
-- Past risk/aging = E; past cycle counts = NULL/N. Trend covers Jan-Sep only,
-- with zero months retained. Critical rows <= 100; historical cycles NULL.
-- Sum(risk)=Sum(aging)=lifecycle closing using identical scope/end date.
-- Sum(trend)=lifecycle paras_settled using identical range and filters.
-- Today is an as-of-now position, not a completed end-of-day snapshot.
-- Repeat with each of entity type, audited-by and reporting-office filters.

-- Date boundaries: inclusive start; midnight after end excluded.
WITH events AS (
 SELECT DATE '2026-09-01' event_on FROM dual UNION ALL
 SELECT DATE '2026-09-30' + 86399/86400 FROM dual UNION ALL
 SELECT DATE '2026-10-01' FROM dual
)
SELECT COUNT(*) expected_two FROM events
 WHERE event_on >= DATE '2026-09-01' AND event_on < DATE '2026-09-30' + 1;

DECLARE
  c SYS_REFCURSOR;
  PROCEDURE expect_rejected(p_start DATE,p_end DATE,p_type VARCHAR2,p_code NUMBER) IS
  BEGIN
    BEGIN
      PKG_EXEC_DASHBOARD.P_GET_SNAP_DISTRIBUTION(p_type,:entity_id,p_start,p_end,IO_CURSOR=>c);
      CLOSE c;
      RAISE_APPLICATION_ERROR(-20999,'Expected request rejection');
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE <> p_code THEN RAISE; END IF;
    END;
  END;
BEGIN
  expect_rejected(NULL,TRUNC(SYSDATE),'CIA',-20046);
  expect_rejected(TRUNC(SYSDATE),TRUNC(SYSDATE)-1,'CIA',-20046);
  expect_rejected(TRUNC(SYSDATE),TRUNC(SYSDATE)+1,'CIA',-20046);
  expect_rejected(TRUNC(SYSDATE)-1,TRUNC(SYSDATE),'CCO',-20045);
  DBMS_OUTPUT.PUT_LINE('Snap invalid-date and perspective checks passed.');
END;
/
-- Required browser/API checks after compilation and approved grants:
-- CIA/Super User: both pages; each new GET requires exact page + API grant.
-- CCO/TOP/GROUP/RLM/Team Leader: Snap page/API 403 even with forged CIA hints.
-- Remove Snap page or exact API grant: 403. Revoked session: 401.
-- Change dates/tabs/entities/report/export: same selected dates throughout.
-- Legacy /ExecutiveOverview/CIA?view=risk redirects to Snap with valid filters.

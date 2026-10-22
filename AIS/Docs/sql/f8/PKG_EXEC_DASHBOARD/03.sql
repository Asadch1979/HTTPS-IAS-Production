-- F8: execute this file alone in PL/SQL Developer SQL Window.
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
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);


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
      IO_CURSOR OUT SYS_REFCURSOR,
      P_POSITION_AS_ON IN DATE DEFAULT NULL,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL);

  -- Exactly ten categories, plus total/unmapped diagnostic columns.
  -- Requires T_IAS_CIA_ISSUE_NATURE_MAP (deploy its DDL first).
  PROCEDURE P_GET_CIA_ISSUE_NATURE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_POSITION_AS_ON IN DATE DEFAULT NULL,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL);



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
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);
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
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);

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

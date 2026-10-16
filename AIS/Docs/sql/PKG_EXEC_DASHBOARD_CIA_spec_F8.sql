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
      IO_CURSOR OUT SYS_REFCURSOR);


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

END PKG_EXEC_DASHBOARD;

/*
Step 04 - Seed/update notification definitions.
Existing unrelated notification rows are preserved.
*/
MERGE INTO IAS_NOTIFICATION_MASTER D
USING (
  SELECT 'MGMT_AUDIT_PARA_STATUS' CODE,'Management Audit Para Status' NAME,'Compliance' MODL,
    'PKG_AE.P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW' CALLER,
    'PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_PARA_STATUS' PROC,
    'V_IAS_POST_COMPLIANCE_NOTIFY + AIS_T_AU_POST_COMPLIANCE_HISTORY' SRC,
    'IAS Notification: Audit Para No. {REFERENCE} {STATUS}' SUBJ,
    'Audit Para No. {REFERENCE} {STATUS}' TITLE,' ' SUMMARY,'WORKFLOW' NT,'N' ATT,
    'Management Audit only (AUDITED_BY 112242/112248); queued after the decision commit.' DESCR FROM DUAL
  UNION ALL
  SELECT 'MGMT_AUDIT_WEEKLY_PARA_STATUS','Weekly Management Audit Para Decisions','Compliance',
    'JOB_MGMT_AUDIT_WEEKLY_NOTIFY','PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_WEEKLY',
    'Decision history + V_IAS_MGMT_AUDIT_NOTIFY_MAP',
    'IAS Notification: Weekly Management Audit Para Decisions ({REFERENCE})',
    'Weekly Management Audit Para Decisions','Reporting period: {REFERENCE}','WORKFLOW','N',
    'Previous Monday-Sunday digest, one queue item per concerned Divisional Head with valid reporting office/group in CC.' FROM DUAL
  UNION ALL
  SELECT 'MGMT_AUDIT_MAPPING_EXCEPTION','Management Audit Entity Mapping Exceptions','Compliance',
    'JOB_MGMT_AUDIT_MAPPING_EXCEPT','PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS',
    'V_IAS_MGMT_AUDIT_NOTIFY_MAP','IAS Notification: Management Audit Entity Mapping Exceptions',
    'Management Audit Entity Mapping Exceptions','Audit domain: {REFERENCE}','WORKFLOW','N',
    'Friday exception report sent separately to the heads of 112242 and 112248 when active/auditable entity mappings are incomplete or conflicting.' FROM DUAL
  UNION ALL
  SELECT 'IAS_NOTIFICATION_HEALTH','IAS Notification Health Report','Administration',
    'JOB_IAS_NOTIFICATION_HEALTH','PKG_IAS_NOTIFICATION.SEND_NOTIFICATION_HEALTH',
    'T_AU_IID_EMAIL_QUEUE + USER_OBJECTS + USER_SCHEDULER_JOBS + USER_ARGUMENTS',
    'IAS Notification: Notification Health Report ({REFERENCE})','IAS Notification Health Report',
    'Operational control snapshot: {REFERENCE}','TECHNICAL','N',
    'Queue, delivery, retry, scheduler, P_GET_EMAIL_QUEUE contract, object-validity and processing-error report for Super Admin.' FROM DUAL
) S
ON (D.NOTIFICATION_CODE=S.CODE)
WHEN MATCHED THEN UPDATE SET
  D.NOTIFICATION_NAME=S.NAME,D.MODULE_NAME=S.MODL,D.CALLING_PROCESS=S.CALLER,
  D.RELATED_PROCEDURE=S.PROC,D.DATA_SOURCE_NAME=S.SRC,D.SUBJECT_TEMPLATE=S.SUBJ,
  D.TITLE_TEMPLATE=S.TITLE,D.SUMMARY_TEMPLATE=TO_CLOB(S.SUMMARY),
  D.NOTIFICATION_TYPE=S.NT,D.REQUIRES_ATTACHMENT=S.ATT,D.UPDATED_BY=USER,D.UPDATED_ON=SYSTIMESTAMP
WHEN NOT MATCHED THEN INSERT
  (NOTIFICATION_CODE,NOTIFICATION_NAME,MODULE_NAME,CALLING_PROCESS,RELATED_PROCEDURE,
   DATA_SOURCE_NAME,SUBJECT_TEMPLATE,TITLE_TEMPLATE,SUMMARY_TEMPLATE,
   NOTIFICATION_TYPE,REQUIRES_ATTACHMENT,IS_ACTIVE,DESCRIPTION)
VALUES
  (S.CODE,S.NAME,S.MODL,S.CALLER,S.PROC,S.SRC,S.SUBJ,S.TITLE,TO_CLOB(S.SUMMARY),
   S.NT,S.ATT,'Y',S.DESCR);

/* Preserve unrelated production customizations; only supply missing central-package prerequisites. */
MERGE INTO IAS_NOTIFICATION_MASTER D
USING (
  SELECT 'AUDIT_TEAM_ASSIGNED' CODE,'Audit Task Assigned' NAME,'Planning' MODL,
    'ApiCallsController.AddEngagementPlan' CALLER,'PKG_PG.P_ADDAUDITENGPLAN' PROC,
    'Existing AddAuditEngagementPlan result + audit-team data source' SRC,
    'IAS Notification: Audit Task Assigned - {REFERENCE}' SUBJ,'Audit Task Assigned' TITLE,
    'An audit task has been allocated to the assigned team. Please review the engagement details and proceed in line with the approved plan.' SUMMARY,
    'WORKFLOW' NT,'N' ATT,'Y' ACTIVE,'Central package prerequisite; inserted only when missing.' DESCR FROM DUAL UNION ALL
  SELECT 'AUDIT_TEAM_JOINED','Audit Team Joined','Audit','DBConnection.AddJoiningReport','PKG_AR.P_ADDJOININGREPORT',
    'PKG_AR.P_ADDJOININGREPORT output cursor','IAS Notification: Audit Team has Joined for {REFERENCE}',
    'Audit Team has Joined','The audit team has officially joined. Please coordinate with the team and provide requested information.',
    'WORKFLOW','N','Y','Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'OBS_SUBMITTED_AUDITEE','Observation Submitted to Auditee','Audit Execution',
    'ApiCallsController.submit_observation_to_auditee','PKG_AR observation submission process','Existing managed-observation data source',
    'IAS Notification: Observation Submitted to Auditee - {REFERENCE}','Observation Submitted to Auditee',
    'An audit observation has been submitted to the auditee for review and response.','WORKFLOW','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'PARA_STATUS_UPDATED','Audit Para Status Updated','Compliance','DBConnection.SubmitPostAuditComplianceReview',
    'PKG_AE.P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW','V_IAS_POST_COMPLIANCE_NOTIFY',
    'IAS Notification: Para No. {REFERENCE} is marked {STATUS}','Audit Para Status Updated',
    'Para No. {REFERENCE} has been marked as {STATUS}.','WORKFLOW','N','Y',
    'Existing non-Management behavior; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'FINAL_REPORT_ISSUED','Final Audit Report Issued','Reporting','FieldAuditReportController finalization',
    'PKG_FRPT final-report process','Existing field-audit report overview data source',
    'IAS Notification: Final Audit Report Issued - {REFERENCE}','Final Audit Report Issued',
    'The final audit report has been issued. The generated PDF is attached by the existing application process.',
    'WORKFLOW','Y','Y','Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'INQUIRY_ASSIGNED_UNIT','Inquiry Assigned to I&I Unit','IID','ApiCallsController.AddHeadReview',
    'PKG_INQ.ADD_HEAD_REVIEW','Existing complaint/head-review/unit data sources',
    'IAS Notification: Inquiry Assigned to I&I Unit - {REFERENCE}','Inquiry Assigned to I&I Unit',
    'A complaint inquiry has been approved and allocated to the relevant I&I Unit.','WORKFLOW','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'PASSWORD_RESET_SUCCESS','Password Reset Successful','Authentication','DBConnection.ResetUserPassword',
    'PKG_AD.RESET_USER_PASSWORD','V_SERVICE_EMPLOYEEINFO + RESET_USER_PASSWORD output cursor',
    'IAS~ Password Reset Successful','Password Reset Successful',
    'Your password has been successfully reset. Change the temporary password immediately after logging in.',
    'SECURITY','N','Y','Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'AUDIT_SAMPLE_ISSUE','Audit Sample Creation Issue','Sampling',
    'DBConnection.CreateSampleDataAfterEngagementApproval','PKG_SM.P_ADD_SAMPLE_DATA','PKG_SM.P_ADD_SAMPLE_DATA output cursor',
    'IAS~Notification: Issue in Audit Sample for Engagement ID: {REFERENCE}','Issue in Audit Sample Creation',
    'An issue was identified while creating the audit sample. Please review and resolve it.','TECHNICAL','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'AUDIT_EXCEPTION_ISSUE','Audit Exception Creation Issue','Exception Monitoring',
    'DBConnection.CreateExceptionDataAfterEngagementApproval','PKG_SM.P_ADD_EXCEPTION_DATA','PKG_SM.P_ADD_EXCEPTION_DATA output cursor',
    'IAS~Notification: Issue in Audit Exception for Engagement ID: {REFERENCE}','Issue in Audit Exception Creation',
    'An issue was identified while creating the audit exception report. Please review and resolve it.','TECHNICAL','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'AUDIT_CRITERIA_SUBMITTED','Audit Criteria Submitted','Planning','DBConnection.SubmitAuditCriteriaForApproval',
    'PKG_PG.P_SUBMITAUDITCRITERIAFORAPPROVAL','Existing planning procedure/data source',
    'IAS~ Notification regarding submission of Audit Criteria','Audit Criteria Submitted',
    'Audit criteria have been submitted for review.','WORKFLOW','N','N',
    'Dormant central package prerequisite; inserted inactive only when missing.' FROM DUAL UNION ALL
  SELECT 'AJAX_APPLICATION_ERROR','AJAX Application Error','Application','AjaxErrorNotificationMiddleware',
    'Application middleware','Application error context + configured recipient',
    'IAS AJAX Error {STATUS} ({REFERENCE})','AJAX Application Error','An AJAX application error was detected.',
    'TECHNICAL','N','Y','Central package prerequisite; inserted only when missing.' FROM DUAL
) S
ON (D.NOTIFICATION_CODE=S.CODE)
WHEN NOT MATCHED THEN INSERT
  (NOTIFICATION_CODE,NOTIFICATION_NAME,MODULE_NAME,CALLING_PROCESS,RELATED_PROCEDURE,
   DATA_SOURCE_NAME,SUBJECT_TEMPLATE,TITLE_TEMPLATE,SUMMARY_TEMPLATE,
   NOTIFICATION_TYPE,REQUIRES_ATTACHMENT,IS_ACTIVE,DESCRIPTION)
VALUES
  (S.CODE,S.NAME,S.MODL,S.CALLER,S.PROC,S.SRC,S.SUBJ,S.TITLE,TO_CLOB(S.SUMMARY),
   S.NT,S.ATT,S.ACTIVE,S.DESCR);

COMMIT;

DECLARE
  V_COUNT NUMBER;
BEGIN
  SELECT COUNT(*) INTO V_COUNT
    FROM IAS_NOTIFICATION_MASTER
   WHERE NOTIFICATION_CODE IN
     ('MGMT_AUDIT_PARA_STATUS',
      'MGMT_AUDIT_WEEKLY_PARA_STATUS',
      'MGMT_AUDIT_MAPPING_EXCEPTION',
      'IAS_NOTIFICATION_HEALTH');

  IF V_COUNT<>4 THEN
    RAISE_APPLICATION_ERROR(-20821,'Management Audit notification configuration is incomplete.');
  END IF;
END;
/

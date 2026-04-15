/*
  IAS_ZTBL Phase 3 lookup seed script

  Purpose
  - seed normalized lookup families required by the approved Phase 2 baseline
  - provide initial status/state values used by DDL, packages, and migration scripts

  Notes
  - rerunnable where practical
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

DECLARE
    PROCEDURE ensure_lookup_type(
        p_lookup_type_code IN VARCHAR2,
        p_lookup_type_name IN VARCHAR2,
        p_description      IN VARCHAR2,
        p_sort_order       IN NUMBER DEFAULT 0
    ) IS
        l_count NUMBER;
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM tbl_lookup_type
         WHERE lookup_type_code = p_lookup_type_code;

        IF l_count = 0 THEN
            INSERT INTO tbl_lookup_type (
                lookup_type_id, lookup_type_code, lookup_type_name, description,
                is_system_type, sort_order, is_active, created_by
            )
            VALUES (
                NULL, p_lookup_type_code, p_lookup_type_name, p_description,
                'Y', p_sort_order, 'Y', 0
            );
        END IF;
    END ensure_lookup_type;

    PROCEDURE ensure_lookup_value(
        p_lookup_type_code IN VARCHAR2,
        p_lookup_code      IN VARCHAR2,
        p_lookup_name      IN VARCHAR2,
        p_sort_order       IN NUMBER,
        p_description      IN VARCHAR2 DEFAULT NULL,
        p_is_default       IN CHAR DEFAULT 'N'
    ) IS
        l_lookup_type_id tbl_lookup_type.lookup_type_id%TYPE;
        l_count          NUMBER;
    BEGIN
        SELECT lookup_type_id
          INTO l_lookup_type_id
          FROM tbl_lookup_type
         WHERE lookup_type_code = p_lookup_type_code;

        SELECT COUNT(*)
          INTO l_count
          FROM tbl_lookup_value
         WHERE lookup_type_id = l_lookup_type_id
           AND lookup_code = p_lookup_code;

        IF l_count = 0 THEN
            INSERT INTO tbl_lookup_value (
                lookup_value_id, lookup_type_id, lookup_code, lookup_name,
                description, sort_order, is_default, is_active, created_by
            )
            VALUES (
                NULL, l_lookup_type_id, p_lookup_code, p_lookup_name,
                p_description, p_sort_order, p_is_default, 'Y', 0
            );
        END IF;
    END ensure_lookup_value;
BEGIN
    ensure_lookup_type('SECURITY_STATUS', 'Security Status', 'Normalized status for users, roles, permissions, pages, and APIs.', 10);
    ensure_lookup_type('ROLE_TYPE', 'Role Type', 'Security role classification.', 20);
    ensure_lookup_type('PERMISSION_TYPE', 'Permission Type', 'Permission classification for page/api permissions.', 30);
    ensure_lookup_type('ENTITY_STATUS', 'Entity Status', 'Status of audit entities.', 40);
    ensure_lookup_type('RELATION_TYPE', 'Relation Type', 'Hierarchy and relationship links between entities.', 50);
    ensure_lookup_type('RISK_RATING', 'Risk Rating', 'Risk classification used in planning and observations.', 60);
    ensure_lookup_type('SIZE_BAND', 'Size Band', 'Entity or case size classification.', 70);
    ensure_lookup_type('PERIOD_STATUS', 'Period Status', 'Audit period lifecycle.', 80);
    ensure_lookup_type('ENGAGEMENT_STATUS', 'Engagement Status', 'Engagement lifecycle status.', 90);
    ensure_lookup_type('ENGAGEMENT_TYPE', 'Engagement Type', 'Engagement classification.', 100);
    ensure_lookup_type('TASK_STATUS', 'Task Status', 'Engagement task status.', 110);
    ensure_lookup_type('CHECKLIST_STATUS', 'Checklist Status', 'Checklist processing status.', 120);
    ensure_lookup_type('OBSERVATION_STATUS', 'Observation Status', 'Observation lifecycle status.', 130);
    ensure_lookup_type('OBSERVATION_DETAIL_TYPE', 'Observation Detail Type', 'Observation detail line classification.', 140);
    ensure_lookup_type('SEVERITY', 'Severity', 'Observation and issue severity.', 150);
    ensure_lookup_type('RESPONSE_STAGE', 'Response Stage', 'Observation response stage.', 160);
    ensure_lookup_type('COMPLIANCE_STATUS', 'Compliance Status', 'Post-compliance status.', 170);
    ensure_lookup_type('COMPLIANCE_STAGE', 'Compliance Stage', 'Post-compliance stage.', 180);
    ensure_lookup_type('PARA_STATUS', 'Para Status', 'Legacy and migrated para status.', 190);
    ensure_lookup_type('PARA_CASE_STATUS', 'Para Case Status', 'Para case lifecycle.', 200);
    ensure_lookup_type('SETTLEMENT_STATUS', 'Settlement Status', 'Para/compliance settlement status.', 210);
    ensure_lookup_type('SETTLEMENT_STAGE', 'Settlement Stage', 'Para/compliance settlement stage.', 220);
    ensure_lookup_type('ACTION_TYPE', 'Action Type', 'Workflow and action classifications.', 230);
    ensure_lookup_type('EVIDENCE_TYPE', 'Evidence Type', 'Evidence categories across modules.', 240);
    ensure_lookup_type('EVIDENCE_STATUS', 'Evidence Status', 'Evidence lifecycle.', 250);
    ensure_lookup_type('IID_CASE_STATUS', 'IID Case Status', 'IID complaint/inquiry lifecycle.', 260);
    ensure_lookup_type('IID_PRIORITY', 'IID Priority', 'IID risk or urgency scale.', 270);
    ensure_lookup_type('COMPLAINT_SOURCE', 'Complaint Source', 'IID complaint source channels.', 280);
    ensure_lookup_type('COMPLAINT_TYPE', 'Complaint Type', 'IID complaint classifications.', 290);
    ensure_lookup_type('SUBJECT_TYPE', 'Subject Type', 'IID subject classifications.', 300);
    ensure_lookup_type('STATEMENT_TYPE', 'Statement Type', 'IID statement classifications.', 310);
    ensure_lookup_type('FINDING_TYPE', 'Finding Type', 'IID finding classifications.', 320);
    ensure_lookup_type('REFERENCE_TYPE', 'Reference Type', 'Observation and compliance reference classification.', 330);
    ensure_lookup_type('REPORT_TYPE', 'Report Type', 'Report categories.', 340);
    ensure_lookup_type('REPORT_STATE', 'Report State', 'Normalized report lifecycle state.', 350);
    ensure_lookup_type('SNAPSHOT_TYPE', 'Snapshot Type', 'Report snapshot categories.', 360);
    ensure_lookup_type('COMMERCIAL_AUDIT_STATUS', 'Commercial Audit Status', 'Commercial audit lifecycle state.', 370);
    ensure_lookup_type('RESOLUTION_TYPE', 'Resolution Type', 'Commercial and compliance resolution types.', 380);
    ensure_lookup_type('ATTACHMENT_LINK_TYPE', 'Attachment Link Type', 'Attachment linkage categories.', 390);
    ensure_lookup_type('NOTIFICATION_QUEUE_STATUS', 'Notification Queue Status', 'Notification queue processing state.', 400);
    ensure_lookup_type('RECIPIENT_TYPE', 'Recipient Type', 'Notification recipient expression type.', 410);
    ensure_lookup_type('SESSION_STATUS', 'Session Status', 'User session status.', 420);
    ensure_lookup_type('DOCUMENT_TYPE', 'Document Type', 'Reference and working-paper document classification.', 430);
    ensure_lookup_type('PLAN_STATUS', 'Plan Status', 'Audit and IID plan approval status.', 440);

    ensure_lookup_value('SECURITY_STATUS', 'ACTIVE', 'Active', 10, 'Active security principal.', 'Y');
    ensure_lookup_value('SECURITY_STATUS', 'INACTIVE', 'Inactive', 20, 'Inactive security principal.');
    ensure_lookup_value('SECURITY_STATUS', 'LOCKED', 'Locked', 30, 'Security principal is locked.');

    ensure_lookup_value('ROLE_TYPE', 'SYSTEM', 'System', 10, 'System-defined role.', 'Y');
    ensure_lookup_value('ROLE_TYPE', 'BUSINESS', 'Business', 20, 'Business-managed role.');
    ensure_lookup_value('ROLE_TYPE', 'TEMPORARY', 'Temporary', 30, 'Temporary or transitional role.');

    ensure_lookup_value('PERMISSION_TYPE', 'PAGE', 'Page', 10, 'Page/UI access permission.', 'Y');
    ensure_lookup_value('PERMISSION_TYPE', 'API', 'API', 20, 'API/service access permission.');
    ensure_lookup_value('PERMISSION_TYPE', 'WORKFLOW', 'Workflow', 30, 'Workflow action permission.');

    ensure_lookup_value('ENTITY_STATUS', 'ACTIVE', 'Active', 10, 'Active auditable entity.', 'Y');
    ensure_lookup_value('ENTITY_STATUS', 'INACTIVE', 'Inactive', 20, 'Inactive entity.');
    ensure_lookup_value('ENTITY_STATUS', 'ARCHIVED', 'Archived', 30, 'Archived entity.');

    ensure_lookup_value('RELATION_TYPE', 'PARENT_CHILD', 'Parent Child', 10, 'Standard hierarchy relationship.', 'Y');
    ensure_lookup_value('RELATION_TYPE', 'COMPLIANCE_UNIT', 'Compliance Unit', 20, 'Compliance unit linkage.');
    ensure_lookup_value('RELATION_TYPE', 'REPORTING_UNIT', 'Reporting Unit', 30, 'Reporting structure linkage.');

    ensure_lookup_value('RISK_RATING', 'LOW', 'Low', 10, 'Low risk rating.');
    ensure_lookup_value('RISK_RATING', 'MEDIUM', 'Medium', 20, 'Medium risk rating.', 'Y');
    ensure_lookup_value('RISK_RATING', 'HIGH', 'High', 30, 'High risk rating.');
    ensure_lookup_value('RISK_RATING', 'CRITICAL', 'Critical', 40, 'Critical risk rating.');

    ensure_lookup_value('SIZE_BAND', 'SMALL', 'Small', 10, 'Small size band.');
    ensure_lookup_value('SIZE_BAND', 'MEDIUM', 'Medium', 20, 'Medium size band.', 'Y');
    ensure_lookup_value('SIZE_BAND', 'LARGE', 'Large', 30, 'Large size band.');

    ensure_lookup_value('PERIOD_STATUS', 'OPEN', 'Open', 10, 'Open audit period.', 'Y');
    ensure_lookup_value('PERIOD_STATUS', 'CLOSED', 'Closed', 20, 'Closed audit period.');
    ensure_lookup_value('PERIOD_STATUS', 'ARCHIVED', 'Archived', 30, 'Archived audit period.');

    ensure_lookup_value('ENGAGEMENT_STATUS', 'PLANNED', 'Planned', 10, 'Planned engagement.', 'Y');
    ensure_lookup_value('ENGAGEMENT_STATUS', 'IN_FIELD', 'In Field', 20, 'Field work is in progress.');
    ensure_lookup_value('ENGAGEMENT_STATUS', 'UNDER_REVIEW', 'Under Review', 30, 'Engagement is under review.');
    ensure_lookup_value('ENGAGEMENT_STATUS', 'FINALIZED', 'Finalized', 40, 'Engagement is finalized.');

    ensure_lookup_value('ENGAGEMENT_TYPE', 'FIELD_AUDIT', 'Field Audit', 10, 'Standard field audit engagement.', 'Y');
    ensure_lookup_value('ENGAGEMENT_TYPE', 'COMPLIANCE', 'Compliance', 20, 'Compliance engagement.');
    ensure_lookup_value('ENGAGEMENT_TYPE', 'IID', 'IID', 30, 'IID investigation or inquiry engagement.');
    ensure_lookup_value('ENGAGEMENT_TYPE', 'COMMERCIAL', 'Commercial Audit', 40, 'Commercial audit engagement.');

    ensure_lookup_value('TASK_STATUS', 'OPEN', 'Open', 10, 'Open task.', 'Y');
    ensure_lookup_value('TASK_STATUS', 'IN_PROGRESS', 'In Progress', 20, 'Task is being worked.');
    ensure_lookup_value('TASK_STATUS', 'COMPLETED', 'Completed', 30, 'Task completed.');
    ensure_lookup_value('TASK_STATUS', 'CANCELLED', 'Cancelled', 40, 'Task cancelled.');

    ensure_lookup_value('CHECKLIST_STATUS', 'NOT_STARTED', 'Not Started', 10, 'Checklist item not started.', 'Y');
    ensure_lookup_value('CHECKLIST_STATUS', 'IN_PROGRESS', 'In Progress', 20, 'Checklist item in progress.');
    ensure_lookup_value('CHECKLIST_STATUS', 'COMPLETED', 'Completed', 30, 'Checklist item completed.');

    ensure_lookup_value('OBSERVATION_STATUS', 'DRAFT', 'Draft', 10, 'Initial maker state.', 'Y');
    ensure_lookup_value('OBSERVATION_STATUS', 'SUBMITTED_TO_AUDITEE', 'Submitted to Auditee', 20, 'Submitted to auditee.');
    ensure_lookup_value('OBSERVATION_STATUS', 'AUDITEE_RESPONDED', 'Auditee Responded', 30, 'Auditee has responded.');
    ensure_lookup_value('OBSERVATION_STATUS', 'PRE_CONCLUDING', 'Pre-Concluding', 40, 'Pre-concluding workflow state.');
    ensure_lookup_value('OBSERVATION_STATUS', 'CONCLUDED', 'Concluded', 50, 'Observation concluded.');
    ensure_lookup_value('OBSERVATION_STATUS', 'SETTLED', 'Settled', 60, 'Observation settled.');
    ensure_lookup_value('OBSERVATION_STATUS', 'DROPPED', 'Dropped', 70, 'Observation dropped.');
    ensure_lookup_value('OBSERVATION_STATUS', 'REVERSED', 'Reversed', 80, 'Observation reversed.');

    ensure_lookup_value('OBSERVATION_DETAIL_TYPE', 'TEXT', 'Text', 10, 'Narrative observation text line.', 'Y');
    ensure_lookup_value('OBSERVATION_DETAIL_TYPE', 'HEADING', 'Heading', 20, 'Observation heading row.');
    ensure_lookup_value('OBSERVATION_DETAIL_TYPE', 'ANNEXURE', 'Annexure', 30, 'Observation annexure row.');

    ensure_lookup_value('SEVERITY', 'LOW', 'Low', 10, 'Low severity.');
    ensure_lookup_value('SEVERITY', 'MEDIUM', 'Medium', 20, 'Medium severity.', 'Y');
    ensure_lookup_value('SEVERITY', 'HIGH', 'High', 30, 'High severity.');
    ensure_lookup_value('SEVERITY', 'CRITICAL', 'Critical', 40, 'Critical severity.');

    ensure_lookup_value('RESPONSE_STAGE', 'AUDITEE', 'Auditee', 10, 'Auditee response stage.', 'Y');
    ensure_lookup_value('RESPONSE_STAGE', 'AUDITOR', 'Auditor', 20, 'Auditor response stage.');
    ensure_lookup_value('RESPONSE_STAGE', 'HEAD_AUDIT', 'Head Audit', 30, 'Head audit response stage.');
    ensure_lookup_value('RESPONSE_STAGE', 'FINAL', 'Final', 40, 'Final response stage.');

    ensure_lookup_value('COMPLIANCE_STAGE', 'OPEN', 'Open', 10, 'Compliance case opened.', 'Y');
    ensure_lookup_value('COMPLIANCE_STAGE', 'SUBMITTED', 'Submitted', 20, 'Submitted for review.');
    ensure_lookup_value('COMPLIANCE_STAGE', 'UNDER_REVIEW', 'Under Review', 30, 'Under review.');
    ensure_lookup_value('COMPLIANCE_STAGE', 'APPROVED', 'Approved', 40, 'Approved stage.');

    ensure_lookup_value('COMPLIANCE_STATUS', 'PENDING', 'Pending', 10, 'Pending compliance action.', 'Y');
    ensure_lookup_value('COMPLIANCE_STATUS', 'IN_PROGRESS', 'In Progress', 20, 'Compliance in progress.');
    ensure_lookup_value('COMPLIANCE_STATUS', 'PARTIALLY_COMPLIED', 'Partially Complied', 30, 'Partial compliance.');
    ensure_lookup_value('COMPLIANCE_STATUS', 'FULLY_COMPLIED', 'Fully Complied', 40, 'Fully complied.');
    ensure_lookup_value('COMPLIANCE_STATUS', 'SETTLED', 'Settled', 50, 'Settled after review.');

    ensure_lookup_value('PARA_STATUS', 'OPEN', 'Open', 10, 'Open para.', 'Y');
    ensure_lookup_value('PARA_STATUS', 'UNDER_COMPLIANCE', 'Under Compliance', 20, 'Under compliance review.');
    ensure_lookup_value('PARA_STATUS', 'SETTLED', 'Settled', 30, 'Para settled.');
    ensure_lookup_value('PARA_STATUS', 'ARCHIVED', 'Archived', 40, 'Archived para.');

    ensure_lookup_value('PARA_CASE_STATUS', 'ACTIVE', 'Active', 10, 'Active para case.', 'Y');
    ensure_lookup_value('PARA_CASE_STATUS', 'CLOSED', 'Closed', 20, 'Closed para case.');
    ensure_lookup_value('PARA_CASE_STATUS', 'ARCHIVED', 'Archived', 30, 'Archived para case.');

    ensure_lookup_value('SETTLEMENT_STATUS', 'PENDING', 'Pending', 10, 'Pending settlement.', 'Y');
    ensure_lookup_value('SETTLEMENT_STATUS', 'UNDER_REVIEW', 'Under Review', 20, 'Settlement under review.');
    ensure_lookup_value('SETTLEMENT_STATUS', 'APPROVED', 'Approved', 30, 'Settlement approved.');
    ensure_lookup_value('SETTLEMENT_STATUS', 'REJECTED', 'Rejected', 40, 'Settlement rejected.');

    ensure_lookup_value('SETTLEMENT_STAGE', 'BRANCH', 'Branch', 10, 'Branch settlement stage.', 'Y');
    ensure_lookup_value('SETTLEMENT_STAGE', 'CAU', 'CAU', 20, 'CAU settlement stage.');
    ensure_lookup_value('SETTLEMENT_STAGE', 'HEAD_OFFICE', 'Head Office', 30, 'Head office settlement stage.');

    ensure_lookup_value('ACTION_TYPE', 'CREATE', 'Create', 10, 'Create action.', 'Y');
    ensure_lookup_value('ACTION_TYPE', 'UPDATE', 'Update', 20, 'Update action.');
    ensure_lookup_value('ACTION_TYPE', 'SUBMIT', 'Submit', 30, 'Submit action.');
    ensure_lookup_value('ACTION_TYPE', 'APPROVE', 'Approve', 40, 'Approve action.');
    ensure_lookup_value('ACTION_TYPE', 'REJECT', 'Reject', 50, 'Reject action.');
    ensure_lookup_value('ACTION_TYPE', 'FINALIZE', 'Finalize', 60, 'Finalize action.');

    ensure_lookup_value('EVIDENCE_TYPE', 'DOCUMENT', 'Document', 10, 'Document evidence.', 'Y');
    ensure_lookup_value('EVIDENCE_TYPE', 'IMAGE', 'Image', 20, 'Image evidence.');
    ensure_lookup_value('EVIDENCE_TYPE', 'REPORT', 'Report', 30, 'Report evidence.');
    ensure_lookup_value('EVIDENCE_TYPE', 'DSA', 'DSA', 40, 'DSA evidence.');

    ensure_lookup_value('EVIDENCE_STATUS', 'RECEIVED', 'Received', 10, 'Evidence received.', 'Y');
    ensure_lookup_value('EVIDENCE_STATUS', 'UNDER_REVIEW', 'Under Review', 20, 'Evidence under review.');
    ensure_lookup_value('EVIDENCE_STATUS', 'ACCEPTED', 'Accepted', 30, 'Evidence accepted.');
    ensure_lookup_value('EVIDENCE_STATUS', 'REJECTED', 'Rejected', 40, 'Evidence rejected.');

    ensure_lookup_value('IID_CASE_STATUS', 'RECEIVED', 'Received', 10, 'Complaint received.', 'Y');
    ensure_lookup_value('IID_CASE_STATUS', 'UNDER_ANALYSIS', 'Under Analysis', 20, 'Case under analysis.');
    ensure_lookup_value('IID_CASE_STATUS', 'UNDER_ASSESSMENT', 'Under Assessment', 30, 'Case under assessment.');
    ensure_lookup_value('IID_CASE_STATUS', 'INVESTIGATION_PLANNED', 'Investigation Planned', 40, 'Investigation planned.');
    ensure_lookup_value('IID_CASE_STATUS', 'UNDER_HEAD_REVIEW', 'Under Head Review', 50, 'Under head review.');
    ensure_lookup_value('IID_CASE_STATUS', 'REPORT_DRAFTED', 'Report Drafted', 60, 'Report drafted.');
    ensure_lookup_value('IID_CASE_STATUS', 'FINALIZED', 'Finalized', 70, 'Case finalized.');

    ensure_lookup_value('IID_PRIORITY', 'LOW', 'Low', 10, 'Low IID priority.');
    ensure_lookup_value('IID_PRIORITY', 'MEDIUM', 'Medium', 20, 'Medium IID priority.', 'Y');
    ensure_lookup_value('IID_PRIORITY', 'HIGH', 'High', 30, 'High IID priority.');

    ensure_lookup_value('COMPLAINT_SOURCE', 'INTERNAL', 'Internal', 10, 'Internal complaint source.', 'Y');
    ensure_lookup_value('COMPLAINT_SOURCE', 'EXTERNAL', 'External', 20, 'External complaint source.');
    ensure_lookup_value('COMPLAINT_SOURCE', 'ANONYMOUS', 'Anonymous', 30, 'Anonymous complaint source.');

    ensure_lookup_value('COMPLAINT_TYPE', 'COMPLAINT', 'Complaint', 10, 'Standard complaint.', 'Y');
    ensure_lookup_value('COMPLAINT_TYPE', 'INQUIRY', 'Inquiry', 20, 'Inquiry.');
    ensure_lookup_value('COMPLAINT_TYPE', 'WHISTLEBLOWING', 'Whistleblowing', 30, 'Whistleblowing intake.');

    ensure_lookup_value('SUBJECT_TYPE', 'ACCUSED', 'Accused', 10, 'Accused subject.', 'Y');
    ensure_lookup_value('SUBJECT_TYPE', 'WITNESS', 'Witness', 20, 'Witness subject.');
    ensure_lookup_value('SUBJECT_TYPE', 'OTHER', 'Other', 30, 'Other subject.');

    ensure_lookup_value('STATEMENT_TYPE', 'WRITTEN', 'Written', 10, 'Written statement.', 'Y');
    ensure_lookup_value('STATEMENT_TYPE', 'ORAL', 'Oral', 20, 'Oral statement.');
    ensure_lookup_value('STATEMENT_TYPE', 'RECORDED', 'Recorded', 30, 'Recorded statement.');

    ensure_lookup_value('FINDING_TYPE', 'OBSERVATION', 'Observation', 10, 'Observation finding.', 'Y');
    ensure_lookup_value('FINDING_TYPE', 'VIOLATION', 'Violation', 20, 'Violation finding.');
    ensure_lookup_value('FINDING_TYPE', 'EXCEPTION', 'Exception', 30, 'Exception finding.');

    ensure_lookup_value('REFERENCE_TYPE', 'MANUAL', 'Manual', 10, 'Manual-based reference.', 'Y');
    ensure_lookup_value('REFERENCE_TYPE', 'CIRCULAR', 'Circular', 20, 'Circular-based reference.');
    ensure_lookup_value('REFERENCE_TYPE', 'LEGACY_REFERENCE', 'Legacy Reference', 30, 'Unstructured legacy reference carried forward.');

    ensure_lookup_value('REPORT_TYPE', 'AUDIT', 'Audit', 10, 'Audit report.', 'Y');
    ensure_lookup_value('REPORT_TYPE', 'IID', 'IID', 20, 'IID report.');
    ensure_lookup_value('REPORT_TYPE', 'FRPT', 'FRPT', 30, 'FRPT report.');
    ensure_lookup_value('REPORT_TYPE', 'COMMERCIAL', 'Commercial', 40, 'Commercial audit report.');

    ensure_lookup_value('REPORT_STATE', 'DRAFT', 'Draft', 10, 'Draft report.', 'Y');
    ensure_lookup_value('REPORT_STATE', 'IN_PREPARATION', 'In Preparation', 20, 'Report in preparation.');
    ensure_lookup_value('REPORT_STATE', 'FINALIZED', 'Finalized', 30, 'Report finalized.');
    ensure_lookup_value('REPORT_STATE', 'APPROVED', 'Approved', 40, 'Report approved.');
    ensure_lookup_value('REPORT_STATE', 'ISSUED', 'Issued', 50, 'Report issued.');

    ensure_lookup_value('SNAPSHOT_TYPE', 'KPI', 'KPI', 10, 'KPI snapshot type.', 'Y');
    ensure_lookup_value('SNAPSHOT_TYPE', 'NPL', 'NPL', 20, 'NPL snapshot type.');
    ensure_lookup_value('SNAPSHOT_TYPE', 'STAFF', 'Staff', 30, 'Staff snapshot type.');
    ensure_lookup_value('SNAPSHOT_TYPE', 'STATISTICS', 'Statistics', 40, 'Statistics snapshot type.');

    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'OM_DRAFT', 'OM Draft', 10, 'OM drafted.', 'Y');
    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'OM_ASSIGNED', 'OM Assigned', 20, 'OM assigned.');
    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'PDP_DRAFTED', 'PDP Drafted', 30, 'PDP drafted.');
    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'ARPSE_DRAFTED', 'ARPSE Drafted', 40, 'ARPSE drafted.');
    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'DAC_REVIEW', 'DAC Review', 50, 'DAC review stage.');
    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'PAC_REVIEW', 'PAC Review', 60, 'PAC review stage.');
    ensure_lookup_value('COMMERCIAL_AUDIT_STATUS', 'CLOSED', 'Closed', 70, 'Commercial item closed.');

    ensure_lookup_value('RESOLUTION_TYPE', 'DAC', 'DAC', 10, 'DAC resolution.', 'Y');
    ensure_lookup_value('RESOLUTION_TYPE', 'PAC', 'PAC', 20, 'PAC resolution.');
    ensure_lookup_value('RESOLUTION_TYPE', 'MANAGEMENT', 'Management', 30, 'Management resolution.');

    ensure_lookup_value('ATTACHMENT_LINK_TYPE', 'OBSERVATION', 'Observation', 10, 'Observation attachment.', 'Y');
    ensure_lookup_value('ATTACHMENT_LINK_TYPE', 'COMPLIANCE', 'Compliance', 20, 'Compliance attachment.');
    ensure_lookup_value('ATTACHMENT_LINK_TYPE', 'IID', 'IID', 30, 'IID attachment.');
    ensure_lookup_value('ATTACHMENT_LINK_TYPE', 'REPORT', 'Report', 40, 'Report attachment.');
    ensure_lookup_value('ATTACHMENT_LINK_TYPE', 'WORKING_PAPER', 'Working Paper', 50, 'Working paper attachment.');

    ensure_lookup_value('NOTIFICATION_QUEUE_STATUS', 'QUEUED', 'Queued', 10, 'Queued notification.', 'Y');
    ensure_lookup_value('NOTIFICATION_QUEUE_STATUS', 'SENT', 'Sent', 20, 'Sent notification.');
    ensure_lookup_value('NOTIFICATION_QUEUE_STATUS', 'FAILED', 'Failed', 30, 'Failed notification.');
    ensure_lookup_value('NOTIFICATION_QUEUE_STATUS', 'RETRY', 'Retry', 40, 'Queued for retry.');

    ensure_lookup_value('RECIPIENT_TYPE', 'ROLE', 'Role', 10, 'Role-based notification rule.', 'Y');
    ensure_lookup_value('RECIPIENT_TYPE', 'USER', 'User', 20, 'User-based notification rule.');
    ensure_lookup_value('RECIPIENT_TYPE', 'MIXED', 'Mixed', 30, 'Mixed role and user notification rule.');

    ensure_lookup_value('SESSION_STATUS', 'OPEN', 'Open', 10, 'Open session.', 'Y');
    ensure_lookup_value('SESSION_STATUS', 'EXPIRED', 'Expired', 20, 'Expired session.');
    ensure_lookup_value('SESSION_STATUS', 'CLOSED', 'Closed', 30, 'Closed session.');

    ensure_lookup_value('DOCUMENT_TYPE', 'MANUAL', 'Manual', 10, 'Manual document.', 'Y');
    ensure_lookup_value('DOCUMENT_TYPE', 'CIRCULAR', 'Circular', 20, 'Circular document.');
    ensure_lookup_value('DOCUMENT_TYPE', 'WORKING_PAPER', 'Working Paper', 30, 'Working paper document.');
    ensure_lookup_value('DOCUMENT_TYPE', 'EVIDENCE', 'Evidence', 40, 'Evidence document.');

    ensure_lookup_value('PLAN_STATUS', 'DRAFT', 'Draft', 10, 'Draft plan.', 'Y');
    ensure_lookup_value('PLAN_STATUS', 'SUBMITTED', 'Submitted', 20, 'Submitted plan.');
    ensure_lookup_value('PLAN_STATUS', 'APPROVED', 'Approved', 30, 'Approved plan.');
END;
/

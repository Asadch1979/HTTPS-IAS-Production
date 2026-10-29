/*
  IAS_ZTBL Phase 3 view script

  Purpose
  - create operational summary views needed for reporting, migration validation,
    and future API/query compatibility
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
CREATE OR REPLACE VIEW vw_para_case_register AS
SELECT
    p.para_case_id,
    p.observation_id,
    p.compliance_case_id,
    p.engagement_id,
    p.entity_id,
    e.entity_code,
    e.entity_name,
    p.para_no,
    p.legacy_reference_no,
    p.case_status_id,
    p.para_status_id,
    p.instance_count,
    p.settled_on,
    COUNT(DISTINCT pa.para_assignment_id) AS assignment_count,
    COUNT(DISTINCT ph.para_status_history_id) AS status_event_count
FROM tbl_para_case p
JOIN tbl_entity e
  ON e.entity_id = p.entity_id
LEFT JOIN tbl_para_assignment pa
  ON pa.para_case_id = p.para_case_id
 AND pa.is_active = 'Y'
LEFT JOIN tbl_para_status_history ph
  ON ph.para_case_id = p.para_case_id
 AND ph.is_active = 'Y'
WHERE p.is_active = 'Y'
GROUP BY
    p.para_case_id,
    p.observation_id,
    p.compliance_case_id,
    p.engagement_id,
    p.entity_id,
    e.entity_code,
    e.entity_name,
    p.para_no,
    p.legacy_reference_no,
    p.case_status_id,
    p.para_status_id,
    p.instance_count,
    p.settled_on;

CREATE OR REPLACE VIEW vw_compliance_case_summary AS
SELECT
    c.compliance_case_id,
    c.observation_id,
    o.observation_no,
    c.responsible_entity_id,
    e.entity_code,
    e.entity_name,
    c.compliance_cycle_no,
    c.compliance_status_id,
    c.compliance_stage_id,
    c.amount_involved,
    COUNT(DISTINCT ev.compliance_evidence_id) AS evidence_count,
    COUNT(DISTINCT ch.compliance_case_history_id) AS history_count
FROM tbl_compliance_case c
LEFT JOIN tbl_observation o
  ON o.observation_id = c.observation_id
LEFT JOIN tbl_entity e
  ON e.entity_id = c.responsible_entity_id
LEFT JOIN tbl_compliance_evidence ev
  ON ev.compliance_case_id = c.compliance_case_id
 AND ev.is_active = 'Y'
LEFT JOIN tbl_compliance_case_history ch
  ON ch.compliance_case_id = c.compliance_case_id
 AND ch.is_active = 'Y'
WHERE c.is_active = 'Y'
GROUP BY
    c.compliance_case_id,
    c.observation_id,
    o.observation_no,
    c.responsible_entity_id,
    e.entity_code,
    e.entity_name,
    c.compliance_cycle_no,
    c.compliance_status_id,
    c.compliance_stage_id,
    c.amount_involved;

CREATE OR REPLACE VIEW vw_document_migration_queue AS
SELECT
    q.document_migration_queue_id,
    q.migration_batch_id,
    q.source_table_name,
    q.source_pk_value,
    q.source_column_name,
    q.legacy_file_path,
    q.legacy_file_name,
    q.target_table_name,
    q.target_pk_value,
    q.attachment_link_type_code,
    q.queue_status_code,
    q.created_on
FROM tbl_document_migration_queue q
WHERE q.is_active = 'Y';
-- Reporting support views
-------------------------------------------------------------------------------

CREATE OR REPLACE VIEW vw_engagement_summary AS
SELECT
    e.engagement_id,
    e.engagement_no,
    e.audit_period_id,
    p.period_name,
    e.entity_id,
    ent.entity_code,
    ent.entity_name,
    e.engagement_status_id,
    e.audit_start_on,
    e.audit_end_on,
    COUNT(DISTINCT em.engagement_member_id) AS member_count,
    COUNT(DISTINCT ot.observation_id) AS observation_count
FROM tbl_engagement e
JOIN tbl_audit_period p
  ON p.audit_period_id = e.audit_period_id
JOIN tbl_entity ent
  ON ent.entity_id = e.entity_id
LEFT JOIN tbl_engagement_member em
  ON em.engagement_id = e.engagement_id
 AND em.is_active = 'Y'
LEFT JOIN tbl_observation ot
  ON ot.engagement_id = e.engagement_id
 AND ot.is_active = 'Y'
WHERE e.is_active = 'Y'
GROUP BY
    e.engagement_id,
    e.engagement_no,
    e.audit_period_id,
    p.period_name,
    e.entity_id,
    ent.entity_code,
    ent.entity_name,
    e.engagement_status_id,
    e.audit_start_on,
    e.audit_end_on;

CREATE OR REPLACE VIEW vw_observation_register AS
SELECT
    o.observation_id,
    o.observation_no,
    o.engagement_id,
    o.entity_id,
    e.entity_code,
    e.entity_name,
    o.observation_status_id,
    o.severity_id,
    o.amount_involved,
    o.draft_para_no,
    o.final_para_no,
    COUNT(DISTINCT c.compliance_case_id) AS compliance_case_count
FROM tbl_observation o
JOIN tbl_entity e
  ON e.entity_id = o.entity_id
LEFT JOIN tbl_compliance_case c
  ON c.observation_id = o.observation_id
 AND c.is_active = 'Y'
WHERE o.is_active = 'Y'
GROUP BY
    o.observation_id,
    o.observation_no,
    o.engagement_id,
    o.entity_id,
    e.entity_code,
    e.entity_name,
    o.observation_status_id,
    o.severity_id,
    o.amount_involved,
    o.draft_para_no,
    o.final_para_no;

CREATE OR REPLACE VIEW vw_iid_case_summary AS
SELECT
    c.iid_case_id,
    c.case_no,
    c.case_status_id,
    c.assigned_entity_id,
    c.submitted_on,
    COUNT(DISTINCT cp.iid_complainant_id) AS complainant_count,
    COUNT(DISTINCT sb.iid_subject_id) AS subject_count,
    COUNT(DISTINCT rp.iid_report_id) AS report_count
FROM tbl_iid_case c
LEFT JOIN tbl_iid_complainant cp
  ON cp.iid_case_id = c.iid_case_id
 AND cp.is_active = 'Y'
LEFT JOIN tbl_iid_subject sb
  ON sb.iid_case_id = c.iid_case_id
 AND sb.is_active = 'Y'
LEFT JOIN tbl_iid_report rp
  ON rp.iid_case_id = c.iid_case_id
 AND rp.is_active = 'Y'
WHERE c.is_active = 'Y'
GROUP BY
    c.iid_case_id,
    c.case_no,
    c.case_status_id,
    c.assigned_entity_id,
    c.submitted_on;

-------------------------------------------------------------------------------

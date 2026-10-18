-- CIA Nature of Issue Identified: authoritative modern observation checklist root.
-- This is a one-row-per-observation dimensional view; it does NOT change
-- V_EXEC_KPI_LIFECYCLE, issue eligibility, dates, or para lifecycle counts.
-- Use LEFT JOIN through checklist masters: missing linkage remains NULL/unmapped.
-- Confirm ID and S_ID/T_ID uniqueness in IAS Development before deployment.
CREATE OR REPLACE VIEW V_IAS_CIA_ISSUE_CHECKLIST_ROOT AS
SELECT o.id AS observation_id,
       o.checklistdetail_id,
       d.s_id AS checklist_sub_id,
       s.t_id AS checklist_root_id,
       c.heading AS checklist_heading
  FROM t_au_observation o
  LEFT JOIN t_audit_checklist_details d
    ON d.id = o.checklistdetail_id
  LEFT JOIN t_audit_checklist_sub s
    ON s.s_id = d.s_id
  LEFT JOIN t_audit_checklist c
    ON c.t_id = s.t_id;

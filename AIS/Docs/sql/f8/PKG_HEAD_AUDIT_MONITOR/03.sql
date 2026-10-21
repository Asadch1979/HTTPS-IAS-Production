-- F8: execute this file alone in PL/SQL Developer SQL Window.
CREATE OR REPLACE VIEW V_HEAD_AUDIT_ENG_SCOPE AS
SELECT DISTINCT d.dept_id,e.eng_id
 FROM v_head_audit_office_scope d JOIN t_au_plan_eng e ON e.auditby_id=d.office_id;

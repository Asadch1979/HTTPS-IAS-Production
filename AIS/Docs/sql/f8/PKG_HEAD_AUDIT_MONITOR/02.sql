-- F8: execute this file alone in PL/SQL Developer SQL Window.
-- Audit-performing entity hierarchy; never expand by the auditee's business parent.
CREATE OR REPLACE VIEW V_HEAD_AUDIT_OFFICE_SCOPE AS
SELECT DISTINCT CONNECT_BY_ROOT entity_id dept_id, entity_id office_id
 FROM v_exec_entity_scope
 CONNECT BY NOCYCLE PRIOR entity_id = parent_id;

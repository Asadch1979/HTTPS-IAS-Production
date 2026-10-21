-- F8: execute this file alone in PL/SQL Developer SQL Window.
-- Source only. Review/install manually; no application-startup DDL.
-- Department identity is the authenticated IAS entity, never a Team Lead PPNO.
-- Page allocation is checked in IAS before every reader; Oracle verifies active posting and entity membership.


-- Retained for legacy consumers; monitoring no longer uses this role catalogue.
CREATE OR REPLACE VIEW V_HEAD_AUDIT_ROLE_DEPT AS
SELECT g.role_id, d.entity_id dept_id, d.deptname
  FROM t_groups g CROSS JOIN t_audit_departments d
 WHERE g.status='Y' AND d.entity_id IS NOT NULL
   AND ((g.role_id=5 AND UPPER(TRIM(g.description))='HEAD FIELD AUDIT DEPARTMENT'
         AND d.entity_id=112243 AND UPPER(d.deptname) LIKE '%FIELD%AUDIT%')
     OR (g.role_id=6 AND UPPER(TRIM(g.description))='HEAD MANAGEMENT AUDIT DEPARTMENT'
         AND UPPER(TRIM(d.deptname))='MANAGEMENT AUDIT DEPARTMENT')
     OR (g.role_id=7 AND UPPER(TRIM(g.description))='HEAD INFORMATION SYSTEM AUDIT DEPARTMENT'
         AND UPPER(TRIM(d.deptname))='INFORMATION SYSTEM AUDIT DEPARTMENT'));

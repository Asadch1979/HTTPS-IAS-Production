-- READ-ONLY source candidates, not an automatic import or proof of completeness.
-- Several legacy procedures UPDATE authorization dates on older log rows. Reconcile
-- with immutable backups/audit trails before certifying a creation state or event order.
-- Source classification is UNKNOWN: actor role/current COM_STAGE is not durable proof.
SELECT 'AIS_T_PARA_STATUS_CHANGE_LOG' source_object, TO_CHAR(log_id) source_key,
       com_id, authorized_on event_on, old_status, new_status, 'UNKNOWN' settlement_source
  FROM AIS_T_PARA_STATUS_CHANGE_LOG
 WHERE action_status = 'AUTHORIZED' AND authorized_on IS NOT NULL;
SELECT 'T_AU_PARAS_STATUS_CHANGE_LOG' source_object, com_id, authorized_on event_on,
       old_status, new_status, ind, created_date, authorized_by
  FROM T_AU_PARAS_STATUS_CHANGE_LOG WHERE status = 'A' AND authorized_on IS NOT NULL;
SELECT 'AIS_T_AU_POST_COMPLIANCE_HISTORY' source_object, TO_CHAR(hist_id) source_key,
       com_id, comment_on event_on, com_status, com_stage, comment_by_ppno,
       'UNKNOWN' settlement_source
  FROM AIS_T_AU_POST_COMPLIANCE_HISTORY
 WHERE com_status = 16 AND com_stage IN (6,7,44,41);
-- These are candidates for PKG_AE's explicit settlement action, not all COM_STATUS=16
-- rows: distinguish PKG_FAD's parallel history write and de-duplicate by source evidence.
SELECT 'T_AU_NEW_PARAS_STATUS_CHANGE_LOG' source_object, l.id, c.com_id,
       l.authorized_on, l.old_status, l.new_status, l.status
  FROM T_AU_NEW_PARAS_STATUS_CHANGE_LOG l
  JOIN AIS_T_AU_POST_COMPLIANCE c ON c.ind = 'A' AND c.new_para_id = l.au_obs_id
 WHERE l.authorized_on IS NOT NULL;
SELECT 'T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG' source_object, l.id, c.com_id,
       l.authorized_on, l.old_status, l.new_status, l.status
  FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG l
  JOIN T_AU_OLD_PARAS_FAD f ON f.ref_p = l.ref_p
  JOIN AIS_T_AU_POST_COMPLIANCE c ON c.ind = 'O' AND c.old_para_id = f.id
 WHERE l.status = 'A' AND l.authorized_on IS NOT NULL;
SELECT 'T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG' source_object, l.id, c.com_id,
       l.authorized_on, l.old_status, l.new_status, l.status
  FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG l
  JOIN AIS_T_AU_POST_COMPLIANCE c ON c.ind = 'C' AND c.new_para_id = l.obs_id
 WHERE l.status = 'A' AND l.authorized_on IS NOT NULL;
-- Same identifier domain, different time semantics: directory is current assignment;
-- observation AUDITED_BY is copied when a case is created (PKG_HD/PKG_AR) and may differ.
SELECT c.com_id, c.entity_id, c.audited_by observation_auditor,
       e.auditby_id current_directory_auditor, c.entity_type_id, et.audit_type,
       c.para_added_on, c.ind, c.old_para_id, c.new_para_id
  FROM AIS_T_AU_POST_COMPLIANCE c
  LEFT JOIN T_AUDITEE_ENTITIES e ON e.entity_id = c.entity_id
  LEFT JOIN T_AUDITEE_ENT_TYPES et ON et.autid = c.entity_type_id
 WHERE DECODE(c.audited_by,e.auditby_id,0,1) = 1;

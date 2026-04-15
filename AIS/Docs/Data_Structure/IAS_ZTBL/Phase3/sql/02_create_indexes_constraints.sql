/*
  IAS_ZTBL Phase 3 constraint and index script

  Purpose
  - close major Phase 2 target-column gaps
  - add post-create foreign keys and performance indexes

  Notes
  - intended as a one-time execution after 01_create_tables.sql
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Gap-closing columns on existing tables
-------------------------------------------------------------------------------

ALTER TABLE tbl_attachment ADD (
    document_type_id       NUMBER,
    legacy_file_path       VARCHAR2(2000),
    legacy_source_table    VARCHAR2(128),
    legacy_source_pk_value VARCHAR2(200)
);

ALTER TABLE tbl_application_page ADD (
    description             VARCHAR2(1000),
    menu_image_path         VARCHAR2(500),
    page_url                VARCHAR2(500),
    parent_application_page_id NUMBER,
    hide_menu_flag          CHAR(1) DEFAULT 'N' NOT NULL,
    page_status_id          NUMBER
);

ALTER TABLE tbl_entity ADD (
    legacy_numeric_code     NUMBER,
    compliance_by_text      VARCHAR2(200),
    is_auditor_flag         CHAR(1) DEFAULT 'N' NOT NULL,
    is_iad_flag             CHAR(1) DEFAULT 'N' NOT NULL
);

ALTER TABLE tbl_user ADD (
    designation_id          NUMBER,
    branch_entity_id        NUMBER,
    department_entity_id    NUMBER,
    audit_zone_entity_id    NUMBER
);

ALTER TABLE tbl_user_session ADD (
    entity_id               NUMBER,
    role_id                 NUMBER,
    session_device_name     VARCHAR2(200),
    session_location        VARCHAR2(500)
);

ALTER TABLE tbl_engagement ADD (
    audit_plan_id           NUMBER,
    branch_code             VARCHAR2(50)
);

ALTER TABLE tbl_workflow_event ADD (
    event_name              VARCHAR2(200),
    status_id               NUMBER,
    entity_id               NUMBER,
    user_id                 NUMBER,
    role_id                 NUMBER,
    detail_text             CLOB
);

ALTER TABLE tbl_reference_document_version ADD (
    parent_reference_document_version_id NUMBER,
    chapter_no              VARCHAR2(100),
    chapter_title           VARCHAR2(500),
    section_code            VARCHAR2(100),
    sub_section_no          VARCHAR2(100),
    heading_text            VARCHAR2(500),
    page_no                 NUMBER,
    display_order           NUMBER
);

ALTER TABLE tbl_observation_assignment ADD (
    entity_id               NUMBER,
    observation_detail_id   NUMBER,
    replied_flag            CHAR(1) DEFAULT 'N' NOT NULL,
    submitted_flag          CHAR(1) DEFAULT 'N' NOT NULL,
    response_status_id      NUMBER,
    response_text           CLOB,
    response_no             NUMBER,
    response_by             NUMBER,
    response_on             DATE,
    account_number          VARCHAR2(100),
    account_amount          NUMBER,
    loan_case_no            VARCHAR2(100),
    loan_amount             NUMBER,
    reference_para_no       VARCHAR2(100),
    authorized_by           NUMBER,
    authorized_on           DATE,
    branch_code             VARCHAR2(50),
    action_code             VARCHAR2(50),
    reasons_text            VARCHAR2(2000),
    detail_text             CLOB
);

ALTER TABLE tbl_observation_response ADD (
    observation_detail_id   NUMBER,
    response_role_id        NUMBER,
    recommendation_text     CLOB,
    audit_reply_text        CLOB,
    key_remarks             VARCHAR2(2000),
    detail_text             CLOB
);

ALTER TABLE tbl_observation_reference ADD (
    heading_text            VARCHAR2(500),
    detail_text             CLOB,
    amount_involved         NUMBER,
    risk_rating_id          NUMBER,
    instance_count          NUMBER,
    audit_period_label      VARCHAR2(200),
    authorized_by           NUMBER,
    authorized_on           DATE,
    indicator_code          VARCHAR2(30),
    legacy_para_no          VARCHAR2(100),
    reference_status_id     NUMBER
);

ALTER TABLE tbl_compliance_case ADD (
    amount_involved         NUMBER,
    gist_text               CLOB,
    instance_count          NUMBER,
    legacy_para_no          VARCHAR2(100),
    audit_period_label      VARCHAR2(200),
    audited_by_entity_id    NUMBER,
    branch_response_by      NUMBER,
    branch_response_on      DATE,
    cau_assigned_by         NUMBER,
    cau_assigned_on         DATE,
    cau_status_id           NUMBER,
    indicator_code          VARCHAR2(30),
    para_added_on           DATE,
    reference_reviewed_flag CHAR(1) DEFAULT 'N' NOT NULL
);

ALTER TABLE tbl_iid_subject ADD (
    cnic_no                 VARCHAR2(30),
    designation_title       VARCHAR2(200),
    father_name             VARCHAR2(200),
    remarks                 VARCHAR2(1000),
    role_type_code          VARCHAR2(100),
    sort_order              NUMBER DEFAULT 0 NOT NULL,
    subject_status_id       NUMBER,
    accusation_text         CLOB,
    linked_user_id          NUMBER
);

ALTER TABLE tbl_iid_statement ADD (
    cnic_no                 VARCHAR2(30),
    role_type_code          VARCHAR2(100),
    statement_status_id     NUMBER,
    statement_datetime      DATE,
    statement_mode_code     VARCHAR2(100),
    statement_place         VARCHAR2(500),
    key_points              CLOB,
    uploaded_statement_text CLOB,
    linked_user_id          NUMBER
);

ALTER TABLE tbl_iid_investigation_plan ADD (
    heading_text            VARCHAR2(500),
    activities_text         CLOB,
    team_lead_text          VARCHAR2(500),
    team_members_text       CLOB,
    travelling_days         NUMBER,
    submitted_by            NUMBER,
    submitted_on            DATE
);

ALTER TABLE tbl_iid_report ADD (
    complainant_name        VARCHAR2(200),
    accused_name            VARCHAR2(200),
    gist_text               CLOB,
    submitted_by            NUMBER,
    submitted_on            DATE,
    uploaded_dsa_path       VARCHAR2(1000),
    uploaded_evidence_path  VARCHAR2(1000),
    uploaded_report_path    VARCHAR2(1000)
);

ALTER TABLE tbl_report_section ADD (
    entity_id               NUMBER,
    description             VARCHAR2(2000),
    detail_text             CLOB,
    line_no                 NUMBER,
    report_version_no       NUMBER,
    auditor_comments        CLOB,
    management_comments     CLOB,
    svp_remarks             CLOB,
    implications_text       CLOB,
    recommendations_text    CLOB,
    finalized_by            NUMBER,
    finalized_on            DATE,
    is_finalized_flag       CHAR(1) DEFAULT 'N' NOT NULL,
    lock_version            NUMBER,
    update_required_flag    CHAR(1) DEFAULT 'N' NOT NULL
);

ALTER TABLE tbl_report_snapshot ADD (
    entity_id               NUMBER,
    snapshot_name           VARCHAR2(200),
    report_version_no       NUMBER,
    period_end              DATE,
    actual_value            NUMBER,
    target_value            NUMBER,
    unit_code               VARCHAR2(50),
    cases_count             NUMBER,
    outstanding_amount      NUMBER,
    provision_amount        NUMBER,
    risk_level_code         VARCHAR2(50),
    reported_count          NUMBER,
    rectified_count         NUMBER,
    outstanding_count       NUMBER,
    remarks                 VARCHAR2(2000),
    is_source_active        CHAR(1) DEFAULT 'Y' NOT NULL
);

ALTER TABLE tbl_commercial_arpse_resolution ADD (
    meeting_no              NUMBER,
    para_no                 VARCHAR2(100),
    progress_report_frequency VARCHAR2(100),
    meeting_year            NUMBER,
    directive_date          DATE,
    directive_source_code   VARCHAR2(50)
);

-------------------------------------------------------------------------------
-- Post-create check constraints
-------------------------------------------------------------------------------

ALTER TABLE tbl_application_page ADD CONSTRAINT ck_tbl_app_page_hide CHECK (hide_menu_flag IN ('Y', 'N'));
ALTER TABLE tbl_entity ADD CONSTRAINT ck_tbl_entity_is_auditor CHECK (is_auditor_flag IN ('Y', 'N'));
ALTER TABLE tbl_entity ADD CONSTRAINT ck_tbl_entity_is_iad CHECK (is_iad_flag IN ('Y', 'N'));
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT ck_tbl_obs_assign_reply CHECK (replied_flag IN ('Y', 'N'));
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT ck_tbl_obs_assign_submit CHECK (submitted_flag IN ('Y', 'N'));
ALTER TABLE tbl_compliance_case ADD CONSTRAINT ck_tbl_compliance_ref_review CHECK (reference_reviewed_flag IN ('Y', 'N'));
ALTER TABLE tbl_report_section ADD CONSTRAINT ck_tbl_report_section_fin CHECK (is_finalized_flag IN ('Y', 'N'));
ALTER TABLE tbl_report_section ADD CONSTRAINT ck_tbl_report_section_upd CHECK (update_required_flag IN ('Y', 'N'));
ALTER TABLE tbl_report_snapshot ADD CONSTRAINT ck_tbl_report_snapshot_src CHECK (is_source_active IN ('Y', 'N'));

-------------------------------------------------------------------------------
-- Foreign keys for new and gap-closed columns
-------------------------------------------------------------------------------

ALTER TABLE tbl_attachment ADD CONSTRAINT fk_tbl_attachment_doc_type FOREIGN KEY (document_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_application_page ADD CONSTRAINT fk_tbl_app_page_parent FOREIGN KEY (parent_application_page_id) REFERENCES tbl_application_page (application_page_id);
ALTER TABLE tbl_application_page ADD CONSTRAINT fk_tbl_app_page_status FOREIGN KEY (page_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_entity_risk_profile ADD CONSTRAINT fk_tbl_ent_risk_profile_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_entity_risk_profile ADD CONSTRAINT fk_tbl_ent_risk_profile_val FOREIGN KEY (risk_rating_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_entity_size_profile ADD CONSTRAINT fk_tbl_ent_size_profile_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_entity_size_profile ADD CONSTRAINT fk_tbl_ent_size_profile_val FOREIGN KEY (size_band_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_checklist_sub_item ADD CONSTRAINT fk_tbl_checklist_sub_item FOREIGN KEY (checklist_item_id) REFERENCES tbl_checklist_item (checklist_item_id);
ALTER TABLE tbl_checklist_annexure ADD CONSTRAINT fk_tbl_check_annex_item FOREIGN KEY (checklist_item_id) REFERENCES tbl_checklist_item (checklist_item_id);
ALTER TABLE tbl_checklist_annexure ADD CONSTRAINT fk_tbl_check_annex_type FOREIGN KEY (annexure_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_checklist_annexure ADD CONSTRAINT fk_tbl_check_annex_ref FOREIGN KEY (reference_document_version_id) REFERENCES tbl_reference_document_version (reference_document_version_id);
ALTER TABLE tbl_checklist_annexure ADD CONSTRAINT fk_tbl_check_annex_att FOREIGN KEY (attachment_id) REFERENCES tbl_attachment (attachment_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_period FOREIGN KEY (audit_period_id) REFERENCES tbl_audit_period (audit_period_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_entity FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_entity_type FOREIGN KEY (entity_type_id) REFERENCES tbl_entity_type (entity_type_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_status FOREIGN KEY (plan_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_criteria FOREIGN KEY (plan_criteria_id) REFERENCES tbl_plan_criteria (plan_criteria_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_risk FOREIGN KEY (risk_rating_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_size FOREIGN KEY (size_band_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_audit_plan ADD CONSTRAINT fk_tbl_audit_plan_audited FOREIGN KEY (audited_by_entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_engagement ADD CONSTRAINT fk_tbl_engagement_plan FOREIGN KEY (audit_plan_id) REFERENCES tbl_audit_plan (audit_plan_id);
ALTER TABLE tbl_engagement_team ADD CONSTRAINT fk_tbl_eng_team_eng FOREIGN KEY (engagement_id) REFERENCES tbl_engagement (engagement_id);
ALTER TABLE tbl_engagement_team ADD CONSTRAINT fk_tbl_eng_team_type FOREIGN KEY (team_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_engagement_team ADD CONSTRAINT fk_tbl_eng_team_lead FOREIGN KEY (lead_user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_engagement_criteria_history ADD CONSTRAINT fk_tbl_eng_criteria_hist_eng FOREIGN KEY (engagement_id) REFERENCES tbl_engagement (engagement_id);
ALTER TABLE tbl_engagement_criteria_history ADD CONSTRAINT fk_tbl_eng_criteria_hist_rule FOREIGN KEY (plan_criteria_id) REFERENCES tbl_plan_criteria (plan_criteria_id);
ALTER TABLE tbl_engagement_criteria_history ADD CONSTRAINT fk_tbl_eng_criteria_hist_sta FOREIGN KEY (status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_engagement_criteria_history ADD CONSTRAINT fk_tbl_eng_criteria_hist_evt FOREIGN KEY (workflow_event_id) REFERENCES tbl_workflow_event (workflow_event_id);
ALTER TABLE tbl_user ADD CONSTRAINT fk_tbl_user_branch_ent FOREIGN KEY (branch_entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_user ADD CONSTRAINT fk_tbl_user_dept_ent FOREIGN KEY (department_entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_user ADD CONSTRAINT fk_tbl_user_audit_zone FOREIGN KEY (audit_zone_entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_user_session ADD CONSTRAINT fk_tbl_user_session_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_user_session ADD CONSTRAINT fk_tbl_user_session_role FOREIGN KEY (role_id) REFERENCES tbl_role (role_id);
ALTER TABLE tbl_workflow_event ADD CONSTRAINT fk_tbl_workflow_evt_status FOREIGN KEY (status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_workflow_event ADD CONSTRAINT fk_tbl_workflow_evt_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_workflow_event ADD CONSTRAINT fk_tbl_workflow_evt_user FOREIGN KEY (user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_workflow_event ADD CONSTRAINT fk_tbl_workflow_evt_role FOREIGN KEY (role_id) REFERENCES tbl_role (role_id);
ALTER TABLE tbl_reference_document_version ADD CONSTRAINT fk_tbl_ref_doc_ver_parent FOREIGN KEY (parent_reference_document_version_id) REFERENCES tbl_reference_document_version (reference_document_version_id);
ALTER TABLE tbl_observation_evidence ADD CONSTRAINT fk_tbl_obs_evd_obs FOREIGN KEY (observation_id) REFERENCES tbl_observation (observation_id);
ALTER TABLE tbl_observation_evidence ADD CONSTRAINT fk_tbl_obs_evd_det FOREIGN KEY (observation_detail_id) REFERENCES tbl_observation_detail (observation_detail_id);
ALTER TABLE tbl_observation_evidence ADD CONSTRAINT fk_tbl_obs_evd_att FOREIGN KEY (attachment_id) REFERENCES tbl_attachment (attachment_id);
ALTER TABLE tbl_observation_evidence ADD CONSTRAINT fk_tbl_obs_evd_type FOREIGN KEY (evidence_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_observation_evidence ADD CONSTRAINT fk_tbl_obs_evd_sta FOREIGN KEY (evidence_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_observation_dsa ADD CONSTRAINT fk_tbl_obs_dsa_obs FOREIGN KEY (observation_id) REFERENCES tbl_observation (observation_id);
ALTER TABLE tbl_observation_dsa ADD CONSTRAINT fk_tbl_obs_dsa_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_observation_dsa ADD CONSTRAINT fk_tbl_obs_dsa_usr FOREIGN KEY (user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_observation_dsa ADD CONSTRAINT fk_tbl_obs_dsa_sta FOREIGN KEY (dsa_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_observation_dsa ADD CONSTRAINT fk_tbl_obs_dsa_att FOREIGN KEY (uploaded_attachment_id) REFERENCES tbl_attachment (attachment_id);
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT fk_tbl_obs_assign_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT fk_tbl_obs_assign_det FOREIGN KEY (observation_detail_id) REFERENCES tbl_observation_detail (observation_detail_id);
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT fk_tbl_obs_assign_rsp_sta FOREIGN KEY (response_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT fk_tbl_obs_assign_rsp_usr FOREIGN KEY (response_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_observation_assignment ADD CONSTRAINT fk_tbl_obs_assign_auth FOREIGN KEY (authorized_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_observation_response ADD CONSTRAINT fk_tbl_obs_response_det FOREIGN KEY (observation_detail_id) REFERENCES tbl_observation_detail (observation_detail_id);
ALTER TABLE tbl_observation_response ADD CONSTRAINT fk_tbl_obs_response_role FOREIGN KEY (response_role_id) REFERENCES tbl_role (role_id);
ALTER TABLE tbl_observation_reference ADD CONSTRAINT fk_tbl_obs_ref_risk FOREIGN KEY (risk_rating_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_observation_reference ADD CONSTRAINT fk_tbl_obs_ref_auth FOREIGN KEY (authorized_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_observation_reference ADD CONSTRAINT fk_tbl_obs_ref_sta FOREIGN KEY (reference_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_case ADD CONSTRAINT fk_tbl_comp_case_aud_ent FOREIGN KEY (audited_by_entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_compliance_case ADD CONSTRAINT fk_tbl_comp_case_br_rsp FOREIGN KEY (branch_response_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_compliance_case ADD CONSTRAINT fk_tbl_comp_case_cau_usr FOREIGN KEY (cau_assigned_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_compliance_case ADD CONSTRAINT fk_tbl_comp_case_cau_sta FOREIGN KEY (cau_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_evidence ADD CONSTRAINT fk_tbl_comp_evd_case FOREIGN KEY (compliance_case_id) REFERENCES tbl_compliance_case (compliance_case_id);
ALTER TABLE tbl_compliance_evidence ADD CONSTRAINT fk_tbl_comp_evd_att FOREIGN KEY (attachment_id) REFERENCES tbl_attachment (attachment_id);
ALTER TABLE tbl_compliance_evidence ADD CONSTRAINT fk_tbl_comp_evd_type FOREIGN KEY (evidence_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_evidence ADD CONSTRAINT fk_tbl_comp_evd_sta FOREIGN KEY (evidence_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_case_history ADD CONSTRAINT fk_tbl_comp_case_hist_case FOREIGN KEY (compliance_case_id) REFERENCES tbl_compliance_case (compliance_case_id);
ALTER TABLE tbl_compliance_case_history ADD CONSTRAINT fk_tbl_comp_case_hist_from FOREIGN KEY (from_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_case_history ADD CONSTRAINT fk_tbl_comp_case_hist_to FOREIGN KEY (to_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_case_history ADD CONSTRAINT fk_tbl_comp_case_hist_stage FOREIGN KEY (compliance_stage_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_compliance_case_history ADD CONSTRAINT fk_tbl_comp_case_hist_evt FOREIGN KEY (workflow_event_id) REFERENCES tbl_workflow_event (workflow_event_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_obs FOREIGN KEY (observation_id) REFERENCES tbl_observation (observation_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_comp FOREIGN KEY (compliance_case_id) REFERENCES tbl_compliance_case (compliance_case_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_eng FOREIGN KEY (engagement_id) REFERENCES tbl_engagement (engagement_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_ent_type FOREIGN KEY (entity_type_id) REFERENCES tbl_entity_type (entity_type_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_risk FOREIGN KEY (risk_rating_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_case_sta FOREIGN KEY (case_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_case ADD CONSTRAINT fk_tbl_para_case_para_sta FOREIGN KEY (para_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_case_text ADD CONSTRAINT fk_tbl_para_case_text_case FOREIGN KEY (para_case_id) REFERENCES tbl_para_case (para_case_id);
ALTER TABLE tbl_para_case_text ADD CONSTRAINT fk_tbl_para_case_text_typ FOREIGN KEY (text_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_assignment ADD CONSTRAINT fk_tbl_para_assign_case FOREIGN KEY (para_case_id) REFERENCES tbl_para_case (para_case_id);
ALTER TABLE tbl_para_assignment ADD CONSTRAINT fk_tbl_para_assign_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_para_assignment ADD CONSTRAINT fk_tbl_para_assign_usr FOREIGN KEY (assignee_user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_para_assignment ADD CONSTRAINT fk_tbl_para_assign_role FOREIGN KEY (assignment_role_id) REFERENCES tbl_role (role_id);
ALTER TABLE tbl_para_assignment ADD CONSTRAINT fk_tbl_para_assign_sta FOREIGN KEY (assignment_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_status_history ADD CONSTRAINT fk_tbl_para_sta_hist_case FOREIGN KEY (para_case_id) REFERENCES tbl_para_case (para_case_id);
ALTER TABLE tbl_para_status_history ADD CONSTRAINT fk_tbl_para_sta_hist_from FOREIGN KEY (from_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_status_history ADD CONSTRAINT fk_tbl_para_sta_hist_to FOREIGN KEY (to_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_status_history ADD CONSTRAINT fk_tbl_para_sta_hist_stage FOREIGN KEY (status_stage_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_status_history ADD CONSTRAINT fk_tbl_para_sta_hist_evt FOREIGN KEY (workflow_event_id) REFERENCES tbl_workflow_event (workflow_event_id);
ALTER TABLE tbl_para_settlement_history ADD CONSTRAINT fk_tbl_para_settle_case FOREIGN KEY (para_case_id) REFERENCES tbl_para_case (para_case_id);
ALTER TABLE tbl_para_settlement_history ADD CONSTRAINT fk_tbl_para_settle_comp FOREIGN KEY (compliance_case_id) REFERENCES tbl_compliance_case (compliance_case_id);
ALTER TABLE tbl_para_settlement_history ADD CONSTRAINT fk_tbl_para_settle_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_para_settlement_history ADD CONSTRAINT fk_tbl_para_settle_sta FOREIGN KEY (settlement_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_settlement_history ADD CONSTRAINT fk_tbl_para_settle_stg FOREIGN KEY (settlement_stage_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_para_settlement_history ADD CONSTRAINT fk_tbl_para_settle_evt FOREIGN KEY (workflow_event_id) REFERENCES tbl_workflow_event (workflow_event_id);
ALTER TABLE tbl_iid_record ADD CONSTRAINT fk_tbl_iid_record_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_record ADD CONSTRAINT fk_tbl_iid_record_type FOREIGN KEY (record_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_record ADD CONSTRAINT fk_tbl_iid_record_att FOREIGN KEY (attachment_id) REFERENCES tbl_attachment (attachment_id);
ALTER TABLE tbl_iid_record ADD CONSTRAINT fk_tbl_iid_record_sta FOREIGN KEY (status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_analysis ADD CONSTRAINT fk_tbl_iid_analysis_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_analysis ADD CONSTRAINT fk_tbl_iid_analysis_typ FOREIGN KEY (analysis_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_analysis ADD CONSTRAINT fk_tbl_iid_analysis_sta FOREIGN KEY (status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_assessment ADD CONSTRAINT fk_tbl_iid_assess_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_assessment ADD CONSTRAINT fk_tbl_iid_assess_sub FOREIGN KEY (iid_subject_id) REFERENCES tbl_iid_subject (iid_subject_id);
ALTER TABLE tbl_iid_assessment ADD CONSTRAINT fk_tbl_iid_assess_typ FOREIGN KEY (assessment_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_assessment ADD CONSTRAINT fk_tbl_iid_assess_sta FOREIGN KEY (assessment_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_plan_approval ADD CONSTRAINT fk_tbl_iid_plan_appr_plan FOREIGN KEY (iid_investigation_plan_id) REFERENCES tbl_iid_investigation_plan (iid_investigation_plan_id);
ALTER TABLE tbl_iid_plan_approval ADD CONSTRAINT fk_tbl_iid_plan_appr_lvl FOREIGN KEY (approval_level_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_plan_approval ADD CONSTRAINT fk_tbl_iid_plan_appr_sta FOREIGN KEY (approval_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_head_review ADD CONSTRAINT fk_tbl_iid_head_review_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_head_review ADD CONSTRAINT fk_tbl_iid_head_review_sta FOREIGN KEY (review_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_case_study ADD CONSTRAINT fk_tbl_iid_case_study_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_case_study ADD CONSTRAINT fk_tbl_iid_case_study_sta FOREIGN KEY (case_study_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_violation ADD CONSTRAINT fk_tbl_iid_violation_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_violation ADD CONSTRAINT fk_tbl_iid_violation_sub FOREIGN KEY (iid_subject_id) REFERENCES tbl_iid_subject (iid_subject_id);
ALTER TABLE tbl_iid_violation ADD CONSTRAINT fk_tbl_iid_violation_typ FOREIGN KEY (violation_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_violation ADD CONSTRAINT fk_tbl_iid_violation_sev FOREIGN KEY (severity_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_violation ADD CONSTRAINT fk_tbl_iid_violation_sta FOREIGN KEY (violation_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_proceeding ADD CONSTRAINT fk_tbl_iid_proceeding_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_proceeding ADD CONSTRAINT fk_tbl_iid_proceeding_typ FOREIGN KEY (proceeding_type_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_proceeding ADD CONSTRAINT fk_tbl_iid_proceeding_sta FOREIGN KEY (proceeding_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_workflow_history ADD CONSTRAINT fk_tbl_iid_workflow_case FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_iid_workflow_history ADD CONSTRAINT fk_tbl_iid_workflow_from FOREIGN KEY (from_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_workflow_history ADD CONSTRAINT fk_tbl_iid_workflow_to FOREIGN KEY (to_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_workflow_history ADD CONSTRAINT fk_tbl_iid_workflow_evt FOREIGN KEY (workflow_event_id) REFERENCES tbl_workflow_event (workflow_event_id);
ALTER TABLE tbl_iid_subject ADD CONSTRAINT fk_tbl_iid_subject_sta FOREIGN KEY (subject_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_subject ADD CONSTRAINT fk_tbl_iid_subject_usr FOREIGN KEY (linked_user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_iid_statement ADD CONSTRAINT fk_tbl_iid_statement_sta FOREIGN KEY (statement_status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_iid_statement ADD CONSTRAINT fk_tbl_iid_statement_usr FOREIGN KEY (linked_user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_iid_investigation_plan ADD CONSTRAINT fk_tbl_iid_inv_plan_sub FOREIGN KEY (submitted_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_iid_report ADD CONSTRAINT fk_tbl_iid_report_sub FOREIGN KEY (submitted_by) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_report_section ADD CONSTRAINT fk_tbl_report_section_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_report_snapshot ADD CONSTRAINT fk_tbl_report_snapshot_ent FOREIGN KEY (entity_id) REFERENCES tbl_entity (entity_id);
ALTER TABLE tbl_working_paper ADD CONSTRAINT fk_tbl_working_paper_eng FOREIGN KEY (engagement_id) REFERENCES tbl_engagement (engagement_id);
ALTER TABLE tbl_working_paper ADD CONSTRAINT fk_tbl_working_paper_obs FOREIGN KEY (observation_id) REFERENCES tbl_observation (observation_id);
ALTER TABLE tbl_working_paper ADD CONSTRAINT fk_tbl_working_paper_iid FOREIGN KEY (iid_case_id) REFERENCES tbl_iid_case (iid_case_id);
ALTER TABLE tbl_working_paper ADD CONSTRAINT fk_tbl_working_paper_usr FOREIGN KEY (user_id) REFERENCES tbl_user (user_id);
ALTER TABLE tbl_working_paper ADD CONSTRAINT fk_tbl_working_paper_att FOREIGN KEY (attachment_id) REFERENCES tbl_attachment (attachment_id);
ALTER TABLE tbl_working_paper ADD CONSTRAINT fk_tbl_working_paper_sta FOREIGN KEY (status_id) REFERENCES tbl_lookup_value (lookup_value_id);
ALTER TABLE tbl_legacy_key_map ADD CONSTRAINT fk_tbl_legacy_key_map_batch FOREIGN KEY (migration_batch_id) REFERENCES tbl_migration_batch (migration_batch_id);
ALTER TABLE tbl_migration_issue ADD CONSTRAINT fk_tbl_migration_issue_batch FOREIGN KEY (migration_batch_id) REFERENCES tbl_migration_batch (migration_batch_id);
ALTER TABLE tbl_document_migration_queue ADD CONSTRAINT fk_tbl_document_mig_batch FOREIGN KEY (migration_batch_id) REFERENCES tbl_migration_batch (migration_batch_id);

-------------------------------------------------------------------------------
-- Key indexes
-------------------------------------------------------------------------------

CREATE INDEX idx_tbl_entity_type_id          ON tbl_entity (entity_type_id);
CREATE INDEX idx_tbl_entity_parent_id        ON tbl_entity (parent_entity_id);
CREATE INDEX idx_tbl_user_home_entity        ON tbl_user (home_entity_id);
CREATE INDEX idx_tbl_user_role_role          ON tbl_user_role (role_id);
CREATE INDEX idx_tbl_role_permission_perm    ON tbl_role_permission (permission_id);
CREATE INDEX idx_tbl_user_scope_entity       ON tbl_user_scope (entity_id);
CREATE UNIQUE INDEX ux_tbl_plan_criteria_rule ON tbl_plan_criteria (audit_period_id, entity_type_id, NVL(risk_rating_id, -1), NVL(size_band_id, -1), NVL(frequency_id, -1));
CREATE INDEX idx_tbl_plan_criteria_period    ON tbl_plan_criteria (audit_period_id);
CREATE INDEX idx_tbl_engagement_period_ent   ON tbl_engagement (audit_period_id, entity_id);
CREATE INDEX idx_tbl_engagement_member_eng   ON tbl_engagement_member (engagement_id);
CREATE INDEX idx_tbl_engagement_task_member  ON tbl_engagement_task (engagement_member_id, task_status_id);
CREATE INDEX idx_tbl_eng_checklist_eng       ON tbl_engagement_checklist (engagement_id, checklist_status_id);
CREATE INDEX idx_tbl_observation_eng_status  ON tbl_observation (engagement_id, observation_status_id);
CREATE INDEX idx_tbl_observation_entity      ON tbl_observation (entity_id, severity_id);
CREATE INDEX idx_tbl_obs_assignment_user     ON tbl_observation_assignment (assignee_user_id, assignment_status_id);
CREATE INDEX idx_tbl_obs_response_obs        ON tbl_observation_response (observation_id, response_stage_id);
CREATE INDEX idx_tbl_compliance_case_status  ON tbl_compliance_case (compliance_status_id, compliance_stage_id);
CREATE INDEX idx_tbl_iid_case_status         ON tbl_iid_case (case_status_id, assigned_entity_id);
CREATE INDEX idx_tbl_iid_plan_case           ON tbl_iid_investigation_plan (iid_case_id, plan_status_id);
CREATE INDEX idx_tbl_iid_find_case           ON tbl_iid_finding (iid_case_id, finding_status_id);
CREATE INDEX idx_tbl_iid_exc_item_report     ON tbl_iid_exception_item (iid_exception_report_id, item_type_id);
CREATE INDEX idx_tbl_commercial_om_status    ON tbl_commercial_om (commercial_status_id);
CREATE INDEX idx_tbl_commercial_pdp_status   ON tbl_commercial_pdp (commercial_status_id);
CREATE INDEX idx_tbl_commercial_arpse_status ON tbl_commercial_arpse (commercial_status_id);
CREATE INDEX idx_tbl_report_engagement       ON tbl_report (engagement_id, report_status_id);
CREATE INDEX idx_tbl_notify_queue_status     ON tbl_notification_queue (queue_status_id, queued_on);
CREATE INDEX idx_tbl_attachment_link_src     ON tbl_attachment_link (source_entity_name, source_entity_id);
CREATE INDEX idx_tbl_workflow_event_src      ON tbl_workflow_event (source_entity_name, source_entity_id, event_on);

-------------------------------------------------------------------------------
-- Additional Phase 3 indexes
-------------------------------------------------------------------------------

CREATE INDEX idx_tbl_attachment_doc_type       ON tbl_attachment (document_type_id);
CREATE INDEX idx_tbl_attachment_legacy_src     ON tbl_attachment (legacy_source_table, legacy_source_pk_value);
CREATE INDEX idx_tbl_app_page_parent           ON tbl_application_page (parent_application_page_id);
CREATE INDEX idx_tbl_app_page_status           ON tbl_application_page (page_status_id);
CREATE INDEX idx_tbl_user_branch_ent           ON tbl_user (branch_entity_id, department_entity_id, audit_zone_entity_id);
CREATE INDEX idx_tbl_user_session_ent_role     ON tbl_user_session (entity_id, role_id);
CREATE INDEX idx_tbl_engagement_plan           ON tbl_engagement (audit_plan_id);
CREATE INDEX idx_tbl_workflow_event_status     ON tbl_workflow_event (module_code, status_id, entity_id, event_on);
CREATE INDEX idx_tbl_ref_doc_version_parent    ON tbl_reference_document_version (parent_reference_document_version_id);
CREATE INDEX idx_tbl_obs_assign_detail         ON tbl_observation_assignment (observation_detail_id, response_status_id);
CREATE INDEX idx_tbl_obs_response_detail       ON tbl_observation_response (observation_detail_id, response_role_id);
CREATE INDEX idx_tbl_obs_reference_status      ON tbl_observation_reference (reference_status_id, risk_rating_id);
CREATE INDEX idx_tbl_obs_evidence_obs          ON tbl_observation_evidence (observation_id, evidence_status_id);
CREATE INDEX idx_tbl_obs_dsa_obs               ON tbl_observation_dsa (observation_id, dsa_status_id);
CREATE INDEX idx_tbl_compliance_case_para      ON tbl_compliance_case (legacy_para_no, compliance_status_id);
CREATE INDEX idx_tbl_compliance_evidence_case  ON tbl_compliance_evidence (compliance_case_id, evidence_status_id);
CREATE INDEX idx_tbl_compliance_hist_case      ON tbl_compliance_case_history (compliance_case_id, commented_on);
CREATE INDEX idx_tbl_para_case_entity          ON tbl_para_case (entity_id, para_status_id, case_status_id);
CREATE INDEX idx_tbl_para_assignment_case      ON tbl_para_assignment (para_case_id, assignment_status_id);
CREATE INDEX idx_tbl_para_status_case          ON tbl_para_status_history (para_case_id, changed_on);
CREATE INDEX idx_tbl_para_settle_case          ON tbl_para_settlement_history (para_case_id, settled_on);
CREATE INDEX idx_tbl_audit_plan_period_ent     ON tbl_audit_plan (audit_period_id, entity_id, plan_status_id);
CREATE INDEX idx_tbl_eng_team_eng              ON tbl_engagement_team (engagement_id, lead_user_id);
CREATE INDEX idx_tbl_eng_criteria_hist         ON tbl_engagement_criteria_history (engagement_id, changed_on);
CREATE INDEX idx_tbl_iid_record_case           ON tbl_iid_record (iid_case_id, status_id);
CREATE INDEX idx_tbl_iid_analysis_case         ON tbl_iid_analysis (iid_case_id, status_id);
CREATE INDEX idx_tbl_iid_assessment_case       ON tbl_iid_assessment (iid_case_id, iid_subject_id, assessment_status_id);
CREATE INDEX idx_tbl_iid_plan_approval_plan    ON tbl_iid_plan_approval (iid_investigation_plan_id, approval_status_id);
CREATE INDEX idx_tbl_iid_head_review_case      ON tbl_iid_head_review (iid_case_id, review_status_id);
CREATE INDEX idx_tbl_iid_case_study_case       ON tbl_iid_case_study (iid_case_id, case_study_status_id);
CREATE INDEX idx_tbl_iid_violation_case        ON tbl_iid_violation (iid_case_id, iid_subject_id, violation_status_id);
CREATE INDEX idx_tbl_iid_proceeding_case       ON tbl_iid_proceeding (iid_case_id, proceeding_status_id);
CREATE INDEX idx_tbl_iid_workflow_case         ON tbl_iid_workflow_history (iid_case_id, created_on);
CREATE INDEX idx_tbl_iid_subject_status        ON tbl_iid_subject (subject_status_id, linked_user_id);
CREATE INDEX idx_tbl_iid_statement_status      ON tbl_iid_statement (statement_status_id, linked_user_id);
CREATE INDEX idx_tbl_iid_inv_plan_submit       ON tbl_iid_investigation_plan (submitted_by, submitted_on);
CREATE INDEX idx_tbl_iid_report_submit         ON tbl_iid_report (submitted_by, submitted_on);
CREATE INDEX idx_tbl_report_section_entity     ON tbl_report_section (report_id, entity_id, section_code);
CREATE INDEX idx_tbl_report_snapshot_entity    ON tbl_report_snapshot (report_id, entity_id, snapshot_type_id);
CREATE INDEX idx_tbl_working_paper_module      ON tbl_working_paper (module_code, engagement_id, iid_case_id);
CREATE INDEX idx_tbl_comm_arpse_res_meeting    ON tbl_commercial_arpse_resolution (commercial_arpse_id, meeting_no, meeting_year);
CREATE INDEX idx_tbl_migration_batch_code      ON tbl_migration_batch (batch_code, batch_status_code);
CREATE INDEX idx_tbl_legacy_key_map_src        ON tbl_legacy_key_map (source_table_name, source_pk_value);
CREATE INDEX idx_tbl_legacy_key_map_tgt        ON tbl_legacy_key_map (target_table_name, target_pk_value);
CREATE INDEX idx_tbl_migration_issue_src       ON tbl_migration_issue (source_table_name, issue_status_code);
CREATE INDEX idx_tbl_doc_mig_queue_src         ON tbl_document_migration_queue (source_table_name, queue_status_code);

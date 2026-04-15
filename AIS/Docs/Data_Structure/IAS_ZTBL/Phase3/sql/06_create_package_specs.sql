/*
  IAS_ZTBL Phase 3 package specification script
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

CREATE OR REPLACE PACKAGE pkg_sec IS
    PROCEDURE save_role(
        io_role_id         IN OUT NUMBER,
        p_role_code        IN VARCHAR2,
        p_role_name        IN VARCHAR2,
        p_role_type_id     IN NUMBER,
        p_role_description IN VARCHAR2,
        p_is_system_role   IN CHAR,
        p_is_active        IN CHAR,
        p_actor_user_id    IN NUMBER,
        o_result_code      OUT NUMBER,
        o_result_message   OUT VARCHAR2
    );

    PROCEDURE save_user(
        io_user_id         IN OUT NUMBER,
        p_login_name       IN VARCHAR2,
        p_pp_no            IN NUMBER,
        p_display_name     IN VARCHAR2,
        p_email_address    IN VARCHAR2,
        p_password_hash    IN VARCHAR2,
        p_home_entity_id   IN NUMBER,
        p_user_status_id   IN NUMBER,
        p_is_locked        IN CHAR,
        p_is_active        IN CHAR,
        p_actor_user_id    IN NUMBER,
        o_result_code      OUT NUMBER,
        o_result_message   OUT VARCHAR2
    );

    PROCEDURE assign_user_role(
        p_user_id          IN NUMBER,
        p_role_id          IN NUMBER,
        p_is_primary_role  IN CHAR,
        p_actor_user_id    IN NUMBER,
        o_result_code      OUT NUMBER,
        o_result_message   OUT VARCHAR2
    );

    PROCEDURE grant_role_permission(
        p_role_id          IN NUMBER,
        p_permission_id    IN NUMBER,
        p_actor_user_id    IN NUMBER,
        o_result_code      OUT NUMBER,
        o_result_message   OUT VARCHAR2
    );

    PROCEDURE list_user_access(
        p_user_id          IN NUMBER,
        io_cursor          OUT SYS_REFCURSOR
    );
END pkg_sec;
/

CREATE OR REPLACE PACKAGE pkg_master IS
    PROCEDURE save_lookup_value(
        io_lookup_value_id  IN OUT NUMBER,
        p_lookup_type_id    IN NUMBER,
        p_lookup_code       IN VARCHAR2,
        p_lookup_name       IN VARCHAR2,
        p_short_name        IN VARCHAR2,
        p_sort_order        IN NUMBER,
        p_is_default        IN CHAR,
        p_is_active         IN CHAR,
        p_actor_user_id     IN NUMBER,
        o_result_code       OUT NUMBER,
        o_result_message    OUT VARCHAR2
    );

    PROCEDURE save_entity_type(
        io_entity_type_id   IN OUT NUMBER,
        p_entity_type_code  IN VARCHAR2,
        p_entity_type_name  IN VARCHAR2,
        p_entity_group_id   IN NUMBER,
        p_is_auditable      IN CHAR,
        p_is_active         IN CHAR,
        p_actor_user_id     IN NUMBER,
        o_result_code       OUT NUMBER,
        o_result_message    OUT VARCHAR2
    );

    PROCEDURE save_entity(
        io_entity_id        IN OUT NUMBER,
        p_entity_type_id    IN NUMBER,
        p_entity_code       IN VARCHAR2,
        p_entity_name       IN VARCHAR2,
        p_parent_entity_id  IN NUMBER,
        p_risk_rating_id    IN NUMBER,
        p_size_band_id      IN NUMBER,
        p_entity_status_id  IN NUMBER,
        p_is_auditable      IN CHAR,
        p_is_active         IN CHAR,
        p_actor_user_id     IN NUMBER,
        o_result_code       OUT NUMBER,
        o_result_message    OUT VARCHAR2
    );

    PROCEDURE save_reference_document(
        io_reference_document_id IN OUT NUMBER,
        p_reference_type_id      IN NUMBER,
        p_document_code          IN VARCHAR2,
        p_document_title         IN VARCHAR2,
        p_issue_date             IN DATE,
        p_effective_date         IN DATE,
        p_expiry_date            IN DATE,
        p_source_url             IN VARCHAR2,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    );

    PROCEDURE list_entities(
        p_entity_type_id    IN NUMBER,
        p_parent_entity_id  IN NUMBER,
        io_cursor           OUT SYS_REFCURSOR
    );
END pkg_master;
/

CREATE OR REPLACE PACKAGE pkg_planning IS
    PROCEDURE save_plan_criteria(
        io_plan_criteria_id IN OUT NUMBER,
        p_audit_period_id   IN NUMBER,
        p_entity_type_id    IN NUMBER,
        p_risk_rating_id    IN NUMBER,
        p_size_band_id      IN NUMBER,
        p_frequency_id      IN NUMBER,
        p_duration_days     IN NUMBER,
        p_visit_count       IN NUMBER,
        p_criteria_status_id IN NUMBER,
        p_actor_user_id     IN NUMBER,
        o_result_code       OUT NUMBER,
        o_result_message    OUT VARCHAR2
    );

    PROCEDURE save_engagement(
        io_engagement_id    IN OUT NUMBER,
        p_engagement_no     IN VARCHAR2,
        p_audit_period_id   IN NUMBER,
        p_plan_criteria_id  IN NUMBER,
        p_entity_id         IN NUMBER,
        p_engagement_type_id IN NUMBER,
        p_team_name         IN VARCHAR2,
        p_audit_start_on    IN DATE,
        p_audit_end_on      IN DATE,
        p_engagement_status_id IN NUMBER,
        p_actor_user_id     IN NUMBER,
        o_result_code       OUT NUMBER,
        o_result_message    OUT VARCHAR2
    );

    PROCEDURE save_engagement_member(
        io_engagement_member_id IN OUT NUMBER,
        p_engagement_id         IN NUMBER,
        p_user_id               IN NUMBER,
        p_member_role_id        IN NUMBER,
        p_is_team_lead          IN CHAR,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE list_engagements(
        p_audit_period_id   IN NUMBER,
        p_entity_id         IN NUMBER,
        io_cursor           OUT SYS_REFCURSOR
    );
END pkg_planning;
/

CREATE OR REPLACE PACKAGE pkg_execution IS
    PROCEDURE save_engagement_task(
        io_engagement_task_id IN OUT NUMBER,
        p_engagement_id       IN NUMBER,
        p_engagement_member_id IN NUMBER,
        p_task_name           IN VARCHAR2,
        p_task_type_id        IN NUMBER,
        p_task_status_id      IN NUMBER,
        p_planned_start_on    IN DATE,
        p_planned_end_on      IN DATE,
        p_actor_user_id       IN NUMBER,
        o_result_code         OUT NUMBER,
        o_result_message      OUT VARCHAR2
    );

    PROCEDURE upsert_engagement_checklist(
        io_engagement_checklist_id IN OUT NUMBER,
        p_engagement_id            IN NUMBER,
        p_checklist_item_id        IN NUMBER,
        p_assigned_member_id       IN NUMBER,
        p_checklist_status_id      IN NUMBER,
        p_remarks                  IN VARCHAR2,
        p_actor_user_id            IN NUMBER,
        o_result_code              OUT NUMBER,
        o_result_message           OUT VARCHAR2
    );

    PROCEDURE complete_engagement_task(
        p_engagement_task_id IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE get_task_board(
        p_engagement_id      IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    );
END pkg_execution;
/

CREATE OR REPLACE PACKAGE pkg_observation IS
    PROCEDURE save_observation(
        io_observation_id   IN OUT NUMBER,
        p_observation_no    IN VARCHAR2,
        p_engagement_id     IN NUMBER,
        p_entity_id         IN NUMBER,
        p_checklist_item_id IN NUMBER,
        p_observation_status_id IN NUMBER,
        p_severity_id       IN NUMBER,
        p_risk_rating_id    IN NUMBER,
        p_amount_involved   IN NUMBER,
        p_actor_user_id     IN NUMBER,
        o_result_code       OUT NUMBER,
        o_result_message    OUT VARCHAR2
    );

    PROCEDURE save_observation_detail(
        io_observation_detail_id IN OUT NUMBER,
        p_observation_id         IN NUMBER,
        p_detail_type_id         IN NUMBER,
        p_heading_text           IN VARCHAR2,
        p_detail_text            IN CLOB,
        p_sequence_no            IN NUMBER,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    );

    PROCEDURE assign_observation(
        io_observation_assignment_id IN OUT NUMBER,
        p_observation_id             IN NUMBER,
        p_assignee_user_id           IN NUMBER,
        p_assignment_role_id         IN NUMBER,
        p_assignment_status_id       IN NUMBER,
        p_due_on                     IN DATE,
        p_actor_user_id              IN NUMBER,
        o_result_code                OUT NUMBER,
        o_result_message             OUT VARCHAR2
    );

    PROCEDURE add_observation_response(
        io_observation_response_id IN OUT NUMBER,
        p_observation_id          IN NUMBER,
        p_response_stage_id       IN NUMBER,
        p_respondent_user_id      IN NUMBER,
        p_response_status_id      IN NUMBER,
        p_response_text           IN CLOB,
        p_submitted_flag          IN CHAR,
        p_actor_user_id           IN NUMBER,
        o_result_code             OUT NUMBER,
        o_result_message          OUT VARCHAR2
    );

    PROCEDURE list_observations(
        p_engagement_id      IN NUMBER,
        p_entity_id          IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    );
END pkg_observation;
/

CREATE OR REPLACE PACKAGE pkg_compliance IS
    PROCEDURE open_compliance_case(
        io_compliance_case_id IN OUT NUMBER,
        p_observation_id      IN NUMBER,
        p_compliance_cycle_no IN NUMBER,
        p_compliance_status_id IN NUMBER,
        p_compliance_stage_id IN NUMBER,
        p_responsible_entity_id IN NUMBER,
        p_actor_user_id       IN NUMBER,
        o_result_code         OUT NUMBER,
        o_result_message      OUT VARCHAR2
    );

    PROCEDURE add_compliance_action(
        io_compliance_action_id IN OUT NUMBER,
        p_compliance_case_id    IN NUMBER,
        p_action_type_id        IN NUMBER,
        p_action_status_id      IN NUMBER,
        p_action_owner_user_id  IN NUMBER,
        p_action_text           IN CLOB,
        p_target_date           IN DATE,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE record_compliance_review(
        io_compliance_review_id IN OUT NUMBER,
        p_compliance_case_id    IN NUMBER,
        p_review_stage_id       IN NUMBER,
        p_review_status_id      IN NUMBER,
        p_reviewer_user_id      IN NUMBER,
        p_review_text           IN CLOB,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE list_compliance_cases(
        p_observation_id        IN NUMBER,
        io_cursor              OUT SYS_REFCURSOR
    );
END pkg_compliance;
/

CREATE OR REPLACE PACKAGE pkg_inquiry IS
    PROCEDURE create_case(
        io_iid_case_id       IN OUT NUMBER,
        p_case_no            IN VARCHAR2,
        p_intake_channel_id  IN NUMBER,
        p_complaint_source_id IN NUMBER,
        p_complaint_type_id  IN NUMBER,
        p_priority_id        IN NUMBER,
        p_case_status_id     IN NUMBER,
        p_assigned_entity_id IN NUMBER,
        p_case_summary       IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE add_complainant(
        io_iid_complainant_id IN OUT NUMBER,
        p_iid_case_id         IN NUMBER,
        p_complainant_name    IN VARCHAR2,
        p_cnic_no             IN VARCHAR2,
        p_contact_no          IN VARCHAR2,
        p_email_address       IN VARCHAR2,
        p_is_primary          IN CHAR,
        p_actor_user_id       IN NUMBER,
        o_result_code         OUT NUMBER,
        o_result_message      OUT VARCHAR2
    );

    PROCEDURE save_investigation_plan(
        io_iid_investigation_plan_id IN OUT NUMBER,
        p_iid_case_id         IN NUMBER,
        p_plan_title          IN VARCHAR2,
        p_plan_details        IN CLOB,
        p_risk_level_id       IN NUMBER,
        p_size_band_id        IN NUMBER,
        p_plan_status_id      IN NUMBER,
        p_start_date          IN DATE,
        p_end_date            IN DATE,
        p_actor_user_id       IN NUMBER,
        o_result_code         OUT NUMBER,
        o_result_message      OUT VARCHAR2
    );

    PROCEDURE submit_report(
        io_iid_report_id      IN OUT NUMBER,
        p_iid_case_id         IN NUMBER,
        p_report_no           IN VARCHAR2,
        p_report_status_id    IN NUMBER,
        p_executive_summary   IN CLOB,
        p_findings_text       IN CLOB,
        p_recommendation_text IN CLOB,
        p_actor_user_id       IN NUMBER,
        o_result_code         OUT NUMBER,
        o_result_message      OUT VARCHAR2
    );

    PROCEDURE list_cases(
        p_case_status_id      IN NUMBER,
        p_assigned_entity_id  IN NUMBER,
        io_cursor             OUT SYS_REFCURSOR
    );
END pkg_inquiry;
/

CREATE OR REPLACE PACKAGE pkg_commercial_audit IS
    PROCEDURE save_om(
        io_commercial_om_id  IN OUT NUMBER,
        p_audit_year         IN NUMBER,
        p_om_no              IN VARCHAR2,
        p_gist_text          IN VARCHAR2,
        p_body_text          IN CLOB,
        p_management_response IN CLOB,
        p_commercial_status_id IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE save_pdp(
        io_commercial_pdp_id IN OUT NUMBER,
        p_audit_year         IN NUMBER,
        p_pdp_no             IN VARCHAR2,
        p_gist_text          IN VARCHAR2,
        p_body_text          IN CLOB,
        p_management_response IN CLOB,
        p_dac_recommendation IN CLOB,
        p_commercial_status_id IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE link_pdp_om(
        p_commercial_pdp_id  IN NUMBER,
        p_commercial_om_id   IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE save_arpse(
        io_commercial_arpse_id IN OUT NUMBER,
        p_audit_year         IN NUMBER,
        p_para_no            IN VARCHAR2,
        p_gist_text          IN VARCHAR2,
        p_management_response IN CLOB,
        p_commercial_status_id IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE list_commercial_items(
        p_audit_year         IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    );
END pkg_commercial_audit;
/

CREATE OR REPLACE PACKAGE pkg_report IS
    PROCEDURE save_report(
        io_report_id         IN OUT NUMBER,
        p_engagement_id      IN NUMBER,
        p_report_type_id     IN NUMBER,
        p_report_title       IN VARCHAR2,
        p_report_status_id   IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE save_report_section(
        io_report_section_id IN OUT NUMBER,
        p_report_id          IN NUMBER,
        p_section_code       IN VARCHAR2,
        p_section_title      IN VARCHAR2,
        p_sequence_no        IN NUMBER,
        p_section_text       IN CLOB,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    );

    PROCEDURE save_report_snapshot(
        io_report_snapshot_id IN OUT NUMBER,
        p_report_id           IN NUMBER,
        p_snapshot_type_id    IN NUMBER,
        p_snapshot_key        IN VARCHAR2,
        p_snapshot_value      IN VARCHAR2,
        p_snapshot_text       IN CLOB,
        p_actor_user_id       IN NUMBER,
        o_result_code         OUT NUMBER,
        o_result_message      OUT VARCHAR2
    );

    PROCEDURE get_report_summary(
        p_engagement_id      IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    );
END pkg_report;
/

CREATE OR REPLACE PACKAGE pkg_notify IS
    PROCEDURE upsert_event(
        io_notification_event_id IN OUT NUMBER,
        p_event_code             IN VARCHAR2,
        p_event_name             IN VARCHAR2,
        p_module_code            IN VARCHAR2,
        p_description            IN VARCHAR2,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    );

    PROCEDURE upsert_template(
        io_notification_template_id IN OUT NUMBER,
        p_notification_event_id     IN NUMBER,
        p_culture_code              IN VARCHAR2,
        p_subject_template          IN CLOB,
        p_body_template             IN CLOB,
        p_actor_user_id             IN NUMBER,
        o_result_code               OUT NUMBER,
        o_result_message            OUT VARCHAR2
    );

    PROCEDURE upsert_rule(
        io_notification_rule_id IN OUT NUMBER,
        p_notification_event_id IN NUMBER,
        p_recipient_type_id     IN NUMBER,
        p_recipient_expression  IN VARCHAR2,
        p_cc_expression         IN VARCHAR2,
        p_attachment_policy_id  IN NUMBER,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE queue_notification(
        io_notification_queue_id IN OUT NUMBER,
        p_notification_event_id  IN NUMBER,
        p_source_entity_name     IN VARCHAR2,
        p_source_entity_id       IN NUMBER,
        p_recipient_to           IN VARCHAR2,
        p_recipient_cc           IN VARCHAR2,
        p_subject_text           IN VARCHAR2,
        p_body_text              IN CLOB,
        p_queue_status_id        IN NUMBER,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    );

    PROCEDURE get_pending_queue(
        p_queue_status_id        IN NUMBER,
        io_cursor                OUT SYS_REFCURSOR
    );
END pkg_notify;
/

CREATE OR REPLACE PACKAGE pkg_entity IS
    PROCEDURE save_entity(
        io_entity_id            IN OUT NUMBER,
        p_entity_type_id        IN NUMBER,
        p_entity_code           IN VARCHAR2,
        p_entity_name           IN VARCHAR2,
        p_parent_entity_id      IN NUMBER,
        p_audited_by_entity_id  IN NUMBER,
        p_compliance_entity_id  IN NUMBER,
        p_cost_center_code      IN VARCHAR2,
        p_address_line          IN VARCHAR2,
        p_telephone_no          IN VARCHAR2,
        p_email_address         IN VARCHAR2,
        p_risk_rating_id        IN NUMBER,
        p_size_band_id          IN NUMBER,
        p_entity_status_id      IN NUMBER,
        p_is_auditable          IN CHAR,
        p_is_active             IN CHAR,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE save_entity_relation(
        io_entity_relation_id   IN OUT NUMBER,
        p_parent_entity_id      IN NUMBER,
        p_child_entity_id       IN NUMBER,
        p_relation_type_id      IN NUMBER,
        p_effective_from        IN DATE,
        p_effective_to          IN DATE,
        p_is_active             IN CHAR,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE add_risk_profile(
        io_entity_risk_profile_id IN OUT NUMBER,
        p_entity_id               IN NUMBER,
        p_risk_rating_id          IN NUMBER,
        p_effective_from          IN DATE,
        p_effective_to            IN DATE,
        p_detail_text             IN CLOB,
        p_is_current_flag         IN CHAR,
        p_is_active               IN CHAR,
        p_actor_user_id           IN NUMBER,
        o_result_code             OUT NUMBER,
        o_result_message          OUT VARCHAR2
    );

    PROCEDURE add_size_profile(
        io_entity_size_profile_id IN OUT NUMBER,
        p_entity_id               IN NUMBER,
        p_size_band_id            IN NUMBER,
        p_description             IN VARCHAR2,
        p_effective_from          IN DATE,
        p_effective_to            IN DATE,
        p_is_current_flag         IN CHAR,
        p_is_active               IN CHAR,
        p_actor_user_id           IN NUMBER,
        o_result_code             OUT NUMBER,
        o_result_message          OUT VARCHAR2
    );

    PROCEDURE list_entity_hierarchy(
        p_parent_entity_id      IN NUMBER,
        io_cursor               OUT SYS_REFCURSOR
    );
END pkg_entity;
/

CREATE OR REPLACE PACKAGE pkg_reference IS
    PROCEDURE save_reference_document(
        io_reference_document_id IN OUT NUMBER,
        p_reference_type_id      IN NUMBER,
        p_document_code          IN VARCHAR2,
        p_document_title         IN VARCHAR2,
        p_issuing_authority      IN VARCHAR2,
        p_issue_date             IN DATE,
        p_effective_date         IN DATE,
        p_expiry_date            IN DATE,
        p_source_url             IN VARCHAR2,
        p_is_active              IN CHAR,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    );

    PROCEDURE save_reference_document_version(
        io_reference_document_version_id IN OUT NUMBER,
        p_reference_document_id          IN NUMBER,
        p_version_no                     IN NUMBER,
        p_version_label                  IN VARCHAR2,
        p_document_text                  IN CLOB,
        p_storage_reference              IN VARCHAR2,
        p_heading_text                   IN VARCHAR2,
        p_section_code                   IN VARCHAR2,
        p_chapter_no                     IN VARCHAR2,
        p_page_no                        IN NUMBER,
        p_is_current_version             IN CHAR,
        p_is_active                      IN CHAR,
        p_actor_user_id                  IN NUMBER,
        o_result_code                    OUT NUMBER,
        o_result_message                 OUT VARCHAR2
    );

    PROCEDURE save_checklist_annexure(
        io_checklist_annexure_id         IN OUT NUMBER,
        p_checklist_item_id              IN NUMBER,
        p_annexure_type_id               IN NUMBER,
        p_heading_text                   IN VARCHAR2,
        p_detail_text                    IN CLOB,
        p_reference_document_version_id  IN NUMBER,
        p_attachment_id                  IN NUMBER,
        p_effective_on                   IN DATE,
        p_expiry_on                      IN DATE,
        p_sort_order                     IN NUMBER,
        p_is_active                      IN CHAR,
        p_actor_user_id                  IN NUMBER,
        o_result_code                    OUT NUMBER,
        o_result_message                 OUT VARCHAR2
    );

    PROCEDURE list_manual_index(
        p_reference_document_id IN NUMBER,
        io_cursor               OUT SYS_REFCURSOR
    );
END pkg_reference;
/

CREATE OR REPLACE PACKAGE pkg_document IS
    PROCEDURE register_attachment(
        io_attachment_id        IN OUT NUMBER,
        p_storage_key           IN VARCHAR2,
        p_original_file_name    IN VARCHAR2,
        p_mime_type             IN VARCHAR2,
        p_file_size_bytes       IN NUMBER,
        p_document_type_id      IN NUMBER,
        p_legacy_file_path      IN VARCHAR2,
        p_legacy_source_table   IN VARCHAR2,
        p_legacy_source_pk_value IN VARCHAR2,
        p_uploaded_by           IN NUMBER,
        p_is_active             IN CHAR,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE link_attachment(
        io_attachment_link_id   IN OUT NUMBER,
        p_attachment_id         IN NUMBER,
        p_source_entity_name    IN VARCHAR2,
        p_source_entity_id      IN NUMBER,
        p_link_type_id          IN NUMBER,
        p_remarks               IN VARCHAR2,
        p_is_primary_link       IN CHAR,
        p_is_active             IN CHAR,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    );

    PROCEDURE queue_legacy_document(
        io_document_migration_queue_id IN OUT NUMBER,
        p_migration_batch_id           IN NUMBER,
        p_source_table_name            IN VARCHAR2,
        p_source_pk_value              IN VARCHAR2,
        p_source_column_name           IN VARCHAR2,
        p_legacy_file_path             IN VARCHAR2,
        p_legacy_file_name             IN VARCHAR2,
        p_target_table_name            IN VARCHAR2,
        p_target_pk_value              IN NUMBER,
        p_attachment_link_type_code    IN VARCHAR2,
        p_queue_status_code            IN VARCHAR2,
        p_remarks                      IN VARCHAR2,
        p_actor_user_id                IN NUMBER,
        o_result_code                  OUT NUMBER,
        o_result_message               OUT VARCHAR2
    );

    PROCEDURE get_document_queue(
        p_migration_batch_id      IN NUMBER,
        p_queue_status_code       IN VARCHAR2,
        io_cursor                 OUT SYS_REFCURSOR
    );
END pkg_document;
/

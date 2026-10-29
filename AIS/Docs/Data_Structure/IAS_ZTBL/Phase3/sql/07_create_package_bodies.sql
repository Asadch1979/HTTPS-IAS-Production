/*
  IAS_ZTBL Phase 3 package body script
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

CREATE OR REPLACE PACKAGE BODY pkg_sec IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'SECURITY', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_role_id IS NULL THEN
            INSERT INTO tbl_role (
                role_id, role_code, role_name, role_description, role_type_id,
                is_system_role, is_active, created_by
            )
            VALUES (
                NULL, UPPER(TRIM(p_role_code)), TRIM(p_role_name), p_role_description, p_role_type_id,
                NVL(p_is_system_role, 'N'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING role_id INTO io_role_id;

            log_event('TBL_ROLE', io_role_id, 'ROLE_CREATED', p_actor_user_id, 'Role created');
        ELSE
            UPDATE tbl_role
               SET role_code        = UPPER(TRIM(p_role_code)),
                   role_name        = TRIM(p_role_name),
                   role_description = p_role_description,
                   role_type_id     = p_role_type_id,
                   is_system_role   = NVL(p_is_system_role, 'N'),
                   is_active        = NVL(p_is_active, 'Y'),
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE role_id = io_role_id;

            log_event('TBL_ROLE', io_role_id, 'ROLE_UPDATED', p_actor_user_id, 'Role updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Role saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_role;

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
    ) IS
    BEGIN
        IF io_user_id IS NULL THEN
            INSERT INTO tbl_user (
                user_id, login_name, pp_no, display_name, email_address, password_hash,
                home_entity_id, user_status_id, is_locked, is_active, created_by
            )
            VALUES (
                NULL, LOWER(TRIM(p_login_name)), p_pp_no, TRIM(p_display_name), p_email_address, p_password_hash,
                p_home_entity_id, p_user_status_id, NVL(p_is_locked, 'N'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING user_id INTO io_user_id;

            log_event('TBL_USER', io_user_id, 'USER_CREATED', p_actor_user_id, 'User created');
        ELSE
            UPDATE tbl_user
               SET login_name     = LOWER(TRIM(p_login_name)),
                   pp_no           = p_pp_no,
                   display_name    = TRIM(p_display_name),
                   email_address   = p_email_address,
                   password_hash   = COALESCE(p_password_hash, password_hash),
                   home_entity_id  = p_home_entity_id,
                   user_status_id  = p_user_status_id,
                   is_locked       = NVL(p_is_locked, 'N'),
                   is_active       = NVL(p_is_active, 'Y'),
                   modified_by     = p_actor_user_id,
                   modified_on     = SYSDATE,
                   record_version  = record_version + 1
             WHERE user_id = io_user_id;

            log_event('TBL_USER', io_user_id, 'USER_UPDATED', p_actor_user_id, 'User updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'User saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_user;

    PROCEDURE assign_user_role(
        p_user_id          IN NUMBER,
        p_role_id          IN NUMBER,
        p_is_primary_role  IN CHAR,
        p_actor_user_id    IN NUMBER,
        o_result_code      OUT NUMBER,
        o_result_message   OUT VARCHAR2
    ) IS
        l_user_role_id NUMBER;
    BEGIN
        BEGIN
            SELECT user_role_id
              INTO l_user_role_id
              FROM tbl_user_role
             WHERE user_id = p_user_id
               AND role_id = p_role_id;

            UPDATE tbl_user_role
               SET is_primary_role = NVL(p_is_primary_role, 'N'),
                   is_active       = 'Y',
                   modified_by     = p_actor_user_id,
                   modified_on     = SYSDATE,
                   record_version  = record_version + 1
             WHERE user_role_id = l_user_role_id;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                INSERT INTO tbl_user_role (
                    user_role_id, user_id, role_id, is_primary_role, created_by
                )
                VALUES (
                    NULL, p_user_id, p_role_id, NVL(p_is_primary_role, 'N'), p_actor_user_id
                )
                RETURNING user_role_id INTO l_user_role_id;
        END;

        log_event('TBL_USER_ROLE', l_user_role_id, 'USER_ROLE_ASSIGNED', p_actor_user_id, 'User role assigned');
        o_result_code := 0;
        o_result_message := 'Role assignment saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END assign_user_role;

    PROCEDURE grant_role_permission(
        p_role_id          IN NUMBER,
        p_permission_id    IN NUMBER,
        p_actor_user_id    IN NUMBER,
        o_result_code      OUT NUMBER,
        o_result_message   OUT VARCHAR2
    ) IS
        l_role_permission_id NUMBER;
    BEGIN
        BEGIN
            SELECT role_permission_id
              INTO l_role_permission_id
              FROM tbl_role_permission
             WHERE role_id = p_role_id
               AND permission_id = p_permission_id;

            UPDATE tbl_role_permission
               SET is_active      = 'Y',
                   modified_by    = p_actor_user_id,
                   modified_on    = SYSDATE,
                   record_version = record_version + 1
             WHERE role_permission_id = l_role_permission_id;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                INSERT INTO tbl_role_permission (
                    role_permission_id, role_id, permission_id, created_by
                )
                VALUES (
                    NULL, p_role_id, p_permission_id, p_actor_user_id
                )
                RETURNING role_permission_id INTO l_role_permission_id;
        END;

        log_event('TBL_ROLE_PERMISSION', l_role_permission_id, 'ROLE_PERMISSION_GRANTED', p_actor_user_id, 'Permission granted');
        o_result_code := 0;
        o_result_message := 'Permission grant saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END grant_role_permission;

    PROCEDURE list_user_access(
        p_user_id          IN NUMBER,
        io_cursor          OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT
                u.user_id,
                u.login_name,
                r.role_id,
                r.role_code,
                p.permission_id,
                p.permission_code,
                ap.page_code,
                ae.api_code
            FROM tbl_user u
            JOIN tbl_user_role ur
              ON ur.user_id = u.user_id
             AND ur.is_active = 'Y'
            JOIN tbl_role r
              ON r.role_id = ur.role_id
            LEFT JOIN tbl_role_permission rp
              ON rp.role_id = r.role_id
             AND rp.is_active = 'Y'
            LEFT JOIN tbl_permission p
              ON p.permission_id = rp.permission_id
            LEFT JOIN tbl_application_page ap
              ON ap.application_page_id = p.application_page_id
            LEFT JOIN tbl_api_endpoint ae
              ON ae.api_endpoint_id = p.api_endpoint_id
            WHERE u.user_id = p_user_id
            ORDER BY r.role_code, p.permission_code;
    END list_user_access;
END pkg_sec;
/

CREATE OR REPLACE PACKAGE BODY pkg_planning IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'PLANNING', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_plan_criteria_id IS NULL THEN
            INSERT INTO tbl_plan_criteria (
                plan_criteria_id, audit_period_id, entity_type_id, risk_rating_id, size_band_id,
                frequency_id, duration_days, visit_count, criteria_status_id, created_by
            )
            VALUES (
                NULL, p_audit_period_id, p_entity_type_id, p_risk_rating_id, p_size_band_id,
                p_frequency_id, p_duration_days, NVL(p_visit_count, 1), p_criteria_status_id, p_actor_user_id
            )
            RETURNING plan_criteria_id INTO io_plan_criteria_id;
            log_event('TBL_PLAN_CRITERIA', io_plan_criteria_id, 'PLAN_CRITERIA_CREATED', p_actor_user_id, 'Plan criteria created');
        ELSE
            UPDATE tbl_plan_criteria
               SET audit_period_id   = p_audit_period_id,
                   entity_type_id    = p_entity_type_id,
                   risk_rating_id    = p_risk_rating_id,
                   size_band_id      = p_size_band_id,
                   frequency_id      = p_frequency_id,
                   duration_days     = p_duration_days,
                   visit_count       = NVL(p_visit_count, 1),
                   criteria_status_id = p_criteria_status_id,
                   modified_by       = p_actor_user_id,
                   modified_on       = SYSDATE,
                   record_version    = record_version + 1
             WHERE plan_criteria_id = io_plan_criteria_id;
            log_event('TBL_PLAN_CRITERIA', io_plan_criteria_id, 'PLAN_CRITERIA_UPDATED', p_actor_user_id, 'Plan criteria updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Plan criteria saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_plan_criteria;

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
    ) IS
    BEGIN
        IF io_engagement_id IS NULL THEN
            INSERT INTO tbl_engagement (
                engagement_id, engagement_no, audit_period_id, plan_criteria_id, entity_id,
                engagement_type_id, team_name, audit_start_on, audit_end_on,
                engagement_status_id, created_by
            )
            VALUES (
                NULL, UPPER(TRIM(p_engagement_no)), p_audit_period_id, p_plan_criteria_id, p_entity_id,
                p_engagement_type_id, TRIM(p_team_name), p_audit_start_on, p_audit_end_on,
                p_engagement_status_id, p_actor_user_id
            )
            RETURNING engagement_id INTO io_engagement_id;
            log_event('TBL_ENGAGEMENT', io_engagement_id, 'ENGAGEMENT_CREATED', p_actor_user_id, 'Engagement created');
        ELSE
            UPDATE tbl_engagement
               SET engagement_no        = UPPER(TRIM(p_engagement_no)),
                   audit_period_id      = p_audit_period_id,
                   plan_criteria_id     = p_plan_criteria_id,
                   entity_id            = p_entity_id,
                   engagement_type_id   = p_engagement_type_id,
                   team_name            = TRIM(p_team_name),
                   audit_start_on       = p_audit_start_on,
                   audit_end_on         = p_audit_end_on,
                   engagement_status_id = p_engagement_status_id,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE engagement_id = io_engagement_id;
            log_event('TBL_ENGAGEMENT', io_engagement_id, 'ENGAGEMENT_UPDATED', p_actor_user_id, 'Engagement updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Engagement saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_engagement;

    PROCEDURE save_engagement_member(
        io_engagement_member_id IN OUT NUMBER,
        p_engagement_id         IN NUMBER,
        p_user_id               IN NUMBER,
        p_member_role_id        IN NUMBER,
        p_is_team_lead          IN CHAR,
        p_actor_user_id         IN NUMBER,
        o_result_code           OUT NUMBER,
        o_result_message        OUT VARCHAR2
    ) IS
    BEGIN
        IF io_engagement_member_id IS NULL THEN
            INSERT INTO tbl_engagement_member (
                engagement_member_id, engagement_id, user_id, member_role_id, is_team_lead, created_by
            )
            VALUES (
                NULL, p_engagement_id, p_user_id, p_member_role_id, NVL(p_is_team_lead, 'N'), p_actor_user_id
            )
            RETURNING engagement_member_id INTO io_engagement_member_id;
            log_event('TBL_ENGAGEMENT_MEMBER', io_engagement_member_id, 'ENGAGEMENT_MEMBER_CREATED', p_actor_user_id, 'Engagement member added');
        ELSE
            UPDATE tbl_engagement_member
               SET member_role_id  = p_member_role_id,
                   is_team_lead    = NVL(p_is_team_lead, 'N'),
                   modified_by     = p_actor_user_id,
                   modified_on     = SYSDATE,
                   record_version  = record_version + 1
             WHERE engagement_member_id = io_engagement_member_id;
            log_event('TBL_ENGAGEMENT_MEMBER', io_engagement_member_id, 'ENGAGEMENT_MEMBER_UPDATED', p_actor_user_id, 'Engagement member updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Engagement member saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_engagement_member;

    PROCEDURE list_engagements(
        p_audit_period_id   IN NUMBER,
        p_entity_id         IN NUMBER,
        io_cursor           OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT *
            FROM vw_engagement_summary
            WHERE (p_audit_period_id IS NULL OR audit_period_id = p_audit_period_id)
              AND (p_entity_id IS NULL OR entity_id = p_entity_id)
            ORDER BY engagement_no;
    END list_engagements;
END pkg_planning;
/

CREATE OR REPLACE PACKAGE BODY pkg_execution IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'EXECUTION', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_engagement_task_id IS NULL THEN
            INSERT INTO tbl_engagement_task (
                engagement_task_id, engagement_id, engagement_member_id, task_name,
                task_type_id, task_status_id, planned_start_on, planned_end_on, created_by
            )
            VALUES (
                NULL, p_engagement_id, p_engagement_member_id, TRIM(p_task_name),
                p_task_type_id, p_task_status_id, p_planned_start_on, p_planned_end_on, p_actor_user_id
            )
            RETURNING engagement_task_id INTO io_engagement_task_id;
            log_event('TBL_ENGAGEMENT_TASK', io_engagement_task_id, 'TASK_CREATED', p_actor_user_id, 'Task created');
        ELSE
            UPDATE tbl_engagement_task
               SET engagement_member_id = p_engagement_member_id,
                   task_name            = TRIM(p_task_name),
                   task_type_id         = p_task_type_id,
                   task_status_id       = p_task_status_id,
                   planned_start_on     = p_planned_start_on,
                   planned_end_on       = p_planned_end_on,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE engagement_task_id = io_engagement_task_id;
            log_event('TBL_ENGAGEMENT_TASK', io_engagement_task_id, 'TASK_UPDATED', p_actor_user_id, 'Task updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Engagement task saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_engagement_task;

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
    ) IS
    BEGIN
        IF io_engagement_checklist_id IS NULL THEN
            INSERT INTO tbl_engagement_checklist (
                engagement_checklist_id, engagement_id, checklist_item_id,
                assigned_member_id, checklist_status_id, remarks, created_by
            )
            VALUES (
                NULL, p_engagement_id, p_checklist_item_id,
                p_assigned_member_id, p_checklist_status_id, p_remarks, p_actor_user_id
            )
            RETURNING engagement_checklist_id INTO io_engagement_checklist_id;
            log_event('TBL_ENGAGEMENT_CHECKLIST', io_engagement_checklist_id, 'CHECKLIST_LINKED', p_actor_user_id, 'Checklist item linked');
        ELSE
            UPDATE tbl_engagement_checklist
               SET assigned_member_id = p_assigned_member_id,
                   checklist_status_id = p_checklist_status_id,
                   remarks             = p_remarks,
                   modified_by         = p_actor_user_id,
                   modified_on         = SYSDATE,
                   record_version      = record_version + 1
             WHERE engagement_checklist_id = io_engagement_checklist_id;
            log_event('TBL_ENGAGEMENT_CHECKLIST', io_engagement_checklist_id, 'CHECKLIST_UPDATED', p_actor_user_id, 'Checklist item updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Engagement checklist saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END upsert_engagement_checklist;

    PROCEDURE complete_engagement_task(
        p_engagement_task_id IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    ) IS
    BEGIN
        UPDATE tbl_engagement_task
           SET completed_on   = SYSDATE,
               modified_by    = p_actor_user_id,
               modified_on    = SYSDATE,
               record_version = record_version + 1
         WHERE engagement_task_id = p_engagement_task_id;

        log_event('TBL_ENGAGEMENT_TASK', p_engagement_task_id, 'TASK_COMPLETED', p_actor_user_id, 'Task completed');
        o_result_code := 0;
        o_result_message := 'Engagement task completed successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END complete_engagement_task;

    PROCEDURE get_task_board(
        p_engagement_id      IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT
                t.engagement_task_id,
                t.task_name,
                t.task_status_id,
                t.planned_start_on,
                t.planned_end_on,
                t.completed_on,
                m.user_id,
                u.display_name AS assigned_to
            FROM tbl_engagement_task t
            LEFT JOIN tbl_engagement_member m
              ON m.engagement_member_id = t.engagement_member_id
            LEFT JOIN tbl_user u
              ON u.user_id = m.user_id
            WHERE t.engagement_id = p_engagement_id
              AND t.is_active = 'Y'
            ORDER BY t.task_sequence_no, t.engagement_task_id;
    END get_task_board;
END pkg_execution;
/

CREATE OR REPLACE PACKAGE BODY pkg_compliance IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'COMPLIANCE', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_compliance_case_id IS NULL THEN
            INSERT INTO tbl_compliance_case (
                compliance_case_id, observation_id, compliance_cycle_no, compliance_status_id,
                compliance_stage_id, responsible_entity_id, created_by
            )
            VALUES (
                NULL, p_observation_id, NVL(p_compliance_cycle_no, 1), p_compliance_status_id,
                p_compliance_stage_id, p_responsible_entity_id, p_actor_user_id
            )
            RETURNING compliance_case_id INTO io_compliance_case_id;
            log_event('TBL_COMPLIANCE_CASE', io_compliance_case_id, 'COMPLIANCE_CASE_OPENED', p_actor_user_id, 'Compliance case opened');
        ELSE
            UPDATE tbl_compliance_case
               SET compliance_status_id  = p_compliance_status_id,
                   compliance_stage_id   = p_compliance_stage_id,
                   responsible_entity_id = p_responsible_entity_id,
                   modified_by           = p_actor_user_id,
                   modified_on           = SYSDATE,
                   record_version        = record_version + 1
             WHERE compliance_case_id = io_compliance_case_id;
            log_event('TBL_COMPLIANCE_CASE', io_compliance_case_id, 'COMPLIANCE_CASE_UPDATED', p_actor_user_id, 'Compliance case updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Compliance case saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END open_compliance_case;

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
    ) IS
    BEGIN
        INSERT INTO tbl_compliance_action (
            compliance_action_id, compliance_case_id, action_type_id, action_status_id,
            action_owner_user_id, action_text, target_date, created_by
        )
        VALUES (
            NULL, p_compliance_case_id, p_action_type_id, p_action_status_id,
            p_action_owner_user_id, p_action_text, p_target_date, p_actor_user_id
        )
        RETURNING compliance_action_id INTO io_compliance_action_id;

        log_event('TBL_COMPLIANCE_ACTION', io_compliance_action_id, 'COMPLIANCE_ACTION_ADDED', p_actor_user_id, 'Compliance action added');
        o_result_code := 0;
        o_result_message := 'Compliance action added successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END add_compliance_action;

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
    ) IS
    BEGIN
        INSERT INTO tbl_compliance_review (
            compliance_review_id, compliance_case_id, review_stage_id, review_status_id,
            reviewer_user_id, review_text, created_by
        )
        VALUES (
            NULL, p_compliance_case_id, p_review_stage_id, p_review_status_id,
            p_reviewer_user_id, p_review_text, p_actor_user_id
        )
        RETURNING compliance_review_id INTO io_compliance_review_id;

        log_event('TBL_COMPLIANCE_REVIEW', io_compliance_review_id, 'COMPLIANCE_REVIEW_ADDED', p_actor_user_id, 'Compliance review recorded');
        o_result_code := 0;
        o_result_message := 'Compliance review recorded successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END record_compliance_review;

    PROCEDURE list_compliance_cases(
        p_observation_id        IN NUMBER,
        io_cursor              OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT *
            FROM tbl_compliance_case
            WHERE (p_observation_id IS NULL OR observation_id = p_observation_id)
              AND is_active = 'Y'
            ORDER BY compliance_case_id DESC;
    END list_compliance_cases;
END pkg_compliance;
/

CREATE OR REPLACE PACKAGE BODY pkg_commercial_audit IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'COMMERCIAL', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_commercial_om_id IS NULL THEN
            INSERT INTO tbl_commercial_om (
                commercial_om_id, audit_year, om_no, gist_text, body_text,
                management_response, commercial_status_id, created_by
            )
            VALUES (
                NULL, p_audit_year, UPPER(TRIM(p_om_no)), p_gist_text, p_body_text,
                p_management_response, p_commercial_status_id, p_actor_user_id
            )
            RETURNING commercial_om_id INTO io_commercial_om_id;
            log_event('TBL_COMMERCIAL_OM', io_commercial_om_id, 'COMMERCIAL_OM_CREATED', p_actor_user_id, 'OM created');
        ELSE
            UPDATE tbl_commercial_om
               SET audit_year           = p_audit_year,
                   om_no                = UPPER(TRIM(p_om_no)),
                   gist_text            = p_gist_text,
                   body_text            = p_body_text,
                   management_response  = p_management_response,
                   commercial_status_id = p_commercial_status_id,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE commercial_om_id = io_commercial_om_id;
            log_event('TBL_COMMERCIAL_OM', io_commercial_om_id, 'COMMERCIAL_OM_UPDATED', p_actor_user_id, 'OM updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'OM saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_om;

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
    ) IS
    BEGIN
        IF io_commercial_pdp_id IS NULL THEN
            INSERT INTO tbl_commercial_pdp (
                commercial_pdp_id, audit_year, pdp_no, gist_text, body_text,
                management_response, dac_recommendation, commercial_status_id, created_by
            )
            VALUES (
                NULL, p_audit_year, UPPER(TRIM(p_pdp_no)), p_gist_text, p_body_text,
                p_management_response, p_dac_recommendation, p_commercial_status_id, p_actor_user_id
            )
            RETURNING commercial_pdp_id INTO io_commercial_pdp_id;
            log_event('TBL_COMMERCIAL_PDP', io_commercial_pdp_id, 'COMMERCIAL_PDP_CREATED', p_actor_user_id, 'PDP created');
        ELSE
            UPDATE tbl_commercial_pdp
               SET audit_year           = p_audit_year,
                   pdp_no               = UPPER(TRIM(p_pdp_no)),
                   gist_text            = p_gist_text,
                   body_text            = p_body_text,
                   management_response  = p_management_response,
                   dac_recommendation   = p_dac_recommendation,
                   commercial_status_id = p_commercial_status_id,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE commercial_pdp_id = io_commercial_pdp_id;
            log_event('TBL_COMMERCIAL_PDP', io_commercial_pdp_id, 'COMMERCIAL_PDP_UPDATED', p_actor_user_id, 'PDP updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'PDP saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_pdp;

    PROCEDURE link_pdp_om(
        p_commercial_pdp_id  IN NUMBER,
        p_commercial_om_id   IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    ) IS
        l_id NUMBER;
    BEGIN
        INSERT INTO tbl_commercial_pdp_observation (
            commercial_pdp_observation_id, commercial_pdp_id, commercial_om_id, created_by
        )
        VALUES (
            NULL, p_commercial_pdp_id, p_commercial_om_id, p_actor_user_id
        )
        RETURNING commercial_pdp_observation_id INTO l_id;

        log_event('TBL_COMMERCIAL_PDP_OBSERVATION', l_id, 'COMMERCIAL_PDP_OM_LINKED', p_actor_user_id, 'PDP linked to OM');
        o_result_code := 0;
        o_result_message := 'PDP to OM link saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END link_pdp_om;

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
    ) IS
    BEGIN
        IF io_commercial_arpse_id IS NULL THEN
            INSERT INTO tbl_commercial_arpse (
                commercial_arpse_id, audit_year, para_no, gist_text,
                management_response, commercial_status_id, created_by
            )
            VALUES (
                NULL, p_audit_year, UPPER(TRIM(p_para_no)), p_gist_text,
                p_management_response, p_commercial_status_id, p_actor_user_id
            )
            RETURNING commercial_arpse_id INTO io_commercial_arpse_id;
            log_event('TBL_COMMERCIAL_ARPSE', io_commercial_arpse_id, 'COMMERCIAL_ARPSE_CREATED', p_actor_user_id, 'ARPSE created');
        ELSE
            UPDATE tbl_commercial_arpse
               SET audit_year           = p_audit_year,
                   para_no              = UPPER(TRIM(p_para_no)),
                   gist_text            = p_gist_text,
                   management_response  = p_management_response,
                   commercial_status_id = p_commercial_status_id,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE commercial_arpse_id = io_commercial_arpse_id;
            log_event('TBL_COMMERCIAL_ARPSE', io_commercial_arpse_id, 'COMMERCIAL_ARPSE_UPDATED', p_actor_user_id, 'ARPSE updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'ARPSE saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_arpse;

    PROCEDURE list_commercial_items(
        p_audit_year         IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT 'OM' AS item_type, commercial_om_id AS item_id, audit_year, om_no AS item_no, commercial_status_id
            FROM tbl_commercial_om
            WHERE (p_audit_year IS NULL OR audit_year = p_audit_year)
              AND is_active = 'Y'
            UNION ALL
            SELECT 'PDP', commercial_pdp_id, audit_year, pdp_no, commercial_status_id
            FROM tbl_commercial_pdp
            WHERE (p_audit_year IS NULL OR audit_year = p_audit_year)
              AND is_active = 'Y'
            UNION ALL
            SELECT 'ARPSE', commercial_arpse_id, audit_year, para_no, commercial_status_id
            FROM tbl_commercial_arpse
            WHERE (p_audit_year IS NULL OR audit_year = p_audit_year)
              AND is_active = 'Y';
    END list_commercial_items;
END pkg_commercial_audit;
/

CREATE OR REPLACE PACKAGE BODY pkg_report IS
    PROCEDURE save_report(
        io_report_id         IN OUT NUMBER,
        p_engagement_id      IN NUMBER,
        p_report_type_id     IN NUMBER,
        p_report_title       IN VARCHAR2,
        p_report_status_id   IN NUMBER,
        p_actor_user_id      IN NUMBER,
        o_result_code        OUT NUMBER,
        o_result_message     OUT VARCHAR2
    ) IS
    BEGIN
        IF io_report_id IS NULL THEN
            INSERT INTO tbl_report (
                report_id, engagement_id, report_type_id, report_title, report_status_id, created_by
            )
            VALUES (
                NULL, p_engagement_id, p_report_type_id, TRIM(p_report_title), p_report_status_id, p_actor_user_id
            )
            RETURNING report_id INTO io_report_id;
        ELSE
            UPDATE tbl_report
               SET report_type_id   = p_report_type_id,
                   report_title     = TRIM(p_report_title),
                   report_status_id = p_report_status_id,
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE report_id = io_report_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Report saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_report;

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
    ) IS
    BEGIN
        IF io_report_section_id IS NULL THEN
            INSERT INTO tbl_report_section (
                report_section_id, report_id, section_code, section_title, sequence_no, section_text, created_by
            )
            VALUES (
                NULL, p_report_id, UPPER(TRIM(p_section_code)), TRIM(p_section_title), NVL(p_sequence_no, 0), p_section_text, p_actor_user_id
            )
            RETURNING report_section_id INTO io_report_section_id;
        ELSE
            UPDATE tbl_report_section
               SET section_code    = UPPER(TRIM(p_section_code)),
                   section_title   = TRIM(p_section_title),
                   sequence_no     = NVL(p_sequence_no, 0),
                   section_text    = p_section_text,
                   modified_by     = p_actor_user_id,
                   modified_on     = SYSDATE,
                   record_version  = record_version + 1
             WHERE report_section_id = io_report_section_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Report section saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_report_section;

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
    ) IS
    BEGIN
        INSERT INTO tbl_report_snapshot (
            report_snapshot_id, report_id, snapshot_type_id, snapshot_key, snapshot_value, snapshot_text, created_by
        )
        VALUES (
            NULL, p_report_id, p_snapshot_type_id, UPPER(TRIM(p_snapshot_key)), p_snapshot_value, p_snapshot_text, p_actor_user_id
        )
        RETURNING report_snapshot_id INTO io_report_snapshot_id;

        o_result_code := 0;
        o_result_message := 'Report snapshot saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_report_snapshot;

    PROCEDURE get_report_summary(
        p_engagement_id      IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT
                r.report_id,
                r.report_title,
                r.report_version_no,
                r.report_status_id,
                COUNT(DISTINCT s.report_section_id) AS section_count,
                COUNT(DISTINCT p.report_snapshot_id) AS snapshot_count
            FROM tbl_report r
            LEFT JOIN tbl_report_section s
              ON s.report_id = r.report_id
             AND s.is_active = 'Y'
            LEFT JOIN tbl_report_snapshot p
              ON p.report_id = r.report_id
             AND p.is_active = 'Y'
            WHERE (p_engagement_id IS NULL OR r.engagement_id = p_engagement_id)
              AND r.is_active = 'Y'
            GROUP BY r.report_id, r.report_title, r.report_version_no, r.report_status_id
            ORDER BY r.report_id DESC;
    END get_report_summary;
END pkg_report;
/

CREATE OR REPLACE PACKAGE BODY pkg_notify IS
    PROCEDURE upsert_event(
        io_notification_event_id IN OUT NUMBER,
        p_event_code             IN VARCHAR2,
        p_event_name             IN VARCHAR2,
        p_module_code            IN VARCHAR2,
        p_description            IN VARCHAR2,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    ) IS
    BEGIN
        IF io_notification_event_id IS NULL THEN
            INSERT INTO tbl_notification_event (
                notification_event_id, event_code, event_name, module_code, description, created_by
            )
            VALUES (
                NULL, UPPER(TRIM(p_event_code)), TRIM(p_event_name), UPPER(TRIM(p_module_code)), p_description, p_actor_user_id
            )
            RETURNING notification_event_id INTO io_notification_event_id;
        ELSE
            UPDATE tbl_notification_event
               SET event_code      = UPPER(TRIM(p_event_code)),
                   event_name      = TRIM(p_event_name),
                   module_code     = UPPER(TRIM(p_module_code)),
                   description     = p_description,
                   modified_by     = p_actor_user_id,
                   modified_on     = SYSDATE,
                   record_version  = record_version + 1
             WHERE notification_event_id = io_notification_event_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Notification event saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END upsert_event;

    PROCEDURE upsert_template(
        io_notification_template_id IN OUT NUMBER,
        p_notification_event_id     IN NUMBER,
        p_culture_code              IN VARCHAR2,
        p_subject_template          IN CLOB,
        p_body_template             IN CLOB,
        p_actor_user_id             IN NUMBER,
        o_result_code               OUT NUMBER,
        o_result_message            OUT VARCHAR2
    ) IS
    BEGIN
        IF io_notification_template_id IS NULL THEN
            INSERT INTO tbl_notification_template (
                notification_template_id, notification_event_id, culture_code,
                subject_template, body_template, created_by
            )
            VALUES (
                NULL, p_notification_event_id, LOWER(TRIM(p_culture_code)),
                p_subject_template, p_body_template, p_actor_user_id
            )
            RETURNING notification_template_id INTO io_notification_template_id;
        ELSE
            UPDATE tbl_notification_template
               SET notification_event_id = p_notification_event_id,
                   culture_code          = LOWER(TRIM(p_culture_code)),
                   subject_template      = p_subject_template,
                   body_template         = p_body_template,
                   modified_by           = p_actor_user_id,
                   modified_on           = SYSDATE,
                   record_version        = record_version + 1
             WHERE notification_template_id = io_notification_template_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Notification template saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END upsert_template;

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
    ) IS
    BEGIN
        IF io_notification_rule_id IS NULL THEN
            INSERT INTO tbl_notification_rule (
                notification_rule_id, notification_event_id, recipient_type_id,
                recipient_expression, cc_expression, attachment_policy_id, created_by
            )
            VALUES (
                NULL, p_notification_event_id, p_recipient_type_id,
                p_recipient_expression, p_cc_expression, p_attachment_policy_id, p_actor_user_id
            )
            RETURNING notification_rule_id INTO io_notification_rule_id;
        ELSE
            UPDATE tbl_notification_rule
               SET notification_event_id = p_notification_event_id,
                   recipient_type_id     = p_recipient_type_id,
                   recipient_expression  = p_recipient_expression,
                   cc_expression         = p_cc_expression,
                   attachment_policy_id  = p_attachment_policy_id,
                   modified_by           = p_actor_user_id,
                   modified_on           = SYSDATE,
                   record_version        = record_version + 1
             WHERE notification_rule_id = io_notification_rule_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Notification rule saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END upsert_rule;

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
    ) IS
    BEGIN
        INSERT INTO tbl_notification_queue (
            notification_queue_id, notification_event_id, source_entity_name, source_entity_id,
            recipient_to, recipient_cc, subject_text, body_text, queue_status_id, created_by
        )
        VALUES (
            NULL, p_notification_event_id, p_source_entity_name, p_source_entity_id,
            p_recipient_to, p_recipient_cc, p_subject_text, p_body_text, p_queue_status_id, p_actor_user_id
        )
        RETURNING notification_queue_id INTO io_notification_queue_id;

        o_result_code := 0;
        o_result_message := 'Notification queued successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END queue_notification;

    PROCEDURE get_pending_queue(
        p_queue_status_id        IN NUMBER,
        io_cursor                OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT
                notification_queue_id,
                notification_event_id,
                source_entity_name,
                source_entity_id,
                recipient_to,
                recipient_cc,
                subject_text,
                body_text,
                queue_status_id,
                queued_on,
                processed_on,
                error_text
            FROM tbl_notification_queue
            WHERE (p_queue_status_id IS NULL OR queue_status_id = p_queue_status_id)
              AND is_active = 'Y'
            ORDER BY notification_queue_id DESC;
    END get_pending_queue;
END pkg_notify;
/

CREATE OR REPLACE PACKAGE BODY pkg_inquiry IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'IID', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_iid_case_id IS NULL THEN
            INSERT INTO tbl_iid_case (
                iid_case_id, case_no, intake_channel_id, complaint_source_id, complaint_type_id,
                priority_id, case_status_id, assigned_entity_id, case_summary, submitted_by, created_by
            )
            VALUES (
                NULL, UPPER(TRIM(p_case_no)), p_intake_channel_id, p_complaint_source_id, p_complaint_type_id,
                p_priority_id, p_case_status_id, p_assigned_entity_id, p_case_summary, p_actor_user_id, p_actor_user_id
            )
            RETURNING iid_case_id INTO io_iid_case_id;
            log_event('TBL_IID_CASE', io_iid_case_id, 'IID_CASE_CREATED', p_actor_user_id, 'IID case created');
        ELSE
            UPDATE tbl_iid_case
               SET case_no             = UPPER(TRIM(p_case_no)),
                   intake_channel_id   = p_intake_channel_id,
                   complaint_source_id = p_complaint_source_id,
                   complaint_type_id   = p_complaint_type_id,
                   priority_id         = p_priority_id,
                   case_status_id      = p_case_status_id,
                   assigned_entity_id  = p_assigned_entity_id,
                   case_summary        = p_case_summary,
                   modified_by         = p_actor_user_id,
                   modified_on         = SYSDATE,
                   record_version      = record_version + 1
             WHERE iid_case_id = io_iid_case_id;
            log_event('TBL_IID_CASE', io_iid_case_id, 'IID_CASE_UPDATED', p_actor_user_id, 'IID case updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'IID case saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END create_case;

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
    ) IS
    BEGIN
        INSERT INTO tbl_iid_complainant (
            iid_complainant_id, iid_case_id, complainant_name, cnic_no,
            contact_no, email_address, is_primary_complainant, created_by
        )
        VALUES (
            NULL, p_iid_case_id, TRIM(p_complainant_name), p_cnic_no,
            p_contact_no, p_email_address, NVL(p_is_primary, 'N'), p_actor_user_id
        )
        RETURNING iid_complainant_id INTO io_iid_complainant_id;

        log_event('TBL_IID_COMPLAINANT', io_iid_complainant_id, 'IID_COMPLAINANT_ADDED', p_actor_user_id, 'Complainant added');
        o_result_code := 0;
        o_result_message := 'Complainant added successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END add_complainant;

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
    ) IS
    BEGIN
        IF io_iid_investigation_plan_id IS NULL THEN
            INSERT INTO tbl_iid_investigation_plan (
                iid_investigation_plan_id, iid_case_id, plan_title, plan_details,
                risk_level_id, size_band_id, plan_status_id, start_date, end_date, created_by
            )
            VALUES (
                NULL, p_iid_case_id, TRIM(p_plan_title), p_plan_details,
                p_risk_level_id, p_size_band_id, p_plan_status_id, p_start_date, p_end_date, p_actor_user_id
            )
            RETURNING iid_investigation_plan_id INTO io_iid_investigation_plan_id;
            log_event('TBL_IID_INVESTIGATION_PLAN', io_iid_investigation_plan_id, 'IID_PLAN_CREATED', p_actor_user_id, 'Investigation plan created');
        ELSE
            UPDATE tbl_iid_investigation_plan
               SET plan_title       = TRIM(p_plan_title),
                   plan_details     = p_plan_details,
                   risk_level_id    = p_risk_level_id,
                   size_band_id     = p_size_band_id,
                   plan_status_id   = p_plan_status_id,
                   start_date       = p_start_date,
                   end_date         = p_end_date,
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE iid_investigation_plan_id = io_iid_investigation_plan_id;
            log_event('TBL_IID_INVESTIGATION_PLAN', io_iid_investigation_plan_id, 'IID_PLAN_UPDATED', p_actor_user_id, 'Investigation plan updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Investigation plan saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_investigation_plan;

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
    ) IS
    BEGIN
        IF io_iid_report_id IS NULL THEN
            INSERT INTO tbl_iid_report (
                iid_report_id, iid_case_id, report_no, report_status_id,
                executive_summary, findings_text, recommendation_text, created_by
            )
            VALUES (
                NULL, p_iid_case_id, UPPER(TRIM(p_report_no)), p_report_status_id,
                p_executive_summary, p_findings_text, p_recommendation_text, p_actor_user_id
            )
            RETURNING iid_report_id INTO io_iid_report_id;
            log_event('TBL_IID_REPORT', io_iid_report_id, 'IID_REPORT_SUBMITTED', p_actor_user_id, 'IID report submitted');
        ELSE
            UPDATE tbl_iid_report
               SET report_no            = UPPER(TRIM(p_report_no)),
                   report_status_id     = p_report_status_id,
                   executive_summary    = p_executive_summary,
                   findings_text        = p_findings_text,
                   recommendation_text  = p_recommendation_text,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE iid_report_id = io_iid_report_id;
            log_event('TBL_IID_REPORT', io_iid_report_id, 'IID_REPORT_UPDATED', p_actor_user_id, 'IID report updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'IID report saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END submit_report;

    PROCEDURE list_cases(
        p_case_status_id      IN NUMBER,
        p_assigned_entity_id  IN NUMBER,
        io_cursor             OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT *
            FROM vw_iid_case_summary
            WHERE (p_case_status_id IS NULL OR case_status_id = p_case_status_id)
              AND (p_assigned_entity_id IS NULL OR assigned_entity_id = p_assigned_entity_id)
            ORDER BY submitted_on DESC, case_no DESC;
    END list_cases;
END pkg_inquiry;
/

CREATE OR REPLACE PACKAGE BODY pkg_observation IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'OBSERVATION', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_observation_id IS NULL THEN
            INSERT INTO tbl_observation (
                observation_id, observation_no, engagement_id, entity_id, checklist_item_id,
                observation_status_id, severity_id, risk_rating_id, amount_involved, created_by
            )
            VALUES (
                NULL, UPPER(TRIM(p_observation_no)), p_engagement_id, p_entity_id, p_checklist_item_id,
                p_observation_status_id, p_severity_id, p_risk_rating_id, p_amount_involved, p_actor_user_id
            )
            RETURNING observation_id INTO io_observation_id;
            log_event('TBL_OBSERVATION', io_observation_id, 'OBSERVATION_CREATED', p_actor_user_id, 'Observation created');
        ELSE
            UPDATE tbl_observation
               SET observation_no        = UPPER(TRIM(p_observation_no)),
                   engagement_id         = p_engagement_id,
                   entity_id             = p_entity_id,
                   checklist_item_id     = p_checklist_item_id,
                   observation_status_id = p_observation_status_id,
                   severity_id           = p_severity_id,
                   risk_rating_id        = p_risk_rating_id,
                   amount_involved       = p_amount_involved,
                   modified_by           = p_actor_user_id,
                   modified_on           = SYSDATE,
                   record_version        = record_version + 1
             WHERE observation_id = io_observation_id;
            log_event('TBL_OBSERVATION', io_observation_id, 'OBSERVATION_UPDATED', p_actor_user_id, 'Observation updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Observation saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_observation;

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
    ) IS
    BEGIN
        IF io_observation_detail_id IS NULL THEN
            INSERT INTO tbl_observation_detail (
                observation_detail_id, observation_id, detail_type_id, heading_text,
                detail_text, sequence_no, created_by
            )
            VALUES (
                NULL, p_observation_id, p_detail_type_id, p_heading_text,
                p_detail_text, NVL(p_sequence_no, 0), p_actor_user_id
            )
            RETURNING observation_detail_id INTO io_observation_detail_id;
            log_event('TBL_OBSERVATION_DETAIL', io_observation_detail_id, 'OBSERVATION_DETAIL_CREATED', p_actor_user_id, 'Observation detail created');
        ELSE
            UPDATE tbl_observation_detail
               SET detail_type_id   = p_detail_type_id,
                   heading_text     = p_heading_text,
                   detail_text      = p_detail_text,
                   sequence_no      = NVL(p_sequence_no, 0),
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE observation_detail_id = io_observation_detail_id;
            log_event('TBL_OBSERVATION_DETAIL', io_observation_detail_id, 'OBSERVATION_DETAIL_UPDATED', p_actor_user_id, 'Observation detail updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Observation detail saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_observation_detail;

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
    ) IS
    BEGIN
        IF io_observation_assignment_id IS NULL THEN
            INSERT INTO tbl_observation_assignment (
                observation_assignment_id, observation_id, assignee_user_id, assignment_role_id,
                assignment_status_id, assigned_by, due_on, created_by
            )
            VALUES (
                NULL, p_observation_id, p_assignee_user_id, p_assignment_role_id,
                p_assignment_status_id, p_actor_user_id, p_due_on, p_actor_user_id
            )
            RETURNING observation_assignment_id INTO io_observation_assignment_id;
            log_event('TBL_OBSERVATION_ASSIGNMENT', io_observation_assignment_id, 'OBSERVATION_ASSIGNED', p_actor_user_id, 'Observation assigned');
        ELSE
            UPDATE tbl_observation_assignment
               SET assignee_user_id     = p_assignee_user_id,
                   assignment_role_id   = p_assignment_role_id,
                   assignment_status_id = p_assignment_status_id,
                   due_on               = p_due_on,
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE observation_assignment_id = io_observation_assignment_id;
            log_event('TBL_OBSERVATION_ASSIGNMENT', io_observation_assignment_id, 'OBSERVATION_REASSIGNED', p_actor_user_id, 'Observation assignment updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Observation assignment saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END assign_observation;

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
    ) IS
    BEGIN
        INSERT INTO tbl_observation_response (
            observation_response_id, observation_id, response_stage_id, respondent_user_id,
            response_status_id, response_text, submitted_flag, created_by
        )
        VALUES (
            NULL, p_observation_id, p_response_stage_id, p_respondent_user_id,
            p_response_status_id, p_response_text, NVL(p_submitted_flag, 'N'), p_actor_user_id
        )
        RETURNING observation_response_id INTO io_observation_response_id;

        log_event('TBL_OBSERVATION_RESPONSE', io_observation_response_id, 'OBSERVATION_RESPONSE_ADDED', p_actor_user_id, 'Observation response added');
        o_result_code := 0;
        o_result_message := 'Observation response added successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END add_observation_response;

    PROCEDURE list_observations(
        p_engagement_id      IN NUMBER,
        p_entity_id          IN NUMBER,
        io_cursor            OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT *
            FROM vw_observation_register
            WHERE (p_engagement_id IS NULL OR engagement_id = p_engagement_id)
              AND (p_entity_id IS NULL OR entity_id = p_entity_id)
            ORDER BY observation_no;
    END list_observations;
END pkg_observation;
/

CREATE OR REPLACE PACKAGE BODY pkg_master IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'MASTER', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_lookup_value_id IS NULL THEN
            INSERT INTO tbl_lookup_value (
                lookup_value_id, lookup_type_id, lookup_code, lookup_name, short_name,
                sort_order, is_default, is_active, created_by
            )
            VALUES (
                NULL, p_lookup_type_id, UPPER(TRIM(p_lookup_code)), TRIM(p_lookup_name), p_short_name,
                NVL(p_sort_order, 0), NVL(p_is_default, 'N'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING lookup_value_id INTO io_lookup_value_id;
            log_event('TBL_LOOKUP_VALUE', io_lookup_value_id, 'LOOKUP_CREATED', p_actor_user_id, 'Lookup value created');
        ELSE
            UPDATE tbl_lookup_value
               SET lookup_type_id  = p_lookup_type_id,
                   lookup_code     = UPPER(TRIM(p_lookup_code)),
                   lookup_name     = TRIM(p_lookup_name),
                   short_name      = p_short_name,
                   sort_order      = NVL(p_sort_order, 0),
                   is_default      = NVL(p_is_default, 'N'),
                   is_active       = NVL(p_is_active, 'Y'),
                   modified_by     = p_actor_user_id,
                   modified_on     = SYSDATE,
                   record_version  = record_version + 1
             WHERE lookup_value_id = io_lookup_value_id;
            log_event('TBL_LOOKUP_VALUE', io_lookup_value_id, 'LOOKUP_UPDATED', p_actor_user_id, 'Lookup value updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Lookup value saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_lookup_value;

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
    ) IS
    BEGIN
        IF io_entity_type_id IS NULL THEN
            INSERT INTO tbl_entity_type (
                entity_type_id, entity_type_code, entity_type_name, entity_group_id,
                is_auditable, is_active, created_by
            )
            VALUES (
                NULL, UPPER(TRIM(p_entity_type_code)), TRIM(p_entity_type_name), p_entity_group_id,
                NVL(p_is_auditable, 'Y'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING entity_type_id INTO io_entity_type_id;
            log_event('TBL_ENTITY_TYPE', io_entity_type_id, 'ENTITY_TYPE_CREATED', p_actor_user_id, 'Entity type created');
        ELSE
            UPDATE tbl_entity_type
               SET entity_type_code = UPPER(TRIM(p_entity_type_code)),
                   entity_type_name = TRIM(p_entity_type_name),
                   entity_group_id  = p_entity_group_id,
                   is_auditable     = NVL(p_is_auditable, 'Y'),
                   is_active        = NVL(p_is_active, 'Y'),
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE entity_type_id = io_entity_type_id;
            log_event('TBL_ENTITY_TYPE', io_entity_type_id, 'ENTITY_TYPE_UPDATED', p_actor_user_id, 'Entity type updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Entity type saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_entity_type;

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
    ) IS
    BEGIN
        IF io_entity_id IS NULL THEN
            INSERT INTO tbl_entity (
                entity_id, entity_type_id, entity_code, entity_name, parent_entity_id,
                risk_rating_id, size_band_id, entity_status_id, is_auditable, is_active, created_by
            )
            VALUES (
                NULL, p_entity_type_id, UPPER(TRIM(p_entity_code)), TRIM(p_entity_name), p_parent_entity_id,
                p_risk_rating_id, p_size_band_id, p_entity_status_id, NVL(p_is_auditable, 'Y'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING entity_id INTO io_entity_id;
            log_event('TBL_ENTITY', io_entity_id, 'ENTITY_CREATED', p_actor_user_id, 'Entity created');
        ELSE
            UPDATE tbl_entity
               SET entity_type_id   = p_entity_type_id,
                   entity_code      = UPPER(TRIM(p_entity_code)),
                   entity_name      = TRIM(p_entity_name),
                   parent_entity_id = p_parent_entity_id,
                   risk_rating_id   = p_risk_rating_id,
                   size_band_id     = p_size_band_id,
                   entity_status_id = p_entity_status_id,
                   is_auditable     = NVL(p_is_auditable, 'Y'),
                   is_active        = NVL(p_is_active, 'Y'),
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE entity_id = io_entity_id;
            log_event('TBL_ENTITY', io_entity_id, 'ENTITY_UPDATED', p_actor_user_id, 'Entity updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Entity saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_entity;

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
    ) IS
    BEGIN
        IF io_reference_document_id IS NULL THEN
            INSERT INTO tbl_reference_document (
                reference_document_id, reference_type_id, document_code, document_title,
                issue_date, effective_date, expiry_date, source_url, created_by
            )
            VALUES (
                NULL, p_reference_type_id, UPPER(TRIM(p_document_code)), TRIM(p_document_title),
                p_issue_date, p_effective_date, p_expiry_date, p_source_url, p_actor_user_id
            )
            RETURNING reference_document_id INTO io_reference_document_id;
            log_event('TBL_REFERENCE_DOCUMENT', io_reference_document_id, 'REFERENCE_CREATED', p_actor_user_id, 'Reference document created');
        ELSE
            UPDATE tbl_reference_document
               SET reference_type_id = p_reference_type_id,
                   document_code     = UPPER(TRIM(p_document_code)),
                   document_title    = TRIM(p_document_title),
                   issue_date        = p_issue_date,
                   effective_date    = p_effective_date,
                   expiry_date       = p_expiry_date,
                   source_url        = p_source_url,
                   modified_by       = p_actor_user_id,
                   modified_on       = SYSDATE,
                   record_version    = record_version + 1
             WHERE reference_document_id = io_reference_document_id;
            log_event('TBL_REFERENCE_DOCUMENT', io_reference_document_id, 'REFERENCE_UPDATED', p_actor_user_id, 'Reference document updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Reference document saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_reference_document;

    PROCEDURE list_entities(
        p_entity_type_id    IN NUMBER,
        p_parent_entity_id  IN NUMBER,
        io_cursor           OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT
                entity_id,
                entity_code,
                entity_name,
                entity_type_id,
                parent_entity_id,
                risk_rating_id,
                size_band_id,
                entity_status_id
            FROM tbl_entity
            WHERE (p_entity_type_id IS NULL OR entity_type_id = p_entity_type_id)
              AND (p_parent_entity_id IS NULL OR parent_entity_id = p_parent_entity_id)
              AND is_active = 'Y'
            ORDER BY entity_code;
    END list_entities;
END pkg_master;
/

CREATE OR REPLACE PACKAGE BODY pkg_entity IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'ENTITY', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_entity_id IS NULL THEN
            INSERT INTO tbl_entity (
                entity_id, entity_type_id, entity_code, entity_name, parent_entity_id,
                audited_by_entity_id, compliance_entity_id, cost_center_code,
                address_line, telephone_no, email_address, risk_rating_id,
                size_band_id, entity_status_id, is_auditable, is_active, created_by
            )
            VALUES (
                NULL, p_entity_type_id, UPPER(TRIM(p_entity_code)), TRIM(p_entity_name), p_parent_entity_id,
                p_audited_by_entity_id, p_compliance_entity_id, p_cost_center_code,
                p_address_line, p_telephone_no, p_email_address, p_risk_rating_id,
                p_size_band_id, p_entity_status_id, NVL(p_is_auditable, 'Y'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING entity_id INTO io_entity_id;

            log_event('TBL_ENTITY', io_entity_id, 'ENTITY_CREATED', p_actor_user_id, 'Entity created');
        ELSE
            UPDATE tbl_entity
               SET entity_type_id       = p_entity_type_id,
                   entity_code          = UPPER(TRIM(p_entity_code)),
                   entity_name          = TRIM(p_entity_name),
                   parent_entity_id     = p_parent_entity_id,
                   audited_by_entity_id = p_audited_by_entity_id,
                   compliance_entity_id = p_compliance_entity_id,
                   cost_center_code     = p_cost_center_code,
                   address_line         = p_address_line,
                   telephone_no         = p_telephone_no,
                   email_address        = p_email_address,
                   risk_rating_id       = p_risk_rating_id,
                   size_band_id         = p_size_band_id,
                   entity_status_id     = p_entity_status_id,
                   is_auditable         = NVL(p_is_auditable, 'Y'),
                   is_active            = NVL(p_is_active, 'Y'),
                   modified_by          = p_actor_user_id,
                   modified_on          = SYSDATE,
                   record_version       = record_version + 1
             WHERE entity_id = io_entity_id;

            log_event('TBL_ENTITY', io_entity_id, 'ENTITY_UPDATED', p_actor_user_id, 'Entity updated');
        END IF;

        o_result_code := 0;
        o_result_message := 'Entity saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_entity;

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
    ) IS
    BEGIN
        IF io_entity_relation_id IS NULL THEN
            INSERT INTO tbl_entity_relation (
                entity_relation_id, parent_entity_id, child_entity_id, relation_type_id,
                effective_from, effective_to, is_active, created_by
            )
            VALUES (
                NULL, p_parent_entity_id, p_child_entity_id, p_relation_type_id,
                NVL(p_effective_from, TRUNC(SYSDATE)), p_effective_to, NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING entity_relation_id INTO io_entity_relation_id;
        ELSE
            UPDATE tbl_entity_relation
               SET parent_entity_id = p_parent_entity_id,
                   child_entity_id  = p_child_entity_id,
                   relation_type_id = p_relation_type_id,
                   effective_from   = NVL(p_effective_from, effective_from),
                   effective_to     = p_effective_to,
                   is_active        = NVL(p_is_active, 'Y'),
                   modified_by      = p_actor_user_id,
                   modified_on      = SYSDATE,
                   record_version   = record_version + 1
             WHERE entity_relation_id = io_entity_relation_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Entity relation saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_entity_relation;

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
    ) IS
    BEGIN
        INSERT INTO tbl_entity_risk_profile (
            entity_risk_profile_id, entity_id, risk_rating_id, effective_from, effective_to,
            detail_text, is_current_flag, is_active, created_by
        )
        VALUES (
            NULL, p_entity_id, p_risk_rating_id, NVL(p_effective_from, TRUNC(SYSDATE)), p_effective_to,
            p_detail_text, NVL(p_is_current_flag, 'Y'), NVL(p_is_active, 'Y'), p_actor_user_id
        )
        RETURNING entity_risk_profile_id INTO io_entity_risk_profile_id;

        o_result_code := 0;
        o_result_message := 'Risk profile added successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END add_risk_profile;

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
    ) IS
    BEGIN
        INSERT INTO tbl_entity_size_profile (
            entity_size_profile_id, entity_id, size_band_id, description, effective_from,
            effective_to, is_current_flag, is_active, created_by
        )
        VALUES (
            NULL, p_entity_id, p_size_band_id, p_description, NVL(p_effective_from, TRUNC(SYSDATE)),
            p_effective_to, NVL(p_is_current_flag, 'Y'), NVL(p_is_active, 'Y'), p_actor_user_id
        )
        RETURNING entity_size_profile_id INTO io_entity_size_profile_id;

        o_result_code := 0;
        o_result_message := 'Size profile added successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END add_size_profile;

    PROCEDURE list_entity_hierarchy(
        p_parent_entity_id      IN NUMBER,
        io_cursor               OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT entity_id, entity_code, entity_name, parent_entity_id, entity_status_id, risk_rating_id, size_band_id
              FROM tbl_entity
             WHERE (p_parent_entity_id IS NULL OR parent_entity_id = p_parent_entity_id)
               AND is_active = 'Y'
             ORDER BY entity_code;
    END list_entity_hierarchy;
END pkg_entity;
/

CREATE OR REPLACE PACKAGE BODY pkg_reference IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'REFERENCE', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

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
    ) IS
    BEGIN
        IF io_reference_document_id IS NULL THEN
            INSERT INTO tbl_reference_document (
                reference_document_id, reference_type_id, document_code, document_title,
                issuing_authority, issue_date, effective_date, expiry_date, source_url,
                is_active, created_by
            )
            VALUES (
                NULL, p_reference_type_id, UPPER(TRIM(p_document_code)), TRIM(p_document_title),
                p_issuing_authority, p_issue_date, p_effective_date, p_expiry_date, p_source_url,
                NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING reference_document_id INTO io_reference_document_id;
        ELSE
            UPDATE tbl_reference_document
               SET reference_type_id = p_reference_type_id,
                   document_code     = UPPER(TRIM(p_document_code)),
                   document_title    = TRIM(p_document_title),
                   issuing_authority = p_issuing_authority,
                   issue_date        = p_issue_date,
                   effective_date    = p_effective_date,
                   expiry_date       = p_expiry_date,
                   source_url        = p_source_url,
                   is_active         = NVL(p_is_active, 'Y'),
                   modified_by       = p_actor_user_id,
                   modified_on       = SYSDATE,
                   record_version    = record_version + 1
             WHERE reference_document_id = io_reference_document_id;
        END IF;

        log_event('TBL_REFERENCE_DOCUMENT', io_reference_document_id, 'REFERENCE_SAVED', p_actor_user_id, 'Reference document saved');
        o_result_code := 0;
        o_result_message := 'Reference document saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_reference_document;

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
    ) IS
    BEGIN
        IF io_reference_document_version_id IS NULL THEN
            INSERT INTO tbl_reference_document_version (
                reference_document_version_id, reference_document_id, version_no, version_label,
                document_text, storage_reference, heading_text, section_code, chapter_no, page_no,
                is_current_version, is_active, created_by
            )
            VALUES (
                NULL, p_reference_document_id, p_version_no, p_version_label,
                p_document_text, p_storage_reference, p_heading_text, p_section_code, p_chapter_no, p_page_no,
                NVL(p_is_current_version, 'Y'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING reference_document_version_id INTO io_reference_document_version_id;
        ELSE
            UPDATE tbl_reference_document_version
               SET version_no          = p_version_no,
                   version_label       = p_version_label,
                   document_text       = p_document_text,
                   storage_reference   = p_storage_reference,
                   heading_text        = p_heading_text,
                   section_code        = p_section_code,
                   chapter_no          = p_chapter_no,
                   page_no             = p_page_no,
                   is_current_version  = NVL(p_is_current_version, 'Y'),
                   is_active           = NVL(p_is_active, 'Y'),
                   modified_by         = p_actor_user_id,
                   modified_on         = SYSDATE,
                   record_version      = record_version + 1
             WHERE reference_document_version_id = io_reference_document_version_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Reference document version saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_reference_document_version;

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
    ) IS
    BEGIN
        IF io_checklist_annexure_id IS NULL THEN
            INSERT INTO tbl_checklist_annexure (
                checklist_annexure_id, checklist_item_id, annexure_type_id, heading_text,
                detail_text, reference_document_version_id, attachment_id, effective_on,
                expiry_on, sort_order, is_active, created_by
            )
            VALUES (
                NULL, p_checklist_item_id, p_annexure_type_id, p_heading_text,
                p_detail_text, p_reference_document_version_id, p_attachment_id, p_effective_on,
                p_expiry_on, NVL(p_sort_order, 0), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING checklist_annexure_id INTO io_checklist_annexure_id;
        ELSE
            UPDATE tbl_checklist_annexure
               SET annexure_type_id             = p_annexure_type_id,
                   heading_text                 = p_heading_text,
                   detail_text                  = p_detail_text,
                   reference_document_version_id = p_reference_document_version_id,
                   attachment_id                = p_attachment_id,
                   effective_on                 = p_effective_on,
                   expiry_on                    = p_expiry_on,
                   sort_order                   = NVL(p_sort_order, 0),
                   is_active                    = NVL(p_is_active, 'Y'),
                   modified_by                  = p_actor_user_id,
                   modified_on                  = SYSDATE,
                   record_version               = record_version + 1
             WHERE checklist_annexure_id = io_checklist_annexure_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Checklist annexure saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END save_checklist_annexure;

    PROCEDURE list_manual_index(
        p_reference_document_id IN NUMBER,
        io_cursor               OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT reference_document_version_id, version_no, chapter_no, chapter_title,
                   section_code, sub_section_no, heading_text, page_no, display_order
              FROM tbl_reference_document_version
             WHERE reference_document_id = p_reference_document_id
               AND is_active = 'Y'
             ORDER BY NVL(display_order, 0), reference_document_version_id;
    END list_manual_index;
END pkg_reference;
/

CREATE OR REPLACE PACKAGE BODY pkg_document IS
    PROCEDURE log_event(
        p_source_entity_name IN VARCHAR2,
        p_source_entity_id   IN NUMBER,
        p_action_code        IN VARCHAR2,
        p_actor_user_id      IN NUMBER,
        p_event_remarks      IN VARCHAR2
    ) IS
    BEGIN
        INSERT INTO tbl_workflow_event (
            workflow_event_id, module_code, source_entity_name, source_entity_id,
            action_code, actor_user_id, event_remarks, created_by
        )
        VALUES (
            NULL, 'DOCUMENT', p_source_entity_name, p_source_entity_id,
            p_action_code, p_actor_user_id, p_event_remarks, p_actor_user_id
        );
    END log_event;

    PROCEDURE register_attachment(
        io_attachment_id         IN OUT NUMBER,
        p_storage_key            IN VARCHAR2,
        p_original_file_name     IN VARCHAR2,
        p_mime_type              IN VARCHAR2,
        p_file_size_bytes        IN NUMBER,
        p_document_type_id       IN NUMBER,
        p_legacy_file_path       IN VARCHAR2,
        p_legacy_source_table    IN VARCHAR2,
        p_legacy_source_pk_value IN VARCHAR2,
        p_uploaded_by            IN NUMBER,
        p_is_active              IN CHAR,
        p_actor_user_id          IN NUMBER,
        o_result_code            OUT NUMBER,
        o_result_message         OUT VARCHAR2
    ) IS
    BEGIN
        IF io_attachment_id IS NULL THEN
            INSERT INTO tbl_attachment (
                attachment_id, storage_key, original_file_name, mime_type, file_size_bytes,
                document_type_id, legacy_file_path, legacy_source_table, legacy_source_pk_value,
                uploaded_by, is_active, created_by
            )
            VALUES (
                NULL, p_storage_key, p_original_file_name, p_mime_type, p_file_size_bytes,
                p_document_type_id, p_legacy_file_path, p_legacy_source_table, p_legacy_source_pk_value,
                p_uploaded_by, NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING attachment_id INTO io_attachment_id;
        ELSE
            UPDATE tbl_attachment
               SET storage_key           = p_storage_key,
                   original_file_name    = p_original_file_name,
                   mime_type             = p_mime_type,
                   file_size_bytes       = p_file_size_bytes,
                   document_type_id      = p_document_type_id,
                   legacy_file_path      = p_legacy_file_path,
                   legacy_source_table   = p_legacy_source_table,
                   legacy_source_pk_value = p_legacy_source_pk_value,
                   uploaded_by           = p_uploaded_by,
                   is_active             = NVL(p_is_active, 'Y'),
                   modified_by           = p_actor_user_id,
                   modified_on           = SYSDATE,
                   record_version        = record_version + 1
             WHERE attachment_id = io_attachment_id;
        END IF;

        log_event('TBL_ATTACHMENT', io_attachment_id, 'ATTACHMENT_SAVED', p_actor_user_id, 'Attachment saved');
        o_result_code := 0;
        o_result_message := 'Attachment saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END register_attachment;

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
    ) IS
    BEGIN
        IF io_attachment_link_id IS NULL THEN
            INSERT INTO tbl_attachment_link (
                attachment_link_id, attachment_id, source_entity_name, source_entity_id,
                link_type_id, remarks, is_primary_link, is_active, created_by
            )
            VALUES (
                NULL, p_attachment_id, p_source_entity_name, p_source_entity_id,
                p_link_type_id, p_remarks, NVL(p_is_primary_link, 'N'), NVL(p_is_active, 'Y'), p_actor_user_id
            )
            RETURNING attachment_link_id INTO io_attachment_link_id;
        ELSE
            UPDATE tbl_attachment_link
               SET link_type_id      = p_link_type_id,
                   remarks           = p_remarks,
                   is_primary_link   = NVL(p_is_primary_link, 'N'),
                   is_active         = NVL(p_is_active, 'Y'),
                   modified_by       = p_actor_user_id,
                   modified_on       = SYSDATE,
                   record_version    = record_version + 1
             WHERE attachment_link_id = io_attachment_link_id;
        END IF;

        o_result_code := 0;
        o_result_message := 'Attachment link saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END link_attachment;

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
    ) IS
    BEGIN
        INSERT INTO tbl_document_migration_queue (
            document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
            source_column_name, legacy_file_path, legacy_file_name, target_table_name,
            target_pk_value, attachment_link_type_code, queue_status_code, remarks, created_by
        )
        VALUES (
            NULL, p_migration_batch_id, p_source_table_name, p_source_pk_value,
            p_source_column_name, p_legacy_file_path, p_legacy_file_name, p_target_table_name,
            p_target_pk_value, p_attachment_link_type_code, NVL(p_queue_status_code, 'QUEUED'), p_remarks, p_actor_user_id
        )
        RETURNING document_migration_queue_id INTO io_document_migration_queue_id;

        o_result_code := 0;
        o_result_message := 'Legacy document queued successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            o_result_code := SQLCODE;
            o_result_message := SUBSTR(SQLERRM, 1, 4000);
    END queue_legacy_document;

    PROCEDURE get_document_queue(
        p_migration_batch_id      IN NUMBER,
        p_queue_status_code       IN VARCHAR2,
        io_cursor                 OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN io_cursor FOR
            SELECT document_migration_queue_id, source_table_name, source_pk_value, source_column_name,
                   legacy_file_path, legacy_file_name, target_table_name, target_pk_value, queue_status_code, created_on
              FROM tbl_document_migration_queue
             WHERE (p_migration_batch_id IS NULL OR migration_batch_id = p_migration_batch_id)
               AND (p_queue_status_code IS NULL OR queue_status_code = p_queue_status_code)
               AND is_active = 'Y'
             ORDER BY created_on, document_migration_queue_id;
    END get_document_queue;
END pkg_document;
/

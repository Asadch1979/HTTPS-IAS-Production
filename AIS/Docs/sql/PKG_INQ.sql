CREATE OR REPLACE PACKAGE PKG_INQ AS

  TYPE T_CURSOR IS REF CURSOR;

  ------------------------------------------------------------------
  -- LOOKUPS (Controller dropdowns)
  ------------------------------------------------------------------
  PROCEDURE GET_RBH_LIST(P_REGION_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_GETINSPECTIONUNITS(IO_CURSOR OUT T_CURSOR);
  ------------------------------------------------------------------
  -- COMPLAINTS
  ------------------------------------------------------------------
  PROCEDURE P_CREATE_COMPLAINT_HDR(P_INTAKE_CHANNEL     IN VARCHAR2, -- 'IAID' or 'FFR'
                                   P_SUBMITTED_BY_PP_NO IN NUMBER,
                                   P_COMPLAINT_ID       OUT NUMBER,
                                   P_COMPLAINT_NO       OUT VARCHAR2);

  PROCEDURE P_SAVE_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                  P_NATURE             IN VARCHAR2,
                                  P_CATEGORY           IN VARCHAR2,
                                  P_RECEIVED_FROM      IN VARCHAR2,
                                  P_LOCATION_TYPE_ID   IN NUMBER,
                                  P_GM_OFFICE_ID       IN NUMBER,
                                  P_REGION_ID          IN NUMBER,
                                  P_BRANCH_ID          IN NUMBER,
                                  P_COMPLAINANT_NAME   IN VARCHAR2,
                                  P_CNIC               IN VARCHAR2,
                                  P_CELLULAR_NUMBER    IN VARCHAR2,
                                  P_MAILING_ADDRESS    IN VARCHAR2,
                                  P_GENDER             IN VARCHAR2,
                                  P_CONTENTS           IN CLOB,
                                  P_UPLOADED_COMPLAINT IN VARCHAR2,
                                  P_UPLOADED_FFR       IN VARCHAR2,
                                  P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                  P_ACTION_REQUIRED    IN VARCHAR2);

  PROCEDURE P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                    P_NATURE             IN VARCHAR2,
                                    P_CATEGORY           IN VARCHAR2,
                                    P_RECEIVED_FROM      IN VARCHAR2,
                                    P_LOCATION_TYPE_ID   IN NUMBER,
                                    P_GM_OFFICE_ID       IN NUMBER,
                                    P_REGION_ID          IN NUMBER,
                                    P_BRANCH_ID          IN NUMBER,
                                    P_CONTENTS           IN CLOB,
                                    P_UPLOADED_COMPLAINT IN VARCHAR2,
                                    P_UPLOADED_FFR       IN VARCHAR2,
                                    P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                    P_ACTION_REQUIRED    IN VARCHAR2);

  PROCEDURE P_ADD_COMPLAINANT(P_COMPLAINT_ID     IN NUMBER,
                              P_COMPLAINANT_NAME IN VARCHAR2,
                              P_CNIC             IN VARCHAR2,
                              P_CELLULAR_NUMBER  IN VARCHAR2,
                              P_MAILING_ADDRESS  IN VARCHAR2,
                              P_GENDER           IN VARCHAR2,
                              P_IS_PRIMARY       IN CHAR,
                              P_BY_PP_NO         IN NUMBER,
                              O_COMPLAINANT_ID   OUT NUMBER);

  PROCEDURE P_UPDATE_COMPLAINANT(P_COMPLAINANT_ID   IN NUMBER,
                                 P_COMPLAINANT_NAME IN VARCHAR2,
                                 P_CNIC             IN VARCHAR2,
                                 P_CELLULAR_NUMBER  IN VARCHAR2,
                                 P_MAILING_ADDRESS  IN VARCHAR2,
                                 P_GENDER           IN VARCHAR2,
                                 P_IS_PRIMARY       IN CHAR,
                                 P_BY_PP_NO         IN NUMBER);

  PROCEDURE P_DELETE_COMPLAINANT(P_COMPLAINANT_ID IN NUMBER,
                                 P_BY_PP_NO       IN NUMBER);

  PROCEDURE P_GET_COMPLAINANTS_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                            IO_CURSOR      OUT T_CURSOR);

  PROCEDURE P_SET_PRIMARY_COMPLAINANT(P_COMPLAINT_ID   IN NUMBER,
                                      P_COMPLAINANT_ID IN NUMBER,
                                      P_BY_PP_NO       IN NUMBER);

  PROCEDURE ADD_COMPLAINT(P_NATURE             IN VARCHAR2,
                          P_CATEGORY           IN VARCHAR2,
                          P_SOURCE             IN VARCHAR2,
                          P_SOURCE_OTHER_TEXT  IN VARCHAR2,
                          P_PERTAINS_TO        IN VARCHAR2,
                          P_FIELD_TYPE         IN VARCHAR2,
                          P_HO_UNIT_TYPE_ID    IN NUMBER,
                          P_HO_UNIT_ID         IN NUMBER,
                          P_REGION_ID          IN NUMBER,
                          P_BRANCH_ID          IN NUMBER,
                          P_CONTENTS           IN CLOB,
                          P_UPLOADED_COMPLAINT IN VARCHAR2,
                          P_UPLOADED_FFR       IN VARCHAR2,
                          P_UPLOADED_EVIDENCE  IN VARCHAR2,
                          P_ACTION_REQUIRED    IN VARCHAR2,
                          P_SUBMITTED_BY       IN NUMBER,
                          O_COMPLAINT_ID       OUT NUMBER);

  PROCEDURE P_GET_COMPLAINT_HDR(P_COMPLAINT_ID IN NUMBER,
                                T_CURSOR       OUT T_CURSOR);

  PROCEDURE P_GET_COMPLAINT_IAID(P_COMPLAINT_ID IN NUMBER,
                                 T_CURSOR       OUT T_CURSOR);

  PROCEDURE GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT(p_complaint_id IN NUMBER,
                                                   io_cursor      OUT SYS_REFCURSOR);

  PROCEDURE GET_COMPLAINTS(P_USER_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_COMPLAINTS_WITHOUT_ASSESSMENT(T_CURSOR OUT T_CURSOR);

  PROCEDURE GET_COMPLAINTS_DD(P_PAGE_ID in number, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_COMPLAINT(P_COMPLAINT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_LATEST_PLAN_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                         IO_CURSOR      OUT T_CURSOR);

  PROCEDURE P_GET_COMPLAINT_LIST(P_INTAKE_CHANNEL IN VARCHAR2 DEFAULT NULL, -- NULL = all
                                 P_STATUS         IN VARCHAR2 DEFAULT NULL,
                                 P_FROM_DATE      IN DATE DEFAULT NULL,
                                 P_TO_DATE        IN DATE DEFAULT NULL,
                                 T_CURSOR         OUT T_CURSOR);

  PROCEDURE P_GET_COMPLAINT_ID_BY_PLAN(P_PLAN_ID      IN NUMBER,
                                       O_COMPLAINT_ID OUT NUMBER);

  PROCEDURE P_GET_COMPLAINT_ID_BY_REPORT(P_REPORT_ID    IN NUMBER,
                                         O_COMPLAINT_ID OUT NUMBER);

  PROCEDURE P_SAVE_INQ_FINDINGS_REC(P_COMPLAINT_ID   IN NUMBER,
                                    P_FINDINGS       IN CLOB,
                                    P_RECOMMENDATION IN CLOB,
                                    P_UPDATED_BY     IN NUMBER,
                                    IO_CURSOR        OUT T_CURSOR);

  ------------------------------------------------------------------
  -- INITIAL ASSESSMENT
  ------------------------------------------------------------------
  PROCEDURE ADD_ASSESSMENT(P_COMPLAINT_ID     IN NUMBER,
                           P_RECEIVED_BY      IN NUMBER,
                           P_ASSESSMENT       IN CLOB,
                           P_RECOMMENDATION   IN VARCHAR2,
                           P_ASSIGNED_UNIT_ID IN NUMBER,
                           O_ASSESSMENT_ID    OUT NUMBER);

  ------------------------------------------------------------------
  -- HEAD REVIEW
  ------------------------------------------------------------------
  PROCEDURE ADD_HEAD_REVIEW(P_COMPLAINT_ID           IN NUMBER,
                            P_ASSESSMENT_ID          IN NUMBER,
                            P_REVIEWED_BY            IN NUMBER,
                            P_DIRECTIONS             IN CLOB,
                            P_ASSIGNED_TO_UNIT       IN NUMBER,
                            P_TEAM_LEAD              IN NUMBER,
                            P_TEAM_MEMBERS           IN CLOB,
                            P_ASSIGNED_ON            IN VARCHAR2,
                            P_DUE_DATE               IN VARCHAR2,
                            P_REFERRED_BACK_COMMENTS IN CLOB,
                            P_ACTION                 IN VARCHAR2,
                            O_REVIEW_ID              OUT NUMBER);

  ------------------------------------------------------------------
  -- INVESTIGATION PLAN
  ------------------------------------------------------------------
  PROCEDURE ADD_INV_PLAN(P_COMPLAINT_ID    IN NUMBER,
                         P_PLAN_DETAILS    IN CLOB,
                         P_SUBMITTED_BY    IN NUMBER,
                         P_STATUS          IN VARCHAR2,
                         P_INV_RISK        IN VARCHAR2,
                         P_INV_SIZE        IN VARCHAR2,
                         P_NO_OF_DAYS      IN NUMBER,
                         P_TRAVELLING_DAYS IN NUMBER,
                         P_TEAM_LEAD       IN VARCHAR2,
                         P_TEAM_MEMBERS    IN VARCHAR2,
                         P_START_DATE      IN DATE,
                         P_ACTIVITIES_TEXT IN VARCHAR2, -- if you added ACTIVITIES_TEXT column
                         O_PLAN_ID         OUT NUMBER);

  PROCEDURE GET_INV_PLAN(p_complaint_id IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_IID_TASK_LIST(P_UNIT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  ------------------------------------------------------------------
  -- PLAN APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_PLAN_APPROVAL(P_PLAN_ID         IN NUMBER,
                              P_APPROVED_BY     IN NUMBER,
                              P_IS_APPROVED     IN VARCHAR2,
                              P_EDITED_PLAN     IN CLOB,
                              P_FURTHER_ACTIONS IN CLOB,
                              O_APPROVAL_ID     OUT NUMBER);

  ------------------------------------------------------------------
  -- INQUIRY REPORT
  ------------------------------------------------------------------
  PROCEDURE ADD_INQUIRY_REPORT(P_COMPLAINT_ID                  IN NUMBER,
                               P_NAME_COMPLAINANT              IN VARCHAR2,
                               P_NAME_ACCUSED                  IN VARCHAR2,
                               P_GIST                          IN CLOB,
                               P_PROCEEDINGS                   IN CLOB,
                               P_FINDINGS                      IN CLOB,
                               P_RECOMMENDATION                IN CLOB,
                               P_CONCLUSION                    IN CLOB,
                               P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                               P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                               P_UPLOADED_REPORT               IN VARCHAR2,
                               P_UPLOADED_EVIDENCE             IN VARCHAR2,
                               P_UPLOADED_DSA                  IN VARCHAR2,
                               P_SUBMITTED_ON                  IN DATE,
                               P_SUBMITTED_BY                  IN NUMBER,
                               O_REPORT_ID                     OUT NUMBER);

  PROCEDURE GET_INQUIRY_REPORT(P_REPORT_ID IN NUMBER,
                               IO_CURSOR   OUT T_CURSOR);

  ------------------------------------------------------------------
  -- ANALYSIS
  ------------------------------------------------------------------
  PROCEDURE ADD_ANALYSIS(P_REPORT_ID             IN NUMBER,
                         P_POLICY_GAPS           IN CLOB,
                         P_CONTROL_GAPS          IN CLOB,
                         P_PROCEDURAL_VIOLATIONS IN CLOB,
                         P_FORWARD_TO            IN VARCHAR2,
                         P_COMMENTS              IN CLOB,
                         P_DECISION              IN VARCHAR2,
                         P_REFER_BACK_COMMENTS   IN CLOB,
                         P_ANALYZED_BY           IN NUMBER,
                         O_ANALYSIS_ID           OUT NUMBER);

  ------------------------------------------------------------------
  -- FINAL APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_FINAL_APPROVAL(P_REPORT_ID         IN NUMBER,
                               P_COMMENTS          IN CLOB,
                               P_APPROVED          IN VARCHAR2,
                               P_APPROVED_BY       IN NUMBER,
                               O_FINAL_APPROVAL_ID OUT NUMBER);

  ------------------------------------------------------------------
  -- CASE STUDY
  ------------------------------------------------------------------
  PROCEDURE ADD_CASE_STUDY(P_COMPLAINT_ID           IN NUMBER,
                           P_ORIGIN_PROCESS_OWNER   IN VARCHAR2,
                           P_NAME_COMPLAINANT       IN VARCHAR2,
                           P_BRANCH                 IN VARCHAR2,
                           P_GIST                   IN CLOB,
                           P_OUTCOME                IN CLOB,
                           P_MODUS_OPERANDI         IN CLOB,
                           P_GAPS                   IN CLOB,
                           P_ROOT_CAUSE             IN CLOB,
                           P_ACTIONS_REC            IN CLOB,
                           P_STATUS                 IN VARCHAR2,
                           P_POLICY_GAPS_IDENTIFIED IN CLOB,
                           P_CONTROL_VIOLATIONS     IN CLOB,
                           P_RISK_IDENTIFIED        IN CLOB,
                           P_REG_COMPLIANCE_FAILURE IN CLOB,
                           O_CASE_ID                OUT NUMBER);

  ------------------------------------------------------------------
  -- REPORTS FILTERING
  ------------------------------------------------------------------
  PROCEDURE GET_REPORTS(P_FILTER          IN VARCHAR2,
                        P_SOURCE          IN VARCHAR2,
                        P_CATEGORY        IN VARCHAR2,
                        P_PERTAINS_TO     IN VARCHAR2,
                        P_DATE_FROM       IN VARCHAR2,
                        P_DATE_TO         IN VARCHAR2,
                        P_REGION_ID       IN NUMBER,
                        P_BRANCH_ID       IN NUMBER,
                        P_HO_UNIT_TYPE_ID IN NUMBER,
                        P_HO_UNIT_ID      IN NUMBER,
                        P_STATUS          IN VARCHAR2,
                        IO_CURSOR         OUT T_CURSOR);

  ---------------------------------------------------------------------
  -- Common result cursor (OK/MESSAGE/ID)
  ----------------------------------------------------------------------
  PROCEDURE P_RESULT_OK(io_cursor OUT t_cursor,
                        p_message IN VARCHAR2,
                        p_id      IN NUMBER DEFAULT NULL);
  PROCEDURE P_RESULT_FAIL(io_cursor OUT t_cursor, p_message IN VARCHAR2);

  ----------------------------------------------------------------------
  -- ACCUSATIONS
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSATIONS(p_complaint_id IN NUMBER,
                                  io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_ACCUSATION(p_complaint_id    IN NUMBER,
                                 p_accusation_text IN CLOB,
                                 p_sort_order      IN NUMBER,
                                 p_created_by      IN NUMBER,
                                 io_cursor         OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_ACCUSATION(p_accusation_id   IN NUMBER,
                                    p_accusation_text IN CLOB,
                                    p_sort_order      IN NUMBER,
                                    p_updated_by      IN NUMBER,
                                    io_cursor         OUT t_cursor);
  PROCEDURE P_DELETE_INQ_ACCUSATION(p_accusation_id IN NUMBER,
                                    p_updated_by    IN NUMBER,
                                    io_cursor       OUT t_cursor);

  ----------------------------------------------------------------------
  -- ACCUSED LIST
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSED_LIST(p_complaint_id IN NUMBER,
                                   io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_ACCUSED(p_complaint_id IN NUMBER,
                              p_person_name  IN VARCHAR2,
                              p_designation  IN VARCHAR2,
                              p_role_type    IN VARCHAR2, -- MAIN/CO
                              p_ppno_number  IN VARCHAR2,
                              p_cnic         IN VARCHAR2,
                              p_Father_name  IN VARCHAR2,
                              p_remarks      IN VARCHAR2,
                              p_sort_order   IN NUMBER,
                              p_created_by   IN NUMBER,
                              io_cursor      OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_person_name    IN VARCHAR2,
                                 p_designation    IN VARCHAR2,
                                 p_role_type      IN VARCHAR2,
                                 p_ppno_number    IN VARCHAR2,
                                 p_cnic           IN VARCHAR2,
                                 p_FATHER_NAME    IN VARCHAR2,
                                 p_remarks        IN VARCHAR2,
                                 p_sort_order     IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor);
  PROCEDURE P_DELETE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor);

  ----------------------------------------------------------------------
  -- RECORD SCRUTINIZED
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_RECORDS(p_complaint_id IN NUMBER,
                              io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_RECORD(p_complaint_id   IN NUMBER,
                             p_record_title   IN VARCHAR2,
                             p_record_details IN VARCHAR2,
                             p_sort_order     IN NUMBER,
                             p_created_by     IN NUMBER,
                             io_cursor        OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_RECORD(p_rec_id         IN NUMBER,
                                p_record_title   IN VARCHAR2,
                                p_record_details IN VARCHAR2,
                                p_sort_order     IN NUMBER,
                                p_updated_by     IN NUMBER,
                                io_cursor        OUT t_cursor);
  PROCEDURE P_DELETE_INQ_RECORD(p_rec_id     IN NUMBER,
                                p_updated_by IN NUMBER,
                                io_cursor    OUT t_cursor);

  ----------------------------------------------------------------------
  -- STATEMENTS REGISTER
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_STATEMENTS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_STATEMENT(p_complaint_id       IN NUMBER,
                                p_person_name        IN VARCHAR2,
                                p_role_type          IN VARCHAR2, -- COMPLAINANT/ACCUSED/WITNESS/OTHER
                                p_ppno_number        IN VARCHAR2, -- UI defaults complainant CNIC here
                                p_cnic               IN VARCHAR2,
                                p_statement_datetime IN DATE,
                                p_place              IN VARCHAR2,
                                p_mode_type          IN VARCHAR2,
                                p_key_points         IN CLOB,
                                P_UPLOADED_STATEMENT in Clob,
                                P_USER_ID            IN NUMBER,
                                io_cursor            OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_STATEMENT(p_statement_id       IN NUMBER,
                                   p_person_name        IN VARCHAR2,
                                   p_role_type          IN VARCHAR2,
                                   p_ppno_number        IN VARCHAR2,
                                   p_cnic               IN VARCHAR2,
                                   p_statement_datetime IN DATE,
                                   p_place              IN VARCHAR2,
                                   p_mode_type          IN VARCHAR2,
                                   p_key_points         IN CLOB,
                                   P_UPLOADED_STATEMENT in Clob,
                                   p_updated_by         IN NUMBER,
                                   io_cursor            OUT t_cursor);
  PROCEDURE P_DELETE_INQ_STATEMENT(p_statement_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor);

  ----------------------------------------------------------------------
  -- EVIDENCE FILES
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_EVIDENCE_FILES(p_complaint_id IN NUMBER,
                                     io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_EVIDENCE_FILE(p_complaint_id  IN NUMBER,
                                    p_evidence_type IN VARCHAR2, -- MATERIAL/CIRCUMSTANTIAL/OTHER
                                    p_description   IN VARCHAR2,
                                    p_file_name     IN VARCHAR2,
                                    p_file_path     IN VARCHAR2,
                                    p_file_ext      IN VARCHAR2,
                                    p_file_size_kb  IN NUMBER,
                                    p_uploaded_by   IN NUMBER,
                                    io_cursor       OUT t_cursor);
  PROCEDURE P_DELETE_INQ_EVIDENCE_FILE(p_evidence_id IN NUMBER,
                                       p_updated_by  IN NUMBER,
                                       io_cursor     OUT t_cursor);

  ----------------------------------------------------------------------
  -- VIOLATIONS (Annex-III)
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_VIOLATIONS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor);
  PROCEDURE P_GET_INQ_VIOLATION_STEP(P_COMPLAINT_ID IN NUMBER,
                                     IO_CURSOR      OUT T_CURSOR);
  PROCEDURE P_SAVE_INQ_VIOLATION_STEP(P_COMPLAINT_ID                  IN NUMBER,
                                      P_CONCLUSION                    IN CLOB,
                                      P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                                      P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                                      P_UPDATED_BY                    IN NUMBER,
                                      IO_CURSOR                       OUT T_CURSOR);
  PROCEDURE P_ADD_INQ_VIOLATION(p_complaint_id     IN NUMBER,
                                p_category         IN VARCHAR2, -- INTERNAL/POLICY_GAP/CONTROL
                                p_violation_detail IN CLOB,
                                p_reference_text   IN VARCHAR2,
                                p_recommendation   IN CLOB,
                                p_sort_order       IN NUMBER,
                                p_created_by       IN NUMBER,
                                io_cursor          OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_VIOLATION(p_violation_id     IN NUMBER,
                                   p_category         IN VARCHAR2,
                                   p_violation_detail IN CLOB,
                                   p_reference_text   IN VARCHAR2,
                                   p_recommendation   IN CLOB,
                                   p_sort_order       IN NUMBER,
                                   p_updated_by       IN NUMBER,
                                   io_cursor          OUT t_cursor);

  PROCEDURE P_DELETE_INQ_VIOLATION(p_violation_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor);
  PROCEDURE GET_EMPLOYEE_INFO(P_PP_NO in number, io_cursor OUT t_cursor);

  PROCEDURE GET_INQ_FIND_RECOMM_STATUS(p_complaint_id IN NUMBER,
                                       io_cursor      OUT t_cursor);
  ----------------------------------------------------------------------
  -- DSA LIST
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_DSA(p_complaint_id IN NUMBER, io_cursor OUT t_cursor);
  PROCEDURE P_ADD_INQ_DSA(p_complaint_id IN NUMBER,
                          p_person_name  IN VARCHAR2,
                          p_designation  IN VARCHAR2,
                          p_ppno_number  IN VARCHAR2,
                          p_cnic         IN VARCHAR2,
                          p_dsa_status   IN VARCHAR2, -- DRAFT/ISSUED/SERVED/CLOSED
                          p_remarks      IN VARCHAR2,
                          p_sort_order   IN NUMBER,
                          p_created_by   IN NUMBER,
                          io_cursor      OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_DSA(p_dsa_id      IN NUMBER,
                             p_person_name IN VARCHAR2,
                             p_designation IN VARCHAR2,
                             p_ppno_number IN VARCHAR2,
                             p_cnic        IN VARCHAR2,
                             p_dsa_status  IN VARCHAR2,
                             p_remarks     IN VARCHAR2,
                             p_sort_order  IN NUMBER,
                             p_updated_by  IN NUMBER,
                             io_cursor     OUT t_cursor);
  PROCEDURE P_DELETE_INQ_DSA(p_dsa_id     IN NUMBER,
                             p_updated_by IN NUMBER,
                             io_cursor    OUT t_cursor);

  PROCEDURE SAVE_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                     p_accusation_id IN NUMBER, -- 0 = Additional Charges
                                     p_finding_text  IN CLOB,
                                     p_recom_text    IN CLOB,
                                     p_outcome       in varchar2,
                                     p_ppno          IN VARCHAR2,
                                     io_cursor       OUT t_cursor);
  PROCEDURE GET_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                    p_accusation_id IN NUMBER,
                                    io_cursor       OUT t_cursor);

  PROCEDURE P_GET_INQ_EVIDENCE_STEP(P_COMPLAINT_ID IN NUMBER,
                                    IO_CURSOR      OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_INQ_EVIDENCE_STEP(P_COMPLAINT_ID                   IN NUMBER,
                                     P_MATERIAL_EVIDENCE_DETAIL       IN CLOB,
                                     P_CIRCUMSTANTIAL_EVIDENCE_DETAIL IN CLOB,
                                     io_cursor                        OUT t_cursor);

  PROCEDURE P_GET_INQ_PROCEEDINGS(P_COMPLAINT_ID IN NUMBER,
                                  IO_CURSOR      OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_INQ_PROCEEDING(P_PROCEEDING_ID               IN OUT NUMBER,
                                  P_COMPLAINT_ID                IN NUMBER,
                                  P_NOTICE_REFERENCE            IN VARCHAR2,
                                  P_VISIT_DATE                  IN DATE,
                                  P_PLACE_VISITED               IN CLOB,
                                  P_PARTICIPANTS_DETAIL         IN CLOB,
                                  P_MISSING_PARTICIPANTS_REASON IN CLOB,
                                  P_SORT_ORDER                  IN NUMBER,
                                  P_STATUS                      IN VARCHAR2,
                                  P_USER_ID                     IN NUMBER,
                                  io_cursor                     OUT t_cursor);

  PROCEDURE P_DELETE_INQ_PROCEEDING(P_PROCEEDING_ID IN NUMBER,
                                    P_UPDATED_BY    IN NUMBER,
                                    io_cursor       OUT t_cursor);

  PROCEDURE P_FINALIZE_IID_INQUIRY_REPORT(P_COMPLAINT_ID IN NUMBER,
                                          P_UPDATED_BY   IN NUMBER);

  PROCEDURE P_ENQUEUE_EMAIL(P_EVENT_CODE IN VARCHAR2,
                            P_REF_ID1    IN NUMBER,
                            P_REF_ID2    IN NUMBER,
                            P_MAIL_TO    IN VARCHAR2,
                            P_MAIL_CC    IN VARCHAR2,
                            P_SUBJECT    IN VARCHAR2,
                            P_BODY       IN CLOB,
                            O_EMAIL_ID   OUT NUMBER);

  PROCEDURE P_GET_EMAIL_QUEUE(P_STATUS    IN DATE,
                              P_FROM_DATE IN DATE,
                              P_TO_DATE   IN DATE,
                              IO_CURSOR   OUT T_CURSOR);

  PROCEDURE P_MARK_EMAIL_SENT(P_EMAIL_ID IN NUMBER);

  PROCEDURE P_MARK_EMAIL_FAILED(P_EMAIL_ID   IN NUMBER,
                                P_ERROR_TEXT IN VARCHAR2);

END PKG_INQ;

CREATE OR REPLACE PACKAGE BODY PKG_INQ AS

  ------------------------------------------------------------------
  -- Status constants (must align with UI)
  ------------------------------------------------------------------
  C_STATUS_SUBMITTED      CONSTANT VARCHAR2(50) := 'Submitted';
  C_STATUS_IN_ASSESS      CONSTANT VARCHAR2(50) := 'Initial Assessment';
  C_STATUS_HEAD_REVIEW    CONSTANT VARCHAR2(50) := 'Head Review';
  C_STATUS_PLAN_DRAFTED   CONSTANT VARCHAR2(50) := 'Plan Drafted';
  C_STATUS_PLAN_APPROVED  CONSTANT VARCHAR2(50) := 'Plan Approved';
  C_STATUS_REPORT_DRAFT   CONSTANT VARCHAR2(50) := 'Inquiry Report Drafted';
  C_STATUS_UNDER_REVIEW   CONSTANT VARCHAR2(50) := 'Under Review';
  C_STATUS_FINAL_APPROVAL CONSTANT VARCHAR2(50) := 'Final Approval';
  C_STATUS_CLOSED         CONSTANT VARCHAR2(50) := 'Closed/Issued';

  ------------------------------------------------------------------
  -- Helpers
  ------------------------------------------------------------------
  FUNCTION F_GEN_COMPLAINT_NO(P_COMPLAINT_ID IN NUMBER) RETURN VARCHAR2 IS
  BEGIN
    RETURN 'IID-' || TO_CHAR(P_COMPLAINT_ID, 'FM000000');
  END F_GEN_COMPLAINT_NO;

  PROCEDURE SET_CASE_STATUS(P_COMPLAINT_ID IN NUMBER,
                            P_STATUS       IN VARCHAR2,
                            P_UPDATED_BY   IN NUMBER DEFAULT NULL) IS
  BEGIN
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET STATUS           = P_STATUS,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = NVL(P_UPDATED_BY, UPDATED_BY_PP_NO)
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  END SET_CASE_STATUS;

  ------------------------------------------------------------------
  -- LOOKUPS
  ------------------------------------------------------------------
  PROCEDURE GET_RBH_LIST(P_REGION_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT DISTINCT e.CODE, e.NAME
        FROM T_AUDITEE_ENTITIES e
       WHERE e.TYPE_ID = 5
       ORDER BY e.NAME;
  END GET_RBH_LIST;

  PROCEDURE P_GETINSPECTIONUNITS(IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT e.ENTITY_ID   AS I_ID,
             e.CODE        AS I_CODE,
             e.NAME        AS UNIT_NAME,
             e.DESCRIPTION AS DISCRIPTION,
             e.ACTIVE      AS STATUS,
             e.ACTIVE      AS ISACTIVE
        FROM T_AUDITEE_ENTITIES e
       WHERE UPPER(e.NAME) LIKE '%INQ%'
         AND e.ACTIVE = 'Y'
       ORDER BY e.ENTITY_ID;
  END P_GETINSPECTIONUNITS;

  ------------------------------------------------------------------
  -- COMPLAINT HEADER CREATE
  ------------------------------------------------------------------
  PROCEDURE P_CREATE_COMPLAINT_HDR(P_INTAKE_CHANNEL     IN VARCHAR2,
                                   P_SUBMITTED_BY_PP_NO IN NUMBER,
                                   P_COMPLAINT_ID       OUT NUMBER,
                                   P_COMPLAINT_NO       OUT VARCHAR2) IS
    V_COMPLAINT_NO VARCHAR2(50);
  BEGIN
    INSERT INTO T_AU_IID_COMPLAINT_HDR
      (COMPLAINT_NO,
       INTAKE_CHANNEL,
       STATUS,
       SUBMITTED_ON,
       SUBMITTED_BY_PP_NO,
       ASSIGNED_UNIT_ID,
       ACTIVE_FLAG,
       STATUS_ID)
    VALUES
      (NULL,
       UPPER(TRIM(P_INTAKE_CHANNEL)),
       'SUBMITTED',
       SYSDATE,
       P_SUBMITTED_BY_PP_NO,
       0,
       'Y',
       346)
    RETURNING COMPLAINT_ID INTO P_COMPLAINT_ID;
  
    V_COMPLAINT_NO := F_GEN_COMPLAINT_NO(P_COMPLAINT_ID);
  
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET COMPLAINT_NO = V_COMPLAINT_NO
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  
    P_COMPLAINT_NO := V_COMPLAINT_NO;
  END P_CREATE_COMPLAINT_HDR;

  ------------------------------------------------------------------
  -- SAVE IAID DETAILS (UPSERT)
  ------------------------------------------------------------------
  PROCEDURE P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                    P_NATURE             IN VARCHAR2,
                                    P_CATEGORY           IN VARCHAR2,
                                    P_RECEIVED_FROM      IN VARCHAR2,
                                    P_LOCATION_TYPE_ID   IN NUMBER,
                                    P_GM_OFFICE_ID       IN NUMBER,
                                    P_REGION_ID          IN NUMBER,
                                    P_BRANCH_ID          IN NUMBER,
                                    P_CONTENTS           IN CLOB,
                                    P_UPLOADED_COMPLAINT IN VARCHAR2,
                                    P_UPLOADED_FFR       IN VARCHAR2,
                                    P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                    P_ACTION_REQUIRED    IN VARCHAR2) IS
  BEGIN
    MERGE INTO T_AU_IID_COMPLAINT_IAID t
    USING (SELECT P_COMPLAINT_ID AS COMPLAINT_ID FROM DUAL) s
    ON (t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.NATURE             = P_NATURE,
             t.CATEGORY           = P_CATEGORY,
             t.RECEIVED_FROM      = P_RECEIVED_FROM,
             t.LOCATION_TYPE_ID   = P_LOCATION_TYPE_ID,
             t.GM_OFFICE_ID       = P_GM_OFFICE_ID,
             t.REGION_ID          = P_REGION_ID,
             t.BRANCH_ID          = P_BRANCH_ID,
             t.CONTENTS           = P_CONTENTS,
             t.UPLOADED_COMPLAINT = P_UPLOADED_COMPLAINT,
             t.UPLOADED_FFR       = P_UPLOADED_FFR,
             t.UPLOADED_EVIDENCE  = P_UPLOADED_EVIDENCE,
             t.ACTION_REQUIRED    = P_ACTION_REQUIRED
    WHEN NOT MATCHED THEN
      INSERT
        (COMPLAINT_ID,
         NATURE,
         CATEGORY,
         RECEIVED_FROM,
         LOCATION_TYPE_ID,
         GM_OFFICE_ID,
         REGION_ID,
         BRANCH_ID,
         CONTENTS,
         UPLOADED_COMPLAINT,
         UPLOADED_FFR,
         UPLOADED_EVIDENCE,
         ACTION_REQUIRED)
      VALUES
        (P_COMPLAINT_ID,
         P_NATURE,
         P_CATEGORY,
         P_RECEIVED_FROM,
         P_LOCATION_TYPE_ID,
         P_GM_OFFICE_ID,
         P_REGION_ID,
         P_BRANCH_ID,
         P_CONTENTS,
         P_UPLOADED_COMPLAINT,
         P_UPLOADED_FFR,
         P_UPLOADED_EVIDENCE,
         P_ACTION_REQUIRED);
  
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET UPDATED_ON = SYSDATE
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  END P_UPSERT_COMPLAINT_IAID;

  PROCEDURE P_ADD_COMPLAINANT(P_COMPLAINT_ID     IN NUMBER,
                              P_COMPLAINANT_NAME IN VARCHAR2,
                              P_CNIC             IN VARCHAR2,
                              P_CELLULAR_NUMBER  IN VARCHAR2,
                              P_MAILING_ADDRESS  IN VARCHAR2,
                              P_GENDER           IN VARCHAR2,
                              P_IS_PRIMARY       IN CHAR,
                              P_BY_PP_NO         IN NUMBER,
                              O_COMPLAINANT_ID   OUT NUMBER) IS
    L_IS_PRIMARY CHAR(1) := NVL(UPPER(TRIM(P_IS_PRIMARY)), 'N');
  BEGIN
    INSERT INTO T_AU_IID_COMPLAINANT
      (COMPLAINT_ID,
       COMPLAINANT_NAME,
       CNIC,
       CELLULAR_NUMBER,
       MAILING_ADDRESS,
       GENDER,
       IS_PRIMARY,
       ACTIVE_FLAG,
       CREATED_ON,
       CREATED_BY_PP_NO)
    VALUES
      (P_COMPLAINT_ID,
       P_COMPLAINANT_NAME,
       P_CNIC,
       P_CELLULAR_NUMBER,
       P_MAILING_ADDRESS,
       P_GENDER,
       CASE WHEN L_IS_PRIMARY = 'Y' THEN 'Y' ELSE 'N' END,
       'Y',
       SYSDATE,
       P_BY_PP_NO)
    RETURNING COMPLAINANT_ID INTO O_COMPLAINANT_ID;
  
    IF L_IS_PRIMARY = 'Y' THEN
      UPDATE T_AU_IID_COMPLAINANT
         SET IS_PRIMARY       = 'N',
             UPDATED_ON       = SYSDATE,
             UPDATED_BY_PP_NO = P_BY_PP_NO
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
         AND COMPLAINANT_ID <> O_COMPLAINANT_ID
         AND IS_PRIMARY = 'Y';
    END IF;
  END P_ADD_COMPLAINANT;

  PROCEDURE P_UPDATE_COMPLAINANT(P_COMPLAINANT_ID   IN NUMBER,
                                 P_COMPLAINANT_NAME IN VARCHAR2,
                                 P_CNIC             IN VARCHAR2,
                                 P_CELLULAR_NUMBER  IN VARCHAR2,
                                 P_MAILING_ADDRESS  IN VARCHAR2,
                                 P_GENDER           IN VARCHAR2,
                                 P_IS_PRIMARY       IN CHAR,
                                 P_BY_PP_NO         IN NUMBER) IS
    L_COMPLAINT_ID NUMBER;
    L_IS_PRIMARY   CHAR(1) := NVL(UPPER(TRIM(P_IS_PRIMARY)), 'N');
  BEGIN
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_COMPLAINANT
     WHERE COMPLAINANT_ID = P_COMPLAINANT_ID;
  
    UPDATE T_AU_IID_COMPLAINANT
       SET COMPLAINANT_NAME = P_COMPLAINANT_NAME,
           CNIC             = P_CNIC,
           CELLULAR_NUMBER  = P_CELLULAR_NUMBER,
           MAILING_ADDRESS  = P_MAILING_ADDRESS,
           GENDER           = P_GENDER,
           IS_PRIMARY = CASE
                          WHEN L_IS_PRIMARY = 'Y' THEN
                           'Y'
                          ELSE
                           'N'
                        END,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINANT_ID = P_COMPLAINANT_ID;
  
    IF L_IS_PRIMARY = 'Y' THEN
      UPDATE T_AU_IID_COMPLAINANT
         SET IS_PRIMARY       = 'N',
             UPDATED_ON       = SYSDATE,
             UPDATED_BY_PP_NO = P_BY_PP_NO
       WHERE COMPLAINT_ID = L_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
         AND COMPLAINANT_ID <> P_COMPLAINANT_ID
         AND IS_PRIMARY = 'Y';
    END IF;
  END P_UPDATE_COMPLAINANT;

  PROCEDURE P_DELETE_COMPLAINANT(P_COMPLAINANT_ID IN NUMBER,
                                 P_BY_PP_NO       IN NUMBER) IS
  BEGIN
    UPDATE T_AU_IID_COMPLAINANT
       SET ACTIVE_FLAG      = 'N',
           IS_PRIMARY       = 'N',
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINANT_ID = P_COMPLAINANT_ID;
  END P_DELETE_COMPLAINANT;

  PROCEDURE P_GET_COMPLAINANTS_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                            IO_CURSOR      OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT COMPLAINANT_ID,
             COMPLAINT_ID,
             COMPLAINANT_NAME,
             CNIC,
             CELLULAR_NUMBER,
             MAILING_ADDRESS,
             GENDER,
             IS_PRIMARY
        FROM T_AU_IID_COMPLAINANT
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
       ORDER BY CASE
                  WHEN IS_PRIMARY = 'Y' THEN
                   0
                  ELSE
                   1
                END,
                COMPLAINANT_ID;
  END P_GET_COMPLAINANTS_BY_COMPLAINT;

  PROCEDURE P_SET_PRIMARY_COMPLAINANT(P_COMPLAINT_ID   IN NUMBER,
                                      P_COMPLAINANT_ID IN NUMBER,
                                      P_BY_PP_NO       IN NUMBER) IS
  BEGIN
    UPDATE T_AU_IID_COMPLAINANT
       SET IS_PRIMARY       = 'N',
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINT_ID = P_COMPLAINT_ID
       AND ACTIVE_FLAG = 'Y'
       AND IS_PRIMARY = 'Y';
  
    UPDATE T_AU_IID_COMPLAINANT
       SET IS_PRIMARY       = 'Y',
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINT_ID = P_COMPLAINT_ID
       AND COMPLAINANT_ID = P_COMPLAINANT_ID
       AND ACTIVE_FLAG = 'Y';
  END P_SET_PRIMARY_COMPLAINANT;

  PROCEDURE P_SAVE_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                  P_NATURE             IN VARCHAR2,
                                  P_CATEGORY           IN VARCHAR2,
                                  P_RECEIVED_FROM      IN VARCHAR2,
                                  P_LOCATION_TYPE_ID   IN NUMBER,
                                  P_GM_OFFICE_ID       IN NUMBER,
                                  P_REGION_ID          IN NUMBER,
                                  P_BRANCH_ID          IN NUMBER,
                                  P_COMPLAINANT_NAME   IN VARCHAR2,
                                  P_CNIC               IN VARCHAR2,
                                  P_CELLULAR_NUMBER    IN VARCHAR2,
                                  P_MAILING_ADDRESS    IN VARCHAR2,
                                  P_GENDER             IN VARCHAR2,
                                  P_CONTENTS           IN CLOB,
                                  P_UPLOADED_COMPLAINT IN VARCHAR2,
                                  P_UPLOADED_FFR       IN VARCHAR2,
                                  P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                  P_ACTION_REQUIRED    IN VARCHAR2) IS
    L_COMPLAINANT_ID NUMBER;
  BEGIN
    P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       => P_COMPLAINT_ID,
                            P_NATURE             => P_NATURE,
                            P_CATEGORY           => P_CATEGORY,
                            P_RECEIVED_FROM      => P_RECEIVED_FROM,
                            P_LOCATION_TYPE_ID   => P_LOCATION_TYPE_ID,
                            P_GM_OFFICE_ID       => P_GM_OFFICE_ID,
                            P_REGION_ID          => P_REGION_ID,
                            P_BRANCH_ID          => P_BRANCH_ID,
                            P_CONTENTS           => P_CONTENTS,
                            P_UPLOADED_COMPLAINT => P_UPLOADED_COMPLAINT,
                            P_UPLOADED_FFR       => P_UPLOADED_FFR,
                            P_UPLOADED_EVIDENCE  => P_UPLOADED_EVIDENCE,
                            P_ACTION_REQUIRED    => P_ACTION_REQUIRED);
  
    -- Create/Update primary complainant easily for now:
    -- If no primary exists, add as primary. If primary exists, update via P_SET_PRIMARY + P_UPDATE later.
    -- Simplest: always add if missing.
    BEGIN
      SELECT MAX(COMPLAINANT_ID)
        INTO L_COMPLAINANT_ID
        FROM T_AU_IID_COMPLAINANT
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
         AND IS_PRIMARY = 'Y';
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_COMPLAINANT_ID := NULL;
    END;
  
    IF L_COMPLAINANT_ID IS NULL THEN
      P_ADD_COMPLAINANT(P_COMPLAINT_ID     => P_COMPLAINT_ID,
                        P_COMPLAINANT_NAME => P_COMPLAINANT_NAME,
                        P_CNIC             => P_CNIC,
                        P_CELLULAR_NUMBER  => P_CELLULAR_NUMBER,
                        P_MAILING_ADDRESS  => P_MAILING_ADDRESS,
                        P_GENDER           => P_GENDER,
                        P_IS_PRIMARY       => 'Y',
                        P_BY_PP_NO         => NULL,
                        O_COMPLAINANT_ID   => L_COMPLAINANT_ID);
    ELSE
      P_UPDATE_COMPLAINANT(P_COMPLAINANT_ID   => L_COMPLAINANT_ID,
                           P_COMPLAINANT_NAME => P_COMPLAINANT_NAME,
                           P_CNIC             => P_CNIC,
                           P_CELLULAR_NUMBER  => P_CELLULAR_NUMBER,
                           P_MAILING_ADDRESS  => P_MAILING_ADDRESS,
                           P_GENDER           => P_GENDER,
                           P_IS_PRIMARY       => 'Y',
                           P_BY_PP_NO         => NULL);
    END IF;
  END;

  ------------------------------------------------------------------
  -- LEGACY WRAPPER (keep signature for C# compatibility)
  ------------------------------------------------------------------
  PROCEDURE ADD_COMPLAINT(P_NATURE             IN VARCHAR2,
                          P_CATEGORY           IN VARCHAR2,
                          P_SOURCE             IN VARCHAR2,
                          P_SOURCE_OTHER_TEXT  IN VARCHAR2,
                          P_PERTAINS_TO        IN VARCHAR2,
                          P_FIELD_TYPE         IN VARCHAR2,
                          P_HO_UNIT_TYPE_ID    IN NUMBER,
                          P_HO_UNIT_ID         IN NUMBER,
                          P_REGION_ID          IN NUMBER,
                          P_BRANCH_ID          IN NUMBER,
                          P_CONTENTS           IN CLOB,
                          P_UPLOADED_COMPLAINT IN VARCHAR2,
                          P_UPLOADED_FFR       IN VARCHAR2,
                          P_UPLOADED_EVIDENCE  IN VARCHAR2,
                          P_ACTION_REQUIRED    IN VARCHAR2,
                          P_SUBMITTED_BY       IN NUMBER,
                          O_COMPLAINT_ID       OUT NUMBER) IS
    V_NO             VARCHAR2(50);
    L_LOCATION_TYPE  NUMBER;
    L_COMPLAINANT_ID NUMBER;
    L_RECEIVED_FROM  VARCHAR2(200);
  BEGIN
    -- 1) Create header
    P_CREATE_COMPLAINT_HDR('IAID', P_SUBMITTED_BY, O_COMPLAINT_ID, V_NO);
  
    -- 2) Derive received_from (use P_SOURCE; if "Other" then store detail)
    L_RECEIVED_FROM := CASE
                         WHEN P_SOURCE IS NULL THEN
                          NULL
                         WHEN UPPER(TRIM(P_SOURCE)) IN ('OTHER', 'OTHERS') AND
                              P_SOURCE_OTHER_TEXT IS NOT NULL THEN
                          TRIM(P_SOURCE_OTHER_TEXT)
                         ELSE
                          TRIM(P_SOURCE)
                       END;
  
    -- 3) Convert location type if passed as text (safe)
    L_LOCATION_TYPE := CASE
                         WHEN P_FIELD_TYPE IS NULL THEN
                          NULL
                         WHEN REGEXP_LIKE(P_FIELD_TYPE, '^\s*\d+\s*$') THEN
                          TO_NUMBER(TRIM(P_FIELD_TYPE))
                         ELSE
                          NULL
                       END;
  
    -- 4) Upsert complaint details (IAID table)
    P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       => O_COMPLAINT_ID,
                            P_NATURE             => P_NATURE,
                            P_CATEGORY           => P_CATEGORY,
                            P_RECEIVED_FROM      => L_RECEIVED_FROM,
                            P_LOCATION_TYPE_ID   => L_LOCATION_TYPE,
                            P_GM_OFFICE_ID       => P_HO_UNIT_ID, -- HO/GM office id comes from P_HO_UNIT_ID in this legacy wrapper
                            P_REGION_ID          => P_REGION_ID,
                            P_BRANCH_ID          => P_BRANCH_ID,
                            P_CONTENTS           => P_CONTENTS,
                            P_UPLOADED_COMPLAINT => P_UPLOADED_COMPLAINT,
                            P_UPLOADED_FFR       => P_UPLOADED_FFR,
                            P_UPLOADED_EVIDENCE  => P_UPLOADED_EVIDENCE,
                            P_ACTION_REQUIRED    => P_ACTION_REQUIRED);
  
    -- 5) Add PRIMARY complainant row (legacy wrapper does not have complainant fields)
    -- If you truly have no complainant details here, insert a minimal row to keep model consistent.
    -- You can later update it through complainant screen.
    P_ADD_COMPLAINANT(P_COMPLAINT_ID     => O_COMPLAINT_ID,
                      P_COMPLAINANT_NAME => NULL,
                      P_CNIC             => NULL,
                      P_CELLULAR_NUMBER  => NULL,
                      P_MAILING_ADDRESS  => NULL,
                      P_GENDER           => NULL,
                      P_IS_PRIMARY       => 'Y',
                      P_BY_PP_NO         => P_SUBMITTED_BY,
                      O_COMPLAINANT_ID   => L_COMPLAINANT_ID);
  
    COMMIT;
  END ADD_COMPLAINT;

  ------------------------------------------------------------------
  -- GET APIs
  ------------------------------------------------------------------
  PROCEDURE P_GET_COMPLAINT_HDR(P_COMPLAINT_ID IN NUMBER,
                                T_CURSOR       OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.INTAKE_CHANNEL,
             h.STATUS,
             h.SUBMITTED_ON,
             h.SUBMITTED_BY_PP_NO,
             h.ASSIGNED_UNIT_ID,
             h.ACTIVE_FLAG
        FROM T_AU_IID_COMPLAINT_HDR h
       WHERE h.COMPLAINT_ID = P_COMPLAINT_ID;
  END P_GET_COMPLAINT_HDR;

  PROCEDURE P_GET_COMPLAINT_IAID(P_COMPLAINT_ID IN NUMBER,
                                 T_CURSOR       OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT d.*
        FROM T_AU_IID_COMPLAINT_IAID d
       WHERE d.COMPLAINT_ID = P_COMPLAINT_ID;
  END P_GET_COMPLAINT_IAID;

  PROCEDURE GET_COMPLAINTS(P_USER_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  
    R_ID number;
  BEGIN
    select m.role_id
      into R_ID
      from t_user_maping m
     where m.ppno = P_USER_ID;
  
    if (R_ID in (1)) then
      OPEN IO_CURSOR FOR
        SELECT h.complaint_id,
               h.COMPLAINT_NO,
               c.complainant_name,
               d.NATURE,
               d.received_from    as Source,
               e.name             as ASSIGNED_UNIT,
               h.STATUS,
               h.updated_on       as SUBMITTED_ON
          FROM T_AU_IID_COMPLAINT_HDR h
         inner JOIN T_AU_IID_COMPLAINT_IAID d
            ON d.COMPLAINT_ID = h.COMPLAINT_ID
         inner join t_au_iid_complainant c
            on c.complaint_id = d.complaint_id
         inner join t_auditee_entities e
            on h.assigned_unit_id = e.entity_id
         WHERE h.status_id between 1 and 7
         ORDER BY h.complaint_id DESC;
    
    else
    
      OPEN IO_CURSOR FOR
        SELECT h.complaint_id     COMPLAINT_ID,
               h.COMPLAINT_NO     COMPLAINT_NO,
               c.complainant_name COMPLAINANT_NAME,
               d.NATURE           NATURE,
               d.received_from    as SOURCE,
               h.ASSIGNED_UNIT_ID ASSIGNED_UNIT,
               h.STATUS,
               h.submitted_on
          FROM T_AU_IID_COMPLAINT_HDR h
         inner JOIN T_AU_IID_COMPLAINT_IAID d
            ON d.COMPLAINT_ID = h.COMPLAINT_ID
         inner join t_au_iid_complainant c
            on c.complaint_id = d.complaint_id
        -- WHERE h.SUBMITTED_BY_PP_NO = P_USER_ID
         ORDER BY h.SUBMITTED_ON DESC;
    end if;
  END GET_COMPLAINTS;

  PROCEDURE GET_COMPLAINTS_WITHOUT_ASSESSMENT(T_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.STATUS,
             h.SUBMITTED_ON,
             h.ASSIGNED_UNIT_ID,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM,
             d.CONTENTS
        FROM T_AU_IID_COMPLAINT_HDR h
        JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE NOT EXISTS (SELECT 1
                FROM T_AU_IID_ASSESSMENT a
               WHERE a.COMPLAINT_ID = h.COMPLAINT_ID)
       ORDER BY h.COMPLAINT_ID DESC;
  END GET_COMPLAINTS_WITHOUT_ASSESSMENT;

  PROCEDURE GET_COMPLAINTS_DD(P_PAGE_ID in number, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             d.NATURE AS NATURE,
             c.complainant_name || '-' || s.status_name STATUS
        FROM T_AU_IID_COMPLAINT_HDR h
        LEFT JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
        LEFT JOIN T_AU_IID_COMPLAINANT c
          ON c.COMPLAINT_ID = h.COMPLAINT_ID
        left join T_AU_IID_STATUS_MST s
          on s.status_id = h.status_id
       WHERE h.status_id = case
               when P_PAGE_ID = 420 then
                h.status_id
               else
                P_PAGE_ID
             end
       ORDER BY h.COMPLAINT_ID DESC;
  END GET_COMPLAINTS_DD;

  PROCEDURE GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT(p_complaint_id IN NUMBER,
                                                   io_cursor      OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT c.complaint_id AS report_id,
             
             /* Complainant name */
             cc.complainant_name AS name_complainant,
             
             /* Accused names combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     al.person_name || ', ')
                                          ORDER BY al.accused_row_id) AS CLOB),
                           ', ')
                FROM t_au_iid_inq_accused_list al
               WHERE al.complaint_id = c.complaint_id) AS name_accused,
             
             /* Gist from complaint main text */
             c.contents AS gist,
             
             /* Proceedings from statements + record scrutinized combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     CASE
                                                       WHEN s.role_type = 'COMPLAINANT' THEN
                                                        'Statement of Complainant: '
                                                       WHEN s.role_type = 'ACCUSED' THEN
                                                        'Statement of Accused: '
                                                       ELSE
                                                        'Statement: '
                                                     END ||
                                                     NVL(s.key_points, '') || CASE
                                                       WHEN s.statement_datetime IS NOT NULL THEN
                                                        ' (Dated: ' ||
                                                        TO_CHAR(s.statement_datetime, 'DD-MON-YYYY HH:MI AM') || ')'
                                                       ELSE
                                                        ''
                                                     END || CHR(10)) ORDER BY
                                          s.statement_id) AS CLOB),
                           CHR(10))
                FROM t_au_iid_inq_statements s
               WHERE s.complaint_id = c.complaint_id) || CASE
               WHEN EXISTS (SELECT 1
                       FROM t_au_iid_inq_record_scrutinized rs
                      WHERE rs.complaint_id = c.complaint_id) THEN
                CHR(10) || CHR(10) || 'Record Scrutinized:' || CHR(10) ||
                (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                        NVL(rs.record_details,
                                                            '') || CHR(10))
                                             ORDER BY rs.rec_id) AS CLOB),
                              CHR(10))
                   FROM t_au_iid_inq_record_scrutinized rs
                  WHERE rs.complaint_id = c.complaint_id)
               ELSE
                NULL
             END AS proceedings,
             
             /* Findings combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     'Allegation: ' ||
                                                     NVL(a.accusation_text,
                                                         '') || CHR(10) ||
                                                     'Finding: ' ||
                                                     NVL(fr.findings_text, '') ||
                                                     CHR(10) || 'Outcome: ' ||
                                                     NVL(fr.recommendation_text,
                                                         '') || CHR(10) ||
                                                     CHR(10)) ORDER BY
                                          a.accusation_id) AS CLOB),
                           CHR(10))
                FROM t_au_iid_inq_find_recomm fr
                JOIN t_au_iid_inq_accusations a
                  ON a.accusation_id = fr.accusation_id
               WHERE fr.complaint_id = c.complaint_id) AS findings,
             
             /* Recommendations combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     'Allegation: ' ||
                                                     NVL(a.accusation_text,
                                                         '') || CHR(10) ||
                                                     'Recommendation: ' ||
                                                     NVL(fr.recommendation_text,
                                                         '') || CHR(10) ||
                                                     CHR(10)) ORDER BY
                                          a.accusation_id) AS CLOB),
                           CHR(10))
                FROM t_au_iid_inq_find_recomm fr
                JOIN t_au_iid_inq_accusations a
                  ON a.accusation_id = fr.accusation_id
               WHERE fr.complaint_id = c.complaint_id) AS recommendation,
             
             /* Step 9 summary fields */
             NVL(vs.conclusion, '') AS conclusion,
             NVL(vs.reported_in_audit_report, rpt.reported_in_audit_report) AS reported_in_audit_report,
             NVL(vs.audit_report_reference_detail,
                 rpt.audit_report_reference_detail) AS audit_report_reference_detail,
             
             /* Uploaded report: latest saved report snapshot if available */
             rpt.uploaded_report AS uploaded_report,
             
             /* Evidence files combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e, ef.file_path || ', ')
                                          ORDER BY ef.evidence_id) AS CLOB),
                           ', ')
                FROM t_au_iid_inq_evidence_files ef
               WHERE ef.complaint_id = c.complaint_id) AS uploaded_evidence,
             
             /* DSA snapshot / fallback */
             NVL(rpt.uploaded_dsa,
                 (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                         dsa.dsa_status || ', ')
                                              ORDER BY dsa.dsa_id) AS CLOB),
                               ', ')
                    FROM t_au_iid_inq_dsa dsa
                   WHERE dsa.complaint_id = c.complaint_id)) AS uploaded_dsa,
             
             TO_CHAR(sysdate, 'DD-MON-YYYY HH:MI AM') AS submitted_on
      
        FROM t_au_iid_complaint_iaid c
       inner join t_au_iid_complainant cc
          on cc.complaint_id = c.complaint_id
        LEFT JOIN (SELECT r1.*
                     FROM t_au_iid_report r1
                    WHERE r1.report_id =
                          (SELECT MAX(r2.report_id)
                             FROM t_au_iid_report r2
                            WHERE r2.complaint_id = r1.complaint_id)) rpt
          ON rpt.complaint_id = c.complaint_id
        LEFT JOIN t_au_iid_inq_violation_step vs
          ON vs.complaint_id = c.complaint_id
         AND NVL(vs.status, 'A') = 'A'
       WHERE c.complaint_id = p_complaint_id;
  END GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT;

  PROCEDURE GET_COMPLAINT(P_COMPLAINT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.STATUS,
             h.SUBMITTED_ON,
             h.SUBMITTED_BY_PP_NO,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = a.assigned_unit_id) as assigned_unit,
             a.assigned_unit_id,
             a.assessment_id,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM,
             d.LOCATION_TYPE_ID,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = d.GM_OFFICE_ID) as GM_OFFICE,
             d.gm_office_id,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = d.REGION_ID) as REGION,
             d.region_id,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = d.BRANCH_ID) as BRANCH,
             d.branch_id,
             C.COMPLAINANT_NAME,
             c.CNIC,
             c.CELLULAR_NUMBER,
             c.MAILING_ADDRESS,
             c.GENDER,
             d.CONTENTS,
             d.UPLOADED_COMPLAINT,
             d.UPLOADED_EVIDENCE,
             d.UPLOADED_FFR,
             d.ACTION_REQUIRED,
             a.ASSESSMENT,
             a.RECOMMENDATION
        FROM T_AU_IID_COMPLAINT_HDR h
       inner JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       inner join T_AU_IID_COMPLAINANT c
          on c.complaint_id = d.complaint_id
         and c.is_primary = 'Y'
        LEFT JOIN T_AU_IID_ASSESSMENT a
          ON a.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE h.COMPLAINT_ID = P_COMPLAINT_ID;
  END GET_COMPLAINT;

  PROCEDURE GET_LATEST_PLAN_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                         IO_CURSOR      OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT *
        FROM (SELECT p.PLAN_ID,
                     p.COMPLAINT_ID,
                     p.PLAN_DETAILS,
                     p.PLAN_TITLE,
                     p.START_DATE,
                     p.END_DATE,
                     p.STATUS,
                     p.SUBMITTED_BY,
                     p.SUBMITTED_ON
                FROM T_AU_IID_INV_PLAN p
               WHERE p.COMPLAINT_ID = P_COMPLAINT_ID
               ORDER BY p.SUBMITTED_ON DESC NULLS LAST, p.PLAN_ID DESC)
       WHERE ROWNUM = 1;
  END GET_LATEST_PLAN_BY_COMPLAINT;

  PROCEDURE P_GET_COMPLAINT_LIST(P_INTAKE_CHANNEL IN VARCHAR2 DEFAULT NULL,
                                 P_STATUS         IN VARCHAR2 DEFAULT NULL,
                                 P_FROM_DATE      IN DATE DEFAULT NULL,
                                 P_TO_DATE        IN DATE DEFAULT NULL,
                                 T_CURSOR         OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.INTAKE_CHANNEL,
             h.STATUS,
             h.SUBMITTED_ON,
             h.SUBMITTED_BY_PP_NO,
             h.ASSIGNED_UNIT_ID,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM
        FROM T_AU_IID_COMPLAINT_HDR h
        LEFT JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE h.ACTIVE_FLAG = 'Y'
         AND (P_INTAKE_CHANNEL IS NULL OR
             h.INTAKE_CHANNEL = UPPER(TRIM(P_INTAKE_CHANNEL)))
         AND (P_STATUS IS NULL OR h.STATUS = P_STATUS)
         AND (P_FROM_DATE IS NULL OR h.SUBMITTED_ON >= P_FROM_DATE)
         AND (P_TO_DATE IS NULL OR h.SUBMITTED_ON < (P_TO_DATE + 1))
       ORDER BY h.SUBMITTED_ON DESC;
  END P_GET_COMPLAINT_LIST;

  PROCEDURE P_GET_COMPLAINT_ID_BY_PLAN(P_PLAN_ID      IN NUMBER,
                                       O_COMPLAINT_ID OUT NUMBER) IS
  BEGIN
    SELECT COMPLAINT_ID
      INTO O_COMPLAINT_ID
      FROM T_AU_IID_INV_PLAN
     WHERE PLAN_ID = P_PLAN_ID;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      O_COMPLAINT_ID := NULL;
  END P_GET_COMPLAINT_ID_BY_PLAN;

  PROCEDURE P_GET_COMPLAINT_ID_BY_REPORT(P_REPORT_ID    IN NUMBER,
                                         O_COMPLAINT_ID OUT NUMBER) IS
  BEGIN
    -- NOTE: PKG_INQ uses T_AU_IID_REPORT (not T_AU_IID_INQUIRY_REPORT)
    SELECT COMPLAINT_ID
      INTO O_COMPLAINT_ID
      FROM T_AU_IID_REPORT
     WHERE REPORT_ID = P_REPORT_ID;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      O_COMPLAINT_ID := NULL;
  END P_GET_COMPLAINT_ID_BY_REPORT;

  PROCEDURE P_SAVE_INQ_FINDINGS_REC(P_COMPLAINT_ID   IN NUMBER,
                                    P_FINDINGS       IN CLOB,
                                    P_RECOMMENDATION IN CLOB,
                                    P_UPDATED_BY     IN NUMBER,
                                    IO_CURSOR        OUT T_CURSOR) IS
    L_REPORT_ID NUMBER;
  BEGIN
    -- get latest report row for complaint (if any)
    SELECT MAX(REPORT_ID)
      INTO L_REPORT_ID
      FROM T_AU_IID_REPORT
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  
    IF L_REPORT_ID IS NULL THEN
      SELECT SEQ_AU_IID_REPORT_ID.NEXTVAL INTO L_REPORT_ID FROM DUAL;
    
      INSERT INTO T_AU_IID_REPORT
        (REPORT_ID,
         COMPLAINT_ID,
         FINDINGS,
         RECOMMENDATION,
         SUBMITTED_ON,
         SUBMITTED_BY)
      VALUES
        (L_REPORT_ID,
         P_COMPLAINT_ID,
         P_FINDINGS,
         P_RECOMMENDATION,
         SYSDATE,
         P_UPDATED_BY);
    ELSE
      UPDATE T_AU_IID_REPORT
         SET FINDINGS = P_FINDINGS, RECOMMENDATION = P_RECOMMENDATION
       WHERE REPORT_ID = L_REPORT_ID;
    END IF;
  
    OPEN IO_CURSOR FOR
      SELECT 'Y' AS OK,
             'Findings & Recommendations saved.' AS MESSAGE,
             L_REPORT_ID AS ID
        FROM DUAL;
  
  EXCEPTION
    WHEN OTHERS THEN
      DECLARE
        v_err VARCHAR2(4000);
      BEGIN
        v_err := SQLERRM;
      
        OPEN io_cursor FOR
          SELECT 'N' AS OK, v_err AS MESSAGE, NULL AS ID FROM dual;
      END;
  END P_SAVE_INQ_FINDINGS_REC;

  ------------------------------------------------------------------
  -- INITIAL ASSESSMENT
  ------------------------------------------------------------------
  PROCEDURE ADD_ASSESSMENT(P_COMPLAINT_ID     IN NUMBER,
                           P_RECEIVED_BY      IN NUMBER,
                           P_ASSESSMENT       IN CLOB,
                           P_RECOMMENDATION   IN VARCHAR2,
                           P_ASSIGNED_UNIT_ID IN NUMBER,
                           O_ASSESSMENT_ID    OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_ASSESSMENT_ID.NEXTVAL INTO O_ASSESSMENT_ID FROM DUAL;
  
    INSERT INTO T_AU_IID_ASSESSMENT
      (ASSESSMENT_ID,
       COMPLAINT_ID,
       RECEIVED_BY,
       ASSESSMENT,
       RECOMMENDATION,
       FORWARDED_ON,
       PRELIM_RISK,
       ASSIGNED_UNIT_ID)
    VALUES
      (O_ASSESSMENT_ID,
       P_COMPLAINT_ID,
       P_RECEIVED_BY,
       P_ASSESSMENT,
       P_RECOMMENDATION,
       SYSDATE,
       NULL,
       P_ASSIGNED_UNIT_ID);
  
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET ASSIGNED_UNIT_ID = P_ASSIGNED_UNIT_ID,
           STATUS_ID        = 345,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_RECEIVED_BY
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
    commit;
    SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_HEAD_REVIEW, P_RECEIVED_BY);
  END ADD_ASSESSMENT;

  ------------------------------------------------------------------
  -- HEAD REVIEW
  ------------------------------------------------------------------
  PROCEDURE ADD_HEAD_REVIEW(P_COMPLAINT_ID           IN NUMBER,
                            P_ASSESSMENT_ID          IN NUMBER,
                            P_REVIEWED_BY            IN NUMBER,
                            P_DIRECTIONS             IN CLOB,
                            P_ASSIGNED_TO_UNIT       IN NUMBER,
                            P_TEAM_LEAD              IN NUMBER,
                            P_TEAM_MEMBERS           IN CLOB,
                            P_ASSIGNED_ON            IN VARCHAR2,
                            P_DUE_DATE               IN VARCHAR2,
                            P_REFERRED_BACK_COMMENTS IN CLOB,
                            P_ACTION                 IN VARCHAR2,
                            O_REVIEW_ID              OUT NUMBER) IS
    L_ASSIGNED_ON  DATE;
    L_DUE_DATE     DATE;
    L_EXIST_REVIEW NUMBER;
  BEGIN
    -- Parse Assigned On
    BEGIN
      L_ASSIGNED_ON := TO_DATE(P_ASSIGNED_ON, 'YYYY-MM-DD');
    EXCEPTION
      WHEN OTHERS THEN
        BEGIN
          L_ASSIGNED_ON := TO_DATE(P_ASSIGNED_ON, 'DD-MON-YYYY');
        EXCEPTION
          WHEN OTHERS THEN
            L_ASSIGNED_ON := SYSDATE;
        END;
    END;
  
    -- Parse Due Date
    BEGIN
      L_DUE_DATE := TO_DATE(P_DUE_DATE, 'YYYY-MM-DD');
    EXCEPTION
      WHEN OTHERS THEN
        BEGIN
          L_DUE_DATE := TO_DATE(P_DUE_DATE, 'DD-MON-YYYY');
        EXCEPTION
          WHEN OTHERS THEN
            L_DUE_DATE := NULL;
        END;
    END;
  
    -- Find existing review for this complaint (and assessment if provided)
    BEGIN
      SELECT MAX(REVIEW_ID)
        INTO L_EXIST_REVIEW
        FROM T_AU_IID_HEAD_REVIEW
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND (P_ASSESSMENT_ID IS NULL OR ASSESSMENT_ID = P_ASSESSMENT_ID);
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_REVIEW := NULL;
    END;
  
    IF L_EXIST_REVIEW IS NULL THEN
      SELECT SEQ_AU_IID_HEAD_REVIEW_ID.NEXTVAL INTO O_REVIEW_ID FROM DUAL;
    ELSE
      O_REVIEW_ID := L_EXIST_REVIEW;
    END IF;
  
    MERGE INTO T_AU_IID_HEAD_REVIEW t
    USING (SELECT O_REVIEW_ID AS REVIEW_ID, P_COMPLAINT_ID AS COMPLAINT_ID
             FROM DUAL) s
    ON (t.REVIEW_ID = s.REVIEW_ID AND t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.ASSESSMENT_ID          = P_ASSESSMENT_ID,
             t.REVIEWED_BY            = P_REVIEWED_BY,
             t.DIRECTIONS             = P_DIRECTIONS,
             t.ASSIGNED_TO_UNIT       = P_ASSIGNED_TO_UNIT,
             t.TEAM_LEAD              = P_TEAM_LEAD,
             t.TEAM_MEMBERS           = P_TEAM_MEMBERS,
             t.ASSIGNED_ON            = L_ASSIGNED_ON,
             t.DUE_DATE               = L_DUE_DATE,
             t.REFERRED_BACK_COMMENTS = P_REFERRED_BACK_COMMENTS,
             t.ACTION                 = P_ACTION,
             t.REVIEWED_ON            = SYSDATE,
             t.APPROVED_ON            = CASE
                                          WHEN UPPER(P_ACTION) = 'APPROVE' THEN
                                           SYSDATE
                                          ELSE
                                           NULL
                                        END WHEN NOT MATCHED THEN INSERT(REVIEW_ID, COMPLAINT_ID, ASSESSMENT_ID, REVIEWED_BY, DIRECTIONS, ASSIGNED_TO_UNIT, TEAM_LEAD, TEAM_MEMBERS, ASSIGNED_ON, DUE_DATE, REFERRED_BACK_COMMENTS, ACTION, REVIEWED_ON, APPROVED_ON) VALUES(O_REVIEW_ID, P_COMPLAINT_ID, P_ASSESSMENT_ID, P_REVIEWED_BY, P_DIRECTIONS, P_ASSIGNED_TO_UNIT, P_TEAM_LEAD, P_TEAM_MEMBERS, L_ASSIGNED_ON, L_DUE_DATE, P_REFERRED_BACK_COMMENTS, P_ACTION, SYSDATE,CASE
               WHEN UPPER(P_ACTION) = 'APPROVE' THEN
                SYSDATE
               ELSE
                NULL
             END);
  
    -- Update header
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET ASSIGNED_UNIT_ID = P_ASSIGNED_TO_UNIT,
           STATUS_ID        = 348,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_REVIEWED_BY
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  
    -- Status transitions
    IF UPPER(P_ACTION) IN ('REFERBACK', 'REFER BACK') THEN
      SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_IN_ASSESS, P_REVIEWED_BY);
    ELSIF UPPER(P_ACTION) = 'CLOSE' THEN
      SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_CLOSED, P_REVIEWED_BY);
    ELSE
      SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_PLAN_DRAFTED, P_REVIEWED_BY);
    END IF;
  
    COMMIT;
  END ADD_HEAD_REVIEW;

  ------------------------------------------------------------------
  -- INVESTIGATION PLAN
  ------------------------------------------------------------------
  PROCEDURE ADD_INV_PLAN(P_COMPLAINT_ID    IN NUMBER,
                         P_PLAN_DETAILS    IN CLOB,
                         P_SUBMITTED_BY    IN NUMBER,
                         P_STATUS          IN VARCHAR2,
                         P_INV_RISK        IN VARCHAR2,
                         P_INV_SIZE        IN VARCHAR2,
                         P_NO_OF_DAYS      IN NUMBER,
                         P_TRAVELLING_DAYS IN NUMBER,
                         P_TEAM_LEAD       IN VARCHAR2,
                         P_TEAM_MEMBERS    IN VARCHAR2,
                         P_START_DATE      IN DATE,
                         P_ACTIVITIES_TEXT IN VARCHAR2,
                         O_PLAN_ID         OUT NUMBER) IS
    L_EXIST_PLAN_ID NUMBER;
  BEGIN
    -- Find existing plan for this complaint (if your table can have multiple,
    -- this picks the latest by PLAN_ID)
    BEGIN
      SELECT MAX(PLAN_ID)
        INTO L_EXIST_PLAN_ID
        FROM T_AU_IID_INV_PLAN
       WHERE COMPLAINT_ID = P_COMPLAINT_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_PLAN_ID := NULL;
    END;
  
    IF L_EXIST_PLAN_ID IS NULL THEN
      SELECT SEQ_AU_IID_INV_PLAN_ID.NEXTVAL INTO O_PLAN_ID FROM DUAL;
    ELSE
      O_PLAN_ID := L_EXIST_PLAN_ID;
    END IF;
  
    MERGE INTO T_AU_IID_INV_PLAN t
    USING (SELECT P_COMPLAINT_ID AS COMPLAINT_ID, O_PLAN_ID AS PLAN_ID
             FROM DUAL) s
    ON (t.PLAN_ID = s.PLAN_ID AND t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.PLAN_DETAILS       = P_PLAN_DETAILS,
             t.SUBMITTED_BY       = P_SUBMITTED_BY,
             t.SUBMITTED_ON       = SYSDATE,
             t.STATUS             = P_STATUS,
             t.PLAN_TITLE         = 'Investigation Plan',
             t.START_DATE         = P_START_DATE,
             t.INVESTIGATION_RISK = P_INV_RISK,
             t.INVESTIGATION_SIZE = P_INV_SIZE,
             t.NO_OF_DAYS         = P_NO_OF_DAYS,
             t.TRAVELLING_DAYS    = P_TRAVELLING_DAYS,
             t.TEAM_LEAD          = P_TEAM_LEAD,
             t.TEAM_MEMBERS       = P_TEAM_MEMBERS,
             t.ACTIVITIES_TEXT    = P_ACTIVITIES_TEXT
    WHEN NOT MATCHED THEN
      INSERT
        (PLAN_ID,
         COMPLAINT_ID,
         PLAN_DETAILS,
         SUBMITTED_BY,
         SUBMITTED_ON,
         STATUS,
         PLAN_TITLE,
         START_DATE,
         INVESTIGATION_RISK,
         INVESTIGATION_SIZE,
         NO_OF_DAYS,
         TRAVELLING_DAYS,
         TEAM_LEAD,
         TEAM_MEMBERS,
         ACTIVITIES_TEXT)
      VALUES
        (O_PLAN_ID,
         P_COMPLAINT_ID,
         P_PLAN_DETAILS,
         P_SUBMITTED_BY,
         SYSDATE,
         P_STATUS,
         'Investigation Plan',
         P_START_DATE,
         P_INV_RISK,
         P_INV_SIZE,
         P_NO_OF_DAYS,
         P_TRAVELLING_DAYS,
         P_TEAM_LEAD,
         P_TEAM_MEMBERS,
         P_ACTIVITIES_TEXT);
  
    -- Status updates (keep these consistent)
    SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_PLAN_DRAFTED, P_SUBMITTED_BY);
  
    UPDATE T_AU_IID_COMPLAINT_HDR h
       SET h.STATUS_ID        = 349,
           h.UPDATED_ON       = SYSDATE,
           h.UPDATED_BY_PP_NO = P_SUBMITTED_BY
     WHERE h.COMPLAINT_ID = P_COMPLAINT_ID;
  
    COMMIT;
  END ADD_INV_PLAN;

  PROCEDURE GET_INV_PLAN(p_complaint_id IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT PLAN_ID,
             COMPLAINT_ID,
             PLAN_DETAILS,
             SUBMITTED_BY,
             SUBMITTED_ON,
             STATUS,
             PLAN_TITLE,
             trunc(START_DATE) as START_DATE,
             INVESTIGATION_RISK,
             INVESTIGATION_SIZE,
             NO_OF_DAYS,
             TRAVELLING_DAYS,
             TEAM_LEAD,
             TEAM_MEMBERS,
             ACTIVITIES_TEXT
        FROM T_AU_IID_INV_PLAN
       WHERE COMPLAINT_ID = p_complaint_id;
  END GET_INV_PLAN;

  PROCEDURE GET_IID_TASK_LIST(P_UNIT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM AS SOURCE,
             c.complainant_name,
             ad.approved_on as ASSIGNED_ON,
             h.STATUS,
             h.SUBMITTED_ON,
             h.ASSIGNED_UNIT_ID AS ASSIGNED_TO,
             a.assigned_unit_id,
             p.plan_id AS PLAN_ID,
             (SELECT MAX(r.REPORT_ID)
                FROM T_AU_IID_REPORT r
               WHERE r.COMPLAINT_ID = h.COMPLAINT_ID) AS REPORT_ID
        FROM T_AU_IID_COMPLAINT_HDR h
       inner JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       inner join T_AU_IID_COMPLAINANT c
          on c.complaint_id = d.complaint_id
         and c.is_primary = 'Y'
       inner join t_au_iid_assessment a
          on a.complaint_id = d.complaint_id
       inner join t_au_iid_head_review ad
          on ad.complaint_id = d.complaint_id
       inner join T_AU_IID_INV_PLAN p
          on p.complaint_id = h.complaint_id
       inner join t_au_iid_head_plan_approval ap
          on ap.plan_id = p.plan_id
       WHERE a.assigned_unit_id = P_UNIT_ID
         and h.status_id = 409
      --113191
      
       ORDER BY h.UPDATED_ON DESC NULLS LAST, h.SUBMITTED_ON DESC;
  END GET_IID_TASK_LIST;

  ------------------------------------------------------------------
  -- PLAN APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_PLAN_APPROVAL(P_PLAN_ID         IN NUMBER,
                              P_APPROVED_BY     IN NUMBER,
                              P_IS_APPROVED     IN VARCHAR2,
                              P_EDITED_PLAN     IN CLOB,
                              P_FURTHER_ACTIONS IN CLOB,
                              O_APPROVAL_ID     OUT NUMBER) IS
    L_COMPLAINT_ID   NUMBER;
    L_EXIST_APPROVAL NUMBER;
    L_IS_APPROVED    VARCHAR2(50);
  BEGIN
    L_IS_APPROVED := UPPER(TRIM(P_IS_APPROVED));
  
    -- Find existing approval record for this plan (latest)
    BEGIN
      SELECT MAX(APPROVAL_ID)
        INTO L_EXIST_APPROVAL
        FROM T_AU_IID_HEAD_PLAN_APPROVAL
       WHERE PLAN_ID = P_PLAN_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_APPROVAL := NULL;
    END;
  
    IF L_EXIST_APPROVAL IS NULL THEN
      SELECT SEQ_AU_IID_PLAN_APPROVAL_ID.NEXTVAL
        INTO O_APPROVAL_ID
        FROM DUAL;
    ELSE
      O_APPROVAL_ID := L_EXIST_APPROVAL;
    END IF;
  
    MERGE INTO T_AU_IID_HEAD_PLAN_APPROVAL t
    USING (SELECT O_APPROVAL_ID AS APPROVAL_ID, P_PLAN_ID AS PLAN_ID
             FROM DUAL) s
    ON (t.APPROVAL_ID = s.APPROVAL_ID AND t.PLAN_ID = s.PLAN_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.APPROVED_BY     = P_APPROVED_BY,
             t.IS_APPROVED     = P_IS_APPROVED,
             t.EDITED_PLAN     = P_EDITED_PLAN,
             t.FURTHER_ACTIONS = P_FURTHER_ACTIONS,
             t.APPROVED_ON     = SYSDATE
    WHEN NOT MATCHED THEN
      INSERT
        (APPROVAL_ID,
         PLAN_ID,
         APPROVED_BY,
         IS_APPROVED,
         EDITED_PLAN,
         FURTHER_ACTIONS,
         APPROVED_ON)
      VALUES
        (O_APPROVAL_ID,
         P_PLAN_ID,
         P_APPROVED_BY,
         P_IS_APPROVED,
         P_EDITED_PLAN,
         P_FURTHER_ACTIONS,
         SYSDATE);
  
    -- Get complaint id
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_INV_PLAN
     WHERE PLAN_ID = P_PLAN_ID;
  
    -- Status updates
    IF L_IS_APPROVED IN ('Y', 'YES', 'APPROVE', 'APPROVED') THEN
      SET_CASE_STATUS(L_COMPLAINT_ID,
                      C_STATUS_PLAN_APPROVED,
                      P_APPROVED_BY);
    
      UPDATE T_AU_IID_COMPLAINT_HDR h
         SET h.STATUS_ID        = 409,
             h.UPDATED_ON       = SYSDATE,
             h.UPDATED_BY_PP_NO = P_APPROVED_BY
       WHERE h.COMPLAINT_ID = L_COMPLAINT_ID;
    ELSE
      SET_CASE_STATUS(L_COMPLAINT_ID, C_STATUS_PLAN_DRAFTED, P_APPROVED_BY);
    END IF;
  
    COMMIT;
  END ADD_PLAN_APPROVAL;

  ------------------------------------------------------------------
  -- INQUIRY REPORT
  ------------------------------------------------------------------
  PROCEDURE ADD_INQUIRY_REPORT(P_COMPLAINT_ID                  IN NUMBER,
                               P_NAME_COMPLAINANT              IN VARCHAR2,
                               P_NAME_ACCUSED                  IN VARCHAR2,
                               P_GIST                          IN CLOB,
                               P_PROCEEDINGS                   IN CLOB,
                               P_FINDINGS                      IN CLOB,
                               P_RECOMMENDATION                IN CLOB,
                               P_CONCLUSION                    IN CLOB,
                               P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                               P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                               P_UPLOADED_REPORT               IN VARCHAR2,
                               P_UPLOADED_EVIDENCE             IN VARCHAR2,
                               P_UPLOADED_DSA                  IN VARCHAR2,
                               P_SUBMITTED_ON                  IN DATE,
                               P_SUBMITTED_BY                  IN NUMBER,
                               O_REPORT_ID                     OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_REPORT_ID.NEXTVAL INTO O_REPORT_ID FROM DUAL;
  
    INSERT INTO T_AU_IID_REPORT
      (REPORT_ID,
       COMPLAINT_ID,
       NAME_COMPLAINANT,
       NAME_ACCUSED,
       GIST,
       PROCEEDINGS,
       FINDINGS,
       RECOMMENDATION,
       CONCLUSION,
       REPORTED_IN_AUDIT_REPORT,
       AUDIT_REPORT_REFERENCE_DETAIL,
       UPLOADED_REPORT,
       UPLOADED_EVIDENCE,
       UPLOADED_DSA,
       SUBMITTED_ON,
       SUBMITTED_BY)
    VALUES
      (O_REPORT_ID,
       P_COMPLAINT_ID,
       P_NAME_COMPLAINANT,
       P_NAME_ACCUSED,
       P_GIST,
       P_PROCEEDINGS,
       P_FINDINGS,
       P_RECOMMENDATION,
       P_CONCLUSION,
       P_REPORTED_IN_AUDIT_REPORT,
       P_AUDIT_REPORT_REFERENCE_DETAIL,
       P_UPLOADED_REPORT,
       P_UPLOADED_EVIDENCE,
       P_UPLOADED_DSA,
       NVL(P_SUBMITTED_ON, SYSDATE),
       P_SUBMITTED_BY);
  
    SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_REPORT_DRAFT, P_SUBMITTED_BY);
  END ADD_INQUIRY_REPORT;

  PROCEDURE GET_INQUIRY_REPORT(P_REPORT_ID IN NUMBER,
                               IO_CURSOR   OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT REPORT_ID,
             CONCLUSION,
             REPORTED_IN_AUDIT_REPORT,
             AUDIT_REPORT_REFERENCE_DETAIL,
             UPLOADED_REPORT,
             UPLOADED_EVIDENCE,
             NVL(UPLOADED_DSA, '') AS UPLOADED_DSA
        FROM T_AU_IID_REPORT
       WHERE REPORT_ID = P_REPORT_ID;
  END GET_INQUIRY_REPORT;

  ------------------------------------------------------------------
  -- ANALYSIS
  ------------------------------------------------------------------
  PROCEDURE ADD_ANALYSIS(P_REPORT_ID             IN NUMBER,
                         P_POLICY_GAPS           IN CLOB,
                         P_CONTROL_GAPS          IN CLOB,
                         P_PROCEDURAL_VIOLATIONS IN CLOB,
                         P_FORWARD_TO            IN VARCHAR2,
                         P_COMMENTS              IN CLOB,
                         P_DECISION              IN VARCHAR2,
                         P_REFER_BACK_COMMENTS   IN CLOB,
                         P_ANALYZED_BY           IN NUMBER,
                         O_ANALYSIS_ID           OUT NUMBER) IS
    L_COMPLAINT_ID   NUMBER;
    L_EXIST_ANALYSIS NUMBER;
  BEGIN
    -- Find existing analysis for this report (pick latest)
    BEGIN
      SELECT MAX(ANALYSIS_ID)
        INTO L_EXIST_ANALYSIS
        FROM T_AU_IID_ANALYSIS
       WHERE REPORT_ID = P_REPORT_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_ANALYSIS := NULL;
    END;
  
    IF L_EXIST_ANALYSIS IS NULL THEN
      SELECT SEQ_AU_IID_ANALYSIS_ID.NEXTVAL INTO O_ANALYSIS_ID FROM DUAL;
    ELSE
      O_ANALYSIS_ID := L_EXIST_ANALYSIS;
    END IF;
  
    MERGE INTO T_AU_IID_ANALYSIS t
    USING (SELECT P_REPORT_ID AS REPORT_ID, O_ANALYSIS_ID AS ANALYSIS_ID
             FROM DUAL) s
    ON (t.ANALYSIS_ID = s.ANALYSIS_ID AND t.REPORT_ID = s.REPORT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.POLICY_GAPS           = P_POLICY_GAPS,
             t.CONTROL_GAPS          = P_CONTROL_GAPS,
             t.PROCEDURAL_VIOLATIONS = P_PROCEDURAL_VIOLATIONS,
             t.FORWARD_TO            = P_FORWARD_TO,
             t.COMMENTS              = P_COMMENTS,
             t.DECISION              = P_DECISION,
             t.REFER_BACK_COMMENTS   = P_REFER_BACK_COMMENTS,
             t.ANALYZED_BY           = P_ANALYZED_BY,
             t.ANALYZED_ON           = SYSDATE
    WHEN NOT MATCHED THEN
      INSERT
        (ANALYSIS_ID,
         REPORT_ID,
         POLICY_GAPS,
         CONTROL_GAPS,
         PROCEDURAL_VIOLATIONS,
         FORWARD_TO,
         COMMENTS,
         DECISION,
         REFER_BACK_COMMENTS,
         ANALYZED_BY,
         ANALYZED_ON)
      VALUES
        (O_ANALYSIS_ID,
         P_REPORT_ID,
         P_POLICY_GAPS,
         P_CONTROL_GAPS,
         P_PROCEDURAL_VIOLATIONS,
         P_FORWARD_TO,
         P_COMMENTS,
         P_DECISION,
         P_REFER_BACK_COMMENTS,
         P_ANALYZED_BY,
         SYSDATE);
  
    -- Derive complaint id for status update
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_REPORT
     WHERE REPORT_ID = P_REPORT_ID;
  
    IF UPPER(P_DECISION) LIKE '%REFER%' THEN
      SET_CASE_STATUS(L_COMPLAINT_ID, C_STATUS_REPORT_DRAFT, P_ANALYZED_BY);
    ELSE
      SET_CASE_STATUS(L_COMPLAINT_ID,
                      C_STATUS_FINAL_APPROVAL,
                      P_ANALYZED_BY);
    END IF;
  
    COMMIT;
  END ADD_ANALYSIS;

  ------------------------------------------------------------------
  -- FINAL APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_FINAL_APPROVAL(P_REPORT_ID         IN NUMBER,
                               P_COMMENTS          IN CLOB,
                               P_APPROVED          IN VARCHAR2,
                               P_APPROVED_BY       IN NUMBER,
                               O_FINAL_APPROVAL_ID OUT NUMBER) IS
    L_COMPLAINT_ID NUMBER;
  BEGIN
    SELECT SEQ_AU_IID_FINAL_APPROVAL_ID.NEXTVAL
      INTO O_FINAL_APPROVAL_ID
      FROM DUAL;
  
    INSERT INTO T_AU_IID_FINAL_APPROVAL
      (FINAL_APPROVAL_ID,
       REPORT_ID,
       COMMENTS,
       APPROVED,
       APPROVED_BY,
       APPROVED_ON)
    VALUES
      (O_FINAL_APPROVAL_ID,
       P_REPORT_ID,
       P_COMMENTS,
       P_APPROVED,
       P_APPROVED_BY,
       SYSDATE);
  
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_REPORT
     WHERE REPORT_ID = P_REPORT_ID;
  
    IF UPPER(P_APPROVED) IN ('Y', 'YES', 'APPROVE', 'APPROVED') THEN
      SET_CASE_STATUS(L_COMPLAINT_ID, C_STATUS_CLOSED, P_APPROVED_BY);
    ELSE
      SET_CASE_STATUS(L_COMPLAINT_ID,
                      C_STATUS_FINAL_APPROVAL,
                      P_APPROVED_BY);
    END IF;
  END ADD_FINAL_APPROVAL;

  ------------------------------------------------------------------
  -- CASE STUDY
  ------------------------------------------------------------------
  PROCEDURE ADD_CASE_STUDY(P_COMPLAINT_ID           IN NUMBER,
                           P_ORIGIN_PROCESS_OWNER   IN VARCHAR2,
                           P_NAME_COMPLAINANT       IN VARCHAR2,
                           P_BRANCH                 IN VARCHAR2,
                           P_GIST                   IN CLOB,
                           P_OUTCOME                IN CLOB,
                           P_MODUS_OPERANDI         IN CLOB,
                           P_GAPS                   IN CLOB,
                           P_ROOT_CAUSE             IN CLOB,
                           P_ACTIONS_REC            IN CLOB,
                           P_STATUS                 IN VARCHAR2,
                           P_POLICY_GAPS_IDENTIFIED IN CLOB,
                           P_CONTROL_VIOLATIONS     IN CLOB,
                           P_RISK_IDENTIFIED        IN CLOB,
                           P_REG_COMPLIANCE_FAILURE IN CLOB,
                           O_CASE_ID                OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_CASE_STUDY_ID.NEXTVAL INTO O_CASE_ID FROM DUAL;
  
    INSERT INTO T_AU_IID_CASE_STUDY
      (CASE_ID,
       COMPLAINT_ID,
       ORIGIN_PROCESS_OWNER,
       NAME_COMPLAINANT,
       BRANCH,
       GIST,
       OUTCOME,
       MODUS_OPERANDI,
       GAPS,
       ROOT_CAUSE,
       ACTIONS_REC,
       STATUS,
       POLICY_GAPS_IDENTIFIED,
       CONTROL_VIOLATIONS,
       RISK_IDENTIFIED,
       REG_COMPLIANCE_FAILURE)
    VALUES
      (O_CASE_ID,
       P_COMPLAINT_ID,
       P_ORIGIN_PROCESS_OWNER,
       P_NAME_COMPLAINANT,
       P_BRANCH,
       P_GIST,
       P_OUTCOME,
       P_MODUS_OPERANDI,
       P_GAPS,
       P_ROOT_CAUSE,
       P_ACTIONS_REC,
       P_STATUS,
       P_POLICY_GAPS_IDENTIFIED,
       P_CONTROL_VIOLATIONS,
       P_RISK_IDENTIFIED,
       P_REG_COMPLIANCE_FAILURE);
  END ADD_CASE_STUDY;

  ------------------------------------------------------------------
  -- REPORTS FILTERING
  ------------------------------------------------------------------
  PROCEDURE GET_REPORTS(P_FILTER          IN VARCHAR2,
                        P_SOURCE          IN VARCHAR2,
                        P_CATEGORY        IN VARCHAR2,
                        P_PERTAINS_TO     IN VARCHAR2,
                        P_DATE_FROM       IN VARCHAR2,
                        P_DATE_TO         IN VARCHAR2,
                        P_REGION_ID       IN NUMBER,
                        P_BRANCH_ID       IN NUMBER,
                        P_HO_UNIT_TYPE_ID IN NUMBER,
                        P_HO_UNIT_ID      IN NUMBER,
                        P_STATUS          IN VARCHAR2,
                        IO_CURSOR         OUT T_CURSOR) IS
    L_DATE_FROM DATE;
    L_DATE_TO   DATE;
  BEGIN
    BEGIN
      L_DATE_FROM := CASE
                       WHEN P_DATE_FROM IS NULL OR TRIM(P_DATE_FROM) = '' THEN
                        NULL
                       ELSE
                        TO_DATE(P_DATE_FROM, 'YYYY-MM-DD')
                     END;
    EXCEPTION
      WHEN OTHERS THEN
        L_DATE_FROM := NULL;
    END;
  
    BEGIN
      L_DATE_TO := CASE
                     WHEN P_DATE_TO IS NULL OR TRIM(P_DATE_TO) = '' THEN
                      NULL
                     ELSE
                      TO_DATE(P_DATE_TO, 'YYYY-MM-DD')
                   END;
    EXCEPTION
      WHEN OTHERS THEN
        L_DATE_TO := NULL;
    END;
  
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM AS SOURCE,
             h.STATUS,
             h.SUBMITTED_ON,
             d.REGION_ID,
             d.BRANCH_ID,
             d.GM_OFFICE_ID AS HO_UNIT_ID,
             (SELECT MAX(r.REPORT_ID)
                FROM T_AU_IID_REPORT r
               WHERE r.COMPLAINT_ID = h.COMPLAINT_ID) AS REPORT_ID
        FROM T_AU_IID_COMPLAINT_HDR h
        LEFT JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE h.ACTIVE_FLAG = 'Y'
         AND (P_FILTER IS NULL OR TRIM(P_FILTER) = '' OR
             d.NATURE = P_FILTER)
         AND (P_SOURCE IS NULL OR TRIM(P_SOURCE) = '' OR
             d.RECEIVED_FROM = P_SOURCE)
         AND (P_CATEGORY IS NULL OR TRIM(P_CATEGORY) = '' OR
             d.CATEGORY = P_CATEGORY)
         AND (P_STATUS IS NULL OR TRIM(P_STATUS) = '' OR
             h.STATUS = P_STATUS)
         AND (P_REGION_ID IS NULL OR P_REGION_ID = 0 OR
             d.REGION_ID = P_REGION_ID)
         AND (P_BRANCH_ID IS NULL OR P_BRANCH_ID = 0 OR
             d.BRANCH_ID = P_BRANCH_ID)
         AND (P_HO_UNIT_ID IS NULL OR P_HO_UNIT_ID = 0 OR
             d.GM_OFFICE_ID = P_HO_UNIT_ID)
         AND (L_DATE_FROM IS NULL OR h.SUBMITTED_ON >= L_DATE_FROM)
         AND (L_DATE_TO IS NULL OR h.SUBMITTED_ON < (L_DATE_TO + 1))
       ORDER BY h.SUBMITTED_ON DESC;
  END GET_REPORTS;

  PROCEDURE GET_EMPLOYEE_INFO(P_PP_NO in number, io_cursor OUT t_cursor) is
  
  Begin
    open io_cursor for
      select e.ppno, e.ename, e.fathername, e.CNIC
        from v_get_iid_employee_info e
       where e.ppno = P_PP_NO;
  
  end;

  PROCEDURE P_RESULT_OK(io_cursor OUT t_cursor,
                        p_message IN VARCHAR2,
                        p_id      IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT 'Y' AS ok, p_message AS message, p_id AS id FROM dual;
  END;

  PROCEDURE P_RESULT_FAIL(io_cursor OUT t_cursor, p_message IN VARCHAR2) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT 'N' AS ok, p_message AS message, CAST(NULL AS NUMBER) AS id
        FROM dual;
  END;

  ----------------------------------------------------------------------
  -- ACCUSATIONS
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSATIONS(p_complaint_id IN NUMBER,
                                  io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT a.accusation_id,
             a.complaint_id,
             a.accusation_text,
             a.sort_order,
             a.status,
             a.created_by,
             a.created_on,
             a.updated_by,
             a.updated_on
        FROM t_au_iid_inq_accusations a
       WHERE a.complaint_id = p_complaint_id
         AND a.status = 'ACTIVE'
       ORDER BY a.sort_order, a.accusation_id;
  END;

  PROCEDURE P_ADD_INQ_ACCUSATION(p_complaint_id    IN NUMBER,
                                 p_accusation_text IN CLOB,
                                 p_sort_order      IN NUMBER,
                                 p_created_by      IN NUMBER,
                                 io_cursor         OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_accusations
      (complaint_id,
       accusation_text,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_accusation_text,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING accusation_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'Accusation added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add accusation: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_ACCUSATION(p_accusation_id   IN NUMBER,
                                    p_accusation_text IN CLOB,
                                    p_sort_order      IN NUMBER,
                                    p_updated_by      IN NUMBER,
                                    io_cursor         OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accusations
       SET accusation_text = p_accusation_text,
           sort_order      = NVL(p_sort_order, sort_order),
           updated_by      = p_updated_by,
           updated_on      = SYSDATE
     WHERE accusation_id = p_accusation_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accusation found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accusation updated.', p_accusation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update accusation: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_ACCUSATION(p_accusation_id IN NUMBER,
                                    p_updated_by    IN NUMBER,
                                    io_cursor       OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accusations
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE accusation_id = p_accusation_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accusation found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accusation deleted.', p_accusation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete accusation: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- ACCUSED LIST
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSED_LIST(p_complaint_id IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT x.accused_row_id,
             x.complaint_id,
             x.person_name,
             x.designation,
             x.role_type,
             x.ppno_number,
             x.cnic,
             x.FATHER_NAME,
             x.remarks,
             x.sort_order,
             x.status,
             x.created_by,
             x.created_on,
             x.updated_by,
             x.updated_on
        FROM t_au_iid_inq_accused_list x
       WHERE x.complaint_id = p_complaint_id
         AND x.status = 'ACTIVE'
       ORDER BY x.sort_order, x.accused_row_id;
  END;

  PROCEDURE P_ADD_INQ_ACCUSED(p_complaint_id IN NUMBER,
                              p_person_name  IN VARCHAR2,
                              p_designation  IN VARCHAR2,
                              p_role_type    IN VARCHAR2,
                              p_ppno_number  IN VARCHAR2,
                              p_cnic         IN VARCHAR2,
                              p_Father_name  IN VARCHAR2,
                              p_remarks      IN VARCHAR2,
                              p_sort_order   IN NUMBER,
                              p_created_by   IN NUMBER,
                              io_cursor      OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_accused_list
      (complaint_id,
       person_name,
       designation,
       role_type,
       ppno_number,
       cnic,
       FATHER_NAME,
       remarks,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_person_name,
       p_designation,
       upper(p_role_type),
       p_ppno_number,
       p_cnic,
       p_Father_name,
       p_remarks,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING accused_row_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'Accused row added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add accused row: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_person_name    IN VARCHAR2,
                                 p_designation    IN VARCHAR2,
                                 p_role_type      IN VARCHAR2,
                                 p_ppno_number    IN VARCHAR2,
                                 p_cnic           IN VARCHAR2,
                                 p_FATHER_NAME    IN VARCHAR2,
                                 p_remarks        IN VARCHAR2,
                                 p_sort_order     IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accused_list
       SET person_name = p_person_name,
           designation = p_designation,
           role_type   = p_role_type,
           ppno_number = p_ppno_number,
           cnic        = p_cnic,
           FATHER_NAME = p_FATHER_NAME,
           remarks     = p_remarks,
           sort_order  = NVL(p_sort_order, sort_order),
           updated_by  = p_updated_by,
           updated_on  = SYSDATE
     WHERE accused_row_id = p_accused_row_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accused row found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accused row updated.', p_accused_row_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update accused row: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accused_list
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE accused_row_id = p_accused_row_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accused row found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accused row deleted.', p_accused_row_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete accused row: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- RECORD SCRUTINIZED
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_RECORDS(p_complaint_id IN NUMBER,
                              io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT r.rec_id,
             r.complaint_id,
             r.record_title,
             r.record_details,
             r.sort_order,
             r.status,
             r.created_by,
             r.created_on,
             r.updated_by,
             r.updated_on
        FROM t_au_iid_inq_record_scrutinized r
       WHERE r.complaint_id = p_complaint_id
         AND r.status = 'ACTIVE'
       ORDER BY r.sort_order, r.rec_id;
  END;

  PROCEDURE P_ADD_INQ_RECORD(p_complaint_id   IN NUMBER,
                             p_record_title   IN VARCHAR2,
                             p_record_details IN VARCHAR2,
                             p_sort_order     IN NUMBER,
                             p_created_by     IN NUMBER,
                             io_cursor        OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_record_scrutinized
      (complaint_id,
       record_title,
       record_details,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_record_title,
       p_record_details,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING rec_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'Record added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add record: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_RECORD(p_rec_id         IN NUMBER,
                                p_record_title   IN VARCHAR2,
                                p_record_details IN VARCHAR2,
                                p_sort_order     IN NUMBER,
                                p_updated_by     IN NUMBER,
                                io_cursor        OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_record_scrutinized
       SET record_title   = p_record_title,
           record_details = p_record_details,
           sort_order     = NVL(p_sort_order, sort_order),
           updated_by     = p_updated_by,
           updated_on     = SYSDATE
     WHERE rec_id = p_rec_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active record found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Record updated.', p_rec_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update record: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_RECORD(p_rec_id     IN NUMBER,
                                p_updated_by IN NUMBER,
                                io_cursor    OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_record_scrutinized
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE rec_id = p_rec_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active record found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Record deleted.', p_rec_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete record: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- STATEMENTS REGISTER
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_STATEMENTS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT s.statement_id,
             s.complaint_id,
             s.person_name,
             s.role_type,
             s.ppno_number,
             s.cnic,
             s.statement_datetime,
             s.place,
             s.mode_type,
             s.key_points,
             s.status,
             s.created_by,
             s.created_on,
             s.updated_by,
             s.updated_on
        FROM t_au_iid_inq_statements s
       WHERE s.complaint_id = p_complaint_id
         AND s.status = 'ACTIVE'
       ORDER BY NVL(s.statement_datetime, DATE '1900-01-01'),
                s.statement_id;
  END;

  PROCEDURE P_ADD_INQ_STATEMENT(p_complaint_id       IN NUMBER,
                                p_person_name        IN VARCHAR2,
                                p_role_type          IN VARCHAR2,
                                p_ppno_number        IN VARCHAR2,
                                p_cnic               IN VARCHAR2,
                                p_statement_datetime IN DATE,
                                p_place              IN VARCHAR2,
                                p_mode_type          IN VARCHAR2,
                                p_key_points         IN CLOB,
                                P_UPLOADED_STATEMENT in Clob,
                                P_USER_ID            IN NUMBER,
                                io_cursor            OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    v_id := NULL;
  
    ------------------------------------------------------------------
    -- 1) If PPNO is available: update existing row for (complaint + ppno)
    ------------------------------------------------------------------
    IF p_ppno_number IS NOT NULL THEN
    
      UPDATE t_au_iid_inq_statements s
         SET s.person_name        = p_person_name,
             s.role_type          = p_role_type,
             s.cnic               = p_cnic,
             s.statement_datetime = p_statement_datetime,
             s.place              = p_place,
             s.mode_type          = p_mode_type,
             s.key_points         = p_key_points,
             s.uploaded_statement = P_UPLOADED_STATEMENT
       WHERE s.complaint_id = p_complaint_id
         AND s.ppno_number = p_ppno_number
         AND s.status = 'ACTIVE';
    
      IF SQL%ROWCOUNT > 0 THEN
        SELECT s.statement_id
          INTO v_id
          FROM t_au_iid_inq_statements s
         WHERE s.complaint_id = p_complaint_id
           AND s.ppno_number = p_ppno_number
           AND s.status = 'ACTIVE';
      
        P_RESULT_OK(io_cursor, 'Statement updated.', v_id);
        RETURN;
      END IF;
    
      ------------------------------------------------------------------
      -- 2) Else fallback to CNIC: update existing row for (complaint + cnic)
      ------------------------------------------------------------------
    ELSIF p_cnic IS NOT NULL THEN
    
      UPDATE t_au_iid_inq_statements s
         SET s.person_name        = p_person_name,
             s.role_type          = p_role_type,
             s.ppno_number        = p_ppno_number,
             s.statement_datetime = p_statement_datetime,
             s.place              = p_place,
             s.mode_type          = p_mode_type,
             s.key_points         = p_key_points,
             s.uploaded_statement = P_UPLOADED_STATEMENT
       WHERE s.complaint_id = p_complaint_id
         AND s.cnic = p_cnic
         AND s.status = 'ACTIVE';
    
      IF SQL%ROWCOUNT > 0 THEN
        SELECT s.statement_id
          INTO v_id
          FROM t_au_iid_inq_statements s
         WHERE s.complaint_id = p_complaint_id
           AND s.cnic = p_cnic
           AND s.status = 'ACTIVE';
      
        P_RESULT_OK(io_cursor, 'Statement updated.', v_id);
        RETURN;
      END IF;
    
    END IF;
  
    ------------------------------------------------------------------
    -- 3) If no existing row found, INSERT new
    ------------------------------------------------------------------
    INSERT INTO t_au_iid_inq_statements
      (statement_id,
       complaint_id,
       person_name,
       role_type,
       ppno_number,
       cnic,
       statement_datetime,
       place,
       mode_type,
       key_points,
       status,
       created_by,
       created_on)
    VALUES
      (seq_iid_inq_statements.nextval,
       p_complaint_id,
       p_person_name,
       upper(p_role_type),
       p_ppno_number,
       p_cnic,
       p_statement_datetime,
       p_place,
       p_mode_type,
       p_key_points,
       'ACTIVE',
       P_USER_ID,
       SYSDATE)
    RETURNING statement_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'Statement added.', v_id);
  
  EXCEPTION
    WHEN TOO_MANY_ROWS THEN
      P_RESULT_FAIL(io_cursor,
                    'Duplicate statement exists for this complaint/person. Please clean duplicates.');
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to add/update statement: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_STATEMENT(p_statement_id       IN NUMBER,
                                   p_person_name        IN VARCHAR2,
                                   p_role_type          IN VARCHAR2,
                                   p_ppno_number        IN VARCHAR2,
                                   p_cnic               IN VARCHAR2,
                                   p_statement_datetime IN DATE,
                                   p_place              IN VARCHAR2,
                                   p_mode_type          IN VARCHAR2,
                                   p_key_points         IN CLOB,
                                   P_UPLOADED_STATEMENT in CLOB,
                                   p_updated_by         IN NUMBER,
                                   io_cursor            OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_statements
       SET person_name        = p_person_name,
           role_type          = p_role_type,
           ppno_number        = p_ppno_number,
           cnic               = p_cnic,
           statement_datetime = p_statement_datetime,
           place              = p_place,
           mode_type          = p_mode_type,
           key_points         = p_key_points,
           uploaded_statement = P_UPLOADED_STATEMENT,
           updated_by         = p_updated_by,
           updated_on         = SYSDATE
     WHERE statement_id = p_statement_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active statement found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Statement updated.', p_statement_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update statement: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_STATEMENT(p_statement_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_statements
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE statement_id = p_statement_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active statement found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Statement deleted.', p_statement_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete statement: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- EVIDENCE FILES
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_EVIDENCE_FILES(p_complaint_id IN NUMBER,
                                     io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT e.evidence_id,
             e.complaint_id,
             e.evidence_type,
             e.description,
             e.file_name,
             e.file_path,
             e.file_ext,
             e.file_size_kb,
             e.status,
             e.uploaded_by,
             e.uploaded_on,
             e.updated_by,
             e.updated_on
        FROM t_au_iid_inq_evidence_files e
       WHERE e.complaint_id = p_complaint_id
         AND e.status = 'ACTIVE'
       ORDER BY e.uploaded_on DESC, e.evidence_id DESC;
  END;

  PROCEDURE P_ADD_INQ_EVIDENCE_FILE(p_complaint_id  IN NUMBER,
                                    p_evidence_type IN VARCHAR2,
                                    p_description   IN VARCHAR2,
                                    p_file_name     IN VARCHAR2,
                                    p_file_path     IN VARCHAR2,
                                    p_file_ext      IN VARCHAR2,
                                    p_file_size_kb  IN NUMBER,
                                    p_uploaded_by   IN NUMBER,
                                    io_cursor       OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_evidence_files
      (complaint_id,
       evidence_type,
       description,
       file_name,
       file_path,
       file_ext,
       file_size_kb,
       status,
       uploaded_by,
       uploaded_on)
    VALUES
      (p_complaint_id,
       p_evidence_type,
       p_description,
       p_file_name,
       p_file_path,
       p_file_ext,
       p_file_size_kb,
       'ACTIVE',
       p_uploaded_by,
       SYSDATE)
    RETURNING evidence_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'Evidence file added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add evidence file: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_EVIDENCE_FILE(p_evidence_id IN NUMBER,
                                       p_updated_by  IN NUMBER,
                                       io_cursor     OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_evidence_files
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE evidence_id = p_evidence_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active evidence file found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Evidence file deleted.', p_evidence_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to delete evidence file: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- VIOLATIONS
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_VIOLATIONS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT v.violation_id,
             v.complaint_id,
             v.category,
             v.violation_detail,
             v.reference_text,
             v.recommendation,
             v.sort_order,
             v.status,
             v.created_by,
             v.created_on,
             v.updated_by,
             v.updated_on
        FROM t_au_iid_inq_violations v
       WHERE v.complaint_id = p_complaint_id
         AND v.status = 'ACTIVE'
       ORDER BY v.category, v.sort_order, v.violation_id;
  END;

  PROCEDURE P_GET_INQ_VIOLATION_STEP(P_COMPLAINT_ID IN NUMBER,
                                     IO_CURSOR      OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT COMPLAINT_ID,
             CONCLUSION,
             REPORTED_IN_AUDIT_REPORT,
             AUDIT_REPORT_REFERENCE_DETAIL,
             STATUS,
             CREATED_BY,
             CREATED_ON,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_IID_INQ_VIOLATION_STEP
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND NVL(STATUS, 'A') = 'A';
  END P_GET_INQ_VIOLATION_STEP;

  PROCEDURE P_SAVE_INQ_VIOLATION_STEP(P_COMPLAINT_ID                  IN NUMBER,
                                      P_CONCLUSION                    IN CLOB,
                                      P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                                      P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                                      P_UPDATED_BY                    IN NUMBER,
                                      IO_CURSOR                       OUT T_CURSOR) IS
  BEGIN
    MERGE INTO T_AU_IID_INQ_VIOLATION_STEP t
    USING (SELECT P_COMPLAINT_ID AS COMPLAINT_ID FROM dual) s
    ON (t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.CONCLUSION                    = P_CONCLUSION,
             t.REPORTED_IN_AUDIT_REPORT      = CASE
                                                 WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                                                  'Y'
                                                 ELSE
                                                  'N'
                                               END,
             t.AUDIT_REPORT_REFERENCE_DETAIL = CASE
                                                 WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                                                  P_AUDIT_REPORT_REFERENCE_DETAIL
                                                 ELSE
                                                  NULL
                                               END,
             t.STATUS                        = 'A',
             t.UPDATED_BY                    = P_UPDATED_BY,
             t.UPDATED_ON                    = SYSDATE WHEN NOT MATCHED THEN INSERT(COMPLAINT_ID, CONCLUSION, REPORTED_IN_AUDIT_REPORT, AUDIT_REPORT_REFERENCE_DETAIL, STATUS, CREATED_BY, CREATED_ON) VALUES(P_COMPLAINT_ID, P_CONCLUSION,CASE
               WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                'Y'
               ELSE
                'N'
             END,CASE
               WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                P_AUDIT_REPORT_REFERENCE_DETAIL
               ELSE
                NULL
             END, 'A', P_UPDATED_BY, SYSDATE);
  
    P_RESULT_OK(IO_CURSOR, 'Violation step summary saved.', P_COMPLAINT_ID);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(IO_CURSOR,
                    'Failed to save violation step summary: ' || SQLERRM);
  END P_SAVE_INQ_VIOLATION_STEP;

  PROCEDURE GET_INQ_FIND_RECOMM_STATUS(p_complaint_id IN NUMBER,
                                       io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
    
    /* 1) Real accusations for this complaint */
      SELECT a.accusation_id,
             a.accusation_text,
             CASE
               WHEN r.complaint_id IS NOT NULL THEN
                'Y'
               ELSE
                'N'
             END AS is_saved,
             NVL(r.updated_on, r.created_on) AS saved_on
        FROM (SELECT accusation_id, accusation_text
                FROM t_au_iid_inq_accusations -- <-- replace with your actual accusations source
               WHERE complaint_id = p_complaint_id) a
        LEFT JOIN t_au_iid_inq_find_recomm r
          ON r.complaint_id = p_complaint_id
         AND r.accusation_id = a.accusation_id
       ORDER BY a.accusation_id;
  
    /* Optional: include Additional Charges inside cursor as well.
    If you want it in DB output, use UNION ALL approach below instead.
    Otherwise your C# already inserts it if missing. */
  
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to load findings/recommendation status: ' ||
                    SQLERRM);
  END;

  PROCEDURE SAVE_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                     p_accusation_id IN NUMBER, -- 0 = Additional Charges
                                     p_finding_text  IN CLOB,
                                     p_recom_text    IN CLOB,
                                     p_outcome       in varchar2,
                                     p_ppno          IN VARCHAR2,
                                     io_cursor       OUT t_cursor) IS
  BEGIN
    MERGE INTO T_AU_IID_INQ_FIND_RECOMM t
    USING (SELECT p_complaint_id  AS complaint_id,
                  p_accusation_id AS accusation_id
             FROM dual) s
    ON (t.complaint_id = s.complaint_id AND t.accusation_id = s.accusation_id)
    WHEN MATCHED THEN
      UPDATE
         SET t.findings_text       = p_finding_text,
             t.recommendation_text = p_recom_text,
             t.updated_by          = p_ppno,
             t.updated_on          = SYSDATE,
             t.accusation          = p_outcome
    WHEN NOT MATCHED THEN
      INSERT
        (complaint_id,
         accusation_id,
         accusation,
         findings_text,
         recommendation_text,
         status,
         created_by,
         created_on)
      VALUES
        (p_complaint_id,
         p_accusation_id,
         p_outcome,
         p_finding_text,
         p_recom_text,
         'ACTIVE',
         p_ppno,
         SYSDATE);
  
    P_RESULT_OK(io_cursor,
                'Findings & recommendation saved.',
                p_complaint_id);
  
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to save findings/recommendation: ' || SQLERRM);
  END;

  PROCEDURE GET_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                    p_accusation_id IN NUMBER,
                                    io_cursor       OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT r.complaint_id,
             r.accusation_id,
             r.findings_text AS finding_text,
             r.recommendation_text AS recom_text,
             r.accusation as OUTCOME,
             NVL(r.updated_by, r.created_by) AS ppno,
             NVL(r.updated_on, r.created_on) AS updated_on
        FROM T_AU_IID_INQ_FIND_RECOMM r
       WHERE r.complaint_id = p_complaint_id;
  
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to load findings/recommendation: ' || SQLERRM);
  END;

  PROCEDURE P_ADD_INQ_VIOLATION(p_complaint_id     IN NUMBER,
                                p_category         IN VARCHAR2,
                                p_violation_detail IN CLOB,
                                p_reference_text   IN VARCHAR2,
                                p_recommendation   IN CLOB,
                                p_sort_order       IN NUMBER,
                                p_created_by       IN NUMBER,
                                io_cursor          OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_violations
      (complaint_id,
       category,
       violation_detail,
       reference_text,
       recommendation,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_category,
       p_violation_detail,
       p_reference_text,
       p_recommendation,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING violation_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'Violation added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add violation: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_VIOLATION(p_violation_id     IN NUMBER,
                                   p_category         IN VARCHAR2,
                                   p_violation_detail IN CLOB,
                                   p_reference_text   IN VARCHAR2,
                                   p_recommendation   IN CLOB,
                                   p_sort_order       IN NUMBER,
                                   p_updated_by       IN NUMBER,
                                   io_cursor          OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_violations
       SET category         = p_category,
           violation_detail = p_violation_detail,
           reference_text   = p_reference_text,
           recommendation   = p_recommendation,
           sort_order       = NVL(p_sort_order, sort_order),
           updated_by       = p_updated_by,
           updated_on       = SYSDATE
     WHERE violation_id = p_violation_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active violation found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Violation updated.', p_violation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update violation: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_VIOLATION(p_violation_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_violations
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE violation_id = p_violation_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active violation found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Violation deleted.', p_violation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete violation: ' || SQLERRM);
  END;

  PROCEDURE P_GET_ALLOWED_PDF_ENG_DETAILS(P_PP_NO  IN NUMBER,
                                          P_R_ID   IN NUMBER,
                                          P_ENT_ID IN NUMBER,
                                          O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    /*
      Returns list of ENG_IDs allowed for the logged-in user to generate PDF,
      along with engagement/entity details.
    
      Roles logic:
      - Admin roles (1,2): all FINAL reports
      - Department Head / Incharge (15,16): FINAL reports for P_ENT_ID
      - Others: FINAL reports where user exists in t_au_audit_team_tasklist
    */
  
    IF P_R_ID IN (1, 2) THEN
      OPEN O_CURSOR FOR
        SELECT m.eng_id AS ENG_ID,
               
               /* ===== Replace these entity columns if your IAS uses different entity view/table ===== */
               NVL(e.p_name, '-') AS REPORTING_OFFICE,
               e.c_name AS ENTITY_NAME,
               /* ================================================================================ */
               
               pe.audit_startdate AS AUDIT_START_DATE,
               pe.audit_enddate   AS AUDIT_END_DATE,
               m.report_status    AS REPORT_STATUS,
               m.report_version   AS REPORT_VERSION
          FROM t_frpt_report_meta m
          JOIN t_au_plan_eng pe
            ON pe.eng_id = m.eng_id
        
        /* ===== Replace this join with your actual entity master/view if needed ===== */
          LEFT JOIN t_auditee_entities_maping e
            ON e.entity_id = pe.entity_id
        /* ======================================================================== */
        
         WHERE m.report_status = 'FINAL'
         ORDER BY m.eng_id DESC;
    
    ELSIF P_R_ID IN (15, 16) THEN
      OPEN O_CURSOR FOR
        SELECT m.eng_id AS ENG_ID,
               NVL(e.p_name, '-') AS REPORTING_OFFICE,
               e.c_name AS ENTITY_NAME,
               pe.audit_startdate AS AUDIT_START_DATE,
               pe.audit_enddate AS AUDIT_END_DATE,
               m.report_status AS REPORT_STATUS,
               m.report_version AS REPORT_VERSION
          FROM t_frpt_report_meta m
          JOIN t_au_plan_eng pe
            ON pe.eng_id = m.eng_id
          LEFT JOIN t_auditee_entities_maping e
            ON e.entity_id = pe.entity_id
         WHERE m.report_status = 'FINAL'
           AND pe.entity_id = P_ENT_ID
         ORDER BY m.eng_id DESC;
    
    ELSE
      OPEN O_CURSOR FOR
        SELECT DISTINCT m.eng_id AS ENG_ID,
                        NVL(e.p_name, '-') AS REPORTING_OFFICE,
                        e.c_name AS ENTITY_NAME,
                        pe.audit_startdate AS AUDIT_START_DATE,
                        pe.audit_enddate AS AUDIT_END_DATE,
                        m.report_status AS REPORT_STATUS,
                        m.report_version AS REPORT_VERSION
          FROM t_frpt_report_meta m
          JOIN t_au_plan_eng pe
            ON pe.eng_id = m.eng_id
          LEFT JOIN t_auditee_entities_maping e
            ON e.entity_id = pe.entity_id
          JOIN t_au_audit_team_tasklist x
            ON x.eng_plan_id = m.eng_id
           AND x.teammember_ppno = P_PP_NO
         WHERE m.report_status = 'FINAL'
           AND NVL(x.isactive, 1) = 1
         ORDER BY m.eng_id DESC;
    END IF;
  
  EXCEPTION
    WHEN OTHERS THEN
      -- return empty cursor instead of failing UI
      OPEN O_CURSOR FOR
        SELECT CAST(NULL AS NUMBER) AS ENG_ID,
               CAST(NULL AS VARCHAR2(200)) AS REPORTING_OFFICE,
               CAST(NULL AS VARCHAR2(200)) AS ENTITY_NAME,
               CAST(NULL AS DATE) AS AUDIT_START_DATE,
               CAST(NULL AS DATE) AS AUDIT_END_DATE,
               CAST(NULL AS VARCHAR2(20)) AS REPORT_STATUS,
               CAST(NULL AS NUMBER) AS REPORT_VERSION
          FROM dual
         WHERE 1 = 0;
  END P_GET_ALLOWED_PDF_ENG_DETAILS;

  ----------------------------------------------------------------------
  -- DSA
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_DSA(p_complaint_id IN NUMBER, io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT d.dsa_id,
             d.complaint_id,
             d.person_name,
             d.designation,
             d.ppno_number,
             d.cnic,
             d.dsa_status,
             d.remarks,
             d.sort_order,
             d.status,
             d.created_by,
             d.created_on,
             d.updated_by,
             d.updated_on
        FROM t_au_iid_inq_dsa d
       WHERE d.complaint_id = p_complaint_id
         AND d.status = 'ACTIVE'
       ORDER BY d.sort_order, d.dsa_id;
  END;

  PROCEDURE P_ADD_INQ_DSA(p_complaint_id IN NUMBER,
                          p_person_name  IN VARCHAR2,
                          p_designation  IN VARCHAR2,
                          p_ppno_number  IN VARCHAR2,
                          p_cnic         IN VARCHAR2,
                          p_dsa_status   IN VARCHAR2,
                          p_remarks      IN VARCHAR2,
                          p_sort_order   IN NUMBER,
                          p_created_by   IN NUMBER,
                          io_cursor      OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_dsa
      (complaint_id,
       person_name,
       designation,
       ppno_number,
       cnic,
       dsa_status,
       remarks,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_person_name,
       p_designation,
       p_ppno_number,
       p_cnic,
       NVL(p_dsa_status, 'DRAFT'),
       p_remarks,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING dsa_id INTO v_id;
  
    P_RESULT_OK(io_cursor, 'DSA row added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add DSA row: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_DSA(p_dsa_id      IN NUMBER,
                             p_person_name IN VARCHAR2,
                             p_designation IN VARCHAR2,
                             p_ppno_number IN VARCHAR2,
                             p_cnic        IN VARCHAR2,
                             p_dsa_status  IN VARCHAR2,
                             p_remarks     IN VARCHAR2,
                             p_sort_order  IN NUMBER,
                             p_updated_by  IN NUMBER,
                             io_cursor     OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_dsa
       SET person_name = p_person_name,
           designation = p_designation,
           ppno_number = p_ppno_number,
           cnic        = p_cnic,
           dsa_status  = p_dsa_status,
           remarks     = p_remarks,
           sort_order  = NVL(p_sort_order, sort_order),
           updated_by  = p_updated_by,
           updated_on  = SYSDATE
     WHERE dsa_id = p_dsa_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active DSA row found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'DSA row updated.', p_dsa_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update DSA row: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_DSA(p_dsa_id     IN NUMBER,
                             p_updated_by IN NUMBER,
                             io_cursor    OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_dsa
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE dsa_id = p_dsa_id
       AND status = 'ACTIVE';
  
    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active DSA row found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'DSA row deleted.', p_dsa_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete DSA row: ' || SQLERRM);
  END;

  PROCEDURE P_GET_INQ_EVIDENCE_STEP(P_COMPLAINT_ID IN NUMBER,
                                    IO_CURSOR      OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT COMPLAINT_ID,
             MATERIAL_EVIDENCE_DETAIL,
             CIRCUMSTANTIAL_EVIDENCE_DETAIL,
             STATUS,
             CREATED_BY,
             CREATED_ON,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_IID_INQ_EVIDENCE_STEP
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND NVL(STATUS, 'A') = 'A';
  END P_GET_INQ_EVIDENCE_STEP;

  PROCEDURE P_SAVE_INQ_EVIDENCE_STEP(P_COMPLAINT_ID                   IN NUMBER,
                                     P_MATERIAL_EVIDENCE_DETAIL       IN CLOB,
                                     P_CIRCUMSTANTIAL_EVIDENCE_DETAIL IN CLOB,
                                     io_cursor                        OUT t_cursor) AS
    V_EXISTS NUMBER := 0;
    v_msg    VARCHAR2(4000);
  BEGIN
    SELECT COUNT(*)
      INTO V_EXISTS
      FROM T_AU_IID_INQ_EVIDENCE_STEP
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  
    IF V_EXISTS = 0 THEN
      INSERT INTO T_AU_IID_INQ_EVIDENCE_STEP
        (COMPLAINT_ID,
         MATERIAL_EVIDENCE_DETAIL,
         CIRCUMSTANTIAL_EVIDENCE_DETAIL,
         STATUS,
         CREATED_BY,
         CREATED_ON)
      VALUES
        (P_COMPLAINT_ID,
         P_MATERIAL_EVIDENCE_DETAIL,
         P_CIRCUMSTANTIAL_EVIDENCE_DETAIL,
         'A',
         0,
         SYSDATE);
    ELSE
      UPDATE T_AU_IID_INQ_EVIDENCE_STEP
         SET MATERIAL_EVIDENCE_DETAIL       = P_MATERIAL_EVIDENCE_DETAIL,
             CIRCUMSTANTIAL_EVIDENCE_DETAIL = P_CIRCUMSTANTIAL_EVIDENCE_DETAIL,
             STATUS                         = 'A',
             UPDATED_BY                     = 0,
             UPDATED_ON                     = SYSDATE
       WHERE COMPLAINT_ID = P_COMPLAINT_ID;
    END IF;
  
    v_msg := 'SUCCESS';
  
    OPEN io_cursor FOR
      SELECT v_msg AS msg, P_COMPLAINT_ID AS complaint_id FROM dual;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_msg := 'ERROR: ' || SQLERRM;
    
      OPEN io_cursor FOR
        SELECT v_msg AS msg, P_COMPLAINT_ID AS complaint_id FROM dual;
      RAISE;
  END P_SAVE_INQ_EVIDENCE_STEP;

  PROCEDURE P_GET_INQ_PROCEEDINGS(P_COMPLAINT_ID IN NUMBER,
                                  IO_CURSOR      OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT PROCEEDING_ID,
             COMPLAINT_ID,
             NOTICE_REFERENCE,
             VISIT_DATE,
             PLACE_VISITED,
             PARTICIPANTS_DETAIL,
             MISSING_PARTICIPANTS_REASON,
             SORT_ORDER,
             STATUS,
             CREATED_BY,
             CREATED_ON,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_IID_INQ_PROCEEDINGS
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND NVL(STATUS, 'A') = 'A'
       ORDER BY NVL(SORT_ORDER, 0), PROCEEDING_ID;
  END P_GET_INQ_PROCEEDINGS;

  PROCEDURE P_SAVE_INQ_PROCEEDING(P_PROCEEDING_ID               IN OUT NUMBER,
                                  P_COMPLAINT_ID                IN NUMBER,
                                  P_NOTICE_REFERENCE            IN VARCHAR2,
                                  P_VISIT_DATE                  IN DATE,
                                  P_PLACE_VISITED               IN CLOB,
                                  P_PARTICIPANTS_DETAIL         IN CLOB,
                                  P_MISSING_PARTICIPANTS_REASON IN CLOB,
                                  P_SORT_ORDER                  IN NUMBER,
                                  P_STATUS                      IN VARCHAR2,
                                  P_USER_ID                     IN NUMBER,
                                  io_cursor                     OUT t_cursor) AS
    v_msg VARCHAR2(4000);
  BEGIN
    IF NVL(P_PROCEEDING_ID, 0) = 0 THEN
      P_PROCEEDING_ID := SEQ_IID_INQ_PROCEEDINGS.NEXTVAL;
    
      INSERT INTO T_AU_IID_INQ_PROCEEDINGS
        (PROCEEDING_ID,
         COMPLAINT_ID,
         NOTICE_REFERENCE,
         VISIT_DATE,
         PLACE_VISITED,
         PARTICIPANTS_DETAIL,
         MISSING_PARTICIPANTS_REASON,
         SORT_ORDER,
         STATUS,
         CREATED_BY,
         CREATED_ON)
      VALUES
        (P_PROCEEDING_ID,
         P_COMPLAINT_ID,
         P_NOTICE_REFERENCE,
         P_VISIT_DATE,
         P_PLACE_VISITED,
         P_PARTICIPANTS_DETAIL,
         P_MISSING_PARTICIPANTS_REASON,
         NVL(P_SORT_ORDER, 0),
         NVL(P_STATUS, 'A'),
         P_USER_ID,
         SYSDATE);
    ELSE
      UPDATE T_AU_IID_INQ_PROCEEDINGS
         SET NOTICE_REFERENCE            = P_NOTICE_REFERENCE,
             VISIT_DATE                  = P_VISIT_DATE,
             PLACE_VISITED               = P_PLACE_VISITED,
             PARTICIPANTS_DETAIL         = P_PARTICIPANTS_DETAIL,
             MISSING_PARTICIPANTS_REASON = P_MISSING_PARTICIPANTS_REASON,
             SORT_ORDER                  = NVL(P_SORT_ORDER, 0),
             STATUS                      = NVL(P_STATUS, 'A'),
             UPDATED_BY                  = P_USER_ID,
             UPDATED_ON                  = SYSDATE
       WHERE PROCEEDING_ID = P_PROCEEDING_ID;
    END IF;
  
    v_msg := 'SUCCESS';
  
    OPEN io_cursor FOR
      SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_msg := 'ERROR: ' || SQLERRM;
    
      OPEN io_cursor FOR
        SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;
      RAISE;
  END P_SAVE_INQ_PROCEEDING;

  PROCEDURE P_DELETE_INQ_PROCEEDING(P_PROCEEDING_ID IN NUMBER,
                                    P_UPDATED_BY    IN NUMBER,
                                    io_cursor       OUT t_cursor) AS
    v_msg VARCHAR2(4000);
  BEGIN
    UPDATE T_AU_IID_INQ_PROCEEDINGS
       SET STATUS = 'D', UPDATED_BY = P_UPDATED_BY, UPDATED_ON = SYSDATE
     WHERE PROCEEDING_ID = P_PROCEEDING_ID;
  
    v_msg := 'SUCCESS';
  
    OPEN io_cursor FOR
      SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_msg := 'ERROR: ' || SQLERRM;
    
      OPEN io_cursor FOR
        SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;
      RAISE;
  END P_DELETE_INQ_PROCEEDING;

  PROCEDURE P_FINALIZE_IID_INQUIRY_REPORT(P_COMPLAINT_ID IN NUMBER,
                                          P_UPDATED_BY   IN NUMBER) AS
  BEGIN
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET IS_FINALIZED     = 'Y',
           FINALIZED_ON     = SYSDATE,
           UPDATED_BY_PP_NO = P_UPDATED_BY,
           UPDATED_ON       = SYSDATE
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  
    IF SQL%ROWCOUNT = 0 THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'No IID Inquiry Report found for the given Complaint ID.');
    END IF;
  END P_FINALIZE_IID_INQUIRY_REPORT;

  PROCEDURE P_ENQUEUE_EMAIL(P_EVENT_CODE IN VARCHAR2,
                            P_REF_ID1    IN NUMBER,
                            P_REF_ID2    IN NUMBER,
                            P_MAIL_TO    IN VARCHAR2,
                            P_MAIL_CC    IN VARCHAR2,
                            P_SUBJECT    IN VARCHAR2,
                            P_BODY       IN CLOB,
                            O_EMAIL_ID   OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_EMAIL_QUEUE_ID.NEXTVAL INTO O_EMAIL_ID FROM DUAL;
  
    INSERT INTO T_AU_IID_EMAIL_QUEUE
      (EMAIL_ID,
       EVENT_CODE,
       REF_ID1,
       REF_ID2,
       MAIL_TO,
       MAIL_CC,
       SUBJECT,
       BODY,
       STATUS,
       CREATED_ON,
       SENT_ON,
       ERROR_TEXT,
       RETRY_COUNT,
       CREATED_BY)
    VALUES
      (O_EMAIL_ID,
       TRIM(P_EVENT_CODE),
       P_REF_ID1,
       P_REF_ID2,
       TRIM(P_MAIL_TO),
       TRIM(P_MAIL_CC),
       P_SUBJECT,
       P_BODY,
       'PENDING',
       SYSDATE,
       NULL,
       NULL,
       0,
       NULL);
  END P_ENQUEUE_EMAIL;

  PROCEDURE P_GET_EMAIL_QUEUE(P_STATUS    IN DATE,
                              P_FROM_DATE IN DATE,
                              P_TO_DATE   IN DATE,
                              IO_CURSOR   OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT EMAIL_ID,
             EVENT_CODE,
             REF_ID1,
             REF_ID2,
             MAIL_TO,
             MAIL_CC,
             SUBJECT,
             BODY,
             STATUS,
             CREATED_ON,
             SENT_ON,
             ERROR_TEXT
        FROM T_AU_IID_EMAIL_QUEUE
       WHERE (P_STATUS IS NULL OR
             UPPER(STATUS) = UPPER(TRIM(TO_CHAR(P_STATUS))))
         AND (P_FROM_DATE IS NULL OR CREATED_ON >= TRUNC(P_FROM_DATE))
         AND (P_TO_DATE IS NULL OR CREATED_ON < TRUNC(P_TO_DATE) + 1)
       ORDER BY CASE UPPER(STATUS)
                  WHEN 'PENDING' THEN
                   1
                  WHEN 'FAILED' THEN
                   2
                  WHEN 'SENT' THEN
                   3
                  ELSE
                   4
                END,
                CREATED_ON,
                EMAIL_ID;
  END P_GET_EMAIL_QUEUE;

  PROCEDURE P_MARK_EMAIL_SENT(P_EMAIL_ID IN NUMBER) IS
  BEGIN
    UPDATE T_AU_IID_EMAIL_QUEUE
       SET STATUS = 'SENT', SENT_ON = SYSDATE, ERROR_TEXT = NULL
     WHERE EMAIL_ID = P_EMAIL_ID;
  END P_MARK_EMAIL_SENT;

  PROCEDURE P_MARK_EMAIL_FAILED(P_EMAIL_ID   IN NUMBER,
                                P_ERROR_TEXT IN VARCHAR2) IS
  BEGIN
    UPDATE T_AU_IID_EMAIL_QUEUE
       SET STATUS      = 'FAILED',
           ERROR_TEXT  = SUBSTR(P_ERROR_TEXT, 1, 2000),
           RETRY_COUNT = NVL(RETRY_COUNT, 0) + 1
     WHERE EMAIL_ID = P_EMAIL_ID;
  END P_MARK_EMAIL_FAILED;

END PKG_INQ;


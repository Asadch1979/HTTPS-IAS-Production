create or replace package PKG_AR is
  TYPE t_cursor IS REF CURSOR;

  procedure P_GetTaskList(ENT_ID    in number,
                          P_NO      in number,
                          R_ID      in number,
                          io_cursor OUT t_cursor);
  --not in use
  procedure AUDIT_JOINING(ENG_PLAN_ID     in T_AU_AUDIT_JOINING.ENG_PLAN_ID%type,
                          TEAM_MEM_PPNO   in T_AU_AUDIT_JOINING.TEAM_MEM_PPNO%type,
                          ENTEREDBY       in T_AU_AUDIT_JOINING.ENTEREDBY%type,
                          JOINING_DATE    in T_AU_AUDIT_JOINING.JOINING_DATE%type,
                          COMPLETION_DATE in T_AU_AUDIT_JOINING.COMPLETION_DATE%type);

  procedure P_AddJoiningReport(ENGID           in number,
                               ENT_ID          in number,
                               P_NO            in number,
                               R_ID            in number,
                               COMPLETION_DATE in date,
                               ENT_EMAIL_ADD   in varchar2,
                               ENT_PHONE_NO    in varchar2,
                               io_cursor       OUT t_cursor);

  procedure P_SetEngIdOnHold(ENGID IN NUMBER, ppno in number);

  procedure P_GetJoiningDetails(ENG       in number,
                                ENT_ID    in number,
                                P_NO      in number,
                                R_ID      in number,
                                io_cursor OUT t_cursor);

  procedure P_GetAuditChecklist(io_cursor OUT t_cursor);

  procedure P_GetAuditChecklistCAD(io_cursor OUT t_cursor);

  procedure p_GetAuditChecklistSub(tid       in number,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure P_GetAuditChecklistDetails(sid       in number,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor);

  PROCEDURE P_getauditeecheckklist(PLANID         IN NUMBER,
                                   SUBCHECKLISTID IN NUMBER,
                                   io_cursor      OUT t_cursor);

  procedure P_GetRiskGroup(io_cursor OUT t_cursor);

  procedure P_GetRiskSubGroup(group_id IN NUMBER, io_cursor OUT t_cursor);

  procedure p_GetRiskActivities(Sub_group_id IN NUMBER,
                                io_cursor    OUT t_cursor);

  procedure P_GetAuditVoilationcats(io_cursor OUT t_cursor);

  procedure P_GetVoilationSubGroup(group_id  in number,
                                   io_cursor OUT t_cursor);

  procedure P_GetAuditChecklistDetails_search(io_cursor OUT t_cursor);

  procedure P_get_employees_information(P_NO      in number,
                                        io_cursor OUT t_cursor);
  PROCEDURE P_GET_ENB_CIRCULARS(p_text VARCHAR2, io_cursor OUT t_cursor);

  procedure P_SaveAuditObservationCAD(PLANID            in T_AU_OBSERVATION.ENGPLANID%type,
                                      STATUS            in T_AU_OBSERVATION.STATUS%type,
                                      REPLYDATE         in T_AU_OBSERVATION.REPLYDATE%type,
                                      ENTEREDBY         in T_AU_OBSERVATION.Enteredby%type,
                                      Severity          in T_AU_OBSERVATION.Severity%type,
                                      SUBCHECKLISTID    in T_AU_OBSERVATION.Subchecklist_Id%type,
                                      CHECKLISTDETAILID in T_AU_OBSERVATION.Checklistdetail_Id%type,
                                      TEXT_DATA         in T_AU_OBSERVATION_TEXT.TEXT%type,
                                      TITLE             in varchar2,
                                      AMOUNT_INV        in number,
                                      NO_INST           in number,
                                      BRANCHID          IN NUMBER,
                                      ENT_ID            in number,
                                      P_NO              in number,
                                      R_ID              in number,
                                      ANNEX_ID          IN NUMBER,
                                      io_cursor         OUT t_cursor);

  procedure P_SaveAuditObservation(PLANID            in T_AU_OBSERVATION.ENGPLANID%type,
                                   STATUS            in T_AU_OBSERVATION.STATUS%type,
                                   REPLYDATE         in T_AU_OBSERVATION.REPLYDATE%type,
                                   ENTEREDBY         in T_AU_OBSERVATION.Enteredby%type,
                                   Severity          in T_AU_OBSERVATION.Severity%type,
                                   SUBCHECKLISTID    in T_AU_OBSERVATION.Subchecklist_Id%type,
                                   CHECKLISTDETAILID in T_AU_OBSERVATION.Checklistdetail_Id%type,
                                   VCATID            in T_AU_OBSERVATION.v_Cat_Id%type,
                                   VCATNATUREID      in T_AU_OBSERVATION.v_Cat_Nature_Id%type,
                                   TEXT_DATA         in T_AU_OBSERVATION_TEXT.TEXT%type,
                                   NOINSTANCES       in t_au_observation.no_of_instances%type,
                                   AMOUNT_INV        in number,
                                   TITLE             IN VARCHAR2,
                                   OT_ENT_ID         in number,
                                   ENT_ID            in number,
                                   P_NO              in number,
                                   R_ID              in number,
                                   ANNEX_ID          IN NUMBER,
                                   io_cursor         OUT t_cursor);

  procedure P_responibilityassigned(N_ID      IN NUMBER,
                                    E_ID      IN NUMBER,
                                    IND       in varchar2,
                                    PPNO      IN NUMBER,
                                    RES_PP    IN NUMBER,
                                    LOANCASE  IN NUMBER,
                                    ACCNUMBER IN NUMBER,
                                    LCAMOUNT  IN NUMBER,
                                    ACAMOUNT  IN NUMBER,
                                    io_cursor OUT t_cursor);

  procedure P_UpdateObservation(OBS_ID       in number,
                                title        in varchar2,
                                obtext       in clob,
                                subprocessid in number,
                                checklistid  in number,
                                RiskID       in number,
                                AnnexureID   in number,
                                ENT_ID       in number,
                                P_NO         in number,
                                R_ID         in number,
                                io_cursor    OUT t_cursor);

  /*  PROCEDURE P_MERGE_ANNEXURE_INSTRUCTIONS(IND                   in Varchar2,
  p_annexure_id         IN NUMBER,
  p_reference_type_id   IN NUMBER,
  p_reference_type      IN VARCHAR2,
  p_instruction_title   IN VARCHAR2,
  p_instruction_date    IN DATE,
  p_instruction_details IN Clob,
  p_division_id         IN NUMBER,
  o_status              OUT VARCHAR2,
  o_annexure_id         OUT NUMBER);*/

  procedure P_DropAuditObservation(OBS_ID    IN NUMBER,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure p_get_auditee_submission_list(ENT_ID    in number,
                                          io_cursor OUT t_cursor);

  procedure P_SubmitAuditObservationToAuditee(OBS_ID    IN NUMBER,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor);

  procedure P_GetLatestAuditeeResponse(obs_id    IN NUMBER,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor);

  procedure P_GetOBSERVATIONSAUDITEERESPONSE(OBS_ID    in number,
                                             ENT_ID    in number,
                                             P_NO      in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor);

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences(resp_id   in t_au_observations_auditee_evidences.respid%type,
                                                         io_cursor OUT t_cursor);

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_by_obs_id(OBS_ID    in t_au_observation.id%type,
                                                                   io_cursor OUT t_cursor);

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_FileData(FILE_ID   in varchar2,
                                                                  io_cursor OUT t_cursor);

  procedure P_UpdateAuditObservationStatus(OBS_ID        IN NUMBER,
                                           NEW_STATUS_ID IN NUMBER,
                                           D_PARA_NO     in varchar2,
                                           Remarks       IN VARCHAR2,
                                           ENT_ID        in number,
                                           P_NO          in number,
                                           R_ID          in number,
                                           io_cursor     OUT t_cursor);

  procedure AUDITOR_RESPONSE(OBS_ID          IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.AU_OBS_ID%type,
                             PPNumber        IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.RECO_BY%TYPE,
                             AUDITOR_COMMENT IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.RECOMMENDATION%type,
                             status          IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.STATUS%type);

  procedure AUDITOR_REPLY(OBS_ID          IN T_AU_OBSERVATIONS_AUDITOR_REPLY.AU_OBS_ID%type,
                          PPNumber        IN T_AU_OBSERVATIONS_AUDITOR_REPLY.REPLIEDBY%TYPE,
                          AUDITOR_COMMENT IN T_AU_OBSERVATIONS_AUDITOR_REPLY.AUDIT_REPLY%type,
                          status          IN T_AU_OBSERVATIONS_AUDITOR_REPLY.OBS_STATUS%type);

  procedure P_GetLatestAuditorResponse(obs_id    IN NUMBER,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor);

  procedure P_GetLatestDepartmentalHeadResponse(obs_id    IN NUMBER,
                                                ENT_ID    in number,
                                                P_NO      in number,
                                                R_ID      in number,
                                                io_cursor OUT t_cursor);

  procedure p_GetObservationEntities(PP_NO     in number,
                                     io_cursor OUT t_cursor);

  procedure p_GetManageAuditParasEntities(P_NO      in number,
                                          R_ID      in number,
                                          ENT_ID    in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetManagedObservations(ENGID     IN NUMBER,
                                     OBSID     IN NUMBER,
                                     ENT_ID    in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor);

  procedure P_GetManagedObservationstext(OBSID     IN NUMBER,
                                         ENT_ID    in number,
                                         P_NO      in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetManagedObservationsForBranches(ENGID     IN NUMBER,
                                                OBSID     IN NUMBER,
                                                ENT_ID    in number,
                                                P_NO      in number,
                                                R_ID      in number,
                                                io_cursor OUT t_cursor);

  procedure P_GetManagedObservationsForBranchesTEXT(OBSID     IN NUMBER,
                                                    ENT_ID    in number,
                                                    P_NO      in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservations(ENGID     IN NUMBER,
                                          ENT_ID    in number,
                                          P_NO      in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor);

  Procedure P_get_details_for_manage_observations_summary(ENGID     in number,
                                                          ENT_ID    in number,
                                                          P_NO      in number,
                                                          R_ID      in number,
                                                          io_cursor OUT t_cursor);
  -- not in use
  procedure P_GetManagedDraftObservationsbranch(ENGID     IN NUMBER,
                                                io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservationsText(OBSID     IN NUMBER,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor);
  -- not in use                                              
  procedure P_GetManagedDraftObservationsreply(OBSID     IN NUMBER,
                                               io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservationsForBranches(ENGID     IN NUMBER,
                                                     ENT_ID    in number,
                                                     P_NO      in number,
                                                     R_ID      in number,
                                                     io_cursor OUT t_cursor);

  procedure p_GetClosingDraftObservations(ENGID     in number,
                                          ENT_ID    in number,
                                          P_NO      in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor);

  procedure P_Closeaudit(engid     in number,
                         ENT_ID    in number,
                         P_NO      in number,
                         R_ID      in number,
                         io_cursor OUT t_cursor);
  -- legacy
  procedure P_GetEntitiesForLegacyPara(entityId  in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor out t_cursor);

  procedure P_GetEntitiesForLegacyPara_ho(entityId  in number,
                                          io_cursor out t_cursor);

  procedure P_GetEntitiesForLegacyPara_ho_report(entityId  in number,
                                                 io_cursor out t_cursor);

  procedure P_GetLeagacyObservations_ho(entityname in varchar2,
                                        paraRef    in varchar2,
                                        ppno       in number,
                                        io_cursor  out t_cursor);

  procedure P_Settel_legacy_para_ho(RefP       in number,
                                    new_status in number,
                                    PPNO       IN NUMBER,
                                    remark     in varchar2,
                                    io_cursor  out t_cursor);

  procedure P_delete_legacy_para_ho(RefP      in number,
                                    PPNO      in number,
                                    io_cursor out t_cursor);

  procedure P_GetLeagacyObservations(entityId  in number,
                                     paraRef   in varchar2,
                                     ppno      in number,
                                     io_cursor out t_cursor);

  procedure p_get_legacy_para_responsibles(paraRef   in number,
                                           io_cursor OUT t_cursor);

  procedure P_update_legacy_Para_text(ref_id       in varchar2,
                                      obtext       in clob,
                                      process_id   in number,
                                      subprocessid in number,
                                      checklistid  in number,
                                      pp_no        in number,
                                      risk_id      in number,
                                      io_cursor    OUT t_cursor);

  procedure p_add_para_responsibility(refid        in number,
                                      PPNO         in number,
                                      AZ_Entity_id in number,
                                      user_ppno    in number,
                                      lC_no        in varchar2,
                                      LC_AMOUNT    in varchar2,
                                      AC_NO        in varchar2,
                                      AC_AMOUNT    in varchar2,
                                      refp         in varchar2,
                                      io_cursor    OUT t_cursor);

  procedure p_delete_para_responsibility(refid     in number,
                                         PPNO      in number,
                                         io_cursor OUT t_cursor);

  procedure P_no_update_legacy_Para_text(ref_id    in varchar2,
                                         ppno      in number,
                                         risk_id   in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetLeagacyObservations_for_gist_update(entityId  in number,
                                                     paraRef   in varchar2,
                                                     ppno      in number,
                                                     io_cursor out t_cursor);
  procedure P_Get_legacy_Para_to_authorize(ENTITYID  IN NUMBER,
                                           io_cursor OUT t_cursor);

  procedure P_update_legacy_Para_Gist(ref_id    in varchar2,
                                      gist      in varchar2,
                                      parano    in varchar2,
                                      pp_no     in number,
                                      u_entity  in number,
                                      io_cursor OUT t_cursor);

  procedure P_Authorize_Para_Gist(RefP      in varchar2,
                                  gist      in varchar2,
                                  parano    in varchar2,
                                  PPNO      IN NUMBER,
                                  ENTITYID  in number,
                                  io_cursor OUT t_cursor);

  --WORKING PAPER PROCEDURES
  procedure P_AddLoanCaseFile(ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              ENGID     in T_WORKING_PAPER_LOAN_CASE_FILE.ENG_ID%type,
                              LCNUMBER  in T_WORKING_PAPER_LOAN_CASE_FILE.LC_NUMBER%type,
                              LCAmount  in T_WORKING_PAPER_LOAN_CASE_FILE.AMOUNT%type,
                              DISBDATE  in T_WORKING_PAPER_LOAN_CASE_FILE.DISB_DATE%type,
                              LC        in T_WORKING_PAPER_LOAN_CASE_FILE.CATEGORY%type,
                              OBS       in T_WORKING_PAPER_LOAN_CASE_FILE.OBSERVATION%type,
                              PARA_NO   in T_WORKING_PAPER_LOAN_CASE_FILE.PARA_NO%type,
                              io_cursor OUT t_cursor);

  procedure P_GetLoanCaseFile(ENGID     in number,
                              ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor);

  procedure P_AddVoucherChecking(ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 ENGID     in T_WORKING_PAPER_VOUCHER_CHECKING.ENG_ID%type,
                                 VNUMBER   in T_WORKING_PAPER_VOUCHER_CHECKING.V_NUMBER%type,
                                 OBS       in T_WORKING_PAPER_VOUCHER_CHECKING.OBSERVATION%type,
                                 PARA_NO   in T_WORKING_PAPER_VOUCHER_CHECKING.PARA_NO%type,
                                 io_cursor OUT t_cursor);

  procedure P_GetVoucherChecking(ENGID     in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_AddAccountOpeningDetails(ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       ENGID     in T_WORKING_PAPER_ACCOUNT_OPENING.ENG_ID%type,
                                       VNUMBER   in T_WORKING_PAPER_ACCOUNT_OPENING.V_NUMBER%type,
                                       ANATURE   in T_WORKING_PAPER_ACCOUNT_OPENING.A_NATURE%type,
                                       OBS       in T_WORKING_PAPER_ACCOUNT_OPENING.OBSERVATION%type,
                                       PARA_NO   in T_WORKING_PAPER_ACCOUNT_OPENING.PARA_NO%type,
                                       io_cursor OUT t_cursor);
  procedure P_GetAccountOpeningDetails(ENGID     in number,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor);

  procedure P_AddFixedAssetsDetails(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    ENGID     in T_WORKING_PAPER_FIXED_ASSETS.ENG_ID%type,
                                    ANAME     in T_WORKING_PAPER_FIXED_ASSETS.ASSET_NAME%type,
                                    PHYEX     in T_WORKING_PAPER_FIXED_ASSETS.PHYSICAL_EXISTANCE%type,
                                    LFAR      in T_WORKING_PAPER_FIXED_ASSETS.LOCATION_AS_PER_FAR%type,
                                    DIFF      in T_WORKING_PAPER_FIXED_ASSETS.DIFFERENCE%type,
                                    REM       in T_WORKING_PAPER_FIXED_ASSETS.REMARKS%type,
                                    io_cursor OUT t_cursor);
  procedure P_GetFixedAssetsDetails(ENGID     in number,
                                    ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor);

  procedure P_AddCashCounterDetails(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    ENGID     in T_WORKING_PAPER_CASH_COUNT.ENG_ID%type,
                                    DVAL      in T_WORKING_PAPER_CASH_COUNT.DENOMINATION_VAULT%type,
                                    CVAL      in T_WORKING_PAPER_CASH_COUNT.NO_CURRENCY_NOTES_VAULT%type,
                                    AVAL      in T_WORKING_PAPER_CASH_COUNT.TOTAL_AMOUNT_VAULT%type,
                                    DSR       in T_WORKING_PAPER_CASH_COUNT.DENOMINATION_SAFE_REGISTER%type,
                                    CSR       in T_WORKING_PAPER_CASH_COUNT.NO_CURRENCY_NOTES_SAFE_REGISTER%type,
                                    ASR       in T_WORKING_PAPER_CASH_COUNT.TOTAL_AMOUNT_SAFE_REGISTER%type,
                                    DIFF      in T_WORKING_PAPER_CASH_COUNT.DIFFERENCE%type,
                                    io_cursor OUT t_cursor);

  procedure P_GetCashCounterDetails(ENGID     in number,
                                    ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor);

  Procedure P_get_entities_for_manage_observations(ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  Procedure P_get_details_for_manage_observations(ENGID     in number,
                                                  ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor);

  Procedure P_get_details_for_manage_observations_text(Obs_id    in number,
                                                       IND       in varchar2,
                                                       ENT_ID    in number,
                                                       P_NO      in number,
                                                       R_ID      in number,
                                                       io_cursor OUT t_cursor);

  procedure P_GetObservationsForManageAuditParas(S_ENT_ID  IN NUMBER,
                                                 ENT_ID    in number,
                                                 P_NO      in number,
                                                 R_ID      in number,
                                                 io_cursor OUT t_cursor);

  procedure P_Update_Audit_Paras(COM_ID         in number,
                                 N_PARA_ID      IN NUMBER,
                                 O_PARA_ID      IN NUMBER,
                                 D_PARA_NO      IN VARCHAR2,
                                 D_AUDIT_PERIOD in VARCHAR2,
                                 D_GIST         in varchar2,
                                 D_RISK         in number,
                                 D_ANNEX        in number,
                                 D_IND          in varchar2,
                                 D_PARA_TEXT    in clob,
                                 D_AMOUNT       in decimal,
                                 D_INSTANCES    in number,
                                 -- ANNEXURE_REF_ID in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_GET_LC_DETAILS(LC_NO     VARCHAR2,
                             B_CODE    VARCHAR2,
                             P_NO      number,
                             ENT_Id    NUMBER,
                             io_cursor OUT t_cursor);

  procedure P_Get_responsibility(Para_ID   IN NUMBER,
                                 IND       in varchar2,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  Procedure P_GET_Para_details_for_Authorize(ENT_ID    in number,
                                             P_NO      in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor);

  Procedure P_GET_Para_changes_for_Authorize(Com_ID    in number,
                                             io_cursor OUT t_cursor);

  Procedure P_Get_responsibility_for_Authorize(C_ID      in number,
                                               io_cursor OUT t_cursor);

  PROCEDURE P_Authorize_Update_Audit_Paras(C_ID           IN NUMBER,
                                           N_PARA_ID      IN NUMBER,
                                           O_PARA_ID      IN NUMBER,
                                           D_PARA_ID      IN NUMBER,
                                           D_PARA_NO      IN VARCHAR2,
                                           D_AUDIT_PERIOD IN VARCHAR2,
                                           D_GIST         IN VARCHAR2,
                                           D_RISK         IN NUMBER,
                                           D_ANNEX        IN NUMBER,
                                           D_IND          IN VARCHAR2,
                                           D_PARA_TEXT    IN CLOB,
                                           D_AMOUNT       IN DECIMAL,
                                           D_INSTANCES    IN NUMBER,
                                           ENT_ID         IN NUMBER,
                                           P_NO           IN NUMBER,
                                           R_ID           IN NUMBER,
                                           P_DECISION     IN VARCHAR2,
                                           io_cursor      OUT t_cursor);

  Procedure P_Update_responsibility(IND        in varchar2,
                                    C_ID       in number,
                                    O_Para_ID  IN NUMBER,
                                    N_PARA_ID  in number,
                                    PPNO       in number,
                                    L_CASE     in number,
                                    LC_AMOUNT  in number,
                                    AC_Amount  in number,
                                    NO_account in number,
                                    Remarks    in varchar2,
                                    U_D_action in varchar2,
                                    E_NAME     in varchar2,
                                    ENT_ID     in number,
                                    P_NO       in number,
                                    R_ID       in number,
                                    io_cursor  OUT t_cursor);

  Procedure P_Delete_responsibility(IND       in varchar2,
                                    O_Para_ID IN NUMBER,
                                    N_PARA_ID in number,
                                    PPNO      in number,
                                    io_cursor OUT t_cursor);

  procedure P_draft_dsa(EID           number,
                        OBSID         number,
                        RESP_PPNO     number,
                        RESP_ROW_ID   number,
                        ENGAGEMENT_ID number,
                        P_NO          number,
                        ENT_Id        NUMBER,
                        R_ID          number,
                        io_cursor     OUT t_cursor);
  Procedure P_get_dsa_content(d_ID number, io_cursor OUT t_cursor);

  Procedure P_get_dsa_guidline(io_cursor OUT t_cursor);

  procedure P_add_dsa_checkilist(d_id       in number,
                                 check_list in number,
                                 P_NO       in number);
  procedure P_get_dsa_list(P_NO      in number,
                           R_ID      in number,
                           ENT_ID    in number,
                           io_cursor OUT t_cursor);

  Procedure P_submit_dsa_to_head_fad(d_ID number, io_cursor OUT t_cursor);
  Procedure P_update_dsa_heading(d_ID      number,
                                 U_HEADING in varchar,
                                 P_NO      in number,
                                 ENT_ID    in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);
  Procedure P_reffered_back_dsa_by_head_fad(d_ID      number,
                                            io_cursor OUT t_cursor);
  Procedure P_submit_dsa_by_head_fad_to_dpd(d_ID      number,
                                            io_cursor OUT t_cursor);
  Procedure P_reffered_back_dsa_by_dpd(d_ID number, io_cursor OUT t_cursor);
  Procedure P_submit_dsa_by_dpd_to_committee(d_ID      number,
                                             io_cursor OUT t_cursor);

  procedure p_get_email_address_for_dsa(ENT_ID    number,
                                        R_ID      number,
                                        P_NO      number,
                                        io_cursor OUT t_cursor);

end PKG_AR;

create or replace package body PKG_AR is

  procedure P_GetTaskList(ENT_ID    in number,
                          P_NO      in number,
                          R_ID      in number,
                          io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.id,
             t.eng_plan_id,
             t.team_id,
             t.sequence_no,
             t.teammember_ppno,
             t.entity_id,
             t.entity_code,
             t.entity_name,
             t.audit_start_date,
             t.audit_end_date,
             t.status_id,
             t.isactive,
             t.i,
             em.p_name,
             ee.type_id as ENTITY_TYPE,
             et.entitytypedesc as ENTITY_TYPE_DESC,
             st.description as ENG_NEXT_STATUS,
             ta.T_NAME,
             ts.DESCRIPTION as ENG_STATUS,
             p.description as audit_year,
             e.operation_startdate,
             e.operation_enddate,
             (case
               when m.isteamlead = 'Y' then
                'Z'
               else
                'C'
             end) as closing,
             (case
               when ee.type_id in (6, 28) then
                'Working Paper'
               else
                ' '
             end) WORKING_PAPER,
             (case
               when ee.type_id in (6, 28) then
                'Pre Audit Information'
               else
                ' '
             end) pre_info
        from T_AU_AUDIT_TEAM_TASKLIST t
       inner join T_AU_AUDIT_TEAMS ta
          on t.TEAM_ID = ta.TEAM_ID
         and t.eng_plan_id = ta.eng_id
       inner join T_AU_AUDIT_TEAM_TASKLIST_STATUS ts
          on t.STATUS_ID = ts.STATUS_ID
       inner join t_au_team_members m
          on t.team_id = m.t_id
         and m.member_ppno = t.teammember_ppno
       inner join t_au_plan_eng e
          on e.eng_id = t.eng_plan_id
       inner join t_auditee_entities ee
          on e.entity_id = ee.entity_id
       inner join t_auditee_ent_types et
          on ee.type_id = et.autid
       inner join T_AU_AUDIT_TEAM_TASKLIST_STATUS st
          on st.status_id = (t.status_id + 1)
        left join T_AU_AUDIT_JOINING j
          on j.eng_plan_id = t.eng_plan_id
         and j.team_mem_ppno = t.teammember_ppno
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       inner join t_auditee_entities_maping em
          on em.entity_id = ee.entity_id
       WHERE t.teammember_ppno = P_NO
         and t.isactive = 'Y'
         and t.status_id < 6
       order by T.AUDIT_START_DATE asc;
  
  end P_GetTaskList;

  procedure AUDIT_JOINING(ENG_PLAN_ID     in T_AU_AUDIT_JOINING.ENG_PLAN_ID%type,
                          TEAM_MEM_PPNO   in T_AU_AUDIT_JOINING.TEAM_MEM_PPNO%type,
                          ENTEREDBY       in T_AU_AUDIT_JOINING.ENTEREDBY%type,
                          JOINING_DATE    in T_AU_AUDIT_JOINING.JOINING_DATE%type,
                          COMPLETION_DATE in T_AU_AUDIT_JOINING.COMPLETION_DATE%type) is
    E_F NUMBER := 0;
  begin
    select nvl(min(t.eng_plan_id), 0)
      into E_F
      from t_au_audit_team_tasklist t
     inner join t_au_audit_joining j
        on j.eng_plan_id = t.eng_plan_id
     where t.teammember_ppno = TEAM_MEM_PPNO
       and j.team_mem_ppno = TEAM_MEM_PPNO
       and t.status_id between '1' and '5'
       and t.isactive = 'Y'
       and j.status != 'C';
    IF (E_F != 0) THEN
      INSERT INTO T_AU_AUDIT_JOINING al
        (al.ID,
         al.ENG_PLAN_ID,
         al.TEAM_MEM_PPNO,
         al.JOINING_DATE,
         al.ENTEREDBY,
         al.ENTEREDDATE,
         al.COMPLETION_DATE,
         al.STATUS)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_AUDIT_JOINING acc),
         ENG_PLAN_ID,
         TEAM_MEM_PPNO,
         to_date(JOINING_DATE, 'dd/mm/yyyy HH:MI:SS AM'),
         ENTEREDBY,
         to_date(sysdate, 'dd/mm/yyyy HH:MI:SS AM'),
         to_date(COMPLETION_DATE, 'dd/mm/yyyy HH:MI:SS AM'),
         'I');
      commit;
    ELSE
      E_F := 1;
    end if;
  
  end AUDIT_JOINING;

  procedure P_SetEngIdOnHold(ENGID IN NUMBER, ppno in number) is
  
  begin
  
    Update t_au_audit_joining ji
       SET ji.STATUS = 'P'
     where Ji.Team_Mem_Ppno = ppno
       and ji.eng_plan_id != ENGID;
    commit;
    Update t_au_plan_eng e SET e.STATUS = 5 where e.eng_id = ENGID;
    COMMIT;
  end P_SetEngIdOnHold;

  procedure P_GetJoiningDetails(ENG       in number,
                                ENT_ID    in number,
                                P_NO      in number,
                                R_ID      in number,
                                io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.team_id,
             tm.member_name,
             tm.member_ppno,
             tm.team_name       as TEAM_NAME,
             t.entity_id,
             t.entity_code,
             t.entity_name,
             t.audit_start_date,
             t.audit_end_date,
             rt.description     as RISK,
             st.description     as ENT_SIZE,
             p.description      as AUDIT_PERIOD,
             tm.isteamlead
        from t_au_audit_team_tasklist t
       inner join t_au_plan_eng pe
          on t.eng_plan_id = pe.eng_id
       INNER JOIN T_AU_PLAN P
          ON P.ID = PE.PLAN_ID
       inner join t_au_period p
          on pe.period_id = p.auditperiodid
       inner join t_au_team_members tm
          on t.teammember_ppno = tm.member_ppno
       inner join t_au_audit_teams audt
          on audt.team_id = tm.t_id
        LEFT join t_risk rt
          on P.AUDITEE_RISK = RT.R_ID
        LEFT join t_auditee_entities_size_disc st
          on P.AUDITEE_SIZE = st.entity_size
       where t.eng_plan_id = ENG
            
         and audt.eng_id = ENG
         and tm.member_ppno = P_NO;
  
  end P_GetJoiningDetails;

  procedure P_AddJoiningReport(ENGID           in number,
                               ENT_ID          in number,
                               P_NO            in number,
                               R_ID            in number,
                               COMPLETION_DATE in date,
                               ENT_EMAIL_ADD   in varchar2,
                               ENT_PHONE_NO    in varchar2,
                               io_cursor       OUT t_cursor) is
  
    T_F        number := 0;
    V_F        NUMBER := 0;
    A_F        NUMBER := 0;
    C_F        date;
    E_F        number := 0;
    R_F        number := 0;
    AUD_ENT_ID number := 0;
    M_F        varchar2(2);
  
  begin
    select eg.entity_id
      into AUD_ENT_ID
      from t_au_plan_eng eg
     where eg.eng_id = ENGID;
  
    select NVL(tm.isteamlead, 'N')
      into M_F
      FROM t_au_audit_teams t
     inner join t_au_team_members tm
        on tm.t_id = t.team_id
     where t.eng_id = ENGID
       and tm.member_ppno = P_NO;
  
    select NVL(MAX(l.id), 0)
      into E_F
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = E_F;
    commit;
  
    insert into t_au_activity_log
      (id,
       entity_id,
       role_id,
       ppnum,
       page_id,
       action,
       start_time,
       seq,
       unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       P_NO,
       19,
       'Submit Joining in ' ||
       (select bt.name
          from t_auditee_entities bt
         inner join t_au_plan_eng e
            on bt.entity_id = e.entity_id
         where e.eng_id = ENGID),
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
    select nvl(max(t.id), 0)
      into R_F
      from t_au_audit_team_tasklist t
     where t.teammember_ppno = P_NO
       and t.status_id = 2;
    if (R_F = 0) then
      SELECT COUNT(M.T_ID)
        INTO A_F
        FROM T_AU_TEAM_MEMBERS M
       WHERE M.MEMBER_PPNO = P_NO
         AND M.ISTEAMLEAD = 'Y';
      Update t_au_audit_joining ji
         SET ji.STATUS = 'P'
       where Ji.Team_Mem_Ppno = P_NO
         and ji.eng_plan_id != ENGID;
      select e.audit_enddate
        into C_F
        from t_au_plan_eng e
       where e.eng_id = ENGID;
      commit;
      Update t_au_plan_eng e SET e.STATUS = 10 where e.eng_id = ENGID;
      COMMIT;
      select e.entity_type
        into T_F
        from t_au_plan_eng e
       WHERE E.ENG_ID = ENGID;
      SELECT nvl(max(j.id), 0)
        INTO V_F
        FROM T_AU_AUDIT_JOINING j
       WHERE j.ENG_PLAN_ID = ENGID
         and j.TEAM_MEM_PPNO = P_NO
         and j.STATUS = 'I';
      if (V_F = 0 AND T_F in (6, 28)) then
        INSERT INTO T_AU_AUDIT_JOINING al
          (al.ID,
           al.ENG_PLAN_ID,
           al.TEAM_MEM_PPNO,
           al.JOINING_DATE,
           al.ENTEREDBY,
           al.ENTEREDDATE,
           al.COMPLETION_DATE,
           al.STATUS)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_AUDIT_JOINING acc),
           ENGID,
           P_NO,
           trunc(sysdate),
           P_NO,
           trunc(SYSDATE),
           C_F,
           'I');
        COMMIT;
        UPDATE T_AU_AUDIT_TEAM_TASKLIST t
           SET t.STATUS_ID =
               (select COALESCE(acc.STATUS_ID + 1, 1)
                  from T_AU_AUDIT_TEAM_TASKLIST acc
                 WHERE acc.ENG_PLAN_ID = ENGID
                   and acc.TEAMMEMBER_PPNO = P_NO)
         WHERE t.ENG_PLAN_ID = ENGID
           and t.TEAMMEMBER_PPNO = P_NO;
        COMMIT;
        IF (A_F != 0) THEN
          FOR NM IN (SELECT * FROM T_AUDIT_CHECKLIST_DETAILS) LOOP
          
            insert into t_auditee_checkklist
              (id, eng_id, checklist_id, ENTEREDBY, ENTEREDON, STATUS)
              select (select COALESCE(MAX(acc.ID) + 1, 1)
                        from t_auditee_checkklist acc),
                     ENGID,
                     d.id,
                     P_NO,
                     TRUNC(SYSDATE),
                     1
                from t_audit_checklist_details d
               WHERE D.ID = NM.ID;
            commit;
          
          END LOOP;
        END IF;
      ELSE
        if (V_F = 0 AND T_F not in (6, 28)) then
          INSERT INTO T_AU_AUDIT_JOINING al
            (al.ID,
             al.ENG_PLAN_ID,
             al.TEAM_MEM_PPNO,
             al.JOINING_DATE,
             al.ENTEREDBY,
             al.ENTEREDDATE,
             al.COMPLETION_DATE,
             al.STATUS)
          VALUES
            ((select COALESCE(max(acc.ID) + 1, 1)
               from T_AU_AUDIT_JOINING acc),
             ENGID,
             P_NO,
             sysdate,
             P_NO,
             SYSDATE,
             COMPLETION_DATE,
             'I');
          COMMIT;
          UPDATE T_AU_AUDIT_TEAM_TASKLIST t
             SET t.STATUS_ID =
                 (select COALESCE(acc.STATUS_ID + 1, 1)
                    from T_AU_AUDIT_TEAM_TASKLIST acc
                   WHERE acc.ENG_PLAN_ID = ENGID
                     and acc.TEAMMEMBER_PPNO = P_NO)
           WHERE t.ENG_PLAN_ID = ENGID
             and t.TEAMMEMBER_PPNO = P_NO;
          COMMIT;
        
        else
          UPDATE T_AU_AUDIT_TEAM_TASKLIST t
             SET t.STATUS_ID =
                 (select COALESCE(acc.STATUS_ID + 1, 1)
                    from T_AU_AUDIT_TEAM_TASKLIST acc
                   WHERE acc.ENG_PLAN_ID = ENGID
                     and acc.TEAMMEMBER_PPNO = P_NO)
           WHERE t.ENG_PLAN_ID = ENGID
             and t.TEAMMEMBER_PPNO = P_NO;
          COMMIT;
        end if;
      end if;
      select eg.entity_id
        into AUD_ENT_ID
        from t_au_plan_eng eg
       where eg.eng_id = ENGID;
      update t_auditee_entities et
         set et.email_address = ENT_EMAIL_ADD, et.telephone = ENT_PHONE_NO
       where et.entity_id = AUD_ENT_ID;
    
      if (M_F = 'Y') then
        OPEN io_Cursor FOR
          SELECT 'Joining Submitted Successfully' AS remarks,
                 'Y' as email,
                 e.email_address as to_email,
                 ad.email_address as cc_email,
                 e.name as auditee_name,
                 'Team Details ' as team,
                 (case
                   when tm.isteamlead = 'Y' then
                    'Team Lead:- ' || tm.member_ppno || ' - ' ||
                    tm.member_name
                 end) as team_lead,
                 'along with ' || (c.no_of_members - 1) || ' Team Members' as team_members
            FROM t_auditee_entities e
           inner join t_Au_Plan_Eng ep
              on ep.entity_id = e.entity_id
            left join t_au_audit_teams t
              on t.eng_id = ep.eng_id
           inner join t_au_team_members tm
              on tm.t_id = t.team_id
           inner join t_auditee_entities ad
              on ad.entity_id = ep.auditby_id
           inner join v_get_audit_team_count c
              on c.t_id = t.team_id
           where ep.eng_id = ENGID
             and tm.isteamlead = 'Y';
      else
        OPEN io_Cursor FOR
          SELECT 'Joining Submitted Successfully' AS remarks,
                 'N' as email,
                 '' as to_email,
                 '' as cc_email,
                 '' as auditee_name,
                 '' as team,
                 '' as team_lead,
                 '' as team_members
            from dual;
      end if;
    
    else
      OPEN io_Cursor FOR
        select 'Please close/Exit the pending audit in your task list' as remarks,
               '' as to_email,
               '' as cc_email,
               '' as auditee_name,
               '' as aud_entity,
               '' as team_members
          from dual;
    end if;
  
  end p_AddJoiningReport;

  Procedure P_ADD_Working_paper_loan(ENG_ID    in number,
                                     LC_num    in number,
                                     LC_AMT    in number,
                                     Disb_dat  in date,
                                     LC_CATE   in number,
                                     obs_text  in varchar2,
                                     OM        in number,
                                     io_cursor OUT t_cursor) is
  begin
    insert into t_au_working_paper_loan
      (id,
       eng,
       lc_number,
       lc_amount,
       disb_date,
       lc_cat,
       observation,
       om_no)
    
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_working_paper_loan p),
       ENG_ID,
       LC_num,
       LC_AMT,
       disb_dat,
       lc_cate,
       obs_text,
       OM);
    commit;
    open io_cursor for
    
      select 'Loan Record entered correctly' as remarks from Dual;
  
  end P_ADD_Working_paper_loan;

  procedure P_GetAuditChecklist(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select t.*, e.entitytypedesc as ENTITY_TYPE_NAME
        from t_audit_checklist t
       inner join t_auditee_ent_types e
          on t.entity_type = e.autid
       where t.STATUS = 'Y'
       order by t.t_id asc;
  
  end P_GetAuditChecklist;

  procedure P_GetAuditChecklistCAD(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select t.*
        from t_audit_checklist t
       where t.STATUS = 'Y'
      --and t.t_id in (5, 11)
       order by t.t_id asc;
  
  end P_GetAuditChecklistCAD;

  procedure p_GetAuditChecklistSub(tid       in number,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) is
  
  begin
  
    if (tid = 0) then
      OPEN io_Cursor FOR
        select t.*,
               p.heading        as T_NAME,
               e.entitytypedesc as ENTITY_TYPE_NAME
          from t_audit_checklist_sub t
         inner join t_audit_checklist p
            on p.t_id = t.t_id
         inner join t_auditee_ent_types e
            on t.entity_type = e.autid
         where t.STATUS = 'Y'
         order by t.s_id asc;
    else
      OPEN io_Cursor FOR
        select t.*,
               p.heading        as T_NAME,
               e.entitytypedesc as ENTITY_TYPE_NAME
          from t_audit_checklist_sub t
         inner join t_audit_checklist p
            on p.t_id = t.t_id
         inner join t_auditee_ent_types e
            on t.entity_type = e.autid
         where t.STATUS = 'Y'
           and t.t_id = tid
         order by t.s_id;
    end if;
  
  end p_GetAuditChecklistSub;

  procedure P_GetAuditChecklistDetails(sid       in number,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor) is
  
  begin
  
    if (sid = 0) then
    
      OPEN io_Cursor FOR
        select t.*,
               p.heading     as S_NAME,
               s.description as V_NAME,
               r.description as RISK
          from t_audit_checklist_details t
         inner join t_audit_checklist_sub p
            on p.s_id = t.s_id
         inner join t_r_sub_group s
            on s.s_gr_id = t.v_id
         inner join t_risk r
            on r.r_id = t.risk_id
         where t.STATUS = 'Y'
         order by t.id asc;
    
    else
      OPEN io_Cursor FOR
        select t.*,
               p.heading     as S_NAME,
               s.description as V_NAME,
               r.description as RISK
          from t_audit_checklist_details t
         inner join t_audit_checklist_sub p
            on p.s_id = t.s_id
         inner join t_r_sub_group s
            on s.s_gr_id = t.v_id
         inner join t_risk r
            on r.r_id = t.risk_id
         where t.STATUS = 'Y'
           AND T.S_ID = SID
         order by t.id asc;
    end if;
  
  end P_GetAuditChecklistDetails;

  PROCEDURE P_getauditeecheckklist(PLANID         IN NUMBER,
                                   SUBCHECKLISTID IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_Cursor FOR
      select s.s_id,
             c.checklist_id,
             c.action       as Status,
             o.engplanid,
             ot.text,
             o.id           as obsid
        from t_auditee_checkklist c
       inner join t_audit_checklist_details d
          on d.id = c.checklist_id
       inner join t_audit_checklist_sub s
          on s.s_id = d.s_id
        left join t_au_observation o
          on o.engplanid = c.eng_id
         and o.subchecklist_id = SUBCHECKLISTID
        left join t_au_observation_text ot
          on ot.observatsion_id = o.id
       where c.eng_id = PLANID
         and s.s_id = SUBCHECKLISTID
         and o.subchecklist_id = SUBCHECKLISTID
         and c.checklist_id = o.checklistdetail_id;
  
    commit;
  END P_getauditeecheckklist;

  procedure P_GetRiskGroup(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select rg.* FROM T_R_GROUP rg order by rg.GR_ID asc;
  
  end P_GetRiskGroup;

  procedure P_GetRiskSubGroup(group_id IN NUMBER, io_cursor OUT t_cursor) as
  
  begin
    IF (group_id = 0) THEN
      OPEN io_cursor FOR
        Select rsg.*, rg.DESCRIPTION as GROUP_DESC
          FROM T_R_SUB_GROUP rsg
         inner join T_R_GROUP rg
            on rsg.GR_ID = rg.GR_ID
         order by rsg.S_GR_ID asc;
    ELSE
      OPEN io_cursor FOR
        Select rsg.*, rg.DESCRIPTION as GROUP_DESC
          FROM T_R_SUB_GROUP rsg
         inner join T_R_GROUP rg
            on rsg.GR_ID = rg.GR_ID
         WHERE rsg.GR_ID = group_id
         order by rsg.S_GR_ID asc;
    END IF;
  
  end P_GetRiskSubGroup;

  procedure p_GetRiskActivities(Sub_group_id IN NUMBER,
                                io_cursor    OUT t_cursor) as
  
  begin
    IF (Sub_group_id = 0) THEN
      OPEN io_cursor FOR
        Select ra.*, rsg.DESCRIPTION as SUB_GROUP_DESC
          FROM T_R_ACTIVITY ra
         inner join T_R_SUB_GROUP rsg
            on ra.S_GR_ID = rsg.S_GR_ID
         order by ra.ACTIVITY_ID asc;
    ELSE
      OPEN io_cursor FOR
        Select ra.*, rsg.DESCRIPTION as SUB_GROUP_DESC
          FROM T_R_ACTIVITY ra
         inner join T_R_SUB_GROUP rsg
            on ra.S_GR_ID = rsg.S_GR_ID
         where ra.S_GR_ID = Sub_group_id
         order by ra.ACTIVITY_ID asc;
    END IF;
  END p_GetRiskActivities;

  procedure P_GetAuditVoilationcats(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select * from t_control_violation V order by V.ID;
  
  end P_GetAuditVoilationcats;

  procedure P_GetVoilationSubGroup(group_id  in number,
                                   io_cursor OUT t_cursor) is
  
  begin
    if (group_id = 0) then
      OPEN io_Cursor FOR
        select * from t_control_violation V order by V.ID;
    else
      OPEN io_Cursor FOR
        select *
          from t_control_violation_sub S
         where s.v_id = group_id
         order by s.v_ID, s.ID asc;
    end if;
  end P_GetVoilationSubGroup;

  procedure P_GetAuditChecklistDetails_search(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
    
      select t.id,
             s.heading as P_NAME,
             p.heading as S_NAME,
             Ltrim(t.heading) as C_NAME,
             ltrim(r.description) as RISK
        from t_audit_checklist_details t
       inner join t_audit_checklist_sub p
          on p.s_id = t.s_id
       inner join t_audit_checklist s
          on s.t_id = p.t_id
       inner join t_risk r
          on r.r_id = t.risk_id
       where t.STATUS = 'Y'
       order by p.t_id, t.s_id, t.id asc;
  
  end P_GetAuditChecklistDetails_search;

  procedure P_get_employees_information(P_NO      in number,
                                        io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
    
      select p.ppno,
             p.employeefirstname || '  ' || p.employeelastname as emp_name
        from v_service_employeeinfo p
       where p.ppno = P_NO;
  
  end P_get_employees_information;

  PROCEDURE P_GET_ENB_CIRCULARS(p_text VARCHAR2, io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT c.circularid,
             c.display_text,
             c.keywords,
             c.redirectedpage,
             c.division,
             --0 AS division,
             c.reference_no,
             c.issuedate,
             c.doctype,
             d.ent_id
        FROM t_au_enb_tbl_divisons d
       INNER JOIN v_ztbl_circulars c
          ON c.division = d.division_code
       WHERE upper(c.keywords) LIKE '%' || upper(p_text) || '%'
       order by c.issuedate desc;
  END P_GET_ENB_CIRCULARS;

  procedure P_SaveAuditObservationCAD(PLANID            in T_AU_OBSERVATION.ENGPLANID%type,
                                      STATUS            in T_AU_OBSERVATION.STATUS%type,
                                      REPLYDATE         in T_AU_OBSERVATION.REPLYDATE%type,
                                      ENTEREDBY         in T_AU_OBSERVATION.Enteredby%type,
                                      Severity          in T_AU_OBSERVATION.Severity%type,
                                      SUBCHECKLISTID    in T_AU_OBSERVATION.Subchecklist_Id%type,
                                      CHECKLISTDETAILID in T_AU_OBSERVATION.Checklistdetail_Id%type,
                                      TEXT_DATA         in T_AU_OBSERVATION_TEXT.TEXT%type,
                                      TITLE             in varchar2,
                                      AMOUNT_INV        in number,
                                      NO_INST           in number,
                                      BRANCHID          IN NUMBER,
                                      ENT_ID            in number,
                                      P_NO              in number,
                                      R_ID              in number,
                                      ANNEX_ID          IN NUMBER,
                                      io_cursor         OUT t_cursor) is
    V_F NUMBER := 0;
    R_F NUMBER := 0;
    E_F NUMBER := 0;
    S_F NUMBER := 0;
    M_F NUMBER := 0;
    T_F NUMBER := 0;
    b_F number := 0;
    Z_B number := 0;
    OBS NUMBER := 0;
  begin
  
      select max(o.ID+1)INTO OBS from T_AU_OBSERVATION o;
    select NVL(MAX(l.id), 0)
      into Z_B
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_B;
    commit;
    insert into t_au_activity_log
      (id,
       entity_id,
       role_id,
       ppnum,
       page_id,
       action,
       start_time,
       seq,
       unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       P_NO,
       142,
       OBS||
       ' Observation Saved',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select COALESCE(max(ac.MEMO_NUMBER) + 1, 1)
      INTO M_F
      from T_AU_OBSERVATION ac
     WHERE AC.ENGPLANID = PLANID;
    SELECT E.ENTITY_ID
      INTO E_F
      FROM T_AU_PLAN_ENG E
     WHERE E.ENG_ID = PLANID;
    SELECT EE.TYPE_ID
      INTO T_F
      FROM T_AUDITEE_ENTITIES EE
     WHERE EE.ENTITY_ID = E_F;
    select NVL(MAX(cd.role_resp_id), 0)
      INTO V_F
      from t_audit_checklist_details cd
     where cd.id = CHECKLISTDETAILID;
    SELECT S.T_ID
      INTO R_F
      FROM T_AUDIT_CHECKLIST_SUB S
     WHERE S.S_ID = SUBCHECKLISTID;
    SELECT CD.RISK_ID
      INTO S_F
      from t_audit_checklist_details cd
     where cd.id = CHECKLISTDETAILID;
    select nvl(max(e.entity_id), 0)
      into B_F
      from t_auditee_entities e
     where e.code = BRANCHID
       and e.type_id in (6, 28);
    IF (BRANCHID <> 0) THEN
      INSERT INTO T_AU_OBSERVATION o
        (o.ID,
         o.ENGPLANID,
         o.STATUS,
         o.ENTEREDBY,
         o.ENTEREDDATE,
         o.ENTITY_ID,
         o.REPLYDATE,
         o.SEVERITY,
         o.RESPONSIBILITY_ASSIGNED,
         o.RISKMODEL_ID,
         o.SUBCHECKLIST_ID,
         o.CHECKLISTDETAIL_ID,
         o.V_CAT_ID,
         o.V_CAT_NATURE_ID,
         o.ENTITY_CODE,
         O.ANNEX,
         o.Amount_Involved,
         o.No_Of_Instances
         
         )
      VALUES
        (OBS,
         PLANID,
         STATUS,
         ENTEREDBY,
         sysdate,
         B_F,
         to_date(REPLYDATE, 'dd/mm/yyyy HH:MI:SS AM'),
         Severity,
         V_F,
         R_F,
         SUBCHECKLISTID,
         CHECKLISTDETAILID,
         0,
         0,
         BRANCHID,
         ANNEX_ID,
         AMOUNT_INV,
         NO_INST);
      commit;
      INSERT INTO T_AU_OBSERVATION_TEXT
        (ID,
         OBSERVATSION_ID,
         TEXT,
         ENTEREDBY,
         ENTEREDDATE,
         ENG_PLAN,
         HEADINGS)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_TEXT acc),
        OBS,
         TEXT_DATA,
         ENTEREDBY,
         SYSDATE,
         PLANID,
         TITLE);
      commit;
    
      Open io_cursor FOR
        SELECT OBS AS ID,
               PLANID AS ENG_ID,
               r.remarks,
               r.ref
          from t_au_remarks r
         where r.id = 15;
    ELSE
      INSERT INTO T_AU_OBSERVATION o
        (o.ID,
         o.ENGPLANID,
         o.STATUS,
         o.ENTEREDBY,
         o.ENTEREDDATE,
         o.ENTITY_ID,
         o.REPLYDATE,
         o.SEVERITY,
         o.RESPONSIBILITY_ASSIGNED,
         o.RISKMODEL_ID,
         o.SUBCHECKLIST_ID,
         o.CHECKLISTDETAIL_ID,
         o.V_CAT_ID,
         o.V_CAT_NATURE_ID,
         O.ANNEX,
         o.Amount_Involved,
         o.No_Of_Instances)
      VALUES
        (OBS,
         PLANID,
         STATUS,
         ENTEREDBY,
         sysdate,
         E_F,
         to_date(REPLYDATE, 'dd/mm/yyyy HH:MI:SS AM'),
         Severity,
         V_F,
         R_F,
         SUBCHECKLISTID,
         CHECKLISTDETAILID,
         0,
         0,
         ANNEX_ID,
         AMOUNT_INV,
         NO_INST);
      commit;
      INSERT INTO T_AU_OBSERVATION_TEXT
        (ID,
         OBSERVATSION_ID,
         TEXT,
         ENTEREDBY,
         ENTEREDDATE,
         ENG_PLAN,
         MEMO_NUMBER,
         HEADINGS)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_TEXT acc),
         OBS,
         TEXT_DATA,
         ENTEREDBY,
         SYSDATE,
         PLANID,
         M_F,
         TITLE);
      commit;
    
      Open io_cursor FOR
        SELECT OBS AS ID,
               PLANID AS ENG_ID,
               r.remarks,
               r.ref
          from t_au_remarks r
         where r.id = 15;
    
    end if;
  
  end P_SaveAuditObservationCAD;

  procedure P_SaveAuditObservation(PLANID            in T_AU_OBSERVATION.ENGPLANID%type,
                                   STATUS            in T_AU_OBSERVATION.STATUS%type,
                                   REPLYDATE         in T_AU_OBSERVATION.REPLYDATE%type,
                                   ENTEREDBY         in T_AU_OBSERVATION.Enteredby%type,
                                   Severity          in T_AU_OBSERVATION.Severity%type,
                                   SUBCHECKLISTID    in T_AU_OBSERVATION.Subchecklist_Id%type,
                                   CHECKLISTDETAILID in T_AU_OBSERVATION.Checklistdetail_Id%type,
                                   VCATID            in T_AU_OBSERVATION.v_Cat_Id%type,
                                   VCATNATUREID      in T_AU_OBSERVATION.v_Cat_Nature_Id%type,
                                   TEXT_DATA         in T_AU_OBSERVATION_TEXT.TEXT%type,
                                   NOINSTANCES       in t_au_observation.no_of_instances%type,
                                   AMOUNT_INV        in number,
                                   TITLE             IN VARCHAR2,
                                   OT_ENT_ID         in number,
                                   ENT_ID            in number,
                                   P_NO              in number,
                                   R_ID              in number,
                                   ANNEX_ID          IN NUMBER,
                                   io_cursor         OUT t_cursor) is
  
    cursor V is
      select e.entity_type,
             t.audit_type,
             e.eng_id,
             E.AUDIT_ENDDATE,
             COALESCE(max(ac.MEMO_NUMBER) + 1, 1) as memo_no,
             (CASE
               WHEN (OT_ENT_ID = 0 or OT_ENT_ID is null) THEN
                E.ENTITY_ID
               ELSE
                OT_ENT_ID
             END) AS ENTITY_ID,
             (select nvl(max(obt.checklistdetail_id), 0)
                from t_au_observation obt
               where obt.engplanid = PLANID
                 and obt.checklistdetail_id = CHECKLISTDETAILID) as check_list,
             (select cd.role_resp_id
                from t_audit_checklist_details cd
               where cd.id = CHECKLISTDETAILID) as role,
             (SELECT S.T_ID
                FROM T_AUDIT_CHECKLIST_SUB S
               WHERE S.S_ID = SUBCHECKLISTID) as check_sub_id
        from T_AU_PLAN_ENG e
       inner join t_auditee_ent_types t
          on t.autid = e.entity_type
        left join T_AU_OBSERVATION ac
          on ac.engplanid = e.eng_id
       WHERE e.eng_id = PLANID
       group by e.entity_type,
                t.audit_type,
                e.eng_id,
                E.AUDIT_ENDDATE,
                E.ENTITY_ID;
    vr1 V%rowtype;
    Z_B number := 0;
    OBS number := 0;
  begin
    Open V;
    Fetch V
      into vr1;
    Close v;
    
    select max(o.ID+1) into OBS from T_AU_OBSERVATION o;
    
    if (vr1.ENTITY_ID is not null) then
      select NVL(MAX(l.id), 0)
        into Z_B
        from t_au_activity_log l
       where l.ppnum = P_NO;
      update t_au_activity_log l set l.end_time = sysdate where l.id = Z_B;
      commit;
      insert into t_au_activity_log
        (id,
         entity_id,
         role_id,
         ppnum,
         page_id,
         action,
         start_time,
         seq,
         unattend)
      VALUES
        ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
         ENT_ID,
         R_ID,
         P_NO,
         142,
         obs ||
         'Observation Saved',
         sysdate,
         (select COALESCE(max(l.seq) + 1, 1)
            from t_au_activity_log l
           where l.id = Z_B
             and l.ppnum = P_NO),
         'Y');
      commit;
    
      if (Severity != 0 and Severity is not null) then
        IF (vr1.audit_type = 'B') THEN
          if (vr1.check_list = 0) then
            INSERT INTO T_AU_OBSERVATION o
              (o.ID,
               o.ENGPLANID,
               o.STATUS,
               o.ENTEREDBY,
               o.ENTEREDDATE,
               o.ENTITY_ID,
               o.REPLYDATE,
               o.SEVERITY,
               o.RESPONSIBILITY_ASSIGNED,
               o.RISKMODEL_ID,
               o.SUBCHECKLIST_ID,
               o.CHECKLISTDETAIL_ID,
               o.V_CAT_ID,
               o.V_CAT_NATURE_ID,
               o.NO_OF_INSTANCES,
               O.ANNEX,
               o.amount_involved)
            VALUES
              (obs,
               PLANID,
               STATUS,
               ENTEREDBY,
               (CASE WHEN TRUNC(sysdate) < TRUNC(VR1.AUDIT_ENDDATE) THEN
                SYSDATE ELSE vr1.AUDIT_ENDDATE end),
               vr1.entity_id,
               trunc(REPLYDATE),
               Severity,
               vr1.role,
               vr1.check_sub_id,
               SUBCHECKLISTID,
               CHECKLISTDETAILID,
               0,
               0,
               NOINSTANCES,
               ANNEX_ID,
               AMOUNT_INV);
            commit;
            INSERT INTO T_AU_OBSERVATION_TEXT
              (ID,
               OBSERVATSION_ID,
               TEXT,
               ENTEREDBY,
               ENTEREDDATE,
               ENG_PLAN,
               HEADINGS)
            VALUES
              ((select COALESCE(max(acc.ID) + 1, 1)
                 from T_AU_OBSERVATION_TEXT acc),
               OBS,
               TEXT_DATA,
               ENTEREDBY,
               SYSDATE,
               PLANID,
               TITLE);
            commit;
            update t_auditee_checkklist t
               set t.status = 2, action = 'Y'
             where t.eng_id = PLANID
               and t.checklist_id = CHECKLISTDETAILID;
            commit;
          
            Open io_cursor FOR
              SELECT OBS AS ID,
                     PLANID AS ENG_ID,
                     r.remarks,
                     R.ref
                from t_au_remarks r
               where r.id = 15;
          else
            Open io_cursor FOR
              SELECT OBS AS ID,
                     PLANID AS ENG_ID,
                     r.remarks,
                     R.ref
                from t_au_remarks r
               where r.id = 16;
          end if;
        
        else
        
          INSERT INTO T_AU_OBSERVATION o
            (o.ID,
             o.ENGPLANID,
             o.STATUS,
             o.ENTEREDBY,
             o.ENTEREDDATE,
             o.ENTITY_ID,
             o.REPLYDATE,
             o.SEVERITY,
             o.RESPONSIBILITY_ASSIGNED,
             o.RISKMODEL_ID,
             o.SUBCHECKLIST_ID,
             o.CHECKLISTDETAIL_ID,
             o.V_CAT_ID,
             o.V_CAT_NATURE_ID,
             o.NO_OF_INSTANCES,
             O.ANNEX,
             o.amount_involved)
          VALUES
            (OBS,
             PLANID,
             STATUS,
             ENTEREDBY,
             (CASE WHEN TRUNC(sysdate) < TRUNC(VR1.AUDIT_ENDDATE) THEN
              SYSDATE ELSE vr1.AUDIT_ENDDATE end),
             vr1.entity_id,
             trunc(REPLYDATE),
             Severity,
             0,
             0,
             0,
             0,
             VCATID,
             VCATNATUREID,
             NOINSTANCES,
             0,
             AMOUNT_INV);
          commit;
          INSERT INTO T_AU_OBSERVATION_TEXT
            (ID,
             OBSERVATSION_ID,
             TEXT,
             ENTEREDBY,
             ENTEREDDATE,
             ENG_PLAN,
             HEADINGS)
          VALUES
            ((select COALESCE(max(acc.ID) + 1, 1)
               from T_AU_OBSERVATION_TEXT acc),
             OBS,
             TEXT_DATA,
             ENTEREDBY,
             SYSDATE,
             PLANID,
             TITLE);
          commit;
        
          Open io_cursor FOR
            SELECT OBS AS ID,
                     PLANID AS ENG_ID,
                   r.remarks,
                   R.ref
              from t_au_remarks r
             where r.id = 15;
        
        END IF;
      else
        Open io_cursor FOR
          select '0' as id,
                 'Please contact Ali Asif and inform the issue' as remarks,
                 R.ref
            from t_au_remarks r
           WHERE R.ID = 16;
      end if;
    else
      Open io_cursor FOR
        select '0' as id,
               'System error, Please logout and login again, if the same message appears contact Ali Asif and inform him about the error' as remarks,
               R.ref
          from t_au_remarks r
         WHERE R.ID = 16;
    end if;
  end P_SaveAuditObservation;

  PROCEDURE P_responibilityassigned(N_ID      IN NUMBER,
                                    E_ID    IN NUMBER,
                                    IND       IN VARCHAR2,
                                    PPNO      IN NUMBER,
                                    RES_PP    IN NUMBER,
                                    LOANCASE  IN NUMBER,
                                    ACCNUMBER IN NUMBER,
                                    LCAMOUNT  IN NUMBER,
                                    ACAMOUNT  IN NUMBER,
                                    io_cursor OUT t_cursor) IS
    v_new_id  NUMBER;
    v_err_msg VARCHAR2(4000);
  BEGIN
    IF IND = 'D' THEN
      DELETE FROM t_au_observation_responibility_assigned r
       WHERE r.obs_id = N_ID
         AND r.pp_no = RES_PP;
    
      INSERT INTO T_AU_RESPONSIBILITY_LOG
        (LOG_ID,
         OBS_ID,
         ACTION_TYPE,
         RES_PP,
         LOAN_CASE,
         ACCOUNT_NUMBER,
         LC_AMOUNT,
         AC_AMOUNT,
         ACTION_BY,
         REMARKS)
      VALUES
        (SEQ_RESP_LOG_ID.NEXTVAL,
         N_ID,
         'D',
         RES_PP,
         LOANCASE,
         ACCNUMBER,
         LCAMOUNT,
         ACAMOUNT,
         PPNO,
         'Deleted responsibility record.');
    
      OPEN io_cursor FOR
        SELECT 'Responsibility deleted successfully.' AS REMARKS FROM DUAL;
    
    ELSIF IND = 'A' THEN
      SELECT NVL(MAX(ID), 0) + 1
        INTO v_new_id
        FROM t_au_observation_responibility_assigned;
    
      INSERT INTO t_au_observation_responibility_assigned
        (ID,
         obs_id,
         assignedby,
         pp_no,
         is_active,
         loan_case,
         account_number,
         lc_amount,
         ac_amount,ENG_ID)
      VALUES
        (v_new_id,
         N_ID,
         PPNO,
         RES_PP,
         'Y',
         LOANCASE,
         ACCNUMBER,
         LCAMOUNT,
         ACAMOUNT,E_ID);
    
      INSERT INTO T_AU_RESPONSIBILITY_LOG
        (LOG_ID,
         OBS_ID,
         ACTION_TYPE,
         RES_PP,
         LOAN_CASE,
         ACCOUNT_NUMBER,
         LC_AMOUNT,
         AC_AMOUNT,
         ACTION_BY,
         REMARKS)
      VALUES
        (SEQ_RESP_LOG_ID.NEXTVAL,
         N_ID,
         'A',
         RES_PP,
         LOANCASE,
         ACCNUMBER,
         LCAMOUNT,
         ACAMOUNT,
         PPNO,
         'Responsibility added.');
    
      OPEN io_cursor FOR
        SELECT 'Responsibility added successfully.' AS REMARKS FROM DUAL;
    
    ELSIF IND = 'U' THEN
      UPDATE t_au_observation_responibility_assigned
         SET loan_case      = LOANCASE,
             account_number = ACCNUMBER,
             lc_amount      = LCAMOUNT,
             ac_amount      = ACAMOUNT,
             ENG_ID         = E_ID
       WHERE obs_id = N_ID 
         AND pp_no = RES_PP;
    
      INSERT INTO T_AU_RESPONSIBILITY_LOG
        (LOG_ID,
         OBS_ID,
         ACTION_TYPE,
         RES_PP,
         LOAN_CASE,
         ACCOUNT_NUMBER,
         LC_AMOUNT,
         AC_AMOUNT,
         ACTION_BY,
         REMARKS)
      VALUES
        (SEQ_RESP_LOG_ID.NEXTVAL,
         N_ID,
         'U',
         RES_PP,
         LOANCASE,
         ACCNUMBER,
         LCAMOUNT,
         ACAMOUNT,
         PPNO,
         'Responsibility updated.');
    
      OPEN io_cursor FOR
        SELECT 'Responsibility updated successfully.' AS REMARKS FROM DUAL;
    END IF;
  
    COMMIT;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_err_msg := SQLERRM;
      OPEN io_cursor FOR
        SELECT 'Error: ' || v_err_msg AS REMARKS FROM DUAL;
  END P_responibilityassigned;

  procedure P_UpdateObservation(OBS_ID       in number,
                                title        in varchar2,
                                obtext       in clob,
                                subprocessid in number,
                                checklistid  in number,
                                RiskID       in number,
                                AnnexureID   in number,
                                ENT_ID       in number,
                                P_NO         in number,
                                R_ID         in number,
                                io_cursor    OUT t_cursor) is
    V_F number := 0;
    N_F number := 0;
    Z_B number := 0;
  begin
  
    select NVL(MAX(l.id), 0)
      into Z_B
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_B;
    commit;
    insert into t_au_activity_log
      (id,
       entity_id,
       role_id,
       ppnum,
       page_id,
       action,
       start_time,
       seq,
       unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       P_NO,
       79,
       OBS_ID || ' Observation Text and Checklist Updated',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select NVL(max(t.id), 0)
      into N_F
      from t_au_audit_team_tasklist t
     inner join t_au_observation o
        on o.engplanid = t.eng_plan_id
     where o.id = obs_id
       and t.teammember_ppno = P_NO;
    select nvl(s.status, 0)
      into V_F
      from t_au_observation s
     where s.id = OBS_ID;
    if (V_F < 8 and N_F != 0) then
      if (subprocessid = 0) then
        update T_AU_OBSERVATION_TEXT ot
           set ot.text            = obtext,
               ot.headings        = title,
               ot.lastupdateddate = sysdate,
               ot.lastupdatedby   = P_NO
         where ot.OBSERVATSION_ID = OBS_ID;
        update t_au_observation o
           set o.severity = RiskID
         where o.id = OBS_ID;
        commit;
        commit;
      else
        update t_au_observation o
           set o.subchecklist_id    = subprocessid,
               o.checklistdetail_id = checklistid,
               o.severity           = RiskID,
               o.annex              = AnnexureID
         where o.id = OBS_ID;
        commit;
        update T_AU_OBSERVATION_TEXT ot
           set ot.text            = obtext,
               ot.headings        = title,
               ot.lastupdateddate = sysdate,
               ot.lastupdatedby   = P_NO
         where ot.OBSERVATSION_ID = OBS_ID;
        commit;
      
      end if;
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 29;
    else
      open io_cursor for
        select 1 as ref,
               'Memo/Observation cannot be updated, either you are not part of the audit team or Observation has been fanialized' as remarks
          from dual;
    end if;
  end P_UpdateObservation;

  procedure P_DropAuditObservation(OBS_ID    IN NUMBER,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) is
    V_F number := 0;
    T_L varchar(5) := 'N';
    Z_B number := 0;
  begin
  
    select NVL(MAX(l.id), 0)
      into Z_B
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_B;
    commit;
    insert into t_au_activity_log
      (id,
       entity_id,
       role_id,
       ppnum,
       page_id,
       action,
       start_time,
       seq,
       unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       P_NO,
       51,
       OBS_ID || ' Observation Dropped',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select nvl(max(tm.isteamlead), 'N')
      into T_L
      from t_au_observation op
     inner join t_au_audit_team_tasklist tl
        on op.engplanid = tl.eng_plan_id
     inner join t_au_team_members tm
        on tl.team_id = tm.t_id
     where tl.teammember_ppno = P_NO
       and op.id = OBS_ID;
  
    select m.role_id into V_F from t_user_maping m where m.ppno = p_no;
    if (T_L = 'Y') then
      UPDATE t_au_observation SET STATUS = 23 WHERE ID = OBS_ID;
      COMMIT;
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 21;
    else
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 22;
    end if;
  
  end P_DropAuditObservation;

  procedure p_get_auditee_submission_list(ENT_ID    in number,
                                          io_cursor OUT t_cursor) is
  begin
  
    if (ENT_id != 112248) then
      open io_cursor for
        select e.entity_id, e.name
          from t_auditee_entities e
         where e.auditby_id = ent_id
           and e.active = 'Y'
           and e.auditable = 'Y';
    else
      open io_cursor for
        select e.entity_id, e.name
          from t_auditee_entities e
         where e.active = 'Y'
           and e.auditable = 'Y'
           AND E.AUDITBY_ID IN (112242, 112248);
    end if;
  end p_get_auditee_submission_list;

  procedure P_SubmitAuditObservationToAuditee(OBS_ID    IN NUMBER,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor) is
  
    A_F number := 0;
    Z_B number := 0;
    cursor V is
      select o.id,
             o.engplanid,
             o.status,
             o.entity_id,
             o.memo_number,
             o.entity_code,
             tm.isteamlead,
             tt.observatsion_id,
             TT.ID AS TEXT_ID,
             (select nvl(max(obt.memo_number), 0)
                from t_au_observation obt
               where obt.engplanid = o.engplanid) as max_num
        from t_au_observation o
       INNER JOIN T_AU_OBSERVATION_TEXT TT
          ON TT.OBSERVATSION_ID = o.id
       inner join t_au_audit_team_tasklist tl
          on o.engplanid = tl.eng_plan_id
       inner join t_au_team_members tm
          on tl.team_id = tm.t_id
         and tm.member_ppno = tl.teammember_ppno
       where o.id = obs_id
         and tl.teammember_ppno = P_NO;
    vr V%rowtype;
  begin
    Open V;
    Fetch V
      into vr;
    Close v;
  
    select NVL(MAX(l.id), 0)
      into Z_B
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_B;
    commit;
    insert into t_au_activity_log
      (id,
       entity_id,
       role_id,
       ppnum,
       page_id,
       action,
       start_time,
       seq,
       unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       P_NO,
       79,
       'Observation ' || VR.ID || ' is submitted to ' || vr.entity_id ||
       ' Auditee',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select NVL(max(s.parent_enititid), 0)
      into A_F
      from T_AUDITEE_ENTITEE_SUBENTITY s
     inner join t_au_observation o
        on o.entity_id = s.enitity_id
     where o.id = OBS_ID;
    if (vr.Status = 1) then
      if (vr.Isteamlead = 'Y') then
        if (A_F = 0) then
          INSERT INTO T_AU_OBSERVATION_ASSIGNEDTO ot
            (ot.ID,
             ot.OBS_ID,
             ot.OBS_TEXT_ID,
             ot.entity_id,
             ot.ASSIGNEDBY,
             ot.ASSIGNED_DATE,
             ot.lastupdateddate,
             ot.IS_ACTIVE,
             ot.REPLIED,
             OT.ENG_ID)
            SELECT (select COALESCE(max(acc.ID) + 1, 1)
                      from T_AU_OBSERVATION_ASSIGNEDTO acc),
                   vr.OBSERVATSION_ID,
                   vr.text_id,
                   vr.ENTITY_ID,
                   P_NO,
                   trunc(sysdate),
                   sysdate,
                   'Y',
                   'N',
                   VR.ENGPLANID
              FROM dual;
          commit;
        else
          INSERT INTO T_AU_OBSERVATION_ASSIGNEDTO ot
            (ot.ID,
             ot.OBS_ID,
             ot.OBS_TEXT_ID,
             ot.entity_id,
             ot.ASSIGNEDBY,
             ot.ASSIGNED_DATE,
             ot.lastupdateddate,
             ot.IS_ACTIVE,
             ot.REPLIED,
             OT.ENG_ID)
            SELECT (select COALESCE(max(acc.ID) + 1, 1)
                      from T_AU_OBSERVATION_ASSIGNEDTO acc),
                   TT.OBSERVATSION_ID,
                   TT.ID,
                   A_F,
                   O.ENTEREDBY,
                   trunc(sysdate),
                   sysdate,
                   'Y',
                   'N',
                   VR.ENGPLANID
              FROM T_AU_OBSERVATION O
             INNER JOIN T_AU_OBSERVATION_TEXT TT
                ON TT.OBSERVATSION_ID = O.ID
             WHERE O.ID = OBS_ID;
          commit;
        end if;
      
        update t_au_observation t
           set t.memo_date   = trunc(sysdate),
               t.status      = 2,
               T.MEMO_NUMBER =
               (vr.max_num + 1)
         where t.id = OBS_ID
           and t.engplanid = vr.Engplanid;
        commit;
      
        update t_au_observation_text Ot
           set ot.memo_number =
               (vr.max_num + 1)
         where ot.observatsion_id = OBS_ID
           and ot.eng_plan = vr.Engplanid;
        commit;
      
        /*      update t_au_observation_text t
           set t.memo_number = M_F
         where t.id = OBS_ID and t.memo_number is not null and t.memo_number !=0;
        commit;*/
      
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 8;
      
      else
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 22;
      end if;
    else
      open io_cursor for
        select 'Observation already submitted to Auditee' as remarks
          from dual;
    end if;
  end P_SubmitAuditObservationToAuditee;

  procedure P_GetLatestAuditeeResponse(obs_id    IN NUMBER,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select r.reply
        from t_au_observations_auditee_response r
       where r.au_obs_id = obs_id
       order by r.id desc
       FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestAuditeeResponse;

  procedure P_GetOBSERVATIONSAUDITEERESPONSE(OBS_ID    in number,
                                             ENT_ID    in number,
                                             P_NO      in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select ot.REPLY
        from T_AU_OBSERVATIONS_AUDITEE_RESPONSE ot
       where ot.au_obs_id = OBS_ID;
  
  end P_GetOBSERVATIONSAUDITEERESPONSE;

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences(resp_id   in t_au_observations_auditee_evidences.respid%type,
                                                         io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select e.id,
             e.file_name,
             e.text_id,
             e.file_type,
             e.length,
             '' as file_data,
             e.memoid,
             e.enteredby,
             e.entereddate,
             e.lastupdatedby,
             e.lastupdateddate,
             e.sequence,
             e.status,
             e.respid
        from t_au_observations_auditee_evidences e
       where e.respid = resp_id
       order by e.sequence;
  
  end P_get_AUDITEE_OBSERVATION_RESPONSE_evidences;

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_by_obs_id(OBS_ID    in t_au_observation.id%type,
                                                                   io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select e.id,
             e.file_name,
             e.text_id,
             e.file_type,
             e.length,
             '' as file_data,
             e.memoid,
             e.enteredby,
             e.entereddate,
             e.lastupdatedby,
             e.lastupdateddate,
             e.sequence,
             e.status,
             e.respid
        from t_au_observations_auditee_evidences e
       inner join t_au_observation_text t
          on e.text_id = t.id
       where t.observatsion_id = OBS_ID
       order by e.sequence;
  
  end P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_by_obs_id;

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_FileData(FILE_ID   in varchar2,
                                                                  io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select e.id,
             e.file_name,
             e.text_id,
             e.file_type,
             e.length,
             e.file_data,
             e.memoid,
             e.enteredby,
             e.entereddate,
             e.lastupdatedby,
             e.lastupdateddate,
             e.sequence,
             e.status,
             e.respid
        from t_au_observations_auditee_evidences e
       where e.id = FILE_ID
       order by e.sequence;
  
    commit;
  
  end P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_FileData;

  procedure P_UpdateAuditObservationStatus(OBS_ID        IN NUMBER,
                                           NEW_STATUS_ID IN NUMBER,
                                           D_PARA_NO     in varchar2,
                                           Remarks       IN VARCHAR2,
                                           ENT_ID        in number,
                                           P_NO          in number,
                                           R_ID          in number,
                                           io_cursor     OUT t_cursor) is
    R_D varchar2(2);
    Z_B number := 0;
  begin
  
    select NVL(MAX(l.id), 0)
      into Z_B
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_B;
    commit;
    insert into t_au_activity_log
      (id,
       entity_id,
       role_id,
       au_obs_id,
       obs_status,
       ppnum,
       page_id,
       action,
       start_time,
       seq,
       unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       OBS_ID,
       NEW_STATUS_ID,
       P_NO,
       79,
       OBS_ID || ' Observation Status Marked as ' || NEW_STATUS_ID,
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select nvl(max(m.isteamlead), 'O')
      into R_D
      from t_au_team_members m
     inner join t_au_audit_team_tasklist t
        on t.team_id = m.t_id
       and t.teammember_ppno = m.member_ppno
     inner join t_au_observation o
        on o.engplanid = t.eng_plan_id
     where m.member_ppno = P_NO
       and o.id = obs_id;
  
    if (R_D = 'Y') then
      UPDATE T_AU_OBSERVATIONS_AUDITEE_RESPONSE e
         SET e.REMARKS         = Remarks,
             E.LASTUPDATEDBY   = P_NO,
             E.LASTUPDATEDDATE = TRUNC(SYSDATE)
       WHERE e.AU_OBS_ID = OBS_ID;
      COMMIT;
      UPDATE T_AU_OBSERVATION o
         SET o.status              = NEW_STATUS_ID,
             o.Draft_Para_No       = D_PARA_NO,
             o.Draft_Para_Added_On = SYSDATE,
             o.stelled_on = (case
                              when NEW_STATUS_ID = 9 then
                               sysdate
                              else
                               null
                            end),
             o.settled_by = (case
                              when NEW_STATUS_ID = 9 then
                               P_NO
                              else
                               null
                            end)
       WHERE o.id = OBS_ID;
      COMMIT;
      open io_cursor for
        select '1' as ref, r.statusname as remarks
          from t_au_observation_status r
         where r.statusid = NEW_STATUS_ID;
    else
      if (R_ID in (6, 7, 15) and R_D = 'O') then
        UPDATE T_AU_OBSERVATIONS_AUDITEE_RESPONSE e
           SET e.REMARKS         = Remarks,
               E.LASTUPDATEDBY   = P_NO,
               E.LASTUPDATEDDATE = TRUNC(SYSDATE)
         WHERE e.AU_OBS_ID = OBS_ID;
        COMMIT;
        if (NEW_STATUS_ID = 8) then
          UPDATE T_AU_OBSERVATION o
             SET o.status              = NEW_STATUS_ID,
                 o.final_para_no       = D_PARA_NO,
                 o.final_para_added_on = SYSDATE
           WHERE o.id = OBS_ID;
          COMMIT;
        else
          if (NEW_STATUS_ID = 9) then
            UPDATE T_AU_OBSERVATION o
               SET o.status              = NEW_STATUS_ID,
                   o.final_para_no       = D_PARA_NO,
                   o.final_para_added_on = SYSDATE,
                   o.stelled_on          = sysdate,
                   o.settled_by          = p_no
             WHERE o.id = OBS_ID;
            COMMIT;
          end if;
        end if;
      
        open io_cursor for
          select '1' as ref, r.statusname as remarks
            from t_au_observation_status r
           where r.statusid = NEW_STATUS_ID;
      else
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 22;
      end if;
    end if;
  end P_UpdateAuditObservationStatus;

  procedure AUDITOR_RESPONSE(OBS_ID          IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.AU_OBS_ID%type,
                             PPNumber        IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.RECO_BY%TYPE,
                             AUDITOR_COMMENT IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.RECOMMENDATION%type,
                             status          IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.STATUS%type) is
  begin
    INSERT INTO T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION
      (ID,
       AU_OBS_ID,
       RECOMMENDATION,
       RECO_BY,
       RECO_DATE,
       OBS_TEXT_ID,
       RECO_ROLE,
       STATUS,
       SUBMITTED)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION acc),
       OBS_ID,
       AUDITOR_COMMENT,
       PPNumber,
       sysdate,
       (select ot.id
          from t_au_observation_text ot
         WHERE ot.observatsion_id = OBS_ID),
       'TEAM LEAD',
       status,
       'Y');
  
    commit;
  
  end AUDITOR_RESPONSE;

  procedure AUDITOR_REPLY(OBS_ID          IN T_AU_OBSERVATIONS_AUDITOR_REPLY.AU_OBS_ID%type,
                          PPNumber        IN T_AU_OBSERVATIONS_AUDITOR_REPLY.REPLIEDBY%TYPE,
                          AUDITOR_COMMENT IN T_AU_OBSERVATIONS_AUDITOR_REPLY.AUDIT_REPLY%type,
                          status          IN T_AU_OBSERVATIONS_AUDITOR_REPLY.OBS_STATUS%type) is
  begin
    INSERT INTO T_AU_OBSERVATIONS_AUDITOR_REPLY
      (ID,
       AU_OBS_ID,
       AUDIT_REPLY,
       REPLIEDBY,
       REPLIEDDATE,
       OBS_TEXT_ID,
       REPLY_ROLE,
       OBS_STATUS,
       SUBMITTED)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_AU_OBSERVATIONS_AUDITOR_REPLY acc),
       OBS_ID,
       AUDITOR_COMMENT,
       PPNumber,
       sysdate,
       (select ot.id
          from t_au_observation_text ot
         WHERE ot.observatsion_id = OBS_ID),
       (select g.description
          from t_groups g
         inner join t_user_maping mp
            on mp.group_id = g.group_id
         where mp.ppno = PPNumber),
       status,
       'Y');
  
    commit;
  
  end AUDITOR_REPLY;

  procedure P_GetLatestAuditorResponse(obs_id    IN NUMBER,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select r.status, r.recommendation
        from T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION r
       where r.au_obs_id = obs_id
      --and r.reco_role IN ('Team Lead', 'Team Member')
       order by r.id desc
       FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestAuditorResponse;

  procedure P_GetLatestDepartmentalHeadResponse(obs_id    IN NUMBER,
                                                ENT_ID    in number,
                                                P_NO      in number,
                                                R_ID      in number,
                                                io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select r.audit_reply
        from t_au_observations_auditor_reply r
       where r.au_obs_id = obs_id
      --and r.reply_role IN ('Departmental Head / Incharge AZ')
       order by r.id desc
       FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestDepartmentalHeadResponse;

  procedure p_GetObservationEntities(PP_NO     in number,
                                     io_cursor OUT t_cursor) is
    V_F NUMBER := 0;
    E_F number := 0;
  begin
  
    SELECT NVL(max(G.GROUP_ID), 0)
      INTO V_F
      FROM t_User_Maping G
     WHERE g.ppno = PP_NO;
    select NVL(MAX(u.entity_id), 0)
      into E_F
      from t_user u
     where u.ppno = pp_no;
    if (V_F = 1) then
      open io_cursor for
        select distinct t.name || '  ( ' || e.audit_startdate || ' to ' ||
                        e.audit_enddate || ' )' as entity_name,
                        t.code,
                        t.type_id,
                        t.entity_id,
                        j.eng_plan_id as eng_id
          from t_au_audit_joining j
         inner join t_au_plan_eng e
            on e.eng_id = j.eng_plan_id
         inner join t_auditee_entities t
            on t.entity_id = e.entity_id
         inner join t_au_period p
            on e.period_id = p.auditperiodid
         where E.STATUS < '13';
    
    else
      if (V_F = 5) then
        open io_cursor for
          select distinct t.name || '  ( ' || e.audit_startdate || ' to ' ||
                          e.audit_enddate || ' )' as entity_name,
                          t.code,
                          t.type_id,
                          t.entity_id,
                          j.eng_plan_id as eng_id
            from t_au_audit_joining j
           inner join t_au_plan_eng e
              on e.eng_id = j.eng_plan_id
           inner join t_auditee_entities t
              on t.entity_id = e.entity_id
           inner join t_au_period p
              on e.period_id = p.auditperiodid
           inner join t_auditee_entities_maping mp
              on mp.entity_id = e.entity_id
           where E.STATUS < '9'
             and mp.parent_id = E_F;
      else
        if (V_F = 36) then
          open io_cursor for
            select distinct t.name || '  ( ' || e.audit_startdate || ' to ' ||
                            e.audit_enddate || ' )' as entity_name,
                            t.code,
                            t.type_id,
                            t.entity_id,
                            j.eng_plan_id as eng_id
              from t_au_audit_joining j
             inner join t_au_plan_eng e
                on e.eng_id = j.eng_plan_id
             inner join t_auditee_entities t
                on t.entity_id = e.entity_id
             inner join t_au_period p
                on e.period_id = p.auditperiodid
             inner join t_auditee_entities_maping_fad fm
                on fm.entity_id = t.auditby_id
               and fm.ppno = PP_NO
             where E.STATUS < '9';
        ELSE
          if (V_F in (4, 6, 7, 15)) then
            open io_cursor for
              select distinct t.name || '  ( ' || e.audit_startdate ||
                              ' to ' || e.audit_enddate || ' )' as entity_name,
                              t.code,
                              t.type_id,
                              t.entity_id,
                              j.eng_plan_id as eng_id
                from t_au_audit_joining j
               inner join t_au_plan_eng e
                  on e.eng_id = j.eng_plan_id
               inner join t_auditee_entities t
                  on t.entity_id = e.entity_id
               inner join t_au_period p
                  on e.period_id = p.auditperiodid
               where E.STATUS in (12)
                 and e.auditby_id = E_F;
          
          else
            open io_cursor for
              select distinct t.name || ' ( ' || e.audit_startdate ||
                              ' to ' || e.audit_enddate || ' )' as entity_name,
                              t.code,
                              t.type_id,
                              t.entity_id,
                              ja.eng_plan_id as eng_id
                from t_au_audit_joining ja
               inner join t_au_plan_eng e
                  on e.eng_id = ja.eng_plan_id
               inner join t_auditee_entities t
                  on t.entity_id = e.entity_id
               inner join t_au_period p
                  on e.period_id = p.auditperiodid
               where p.status_id = 2
                 and ja.team_mem_ppno = PP_NO
                 and e.status < 13;
          end if;
        end if;
      end if;
    end if;
  end p_GetObservationEntities;

  procedure P_GetManagedObservations(ENGID     IN NUMBER,
                                     OBSID     IN NUMBER,
                                     ENT_ID    in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select c.v_name as Violation,
             otx.headings as heading,
             csb.sub_v_name AS NATURE,
             p.description as PERIOD,
             o.ID as OBS_ID,
             aee.name as ENTITY_NAME,
             nvl(o.memo_number, 0) as MEMO_NO,
             o.severity as OBS_RISK_ID,
             nvl(o.no_of_instances, 1) as noinstances,
             r.description as OBS_RISK,
             o.status as OBS_STATUS_ID,
             ost.Statusname as OBS_STATUS
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_auditee_entities aee
          on e.entity_id = aee.entity_id
       inner join t_control_violation c
          on c.id = o.v_cat_id
       inner join t_control_violation_sub csb
          on csb.id = o.v_cat_nature_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_status ost
          on o.status = ost.statusid
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       inner join t_au_observation_text otx
          on o.id = otx.observatsion_id
       Where o.engplanid = ENGID
       order by o.memo_number, o.id;
  
  end P_GetManagedObservations;

  procedure P_GetManagedObservationstext(OBSID     IN NUMBER,
                                         ENT_ID    in number,
                                         P_NO      in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select c.v_name          as Violation,
             csb.sub_v_name    AS NATURE,
             o.memo_number     as MEMO_NO,
             o.no_of_instances as noinstances,
             ot.text           as OBS_TEXT,
             ot.headings       as OBS_HEADING,
             o.severity        as OBS_RISK_ID,
             r.description     as OBS_RISK
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_control_violation c
          on c.id = o.v_cat_id
       inner join t_control_violation_sub csb
          on csb.id = o.v_cat_nature_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_status ost
          on o.status = ost.statusid
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       Where O.ID = OBSID
       order by o.memo_number;
  
  end P_GetManagedObservationstext;

  procedure P_GetManagedObservationsForBranches(ENGID     IN NUMBER,
                                                OBSID     IN NUMBER,
                                                ENT_ID    in number,
                                                P_NO      in number,
                                                R_ID      in number,
                                                io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select 'N/A' as Process,
             'N/A' as Sub_process,
             ot.headings AS Check_List_Detail,
             ot.headings,
             p.description as PERIOD,
             o.ID as OBS_ID,
             nvl(o.no_of_instances, 1) as noinstances,
             aee.name as ENTITY_NAME,
             aee.entity_id as ENTITY_ID,
             nvl(o.memo_number, 0) as MEMO_NO,
             nvl(o.Draft_Para_No, 0) as DRAFT_PARA,
             o.severity as OBS_RISK_ID,
             r.description as OBS_RISK,
             o.status as OBS_STATUS_ID,
             ost.Statusname as OBS_STATUS,
             o.annex as annex_id,
             ax.code as annex_code,
             (case
               when o.annex = 1 then
                'Y'
               else
                'N'
             end) as DSA
      
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_auditee_entities aee
          on e.entity_id = aee.entity_id
       inner join t_au_observation_status ost
          on o.status = ost.statusid
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_audit_checklist_annexure ax
          on o.annex = ax.id
       inner join t_risk r
          on r.r_id = ax.risk
       Where o.engplanid = ENGID
         and o.status not in (27)
       order by o.memo_number, o.id;
  
  end P_GetManagedObservationsForBranches;

  procedure P_GetManagedObservationsForBranchesTEXT(OBSID     IN NUMBER,
                                                    ENT_ID    in number,
                                                    P_NO      in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select c.heading     as Process,
             c.t_id        as process_id,
             cc.heading    as Sub_process,
             cc.s_id       as Sub_process_id,
             csb.heading   AS Check_List_Detail,
             csb.id        as Check_List_Detail_id,
             o.memo_number as MEMO_NO,
             ot.text       as OBS_TEXT,
             ot.headings   as headings,
             o.severity    as risk_id,
             o.annex       as annexure_id
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_audit_checklist_details csb
          on csb.id = o.checklistdetail_id
       inner join t_audit_checklist_sub cc
          on cc.s_id = csb.s_id
       inner join t_audit_checklist c
          on c.t_id = cc.t_id
       Where O.ID = OBSID
       order by o.memo_number;
  end P_GetManagedObservationsForBranchesTEXT;

  procedure P_GetManagedDraftObservations(ENGID     IN NUMBER,
                                          ENT_ID    in number,
                                          P_NO      in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor) is
    E_F number := 0;
    O_F number := 0;
    M_F number := 0;
  begin
  
    select nvl(max(ob.id), 0)
      into O_F
      from t_au_observation ob
     where ob.engplanid = engid
       and ob.status > 2;
    select nvl(min(ob.id), 0)
      into M_F
      from t_au_observation ob
     where ob.engplanid = engid;
  
    select e.entity_type
      into E_F
      from t_au_plan_eng e
     where e.eng_id = ENGID;
    if (E_F in (6, 5, 7, 17, 25, 21, 20, 23, 22, 28)) then
      if (O_F = 0) then
        OPEN io_Cursor FOR
          select 'B' as etype,
                 o.engplanid as eng_id,
                 null as Violation,
                 null AS NATURE,
                 0 as Process,
                 0 as Sub_process,
                 0 as Check_List_Detail,
                 0 as PERIOD,
                 o.id as OBS_ID,
                 0 as ENTITY_NAME,
                 0 as MEMO_NO,
                 0 as Draft_para,
                 0 as OBS_RISK_ID,
                 0 as AUD_REPLY,
                 0 as OBS_RISK,
                 0 as OBS_STATUS_ID,
                 0 as OBS_STATUS
            from t_au_observation o
           where o.engplanid = engid
             and o.id = M_F;
      else
        OPEN io_Cursor FOR
          select 'B' as etype,
                 o.engplanid as eng_id,
                 null as Violation,
                 null AS NATURE,
                 ch.heading as Process,
                 csb.heading as Sub_process,
                 cd.heading as Check_List_Detail,
                 p.description as PERIOD,
                 o.ID as OBS_ID,
                 aee.name as ENTITY_NAME,
                 o.memo_number as MEMO_NO,
                 nvl(o.draft_para_no, 0) as Draft_para,
                 o.severity as OBS_RISK_ID,
                 ar.reply as AUD_REPLY,
                 r.description as OBS_RISK,
                 o.status as OBS_STATUS_ID,
                 ost.Statusname as OBS_STATUS
            from t_au_observation o
           inner join t_au_plan_eng e
              on o.engplanid = e.eng_id
           inner join t_au_observation_text ot
              on o.id = ot.observatsion_id
           inner join t_auditee_entities aee
              on e.entity_id = aee.entity_id
           inner join t_risk r
              on r.r_id = o.severity
           inner join t_au_observation_status ost
              on o.status = ost.statusid
           inner join t_au_period p
              on p.auditperiodid = e.period_id
           inner join t_audit_checklist_details cd
              on cd.id = o.checklistdetail_id
           inner join t_audit_checklist_sub csb
              on csb.s_id = cd.s_id
           inner join t_audit_checklist ch
              on ch.t_id = csb.t_id
            left join t_au_observations_auditor_response ar
              on ar.au_obs_id = o.id
           where o.engplanid = ENGID
             and o.status not in (1, 2, 7, 23)
           order by o.memo_number, o.id;
      end if;
    else
      if (E_F not in (6, 5, 7, 17, 25, 21, 20, 23, 22, 28)) then
        if (O_F = 0) then
          OPEN io_Cursor FOR
            select 'D' as etype,
                   o.engplanid as eng_id,
                   0 as Violation,
                   0 AS NATURE,
                   Null as Process,
                   Null as Sub_process,
                   Null as Check_List_Detail,
                   0 as PERIOD,
                   o.id as OBS_ID,
                   0 as ENTITY_NAME,
                   0 as MEMO_NO,
                   nvl(o.draft_para_no, 0) as Draft_para,
                   0 as OBS_RISK_ID,
                   0 as AUD_REPLY,
                   0 as OBS_RISK,
                   0 as OBS_STATUS_ID,
                   0 as OBS_STATUS
              from t_au_observation o
             where o.engplanid = engid
               and o.id = M_F;
        else
          OPEN io_Cursor FOR
            select 'D' as etype,
                   o.engplanid as eng_id,
                   c.v_name as Violation,
                   Null as Process,
                   Null as Sub_process,
                   Null as Check_List_Details,
                   csb.sub_v_name AS NATURE,
                   p.description as PERIOD,
                   o.ID as OBS_ID,
                   aee.name as ENTITY_NAME,
                   o.memo_number as MEMO_NO,
                   nvl(o.draft_para_no, 0) as Draft_para,
                   o.severity as OBS_RISK_ID,
                   ar.reply as AUD_REPLY,
                   r.description as OBS_RISK,
                   o.status as OBS_STATUS_ID,
                   ost.Statusname as OBS_STATUS
              from t_au_observation o
             inner join t_au_plan_eng e
                on o.engplanid = e.eng_id
             inner join t_au_observation_text ot
                on o.id = ot.observatsion_id
             inner join t_auditee_entities aee
                on e.entity_id = aee.entity_id
             inner join t_control_violation c
                on c.id = o.v_cat_id
             inner join t_control_violation_sub csb
                on csb.id = o.v_cat_nature_id
             inner join t_risk r
                on r.r_id = o.severity
             inner join t_au_observation_status ost
                on o.status = ost.statusid
             inner join t_au_period p
                on p.auditperiodid = e.period_id
              left join t_au_observations_auditor_response ar
                on ar.au_obs_id = o.id
             where o.engplanid = ENGID
             order by o.memo_number;
        end if;
      end if;
    end if;
  end P_GetManagedDraftObservations;

  procedure P_GetManagedDraftObservationsbranch(ENGID     IN NUMBER,
                                                io_cursor OUT t_cursor) is
    O_F number := 0;
    M_F number := 0;
  begin
    select nvl(max(ob.id), 0)
      into O_F
      from t_au_observation ob
     where ob.engplanid = engid
       and ob.status > 2;
    select nvl(min(ob.id), 0)
      into M_F
      from t_au_observation ob
     where ob.engplanid = engid;
  
    if (O_F = 0) then
      OPEN io_Cursor FOR
        select 'B' as etype,
               o.engplanid as eng_id,
               0 as Process,
               0 as Sub_process,
               0 as Check_List_Detail,
               0 as PERIOD,
               o.id as OBS_ID,
               0 as ENTITY_NAME,
               0 as MEMO_NO,
               nvl(o.draft_para_no, 0) as Draft_para,
               0 as OBS_RISK_ID,
               0 as AUD_REPLY,
               0 as OBS_RISK,
               0 as OBS_STATUS_ID,
               0 as OBS_STATUS
          from t_au_observation o
         where o.engplanid = engid
           and o.id = M_F;
    else
      OPEN io_Cursor FOR
        select 'B' as etype,
               o.engplanid as eng_id,
               ch.heading as Process,
               csb.heading as Sub_process,
               cd.heading as Check_List_Detail,
               p.description as PERIOD,
               o.ID as OBS_ID,
               aee.name as ENTITY_NAME,
               o.memo_number as MEMO_NO,
               nvl(o.draft_para_no, 0) as Draft_para,
               o.severity as OBS_RISK_ID,
               ar.reply as AUD_REPLY,
               r.description as OBS_RISK,
               o.status as OBS_STATUS_ID,
               ost.Statusname as OBS_STATUS
          from t_au_observation o
         inner join t_au_plan_eng e
            on o.engplanid = e.eng_id
         inner join t_au_observation_text ot
            on o.id = ot.observatsion_id
         inner join t_auditee_entities aee
            on e.entity_id = aee.entity_id
         inner join t_risk r
            on r.r_id = o.severity
         inner join t_au_observation_status ost
            on o.status = ost.statusid
         inner join t_au_period p
            on p.auditperiodid = e.period_id
         inner join t_audit_checklist_details cd
            on cd.id = o.checklistdetail_id
         inner join t_audit_checklist_sub csb
            on csb.s_id = cd.s_id
         inner join t_audit_checklist ch
            on ch.t_id = csb.t_id
          left join t_au_observations_auditor_response ar
            on ar.au_obs_id = o.id
         where o.engplanid = ENGID
           and o.status not in (1, 2, 7, 23, 27)
         order by o.memo_number;
    end if;
  end P_GetManagedDraftObservationsbranch;

  procedure P_GetManagedDraftObservationsText(OBSID     IN NUMBER,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select ar.observatsion_id, ar.text as OBS_TEXT
        from t_au_observation_text ar
       where ar.observatsion_id = OBSID;
  end P_GetManagedDraftObservationsText;

  procedure P_GetManagedDraftObservationsreply(OBSID     IN NUMBER,
                                               io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select ar.au_obs_id, ar.reply
        from t_au_observations_auditor_response ar
       where ar.au_obs_id = OBSID;
  end P_GetManagedDraftObservationsreply;

  procedure P_GetManagedDraftObservationsForBranches(ENGID     IN NUMBER,
                                                     ENT_ID    in number,
                                                     P_NO      in number,
                                                     R_ID      in number,
                                                     io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select o.engplanid as eng_id,
             c.heading as Process,
             cc.heading as Sub_process,
             csb.heading AS Check_List_Detail,
             ot.headings,
             p.description as PERIOD,
             o.ID as OBS_ID,
             aee.name as ENTITY_NAME,
             o.memo_number as MEMO_NO,
             nvl(o.Draft_Para_No, 0) as DRAFT_PARA,
             o.severity as OBS_RISK_ID,
             r.description as OBS_RISK,
             ar.reply as Aud_reply,
             o.status as OBS_STATUS_ID,
             ost.Statusname as OBS_STATUS
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_auditee_entities aee
          on e.entity_id = aee.entity_id
       inner join t_audit_checklist_details csb
          on csb.id = o.checklistdetail_id
       inner join t_audit_checklist_sub cc
          on cc.s_id = csb.s_id
       inner join t_audit_checklist c
          on c.t_id = cc.t_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_status ost
          on o.status = ost.statusid
       inner join t_au_period p
          on p.auditperiodid = e.period_id
        left join t_au_observations_auditor_response ar
          on ar.au_obs_id = o.id
       where o.engplanid = ENGID
         and o.status not in (1, 2)
       order by o.memo_number;
  
  end P_GetManagedDraftObservationsForBranches;

  procedure p_GetClosingDraftObservations(ENGID     in number,
                                          ENT_ID    in number,
                                          P_NO      in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor) is
    V_F number := 0;
  
  begin
  
    select min(ts.eng_plan_id)
      into V_F
      from t_au_audit_team_tasklist ts
     where ts.teammember_ppno = P_NO
       and ts.status_id between '1' and '4'
       and ts.isactive = 'Y';
  
    OPEN io_Cursor FOR
      select (select e.name
                from t_auditee_entities e
               inner join t_au_plan_eng ep
                  on ep.entity_id = e.entity_id
               where ep.eng_id = ENGID) as entity_name,
             (select ej.joining_date
                from t_au_audit_joining ej
               where ej.eng_plan_id = ENGID
                 and t.member_ppno = ej.team_mem_ppno) as joining_date,
             (select ep.audit_enddate
                from t_au_plan_eng ep
               where ep.eng_id = ENGID) as completion_date,
             t.member_ppno,
             t.Enteredby as member_name,
             t.engplanid as eng_plan_id,
             t.no_of_Ob as total_no_ob,
             (select m.isteamlead
                from v_getclosingdraft_teammember_summary m
               where m.eng_id = ENGID
                 and m.member_name = t.Enteredby) as teamlead,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = ENGID
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 2) as Submitted_to_Auditee,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = ENGID
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 3) as Responded,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = ENGID
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 4) as Resolved,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = ENGID
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 5) as Added_to_Draft,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = ENGID
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 23) as dropped
        from V_GETCLOSINGDRAFT_TEAM_SUMMARY t
      
       where t.engplanid = ENGID
       order by teamlead desc;
  
  end p_GetClosingDraftObservations;

  procedure P_Closeaudit(engid     in number,
                         ENT_ID    in number,
                         P_NO      in number,
                         R_ID      in number,
                         io_cursor OUT t_cursor) is
    C_F number := 0;
    E_F number := 0;
    T_L varchar(5) := 'N';
    M_F number := 0;
  begin
  
    select nvl(max(tm.isteamlead), 'N')
      into T_L
      from t_au_audit_team_tasklist tl
     inner join t_au_team_members tm
        on tl.team_id = tm.t_id
       and tl.teammember_ppno = tm.member_ppno
     where tl.teammember_ppno = P_NO
       and tl.eng_plan_id = engid;
    if (T_L = 'Y') then
      select nvl(max(o.id), 0)
        into C_F
        from t_au_observation o
       where o.status in (1)
         and o.engplanid = engid;
      select nvl(count(o.id), 0)
        into M_F
        from t_au_observation o
       where o.engplanid = engid;
      if (M_F = 0 or C_F != 0) then
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 19;
      else
        update t_au_audit_joining ji
           set ji.status          = 'C',
               ji.lastupdatedby   = P_NO,
               ji.lastupdateddate = trunc(sysdate)
         where ji.eng_plan_id = engid;
        commit;
        update t_au_audit_team_tasklist t
           set t.isactive = 'Y', t.status_id = '5'
         where t.eng_plan_id = engid;
        commit;
        update t_au_plan_eng e set e.status = 12 where e.eng_id = engid;
        commit;
        update t_au_audit_teams tm
           set tm.status = 5
         where tm.eng_id = engid;
        commit;
      
        insert into t_au_plan_eng_log
          (id, e_id, status_id, createdby_id, created_on, remarks)
        VALUES
          ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
           engid,
           5,
           P_NO,
           sysdate,
           'Completed');
        commit;
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 20;
      end if;
    else
      open io_cursor for
        select r.ref,
               'Only Team Lead of this audit can perform closing' as remarks
          from t_au_remarks r
         where r.id = 20;
    end if;
  end P_Closeaudit;

  --legacy
  Procedure P_get_details_for_manage_observations_summary(ENGID     in number,
                                                          ENT_ID    in number,
                                                          P_NO      in number,
                                                          R_ID      in number,
                                                          io_cursor OUT t_cursor) as
  
  begin
    open io_cursor for
      select v.eng_id,
             v.ppno,
             v.e_name,
             v.status,
             v.team,
             v.Crated as created,
             v.Submit_to_Auditee,
             v.Responded_by_Auditee,
             v.Drop_Resolved_by_team_head,
             v.Added_to_draft,
             v.Added_to_final,
             v.Setteled,
             v.Total
        from v_get_P_get_details_for_manage_observations_summary v
       where v.eng_id = ENGID;
  
  end P_get_details_for_manage_observations_summary;

  procedure P_GetEntitiesForLegacyPara(entityId  in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor out t_cursor) as
  begin
    if (entityId in (112242, 112248, 112243)) then
    
      open io_cursor for
        select distinct e.name, e.entity_id
          from T_AU_OBSERVATION_OLD_CAD_PARAS f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         where e.auditby_id = entityId
           and (f.gist_of_paras is null or exists -- not to be added after complition of cad assignment
                (select 'z'
                   from t_au_observation_old_cad_paras_text t
                  where t.observatsion_id = f.para_id))
           AND F.PARA_STATUS = 8
         order by e.name;
    else
      open io_cursor for
        select distinct e.name, e.entity_id
          from t_au_old_paras_fad f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         where e.auditby_id = entityId
         order by e.name;
    
    end if;
  
  end P_GetEntitiesForLegacyPara;

  procedure P_GetEntitiesForLegacyPara_ho(entityId  in number,
                                          io_cursor out t_cursor) as
  begin
  
    open io_cursor for
      select distinct e.name, e.entity_id
        from T_AU_OBSERVATION_OLD_CAD_PARAS f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       where f.audited_by = entityId
         and (f.gist_of_paras is null or not exists
              (select 'z'
                 from t_au_observation_old_cad_paras_text t
                where t.observatsion_id = f.para_id))
         AND F.PARA_STATUS = 8
      
       order by e.name;
  
  end P_GetEntitiesForLegacyPara_ho;

  procedure P_GetEntitiesForLegacyPara_ho_report(entityId  in number,
                                                 io_cursor out t_cursor) as
  begin
  
    open io_cursor for
      select distinct f.entity_name as name, e.entity_id
        from T_AU_OBSERVATION_OLD_CAD_PARAS f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       where e.entity_id = entityId
         and (f.gist_of_paras is null or not exists
              (select 'z'
                 from t_au_observation_old_cad_paras_text t
                where t.observatsion_id = f.para_id))
         AND F.PARA_STATUS = 8
       order by f.entity_name;
  
  end P_GetEntitiesForLegacyPara_ho_report;

  procedure P_GetLeagacyObservations_ho(entityname in varchar2,
                                        paraRef    in varchar2,
                                        ppno       in number,
                                        io_cursor  out t_cursor) as
  
  begin
  
    if (ppno is null) then
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'AR',
         'P_GetLeagacyObservations',
         'PP No was null',
         ppno,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    
    else
      if (paraRef is null) then
        open io_cursor for
          select f.para_id as ref_p,
                 '0' as id,
                 f.period as audit_period,
                 f.entity_name,
                 '' as entity_code,
                 f.para_no,
                 f.gist_of_paras,
                 f.entity_id,
                 f.audited_by,
                 f.para_status,
                 f.risk_id as riskid,
                 e.type_id,
                 '' as annexure,
                 '' as vol_i_ii,
                 '' as amount_involved
            from T_AU_OBSERVATION_OLD_CAD_PARAS f
           inner join t_auditee_entities e
              on e.entity_id = f.entity_id
           where trim(f.entity_name) = trim(entityname)
             and (NOT EXISTS
                  (select 'z'
                     from T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT t
                    where t.observatsion_id = f.para_id) or
                  (f.status not in (0) or f.risk_id not in (1, 2, 3)))
             AND F.PARA_STATUS = 8
           order by f.period, f.para_no;
      else
        open io_cursor for
          select f.para_id as ref_p,
                 '0' as id,
                 f.period as audit_period,
                 e.description as entity_name,
                 f.para_no,
                 '' as entity_code,
                 e.type_id,
                 f.gist_of_paras,
                 f.entity_id,
                 f.audited_by,
                 f.para_status,
                 nvl(f.v_cat_id, 0) as process,
                 nvl(f.v_cat_nature_id, 0) as sub_Process,
                 nvl(v.risk_id, 0) as process_detail,
                 nvl(f.risk_id, 0) AS RISKID,
                 pt.text as para_text,
                 '' as annexure,
                 '' as vol_i_ii,
                 '' as amount_involved,
                 'D' as ent_type
            from T_AU_OBSERVATION_OLD_CAD_PARAS f
           inner join t_auditee_entities e
              on e.entity_id = f.entity_id
            left join t_control_violation_sub v
              on f.v_cat_nature_id = v.id
            left join T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT pt
              on f.para_id = pt.observatsion_id
           where f.para_id = cast(paraRef as number)
          -- and f.status = 1
           order by f.period, f.para_no;
      end if;
      INSERT INTO T_AU_DATA_VALIDATION_HO_LOG
        (ID, REF_P, PARA_REVIEWED, REMARKS, FAD_DATE, DESK_OFFICER)
      
      VALUES
        ((SELECT COALESCE(max(u.Id) + 1, 1)
           FROM T_AU_DATA_VALIDATION_HO_LOG U),
         paraRef,
         1,
         'Para has been Viewed',
         sysdate,
         PPNO);
      COMMIT;
    end if;
  end P_GetLeagacyObservations_ho;

  procedure P_Settel_legacy_para_ho(RefP       in number,
                                    new_status in number,
                                    PPNO       IN NUMBER,
                                    remark     in varchar2,
                                    io_cursor  out t_cursor) as
    S_F number := 0;
  begin
    if (new_status = 6) then
      S_F := 9;
    else
      S_F := new_status;
    end if;
    update t_Au_Observation_Old_Cad_Paras t
       set t.para_status = S_F, t.status = 2
     where t.para_id = refp;
    commit;
    insert into T_AU_OBSERVATION_OLD_CAD_PARAS_SETTLE_LOG
      (ID, PARA_ID, PP_NUM, STATUS_CHANGE_DATE, REMARKS)
    VALUES
      ((SELECT COALESCE(max(u.Id) + 1, 1)
         FROM T_AU_DATA_VALIDATION_HO_LOG U),
       RefP,
       PPNO,
       sysdate,
       remark);
    COMMIT;
    open io_cursor for
      select Refp || '  has been marked as settled' as remarks from dual;
  
  end P_Settel_legacy_para_ho;

  procedure P_delete_legacy_para_ho(RefP      in number,
                                    PPNO      in number,
                                    io_cursor out t_cursor) as
  begin
  
    update t_Au_Observation_Old_Cad_Paras t
       set t.para_status = 0, t.status = 0
     where t.para_id = refp;
    commit;
    insert into T_AU_OBSERVATION_OLD_CAD_PARAS_SETTLE_LOG
      (ID, PARA_ID, PP_NUM, STATUS_CHANGE_DATE, REMARKS)
    VALUES
      ((SELECT COALESCE(max(u.Id) + 1, 1)
         FROM T_AU_DATA_VALIDATION_HO_LOG U),
       RefP,
       PPNO,
       sysdate,
       'Duplicate Para Delted');
    COMMIT;
    open io_cursor for
      select Refp || ' is Deleted' as remarks from dual;
  
  end P_delete_legacy_para_ho;

  procedure P_GetLeagacyObservations(entityId  in number,
                                     paraRef   in varchar2,
                                     ppno      in number,
                                     io_cursor out t_cursor) as
    v_F number := 0;
  begin
    select e.auditby_id
      into V_F
      from t_auditee_entities e
     where e.entity_id = entityId;
    if (ppno is null) then
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'AR',
         'P_GetLeagacyObservations',
         'PP No was null',
         entityId,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    
    else
      if (V_F in (112242, 112248)) then
        if (paraRef is null) then
          open io_cursor for
            select f.para_id as ref_p,
                   '0' as id,
                   f.period as audit_period,
                   f.entity_name,
                   '' as entity_code,
                   f.para_no,
                   f.gist_of_paras,
                   f.entity_id,
                   f.audited_by,
                   f.para_status,
                   f.risk_id,
                   e.type_id,
                   '' as annexure,
                   '' as vol_i_ii,
                   '' as amount_involved
              from T_AU_OBSERVATION_OLD_CAD_PARAS f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
             where f.entity_id = entityId
               and f.status = 1
             order by f.period, f.para_no;
        else
          open io_cursor for
            select f.para_id as ref_p,
                   '0' as id,
                   f.period as audit_period,
                   e.description as entity_name,
                   f.para_no,
                   '' as entity_code,
                   e.type_id,
                   f.gist_of_paras,
                   f.entity_id,
                   f.audited_by,
                   f.para_status,
                   nvl(f.v_cat_id, 0) as process,
                   nvl(f.v_cat_nature_id, 0) as sub_Process,
                   nvl(f.risk_id, 0) as process_detail,
                   pt.text as para_text,
                   '' as annexure,
                   '' as vol_i_ii,
                   '' as amount_involved,
                   'D' as ent_type
              from T_AU_OBSERVATION_OLD_CAD_PARAS f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
              left join T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT pt
                on f.para_id = pt.observatsion_id
             where e.entity_id = entityId
               and f.para_id = cast(paraRef as number)
            -- and f.status = 1
             order by f.period, f.para_no;
        end if;
      else
        if (paraRef is null) then
          open io_cursor for
            select f.id,
                   f.ref_p,
                   f.entity_code,
                   f.type_id,
                   f.audit_period,
                   f.entity_name,
                   f.para_no,
                   f.entity_id,
                   f.gist_of_paras,
                   f.annexure,
                   f.vol_i_ii,
                   f.amount_involved
              from t_au_old_paras_fad f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
             where f.entity_id = entityId
               and exists (select 'z'
                      from t_au_old_paras_fad_text nt
                     where f.ref_p = nt.ref_p)
             order by f.audit_period, f.para_no;
        else
          open io_cursor for
            select f.*, pt.para_text, 'B' as ent_type
              from t_au_old_paras_fad f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
              left join t_au_old_paras_fad_text pt
                on f.ref_p = pt.ref_p
             where e.entity_id = entityId
               and f.ref_p = paraRef
            --and f.update_status = 1
             order by f.audit_period, f.para_no;
        
          INSERT INTO T_AU_OLD_PARAS_FAD_LOG
            (ID, REF_P, Remarks, Created_Date, Created_By)
          VALUES
            ((SELECT COALESCE(max(u.Id) + 1, 1)
               FROM T_AU_OLD_PARAS_FAD_LOG U),
             paraRef,
             'Para has been Viewed',
             sysdate,
             PPNO);
          COMMIT;
        end if;
      end if;
    end if;
  
  end P_GetLeagacyObservations;

  procedure p_get_legacy_para_responsibles(paraRef   in number,
                                           io_cursor OUT t_cursor) as
    R_F number := 0;
  begin
    select NVL(count(rf.pp_no), 0)
      into R_F
      from t_au_old_paras_fad_responsibility_assigned rf
     where rf.obs_id = paraRef;
    if (R_F = 0) then
      open io_cursor for
        select f.pp_no,
               e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
               '' as LOANCASE,
               '' as LCAMOUNT,
               '' as ACCNUMBER,
               '' as ACAMOUNT
          from t_au_observation_old_paras_responibility_assigned f
         inner join v_service_employeeinfo e
            on e.PPNO = f.pp_no
         WHERE f.ref_p = paraRef
           and f.del_status = 'N';
    else
      open io_cursor for
        select f.pp_no,
               e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
               f.loan_case as LOANCASE,
               f.lc_amount as LCAMOUNT,
               f.account_number as ACCNUMBER,
               f.ac_amount as ACAMOUNT
          from t_au_old_paras_fad_responsibility_assigned f
         inner join v_service_employeeinfo e
            on e.PPNO = f.pp_no
         WHERE f.obs_id = paraRef
           and f.is_active = 'Y';
    end if;
  
  end p_get_legacy_para_responsibles;

  procedure P_update_legacy_Para_text(ref_id       in varchar2,
                                      obtext       in clob,
                                      process_id   in number,
                                      subprocessid in number,
                                      checklistid  in number,
                                      pp_no        in number,
                                      risk_id      in number,
                                      io_cursor    OUT t_cursor) is
  
    v_F number := 0;
    t_f number := 0;
    C_K number := 0;
    CID number := 0;
    cursor V is
      select f.id as old_para_id,
             f.entity_id,
             e.type_id,
             e.auditby_id,
             f.audit_period,
             trunc(f.entered_on) as entered_on,
             e.code,
             f.para_no,
             f.gist_of_paras
      
        from t_au_old_paras_fad f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       where f.ref_p = ref_id;
    vr1 V%rowtype;
  begin
    Open V;
    Fetch V
      into vr1;
    Close v;
  
    select nvl(max(c.com_id), 0)
      into CID
      from ais_t_au_post_compliance c
     where c.old_para_id = vr1.old_para_id;
    if (CID = 0) then
      insert into ais_t_au_post_compliance
        (com_id,
         old_para_id,
         audit_period,
         entity_id,
         entity_code,
         audited_by,
         entity_type_id,
         com_cycle,
         com_status,
         com_stage,
         para_status,
         para_no,
         gist_of_paras,
         ind,
         para_added_on,
         risk)
      values
        ((select COALESCE(max(ca.com_id) + 1, 1)
           from ais_t_au_post_compliance ca),
         vr1.old_para_id,
         vr1.audit_period,
         vr1.entity_id,
         vr1.code,
         vr1.auditby_id,
         vr1.type_id,
         0,
         8,
         (case when vr1.type_id = 6 then 13 else 21 end),
         8,
         vr1.para_no,
         vr1.gist_of_paras,
         'O',
         vr1.entered_on,
         risk_id);
      commit;
    end if;
    select e.entity_id into V_F from t_user e where e.ppno = pp_no;
    select nvl(max(fd.id), 0)
      into t_f
      from t_au_old_paras_fad_text fd
     where fd.ref_p = ref_id;
    if (pp_no is not null) then
    
      if (V_F in (112242, 112248)) then
        select nvl(max(ff.id), 0)
          into C_K
          from t_au_observation_old_cad_paras_text ff
         where ff.observatsion_id = cast(ref_id as number);
        if (C_K = 0) then
          insert into t_au_observation_old_cad_paras_text
            (id, observatsion_id, text, remarks, enteredby, entereddate)
          values
            ((select COALESCE(max(p.id) + 1, 1)
               from t_au_observation_old_cad_paras_text p),
             cast(ref_id as number),
             obtext,
             'Para Updated',
             pp_no,
             sysdate);
          commit;
        else
          update t_au_observation_old_cad_paras_text t
             set t.text = obtext
           where t.observatsion_id = cast(ref_id as number);
        end if;
      
        update t_au_observation_old_cad_paras fd
           set fd.v_cat_id        = process_id,
               fd.v_cat_nature_id = subprocessid,
               fd.risk_id         = risk_id,
               fd.status          = 2
         where fd.para_id = cast(ref_id as number);
        commit;
      
        UPDATE T_AU_DATA_VALIDATION_HO_LOG LG
           SET lg.para_final   = 1,
               LG.DESK_OFFICER = PP_NO,
               lg.remarks      = 'Para has been Updated with changes'
         WHERE LG.REF_P = REF_ID;
        COMMIT;
      
      end if;
    
      if (subprocessid = 0) then
        if (t_f != 0) then
          update t_au_old_paras_fad_text ot
             set ot.para_text = obtext
           where ot.ref_p = ref_id;
          commit;
        else
          insert into t_au_old_paras_fad_text
            (id, ref_p, para_text)
          values
            ((SELECT COALESCE(max(u.ID) + 1, 1)
               FROM t_au_old_paras_fad_text U),
             ref_id,
             obtext);
          commit;
        end if;
        update t_au_old_paras_fad o
           set o.az_status_updated_by = pp_no,
               o.update_status        = 2,
               o.az_updated_on        = sysdate,
               o.risk                 = risk_id
         where o.ref_p = ref_id;
        commit;
        UPDATE T_AU_OLD_PARAS_FAD_LOG LG
           SET lg.up_text_status = 1,
               LG.CREATED_BY     = PP_NO,
               lg.remarks        = 'Para has been Updated without changes'
         WHERE LG.REF_P = REF_ID
           and lg.created_by = PP_NO;
        COMMIT;
      else
        update t_au_old_paras_fad o
           set o.process_detail       = checklistid,
               o.az_status_updated_by = pp_no,
               o.update_status        = 2,
               o.risk                 = risk_id,
               o.az_updated_on        = sysdate
         where o.ref_p = ref_id;
        commit;
        if (t_f != 0) then
          update t_au_old_paras_fad_text ot
             set ot.para_text = obtext
           where ot.ref_p = ref_id;
          commit;
        else
          insert into t_au_old_paras_fad_text
            (id, ref_p, para_text)
          values
            ((SELECT COALESCE(max(u.ID) + 1, 1)
               FROM t_au_old_paras_fad_text U),
             ref_id,
             obtext);
          commit;
        end if;
        UPDATE T_AU_OLD_PARAS_FAD_LOG LG
           SET lg.up_text_status = 1,
               LG.CREATED_BY     = PP_NO,
               lg.remarks        = 'Para has been Updated with changes'
         WHERE LG.REF_P = REF_ID
           and lg.created_by = PP_NO;
        COMMIT;
      end if;
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 29;
    else
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'AR',
         'P_legacy_Para_text',
         'PP No was null',
         ref_id,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    end if;
  end P_update_legacy_Para_text;

  procedure p_add_para_responsibility(refid        in number,
                                      PPNO         in number,
                                      AZ_Entity_id in number,
                                      user_ppno    in number,
                                      lC_no        in varchar2,
                                      LC_AMOUNT    in varchar2,
                                      AC_NO        in varchar2,
                                      AC_AMOUNT    in varchar2,
                                      refp         in varchar2,
                                      io_cursor    OUT t_cursor) as
    R_F number := 0;
  begin
    if (user_ppno is not null) then
      insert into t_au_old_paras_fad_responsibility_assigned
        (id,
         obs_id,
         pp_no,
         assignedby,
         lastupdatedby,
         lastupdateddate,
         is_active,
         loan_case,
         account_number,
         lc_amount,
         ac_amount,
         ref_p)
      values
        ((select COALESCE(max(acc.ID) + 1, 1)
           from t_au_old_paras_fad_responsibility_assigned acc),
         refid,
         ppno,
         AZ_Entity_id,
         user_ppno,
         sysdate,
         'Y',
         lC_no,
         AC_NO,
         LC_AMOUNT,
         AC_AMOUNT,
         refp);
      commit;
      UPDATE T_AU_OLD_PARAS_FAD_LOG LG
         SET lg.up_res_status = 1,
             lg.resp_by       = user_ppno,
             lg.resp_on       = sysdate
       WHERE LG.REF_P = REFP
         and lg.created_by = user_ppno;
      COMMIT;
      select count(f.pp_no)
        into r_f
        from t_au_old_paras_fad_responsibility_assigned f
       where f.obs_id = refid
         and f.is_active = 'Y';
      open io_cursor for
        select r_F || ' responsibilities added' as remarks from dual;
    else
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'AR',
         'p_add_para_responsibility',
         'PP No was null',
         refid,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    end if;
  end p_add_para_responsibility;

  procedure p_delete_para_responsibility(refid     in number,
                                         PPNO      in number,
                                         io_cursor OUT t_cursor) as
    R_F number := 0;
  begin
    select NVL(count(rf.pp_no), 0)
      into R_F
      from t_au_old_paras_fad_responsibility_assigned rf
     where rf.obs_id = refid;
    if (R_F = 0) then
      update t_au_observation_old_paras_responibility_assigned r
         set r.del_status = 'Y'
       where r.ref_p = refid
         and r.pp_no = ppno;
      commit;
    
      open io_cursor for
        select 'Responsibility of ' || PPNO || ' Deleted' as remarks
          from dual;
    else
      update t_au_old_paras_fad_responsibility_assigned r
         set r.is_active = 'N'
       where r.obs_id = refid
         and r.pp_no = ppno;
      commit;
    
      open io_cursor for
        select 'Responsibility of ' || PPNO || ' Deleted' as remarks
          from dual;
    end if;
  end p_delete_para_responsibility;

  procedure P_no_update_legacy_Para_text(ref_id    in varchar2,
                                         ppno      in number,
                                         risk_id   in number,
                                         io_cursor OUT t_cursor) is
    t_f number := 0;
  begin
    select nvl(max(fd.id), 0)
      into t_f
      from t_au_old_paras_fad_text fd
     where fd.ref_p = ref_id;
    if (t_F != 0) then
      if (ppno is not null) then
      
        update t_au_old_paras_fad o
           set o.az_status_updated_by = ppno,
               o.update_status        = 3,
               o.risk                 = risk_id,
               o.az_updated_on        = sysdate
         where o.ref_p = ref_id;
        commit;
        UPDATE T_AU_OLD_PARAS_FAD_LOG LG
           SET lg.up_text_status = 1,
               LG.CREATED_BY     = PPNO,
               lg.remarks        = 'Para has been Updated without any changes'
         WHERE LG.REF_P = REF_ID
           and lg.created_by = PPNO;
        COMMIT;
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 29;
      else
        insert into t_au_error_logs
          (id,
           package_name,
           procedure_name,
           nature,
           ppno,
           record_on,
           status)
        Values
          ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
           'AR',
           'P_legacy_Para_text',
           'PP No was null',
           ref_id,
           sysdate,
           'to be checked');
        commit;
        open io_cursor for
          select 'Your session has been expired, Logout and Login again.' as remarks
            from dual;
      end if;
    else
      open io_cursor for
        select 'ADD para text first' as remarks from dual;
    end if;
  end P_no_update_legacy_Para_text;

  procedure P_GetLeagacyObservations_for_gist_update(entityId  in number,
                                                     paraRef   in varchar2,
                                                     ppno      in number,
                                                     io_cursor out t_cursor) as
    v_F number := 0;
  begin
    select e.auditby_id
      into V_F
      from t_auditee_entities e
     where e.entity_id = entityId;
    if (ppno is null) then
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'AR',
         'P_GetLeagacyObservations',
         'PP No was null',
         entityId,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    
    else
      if (V_F in (112242, 112248)) then
        if (paraRef is null) then
          open io_cursor for
            select f.para_id as ref_p,
                   '0' as id,
                   f.period as audit_period,
                   f.entity_name,
                   '' as entity_code,
                   f.para_no,
                   f.gist_of_paras,
                   f.entity_id,
                   f.audited_by,
                   f.para_status,
                   f.risk_id,
                   e.type_id,
                   '' as annexure,
                   '' as vol_i_ii,
                   '' as amount_involved
              from T_AU_OBSERVATION_OLD_CAD_PARAS f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
             where f.entity_id = entityId
               and f.gist_of_paras is null
               and not exists
             (select 'z'
                      from T_AU_OLD_PARAS_GIST_UPDATE_LOG g
                     where g.ref_p = cast(f.para_id as varchar2(50))
                       and g.approve_on is null)
             order by f.period, f.para_no;
        else
          open io_cursor for
            select f.para_id as ref_p,
                   '0' as id,
                   f.period as audit_period,
                   e.description as entity_name,
                   f.para_no,
                   '' as entity_code,
                   e.type_id,
                   f.gist_of_paras,
                   f.entity_id,
                   f.audited_by,
                   f.para_status,
                   nvl(f.v_cat_id, 0) as process,
                   nvl(f.v_cat_nature_id, 0) as sub_Process,
                   nvl(f.risk_id, 0) as process_detail,
                   pt.text as para_text,
                   '' as annexure,
                   '' as vol_i_ii,
                   '' as amount_involved,
                   'D' as ent_type
              from T_AU_OBSERVATION_OLD_CAD_PARAS f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
              left join T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT pt
                on f.para_id = pt.observatsion_id
             where e.entity_id = entityId
               and f.para_id = cast(paraRef as number)
            --and f.status = 1
             order by f.period, f.para_no;
        end if;
      else
        if (paraRef is null) then
          open io_cursor for
            select f.id,
                   f.ref_p,
                   f.entity_code,
                   f.type_id,
                   f.audit_period,
                   f.entity_name,
                   f.para_no,
                   f.entity_id,
                   f.gist_of_paras,
                   f.annexure,
                   f.vol_i_ii,
                   f.amount_involved
              from t_au_old_paras_fad f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
             where f.entity_id = entityId
               and f.para_status = 8
             order by f.audit_period, f.para_no;
        else
          open io_cursor for
            select f.*, '' as para_text, 'B' as ent_type
              from t_au_old_paras_fad f
             inner join t_auditee_entities e
                on e.entity_id = f.entity_id
             where e.entity_id = entityId
               and f.ref_p = paraRef
               and f.para_status = 8
             order by f.audit_period, f.para_no;
        
          INSERT INTO T_AU_OLD_PARAS_GIST_UPDATE_LOG
            (ID,
             REF_P,
             GIST_STATUS,
             PARA_NO_STATUS,
             CREATED_BY,
             CREATED_DATE)
          
          VALUES
            ((SELECT COALESCE(max(u.Id) + 1, 1)
               FROM T_AU_OLD_PARAS_GIST_UPDATE_LOG U),
             paraRef,
             '0',
             '0',
             ppno,
             sysdate);
          COMMIT;
        end if;
      end if;
    end if;
  
  end P_GetLeagacyObservations_for_gist_update;

  procedure P_update_legacy_Para_Gist(ref_id    in varchar2,
                                      gist      in varchar2,
                                      parano    in varchar2,
                                      pp_no     in number,
                                      u_entity  in number,
                                      io_cursor OUT t_cursor) is
  
  begin
  
    if (pp_no is not null) then
    
      INSERT INTO T_AU_OLD_PARAS_GIST_UPDATE_LOG
        (ID,
         REF_P,
         GIST_STATUS,
         PARA_NO_STATUS,
         CREATED_BY,
         CREATED_DATE,
         GIST_PARA,
         PARA_NO_U)
      
      VALUES
        ((SELECT COALESCE(max(u.Id) + 1, 1)
           FROM T_AU_OLD_PARAS_GIST_UPDATE_LOG U),
         ref_id,
         1,
         1,
         pp_no,
         sysdate,
         gist,
         parano);
      commit;
      open io_cursor for
        select 'Gist and Para No has been updation submited for Authorization' as remarks
          from dual;
    else
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'AR',
         'P_legacy_Para_text',
         'PP No was null',
         ref_id,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    end if;
  end P_update_legacy_Para_Gist;

  procedure P_Get_legacy_Para_to_authorize(ENTITYID  IN NUMBER,
                                           io_cursor OUT t_cursor) as
  begin
  
    if (ENTITYID in (112242, 112248)) then
      open io_cursor for
        SELECT f.para_id as ref_p,
               trunc(f.period) as Audit_year,
               e.code as e_code,
               e.name as e_name,
               'Regular' as nature,
               f.para_no as para_no,
               f.gist_of_paras as old_gist_of_paras,
               lg.gist_para as gist_of_paras,
               '' as annexure,
               '' as amount_involved,
               '' as vol_i_ii
        
          FROM T_AU_OBSERVATION_OLD_CAD_PARAS f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         inner join T_AU_OLD_PARAS_GIST_UPDATE_LOG lg
            on lg.ref_p = cast(f.para_id as varchar2(10))
         where lg.approve_by is null
           and f.audited_by = ENTITYID;
    
    else
      OPEN io_cursor FOR
        SELECT f.ref_p,
               p.description || '  ' || p.audit_year as Audit_year,
               e.code as e_code,
               e.name as e_name,
               n.description as nature,
               lg.para_no_u as para_no,
               f.gist_of_paras as old_gist_of_paras,
               lg.gist_para as gist_of_paras,
               f.annexure,
               f.amount_involved,
               f.vol_i_ii
        
          FROM t_au_old_paras_fad f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         inner join T_AU_OLD_AUDIT_NATURE n
            on n.nid = f.nature_of_audit
         inner join T_AU_OLD_AUDIT_PERIOD p
            on p.audit_year = f.audit_period
         inner join T_AU_OLD_PARAS_GIST_UPDATE_LOG lg
            on lg.ref_p = f.ref_p
         where lg.approve_by is null
           and f.audited_by = ENTITYID;
    end if;
  end P_Get_legacy_Para_to_authorize;

  procedure P_Authorize_Para_Gist(RefP      in varchar2,
                                  gist      in varchar2,
                                  parano    in varchar2,
                                  PPNO      IN NUMBER,
                                  ENTITYID  in number,
                                  io_cursor OUT t_cursor) as
  begin
  
    if (ENTITYID in (112242, 112248)) then
      UPDATE T_AU_OBSERVATION_OLD_CAD_PARAS alc
         SET alc.gist_of_paras = gist, alc.para_no = cast(parano as number)
       WHERE alc.para_id = cast(RefP as number);
      commit;
    
      update ais_t_au_post_compliance c
         set c.gist_of_paras = gist
       where c.new_para_id = cast(RefP as number)
         and c.ind = 'C';
      commit;
    
      UPDATE T_AU_OLD_PARAS_GIST_UPDATE_LOG al
         SET al.gist_para  = null,
             al.para_no_u  = null,
             al.approve_by = ppno,
             al.approve_on = sysdate
       WHERE al.ref_p = RefP;
      commit;
      open io_cursor for
        select 'Gist and Para No has been updated' as remarks from dual;
    else
    
      UPDATE T_AU_OLD_PARAS_FAD al
         SET al.gist_of_paras = gist, al.para_no = parano
       WHERE al.ref_p = RefP;
      commit;
      UPDATE T_AU_OLD_PARAS_GIST_UPDATE_LOG al
         SET al.gist_para  = null,
             al.para_no_u  = null,
             al.approve_by = ppno,
             al.approve_on = sysdate
       WHERE al.ref_p = RefP;
      commit;
      open io_cursor for
        select 'Gist and Para No has been updated' as remarks from dual;
    end if;
  end P_Authorize_Para_Gist;

  --WORKING PAPER PROCEDURES

  procedure P_AddLoanCaseFile(ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              ENGID     in T_WORKING_PAPER_LOAN_CASE_FILE.ENG_ID%type,
                              LCNUMBER  in T_WORKING_PAPER_LOAN_CASE_FILE.LC_NUMBER%type,
                              LCAmount  in T_WORKING_PAPER_LOAN_CASE_FILE.AMOUNT%type,
                              DISBDATE  in T_WORKING_PAPER_LOAN_CASE_FILE.DISB_DATE%type,
                              LC        in T_WORKING_PAPER_LOAN_CASE_FILE.CATEGORY%type,
                              OBS       in T_WORKING_PAPER_LOAN_CASE_FILE.OBSERVATION%type,
                              PARA_NO   in T_WORKING_PAPER_LOAN_CASE_FILE.PARA_NO%type,
                              io_cursor OUT t_cursor) as
  BEGIN
    insert into T_WORKING_PAPER_LOAN_CASE_FILE
      (LC_ID,
       LC_NUMBER,
       AMOUNT,
       DISB_DATE,
       CATEGORY,
       OBSERVATION,
       PARA_NO,
       ENTERED_BY,
       ENTERED_ON,
       ENG_ID)
    VALUES
      ((SELECT COALESCE(max(PP.LC_ID) + 1, 1)
         FROM T_WORKING_PAPER_LOAN_CASE_FILE PP),
       LCNUMBER,
       LCAmount,
       DISBDATE,
       LC,
       OBS,
       PARA_NO,
       P_NO,
       sysdate,
       ENGID);
    commit;
    OPEN io_cursor FOR
      SELECT 'Loan Case file Added with LC Number ' || LCNUMBER as remarks
        from dual;
  end P_AddLoanCaseFile;

  procedure P_GetLoanCaseFile(ENGID     in number,
                              ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor) as
  BEGIN
  
    OPEN io_cursor FOR
      Select *
        from T_WORKING_PAPER_LOAN_CASE_FILE lc
       where lc.Eng_Id = ENGID;
  
  end P_GetLoanCaseFile;

  procedure P_AddVoucherChecking(ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 ENGID     in T_WORKING_PAPER_VOUCHER_CHECKING.ENG_ID%type,
                                 VNUMBER   in T_WORKING_PAPER_VOUCHER_CHECKING.V_NUMBER%type,
                                 OBS       in T_WORKING_PAPER_VOUCHER_CHECKING.OBSERVATION%type,
                                 PARA_NO   in T_WORKING_PAPER_VOUCHER_CHECKING.PARA_NO%type,
                                 io_cursor OUT t_cursor) as
  BEGIN
    insert into T_WORKING_PAPER_VOUCHER_CHECKING
      (V_ID,
       V_NUMBER,
       OBSERVATION,
       PARA_NO,
       ENTERED_BY,
       ENTERED_ON,
       ENG_ID)
    VALUES
      ((SELECT COALESCE(max(PP.V_ID) + 1, 1)
         FROM T_WORKING_PAPER_VOUCHER_CHECKING PP),
       VNUMBER,
       OBS,
       PARA_NO,
       P_NO,
       sysdate,
       ENGID);
    commit;
    OPEN io_cursor FOR
      SELECT 'Voucher Added with Voucher Number ' || VNUMBER as remarks
        from dual;
  end P_AddVoucherChecking;

  procedure P_GetVoucherChecking(ENGID     in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) as
  BEGIN
    OPEN io_cursor FOR
      Select *
        from T_WORKING_PAPER_VOUCHER_CHECKING lc
       where lc.Eng_Id = ENGID;
  
  end P_GetVoucherChecking;

  procedure P_AddAccountOpeningDetails(ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       ENGID     in T_WORKING_PAPER_ACCOUNT_OPENING.ENG_ID%type,
                                       VNUMBER   in T_WORKING_PAPER_ACCOUNT_OPENING.V_NUMBER%type,
                                       ANATURE   in T_WORKING_PAPER_ACCOUNT_OPENING.A_NATURE%type,
                                       OBS       in T_WORKING_PAPER_ACCOUNT_OPENING.OBSERVATION%type,
                                       PARA_NO   in T_WORKING_PAPER_ACCOUNT_OPENING.PARA_NO%type,
                                       io_cursor OUT t_cursor) as
  BEGIN
    insert into T_WORKING_PAPER_ACCOUNT_OPENING
      (A_ID,
       V_NUMBER,
       A_NATURE,
       OBSERVATION,
       PARA_NO,
       ENTERED_BY,
       ENTERED_ON,
       ENG_ID)
    VALUES
      ((SELECT COALESCE(max(PP.A_ID) + 1, 1)
         FROM T_WORKING_PAPER_ACCOUNT_OPENING PP),
       VNUMBER,
       ANATURE,
       OBS,
       PARA_NO,
       P_NO,
       sysdate,
       ENGID);
    commit;
    OPEN io_cursor FOR
      SELECT 'Account Details Added with Voucher Number ' || VNUMBER as remarks
        from dual;
  end P_AddAccountOpeningDetails;

  procedure P_GetAccountOpeningDetails(ENGID     in number,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor) as
  BEGIN
  
    OPEN io_cursor FOR
      Select *
        from T_WORKING_PAPER_ACCOUNT_OPENING lc
       where lc.eng_id = ENGID;
  
  end P_GetAccountOpeningDetails;

  procedure P_AddFixedAssetsDetails(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    ENGID     in T_WORKING_PAPER_FIXED_ASSETS.ENG_ID%type,
                                    ANAME     in T_WORKING_PAPER_FIXED_ASSETS.ASSET_NAME%type,
                                    PHYEX     in T_WORKING_PAPER_FIXED_ASSETS.PHYSICAL_EXISTANCE%type,
                                    LFAR      in T_WORKING_PAPER_FIXED_ASSETS.LOCATION_AS_PER_FAR%type,
                                    DIFF      in T_WORKING_PAPER_FIXED_ASSETS.DIFFERENCE%type,
                                    REM       in T_WORKING_PAPER_FIXED_ASSETS.REMARKS%type,
                                    io_cursor OUT t_cursor) as
  BEGIN
    insert into T_WORKING_PAPER_FIXED_ASSETS
      (FA_ID,
       ASSET_NAME,
       PHYSICAL_EXISTANCE,
       LOCATION_AS_PER_FAR,
       DIFFERENCE,
       REMARKS,
       ENTERED_BY,
       ENTERED_ON,
       ENG_ID)
    VALUES
      ((SELECT COALESCE(max(PP.FA_ID) + 1, 1)
         FROM T_WORKING_PAPER_FIXED_ASSETS PP),
       ANAME,
       PHYEX,
       LFAR,
       DIFF,
       REM,
       P_NO,
       sysdate,
       ENGID);
    commit;
    OPEN io_cursor FOR
      SELECT 'Fixed Assets Added ' as remarks from dual;
  end P_AddFixedAssetsDetails;

  procedure P_GetFixedAssetsDetails(ENGID     in number,
                                    ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor) as
  BEGIN
  
    OPEN io_cursor FOR
      Select *
        from T_WORKING_PAPER_FIXED_ASSETS lc
       where lc.Eng_Id = ENGID;
  
  end P_GetFixedAssetsDetails;

  procedure P_AddCashCounterDetails(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    ENGID     in T_WORKING_PAPER_CASH_COUNT.ENG_ID%type,
                                    DVAL      in T_WORKING_PAPER_CASH_COUNT.DENOMINATION_VAULT%type,
                                    CVAL      in T_WORKING_PAPER_CASH_COUNT.NO_CURRENCY_NOTES_VAULT%type,
                                    AVAL      in T_WORKING_PAPER_CASH_COUNT.TOTAL_AMOUNT_VAULT%type,
                                    DSR       in T_WORKING_PAPER_CASH_COUNT.DENOMINATION_SAFE_REGISTER%type,
                                    CSR       in T_WORKING_PAPER_CASH_COUNT.NO_CURRENCY_NOTES_SAFE_REGISTER%type,
                                    ASR       in T_WORKING_PAPER_CASH_COUNT.TOTAL_AMOUNT_SAFE_REGISTER%type,
                                    DIFF      in T_WORKING_PAPER_CASH_COUNT.DIFFERENCE%type,
                                    io_cursor OUT t_cursor) as
  BEGIN
    insert into T_WORKING_PAPER_CASH_COUNT
      (ID,
       DENOMINATION_VAULT,
       NO_CURRENCY_NOTES_VAULT,
       TOTAL_AMOUNT_VAULT,
       DENOMINATION_SAFE_REGISTER,
       NO_CURRENCY_NOTES_SAFE_REGISTER,
       TOTAL_AMOUNT_SAFE_REGISTER,
       DIFFERENCE,
       ENTERED_BY,
       ENTERED_ON,
       ENG_ID)
    VALUES
      ((SELECT COALESCE(max(PP.ID) + 1, 1)
         FROM T_WORKING_PAPER_CASH_COUNT PP),
       DVAL,
       CVAL,
       AVAL,
       DSR,
       CSR,
       ASR,
       DIFF,
       P_NO,
       sysdate,
       ENGID);
    commit;
    OPEN io_cursor FOR
      SELECT 'Cash Counter Details Added ' as remarks from dual;
  end P_AddCashCounterDetails;

  procedure P_GetCashCounterDetails(ENGID     in number,
                                    ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor) as
  BEGIN
  
    OPEN io_cursor FOR
      Select * from T_WORKING_PAPER_CASH_COUNT lc where lc.Eng_Id = ENGID;
  
  end P_GetCashCounterDetails;

  Procedure P_get_entities_for_manage_observations(ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) as
  
  begin
  
    if (R_ID in (10, 18)) then
      OPEN io_cursor FOR
        select distinct (et.entity_id) as entity_id,
                        et.name || ' ( ' || e.audit_startdate || ' to ' ||
                        e.audit_enddate || ' )' as name,
                        e.eng_id
          from t_au_plan_eng e
         inner join t_auditee_entities et
            on e.entity_id = et.entity_id
         inner join t_au_audit_team_tasklist t
            on t.eng_plan_id = e.eng_id
        
         where t.status_id between 1 and 5
           and t.teammember_ppno = P_NO;
    else
      if (R_ID in (6, 7, 15, 16)) then
        OPEN io_cursor FOR
          select distinct (et.entity_id) as entity_id,
                          et.name || ' ( ' || e.audit_startdate || ' to ' ||
                          e.audit_enddate || ' )' as name,
                          e.eng_id
            from t_au_plan_eng e
           inner join t_auditee_entities et
              on e.entity_id = et.entity_id
           inner join t_au_audit_team_tasklist t
              on t.eng_plan_id = e.eng_id
          
           where e.auditby_id = ENT_ID
             and e.status between 4 and 13;
      
      else
        if (R_ID in (9)) then
          OPEN io_cursor FOR
            select distinct (et.entity_id) as entity_id,
                            et.name || ' ( ' || e.audit_startdate || ' to ' ||
                            e.audit_enddate || ' )' as name,
                            e.eng_id
              from t_au_plan_eng e
             inner join t_auditee_entities et
                on e.entity_id = et.entity_id
             inner join t_au_audit_team_tasklist t
                on t.eng_plan_id = e.eng_id
             inner join t_auditee_entities_maping_fad fd
                on fd.entity_id = e.auditby_id
             where e.status between 4 and 12
               and fd.ppno = P_NO;
        
        end if;
      end if;
    end if;
  
  end P_get_entities_for_manage_observations;

  Procedure P_get_details_for_manage_observations(ENGID     in number,
                                                  ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor) as
  
  begin
  
    if (R_ID in (10, 18)) then
      OPEN io_cursor FOR
        select (case
                 when e.entity_id != o.entity_id then
                  'ONT'
                 else
                  null
               end) IND,
               et.name as e_name,
               NVl(o.memo_number, 0) as memo,
               nvl(o.draft_para_no, 0) as draft_para,
               nvl(o.final_para_no, 0) as Final_para,
               nvl(t.headings, 'Please add Heading') as Title,
               ty.audit_type as T_IND,
               o.id as OBD_ID,
               s.statusname as status,
               o.status as status_id
          from t_au_plan_eng e
         inner join t_au_observation o
            on o.engplanid = e.eng_id
         inner join t_au_observation_text t
            on t.observatsion_id = o.id
           and t.eng_plan = o.engplanid
         inner join t_auditee_ent_types ty
            on ty.autid = e.entity_type
         inner join t_auditee_entities et
            on o.entity_id = et.entity_id
         inner join t_au_observation_status s
            on s.statusid = o.status
        
         where e.status between 10 and 13
           and o.engplanid = ENGID;
    else
      if (R_ID in (6, 7, 15, 16)) then
        OPEN io_cursor FOR
          select (case
                   when e.entity_id != o.entity_id then
                    'ONT'
                   else
                    null
                 end) IND,
                 et.name as e_name,
                 NVl(o.memo_number, 0) as memo,
                 nvl(o.draft_para_no, 0) as draft_para,
                 nvl(o.final_para_no, 0) as Final_para,
                 nvl(t.headings, 'Please add Heading') as Title,
                 ty.audit_type as T_IND,
                 o.id as OBD_ID,
                 s.statusname as status,
                 o.status as status_id
            from t_au_plan_eng e
           inner join t_au_observation o
              on o.engplanid = e.eng_id
           inner join t_au_observation_text t
              on t.observatsion_id = o.id
             and t.eng_plan = o.engplanid
           inner join t_auditee_ent_types ty
              on ty.autid = e.entity_type
           inner join t_auditee_entities et
              on o.entity_id = et.entity_id
           inner join t_au_observation_status s
              on s.statusid = o.status
           where e.status between 12 and 13
             and o.status between 5 and 10
             and o.engplanid = ENGID;
      
      else
        if (R_ID in (9)) then
          OPEN io_cursor FOR
            select (case
                     when e.entity_id = o.entity_id then
                      'ONT'
                     else
                      null
                   end) IND,
                   et.name as e_name,
                   NVl(o.memo_number, 0) as memo,
                   nvl(o.draft_para_no, 0) as draft_para,
                   nvl(o.final_para_no, 0) as Final_para,
                   nvl(t.headings, 'Please add Heading') as Title,
                   ty.audit_type as T_IND,
                   o.id as OBD_ID,
                   s.statusname as status,
                   o.status as status_id
              from t_au_plan_eng e
             inner join t_au_observation o
                on o.engplanid = e.eng_id
             inner join t_au_observation_text t
                on t.observatsion_id = o.id
               and t.eng_plan = o.engplanid
             inner join t_auditee_ent_types ty
                on ty.autid = e.entity_type
             inner join t_auditee_entities et
                on o.entity_id = et.entity_id
             inner join t_au_observation_status s
                on s.statusid = o.status
             where e.status between 10 and 13
               and o.status between 1 and 10
               and o.engplanid = ENGID;
        
        end if;
      end if;
    end if;
  
  end P_get_details_for_manage_observations;

  Procedure P_get_details_for_manage_observations_text(Obs_id    in number,
                                                       IND       in varchar2,
                                                       ENT_ID    in number,
                                                       P_NO      in number,
                                                       R_ID      in number,
                                                       io_cursor OUT t_cursor) as
  
  begin
    if (IND = 'B') then
      open io_cursor for
        select o.obsid,
               o.CP_ID,
               o.CP,
               o.PSN_ID,
               o.CD_Id,
               o.PSN,
               o.CD,
               o.Instances,
               o.Amount,
               o.text,
               o.Title,
               o.RISK_ID,
               o.RISK,
               o.IND
          from v_para_text_field_ais o
         where o.obsid = obs_id;
    else
      open io_cursor for
        select o.obsid,
               o.CP_ID,
               o.CP,
               o.PSN_ID,
               o.PSN,
               o.CD_Id,
               o.cd,
               o.Instances,
               o.Amount,
               o.text,
               o.Title,
               o.RISK_ID,
               o.RISK,
               o.IND
          from v_para_text_department_ais o
         where o.obsid = obs_id;
    end if;
  
  end P_get_details_for_manage_observations_text;

  procedure p_GetManageAuditParasEntities(P_NO      in number,
                                          R_ID      in number,
                                          ENT_ID    in number,
                                          io_cursor OUT t_cursor) is
  begin
  
    if (R_ID = 1) then
      open io_cursor for
      
        select f.entity_id, e.name
          from t_au_observation_fad f
         inner join t_auditee_entities e
            on f.entity_id = e.entity_id
         group by f.entity_id, e.name;
    
    else
    
      open io_cursor for
        select f.entity_id, e.name
          from t_au_observation_fad f
         inner join t_auditee_entities e
            on f.entity_id = e.entity_id
         where e.auditby_id = ENT_ID
         group by f.entity_id, e.name;
    end if;
  end p_GetManageAuditParasEntities;

  PROCEDURE P_GetObservationsForManageAuditParas(S_ENT_ID  IN NUMBER,
                                                 ENT_ID    IN NUMBER,
                                                 P_NO      IN NUMBER,
                                                 R_ID      IN NUMBER,
                                                 io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT f.com_id,
             f.old_para_id,
             f.new_para_id,
             f.para_no,
             f.audit_period,
             f.gist_of_paras,
             r.description AS risk,
             r.r_id AS risk_id,
             f.IND,
             NVL(n.code, 0) AS Annex,
             n.id AS Annex_ID,
             -- Get para_text by joining each type, then using CASE
             CASE
               WHEN f.IND = 'O' THEN
                ft.para_text
               WHEN f.IND = 'A' THEN
                ot.text
               WHEN f.IND = 'C' THEN
                nt.text
               ELSE
                NULL
             END AS para_text,
             0 AS amount,
             f.no_of_instances AS no_instances,
             f.annex_ref_id,
             
             ud.annexure_ref_id,
             c.REFERENCE_TYPE,
             c.INSTRUCTIONSTITLE,
             trunc(c.INSTRUCTIONSDATE) as INSTRUCTIONS_DATE,
             E.NAME AS DIVISION
      
        FROM Ais_t_Au_Post_Compliance f
       INNER JOIN t_risk r
          ON f.risk = r.r_id
        LEFT JOIN t_audit_checklist_annexure n
          ON f.ANNEX = n.id
        LEFT JOIN t_au_old_paras_fad_text ft
          ON (f.IND = 'O' AND ft.para_id = f.old_para_id)
        LEFT JOIN t_au_observation_text ot
          ON (f.IND = 'A' AND ot.observatsion_id = f.new_para_id)
        LEFT JOIN t_au_observation_old_cad_paras_text nt
          ON (f.IND = 'C' AND nt.observatsion_id = f.new_para_id)
      
        left join T_AU_OBSERVATION_UPDATED_REFERENCE ud
          on ud.c_id = f.com_id
         AND ud.status = 'A'
        left join t_Audit_Checklist_Annexure_Circular c
          on c.id = ud.annexure_ref_id
        left join t_Auditee_Entities e
          on c.division_ent_id = e.entity_id
      
       WHERE f.entity_id = S_ENT_ID
         AND f.para_status = 8
         AND f.audited_by = ENT_ID
         and not exists (select 1
                from T_AU_OBSERVATION_UPDATED_REFERENCE ud2
               where ud2.c_id = f.com_id
                 AND ud2.status = 'P')
       ORDER BY f.audit_period, f.para_no;
  END P_GetObservationsForManageAuditParas;

  procedure P_Get_responsibility(Para_ID   IN NUMBER,
                                 IND       in varchar2,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
    
      select v.pp_no,
             v.loan_case,
             v.lc_amount,
             v.ac_amount,
             v.amount_involved,
             v.account_number,
             (case
               when v.status in ('Y', 'N') then
                'Responsibity Fixed'
               when v.status = 'U' then
                'Update is Pending for Authorization'
               when v.status = 'D' then
                'Deletion is Pending for Authorization'
               when v.status = 'A' then
                'Addition is Pending for Authorization'
             end) as remarks,
             (case
               when v.status in ('Y', 'N') then
                'O'
               else
                'N'
             end) as R_IND,
             a.employeefirstname || ' ' || a.employeelastname as emp_name
        from v_get_auditee_pp_responsibility_for_authorization v
       inner join v_service_employeeinfo a
          on a.ppno = v.pp_no
       where para_id = case
               when IND = 'O' then
                v.OLD_PARA_ID
               else
                v.new_para_id
             end;
  end P_Get_responsibility;

  procedure P_Update_Audit_Paras(COM_ID         in number,
                                 N_PARA_ID      IN NUMBER,
                                 O_PARA_ID      IN NUMBER,
                                 D_PARA_NO      IN VARCHAR2,
                                 D_AUDIT_PERIOD in VARCHAR2,
                                 D_GIST         in varchar2,
                                 D_RISK         in number,
                                 D_ANNEX        in number,
                                 D_IND          in varchar2,
                                 D_PARA_TEXT    in clob,
                                 D_AMOUNT       in decimal,
                                 D_INSTANCES    in number,
                                 
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) IS
  
  begin
    update T_AU_OBSERVATION_UPDATED_REFERENCE c
       set c.status = 'O'
     where c.c_id = com_id;
    commit;
    insert into T_AU_OBSERVATION_UPDATED_REFERENCE
      (C_ID,
       N_PARA_ID,
       O_PARA_ID,
       PARA_NO,
       AUDIT_PERIOD,
       GIST_OF_PARAS,
       RISK,
       IND,
       ANNEX_ID,
       NO_INSTANCES,
       AMOUNT,
       UPDATED_BY,
       UPDATED_ON,
       P_TYPE_IND,
       PARA_TEXT,
       STATUS)
    
    values
      (COM_ID,
       N_PARA_ID,
       O_PARA_ID,
       D_PARA_NO,
       D_AUDIT_PERIOD,
       D_GIST,
       D_Risk,
       D_IND,
       D_ANNEX,
       D_INSTANCES,
       D_AMOUNT,
       P_NO,
       sysdate,
       D_IND,
       D_PARA_TEXT,
       'P');
    COMMIT;
    open io_cursor for
      select 'Para Updated and Submitted for Authorization ' as remarks
        from dual;
  
  END P_Update_Audit_Paras;

  PROCEDURE P_Update_responsibility(IND        IN VARCHAR2,
                                    C_ID       IN NUMBER,
                                    O_Para_ID  IN NUMBER,
                                    N_PARA_ID  IN NUMBER,
                                    PPNO       IN NUMBER,
                                    L_CASE     IN NUMBER,
                                    LC_AMOUNT  IN NUMBER,
                                    AC_Amount  IN NUMBER,
                                    NO_account IN NUMBER,
                                    Remarks    IN VARCHAR2,
                                    U_D_action IN VARCHAR2,
                                    E_NAME     IN VARCHAR2,
                                    ENT_ID     IN NUMBER,
                                    P_NO       IN NUMBER,
                                    R_ID       IN NUMBER,
                                    io_cursor  OUT t_cursor) IS
    v_f       NUMBER := 0;
    v_err_msg VARCHAR2(4000);
  BEGIN
    -- COM_ID + PPNO uniquely identifies the row for old para flow
    SELECT NVL(MAX(p.para_id), 0)
      INTO v_f
      FROM t_Au_Observation_Responsibility_Updated p
     WHERE p.pp_no = PPNO
       AND p.com_id = C_ID;
  
    IF (v_f = 0 OR v_f IS NULL) THEN
      INSERT INTO t_Au_Observation_Responsibility_Updated
        (para_id,
         IND,
         Old_Para_Id,
         New_Para_Id,
         Pp_No,
         Loan_Case,
         Loan_Amount,
         Acccount_Amount,
         Reasons,
         action,
         Account_no,
         Updated_By,
         Updated_On,
         com_id)
      VALUES
        ((SELECT COALESCE(MAX(acc.para_ID) + 1, 1)
           FROM t_Au_Observation_Responsibility_Updated acc),
         IND,
         NULL, -- not used in COM flow
         NULL, -- not used in COM flow
         PPNO,
         L_CASE,
         LC_AMOUNT,
         AC_Amount,
         Remarks,
         U_D_action,
         NO_Account,
         P_NO,
         SYSDATE,
         C_ID);
    ELSE
      UPDATE t_Au_Observation_Responsibility_Updated c
         SET c.loan_case       = L_CASE,
             c.loan_amount     = LC_AMOUNT,
             c.account_no      = NO_account,
             c.acccount_amount = AC_Amount,
             c.action          = U_D_action,
             c.authorized_by   = NULL,
             c.authorized_on   = NULL,
             c.com_id          = C_ID
       WHERE c.para_id = v_f;
    END IF;
  
    COMMIT;
  
    OPEN io_cursor FOR
      SELECT 'Responsibility ' || (CASE U_D_action
               WHEN 'A' THEN
                'Added'
               WHEN 'D' THEN
                'Deleted'
               WHEN 'U' THEN
                'Updated'
             END) || ' and Submitted for Authorization' AS remarks
        FROM dual;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_err_msg := SQLERRM;
      OPEN io_cursor FOR
        SELECT 'Error: ' || v_err_msg AS remarks FROM dual;
  END P_Update_responsibility;

  Procedure P_GET_Para_details_for_Authorize(ENT_ID    in number,
                                             P_NO      in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor) is
  
  begin
  
    Open io_cursor for
      SELECT f.com_id,
             f.old_para_id,
             f.new_para_id,
             e.name as AUDITEE,
             f.para_no,
             f.audit_period,
             f.gist_of_paras,
             r.description AS risk,
             r.r_id AS risk_id,
             f.IND,
             f.ind as P_TYPE_IND,
             NVL(n.code, 0) AS Annex,
             n.id AS Annex_ID,
             -- Get para_text by joining each type, then using CASE
             CASE
               WHEN f.IND = 'O' THEN
                ft.para_text
               WHEN f.IND = 'A' THEN
                ot.text
               WHEN f.IND = 'C' THEN
                nt.text
               ELSE
                NULL
             END AS para_text,
             0 AS amount,
             f.no_of_instances AS no_instances,
             f.annex_ref_id,
             ud.updated_by as UPDATED_BY,
             ud.updated_on as UPDATED_ON,
             f.annex_ref_id as ANNEXURE_REF_ID
        FROM Ais_t_Au_Post_Compliance f
       INNER JOIN t_risk r
          ON f.risk = r.r_id
       inner join T_AU_OBSERVATION_UPDATED_REFERENCE ud
          on ud.c_id = f.com_id
         and ud.status = 'P'
        left join t_auditee_entities e
          on f.entity_id = e.entity_id
        LEFT JOIN t_audit_checklist_annexure n
          ON f.ANNEX = n.id
        LEFT JOIN t_au_old_paras_fad_text ft
          ON (f.IND = 'O' AND ft.para_id = f.old_para_id)
        LEFT JOIN t_au_observation_text ot
          ON (f.IND = 'A' AND ot.observatsion_id = f.new_para_id)
        LEFT JOIN t_au_observation_old_cad_paras_text nt
          ON (f.IND = 'C' AND nt.observatsion_id = f.new_para_id)
       WHERE f.para_status = 8
            
         AND f.audited_by = ENT_ID
       ORDER BY f.audit_period, f.para_no;
  
  end P_GET_Para_details_for_Authorize;

  Procedure P_GET_Para_changes_for_Authorize(Com_ID    in number,
                                             io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select d.c_id as com_ID,
             d.n_para_id as new_para_id,
             d.o_para_id as old_para_id,
             d.para_no,
             d.audit_period,
             d.gist_of_paras as gist_of_para,
             r.description as risk,
             r.r_id as risk_id,
             d.ind,
             d.annex_id,
             d.no_instances,
             d.amount,
             d.p_type_ind,
             d.para_text,
             c.REFERENCE_TYPE,
             c.INSTRUCTIONSTITLE,
             trunc(c.INSTRUCTIONSDATE) as INSTRUCTIONS_DATE,
             d.annexure_ref_id as annex_ref_id,
             E.NAME AS DIVISION
        from T_AU_OBSERVATION_UPDATED_REFERENCE d
        left join t_Audit_Checklist_Annexure_Circular c
          on d.annexure_ref_id = c.id
       INNER JOIN T_AUDIT_CHECKLIST_ANNEXURE A
          ON D.ANNEX_ID = A.ID
        left JOIN T_AUDITEE_ENTITIES E
          ON E.ENTITY_ID = C.DIVISION_ENT_ID
       inner join t_risk r
          on r.r_id = a.risk
       where d.c_id = Com_ID
         and d.status = 'P';
  end P_GET_Para_changes_for_Authorize;

  Procedure P_Get_responsibility_for_Authorize(C_ID      in number,
                                               io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
    
      select r.ind,
             r.old_para_id,
             r.new_para_id,
             r.pp_no,
             r.loan_case,
             r.loan_amount,
             r.acccount_amount,
             r.account_no,
             r.reasons,
             (case
               when r.action = 'A' then
                'New Addition'
               when r.action = 'D' then
                'Delete Responsibility'
               when r.action = 'U' then
                'Update'
             end) as action,
             r.updated_by,
             r.updated_on,
             r.authorized_by,
             r.authorized_on
        from t_Au_Observation_Responsibility_Updated r
       where r.com_id = C_ID
         and r.authorized_on is null;
  
  end P_Get_responsibility_for_Authorize;

  Procedure P_Delete_responsibility(IND       in varchar2,
                                    O_Para_ID IN NUMBER,
                                    N_PARA_ID in number,
                                    PPNO      in number,
                                    io_cursor OUT t_cursor) is
  
  begin
  
    commit;
  
    open io_cursor for
      select 'Responsibilility deleted ' as remarks from dual;
  
  end P_Delete_responsibility;

  PROCEDURE P_Authorize_Update_Audit_Paras(C_ID           IN NUMBER,
                                           N_PARA_ID      IN NUMBER,
                                           O_PARA_ID      IN NUMBER,
                                           D_PARA_ID      IN NUMBER,
                                           D_PARA_NO      IN VARCHAR2,
                                           D_AUDIT_PERIOD IN VARCHAR2,
                                           D_GIST         IN VARCHAR2,
                                           D_RISK         IN NUMBER,
                                           D_ANNEX        IN NUMBER,
                                           D_IND          IN VARCHAR2,
                                           D_PARA_TEXT    IN CLOB,
                                           D_AMOUNT       IN DECIMAL,
                                           D_INSTANCES    IN NUMBER,
                                           ENT_ID         IN NUMBER,
                                           P_NO           IN NUMBER,
                                           R_ID           IN NUMBER,
                                           P_DECISION     IN VARCHAR2,
                                           io_cursor      OUT t_cursor) IS
    N_F NUMBER := 0;
  
    PROCEDURE set_ref_status(p_status IN VARCHAR2) IS
    BEGIN
      IF p_status = 'A' THEN
        UPDATE T_AU_OBSERVATION_UPDATED_REFERENCE ud
           SET ud.AUTHORIZED_BY = P_NO,
               ud.AUTHORIZED_ON = SYSDATE,
               ud.STATUS        = 'A',
               ud.UPDATED_BY    = P_NO,
               ud.UPDATED_ON    = SYSDATE
         WHERE ud.C_ID = N_F
           AND NVL(ud.STATUS, 'P') IN ('P', 'R'); -- allow authorize from pending or referred
      ELSIF p_status = 'R' THEN
        UPDATE T_AU_OBSERVATION_UPDATED_REFERENCE ud
           SET ud.AUTHORIZED_BY = NULL,
               ud.AUTHORIZED_ON = NULL,
               ud.STATUS        = 'R',
               ud.UPDATED_BY    = P_NO,
               ud.UPDATED_ON    = SYSDATE
         WHERE ud.C_ID = N_F;
      END IF;
    END;
  BEGIN
    N_F := C_ID;
  
    IF P_DECISION NOT IN ('A', 'R') THEN
      RAISE_APPLICATION_ERROR(-20001, 'Invalid P_DECISION. Use A or R.');
    END IF;
  
    -- ====================
    -- Refer back (no data writes to para tables)
    -- ====================
    IF P_DECISION = 'R' THEN
      -- Keep responsibilities pending (don’t authorize anything)
      UPDATE T_AU_OBSERVATION_RESPONSIBILITY_UPDATED
         SET AUTHORIZED_BY = NULL, AUTHORIZED_ON = NULL
       WHERE COM_ID = C_ID
         AND AUTHORIZED_ON IS NULL;
    
      set_ref_status('R');
      COMMIT;
    
      OPEN io_cursor FOR
        SELECT 'Para has been referred back' AS remarks FROM dual;
      RETURN;
    
    ELSIF P_DECISION = 'A' THEN
    
      -- ====================
      -- Authorize (existing behavior)
      -- ====================
      IF D_IND = 'A' THEN
        UPDATE t_au_observation o
           SET o.final_para_no   = D_PARA_NO,
               o.severity        = D_RISK,
               o.annex           = D_ANNEX,
               o.amount_involved = D_AMOUNT,
               o.no_of_instances = D_INSTANCES,
               o.lastupdatedby   = P_NO,
               o.lastupdateddate = SYSDATE
         WHERE o.id = N_PARA_ID;
      
        UPDATE t_au_observation_text t
           SET t.headings        = D_GIST,
               t.text            = D_PARA_TEXT,
               t.lastupdatedby   = P_NO,
               t.lastupdateddate = SYSDATE
         WHERE t.observatsion_id = N_PARA_ID;
      
        UPDATE ais_t_au_post_compliance c
           SET c.para_no         = D_PARA_NO,
               c.gist_of_paras   = D_GIST,
               c.audit_period    = D_AUDIT_PERIOD,
               c.risk            = D_RISK,
               c.annex           = D_ANNEX,
               c.amount          = D_AMOUNT,
               c.no_of_instances = D_INSTANCES
         WHERE c.com_id = C_ID;
      
        FOR j IN (SELECT rm.*
                    FROM T_AU_OBSERVATION_RESPONSIBILITY_UPDATED rm
                   WHERE rm.com_id = C_ID
                     AND rm.authorized_on IS NULL) LOOP
          IF j.action = 'A' THEN
            INSERT INTO t_au_observation_responibility_assigned
              (id,
               obs_id,
               assignedby,
               pp_no,
               is_active,
               remarks,
               loan_case,
               account_number,
               lc_amount,
               ac_amount,
               com_id)
            VALUES
              ((SELECT NVL(MAX(id), 0) + 1
                 FROM t_au_observation_responibility_assigned),
               j.new_para_id,
               P_NO,
               j.pp_no,
               'Y',
               'Y',
               j.loan_case,
               j.account_no,
               j.loan_amount,
               j.acccount_amount,
               j.com_id);
          
            UPDATE T_AU_OBSERVATION_RESPONSIBILITY_UPDATED d
               SET d.action        = 'Y',
                   d.authorized_by = P_NO,
                   d.authorized_on = SYSDATE
             WHERE d.com_id = j.com_id;
          ELSE
            UPDATE t_au_observation_responibility_assigned ar
               SET ar.loan_case      = j.loan_case,
                   ar.lc_amount      = j.loan_amount,
                   ar.account_number = j.account_no,
                   ar.ac_amount      = j.acccount_amount,
                   ar.is_active = CASE
                                    WHEN j.action = 'D' THEN
                                     'N'
                                    ELSE
                                     'Y'
                                  END
             WHERE ar.pp_no = j.pp_no
               AND ar.com_id = j.com_id;
          
            UPDATE T_AU_OBSERVATION_RESPONSIBILITY_UPDATED dm
               SET dm.authorized_by = P_NO, dm.authorized_on = SYSDATE
             WHERE dm.com_id = j.com_id;
          END IF;
        END LOOP;
      
      ELSE
        -- Old-paras branch
        UPDATE t_au_old_paras_fad d
           SET d.audit_period         = D_AUDIT_PERIOD,
               d.para_no              = D_PARA_NO,
               d.gist_of_paras        = D_GIST,
               d.risk                 = D_RISK,
               d.amount_involved      = D_AMOUNT,
               d.no_of_instances      = D_INSTANCES,
               d.az_status_updated_by = P_NO,
               d.annex                = D_ANNEX,
               d.az_updated_on        = SYSDATE
         WHERE d.com_id = C_ID;
      
        UPDATE t_au_old_paras_fad_text t
           SET t.para_text = D_PARA_TEXT
         WHERE t.com_id = C_ID;
      
        UPDATE ais_t_au_post_compliance c
           SET c.para_no       = D_PARA_NO,
               c.gist_of_paras = D_GIST,
               c.audit_period  = D_AUDIT_PERIOD,
               c.risk          = D_RISK
         WHERE c.com_id = C_ID;
      
        FOR j IN (SELECT rm.*
                    FROM T_AU_OBSERVATION_RESPONSIBILITY_UPDATED rm
                   WHERE rm.com_id = C_ID
                     AND rm.authorized_on IS NULL) LOOP
          IF j.action = 'A' THEN
            INSERT INTO t_au_old_paras_fad_responsibility_assigned
              (id,
               obs_id,
               assignedby,
               pp_no,
               is_active,
               remarks,
               loan_case,
               account_number,
               lc_amount,
               ac_amount,
               com_id)
            VALUES
              ((SELECT NVL(MAX(id), 0) + 1
                 FROM t_au_old_paras_fad_responsibility_assigned),
               j.old_para_id,
               P_NO,
               j.pp_no,
               'Y',
               'Y',
               j.loan_case,
               j.account_no,
               j.loan_amount,
               j.acccount_amount,
               j.com_id);
          
            UPDATE T_AU_OBSERVATION_RESPONSIBILITY_UPDATED d
               SET d.action        = 'Y',
                   d.authorized_by = P_NO,
                   d.authorized_on = SYSDATE
             WHERE d.com_id = j.com_id;
          ELSE
            UPDATE t_au_old_paras_fad_responsibility_assigned ar
               SET ar.loan_case      = j.loan_case,
                   ar.lc_amount      = j.loan_amount,
                   ar.account_number = j.account_no,
                   ar.ac_amount      = j.acccount_amount,
                   ar.is_active = CASE
                                    WHEN j.action = 'D' THEN
                                     'N'
                                    ELSE
                                     'Y'
                                  END
             WHERE ar.pp_no = j.pp_no
               AND ar.com_id = j.com_id;
          
            UPDATE T_AU_OBSERVATION_RESPONSIBILITY_UPDATED dm
               SET dm.authorized_by = P_NO, dm.authorized_on = SYSDATE
             WHERE dm.com_id = j.com_id;
          END IF;
        END LOOP;
      END IF;
    
      -- Finalize reference and logs for Authorization
      set_ref_status('A');
    
      UPDATE T_AU_PARAS_STATUS_CHANGE_LOG lg
         SET lg.authorized_on = SYSDATE, lg.authorized_by = P_NO
       WHERE lg.com_id = C_ID;
    
      COMMIT;
    
      OPEN io_cursor FOR
        SELECT 'Para details have been Authorized' AS remarks FROM dual;
    END IF;
  END P_Authorize_Update_Audit_Paras;

  procedure P_GET_LC_DETAILS(LC_NO     VARCHAR2,
                             B_CODE    VARCHAR2,
                             P_NO      number,
                             ENT_Id    NUMBER,
                             io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select v.loan_app_id,
             v.loan_case_no,
             v.cnic,
             v.name,
             v.app_date,
             v.cad_receive_date,
             v.sanction_date,
             '' as disb_date,
             v.mco_ppno,
             v.mco_name,
             v.manager_ppno,
             v.manager_name,
             v.rgm_ppno,
             v.rgm_name,
             v.cad_reviewer,
             v.cad_name,
             v.cad_authorizer,
             v.cad_authorizer_name,
             d.Outstanding_amount,
             d.disbursed_amount
      
        from v_loan_outstanding_amount d
       inner join v_loan_sanction_process v
          on d.loan_app_id = v.loan_app_id
         and d.org_unitid = v.org_unitid
      
       where v.loan_case_no = LC_NO
         AND v.CODE = B_CODE;
  
  end P_GET_LC_DETAILS;

  procedure P_draft_dsa(EID           number,
                        OBSID         number,
                        RESP_PPNO     number,
                        RESP_ROW_ID   number,
                        ENGAGEMENT_ID number,
                        P_NO          number,
                        ENT_Id        NUMBER,
                        R_ID          number,
                        io_cursor     OUT t_cursor) is
  begin
  
    INSERT INTO t_au_dsa
      (id,
       entity_id,
       audited_by,
       obs_id,
       ppno,
       resp_id,
       created_by,
       created_on,
       status,
       eng_id)
    VALUES
      ((SELECT COALESCE(max(d.id) + 1, 1) FROM t_au_dsa d),
       EID,
       ENT_Id,
       OBSID,
       RESP_PPNO,
       RESP_ROW_ID,
       P_NO,
       SYSDATE,
       1,
       ENGAGEMENT_ID);
    commit;
    insert into t_au_dsa_text
      (id, dsa_id, dsa_body)
    Values
      ((SELECT COALESCE(max(d.id) + 1, 1) FROM t_au_dsa_text d),
       (SELECT COALESCE(max(d.id), 0) FROM t_au_dsa d),
       (select ot.text
          from t_au_observation_text ot
         where ot.observatsion_id = OBSID));
    commit;
  
    OPEN io_cursor FOR
      SELECT ' DSA added for ' || ' ' || RESP_PPNO as remarks,
             (SELECT COALESCE(max(d.id), 0) FROM t_au_dsa d) as DSA_ID
        FROM dual;
  
  end P_draft_dsa;

  Procedure P_get_drafted_dsa(d_id      in number,
                              P_NO      in number,
                              ENT_ID    in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor) as
  begin
  
    open io_cursor for
      select e.name,
             n.heading,
             d.obs_id,
             d.created_on,
             r.pp_no,
             r.lc_amount,
             d.status
        from t_au_dsa d
       inner join t_au_observation o
          on o.id = d.obs_id
       inner join t_au_observation_responibility_assigned r
          on r.obs_id = o.id
       inner join t_auditee_entities_maping m
          on m.entity_id = d.audited_by
       inner join t_auditee_entities e
          on e.entity_id = d.entity_id
       inner join t_audit_checklist_annexure n
          on n.id = o.annex
       where ent_id = case
               when R_ID = 15 then
                d.audited_by
               when R_ID = 7 then
                m.parent_id
               when R_ID = 16 then
                d.audited_by
             end
         and d.status = case
               when R_ID = 15 then
                2
               when R_ID = 7 then
                3
               when R_ID = 16 then
                1
             end;
  end P_get_drafted_dsa;

  Procedure P_get_dsa_guidline(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT * FROM T_AU_DSA_GUIDELINES G WHERE G.STATUS = 'Y';
  end P_get_dsa_guidline;

  procedure P_add_dsa_checkilist(d_id       number,
                                 check_list number,
                                 P_NO       number) as
  begin
    insert into t_au_dsa_guidelines_added
      (id, dsa_id, gd_id, entered_by, entered_on, status)
    values
      ((SELECT COALESCE(max(d.id) + 1, 1) FROM t_au_dsa_guidelines_added d),
       d_id,
       check_list,
       P_NO,
       sysdate,
       'Y');
    commit;
  end P_add_dsa_checkilist;

  procedure P_get_dsa_list(P_NO      in number,
                           R_ID      in number,
                           ENT_ID    in number,
                           io_cursor OUT t_cursor) as
  begin
  
    if (R_ID in (15)) then
      open io_cursor for
      
        SELECT d.id AS ID,
               d.entity_id || '/' || d.eng_id || '/' || d.obs_id || '/' ||
               d.ppno AS D_NO,
               p.description as A_PERIOD,
               me.p_name as REPO_OFFICE,
               ee.name AS ENTITY_NAME,
               otx.HEADINGS AS HEADING,
               d.entity_id AS ENTITY_ID,
               (select er.name
                  from t_auditee_entities er
                 where er.entity_id = d.audited_by) as AZ_NAME,
               d.eng_id AS ENG_ID,
               d.obs_id AS OBS_ID,
               ot.resp_row_id AS ROW_RESP_ID,
               ot.pp_no AS PP_NO,
               em.EMPLOYEEFIRSTNAME || '  ' || em.EMPLOYEELASTNAME AS EMP_NAME,
               ot.LOAN_CASE AS LOANCASE,
               ot.lc_amount AS LCAMOUNT,
               ot.account_number AS ACCNUMBER,
               ot.ac_amount AS ACAMOUNT,
               eg.team_name AS TEAMNAME,
               ds.description as DSA_STATUS,
               'FORWARD TO HEAD FAD' as STATUS_UP,
               '' as STATUS_DOWN
          FROM t_au_dsa d
         inner join t_au_dsa_text dt
            on d.id = dt.dsa_id
         inner join v_get_auditee_pp_responsibility ot
            on ot.resp_row_id = d.resp_id
         inner join v_service_employeeinfo em
            on em.PPNO = ot.pp_no
         inner join t_auditee_entities ee
            on ee.entity_id = d.entity_id
         inner join t_au_observation_text otx
            on otx.OBSERVATSION_ID = d.obs_id
        
         inner join t_au_plan_eng eg
            on eg.eng_id = d.eng_id
         inner join t_au_period p
            on p.auditperiodid = eg.period_id
         inner join t_auditee_entities_maping me
            on me.entity_id = ee.entity_id
         inner join t_au_dsa_status ds
            on ds.status = d.status
         where d.audited_by = ENT_ID
           and d.status in (1, 3);
    
    else
      if (R_ID in (5)) then
        open io_cursor for
        
          SELECT d.id AS ID,
                 d.entity_id || '/' || d.eng_id || '/' || d.obs_id || '/' ||
                 d.ppno AS D_NO,
                 p.description as A_PERIOD,
                 me.p_name as REPO_OFFICE,
                 ee.name AS ENTITY_NAME,
                 otx.HEADINGS AS HEADING,
                 d.entity_id AS ENTITY_ID,
                 (select er.name
                    from t_auditee_entities er
                   where er.entity_id = d.audited_by) as AZ_NAME,
                 d.eng_id AS ENG_ID,
                 d.obs_id AS OBS_ID,
                 ot.resp_row_id AS ROW_RESP_ID,
                 ot.pp_no AS PP_NO,
                 em.EMPLOYEEFIRSTNAME || '  ' || em.EMPLOYEELASTNAME AS EMP_NAME,
                 ot.LOAN_CASE AS LOANCASE,
                 ot.lc_amount AS LCAMOUNT,
                 ot.account_number AS ACCNUMBER,
                 ot.ac_amount AS ACAMOUNT,
                 eg.team_name AS TEAMNAME,
                 ds.description as DSA_STATUS,
                 'FORWARD TO HEAD DPD' as STATUS_UP,
                 'REFFERED-BACK TO SVP AZ' as STATUS_DOWN
            FROM t_au_dsa d
           inner join t_au_dsa_text dt
              on d.id = dt.dsa_id
           inner join v_get_auditee_pp_responsibility ot
              on ot.resp_row_id = d.resp_id
           inner join v_service_employeeinfo em
              on em.PPNO = ot.pp_no
           inner join t_auditee_entities ee
              on ee.entity_id = d.entity_id
           inner join t_au_observation_text otx
              on otx.OBSERVATSION_ID = d.obs_id
          
           inner join t_au_plan_eng eg
              on eg.eng_id = d.eng_id
           inner join t_au_period p
              on p.auditperiodid = eg.period_id
           inner join t_auditee_entities_maping me
              on me.entity_id = ee.entity_id
           inner join t_au_dsa_status ds
              on ds.status = d.status
           where d.status in (2, 5);
      else
        if (R_ID in (12) and ENT_ID in (112259)) then
          open io_cursor for
          
            SELECT d.id AS ID,
                   d.entity_id || '/' || d.eng_id || '/' || d.obs_id || '/' ||
                   d.ppno AS D_NO,
                   p.description as A_PERIOD,
                   me.p_name as REPO_OFFICE,
                   ee.name AS ENTITY_NAME,
                   otx.HEADINGS AS HEADING,
                   d.entity_id AS ENTITY_ID,
                   (select er.name
                      from t_auditee_entities er
                     where er.entity_id = d.audited_by) as AZ_NAME,
                   d.eng_id AS ENG_ID,
                   d.obs_id AS OBS_ID,
                   ot.resp_row_id AS ROW_RESP_ID,
                   ot.pp_no AS PP_NO,
                   em.EMPLOYEEFIRSTNAME || '  ' || em.EMPLOYEELASTNAME AS EMP_NAME,
                   ot.LOAN_CASE AS LOANCASE,
                   ot.lc_amount AS LCAMOUNT,
                   ot.account_number AS ACCNUMBER,
                   ot.ac_amount AS ACAMOUNT,
                   eg.team_name AS TEAMNAME,
                   ds.description as DSA_STATUS,
                   (case
                     when d.status = 6 then
                      ''
                     else
                      'ACKNOWLEDGE'
                   end) as STATUS_UP,
                   (case
                     when d.status = 6 then
                      ''
                     else
                      'REFFERED-BACK TO HEAD FAD'
                   end) as STATUS_DOWN
              FROM t_au_dsa d
             inner join t_au_dsa_text dt
                on d.id = dt.dsa_id
             inner join v_get_auditee_pp_responsibility ot
                on ot.resp_row_id = d.resp_id
             inner join v_service_employeeinfo em
                on em.PPNO = ot.pp_no
             inner join t_auditee_entities ee
                on ee.entity_id = d.entity_id
             inner join t_au_observation_text otx
                on otx.OBSERVATSION_ID = d.obs_id
            
             inner join t_au_plan_eng eg
                on eg.eng_id = d.eng_id
             inner join t_au_period p
                on p.auditperiodid = eg.period_id
             inner join t_auditee_entities_maping me
                on me.entity_id = ee.entity_id
             inner join t_au_dsa_status ds
                on ds.status = d.status
             where d.status in (4, 6);
        end if;
      end if;
    
    end if;
  
  end P_get_dsa_list;
  Procedure P_get_dsa_content(d_ID number, io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT d.entity_id || '/' || d.eng_id || '/' || d.obs_id || '/' ||
             d.ppno AS D_NO,
             d.id,
             dt.dsa_body as text,
             ot.headings as heading
        FROM T_AU_DSA d
       inner join t_au_dsa_text dt
          on d.id = dt.dsa_id
       inner join t_au_observation_text ot
          on d.obs_id = ot.observatsion_id
       where d.id = D_ID;
  end P_get_dsa_content;

  Procedure P_update_dsa_heading(d_ID      number,
                                 U_HEADING in varchar,
                                 P_NO      in number,
                                 ENT_ID    in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) as
  begin
  
    if (R_ID in (15)) then
      update t_au_observation_text ot
         set ot.headings = U_HEADING
       where ot.observatsion_id =
             (select d.obs_id from t_au_dsa d where d.id = d_ID);
    
      OPEN io_cursor FOR
        SELECT 'DSA Heading updated successfully' as remarks from dual;
    
    else
      OPEN io_cursor FOR
        SELECT 'Only SVP AZ can update the DSA Heading' as remarks
          from dual;
    end if;
  
  end P_update_dsa_heading;

  Procedure P_submit_dsa_to_head_fad(d_ID number, io_cursor OUT t_cursor) as
  begin
    update t_au_dsa d set d.status = 2 where d.id = D_ID;
    commit;
  
    OPEN io_cursor FOR
      SELECT 'DSA Submitted to Head FAD' as remarks from dual;
  end P_submit_dsa_to_head_fad;
  Procedure P_reffered_back_dsa_by_head_fad(d_ID      number,
                                            io_cursor OUT t_cursor) as
  begin
    update t_au_dsa d set d.status = 3 where d.id = D_ID;
    commit;
  
    OPEN io_cursor FOR
      SELECT 'DSA Reffered Back to Audit Zone ' as remarks from dual;
  end P_reffered_back_dsa_by_head_fad;

  Procedure P_submit_dsa_by_head_fad_to_dpd(d_ID      number,
                                            io_cursor OUT t_cursor) as
  begin
    update t_au_dsa d set d.status = 4 where d.id = D_ID;
    commit;
  
    OPEN io_cursor FOR
      SELECT 'DSA Submitted to DPD  ' as remarks from dual;
  end P_submit_dsa_by_head_fad_to_dpd;

  Procedure P_reffered_back_dsa_by_dpd(d_ID number, io_cursor OUT t_cursor) as
  begin
    update t_au_dsa d set d.status = 5 where d.id = D_ID;
    commit;
  
    OPEN io_cursor FOR
      SELECT 'DSA Reffered Back to Head FAD ' as remarks from dual;
  end P_reffered_back_dsa_by_dpd;

  Procedure P_submit_dsa_by_dpd_to_committee(d_ID      number,
                                             io_cursor OUT t_cursor) as
  begin
    update t_au_dsa d set d.status = 6 where d.id = D_ID;
    commit;
  
    OPEN io_cursor FOR
      SELECT 'DSA Acknowledged ' as remarks from dual;
  end P_submit_dsa_by_dpd_to_committee;

  procedure p_get_email_address_for_dsa(ENT_ID    number,
                                        R_ID      number,
                                        P_NO      number,
                                        io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      SELECT e.email_address as to_email, ee.email_address as cc
        from t_auditee_entities e
       inner join t_auditee_entities_maping m
          on m.entity_id = e.entity_id
       inner join t_auditee_entities ee
          on m.parent_id = ee.entity_id
       where e.entity_id = ENT_ID;
  
  end p_get_email_address_for_dsa;

end PKG_AR;

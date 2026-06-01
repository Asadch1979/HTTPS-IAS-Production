create or replace package pkg_AIS is

  TYPE t_cursor IS REF CURSOR;

  procedure UPDATE_USERS(PPNUMBER  IN T_USER.PPNO%TYPE,
                         PASS      IN T_USER.PASSWORD%TYPE,
                         ISACTIVE  IN T_USER.ISACTIVE%TYPE,
                         ROLEID    IN T_USER_MAPING.ROLE_ID%TYPE,
                         io_cursor OUT t_cursor);

  PROCEDURE P_AddAuditEntity(AUDITABLE      in t_auditee_ent_types.auditable%type,
                             ENTITYTYPEDESC in t_auditee_ent_types.entitytypedesc%type);

  procedure P_AUDITEE_OBSERVATION_RESPONSE(AUOBSID   IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.AU_OBS_ID%type,
                                           REPLYDATA IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REPLY%type,
                                           REPLIEDBY IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REPLIEDBY%type,
                                           OBSTEXTID IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.OBS_TEXT_ID%type,
                                           REPLYROLE IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REPLY_ROLE%type,
                                           REMARKS   IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REMARKS%type,
                                           SUBMITTED IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.SUBMITTED%type,
                                           io_cursor OUT t_cursor);

  procedure P_AUDITEE_OBSERVATION_RESPONSE_evidences(respid    in t_au_observations_auditee_evidences.respid%type,
                                                     AUOBSID   IN t_au_observations_auditee_evidences.memoid%type,
                                                     filename  IN t_au_observations_auditee_evidences.file_name%type,
                                                     filetype  IN t_au_observations_auditee_evidences.file_type%type,
                                                     length    in t_au_observations_auditee_evidences.length%type,
                                                     enteredby IN t_au_observations_auditee_evidences.enteredby%type,
                                                     filedata  IN t_au_observations_auditee_evidences.file_data%type,
                                                     sequence  IN t_au_observations_auditee_evidences.sequence%type,
                                                     text_id   in t_au_observations_auditee_evidences.text_id%type);
  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences(resp_id   in t_au_observations_auditee_evidences.respid%type,
                                                         io_cursor OUT t_cursor);
  procedure AUDITOR_RESPONSE(OBS_ID          IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.AU_OBS_ID%type,
                             PPNumber        IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.RECO_BY%type,
                             AUDITOR_COMMENT IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.RECOMMENDATION%type,
                             status          IN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION.STATUS%type);

  procedure AUDITOR_REPLY(OBS_ID          IN T_AU_OBSERVATIONS_AUDITOR_REPLY.AU_OBS_ID%type,
                          PPNumber        IN T_AU_OBSERVATIONS_AUDITOR_REPLY.REPLIEDBY%TYPE,
                          AUDITOR_COMMENT IN T_AU_OBSERVATIONS_AUDITOR_REPLY.AUDIT_REPLY%type,
                          status          IN T_AU_OBSERVATIONS_AUDITOR_REPLY.OBS_STATUS%type);
  /* procedure Audit_checklist(p_name  in t_audit_checklist.heading%type,
  RISK_ID in number);*/

  /*    procedure audit_checklist_detail(p_id           in t_audit_checklist_details.s_id%type,
  DESCRIPTION    in t_audit_checklist_details.heading%type,
  V_ID           in t_audit_checklist_details.v_id%type,
  CONTROL_OWNER  in t_audit_checklist_details.owner_enitity_id%type,
  RISK_WEIGHTAGE in t_audit_checklist_details.risk_id%type,
  ACTION         in t_audit_checklist_details.process_owner_id%type);*/

  procedure audit_checklist_details_log(ppnumber in t_audit_checklist_details_log.user_id%type,
                                        comments in t_audit_checklist_details_log.comments%type,
                                        t_id     in t_audit_checklist_details_log.t_id%type);
  /*procedure p_Recommend_Process_Transaction_By_Reviewer(T_ID in t_audit_checklist_details_log.t_id%type);*/

  procedure audit_checklist_details_status_mapping;
  /*procedure audit_checklist_sub(p_ID        in t_audit_checklist_sub.t_id%type,
  TITLE       in t_audit_checklist_sub.heading%type,
  ENTITY_TYPE in t_audit_checklist_sub.entity_type%type);*/

  procedure P_GetTaskList(PPNumber in number, io_cursor OUT t_cursor);
  procedure P_ADDAUDITCRITERIA(ENTITYTYPEID   in T_AUDIT_CRITERIA.ENTITY_TYPEID%type,
                               SIZEID         in T_AUDIT_CRITERIA.SIZE_ID%type,
                               RISKID         in T_AUDIT_CRITERIA.RISK_ID%type,
                               FREQUENCYID    in T_AUDIT_CRITERIA.FREQUENCY_ID%type,
                               NOOFDAYS       in T_AUDIT_CRITERIA.NO_OF_DAYS%type,
                               visit          in T_AUDIT_CRITERIA.VISIT%type,
                               APPROVALSTATUS in T_AUDIT_CRITERIA.APPROVAL_STATUS%type,
                               AUDITPERIODID  in T_AUDIT_CRITERIA.AUDITPERIODID%type,
                               UserEntityID   in T_AUDIT_CRITERIA.CREATED_BY%type,
                               REMARKS        in T_AUDIT_CRITERIA_LOG.REMARKS%type,
                               CREATEDBY      in T_AUDIT_CRITERIA_LOG.CREATEDBY_ID%type,
                               entityid       in number,
                               io_cursor      OUT t_cursor);

  procedure AUDIT_CRITERIA_LOG(CREATEDBY_ID in T_AUDIT_CRITERIA_LOG.CREATEDBY_ID%type,
                               STATUS_ID    in T_AUDIT_CRITERIA_LOG.STATUS_ID%type,
                               REMARKS      in T_AUDIT_CRITERIA_LOG.REMARKS%type);

  procedure P_SetAuditCriteriaStatusApprove(CAID     IN t_audit_criteria.id%type,
                                            REMARKS  IN VARCHAR2,
                                            PPNumber IN NUMBER);

  procedure P_SetAuditCriteriaStatusReferredBack(CID      IN t_audit_criteria.id%type,
                                                 REMARKS  IN VARCHAR2,
                                                 PPNumber IN NUMBER);

  procedure P_GetAuditeeEntities(ENTITYID  IN NUMBER,
                                 TYPEID    IN NUMBER,
                                 io_cursor OUT t_cursor);

  procedure AUDIT_JOINING(ENG_PLAN_ID     in T_AU_AUDIT_JOINING.ENG_PLAN_ID%type,
                          TEAM_MEM_PPNO   in T_AU_AUDIT_JOINING.TEAM_MEM_PPNO%type,
                          ENTEREDBY       in T_AU_AUDIT_JOINING.ENTEREDBY%type,
                          JOINING_DATE    in T_AU_AUDIT_JOINING.JOINING_DATE%type,
                          COMPLETION_DATE in T_AU_AUDIT_JOINING.COMPLETION_DATE%type);

  procedure AUDIT_OBSERVATION_ASSIGNEDTO(OBS_ID       IN T_AU_OBSERVATION_ASSIGNEDTO.OBS_ID%type,
                                         ReplyByQuery IN T_AU_OBSERVATION_ASSIGNEDTO.ENTITY_ID%type,
                                         ENTEREDBY    IN T_AU_OBSERVATION_ASSIGNEDTO.ASSIGNEDBY%type);

  /*procedure Audit_period(DESCRIPTION in T_AU_PERIOD.DESCRIPTION%type,
                           START_DATE  in T_AU_PERIOD.START_DATE%type,
                           END_DATE    in T_AU_PERIOD.End_Date%type,
                           STATUS_ID   in T_AU_PERIOD.STATUS_ID%type);
  */
  procedure AUDIT_TEAMS(TEAM_ID        in T_AU_AUDIT_TEAMS.TEAM_ID%type,
                        TEAM_NAME      in T_AU_AUDIT_TEAMS.T_NAME%type,
                        placeofposting in T_AU_AUDIT_TEAMS.PLACE_OF_POSTING%type);

--  PROCEDURE Branch_risk_rating_model;

  procedure CAU_OM(OM_NO       IN T_CAU_OM.OM_NO%type,
                   ENCODED_MSG IN T_CAU_OM.CONTENTS_OF_OM%type,
                   DIV_ID      IN T_CAU_OM.DIV_ID%type);
  procedure P_Closeaudit(engid     in number,
                         PP_NO     in number,
                         io_cursor OUT t_cursor);
 /* procedure Closing(engid       in t_Au_Plan_Eng.Eng_Id%type,
                    entity_type in t_Au_Plan_Eng.Entity_Type%type,
                    io_cursor   OUT t_cursor);*/

  procedure Coso_Risk_register(e_id in T_COSO_RATING_DEPARTMENT_INHERITED_RISK.ENTITY_ID%type);

  procedure Criteria(CID IN t_audit_criteria.id%type);

  procedure P_oldparasresponibilityassigned(ID IN t_au_observation_old_paras_responibility_assigned.ref_p%type,
                                            pp IN t_au_observation_old_paras_responibility_assigned.pp_no%type);

  procedure Pre_Coso_Risk_register(entityid in number);

  procedure Session_END(PPNumber  in t_user_session.user_pp_number%type,
                        SessionId in t_user_session.session_id%type);

  procedure Session_Kill(PPNumber in t_user_session.user_pp_number%type);

  procedure Session_Kill_day_end(io_cursor OUT t_cursor);

  procedure TEAM_TASKLIST(TEAM_ID         in T_AU_AUDIT_TEAM_TASKLIST.TEAM_ID%type,
                          sequence_no     in T_AU_AUDIT_TEAM_TASKLIST.SEQUENCE_NO%type,
                          member_pp       in T_AU_AUDIT_TEAM_TASKLIST.TEAMMEMBER_PPNO%type,
                          ENTITY_ID       in T_AU_AUDIT_TEAM_TASKLIST.ENTITY_ID%type,
                          AUDIT_STARTDATE in T_AU_AUDIT_TEAM_TASKLIST.AUDIT_START_DATE%type,
                          AUDIT_endDATE   in T_AU_AUDIT_TEAM_TASKLIST.AUDIT_END_DATE%type);

  procedure Tentative_Audit_Plan(CRITERIA_ID in t_audit_criteria.id%type,
                                 io_cursor   OUT t_cursor);

  procedure P_ADDCADAuditPlan(auditperiod_id in T_AU_PLAN.AUDITPERIODID%type,
                              auditby_id     in number,
                              entityid       in number,
                              noofdays       in number,
                              typeid         in number);


  procedure user_maping(USER_ID in t_user_maping.userid%type,
                        ROLE_ID in t_user_maping.role_id%type,
                        PPNO    in t_user_maping.ppno%type);

  procedure User_SESSION(PPNumber             in T_USER_SESSION.USER_PP_NUMBER%type,
                         UserRoleID           IN T_USER_SESSION.ROLE_ID%type,
                         LocalIpAddress       IN T_USER_SESSION.IP_ADDRESS%type,
                         SessionId            IN T_USER_SESSION.SESSION_ID%type,
                         UserLocationType     IN T_USER_SESSION.LOGIN_LOCATION_TYPE%type,
                         MACAddress           IN T_USER_SESSION.MAC_ADDRESS%type,
                         FirstMACCardAddress  IN T_USER_SESSION.PRIMARY_MAC_CARD_ADDRESS%type,
                         UserPostingDiv       IN T_USER_SESSION.POSTING_DIV%type,
                         UserGroupID          IN T_USER_SESSION.GROUP_ID%type,
                         UserPostingDept      IN T_USER_SESSION.POSTING_DEPT%type,
                         UserPostingZone      IN T_USER_SESSION.POSTING_ZONE%type,
                         UserPostingBranch    IN T_USER_SESSION.POSTING_BRANCH%type,
                         UserPostingAuditZone IN T_USER_SESSION.POSTING_AZ%type);

  procedure p_get_audit_plan(AUDITED_BY in number, io_cursor OUT t_cursor);

  procedure p_get_user(PPNumber  in t_user.ppno%type,
                       enc_pass  in t_user.password%type,
                       io_cursor OUT t_cursor);

  procedure p_get_user_id(PPNumber  in t_user.ppno%type,
                          io_cursor OUT t_cursor);

  procedure p_get_allusers(ENTITYID  in t_user.ppno%type,
                           EMAIL     in v_service_employeeinfo.EMAIL%type,
                           GROUPID   in t_groups.group_id%type,
                           PPNUMBER  in t_user.ppno%type,
                           LOGINNAME in t_user.login_name%type,
                           io_cursor OUT t_cursor);

  procedure p_get_user_session(PPNumber  in t_user.ppno%type,
                               io_cursor OUT t_cursor);

  procedure p_GetTopMenus(UserRoleID in t_user_group_map.role_id%type,
                          io_cursor  OUT t_cursor);

  procedure p_GetTopMenuPages(UserGroupID in t_menu_pages_groupmap.group_id%type,
                              io_cursor   OUT t_cursor);

  procedure P_GetAllTopMenus(io_cursor OUT t_cursor);

  procedure P_GetAssignedMenuPages(groupId   in t_menu_pages_groupmap.group_id%type,
                                   menuId    in T_MENU_PAGES.MENU_ID%type,
                                   io_cursor OUT t_cursor);

  procedure P_GetAllMenuPage(io_cursor OUT t_cursor);

  procedure P_updateAllMenuPages(menuId in T_MENU_PAGES.MENU_ID%type,
                                 p_id   in T_MENU_PAGES.MENU_ID%type);

  procedure P_GetGroups(io_cursor OUT t_cursor);

  procedure P_Group_Update(GROUP_ID          in t_groups.group_id%type,
                           GROUP_DESCRIPTION in t_groups.description%type,
                           GROUP_NAME        in t_groups.group_name%type,
                           ISACTIVE          in t_groups.status%type);

  procedure p_AddGroup(GROUP_DESCRIPTION in t_groups.description%type,
                       GROUP_NAME        in t_groups.group_name%type,
                       ISACTIVE          in t_groups.status%type);

  procedure P_GetRoleResponsibilities(io_cursor OUT t_cursor);

  procedure P_GetAuditEntities(ENTITYID IN NUMBER, io_cursor OUT t_cursor);

  procedure P_GetAuditeeEntitiesForOldParas(ENTITY_ID    in number,
                                            UserEntityID in t_au_old_paras_fad.entity_id%type,
                                            io_cursor    OUT t_cursor);

  procedure P_GetAuditeeOldParasFAD(EntityID  in number,
                                    io_cursor OUT t_cursor);
  procedure P_GetAuditeeOldParasFADtext(refp      in VARCHAR2,
                                        io_cursor OUT t_cursor);

  procedure P_updateAuditeeOldParasFADtext(refp       in VARCHAR2,
                                           textchange in number,
                                           ptext      in clob,
                                           newstatus  in number);

  procedure P_GetAuditSubEntities(io_cursor OUT t_cursor);

  procedure P_UpdateUser(USER_ID  in t_user.userid%type,
                         enc_pass in t_user.password%type,
                         role_id  in t_user_maping.role_id%type,
                         PPNO     in t_user.ppno%type,
                         ISACTIVE in t_user.isactive%type);
  procedure P_ChangePassword(PP_NO    in t_user.ppno%type,
                             enc_pass in t_user.password%type);

  procedure P_RemoveGroupMenuAssignment(roleid in T_USER_GROUP_MAP.role_id%type,
                                        menuid in T_USER_GROUP_MAP.MENU_ID%type);

  procedure P_AddGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                          PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type);

  procedure P_RemoveGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                             PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type);

  procedure P_GetAuditZones(ENTITYID  in t_auditee_entities.entity_id%type,
                            io_cursor OUT t_cursor);

  procedure P_GetInspectionUnits(io_cursor OUT t_cursor);

  procedure P_GetBranches(Zone_Id in number, io_cursor OUT t_cursor);

  procedure P_GetZones(io_cursor OUT t_cursor);

  procedure P_GetBranchSizes(io_cursor OUT t_cursor);

  procedure P_GetControlViolations(io_cursor OUT t_cursor);

  procedure P_GetEntitees(ENTITYID  IN NUMBER,
                          TYPEID    IN NUMBER,
                          io_cursor OUT t_cursor);

  procedure P_GetSubEntities(dept_code in number,
                             Div_id    in number,
                             io_cursor OUT t_cursor);

  procedure P_AddSubEntity(NAME   IN T_AUDITEE_ENTITEE_SUBENTITY.NAME%TYPE,
                           DIV_ID IN T_AUDITEE_ENTITEE_SUBENTITY.DEPT_ID%TYPE,
                           DEP_ID IN T_AUDITEE_ENTITEE_SUBENTITY.Parent_Enititid%type,
                           STATUS IN T_AUDITEE_ENTITEE_SUBENTITY.STATUS%type);

  procedure P_GetDepartments(E_id in number, io_cursor OUT t_cursor);

  procedure P_UpdateSubEntity(E_id   in number,
                              NAME   IN VARCHAR2,
                              DIV_ID IN NUMBER,
                              DEP_ID IN NUMBER,
                              STATUS IN T_AUDITEE_ENTITEE_SUBENTITY.STATUS%TYPE);

  procedure P_GetRiskGroup(io_cursor OUT t_cursor);

  procedure P_GetRiskSubGroup(group_id IN NUMBER, io_cursor OUT t_cursor);

  procedure p_GetRiskActivities(Sub_group_id IN NUMBER,
                                io_cursor    OUT t_cursor);

  procedure P_GetAuditEmployees(dept_code IN NUMBER,
                                io_cursor OUT t_cursor);

  procedure P_GetAuditOperationalStartDate(entityCode    IN NUMBER,
                                           auditPeriodId IN NUMBER,
                                           io_cursor     OUT t_cursor);

  procedure P_GetAuditTeams(dept_code    IN NUMBER,
                            UserEntityID IN NUMBER,
                            io_cursor    OUT t_cursor);

  procedure P_AddAuditTeam(TEAMNAME      in T_AU_TEAM_MEMBERS.TEAM_NAME%type,
                           TEAMMEMBER_ID in T_AU_TEAM_MEMBERS.MEMBER_PPNO%type,
                           MAX_T_ID      IN T_AU_TEAM_MEMBERS.T_ID%TYPE,
                           EMPLOYEENAME  in T_AU_TEAM_MEMBERS.MEMBER_NAME%type,
                           IS_TEAMLEAD   in T_AU_TEAM_MEMBERS.ISTEAMLEAD%type,
                           STATUS        in T_AU_TEAM_MEMBERS.STATUS%type,
                           entityid      in number);

  PROCEDURE P_DeleteAuditTeam(TID IN NUMBER);

  procedure P_GetAuditPeriods(io_cursor OUT t_cursor);

  procedure P_AddAuditPeriod(DESCRIPTION in T_AU_PERIOD.DESCRIPTION%type,
                             START_DATE  in T_AU_PERIOD.START_DATE%type,
                             END_DATE    in T_AU_PERIOD.End_Date%type,
                             io_cursor   OUT t_cursor);

  PROCEDURE P_MAXTEAMID(io_cursor OUT t_cursor);

  procedure P_GetAuditEngagementPlans(EntityID  IN NUMBER,
                                      io_cursor OUT t_cursor);

  procedure P_GetRefferedBackAuditEngagementPlans(EntityID  IN NUMBER,
                                                  io_cursor OUT t_cursor);

  procedure P_AddAuditEngagementPlan(PERIODID        in T_AU_PLAN_ENG.PERIOD_ID%type,
                                     ENTITYID        in T_AU_PLAN_ENG.ENTITY_ID%type,
                                     AUDIT_STARTDATE in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                     CREATEDBY       in T_AU_PLAN_ENG.CREATEDBY%type,
                                     AUDIT_ENDDATE   in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                     STATUS          in T_AU_PLAN_ENG.STATUS%type,
                                     TEAMID          in number,
                                     TEAM_NAME       in T_AU_PLAN_ENG.TEAM_NAME%type,
                                     PLANID          IN NUMBER,
                                     OP_STARTDATE    in date,
                                     OP_ENDDATE      in date,
                                     TRAVELDAY       in number,
                                     RRDAY           in number,
                                     D_Day           in number,
                                     io_cursor       OUT t_cursor);

  procedure p_GetCOSORisks(io_cursor OUT t_cursor);

  procedure P_RerecommendAuditEngagementPlan(ENGID        in T_AU_PLAN_ENG.PERIOD_ID%type,
                                             ENTITYID     in T_AU_PLAN_ENG.ENTITY_ID%type,
                                             STARTDATE    in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                             ENDDATE      in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                             TEAMID       in T_AU_PLAN_ENG.TEAM_ID%type,
                                             UPDATEDBY    in T_AU_PLAN_ENG.CREATEDBY%type,
                                             PLANID       IN NUMBER,
                                             OP_STARTDATE in date,
                                             OP_ENDDATE   in date,
                                             REMARKS      in varchar2,
                                             io_cursor    OUT t_cursor);
  procedure P_GetLatestCommentsOnEngagement(ENGID     IN NUMBER,
                                            io_cursor OUT t_cursor);

  procedure P_AddAuditteamtasklist(TEAMID   in number,
                                   PLANID   IN NUMBER,
                                   ENTITYID IN NUMBER);

  procedure P_AddGroupMenuAssignment(roleid  in T_USER_GROUP_MAP.ROLE_ID%type,
                                     menuid  in T_USER_GROUP_MAP.MENU_ID%type,
                                     pageids in T_USER_GROUP_MAP.PAGE_IDS%type);

  procedure plan_eng_log(createdbyId in t_au_plan_eng_log.createdby_id%type,
                         STATUS      in t_au_plan_eng_log.status_id%type);

  procedure P_RefferedBackAuditEngagementPlan(ENGID    IN NUMBER,
                                              REMARKS  IN VARCHAR2,
                                              PPNumber IN NUMBER);

  procedure P_ApproveAuditEngagementPlan(ENGID IN NUMBER,
                                         
                                         PPNumber IN NUMBER);

  procedure P_GetRiskProcessDefinition(io_cursor OUT t_cursor);

  procedure P_GetRiskProcessDetails(procId    IN NUMBER,
                                    io_cursor OUT t_cursor);

  procedure P_GetRiskProcessTransactions(procDetailId  IN NUMBER,
                                         transactionId IN NUMBER,
                                         io_cursor     OUT t_cursor);
  procedure p_GetRiskProcessTransactionsWithStatus(statusId  IN NUMBER,
                                                   io_cursor OUT t_cursor);

  procedure P_audit_checklist(p_name  in t_audit_checklist.heading%type,
                              RISK_ID in number);
  procedure P_GetAuditChecklistCAD(io_cursor OUT t_cursor);

  procedure P_audit_checklist_sub(p_ID        in t_audit_checklist_sub.t_id%type,
                                  TITLE       in t_audit_checklist_sub.heading%type,
                                  ENTITY_TYPE in t_audit_checklist_sub.entity_type%type);

  procedure audit_checklist_detail(p_id           in t_audit_checklist_details.s_id%type,
                                   DESCRIPTION    in t_audit_checklist_details.heading%type,
                                   V_ID           in t_audit_checklist_details.v_id%type,
                                   CONTROL_OWNER  in t_audit_checklist_details.owner_enitity_id%type,
                                   RISK_WEIGHTAGE in t_audit_checklist_details.risk_id%type,
                                   ACTION         in t_audit_checklist_details.process_owner_id%type,
                                   PPNumber       IN NUMBER);

  procedure p_Recommend_Process_Transaction_By_Reviewer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                        COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                        PPNumber IN NUMBER);
  procedure p_RefferedBack_Process_Transaction_By_Reviewer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                           COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                           PPNumber IN NUMBER);
  procedure p_RefferedBack_Process_Transaction_By_Authorizer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                             COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                             PPNumber IN NUMBER);

  procedure p_Recommend_Process_Transaction_By_Authorizer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                          COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                          PPNumber IN NUMBER);

  procedure p_GetAuditFrequencies(io_cursor OUT t_cursor);

  procedure P_GetRisks(io_cursor OUT t_cursor);

  procedure P_GetPendingAuditCriterias(UserEntityID IN NUMBER,
                                       io_cursor    OUT t_cursor);

  procedure P_GetRefferedBackAuditCriterias(UserEntityID IN NUMBER,
                                            io_cursor    OUT t_cursor);

  procedure P_GetAuditCriteriasToAuthorize(io_cursor OUT t_cursor);

  procedure P_GetPostChangesAuditCriterias(userentityid in number,
                                           io_cursor    OUT t_cursor);

  procedure P_UpdateAuditCriteria(CID           IN t_audit_criteria.id%type,
                                  ENTITY_TYPEID IN T_AUDIT_CRITERIA.ENTITY_TYPEID%TYPE,
                                  SIZE_ID       IN T_AUDIT_CRITERIA.SIZE_ID%TYPE,
                                  RISK_ID       IN T_AUDIT_CRITERIA.RISK_ID%TYPE,
                                  FREQUENCY_ID  IN T_AUDIT_CRITERIA.FREQUENCY_ID%TYPE,
                                  NO_OF_DAYS    IN T_AUDIT_CRITERIA.NO_OF_DAYS%TYPE,
                                  VISIT         IN T_AUDIT_CRITERIA.VISIT%TYPE,
                                  
                                  AUDITPERIODID IN T_AUDIT_CRITERIA.AUDITPERIODID%TYPE,
                                  REMARKS       IN VARCHAR2,
                                  CREATED_BY    IN NUMBER);

  PROCEDURE P_getauditeecheckklist(PLANID         IN NUMBER,
                                   SUBCHECKLISTID IN NUMBER,
                                   io_cursor      OUT t_cursor);

  procedure p_GetAssignedObservations(ENTID     in number,
                                      ENGID     in number,
                                      io_cursor OUT t_cursor);

  procedure P_GetAssignedObservationsForBranch(entityid  in number,
                                               io_cursor OUT t_cursor);
  procedure p_GetAssignedObservationstext(OBSID     in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetObservationText(OBS_ID in number, io_cursor OUT t_cursor);

  procedure P_UpdateObservation(OBS_ID       in number,
                                obtext       in clob,
                                subprocessid in number,
                                checklistid  in number,
                                ppno         in number,
                                io_cursor    OUT t_cursor);

  procedure P_GetObservationResponsible(OBSID     in number,
                                        io_cursor OUT t_cursor);

  procedure P_GetOBSERVATIONSAUDITEERESPONSE(OBS_ID    in number,
                                             io_cursor OUT t_cursor);

  procedure P_GetLoggedInUserEngId(PPNumber  IN NUMBER,
                                   io_cursor OUT t_cursor);

  procedure P_SetEngIdOnHold(ENGID IN NUMBER, ppno in number);

  procedure P_GetLatestAuditorResponse(obs_id    IN NUMBER,
                                       io_cursor OUT t_cursor);

  procedure P_GetLatestDepartmentalHeadResponse(obs_id    IN NUMBER,
                                                io_cursor OUT t_cursor);

  procedure P_GetRiskDescByID(risk_id IN NUMBER, io_cursor OUT t_cursor);

  procedure P_GetLatestCommentsOnProcess(procId    IN NUMBER,
                                         io_cursor OUT t_cursor);

  procedure P_GetLatestAuditeeResponse(obs_id    IN NUMBER,
                                       io_cursor OUT t_cursor);

  procedure P_GetManagedObservations(ENGID     IN NUMBER,
                                     OBSID     IN NUMBER,
                                     io_cursor OUT t_cursor);

  procedure P_GetManagedObservationstext(OBSID     IN NUMBER,
                                         io_cursor OUT t_cursor);

  procedure P_GetManagedObservationsForBranches(ENGID     IN NUMBER,
                                                OBSID     IN NUMBER,
                                                io_cursor OUT t_cursor);

  procedure P_GetManagedObservationsForBranchesTEXT(OBSID     IN NUMBER,
                                                    io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservations(ENGID     IN NUMBER,
                                          io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservationsbranch(ENGID     IN NUMBER,
                                                io_cursor OUT t_cursor);

  Procedure P_GetFinalizedDraftObservations(ENGID     IN NUMBER,
                                            io_cursor OUT t_cursor);

  procedure P_GetFinalizedDraftObservationsbranch(ENGID     IN NUMBER,
                                                  io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservationsForBranches(ENGID     IN NUMBER,
                                                     io_cursor OUT t_cursor);

  procedure P_GetManagedDraftObservationsText(OBSID     IN NUMBER,
                                              io_cursor OUT t_cursor);

  procedure P_GetViolationObservations(io_cursor OUT t_cursor);

  procedure P_DropAuditObservation(OBS_ID    IN NUMBER,
                                   pp_no     in number,
                                   io_cursor OUT t_cursor);

  procedure P_UpdateAuditObservationStatus(OBS_ID        IN NUMBER,
                                           NEW_STATUS_ID IN NUMBER,
                                           Remarks       IN VARCHAR2,
                                           PP_NO         IN NUMBER,
                                           io_cursor     OUT t_cursor);

  procedure p_GetClosingDraftObservations(PP_NO     in number,
                                          io_cursor OUT t_cursor);

  procedure p_Getauditteams(ENGID in number, io_cursor OUT t_cursor);

  procedure P_GetClosingDraftTeamDetails(ENGID     in number,
                                         io_cursor OUT t_cursor);

  procedure P_CloseDraftAuditReport(ENGID     IN NUMBER,
                                    io_cursor OUT t_cursor);

  procedure P_DeletePendingCriteria(CID IN NUMBER);

  procedure P_SubmitAuditCriteriaForApproval(CID IN NUMBER);

  procedure P_GetCOSORiskForDepartment(PERIOD_ID    in number,
                                       UserEntityID in number,
                                       io_cursor    OUT t_cursor);

  procedure P_CAUGetAssignedOMs(UserEntityID in number,
                                io_cursor    OUT t_cursor);

  procedure P_GetCCQ(UserEntityID in number, io_cursor OUT t_cursor);

  procedure P_UpdateCCQ(CID                  IN NUMBER,
                        QUESTIONS            in varchar2,
                        CONTROL_VIOLATION_ID in number,
                        RISK_ID              in number,
                        STATUS               in varchar2,
                        PPNumber             in number);

  procedure P_UpdateENTITIEES(ID          IN NUMBER,
                              CODE        IN NUMBER,
                              NAME        IN VARCHAR2,
                              DISCRIPTION IN VARCHAR2,
                              AUDITEDBY   IN NUMBER,
                              INSPECTEDBY IN NUMBER,
                              TYPEID      IN NUMBER,
                              ENTITYID    IN NUMBER,
                              STATUS      IN CHAR,
                              AUDITABLE   IN VARCHAR2,
                              parentid    in number,
                              childid     in number);

  procedure P_InsertENTITIEES(ID          IN NUMBER,
                              CODE        IN NUMBER,
                              NAME        IN VARCHAR2,
                              DISCRIPTION IN VARCHAR2,
                              AUDITEDBY   IN NUMBER,
                              INSPECTEDBY IN NUMBER,
                              TYPEID      IN NUMBER,
                              
                              STATUS    IN CHAR,
                              AUDITABLE IN VARCHAR2);
                              
  Procedure p_getglheadsummary_Yearly(PPNumber  in number,
                                      io_cursor OUT t_cursor);                              

  procedure P_GetAuditVoilationcats(io_cursor OUT t_cursor);

  procedure P_GetVoilationSubGroup(group_id  in number,
                                   io_cursor OUT t_cursor);

  procedure P_GetJoiningDetails(engId    in number,
                                PPNumber in number,
                                
                                io_cursor OUT t_cursor);

  procedure P_AddJoiningReport(ENGID           in number,
                               PPNO            in number,
                               COMPLETION_DATE in date);

  procedure P_GetAuditChecklist(io_cursor OUT t_cursor);

  procedure p_GetAuditChecklistSub(tid in number, io_cursor OUT t_cursor);

  procedure P_GetAuditChecklistDetails(sid in number,
                                       
                                       io_cursor OUT t_cursor);

  procedure P_SaveAuditObservationCAD(PLANID            in T_AU_OBSERVATION.ENGPLANID%type,
                                      STATUS            in T_AU_OBSERVATION.STATUS%type,
                                      REPLYDATE         in T_AU_OBSERVATION.REPLYDATE%type,
                                      ENTEREDBY         in T_AU_OBSERVATION.Enteredby%type,
                                      Severity          in T_AU_OBSERVATION.Severity%type,
                                      SUBCHECKLISTID    in T_AU_OBSERVATION.Subchecklist_Id%type,
                                      CHECKLISTDETAILID in T_AU_OBSERVATION.Checklistdetail_Id%type,
                                      TEXT_DATA         in T_AU_OBSERVATION_TEXT.TEXT%type,
                                      BRANCHID          IN NUMBER,
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
                                   io_cursor         OUT t_cursor);

  procedure P_SubmitAuditObservationToAuditee(OBS_ID    IN NUMBER,
                                              pp_no     in number,
                                              io_cursor OUT t_cursor);

  PROCEDURE P_UPDATEtauditeecheckklist(PLANID            IN NUMBER,
                                       CHECKLISTDETAILID IN NUMBER);


  Procedure p_getglheadsummary(PPNumber in number, io_cursor OUT t_cursor);
  procedure P_GetGlheadDetails(PPNumber  in number,
                               subcode   in number,
                               io_cursor OUT t_cursor);

  procedure P_GetStaffPosition(PPNumber in number, io_cursor OUT t_cursor);

  procedure P_preauditinfo_loan_scheme_yearly(PPNumber  in number,
                                              io_cursor OUT t_cursor);
  procedure P_preauditinfo_loan_scheme(PPNumber  in number,
                                       io_cursor OUT t_cursor);

  procedure P_GetFunctionalResponsibilityWisePara(ENTITYID         in number,
                                                  PROCESSID        IN NUMBER,
                                                  SUB_PROCESSID    IN NUMBER,
                                                  PROCESS_DETAILID IN NUMBER,
                                                  io_cursor        OUT t_cursor);
  procedure P_GetGlheadSum(PPNumber  in number,
                           GLTYPEID  IN NUMBER,
                           io_cursor OUT t_cursor);

 

  procedure P_GetDepositAccountSubdetails(PPNumber  in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetDepositACCOUNTCATEGORY(PPNumber  in number,
                                        io_cursor OUT t_cursor);

  procedure P_GetIncomeExpenceDetails(PPNumber  in number,
                                      io_cursor OUT t_cursor);

  procedure P_GetLoanCaseDetails(PPNumber  in number,
                                 Loantype  in varchar2,
                                 io_cursor OUT t_cursor);

  procedure P_GetOldParas(Entityid  in number,
                          AUDITYEAR in number,
                          io_cursor OUT t_cursor);

  procedure p_GetObservationEntities(PP_NO     in number,
                                     io_cursor OUT t_cursor);

  procedure P_Getrealtionshiptype(io_cursor OUT t_cursor);

  procedure P_Getparentrepoffice(rid in number, io_cursor OUT t_cursor);

  procedure P_Getchildposting(erid in number, io_cursor OUT t_cursor);

  procedure P_GetCCQsEntities(PPNO in number, io_cursor OUT t_cursor);

  procedure P_GetAuditeeAssignedEntities(ENTITID   in number,
                                         io_cursor OUT t_cursor);

  Procedure P_AddDivisionalHeadRemarksOnFunctionalLegacyPara(CONCERNED_DEPTID in number,
                                                             COMMENTS         in varchar2,
                                                             REF_PARAID       in number,
                                                             PPNumber         in number);

  procedure P_GetActiveInactiveChartData(io_cursor OUT t_cursor);

  procedure P_AddOldParas(PROCESS       in number,
                          SUBPROCESS    in number,
                          PROCESSDETAIL in number,
                          PPNO          in number,
                          PID           IN NUMBER,
                          REPLYTEXT     in clob);

  procedure P_GetOutstandingParasAuditYear(io_cursor OUT t_cursor);

  procedure P_GetOldParasAuditYear(io_cursor OUT t_cursor);

  procedure P_responibilityassigned(ID        IN t_au_observation_responibility_assigned.id%type,
                                    PPNO      IN t_au_observation_responibility_assigned.assignedby%TYPE,
                                    RES_PP    IN t_au_observation_responibility_assigned.pp_no%type,
                                    LOANCASE  IN NUMBER,
                                    ACCNUMBER IN NUMBER,
                                    LCAMOUNT  IN NUMBER,
                                    ACAMOUNT  IN NUMBER);

  procedure P_DashboardDivisionalHeadfad(entityid  in number,
                                         io_cursor OUT t_cursor);
  procedure P_DashboardDivisionalHeadfadDetail(entityid  in number,
                                               io_cursor OUT t_cursor);

  Procedure P_getoldparamanagement(EnitityID in number,
                                   io_cursor OUT t_cursor);
  Procedure P_updateoldparamanagement(Paraid       in number,
                                      VCATID       in number,
                                      VCATNATUREID in number,
                                      RISKID       in number,
                                      paraText     in clob,
                                      CREATEDBY    IN NUMBER,
                                      io_cursor    OUT t_cursor);
  procedure P_GetAuditeeOldParasentities(EntityID  in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParas(EntityID  in number,
                                 io_cursor OUT t_cursor);

  Procedure P_UpdateAuditeeOldParasresponse(Paraid    in number,
                                            cdate     in date,
                                            Text      in clob,
                                            PPNO      in number,
                                            Remarks   in clob,
                                            imprec    in varchar2,
                                            io_cursor OUT t_cursor);

  Procedure P_SubmitAuditeeOldParasresponse(Paraid    in number,
                                            io_cursor OUT t_cursor);

  Procedure P_UpdateAuditeeOldPacompliance(Paraid    in number,
                                           text      in varchar2,
                                           status    in number,
                                           io_cursor OUT t_cursor);

  Procedure p_getglheadsummary_dash(PPNumber  in number,
                                    io_cursor OUT t_cursor);
  Procedure p_getglheadsummary_dash_Yearly(PPNumber  in number,
                                           io_cursor OUT t_cursor);

  procedure P_GetUserWiseOldParasPerformance(UserEntityID IN NUMBER,
                                             io_cursor    OUT t_cursor);

end pkg_AIS;

create or replace package body PKG_AIS is

  procedure p_get_user(PPNumber  in t_user.ppno%type,
                       enc_pass  in t_user.password%type,
                       io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      Select U.*, UM.*, e.Employeefirstname, e.employeelastname
        FROM t_user u
       inner join t_user_maping um
          on U.PPNO = UM.PPNO
        left join v_service_employeeinfo e
          on u.PPNO = e.PPNO
       WHERE U.PPNO = PPNumber
         and u.Password = enc_pass
         and u.ISACTIVE = 'Y';
  end p_get_user;

  procedure UPDATE_USERS(PPNUMBER  IN T_USER.PPNO%TYPE,
                         PASS      IN T_USER.PASSWORD%TYPE,
                         ISACTIVE  IN T_USER.ISACTIVE%TYPE,
                         ROLEID    IN T_USER_MAPING.ROLE_ID%TYPE,
                         io_cursor OUT t_cursor) as
  
  begin
    IF (PASS IS NOT NULL) THEN
      UPDATE t_user T
         SET T.PASSWORD = PASS, T.ISACTIVE = 'Y'
       where t.ppno = PPNUMBER;
      COMMIT;
    ELSE
      if (ISACTIVE is not null) then
        UPDATE t_user T SET T.ISACTIVE = 'Y' where t.ppno = PPNUMBER;
        COMMIT;
      END IF;
    END IF;
    IF (ROLEID IS NOT NULL) THEN
      DELETE FROM t_user_maping um WHERE um.PPNO = PPNUMBER;
      COMMIT;
      INSERT INTO t_user_maping
        (userid, ppno, group_id, role_id)
      VALUES
        ((SELECT U.USERID FROM T_USER U WHERE U.PPNO = PPNUMBER),
         PPNUMBER,
         ROLEID,
         ROLEID);
      COMMIT;
    
    END IF;
  
    OPEN io_cursor FOR
      SELECT r.id, r.remarks FROM T_AU_REMARKS R WHERE r.id = 10;
  
  end UPDATE_USERS;

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

  procedure AUDIT_CRITERIA_LOG(CREATEDBY_ID in T_AUDIT_CRITERIA_LOG.CREATEDBY_ID%type,
                               STATUS_ID    in T_AUDIT_CRITERIA_LOG.STATUS_ID%type,
                               REMARKS      in T_AUDIT_CRITERIA_LOG.REMARKS%type) is
  begin
    INSERT INTO T_AUDIT_CRITERIA_LOG al
      (al.ID,
       al.C_ID,
       al.STATUS_ID,
       al.CREATEDBY_ID,
       al.CREATED_ON,
       al.REMARKS,
       al.UPDATED_BY,
       al.LAST_UPDATED_ON)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA_LOG acc),
       (select max(acc1.ID) from T_AUDIT_CRITERIA acc1),
       STATUS_ID,
       CREATEDBY_ID,
       to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       REMARKS,
       CREATEDBY_ID,
       to_date(sysdate, 'dd/mm/yyyy HH:MI:SS AM'));
    commit;
  
  end AUDIT_CRITERIA_LOG;

  procedure AUDIT_JOINING(ENG_PLAN_ID     in T_AU_AUDIT_JOINING.ENG_PLAN_ID%type,
                          TEAM_MEM_PPNO   in T_AU_AUDIT_JOINING.TEAM_MEM_PPNO%type,
                          ENTEREDBY       in T_AU_AUDIT_JOINING.ENTEREDBY%type,
                          JOINING_DATE    in T_AU_AUDIT_JOINING.JOINING_DATE%type,
                          COMPLETION_DATE in T_AU_AUDIT_JOINING.COMPLETION_DATE%type) is
  begin
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
  
  end AUDIT_JOINING;

  procedure AUDIT_OBSERVATION_ASSIGNEDTO(OBS_ID       IN T_AU_OBSERVATION_ASSIGNEDTO.OBS_ID%type,
                                         ReplyByQuery IN T_AU_OBSERVATION_ASSIGNEDTO.ENTITY_ID%type,
                                         ENTEREDBY    IN T_AU_OBSERVATION_ASSIGNEDTO.ASSIGNEDBY%type) is
    S_B number := 0;
  begin
    select nvl(s.parent_enititid, 0)
      into S_B
      from T_AUDITEE_ENTITEE_SUBENTITY s
     where s.enitity_id = ReplyByQuery;
    if (S_B = 0) then
      INSERT INTO T_AU_OBSERVATION_ASSIGNEDTO ot
        (ot.ID,
         ot.OBS_ID,
         ot.OBS_TEXT_ID,
         ot.entity_id,
         ot.ASSIGNEDBY,
         ot.ASSIGNED_DATE,
         ot.IS_ACTIVE,
         ot.REPLIED)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_ASSIGNEDTO acc),
         OBS_ID,
         (select tt.ID
            from T_AU_OBSERVATION_TEXT tt
           WHERE tt.OBSERVATSION_ID = OBS_ID),
         (select o.entity_id from t_au_observation o where o.id = OBS_ID),
         --ReplyByQuery,
         ENTEREDBY,
         to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
         'Y',
         'N');
    
      commit;
    else
      INSERT INTO T_AU_OBSERVATION_ASSIGNEDTO ot
        (ot.ID,
         ot.OBS_ID,
         ot.OBS_TEXT_ID,
         ot.entity_id,
         ot.ASSIGNEDBY,
         ot.ASSIGNED_DATE,
         ot.IS_ACTIVE,
         ot.REPLIED)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_ASSIGNEDTO acc),
         OBS_ID,
         (select tt.ID
            from T_AU_OBSERVATION_TEXT tt
           WHERE tt.OBSERVATSION_ID = OBS_ID),
         (select o.entity_id from t_au_observation o where o.id = OBS_ID),
         S_B,
         to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
         'Y',
         'N');
      commit;
    end if;
  
  end AUDIT_OBSERVATION_ASSIGNEDTO;
/*
  PROCEDURE Branch_risk_rating_model is
  
  begin
  
    --DELETE FROM T_RISK_BRANCH_WISE;
    -- DELETE FROM T_BRANCH_RISK_RATING;
    --COMMIT;
  
    INSERT INTO T_RISK_BRANCH_WISE
      (AUDIT_PERIOD,
       ENITITY_CODE,
       entity_id,
       GR_ID,
       S_GR_ID,
       MAX_NUMBER,
       WEIGHTAGE_AVERAGE,
       GRAVITY_RISK,
       NUMBER_OF_OBSERVATIONS)
    
      SELECT P.AUDITPERIODID,
             E.ENTITY_CODE,
             e.entity_id,
             r.gr_id,
             rs.gr_id,
             r.max_number,
             rs.weightage as Weighted_Average,
             RS.GRAVITY,
             count(o.memo_number) as para
      
        FROM T_AU_PERIOD P
       INNER JOIN T_AU_PLAN_ENG E
          ON P.AUDITPERIODID = E.PERIOD_ID
       INNER JOIN T_AUDIT_PARA O
          ON E.ENG_ID = O.ENGPLANID
       INNER JOIN T_AUDIT_CHECKLIST_DETAILS D
          ON O.CHECKLISTDETAIL_ID = d.id
       INNER JOIN T_R_SUB_GROUP RS
          ON RS.S_GR_ID = D.V_ID
       INNER JOIN T_R_GROUP R
          ON R.GR_ID = RS.GR_ID
      
       GROUP BY P.AUDITPERIODID,
                E.ENTITY_CODE,
                e.entity_id,
                r.gr_id,
                rs.gr_id,
                r.max_number,
                rs.weightage,
                RS.GRAVITY;
    commit;
    update T_RISK_BRANCH_WISE t
       set t.risk_based_marks =
           (t.number_of_observations * T.GRAVITY_RISK);
    commit;
  
    update T_RISK_BRANCH_WISE t
       set t.weighted_average_marks =
           (t.number_of_observations * T.GRAVITY_RISK);
    commit;
  
    INSERT INTO T_BRANCH_RISK_RATING
      (AUDIT_PERIOD_ID, BRANCH_CODE, RISK_RATING)
    
      SELECT BB.AUDIT_PERIOD, BB.ENITITY_CODE, SUM(BB.RISK_BASED_MARKS)
        FROM T_RISK_BRANCH_WISE BB
       GROUP BY BB.AUDIT_PERIOD, BB.ENITITY_CODE;
    COMMIT;
    UPDATE T_BRANCH_RISK_RATING b
       set b.risk_category =
           (select r.rating
              from T_COSO_RATING r
             where b.risk_rating between (r.range_start) and (r.range_end));
    commit;
  
  end Branch_risk_rating_model;
*/
  procedure Coso_Risk_register(e_id in T_COSO_RATING_DEPARTMENT_INHERITED_RISK.ENTITY_ID%type) is
  
  begin
  
    insert into T_COSO_RATING_DEPARTMENT_INHERITED_RISK
      (AUDIT_PERIOD,
       DEPT_NAME,
       COSO_BASE_RATING_FACTORS,
       NUMBER_OF_SUB_FACTORS,
       MAXIMUM_SCORE_OF_SUB_FACTORS,
       OVERALL_WEIGHT_ASSIGNED,
       SCORE_ASSIGNED_BY_AUDITOR,
       WEIGHTED_AVERAGE_SCORE,
       
       STATUS,
       ENTITY_ID)
    
      (select p.description,
              a.description,
              c.v_name,
              count(o.id),
              (count(o.id) * 4),
              c.max_number,
              sum(o.risk_id),
              round((sum(o.risk_id) / ((count(o.id) * 4))) * (c.max_number)) as Average_Score,
              'Y',
              e.entity_id
         from t_au_period         p,
              t_au_ccq            o,
              t_au_plan           e,
              t_Control_Violation c,
              t_auditee_entities  a
        where p.auditperiodid = (e.auditperiodid + 1)
          and c.id = o.control_violation_id
          and a.entity_id = e.entity_id
          and e.entity_id = o.entity_id
          and p.status_id = 1
          and e.entity_id = e_id
        group by p.description, a.description, c.v_name, c.max_number);
    commit;
    update T_COSO_RATING_DEPARTMENT_INHERITED_RISK up
       set up.audit_rating =
           (select rr.rating
              from t_coso_rating rr
             where Round(up.weighted_average_score) between (rr.range_start) and
                   (rr.range_end));
    commit;
  
    insert into T_COSO_RATING_DEPARTMENT_FINAL
      (AUDIT_PERIOD, DEPT, FINAL_SCORE)
    
      select rr.audit_period, rr.dept_name, sum(rr.weighted_average_score)
        from T_COSO_RATING_DEPARTMENT_INHERITED_RISK rr
       group by rr.audit_period, rr.dept_name;
    commit;
  
    update T_COSO_RATING_DEPARTMENT_FINAL fn
       set fn.final_rating =
           (select rr.rating
              from t_coso_rating rr
             where Round(fn.final_score) between (rr.range_start) and
                   (rr.range_end))
     where fn.final_rating is null;
  
  end Coso_Risk_register;

  procedure Criteria(CID IN t_audit_criteria.id%type) is
  begin
    update t_audit_criteria t
       set t.no_of_entity =
           (select count(*)
              from t_audit_criteria a
             inner join t_au_period p
                on a.auditperiodid = p.auditperiodid
             inner join t_auditee_entities e
                on a.entity_typeid = e.type_id
              left join t_auditee_entities_risk er
                on e.entity_id = er.entity_id
              left join t_auditee_entities_size es
                on e.entity_id = es.entity_id
             inner join t_audit_frequency f
                on a.frequency_id = f.frequency_id
             inner join t_auditee_entities_size_disc ess
                on ess.entity_size = es.entity_size
             inner join t_risk_status ers
                on ers.r_id = er.risk_rating
             where a.auditperiodid = er.audit_period_id
               and a.size_id = es.entity_size
               and a.risk_id = er.risk_rating
               AND T.ID = CID
               and p.status_id = 2
               and a.id = t.id)
     WHERE T.NO_OF_ENTITY IS NULL;
    commit;
  end Criteria;

  procedure P_oldparasresponibilityassigned(ID IN t_au_observation_old_paras_responibility_assigned.ref_p%type,
                                            
                                            pp IN t_au_observation_old_paras_responibility_assigned.pp_no%type) is
  begin
    INSERT INTO t_au_observation_old_paras_responibility_assigned
      (ID, REF_P, PP_NO, STATUS)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from t_au_observation_old_paras_responibility_assigned acc),
       ID,
       pp,
       1);
  
    commit;
  
  end P_oldparasresponibilityassigned;

  procedure Pre_Coso_Risk_register(ENTITYID in number) is
  
  begin
    --delete from t_coso_rating;
    --delete from T_COSO_RATING_DEPARTMENT_INHERITED_RISK;
    --delete from T_COSO_RATING_DEPARTMENT_FINAL;
    --commit;
  
    insert into T_COSO_RATING_DEPARTMENT_INHERITED_RISK
      (AUDIT_PERIOD,
       DEPT_NAME,
       COSO_BASE_RATING_FACTORS,
       NUMBER_OF_SUB_FACTORS,
       MAXIMUM_SCORE_OF_SUB_FACTORS,
       OVERALL_WEIGHT_ASSIGNED,
       SCORE_ASSIGNED_BY_AUDITOR,
       WEIGHTED_AVERAGE_SCORE,
       STATUS,
       ENTITY_ID)
      select p.description,
             a.description,
             c.v_name,
             count(o.id),
             (count(o.id) * 3),
             c.max_number,
             sum(o.risk_rating),
             round((sum(o.risk_rating) / ((count(o.id) * 3))) *
                   (c.max_number)) as Average_Score,
             'Y',
             ENTITYID
        from t_au_period         p,
             t_au_ccq            o,
             t_Control_Violation c,
             t_auditee_entities  a
       where p.status_id = 1
         and c.id = o.control_violation_id
            --and a.entity_id = e.entity_code
         and a.entity_id = o.entity_id
      --and a.type_id in (4, 14)
       group by p.description, a.description, c.v_name, c.max_number;
    commit;
    update T_COSO_RATING_DEPARTMENT_INHERITED_RISK up
       set up.audit_rating =
           (select rr.rating
              from t_coso_rating rr
             where up.weighted_average_score between (rr.range_start) and
                   (rr.range_end));
    commit;
    insert into T_COSO_RATING_DEPARTMENT_FINAL
      (AUDIT_PERIOD, DEPT, Final_Score)
      select d.audit_period, d.dept_name, sum(d.weighted_average_score)
        from T_COSO_RATING_DEPARTMENT_INHERITED_RISK d
       group by d.audit_period, d.dept_name;
  
    update T_COSO_RATING_DEPARTMENT_FINAL de
       set de.Final_RATING =
           (select r.rating
              from T_COSO_RATING r
             where de.Final_Score between (r.range_start) and (r.range_end))
    
    ;
  
    commit;
  end Pre_Coso_Risk_register;

  procedure Session_END(PPNumber  in t_user_session.user_pp_number%type,
                        SessionId in t_user_session.session_id%type) is
  
  begin
  
    update t_user_session ss
       set ss.logged_out_date = sysdate, SS.session_active = 'N'
     where ss.session_active = 'Y'
       and ss.user_pp_number = PPNumber
       and ss.session_id = SessionId;
    commit;
  End Session_END;

  procedure Session_Kill(PPNumber in t_user_session.user_pp_number%type) is
  
  begin
  
    update t_user_session ss
       set ss.logged_out_date = sysdate, SS.session_active = 'N'
     where ss.session_active = 'Y'
       and ss.user_pp_number = PPNumber;
    commit;
  End Session_Kill;

  procedure Session_Kill_day_end(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_cursor FOR
      select count(*) from t_user_session s where s.session_active = 'Y';
  
    update t_user_session s
       set s.session_active = 'N'
     where s.session_active = 'Y';
  
    commit;
  
  End Session_Kill_day_end;

  procedure TEAM_TASKLIST(TEAM_ID         in T_AU_AUDIT_TEAM_TASKLIST.TEAM_ID%type,
                          sequence_no     in T_AU_AUDIT_TEAM_TASKLIST.SEQUENCE_NO%type,
                          member_pp       in T_AU_AUDIT_TEAM_TASKLIST.TEAMMEMBER_PPNO%type,
                          ENTITY_ID       in T_AU_AUDIT_TEAM_TASKLIST.ENTITY_ID%type,
                          AUDIT_STARTDATE in T_AU_AUDIT_TEAM_TASKLIST.AUDIT_START_DATE%type,
                          AUDIT_endDATE   in T_AU_AUDIT_TEAM_TASKLIST.AUDIT_END_DATE%type) is
  begin
  
    insert into T_AU_AUDIT_TEAM_TASKLIST t
      (t.ID,
       t.ENG_PLAN_ID,
       t.TEAM_ID,
       t.SEQUENCE_NO,
       t.TEAMMEMBER_PPNO,
       t.ENTITY_ID,
       t.ENTITY_CODE,
       t.ENTITY_NAME,
       t.AUDIT_START_DATE,
       t.AUDIT_END_DATE,
       t.STATUS_ID,
       t.ISACTIVE)
    VALUES
      ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM T_AU_AUDIT_TEAM_TASKLIST ll),
       (SELECT max(lp.ENG_ID) FROM t_au_plan_eng lp),
       TEAM_ID,
       sequence_no,
       member_pp,
       ENTITY_ID,
       (select et.code
          from t_auditee_entities et
         where et.entity_id = ENTITY_ID),
       (select et.name
          from t_auditee_entities et
         where et.entity_id = ENTITY_ID),
       to_date(AUDIT_STARTDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       to_date(AUDIT_ENDDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       1,
       'N');
    commit;
  
  end TEAM_TASKLIST;

  procedure Tentative_Audit_Plan(CRITERIA_ID in t_audit_criteria.id%type,
                                 io_cursor   OUT t_cursor) is
    v_f number;
  begin
  
    for j in (select * from t_audit_criteria acl where acl.id = CRITERIA_ID) loop
    
      select time
        into v_f
        from T_AUDIT_FREQUENCY af
       where af.frequency_id = (select ac.frequency_id
                                  from t_audit_criteria ac
                                 where ac.id = j.id);
    
      if (j.entity_id is not null) then
        INSERT INTO T_AU_PLAN
          (CRITERIA_ID,
           AUDITPERIODID,
           AUDITEDBY,
           ENTITY_ID,
           ENTITY_CODE,
           AUDITEE_RISK,
           AUDITEE_SIZE,
           NO_OF_DAYS,
           FR_ID,
           COST_CENTER,
           ENTITY_TYPEID,
           STATUS,
           f_id)
          select a.id,
                 a.auditperiodid,
                 e.auditby_id,
                 a.entity_id,
                 e.code,
                 a.risk_id,
                 a.size_id,
                 a.no_of_days,
                 a.frequency_id,
                 e.cost_center,
                 e.type_id,
                 '1',
                 1
            from t_audit_criteria a
           inner join t_au_period p
              on a.auditperiodid = p.auditperiodid
           inner join t_auditee_entities e
              on a.entity_id = e.entity_id
           where a.id = CRITERIA_ID
             and e.auditable = 'Y';
        commit;
      
      else
        if (j.entity_id is null AND j.frequency_id = 3) then
          for i in 1 .. v_f loop
          
            INSERT INTO T_AU_PLAN
              (CRITERIA_ID,
               AUDITPERIODID,
               AUDITEDBY,
               ENTITY_ID,
               ENTITY_CODE,
               AUDITEE_RISK,
               AUDITEE_SIZE,
               NO_OF_DAYS,
               FR_ID,
               COST_CENTER,
               ENTITY_TYPEID,
               STATUS,
               f_id)
              select a.id,
                     a.auditperiodid,
                     e.auditby_id,
                     e.entity_id,
                     e.code,
                     ers.r_id,
                     ess.entity_size,
                     a.no_of_days,
                     f.id,
                     e.cost_center,
                     e.type_id,
                     '1',
                     i
                from t_audit_criteria a
               inner join t_au_period p
                  on a.auditperiodid = p.auditperiodid
               inner join t_auditee_entities e
                  on a.entity_typeid = e.type_id
               inner join t_auditee_entities_risk er
                  on e.entity_id = er.entity_id
               inner join t_auditee_entities_size es
                  on e.entity_id = es.entity_id
               inner join t_audit_frequency f
                  on a.frequency_id = f.frequency_id
               inner join t_auditee_entities_size_disc ess
                  on ess.entity_size = es.entity_size
               inner join t_risk_status ers
                  on ers.r_id = er.risk_rating
               where a.auditperiodid = er.audit_period_id
                 and a.size_id = es.entity_size
                 and a.risk_id = er.risk_rating
                 and e.auditable = 'Y'
                 and a.id = CRITERIA_ID
                 and a.id = j.id;
          end loop;
          commit;
        else
          for i in 1 .. v_f loop
          
            INSERT INTO T_AU_PLAN
              (CRITERIA_ID,
               AUDITPERIODID,
               AUDITEDBY,
               ENTITY_ID,
               ENTITY_CODE,
               AUDITEE_RISK,
               AUDITEE_SIZE,
               NO_OF_DAYS,
               FR_ID,
               COST_CENTER,
               ENTITY_TYPEID,
               STATUS,
               f_id)
              select a.id,
                     a.auditperiodid,
                     e.auditby_id,
                     e.entity_id,
                     e.code,
                     ers.r_id,
                     ess.entity_size,
                     a.no_of_days,
                     f.id,
                     e.cost_center,
                     e.type_id,
                     '1',
                     i
                from t_audit_criteria a
               inner join t_au_period p
                  on a.auditperiodid = p.auditperiodid
               inner join t_auditee_entities e
                  on a.entity_typeid = e.type_id
               inner join t_auditee_entities_risk er
                  on e.entity_id = er.entity_id
               inner join t_auditee_entities_size es
                  on e.entity_id = es.entity_id
               inner join t_audit_frequency f
                  on a.frequency_id = f.frequency_id
               inner join t_auditee_entities_size_disc ess
                  on ess.entity_size = es.entity_size
               inner join t_risk_status ers
                  on ers.r_id = er.risk_rating
               where a.auditperiodid = er.audit_period_id
                 and a.size_id = es.entity_size
                 and a.risk_id = er.risk_rating
                 and e.auditable = 'Y'
                 and a.id = CRITERIA_ID
                 and a.id = j.id;
          end loop;
          commit;
        end if;
      end if;
    end loop;
  
    commit;
  
    update t_audit_criteria aa
       set aa.approval_status = 5
     where aa.id = CRITERIA_ID;
  
    commit;
  
    open io_cursor for
      select m.ref, m.remarks from t_au_remarks m where m.id = 17;
  
    /* open io_cursor for
        
          select m.ref, m.remarks from t_au_remarks m where m.id = 14;
      end if;
    */
  end Tentative_Audit_Plan;

  procedure P_ADDCADAuditPlan(auditperiod_id in T_AU_PLAN.AUDITPERIODID%type,
                              auditby_id     in number,
                              entityid       in number,
                              noofdays       in number,
                              typeid         in number) is
  
  begin
  
    INSERT INTO T_AU_PLAN
      (CRITERIA_ID,
       AUDITPERIODID,
       AUDITEDBY,
       ENTITY_ID,
       ENTITY_CODE,
       AUDITEE_RISK,
       AUDITEE_SIZE,
       NO_OF_DAYS,
       FR_ID,
       ENTITY_TYPEID,
       STATUS)
    values
      (0,
       auditperiod_id,
       auditby_id,
       entityid,
       (select eE.code
          from t_auditee_entities ee
         where ee.entitY_id = entityid),
       1,
       1,
       noofdays,
       1,
       typeid,
       '1');
    commit;
  
  end P_ADDCADAuditPlan;

  

  procedure user_maping(USER_ID in t_user_maping.userid%type,
                        ROLE_ID in t_user_maping.role_id%type,
                        PPNO    in t_user_maping.ppno%type) is
    io number := 0;
  begin
    select m.ppno into io from t_user_maping m where m.ppno = PPNO;
    if (ppno = io) then
      update t_user_maping mm
         set mm.GROUP_ID = ROLE_ID
       where mm.ppno = PPNO;
      commit;
    
      update t_user_maping mm
         set mm.ROLE_ID = ROLE_ID
       where mm.ppno = PPNO;
      commit;
    else
      INSERT INTO t_user_maping
        (USERID, PPNO, GROUP_ID, ROLE_ID)
      VALUES
        (USER_ID, PPNO, ROLE_ID, ROLE_ID);
      commit;
    end if;
  end user_maping;

  procedure User_SESSION(PPNumber             in T_USER_SESSION.USER_PP_NUMBER%type,
                         UserRoleID           IN T_USER_SESSION.ROLE_ID%type,
                         LocalIpAddress       IN T_USER_SESSION.IP_ADDRESS%type,
                         SessionId            IN T_USER_SESSION.SESSION_ID%type,
                         UserLocationType     IN T_USER_SESSION.LOGIN_LOCATION_TYPE%type,
                         MACAddress           IN T_USER_SESSION.MAC_ADDRESS%type,
                         FirstMACCardAddress  IN T_USER_SESSION.PRIMARY_MAC_CARD_ADDRESS%type,
                         UserPostingDiv       IN T_USER_SESSION.POSTING_DIV%type,
                         UserGroupID          IN T_USER_SESSION.GROUP_ID%type,
                         UserPostingDept      IN T_USER_SESSION.POSTING_DEPT%type,
                         UserPostingZone      IN T_USER_SESSION.POSTING_ZONE%type,
                         UserPostingBranch    IN T_USER_SESSION.POSTING_BRANCH%type,
                         UserPostingAuditZone IN T_USER_SESSION.POSTING_AZ%type) is
  
  begin
    INSERT INTO T_USER_SESSION
      (ID,
       USER_PP_NUMBER,
       ROLE_ID,
       IP_ADDRESS,
       SESSION_ID,
       LOGIN_LOCATION_TYPE,
       MAC_ADDRESS,
       PRIMARY_MAC_CARD_ADDRESS,
       POSTING_DIV,
       GROUP_ID,
       POSTING_DEPT,
       POSTING_ZONE,
       POSTING_BRANCH,
       POSTING_AZ,
       SESSION_ACTIVE)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from T_USER_SESSION p),
       PPNumber,
       UserRoleID,
       LocalIpAddress,
       SessionId,
       UserLocationType,
       MACAddress,
       FirstMACCardAddress,
       UserPostingDiv,
       UserGroupID,
       UserPostingDept,
       UserPostingZone,
       UserPostingBranch,
       UserPostingAuditZone,
       'Y');
  
    commit;
  
  end User_SESSION;

  procedure p_get_audit_plan(AUDITED_BY in number, io_cursor OUT t_cursor) as
  begin
    IF (AUDITED_BY NOT IN ('112243', '112201', '113068')) THEN
      OPEN io_cursor FOR
        select a.criteria_id           as CRITERIA_ID,
               A.ID                    AS PLAN_ID,
               a.AUDITPERIODID         as AUDITPERIODID,
               e.auditby_id            as AUDITEDBY,
               ess.description         as AUDITEE_SIZE,
               ers.description         as AUDITEE_RISK,
               a.no_of_days            as NO_OF_DAYS,
               e.entity_id             as ENTITY_ID,
               e.type_id               as ENTITY_TYPE_ID,
               e.code                  as ENTITY_CODE,
               mp.p_name               as Reporting_office,
               e.name                  as AUDITEE_NAME,
               f.frequency_discription as FREQUENCY_DISCRIPTION,
               p.description           as PERIOD_NAME
        
          from t_au_plan a
         inner join t_au_period p
            on a.auditperiodid = p.auditperiodid
         inner join t_auditee_entities e
            on a.entity_id = e.entity_id
         inner join t_audit_departments d
            on d.entity_id = e.auditby_id
         inner join t_audit_frequency f
            on a.f_id = f.frequency_id
         inner join t_auditee_entities_size_disc ess
            on ess.entity_size = a.auditee_size
         inner join t_risk_status ers
            on ers.r_id = a.auditee_risk
          left join t_auditee_entities_maping mp
            on mp.entity_id = a.entity_id
         where a.status is not null
           and a.status = 1
           AND A.AUDITEDBY = AUDITED_BY
         order by a.auditee_risk Asc;
    
    ELSE
      IF (AUDITED_BY = ('112243')) THEN
        OPEN io_cursor FOR
          select a.criteria_id           as CRITERIA_ID,
                 A.ID                    AS PLAN_ID,
                 a.AUDITPERIODID         as AUDITPERIODID,
                 e.auditby_id            as AUDITEDBY,
                 ess.description         as AUDITEE_SIZE,
                 ers.description         as AUDITEE_RISK,
                 a.no_of_days            as NO_OF_DAYS,
                 e.entity_id             as ENTITY_ID,
                 e.type_id               as ENTITY_TYPE_ID,
                 e.code                  as ENTITY_CODE,
                 mp.p_name               as Reporting_office,
                 e.name                  as AUDITEE_NAME,
                 f.frequency_discription as FREQUENCY_DISCRIPTION,
                 p.description           as PERIOD_NAME
          
            from t_au_plan a
           inner join t_au_period p
              on a.auditperiodid = p.auditperiodid
           inner join t_auditee_entities e
              on a.entity_id = e.entity_id
           inner join t_audit_departments d
              on d.entity_id = e.auditby_id
           inner join t_audit_frequency f
              on a.f_id = f.frequency_id
           inner join t_auditee_entities_size_disc ess
              on ess.entity_size = a.auditee_size
           inner join t_risk_status ers
              on ers.r_id = a.auditee_risk
            left join t_auditee_entities_maping mp
              on mp.entity_id = a.entity_id
           where a.status is not null
                
             AND D.DEPTNAME LIKE ('%AUDIT ZONE%')
           order by a.auditee_risk asc;
      
      ELSE
        IF (AUDITED_BY IN ('112201', '113068')) THEN
          OPEN io_cursor FOR
            select a.criteria_id           as CRITERIA_ID,
                   A.ID                    AS PLAN_ID,
                   a.AUDITPERIODID         as AUDITPERIODID,
                   e.auditby_id            as AUDITEDBY,
                   ess.description         as AUDITEE_SIZE,
                   ers.description         as AUDITEE_RISK,
                   a.no_of_days            as NO_OF_DAYS,
                   e.entity_id             as ENTITY_ID,
                   e.type_id               as ENTITY_TYPE_ID,
                   e.code                  as ENTITY_CODE,
                   mp.p_name               as Reporting_office,
                   e.name                  as AUDITEE_NAME,
                   f.frequency_discription as FREQUENCY_DISCRIPTION,
                   p.description           as PERIOD_NAME
            
              from t_au_plan a
             inner join t_au_period p
                on a.auditperiodid = p.auditperiodid
             inner join t_auditee_entities e
                on a.entity_id = e.entity_id
             inner join t_audit_departments d
                on d.entity_id = e.auditby_id
             inner join t_audit_frequency f
                on a.f_id = f.frequency_id
             inner join t_auditee_entities_size_disc ess
                on ess.entity_size = a.auditee_size
             inner join t_risk_status ers
                on ers.r_id = a.auditee_risk
              left join t_auditee_entities_maping mp
                on mp.entity_id = a.entity_id
             where a.status is not null
            
             order by a.auditee_risk asc;
        
        END IF;
      END IF;
    END IF;
  end p_get_audit_plan;

  procedure p_get_user_id(PPNumber  in t_user.ppno%type,
                          io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      SELECT u.ID
        FROM T_USER_SESSION u
       WHERE u.USER_PP_NUMBER = PPNumber
         and u.SESSION_ACTIVE = 'Y';
  end p_get_user_id;

  procedure p_get_allusers(ENTITYID  in t_user.ppno%type,
                           EMAIL     in v_service_employeeinfo.EMAIL%type,
                           GROUPID   in t_groups.group_id%type,
                           PPNUMBER  in t_user.ppno%type,
                           LOGINNAME in t_user.login_name%type,
                           io_cursor OUT t_cursor) as
  begin
  
    if (PPNUMBER != 0) then
      OPEN io_cursor FOR
        select u.userid,
               mp.parent_id,
               mp.parent_code,
               mp.child_code,
               mp.p_type_id,
               mp.c_type_id,
               u.ppno,
               u.entity_id,
               e.code,
               e.type_id,
               emp.EMPLOYEEFIRSTNAME,
               emp.EMPLOYEELASTNAME,
               emp.EMAIL,
               u.ISACTIVE,
               r.group_name,
               rm.group_id,
               mp.p_name,
               mp.c_name
          from t_auditee_entities e
         INNER join t_auditee_entities_maping mp
            on e.entity_id = mp.entity_id
         inner join t_user u
            on u.entity_id = e.entity_id
         INNER join v_service_employeeinfo emp
            on emp.PPNO = u.ppno
          left join t_user_maping rm
            on u.PPNO = rm.ppno
          left join t_groups r
            on r.role_id = rm.role_id
         WHERE u.ppno = PPNUMBER
        
         ORDER BY emp.CURRENTRANKCODE;
    
    else
      if (LOGINNAME != 0) then
        OPEN io_cursor FOR
          select u.userid,
                 mp.parent_id,
                 mp.parent_code,
                 mp.child_code,
                 mp.p_type_id,
                 mp.c_type_id,
                 u.ppno,
                 u.entity_id,
                 e.code,
                 e.type_id,
                 emp.EMPLOYEEFIRSTNAME,
                 emp.EMPLOYEELASTNAME,
                 emp.EMAIL,
                 u.ISACTIVE,
                 r.group_name,
                 rm.group_id,
                 mp.p_name,
                 mp.c_name
            from t_auditee_entities e
           inner join t_auditee_entities_maping mp
              on e.entity_id = mp.entity_id
           inner join t_user u
              on u.entity_id = e.entity_id
           inner join v_service_employeeinfo emp
              on emp.PPNO = u.ppno
            left join t_user_maping rm
              on u.PPNO = rm.ppno
            left join t_groups r
              on r.role_id = rm.role_id
           WHERE u.login_name = LOGINNAME
             and e.type_id = mp.c_type_id
           ORDER BY emp.CURRENTRANKCODE;
      else
        if (ENTITYID != 0) then
          OPEN io_cursor FOR
            select u.userid,
                   mp.parent_id,
                   mp.parent_code,
                   mp.child_code,
                   mp.p_type_id,
                   mp.c_type_id,
                   u.ppno,
                   u.entity_id,
                   e.code,
                   e.type_id,
                   emp.EMPLOYEEFIRSTNAME,
                   emp.EMPLOYEELASTNAME,
                   emp.EMAIL,
                   u.ISACTIVE,
                   r.group_name,
                   rm.group_id,
                   mp.p_name,
                   mp.c_name
              from t_auditee_entities e
             inner join t_auditee_entities_maping mp
                on e.entity_id = mp.entity_id
             inner join t_user u
                on u.entity_id = e.entity_id
             inner join v_service_employeeinfo emp
                on emp.PPNO = u.ppno
              left join t_user_maping rm
                on u.PPNO = rm.ppno
              left join t_groups r
                on r.role_id = rm.role_id
             WHERE u.ENTITY_ID = entityid
               and e.type_id = mp.c_type_id
             ORDER BY emp.CURRENTRANKCODE;
        else
          if (GROUPID != 0) then
            OPEN io_cursor FOR
              select u.userid,
                     mp.parent_id,
                     mp.parent_code,
                     mp.child_code,
                     mp.p_type_id,
                     mp.c_type_id,
                     u.ppno,
                     u.entity_id,
                     e.code,
                     e.type_id,
                     emp.EMPLOYEEFIRSTNAME,
                     emp.EMPLOYEELASTNAME,
                     emp.EMAIL,
                     u.ISACTIVE,
                     r.group_name,
                     rm.group_id,
                     mp.p_name,
                     mp.c_name
                from t_auditee_entities e
               inner join t_auditee_entities_maping mp
                  on e.entity_id = mp.entity_id
               inner join t_user u
                  on u.entity_id = e.entity_id
               inner join v_service_employeeinfo emp
                  on emp.PPNO = u.ppno
                left join t_user_maping rm
                  on u.PPNO = rm.ppno
                left join t_groups r
                  on r.role_id = rm.role_id
               WHERE r.group_id = GROUPID
                 and e.type_id = mp.c_type_id
               ORDER BY emp.CURRENTRANKCODE;
          else
            if (EMAIL is not null) then
              OPEN io_cursor FOR
                select u.userid,
                       mp.parent_id,
                       mp.parent_code,
                       mp.child_code,
                       mp.p_type_id,
                       mp.c_type_id,
                       u.ppno,
                       u.entity_id,
                       e.code,
                       e.type_id,
                       emp.EMPLOYEEFIRSTNAME,
                       emp.EMPLOYEELASTNAME,
                       emp.EMAIL,
                       u.ISACTIVE,
                       r.group_name,
                       rm.group_id,
                       mp.p_name,
                       mp.c_name
                  from t_auditee_entities e
                 inner join t_auditee_entities_maping mp
                    on e.entity_id = mp.entity_id
                 inner join t_user u
                    on u.entity_id = e.entity_id
                 inner join v_service_employeeinfo emp
                    on emp.PPNO = u.ppno
                  left join t_user_maping rm
                    on u.PPNO = rm.ppno
                  left join t_groups r
                    on r.role_id = rm.role_id
                 WHERE emp.EMAIL = email
                   and e.type_id = mp.c_type_id
                 ORDER BY emp.CURRENTRANKCODE;
            end if;
          end if;
        end if;
      end if;
    end if;
  
  end p_get_allusers;

  procedure p_get_user_session(PPNumber  in t_user.ppno%type,
                               io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      SELECT u.ID
        FROM T_USER_SESSION u
       WHERE u.USER_PP_NUMBER = PPNumber
         and u.SESSION_ACTIVE = 'Y';
  end p_get_user_session;

  procedure p_GetTopMenus(UserRoleID in t_user_group_map.role_id%type,
                          io_cursor  OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      select distinct (m.menu_name), m.*
        from t_menu m
       inner join t_menu_pages p
          on m.menu_id = p.menu_id
       inner join T_MENU_PAGES_GROUPMAP r
          on r.page_id = p.id
       where r.group_id = UserRoleID
         and p.status = 'A'
       ORDER BY M.MENU_ORDER ASC;
  
  end p_GetTopMenus;

  procedure p_GetTopMenuPages(UserGroupID in t_menu_pages_groupmap.group_id%type,
                              io_cursor   OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      Select *
        FROM T_MENU_PAGES mp
       inner join t_menu_pages_groupmap mpg
          on mp.Id = mpg.page_id
       WHERE mp.Status = 'A'
         and mpg.GROUP_ID = UserGroupID
       order by mp.PAGE_ORDER asc;
  
  end p_GetTopMenuPages;

  procedure P_GetAllTopMenus(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      select m.* from t_menu m ORDER BY M.MENU_ORDER ASC;
  
  end P_GetAllTopMenus;

  procedure P_GetAssignedMenuPages(groupId   in t_menu_pages_groupmap.group_id%type,
                                   menuId    in T_MENU_PAGES.MENU_ID%type,
                                   io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      Select *
        FROM T_MENU_PAGES mp
       inner join t_menu_pages_groupmap mpg
          on mp.Id = mpg.page_id
       WHERE mp.Status = 'A'
         and mpg.GROUP_ID = groupId
         and mp.MENU_ID = menuId
       order by mp.PAGE_ORDER asc;
  
  end P_GetAssignedMenuPages;

  procedure P_AddGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                          PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type) is
  begin
    delete from T_MENU_PAGES_GROUPMAP mp
     where mp.group_id = groupid
       and mp.page_id = PAGEID;
    commit;
    INSERT INTO T_MENU_PAGES_GROUPMAP
      (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    VALUES
      ((select COALESCE(max(p.GROUPMAP_ID) + 1, 1)
         from T_MENU_PAGES_GROUPMAP p),
       groupid,
       PAGEID);
    commit;
  end P_AddGroupMenuItemsAssignment;

  procedure P_RemoveGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                             PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type) is
  begin
    delete from T_MENU_PAGES_GROUPMAP mp
     where mp.group_id = groupid
       and mp.page_id = PAGEID;
  
    commit;
  
  end P_RemoveGroupMenuItemsAssignment;

  procedure P_GETALLMENUPAGE(io_cursor OUT t_cursor) as
  begin

      OPEN io_cursor FOR
        Select * FROM T_MENU_PAGES mp
        where mp.id is not null and mp.page_path is not null and mp.page_url is not null
         order by mp.PAGE_ORDER;

  end P_GETALLMENUPAGE;

  procedure P_updateAllMenuPages(menuId in T_MENU_PAGES.MENU_ID%type,
                                 p_id   in T_MENU_PAGES.MENU_ID%type) as
  begin
  
    update T_MENU_PAGES mp set mp.status = 'A' WHERE mp.id = p_id;
    commit;
    update T_MENU_PAGES mp set mp.menu_id = menuId WHERE mp.id = p_id;
    commit;
  
  end P_updateAllMenuPages;
  procedure P_GetGroups(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      select g.* from t_groups g WHERE g.STATUS = 'Y' ORDER BY g.GROUP_ID;
  
  end P_GetGroups;

  procedure P_GetRoleResponsibilities(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      select * from t_hr_designations s WHERE s.STATUSTYPE = 'A';
  
  end P_GetRoleResponsibilities;

  procedure P_Group_Update(GROUP_ID          in t_groups.group_id%type,
                           GROUP_DESCRIPTION in t_groups.description%type,
                           GROUP_NAME        in t_groups.group_name%type,
                           ISACTIVE          in t_groups.status%type) as
  begin
  
    UPDATE T_GROUPS g
       SET g.GROUP_NAME  = GROUP_NAME,
           g.DESCRIPTION = GROUP_DESCRIPTION,
           g.STATUS      = ISACTIVE
     WHERE g.GROUP_ID = GROUP_ID;
    commit;
  
  end P_Group_Update;

  procedure p_AddGroup(GROUP_DESCRIPTION in t_groups.description%type,
                       GROUP_NAME        in t_groups.group_name%type,
                       ISACTIVE          in t_groups.status%type) is
  begin
  
    INSERT INTO t_groups g
      (g.ROLE_ID, g.GROUP_ID, g.DESCRIPTION, g.GROUP_NAME, g.STATUS)
    VALUES
      ((select COALESCE(max(pr.ROLE_ID) + 1, 1) from t_groups pr),
       (select COALESCE(max(pg.GROUP_ID) + 1, 1) from t_groups pg),
       GROUP_DESCRIPTION,
       GROUP_NAME,
       ISACTIVE);
    commit;
  
  end p_AddGroup;

  procedure P_GetAuditPeriods(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select p.* from T_AU_PERIOD p order by p.AUDITPERIODID asc;
  
  end P_GetAuditPeriods;

  procedure P_AddAuditPeriod(DESCRIPTION in T_AU_PERIOD.DESCRIPTION%type,
                             START_DATE  in T_AU_PERIOD.START_DATE%type,
                             END_DATE    in T_AU_PERIOD.End_Date%type,
                             io_cursor   OUT t_cursor) is
    A_P NUMBER := 0;
  
  begin
  
    SELECT nvl(max(P.AUDITPERIODID), 0)
      INTO A_P
      FROM T_AU_PERIOD P
     WHERE P.START_DATE BETWEEN START_DATE AND END_DATE
        OR P.END_DATE BETWEEN START_DATE AND END_DATE;
  
    IF (A_P = 0) THEN
      insert into T_AU_PERIOD p
        (p.AUDITPERIODID,
         p.DESCRIPTION,
         p.START_DATE,
         p.END_DATE,
         p.STATUS_ID)
      VALUES
        ((SELECT COALESCE(max(PP.AUDITPERIODID) + 1, 1) FROM T_AU_PERIOD PP),
         DESCRIPTION,
         START_DATE,
         END_DATE,
         '2');
      commit;
    
      OPEN io_cursor FOR
        SELECT R.REF, R.REMARKS FROM T_AU_REMARKS R WHERE R.ID = 4;
    ELSE
      OPEN io_cursor FOR
        SELECT R.REF, R.REMARKS FROM T_AU_REMARKS R WHERE R.ID = 5;
    
    END IF;
  end P_AddAuditPeriod;

  procedure P_GetAuditTeams(dept_code    IN NUMBER,
                            UserEntityID IN NUMBER,
                            io_cursor    OUT t_cursor) as
  
  begin
    IF (dept_code != 0) THEN
      OPEN io_cursor FOR
        select t.*, d.name as AUDIT_DEPARTMENT
          from t_au_team_members t
         inner join t_auditee_entities d
            on d.entity_id = t.PLACE_OF_POSTING
         Where d.code = dept_code
           and t.status = 'Y'
         order by t.status desc, t.ISTEAMLEAD desc;
    ELSE
      OPEN io_cursor FOR
        select t.*, d.name as AUDIT_DEPARTMENT
          from t_au_team_members t
         inner join t_auditee_entities d
            on d.entity_id = t.PLACE_OF_POSTING
         Where t.PLACE_OF_POSTING = UserEntityID
         order by t.status desc, t.ISTEAMLEAD desc;
    END IF;
  
  end P_GetAuditTeams;

  PROCEDURE P_MAXTEAMID(io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT COALESCE(max(PP.T_ID) + 1, 1) AS MAX_T_ID
        FROM T_AU_TEAM_MEMBERS PP;
  END P_MAXTEAMID;

  procedure P_AddAuditTeam(TEAMNAME      in T_AU_TEAM_MEMBERS.TEAM_NAME%type,
                           TEAMMEMBER_ID in T_AU_TEAM_MEMBERS.MEMBER_PPNO%type,
                           MAX_T_ID      IN T_AU_TEAM_MEMBERS.T_ID%TYPE,
                           EMPLOYEENAME  in T_AU_TEAM_MEMBERS.MEMBER_NAME%type,
                           IS_TEAMLEAD   in T_AU_TEAM_MEMBERS.ISTEAMLEAD%type,
                           STATUS        in T_AU_TEAM_MEMBERS.STATUS%type,
                           entityid      in number) is
  
    typeid number := 0;
  begin
    INSERT INTO T_AU_TEAM_MEMBERS
      (ID,
       T_ID,
       T_CODE,
       TEAM_NAME,
       MEMBER_PPNO,
       MEMBER_NAME,
       ISTEAMLEAD,
       PLACE_OF_POSTING,
       STATUS)
    VALUES
      ((SELECT COALESCE(max(PP.ID) + 1, 1) FROM T_AU_TEAM_MEMBERS PP),
       MAX_T_ID,
       MAX_T_ID,
       TEAMNAME,
       TEAMMEMBER_ID,
       EMPLOYEENAME,
       IS_TEAMLEAD,
       (select uu.entity_id from t_user uu where uu.ppno = TEAMMEMBER_ID),
       STATUS);
    commit;
    select e.type_id
      into typeid
      from t_auditee_entities e
     where e.entity_id = entityid;
    DELETE from t_user_maping g WHERE G.PPNO = TEAMMEMBER_ID;
    COMMIT;
    insert into t_user_maping
      (userid, ppno, group_id, role_id)
    VALUES
      ((SELECT USERID FROM T_USER UU WHERE UU.PPNO = TEAMMEMBER_ID),
       (TEAMMEMBER_ID),
       0,
       0);
    commit;
    if (typeid = 9 and IS_TEAMLEAD = 'Y') then
    
      update t_user_maping m
         set m.group_id = 18, m.role_id = 18
       where m.ppno = TEAMMEMBER_ID;
      commit;
    else
      if (typeid = 9 and IS_TEAMLEAD = 'N') then
        update t_user_maping m
           set m.group_id = 28, m.role_id = 28
         where m.ppno = TEAMMEMBER_ID;
        commit;
      
      else
        if (typeid = 4 and IS_TEAMLEAD = 'Y') then
          update t_user_maping m
             set m.group_id = 10, m.role_id = 10
           where m.ppno = TEAMMEMBER_ID;
          commit;
        else
          if (typeid = 4 and IS_TEAMLEAD = 'N') then
            update t_user_maping m
               set m.group_id = 27, m.role_id = 27
             where m.ppno = TEAMMEMBER_ID;
            commit;
          else
            update t_user_maping m
               set m.group_id = 0, m.role_id = 0
             where m.ppno = TEAMMEMBER_ID;
            commit;
          end if;
        end if;
      end if;
    end if;
  
  end P_AddAuditTeam;

  PROCEDURE P_DeleteAuditTeam(TID IN NUMBER) AS
  BEGIN
    UPDATE T_AU_TEAM_MEMBERS T SET T.STATUS = 'N' WHERE T.T_ID = TID;
    COMMIT;
  END P_DeleteAuditTeam;

  procedure P_ADDAUDITCRITERIA(ENTITYTYPEID   in T_AUDIT_CRITERIA.ENTITY_TYPEID%type,
                               SIZEID         in T_AUDIT_CRITERIA.SIZE_ID%type,
                               RISKID         in T_AUDIT_CRITERIA.RISK_ID%type,
                               FREQUENCYID    in T_AUDIT_CRITERIA.FREQUENCY_ID%type,
                               NOOFDAYS       in T_AUDIT_CRITERIA.NO_OF_DAYS%type,
                               visit          in T_AUDIT_CRITERIA.VISIT%type,
                               APPROVALSTATUS in T_AUDIT_CRITERIA.APPROVAL_STATUS%type,
                               AUDITPERIODID  in T_AUDIT_CRITERIA.AUDITPERIODID%type,
                               UserEntityID   in T_AUDIT_CRITERIA.CREATED_BY%type,
                               REMARKS        in T_AUDIT_CRITERIA_LOG.REMARKS%type,
                               CREATEDBY      in T_AUDIT_CRITERIA_LOG.CREATEDBY_ID%type,
                               entityid       in number,
                               io_cursor      OUT t_cursor) is
    A_C number := 0;
    C_A number := 0;
  
  begin
    select count(a.id)
      into A_C
      from t_audit_criteria a
     where a.entity_typeid = ENTITYTYPEID
       and a.auditperiodid = AUDITPERIODID
       and a.size_id = SIZEID
       and a.risk_id = RISKID
       and entityid is null;
    select count(a.id)
      into C_A
      from t_audit_criteria a
     where a.entity_typeid = ENTITYTYPEID
       and a.auditperiodid = AUDITPERIODID
       and a.entity_id = entityid
       AND A.APPROVAL_STATUS = 1;
  
    if (A_C = 0 AND C_A = 0) then
      if (ENTITYTYPEID = 25) then
        INSERT INTO T_AUDIT_CRITERIA a
          (a.ID,
           a.ENTITY_TYPEID,
           a.entity_id,
           a.SIZE_ID,
           a.RISK_ID,
           a.FREQUENCY_ID,
           a.NO_OF_DAYS,
           a.VISIT,
           a.APPROVAL_STATUS,
           a.AUDITPERIODID,
           a.CREATED_BY,
           a.CRITERIA_SUBMITTED,
           a.no_of_entity)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA acc),
           ENTITYTYPEID,
           entityid,
           SIZEID,
           RISKID,
           FREQUENCYID,
           NOOFDAYS,
           VISIT,
           APPROVALSTATUS,
           AUDITPERIODID,
           UserEntityID,
           'N',
           1);
        commit;
        INSERT INTO T_AUDIT_CRITERIA_LOG al
          (al.ID,
           al.C_ID,
           al.STATUS_ID,
           al.CREATEDBY_ID,
           al.CREATED_ON,
           al.REMARKS)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1)
             from T_AUDIT_CRITERIA_LOG acc),
           (select max(acc1.ID) from T_AUDIT_CRITERIA acc1),
           1,
           CREATEDBY,
           SYSDATE,
           REMARKS);
        commit;
        open io_cursor for
          select m.ref, m.remarks from t_au_remarks m where m.id = 13;
      
      else
        INSERT INTO T_AUDIT_CRITERIA a
          (a.ID,
           a.ENTITY_TYPEID,
           a.SIZE_ID,
           a.RISK_ID,
           a.FREQUENCY_ID,
           a.NO_OF_DAYS,
           a.VISIT,
           a.APPROVAL_STATUS,
           a.AUDITPERIODID,
           a.CREATED_BY,
           a.CRITERIA_SUBMITTED)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA acc),
           ENTITYTYPEID,
           SIZEID,
           RISKID,
           FREQUENCYID,
           NOOFDAYS,
           VISIT,
           APPROVALSTATUS,
           AUDITPERIODID,
           UserEntityID,
           'N');
        commit;
      
        update t_audit_criteria t
           set t.no_of_entity =
               (select count(*)
                  from t_audit_criteria a
                 inner join t_au_period p
                    on a.auditperiodid = p.auditperiodid
                 inner join t_auditee_entities e
                    on a.entity_typeid = e.type_id
                  left join t_auditee_entities_risk er
                    on e.entity_id = er.entity_id
                  left join t_auditee_entities_size es
                    on e.entity_id = es.entity_id
                 inner join t_audit_frequency f
                    on a.frequency_id = f.frequency_id
                 inner join t_auditee_entities_size_disc ess
                    on ess.entity_size = es.entity_size
                 inner join t_risk_status ers
                    on ers.r_id = er.risk_rating
                 where a.auditperiodid = er.audit_period_id
                   and a.size_id = es.entity_size
                   and a.risk_id = er.risk_rating
                   and p.status_id = 2
                   and a.id = t.id)
         WHERE T.NO_OF_ENTITY IS NULL;
        commit;
      
        INSERT INTO T_AUDIT_CRITERIA_LOG al
          (al.ID,
           al.C_ID,
           al.STATUS_ID,
           al.CREATEDBY_ID,
           al.CREATED_ON,
           al.REMARKS)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1)
             from T_AUDIT_CRITERIA_LOG acc),
           (select max(acc1.ID) from T_AUDIT_CRITERIA acc1),
           1,
           CREATEDBY,
           SYSDATE,
           REMARKS);
        commit;
      
        open io_cursor for
          select m.ref, m.remarks from t_au_remarks m where m.id = 13;
      end if;
    else
      open io_cursor for
      
        select m.ref, m.remarks from t_au_remarks m where m.id = 14;
    end if;
  
  end P_ADDAUDITCRITERIA;

  procedure P_UpdateAuditCriteria(CID           IN t_audit_criteria.id%type,
                                  ENTITY_TYPEID IN T_AUDIT_CRITERIA.ENTITY_TYPEID%TYPE,
                                  SIZE_ID       IN T_AUDIT_CRITERIA.SIZE_ID%TYPE,
                                  RISK_ID       IN T_AUDIT_CRITERIA.RISK_ID%TYPE,
                                  FREQUENCY_ID  IN T_AUDIT_CRITERIA.FREQUENCY_ID%TYPE,
                                  NO_OF_DAYS    IN T_AUDIT_CRITERIA.NO_OF_DAYS%TYPE,
                                  VISIT         IN T_AUDIT_CRITERIA.VISIT%TYPE,
                                  AUDITPERIODID IN T_AUDIT_CRITERIA.AUDITPERIODID%TYPE,
                                  REMARKS       IN VARCHAR2,
                                  CREATED_BY    IN NUMBER) is
  begin
    UPDATE T_AUDIT_CRITERIA a
       SET a.ENTITY_TYPEID   = ENTITY_TYPEID,
           a.FREQUENCY_ID    = FREQUENCY_ID,
           a.NO_OF_DAYS      = NO_OF_DAYS,
           a.VISIT           = VISIT,
           a.APPROVAL_STATUS = 3
    
     WHERE a.ID = CID;
    commit;
  
    INSERT INTO T_AUDIT_CRITERIA_LOG al
      (al.ID,
       al.C_ID,
       al.STATUS_ID,
       al.CREATEDBY_ID,
       al.CREATED_ON,
       al.REMARKS,
       al.UPDATED_BY,
       al.LAST_UPDATED_ON)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA_LOG acc),
       CID,
       3,
       (SELECT MAX(acc.createdby_id)
          from T_AUDIT_CRITERIA_LOG acc
         where acc.c_id = CID),
       (SELECT max(acc.created_on)
          from T_AUDIT_CRITERIA_LOG acc
         where acc.c_id = CID),
       REMARKS,
       CREATED_BY,
       SYSDATE);
    commit;
  end P_UpdateAuditCriteria;

  procedure P_SetAuditCriteriaStatusReferredBack(CID      IN t_audit_criteria.id%type,
                                                 REMARKS  IN VARCHAR2,
                                                 PPNumber IN NUMBER) is
  
  begin
    UPDATE T_AUDIT_CRITERIA a SET a.APPROVAL_STATUS = 2 WHERE a.ID = CID;
    commit;
    IF (REMARKS IS NOT NULL) THEN
      INSERT INTO T_AUDIT_CRITERIA_LOG al
        (al.ID,
         al.C_ID,
         al.STATUS_ID,
         al.CREATEDBY_ID,
         al.CREATED_ON,
         al.REMARKS,
         al.UPDATED_BY,
         al.LAST_UPDATED_ON)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA_LOG acc),
         CID,
         2,
         (SELECT Max(acc.createdby_id)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CID),
         (SELECT max(acc.created_on)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CID),
         REMARKS,
         PPNumber,
         SYSDATE);
      commit;
    ELSE
      INSERT INTO T_AUDIT_CRITERIA_LOG al
        (al.ID,
         al.C_ID,
         al.STATUS_ID,
         al.CREATEDBY_ID,
         al.CREATED_ON,
         al.REMARKS,
         al.UPDATED_BY,
         al.LAST_UPDATED_ON)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA_LOG acc),
         CID,
         2,
         (SELECT Max(acc.createdby_id)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CID),
         (SELECT max(acc.created_on)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CID),
         'Reffered Back',
         PPNumber,
         SYSDATE);
      commit;
    END IF;
  
  end P_SetAuditCriteriaStatusReferredBack;

  procedure P_GetAuditeeEntities(ENTITYID  IN NUMBER,
                                 TYPEID    IN NUMBER,
                                 io_cursor OUT t_cursor) as
  
  begin
    if (ENTITYID is null and TYPEID is null) then
      OPEN io_cursor FOR
        Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
          FROM t_auditee_ent_types G
         order by G.ENTITYTYPEDESC;
    else
      OPEN io_cursor FOR
        Select G.ENTITYTYPEDESC AS ENTITY_TYPE, E.ENTITY_ID, E.NAME
          FROM t_auditee_entities e
         INNER JOIN t_auditee_ent_types G
            ON g.entitycode = e.type_id
         WHERE e.type_id = TYPEID
              
           and g.audited_by_enitity = ENTITYID;
    end if;
  end P_GetAuditeeEntities;

  procedure P_SubmitAuditCriteriaForApproval(CID IN NUMBER) is
  begin
    Update t_audit_criteria c
       SET c.CRITERIA_SUBMITTED = 'Y'
     where c.created_by = CID
       and c.criteria_submitted = 'N';
    COMMIT;
  end P_SubmitAuditCriteriaForApproval;

  procedure P_SetAuditCriteriaStatusApprove(CAID     IN t_audit_criteria.id%type,
                                            REMARKS  IN VARCHAR2,
                                            PPNumber IN NUMBER) is
  
  begin
    UPDATE T_AUDIT_CRITERIA a SET a.APPROVAL_STATUS = 4 WHERE a.ID = CAID;
    commit;
    if (REMARKS is not null) then
      INSERT INTO T_AUDIT_CRITERIA_LOG al
        (al.ID,
         al.C_ID,
         al.STATUS_ID,
         al.CREATEDBY_ID,
         al.CREATED_ON,
         al.REMARKS,
         al.UPDATED_BY,
         al.LAST_UPDATED_ON)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA_LOG acc),
         CAID,
         4,
         (SELECT Max(acc.createdby_id)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CAID),
         (SELECT max(acc.created_on)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CAID),
         REMARKS,
         PPNumber,
         SYSDATE);
      commit;
    else
      INSERT INTO T_AUDIT_CRITERIA_LOG al
        (al.ID,
         al.C_ID,
         al.STATUS_ID,
         al.CREATEDBY_ID,
         al.CREATED_ON,
         al.REMARKS,
         al.UPDATED_BY,
         al.LAST_UPDATED_ON)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AUDIT_CRITERIA_LOG acc),
         CAID,
         4,
         (SELECT Max(acc.createdby_id)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CAID),
         (SELECT max(acc.created_on)
            from T_AUDIT_CRITERIA_LOG acc
           where acc.C_id = CAID),
         'Approved',
         PPNumber,
         SYSDATE);
      commit;
    end if;
  end P_SetAuditCriteriaStatusApprove;

  procedure P_GetAuditCriteriaLogLastStatus(ID        IN NUMBER,
                                            io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select remarks
        from T_AUDIT_CRITERIA_LOG l
       where l.c_id = ID
       order by l.id desc FETCH NEXT 1 ROWS ONLY;
  
  end P_GetAuditCriteriaLogLastStatus;

  procedure P_GetAuditEntities(ENTITYID IN NUMBER, io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT *
        FROM t_auditee_ent_types et
       where et.auditable = 'A'
         AND ET.AUDITED_BY_ENITITY = ENTITYID;
  
  end P_GetAuditEntities;

  procedure P_GetAuditeeEntitiesForOldParas(ENTITY_ID    in number,
                                            UserEntityID in t_au_old_paras_fad.entity_id%type,
                                            io_cursor    OUT t_cursor) as
  begin
    if (ENTITY_ID != 0) then
      OPEN io_cursor FOR
        select distinct f.entity_name, '' AS entity_code , f.entity_id
          from t_Au_Observation_Old_Cad_Paras f
         where f.entity_id = ENTITY_ID
           and f.audited_by = UserEntityID
         order by entity_name;
    else
      OPEN io_cursor FOR
        select distinct f.entity_name, '' AS entity_code , f.entity_id
          from t_Au_Observation_Old_Cad_Paras f
         where f.audited_by = UserEntityID
         order by entity_name;
    
    end if;
  
  end P_GetAuditeeEntitiesForOldParas;

  procedure P_AddAuditEntity(AUDITABLE      in t_auditee_ent_types.auditable%type,
                             ENTITYTYPEDESC in t_auditee_ent_types.entitytypedesc%type) is
  begin
    INSERT INTO t_auditee_ent_types et
      (et.AUTID, et.ENTITYCODE, et.ENTITYTYPEDESC, et.AUDITABLE)
    VALUES
      ((select COALESCE(max(p.AUTID) + 1, 1) from t_auditee_ent_types p),
       LPAD((select COALESCE(max(p.AUTID) + 1, 1) from t_auditee_ent_types p),
            3,
            0),
       ENTITYTYPEDESC,
       AUDITABLE);
    commit;
  
  end P_AddAuditEntity;

  procedure P_GetAuditSubEntities(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT * FROM T_AUDITEE_ENTITEE_SUBENTITY se where se.STATUS = 'Y';
  
  end P_GetAuditSubEntities;

  procedure P_UpdateUser(USER_ID  in t_user.userid%type,
                         enc_pass in t_user.password%type,
                         role_id  in t_user_maping.role_id%type,
                         PPNO     in t_user.ppno%type,
                         ISACTIVE in t_user.isactive%type) as
    u_location varchar2(10) := 0;
  begin
    if (ppno != 0 and enc_pass != 0) then
      UPDATE t_user
         SET PASSWORD = enc_pass, ISACTIVE = ISACTIVE
       WHERE PPNO = PPNO;
      commit;
    
      update t_user_maping um
         set um.role_id = role_id, um.group_id = role_id
       WHERE um.PPNO = PPNO;
      commit;
    
      INSERT INTO t_user_maping
        (USERID, PPNO, GROUP_ID, ROLE_ID)
      VALUES
        (USER_ID, PPNO, role_id, role_id);
    else
      if (ppno != 0) then
        UPDATE t_user SET ISACTIVE = ISACTIVE WHERE PPNO = PPNO;
        commit;
      
        update t_user_maping um
           set um.role_id = role_id, um.group_id = role_id
         WHERE um.PPNO = PPNO;
        commit;
        commit;
      
        INSERT INTO t_user_maping
          (USERID, PPNO, GROUP_ID, ROLE_ID)
        VALUES
          (USER_ID, PPNO, role_id, role_id);
      
      else
        insert into t_user
          (userid, password, ppno, isactive)
        values
          ((select COALESCE(max(pp.userid) + 1, 1) from T_USER pp),
           enc_pass,
           PPNO,
           ISACTIVE);
        update t_user u
           set u.user_location_type =
               (select e.EMPLOYEETYPE
                  from v_service_employeeinfo e
                 where u.ppno = e.PPNO);
        commit;
        select u.user_location_type
          into u_location
          from t_user u
         where u.ppno = ppno;
        if (u_location = 'H') then
          update t_user u
             set u.divisionid  =
                 (select e.CURRENTDIVISIONCODE
                    from v_service_employeeinfo e
                   where u.ppno = e.PPNO
                     and u.divisionid is null),
                 u.departmentid =
                 (select e.CURRENTDEPARTMENTCODE
                    from v_service_employeeinfo e
                   where u.ppno = e.PPNO
                     and u.departmentid is null)
           where u.ppno = ppno;
          commit;
          update t_user u
             set u.entity_id =
                 (select e.entity_id
                    from t_auditee_entities e
                   where e.type_id in (3, 4, 14)
                     and u.divisionid = e.code
                      or u.departmentid = e.code)
           where u.entity_id is null;
          commit;
        else
          update t_user u
             set u.branchid =
                 (select e.CURRENTBRANCHCODE
                    from v_service_employeeinfo e
                   where u.ppno = e.PPNO
                     and u.branchid is null),
                 u.zoneid  =
                 (select e.CURRENTZONECODE
                    from v_service_employeeinfo e
                   where u.ppno = e.PPNO
                     and u.zoneid is not null);
          commit;
          update t_user u
             set u.entity_id =
                 (select e.entity_id
                    from t_auditee_entities e
                   where e.type_id in (6, 11)
                     and u.zoneid = e.code
                      or u.branchid = e.code)
           where u.entity_id is null;
        end if;
      end if;
    end if;
  end P_UpdateUser;

  procedure P_ChangePassword(PP_NO    in t_user.ppno%type,
                             enc_pass in t_user.password%type) as
  begin
  
    UPDATE t_user SET PASSWORD = enc_pass WHERE PPNO = PP_NO;
    commit;
  
  end P_ChangePassword;

  procedure P_AddGroupMenuAssignment(roleid  in T_USER_GROUP_MAP.ROLE_ID%type,
                                     menuid  in T_USER_GROUP_MAP.MENU_ID%type,
                                     pageids in T_USER_GROUP_MAP.PAGE_IDS%type) is
  begin
    INSERT INTO T_USER_GROUP_MAP
      (GROUP_MAP_ID, ROLE_ID, MENU_ID, PAGE_IDS)
    VALUES
      ((select COALESCE(max(p.GROUP_MAP_ID) + 1, 1) from T_USER_GROUP_MAP p),
       roleid,
       menuid,
       pageids);
    commit;
  
  end P_AddGroupMenuAssignment;

  procedure P_RemoveGroupMenuAssignment(roleid in T_USER_GROUP_MAP.role_id%type,
                                        menuid in T_USER_GROUP_MAP.MENU_ID%type) as
  begin
    delete from T_USER_GROUP_MAP mp
     where mp.role_id = roleid
       and mp.menu_id = menuid;
  
    commit;
  
  end P_RemoveGroupMenuAssignment;

  procedure P_GetAuditZones(ENTITYID  in t_auditee_entities.entity_id%type,
                            io_cursor OUT t_cursor) as
  
  begin
    if (ENTITYID != 0) then
    
      OPEN io_cursor FOR
        Select *
          FROM t_auditee_entities z
         WHERE z.entity_id = ENTITYID
         order by z.name asc;
    else
      OPEN io_cursor FOR
        Select *
          FROM t_auditee_entities z
         WHERE z.type_id = '9'
         order by z.name asc;
    end if;
  
  end P_GetAuditZones;

  procedure P_GetInspectionUnits(io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select z.* FROM dual z;
  
  end P_GetInspectionUnits;

  procedure P_GetBranches(Zone_Id in number, io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select b.*, z.*
        FROM v_service_branch b, v_service_zones z
       where z.ZONEID = b.ZONEID
         and z.ZONEID = Zone_Id
       order by b.BRANCHID asc;
  
  end P_GetBranches;

  procedure P_GetZones(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select z.* FROM v_service_zones z order by Z.ZONENAME asc;
  
  end P_GetZones;

  procedure P_GetBranchSizes(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select bs.*
        FROM t_auditee_entities_size_disc bs
       order by bs.ENTITY_SIZE asc;
  
  end P_GetBranchSizes;

  procedure P_GetControlViolations(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select v.*
        FROM t_r_sub_group v
       WHERE v.gr_id in (1, 3)
       order by v.S_GR_ID asc;
  end P_GetControlViolations;

  procedure P_GetEntitees(ENTITYID  IN NUMBER,
                          TYPEID    IN NUMBER,
                          io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select G.ENTITYTYPEDESC AS ENTITY_TYPE, E.ENTITY_ID, E.NAME
        FROM t_auditee_entities e
       INNER JOIN t_auditee_ent_types G
          ON G.AUDITEDBY = E.TYPE_ID
       WHERE e.type_id = TYPEID;
  end P_GetEntitees;

  procedure P_GetSubEntities(dept_code in number,
                             Div_id    in number,
                             io_cursor OUT t_cursor) as
  
  begin
    if (dept_code = 0) then
      OPEN io_cursor FOR
        Select s.*, d.NAME as DIV_NAME, dp.NAME as DEPT_NAME
          FROM T_AUDITEE_ENTITEE_SUBENTITY s
         inner join v_service_division d
            on s.Dept_Id = d.DIVISIONID
         inner join v_service_department dp
         on d.DIVISIONID = dp.DIVISIONID

         WHERE s.STATUS = 'Y'
         order by s.NAME asc;
    else
      if (dept_code != 0) then
        OPEN io_cursor FOR
          Select s.*, d.NAME as DIV_NAME, dp.NAME as DEPT_NAME
            FROM T_AUDITEE_ENTITEE_SUBENTITY s
         inner join v_service_division d
            on s.Dept_Id = d.DIVISIONID
         inner join v_service_department dp
         on d.DIVISIONID = dp.DIVISIONID
           WHERE s.STATUS = 'Y'
             and dp.code = dept_code
           order by s.NAME asc;
      else
        if (Div_id = 0) then
          OPEN io_cursor FOR
            Select s.*, d.NAME as DIV_NAME, dp.NAME as DEPT_NAME
              FROM T_AUDITEE_ENTITEE_SUBENTITY s
         inner join v_service_division d
            on s.Dept_Id = d.DIVISIONID
         inner join v_service_department dp
         on d.DIVISIONID = dp.DIVISIONID
             WHERE s.STATUS = 'Y'
             order by s.NAME asc;
        else
          OPEN io_cursor FOR
            Select s.*, d.NAME as DIV_NAME, dp.NAME as DEPT_NAME
              FROM T_AUDITEE_ENTITEE_SUBENTITY s
         inner join v_service_division d
            on s.Dept_Id = d.DIVISIONID
         inner join v_service_department dp
         on d.DIVISIONID = dp.DIVISIONID
             WHERE s.STATUS = 'Y'
               and d.divisionid = Div_id;
        end if;
      end if;
    end if;
  end P_GetSubEntities;

  procedure P_AddSubEntity(NAME   IN T_AUDITEE_ENTITEE_SUBENTITY.NAME%TYPE,
                           DIV_ID IN T_AUDITEE_ENTITEE_SUBENTITY.DEPT_ID%TYPE,
                           DEP_ID IN T_AUDITEE_ENTITEE_SUBENTITY.Parent_Enititid%type,
                           STATUS IN T_AUDITEE_ENTITEE_SUBENTITY.STATUS%type) is
  begin
    INSERT INTO T_AUDITEE_ENTITEE_SUBENTITY d
      (d.ID, d.NAME, D.DEPT_ID, d.Parent_Enititid, d.STATUS)
    VALUES
      ((SELECT COALESCE(max(PP.ID) + 1, 1)
         FROM T_AUDITEE_ENTITEE_SUBENTITY PP),
       NAME,
       DIV_ID,
       DEP_ID,
       STATUS);
  
    commit;
  
  end P_AddSubEntity;

  procedure P_GetDepartments(E_id in number, io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      select mp.parent_id,
             mp.parent_code,
             mp.entity_id,
             mp.auditedby,
             mp.status,
             (select ee.name
                from t_auditee_entities ee
               where ee.entity_id = mp.parent_id) as p_name,
             mp.child_code,
             mp.status,
             (select ee.name
                from t_auditee_entities ee
               where ee.entity_id = mp.entity_id) as c_name,
             mp.p_type_id,
             mp.c_type_id,
             mp.relation_type_id
        from t_auditee_entities_maping mp
       where mp.entity_id = E_id;
  
  end P_GetDepartments;

  procedure P_UpdateSubEntity(E_id   in number,
                              NAME   IN VARCHAR2,
                              DIV_ID IN NUMBER,
                              DEP_ID IN NUMBER,
                              STATUS IN T_AUDITEE_ENTITEE_SUBENTITY.STATUS%TYPE) as
  begin
    UPDATE T_AUDITEE_ENTITEE_SUBENTITY d
       SET d.NAME            = NAME,
           D.DEPT_ID         = DIV_ID,
           d.parent_enititid = DEP_ID,
           d.STATUS          = STATUS
     WHERE d.ID = E_id;
  end P_UpdateSubEntity;

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

  procedure P_GetAuditEmployees(dept_code IN NUMBER,
                                io_cursor OUT t_cursor) as
  
  begin
    IF (dept_code = 0) THEN
      OPEN io_cursor FOR
        select e.*
          from t_audit_emp e
         inner join t_user u
            on e.ppno = u.ppno
         order by e.RANKCODE asc;
    ELSE
      OPEN io_cursor FOR
        select e.*
          from t_audit_emp e
         inner join t_user u
            on e.ppno = u.ppno
         WHERE u.entity_id = dept_code
         order by e.RANKCODE asc;
    END IF;
  end P_GetAuditEmployees;

  procedure P_GetAuditOperationalStartDate(entityCode    IN NUMBER,
                                           auditPeriodId IN NUMBER,
                                           io_cursor     OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select EXTRACT(year FROM d.audit_enddate) as year,
             EXTRACT(month FROM d.audit_enddate) as month,
             EXTRACT(day FROM d.audit_enddate) as day
        FROM T_AUDITEE_ENTITIES_AUDIT_DATES d
       WHERE d.ENTITY_CODE = entityCode
         and d.AUDIT_PERIOD_ID = auditPeriodId;
  
  end P_GetAuditOperationalStartDate;

  procedure P_GetAuditEngagementPlans(EntityID  IN NUMBER,
                                      io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select e.eng_id,
             ee.entity_id,
             ee.name,
             e.team_name,
             e.audit_startdate,
             e.audit_enddate,
             E.OPERATION_STARTDATE AS OP_STARTDATE,
             E.OPERATION_ENDDATE   AS OP_ENDDATE
        from t_au_plan_eng e
       inner join t_auditee_entities ee
          on e.entity_id = ee.entity_id
       where e.STATUS IN (1, 2, 3, 7)
         and e.auditby_id = EntityID;
  
  end P_GetAuditEngagementPlans;

  procedure P_GetRefferedBackAuditEngagementPlans(EntityID  IN NUMBER,
                                                  io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select e.eng_id,
             e.plan_id,
             ee.entity_id,
             ee.name,
             e.team_name,
             tt.team_id,
             e.audit_startdate,
             e.audit_enddate,
             E.OPERATION_STARTDATE AS OP_STARTDATE,
             E.OPERATION_ENDDATE   AS OP_ENDDATE
        from t_au_plan_eng e
       inner join t_auditee_entities ee
          on e.entity_id = ee.entity_id
       inner join t_au_audit_teams tt
          on tt.eng_id = e.eng_id
       where e.STATUS IN (6, 9)
         and e.auditby_id = EntityID;
  
  end P_GetRefferedBackAuditEngagementPlans;

  procedure P_AddAuditEngagementPlan(PERIODID        in T_AU_PLAN_ENG.PERIOD_ID%type,
                                     ENTITYID        in T_AU_PLAN_ENG.ENTITY_ID%type,
                                     AUDIT_STARTDATE in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                     CREATEDBY       in T_AU_PLAN_ENG.CREATEDBY%type,
                                     AUDIT_ENDDATE   in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                     STATUS          in T_AU_PLAN_ENG.STATUS%type,
                                     TEAMID          in number,
                                     TEAM_NAME       in T_AU_PLAN_ENG.TEAM_NAME%type,
                                     PLANID          IN NUMBER,
                                     OP_STARTDATE    in date,
                                     OP_ENDDATE      in date,
                                     TRAVELDAY       in number,
                                     RRDAY           in number,
                                     D_Day           in number,
                                     io_cursor       OUT t_cursor) is
    E_P number := 0;
    T_I number := 0;
  begin
  
    if (OP_STARTDATE = OP_ENDDATE or OP_STARTDATE is null) then
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 25;
    else
      select nvl(max(e.plan_id), 0)
        into E_P
        from T_AU_PLAN_ENG e
       where e.plan_id = PLANID;
    
      if (E_P = 0) then
        select nvl(max(t.id), 0)
          into T_I
          from t_au_audit_team_tasklist t
         where t.team_id = TEAMID
           and ((t.audit_start_date between trunc(AUDIT_STARTDATE) and
               trunc(AUDIT_ENDDATE)) or
               (t.audit_end_date between trunc(AUDIT_STARTDATE) and
               trunc(AUDIT_ENDDATE)));
        if (T_I = 0) then
          insert into T_AU_PLAN_ENG p
            (p.ENG_ID,
             p.PERIOD_ID,
             p.ENTITY_TYPE,
             p.AUDITBY_ID,
             p.AUDIT_STARTDATE,
             p.AUDIT_ENDDATE,
             p.CREATEDBY,
             p.CREATED_ON,
             p.TEAM_NAME,
             p.STATUS,
             p.TEAM_ID,
             p.ENTITY_ID,
             p.ENTITY_CODE,
             P.PLAN_ID,
             P.operation_startdate,
             P.operation_ENDdate,
             p.TRAVEL_DAY,
             p.REVENUE_RECORD_DAY,
             p.DISCUSSION_DAY)
          
            select (SELECT COALESCE(max(PP.ENG_ID) + 1, 1)
                      FROM T_AU_PLAN_ENG PP),
                   PERIODID,
                   et.type_id,
                   et.auditby_id,
                   AUDIT_STARTDATE,
                   AUDIT_ENDDATE,
                   CREATEDBY,
                   sysdate,
                   TEAM_NAME,
                   STATUS,
                   TEAMID,
                   ENTITY_ID,
                   et.code,
                   PLANID,
                   OP_STARTDATE,
                   OP_ENDDATE,
                   TRAVELDAY,
                   RRDay,
                   D_Day
              from t_auditee_entities et
             where et.entity_id = ENTITYID;
        
          commit;
        
          insert into t_au_plan_eng_log l
            (l.ID,
             l.E_ID,
             l.STATUS_ID,
             l.CREATEDBY_ID,
             l.CREATED_ON,
             l.REMARKS)
          VALUES
            ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
             (SELECT max(lp.ENG_ID) FROM t_au_plan_eng lp),
             2,
             CREATEDBY,
             to_date(sysdate, 'dd/mm/yyyy HH:MI:SS AM'),
             'NEW ENGAGEMENT PLAN CREATED');
          commit;
        
          insert into T_AU_AUDIT_TEAMS t
            (t.ID,
             t.ENG_ID,
             t.TEAM_ID,
             t.T_NAME,
             t.T_CODE,
             t.PLACE_OF_POSTING,
             t.STATUS)
          VALUES
            ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM T_AU_AUDIT_TEAMS ll),
             (SELECT MAX(PP.ENG_ID) FROM T_AU_PLAN_ENG PP),
             TEAMID,
             TEAM_NAME,
             (SELECT distinct T_ID
                FROM T_AU_TEAM_MEMBERS
               WHERE T_ID = TEAMID),
             ENTITYID,
             1);
          COMMIT;
          UPDATE T_AU_PLAN P SET P.STATUS = 2 WHERE P.ID = PLANID;
          COMMIT;
          OPEN io_cursor FOR
            select r.ref, r.remarks from t_au_remarks r where r.id = 1;
        else
          OPEN io_cursor FOR
            select r.ref, r.remarks from t_au_remarks r where r.id = 2;
        
        end if;
      else
        OPEN io_cursor FOR
          select r.ref, r.remarks from t_au_remarks r where r.id = 3;
      
      end if;
    end if;
  end P_AddAuditEngagementPlan;

  procedure P_RerecommendAuditEngagementPlan(ENGID        in T_AU_PLAN_ENG.PERIOD_ID%type,
                                             ENTITYID     in T_AU_PLAN_ENG.ENTITY_ID%type,
                                             STARTDATE    in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                             ENDDATE      in T_AU_PLAN_ENG.AUDIT_STARTDATE%type,
                                             TEAMID       in T_AU_PLAN_ENG.TEAM_ID%type,
                                             UPDATEDBY    in T_AU_PLAN_ENG.CREATEDBY%type,
                                             PLANID       IN NUMBER,
                                             OP_STARTDATE in date,
                                             OP_ENDDATE   in date,
                                             REMARKS      in varchar2,
                                             io_cursor    OUT t_cursor) is
    E_F number := 0;
    T_I number := 0;
    V_F NUMBER := 1;
  begin
    if (OP_STARTDATE = OP_ENDDATE or OP_STARTDATE is null) then
      open io_cursor for
        select r.ref, r.remarks as remark
          from t_au_remarks r
         where r.id = 25;
    else
      DELETE FROM T_AU_AUDIT_TEAMS T WHERE T.ENG_ID = ENGID;
      COMMIT;
      DELETE FROM T_AU_AUDIT_TEAM_TASKLIST TT WHERE TT.ENG_PLAN_ID = ENGID;
      COMMIT;
      select nvl(max(e.plan_id), 0)
        into E_F
        from T_AU_PLAN_ENG e
       where e.plan_id = PLANID;
      select nvl(max(t.id), 0)
        into T_I
        from t_au_audit_team_tasklist t
       where t.team_id = TEAMID
         and ((t.audit_start_date between trunc(STARTDATE) and
             trunc(ENDDATE)) or
             (t.audit_end_date between trunc(STARTDATE) and trunc(ENDDATE)));
      if (T_I = 0) then
        UPDATE T_AU_PLAN_ENG a
           SET a.STATUS              = 3,
               a.team_id             = TEAMID,
               a.TEAM_NAME          =
               (SELECT DISTINCT (team_name)
                  from t_au_team_members m
                 where m.t_id = TEAMID),
               a.audit_startdate     = STARTDATE,
               a.audit_enddate       = ENDDATE,
               a.LASTUPDATEDBY       = UPDATEDBY,
               a.LASTUPDATEDDATE     = SYSDATE,
               A.OPERATION_STARTDATE = OP_STARTDATE,
               A.OPERATION_ENDDATE   = OP_STARTDATE
         WHERE a.ENG_ID = ENGID;
        COMMIT;
        insert into t_au_plan_eng_log
          (id,
           e_id,
           status_id,
           createdby_id,
           created_on,
           remarks,
           updated_by,
           last_updated_on,
           comments)
        
        VALUES
          ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
           ENGID,
           3,
           (select max(lp.createdby_id)
              from t_au_plan_eng_log lp
             where lp.e_id = ENGID),
           (select max(lp.created_on)
              from t_au_plan_eng_log lp
             where lp.e_id = ENGID),
           REMARKS,
           UPDATEDBY,
           SYSDATE,
           REMARKS);
        COMMIT;
      
        insert into T_AU_AUDIT_TEAMS t
          (t.ID,
           t.ENG_ID,
           t.TEAM_ID,
           t.T_NAME,
           t.T_CODE,
           t.PLACE_OF_POSTING,
           t.STATUS)
        VALUES
          ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM T_AU_AUDIT_TEAMS ll),
           ENGID,
           TEAMID,
           (SELECT distinct (team_name)
              from t_au_team_members m
             where m.t_id = TEAMID),
           TEAMID,
           ENTITYID,
           1);
        COMMIT;
        UPDATE T_AU_PLAN P SET P.STATUS = 2 WHERE P.ID = PLANID;
        COMMIT;
      
        for JJ in (SELECT * FROM t_au_team_members MT where mT.t_id = TEAMID) loop
        
          for i in 1 .. v_f loop
          
            insert into T_AU_AUDIT_TEAM_TASKLIST t
            
              (t.ID,
               t.ENG_PLAN_ID,
               t.TEAM_ID,
               t.SEQUENCE_NO,
               t.TEAMMEMBER_PPNO,
               t.ENTITY_ID,
               t.ENTITY_CODE,
               t.ENTITY_NAME,
               t.AUDIT_START_DATE,
               t.AUDIT_END_DATE,
               t.STATUS_ID,
               t.ISACTIVE,
               I)
              SELECT (SELECT COALESCE(max(ll.ID) + 1, 1)
                        FROM T_AU_AUDIT_TEAM_TASKLIST ll),
                     EE.ENG_ID,
                     TEAM_ID,
                     (SELECT COALESCE(max(ll.sequence_no) + 1, 1)
                        FROM T_AU_AUDIT_TEAM_TASKLIST ll
                       where ll.teammember_ppno = JJ.MEMBER_PPNO),
                     JJ.MEMBER_PPNO,
                     EE.ENTITY_ID,
                     EE.ENTITY_CODE,
                     (select et.name
                        from t_auditee_entities et
                       where et.entity_id = ENTITYID),
                     EE.AUDIT_STARTDATE,
                     EE.AUDIT_ENDDATE,
                     1,
                     'N',
                     i
                FROM T_AU_PLAN_ENG EE
               WHERE EE.PLAN_ID = PLANID
                 AND EE.TEAM_ID = JJ.T_ID;
            COMMIT;
          end loop;
        end loop;
      else
        open io_cursor for
          select r.ref, r.remarks as remark
            from t_au_remarks r
           where r.id = 26;
      end if;
    end if;
  end P_RerecommendAuditEngagementPlan;

  procedure P_GetLatestCommentsOnEngagement(ENGID     IN NUMBER,
                                            io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select l.remarks
        from t_au_plan_eng_log l
       where l.e_id = ENGID
       order by l.id desc FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestCommentsOnEngagement;

  procedure P_AddAuditteamtasklist(TEAMID   in number,
                                   PLANID   IN NUMBER,
                                   ENTITYID IN NUMBER) is
    V_F NUMBER := 1;
  begin
  
    for JJ in (SELECT * FROM t_au_team_members MT where mT.t_id = TEAMID) loop
    
      for i in 1 .. v_f loop
      
        insert into T_AU_AUDIT_TEAM_TASKLIST t
        
          (t.ID,
           t.ENG_PLAN_ID,
           t.TEAM_ID,
           t.SEQUENCE_NO,
           t.TEAMMEMBER_PPNO,
           t.ENTITY_ID,
           t.ENTITY_CODE,
           t.ENTITY_NAME,
           t.AUDIT_START_DATE,
           t.AUDIT_END_DATE,
           t.STATUS_ID,
           t.ISACTIVE,
           I)
        
          SELECT (SELECT COALESCE(max(ll.ID) + 1, 1)
                    FROM T_AU_AUDIT_TEAM_TASKLIST ll),
                 EE.ENG_ID,
                 TEAM_ID,
                 (SELECT COALESCE(max(ll.sequence_no) + 1, 1)
                    FROM T_AU_AUDIT_TEAM_TASKLIST ll
                   where ll.teammember_ppno = JJ.MEMBER_PPNO),
                 JJ.MEMBER_PPNO,
                 EE.ENTITY_ID,
                 EE.ENTITY_CODE,
                 (select et.name
                    from t_auditee_entities et
                   where et.entity_id = ENTITYID),
                 EE.AUDIT_STARTDATE,
                 EE.AUDIT_ENDDATE,
                 1,
                 'N',
                 i
            FROM T_AU_PLAN_ENG EE
           WHERE EE.PLAN_ID = PLANID
             AND EE.TEAM_ID = JJ.T_ID;
        COMMIT;
      end loop;
    end loop;
  
  end P_AddAuditteamtasklist;

  procedure AUDIT_TEAMS(TEAM_ID        in T_AU_AUDIT_TEAMS.TEAM_ID%type,
                        TEAM_NAME      in T_AU_AUDIT_TEAMS.T_NAME%type,
                        placeofposting in T_AU_AUDIT_TEAMS.PLACE_OF_POSTING%type) is
  begin
    insert into T_AU_AUDIT_TEAMS t
      (t.ID,
       t.ENG_ID,
       t.TEAM_ID,
       t.T_NAME,
       t.T_CODE,
       t.PLACE_OF_POSTING,
       t.STATUS)
    VALUES
      ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM T_AU_AUDIT_TEAMS ll),
       (SELECT MAX(PP.ENG_ID) FROM T_AU_PLAN_ENG PP),
       TEAM_ID,
       TEAM_NAME,
       TEAM_ID,
       placeofposting,
       1);
    commit;
  
  end AUDIT_TEAMS;

  procedure plan_eng_log(createdbyId in t_au_plan_eng_log.createdby_id%type,
                         STATUS      in t_au_plan_eng_log.status_id%type) is
  begin
    insert into t_au_plan_eng_log l
      (l.ID, l.E_ID, l.STATUS_ID, l.CREATEDBY_ID, l.CREATED_ON, l.REMARKS)
    VALUES
      ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
       (SELECT max(lp.ENG_ID) FROM t_au_plan_eng lp),
       STATUS,
       createdbyId,
       to_date(sysdate, 'dd/mm/yyyy HH:MI:SS AM'),
       'NEW ENGAGEMENT PLAN CREATED');
    commit;
  end plan_eng_log;

  procedure P_RefferedBackAuditEngagementPlan(ENGID    IN NUMBER,
                                              REMARKS  IN VARCHAR2,
                                              PPNumber IN NUMBER) as
  
  begin
    UPDATE T_AU_PLAN_ENG a SET a.STATUS = 9 WHERE a.ENG_ID = ENGID;
    UPDATE T_AU_PLAN p
       SET p.STATUS = 9
     WHERE p.entity_id = (select distinct e.entity_id
                            from t_au_plan_eng e
                           where e.eng_id = ENGID)
       and p.auditperiodid = (select distinct e.period_id
                                from t_au_plan_eng e
                               where e.eng_id = ENGID);
  
    insert into t_au_plan_eng_log l
      (l.ID, l.E_ID, l.STATUS_ID, l.CREATEDBY_ID, l.CREATED_ON, l.REMARKS)
    VALUES
      ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
       ENGID,
       9,
       PPNumber,
       to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       REMARKS);
    COMMIT;
  
  end P_RefferedBackAuditEngagementPlan;

  procedure P_ApproveAuditEngagementPlan(ENGID IN NUMBER,
                                         
                                         PPNumber IN NUMBER) as
  
  begin
  
    UPDATE T_AU_PLAN_ENG a SET a.STATUS = 4 WHERE a.ENG_ID = ENGID;
    UPDATE T_AU_PLAN p
       SET p.STATUS = 4
     WHERE p.entity_id = (select distinct e.entity_id
                            from t_au_plan_eng e
                           where e.eng_id = ENGID)
       and p.auditperiodid = (select distinct e.period_id
                                from t_au_plan_eng e
                               where e.eng_id = ENGID);
    UPDATE T_AU_AUDIT_TEAM_TASKLIST T
       SET T.ISACTIVE = 'Y'
     WHERE T.ENG_PLAN_ID = ENGID;
    COMMIT;
    UPDATE T_AU_AUDIT_TEAMS TT set TT.STATUS = '2' WHERE TT.ENG_ID = ENGID;
    COMMIT;
  
    insert into t_au_plan_eng_log l
      (l.ID, l.E_ID, l.STATUS_ID, l.CREATEDBY_ID, l.CREATED_ON, l.REMARKS)
    VALUES
      ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
       ENGID,
       2,
       PPNumber,
       to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       'Engagement Plan Approved');
    COMMIT;
  
  end P_ApproveAuditEngagementPlan;

  procedure P_GetRiskProcessDefinition(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select * from t_audit_checklist t order by t.T_ID;
  
  end P_GetRiskProcessDefinition;

  procedure P_GetRiskProcessDetails(procId    IN NUMBER,
                                    io_cursor OUT t_cursor) as
  
  begin
    if (procId = 0) THEN
    
      OPEN io_cursor FOR
        select * from t_audit_checklist t order by t.T_ID;
    ELSE
    
      OPEN io_cursor FOR
        select *
          from t_audit_checklist_sub pd
         where pd.t_id = procId
         order by pd.s_id asc;
    END IF;
  end P_GetRiskProcessDetails;

  procedure P_GetRiskProcessTransactions(procDetailId  IN NUMBER,
                                         transactionId IN NUMBER,
                                         io_cursor     OUT t_cursor) as
  
  begin
    if (procDetailId = 0) THEN
      IF (transactionId = 0) THEN
        OPEN io_cursor FOR
          select s.description  as DIV_NAME,
                 d.name         as CONTROL_OWNER,
                 pt.*,
                 pd.heading     as TITLE,
                 p.heading      as P_NAME,
                 vc.DESCRIPTION as V_NAME
            from t_audit_checklist_details pt
           inner join t_audit_checklist_sub pd
              on pt.S_ID = pd.S_ID
           inner join t_audit_checklist p
              on pd.T_ID = p.T_ID
           inner join t_r_sub_group vc
              on vc.S_GR_ID = pt.V_ID
           inner join t_hr_designations s
              on pt.role_resp_id = s.designationcode
           inner join v_service_division d
              on pt.process_owner_id = d.DIVISIONID
           order by pt.id asc;
      ELSE
        OPEN io_cursor FOR
          select s.description  as DIV_NAME,
                 d.name         as CONTROL_OWNER,
                 pt.*,
                 pd.heading     as TITLE,
                 p.heading      as P_NAME,
                 vc.DESCRIPTION as V_NAME
            from t_audit_checklist_details pt
           inner join t_audit_checklist_sub pd
              on pt.S_ID = pd.S_ID
           inner join t_audit_checklist p
              on pd.T_ID = p.T_ID
           inner join t_r_sub_group vc
              on vc.S_GR_ID = pt.V_ID
           inner join t_hr_designations s
              on pt.role_resp_id = s.designationcode
           inner join v_service_division d
              on pt.process_owner_id = d.DIVISIONID
           WHERE pt.ID = transactionId
           order by pt.id asc;
      end if;
    else
      IF (transactionId = 0) THEN
        OPEN io_cursor FOR
        
          select s.description  as DIV_NAME,
                 d.name         as CONTROL_OWNER,
                 pt.*,
                 pd.heading     as TITLE,
                 p.heading      as P_NAME,
                 vc.DESCRIPTION as V_NAME
            from t_audit_checklist_details pt
           inner join t_audit_checklist_sub pd
              on pt.S_ID = pd.S_ID
           inner join t_audit_checklist p
              on pd.T_ID = p.T_ID
           inner join t_r_sub_group vc
              on vc.S_GR_ID = pt.V_ID
           inner join t_hr_designations s
              on pt.role_resp_id = s.designationcode
           inner join v_service_division d
              on pt.process_owner_id = d.DIVISIONID
           where pt.s_id = procDetailId
           order by pt.Id asc;
      ELSE
        OPEN io_cursor FOR
        
          select s.description  as DIV_NAME,
                 d.name         as CONTROL_OWNER,
                 pt.*,
                 pd.heading     as TITLE,
                 p.heading      as P_NAME,
                 vc.DESCRIPTION as V_NAME
            from t_audit_checklist_details pt
           inner join t_audit_checklist_sub pd
              on pt.S_ID = pd.S_ID
           inner join t_audit_checklist p
              on pd.T_ID = p.T_ID
           inner join t_r_sub_group vc
              on vc.S_GR_ID = pt.V_ID
           inner join t_hr_designations s
              on pt.role_resp_id = s.designationcode
           inner join v_service_division d
              on pt.process_owner_id = d.DIVISIONID
           where pt.ID = transactionId
             and pt.s_id = procDetailId
           order by pt.Id asc;
      END IF;
    end if;
  end P_GetRiskProcessTransactions;

  procedure p_GetRiskProcessTransactionsWithStatus(statusId  IN NUMBER,
                                                   io_cursor OUT t_cursor) as
  
  begin
    if (statusId = 3) THEN
    
      OPEN io_cursor FOR
        select s.description  as DIV_NAME,
               d.name         as CONTROL_OWNER,
               pt.*,
               pd.HEADING     as TITLE,
               p.HEADING      as P_NAME,
               vc.DESCRIPTION as V_NAME,
               s.status
          from t_audit_checklist_details pt
         inner join t_audit_checklist_sub pd
            on pt.S_ID = pd.S_ID
         inner join t_audit_checklist p
            on pd.T_ID = p.T_ID
         inner join t_r_sub_group vc
            on vc.S_GR_ID = pt.V_ID
         inner join t_hr_designations s
            on pt.role_resp_id = s.designationcode
         inner join v_service_division d
            on pt.process_owner_id = d.DIVISIONID
         inner join t_audit_checklist_details_status_mapping sm
            on pt.id = sm.T_ID
         inner join t_audit_checklist_details_status s
            on s.ID = sm.STATUS_ID
           and s.ID = 3
         order by pt.id asc;
    
    ELSE
      if (statusId = 4) THEN
      
        OPEN io_cursor FOR
          select s.description  as Role_Responsible,
                 d.name         as CONTROL_OWNER,
                 pt.*,
                 pd.HEADING     as TITLE,
                 p.HEADING      as P_NAME,
                 vc.DESCRIPTION as V_NAME,
                 s.status
            from t_audit_checklist_details pt
           inner join t_audit_checklist_sub pd
              on pt.S_ID = pd.S_ID
           inner join t_audit_checklist p
              on pd.T_ID = p.T_ID
           inner join t_r_sub_group vc
              on vc.S_GR_ID = pt.V_ID
           inner join t_hr_designations s
              on pt.role_resp_id = s.designationcode
           inner join v_service_division d
              on pt.process_owner_id = d.DIVISIONID
           inner join t_audit_checklist_details_status_mapping sm
              on pt.id = sm.T_ID
           inner join t_audit_checklist_details_status s
              on s.ID = sm.STATUS_ID
             and s.ID IN (1, 4)
           order by pt.id asc;
      
      ELSE
        OPEN io_cursor FOR
          select s.description  as Role_Responsible,
                 d.name         as CONTROL_OWNER,
                 pt.*,
                 pd.HEADING     as TITLE,
                 p.HEADING      as P_NAME,
                 vc.DESCRIPTION as V_NAME,
                 s.status
            from t_audit_checklist_details pt
           inner join t_audit_checklist_sub pd
              on pt.S_ID = pd.S_ID
           inner join t_audit_checklist p
              on pd.T_ID = p.T_ID
           inner join t_r_sub_group vc
              on vc.S_GR_ID = pt.V_ID
           inner join t_hr_designations s
              on pt.role_resp_id = s.designationcode
           inner join v_service_division d
              on pt.process_owner_id = d.DIVISIONID
           inner join t_audit_checklist_details_status_mapping sm
              on pt.id = sm.T_ID
           inner join t_audit_checklist_details_status s
              on s.ID = sm.STATUS_ID
          
           order by pt.id asc;
      END IF;
    END IF;
  
  end p_GetRiskProcessTransactionsWithStatus;

  procedure P_audit_checklist(p_name  in t_audit_checklist.heading%type,
                              RISK_ID in number) is
  begin
  
    insert into t_audit_checklist p
      (p.T_ID, p.HEADING, p.ENTITY_TYPE, p.STATUS)
    VALUES
      ((select COALESCE(max(pp.T_ID) + 1, 1) from t_audit_checklist pp),
       P_NAME,
       RISK_ID,
       'Y');
    commit;
  
  end P_audit_checklist;

  procedure P_audit_checklist_sub(p_ID        in t_audit_checklist_sub.t_id%type,
                                  TITLE       in t_audit_checklist_sub.heading%type,
                                  ENTITY_TYPE in t_audit_checklist_sub.entity_type%type) is
  begin
    insert into t_audit_checklist_sub p
      (p.S_ID, p.T_ID, p.HEADING, p.ENTITY_TYPE, p.STATUS)
    VALUES
      ((select COALESCE(max(pp.S_ID) + 1, 1) from t_audit_checklist_sub pp),
       P_ID,
       TITLE,
       ENTITY_TYPE,
       'Y');
    commit;
  
  end P_audit_checklist_sub;

  procedure audit_checklist_detail(p_id           in t_audit_checklist_details.s_id%type,
                                   DESCRIPTION    in t_audit_checklist_details.heading%type,
                                   V_ID           in t_audit_checklist_details.v_id%type,
                                   CONTROL_OWNER  in t_audit_checklist_details.owner_enitity_id%type,
                                   RISK_WEIGHTAGE in t_audit_checklist_details.risk_id%type,
                                   ACTION         in t_audit_checklist_details.process_owner_id%type,
                                   PPNumber       IN NUMBER) is
  begin
    insert into t_audit_checklist_details p
      (p.ID,
       p.V_ID,
       p.S_ID,
       p.HEADING,
       p.PROCESS_OWNER_ID,
       p.ROLE_RESP_ID,
       p.RISK_ID,
       p.STATUS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details pp),
       V_ID,
       P_ID,
       DESCRIPTION,
       CONTROL_OWNER,
       ACTION,
       RISK_WEIGHTAGE,
       'Y');
    commit;
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       (select max(tp.ID) from t_audit_checklist_details tp),
       '1',
       PPNUMBER,
       'New Transaction Added');
    commit;
    insert into t_audit_checklist_details_status_mapping p
      (p.ID, p.T_ID, p.STATUS_ID)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_status_mapping pp),
       (select max(tp.ID) from t_audit_checklist_details tp),
       '1');
    commit;
  
  end audit_checklist_detail;

  procedure audit_checklist_details_log(ppnumber in t_audit_checklist_details_log.user_id%type,
                                        comments in t_audit_checklist_details_log.comments%type,
                                        t_id     in t_audit_checklist_details_log.t_id%type) is
  begin
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '3',
       PPNumber,
       COMMENTS);
    commit;
    insert into t_audit_checklist_details_status_mapping p
      (p.ID, p.T_ID, p.STATUS_ID)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_status_mapping pp),
       (select max(tp.ID) from t_audit_checklist_details tp),
       '1');
    commit;
  
  end audit_checklist_details_log;

  procedure p_Recommend_Process_Transaction_By_Reviewer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                        COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                        PPNumber IN NUMBER) is
  
  begin
    update t_audit_checklist_details_status_mapping tm
       SET tm.STATUS_ID = 3
     WHERE tm.T_ID = T_ID;
    commit;
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '3',
       PPNumber,
       COMMENTS);
    commit;
  
  end p_Recommend_Process_Transaction_By_Reviewer;

  procedure p_RefferedBack_Process_Transaction_By_Reviewer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                           COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                           PPNumber IN NUMBER) is
  
  begin
    update t_audit_checklist_details_status_mapping tm
       SET tm.STATUS_ID = 2
     WHERE tm.T_ID = T_ID;
    commit;
  
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '2',
       PPNumber,
       COMMENTS);
    commit;
  
  end p_RefferedBack_Process_Transaction_By_Reviewer;

  procedure p_RefferedBack_Process_Transaction_By_Authorizer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                             COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                             PPNumber IN NUMBER) is
  
  begin
    update t_audit_checklist_details_status_mapping tm
       SET tm.STATUS_ID = 4
     WHERE tm.T_ID = T_ID;
    commit;
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '4',
       PPNumber,
       COMMENTS);
    commit;
  
  end p_RefferedBack_Process_Transaction_By_Authorizer;

  procedure p_Recommend_Process_Transaction_By_Authorizer(T_ID     in t_audit_checklist_details_log.t_id%type,
                                                          COMMENTS IN t_audit_checklist_details_log.comments%TYPE,
                                                          PPNumber IN NUMBER) is
  begin
    update t_audit_checklist_details_status_mapping tm
       SET tm.STATUS_ID = 5
     WHERE tm.T_ID = T_ID;
    commit;
  
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '5',
       PPNumber,
       COMMENTS);
    commit;
  
  end p_Recommend_Process_Transaction_By_Authorizer;

  procedure p_GetAuditFrequencies(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select * from T_AUDIT_FREQUENCY F WHERE F.STATUS = 'Y' order by F.ID;
  
  end p_GetAuditFrequencies;

  procedure audit_checklist_details_status_mapping is
  begin
    insert into t_audit_checklist_details_status_mapping p
      (p.ID, p.T_ID, p.STATUS_ID)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_status_mapping pp),
       (select max(tp.ID) from t_audit_checklist_details tp),
       '1');
    commit;
  
  end audit_checklist_details_status_mapping;

  procedure P_GetRisks(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select * from T_RISK R order by R.R_ID;
  
  end P_GetRisks;

  procedure P_GetPendingAuditCriterias(UserEntityID IN NUMBER,
                                       io_cursor    OUT t_cursor) is
  
  begin
    --select  from t_auditee_entities e where e.
    OPEN io_Cursor FOR
      select AC.ID,
             AC.ENTITY_TYPEID,
             AC.SIZE_ID,
             AC.RISK_ID,
             AC.FREQUENCY_ID,
             AC.NO_OF_DAYS,
             AC.VISIT,
             AC.APPROVAL_STATUS,
             AC.AUDITPERIODID,
             AC.NO_OF_ENTITY,
             AC.CREATED_BY,
             AC.CRITERIA_SUBMITTED,
             p.DESCRIPTION as PERIOD,
             et.entitytypedesc as ENTITY,
             r.description as RISK,
             f.frequency_discription as FREQUENCY,
             s.description as BRSIZE,
             ac.entity_id,
             (select ee.name
                from t_auditee_entities ee
               where ee.entity_id = ac.entity_id) as name,
             
             MAX(L.REMARKS) AS REMARKS
        from t_audit_criteria ac
       inner join t_au_period p
          on ac.auditperiodid = p.auditperiodid
       inner join t_auditee_ent_types et
          on ac.entity_typeid = et.autid
         and et.auditable = 'A'
       inner join t_risk r
          on ac.risk_id = r.r_id
       inner join t_audit_frequency f
          on ac.frequency_id = f.frequency_id
        left join t_auditee_entities_size_disc s
          on ac.size_id = s.entity_size
        LEFT JOIN T_AUDIT_CRITERIA_LOG L
          ON L.C_ID = AC.ID
       WHERE ac.CRITERIA_SUBMITTED = 'N'
         and ac.CREATED_BY = UserEntityID
       GROUP BY AC.ID,
                AC.ENTITY_TYPEID,
                AC.SIZE_ID,
                AC.RISK_ID,
                AC.FREQUENCY_ID,
                AC.NO_OF_DAYS,
                AC.VISIT,
                AC.APPROVAL_STATUS,
                AC.AUDITPERIODID,
                AC.NO_OF_ENTITY,
                AC.CREATED_BY,
                AC.CRITERIA_SUBMITTED,
                p.DESCRIPTION,
                et.entitytypedesc,
                r.description,
                f.frequency_discription,
                s.description,
                ac.entity_id;
  end P_GetPendingAuditCriterias;

  procedure P_GetRefferedBackAuditCriterias(UserEntityID IN NUMBER,
                                            io_cursor    OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select AC.ID,
             AC.ENTITY_TYPEID,
             AC.SIZE_ID,
             AC.RISK_ID,
             AC.FREQUENCY_ID,
             AC.NO_OF_DAYS,
             AC.VISIT,
             AC.APPROVAL_STATUS,
             AC.AUDITPERIODID,
             AC.NO_OF_ENTITY,
             AC.CREATED_BY,
             AC.CRITERIA_SUBMITTED,
             p.DESCRIPTION as PERIOD,
             et.entitytypedesc as ENTITY,
             r.description as RISK,
             f.frequency_discription as FREQUENCY,
             s.description as BRSIZE,
             ac.entity_id,
             (select ee.name
                from t_auditee_entities ee
               where ee.entity_id = ac.entity_id) as name,
             MAX(L.REMARKS) AS REMARKS
        from t_audit_criteria ac
       inner join t_au_period p
          on ac.auditperiodid = p.auditperiodid
       inner join t_auditee_ent_types et
          on ac.entity_typeid = et.autid
         and et.auditable = 'A'
       inner join t_risk r
          on ac.risk_id = r.r_id
       inner join t_audit_frequency f
          on ac.frequency_id = f.frequency_id
        left join t_auditee_entities_size_disc s
          on ac.size_id = s.entity_size
        LEFT JOIN T_AUDIT_CRITERIA_LOG L
          ON L.C_ID = AC.ID
       WHERE ac.APPROVAL_STATUS = 2
         and ac.CRITERIA_SUBMITTED = 'Y'
         and ac.CREATED_BY = UserEntityID
       GROUP BY AC.ID,
                AC.ENTITY_TYPEID,
                AC.SIZE_ID,
                AC.RISK_ID,
                AC.FREQUENCY_ID,
                AC.NO_OF_DAYS,
                AC.VISIT,
                AC.APPROVAL_STATUS,
                AC.AUDITPERIODID,
                AC.NO_OF_ENTITY,
                AC.CREATED_BY,
                AC.CRITERIA_SUBMITTED,
                p.DESCRIPTION,
                et.entitytypedesc,
                r.description,
                f.frequency_discription,
                s.description,
                ac.entity_id;
  
  end P_GetRefferedBackAuditCriterias;

  procedure P_GetAuditCriteriasToAuthorize(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select AC.ID,
             AC.ENTITY_TYPEID,
             AC.SIZE_ID,
             AC.RISK_ID,
             AC.FREQUENCY_ID,
             AC.NO_OF_DAYS,
             AC.VISIT,
             AC.APPROVAL_STATUS,
             AC.AUDITPERIODID,
             AC.NO_OF_ENTITY,
             AC.CREATED_BY,
             AC.CRITERIA_SUBMITTED,
             p.DESCRIPTION as PERIOD,
             et.entitytypedesc as ENTITY,
             r.description as RISK,
             f.frequency_discription as FREQUENCY,
             s.description as BRSIZE,
             ac.entity_id,
             (select ee.name
                from t_auditee_entities ee
               where ee.entity_id = ac.entity_id) as name,
             MAX(L.REMARKS) AS REMARKS
        from t_audit_criteria ac
       inner join t_au_period p
          on ac.auditperiodid = p.auditperiodid
       inner join t_auditee_ent_types et
          on ac.entity_typeid = et.autid
         and et.auditable = 'A'
       inner join t_risk r
          on ac.risk_id = r.r_id
       inner join t_audit_frequency f
          on ac.frequency_id = f.frequency_id
        left join t_auditee_entities_size_disc s
          on ac.size_id = s.entity_size
        LEFT JOIN T_AUDIT_CRITERIA_LOG L
          ON L.C_ID = AC.ID
       WHERE ac.APPROVAL_STATUS IN (1, 3, 6)
         and ac.CRITERIA_SUBMITTED = 'Y'
       GROUP BY AC.ID,
                AC.ENTITY_TYPEID,
                AC.SIZE_ID,
                AC.RISK_ID,
                AC.FREQUENCY_ID,
                AC.NO_OF_DAYS,
                AC.VISIT,
                AC.APPROVAL_STATUS,
                AC.AUDITPERIODID,
                AC.NO_OF_ENTITY,
                AC.CREATED_BY,
                AC.CRITERIA_SUBMITTED,
                p.DESCRIPTION,
                et.entitytypedesc,
                r.description,
                f.frequency_discription,
                s.description,
                ac.entity_id
       order by AC.ENTITY_TYPEID, AC.RISK_ID, AC.SIZE_ID;
  end P_GetAuditCriteriasToAuthorize;

  procedure P_GetPostChangesAuditCriterias(userentityid in number,
                                           io_cursor    OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select AC.ID,
             AC.ENTITY_TYPEID,
             AC.SIZE_ID,
             AC.RISK_ID,
             AC.FREQUENCY_ID,
             AC.NO_OF_DAYS,
             AC.VISIT,
             AC.APPROVAL_STATUS,
             AC.AUDITPERIODID,
             AC.NO_OF_ENTITY,
             AC.CREATED_BY,
             AC.CRITERIA_SUBMITTED,
             p.DESCRIPTION as PERIOD,
             et.entitytypedesc as ENTITY,
             r.description as RISK,
             f.frequency_discription as FREQUENCY,
             s.description as BRSIZE,
             ac.entity_id,
             (select ee.name
                from t_auditee_entities ee
               where ee.entity_id = ac.entity_id) as name,
             MAX(L.REMARKS) AS REMARKS
        from t_audit_criteria ac
       inner join t_au_period p
          on ac.auditperiodid = p.auditperiodid
       inner join t_auditee_ent_types et
          on ac.entity_typeid = et.autid
         and et.auditable = 'A'
       inner join t_risk r
          on ac.risk_id = r.r_id
       inner join t_audit_frequency f
          on ac.frequency_id = f.frequency_id
        left join t_auditee_entities_size_disc s
          on ac.size_id = s.entity_size
        LEFT JOIN T_AUDIT_CRITERIA_LOG L
          ON L.C_ID = AC.ID
       WHERE ac.APPROVAL_STATUS IN (4)
         and ac.CRITERIA_SUBMITTED = 'Y'
         and ac.created_by = userentityid
       GROUP BY AC.ID,
                AC.ENTITY_TYPEID,
                AC.SIZE_ID,
                AC.RISK_ID,
                AC.FREQUENCY_ID,
                AC.NO_OF_DAYS,
                AC.VISIT,
                AC.APPROVAL_STATUS,
                AC.AUDITPERIODID,
                AC.NO_OF_ENTITY,
                AC.CREATED_BY,
                AC.CRITERIA_SUBMITTED,
                p.DESCRIPTION,
                et.entitytypedesc,
                r.description,
                f.frequency_discription,
                s.description,
                ac.entity_id;
  
  end P_GetPostChangesAuditCriterias;

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

  procedure P_GetTaskList(PPNumber in number, io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select t.*,
             (select tt.type_id
                from t_auditee_entities tt
               where tt.entity_id = t.entity_id) as ENTITY_TYPE,
             (select ss.description
                from T_AU_AUDIT_TEAM_TASKLIST_STATUS ss
               where ss.status_id = (t.status_id + 1)) as ENG_NEXT_STATUS,
             ta.T_NAME,
             ts.DESCRIPTION as ENG_STATUS,
             p.description as audit_year,
             e.operation_startdate,
             e.operation_enddate
        from T_AU_AUDIT_TEAM_TASKLIST t
       inner join T_AU_AUDIT_TEAMS ta
          on t.TEAM_ID = ta.TEAM_ID
         and t.eng_plan_id = ta.eng_id
       inner join T_AU_AUDIT_TEAM_TASKLIST_STATUS ts
          on t.STATUS_ID = ts.STATUS_ID
       inner join t_au_plan_eng e
          on e.eng_id = t.eng_plan_id
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       WHERE t.teammember_ppno = PPNumber
       order by T.AUDIT_START_DATE asc;
  
  end P_GetTaskList;

  procedure P_GetJoiningDetails(engId    in number,
                                PPNumber in number,
                                
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
       where t.eng_plan_id = engId
            
         and audt.eng_id = engId
         and tm.member_ppno = PPNumber;
  
  end P_GetJoiningDetails;

  procedure P_AddJoiningReport(ENGID           in number,
                               PPNO            in number,
                               COMPLETION_DATE in date) is
  
    T_F number := 0;
    V_F NUMBER := 0;
    A_F NUMBER := 0;
  
    C_F date;
  begin
    SELECT COUNT(M.T_ID)
      INTO A_F
      FROM T_AU_TEAM_MEMBERS M
     WHERE M.MEMBER_PPNO = PPNO
       AND M.ISTEAMLEAD = 'Y';
    Update t_au_audit_joining ji
       SET ji.STATUS = 'P'
     where Ji.Team_Mem_Ppno = ppno
       and ji.eng_plan_id != ENGID;
    select e.audit_enddate
      into C_F
      from t_au_plan_eng e
     where e.eng_id = ENGID;
    commit;
    Update t_au_plan_eng e SET e.STATUS = 5 where e.eng_id = ENGID;
    COMMIT;
    select e.entity_type
      into T_F
      from t_au_plan_eng e
     WHERE E.ENG_ID = ENGID;
    SELECT nvl(max(j.id), 0)
      INTO V_F
      FROM T_AU_AUDIT_JOINING j
     WHERE j.ENG_PLAN_ID = ENGID
       and j.TEAM_MEM_PPNO = PPNO
       and j.STATUS = 'I';
    if (V_F = 0 AND T_F = 6) then
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
         PPNO,
         trunc(sysdate),
         PPNO,
         trunc(SYSDATE),
         C_F,
         'I');
      COMMIT;
      UPDATE T_AU_AUDIT_TEAM_TASKLIST t
         SET t.STATUS_ID =
             (select COALESCE(acc.STATUS_ID + 1, 1)
                from T_AU_AUDIT_TEAM_TASKLIST acc
               WHERE acc.ENG_PLAN_ID = ENGID
                 and acc.TEAMMEMBER_PPNO = PPNO)
       WHERE t.ENG_PLAN_ID = ENGID
         and t.TEAMMEMBER_PPNO = PPNO;
      COMMIT;
      IF (A_F != 0) THEN
        FOR NM IN (SELECT * FROM T_AUDIT_CHECKLIST_DETAILS) LOOP
        
          insert into t_auditee_checkklist
            (id, eng_id, checklist_id, ENTEREDBY, ENTEREDON, STATUS)
            select (select COALESCE(MAX(acc.ID) + 1, 1)
                      from t_auditee_checkklist acc),
                   ENGID,
                   d.id,
                   PPNO,
                   TRUNC(SYSDATE),
                   1
              from t_audit_checklist_details d
             WHERE D.ID = NM.ID;
          commit;
        
        END LOOP;
      END IF;
    ELSE
      if (V_F = 0 AND T_F != 6) then
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
           PPNO,
           sysdate,
           PPNO,
           SYSDATE,
           C_F,
           'I');
        COMMIT;
        UPDATE T_AU_AUDIT_TEAM_TASKLIST t
           SET t.STATUS_ID =
               (select COALESCE(acc.STATUS_ID + 1, 1)
                  from T_AU_AUDIT_TEAM_TASKLIST acc
                 WHERE acc.ENG_PLAN_ID = ENGID
                   and acc.TEAMMEMBER_PPNO = PPNO)
         WHERE t.ENG_PLAN_ID = ENGID
           and t.TEAMMEMBER_PPNO = PPNO;
        COMMIT;
      
      else
        UPDATE T_AU_AUDIT_TEAM_TASKLIST t
           SET t.STATUS_ID =
               (select COALESCE(acc.STATUS_ID + 1, 1)
                  from T_AU_AUDIT_TEAM_TASKLIST acc
                 WHERE acc.ENG_PLAN_ID = ENGID
                   and acc.TEAMMEMBER_PPNO = PPNO)
         WHERE t.ENG_PLAN_ID = ENGID
           and t.TEAMMEMBER_PPNO = PPNO;
        COMMIT;
      end if;
    end if;
  
  end p_AddJoiningReport;

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

  procedure p_GetAuditChecklistSub(tid in number, io_cursor OUT t_cursor) is
  
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

  procedure P_GetAuditChecklistDetails(sid in number,
                                       
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

  procedure P_SaveAuditObservationCAD(PLANID            in T_AU_OBSERVATION.ENGPLANID%type,
                                      STATUS            in T_AU_OBSERVATION.STATUS%type,
                                      REPLYDATE         in T_AU_OBSERVATION.REPLYDATE%type,
                                      ENTEREDBY         in T_AU_OBSERVATION.Enteredby%type,
                                      Severity          in T_AU_OBSERVATION.Severity%type,
                                      SUBCHECKLISTID    in T_AU_OBSERVATION.Subchecklist_Id%type,
                                      CHECKLISTDETAILID in T_AU_OBSERVATION.Checklistdetail_Id%type,
                                      TEXT_DATA         in T_AU_OBSERVATION_TEXT.TEXT%type,
                                      BRANCHID          IN NUMBER,
                                      io_cursor         OUT t_cursor) is
    V_F NUMBER := 0;
    R_F NUMBER := 0;
    E_F NUMBER := 0;
    S_F NUMBER := 0;
    M_F NUMBER := 0;
    T_F NUMBER := 0;
    b_F number := 0;
  begin
  
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
       and e.type_id = 6;
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
         o.ENTITY_CODE)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_OBSERVATION acc),
         PLANID,
         STATUS,
         ENTEREDBY,
         sysdate,
         B_F,
         to_date(REPLYDATE, 'dd/mm/yyyy HH:MI:SS AM'),
         S_F,
         V_F,
         R_F,
         SUBCHECKLISTID,
         CHECKLISTDETAILID,
         0,
         0,
         BRANCHID);
      commit;
      INSERT INTO T_AU_OBSERVATION_TEXT
        (ID,
         OBSERVATSION_ID,
         TEXT,
         ENTEREDBY,
         ENTEREDDATE,
         ENG_PLAN,
         MEMO_NUMBER)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_TEXT acc),
         (select max(o.ID) from T_AU_OBSERVATION o),
         TEXT_DATA,
         ENTEREDBY,
         SYSDATE,
         PLANID,
         M_F);
      commit;
    
      Open io_cursor FOR
        SELECT (select max(acc.ID) from T_AU_OBSERVATION acc) AS ID,
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
         o.V_CAT_NATURE_ID)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_OBSERVATION acc),
         PLANID,
         STATUS,
         ENTEREDBY,
         sysdate,
         E_F,
         to_date(REPLYDATE, 'dd/mm/yyyy HH:MI:SS AM'),
         S_F,
         
         V_F,
         R_F,
         SUBCHECKLISTID,
         CHECKLISTDETAILID,
         0,
         0);
      commit;
      INSERT INTO T_AU_OBSERVATION_TEXT
        (ID,
         OBSERVATSION_ID,
         TEXT,
         ENTEREDBY,
         ENTEREDDATE,
         ENG_PLAN,
         MEMO_NUMBER)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_TEXT acc),
         (select max(o.ID) from T_AU_OBSERVATION o),
         TEXT_DATA,
         ENTEREDBY,
         SYSDATE,
         PLANID,
         M_F);
      commit;
    
      Open io_cursor FOR
        SELECT (select max(acc.ID) from T_AU_OBSERVATION acc) AS ID,
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
                                   io_cursor         OUT t_cursor) is
    V_F NUMBER := 0;
    R_F NUMBER := 0;
    E_F NUMBER := 0;
    S_F NUMBER := 0;
    M_F NUMBER := 0;
    O_F NUMBER := 0;
    T_F NUMBER := 0;
  begin
  
    SELECT NVL(MAX(o.memo_number), 0)
      INTO O_F
      FROM T_AU_OBSERVATION O
     WHERE O.ENGPLANID = PLANID
       AND O.CHECKLISTDETAIL_ID = CHECKLISTDETAILID;
    select COALESCE(max(ac.MEMO_NUMBER) + 1, 1)
      INTO M_F
      from T_AU_OBSERVATION ac
     WHERE AC.ENGPLANID = PLANID;
    if (o_f = 0) then
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
      IF (T_F = 6) THEN
        select cd.role_resp_id
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
      END IF;
      IF (S_F > 0) THEN
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
           o.NO_OF_INSTANCES)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_OBSERVATION acc),
           PLANID,
           STATUS,
           ENTEREDBY,
           sysdate,
           E_F,
           trunc(REPLYDATE),
           S_F,
           V_F,
           R_F,
           SUBCHECKLISTID,
           CHECKLISTDETAILID,
           0,
           0,
           NOINSTANCES);
        commit;
        INSERT INTO T_AU_OBSERVATION_TEXT
          (ID, OBSERVATSION_ID, TEXT, ENTEREDBY, ENTEREDDATE, ENG_PLAN)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1)
             from T_AU_OBSERVATION_TEXT acc),
           (select max(o.ID) from T_AU_OBSERVATION o),
           TEXT_DATA,
           ENTEREDBY,
           SYSDATE,
           PLANID);
        commit;
        update t_auditee_checkklist t
           set t.status = 2, action = 'Y'
         where t.eng_id = PLANID
           and t.checklist_id = CHECKLISTDETAILID;
        commit;
      
        Open io_cursor FOR
          SELECT (select max(acc.ID) from T_AU_OBSERVATION acc) AS ID,
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
           o.NO_OF_INSTANCES)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_OBSERVATION acc),
           PLANID,
           STATUS,
           ENTEREDBY,
           sysdate,
           E_F,
           trunc(REPLYDATE),
           Severity,
           
           0,
           0,
           0,
           0,
           VCATID,
           VCATNATUREID,
           NOINSTANCES);
        commit;
        INSERT INTO T_AU_OBSERVATION_TEXT
          (ID,
           OBSERVATSION_ID,
           TEXT,
           ENTEREDBY,
           ENTEREDDATE,
           ENG_PLAN,
           MEMO_NUMBER)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1)
             from T_AU_OBSERVATION_TEXT acc),
           (select max(o.ID) from T_AU_OBSERVATION o),
           TEXT_DATA,
           ENTEREDBY,
           SYSDATE,
           PLANID,
           M_F);
        commit;
      
        Open io_cursor FOR
          SELECT (select max(acc.ID) from T_AU_OBSERVATION acc) AS ID,
                 r.remarks,
                 r.ref
            from t_au_remarks r
           where r.id = 15;
      
      END IF;
    else
      Open io_cursor FOR
        SELECT O_F as id, r.remarks, r.ref
          from t_au_remarks r
         where r.id = 16;
    end if;
  
  end P_SaveAuditObservation;

  procedure P_DropAuditObservation(OBS_ID    IN NUMBER,
                                   pp_no     in number,
                                   io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select m.role_id into V_F from t_user_maping m where m.ppno = pp_no;
    if (V_F = 18 or V_F = 10 or V_F = 4) then
      UPDATE t_au_observation SET STATUS = 23 WHERE ID = OBS_ID;
      COMMIT;
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 21;
    else
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 22;
    end if;
  
  end P_DropAuditObservation;

  procedure P_SubmitAuditObservationToAuditee(OBS_ID    IN NUMBER,
                                              pp_no     in number,
                                              io_cursor OUT t_cursor) is
  
    M_F number := 0;
    P_F number := 0;
    A_F number := 0;
    T_L varchar(5) := 'N';
  
  begin
  
    select nvl(max(tm.isteamlead), 'N')
      into T_L
      from t_au_observation op
     inner join t_au_audit_team_tasklist tl
        on op.engplanid = tl.eng_plan_id
     inner join t_au_team_members tm
        on tl.team_id = tm.t_id
     where tl.teammember_ppno = PP_NO
       and op.id = OBS_ID;
  
    select NVL((o.engplanid), 0)
      into P_F
      from t_au_observation o
     where o.id = OBS_ID;
    --select m.role_id into V_F from t_user_maping m where m.ppno = pp_no;
    select COALESCE(max(ac.MEMO_NUMBER) + 1, 1)
      INTO M_F
      from T_AU_OBSERVATION ac
     WHERE ac.engplanid = P_F;
    select NVL(max(s.parent_enititid), 0)
      into A_F
      from T_AUDITEE_ENTITEE_SUBENTITY s
     inner join t_au_observation o
        on o.entity_id = s.enitity_id
     where o.id = OBS_ID;
    if (T_L = 'Y') then
      if (A_F = 0) then
        INSERT INTO T_AU_OBSERVATION_ASSIGNEDTO ot
          (ot.ID,
           ot.OBS_ID,
           ot.OBS_TEXT_ID,
           ot.entity_id,
           ot.ASSIGNEDBY,
           ot.ASSIGNED_DATE,
           ot.IS_ACTIVE,
           ot.REPLIED)
          SELECT (select COALESCE(max(acc.ID) + 1, 1)
                    from T_AU_OBSERVATION_ASSIGNEDTO acc),
                 TT.OBSERVATSION_ID,
                 TT.ID,
                 O.ENTITY_ID,
                 O.ENTEREDBY,
                 trunc(sysdate),
                 'Y',
                 'N'
            FROM T_AU_OBSERVATION O
           INNER JOIN T_AU_OBSERVATION_TEXT TT
              ON TT.OBSERVATSION_ID = O.ID
           WHERE O.ID = OBS_ID;
        commit;
      else
        INSERT INTO T_AU_OBSERVATION_ASSIGNEDTO ot
          (ot.ID,
           ot.OBS_ID,
           ot.OBS_TEXT_ID,
           ot.entity_id,
           ot.ASSIGNEDBY,
           ot.ASSIGNED_DATE,
           ot.IS_ACTIVE,
           ot.REPLIED)
          SELECT (select COALESCE(max(acc.ID) + 1, 1)
                    from T_AU_OBSERVATION_ASSIGNEDTO acc),
                 TT.OBSERVATSION_ID,
                 TT.ID,
                 A_F,
                 O.ENTEREDBY,
                 trunc(sysdate),
                 'Y',
                 'N'
            FROM T_AU_OBSERVATION O
           INNER JOIN T_AU_OBSERVATION_TEXT TT
              ON TT.OBSERVATSION_ID = O.ID
           WHERE O.ID = OBS_ID;
        commit;
      end if;
      update t_au_observation t
         set t.memo_number = M_F,
             t.memo_date   = trunc(sysdate),
             t.status      = 2
       where t.id = OBS_ID
         and t.engplanid = P_F;
    
      commit;
      update t_au_observation_text t
         set t.memo_number = M_F
       where t.id = OBS_ID;
      commit;
    
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 8;
    else
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 22;
    end if;
  
  end P_SubmitAuditObservationToAuditee;

  procedure P_UpdateAuditObservationStatus(OBS_ID        IN NUMBER,
                                           NEW_STATUS_ID IN NUMBER,
                                           Remarks       IN VARCHAR2,
                                           PP_NO         IN NUMBER,
                                           io_cursor     OUT t_cursor) is
  
    S_Z number := 0;
    T_L varchar(5) := 'N';
  begin
    select nvl(max(mp.role_id), 0)
      into S_Z
      from t_user_maping mp
     where mp.ppno = PP_NO;
    select nvl(max(tm.isteamlead), 'N')
      into T_L
      from t_au_observation op
     inner join t_au_audit_team_tasklist tl
        on op.engplanid = tl.eng_plan_id
     inner join t_au_team_members tm
        on tl.team_id = tm.t_id
     where tl.teammember_ppno = PP_NO
       and op.id = OBS_ID;
    if (T_L = 'Y' or S_Z = 15) then
      UPDATE T_AU_OBSERVATIONS_AUDITEE_RESPONSE e
         SET e.REMARKS         = Remarks,
             E.LASTUPDATEDBY   = PP_NO,
             E.LASTUPDATEDDATE = TRUNC(SYSDATE)
       WHERE e.AU_OBS_ID = OBS_ID;
      COMMIT;
      UPDATE T_AU_OBSERVATION o
         SET o.status = NEW_STATUS_ID
       WHERE o.id = OBS_ID;
      COMMIT;
      open io_cursor for
        select '1' as ref, r.statusname as remarks
          from t_au_observation_status r
         where r.statusid = NEW_STATUS_ID;
    else
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 22;
    end if;
  
  end P_UpdateAuditObservationStatus;

  PROCEDURE P_UPDATEtauditeecheckklist(PLANID            IN NUMBER,
                                       CHECKLISTDETAILID IN NUMBER) IS
  BEGIN
    update t_auditee_checkklist c
       set c.status = 2
     where c.checklist_id = CHECKLISTDETAILID
       and c.eng_id = PLANID;
    commit;
  END P_UPDATEtauditeecheckklist;

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

  procedure p_GetAssignedObservations(ENTID     in number,
                                      ENGID     in number,
                                      io_cursor OUT t_cursor) is
    V_F number := 0;
  
  begin
    select e.type_id
      into V_F
      from t_auditee_entities e
     where e.entity_id = ENTID;
  
    if (V_F in (6, 25)) then
      OPEN io_Cursor FOR
        select o.engplanid,
               null                   as Violation,
               null                   AS NATURE,
               ch.heading             as Process,
               csb.heading            as Sub_process,
               cd.heading             as Check_List_Details,
               O.MEMO_NUMBER          AS MEMO_NUMBER,
               o.Memo_Date,
               o.replydate,
               t.id,
               t.obs_id,
               t.obs_text_id,
               t.entity_id,
               t.assignedby,
               t.assigned_date,
               t.lastupdatedby,
               t.lastupdateddate,
               t.is_active,
               t.replied,
               s.statusname           as STATUS,
               o.STATUS               as STATUS_ID,
               ep.audit_startdate     as AUDIT_STARTDATE,
               ep.audit_enddate       as AUDIT_ENDDATE,
               ep.operation_startdate,
               ep.operation_enddate,
               pe.description         as Audit_year,
               ob.id                  as resp_id
          from t_au_observation_assignedto t
         inner join t_au_observation o
            on o.id = t.obs_id
         inner join t_au_observation_status s
            on o.status = s.statusid
         inner join t_au_plan_eng ep
            on ep.eng_id = o.engplanid
         inner join t_au_period pe
            on pe.auditperiodid = ep.period_id
         inner join t_audit_checklist_sub csb
            on csb.s_id = o.subchecklist_id
         inner join t_audit_checklist_details cd
            on cd.id = o.checklistdetail_id
         inner join t_audit_checklist ch
            on ch.t_id = csb.t_id
          LEFT join t_au_observations_auditee_response ob
            on ob.au_obs_id = o.id
         WHERE o.entity_id = ENTID
           and ep.eng_id = engid
         order by t.OBS_ID asc;
    else
      OPEN io_Cursor FOR
        select o.engplanid,
               vc.v_name              as Violation,
               vcs.sub_v_name         AS NATURE,
               Null                   as Process,
               Null                   as Sub_process,
               Null                   as Check_List_Details,
               O.MEMO_NUMBER          AS MEMO_NUMBER,
               o.Memo_Date,
               o.replydate,
               t.id,
               t.obs_id,
               t.obs_text_id,
               t.entity_id,
               t.assignedby,
               t.assigned_date,
               t.lastupdatedby,
               t.lastupdateddate,
               t.is_active,
               t.replied,
               s.statusname           as STATUS,
               o.STATUS               as STATUS_ID,
               ep.audit_startdate     as AUDIT_STARTDATE,
               ep.audit_enddate       as AUDIT_ENDDATE,
               ep.operation_startdate,
               ep.operation_enddate,
               pe.description         as Audit_year,
               ob.id                  as resp_id
          from t_au_observation_assignedto t
         inner join t_au_observation o
            on o.id = t.obs_id
         inner join t_au_observation_status s
            on o.status = s.statusid
         inner join t_au_plan_eng ep
            on ep.eng_id = o.engplanid
         inner join t_control_violation vc
            on o.v_cat_id = vc.id
         inner join t_control_violation_sub vcs
            on o.v_cat_nature_id = vcs.id
         inner join t_au_period pe
            on pe.auditperiodid = ep.period_id
          LEFT join t_au_observations_auditee_response ob
            on ob.au_obs_id = o.id
        
         WHERE o.entity_id = ENTID
           and ep.eng_id = engid
         order by t.OBS_ID asc;
    end if;
  
  end p_GetAssignedObservations;

  procedure p_GetAssignedObservationstext(OBSID     in number,
                                          io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select o.engplanid,
             vc.v_name      as Violation,
             vcs.sub_v_name AS NATURE,
             ch.heading     as Process,
             csb.heading    as Sub_process,
             cd.heading     as Check_List_Details,
             O.MEMO_NUMBER  AS MEMO_NUMBER,
             o.Memo_Date,
             ot.text        as OBSERVATION_TEXT,
             ot.headings  as OBSERVATION_TEXT_PLAIN,
             ob.reply       as replytext,
             ob.id          as resp_id
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_text ot
          on ot.id = t.obs_text_id
        left join t_control_violation vc
          on o.v_cat_id = vc.id
        left join t_control_violation_sub vcs
          on o.v_cat_nature_id = vcs.id
        left join t_audit_checklist_sub csb
          on csb.s_id = o.subchecklist_id
        left join t_audit_checklist_details cd
          on cd.id = o.checklistdetail_id
        left join t_audit_checklist ch
          on ch.t_id = csb.t_id
        LEFT join t_au_observations_auditee_response ob
          on ob.au_obs_id = o.id
       WHERE o.id = OBSID
      
       order by t.OBS_ID asc;
  
  end p_GetAssignedObservationstext;

  procedure P_GetAssignedObservationsForBranch(entityid  in number,
                                               io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select vc.v_name              as Violation,
             vcs.sub_v_name         AS NATURE,
             o.Memo_Date,
             o.replydate,
             t.*,
             ot.text                as OBSERVATION_TEXT,
             ot.headings          as OBSERVATION_TEXT_PLAIN,
             ob.reply               as replytext,
             s.statusname           as STATUS,
             o.STATUS               as STATUS_ID,
             e.name                 AS ENTITY_NAME,
             ep.audit_startdate     as AUDIT_STARTDATE,
             ep.audit_enddate       as AUDIT_ENDDATE,
             pe.description         as Audit_year,
             ob.id                  as resp_id,
             ep.operation_startdate,
             ep.operation_enddate
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_text ot
          on ot.id = t.obs_text_id
       inner join t_au_observation_status s
          on o.status = s.statusid
       inner join t_auditee_entities e
          on e.entity_id = t.ENTITY_ID
       inner join t_control_violation vc
          on o.v_cat_id = vc.id
       inner join t_control_violation_sub vcs
          on o.v_cat_nature_id = vcs.id
       inner join t_au_plan_eng ep
          on ep.entity_id = e.entity_id
       inner join t_au_period pe
          on pe.auditperiodid = ep.period_id
        left join t_au_observations_auditee_response ob
          on ob.au_obs_id = o.id
      
       WHERE o.entity_id = entityid
       order by t.OBS_ID asc;
  
  end P_GetAssignedObservationsForBranch;

  procedure P_GetObservationText(OBS_ID in number, io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select ot.text
        from T_AU_OBSERVATION_TEXT ot
       where ot.OBSERVATSION_ID = OBS_ID;
  
  end P_GetObservationText;

  procedure P_UpdateObservation(OBS_ID       in number,
                                obtext       in clob,
                                subprocessid in number,
                                checklistid  in number,
                                ppno         in number,
                                io_cursor    OUT t_cursor) is
  
  begin
  
    if (subprocessid = 0) then
      update T_AU_OBSERVATION_TEXT ot
         set ot.text            = obtext,
             ot.lastupdateddate = sysdate,
             ot.lastupdatedby   = ppno
       where ot.OBSERVATSION_ID = OBS_ID;
      commit;
    else
      update t_au_observation o
         set o.subchecklist_id    = subprocessid,
             o.checklistdetail_id = checklistid,
             o.severity          =
             (select d.risk_id
                from t_audit_checklist_details d
               where d.id = checklistid)
       where o.id = OBS_ID;
      commit;
      update T_AU_OBSERVATION_TEXT ot
         set ot.text            = obtext,
             ot.lastupdateddate = sysdate,
             ot.lastupdatedby   = ppno
       where ot.OBSERVATSION_ID = OBS_ID;
      commit;
    
    end if;
    open io_cursor for
      select r.ref, r.remarks from t_au_remarks r where r.id = 29;
  
  end P_UpdateObservation;

  procedure P_GetObservationResponsible(OBSID     in number,
                                        io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select ot.pp_no,
             em.EMPLOYEEFIRSTNAME || '  ' || em.EMPLOYEELASTNAME as EMP_NAME,
             ot.LOAN_CASE as LOANCASE,
             ot.lc_amount as LCAMOUNT,
             ot.account_number as ACCNUMBER,
             ot.ac_amount as ACAMOUNT
        from T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED ot
       inner join v_service_employeeinfo em
          on em.PPNO = ot.pp_no
       where ot.obs_id = OBSID;
  
  end P_GetObservationResponsible;

  procedure P_GetOBSERVATIONSAUDITEERESPONSE(OBS_ID    in number,
                                             io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select ot.REPLY
        from T_AU_OBSERVATIONS_AUDITEE_RESPONSE ot
       where ot.au_obs_id = OBS_ID;
  
  end P_GetOBSERVATIONSAUDITEERESPONSE;

  procedure P_AUDITEE_OBSERVATION_RESPONSE(AUOBSID   IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.AU_OBS_ID%type,
                                           REPLYDATA IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REPLY%type,
                                           REPLIEDBY IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REPLIEDBY%type,
                                           OBSTEXTID IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.OBS_TEXT_ID%type,
                                           REPLYROLE IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REPLY_ROLE%type,
                                           REMARKS   IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.REMARKS%type,
                                           SUBMITTED IN T_AU_OBSERVATIONS_AUDITEE_RESPONSE.SUBMITTED%type,
                                           io_cursor OUT t_cursor) is
  begin
    INSERT INTO T_AU_OBSERVATIONS_AUDITEE_RESPONSE o
      (o.ID,
       o.AU_OBS_ID,
       o.REPLY,
       o.REPLIEDBY,
       o.REPLIEDDATE,
       o.OBS_TEXT_ID,
       o.REPLY_ROLE,
       o.REMARKS,
       o.SUBMITTED)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_AU_OBSERVATIONS_AUDITEE_RESPONSE acc),
       AUOBSID,
       REPLYDATA,
       REPLIEDBY,
       trunc(SYSDATE),
       OBSTEXTID,
       REPLYROLE,
       REMARKS,
       SUBMITTED);
  
    commit;
  
    UPDATE T_AU_OBSERVATION_ASSIGNEDTO
       SET REPLIED = 'Y'
     WHERE OBS_ID = AUOBSID
       and OBS_TEXT_ID = OBSTEXTID;
    commit;
    UPDATE t_au_observation T
       SET STATUS          = 3,
           T.LASTREPLYBY   = REPLIEDBY,
           MEMO_REPLY_DATE = trunc(SYSDATE)
     WHERE ID = AUOBSID;
  
    open io_cursor for
      select acc.au_obs_id as ob_id, r.id as resp_id
        from T_AU_OBSERVATIONS_AUDITEE_RESPONSE acc
       inner join T_AU_OBSERVATIONS_AUDITEE_RESPONSE r
          on r.au_obs_id = acc.au_obs_id
       where acc.au_obs_id = AUOBSID;
  
    commit;
  
  end P_AUDITEE_OBSERVATION_RESPONSE;

  procedure P_AUDITEE_OBSERVATION_RESPONSE_evidences(respid    in t_au_observations_auditee_evidences.respid%type,
                                                     AUOBSID   IN t_au_observations_auditee_evidences.memoid%type,
                                                     filename  IN t_au_observations_auditee_evidences.file_name%type,
                                                     filetype  IN t_au_observations_auditee_evidences.file_type%type,
                                                     length    in t_au_observations_auditee_evidences.length%type,
                                                     enteredby IN t_au_observations_auditee_evidences.enteredby%type,
                                                     filedata  IN t_au_observations_auditee_evidences.file_data%type,
                                                     sequence  IN t_au_observations_auditee_evidences.sequence%type,
                                                     text_id   in t_au_observations_auditee_evidences.text_id%type) is
  begin
  
    insert into t_au_observations_auditee_evidences
      (id,
       file_name,
       file_type,
       length,
       file_data,
       memoid,
       enteredby,
       entereddate,
       sequence,
       status,
       text_id,
       respid)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from t_au_observations_auditee_evidences acc),
       filename,
       filetype,
       length,
       filedata,
       AUOBSID,
       enteredby,
       trunc(SYSDATE),
       sequence,
       'Y',
       text_id,
       respid);
    commit;
  
  end P_AUDITEE_OBSERVATION_RESPONSE_evidences;

  procedure P_get_AUDITEE_OBSERVATION_RESPONSE_evidences(resp_id   in t_au_observations_auditee_evidences.respid%type,
                                                         io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select *
        from t_au_observations_auditee_evidences e
       where e.respid = resp_id
       order by e.sequence;
  
  end P_get_AUDITEE_OBSERVATION_RESPONSE_evidences;

  procedure P_GetLoggedInUserEngId(PPNumber  IN NUMBER,
                                   io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select j.eng_plan_id
        from t_au_audit_joining j
       where j.team_mem_ppno = PPNumber
         and j.status = 'I';
  
  end P_GetLoggedInUserEngId;

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

  procedure P_GetLatestAuditorResponse(obs_id    IN NUMBER,
                                       io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select r.status, r.recommendation
        from T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION r
       where r.au_obs_id = obs_id
      --and r.reco_role IN ('Team Lead', 'Team Member')
       order by r.id desc FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestAuditorResponse;

  procedure P_GetLatestDepartmentalHeadResponse(obs_id    IN NUMBER,
                                                io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select r.audit_reply
        from t_au_observations_auditor_reply r
       where r.au_obs_id = obs_id
      --and r.reply_role IN ('Departmental Head / Incharge AZ')
       order by r.id desc FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestDepartmentalHeadResponse;

  procedure P_GetRiskDescByID(risk_id IN NUMBER, io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select r.DESCRIPTION from T_RISK r where r.R_ID = risk_id;
  
  end P_GetRiskDescByID;

  procedure P_GetLatestCommentsOnProcess(procId    IN NUMBER,
                                         io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select l.comments
        from t_audit_checklist_details_log l
       where l.t_id = procId
       order by l.created_on desc FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestCommentsOnProcess;

  procedure P_GetLatestAuditeeResponse(obs_id    IN NUMBER,
                                       io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select r.reply
        from t_au_observations_auditee_response r
       where r.au_obs_id = obs_id
       order by r.id desc FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestAuditeeResponse;

  procedure P_GetManagedObservations(ENGID     IN NUMBER,
                                     OBSID     IN NUMBER,
                                     io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select c.v_name as Violation,
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
       Where o.engplanid = ENGID
       order by o.memo_number;
  
  end P_GetManagedObservations;

  procedure P_GetManagedObservationstext(OBSID     IN NUMBER,
                                         io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select c.v_name          as Violation,
             csb.sub_v_name    AS NATURE,
             o.memo_number     as MEMO_NO,
             o.no_of_instances as noinstances,
             ot.text           as OBS_TEXT,
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
                                                io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select c.heading as Process,
             cc.heading as Sub_process,
             csb.heading AS Check_List_Detail,
             p.description as PERIOD,
             o.ID as OBS_ID,
             nvl(o.no_of_instances, 1) as noinstances,
             aee.name as ENTITY_NAME,
             nvl(o.memo_number, 0) as MEMO_NO,
             o.severity as OBS_RISK_ID,
             r.description as OBS_RISK,
             o.status as OBS_STATUS_ID,
             ost.Statusname as OBS_STATUS
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_auditee_entities aee
          on e.entity_id = aee.entity_id
       inner join t_audit_checklist_sub cc
          on cc.s_id = o.subchecklist_id
       inner join t_audit_checklist_details csb
          on csb.id = o.checklistdetail_id
       inner join t_audit_checklist c
          on c.t_id = cc.t_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_status ost
          on o.status = ost.statusid
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       Where o.engplanid = ENGID
         --and o.status in (1, 2)
       order by o.memo_number;
  
  end P_GetManagedObservationsForBranches;

  procedure P_GetManagedObservationsForBranchesTEXT(OBSID     IN NUMBER,
                                                    io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select c.heading     as Process,
             cc.heading    as Sub_process,
             csb.heading   AS Check_List_Detail,
             o.memo_number as MEMO_NO,
             ot.text       as OBS_TEXT
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_audit_checklist_sub cc
          on cc.s_id = o.subchecklist_id
       inner join t_audit_checklist_details csb
          on csb.id = o.checklistdetail_id
       inner join t_audit_checklist c
          on c.t_id = cc.t_id
       Where O.ID = OBSID
       order by o.memo_number;
  end P_GetManagedObservationsForBranchesTEXT;

  procedure P_GetManagedDraftObservations(ENGID     IN NUMBER,
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
    if (E_F in (6, 25)) then
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
           inner join t_audit_checklist_sub csb
              on csb.s_id = o.subchecklist_id
           inner join t_audit_checklist_details cd
              on cd.id = o.checklistdetail_id
           inner join t_audit_checklist ch
              on ch.t_id = csb.t_id
            left join t_au_observations_auditor_response ar
              on ar.au_obs_id = o.id
           where o.engplanid = ENGID
             and o.status not in (1, 2, 7, 23)
           order by o.memo_number;
      end if;
    else
      if (E_F not in (6, 25)) then
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
         inner join t_audit_checklist_sub csb
            on csb.s_id = o.subchecklist_id
         inner join t_audit_checklist_details cd
            on cd.id = o.checklistdetail_id
         inner join t_audit_checklist ch
            on ch.t_id = csb.t_id
          left join t_au_observations_auditor_response ar
            on ar.au_obs_id = o.id
         where o.engplanid = ENGID
           and o.status not in (1, 2, 7, 23)
         order by o.memo_number;
    end if;
  end P_GetManagedDraftObservationsbranch;

  procedure P_GetFinalizedDraftObservations(ENGID     IN NUMBER,
                                            io_cursor OUT t_cursor) is
    V_F number := 0;
    O_F number := 0;
    M_F number := 0;
  begin
    select nvl(max(ob.id), 0)
      into O_F
      from t_au_observation ob
     where ob.engplanid = engid
       and ob.status in (5);
    select nvl(min(ob.id), 0)
      into M_F
      from t_au_observation ob
     where ob.engplanid = engid;
  
    select e.entity_type
      into V_F
      from t_au_plan_eng e
     where e.eng_id = ENGID;
    if (V_F in (6, 25)) then
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
           inner join t_audit_checklist_sub csb
              on csb.s_id = o.subchecklist_id
           inner join t_audit_checklist_details cd
              on cd.id = o.checklistdetail_id
           inner join t_audit_checklist ch
              on ch.t_id = csb.t_id
            left join t_au_observations_auditor_response ar
              on ar.au_obs_id = o.id
           where o.engplanid = ENGID
             and o.status not in (1, 2, 7, 23)
           order by o.memo_number;
      end if;
    else
      if (V_F not in (6, 25)) then
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
  end P_GetFinalizedDraftObservations;

  procedure P_GetFinalizedDraftObservationsbranch(ENGID     IN NUMBER,
                                                  io_cursor OUT t_cursor) is
    O_F number := 0;
    M_F number := 0;
  begin
    select nvl(max(ob.id), 0)
      into O_F
      from t_au_observation ob
     where ob.engplanid = engid
       and ob.status > 4;
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
         inner join t_audit_checklist_sub csb
            on csb.s_id = o.subchecklist_id
         inner join t_audit_checklist_details cd
            on cd.id = o.checklistdetail_id
         inner join t_audit_checklist ch
            on ch.t_id = csb.t_id
          left join t_au_observations_auditor_response ar
            on ar.au_obs_id = o.id
         where o.engplanid = ENGID
           and o.status not in (1, 2, 7, 23)
         order by o.memo_number;
    end if;
  end P_GetFinalizedDraftObservationsbranch;

  procedure P_GetManagedDraftObservationsText(OBSID     IN NUMBER,
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
                                                     io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select o.engplanid    as eng_id,
             c.heading      as Process,
             cc.heading     as Sub_process,
             csb.heading    AS Check_List_Detail,
             p.description  as PERIOD,
             o.ID           as OBS_ID,
             aee.name       as ENTITY_NAME,
             o.memo_number  as MEMO_NO,
             o.severity     as OBS_RISK_ID,
             r.description  as OBS_RISK,
             ar.reply       as Aud_reply,
             o.status       as OBS_STATUS_ID,
             ost.Statusname as OBS_STATUS
        from t_au_observation o
       inner join t_au_plan_eng e
          on o.engplanid = e.eng_id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_auditee_entities aee
          on e.entity_id = aee.entity_id
       inner join t_audit_checklist_sub cc
          on cc.s_id = o.subchecklist_id
       inner join t_audit_checklist_details csb
          on csb.id = o.checklistdetail_id
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

  procedure P_GetViolationObservations(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select * from v_Dash_Borad_of_Divisional_Head;
  
  end P_GetViolationObservations;

  procedure p_GetClosingDraftObservations(PP_NO     in number,
                                          io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select min(ts.eng_plan_id)
      into V_F
      from t_au_audit_team_tasklist ts
     where ts.teammember_ppno = PP_NO
       and ts.status_id between '2' and '6';
    OPEN io_Cursor FOR
      select (select e.name
                from t_auditee_entities e
               inner join t_au_plan_eng ep
                  on ep.entity_id = e.entity_id
               where ep.eng_id = V_F) as entity_name,
             (select ej.joining_date
                from t_au_audit_joining ej
               where ej.eng_plan_id = V_F
                 and t.member_ppno = ej.team_mem_ppno) as joining_date,
             (select ep.audit_enddate
                from t_au_plan_eng ep
               where ep.eng_id = V_F) as completion_date,
             t.member_ppno,
             t.Enteredby as member_name,
             t.no_of_Ob as total_no_ob,
             (select m.isteamlead
                from v_getclosingdraft_teammember_summary m
               where m.eng_id = V_F
                 and m.member_name = t.Enteredby) as teamlead,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = V_F
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 2) as Submitted_to_Auditee,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = V_F
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 3) as Responded,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = V_F
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 4) as Resolved,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = V_F
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 5) as Added_to_Draft,
             (select NVL(max(tt.ob), 0)
                from V_GETCLOSINGDRAFT_STATUS_SUMMARY tt
               where tt.engplanid = V_F
                 and tt.ppno = t.member_ppno
                 and tt.statusid = 23) as dropped
        from V_GETCLOSINGDRAFT_TEAM_SUMMARY t
      
       where t.engplanid = V_F
       order by teamlead desc;
  
  end p_GetClosingDraftObservations;

  procedure P_Closeaudit(engid     in number,
                         PP_NO     in number,
                         io_cursor OUT t_cursor) is
    C_F number := 0;
    E_F number := 0;
  begin
    select nvl(min(t.eng_plan_id), 0)
      into E_F
      from t_au_audit_team_tasklist t
     where t.teammember_ppno = PP_NO
       and t.status_id < '5';
    select nvl(max(o.id), 0)
      into C_F
      from t_au_observation o
     where o.status in (1)
       and o.engplanid = E_F;
    if (E_F != 0) then
      if (C_F != 0) then
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 19;
      else
        update t_au_audit_joining ji
           set ji.status          = 'C',
               ji.lastupdatedby   = PP_NO,
               ji.lastupdateddate = trunc(sysdate)
         where ji.eng_plan_id = E_F;
        commit;
      
        update t_au_audit_team_tasklist t
           set t.isactive = 'N', t.status_id = '5'
        
         where t.eng_plan_id = E_F;
        commit;
      
        update t_au_plan_eng e set e.status = 5 where e.eng_id = E_F;
        commit;
      
        insert into t_au_plan_eng_log
          (id, e_id, status_id, createdby_id, created_on, remarks)
        VALUES
          ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
           E_F,
           5,
           PP_NO,
           sysdate,
           'Completed');
        commit;
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 20;
      
      end if;
    else
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 20;
    end if;
  
  end P_Closeaudit;
/*
  procedure Closing(engid       in t_Au_Plan_Eng.Eng_Id%type,
                    entity_type in t_Au_Plan_Eng.Entity_Type%type,
                    io_cursor   OUT t_cursor) as
    V_F number := 0;
    C_F number := 0;
  begin
  
    select nvl(max(o.id), 0)
      into C_F
      from t_au_observation o
     where o.status in (1, 2, 3, 6)
       and o.engplanid = engid;
    if (C_F != 0) then
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 19;
    else
    
      if (entity_type not in ('6', '25')) then
        insert into T_COSO_RATING_DEPARTMENT
          (AUDIT_PERIOD,
           DEPT_Name,
           ENTITY_ID,
           RATING_FACTORS,
           SUB_FACTORS,
           MAX_SCORE,
           WEIGHT_ASSIGNED,
           NO_OF_OBSERVATIONS,
           WEIGHTED_AVERAGE_SCORE,
           STATUS)
          select p.description,
                 a.description,
                 e.entity_id,
                 c.v_name,
                 count(o.id),
                 (count(o.id) * 3),
                 c.max_number,
                 sum(o.severity),
                 round((sum(o.severity) / ((count(o.id) * 3))) *
                       (c.max_number)) as Average_Score,
                 'Y'
            from t_au_period                 p,
                 t_au_plan_eng               e,
                 t_au_observation            o,
                 t_au_observation_assignedto t,
                 t_Control_Violation         c,
                 t_auditee_entities          a,
                 t_Au_Audit_Joining          j
           where p.auditperiodid = e.period_id
             and e.eng_id = o.engplanid
             and o.id = t.obs_id
             and c.id = o.v_cat_id
             and a.entity_id = e.entity_code
             and a.type_id in (4, 14)
             and j.eng_plan_id = e.eng_id
             and j.status = 'C'
             and o.status = 8
             and e.eng_id = engid
           group by p.description, a.description, c.v_name, c.max_number;
        commit;
        insert into t_auditee_entities_risk
          (r_id,
           audit_period_id,
           entity_code,
           risk_rating,
           entity_id,
           marks)
          select (select COALESCE(max(p.r_ID) + 1, 1)
                    from T_auditee_entities_risk p),
                 ee.period_id,
                 ee.entity_code,
                 ff.final_rating,
                 ff.entity_id,
                 ff.final_score
            from t_Coso_Rating_Department_Final ff
           inner join t_au_plan_eng ee
              on ee.entity_id = ff.entity_id
           where ee.eng_id = engid;
        commit;
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 20;
      else
        if entity_type = '6' then
        
          INSERT INTO T_RISK_BRANCH_WISE
            (AUDIT_PERIOD,
             ENITITY_CODE,
             GR_ID,
             S_GR_ID,
             MAX_NUMBER,
             WEIGHTAGE_AVERAGE,
             NUMBER_OF_OBSERVATIONS)
            SELECT P.AUDITPERIODID,
                   E.ENTITY_CODE,
                   r.gr_id,
                   rs.gr_id,
                   r.max_number,
                   rs.weightage as Weighted_Average,
                   count(o.memo_number) as para
            
              FROM T_AU_PERIOD P
             INNER JOIN T_AU_PLAN_ENG E
                ON P.AUDITPERIODID = E.PERIOD_ID
             INNER JOIN T_AUDIT_PARA O
                ON E.ENG_ID = O.ENGPLANID
             INNER JOIN T_AUDIT_CHECKLIST_DETAILS D
                ON O.CHECKLISTDETAIL_ID = d.id
             INNER JOIN T_R_SUB_GROUP RS
                ON RS.S_GR_ID = D.V_ID
             INNER JOIN T_R_GROUP R
                ON R.GR_ID = RS.GR_ID
             where e.eng_id = 1
             GROUP BY P.AUDITPERIODID,
                      E.ENTITY_CODE,
                      r.gr_id,
                      rs.gr_id,
                      r.max_number,
                      rs.weightage;
          commit;
          update T_RISK_BRANCH_WISE t
             set t.risk_based_marks = round(t.number_of_observations *
                                            t.risk_based_marks)
           where t.entity_id = (select ep.entity_id
                                  from t_au_plan_eng ep
                                 where ep.eng_id = engid);
          commit;
          insert into t_branch_risk_rating
            (audit_period_id, branch_code, risk_category)
            select ww.audit_period,
                   ww.enitity_code,
                   sum(ww.weighted_average_marks)
              from t_risk_branch_wise ww
             inner join t_au_plan_eng eg
                on ww.entity_id = eg.entity_id
             where eg.eng_id = engid
             group by ww.audit_period, ww.enitity_code;
          update t_branch_risk_rating rr
             set rr.risk_category =
                 (select cr.rating
                    from t_coso_rating cr
                   where rr.risk_rating between cr.range_start and
                         cr.range_end);
          commit;
          insert into t_auditee_entities_risk
            (r_id, audit_period_id, entity_code, entity_id, marks)
            select (select COALESCE(max(p.r_ID) + 1, 1)
                      from T_auditee_entities_risk p),
                   (ee.period_id + 1),
                   ee.entity_code,
                   bb.entity_id,
                   bb.weighted_average_marks
              from T_RISK_BRANCH_WISE bb
             inner join t_au_plan_eng ee
                on ee.entity_id = bb.entity_id
             where ee.eng_id = engid;
          commit;
          open io_cursor for
            select r.ref, r.remarks from t_au_remarks r where r.id = 20;
        
          insert into T_AUDIT_PARA p
            select *
              from t_au_observation o
             where o.engplanid = engid
               and o.status = '8';
          commit;
        
          update t_au_audit_joining j
             set j.status = 'C'
           where j.eng_plan_id = engid;
          commit;
        
          update T_AU_AUDIT_TEAM_TASKLIST t
             set t.status_id = 6
           where t.eng_plan_id = engid;
          commit;
        end if;
      
      end if;
    end if;
  end Closing;
*/
  procedure p_Getauditteams(ENGID in number, io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select distinct (tm.team_name),
                      tm.member_ppno,
                      tm.member_name,
                      tm.isteamlead
        from t_au_audit_teams m
       inner join t_au_team_members tm
          on tm.t_id = m.team_id
       where m.eng_id = ENGID
         and m.status = 'Y'
       order by tm.team_name asc;
  
  end p_Getauditteams;

  procedure P_GetClosingDraftTeamDetails(ENGID     in number,
                                         io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select jo.*, tm.isteamlead, tm.member_name
        from t_au_audit_joining jo
       inner join t_au_team_members tm
          on tm.member_ppno = jo.team_mem_ppno
       inner join t_au_audit_teams aut
          on tm.t_id = aut.team_id
       where jo.eng_plan_id = ENGID
         and jo.eng_plan_id = aut.eng_id;
  
  end P_GetClosingDraftTeamDetails;

  procedure P_CloseDraftAuditReport(ENGID     IN NUMBER,
                                    io_cursor OUT t_cursor) is
  
    V_F number := 0;
  begin
    select count(nvl(o.id, 0))
      into V_F
      from t_au_observation o
     where o.engplanid = ENGID
       and o.status = 1;
    if (V_F > 0) then
      open io_cursor for
        select * from t_au_remarks r where r.id = 27;
    else
      UPDATE t_au_audit_joining set STATUS = 'P' WHERE ENG_PLAN_ID = ENGID;
      COMMIT;
    
      UPDATE t_au_audit_team_tasklist
         set STATUS_ID = 5
       WHERE ENG_PLAN_ID = ENGID;
      commit;
      open io_cursor for
        select * from t_au_remarks r where r.id = 28;
    end if;
  
  end P_CloseDraftAuditReport;

  procedure P_DeletePendingCriteria(CID IN NUMBER) is
  begin
    Delete FROM t_audit_criteria c where c.id = cid;
    COMMIT;
  
  end P_DeletePendingCriteria;

  procedure p_GetCOSORisks(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select * from T_COSO_RISK R order by R.R_ID;
  end p_GetCOSORisks;

  procedure P_GetCOSORiskForDepartment(PERIOD_ID    in number,
                                       UserEntityID in number,
                                       io_cursor    OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select cr.audit_period,
             cr.dept_name,
             cr.coso_base_rating_factors as RATING_FACTORS,
             cr.number_of_sub_factors as SUB_FACTORS,
             cr.maximum_score_of_sub_factors as MAX_SCORE,
             cr.overall_weight_assigned as WEIGHT_ASSIGNED,
             cr.score_assigned_by_auditor as NO_OF_OBSERVATIONS,
             cr.weighted_average_score as WEIGHTED_AVERAGE_SCORE,
             t.final_score as FINAL_SCORE,
             cr.audit_rating,
             cr.status,
             cr.entity_id,  t.final_rating as final_audit_rating
        from T_COSO_RATING_DEPARTMENT_INHERITED_RISK cr
       inner join T_COSO_RATING_DEPARTMENT_FINAL t
          on cr.dept_name = t.dept
       inner join t_au_period p
          on cr.audit_period = p.description
       inner join t_auditee_entities e
          on e.name = cr.dept_name
       where cr.audit_period = t.audit_period
         and e.auditby_id = UserEntityID
         and p.auditperiodid = PERIOD_ID
                  and t.final_score is not null
       order by cr.DEPT_NAME ASC;
  
  end P_GetCOSORiskForDepartment;

  procedure CAU_OM(OM_NO       IN T_CAU_OM.OM_NO%type,
                   ENCODED_MSG IN T_CAU_OM.CONTENTS_OF_OM%type,
                   DIV_ID      IN T_CAU_OM.DIV_ID%type) is
  
  begin
  
    INSERT INTO T_CAU_OM
      (ID, OM_NO, CONTENTS_OF_OM, DIV_ID, STATUS)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1) from T_CAU_OM acc),
       OM_NO,
       ENCODED_MSG,
       DIV_ID,
       2);
  
    commit;
  
  end CAU_OM;

  procedure P_CAUGetAssignedOMs(UserEntityID in number,
                                io_cursor    OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select s.*, ts.DISCRIPTION
        from t_cau_om s
       inner join t_cau_status ts
          on s.STATUS = ts.ID
       where s.div_id = UserEntityID
       order by s.ID;
  
  end P_CAUGetAssignedOMs;

  procedure P_GetCCQ(UserEntityID in number, io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select c.*,
             e.name        as ENTITY_NAME,
             r.description as RISK_DEF,
             v.v_name      as VIOLATION_NAME
        from t_au_ccq c
       inner join t_auditee_entities e
          on e.entity_id = c.entity_id
        left join t_coso_risk r
          on r.r_id = c.risk_id
        left join t_control_violation v
          on v.id = c.control_violation_id
       where e.auditby_id = UserEntityID
       order by e.name;
  
  end P_GetCCQ;

  procedure P_UpdateCCQ(CID                  IN NUMBER,
                        QUESTIONS            in varchar2,
                        CONTROL_VIOLATION_ID in number,
                        RISK_ID              in number,
                        STATUS               in varchar2,
                        PPNumber             in number) is
    risk_rating number := 0;
  begin
    select rr.rating
      into risk_rating
      from t_risk rr
     where rr.r_id = RISK_ID;
    update t_au_ccq c
       SET c.QUESTIONS            = QUESTIONS,
           c.CONTROL_VIOLATION_ID = CONTROL_VIOLATION_ID,
           c.RISK_ID              = RISK_ID,
           c.RISK_RATING          = risk_rating,
           c.STATUS               = STATUS,
           c.UPDATED_BY           = PPNumber,
           c.UPDATED_DATETIME     = sysdate
    
     WHERE c.ID = CID;
    COMMIT;
  end P_UpdateCCQ;

  procedure P_UpdateENTITIEES(ID          IN NUMBER,
                              CODE        IN NUMBER,
                              NAME        IN VARCHAR2,
                              DISCRIPTION IN VARCHAR2,
                              AUDITEDBY   IN NUMBER,
                              INSPECTEDBY IN NUMBER,
                              TYPEID      IN NUMBER,
                              ENTITYID    IN NUMBER,
                              STATUS      IN CHAR,
                              AUDITABLE   IN VARCHAR2,
                              PARENTID    in number,
                              CHILDID     in number) is
  begin
    update T_AUDITEE_ENTITIES C
       SET --C.  = ID,
           C.CODE           = CODE,
           C.DESCRIPTION    = DISCRIPTION,
           C.NAME           = NAME,
           C.ACTIVE         = STATUS,
           C.TYPE_ID        = TYPEID,
           C.AUDITBY_ID     = AUDITEDBY,
           C.INSPECTEDBY_ID = INSPECTEDBY,
           C.AUDITABLE      = AUDITABLE
     WHERE C.ENTITY_ID = ENTITYID;
    COMMIT;
    update t_auditee_entities_maping m
       set m.parent_id        = parentid,
           m.parent_code     =
           (select e.code
              from t_auditee_entities e
             where e.entity_id = parentid),
           m.child_code      =
           (select e.code
              from t_auditee_entities e
             where e.entity_id = childid),
           m.entity_id        = ENTITYID,
           m.auditedby        = AUDITEDBY,
           m.status           = STATUS,
           m.p_type_id       =
           (select e.type_id
              from t_auditee_entities e
             where e.entity_id = parentid),
           m.c_type_id       =
           (select e.type_id
              from t_auditee_entities e
             where e.entity_id = childid),
           m.relation_type_id =
           (select e.type_id
              from t_auditee_entities e
             where e.entity_id = parentid)
     WHERE m.ENTITY_ID = ENTITYID;
    COMMIT;
  
  end P_UpdateENTITIEES;

  procedure P_InsertENTITIEES(ID          IN NUMBER,
                              CODE        IN NUMBER,
                              NAME        IN VARCHAR2,
                              DISCRIPTION IN VARCHAR2,
                              AUDITEDBY   IN NUMBER,
                              INSPECTEDBY IN NUMBER,
                              TYPEID      IN NUMBER,
                              STATUS      IN CHAR,
                              AUDITABLE   IN VARCHAR2) is
  begin
    insert into T_AUDITEE_ENTITIES
      (ENTITY_ID,
       
       CODE,
       DESCRIPTION,
       NAME,
       ACTIVE,
       TYPE_ID,
       AUDITBY_ID,
       INSPECTEDBY_ID,
       AUDITABLE)
    values
      ((select COALESCE(max(acc.ENTITY_ID) + 1, 1)
         from T_AUDITEE_ENTITIES acc),
       CODE,
       DISCRIPTION,
       name,
       STATUS,
       TYPEID,
       AUDITEDBY,
       INSPECTEDBY,
       AUDITABLE);
  
    COMMIT;
  end P_InsertENTITIEES;

  Procedure p_getglheadsummary_Yearly(PPNumber  in number,
                                      io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select GT.GL_TYPEID as GLSUBCODE,
             T.CODE as branchid,
             GT.DESCRIPTION as GLSUBNAME,
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2021' and
                        '31-Dec-2021' then
                    t.cr
                   else
                    0
                 end) as credit_2021,
             
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2021' and
                        '31-Dec-2021' then
                    t.running_dr
                   else
                    0
                 end) as debit_2021,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2021' and
                         '31-Dec-2021' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2021' and
                                     '31-Dec-2021' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2021,
             
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2022' and
                        '31-Dec-2022' then
                    t.cr
                   else
                    0
                 end) as credit_2022,
             
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2022' and
                        '31-Dec-2022' then
                    t.running_dr
                   else
                    0
                 end) as debit_2022,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2022' and
                         '31-Dec-2022' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2022' and
                                     '31-Dec-2022' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2022
      
        from t_Au_Preinfo_Gldayendbalance t
       inner join t_auditee_entities ee
          on ee.entity_id = t.enitity_id
       inner join t_au_plan_eng e
          on e.entity_id = ee.entity_id
       inner join t_au_audit_joining j
          on j.eng_plan_id = e.eng_id
       inner join t_au_audit_team_tasklist tt
          on tt.eng_plan_id = e.eng_id
       INNER JOIN t_au_preinfo_gl_type GT
          ON GT.DESCRIPTION = T.DESCRIPTION
       where  j.team_mem_ppno = 126955
       GROUP BY GT.GL_TYPEID, T.code, GT.DESCRIPTION
       ORDER BY GT.GL_TYPEID;
  
  end p_getglheadsummary_Yearly;

  Procedure p_getglheadsummary(PPNumber in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select distinct GT.GL_TYPEID,
                      T.CODE as branchid,
                      GT.DESCRIPTION,
                      SUM(case
                            when t.balancetype = 'D' then
                             t.running_dr
                            else
                             null
                          end) as debit,
                      SUM(case
                            when t.balancetype = 'C' then
                             t.cr
                            else
                             null
                          end) as credit,
                      SUM(t.balance) AS BALANCE
        from t_Au_Preinfo_Gldayendbalance t
       inner join t_auditee_entities ee
          on ee.entity_id = t.enitity_id
       inner join t_au_plan_eng e
          on e.entity_id = ee.entity_id
         and t.enddate between e.operation_startdate and
             e.operation_enddate
       inner join t_au_audit_joining j
          on j.eng_plan_id = e.eng_id
       inner join t_au_audit_team_tasklist tt
          on tt.eng_plan_id = e.eng_id
       INNER JOIN t_au_preinfo_gl_type GT
          ON GT.DESCRIPTION = T.DESCRIPTION
       where j.status = 'I'
         AND j.team_mem_ppno = PPNumber
       GROUP BY GT.GL_TYPEID, T.code, GT.DESCRIPTION
       ORDER BY GT.GL_TYPEID;
  
  end p_getglheadsummary;

  procedure P_GetGlheadSum(PPNumber  in number,
                           GLTYPEID  IN NUMBER,
                           io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select distinct T.glsubcode,
                      t.code        as branchid,
                      T.DESCRIPTION,
                      T.glsubname,
                      t.running_dr  as debit,
                      t.cr          as credit,
                      t.balance
        from t_Au_Preinfo_Gldayendbalance t
       inner join t_auditee_entities ee
          on ee.entity_id = t.enitity_id
       inner join t_au_plan_eng e
          on e.entity_id = ee.entity_id
         and t.enddate between e.operation_startdate and
             e.operation_enddate
       inner join t_au_audit_joining j
          on j.eng_plan_id = e.eng_id
       where j.status = 'I'
         AND j.team_mem_ppno = PPNumber
         AND t.gl_typeid = GLTYPEID
       ORDER BY T.DESCRIPTION desc, t.glsubname;
  
  end P_GetGlheadSum;

  procedure P_GetGlheadDetails(PPNumber  in number,
                               subcode   in number,
                               io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select T.code        as branchid,
             T.DESCRIPTION,
             T.glsubcode,
             T.glsubname,
             T.ENDDATE     as datetime,
             t.running_dr  as debit,
             t.cr          as credit,
             t.balance
        from t_Au_Preinfo_Gldayendbalance t
       inner join t_auditee_entities ee
          on ee.entity_id = t.enitity_id
       inner join t_au_plan_eng e
          on e.entity_id = ee.entity_id
         and t.enddate between e.operation_startdate and
             e.operation_enddate
       inner join t_au_audit_joining j
          on j.eng_plan_id = e.eng_id
       where j.status = 'I'
         AND j.team_mem_ppno = PPNumber
         AND t.glsubcode = subcode
       ORDER BY T.DESCRIPTION, t.enddate;
  
  end P_GetGlheadDetails;

  procedure P_GetStaffPosition(PPNumber in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select emp.pp_no                  as PPNO,
             emp.employee_name          as EMPLOYEE_NAME,
             emp.qualification          as QUALIFICATION,
             emp.functional_designation as DESIGNATION,
             emp.rank                   as RANK_DESC,
             emp.place_of_posting_name  as PLACE_OF_POSTING,
             emp.last_posting_date      as Date_of_Posting
      
        from t_au_preinfo_hr emp
       inner join t_auditee_entities e
          on upper(e.name) = Upper(emp.place_of_posting_name)
       inner join t_au_plan_eng eg
          on eg.entity_id = e.entity_id
       inner join t_au_audit_joining j
          on j.eng_plan_id = eg.eng_id
         and j.status = 'I'
       where j.team_mem_ppno = PPNumber
       order by emp.functional_designation asc;
  
  end P_GetStaffPosition;

  procedure P_preauditinfo_loan_scheme_yearly(PPNumber  in number,
                                              io_cursor OUT t_cursor) as
  begin
  
    OPEN io_cursor FOR
      select EE.ENTITY_ID,
             a.disb_statusid,
             a.glsubcode,
             s.glsubname,
             Sum(case
                   when trunc(a.disb_date) between '01-Jan-2021' and
                        '31-Dec-2021' then
                    a.disbursed_amount
                   else
                    0
                 end) as disbursed_amount_2021,
             Sum(case
                   when trunc(a.disb_date) between '01-Jan-2021' and
                        '31-Dec-2021' then
                    a.prin
                   else
                    0
                 end) as Prin_out_2021,
             Sum(case
                   when trunc(a.disb_date) between '01-Jan-2021' and
                        '31-Dec-2021' then
                    a.markup
                   else
                    0
                 end) as Markup_out_2021,
             Sum(case
                   when trunc(a.disb_date) between '01-Jan-2022' and
                        '31-Dec-2022' then
                    a.disbursed_amount
                   else
                    0
                 end) as disbursed_amount_2022,
             Sum(case
                   when trunc(a.disb_date) between '01-Jan-2022' and
                        '31-Dec-2022' then
                    a.prin
                   else
                    0
                 end) as Prin_out_2022,
             Sum(case
                   when trunc(a.disb_date) between '01-Jan-2022' and
                        '31-Dec-2022' then
                    a.markup
                   else
                    0
                 end) as Markup_out_2022
      
        from T_AU_PREINFO_liveloan_details a
       inner join T_AUDITEE_ENTITIES EE
          on a.branchid = ee.code
         and  ee.type_id = 6
       inner join t_au_plan_eng e
          on e.entity_id = ee.entity_id
       inner join t_au_preinfo_glsub s
          on s.glsubcode = a.glsubcode
       inner join t_au_audit_joining j
          on j.eng_plan_id = e.eng_id
       where j.status = 'I'
         AND j.team_mem_ppno = PPNumber
       group by EE.ENTITY_ID, a.disb_statusid, a.glsubcode, s.glsubname;
  
  end P_preauditinfo_loan_scheme_yearly;

  procedure P_preauditinfo_loan_scheme(PPNumber  in number,
                                       io_cursor OUT t_cursor) as
  begin
  
    OPEN io_cursor FOR
      select EE.ENTITY_ID,
             a.glsubcode,
             s.glsubname,
             Sum(a.disbursed_amount) as disbursed_amount,
             Sum(a.prin) as Prin_out,
             Sum(a.markup) as Markup_out
      
        from T_AU_PREINFO_liveloan_details a
       inner join T_AUDITEE_ENTITIES EE
          on a.branchid = ee.code
         and ee.type_id = 6
       inner join t_au_plan_eng e
          on e.entity_id = ee.entity_id
         and a.disb_date between e.operation_startdate and
             e.operation_enddate
       inner join t_au_preinfo_glsub s
          on s.glsubcode = a.glsubcode
       inner join t_au_audit_joining j
          on j.eng_plan_id = e.eng_id
       where j.status = 'I'
         AND j.team_mem_ppno = PPNumber
       group by EE.ENTITY_ID, a.glsubcode, s.glsubname;
  
  end P_preauditinfo_loan_scheme;

  procedure P_GetLoanCaseDetails(PPNumber  in number,
                                 Loantype  in varchar2,
                                 io_cursor OUT t_cursor) as
  
  begin
    if (Loantype = 'live') then
      OPEN io_cursor FOR
        select J.TEAM_MEM_PPNO,
               EE.ENTITY_ID as BRANCHID,
               a.loan_case_no,
               a.cnic,
               a.customername,
               a.fathername,
               a.disbursed_amount,
               a.prin,
               a.markup,
               a.glsubcode,
               a.disb_date,
               a.valid_until,
               a.disb_statusid
        
          from T_AU_PREINFO_liveloan_details a
         inner join T_AUDITEE_ENTITIES EE
            on a.branchid = ee.code
           and ee.type_id = 6
         inner join t_au_plan_eng e
            on e.entity_id = ee.entity_id
           and a.disb_date between e.operation_startdate and
               e.operation_enddate
         inner join t_au_audit_joining j
            on j.eng_plan_id = e.eng_id
         where ceil(a.markup) > '0'
           and (a.prin) > '0'
           and j.status = 'I'
           AND j.team_mem_ppno = PPNumber;
    else
      OPEN io_cursor FOR
        select J.TEAM_MEM_PPNO,
               EE.ENTITY_ID,
               a.loan_case_no,
               a.cnic,
               a.customername,
               a.fathername,
               a.disbursed_amount,
               a.prin,
               a.markup,
               a.glsubcode,
               a.disb_date,
               a.valid_until,
               a.disb_statusid
        
          from T_AU_PREINFO_liveloan_details a
         inner join T_AUDITEE_ENTITIES EE
            on a.branchid = ee.code 
           and ee.type_id = 6
         inner join t_au_plan_eng e
            on e.entity_id = ee.entity_id
           and a.disb_date between e.operation_startdate and
               e.operation_enddate
         inner join t_au_audit_joining j
            on j.eng_plan_id = e.eng_id
         where j.status = 'I'
           AND j.team_mem_ppno = PPNumber;
    end if;
  end P_GetLoanCaseDetails;

  procedure P_GetFunctionalResponsibilityWisePara(ENTITYID         in number,
                                                  PROCESSID        IN NUMBER,
                                                  SUB_PROCESSID    IN NUMBER,
                                                  PROCESS_DETAILID IN NUMBER,
                                                  io_cursor        OUT t_cursor) is
  
  begin
    IF (PROCESSID != 0) THEN
      open io_cursor for
        select *
          FROM v_dash_borad_of_divisional_head BD
         WHERE BD.ENTITY_ID = ENTITYID
           AND BD.process_id = PROCESSID;
    ELSE
      IF (SUB_PROCESSID != 0) THEN
        open io_cursor for
          select *
            FROM v_dash_borad_of_divisional_head BD
           WHERE BD.ENTITY_ID = ENTITYID
             AND BD.Sub_process_id = SUB_PROCESSID;
      ELSE
        IF (PROCESS_DETAILID != 0) THEN
          open io_cursor for
            select *
              FROM v_dash_borad_of_divisional_head BD
             WHERE BD.ENTITY_ID = ENTITYID
               AND BD.check_list_detail_id = PROCESS_DETAILID;
        ELSE
          open io_cursor for
            select *
              FROM v_dash_borad_of_divisional_head BD
             WHERE BD.ENTITY_ID = ENTITYID;
        END IF;
      END IF;
    END IF;
  
  end P_GetFunctionalResponsibilityWisePara;

  Procedure P_AddDivisionalHeadRemarksOnFunctionalLegacyPara(CONCERNED_DEPTID in number,
                                                             COMMENTS         in varchar2,
                                                             REF_PARAID       in number,
                                                             PPNumber         in number) is
  begin
  
    INSERT INTO t_au_old_functional_para_division_head_remarks
      (ID,
       PARA_ID,
       REF_P,
       ENTITY_ID,
       ENTITY_NAME,
       PARA_NO,
       CONCERNED_DEPT_ID,
       REMARKS,
       CREATED_BY)
      select (select COALESCE(max(p.ID) + 1, 1)
                from t_au_old_functional_para_division_head_remarks p),
             REF_PARAID,
             f.REF_P,
             f.entity_id,
             f.entity_name,
             f.para_no,
             CONCERNED_DEPTID,
             COMMENTS,
             PPNumber
        from t_au_old_paras_fad f
       where f.id = REF_PARAID;
    commit;
  
  end P_AddDivisionalHeadRemarksOnFunctionalLegacyPara;

  procedure P_Getrealtionshiptype(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select f.entity_realtion_id,
             f.parent_name || '   TO   ' || f.chlid_name as field_name
        from t_auditee_ent_relation f
       where f.status = 'Y'
         and f.id is not null
       order by f.id;
  end P_Getrealtionshiptype;

  procedure P_Getparentrepoffice(rid in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select Distinct (r.p_name) as DESCRIPTION,
                      r.parent_id as ENTITY_ID,
                      r.relation_type_id as ENTITY_REALTION_ID,
                      t.entitytypedesc as ENTITYTYPEDESC,
                      r.status as ACTIVE
        from t_auditee_ent_relation    e,
             t_auditee_ent_types       t,
             T_AUDITEE_ENTITIES_MAPING r
       where t.autid = r.relation_type_id
         and r.p_type_id = e.parent_entity_typeid
         and r.c_type_id = e.child_entity_typeid
         and r.relation_type_id = rid
         and r.parent_id is not null
       order by r.p_name;
  end P_Getparentrepoffice;

  procedure P_Getchildposting(erid in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select distinct (r.entity_id), r.c_name, r.c_name, e.status
        from t_auditee_ent_relation    e,
             t_auditee_ent_types       t,
             T_AUDITEE_ENTITIES_MAPING r
       where t.autid = r.relation_type_id
         and r.p_type_id = e.parent_entity_typeid
         and r.c_type_id = e.child_entity_typeid
         and r.parent_id = erid
       order by r.c_name;
  end P_Getchildposting;

  procedure P_GetCCQsEntities(PPNO in number, io_cursor OUT t_cursor) is
  
  begin
    if (PPNO is not null) then
      open io_cursor for
        select distinct e.code, e.name, e.entity_id
          from t_au_ccq c
         inner join t_auditee_entities e
            on e.entity_id = c.entity_id
         inner join t_user t
            on t.entity_id = e.auditby_id
         where t.ppno = PPNO;
    else
      open io_cursor for
        select distinct e.code, e.name, e.entity_id
          from t_au_ccq c
         inner join t_auditee_entities e
            on e.entity_id = c.entity_id;
    end if;
  end P_GetCCQsEntities;

  procedure P_GetAuditeeAssignedEntities(ENTITID   in number,
                                         io_cursor OUT t_cursor) is
  
  begin
    if (ENTITID is not null) then
      open io_cursor for
        select distinct t.name, t.code, t.entity_id, o.engplanid
          from t_au_observation o
         inner join t_au_plan_eng e
            on e.eng_id = o.engplanid
         inner join t_auditee_entities t
            on t.entity_id = e.entity_id
         inner join t_au_period p
            on e.period_id = p.auditperiodid
         inner join t_au_observation_assignedto ot
            on o.id = ot.obs_id
         where p.status_id = 2
           and ot.entity_id = ENTITID;
    else
      open io_cursor for
        select distinct t.name, t.code, t.entity_id, o.engplanid
          from t_au_observation o
         inner join t_au_plan_eng e
            on e.eng_id = o.engplanid
         inner join t_auditee_entities t
            on t.entity_id = e.entity_id
         inner join t_au_period p
            on e.period_id = p.auditperiodid
         inner join t_au_observation_assignedto ot
            on o.id = ot.obs_id
         where p.status_id = 2;
    end if;
  end P_GetAuditeeAssignedEntities;

  procedure p_GetObservationEntities(PP_NO     in number,
                                     io_cursor OUT t_cursor) is
    V_F NUMBER := 0;
    E_F number := 0;
  begin
  
    SELECT NVL(max(G.GROUP_ID), 0)
      INTO V_F
      FROM t_User_Maping G
     WHERE g.ppno = PP_NO;
    select u.entity_id into E_F from t_user u where u.ppno = pp_no;
    if (V_F = 1) then
      open io_cursor for
        select distinct t.name, t.code, t.entity_id, j.eng_plan_id
          from t_au_audit_joining j
         inner join t_au_plan_eng e
            on e.eng_id = j.eng_plan_id
         inner join t_auditee_entities t
            on t.entity_id = e.entity_id
         inner join t_au_period p
            on e.period_id = p.auditperiodid
         where E.STATUS < '9';
    
    else
      if (V_F = 5) then
        open io_cursor for
          select distinct t.name, t.code, t.entity_id, j.eng_plan_id
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
      
      ELSE
        if (V_F in (4, 6, 7, 15)) then
          open io_cursor for
            select distinct t.name, t.code, t.entity_id, j.eng_plan_id
              from t_au_audit_joining j
             inner join t_au_plan_eng e
                on e.eng_id = j.eng_plan_id
             inner join t_auditee_entities t
                on t.entity_id = e.entity_id
             inner join t_au_period p
                on e.period_id = p.auditperiodid
             where E.STATUS < '9'
               and e.auditby_id = E_F;
        
        else
          open io_cursor for
            select distinct t.name, t.code, t.entity_id, ja.eng_plan_id
              from t_au_audit_joining ja
             inner join t_au_plan_eng e
                on e.eng_id = ja.eng_plan_id
             inner join t_auditee_entities t
                on t.entity_id = e.entity_id
             inner join t_au_period p
                on e.period_id = p.auditperiodid
             where p.status_id = 2
               and ja.team_mem_ppno = PP_NO;
        end if;
      end if;
    end if;
  end p_GetObservationEntities;

  procedure P_GetActiveInactiveChartData(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      select f.status, count(f.status) as total_count
        from t_au_old_paras_fad f
       group by f.status;
  end P_GetActiveInactiveChartData;

  procedure P_GetUserWiseOldParasPerformance(UserEntityID IN NUMBER,
                                             io_cursor    OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      select s.*, sz.zonename
        from v_report_az_emp_progress s
       inner join v_report_az_progress sz
          on s.audit_zoneid = sz.id;
--       WHERE s.AUDIT_ZONEID = UserEntityID;
  end P_GetUserWiseOldParasPerformance;

  procedure P_GetZoneWiseOldParasPerformance(UserEntityID IN NUMBER,
                                             io_cursor    OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      select * from v_report_az_progress s where s.ID = UserEntityID;
  end P_GetZoneWiseOldParasPerformance;

  procedure P_AddOldParas(PROCESS       in number,
                          SUBPROCESS    in number,
                          PROCESSDETAIL in number,
                          PPNO          in number,
                          PID           IN NUMBER,
                          REPLYTEXT     in clob) as
  begin
    UPDATE T_AU_OLD_PARAS_FAD al
       SET 
           al.PROCESS_DETAIL = PROCESSDETAIL,
           al.STATUS         = 1,
           al.ENTERED_BY     = ppno,
           al.ENTERED_ON     = sysdate
     WHERE al.ID = PID;
    commit;
  end P_AddOldParas;

  procedure P_GetOutstandingParasAuditYear(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      select distinct P.PERIOD
        from t_Au_Observation_Old_Cad_Paras P
       order by P.PERIOD;
  end P_GetOutstandingParasAuditYear;

  procedure P_GetOldParasAuditYear(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      select distinct audit_period
        from t_au_old_paras_fad
       WHERE STATUS = 0
       order by audit_period;
  end P_GetOldParasAuditYear;

  procedure P_GetAuditeeOldParasFAD(EntityID  in number,
                                    io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select e.name, s.*
        from t_au_old_paras_fad s
        left join t_auditee_entities e
          on e.code = s.entity_code
       WHERE e.auditby_id = EntityID
       order by s.AUDIT_PERIOD, s.Entity_Name, s.para_no;
  
  end P_GetAuditeeOldParasFAD;

  procedure P_GetAuditeeOldParasFADtext(refp      in VARCHAR2,
                                        io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select s.para_text
        from t_au_old_paras_fad_text s
       WHERE s.ref_p = refp;
  
  end P_GetAuditeeOldParasFADtext;

  procedure P_updateAuditeeOldParasFADtext(refp       in VARCHAR2,
                                           textchange in number,
                                           ptext      in clob,
                                           newstatus  in number) as
  begin
    if (textchange = 0) then
      update t_au_old_paras_fad fs
         set fs.para_status = newstatus
       WHERE fs.ref_p = refp;
      commit;
    else
      update t_au_old_paras_fad_text s
         set s.para_text = ptext
       WHERE s.ref_p = refp;
      commit;
      update t_au_old_paras_fad fs
         set fs.para_status = newstatus
       WHERE fs.ref_p = refp;
      commit;
    end if;
  
  end P_updateAuditeeOldParasFADtext;



  procedure P_GetOldParas(Entityid  in number,
                          AUDITYEAR in number,
                          io_cursor OUT t_cursor) is
  begin
    if (AUDITYEAR > 0 and AUDITYEAR is not null and Entityid is not null) then
      open io_cursor for
        select *
          from t_au_old_paras_fad
         WHERE STATUS = 0
           and AUDIT_PERIOD = AUDITYEAR
           and entity_id = Entityid
         order by ID;
    else
      if (Entityid is not null) then
        open io_cursor for
          select *
            from t_au_old_paras_fad
           WHERE STATUS = 0
             and entity_id = Entityid
           order by ID;
      else
        if (AUDITYEAR is not null) then
          open io_cursor for
            select *
              from t_au_old_paras_fad
             WHERE STATUS = 0
               and AUDIT_PERIOD = AUDITYEAR
             order by ID;
        else
          open io_cursor for
            select * from t_au_old_paras_fad WHERE STATUS = 0 order by ID;
        end if;
      end if;
    end if;
  end P_GetOldParas;

  procedure P_AddOldParasReply(PPNO  in number,
                               PID   IN NUMBER,
                               REPLY in clob) as
  begin
    INSERT INTO T_AU_OLD_PARAS_RESPONSE_FAD o
      (o.ID,
       o.REF_P,
       o.REPLY,
       o.REPLIEDBY,
       o.REPLIEDDATE,
       o.REMARKS,
       o.SUBMITTED,
       o.AUDITEDBY,
       o.ENTITY_ID,
       o.C_STATUS)
      SELECT (select COALESCE(max(acc.ID) + 1, 1)
                from T_AU_OLD_PARAS_RESPONSE_FAD acc),
             PID,
             REPLY,
             PPNO,
             SYSDATE,
             'Response Submitted',
             'Y',
             f.audited_by,
             f.entity_id,
             '3'
        from t_au_old_paras_fad f
       where f.id = pid;
    commit;
    UPDATE T_AU_OLD_PARAS_FAD al SET al.STATUS = 3 WHERE al.ID = PID;
    commit;
  end P_AddOldParasReply;

  

  procedure P_GetDepositAccountSubdetails(PPNumber  in number,
                                          io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select v.*
        from t_au_preinfo_depositaccount_sum v
       inner join t_au_plan_eng e
          on e.entity_code = v.branchcode
       inner join t_au_audit_joining j
          on e.eng_id = j.eng_plan_id
       where j.team_mem_ppno = PPNumber
         and j.status = 'I';
  
  end P_GetDepositAccountSubdetails;

  procedure P_GetDepositACCOUNTCATEGORY(PPNumber  in number,
                                        io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select v.*
        from t_au_preinfo_DEPOSITACCOUNTCATEGORY v
       inner join t_au_plan_eng e
          on e.entity_code = v.branchcode
       inner join t_au_audit_joining j
          on e.eng_id = j.eng_plan_id
       where j.team_mem_ppno = PPNumber
         and j.status = 'I';
  
  end P_GetDepositACCOUNTCATEGORY;

  procedure P_GetIncomeExpenceDetails(PPNumber  in number,
                                      io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select * from V_GET_GL_SUM;
  
  end P_GetIncomeExpenceDetails;

  /*procedure P_GetLoanCaseDocuments(PPNumber  in number,
                                   io_cursor OUT t_cursor) as
    
    begin
      OPEN io_cursor FOR
        select * from v_list_cbas_sdms s where s.team_mem_ppno =PPNumber;
    
    end P_GetLoanCaseDocuments;
  */

  procedure P_responibilityassigned(ID        IN t_au_observation_responibility_assigned.id%type,
                                    PPNO      IN t_au_observation_responibility_assigned.assignedby%TYPE,
                                    RES_PP    IN t_au_observation_responibility_assigned.pp_no%type,
                                    LOANCASE  IN NUMBER,
                                    ACCNUMBER IN NUMBER,
                                    LCAMOUNT  IN NUMBER,
                                    ACAMOUNT  IN NUMBER) is
  begin
    INSERT INTO t_au_observation_responibility_assigned
      (id,
       obs_id,
       assignedby,
       pp_no,
       is_active,
       LOAN_CASE,
       ACCOUNT_NUMBER,
       LC_AMOUNT,
       AC_AMOUNT)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from t_au_observation_responibility_assigned acc),
       ID,
       ppno,
       RES_PP,
       'Y',
       LOANCASE,
       ACCNUMBER,
       LCAMOUNT,
       ACAMOUNT);
    commit;
  
  end P_responibilityassigned;

  procedure P_DashboardDivisionalHeadfad(entityid  in number,
                                         io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select e.entity_id,
             e.name as F_NAME,
             c.id,
             C.HEADING,
             COUNT(T.ID) NO_OF_para
        from T_AU_OLD_PARAS_FAD t
       INNER JOIN T_AUDIT_CHECKLIST_DETAILS C
          ON C.ID = T.PROCESS_DETAIL
       inner join t_r_sub_group s
          on s.s_gr_id = C.V_ID
       inner join t_auditee_entities e
          on e.entity_id = c.owner_enitity_id
       where c.owner_enitity_id = entityid
       GROUP BY e.entity_id, e.name, c.id, C.HEADING;
  
  end P_DashboardDivisionalHeadfad;

  procedure P_DashboardDivisionalHeadfadDetail(entityid  in number,
                                               io_cursor OUT t_cursor) is
  begin
    open io_cursor for
    
      select e.entity_id,
             e.name as F_NAME,
             Z.ZONEID,
             Z.ZONENAME,
             T.ENTITY_CODE,
             T.ENTITY_NAME,
             C.HEADING
        from T_AU_OLD_PARAS_FAD t
       INNER JOIN T_AUDIT_CHECKLIST_DETAILS C
          ON C.ID = T.PROCESS_DETAIL
       inner join t_r_sub_group s
          on s.s_gr_id = C.V_ID
       inner join t_auditee_entities e
          on e.entity_id = c.owner_enitity_id
       INNER JOIN V_SERVICE_BRANCH B
          ON B.BRANCHCODE = T.ENTITY_CODE
       INNER JOIN V_SERVICE_ZONES Z
          ON Z.ZONEID = B.ZONEID
       where c.owner_enitity_id = entityid;
  
  end P_DashboardDivisionalHeadfadDetail;

  Procedure P_email(PPNO      NUMBER,
                    EnitityID in number,
                    io_cursor OUT t_cursor) is
  
    E_F VARCHAR2(200) := 0;
  begin
    SELECT E.EMAIL
      INTO E_F
      FRoM V_SERVICE_EMPLOYEEINFO E
     WHERE E.PPNO = PPNO;
  
    --open io_cursor for
  
  end P_email;

  Procedure P_getoldparamanagement(EnitityID in number,
                                   io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select count(NVL(e.parent_enititid, 0))
      into V_F
      from T_AUDITEE_ENTITEE_SUBENTITY e
     where e.enitity_id = EnitityID;
    if (V_F = 0) then
      open io_cursor for
        select t.*
          from T_AU_OBSERVATION_OLD_CAD_PARAS t
         WHERE T.AUDITED_BY = EnitityID
           AND T.PARA_STATUS = 1;
    else
      open io_cursor for
        select t.*, s.parent_enititid
          from T_AU_OBSERVATION_OLD_CAD_PARAS t
          left join T_AUDITEE_ENTITEE_SUBENTITY s
            on t.entity_id = s.enitity_id
         WHERE T.AUDITED_BY = EnitityID
           AND T.PARA_STATUS = 1;
    end if;
  end P_getoldparamanagement;

  Procedure P_updateoldparamanagement(Paraid       in number,
                                      VCATID       in number,
                                      VCATNATUREID in number,
                                      RISKID       in number,
                                      ParaText     in clob,
                                      CREATEDBY    IN NUMBER,
                                      io_cursor    OUT t_cursor) is
  
  begin
    If (VCATID is null or VCATNATUREID is null or RISKID is null) then
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 9;
    else
      update T_AU_OBSERVATION_OLD_CAD_PARAS t
         set t.v_cat_id        = VCATID,
             t.v_cat_nature_id = VCATNATUREID,
             t.risk_id         = RISKID,
             T.ENTERED_BY      = CREATEDBY,
             T.ENTERED_ON      = SYSDATE,
             t.status          = 1
       where t.para_id = paraid;
      commit;
      insert into T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT
        (ID, OBSERVATSION_ID, TEXT, ENTEREDBY, ENTEREDDATE)
      values
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT acc),
         paraid,
         paratext,
         CREATEDBY,
         sysdate);
      commit;
    
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 15;
    end if;
  end P_updateoldparamanagement;

  procedure P_GetAuditeeOldParasentities(EntityID  in number,
                                         io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select distinct e.entity_id, e.name
        from T_AU_OBSERVATION_OLD_CAD_PARAS s
       inner join t_auditee_entities e
          on s.entity_id = e.entity_id
       WHERE e.entity_id = EntityID
       order by e.name;
  
  end P_GetAuditeeOldParasentities;

  procedure P_GetAuditeeOldParas(EntityID  in number,
                                 io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select e.name,
             s.para_id,
             s.period,
             s.para_no,
             s.gist_of_paras,
             v.v_name,
             vs.sub_v_name,
             r.description as risk
        from T_AU_OBSERVATION_OLD_CAD_PARAS s
       inner join t_auditee_entities e
          on s.entity_id = e.entity_id
       inner join t_control_violation v
          on s.v_cat_id = v.id
       inner join t_control_violation_sub vs
          on vs.id = s.v_cat_nature_id
       inner join t_coso_risk r
          on r.r_id = s.risk_id
       WHERE e.entity_id = EntityID
         and s.status = 2
       order by s.Entity_Name, s.para_no;
  
  end P_GetAuditeeOldParas;

  procedure P_GetAuditeeOldParastext(paraid    in number,
                                     io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select t.*
        from t_au_observation_old_cad_paras_text t
       where t.observatsion_id = paraid
       order by t.id;
  
  end P_GetAuditeeOldParastext;

  Procedure P_getAuditeeOldParascompliance(Paraid    in number,
                                           Reply_NO  in number,
                                           io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select *
        from T_AU_OBSERVATION_OLD_CAD_PARAS_Response r
       where r.au_obs_id = paraid
         and r.replyno = reply_no
       order by r.replieddate;
  
  end P_getAuditeeOldParascompliance;

  Procedure P_UpdateAuditeeOldParasresponse(Paraid    in number,
                                            cdate     in date,
                                            Text      in clob,
                                            PPNO      in number,
                                            Remarks   in clob,
                                            imprec    in varchar2,
                                            io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select nvl(max(r.replyno), 0)
      into V_F
      from T_AU_OBSERVATION_OLD_CAD_PARAS_Response r
     where r.au_obs_id = paraid;
    insert into T_AU_OBSERVATION_OLD_CAD_PARAS_Response
      (Id,
       replyno,
       Au_Obs_Id,
       Reply,
       Repliedby,
       Replieddate,
       C_I_Remarks,
       imp_recommendation,
       status)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_AU_OBSERVATION_OLD_CAD_PARAS_Response acc),
       (V_F + 1),
       Paraid,
       Text,
       PPNO,
       cdate,
       Remarks,
       imprec,
       '1');
    commit;
    update t_au_observation_old_cad_paras t
       set t.status = 2
     where t.para_id = Paraid;
    commit;
    open io_cursor for
      select r.ref, r.remarks from t_au_remarks r where r.id = 15;
  
  end P_UpdateAuditeeOldParasresponse;

  Procedure P_SubmitAuditeeOldParasresponse(Paraid    in number,
                                            io_cursor OUT t_cursor) is
  
  begin
    update T_AU_OBSERVATION_OLD_CAD_PARAS_Response t
       set t.status = 2
     where t.au_obs_id = paraid;
    commit;
    open io_cursor for
      select r.ref, r.remarks from t_au_remarks r where r.id = 15;
  
  end P_SubmitAuditeeOldParasresponse;

  Procedure P_UpdateAuditeeOldPacompliance(Paraid    in number,
                                           text      in varchar2,
                                           status    in number,
                                           io_cursor OUT t_cursor) is
  
  begin
    update T_AU_OBSERVATION_OLD_CAD_PARAS_Response t
       set t.headauditcomments = text, t.headauditdecision = status
     where t.au_obs_id = paraid;
    commit;
    open io_cursor for
      select r.ref, r.remarks from t_au_remarks r where r.id = 15;
  
  end P_UpdateAuditeeOldPacompliance;

  --- Dash board

  Procedure p_getglheadsummary_dash(PPNumber  in number,
                                    io_cursor OUT t_cursor) is
    R_F number := 0;
  begin
    select m.group_id
      into R_F
      from t_user_maping m
     where m.ppno = PPNumber;
    if (R_F = 1) then
      open io_cursor for
        select t.year,
               GT.GL_TYPEID as GLSUBCODE,
               
               GT.DESCRIPTION as GLSUBNAME,
               
               sum(case
                     when t.balancetype = 'C' and t.gl_typeid = 1 then
                      t.cr
                     else
                      0
                   end) as Assest_Credit,
               sum(case
                     when t.balancetype = 'D' and t.gl_typeid = 1 then
                      t.running_dr
                     else
                      0
                   end) as Assest_Debit,
               sum(case
                     when t.balancetype = 'C' and t.gl_typeid = 2 then
                      t.cr
                     else
                      0
                   end) as LIABILITY_Credit,
               sum(case
                     when t.balancetype = 'D' and t.gl_typeid = 2 then
                      t.running_dr
                     else
                      0
                   end) as LIABILITY_Debit,
               sum(case
                     when t.balancetype = 'C' and t.gl_typeid = 3 then
                      t.cr
                     else
                      0
                   end) as INCOME_Credit,
               sum(case
                     when t.balancetype = 'D' and t.gl_typeid = 3 then
                      t.running_dr
                     else
                      0
                   end) as INCOME_Debit,
               sum(case
                     when t.balancetype = 'C' and t.gl_typeid = 4 then
                      t.cr
                     else
                      0
                   end) as EXPENSE_Credit,
               sum(case
                     when t.balancetype = 'D' and t.gl_typeid = 4 then
                      t.running_dr
                     else
                      0
                   end) as EXPENSE_Debit
          from t_Au_Preinfo_Gldayendbalance t
         inner join t_auditee_entities ee
            on ee.entity_id = t.enitity_id
         inner join t_auditee_entities_maping e
            on e.entity_id = ee.entity_id
         INNER JOIN t_au_preinfo_gl_type GT
            ON GT.DESCRIPTION = T.DESCRIPTION
         group by t.year, GT.GL_TYPEID, Gt.Description
         order by t.year;
    else
    
      open io_cursor for
        select GT.GL_TYPEID   as GLSUBCODE,
               Gt.Description as GL_Type,
               GT.DESCRIPTION as GLSUBNAME,
               t.running_dr   as Debit,
               t.cr           as Credit
          from t_Au_Preinfo_Gldayendbalance t
         inner join t_auditee_entities ee
            on ee.entity_id = t.enitity_id
         inner join t_auditee_entities_maping e
            on e.entity_id = ee.entity_id
         INNER JOIN t_au_preinfo_gl_type GT
            ON GT.DESCRIPTION = T.DESCRIPTION
         inner join t_user m
            on m.entity_id = ee.entity_id
         inner join t_user_maping mp
            on mp.ppno = m.ppno
         where mp.role_id = R_F
           and m.entity_id = e.parent_id;
    
    end if;
  end p_getglheadsummary_dash;

  Procedure p_getglheadsummary_dash_Yearly(PPNumber  in number,
                                           io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select GT.GL_TYPEID as GLSUBCODE,
             T.CODE as branchid,
             GT.DESCRIPTION as GLSUBNAME,
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2021' and
                        '31-Dec-2021' then
                    t.cr
                   else
                    0
                 end) as credit_2021,
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2021' and
                        '31-Dec-2021' then
                    t.running_dr
                   else
                    0
                 end) as debit_2021,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2021' and
                         '31-Dec-2021' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2021' and
                                     '31-Dec-2021' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2021,
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2022' and
                        '31-Dec-2022' then
                    t.cr
                   else
                    0
                 end) as credit_2022,
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2022' and
                        '31-Dec-2022' then
                    t.running_dr
                   else
                    0
                 end) as debit_2022,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2022' and
                         '31-Dec-2022' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2022' and
                                     '31-Dec-2022' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2022,
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2020' and
                        '31-Dec-2020' then
                    t.cr
                   else
                    0
                 end) as credit_2020,
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2020' and
                        '31-Dec-2020' then
                    t.running_dr
                   else
                    0
                 end) as debit_2020,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2020' and
                         '31-Dec-2020' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2020' and
                                     '31-Dec-2020' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2020,
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2019' and
                        '31-Dec-2019' then
                    t.cr
                   else
                    0
                 end) as credit_2019,
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2019' and
                        '31-Dec-2019' then
                    t.running_dr
                   else
                    0
                 end) as debit_2019,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2019' and
                         '31-Dec-2019' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2019' and
                                     '31-Dec-2019' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2019,
             SUM(case
                   when t.balancetype = 'C' and t.enddate between '01-Jan-2018' and
                        '31-Dec-2018' then
                    t.cr
                   else
                    0
                 end) as credit_2018,
             SUM(case
                   when t.balancetype = 'D' and t.enddate between '01-Jan-2018' and
                        '31-Dec-2018' then
                    t.running_dr
                   else
                    0
                 end) as debit_2018,
             (SUM(case
                    when t.balancetype = 'C' and t.enddate between '01-Jan-2018' and
                         '31-Dec-2018' then
                     t.cr
                    else
                     0
                  end) - SUM(case
                                when t.balancetype = 'D' and t.enddate between '01-Jan-2018' and
                                     '31-Dec-2018' then
                                 t.running_dr
                                else
                                 0
                              end)) AS BALANCE_2018
      
        from t_Au_Preinfo_Gldayendbalance t
       inner join t_auditee_entities ee
          on ee.entity_id = t.enitity_id
       inner join t_auditee_entities_maping e
          on e.entity_id = ee.entity_id
       INNER JOIN t_au_preinfo_gl_type GT
          ON GT.DESCRIPTION = T.DESCRIPTION
       inner join t_user m
          on m.entity_id = ee.entity_id
      --where 
       GROUP BY GT.GL_TYPEID, T.code, GT.DESCRIPTION
       ORDER BY GT.GL_TYPEID;
  
  end p_getglheadsummary_dash_Yearly;

/*procedure P_Session_Kill is
  
  sid number := 0;
  spid number := 0;
  begin
  
 \* for s in (SELECT s.sid, s.serial#, s.spid
FROM  v$session s
WHERE p.addr = s.paddr
AND s.username = 'ZTBLAIS' and s.status in ('INACTIVE')) loop

 select count(s.sid) into sid from v$session ss where ss.USERNAME =  'ZTBLAIS' and s.status in ('INACTIVE');
  
  end loop;*\
  end P_Session_Kill;*/

end PKG_AIS;

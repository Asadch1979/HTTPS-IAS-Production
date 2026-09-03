DECLARE
  V_COUNT NUMBER := 0;
BEGIN
  SELECT COUNT(*)
    INTO V_COUNT
    FROM USER_TABLES
   WHERE TABLE_NAME = 'T_AU_ENG_ENTITY_SHIFT_HIST';

  IF V_COUNT = 0 THEN
    EXECUTE IMMEDIATE '
      CREATE TABLE T_AU_ENG_ENTITY_SHIFT_HIST
      (
        ID                   NUMBER PRIMARY KEY,
        ENG_ID               NUMBER NOT NULL,
        OLD_ENTITY_ID        NUMBER NOT NULL,
        NEW_ENTITY_ID        NUMBER NOT NULL,
        REASON               VARCHAR2(1000) NOT NULL,
        PPNO                 NUMBER NOT NULL,
        ROLE_ID              NUMBER NOT NULL,
        SHIFTED_ON           DATE DEFAULT SYSDATE NOT NULL,
        PLAN_ENG_ROWS        NUMBER DEFAULT 0 NOT NULL,
        OBSERVATION_ROWS     NUMBER DEFAULT 0 NOT NULL,
        AIS_OBSERVATION_ROWS NUMBER DEFAULT 0 NOT NULL,
        OBS_ASSIGNEDTO_ROWS  NUMBER DEFAULT 0 NOT NULL,
        TEAM_TASKLIST_ROWS   NUMBER DEFAULT 0 NOT NULL
      )';
  END IF;

  FOR C IN (SELECT COLUMN_NAME
              FROM USER_TAB_COLUMNS
             WHERE TABLE_NAME = 'T_AU_ENG_ENTITY_SHIFT_HIST'
               AND COLUMN_NAME IN ('DSA_ROWS',
                                   'OBSERVATION_MAN_ROWS',
                                   'BRANCH_RISK_RATING_ROWS',
                                   'COSO_RATING_DEPT_ROWS',
                                   'RISK_BRANCH_WISE_ROWS')) LOOP
    EXECUTE IMMEDIATE 'ALTER TABLE T_AU_ENG_ENTITY_SHIFT_HIST DROP COLUMN ' || C.COLUMN_NAME;
  END LOOP;
END;
/

DECLARE
  V_COUNT NUMBER := 0;
BEGIN
  SELECT COUNT(*)
    INTO V_COUNT
    FROM USER_SEQUENCES
   WHERE SEQUENCE_NAME = 'SEQ_AU_ENG_ENTITY_SHIFT_HIST';

  IF V_COUNT = 0 THEN
    EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_AU_ENG_ENTITY_SHIFT_HIST START WITH 1 INCREMENT BY 1 NOCACHE';
  END IF;
END;
/

create or replace package PKG_AD is

  TYPE t_cursor IS REF CURSOR;
  procedure RESET_USER_PASSWORD(PPNUMBER  IN T_USER.PPNO%TYPE,
                                CNIC      in varchar2,
                                PASS      IN T_USER.PASSWORD%TYPE,
                                io_cursor OUT t_cursor);
  procedure UPDATE_USERS(PPNUMBER      IN T_USER.PPNO%TYPE,
                         PASS          IN T_USER.PASSWORD%TYPE,
                         IS_ACTIVE     IN T_USER.ISACTIVE%TYPE,
                         ROLEID        IN T_USER_MAPING.ROLE_ID%TYPE,
                         entityid      in t_user.entity_id%type,
                         EMAIL_ADDRESS in varchar2,
                         io_cursor     OUT t_cursor);

  procedure user_maping(USER_ID in t_user_maping.userid%type,
                        ROLE_ID in t_user_maping.role_id%type,
                        PPNO    in t_user_maping.ppno%type);

  procedure P_add_new_user(enc_pass  in varchar2,
                           role_id   in number,
                           P_NO      in number,
                           ent_id    in number,
                           io_cursor OUT t_cursor);

  procedure P_UpdateUser(USER_ID  in t_user.userid%type,
                         enc_pass in t_user.password%type,
                         role_id  in t_user_maping.role_id%type,
                         PPNO     in t_user.ppno%type,
                         ISACTIVE in t_user.isactive%type);

  Procedure P_Get_observvation_no(obs_id in number, io_cursor OUT t_cursor);

  Procedure P_Update_observation_no(m_no      in number,
                                    D_no      in number,
                                    P_no      in number,
                                    obs_id    in number,
                                    io_cursor OUT t_cursor);

  procedure P_UPDATE_ENG_DATE(ENGID     IN NUMBER,
                              ST_DATE   IN DATE,
                              ED_DATE   IN DATE,
                              io_cursor OUT t_cursor);

  procedure p_get_allusers(ENTITYID  in t_user.ppno%type,
                           EMAIL     in v_service_employeeinfo.EMAIL%type,
                           GROUPID   in t_groups.group_id%type,
                           PPNUMBER  in t_user.ppno%type,
                           LOGINNAME in t_user.login_name%type,
                           ENT_ID    in number,
                           P_NO      in number,
                           R_ID      in number,
                           io_cursor OUT t_cursor);

  procedure P_GetAllTopMenus(ENT_ID    in number,
                             P_NO      in number,
                             R_ID      in number,
                             io_cursor OUT t_cursor);

  procedure P_GetAssignedMenuPages(groupId   in t_menu_pages_groupmap.group_id%type,
                                   menuId    in T_MENU_PAGES.MENU_ID%type,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure P_AddGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                          PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type);

  procedure P_RemoveGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                             PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type);

  procedure P_GetAllMenuPages(menuId    in T_MENU_PAGES.MENU_ID%type,
                              ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor);

  procedure P_updateAllMenuPages(menuId in T_MENU_PAGES.MENU_ID%type,
                                 p_id   in T_MENU_PAGES.MENU_ID%type,
                                 ENT_ID in number,
                                 P_NO   in number,
                                 R_ID   in number);

  procedure P_GetGroups(ENT_ID    in number,
                        P_NO      in number,
                        R_ID      in number,
                        io_cursor OUT t_cursor);

  procedure P_GetRoleResponsibilities(io_cursor OUT t_cursor);

  procedure P_Group_Update(P_GROUPID           in t_groups.group_id%type,
                           P_GROUP_DESCRIPTION in t_groups.description%type,
                           P_GROUP_NAME        in t_groups.group_name%type,
                           P_ISACTIVE          in t_groups.status%type,
                           ENT_ID              in number,
                           P_NO                in number,
                           R_ID                in number);

  procedure p_AddGroup(GROUP_DESCRIPTION in t_groups.description%type,
                       GROUP_NAME        in t_groups.group_name%type,
                       ISACTIVE          in t_groups.status%type,
                       ENT_ID            in number,
                       P_NO              in number,
                       R_ID              in number);

  procedure P_AddGroupMenuAssignment(roleid  in T_USER_GROUP_MAP.ROLE_ID%type,
                                     menuid  in T_USER_GROUP_MAP.MENU_ID%type,
                                     pageids in T_USER_GROUP_MAP.PAGE_IDS%type);

  procedure P_RemoveGroupMenuAssignment(roleid in T_USER_GROUP_MAP.role_id%type,
                                        menuid in T_USER_GROUP_MAP.MENU_ID%type);

  procedure P_AddAuditEntity(AUDITABLE      in t_auditee_ent_types.auditable%type,
                             ENTITYTYPEDESC in t_auditee_ent_types.entitytypedesc%type);

  procedure P_GetAuditSubEntities(io_cursor OUT t_cursor);

  procedure P_UpdateENTITIEES(P_NO          in number,
                              ENT_ID        IN NUMBER,
                              R_ID          IN NUMBER,
                              E_CODE        IN NUMBER,
                              E_NAME        IN VARCHAR2,
                              E_DISCRIPTION IN VARCHAR2,
                              E_AUDITEDBY   IN NUMBER,
                              E_TYPEID      IN NUMBER,
                              E_STATUS      IN CHAR,
                              E_AUDITABLE   IN VARCHAR2,
                              ENTITYID      IN NUMBER,
                              io_cursor     OUT t_cursor);

  procedure P_InsertENTITIEES(P_NO          in number,
                              ENT_ID        in number,
                              R_ID          in number,
                              E_CODE        IN NUMBER,
                              E_NAME        IN VARCHAR2,
                              E_DISCRIPTION IN VARCHAR2,
                              E_AUDITEDBY   IN NUMBER,
                              E_TYPEID      IN NUMBER,
                              E_STATUS      IN CHAR,
                              E_AUDITABLE   IN VARCHAR2,
                              io_cursor     OUT t_cursor);

  procedure P_Getrealtionshiptype(R_ID      in number,
                                  ENT_ID    in number,
                                  P_NO      in number,
                                  PAGE_ID   in number,
                                  io_cursor OUT t_cursor);

  procedure P_Get_Entity_type(ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              PG_ID     in number,
                              io_cursor OUT t_cursor);

  procedure p_get_audited_by(io_cursor OUT t_cursor);

  procedure P_Getparentrepoffice(rid       in number,
                                 ENT_ID    in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_Getchildposting(erid in number, io_cursor OUT t_cursor);

  procedure P_GetAuditZones(ENTITYID  in t_auditee_entities.entity_id%type,
                            io_cursor OUT t_cursor);

  procedure P_GetBranches(Zone_Id in number, io_cursor OUT t_cursor);

  procedure P_GetZones(io_cursor OUT t_cursor);

  procedure P_GetZonesForHoMointoring(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      io_cursor OUT t_cursor);

  procedure P_GetBranchSizes(io_cursor OUT t_cursor);

  procedure P_GetControlViolations(io_cursor OUT t_cursor);

  procedure P_GetEntitees(ENTITYID  IN NUMBER,
                          TYPEID    IN NUMBER,
                          io_cursor OUT t_cursor);

  procedure P_GetEntitees_for_update(ENTITYID  NUMBER,
                                     P_NO      number,
                                     ENT_ID    number,
                                     TYPEID    NUMBER,
                                     Ro_ID     number,
                                     io_cursor OUT t_cursor);

  procedure P_GetEntitees_for_update_comp(E_ENTITY_ID number,
                                          P_NO        NUMBER,
                                          ENT_ID      number,
                                          R_ID        NUMBER,
                                          io_cursor   OUT t_cursor);

  procedure P_GetEntitees_for_update_authorization(E_ENTITY_ID number,
                                                   E_up_status VARCHAR2,
                                                   P_NO        NUMBER,
                                                   R_ID        NUMBER,
                                                   IND         VARCHAR2,
                                                   io_cursor   OUT t_cursor);

  PROCEDURE P_UPDATE_ENTITIES(E_entity_id     NUMBER,
                              E_code          VARCHAR2,
                              E_name          VARCHAR2,
                              E_active        VARCHAR2,
                              E_auditable     VARCHAR2,
                              E_address       VARCHAR2,
                              E_telephone     VARCHAR2,
                              E_email_address VARCHAR2,
                              E_risk_id       NUMBER,
                              E_size_id       NUMBER,
                              E_up_status     VARCHAR2,
                              P_NO            NUMBER,
                              R_ID            NUMBER,
                              IND             VARCHAR2,
                              io_cursor       OUT t_cursor);

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

  procedure P_GetRisks(io_cursor OUT t_cursor);

  procedure P_GetRiskProcessDetails(procId    IN NUMBER,
                                    io_cursor OUT t_cursor);

  procedure P_get_checklist_update_byid(cd_id     IN NUMBER,
                                        io_cursor OUT t_cursor);

  procedure P_get_sub_checklist_update_byid(Sid       IN NUMBER,
                                            io_cursor OUT t_cursor);

  procedure P_get_checklist_update_byid_ref(cd_id     IN NUMBER,
                                            io_cursor OUT t_cursor);

  procedure p_Get_updated_Sub_Checklist_for_review(statusId  IN NUMBER,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  procedure p_Get_updated_Checklist_for_review(statusId  IN NUMBER,
                                               ENT_ID    in number,
                                               P_NO      in number,
                                               R_ID      in number,
                                               io_cursor OUT t_cursor);

  procedure p_Get_sub_Checklist_maker(processid in number,
                                      io_cursor OUT t_cursor);

  procedure p_GetChecklistDetailBySubProcessId(subProcessId in number,
                                               ENT_ID       in number,
                                               P_NO         in number,
                                               R_ID         in number,
                                               io_cursor    OUT t_cursor);

  procedure p_GetChecklistDetail_ref(ENT_ID    in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor);

  procedure P_GetChecklistDetailById(d_id      in number,
                                     io_cursor OUT t_cursor);

  procedure P_audit_checklist(p_name    in varchar2,
                              c_sec     in number,
                              c_weight  in varchar2,
                              RISK_ID   in number,
                              ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor);

  procedure P_audit_checklist_update(tid       in number,
                                     p_name    in varchar2,
                                     active    in varchar2,
                                     c_sec     in number,
                                     c_weight  in varchar2,
                                     ENT_ID    in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor);

  procedure P_audit_checklist_sub(p_ID        in number,
                                  TITLE       in varchar2,
                                  s_sec       in number,
                                  s_weight    in varchar2,
                                  ENTITY_TYPE in varchar2,
                                  ENT_ID      in number,
                                  P_NO        in number,
                                  R_ID        in number,
                                  io_cursor   OUT t_cursor);

  procedure P_get_checklistdetail_for_subchecklist(sid       in number,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  procedure P_audit_checklist_sub_update(TID         in number,
                                         N_TID       in number,
                                         sid         in number,
                                         TITLE       in varchar2,
                                         s_sec       in number,
                                         s_wieght    in number,
                                         ENTITY_TYPE in number,
                                         ENT_ID      in number,
                                         P_NO        in number,
                                         R_ID        in number,
                                         io_cursor   OUT t_cursor);

  Procedure p_get_annexure_process(io_cursor OUT t_cursor);

  Procedure p_get_annexure(ENT_ID    in number,
                           P_NO      in number,
                           R_ID      in number,
                           io_cursor OUT t_cursor);

  Procedure p_update_annexure(ENT_ID        in number,
                              P_NO          in number,
                              R_ID          in number,
                              anexx         in number,
                              title         in varchar2,
                              risk_id       in number,
                              owner         in number,
                              FUNCTION_ID_1 in number,
                              FUNCTION_ID_2 in number,
                              process_id    in number,
                              max_num       in varchar2,
                              weightage_num in varchar2,
                              gravity_num   in varchar2,
                              io_cursor     OUT t_cursor);

  Procedure P_add_annexure(ENT_ID        in number,
                           P_NO          in number,
                           R_ID          in number,
                           code          in varchar2,
                           title         in varchar2,
                           risk_id       in number,
                           owner         in number,
                           FUNCTION_ID_1 in number,
                           FUNCTION_ID_2 in number,
                           PROCESS_ID    IN NUMBER,
                           max_num       in varchar2,
                           weightage_num in varchar2,
                           gravity_num   in varchar2,
                           io_cursor     OUT t_cursor);

  procedure P_audit_checklist_detail(p_id          in number,
                                     SID           in number,
                                     DESCRIPTION   in varchar2,
                                     VID           in number,
                                     CONTROL_OWNER in number,
                                     role          in number,
                                     RISK          in number,
                                     Annexure      in number,
                                     ENT_ID        in number,
                                     P_NO          in number,
                                     R_ID          in number,
                                     io_cursor     OUT t_cursor);

  procedure audit_checklist_detail_update(Did           in number,
                                          SID           in number,
                                          DESCRIPTION   in varchar2,
                                          VID           in number,
                                          CONTROL_OWNER in number,
                                          role          in number,
                                          RISK          in number,
                                          Annexure      in number,
                                          ENT_ID        in number,
                                          P_NO          in number,
                                          R_ID          in number,
                                          io_cursor     OUT t_cursor);

  procedure audit_checklist_details_log(ppnumber in t_audit_checklist_details_log.user_id%type,
                                        comments in t_audit_checklist_details_log.comments%type,
                                        t_id     in t_audit_checklist_details_log.t_id%type);

  procedure P_Recommend_Checklist_By_Reviewer(DID           in number,
                                              SID           in number,
                                              DESCRIPTION   in varchar2,
                                              VID           in number,
                                              CONTROL_OWNER in number,
                                              role          in number,
                                              RISK          in number,
                                              Annexure      in number,
                                              ENT_ID        in number,
                                              P_NO          in number,
                                              R_ID          in number,
                                              T_ID          in number,
                                              COMMENTS      in varchar2,
                                              io_cursor     OUT t_cursor);

  procedure P_RefferedBack_checklist_By_Reviewer(T_ID      in number,
                                                 COMMENTS  IN varchar2,
                                                 ENT_ID    in number,
                                                 P_NO      in number,
                                                 R_ID      in number,
                                                 io_cursor OUT t_cursor);

  procedure p_RefferedBack_Sub_checklist_By_Reviewer(SID       in number,
                                                     COMMENTS  IN varchar2,
                                                     ENT_ID    in number,
                                                     R_ID      in number,
                                                     P_NO      in number,
                                                     io_cursor OUT t_cursor);

  procedure p_Approved_Sub_Process_By_Authorizer(SID       in number,
                                                 COMMENTS  IN varchar2,
                                                 ENT_ID    in number,
                                                 P_NO      in number,
                                                 R_ID      in number,
                                                 io_cursor OUT t_cursor);

  procedure p_RefferedBack_checklist_By_Authorizer(T_ID      in number,
                                                   COMMENTS  IN varchar2,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  procedure p_approve_checklist_By_Authorizer(T_ID      in number,
                                              COMMENTS  in varchar2,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor);

  procedure P_GetLatestCommentsOnProcess(procId    IN NUMBER,
                                         io_cursor OUT t_cursor);

  procedure p_get_audit_team_postchanges(ENT_ID    in number,
                                         P_NO      in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetAuditTeamsForEngReversal(AuditedByDept IN NUMBER,
                                           CurrentTeamID IN NUMBER,
                                           P_NO          IN NUMBER,
                                           R_ID          IN NUMBER,
                                           io_cursor     OUT t_cursor);

  procedure p_audit_team_postchanges(ENGID     in number,
                                     PPNO      in number,
                                     Teamid    in number,
                                     audid     in number,
                                     teamname  in varchar2,
                                     io_cursor OUT t_cursor);

  procedure p_get_audit_engagement(ent_id    in number,
                                   io_cursor OUT t_cursor);

  procedure P_GET_SHIFTABLE_AUDIT_ENGAGEMENT(ENT_ID    IN NUMBER,
                                             IO_CURSOR OUT T_CURSOR);

  procedure p_get_audit_engagement_status(engid     in number,
                                          io_cursor OUT t_cursor);

  procedure p_audit_engagement_reversal(engid     in number,
                                        sid       in number,
                                        p_id      in number,
                                        comments  in varchar2,
                                        P_NO      in number,
                                        io_cursor OUT t_cursor);

  procedure P_SHIFT_ENGAGEMENT_ENTITY(P_ENG_ID        IN NUMBER,
                                      P_NEW_ENTITY_ID IN NUMBER,
                                      P_PPNO          IN NUMBER,
                                      P_ROLE_ID       IN NUMBER,
                                      P_REASON        IN VARCHAR2,
                                      IO_CURSOR       OUT T_CURSOR);

  procedure p_get_audit_observtion_status(io_cursor OUT t_cursor);

  procedure p_get_audit_observtion(ENGID in number, io_cursor OUT t_cursor);

  Procedure p_audit_observation_reversal(ENGID     in number,
                                         obs_id    in number,
                                         S_ID      in number,
                                         P_NO      in number,
                                         io_cursor OUT t_cursor);

  Procedure p_get_audit_observation_number(ENGID     in number,
                                           Memo      in number,
                                           io_cursor OUT t_cursor);

  Procedure p_get_audit_observation_num_obs(ENGID     in number,
                                            Memo      in number,
                                            io_cursor OUT t_cursor);

  Procedure p_get_audit_observation_num_text(ENGID     in number,
                                             Obs_id    in number,
                                             io_cursor OUT t_cursor);

  Procedure p_get_audit_observation_num_assigned(ENGID     in number,
                                                 Obsid     in number,
                                                 io_cursor OUT t_cursor);

  Procedure p_audit_observation_assingment(ENGID  in number,
                                           Memo   in number,
                                           ent_id in number);

  Procedure p_audit_observation_reversal_closing(ENGID in number);

  procedure P_get_auditee_entities(Ent_id    in number,
                                   t_id      in number,
                                   io_cursor OUT t_cursor);

  procedure P_add_auditee_entities(cbas_code        in number,
                                   e_code           in number,
                                   e_name           in varchar2,
                                   status           in varchar2,
                                   t_id             in number,
                                   audited_by_id    in number,
                                   cost_center_code in number,
                                   auditable_status in varchar2,
                                   io_cursor        OUT t_cursor);

  procedure P_GetRiskProcessTransactions(procDetailId  IN NUMBER,
                                         transactionId IN NUMBER,
                                         io_cursor     OUT t_cursor);

  procedure P_GetAuditeeEntityTypes(ENTITYID  IN NUMBER,
                                    io_cursor OUT t_cursor);

  procedure P_GetAuditeeTypes(io_cursor OUT t_cursor);

  procedure P_get_auditee_entities_mapping(Ent_id    in number,
                                           t_id      in number,
                                           io_cursor OUT t_cursor);

  Procedure P_ADD_ENTITIES_MAPPING(P_ENT_ID    IN NUMBER,
                                   ENT_ID      in number,
                                   RELATION_ID in number,
                                   io_cursor   OUT t_cursor);

  Procedure P_UPDATE_ENTITIES_MAPPING(P_ENT_ID    IN NUMBER,
                                      RELATION_ID in number,
                                      ENT_ID      in number,
                                      io_cursor   OUT t_cursor);

  Procedure P_GET_HR_ENTITIES(ENT_CODE  in number,
                              ENT_NAME  in varchar2,
                              io_cursor OUT t_cursor);

  Procedure P_GET_AIS_ENTITIES(ENT_CODE  in number,
                               ENT_NAME  in varchar2,
                               ENT_TYPE  in number,
                               io_cursor OUT t_cursor);

  Procedure P_GET_ERP_ENTITIES(ENT_CODE  in number,
                               ENT_NAME  in varchar2,
                               io_cursor OUT t_cursor);

  Procedure P_GET_CBAS_ENTITIES(ENT_CODE  in number,
                                ENT_NAME  in varchar2,
                                io_cursor OUT t_cursor);

  Procedure P_GET_ENTITIES_MAPPING_CODE(ENT_CODE  in number,
                                        io_cursor OUT t_cursor);

  Procedure P_ADD_ENTITIES_MAPPING_CODE(ENT_CODE  in number,
                                        CBAS      IN NUMBER,
                                        HRMS      IN NUMBER,
                                        ERP       IN NUMBER,
                                        HYPER     IN NUMBER,
                                        CDMS      IN NUMBER,
                                        CPMS      IN NUMBER,
                                        io_cursor OUT t_cursor);

  Procedure P_UPDATE_ENTITIES_MAPPING_CODE(ENT_CODE  in number,
                                           CBAS      IN NUMBER,
                                           HRMS      IN NUMBER,
                                           ERP       IN NUMBER,
                                           HYPER     IN NUMBER,
                                           CDMS      IN NUMBER,
                                           CPMS      IN NUMBER,
                                           io_cursor OUT t_cursor);

  procedure p_get_auditee_engagement(ent_id    in number,
                                     period    in number,
                                     io_cursor OUT t_cursor);

  procedure P_GetAuditeeRisk(ENT_ID IN NUMBER, io_cursor OUT t_cursor);

  procedure P_GetAuditeeRisk_details(ENT_ID    IN NUMBER,
                                     io_cursor OUT t_cursor);

  procedure P_Get_Entity_Risk(ENT_TYP   IN NUMBER,
                              period    in number,
                              io_cursor OUT t_cursor);

  procedure p_Get_sub_Checklist_MERGER_FOR_REVIEW(SID       in number,
                                                  ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor);

  procedure p_Get_Checklist_MERGER_FOR_REVIEW(CID       in number,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor);

  procedure p_Get_ChecklistDetail_FOR_DUPLICATE(subProcessId in number,
                                                ENT_ID       in number,
                                                P_NO         in number,
                                                R_ID         in number,
                                                io_cursor    OUT t_cursor);

  Procedure P_UPDATE_CHECKLIST_DETAILS(C_ID       IN NUMBER,
                                       SID        in number,
                                       check_list in varchar2,
                                       io_cursor  OUT t_cursor);

  Procedure P_REMOVE_DUPLICATE_CHECKLIST_DETAILS(C_ID IN NUMBER,
                                                 D_ID in number);

  Procedure p_merge_sub_checklist(sid       in number,
                                  msid      in number,
                                  io_cursor OUT t_cursor);

  Procedure p_merge_checklist(cid       in number,
                              mcid      in number,
                              io_cursor OUT t_cursor);

  Procedure P_GET_DUPLICATE_CHECKLIST_DETAILS_DROPDOWN(io_cursor OUT t_cursor);

  Procedure P_GET_DUPLICATE_CHECKLIST_DETAILS(D_ID      in number,
                                              io_cursor OUT t_cursor);

  Procedure P_GET_DUPLICATE_CHECKLIST_DETAILS_COUNT(D_ID      in number,
                                                    io_cursor OUT t_cursor);

  Procedure P_AUTHORIZE_MERGER_CHECKLIST(C_ID      in number,
                                         M_CID     IN NUMBER,
                                         io_cursor OUT t_cursor);

  Procedure P_AUTHORIZE_MERGER_CHECKLIST_SUB(SID       in number,
                                             M_SID     IN NUMBER,
                                             io_cursor OUT t_cursor);

  Procedure P_AUTHORIZE_DUPLICATE_CHECKLIST_DETAILS(D_ID      in number,
                                                    io_cursor OUT t_cursor);

  Procedure P_Del_User_Data_in_temp_table(io_cursor OUT t_cursor);

  Procedure P_get_user_role_type(D_CODE in number, io_cursor OUT t_cursor);

  procedure p_update_role_hr(D_Code    in number,
                             g_id      in number,
                             io_cursor OUT t_cursor);

  Procedure P_get_new_user(io_cursor OUT t_cursor);

  Procedure P_UPDATE_NEW_USER(P_NO in number, io_cursor OUT t_cursor);

  procedure P_Get_details_for_entity_shifting(ENT_ID    in number,
                                              io_cursor OUT t_cursor);

  procedure P_Get_Entities_types(io_cursor OUT t_cursor);

  procedure P_update_Entities_types(aut_id         in number,
                                    e_code         in number,
                                    e_desc         in varchar2,
                                    e_auditable    in varchar2,
                                    e_auditby_code in number,
                                    e_auditby_id   in number,
                                    e_type         in varchar2,
                                    e_autid        in number,
                                    io_cursor      OUT t_cursor);

  procedure P_Get_Entities_Relationship(R_ID      in number,
                                        io_cursor OUT t_cursor);

  Procedure P_Update_Entities_Relationship(r_ship_id in number,
                                           p_type_id in number,
                                           c_type_id in number,
                                           active    in varchar2,
                                           p_name    in varchar2,
                                           c_name    in varchar2,
                                           map_id    in number,
                                           a_id      in number,
                                           io_cursor OUT t_cursor);

  Procedure P_GET_ENTITIES_MAPPING_REPORTING(ent_id        in number,
                                             P_TYPE        IN NUMBER,
                                             C_TYPE        IN NUMBER,
                                             REALTION_TYPE IN NUMBER,
                                             ind           IN VARCHAR2,
                                             io_cursor     OUT t_cursor);

  Procedure P_GET_ENTITIES_MAPPING(ent_id        in number,
                                   P_TYPE        IN NUMBER,
                                   C_TYPE        IN NUMBER,
                                   REALTION_TYPE IN NUMBER,
                                   ind           IN VARCHAR2,
                                   io_cursor     OUT t_cursor);

  Procedure P_ADD_ENTITIES_MAPPING_REPORTING(P_ID          IN NUMBER,
                                             P_CODE        IN NUMBER,
                                             C_CODE        IN NUMBER,
                                             C_ID          IN NUMBER,
                                             AUDIT_BY      IN NUMBER,
                                             e_STATUS      IN VARCHAR2,
                                             PAR_NAME      IN VARCHAR2,
                                             CH_NAME       IN VARCHAR2,
                                             P_TYPE        IN NUMBER,
                                             C_TYPE        IN NUMBER,
                                             RELATION_TYPE IN NUMBER,
                                             io_cursor     OUT t_cursor);

  Procedure P_get_entities(p_type    in number,
                           c_type    in number,
                           io_cursor OUT t_cursor);

  Procedure P_update_entity_shifting_plan(p_id      in number,
                                          io_cursor OUT t_cursor);

  Procedure P_update_entity_shifting_engagement(p_id      in number,
                                                E_id      in number,
                                                io_cursor OUT t_cursor);

  Procedure P_Add_Entity_shifting(Old_Ent_id in number,
                                  new_ent_id in number,
                                  P_NO       in number,
                                  ENT_ID     in number,
                                  R_ID       in number,
                                  cir_no     in varchar2,
                                  cir_attach in clob,
                                  cir_date   in date,
                                  io_cursor  OUT t_cursor);

  procedure P_get_roles_for_compliance_flow(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure P_get_ent_types_for_compliance_flow(ENT_ID    in number,
                                                P_NO      in number,
                                                R_ID      in number,
                                                io_cursor OUT t_cursor);

  procedure P_get_ent_types_for_hr_designation(ENT_ID    in number,
                                               P_NO      in number,
                                               R_ID      in number,
                                               io_cursor OUT t_cursor);

  procedure P_get_compliance_statuses_for_compliance_flow(io_cursor OUT t_cursor);

  PROCEDURE P_get_group_prev_next_stage(E_TYPE    IN NUMBER,
                                        G_ID      IN NUMBER,
                                        io_cursor OUT t_cursor);

  PROCEDURE P_get_entity_type_compliance_flow(E_TYPE    IN NUMBER,
                                              G_ID      in number,
                                              io_cursor OUT t_cursor);

  PROCEDURE P_add_update_compliance_flow(f_id          in number,
                                         TYPE_ID       IN NUMBER,
                                         GROUP_ID      IN NUMBER,
                                         P_GROUP_ID    IN NUMBER,
                                         N_GROUP_ID    IN NUMBER,
                                         C_UP_STATUS   IN NUMBER,
                                         C_DOWN_STATUS IN NUMBER,
                                         io_cursor     OUT t_cursor);

  Procedure P_GET_HR_DESIGNATION_RIGHT(io_cursor OUT t_cursor);

  Procedure P_UPDATE_HR_DESIGNATION_RIGHT(M_ID                IN NUMBER,
                                          HR_DES_CODE         in number,
                                          AIS_GROUP_ID        IN NUMBER,
                                          AIS_SUB_ENTITY_TYPE IN VARCHAR2,
                                          io_cursor           OUT t_cursor);

  Procedure P_ADD_HR_DESIGNATION_RIGHT(HR_DES_CODE         in number,
                                       AIS_GROUP_ID        IN NUMBER,
                                       AIS_SUB_ENTITY_TYPE in VARCHAR2,
                                       io_cursor           OUT t_cursor);

  Procedure P_GET_OBS_STATUS(io_cursor OUT t_cursor);

  Procedure P_ADD_OBS_STATUS(S_NAME    in varchar2,
                             ACTIVE    IN varchar2,
                             S_CODE    in VARCHAR2,
                             SATISFY   in VARCHAR2,
                             io_cursor OUT t_cursor);

  Procedure P_UPDATE_OBS_STATUS(S_ID      in number,
                                S_NAME    in varchar2,
                                ACTIVE    IN varchar2,
                                S_CODE    in VARCHAR2,
                                SATISFY   in VARCHAR2,
                                io_cursor OUT t_cursor);

  Procedure P_GET_ENTITIES_AUDIT_DEPARTMENT(io_cursor OUT t_cursor);

  Procedure P_UPDATE_ENTITIES_AUDIT_DEPARTMENT(R_ID      in number,
                                               D_ID      in number,
                                               D_CODE    IN number,
                                               D_NAME    in VARCHAR2,
                                               STATUS    in VARCHAR2,
                                               CBAS_CODE in number,
                                               ENT_ID    in number,
                                               AUD_ID    in number,
                                               AUDITOR   in varchar2,
                                               io_cursor OUT t_cursor);

  Procedure P_GET_ALL_MENU(io_cursor OUT t_cursor);

  Procedure P_GET_SUB_MENUS(M_ID in number, io_cursor OUT t_cursor);

  Procedure P_ADD_NEW_SUB_MENU(M_ID      in number,
                               SM_NAME   in varchar2,
                               SM_ORDER  in number,
                               SM_STATUS in varchar2,
                               SM_DESC   in varchar2,
                               io_cursor OUT t_cursor);

  Procedure P_UPDATE_SUB_MENU(SM_ID     in number,
                              M_ID      in number,
                              SM_NAME   in varchar2,
                              SM_ORDER  in number,
                              SM_STATUS in varchar2,
                              SM_DESC   in varchar2,
                              io_cursor OUT t_cursor);

  Procedure P_GET_ALL_PAGES(M_ID      in number,
                            SM_ID     in number,
                            io_cursor OUT t_cursor);

  Procedure P_ADD_NEW_PAGE(M_ID        in number,
                           SM_ID       in number,
                           P_NAME      in varchar2,
                           P_PAGE_KEY  in Varchar2,
                           P_PAGE_URL  in varchar2,
                           P_PATH      in varchar2,
                           P_ORDER     in number,
                           P_STATUS    in varchar2,
                           P_HIDE_MENU in number,
                           io_cursor   OUT t_cursor);

  Procedure P_UPDATE_PAGE(P_ID        in number,
                          M_ID        in number,
                          SM_ID       in number,
                          P_NAME      in varchar2,
                          P_PAGE_KEY  in varchar2,
                          P_PAGE_URL  in varchar2,
                          P_PATH      in varchar2,
                          P_ORDER     in number,
                          P_STATUS    in varchar2,
                          P_HIDE_MENU in number,
                          io_cursor   OUT t_cursor);

  Procedure P_GET_COMPLIANCE_OFFICE(io_cursor OUT t_cursor);

  Procedure P_UPDATE_ENTITY_COMP(R_ID       in number,
                                 ENT_ID     in number,
                                 Auditor    in number,
                                 compliance in number,
                                 io_cursor  out t_cursor);

  Procedure P_GET_ENTITY_FOR_PARA_Reconsilation(R_ID      in number,
                                                ENT_ID    in number,
                                                io_cursor out t_cursor);

  procedure P_add_branch_risk_rating(ENGID     in number,
                                     io_cursor out t_cursor);

  procedure p_get_traditional_risk_rating(ENGID     in number,
                                          io_cursor out t_cursor);

  Procedure p_get_new_risk_model(eng_id in number, io_cursor out t_cursor);

  Procedure P_GET_compliance_hierarchy(io_cursor out t_cursor);

  Procedure P_GET_SUBCHECKILIST(io_cursor out t_cursor);

  Procedure P_UPDATE_COM_OFFICER(ENT_ID    number,
                                 AP_P_NO   number,
                                 RE_P_NO   number,
                                 E_COM_KEY varchar2,
                                 io_cursor out t_cursor);

  Procedure P_ADD_COM_OFFICER(ENT_ID    number,
                              AP_P_NO   number,
                              RE_P_NO   number,
                              io_cursor out t_cursor);
  Procedure P_SHIFTING_AUDIT_PARA(NEW_P_ID    in number,
                                  OLD_P_ID    in number,
                                  P_IND       in varchar2,
                                  DEST_ENT_ID in number,
                                  P_NO        in number,
                                  R_ID        in number,
                                  ENT_ID      in number,
                                  io_cursor   out t_cursor);
  Procedure P_Shift_BR_to_islamic(Old_br    number,
                                  new_br    number,
                                  io_cursor out t_cursor);

  Procedure P_GET_GM_OFFICE(io_cursor out t_cursor);
  Procedure P_GET_RPT_OFFICE(io_cursor out t_cursor);
  Procedure P_UPDATE_GM_OFFICE_RELATIONSHIP(GM        number,
                                            ENT       number,
                                            io_cursor out t_cursor);
  Procedure P_UPDATE_RPT_OFFICE_RELATIONSHIP(RPT       number,
                                             ENT       number,
                                             io_cursor out t_cursor);

  Procedure P_get_latest_para_details(ENT number, io_cursor out t_cursor);

  Procedure P_Update_para_AIS_post_compliance(ca_com_id         NUMBER,
                                              ca_audit_period   VARCHAR2,
                                              ca_gist_of_paras  VARCHAR2,
                                              ca_audited_by     NUMBER,
                                              ca_entity_type_id NUMBER,
                                              ca_com_cycle      NUMBER,
                                              ca_com_status     NUMBER,
                                              ca_com_stage      NUMBER,
                                              ca_para_status    NUMBER,
                                              ca_para_no        VARCHAR2,
                                              ca_ind            VARCHAR2,
                                              ca_risk           NUMBER,
                                              io_cursor         out t_cursor);

  Procedure P_Get_Audit_EMP(P_NO      in number,
                            R_ID      in number,
                            ENT_ID    in number,
                            io_cursor out t_cursor);

  procedure P_get_hr_rank(io_cursor out t_cursor);

  Procedure P_get_certification(io_cursor out t_cursor);

  Procedure P_get_hr_designation(io_cursor out t_cursor);

  Procedure P_get_qualification(io_cursor out t_cursor);

  Procedure P_get_qualification_specialization(io_cursor out t_cursor);

  Procedure P_get_hr_posting(io_cursor out t_cursor);

  Procedure P_Get_Audit_Manpower(P_NO      in number,
                                 R_ID      in number, /*
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           ENT_ID      in number,*/
                                 io_cursor out t_cursor);

  PROCEDURE P_GET_PUBLIC_HOLIDAYS(p_year    IN NUMBER DEFAULT NULL,
                                  io_cursor OUT SYS_REFCURSOR);
  PROCEDURE P_GET_PUBLIC_HOLIDAY_DAY(p_day     IN date,
                                     io_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_INSERT_PUBLIC_HOLIDAY(p_holiday_date IN DATE,
                                    p_is_weekend   IN CHAR DEFAULT 'N',
                                    p_is_holiday   IN CHAR DEFAULT 'N',
                                    p_holiday_name IN VARCHAR2 DEFAULT NULL,
                                    p_id           IN NUMBER DEFAULT NULL);

  PROCEDURE P_GET_VERSION_HISTORY(io_cursor OUT SYS_REFCURSOR);
  PROCEDURE P_ADD_VERSION_HISTORY(i_version_no   IN VARCHAR2,
                                  i_release_date IN DATE,
                                  i_description  IN VARCHAR2,
                                  i_released_by  IN VARCHAR2,
                                  o_result       OUT VARCHAR2);
  PROCEDURE P_UPDATE_VERSION_HISTORY(i_version_id   IN NUMBER,
                                     i_version_no   IN VARCHAR2,
                                     i_release_date IN DATE,
                                     i_description  IN VARCHAR2,
                                     i_released_by  IN VARCHAR2,
                                     i_is_active    IN CHAR,
                                     o_result       OUT VARCHAR2);

  PROCEDURE P_GET_ROLE_DASHBOARD_PAGES(p_role_id IN NUMBER,
                                       O_CURSOR  OUT SYS_REFCURSOR);

  PROCEDURE P_MAINT_ROLE_DASHBOARD_PAGE(P_ROLE_ID         IN NUMBER,
                                        P_PAGE_ID         IN NUMBER,
                                        P_DASHBOARD_ORDER IN NUMBER,
                                        P_IS_ACTIVE       IN VARCHAR2,
                                        P_ACTION_IND      IN VARCHAR2,
                                        O_MESSAGE         OUT VARCHAR2);

  PROCEDURE P_GET_ROLE_DASHBOARD_CONFIG(p_role_id IN NUMBER,
                                        O_CURSOR  OUT SYS_REFCURSOR);

  PROCEDURE P_GET_API_MASTER(O_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_MAINT_API_MASTER(P_API_ID          IN NUMBER,
                               P_API_NAME        IN VARCHAR2,
                               P_CONTROLLER_NAME IN VARCHAR2,
                               P_API_ROUTE       IN VARCHAR2,
                               P_HTTP_METHOD     IN VARCHAR2,
                               P_PAGE_ID         in NUMBER,
                               P_STATUS          IN VARCHAR2,
                               P_ACTION_IND      IN VARCHAR2,
                               O_MESSAGE         OUT VARCHAR2);

  PROCEDURE P_CHECK_API_UNIQUE(P_PAGE_ID     IN NUMBER,
                               P_API_PATH    IN VARCHAR2,
                               P_HTTP_METHOD IN VARCHAR2,
                               O_EXISTS      OUT NUMBER);

  PROCEDURE P_INSERT_API_MASTER(P_API_NAME    IN VARCHAR2,
                                P_API_PATH    IN VARCHAR2,
                                P_HTTP_METHOD IN VARCHAR2,
                                P_IS_ACTIVE   IN VARCHAR2,
                                O_MESSAGE     OUT VARCHAR2);

  PROCEDURE P_UPDATE_API_MASTER(P_API_ID      IN NUMBER,
                                P_API_NAME    IN VARCHAR2,
                                P_API_PATH    IN VARCHAR2,
                                P_HTTP_METHOD IN VARCHAR2,
                                P_IS_ACTIVE   IN VARCHAR2,
                                O_MESSAGE     OUT VARCHAR2);

  PROCEDURE P_GET_DASHBOARD_QUICK_LINKS(P_ROLE_ID IN NUMBER,
                                        O_CURSOR  OUT SYS_REFCURSOR);

  PROCEDURE P_ADD_USER_ENTITY(p_user_id    IN NUMBER,
                              p_entity_id  IN NUMBER,
                              p_role_id    IN NUMBER,
                              p_is_primary IN CHAR DEFAULT 'N',
                              p_created_by IN NUMBER,
                              o_status     OUT NUMBER,
                              o_message    OUT VARCHAR2);

  PROCEDURE P_UPDATE_USER_ENTITY(p_id         IN NUMBER,
                                 p_entity_id  IN NUMBER,
                                 p_role_id    IN NUMBER,
                                 p_is_primary IN CHAR,
                                 p_status     IN CHAR,
                                 p_updated_by IN NUMBER,
                                 o_status     OUT NUMBER,
                                 o_message    OUT VARCHAR2);

  PROCEDURE P_DELETE_USER_ENTITY(p_id         IN NUMBER,
                                 p_deleted_by IN NUMBER,
                                 o_status     OUT NUMBER,
                                 o_message    OUT VARCHAR2);

  PROCEDURE P_GET_USER_ENTITIES(p_user_id IN NUMBER,
                                io_cursor OUT SYS_REFCURSOR);

  Procedure P_GET_ALL_CONTROLLER(O_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_HEAD_OBS_RISK_SUMMARY(P_ROLE_ID      IN NUMBER,
                                        P_ENT_ID       IN NUMBER,
                                        P_CYCLE_BUCKET IN VARCHAR2,
                                        IO_CURSOR      OUT SYS_REFCURSOR);

  Procedure P_Add_Department_Entity_Shifting(Old_Ent_id in number,
                                             new_ent_id in number,
                                             P_NO       in number,
                                             ENT_ID     in number,
                                             R_ID       in number,
                                             cir_no     in varchar2,
                                             cir_attach in clob,
                                             cir_date   in date,
                                             io_cursor  OUT t_cursor);

  Procedure P_GET_HEAD_OBS_RISK_DETAILS(P_ROLE_ID       IN NUMBER,
                                        P_ENT_ID        IN NUMBER,
                                        P_DEPARTMENT_ID IN NUMBER,
                                        P_CYCLE_BUCKET  IN VARCHAR2,
                                        IO_CURSOR       OUT SYS_REFCURSOR);

  PROCEDURE P_Get_Entity_Shifting_List(Io_Cursor OUT T_Cursor);

  PROCEDURE P_Get_Entity_Shifting_Paras(P_Ref_Id  IN NUMBER,
                                        Io_Cursor OUT T_Cursor);

end PKG_AD;
/
create or replace package body PKG_AD is

  procedure UPDATE_USERS(PPNUMBER      IN T_USER.PPNO%TYPE,
                         PASS          IN T_USER.PASSWORD%TYPE,
                         IS_ACTIVE     IN T_USER.ISACTIVE%TYPE,
                         ROLEID        IN T_USER_MAPING.ROLE_ID%TYPE,
                         entityid      in t_user.entity_id%type,
                         EMAIL_ADDRESS in varchar2,
                         io_cursor     OUT t_cursor) as
  
    v_count number := 0;
  begin
    IF (PASS IS NOT NULL) THEN
      UPDATE t_user T
         SET T.PASSWORD  = PASS,
             T.ISACTIVE  = IS_ACTIVE,
             t.entity_id = entityid
       where t.ppno = PPNUMBER;
      COMMIT;
    ELSE
      if (IS_ACTIVE is not null) then
        UPDATE t_user T
           SET T.ISACTIVE = IS_ACTIVE, t.entity_id = entityid
         where t.ppno = PPNUMBER;
        COMMIT;
      end if;
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
  
    IF (EMAIL_ADDRESS is not null or EMAIL_ADDRESS != '') THEN
    
      SELECT COUNT(*)
        INTO v_count
        FROM t_email_address
       WHERE ppno = PPNUMBER;
    
      IF v_count > 0 THEN
        UPDATE t_email_address
           SET email     = EMAIL_ADDRESS,
               entity_id = ENTITYID,
               roleid    = ROLEID,
               status    = IS_ACTIVE
         WHERE ppno = PPNUMBER;
        COMMIT;
      ELSE
        INSERT INTO t_email_address
          (ppno, email, entity_id, roleid, status)
        VALUES
          (PPNUMBER, EMAIL_ADDRESS, ENTITYID, ROLEID, IS_ACTIVE);
        COMMIT;
      END IF;
    
      SELECT COUNT(*) INTO v_count FROM t_emp_emails WHERE ppno = PPNUMBER;
    
      IF v_count > 0 THEN
        UPDATE t_emp_emails
           SET emailid = EMAIL_ADDRESS
         WHERE ppno = PPNUMBER;
        COMMIT;
      ELSE
        INSERT INTO t_emp_emails tt
          (ppno, EMAILID)
        VALUES
          (PPNUMBER, EMAIL_ADDRESS);
        COMMIT;
      END IF;
    
    END IF;
  
    OPEN io_cursor FOR
      SELECT r.id, r.remarks FROM T_AU_REMARKS R WHERE r.id = 10;
  
  end UPDATE_USERS;

  PROCEDURE RESET_USER_PASSWORD(PPNUMBER  IN T_USER.PPNO%TYPE,
                                CNIC      IN VARCHAR2,
                                PASS      IN T_USER.PASSWORD%TYPE,
                                io_cursor OUT t_cursor) AS
    v_ent_type VARCHAR2(2) := '';
    v_ent_id   NUMBER := 0;
  
    v_emp_cnic_raw  VARCHAR2(100);
    v_emp_cnic_norm VARCHAR2(100);
    v_cnic_norm     VARCHAR2(100);
  
    v_email_primary VARCHAR2(4000) := '0'; -- personal or entity email, depending on ENT_TYPE
    v_email_branch  VARCHAR2(4000) := '';
    v_emp_name      VARCHAR2(400) := 'Unknown';
  BEGIN
    -- Find user's entity type and entity id
    BEGIN
      SELECT NVL(t.audit_type, ''), e.entity_id
        INTO v_ent_type, v_ent_id
        FROM t_user u
        JOIN t_auditee_entities e
          ON e.entity_id = u.entity_id
        JOIN t_auditee_ent_types t
          ON t.autid = e.type_id
       WHERE u.ppno = PPNUMBER;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        OPEN io_cursor FOR
          SELECT 'No user or entity mapping found for the given PPNUMBER.' AS remarks,
                 '' AS emailAddress,
                 '' AS emailAddress2,
                 'N' AS IND,
                 '' AS empFullName
            FROM dual;
        RETURN;
    END;
  
    -- Get employee CNIC
    BEGIN
      SELECT em.nicnonew
        INTO v_emp_cnic_raw
        FROM v_service_employeeinfo em
       WHERE em.ppno = PPNUMBER;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        OPEN io_cursor FOR
          SELECT 'No Employee found with the given PPNUMBER.' AS remarks,
                 '' AS emailAddress,
                 '' AS emailAddress2,
                 'N' AS IND,
                 '' AS empFullName
            FROM dual;
        RETURN;
    END;
  
    -- Normalize CNICs for comparison
    v_cnic_norm     := REPLACE(REPLACE(UPPER(TRIM(CNIC)), '-', ''), ' ', '');
    v_emp_cnic_norm := REPLACE(REPLACE(UPPER(TRIM(v_emp_cnic_raw)), '-', ''),
                               ' ',
                               '');
  
    IF v_cnic_norm <> v_emp_cnic_norm THEN
      OPEN io_cursor FOR
        SELECT 'Your CNIC is incorrect, Please provide correct CNIC' AS remarks,
               '' AS emailAddress,
               '' AS emailAddress2,
               'N' AS IND,
               '' AS empFullName
          FROM dual;
      RETURN;
    END IF;
  
    -- Determine primary email based on entity type
    IF v_ent_type = 'D' THEN
      -- Personal email from v_emails
      BEGIN
        SELECT COALESCE(e.emailid, '0')
          INTO v_email_primary
          FROM v_emails e
         WHERE e.ppno = PPNUMBER;
      EXCEPTION
        WHEN NO_DATA_FOUND THEN
          v_email_primary := '0';
      END;
    ELSIF v_ent_type = 'B' THEN
      -- Entity email from t_auditee_entities
      BEGIN
        SELECT COALESCE(e.email_address, '0')
          INTO v_email_primary
          FROM t_auditee_entities e
         WHERE e.entity_id = v_ent_id;
      EXCEPTION
        WHEN NO_DATA_FOUND THEN
          v_email_primary := '0';
      END;
    ELSE
      OPEN IO_CURSOR FOR
        SELECT 'Your entity type is not supported for password reset. Please contact System Administrator on 051-2002110' AS remarks,
               '' AS emailAddress,
               '' AS emailAddress2,
               'N' AS IND,
               '' AS empFullName
          FROM dual;
      RETURN;
    END IF;
  
    -- Secondary email (branch list) and employee name
    BEGIN
      SELECT RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(e.email_address, ' ', '{}'),
                                         '}{',
                                         ''),
                                 '{}',
                                 '')))
        INTO v_email_branch
        FROM t_auditee_entities e
       WHERE e.entity_id = v_ent_id;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        v_email_branch := '';
    END;
  
    BEGIN
      SELECT em.Employeefirstname || ' ' || em.employeelastname
        INTO v_emp_name
        FROM v_service_employeeinfo em
       WHERE em.ppno = PPNUMBER;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        v_emp_name := 'Unknown';
    END;
  
    -- Require a usable primary email
    IF v_email_primary = '0' OR NVL(TRIM(v_email_primary), '') IS NULL THEN
      OPEN io_cursor FOR
        SELECT 'Email Address not configured in IAS, please contact System Administrator on 051-2002110' AS remarks,
               '' AS emailAddress,
               '' AS emailAddress2,
               'N' AS IND,
               '' AS empFullName
          FROM dual;
      RETURN;
    END IF;
  
    -- Update password (hash recommended)
    UPDATE t_user t
       SET t.password = pass, t.password_change_req = 'Y'
     WHERE t.ppno = PPNUMBER;
  
    -- No COMMIT here; let the caller decide
    -- COMMIT;
  
    OPEN io_cursor FOR
      SELECT 'Password has been reset and email forwarded on ' ||
             v_email_primary AS remarks,
             v_email_primary AS emailAddress,
             v_email_branch AS emailAddress2,
             'Y' AS IND,
             v_emp_name AS empFullName
        FROM dual;
  
  EXCEPTION
    WHEN OTHERS THEN
      OPEN io_cursor FOR
        SELECT 'An unexpected error occurred while resetting the password.' AS remarks,
               '' AS emailAddress,
               '' AS emailAddress2,
               'N' AS IND,
               '' AS empFullName
          FROM dual;
  END RESET_USER_PASSWORD;

  procedure user_maping(USER_ID in t_user_maping.userid%type,
                        ROLE_ID in t_user_maping.role_id%type,
                        PPNO    in t_user_maping.ppno%type) is
    io number := 0;
  begin
    select m.ppno into io from t_user_maping m where m.ppno = PPNO;
    if (ppno = io) then
      update t_user_maping mm
         set mm.GROUP_ID = ROLE_ID, mm.role_id = ROLE_ID
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

  -- duplicate
  procedure P_add_new_user(enc_pass  in varchar2,
                           role_id   in number,
                           P_NO      in number,
                           ent_id    in number,
                           io_cursor OUT t_cursor) as
    N_F number := 0;
  begin
  
    select nvl(max(mp.userid), 0)
      into N_F
      from t_user_maping mp
     where mp.ppno = P_NO;
    if (N_F = 0) then
      INSERT INTO t_user_maping
        (USERID, PPNO, GROUP_ID, ROLE_ID)
      VALUES
        ((select COALESCE(max(pp.userid) + 1, 1) from T_USER pp),
         P_NO,
         role_id,
         role_id);
      commit;
    end if;
    insert into t_user
      (userid, password, ppno, isactive, entity_id, login_name)
    values
      ((select COALESCE(max(pp.userid) + 1, 1) from T_USER pp),
       enc_pass,
       P_NO,
       'Y',
       ent_id,
       P_NO);
    commit;
    update t_user u
       set u.user_location_type =
           (select e.EMPLOYEETYPE
              from v_service_employeeinfo e
             where P_NO = e.PPNO);
    commit;
    open io_cursor for
      select 'User has been created' as remarks from dual;
  
  end P_add_new_user;

  procedure P_UpdateUser(USER_ID  in t_user.userid%type,
                         enc_pass in t_user.password%type,
                         role_id  in t_user_maping.role_id%type,
                         PPNO     in t_user.ppno%type,
                         ISACTIVE in t_user.isactive%type) as
    u_location varchar2(10) := 0;
    A_C        varchar2(10) := 0;
  begin
    select ISACTIVE into A_C from dual;
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
        UPDATE t_user SET ISACTIVE = A_C WHERE PPNO = PPNO;
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
           A_C);
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

  Procedure P_Get_observvation_no(obs_id in number, io_cursor OUT t_cursor) as
  begin
    OPEN IO_CURSOR FOR
      Select o.memo_number, o.draft_para_no, o.final_para_no
        from t_au_observation o
       where o.id = obs_id;
  
  end P_Get_observvation_no;

  Procedure P_Update_observation_no(m_no      in number,
                                    D_no      in number,
                                    P_no      in number,
                                    obs_id    in number,
                                    io_cursor OUT t_cursor) as
  begin
    update t_au_observation o
       set o.memo_number   = m_no,
           o.draft_para_no = D_no,
           o.final_para_no = P_NO
     where o.id = obs_id;
    commit;
    OPEN io_cursor FOR
      SELECT obs_id || 'Observation Number has been updated' AS REMARKS
        FROM DUAL;
  end P_Update_observation_no;

  procedure P_UPDATE_ENG_DATE(ENGID     IN NUMBER,
                              ST_DATE   IN DATE,
                              ED_DATE   IN DATE,
                              io_cursor OUT t_cursor) as
  begin
    UPDATE T_AU_PLAN_ENG E
       SET E.AUDIT_STARTDATE = ST_DATE, E.AUDIT_ENDDATE = ED_DATE
     WHERE E.ENG_ID = ENGID;
  
    COMMIT;
    OPEN io_cursor FOR
      SELECT 'DATES UPDATED' AS REMARKS FROM DUAL;
  END P_UPDATE_ENG_DATE;

  procedure p_get_allusers(ENTITYID  in t_user.ppno%type,
                           EMAIL     in v_service_employeeinfo.EMAIL%type,
                           GROUPID   in t_groups.group_id%type,
                           PPNUMBER  in t_user.ppno%type,
                           LOGINNAME in t_user.login_name%type,
                           ENT_ID    in number,
                           P_NO      in number,
                           R_ID      in number,
                           io_cursor OUT t_cursor) as
    E_F     number := 0;
    V_EMAIL varchar2(100) := EMAIL;
  begin
  
    if (PPNUMBER != 0) then
      OPEN io_cursor FOR
        select nvl(u.userid, 1) as userid,
               mp.parent_id,
               mp.parent_code,
               mp.child_code,
               mp.p_type_id,
               mp.c_type_id,
               emp.ppno ppno,
               nvl(u.entity_id, 1) as entity_id,
               nvl(e.code, 1) as code,
               nvl(e.type_id, 1) as type_id,
               emp.EMPLOYEEFIRSTNAME,
               emp.EMPLOYEELASTNAME,
               case
                 when t.audit_type = 'B' then
                  e.email_address
                 else
                  nvl(emp.email, ema.email)
               end as email,
               nvl(u.ISACTIVE, 'Y') as isactive,
               r.group_name,
               rm.group_id,
               mp.relation_type_id,
               mp.p_name,
               mp.c_name
          from v_service_employeeinfo emp
        
          left join t_user u
            on emp.PPNO = u.ppno
          left join t_email_address ema
            on ema.ppno = u.ppno
          left join t_auditee_entities e
            on u.entity_id = e.entity_id
          left join v_get_parent_office mp
            on e.entity_id = mp.entity_id
          left join t_auditee_ent_types t
            on t.autid = e.type_id
          left join t_user_maping rm
            on u.PPNO = rm.ppno
          left join t_groups r
            on r.role_id = rm.role_id
         WHERE emp.ppno = PPNUMBER
        
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
                 case
                   when t.audit_type = 'B' then
                    e.email_address
                   else
                    nvl(emp.email, ema.email)
                 end as email,
                 u.ISACTIVE,
                 r.group_name,
                 rm.group_id,
                 mp.relation_type_id,
                 mp.p_name,
                 mp.c_name
            from t_auditee_entities e
           inner join v_get_parent_office mp
              on e.entity_id = mp.entity_id
           inner join t_auditee_ent_types t
              on t.autid = e.type_id
           inner join t_user u
              on u.entity_id = e.entity_id
           inner join v_service_employeeinfo emp
              on emp.PPNO = u.ppno
            left join t_email_address ema
              on ema.ppno = u.ppno
            left join t_user_maping rm
              on u.PPNO = rm.ppno
            left join t_groups r
              on r.role_id = rm.role_id
           WHERE u.ENTITY_ID = entityid
          --and e.type_id = mp.c_type_id
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
                   case
                     when t.audit_type = 'B' then
                      e.email_address
                     else
                      nvl(emp.email, ema.email)
                   end as email,
                   u.ISACTIVE,
                   r.group_name,
                   rm.group_id,
                   mp.relation_type_id,
                   mp.p_name,
                   mp.c_name
              from t_auditee_entities e
             inner join v_get_parent_office mp
                on e.entity_id = mp.entity_id
             inner join t_auditee_ent_types t
                on t.autid = e.type_id
             inner join t_user u
                on u.entity_id = e.entity_id
             inner join v_service_employeeinfo emp
                on emp.PPNO = u.ppno
              left join t_email_address ema
                on ema.ppno = u.ppno
              left join t_user_maping rm
                on u.PPNO = rm.ppno
              left join t_groups r
                on r.role_id = rm.role_id
             WHERE r.group_id = GROUPID
               and e.type_id = mp.c_type_id
             ORDER BY emp.CURRENTRANKCODE;
        else
          if (V_EMAIL is not null) then
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
                     case
                       when t.audit_type = 'B' then
                        e.email_address
                       else
                        nvl(emp.email, ema.email)
                     end as email,
                     u.ISACTIVE,
                     r.group_name,
                     rm.group_id,
                     mp.relation_type_id,
                     mp.p_name,
                     mp.c_name
                from t_auditee_entities e
               inner join v_get_parent_office mp
                  on e.entity_id = mp.entity_id
               inner join t_auditee_ent_types t
                  on t.autid = e.type_id
               inner join t_user u
                  on u.entity_id = e.entity_id
               inner join v_service_employeeinfo emp
                  on emp.PPNO = u.ppno
                left join t_email_address ema
                  on ema.ppno = u.ppno
                left join t_user_maping rm
                  on u.PPNO = rm.ppno
                left join t_groups r
                  on r.role_id = rm.role_id
               WHERE emp.EMAIL = V_EMAIL
                 and e.type_id = mp.c_type_id
               ORDER BY emp.CURRENTRANKCODE;
          end if;
        end if;
      end if;
    end if;
  
  end p_get_allusers;

  procedure P_GetAllTopMenus(ENT_ID    in number,
                             P_NO      in number,
                             R_ID      in number,
                             io_cursor OUT t_cursor) as
  begin
    OPEN IO_CURSOR FOR
    
      select m.* from t_menu m ORDER BY M.MENU_ORDER ASC;
  
  end P_GetAllTopMenus;

  procedure P_GetAssignedMenuPages(groupId   in t_menu_pages_groupmap.group_id%type,
                                   menuId    in T_MENU_PAGES.MENU_ID%type,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) as
  begin
    OPEN IO_CURSOR FOR
    
      Select *
        FROM T_MENU_PAGES mp
       inner join t_menu_pages_groupmap mpg
          on mp.Id = mpg.page_id
       WHERE mp.Status = 'A'
         and mpg.GROUP_ID = groupId
         and mp.MENU_ID = menuId
       order by mp.page_name asc;
  
  end P_GetAssignedMenuPages;

  PROCEDURE P_AddGroupMenuItemsAssignment(groupid IN T_MENU_PAGES_GROUPMAP.GROUP_ID%TYPE,
                                          PAGEID  IN T_MENU_PAGES_GROUPMAP.PAGE_ID%TYPE) IS
    v_api_count NUMBER := 0;
  BEGIN
    -- 1. Ensure menu mapping is clean
    DELETE FROM T_MENU_PAGES_GROUPMAP mp
     WHERE mp.group_id = groupid
       AND mp.page_id = PAGEID;
  
    INSERT INTO T_MENU_PAGES_GROUPMAP
      (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    VALUES
      ((SELECT COALESCE(MAX(p.GROUPMAP_ID) + 1, 1)
         FROM T_MENU_PAGES_GROUPMAP p),
       groupid,
       PAGEID);
  
    -- 2. Check whether this page has any active API calls
    SELECT COUNT(1)
      INTO v_api_count
      FROM T_AU_API_MASTER m
     WHERE m.PAGE_ID = PAGEID
       AND m.IS_ACTIVE = 'Y';
  
    -- 3. Grant APIs only if:
    --    a) page has active APIs
    --    b) group is not group 1
    IF v_api_count > 0 or NVL(groupid, 0) <> 1 THEN
    
      INSERT INTO T_AU_ROLE_API_PERMISSION
        (ROLE_ID, API_ID, IS_ACTIVE, CREATED_BY)
        SELECT DISTINCT groupid, m.API_ID, 'Y', 113092
          FROM T_AU_API_MASTER m
         WHERE m.PAGE_ID = PAGEID
           AND m.IS_ACTIVE = 'Y'
           AND NOT EXISTS (SELECT 1
                  FROM T_AU_ROLE_API_PERMISSION p
                 WHERE p.ROLE_ID = groupid
                   AND p.API_ID = m.API_ID);
    
    END IF;
  
    COMMIT;
  END P_AddGroupMenuItemsAssignment;
  procedure P_RemoveGroupMenuItemsAssignment(groupid in T_MENU_PAGES_GROUPMAP.GROUP_ID%type,
                                             PAGEID  in T_MENU_PAGES_GROUPMAP.PAGE_ID%type) is
  begin
    -- 1. Remove menu visibility
    delete from T_MENU_PAGES_GROUPMAP mp
     where mp.group_id = groupid
       and mp.page_id = PAGEID;
  
    -- 2. Revoke all APIs of this page
    delete from T_AU_ROLE_API_PERMISSION p
     where p.ROLE_ID = groupid
       and p.API_ID in
           (select m.API_ID from T_AU_API_MASTER m where m.PAGE_ID = PAGEID);
  
    commit;
  end P_RemoveGroupMenuItemsAssignment;

  procedure P_GetAllMenuPages(menuId    in T_MENU_PAGES.MENU_ID%type,
                              ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor) as
  begin
    if (menuId != 0) then
      OPEN io_cursor FOR
      
        Select mp.id,
               mp.menu_id,
               mp.page_name,
               mp.page_path,
               mp.page_order,
               mp.status,
               mp.hide_menu,
               mp.sub_menu,
               mp.page_key
          FROM T_MENU_PAGES mp
         WHERE mp.MENU_ID = menuId
           and mp.id is not null
           and mp.page_path is not null
         order by mp.page_name asc;
    else
      OPEN io_cursor FOR
        Select mp.id,
               mp.id         as Page_id,
               mp.menu_id,
               mp.page_name,
               mp.page_path,
               mp.page_path  as PAGE_URL,
               mp.page_order,
               mp.status,
               mp.hide_menu,
               mp.sub_menu,
               mp.page_key
          FROM T_MENU_PAGES mp
         where mp.status = 'A'
           and mp.id is not null
           and mp.page_path is not null
         order by mp.PAGE_ORDER;
    end if;
  end P_GetAllMenuPages;

  procedure P_updateAllMenuPages(menuId in T_MENU_PAGES.MENU_ID%type,
                                 p_id   in T_MENU_PAGES.MENU_ID%type,
                                 ENT_ID in number,
                                 P_NO   in number,
                                 R_ID   in number) as
  begin
  
    update T_MENU_PAGES mp
       set mp.status = 'A', mp.menu_id = menuid
     WHERE mp.id = p_id;
    commit;
  
  end P_updateAllMenuPages;

  procedure P_GetGroups(ENT_ID    in number,
                        P_NO      in number,
                        R_ID      in number,
                        io_cursor OUT t_cursor) as
  begin
    if (R_ID = 2) then
      OPEN io_cursor FOR
        select g.role_id, g.group_id, g.description, g.group_name, g.status
          from t_groups g
         WHERE g.STATUS = 'Y'
           and g.role_id not in (1, 2, 3, 5, 6, 7, 37)
         ORDER BY g.GROUP_ID;
    
    else
      OPEN io_cursor FOR
        select g.role_id, g.group_id, g.description, g.group_name, g.status
          from t_groups g
         WHERE g.STATUS = 'Y'
         ORDER BY g.GROUP_ID;
    
    end if;
  
  end P_GetGroups;

  procedure P_GetRoleResponsibilities(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      select * from t_hr_designations s WHERE s.STATUSTYPE = 'A';
  
  end P_GetRoleResponsibilities;

  procedure P_Group_Update(P_GROUPID           in t_groups.group_id%type,
                           P_GROUP_DESCRIPTION in t_groups.description%type,
                           P_GROUP_NAME        in t_groups.group_name%type,
                           P_ISACTIVE          in t_groups.status%type,
                           ENT_ID              in number,
                           P_NO                in number,
                           R_ID                in number) as
  begin
  
    UPDATE T_GROUPS g
       SET g.GROUP_NAME  = P_GROUP_NAME,
           g.DESCRIPTION = P_GROUP_DESCRIPTION,
           g.STATUS      = P_ISACTIVE
     WHERE g.GROUP_ID = P_GROUPID;
    commit;
  
  end P_Group_Update;

  procedure p_AddGroup(GROUP_DESCRIPTION in t_groups.description%type,
                       GROUP_NAME        in t_groups.group_name%type,
                       ISACTIVE          in t_groups.status%type,
                       ENT_ID            in number,
                       P_NO              in number,
                       R_ID              in number) is
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

  --not used

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

  procedure P_UpdateENTITIEES(P_NO          in number,
                              ENT_ID        IN NUMBER,
                              R_ID          IN NUMBER,
                              E_CODE        IN NUMBER,
                              E_NAME        IN VARCHAR2,
                              E_DISCRIPTION IN VARCHAR2,
                              E_AUDITEDBY   IN NUMBER,
                              E_TYPEID      IN NUMBER,
                              E_STATUS      IN CHAR,
                              E_AUDITABLE   IN VARCHAR2,
                              ENTITYID      IN NUMBER,
                              io_cursor     OUT t_cursor) as
  
  begin
  
    IF (R_ID in (1, 2, 7)) THEN
      update T_AUDITEE_ENTITIES C
         SET C.CODE        = E_CODE,
             C.DESCRIPTION = E_name,
             C.NAME        = E_NAME,
             C.ACTIVE      = E_STATUS,
             C.TYPE_ID     = E_TYPEID,
             C.AUDITBY_ID  = E_AUDITEDBY,
             C.AUDITABLE   = E_AUDITABLE
       WHERE C.ENTITY_ID = ENTITYID;
      COMMIT;
      if (E_STATUS = 'Y') then
        update t_auditee_entities_maping m
           set m.auditedby  = E_AUDITEDBY,
               m.c_name     = E_name,
               m.c_type_id  = E_TYPEID,
               m.status     = E_STATUS,
               m.child_code = E_CODE
         where m.entity_id = ENTITYID;
        commit;
      else
      
        delete from t_auditee_entities_maping em
         where em.entity_id = ENTITYID;
        commit;
      end if;
    
      update ais_t_au_post_compliance c
         set c.entity_type_id = E_TYPEID, c.entity_code = E_CODE
       where c.entity_id = ENTITYID;
      commit;
    
      update t_au_plan_eng e
         set e.entity_type = E_TYPEID, e.entity_code = e_code
       where e.entity_id = ENTITYID;
      commit;
    
      insert into T_AUDITEE_ENTITIES_UPDATE_LOG
        (ID,
         USER_ENTITY_ID,
         PPNUM,
         ACTION,
         RECORD_TIME,
         ROLE_ID,
         ENTITY_ID)
      Values
        ((select COALESCE(max(acc.ENTITY_ID) + 1, 1)
           from T_AUDITEE_ENTITIES_UPDATE_LOG acc),
         ENT_ID,
         p_no,
         E_Name || '  has been Entity Updated',
         sysdate,
         R_ID,
         ENTITYID);
      commit;
    
      open io_cursor for
        Select E_Name || ' has been Updated' as remarks from dual;
    else
      open io_cursor for
        Select 'You have no Rights to update, please contact system Administrator' as remarks
          from dual;
    end if;
  
  end P_UpdateENTITIEES;

  procedure P_InsertENTITIEES(P_NO          in number,
                              ENT_ID        in number,
                              R_ID          in number,
                              E_CODE        IN NUMBER,
                              E_NAME        IN VARCHAR2,
                              E_DISCRIPTION IN VARCHAR2,
                              E_AUDITEDBY   IN NUMBER,
                              E_TYPEID      IN NUMBER,
                              E_STATUS      IN CHAR,
                              E_AUDITABLE   IN VARCHAR2,
                              io_cursor     OUT t_cursor) is
  begin
    insert into T_AUDITEE_ENTITIES
      (ENTITY_ID,
       CODE,
       DESCRIPTION,
       NAME,
       ACTIVE,
       TYPE_ID,
       AUDITBY_ID,
       AUDITABLE,
       AUDITOR,
       IAD)
    values
      ((select COALESCE(max(acc.ENTITY_ID) + 1, 1)
         from T_AUDITEE_ENTITIES acc),
       E_CODE,
       E_DISCRIPTION,
       E_name,
       E_STATUS,
       E_TYPEID,
       E_AUDITEDBY,
       E_AUDITABLE,
       'N',
       'N');
    COMMIT;
  
    Open io_cursor for
      Select E_Name || ' added in system' as remarks from dual;
  
  end P_InsertENTITIEES;

  procedure P_Getrealtionshiptype(R_ID      in number,
                                  ENT_ID    in number,
                                  P_NO      in number,
                                  PAGE_ID   in number,
                                  io_cursor OUT t_cursor) is
  
  begin
  
    OPEN IO_CURSOR FOR
      select distinct F.ID, f.entity_realtion_id, F.field_name
        from v_Get_realtionshiptype f
      
       where F.AUDITBY_ID = case
               when R_ID in (16, 17) then
                ENT_ID
               else
                F.auditby_id
             end
      
       order by F.ID;
  end P_Getrealtionshiptype;

  procedure P_Get_Entity_type(ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              PG_ID     in number,
                              io_cursor OUT t_cursor) is
  
  begin
  
    open io_cursor for
      SELECT NVL(F.AUTID, 0) AS AUTID,
             NVL(F.ENTITYCODE, 0) AS ENTITYCODE,
             NVL(F.ENTITYTYPEDESC, 'Not Available') AS ENTITYTYPEDESC,
             NVL(F.AUDITABLE, '-') AS AUDITABLE,
             NVL(F.AUDITEDBY, 0) AS AUDITEDBY,
             NVL(F.AUDITED_BY_ENITITY, 0) AS AUDITED_BY_ENITITY
        from t_auditee_ent_types f
      
       WHERE (R_ID IN (5, 15, 16) AND F.AUDIT_TYPE = 'B')
          OR (R_ID in (6, 7, 11) AND f.audited_by_enitity = ENT_ID)
          OR (R_ID in (1,2) and f.autid is not null)
          OR ((R_ID in  (11) and ENT_ID = 112243) AND F.AUDIT_TYPE = 'B')
       order by f.autid;
  end P_Get_Entity_type;

  procedure p_get_audited_by(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select f.deptname, f.status, f.entity_id, f.auditor
        from t_audit_departments f
      --where f.auditor = 'Y'
       order by f.entity_id;
  
  end p_get_audited_by;

  procedure P_Getparentrepoffice(rid       in number,
                                 ENT_ID    in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) is
  
  begin
  
    open io_cursor for
      select Distinct (r.p_name) as DESCRIPTION,
                      r.parent_id as ENTITY_ID,
                      r.relation_type_id as ENTITY_REALTION_ID,
                      t.entitytypedesc as ENTITYTYPEDESC,
                      r.status as ACTIVE,
                      r.p_type_id as typeid
        from t_auditee_ent_relation e,
             t_auditee_ent_types    t,
             v_get_parent_office    r
       where t.autid = r.relation_type_id
         --and r.p_type_id = e.parent_entity_typeid
         and r.c_type_id = e.child_entity_typeid
         and r.relation_type_id = rid
         and r.parent_id is not null
         and r.auditedby = case
               when R_ID in (1, 2, 41) then
                r.auditedby
               else
                ENT_ID
             end
       order by r.p_name;
  
  end P_Getparentrepoffice;

  procedure P_Getchildposting(erid in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      SELECT DISTINCT r.c_name,
                      r.entity_id,
                      r.status,
                      r.c_type_id AS typeid,
                      r.COMPLICE_BY,
                      r.AUDIT_BY,
                      (CASE
                        WHEN m.Gm_Office IS NOT NULL THEN
                         (SELECT r.name
                            FROM t_auditee_entities r
                           WHERE r.entity_id = m.gm_office)
                        ELSE
                         '-'
                      END) AS GM_OFFICE,
                      (CASE
                        WHEN m.reporting IS NOT NULL THEN
                         (SELECT r.name
                            FROM t_auditee_entities r
                           WHERE r.entity_id = m.reporting)
                        ELSE
                         '-'
                      END) AS reporting
        FROM v_get_parent_office r
       INNER JOIN t_auditee_ent_types t
          ON t.autid = r.relation_type_id
       INNER JOIN t_auditee_entities_maping m
          ON m.entity_id = r.entity_id
       WHERE r.parent_id = erid
       ORDER BY r.c_name;
  end P_Getchildposting;

  procedure P_GetAuditZones(ENTITYID  in t_auditee_entities.entity_id%type,
                            io_cursor OUT t_cursor) as
  
  begin
    if (ENTITYID != 0) then
    
      OPEN io_cursor FOR
        Select z.entity_id,
               z.code,
               z.description,
               z.name,
               z.type_id,
               z.auditby_id,
               z.inspectedby_id,
               z.cost_center,
               z.active,
               z.auditable
          FROM t_auditee_entities z
         WHERE z.entity_id = ENTITYID
         order by z.name asc;
    else
      OPEN io_cursor FOR
        Select z.entity_id,
               z.code,
               z.description,
               z.name,
               z.type_id,
               z.auditby_id,
               z.inspectedby_id,
               z.cost_center,
               z.active,
               z.auditable
          FROM t_auditee_entities z
         WHERE z.type_id = '9'
         order by z.name asc;
    end if;
  
  end P_GetAuditZones;

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
      select t.code        as ZONEID,
             t.code        as zonecode,
             t.name,
             t.description,
             t.active      as ISACTIVE,
             t.entity_id
        from T_AUDITEE_ENTITIES t
       where t.type_id = 5
       order by t.name asc;
  
  end P_GetZones;

  procedure P_GetZonesForHoMointoring(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      io_cursor OUT t_cursor) as
  
  begin
    if (R_ID in (1, 3, 4, 5, 25)) then
      OPEN io_cursor FOR
        select distinct e.parent_id        as ZONEID,
                        t.entity_id        as zonecode,
                        t.name,
                        t.description,
                        t.active           as ISACTIVE,
                        t.entity_id,
                        '' AS relation_type_id
          from T_AUDITEE_ENTITIES t
         inner join v_get_parent_office e
            on t.entity_id = e.parent_id
         inner join t_auditee_ent_types et
            on e.c_type_id = et.autid
         where et.audit_type = CASE
                 WHEN r_id IN (1, 3) THEN
                  et.audit_type
                 ELSE
                  'B'
               END
           and t.code is not null
         order by e.parent_id asc;
    elsif (R_ID in (6, 7, 9, 11,35)) then
        OPEN io_cursor FOR
          select distinct m.parent_id   as entity_id,
                          m.parent_id   as ZONEID,
                          m.parent_code as zonecode,
                          m.p_name      as name,
                          m.p_name      as description,
                          M.STATUS      as ISACTIVE
          
            from T_AUDITEE_ENTITIES t
           inner join t_auditee_entities_maping m
              on m.entity_id = t.entity_id
           inner join t_auditee_ent_types ft
              on ft.autid = t.type_id
           where (R_ID IN (6,7, 9,40) AND t.auditby_id = ENT_ID)
              OR (R_ID IN ( 11,35) AND ft.audit_type = 'B')
           order by m.p_name asc;
      elsif (R_ID in (15, 16, 2)) then
          OPEN io_cursor FOR
            select distinct m.parent_id as ZONEID,
                            m.parent_id as zonecode,
                            m.p_name as name,
                            m.p_name as description,
                            '' as ISACTIVE,
                            m.parent_id as entity_id
              from T_AUDITEE_ENTITIES t
             inner join t_auditee_entities_maping m
                on m.entity_id = t.entity_id
             where m.auditedby = ENT_ID
             order by m.p_name asc;
        elsif (R_ID in (21)) then
            OPEN io_cursor FOR
              select distinct m.parent_id   as entity_id,
                              m.parent_id   as ZONEID,
                              m.parent_code as zonecode,
                              m.p_name      as name,
                              m.p_name      as description,
                              M.STATUS      as ISACTIVE
              
                from T_AUDITEE_ENTITIES t
               inner join t_auditee_entities_maping m
                  on m.entity_id = t.entity_id
               where M.PARENT_ID = ENT_ID
               order by m.p_name asc;   
       elsif (R_ID in (40)) then
            OPEN io_cursor FOR
              select distinct m.parent_id   as entity_id,
                              m.parent_id   as ZONEID,
                              m.parent_code as zonecode,
                              m.p_name      as name,
                              m.p_name      as description,
                              M.STATUS      as ISACTIVE
              
                from T_AUDITEE_ENTITIES t
               inner join t_auditee_entities_maping m
                  on m.entity_id = t.entity_id
                  inner join t_auditee_ent_types e
                  on e.autid = t.type_id
               where e.controlling = ENT_ID
               order by m.p_name asc;          
          else
            OPEN io_cursor FOR
              select t.code        as ZONEID,
                     t.code        as zonecode,
                     t.name,
                     t.description,
                     t.active      as ISACTIVE,
                     t.entity_id
                from T_AUDITEE_ENTITIES t
               where t.type_id = 5
                 and t.entity_id = ENT_ID
               order by t.name asc;
          end if;
   
  end P_GetZonesForHoMointoring;

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

  procedure P_GetEntitees_for_update(ENTITYID  NUMBER,
                                     P_NO      number,
                                     ENT_ID    number,
                                     TYPEID    NUMBER,
                                     Ro_ID     number,
                                     io_cursor OUT t_cursor) as
  
  begin
  
    open io_cursor for
      select e.entity_id,
             e.code,
             e.name,
             e.active,
             e.auditby_id,
             az.name AS auditby_name,
             e.auditable,
             NVL(e.address, '-') AS address,
             e.telephone,
             e.email_address,
             e.risk_id,
             r.description AS erisk,
             e.size_id,
             s.description AS esize
        FROM t_auditee_entities e
       inner join t_auditee_entities az
          on e.auditby_id = az.entity_id
       inner join t_risk r
          on r.r_id = e.risk_id
        join t_auditee_entities_size_disc s
          on s.entity_size = e.size_id
       WHERE e.type_id = TYPEID
         and e.auditby_id = case
               when Ro_ID in (1 , 5) then
                e.auditby_id
               when Ro_ID in (2, 5, 6, 7, 15, 16,11) then
                ENT_ID
             end
         and not exists (select 'z'
                from t_auditee_entities_update_Az az
               where az.entity_id = e.entity_id
                 and az.up_status = 'U');
  
  end P_GetEntitees_for_update;

  procedure P_GetEntitees_for_update_comp(E_ENTITY_ID number,
                                          P_NO        NUMBER,
                                          ENT_ID      number,
                                          R_ID        NUMBER,
                                          io_cursor   OUT t_cursor) AS
  
  BEGIN
  
    open io_cursor for
      select az.id,
             az.entity_id,
             az.code,
             az.name,
             az.active,
             az.auditby_id,
             az.auditable,
             az.address,
             az.telephone,
             az.email_address,
             az.risk_id,
             r.description as erisk,
             az.size_id,
             s.description as esize,
             az.up_status,
             az.updated_by,
             az.update_on,
             az.authorized_by,
             az.authorized_on,
             e.code as code_old,
             e.name as name_old,
             e.active as active_old,
             e.auditable as auditable_old,
             e.auditby_id as auditby_id_old,
             nvl(e.address, '-') as address_old,
             e.telephone as telephone_old,
             e.email_address as email_address_old,
             e.risk_id as risk_id_old,
             e.size_id as size_id_old,
             er.description as Erisk_old,
             es.description as Esize_old,
             '' as AUDITBY_NAME,
             old_e.name as AUDITBY_NAME_old
      
        FROM t_auditee_entities_update_Az az
        join t_auditee_entities old_e
          on az.auditby_id = old_e.entity_id
        join t_auditee_entities e
          on e.entity_id = az.entity_id
        join t_risk er
          on er.r_id = e.risk_id
        join t_auditee_entities_size_disc es
          on es.entity_size = e.size_id
        join t_risk r
          on r.r_id = az.risk_id
        join t_auditee_entities_size_disc s
          on s.entity_size = az.size_id
       where az.up_status = 'U'
         and az.entity_id = E_ENTITY_ID;
  
  end P_GetEntitees_for_update_comp;

  procedure P_GetEntitees_for_update_authorization(E_ENTITY_ID number,
                                                   E_up_status VARCHAR2,
                                                   P_NO        NUMBER,
                                                   R_ID        NUMBER,
                                                   IND         VARCHAR2,
                                                   io_cursor   OUT t_cursor) AS
  
  BEGIN
  
    open io_cursor for
      select az.id,
             az.entity_id,
             az.code,
             az.name,
             az.active,
             az.auditby_id,
             e.name           as auditby_name,
             az.auditable,
             az.address,
             az.telephone,
             az.email_address,
             az.risk_id,
             r.description    as erisk,
             az.size_id,
             s.description    as esize,
             az.up_status,
             az.updated_by,
             az.update_on,
             az.authorized_by,
             az.authorized_on
      
        FROM t_auditee_entities_update_Az az
       inner join t_auditee_entities e
          on az.auditby_id = e.entity_id
        join t_risk r
          on r.r_id = az.risk_id
        join t_auditee_entities_size_disc s
          on s.entity_size = az.size_id
       where az.up_status = 'U'
         and az.auditby_id = case
               when R_ID in (1, 5) then
                az.auditby_id
               else
                E_ENTITY_ID
             end;
  
  end P_GetEntitees_for_update_authorization;

  PROCEDURE P_UPDATE_ENTITIES(E_entity_id     NUMBER,
                              E_code          VARCHAR2,
                              E_name          VARCHAR2,
                              E_active        VARCHAR2,
                              E_auditable     VARCHAR2,
                              E_address       VARCHAR2,
                              E_telephone     VARCHAR2,
                              E_email_address VARCHAR2,
                              E_risk_id       NUMBER,
                              E_size_id       NUMBER,
                              E_up_status     VARCHAR2,
                              P_NO            NUMBER,
                              R_ID            NUMBER,
                              IND             VARCHAR2,
                              io_cursor       OUT t_cursor) AS
    v_new_id        NUMBER;
    v_pending_count NUMBER;
    AZ_ENT_ID       number;
  BEGIN
  
    SELECT COUNT(*)
      INTO v_pending_count
      FROM t_auditee_entities_update_Az
     WHERE entity_id = E_entity_id
       AND up_status = 'U';
  
    select e.auditby_id
      into AZ_ENT_ID
      from t_auditee_entities e
     where e.entity_id = E_entity_id;
  
    IF IND = 'R' then
      update t_auditee_entities_update_Az a
         set a.up_status = 'R'
       where a.entity_id = E_entity_id
         and a.up_status = 'U';
      COMMIT;
      OPEN io_cursor FOR
        SELECT 'Updation Rejected' AS remarks FROM dual;
    
    ELSIF IND = 'U' AND v_pending_count = 0 THEN
      SELECT NVL(MAX(ID), 0) + 1
        INTO v_new_id
        FROM t_auditee_entities_update_Az;
    
      INSERT INTO t_auditee_entities_update_Az
        (Id,
         Entity_Id,
         Code,
         Name,
         Active,
         Auditby_Id,
         Auditable,
         Address,
         Telephone,
         Email_Address,
         Risk_Id,
         Size_Id,
         Up_Status,
         Updated_By,
         Update_On)
      VALUES
        (v_new_id,
         E_entity_id,
         E_code,
         E_name,
         E_active,
         AZ_ENT_ID,
         E_auditable,
         E_address,
         E_telephone,
         E_email_address,
         E_risk_id,
         E_size_id,
         'U',
         P_NO,
         SYSDATE);
      COMMIT;
      OPEN io_cursor FOR
        SELECT 'Updation Submitted' AS remarks FROM dual;
    
    ELSIF IND = 'A' AND v_pending_count > 0 THEN
      UPDATE t_auditee_entities_update_Az
         SET up_status = 'A', authorized_by = P_NO, authorized_on = SYSDATE
       WHERE entity_id = E_entity_id
         AND up_status = 'U';
    
      UPDATE t_Auditee_Entities
         SET code          = E_code,
             name          = E_name,
             active        = E_active,
             auditable     = E_auditable,
             address       = E_address,
             telephone     = E_telephone,
             email_address = E_email_address,
             risk_id       = E_risk_id,
             size_id       = E_size_id
       WHERE entity_id = E_entity_id;
    
      COMMIT;
      OPEN io_cursor FOR
        SELECT 'Updation is Authorized' AS remarks FROM dual;
    
    ELSE
      -- Already a pending request exists, or no pending request to authorize
      OPEN io_cursor FOR
        SELECT 'Updation is already submitted for Authorization or no pending update to authorize' AS remarks
          FROM dual;
    
    END IF;
  
  END P_UPDATE_ENTITIES;

  procedure P_GetSubEntities(dept_code in number,
                             Div_id    in number,
                             io_cursor OUT t_cursor) as
  
  begin
    if (dept_code = 0) then
      OPEN io_cursor FOR
        Select s.*,
               m.p_name    as DIV_NAME,
               m.c_name    as DEPT_NAME,
               m.entity_id as ID,
               m.parent_id as DIV_ID
          FROM T_AUDITEE_ENTITEE_SUBENTITY s
         inner join t_Auditee_Entities_Maping m
            on s.enitity_id = m.entity_id
         WHERE s.STATUS = 'Y'
         order by s.NAME asc;
    else
      if (dept_code != 0) then
        OPEN io_cursor FOR
          Select s.*,
                 m.p_name    as DIV_NAME,
                 m.c_name    as DEPT_NAME,
                 m.entity_id as ID,
                 m.parent_id as DIV_ID
            FROM T_AUDITEE_ENTITEE_SUBENTITY s
           inner join t_Auditee_Entities_Maping m
              on s.enitity_id = m.entity_id
           WHERE s.STATUS = 'Y'
             and m.p_type_id = 4
           order by s.NAME asc;
      else
        if (Div_id = 0) then
          OPEN io_cursor FOR
            Select s.*,
                   m.p_name    as DIV_NAME,
                   m.c_name    as DEPT_NAME,
                   m.entity_id as ID,
                   m.parent_id as DIV_ID
              FROM T_AUDITEE_ENTITEE_SUBENTITY s
             inner join t_Auditee_Entities_Maping m
                on s.enitity_id = m.entity_id
             WHERE s.STATUS = 'Y'
               and m.p_type_id = 3
             order by s.NAME asc;
        else
          OPEN io_cursor FOR
            Select s.*,
                   m.p_name    as DIV_NAME,
                   m.c_name    as DEPT_NAME,
                   m.entity_id as ID,
                   m.parent_id as DIV_ID
              FROM T_AUDITEE_ENTITEE_SUBENTITY s
             inner join t_Auditee_Entities_Maping m
                on s.enitity_id = m.entity_id
             WHERE s.STATUS = 'Y'
             order by s.NAME asc;
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
        from v_get_parent_office mp
       where mp.c_type_id = E_id;
  
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

  procedure P_GetRisks(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select * from T_RISK R order by R.R_ID;
  
  end P_GetRisks;

  procedure P_GetRiskProcessDetails(procId    IN NUMBER,
                                    io_cursor OUT t_cursor) as
  
  begin
    if (procId = 0) THEN
    
      OPEN io_cursor FOR
        select * from t_audit_checklist t order by t.risk_sequence, t.T_ID;
    ELSE
    
      OPEN io_cursor FOR
        select *
          from t_audit_checklist_sub pd
         where pd.t_id = procId
         order by pd.s_id asc;
    END IF;
  end P_GetRiskProcessDetails;

  procedure P_get_checklist_update_byid(cd_id     IN NUMBER,
                                        io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      select e.id,
             e.PROCESS,
             e.SUB_PROCESS,
             e.Check_list,
             e.voilation,
             e.funtional_responible,
             e.Role_responsible,
             e.Risk,
             e.annexure,
             p.id,
             p.n_d_id,
             p.n_s_id,
             p.n_v_id,
             p.n_risk_id,
             p.n_role_resp_id,
             p.n_process_owner_id,
             p.n_owner_enitity_id,
             p.n_annex,
             p.New_SUB_PROCESS,
             p.new_Check_list,
             p.new_voilation,
             p.new_funtional_responible,
             p.new_Role_responsible,
             p.new_Risk,
             p.new_annexure,
             p.status
        from v_checklist_update_exiting e
       inner join v_checklist_update_propose p
          on e.id = p.id
       WHERE e.id = cd_id
      --and e.status = 'P'
       order by e.id asc;
  
  end P_get_checklist_update_byid;

  procedure P_get_sub_checklist_update_byid(Sid       IN NUMBER,
                                            io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      select e.s_id,
             e.t_id,
             p.n_t_id,
             p.n_s_id,
             e.PROCESS,
             e.SUB_PROCESS as sub_porcess,
             p.New_PROCESS as new_sub_process,
             p.New_SUB_PROCESS as New_Process,
             p.status,
             '' as comments
      
        from v_sub_checklist_update_exiting e
        left join v_sub_checklist_update_propose p
          on e.s_id = p.s_id
      
       WHERE e.s_id = sid
      --and e.status = 'P'
       order by e.s_id asc;
  
  end P_get_sub_checklist_update_byid;

  procedure P_get_checklist_update_byid_ref(cd_id     IN NUMBER,
                                            io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      select e.id,
             e.PROCESS,
             e.SUB_PROCESS,
             e.Check_list,
             e.voilation,
             e.funtional_responible,
             e.Role_responsible,
             e.Risk,
             e.annexure,
             p.PROCESS_ID,
             p.New_SUB_PROCESS,
             p.new_Check_list,
             p.new_voilation,
             p.new_funtional_responible,
             p.new_Role_responsible,
             p.new_Risk,
             p.new_annexure,
             p.status
        from v_checklist_update_exiting e
       inner join v_checklist_update_propose_id p
          on e.id = p.id
      
       WHERE e.id = cd_id
      --and e.status = 'P'
       order by e.id asc;
  
  end P_get_checklist_update_byid_ref;

  procedure p_Get_updated_Sub_Checklist_for_review(statusId  IN NUMBER,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select c.t_id,
             c.s_id,
             c.n_t_id,
             c.n_s_id,
             p.heading as Process,
             s.heading as sub_process,
             (select cb.heading
                from t_audit_checklist cb
               where c.n_t_id = cb.t_id) as New_Process,
             c.sub_process as new_sub_process,
             'Recommended For Approval' AS STATUS,
             d.comments
        from t_audit_checklist_sub s
       inner join t_audit_checklist p
          on s.T_ID = p.T_ID
       inner join t_audit_checklist_sub_change c
          on c.s_id = s.s_id
        left join v_sub_checklist_refferedback_comments v
          on v.s_id = s.s_id
        left join T_AUDIT_CHECKLIST_DETAILS_LOG d
          on d.id = v.id
       where c.status = case
               when statusId = 3 then
                'R'
               when statusId = 4 then
                'P'
             end
       order by s.s_id asc;
  
  end p_Get_updated_Sub_Checklist_for_review;

  procedure p_Get_updated_Checklist_for_review(statusId  IN NUMBER,
                                               ENT_ID    in number,
                                               P_NO      in number,
                                               R_ID      in number,
                                               io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      select s.description as Role_Responsible,
             d.name as CONTROL_OWNER,
             pt.id,
             pt.s_id,
             pt.heading,
             pt.v_id,
             pt.risk_id,
             pt.role_resp_id,
             pt.process_owner_id,
             pt.owner_enitity_id as owner_entity_id,
             pt.annex,
             RS.DESCRIPTION AS RISK_DESC,
             pd.HEADING as TITLE,
             p.HEADING as P_NAME,
             vc.DESCRIPTION as V_NAME,
             NVL(dd.comments, 'Recommended For Approval') AS STATUS
        from t_audit_checklist_details pt
       inner join t_audit_checklist_details_change ch
          on ch.id = pt.id
       inner join t_audit_checklist_sub pd
          on pt.S_ID = pd.S_ID
       inner join t_audit_checklist p
          on pd.T_ID = p.T_ID
       inner join t_r_sub_group vc
          on vc.S_GR_ID = pt.V_ID
       inner join t_hr_designations s
          on pt.role_resp_id = s.designationcode
       inner join t_Auditee_Entities d
          on pt.process_owner_id = d.entity_id
       INNER JOIN T_RISK RS
          ON PT.RISK_ID = RS.R_ID
        left join v_checklist_refferedback_comments v
          on v.d_id = pt.id
        left join T_AUDIT_CHECKLIST_DETAILS_LOG dd
          on dd.id = v.id
       where ch.status = case
               when statusid = 3 then
                'R'
               when statusId = 4 then
                'P'
             end
       order by pt.id asc;
  
  end p_Get_updated_Checklist_for_review;

  procedure p_Get_sub_Checklist_maker(processid in number,
                                      io_cursor OUT t_cursor) is
  
  begin
    if (processid = 0) then
      OPEN io_Cursor FOR
        select s.s_id,
               c.t_id,
               c.heading as Process,
               s.heading as sub_process,
               '' as comments,
               s.weight_assigned,
               s.risk_sequence
        
          from t_audit_checklist_sub s
         inner join t_audit_checklist c
            on s.t_id = c.t_id
         order by s.risk_sequence, s.heading;
    else
      OPEN io_Cursor FOR
        select t.s_id,
               t.t_id,
               t.PROCESS,
               t.SUB_PROCESS,
               d.comments,
               T.WEIGHT_ASSIGNED,
               T.RISK_SEQUENCE
          from v_sub_checklist_update_exiting t
          left join v_Sub_Checklist_Refferedback_Comments p
            on p.s_id = t.s_id
          left join T_AUDIT_CHECKLIST_DETAILS_LOG d
            on p.id = d.id
         where processid = case
                 when processid != 0 then
                  t.t_id
                 when processid = 0 then
                  processid
               end
         order by t.RISK_SEQUENCE, t.s_id;
    end if;
  end p_Get_sub_Checklist_maker;

  procedure p_GetChecklistDetailBySubProcessId(subProcessId in number,
                                               ENT_ID       in number,
                                               P_NO         in number,
                                               R_ID         in number,
                                               io_cursor    OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.id,
             t.s_id,
             t.heading,
             t.v_id,
             t.risk_id,
             t.role_resp_id,
             t.process_owner_id,
             t.status,
             t.owner_enitity_id as owner_entity_id,
             t.annex,
             p.heading          as T_NAME
        from t_audit_checklist_details t
       inner join t_audit_checklist_sub p
          on p.s_id = t.s_id
       inner join t_audit_checklist_details_change ch
          on ch.id = t.id
       where t.STATUS = 'Y'
         and ch.s_id = subProcessId;
    --and nvl(CH.status, 'Y') not in ('P', 'N', 'R');
  
  end p_GetChecklistDetailBySubProcessId;

  procedure p_GetChecklistDetail_ref(ENT_ID    in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.id,
             c.t_id             as p_id,
             t.s_id,
             t.heading,
             t.v_id,
             t.risk_id,
             t.role_resp_id,
             t.process_owner_id,
             t.status,
             t.owner_enitity_id as owner_entity_id,
             t.annex,
             p.heading          as T_NAME,
             l.comments         as comments
        from t_audit_checklist_details t
       inner join t_audit_checklist_sub p
          on p.s_id = t.s_id
       inner join t_audit_checklist c
          on p.t_id = c.t_id
       inner join t_audit_checklist_details_change ch
          on ch.id = t.id
       inner join v_checklist_refferedback_comments rf
          on rf.D_id = ch.id
       inner join t_audit_checklist_details_log l
          on l.id = rf.id
       where t.STATUS = 'Y'
         and ch.status = 'N';
  
  end p_GetChecklistDetail_ref;

  procedure P_GetChecklistDetailById(d_id      in number,
                                     io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.id as id,
             t.s_id,
             (select d.heading
                from t_audit_checklist_details d
               where d.id = d_id) as heading,
             t.v_id,
             t.role_resp_id,
             t.process_owner_id,
             t.risk_id,
             t.n_s_id as S_ID_NEW,
             t.n_heading as HEADING_NEW,
             t.n_v_id,
             t.n_role_resp_id,
             t.n_process_owner_id,
             t.n_risk_id
      
        from t_audit_checklist_details_change t
      
       where t.id = d_id
       order by t.s_id;
  
  end P_GetChecklistDetailById;

  procedure P_audit_checklist(p_name    in varchar2,
                              c_sec     in number,
                              c_weight  in varchar2,
                              RISK_ID   in number,
                              ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor) is
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
       212,
       P_NAME || '  New Process Added',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    insert into t_audit_checklist
      (T_ID, HEADING, ENTITY_TYPE, STATUS, weight_assigned, risk_sequence)
    VALUES
      ((select COALESCE(max(pp.T_ID) + 1, 1) from t_audit_checklist pp),
       P_NAME,
       RISK_ID,
       'Y',
       c_sec,
       c_weight);
    commit;
    open io_cursor for
      select 'Main Process added' as remarks from dual;
  
  end P_audit_checklist;

  procedure P_audit_checklist_update(tid       in number,
                                     p_name    in varchar2,
                                     active    in varchar2,
                                     c_sec     in number,
                                     c_weight  in varchar2,
                                     ENT_ID    in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor) is
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
       212,
       tid || ' ' || p_name || ' Updated',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    update t_audit_checklist s
       set s.heading         = p_name,
           s.status          = active,
           s.risk_sequence   = c_sec,
           s.weight_assigned = c_weight
     where s.t_id = tid;
    commit;
    open io_cursor for
      select 'Main Process Updated' as remarks from dual;
  end P_audit_checklist_update;

  procedure P_audit_checklist_sub(p_ID        in number,
                                  TITLE       in varchar2,
                                  s_sec       in number,
                                  s_weight    in varchar2,
                                  ENTITY_TYPE in varchar2,
                                  ENT_ID      in number,
                                  P_NO        in number,
                                  R_ID        in number,
                                  io_cursor   OUT t_cursor) is
    E_F number := 0;
  begin
  
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
       214,
       p_ID || ' ' || TITLE || ' Updated',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    insert into T_AUDIT_CHECKLIST_SUB_CHANGE
      (S_ID,
       N_S_ID,
       N_T_ID,
       SUB_PROCESS,
       STATUS,
       WEIGHT_ASSIGNED,
       RISK_SEQUENCE)
    
    VALUES
      ((select COALESCE(max(pp.S_ID) + 1, 1)
         from T_AUDIT_CHECKLIST_SUB_CHANGE pp),
       (select COALESCE(max(p.S_ID) + 1, 1)
          from T_AUDIT_CHECKLIST_SUB_CHANGE p),
       P_ID,
       TITLE,
       'P',
       s_sec,
       s_weight);
    commit;
  
    insert into T_AUDIT_CHECKLIST_SUB
      (S_ID,
       T_ID,
       HEADING,
       ENTITY_TYPE,
       STATUS,
       WEIGHT_ASSIGNED,
       RISK_SEQUENCE)
    VALUES
      ((select COALESCE(max(pp.S_ID) + 1, 1) from t_audit_checklist_sub pp),
       P_ID,
       TITLE,
       '6',
       'N',
       s_sec,
       s_weight);
    commit;
  
    insert into T_AUDIT_CHECKLIST_DETAILS_LOG p
      (ID, S_ID, T_ID, STATUS_ID, COMMENTS, p.created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       (select max(p.s_id) from T_AUDIT_CHECKLIST_SUB_CHANGE p),
       P_ID,
       '3',
       ' NEW Sub Process Added and submitted for Review',
       sysdate);
    commit;
  
    open io_cursor for
      select 'Sub Process Added and Forwarded to Reviewer' as remarks
        from dual;
  
  end P_audit_checklist_sub;

  procedure P_get_checklistdetail_for_subchecklist(sid       in number,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) is
  begin
  
    open io_cursor for
      select d.heading as details
        from t_audit_checklist_details d
       where d.s_id = sid;
  
  end P_get_checklistdetail_for_subchecklist;

  procedure P_audit_checklist_sub_update(TID         in number,
                                         N_TID       in number,
                                         sid         in number,
                                         TITLE       in varchar2,
                                         s_sec       in number,
                                         s_wieght    in number,
                                         ENTITY_TYPE in number,
                                         ENT_ID      in number,
                                         P_NO        in number,
                                         R_ID        in number,
                                         io_cursor   OUT t_cursor) is
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
       203,
       SID || ' ' || TITLE || ' Updated',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    update T_AUDIT_CHECKLIST_SUB_CHANGE p
       set p.t_id              = TID,
           p.n_t_id            = N_TID,
           p.sub_process       = title,
           p.status            = 'P',
           p.n_s_id            = sid,
           p.n_weight_assigned = s_wieght,
           p.n_risk_sequence   = s_sec
     where p.s_id = sid;
    commit;
    insert into T_AUDIT_CHECKLIST_DETAILS_LOG cl
      (ID, S_ID, T_ID, STATUS_ID, COMMENTS, cl.created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       sid,
       tid,
       '3',
       'Sub Process Updated and submitted for Review',
       sysdate);
    commit;
    open io_cursor for
      select 'Sub Process is updated and forwarded to Reviewer' as remarks
        from dual;
  
  end P_audit_checklist_sub_update;

  Procedure p_get_annexure_process(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select p.id, p.heading, p.audit_comments, p.automation, p.monitoring
        from t_audit_checklist_annexure_process p;
  
  end p_get_annexure_process;

  Procedure p_get_annexure(ENT_ID    in number,
                           P_NO      in number,
                           R_ID      in number,
                           io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select a.id,
             a.code || '  ' || a.heading as annex,
             a.heading as heading,
             a.code as code,
             a.vol,
             a.risk as risk_id,
             nvl(r.description, '-') as risk,
             a.function as function_id,
             nvl(e.name, '-') as function,
             a.co_function_1 as function_id_1,
             nvl(ee.name, '-') as function_1,
             a.co_function_2 as function_id_2,
             nvl(eee.name, '-') as function_2,
             a.status,
             P.ID AS PROCESS_ID,
             P.HEADING AS PROCESS,
             a.max_number,
             a.weightage,
             a.gravity
      
        from T_AUDIT_CHECKLIST_ANNEXURE a
       INNER JOIN T_AUDIT_CHECKLIST_ANNEXURE_PROCESS P
          ON P.ID = A.MAIN_PROCESS
        left join t_risk r
          on a.risk = r.r_id
        left join t_auditee_entities e
          on a.function = e.entity_id
        left join t_auditee_entities ee
          on a.co_function_1 = ee.entity_id
      
        left join t_auditee_entities eee
          on a.co_function_2 = eee.entity_id
       order by p.id, a.risk, a.id;
  
  end p_get_annexure;

  Procedure p_update_annexure(ENT_ID        in number,
                              P_NO          in number,
                              R_ID          in number,
                              anexx         in number,
                              title         in varchar2,
                              risk_id       in number,
                              owner         in number,
                              FUNCTION_ID_1 in number,
                              FUNCTION_ID_2 in number,
                              process_id    in number,
                              max_num       in varchar2,
                              weightage_num in varchar2,
                              gravity_num   in varchar2,
                              io_cursor     OUT t_cursor) is
    N_F number := 0;
  begin
  
    N_F := anexx;
    update T_AUDIT_CHECKLIST_ANNEXURE a
       set a.heading       = Title,
           a.risk          = Risk_id,
           a.function      = owner,
           a.main_process  = process_id,
           a.max_number    = max_num,
           a.weightage     = weightage_num,
           a.gravity       = gravity_num,
           a.co_function_1 = FUNCTION_ID_1,
           a.co_function_2 = FUNCTION_ID_2
     where a.id = anexx;
    commit;
  
    open io_cursor for
      select 'ANNEXURE Updated' as remarks from dual;
  
    Update t_au_observation o set o.severity = risk_id where o.annex = N_F;
    commit;
  
    UPDATE AIS_T_AU_POST_COMPLIANCE C
       SET C.RISK = risk_id
     WHERE C.ANNEX = N_F;
    commit;
  
    update t_au_old_paras_fad f set f.risk = risk_id where f.annex = N_F;
    commit;
  
  end p_update_annexure;

  Procedure P_add_annexure(ENT_ID        in number,
                           P_NO          in number,
                           R_ID          in number,
                           code          in varchar2,
                           title         in varchar2,
                           risk_id       in number,
                           owner         in number,
                           FUNCTION_ID_1 in number,
                           FUNCTION_ID_2 in number,
                           PROCESS_ID    IN NUMBER,
                           max_num       in varchar2,
                           weightage_num in varchar2,
                           gravity_num   in varchar2,
                           io_cursor     OUT t_cursor) is
  
  begin
    insert into T_AUDIT_CHECKLIST_ANNEXURE
      (ID,
       CODE,
       HEADING,
       STATUS,
       RISK,
       FUNCTION,
       CO_FUNCTION_1,
       CO_FUNCTION_2,
       MAIN_PROCESS,
       MAX_NUMBER,
       WEIGHTAGE,
       GRAVITY)
    values
      ((SELECT COALESCE(max(s.id) + 1, 1) FROM T_AUDIT_CHECKLIST_ANNEXURE s),
       code,
       title,
       'Y',
       risk_id,
       owner,
       FUNCTION_ID_1,
       FUNCTION_ID_2,
       PROCESS_ID,
       max_num,
       weightage_num,
       gravity_num);
    commit;
    OPEN io_cursor FOR
      SELECT 'Annexure Added' AS REMARKS FROM DUAL;
  end P_add_annexure;

  procedure P_audit_checklist_detail(p_id          in number,
                                     SID           in number,
                                     DESCRIPTION   in varchar2,
                                     VID           in number,
                                     CONTROL_OWNER in number,
                                     role          in number,
                                     RISK          in number,
                                     Annexure      in number,
                                     ENT_ID        in number,
                                     P_NO          in number,
                                     R_ID          in number,
                                     io_cursor     OUT t_cursor) is
  
    Z_B number := 0;
  begin
    if (P_NO is not null) then
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
         204,
         DESCRIPTION || ' Added as New Check List',
         sysdate,
         (select COALESCE(max(l.seq) + 1, 1)
            from t_au_activity_log l
           where l.id = Z_B
             and l.ppnum = P_NO),
         'Y');
      commit;
    
      insert into t_audit_checklist_details_change
        (id,
         n_s_Id,
         n_v_id,
         n_Risk_Id,
         n_Role_Resp_Id,
         n_owner_enitity_id,
         n_annex,
         n_heading,
         status)
      
      VALUES
        ((select COALESCE(max(pp.ID) + 1, 1)
           from t_audit_checklist_details_change pp),
         SID,
         VID,
         risk,
         role,
         CONTROL_OWNER,
         Annexure,
         DESCRIPTION,
         'P');
      commit;
    
      insert into t_audit_checklist_details
        (id,
         s_id,
         heading,
         v_id,
         risk_id,
         role_resp_id,
         process_owner_id,
         status,
         owner_enitity_id,
         annex)
      VALUES
        ((select COALESCE(max(pp.ID) + 1, 1)
           from t_audit_checklist_details pp),
         SID,
         DESCRIPTION,
         VID,
         risk,
         role,
         CONTROL_OWNER,
         'P',
         CONTROL_OWNER,
         Annexure);
      commit;
    
      insert into t_audit_checklist_details_log p
        (p.ID,
         p.T_ID,
         p.STATUS_ID,
         p.USER_ID,
         p.COMMENTS,
         d_id,
         p.created_on)
      VALUES
        ((select COALESCE(max(pp.ID) + 1, 1)
           from t_audit_checklist_details_log pp),
         (select max(tp.ID) from t_audit_checklist_details tp),
         '1',
         P_NO,
         'New Transaction Added',
         p_id,
         sysdate);
      commit;
    
      open io_cursor for
        select 'Check list added and forwarded to reviewer' as remarks
          from dual;
    else
      open io_cursor for
        select 'Your Session Terminated by Host. Logout and Login again, also inform Administrator' as remarks
          from dual;
    end if;
  end P_audit_checklist_detail;

  procedure audit_checklist_detail_update(Did           in number,
                                          SID           in number,
                                          DESCRIPTION   in varchar2,
                                          VID           in number,
                                          CONTROL_OWNER in number,
                                          role          in number,
                                          RISK          in number,
                                          Annexure      in number,
                                          ENT_ID        in number,
                                          P_NO          in number,
                                          R_ID          in number,
                                          io_cursor     OUT t_cursor) is
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
       204,
       DID || ' Check List has been updated by ' || P_NO,
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    update T_AUDIT_CHECKLIST_DETAILS_CHANGE p
       set p.n_s_id             = SID,
           p.n_heading          = DESCRIPTION,
           p.n_v_id             = vid,
           p.n_owner_enitity_id = CONTROL_OWNER,
           p.n_risk_id          = RISK,
           p.n_role_resp_id     = role,
           p.n_annex            = Annexure,
           p.status             = 'P',
           p.updated_on         = sysdate
     where p.id = did;
  
    commit;
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS, p.CREATED_ON)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       (select max(tp.ID) from t_audit_checklist_details tp),
       '1',
       P_NO,
       'Check List Updated',
       sysdate);
    commit;
    open io_cursor for
      select 'Check list updated and forwarded to reviewer' as remarks
        from dual;
  end audit_checklist_detail_update;

  procedure audit_checklist_details_log(ppnumber in t_audit_checklist_details_log.user_id%type,
                                        comments in t_audit_checklist_details_log.comments%type,
                                        t_id     in t_audit_checklist_details_log.t_id%type) is
  begin
    insert into t_audit_checklist_details_log p
      (p.ID, p.T_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS, D_ID, CREATED_ON)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '3',
       PPNumber,
       COMMENTS,
       t_id,
       sysdate);
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

  procedure P_Recommend_Checklist_By_Reviewer(DID           in number,
                                              SID           in number,
                                              DESCRIPTION   in varchar2,
                                              VID           in number,
                                              CONTROL_OWNER in number,
                                              role          in number,
                                              RISK          in number,
                                              Annexure      in number,
                                              ENT_ID        in number,
                                              P_NO          in number,
                                              R_ID          in number,
                                              T_ID          in number,
                                              COMMENTS      in varchar2,
                                              io_cursor     OUT t_cursor) is
  
    E_F number := 0;
  begin
  
    update t_audit_checklist_details_change tm
       SET tm.n_s_id             = sid,
           tm.n_heading          = DESCRIPTION,
           tm.n_v_id             = vid,
           tm.n_risk_id          = risk,
           tm.n_role_resp_id     = role,
           tm.n_annex            = annexure,
           tm.n_process_owner_id = CONTROL_OWNER,
           tm.n_owner_enitity_id = CONTROL_OWNER,
           tm.status             = 'R'
    
     WHERE tm.id = t_id;
    commit;
  
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
       206,
       'Checklist ' || T_ID || ' is recommend by' || P_NO,
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    insert into t_audit_checklist_details_log p
      (p.ID, p.D_ID, p.STATUS_ID, p.USER_ID, p.COMMENTS, p.created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '3',
       P_NO,
       COMMENTS,
       sysdate);
    commit;
    open io_cursor for
      select T_ID || ' has been recommened' as remarks from dual;
  
  end p_Recommend_Checklist_By_Reviewer;

  procedure P_RefferedBack_checklist_By_Reviewer(T_ID      in number,
                                                 COMMENTS  IN varchar2,
                                                 ENT_ID    in number,
                                                 P_NO      in number,
                                                 R_ID      in number,
                                                 io_cursor OUT t_cursor) is
  
    E_F number := 0;
  begin
  
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
       204,
       'Check list ' || T_ID || ' is reffered Back By Authorizer',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    update t_audit_checklist_details_change tm
       SET tm.status = 'N'
     WHERE tm.id = t_id;
    commit;
    insert into t_audit_checklist_details_log p
      (p.ID,
       p.T_ID,
       p.STATUS_ID,
       p.USER_ID,
       p.COMMENTS,
       p.d_id,
       p.created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '4',
       P_NO,
       COMMENTS,
       t_id,
       sysdate);
    commit;
    open io_cursor for
      select T_ID || ' has been rejected' as remarks from dual;
  
  end p_RefferedBack_checklist_By_Reviewer;

  procedure p_RefferedBack_Sub_checklist_By_Reviewer(SID       in number,
                                                     COMMENTS  IN varchar2,
                                                     ENT_ID    in number,
                                                     R_ID      in number,
                                                     P_NO      in number,
                                                     io_cursor OUT t_cursor) is
  
  begin
  
    update t_audit_checklist_sub_change Sm
       SET sm.status = 'N'
     WHERE sm.s_id = SID;
    commit;
    insert into t_audit_checklist_details_log
      (ID, S_ID, STATUS_ID, USER_ID, COMMENTS, created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       SID,
       '3',
       P_NO,
       COMMENTS,
       sysdate);
    commit;
    open io_cursor for
      select SID || ' has been rejected' as remarks from dual;
  
  end p_RefferedBack_Sub_checklist_By_Reviewer;

  procedure p_Approved_Sub_Process_By_Authorizer(SID       in number,
                                                 COMMENTS  IN varchar2,
                                                 ENT_ID    in number,
                                                 P_NO      in number,
                                                 R_ID      in number,
                                                 io_cursor OUT t_cursor) is
    V_F number := 0;
    E_F number := 0;
  begin
  
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
       214,
       SID || ' Sub Process is Auhtorized by ' || P_NO,
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select SID into V_F from dual;
    update t_audit_checklist_sub_change a
       SET a.s_id            = a.n_s_id,
           a.t_id            = a.n_t_id,
           a.status          = 'Y',
           a.weight_assigned = a.n_weight_assigned,
           a.risk_sequence   = a.n_risk_sequence
    
     WHERE a.s_id = SID;
    commit;
  
    update t_audit_checklist_sub z
       SET Z.T_ID           =
           (SELECT e.t_id
              FROM t_audit_checklist_sub_change e
             where e.s_id = z.s_id),
           Z.HEADING        =
           (SELECT e.sub_process
              FROM t_audit_checklist_sub_change e
             where e.s_id = z.s_id),
           z.status          = 'Y',
           z.weight_assigned =
           (SELECT e.weight_assigned
              FROM t_audit_checklist_sub_change e
             where e.s_id = z.s_id),
           z.risk_sequence  =
           (SELECT e.risk_sequence
              FROM t_audit_checklist_sub_change e
             where e.s_id = z.s_id)
     WHERE z.s_id = SID;
    commit;
  
    insert into t_audit_checklist_details_log
      (ID, T_ID, STATUS_ID, USER_ID, COMMENTS, created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       SID,
       '3',
       P_NO,
       COMMENTS,
       sysdate);
    commit;
    open io_cursor for
      select SID || ' has been Authorized' as remarks from dual;
  
  end p_Approved_Sub_Process_By_Authorizer;

  procedure p_RefferedBack_checklist_By_Authorizer(T_ID      in number,
                                                   COMMENTS  IN varchar2,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) is
  
    E_F number := 0;
  begin
  
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
       213,
       T_ID || ' is Reffered Back By Authorizer',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    update t_audit_checklist_details_change t
       SET t.status = 'N'
     WHERE t.id = t_id;
    commit;
    insert into t_audit_checklist_details_log
      (ID, T_ID, STATUS_ID, USER_ID, COMMENTS, D_ID, created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '4',
       P_NO,
       COMMENTS,
       T_ID,
       sysdate);
    commit;
    open io_cursor for
      select T_ID || ' has been rejected' as remarks from dual;
  end p_RefferedBack_checklist_By_Authorizer;

  procedure p_approve_checklist_By_Authorizer(T_ID      in number,
                                              COMMENTS  in varchar2,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor) is
    V_F number := 0;
    S_F number := 0;
    E_F number := 0;
  
  begin
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
       31,
       T_ID || ' has been Authorized',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select NVL(dd.s_id, 0)
      into V_F
      from t_audit_checklist_details dd
     where dd.id = t_id;
  
    select NVL(cd.n_s_id, 0)
      into S_F
      from t_audit_checklist_details_change cd
     where cd.id = t_id;
  
    update t_audit_checklist_details_change a
       SET a.s_id             = a.n_s_id,
           a.v_id             = a.n_v_id,
           a.risk_id          = a.n_risk_id,
           a.role_resp_id     = a.n_role_resp_id,
           a.process_owner_id = a.n_owner_enitity_id,
           a.owner_enitity_id = a.n_owner_enitity_id,
           a.annex            = a.n_annex,
           a.status           = 'Y'
     WHERE a.id = t_id;
    commit;
  
    update t_audit_checklist_details z
       SET Z.S_ID            =
           (SELECT e.s_id
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.HEADING         =
           (SELECT e.n_heading
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.V_ID            =
           (SELECT e.v_id
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.RISK_ID         =
           (SELECT e.risk_id
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.ROLE_RESP_ID    =
           (SELECT e.role_resp_id
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.PROCESS_OWNER_ID =
           (SELECT e.owner_enitity_id
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.OWNER_ENiTITY_ID =
           (SELECT e.owner_enitity_id
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           Z.ANNEX           =
           (SELECT e.annex
              FROM t_audit_checklist_details_change e
             where e.id = z.id),
           z.status           = 'Y'
    
     WHERE z.id = t_id;
    commit;
  
    if (V_F != S_F) then
      update t_au_observation o
         set o.subchecklist_id =
             (select d.s_id
                from t_audit_checklist_details d
               where d.id = o.checklistdetail_id)
       where o.checklistdetail_id = t_id;
      commit;
    end if;
  
    insert into t_audit_checklist_details_log p
      (p.ID,
       p.T_ID,
       p.STATUS_ID,
       p.USER_ID,
       p.COMMENTS,
       p.d_id,
       p.created_on)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       T_ID,
       '3',
       P_NO,
       COMMENTS,
       t_id,
       sysdate);
    commit;
    open io_cursor for
      select T_ID || ' has been Approved / Authorized' as remarks
        from dual;
  
  end p_approve_checklist_By_Authorizer;

  procedure P_GetLatestCommentsOnProcess(procId    IN NUMBER,
                                         io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select l.comments
        from t_audit_checklist_details_log l
       where l.t_id = procId
       order by l.created_on desc
       FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestCommentsOnProcess;

  --extra

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

  --Post changes
  procedure p_get_audit_team_postchanges(ENT_ID    in number,
                                         P_NO      in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor) is
  
  begin
  
    open io_cursor for
      select eng.ENG_ID,
             tm.t_name               as team_name,
             t.entity_name           as ENTITY_NAME,
             eng.AUDIT_STARTDATE,
             eng.AUDIT_ENDDATE,
             eng.operation_startdate as OP_STARTDATE,
             eng.operation_enddate   as OP_ENDDATE,
             eng.ENTITY_ID
        from t_au_audit_team_tasklist t
       inner join t_au_team_members m
          on m.t_id = t.team_id
       inner join t_au_audit_teams tm
          on tm.team_id = m.t_id
       inner join t_au_plan_eng eng
          on eng.eng_id = t.eng_plan_id
      
       where t.eng_plan_id = ENT_ID;
  
  end p_get_audit_team_postchanges;

  procedure P_GetAuditTeamsForEngReversal(AuditedByDept IN NUMBER,
                                           CurrentTeamID IN NUMBER,
                                           P_NO          IN NUMBER,
                                           R_ID          IN NUMBER,
                                           io_cursor     OUT t_cursor) is
  begin
    open io_cursor for
      select t.*, d.name as AUDIT_DEPARTMENT
        from t_au_team_members t
       inner join t_auditee_entities d
          on d.entity_id = t.place_of_posting
       where t.place_of_posting = AuditedByDept
         and (t.status = 'Y' or t.t_id = CurrentTeamID)
       order by case when t.t_id = CurrentTeamID then 0 else 1 end,
                t.t_id,
                t.isteamlead desc,
                t.member_name;
  end P_GetAuditTeamsForEngReversal;

  procedure p_audit_team_postchanges(ENGID     in number,
                                     PPNO      in number,
                                     Teamid    in number,
                                     audid     in number,
                                     teamname  in varchar2,
                                     io_cursor OUT t_cursor) is
  
    team_id number := 0;
  begin
  
    select nvl(max(tm.team_id), 0)
      into team_id
      from t_au_audit_teams tm
     where tm.eng_id = engid
       and tm.team_id = TEAMID;
  
    if (team_id != TEAMID) then
      DELETE FROM T_AU_AUDIT_TEAMS T WHERE T.ENG_ID = ENGID;
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
         teamname,
         TEAMID,
         audid,
         1);
      COMMIT;
    else
      team_id := -1;
    end if;
  
    delete from T_AU_AUDIT_TEAM_TASKLIST lt where lt.eng_plan_id = engid;
    commit;
  
    for JJ in (SELECT * FROM t_au_team_members MT where MT.t_id = TEAMID) loop
    
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
        SELECT (SELECT COALESCE(max(ll.ID) + 1, 1)
                  FROM T_AU_AUDIT_TEAM_TASKLIST ll),
               EE.ENG_ID,
               TEAMID,
               (SELECT COALESCE(max(lp.sequence_no) + 1, 1)
                  FROM T_AU_AUDIT_TEAM_TASKLIST lp
                 where lp.teammember_ppno = JJ.MEMBER_PPNO),
               JJ.MEMBER_PPNO,
               EE.ENTITY_ID,
               EE.ENTITY_CODE,
               e.name,
               EE.AUDIT_STARTDATE,
               EE.AUDIT_ENDDATE,
               1,
               'Y'
          FROM T_AU_PLAN_ENG EE
         inner join t_auditee_entities e
            on e.entity_id = ee.entity_id
         WHERE EE.ENG_ID = ENGID
        --AND EE.TEAM_ID = JJ.T_ID
        ;
      COMMIT;
    end loop;
  
    update t_au_plan_eng ep
       set ep.team_id = Teamid, ep.team_name = teamname
     where ep.eng_id = engid;
    commit;
  
    open io_cursor for
      select 'Team Has been changed' as remarks from dual;
  
  end p_audit_team_postchanges;

  procedure p_get_audit_engagement(ent_id    in number,
                                   io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select ep.id                   as plan_id,
             eng.ENG_ID,
             nvl(eng.team_name, tm.t_name) as TEAM_NAME,
             eng.team_id             as TEAM_ID,
             eng.AUDIT_STARTDATE,
             eng.AUDIT_ENDDATE,
             eng.operation_startdate as OP_STARTDATE,
             eng.operation_enddate   as OP_ENDDATE,
             eng.ENTITY_ID,
             eng.Auditby_Id,
             s.id                    as status_id,
             s.status
        from t_au_plan_eng eng
       inner join t_au_period p
          on eng.period_id = p.auditperiodid
        left join t_au_plan ep
          on ep.id = eng.plan_id
       inner join t_au_plan_eng_status s
          on eng.status = s.id
        left join t_au_audit_teams tm
          on eng.eng_id = tm.eng_id
         and eng.team_id = tm.team_id
      
       where eng.entity_id = ent_id
         and eng.status < 16;
  
  end p_get_audit_engagement;

  procedure P_GET_SHIFTABLE_AUDIT_ENGAGEMENT(ENT_ID    IN NUMBER,
                                             IO_CURSOR OUT T_CURSOR) is
  begin
    open IO_CURSOR for
      select ep.id                         as plan_id,
             eng.ENG_ID,
             nvl(eng.team_name, tm.t_name) as TEAM_NAME,
             eng.team_id                   as TEAM_ID,
             eng.AUDIT_STARTDATE,
             eng.AUDIT_ENDDATE,
             eng.operation_startdate       as OP_STARTDATE,
             eng.operation_enddate         as OP_ENDDATE,
             eng.ENTITY_ID,
             eng.Auditby_Id,
             s.id                          as status_id,
             s.status
        from T_AU_PLAN_ENG eng
       inner join T_AU_PERIOD p
          on eng.period_id = p.auditperiodid
        left join T_AU_PLAN ep
          on ep.id = eng.plan_id
       inner join T_AU_PLAN_ENG_STATUS s
          on eng.status = s.id
        left join T_AU_AUDIT_TEAMS tm
          on eng.eng_id = tm.eng_id
         and eng.team_id = tm.team_id
       where eng.entity_id = ENT_ID
         and not exists
       (select 1 from T_FRPT_REPORT_META frm where frm.eng_id = eng.eng_id);
  end P_GET_SHIFTABLE_AUDIT_ENGAGEMENT;

  procedure p_get_audit_engagement_status(engid     in number,
                                          io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select e.status into V_F from t_au_plan_eng e where e.eng_id = engid;
  
    if (V_F < 10) then
      open io_cursor for
        select s.id, s.status
          from t_au_plan_eng_status s
         where s.id in (1, 2, 3, 4, 5, 7, 8, 9)
         order by s.id;
    elsif (V_F > 10) then
      open io_cursor for
        select s.id, s.status
          from t_au_plan_eng_status s
         where s.id in (10, 11, 12, 13)
         order by s.id;
    end if;
  
  end p_get_audit_engagement_status;

  procedure p_audit_engagement_reversal(engid     in number,
                                        sid       in number,
                                        p_id      in number,
                                        comments  in varchar2,
                                        P_NO      in number,
                                        io_cursor OUT t_cursor) is
    v_f             number := 0;
    v_target_engid  number := engid;
  begin
  
    if (sid in (1)) then
      select e.plan_id
        into V_F
        from t_au_plan_eng e
       where e.eng_id = engid;
      update t_au_plan e set e.status = SID where e.id = p_ID;
      commit;
    
      delete from t_au_audit_teams tm where tm.eng_id in (engid);
      commit;
    
      delete from t_au_audit_team_tasklist t
       where t.eng_plan_id in (engid);
      commit;
    
      Delete from t_au_sample_branch s where s.eng_id = engid;
      commit;
    
      delete from t_exception_accounts_cust where eng_id = engid;
    
      delete from t_exception_accounts_txn where eng_id = engid;
    
      delete from t_exception_eng where eng_id = engid;
    
      delete from t_exception_eng_branches b
       where b.engid = v_target_engid;
    
      delete from t_exception_accounts where eng_id = engid;
    
      delete from t_exception_accounts_data where eng_id = engid;
      commit;
    
      delete from t_au_plan_eng e where e.eng_id in (engid);
      commit;
    
      insert into t_au_plan_eng_log
        (id, e_id, status_id, createdby_id, created_on, remarks)
      VALUES
        ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
         ENGID,
         '10',
         P_NO,
         to_date(sysdate, 'dd/mm/yyyy HH:MI:SS AM'),
         comments);
      commit;
    else
    
      update t_au_plan_eng e set e.status = sid where e.eng_id in (engid);
      commit;
    
      insert into t_au_plan_eng_log
        (id, e_id, status_id, createdby_id, created_on, remarks)
      VALUES
        ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
         ENGID,
         '10',
         P_NO,
         to_date(sysdate, 'dd/mm/yyyy HH:MI:SS AM'),
         comments);
      commit;
    end if;
    open io_cursor for
      select 'Audit Engagement has been reversed' as remarks from dual;
  
  end p_audit_engagement_reversal;

  procedure P_SHIFT_ENGAGEMENT_ENTITY(P_ENG_ID        IN NUMBER,
                                      P_NEW_ENTITY_ID IN NUMBER,
                                      P_PPNO          IN NUMBER,
                                      P_ROLE_ID       IN NUMBER,
                                      P_REASON        IN VARCHAR2,
                                      IO_CURSOR       OUT T_CURSOR) is
    V_OLD_ENTITY_ID           NUMBER;
    V_DEST_COUNT              NUMBER := 0;
    V_REPORT_COUNT            NUMBER := 0;
    V_PLAN_ENG_ROWS           NUMBER := 0;
    V_OBSERVATION_ROWS        NUMBER := 0;
    V_AIS_OBSERVATION_ROWS    NUMBER := 0;
    V_OBS_ASSIGNEDTO_ROWS     NUMBER := 0;
    V_TEAM_TASKLIST_ROWS      NUMBER := 0;
  begin
    if P_ROLE_ID not in (1) then
      open IO_CURSOR for
        select 'FALSE' as status,
               'Unauthorized: engagement entity shifting is restricted to authorized administrators.' as remarks
          from dual;
      return;
    end if;

    if P_REASON is null or length(trim(P_REASON)) = 0 then
      open IO_CURSOR for
        select 'FALSE' as status, 'Reason / remarks are mandatory.' as remarks
          from dual;
      return;
    end if;

    begin
      select ENG.ENTITY_ID
        into V_OLD_ENTITY_ID
        from T_AU_PLAN_ENG ENG
       where ENG.ENG_ID = P_ENG_ID
       for update;
    exception
      when NO_DATA_FOUND then
        open IO_CURSOR for
          select 'FALSE' as status, 'Invalid Engagement ID.' as remarks
            from dual;
        return;
    end;

    select count(*)
      into V_DEST_COUNT
      from T_AUDITEE_ENTITIES E
     where E.ENTITY_ID = P_NEW_ENTITY_ID
       and nvl(E.ACTIVE, 'Y') = 'Y';

    if V_DEST_COUNT = 0 then
      open IO_CURSOR for
        select 'FALSE' as status, 'Invalid or inactive destination Entity ID.' as remarks
          from dual;
      return;
    end if;

    if V_OLD_ENTITY_ID = P_NEW_ENTITY_ID then
      open IO_CURSOR for
        select 'FALSE' as status, 'Destination entity cannot be the current entity.' as remarks
          from dual;
      return;
    end if;

    select count(*)
      into V_REPORT_COUNT
      from T_FRPT_REPORT_META
     where ENG_ID = P_ENG_ID;

    if V_REPORT_COUNT > 0 then
      open IO_CURSOR for
        select 'FALSE' as status,
               'Final Report has already been issued. Engagement cannot be shifted.' as remarks
          from dual;
      return;
    end if;

    update T_AU_PLAN_ENG
       set ENTITY_ID = P_NEW_ENTITY_ID, LASTUPDATEDBY = P_PPNO, LASTUPDATEDDATE = sysdate
     where ENG_ID = P_ENG_ID;
    V_PLAN_ENG_ROWS := sql%rowcount;

    update T_AU_OBSERVATION set ENTITY_ID = P_NEW_ENTITY_ID where ENGPLANID = P_ENG_ID;
    V_OBSERVATION_ROWS := sql%rowcount;

    update AIS_T_AU_OBSERVATION set ENTITY_ID = P_NEW_ENTITY_ID where ENGPLANID = P_ENG_ID;
    V_AIS_OBSERVATION_ROWS := sql%rowcount;

    update T_AU_OBSERVATION_ASSIGNEDTO set ENTITY_ID = P_NEW_ENTITY_ID where ENG_ID = P_ENG_ID;
    V_OBS_ASSIGNEDTO_ROWS := sql%rowcount;

    update T_AU_AUDIT_TEAM_TASKLIST set ENTITY_ID = P_NEW_ENTITY_ID where ENG_PLAN_ID = P_ENG_ID;
    V_TEAM_TASKLIST_ROWS := sql%rowcount;

    if V_PLAN_ENG_ROWS <> 1 then
      raise_application_error(-20041, 'Engagement shift failed: engagement master row was not updated exactly once.');
    end if;

    insert into T_AU_ENG_ENTITY_SHIFT_HIST
      (ID, ENG_ID, OLD_ENTITY_ID, NEW_ENTITY_ID, REASON, PPNO, ROLE_ID, SHIFTED_ON,
       PLAN_ENG_ROWS, OBSERVATION_ROWS, AIS_OBSERVATION_ROWS, OBS_ASSIGNEDTO_ROWS,
       TEAM_TASKLIST_ROWS)
    values
      (SEQ_AU_ENG_ENTITY_SHIFT_HIST.nextval, P_ENG_ID, V_OLD_ENTITY_ID, P_NEW_ENTITY_ID,
       trim(P_REASON), P_PPNO, P_ROLE_ID, sysdate, V_PLAN_ENG_ROWS, V_OBSERVATION_ROWS,
       V_AIS_OBSERVATION_ROWS, V_OBS_ASSIGNEDTO_ROWS, V_TEAM_TASKLIST_ROWS);

    commit;

    open IO_CURSOR for
      select 'TRUE' as status,
             'Engagement ' || P_ENG_ID || ' shifted from Entity ' || V_OLD_ENTITY_ID ||
             ' to Entity ' || P_NEW_ENTITY_ID || '.' as remarks
        from dual;
  exception
    when others then
      rollback;
      open IO_CURSOR for
        select 'FALSE' as status,
               'Engagement entity shift failed: ' || substr(sqlerrm, 1, 900) as remarks
          from dual;
  end P_SHIFT_ENGAGEMENT_ENTITY;

  procedure p_get_audit_observtion_status(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select s.statusid, s.statusname, s.isactive, s.code, s.satisfied
        from t_au_observation_status s
       order by s.statusid;
  
  end p_get_audit_observtion_status;

  procedure p_get_audit_observtion(ENGID in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select o.id,
             ot.obs_id,
             o.memo_number   as memo_no,
             o.draft_para_no,
             O.FINAL_PARA_NO,
             o.memo_date,
             e.name          as assigned_to,
             s.statusname    as status,
             t.headings,
             t.headings      as gist,
             r.description   as RISK
        from t_au_observation o
       inner join t_au_observation_status s
          on s.statusid = o.status
       inner join t_au_observation_assignedto ot
          on ot.obs_id = o.id
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_auditee_entities e
          on e.entity_id = ot.entity_id
       inner join t_risk r
          on r.r_id = o.severity
       where o.engplanid = ENGID
       order by o.id, O.FINAL_PARA_NO;
  
  end p_get_audit_observtion;

  Procedure p_audit_observation_reversal(ENGID     in number,
                                         obs_id    in number,
                                         S_ID      in number,
                                         P_NO      in number,
                                         io_cursor OUT t_cursor) is
    M_F number := 0;
    S_F varchar2(100);
    O_B number := 0;
  begin
    select obs_id into O_B from dual;
    select max(ob.memo_number)
      into M_F
      from t_au_observation ob
     where ob.id = obs_id;
    select s.statusname
      into S_F
      from t_au_observation_status s
     where s.statusid = S_ID;
    update t_au_observation o set o.status = S_ID where o.id = obs_id;
    commit;
    if (S_ID = 1) then
      delete from t_au_observation_assignedto o where o.obs_id = O_B;
      commit;
      delete from t_au_observations_auditee_response s
       where s.au_obs_id = O_B;
      delete from t_au_observations_auditee_evidences e
       where e.memoid = o_b;
      commit;
      delete from t_au_observations_auditor_recommendation r
       where r.au_obs_id = O_B;
      commit;
    
      UPDATE T_AU_OBSERVATION O
         SET O.MEMO_NUMBER         = NULL,
             O.DRAFT_PARA_NO       = NULL,
             O.DRAFT_PARA_ADDED_ON = NULL,
             O.FINAL_PARA_NO       = NULL,
             O.FINAL_PARA_ADDED_ON = NULL,
             O.REPLYDATE           = NULL,
             O.LASTREPLYBY         = NULL,
             O.LASTREPLYDATE       = NULL,
             o.memo_reply_date     = null
       WHERE O.ID = o_b;
      COMMIT;
      UPDATE T_AU_OBSERVATION_TEXT T
         SET T.MEMO_NUMBER = NULL
       WHERE T.OBSERVATSION_ID = o_b;
      COMMIT;
    
    else
      if (S_ID in (2)) then
        delete from t_au_observations_auditee_response s
         where s.au_obs_id = O_B;
        delete from t_au_observations_auditee_evidences e
         where e.memoid = o_b;
        commit;
        delete from t_au_observations_auditor_recommendation r
         where r.au_obs_id = O_B;
        commit;
        delete from t_au_observations_auditor_reply rp
         where rp.au_obs_id = O_B;
        update t_au_observation_assignedto s
           set s.replied = 'N'
         where s.obs_id = O_B;
        commit;
      
        UPDATE T_AU_OBSERVATION O
           SET O.DRAFT_PARA_NO       = NULL,
               O.DRAFT_PARA_ADDED_ON = NULL,
               O.FINAL_PARA_NO       = NULL,
               O.FINAL_PARA_ADDED_ON = NULL,
               O.REPLYDATE           = NULL,
               O.LASTREPLYBY         = NULL,
               O.LASTREPLYDATE       = NULL,
               o.memo_reply_date     = null
         WHERE O.ID = o_b;
        COMMIT;
      
      else
        if (S_ID in (3)) then
          delete from t_au_observations_auditor_recommendation r
           where r.au_obs_id = O_B;
          commit;
          delete from t_au_observations_auditor_reply rp
           where rp.au_obs_id = O_B;
          commit;
          UPDATE T_AU_OBSERVATION O
             SET O.DRAFT_PARA_NO       = NULL,
                 O.DRAFT_PARA_ADDED_ON = NULL,
                 O.FINAL_PARA_NO       = NULL,
                 O.FINAL_PARA_ADDED_ON = NULL
           WHERE O.ID = o_b;
          COMMIT;
        
          ELSE
            IF (S_ID = 5) then
              delete from t_au_observations_auditor_reply rp
               where rp.au_obs_id = O_B;
              commit;
              UPDATE T_AU_OBSERVATION O
                 SET O.FINAL_PARA_NO       = NULL,
                     O.FINAL_PARA_ADDED_ON = NULL,
                     O.STELLED_ON          = NULL,
                     O.SETTLED_BY          = NULL,
                     o.status              = S_ID
               WHERE O.ID = o_b;
              COMMIT;
            ELSIF (S_ID = 8) THEN
              UPDATE T_AU_OBSERVATION O
                 SET O.STELLED_ON = NULL,
                     O.SETTLED_BY = NULL,
                     O.STATUS     = S_ID
               WHERE O.ID = O_B;
              COMMIT;
            end if;
        end if;
      end if;
    end if;
  
    open io_cursor for
    
      select 'Memo Number ' || M_F || ' Status changed to ' || S_F as remarks
        from dual;
  
  end p_audit_observation_reversal;

  Procedure p_get_audit_observation_number(ENGID     in number,
                                           Memo      in number,
                                           io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select o.id              as id_in_observation_table,
             o.engplanid       as eng_in_observation_table,
             o.memo_number     as memo_in_observation_table,
             t.observatsion_id as osb_id_in_text_table,
             t.eng_plan        as eng_in_text_table,
             t.memo_number     as memo_in_text_table,
             t.id              as id_in_text_table,
             a.obs_id          as obs_id_in_assinged_table,
             a.id              as id_in_assinged_table,
             a.eng_id          as ang_id_in_assigned_table
        from t_au_observation o
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
         and t.eng_plan = o.engplanid
        left join t_au_observation_assignedto a
          on a.obs_id = o.id
         and a.eng_id = o.engplanid
       where o.engplanid = ENGID
         and o.memo_number = memo;
  end p_get_audit_observation_number;

  Procedure p_get_audit_observation_num_obs(ENGID     in number,
                                            Memo      in number,
                                            io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select o.id          as o_obs_id,
             o.engplanid   as o_eng_id,
             o.memo_number as o_memo_number,
             o.enteredby   as o_enter_date
        from t_au_observation o
       where o.engplanid = ENGID
         and o.memo_number = memo;
  end p_get_audit_observation_num_obs;

  Procedure p_get_audit_observation_num_text(ENGID     in number,
                                             Obs_id    in number,
                                             io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select t.id              as t_id,
             t.eng_plan        as t_eng_id,
             t.observatsion_id as t_osb_id,
             t.memo_number     as t_memo_number,
             t.entereddate     as t_enter_date
        from t_au_observation_text t
       where t.eng_plan = ENGID
         and t.observatsion_id = obs_id;
  end p_get_audit_observation_num_text;

  Procedure p_get_audit_observation_num_assigned(ENGID     in number,
                                                 Obsid     in number,
                                                 io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select a.id            as t_id,
             a.eng_id        as t_eng_id,
             a.obs_id        as t_osb_id,
             a.assigned_date as a_enter_date
        from t_au_observation_assignedto a
       where a.eng_id = ENGID
         and a.obs_id = obsid;
  end p_get_audit_observation_num_assigned;

  Procedure p_audit_observation_assingment(ENGID  in number,
                                           Memo   in number,
                                           ent_id in number) is
  begin
    update t_au_observation_assignedto o
       set o.entity_id = ent_id
     where o.obs_id in (Memo);
    commit;
  end p_audit_observation_assingment;

  Procedure p_audit_observation_reversal_closing(ENGID in number) is
  begin
    update t_au_audit_joining ji
       set ji.status = 'I'
     where ji.eng_plan_id in (ENGID);
    commit;
  
    update t_au_audit_team_tasklist t
       set t.isactive = 'Y', t.status_id = '2'
    
     where t.eng_plan_id in (ENGID);
    commit;
  
    update t_au_plan_eng e set e.status = 4 where e.eng_id in (ENGID);
    commit;
  end p_audit_observation_reversal_closing;

  procedure P_get_auditee_entities(Ent_id    in number,
                                   t_id      in number,
                                   io_cursor OUT t_cursor) is
  
  begin
    if (Ent_id = 0 and T_id != 0) then
      open io_cursor for
        select --e.cbas_entity_id,
         e.code,
         e.name,
         e.active,
         e.type_id,
         t.entitytypedesc as type_name,
         e.auditby_id,
         ee.name          as auditby_name,
         --e.cost_center,
         e.entity_id,
         e.auditable
          from t_auditee_entities e
         inner join t_auditee_ent_types t
            on t.autid = e.type_id
         inner join t_auditee_entities ee
            on ee.entity_id = e.auditby_id
         where e.type_id = t_id
         order by e.entity_id;
    else
      if (Ent_id != 0) then
        open io_cursor for
          select --e.cbas_entity_id,
           e.code,
           e.name,
           e.active,
           e.type_id,
           t.entitytypedesc as type_name,
           e.auditby_id,
           ee.name          as auditby_name,
           --e.cost_center,
           e.entity_id,
           e.auditable
            from t_auditee_entities e
           inner join t_auditee_ent_types t
              on t.autid = e.type_id
           inner join t_auditee_entities ee
              on ee.entity_id = e.auditby_id
           where e.entity_id = Ent_id
           order by e.type_id, e.entity_id;
      else
        open io_cursor for
          select --e.cbas_entity_id,
           e.code,
           e.name,
           e.active,
           e.type_id,
           t.entitytypedesc as type_name,
           e.auditby_id,
           ee.name          as auditby_name,
           --e.cost_center,
           e.entity_id,
           e.auditable
            from t_auditee_entities e
           inner join t_auditee_ent_types t
              on t.autid = e.type_id
           inner join t_auditee_entities ee
              on ee.entity_id = e.auditby_id
           order by e.type_id, e.entity_id;
      end if;
    end if;
  end P_get_auditee_entities;

  procedure P_add_auditee_entities(cbas_code        in number,
                                   e_code           in number,
                                   e_name           in varchar2,
                                   status           in varchar2,
                                   t_id             in number,
                                   audited_by_id    in number,
                                   cost_center_code in number,
                                   auditable_status in varchar2,
                                   io_cursor        OUT t_cursor) is
  
  begin
    insert into t_auditee_entities
      (code,
       description,
       name,
       active,
       type_id,
       auditby_id,
       inspectedby_id,
       entity_id,
       auditable)
    values
      (e_code,
       e_name,
       e_name,
       status,
       t_id,
       audited_by_id,
       0,
       (SELECT COALESCE(max(le.entity_id) + 1, 1) FROM t_auditee_entities le),
       auditable_status);
    commit;
  end P_add_auditee_entities;

  Procedure P_get_error_logs(accured_on date, io_cursor OUT t_cursor) is
  begin
    if (accured_on is null or accured_on = '0') then
      open io_cursor for
        select *
          from t_au_error_logs r
         order by r.record_on, r.package_name;
    else
      open io_cursor for
        select *
          from t_au_error_logs r
         where r.record_on = accured_on
         order by r.record_on, r.package_name;
    end if;
  end P_get_error_logs;

  procedure P_GetRiskProcessTransactions(procDetailId  IN NUMBER,
                                         transactionId IN NUMBER,
                                         io_cursor     OUT t_cursor) as
  
  begin
  
    if (procDetailId = 0) THEN
      IF (transactionId = 0 or transactionId is null) THEN
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
           inner join t_auditee_entities d
              on pt.process_owner_id = d.entity_id
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
           inner join t_auditee_entities d
              on pt.process_owner_id = d.entity_id
           WHERE pt.ID = transactionId
           order by pt.id asc;
      end if;
    else
      IF (transactionId = 0 or transactionId is null) THEN
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
           inner join t_auditee_entities d
              on pt.process_owner_id = d.entity_id
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
           inner join t_auditee_entities d
              on pt.process_owner_id = d.entity_id
           where pt.ID = transactionId
             and pt.s_id = procDetailId
           order by pt.Id asc;
      END IF;
    end if;
  end P_GetRiskProcessTransactions;

  procedure P_GetAuditeeEntityTypes(ENTITYID  IN NUMBER,
                                    io_cursor OUT t_cursor) as
  
  begin
    if (ENTITYID is null) then
      OPEN io_cursor FOR
        Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
          FROM t_auditee_ent_types G
         order by G.ENTITYTYPEDESC;
    else
      OPEN io_cursor FOR
        Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
          FROM t_auditee_ent_types G;
      --Where g.audited_by_enitity = ENTITYID;
    
    end if;
  end P_GetAuditeeEntityTypes;

  procedure P_GetAuditeeTypes(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
        FROM t_auditee_ent_types G
      
       order by G.ENTITYTYPEDESC;
  
  end P_GetAuditeeTypes;

  procedure P_get_auditee_entities_mapping(Ent_id    in number,
                                           t_id      in number,
                                           io_cursor OUT t_cursor) is
  
  begin
    if (Ent_id = 0 and T_id != 0) then
      open io_cursor for
        select e.parent_id,
               e.parent_code,
               e.child_code,
               e.entity_id,
               e.auditedby,
               e.status,
               e.p_name,
               e.c_name,
               e.p_type_id,
               e.c_type_id,
               e.relation_type_id
          from v_get_parent_office e
         where e.c_type_id = t_id
         order by e.entity_id;
    else
      if (Ent_id != 0) then
        open io_cursor for
          select e.parent_id,
                 e.parent_code,
                 e.child_code,
                 e.entity_id,
                 e.auditedby,
                 e.status,
                 e.p_name,
                 e.c_name,
                 e.p_type_id,
                 e.c_type_id,
                 e.relation_type_id
            from v_get_parent_office e
           where e.entity_id = Ent_id
           order by e.c_type_id, e.entity_id;
      else
        open io_cursor for
          select *
            from v_get_parent_office e
           order by e.c_type_id, e.entity_id;
      end if;
    end if;
  end P_get_auditee_entities_mapping;

  Procedure P_ADD_ENTITIES_MAPPING(P_ENT_ID    IN NUMBER,
                                   ENT_ID      in number,
                                   RELATION_ID in number,
                                   io_cursor   OUT t_cursor) as
    v_f  number := 0;
    C_F  number := 0;
    EC_F number := 0;
    N_F  varchar2(200);
    CN_F varchar2(200);
    T_F  number := 0;
    CT_F number := 0;
    A_F  number := 0;
  begin
    select e.code
      into C_F
      from t_auditee_entities e
     where e.entity_id = P_ENT_ID;
  
    select e.code
      into EC_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
  
    select e.name
      into N_F
      from t_auditee_entities e
     where e.entity_id = P_ENT_ID;
  
    select e.name
      into CN_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
  
    select e.type_id
      into T_F
      from t_auditee_entities e
     where e.entity_id = P_ENT_ID;
  
    select e.auditby_id
      into A_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
  
    select e.type_id
      into CT_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
  
    select NVL(max(m.entity_id), 0)
      into V_F
      from T_AUDITEE_ENTITIES_MAPING m
     where m.entity_id = ENT_ID;
    if (V_F = 0) then
      Insert into T_AUDITEE_ENTITIES_MAPING
        (PARENT_ID,
         PARENT_CODE,
         CHILD_CODE,
         ENTITY_ID,
         AUDITEDBY,
         STATUS,
         P_NAME,
         C_NAME,
         P_TYPE_ID,
         C_TYPE_ID,
         RELATION_TYPE_ID)
      values
        (P_ENT_ID,
         C_F,
         EC_F,
         ENT_ID,
         A_F,
         'Y',
         N_F,
         CN_F,
         T_F,
         CT_F,
         RELATION_ID);
      COMMIT;
      Open io_cursor for
        select CN_F || ' Maping added' as remarks from dual;
    else
      open io_cursor for
        select 'Relationship Already Exist' as remarks from dual;
    end if;
  end P_ADD_ENTITIES_MAPPING;

  Procedure P_UPDATE_ENTITIES_MAPPING(P_ENT_ID    IN NUMBER,
                                      RELATION_ID in number,
                                      ENT_ID      in number,
                                      io_cursor   OUT t_cursor) as
    C_F number := 0;
    N_F varchar2(200);
    T_F number := 0;
  begin
    select e.code
      into C_F
      from t_auditee_entities e
     where e.entity_id = P_ENT_ID;
    select e.name
      into N_F
      from t_auditee_entities e
     where e.entity_id = P_ENT_ID;
  
    select e.type_id
      into T_F
      from t_auditee_entities e
     where e.entity_id = P_ENT_ID;
  
    UPDATE T_AUDITEE_ENTITIES_MAPING M
       SET M.PARENT_ID        = P_ENT_ID,
           M.PARENT_CODE      = C_F,
           M.STATUS           = 'Y',
           M.P_NAME           = N_F,
           M.P_TYPE_ID        = T_F,
           M.RELATION_TYPE_ID = RELATION_ID
     where m.entity_id = ENT_ID;
    COMMIT;
  
    Open io_cursor for
      Select ' Mapping Updated in ' || N_F as remarks from dual;
  
  end P_UPDATE_ENTITIES_MAPPING;

  Procedure P_GET_HR_ENTITIES(ENT_CODE  in number,
                              ENT_NAME  in varchar2,
                              io_cursor OUT t_cursor) is
  begin
    if (ENT_Name is not null) then
      open io_cursor for
        Select e.Rept_code,
               e.Rept_name,
               e.Rept_status,
               e.Entity_code,
               e.Entity_name,
               e.Entity_status,
               e.rept_ind,
               e.ind
          from v_get_HR_entities e
         where Upper(e.Entity_name) like '%' || upper(ENT_NAME) || '%'
           and e.Entity_status = 'A'
           and e.Rept_status = 'A';
    else
      if (ENT_CODE != 0) then
        open io_cursor for
          Select e.Rept_code,
                 e.Rept_name,
                 e.Rept_status,
                 e.Entity_code,
                 e.Entity_name,
                 e.Entity_status,
                 e.rept_ind,
                 e.ind
            from v_get_HR_entities e
           where e.Entity_code = ent_code
             and e.Entity_status = 'A'
             and e.Rept_status = 'A';
      end if;
    end if;
  
  end P_GET_HR_ENTITIES;

  Procedure P_GET_AIS_ENTITIES(ENT_CODE  in number,
                               ENT_NAME  in varchar2,
                               ENT_TYPE  in number,
                               io_cursor OUT t_cursor) is
  begin
  
    if (ENT_Name is not null) then
      open io_cursor for
        Select e.entity_id,
               e.code,
               e.description,
               e.name,
               e.type_id,
               e.auditby_id,
               et.name as audit_by,
               e.active,
               e.auditable
          from t_Auditee_Entities e
          left join t_auditee_entities et
            on et.entity_id = e.auditby_id
         where Upper(e.name) like '%' || upper(ENT_NAME) || '%';
    else
      if (ENT_CODE is not null) then
        open io_cursor for
          Select e.entity_id,
                 e.code,
                 e.description,
                 e.name,
                 e.type_id,
                 e.auditby_id,
                 et.name as audit_by,
                 e.active,
                 e.auditable
            from t_Auditee_Entities e
            left join t_auditee_entities et
              on et.entity_id = e.auditby_id
           where e.code = ENT_CODE;
      else
        open io_cursor for
          Select e.entity_id,
                 e.code,
                 e.description,
                 e.name,
                 e.type_id,
                 e.auditby_id,
                 et.name as audit_by,
                 e.active,
                 e.auditable
            from t_Auditee_Entities e
            left join t_auditee_entities et
              on et.entity_id = e.auditby_id
           where e.type_id = ENT_TYPE
             and e.active = 'Y';
      end if;
    end if;
  end P_GET_AIS_ENTITIES;

  Procedure P_GET_ERP_ENTITIES(ENT_CODE  in number,
                               ENT_NAME  in varchar2,
                               io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select '' as rem from dual;
    /*      select e.ORG_ID, e.ORG_DESC, e.ORG_TYPE, e.LOCATION_ID
     from v_erp_departments e
    where e.ORG_ID = ENT_CODE
       or upper(e.ORG_DESC) = upper(ENT_NAME);*/
  
  end P_GET_ERP_ENTITIES;

  Procedure P_GET_CBAS_ENTITIES(ENT_CODE  in number,
                                ENT_NAME  in varchar2,
                                io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select cast(e.org_unitid as number) as org_unitid,
             e.name,
             e.code,
             e.org_unit_typeid,
             e.active,
             e.address,
             e.phoneno,
             e.isonline
        from V_GET_CBAS_ENTITIES e
       where e.code = cast(ENT_CODE as varchar2(20))
          or upper(e.name) = upper(ENT_NAME);
  
  end P_GET_CBAS_ENTITIES;

  Procedure P_GET_ENTITIES_MAPPING_CODE(ENT_CODE  in number,
                                        io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select e.id,
             e.ais_id,
             e.cbas_id,
             e.hrms_id,
             e.erp_id,
             e.hyp_id,
             e.cdms_id,
             e.cpms_id
        from t_auditee_entities_code e
       where e.ais_id = ENT_CODE;
  
  end P_GET_ENTITIES_MAPPING_CODE;

  Procedure P_ADD_ENTITIES_MAPPING_CODE(ENT_CODE  in number,
                                        CBAS      IN NUMBER,
                                        HRMS      IN NUMBER,
                                        ERP       IN NUMBER,
                                        HYPER     IN NUMBER,
                                        CDMS      IN NUMBER,
                                        CPMS      IN NUMBER,
                                        io_cursor OUT t_cursor) is
  begin
    INSERT INTO t_auditee_entities_code
      (id, ais_id, cbas_id, hrms_id, erp_id, hyp_id, cdms_id, cpms_id)
    VALUES
      ((SELECT COALESCE(max(C.ID) + 1, 1) FROM t_auditee_entities_code C),
       ENT_CODE,
       CBAS,
       HRMS,
       ERP,
       HYPER,
       CDMS,
       CPMS);
    COMMIT;
  
  end P_ADD_ENTITIES_MAPPING_CODE;

  Procedure P_UPDATE_ENTITIES_MAPPING_CODE(ENT_CODE  in number,
                                           CBAS      IN NUMBER,
                                           HRMS      IN NUMBER,
                                           ERP       IN NUMBER,
                                           HYPER     IN NUMBER,
                                           CDMS      IN NUMBER,
                                           CPMS      IN NUMBER,
                                           io_cursor OUT t_cursor) is
  begin
    update t_auditee_entities_code c
       set c.cbas_id = CBAS,
           c.hrms_id = HRMS,
           c.erp_id  = ERP,
           c.hyp_id  = HYPER,
           c.cdms_id = CDMS,
           c.cpms_id = CPMS
     where c.ais_id = ENT_CODE;
    COMMIT;
  
  end P_UPDATE_ENTITIES_MAPPING_CODE;

  procedure p_get_auditee_engagement(ent_id    in number,
                                     period    in number,
                                     io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select eng.ENG_ID,
             e.name || ' (' || eng.AUDIT_STARTDATE || ' to ' ||
             eng.AUDIT_ENDDATE || ' )' as Eng_name
        from t_au_plan_eng eng
       inner join t_auditee_entities e
          on e.entity_id = eng.entity_id
       where eng.entity_id = ent_id
         and eng.period_id = period;
  
  end p_get_auditee_engagement;

  procedure P_GetAuditeeRisk(ENT_ID IN NUMBER, io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select e.risk_areas, e.max_number, e.Marks
        FROM t_au_entities_group_risk e
       where e.eng_id = ENT_ID
       order by e.gr_id;
  
  end P_GetAuditeeRisk;

  procedure P_GetAuditeeRisk_details(ENT_ID    IN NUMBER,
                                     io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select ed.risk_areas,
             nvl(ed.max_number, 0) as max_number,
             nvl(ed.weightage_average, 0) as weightage_average,
             nvl(ed.gravity_risk, 0) as gravity_risk,
             nvl(ed.number_of_observations, 0) as number_of_observations,
             nvl(ed.risk_based_marks, 0) as risk_based_marks,
             nvl(ed.weighted_average_marks, 0) as weighted_average_marks
        FROM t_au_entities_group_risk_details ed
       where ed.eng_id = ENT_ID
       order by ed.s_gr_id;
  
  end P_GetAuditeeRisk_details;

  procedure P_Get_Entity_Risk(ENT_TYP   IN NUMBER,
                              period    in number,
                              io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select mp.p_name as parent_office,
             e.name,
             e.branch_code,
             e.risk_rating,
             e.risk_category
        FROM t_au_entities_period_risk e
       inner join v_get_parent_office mp
          on mp.entity_id = e.entity_id
       where e.type_id = ENT_TYP
         and e.audit_period_id = period
       order by e.risk_rating desc, mp.parent_id asc;
  
  end P_Get_Entity_Risk;

  procedure p_Get_sub_Checklist_MERGER_FOR_REVIEW(SID       in number,
                                                  ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select a.s_id    as sid,
             a.heading as sub_process,
             s.s_id    as m_sid,
             s.heading as for_merger
        FROM t_audit_checklist_sub S
       inner join T_AUDIT_CHECKLIST_Sub_merger sb
          on sb.m_sid = s.s_id
       inner join t_audit_checklist_sub a
          on a.s_id = sb.s_id
       where sb.STATUS = 'P'
         and sb.s_id = sid;
  
  end p_Get_sub_Checklist_MERGER_FOR_REVIEW;

  procedure p_Get_Checklist_MERGER_FOR_REVIEW(CID       in number,
                                              ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select a.t_id    as cid,
             a.heading as main_process,
             s.t_id    as m_cid,
             s.heading as for_merger
        FROM t_audit_checklist S
       inner join T_AUDIT_CHECKLIST_merger sb
          on sb.m_cid = s.t_id
       inner join t_audit_checklist a
          on a.t_id = sb.c_id
       where sb.STATUS = 'P'
         and a.t_id = CID;
  
  end p_Get_Checklist_MERGER_FOR_REVIEW;

  procedure p_Get_ChecklistDetail_FOR_DUPLICATE(subProcessId in number,
                                                ENT_ID       in number,
                                                P_NO         in number,
                                                R_ID         in number,
                                                io_cursor    OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.id,
             t.s_id,
             t.heading,
             t.v_id,
             t.risk_id,
             t.role_resp_id,
             t.process_owner_id,
             t.status,
             t.owner_enitity_id as owner_entity_id,
             t.annex,
             p.heading          as T_NAME
        from t_audit_checklist_details t
       inner join t_audit_checklist_sub p
          on p.s_id = t.s_id
       inner join t_audit_checklist_details_change ch
          on ch.id = t.id
       where t.STATUS = 'Y'
         and ch.s_id = subProcessId;
  
  end p_Get_ChecklistDetail_FOR_DUPLICATE;

  Procedure P_UPDATE_CHECKLIST_DETAILS(C_ID       IN NUMBER,
                                       SID        in number,
                                       check_list in varchar2,
                                       io_cursor  OUT t_cursor) as
  
  begin
    update t_audit_checklist_details_change c
       set c.n_d_id    = C_ID,
           c.n_s_id    = SID,
           c.n_heading = check_list,
           c.status    = 'Z'
     where c.id = C_ID;
    commit;
    open io_cursor for
      select 'Checklist Updated' as remarks from dual;
  
  end P_UPDATE_CHECKLIST_DETAILS;

  Procedure P_REMOVE_DUPLICATE_CHECKLIST_DETAILS(C_ID IN NUMBER,
                                                 D_ID in number) as
  
  begin
    Insert into t_audit_checklist_detail_duplicate
      (checklist, checklist_replacement, status)
    values
      (C_ID, D_ID, 'P');
    commit;
  
  end P_REMOVE_DUPLICATE_CHECKLIST_DETAILS;

  Procedure p_merge_sub_checklist(sid       in number,
                                  msid      in number,
                                  io_cursor OUT t_cursor) as
    R_F number := 0;
    N_F varchar2(500);
  begin
    select nvl(s.heading, 'Y')
      into N_F
      from t_audit_checklist_sub s
     where s.s_id = msid;
    select nvl(max(r.s_id), 0)
      into R_F
      from T_AUDIT_CHECKLIST_Sub_merger r
     where r.m_sid = msid
       and r.status = 'P';
    if (R_F = 0) then
      Insert into T_AUDIT_CHECKLIST_Sub_merger
        (s_Id, m_Sid, Status)
      
      values
        (SID, MSID, 'P');
      commit;
      open io_cursor for
        select 'Request submitted for Authorization' as remarks from dual;
    else
      open io_cursor for
        select 'Request for merger of ' || N_F || '  already submitted' as remarks
          from dual;
    end if;
  
  end p_merge_sub_checklist;

  Procedure p_merge_checklist(cid       in number,
                              mcid      in number,
                              io_cursor OUT t_cursor) as
    R_F number := 0;
    N_F varchar2(500);
  begin
    select nvl(s.heading, 'Y')
      into N_F
      from t_audit_checklist s
     where s.t_id = mcid;
    select nvl(max(r.c_id), 0)
      into R_F
      from T_AUDIT_CHECKLIST_merger r
     where r.m_cid = mcid
       and r.status = 'P';
    if (R_F = 0) then
      Insert into T_AUDIT_CHECKLIST_merger
        (c_Id, m_Cid, Status)
      
      values
        (CID, MCID, 'P');
      commit;
      open io_cursor for
        select 'Request submitted for Authorization' as remarks from dual;
    else
      open io_cursor for
        select 'Request for merger of ' || N_F || '  already submitted' as remarks
          from dual;
    end if;
  
  end p_merge_checklist;

  Procedure P_GET_DUPLICATE_CHECKLIST_DETAILS_DROPDOWN(io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select distinct d.id, d.heading as Main_checklist
        from t_audit_checklist_detail_duplicate c
       inner join t_audit_checklist_details d
          on d.id = c.checklist
       where c.status = 'P';
  
  end P_GET_DUPLICATE_CHECKLIST_DETAILS_DROPDOWN;

  Procedure P_GET_DUPLICATE_CHECKLIST_DETAILS(D_ID      in number,
                                              io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select c.checklist_replacement as C_ID, d.heading
        from t_audit_checklist_detail_duplicate c
       inner join t_audit_checklist_details d
          on d.id = c.checklist_replacement
       where c.status = 'P'
         and c.checklist = D_ID;
  
  end P_GET_DUPLICATE_CHECKLIST_DETAILS;

  Procedure P_GET_DUPLICATE_CHECKLIST_DETAILS_COUNT(D_ID      in number,
                                                    io_cursor OUT t_cursor) as
    O_F number;
    N_F number;
  begin
    select count(f.id)
      into O_F
      from t_au_old_paras_fad f
     inner join t_audit_checklist_detail_duplicate d
        on f.process_detail = d.checklist_replacement
     where d.checklist = D_ID;
    select count(o.id)
      into N_F
      from t_au_observation o
     inner join t_audit_checklist_detail_duplicate d
        on o.checklistdetail_id = d.checklist_replacement
     where d.checklist = D_ID;
  
    open io_cursor for
      select O_F as old, N_F as new from dual;
  
  end P_GET_DUPLICATE_CHECKLIST_DETAILS_COUNT;

  Procedure P_AUTHORIZE_MERGER_CHECKLIST(C_ID      in number,
                                         M_CID     IN NUMBER,
                                         io_cursor OUT t_cursor) as
  
  begin
    UPDATE T_AUDIT_CHECKLIST_SUB S SET S.T_ID = C_ID where S.T_ID = M_CID;
    COMMIT;
  
    delete t_audit_checklist c where c.t_id = m_cid;
    commit;
  
    OPEN io_cursor FOR
      SELECT 'Checklist mereged' as remarks FROM DUAL;
  
  END P_AUTHORIZE_MERGER_CHECKLIST;

  Procedure P_AUTHORIZE_MERGER_CHECKLIST_SUB(SID       in number,
                                             M_SID     IN NUMBER,
                                             io_cursor OUT t_cursor) as
    SD number;
  begin
    select M_SID into SD from dual;
    UPDATE T_AUDIT_CHECKLIST_DETAILS D
       SET D.S_ID = SID
     where D.S_ID = M_SID;
    COMMIT;
    UPDATE T_AUDIT_CHECKLIST_DETAILS_CHANGE CD
       SET CD.S_ID = SID, CD.N_S_ID = SID
     where CD.S_ID = M_SID;
    COMMIT;
  
    update t_audit_checklist_sub_merger m
       set m.status = 'R'
     where m.m_sid = SD;
    commit;
  
    delete t_audit_checklist_sub s where s.s_id = m_sid;
    commit;
  
    delete t_audit_checklist_sub_change ns where ns.s_id = m_sid;
    commit;
  
    OPEN io_cursor FOR
      SELECT M_SID || ' Sub Checklist mereged into ' || SID as remarks
        FROM DUAL;
  
  END P_AUTHORIZE_MERGER_CHECKLIST_SUB;

  Procedure P_AUTHORIZE_DUPLICATE_CHECKLIST_DETAILS(D_ID      in number,
                                                    io_cursor OUT t_cursor) as
  
  begin
  
    update t_au_old_paras_fad o
       set o.process_detail =
           (select t.checklist
              from t_audit_checklist_detail_duplicate t
             where t.checklist_replacement = o.process_detail
               and t.checklist = D_ID)
     where exists (select 'z'
              from t_audit_checklist_detail_duplicate t
             where t.checklist_replacement = o.process_detail);
    commit;
  
    update t_au_observation ob
       set ob.checklistdetail_id =
           (select t.checklist
              from t_audit_checklist_detail_duplicate t
             where t.checklist_replacement = ob.checklistdetail_id
               and t.checklist = D_ID)
     where exists
     (select 'z'
              from t_audit_checklist_detail_duplicate t
             where t.checklist_replacement = ob.checklistdetail_id);
    commit;
  
    update t_audit_checklist_details d
       set d.status = 'N'
     where exists (select 'z'
              from t_audit_checklist_detail_duplicate t
             where t.checklist_replacement = d.id);
    commit;
  
    update t_audit_checklist_detail_duplicate dt
       set dt.status = 'R'
     where dt.checklist = D_ID;
    commit;
  
    open io_cursor for
      select 'Checklist Updated' as remarks from dual;
  
  end P_AUTHORIZE_DUPLICATE_CHECKLIST_DETAILS;

  Procedure P_Del_User_Data_in_temp_table(io_cursor OUT t_cursor) as
  
    U_F number;
  begin
    select count(*) into U_F from TEMP_PPNO;
    DELETE from TEMP_PPNO COMMIT;
  
    open io_cursor for
    
      select U_F || '  No of records deleted' as no_of_records from dual;
  
  end P_Del_User_Data_in_temp_table;

  Procedure P_get_user_role_type(D_CODE in number, io_cursor OUT t_cursor) as
  
    cursor V is
      Select NVL(e.designationcode, 0) as designationcode,
             e.description,
             e.group_id
        from T_GROUP_RIGHTS e
       where e.designationcode = D_CODE
         AND E.GROUP_ID IS NOT NULL;
  
    vr V%rowtype;
  begin
    Open V;
    Fetch V
      into vr;
    Close v;
    if (VR.DESIGNATIONCODE != 0) then
      open io_cursor for
        select g.role_id, g.group_id, g.group_name
          from t_groups g
         where g.group_id = VR.GROUP_ID;
    else
      open io_cursor for
        select g.role_id, g.group_id, g.group_name from t_groups g;
    end if;
  end P_get_user_role_type;

  procedure p_update_role_hr(D_Code    in number,
                             g_id      in number,
                             io_cursor OUT t_cursor) is
  begin
    update T_GROUP_RIGHTS t
       set t.group_id = g_id
     where t.designationcode = d_code;
    commit;
    open io_cursor for
      select D_code || ' Updated  in ' || G_id as remarks from dual;
  
  end p_update_role_hr;

  Procedure P_get_new_user(io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select t.ppno,
             e.employeefirstname || ' ' || e.employeelastname as e_name,
             t.designationcode,
             t.designation,
             e.employeetype,
             t.Posting_Type,
             t.CODE,
             t.NAME,
             t.entity_id
        from v_AIS_new_user t
       inner join v_service_employeeinfo e
          on e.ppno = t.ppno
       where not exists
       (select 'z' from t_audit_emp ap where T.ppno = ap.ppno)
         AND E.PPNO NOT IN (116681, 111751, 132704);
  end P_get_new_user;

  Procedure P_UPDATE_NEW_USER(P_NO in number, io_cursor OUT t_cursor) as
  
    cursor V is
      select (SELECT COALESCE(max(ll.userid) + 1, 1) FROM t_user ll) as userid,
             v.ppno,
             'b4b55fe71e7c7c181f7074ff8f5c5ef3' as pass,
             v.employeetype,
             'Y',
             v.CURRENTZONECODE,
             v.CURRENTBRANCHCODE,
             v.CURRENTDIVISIONCODE,
             v.CURRENTDEPARTMENTCODE,
             v.entity_id,
             v.designationcode
        from v_AIS_new_user_for_addition v
       where v.ppno = P_NO;
    vr1 V%rowtype;
    U_F number := 0;
  begin
    Open V;
    Fetch V
      into vr1;
    Close v;
    select nvl(max(q.userid), 0)
      into U_F
      from t_user q
     where q.ppno = P_NO;
    if (U_F > 0) then
      update t_user u
         set u.user_location_type = vr1.employeetype,
             u.isactive           = 'Y',
             u.password           = vr1.pass,
             u.zoneid             = vr1.currentzonecode,
             u.branchid           = vr1.currentbranchcode,
             u.divisionid         = vr1.currentdivisioncode,
             u.departmentid       = vr1.currentdepartmentcode,
             u.entity_id          = vr1.entity_id,
             u.designation        = vr1.designationcode
       where u.ppno = vr1.ppno;
      commit;
    else
      DELETE FROM T_USER U WHERE U.PPNO = P_NO;
      COMMIT;
    
      insert into T_USER
        (userid,
         login_name,
         password,
         ppno,
         user_location_type,
         isactive,
         zoneid,
         branchid,
         divisionid,
         departmentid,
         ENTITY_ID,
         DESIGNATION)
        select (SELECT COALESCE(max(ll.userid) + 1, 1) FROM t_user ll),
               v.ppno,
               'b4b55fe71e7c7c181f7074ff8f5c5ef3',
               v.ppno,
               v.employeetype,
               'Y',
               v.CURRENTZONECODE,
               v.CURRENTBRANCHCODE,
               v.CURRENTDIVISIONCODE,
               v.CURRENTDEPARTMENTCODE,
               v.entity_id,
               v.designationcode
          from v_AIS_new_user_for_addition v
         where v.ppno = P_NO;
      commit;
    end if;
    DELETE FROM T_USER_MAPING U WHERE U.PPNO = P_NO;
    COMMIT;
  
    INSERT INTO T_USER_MAPING
      (USERID, PPNO, GROUP_ID, ROLE_ID)
    
      SELECT p.USERID, P_NO, g.group_id, g.group_id
        from T_GROUP_RIGHTS g
       inner join t_user p
          on p.designation = g.designationcode
         and p.ppno = P_NO;
    commit;
  
    open io_cursor for
      Select 'User updated' as remarks from dual;
  
  end P_UPDATE_NEW_USER;

  PROCEDURE P_Get_Details_For_Entity_Shifting(ENT_ID  IN NUMBER,
                                              io_cursor OUT t_cursor) AS
  BEGIN
    OPEN Io_Cursor FOR
      SELECT E.Entity_Id,
             E.Code         AS Entity_Code,
             E.Type_Id,
             Et.Audit_Type,
             E.Name,
             Sd.Description AS E_Size,
             Rd.Description AS Risk,
             
             Ep.Eng_Id,
             Ep.Audit_Startdate AS Start_Date,
             Ep.Audit_Enddate   AS End_Date,
             
             NVL(Pc.Total_Para, 0) AS Total_Para,
             NVL(Pc.Legacy_Para, 0) AS LEGACY_PARA,
             NVL(Pc.Legacy_Open, 0) AS Legacy_Open,
             NVL(Pc.Legacy_Close, 0) AS Legacy_Close,
             NVL(Pc.Ais_Para, 0) AS Ais_Para,
             NVL(Pc.Ais_Open, 0) AS Ais_Open,
             NVL(Pc.Ais_Close, 0) AS Ais_Close,
             
             NVL(H.Comp_Sub, 0) AS Comp_Sub
      
        FROM T_Auditee_Entities E
      
        JOIN T_Auditee_Ent_Types Et
          ON Et.Autid = E.Type_Id
      
        LEFT JOIN T_Auditee_Entities_Size_Disc Sd
          ON Sd.Entity_Size = E.Size_Id
      
        LEFT JOIN T_Risk Rd
          ON Rd.R_Id = E.Risk_Id
      
      ------------------------------------------------------------
      -- Latest engagement of the entity
      ------------------------------------------------------------
        LEFT JOIN (SELECT Entity_Id, Eng_Id, Audit_Startdate, Audit_Enddate
                     FROM (SELECT P.Entity_Id,
                                  P.Eng_Id,
                                  P.Audit_Startdate,
                                  P.Audit_Enddate,
                                  ROW_NUMBER() OVER (PARTITION BY P.Entity_Id ORDER BY P.Eng_Id DESC) AS Rn
                             FROM T_Au_Plan_Eng P)
                    WHERE Rn = 1) Ep
          ON Ep.Entity_Id = E.Entity_Id
      
      ------------------------------------------------------------
      -- Compliance paragraph counts
      ------------------------------------------------------------
        LEFT JOIN (SELECT C.Entity_Id,
                          COUNT(C.Com_Id) AS Total_Para,
                          
                          SUM(CASE
                                WHEN C.New_Para_Id IS NULL THEN
                                 1
                                ELSE
                                 0
                              END) AS Legacy_Para,
                          
                          SUM(CASE
                                WHEN C.Para_Status = 8 AND C.New_Para_Id IS NULL THEN
                                 1
                                ELSE
                                 0
                              END) AS Legacy_Open,
                          
                          SUM(CASE
                                WHEN NVL(C.Para_Status, 0) <> 8 AND
                                     C.New_Para_Id IS NULL THEN
                                 1
                                ELSE
                                 0
                              END) AS Legacy_Close,
                          
                          SUM(CASE
                                WHEN C.New_Para_Id IS NOT NULL THEN
                                 1
                                ELSE
                                 0
                              END) AS Ais_Para,
                          
                          SUM(CASE
                                WHEN C.Para_Status = 8 AND
                                     C.New_Para_Id IS NOT NULL THEN
                                 1
                                ELSE
                                 0
                              END) AS Ais_Open,
                          
                          SUM(CASE
                                WHEN NVL(C.Para_Status, 0) <> 8 AND
                                     C.New_Para_Id IS NOT NULL THEN
                                 1
                                ELSE
                                 0
                              END) AS Ais_Close
                   
                     FROM Ais_T_Au_Post_Compliance C
                    GROUP BY C.Entity_Id) Pc
          ON Pc.Entity_Id = E.Entity_Id
      
      ------------------------------------------------------------
      -- Settlement/compliance submissions count
      ------------------------------------------------------------
        LEFT JOIN (SELECT H.Entity_Id, COUNT(H.Id) AS Comp_Sub
                     FROM T_Au_Post_Compliance_Settlemetment_History H
                    GROUP BY H.Entity_Id) H
          ON H.Entity_Id = E.Entity_Id
      
       WHERE E.Entity_Id = ENT_ID;
  END P_Get_Details_For_Entity_Shifting;

  procedure P_Get_Entities_types(io_cursor OUT t_cursor) as
  begin
  
    open io_cursor for
      select a.autid,
             a.entitycode,
             a.entitytypedesc,
             a.auditable,
             a.auditedby,
             a.audited_by_enitity,
             a.audit_type
        from T_AUDITEE_ENT_TYPES a;
  end P_Get_Entities_types;

  procedure P_update_Entities_types(aut_id         in number,
                                    e_code         in number,
                                    e_desc         in varchar2,
                                    e_auditable    in varchar2,
                                    e_auditby_code in number,
                                    e_auditby_id   in number,
                                    e_type         in varchar2,
                                    e_autid        in number,
                                    io_cursor      OUT t_cursor) as
  begin
    update T_AUDITEE_ENT_TYPES e
       set e.autid              = aut_id,
           e.entitycode         = e_code,
           e.entitytypedesc     = e_desc,
           e.auditable          = e_auditable,
           e.auditedby          = e_auditby_code,
           e.audited_by_enitity = e_auditby_id,
           e.audit_type         = e_type
     where e.autid = e_autid;
    commit;
    OPEN io_cursor FOR
      SELECT 'Updated' as remarks from dual;
  
  end P_update_Entities_types;

  procedure P_Get_Entities_Relationship(R_ID      in number,
                                        io_cursor OUT t_cursor) as
  begin
    if (R_ID = 16) then
      open io_cursor for
        select a.entity_realtion_id,
               a.parent_entity_typeid,
               a.child_entity_typeid,
               a.status,
               a.parent_name,
               a.chlid_name,
               a.id
          from T_AUDITEE_ENT_RELATION a
         where exists (select 'z'
                  from t_auditee_entities e
                 where e.type_id = a.child_entity_typeid
                   and e.complice_by is not null);
    else
      open io_cursor for
        select a.entity_realtion_id,
               a.parent_entity_typeid,
               a.child_entity_typeid,
               a.status,
               a.parent_name,
               a.chlid_name,
               a.id
          from T_AUDITEE_ENT_RELATION a;
    end if;
  end P_Get_Entities_Relationship;

  Procedure P_Update_Entities_Relationship(r_ship_id in number,
                                           p_type_id in number,
                                           c_type_id in number,
                                           active    in varchar2,
                                           p_name    in varchar2,
                                           c_name    in varchar2,
                                           map_id    in number,
                                           a_id      in number,
                                           io_cursor OUT t_cursor) as
  begin
    update T_AUDITEE_ENT_RELATION a
       set a.entity_realtion_id   = r_ship_id,
           a.parent_entity_typeid = p_type_id,
           a.child_entity_typeid  = c_type_id,
           a.status               = active,
           a.parent_name          = p_name,
           a.chlid_name           = c_name,
           a.id                   = map_id
     where a.id = a_id;
    commit;
  end P_Update_Entities_Relationship;

  Procedure P_GET_ENTITIES_MAPPING_REPORTING(ent_id        in number,
                                             P_TYPE        IN NUMBER,
                                             C_TYPE        IN NUMBER,
                                             REALTION_TYPE IN NUMBER,
                                             ind           IN VARCHAR2,
                                             io_cursor     OUT t_cursor) as
  begin
    if (IND = 'Y') then
      open io_cursor for
        select m.parent_id,
               m.parent_code,
               m.child_code,
               m.entity_id,
               m.auditedby,
               m.status,
               m.p_name,
               m.c_name,
               m.p_type_id,
               m.c_type_id,
               m.relation_type_id
          from T_AUDITEE_ENTITIES_MAPING_REPORTING m
         WHERE m.parent_id = ent_id
            OR M.P_TYPE_ID = P_TYPE
            OR M.C_TYPE_ID = C_TYPE
            OR M.RELATION_TYPE_ID = REALTION_TYPE;
    
    else
      open io_cursor for
        select m.parent_id,
               m.parent_code,
               m.child_code,
               m.entity_id,
               m.auditedby,
               m.status,
               m.p_name,
               m.c_name,
               m.p_type_id,
               m.c_type_id,
               m.relation_type_id
          from T_AUDITEE_ENTITIES_MAPING_REPORTING m;
    end if;
  
  end P_GET_ENTITIES_MAPPING_REPORTING;

  Procedure P_GET_ENTITIES_MAPPING(ent_id        in number,
                                   P_TYPE        IN NUMBER,
                                   C_TYPE        IN NUMBER,
                                   REALTION_TYPE IN NUMBER,
                                   ind           IN VARCHAR2,
                                   io_cursor     OUT t_cursor) as
  
  begin
    if (IND = 'Y') then
      open io_cursor for
        select m.parent_id,
               m.parent_code,
               m.child_code,
               m.entity_id,
               m.auditedby,
               m.status,
               m.p_name,
               m.c_name,
               m.p_type_id,
               m.c_type_id,
               m.relation_type_id
          from T_AUDITEE_ENTITIES_MAPING m
         WHERE M.ENTITY_ID = ent_id
            OR M.P_TYPE_ID = P_TYPE
            OR M.C_TYPE_ID = C_TYPE
            OR M.RELATION_TYPE_ID = REALTION_TYPE;
    
    else
      open io_cursor for
        select m.parent_id,
               m.parent_code,
               m.child_code,
               m.entity_id,
               m.auditedby,
               m.status,
               m.p_name,
               m.c_name,
               m.p_type_id,
               m.c_type_id,
               m.relation_type_id
          from T_AUDITEE_ENTITIES_MAPING m;
    end if;
  
  end P_GET_ENTITIES_MAPPING;

  Procedure P_ADD_ENTITIES_MAPPING_REPORTING(P_ID          IN NUMBER,
                                             P_CODE        IN NUMBER,
                                             C_CODE        IN NUMBER,
                                             C_ID          IN NUMBER,
                                             AUDIT_BY      IN NUMBER,
                                             e_STATUS      IN VARCHAR2,
                                             PAR_NAME      IN VARCHAR2,
                                             CH_NAME       IN VARCHAR2,
                                             P_TYPE        IN NUMBER,
                                             C_TYPE        IN NUMBER,
                                             RELATION_TYPE IN NUMBER,
                                             io_cursor     OUT t_cursor) AS
  BEGIN
  
    INSERT INTO T_AUDITEE_ENTITIES_MAPING_REPORTING
    
    VALUES
      (P_ID,
       P_CODE,
       C_CODE,
       C_ID,
       AUDIT_BY,
       e_STATUS,
       PAR_NAME,
       CH_NAME,
       P_TYPE,
       C_TYPE,
       RELATION_TYPE,
       P_ID || C_ID);
  
    COMMIT;
  END P_ADD_ENTITIES_MAPPING_REPORTING;

  Procedure P_get_entities(p_type    in number,
                           c_type    in number,
                           io_cursor OUT t_cursor) AS
  BEGIN
    if (p_type != 0) then
      open io_cursor for
        select distinct m.parent_id, m.p_name
          from t_auditee_entities_maping m
         where m.p_type_id = p_type;
    else
      open io_cursor for
        select distinct m.parent_id, m.p_name
          from t_auditee_entities_maping m
         where m.c_type_id = c_type;
    end if;
  end P_get_entities;

  Procedure P_update_entity_shifting_plan(p_id      in number,
                                          io_cursor OUT t_cursor) AS
  
  begin
  
    update t_au_plan p set p.status = 0 where p.id = p_id;
    commit;
    open io_cursor for
      select 'Plan has been made In-Active' as remarks from dual;
  
  end P_update_entity_shifting_plan;

  Procedure P_update_entity_shifting_engagement(p_id      in number,
                                                E_id      in number,
                                                io_cursor OUT t_cursor) AS
  
  begin
  
    Delete from t_au_plan_eng e
     where e.plan_id = p_id
       and e.eng_id = e_id;
    commit;
    insert into t_au_plan_eng_log
      (id, e_id, status_id, createdby_id, created_on, remarks, comments)
    values
      ((select COALESCE(max(a.id) + 1, 1) from t_au_plan_eng_log a),
       E_id,
       0,
       0,
       sysdate,
       'Engagement Deleted',
       'Entity shifted');
    commit;
  
    open io_cursor for
      select 'Engagement has been Deleted' as remarks from dual;
  
  end P_update_entity_shifting_engagement;

  PROCEDURE P_Add_Entity_Shifting(Old_Ent_Id IN NUMBER,
                                  New_Ent_Id IN NUMBER,
                                  P_No       IN NUMBER,
                                  Ent_Id     IN NUMBER,
                                  R_Id       IN NUMBER,
                                  Cir_No     IN VARCHAR2,
                                  Cir_Attach IN CLOB,
                                  Cir_Date   IN DATE,
                                  Io_Cursor  OUT T_Cursor) AS
    V_Old_Entity_Name T_Auditee_Entities.Name%TYPE;
    V_New_Entity_Name T_Auditee_Entities.Name%TYPE;
    V_New_Entity_Code T_Auditee_Entities.Code%TYPE;
    V_Old_Type_Id     T_Auditee_Entities.Type_Id%TYPE;
  
    V_Shift_Count    NUMBER := 0;
    V_Shift_Ref_Id   NUMBER := 0;
    V_Observation_Id NUMBER := 0;
    V_Config_Count   NUMBER := 0;
  
  BEGIN
    ------------------------------------------------------------------
    -- Basic validation
    ------------------------------------------------------------------
    IF Old_Ent_Id IS NULL OR New_Ent_Id IS NULL THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'Old entity and new entity IDs are required.');
    END IF;
  
    IF Old_Ent_Id = New_Ent_Id THEN
      RAISE_APPLICATION_ERROR(-20002,
                              'Old entity and new entity cannot be the same.');
    END IF;
  
    IF P_No IS NULL OR P_No <= 0 THEN
      RAISE_APPLICATION_ERROR(-20003,
                              'A valid logged-in user is required.');
    END IF;
  
    ------------------------------------------------------------------
    -- Obtain old entity information
    ------------------------------------------------------------------
    BEGIN
      SELECT E.Name, E.Type_Id
        INTO V_Old_Entity_Name, V_Old_Type_Id
        FROM T_Auditee_Entities E
       WHERE E.Entity_Id = Old_Ent_Id;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20004,
                                'Old entity ID ' || Old_Ent_Id ||
                                ' does not exist.');
    END;
  
    ------------------------------------------------------------------
    -- Obtain new entity information
    ------------------------------------------------------------------
    BEGIN
      SELECT E.Name, E.Code
        INTO V_New_Entity_Name, V_New_Entity_Code
        FROM T_Auditee_Entities E
       WHERE E.Entity_Id = New_Ent_Id;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20005,
                                'New entity ID ' || New_Ent_Id ||
                                ' does not exist.');
    END;
  
    ------------------------------------------------------------------
    -- Validate Annexure configuration for branch shifting
    ------------------------------------------------------------------
    IF V_Old_Type_Id = 6 THEN
    
      SELECT COUNT(*)
        INTO V_Config_Count
        FROM T_Au_Fad_Annexure_Config C
       WHERE C.Active = 'Y';
    
      IF V_Config_Count = 0 THEN
        RAISE_APPLICATION_ERROR(-20006,
                                'No active Annexure shifting configuration is available.');
      END IF;
    
    END IF;
  
    ------------------------------------------------------------------
    -- Check whether old entity has already been shifted
    ------------------------------------------------------------------
    SELECT COUNT(*)
      INTO V_Shift_Count
      FROM T_Au_Entity_Shifting S
     WHERE S.Old_Entity_Id = Old_Ent_Id;
  
    IF V_Shift_Count > 0 THEN
      OPEN Io_Cursor FOR
        SELECT V_Old_Entity_Name || ' to ' || V_New_Entity_Name ||
               ' shifting request has already been entered.' AS Remarks
          FROM Dual;
    
      RETURN;
    END IF;
  
    ------------------------------------------------------------------
    -- Generate shifting reference ID
    --
    -- Existing MAX + 1 mechanism retained.
    -- A database sequence should be introduced separately.
    ------------------------------------------------------------------
    SELECT NVL(MAX(S.Ref_Id), 0) + 1
      INTO V_Shift_Ref_Id
      FROM T_Au_Entity_Shifting S;
  
    INSERT INTO T_Au_Entity_Shifting
      (Ref_Id,
       Old_Entity_Id,
       New_Entity_Id,
       Circular_No,
       Circular_Date,
       Circular,
       Entered_By,
       Entered_On)
    VALUES
      (V_Shift_Ref_Id,
       Old_Ent_Id,
       New_Ent_Id,
       Cir_No,
       Cir_Date,
       Cir_Attach,
       P_No,
       SYSDATE);
  
    ------------------------------------------------------------------
    -- Generate observation-shifting history IDs
    ------------------------------------------------------------------
    SELECT NVL(MAX(S.Id), 0)
      INTO V_Observation_Id
      FROM T_Au_Observation_Shifting S;
  
    ------------------------------------------------------------------
    -- Record observation shifting history
    --
    -- For branch entities:
    --   Configured Y = shifted/open status 8
    --   Configured N, inactive, missing or null = closed status 28
    --
    -- ENG_ID does not override Annexure configuration.
    ------------------------------------------------------------------
    INSERT INTO T_Au_Observation_Shifting
      (Id,
       Shift_Ref_Id,
       Old_Entity_Id,
       New_Entity_Id,
       Old_Para_Id,
       New_Para_Id,
       Shifting_Date,
       Para_Status,
       Annex)
      SELECT V_Observation_Id + ROW_NUMBER() OVER (ORDER BY F.Old_Para_Id, F.New_Paraid) AS Id,
             V_Shift_Ref_Id,
             Old_Ent_Id,
             New_Ent_Id,
             F.Old_Para_Id,
             F.New_Paraid,
             SYSDATE,
             
             CASE
               WHEN T.Audit_Type = 'B' AND EXISTS
                (SELECT 1
                       FROM T_Au_Fad_Annexure_Config C
                      WHERE C.Annexure_Id = F.Annex
                        AND C.Shift_Applicable = 'Y'
                        AND C.Active = 'Y') THEN
                8
             
               WHEN T.Audit_Type = 'B' THEN
                28
             
               ELSE
                F.Para_Status
             END AS Para_Status,
             F.Annex
        FROM T_Au_Observation_Fad F
        JOIN T_Auditee_Entities E
          ON E.Entity_Id = F.Entity_Id
        JOIN T_Auditee_Ent_Types T
          ON T.Autid = E.Type_Id
       WHERE F.Entity_Id = Old_Ent_Id
         AND F.Para_Status = 8;
  
    ------------------------------------------------------------------
    -- Deactivate old entity
    ------------------------------------------------------------------
    UPDATE T_Auditee_Entities E
       SET E.Auditable = 'N', E.Active = 'N'
     WHERE E.Entity_Id = Old_Ent_Id;
  
    ------------------------------------------------------------------
    -- Remove old entity mapping
    ------------------------------------------------------------------
    DELETE FROM T_Auditee_Entities_Maping M WHERE M.Entity_Id = Old_Ent_Id;
  
    ------------------------------------------------------------------
    -- Shift entity-size records
    ------------------------------------------------------------------
    UPDATE T_Auditee_Entities_Size S
       SET S.Entity_Id = New_Ent_Id
     WHERE S.Entity_Id = Old_Ent_Id;
  
    ------------------------------------------------------------------
    -- Shift latest risk-period record
    ------------------------------------------------------------------
    UPDATE T_Auditee_Entities_Risk R
       SET R.Entity_Id = New_Ent_Id
     WHERE R.Entity_Id = Old_Ent_Id
       AND R.Audit_Period_Id =
           (SELECT MAX(R1.Audit_Period_Id)
              FROM T_Auditee_Entities_Risk R1
             WHERE R1.Entity_Id = Old_Ent_Id);
  
    ------------------------------------------------------------------
    -- Branch-type entity
    ------------------------------------------------------------------
    IF V_Old_Type_Id = 6 THEN
    
      --------------------------------------------------------------
      -- Move applicable current observations
      --
      -- ENGPLANID > 1261 identifies observations maintained in
      -- T_AU_OBSERVATION.
      --------------------------------------------------------------
      UPDATE T_Au_Observation O
         SET O.Entity_Id = New_Ent_Id, O.Entity_Code = V_New_Entity_Code
       WHERE O.Entity_Id = Old_Ent_Id
         AND O.Status = 8
         AND NVL(O.Engplanid, 0) > 1261
         AND EXISTS (SELECT 1
                FROM T_Au_Fad_Annexure_Config C
               WHERE C.Annexure_Id = O.Annex
                 AND C.Shift_Applicable = 'Y'
                 AND C.Active = 'Y');
    
      --------------------------------------------------------------
      -- Close non-applicable current observations
      --
      -- NOT EXISTS also covers:
      --   Annexure marked N
      --   inactive configuration
      --   missing configuration
      --   null Annexure
      --------------------------------------------------------------
      UPDATE T_Au_Observation O
         SET O.Status = 28
       WHERE O.Entity_Id = Old_Ent_Id
         AND O.Status = 8
         AND NVL(O.Engplanid, 0) > 1261
         AND NOT EXISTS (SELECT 1
                FROM T_Au_Fad_Annexure_Config C
               WHERE C.Annexure_Id = O.Annex
                 AND C.Shift_Applicable = 'Y'
                 AND C.Active = 'Y');
    
      --------------------------------------------------------------
      -- Move applicable old FAD paras
      --------------------------------------------------------------
      UPDATE T_Au_Old_Paras_Fad F
         SET F.Entity_Id   = New_Ent_Id,
             F.Entity_Code = V_New_Entity_Code,
             F.Entity_Name = V_New_Entity_Name
       WHERE F.Entity_Id = Old_Ent_Id
         AND F.Para_Status = 8
         AND EXISTS (SELECT 1
                FROM T_Au_Fad_Annexure_Config C
               WHERE C.Annexure_Id = F.Annex
                 AND C.Shift_Applicable = 'Y'
                 AND C.Active = 'Y');
    
      --------------------------------------------------------------
      -- Close non-applicable old FAD paras
      --------------------------------------------------------------
      UPDATE T_Au_Old_Paras_Fad F
         SET F.Para_Status = 28
       WHERE F.Entity_Id = Old_Ent_Id
         AND F.Para_Status = 8
         AND NOT EXISTS (SELECT 1
                FROM T_Au_Fad_Annexure_Config C
               WHERE C.Annexure_Id = F.Annex
                 AND C.Shift_Applicable = 'Y'
                 AND C.Active = 'Y');
    
    ELSE
      --------------------------------------------------------------
      -- Other entity types: move all open observations
      --------------------------------------------------------------
      UPDATE T_Au_Observation O
         SET O.Entity_Id = New_Ent_Id, O.Entity_Code = V_New_Entity_Code
       WHERE O.Entity_Id = Old_Ent_Id
         AND O.Status = 8;
    
      UPDATE T_Au_Old_Paras_Fad F
         SET F.Entity_Id   = New_Ent_Id,
             F.Entity_Code = V_New_Entity_Code,
             F.Entity_Name = V_New_Entity_Name
       WHERE F.Entity_Id = Old_Ent_Id
         AND F.Para_Status = 8;
    
      UPDATE T_Au_Observation_Old_Cad_Paras C
         SET C.Entity_Id = New_Ent_Id, C.Entity_Name = V_New_Entity_Name
       WHERE C.Entity_Id = Old_Ent_Id
         AND C.Para_Status = 8;
    END IF;
  
    ------------------------------------------------------------------
    -- Update post-compliance records
    ------------------------------------------------------------------
    FOR R_Shift IN (SELECT S.Para_Status,
                           S.Old_Para_Id,
                           S.New_Para_Id,
                           S.Old_Entity_Id
                      FROM T_Au_Observation_Shifting S
                     WHERE S.Old_Entity_Id = Old_Ent_Id
                       AND S.New_Entity_Id = New_Ent_Id) LOOP
      UPDATE Ais_T_Au_Post_Compliance C
         SET C.Entity_Id = New_Ent_Id, C.Para_Status = R_Shift.Para_Status
       WHERE C.Entity_Id = R_Shift.Old_Entity_Id
         AND (C.Old_Para_Id = R_Shift.Old_Para_Id OR
             C.New_Para_Id = R_Shift.New_Para_Id);
    END LOOP;
  
    ------------------------------------------------------------------
    -- Commit complete shifting transaction
    ------------------------------------------------------------------
    COMMIT;
  
    OPEN Io_Cursor FOR
      SELECT V_Old_Entity_Name || ' has been shifted to ' ||
             V_New_Entity_Name || '.' AS Remarks
        FROM Dual;
  
  EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
      ROLLBACK;
    
      RAISE_APPLICATION_ERROR(-20007,
                              'Duplicate record encountered during entity shifting.');
    
    WHEN OTHERS THEN
      ROLLBACK;
    
      IF SQLCODE BETWEEN - 20999 AND - 20000 THEN
        RAISE;
      ELSE
        RAISE_APPLICATION_ERROR(-20099,
                                'Entity shifting failed due to a database error.');
      END IF;
    
  END P_Add_Entity_Shifting;
  
  procedure P_Shift_BR_to_islamic(Old_br    number,
                                  new_br    number,
                                  io_cursor OUT t_cursor) as
  
    eng_num number := 0;
  begin
  
    select nvl(max(e.eng_id), 0)
      into eng_num
      from t_au_plan_eng e
     where e.entity_id = old_br;
  
    update T_AU_PLAN_ENG E
       set e.entity_id = new_br
     WHERE E.Entity_Id = old_br
       and e.eng_id = eng_num;
    commit;
  
    update t_au_observation o
       set o.entity_id = new_br
     WHERE o.Entity_Id = old_br
       and o.engplanid = eng_num;
    commit;
  
    update t_au_observation_assignedto ao
       set ao.entity_id = new_br
     where ao.entity_id = old_br
       and ao.eng_id = eng_num;
    commit;
  
    update ais_t_au_post_compliance c
       set c.entity_id = new_br
     where c.entity_id = old_br
       and c.para_status = 8;
    --and extract(year from c.para_added_on) = extract(year from sysdate);
    commit;
  
    update t_au_audit_team_tasklist t
       set t.entity_id = new_br
     where t.eng_plan_id = eng_num;
    commit;
  
    open io_cursor for
      select 'Entity Shifting performed successfully' as remarks from dual;
  
  end P_Shift_BR_to_islamic;

  procedure P_get_roles_for_compliance_flow(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      select g.* from t_groups g WHERE g.STATUS = 'Y' ORDER BY g.GROUP_ID;
  
  end P_get_roles_for_compliance_flow;

  procedure P_get_ent_types_for_compliance_flow(ENT_ID    in number,
                                                P_NO      in number,
                                                R_ID      in number,
                                                io_cursor OUT t_cursor) as
  
  begin
    if (R_ID = 1) then
      OPEN io_cursor FOR
        Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
          FROM t_auditee_ent_types G;
    else
      OPEN io_cursor FOR
        Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
          FROM t_auditee_ent_types G
         where g.audited_by_enitity = ENT_ID;
    end if;
  end P_get_ent_types_for_compliance_flow;

  procedure P_get_ent_types_for_hr_designation(ENT_ID    in number,
                                               P_NO      in number,
                                               R_ID      in number,
                                               io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select distinct (G.ENTITYTYPEDESC) AS ENTITY_TYPE, g.entitycode
        FROM t_auditee_ent_types G;
  end P_get_ent_types_for_hr_designation;

  procedure P_get_compliance_statuses_for_compliance_flow(io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select s.statusid, s.statusname from t_au_observation_status s;
  end P_get_compliance_statuses_for_compliance_flow;

  PROCEDURE P_get_group_prev_next_stage(E_TYPE    IN NUMBER,
                                        G_ID      IN NUMBER,
                                        io_cursor OUT t_cursor) AS
  BEGIN
  
    OPEN io_cursor FOR
      SELECT c.entity_type as e_id,
             c.role_id     as g_id,
             c.next_r_id   as next_r_id,
             c.per_r_id    as prev_r_id
        from t_au_post_compliance_flow c
       where c.entity_type = E_TYPE
         and c.role_id = G_ID;
  END P_get_group_prev_next_stage;

  PROCEDURE P_get_entity_type_compliance_flow(E_TYPE    IN NUMBER,
                                              G_ID      in number,
                                              io_cursor OUT t_cursor) AS
  BEGIN
    if (E_TYPE = 0 or E_TYPE is null) then
      --- change 0 and or
      if (G_ID = 0) then
        OPEN io_cursor FOR
          SELECT c.entity_type as e_id,
                 (select t.entitytypedesc
                    from t_auditee_ent_types t
                   where t.autid = c.entity_type) as e_name,
                 c.role_id as g_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.role_id) as g_name,
                 c.next_r_id as next_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.next_r_id) as next_r_name,
                 c.per_r_id as prev_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.per_r_id) as prev_r_name,
                 c.c_status_up,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_up) as c_status_up_desc,
                 c.c_status_down,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_down) as c_status_down_desc,
                 c.id
            from t_au_post_compliance_flow c;
        --where c.entity_type = E_TYPE;
      else
        OPEN io_cursor FOR
          SELECT c.entity_type as e_id,
                 (select t.entitytypedesc
                    from t_auditee_ent_types t
                   where t.autid = c.entity_type) as e_name,
                 c.role_id as g_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.role_id) as g_name,
                 c.next_r_id as next_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.next_r_id) as next_r_name,
                 c.per_r_id as prev_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.per_r_id) as prev_r_name,
                 c.c_status_up,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_up) as c_status_up_desc,
                 c.c_status_down,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_down) as c_status_down_desc,
                 c.id
            from t_au_post_compliance_flow c
           inner join t_auditee_ent_types en
              on en.autid = c.entity_type;
      end if;
    else
    
      if (G_ID = 0 or G_ID is null) then
        OPEN io_cursor FOR
          SELECT c.entity_type as e_id,
                 (select t.entitytypedesc
                    from t_auditee_ent_types t
                   where t.autid = c.entity_type) as e_name,
                 c.role_id as g_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.role_id) as g_name,
                 c.next_r_id as next_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.next_r_id) as next_r_name,
                 c.per_r_id as prev_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.per_r_id) as prev_r_name,
                 c.c_status_up,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_up) as c_status_up_desc,
                 c.c_status_down,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_down) as c_status_down_desc,
                 c.id
            from t_au_post_compliance_flow c
           where c.entity_type = E_TYPE;
      
      else
        OPEN io_cursor FOR
          SELECT c.entity_type as e_id,
                 (select t.entitytypedesc
                    from t_auditee_ent_types t
                   where t.autid = c.entity_type) as e_name,
                 c.role_id as g_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.role_id) as g_name,
                 c.next_r_id as next_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.next_r_id) as next_r_name,
                 c.per_r_id as prev_r_id,
                 (select p.group_name
                    from t_groups p
                   where p.role_id = c.per_r_id) as prev_r_name,
                 c.c_status_up,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_up) as c_status_up_desc,
                 c.c_status_down,
                 (select s.statusname
                    from t_au_observation_status s
                   where s.statusid = c.c_status_down) as c_status_down_desc,
                 c.id
            from t_au_post_compliance_flow c
           where c.entity_type = E_TYPE
             and c.role_id = G_ID;
      end if;
    
    end if;
  END P_get_entity_type_compliance_flow;

  PROCEDURE P_add_update_compliance_flow(f_id          in number,
                                         TYPE_ID       IN NUMBER,
                                         GROUP_ID      IN NUMBER,
                                         P_GROUP_ID    IN NUMBER,
                                         N_GROUP_ID    IN NUMBER,
                                         C_UP_STATUS   IN NUMBER,
                                         C_DOWN_STATUS IN NUMBER,
                                         io_cursor     OUT t_cursor) AS
  
    R_F number := 0;
    S_F number := 0;
  BEGIN
    MERGE INTO t_au_post_compliance_flow target
    USING dual
    ON (target.entity_type = TYPE_ID AND target.id = f_id)
    WHEN MATCHED THEN
      UPDATE
         SET target.next_r_id     = N_GROUP_ID,
             target.per_r_id      = P_GROUP_ID,
             target.c_status_up   = C_UP_STATUS,
             target.c_status_down = C_DOWN_STATUS,
             target.role_id       = GROUP_ID,
             target.active        = 'Y'
    WHEN NOT MATCHED THEN
      INSERT
        (id,
         entity_type,
         role_id,
         next_r_id,
         per_r_id,
         c_status_up,
         c_status_down,
         active)
      VALUES
        ((SELECT COALESCE(MAX(a.id) + 1, 1)
           FROM t_au_post_compliance_flow a),
         TYPE_ID,
         GROUP_ID,
         N_GROUP_ID,
         P_GROUP_ID,
         C_UP_STATUS,
         C_DOWN_STATUS,
         'Y');
  
    COMMIT;
  
    OPEN io_cursor FOR
      SELECT 'Compliance Work Flow updated' AS remarks FROM dual;
  END P_add_update_compliance_flow;

  Procedure P_GET_HR_DESIGNATION_RIGHT(io_cursor OUT t_cursor) AS
  BEGIN
    OPEN io_cursor FOR
      select r.designationcode,
             r.description,
             r.statustype,
             r.categorytype,
             r.group_id,
             r.entity_type,
             r.sub_entity_type,
             r.id
        from t_group_rights r
       where r.statustype = 'A';
  
  end P_GET_HR_DESIGNATION_RIGHT;

  Procedure P_UPDATE_HR_DESIGNATION_RIGHT(M_ID                IN NUMBER,
                                          HR_DES_CODE         in number,
                                          AIS_GROUP_ID        IN NUMBER,
                                          AIS_SUB_ENTITY_TYPE IN VARCHAR2,
                                          io_cursor           OUT t_cursor) AS
  BEGIN
    UPDATE t_group_rights GR
       SET GR.DESIGNATIONCODE = hr_DES_CODE,
           GR.DESCRIPTION    =
           (select h.DESCRIPTION
              from t_hr_designations h
             where h.DESIGNATIONCODE = hr_DES_CODE),
           GR.STATUSTYPE     =
           (select h.STATUSTYPE
              from t_hr_designations h
             where h.DESIGNATIONCODE = hr_DES_CODE),
           GR.CATEGORYTYPE   =
           (select h.CATEGORYTYPE
              from t_hr_designations h
             where h.DESIGNATIONCODE = hr_DES_CODE),
           GR.GROUP_ID        = AIS_GROUP_ID,
           GR.ENTITY_TYPE    =
           (select g.group_name
              from t_groups g
             where g.role_id = AIS_GROUP_ID),
           GR.SUB_ENTITY_TYPE = AIS_SUB_ENTITY_TYPE
     WHERE GR.ID = M_ID;
    COMMIT;
    open io_cursor for
      select 'Rights Updated' as remarks from dual;
  END P_UPDATE_HR_DESIGNATION_RIGHT;

  Procedure P_ADD_HR_DESIGNATION_RIGHT(HR_DES_CODE         in number,
                                       AIS_GROUP_ID        IN NUMBER,
                                       AIS_SUB_ENTITY_TYPE in VARCHAR2,
                                       io_cursor           OUT t_cursor) AS
  BEGIN
    insert into t_group_rights
      (id,
       designationcode,
       description,
       statustype,
       categorytype,
       group_id,
       entity_type,
       sub_entity_type)
    
      select (select COALESCE(max(a.id) + 1, 1) from t_group_rights a),
             d.DESIGNATIONCODE,
             d.DESCRIPTION,
             d.STATUSTYPE,
             d.CATEGORYTYPE,
             g.group_id,
             g.description,
             AIS_SUB_ENTITY_TYPE
      
        from t_hr_designations d, t_groups g
       where g.group_id = AIS_GROUP_ID
         and d.DESIGNATIONCODE = HR_DES_CODE;
    commit;
  
    open io_cursor for
      select 'Rights Added' as remarks from dual;
  
  end P_ADD_HR_DESIGNATION_RIGHT;

  Procedure P_GET_OBS_STATUS(io_cursor OUT t_cursor) AS
  BEGIN
    OPEN io_cursor FOR
      select r.statusid, r.statusname, r.isactive, r.code, r.satisfied
        from t_au_observation_status r;
  
  end P_GET_OBS_STATUS;

  Procedure P_ADD_OBS_STATUS(S_NAME    in varchar2,
                             ACTIVE    IN varchar2,
                             S_CODE    in VARCHAR2,
                             SATISFY   in VARCHAR2,
                             io_cursor OUT t_cursor) AS
  BEGIN
    insert into t_au_observation_status r
      (r.statusid, r.statusname, r.isactive, r.code, r.satisfied)
    
    Values
      ((select COALESCE(max(a.statusid) + 1, 1)
         from t_au_observation_status a),
       S_NAME,
       ACTIVE,
       S_CODE,
       SATISFY);
  
    commit;
  
    open io_cursor for
      select 'Observation Status Added' as remarks from dual;
  
  end P_ADD_OBS_STATUS;

  Procedure P_UPDATE_OBS_STATUS(S_ID    in number,
                                S_NAME  in varchar2,
                                ACTIVE  IN varchar2,
                                S_CODE  in VARCHAR2,
                                SATISFY in VARCHAR2,
                                
                                io_cursor OUT t_cursor) AS
  BEGIN
    UPDATE t_au_observation_status s
       SET s.statusname = S_NAME,
           s.isactive   = ACTIVE,
           s.code       = S_CODE,
           s.satisfied  = SATISFY
    
     WHERE s.statusid = S_ID;
    COMMIT;
    open io_cursor for
      select 'Observation Status Updated' as remarks from dual;
  END P_UPDATE_OBS_STATUS;

  Procedure P_GET_ENTITIES_AUDIT_DEPARTMENT(io_cursor OUT t_cursor) AS
  BEGIN
    OPEN io_cursor FOR
      select r.deptid,
             r.deptcode,
             r.deptname,
             r.status,
             r.cbas_code,
             r.entity_id,
             r.audit_id,
             r.auditor
        from t_audit_departments r;
  
  end P_GET_ENTITIES_AUDIT_DEPARTMENT;

  Procedure P_UPDATE_ENTITIES_AUDIT_DEPARTMENT(R_ID      in number,
                                               D_ID      in number,
                                               D_CODE    IN number,
                                               D_NAME    in VARCHAR2,
                                               STATUS    in VARCHAR2,
                                               CBAS_CODE in number,
                                               ENT_ID    in number,
                                               AUD_ID    in number,
                                               AUDITOR   in varchar2,
                                               io_cursor OUT t_cursor) AS
  BEGIN
    UPDATE t_audit_departments s
       SET s.deptid    = D_ID,
           s.deptcode  = D_CODE,
           s.deptname  = D_NAME,
           s.status    = STATUS,
           s.cbas_code = CBAS_CODE,
           s.entity_id = ENT_ID,
           s.audit_id  = AUD_ID,
           s.auditor   = AUDITOR
    
     WHERE s.ENTITY_ID = R_ID;
    COMMIT;
    open io_cursor for
      select 'Entities Audit Department Updated' as remarks from dual;
  END P_UPDATE_ENTITIES_AUDIT_DEPARTMENT;

  Procedure P_GET_ALL_MENU(io_cursor OUT t_cursor) AS
  BEGIN
    open io_cursor for
      select m.menu_id,
             m.sub_menu as app_id,
             m.menu_name,
             m.menu_order,
             m.menu_description,
             m.menu_image_path,
             m.isactive
        from t_menu m;
  
  end P_GET_ALL_MENU;

  Procedure P_GET_SUB_MENUS(M_ID in number, io_cursor OUT t_cursor) AS
  BEGIN
  
    open io_cursor for
      select s.sub_menu_id,
             s.menu_id,
             s.sub_menu_name,
             s.sub_menu_order,
             s.description,
             s.status
        from t_menu_sub s
       where s.menu_id = M_ID
       order by s.sub_menu_order;
  end P_GET_SUB_MENUS;

  Procedure P_ADD_NEW_SUB_MENU(M_ID      in number,
                               SM_NAME   in varchar2,
                               SM_ORDER  in number,
                               SM_STATUS in varchar2,
                               SM_DESC   in varchar2,
                               io_cursor OUT t_cursor) AS
  BEGIN
  
    Insert into t_menu_sub p
      (sub_menu_id,
       menu_id,
       sub_menu_name,
       sub_menu_order,
       description,
       status)
    values
      ((SELECT COALESCE(max(s.sub_menu_id) + 1, 1) FROM t_menu_sub s),
       M_ID,
       SM_NAME,
       SM_ORDER,
       SM_DESC,
       SM_STATUS);
    commit;
  
    open io_cursor for
      Select SM_NAME || '  Added' as remarks from dual;
  
  end P_ADD_NEW_SUB_MENU;

  Procedure P_UPDATE_SUB_MENU(SM_ID     in number,
                              M_ID      in number,
                              SM_NAME   in varchar2,
                              SM_ORDER  in number,
                              SM_STATUS in varchar2,
                              SM_DESC   in varchar2,
                              io_cursor OUT t_cursor) AS
  BEGIN
  
    Update t_menu_sub s
       set s.menu_id        = M_ID,
           s.sub_menu_name  = SM_NAME,
           s.sub_menu_order = SM_ORDER,
           s.description    = SM_DESC,
           s.status         = SM_STATUS
     where s.sub_menu_id = SM_ID;
  
    commit;
  
    open io_cursor for
      Select SM_NAME || '  Updated' as remarks from dual;
  
  end P_UPDATE_SUB_MENU;

  Procedure P_GET_ALL_PAGES(M_ID      in number,
                            SM_ID     in number,
                            io_cursor OUT t_cursor) AS
  BEGIN
    if (SM_ID = 0) then
      open io_cursor for
        SELECT p.id,
               p.menu_id,
               p.page_name,
               p.page_key,
               p.page_url,
               s.sub_menu_id,
               s.sub_menu_name,
               p.page_path,
               p.page_order,
               p.status,
               p.hide_menu
          FROM t_menu_pages p
         INNER JOIN t_menu m
            ON m.menu_id = p.menu_id
          LEFT JOIN t_menu_sub s
            ON s.sub_menu_id = P.SUB_MENU
         WHERE m.menu_id = M_ID
         ORDER BY s.sub_menu_id, p.page_order;
    else
      open io_cursor for
        SELECT p.id,
               p.menu_id,
               p.page_name,
               p.page_key,
               p.page_url,
               s.sub_menu_id,
               s.sub_menu_name,
               p.page_path,
               p.page_order,
               p.status,
               p.hide_menu
          FROM t_menu_pages p
         INNER JOIN t_menu m
            ON m.menu_id = p.menu_id
          LEFT JOIN t_menu_sub s
            ON s.sub_menu_id = p.sub_menu
         WHERE m.menu_id = M_ID
           and s.sub_menu_id = SM_ID
         ORDER BY s.sub_menu_id, p.page_order;
    end if;
  end P_GET_ALL_PAGES;

  Procedure P_ADD_NEW_PAGE(M_ID        in number,
                           SM_ID       in number,
                           P_NAME      in varchar2,
                           P_PAGE_KEY  in Varchar2,
                           P_PAGE_URL  in varchar2,
                           P_PATH      in varchar2,
                           P_ORDER     in number,
                           P_STATUS    in varchar2,
                           P_HIDE_MENU in number,
                           io_cursor   OUT t_cursor) AS
  BEGIN
    if (SM_ID = 0) then
      Insert into t_menu_pages
        (id,
         menu_id,
         page_name,
         page_path,
         page_order,
         status,
         hide_menu,
         sub_menu,
         page_key,
         page_url)
      values
        ((SELECT COALESCE(max(PP.id) + 1, 1) FROM t_menu_pages PP),
         M_ID,
         P_NAME,
         P_PATH,
         P_ORDER,
         P_STATUS,
         P_HIDE_MENU,
         '',
         P_PAGE_KEY,
         P_PAGE_URL);
      commit;
    else
      Insert into t_menu_pages
        (id,
         menu_id,
         page_name,
         page_path,
         page_order,
         status,
         hide_menu,
         sub_menu,
         page_key,
         page_url)
      values
        ((SELECT COALESCE(max(PP.id) + 1, 1) FROM t_menu_pages PP),
         M_ID,
         P_NAME,
         P_PATH,
         P_ORDER,
         P_STATUS,
         P_HIDE_MENU,
         SM_ID,
         P_PAGE_KEY,
         P_PAGE_URL);
      commit;
    end if;
    open io_cursor for
      Select P_NAME || '  Added' as remarks from dual;
  
  end P_ADD_NEW_PAGE;

  Procedure P_UPDATE_PAGE(P_ID        in number,
                          M_ID        in number,
                          SM_ID       in number,
                          P_NAME      in varchar2,
                          P_PAGE_KEY  in varchar2,
                          P_PAGE_URL  in varchar2,
                          P_PATH      in varchar2,
                          P_ORDER     in number,
                          P_STATUS    in varchar2,
                          P_HIDE_MENU in number,
                          io_cursor   OUT t_cursor) AS
  BEGIN
    if (SM_ID = 0) then
      Update t_menu_pages p
         set p.menu_id    = M_ID,
             p.sub_menu   = '',
             p.page_name  = P_NAME,
             p.page_path  = P_PATH,
             p.page_order = P_ORDER,
             p.status     = P_STATUS,
             p.hide_menu  = P_HIDE_MENU,
             p.page_key   = P_PAGE_KEY,
             p.page_url   = P_PAGE_URL
       where p.id = P_ID;
    else
      Update t_menu_pages p
         set p.menu_id    = M_ID,
             p.sub_menu   = SM_ID,
             p.page_name  = P_NAME,
             p.page_path  = P_PATH,
             p.page_order = P_ORDER,
             p.status     = P_STATUS,
             p.hide_menu  = P_HIDE_MENU,
             p.page_key   = P_PAGE_KEY,
             p.page_url   = P_PAGE_URL
       where p.id = P_ID;
    end if;
    commit;
  
    open io_cursor for
      Select P_NAME || '  Updated' as remarks from dual;
  
  end P_UPDATE_PAGE;

  Procedure P_GET_COMPLIANCE_OFFICE(io_cursor OUT t_cursor) AS
  BEGIN
    open io_cursor for
      select e.COM_KEY as entity_id,
             '( ' || upper(E.Compliance_Unit) || '  )   --' ||
             e.Approver_name || ' ---' || e.Reviewer_name as name
      
        from V_GET_COMPLIANCE_REVIEWER_APPROVER e
       order by e.ENTITY_ID;
    /*      select e.entity_id, e.name, ee.name as audited_by
     from t_auditee_entities e
    inner join t_auditee_entities ee
       on e.auditby_id = ee.entity_id
    where e.type_id = 22
      and e.active = 'Y';*/
  
  end P_GET_COMPLIANCE_OFFICE;

  Procedure P_UPDATE_ENTITY_COMP(R_ID       in number,
                                 ENT_ID     in number,
                                 Auditor    in number,
                                 compliance in number,
                                 io_cursor  out t_cursor) as
  begin
    if (R_ID IN (1, 41)) then
      Update t_auditee_entities e
         set e.complice_by     = compliance,
             e.compliance_unit =
             (select c.entity_id
                from t_auditee_entities_maping_com c
               where c.com_key = compliance)
       where e.entity_id = ent_id;
      commit;
      open io_cursor for
        select 'Compliance Office has been Updated in selected entities' as remarks
          from dual;
    else
      if (R_ID = 2) then
        UPDATE t_auditee_entities ee
           set ee.auditby_id = auditor
         where ee.entity_id = ent_id;
        commit;
        open io_cursor for
          select 'Audit Clustor has been Updated in selected entities' as remarks
            from dual;
      end if;
    end if;
  
  end P_UPDATE_ENTITY_COMP;

  Procedure P_GET_ENTITY_FOR_PARA_Reconsilation(R_ID      in number,
                                                ENT_ID    in number,
                                                io_cursor out t_cursor) as
  begin
    if (R_ID in (15, 16)) then
      open io_cursor for
        select e.entity_id, e.name
          from t_auditee_entities e
         where e.type_id = ENT_ID;
    end if;
  
  end P_GET_ENTITY_FOR_PARA_Reconsilation;

  procedure P_add_branch_risk_rating(ENGID     in number,
                                     io_cursor out t_cursor) as
  
  begin
  
    DELETE FROM T_RISK_BRANCH_WISE d where d.eng_id = engid;
    DELETE FROM T_BRANCH_RISK_RATING r where r.eng_id = ENGID;
    COMMIT;
  
    INSERT INTO T_RISK_BRANCH_WISE
      (ENG_ID, GR_ID, S_GR_ID, MAX_NUMBER, WEIGHTAGE_AVERAGE, GRAVITY_RISK)
    
      SELECT engid,
             r.gr_id,
             rs.s_gr_id,
             r.max_number,
             rs.weightage as Weighted_Average,
             RS.GRAVITY
      
        FROM T_R_SUB_GROUP RS
       INNER JOIN T_R_GROUP R
          ON R.GR_ID = RS.GR_ID
       ORDER BY RS.GR_ID, RS.S_GR_ID;
    commit;
  
    for j in (select p.description,
                     d.entity_code,
                     d.entity_id,
                     cd.v_id,
                     count(ob.id) as no_of_ob,
                     sum(ob.no_of_instances) as no_of_inst
                from t_au_observation ob
               inner join t_au_plan_eng d
                  on d.eng_id = ob.engplanid
               inner join t_au_period p
                  on p.auditperiodid = d.period_id
               inner join t_audit_checklist_details cd
                  on cd.id = ob.checklistdetail_id
               where ob.engplanid = engid
               group by p.description, d.entity_code, d.entity_id, cd.v_id) loop
    
      update T_RISK_BRANCH_WISE t
         set t.audit_period           = extract(year from sysdate),
             t.entity_id              = j.entity_id,
             t.enitity_code           = j.entity_code,
             t.number_of_observations = j.no_of_ob
       where t.eng_id = engid
         and t.s_gr_id = j.v_id
         and t.gr_id = 1;
      commit;
    
      update T_RISK_BRANCH_WISE t
         set t.audit_period           = extract(year from sysdate),
             t.entity_id              = j.entity_id,
             t.enitity_code           = j.entity_code,
             t.number_of_observations = j.no_of_inst
       where t.eng_id = engid
         and t.s_gr_id = j.v_id
         and t.gr_id in (2, 3);
      commit;
    
    end loop;
    update T_RISK_BRANCH_WISE t
       set t.risk_based_marks =
           (t.number_of_observations * T.GRAVITY_RISK)
     where t.eng_id = ENGID;
    commit;
  
    update T_RISK_BRANCH_WISE t
       set t.weighted_average_marks =
           (t.risk_based_marks * t.weightage_average)
     where t.eng_id = ENGID;
    commit;
  
    update T_RISK_BRANCH_WISE t
       set t.weighted_average_marks = (case
                                        when t.weighted_average_marks >
                                             t.max_number then
                                         t.max_number
                                        else
                                         t.weighted_average_marks
                                      end)
     where t.eng_id = ENGID;
    commit;
  
    INSERT INTO T_BRANCH_RISK_RATING
      (AUDIT_PERIOD_ID, BRANCH_CODE, RISK_RATING)
      SELECT BB.AUDIT_PERIOD, BB.ENITITY_CODE, SUM(BB.RISK_BASED_MARKS)
        FROM T_RISK_BRANCH_WISE BB
       where bb.eng_id = ENGID
       GROUP BY BB.AUDIT_PERIOD, BB.ENITITY_CODE;
    COMMIT;
  
    UPDATE T_BRANCH_RISK_RATING b
       set b.risk_category =
           (select r.rating
              from T_COSO_RATING r
             where b.risk_rating between (r.range_start) and (r.range_end))
     where b.eng_id = ENGID;
    commit;
  
    for c in (select s.s_gr_id, s.max_number, count(o.id) as no_of_ob
                from t_au_observation o
               inner join t_audit_checklist_details d
                  on d.id = o.checklistdetail_id
               inner join t_r_sub_group s
                  on s.s_gr_id = d.v_id
               where o.engplanid = ENGID
               group by s.s_gr_id, s.max_number) loop
    
      update T_RISK_BRANCH_WISE t
         set t.cia_marks = (case
                             when c.no_of_ob > 0 then
                              c.max_number
                             else
                              0
                           end)
       where t.eng_id = engid
         and t.s_gr_id = c.s_gr_id;
      commit;
    end loop;
  
    open io_cursor for
      select 'Risk for the entity generated' as remarks from dual;
  
  end P_add_branch_risk_rating;

  procedure p_get_traditional_risk_rating(ENGID     in number,
                                          io_cursor out t_cursor) as
  
  begin
    open io_cursor for
      select g.gr_id,
             s.s_gr_id,
             g.description as main_process,
             s.description as risk_model,
             nvl(s.max_number, 0) max_number,
             nvl(s.weightage, 0) weightage,
             nvl(s.gravity, 0) gravity,
             nvl(sum(b.weightage_average), 0) as weightage_average,
             nvl(sum(b.gravity_risk), 0) as gravity_risk,
             nvl(sum(b.number_of_observations), 0) as number_of_observations,
             nvl(sum(b.risk_based_marks), 0) as risk_based_marks,
             nvl(sum(b.weighted_average_marks), 0) as weighted_average_marks,
             nvl(sum(b.cia_marks), 0) as cia_marks
        from t_r_group g
       inner join t_r_sub_group s
          on g.gr_id = s.gr_id
       inner join T_RISK_BRANCH_WISE b
          on b.s_gr_id = s.s_gr_id
       where b.eng_id = engid
       group by g.gr_id,
                s.s_gr_id,
                g.description,
                s.description,
                s.max_number,
                s.weightage,
                s.gravity
       order by g.gr_id, s.s_gr_id;
  
  end p_get_traditional_risk_rating;

  Procedure p_get_new_risk_model(eng_id in number, io_cursor out t_cursor) as
  
  begin
    open io_cursor for
      select c.risk_sequence,
             c.heading as Main_process,
             c.weight_assigned,
             s.risk_sequence,
             s.heading,
             s.weight_assigned,
             sum(case
                   when d.risk_id = 3 then
                    1
                   else
                    0
                 end) as High,
             sum(case
                   when d.risk_id = 2 then
                    1
                   else
                    0
                 end) as medium,
             sum(case
                   when d.risk_id = 1 then
                    1
                   else
                    0
                 end) as Low,
             count(d.id) as total_no_of_test,
             SUM(R.AVAILABLE_WEIGHTED_SCORE) AS AVAILABLE_WEIGHTED_SCORE,
             SUM(R.AVAILABLE_PROCESS_SCORE) AS AVAILABLE_PROCESS_SCORE,
             SUM(R.HIGH) AS HIGH,
             SUM(R.MEDIUM) AS MEDIUM,
             SUM(R.LOW) AS LOW,
             SUM(R.TOTAL) AS TOTAL_OBS,
             SUM(R.TOTAL_SCORE_SUB_PROCESS) AS TOTAL_SCORE_SUB_PROCESS,
             SUM(R.WEIGHTED_AVERAGE_SCORE) AS WEIGHTED_AVERAGE_SCORE,
             SUM(R.TOTAL_SCORE_PROCESS) AS TOTAL_SCORE_PROCESS,
             SUM(R.WEIGHTED_AVERAGE_SCORE_OVERALL) AS WEIGHTED_AVERAGE_SCORE_OVERALL
      
        from t_audit_checklist c
       inner join t_audit_checklist_sub s
          on c.t_id = s.t_id
       inner join t_audit_checklist_details d
          on d.s_id = s.s_id
        LEFT JOIN t_risk_new_model_working R
          ON R.S_ID = S.S_ID
       group by c.risk_sequence,
                c.heading,
                c.weight_assigned,
                s.risk_sequence,
                s.heading,
                s.weight_assigned;
  
  end p_get_new_risk_model;

  Procedure P_GET_compliance_hierarchy(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
      select r.ENTITY_ID,
             r.Compliance_Unit,
             r.APPROVER_PPNO,
             r.Approver_name,
             r.REVIEWER_PPNO,
             r.Reviewer_name,
             r.COM_KEY
        from v_Get_Compliance_Reviewer_Approver r
       order by r.Compliance_Unit, r.APPROVER_PPNO, r.REVIEWER_PPNO;
  
  end P_GET_compliance_hierarchy;

  Procedure P_GET_SUBCHECKILIST(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
      select s.s_id,
             s.t_id,
             s.heading,
             s.entity_type,
             s.status,
             s.weight_assigned,
             s.risk_sequence
      
        from t_audit_checklist_sub s
       where exists (select 'z'
                from t_audit_checklist_sub_merger m
               where m.s_id = s.s_id
                 and m.status = 'P');
  
  end P_GET_SUBCHECKILIST;

  Procedure P_UPDATE_COM_OFFICER(ENT_ID    number,
                                 AP_P_NO   number,
                                 RE_P_NO   number,
                                 E_COM_KEY varchar2,
                                 io_cursor out t_cursor) as
  
    Z_B number := 0;
  begin
    select nvl(max(cb.entity_id), 0)
      into Z_B
      from t_auditee_entities_maping_com cb
     where cb.com_key = ENT_ID || AP_P_NO || RE_P_NO;
  
    if (Z_B = 0) then
      update t_auditee_entities_maping_com cc
         set cc.entity_id     = ENT_ID,
             cc.approver_ppno = AP_P_NO,
             cc.reviewer_ppno = RE_P_NO,
             cc.com_key       = ENT_ID || AP_P_NO || RE_P_NO
       where cc.com_key = E_COM_KEY;
      commit;
    
      update t_auditee_entities e
         set e.complice_by = ENT_ID || AP_P_NO || RE_P_NO
       where e.complice_by = E_COM_KEY;
      commit;
    
      Open io_cursor for
        select 'Updated' as remarks from dual;
    
    else
      Open io_cursor for
        select 'Entry Already Exists' as remarks from dual;
    end if;
  
  end P_UPDATE_COM_OFFICER;

  Procedure P_ADD_COM_OFFICER(ENT_ID    number,
                              AP_P_NO   number,
                              RE_P_NO   number,
                              io_cursor out t_cursor) as
  
    C_F number;
  begin
    select nvl(max(cb.entity_id), 0)
      into C_F
      from t_auditee_entities_maping_com cb
     where cb.com_key = ENT_ID || AP_P_NO || RE_P_NO;
    if (C_F = 0) then
      insert into t_auditee_entities_maping_com
        (entity_id, approver_ppno, reviewer_ppno, com_key)
      values
        (ENT_ID, AP_P_NO, RE_P_NO, ENT_ID || AP_P_NO || RE_P_NO);
      commit;
      open io_cursor for
        select 'New postions Added, please assign them entities' as remarks
          from dual;
    else
      open io_cursor for
        select 'Entry Already Exists' as remarks from dual;
    end if;
  end P_ADD_COM_OFFICER;

  Procedure P_SHIFTING_AUDIT_PARA(NEW_P_ID    in number,
                                  OLD_P_ID    in number,
                                  P_IND       in varchar2,
                                  DEST_ENT_ID in number,
                                  P_NO        in number,
                                  R_ID        in number,
                                  ENT_ID      in number,
                                  io_cursor   out t_cursor) as
  
  begin
  
    update t_au_observation o
       set o.entity_id = DEST_ENT_ID
     where o.id = NEW_P_ID;
    commit;
  
    update t_au_observation_fad f
       set f.entity_id = DEST_ENT_ID
     where f.new_paraid = NEW_P_ID
       and f.old_para_id = OLD_P_ID
       and f.IND = P_IND;
    commit;
  
    -- THIS QUERY IS NEEDED TO DISCUSS WITH ASAD SB FOR INDICATOR C
    update t_au_old_paras_fad p
       set p.entity_id = DEST_ENT_ID
     where (p.id = NEW_P_ID and P_IND = 'A')
        or (p.id = OLD_P_ID and P_IND = 'O');
    commit;
  
    update t_au_observation_assignedto a
       set a.entity_id = DEST_ENT_ID
     where a.Obs_Id = NEW_P_ID;
    commit;
  
    update ais_t_au_post_compliance c
       set c.entity_id = DEST_ENT_ID
     where c.old_para_id = OLD_P_ID
       and c.New_Para_Id = NEW_P_ID
       and c.ind = P_IND;
    commit;
  
    open io_cursor for
      select 'Para shifting successfuly done' as remarks from dual;
  
  end P_SHIFTING_AUDIT_PARA;

  Procedure P_GET_GM_OFFICE(io_cursor out t_cursor) as
  begin
    open io_cursor for
      select distinct e.name, e.entity_id
        from t_auditee_entities e
       where e.type_id = 21;
  
  end P_GET_GM_OFFICE;

  Procedure P_GET_RPT_OFFICE(io_cursor out t_cursor) as
  begin
    open io_cursor for
      select distinct e.name, e.entity_id
        from t_auditee_entities e
       where e.type_id = 18;
  
  end P_GET_RPT_OFFICE;

  Procedure P_UPDATE_GM_OFFICE_RELATIONSHIP(GM        number,
                                            ENT       number,
                                            io_cursor out t_cursor) as
  begin
    update t_auditee_entities_maping m
       set m.gm_office = gm
     where m.entity_id = ent;
    commit;
    open io_cursor for
      select 'GM Office updated Succesfully' as remarks from dual;
  
  end P_UPDATE_GM_OFFICE_RELATIONSHIP;

  Procedure P_UPDATE_RPT_OFFICE_RELATIONSHIP(RPT       number,
                                             ENT       number,
                                             io_cursor out t_cursor) as
  begin
    update t_auditee_entities_maping m
       set m.reporting = RPT
     where m.entity_id = ent;
    commit;
    open io_cursor for
      select 'Reporting Line updated Succesfully' as remarks from dual;
  
  end P_UPDATE_RPT_OFFICE_RELATIONSHIP;

  Procedure P_get_latest_para_details(ENT number, io_cursor out t_cursor) as
  begin
    open io_cursor for
    
      select ca.com_id         as comid,
             ca.old_para_id    as oldparaid,
             ca.new_para_id    as newparaid,
             ca.audit_period   as auditperiod,
             ca.entity_id      as entityid,
             ca.entity_code    as entitycode,
             ca.audited_by     as auditedby,
             ca.entity_type_id as entitytypeid,
             ca.com_cycle      as comcycle,
             ca.com_status     as comstatus,
             ca.com_stage      as comstage,
             ca.para_status    as parastatus,
             ca.para_no        as parano,
             ca.gist_of_paras  as gistofparas,
             ca.setteled_on    as setteledon,
             ca.setteled_by    as SETTELEDBY,
             ca.ind            as ind,
             ca.risk,
             ca.annex
        from ais_t_au_post_compliance ca
       where ca.entity_id = ENT;
    --and ca.para_status = 8;
  end P_get_latest_para_details;

  Procedure P_Update_para_AIS_post_compliance(ca_com_id         NUMBER,
                                              ca_audit_period   VARCHAR2,
                                              ca_gist_of_paras  VARCHAR2,
                                              ca_audited_by     NUMBER,
                                              ca_entity_type_id NUMBER,
                                              ca_com_cycle      NUMBER,
                                              ca_com_status     NUMBER,
                                              ca_com_stage      NUMBER,
                                              ca_para_status    NUMBER,
                                              ca_para_no        VARCHAR2,
                                              ca_ind            VARCHAR2,
                                              ca_risk           NUMBER,
                                              io_cursor         out t_cursor) as
  begin
  
    update ais_t_au_post_compliance ec
    
       set ec.audit_period   = ca_audit_period,
           ec.audited_by     = ca_audited_by,
           ec.gist_of_paras  = ca_gist_of_paras,
           ec.entity_type_id = ca_entity_type_id,
           ec.com_cycle      = ca_com_cycle,
           ec.com_status     = ca_com_status,
           ec.com_stage      = ca_com_stage,
           ec.para_status    = ca_para_status,
           ec.para_no        = ca_para_no,
           ec.ind            = ca_ind,
           ec.risk           = ca_risk
     where ec.com_id = ca_com_id;
    commit;
    Open io_cursor for
      select 'Details updated' as remarks from dual;
  end P_Update_para_AIS_post_compliance;

  Procedure P_Get_Audit_EMP(P_NO      in number,
                            R_ID      in number,
                            ENT_ID    in number,
                            io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select e.ppno,
             e.ppno as id,
             e.employeefirstname || ' ' || e.employeelastname as Name,
             e.departmentcode,
             e.deptarment as Placement,
             
             e.rankcode,
             e.current_rank as Rank,
             e.designationcode,
             e.fun_designation as designation,
             e.type,
             e.entity_id,
             '' as QUALIFICATION,
             '' as SPECIALIZATION,
             '' as CERTIFICATION,
             '' as TOTAL_EXPERIENCE,
             '' as AUDIT_EXPERIENCE
      
        from t_audit_emp e
       order by e.rankcode
      
      ;
  end P_Get_Audit_EMP;

  procedure P_get_hr_rank(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select r.id, r.description
        from v_services__hrms_hr_rank r
      
      ;
  end P_get_hr_rank;

  Procedure P_get_certification(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select *
        from v_services__hrms_hr_rank r
      
      ;
  end P_get_certification;

  Procedure P_get_hr_designation(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select *
        from v_services__hrms_hr_designations
      
      ;
  end P_get_hr_designation;

  Procedure P_get_qualification(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select *
        from v_services__hrms_hr_qualifications
      
      ;
  end P_get_qualification;

  Procedure P_get_qualification_specialization(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select * from v_services__hrms_hr_qualifications;
  
  end P_get_qualification_specialization;

  Procedure P_get_hr_posting(io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select e.entity_id as id, e.description
        from t_auditee_entities e
       where e.auditor = 'Y';
  
  end P_get_hr_posting;

  Procedure P_Get_Audit_Manpower(P_NO      in number,
                                 R_ID      in number, /*
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          ENT_ID      in number,*/
                                 io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select 1 as id,
             'ZTBL' as COMPANY,
             e.current_rank as Rank,
             e.deptarment as PLACEMENT,
             count(e.ppno) as EXISTING,
             '' as ADDITIONAL_REQUIRED
      
        from t_audit_emp e
       group by e.current_rank, e.deptarment;
  end P_Get_Audit_Manpower;

  PROCEDURE P_GET_PUBLIC_HOLIDAYS(p_year    IN NUMBER DEFAULT NULL,
                                  io_cursor OUT SYS_REFCURSOR) AS
  BEGIN
    IF p_year IS NULL THEN
      OPEN io_cursor FOR
        SELECT * FROM tbl_PUBLIC_HOLIDAYS ORDER BY HOLIDAY_DATE;
    ELSE
      OPEN io_cursor FOR
        SELECT *
          FROM tbl_PUBLIC_HOLIDAYS
        --WHERE HOLIDAY_YEAR = p_year
         ORDER BY HOLIDAY_DATE;
    END IF;
  END P_GET_PUBLIC_HOLIDAYS;

  PROCEDURE P_GET_PUBLIC_HOLIDAY_DAY(p_day     IN date,
                                     io_cursor OUT SYS_REFCURSOR) AS
  BEGIN
  
    OPEN io_cursor FOR
      SELECT NVL(d.is_holiday, 'N') as holiday,
             NVL(d.is_weekend, 'N') as weekend
        FROM tbl_PUBLIC_HOLIDAYS d
       WHERE d.holiday_date = p_day;
  
  END P_GET_PUBLIC_HOLIDAY_DAY;

  PROCEDURE P_INSERT_PUBLIC_HOLIDAY(p_holiday_date IN DATE,
                                    p_is_weekend   IN CHAR DEFAULT 'N',
                                    p_is_holiday   IN CHAR DEFAULT 'N',
                                    p_holiday_name IN VARCHAR2 DEFAULT NULL,
                                    p_id           IN NUMBER DEFAULT NULL) AS
    v_year  NUMBER(4);
    v_count NUMBER := 0;
  BEGIN
    -- Get year from date
    v_year := TO_NUMBER(TO_CHAR(p_holiday_date, 'YYYY'));
  
    IF p_id IS NOT NULL THEN
      SELECT COUNT(*)
        INTO v_count
        FROM tbl_PUBLIC_HOLIDAYS
       WHERE HOLIDAY_DATE = p_holiday_date
         AND ID <> p_id;
    
      IF v_count = 0 THEN
        UPDATE tbl_PUBLIC_HOLIDAYS
           SET HOLIDAY_DATE = p_holiday_date,
               HOLIDAY_YEAR = v_year,
               IS_WEEKEND   = p_is_weekend,
               IS_HOLIDAY   = p_is_holiday,
               HOLIDAY_NAME = p_holiday_name
         WHERE ID = p_id;
      END IF;
    
      RETURN;
    END IF;
  
    -- Check for duplicate entry
    SELECT COUNT(*)
      INTO v_count
      FROM tbl_PUBLIC_HOLIDAYS
     WHERE HOLIDAY_DATE = p_holiday_date;
  
    IF v_count = 0 THEN
      INSERT INTO tbl_PUBLIC_HOLIDAYS
        (HOLIDAY_DATE, HOLIDAY_YEAR, IS_WEEKEND, IS_HOLIDAY, HOLIDAY_NAME)
      VALUES
        (p_holiday_date,
         v_year,
         p_is_weekend,
         p_is_holiday,
         p_holiday_name);
    END IF;
  END P_INSERT_PUBLIC_HOLIDAY;

  PROCEDURE P_GET_VERSION_HISTORY(io_cursor OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT VERSION_ID,
             VERSION_NO,
             RELEASE_DATE,
             DESCRIPTION,
             RELEASED_BY,
             CREATED_ON,
             UPDATED_ON,
             IS_ACTIVE
        FROM T_IAS_VERSION_HISTORY
       ORDER BY RELEASE_DATE DESC;
  END;

  -- ADD a new version
  PROCEDURE P_ADD_VERSION_HISTORY(i_version_no   IN VARCHAR2,
                                  i_release_date IN DATE,
                                  i_description  IN VARCHAR2,
                                  i_released_by  IN VARCHAR2,
                                  o_result       OUT VARCHAR2) IS
  BEGIN
    INSERT INTO T_IAS_VERSION_HISTORY
      (VERSION_NO, RELEASE_DATE, DESCRIPTION, RELEASED_BY)
    VALUES
      (i_version_no, i_release_date, i_description, i_released_by);
    o_result := 'SUCCESS';
  EXCEPTION
    WHEN OTHERS THEN
      o_result := 'ERROR: ' || SQLERRM;
  END;

  -- UPDATE an existing version
  PROCEDURE P_UPDATE_VERSION_HISTORY(i_version_id   IN NUMBER,
                                     i_version_no   IN VARCHAR2,
                                     i_release_date IN DATE,
                                     i_description  IN VARCHAR2,
                                     i_released_by  IN VARCHAR2,
                                     i_is_active    IN CHAR,
                                     o_result       OUT VARCHAR2) IS
  BEGIN
    UPDATE T_IAS_VERSION_HISTORY
       SET VERSION_NO   = i_version_no,
           RELEASE_DATE = i_release_date,
           DESCRIPTION  = i_description,
           RELEASED_BY  = i_released_by,
           IS_ACTIVE    = i_is_active,
           UPDATED_ON   = SYSDATE
     WHERE VERSION_ID = i_version_id;
  
    IF SQL%ROWCOUNT > 0 THEN
      o_result := 'SUCCESS';
    ELSE
      o_result := 'NO_RECORD_UPDATED';
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      o_result := 'ERROR: ' || SQLERRM;
  END;

  PROCEDURE P_GET_ROLE_DASHBOARD_PAGES(p_role_id IN NUMBER,
                                       O_CURSOR  OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT mp.ID AS PAGE_ID,
             mp.PAGE_NAME,
             mp.PAGE_URL,
             mp.PAGE_PATH,
             mp.PAGE_ORDER,
             rdp.DASHBOARD_ORDER
        FROM T_ROLE_DASHBOARD_PAGES rdp
        JOIN T_MENU_PAGES mp
          ON mp.ID = rdp.PAGE_ID
       WHERE rdp.ROLE_ID = p_role_id
         AND rdp.IS_ACTIVE = 'Y'
         AND mp.STATUS = 'Y'
       ORDER BY rdp.DASHBOARD_ORDER;
  END P_GET_ROLE_DASHBOARD_PAGES;

  PROCEDURE P_MAINT_ROLE_DASHBOARD_PAGE(P_ROLE_ID         IN NUMBER,
                                        P_PAGE_ID         IN NUMBER,
                                        P_DASHBOARD_ORDER IN NUMBER,
                                        P_IS_ACTIVE       IN VARCHAR2,
                                        P_ACTION_IND      IN VARCHAR2,
                                        O_MESSAGE         OUT VARCHAR2) IS
  BEGIN
    IF P_ACTION_IND = 'A' THEN
      INSERT INTO T_ROLE_DASHBOARD_PAGES
        (ROLE_ID, PAGE_ID, DASHBOARD_ORDER, IS_ACTIVE, CREATED_ON)
      VALUES
        (P_ROLE_ID, P_PAGE_ID, P_DASHBOARD_ORDER, P_IS_ACTIVE, SYSDATE);
    
      O_MESSAGE := 'Dashboard page added successfully';
    
    ELSIF P_ACTION_IND = 'U' THEN
      UPDATE T_ROLE_DASHBOARD_PAGES
         SET DASHBOARD_ORDER = P_DASHBOARD_ORDER,
             IS_ACTIVE       = P_IS_ACTIVE,
             UPDATED_ON      = SYSDATE
       WHERE ROLE_ID = P_ROLE_ID
         AND PAGE_ID = P_PAGE_ID;
    
      O_MESSAGE := 'Dashboard page updated successfully';
    
    ELSIF P_ACTION_IND = 'D' THEN
      UPDATE T_ROLE_DASHBOARD_PAGES
         SET IS_ACTIVE = 'N', UPDATED_ON = SYSDATE
       WHERE ROLE_ID = P_ROLE_ID
         AND PAGE_ID = P_PAGE_ID;
    
      O_MESSAGE := 'Dashboard page disabled successfully';
    
    ELSE
      O_MESSAGE := 'Invalid ACTION_IND supplied';
    END IF;
  
    COMMIT;
  
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      O_MESSAGE := 'Error: ' || SQLERRM;
  END P_MAINT_ROLE_DASHBOARD_PAGE;

  PROCEDURE P_GET_ROLE_DASHBOARD_CONFIG(p_role_id IN NUMBER,
                                        O_CURSOR  OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT rdp.ROLE_ID,
             rdp.PAGE_ID,
             mp.PAGE_NAME,
             mp.PAGE_URL,
             rdp.DASHBOARD_ORDER,
             rdp.IS_ACTIVE
        FROM T_ROLE_DASHBOARD_PAGES rdp
        JOIN T_MENU_PAGES mp
          ON mp.ID = rdp.PAGE_ID
       WHERE rdp.ROLE_ID = p_role_id
       ORDER BY rdp.DASHBOARD_ORDER;
  END P_GET_ROLE_DASHBOARD_CONFIG;

  PROCEDURE P_GET_API_MASTER(O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT API_ID,
             PAGE_ID,
             action_name as VIEW_NAME,
             CONTROLLER_NAME,
             API_PATH,
             HTTP_METHOD,
             IS_ACTIVE
        FROM T_AU_API_MASTER
       ORDER BY API_PATH;
  END;

  PROCEDURE P_MAINT_API_MASTER(P_API_ID          IN NUMBER,
                               P_API_NAME        IN VARCHAR2,
                               P_CONTROLLER_NAME IN VARCHAR2,
                               P_API_ROUTE       IN VARCHAR2,
                               P_HTTP_METHOD     IN VARCHAR2,
                               P_PAGE_ID         in NUMBER,
                               P_STATUS          IN VARCHAR2,
                               P_ACTION_IND      IN VARCHAR2,
                               O_MESSAGE         OUT VARCHAR2) IS
  BEGIN
    IF P_ACTION_IND = 'A' THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME,
         PAGE_ID,
         CONTROLLER_NAME,
         API_PATH,
         HTTP_METHOD,
         IS_ACTIVE,
         CREATED_ON)
      VALUES
        (P_API_NAME,
         P_PAGE_ID,
         P_CONTROLLER_NAME,
         P_API_ROUTE,
         P_HTTP_METHOD,
         P_STATUS,
         SYSDATE);
    
      O_MESSAGE := 'API added successfully';
    
    ELSIF P_ACTION_IND = 'U' THEN
      UPDATE T_AU_API_MASTER m
         SET m.ACTION_NAME     = P_API_NAME,
             m.page_id         = P_PAGE_ID,
             m.CONTROLLER_NAME = P_CONTROLLER_NAME,
             m.API_PATH        = P_API_ROUTE,
             m.HTTP_METHOD     = UPPER(P_HTTP_METHOD),
             m.IS_ACTIVE       = P_STATUS,
             m.UPDATED_ON      = SYSDATE
       WHERE m.API_ID = P_API_ID;
    
      O_MESSAGE := 'API updated successfully';
    
    ELSIF P_ACTION_IND = 'D' THEN
      UPDATE T_AU_API_MASTER m
         SET m.IS_ACTIVE = 'N', m.UPDATED_ON = SYSDATE
       WHERE m.API_ID = P_API_ID;
    
      O_MESSAGE := 'API disabled successfully';
    
    ELSE
      O_MESSAGE := 'Invalid ACTION_IND';
    END IF;
  
    COMMIT;
  
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      O_MESSAGE := 'Error: ' || SQLERRM;
  END;
  PROCEDURE P_CHECK_API_UNIQUE(P_PAGE_ID     IN NUMBER,
                               P_API_PATH    IN VARCHAR2,
                               P_HTTP_METHOD IN VARCHAR2,
                               O_EXISTS      OUT NUMBER) IS
  BEGIN
    SELECT COUNT(1)
      INTO O_EXISTS
      FROM T_AU_API_MASTER
     WHERE UPPER(API_PATH) = UPPER(P_API_PATH)
       AND UPPER(HTTP_METHOD) = UPPER(P_HTTP_METHOD)
       AND PAGE_ID = P_PAGE_ID;
  END P_CHECK_API_UNIQUE;

  PROCEDURE P_INSERT_API_MASTER(P_API_NAME    IN VARCHAR2,
                                P_API_PATH    IN VARCHAR2,
                                P_HTTP_METHOD IN VARCHAR2,
                                P_IS_ACTIVE   IN VARCHAR2,
                                O_MESSAGE     OUT VARCHAR2) IS
  BEGIN
    INSERT INTO T_AU_API_MASTER
      (VIEW_NAME, API_PATH, HTTP_METHOD, IS_ACTIVE, CREATED_ON)
    VALUES
      (P_API_NAME,
       UPPER(P_API_PATH),
       UPPER(P_HTTP_METHOD),
       P_IS_ACTIVE,
       SYSDATE);
  
    O_MESSAGE := 'API inserted successfully';
  
  EXCEPTION
    WHEN OTHERS THEN
      O_MESSAGE := 'Error inserting API: ' || SQLERRM;
  END P_INSERT_API_MASTER;

  PROCEDURE P_UPDATE_API_MASTER(P_API_ID      IN NUMBER,
                                P_API_NAME    IN VARCHAR2,
                                P_API_PATH    IN VARCHAR2,
                                P_HTTP_METHOD IN VARCHAR2,
                                P_IS_ACTIVE   IN VARCHAR2,
                                O_MESSAGE     OUT VARCHAR2) IS
  BEGIN
    UPDATE T_AU_API_MASTER
       SET VIEW_NAME   = P_API_NAME,
           API_PATH    = UPPER(P_API_PATH),
           HTTP_METHOD = UPPER(P_HTTP_METHOD),
           IS_ACTIVE   = P_IS_ACTIVE,
           UPDATED_ON  = SYSDATE
     WHERE API_ID = P_API_ID;
  
    IF SQL%ROWCOUNT = 0 THEN
      O_MESSAGE := 'No API record found to update';
    ELSE
      O_MESSAGE := 'API updated successfully';
    END IF;
  
  EXCEPTION
    WHEN OTHERS THEN
      O_MESSAGE := 'Error updating API: ' || SQLERRM;
  END P_UPDATE_API_MASTER;

  PROCEDURE P_MAINT_API_MASTER(P_API_ID      IN NUMBER,
                               P_API_NAME    IN VARCHAR2,
                               P_API_PATH    IN VARCHAR2,
                               P_HTTP_METHOD IN VARCHAR2,
                               P_IS_ACTIVE   IN VARCHAR2,
                               P_ACTION_IND  IN VARCHAR2,
                               O_MESSAGE     OUT VARCHAR2) IS
  BEGIN
    IF P_ACTION_IND = 'A' THEN
      INSERT INTO T_AU_API_MASTER
        (VIEW_NAME, API_PATH, HTTP_METHOD, IS_ACTIVE, CREATED_ON)
      VALUES
        (P_API_NAME,
         UPPER(P_API_PATH),
         UPPER(P_HTTP_METHOD),
         P_IS_ACTIVE,
         SYSDATE);
    
      O_MESSAGE := 'API added successfully';
    
    ELSIF P_ACTION_IND = 'U' THEN
      UPDATE T_AU_API_MASTER
         SET VIEW_NAME   = P_API_NAME,
             API_PATH    = UPPER(P_API_PATH),
             HTTP_METHOD = UPPER(P_HTTP_METHOD),
             IS_ACTIVE   = P_IS_ACTIVE,
             UPDATED_ON  = SYSDATE
       WHERE API_ID = P_API_ID;
    
      O_MESSAGE := 'API updated successfully';
    
    ELSIF P_ACTION_IND = 'D' THEN
      UPDATE T_AU_API_MASTER
         SET IS_ACTIVE = 'N', UPDATED_ON = SYSDATE
       WHERE API_ID = P_API_ID;
    
      O_MESSAGE := 'API disabled successfully';
    
    ELSE
      O_MESSAGE := 'Invalid ACTION_IND';
    END IF;
  
  EXCEPTION
    WHEN OTHERS THEN
      O_MESSAGE := 'Error: ' || SQLERRM;
  END P_MAINT_API_MASTER;

  PROCEDURE P_GET_DASHBOARD_QUICK_LINKS(P_ROLE_ID IN NUMBER,
                                        O_CURSOR  OUT SYS_REFCURSOR) AS
    V_COUNT NUMBER := 0;
  BEGIN
    /*
      Step 1: Check if dashboard layout exists for the role
    */
    SELECT COUNT(1)
      INTO V_COUNT
      FROM T_ROLE_DASHBOARD_PAGES d
     WHERE d.ROLE_ID = P_ROLE_ID
       AND d.IS_ACTIVE = 'Y';
  
    /*
      Step 2: If dashboard layout exists ? use it
    */
    IF V_COUNT > 0 THEN
      OPEN O_CURSOR FOR
        SELECT d.PAGE_ID,
               p.PAGE_NAME,
               p.page_path,
               p.page_path       as page_url,
               d.dashboard_order as DISPLAY_ORDER,
               d.dashboard_order
          FROM T_ROLE_DASHBOARD_PAGES d
          JOIN T_MENU_PAGES p
            ON p.ID = d.PAGE_ID
         WHERE d.ROLE_ID = P_ROLE_ID
           AND d.IS_ACTIVE = 'Y'
           AND p.status = 'A'
         ORDER BY d.dashboard_order;
    
      /*
        Step 3: Else fallback to menu/page order
      */
    ELSE
      OPEN O_CURSOR FOR
        SELECT p.ID         as PAGE_ID,
               p.PAGE_NAME,
               p.PAGE_URL,
               p.PAGE_ORDER AS DISPLAY_ORDER
          FROM T_MENU_PAGES p
          JOIN t_User_Group_Map rm
            ON rm.PAGE_IDS = p.ID
         WHERE rm.ROLE_ID = P_ROLE_ID
           AND rm.status = 'Y'
           AND p.status = 'A7'
         ORDER BY p.PAGE_ORDER;
    END IF;
  
  END P_GET_DASHBOARD_QUICK_LINKS;

  PROCEDURE P_ADD_USER_ENTITY(p_user_id    IN NUMBER,
                              p_entity_id  IN NUMBER,
                              p_role_id    IN NUMBER,
                              p_is_primary IN CHAR DEFAULT 'N',
                              p_created_by IN NUMBER,
                              o_status     OUT NUMBER,
                              o_message    OUT VARCHAR2) AS
    v_count NUMBER;
  BEGIN
    -- Check duplicate
    SELECT COUNT(*)
      INTO v_count
      FROM t_user_entities
     WHERE user_id = p_user_id
       AND entity_id = p_entity_id
       AND role_id = p_role_id
       AND status = 'A';
  
    IF v_count > 0 THEN
      o_status  := 0;
      o_message := 'User already mapped with this entity and role.';
      RETURN;
    END IF;
  
    -- If primary, demote existing primary
    IF p_is_primary = 'Y' THEN
      UPDATE t_user_entities
         SET is_primary = 'N'
       WHERE user_id = p_user_id
         AND status = 'A';
    END IF;
  
    INSERT INTO t_user_entities
      (user_id,
       entity_id,
       role_id,
       is_primary,
       status,
       created_on,
       created_by)
    VALUES
      (p_user_id,
       p_entity_id,
       p_role_id,
       p_is_primary,
       'A',
       SYSDATE,
       p_created_by);
  
    o_status  := 1;
    o_message := 'User entity mapping added successfully.';
  
  EXCEPTION
    WHEN OTHERS THEN
      o_status  := -1;
      o_message := SQLERRM;
  END;

  PROCEDURE P_UPDATE_USER_ENTITY(p_id         IN NUMBER,
                                 p_entity_id  IN NUMBER,
                                 p_role_id    IN NUMBER,
                                 p_is_primary IN CHAR,
                                 p_status     IN CHAR,
                                 p_updated_by IN NUMBER,
                                 o_status     OUT NUMBER,
                                 o_message    OUT VARCHAR2) AS
    v_user_id NUMBER;
  BEGIN
    SELECT user_id INTO v_user_id FROM t_user_entities WHERE id = p_id;
  
    -- If making primary, demote others
    IF p_is_primary = 'Y' THEN
      UPDATE t_user_entities
         SET is_primary = 'N'
       WHERE user_id = v_user_id
         AND status = 'A';
    END IF;
  
    UPDATE t_user_entities
       SET entity_id  = p_entity_id,
           role_id    = p_role_id,
           is_primary = p_is_primary,
           status     = p_status,
           updated_on = SYSDATE,
           updated_by = p_updated_by
     WHERE id = p_id;
  
    o_status  := 1;
    o_message := 'User entity mapping updated successfully.';
  
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      o_status  := 0;
      o_message := 'Mapping record not found.';
    WHEN OTHERS THEN
      o_status  := -1;
      o_message := SQLERRM;
  END;

  PROCEDURE P_DELETE_USER_ENTITY(p_id         IN NUMBER,
                                 p_deleted_by IN NUMBER,
                                 o_status     OUT NUMBER,
                                 o_message    OUT VARCHAR2) AS
  BEGIN
    UPDATE t_user_entities
       SET status     = 'I',
           updated_on = SYSDATE,
           updated_by = p_deleted_by,
           is_primary = 'N'
     WHERE id = p_id;
  
    IF SQL%ROWCOUNT = 0 THEN
      o_status  := 0;
      o_message := 'Mapping record not found.';
      RETURN;
    END IF;
  
    o_status  := 1;
    o_message := 'User entity mapping deactivated successfully.';
  
  EXCEPTION
    WHEN OTHERS THEN
      o_status  := -1;
      o_message := SQLERRM;
  END;

  PROCEDURE P_GET_USER_ENTITIES(p_user_id IN NUMBER,
                                io_cursor OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN io_cursor FOR
      SELECT ue.id,
             ue.entity_id,
             e.name,
             ue.role_id,
             r.group_name as role_name,
             ue.is_primary,
             ue.status
        FROM t_user_entities ue
        JOIN t_auditee_entities e
          ON e.entity_id = ue.entity_id
        JOIN t_groups r
          ON r.role_id = ue.role_id
       WHERE ue.user_id = 113092
            --p_user_id
         AND ue.status = 'A'
       ORDER BY ue.is_primary DESC, e.name;
  END;

  Procedure P_GET_ALL_CONTROLLER(O_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN O_CURSOR FOR
    
      select distinct m.controller_name from t_Au_Api_Master m;
  end P_GET_ALL_CONTROLLER;

  PROCEDURE P_GET_HEAD_OBS_RISK_SUMMARY(P_ROLE_ID      IN NUMBER,
                                        P_ENT_ID       IN NUMBER,
                                        P_CYCLE_BUCKET IN VARCHAR2,
                                        IO_CURSOR      OUT SYS_REFCURSOR) AS
    V_BUCKET VARCHAR2(20) := UPPER(TRIM(P_CYCLE_BUCKET));
  BEGIN
    IF P_ENT_ID IS NULL OR P_ENT_ID <= 0 OR P_ROLE_ID NOT IN (1, 3, 14) OR
       V_BUCKET IS NULL OR V_BUCKET NOT IN ('OVER_THREE', 'ZERO') THEN
      OPEN IO_CURSOR FOR
        SELECT CAST(NULL AS NUMBER) AS department_id,
               CAST(NULL AS VARCHAR2(100)) AS department_name,
               0 AS total_observations,
               0 AS high_risk,
               0 AS medium_risk,
               0 AS low_risk,
               0 AS unrated_risk,
               CAST(NULL AS VARCHAR2(10)) AS risk_status
          FROM dual
         WHERE 1 = 0;
      RETURN;
    END IF;
  
    OPEN IO_CURSOR FOR
      SELECT department_id,
             department_name,
             COUNT(*) AS total_observations,
             SUM(CASE
                   WHEN risk = 1 THEN
                    1
                   ELSE
                    0
                 END) AS high_risk,
             SUM(CASE
                   WHEN risk = 2 THEN
                    1
                   ELSE
                    0
                 END) AS medium_risk,
             SUM(CASE
                   WHEN risk = 3 THEN
                    1
                   ELSE
                    0
                 END) AS low_risk,
             SUM(CASE
                   WHEN risk IS NULL OR risk NOT IN (1, 2, 3) THEN
                    1
                   ELSE
                    0
                 END) AS unrated_risk,
             CASE
               WHEN SUM(CASE
                          WHEN risk = 1 THEN
                           1
                          ELSE
                           0
                        END) > 0 THEN
                'High'
               WHEN SUM(CASE
                          WHEN risk = 2 THEN
                           1
                          ELSE
                           0
                        END) > 0 THEN
                'Medium'
               WHEN SUM(CASE
                          WHEN risk = 3 THEN
                           1
                          ELSE
                           0
                        END) > 0 THEN
                'Low'
               ELSE
                'Unrated'
             END AS risk_status
        FROM V_DASH_HEAD_OBS_RISK_BASE b
       WHERE ((V_BUCKET = 'OVER_THREE' AND com_cycle > 3) OR
             (V_BUCKET = 'ZERO' AND com_cycle = 0))
         AND (department_id = P_ENT_ID OR EXISTS
              (SELECT 1
                 FROM T_AUDITEE_ENTITIES_MAPING m
                WHERE m.entity_id = b.department_id
                  AND NVL(m.status, 'Y') = 'Y'
                  AND (m.parent_id = P_ENT_ID OR m.reporting = P_ENT_ID OR
                      m.gm_office = P_ENT_ID OR m.div_office = P_ENT_ID OR
                      m.b_group = P_ENT_ID)))
       GROUP BY department_id, department_name
       ORDER BY department_name;
  
  END P_GET_HEAD_OBS_RISK_SUMMARY;

  Procedure P_Add_Department_Entity_Shifting(Old_Ent_id in number,
                                             new_ent_id in number,
                                             P_NO       in number,
                                             ENT_ID     in number,
                                             R_ID       in number,
                                             cir_no     in varchar2,
                                             cir_attach in clob,
                                             cir_date   in date,
                                             io_cursor  OUT t_cursor) as
    V_OLD_NAME varchar2(1000);
    V_NEW_NAME varchar2(1000);
    V_OLD_CODE number;
    V_NEW_CODE number;
    V_OLD_TYPE number;
    V_NEW_TYPE number;
    V_EXISTS   number := 0;
  begin
    if Old_Ent_id is null or new_ent_id is null or Old_Ent_id = new_ent_id then
      open io_cursor for
        select 'Old and new entities must be different valid entities.' as remarks
          from dual;
      return;
    end if;
  
    select e.name, e.code, e.type_id
      into V_OLD_NAME, V_OLD_CODE, V_OLD_TYPE
      from t_auditee_entities e
     where e.entity_id = Old_Ent_id;
  
    select e.name, e.code, e.type_id
      into V_NEW_NAME, V_NEW_CODE, V_NEW_TYPE
      from t_auditee_entities e
     where e.entity_id = new_ent_id;
  
    if V_OLD_TYPE = 6 or V_NEW_TYPE = 6 then
      open io_cursor for
        select 'Branch shifting must use the existing branch shifting process.' as remarks
          from dual;
      return;
    end if;
  
    if V_OLD_TYPE <> V_NEW_TYPE then
      open io_cursor for
        select 'Department/entity shifting requires matching entity types.' as remarks
          from dual;
      return;
    end if;
  
    select count(*)
      into V_EXISTS
      from t_au_entity_shifting s
     where s.old_entity_id = Old_Ent_id
       and s.new_entity_id = new_ent_id;
  
    if V_EXISTS > 0 then
      open io_cursor for
        select V_OLD_NAME || ' to ' || V_NEW_NAME ||
               ' request already entered.' as remarks
          from dual;
      return;
    end if;
  
    insert into t_au_entity_shifting
      (ref_id,
       old_entity_id,
       new_entity_id,
       circular_no,
       circular_date,
       circular,
       entered_by,
       entered_on)
    values
      ((select COALESCE(max(s.ref_id) + 1, 1) from t_au_entity_shifting s),
       Old_Ent_id,
       new_ent_id,
       cir_no,
       cir_date,
       cir_attach,
       P_NO,
       sysdate);
  
    insert into t_au_observation_shifting
      (id,
       old_entity_id,
       new_entity_id,
       old_para_id,
       new_para_id,
       shifting_date,
       para_status,
       annex)
      select (select COALESCE(max(s.id), 0) from t_au_observation_shifting s) +
             row_number() over (order by f.com_id),
             Old_Ent_id,
             new_ent_id,
             f.old_para_id,
             f.new_para_id,
             sysdate,
             f.para_status,
             null
        from AIS_T_AU_POST_COMPLIANCE f
       where f.entity_id = Old_Ent_id
         and f.para_status = 8;
  
    update t_au_observation o
       set o.entity_id = new_ent_id, o.entity_code = V_NEW_CODE
     where o.entity_id = Old_Ent_id
       and o.status = 8;
  
    update t_au_observation_assignedto a
       set a.entity_id = new_ent_id
     where a.entity_id = Old_Ent_id;
  
    update t_au_old_paras_fad f
       set f.entity_id   = new_ent_id,
           f.entity_code = V_NEW_CODE,
           f.entity_name = V_NEW_NAME
     where f.entity_id = Old_Ent_id
       and f.para_status = 8;
  
    update t_au_observation_old_cad_paras c
       set c.entity_id = new_ent_id, c.entity_name = V_NEW_NAME
     where c.entity_id = Old_Ent_id
       and c.para_status = 8;
  
    update AIS_T_AU_POST_COMPLIANCE c
       set c.entity_id      = new_ent_id,
           c.entity_code    = V_NEW_CODE,
           c.entity_type_id = V_NEW_TYPE
     where c.entity_id = Old_Ent_id
       and c.para_status = 8;
  
    update t_auditee_entities_size s
       set s.entity_id = new_ent_id, s.entity_code = V_NEW_CODE
     where s.entity_id = Old_Ent_id;
  
    update t_auditee_entities_risk r
       set r.entity_id = new_ent_id, r.entity_code = V_NEW_CODE
     where r.entity_id = Old_Ent_id;
  
    update t_auditee_entities_maping m
       set m.parent_id   = new_ent_id,
           m.parent_code = V_NEW_CODE,
           m.p_name      = V_NEW_NAME,
           m.p_type_id   = V_NEW_TYPE
     where m.parent_id = Old_Ent_id;
  
    update t_auditee_entities_maping m
       set m.entity_id  = new_ent_id,
           m.child_code = V_NEW_CODE,
           m.c_name     = V_NEW_NAME,
           m.c_type_id  = V_NEW_TYPE,
           m.r_key      = m.parent_id || new_ent_id
     where m.entity_id = Old_Ent_id;
  
    update t_auditee_entities_maping m
       set m.reporting = case
                           when m.reporting = Old_Ent_id then
                            new_ent_id
                           else
                            m.reporting
                         end,
           m.gm_office = case
                           when m.gm_office = Old_Ent_id then
                            new_ent_id
                           else
                            m.gm_office
                         end,
           m.div_office = case
                            when m.div_office = Old_Ent_id then
                             new_ent_id
                            else
                             m.div_office
                          end,
           m.b_group = case
                         when m.b_group = Old_Ent_id then
                          new_ent_id
                         else
                          m.b_group
                       end
     where m.reporting = Old_Ent_id
        or m.gm_office = Old_Ent_id
        or m.div_office = Old_Ent_id
        or m.b_group = Old_Ent_id;
  
    update t_auditee_entities_maping_reporting m
       set m.parent_id   = new_ent_id,
           m.parent_code = V_NEW_CODE,
           m.p_name      = V_NEW_NAME,
           m.p_type_id   = V_NEW_TYPE
     where m.parent_id = Old_Ent_id;
  
    update t_auditee_entities_maping_reporting m
       set m.entity_id  = new_ent_id,
           m.child_code = V_NEW_CODE,
           m.c_name     = V_NEW_NAME,
           m.c_type_id  = V_NEW_TYPE
     where m.entity_id = Old_Ent_id;
  
    update t_auditee_entities e
       set e.auditable = 'N', e.active = 'N'
     where e.entity_id = Old_Ent_id;
  
    commit;
    open io_cursor for
      select V_OLD_NAME || ' has been shifted to ' || V_NEW_NAME ||
             ' successfully.' as remarks
        from dual;
  exception
    when no_data_found then
      rollback;
      open io_cursor for
        select 'Old or new entity could not be found.' as remarks
          from dual;
    when others then
      rollback;
      raise;
  end P_Add_Department_Entity_Shifting;

  Procedure P_GET_HEAD_OBS_RISK_DETAILS(P_ROLE_ID       IN NUMBER,
                                        P_ENT_ID        IN NUMBER,
                                        P_DEPARTMENT_ID IN NUMBER,
                                        P_CYCLE_BUCKET  IN VARCHAR2,
                                        IO_CURSOR       OUT SYS_REFCURSOR) AS
    V_BUCKET VARCHAR2(20) := UPPER(TRIM(P_CYCLE_BUCKET));
  BEGIN
    IF P_ENT_ID IS NULL OR P_ENT_ID <= 0 OR P_DEPARTMENT_ID IS NULL OR
       P_DEPARTMENT_ID <= 0 OR P_ROLE_ID NOT IN (1, 3, 14) OR
       V_BUCKET NOT IN ('OVER_THREE', 'ZERO') THEN
      OPEN IO_CURSOR FOR
        SELECT CAST(NULL AS NUMBER) AS com_id,
               CAST(NULL AS VARCHAR2(50)) AS audit_period,
               CAST(NULL AS VARCHAR2(100)) AS para_no,
               CAST(NULL AS VARCHAR2(4000)) AS gist_of_paras,
               CAST(NULL AS VARCHAR2(20)) AS risk
          FROM dual
         WHERE 1 = 0;
      RETURN;
    END IF;
  
    OPEN IO_CURSOR FOR
      SELECT c.com_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             CASE c.risk
               WHEN 1 THEN
                'High'
               WHEN 2 THEN
                'Medium'
               WHEN 3 THEN
                'Low'
               ELSE
                'Unrated'
             END AS risk
        FROM AIS_T_AU_POST_COMPLIANCE c
       WHERE c.entity_id = P_DEPARTMENT_ID
         AND ((V_BUCKET = 'OVER_THREE' AND NVL(c.com_cycle, 0) > 3) OR
             (V_BUCKET = 'ZERO' AND NVL(c.com_cycle, 0) = 0))
         AND (P_DEPARTMENT_ID = P_ENT_ID OR EXISTS
              (SELECT 1
                 FROM T_AUDITEE_ENTITIES_MAPING m
                WHERE m.entity_id = P_DEPARTMENT_ID
                  AND NVL(m.status, 'Y') = 'Y'
                  AND (m.parent_id = P_ENT_ID OR m.reporting = P_ENT_ID OR
                      m.gm_office = P_ENT_ID OR m.div_office = P_ENT_ID OR
                      m.b_group = P_ENT_ID)))
       ORDER BY c.audit_period DESC, c.para_no, c.com_id;
  END P_GET_HEAD_OBS_RISK_DETAILS;

  PROCEDURE P_Get_Entity_Shifting_List(Io_Cursor OUT T_Cursor) AS
  BEGIN
  
    OPEN Io_Cursor FOR
      SELECT T.Ref_Id,
             
             O.Entity_Id   AS Old_Ent_Id,
             O.Code        AS Old_Ent_Code,
             O.Description AS Old_Entity,
             
             N.Entity_Id   AS New_Ent_Id,
             N.Code        AS New_Ent_Code,
             N.Description AS New_Entity,
             
             T.Circular_No,
             T.Circular_Date,
             T.Entered_By,
             T.Entered_On
      
        FROM T_Au_Entity_Shifting T
      
       INNER JOIN T_Auditee_Entities O
          ON O.Entity_Id = T.Old_Entity_Id
      
       INNER JOIN T_Auditee_Entities N
          ON N.Entity_Id = T.New_Entity_Id
      
       ORDER BY T.Entered_On DESC, T.Ref_Id DESC;
  
  END P_Get_Entity_Shifting_List;

  PROCEDURE P_Get_Entity_Shifting_Paras(P_Ref_Id  IN NUMBER,
                                        Io_Cursor OUT T_Cursor) AS
  BEGIN
  
    IF P_Ref_Id IS NULL OR P_Ref_Id <= 0 THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'A valid shifting reference ID is required.');
    END IF;
  
    OPEN Io_Cursor FOR
      SELECT C.Audit_Period,
             C.Para_No,
             C.Gist_Of_Paras,
             
             CASE
               WHEN T.Para_Status = 8 THEN
                'Open'
               ELSE
                'Closed'
             END AS Para_Status,
             
             a.code as Annex
      
        FROM T_Au_Observation_Shifting T
      
       INNER JOIN Ais_T_Au_Post_Compliance C
          ON C.Entity_Id = T.New_Entity_Id
         AND (C.Old_Para_Id = T.Old_Para_Id OR
             C.New_Para_Id = T.New_Para_Id)
         left join t_audit_checklist_annexure a
           on t.annex = a.id
      
       WHERE T.Shift_Ref_Id = P_Ref_Id
       
      
       ORDER BY C.Audit_Period, C.Para_No;
  
  END P_Get_Entity_Shifting_Paras;

end PKG_AD;

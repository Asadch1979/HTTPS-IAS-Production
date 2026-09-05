create or replace package PKG_LG is

  TYPE t_cursor IS REF CURSOR;

  procedure p_get_user(PPNumber  in t_user.ppno%type,
                       enc_pass  in t_user.password%type,
                       io_cursor OUT t_cursor);

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
                         UserPostingAuditZone IN T_USER_SESSION.POSTING_AZ%type,
                         ENT_ID               in number);

  procedure P_GetLoggedInUserEngId(PPNumber  IN NUMBER,
                                   io_cursor OUT t_cursor);

  procedure Session_END(PPNumber  in t_user_session.user_pp_number%type,
                        SessionId in t_user_session.session_id%type,
                        ENT_ID    in number,
                        R_ID      in number);

  procedure Session_Kill(PPNumber in t_user_session.user_pp_number%type);

  procedure Session_Kill_day_end;

  procedure P_ChangePassword(PP_NO    in number,
                             enc_pass in t_user.password%type,
                             ENT_ID   in number,
                             P_NO     IN NUMBER,
                             R_ID     IN NUMBER);
  procedure p_get_user_id(PPNumber  in t_user.ppno%type,
                          io_cursor OUT t_cursor);

  procedure p_get_user_session(PPNumber  in t_user.ppno%type,
                               io_cursor OUT t_cursor);

  procedure p_GetTopMenus(UserRoleID in t_user_group_map.role_id%type,
                          ENT_ID     in number,
                          P_NO       in number,
                          R_ID       in number,
                          io_cursor  OUT t_cursor);

  procedure p_GetTopMenuPages(UserGroupID in t_menu_pages_groupmap.group_id%type,
                              ENT_ID      in number,
                              P_NO        in number,
                              R_ID        in number,
                              io_cursor   OUT t_cursor);

  Procedure p_GetApiPermissions(ENT_ID    in number,
                                P_NO      in number,
                                R_ID      in number,
                                io_cursor OUT t_cursor);
                                
  PROCEDURE P_GET_ENG_PAGE_PERMISSIONS_BY_PPNO(P_PP_NO   IN NUMBER,
                                               io_cursor OUT t_cursor);                                
                                
  PROCEDURE P_GET_COM_PAGE_PERMISSIONS_BY_PPNO(P_PP_NO   IN NUMBER,
                                               io_cursor OUT t_cursor);                                

  procedure P_GetRiskProcessDefinition(io_cursor OUT t_cursor);

  procedure p_get_emp_name(PP_NO in number, io_cursor OUT t_cursor);

  PROCEDURE LOG_INFO(p_module       IN VARCHAR2,
                     p_controller   IN VARCHAR2,
                     p_action       IN VARCHAR2,
                     p_message      IN VARCHAR2,
                     p_tech_details IN CLOB DEFAULT NULL,
                     p_page_id      IN NUMBER DEFAULT NULL,
                     p_eng_id       IN NUMBER DEFAULT NULL,
                     p_user_ppno    IN NUMBER DEFAULT NULL);

  PROCEDURE LOG_WARNING(p_module       IN VARCHAR2,
                        p_controller   IN VARCHAR2,
                        p_action       IN VARCHAR2,
                        p_message      IN VARCHAR2,
                        p_tech_details IN CLOB DEFAULT NULL,
                        p_page_id      IN NUMBER DEFAULT NULL,
                        p_eng_id       IN NUMBER DEFAULT NULL,
                        p_user_ppno    IN NUMBER DEFAULT NULL);

  PROCEDURE LOG_ERROR(p_module       IN VARCHAR2,
                      p_controller   IN VARCHAR2,
                      p_action       IN VARCHAR2,
                      p_message      IN VARCHAR2,
                      p_tech_details IN CLOB DEFAULT NULL,
                      p_page_id      IN NUMBER DEFAULT NULL,
                      p_eng_id       IN NUMBER DEFAULT NULL,
                      p_user_ppno    IN NUMBER DEFAULT NULL);

  PROCEDURE P_GET_SYS_LOGS(p_start_time IN TIMESTAMP DEFAULT NULL,
                           p_end_time   IN TIMESTAMP DEFAULT NULL,
                           p_log_level  IN VARCHAR2 DEFAULT NULL,
                           p_module     IN VARCHAR2 DEFAULT NULL,
                           p_user_ppno  IN NUMBER DEFAULT NULL,
                           p_eng_id     IN NUMBER DEFAULT NULL,
                           io_cursor     OUT SYS_REFCURSOR);

  PROCEDURE REGISTER_SYSTEM_ERROR(P_FINGERPRINT          IN VARCHAR2,
                                  P_ERROR_TYPE           IN VARCHAR2,
                                  P_ERROR_CODE           IN VARCHAR2,
                                  P_MODULE               IN VARCHAR2,
                                  P_CONTROLLER           IN VARCHAR2,
                                  P_ACTION               IN VARCHAR2,
                                  P_API_PATH             IN VARCHAR2,
                                  P_STORED_PROCEDURE     IN VARCHAR2,
                                  P_PPNO                 IN VARCHAR2,
                                  P_ROLE_NAME            IN VARCHAR2,
                                  P_ENTITY_NAME          IN VARCHAR2,
                                  P_PAGE_ID              IN NUMBER,
                                  P_ENG_ID               IN NUMBER,
                                  P_PARA_ID              IN NUMBER,
                                  P_COM_ID               IN NUMBER,
                                  P_TRACE_ID             IN VARCHAR2,
                                  P_IP_ADDRESS           IN VARCHAR2,
                                  P_USER_AGENT           IN VARCHAR2,
                                  P_ERROR_MESSAGE        IN CLOB,
                                  P_TECHNICAL_DETAILS    IN CLOB,
                                  O_ERROR_ID             OUT NUMBER,
                                  O_ERROR_REFERENCE      OUT VARCHAR2,
                                  O_IS_FIRST_OCCURRENCE  OUT NUMBER,
                                  O_FIRST_OCCURRENCE_UTC OUT TIMESTAMP,
                                  O_LAST_OCCURRENCE_UTC  OUT TIMESTAMP,
                                  O_OCCURRENCE_COUNT     OUT NUMBER,
                                  O_EMAIL_ALREADY_SENT   OUT NUMBER);

  PROCEDURE CLAIM_SYSTEM_ERROR_EMAIL(P_ERROR_ID IN NUMBER, O_CLAIMED OUT NUMBER);

  PROCEDURE MARK_SYSTEM_ERROR_EMAIL(P_ERROR_ID IN NUMBER, P_EMAIL_SENT IN NUMBER);

  PROCEDURE GET_SYSTEM_ERROR_RECIPIENTS(O_CUR OUT SYS_REFCURSOR);

  PROCEDURE GET_SYSTEM_ERRORS(P_STATUS          IN VARCHAR2,
                              P_FROM_DATE       IN TIMESTAMP,
                              P_TO_DATE         IN TIMESTAMP,
                              P_ERROR_REFERENCE IN VARCHAR2,
                              P_MODULE          IN VARCHAR2,
                              P_USER_PPNO       IN VARCHAR2,
                              P_ENTITY          IN VARCHAR2,
                              P_ERROR_TYPE_CODE IN VARCHAR2,
                              O_CUR             OUT SYS_REFCURSOR);

  PROCEDURE GET_SYSTEM_ERROR_DETAIL(P_ERROR_ID       IN NUMBER,
                                    O_MASTER         OUT SYS_REFCURSOR,
                                    O_HISTORY        OUT SYS_REFCURSOR,
                                    O_STATUS_HISTORY OUT SYS_REFCURSOR);

  PROCEDURE RESOLVE_SYSTEM_ERROR(P_ERROR_ID         IN NUMBER,
                                 P_RESOLVED_BY_PPNO IN VARCHAR2,
                                 P_REMARKS          IN VARCHAR2);

  PROCEDURE P_LOG_APPLICATION_ACTIVITY(
    P_EVENT_TYPE IN VARCHAR2, P_ACTION_NAME IN VARCHAR2,
    P_ACTION_CATEGORY IN VARCHAR2 DEFAULT NULL, P_MODULE_NAME IN VARCHAR2 DEFAULT NULL,
    P_PPNO IN VARCHAR2 DEFAULT NULL, P_ROLE_ID IN NUMBER DEFAULT NULL,
    P_GROUP_ID IN NUMBER DEFAULT NULL, P_ENTITY_ID IN NUMBER DEFAULT NULL,
    P_USER_CONTEXT_ID IN NUMBER DEFAULT NULL, P_SESSION_ID IN VARCHAR2 DEFAULT NULL,
    P_PAGE_ID IN NUMBER DEFAULT NULL, P_CONTROLLER_NAME IN VARCHAR2 DEFAULT NULL,
    P_CONTROLLER_ACTION IN VARCHAR2 DEFAULT NULL, P_API_PATH IN VARCHAR2 DEFAULT NULL,
    P_HTTP_METHOD IN VARCHAR2 DEFAULT NULL, P_DB_PACKAGE_NAME IN VARCHAR2 DEFAULT NULL,
    P_DB_PROCEDURE_NAME IN VARCHAR2 DEFAULT NULL, P_ENGAGEMENT_ID IN NUMBER DEFAULT NULL,
    P_PARA_ID IN NUMBER DEFAULT NULL, P_OLD_PARA_ID IN NUMBER DEFAULT NULL,
    P_NEW_PARA_ID IN NUMBER DEFAULT NULL, P_COM_ID IN NUMBER DEFAULT NULL,
    P_OBJECT_TYPE IN VARCHAR2 DEFAULT NULL, P_OBJECT_ID IN VARCHAR2 DEFAULT NULL,
    P_RESULT_STATUS IN VARCHAR2 DEFAULT 'SUCCESS', P_RESULT_CODE IN VARCHAR2 DEFAULT NULL,
    P_RESULT_MESSAGE IN VARCHAR2 DEFAULT NULL, P_CLIENT_IP_ADDRESS IN VARCHAR2 DEFAULT NULL,
    P_PROXY_IP_ADDRESS IN VARCHAR2 DEFAULT NULL, P_USER_AGENT IN VARCHAR2 DEFAULT NULL,
    P_TRACE_ID IN VARCHAR2 DEFAULT NULL, P_REQUEST_ID IN VARCHAR2 DEFAULT NULL,
    P_DURATION_MS IN NUMBER DEFAULT NULL, P_DETAILS IN CLOB DEFAULT NULL);

end PKG_LG;
/
create or replace package body PKG_LG is

  procedure p_get_user(PPNumber  in t_user.ppno%type,
                       enc_pass  in t_user.password%type,
                       io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      Select U.*,
             UM.*,
             e.Employeefirstname,
             e.employeelastname,
             ee.name as ent_name,
             g.group_name
        FROM t_user u
       inner join t_user_maping um
          on U.PPNO = UM.PPNO
        left join v_service_employeeinfo e
          on u.PPNO = e.PPNO
        left join t_auditee_entities ee
          on u.entity_id = ee.entity_id
        left join t_groups g
          on um.group_id = g.group_id
      
       WHERE U.PPNO = PPNumber
         and u.Password = enc_pass
         and u.ISACTIVE = 'Y';
  end p_get_user;

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
                         UserPostingAuditZone IN T_USER_SESSION.POSTING_AZ%type,
                         ENT_ID               in number) is
    E_F number := 0;
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
       SESSION_ACTIVE,
       ENTITY_ID)
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
       'Y',
       ENT_ID);
  
    commit;
  
    select NVL(MAX(l.id), 0)
      into E_F
      from t_au_activity_log l
     where l.ppnum = PPNumber;
    insert into t_au_activity_log
      (id, entity_id, role_id, ppnum, page_id, action, seq, unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       UserRoleID,
       PPNumber,
       0,
       'Login in System',
       1,
       'N');
    commit;
  
  end User_SESSION;

  procedure P_GetLoggedInUserEngId(PPNumber  IN NUMBER,
                                   io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select j.eng_plan_id
        from t_au_audit_joining j
       where j.team_mem_ppno = PPNumber
         and j.status = 'I';
  
  end P_GetLoggedInUserEngId;

  procedure Session_END(PPNumber  in t_user_session.user_pp_number%type,
                        SessionId in t_user_session.session_id%type,
                        ENT_ID    in number,
                        R_ID      in number) is
  
    E_F number := 0;
  begin
  
    select NVL(MAX(l.id), 0)
      into E_F
      from t_au_activity_log l
     where l.ppnum = PPNumber;
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
       PPNumber,
       0,
       'Perform Logout in System',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = PPNumber),
       'Y');
    commit;
  
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
  
    PKG_LG.Session_Kill_day_end;
  
  End Session_Kill;

  procedure Session_Kill_day_end is
  
  begin
  
    update t_user_session s
       set s.session_active = 'N'
     where s.session_active = 'Y'
       and trunc(s.logged_in_date) < trunc(sysdate);
  
    commit;
  
  End Session_Kill_day_end;

  procedure P_ChangePassword(PP_NO    in number,
                             enc_pass in t_user.password%type,
                             ENT_ID   in number,
                             P_NO     IN NUMBER,
                             R_ID     IN NUMBER) as
    E_F number := 0;
  begin
  
    UPDATE t_user u
       SET u.PASSWORD = enc_pass, u.password_change_req = 'N'
     WHERE PPNO = PP_NO;
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
       0,
       'Password has been updated',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  end P_ChangePassword;

  procedure p_get_user_id(PPNumber  in t_user.ppno%type,
                          io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      SELECT u.ID
        FROM T_USER_SESSION u
       WHERE u.USER_PP_NUMBER = PPNumber
         and u.SESSION_ACTIVE = 'Y';
  end p_get_user_id;

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
                          ENT_ID     in number,
                          P_NO       in number,
                          R_ID       in number,
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
                              ENT_ID      in number,
                              P_NO        in number,
                              R_ID        in number,
                              io_cursor   OUT t_cursor) as
  begin
    OPEN io_cursor FOR
    
      Select mp.id as page_id,
             mp.id,
             mp.menu_id,
             mp.page_name,
             mp.page_path,
             mp.page_order,
             mp.status,
             mp.hide_menu,
             s.sub_menu_id,
             s.Sub_Menu_Name,
             mp.sub_menu,
             mp.page_key
        FROM T_MENU_PAGES mp
       inner join t_menu_pages_groupmap mpg
          on mp.Id = mpg.page_id
        left join T_MENU_SUB s
          on s.sub_menu_id = mp.sub_menu
       WHERE mp.Status = 'A'
         and mpg.GROUP_ID = UserGroupID
       order by mp.PAGE_ORDER asc;
  
  end p_GetTopMenuPages;

  PROCEDURE p_GetApiPermissions(ENT_ID    IN NUMBER,
                                P_NO      IN NUMBER,
                                R_ID      IN NUMBER,
                                io_cursor OUT t_cursor) AS
  BEGIN
  
    -- Field Audit roles only
    IF R_ID IN (10, 27, 18, 28) THEN
    
      OPEN io_cursor FOR
        SELECT DISTINCT m.API_ID, m.API_PATH, m.HTTP_METHOD
          FROM T_AU_ROLE_API_PERMISSION p
          JOIN T_AU_API_MASTER m
            ON m.API_ID = p.API_ID
         WHERE p.ROLE_ID IN (SELECT R_ID
                               FROM dual
                             
                             UNION
                             
                             SELECT CASE
                                      WHEN et.audit_type = 'D' AND
                                           NVL(tm.isteamlead, 'N') = 'Y' THEN
                                       10
                                      WHEN et.audit_type = 'D' AND
                                           NVL(tm.isteamlead, 'N') = 'N' THEN
                                       27
                                      WHEN et.audit_type = 'B' AND
                                           NVL(tm.isteamlead, 'N') = 'Y' THEN
                                       18
                                      WHEN et.audit_type = 'B' AND
                                           NVL(tm.isteamlead, 'N') = 'N' THEN
                                       28
                                    END
                               FROM t_au_audit_joining j
                               JOIN t_au_plan_eng pe
                                 ON pe.eng_id = j.eng_plan_id
                               JOIN t_auditee_entities ae
                                 ON ae.entity_id = pe.entity_id
                               JOIN t_auditee_ent_types et
                                 ON ae.type_id = et.autid
                               JOIN t_au_audit_team_tasklist tl
                                 ON tl.eng_plan_id = j.eng_plan_id
                                AND tl.teammember_ppno = j.team_mem_ppno
                               JOIN t_au_team_members tm
                                 ON tm.t_id = tl.team_id
                                AND tm.member_ppno = tl.teammember_ppno
                              WHERE j.team_mem_ppno = P_NO
                                AND j.status = 'I'
                                AND tl.isactive = 'Y')
           AND p.IS_ACTIVE = 'Y'
           AND m.IS_ACTIVE = 'Y';
    
    ELSE
    
      -- Existing IAS behaviour remains completely unchanged
      OPEN io_cursor FOR
        SELECT m.API_ID, m.API_PATH, m.HTTP_METHOD
          FROM T_AU_ROLE_API_PERMISSION p
          JOIN T_AU_API_MASTER m
            ON m.API_ID = p.API_ID
         WHERE p.ROLE_ID = R_ID
           AND p.IS_ACTIVE = 'Y'
           AND m.IS_ACTIVE = 'Y';
    
    END IF;
  
  END p_GetApiPermissions;

  PROCEDURE P_GET_ENG_PAGE_PERMISSIONS_BY_PPNO(P_PP_NO   IN NUMBER,
                                               io_cursor OUT t_cursor) AS
  BEGIN
    OPEN io_cursor FOR
      SELECT p.ppno,
             p.page_id,
             e.eng_id,
             p.status,
             p.created_on,
             p.created_by
        FROM T_AU_USER_ENG_PAGE_PERM p
        JOIN M_ENG e
          ON e.m_eng_id = p.m_eng_id
       WHERE p.ppno = P_PP_NO
         AND p.status = 1
         AND e.status = 1
       ORDER BY e.eng_id, p.page_id;
  END P_GET_ENG_PAGE_PERMISSIONS_BY_PPNO;

  PROCEDURE P_GET_COM_PAGE_PERMISSIONS_BY_PPNO(P_PP_NO   IN NUMBER,
                                               io_cursor OUT t_cursor) AS
  BEGIN
    OPEN io_cursor FOR
      SELECT p.ppno,
             p.page_id,
             p.com_id,
             p.status,
             p.created_on,
             p.created_by
        FROM T_AU_USER_COM_PAGE_PERM p
       WHERE p.ppno = P_PP_NO
         AND p.status = 1
       ORDER BY p.com_id, p.page_id;
  END P_GET_COM_PAGE_PERMISSIONS_BY_PPNO;

  procedure P_GetRiskProcessDefinition(io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select * from t_audit_checklist t order by t.T_ID;
  
  end P_GetRiskProcessDefinition;

  procedure p_get_emp_name(PP_NO in number, io_cursor OUT t_cursor) as
  
  begin
    open io_cursor for
      select e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name
        from v_service_employeeinfo e
       where e.PPNO = PP_NO;
  
  end p_get_emp_name;

  PROCEDURE LOG_INFO(p_module       IN VARCHAR2,
                     p_controller   IN VARCHAR2,
                     p_action       IN VARCHAR2,
                     p_message      IN VARCHAR2,
                     p_tech_details IN CLOB DEFAULT NULL,
                     p_page_id      IN NUMBER DEFAULT NULL,
                     p_eng_id       IN NUMBER DEFAULT NULL,
                     p_user_ppno    IN NUMBER DEFAULT NULL) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    INSERT INTO T_SYS_LOG
      (LOG_LEVEL,
       LOG_TIME,
       MODULE,
       CONTROLLER,
       ACTION,
       MESSAGE,
       TECH_DETAILS,
       PAGE_ID,
       ENG_ID,
       USER_PPNO)
    VALUES
      ('INFO',
       SYSTIMESTAMP,
       p_module,
       p_controller,
       p_action,
       p_message,
       p_tech_details,
       p_page_id,
       p_eng_id,
       p_user_ppno);
  
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      NULL;
  END LOG_INFO;

  PROCEDURE LOG_WARNING(p_module       IN VARCHAR2,
                        p_controller   IN VARCHAR2,
                        p_action       IN VARCHAR2,
                        p_message      IN VARCHAR2,
                        p_tech_details IN CLOB DEFAULT NULL,
                        p_page_id      IN NUMBER DEFAULT NULL,
                        p_eng_id       IN NUMBER DEFAULT NULL,
                        p_user_ppno    IN NUMBER DEFAULT NULL) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    INSERT INTO T_SYS_LOG
      (LOG_LEVEL,
       LOG_TIME,
       MODULE,
       CONTROLLER,
       ACTION,
       MESSAGE,
       TECH_DETAILS,
       PAGE_ID,
       ENG_ID,
       USER_PPNO)
    VALUES
      ('WARNING',
       SYSTIMESTAMP,
       p_module,
       p_controller,
       p_action,
       p_message,
       p_tech_details,
       p_page_id,
       p_eng_id,
       p_user_ppno);
  
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      NULL;
  END LOG_WARNING;

  PROCEDURE LOG_ERROR(p_module       IN VARCHAR2,
                      p_controller   IN VARCHAR2,
                      p_action       IN VARCHAR2,
                      p_message      IN VARCHAR2,
                      p_tech_details IN CLOB DEFAULT NULL,
                      p_page_id      IN NUMBER DEFAULT NULL,
                      p_eng_id       IN NUMBER DEFAULT NULL,
                      p_user_ppno    IN NUMBER DEFAULT NULL) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    INSERT INTO T_SYS_LOG
      (LOG_LEVEL,
       LOG_TIME,
       MODULE,
       CONTROLLER,
       ACTION,
       MESSAGE,
       TECH_DETAILS,
       PAGE_ID,
       ENG_ID,
       USER_PPNO)
    VALUES
      ('ERROR',
       SYSTIMESTAMP,
       p_module,
       p_controller,
       p_action,
       p_message,
       p_tech_details,
       p_page_id,
       p_eng_id,
       p_user_ppno);
  
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      NULL;
  END LOG_ERROR;

  PROCEDURE P_GET_SYS_LOGS(p_start_time IN TIMESTAMP DEFAULT NULL,
                           p_end_time   IN TIMESTAMP DEFAULT NULL,
                           p_log_level  IN VARCHAR2 DEFAULT NULL,
                           p_module     IN VARCHAR2 DEFAULT NULL,
                           p_user_ppno  IN NUMBER DEFAULT NULL,
                           p_eng_id     IN NUMBER DEFAULT NULL,
                           io_cursor    OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT LOG_ID,
             LOG_LEVEL,
             LOG_TIME,
             MODULE,
             CONTROLLER,
             ACTION,
             MESSAGE,
             TECH_DETAILS,
             PAGE_ID,
             ENG_ID,
             USER_PPNO
        FROM T_SYS_LOG
      /*       WHERE (p_start_time IS NULL OR LOG_TIME >= p_start_time)
      AND (p_end_time IS NULL OR LOG_TIME <= p_end_time)
      AND (p_log_level IS NULL OR LOG_LEVEL = p_log_level)
      AND (p_module IS NULL OR MODULE = p_module)
      AND (p_user_ppno IS NULL OR USER_PPNO = p_user_ppno)
      AND (p_eng_id IS NULL OR ENG_ID = p_eng_id)*/
       ORDER BY LOG_TIME DESC;
  END P_GET_SYS_LOGS;

  PROCEDURE REGISTER_SYSTEM_ERROR(P_FINGERPRINT          IN VARCHAR2,
                                  P_ERROR_TYPE           IN VARCHAR2,
                                  P_ERROR_CODE           IN VARCHAR2,
                                  P_MODULE               IN VARCHAR2,
                                  P_CONTROLLER           IN VARCHAR2,
                                  P_ACTION               IN VARCHAR2,
                                  P_API_PATH             IN VARCHAR2,
                                  P_STORED_PROCEDURE     IN VARCHAR2,
                                  P_PPNO                 IN VARCHAR2,
                                  P_ROLE_NAME            IN VARCHAR2,
                                  P_ENTITY_NAME          IN VARCHAR2,
                                  P_PAGE_ID              IN NUMBER,
                                  P_ENG_ID               IN NUMBER,
                                  P_PARA_ID              IN NUMBER,
                                  P_COM_ID               IN NUMBER,
                                  P_TRACE_ID             IN VARCHAR2,
                                  P_IP_ADDRESS           IN VARCHAR2,
                                  P_USER_AGENT           IN VARCHAR2,
                                  P_ERROR_MESSAGE        IN CLOB,
                                  P_TECHNICAL_DETAILS    IN CLOB,
                                  O_ERROR_ID             OUT NUMBER,
                                  O_ERROR_REFERENCE      OUT VARCHAR2,
                                  O_IS_FIRST_OCCURRENCE  OUT NUMBER,
                                  O_FIRST_OCCURRENCE_UTC OUT TIMESTAMP,
                                  O_LAST_OCCURRENCE_UTC  OUT TIMESTAMP,
                                  O_OCCURRENCE_COUNT     OUT NUMBER,
                                  O_EMAIL_ALREADY_SENT   OUT NUMBER) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    v_now          TIMESTAMP := SYS_EXTRACT_UTC(SYSTIMESTAMP);
    v_was_resolved NUMBER := 0;
  BEGIN
    BEGIN
      INSERT INTO T_AU_SYSTEM_ERROR_MASTER
        (FINGERPRINT,
         ERROR_REFERENCE,
         ERROR_TYPE,
         ERROR_CODE,
         MODULE,
         CONTROLLER,
         ACTION,
         API_PATH,
         STORED_PROCEDURE,
         PPNO,
         ROLE_NAME,
         ENTITY_NAME,
         PAGE_ID,
         ENG_ID,
         PARA_ID,
         COM_ID,
         FIRST_OCCURRENCE_UTC,
         LAST_OCCURRENCE_UTC,
         OCCURRENCE_COUNT,
         LAST_TRACE_ID,
         LAST_IP_ADDRESS,
         ERROR_MESSAGE,
         TECHNICAL_DETAILS)
      VALUES
        (P_FINGERPRINT,
         'IAS-ERR-' || SUBSTR(RAWTOHEX(SYS_GUID()), 1, 12),
         P_ERROR_TYPE,
         P_ERROR_CODE,
         P_MODULE,
         P_CONTROLLER,
         P_ACTION,
         P_API_PATH,
         P_STORED_PROCEDURE,
         P_PPNO,
         P_ROLE_NAME,
         P_ENTITY_NAME,
         P_PAGE_ID,
         P_ENG_ID,
         P_PARA_ID,
         P_COM_ID,
         v_now,
         v_now,
         1,
         P_TRACE_ID,
         P_IP_ADDRESS,
         P_ERROR_MESSAGE,
         P_TECHNICAL_DETAILS)
      RETURNING ERROR_ID, FIRST_OCCURRENCE_UTC, LAST_OCCURRENCE_UTC, OCCURRENCE_COUNT, DECODE
        (EMAIL_SENT, 'Y', 1, 0) INTO O_ERROR_ID, O_FIRST_OCCURRENCE_UTC, O_LAST_OCCURRENCE_UTC, O_OCCURRENCE_COUNT, O_EMAIL_ALREADY_SENT;
    
      O_ERROR_REFERENCE := 'IAS-ERR-' || LPAD(O_ERROR_ID, 5, '0');
      UPDATE T_AU_SYSTEM_ERROR_MASTER
         SET ERROR_REFERENCE = O_ERROR_REFERENCE
       WHERE ERROR_ID = O_ERROR_ID;
      O_IS_FIRST_OCCURRENCE := 1;
    EXCEPTION
      WHEN DUP_VAL_ON_INDEX THEN
        SELECT CASE
                 WHEN RESOLUTION_STATUS = 'RESOLVED' THEN
                  1
                 ELSE
                  0
               END
          INTO v_was_resolved
          FROM T_AU_SYSTEM_ERROR_MASTER
         WHERE FINGERPRINT = P_FINGERPRINT;
        UPDATE T_AU_SYSTEM_ERROR_MASTER
           SET LAST_OCCURRENCE_UTC = v_now,
               OCCURRENCE_COUNT    = OCCURRENCE_COUNT + 1,
               LAST_TRACE_ID       = P_TRACE_ID,
               LAST_IP_ADDRESS     = P_IP_ADDRESS,
               ERROR_MESSAGE       = P_ERROR_MESSAGE,
               TECHNICAL_DETAILS   = P_TECHNICAL_DETAILS,
               EMAIL_SENT = CASE
                              WHEN RESOLUTION_STATUS = 'RESOLVED' THEN
                               'N'
                              ELSE
                               EMAIL_SENT
                            END,
               EMAIL_SENT_UTC = CASE
                                  WHEN RESOLUTION_STATUS = 'RESOLVED' THEN
                                   NULL
                                  ELSE
                                   EMAIL_SENT_UTC
                                END,
               RESOLUTION_STATUS = CASE
                                     WHEN RESOLUTION_STATUS = 'RESOLVED' THEN
                                      'OPEN'
                                     ELSE
                                      RESOLUTION_STATUS
                                   END
         WHERE FINGERPRINT = P_FINGERPRINT
         RETURNING ERROR_ID, ERROR_REFERENCE, FIRST_OCCURRENCE_UTC, LAST_OCCURRENCE_UTC, OCCURRENCE_COUNT, DECODE(EMAIL_SENT, 'Y', 1, 0)
           INTO O_ERROR_ID, O_ERROR_REFERENCE, O_FIRST_OCCURRENCE_UTC, O_LAST_OCCURRENCE_UTC, O_OCCURRENCE_COUNT, O_EMAIL_ALREADY_SENT;
        O_IS_FIRST_OCCURRENCE := 0;
        
        INSERT INTO T_AU_SYSTEM_ERROR_STATUS_HISTORY
          (ERROR_ID, OLD_STATUS, NEW_STATUS, CHANGED_BY_PPNO, REMARKS)
        SELECT O_ERROR_ID, 'RESOLVED', 'OPEN', P_PPNO, 'Fingerprint recurred after resolution; reopened automatically.'
          FROM dual
         WHERE v_was_resolved = 1;
    END;
  
    INSERT INTO T_AU_SYSTEM_ERROR_HISTORY
      (ERROR_ID,
       OCCURRED_ON_UTC,
       TRACE_ID,
       IP_ADDRESS,
       USER_AGENT,
       PPNO,
       ROLE_NAME,
       ENTITY_NAME,
       PAGE_ID,
       ENG_ID,
       PARA_ID,
       COM_ID,
       API_PATH,
       ERROR_MESSAGE,
       TECHNICAL_DETAILS)
    VALUES
      (O_ERROR_ID,
       v_now,
       P_TRACE_ID,
       P_IP_ADDRESS,
       P_USER_AGENT,
       P_PPNO,
       P_ROLE_NAME,
       P_ENTITY_NAME,
       P_PAGE_ID,
       P_ENG_ID,
       P_PARA_ID,
       P_COM_ID,
       P_API_PATH,
       P_ERROR_MESSAGE,
       P_TECHNICAL_DETAILS);
  
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
  END REGISTER_SYSTEM_ERROR;

  PROCEDURE MARK_SYSTEM_ERROR_EMAIL(P_ERROR_ID   IN NUMBER,
                                    P_EMAIL_SENT IN NUMBER) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    UPDATE T_AU_SYSTEM_ERROR_MASTER
       SET EMAIL_SENT = CASE
                          WHEN P_EMAIL_SENT = 1 THEN
                           'Y'
                          ELSE
                           'N'
                        END,
           EMAIL_SENT_UTC = CASE
                              WHEN P_EMAIL_SENT = 1 THEN
                               SYS_EXTRACT_UTC(SYSTIMESTAMP)
                              ELSE
                               EMAIL_SENT_UTC
                            END
     WHERE ERROR_ID = P_ERROR_ID;
  
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
  END MARK_SYSTEM_ERROR_EMAIL;

  PROCEDURE GET_SYSTEM_ERROR_RECIPIENTS(O_CUR OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CUR FOR
      SELECT RECIPIENT_TYPE, EMAIL_ADDRESS
        FROM T_AU_SYSTEM_ERROR_RECIPIENTS
       WHERE IS_ACTIVE = 'Y'
       ORDER BY SORT_ORDER, RECIPIENT_ID;
  END GET_SYSTEM_ERROR_RECIPIENTS;

 PROCEDURE GET_SYSTEM_ERRORS(
      P_STATUS          IN  VARCHAR2,
      P_FROM_DATE       IN  TIMESTAMP,
      P_TO_DATE         IN  TIMESTAMP,
      P_ERROR_REFERENCE IN  VARCHAR2,
      P_MODULE          IN  VARCHAR2,
      P_USER_PPNO       IN  VARCHAR2,
      P_ENTITY          IN  VARCHAR2,
      P_ERROR_TYPE_CODE IN  VARCHAR2,
      O_CUR             OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CUR FOR
      SELECT ERROR_ID, ERROR_REFERENCE, RESOLUTION_STATUS, FIRST_OCCURRENCE_UTC, LAST_OCCURRENCE_UTC,
             OCCURRENCE_COUNT, MODULE, CONTROLLER, ACTION, API_PATH, PPNO, ROLE_NAME, ENTITY_NAME,
             ERROR_TYPE, ERROR_CODE, STORED_PROCEDURE, LAST_IP_ADDRESS, RESOLVED_BY, RESOLVED_ON_UTC,
             RESOLUTION_REMARKS
        FROM T_AU_SYSTEM_ERROR_MASTER
       WHERE (P_STATUS IS NULL OR P_STATUS = 'ALL' OR RESOLUTION_STATUS = P_STATUS)
         AND (P_FROM_DATE IS NULL OR LAST_OCCURRENCE_UTC >= P_FROM_DATE)
         AND (P_TO_DATE IS NULL OR LAST_OCCURRENCE_UTC <= P_TO_DATE)
         AND (P_ERROR_REFERENCE IS NULL OR UPPER(ERROR_REFERENCE) LIKE '%' || UPPER(P_ERROR_REFERENCE) || '%')
         AND (P_MODULE IS NULL OR UPPER(MODULE) LIKE '%' || UPPER(P_MODULE) || '%')
         AND (P_USER_PPNO IS NULL OR UPPER(PPNO) LIKE '%' || UPPER(P_USER_PPNO) || '%')
         AND (P_ENTITY IS NULL OR UPPER(ENTITY_NAME) LIKE '%' || UPPER(P_ENTITY) || '%')
         AND (P_ERROR_TYPE_CODE IS NULL OR UPPER(ERROR_TYPE) LIKE '%' || UPPER(P_ERROR_TYPE_CODE) || '%' OR UPPER(ERROR_CODE) LIKE '%' || UPPER(P_ERROR_TYPE_CODE) || '%')
       ORDER BY LAST_OCCURRENCE_UTC DESC;
  END GET_SYSTEM_ERRORS;

  PROCEDURE GET_SYSTEM_ERROR_DETAIL(
      P_ERROR_ID         IN  NUMBER,
      O_MASTER           OUT SYS_REFCURSOR,
      O_HISTORY          OUT SYS_REFCURSOR,
      O_STATUS_HISTORY   OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_MASTER FOR
      SELECT *
        FROM T_AU_SYSTEM_ERROR_MASTER
       WHERE ERROR_ID = P_ERROR_ID;

    OPEN O_HISTORY FOR
      SELECT HISTORY_ID, OCCURRED_ON_UTC, TRACE_ID, IP_ADDRESS, USER_AGENT, PPNO, ROLE_NAME,
             ENTITY_NAME, PAGE_ID, ENG_ID, PARA_ID, COM_ID, API_PATH, ERROR_MESSAGE
        FROM T_AU_SYSTEM_ERROR_HISTORY
       WHERE ERROR_ID = P_ERROR_ID
       ORDER BY OCCURRED_ON_UTC DESC;

    OPEN O_STATUS_HISTORY FOR
      SELECT CHANGED_ON_UTC, OLD_STATUS, NEW_STATUS, CHANGED_BY_PPNO, REMARKS
        FROM T_AU_SYSTEM_ERROR_STATUS_HISTORY
       WHERE ERROR_ID = P_ERROR_ID
       ORDER BY CHANGED_ON_UTC DESC;
  END GET_SYSTEM_ERROR_DETAIL;

  PROCEDURE RESOLVE_SYSTEM_ERROR(
      P_ERROR_ID          IN NUMBER,
      P_RESOLVED_BY_PPNO  IN VARCHAR2,
      P_REMARKS           IN VARCHAR2) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
    v_old_status VARCHAR2(30);
  BEGIN
    IF TRIM(P_REMARKS) IS NULL THEN
      RAISE_APPLICATION_ERROR(-20001, 'Resolution remarks are required.');
    END IF;

    SELECT RESOLUTION_STATUS
      INTO v_old_status
      FROM T_AU_SYSTEM_ERROR_MASTER
     WHERE ERROR_ID = P_ERROR_ID
     FOR UPDATE;

    IF v_old_status = 'RESOLVED' THEN
      RAISE_APPLICATION_ERROR(-20002, 'System error is already resolved.');
    END IF;

    UPDATE T_AU_SYSTEM_ERROR_MASTER
       SET RESOLUTION_STATUS = 'RESOLVED',
           RESOLVED_BY = P_RESOLVED_BY_PPNO,
           RESOLVED_ON_UTC = SYS_EXTRACT_UTC(SYSTIMESTAMP),
           RESOLUTION_REMARKS = P_REMARKS
     WHERE ERROR_ID = P_ERROR_ID;

    INSERT INTO T_AU_SYSTEM_ERROR_STATUS_HISTORY
      (ERROR_ID, OLD_STATUS, NEW_STATUS, CHANGED_BY_PPNO, REMARKS)
    VALUES
      (P_ERROR_ID, v_old_status, 'RESOLVED', P_RESOLVED_BY_PPNO, P_REMARKS);

    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
  END RESOLVE_SYSTEM_ERROR;

  PROCEDURE P_LOG_APPLICATION_ACTIVITY(
    P_EVENT_TYPE IN VARCHAR2, P_ACTION_NAME IN VARCHAR2,
    P_ACTION_CATEGORY IN VARCHAR2 DEFAULT NULL, P_MODULE_NAME IN VARCHAR2 DEFAULT NULL,
    P_PPNO IN VARCHAR2 DEFAULT NULL, P_ROLE_ID IN NUMBER DEFAULT NULL,
    P_GROUP_ID IN NUMBER DEFAULT NULL, P_ENTITY_ID IN NUMBER DEFAULT NULL,
    P_USER_CONTEXT_ID IN NUMBER DEFAULT NULL, P_SESSION_ID IN VARCHAR2 DEFAULT NULL,
    P_PAGE_ID IN NUMBER DEFAULT NULL, P_CONTROLLER_NAME IN VARCHAR2 DEFAULT NULL,
    P_CONTROLLER_ACTION IN VARCHAR2 DEFAULT NULL, P_API_PATH IN VARCHAR2 DEFAULT NULL,
    P_HTTP_METHOD IN VARCHAR2 DEFAULT NULL, P_DB_PACKAGE_NAME IN VARCHAR2 DEFAULT NULL,
    P_DB_PROCEDURE_NAME IN VARCHAR2 DEFAULT NULL, P_ENGAGEMENT_ID IN NUMBER DEFAULT NULL,
    P_PARA_ID IN NUMBER DEFAULT NULL, P_OLD_PARA_ID IN NUMBER DEFAULT NULL,
    P_NEW_PARA_ID IN NUMBER DEFAULT NULL, P_COM_ID IN NUMBER DEFAULT NULL,
    P_OBJECT_TYPE IN VARCHAR2 DEFAULT NULL, P_OBJECT_ID IN VARCHAR2 DEFAULT NULL,
    P_RESULT_STATUS IN VARCHAR2 DEFAULT 'SUCCESS', P_RESULT_CODE IN VARCHAR2 DEFAULT NULL,
    P_RESULT_MESSAGE IN VARCHAR2 DEFAULT NULL, P_CLIENT_IP_ADDRESS IN VARCHAR2 DEFAULT NULL,
    P_PROXY_IP_ADDRESS IN VARCHAR2 DEFAULT NULL, P_USER_AGENT IN VARCHAR2 DEFAULT NULL,
    P_TRACE_ID IN VARCHAR2 DEFAULT NULL, P_REQUEST_ID IN VARCHAR2 DEFAULT NULL,
    P_DURATION_MS IN NUMBER DEFAULT NULL, P_DETAILS IN CLOB DEFAULT NULL) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    INSERT INTO T_AU_APPLICATION_AUDIT_LOG
      (ID, EVENT_TIME, EVENT_TYPE, ACTION_NAME, ACTION_CATEGORY, MODULE_NAME,
       PPNO, ROLE_ID, GROUP_ID, ENTITY_ID, USER_CONTEXT_ID, SESSION_ID,
       PAGE_ID, CONTROLLER_NAME, CONTROLLER_ACTION, API_PATH, HTTP_METHOD,
       DB_PACKAGE_NAME, DB_PROCEDURE_NAME, ENGAGEMENT_ID, PARA_ID, OLD_PARA_ID,
       NEW_PARA_ID, COM_ID, OBJECT_TYPE, OBJECT_ID, RESULT_STATUS, RESULT_CODE,
       RESULT_MESSAGE, CLIENT_IP_ADDRESS, PROXY_IP_ADDRESS, USER_AGENT,
       TRACE_ID, REQUEST_ID, DURATION_MS, DETAILS, CREATED_ON)
    VALUES
      (SEQ_T_AU_APPLICATION_AUDIT_LOG.NEXTVAL, SYSTIMESTAMP, P_EVENT_TYPE,
       P_ACTION_NAME, P_ACTION_CATEGORY, P_MODULE_NAME, P_PPNO, P_ROLE_ID,
       P_GROUP_ID, P_ENTITY_ID, P_USER_CONTEXT_ID, P_SESSION_ID, P_PAGE_ID,
       P_CONTROLLER_NAME, P_CONTROLLER_ACTION, P_API_PATH, P_HTTP_METHOD,
       P_DB_PACKAGE_NAME, P_DB_PROCEDURE_NAME, P_ENGAGEMENT_ID, P_PARA_ID,
       P_OLD_PARA_ID, P_NEW_PARA_ID, P_COM_ID, P_OBJECT_TYPE, P_OBJECT_ID,
       NVL(P_RESULT_STATUS, 'SUCCESS'), P_RESULT_CODE, P_RESULT_MESSAGE,
       P_CLIENT_IP_ADDRESS, P_PROXY_IP_ADDRESS, P_USER_AGENT, P_TRACE_ID,
       P_REQUEST_ID, P_DURATION_MS, P_DETAILS, SYSTIMESTAMP);
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
  END P_LOG_APPLICATION_ACTIVITY;

end PKG_LG;


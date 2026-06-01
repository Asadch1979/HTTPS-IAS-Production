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

end PKG_LG;

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
    OPEN io_cursor FOR
      SELECT m.API_ID, m.API_PATH, m.HTTP_METHOD
        FROM T_AU_ROLE_API_PERMISSION p
        JOIN T_AU_API_MASTER m
          ON m.API_ID = p.API_ID
       WHERE p.ROLE_ID = R_ID
         AND p.IS_ACTIVE = 'Y'
         AND m.IS_ACTIVE = 'Y';
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
                           io_cursor     OUT SYS_REFCURSOR) IS
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

end PKG_LG;

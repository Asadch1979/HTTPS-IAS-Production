create or replace package PKG_PG is

  TYPE t_cursor IS REF CURSOR;

  Procedure P_get_Criteria_ent_count(CID IN NUMBER, io_cursor OUT t_cursor);

  procedure P_GetAuditPeriods(ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor);

  procedure P_AddAuditPeriod(DESCRIPTION in T_AU_PERIOD.DESCRIPTION%type,
                             START_DATE  in T_AU_PERIOD.START_DATE%type,
                             END_DATE    in T_AU_PERIOD.End_Date%type,
                             io_cursor   OUT t_cursor);

  Procedure P_GET_Auditperiod_status(io_cursor OUT t_cursor);

  Procedure p_update_Auditperiod(P_ID      in number,
                                 S_ID      in number,
                                 io_cursor OUT t_cursor);
  -- not in use
  procedure AUDIT_CRITERIA_LOG(CREATEDBY_ID in T_AUDIT_CRITERIA_LOG.CREATEDBY_ID%type,
                               STATUS_ID    in T_AUDIT_CRITERIA_LOG.STATUS_ID%type,
                               REMARKS      in T_AUDIT_CRITERIA_LOG.REMARKS%type);

  procedure P_ADDAUDITCRITERIA(ENTITYTYPEID   in T_AUDIT_CRITERIA.ENTITY_TYPEID%type,
                               SIZEID         in T_AUDIT_CRITERIA.SIZE_ID%type,
                               RISKID         in T_AUDIT_CRITERIA.RISK_ID%type,
                               FREQUENCYID    in T_AUDIT_CRITERIA.FREQUENCY_ID%type,
                               NOOFDAYS       in T_AUDIT_CRITERIA.NO_OF_DAYS%type,
                               visit          in T_AUDIT_CRITERIA.VISIT%type,
                               APPROVALSTATUS in T_AUDIT_CRITERIA.APPROVAL_STATUS%type,
                               AUDITPERIODID  in T_AUDIT_CRITERIA.AUDITPERIODID%type,
                               ENTITYID       in T_AUDIT_CRITERIA.CREATED_BY%type,
                               REMARKS        in T_AUDIT_CRITERIA_LOG.REMARKS%type,
                               P_NO           in number,
                               ENT_ID         in number,
                               R_ID           in number,
                               io_cursor      OUT t_cursor);
  -- not in use
  procedure P_ADDCADAuditPlan(auditperiod_id in T_AU_PLAN.AUDITPERIODID%type,
                              auditby_id     in number,
                              entityid       in number,
                              noofdays       in number,
                              typeid         in number);

  procedure P_DeletePendingCriteria(ENT_ID in number,
                                    P_NO   in number,
                                    R_ID   in number,
                                    CID    IN NUMBER);

  procedure P_UpdateAuditCriteria(CID            IN t_audit_criteria.id%type,
                                  ENTITYTYPEID   IN T_AUDIT_CRITERIA.ENTITY_TYPEID%TYPE,
                                  SIZEID         IN T_AUDIT_CRITERIA.SIZE_ID%TYPE,
                                  RISKID         IN T_AUDIT_CRITERIA.RISK_ID%TYPE,
                                  FREQUENCYID    IN T_AUDIT_CRITERIA.FREQUENCY_ID%TYPE,
                                  NOOFDAYS       IN T_AUDIT_CRITERIA.NO_OF_DAYS%TYPE,
                                  VISITS         IN T_AUDIT_CRITERIA.VISIT%TYPE,
                                  AUDITPERIOD_ID IN T_AUDIT_CRITERIA.AUDITPERIODID%TYPE,
                                  REMARKS        IN VARCHAR2,
                                  ENT_ID         in number,
                                  P_NO           in number,
                                  R_ID           in number);

  procedure P_SetAuditCriteriaStatusReferredBack(CID     IN t_audit_criteria.id%type,
                                                 REMARKS IN VARCHAR2,
                                                 ENT_ID  in number,
                                                 P_NO    in number,
                                                 R_ID    in number);

  procedure P_SubmitAuditCriteriaForApproval(ENT_ID in number,
                                             P_NO   in number,
                                             R_ID   in number,
                                             CID    IN NUMBER);

  procedure P_SetAuditCriteriaStatusApprove(CAID      IN t_audit_criteria.id%type,
                                            REMARKS   IN VARCHAR2,
                                            ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);
  -- not in use
  procedure P_GetAuditCriteriaLogLastStatus(ID        IN NUMBER,
                                            io_cursor OUT t_cursor);

  procedure P_GetPendingAuditCriterias(ENT_ID    IN NUMBER,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor);

  procedure P_GetRefferedBackAuditCriterias(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure P_GetAuditCriteriasToAuthorize(ENT_ID    in number,
                                           P_NO      in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor);

  procedure P_GetPostChangesAuditCriterias(ENT_ID    in number,
                                           P_NO      in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor);

  procedure P_GetAuditEntities(ENTITYID IN NUMBER, io_cursor OUT t_cursor);

  procedure P_GetAuditEmployees(dept_code IN NUMBER,
                                io_cursor OUT t_cursor);

  procedure P_AddAuditTeam(TEAMNAME      in T_AU_TEAM_MEMBERS.TEAM_NAME%type,
                           TEAMMEMBER_ID in T_AU_TEAM_MEMBERS.MEMBER_PPNO%type,
                           MAX_T_ID      IN T_AU_TEAM_MEMBERS.T_ID%TYPE,
                           EMPLOYEENAME  in T_AU_TEAM_MEMBERS.MEMBER_NAME%type,
                           IS_TEAMLEAD   in T_AU_TEAM_MEMBERS.ISTEAMLEAD%type,
                           STATUS        in T_AU_TEAM_MEMBERS.STATUS%type,
                           ENT_ID        in number,
                           P_NO          in number,
                           R_ID          in number,
                           io_cursor     OUT t_cursor);

  PROCEDURE P_DeleteAuditTeam(TID    IN NUMBER,
                              ENT_ID IN NUMBER,
                              P_NO   IN NUMBER,
                              R_ID   IN NUMBER);

  PROCEDURE P_MAXTEAMID(ENT_ID    in number,
                        P_NO      in number,
                        R_ID      in number,
                        io_cursor OUT t_cursor);
  --not in use
  /* procedure p_Getauditteams(ENGID in number, io_cursor OUT t_cursor);*/

  procedure P_GetAuditTeams(dept_code    IN NUMBER,
                            UserEntityID IN NUMBER,
                            P_NO         in number,
                            R_ID         in number,
                            io_cursor    OUT t_cursor);

  procedure p_GetAuditFrequencies(io_cursor OUT t_cursor);

  Procedure P_ADD_Special_Audit_Plan(P_ID           in number,
                                     NOOFDAYS       in number,
                                     Nature         in number,
                                     AUDITPERIOD_ID in number,
                                     ENTITYID       in number,
                                     IND            in varchar2,
                                     P_NO           in number,
                                     ENT_ID         in number,
                                     R_ID           in number,
                                     io_cursor      OUT t_cursor);

  procedure P_GET_Specical_Audit_for_Approval(ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor);

  Procedure P_Update_Special_Audit(P_ID      in number,
                                   IND       in varchar2,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure Tentative_Audit_Plan(CRITERIA_ID in t_audit_criteria.id%type,
                                 ENT_ID      in number,
                                 P_NO        in number,
                                 R_ID        in number,
                                 io_cursor   OUT t_cursor);

  procedure p_get_audit_plan(ENT_ID    in number,
                             P_NO      in number,
                             R_ID      in number,
                             io_cursor OUT t_cursor);

  procedure P_GetAuditOperationalStartDate(entityCode    IN NUMBER,
                                           auditPeriodId IN NUMBER,
                                           io_cursor     OUT t_cursor);

  procedure P_AddAuditEngagementPlan(PERIODID        in number,
                                     ENTITYID        in number,
                                     AUDIT_STARTDATE in date,
                                     CREATEDBY       in number,
                                     AUDIT_ENDDATE   in date,
                                     STATUS          in varchar2,
                                     TEAMID          in number,
                                     TEAM_NAME       in varchar2,
                                     PLANID          IN NUMBER,
                                     OP_STARTDATE    in date,
                                     OP_ENDDATE      in date,
                                     TRAVELDAY       in number,
                                     RRDAY           in number,
                                     D_Day           in number,
                                     ENT_ID          in number,
                                     P_NO            in number,
                                     R_ID            in number,
                                     io_cursor       OUT t_cursor);

  procedure P_RerecommendAuditEngagementPlan(ENGID        in number,
                                             ENTITYID     in number,
                                             STARTDATE    in date,
                                             ENDDATE      in date,
                                             TEAMID       in number,
                                             PLANID       IN NUMBER,
                                             OP_STARTDATE in date,
                                             OP_ENDDATE   in date,
                                             REMARKS      in varchar2,
                                             ENT_ID       in number,
                                             P_NO         in number,
                                             R_ID         in number,
                                             io_cursor    OUT t_cursor);

  procedure P_GetLatestCommentsOnEngagement(ENGID     IN NUMBER,
                                            io_cursor OUT t_cursor);

  procedure P_GetAuditEngagementPlans(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      io_cursor OUT t_cursor);

  procedure P_GetRefferedBackAuditEngagementPlans(ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor);

  procedure P_AddAuditteamtasklist(TEAMID   in number,
                                   PLANID   IN NUMBER,
                                   ENTITYID IN NUMBER,
                                   ENT_ID   in number,
                                   P_NO     in number,
                                   R_ID     in number);
  -- not in use
  procedure AUDIT_TEAMS(TEAM_ID        in T_AU_AUDIT_TEAMS.TEAM_ID%type,
                        TEAM_NAME      in T_AU_AUDIT_TEAMS.T_NAME%type,
                        placeofposting in T_AU_AUDIT_TEAMS.PLACE_OF_POSTING%type);
  -- not in use
  procedure plan_eng_log(createdbyId in t_au_plan_eng_log.createdby_id%type,
                         STATUS      in t_au_plan_eng_log.status_id%type);

  procedure P_RefferedBackAuditEngagementPlan(ENGID   IN NUMBER,
                                              REMARKS IN VARCHAR2,
                                              ENT_ID  in number,
                                              P_NO    in number,
                                              R_ID    in number);

  procedure P_ApproveAuditEngagementPlan(ENGID  IN NUMBER,
                                         ENT_ID in number,
                                         P_NO   in number,
                                         R_ID   in number);
  -- not in use
  procedure TEAM_TASKLIST(TEAM_ID         in T_AU_AUDIT_TEAM_TASKLIST.TEAM_ID%type,
                          sequence_no     in T_AU_AUDIT_TEAM_TASKLIST.SEQUENCE_NO%type,
                          member_pp       in T_AU_AUDIT_TEAM_TASKLIST.TEAMMEMBER_PPNO%type,
                          ENTITY_ID       in T_AU_AUDIT_TEAM_TASKLIST.ENTITY_ID%type,
                          AUDIT_STARTDATE in T_AU_AUDIT_TEAM_TASKLIST.AUDIT_START_DATE%type,
                          AUDIT_endDATE   in T_AU_AUDIT_TEAM_TASKLIST.AUDIT_END_DATE%type);

  procedure P_GetCCQ(ENT_ID    in number,
                     P_NO      in number,
                     R_ID      in number,
                     io_cursor OUT t_cursor);

  procedure P_UpdateCCQ(CID                  IN NUMBER,
                        QUESTIONS            in varchar2,
                        CONTROL_VIOLATION_ID in number,
                        RISK_ID              in number,
                        STATUS               in varchar2,
                        ENT_ID               in number,
                        P_NO                 in number,
                        R_ID                 in number);

  procedure P_GetAuditeeEntities(ENTITYID  IN NUMBER,
                                 TYPEID    IN NUMBER,
                                 io_cursor OUT t_cursor);

end PKG_PG;

create or replace package body PKG_PG is

  Procedure P_get_Criteria_ent_count(CID IN NUMBER, io_cursor OUT t_cursor) is
  begin
    update t_audit_criteria t
       set t.no_of_entity =
           (select count(*)
              from t_audit_criteria a
             inner join t_au_period p
                on a.auditperiodid = p.auditperiodid
             inner join t_auditee_entities e
                on a.entity_typeid = e.type_id
               and e.risk_id = a.risk_id
               and a.size_id = e.size_id
               and a.entity_id = case
                     when e.type_id not in (6,28) then
                      e.entity_id
                     else
                      a.entity_id
                   end
             inner join t_audit_frequency f
                on a.frequency_id = f.frequency_id

             where a.id = t.id)
     WHERE t.id = CID;
    commit;
  
    OPEN io_cursor FOR
      SELECT c.NO_OF_ENTITY FROM t_audit_criteria c WHERE c.id = CID;
  
  end P_get_Criteria_ent_count;

  procedure P_GetAuditPeriods(ENT_ID    in number,
                              P_NO      in number,
                              R_ID      in number,
                              io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select p.auditperiodid,
             p.description,
             p.start_date,
             p.end_date,
             p.status_id,
             s.status
        from T_AU_PERIOD p
       inner join t_au_period_status s
          on p.status_id = s.id
       where p.status_id = 2
       order by p.AUDITPERIODID asc;
  
  end P_GetAuditPeriods;

  procedure P_GetAuditEntities(ENTITYID IN NUMBER, io_cursor OUT t_cursor) as
  begin
    if (ENTITYID in (112242, 112248)) then
      OPEN io_cursor FOR
        SELECT et.autid,
               et.entitycode,
               et.entitytypedesc,
               et.auditable,
               et.auditedby,
               et.audited_by_enitity,
               'Y' as d_risk
          FROM t_auditee_ent_types et
         where et.auditable = 'A'
           AND ET.AUDITED_BY_ENITITY = ENTITYID;
    else
      if (ENTITYID = 112243) THEN
        OPEN io_cursor FOR
          select t.autid,
                 t.entitycode,
                 t.entitytypedesc,
                 t.auditable,
                 t.auditedby,
                 t.audited_by_enitity,
                 'Y' as d_risk
            from T_AUDITEE_ENT_TYPES t
           where t.autid in (6, 28);
      ELSE
        OPEN io_cursor FOR
          select t.autid,
                 t.entitycode,
                 t.entitytypedesc,
                 t.auditable,
                 t.auditedby,
                 t.audited_by_enitity,
                 'N' as d_risk
            from T_AUDITEE_ENT_TYPES t
           where t.autid in (5, 7, 17, 25, 21, 20, 23, 22);
      end if;
    end if;
  end P_GetAuditEntities;

  procedure P_GetAuditeeEntities(ENTITYID  IN NUMBER,
                                 TYPEID    IN NUMBER,
                                 io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      Select G.ENTITYTYPEDESC AS ENTITY_TYPE, E.ENTITY_ID, E.NAME
        FROM t_auditee_entities e
       INNER JOIN t_auditee_ent_types G
          ON g.entitycode = e.type_id
       WHERE e.type_id = TYPEID
         and e.active = 'Y'
         and e.auditby_id = ENTITYID;
  
  end P_GetAuditeeEntities;

  procedure P_AddAuditPeriod(DESCRIPTION in T_AU_PERIOD.DESCRIPTION%type,
                             START_DATE  in T_AU_PERIOD.START_DATE%type,
                             END_DATE    in T_AU_PERIOD.End_Date%type,
                             io_cursor   OUT t_cursor) is
    A_P NUMBER := 0;
  
  begin
  
    /* SELECT nvl(max(P.AUDITPERIODID), 0)
     INTO A_P
     FROM T_AU_PERIOD P
    WHERE P.START_DATE BETWEEN START_DATE AND END_DATE
       OR P.END_DATE BETWEEN START_DATE AND END_DATE; */
  
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

  Procedure P_GET_Auditperiod_status(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select s.id, s.status from t_au_period_status s;
  
  end P_GET_Auditperiod_status;

  Procedure p_update_Auditperiod(P_ID      in number,
                                 S_ID      in number,
                                 io_cursor OUT t_cursor) is
  
  begin
  
    update t_au_period p
       set p.status_id = S_ID
     where p.auditperiodid = P_ID;
    commit;
  
    open io_cursor for
      Select 'Audit Period Status Updated' as remarks from dual;
  
  end p_update_Auditperiod;

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

  procedure P_ADDAUDITCRITERIA(ENTITYTYPEID   in T_AUDIT_CRITERIA.ENTITY_TYPEID%type,
                               SIZEID         in T_AUDIT_CRITERIA.SIZE_ID%type,
                               RISKID         in T_AUDIT_CRITERIA.RISK_ID%type,
                               FREQUENCYID    in T_AUDIT_CRITERIA.FREQUENCY_ID%type,
                               NOOFDAYS       in T_AUDIT_CRITERIA.NO_OF_DAYS%type,
                               visit          in T_AUDIT_CRITERIA.VISIT%type,
                               APPROVALSTATUS in T_AUDIT_CRITERIA.APPROVAL_STATUS%type,
                               AUDITPERIODID  in T_AUDIT_CRITERIA.AUDITPERIODID%type,
                               ENTITYID       in T_AUDIT_CRITERIA.CREATED_BY%type,
                               REMARKS        in T_AUDIT_CRITERIA_LOG.REMARKS%type,
                               P_NO           in number,
                               ENT_ID         in number,
                               R_ID           in number,
                               io_cursor      OUT t_cursor) is
    A_C number := 0;
    C_A number := 0;
    E_F number := 0;
    E_C number := 0;
  begin
  
    SELECT COALESCE(MAX(acc.id) + 1, 1) INTO e_c FROM t_audit_criteria acc;
  
    select count(a.id)
      into A_C
      from t_audit_criteria a
     where a.entity_typeid = ENTITYTYPEID
       and a.auditperiodid = AUDITPERIODID
       and a.size_id = SIZEID
       and a.risk_id = RISKID
       and a.entity_id = 0
       and a.approval_status = 1;
  
    select count(a.id)
      into C_A
      from t_audit_criteria a
     where a.entity_typeid = ENTITYTYPEID
       and a.auditperiodid = AUDITPERIODID
       and a.entity_id = entityid
       and a.entity_id != 0
       AND A.APPROVAL_STATUS = 1;
  
    if (A_C = 0 AND C_A = 0) then
      if (ENTITYTYPEID in (6, 28)) then
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
           a.CRITERIA_SUBMITTED)
        VALUES
          (e_c,
           ENTITYTYPEID,
           entityid,
           SIZEID,
           RISKID,
           FREQUENCYID,
           NOOFDAYS,
           VISIT,
           APPROVALSTATUS,
           AUDITPERIODID,
           ENT_ID,
           'N');
        commit;
        update t_audit_criteria t
           set t.no_of_entity =
               (               
                select count(*)
                  from t_audit_criteria a
                 inner join t_au_period p
                    on a.auditperiodid = p.auditperiodid
                 inner join t_auditee_entities e
                    on a.entity_typeid = e.type_id
                   and e.risk_id = a.risk_id
                   and a.size_id = e.size_id
                 inner join t_audit_frequency f
                    on a.frequency_id = f.frequency_id
                 where a.id = t.id);
        commit;
      
        INSERT INTO T_AUDIT_CRITERIA_LOG al
          (al.ID,
           al.C_ID,
           al.STATUS_ID,
           al.CREATEDBY_ID,
           al.CREATED_ON,
           al.REMARKS)
        VALUES
          ((select max(acc1.ID) + 1 from T_AUDIT_CRITERIA_LOG acc1),
           e_c,
           1,
           P_NO,
           SYSDATE,
           REMARKS);
        commit;
        open io_cursor for
          select m.ref, m.remarks from t_au_remarks m where m.id = 13;
      
      else
        if (ENTITYTYPEID in (25)) then
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
            (e_c,
             ENTITYTYPEID,
             entityid,
             SIZEID,
             RISKID,
             '1',
             NOOFDAYS,
             VISIT,
             APPROVALSTATUS,
             AUDITPERIODID,
             ENT_ID,
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
            ((select max(acc1.ID) + 1 from T_AUDIT_CRITERIA_LOG acc1),
             e_c,
             1,
             P_NO,
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
             a.CRITERIA_SUBMITTED,
             a.entity_id,
             a.no_of_entity)
          VALUES
            (e_c,
             ENTITYTYPEID,
             SIZEID,
             RISKID,
             FREQUENCYID,
             NOOFDAYS,
             VISIT,
             APPROVALSTATUS,
             AUDITPERIODID,
             ENT_ID,
             'N',
             ENTITYID,
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
            ((select max(acc1.ID) + 1 from T_AUDIT_CRITERIA_LOG acc1),
             e_c,
             
             1,
             P_NO,
             SYSDATE,
             REMARKS);
          commit;
        
          open io_cursor for
            select m.ref, m.remarks from t_au_remarks m where m.id = 13;
        end if;
      end if;
    
    else
      open io_cursor for
      
        select m.ref, m.remarks from t_au_remarks m where m.id = 14;
    end if;
  
  end P_ADDAUDITCRITERIA;

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

  procedure P_DeletePendingCriteria(ENT_ID in number,
                                    P_NO   in number,
                                    R_ID   in number,
                                    CID    IN NUMBER) is
    V_F number := 0;
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
       3,
       'Delete Pending Criteria',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select NVL(MAX(cc.entity_id), 0)
      into V_F
      from t_audit_criteria cc
     where cc.id = cid;
    Delete FROM t_audit_criteria c where c.id = cid;
    COMMIT;
  
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
       V_F,
       (SELECT MAX(acc.createdby_id)
          from T_AUDIT_CRITERIA_LOG acc
         where acc.c_id = CID),
       (SELECT max(acc.created_on)
          from T_AUDIT_CRITERIA_LOG acc
         where acc.c_id = CID),
       'Audit Criteria Deleted',
       '0',
       SYSDATE);
    commit;
  
  end P_DeletePendingCriteria;

  procedure P_UpdateAuditCriteria(CID            IN t_audit_criteria.id%type,
                                  ENTITYTYPEID   IN T_AUDIT_CRITERIA.ENTITY_TYPEID%TYPE,
                                  SIZEID         IN T_AUDIT_CRITERIA.SIZE_ID%TYPE,
                                  RISKID         IN T_AUDIT_CRITERIA.RISK_ID%TYPE,
                                  FREQUENCYID    IN T_AUDIT_CRITERIA.FREQUENCY_ID%TYPE,
                                  NOOFDAYS       IN T_AUDIT_CRITERIA.NO_OF_DAYS%TYPE,
                                  VISITS         IN T_AUDIT_CRITERIA.VISIT%TYPE,
                                  AUDITPERIOD_ID IN T_AUDIT_CRITERIA.AUDITPERIODID%TYPE,
                                  REMARKS        IN VARCHAR2,
                                  ENT_ID         in number,
                                  P_NO           in number,
                                  R_ID           in number) is
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
       3,
       'Updated Audit Criteria',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    UPDATE T_AUDIT_CRITERIA a
       SET a.ENTITY_TYPEID   = ENTITYTYPEID,
           a.FREQUENCY_ID    = FREQUENCYID,
           a.NO_OF_DAYS      = NOOFDAYS,
           a.VISIT           = VISITS,
           a.size_id         = SIZEID,
           a.risk_id         = RISKID,
           a.APPROVAL_STATUS = 3
    
     WHERE a.ID = CID
       and a.auditperiodid = AUDITPERIODID;
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
       P_NO,
       SYSDATE);
    commit;
  end P_UpdateAuditCriteria;

  procedure P_SetAuditCriteriaStatusReferredBack(CID     IN t_audit_criteria.id%type,
                                                 REMARKS IN VARCHAR2,
                                                 ENT_ID  in number,
                                                 P_NO    in number,
                                                 R_ID    in number) is
  
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
       4,
       'Audit Criteria Rejected/Referred Back',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
         P_NO,
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
         P_NO,
         SYSDATE);
      commit;
    END IF;
  
  end P_SetAuditCriteriaStatusReferredBack;

  procedure P_SubmitAuditCriteriaForApproval(ENT_ID in number,
                                             P_NO   in number,
                                             R_ID   in number,
                                             CID    IN NUMBER) is
  
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
       3,
       'Submitted Audit Criteria for Approval',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    Update t_audit_criteria c
       SET c.CRITERIA_SUBMITTED = 'Y'
     where c.created_by = CID
       and c.criteria_submitted = 'N';
    COMMIT;
  end P_SubmitAuditCriteriaForApproval;

  procedure P_SetAuditCriteriaStatusApprove(CAID      IN t_audit_criteria.id%type,
                                            REMARKS   IN VARCHAR2,
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
       4,
       'Audit Criteria Rejected/Referred Back',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    if (R_ID in (3, 5)) then
      UPDATE T_AUDIT_CRITERIA a
         SET a.APPROVAL_STATUS = 4
       WHERE a.ID = CAID;
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
          ((select COALESCE(max(acc.ID) + 1, 1)
             from T_AUDIT_CRITERIA_LOG acc),
           CAID,
           4,
           (SELECT Max(acc.createdby_id)
              from T_AUDIT_CRITERIA_LOG acc
             where acc.C_id = CAID),
           (SELECT max(acc.created_on)
              from T_AUDIT_CRITERIA_LOG acc
             where acc.C_id = CAID),
           REMARKS,
           P_NO,
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
          ((select COALESCE(max(acc.ID) + 1, 1)
             from T_AUDIT_CRITERIA_LOG acc),
           CAID,
           4,
           (SELECT Max(acc.createdby_id)
              from T_AUDIT_CRITERIA_LOG acc
             where acc.C_id = CAID),
           (SELECT max(acc.created_on)
              from T_AUDIT_CRITERIA_LOG acc
             where acc.C_id = CAID),
           'Approved',
           P_NO,
           SYSDATE);
        commit;
      end if;
      open io_cursor for
        select 'Criteria Approved' as remark from dual;
    else
      open io_cursor for
        select 'You have not right to approve the said criteria' as remark
          from dual;
    end if;
  end P_SetAuditCriteriaStatusApprove;

  procedure P_GetAuditCriteriaLogLastStatus(ID        IN NUMBER,
                                            io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select remarks
        from T_AUDIT_CRITERIA_LOG l
       where l.c_id = ID
       order by l.id desc
       FETCH NEXT 1 ROWS ONLY;
  
  end P_GetAuditCriteriaLogLastStatus;

  procedure P_GetPendingAuditCriterias(ENT_ID    IN NUMBER,
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
       5,
       'Get Pending Audit Criterias',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    select nvl(max(m.entity_id), 0)
      into V_F
      from t_auditee_entities_maping m
     where m.parent_id = ENT_ID;
  
    if (ENT_ID = 112243) then
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
               (select tt.entitytypedesc
                  from t_auditee_ent_types tt
                 where tt.autid = ac.entity_typeid) as ENTITY,
               r.description as RISK,
               f.frequency_discription as FREQUENCY,
               s.description as BRSIZE,
               ac.entity_id,
               (select tt.entitytypedesc
                  from t_auditee_ent_types tt
                 where tt.autid = ac.entity_typeid) as name,
               
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
         inner join t_auditee_entities_maping m
            on m.entity_id = ac.created_by
          left join t_auditee_entities_size_disc s
            on ac.size_id = s.entity_size
          LEFT JOIN T_AUDIT_CRITERIA_LOG L
            ON L.C_ID = AC.ID
         WHERE ac.CRITERIA_SUBMITTED = 'N'
           and m.entity_id = ENT_ID
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
    
    else
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
               (case
                 when ac.entity_typeid = 6 then
                  et.entitytypedesc
                 else
                  (select ee.name
                     from t_auditee_entities ee
                    where ee.entity_id = ac.entity_id)
               end) as ENTITY,
               -- et.entitytypedesc as ENTITY,
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
          left join t_risk r
            on ac.risk_id = r.r_id
         inner join t_audit_frequency f
            on ac.frequency_id = f.frequency_id
          left join t_auditee_entities_size_disc s
            on ac.size_id = s.entity_size
          LEFT JOIN T_AUDIT_CRITERIA_LOG L
            ON L.C_ID = AC.ID
         WHERE ac.CRITERIA_SUBMITTED = 'N'
           and ac.CREATED_BY = ENT_ID
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
    end if;
  
  end P_GetPendingAuditCriterias;

  procedure P_GetRefferedBackAuditCriterias(ENT_ID    in number,
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
       4,
       'Get Reffered Back Audit Criterias',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
         and ac.CREATED_BY = ENT_ID
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

  procedure P_GetAuditCriteriasToAuthorize(ENT_ID    in number,
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
       5,
       'Get Audit Criterias To Authorize',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    if (R_ID in (1, 3)) then
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
               (case
                 when ac.entity_typeid in (6, 28) then
                  et.entitytypedesc
                 else
                  (select ee.description
                     from t_auditee_entities ee
                    where ee.entity_id = ac.entity_id)
               end) as name,
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
    else
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
               (case
                 when ac.entity_typeid in (6, 28) then
                  et.entitytypedesc
                 else
                  (select ee.name
                     from t_auditee_entities ee
                    where ee.entity_id = ac.entity_id)
               end) as ENTITY,
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
          left join t_risk r
            on ac.risk_id = r.r_id
         inner join t_audit_frequency f
            on ac.frequency_id = f.frequency_id
         inner join t_auditee_entities_maping mp
            on mp.entity_id = ac.created_by
          left join t_auditee_entities_size_disc s
            on ac.size_id = s.entity_size
          LEFT JOIN T_AUDIT_CRITERIA_LOG L
            ON L.C_ID = AC.ID
         WHERE ac.APPROVAL_STATUS IN (1, 3, 6)
           and ac.CRITERIA_SUBMITTED = 'Y'
           and mp.parent_id = ENT_ID
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
    end if;
  end P_GetAuditCriteriasToAuthorize;

  procedure P_GetPostChangesAuditCriterias(ENT_ID    in number,
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
       7,
       'Get Post Changes Audit Criterias',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
    if (ENT_ID = 114243) then
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
           and ac.created_by = ENT_ID
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
    else
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
               (select ee.name
                  from t_auditee_entities ee
                 where ee.entity_id = ac.entity_id) as ENTITY,
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
           and ac.created_by = ENT_ID
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
    end if;
  
  end P_GetPostChangesAuditCriterias;

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
         order by e.CURRENT_RANK asc;
    END IF;
  end P_GetAuditEmployees;

  procedure P_AddAuditTeam(TEAMNAME      in T_AU_TEAM_MEMBERS.TEAM_NAME%type,
                           TEAMMEMBER_ID in T_AU_TEAM_MEMBERS.MEMBER_PPNO%type,
                           MAX_T_ID      IN T_AU_TEAM_MEMBERS.T_ID%TYPE,
                           EMPLOYEENAME  in T_AU_TEAM_MEMBERS.MEMBER_NAME%type,
                           IS_TEAMLEAD   in T_AU_TEAM_MEMBERS.ISTEAMLEAD%type,
                           STATUS        in T_AU_TEAM_MEMBERS.STATUS%type,
                           ENT_ID        in number,
                           P_NO          in number,
                           R_ID          in number,
                           io_cursor     OUT t_cursor) is
  
    typeid number := 0;
    E_F    number := 0;
    T_P    number := 0;
  begin
  
    select NVL(max(tm.t_id), 0)
      into T_P
      from t_au_team_members tm
     where tm.member_ppno = TEAMMEMBER_ID
       and tm.status = 'Y';
    --   if (T_P = 0) then
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
       11,
       'New Team Added',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
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
     where e.entity_id = ENT_ID;
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
    open io_cursor for
      select TEAMNAME || ' Team Created' as remarks from Dual;
  
    /* else
    open io_cursor for
    select TEAMMEMBER_ID||' is already member of active team, Please inactive before making new team' as remarks from Dual;
    end if;*/
  end P_AddAuditTeam;

  PROCEDURE P_DeleteAuditTeam(TID    IN NUMBER,
                              ENT_ID IN NUMBER,
                              P_NO   IN NUMBER,
                              R_ID   IN NUMBER) AS
    E_F number := 0;
  begin
  
    select NVL(MAX(l.id), 0)
      into E_F
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = E_F;
    commit;
    insert into t_au_activity_log
      (id, entity_id, role_id, ppnum, page_id, action, seq, unattend)
    VALUES
      ((select COALESCE(max(p.ID) + 1, 1) from t_au_activity_log p),
       ENT_ID,
       R_ID,
       P_NO,
       11,
       'Team Deleted/Marked In-Active',
       
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
    UPDATE T_AU_TEAM_MEMBERS T SET T.STATUS = 'N' WHERE T.T_ID = TID;
    COMMIT;
  END P_DeleteAuditTeam;

  PROCEDURE P_MAXTEAMID(ENT_ID    in number,
                        P_NO      in number,
                        R_ID      in number,
                        io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT COALESCE(max(PP.T_ID) + 1, 1) AS MAX_T_ID
        FROM T_AU_TEAM_MEMBERS PP;
  END P_MAXTEAMID;

  /*  procedure p_Getauditteams(ENGID in number, io_cursor OUT t_cursor) is
  
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
  */
  procedure P_GetAuditTeams(dept_code    IN NUMBER,
                            UserEntityID IN NUMBER,
                            P_NO         in number,
                            R_ID         in number,
                            io_cursor    OUT t_cursor) as
  
  begin
  
    IF (dept_code != 0) THEN
      OPEN io_cursor FOR
        select t.*, d.name as AUDIT_DEPARTMENT
          from t_au_team_members t
         inner join t_auditee_entities d
            on d.entity_id = t.PLACE_OF_POSTING
         Where d.entity_id = dept_code
           and t.status = 'Y'
         order by t.status desc, t.ISTEAMLEAD desc;
    ELSE
      OPEN io_cursor FOR
        select t.*, d.name as AUDIT_DEPARTMENT
          from t_au_team_members t
         inner join t_auditee_entities d
            on d.entity_id = t.PLACE_OF_POSTING
         Where t.PLACE_OF_POSTING = UserEntityID
           and t.status = 'Y'
         order by t.status desc, t.ISTEAMLEAD desc;
    END IF;
  
  end P_GetAuditTeams;

  procedure p_GetAuditFrequencies(io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select * from T_AUDIT_FREQUENCY F WHERE F.STATUS = 'Y' order by F.ID;
  
  end p_GetAuditFrequencies;

  Procedure P_ADD_Special_Audit_Plan(P_ID           in number,
                                     NOOFDAYS       in number,
                                     Nature         in number,
                                     AUDITPERIOD_ID in number,
                                     ENTITYID       in number,
                                     IND            in varchar2,
                                     P_NO           in number,
                                     ENT_ID         in number,
                                     R_ID           in number,
                                     io_cursor      OUT t_cursor) is
    A_C    number := 0;
    A_type number := 0;
  begin
    if (IND = 'N') then
      select count(a.id)
        into A_C
        from T_AU_pre_PLAN a
       where a.auditperiodid = AUDITPERIODID
         and a.entity_id = ENTITYID;
      select e.type_id
        into a_type
        from t_auditee_entities e
       where e.entity_id = ENTITYID;
      if (A_C = 0) then
        INSERT INTO T_AU_pre_PLAN
          (Id,
           Criteria_Id,
           Auditperiodid,
           Auditedby,
           Entity_Id,
           Auditee_Risk,
           Auditee_Size,
           No_Of_Days,
           Entity_Typeid,
           
           fr_id,
           Status,
           Nature_Id)
        VALUES
          ((select COALESCE(max(acc.ID) + 1, 1) from T_AU_pre_PLAN acc),
           0,
           AUDITPERIOD_ID,
           ENT_ID,
           ENTITYID,
           1,
           1,
           NOOFDAYS,
           a_type,
           1,
           1,
           nature);
        commit;
        open io_cursor for
          select 'Request for Approval Submited' remarks from dual;
      
      else
        open io_cursor for
          select 'Already Submited' remarks from dual;
      end if;
    else
      if (IND = 'E') then
        update T_AU_pre_PLAN p
           set p.auditperiodid = AUDITPERIOD_ID,
               p.no_of_days    = NOOFDAYS,
               p.nature_id     = nature,
               p.status        = 1
         where p.id = P_ID;
        commit;
        open io_cursor for
          select 'Plan Updated' remarks from dual;
      end if;
    end if;
  
  end P_ADD_Special_Audit_Plan;

  procedure P_GET_Specical_Audit_for_Approval(ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select az.name          as auditor,
             az.entity_id     as auditor_id,
             m.p_name         as reporting,
             m.parent_id      as reporting_id,
             m.c_name         as auditee,
             m.entity_id      as auditee_id,
             n.description    as nature,
             n.nid            as nature_id,
             ap.description   as period,
             ap.auditperiodid as period_id,
             p.no_of_days,
             p.id             as P_ID
        from t_au_pre_plan p
       inner join t_au_period ap
          on ap.auditperiodid = p.auditperiodid
       inner join t_audit_frequency f
          on f.frequency_id = p.fr_id
       inner join t_auditee_entities_maping m
          on m.entity_id = p.entity_id
       inner join t_auditee_ent_types t
          on t.autid = m.c_type_id
       inner join t_auditee_entities az
          on az.entity_id = p.auditedby
       inner join t_au_old_audit_nature n
          on n.nid = p.nature_id
       where ((p.status = case
               when ent_id = 112243 then
                2
               else
                1
             end) or (p.status = case
               when ent_id = 112243 then
                2
               else
                3
             end))
         and p.auditedby = case
               when ent_id = 112243 then
                p.auditedby
               else
                ent_id
             end;
  
  end P_GET_Specical_Audit_for_Approval;

  Procedure P_Update_Special_Audit(P_ID      in number,
                                   IND       in varchar2,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) is
  begin
  
    If (IND = 'S') then
      update t_au_pre_plan p set p.status = 2 where p.id = P_ID;
      commit;
      open io_cursor for
        select 'Plan Submitted' remarks from dual;
    else
      If (IND = 'D') then
        delete t_au_pre_plan p where p.id = P_ID;
        commit;
        open io_cursor for
          select 'Plan Deleted' remarks from dual;
      else
        if (IND = 'R') then
          update t_au_pre_plan p set p.status = 3 where p.id = P_ID;
          commit;
          open io_cursor for
            select 'Plan refered back' remarks from dual;
        else
          if (IND = 'A') then
            update t_au_pre_plan p set p.status = 4 where p.id = P_ID;
            commit;
            INSERT INTO T_AU_PLAN
              (ID,
               CRITERIA_ID,
               AUDITPERIODID,
               AUDITEDBY,
               ENTITY_ID,
               ENTITY_CODE,
               AUDITEE_RISK,
               AUDITEE_SIZE,
               NO_OF_DAYS,
               FR_ID,
               --COST_CENTER,
               ENTITY_TYPEID,
               STATUS,
               NATURE_ID,
               f_id)
              select SEQ_AU_IID_INV_PLAN_ID.NEXTVAL,
                     p.criteria_id,
                     p.auditperiodid,
                     p.auditedby,
                     p.entity_id,
                     m.child_code,
                     1,
                     1,
                     p.no_of_days,
                     p.fr_id,
                     p.entity_typeid,
                     1,
                     p.nature_id,
                     1
                from t_au_pre_plan p
               inner join t_au_period ap
                  on ap.auditperiodid = p.auditperiodid
               inner join t_auditee_entities_maping m
                  on m.entity_id = p.entity_id
               inner join t_auditee_entities az
                  on az.entity_id = p.auditedby
               where p.id = P_ID;
            commit;
          
            open io_cursor for
              select 'Approved and Plan Generated' remarks from dual;
          end if;
        end if;
      end if;
    end if;
  end P_Update_Special_Audit;

  procedure Tentative_Audit_Plan(CRITERIA_ID in t_audit_criteria.id%type,
                                 ENT_ID      in number,
                                 P_NO        in number,
                                 R_ID        in number,
                                 io_cursor   OUT t_cursor) is
    v_f number;
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
       6,
       'Generate Plan For Audit Criteria',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    for j in (select * from t_audit_criteria acl where acl.id = CRITERIA_ID) loop
    
      select time
        into v_f
        from T_AUDIT_FREQUENCY af
       where af.frequency_id = (select ac.frequency_id
                                  from t_audit_criteria ac
                                 where ac.id = j.id);
    
      if (j.entity_id is not null and j.entity_id != 0) then
        for i in 1 .. v_f loop
          INSERT INTO T_AU_PLAN
            (ID,
             CRITERIA_ID,
             AUDITPERIODID,
             AUDITEDBY,
             ENTITY_ID,
             ENTITY_CODE,
             AUDITEE_RISK,
             AUDITEE_SIZE,
             NO_OF_DAYS,
             FR_ID,
             --COST_CENTER,
             ENTITY_TYPEID,
             STATUS,
             NATURE_ID,
             f_id)
            select SEQ_AU_IID_INV_PLAN_ID.NEXTVAL,
                   a.id,
                   a.auditperiodid,
                   e.auditby_id,
                   a.entity_id,
                   e.code,
                   a.risk_id,
                   a.size_id,
                   a.no_of_days,
                   a.frequency_id,
                   --e.cost_center,
                   e.type_id,
                   '1',
                   1,
                   1
              from t_audit_criteria a
             inner join t_au_period p
                on a.auditperiodid = p.auditperiodid
             inner join t_auditee_entities e
                on a.entity_id = e.entity_id
             where a.id = CRITERIA_ID
               and e.auditable = 'Y';
          commit;
        end loop;
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
               --COST_CENTER,
               ENTITY_TYPEID,
               STATUS,
               NATURE_ID,
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
                     --e.cost_center,
                     e.type_id,
                     '1',
                     1,
                     i
                from t_audit_criteria a
               inner join t_au_period p
                  on a.auditperiodid = p.auditperiodid
               inner join t_auditee_entities e
                  on a.entity_typeid = e.type_id
                 and a.size_id = e.size_id
                 and a.risk_id = e.risk_id
               inner join t_audit_frequency f
                  on a.frequency_id = f.frequency_id
               inner join t_auditee_entities_size_disc ess
                  on ess.entity_size = e.size_id
               inner join t_risk_status ers
                  on ers.r_id = e.risk_id
               where e.auditable = 'Y'
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
               --COST_CENTER,
               ENTITY_TYPEID,
               STATUS,
               NATURE_ID,
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
                     --e.cost_center,
                     e.type_id,
                     '1',
                     1,
                     i
                from t_audit_criteria a
               inner join t_au_period p
                  on a.auditperiodid = p.auditperiodid
               inner join t_auditee_entities e
                  on a.entity_typeid = e.type_id
                 and a.size_id = e.size_id
                 and a.risk_id = e.risk_id
              
               inner join t_audit_frequency f
                  on a.frequency_id = f.frequency_id
               inner join t_auditee_entities_size_disc ess
                  on ess.entity_size = e.size_id
               inner join t_risk_status ers
                  on ers.r_id = e.risk_id
               where e.auditable = 'Y'
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

  procedure p_get_audit_plan(ENT_ID    in number,
                             P_NO      in number,
                             R_ID      in number,
                             io_cursor OUT t_cursor) as
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
       6,
       'Get Tentative Plans For Fields',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    IF ENT_ID NOT IN ('112243', '112201', '113068') THEN
    
      OPEN io_cursor FOR
        SELECT a.criteria_id           AS CRITERIA_ID,
               et.entitytypedesc       AS ent_type,
               a.ID                    AS PLAN_ID,
               a.AUDITPERIODID         AS AUDITPERIODID,
               e.auditby_id            AS AUDITEDBY,
               ess.description         AS AUDITEE_SIZE,
               ers.description         AS AUDITEE_RISK,
               a.no_of_days            AS NO_OF_DAYS,
               e.entity_id             AS ENTITY_ID,
               e.type_id               AS ENTITY_TYPE_ID,
               e.code                  AS ENTITY_CODE,
               mp.p_name               AS Reporting_office,
               e.name                  AS AUDITEE_NAME,
               f.frequency_discription AS FREQUENCY_DISCRIPTION,
               p.description           AS PERIOD_NAME,
               n.description           AS nature_of_audit
          FROM t_au_plan a
         INNER JOIN t_au_period p
            ON a.auditperiodid = p.auditperiodid
         INNER JOIN t_auditee_entities e
            ON a.entity_id = e.entity_id
         INNER JOIN t_audit_departments d
            ON d.entity_id = e.auditby_id
         INNER JOIN t_audit_frequency f
            ON a.f_id = f.frequency_id
         INNER JOIN t_auditee_entities_size_disc ess
            ON ess.entity_size = a.auditee_size
         INNER JOIN t_risk_status ers
            ON ers.r_id = a.auditee_risk
         INNER JOIN t_au_old_audit_nature n
            ON n.nid = a.nature_id
         INNER JOIN t_auditee_ent_types et
            ON a.entity_typeid = et.autid
          LEFT JOIN t_auditee_entities_maping mp
            ON mp.entity_id = a.entity_id
         WHERE a.status = 1
           AND a.auditedby = ENT_ID
         ORDER BY a.auditee_risk ASC;
    
    ELSIF ENT_ID = '112243' THEN
    
      OPEN io_cursor FOR
        SELECT a.criteria_id           AS CRITERIA_ID,
               a.ID                    AS PLAN_ID,
               et.entitytypedesc       AS ent_type,
               a.AUDITPERIODID         AS AUDITPERIODID,
               e.auditby_id            AS AUDITEDBY,
               ess.description         AS AUDITEE_SIZE,
               ers.description         AS AUDITEE_RISK,
               a.no_of_days            AS NO_OF_DAYS,
               e.entity_id             AS ENTITY_ID,
               e.type_id               AS ENTITY_TYPE_ID,
               e.code                  AS ENTITY_CODE,
               mp.p_name               AS Reporting_office,
               e.name                  AS AUDITEE_NAME,
               f.frequency_discription AS FREQUENCY_DISCRIPTION,
               p.description           AS PERIOD_NAME,
               n.description           AS nature_of_audit
          FROM t_au_plan a
         INNER JOIN t_au_period p
            ON a.auditperiodid = p.auditperiodid
         INNER JOIN t_auditee_entities e
            ON a.entity_id = e.entity_id
         INNER JOIN t_audit_departments d
            ON d.entity_id = e.auditby_id
         INNER JOIN t_audit_frequency f
            ON a.f_id = f.frequency_id
         INNER JOIN t_auditee_entities_size_disc ess
            ON ess.entity_size = a.auditee_size
         INNER JOIN t_risk_status ers
            ON ers.r_id = a.auditee_risk
         INNER JOIN t_au_old_audit_nature n
            ON n.nid = a.nature_id
         INNER JOIN t_auditee_ent_types et
            ON a.entity_typeid = et.autid
          LEFT JOIN t_auditee_entities_maping mp
            ON mp.entity_id = a.entity_id
         WHERE a.status IS NOT NULL
           AND d.deptname LIKE '%AUDIT ZONE%'
         ORDER BY a.auditee_risk ASC;
    
    ELSIF ENT_ID IN ('112201', '113068') THEN
    
      OPEN io_cursor FOR
        SELECT a.criteria_id           AS CRITERIA_ID,
               a.ID                    AS PLAN_ID,
               et.entitytypedesc       AS ent_type,
               a.AUDITPERIODID         AS AUDITPERIODID,
               e.auditby_id            AS AUDITEDBY,
               ess.description         AS AUDITEE_SIZE,
               ers.description         AS AUDITEE_RISK,
               a.no_of_days            AS NO_OF_DAYS,
               e.entity_id             AS ENTITY_ID,
               e.type_id               AS ENTITY_TYPE_ID,
               e.code                  AS ENTITY_CODE,
               mp.p_name               AS Reporting_office,
               e.name                  AS AUDITEE_NAME,
               f.frequency_discription AS FREQUENCY_DISCRIPTION,
               p.description           AS PERIOD_NAME,
               n.description           AS nature_of_audit
          FROM t_au_plan a
         INNER JOIN t_au_period p
            ON a.auditperiodid = p.auditperiodid
         INNER JOIN t_auditee_entities e
            ON a.entity_id = e.entity_id
         INNER JOIN t_audit_departments d
            ON d.entity_id = e.auditby_id
         INNER JOIN t_audit_frequency f
            ON a.f_id = f.frequency_id
         INNER JOIN t_auditee_entities_size_disc ess
            ON ess.entity_size = a.auditee_size
         INNER JOIN t_risk_status ers
            ON ers.r_id = a.auditee_risk
         INNER JOIN t_au_old_audit_nature n
            ON n.nid = a.nature_id
         INNER JOIN t_auditee_ent_types et
            ON a.entity_typeid = et.autid
          LEFT JOIN t_auditee_entities_maping mp
            ON mp.entity_id = a.entity_id
         WHERE a.status IS NOT NULL
         ORDER BY a.auditee_risk ASC;
    
    END IF;
  
  end p_get_audit_plan;

  procedure P_GetAuditOperationalStartDate(entityCode    IN NUMBER,
                                           auditPeriodId IN NUMBER,
                                           io_cursor     OUT t_cursor) as
    eg_plan number := 0;
  begin
    select nvl(max(e.eng_id), 0)
      into EG_PLAN
      from t_au_plan_eng e
     where e.entity_code = entityCode;
    OPEN io_cursor FOR
      select EXTRACT(year FROM d.operation_enddate) as year,
             EXTRACT(month FROM d.operation_enddate) as month,
             EXTRACT(day FROM d.operation_enddate) as day
        FROM t_Au_Plan_Eng d
       WHERE d.eng_id = eg_plan;
  
  end P_GetAuditOperationalStartDate;

  procedure P_AddAuditEngagementPlan(PERIODID        in number,
                                     ENTITYID        in number,
                                     AUDIT_STARTDATE in date,
                                     CREATEDBY       in number,
                                     AUDIT_ENDDATE   in date,
                                     STATUS          in varchar2,
                                     TEAMID          in number,
                                     TEAM_NAME       in varchar2,
                                     PLANID          IN NUMBER,
                                     OP_STARTDATE    in date,
                                     OP_ENDDATE      in date,
                                     TRAVELDAY       in number,
                                     RRDAY           in number,
                                     D_Day           in number,
                                     ENT_ID          in number,
                                     P_NO            in number,
                                     R_ID            in number,
                                     io_cursor       OUT t_cursor) is
    E_P number := 0;
    T_I number := 0;
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
       2,
       'Add Audit Engagement Plan',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
        
          UPDATE T_AU_PLAN P SET P.STATUS = 2 WHERE P.ID = PLANID;
          commit;
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

  procedure P_RerecommendAuditEngagementPlan(ENGID        in number,
                                             ENTITYID     in number,
                                             STARTDATE    in date,
                                             ENDDATE      in date,
                                             TEAMID       in number,
                                             PLANID       IN NUMBER,
                                             OP_STARTDATE in date,
                                             OP_ENDDATE   in date,
                                             REMARKS      in varchar2,
                                             ENT_ID       in number,
                                             P_NO         in number,
                                             R_ID         in number,
                                             io_cursor    OUT t_cursor) is
  
    T_I     number := 0;
    V_F     NUMBER := 1;
    team_id number := 0;
    E_F     number := 0;
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
       71,
       'Rerecommend Audit Engagement Plan',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    if (OP_STARTDATE = OP_ENDDATE or OP_STARTDATE is null) then
      open io_cursor for
        select r.ref, r.remarks as remark
          from t_au_remarks r
         where r.id = 25;
    else
      DELETE FROM T_AU_AUDIT_TEAM_TASKLIST TT WHERE TT.ENG_PLAN_ID = ENGID;
      COMMIT;
    
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
               a.LASTUPDATEDBY       = P_NO,
               a.LASTUPDATEDDATE     = SYSDATE,
               A.OPERATION_STARTDATE = OP_STARTDATE,
               A.OPERATION_ENDDATE   = OP_ENDDATE
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
           P_NO,
           SYSDATE,
           REMARKS);
        COMMIT;
      
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
             (SELECT distinct (team_name)
                from t_au_team_members m
               where m.t_id = TEAMID),
             TEAMID,
             ENTITYID,
             1);
          COMMIT;
        else
          team_id := -1;
        end if;
      
        UPDATE T_AU_PLAN P SET P.STATUS = 2 WHERE P.ID = PLANID;
        COMMIT;
      
        for JJ in (SELECT * FROM t_au_team_members MT where MT.t_id = TEAMID) loop
        
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
                     (SELECT COALESCE(max(lp.sequence_no) + 1, 1)
                        FROM T_AU_AUDIT_TEAM_TASKLIST lp
                       where lp.teammember_ppno = JJ.MEMBER_PPNO),
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
      
        open io_cursor for
          select r.ref, r.remarks as remark
            from t_au_remarks r
           where r.id = 26;
      else
        open io_cursor for
          select r.ref, r.remarks as remark
            from t_au_remarks r
           where r.id = 2;
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
       order by l.id desc
       FETCH NEXT 1 ROWS ONLY;
  
  end P_GetLatestCommentsOnEngagement;

  procedure P_GetAuditEngagementPlans(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      io_cursor OUT t_cursor) as
  
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
       12,
       'Get Audit Engagement Plans',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
         and e.auditby_id = ENT_ID;
  
  end P_GetAuditEngagementPlans;

  procedure P_GetRefferedBackAuditEngagementPlans(ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor) as
  
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
       3,
       'Get Reffered Back Audit Engagement Plans',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
             E.OPERATION_ENDDATE   AS OP_ENDDATE,
             e.auditby_id
        from t_au_plan_eng e
       inner join t_auditee_entities ee
          on e.entity_id = ee.entity_id
       inner join t_au_audit_teams tt
          on tt.eng_id = e.eng_id
       where e.STATUS IN (6, 9)
         and e.auditby_id = ENT_ID;
  
  end P_GetRefferedBackAuditEngagementPlans;

  procedure P_AddAuditteamtasklist(TEAMID   in number,
                                   PLANID   IN NUMBER,
                                   ENTITYID IN NUMBER,
                                   ENT_ID   in number,
                                   P_NO     in number,
                                   R_ID     in number) is
    V_F NUMBER := 1;
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
       2,
       'Add Audit/Assign Engagement to team task list',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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

  procedure P_RefferedBackAuditEngagementPlan(ENGID   IN NUMBER,
                                              REMARKS IN VARCHAR2,
                                              ENT_ID  in number,
                                              P_NO    in number,
                                              R_ID    in number) as
  
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
       71,
       'Reffered Back Audit Engagement Plan list',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
       P_NO,
       to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       REMARKS);
    COMMIT;
  
  end P_RefferedBackAuditEngagementPlan;

  procedure P_ApproveAuditEngagementPlan(ENGID  IN NUMBER,
                                         ENT_ID in number,
                                         P_NO   in number,
                                         R_ID   in number) as
  
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
       11,
       'GET Approve Audit Engagement Plan',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    UPDATE T_AU_PLAN_ENG a
       SET a.STATUS = 4, a.branch_code = a.entity_code
     WHERE a.ENG_ID = ENGID;
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
       P_NO,
       to_date(SYSDATE, 'dd/mm/yyyy HH:MI:SS AM'),
       'Engagement Plan Approved');
    COMMIT;
  
  end P_ApproveAuditEngagementPlan;

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

  procedure P_GetCCQ(ENT_ID    in number,
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
       53,
       'Review / Check CCQs',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
  
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
       where e.auditby_id = ENT_ID
       order by e.name;
  
  end P_GetCCQ;

  procedure P_UpdateCCQ(CID                  IN NUMBER,
                        QUESTIONS            in varchar2,
                        CONTROL_VIOLATION_ID in number,
                        RISK_ID              in number,
                        STATUS               in varchar2,
                        ENT_ID               in number,
                        P_NO                 in number,
                        R_ID                 in number) is
    risk_rating number := 0;
    Z_B         number := 0;
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
       53,
       'Updated CCQs',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
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
           c.UPDATED_BY           = P_NO,
           c.UPDATED_DATETIME     = sysdate
    
     WHERE c.ID = CID;
    COMMIT;
  end P_UpdateCCQ;

end PKG_PG;

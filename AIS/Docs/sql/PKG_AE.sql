create or replace package PKG_AE is
  TYPE t_cursor IS REF CURSOR;

  procedure P_GetAuditeeAssignedEntities(ENTITID   in number,
                                         io_cursor OUT t_cursor);

  procedure p_GetCOSORisks(io_cursor OUT t_cursor);

  procedure P_GetCCQsEntities(PPNO in number, io_cursor OUT t_cursor);

  procedure p_GetAssignedObservations(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      ENGID     in number,
                                      io_cursor OUT t_cursor);

  procedure P_GetObservationResponsible(OBSID     in number,
                                        E_ID      in number,
                                        io_cursor OUT t_cursor);

  procedure p_GetAssignedObservationstext(OBSID     in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetAssignedObservationsForBranch(entityid  in number,
                                               io_cursor OUT t_cursor);

  procedure P_GetObservationText(OBS_ID    in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_AUDITEE_OBSERVATION_RESPONSE(AUOBSID   IN number,
                                           REPLYDATA IN clob,
                                           REPLIEDBY IN number,
                                           OBSTEXTID IN number,
                                           REPLYROLE IN number,
                                           REMARKS   IN varchar2,
                                           SUBMITTED IN varchar2,
                                           ENT_ID    in number,
                                           P_NO      in number,
                                           R_ID      in number,
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

  procedure P_GetAuditeeOldParasFAD(EntityID  in number,
                                    io_cursor OUT t_cursor);

  procedure P_GetAuditeeAllParasFAD(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor);

  procedure P_UpdateOldParasStatus(PPNO       in number,
                                   PID        IN varchar2,
                                   NEW_STATUS in number);
  Procedure P_updateoldparamanagement(Paraid       in number,
                                      VCATID       in number,
                                      VCATNATUREID in number,
                                      RISKID       in number,
                                      ParaText     in clob,
                                      CREATEDBY    IN NUMBER,
                                      io_cursor    OUT t_cursor);

  procedure P_GetAuditeeOldParasentities(EntityID  in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParasentitiesFAD(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParas(ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParastext(paraid    in number,
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

  procedure P_SubmitPostAuditCompliance_Evidence(TEXT_ID  in varchar2,
                                                 filename IN Varchar2,
                                                 len_id   in number,
                                                 enter_by IN number,
                                                 filetype in varchar2,
                                                 filedata IN clob,
                                                 seq_id   IN number);

  procedure P_GetPostAuditCompliance_Evidence(TEXT_ID   in varchar2,
                                              io_cursor OUT t_cursor);
  procedure P_GetPostAuditCompliance_Evidence_FileData(FILE_ID   in varchar2,
                                                       io_cursor OUT t_cursor);
  procedure P_GetParasForComplianceByAuditee(P_NO      in number,
                                             ENT_ID    in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor);

  procedure P_GetParasForCompliancereview(P_NO      in number,
                                          ENT_ID    in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetParasForComplianceByAuditee_text(Old_id    in number,
                                                  new_id    in number,
                                                  IND       in varchar2,
                                                  io_cursor OUT t_cursor);

  procedure P_GetParasForCompliancehistory(comp_id   in number,
                                           io_cursor OUT t_cursor);

  procedure P_GetParasForComplianceforhistory(c_cycle   in number,
                                              comp_id   in number,
                                              io_cursor OUT t_cursor);

  Procedure P_SubmitPostAuditCompliance(Old_id      number,
                                        new_id      number,
                                        ENT_ID      number,
                                        P_NO        number,
                                        R_ID        number,
                                        Auditee_COM clob,
                                        A_COMMENTS  varchar2,
                                        P_IND       varchar2,
                                        io_cursor   OUT t_cursor);

  Procedure P_SubmitPostAuditCompliance_Review(Old_id     number,
                                               new_id     number,
                                               ENT_ID     number,
                                               P_NO       number,
                                               R_ID       number,
                                               A_COMMENTS varchar2,
                                               P_IND      varchar2,
                                               io_cursor  OUT t_cursor);

  procedure P_GetComplianceByAuditee(EntityID  in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     ENT_ID    in Number,
                                     io_cursor OUT t_cursor);

  procedure p_GetParaComplianceResponsible(Old_id    in number,
                                           new_id    in number,
                                           IND       in varchar2,
                                           io_cursor OUT t_cursor);

  procedure P_GetOldParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     io_cursor    OUT t_cursor);

  procedure P_GetParasForComplianceByCAU(P_NO      in number,
                                         ENT_ID    in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetrealtionshiptypeforCAU(io_cursor OUT t_cursor);

  procedure P_GetparentrepofficeforCAU(rid       in number,
                                       ENT_ID    in number,
                                       io_cursor OUT t_cursor);

  procedure P_GetchildpostingforCAU(P_ENT_ID  in number,
                                    io_cursor OUT t_cursor);

  Procedure P_FORWARD_CAU_PARA_TO_BRANCH(C_ID         number,
                                         ENT_ID       number,
                                         P_NO         number,
                                         R_ID         number,
                                         B_ENT_ID     number,
                                         CAU_COMMENTS varchar2,
                                         io_cursor    OUT t_cursor);

  procedure P_GetParasForComplianceByCAU_BY_BRANCH(P_NO      in number,
                                                   ENT_ID    in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  procedure P_GetParasForCompliance_CAU_para_text(c_id      number,
                                                  IND       varchar2,
                                                  io_cursor OUT t_cursor);

  Procedure P_SubmitPostAuditCompliance_BY_BRANCH(C_ID        number,
                                                  T_ID        number,
                                                  ENT_ID      number,
                                                  P_NO        number,
                                                  R_ID        number,
                                                  Auditee_COM clob,
                                                  -- A_COMMENTS  varchar2,
                                                  io_cursor OUT t_cursor);

  procedure P_SubmitPostAuditCompliance_Evidence_By_BRANCH(TEXT_ID  in varchar2,
                                                           filename IN Varchar2,
                                                           len_id   in number,
                                                           enter_by IN number,
                                                           filetype in varchar2,
                                                           filedata IN clob,
                                                           seq_id   IN number);

  procedure P_GetAllCompliance_Evidence_CAU(TEXT_ID   in varchar2,
                                            io_cursor OUT t_cursor);
  procedure P_GetPostAuditCompliance_Evidence_FileData_CAU(FILE_ID   in varchar2,
                                                           io_cursor OUT t_cursor);

  procedure P_GetParasForComplianceByCAU_FOR_REVIEW(P_NO      in number,
                                                    ENT_ID    in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor);

  Procedure p_GetPostAuditComplianceSecuritySnapshot(CM_ID     in number,
                                                     io_cursor OUT t_cursor);

  Procedure P_HasActiveUserContextAssignment(P_NO        in number,
                                             ENT_ID      in number,
                                             R_ID        in number,
                                             USER_CON_ID in number,
                                             io_cursor   OUT t_cursor);                                                     

end PKG_AE;

create or replace package body PKG_AE is

  procedure P_GetAuditeeAssignedEntities(ENTITID   in number,
                                         io_cursor OUT t_cursor) is
  
  begin
    if (ENTITID is not null) then
      open io_cursor for
        select distinct t.name || ' ( ' || e.operation_startdate || ' to ' ||
                        e.operation_enddate || ' )' as name,
                        t.code,
                        t.entity_id,
                        o.engplanid
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
           and (e.entity_id = ENTITID or ot.entity_id = ENTITID)
           and e.status < 14;
    else
      open io_cursor for
        select distinct t.name || ' ( ' || e.operation_startdate || ' to ' ||
                        e.operation_enddate || ' )' as name,
                        t.code,
                        t.entity_id,
                        o.engplanid
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
           and e.status < 14;
    end if;
  end P_GetAuditeeAssignedEntities;

  procedure p_GetCOSORisks(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select * from T_COSO_RISK R order by R.R_ID;
  end p_GetCOSORisks;

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

  procedure p_GetAssignedObservations(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      ENGID     in number,
                                      io_cursor OUT t_cursor) is
  begin
  
    OPEN io_Cursor FOR
      select o.id as OBS_ID,
             ot.id as OBS_TEXT_ID,
             O.MEMO_NUMBER AS MEMO_NUMBER,
             o.Memo_Date,
             NVL(o.replydate, null) as replydate,
             ot.headings as gist,
             nvl(ob.id, 0) as response_id,
             t.entity_id,
             ee.name as entity_name,
             s.statusname as STATUS,
             o.STATUS as STATUS_ID,
             pe.description as Audit_year,
             (case
               when o.entity_id = ENT_ID and o.status = 2 then
                1
               else
                (case
                  when o.entity_id = ENT_ID and o.status < 8 and o.status != 23 then
                   2
                  else
                   0
                end)
             
             end) as canreply,
             (case
               when o.status < 8 and o.status != 23 then
                1
               else
                0
             end) as editable
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_status s
          on o.status = s.statusid
       inner join t_au_plan_eng ep
          on ep.eng_id = o.engplanid
       inner join t_au_period pe
          on pe.auditperiodid = ep.period_id
       inner join t_auditee_entities ee
          on ee.entity_id = t.entity_id
        LEFT join t_au_observations_auditee_response ob
          on ob.au_obs_id = o.id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       WHERE ENT_ID = case
               when ep.entity_type = 25 then
                ENT_ID
               else
                t.entity_id
             end
         and ep.eng_id = engid
       order by t.OBS_ID asc;
  end p_GetAssignedObservations;
  procedure P_GetObservationResponsible(OBSID     in number,
                                        E_ID      in number,
                                        io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select ot.resp_row_id,
             ot.pp_no,
             em.EMPLOYEEFIRSTNAME || '  ' || em.EMPLOYEELASTNAME as EMP_NAME,
             ot.LOAN_CASE as LOANCASE,
             ot.lc_amount as LCAMOUNT,
             ot.account_number as ACCNUMBER,
             ot.ac_amount as ACAMOUNT
        from v_get_auditee_pp_responsibility ot
       inner join v_service_employeeinfo em
          on em.PPNO = ot.pp_no
       where ot.au_obs_id = OBSID;
  
  end P_GetObservationResponsible;

  procedure p_GetAssignedObservationstext(OBSID     in number,
                                          io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select O.MEMO_NUMBER AS MEMO_NUMBER,
             ot.text       as OBSERVATION_TEXT,
             ot.headings   as OBSERVATION_TEXT_PLAIN,
             ob.reply      as replytext,
             o.id          as resp_id
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_text ot
          on ot.id = t.obs_text_id
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
             ot.headings            as OBSERVATION_TEXT_PLAIN,
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

  procedure P_GetObservationText(OBS_ID    in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) is
  
    Z_B number := 0;
  begin
  
    commit;
    commit;
  
    OPEN io_Cursor FOR
      select ot.text, o.reference_id
        from T_AU_OBSERVATION_TEXT ot
       inner join t_au_observation o
          on o.id = ot.observatsion_id
       where ot.OBSERVATSION_ID = OBS_ID;
  
  end P_GetObservationText;

  procedure P_AUDITEE_OBSERVATION_RESPONSE(AUOBSID   IN number,
                                           REPLYDATA IN clob,
                                           REPLIEDBY IN number,
                                           OBSTEXTID IN number,
                                           REPLYROLE IN number,
                                           REMARKS   IN varchar2,
                                           SUBMITTED IN varchar2,
                                           ENT_ID    in number,
                                           P_NO      in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor) is
    Z_B             number := 0;
    Already_Replied number := 0;
  begin
  
    commit;
    commit;
  
    select NVL(MAX(l.id), 0)
      into Already_Replied
      from t_au_observations_auditee_response l
     where l.au_obs_id = AUOBSID;
    if (Already_Replied = 0) THEN
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
         SET t.STATUS          = 3,
             T.LASTREPLYBY     = REPLIEDBY,
             t.MEMO_REPLY_DATE = trunc(SYSDATE)
       WHERE ID = AUOBSID;
    
    ELSE
      UPDATE T_AU_OBSERVATIONS_AUDITEE_RESPONSE AR
         SET AR.REPLY       = REPLYDATA,
             AR.REPLIEDBY   = REPLIEDBY,
             AR.REPLIEDDATE = trunc(SYSDATE)
       WHERE AR.AU_OBS_ID = AUOBSID;
      commit;
    END IF;
  
    open io_cursor for
      select r.au_obs_id as ob_id, r.id as resp_id
        from T_AU_OBSERVATIONS_AUDITEE_RESPONSE r
       where r.au_obs_id = AUOBSID;
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
  -- Ali & Asfand from here
  procedure P_GetAuditeeOldParasFAD(EntityID  in number,
                                    io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select f.audit_period,
             f.name,
             f.para_no,
             f.id,
             f.au_obs_id,
             f.ref_p,
             f.gist_of_paras,
             f.vol_i_ii,
             f.amount,
             f.para_category,
             f.audited_by
      
        from v_get_P_GetAuditeeOldParasFAD f
       where f.ENTITY_ID = EntityID
       order by f.audit_period, f.para_no;
    --and f.c_status ;
  
  end P_GetAuditeeOldParasFAD;
  -- auditee portal field

  procedure P_GetAuditeeAllParasFAD(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor) as
  
    Z_R number := 0;
  begin
  
    commit;
    commit;
  
    OPEN io_cursor FOR
      select f.audit_period,
             e.name,
             f.para_no,
             f.id,
             f.ref_p,
             f.gist_of_paras,
             f.vol_i_ii,
             fd.remarks        as reviewer_remarks,
             f.amount_involved as amount
        FROM t_au_old_paras_fad f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
        left join v_get_P_GetAuditeeOldParasFAD_ref fd
          on fd.ref_p = f.ref_p --and fd.c_status IN (12)
       WHERE fd.audited_by = ENT_ID
      
       order by f.audit_period, e.name, f.para_no;
  
  end P_GetAuditeeAllParasFAD;

  procedure P_UpdateOldParasStatus(PPNO       in number,
                                   PID        IN varchar2,
                                   NEW_STATUS in number) as
  begin
  
    UPDATE T_AU_OLD_PARAS_FAD al
       SET al.Para_Status = NEW_STATUS, al.parastatusupdatedby = PPNO
     WHERE al.ref_p = PID;
    commit;
  end P_UpdateOldParasStatus;

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
      --   WHERE e.entity_id = EntityID
       order by e.name;
  
  end P_GetAuditeeOldParasentities;

  procedure P_GetAuditeeOldParasentitiesFAD(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor) as
  
    Z_B number := 0;
  begin
  
    commit;
    commit;
  
    OPEN io_cursor FOR
      select distinct s.entity_id, s.name
        from v_get_P_GetAuditeeOldParasentitiesFAD s
       WHERE s.entity_id = ENT_ID
       order by s.name;
  
  end P_GetAuditeeOldParasentitiesFAD;

  procedure P_GetAuditeeOldParas(ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) as
  
    Z_B number := 0;
  begin
  
    commit;
    commit;
    OPEN io_cursor FOR
      select e.name as ENTITY_NAME,
             s.para_id as id,
             s.entity_id as ENTITY_CODE,
             '2022' as AUDIT_PERIOD,
             s.para_no,
             s.gist_of_paras,
             'A' as AUDITEE_RESPONSE,
             'A' as AUDITOR_REMARKS,
             sysdate as DATE_OF_LAST_COMPLIANCE_RECEIVED,
             s.audited_by,
             '14' as TYPE_ID,
             'Department' as entitytypedesc,
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
          on r.r_id = vs.risk_id
       WHERE e.entity_id = ENT_ID
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

  procedure P_GetParasForComplianceByAuditee(P_NO      in number,
                                             ENT_ID    in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor) as
  
  begin
    if (ENT_ID in (113176, 113182)) then
      OPEN io_cursor FOR
        select C.AUDIT_PERIOD,
               c.name,
               C.PARA_NO,
               C.NEW_PARAID AS NEW_PARA_ID,
               C.OLD_PARA_ID,
               C.GIST_OF_PARAS,
               C.AUDITBY_ID,
               '' AS AUDIT_REPLY,
               c.REC_FROM,
               C.NEXT_R_ID,
               C.PER_R_ID,
               C.C_STATUS_UP,
               C.C_STATUS_DOWN,
               c.IND,
               c.Rsk as Risk,
               c.com_id,
               C.AUDIT_PERIOD,
               C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
          FROM V_GET_AIS_POST_COMPLIANCE C
         inner join t_auditee_entities_maping m
            on m.entity_id = c.entity_id
         where ((ENT_ID = 113176 AND
               c.ENTITY_ID IN (113176, 113173, 112935, 112933)) OR
               (ENT_ID = 113182 AND c.ENTITY_ID IN (113182, 113105)))
           and C.com_stage = R_ID
           and C.com_status != 16
         order by C.audit_period desc, C.para_no asc;
    else
      OPEN io_cursor FOR
        select C.AUDIT_PERIOD,
               c.name,
               C.PARA_NO,
               C.NEW_PARAID AS NEW_PARA_ID,
               C.OLD_PARA_ID,
               C.GIST_OF_PARAS,
               C.AUDITBY_ID,
               '' AS AUDIT_REPLY,
               c.REC_FROM,
               C.NEXT_R_ID,
               C.PER_R_ID,
               C.C_STATUS_UP,
               C.C_STATUS_DOWN,
               c.IND,
               c.Rsk as Risk,
               c.com_id,
               C.AUDIT_PERIOD,
               C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
          FROM V_GET_AIS_POST_COMPLIANCE C
         inner join t_auditee_entities_maping m
            on m.entity_id = c.entity_id
          left join t_auditee_entities_maping_com e
            on e.com_key = c.com_key
         where c.ENTITY_ID = ENT_ID
           and C.com_stage = ---CASE ADDED because GM USERS can't view their own PARAS ( CASES SHOULD BE ADDED FOR OTHER USERS AS WELL )
               (case
                 when R_ID = 39 then
                  21
                 else
                  R_ID
               end)
           and C.com_status != 16
         order by C.audit_period desc, C.para_no asc;
    end if;
  end P_GetParasForComplianceByAuditee;

  procedure P_GetParasForCompliancereview(P_NO      in number,
                                          ENT_ID    in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor) as
  
    N_F number := 0;
  begin
  
    select NVL(max(m.ppno), 0)
      into N_F
      from t_user_context_assignment m
        
     where m.ppno = P_NO
       and m.entity_id = ent_id
       and m.role_id = R_ID
       and R_ID != 13;
  
    if (N_F != 0) then
      IF (R_ID in (41)) then
        OPEN io_cursor FOR
          select C.AUDIT_PERIOD,
                 '' AS NAME,
                 C.PARA_NO,
                 C.NEW_PARAID AS NEW_PARA_ID,
                 C.OLD_PARA_ID,
                 C.GIST_OF_PARAS,
                 C.AUDITBY_ID,
                 '' AS AUDIT_REPLY,
                 c.REC_FROM,
                 C.NEXT_R_ID,
                 C.PER_R_ID,
                 C.C_STATUS_UP,
                 C.C_STATUS_DOWN,
                 c.IND,
                 c.Rsk,
                 c.Rsk as Risk,
                 c.com_id,
                 C.START_DATE AS AUDIT_START_DATE,
                 C.END_DATE AS AUDIT_END_DATE,
                 c.OPS_START_DATE,
                 c.OPS_END_DATE
            FROM V_GET_AIS_POST_COMPLIANCE C
           inner join t_auditee_entities_maping m
              on m.entity_id = c.entity_id
           where c.com_stage = R_ID
             and c.com_status != 16
          -- and trunc(c.STELLED_ON) between '01-Jan-2025' and '31-Dec-25'
           order by c.audit_period desc, c.para_no asc;
      
      else
      
        IF (R_ID in (43, 44)) then
          OPEN io_cursor FOR
            select C.AUDIT_PERIOD,
                   '' AS NAME,
                   C.PARA_NO,
                   C.NEW_PARAID AS NEW_PARA_ID,
                   C.OLD_PARA_ID,
                   C.GIST_OF_PARAS,
                   C.AUDITBY_ID,
                   '' AS AUDIT_REPLY,
                   c.REC_FROM,
                   C.NEXT_R_ID,
                   C.PER_R_ID,
                   C.C_STATUS_UP,
                   C.C_STATUS_DOWN,
                   c.IND,
                   c.Rsk,
                   c.Rsk as Risk,
                   c.com_id,
                   C.START_DATE AS AUDIT_START_DATE,
                   C.END_DATE AS AUDIT_END_DATE,
                   c.OPS_START_DATE,
                   c.OPS_END_DATE
              FROM V_GET_AIS_POST_COMPLIANCE C
             inner join t_auditee_entities_maping m
                on m.entity_id = c.entity_id
             inner join t_auditee_entities_maping_com e
                on e.com_key = c.com_key
             where P_NO = case
                     when R_ID = 43 then
                      e.reviewer_ppno
                     when r_id = 44 then
                      e.approver_ppno
                     WHEN r_id = 41 THEN
                      p_no
                   end
               and c.com_stage = R_ID
               and c.com_status != 16
             order by c.audit_period desc, c.para_no asc;
        
        else
          OPEN io_cursor FOR
            select C.AUDIT_PERIOD,
                   '' AS NAME,
                   C.PARA_NO,
                   C.NEW_PARAID AS NEW_PARA_ID,
                   C.OLD_PARA_ID,
                   C.GIST_OF_PARAS,
                   C.AUDITBY_ID,
                   '' AS AUDIT_REPLY,
                   C.REC_FROM,
                   C.NEXT_R_ID,
                   C.PER_R_ID,
                   C.C_STATUS_UP,
                   C.C_STATUS_DOWN,
                   c.IND,
                   c.Rsk,
                   c.Rsk as Risk,
                   c.com_id,
                   C.START_DATE AS AUDIT_START_DATE,
                   C.END_DATE AS AUDIT_END_DATE,
                   c.OPS_START_DATE,
                   c.OPS_END_DATE
              FROM V_GET_AIS_POST_COMPLIANCE C
             inner join t_auditee_entities_maping m
                on m.entity_id = c.entity_id
              left join t_auditee_entities_maping_com e
                on e.com_key = c.com_key
             where Ent_ID in case
                     when R_ID in (21, 45) then
                      m.parent_id
                     when R_ID in (2, 9, 7, 6, 15, 16, 11) then
                      c.auditby_id
                   end
               and c.com_stage = R_ID
               and c.com_status != 16
             order by c.audit_period desc, c.para_no asc;
        end if;
      end if;
    else
    
      open io_cursor for
        select 'System Issue, Please contact System Administrator on 051-2002110' as remarks
          from dual;
    end if;
  
  end P_GetParasForCompliancereview;

  procedure P_GetParasForComplianceByAuditee_text(Old_id    in number,
                                                  new_id    in number,
                                                  IND       in varchar2,
                                                  io_cursor OUT t_cursor) as
  
  begin
    if (IND = 'O') then
      OPEN io_cursor FOR
        select f.gist_of_paras, t.para_text as text, t.id as text_id
          from t_au_old_paras_fad f
          left join t_au_old_paras_fad_text t
            on t.ref_p = f.ref_p
         where f.id = Old_id;
    else
      if (IND = 'A') then
        OPEN io_cursor FOR
          select at.headings as gist_of_paras, at.text, at.id as text_id
            from t_au_observation af
           inner join t_au_observation_text at
              on at.observatsion_id = af.id
           where af.id = new_id;
      else
        if (IND = 'C') then
          OPEN io_cursor FOR
            select cf.gist_of_paras, ct.text, ct.id as text_id
              from t_au_observation_old_cad_paras cf
             inner join t_au_observation_old_cad_paras_text ct
                on cf.para_id = ct.observatsion_id
             where cf.para_id = new_id;
        end if;
      end if;
    end if;
  
  end P_GetParasForComplianceByAuditee_text;

  procedure P_GetParasForCompliancehistory(comp_id   in number,
                                           io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select f.hist_id,
             f.com_id,
             f.com_cycle,
             f.com_status,
             f.com_stage,
             f.comment_by_role,
             f.comment_by_ppno as PP_NO,
             e.employeefirstname || ' ' || e.employeelastname as name,
             f.comment_on,
             f.comments,
             f.com_flow
      
        from AIS_T_AU_POST_COMPLIANCE_HISTORY f
        left join v_service_employeeinfo e
          on e.ppno = f.comment_by_PPNO
       where f.com_id = comp_id
       order by f.com_cycle, f.comment_on, f.comment_on asc;
  
  end P_GetParasForCompliancehistory;

  procedure P_GetParasForComplianceforhistory(c_cycle   in number,
                                              comp_id   in number,
                                              io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select t.reply,
             t.c_txt_id as text_id,
             (case
               when o.IND = 'A' then
                pt.text
               else
                (case
                  when o.IND = 'C' then
                   ct.text
                  else
                   (case
                     when o.IND = 'O' then
                      ft.para_text
                   end)
                end)
             end) as para_text
        from AIS_T_AU_POST_COMPLIANCE_text t
       inner join AIS_T_AU_POST_COMPLIANCE p
          on t.com_id = p.com_id
       inner join T_AU_OBSERVATION_FAD o
          on (o.new_paraid = p.new_para_id or o.old_para_id = p.old_para_id)
        left join t_au_observation_text pt
          on o.new_paraid = pt.observatsion_id
         and o.IND = 'A'
        left join t_au_old_paras_fad_text ft
          on ft.ref_p = o.Old_para_ref
        left join t_au_observation_old_cad_paras_text ct
          on ct.observatsion_id = o.new_paraid
         and o.IND = 'C'
       where t.com_id = comp_ID
         and t.com_cycle = c_cycle;
  end P_GetParasForComplianceforhistory;

  Procedure P_SubmitPostAuditCompliance(Old_id      number,
                                        new_id      number,
                                        ENT_ID      number,
                                        P_NO        number,
                                        R_ID        number,
                                        Auditee_COM clob,
                                        A_COMMENTS  varchar2,
                                        P_IND       varchar2,
                                        io_cursor   OUT t_cursor) as
    P_F varchar2(50);
 
    cursor V is
      select C.AUDIT_PERIOD,
             c.name,
             C.PARA_NO,
             C.NEW_PARAID,
             C.OLD_PARA_ID,
             C.GIST_OF_PARAS,
             C.AUDITBY_ID,
             c.REC_FROM,
             C.NEXT_R_ID,
             C.PER_R_ID,
             C.C_STATUS_UP,
             C.C_STATUS_DOWN,
             c.IND,
             c.com_CYCLE,
             c.com_id,
             c.com_stage,
             C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
        FROM V_GET_AIS_POST_COMPLIANCE C
       inner join t_auditee_entities_maping m
          on m.entity_id = c.entity_id
        left join t_auditee_entities_maping_com e
          on e.com_key = c.com_key
       where (C.OLD_PARA_ID = Old_id or
             (C.NEW_PARAID = new_id and c.IND = P_F));
  
    Vr1 V%rowtype;
    V_F number := 0;
     text_id number;
    Z_B number := 0;
    N_F number := 0;
    C_F number := 0;
  begin
    select NVL(max(cad.para_id), 0)
      into C_F
      from t_au_observation_old_cad_paras cad
     where cad.para_id = new_id;
    --and cad.entity_id = ENT_ID;
    select (case
             when et.audit_type = 'B' then
              'A'
             else
              (case
                when C_F != 0 then
                 'C'
                else
                 'A'
              end)
           end)
      into P_F
      from t_auditee_entities en
     inner join t_auditee_ent_types et
        on en.type_id = et.autid
     where en.entity_id = ENT_ID;
  
    Open V;
    Fetch V
      into vr1;
    Close v;
  
    IF ((R_ID = vr1.com_stage) OR (R_ID IN (39))) THEN
      select NVL(max(m.ppno), 0)
        into N_F
       from t_user_context_assignment m
          
       where m.ppno = P_NO
         and m.entity_id = ENT_ID
         and m.role_id = R_ID;
    
      if (N_F != 0) then
        commit;
      
        commit;
      
        update AIS_T_AU_POST_COMPLIANCE c
           set c.com_cycle  = vr1.com_cycle + 1,
               c.com_stage  = vr1.next_r_id,
               c.com_status = 10
         where c.com_id = vr1.com_id;
        commit;
      
        update AIS_T_AU_POST_COMPLIANCE_text t
           set t.status = 'N'
         where t.com_id = vr1.com_id
           and t.com_cycle = vr1.com_cycle;
        commit;
      
        select max(ts.c_txt_id + 1)
          into V_F
          from AIS_T_AU_POST_COMPLIANCE_text ts;
      
        insert into AIS_T_AU_POST_COMPLIANCE_text
          (c_Txt_Id, Com_Id, Reply, Com_Cycle, Status)
        values
          (V_F, vr1.com_id, Auditee_COM, vr1.com_cycle + 1, 'Y');
        commit;
      
        insert into AIS_T_AU_POST_COMPLIANCE_HISTORY
          (HIST_ID,
           COM_ID,
           COM_CYCLE,
           COM_STATUS,
           COM_STAGE,
           COMMENT_BY_ROLE,
           COMMENT_BY_PPNO,
           COMMENT_ON,
           COMMENTS,
           COM_FLOW)
        values
          ((select COALESCE(max(ct.HIST_ID) + 1, 1)
             From AIS_T_AU_POST_COMPLIANCE_HISTORY ct),
           vr1.com_id,
           vr1.com_cycle + 1,
           10,
           R_ID,
           (select g.group_name from t_groups g where g.group_id = R_ID),
           P_NO,
           sysdate,
           'Compliance Submitted',
           'Y');
        commit;
      
        open io_cursor for
          select 'Complaince Submitted' as remarks, V_F as text_id
            from dual;
      
      else
        open io_cursor for
          select 'Sorry you connection is lost, logout and login again' as remarks,
                 0 as text_id
            from dual;
      end if;
    
    else
      open io_cursor for
        select 'Compliance Already Submitted, If you face the same issue, Contact on  051-2002110' as remarks,
               0 as text_id
          from dual;
    end if;
  
  end P_SubmitPostAuditCompliance;

  procedure P_SubmitPostAuditCompliance_Evidence(TEXT_ID  in varchar2,
                                                 filename IN Varchar2,
                                                 len_id   in number,
                                                 enter_by IN number,
                                                 filetype in varchar2,
                                                 filedata IN clob,
                                                 seq_id   IN number) as
  
    C_M          NUMBER;
    PREV_TEXT_ID NUMBER;
  BEGIN
    -- Get comp_id for the given text_id
    SELECT t.com_id
      INTO C_M
      FROM ais_t_au_post_compliance_text t
     WHERE t.c_txt_id = TEXT_ID;
  
    -- Find the immediately previous text_id for the same comp_id
    SELECT MAX(c_txt_id)
      INTO PREV_TEXT_ID
      FROM ais_t_au_post_compliance_text
     WHERE com_id = C_M
       AND c_txt_id < TEXT_ID;
  
    insert into AIS_T_AU_POST_COMPLIANCE_EVIDENCE_archive
      select *
        from ais_t_au_post_compliance_evidence c
       where c.comp_id = C_M
         and c.textid = PREV_TEXT_ID;
    commit;
    -- Delete evidence for the previous text_id, if it exists
    IF PREV_TEXT_ID IS NOT NULL THEN
      DELETE FROM ais_t_au_post_compliance_evidence
       WHERE textid = PREV_TEXT_ID;
      commit;
    END IF;
  
    insert into ais_t_au_post_compliance_evidence
      (id,
       file_name,
       length,
       file_data,
       textid,
       enteredby,
       entereddate,
       sequence,
       description,
       COMP_ID)
    
    VALUES
      ((select COALESCE(max(ac.ID) + 1, 1)
         from ais_t_au_post_compliance_evidence ac),
       filename,
       len_id,
       filedata,
       TEXT_ID,
       enter_by,
       sysdate,
       seq_id,
       filetype,
       C_M);
    commit;
  
  end P_SubmitPostAuditCompliance_Evidence;

  PROCEDURE P_SubmitPostAuditCompliance_Review(Old_id     NUMBER,
                                               new_id     NUMBER,
                                               ENT_ID     NUMBER,
                                               P_NO       NUMBER,
                                               R_ID       NUMBER,
                                               A_COMMENTS VARCHAR2,
                                               P_IND      VARCHAR2,
                                               io_cursor  OUT t_cursor) AS
  
    ------------------------------------------------------------------
    -- Local variables
    ------------------------------------------------------------------
    P_F VARCHAR2(50);
    N_F NUMBER := 0;
    Z_B NUMBER := 0;
    C_F NUMBER := 0;
  
    v_current_stage NUMBER;
    v_savepoint_set BOOLEAN := FALSE;
  
    ------------------------------------------------------------------
    -- Compliance cursor
    ------------------------------------------------------------------
    CURSOR V IS
      SELECT C.AUDIT_PERIOD,
             C.NAME,
             C.PARA_NO,
             C.NEW_PARAID,
             C.OLD_PARA_ID,
             C.GIST_OF_PARAS,
             C.AUDITBY_ID,
             C.REC_FROM,
             ET.EMAIL_ADDRESS AS TO_EMAIL,
             AD.EMAIL_ADDRESS AS CC_EMAIL,
             MT.EMAIL_ADDRESS AS CC_EMAIL2,
             
             CASE
               WHEN P_IND = 'U' THEN
                C.NEXT_R_ID
               ELSE
                C.PER_R_ID
             END AS ROLE_ID,
             
             CASE
               WHEN P_IND = 'U' THEN
                C.C_STATUS_UP
               ELSE
                C.C_STATUS_DOWN
             END AS STATUS_ID,
             
             C.IND,
             C.COM_STAGE,
             C.COM_CYCLE,
             C.COM_ID AS COMID,
             C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
      
        FROM V_GET_AIS_POST_COMPLIANCE C
      
       INNER JOIN T_AUDITEE_ENTITIES_MAPING M
          ON M.ENTITY_ID = C.ENTITY_ID
      
       INNER JOIN T_AUDITEE_ENTITIES ET
          ON ET.ENTITY_ID = M.ENTITY_ID
       INNER JOIN T_AUDITEE_ENTITIES MT
          ON MT.ENTITY_ID = M.PARENT_ID
       INNER JOIN T_AUDITEE_ENTITIES AD
          ON AD.ENTITY_ID = ET.AUDITBY_ID
      
        LEFT JOIN T_AUDITEE_ENTITIES_MAPING_COM E
          ON E.COM_KEY = C.COM_KEY
      
       WHERE C.OLD_PARA_ID = Old_id
          OR (C.NEW_PARAID = new_id AND C.IND = P_F);
  
    Vr1 V%ROWTYPE;
  
  BEGIN
  
    ------------------------------------------------------------------
    -- 1. Determine whether para belongs to normal observation
    --    or old CAD para
    ------------------------------------------------------------------
    SELECT NVL(MAX(CAD.PARA_ID), 0)
      INTO C_F
      FROM T_AU_OBSERVATION_OLD_CAD_PARAS CAD
     WHERE CAD.PARA_ID = new_id
       AND CAD.AUDITED_BY = ENT_ID;
  
    SELECT CASE
             WHEN ET.AUDIT_TYPE = 'B' THEN
              'A'
             ELSE
              CASE
                WHEN C_F <> 0 THEN
                 'C'
                ELSE
                 'A'
              END
           END
      INTO P_F
      FROM T_AUDITEE_ENTITIES EN
     INNER JOIN T_AUDITEE_ENT_TYPES ET
        ON EN.TYPE_ID = ET.AUTID
     WHERE EN.ENTITY_ID = ENT_ID;
  
    ------------------------------------------------------------------
    -- 2. Fetch compliance record
    ------------------------------------------------------------------
    OPEN V;
  
    FETCH V
      INTO Vr1;
  
    IF V%NOTFOUND THEN
    
      CLOSE V;
    
      OPEN io_cursor FOR
        SELECT 'Compliance record not found. Please refresh and try again.' AS remarks,
               '' AS para_no,
               '' AS para_status,
               '' AS GIST_OF_PARAS,
               '' AS TO_EMAIL,
               '' AS CC_EMAIL,
               '' AS CC_EMAIL2
          FROM DUAL;
    
      RETURN;
    
    END IF;
  
    CLOSE V;
  
    ------------------------------------------------------------------
    -- 3. Validate user/context before starting transaction
    ------------------------------------------------------------------
    SELECT NVL(MAX(M.PPNO), 0)
      INTO N_F
      FROM T_USER U
     INNER JOIN T_USER_CONTEXT_ASSIGNMENT M
        ON M.PPNO = U.PPNO
     WHERE M.PPNO = P_NO
       AND M.ENTITY_ID = ENT_ID
       AND M.ROLE_ID = R_ID
       AND R_ID <> 13;
  
    IF N_F = 0 THEN
    
      OPEN io_cursor FOR
        SELECT 'Sorry you connection is lost, logout and login again' AS remarks,
               Vr1.PARA_NO AS PARA_NO,
               '' AS para_status,
               '' AS GIST_OF_PARAS,
               '' AS TO_EMAIL,
               '' AS CC_EMAIL,
               '' AS CC_EMAIL2
          FROM DUAL;
    
      RETURN;
    
    END IF;
  
    ------------------------------------------------------------------
    -- 4. Start protected transaction
    ------------------------------------------------------------------
    SAVEPOINT SP_POST_COMPLIANCE;
  
    v_savepoint_set := TRUE;
  
    ------------------------------------------------------------------
    -- 5. Lock only this compliance record.
    --
    -- Prevents two users from moving the same para simultaneously.
    -- Other COM_IDs remain available to other users.
    ------------------------------------------------------------------
    SELECT C.COM_STAGE
      INTO v_current_stage
      FROM AIS_T_AU_POST_COMPLIANCE C
     WHERE C.COM_ID = Vr1.COMID
       FOR UPDATE;
  
    ------------------------------------------------------------------
    -- 6. Revalidate stage AFTER locking the row
    ------------------------------------------------------------------
    IF R_ID <> v_current_stage THEN
    
      ROLLBACK TO SP_POST_COMPLIANCE;
    
      OPEN io_cursor FOR
        SELECT 'Compliance Already Submitted, If the para no ' ||
               Vr1.PARA_NO ||
               ' still showing on the grid, Contact on  051-2002110' AS remarks,
               '' AS PARA_NO,
               '' AS para_status,
               '' AS GIST_OF_PARAS,
               '' AS TO_EMAIL,
               '' AS CC_EMAIL,
               '' AS CC_EMAIL2
          FROM DUAL;
    
      RETURN;
    
    END IF;
  
    ------------------------------------------------------------------
    -- 7. Close previous activity
    ------------------------------------------------------------------
    INSERT INTO AIS_T_AU_POST_COMPLIANCE_HISTORY
      (HIST_ID,
       COM_ID,
       COM_CYCLE,
       COM_STATUS,
       COM_STAGE,
       COMMENT_BY_ROLE,
       COMMENT_BY_PPNO,
       COMMENT_ON,
       COMMENTS,
       COM_FLOW)
    VALUES
      (SEQ_POST_COMPLIANCE_HISTORY.NEXTVAL,
       Vr1.COMID,
       Vr1.COM_CYCLE,
       Vr1.STATUS_ID,
       R_ID,
       
       (SELECT G.GROUP_NAME FROM T_GROUPS G WHERE G.GROUP_ID = R_ID),
       
       P_NO,
       SYSDATE,
       A_COMMENTS,
       'Y');
  
    ------------------------------------------------------------------
    -- 10. Settlement
    ------------------------------------------------------------------
    IF Vr1.STATUS_ID = 16 AND R_ID IN (6, 7, 44, 41) THEN
    
      --------------------------------------------------------------
      -- Update post-compliance master
      --------------------------------------------------------------
      UPDATE AIS_T_AU_POST_COMPLIANCE C
         SET C.COM_CYCLE   = Vr1.COM_CYCLE,
             C.COM_STAGE   = Vr1.ROLE_ID,
             C.COM_STATUS  = Vr1.STATUS_ID,
             C.PARA_STATUS = 9,
             C.SETTELED_ON = SYSDATE,
             C.SETTELED_BY = P_NO
       WHERE C.COM_ID = Vr1.COMID;
    
      --------------------------------------------------------------
      -- Update originating para according to para type
      --------------------------------------------------------------
      CASE Vr1.IND
      
      ----------------------------------------------------------
      -- Current observation
      ----------------------------------------------------------
        WHEN 'A' THEN
        
          UPDATE T_AU_OBSERVATION O
             SET O.STATUS = 9, O.STELLED_ON = SYSDATE, O.SETTLED_BY = P_NO
           WHERE O.ID = new_id;
        
      ----------------------------------------------------------
      -- Old FAD para
      ----------------------------------------------------------
        WHEN 'O' THEN
        
          UPDATE T_AU_OLD_PARAS_FAD FD
             SET FD.PARA_STATUS    = 6,
                 FD.SETTLED_BY     = P_NO,
                 FD.PARASETTELEDON = SYSDATE
           WHERE FD.ID = Old_id;
        
      ----------------------------------------------------------
      -- Old CAD para
      ----------------------------------------------------------
        WHEN 'C' THEN
        
          UPDATE T_AU_OBSERVATION_OLD_CAD_PARAS CD
             SET CD.PARA_STATUS = 9,
                 CD.SETTELED_BY = P_NO,
                 CD.SETTELED_ON = SYSDATE
           WHERE CD.PARA_ID = new_id;
        
      ----------------------------------------------------------
      -- No originating table update required
      ----------------------------------------------------------
        ELSE
          NULL;
        
      END CASE;
    
    ELSE
    
      ----------------------------------------------------------------
      -- 11. Normal forwarding / referring back
      ----------------------------------------------------------------
      UPDATE AIS_T_AU_POST_COMPLIANCE C
         SET C.COM_CYCLE  = Vr1.COM_CYCLE,
             C.COM_STAGE  = Vr1.ROLE_ID,
             C.COM_STATUS = Vr1.STATUS_ID
       WHERE C.COM_ID = Vr1.COMID;
    
    END IF;
  
    ------------------------------------------------------------------
    -- 12. Entire business transaction succeeded
    ------------------------------------------------------------------
    COMMIT;
  
    v_savepoint_set := FALSE;
  
    ------------------------------------------------------------------
    -- 13. Return response to application
    ------------------------------------------------------------------
    IF Vr1.STATUS_ID = 16 THEN
    
      OPEN io_cursor FOR
        SELECT 'Para no ' || Vr1.PARA_NO ||
               ' is marked as settled, Please inform the auditee ' AS remarks,
               
               Vr1.PARA_NO AS para_no,
               'Settled' AS para_status,
               Vr1.GIST_OF_PARAS AS GIST_OF_PARAS,
               Vr1.TO_EMAIL AS TO_EMAIL,
               Vr1.CC_EMAIL AS CC_EMAIL,
               '' AS CC_EMAIL2
        
          FROM DUAL;
    
    ELSE
    
      IF P_IND = 'U' THEN
      
        OPEN io_cursor FOR
          SELECT 'Complaince Forwarded' AS remarks,
                 Vr1.PARA_NO AS para_no,
                 '' AS para_status,
                 '' AS GIST_OF_PARAS,
                 '' AS TO_EMAIL,
                 '' AS CC_EMAIL,
                 '' AS CC_EMAIL2
            FROM DUAL;
      
      ELSE
      
        OPEN io_cursor FOR
          SELECT 'Complaince Rejected/Referred Back' AS remarks,
                 Vr1.PARA_NO AS para_no,
                 'Rejected/Referred Back' AS para_status,
                 '' AS GIST_OF_PARAS,
                 '' AS TO_EMAIL,
                 '' AS CC_EMAIL,
                 '' AS CC_EMAIL2
            FROM DUAL;
      
      END IF;
    
    END IF;
  
  EXCEPTION
  
    ------------------------------------------------------------------
    -- Any failure in history/workflow/settlement means the complete
    -- submission is rolled back.
    ------------------------------------------------------------------
    WHEN OTHERS THEN
    
      IF v_savepoint_set THEN
        ROLLBACK TO SP_POST_COMPLIANCE;
      END IF;
    
      RAISE;
    
  END P_SubmitPostAuditCompliance_Review;
  procedure P_GetPostAuditCompliance_Evidence(TEXT_ID   in varchar2,
                                              io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             '' as file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence
       where textid = TEXT_ID
       order by sequence asc;
  
    commit;
  
  end P_GetPostAuditCompliance_Evidence;

  procedure P_GetPostAuditCompliance_Evidence_FileData(FILE_ID   in varchar2,
                                                       io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence
       where id = FILE_ID;
  
    commit;
  
  end P_GetPostAuditCompliance_Evidence_FileData;

  procedure P_GetComplianceByAuditee(EntityID  in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     ENT_ID    in Number,
                                     io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select f.audit_period,
             e.name,
             f.para_no,
             f.old_para_id,
             f.new_para_id,
             f.gist_of_paras,
             e.auditby_id,
             f.IND,
             f.com_id,
             f.com_status,
             f.com_cycle,
             fl.next_r_id,
             fl.per_r_id,
             fl.c_status_up,
             fl.c_status_down
        from AIS_T_AU_POST_COMPLIANCE f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       inner join t_au_post_compliance_flow fl
          on fl.entity_type = e.type_id
         and fl.role_id = f.com_stage
       where f.ENTITY_ID = EntityID
         and f.com_stage = R_ID
       order by f.audit_period desc;
  
  end P_GetComplianceByAuditee;

  procedure p_GetParaComplianceResponsible(Old_id    in number,
                                           new_id    in number,
                                           IND       in varchar2,
                                           io_cursor OUT t_cursor) as
  
  begin
    if (IND = 'O') then
      open io_cursor for
        select f.pp_no,
               e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
               f.loan_case as LOANCASE,
               f.lc_amount as LCAMOUNT,
               f.account_number as ACCNUMBER,
               f.ac_amount as ACAMOUNT
          from v_get_auditee_PP_responsibility f
         inner join v_service_employeeinfo e
            on e.PPNO = f.pp_no
         WHERE F.OLD_PARA_ID = Old_id
           and f.status = 'Y';
    else
      if (IND = 'N') then
        open io_cursor for
          select f.pp_no,
                 e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
                 f.loan_case as LOANCASE,
                 f.lc_amount as LCAMOUNT,
                 f.account_number as ACCNUMBER,
                 f.ac_amount as ACAMOUNT
            from v_get_auditee_PP_responsibility f
           inner join v_service_employeeinfo e
              on e.PPNO = f.pp_no
           WHERE F.au_obs_id = new_id
             and f.status = 'Y';
      end if;
    end if;
  end p_GetParaComplianceResponsible;

  procedure P_GetOldParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     io_cursor    OUT t_cursor) is
  begin
    open io_cursor for
    
      SELECT f.*,
             c.heading   as Process_Des,
             cc.heading  as Sub_process_Des,
             csb.heading AS Check_List_Detail_Des
        FROM t_au_old_paras_fad f
       inner join t_audit_checklist_details csb
          on csb.id = f.process_detail
       inner join t_audit_checklist_sub cc
          on cc.s_id = csb.s_id
       inner join t_audit_checklist c
          on c.t_id = cc.t_id
       WHERE f.audited_by = UserEntityID
         and f.entity_id = entityId
       order by f.ID;
  end P_GetOldParasForResponse;

  procedure P_GetParasForComplianceByCAU(P_NO      in number,
                                         ENT_ID    in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select c.com_id,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             c.ind,
             c.cau_status,
             c.cau_assigned_ent_id
        FROM AIS_T_AU_POST_COMPLIANCE C
       inner join t_auditee_entities_maping m
          on m.entity_id = c.entity_id
       where c.ENTITY_ID = ENT_ID
         and C.com_stage = R_ID
         and C.com_status != 16
         and c.para_status = 8
         and c.cau_status is null
       order by C.audit_period desc, C.para_no asc;
  
  end P_GetParasForComplianceByCAU;

  procedure P_GetrealtionshiptypeforCAU(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select f.entity_realtion_id,
             f.parent_name || '   TO   ' || f.chlid_name as field_name
        from t_auditee_ent_relation f
       where f.status = 'Y'
         and f.id is not null
         and f.child_entity_typeid in (6, 28)
       order by f.id;
  end P_GetrealtionshiptypeforCAU;

  procedure P_GetparentrepofficeforCAU(rid       in number,
                                       ENT_ID    in number,
                                       io_cursor OUT t_cursor) is
  
    A_F number := 0;
    N_F number := 0;
  begin
    select e.type_id
      into N_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
    if (N_F = 25) then
      select e.auditby_id
        into A_F
        from t_auditee_entities e
       where e.entity_id = ENT_ID;
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
           and r.p_type_id = e.parent_entity_typeid
           and r.c_type_id = e.child_entity_typeid
           and r.relation_type_id = rid
           and r.auditedby = A_F
           and r.parent_id is not null
         order by r.p_name;
    end if;
  end P_GetparentrepofficeforCAU;

  procedure P_GetchildpostingforCAU(P_ENT_ID  in number,
                                    io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select distinct (r.c_name),
                      r.entity_id,
                      r.c_name,
                      r.status,
                      r.c_type_id as typeid,
                      R.COMPLICE_BY,
                      R.AUDIT_BY
      
        from v_get_parent_office r
       inner join t_auditee_ent_types t
          on t.autid = r.relation_type_id
       where r.parent_id = P_ENT_ID
       order by r.c_name;
  end P_GetchildpostingforCAU;

  Procedure P_FORWARD_CAU_PARA_TO_BRANCH(C_ID         number,
                                         ENT_ID       number,
                                         P_NO         number,
                                         R_ID         number,
                                         B_ENT_ID     number,
                                         CAU_COMMENTS varchar2,
                                         io_cursor    OUT t_cursor) as
    N_F number := 0;
    Z_B number := 0;
    T_F number := 0;
    E_F number := 0;
  begin
    select e.type_id
      into E_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
    if (E_F = 25) then
      select NVL(max(u.ppno), 0)
        into N_F
        from t_user u
       inner join t_user_maping m
          on m.ppno = u.ppno
       where u.ppno = P_NO
         and u.entity_id = ENT_ID
         and m.role_id = R_ID;
    
      if (N_F != 0) then
        commit;
      
        commit;*/
      
        update AIS_T_AU_POST_COMPLIANCE c
           set c.cau_status          = 1,
               c.cau_assigned_by     = P_NO,
               c.cau_assigned_on     = sysdate,
               c.cau_assigned_ent_id = B_ENT_ID
         where c.com_id = c_id;
        commit;
      
        select nvl(max(c.c_txt_id), 0)
          into T_F
          from AIS_T_AU_POST_COMPLIANCE_text_CAU c
         where c.com_id = C_ID;
      
        if (T_F = 0) then
          insert into AIS_T_AU_POST_COMPLIANCE_text_CAU
            (c_Txt_Id, Com_ID, Cau_Instructions, Status)
          values
            ((select COALESCE(max(ct.c_Txt_Id) + 1, 1)
               From AIS_T_AU_POST_COMPLIANCE_text_cau ct),
             C_ID,
             CAU_COMMENTS,
             'F');
          commit;
        else
          update AIS_T_AU_POST_COMPLIANCE_text_CAU c
             set c.cau_instructions = CAU_COMMENTS
           where c.com_id = C_ID;
          commit;
        end if;
        open io_cursor for
          select 'Para Forwarded to Branch' as remarks from dual;
      else
        open io_cursor for
          select 'Sorry you connection is lost, logout and login again' as remarks
            from dual;
      end if;
    else
      open io_cursor for
        select 'Sorry this facility is only for CAU' as remarks from dual;
    end if;
  
  end P_FORWARD_CAU_PARA_TO_BRANCH;

  procedure P_GetParasForComplianceByCAU_BY_BRANCH(P_NO      in number,
                                                   ENT_ID    in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select c.com_id,
             e.name as CAU_NAME,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             cau.cau_instructions,
             c.ind
        FROM AIS_T_AU_POST_COMPLIANCE C
       inner join t_auditee_entities e
          on e.entity_id = c.entity_id
       inner join ais_t_au_post_compliance_text_cau cau
          on cau.com_id = c.com_id
       where c.para_status = 8
         and c.cau_assigned_ent_id = ENT_ID
         and c.cau_status = 1
       order by C.audit_period desc, C.para_no asc;
  
  end P_GetParasForComplianceByCAU_BY_BRANCH;

  procedure P_GetParasForCompliance_CAU_para_text(c_id      number,
                                                  IND       varchar2,
                                                  io_cursor OUT t_cursor) as
  begin
    if (IND = 'O') then
      open io_cursor for
        select o.gist_of_paras as gist,
               pt.para_text as para_text,
               t.reply,
               nvl(t.c_txt_id, 0) as text_id,
               t.cau_instructions
          from AIS_T_AU_POST_COMPLIANCE p
          left join AIS_T_AU_POST_COMPLIANCE_text_CAU t
            on t.com_id = p.com_id
         inner join t_au_old_paras_fad o
            on o.id = p.old_para_id
         inner join t_au_old_paras_fad_text pt
            on o.ref_p = Pt.Ref_p
         where p.com_id = C_ID;
    else
      open io_cursor for
        select pt.headings as gist,
               pt.text as para_text,
               t.reply,
               nvl(t.c_txt_id, 0) as text_id,
               t.cau_instructions
          from AIS_T_AU_POST_COMPLIANCE p
          left join AIS_T_AU_POST_COMPLIANCE_text_CAU t
            on t.com_id = p.com_id
         inner join t_au_observation_text pt
            on p.new_para_id = pt.observatsion_id
         where p.com_id = C_ID;
    end if;
  
  end P_GetParasForCompliance_CAU_para_text;

  Procedure P_SubmitPostAuditCompliance_BY_BRANCH(C_ID        number,
                                                  T_ID        number,
                                                  ENT_ID      number,
                                                  P_NO        number,
                                                  R_ID        number,
                                                  Auditee_COM clob,
                                                  --  A_COMMENTS  varchar2,
                                                  io_cursor OUT t_cursor) as
    N_F number := 0;
    Z_B number := 0;
  begin
  
    select NVL(max(u.ppno), 0)
      into N_F
      from t_user u
     inner join t_user_context_assignment m
        on m.ppno = u.ppno
     where u.ppno = P_NO
       and u.entity_id = ENT_ID
       and m.role_id = R_ID;
  
    if (N_F != 0) then
      commit;
    
      commit;
    
      update AIS_T_AU_POST_COMPLIANCE c
         set c.cau_status     = 2,
             c.br_response_by = P_NO,
             c.br_response_on = sysdate
       where c.com_id = c_id;
      commit;
    
      Update AIS_T_AU_POST_COMPLIANCE_text_CAU c
         set c.reply = Auditee_COM, c.status = 'R'
       where c.com_id = C_ID
         and c.c_txt_id = T_ID;
      commit;
    
      open io_cursor for
        select 'Complaince Submitted to CAU' as remarks from dual;
    
    else
      open io_cursor for
        select 'Sorry you connection is lost, logout and login again' as remarks
          from dual;
    end if;
  end P_SubmitPostAuditCompliance_BY_BRANCH;

  procedure P_SubmitPostAuditCompliance_Evidence_By_BRANCH(TEXT_ID  in varchar2,
                                                           filename IN Varchar2,
                                                           len_id   in number,
                                                           enter_by IN number,
                                                           filetype in varchar2,
                                                           filedata IN clob,
                                                           seq_id   IN number) as
  
  begin
    insert into ais_t_au_post_compliance_evidence_cau
      (id,
       file_name,
       length,
       file_data,
       textid,
       enteredby,
       entereddate,
       sequence,
       description,
       COMP_ID)
    
    VALUES
      ((select COALESCE(max(ac.ID) + 1, 1)
         from ais_t_au_post_compliance_evidence_cau ac),
       filename,
       len_id,
       filedata,
       TEXT_ID,
       enter_by,
       sysdate,
       seq_id,
       filetype,
       (select max(t.com_id)
          from ais_t_au_post_compliance_text t
         where t.c_txt_id = TEXT_ID));
    commit;
  
  end P_SubmitPostAuditCompliance_Evidence_By_BRANCH;
  procedure P_GetAllCompliance_Evidence_CAU(TEXT_ID   in varchar2,
                                            io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             '' as file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence_cau
       where textid = TEXT_ID;
  
    commit;
  
  end P_GetAllCompliance_Evidence_CAU;
  procedure P_GetPostAuditCompliance_Evidence_FileData_CAU(FILE_ID   in varchar2,
                                                           io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence_cau
       where id = FILE_ID;
  
    commit;
  
  end P_GetPostAuditCompliance_Evidence_FileData_CAU;

  procedure P_GetParasForComplianceByCAU_FOR_REVIEW(P_NO      in number,
                                                    ENT_ID    in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
      select c.com_id,
             c.ind,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             c.cau_status,
             c.cau_assigned_ent_id
        FROM AIS_T_AU_POST_COMPLIANCE C
       where c.ENTITY_ID = ENT_ID
            --and C.com_stage = R_ID
         and C.com_status != 16
         and c.para_status = 8
         and c.cau_status = 2
       order by C.audit_period desc, C.para_no asc;
  
  end P_GetParasForComplianceByCAU_FOR_REVIEW;

  Procedure p_GetPostAuditComplianceSecuritySnapshot(CM_ID     in number,
                                                     io_cursor OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
    
      select C.COM_ID,
             C.ENTITY_ID,
             C.NAME,
             C.AUDIT_PERIOD,
             C.PARA_NO,
             C.NEW_PARAID,
             C.OLD_PARA_ID,
             C.COM_STAGE,
             C.COM_STATUS,
             C.COM_CYCLE,
             C.NEXT_R_ID,
             C.PER_R_ID,
             C.IND,
             C.REC_FROM
        from V_GET_AIS_POST_COMPLIANCE C
       where C.COM_ID = CM_ID;
  end;

  Procedure P_HasActiveUserContextAssignment(P_NO        in number,
                                             ENT_ID      in number,
                                             R_ID        in number,
                                             USER_CON_ID in number,
                                             io_cursor   OUT t_cursor) as
  
  begin
    OPEN io_cursor FOR
    
      select count(1)
        from T_USER_CONTEXT_ASSIGNMENT uca
       where uca.PPNO = P_NO
         and uca.ENTITY_ID = ENT_ID
         and uca.ROLE_ID = R_ID
         and uca.IS_ACTIVE = 'Y';
  
  end;
end PKG_AE;

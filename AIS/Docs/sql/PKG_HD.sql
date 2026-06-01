create or replace package PKG_HD is

  TYPE t_cursor IS REF CURSOR;

  procedure P_GetFinalizedDraftObservations(ENGID     IN NUMBER,
                                            ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure P_GetFinalizedDraftObservationsbranch(ENGID     IN NUMBER,
                                                  ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor);

  procedure P_Finalise_para(engplan_id    in number,
                            OBS_ID        IN NUMBER,
                            memo_number   in number,
                            NEW_STATUS_ID IN NUMBER,
                            Remarks       IN VARCHAR2,
                            para_no       in number,
                            PP_NO         IN NUMBER,
                            io_cursor     OUT t_cursor);

  procedure P_GetOldParas(Entityid in number, io_cursor OUT t_cursor);

  procedure P_GetOldSettledParas(Entityid  in number,
                                 io_cursor OUT t_cursor);

  procedure P_Get_All_Paras_monitoring(Entityid  in number,
                                       io_cursor OUT t_cursor);

  Procedure p_ppno_para(P_NO      in number,
                        R_ID      in number,
                        ENT_ID    in number,
                        PPNO      in number,
                        io_cursor OUT t_cursor);

  procedure P_GET_All_PARA_TEXT(CAT       in varchar2,
                                OBS_ID    in number,
                                Para_ID   IN NUMBER,
                                io_cursor OUT t_cursor);

  procedure p_ppno_name(ppno      in number,
                        ENT_ID    in number,
                        P_NO      in number,
                        R_ID      in number,
                        io_cursor OUT t_cursor);

  procedure P_GetEntitiesFornewPara(entityId  in number,
                                    io_cursor out t_cursor);

  procedure P_GetOldParasEntityid(Entityid  in number,
                                  ENT_ID    in number,
                                  P_NO      in number,
                                  R_ID      in number,
                                  io_cursor OUT t_cursor);

  procedure P_GetOldParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     R_ID         in number,
                                     io_cursor    OUT t_cursor);

  procedure P_GetnewParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     io_cursor    OUT t_cursor);

  procedure P_GetnewParasForResponse_reviewer(UserEntityID in number,
                                              io_cursor    OUT t_cursor);

  procedure P_UpdateOldParasFadsettleunsettle(PPNO       in number,
                                              PID        IN NUMBER,
                                              NEW_STATUS in number);

  procedure P_AddOldParas(PROCESS       in number,
                          SUBPROCESS    in number,
                          PROCESSDETAIL in number,
                          PPNO          in number,
                          PID           IN NUMBER,
                          REPLYTEXT     in clob);

  procedure P_GetnewParastext(obs_id in number, io_cursor OUT t_cursor);

  procedure p_get_para_responsibles(paraRef   in number,
                                    P_C       in varchar2,
                                    io_cursor OUT t_cursor);

  procedure P_get_para_evidences(ref_p      in varchar2,
                                 P_C        in varchar2,
                                 reply_date in date,
                                 io_cursor  OUT t_cursor);

  PROCEDURE P_Branch_risk_rating_model(ENGID     in number,
                                       Entityid  in number,
                                       io_cursor OUT t_cursor);

  PROCEDURE P_GET_Branch_risk_rating_model(ENGID     in number,
                                           Entityid  in number,
                                           io_cursor OUT t_cursor);

  procedure P_ChangeStatusRequestForSettledPara(RefP      in varchar2,
                                                au_obs_id in number,
                                                IND       in varchar2,
                                                NewStatus in number,
                                                PPNO      IN NUMBER,
                                                Remarks   in varchar2,
                                                io_cursor OUT t_cursor) ;

  procedure P_ChangeStatusRequestForSettledPara_new(obs_id    in number,
                                                    NewStatus in number,
                                                    Remarks   in varchar2,
                                                    ENT_ID    in number,
                                                    P_NO      in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor);

  procedure P_ChangeStatusRequestForSettledPara_new_reviewer(obsid     in number,
                                                             P_IND     in varchar2,
                                                             Remark    in varchar2,
                                                             IND       in varchar2,
                                                             ENT_ID    in number,
                                                             P_NO      in number,
                                                             R_ID      in number,
                                                             io_cursor OUT t_cursor);

  procedure P_GetOldParastext(para_ref in varchar2, io_cursor OUT t_cursor);

  procedure P_get_audit_pre_Concluding_entities(userentityid in t_Au_Plan_Eng.Eng_Id%type,
                                                io_cursor    OUT t_cursor);

  procedure P_get_audit_pre_Concluding(engid     in number,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor);

  procedure P_get_audit_Concluding_entities(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure P_audit_pre_Concluding(obsid     in number,
                                   gist      in varchar2,
                                   recom     in varchar2,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure P_audit_pre_submission(engid     in t_Au_Plan_Eng.Eng_Id%type,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure P_audit_Concluding(engid     in t_Au_Plan_Eng.Eng_Id%type,
                               ENT_ID    in number,
                               P_NO      in number,
                               R_ID      in number,
                               io_cursor OUT t_cursor);

  procedure P_reviewed_legacy_Para(ref_id    in varchar2,
                                   ppno      in number,
                                   io_cursor OUT t_cursor);

  procedure P_GetAuditEntitiestype(ENTITYID  IN NUMBER,
                                   io_cursor OUT t_cursor);

  procedure P_GetAuditEntities(typeid    in number,
                               ENTITYID  IN NUMBER,
                               io_cursor OUT t_cursor);

  procedure P_GetAuditYear(io_cursor OUT t_cursor);

  procedure P_GetAuditnature(io_cursor OUT t_cursor);

  procedure P_add_legacy_Para(typeid      in number,
                              audityear   in varchar2,
                              PARANO      in varchar2,
                              GIST        in varchar2,
                              ANEXURE     in varchar2,
                              amount      in varchar2,
                              VOL         in varchar2,
                              Entityid    in number,
                              USER_ENT_ID IN NUMBER,
                              nature      in number,
                              ppno        in number,
                              io_cursor   OUT t_cursor);

  procedure P_Get_legacy_Para_to_authorize(ENTITYID  IN NUMBER,
                                           io_cursor OUT t_cursor);

  procedure P_Authorize_legacy_Para_addition(RefP      in varchar2,
                                             PPNO      IN NUMBER,
                                             io_cursor OUT t_cursor);

  procedure P_referedback_Del_para(RefP      in varchar2,
                                   PPNO      IN NUMBER,
                                   io_cursor OUT t_cursor);

  Procedure p_update_para_no(obs_id    in number,
                             para_no   in number,
                             io_cursor OUT t_cursor);

  procedure P_GetSettledParasForReview(P_NO      in number,
                                       ENT_ID    in number,
                                       R_ID      in number,
                                       MON       in varchar2,
                                       Yr        in varchar2,
                                       io_cursor OUT t_cursor);

  procedure P_ADD_DUPLICATE_PARAS(o_para_id in number,
                                  n_para_id in number,
                                  p_ind     in varchar2,
                                  r_remarks in varchar2,
                                  P_NO      in number,
                                  ENT_ID    in number,
                                  R_ID      in number,
                                  io_cursor OUT t_cursor);

  procedure P_GET_DUPLICATE_PARAS_ENT_FOR_AUTH(P_NO      in number,
                                               ENT_ID    in number,
                                               R_ID      in number,
                                               io_cursor OUT t_cursor);
  procedure P_GET_DUPLICATE_PARAS_FOR_AUTH(P_NO      in number,
                                           ENT_ID    in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor);

  procedure P_AUTH_DUPLICATE_PARAS(DID       in number,
                                   P_NO      in number,
                                   ENT_ID    in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure P_REJECT_DUPLICATE_PARAS(DID       in number,
                                     P_NO      in number,
                                     ENT_ID    in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor);

  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID(obid      number,
                                              P_NO      NUMBER,
                                              R_ID      NUMBER,
                                              ENT_ID    NUMBER,
                                              io_cursor Out t_cursor);
  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON(obid      number,
                                                      P_NO      NUMBER,
                                                      R_ID      NUMBER,
                                                      ENT_ID    NUMBER,
                                                      io_cursor Out t_cursor);

  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID_HO(obid      number,
                                                 P_NO      NUMBER,
                                                 R_ID      NUMBER,
                                                 ENT_ID    NUMBER,
                                                 io_cursor Out t_cursor);
  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON_HO(obid      number,
                                                         P_NO      NUMBER,
                                                         R_ID      NUMBER,
                                                         ENT_ID    NUMBER,
                                                         io_cursor Out t_cursor);

  PROCEDURE P_GET_PRECON_DISPLAY(i_obid    IN NUMBER,
                                 i_p_no    IN NUMBER,
                                 i_r_id    IN NUMBER,
                                 i_ent_id  IN NUMBER,
                                 io_cursor OUT t_cursor);

  procedure P_audit_para_update_svz_az(OBID         in number,
                                       ANXID        in number,
                                       PROCID       in number,
                                       SUB_PROCID   in number,
                                       PROC_DETID   in number,
                                       RISKID       in number,
                                       FINAL_PARA   in number,
                                       PARA_GIST    in varchar2,
                                       TEXT_OF_PARA in clob,
                                       AMOUNT_INV   in number,
                                       NO_INST      in number,
                                       P_NO         in number,
                                       ENT_ID       in number,
                                       R_ID         in number,
                                       io_cursor    OUT t_cursor);
  procedure P_audit_para_update_head_dept(OBID         in number,
                                          V_ID         in number,
                                          V_NATUREID   in number,
                                          RISKID       in number,
                                          PARA_GIST    in varchar2,
                                          TEXT_OF_PARA in clob,
                                          P_NO         in number,
                                          ENT_ID       in number,
                                          R_ID         in number,
                                          io_cursor    OUT t_cursor);

  Procedure P_UPLOAD_AUDIT_REPORT(ENGID     number,
                                  AREP      clob,
                                  REP_TYPE  varchar2,
                                  REP_NAME  varchar2,
                                  P_NO      number,
                                  R_ID      number,
                                  ENT_ID    number,
                                  io_cursor out t_cursor);

  Procedure P_GET_FINAL_AUDIT_REPORT(ENGID     number,
                                     P_NO      number,
                                     R_ID      number,
                                     ENT_ID    number,
                                     io_cursor out t_cursor);

  Procedure P_GET_AUDIT_REPORT_CONTENT(FILE_ID   number,
                                       P_NO      number,
                                       R_ID      number,
                                       ENT_ID    number,
                                       io_cursor out t_cursor);
  Procedure P_GET_CHECK_AUDIT_REPORT_UPLOADED(ENGID     number,
                                              P_NO      number,
                                              R_ID      number,
                                              ENT_ID    number,
                                              io_cursor out t_cursor);

  procedure P_Get_Paras_For_Status_Change(ENT_ID    in number,
                                          R_ID      in number,
                                          entityId  in number,
                                          io_cursor OUT t_cursor);

  PROCEDURE P_Add_Paras_For_Status_Change(C_ID       IN NUMBER,
                                          NewStatus  IN NUMBER,
                                          Remarks    IN VARCHAR2,
                                          IND        IN VARCHAR2,
                                          Action_IND IN VARCHAR2,
                                          ENT_ID     IN NUMBER,
                                          P_NO       IN NUMBER,
                                          R_ID       IN NUMBER,
                                          io_cursor  OUT t_cursor);

  procedure P_Get_Paras_For_Status_Change_For_Authorize(ENT_ID    in number,
                                                        io_cursor OUT t_cursor);

  PROCEDURE P_Authorize_Paras_For_Status(C_ID       IN NUMBER,
                                         N_PARA_ID  IN NUMBER,
                                         O_PARA_ID  IN NUMBER,
                                         remark     IN VARCHAR2,
                                         IND        IN VARCHAR2,
                                         Action_IND IN VARCHAR2,
                                         ENT_ID     IN NUMBER,
                                         P_NO       IN NUMBER,
                                         R_ID       IN NUMBER,
                                         io_cursor  OUT t_cursor);

end PKG_HD;

create or replace package body PKG_HD is

  procedure P_GetFinalizedDraftObservations(ENGID     IN NUMBER,
                                            ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor) is
    B_N varchar2(100);
  begin
    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    OPEN io_Cursor FOR
      select ot.headings as Title,
             o.ID as OBS_ID,
             o.memo_number as MEMO_NO,
             nvl(o.Draft_Para_No, 0) as DRAFT_PARA,
             nvl(o.Final_Para_No, 0) as FINAL_PARA,
             o.severity as OBS_RISK_ID,
             r.description as OBS_RISK,
             o.status as OBS_STATUS_ID,
             ost.Statusname as OBS_STATUS,
             p.description as period,
             et.name as entity_name

        from t_au_observation o
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_status ost
          on o.status = ost.statusid
       inner join t_au_plan_eng e
          on e.eng_id = o.engplanid
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       inner join t_auditee_entities et
          on et.entity_id = e.entity_id
       where o.engplanid = ENGID
       order by o.status, o.final_para_no, o.memo_number;

  end P_GetFinalizedDraftObservations;

  procedure P_GetFinalizedDraftObservationsbranch(ENGID     IN NUMBER,
                                                  ENT_ID    in number,
                                                  P_NO      in number,
                                                  R_ID      in number,
                                                  io_cursor OUT t_cursor) is
    O_F number := 0;
    M_F number := 0;
    Z_B number := 0;
    B_N varchar2(100);
  begin
    select '-' into B_N from dual;

    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

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
       'Viewed / Finalize Draft Observations of ' ||
       (select bt.name
          from t_auditee_entities bt
         inner join t_au_plan_eng e
            on bt.entity_id = e.entity_id
         where e.eng_id = ENGID),
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_B
           and l.ppnum = P_NO),
       'Y');
    commit;
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
               0 as headings,
               0 as PERIOD,
               o.id as OBS_ID,
               0 as ENTITY_NAME,
               0 as MEMO_NO,
               nvl(o.Draft_Para_No, 0) as DRAFT_PARA,
               nvl(o.Final_Para_No, 0) as FINAL_PARA,
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
               ot.headings,
               p.description as PERIOD,
               o.ID as OBS_ID,
               aee.name as ENTITY_NAME,
               o.memo_number as MEMO_NO,
               nvl(o.Draft_Para_No, 0) as DRAFT_PARA,
               nvl(o.Final_Para_No, 0) as FINAL_PARA,
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
         order by o.memo_number;
    end if;
  end P_GetFinalizedDraftObservationsbranch;

  procedure P_Finalise_para(engplan_id    in number,
                            OBS_ID        IN NUMBER,
                            memo_number   in number,
                            NEW_STATUS_ID IN NUMBER,
                            Remarks       IN VARCHAR2,
                            para_no       in number,
                            PP_NO         IN NUMBER,
                            io_cursor     OUT t_cursor) is

    S_Z number := 0;
    P_O number := 0;
    B_N varchar2(100);
  begin

    select NVL(max(p.id), 0)
      into P_O
      from t_audit_para p
     where p.engplanid = engplan_id
       and p.obid = OBS_ID;
    if (P_O = 0) then
      select nvl(max(mp.role_id), 0)
        into S_Z
        from t_user_maping mp
       where mp.ppno = PP_NO;
      if (S_Z = 15) then

        UPDATE T_AU_OBSERVATION o
           SET o.status = NEW_STATUS_ID
         WHERE o.id = OBS_ID;
        COMMIT;
        insert into t_audit_para
          (id, engplanid, obid, status, memo_number, para_no)
        values
          ((select COALESCE(max(acc.ID) + 1, 1) from t_audit_para acc),
           engplan_id,
           OBS_ID,
           NEW_STATUS_ID,
           memo_number,
           para_no);
        commit;
        insert into t_au_observation_final_reccomendation
          (id, obs_id, recommendation, entered_by, entered_on)
        values
          ((select COALESCE(max(acc.ID) + 1, 1)
             from t_au_observation_final_reccomendation acc),
           OBS_ID,
           Remarks,
           PP_NO,
           sysdate);
        commit;

        open io_cursor for
          select '1' as ref, r.statusname as remarks
            from t_au_observation_status r
           where r.statusid = NEW_STATUS_ID;
      else
        open io_cursor for
          select r.ref, r.remarks from t_au_remarks r where r.id = 22;
      end if;

    else
      P_add_error_log('HD',
                      'P_audit_pre_Concluding',
                      'PP No was null',
                      OBS_ID);
      commit;
      open io_cursor for
        select '1' as ref,
               'Please contact system Administrator as para already finalized' as remarks
          from dual;

    end if;
  end P_Finalise_para;

  procedure P_GetOldParasEntityid(Entityid  in number,
                                  ENT_ID    in number,
                                  P_NO      in number,
                                  R_ID      in number,
                                  io_cursor OUT t_cursor) is
    N_F number := 0;
    A_D number := 0;
    B_N varchar2(100);
  begin

    select nvl(max(name), 'Unknown')
      into B_N
      from t_auditee_entities
     where entity_id = Entityid;

    select nvl(max(e.type_id), 0)
      into N_F
      from t_auditee_entities e
     where (e.entity_id = Entityid or e.code = Entityid);
    select nvl(max(ee.auditby_id), 0)
      into A_D
      from t_auditee_entities ee
     inner join t_auditee_entities_maping mm
        on mm.entity_id = ee.entity_id
     where (mm.parent_id = Entityid or mm.parent_code = Entityid);
    if (N_F in (4, 5, 25)) then
      open io_cursor for
        select m.entity_id as branchentityid, m.c_name as branchname
          from v_get_parent_office m
         WHERE (m.parent_id = Entityid or m.parent_code = Entityid)
           and m.relation_type_id in (4, 5)
          -- and m.auditedby = ENT_ID
         order by m.c_name;
    else
      open io_cursor for
        select e.entity_id as branchentityid, e.name as branchname
          from t_auditee_entities_maping m
         inner join t_auditee_entities e
            on e.entity_id = m.entity_id
         WHERE m.parent_id = Entityid
           --and e.auditby_id = ENT_ID
           and m.relation_type_id in (4, 5)
         order by m.c_name;
    end if;

  end P_GetOldParasEntityid;

  procedure P_GetOldParas(Entityid in number, io_cursor OUT t_cursor) is

    v_count number := 0;
    B_N     varchar2(100);

  begin

    open io_cursor for
      select f.id,
             f.ref_p,
             f.entity_id,
             f.entity_code,
             f.type_id,
             f.audit_period,
             f.entity_name,
             f.para_no,
             f.gist_of_paras,
             f.annexure,
             f.amount_involved,
             f.vol_i_ii,
             f.audited_by,
             f.process_detail,
             f.status,
             f.entered_on,
             f.entered_by,
             f.para_status,
             f.update_status,
             f.az_status_updated_by,
             f.az_updated_on,
             f.fad_status,
             f.fad_reviewed_by,
             f.fad_reviewed_on,
             f.risk,
             f.temp_status_for_change,
             f.nature_of_audit,
             f.annex,
             f.no_of_instances,
             f.parastatusupdatedby,
             f.parasetteledon,
             f.audited_by as auditedby
        from t_au_old_paras_fad f
       WHERE f.audited_by = Entityid
         and f.para_status not in (6, 8)
         and not exists (select 'z'
                from t_au_old_paras_fad_text nt
               where f.ref_p = nt.ref_p)
       order by f.audit_period desc, ID;

  end P_GetOldParas;

  procedure P_GetOldSettledParas(Entityid  in number,
                                 io_cursor OUT t_cursor) is

    B_N varchar2(100);
  begin

    select nvl(max(entity_name), 'Unknown')
      into B_N
      from t_au_old_paras_fad
     where audited_by = Entityid;

    P_add_error_log('HD',
                    'P_audit_pre_Concluding',
                    'PP No was null',
                    Entityid);

    open io_cursor for
      select f.*
        from t_au_old_paras_fad f
       WHERE f.audited_by = Entityid
         and f.para_status in (6)
       order by f.audit_period desc, ID;

  end P_GetOldSettledParas;

  procedure P_Get_All_Paras_Monitoring(Entityid  in number,
                                       io_cursor OUT t_cursor) is

  begin

    open io_cursor for
      select f.old_para_id OLD_PARA_ID,
             case
               when f.old_para_id is not null then
                f.old_para_id
               else
                f.new_para_id
             end as obs_id,
             e.name as entity_name,
             f.gist_of_paras,
             f.para_no,
             f.audit_period,
             r.description as para_RISK,
             f.IND
        from AIS_T_AU_POST_COMPLIANCE f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       inner join t_risk r
          on r.r_id = f.risk
       WHERE f.entity_id = Entityid and f.para_status = 8
       order by f.audit_period;

  end P_Get_All_Paras_Monitoring;

  procedure P_GetOldParastext(para_ref in varchar2, io_cursor OUT t_cursor) is

  begin

    open io_cursor for
      select ot.*
        from t_au_old_paras_fad_text ot
       WHERE ot.ref_p = para_ref;

  end P_GetOldParastext;

  procedure P_GetnewParastext(obs_id in number, io_cursor OUT t_cursor) is
  begin

    P_add_error_log('HD',
                    'P_audit_pre_Concluding',
                    'PP No was null',
                    obs_id);

    open io_cursor for
      select ot.text, ot.headings
        from t_au_observation_text ot
       WHERE ot.observatsion_id = obs_id;

  end P_GetnewParastext;

  procedure P_GetEntitiesFornewPara(entityId  in number,
                                    io_cursor out t_cursor) as
  begin
    if (entityId in (112242, 112248, 112243)) then

      open io_cursor for
        select e.name || ' ( ' || eg.audit_startdate || ' from ' ||
               eg.audit_enddate || ' )' as name,
               e.entity_id,
               eg.eng_id
          from t_au_plan_eng eg
         inner join t_auditee_entities e
            on e.entity_id = eg.entity_id
           and eg.period_id > 1
         inner join v_get_parent_office mp
            on mp.entity_id = e.entity_id
         where eg.auditby_id = entityId
         order by e.name;
    else
      open io_cursor for
        select e.name || ' ( ' || eg.audit_startdate || ' from ' ||
               eg.audit_enddate || ' )' as name,
               e.entity_id,
               eg.eng_id

          from t_au_plan_eng eg
         inner join t_auditee_entities e
            on e.entity_id = eg.entity_id
           and eg.period_id > 1
         where e.auditby_id = entityId
         order by e.name;

    end if;

  end P_GetEntitiesFornewPara;

  procedure P_GetOldParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     R_ID         in number,
                                     io_cursor    OUT t_cursor) is
  begin

    if (R_ID in (15, 16)) then
      open io_cursor for
        SELECT f.id,
               null as au_obs_id,
               f.ref_p,
               f.entity_id,
               f.entity_code,
               f.audit_period,
               f.entity_name,
               f.para_no,
               f.gist_of_paras,
               f.amount_involved,
               (case
                 when f.para_status = 8 then
                  'Un-settle'
                 else
                  'settle'
               end) as para_status,
               'O' as IND
          FROM t_au_old_paras_fad f
         WHERE f.entity_id = entityId
           and not exists (select 'z'
                  from T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG lg
                 where lg.ref_p = f.ref_p
                   and lg.authorized_by is null)
         order by f.ID;
    else
      open io_cursor for
        SELECT f.para_id as id,
               f.para_id as au_obs_id,
               '' as ref_p,
               e.entity_id,

               E.CODE AS ENTITY_CODE,
               e.type_id,
               f.period as audit_period,
               f.entity_name,
               f.para_no,
               f.gist_of_paras,
               null as AMOUNT_INVOLVED,
               e.auditby_id as AUDITED_BY,
               (case
                 when f.para_status = 8 then
                  'Un-settle'
                 else
                  'settle'
               end) as para_status,
               'C' as IND
          FROM t_au_observation_old_cad_paras f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         WHERE e.entity_id = entityId
           and not exists (select 'z'
                  from T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG lg
                 where lg.obs_id = f.para_id
                   and lg.authorized_by is null)
         order by f.para_id;
    end if;

  end P_GetOldParasForResponse;

  procedure P_GetnewParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     io_cursor    OUT t_cursor) is
  begin
    open io_cursor for
      SELECT o.id,
             e.entity_id,
             eg.entity_code,
             p.description as audit_period,
             e.name as entity_name,
             o.final_para_no as para_no,
             tx.headings as gist_of_para,
             o.amount_involved,
             'A' as Ind,
             -- changes has to be made under as s.para_status
             (Case
               when s.statusid = 8 then
                'Un-Settled'
               else
                (case
                  when s.statusid in (9, 6) then
                   'Settled'

                end)
             end) as para_status
        FROM t_au_observation o
       inner join t_au_plan_eng eg
          on eg.eng_id = o.engplanid
       inner join t_auditee_entities e
          on e.entity_id = eg.entity_id
       inner join t_au_period p
          on p.auditperiodid = eg.period_id
       inner join t_au_observation_text tx
          on tx.observatsion_id = o.id
       inner join t_au_observation_status s
          on s.statusid = o.status
       inner join T_AU_OBSERVATION_FAD f
          on f.new_paraid = o.id
      -- and f.para_status = 8
       WHERE eg.eng_id = entityId

         and eg.period_id > 1
         and not exists (select 'z'
                from T_AU_new_PARAS_STATUS_CHANGE_LOG lg
               where lg.au_obs_id = o.id)
       order by o.final_para_no;
  end P_GetnewParasForResponse;

  procedure P_GetnewParasForResponse_reviewer(UserEntityID in number,
                                              io_cursor    OUT t_cursor) is
  begin
    open io_cursor for

      SELECT o.id,
             e.entity_id,
             eg.entity_code,
             p.description as audit_period,
             e.name as entity_name,
             o.memo_number as para_no,
             tx.headings as gist_of_para,
             o.amount_involved,
             lg.remarks,
             'A' as IND,
             -- changes has to be made under as s.para_status
             (Case
               when s.statusid = 8 then
                8
               else
                (case
                  when s.statusid = 9 then
                   6
                  else
                   0
                end)
             end) as para_status
        FROM t_au_observation o
       inner join t_au_plan_eng eg
          on eg.eng_id = o.engplanid
       inner join t_auditee_entities e
          on e.entity_id = eg.entity_id
       inner join t_au_period p
          on p.auditperiodid = eg.period_id
       inner join t_au_observation_text tx
          on tx.observatsion_id = o.id
       inner join t_au_observation_status s
          on s.statusid = o.status
       inner join T_AU_new_PARAS_STATUS_CHANGE_LOG lg
          on lg.au_obs_id = o.id
       WHERE eg.auditby_id = UserEntityID
         and lg.reviewed_by is null
       order by o.memo_number;
  end P_GetnewParasForResponse_reviewer;

  procedure P_UpdateOldParasFadsettleunsettle(PPNO       in number,
                                              PID        IN NUMBER,
                                              NEW_STATUS in number) as

  begin

    UPDATE T_AU_OLD_PARAS_FAD al
       SET al.Para_Status         = NEW_STATUS,
           al.parastatusupdatedby = PPNO,
           al.parasetteledon      = sysdate
     WHERE al.ID = PID;
    commit;

  end P_UpdateOldParasFadsettleunsettle;

  procedure P_ChangeStatusRequestForSettledPara_new(obs_id    in number,
                                                    NewStatus in number,
                                                    Remarks   in varchar2,
                                                    ENT_ID    in number,
                                                    P_NO      in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor) as
    M_F number := 0;
    S_F number := 0;
    Z_R number := 0;
  begin

    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || OBS_ID);

    select NVL(MAX(l.id), 0)
      into Z_R
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_R;
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
       222,
       'Para Marked as settled in AIS',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_R
           and l.ppnum = P_NO),
       'Y');
    commit;

    select nvl(max(l.id), 0)
      into S_F
      from T_AU_new_PARAS_STATUS_CHANGE_LOG l
     where (l.obs_id = obs_id);
    if (S_F < 0) then
      open io_cursor for
        select 'Request for updation in Para Status already exist, One para is allowed only once. Please contact System Administrator' as remark
          from dual;

    else
      select fd.status
        into M_F
        from t_au_observation fd
       where fd.id = obs_id;
      if (M_F = NewStatus) then
        open io_cursor for
          select 'Same status Para cannot be submitted' as remark
            from dual;
      else
        INSERT INTO T_AU_new_PARAS_STATUS_CHANGE_LOG
          (ID,
           au_obs_id,
           Old_Status,
           New_Status,
           Remarks,
           Created_Date,
           Created_By,
           sequence)
        VALUES
          ((SELECT COALESCE(max(u.ID) + 1, 1)
             FROM T_AU_new_PARAS_STATUS_CHANGE_LOG U),
           obs_id,
           (Select f.status from t_au_observation f where f.id = obs_id),
           NewStatus,
           Remarks,
           sysdate,
           P_NO,
           (SELECT COALESCE(max(ul.sequence) + 1, 1)
              FROM T_AU_new_PARAS_STATUS_CHANGE_LOG ul));
        COMMIT;

        P_add_error_log('HD',
                        'P_audit_pre_Concluding',
                        'PP No was null',
                        obs_id);
        commit;

        open io_cursor for
          select 'Request for updation in Para Status submitted to Reviewer' as remark
            from dual;

      end if;
    end if;
  end P_ChangeStatusRequestForSettledPara_new;

  procedure P_ChangeStatusRequestForSettledPara_new_reviewer(obsid     in number,
                                                             P_IND     in varchar2,
                                                             Remark    in varchar2,
                                                             IND       in varchar2,
                                                             ENT_ID    in number,
                                                             P_NO      in number,
                                                             R_ID      in number,
                                                             io_cursor OUT t_cursor) as
    O_B number := 0;
    Z_R number := 0;
  begin

    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       221,
                       'Change Status Request For ' || obsid);

    select obsid into O_B from dual;
    if ind = 'A' then
      update T_AU_new_PARAS_STATUS_CHANGE_LOG ls
         set ls.reviewed_by       = p_no,
             ls.reviewed_on       = sysdate,
             ls.reviewer_comments = Remark,
             ls.ind               = P_IND
       where ls.au_obs_id = O_B;
      COMMIT;

    elsif ind = 'R' then
      delete from T_AU_new_PARAS_STATUS_CHANGE_LOG l
       where l.au_obs_id = O_B
         and l.ind = P_IND;
      commit;
    end if;

    P_add_error_log('HD',
                    'P_audit_pre_Concluding',
                    'PP No was null',
                    obsid);
    commit;

    open io_cursor for
      select ' Request for updation in Para Status submitted to Authorizer' as remark
        from dual;

  end P_ChangeStatusRequestForSettledPara_new_reviewer;

  procedure P_ChangeStatusRequestForSettledPara(RefP      in varchar2,
                                                au_obs_id in number,
                                                IND       in varchar2,
                                                NewStatus in number,
                                                PPNO      IN NUMBER,
                                                Remarks   in varchar2,
                                                io_cursor OUT t_cursor) as
    M_F number := 0;
    S_F number := 0;
    B_N varchar2(100);
  begin

    if (PPNO is not null) then
      select nvl(max(l.id), 0)
        into S_F
        from T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG l
       where (l.ref_p = refp);
      if (S_F < 0) then
        open io_cursor for
          select 'Request for updation in Para Status already exist, One para is allowed only once. Please contact System Administrator' as remark
            from dual;
      else
        if (IND = 'O') then
          select fd.para_status
            into M_F
            from t_au_old_paras_fad fd
           where fd.ref_p = RefP;
          if (M_F = NewStatus) then
            open io_cursor for
              select 'Same status Para cannot be submitted' as remark
                from dual;
          else
            INSERT INTO T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG
              (ID,
               REF_P,
               Old_Status,
               New_Status,
               Remarks,
               Created_Date,
               Created_By,
               sequence)
            VALUES
              ((SELECT COALESCE(max(u.ID) + 1, 1)
                 FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG U),
               RefP,
               (Select Para_Status
                  from T_AU_OLD_PARAS_FAD
                 where ref_p = RefP),
               NewStatus,
               Remarks,
               sysdate,
               PPNO,
               (SELECT COALESCE(max(ul.sequence) + 1, 1)
                  FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG ul));
            COMMIT;
            UPDATE T_AU_OLD_PARAS_FAD al
               SET al.temp_status_for_change = NewStatus,
                   al.parastatusupdatedby    = PPNO
             WHERE al.ref_p = RefP;
            commit;
            open io_cursor for
              select 'Request for updation in Para Status submitted to Head FAD' as remark
                from dual;
          end if;
        else
          if (IND = 'C') then
            select md.para_status
              into M_F
              from t_au_observation_old_cad_paras md
             where md.para_id = au_obs_id;
            if (M_F = NewStatus) then
              open io_cursor for
                select 'Same status Para cannot be submitted' as remark
                  from dual;
            else
              INSERT INTO T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG
                (ID,
                 REF_P,
                 Old_Status,
                 New_Status,
                 Remarks,
                 Created_Date,
                 Created_By,
                 sequence,
                 OBS_ID)
              VALUES
                ((SELECT COALESCE(max(u.ID) + 1, 1)
                   FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG U),
                 RefP,
                 (Select Para_Status
                    from T_AU_OLD_PARAS_FAD
                   where ref_p = RefP),
                 NewStatus,
                 Remarks,
                 sysdate,
                 PPNO,
                 (SELECT COALESCE(max(ul.sequence) + 1, 1)
                    FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG ul),
                 au_obs_id);
              COMMIT;

              open io_cursor for
                select 'Request for updation in Para Status submitted to Departmental Head' as remark
                  from dual;
            end if;
          end if;
        end if;
      end if;
    else
      open io_cursor for
        select 'Session Expired, Logout and login again' as remark
          from dual;
    end if;

  end P_ChangeStatusRequestForSettledPara;

  procedure P_AddOldParas(PROCESS       in number,
                          SUBPROCESS    in number,
                          PROCESSDETAIL in number,
                          PPNO          in number,
                          PID           IN NUMBER,
                          REPLYTEXT     in clob) as

    B_N varchar2(100);
  begin

    UPDATE T_AU_OLD_PARAS_FAD al
       SET al.PROCESS_DETAIL = PROCESSDETAIL,
           al.STATUS         = 1,
           al.ENTERED_BY     = ppno,
           al.ENTERED_ON     = sysdate
     WHERE al.ID = PID;
    commit;

    UPDATE T_AU_OLD_PARAS_FAD_TEXT FD
       SET FD.PARA_TEXT = REPLYTEXT
     WHERE FD.ID = PID;
    COMMIT;

  end P_AddOldParas;
  -- Ali & Asfand
  procedure p_get_para_responsibles(paraRef   in number,
                                    P_C       in varchar2,
                                    io_cursor OUT t_cursor) as

  begin
    if (P_C = 'O') then
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
         WHERE f.ref_p = paraRef
           and f.status = 'N';
    else
      if (P_C = 'N') then
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
           WHERE f.au_obs_id = paraRef
             and f.status = 'N';
      end if;
    end if;
  end p_get_para_responsibles;

  procedure P_get_para_evidences(ref_p      in varchar2,
                                 P_C        in varchar2,
                                 reply_date in date,
                                 io_cursor  OUT t_cursor) is
  begin
    if (P_C = 'O') then
      open io_cursor for
        select *
          from T_AU_OLD_PARAS_RESPONSE_EVIDENCES e
         where e.para_ref = ref_p
         order by e.sequence;
    else
      if (P_C = 'N') then
        open io_cursor for
          select *
            from T_AU_OLD_PARAS_RESPONSE_EVIDENCES e
          --where e.AU_OBS_ID = obs_id
           order by e.sequence;
      end if;
    end if;

  end P_get_para_evidences;

  PROCEDURE P_Branch_risk_rating_model(ENGID     in number,
                                       Entityid  in number,
                                       io_cursor OUT t_cursor) is

  begin

    DELETE FROM T_RISK_BRANCH_WISE;
    DELETE FROM T_BRANCH_RISK_RATING;
    COMMIT;

    INSERT INTO T_RISK_BRANCH_WISE
      (AUDIT_PERIOD,
       ENG_ID,
       entity_id,
       GR_ID,
       S_GR_ID,
       MAX_NUMBER,
       WEIGHTAGE_AVERAGE,
       GRAVITY_RISK)

      SELECT P.AUDITPERIODID,
             E.ENG_ID,
             e.entity_id,
             rs.gr_id,
             rs.s_gr_id,
             rs.max_number,
             rs.weightage as Weighted_Average,
             RS.GRAVITY

        FROM T_AU_PERIOD P, T_AU_PLAN_ENG E, T_R_SUB_GROUP RS
       where p.auditperiodid = e.period_id
         and e.eng_id = ENGID;
    commit;

    update T_RISK_BRANCH_WISE ts
       set ts.number_of_observations =
           (select count(os.id)
              from t_au_observation os
             inner join t_audit_checklist_details d
                on d.id = os.checklistdetail_id
             where os.engplanid = ts.eng_id
               and os.entity_id = ts.entity_id
               and d.v_id = ts.s_gr_id
               and os.status = 8);
    commit;

    update T_RISK_BRANCH_WISE t
       set t.risk_based_marks =
           (t.number_of_observations * T.GRAVITY_RISK)
     where t.entity_id = Entityid;
    commit;

    update T_RISK_BRANCH_WISE t
       set t.weighted_average_marks =
           (t.number_of_observations * T.GRAVITY_RISK)
     where t.entity_id = Entityid;
    commit;

    update T_RISK_BRANCH_WISE t
       set t.final_marks = (case
                             when t.weighted_average_marks > t.max_number then
                              t.max_number
                             else
                              t.weighted_average_marks
                           end)
     where t.entity_id = Entityid;
    commit;

    INSERT INTO T_BRANCH_RISK_RATING
      (AUDIT_PERIOD_ID, Entity_id, RISK_RATING)

      SELECT BB.AUDIT_PERIOD, bb.entity_id, SUM(BB.FINAL_MARKS)
        FROM T_RISK_BRANCH_WISE BB
       where bb.eng_id = engid
         and bb.entity_id = Entityid
       GROUP BY BB.AUDIT_PERIOD, BB.ENTITY_ID;
    COMMIT;
    UPDATE T_BRANCH_RISK_RATING b
       set b.risk_category =
           (select r.rating
              from T_COSO_RATING r
             where b.risk_rating between (r.range_start) and (r.range_end));
    commit;

    open io_cursor for
      select * from T_RISK_BRANCH_WISE b where b.eng_id = ENGID;

  end P_Branch_risk_rating_model;

  PROCEDURE P_GET_Branch_risk_rating_model(ENGID     in number,
                                           Entityid  in number,
                                           io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select r.description            as Mian_Areas,
             rs.description           as Sub_areas,
             o.max_number,
             o.weightage_average,
             o.gravity_risk,
             o.number_of_observations,
             o.risk_based_marks,
             o.weighted_average_marks,
             o.final_marks

        FROM T_R_SUB_GROUP RS
       INNER JOIN T_R_GROUP R
          ON R.GR_ID = RS.GR_ID
       inner join T_RISK_BRANCH_WISE O
          on o.gr_id = r.gr_id
         and o.s_gr_id = rs.s_gr_id

       where o.eng_id = ENGID
         and o.entity_id = Entityid;

  end P_GET_Branch_risk_rating_model;

  Procedure p_ppno_para(P_NO      in number,
                        R_ID      in number,
                        ENT_ID    in number,
                        PPNO      in number,
                        io_cursor OUT t_cursor) is

  
  begin

   
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Para checked by ' || P_NO ||' for NDC of '||PPNO);

    if (R_ID = 0) then
      open io_cursor for
        select t.com_id,
               t.old_para_id,
               t.new_para_id,
               e.name,
               t.audit_period,
               t.amount,
               a.code,
               'Un-Settled' as PARA_STATUS,
               t.ind,
               t.para_no,
               t.gist_of_paras
          from ais_t_au_post_compliance t
         inner join v_Get_PPNO_Responsibility_list r
            on r.com_id = t.com_id
         inner join t_auditee_entities e
            on e.entity_id = t.entity_id
         inner join t_audit_checklist_annexure a
            on a.id = t.annex
         where t.para_status = 8
           and r.pp_no = p_no
         order by t.audit_period;
    else
      open io_cursor for
	select t.com_id,
               t.old_para_id,
               t.new_para_id,
               e.name,
               t.audit_period,
               t.amount,
               a.code,
               'Un-Settled' as PARA_STATUS,
               t.ind,
               t.para_no,
               t.gist_of_paras
          from ais_t_au_post_compliance t
         inner join v_Get_PPNO_Responsibility_list r
            on r.com_id = t.com_id
         inner join t_auditee_entities e
            on e.entity_id = t.entity_id
         inner join t_audit_checklist_annexure a
            on a.id = t.annex
         where t.para_status = 8
           and r.pp_no =  PPNO
         order by t.audit_period;
    end if;

  end p_ppno_para;

  procedure P_GET_ALL_PARA_TEXT(CAT       in varchar2,
                                OBS_ID    in number,
                                Para_ID   in number,
                                io_cursor OUT t_cursor) is
    IND varchar2(2);

  begin

    if (CAT = 'N') then
      select 'A' into IND from dual;
    else
      select CAT into IND from dual;
    end if;

    if (IND = 'O') then
      open io_cursor for
        select ft.para_text
          from t_au_old_paras_fad_text ft
         inner join t_au_old_paras_fad f
            on ft.ref_p = f.ref_p
         where f.id = OBS_ID;

    else
      if (IND = 'A') then
        open io_cursor for
          select ot.text as para_text
            from t_au_observation_text ot
           where ot.observatsion_id = OBS_ID;

      else
        if (IND = 'C') then
          open io_cursor for
            select nt.text as para_text
              from t_au_observation_old_cad_paras_text nt
             where nt.observatsion_id = obs_id;

        else
          P_add_error_log('HD',
                          'P_audit_pre_Concluding',
                          'PP No was null',
                          OBS_ID);
          commit;
        end if;
      end if;
    end if;
  end P_GET_ALL_PARA_TEXT;

  procedure p_ppno_name(ppno      in number,
                        ENT_ID    in number,
                        P_NO      in number,
                        R_ID      in number,
                        io_cursor OUT t_cursor) is

    E_F number := 0;


  begin
  

    open io_cursor for
      select distinct f.p_no as PPNO, f.EMPLOYEE_NAME
        from V_HR_EMPLOYEE_INFO_PPNO_NDC f
       where f.p_no = ppno;
  end p_ppno_name;

  procedure P_audit_pre_Concluding(obsid     in number,
                                   gist      in varchar2,
                                   recom     in varchar2,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) as

    B_N varchar2(100);
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       181,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    update t_au_observation_text ot
       set ot.headings = gist
     where ot.observatsion_id = obsid;
    commit;
    if (P_NO is not null) then

      MERGE INTO t_au_observation_gist g
      USING (SELECT obsid AS obs_id, gist AS gist, P_NO AS entered_by
               FROM dual) src
      ON (g.obs_id = src.obs_id)
      WHEN MATCHED THEN
        UPDATE
           SET g.gist       = src.gist,
               g.entered_by = src.entered_by,
               g.entered_on = sysdate
      WHEN NOT MATCHED THEN
        INSERT
          (id, obs_id, gist, entered_by, entered_on)
        VALUES
          ((SELECT COALESCE(MAX(p.id) + 1, 1) FROM t_au_observation_gist p),
           src.obs_id,
           src.gist,
           src.entered_by,
           sysdate);

      commit;

      MERGE INTO t_au_observation_final_reccomendation t
      USING (SELECT obsid AS obs_id FROM dual) s
      ON (t.obs_id = s.obs_id)
      WHEN MATCHED THEN
        UPDATE
           SET t.recommendation = recom,
               t.entered_by     = P_NO,
               t.entered_on     = sysdate
      WHEN NOT MATCHED THEN
        INSERT
          (id, obs_id, recommendation, entered_by, entered_on)
        VALUES
          ((SELECT COALESCE(max(p.id) + 1, 1)
             FROM t_au_observation_final_reccomendation p),
           obsid,
           recom,
           P_NO,
           sysdate);
      commit;

      open io_cursor for
        select 'Gist & Recommendation added sucessfuly' as remarks
          from dual;

    else
      P_add_error_log('HD',
                      'P_audit_pre_Concluding',
                      'PP No was null',
                      obsid);
      commit;
      open io_cursor for
        select 'system error, Logout and Login again.' as remarks
          from dual;

    end if;
  end P_audit_pre_Concluding;

  procedure P_get_audit_pre_Concluding_entities(userentityid in t_Au_Plan_Eng.Eng_Id%type,
                                                io_cursor    OUT t_cursor) as
  begin

    open io_cursor for
      select e.entity_id,
             e.code,
             eg.eng_id,
             e.name || '  ( ' || eg.audit_startdate || ' to ' ||
             eg.audit_enddate || ' )' as entity_name,
             e.type_id
        from t_au_plan_eng eg
       inner join t_auditee_entities e
          on e.entity_id = eg.entity_id
         and eg.period_id > 1 ---change by ALI because period_id=2 hardcoded
         and eg.status in (12) ---- between '5' and '12'
         and eg.auditby_id = userentityid;

  end P_get_audit_pre_Concluding_entities;

  procedure P_get_audit_pre_Concluding(engid     in number,
                                       ENT_ID    in number,
                                       P_NO      in number,
                                       R_ID      in number,
                                       io_cursor OUT t_cursor) as

    E_F number := 0;
    D_F varchar2(50);
    B_N varchar2(100);
  begin
    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       181,
                       'Viewed / Finalize Draft Observations of ' || B_N);

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
       181,
       'Get Entity Observation Details in Pre Concluding Audit of  ' ||
       (select distinct t.name
          from T_AU_PLAN_DISPLAY t
         inner join t_au_plan_eng e
            on e.entity_id = t.entity_id
         where e.eng_id = engid),
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;

    open io_cursor for
      select o.id,
             ot.headings,
             rr.description as severity,
             o.final_para_no,
             os.statusname as ob_status,
             (case
               when o.id = (select gt.obs_id
                              from t_au_observation_gist gt
                             inner join t_au_observation_final_reccomendation r
                                on r.obs_id = gt.obs_id
                             where gt.obs_id = o.id) then
                'Completed'
               else
                'Pending'
             end) as status
        from t_au_observation o
       inner join t_au_observation_text ot
          on ot.observatsion_id = o.id
       inner join t_risk rr
          on rr.r_id = o.severity
       inner join t_au_observation_status os
          on os.statusid = o.status
       where o.engplanid = engid
         and o.status in (4, 8, 9);

  end P_get_audit_pre_Concluding;

  procedure P_audit_pre_submission(engid     in t_Au_Plan_Eng.Eng_Id%type,
                                   ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) as
    G_F number := 0;
    O_F number := 0;
    S_F number := 0;
    N_F number := 0;
    Z_R number := 0;
    B_N varchar2(100);
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    select NVL(MAX(l.id), 0)
      into Z_R
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_R;
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
       157,
       'Finalize Draft Audit Report FAD',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_R
           and l.ppnum = P_NO),
       'Y');
    commit;

    select count(o.id)
      into N_F
      from t_au_observation o
     where o.engplanid = engid
       and o.status in (1, 2, 3, 5, 6, 7);
    if (N_F = 0) then
      select NVL(max(e.eng_id), 0)
        into S_F
        from t_au_plan_eng e
       where e.eng_id = engid
         and e.status between 5 and 12;

      if (S_F != 0) then
        select count(g.id)
          into G_F
          from t_au_observation_gist g
         inner join t_au_observation o
            on o.id = g.obs_id
           and o.engplanid = engid
           and o.status = 8;
        select count(o.id)
          into O_F
          from t_au_observation o
         where o.engplanid = engid
           and o.status = 8;

        if (G_F = O_F) then
          update t_au_plan_eng e set e.status = 13 where e.eng_id = engid;
          commit;
          insert into t_au_plan_eng_log
            (id, e_id, status_id, createdby_id, created_on, remarks)
          VALUES
            ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
             engid,
             13,
             P_NO,
             sysdate,
             'Report Finalized');
          commit;
          open io_cursor for
            select 'Submitted to Head Audit for Report Issuance' as remarks
              from dual;
        else
          open io_cursor for
            select 'Please complete the pending Gist & Recommendation first' as remarks
              from dual;
        end if;
      else
        open io_cursor for
          select 'Please complete the pending task first' as remarks
            from dual;
      end if;
    else
      open io_cursor for
        select 'Paras/Observation needed to be Marked settled/add to final' as remarks
          from dual;
    end if;

  end P_audit_pre_submission;

  procedure P_get_audit_Concluding_entities(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor) as
    Z_R number := 0;
  begin

    select NVL(MAX(l.id), 0)
      into Z_R
      from t_au_activity_log l
     where l.ppnum = P_NO;
    update t_au_activity_log l set l.end_time = sysdate where l.id = Z_R;
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
       150,
       'Get Entities for Concluding Closing Audit',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = Z_R
           and l.ppnum = P_NO),
       'Y');
    commit;

    open io_cursor for
      select e.entity_id,
             e.code,
             eg.eng_id,
             e.name || '  ( ' || eg.audit_startdate || ' to ' ||
             eg.audit_enddate || ' )' as entity_name,
             e.type_id
        from t_au_plan_eng eg
       inner join t_auditee_entities e
          on e.entity_id = eg.entity_id
         and eg.status = 13
         and eg.auditby_id = ENT_ID;

  end P_get_audit_Concluding_entities;

  procedure P_audit_Concluding(engid  in t_Au_Plan_Eng.Eng_Id%type,
                               ENT_ID in number,
                               P_NO   in number,
                               R_ID   in number,

                               io_cursor OUT t_cursor) as
    V_F number := 0;
    C_F number := 0;
    T_Y number := 0;
    P_R number := 0;
    Z_R number := 0;
    P_C number := 0;
    r_f NUMBER := 0;
    B_N varchar2(100);

  begin

    select '-' into B_N from dual;

    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    SELECT NVL(MAX(R.ID), 0)
      INTO r_f
      frOM T_AUDIT_REPORTS R
     WHERE R.ENG_ID = engid;
    IF (R_F > 0) then

      select NVL(MAX(e.entity_type), 0)
        into T_Y
        from t_au_plan_eng e
       where e.eng_id = engid;
      select nvl(max(o.id), 0)
        into C_F
        from t_au_observation o
       where o.status in (1, 2, 3, 6)
         and o.engplanid = engid;
      select eg.status
        into V_F
        from t_au_plan_eng eg
       where eg.eng_id = engid;

      --STATUS 13 ENG WIll be allowed to Proceed for Concluding
      IF (V_F = 13) then
        if (C_F != 0) then
          open io_cursor for
            select r.ref, r.remarks from t_au_remarks r where r.id = 19;
        else

          for ee in (select o.id,
                            p.description as audit_period,
                            e.entity_id,
                            e.code,
                            e.auditby_id,
                            e.type_id,
                            (case
                              when e.type_id in (6, 28) then
                               13
                              else
                               (case
                                 when e.type_id = 22 then
                                  44
                                 else
                                  (case
                                    when e.type_id not in (6,22, 28) and
                                         et.audit_type = 'B' then
                                     21
                                    else
                                     12
                                  end)
                               end)
                            end) as c_stage,
                            o.status,
                            o.final_para_no,
                            t.headings,
                            o.entereddate,
                            o.stelled_on,
                            o.settled_by,
                            o.severity,
                            O.ANNEX

                       from t_au_observation o
                      inner join t_au_plan_eng ep
                         on ep.eng_id = o.engplanid
                      inner join t_auditee_entities e
                         on e.entity_id = ep.entity_id
                      inner join t_au_period p
                         on p.auditperiodid = ep.period_id
                      inner join t_au_observation_text t
                         on t.observatsion_id = o.id
                      inner join t_auditee_ent_types et
                         on et.autid = e.type_id
                      where o.engplanid = engid
                        and o.status in (8, 9)
                        and not exists
                      (select 'z'
                               from ais_t_au_post_compliance cc
                              where cc.new_para_id = o.id
                                and cc.ind = 'A'
                                and cc.old_para_id is null)) loop

            insert into ais_t_au_post_compliance
              (com_id,
               old_para_id,
               new_para_id,
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
               setteled_on,
               setteled_by,
               ind,
               para_added_on,
               risk,
               ANNEX)
            values
              ((select COALESCE(max(p.com_id) + 1, 1)
                 from ais_t_au_post_compliance p),
               null,
               ee.id,
               ee.audit_period,
               ee.entity_id,
               ee.code,
               ee.auditby_id,
               ee.type_id,
               0,
               ee.status,
               ee.c_stage,
               ee.status,
               cast(ee.final_para_no as varchar2(100)),
               ee.headings,
               (CASE WHEN EE.status = 9 AND ee.stelled_on IS NOT NULL THEN
                EE.STELLED_ON ELSE EE.ENTEREDDATE + 3 END),
               ee.settled_by,
               'A',
               ee.entereddate,
               ee.severity,
               ee.annex);
            commit;
          end loop;
          if (T_Y not in (6, 25, 26, 20, 21, 5, 22, 23, 17)) then
            select NVL(MAX(fd.eng_id), 0)
              into P_R
              from T_AU_OBSERVATION_MAN fd
             where fd.eng_id = engid;
            if (P_R = 0) then
              insert into T_AU_OBSERVATION_MAN
                (ID, ENTITY_ID, NEW_PARAID, PARA_STATUS, ENG_ID)
                select (select COALESCE(max(p.ID) + 1, 1)
                          from T_AU_OBSERVATION_FAD p),
                       o.entity_id,
                       o.id,
                       o.status,
                       engid
                  from t_au_observation o
                 where o.engplanid = engid
                   and o.status = '8';
            end if;
          end if;
          for j in (select *
                      from t_au_observation acl
                     where acl.engplanid = engid
                       and acl.status = 8
                       and not exists (select 'x'
                              from t_audit_para p
                             where p.obid = acl.id)
                     order by acl.memo_number) loop
            insert into t_audit_para
              (id, engplanid, obid, status, memo_number, para_no)
              select (select COALESCE(max(acc.ID) + 1, 1)
                        from t_audit_para acc),
                     o.engplanid,
                     o.id,
                     o.status,
                     o.memo_number,
                     (select COALESCE(max(ac.para_no) + 1, 1)
                        from t_audit_para ac
                       where ac.engplanid = engid)
                from t_au_observation o
               where o.engplanid = engid
                 and o.memo_number = j.memo_number;
            commit;
          end loop;
          update t_au_audit_joining j
             set j.status = 'C'
           where j.eng_plan_id = engid;
          commit;

          update T_AU_AUDIT_TEAM_TASKLIST t
             set t.status_id = 6
           where t.eng_plan_id = engid;
          commit;

          update t_au_plan_eng ep
             set ep.status = 14, ep.lastupdateddate = sysdate
           where ep.eng_id = engid;
          commit;
          pkg_sm.P_Store_Sample_data(engid, 'N', P_NO, ENT_ID, R_ID);
          open io_cursor for
            select r.ref, r.remarks from t_au_remarks r where r.id = 20;
        end if;
        insert into t_au_plan_eng_log
          (id, e_id, status_id, createdby_id, created_on, remarks)
        VALUES
          ((SELECT COALESCE(max(ll.ID) + 1, 1) FROM t_au_plan_eng_log ll),
           engid,
           14,
           '1',
           sysdate,
           'AUDIT CONCLUDED');
        commit;
      else
        open io_cursor for
          select 'Audit Already Concluded' as remarks from dual;
      end if;
    else
      open io_cursor for
        select 'Please upload the Audit Report first' as remarks from dual;
    end if;
  end P_audit_Concluding;

  procedure P_reviewed_legacy_Para(ref_id    in varchar2,
                                   ppno      in number,
                                   io_cursor OUT t_cursor) is
  begin
    if (ppno is not null) then

      update t_au_old_paras_fad o
         set o.fad_reviewed_by = ppno,
             o.update_status   = 3,
             o.fad_reviewed_on = sysdate,
             o.fad_status      = 3
       where o.ref_p = ref_id;
      commit;
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

  end P_reviewed_legacy_Para;

  procedure P_GetAuditEntitiestype(ENTITYID  IN NUMBER,
                                   io_cursor OUT t_cursor) as
  begin
    if (ENTITYID in (112242, 112248)) then
      OPEN io_cursor FOR
        SELECT et.autid as typeid, et.entitytypedesc as e_name
          FROM t_auditee_ent_types et
         where et.auditable = 'A'
           AND et.audited_by_enitity = ENTITYID;
    else
      OPEN io_cursor FOR
        SELECT et.autid as typeid, et.entitytypedesc as e_name
          FROM t_auditee_ent_types et
         inner join v_get_parent_office mp
            on mp.parent_id = et.audited_by_enitity
         where et.auditable = 'A'
           AND mp.entity_id = ENTITYID;
    end if;
  end P_GetAuditEntitiestype;

  procedure P_GetAuditEntities(typeid    in number,
                               ENTITYID  IN NUMBER,
                               io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT e.description as e_name, e.entity_id as ENTITY_ID
        FROM t_auditee_entities e
       where e.auditable = 'Y'
         AND e.auditby_id = ENTITYID
         and e.type_id = typeid;
  end P_GetAuditEntities;

  procedure P_GetAuditYear(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT p.audit_year, p.description || '  ' || p.audit_year as period
        FROM T_AU_OLD_AUDIT_PERIOD p;
  end P_GetAuditYear;

  procedure P_GetAuditnature(io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      SELECT * FROM T_AU_OLD_AUDIT_NATURE N;
  end P_GetAuditnature;

  procedure P_add_legacy_Para(typeid      in number,
                              audityear   in varchar2,
                              PARANO      in varchar2,
                              GIST        in varchar2,
                              ANEXURE     in varchar2,
                              amount      in varchar2,
                              VOL         in varchar2,
                              Entityid    in number,
                              USER_ENT_ID IN NUMBER,
                              nature      in number,
                              ppno        in number,
                              io_cursor   OUT t_cursor) is
  begin
    if (ppno is not null) then
      if (USER_ENT_ID in (112242, 112248)) then
        insert into T_AU_OBSERVATION_OLD_CAD_PARAS --(PERIOD
          (PARA_ID,
           PERIOD,
           PARA_NO,
           GIST_OF_PARAS,
           ENTITY_ID,
           AUDITED_BY,
           ENTERED_BY,
           ENTERED_ON)
        values
          ((select COALESCE(max(ac.PARA_ID) + 1, 1)
             from T_AU_OBSERVATION_OLD_CAD_PARAS ac),
           to_date(audityear, 'YYYY'),
           PARANO,
           GIST,
           Entityid,
           USER_ENT_ID,
           ppno,
           sysdate);
        commit;
        OPEN io_cursor FOR
          SELECT 'Para added in legacy list and submitted to Head for Authorization' as remarks
            from dual;
      else
        INSERT into t_au_old_paras_fad
          (id,
           ref_P,
           type_id,
           audit_period,
           para_no,
           gist_of_paras,
           annexure,
           amount_involved,
           vol_i_ii,
           audited_by,
           entity_id,
           nature_of_audit,
           entered_by,
           entered_on)
        values
          ((select COALESCE(max(acc.ID) + 1, 1) from t_au_old_paras_fad acc),
           ('B' ||
           (select COALESCE(max(acc.ID) + 1, 1) from t_au_old_paras_fad acc)),
           typeid,
           audityear,
           PARANO,
           GIST,
           ANEXURE,
           amount,
           VOL,
           USER_ENT_ID,
           Entityid,
           nature,
           ppno,
           sysdate);
        COMMIT;

        OPEN io_cursor FOR
          SELECT 'Para added in legacy list and submitted to Incharge Audit Zone for Authorization' as remarks
            from dual;
      end if;
    else
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'HD',
         'P_add_legacy_Para',
         'PP No was null',
         USER_ENT_ID,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    end if;
  end P_add_legacy_Para;

  procedure P_Get_legacy_Para_to_authorize(ENTITYID  IN NUMBER,
                                           io_cursor OUT t_cursor) as
  begin

    if (ENTITYID in (112242, 112248)) then
      open io_cursor for
        SELECT f.para_id as ref_p,
               f.period as Audit_year,
               e.code as e_code,
               e.name as e_name,
               'Regular' as nature,
               f.para_no,
               f.gist_of_paras,
               '' as annexure,
               '' as amount_involved,
               '' as vol_i_ii

          FROM T_AU_OBSERVATION_OLD_CAD_PARAS f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         where f.para_status is null
           and f.audited_by = ENTITYID;

    else
      OPEN io_cursor FOR
        SELECT f.ref_p,
               p.description || '  ' || p.audit_year as Audit_year,
               e.code as e_code,
               e.name as e_name,
               n.description as nature,
               f.para_no,
               f.gist_of_paras,
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
         where f.para_status is null
           and f.audited_by = ENTITYID;
    end if;
  end P_Get_legacy_Para_to_authorize;

  procedure P_Authorize_legacy_Para_addition(RefP      in varchar2,
                                             PPNO      IN NUMBER,
                                             io_cursor OUT t_cursor) as
  begin
    if (ppno in (113092, 111564)) then
      update T_AU_OBSERVATION_OLD_CAD_PARAS c
         set c.para_status = 8, c.status = 2
       where c.para_id = refp;
    else
      if (ppno not in (113092, 111564)) then
        /* update t_au_old_paras_fad t
          set t.para_status         = 8,
              t.parastatusupdatedby = ppno,
              t.update_status       = 1
        where t.ref_p = refp; */
        commit;
        open io_cursor for
          select 'Request for addition has been Authorized, please tell your team to add para text' as remarks
            from dual;
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
           'HD',
           'P_add_legacy_Para',
           'PP No was null',
           PPNO,
           sysdate,
           'to be checked');
        commit;
        open io_cursor for
          select 'Your session has been expired, Logout and Login again.' as remarks
            from dual;
      end if;
    end if;
  end P_Authorize_legacy_Para_addition;

  procedure P_referedback_Del_para(RefP      in varchar2,
                                   PPNO      IN NUMBER,
                                   io_cursor OUT t_cursor) as
  begin
    if (ppno is not null) then

      delete from T_AU_OBSERVATION_OLD_CAD_PARAS t where t.para_id = refp;
      commit;

      -- delete from t_au_old_paras_fad t where t.ref_p = refp;
      -- commit;

      open io_cursor for
        select 'Para Deleted' as remarks from dual;
    else
      insert into t_au_error_logs
        (id, package_name, procedure_name, nature, ppno, record_on, status)
      Values
        ((select COALESCE(max(p.id) + 1, 1) from t_au_error_logs p),
         'HD',
         'P_add_legacy_Para',
         'PP No was null',
         PPNO,
         sysdate,
         'to be checked');
      commit;
      open io_cursor for
        select 'Your session has been expired, Logout and Login again.' as remarks
          from dual;
    end if;
  end P_referedback_Del_para;

  Procedure p_update_para_no(obs_id    in number,
                             para_no   in number,
                             io_cursor OUT t_cursor) as
  begin

    update t_au_observation o
       set o.final_para_no = para_no
     where o.id = obs_id;
    commit;
    P_add_error_log('HD',
                    'P_audit_pre_Concluding',
                    'PP No was null',
                    obs_id);
    commit;
  end p_update_para_no;

  procedure P_GetSettledParasForReview(P_NO      in number,
                                       ENT_ID    in number,
                                       R_ID      in number,
                                       MON       in varchar2,
                                       Yr        in varchar2,
                                       io_cursor OUT t_cursor) as
    ENG      number := 0;
    ENT_TYPE number := 0;
    S_Date   date;
    E_Date   date;
    B_N      varchar2(100);
  begin

    select '-' into B_N from dual;

    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    SELECT '01-' || MON || '-' || Yr into S_Date from dual;
    SELECT LAST_DAY(S_DATE) into E_Date from dual;
    select max(nvl(t.entity_id, 0))
      into ENG
      from t_au_audit_team_tasklist t
     where t.teammember_ppno = P_NO
       and t.status_id between 2 and 5;
    if (ENG > 0) then
      select e.type_id
        into ENT_TYPE
        from t_auditee_entities e
       where e.entity_id = ENG;
    end if;

    if (R_ID in (41, 45, 1)) then
      OPEN io_cursor FOR
        select c.ENTITY_ID,
               c.COM_ID,
               c.NAME,
               c.COM_KEY,
               c.OLD_PARA_ID,
               c.NEW_PARAID,
               c.PARA_STATUS,
               c.IND,
               c.RSK,
               c.GIST_OF_PARAS,
               c.AUDIT_PERIOD,
               c.PARA_NO,
               c.STELLED_ON,
               c.COM_STAGE,
               c.COM_STATUS,
               c.COM_CYCLE,
               c.COM_UNIT,
               c.SETTLED_BY,
               c.REVIEWED_BY
          FROM V_GET_AIS_POST_COMPLIANCE_HO C
         where C.com_stage = 45
           and C.com_status = 16
           and trunc(c.STELLED_ON) between S_Date and E_Date
         order by C.audit_period desc, C.para_no asc;
    else
      if (R_ID in (18, 28) and ENT_TYPE != 22) then
        OPEN io_cursor FOR
          select c.ENTITY_ID,
                 c.COM_ID,
                 c.NAME,
                 c.COM_KEY,
                 c.OLD_PARA_ID,
                 c.NEW_PARAID,
                 c.PARA_STATUS,
                 c.IND,
                 c.RSK,
                 c.GIST_OF_PARAS,
                 c.AUDIT_PERIOD,
                 c.PARA_NO,
                 c.STELLED_ON,
                 c.COM_STAGE,
                 c.COM_STATUS,
                 c.COM_CYCLE,
                 c.COM_UNIT,
                 c.SETTLED_BY,
                 c.REVIEWED_BY
            FROM V_GET_AIS_POST_COMPLIANCE_HO C
           where C.com_stage = 45
             and C.com_status = 16
             and C.ENTITY_ID = ENG
             and trunc(c.STELLED_ON) between S_Date and E_Date
           order by C.audit_period desc, C.para_no asc;
      else
        if (R_ID in (18, 28) and ENT_TYPE = 22) then
          OPEN io_cursor FOR
            select c.ENTITY_ID,
                   c.COM_ID,
                   c.NAME,
                   c.COM_KEY,
                   c.OLD_PARA_ID,
                   c.NEW_PARAID,
                   c.PARA_STATUS,
                   c.IND,
                   c.RSK,
                   c.GIST_OF_PARAS,
                   c.AUDIT_PERIOD,
                   c.PARA_NO,
                   c.STELLED_ON,
                   c.COM_STAGE,
                   c.COM_STATUS,
                   c.COM_CYCLE,
                   c.COM_UNIT,
                   c.SETTLED_BY,
                   c.REVIEWED_BY
              FROM V_GET_AIS_POST_COMPLIANCE_HO C
             where C.com_stage = 45
               and C.com_status = 16
               and C.COM_UNIT_ID = ENG
               and trunc(c.STELLED_ON) between S_Date and E_Date
             order by C.audit_period desc, C.para_no asc;
        else

          OPEN io_cursor FOR
            select c.ENTITY_ID,
                   c.COM_ID,
                   c.NAME,
                   c.COM_KEY,
                   c.OLD_PARA_ID,
                   c.NEW_PARAID,
                   c.PARA_STATUS,
                   c.IND,
                   c.RSK,
                   c.GIST_OF_PARAS,
                   c.AUDIT_PERIOD,
                   c.PARA_NO,
                   c.STELLED_ON,
                   c.COM_STAGE,
                   c.COM_STATUS,
                   c.COM_CYCLE,
                   c.COM_UNIT,
                   c.SETTLED_BY,
                   c.REVIEWED_BY
              FROM V_GET_AIS_POST_COMPLIANCE_HO C
             where C.com_stage = 45
               and C.com_status = 16
               and C.auditby_id = ENT_ID
               and trunc(c.STELLED_ON) between S_Date and E_Date
             order by C.audit_period desc, C.para_no asc;
        end if;
      end if;
    end if;

  end P_GetSettledParasForReview;

  procedure P_ADD_DUPLICATE_PARAS(o_para_id in number,
                                  n_para_id in number,
                                  p_ind     in varchar2,
                                  r_remarks in varchar2,
                                  P_NO      in number,
                                  ENT_ID    in number,
                                  R_ID      in number,
                                  io_cursor OUT t_cursor) as
    B_N varchar2(100);
    cursor V is
      select f.id,
             f.entity_id,
             e.code,
             e.auditby_id,
             f.old_para_id,
             f.new_paraid,
             f.audit_period,
             f.para_status,
             f.audited_by,
             f.eng_id,
             f.r_id,
             f.Old_para_ref,
             f.IND,
             f.para_no,
             f.entereddate,
             f.stelled_on,
             f.settled_by,
             f.ANNEX,
             f.amount_involved,
             f.no_of_instances,
             f.gist_of_paras,
             f.last_updated_on,
             f.last_update_by

        from t_au_observation_fad f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       WHERE f.IND = P_IND
         and (f.old_para_id = o_para_id or f.new_paraid = n_para_id);

    vr1 V%rowtype;
    Z_B number := 0;
  begin
    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    Open V;
    Fetch V
      into vr1;
    Close v;

    insert into T_AU_DUPLICATE_PARAS
      (D_ID,
       OLD_PARA_ID,
       NEW_PARA_ID,
       ENTITY_ID,
       ENTITY_CODE,
       para_gist,
       AUDIT_PERIOD,
       AUDITED_BY,
       PARA_NO,
       PARA_STATUS,
       IND,
       RISK,
       INSTANCES,
       AMOUNT,
       ANNEX,
       REMARKS,
       AUTHORIZED_STATUS,
       ADDED_BY,
       ADDED_ON)
    values
      ((select COALESCE(max(p.d_id) + 1, 1) from T_AU_DUPLICATE_PARAS p),
       o_para_id,
       n_para_id,
       vr1.entity_id,
       vr1.code,
       vr1.gist_of_paras,
       VR1.AUDIT_PERIOD,
       vr1.auditby_id,
       vr1.para_no,
       vr1.para_status,
       vr1.ind,
       vr1.r_id,
       vr1.no_of_instances,
       vr1.amount_involved,
       vr1.annex,
       r_remarks,
       'N', --Authorization Pending
       P_NO,
       sysdate);
    commit;

    open io_cursor for
      select 'Para marked as duplicated, and forwarded for authorization' as remarks
        from dual;

  end P_ADD_DUPLICATE_PARAS;

  procedure P_GET_DUPLICATE_PARAS_ENT_FOR_AUTH(P_NO      in number,
                                               ENT_ID    in number,
                                               R_ID      in number,
                                               io_cursor OUT t_cursor) as
    B_N varchar2(100);
  begin
    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    open io_cursor for
      select e.entity_id, e.name
        from t_auditee_entities e
       where e.entity_id in (select distinct d.entity_id
                               from T_AU_DUPLICATE_PARAS d
                              where d.authorized_on is null);

  end P_GET_DUPLICATE_PARAS_ENT_FOR_AUTH;

  procedure P_GET_DUPLICATE_PARAS_FOR_AUTH(P_NO      in number,
                                           ENT_ID    in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor) as
    B_N varchar2(100);
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    open io_cursor for
      select d.d_id,
             d.old_para_id,
             d.new_para_id,
             d.entity_id,
             d.entity_code,
             e.name              as EntityName,
             d.audited_by,
             d.para_no,
             d.para_status,
             d.ind,
             d.risk,
             d.instances,
             d.amount,
             d.annex,
             d.added_by,
             d.added_on,
             d.authorized_status,
             d.authorized_by,
             d.authorized_on,
             d.remarks,
             d.para_gist         AS gist_of_paras,
             d.audit_period
        from T_AU_DUPLICATE_PARAS d
       inner join t_auditee_entities e
          on d.entity_id = e.entity_id
       where d.authorized_status = 'N';

  end P_GET_DUPLICATE_PARAS_FOR_AUTH;

  procedure P_AUTH_DUPLICATE_PARAS(DID       in number,
                                   P_NO      in number,
                                   ENT_ID    in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor) as
    B_N varchar2(100);
    cursor V is
      select d.d_id,
             d.old_para_id as O_PARA_ID,
             d.new_para_id as N_PARA_ID,
             d.entity_id   as entit_ID,
             d.entity_code,
             d.audited_by,
             d.ind

        from T_AU_DUPLICATE_PARAS d
       where d.d_id = DID;

    vr1 V%rowtype;
    Z_B number := 0;
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    Open V;
    Fetch V
      into vr1;
    Close v;

    update T_AU_DUPLICATE_PARAS d
       set d.authorized_status = 'Y',
           d.authorized_by     = P_NO,
           d.authorized_on     = sysdate
     where d.d_id = DID;
    commit;

    if (VR1.IND = 'O') then
      update t_au_old_paras_fad fd
         set fd.para_status = 28
       where fd.id = vr1.o_para_id
         and vr1.entit_id = fd.entity_id;
      commit;
    else
      if (vr1.ind = 'A') then
        update t_au_observation o
           set o.status = 28
         where o.id = vr1.n_para_id;
        commit;
      else
        if (vr1.ind = 'C') then
          update t_au_observation_old_cad_paras cad
             set cad.para_status = 28
           where cad.para_id = vr1.n_para_id;
          commit;
        end if;
      end if;
    end if;
    update ais_t_au_post_compliance ca
       set ca.para_status = 28
     where ca.entity_id = vr1.entit_id
       and (ca.old_para_id = vr1.o_para_id or
           ca.new_para_id = vr1.n_para_id);
    commit;

    open io_cursor for
      select 'Authorization of Duplicate request succesfully completed' as remarks
        from dual;

  end P_AUTH_DUPLICATE_PARAS;

  procedure P_REJECT_DUPLICATE_PARAS(DID       in number,
                                     P_NO      in number,
                                     ENT_ID    in number,
                                     R_ID      in number,
                                     io_cursor OUT t_cursor) as

    B_N varchar2(100);
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    update T_AU_DUPLICATE_PARAS d
       set d.authorized_status = 'R',
           d.authorized_by     = P_NO,
           d.authorized_on     = sysdate
     where d.d_id = DID;
    commit;
    open io_cursor for
      select 'Deletion of Duplicate para request rejected successfully' as remarks
        from dual;

  end P_REJECT_DUPLICATE_PARAS;

  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID_HO(obid      number,
                                                 P_NO      NUMBER,
                                                 R_ID      NUMBER,
                                                 ENT_ID    NUMBER,
                                                 io_cursor Out t_cursor) as

    B_N varchar2(100);

  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    open io_cursor for
      select c.id              as control_violation,
             sb.id             as nature_id,
             o.severity,
             t.headings,
             t.text,
             ae.reply,
             ar.recommendation
        from t_au_observation o
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_au_observations_auditee_response ae
          on ae.au_obs_id = o.id
       inner join t_au_observations_auditor_recommendation ar
          on ar.au_obs_id = o.id
       inner join t_control_violation_sub sb
          on sb.id = o.v_cat_nature_id
       inner join T_CONTROL_VIOLATION c
          on c.id = o.v_cat_id
       where o.id = obid;
  END P_GET_OBSERVATION_DETAILS_FROM_ID_HO;

  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID(obid      number,
                                              P_NO      NUMBER,
                                              R_ID      NUMBER,
                                              ENT_ID    NUMBER,
                                              io_cursor Out t_cursor) as

    B_N varchar2(100);

  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    P_add_error_log('HD', 'P_audit_pre_Concluding', 'PP No was null', obid);
    commit;

    open io_cursor for
      select a.id as annex_id,
             p.t_id,
             s.s_id,
             d.id as d_id,
             o.severity,
             t.headings,
             t.text,
             ae.reply,
             ar.recommendation,
             o.amount_involved,
             o.no_of_instances,
             (case
               when ds.id is null then
                'N'
               else
                'Y'
             end) as dsa
        from t_au_observation o
       inner join t_audit_checklist_annexure a
          on a.id = o.annex
       inner join t_audit_checklist_details d
          on o.checklistdetail_id = d.id
       inner join t_audit_checklist_sub s
          on s.s_id = d.s_id
       inner join t_audit_checklist p
          on p.t_id = s.t_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_au_observations_auditee_response ae
          on ae.au_obs_id = o.id
       inner join t_au_observations_auditor_recommendation ar
          on ar.au_obs_id = o.id
        left join t_au_dsa ds
          on ds.obs_id = o.id
       where o.id = obid;
  END P_GET_OBSERVATION_DETAILS_FROM_ID;

  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON(obid      number,
                                                      P_NO      NUMBER,
                                                      R_ID      NUMBER,
                                                      ENT_ID    NUMBER,
                                                      io_cursor Out t_cursor) as

    B_N varchar2(100);

  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    --P_add_error_log('HD', 'P_audit_pre_Concluding', 'PP No was null', obid);

    open io_cursor for
      select a.id              as annex_id,
             p.t_id,
             p.heading         as process,
             s.s_id,
             s.heading         as sub_process,
             d.id              as d_id,
             d.heading         as check_list_detail,
             o.severity,
             t.headings,
             o.final_para_no,
             t.text,
             ae.reply,
             ar.recommendation,
             hr.audit_reply    as head_recom,
             fr.recommendation as qa_recom,
             gg.gist           as qa_gist,
             o.amount_involved,
             o.no_of_instances
        from t_au_observation o
       inner join t_audit_checklist_annexure a
          on a.id = o.annex
       inner join t_audit_checklist_details d
          on o.checklistdetail_id = d.id
       inner join t_audit_checklist_sub s
          on s.s_id = d.s_id
       inner join t_audit_checklist p
          on p.t_id = s.t_id
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_au_observations_auditee_response ae
          on ae.au_obs_id = o.id
       inner join t_au_observations_auditor_recommendation ar
          on ar.au_obs_id = o.id
       inner join t_au_observations_auditor_reply hr
          on hr.au_obs_id = o.id
        left join t_au_observation_final_reccomendation fr
          on fr.obs_id = o.id
        left join t_au_observation_gist gg
          on gg.obs_id = o.id

       where o.id = obid;
  END P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON;

  Procedure P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON_HO(obid      number,
                                                         P_NO      NUMBER,
                                                         R_ID      NUMBER,
                                                         ENT_ID    NUMBER,
                                                         io_cursor Out t_cursor) as

    B_N varchar2(100);

  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    P_add_error_log('HD', 'P_audit_pre_Concluding', 'PP No was null', obid);

    open io_cursor for
      select 0                 as annex_id,
             o.v_cat_id        as t_id,
             o.v_cat_nature_id as s_id,
             0                 as d_id,
             o.severity,
             t.headings,
             t.text,
             ae.reply,
             ar.recommendation,
             hr.audit_reply    as head_recom,
             fr.recommendation as qa_recom,
             gg.gist           as qa_gist,
             o.amount_involved,
             o.no_of_instances
        from t_au_observation o
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_au_observations_auditee_response ae
          on ae.au_obs_id = o.id
       inner join t_au_observations_auditor_recommendation ar
          on ar.au_obs_id = o.id
       inner join t_au_observations_auditor_reply hr
          on hr.au_obs_id = o.id
        left join t_au_observation_final_reccomendation fr
          on fr.obs_id = o.id
        left join t_au_observation_gist gg
          on gg.obs_id = o.id

       where o.id = obid;
  END P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON_HO;

  PROCEDURE P_GET_PRECON_DISPLAY(i_obid    IN NUMBER,
                                 i_p_no    IN NUMBER,
                                 i_r_id    IN NUMBER,
                                 i_ent_id  IN NUMBER,
                                 io_cursor OUT t_cursor) AS
  BEGIN
    -- Better activity log message
    P_add_activity_log(i_ent_id,
                       i_r_id,
                       i_p_no,
                       79,
                       'Viewed Pre-Concluding details for Observation ID ' ||
                       i_obid);
  
    OPEN io_cursor FOR

      SELECT a.id              AS annex_id,
             p.t_id,
             p.heading         AS process,
             s.s_id,
             s.heading         AS sub_process,
             d.id              AS d_id,
             d.heading         AS check_list_detail,
             o.severity,
             r.description     AS severity_text, -- add this
             t.headings,
             o.final_para_no,
             t.text,
             ae.reply,
             ar.recommendation,
             hr.audit_reply    AS head_recom,
             fr.recommendation AS qa_recom,
             gg.gist           AS qa_gist,
             o.amount_involved,
             o.no_of_instances
        FROM t_au_observation o
        JOIN t_audit_checklist_annexure a
          ON a.id = o.annex
        JOIN t_audit_checklist_details d
          ON d.id = o.checklistdetail_id
        JOIN t_audit_checklist_sub s
          ON s.s_id = d.s_id
        JOIN t_audit_checklist p
          ON p.t_id = s.t_id
        JOIN t_risk r
          ON r.r_id = a.risk
         JOIN t_au_observation_text t
          ON t.observatsion_id = o.id
          join t_au_observations_auditee_response ae      
          ON ae.au_obs_id = o.id

         JOIN t_au_observations_auditor_recommendation  ar
          ON ar.au_obs_id = o.id
         
         JOIN t_au_observations_auditor_reply  hr
          ON hr.au_obs_id = o.id
         
         JOIN t_au_observation_final_reccomendation  fr
          ON fr.obs_id = o.id
         
        LEFT JOIN t_au_observation_gist  gg
          ON gg.obs_id = o.id
         
       WHERE o.id = i_obid;
  END P_GET_PRECON_DISPLAY;

  procedure P_audit_para_update_svz_az(OBID         in number,
                                       ANXID        in number,
                                       PROCID       in number,
                                       SUB_PROCID   in number,
                                       PROC_DETID   in number,
                                       RISKID       in number,
                                       FINAL_PARA   in number,
                                       PARA_GIST    in varchar2,
                                       TEXT_OF_PARA in clob,
                                       AMOUNT_INV   in number,
                                       NO_INST      in number,
                                       P_NO         in number,
                                       ENT_ID       in number,
                                       R_ID         in number,
                                       io_cursor    OUT t_cursor) as
    B_N varchar2(100);
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    if (SUB_PROCID = 0) then
      open io_cursor for
        select 'Select Sub Process to proceed' as remarks from dual;
    end if;

    if (PROC_DETID = 0) then
      open io_cursor for
        select 'Select Checklist Detail to proceed' as remarks from dual;
    end if;
    if (PROC_DETID != 0 and SUB_PROCID != 0) then
      update t_au_observation o
         set o.severity           = RISKID,
             o.checklistdetail_id = PROC_DETID,
             o.subchecklist_id    = SUB_PROCID,
             o.annex              = ANXID,
             o.amount_involved    = AMOUNT_INV,
             o.no_of_instances    = NO_INST,
             o.final_para_no = FINAL_PARA
       where o.id = OBID;
      commit;

      update t_au_observation_text t
         set t.text = TEXT_OF_PARA, t.headings = PARA_GIST
       where t.observatsion_id = OBID;
      commit;
      P_add_error_log('HD',
                      'P_audit_pre_Concluding',
                      'PP No was null',
                      OBID);
      commit;

      open io_cursor for
        select 'Paras details have been updated successfully' as remarks
          from dual;

    end if;

  end P_audit_para_update_svz_az;

  procedure P_audit_para_update_head_dept(OBID         in number,
                                          V_ID         in number,
                                          V_NATUREID   in number,
                                          RISKID       in number,
                                          PARA_GIST    in varchar2,
                                          TEXT_OF_PARA in clob,
                                          P_NO         in number,
                                          ENT_ID       in number,
                                          R_ID         in number,
                                          io_cursor    OUT t_cursor) as
    B_N varchar2(100);
  begin

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    update t_au_observation o
       set o.severity        = RISKID,
           o.v_cat_id        = V_ID,
           o.v_cat_nature_id = V_NATUREID

     where o.id = OBID;
    commit;

    update t_au_observation_text t
       set t.text = TEXT_OF_PARA, t.headings = PARA_GIST
     where t.observatsion_id = OBID;
    commit;

    P_add_error_log('HD', 'P_audit_pre_Concluding', 'PP No was null', OBID);
    commit;

    open io_cursor for
      select 'Paras details have been updated successfully' as remarks
        from dual;

  end P_audit_para_update_head_dept;
  Procedure P_UPLOAD_AUDIT_REPORT(ENGID     number,
                                  AREP      clob,
                                  REP_TYPE  varchar2,
                                  REP_NAME  varchar2,
                                  P_NO      number,
                                  R_ID      number,
                                  ENT_ID    number,
                                  io_cursor out t_cursor) as
    v_exists NUMBER := 0;
    d_clob   number := 0;
    error1   varchar2(2);
    B_N      varchar2(100);
  BEGIN

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    SELECT DBMS_LOB. GETLENGTH(arep) into D_clob FROM dual;
    if (D_CLOB > 1) then
      SELECT COUNT(*)
        INTO v_exists
        FROM t_audit_reports
       WHERE eng_id = ENGID;

      IF v_exists > 0 THEN
        UPDATE t_audit_reports
           SET audit_report = AREP,
               DOC_TYPE     = REP_TYPE,
               DOC_NAME     = REP_NAME,
               added_on     = CURRENT_TIMESTAMP,
               added_by     = P_NO
         WHERE eng_id = ENGID;

        COMMIT;

        -- Return a success message for the update
        OPEN IO_CURSOR FOR
          SELECT 'Audit report updated successfully for eng_id: ' || ENGID AS remarks,
                 'Y' as error1
            FROM DUAL;

      ELSE
        INSERT INTO t_audit_reports
          (eng_id, audit_report, DOC_TYPE, DOC_NAME, added_by, added_on)
        VALUES
          (ENGID, AREP, REP_TYPE, REP_NAME, P_NO, CURRENT_TIMESTAMP);

        COMMIT;

        OPEN IO_CURSOR FOR
          SELECT 'Audit report inserted successfully  eng_id: ' || ENGID AS remarks,
                 'Y' as error1
            FROM DUAL;

      END IF;
    else
      OPEN IO_CURSOR FOR
        SELECT 'You havent uploaded any report, Please upload the Report FIrst then press upload button' AS remarks,
               'N' as error1
          FROM DUAL;
    end if;

  end P_UPLOAD_AUDIT_REPORT;

  Procedure P_GET_FINAL_AUDIT_REPORT(ENGID     number,
                                     P_NO      number,
                                     R_ID      number,
                                     ENT_ID    number,
                                     io_cursor out t_cursor) as
    B_N varchar2(100);
  BEGIN

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    OPEN IO_CURSOR FOR

      select t.id,
             t.eng_id,
             p.description as audit_period,
             et.name       as ENTITY_NAME,
             e.entity_id
        from t_audit_reports t
       inner join t_au_plan_eng e
          on e.eng_id = t.eng_id
       inner join t_au_period p
          on p.auditperiodid = e.period_id
       inner join t_auditee_entities et
          on et.entity_id = e.entity_id
       where e.auditby_id = ENT_ID;

  end P_GET_FINAL_AUDIT_REPORT;

  Procedure P_GET_AUDIT_REPORT_CONTENT(FILE_ID   number,
                                       P_NO      number,
                                       R_ID      number,
                                       ENT_ID    number,
                                       io_cursor out t_cursor) as
    B_N varchar2(100);
  BEGIN

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    OPEN IO_CURSOR FOR

      select t.id, t.audit_report as FILE_DATA, t.doc_type, t.doc_name

        from t_audit_reports t
       where t.id = FILE_ID;

  end P_GET_AUDIT_REPORT_CONTENT;

  Procedure P_GET_CHECK_AUDIT_REPORT_UPLOADED(ENGID     number,
                                              P_NO      number,
                                              R_ID      number,
                                              ENT_ID    number,
                                              io_cursor out t_cursor) as

    B_N varchar2(100);

  BEGIN

    select '-' into B_N from dual;
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Viewed / Finalize Draft Observations of ' || B_N);

    OPEN IO_CURSOR FOR

      SELECT NVL(MIN(t.id), 0) AS id,
             NVL(MIN(t.doc_type), '') as doc_type,
             NVL(MIN(t.doc_name), '') as doc_name

        from t_audit_reports t
       where t.eng_id = ENGID;

  end P_GET_CHECK_AUDIT_REPORT_UPLOADED;

  procedure P_Get_Paras_For_Status_Change(ENT_ID    in number,
                                          R_ID      in number,
                                          entityId  in number,
                                          io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      SELECT c.com_id,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             r.description as risk,
             c.ind,
             (Case
               when c.para_status = 8 then
                'Un-Settled'
               else
                (case
                  when c.para_status in (9, 6) then
                   'Settled'
                end)
             end) as para_status
        FROM ais_t_au_post_compliance c
       inner join t_risk r
          on r.r_id = c.risk
       WHERE c.entity_id = entityId
         and not exists (select 'z'
                from T_AU_PARAS_STATUS_CHANGE_LOG lg
               where lg.com_id = c.com_id
                 and lg.status = 'P')
       order by c.com_id;
  end P_Get_Paras_For_Status_Change;

  PROCEDURE P_Add_Paras_For_Status_Change(C_ID       IN NUMBER,
                                          NewStatus  IN NUMBER,
                                          Remarks    IN VARCHAR2,
                                          IND        IN VARCHAR2,
                                          Action_IND IN VARCHAR2,
                                          ENT_ID     IN NUMBER,
                                          P_NO       IN NUMBER,
                                          R_ID       IN NUMBER,
                                          io_cursor  OUT t_cursor) AS
    M_F      NUMBER := 0;
    N_P      NUMBER := 0;
    O_P      NUMBER := 0;
    v_exists NUMBER := 0;
  BEGIN
    BEGIN
      SELECT c.new_para_id, c.old_para_id, c.para_status
        INTO N_P, O_P, M_F
        FROM ais_t_au_post_compliance c
       WHERE c.com_id = C_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        OPEN io_cursor FOR
          SELECT 'No record found for the specified Para (COM_ID).' AS remark
            FROM dual;
        RETURN;
      WHEN TOO_MANY_ROWS THEN
        OPEN io_cursor FOR
          SELECT 'Multiple records found for this COM_ID. Please check data integrity.' AS remark
            FROM dual;
        RETURN;
    END;

    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Para Submitted for Status Change for ' || C_ID);

    IF (M_F = NewStatus) THEN
      OPEN io_cursor FOR
        SELECT 'Same status Para cannot be submitted' AS remark FROM dual;
    ELSE
      -- Duplicate check
      SELECT COUNT(1)
        INTO v_exists
        FROM T_AU_PARAS_STATUS_CHANGE_LOG
       WHERE COM_ID = C_ID
         AND NEW_STATUS = NewStatus
         AND STATUS = 'P'; -- Only check pending requests

      IF v_exists > 0 THEN
        OPEN io_cursor FOR
          SELECT 'Duplicate request already exists for this Para and status.' AS remark
            FROM dual;
        RETURN;
      END IF;

      INSERT INTO T_AU_PARAS_STATUS_CHANGE_LOG
        (COM_ID,
         NEW_PARA_ID,
         OLD_PARA_ID,
         OLD_STATUS,
         IND,
         NEW_STATUS,
         CREATED_BY,
         CREATED_DATE,
         REMARKS,
         ACTION,
         STATUS)
      VALUES
        (C_ID,
         N_P,
         O_P,
         M_F,
         IND,
         NewStatus,
         P_NO,
         SYSDATE,
         Remarks,
         Action_IND,
         'P');
      COMMIT;

      P_add_error_log('HD',
                      'P_audit_pre_Concluding',
                      'PP No was null',
                      C_ID);

      OPEN io_cursor FOR
        SELECT 'Request for updation in Para Status submitted for Authorization' AS remark
          FROM dual;
    END IF;

  EXCEPTION
    WHEN OTHERS THEN
      OPEN io_cursor FOR
        SELECT 'An unexpected error occurred: ' AS remark FROM dual;
  END P_Add_Paras_For_Status_Change;

  Procedure P_Get_Paras_For_Status_Change_For_Authorize(ENT_ID    in number,
                                                        io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      SELECT c.com_id,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             r.description as risk,
             c.ind,
             (Case
               when c.para_status = 8 then
                'Un-Settled'
               else
                (case
                  when c.para_status in (9, 6) then
                   'Settled'
                end)
             end) as old_para_status,
             (Case
               when lg.new_status = 8 then
                'Un-Settled'
               else
                (case
                  when lg.new_status in (9, 6) then
                   'Settled'
                end)
             end) as new_para_status
        FROM ais_t_au_post_compliance c
       inner join t_risk r
          on r.r_id = c.risk
       inner join T_AU_PARAS_STATUS_CHANGE_LOG lg
          on lg.com_id = c.com_id
         and lg.status = 'P'
       WHERE c.audited_by = ENT_ID
       order by c.com_id;
  end P_Get_Paras_For_Status_Change_For_Authorize;

  PROCEDURE P_Authorize_Paras_For_Status(C_ID       IN NUMBER,
                                         N_PARA_ID  IN NUMBER,
                                         O_PARA_ID  IN NUMBER,
                                         remark     IN VARCHAR2,
                                         IND        IN VARCHAR2,
                                         Action_IND IN VARCHAR2,
                                         ENT_ID     IN NUMBER,
                                         P_NO       IN NUMBER,
                                         R_ID       IN NUMBER,
                                         io_cursor  OUT t_cursor) AS
    S_F NUMBER := 0;
    S_T VARCHAR2(100);
    P_F VARCHAR2(50);
  BEGIN
    -- Log the activity
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Authorize of Status Change of com_id ' || C_ID ||
                       ' as ' || Action_IND);

    IF Action_IND = 'A' THEN
      -- "A" should mean Authorized; adjust if otherwise
      BEGIN
        SELECT l.new_status
          INTO S_F
          FROM T_AU_PARAS_STATUS_CHANGE_LOG l
         WHERE l.com_id = C_ID
           AND l.status = 'P';
      EXCEPTION
        WHEN NO_DATA_FOUND THEN
          OPEN io_cursor FOR
            SELECT 'No pending status found in change log for this para.' AS remarks
              FROM dual;
          RETURN;
      END;

      -- Update PARA STATUS for all relevant tables
      IF S_F != 8 THEN
        -- 8 = "Settled" status (confirm your mapping)
        IF IND = 'A' THEN
          UPDATE T_AU_OBSERVATION o
             SET o.status     = S_F,
                 o.stelled_on = SYSDATE,
                 o.settled_by = P_NO
           WHERE id = N_PARA_ID;
        ELSIF IND = 'O' THEN
          UPDATE t_au_old_paras_fad
             SET para_status    = S_F,
                 settled_by     = P_NO,
                 parasetteledon = SYSDATE
           WHERE id = O_PARA_ID;
        ELSIF IND = 'C' THEN
          UPDATE t_au_observation_old_cad_paras ca
             SET para_status    = S_F,
                 ca.setteled_by = P_NO,
                 ca.setteled_on = SYSDATE
           WHERE para_id = N_PARA_ID;
        END IF;

        UPDATE ais_t_au_post_compliance c
           SET c.para_status = S_F,
               c.setteled_on = SYSDATE,
               c.setteled_by = P_NO
         WHERE com_id = C_ID;

      ELSE
        -- Unsettled
        IF IND = 'A' THEN
          UPDATE T_AU_OBSERVATION
             SET status = S_F, stelled_on = NULL, settled_by = NULL
           WHERE id = N_PARA_ID;
        ELSIF IND = 'O' THEN
          UPDATE t_au_old_paras_fad
             SET para_status    = S_F,
                 settled_by     = NULL,
                 parasetteledon = NULL
           WHERE id = O_PARA_ID;
        ELSIF IND = 'C' THEN
          UPDATE t_au_observation_old_cad_paras
             SET para_status = S_F, setteled_by = NULL, setteled_on = NULL
           WHERE para_id = N_PARA_ID;
        END IF;

        UPDATE ais_t_au_post_compliance
           SET para_status = S_F, setteled_on = NULL, setteled_by = NULL
         WHERE com_id = C_ID;
      END IF;

      -- Output the status
      IF S_F = 8 THEN
        S_T := 'Un-Settled';
      ELSE
        S_T := 'Settled';
      END IF;

      -- Para identifier for message
      select ca.para_no
        into P_F
        from ais_t_au_post_compliance ca
       where ca.com_id = C_ID;
      -- Update change log (always runs if no RETURN above)
      UPDATE T_AU_PARAS_STATUS_CHANGE_LOG l
         SET l.authorized_on       = SYSDATE,
             l.authorized_by       = P_NO,
             l.authorized_comments = remark,
             l.status              = Action_IND
       WHERE l.com_id = C_ID
         AND l.status = 'P';
      COMMIT;

      OPEN io_cursor FOR
        SELECT 'Para number ' || P_F || ' has been marked as ' || S_T AS Remark
          FROM dual;
      RETURN;
    ELSE
      -- Update change log (always runs if no RETURN above)
      UPDATE T_AU_PARAS_STATUS_CHANGE_LOG l
         SET l.authorized_on       = SYSDATE,
             l.authorized_by       = P_NO,
             l.authorized_comments = remark,
             l.status              = Action_IND
       WHERE l.com_id = C_ID
         AND l.status = 'P';
      COMMIT;
      -- Action is 'A': Authorization Denied/Rejected (confirm this logic for your system)
      OPEN io_cursor FOR
        SELECT 'Para Rejected' AS Remark FROM dual;
      RETURN;
    END IF;

  EXCEPTION
    WHEN OTHERS THEN
      OPEN io_cursor FOR
        SELECT 'Error occurred: ' AS remark FROM dual;
  END P_Authorize_Paras_For_Status;

end PKG_HD;

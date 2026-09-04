create or replace package PKG_DB is
  TYPE t_cursor IS REF CURSOR;

  procedure P_Getrealtionshiptype(UserRoleid IN NUMBER,
                                  ENT_ID     in number,
                                  P_NO       in number,
                                  io_cursor  OUT t_cursor);

  procedure P_Getparentrepoffice(rid in number, io_cursor OUT t_cursor);

  procedure P_Getchildposting(erid in number,
                              
                              io_cursor OUT t_cursor);

  procedure P_GET_Dash_table(UserEntityID IN NUMBER,
                             io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_new(UserEntityID IN NUMBER,
                                 io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_old(UserEntityID IN NUMBER,
                                 io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_v_wise(process_id IN NUMBER,
                                    sub_id     in number,
                                    d_id       in number,
                                    io_cursor  OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_names(ENT_ID in number,
                                                P_NO   in number,
                                                R_ID   in number,
                                                
                                                io_cursor OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_names_checklist(ENT_ID    in number,
                                                          P_NO      in number,
                                                          R_ID      in number,
                                                          E_ID      IN NUMBER,
                                                          io_cursor OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_names_checklist_sub(UserEntityID IN NUMBER,
                                                              PROCESSID    IN NUMBER,
                                                              ENT_ID       in number,
                                                              P_NO         in number,
                                                              R_ID         in number,
                                                              io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_functionwise(ENT_ID    in number,
                                          P_NO      in number,
                                          R_ID      in number,
                                          E_ID      IN NUMBER,
                                          io_cursor OUT t_cursor);

  Procedure P_GET_Dash_table_functionwise_PARA_summary(A_ID      in number,
                                                       R_ID      in number,
                                                       ENT_ID    in number,
                                                       io_cursor OUT t_cursor);

  Procedure P_GET_Dash_table_functionwise_PARA(A_ID      in number,
                                               R_ID      in number,
                                               ENT_ID    in number,
                                               io_cursor OUT t_cursor);

  Procedure P_GET_Dash_table_functionwise_PARA_TEXT(P_ID      in number,
                                                    P_C       in varchar2,
                                                    io_cursor OUT t_cursor);

  procedure p_get_risk_baseplan(io_cursor OUT t_cursor);

  PROCEDURE P_GET_AUDIT_PERFORMANCE(io_cursor OUT t_cursor);

  procedure P_GetFunctionalResponsibilityWisePara(ENTITYID         in number,
                                                  PROCESSID        IN NUMBER,
                                                  SUB_PROCESSID    IN NUMBER,
                                                  PROCESS_DETAILID IN NUMBER,
                                                  io_cursor        OUT t_cursor);
  -- not in use
  procedure P_DashboardDivisionalHeadfad(entityid  in number,
                                         io_cursor OUT t_cursor);

  -- not in use
  procedure P_DashboardDivisionalHeadfadDetail(entityid  in number,
                                               io_cursor OUT t_cursor);
  -- not in use
  Procedure p_getglheadsummary_dash(PPNumber  in number,
                                    io_cursor OUT t_cursor);
  -- not in use
  Procedure p_getglheadsummary_dash_Yearly(PPNumber  in number,
                                           io_cursor OUT t_cursor);

  procedure p_get_dash_repetitive(P_ID         in number,
                                  PS_ID        IN NUMBER,
                                  D_ID         in number,
                                  UserEntityID IN NUMBER,
                                  io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_names_HO(auditedby in number,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_names_checklist_HO(UserEntityID IN NUMBER,
                                                             ENT_ID       in number,
                                                             P_NO         in number,
                                                             R_ID         in number,
                                                             io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_names_checklist_sub_HO(UserEntityID IN NUMBER,
                                                                 PROCESSID    IN NUMBER,
                                                                 io_cursor    OUT t_cursor);

  procedure P_GET_Dash_table_functionwise_HO(UserEntityID  IN NUMBER,
                                             PROCESSID     IN NUMBER,
                                             SUB_PROCESSID IN NUMBER,
                                             AUDITEDBY     in number,
                                             io_cursor     OUT t_cursor);

  procedure p_get_dashborad_scorecard(io_cursor OUT t_cursor);

  Procedure P_Functional_ANALYSIS_DETAILS(R_ID       in number,
                                          ENT_ID     in number,
                                          P_REF_DATE in date,
                                          io_cursor  OUT t_cursor);

  Procedure P_Functional_ENTITY_WISE_ANALYSIS(R_ID      in number,
                                              ENT_ID    in number,
                                              io_cursor OUT t_cursor);

  Procedure P_Functional_ENTITY_WISE_Paras(R_ID      in number,
                                           ENT_ID    in number,
                                           P_NO      in number,
                                           io_cursor OUT t_cursor);

  Procedure P_Functional_Reporting_office_WISE_ANALYSIS(R_ID      in number,
                                                        ENT_ID    in number,
                                                        io_cursor OUT t_cursor);

  Procedure P_Function_Annexure(E_ID      in number,
                                R_ID      in number,
                                io_cursor OUT t_cursor);

  Procedure P_Function_Annexure_Paras(A_ID      in number,
                                      R_ID      in number,
                                      ENT_ID    in number,
                                      io_cursor OUT t_cursor);

  Procedure P_Function_Annexure_Paras_text(P_ID      in number,
                                           P_C       in varchar2,
                                           io_cursor OUT t_cursor);

  procedure P_compliance_summary(P_NO      in number,
                                 R_ID      in number,
                                 ENT_ID    in number,
                                 ENTITY    IN NUMBER,
                                 io_cursor OUT t_cursor);

end PKG_DB;
/
create or replace package body PKG_DB is

  procedure P_Getrealtionshiptype(UserRoleid IN NUMBER,
                                  ENT_ID     in number,
                                  P_NO       in number,
                                  io_cursor  OUT t_cursor) is
  
  begin
    if (UserRoleid = 34) then
      open io_cursor for
        select f.entity_realtion_id,
               f.parent_name || '   TO   ' || f.chlid_name as field_name
          from t_auditee_ent_relation f
         where f.status = 'Y'
           and f.id is not null
           and f.id in (4)
         order by f.id;
    else
      if (UserRoleid = 35) then
        open io_cursor for
          select f.entity_realtion_id,
                 f.parent_name || '   TO   ' || f.chlid_name as field_name
            from t_auditee_ent_relation f
           where f.status = 'Y'
             and f.id is not null
             and f.id in (1, 2)
           order by f.id;
      else
        if (UserRoleid in (1, 3, 5, 6, 7,11)) then
          open io_cursor for
            select f.entity_realtion_id,
                   f.parent_name || '   TO   ' || f.chlid_name as field_name
              from t_auditee_ent_relation f
             where f.status = 'Y'
               and f.id is not null
            --and f.id in (1,2)
             order by f.id;
        else
          open io_cursor for
            select distinct f.entity_realtion_id,
                            f.parent_name || '   TO   ' || f.chlid_name as field_name
              from t_auditee_ent_relation f
             inner join t_auditee_entities_maping e
                on e.relation_type_id = f.entity_realtion_id
             inner join T_AUDITEE_ENTITIES_MAPING_function d
                on d.c_type_id = e.c_type_id
             where f.status = 'Y'
               and d.parent_id = ENT_ID;
        
        end if;
      end if;
    end if;
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

  procedure P_Getchildposting(erid in number,
                              
                              io_cursor OUT t_cursor) is
  begin
  
    open io_cursor for
      select distinct (r.entity_id), r.c_name, r.c_name, e.status
        from t_auditee_ent_relation    e,
             t_auditee_ent_types       t,
             T_AUDITEE_ENTITIES_MAPING r,
             t_auditee_entities        et
       where t.autid = r.relation_type_id
         and r.p_type_id = e.parent_entity_typeid
         and r.c_type_id = e.child_entity_typeid
         and et.entity_id = r.entity_id
         and r.parent_id = erid
      --and et.auditby_id = USER_ENTITY_ID
       order by r.c_name;
  end P_Getchildposting;

  procedure P_GET_Dash_table(UserEntityID IN NUMBER,
                             io_cursor    OUT t_cursor) as
  begin
    if (UserEntityID <> 0) then
      OPEN io_cursor FOR
        select an.heading as Process,
               count(f.com_id) as Total_Paras,
               sum(case
                     when f.para_status != '8' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when f.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when f.para_status != '8' then
                            1
                           else
                            0
                         end) / count(f.com_id) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when f.risk = '1' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when f.risk = '2' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when f.risk = '3' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from ais_t_au_post_compliance f
         inner join t_audit_checklist_annexure an
            on f.ANNEX = an.id
         inner join t_auditee_entities e
            on e.entity_id = f.ENTITY_ID
         inner join t_auditee_entities_maping m
            on m.entity_id = e.entity_id
         where m.entity_id = UserEntityID
         group by an.heading
         order by an.heading;
    else
      OPEN io_cursor FOR
        select an.heading as Process,
               count(f.com_id) as Total_Paras,
               sum(case
                     when f.para_status != '8' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when f.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when f.para_status != '8' then
                            1
                           else
                            0
                         end) / count(f.com_id) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when f.risk = '1' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when f.risk = '2' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when f.risk = '3' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from ais_t_au_post_compliance f
         inner join t_audit_checklist_annexure an
            on f.ANNEX = an.id
         inner join t_auditee_entities e
            on e.entity_id = f.ENTITY_ID
         inner join t_auditee_entities_maping m
            on m.entity_id = e.entity_id
         group by an.heading
         order by an.heading;
    end if;
  end P_GET_Dash_table;

  procedure P_GET_Dash_table_new(UserEntityID IN NUMBER,
                                 io_cursor    OUT t_cursor) as
  begin
    if (UserEntityID <> 0) then
      OPEN io_cursor FOR
        select an.heading as Process,
               count(f.com_id) as Total_Paras,
               sum(case
                     when f.para_status != '8' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when f.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when f.para_status != '8' then
                            1
                           else
                            0
                         end) / count(f.com_id) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when f.risk = '1' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when f.risk = '2' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when f.risk = '3' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from ais_t_au_post_compliance f
         inner join t_audit_checklist_annexure an
            on f.ANNEX = an.id
           and f.old_para_id is null
         inner join t_auditee_entities e
            on e.entity_id = f.ENTITY_ID
         inner join t_auditee_entities_maping m
            on m.entity_id = e.entity_id
         where m.entity_id = UserEntityID
         group by an.heading
         order by an.heading;
    else
      OPEN io_cursor FOR
        select an.heading as Process,
               count(f.com_id) as Total_Paras,
               sum(case
                     when f.para_status != '8' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when f.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when f.para_status != '8' then
                            1
                           else
                            0
                         end) / count(f.com_id) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when f.risk = '1' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when f.risk = '2' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when f.risk = '3' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from ais_t_au_post_compliance f
         inner join t_audit_checklist_annexure an
            on f.ANNEX = an.id
           and f.old_para_id is null
         inner join t_auditee_entities e
            on e.entity_id = f.ENTITY_ID
         inner join t_auditee_entities_maping m
            on m.entity_id = e.entity_id
         group by an.heading
         order by an.heading;
    end if;
  end P_GET_Dash_table_new;

  procedure P_GET_Dash_table_old(UserEntityID IN NUMBER,
                                 io_cursor    OUT t_cursor) as
  begin
    if (UserEntityID <> 0) then
      OPEN io_cursor FOR
        select an.heading as Process,
               count(f.com_id) as Total_Paras,
               sum(case
                     when f.para_status != '8' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when f.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when f.para_status != '8' then
                            1
                           else
                            0
                         end) / count(f.com_id) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when f.risk = '1' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when f.risk = '2' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when f.risk = '3' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from ais_t_au_post_compliance f
         inner join t_audit_checklist_annexure an
            on f.ANNEX = an.id
           and f.old_para_id is not null
         inner join t_auditee_entities e
            on e.entity_id = f.ENTITY_ID
         inner join t_auditee_entities_maping m
            on m.entity_id = e.entity_id
         where m.entity_id = UserEntityID
         group by an.heading
         order by an.heading;
    else
      OPEN io_cursor FOR
        select an.heading as Process,
               count(f.com_id) as Total_Paras,
               sum(case
                     when f.para_status != '8' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when f.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when f.para_status != '8' then
                            1
                           else
                            0
                         end) / count(f.com_id) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when f.risk = '1' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when f.risk = '2' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when f.risk = '3' and f.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from ais_t_au_post_compliance f
         inner join t_audit_checklist_annexure an
            on f.ANNEX = an.id
           and f.old_para_id is not null
         inner join t_auditee_entities e
            on e.entity_id = f.ENTITY_ID
         inner join t_auditee_entities_maping m
            on m.entity_id = e.entity_id
         group by an.heading
         order by an.heading;
    end if;
  end P_GET_Dash_table_old;

  procedure P_GET_Dash_table_v_wise(process_id IN NUMBER,
                                    sub_id     in number,
                                    d_id       in number,
                                    io_cursor  OUT t_cursor) as
  begin
    if (process_id = 0) then
      OPEN io_cursor FOR
        select sb.heading as Process,
               count(o.ID) as Total_Paras,
               sum(case
                     when o.para_status = '6' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when o.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when o.para_status = '6' then
                            1
                           else
                            0
                         end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when cd.risk_id = '1' and o.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when cd.risk_id = '2' and o.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when cd.risk_id = '3' and o.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
          from V_REPORT_DASHBORAD_DETAIL o
         inner join t_audit_checklist_details cd
            on cd.id = o.PROCESS_DETAIL
         inner join t_audit_checklist_sub sb
            on cd.s_id = sb.s_id
         inner join t_audit_checklist ck
            on ck.t_id = sb.t_id
         inner join t_auditee_entities e
            on e.entity_id = cd.owner_enitity_id
        
         group by sb.heading
         order by sb.heading;
    else
      if (process_id != 0 and sub_id = 0) then
        OPEN io_cursor FOR
          select sb.heading as Process,
                 count(o.ID) as Total_Paras,
                 sum(case
                       when o.para_status = '6' then
                        1
                       else
                        0
                     end) as Setteled_para,
                 sum(case
                       when o.para_status = '8' then
                        1
                       else
                        0
                     end) as UnSetteled_para,
                 cast((sum(case
                             when o.para_status = '6' then
                              1
                             else
                              0
                           end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
                 sum(case
                       when cd.risk_id = '1' and o.para_status = '8' then
                        1
                       else
                        0
                     end) as R1,
                 sum(case
                       when cd.risk_id = '2' and o.para_status = '8' then
                        1
                       else
                        0
                     end) as R2,
                 sum(case
                       when cd.risk_id = '3' and o.para_status = '8' then
                        1
                       else
                        0
                     end) as R3
            from V_REPORT_DASHBORAD_DETAIL o
           inner join t_audit_checklist_details cd
              on cd.id = o.PROCESS_DETAIL
           inner join t_audit_checklist_sub sb
              on cd.s_id = sb.s_id
           inner join t_audit_checklist ck
              on ck.t_id = sb.t_id
           inner join t_auditee_entities e
              on e.entity_id = cd.owner_enitity_id
           where ck.t_id = process_id
           group by sb.heading
           order by sb.heading;
      else
      
        if (process_id != 0 and sub_id != 0) then
          OPEN io_cursor FOR
            select cd.heading as Process,
                   count(o.ID) as Total_Paras,
                   sum(case
                         when o.para_status = '6' then
                          1
                         else
                          0
                       end) as Setteled_para,
                   sum(case
                         when o.para_status = '8' then
                          1
                         else
                          0
                       end) as UnSetteled_para,
                   cast((sum(case
                               when o.para_status = '6' then
                                1
                               else
                                0
                             end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
                   sum(case
                         when cd.risk_id = '1' and o.para_status = '8' then
                          1
                         else
                          0
                       end) as R1,
                   sum(case
                         when cd.risk_id = '2' and o.para_status = '8' then
                          1
                         else
                          0
                       end) as R2,
                   sum(case
                         when cd.risk_id = '3' and o.para_status = '8' then
                          1
                         else
                          0
                       end) as R3
              from V_REPORT_DASHBORAD_DETAIL o
             inner join t_audit_checklist_details cd
                on cd.id = o.PROCESS_DETAIL
             inner join t_audit_checklist_sub sb
                on cd.s_id = sb.s_id
             inner join t_audit_checklist ck
                on ck.t_id = sb.t_id
             inner join t_auditee_entities e
                on e.entity_id = cd.owner_enitity_id
             inner join t_auditee_entities_maping mp
                on mp.entity_id = o.ENTITY_ID
             where ck.t_id = process_id
               and sb.s_id = sub_id
             group by cd.heading
             order by cd.heading;
        
        end if;
      end if;
    end if;
  
  end P_GET_Dash_table_v_wise;

  procedure P_GET_Dash_table_functionwise_names(ENT_ID    in number,
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
       125,
       'Get Functional List For Dashboard',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    open io_cursor for
      select e.description as Functional_owner, e.entity_id
        from V_REPORT_DASHBORAD_DETAIL o
       inner join t_audit_checklist_details cd
          on cd.id = o.PROCESS_DETAIL
       inner join t_audit_checklist_sub sb
          on cd.s_id = sb.s_id
       inner join t_audit_checklist ck
          on ck.t_id = sb.t_id
       inner join t_auditee_entities e
          on e.entity_id = cd.owner_enitity_id
       group by e.description, e.entity_id
       order by e.description;
  
  end P_GET_Dash_table_functionwise_names;

  procedure P_GET_Dash_table_functionwise_names_checklist(ENT_ID    in number,
                                                          P_NO      in number,
                                                          R_ID      in number,
                                                          E_ID      IN NUMBER,
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
       199,
       'Get Violation List For Dashboard',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    open io_cursor for
      select ck.heading, ck.t_id
        from V_REPORT_DASHBORAD_DETAIL o
       inner join t_audit_checklist_details cd
          on cd.id = o.PROCESS_DETAIL
       inner join t_audit_checklist_sub sb
          on cd.s_id = sb.s_id
       inner join t_audit_checklist ck
          on ck.t_id = sb.t_id
       inner join t_auditee_entities e
          on e.entity_id = cd.owner_enitity_id
       where e.entity_id = E_ID
       group by ck.heading, ck.t_id
       order by ck.heading, ck.t_id;
  
  end P_GET_Dash_table_functionwise_names_checklist;

  procedure P_GET_Dash_table_functionwise_names_checklist_sub(UserEntityID IN NUMBER,
                                                              PROCESSID    IN NUMBER,
                                                              ENT_ID       in number,
                                                              P_NO         in number,
                                                              R_ID         in number,
                                                              io_cursor    OUT t_cursor) as
  
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
       199,
       'Get Violation List For Dashboard with Process',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    open io_cursor for
      select SB.S_ID, SB.HEADING
        from V_REPORT_DASHBORAD_DETAIL o
       inner join t_audit_checklist_details cd
          on cd.id = o.PROCESS_DETAIL
       inner join t_audit_checklist_sub sb
          on cd.s_id = sb.s_id
       inner join t_audit_checklist ck
          on ck.t_id = sb.t_id
       inner join t_auditee_entities e
          on e.entity_id = cd.owner_enitity_id
       where e.entity_id = UserEntityID
         AND CK.T_ID = PROCESSID
       group by SB.S_ID, SB.HEADING
       order by SB.S_ID, SB.HEADING;
  
  end P_GET_Dash_table_functionwise_names_checklist_sub;

  procedure P_GET_Dash_table_functionwise(ENT_ID    in number,
                                          P_NO      in number,
                                          R_ID      in number,
                                          E_ID      IN NUMBER,
                                          io_cursor OUT t_cursor) as
  begin
  
    OPEN io_cursor FOR
      select o.id as d_id,
             m.p_name,
             m.c_name,
             a.code as Annex,
             cd.heading as chlist,
             o.final_para_no,
             r.description as Risk,
             t.headings as Gist,
             '' as ref_p,
             'A' as pc
        from t_au_observation o
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_audit_checklist_annexure a
          on a.id = o.annex
       inner join t_audit_checklist_details cd
          on cd.id = o.checklistdetail_id
       inner join t_auditee_entities pr
          on pr.entity_id = cd.owner_enitity_id
       inner join t_auditee_entities_maping m
          on o.entity_id = m.entity_id
       inner join t_risk r
          on r.r_id = o.severity
       INNER JOIN T_AU_PLAN_ENG ep
          on EP.ENG_ID = O.ENGPLANID
         and to_char(ep.lastupdateddate, 'MM') = to_char(sysdate, 'MM') - 1
         and ep.status = 14
       INNER JOIN T_AU_PERIOD P
          ON P.AUDITPERIODID = EP.PERIOD_ID
         AND P.STATUS_ID = 2
       where pr.entity_id = ENT_ID
         and o.status = 8
       order by m.p_name, m.c_name;
  
  end P_GET_Dash_table_functionwise;

  Procedure P_GET_Dash_table_functionwise_PARA_summary(A_ID      in number,
                                                       R_ID      in number,
                                                       ENT_ID    in number,
                                                       io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select m.p_name, o.audit_period, count(o.id) as para_no
        from T_AUDIT_CHECKLIST_ANNEXURE e
       inner join t_audit_checklist_details d
          on d.annex = e.id
       inner join v_cia_analysis o
          on o.annex = e.id
       inner join t_auditee_entities et
          on et.entity_id = o.entity_id
       inner join t_auditee_entities_maping m
          on m.entity_id = et.entity_id
       where d.id = A_ID
      --and d.owner_enitity_id = ENT_ID
       group by m.p_name, o.audit_period;
  END P_GET_Dash_table_functionwise_PARA_summary;

  Procedure P_GET_Dash_table_functionwise_PARA(A_ID      in number,
                                               R_ID      in number,
                                               ENT_ID    in number,
                                               io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select et.name,
             o.audit_period,
             o.para_no,
             o.ind as para_category,
             case
               when o.old_para_id is not null then
                o.old_para_id
               else
                o.new_para_id
             end as id
        from T_AUDIT_CHECKLIST_ANNEXURE e
       inner join ais_t_au_post_compliance o
          on o.annex = e.id
       inner join t_auditee_entities et
          on et.entity_id = o.entity_id
       where e.id = A_ID
      --and d.owner_enitity_id = ENT_ID
      ;
  END P_GET_Dash_table_functionwise_PARA;

  Procedure P_GET_Dash_table_functionwise_PARA_TEXT(P_ID      in number,
                                                    P_C       in varchar2,
                                                    io_cursor OUT t_cursor) is
  
  begin
    if (P_C = 'O') then
      open io_cursor for
        select f.gist_of_paras as headings, t.para_text
          from t_au_old_paras_fad f
         inner join t_au_old_paras_fad_text t
            on t.ref_p = f.ref_p
         where f.id = P_ID;
    else
      if (P_C = 'A') then
        open io_cursor for
          select t.headings, t.text as para_text
            from t_au_observation f
           inner join t_au_observation_text t
              on f.id = t.observatsion_id
           where f.id = P_ID;
      end if;
    end if;
  
  END P_GET_Dash_table_functionwise_PARA_TEXT;

  procedure p_get_risk_baseplan(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select r.name,
             r.R_ID,
             r.entitytypedesc,
             r.risk,
             r.entity_size,
             r.NO_OF_Enitites,
             r.plans,
             r.eng
        from V_R_RISKBASE_PLANNING R
       ORDER BY R.R_ID;
  
  end p_get_risk_baseplan;

  procedure p_get_risk_baseplan_total(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select * from V_R_RISKBASE_PLANNING;
  
  end p_get_risk_baseplan_total;

  PROCEDURE P_GET_AUDIT_PERFORMANCE(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select T.ID,
             t.Title,
             sum(t.Total_Paras) as total_paras,
             sum(t.Settled_para) as Setteled_para,
             sum(t.Unsettled_para) UnSetteled_para,
             cast((sum(t.Settled_para) / sum(t.Total_Paras) * 100) as
                  decimal(10, 2)) || ' %' as Ratio,
             sum(t.R1) as r1,
             sum(t.R2) as r2,
             sum(t.R3) as r3
        from v_audit_performance t
      
       group by T.ID, t.Title
       order by T.ID;
  
  end P_GET_AUDIT_PERFORMANCE;

  procedure P_GetFunctionalResponsibilityWisePara(ENTITYID         in number,
                                                  PROCESSID        IN NUMBER,
                                                  SUB_PROCESSID    IN NUMBER,
                                                  PROCESS_DETAILID IN NUMBER,
                                                  io_cursor        OUT t_cursor) is
  
  begin
    IF (PROCESSID != 0) THEN
      open io_cursor for
        select *
          FROM V_FUNCTIONAL_RESPONSIBLITY BD
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

  procedure p_get_dash_repetitive(P_ID         in number,
                                  PS_ID        IN NUMBER,
                                  D_ID         in number,
                                  UserEntityID IN NUMBER,
                                  io_cursor    OUT t_cursor) as
  begin
    IF (P_ID = 0 AND PS_ID = 0) THEN
      open io_cursor for
        select * from V_R_REPEATED_VOILATIONS t;
    ELSE
      IF (P_ID != 0 AND PS_ID = 0) THEN
        open io_cursor for
          select * from V_R_REPEATED_VOILATIONS t where T.t_id = P_ID;
      ELSE
        IF (P_ID != 0 AND PS_ID != 0) THEN
          open io_cursor for
            select *
              from V_R_REPEATED_VOILATIONS t
             where T.t_id = P_ID
               and t.s_id = PS_ID;
        END IF;
      END IF;
    END IF;
  
  end p_get_dash_repetitive;

  procedure P_get_dash_table_functionwise_names_ho(auditedby in number,
                                                   ENT_ID    in number,
                                                   P_NO      in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) as
  
  begin
  
    open io_cursor for
      select Upper(e.name) as Functional_owner, e.entity_id
        from V_get_dash_table_functionwise_names_ho o
       inner join t_coso_rating_factors cd
          on cd.id = o.v_cat_id
       inner join t_control_violation_sub sb
          on o.V_CAT_NATURE_ID = sb.id
       inner join t_risk ck
          on ck.r_id = sb.risk_id
       inner join t_auditee_entities e
          on e.entity_id = o.entity_id
       WHERE O.AUDITED_BY = AUDITEDBY
       group by e.name, e.entity_id
       order by e.name;
  
  end P_get_dash_table_functionwise_names_ho;

  procedure P_GET_Dash_table_functionwise_names_checklist_HO(UserEntityID IN NUMBER,
                                                             ENT_ID       in number,
                                                             P_NO         in number,
                                                             R_ID         in number,
                                                             io_cursor    OUT t_cursor) as
  
    E_F number := 0;
  begin
  
    open io_cursor for
      select cd.rating_factors as heading, cd.id as t_id
        from V_get_dash_table_functionwise_names_ho o
       inner join t_coso_rating_factors cd
          on cd.id = o.v_cat_id
       inner join t_control_violation_sub sb
          on o.V_CAT_NATURE_ID = sb.id
       inner join t_risk ck
          on ck.r_id = sb.risk_id
       inner join t_auditee_entities e
          on e.entity_id = o.entity_id
       where e.entity_id = UserEntityID
       group by cd.rating_factors, cd.id
       order by cd.rating_factors, cd.id;
  
  end P_GET_Dash_table_functionwise_names_checklist_HO;

  procedure P_GET_Dash_table_functionwise_names_checklist_sub_HO(UserEntityID IN NUMBER,
                                                                 PROCESSID    IN NUMBER,
                                                                 io_cursor    OUT t_cursor) as
  
  begin
  
    open io_cursor for
      select distinct sb.sub_v_name as heading, sb.id as s_id
        from V_get_dash_table_functionwise_names_ho o
       inner join t_coso_rating_factors cd
          on cd.id = o.v_cat_id
       inner join t_control_violation_sub sb
          on o.V_CAT_NATURE_ID = sb.id
       inner join t_risk ck
          on ck.r_id = sb.risk_id
       inner join t_auditee_entities e
          on e.entity_id = o.entity_id
       where e.entity_id = UserEntityID
         and cd.id = PROCESSID
       group by sb.sub_v_name, sb.id
       order by sb.sub_v_name, sb.id;
  
  end P_GET_Dash_table_functionwise_names_checklist_sub_HO;

  procedure P_GET_Dash_table_functionwise_HO(UserEntityID  IN NUMBER,
                                             PROCESSID     IN NUMBER,
                                             SUB_PROCESSID IN NUMBER,
                                             AUDITEDBY     in number,
                                             io_cursor     OUT t_cursor) as
  begin
    if (UserEntityID = 0 or UserEntityID is null) then
      OPEN io_cursor FOR
        select Upper(e.name) as Functional_owner,
               count(o.entity_id) as Total_Paras,
               sum(case
                     when o.para_status = '9' then
                      1
                     else
                      0
                   end) as Setteled_para,
               sum(case
                     when o.para_status = '8' then
                      1
                     else
                      0
                   end) as UnSetteled_para,
               cast((sum(case
                           when o.para_status = '9' then
                            1
                           else
                            0
                         end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
               sum(case
                     when sb.risk_id = '1' and o.para_status = '8' then
                      1
                     else
                      0
                   end) as R1,
               sum(case
                     when sb.risk_id = '2' and o.para_status = '8' then
                      1
                     else
                      0
                   end) as R2,
               sum(case
                     when sb.risk_id = '3' and o.para_status = '8' then
                      1
                     else
                      0
                   end) as R3
        
          from V_get_dash_table_functionwise_names_ho o
         inner join t_coso_rating_factors cd
            on cd.id = o.v_cat_id
         inner join t_control_violation_sub sb
            on o.V_CAT_NATURE_ID = sb.id
         inner join t_risk ck
            on ck.r_id = sb.risk_id
         inner join t_auditee_entities e
            on e.entity_id = o.entity_id
         where o.AUDITED_BY = AUDITEDBY
           and o.para_status in (8, 9)
         group by e.name
         order by e.name;
    else
      if (UserEntityID != 0 and PROCESSID = 0) then
        OPEN io_cursor FOR
          select cd.rating_factors as Functional_owner,
                 count(o.ID) as Total_Paras,
                 sum(case
                       when o.para_status = '9' then
                        1
                       else
                        0
                     end) as Setteled_para,
                 sum(case
                       when o.para_status = '8' then
                        1
                       else
                        0
                     end) as UnSetteled_para,
                 cast((sum(case
                             when o.para_status = '9' then
                              1
                             else
                              0
                           end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
                 sum(case
                       when sb.risk_id = '1' and o.para_status = '8' then
                        1
                       else
                        0
                     end) as R1,
                 sum(case
                       when sb.risk_id = '2' and o.para_status = '8' then
                        1
                       else
                        0
                     end) as R2,
                 sum(case
                       when sb.risk_id = '3' and o.para_status = '8' then
                        1
                       else
                        0
                     end) as R3
            from V_get_dash_table_functionwise_names_ho o
           inner join t_coso_rating_factors cd
              on cd.id = o.v_cat_id
           inner join t_control_violation_sub sb
              on o.V_CAT_NATURE_ID = sb.id
           inner join t_risk ck
              on ck.r_id = sb.risk_id
           inner join t_auditee_entities e
              on e.entity_id = o.entity_id
           where e.entity_id = UserEntityID
             and o.AUDITED_BY = AUDITEDBY
             and o.para_status in (8, 9)
           group by cd.rating_factors
           order by cd.rating_factors;
      else
        if (SUB_PROCESSID != 0) then
          OPEN io_cursor FOR
            select sb.sub_v_name as Functional_owner,
                   count(o.ID) as Total_Paras,
                   sum(case
                         when o.para_status = '9' then
                          1
                         else
                          0
                       end) as Setteled_para,
                   sum(case
                         when o.para_status = '8' then
                          1
                         else
                          0
                       end) as UnSetteled_para,
                   cast((sum(case
                               when o.para_status = '9' then
                                1
                               else
                                0
                             end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
                   sum(case
                         when sb.risk_id = '1' and o.para_status = '8' then
                          1
                         else
                          0
                       end) as R1,
                   sum(case
                         when sb.risk_id = '2' and o.para_status = '8' then
                          1
                         else
                          0
                       end) as R2,
                   sum(case
                         when sb.risk_id = '3' and o.para_status = '8' then
                          1
                         else
                          0
                       end) as R3
              from V_get_dash_table_functionwise_names_ho o
             inner join t_coso_rating_factors cd
                on cd.id = o.v_cat_id
             inner join t_control_violation_sub sb
                on o.V_CAT_NATURE_ID = sb.id
             inner join t_risk ck
                on ck.r_id = sb.risk_id
             inner join t_auditee_entities e
                on e.entity_id = o.entity_id
             where sb.id = SUB_PROCESSID
               and e.entity_id = UserEntityID
               and o.AUDITED_BY = AUDITEDBY
               and o.para_status in (8, 9)
             group by sb.sub_v_name
             order by sb.sub_v_name;
        else
          if (PROCESSID != 0) then
            OPEN io_cursor FOR
              select o.gist_of_paras as Functional_owner,
                     count(o.ID) as Total_Paras,
                     sum(case
                           when o.para_status = '9' then
                            1
                           else
                            0
                         end) as Setteled_para,
                     sum(case
                           when o.para_status = '8' then
                            1
                           else
                            0
                         end) as UnSetteled_para,
                     cast((sum(case
                                 when o.para_status = '9' then
                                  1
                                 else
                                  0
                               end) / count(o.ID) * 100) as decimal(10, 2)) || ' %' as Ratio,
                     sum(case
                           when sb.risk_id = '1' and o.para_status = '8' then
                            1
                           else
                            0
                         end) as R1,
                     sum(case
                           when sb.risk_id = '2' and o.para_status = '8' then
                            1
                           else
                            0
                         end) as R2,
                     sum(case
                           when sb.risk_id = '3' and o.para_status = '8' then
                            1
                           else
                            0
                         end) as R3
                from V_get_dash_table_functionwise_names_ho o
               inner join t_coso_rating_factors cd
                  on cd.id = o.v_cat_id
               inner join t_control_violation_sub sb
                  on o.V_CAT_NATURE_ID = sb.id
               inner join t_risk ck
                  on ck.r_id = sb.risk_id
               inner join t_auditee_entities e
                  on e.entity_id = o.entity_id
               where sb.v_id = PROCESSID
                 and e.entity_id = UserEntityID
                 and o.AUDITED_BY = AUDITEDBY
                 and o.para_status in (8, 9)
               group by o.gist_of_paras
               order by o.gist_of_paras;
          
          end if;
        end if;
      
      end if;
    
    end if;
  
  end P_GET_Dash_table_functionwise_HO;

  procedure p_get_dashborad_scorecard(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select r.department,
             r.heading,
             r.no_of_ent,
             r.tot_ent,
             r.pencent,
             r.remarks
        from t_dashboard_scorecard R;
  
  end p_get_dashborad_scorecard;

PROCEDURE P_Functional_ANALYSIS_DETAILS(
    R_ID       IN NUMBER,
    ENT_ID     IN NUMBER,
    P_REF_DATE IN DATE,
    io_cursor  OUT t_cursor
) IS

    V_REF_DATE    DATE := TRUNC(P_REF_DATE);
    V_YEAR_START  DATE := TRUNC(P_REF_DATE, 'YYYY');

BEGIN

    IF R_ID IN (1, 2, 3, 5, 7, 11, 41) THEN

        OPEN io_cursor FOR
            SELECT 
                   a.id,
                   a.e_heading AS heading,               
                   a.code || '  ' || a.a_heading AS annex,
                   a.audit_comments,

                   COUNT(CASE
                           WHEN TRUNC(a.para_added_on) <= V_REF_DATE
                            AND (a.setteled_on IS NULL OR TRUNC(a.setteled_on) > V_REF_DATE)
                           THEN 1
                         END) AS total,               

                   NVL(SUM(CASE
                             WHEN TRUNC(a.para_added_on) < V_YEAR_START
                              AND (a.setteled_on IS NULL OR TRUNC(a.setteled_on) > V_REF_DATE)
                             THEN 1
                           END), 0) AS old_total,

                   NVL(SUM(CASE
                             WHEN TRUNC(a.para_added_on) BETWEEN V_YEAR_START AND V_REF_DATE
                              AND (a.setteled_on IS NULL OR TRUNC(a.setteled_on) > V_REF_DATE)
                             THEN 1
                           END), 0) AS new_total,

                   NVL(SUM(CASE
                             WHEN a.risk = 1
                              AND TRUNC(a.para_added_on) <= V_REF_DATE
                              AND (a.setteled_on IS NULL OR TRUNC(a.setteled_on) > V_REF_DATE)
                             THEN 1
                           END), 0) AS R1,

                   NVL(SUM(CASE
                             WHEN a.risk = 2
                              AND TRUNC(a.para_added_on) <= V_REF_DATE
                              AND (a.setteled_on IS NULL OR TRUNC(a.setteled_on) > V_REF_DATE)
                             THEN 1
                           END), 0) AS R2,

                   NVL(SUM(CASE
                             WHEN a.risk = 3
                              AND TRUNC(a.para_added_on) <= V_REF_DATE
                              AND (a.setteled_on IS NULL OR TRUNC(a.setteled_on) > V_REF_DATE)
                             THEN 1
                           END), 0) AS R3

            FROM VW_AU_POST_COMPLIANCE_FULL a

            WHERE (
                      R_ID NOT IN (2, 6, 7)
                   OR a.audited_by = ENT_ID
                  )

            GROUP BY 
                   a.id, 
                   a.e_heading, 
                   a.code, 
                   a.a_heading, 
                   a.audit_comments

            ORDER BY a.id;

    
    else
      if (R_id in (39)) then
        open io_cursor for
          select A.id,
                 e.heading,
                 a.code || '  ' || a.heading as annex,
                 e.audit_comments,
                 count(o.com_id) as total,
                 SUM(CASE
                       WHEN o.para_added_on <
                            TO_DATE('01-01-' || TO_CHAR(P_REF_DATE, 'YYYY'),
                                    'DD-MM-YYYY') THEN
                        1
                       ELSE
                        0
                     END) AS old_total,
                 SUM(CASE
                       WHEN o.para_added_on >=
                            TO_DATE('01-01-' || TO_CHAR(P_REF_DATE, 'YYYY'),
                                    'DD-MM-YYYY') THEN
                        1
                       ELSE
                        0
                     END) AS new_total,
                 sum(case
                       when a.RISK = '1' then
                        1
                       else
                        0
                     end) as R1,
                 sum(case
                       when a.RISK = '2' then
                        1
                       else
                        0
                     end) as R2,
                 sum(case
                       when a.RISK = '3' then
                        1
                       else
                        0
                     end) as R3
            from T_AUDIT_CHECKLIST_ANNEXURE_PROCESS e
           inner join T_AUDIT_CHECKLIST_ANNEXURE_MAPPING m
              on e.id = m.process_id
           inner join T_AUDIT_CHECKLIST_ANNEXURE a
              on a.code = m.annex
           inner join ais_t_au_post_compliance o
              on o.annex = a.id
             AND O.PARA_STATUS = 8
           inner join t_auditee_entities_maping gm
              on o.entity_id = gm.entity_id
          
           where gm.gm_office = ENT_ID
           group by A.id, e.heading, a.code, a.heading, e.audit_comments
           order by A.id;
      else
        if (R_id in (40) AND ENT_ID != 112277) then
          open io_cursor for
            select A.id,
                   e.heading,
                   a.code || '  ' || a.heading as annex,
                   e.audit_comments,
                   count(o.com_id) as total,
                   SUM(CASE
                         WHEN o.para_added_on <
                              TO_DATE('01-01-' || TO_CHAR(P_REF_DATE, 'YYYY'),
                                      'DD-MM-YYYY') THEN
                          1
                         ELSE
                          0
                       END) AS old_total,
                   SUM(CASE
                         WHEN o.para_added_on >=
                              TO_DATE('01-01-' || TO_CHAR(P_REF_DATE, 'YYYY'),
                                      'DD-MM-YYYY') THEN
                          1
                         ELSE
                          0
                       END) AS new_total,
                   sum(case
                         when a.RISK = '1' then
                          1
                         else
                          0
                       end) as R1,
                   sum(case
                         when a.RISK = '2' then
                          1
                         else
                          0
                       end) as R2,
                   sum(case
                         when a.RISK = '3' then
                          1
                         else
                          0
                       end) as R3
              from T_AUDIT_CHECKLIST_ANNEXURE_PROCESS e
             inner join T_AUDIT_CHECKLIST_ANNEXURE_MAPPING m
                on e.id = m.process_id
             inner join T_AUDIT_CHECKLIST_ANNEXURE a
                on a.code = m.annex
             inner join ais_t_au_post_compliance o
                on o.annex = a.id
               AND O.PARA_STATUS = 8
             group by A.id, e.heading, a.code, a.heading, e.audit_comments
             order by A.id;
        end if;
      
      end if;
    end if;
  END P_Functional_ANALYSIS_DETAILS;

  Procedure P_Functional_ENTITY_WISE_ANALYSIS(R_ID      in number,
                                              ENT_ID    in number,
                                              io_cursor OUT t_cursor) is
  
  begin
  
    if (R_id in (1, 2, 3, 5, 7, 38)) then
      open io_cursor for
        select distinct s.entity_id,
                        s.parent_id,
                        s.reporting_office,
                        s.name,
                        s.total,
                        s.old_total,
                        s.new_total,
                        s.R1,
                        s.R2,
                        s.R3
          from v_p_functional_entity_wise_analysis s;
    elsif (R_id in (6,9)) then
        open io_cursor for
          select distinct s.entity_id,
                          s.parent_id,
                          s.reporting_office,
                          s.name,
                          s.total,
                          s.old_total,
                          s.new_total,
                          s.R1,
                          s.R2,
                          s.R3
            from v_p_functional_entity_wise_analysis_man s
           inner join t_auditee_entities e
              on s.entity_id = e.entity_id
           where e.auditby_id = ENT_ID
           order by s.reporting_office;
      elsif (R_id in (4)) then
          open io_cursor for
            select distinct s.entity_id,
                            s.parent_id,
                            s.reporting_office,
                            s.name,
                            s.total,
                            s.old_total,
                            s.new_total,
                            s.R1,
                            s.R2,
                            s.R3
              from v_p_functional_entity_wise_analysis s
             where s.relation_type_id in (22, 5, 20);
        elsif (R_id in (39)) then
            open io_cursor for
              select distinct s.entity_id,
                              s.parent_id,
                              s.reporting_office,
                              s.name,
                              s.total,
                              s.old_total,
                              s.new_total,
                              s.R1,
                              s.R2,
                              s.R3
                from v_p_functional_entity_wise_analysis s
               inner join v_GM_mapping m
                  on (s.entity_id = m.br_id or s.entity_id = m.Region_id or
                     s.entity_id = m.GM_ID)
               where s.relation_type_id in (5, 23, 20)
                 and m.GM_ID = ENT_ID
               order by s.parent_id desc;
          elsif (R_id in (14, 21)) then
              open io_cursor for
                select distinct s.entity_id,
                                s.parent_id,
                                s.reporting_office,
                                s.name,
                                s.total,
                                s.old_total,
                                s.new_total,
                                s.R1,
                                s.R2,
                                s.R3
                  from v_p_functional_entity_wise_analysis s
                 inner join T_AUDITEE_ENTITIES_MAPING m
                    on M.ENTITY_ID = S.entity_id
                 where M.PARENT_ID = ENT_ID
                 order by s.parent_id desc;
            elsif (R_id in (40) and ent_id = 112247) then
                open io_cursor for
                  select distinct s.entity_id,
                                  s.parent_id,
                                  s.reporting_office,
                                  s.name,
                                  s.total,
                                  s.old_total,
                                  s.new_total,
                                  s.R1,
                                  s.R2,
                                  s.R3
                    from v_p_functional_entity_wise_analysis_ops s
                   where s.c_type_id = 5
                   order by s.parent_id desc;
              elsif (R_id in (40) and ent_id != 112247) then
                  open io_cursor for
                    select distinct s.entity_id,
                                    s.parent_id,
                                    s.reporting_office,
                                    s.name,
                                    s.total,
                                    s.old_total,
                                    s.new_total,
                                    s.R1,
                                    s.R2,
                                    s.R3
                      from v_p_functional_entity_wise_analysis s
                     where s.c_type_id = case
                             when ENT_ID = 112262 then
                              25
                             WHEN ENT_ID = 112277 then
                              28
                             WHEN ENT_ID = 112261 THEN
                               s.c_type_id
                           end
                     order by s.total desc;
                end if;
              
  END P_Functional_ENTITY_WISE_ANALYSIS;

  Procedure P_Functional_ENTITY_WISE_Paras(R_ID      in number,
                                           ENT_ID    in number,
                                           P_NO      in number,
                                           io_cursor OUT t_cursor) is
    E_ID number := 0;
  begin
 select u.entity_id into E_ID from t_user u where u.ppno = P_NO;
    if (R_id in (40) and E_ID in (112247,112261)) then
      open io_cursor for
        select distinct o.entity_id,
                        o.name,
                        o.audit_period,
                        o.ref_p,
                        o.au_obs_id,
                        o.para_no,
                        o.gist_of_paras,
                        o.risk,
                        o.para_category,
                        o.com_id
          from v_dash_borad_para_deatils o
         inner join t_auditee_entities_maping m
            on (m.parent_id = o.entity_id or m.entity_id = o.entity_id)
         where m.parent_id = ENT_ID;
    elsIF (R_ID in (14)) then
        open io_cursor for
          select distinct o.entity_id,
                          o.name,
                          o.audit_period,
                          o.ref_p,
                          o.au_obs_id,
                          o.para_no,
                          o.gist_of_paras,
                          o.risk,
                          o.para_category,
                          o.com_id
            from v_dash_borad_para_deatils o
           inner join t_auditee_entities_maping m
              on (m.parent_id = o.entity_id or m.entity_id = o.entity_id)
           where m.parent_id = ENT_ID;
      elsif (E_ID in (112242,112248)) then
        open io_cursor for
          select distinct o.entity_id,
                          o.name,
                          o.audit_period,
                          o.ref_p,
                          o.au_obs_id,
                          o.para_no,
                          o.gist_of_paras,
                          o.risk,
                          o.para_category,
                          o.com_id
            from v_dash_borad_para_deatils_man o
           inner join t_auditee_entities_maping e
              on e.entity_id = o.entity_id
           where o.entity_id = ENT_ID;
       else
        open io_cursor for
          select distinct o.entity_id,
                          o.name,
                          o.audit_period,
                          o.ref_p,
                          o.au_obs_id,
                          o.para_no,
                          o.gist_of_paras,
                          o.risk,
                          o.para_category,
                          o.com_id
            from v_dash_borad_para_deatils o
           inner join t_auditee_entities_maping e
              on e.entity_id = o.entity_id
           where e.entity_id = ENT_ID;
      end if;
  END P_Functional_ENTITY_WISE_Paras;

  Procedure P_Functional_Reporting_office_WISE_ANALYSIS(R_ID      in number,
                                                        ENT_ID    in number,
                                                        io_cursor OUT t_cursor) is
  
  begin
    if (R_id in (1, 2, 3, 5, 7)) then
      open io_cursor for
        SELECT m.parent_id,
               m.p_name AS name,
               COUNT(f.com_id) AS total,
               SUM(CASE
                     WHEN f.para_added_on <
                          TO_DATE('01-01-' || TO_CHAR(SYSDATE, 'YYYY'),
                                  'DD-MM-YYYY') THEN
                      1
                     ELSE
                      0
                   END) AS old_total,
               SUM(CASE
                     WHEN f.para_added_on >=
                          TO_DATE('01-01-' || TO_CHAR(SYSDATE, 'YYYY'),
                                  'DD-MM-YYYY') THEN
                      1
                     ELSE
                      0
                   END) AS new_total,
               SUM(CASE
                     WHEN a.risk = '1' THEN
                      1
                     ELSE
                      0
                   END) AS R1,
               SUM(CASE
                     WHEN a.risk = '2' THEN
                      1
                     ELSE
                      0
                   END) AS R2,
               SUM(CASE
                     WHEN a.risk = '3' THEN
                      1
                     ELSE
                      0
                   END) AS R3
          FROM ais_t_au_post_compliance f
         INNER JOIN T_AUDIT_CHECKLIST_ANNEXURE a
            ON a.id = f.annex
         INNER JOIN t_auditee_entities_maping m
            ON (m.entity_id = a.co_function_1 or m.entity_id = a.co_function_2 or m.entity_id = a.function)
         GROUP BY m.parent_id, m.p_name
         ORDER BY m.parent_id, m.p_name;
    else
      open io_cursor for
        SELECT m.parent_id,
               m.p_name AS name,
               COUNT(f.com_id) AS total,
               SUM(CASE
                     WHEN f.para_added_on <
                          TO_DATE('01-01-' || TO_CHAR(SYSDATE, 'YYYY'),
                                  'DD-MM-YYYY') THEN
                      1
                     ELSE
                      0
                   END) AS old_total,
               SUM(CASE
                     WHEN f.para_added_on >=
                          TO_DATE('01-01-' || TO_CHAR(SYSDATE, 'YYYY'),
                                  'DD-MM-YYYY') THEN
                      1
                     ELSE
                      0
                   END) AS new_total,
               SUM(CASE
                     WHEN a.risk = '1' THEN
                      1
                     ELSE
                      0
                   END) AS R1,
               SUM(CASE
                     WHEN a.risk = '2' THEN
                      1
                     ELSE
                      0
                   END) AS R2,
               SUM(CASE
                     WHEN a.risk = '3' THEN
                      1
                     ELSE
                      0
                   END) AS R3
          FROM ais_t_au_post_compliance f
         INNER JOIN T_AUDIT_CHECKLIST_ANNEXURE a
            ON a.id = f.annex
         INNER JOIN t_auditee_entities_maping m
            ON m.entity_id = f.entity_id
        
         where m.parent_id = ENT_ID
         GROUP BY m.parent_id, m.p_name
         ORDER BY m.parent_id, m.p_name;
    
    end if;
  
  END P_Functional_Reporting_office_WISE_ANALYSIS;

  Procedure P_Function_Annexure(E_ID      in number,
                                R_ID      in number,
                                io_cursor OUT t_cursor) is
  
  begin
  
    open io_cursor for
      select Distinct E.ID, E.HEADING
        from T_AUDIT_CHECKLIST_ANNEXURE e
       inner join v_cia_analysis s
          on s.annex = e.id
       inner join v_GM_mapping m
          on (s.entity_id = m.br_id or s.entity_id = m.Region_id)
       where m.GM_ID = E_ID;
  END P_Function_Annexure;

  Procedure P_Function_Annexure_Paras(A_ID      in number,
                                      R_ID      in number,
                                      ENT_ID    in number,
                                      io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select et.name,
             o.audit_period,
             o.para_no,
             o.para_category,
             e.id,
             '' as ref_p,
             0 as au_obs_id
        from T_AUDIT_CHECKLIST_ANNEXURE e
      
       inner join v_cia_analysis o
          on o.annex = e.id
       inner join t_auditee_entities et
          on et.entity_id = o.entity_id
       where e.id = A_ID
         and et.entity_id = ENT_ID
       ORDER BY O.audit_period DESC;
  END P_Function_Annexure_Paras;

  Procedure P_Function_Annexure_Paras_text(P_ID      in number,
                                           P_C       in varchar2,
                                           io_cursor OUT t_cursor) is
  
  begin
    if (P_C = 'O') then
      open io_cursor for
        select f.gist_of_paras as headings, t.para_text
          from t_au_old_paras_fad f
         inner join t_au_old_paras_fad_text t
            on t.ref_p = f.ref_p
         where f.id = P_ID;
    else
      if (P_C = 'A') then
        open io_cursor for
          select t.headings, t.text as para_text
            from t_au_observation f
           inner join t_au_observation_text t
              on f.id = t.observatsion_id
           where f.id = P_ID;
      end if;
    end if;
  
  END P_Function_Annexure_Paras_text;

  procedure P_new_compliance_summary(P_NO      in number,
                                     R_ID      in number,
                                     ENT_ID    in number,
                                     ENTITY    IN NUMBER,
                                     io_cursor OUT t_cursor) is
  
    V_F number := 0;
  begin
  
    select NVL(e.auditby_id, 0)
      into V_F
      from T_AUDITEE_ENTITIES e
     where e.entity_id = ENT_ID;
  
    --- COMPLIANCE SUMMARY FOR AUDITEE BRANCH
    if (ENT_ID in (113186)) then
      open io_cursor for
        select m.autid as Region_id,
               e.name as Region,
               (select count(c.com_id)
                  from AIS_T_AU_POST_COMPLIANCE c
                 where C.ENTITY_ID = F.ENTITY_ID
                   and c.para_status = 8) as Total_para,
               sum(case
                     when f.com_cycle != 0 then
                      1
                     else
                      0
                   end) as Total_Comp,
               sum(case
                     when f.com_stage = 21 and f.entity_type_id in (6, 28) and
                          f.para_status = 8 then
                      1
                     else
                      0
                   end) as AT_reporting,
               sum(case
                     when f.com_stage in (43, 44, 2, 7, 9) and f.para_status = 8 then
                      1
                     else
                      0
                   end) as Under_consideration,
               sum(case
                     when f.com_status in (12, 15, 18) and f.para_status = 8 then
                      1
                     else
                      0
                   end) as Rejected,
               sum(case
                     when f.com_status = 16 and f.para_status != 8 then
                      1
                     else
                      0
                   end) as settled
        
          from AIS_T_AU_POST_COMPLIANCE f
         inner join t_auditee_ent_types m
            on f.entity_type_id = m.autid
         INNER JOIN T_AUDITEE_ENTITIES E
            ON F.ENTITY_ID = E.ENTITY_ID
         inner join t_auditee_entities_maping em
            on em.entity_id = e.entity_id
        
         WHERE f.audited_by = 112248
           and f.entity_id not in (113093, 113106, 112937)
        
         group by M.AUTID, e.name, F.ENTITY_ID
         order by m.autid;
    
    ELSIF (R_ID in (13, 21, 12, 14, 4)) then
      open io_cursor for
        select m.autid as Region_id,
               e.c_name as Region,
               (select count(c.com_id)
                  from AIS_T_AU_POST_COMPLIANCE c
                 where C.ENTITY_ID = F.ENTITY_ID
                   and c.para_status = 8) as Total_para,
               sum(case
                     when f.com_cycle != 0 then
                      1
                     else
                      0
                   end) as Total_Comp,
               sum(case
                     when f.com_stage = 21 and f.entity_type_id in (6, 28) and
                          f.para_status = 8 then
                      1
                     else
                      0
                   end) as AT_reporting,
               sum(case
                     when f.com_stage in (43, 44, 2, 7, 9) and f.para_status = 8 then
                      1
                     else
                      0
                   end) as Under_consideration,
               sum(case
                     when f.com_status in (12, 15, 18) and f.para_status = 8 then
                      1
                     else
                      0
                   end) as Rejected,
               sum(case
                     when f.com_status = 16 and f.para_status != 8 then
                      1
                     else
                      0
                   end) as settled
        
          from AIS_T_AU_POST_COMPLIANCE f
         inner join t_auditee_ent_types m
            on f.entity_type_id = m.autid
         INNER JOIN T_AUDITEE_ENTITIES_MAPING E
            ON F.ENTITY_ID = E.ENTITY_ID
         inner join t_auditee_entities_maping em
            on em.entity_id = e.parent_id
        
         WHERE (f.entity_id = ENT_ID or em.parent_id = ENT_ID)
        
         group by M.AUTID, e.c_name, F.ENTITY_ID
         order by m.autid;
    ELSIF (R_ID in (1, 2, 5, 41, 43, 44) and ENTITY = 0) then
      open io_cursor for
        select t.Region_id,
               t.Region,
               Nvl(t.total_para, 0) as Total_para,
               nvl(t.Total_Comp, 0) as Total_Comp,
               NVL(t.AT_reporting, 0) as AT_reporting,
               nvl(t.Under_consideration, 0) as Under_consideration,
               nvl(t.Rejected, 0) as Rejected,
               nvl(t.settled, 0) as settled
          from v_rpt_db_p_compaince_summary_region_total t
        
        -- WHERE R.P_TYPE_ID = 21
        
         order by t.Region;
    ELSIF (R_ID in (6, 7) and ENTITY = 0) then
      open io_cursor for
        select t.Region_id,
               t.Region,
               Nvl(t.total_para, 0) as Total_para,
               nvl(t.Total_Comp, 0) as Total_Comp,
               NVL(t.AT_reporting, 0) as AT_reporting,
               nvl(t.Under_consideration, 0) as Under_consideration,
               nvl(t.Rejected, 0) as Rejected,
               nvl(t.settled, 0) as settled
          from v_rpt_db_p_compaince_summary_region_total t
         WHERE t.GM_ID = ENT_ID;
    ELSIF (R_ID IN (39) and Entity = 0) then
      open io_cursor for
        select m.autid as Region_id,
               m.entitytypedesc as Region,
               sum(case
                     when f.para_status = 8 then
                      1
                     else
                      0
                   end) as Total_para,
               sum(case
                     when f.com_cycle != 0 then
                      1
                     else
                      0
                   end) as Total_Comp,
               sum(case
                     when f.com_status in (10) and f.entity_type_id = 6 and
                          f.para_status = 8 then
                      1
                     else
                      0
                   end) as AT_reporting,
               sum(case
                     when f.com_stage in (2, 6, 7, 9, 15, 16) and
                          f.para_status = 8 then
                      1
                     else
                      0
                   end) as Under_consideration,
               sum(case
                     when f.com_status in (12, 15, 18) and f.para_status = 8 then
                      1
                     else
                      0
                   end) as Rejected,
               sum(case
                     when f.com_status = 16 and f.para_status != 8 then
                      1
                     else
                      0
                   end) as settled
          from AIS_T_AU_POST_COMPLIANCE f
         inner join t_auditee_ent_types m
            on f.entity_type_id = m.autid
         inner join T_AUDITEE_ENTITIES E
            on f.entity_id = E.entity_id
         where E.AUDITBY_ID = V_F
         group by m.autid, m.entitytypedesc;
    
    ELSIF (R_ID = 40 and Entity = 0) then
      open io_cursor for
        select m.autid as Region_id,
               m.entitytypedesc as Region,
               SUM(CASE
                     WHEN F.PARA_STATUS = 8 THEN
                      1
                     ELSE
                      0
                   END) as Total_para,
               sum(case
                     when f.com_cycle != 0 then
                      1
                     else
                      0
                   end) as Total_Comp,
               sum(case
                     when f.com_status in (10) and f.entity_type_id = 6 and
                          f.para_status = 8 then
                      1
                     else
                      0
                   end) as AT_reporting,
               sum(case
                     when f.com_stage in (2, 6, 7, 9, 15, 16) and
                          f.para_status = 8 then
                      1
                     else
                      0
                   end) as Under_consideration,
               sum(case
                     when f.com_status in (12, 15, 18) and f.para_status = 8 then
                      1
                     else
                      0
                   end) as Rejected,
               sum(case
                     when f.com_status = 16 and f.para_status != 8 then
                      1
                     else
                      0
                   end) as settled
        
          from AIS_T_AU_POST_COMPLIANCE f
         inner join t_auditee_ent_types m
            on f.entity_type_id = m.autid
         INNER JOIN T_AUDITEE_ENTITIES E
            ON F.ENTITY_ID = E.ENTITY_ID
         WHERE m.controlling = ENT_ID
         group by M.AUTID, m.entitytypedesc;
    
    ELSIF (Entity is not null and ENTITY in (4, 25, 6) and
          R_ID not in (13, 14, 21)) then
      open io_cursor for
        select t.Region_id,
               t.Region,
               Nvl(t.total_para, 0) as Total_para,
               nvl(t.Total_Comp, 0) as Total_Comp,
               NVL(t.AT_reporting, 0) as at_reporting,
               nvl(t.Under_consideration, 0) as Under_consideration,
               nvl(t.Rejected, 0) as Rejected,
               nvl(t.settled, 0) as Settled
          from v_rpt_db_p_compaince_details_branch t
         where t.autid = Entity
           and V_F = case
                 when R_ID IN (39) then
                  t.GM_ID
                 when R_ID in (1, 2, 5, 6, 7) then
                  0
                 when ent_id in (112247, 112274, 112262) then
                  112242
                 When ent_id in (112907, 112855) then
                  112925
               end
         order by t.Region_id;
    ELSIF (Entity is not null and ENTITY not in (4, 25, 6) and
          R_ID not in (13, 21)) then
      open io_cursor for
        select t.Region_id,
               t.Region,
               Nvl(t.total_para, 0) as Total_para,
               nvl(t.Total_Comp, 0) as Total_Comp,
               NVL(t.AT_reporting, 0) as at_reporting,
               nvl(t.Under_consideration, 0) as Under_consideration,
               nvl(t.Rejected, 0) as Rejected,
               nvl(t.settled, 0) as Settled
          from v_rpt_db_p_compaince_details t
         where t.autid = Entity
              --and t.GM_ID = ENT_ID
           and V_F = case
                 when R_ID in (39, 14) then
                  t.GM_ID
                 when R_ID in (1, 5, 6, 7) then
                  0
                 when ent_id in (112247, 112274, 112262, 112277) then
                  112242
                 When ent_id in (112907, 112855) then
                  112925
               end
         order by t.Region_id;
    end if;
  
  end P_new_compliance_summary;

  PROCEDURE P_COMPLIANCE_SUMMARY(P_NO      IN NUMBER,
                                 R_ID      IN NUMBER,
                                 ENT_ID    IN NUMBER,
                                 ENTITY    IN NUMBER,
                                 io_cursor OUT t_cursor) IS
    V_F NUMBER := 0;
  BEGIN
    IF ent_id IN (112242, 112248) THEN
      v_f := NVL(ent_id, 0);
    ELSIF ent_id in (112247, 112274, 112262) then
      V_F := -1;
    else
      SELECT NVL(e.gm_office, 0)
        INTO v_f
        FROM t_auditee_entities_maping e
       WHERE e.entity_id = ent_id;
    END IF;
  
    -- Example for Auditee Branch, adapt other cases similarly
    IF (R_ID in (13, 21, 12, 14, 4) and ENTITY = 0) then
      open io_cursor for
        select f.Region_id,
               f.Region,
               f.Total_para,
               f.Total_Comp,
               f.AT_reporting,
               f.Under_consideration,
               f.Rejected,
               f.settled
        
          from v_get_Para_COMPLIANCE_SUMMARY f
        
         WHERE (f.Region_id = ENT_ID or f.grand_parent_id = ENT_ID OR
               F.entity_id = ENT_ID);
    elsif R_ID in (1, 2, 3, 5, 6, 7, 39, 40, 41, 44) and ENTITY = 0 then
      open io_cursor for
        select f.type_id as Region_id,
               t.entitytypedesc as Region,
               sum(f.Total_para) as total_para,
               sum(f.Total_Comp) as total_comp,
               sum(f.AT_reporting) as AT_reporting,
               sum(f.Under_consideration) as Under_consideration,
               sum(f.Rejected) as Rejected,
               sum(f.settled) as settled
        
          from v_get_Para_COMPLIANCE_SUMMARY f
         inner join t_auditee_ent_types t
            on f.type_id = t.autid
        
         where ((R_ID IN (39) AND V_F = f.gm_office) OR
               (R_ID IN (1, 5) AND t.audit_type in ('B', 'D')) OR
               (ent_id IN (112907, 112855) AND V_F = 112925) OR
               (R_ID IN (2, 6, 7) AND f.AUDITEDBY = ENT_ID) OR
               (ent_id IN (112247, 112274, 112262) AND t.audit_type = 'B'))
        
         group by f.type_id, t.entitytypedesc;
    elsif R_ID in (1, 2, 3, 5, 6, 7, 40, 39, 41) and ENTITY != 0 then
      open io_cursor for
        select f.Region_id,
               f.Region,
               sum(f.Total_para) as total_para,
               sum(f.Total_Comp) as total_comp,
               sum(f.AT_reporting) as AT_reporting,
               sum(f.Under_consideration) as Under_consideration,
               sum(f.Rejected) as Rejected,
               sum(f.settled) as settled
        
          from v_get_Para_COMPLIANCE_SUMMARY f
         where f.type_id = ENTITY
           and ((R_ID IN (39) AND V_F = f.gm_office) OR
               (R_ID IN (1, 5) AND f.audit_type in ('B', 'D')) OR
               (ent_id IN (112907, 112855) AND V_F = 112925) OR
               (R_ID IN (2, 6, 7) AND f.AUDITEDBY = ENT_ID) OR
               (ent_id IN (112247, 112274, 112262) AND f.audit_type = 'B'))
         group by f.Region_id, f.Region;
    
    END IF;
  
  END P_COMPLIANCE_SUMMARY;

end PKG_DB;


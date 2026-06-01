create or replace package PKG_RPT is

  TYPE t_cursor IS REF CURSOR;

  -- Active Reports support
  Procedure P_GET_ENTITY_TYPE(P_NO      number,
                              R_ID      number,
                              ENT_ID    number,
                              io_cursor OUT t_cursor);

  procedure P_Getrealtionshiptype(UserRoleid IN NUMBER,
                                  io_cursor  OUT t_cursor);

  procedure P_Getparentrepoffice(rid            in number,
                                 USER_ENTITY_ID IN NUMBER,
                                 io_cursor      OUT t_cursor);

  procedure P_Getchildposting(erid           in number,
                              USER_ENTITY_ID in number,
                              io_cursor      OUT t_cursor);

  procedure P_GET_AUDIT_ZONES(io_cursor OUT t_cursor);

  procedure p_Getauditteams(ENGID in number, io_cursor OUT t_cursor);

  procedure R_GetDepartments(EntityId  in number,
                             ppnum     in number,
                             io_cursor OUT t_cursor);

  PROCEDURE R_joiningcompletion(Dept_id     in number,
                                Audit_start in date,
                                Audit_end   in date,
                                io_cursor   OUT t_cursor);

  procedure R_getauditeeAddress(EngId     in number,
                                ppnum     in number,
                                io_cursor OUT t_cursor);

  procedure R_getauditeeParas(EngId in number, io_cursor OUT t_cursor);

  procedure P_FAD_audit_Para_Reconciliation(P_NO      in number,
                                            ENT_ID    in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure p_get_loan_status(io_cursor OUT t_cursor);

  Procedure R_get_rbh_list(gm in number, io_cursor OUT t_cursor);

  procedure P_find_gist(gst varchar2, io_cursor OUT t_cursor);

  Procedure P_GET_COM_PROGREE_REPORT(R_TYPE    Varchar2,
                                     io_cursor OUT t_cursor);

  Procedure P_GET_COM_PROGREE_REPORT_DETAIL(R_TYPE    Varchar2,
                                            P_NO      number,
                                            io_cursor OUT t_cursor);

  Procedure P_GET_ENTITY_TYPE_FOR_SETTLEMENT(R_ID      number,
                                             ENT_ID    number,
                                             P_NO      number,
                                             io_cursor OUT t_cursor);

  Procedure P_GET_COMPLIANCE_REPORT(ENT_ID      number,
                                    S_ENT_ID    in number,
                                    S_DATE_FROM in date,
                                    S_DATE_TO   in date,
                                    R_ID        number,
                                    P_NO        number,
                                    io_cursor   OUT t_cursor);

  Procedure p_delay_audits(P_NO      number,
                           ENT_ID    NUMBER,
                           R_ID      NUMBER,
                           io_cursor OUT t_cursor);

  Procedure R_FAD_MONTHLY_REVIEW(P_NO      in number,
                                 ENT_ID    in number,
                                 R_ID      in number,
                                 R_T       in number,
                                 S_Date    in date,
                                 E_Date    in date,
                                 io_cursor OUT t_cursor);

  Procedure P_GET_GM_WISE_SERIOUS_PARAS(P_NO      in number,
                                        ENT_ID    in number,
                                        R_ID      in number,
                                        R_T       in number,
                                        S_Date    in date,
                                        E_Date    in date,
                                        io_cursor OUT t_cursor);

  Procedure P_GET_GM_WISE_SERIOUS_PARAS_DETAILS(PARENT_ENT_ID in number,
                                                IND           in varchar2,
                                                P_ANNEX       in varchar2,
                                                P_NO          in number,
                                                ENT_ID        in number,
                                                R_ID          in number,
                                                io_cursor     OUT t_cursor);

  Procedure P_GET_PARA_TEXT_WORDS_V2(T_TEXT    varchar2,
                                     io_cursor OUT t_cursor);

  Procedure P_GET_FAD_DESK_OFFICER_RPT_BY_PERIOD(startDate date,
                                                 endDate   date,
                                                 io_cursor OUT t_cursor);

  PROCEDURE P_GET_SETTLED_PARA_ENTITIES(P_NO      IN NUMBER,
                                        ENT_ID    IN NUMBER,
                                        R_ID      IN NUMBER,
                                        io_cursor OUT t_cursor);

  PROCEDURE P_GET_SETTLED_PARA_DETAILS(P_NO       IN NUMBER,
                                       ENT_ID     IN NUMBER,
                                       R_ID       IN NUMBER,
                                       auditee_id IN NUMBER,
                                       io_cursor  OUT t_cursor);

  PROCEDURE P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE(P_COM_ID  IN NUMBER,
                                                       io_cursor OUT t_cursor);

  PROCEDURE P_GetParasForCompliancehistory(P_COM_ID  IN NUMBER,
                                           io_cursor OUT t_cursor);

  PROCEDURE P_GetParasForComplianceforhistory(P_C_CYCLE IN NUMBER,
                                              P_COM_ID   IN NUMBER,
                                              io_cursor  OUT t_cursor);

End PKG_RPT;
/

create or replace package body PKG_RPT is

  -- Active Reports support
  Procedure P_GET_ENTITY_TYPE(P_NO      number,
                              R_ID      number,
                              ENT_ID    number,
                              io_cursor OUT t_cursor) is
  begin
  
    if (ENT_ID = 112243 or R_ID in (41, 45)) then
      open io_cursor for
        select DISTINCT e.autid          as entitycode,
                        e.entitytypedesc as entity_type
          from t_auditee_ent_types e
         where e.audit_type = 'B'
        union
        Select -1 as entitycode, 'All' as entity_type
          from dual
        
        ;
    
    elsif (r_id in (1, 3, 5, 6, 7)) then
    
      open io_cursor for
        select DISTINCT e.entitytypedesc as entity_type, e.entitycode
          from t_auditee_ent_types e
         inner join t_auditee_entities d
            on d.type_id = e.autid
           and d.auditby_id = case
                 when R_ID in (1, 3, 41) then
                  d.auditby_id
                 else
                  ENT_ID
               end
        union
        Select 'All' as entity_type, '-1' as entitycode
        
          from dual;
    elsif (r_id in (43, 44)) then
      open io_cursor for
        select DISTINCT e.entitytypedesc as entity_type, e.entitycode
          from t_auditee_ent_types e
         inner join t_auditee_entities et
            on e.autid = et.type_id
         inner join v_Get_Compliance_Reviewer_Approver r
            on r.COM_KEY = et.complice_by
         where r.ENTITY_ID = ENT_ID;
    elsif (r_id in (40)) then
      open io_cursor for
        select DISTINCT e.entitytypedesc as entity_type, e.entitycode
          from t_auditee_ent_types e
         where e.controlling = ENT_ID;
    elsif (r_id in (15, 16)) then
      open io_cursor for
        select DISTINCT e.entitytypedesc as entity_type, e.entitycode
          from t_auditee_ent_types e
         where E.AUTID IN (5, 6, 7, 17, 21, 25, 24, 26, 28)
        union
        Select 'All' as entity_type, '-1' as entitycode
        
          from dual;
    end if;
  
  end P_GET_ENTITY_TYPE;

  procedure P_Getrealtionshiptype(UserRoleid IN NUMBER,
                                  io_cursor  OUT t_cursor) is
  begin
    if (UserRoleid IN (15, 16, 17)) then
      open io_cursor for
        select f.entity_realtion_id,
               f.parent_name || '   TO   ' || f.chlid_name as field_name
          from t_auditee_ent_relation f
         where f.status = 'Y'
           and f.id is not null
           and f.id in (5, 9)
         order by f.id;
    elsif (UserRoleid = 35) then
      open io_cursor for
        select f.entity_realtion_id,
               f.parent_name || '   TO   ' || f.chlid_name as field_name
          from t_auditee_ent_relation f
         where f.status = 'Y'
           and f.id is not null
           and f.id in (1, 2)
         order by f.id;
    elsif (UserRoleid in (1, 3)) then
      open io_cursor for
        select f.entity_realtion_id,
               f.parent_name || '   TO   ' || f.chlid_name as field_name
          from t_auditee_ent_relation f
         where f.status = 'Y'
           and f.id is not null
         order by f.id;
    elsif (UserRoleid in (6, 7)) then
      open io_cursor for
        select f.entity_realtion_id,
               f.parent_name || '   TO   ' || f.chlid_name as field_name
          from t_auditee_ent_relation f
         where f.status = 'Y'
           and f.entity_realtion_id in (3)
         order by f.id;
    end if;
  
  end P_Getrealtionshiptype;

  procedure P_Getparentrepoffice(rid            in number,
                                 USER_ENTITY_ID IN NUMBER,
                                 io_cursor      OUT t_cursor) is
  
  begin
    if (rid = 5) then
      open io_cursor for
        select Distinct (r.p_name) as DESCRIPTION,
                        r.parent_id as ENTITY_ID,
                        r.relation_type_id as ENTITY_REALTION_ID,
                        t.entitytypedesc as ENTITYTYPEDESC,
                        r.status as ACTIVE
          from t_auditee_ent_relation    e,
               t_auditee_ent_types       t,
               T_AUDITEE_ENTITIES_MAPING r,
               t_auditee_entities        et
         where t.autid = r.relation_type_id
           and r.p_type_id = e.parent_entity_typeid
           and r.c_type_id = e.child_entity_typeid
           and r.relation_type_id = rid
           and r.parent_id is not null
           and et.entity_id = r.parent_id
           and et.auditby_id = USER_ENTITY_ID
         order by r.p_name;
    else
      if (rid = 4) then
        open io_cursor for
          select Distinct (r.p_name) as DESCRIPTION,
                          r.parent_id as ENTITY_ID,
                          r.relation_type_id as ENTITY_REALTION_ID,
                          t.entitytypedesc as ENTITYTYPEDESC,
                          r.status as ACTIVE
            from t_auditee_entities et
           inner join T_AUDITEE_ENTITIES_MAPING r
              on et.entity_id = r.entity_id
           inner join t_auditee_ent_relation e
              on e.parent_entity_typeid = r.p_type_id
             and e.child_entity_typeid = r.c_type_id
           inner join t_auditee_ent_types t
              on t.autid = r.relation_type_id
          
           where r.relation_type_id = 4
             and et.type_id in (5, 7, 17, 25, 21, 20, 23, 22)
             and r.parent_id is not null
             and et.auditby_id = USER_ENTITY_ID
           order by r.p_name;
      else
        open io_cursor for
          select Distinct (r.p_name) as DESCRIPTION,
                          r.parent_id as ENTITY_ID,
                          r.relation_type_id as ENTITY_REALTION_ID,
                          t.entitytypedesc as ENTITYTYPEDESC,
                          r.status as ACTIVE
            from t_auditee_entities et
           inner join T_AUDITEE_ENTITIES_MAPING r
              on et.entity_id = r.entity_id
           inner join t_auditee_ent_relation e
              on e.parent_entity_typeid = r.p_type_id
             and e.child_entity_typeid = r.c_type_id
           inner join t_auditee_ent_types t
              on t.autid = r.relation_type_id
          
           where r.relation_type_id = rid
                
             and r.parent_id is not null
           order by r.p_name;
      end if;
    end if;
  
  end P_Getparentrepoffice;

  procedure P_Getchildposting(erid           in number,
                              USER_ENTITY_ID in number,
                              io_cursor      OUT t_cursor) is
  
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
       order by r.c_name;
  end P_Getchildposting;

  procedure P_GET_AUDIT_ZONES(io_cursor OUT t_cursor) as
  
  begin
  
    OPEN io_cursor FOR
      Select z.entity_id      as ZONE_ID,
             z.code,
             z.description,
             z.name           as ZONE_NAME,
             z.type_id,
             z.auditby_id,
             z.inspectedby_id,
             z.cost_center,
             z.active,
             z.auditable
        FROM t_auditee_entities z
       WHERE z.type_id = '9'
       order by z.name asc;
  
  end P_GET_AUDIT_ZONES;

  procedure p_Getauditteams(ENGID in number, io_cursor OUT t_cursor) is
  
  begin
    OPEN io_Cursor FOR
      select distinct (tm.team_name),
                      tm.member_ppno,
                      tm.member_name,
                      tm.isteamlead,
                      tk.audit_start_date,
                      tk.audit_end_date
        from t_au_audit_teams m
       inner join t_au_team_members tm
          on tm.t_id = m.team_id
       inner join t_au_audit_team_tasklist tk
          on tk.team_id = tm.t_id
         and tk.eng_plan_id = engid
       where m.eng_id = ENGID
       order by tm.team_name asc;
  
  end p_Getauditteams;

  procedure R_GetDepartments(EntityId  in number,
                             ppnum     in number,
                             io_cursor OUT t_cursor) is
    R_F number := 0;
    E_F number := 0;
  
  begin
    select nvl(max(u.entity_id), 0)
      into E_F
      from t_user u
     where u.ppno = ppnum;
    select NVL(max(m.role_id), 0)
      into R_F
      from t_User_Maping m
     where m.ppno = ppnum;
    if (R_F in (5, 15)) then
      open io_cursor for
        select mp.parent_id  as DIVISIONID,
               mp.entity_id  as ID,
               mp.c_name     as NAME,
               mp.child_code as CODE,
               e.active      as ISACTIVE,
               mp.p_name     as DIV_NAME,
               mp.auditedby  as AUDITED_BY_DEPID
          from t_auditee_entities e
         inner join t_auditee_entities_maping mp
            on mp.parent_id = e.entity_id
         where mp.entity_id is not null
           and e.entity_id = E_F
        union
        select 112243 as DIVISIONID,
               112243 as ID,
               'All' as NAME,
               112243 as CODE,
               'Y' as ISACTIVE,
               'Y' as DIV_NAME,
               112243 as AUDITED_BY_DEPID
          from dual;
    
    else
      open io_cursor for
        select mp.parent_id  as DIVISIONID,
               mp.entity_id  as ID,
               mp.c_name     as NAME,
               mp.child_code as CODE,
               mp.status     as ISACTIVE,
               mp.p_name     as DIV_NAME,
               mp.auditedby  as AUDITED_BY_DEPID
          from t_auditee_entities e, t_auditee_entities_maping mp
         where mp.entity_id is not null
           and e.type_id IN (3)
           and e.entity_id = mp.parent_id
           and e.entity_id = EntityId;
    end if;
  end R_GetDepartments;

  PROCEDURE R_joiningcompletion(Dept_id     IN NUMBER,
                                Audit_start IN DATE,
                                Audit_end   IN DATE,
                                io_cursor   OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT dem.c_name         AS audit_by,
             e.p_name           as Reporting,
             e.child_code       as code,
             e.c_name           AS auditee_name,
             r.description      AS Risk,
             ep.audit_startdate AS Start_date,
             ep.audit_enddate   AS End_date,
             st.status          AS status,
             ep.lastupdateddate AS issuance_date
        FROM t_au_plan_eng ep
       inner join t_auditee_entities ee
          on ee.entity_id = ep.entity_id
       INNER JOIN t_auditee_entities_maping e
          ON e.entity_id = ep.entity_id
       inner join t_au_plan_eng_status st
          on st.id = ep.status
       INNER JOIN t_risk r
          ON r.rating = ee.risk_id
       inner JOIN t_auditee_entities_maping dem
          ON dem.entity_id = ep.auditby_id
       WHERE ((Dept_id = 112243 AND dem.parent_id = Dept_id) OR
             (Dept_id <> 112243 AND dem.entity_id = Dept_id))
         AND TRUNC(ep.audit_startdate) >= TRUNC(Audit_start)
         AND TRUNC(ep.audit_enddate) <= TRUNC(Audit_end)
       ORDER BY ep.auditby_id, ep.audit_startdate;
  
  END R_joiningcompletion;

  procedure R_getauditeeAddress(EngId     in number,
                                ppnum     in number,
                                io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select ep.eng_id,
             e.entity_id,
             NVL(mp.p_name, '-') as p_name,
             e.code,
             e.name,
             NVL(ed.address, ' --- ') as address,
             NVL(ed.date_of_opening, '01-Jan-9999') as date_of_opening,
             NVL(ed.license, ' Not Applicable ') as license,
             ep.audit_startdate,
             ep.audit_enddate,
             ep.operation_startdate,
             ep.operation_enddate,
             sum(case
                   when r.rating = 1 then
                    1
                   else
                    0
                 end) as High,
             sum(case
                   when r.rating = 2 then
                    1
                   else
                    0
                 end) as Medium,
             sum(case
                   when r.rating = 3 then
                    1
                   else
                    0
                 end) as Low,
             sum(case
                   when r.rating = 1 and o.status in (9, 23) then
                    1
                   else
                    0
                 end) as Settle_High,
             sum(case
                   when r.rating = 2 and o.status in (9, 23) then
                    1
                   else
                    0
                 end) as settle_Medium,
             sum(case
                   when r.rating = 3 and o.status in (9, 23) then
                    1
                   else
                    0
                 end) as settle_Low,
             sum(case
                   when r.rating = 1 and o.status = 8 then
                    1
                   else
                    0
                 end) as open_High,
             sum(case
                   when r.rating = 2 and o.status = 8 then
                    1
                   else
                    0
                 end) as open_Medium,
             sum(case
                   when r.rating = 3 and o.status = 8 then
                    1
                   else
                    0
                 end) as open_Low
        from t_au_plan_eng ep
       inner join t_auditee_entities e
          on ep.entity_id = e.entity_id
       INNER join t_au_observation o
          on o.engplanid = ep.eng_id
       inner join t_risk r
          on r.rating = o.severity
        left join t_auditee_entities_address ed
          on ep.entity_id = ed.entity_id
        left join t_auditee_entities_maping mp
          on e.entity_id = mp.entity_id
       Where ep.eng_id = EngId
       group by ep.eng_id,
                e.entity_id,
                mp.p_name,
                e.code,
                e.name,
                ed.address,
                ed.date_of_opening,
                ed.license,
                ep.audit_startdate,
                ep.audit_enddate,
                ep.operation_startdate,
                ep.operation_enddate;
  
  end R_getauditeeAddress;

  procedure R_getauditeeParas(EngId in number, io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select NVL(max(p.engplanid), 0)
      into V_F
      from t_audit_para p
     where p.engplanid = EngId;
    if (V_F != 0) then
      open io_cursor for
        select (case
                 when eg.eng_id = o.engplanid and o.entity_id = eg.entity_id then
                  'CAU'
                 else
                  'BRANCH'
               end) Assignedto,
               e.name as entity_name,
               pr.para_no,
               ot.memo_number,
               vd.description as V_Header,
               cd.heading as V_detail,
               rsk.description as Risk,
               ot.text as Observation,
               r.reply as Management_reply,
               ad.recommendation,
               'Please Review the audit report and in case of any discrepancy contact system administrator' as Message,
               '1' as REF_OUT
          from t_au_observation o
         inner join t_au_plan_eng eg
            on eg.eng_id = o.engplanid
         inner join t_au_observation_text ot
            on o.id = ot.observatsion_id
         inner join t_au_observations_auditee_response r
            on r.obs_text_id = ot.observatsion_id
         inner join t_au_observations_auditor_recommendation ad
            on ot.observatsion_id = ad.obs_text_id
         inner join t_au_observations_auditor_reply rs
            on rs.au_obs_id = o.id
           and rs.obs_text_id = ot.id
           and rs.obs_status = o.status
         inner join t_auditee_entities e
            on e.entity_id = o.entity_id
         inner join t_audit_para pr
            on pr.engplanid = o.engplanid
           and pr.memo_number = o.memo_number
           and pr.obid = o.id
         inner join t_audit_checklist_details cd
            on cd.id = o.checklistdetail_id
         inner join t_audit_checklist_sub csb
            on csb.s_id = cd.s_id
         inner join t_audit_checklist cl
            on cl.t_id = csb.t_id
         inner join t_risk rsk
            on rsk.r_id = cd.risk_id
         INNER JOIN T_audit_violation_detail vd
            ON vd.s_gr_id = cd.v_id
         where o.engplanid = EngId
           and o.status = 8
         order by pr.para_no;
    
    else
      open io_cursor for
        select 'Please Conculded/Close the Audit First' as Message,
               '0' as REF_OUT
          from dual;
    end if;
  
  end R_getauditeeParas;

  procedure P_FAD_audit_Para_Reconciliation(P_NO      in number,
                                            ENT_ID    in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor) as
  
    T_F number := 0;
  begin
    select e.type_id
      into T_F
      from t_auditee_entities e
     where e.entity_id = ent_id;
    if (T_F = 9) then
      open io_cursor for
        select r.name as Audit_zone,
               r.entitytypedesc,
               r.Reporting_Office,
               r.Auditee,
               r.Open_balance,
               r.Added,
               r.Total,
               r.Settled_Legacy,
               r.Settled_New_Paras,
               r.Un_Settled,
               round(((r.Settled_Legacy + r.Settled_New_Paras) / r.Total) * 100) as percentage,
               'Z' as ind,
               sum(r.R1) as R1,
               sum(r.R2) as R2,
               sum(r.R3) as R3
          from v_FAD_audit_Para_Reconciliation r
         where r.az_id = ENT_ID
           and r.Total > 0
         group by r.name,
                  r.entitytypedesc,
                  r.Reporting_Office,
                  r.Auditee,
                  r.Open_balance,
                  r.Added,
                  r.Total,
                  r.Settled_Legacy,
                  r.Settled_New_Paras,
                  r.Un_Settled
         order by r.parent_id;
    else
      if (r_id in (1, 3)) then
        open io_cursor for
          select r.name as Audit_zone,
                 r.auditby_id,
                 '' as entitytypedesc,
                 '' as Reporting_Office,
                 '' as Auditee,
                 sum(r.Open_balance) as Open_balance,
                 sum(r.Added) as added,
                 sum(r.Total) as total,
                 sum(r.Settled_Legacy) as Settled_Legacy,
                 sum(r.Settled_New_Paras) as Settled_New_Paras,
                 sum(r.Un_Settled) as Un_Settled,
                 round((sum(r.Settled_New_Paras) + sum(r.Settled_Legacy)) /
                       (sum(r.Total)) * 100) as percentage,
                 'A' as ind,
                 sum(r.R1) as r1,
                 sum(r.R2) as r2,
                 sum(r.R3) as r3
            from v_FAD_audit_Para_Reconciliation r
           where r.Total > 0
             and r.name is not null
           group by r.name, r.auditby_id
           order by r.auditby_id;
      
      else
        if (r_id in (6, 7, 9, 11)) then
          open io_cursor for
            select r.name as Audit_zone,
                   r.auditby_id,
                   r.entitytypedesc,
                   r.Reporting_Office,
                   r.Auditee,
                   r.Open_balance,
                   r.Added,
                   r.Total,
                   r.Settled_Legacy,
                   r.Settled_New_Paras,
                   r.Un_Settled,
                   round(((r.Settled_Legacy + r.Settled_New_Paras) /
                         r.Total) * 100) as percentage,
                   'Z' as ind,
                   sum(r.R1) as R1,
                   sum(r.R2) as R2,
                   sum(r.R3) as R3
              from v_FAD_audit_Para_Reconciliation r
             where r.auditby_id = ENT_ID
               and r.Total > 0
             group by r.name,
                      r.entitytypedesc,
                      r.Reporting_Office,
                      r.Auditee,
                      r.Open_balance,
                      r.Added,
                      r.Total,
                      r.Settled_Legacy,
                      r.Settled_New_Paras,
                      r.Un_Settled
             order by r.az_id, r.parent_id;
        else
          if (r_id in (5, 11)) then
            open io_cursor for
            
              select r.name as Audit_zone,
                     r.az_id,
                     r.parent_id,
                     r.entitytypedesc,
                     r.Reporting_Office,
                     r.Auditee,
                     r.Open_balance,
                     r.Added,
                     r.Total,
                     r.Settled_Legacy,
                     r.Settled_New_Paras,
                     r.Un_Settled,
                     round(((r.Settled_Legacy + r.Settled_New_Paras) /
                           r.Total) * 100) as percentage,
                     'Z' as ind,
                     sum(r.R1) as R1,
                     sum(r.R2) as R2,
                     sum(r.R3) as R3
                from v_FAD_audit_Para_Reconciliation r
               where r.auditby_id not in (112248, 112242)
                 and r.Total > 0
               group by r.name,
                        r.entitytypedesc,
                        r.az_id,
                        r.parent_id,
                        r.Reporting_Office,
                        r.Auditee,
                        r.Open_balance,
                        r.Added,
                        r.Total,
                        r.Settled_Legacy,
                        r.Settled_New_Paras,
                        r.Un_Settled
               order by r.az_id, r.parent_id;
          else
            if (ENT_ID = 112243) then
              open io_cursor for
                select r.name as Audit_zone,
                       r.entitytypedesc,
                       r.Reporting_Office,
                       r.Auditee,
                       r.Open_balance,
                       r.Added,
                       r.Total,
                       r.Settled_Legacy,
                       r.Settled_New_Paras,
                       r.Un_Settled,
                       round(((r.Settled_Legacy + r.Settled_New_Paras) /
                             r.Total) * 100) as percentage,
                       'F' as ind,
                       sum(r.R1) as R1,
                       sum(r.R2) as R2,
                       sum(r.R3) as R3
                  from v_FAD_audit_Para_Reconciliation r
                 inner join t_auditee_entities_maping_fad f
                    on f.entity_id = r.az_id
                   and r.auditby_id not in (112248, 112242)
                 where f.ppno = P_NO
                   and r.Total > 0
                 group by r.name,
                          r.entitytypedesc,
                          r.Reporting_Office,
                          r.Auditee,
                          r.Open_balance,
                          r.Added,
                          r.Total,
                          r.Settled_Legacy,
                          r.Settled_New_Paras,
                          r.Un_Settled
                 order by r.name, r.entitytypedesc, r.parent_id;
            else
              if (R_ID = 14) then
                open io_cursor for
                  select r.name as Audit_zone,
                         r.entitytypedesc,
                         r.Reporting_Office,
                         r.Auditee,
                         r.Open_balance,
                         r.Added,
                         r.Total,
                         r.Settled_Legacy,
                         r.Settled_New_Paras,
                         r.Un_Settled,
                         round(((r.Settled_Legacy + r.Settled_New_Paras) /
                               r.Total) * 100) as percentage,
                         'F' as ind,
                         sum(r.R1) as R1,
                         sum(r.R2) as R2,
                         sum(r.R3) as R3
                    from v_man_audit_para_reconciliation r
                   where r.parent_id = ENT_ID
                     and r.Total > 0
                   group by r.name,
                            r.entitytypedesc,
                            r.Reporting_Office,
                            r.Auditee,
                            r.Open_balance,
                            r.Added,
                            r.Total,
                            r.Settled_Legacy,
                            r.Settled_New_Paras,
                            r.Un_Settled
                   order by r.name, r.entitytypedesc, r.parent_id;
              
              end if;
            end if;
          end if;
        end if;
      end if;
    end if;
  end P_FAD_audit_Para_Reconciliation;

  procedure p_get_loan_status(io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select t.accountstatusid, t.description
        from TEMP_ATAS_T_DISB_STATUS t
      ;
  end p_get_loan_status;

  Procedure R_get_rbh_list(gm in number, io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select e.entity_id,
             e.child_code as code,
             e.c_name     as name,
             e.auditedby  as auditby_id
        from t_auditee_entities_maping e
       where e.parent_id = gm;
  end R_get_rbh_list;

  procedure P_find_gist(gst varchar2, io_cursor OUT t_cursor) as
  
  begin
  
    open io_cursor for
      select az.name as audit_zone,
             m.p_name as Region,
             e.name as branch,
             e.code as branch_code,
             trunc(fd.entereddate) entereddate,
             g.para_no,
             nvl(n.code, '-') as annex,
             g.gist_of_paras,
             nvl(g.no_of_instances, 0) no_of_instances,
             nvl(g.amount_involved, '-') amount_involved
      
        from t_auditee_entities e
       inner join t_auditee_entities_maping m
          on m.entity_id = e.entity_id
       inner join t_auditee_entities az
          on az.entity_id = e.auditby_id
       inner join t_au_observation_fad fd
          on fd.entity_id = e.entity_id
       inner join V_GET_PARA_GIST g
          on g.entity_id = fd.entity_id
         and g.IND = fd.IND
         and ((g.para_id = fd.old_para_id and fd.IND = 'O') or
             (g.para_id = fd.new_paraid and fd.IND = 'A'))
       inner join t_audit_checklist_annexure n
          on n.id = g.annex
       where upper(g.gist_of_paras) like ('%' || upper(gst) || '%');
  
  end P_find_gist;

  Procedure P_GET_COM_PROGREE_REPORT(R_TYPE    Varchar2,
                                     io_cursor OUT t_cursor) as
  
  begin
    if (R_TYPE = 'A') then
      open io_cursor for
        select a.APPROVER_PPNO as PP_NO,
               a.Approver_name as Name,
               a.Total,
               a.Referred_Back,
               a.Settled as Recommended,
               (case
                 when (a.total - a.Settled - a.Referred_Back) < 0 then
                  0
                 else
                  (a.total - a.Settled - a.Referred_Back)
               end) as pending,
               (select max(s.logged_in_date)
                  from t_user_session s
                 where s.user_pp_number = a.APPROVER_PPNO) as last_login
        
          from v_get_compliance_progress_APPROVER a;
    else
    
      open io_cursor for
        select r.REVIEWER_PPNO as PP_NO,
               r.Reviewer_name as Name,
               r.Total,
               r.Referred_Back,
               r.Recommended,
               (case
                 when (r.total - r.Recommended - r.Referred_Back) < 0 then
                  0
                 else
                  (r.total - r.Recommended - r.Referred_Back)
               end) as pending,
               (select max(s.logged_in_date)
                  from t_user_session s
                 where s.user_pp_number = r.REVIEWER_PPNO) as last_login
        
          from v_get_compliance_progress_reviewer r;
    
    end if;
  
  end P_GET_COM_PROGREE_REPORT;

  Procedure P_GET_COM_PROGREE_REPORT_DETAIL(R_TYPE    Varchar2,
                                            P_NO      number,
                                            io_cursor OUT t_cursor) as
  
  begin
    if (R_TYPE = 'A') then
      open io_cursor for
        select a.Compliance_Unit,
               a.parent_id,
               a.p_name,
               a.entity_id,
               a.entity_id as code,
               a.name,
               a.COM_KEY,
               a.APPROVER_PPNO as PP_NO,
               a.Approver_name as emp_name,
               a.total,
               a.Satisfied,
               a.Refered_back,
               (case
                 when (a.total - a.Satisfied - a.Refered_back) < 0 then
                  0
                 else
                  (a.total - a.Satisfied - a.Refered_back)
               end) as pending
        
          from v_get_compliance_progress_approver_details a
         where a.APPROVER_PPNO = P_NO;
    else
    
      open io_cursor for
        select r.Compliance_Unit,
               r.parent_id,
               r.p_name,
               r.entity_id,
               r.entity_id as code,
               r.name,
               r.COM_KEY,
               r.REVIEWER_PPNO as PP_NO,
               r.Reviewer_name as emp_name,
               r.total,
               r.Recommended as Satisfied,
               r.Referred_Back as Refered_back,
               (case
                 when (r.total - r.Recommended - r.Referred_Back) < 0 then
                  0
                 else
                  (r.total - r.Recommended - r.Referred_Back)
               end) as pending
        
          from v_get_compliance_progress_reviewer_details r
         where r.REVIEWER_PPNO = P_NO;
    
    end if;
  
  end P_GET_COM_PROGREE_REPORT_DETAIL;

  PROCEDURE P_GET_ENTITY_TYPE_FOR_SETTLEMENT(R_ID      NUMBER,
                                             ENT_ID    NUMBER,
                                             P_NO      NUMBER,
                                             io_cursor OUT t_cursor) AS
    T_F NUMBER := 0;
  BEGIN
    SELECT NVL(MAX(e.type_id), 0)
      INTO T_F
      FROM t_auditee_entities e
     WHERE e.entity_id = ENT_ID;
  
    CASE
      WHEN R_ID IN (1, 5, 6, 7) THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc FROM t_auditee_ent_types t;
      
      WHEN R_ID = 9 THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE t.audited_by_enitity = ENT_ID;
      
      WHEN R_ID = 39 THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE t.autid IN (5, 6, 20, 21, 28);
      
      WHEN R_ID IN (15, 16, 21) THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE t.autid IN (5, 6, 28, 25);
      
      WHEN R_ID IN (14, 24, 12, 13) THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE t.autid = T_F;
      
      WHEN R_ID IN (43, 44, 45) THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE t.autid = 6;
      
      WHEN R_ID = 40 THEN
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE t.autid IN
                 (CASE WHEN ENT_ID = 112247 THEN 6 WHEN ENT_ID = 112262 THEN 25 ELSE NULL END);
      
      ELSE
        OPEN io_cursor FOR
          SELECT t.autid, t.entitytypedesc
            FROM t_auditee_ent_types t
           WHERE 1 = 0;
    END CASE;
  END P_GET_ENTITY_TYPE_FOR_SETTLEMENT;

  Procedure P_GET_COMPLIANCE_REPORT(ENT_ID      number,
                                    S_ENT_ID    number,
                                    S_DATE_FROM date,
                                    S_DATE_TO   date,
                                    R_ID        number,
                                    P_NO        number,
                                    io_cursor   OUT t_cursor) as
  
  begin
  
    if (R_ID in (1, 5, 6, 7)) then
      open io_cursor for
        SELECT f.parent_id,
               f.p_name,
               f.c_name,
               f.entity_id,
               f.auditedby,
               f.period,
               f.para_no,
               f.Gist,
               TO_CHAR(f.stelled_on, 'DD/MM/YYYY') as setteled_on
          FROM v_get_settlement_report f
         WHERE f.c_type_id = CASE
                 WHEN S_ENT_ID = 0 THEN
                  f.c_type_id
                 ELSE
                  S_ENT_ID
               END
           and f.stelled_on between S_DATE_FROM and S_DATE_TO;
    
    else
      if (R_ID in (39)) then
        open io_cursor for
          select f.parent_id,
                 f.p_name,
                 f.c_name,
                 f.entity_id,
                 f.auditedby,
                 f.period,
                 f.para_no,
                 f.Gist,
                 f.stelled_on as setteled_on
            from v_get_settlement_report f
           INNER JOIN T_AUDITEE_ENTITIES_MAPING M
              ON (M.ENTITY_ID = F.entity_id OR m.entity_id = F.parent_id)
            left JOIN T_AUDITEE_ENTITIES_MAPING MM
              ON MM.ENTITY_ID = M.PARENT_ID
           where (MM.ENTITY_ID = ent_id or f.entity_id = ent_id)
             and f.stelled_on between S_DATE_FROM and S_DATE_TO
             AND F.AUTID = CASE
                   WHEN S_ENT_ID = 0 THEN
                    F.AUTID
                   ELSE
                    S_ENT_ID
                 END
           ORDER BY F.AUTID;
      
      else
        if (R_ID in (15, 16)) then
          open io_cursor for
            select f.parent_id,
                   f.p_name,
                   f.c_name,
                   f.entity_id,
                   f.auditedby,
                   f.period,
                   f.para_no,
                   f.Gist,
                   f.stelled_on as setteled_on
              from v_get_settlement_report f
             where f.AUDIT_BY_ID = ENT_ID
               and f.c_type_id = CASE
                     WHEN S_ENT_ID = 0 THEN
                      f.c_type_id
                     ELSE
                      S_ENT_ID
                   END
                  
               and f.stelled_on between S_DATE_FROM and S_DATE_TO
             ORDER BY F.AUTID;
        else
          if (R_ID in (43)) then
            open io_cursor for
              select f.parent_id,
                     f.p_name,
                     f.c_name,
                     f.entity_id,
                     f.auditedby,
                     f.period,
                     f.para_no,
                     f.Gist,
                     f.stelled_on as setteled_on
              
                from v_get_settlement_report f
               where f.entity_id in
                     (select e.entity_id
                        from t_auditee_entities e
                       where e.compliance_unit in
                             (select cm.Entity_Id
                                from t_auditee_entities_maping_com cm
                               where cm.reviewer_ppno = P_NO))
                 and f.c_type_id = CASE
                       WHEN S_ENT_ID = 0 THEN
                        f.c_type_id
                       ELSE
                        S_ENT_ID
                     END
                 and f.stelled_on between S_DATE_FROM and S_DATE_TO;
          
          else
            if (R_ID in (44)) then
              open io_cursor for
                select f.parent_id,
                       f.p_name,
                       f.c_name,
                       f.entity_id,
                       f.auditedby,
                       f.period,
                       f.para_no,
                       f.Gist,
                       f.stelled_on as setteled_on
                
                  from v_get_settlement_report f
                 where f.entity_id in
                       (select e.entity_id
                          from t_auditee_entities e
                         where e.compliance_unit in
                               (select cm.Entity_Id
                                  from t_auditee_entities_maping_com cm
                                 where cm.approver_ppno = P_NO))
                   and f.c_type_id = CASE
                         WHEN S_ENT_ID = 0 THEN
                          f.c_type_id
                         ELSE
                          S_ENT_ID
                       END
                   and f.stelled_on between S_DATE_FROM and S_DATE_TO
                 ORDER BY F.AUTID;
            else
              if (R_ID in (21)) then
                open io_cursor for
                  select f.parent_id,
                         f.p_name,
                         f.c_name,
                         f.entity_id,
                         f.auditedby,
                         f.period,
                         f.para_no,
                         f.Gist,
                         f.stelled_on as setteled_on
                  
                    from v_get_settlement_report f
                   where (f.entity_id in
                         (select cm.Entity_Id
                             from t_auditee_entities_maping cm
                            where cm.parent_id = ENT_ID) or
                         f.entity_id = ENT_ID)
                     AND F.AUTID = CASE
                           WHEN S_ENT_ID = 0 THEN
                            F.AUTID
                           ELSE
                            S_ENT_ID
                         END
                        
                     and f.stelled_on between S_DATE_FROM and S_DATE_TO
                   ORDER BY F.AUTID;
              else
                if (R_ID in (40)) then
                  open io_cursor for
                    select f.parent_id,
                           f.p_name,
                           f.c_name,
                           f.entity_id,
                           f.auditedby,
                           f.period,
                           f.para_no,
                           f.Gist,
                           f.stelled_on as setteled_on
                    
                      from v_get_settlement_report f
                     where f.AUTID in case
                             when ENT_ID = 112247 then
                              5
                             when ENT_ID = 112262 then
                              25
                           end
                        or F.AUTID IN case
                             when ENT_ID = 112247 then
                              6
                           end
                       and f.stelled_on between S_DATE_FROM and S_DATE_TO
                     ORDER BY F.AUTID;
                else
                  open io_cursor for
                    select f.parent_id,
                           f.p_name,
                           f.c_name,
                           f.entity_id,
                           f.auditedby,
                           f.period,
                           f.para_no,
                           f.Gist,
                           f.stelled_on as setteled_on
                      from v_get_settlement_report f
                     where f.entity_id = ENT_ID
                       and f.stelled_on between S_DATE_FROM and S_DATE_TO;
                end if;
              end if;
            end if;
          end if;
        end if;
      end if;
    end if;
  end P_GET_COMPLIANCE_REPORT;

  Procedure p_delay_audits(P_NO      number,
                           ENT_ID    NUMBER,
                           R_ID      NUMBER,
                           io_cursor OUT t_cursor) as
  BEGIN
    OPEN IO_CURSOR FOR
      select e.entity_id,
             e.name,
             m.p_name,
             m.c_name,
             p.audit_startdate,
             p.audit_enddate,
             p.status as status_id,
             s.status,
             (trunc(sysdate) - p.audit_enddate) as no_of_days
        from t_au_plan_eng p
       inner join t_auditee_entities e
          on e.entity_id = p.auditby_id
       inner join t_auditee_entities_maping m
          on p.entity_id = m.entity_id
       inner join t_au_plan_eng_status s
          on p.status = s.id
       inner join t_auditee_entities_maping fad
          on fad.entity_id = e.entity_id
       where p.status between 4 and 13
         and (p.audit_enddate + 15) < trunc(sysdate)
         AND ENT_ID = CASE
               WHEN R_ID in (6, 7) then
                p.auditby_id
               when R_ID = 5 then
                fad.parent_id
               when R_ID in (1, 3) then
                ENT_ID
               when R_ID = 15 then
                p.auditby_id
             end;
  
  END p_delay_audits;

  PROCEDURE R_FAD_MONTHLY_REVIEW(P_NO      IN NUMBER,
                                 ENT_ID    IN NUMBER,
                                 R_ID      IN NUMBER,
                                 R_T       IN NUMBER,
                                 S_Date    IN DATE,
                                 E_Date    IN DATE,
                                 io_cursor OUT t_cursor) IS
  BEGIN
    IF (R_ID IN (1, 3, 5) AND R_T = -1) THEN
      OPEN io_cursor FOR
        SELECT '-' AS p_name,
               v.autid AS child_code,
               v.entitytypedesc AS c_name,
               v.audit_type,
               SUM(CASE
                     WHEN (TRUNC(v.para_added_on) < S_Date AND
                          TRUNC(v.setteled_on) >= S_Date) OR
                          (TRUNC(v.para_added_on) < S_Date AND v.para_status = 8) THEN
                      1
                     ELSE
                      0
                   END) opening_bal,
               SUM(CASE
                     WHEN TRUNC(v.para_added_on) BETWEEN S_Date AND E_Date THEN
                      1
                     ELSE
                      0
                   END) Para_added,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage IN (41, 45) THEN
                      1
                     ELSE
                      0
                   END) Settled_com,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage NOT IN (41, 45) THEN
                      1
                     ELSE
                      0
                   END) Settled_aud,
               SUM(CASE
                     WHEN ((TRUNC(v.para_added_on) <= E_Date AND
                          TRUNC(v.setteled_on) > E_Date) OR
                          (TRUNC(v.para_added_on) <= E_Date AND
                          v.para_status = 8)) THEN
                      1
                     else
                      0
                   END) Outstanding,
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '1') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '1') OR
                          (v.c_type_id = 28 AND v.risk = '1')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R1,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '2') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '2') OR
                          (v.c_type_id = 28 AND v.risk = '2')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R2,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk IN (0, '3')) OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk IN (0, '3')) OR
                          (v.c_type_id = 28 AND v.risk IN (0, '3'))))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R3
          FROM V_RPT_FAD_MONTHLY_BASE v
         WHERE v.audit_type IN ('B', 'D')
         GROUP BY v.autid, v.entitytypedesc, v.audit_type
         ORDER BY child_code;
    
    ELSIF R_ID IN (6, 7) THEN
      OPEN io_cursor FOR
        SELECT v.p_name,
               v.entity_id AS child_code,
               v.c_name,
               SUM(CASE
                     WHEN (TRUNC(v.para_added_on) < S_Date AND
                          TRUNC(v.setteled_on) >= S_Date) OR
                          (TRUNC(v.para_added_on) < S_Date AND v.para_status = 8) THEN
                      1
                     ELSE
                      0
                   END) opening_bal,
               SUM(CASE
                     WHEN TRUNC(v.para_added_on) BETWEEN S_Date AND E_Date THEN
                      1
                     else
                      0
                   END) Para_added,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage IN (41, 45) THEN
                      1
                     ELSE
                      0
                   END) Settled_com,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage NOT IN (41, 45) THEN
                      1
                     ELSE
                      0
                   END) Settled_aud,
               SUM(CASE
                     WHEN ((TRUNC(v.para_added_on) <= E_Date AND
                          TRUNC(v.setteled_on) > E_Date) OR
                          (TRUNC(v.para_added_on) <= E_Date AND
                          v.para_status = 8)) THEN
                      1
                     ELSE
                      0
                   END) Outstanding,
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '1') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '1') OR
                          (v.c_type_id = 28 AND v.risk = '1')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R1,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '2') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '2') OR
                          (v.c_type_id = 28 AND v.risk = '2')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R2,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk IN (0, '3')) OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk IN (0, '3')) OR
                          (v.c_type_id = 28 AND v.risk IN (0, '3'))))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R3
          FROM V_RPT_FAD_MONTHLY_BASE v
         WHERE v.audited_by = ENT_ID
         GROUP BY v.p_name, v.entity_id, v.c_name
         ORDER BY v.p_name;
    ELSIF R_ID IN (43, 44) THEN
      OPEN io_cursor FOR
        SELECT v.p_name,
               v.entity_id AS child_code,
               v.c_name,
               SUM(CASE
                     WHEN (TRUNC(v.para_added_on) < S_Date AND
                          TRUNC(v.setteled_on) >= S_Date) OR
                          (TRUNC(v.para_added_on) < S_Date AND v.para_status = 8) THEN
                      1
                     ELSE
                      0
                   END) opening_bal,
               SUM(CASE
                     WHEN TRUNC(v.para_added_on) BETWEEN S_Date AND E_Date THEN
                      1
                     ELSE
                      0
                   END) Para_added,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage IN (41, 45) THEN
                      1
                     else
                      0
                   END) Settled_com,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage NOT IN (41, 45) THEN
                      1
                     else
                      0
                   END) Settled_aud,
               SUM(CASE
                     WHEN ((TRUNC(v.para_added_on) <= E_Date AND
                          TRUNC(v.setteled_on) > E_Date) OR
                          (TRUNC(v.para_added_on) <= E_Date AND
                          v.para_status = 8)) THEN
                      1
                     else
                      0
                   END) Outstanding,
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '1') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '1') OR
                          (v.c_type_id = 28 AND v.risk = '1')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R1,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '2') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '2') OR
                          (v.c_type_id = 28 AND v.risk = '2')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R2,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk IN (0, '3')) OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk IN (0, '3')) OR
                          (v.c_type_id = 28 AND v.risk IN (0, '3'))))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R3
          FROM V_RPT_FAD_MONTHLY_BASE v
         inner join t_auditee_entities e
            on e.entity_id = v.entity_id
         inner join v_get_compliance_reviewer_approver c
            on c.COM_KEY = e.complice_by
         WHERE c.ENTITY_ID = ENT_ID
           and v.autid = R_T
         GROUP BY v.p_name, v.entity_id, v.c_name
         ORDER BY v.p_name;
    ELSE
      OPEN io_cursor FOR
        SELECT v.p_name,
               v.entity_id AS child_code,
               v.c_name,
               SUM(CASE
                     WHEN (TRUNC(v.para_added_on) < S_Date AND
                          TRUNC(v.setteled_on) >= S_Date) OR
                          (TRUNC(v.para_added_on) < S_Date AND v.para_status = 8) THEN
                      1
                     ELSE
                      0
                   END) opening_bal,
               SUM(CASE
                     WHEN TRUNC(v.para_added_on) BETWEEN S_Date AND E_Date THEN
                      1
                     ELSE
                      0
                   END) Para_added,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage IN (41, 45) THEN
                      1
                     else
                      0
                   END) Settled_com,
               SUM(CASE
                     WHEN TRUNC(v.setteled_on) BETWEEN S_Date AND E_Date AND
                          v.para_status != 8 AND v.com_stage NOT IN (41, 45) THEN
                      1
                     else
                      0
                   END) Settled_aud,
               SUM(CASE
                     WHEN ((TRUNC(v.para_added_on) <= E_Date AND
                          TRUNC(v.setteled_on) > E_Date) OR
                          (TRUNC(v.para_added_on) <= E_Date AND
                          v.para_status = 8)) THEN
                      1
                     else
                      0
                   END) Outstanding,
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '1') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '1') OR
                          (v.c_type_id = 28 AND v.risk = '1')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R1,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk = '2') OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk = '2') OR
                          (v.c_type_id = 28 AND v.risk = '2')))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R2,
               
               
               SUM(CASE
                     WHEN ((v.audit_type = 'D' AND v.risk IN (0, '3')) OR
                          (v.audit_type = 'B' AND
                          ((v.c_type_id != 28 AND v.annex_risk IN (0, '3')) OR
                          (v.c_type_id = 28 AND v.risk IN (0, '3'))))) AND
                          ((v.para_status = 8 AND
                          TRUNC(v.para_added_on) <= E_Date) OR
                          (TRUNC(v.setteled_on) > E_Date AND
                          TRUNC(v.para_added_on) <= E_Date)) THEN
                      1
                     ELSE
                      0
                   END) R3
          FROM V_RPT_FAD_MONTHLY_BASE v
         WHERE v.c_type_id = (CASE
                 WHEN R_T = -1 THEN
                  v.autid
                 ELSE
                  R_T
               END)
           AND v.audited_by = (CASE
                 WHEN R_ID IN (15, 16) THEN
                  ENT_ID
                 ELSE
                  v.audited_by
               END)
         GROUP BY v.p_name, v.entity_id, v.c_name
         ORDER BY v.p_name;
    END IF;
  END R_FAD_MONTHLY_REVIEW;

  Procedure P_GET_GM_WISE_SERIOUS_PARAS(P_NO      in number,
                                        ENT_ID    in number,
                                        R_ID      in number,
                                        R_T       in number,
                                        S_Date    in date,
                                        E_Date    in date,
                                        io_cursor OUT t_cursor) is
  
  begin
    if (R_ID in (15, 16)) then
      open io_cursor for
      
        SELECT gm.parent_id,
               gm.p_name,
               COUNT(CASE
                       WHEN TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR') THEN
                        c.id
                     END) AS total_before_current_year,
               COUNT(CASE
                       WHEN TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR') THEN
                        c.id
                     END) AS total_in_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      1
                     ELSE
                      0
                   END) AS a1_before_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      1
                     ELSE
                      0
                   END) AS a1_in_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      cast(cast(c.amount_involved as number) / 1000000 as
                           number(10, 3))
                     ELSE
                      0
                   END) AS c_amount_before_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      cast(cast(c.amount_involved as number) / 1000000 as
                           number(10, 3))
                     ELSE
                      0
                   END) AS c_amount_in_current_year,
               sum(case
                     when r.O_PARA_ID = c.old_para_id then
                      r.NO_OF_EMP
                   end) AS per_inv,
               sum(case
                     when r.N_PARA_ID = c.new_paraid then
                      r.NO_OF_EMP
                   end) AS c_per_inv
          FROM t_auditee_entities_maping rg
         INNER JOIN t_auditee_entities_maping gm
            ON gm.entity_id = rg.parent_id
         INNER JOIN t_au_observation_fad c
            ON c.entity_id = rg.entity_id
         inner join V_GET_ANEX_NO_OF_RESPONSIBLE r
            on (r.O_PARA_ID = c.old_para_id or c.new_paraid = r.N_PARA_ID)
         WHERE c.r_id = 1
           AND c.para_status = 8
           and c.audited_by = ENT_ID
         GROUP BY gm.parent_id, gm.p_name;
    else
      open io_cursor for
      
        SELECT gm.parent_id,
               gm.p_name,
               COUNT(CASE
                       WHEN TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR') THEN
                        c.id
                     END) AS total_before_current_year,
               COUNT(CASE
                       WHEN TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR') THEN
                        c.id
                     END) AS total_in_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      1
                     ELSE
                      0
                   END) AS a1_before_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      1
                     ELSE
                      0
                   END) AS a1_in_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      cast(cast(c.amount_involved as number) / 1000000 as
                           number(10, 3))
                     ELSE
                      0
                   END) AS c_amount_before_current_year,
               SUM(CASE
                     WHEN TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR') AND
                          c.annex = 1 THEN
                      cast(cast(c.amount_involved as number) / 1000000 as
                           number(10, 3))
                     ELSE
                      0
                   END) AS c_amount_in_current_year,
               sum(case
                     when r.O_PARA_ID = c.old_para_id then
                      r.NO_OF_EMP
                   end) AS per_inv,
               sum(case
                     when r.N_PARA_ID = c.new_paraid then
                      r.NO_OF_EMP
                   end) AS c_per_inv
          FROM t_auditee_entities_maping rg
         INNER JOIN t_auditee_entities_maping gm
            ON gm.entity_id = rg.parent_id
         INNER JOIN t_au_observation_fad c
            ON c.entity_id = rg.entity_id
         inner join V_GET_ANEX_NO_OF_RESPONSIBLE r
            on (r.O_PARA_ID = c.old_para_id or c.new_paraid = r.N_PARA_ID)
         WHERE c.r_id = 1
           AND c.para_status = 8
         GROUP BY gm.parent_id, gm.p_name
         order by gm.p_name;
    end if;
  
  end P_GET_GM_WISE_SERIOUS_PARAS;

  Procedure P_GET_GM_WISE_SERIOUS_PARAS_DETAILS(PARENT_ENT_ID in number,
                                                IND           in varchar2,
                                                P_ANNEX       in varchar2,
                                                P_NO          in number,
                                                ENT_ID        in number,
                                                R_ID          in number,
                                                
                                                io_cursor OUT t_cursor) is
  
  begin
  
    if (P_ANNEX = 'ALL') then
      if (IND = 'S') then
      
        open io_cursor for
          SELECT rg.p_name,
                 rg.c_name,
                 cp.audit_period,
                 cp.para_no,
                 aa.code           as heading,
                 r.description     as risk,
                 cp.gist_of_paras,
                 c.amount_involved
            FROM t_auditee_entities_maping rg
           INNER JOIN t_auditee_entities_maping gm
              ON gm.entity_id = rg.parent_id
           INNER JOIN t_au_observation_fad c
              ON c.entity_id = rg.entity_id
           inner join ais_t_au_post_compliance cp
              on cp.Old_Para_Id = C.old_para_id
           inner join t_risk r
              on r.r_id = cp.risk
           inner join t_audit_checklist_annexure aa
              on c.annex = aa.id
           inner join V_GET_ANEX_NO_OF_RESPONSIBLE r
              on (r.O_PARA_ID = c.old_para_id or c.new_paraid = r.N_PARA_ID)
           WHERE c.r_id = 1
             AND c.para_status = 8
             and TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR')
             AND gm.parent_id IN (PARENT_ENT_ID)
          ;
      else
        open io_cursor for
          SELECT rg.p_name,
                 rg.c_name,
                 c.audit_period,
                 c.para_no,
                 aa.code           as heading,
                 r.description     as risk,
                 c.gist_of_paras,
                 c.amount_involved
            FROM t_auditee_entities_maping rg
           INNER JOIN t_auditee_entities_maping gm
              ON gm.entity_id = rg.parent_id
           INNER JOIN t_au_observation_fad c
              ON c.entity_id = rg.entity_id
           inner join ais_t_au_post_compliance cp
              on cp.new_para_id = C.new_paraid
           inner join t_risk r
              on r.r_id = cp.risk
           inner join t_audit_checklist_annexure aa
              on c.annex = aa.id
           inner join V_GET_ANEX_NO_OF_RESPONSIBLE r
              on (r.O_PARA_ID = c.old_para_id or c.new_paraid = r.N_PARA_ID)
           WHERE c.r_id = 1
             AND c.para_status = 8
             and TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR')
             AND gm.parent_id IN (PARENT_ENT_ID)
          ;
      end if;
    
    else
      if (IND = 'S') then
      
        open io_cursor for
          SELECT rg.p_name,
                 rg.c_name,
                 cp.audit_period,
                 cp.para_no,
                 aa.code           as heading,
                 r.description     as risk,
                 cp.gist_of_paras,
                 c.amount_involved
            FROM t_auditee_entities_maping rg
           INNER JOIN t_auditee_entities_maping gm
              ON gm.entity_id = rg.parent_id
           INNER JOIN t_au_observation_fad c
              ON c.entity_id = rg.entity_id
           inner join ais_t_au_post_compliance cp
              on cp.Old_Para_Id = C.old_para_id
           inner join t_risk r
              on r.r_id = cp.risk
           inner join t_audit_checklist_annexure aa
              on c.annex = aa.id
           inner join V_GET_ANEX_NO_OF_RESPONSIBLE r
              on (r.O_PARA_ID = c.old_para_id or c.new_paraid = r.N_PARA_ID)
           WHERE c.r_id = 1
             AND c.para_status = 8
             and c.annex = 1
             and TRUNC(c.entereddate) < TRUNC(SYSDATE, 'YEAR')
             AND gm.parent_id IN (PARENT_ENT_ID)
          ;
      else
        open io_cursor for
          SELECT rg.p_name,
                 rg.c_name,
                 cp.audit_period,
                 cp.para_no,
                 aa.code           as heading,
                 r.description     as risk,
                 cp.gist_of_paras,
                 c.amount_involved
            FROM t_auditee_entities_maping rg
           INNER JOIN t_auditee_entities_maping gm
              ON gm.entity_id = rg.parent_id
           INNER JOIN t_au_observation_fad c
              ON c.entity_id = rg.entity_id
           inner join ais_t_au_post_compliance cp
              on cp.new_para_id = C.new_paraid
           inner join t_risk r
              on r.r_id = cp.risk
           inner join t_audit_checklist_annexure aa
              on c.annex = aa.id
           inner join V_GET_ANEX_NO_OF_RESPONSIBLE r
              on (r.O_PARA_ID = c.old_para_id or c.new_paraid = r.N_PARA_ID)
           WHERE c.r_id = 1
             AND c.para_status = 8
             and c.annex = 1
             and TRUNC(c.entereddate) >= TRUNC(SYSDATE, 'YEAR')
             AND gm.parent_id IN (PARENT_ENT_ID)
          ;
      end if;
    
    end if;
  end P_GET_GM_WISE_SERIOUS_PARAS_DETAILS;

  Procedure P_GET_PARA_TEXT_WORDS_V2(T_TEXT    varchar2,
                                     io_cursor OUT t_cursor) is
    v_1 varchar2(1000);
  
  begin
  
    v_1 := ltrim(rtrim(lower(t_text)));
  
    OPEN IO_CURSOR FOR
      SELECT et.name,
             m.p_name,
             m.c_name,
             t.audit_period,
             t.para_no,
             t.gist_of_paras,
             t.code as annex,
             t.text
        FROM VM_GET_ALL_PARA_TEXT t
       INNER JOIN T_AUDITEE_ENTITIES et
          ON et.entity_id = t.audited_by
       INNER JOIN T_AUDITEE_ENTITIES_MAPING m
          ON m.entity_id = t.entity_id
       WHERE CONTAINS(t.text, '%' || v_1 || '%') > 0
            
         AND t.audit_period IN (2022, 2023, 2024)
         AND t.para_status = 8;
  end P_GET_PARA_TEXT_WORDS_V2;

  Procedure P_GET_FAD_DESK_OFFICER_RPT_BY_PERIOD(startDate date,
                                                 endDate   date,
                                                 io_cursor OUT t_cursor) is
  
  begin
  
    OPEN IO_CURSOR FOR
      select c.com_id,
             c.audit_period,
             c.child_code,
             c.c_name,
             c.az,
             c.p_name,
             c.annex,
             c.gist_of_paras,
             c.para_no,
             c.no_of_instances,
             c.risk,
             c.amount,
             c.para_status
      
        from V_FAD_NAUMAN_RPT c
      
       where c.para_added_on between trunc(startDate) and trunc(endDate);
  
  end P_GET_FAD_DESK_OFFICER_RPT_BY_PERIOD;

CREATE OR REPLACE VIEW V_P_GET_SETTLED_PARA_DETAILS AS
SELECT DISTINCT
       m.p_name        AS reporting_office,
       m.c_name        AS entity_name,
       c.audit_period,
       c.com_id,
       c.para_no,
       c.setteled_by   AS settled_by,
       c.setteled_on   AS settled_on,
       r.description   AS risk,
       c.ind           AS para_category,
       c.com_cycle     AS compliance_cycle,
       c.entity_id,
       m.auditedby
  FROM ais_t_au_post_compliance c
 INNER JOIN ais_t_au_post_compliance_history h
    ON h.com_id = c.com_id
 INNER JOIN t_auditee_entities_maping m
    ON m.entity_id = c.entity_id
 INNER JOIN t_risk r
    ON r.rating = c.risk
 WHERE c.setteled_on IS NOT NULL;
/

PROCEDURE P_GET_SETTLED_PARA_ENTITIES
(
    P_NO      IN  NUMBER,
    ENT_ID    IN  NUMBER,
    R_ID      IN  NUMBER,
    io_cursor OUT t_cursor
)
AS
BEGIN
    IF R_ID IN (1, 3, 5, 7, 11) THEN

        OPEN io_cursor FOR
            SELECT DISTINCT
                   e.name,
                   e.entity_id
              FROM V_P_GET_SETTLED_PARA_DETAILS f
             INNER JOIN t_auditee_entities e
                ON e.entity_id = f.auditedby
             WHERE f.settled_on IS NOT NULL;

    ELSE

        OPEN io_cursor FOR
            SELECT DISTINCT
                   e.name,
                   e.entity_id
              FROM V_P_GET_SETTLED_PARA_DETAILS f
             INNER JOIN t_auditee_entities e
                ON e.entity_id = f.entity_id
             INNER JOIN t_auditee_entities_maping_fad fad
                ON fad.entity_id = f.auditedby
             INNER JOIN T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY h
                ON h.entity_id = f.entity_id
             WHERE fad.ppno = P_NO
               AND h.reviewed_by IS NULL;

    END IF;
END P_GET_SETTLED_PARA_ENTITIES;

PROCEDURE P_GET_SETTLED_PARA_DETAILS
(
    P_NO       IN  NUMBER,
    ENT_ID     IN  NUMBER,
    R_ID       IN  NUMBER,
    auditee_id IN  NUMBER,
    io_cursor  OUT t_cursor
)
AS
BEGIN
    OPEN io_cursor FOR
        SELECT d.reporting_office,
               d.entity_name,
               d.audit_period,
               d.para_no,
               d.settled_by,
               d.settled_on,
               d.risk,
               d.para_category,
               d.com_id,
               d.compliance_cycle,
               d.entity_id,
               d.auditedby
          FROM V_P_GET_SETTLED_PARA_DETAILS d
         WHERE d.entity_id = auditee_id
            OR d.auditedby = auditee_id
         ORDER BY d.settled_on;

END P_GET_SETTLED_PARA_DETAILS;

PROCEDURE P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE
(
    P_COM_ID  IN  NUMBER,
    io_cursor OUT t_cursor
)
AS
BEGIN
    OPEN io_cursor FOR
        SELECT h.comment_by_ppno AS attended_by,
               d.description     AS designation,
               emp.employeefirstname || ' ' || emp.employeelastname AS emp_name,
               h.comments        AS remarks,
               f.com_cycle       AS compliance_cycle
          FROM ais_t_au_post_compliance_history h
         INNER JOIN ais_t_au_post_compliance f
            ON f.com_id = h.com_id
         INNER JOIN v_service_employeeinfo emp
            ON emp.ppno = h.comment_by_ppno
         INNER JOIN t_groups d
            ON d.group_id = h.com_stage
         WHERE f.com_id = P_COM_ID
         ORDER BY f.com_cycle, h.comment_on;

END P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE;

PROCEDURE P_GetParasForCompliancehistory
(
    P_COM_ID  IN  NUMBER,
    io_cursor OUT t_cursor
)
AS
BEGIN
    OPEN io_cursor FOR
        SELECT h.hist_id,
               h.com_id,
               h.com_cycle,
               h.com_status,
               h.com_stage,
               NVL(g.description, h.com_stage) AS comment_by_role,
               h.comment_by_ppno AS pp_no,
               NVL(emp.employeefirstname || ' ' || emp.employeelastname, '') AS name,
               '' AS designation,
               h.comment_on,
               h.comments,
               h.com_flow
          FROM ais_t_au_post_compliance_history h
          LEFT JOIN t_groups g
            ON g.group_id = h.com_stage
          LEFT JOIN v_service_employeeinfo emp
            ON emp.ppno = h.comment_by_ppno
         WHERE h.com_id = P_COM_ID
         ORDER BY h.com_cycle, h.comment_on, h.hist_id;
END P_GetParasForCompliancehistory;

PROCEDURE P_GetParasForComplianceforhistory
(
    P_C_CYCLE IN  NUMBER,
    P_COM_ID   IN  NUMBER,
    io_cursor  OUT t_cursor
)
AS
BEGIN
    OPEN io_cursor FOR
        SELECT pct.reply,
               pct.text_id,
               pc.para_no,
               para_text.gist_of_paras,
               para_text.text AS para_text
          FROM ais_t_au_post_compliance pc
          LEFT JOIN ais_t_au_post_compliance_text pct
            ON pct.com_id = pc.com_id
           AND pct.com_cycle = P_C_CYCLE
          LEFT JOIN VM_GET_ALL_PARA_TEXT para_text
            ON para_text.entity_id = pc.entity_id
           AND TO_CHAR(para_text.audit_period) = TO_CHAR(pc.audit_period)
           AND UPPER(TRIM(para_text.para_no)) = UPPER(TRIM(pc.para_no))
         WHERE pc.com_id = P_COM_ID
           AND ROWNUM = 1;
END P_GetParasForComplianceforhistory;



End PKG_RPT;
/

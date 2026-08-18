create or replace package PKG_FAD is
  TYPE t_cursor IS REF CURSOR;

  procedure P_GetRelationTypes(R_ID IN NUMBER, io_cursor OUT t_cursor);

  procedure P_GetReportingOffices(p_relation_id in number,
                                  R_ID          in number,
                                  ENT_ID        IN NUMBER,
                                  io_cursor     OUT t_cursor);

  PROCEDURE P_GetEntitiesForOffice(p_office_id IN NUMBER,
                                   io_cursor   OUT SYS_REFCURSOR);

  procedure P_Get_Auditee_Entities(ENTITYID  IN NUMBER,
                                   io_cursor OUT t_cursor);

  Procedure P_GetAuditEmployees(P_NO      in number,
                                R_ID      in number,
                                ENT_ID    in number,
                                io_cursor out t_cursor);

  procedure P_Get_Auditee_Parent_FAD(ENT_ID    IN NUMBER,
                                     P_NO      in number,
                                     io_cursor OUT t_cursor);

  procedure P_Get_Auditee_Child_FAD(ENT_ID    IN NUMBER,
                                    P_NO      in number,
                                    io_cursor OUT t_cursor);

  procedure P_Get_all_paras_fad(Entityid in number, io_cursor OUT t_cursor);

  procedure P_Update_paras_annex_fad(CAT       in varchar2,
                                     OBS_ID    in number,
                                     refp      in varchar2,
                                     Anex      in number,
                                     P_NO      in number,
                                     R_ID      IN NUMBER,
                                     ENT_ID    IN NUMBER,
                                     io_cursor OUT t_cursor);

  procedure P_Get_Observation(ENTITYID IN NUMBER, io_cursor OUT t_cursor);

  procedure P_get_gist_recommendation(obsid     in number,
                                      io_cursor OUT t_cursor);

  procedure p_get_old_para_fad(ppno in number, io_cursor OUT t_cursor);

  procedure p_get_old_para_AZ(entityid in number, io_cursor OUT t_cursor);

  procedure p_get_legacy_para_responsibles(paraRef   in varchar2,
                                           io_cursor OUT t_cursor);

  procedure p_update_para_text(refid in number, paratext in clob);

  procedure P_get_para_responsibility(refid     in varchar2,
                                      io_cursor OUT t_cursor);

  procedure p_delete_para_responsibility(refp      in varchar2,
                                         refid     in number,
                                         PPNO      in number,
                                         io_cursor OUT t_cursor);

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

  procedure P_GetEntitiesForLegacyPara(PP_NO     in number,
                                       io_cursor out t_cursor);

  procedure P_GetLeagacyObservations(entityId  in number,
                                     paraRef   in varchar2,
                                     ppno      in number,
                                     io_cursor out t_cursor);

  procedure P_update_legacy_Para_text(ref_id       in varchar2,
                                      obtext       in clob,
                                      process_id   in number,
                                      subprocessid in number,
                                      checklistid  in number,
                                      ppno         in number,
                                      risk_id      in number,
                                      io_cursor    OUT t_cursor);

  procedure P_reviewed_legacy_Para(ref_id    in varchar2,
                                   ppno      in number,
                                   io_cursor OUT t_cursor);

  procedure P_referback_legacy_Para(ref_id    in varchar2,
                                    ppno      in number,
                                    io_cursor OUT t_cursor);

  procedure P_GetOldParasForResponseAuthorize(ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor);

  procedure P_GetnewParasForResponseAuthorize(UserEntityId in number,
                                              io_cursor    OUT t_cursor);

  procedure P_AuthorizeChangeStatusRequestForSettledPara(RefP       in varchar2,
                                                         au_obs_id  in number,
                                                         P_IND      in varchar2,
                                                         Action_IND in varchar2,
                                                         ENT_ID     in number,
                                                         P_NO       in number,
                                                         R_ID       in number,
                                                         io_cursor  OUT t_cursor);

  procedure P_AuthorizeChangeStatusRequestForSettledPara_new(obsid     in number,
                                                             P_IND     in varchar2,
                                                             remark    in varchar2,
                                                             indicator in varchar2,
                                                             ENT_ID    in number,
                                                             P_NO      in number,
                                                             R_ID      in number,
                                                             io_cursor OUT t_cursor);

  procedure p_get_violations(io_cursor OUT t_cursor);

  Procedure p_get_process_owner(ENT_ID    in number,
                                P_NO      in number,
                                R_ID      in number,
                                io_cursor OUT t_cursor);

  Procedure p_get_role_responsible(ENT_ID    in number,
                                   P_NO      in number,
                                   R_ID      in number,
                                   io_cursor OUT t_cursor);

  procedure p_GetChecklistSubByProcessId(processId in number,
                                         io_cursor OUT t_cursor);

  procedure p_GetChecklistDetailBySubProcessId(subProcessId in number,
                                               io_cursor    OUT t_cursor);

  procedure p_update_sub_process(sid       in number,
                                 p_id      in number,
                                 sub_name  in varchar2,
                                 io_cursor OUT t_cursor);

  procedure p_update_process_details(d_id        in number,
                                     sid         in number,
                                     sub_name    in varchar2,
                                     vid         in number,
                                     r_id        in number,
                                     owner_id    in number,
                                     p_entity_id in number,
                                     active      in varchar2,
                                     io_cursor   OUT t_cursor);

  procedure P_GET_SETTLED_PARA_ENTITIES(P_NO      in number,
                                        ENT_ID    in number,
                                        R_ID      in number,
                                        io_cursor out t_cursor);

  Procedure P_GET_SETTLED_PARA_DETAILS(P_NO       in number,
                                       ENT_ID     in number,
                                       R_ID       in number,
                                       auditee_id in number,
                                       io_cursor  OUT t_cursor);

  Procedure P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE(refp      varchar2,
                                                       obs_id    in number,
                                                       io_cursor OUT t_cursor);

  Procedure P_get_auditee_reporting_fad(ENT_ID    in number,
                                        Type_id   in number,
                                        io_cursor OUT t_cursor);

  Procedure P_ADD_auditee_reporting_fad(AZ_ID     in number,
                                        Reg_ID    in number,
                                        ENT_ID    in number,
                                        TYP_ID    in number,
                                        TR        in number,
                                        io_cursor OUT t_cursor);

  Procedure P_UPDATE_auditee_reporting_fad(update_id in number,
                                           AZ_ID     in number,
                                           Reg_ID    in number,
                                           ENT_ID    in number,
                                           TYP_ID    in number,
                                           TR        in number,
                                           io_cursor OUT t_cursor);

  Procedure P_GET_SAMPLE(io_cursor OUT t_cursor);

  Procedure P_UPDATE_SAMPLE(s_id      NUMBER,
                            s_per     NUMBER,
                            io_cursor OUT t_cursor);

  procedure p_get_audit_engagement(ent_id    in number,
                                   io_cursor OUT t_cursor);

  procedure p_get_audit_observtion(OB_ID in number, io_cursor OUT t_cursor);

  procedure p_get_audit_glance(ENGID in number, io_cursor OUT t_cursor);

  procedure p_get_audit_Report(ENGID     in number,
                               RPT_ID    in number,
                               io_cursor OUT t_cursor);

  PROCEDURE P_PARA_SHIFTING(NEW_ENT_ID IN NUMBER,
                            OLD_ENT_ID IN NUMBER,
                            O_ID       IN NUMBER,
                            P_IND      IN VARCHAR2,
                            P_NO       IN NUMBER,
                            ENT_ID     IN NUMBER,
                            R_ID       IN NUMBER,
                            io_cursor  OUT t_cursor);

  PROCEDURE P_GetAuditChecklistAnnexureCirculars(io_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_InsertCircularDoc(p_circular_id IN NUMBER,
                                p_file_name   IN VARCHAR2,
                                p_file_type   IN VARCHAR2,
                                p_file_size   IN NUMBER,
                                p_file_blob   IN BLOB,
                                p_uploaded_by IN VARCHAR2,
                                o_status      OUT VARCHAR2);

  -- Allocate an entity to an auditor within an Audit Zone
  PROCEDURE p_allocate_entity_to_auditor(p_az_id        IN NUMBER,
                                         p_ent_id       IN NUMBER,
                                         p_auditor_ppno IN NUMBER,
                                         p_assigned_by  IN NUMBER,
                                         io_cursor      OUT t_cursor);

  -- Approve allocation
  PROCEDURE p_approve_allocation(p_allocation_id IN NUMBER,
                                 p_approved_by   IN NUMBER,
                                 io_cursor       OUT t_cursor);

  PROCEDURE P_GetParaText(p_com_id IN NUMBER, io_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_GetParaReferences(p_com_id  IN NUMBER,
                                io_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_ManageReference(p_action            IN VARCHAR2, -- 'ADD', 'UPDATE', 'DELETE'
                              p_link_id           IN NUMBER DEFAULT NULL,
                              p_entity_id         IN NUMBER DEFAULT NULL,
                              p_old_para_id       IN NUMBER DEFAULT NULL,
                              p_new_para_id       IN NUMBER DEFAULT NULL,
                              p_para_id           IN NUMBER DEFAULT NULL,
                              p_ref_id            IN NUMBER DEFAULT NULL,
                              p_ref_title         IN VARCHAR2 DEFAULT NULL,
                              p_credit_manual_id  IN NUMBER DEFAULT NULL,
                              p_op_manual_id      IN NUMBER DEFAULT NULL,
                              p_manual_type       IN VARCHAR2 DEFAULT NULL,
                              p_chapter           IN VARCHAR2 DEFAULT NULL,
                              p_instructions_date in date,
                              p_matched_text      IN VARCHAR2 DEFAULT NULL,
                              p_link_type         IN VARCHAR2 DEFAULT NULL,
                              p_new_ref           IN NUMBER DEFAULT NULL,
                              p_user              IN VARCHAR2 DEFAULT NULL,
                              io_cursor           OUT t_cursor);

  -- Update reference of an observation and log the change
  PROCEDURE p_update_para_reference(p_com_id     IN NUMBER,
                                    p_new_ref    IN NUMBER,
                                    p_updated_by IN NUMBER,
                                    io_cursor    OUT t_cursor);

  -- Get all logs for a particular observation
  PROCEDURE p_get_update_log(p_com_id IN NUMBER, io_cursor OUT t_cursor);

  PROCEDURE P_GetObservationsForReferenceUpdate(p_ent_id           IN NUMBER,
                                                p_assigned_auditor IN NUMBER,
                                                p_reference_id     IN NUMBER,
                                                io_cursor          OUT SYS_REFCURSOR);

  PROCEDURE P_GetEntityTaskSummary(p_auditor_ppno IN NUMBER,
                                   io_cursor      OUT SYS_REFCURSOR);

  PROCEDURE P_GetPendingParas(p_entity_id  IN NUMBER,
                              p_audit_year IN varchar2,
                              io_cursor    OUT SYS_REFCURSOR);

  PROCEDURE P_SearchReferences(p_ref_type IN VARCHAR2,
                               p_keyword  IN VARCHAR2,
                               io_cursor  OUT SYS_REFCURSOR);

  -- Fetch paras by entity and status
  PROCEDURE P_GET_PARA_STATUS_REQUEST(P_ENTITY_ID IN NUMBER,
                                      P_STATUS    IN NUMBER,
                                      IO_CURSOR   OUT SYS_REFCURSOR);

  -- Maker adds status change request
  PROCEDURE P_ADD_PARA_STATUS_CHANGE(P_COM_ID        IN NUMBER,
                                     P_NEW_STATUS    IN NUMBER,
                                     P_MAKER_REMARKS IN VARCHAR2,
                                     P_USER_ID       IN NUMBER,
                                     IO_CURSOR       OUT SYS_REFCURSOR);

  -- Get all pending authorization requests
  PROCEDURE P_GET_PARA_STATUS_AUTHORIZATION(IO_CURSOR OUT SYS_REFCURSOR);

  -- Authorize or reject request
  PROCEDURE P_AUTHORIZE_PARA_STATUS_CHANGE(P_LOG_ID       IN NUMBER,
                                           P_ACTION       IN VARCHAR2, -- 'A' or 'R'
                                           P_AUTH_BY      IN NUMBER,
                                           P_AUTH_REMARKS IN VARCHAR2,
                                           IO_CURSOR      OUT SYS_REFCURSOR);

  PROCEDURE P_GET_IAS_PARA_TEXT(P_COM_ID  IN NUMBER,
                                IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_GET_REFERENCE_MASTER_DETAIL(p_search_text           IN VARCHAR2 DEFAULT NULL,
                                          p_reference_source_type IN VARCHAR2 DEFAULT NULL,
                                          p_ref_id                IN NUMBER DEFAULT NULL,
                                          o_cursor                OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MANUAL_MASTER(o_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MANUAL_SECTIONS(p_manual_id IN NUMBER,
                                  o_cursor    OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MANUAL_CHAPTERS(p_manual_id    IN NUMBER,
                                  p_section_text IN VARCHAR2,
                                  o_cursor       OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MANUAL_REFERENCE_GRID(p_manual_id    IN NUMBER,
                                        p_section_text IN VARCHAR2,
                                        p_chapter_no   IN VARCHAR2,
                                        o_cursor       OUT SYS_REFCURSOR);

  PROCEDURE P_GET_REFERENCE_DETAIL_BY_ID(p_ref_id IN NUMBER,
                                         o_cursor OUT SYS_REFCURSOR);

  PROCEDURE P_GET_FAD_ANNEXURE_CONFIG(IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_UPDATE_FAD_ANNEXURE_STATUS(P_ANNEXURE_ID      IN NUMBER,
                                         P_SHIFT_APPLICABLE IN VARCHAR2,
                                         P_NO               IN NUMBER,
                                         IO_CURSOR          OUT T_CURSOR);

end PKG_FAD;
/
create or replace package body PKG_FAD is

  procedure P_GetRelationTypes(R_ID IN NUMBER, io_cursor OUT t_cursor) is
  begin
    if (R_ID IN (15, 16, 17)) then
      open io_cursor for
        select f.entity_realtion_id as id,
               f.parent_name || '   TO   ' || f.chlid_name as name
          from t_auditee_ent_relation f
         where f.status = 'Y'
           and f.id is not null
           and f.id in (5, 9)
         order by f.id;
    else
      if (R_ID = 35) then
        open io_cursor for
          select f.entity_realtion_id,
                 f.parent_name || '   TO   ' || f.chlid_name as field_name
            from t_auditee_ent_relation f
           where f.status = 'Y'
             and f.id is not null
             and f.id in (1, 2)
           order by f.id;
      else
        if (R_ID in (1, 3)) then
          open io_cursor for
            select f.entity_realtion_id,
                   f.parent_name || '   TO   ' || f.chlid_name as field_name
              from t_auditee_ent_relation f
             where f.status = 'Y'
               and f.id is not null
            --and f.id in (1,2)
             order by f.id;
        end if;
      end if;
    end if;
  end P_GetRelationTypes;

  procedure P_GetReportingOffices(p_relation_id in number,
                                  R_ID          in number,
                                  ENT_ID        IN NUMBER,
                                  io_cursor     OUT t_cursor) is
  
  begin
    if (p_relation_id = 5) then
      open io_cursor for
        select Distinct (r.p_name) as Name,
                        r.parent_id as ID,
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
           and r.relation_type_id = p_relation_id
           and r.parent_id is not null
           and et.entity_id = r.parent_id
           and et.auditby_id = ENT_ID
         order by r.p_name;
    else
      if (p_relation_id = 4) then
        open io_cursor for
          select Distinct (r.p_name) as Name,
                          r.parent_id as ID,
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
             and et.auditby_id = ENT_ID
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
          
           where r.relation_type_id = p_relation_id
             and r.parent_id is not null
          --   and et.auditby_id = USER_ENTITY_ID
           order by r.p_name;
      end if;
    end if;
  
  end P_GetReportingOffices;

  PROCEDURE P_GetEntitiesForOffice(p_office_id IN NUMBER,
                                   io_cursor   OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT e.ENTITY_ID,
             e.child_code as ENTITY_CODE,
             e.c_name as NAME,
             e.c_type_id as TYPE,
             e.div_office as DIVISION_ENT_ID,
             (select em.employeefirstname || em.employeelastname as name
                from t_au_tbl_allocations l
               inner join t_audit_emp em
                  on em.ppno = l.auditor_ppno
               where l.ent_id = e.entity_id) as ALLOCATEDTO,
             (SELECT COUNT(*)
                FROM ais_t_au_post_compliance o
               WHERE o.entity_id = e.entity_id) AS TOTAL_PARAS
        FROM t_auditee_entities_maping e
       WHERE e.parent_id = p_office_id;
  END P_GetEntitiesForOffice;

  procedure P_Get_Auditee_Entities(ENTITYID  IN NUMBER,
                                   io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      Select E.ENTITY_ID, E.NAME
        FROM t_auditee_entities e
       inner join t_auditee_entities_maping m
          on m.entity_id = e.auditby_id
       inner join T_AUDITEE_ENTITIES_MAPING_FAD mp
          on mp.entity_id = m.entity_id
      
       WHERE m.parent_id = ENTITYID;
  
  end P_Get_Auditee_Entities;

  Procedure P_GetAuditEmployees(P_NO      in number,
                                R_ID      in number,
                                ENT_ID    in number,
                                io_cursor out t_cursor) as
  
  begin
    open io_cursor for
    
      select e.ppno,
             e.ppno as id,
             e.employeefirstname,
             e.employeelastname,
             e.employeefirstname || e.employeelastname as name,
             e.DEPARTMENTCODE,
             e.deptarment as DEPTARMENT,
             (Select NVL(max(count(r.ent_id)), 0)
                from t_au_tbl_allocations r
               where r.auditor_ppno = e.ppno
               group by r.ent_id) as TASK_ALLOCATED,
             e.RANKCODE,
             e.current_rank,
             e.DESIGNATIONCODE,
             e.fun_designation,
             e.type,
             e.entity_id,
             '' as QUALIFICATION,
             '' as SPECIALIZATION,
             '' as CERTIFICATION,
             '' as TOTAL_EXPERIENCE,
             '' as AUDIT_EXPERIENCE
      
        from t_audit_emp e
      
       where e.entity_id = ENT_ID
       order by e.rankcode
      
      ;
  end P_GetAuditEmployees;

  procedure P_Get_Auditee_Parent_FAD(ENT_ID    IN NUMBER,
                                     P_NO      in number,
                                     io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      Select distinct (Ee.ENTITY_ID), Ee.NAME
        FROM t_auditee_entities e
       inner join T_AUDITEE_ENTITIES_MAPING_FAD mp
          on mp.entity_id = e.auditby_id
       inner join t_auditee_entities_maping m
          on m.entity_id = e.entity_id
       inner join t_auditee_entities ee
          on m.parent_id = ee.entity_id
       WHERE mp.ppno = P_NO
         and exists (select 'z'
                from V_all_outastanding_paras_fad f
               where m.entity_id = e.entity_id
                 and f.annex is null);
    /*and f.entereddate < '01-Jan'||(EXTRACT(YEAR FROM SYSDATE)))
    or exists (select 'z'
             from V_all_outastanding_paras_fad f
            where m.entity_id = e.entity_id
            and  f.reviewed_by is null
              and f.entereddate > '01-Jan'||(EXTRACT(YEAR FROM SYSDATE))));*/
  
  end P_Get_Auditee_Parent_FAD;

  procedure P_Get_Auditee_Child_FAD(ENT_ID    IN NUMBER,
                                    P_NO      in number,
                                    io_cursor OUT t_cursor) as
  begin
    OPEN io_cursor FOR
      Select distinct (e.ENTITY_ID), e.NAME
        FROM t_auditee_entities e
       inner join T_AUDITEE_ENTITIES_MAPING_FAD mp
          on mp.entity_id = e.auditby_id
        left join t_auditee_entities_maping m
          on m.entity_id = e.entity_id
       inner join V_all_outastanding_paras_fad f
          on f.entity_id = e.entity_id
         and f.annex is null
       WHERE m.parent_id = ENT_ID
         and mp.ppno = P_NO;
    /*                 and not exists (select 'z'
     from V_all_outastanding_paras_fad f
    where m.entity_id = e.entity_id
      and f.annex is null);*/
    --and f.entereddate < '01-Jan'||(EXTRACT(YEAR FROM SYSDATE)))
    /*or exists (select 'z'
     from V_all_outastanding_paras_fad f
    where m.entity_id = e.entity_id
    and  f.reviewed_by is null
      and f.entereddate > '01-Jan'||(EXTRACT(YEAR FROM SYSDATE))));*/
  
  end P_Get_Auditee_Child_FAD;

  procedure P_Get_all_paras_fad(Entityid in number, io_cursor OUT t_cursor) is
  begin
  
    open io_cursor for
      select n.code          as annex_id,
             n.id,
             f.entity_id,
             f.ref_p,
             f.obs_id,
             n.heading       as name,
             f.audit_period,
             f.para_no,
             f.gist_of_paras,
             f.para_category
        from V_all_outastanding_paras_fad f
        left join t_audit_checklist_annexure n
          on n.id = f.annex
       WHERE f.entity_id = Entityid
         and (f.annex is null)
      -- AND f.entereddate < '01-Jan'||(EXTRACT(YEAR FROM SYSDATE)))
      --or ( f.reviewed_by is null
      --and f.entereddate > '01-Jan'||(EXTRACT(YEAR FROM SYSDATE))))
       order by f.audit_period, f.obs_id;
  
  end P_Get_all_paras_fad;

  procedure P_Update_paras_annex_fad(CAT       in varchar2,
                                     OBS_ID    in number,
                                     refp      in varchar2,
                                     Anex      in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     ENT_ID    in number,
                                     io_cursor OUT t_cursor) is
  begin
  
    if (CAT = 'O') then
      update t_au_old_paras_fad f set f.annex = anex where f.ref_p = refp;
      commit;
      open io_cursor for
        select 'Para Updated' as remarks from dual;
    else
      if (CAT = 'N') then
        update t_au_observation o
           set o.annex       = anex,
               o.reviewed_on = sysdate,
               o.reviewed_by = P_NO
         where o.id = obs_id;
        commit;
        open io_cursor for
          select 'Para Updated' as remarks from dual;
      end if;
    end if;
  
  end P_Update_paras_annex_fad;

  procedure P_Get_Observation(ENTITYID IN NUMBER, io_cursor OUT t_cursor) is
  
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
       Where e.entity_id = ENTITYID
         and p.status_id = 2
         and o.status not in (27)
       order by o.memo_number;
  
  end P_Get_Observation;

  procedure P_get_gist_recommendation(obsid     in number,
                                      io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select Gt.Obs_Id, gt.gist, r.recommendation
        from t_au_observation_gist gt
       inner join t_au_observation_final_reccomendation r
          on r.obs_id = gt.obs_id
       where gt.obs_id = obsid;
  end P_get_gist_recommendation;

  procedure p_get_old_para_fad(ppno in number, io_cursor OUT t_cursor) as
  
  begin
    open io_cursor for
      select f.ref_p, t.para_text, p.pp_no
        from t_au_old_paras_fad f
       inner join t_au_old_paras_fad_text t
          on f.ref_p = t.ref_p
        left join t_au_observation_old_paras_responibility_assigned p
          on p.ref_p = f.ref_p
       inner join T_AUDITEE_ENTITIES_MAPING_FAD mp
          on mp.entity_id = f.audited_by
       WHERE mp.ppno = PPNO;
  end p_get_old_para_fad;

  procedure p_get_old_para_AZ(entityid in number, io_cursor OUT t_cursor) as
  
  begin
    open io_cursor for
      select f.ref_p, t.para_text, p.pp_no
        from t_au_old_paras_fad f
       inner join t_au_old_paras_fad_text t
          on f.ref_p = t.ref_p
        left join t_au_observation_old_paras_responibility_assigned p
          on p.ref_p = f.ref_p
       WHERE f.audited_by = entityid;
  end p_get_old_para_AZ;

  procedure p_get_legacy_para_responsibles(paraRef   in varchar2,
                                           io_cursor OUT t_cursor) as
  
  begin
    open io_cursor for
      select f.*,
             e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name
        from t_au_old_paras_fad_responsibility_assigned f
       inner join v_service_employeeinfo e
          on e.PPNO = f.pp_no
       WHERE f.ref_p = paraRef
         and f.is_active = 'Y';
  end p_get_legacy_para_responsibles;

  procedure p_update_para_text(refid in number, paratext in clob) as
  
  begin
    update t_au_old_paras_fad_text f
       set f.para_text = paratext
     where f.ref_p = refid;
    commit;
  end p_update_para_text;

  procedure P_get_para_responsibility(refid     in varchar2,
                                      io_cursor OUT t_cursor) as
  
  begin
    open io_cursor for
    
      select p.pp_no,
             e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
             p.loan_case as LOANCASE,
             p.lc_amount as LCAMOUNT,
             p.ac_amount as ACCNUMBER,
             p.ac_amount as ACAMOUNT
        from t_au_old_paras_fad_responsibility_assigned p
       inner join v_service_employeeinfo e
          on e.PPNO = p.pp_no
       where p.ref_p = refid
         and p.is_active = 'Y';
  
  end P_get_para_responsibility;

  procedure p_delete_para_responsibility(refp      in varchar2,
                                         refid     in number,
                                         PPNO      in number,
                                         io_cursor OUT t_cursor) as
  
  begin
    update t_au_old_paras_fad_responsibility_assigned r
       set r.is_active = 'N'
     where r.obs_id = refid
       and r.ref_p = refp
       and r.pp_no = ppno;
    commit;
    open io_cursor for
      select 'Responsibility of ' || PPNO || ' Deleted' as remarks
        from dual;
  
  end p_delete_para_responsibility;

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
  
    insert into t_au_old_paras_fad_responsibility_assigned
      (id,
       obs_id,
       assignedby,
       pp_no,
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
       AZ_Entity_id,
       ppno,
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
  end p_add_para_responsibility;

  procedure P_GetEntitiesForLegacyPara(PP_NO     in number,
                                       io_cursor out t_cursor) as
  begin
    open io_cursor for
      select distinct e.name, e.entity_id
        from t_au_old_paras_fad f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       inner join t_auditee_entities_maping_fad fad
          on fad.entity_id = F.AUDITED_BY
       where f.update_status = 2
         and fad.ppno = PP_NO
       order by e.name;
  end P_GetEntitiesForLegacyPara;

  procedure P_GetLeagacyObservations(entityId  in number,
                                     paraRef   in varchar2,
                                     ppno      in number,
                                     io_cursor out t_cursor) as
  begin
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
                 f.amount_involved,
                 F.RISK
            from t_au_old_paras_fad f
           inner join t_auditee_entities e
              on e.entity_id = f.entity_id
           inner join t_auditee_entities_maping_fad fp
              on fp.entity_id = f.audited_by
           where fp.ppno = ppno
             and f.entity_id = entityId
             and f.update_status = 2
           order by f.audit_period, f.para_no;
      
      else
        open io_cursor for
          select f.*, n.description as nature, pt.para_text
            from t_au_old_paras_fad f
           inner join t_auditee_entities e
              on e.entity_id = f.entity_id
           inner join t_au_old_paras_fad_text pt
              on f.ref_p = pt.ref_p
           inner join t_auditee_entities_maping_fad fp
              on fp.entity_id = f.audited_by
           inner join t_au_old_audit_nature n
              on f.nature_of_audit = n.nid
           where fp.ppno = ppno
             and f.ref_p = paraRef
             and f.entity_id = entityId
             and f.update_status = 2
           order by f.audit_period, f.para_no;
      
        INSERT INTO T_AU_DATA_VALIDATION_FAD_LOG
          (ID, REF_P, PARA_REVIEWED, DESK_OFFICER, FAD_DATE, REMARKS)
        VALUES
          ((SELECT COALESCE(max(u.Id) + 1, 1)
             FROM T_AU_DATA_VALIDATION_FAD_LOG U),
           paraRef,
           '1',
           PPNO,
           sysdate,
           'Para has been Viewed');
        COMMIT;
      end if;
    end if;
  end P_GetLeagacyObservations;

  procedure P_update_legacy_Para_text(ref_id       in varchar2,
                                      obtext       in clob,
                                      process_id   in number,
                                      subprocessid in number,
                                      checklistid  in number,
                                      ppno         in number,
                                      risk_id      in number,
                                      io_cursor    OUT t_cursor) is
  
  begin
    if (risk_id < 4) then
      if (ppno is not null) then
        if (subprocessid = 0) then
          update t_au_old_paras_fad_text ot
             set ot.para_text = obtext
           where ot.ref_p = ref_id;
          commit;
          update t_au_old_paras_fad o
             set o.az_status_updated_by = ppno, o.update_status = 4
           where o.ref_p = ref_id;
          commit;
          UPDATE T_AU_DATA_VALIDATION_FAD_LOG LG
             SET lg.para_final   = 1,
                 LG.DESK_OFFICER = PPNO,
                 lg.remarks      = 'Para has been Updated with changes'
           WHERE LG.REF_P = REF_ID;
          COMMIT;
        else
          update t_au_old_paras_fad o
             set o.process_detail  = checklistid,
                 o.fad_reviewed_by = ppno,
                 o.update_status   = 4,
                 o.fad_reviewed_on = sysdate,
                 o.fad_status      = 4,
                 o.risk            = risk_id
           where o.ref_p = ref_id;
          commit;
          update t_au_old_paras_fad_text ot
             set ot.para_text = obtext
           where ot.ref_p = ref_id;
          commit;
          UPDATE T_AU_DATA_VALIDATION_FAD_LOG LG
             SET lg.para_final   = 1,
                 LG.DESK_OFFICER = PPNO,
                 lg.remarks      = 'Para has been Updated with changes'
           WHERE LG.REF_P = REF_ID;
          COMMIT;
        end if;
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
        select 'System Issue, Please Contact Ali Asif' as remarks
          from dual;
    end if;
  end P_update_legacy_Para_text;

  procedure P_reviewed_legacy_Para(ref_id    in varchar2,
                                   ppno      in number,
                                   io_cursor OUT t_cursor) is
  begin
    if (ppno is not null) then
      update t_au_old_paras_fad o
         set o.fad_reviewed_by = ppno,
             o.update_status   = 4,
             o.fad_reviewed_on = sysdate,
             o.fad_status      = 4
       where o.ref_p = ref_id;
      commit;
      UPDATE T_AU_DATA_VALIDATION_FAD_LOG LG
         SET lg.PARA_FINAL_NO_UPDATE = 1,
             LG.DESK_OFFICER         = PPNO,
             lg.remarks              = 'Para has been Updated without any changes'
       WHERE LG.REF_P = REF_ID;
      COMMIT;
    
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

  procedure P_referback_legacy_Para(ref_id    in varchar2,
                                    ppno      in number,
                                    io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select 'This option has been revoked due to technical issues, Please email the short comming/desired information
        from Audit Zone' as remarks
        from dual;
    /*if (ppno is not null) then
        update t_au_old_paras_fad o
           set o.fad_reviewed_by = ppno,
               o.update_status   = 1,
               o.fad_reviewed_on = sysdate,
               o.fad_status      = 1
         where o.ref_p = ref_id;
        commit;
           UPDATE T_AU_DATA_VALIDATION_FAD_LOG LG
             SET lg.para_refered_back = 1,
                 LG.DESK_OFFICER = PPNO,
                 lg.remarks        = 'Para has been refered back'
           WHERE LG.REF_P = REF_ID ;
          COMMIT;
        open io_cursor for
          select r.ref,
                 'Para has been refered back, email to the concern Incharge Audit about the short coming' as remarks
            from t_au_remarks r
           where r.id = 29;
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
    */
  end P_referback_legacy_Para;

  procedure P_GetOldParasForResponseAuthorize(ENT_ID    in number,
                                              P_NO      in number,
                                              R_ID      in number,
                                              io_cursor OUT t_cursor) is
  
  begin
    if (ENT_ID in (112248, 112242)) then
      open io_cursor for
        SELECT f.para_id,
               '' as ref_p,
               f.para_id as au_obs_id,
               f.entity_name,
               f.period as audit_period,
               f.entity_id,
               f.para_no,
               f.gist_of_paras,
               '' as annexure,
               '' as amount_involved,
               '' as vol_i_ii,
               (case
                 when f.para_status = 8 then
                  'Un-settle'
                 else
                  'settle'
               end) as para_status,
               (case
                 when lg.new_status = 8 then
                  'Un-settle'
                 else
                  'settle'
               end) as temp_status_for_change,
               lg.remarks,
               'C' as IND
          FROM t_au_observation_old_cad_paras f
         inner join T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG lg
            on f.para_id = lg.obs_id
         WHERE lg.authorized_by is null
           and f.audited_by = ENT_ID;
    else
      open io_cursor for
        SELECT f.ref_p,
               null as para_id,
               null as au_obs_id,
               f.entity_name,
               f.audit_period,
               f.entity_id,
               f.para_no,
               f.gist_of_paras,
               f.annexure,
               f.amount_involved,
               f.vol_i_ii,
               (case
                 when f.para_status = 8 then
                  'Un-settle'
                 else
                  'settle'
               end) as para_status,
               (case
                 when lg.new_status = 8 then
                  'Un-settle'
                 else
                  'settle'
               end) as temp_status_for_change,
               lg.remarks,
               'O' as IND
          FROM t_au_old_paras_fad f
          left join t_audit_checklist_details csb
            on csb.id = f.process_detail
         inner join T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG lg
            on lg.ref_p = f.ref_p
         inner join t_auditee_entities_maping_fad mp
            on mp.entity_id = f.audited_by
         WHERE LG.ID = (SELECT MAX(L.ID)
                          FROM T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG L
                         WHERE L.REF_P = LG.REF_P)
           AND lG.authorized_by is null
           and f.audited_by = ENT_ID;
      --and mp.ppno = p_no;
    end if;
  
  end P_GetOldParasForResponseAuthorize;

  procedure P_GetnewParasForResponseAuthorize(UserEntityId in number,
                                              io_cursor    OUT t_cursor) is
  begin
  
    open io_cursor for
      SELECT o.id,
             e.entity_id,
             eg.entity_code,
             p.description as audit_period,
             e.name as entity_name,
             o.memo_number as para_no,
             nvl(tx.headings, 'Gist not Entered') as gist_of_para,
             o.amount_involved,
             lg.remarks,
             r.description as para_risk,
             (case
               when o.status = 8 then
                'Un-settle'
               else
                'settle'
             end) as reviewer_comments,
             (case
               when o.status = 8 then
                'Un-settle'
               else
                'settle'
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
        join t_risk r
          on r.r_id = o.severity
        left join t_auditee_entities_maping_fad mp
          on mp.entity_id = eg.auditby_id
       WHERE e.auditby_id = UserEntityId
         and lg.authorized_on is null;
  
  end P_GetnewParasForResponseAuthorize;

  procedure P_AuthorizeChangeStatusRequestForSettledPara(RefP       in varchar2,
                                                         au_obs_id  in number,
                                                         P_IND      in varchar2,
                                                         Action_IND in varchar2,
                                                         ENT_ID     in number,
                                                         P_NO       in number,
                                                         R_ID       in number,
                                                         io_cursor  OUT t_cursor) as
    p_f   number := 0;
    D_F   number := 0;
    IND   varchar2(2); -- Para Indicatior from AIS_COMP
    N_F   number := 0;
    P_N   varchar2(100); -- Adjusted size for safety
    CM_ID number := 0;
    ENT_T number := 0;
  begin
  
    -- Get latest ID for the para reference
    select nvl(max(fd.id), 0)
      into N_F
      from t_au_old_paras_fad fd
     where fd.ref_p = RefP;
  
    SELECT NVL(c.ind, ''), NVL(c.com_id, 0), NVL(c.entity_type_id, 0)
      INTO IND, CM_ID, ENT_T
      FROM ais_t_au_post_compliance c
     where (c.old_para_id = N_F or c.new_para_id = au_obs_id);
  
    -- Get para number (if exists)
    begin
      select cp.para_no
        into P_N
        from ais_t_au_post_compliance cp
       where cp.old_para_id = N_F;
    exception
      when no_data_found then
        P_N := 'N/A';
    end;
  
    -- Log the authorization action
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       169,
                       'Authorize the Status Change of Legacy Paras of ' ||
                       au_obs_id || RefP || ' is ' || Action_IND);
  
    -- For 'O' (Old/Other) Action_IND
    if (Action_IND = 'A' and IND = 'O') then
    
      -- Get current temp_status_for_change
      select NVL(max(fd.temp_status_for_change), 0)
        into p_f
        from t_au_old_paras_fad fd
       where fd.ref_p = RefP;
    
      -- Get max id for para reference
      select NVL(MAX(fr.id), 0)
        into D_F
        from t_au_old_paras_fad fr
       where fr.ref_p = RefP;
    
      -- Mark status change log as authorized
      update T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG t
         set t.authorized_on       = sysdate,
             t.authorized_by       = P_NO,
             t.status              = 'A',
             t.authorized_comments = 'Request for Status Change is Authorized'
       where t.ref_p = RefP;
    
      -- Update para status depending on temp_status_for_change value
      if (p_f = 8) then
        UPDATE T_AU_OLD_PARAS_FAD al
           SET al.Para_Status = 8, al.update_status = 1
         WHERE al.ref_p = RefP;
      elsif (p_f = 6) then
        UPDATE T_AU_OLD_PARAS_FAD al
           SET al.Para_Status    = 6,
               al.update_status  = 0,
               al.parasetteledon = sysdate,
               al.settled_by     = P_NO
         WHERE al.ref_p = RefP;
      end if;
    
      -- Clear temp status for change
      UPDATE T_AU_OLD_PARAS_FAD al
         SET al.temp_status_for_change = null
       WHERE al.ref_p = RefP;
    
      commit;
    
      open io_cursor for
        Select RefP || ' Para has been Marked' as remark from dual;
    
      -- For 'C' (CAD) Action_IND
    elsif (Action_IND = 'A' and IND = 'C') then
      select max(lc.id)
        into p_f
        from T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG lc
       where lc.obs_id = au_obs_id;
    
      update t_au_observation_old_cad_paras cd
         set cd.para_status =
             (select ml.new_status
                from T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG ml
               where ml.obs_id = au_obs_id
                 and ml.id = p_f),
             cd.setteled_on = sysdate,
             cd.setteled_by = P_NO
       where cd.para_id = au_obs_id;
    
      update T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG t
         set t.authorized_on       = sysdate,
             t.authorized_by       = P_NO,
             t.authorized_comments = 'Request for Status Change is Authorized',
             t.status              = 'A'
       where t.obs_id = au_obs_id;
    
      commit;
    
      IF (P_F = 8) then
        update ais_t_au_post_compliance c
           set c.para_status = p_f,
               c.com_stage = (Case
                               when ENT_T in (6, 28) then
                                13
                               when ENT_T = 22 then
                                44
                               when ENT_T in (5, 17, 21, 25) then
                                21
                               else
                                12
                             end),
               C.COM_STATUS = (Case
                                when ENT_T in (6, 28) then
                                 13
                                when ENT_T = 22 then
                                 44
                                when ENT_T in (5, 17, 21, 25) then
                                 21
                                else
                                 12
                              end)
         where c.com_id = CM_ID;
      else
        update ais_t_au_post_compliance c
           set c.para_status = p_f,
               c.com_stage = (Case
                               when ENT_T in (6, 28) then
                                44
                               when ENT_T = 22 then
                                44
                               when ENT_T in (5, 17, 21, 25) then
                                44
                               else
                                12
                             end),
               C.COM_STATUS  = 16
         where c.com_id = CM_ID;
      end if;
    
      open io_cursor for
        select 'Para number ' || P_N || ' Marked as settled' as remark
          from dual;
    
      -- For 'R' (Reject) Action_IND
    elsif (Action_IND = 'R') then
    
      update T_AU_OLD_PARAS_FAD_STATUS_CHANGE_LOG t
         set t.authorized_on       = sysdate,
             t.authorized_by       = P_NO,
             t.authorized_comments = 'Request for Status Change is Rejected',
             t.status              = 'R'
       where t.obs_id = au_obs_id;
    
      commit;
    
      open io_cursor for
        select 'Para number ' || P_N || ' Rejected' as remark from dual;
    end if;
  
  end P_AuthorizeChangeStatusRequestForSettledPara;

  procedure P_AuthorizeChangeStatusRequestForSettledPara_new(obsid     in number,
                                                             P_IND     in varchar2,
                                                             remark    in varchar2,
                                                             indicator in varchar2,
                                                             ENT_ID    in number,
                                                             P_NO      in number,
                                                             R_ID      in number,
                                                             io_cursor OUT t_cursor) as
    V_F number := 0;
    T_F number := 0;
    Z_R number := 0;
    I_D number := 0;
    S_F number := 0;
    S_T varchar2(100);
    P_F varchar2(10);
  begin
  
    P_add_activity_log(ENT_ID,
                       R_ID,
                       P_NO,
                       79,
                       'Authorize new Observations of ' || obsid || ' as ' ||
                       indicator);
  
    IF (indicator != 'R') then
      select NVL(max(e.id), 0)
        into I_D
        from T_AU_new_PARAS_STATUS_CHANGE_LOG e
       where e.au_obs_id = obsid;
      select l.new_status
        into S_F
        from T_AU_new_PARAS_STATUS_CHANGE_LOG l
       where l.au_obs_id = obsid
         and l.id = I_D;
      select nvl(max(o.id), 0)
        into V_F
        from t_au_observation o
       where o.id = obsid;
      select cp.para_no
        into P_F
        from ais_t_au_post_compliance cp
       where cp.new_para_id = obsid;
    
      select NVL(max(e.au_obs_id), 0)
        into T_F
        from T_AU_new_PARAS_STATUS_CHANGE_LOG e
       where e.au_obs_id = obsid;
      select NVL(max(e.id), 0)
        into I_D
        from T_AU_new_PARAS_STATUS_CHANGE_LOG e
       where e.au_obs_id = obsid;
    
      if (V_F = T_F) then
        if (S_F != 8) then
          update T_AU_OBSERVATION o
             set o.status     = S_F,
                 O.STELLED_ON = SYSDATE,
                 O.SETTLED_BY = P_NO
           where o.id = obsid;
          commit;
          update T_AU_new_PARAS_STATUS_CHANGE_LOG t
             set t.authorized_on       = sysdate,
                 t.authorized_by       = p_no,
                 t.authorized_comments = remark
           where t.au_obs_id = obsid;
          commit;
          update ais_t_au_post_compliance c
             set c.para_status = S_F,
                 c.setteled_on = sysdate,
                 c.setteled_by = P_NO
           where c.new_para_id = obsid
             and c.ind = 'A';
          commit;
        
        else
          if (S_F = 8) then
          
            update T_AU_OBSERVATION o
               set o.status = S_F, O.STELLED_ON = null, O.SETTLED_BY = null
             where o.id = obsid;
            commit;
            update T_AU_new_PARAS_STATUS_CHANGE_LOG t
               set t.authorized_on       = sysdate,
                   t.authorized_by       = p_no,
                   t.authorized_comments = remark
             where t.au_obs_id = obsid;
            commit;
            update ais_t_au_post_compliance c
               set c.para_status = S_F,
                   c.setteled_on = null,
                   c.setteled_by = null
             where c.new_para_id = obsid
               and c.ind = 'A';
            commit;
          end if;
        end if;
        if (S_F = 8) then
          select 'Un-Settled' into S_T from dual;
        else
          select 'Settled' into S_T from dual;
        end if;
        open io_cursor for
          select 'Para number ' || P_F || ' has been markerd as ' || S_T as remarks
            from dual;
      else
        open io_cursor for
          select 'Please contact Mubashir or Asad Ch' as remarks from dual;
      
      end if;
    else
      update T_AU_new_PARAS_STATUS_CHANGE_LOG l
         set l.status        = 'R',
             l.authorized_on = sysdate,
             l.authorized_by = P_NO
       where l.au_obs_id = obsid
         and l.authorized_on is null;
      commit;
      open io_cursor for
        select 'Para Rejected' as remarks from dual;
    
    end if;
  
  end P_AuthorizeChangeStatusRequestForSettledPara_new;

  procedure p_get_violations(io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select * from T_R_SUB_GROUP s where s.max_number is not null;
  
  end p_get_violations;

  Procedure p_get_process_owner(ENT_ID    in number,
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
       212,
       'Get Proc Owner For Check list Detail',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    OPEN io_Cursor FOR
      select e.entity_id, e.name
        from t_auditee_entities e
       where e.type_id = 3
         and e.active = 'Y'
         and e.entity_id not in (112206, 112207, 112221, 112215);
  
  end p_get_process_owner;

  Procedure p_get_role_responsible(ENT_ID    in number,
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
       'Get Role Responsible For Checklist Detail',
       sysdate,
       (select COALESCE(max(l.seq) + 1, 1)
          from t_au_activity_log l
         where l.id = E_F
           and l.ppnum = P_NO),
       'Y');
    commit;
  
    OPEN io_Cursor FOR
      select S.DESIGNATIONCODE, S.DESCRIPTION
        from t_hr_designations s
       where s.designationcode in (7943,
                                   7970,
                                   7973,
                                   7944,
                                   402,
                                   602,
                                   635,
                                   641,
                                   648,
                                   809,
                                   141,
                                   142,
                                   145,
                                   147,
                                   149,
                                   1371,
                                   213,
                                   96,
                                   95,
                                   150,
                                   312)
      
       order by s.designationcode;
  
  end p_get_role_responsible;

  procedure p_GetChecklistSubByProcessId(processId in number,
                                         io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.*, p.heading as T_NAME, e.entitytypedesc as ENTITY_TYPE_NAME
        from t_audit_checklist_sub t
       inner join t_audit_checklist p
          on p.t_id = t.t_id
       inner join t_auditee_ent_types e
          on t.entity_type = e.autid
       where t.STATUS = 'Y'
         and p.t_id = processId
       order by t.s_id;
  
  end p_GetChecklistSubByProcessId;

  procedure p_GetChecklistDetailBySubProcessId(subProcessId in number,
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
             t.owner_enitity_id,
             t.annex,
             p.heading as T_NAME
        from t_audit_checklist_details t
       inner join t_audit_checklist_sub p
          on p.s_id = t.s_id
       where t.STATUS = 'Y'
         and p.s_id = subProcessId
       order by t.s_id;
  
  end p_GetChecklistDetailBySubProcessId;

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

  procedure p_GetRiskProcessTransactionsWithStatus(statusId  IN NUMBER,
                                                   io_cursor OUT t_cursor) as
  
  begin
    if (statusId = 3) THEN
    
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
         inner join t_auditee_entities d
            on pt.process_owner_id = d.code
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
           inner join t_auditee_entities d
              on pt.process_owner_id = d.code
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
           inner join t_auditee_entities d
              on pt.process_owner_id = d.code
           inner join t_audit_checklist_details_status_mapping sm
              on pt.id = sm.T_ID
           inner join t_audit_checklist_details_status s
              on s.ID = sm.STATUS_ID
          
           order by pt.id asc;
      END IF;
    END IF;
  
  end p_GetRiskProcessTransactionsWithStatus;

  procedure p_update_sub_process(sid       in number,
                                 p_id      in number,
                                 sub_name  in varchar2,
                                 io_cursor OUT t_cursor) is
  
  begin
  
    update t_audit_checklist_sub_change s
       set s.n_t_id = p_id, s.sub_process = sub_name, s.status = 'P'
     where s.s_id = sid;
    commit;
    open io_cursor for
      select 'Sub Process has been forwarded to the FAD for review' as remarks
        from dual;
  
  end p_update_sub_process;

  procedure p_update_process_details(d_id        in number,
                                     sid         in number,
                                     sub_name    in varchar2,
                                     vid         in number,
                                     r_id        in number,
                                     owner_id    in number,
                                     p_entity_id in number,
                                     active      in varchar2,
                                     io_cursor   OUT t_cursor) is
  
  begin
  
    update t_audit_checklist_details_change dd
       set dd.n_s_id             = sid,
           dd.n_heading          = sub_name,
           dd.n_v_id             = vid,
           dd.n_risk_id          = r_id,
           dd.n_role_resp_id     = owner_id,
           dd.n_owner_enitity_id = p_entity_id,
           dd.status             = active
     where dd.id = d_id;
    commit;
    open io_cursor for
      select 'Check List Detail has been updated' as remarks from dual;
  
  end p_update_process_details;

  /*  procedure p_add_process_details   (T_id        in number,
                                     sid         in number,
                                     Check_list  in varchar2,
                                     vid         in number,
                                     r_id        in number,
                                     owner_id    in number,
                                     p_entity_id in number,
                                     active      in varchar2,
                                     io_cursor   OUT t_cursor) is
  
  begin
  
    Insert into t_audit_checklist_details_change(d_id,s_id,
                                                 checklist_details,
                                                 risk_id,
                                                 funtional_owner,
                                                 v_id,
                                                 process_owner_id,
                                                 role_resp_id)
  
       values((select COALESCE(max(p.d_id) + 1, 1) from t_audit_checklist_details_change p),
              sid,Check_list,r_id,owner_id,'New Addition',vid,p_entity_id,owner_id
  
       );
    commit;
    open io_cursor for
      select 'Check List Detail has been updated' as remarks from dual;
  
  end p_add_process_details;*/

  /*  procedure p_Get_sub_Process_Review(processId in number,
                                     io_cursor OUT t_cursor) is
  
  begin
  
    OPEN io_Cursor FOR
      select t.s_id,
             t.t_id,
             t.heading,
             t.entity_type,
             t.status,
             ch.s_id as s_id2,
             ch.t_id as t_id2,
             ch.correct_checklist_details as heading2,
             '' as entity_type2,
             ch.correct_status as status2,
             p.heading as T_NAME,
             e.entitytypedesc as ENTITY_TYPE_NAME
        from t_audit_checklist_sub t
       inner join t_audit_checklist_sub_change ch
          on ch.s_id = t.s_id
       inner join t_audit_checklist p
          on p.t_id = t.t_id
       inner join t_auditee_ent_types e
          on t.entity_type = e.autid
       where ch.status = 'Y'
       order by t.s_id;
  
  end p_Get_sub_Process_Review;*/

  /*  procedure p_Get_Process_detail_Review(subProcessId in number,
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
             t.owner_enitity_id,
             t.annex,
             p.heading as T_NAME
        from t_audit_checklist_details t
       inner join t_audit_checklist_details_change ch
          on ch.d_id = t.id
       inner join t_audit_checklist_sub p
          on p.s_id = t.s_id
       where t.STATUS = 'Y'
         and ch.change = 'Y'
       order by t.s_id;
  
  end p_Get_Process_detail_Review;*/

  procedure p_recommend_sub_process(sid            in number,
                                    COMMENTS       in varchar2,
                                    user_PPNO      in number,
                                    user_entity_id in number,
                                    io_cursor      OUT t_cursor) is
  
  begin
  
    update T_AUDIT_CHECKLIST_SUB_change s
       set s.status = 'Y'
     where s.s_id = sid;
    commit;
    insert into t_audit_checklist_details_log
      (id, s_id, status_id, user_id, user_entity, created_on, comments)
    VALUES
      ((select COALESCE(max(pp.ID) + 1, 1)
         from t_audit_checklist_details_log pp),
       sid,
       '3',
       user_PPNO,
       user_entity_id,
       sysdate,
       COMMENTS);
    commit;
    open io_cursor for
      select 'Sub Process has been recommended for approval' as remarks
        from dual;
  
  end p_recommend_sub_process;

  procedure P_GET_SETTLED_PARA_ENTITIES(P_NO      in number,
                                        ENT_ID    in number,
                                        R_ID      in number,
                                        io_cursor out t_cursor) as
  begin
    If (R_ID in (1, 3, 5, 7, 11)) then
      open io_cursor for
        select distinct e.name, e.entity_id
          from V_P_GET_SETTLED_PARA_DETAILS f
         inner join t_auditee_entities e
            on e.entity_id = f.auditedby
        
         where f.settled_on is not null; -- AND E.AUDITBY_ID = ENT_ID;
    else
      open io_cursor for
        select distinct e.name, e.entity_id
          from V_P_GET_SETTLED_PARA_DETAILS f
         inner join t_auditee_entities e
            on e.entity_id = f.entity_id
         inner join t_auditee_entities_maping_fad fad
            on fad.entity_id = F.AUDITEDBY
         inner join T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY h
            on h.entity_id = f.entity_id
         where fad.ppno = P_NO
           and h.reviewed_by is null;
    end if;
  end P_GET_SETTLED_PARA_ENTITIES;

  Procedure P_GET_SETTLED_PARA_DETAILS(P_NO       in number,
                                       ENT_ID     in number,
                                       R_ID       in number,
                                       auditee_id in number,
                                       io_cursor  OUT t_cursor) is
  
  begin
  
    OPEN io_cursor for
      select d.reporting_office,
             d.Entity_name,
             d.audit_period,
             d.para_no,
             d.settled_by,
             d.settled_on,
             d.risk,
             d.para_category,
             d.ref_p,
             d.au_obs_id,
             d.com_id,
             d.compliance_cycle,
             d.entity_id,
             d.auditedby
        from V_P_GET_SETTLED_PARA_DETAILS d
       where d.entity_id = auditee_id
          or d.auditedby = auditee_id
       order by d.settled_on;
  
  end P_GET_SETTLED_PARA_DETAILS;

  Procedure P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE(refp      varchar2,
                                                       obs_id    in number,
                                                       io_cursor OUT t_cursor) is
  begin
    OPEN io_cursor for
      select h.comment_by_ppno attended_by,
             d.description as designation,
             emp.employeefirstname || '  ' || emp.employeelastname as emp_name,
             h.comments remarks,
             f.com_cycle as COMPLIANCE_CYCLE
      
        from ais_t_au_post_compliance_history h
       inner join ais_t_au_post_compliance f
          on f.com_id = h.com_id
       inner join v_service_employeeinfo emp
          on emp.ppno = h.comment_by_ppno
       inner join t_groups d
          on d.group_id = h.com_stage
       where (f.com_id = refp or f.com_id = obs_id)
       order by f.com_cycle, h.comment_on;
  
  end P_GET_SETTLED_PARA_DETAILS_PARA_COMPLIANCE;

  Procedure P_get_auditee_reporting_fad(ENT_ID    in number,
                                        Type_id   in number,
                                        io_cursor OUT t_cursor) is
  begin
  
    open io_cursor for
    
      select f.auditzone_id,
             e.name         as audit_zone,
             f.region_id,
             ee.name        as region,
             f.entity_id,
             et.name        as entity
        from t_auditee_entities_maping_fad_reporting f
       inner join t_auditee_entities e
          on f.auditzone_id = e.entity_id
       inner join t_auditee_entities ee
          on f.region_id = ee.entity_id
       inner join t_auditee_entities et
          on et.entity_id = f.entity_id;
  end P_get_auditee_reporting_fad;

  Procedure P_ADD_auditee_reporting_fad(AZ_ID     in number,
                                        Reg_ID    in number,
                                        ENT_ID    in number,
                                        TYP_ID    in number,
                                        TR        in number,
                                        io_cursor OUT t_cursor) is
  begin
    insert into t_auditee_entities_maping_fad_reporting
      (id, auditzone_id, region_id, entity_id, type_id, teir)
    values
      ((select COALESCE(max(acc.ID) + 1, 1)
         from t_auditee_entities_maping_fad_reporting acc),
       AZ_ID,
       REG_ID,
       ENT_ID,
       TYP_ID,
       TR);
    commit;
  
    open io_cursor for
      select 'Mapping Added' as remarks from dual;
  end P_ADD_auditee_reporting_fad;

  Procedure P_UPDATE_auditee_reporting_fad(update_id in number,
                                           AZ_ID     in number,
                                           Reg_ID    in number,
                                           ENT_ID    in number,
                                           TYP_ID    in number,
                                           TR        in number,
                                           io_cursor OUT t_cursor) is
  begin
    Update t_auditee_entities_maping_fad_reporting t
       set t.auditzone_id = AZ_ID,
           t.region_id    = reg_id,
           t.entity_id    = ent_id,
           t.type_id      = typ_id,
           t.teir         = TR
     where t.id = update_id;
    commit;
    open io_cursor for
      select 'Mapping Updated' as remarks from dual;
  
    commit;
  end P_UPDATE_auditee_reporting_fad;

  Procedure P_GET_SAMPLE(io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
    
      Select s.id, s.sample_type, s.sample_percentage, s.status
        from t_au_sample s;
  
  end P_GET_SAMPLE;

  Procedure P_UPDATE_SAMPLE(s_id      NUMBER,
                            s_per     NUMBER,
                            io_cursor OUT t_cursor) is
  
  begin
    UPDATE T_AU_SAMPLE S SET S.SAMPLE_PERCENTAGE = S_PER WHERE S.ID = s_id;
    COMMIT;
    OPEN io_cursor FOR
    
      Select 'Sample Updated' as remarks from dual;
  
  END P_UPDATE_SAMPLE;

  PROCEDURE p_get_audit_engagement(ent_id    IN NUMBER,
                                   io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT ep.id AS plan_id,
             eng.ENG_ID,
             -- Concatenate all member names for the given team_id
             (SELECT LISTAGG(m.member_name, ', ') WITHIN GROUP(ORDER BY m.member_name)
                FROM t_au_team_members m
               WHERE m.t_id = eng.team_id) AS TEAM_NAME,
             eng.AUDIT_STARTDATE,
             eng.AUDIT_ENDDATE,
             eng.operation_startdate AS OP_STARTDATE,
             eng.operation_enddate AS OP_ENDDATE,
             eng.ENTITY_ID,
             eng.Auditby_Id,
             s.id AS status_id,
             s.status,
             r.id AS rpt_id
        FROM t_au_plan_eng eng
       INNER JOIN t_au_period p
          ON eng.period_id = p.auditperiodid
        LEFT JOIN t_au_plan ep
          ON ep.id = eng.plan_id
       INNER JOIN t_au_plan_eng_status s
          ON eng.status = s.id
       INNER JOIN t_au_audit_teams tm
          ON eng.eng_id = tm.eng_id
        LEFT JOIN t_Audit_Reports r
          ON r.eng_id = eng.eng_id
       WHERE eng.entity_id = ent_id
         AND eng.status < 16;
  END p_get_audit_engagement;

  procedure p_get_audit_observtion(OB_ID in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select o.id,
             ot.obs_id,
             o.memo_number as MEMO,
             o.final_para_no as PARA_NO,
             a.heading as annex,
             p.heading as HEADINGS,
             sb.heading as ASSIGNED_TO,
             cd.heading as CHECK_LIST,
             t.headings as gist,
             t.text as PARA_TEXT,
             o.amount_involved as AMOUNT_INV,
             o.no_of_instances as NO_INSTANCES,
             o.memo_number as memo_no,
             o.memo_date,
             0 as PPNO,
             '-' as RESP_ROLE,
             0 as RESP_AMOUNT,
             AE.REPLY AS auditee_reply,
             AR.AUDIT_REPLY AS AUDITOR_COMMENTS,
             hr.recommendation as HEAD_COMMENTS,
             F.RECOMMENDATION AS ROOT_CAUSE
        from t_au_observation o
       inner join t_au_observation_status s
          on s.statusid = o.status
       inner join t_au_observation_assignedto ot
          on ot.obs_id = o.id
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
       inner join t_auditee_entities e
          on e.entity_id = ot.entity_id
       inner join t_audit_checklist_annexure a
          on a.id = o.annex
       inner join t_audit_checklist_details cd
          on o.checklistdetail_id = cd.id
       inner join t_audit_checklist_sub sb
          on sb.s_id = cd.s_id
       inner join t_audit_checklist p
          on p.t_id = sb.t_id
       INNER JOIN T_AU_OBSERVATIONS_AUDITEE_RESPONSE AE
          ON AE.AU_OBS_ID = O.ID
       INNER JOIN T_AU_OBSERVATIONS_AUDITOR_REPLY AR
          ON AR.AU_OBS_ID = O.ID
       INNER JOIN T_AU_OBSERVATION_FINAL_RECCOMENDATION F
          ON F.OBS_ID = O.ID
       INNER JOIN T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION hr
          on hr.au_obs_id = o.id
       where o.id = OB_ID;
  
  end p_get_audit_observtion;

  procedure p_get_audit_glance(ENGID in number, io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select o.id,
             o.id            as obs_id,
             o.memo_number   as memo_no,
             o.final_para_no,
             o.memo_date,
             r.description   as Risk,
             s.statusname    as status,
             t.headings,
             t.headings      as gist
        from t_au_observation o
       inner join t_au_observation_status s
          on s.statusid = o.status
       inner join t_risk r
          on r.r_id = o.severity
       inner join t_au_observation_text t
          on t.observatsion_id = o.id
      
       where o.engplanid = ENGID
       order by o.status, o.final_para_no;
  
  end p_get_audit_glance;

  procedure p_get_audit_Report(ENGID     in number,
                               RPT_ID    in number,
                               io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select r.id,
             r.eng_id,
             r.audit_report,
             r.added_by,
             r.added_on,
             r.doc_type,
             r.doc_name
        from T_AUDIT_REPORTS r
       where r.eng_id = Engid;
  
  end p_get_audit_Report;

  PROCEDURE P_PARA_SHIFTING(NEW_ENT_ID IN NUMBER,
                            OLD_ENT_ID IN NUMBER,
                            O_ID       IN NUMBER,
                            P_IND      IN VARCHAR2,
                            P_NO       IN NUMBER,
                            ENT_ID     IN NUMBER,
                            R_ID       IN NUMBER,
                            io_cursor  OUT t_cursor) IS
  
    Old_br   NUMBER := 0;
    new_br   NUMBER := 0;
    ENT_TYPE NUMBER := 0;
  
  BEGIN
  
    SELECT e.code
      INTO old_br
      FROM t_auditee_entities e
     WHERE e.entity_id = old_ENT_ID;
    SELECT n.code
      INTO new_br
      FROM t_auditee_entities n
     WHERE n.entity_id = NEW_ENT_ID;
    SELECT t.type_id
      INTO ENT_TYPE
      FROM t_auditee_entities t
     WHERE t.entity_id = NEW_ENT_ID;
  
    IF P_IND = 'A' THEN
    
      UPDATE t_au_observation o
         SET o.entity_id = NEW_ENT_ID, o.entity_code = new_br
       WHERE o.Entity_Id = OLD_ENT_ID
         AND o.id = O_ID;
    
      UPDATE t_au_observation_assignedto ao
         SET ao.entity_id = NEW_ENT_ID
       WHERE ao.entity_id = old_br
         AND ao.obs_id = O_ID;
    
      UPDATE ais_t_au_post_compliance c
         SET c.entity_id      = NEW_ENT_ID,
             c.entity_code    = new_br,
             c.entity_type_id = ENT_TYPE
       WHERE c.entity_id = OLD_ENT_ID
         AND c.ind = P_IND
         AND c.new_para_id = O_ID;
    
    ELSIF P_IND = 'O' THEN
    
      UPDATE t_au_old_paras_fad fd
         SET fd.entity_id = NEW_ENT_ID, fd.entity_code = new_br
       WHERE fd.id = O_ID
         AND fd.entity_id = old_ENT_ID;
    
      UPDATE ais_t_au_post_compliance c
         SET c.entity_id      = NEW_ENT_ID,
             c.entity_code    = new_br,
             c.entity_type_id = ENT_TYPE
       WHERE c.entity_id = OLD_ENT_ID
         AND c.ind = P_IND
         AND c.old_para_id = O_ID;
    
    ELSIF P_IND = 'C' THEN
      UPDATE t_au_observation_old_cad_paras cd
         set cd.entity_id = NEW_ENT_ID
       where cd.para_id = O_ID;
    
      UPDATE ais_t_au_post_compliance c
         SET c.entity_id      = NEW_ENT_ID,
             c.entity_code    = new_br,
             c.entity_type_id = ENT_TYPE
       WHERE c.entity_id = OLD_ENT_ID
         AND c.ind = P_IND
         AND c.new_para_id = O_ID;
    
    END IF;
    COMMIT;
    Open io_cursor for
      Select 'PARA Shifted' as remarks from Dual;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
    
  END P_PARA_SHIFTING;

  PROCEDURE P_GetAuditChecklistAnnexureCirculars(io_cursor OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT ID,
             DIVISION_ENT_ID,
             REFERENCE_TYPE_ID,
             REFERENCE_TYPE,
             INSTRUCTIONSDETAILS,
             KEYWORDS,
             REDIRECTEDPAGE,
             DIVISION,
             INSTRUCTIONSTITLE,
             INSTRUCTIONSDATE,
             DOCTYPE
        FROM T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR;
  END P_GetAuditChecklistAnnexureCirculars;

  PROCEDURE P_InsertCircularDoc(p_circular_id IN NUMBER,
                                p_file_name   IN VARCHAR2,
                                p_file_type   IN VARCHAR2,
                                p_file_size   IN NUMBER,
                                p_file_blob   IN BLOB,
                                p_uploaded_by IN VARCHAR2,
                                o_status      OUT VARCHAR2) IS
  BEGIN
    INSERT INTO T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR_DOCS
      (CIRCULAR_ID,
       FILE_NAME,
       FILE_TYPE,
       FILE_SIZE,
       FILE_BLOB,
       UPLOADED_BY)
    VALUES
      (p_circular_id,
       p_file_name,
       p_file_type,
       p_file_size,
       p_file_blob,
       p_uploaded_by);
  
    o_status := 'Success';
  EXCEPTION
    WHEN OTHERS THEN
      o_status := 'Error: ' || SQLERRM;
  END P_InsertCircularDoc;

  -- Allocate an entity to an auditor within an Audit Zone
  PROCEDURE P_allocate_entity_to_auditor(p_az_id        IN NUMBER,
                                         p_ent_id       IN NUMBER,
                                         p_auditor_ppno IN NUMBER,
                                         p_assigned_by  IN NUMBER,
                                         io_cursor      OUT t_cursor) IS
    v_err_msg VARCHAR2(4000);
    ar_vf     number := 0;
  BEGIN
    select nvl(e.allocation_id, 0)
      into ar_vf
      from t_au_tbl_allocations e
     where e.ent_id = p_ent_id;
    if ar_vf is null or ar_vf = 0 then
      INSERT INTO t_au_tbl_allocations
        (az_id, ent_id, auditor_ppno, assigned_by, assigned_on)
      VALUES
        (p_az_id, p_ent_id, p_auditor_ppno, p_assigned_by, SYSDATE);
    
      OPEN io_cursor FOR
        SELECT 'Allocation successful.' AS remarks FROM DUAL;
    else
      OPEN io_cursor FOR
        SELECT 'Entity already Allocated' AS remarks FROM DUAL;
    
    end if;
  EXCEPTION
    WHEN OTHERS THEN
      v_err_msg := 'Error in allocation: ' || SQLERRM;
      OPEN io_cursor FOR
        SELECT v_err_msg AS remarks FROM DUAL;
    
  END P_allocate_entity_to_auditor;

  -- Approve allocation
  PROCEDURE P_approve_allocation(p_allocation_id IN NUMBER,
                                 p_approved_by   IN NUMBER,
                                 io_cursor       OUT t_cursor) IS
    v_err_msg VARCHAR2(4000);
  BEGIN
    UPDATE t_au_tbl_allocations
       SET approved_by = p_approved_by, approved_on = SYSDATE
     WHERE allocation_id = p_allocation_id;
  
    IF SQL%ROWCOUNT = 0 THEN
      OPEN io_cursor FOR
        SELECT 'No such allocation found.' AS remarks FROM DUAL;
    ELSE
      OPEN io_cursor FOR
        SELECT 'Approval successful.' AS remarks FROM DUAL;
    END IF;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_err_msg := 'Error in approval: ' || SQLERRM;
      OPEN io_cursor FOR
        SELECT v_err_msg AS remarks FROM DUAL;
  END P_approve_allocation;

  PROCEDURE P_GetParaText(p_com_id IN NUMBER, io_cursor OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN io_cursor FOR
      SELECT r.com_id,
             r.entity_id,
             r.old_para_id,
             r.new_para_id,
             r.audit_period,
             r.para_status,
             r.audited_by,
             r.risk,
             r.IND,
             r.para_no,
             r.para_added_on,
             r.gist_of_paras,
             r.text,
             r.text as para_text
        FROM V_GET_ALL_PARA_TEXT_FOR_REFERENCE r
       WHERE COM_ID = p_com_id;
  END P_GetParaText;

  PROCEDURE P_GetParaReferences(p_com_id  IN NUMBER,
                                io_cursor OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN io_cursor FOR
      SELECT t.link_id,
             t.entity_id,
             t.old_para_id,
             t.new_para_id,
             t.para_id,
             t.reference_id,
             t.reference_title,
             t.credit_manual_id,
             t.op_manual_id,
             t.manual_type,
             t.chapter,
             t.matched_text,
             t.link_type,
             t.created_on,
             t.no_reference_flag,
             t.issuance_date as INSTRUCTIONSDATE
        FROM TBL_PARA_REFERENCE_LINKS t
       WHERE PARA_ID = p_com_id
       ORDER BY LINK_ID;
  END;

  -- Update reference of an observation and log the change
  PROCEDURE P_update_para_reference(p_com_id     IN NUMBER,
                                    p_new_ref    IN NUMBER,
                                    p_updated_by IN NUMBER,
                                    io_cursor    OUT t_cursor) IS
    v_err_msg VARCHAR2(4000);
    v_old_ref NUMBER;
  BEGIN
    SELECT nvl(c.annex_ref_id, 0)
      INTO v_old_ref
      FROM ais_t_au_post_compliance c
     WHERE c.com_id = p_com_id;
  
    UPDATE ais_t_au_post_compliance c
       SET c.annex_ref_id = p_new_ref
     WHERE com_id = p_com_id;
  
    INSERT INTO t_au_tbl_update_log
      (com_id,
       field_name,
       old_value,
       new_value,
       updated_by,
       updated_on,
       action_type)
    VALUES
      (p_com_id,
       'REFERENCE_ID',
       TO_CHAR(v_old_ref),
       TO_CHAR(p_new_ref),
       TO_CHAR(p_updated_by),
       SYSDATE,
       'UPDATE');
    commit;
  
    OPEN io_cursor FOR
      SELECT 'Update successful.' AS remarks FROM DUAL;
  
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      OPEN io_cursor FOR
        SELECT 'No observation found for this com_id.' AS remarks
          FROM DUAL;
    WHEN OTHERS THEN
      v_err_msg := 'Error in update: ' || SQLERRM;
      OPEN io_cursor FOR
        SELECT v_err_msg AS remarks FROM DUAL;
  END P_update_para_reference;

  -- Get all logs for a given observation
  PROCEDURE P_get_update_log(p_com_id IN NUMBER, io_cursor OUT t_cursor) IS
    v_err_msg VARCHAR2(4000);
  BEGIN
    OPEN io_cursor FOR
      SELECT updated_on,
             updated_by,
             action_type,
             field_name,
             old_value,
             new_value
        FROM t_au_tbl_update_log
       WHERE com_id = p_com_id
       ORDER BY updated_on DESC;
  
  EXCEPTION
    WHEN OTHERS THEN
      v_err_msg := 'Error retrieving logs: ' || SQLERRM;
      OPEN io_cursor FOR
        SELECT v_err_msg AS remarks FROM DUAL;
  END P_get_update_log;

  PROCEDURE P_GetObservationsForReferenceUpdate(p_ent_id           IN NUMBER,
                                                p_assigned_auditor IN NUMBER,
                                                p_reference_id     IN NUMBER,
                                                io_cursor          OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT c.com_id         as comid,
             c.entity_id      as entid,
             c.gist_of_paras  as paratitle,
             r.id             as referenceid,
             r.reference_type as referencetype,
             l.auditor_ppno   as assignedauditorid,
             c.para_status    as status,
             lg.updated_by,
             lg.updated_on
        FROM ais_t_au_post_compliance c
       inner join t_au_tbl_allocations l
          on c.entity_id = l.ent_id
        left join t_au_tbl_update_log lg
          on lg.com_id = c.com_id
        left join t_audit_checklist_annexure_circular r
          on r.id = c.annex_ref_id
       WHERE (p_ent_id IS NULL OR c.entity_id = p_ent_id)
         AND (p_assigned_auditor IS NULL OR
             l.auditor_ppno = p_assigned_auditor)
         AND (p_reference_id IS NULL OR c.annex_ref_id = p_reference_id);
  END P_GetObservationsForReferenceUpdate;

  PROCEDURE P_GetEntityTaskSummary(p_auditor_ppno IN NUMBER,
                                   io_cursor      OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT a.ent_id AS entity_id,
             e.code AS entity_code, -- from your entity/branch table
             e.name AS entity_name, -- from your entity/branch table
             NVL(o.audit_period, 0) AS audit_year,
             COUNT(o.com_id) AS total_paras,
             SUM(CASE
                   WHEN o.annex_ref_id is not null THEN
                    1
                   ELSE
                    0
                 END) AS paras_updated
        FROM t_au_tbl_allocations a
        JOIN t_auditee_entities e
          ON a.ent_id = e.entity_id -- replace t_entities with your actual entity table
        LEFT JOIN ais_t_au_post_compliance o
          ON o.entity_id = a.ent_id
         and o.para_status = 8
         AND a.auditor_ppno = p_auditor_ppno
       WHERE a.auditor_ppno = p_auditor_ppno
       GROUP BY a.ent_id, e.code, e.name, o.audit_period
       ORDER BY e.name, o.audit_period;
  END P_GetEntityTaskSummary;

  PROCEDURE P_ManageReference(p_action            IN VARCHAR2, -- 'ADD', 'UPDATE', 'DELETE'
                              p_link_id           IN NUMBER DEFAULT NULL,
                              p_entity_id         IN NUMBER DEFAULT NULL,
                              p_old_para_id       IN NUMBER DEFAULT NULL,
                              p_new_para_id       IN NUMBER DEFAULT NULL,
                              p_para_id           IN NUMBER DEFAULT NULL,
                              p_ref_id            IN NUMBER DEFAULT NULL,
                              p_ref_title         IN VARCHAR2 DEFAULT NULL,
                              p_credit_manual_id  IN NUMBER DEFAULT NULL,
                              p_op_manual_id      IN NUMBER DEFAULT NULL,
                              p_manual_type       IN VARCHAR2 DEFAULT NULL,
                              p_chapter           IN VARCHAR2 DEFAULT NULL,
                              p_instructions_date in date,
                              p_matched_text      IN VARCHAR2 DEFAULT NULL,
                              p_link_type         IN VARCHAR2 DEFAULT NULL,
                              p_new_ref           IN NUMBER DEFAULT NULL,
                              p_user              IN VARCHAR2 DEFAULT NULL,
                              io_cursor           OUT t_cursor) AS
    v_total_links  NUMBER;
    v_new_ref_id   NUMBER;
    v_final_ref_id NUMBER;
  
  BEGIN
    IF p_action = 'ADD' THEN
      v_final_ref_id := p_ref_id;
    
      -- If Credit Manual or Operations Manual, ensure an entry exists in T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR
      IF p_manual_type = 'Credit Manual' THEN
        BEGIN
          -- Try to find existing
          SELECT id
            INTO v_new_ref_id
            FROM T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR
           WHERE REFERENCE_TYPE = 'Manual'
             AND INSTRUCTIONSTITLE = 'Credit Manual'
             AND ROWNUM = 1;
          v_final_ref_id := v_new_ref_id;
        EXCEPTION
          WHEN NO_DATA_FOUND THEN
            -- Insert new entry for Credit Manual
            INSERT INTO T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR
              (DIVISION_ENT_ID,
               REFERENCE_TYPE_ID,
               REFERENCE_TYPE,
               INSTRUCTIONSDETAILS,
               KEYWORDS,
               REDIRECTEDPAGE,
               DIVISION,
               INSTRUCTIONSTITLE,
               INSTRUCTIONSDATE,
               DOCTYPE)
            VALUES
              (NULL,
               NULL,
               'Manual',
               'Credit Manual',
               'Credit Manual',
               NULL,
               NULL,
               p_chapter,
               p_instructions_date,
               'MANUAL')
            RETURNING ID INTO v_new_ref_id;
            v_final_ref_id := v_new_ref_id;
          
        END;
      END IF;
      -- Overwrite p_ref_id to always use the correct one
    
      IF p_manual_type = 'Operations Manual' THEN
        BEGIN
          -- Try to find existing
          SELECT id
            INTO v_new_ref_id
            FROM T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR
           WHERE REFERENCE_TYPE = 'Manual'
             AND INSTRUCTIONSTITLE = 'Operations Manual'
             AND ROWNUM = 1;
          v_final_ref_id := v_new_ref_id;
        EXCEPTION
          WHEN NO_DATA_FOUND THEN
            -- Insert new entry for Operations Manual
            INSERT INTO T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR
              (DIVISION_ENT_ID,
               REFERENCE_TYPE_ID,
               REFERENCE_TYPE,
               INSTRUCTIONSDETAILS,
               KEYWORDS,
               REDIRECTEDPAGE,
               DIVISION,
               INSTRUCTIONSTITLE,
               INSTRUCTIONSDATE,
               DOCTYPE)
            VALUES
              (NULL,
               NULL,
               'Manual',
               'Operations Manual',
               'Operations Manual',
               NULL,
               NULL,
               p_chapter,
               p_instructions_date,
               'MANUAL')
            RETURNING ID INTO v_new_ref_id;
            v_final_ref_id := v_new_ref_id;
        END;
      ENd if;
      --p_manual_type = 'Circular' and
      If (p_old_para_id = 0 and p_new_para_id = 0 and
         p_manual_type = 'Circular') then
        INSERT INTO TBL_PARA_REFERENCE_LINKS
          (ENTITY_ID,
           OLD_PARA_ID,
           NEW_PARA_ID,
           PARA_ID,
           REFERENCE_ID,
           REFERENCE_TITLE,
           CREDIT_MANUAL_ID,
           OP_MANUAL_ID,
           MANUAL_TYPE,
           CHAPTER,
           MATCHED_TEXT,
           LINK_TYPE,
           CREATED_ON,
           ISSUANCE_DATE)
        VALUES
          (p_entity_id,
           p_old_para_id,
           p_para_id,
           p_para_id,
           v_final_ref_id,
           p_ref_title,
           p_credit_manual_id,
           p_op_manual_id,
           p_manual_type,
           p_chapter,
           p_matched_text,
           p_link_type,
           SYSDATE,
           p_instructions_date);
        COMMIT;
      
        -- Overwrite p_ref_id to always use the correct one
      ELSIF (p_old_para_id != 0 and p_new_para_id != 0 and
            p_manual_type != 'Circular') then
        -- Insert into TBL_PARA_REFERENCE_LINKS
        INSERT INTO TBL_PARA_REFERENCE_LINKS
          (ENTITY_ID,
           OLD_PARA_ID,
           NEW_PARA_ID,
           PARA_ID,
           REFERENCE_ID,
           REFERENCE_TITLE,
           CREDIT_MANUAL_ID,
           OP_MANUAL_ID,
           MANUAL_TYPE,
           CHAPTER,
           MATCHED_TEXT,
           LINK_TYPE,
           CREATED_ON,
           ISSUANCE_DATE)
        VALUES
          (p_entity_id,
           p_old_para_id,
           p_para_id,
           p_para_id,
           v_final_ref_id,
           p_ref_title,
           p_credit_manual_id,
           p_op_manual_id,
           p_manual_type,
           p_chapter,
           p_matched_text,
           p_link_type,
           SYSDATE,
           p_instructions_date);
        COMMIT;
      end if;
    ELSIF p_action = 'UPDATE' THEN
      UPDATE TBL_PARA_REFERENCE_LINKS
         SET REFERENCE_TITLE = p_ref_title, MATCHED_TEXT = p_matched_text
       WHERE LINK_ID = p_link_id;
      COMMIT;
    ELSIF p_action = 'DELETE' THEN
      DELETE FROM TBL_PARA_REFERENCE_LINKS
       WHERE LINK_ID = p_link_id
         AND PARA_ID = p_para_id;
      COMMIT;
    END IF;
  
    -- After every action (add, update, delete), update ais_t_au_post_compliance:
    SELECT COUNT(*)
      INTO v_total_links
      FROM TBL_PARA_REFERENCE_LINKS
     WHERE PARA_ID = p_para_id;
  
    UPDATE ais_t_au_post_compliance c
       SET c.reference_reviewed = CASE
                                    WHEN v_total_links > 0 THEN
                                     1
                                    ELSE
                                     0
                                  END
     WHERE c.com_id = p_para_id;
  
    COMMIT;
    OPEN io_cursor FOR
      SELECT 'Success' AS remarks, p_action AS action, p_para_id AS para_id
        FROM dual;
  END P_ManageReference;

  PROCEDURE P_GetPendingParas(p_entity_id  IN NUMBER,
                              p_audit_year IN varchar2,
                              io_cursor    OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT com_id         AS para_id,
             c.audit_period as audit_year,
             --2025            as audit_year,
             para_no,
             c.gist_of_paras AS gist,
             c.risk -- Adjust field if you have risk in another column/table
        FROM ais_t_au_post_compliance c
       WHERE c.entity_id = p_entity_id
         AND c.audit_period = p_audit_year
         and c.para_status = 8
         AND c.reference_reviewed = 0 -- 'P' = Pending; change if your status code is different
       ORDER BY para_no;
  END P_GetPendingParas;

  PROCEDURE P_SearchReferences(p_ref_type IN VARCHAR2,
                               p_keyword  IN VARCHAR2,
                               io_cursor  OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT ID AS REFERENCE_ID,
             id,
             REFERENCE_TYPE as REFERENCE_TYPE,
             INSTRUCTIONSTITLE AS TITLE,
             trunc(INSTRUCTIONSDATE) as INSTRUCTIONSDATE,
             DIVISION,
             INSTRUCTIONSDETAILS,
             KEYWORDS,
             'http://10.100.20.14/' || redirectedpage as REFERENCEURL
        FROM T_AUDIT_CHECKLIST_ANNEXURE_CIRCULAR
       WHERE (UPPER(REFERENCE_TYPE) = UPPER(p_ref_type) OR
             p_ref_type IS NULL)
         AND (UPPER(INSTRUCTIONSTITLE) LIKE '%' || UPPER(p_keyword) || '%' OR
             UPPER(KEYWORDS) LIKE '%' || UPPER(p_keyword) || '%' OR
             UPPER(INSTRUCTIONSDETAILS) LIKE
             '%' || UPPER(p_keyword) || '%' OR p_keyword IS NULL)
       ORDER BY INSTRUCTIONSDATE DESC, INSTRUCTIONSTITLE;
  END P_SearchReferences;

  -- Get para list by status
  PROCEDURE P_GET_PARA_STATUS_REQUEST(P_ENTITY_ID IN NUMBER,
                                      P_STATUS    IN NUMBER,
                                      IO_CURSOR   OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT c.COM_ID,
             c.AUDIT_PERIOD,
             c.PARA_NO,
             (case
               when c.annex is not null then
                a.code
               else
                '-'
             end) as ANNEX,
             c.GIST_OF_PARAS,
             r.description as RISK,
             c.PARA_STATUS,
             'OK' AS RESULT_MSG
        FROM AIS_T_AU_POST_COMPLIANCE c
       inner join t_risk r
          on r.r_id = c.risk
        left join t_audit_checklist_annexure a
          on a.id = c.annex
       WHERE c.ENTITY_ID = P_ENTITY_ID
         AND c.PARA_STATUS = P_STATUS
         AND NOT EXISTS (SELECT 1
                FROM AIS_T_PARA_STATUS_CHANGE_LOG l
               WHERE c.com_id = l.com_id
                 AND l.action_status = 'PENDING')
       order by c.audit_period, c.para_no;
  END P_GET_PARA_STATUS_REQUEST;

  -- Maker submits status change request
  PROCEDURE P_ADD_PARA_STATUS_CHANGE(P_COM_ID        IN NUMBER,
                                     P_NEW_STATUS    IN NUMBER,
                                     P_MAKER_REMARKS IN VARCHAR2,
                                     P_USER_ID       IN NUMBER,
                                     IO_CURSOR       OUT SYS_REFCURSOR) AS
    v_old_status NUMBER;
  BEGIN
    SELECT PARA_STATUS
      INTO v_old_status
      FROM AIS_T_AU_POST_COMPLIANCE
     WHERE COM_ID = P_COM_ID;
  
    INSERT INTO AIS_T_PARA_STATUS_CHANGE_LOG
      (COM_ID, OLD_STATUS, NEW_STATUS, MAKER_REMARKS, CHANGED_BY)
    VALUES
      (P_COM_ID, v_old_status, P_NEW_STATUS, P_MAKER_REMARKS, P_USER_ID);
  
    OPEN IO_CURSOR FOR
      SELECT 'Submitted for Authorization' AS RESULT_MSG FROM dual;
  END P_ADD_PARA_STATUS_CHANGE;

  -- Get pending authorization requests
  PROCEDURE P_GET_PARA_STATUS_AUTHORIZATION(IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT L.LOG_ID,
             L.COM_ID,
             L.OLD_STATUS,
             L.NEW_STATUS,
             L.MAKER_REMARKS,
             L.AUTHORIZER_REMARKS,
             L.CHANGED_BY,
             L.CHANGED_ON,
             L.ACTION_STATUS,
             P.AUDIT_PERIOD,
             P.PARA_NO,
             P.ANNEX,
             P.GIST_OF_PARAS,
             P.RISK
        FROM AIS_T_PARA_STATUS_CHANGE_LOG L
        JOIN AIS_T_AU_POST_COMPLIANCE P
          ON L.COM_ID = P.COM_ID
       WHERE L.ACTION_STATUS = 'PENDING';
  END P_GET_PARA_STATUS_AUTHORIZATION;

  -- Authorize or reject request
  PROCEDURE P_AUTHORIZE_PARA_STATUS_CHANGE(P_LOG_ID       IN NUMBER,
                                           P_ACTION       IN VARCHAR2,
                                           P_AUTH_BY      IN NUMBER,
                                           P_AUTH_REMARKS IN VARCHAR2,
                                           IO_CURSOR      OUT SYS_REFCURSOR) AS
    v_com_id        NUMBER;
    v_new_status    NUMBER;
    v_action        VARCHAR2(20);
    v_role_id       NUMBER;
    v_status_id     NUMBER;
    v_cycle         NUMBER;
    v_maker_remarks VARCHAR2(1000);
    v_direction     VARCHAR2(1);
  BEGIN
    -- Map shorthand to full value
    IF UPPER(P_ACTION) = 'A' THEN
      v_action := 'AUTHORIZED';
    ELSIF UPPER(P_ACTION) = 'R' THEN
      v_action := 'REJECTED';
    ELSE
      v_action := 'PENDING';
    END IF;
  
    -- Fetch log details
    SELECT L.COM_ID, L.NEW_STATUS, L.MAKER_REMARKS
      INTO v_com_id, v_new_status, v_maker_remarks
      FROM AIS_T_PARA_STATUS_CHANGE_LOG L
     WHERE L.LOG_ID = P_LOG_ID;
  
    -- Update the log
    UPDATE AIS_T_PARA_STATUS_CHANGE_LOG
       SET ACTION_STATUS      = v_action,
           AUTHORIZED_BY      = P_AUTH_BY,
           AUTHORIZED_ON      = SYSDATE,
           AUTHORIZER_REMARKS = P_AUTH_REMARKS
     WHERE LOG_ID = P_LOG_ID;
  
    -- Determine direction automatically
    IF v_new_status = 9 THEN
      v_direction := 'U';
    ELSE
      v_direction := 'D';
    END IF;
  
    -- Only on Authorization update compliance + history
    IF v_action = 'AUTHORIZED' THEN
      -- Get compliance context
      SELECT C.COM_CYCLE,
             (CASE
               WHEN v_direction = 'U' THEN
                C.NEXT_R_ID
               ELSE
                C.PER_R_ID
             END) AS ROLE_ID,
             (CASE
               WHEN v_direction = 'U' THEN
                C.C_STATUS_UP
               ELSE
                C.C_STATUS_DOWN
             END) AS STATUS_ID
        INTO v_cycle, v_role_id, v_status_id
        FROM v_get_ais_post_compliance_ais_for_para_change C
       WHERE C.COM_ID = v_com_id;
    
      -- Update compliance record
      UPDATE AIS_T_AU_POST_COMPLIANCE
         SET PARA_STATUS = v_new_status,
             COM_CYCLE   = v_cycle,
             COM_STAGE   = v_role_id,
             COM_STATUS  = v_status_id
       WHERE COM_ID = v_com_id;
    
      -- Insert into history (maker’s remarks as reasoning)
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
        ((SELECT NVL(MAX(HIST_ID), 0) + 1
           FROM AIS_T_AU_POST_COMPLIANCE_HISTORY),
         v_com_id,
         v_cycle,
         v_status_id,
         v_role_id,
         (SELECT g.group_name FROM t_groups g WHERE g.group_id = v_role_id),
         P_AUTH_BY,
         SYSDATE,
         v_maker_remarks, -- keep maker’s actual reasoning
         'Y');
    END IF;
  
    OPEN IO_CURSOR FOR
      SELECT v_action AS RESULT_MSG FROM dual;
  END P_AUTHORIZE_PARA_STATUS_CHANGE;

  PROCEDURE P_GET_IAS_PARA_TEXT(P_COM_ID  IN NUMBER,
                                IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT t.com_id, t.entity_id, t.IND, t.gist_of_paras, t.text
        FROM v_get_ias_para_text t
       WHERE COM_ID = P_COM_ID;
  END P_GET_IAS_PARA_TEXT;

  PROCEDURE P_GET_REFERENCE_MASTER_DETAIL(p_search_text           IN VARCHAR2 DEFAULT NULL,
                                          p_reference_source_type IN VARCHAR2 DEFAULT NULL,
                                          p_ref_id                IN NUMBER DEFAULT NULL,
                                          o_cursor                OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT v.ref_id,
             v.reference_source_type,
             v.source_pk_id,
             v.manual_id,
             v.reference_type,
             v.division,
             v.instruction_date,
             v.section_text,
             v.chapter_no,
             v.sub_section_no,
             v.title_or_heading,
             CASE
               WHEN v.reference_source_type = 'MANUAL_INDEX' THEN
                NVL(v.section_text, '-') || ' / ' || NVL(v.chapter_no, '-') ||
                ' / ' || NVL(v.sub_section_no, '-') || ' / ' ||
                NVL(v.title_or_heading, '-')
               ELSE
                NVL(v.title_or_heading, '-')
             END AS display_text
        FROM vw_reference_master_detail v
       WHERE (p_ref_id IS NULL OR v.ref_id = p_ref_id)
         AND (p_reference_source_type IS NULL OR
             UPPER(v.reference_source_type) =
             UPPER(p_reference_source_type))
         AND (p_search_text IS NULL OR UPPER(NVL(v.title_or_heading, '')) LIKE
             '%' || UPPER(p_search_text) || '%' OR
             UPPER(NVL(v.section_text, '')) LIKE
             '%' || UPPER(p_search_text) || '%' OR
             UPPER(NVL(v.chapter_no, '')) LIKE
             '%' || UPPER(p_search_text) || '%' OR
             UPPER(NVL(v.sub_section_no, '')) LIKE
             '%' || UPPER(p_search_text) || '%' OR
             UPPER(NVL(v.reference_type, '')) LIKE
             '%' || UPPER(p_search_text) || '%' OR
             UPPER(NVL(v.division, '')) LIKE
             '%' || UPPER(p_search_text) || '%')
       ORDER BY CASE
                  WHEN v.reference_source_type = 'MANUAL_INDEX' THEN
                   1
                  ELSE
                   2
                END,
                v.manual_id,
                v.section_text,
                v.chapter_no,
                v.sub_section_no,
                v.title_or_heading;
  END P_GET_REFERENCE_MASTER_DETAIL;

  PROCEDURE P_GET_MANUAL_MASTER(o_cursor OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT m.manual_id,
             m.manual_name,
             m.volume_name,
             CASE
               WHEN m.volume_name IS NOT NULL THEN
                m.manual_name || ' - ' || m.volume_name
               ELSE
                m.manual_name
             END AS display_name
        FROM t_manual_master m
       WHERE NVL(m.is_active, 'Y') = 'Y'
       ORDER BY m.manual_name, m.volume_name;
  END P_GET_MANUAL_MASTER;

  PROCEDURE P_GET_MANUAL_SECTIONS(p_manual_id IN NUMBER,
                                  o_cursor    OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT DISTINCT m.section AS section_text
        FROM t_manual_index m
       WHERE m.manual_id = p_manual_id
         AND NVL(m.is_active, 'Y') = 'Y'
         AND m.section IS NOT NULL
       ORDER BY m.section;
  END P_GET_MANUAL_SECTIONS;

  PROCEDURE P_GET_MANUAL_CHAPTERS(p_manual_id    IN NUMBER,
                                  p_section_text IN VARCHAR2,
                                  o_cursor       OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT DISTINCT m.chapter_no
        FROM t_manual_index m
       WHERE m.manual_id = p_manual_id
         AND NVL(m.is_active, 'Y') = 'Y'
         AND NVL(m.section, '##') = NVL(p_section_text, '##')
         AND m.chapter_no IS NOT NULL
       ORDER BY m.chapter_no;
  END P_GET_MANUAL_CHAPTERS;

  PROCEDURE P_GET_MANUAL_REFERENCE_GRID(p_manual_id    IN NUMBER,
                                        p_section_text IN VARCHAR2,
                                        p_chapter_no   IN VARCHAR2,
                                        o_cursor       OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT v.ref_id,
             v.reference_source_type,
             v.source_pk_id,
             v.manual_id,
             v.section_text,
             v.chapter_no,
             v.sub_section_no,
             v.title_or_heading,
             NVL(TRIM(v.section_text), '-') || ' / ' ||
             NVL(TRIM(v.chapter_no), '-') || ' / ' ||
             NVL(TRIM(v.sub_section_no), '-') || ' / ' ||
             NVL(TRIM(v.title_or_heading), '-') AS display_text
        FROM vw_reference_master_detail v
       WHERE TRIM(UPPER(v.reference_source_type)) = 'MANUAL_INDEX'
         AND v.manual_id = p_manual_id
         AND NVL(TRIM(v.section_text), '##') =
             NVL(TRIM(p_section_text), '##')
         AND NVL(TRIM(v.chapter_no), '##') = NVL(TRIM(p_chapter_no), '##')
       ORDER BY v.sub_section_no, v.title_or_heading;
  END P_GET_MANUAL_REFERENCE_GRID;

  PROCEDURE P_GET_REFERENCE_DETAIL_BY_ID(p_ref_id IN NUMBER,
                                         o_cursor OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT v.ref_id,
             v.reference_source_type,
             v.source_pk_id,
             v.manual_id,
             v.reference_type,
             v.division,
             v.instruction_date,
             v.section_text,
             v.chapter_no,
             v.sub_section_no,
             v.title_or_heading,
             CASE
               WHEN v.reference_source_type = 'MANUAL_INDEX' THEN
                NVL(v.reference_Source, '-') || ': ' ||
                NVL(v.section_text, '-') || ' / ' || NVL(v.chapter_no, '-') ||
                ' / ' || NVL(v.sub_section_no, '-') || ' / ' ||
                NVL(v.title_or_heading, '-')
               ELSE
                NVL(v.title_or_heading, '-')
             END AS display_text
        FROM vw_reference_master_detail v
       WHERE v.ref_id = p_ref_id;
  END P_GET_REFERENCE_DETAIL_BY_ID;

  PROCEDURE P_GET_FAD_ANNEXURE_CONFIG(IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT C.ANNEXURE_ID,
             A.CODE             AS ANNEXURE_CODE,
             A.HEADING          AS DESCRIPTION,
             C.SHIFT_APPLICABLE,
             C.ACTIVE,
             C.UPDATED_BY,
             C.UPDATED_ON
        FROM T_AU_FAD_ANNEXURE_CONFIG C
        JOIN T_AUDIT_CHECKLIST_ANNEXURE A
          ON A.ID = C.ANNEXURE_ID
       ORDER BY C.ANNEXURE_ID;
  END P_GET_FAD_ANNEXURE_CONFIG;

  PROCEDURE P_UPDATE_FAD_ANNEXURE_STATUS(P_ANNEXURE_ID      IN NUMBER,
                                         P_SHIFT_APPLICABLE IN VARCHAR2,
                                         P_NO               IN NUMBER,
                                         IO_CURSOR          OUT T_CURSOR) IS
    V_ACTIVE T_AU_FAD_ANNEXURE_CONFIG.ACTIVE%TYPE;
  BEGIN
    IF P_ANNEXURE_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20001, 'Annexure ID is required.');
    END IF;
    IF UPPER(TRIM(P_SHIFT_APPLICABLE)) NOT IN ('Y', 'N') OR
       P_SHIFT_APPLICABLE IS NULL THEN
      RAISE_APPLICATION_ERROR(-20002,
                              'Shifting applicability must be Y or N.');
    END IF;
    IF P_NO IS NULL OR P_NO <= 0 THEN
      RAISE_APPLICATION_ERROR(-20003,
                              'A valid logged-in user is required.');
    END IF;
  
    BEGIN
      SELECT ACTIVE
        INTO V_ACTIVE
        FROM T_AU_FAD_ANNEXURE_CONFIG
       WHERE ANNEXURE_ID = P_ANNEXURE_ID
         FOR UPDATE;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20004,
                                'Annexure configuration was not found.');
    END;
  
    IF V_ACTIVE <> 'Y' THEN
      RAISE_APPLICATION_ERROR(-20005,
                              'Inactive Annexure configuration cannot be updated.');
    END IF;
  
    UPDATE T_AU_FAD_ANNEXURE_CONFIG
       SET SHIFT_APPLICABLE = UPPER(TRIM(P_SHIFT_APPLICABLE)),
           UPDATED_BY       = P_NO,
           UPDATED_ON       = SYSDATE
     WHERE ANNEXURE_ID = P_ANNEXURE_ID
       AND ACTIVE = 'Y';
  
    COMMIT;
    OPEN IO_CURSOR FOR
      SELECT 'Y' AS SUCCESS,
             'Annexure shifting status has been updated successfully.' AS REMARKS,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_FAD_ANNEXURE_CONFIG
       WHERE ANNEXURE_ID = P_ANNEXURE_ID;
  EXCEPTION
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
  END P_UPDATE_FAD_ANNEXURE_STATUS;

end PKG_FAD;

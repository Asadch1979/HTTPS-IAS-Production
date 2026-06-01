create or replace package PKG_BAC is
  TYPE t_cursor IS REF CURSOR;

  procedure P_BAC_Meeting(Meeting_Number   in number,
                          Meeting_Date     in date,
                          Meeting_Location in varchar2,
                          Bac_Chairman     in varchar2,
                          Bac_Scertery     in varchar2,
                          ppno             in number,
                          io_cursor        OUT t_cursor);

  procedure P_BAC_Meeting_minutes(Meeting_Number        in number,
                                  Mintues_Approval_Date in date,
                                  Total_Actionables     in number,
                                  ppno                  in number,
                                  Status                in varchar2,
                                  io_cursor             OUT t_cursor);

  procedure P_Bac_Actionable(Meeting_Number in number,
                             Item_Number    in number,
                             Bac_Decision   in varchar2,
                             Time_Line      in date,
                             Due_Date       in date,
                             Rpt_Frequency  in varchar2,
                             ppno           in number,
                             Status         in varchar2,
                             io_cursor      OUT t_cursor);

  procedure P_Bac_Actionable_Assign_to(Item_Number in number,
                                       Div_ID      in number,
                                       DUE_DATE    in date,
                                       Action      in varchar2,
                                       ppno        in number,
                                       Status      in varchar2,
                                       io_cursor   OUT t_cursor);

  Procedure P_BAC_DIV(io_cursor OUT t_cursor);

  Procedure P_BAC_DEPT(User_entityid in number, io_cursor OUT t_cursor);

  procedure P_Bac_get_actionable(status    in varchar2,
                                 io_cursor OUT t_cursor);

  procedure P_Bac_get_actionable_snap(io_cursor OUT t_cursor);

  procedure P_Bac_get_actionable_sum(io_cursor OUT t_cursor);

  procedure P_Bac_get_actionable_meetings(meeting   in number,
                                          io_cursor OUT t_cursor);

  procedure P_Bac_get_actionable_meetings_with_status(meeting   in number,
                                                      A_Status  in Varchar2,
                                                      io_cursor OUT t_cursor);

  Procedure P_Bac_get_Actionable_assign_to_dept(User_entityid in number,
                                                io_cursor     OUT t_cursor);

  Procedure P_Bac_Actionable_response(Item_Number in number,
                                      DEPTID      in number,
                                      RESPONSE    in clob,
                                      attachement in blob,
                                      ppno        in number,
                                      Status      in varchar2,
                                      io_cursor   OUT t_cursor);

  Procedure P_Bac_Actionable_response_reviewer(User_entityid in number,
                                               io_cursor     OUT t_cursor);

  Procedure P_BAC_AGENDA(Meeting in number, io_cursor OUT t_cursor);

  Procedure P_CIA_ANALYSIS(io_cursor OUT t_cursor);

  Procedure P_CIA_ANALYSIS_DETAILS(a_id in number, io_cursor OUT t_cursor);

  Procedure P_CIA_ANALYSIS_DETAILS_PARA(A_ID      in number,
                                        R_ID      in number,
                                        ENT_ID    in number,
                                        P_NO      in number,
                                        io_cursor OUT t_cursor);

  Procedure P_CIA_ANALYSIS_DETAILS_PARA_TEXT(P_ID      in number,
                                             P_C       in varchar2,
                                             io_cursor OUT t_cursor);

  Procedure P_CIA_ANALYSIS_SUMMARY(A_ID      in number,
                                   R_ID      in number,
                                   ENT_ID    in number,
                                   io_cursor OUT t_cursor);

end PKG_BAC;

create or replace package body PKG_BAC is

  procedure P_BAC_Meeting(Meeting_Number   in number,
                          Meeting_Date     in date,
                          Meeting_Location in varchar2,
                          Bac_Chairman     in varchar2,
                          Bac_Scertery     in varchar2,
                          ppno             in number,
                          io_cursor        OUT t_cursor) is

  begin

    INSERT INTO t_Bac_Meetings
      (Id,
       Meeting_Number,
       Meeting_Date,
       Meeting_Location,
       Bac_Chairman,
       Bac_Scertery,
       Entered_By,
       Entered_On)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1) from t_Bac_Meetings acc),
       Meeting_Number,
       Meeting_Date,
       Meeting_Location,
       Bac_Chairman,
       Bac_Scertery,
       ppno,
       sysdate);
    commit;
    open io_cursor for
      select max(ac.id) AS M_ID
        from t_Bac_Meetings ac
       where ac.entered_by = PPNO
         and trunc(ac.entered_on) = trunc(sysdate);
  end P_BAC_Meeting;

  procedure P_BAC_Meeting_minutes(Meeting_Number        in number,
                                  Mintues_Approval_Date in date,
                                  Total_Actionables     in number,
                                  ppno                  in number,
                                  Status                in varchar2,
                                  io_cursor             OUT t_cursor) is

  begin

    INSERT INTO t_Bac_Meetings_Minutes
      (Id,
       Meeting_Number,
       Mintues_Approval_Date,
       Total_Actionables,
       Entered_By,
       Entered_On,
       Status)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1) from t_Bac_Meetings_Minutes acc),
       Meeting_Number,
       Mintues_Approval_Date,
       Total_Actionables,
       ppno,
       sysdate,
       Status);
    commit;
    open io_cursor for
      select max(ac.id) AS MID
        from t_Bac_Meetings_Minutes ac
       where ac.entered_by = PPNO
         and trunc(ac.entered_on) = trunc(sysdate);
  end P_BAC_Meeting_minutes;

  procedure P_Bac_Actionable(Meeting_Number in number,
                             Item_Number    in number,
                             Bac_Decision   in varchar2,
                             Time_Line      in date,
                             Due_Date       in date,
                             Rpt_Frequency  in varchar2,
                             ppno           in number,
                             Status         in varchar2,
                             io_cursor      OUT t_cursor) is

  begin

    /*    INSERT INTO t_Bac_Actionable      (Id,
       Meeting_Number,Item_Heading,
       Time_Line,
       Due_Date,
       Rpt_Frequency,
       Entered_By,
       Entered_On,
       Status)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1) from t_Bac_Actionable acc),
       Meeting_Number,
       Item_Number,
       Bac_Decision,
       Time_Line,
       Due_Date,
       Rpt_Frequency,
       ppno,
       sysdate,
       Status);
    commit;*/
    open io_cursor for
      select max(ac.id) AS ACT_ID
        from t_Bac_Actionable ac
       where ac.entered_by = PPNO
         and trunc(ac.entered_on) = trunc(sysdate);
  end P_Bac_Actionable;

  procedure P_Bac_Actionable_Assign_to(Item_Number in number,
                                       Div_ID      in number,
                                       DUE_DATE    in date,
                                       Action      in varchar2,
                                       ppno        in number,
                                       Status      in varchar2,
                                       io_cursor   OUT t_cursor) is

  begin

    INSERT INTO T_BAC_ACTIONABLE_ASSIGN
      (ID,
       ITEMNUMBER,
       DIV_ID,
       DUE_DATE,
       ACTION,
       ENTERED_BY,
       ENTERED_ON,
       STATUS)

    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_BAC_ACTIONABLE_ASSIGN_DEPT acc),
       Item_Number,
       Div_ID,
       DUE_DATE,
       Action,
       ppno,
       sysdate,
       Status);
    commit;
    open io_cursor for
      SELECT 'ACTIONABLE ASSIGNED' AS REF_OUT FROM DUAL;
  end P_Bac_Actionable_Assign_to;

  Procedure P_BAC_DIV(io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select * from t_auditee_entities e where e.type_id = 3;

  end P_BAC_DIV;

  Procedure P_BAC_DEPT(User_entityid in number, io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select e.entity_id, e.c_name as dept_name
        from t_auditee_entities_maping e
       where e.parent_id = User_entityid;

  end P_BAC_DEPT;

  procedure P_Bac_get_actionable_snap(io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select Count(t.id) as total,
             sum(case
                   when t.status = 'Completed' then
                    1
                   else
                    0
                 end) Completed,
             sum(case
                   when t.status != 'Completed' then
                    1
                   else
                    0
                 end) un_Completed

        from T_BAC_ACTIONABLE t;

  end P_Bac_get_actionable_snap;

  procedure P_Bac_get_actionable_sum(io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select T.MEETING_NUMBER,
             T.total,
             T.Completed,
             T.un_Completed,
             S.BAC_DIRECTIVES,
             S.RESPONSIBLE,
             S.RESPONSE,
             S.CIA_REMARKS

        from V_GET_BAC_ACTIONABLE_SUMMARY t
       INNER JOIN T_BAC_ACTIONABLE_UPDATES S
          ON T.MEETING_NUMBER = S.MEETING

       order by t.meeting_number;

  end P_Bac_get_actionable_sum;

  procedure P_Bac_get_actionable(status    in varchar2,
                                 io_cursor OUT t_cursor) is

  begin
    if (status = 'Completed') then
      open io_cursor for
        select a.id,
               a.meeting_number,
               a.item_heading,
               a.bac_direction,
               a.assign_to,
               a.time_line,
               a.open_time_line,
               a.due_date,
               a.rpt_frequency,
               a.entered_by,
               a.entered_on,
               a.delay,
               a.status
          from t_bac_actionable a
         where a.status = 'Completed';
    else
      open io_cursor for
        select a.id,
               a.meeting_number,
               a.item_heading,
               a.bac_direction,
               a.assign_to,
               a.time_line,
               a.open_time_line,
               a.due_date,
               a.rpt_frequency,
               a.entered_by,
               a.entered_on,
               a.delay,
               a.status
          from t_bac_actionable a
         where a.status != 'Completed';
    end if;
  end P_Bac_get_actionable;

  procedure P_Bac_get_actionable_meetings(meeting   in number,
                                          io_cursor OUT t_cursor) is

  begin

    open io_cursor for
      select a.id,
             a.meeting_number,
             a.item_heading,
             a.bac_direction,
             a.assign_to,
             a.time_line,
             a.open_time_line,
             a.due_date,
             a.rpt_frequency,
             a.entered_by,
             a.entered_on,
             a.delay,
             a.status
        from t_bac_actionable a
       where a.meeting_number = meeting;
  end P_Bac_get_actionable_meetings;

  procedure P_Bac_get_actionable_meetings_with_status(meeting   in number,
                                                      A_Status  in Varchar2,
                                                      io_cursor OUT t_cursor) is

  begin
    if (A_Status = 'All') then
      open io_cursor for
        select a.id,
               a.meeting_number,
               a.item_heading,
               a.bac_direction,
               a.assign_to,
               a.time_line,
               a.open_time_line,
               a.due_date,
               a.rpt_frequency,
               a.entered_by,
               a.entered_on,
               a.delay,
               a.status
          from t_bac_actionable a
         where a.meeting_number = meeting;
    else
      if (A_Status = 'Completed') then
        open io_cursor for
          select a.id,
                 a.meeting_number,
                 a.item_heading,
                 a.bac_direction,
                 a.assign_to,
                 a.time_line,
                 a.open_time_line,
                 a.due_date,
                 a.rpt_frequency,
                 a.entered_by,
                 a.entered_on,
                 a.delay,
                 a.status
            from t_bac_actionable a
           where a.meeting_number = meeting
             and a.status = 'Completed';
      else
        open io_cursor for
          select a.id,
                 a.meeting_number,
                 a.item_heading,
                 a.bac_direction,
                 a.assign_to,
                 a.time_line,
                 a.open_time_line,
                 a.due_date,
                 a.rpt_frequency,
                 a.entered_by,
                 a.entered_on,
                 a.delay,
                 a.status
            from t_bac_actionable a
           where a.meeting_number = meeting
             and a.status != 'Completed';
      end if;
    end if;

  end P_Bac_get_actionable_meetings_with_status;

  Procedure P_Bac_get_Actionable_assign_to_dept(User_entityid in number,
                                                io_cursor     OUT t_cursor) is

  begin
    open io_cursor for
      select *
        from T_BAC_ACTIONABLE_ASSIGN_DEPT S
       where s.dept_id = user_entityid;

  END P_Bac_get_Actionable_assign_to_dept;

  Procedure P_Bac_Actionable_response(Item_Number in number,
                                      DEPTID      in number,
                                      RESPONSE    in clob,
                                      attachement in blob,
                                      ppno        in number,
                                      Status      in varchar2,
                                      io_cursor   OUT t_cursor) is

  begin

    INSERT INTO T_BAC_ACTIONABLE_RESPONSE
      (ID,
       ITEM_NUMBER,
       Dept_ID,
       RESPONSE,
       EVIDENCE,
       ENTERED_BY,
       ENTERED_ON,
       STATUS)

    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_BAC_ACTIONABLE_RESPONSE acc),
       Item_Number,
       DEPTID,
       RESPONSE,
       attachement,
       ppno,
       sysdate,
       'Submitted');
    commit;
    open io_cursor for
      SELECT 'ACTIONABLE RESPONSE SUBMITTED' AS REF_OUT FROM DUAL;
  end P_Bac_Actionable_response;

  Procedure P_Bac_Actionable_response_reviewer(User_entityid in number,
                                               io_cursor     OUT t_cursor) is

  begin
    open io_cursor for
      select s.id,
             s.item_number,
             s.dept_id,
             s.response,
             s.evidence,
             s.entered_by,
             s.entered_on,
             s.status
        from T_BAC_ACTIONABLE_RESPONSE S
       inner join t_auditee_entities_maping p
          on s.dept_id = p.entity_id
       where p.parent_id = user_entityid;

  END P_Bac_Actionable_response_reviewer;

  Procedure P_BAC_AGENDA(Meeting in number, io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select e.id, e.meeting_no, e.memo_no, e.subject, e.remarks
        from T_BAC_MEETINGS_AGENDA e
       where e.meeting_no = meeting;

  end P_BAC_AGENDA;

  Procedure P_CIA_ANALYSIS(io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select E.ID, E.HEADING, E.AUDIT_COMMENTS, E.AUTOMATION, E.MONITORING
        from T_AUDIT_CHECKLIST_ANNEXURE_PROCESS e;
  END P_CIA_ANALYSIS;

  Procedure P_CIA_ANALYSIS_DETAILS(a_id in number, io_cursor OUT t_cursor) is

  begin
    if (a_id != 0) then
      open io_cursor for
        select a.id,
               'Y' as indicator,
               e.heading,
               a.code || '  ' || a.heading as annex,
               e.audit_comments,
               count(o.id) as total,
               sum(case
                     when o.para_category = 'O' then
                      1
                     else
                      0
                   end) as old_total,
               sum(case
                     when o.para_category = 'N' then
                      1
                     else
                      0
                   end) as new_total
          from T_AUDIT_CHECKLIST_ANNEXURE_PROCESS e
         inner join T_AUDIT_CHECKLIST_ANNEXURE_MAPPING m
            on e.id = m.process_id
         inner join T_AUDIT_CHECKLIST_ANNEXURE a
            on a.code = m.annex
         inner join v_cia_analysis o
            on o.annex = a.id
         where e.id = a_id
         group by a.id,
                  'indicator',
                  e.heading,
                  a.code,
                  a.heading,
                  e.audit_comments
         order by e.id;
    else
      open io_cursor for
        select a.id,
               'N' as indicator,
               e.heading,
               '' as annex,
               e.audit_comments,
               count(o.id) as total,
               sum(case
                     when o.para_category = 'O' then
                      1
                     else
                      0
                   end) as old_total,
               sum(case
                     when o.para_category = 'N' then
                      1
                     else
                      0
                   end) as new_total
          from T_AUDIT_CHECKLIST_ANNEXURE_PROCESS e
         inner join T_AUDIT_CHECKLIST_ANNEXURE_MAPPING m
            on e.id = m.process_id
         inner join T_AUDIT_CHECKLIST_ANNEXURE a
            on a.code = m.annex
         inner join v_cia_analysis o
            on o.annex = a.id
         group by a.id, e.id, 'indicator', e.heading, e.audit_comments
         order by e.id;
    end if;
  END P_CIA_ANALYSIS_DETAILS;

  Procedure P_CIA_ANALYSIS_DETAILS_PARA(A_ID      in number,
                                        R_ID      in number,
                                        ENT_ID    in number,
                                        P_NO      in number,
                                        io_cursor OUT t_cursor) is

    V_F   number := 0;
    RR_ID number := 0;
  begin
    select R_ID into RR_ID from dual;
    select max(nvl(en.auditby_id, 0))
      into V_F
      from t_auditee_entities en
     where en.entity_id = ent_id;

    open io_cursor for
      select gm.c_name as name,
             gm.auditedby as auditby_id,
             R_ID,
             V_F,
             o.audit_period,
             o.para_no,
             o.ind AS para_category,
             (CASE WHEN O.IND = 'O' then o.old_para_id else o.new_para_id end) as  id
        from T_AUDIT_CHECKLIST_ANNEXURE e
       inner join AIS_T_AU_POST_COMPLIANCE o
          on o.annex = e.id and o.para_status = 8
       inner join t_auditee_entities_maping gm
              on o.entity_id = gm.entity_id

       where e.id = A_ID
         and gm.gm_office = case
               when RR_ID = 39 then
                ENT_ID
               when RR_ID in (1,5, 7,40,41) then
                gm.gm_office
             end
       ORDER BY O.audit_period DESC;
  END P_CIA_ANALYSIS_DETAILS_PARA;

  Procedure P_CIA_ANALYSIS_DETAILS_PARA_TEXT(P_ID      in number,
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
      if (P_C = 'N') then
        open io_cursor for
          select t.headings, t.text as para_text
            from t_au_observation f
           inner join t_au_observation_text t
              on f.id = t.observatsion_id
           where f.id = P_ID;
      end if;
    end if;

  END P_CIA_ANALYSIS_DETAILS_PARA_TEXT;

  Procedure P_CIA_ANALYSIS_SUMMARY(A_ID      in number,
                                   R_ID      in number,
                                   ENT_ID    in number,
                                   io_cursor OUT t_cursor) is

    V_F   number := 0;
    RR_ID number := 0;
  begin
    select R_ID into RR_ID from dual;
    select max(nvl(en.auditby_id, 0))
      into V_F
      from t_auditee_entities en
     where en.entity_id = ent_id;

    open io_cursor for
      select m.p_name, et.name, o.audit_period, count(o.com_id) as para_no
        from T_AUDIT_CHECKLIST_ANNEXURE e

       inner join ais_t_au_post_compliance o
          on o.annex = e.id and o.para_status = 8
       inner join t_auditee_entities et
          on et.entity_id = o.entity_id
       inner join t_auditee_entities_maping m
          on m.entity_id = et.entity_id
         --and m.p_name is not null
       inner join t_auditee_entities_maping gm
              on o.entity_id = gm.entity_id

       where e.id = A_ID
         and gm.gm_office = case
               when RR_ID = 39 then
                ENT_ID
               when RR_ID in (1,5, 7,40,41) then
                gm.gm_office
             end
       group by m.p_name, et.name, o.audit_period
       order by m.p_name, et.name, o.audit_period;

  END P_CIA_ANALYSIS_SUMMARY;

  Procedure P_CIA_ANALYSIS_AUDIT_COMMENTS(a_id      in number,
                                          io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select E.AUDIT_COMMENTS, E.AUTOMATION, E.MONITORING
        from T_AUDIT_CHECKLIST_ANNEXURE_PROCESS e
       where e.id = a_id;
  END P_CIA_ANALYSIS_AUDIT_COMMENTS;

end PKG_BAC;

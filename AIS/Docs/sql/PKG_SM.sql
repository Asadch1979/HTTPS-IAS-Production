create or replace package PKG_SM is

  TYPE t_cursor IS REF CURSOR;

  Procedure P_GET_SAMPLE_ENTITIES(io_cursor OUT t_cursor);

  procedure P_Sample_update(E_ID      number,
                            SID       number,
                            P_NO      number,
                            ENT_ID    number,
                            R_ID      number,
                            io_cursor OUT t_cursor);

  Procedure P_GET_SAMPLE(E_ID      number,
                         P_NO      number,
                         ENT_ID    number,
                         R_ID      number,
                         io_cursor OUT t_cursor);

  PROCEDURE P_GET_BRANCH_ACCOUNTS(E_ID IN NUMBER, io_cursor OUT t_cursor);

  Procedure P_add_sample_data(E_ID      number,
                              P_NO      number,
                              ENT_ID    number,
                              R_ID      number,
                              io_cursor OUT t_cursor);

  procedure p_create_Sample(EID       number,
                            P_NO      number,
                            ENT_ID    number,
                            R_ID      number,
                            io_cursor OUT t_cursor);

  Procedure P_GET_ACCOUNT_DOC(AC_number number,
                              P_NO      number,
                              ENT_ID    number,
                              R_ID      number,
                              io_cursor OUT t_cursor);

  procedure p_get_Account(E_ID      number,
                          P_NO      number,
                          R_ID      number,
                          ENT_ID    number,
                          io_cursor OUT t_cursor);

  Procedure p_get_account_transcations(E_ID      number,
                                       AC_number number,
                                       P_NO      number,
                                       ENT_ID    number,
                                       R_ID      number,
                                       io_cursor OUT t_cursor);

  Procedure p_get_account_transcations_master(E_ID      number,
                                              AC_number number,
                                              CNIC_NO   number,
                                              ST_DATE   date,
                                              ED_DATE   date,
                                              P_NO      number,
                                              ENT_ID    number,
                                              R_ID      number,
                                              io_cursor OUT t_cursor);

  Procedure P_GET_LOANS_SAMPLE(S_ID      number,
                               LStatus   number,
                               E_ID      number,
                               P_NO      number,
                               ENT_ID    number,
                               R_ID      number,
                               io_cursor OUT t_cursor);

  Procedure P_Store_Sample_data(E_ID   number,
                                IND    varchar2,
                                P_NO   number,
                                ENT_ID number,
                                R_ID   number);

  procedure p_get_Loan_Transactions(E_ID      number,
                                    L_DISB_ID number,
                                    P_NO      number,
                                    ENT_ID    number,
                                    R_ID      number,
                                    io_cursor OUT t_cursor);

  procedure p_get_Loan_Documents(E_ID      number,
                                 L_DISB_ID number,
                                 P_NO      number,
                                 ENT_ID    number,
                                 R_ID      number,
                                 io_cursor OUT t_cursor);

  procedure p_get_Loan_Documents_image(image_ID  number,
                                       io_cursor OUT t_cursor);

  procedure p_get_biometric(CNIC_ID varchar2, io_cursor OUT t_cursor);

  Procedure P_add_sample_data_update(E_ID      number,
                                     SID       number,
                                     P_NO      number,
                                     ENT_ID    number,
                                     R_ID      number,
                                     io_cursor OUT t_cursor);

  procedure p_update_Sample(EID       number,
                            SD        number,
                            P_NO      number,
                            ENT_ID    number,
                            R_ID      number,
                            io_cursor OUT t_cursor);

  Procedure T_AU_EXCEPTION_REPORT(E_ID      number,
                                  P_NO      number,
                                  ENT_ID    number,
                                  R_ID      number,
                                  io_cursor OUT t_cursor);

  Procedure P_add_exception_data(E_ID      number,
                                 P_NO      number,
                                 R_ID      number,
                                 ENT_ID    number,
                                 io_cursor OUT t_cursor);

  Procedure P_PREP_EXCEPTION_BASE(E_ID IN NUMBER);

  PROCEDURE P_POP_EXCEPTION_CTR(E_ID IN NUMBER);

  PROCEDURE P_POP_EXCEPTION_DORMANT(E_ID IN NUMBER);

  PROCEDURE P_POP_EXCEPTION_HIGH_TRUN_OVER(E_ID IN NUMBER);

  PROCEDURE P_POP_EXCEPTION_CNIC_EXPIRY(E_ID IN NUMBER);

  PROCEDURE P_POP_EXCEPTION_NEG_BAL(E_ID IN NUMBER);

  PROCEDURE P_POP_EXCEPTION_EMP_ACCOUNTS(E_ID IN NUMBER);
  PROCEDURE P_POP_EXCEPTION_Cell_Change(E_ID IN NUMBER);
  PROCEDURE P_POP_EXCEPTION_PROFIT_AMOUNT(E_ID IN NUMBER);
  PROCEDURE P_POP_EXCEPTION_ZAKAT(E_ID IN NUMBER);

  Procedure P_Add_new_exp_report(IND          varchar2,
                                 REPORT_ID    number,
                                 REPORT_TITLE varchar2,
                                 DESCRIPTION  varchar2,
                                 R_TYPE       varchar2,
                                 L_Status     number,
                                 P_NO         number,
                                 R_ID         number,
                                 ENT_ID       number,
                                 io_cursor    OUT t_cursor);

  PROCEDURE P_GET_EXCEPTION_REPORT_FORMAT(P_R_ID    IN NUMBER,
                                          IO_CURSOR OUT SYS_REFCURSOR);

  PROCEDURE P_INSERT_EXCEPTION_REPORT_FORMAT(P_R_ID          IN NUMBER,
                                             P_COLUMN_NAME   IN VARCHAR2,
                                             P_COLUMN_HEADER IN VARCHAR2,
                                             P_COLUMN_ORDER  IN NUMBER,
                                             P_DATA_TYPE     IN VARCHAR2,
                                             O_FORMAT_ID     OUT NUMBER);

  PROCEDURE P_UPDATE_EXCEPTION_REPORT_FORMAT(P_FORMAT_ID     IN NUMBER,
                                             P_COLUMN_HEADER IN VARCHAR2,
                                             P_COLUMN_ORDER  IN NUMBER,
                                             P_DATA_TYPE     IN VARCHAR2,
                                             P_IS_ACTIVE     IN VARCHAR2);

  procedure P_GET_LOAN_STATUS(io_cursor OUT t_cursor);

  PROCEDURE P_GET_EXCEPTION_REPORT_DATA(P_R_ID     IN NUMBER,
                                        P_ENG_ID   IN NUMBER,
                                        IO_CURSOR1 OUT SYS_REFCURSOR, -- FORMAT
                                        IO_CURSOR2 OUT SYS_REFCURSOR -- DATA
                                        );

  PROCEDURE P_LOG_EXCEPTION_EVENT(p_eng_id          IN NUMBER,
                                  p_proc_name       IN VARCHAR2,
                                  p_status          IN VARCHAR2,
                                  p_error_message   IN VARCHAR2,
                                  p_error_stack     IN VARCHAR2,
                                  p_error_backtrace IN VARCHAR2,
                                  p_failed_step     IN VARCHAR2);

  PROCEDURE P_SEND_EXCEPTION_ALERT(p_eng_id IN NUMBER,
                                   p_step   IN VARCHAR2,
                                   p_errmsg IN VARCHAR2);

  PROCEDURE P_GET_EXCEPTION_MONITOR(io_cursor OUT t_cursor);

  PROCEDURE P_GET_EXCEPTION_MONITOR_ENTITIES(IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_REGENERATE_EXCEPTION(P_ENG_ID IN NUMBER, P_ER_ID IN NUMBER);

  PROCEDURE P_GET_EXCEPTION_MONITOR_DETAILS(P_ENG_ID  IN NUMBER,
                                            IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_LOANS_EXCEPTIONS(LStatus  IN NUMBER,
                                   E_ID     IN NUMBER,
                                   P_NO     IN NUMBER,
                                   R_ID     IN NUMBER,
                                   ENT_ID   IN NUMBER,
                                   T_CURSOR OUT SYS_REFCURSOR);
end PKG_SM;


create or replace package body PKG_SM is

  Procedure P_GET_SAMPLE_ENTITIES(io_cursor OUT t_cursor) is
  
  begin
  
    open io_cursor for
    
      select distinct et.name as E_NAME, e.eng_id
        from t_Au_Sample_Data d
       inner join t_au_plan_eng e
          on d.eg_id = e.eng_id
       inner join t_au_audit_team_tasklist t
          on t.eng_plan_id = e.eng_id
         and t.status_id = 2
       inner join t_auditee_entities et
          on et.entity_id = e.entity_id
         and et.type_id in (6, 28);
  
  end P_GET_SAMPLE_ENTITIES;

  procedure P_Sample_update(E_ID      number,
                            SID       number,
                            P_NO      number,
                            ENT_ID    number,
                            R_ID      number,
                            io_cursor OUT t_cursor) is
    E_D number := 0;
  begin
    E_D := E_ID;
    IF (R_ID not in (3)) then
      if (E_ID is null) then
      
        for c in (select distinct d.eg_id as eid from t_au_sample_data d) loop
        
          delete from t_au_sample_data d where d.eg_id = c.eid;
          commit;
          delete from t_au_sample_biometric b where b.eng_id = c.eid;
          commit;
          delete from t_Au_Sample_Branch bd where bd.eng_id = c.eid;
          commit;
          delete from t_sample_accounts_data l where l.eng_id = c.eid;
          commit;
          delete from t_sample_loans_data a where a.engid = c.eid;
          commit;
        
          pkg_sm.P_add_sample_data(E_ID      => c.eid,
                                   P_NO      => P_NO,
                                   ENT_ID    => ENT_ID,
                                   R_ID      => R_ID,
                                   io_cursor => io_cursor);
        
        end loop;
      else
        if (E_ID is not null and SID = 1) then
        
          delete from t_au_sample_data d
           where d.eg_id = E_ID
             and d.s_id = SID;
          commit;
          delete from t_Au_Sample_Branch bd
           where bd.eng_id = E_ID
             and bd.s_id = SID;
          commit;
          delete from t_sample_accounts_data l
           where l.eng_id = E_ID
             and l.s_d = SID;
          commit;
          delete from t_sample_loans_data a
           where a.engid = E_ID
             and a.s_d = SID;
          commit;
        
        else
          if (E_ID is not null and SID = 2) then
            delete from t_au_sample_data d
             where d.eg_id = E_ID
               and d.s_id = SID;
            commit;
            delete from t_Au_Sample_Branch bd
             where bd.eng_id = E_ID
               and bd.s_id = SID;
            commit;
            delete from t_sample_accounts_data l
             where l.eng_id = E_ID
               and l.s_d = SID;
            commit;
            delete from t_sample_loans_data a
             where a.engid = E_ID
               and a.s_d = SID;
            commit;
          
          else
            if (E_ID is not null and SID = 3) then
              delete from t_au_sample_data d
               where d.eg_id = E_ID
                 and d.s_id = SID;
              commit;
              delete from t_Au_Sample_Branch bd
               where bd.eng_id = E_ID
                 and bd.s_id = SID;
              commit;
              delete from t_sample_accounts_data l
               where l.eng_id = E_ID
                 and l.s_d = SID;
              commit;
              delete from t_sample_loans_data a
               where a.engid = E_ID
                 and a.s_d = SID;
              commit;
            
            else
              if (E_ID is not null and SID = 4) then
                delete from t_au_sample_data d
                 where d.eg_id = E_ID
                   and d.s_id = SID;
                commit;
                delete from t_Au_Sample_Branch bd
                 where bd.eng_id = E_ID
                   and bd.s_id = SID;
                commit;
                delete from t_sample_accounts_data l
                 where l.eng_id = E_ID
                   and l.s_d = SID;
                commit;
                delete from t_sample_loans_data a
                 where a.engid = E_ID
                   and a.s_d = SID;
                commit;
              
              else
                if (E_ID is not null and SID = 5) then
                  delete from t_au_sample_data d
                   where d.eg_id = E_ID
                     and d.s_id = SID;
                  commit;
                  delete from t_Au_Sample_Branch bd
                   where bd.eng_id = E_ID
                     and bd.s_id = SID;
                  commit;
                  delete from t_sample_accounts_data l
                   where l.eng_id = E_ID
                     and l.s_d = SID;
                  commit;
                  delete from t_sample_loans_data a
                   where a.engid = E_ID
                     and a.s_d = SID;
                  commit;
                
                else
                  if (E_ID is not null and SID = 6) then
                    delete from t_au_sample_data d
                     where d.eg_id = E_ID
                       and d.s_id = SID;
                    commit;
                    delete from t_Au_Sample_Branch bd
                     where bd.eng_id = E_ID
                       and bd.s_id = SID;
                    commit;
                    delete from t_sample_accounts_data l
                     where l.eng_id = E_ID
                       and l.s_d = SID;
                    commit;
                    delete from t_sample_loans_data a
                     where a.engid = E_ID
                       and a.s_d = SID;
                    commit;
                  
                  else
                    if (E_ID is not null and SID = 7) then
                      delete from t_au_sample_data d
                       where d.eg_id = E_ID
                         and d.s_id = SID;
                      commit;
                      delete from t_Au_Sample_Branch bd
                       where bd.eng_id = E_ID
                         and bd.s_id = SID;
                      commit;
                      delete from t_sample_accounts_data l
                       where l.eng_id = E_ID
                         and l.s_d = SID;
                      commit;
                      delete from t_sample_loans_data a
                       where a.engid = E_ID
                         and a.s_d = SID;
                      commit;
                    
                    else
                      if (E_ID is not null and SID = 8) then
                        delete from t_au_sample_data d
                         where d.eg_id = E_ID
                           and d.s_id = SID;
                        commit;
                        delete from t_Au_Sample_Branch bd
                         where bd.eng_id = E_ID
                           and bd.s_id = SID;
                        commit;
                        delete from t_sample_accounts_data l
                         where l.eng_id = E_ID
                           and l.s_d = SID;
                        commit;
                        delete from t_sample_loans_data a
                         where a.engid = E_ID
                           and a.s_d = SID;
                        commit;
                      
                      else
                        if (E_ID is not null and SID = 9) then
                          delete from t_au_sample_data d
                           where d.eg_id = E_ID
                             and d.s_id = SID;
                          commit;
                          delete from t_Au_Sample_Branch bd
                           where bd.eng_id = E_ID
                             and bd.s_id = SID;
                          commit;
                          delete from t_sample_accounts_data l
                           where l.eng_id = E_ID
                             and l.s_d = SID;
                          commit;
                          delete from t_sample_loans_data a
                           where a.engid = E_ID
                             and a.s_d = SID;
                          commit;
                        
                        else
                          if (E_ID is not null and SID = 10) then
                            delete from t_au_sample_data d
                             where d.eg_id = E_ID
                               and d.s_id = SID;
                            commit;
                            delete from t_Au_Sample_Branch bd
                             where bd.eng_id = E_ID
                               and bd.s_id = SID;
                            commit;
                            delete from t_sample_accounts_data l
                             where l.eng_id = E_ID
                               and l.s_d = SID;
                            commit;
                            delete from t_sample_loans_data a
                             where a.engid = E_ID
                               and a.s_d = SID;
                            commit;
                          
                          else
                            if (E_ID is not null and SID = 11) then
                              delete from t_au_sample_data d
                               where d.eg_id = E_ID
                                 and d.s_id = SID;
                              commit;
                              delete from t_Au_Sample_Branch bd
                               where bd.eng_id = E_ID
                                 and bd.s_id = SID;
                              commit;
                              delete from t_sample_accounts_data l
                               where l.eng_id = E_ID
                                 and l.s_d = SID;
                              commit;
                              delete from t_sample_loans_data a
                               where a.engid = E_ID
                                 and a.s_d = SID;
                              commit;
                            
                            else
                              if (E_ID is not null and SID = 12) then
                                delete from t_au_sample_data d
                                 where d.eg_id = E_ID
                                   and d.s_id = SID;
                                commit;
                                delete from t_Au_Sample_Branch bd
                                 where bd.eng_id = E_ID
                                   and bd.s_id = SID;
                                commit;
                                delete from t_sample_accounts_data l
                                 where l.eng_id = E_ID
                                   and l.s_d = SID;
                                commit;
                                delete from t_sample_loans_data a
                                 where a.engid = E_ID
                                   and a.s_d = SID;
                                commit;
                              
                              else
                                if (E_ID is not null and SID = 13) then
                                  delete from t_au_sample_data d
                                   where d.eg_id = E_ID
                                     and d.s_id = SID;
                                  commit;
                                  delete from t_Au_Sample_Branch bd
                                   where bd.eng_id = E_ID
                                     and bd.s_id = SID;
                                  commit;
                                  delete from t_sample_accounts_data l
                                   where l.eng_id = E_ID
                                     and l.s_d = SID;
                                  commit;
                                  delete from t_sample_loans_data a
                                   where a.engid = E_ID
                                     and a.s_d = SID;
                                  commit;
                                
                                  pkg_sm.P_add_sample_data(E_ID      => E_D,
                                                           P_NO      => P_NO,
                                                           ENT_ID    => ENT_ID,
                                                           R_ID      => R_ID,
                                                           io_cursor => io_cursor);
                                
                                end if;
                              end if;
                            end if;
                          end if;
                        end if;
                      end if;
                    end if;
                  end if;
                end if;
              end if;
            end if;
          end if;
        end if;
      end if;
    else
      Open io_cursor for
        Select 'You havent got rights to Regenerate Sample, Please Contact FAD' as Remarks
          from dual;
    end if;
  end P_Sample_update;

  Procedure P_GET_SAMPLE(E_ID      number,
                         P_NO      number,
                         ENT_ID    number,
                         R_ID      number,
                         io_cursor OUT t_cursor) is
  
  begin
  
    open io_cursor for
    
      select s.s_id,
             t.sample_type,
             s.samp_tot,
             s.sample_percentage,
             s.sample_final,
             t.loan_status,
             T.IND
        from T_AU_SAMPLE t
       inner join t_au_sample_data s
          on t.id = s.s_id
       where s.eg_id = E_ID;
  
  end P_GET_SAMPLE;

  PROCEDURE P_GET_BRANCH_ACCOUNTS(E_ID IN NUMBER, io_cursor OUT t_cursor)
  
   AS
  BEGIN
    OPEN io_cursor FOR
      SELECT b.branchcode,
             b.accountid,
             b.oldaccountno,
             b.name,
             b.customername,
             b.dob,
             b.phonecell,
             b.cnic,
             b.cnicexpirydate,
             b.openingdate,
             b.bmvs_verified,
             b.purpose,
             b.acc_type,
             b.acc_category,
             b.risk
        FROM V_BRANCH_ACCOUNTS b
       inner join t_au_plan_eng ep
          on ep.branch_code = b.branchcode
         and b.openingdate between ep.audit_startdate and ep.audit_enddate
      
       WHERE ep.eng_id = E_ID;
  
  END P_GET_BRANCH_ACCOUNTS;

  Procedure P_add_sample_data(E_ID      number,
                              P_NO      number,
                              ENT_ID    number,
                              R_ID      number,
                              io_cursor OUT t_cursor) is
  
    S_E      number := 0;
    B_E      number := 0;
    P_F      number := 0;
    D_F      number := 0;
    Z_F      number := 0;
    C_F      number := 0;
    L_F      number := 0;
    S_F      number := 0;
    OE_F     number := 0;
    SB_F     number := 0;
    DB_F     number := 0;
    LB_F     number := 0;
    A_F      number := 0;
    ENT_RISK number := 0;
  begin
    select e.risk_id
      into ENT_RISK
      from t_auditee_entities e
     inner join t_au_plan_eng ep
        on e.entity_id = ep.entity_id
     where ep.eng_id = E_ID;
    select count(*) into A_F from V_SAM_ACCOUNT n where n.eng_id = E_ID;
    select count(*)
      into B_E
      from v_sampling_out_loans_status n
     where n.eng_id = E_ID
       and n.disb_statusid not in (11, 12);
    select count(*)
      into Z_F
      from v_sampling_zero_recovery n
     where n.eng_id = E_ID;
    select count(*)
      into C_F
      from v_sampling_closed_loans n1
     where n1.eng_id = E_ID;
    select count(*)
      into L_F
      from v_sampling_Disb_loans n2
     where n2.eng_id = E_ID;
    select count(*)
      into P_F
      from v_sampling_Prod_loans n3
     where n3.eng_id = E_ID;
    select count(*)
      into D_F
      from v_sampling_Devp_loans n4
     where n4.eng_id = E_ID;
    select count(*)
      into S_F
      from v_sampling_out_loans_status n5
     where n5.eng_id = E_ID
       and n5.disb_statusid = 17;
    select count(*)
      into OE_F
      from v_sampling_out_loans_status n5
     where n5.eng_id = E_ID
       and n5.disb_statusid = 13;
    select count(*)
      into SB_F
      from v_sampling_out_loans_status n5
     where n5.eng_id = E_ID
       and n5.disb_statusid = 14;
    select count(*)
      into DB_F
      from v_sampling_out_loans_status n5
     where n5.eng_id = E_ID
       and n5.disb_statusid = 15;
    select count(*)
      into LB_F
      from v_sampling_out_loans_status n5
     where n5.eng_id = E_ID
       and n5.disb_statusid = 16;
  
    select nvl(max(s.id), 0)
      into S_E
      from T_AU_SAMPLE_data s
     where s.eg_id = E_ID;
  
    if (S_E = 0) then
      if (ENT_RISK = 3) then
        insert into T_AU_SAMPLE_data
          (Eg_Id, s_Id, Sample_Percentage)
          select e_id, se.id, se.sample_percentage from t_au_sample se;
        commit;
      ElSIF (ENT_RISK = 2) then
        insert into T_AU_SAMPLE_data
          (Eg_Id, s_Id, Sample_Percentage)
          select e_id, se.id, se.sample_percentage_medium
            from t_au_sample se;
        commit;
      ElSIF (ENT_RISK = 1) then
        insert into T_AU_SAMPLE_data
          (Eg_Id, s_Id, Sample_Percentage)
          select e_id, se.id, se.sample_percentage_high
            from t_au_sample se;
        commit;
      end if;
    
    end if;
  
    update t_au_sample_data s
       set s.samp_tot = A_F
     where s.eg_id = E_id
       and s.S_id = 1;
    commit;
    update t_au_sample_data s
       set s.samp_tot = B_E
     where s.eg_id = E_id
       and s.S_id = 2;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = Z_F
     where s.eg_id = E_id
       and s.S_id = 4;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = C_F
     where s.eg_id = E_id
       and s.S_id = 3;
    commit;
    update t_au_sample_data s
       set s.samp_tot = L_F
     where s.eg_id = E_id
       and s.S_id in (12);
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = P_F
     where s.eg_id = E_id
       and s.S_id = 10;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = D_F
     where s.eg_id = E_id
       and s.S_id = 11;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = S_F
     where s.eg_id = E_id
       and s.S_id = 5;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = OE_F
     where s.eg_id = E_id
       and s.S_id = 6;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = DB_F
     where s.eg_id = E_id
       and s.S_id = 7;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = SB_F
     where s.eg_id = E_id
       and s.S_id = 8;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = LB_F
     where s.eg_id = E_id
       and s.S_id = 9;
    commit;
  
    update t_au_sample_data s
       set s.samp_tot = A_F
     where s.eg_id = E_id
       and s.S_id = 13;
    commit;
  
    select nvl(max(s.id), 0)
      into S_E
      from T_AU_SAMPLE_data s
     where s.eg_id = E_ID
       and s.s_id = 12;
  
    if (S_E < 20) then
      update t_au_sample_data d
         set d.sample_final = 20
       where d.s_id = 12
         and d.eg_id = E_ID;
      commit;
    end if;
  
    update t_au_sample_data s
       set s.sample_final = round(s.samp_tot * s.sample_percentage / 100)
     where s.eg_id = E_id;
    commit;
  
    update t_au_sample_data ds
       set ds.sample_final = 1
     where ds.sample_final = 0
       and ds.eg_id = E_ID;
    commit;
  
    select nvl(max(s.id), 0)
      into S_E
      from T_AU_SAMPLE_data s
     where s.eg_id = E_ID
       and s.sample_final is not null;
  
    if (S_E is not null) then
      open io_cursor for
        Select 'Y' as remarks from dual;
    else
      open io_cursor for
        Select 'N' as remarks from dual;
    
    end if;
  
    pkg_sm.p_create_Sample(EID       => E_ID,
                           P_NO      => P_NO,
                           ENT_ID    => ENT_ID,
                           R_ID      => R_ID,
                           io_cursor => io_cursor);
  
  end P_add_sample_data;

  procedure p_create_Sample(EID       number,
                            P_NO      number,
                            ENT_ID    number,
                            R_ID      number,
                            io_cursor OUT t_cursor) is
    B_F number := 0;
    S_F number := 0;
  begin
  
    select nvl(max(b.id), 0)
      into S_F
      from T_AU_SAMPLE_BRANCH b
     where b.eng_id = EID;
    if (S_F = 0) then
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 1
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, AC_NO, IND)
      
        SELECT B.eng_id, 1, B.oldaccountno, 'A'
        
          FROM V_SAM_BIOMET B
         WHERE B.eng_id = EID
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.ac_no = b.oldaccountno)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 2
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_CASE, LOAN_ID, DISB_ID, IND)
      
        SELECT B.eng_id,
               2,
               B.loan_case_no,
               b.loan_app_id,
               b.loan_disb_id,
               'L'
        
          FROM v_sampling_out_loans_status B
         WHERE B.eng_id = EID
           and b.disb_statusid in (13, 14, 15, 16)
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.loan_case = b.loan_case_no)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 3
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND)
      
        SELECT B.eng_id, 3, b.loan_app_id, b.loan_disb_id, 'L'
        
          FROM v_sampling_closed_loans B
         WHERE B.eng_id = EID
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 4
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND)
      
        SELECT B.eng_id, 4, b.loan_app_id, b.loan_disb_id, 'L'
        
          FROM v_sampling_zero_recovery B
         INNER JOIN T_AU_PLAN_ENG E
            ON E.ENG_ID = B.ENG_ID
         WHERE B.eng_id = EID
           AND B.disb_date < E.OPERATION_STARTDATE
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 5
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, L_STATUS)
      
        SELECT B.eng_id, 5, b.loan_app_id, b.loan_disb_id, 'L', 17
        
          FROM v_sampling_out_loans_status B
         WHERE B.eng_id = EID
           and b.disb_statusid = 17
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 6
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, L_STATUS)
      
        SELECT B.eng_id, 6, b.loan_app_id, b.loan_disb_id, 'L', 13
        
          FROM v_sampling_out_loans_status B
         WHERE B.eng_id = EID
           and b.disb_statusid = 13
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 7
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, L_STATUS)
      
        SELECT B.eng_id, 7, b.loan_app_id, b.loan_disb_id, 'L', 14
        
          FROM v_sampling_out_loans_status B
         WHERE B.eng_id = EID
           and b.disb_statusid = 14
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 8
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, L_STATUS)
      
        SELECT B.eng_id, 8, b.loan_app_id, b.loan_disb_id, 'L', 15
        
          FROM v_sampling_out_loans_status B
         WHERE B.eng_id = EID
           and b.disb_statusid = 15
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 9
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, L_STATUS)
      
        SELECT B.eng_id, 9, b.loan_app_id, b.loan_disb_id, 'L', 16
        
          FROM v_sampling_out_loans_status B
         WHERE B.eng_id = EID
           and b.disb_statusid = 16
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 10
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND)
      
        SELECT B.eng_id, 10, b.loan_app_id, b.loan_disb_id, 'L'
        
          FROM v_sampling_Prod_loans B
         WHERE B.eng_id = EID
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 11
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND)
      
        SELECT B.eng_id, 11, b.loan_app_id, b.loan_disb_id, 'L'
        
          FROM v_sampling_devp_loans B
         WHERE B.eng_id = EID
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 12
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, L_STATUS)
      
        SELECT B.eng_id, 12, b.loan_app_id, b.loan_disb_id, 'L', 11
        
          FROM v_sampling_Disb_loans B
         WHERE B.eng_id = EID
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.disb_id = b.loan_disb_id)
           and rownum <= B_F;
      commit;
    
      select d.sample_final
        into B_F
        from t_au_sample_data d
       where d.s_id = 13
         and d.eg_id = EID;
    
      insert into T_AU_SAMPLE_BRANCH
        (ENG_ID, S_ID, AC_NO, IND)
      
        SELECT B.eng_id, 13, b.oldaccountno, 'A'
        
          FROM V_SAM_ACCOUNT B
         WHERE B.eng_id = EID
           and not exists (select 'z'
                  from T_AU_SAMPLE_BRANCH sm
                 where sm.ac_no = b.oldaccountno)
           and rownum <= B_F;
      commit;
    
      open io_cursor for
        Select 'Y' as remarks,
               --'Sample Has been Created' as remarks,
               'asad.chaudhry@ztnl.com.pk' as email,
               '' as email_cc
          from dual;
    else
      open io_cursor for
        Select 'N' as remarks,
               --'Sample Creation Failed' as remarks,
               'asad.chaudhry@ztnl.com.pk' as email,
               '' as email_cc
          from dual;
    end if;
  
    pkg_sm.P_Store_Sample_data(E_ID   => EID,
                               IND    => 'Y',
                               P_NO   => P_NO,
                               ENT_ID => ENT_ID,
                               R_ID   => R_ID);
  
    pkg_sm.P_add_exception_data(E_ID      => EID,
                                P_NO      => P_NO,
                                R_ID      => R_ID,
                                ENT_ID    => ENT_ID,
                                io_cursor => io_cursor);
  
  end p_create_Sample;

  Procedure P_GET_ACCOUNT_DOC(AC_number number,
                              P_NO      number,
                              ENT_ID    number,
                              R_ID      number,
                              io_cursor OUT t_cursor) is
  
  begin
    open io_cursor for
      select 0 OLDACCOUNTNO,
             0 PAGENO,
             'a' name,
             '' doc_image,
             '' doc_remarks
      
        from dual;
  
  end P_GET_ACCOUNT_DOC;

  procedure p_get_Account(E_ID      number,
                          P_NO      number,
                          R_ID      number,
                          ENT_ID    number,
                          io_cursor OUT t_cursor) is
  begin
    open io_cursor for
    
      select sm.eng_id as branchcode,
             sm.oldaccountno,
             sm.name,
             sm.customername,
             
             sm.dob,
             to_char(trunc(sm.dob),
                     'DD-Mon-YYYY',
                     'NLS_DATE_LANGUAGE=ENGLISH') as dob_disp,
             
             sm.phonecell,
             sm.cnic,
             
             sm.cnicexpirydate,
             to_char(trunc(sm.cnicexpirydate),
                     'DD-Mon-YYYY',
                     'NLS_DATE_LANGUAGE=ENGLISH') as cnicexpirydate_disp,
             
             sm.openingdate,
             to_char(trunc(sm.openingdate),
                     'DD-Mon-YYYY',
                     'NLS_DATE_LANGUAGE=ENGLISH') as openingdate_disp,
             
             sm.bmvs_verified,
             sm.purpose,
             sm.acc_type,
             sm.acc_category,
             sm.risk
      
        from t_sample_accounts_data sm
       inner join T_AU_SAMPLE_BRANCH b
          on b.ac_no = sm.oldaccountno
       where b.eng_id = E_ID;
  
  end p_get_Account;

  Procedure p_get_account_transcations(E_ID      number,
                                       AC_number number,
                                       P_NO      number,
                                       ENT_ID    number,
                                       R_ID      number,
                                       io_cursor OUT t_cursor) is
  begin
    open io_cursor for
    /*select m.transactionmastercode,
                                     m.description,
                                     TR.REMARKS,
                                     tr.transactiondate,
                                     tr.transactiondate      as transactiondate_disp,
                                     tr.authorizationdate,
                                     tr.authorizationdate    as authorizationdate_disp,
                                     tr.dramount,
                                     tr.cramount,
                                     tr.toaccountid,
                                     tr.toaccounttitle,
                                     tr.toaccountno,
                                     tr.to_acc_branchid,
                                     tr.instrumentno
                                from branch.t_transaction  tr
                               inner join branch.t_account  a
                                  on a.accountid = tr.accountid
                               inner join branch.t_customeraccounts  ca
                                  on ca.accountid = tr.accountid
                               inner join branch.t_customer  c
                                  on c.customerid = ca.customerid
                               inner join branch.t_branch@proddb.l1.local b
                                  on a.branchid = b.branchid
                               inner join branch.t_transactionmaster  m
                                  on m.transactionmasterid = tr.transactionmasterid
                               inner join t_auditee_entities e
                                  on cast(e.code as varchar2(50)) = b.branchcode
                               inner join t_au_plan_eng ep
                                  on ep.entity_id = e.entity_id
                               where trunc(tr.transactiondate) between ep.operation_startdate and
                                     ep.operation_enddate
                                 and ep.eng_id = e_id
                                 and a.oldaccountno = AC_number
                               order by tr.authorizationdate;*/
      select a.*
        from v_p_get_account_transcations a
       where a.eng_id = e_id
         and a.oldaccountno = AC_number;
  
  end p_get_account_transcations;

  Procedure p_get_account_transcations_master(E_ID      number,
                                              AC_number number,
                                              CNIC_NO   number,
                                              ST_DATE   date,
                                              ED_DATE   date,
                                              P_NO      number,
                                              ENT_ID    number,
                                              R_ID      number,
                                              io_cursor OUT t_cursor) is
    N_C number := 0;
  begin
    n_C := CNIC_NO;
    if (AC_number != NULL or AC_number != 0) THEN
      open io_cursor for
      /*select distinct tr.transactionid,
                                                            b.name                  as b_name,
                                                            a.oldaccountno,
                                                            c.cnic,
                                                            a.name                  as title,
                                                            c.customername,
                                                            m.transactionmastercode,
                                                            m.description,
                                                            TR.REMARKS,
                                                            tr.transactiondate,
                                                            tr.authorizationdate,
                                                            tr.dramount,
                                                            tr.cramount,
                                                            tr.toaccountid,
                                                            tr.toaccounttitle,
                                                            tr.toaccountno,
                                                            tr.to_acc_branchid,
                                                            tr.instrumentno
                                              from branch.t_transaction@proddb.l1.local tr
                                             inner join branch.t_account@proddb.l1.local a
                                                on a.accountid = tr.accountid
                                             inner join branch.t_customeraccounts@proddb.l1.local ca
                                                on ca.accountid = tr.accountid
                                             inner join branch.t_customer@proddb.l1.local c
                                                on c.customerid = ca.customerid
                                             inner join branch.t_branch@proddb.l1.local b
                                                on a.branchid = b.branchid
                                             inner join branch.t_transactionmaster@proddb.l1.local m
                                                on m.transactionmasterid = tr.transactionmasterid
                                             inner join t_auditee_entities e
                                                on b.branchcode = e.code
                                             where trunc(tr.transactiondate) between ST_DATE and ED_DATE
                                               and a.oldaccountno = AC_number
                                               and e.entity_id = E_ID
                                             order by tr.authorizationdate*/
        select *
          from v_p_get_account_transcations_master m
         where m.entity_id = E_ID
        
        ;
    ELSE
      IF (N_C != NULL or N_C != 0) THEN
        open io_cursor for
        /* select distinct tr.transactionid,
                                                                          b.name                  as b_name,
                                                                          a.oldaccountno,
                                                                          c.cnic,
                                                                          a.name                  as title,
                                                                          c.customername,
                                                                          m.transactionmastercode,
                                                                          m.description,
                                                                          TR.REMARKS,
                                                                          tr.transactiondate,
                                                                          tr.authorizationdate,
                                                                          tr.dramount,
                                                                          tr.cramount,
                                                                          tr.toaccountid,
                                                                          tr.toaccounttitle,
                                                                          tr.toaccountno,
                                                                          tr.to_acc_branchid,
                                                                          tr.instrumentno
                                                            from branch.t_transaction@proddb.l1.local tr
                                                           inner join branch.t_account@proddb.l1.local a
                                                              on a.accountid = tr.accountid
                                                           inner join branch.t_customeraccounts@proddb.l1.local ca
                                                              on ca.accountid = tr.accountid
                                                           inner join branch.t_customer@proddb.l1.local c
                                                              on c.customerid = ca.customerid
                                                           inner join branch.t_branch@proddb.l1.local b
                                                              on a.branchid = b.branchid
                                                           inner join branch.t_transactionmaster@proddb.l1.local m
                                                              on m.transactionmasterid = tr.transactionmasterid
                                                           inner join t_auditee_entities e
                                                              on b.branchcode = e.code
                                                           where c.cnic = CNIC_NO
                                                             and trunc(tr.transactiondate) between trunc(ST_DATE) and
                                                                 trunc(ED_DATE)
                                                             and e.entity_id = E_ID
                                                           order by tr.authorizationdate*/
          select *
            from v_p_get_account_transcations_master m
           where m.entity_id = E_ID;
      ELSE
        open io_cursor for
        /*select distinct tr.transactionid,
                                                                          b.name                  as b_name,
                                                                          a.oldaccountno,
                                                                          c.cnic,
                                                                          a.name                  as title,
                                                                          c.customername,
                                                                          m.transactionmastercode,
                                                                          m.description,
                                                                          TR.REMARKS,
                                                                          tr.transactiondate,
                                                                          tr.authorizationdate,
                                                                          tr.dramount,
                                                                          tr.cramount,
                                                                          tr.toaccountid,
                                                                          tr.toaccounttitle,
                                                                          tr.toaccountno,
                                                                          tr.to_acc_branchid,
                                                                          tr.instrumentno
                                                            from branch.t_transaction@proddb.l1.local tr
                                                           inner join branch.t_account@proddb.l1.local a
                                                              on a.accountid = tr.accountid
                                                           inner join branch.t_customeraccounts@proddb.l1.local ca
                                                              on ca.accountid = tr.accountid
                                                           inner join branch.t_customer@proddb.l1.local c
                                                              on c.customerid = ca.customerid
                                                           inner join branch.t_branch@proddb.l1.local b
                                                              on a.branchid = b.branchid
                                                           inner join branch.t_transactionmaster@proddb.l1.local m
                                                              on m.transactionmasterid = tr.transactionmasterid
                                                           inner join t_auditee_entities e
                                                              on b.branchcode = e.code
                                                           where trunc(tr.transactiondate) = trunc(ST_DATE)
                                                             and e.entity_id = E_ID
                                                           order by tr.authorizationdate*/
          select *
            from v_p_get_account_transcations_master m
           where m.entity_id = E_ID;
      
      END IF;
    END IF;
  
  end p_get_account_transcations_master;

  PROCEDURE P_GET_LOANS_SAMPLE(S_ID      NUMBER,
                               LStatus   NUMBER,
                               E_ID      NUMBER,
                               P_NO      NUMBER,
                               ENT_ID    NUMBER,
                               R_ID      NUMBER,
                               io_cursor OUT t_cursor) IS
  BEGIN
  
    OPEN io_cursor FOR
      SELECT l.loan_disb_id,
             l.type,
             l.scheme,
             l.l_purpose,
             l.loan_case_no AS lc_no,
             l.cnic,
             l.customername,
             
             l.app_date,
             TO_CHAR(TRUNC(l.app_date),
                     'DD-Mon-YYYY',
                     'NLS_DATE_LANGUAGE=ENGLISH') AS app_date_disp,
             
             l.disb_date,
             TO_CHAR(TRUNC(l.disb_date),
                     'DD-Mon-YYYY',
                     'NLS_DATE_LANGUAGE=ENGLISH') AS disb_date_disp,
             
             l.dev_amount,
             l.outstanding
        FROM t_sample_loans_data l
       WHERE l.engid = e_id
         AND l.s_d = s_id;
  
  END P_GET_LOANS_SAMPLE;

  Procedure P_Store_Sample_data(E_ID   number,
                                IND    varchar2,
                                P_NO   number,
                                ENT_ID number,
                                R_ID   number) is
  
  begin
    if (IND = 'Y') then
    
      FOR J IN (select d.loan_disb_id,
                       B.ENG_ID,
                       B.S_ID,
                       c.cnic,
                       c.customername,
                       (case
                         when dp.prod_dev = 'P' then
                          'Production'
                         when dp.prod_dev = 'D' then
                          'Development'
                         else
                          ''
                       end) as Type,
                       sk.description as scheme,
                       gl.glsubname as l_purpose,
                       dp.crop_id as l_crop,
                       gl.glsubcode,
                       dp.cultivated_area,
                       dp.market_values,
                       dp.quantity,
                       dp.total_estimated_cost,
                       dp.prod_dev,
                       dp.loan_app_id,
                       dp.required_item_id,
                       dp.purpose_id,
                       dp.sub_proposal_id,
                       dp.entered_by,
                       dp.entered_date,
                       dp.amount_recommedned,
                       dp.scheme_id,
                       dp.mark_cal_mode,
                       dp.installment_frequency,
                       dp.total_installment,
                       dp.consent_insurance_opt_crop,
                       lp.mco_ppno,
                       lp.manager_ppno,
                       lp.dev_amount,
                       lp.prod_amount,
                       lp.tot_sanctioned_amount,
                       lp.tot_max_credit_limit,
                       lp.no_of_products,
                       lp.no_of_securities,
                       lp.tot_land,
                       lp.circle_code,
                       lp.loan_case_no,
                       lp.app_date,
                       lp.app_status_change_date,
                       lp.app_status_change_reason,
                       lp.sanction_date,
                       d.disb_date,
                       d.disb_statusid as l_status,
                       d.valid_until,
                       ((d.disbursed_amount - d.recovered_principal) +
                       ceil(d.todate_markup - d.recov_markup)) as Outstanding
                
                  from atas.t_ln_loan_app@proddb.l1.local lp
                
                 inner join ATAS.t_Ln_Production_Dev_Loan@PRODDB.L1.LOCAL dp
                    on dp.loan_app_id = lp.loan_app_id
                   and dp.branch_id = lp.branch_id
                
                 inner join atas.t_ln_customer_loan_app@proddb.l1.local cl
                    on lp.loan_app_id = cl.loan_app_id
                   and cl.branch_id = lp.branch_id
                
                 inner join atas.t_customer@proddb.l1.local c
                    ON cl.customer_id = c.customerid
                   and c.branchid = cl.branch_id
                
                 inner join atas.t_glsub@proddb.l1.local gl
                    on gl.glsubid = dp.gl_sub_id
                   and gl.branchid = lp.branch_id
                
                 inner join atas.t_ln_disbursement@PRODDB.L1.LOCAL d
                    on d.loan_app_id = lp.loan_app_id
                   and lp.branch_id = d.org_unitid
                   and d.glsubid = dp.gl_sub_id
                
                 inner join atas.cm_scheme@proddb.l1.local sk
                    on d.scheme_id = sk.scheme_id
                
                 inner join T_AU_SAMPLE_BRANCH b
                    on D.loan_disb_id = b.disb_id
                   and b.eng_id = E_ID
                 where not exists (select 'z'
                          from T_SAMPLE_loans_data sd
                         where sd.loan_disb_id = b.disb_id)) LOOP
      
        insert into T_SAMPLE_loans_data
          (Loan_Disb_Id,
           Engid,
           S_D,
           Cnic,
           Customername,
           Type,
           Scheme,
           l_Purpose,
           l_Crop,
           Glsubcode,
           Cultivated_Area,
           Market_Values,
           Quantity,
           Total_Estimated_Cost,
           Prod_Dev,
           Loan_App_Id,
           Required_Item_Id,
           Purpose_Id,
           Sub_Proposal_Id,
           Entered_By,
           Entered_Date,
           Amount_Recommedned,
           Scheme_Id,
           Mark_Cal_Mode,
           Installment_Frequency,
           Total_Installment,
           Consent_Insurance_Opt_Crop,
           Mco_Ppno,
           Manager_Ppno,
           Dev_Amount,
           Prod_Amount,
           Tot_Sanctioned_Amount,
           Tot_Max_Credit_Limit,
           No_Of_Products,
           No_Of_Securities,
           Tot_Land,
           Circle_Code,
           Loan_Case_No,
           App_Date,
           App_Status_Change_Date,
           App_Status_Change_Reason,
           Sanction_Date,
           Disb_Date,
           l_Status,
           Valid_Until,
           Outstanding)
        VALUES
          (J.loan_disb_id,
           J.ENG_ID,
           J.S_ID,
           J.cnic,
           J.customername,
           J.Type,
           J.scheme,
           J.l_purpose,
           J.l_crop,
           J.glsubcode,
           J.cultivated_area,
           J.market_values,
           J.quantity,
           J.total_estimated_cost,
           J.prod_dev,
           J.loan_app_id,
           J.required_item_id,
           J.purpose_id,
           J.sub_proposal_id,
           J.entered_by,
           J.entered_date,
           J.amount_recommedned,
           J.scheme_id,
           J.mark_cal_mode,
           J.installment_frequency,
           J.total_installment,
           J.consent_insurance_opt_crop,
           J.mco_ppno,
           J.manager_ppno,
           J.dev_amount,
           J.prod_amount,
           J.tot_sanctioned_amount,
           J.tot_max_credit_limit,
           J.no_of_products,
           J.no_of_securities,
           J.tot_land,
           J.circle_code,
           J.loan_case_no,
           J.app_date,
           J.app_status_change_date,
           J.app_status_change_reason,
           J.sanction_date,
           J.disb_date,
           J.l_status,
           J.valid_until,
           J.Outstanding);
      
        commit;
      END LOOP;
    
      for i in ( /*SELECT bb.eng_id,
                                                                                                                       b.branchcode,
                                                                                                                       a.accountid,
                                                                                                                       a.oldaccountno,
                                                                                                                       a.name,
                                                                                                                       c.customername,
                                                                                                                       c.dob,
                                                                                                                       c.phonecell,
                                                                                                                       c.cnic,
                                                                                                                       c.cnicexpirydate,
                                                                                                                       a.openingdate,
                                                                                                                       c.bmvs_verified,
                                                                                                                       p.description as purpose,
                                                                                                                       t.description as acc_type,
                                                                                                                       ct.description as acc_category,
                                                                                                                       (SELECT 'High'
                                                                                                                          from branch.t_account_highrisk@PRODDB.L1.LOCAL ra
                                                                                                                         where ra.accountid = a.accountid) as risk
                                                                                                                  from BRANCH.T_CUSTOMERACCOUNTS@PRODDB.L1.LOCAL ca
                                                                                                                 inner join branch.t_customer@PRODDB.L1.LOCAL c
                                                                                                                    on ca.customerid = c.customerid
                                                                                                                 INNER JOIN BRANCH.t_Account@PRODDB.L1.LOCAL a
                                                                                                                    on a.accountid = ca.accountid
                                                                                                                 inner join branch.t_branch@proddb.l1.local b
                                                                                                                    on a.branchid = b.branchid
                                                                                                                 inner join branch.T_ACCOUNTCATEGORY@PRODDB.L1.LOCAL ct
                                                                                                                    on ct.accountcategoryid = a.accountcategoryid
                                                                                                                 inner join BRANCH.T_ACCOUNT_PURPOSE@PRODDB.L1.LOCAL p
                                                                                                                    on p.account_purposeid = a.account_purposeid
                                                                                                                 inner join BRANCH.T_ACCOUNTTYPE@PRODDB.L1.LOCAL t
                                                                                                                    on t.accounttypeid = a.accounttypeid
                                                                                                                 inner join T_AU_SAMPLE_BRANCH bb
                                                                                                                    on bb.ac_no = a.oldaccountno*/
                select *
                  from v_P_Store_Sample_data bb
                 WHERE BB.ENG_ID = e_id
                   AND not exists
                 (select 'z'
                          from t_sample_accounts_data ds
                         where ds.oldaccountno = bb.ac_no)) loop
      
        insert into t_sample_accounts_data
          (eng_id,
           accountid,
           oldaccountno,
           name,
           customername,
           dob,
           phonecell,
           cnic,
           cnicexpirydate,
           openingdate,
           bmvs_verified,
           purpose,
           acc_type,
           acc_category,
           risk)
        values
          (i.eng_id,
           i.accountid,
           i.oldaccountno,
           i.name,
           i.customername,
           i.dob,
           i.phonecell,
           i.cnic,
           i.cnicexpirydate,
           i.openingdate,
           i.bmvs_verified,
           i.purpose,
           i.acc_type,
           i.acc_category,
           i.risk);
      
        commit;
      end loop;
    
    else
      if (IND = 'N') then
        delete from T_SAMPLE_loans_data d where d.engid = e_ID;
        commit;
        delete from t_sample_accounts_data a where a.eng_id = E_ID;
        commit;
      end if;
    end if;
  
  end P_Store_Sample_data;

  procedure p_get_Loan_Transactions(E_ID      number,
                                    L_DISB_ID number,
                                    P_NO      number,
                                    ENT_ID    number,
                                    R_ID      number,
                                    io_cursor OUT t_cursor) is
  begin
    open io_cursor for
    
      select m.description,
             t.manualvoucherno,
             t.transactiondate,
             t.transactiondate   as transactiondate_disp,
             t.dramount,
             t.cramount,
             t.ln_accountid,
             t.createdon,
             t.createdon         as createdon_disp,
             t.remarks,
             t.rejectiondate,
             t.rejectiondate     as rejectiondate_disp,
             t.reversaldate,
             t.reversaldate      as reversaldate_disp,
             t.workingdate,
             t.workingdate       as workingdate_disp,
             t.authorizationdate,
             t.authorizationdate as authorizationdate_disp,
             t.mco_receipt_no,
             t.mco_book_no
      
        from atas.t_ln_disbursement@proddb.l1.local d
       Inner join atas.t_ln_transaction@proddb.l1.local t
          on t.loan_disb_id = d.loan_disb_id
      
       inner join atas.t_ln_transactionmaster@proddb.l1.local m
          on t.transactionmasterid = m.ln_transactionmasterid
      
       where t.loan_disb_id = L_DISB_ID
       order by t.authorizationdate;
  
  end p_get_Loan_Transactions;

  PROCEDURE p_get_Loan_Documents(E_ID      NUMBER,
                                 L_DISB_ID NUMBER,
                                 P_NO      NUMBER,
                                 ENT_ID    NUMBER,
                                 R_ID      NUMBER,
                                 io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
    
    /*SELECT DISTINCT l2.imageid,
                                              b.branchcode,
                                              d.loan_disb_id,
                                              a.loan_app_id,
                                              c.cnic,
                                              c.customername,
                                              a.loan_case_no,
                                              d.loan_disb_id,
                                              ds.docname,
                                              
                                              \* ===== Date display columns (add the ones that exist) ===== *\
                                              
                                              \* If loan application has a date column like APP_DATE *\
                                              a.app_date,
                                              TO_CHAR(TRUNC(a.app_date),
                                                      'DD-Mon-YYYY',
                                                      'NLS_DATE_LANGUAGE=ENGLISH') AS app_date_disp,
                                              
                                              \* If disbursement has date like DISB_DATE / DISBURSAL_DATE *\
                                              d.disb_date,
                                              TO_CHAR(TRUNC(d.disb_date),
                                                      'DD-Mon-YYYY',
                                                      'NLS_DATE_LANGUAGE=ENGLISH') AS disb_date_disp
                              
                              \* If SDMS documents table has created/updated dates *\
                              \*           ds.created_on,
                              TO_CHAR(TRUNC(ds.created_on),
                                      'DD-Mon-YYYY',
                                      'NLS_DATE_LANGUAGE=ENGLISH') AS created_on_disp*\ \*,
                                                      
                                                                 ds.updated_on,
                                                                 TO_CHAR(TRUNC(ds.updated_on),
                                                                         'DD-Mon-YYYY',
                                                                         'NLS_DATE_LANGUAGE=ENGLISH') AS updated_on_disp*\
                              
                                FROM atas.t_ln_disbursement@proddb.l1.local d
                                JOIN atas.t_ln_loan_app@proddb.l1.local a
                                  ON d.loan_app_id = a.loan_app_id
                              
                                JOIN atas.t_branch@proddb.l1.local b
                                  ON a.branch_id = b.branchid
                              
                                JOIN atas.t_ln_customer_loan_app@proddb.l1.local cla
                                  ON d.loan_app_id = cla.loan_app_id
                              
                                JOIN atas.t_customer@proddb.l1.local c
                                  ON cla.customer_id = c.customerid
                              
                                JOIN sdms.t_loaninformation@proddb.l1.local li
                                  ON li.loancasenumber = a.loan_case_no
                                 AND li.branchid = b.branchcode
                              
                                JOIN sdms.t_generaicdocument@proddb.l1.local g
                                  ON g.loancaseid = li.loancaseid
                              
                                JOIN sdms.t_documents@proddb.l1.local ds
                                  ON g.docid = ds.docid
                                 AND ds.isactive = 'Y'
                              
                                JOIN sdms.t_loandocumentimages@proddb.l1.local l2
                                  ON li.loancaseid = l2.loancaseid
                                 AND li.loancasenumber = a.loan_case_no
                                 AND l2.docid = ds.docid
                                 AND li.branchid = b.branchcode
                              
                               WHERE cla.major_borrower IN ('A')
                                 AND cla.is_active = 'Y'
                                 AND d.is_active = 'Y'
                                 AND d.loan_disb_id */
    
      select *
        from v_p_get_Loan_Documents d
      
       where d.loan_disb_id = L_DISB_ID;
  
  end p_get_Loan_Documents;

  procedure p_get_Loan_Documents_image(image_ID  number,
                                       io_cursor OUT t_cursor)
  
   is
  begin
  
    open io_cursor for
    
      select 'asad'
      
        from dual s
      
      /*    
          select s.imagedata
          
            from sdms.v_t_loandocumentimages s
           where s.IMAGEID = image_ID;
        -- ;
      */
      ;
  end p_get_Loan_Documents_image;

  procedure p_get_biometric(CNIC_ID varchar2, io_cursor OUT t_cursor)
  
   is
  begin
    open io_cursor for
    /*      select d.name,
                                     d.fname,
                                     d.caddress,
                                     d.paddress,
                                     d.birthplace,
                                     d.doe,
                                     d.dob,
                                     d.card_type,
                                     c.contact_no
                              
                                from biomet.tbl_bio_nadra_data@proddb.l1.local d
                               inner join biomet.tbl_bio_customer@proddb.l1.local c
                                  on c.id = d.cust_id
                               where d.isactive = 'Y'
                                 and c.cnic*/
    
      select * from v_p_get_biometric c where c.cnic = CNIC_ID;
  
  end p_get_biometric;

  Procedure p_get_account_transcations_master_cbas(E_ID      number,
                                                   ST_DATE   date,
                                                   ED_DATE   date,
                                                   P_NO      number,
                                                   ENT_ID    number,
                                                   R_ID      number,
                                                   io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select m.description,
             m.ln_transactionmastercode,
             t.instrumentno,
             t.tr_code,
             t.manualvoucherno,
             t.transactiondate,
             td.glsubid,
             td.dramount,
             td.cramount
      
        from atas.t_ln_transaction@proddb.l1.local t
      
       inner join atas.t_ln_transactionmaster@proddb.l1.local m
          on t.transactionmasterid = m.ln_transactionmasterid
      
       inner join atas.t_ln_transactiondetail@proddb.l1.local td
          on td.ln_transactionid = t.ln_transactionid
      
       inner join atas.t_org_units@proddb.l1.local u
          on u.org_unitid = t.org_unitid
      
       inner join t_auditee_entities e
          on cast(e.code as varchar2(200)) = u.code
      
       where e.entity_id = E_ID
         and trunc(t.transactiondate) between ST_DATE and ED_DATE
       order by t.authorizationdate, t.manualvoucherno;
  
  end p_get_account_transcations_master_cbas;

  Procedure P_add_sample_data_update(E_ID      number,
                                     SID       number,
                                     P_NO      number,
                                     ENT_ID    number,
                                     R_ID      number,
                                     io_cursor OUT t_cursor) is
  
    S_E  number := 0;
    B_E  number := 0;
    P_F  number := 0;
    D_F  number := 0;
    Z_F  number := 0;
    C_F  number := 0;
    L_F  number := 0;
    S_F  number := 0;
    OE_F number := 0;
    SB_F number := 0;
    DB_F number := 0;
    LB_F number := 0;
    A_F  number := 0;
  
  begin
    IF (R_ID not in (3)) then
      if (SID = 1) then
        select count(*)
          into A_F
          from V_SAM_ACCOUNT n
         where n.eng_id = E_ID;
        update t_au_sample_data s
           set s.samp_tot = A_F
         where s.eg_id = E_id
           and s.S_id = 1;
        commit;
      
      elsif (SID = 2) then
        select count(*)
          into B_E
          from v_sampling_out_loans_status n
         where n.eng_id = E_ID
           and n.disb_statusid not in (11, 12);
        update t_au_sample_data s
           set s.samp_tot = B_E
         where s.eg_id = E_id
           and s.S_id = 2;
        commit;
      
      elsif (SID = 3) then
        select count(*)
          into C_F
          from v_sampling_closed_loans n1
         where n1.eng_id = E_ID;
      
        update t_au_sample_data s
           set s.samp_tot = C_F
         where s.eg_id = E_id
           and s.S_id = 3;
        commit;
      
      elsif (SID = 4) then
        select count(*)
          into Z_F
          from v_sampling_zero_recovery n
         where n.eng_id = E_ID;
        update t_au_sample_data s
           set s.samp_tot = Z_F
         where s.eg_id = E_id
           and s.S_id = 4;
        commit;
      
      elsif (SID = 5) then
        select count(*)
          into S_F
          from v_sampling_out_loans_status n5
         where n5.eng_id = E_ID
           and n5.disb_statusid = 17;
        update t_au_sample_data s
           set s.samp_tot = S_F
         where s.eg_id = E_id
           and s.S_id = 5;
        commit;
      
      elsif (SID = 6) then
      
        select count(*)
          into OE_F
          from v_sampling_out_loans_status n5
         where n5.eng_id = E_ID
           and n5.disb_statusid = 13;
        update t_au_sample_data s
           set s.samp_tot = OE_F
         where s.eg_id = E_id
           and s.S_id = 6;
        commit;
      
      elsif (SID = 7) then
        select count(*)
          into SB_F
          from v_sampling_out_loans_status n5
         where n5.eng_id = E_ID
           and n5.disb_statusid = 14;
        update t_au_sample_data s
           set s.samp_tot = DB_F
         where s.eg_id = E_id
           and s.S_id = 7;
        commit;
      elsif (SID = 8) then
      
        select count(*)
          into DB_F
          from v_sampling_out_loans_status n5
         where n5.eng_id = E_ID
           and n5.disb_statusid = 15;
        update t_au_sample_data s
           set s.samp_tot = SB_F
         where s.eg_id = E_id
           and s.S_id = 8;
        commit;
      elsif (SID = 9) then
      
        select count(*)
          into LB_F
          from v_sampling_out_loans_status n5
         where n5.eng_id = E_ID
           and n5.disb_statusid = 16;
        update t_au_sample_data s
           set s.samp_tot = LB_F
         where s.eg_id = E_id
           and s.S_id = 9;
        commit;
      elsif (SID = 10) then
        select count(*)
          into P_F
          from v_sampling_Prod_loans n3
         where n3.eng_id = E_ID;
        update t_au_sample_data s
           set s.samp_tot = P_F
         where s.eg_id = E_id
           and s.S_id = 10;
        commit;
      elsif (SID = 11) then
        select count(*)
          into D_F
          from v_sampling_Devp_loans n4
         where n4.eng_id = E_ID;
        update t_au_sample_data s
           set s.samp_tot = D_F
         where s.eg_id = E_id
           and s.S_id = 11;
        commit;
      elsif (SID = 12) then
        select count(*)
          into L_F
          from v_sampling_Disb_loans n2
         where n2.eng_id = E_ID;
        update t_au_sample_data s
           set s.samp_tot = L_F
         where s.eg_id = E_id
           and s.S_id in (12);
      
        select nvl(max(s.id), 0)
          into S_E
          from T_AU_SAMPLE_data s
         where s.eg_id = E_ID
           and s.s_id = 12;
      
        if (S_E < 20) then
          update t_au_sample_data d
             set d.sample_final = 20
           where d.s_id = 12
             and d.eg_id = E_ID;
          commit;
        end if;
        commit;
      elsif (SID = 13) then
        select count(*)
          into A_F
          from V_SAM_ACCOUNT n
         where n.eng_id = E_ID;
      
        update t_au_sample_data s
           set s.samp_tot = A_F
         where s.eg_id = E_id
           and s.S_id = 13;
        commit;
      end if;
    
      update t_au_sample_data s
         set s.sample_final = round(s.samp_tot * s.sample_percentage / 100)
       where s.eg_id = E_id
         and s.s_id = SID;
      commit;
    
      update t_au_sample_data ds
         set ds.sample_final = 1
       where ds.sample_final = 0
         and ds.eg_id = E_ID
         and ds.s_id = SID;
      commit;
    
      pkg_sm.p_update_Sample(EID       => E_ID,
                             SD        => SID,
                             P_NO      => P_NO,
                             ENT_ID    => ENT_ID,
                             R_ID      => R_ID,
                             io_cursor => io_cursor);
    
    else
      Open io_cursor for
        Select 'You havent got rights to Regenerate Sample, Please Contact FAD' as remarks
          from dual;
    
    end if;
  end P_add_sample_data_update;

  procedure p_update_Sample(EID       number,
                            SD        number,
                            P_NO      number,
                            ENT_ID    number,
                            R_ID      number,
                            io_cursor OUT t_cursor) is
    B_F number := 0;
    S_F number := 0;
    S_M varchar2(500);
  begin
    select s.sample_type into S_M from t_au_sample s where s.id = SD;
  
    select nvl(max(b.id), 0)
      into S_F
      from T_AU_SAMPLE_BRANCH b
     where b.eng_id = EID;
    if (S_F != 0 and SD is not null) then
      if (SD = 1) then
      
        delete from t_au_sample_branch b
         where b.eng_id = EID
           and b.s_id = SD;
        commit;
      
        select d.sample_final
          into B_F
          from t_au_sample_data d
         where d.s_id = 1
           and d.eg_id = EID;
      
        insert into T_AU_SAMPLE_BRANCH
          (ENG_ID, S_ID, AC_NO, IND)
        
          SELECT B.eng_id, 1, B.oldaccountno, 'A'
          
            FROM V_SAM_BIOMET B
           WHERE B.eng_id = EID
             and not exists (select 'z'
                    from T_AU_SAMPLE_BRANCH sm
                   where sm.ac_no = b.oldaccountno)
             and rownum <= B_F;
        commit;
        delete from t_sample_accounts_data d
         where d.eng_id = EID
           and D.S_D = SD;
        commit;
      else
        if (SD = 2) then
        
          delete from t_au_sample_branch b
           where b.eng_id = EID
             and b.s_id = SD;
          commit;
          select d.sample_final
            into B_F
            from t_au_sample_data d
           where d.s_id = 2
             and d.eg_id = EID;
        
          insert into T_AU_SAMPLE_BRANCH
            (ENG_ID, S_ID, LOAN_ID, DISB_ID, LOAN_CASE, IND)
          
            SELECT B.eng_id,
                   2,
                   b.loan_app_id,
                   b.loan_disb_id,
                   b.loan_case_no,
                   'L'
            
              FROM v_sampling_out_loans_status B
             WHERE B.eng_id = EID
               and b.disb_statusid not in (11, 12)
               and not exists (select 'z'
                      from T_AU_SAMPLE_BRANCH sm
                     where sm.disb_id = b.loan_disb_id)
               and rownum <= B_F;
          commit;
        else
          if (SD = 3) then
            delete from t_au_sample_branch b
             where b.eng_id = EID
               and b.s_id = SD;
            commit;
            select d.sample_final
              into B_F
              from t_au_sample_data d
             where d.s_id = 3
               and d.eg_id = EID;
          
            insert into T_AU_SAMPLE_BRANCH
              (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, LOAN_CASE)
            
              SELECT B.eng_id,
                     3,
                     b.loan_app_id,
                     b.loan_disb_id,
                     'L',
                     b.loan_case_no
              
                FROM v_sampling_closed_loans B
               WHERE B.eng_id = EID
                 and not exists (select 'z'
                        from T_AU_SAMPLE_BRANCH sm
                       where sm.disb_id = b.loan_disb_id)
                 and rownum <= B_F;
            commit;
          else
            if (SD = 4) then
              delete from t_au_sample_branch b
               where b.eng_id = EID
                 and b.s_id = SD;
              commit;
              select d.sample_final
                into B_F
                from t_au_sample_data d
               where d.s_id = 4
                 and d.eg_id = EID;
            
              insert into T_AU_SAMPLE_BRANCH
                (ENG_ID, S_ID, LOAN_ID, DISB_ID, IND, LOAN_CASE)
              
                SELECT B.eng_id,
                       4,
                       b.loan_app_id,
                       b.loan_disb_id,
                       'L',
                       b.loan_case_no
                
                  FROM v_sampling_zero_recovery B
                 INNER JOIN T_AU_PLAN_ENG E
                    ON E.ENG_ID = B.ENG_ID
                 WHERE B.eng_id = EID
                   AND B.disb_date < E.OPERATION_STARTDATE
                   and not exists (select 'z'
                          from T_AU_SAMPLE_BRANCH sm
                         where sm.disb_id = b.loan_disb_id)
                   and rownum <= B_F;
              commit;
            else
              if (SD = 5) then
                delete from t_au_sample_branch b
                 where b.eng_id = EID
                   and b.s_id = SD;
                commit;
                select d.sample_final
                  into B_F
                  from t_au_sample_data d
                 where d.s_id = 5
                   and d.eg_id = EID;
              
                insert into T_AU_SAMPLE_BRANCH
                  (ENG_ID,
                   S_ID,
                   LOAN_ID,
                   DISB_ID,
                   IND,
                   L_STATUS,
                   LOAN_CASE)
                
                  SELECT B.eng_id,
                         5,
                         b.loan_app_id,
                         b.loan_disb_id,
                         'L',
                         17,
                         b.loan_case_no
                  
                    FROM v_sampling_out_loans_status B
                   WHERE B.eng_id = EID
                     and b.disb_statusid = 17
                     and not exists
                   (select 'z'
                            from T_AU_SAMPLE_BRANCH sm
                           where sm.disb_id = b.loan_disb_id)
                     and rownum <= B_F;
                commit;
              else
                if (SD = 6) then
                  delete from t_au_sample_branch b
                   where b.eng_id = EID
                     and b.s_id = SD;
                  commit;
                  select d.sample_final
                    into B_F
                    from t_au_sample_data d
                   where d.s_id = 6
                     and d.eg_id = EID;
                
                  insert into T_AU_SAMPLE_BRANCH
                    (ENG_ID,
                     S_ID,
                     LOAN_ID,
                     DISB_ID,
                     IND,
                     L_STATUS,
                     LOAN_CASE)
                  
                    SELECT B.eng_id,
                           6,
                           b.loan_app_id,
                           b.loan_disb_id,
                           'L',
                           13,
                           b.loan_case_no
                    
                      FROM v_sampling_out_loans_status B
                     WHERE B.eng_id = EID
                       and b.disb_statusid = 13
                       and not exists
                     (select 'z'
                              from T_AU_SAMPLE_BRANCH sm
                             where sm.disb_id = b.loan_disb_id)
                       and rownum <= B_F;
                  commit;
                else
                  if (SD = 7) then
                    delete from t_au_sample_branch b
                     where b.eng_id = EID
                       and b.s_id = SD;
                    commit;
                    select d.sample_final
                      into B_F
                      from t_au_sample_data d
                     where d.s_id = 7
                       and d.eg_id = EID;
                  
                    insert into T_AU_SAMPLE_BRANCH
                      (ENG_ID,
                       S_ID,
                       LOAN_ID,
                       DISB_ID,
                       IND,
                       L_STATUS,
                       LOAN_CASE)
                    
                      SELECT B.eng_id,
                             7,
                             b.loan_app_id,
                             b.loan_disb_id,
                             'L',
                             14,
                             b.loan_case_no
                      
                        FROM v_sampling_out_loans_status B
                       WHERE B.eng_id = EID
                         and b.disb_statusid = 14
                         and not exists
                       (select 'z'
                                from T_AU_SAMPLE_BRANCH sm
                               where sm.disb_id = b.loan_disb_id)
                         and rownum <= B_F;
                    commit;
                  else
                    if (SD = 8) then
                      delete from t_au_sample_branch b
                       where b.eng_id = EID
                         and b.s_id = SD;
                      commit;
                      select d.sample_final
                        into B_F
                        from t_au_sample_data d
                       where d.s_id = 8
                         and d.eg_id = EID;
                    
                      insert into T_AU_SAMPLE_BRANCH
                        (ENG_ID,
                         S_ID,
                         LOAN_ID,
                         DISB_ID,
                         IND,
                         L_STATUS,
                         LOAN_CASE)
                      
                        SELECT B.eng_id,
                               8,
                               b.loan_app_id,
                               b.loan_disb_id,
                               'L',
                               15,
                               b.loan_case_no
                        
                          FROM v_sampling_out_loans_status B
                         WHERE B.eng_id = EID
                           and b.disb_statusid = 15
                           and not exists
                         (select 'z'
                                  from T_AU_SAMPLE_BRANCH sm
                                 where sm.disb_id = b.loan_disb_id)
                           and rownum <= B_F;
                      commit;
                    else
                      if (SD = 9) then
                        delete from t_au_sample_branch b
                         where b.eng_id = EID
                           and b.s_id = SD;
                        commit;
                        select d.sample_final
                          into B_F
                          from t_au_sample_data d
                         where d.s_id = 9
                           and d.eg_id = EID;
                      
                        insert into T_AU_SAMPLE_BRANCH
                          (ENG_ID,
                           S_ID,
                           LOAN_ID,
                           DISB_ID,
                           IND,
                           L_STATUS,
                           LOAN_CASE)
                        
                          SELECT B.eng_id,
                                 9,
                                 b.loan_app_id,
                                 b.loan_disb_id,
                                 'L',
                                 16,
                                 b.loan_case_no
                          
                            FROM v_sampling_out_loans_status B
                           WHERE B.eng_id = EID
                             and b.disb_statusid = 16
                             and not exists
                           (select 'z'
                                    from T_AU_SAMPLE_BRANCH sm
                                   where sm.disb_id = b.loan_disb_id)
                             and rownum <= B_F;
                        commit;
                      else
                        if (SD = 10) then
                          delete from t_au_sample_branch b
                           where b.eng_id = EID
                             and b.s_id = SD;
                          commit;
                          select d.sample_final
                            into B_F
                            from t_au_sample_data d
                           where d.s_id = 10
                             and d.eg_id = EID;
                        
                          insert into T_AU_SAMPLE_BRANCH
                            (ENG_ID,
                             S_ID,
                             LOAN_ID,
                             DISB_ID,
                             IND,
                             LOAN_CASE)
                          
                            SELECT B.eng_id,
                                   10,
                                   b.loan_app_id,
                                   b.loan_disb_id,
                                   'L',
                                   b.loan_case_no
                            
                              FROM v_sampling_Prod_loans B
                             WHERE B.eng_id = EID
                               and not exists
                             (select 'z'
                                      from T_AU_SAMPLE_BRANCH sm
                                     where sm.disb_id = b.loan_disb_id)
                               and rownum <= B_F;
                          commit;
                        else
                          if (SD = 11) then
                            delete from t_au_sample_branch b
                             where b.eng_id = EID
                               and b.s_id = SD;
                            commit;
                            select d.sample_final
                              into B_F
                              from t_au_sample_data d
                             where d.s_id = 11
                               and d.eg_id = EID;
                          
                            insert into T_AU_SAMPLE_BRANCH
                              (ENG_ID,
                               S_ID,
                               LOAN_ID,
                               DISB_ID,
                               IND,
                               LOAN_CASE)
                            
                              SELECT B.eng_id,
                                     11,
                                     b.loan_app_id,
                                     b.loan_disb_id,
                                     'L',
                                     b.loan_case_no
                              
                                FROM v_sampling_devp_loans B
                               WHERE B.eng_id = EID
                                 and not exists
                               (select 'z'
                                        from T_AU_SAMPLE_BRANCH sm
                                       where sm.disb_id = b.loan_disb_id)
                                 and rownum <= B_F;
                            commit;
                          else
                            if (SD = 12) then
                              delete from t_au_sample_branch b
                               where b.eng_id = EID
                                 and b.s_id = SD;
                              commit;
                              select d.sample_final
                                into B_F
                                from t_au_sample_data d
                               where d.s_id = 12
                                 and d.eg_id = EID;
                            
                              insert into T_AU_SAMPLE_BRANCH
                                (ENG_ID,
                                 S_ID,
                                 LOAN_ID,
                                 DISB_ID,
                                 IND,
                                 L_STATUS,
                                 LOAN_CASE)
                              
                                SELECT B.eng_id,
                                       12,
                                       b.loan_app_id,
                                       b.loan_disb_id,
                                       'L',
                                       11,
                                       b.loan_case_no
                                
                                  FROM v_sampling_Disb_loans B
                                 WHERE B.eng_id = EID
                                   and not exists
                                 (select 'z'
                                          from T_AU_SAMPLE_BRANCH sm
                                         where sm.disb_id = b.loan_disb_id)
                                   and rownum <= B_F;
                              commit;
                            else
                              if (SD = 13) then
                                delete from t_au_sample_branch b
                                 where b.eng_id = EID
                                   and b.s_id = SD;
                                commit;
                                select d.sample_final
                                  into B_F
                                  from t_au_sample_data d
                                 where d.s_id = 13
                                   and d.eg_id = EID;
                              
                                insert into T_AU_SAMPLE_BRANCH
                                  (ENG_ID, S_ID, AC_NO, IND)
                                  SELECT B.eng_id, 13, b.oldaccountno, 'A'
                                    FROM V_SAM_ACCOUNT B
                                   WHERE B.eng_id = EID
                                     and not exists
                                   (select 'z'
                                            from T_AU_SAMPLE_BRANCH sm
                                           where sm.ac_no = b.oldaccountno)
                                     and rownum <= B_F;
                                commit;
                              
                              end if;
                            end if;
                          end if;
                        end if;
                      end if;
                    end if;
                  end if;
                end if;
              end if;
            end if;
          end if;
        end if;
      end if;
    
      if (SD not in (1, 12)) then
        delete from t_sample_loans_data d
         where d.engid = EID
           and D.S_D = SD;
        commit;
      
        insert into t_sample_loans_data
          (loan_disb_id,
           engid,
           s_d,
           cnic,
           customername,
           type,
           scheme,
           l_purpose,
           l_crop,
           glsubcode,
           cultivated_area,
           market_values,
           quantity,
           total_estimated_cost,
           prod_dev,
           loan_app_id,
           required_item_id,
           purpose_id,
           sub_proposal_id,
           entered_by,
           entered_date,
           amount_recommedned,
           scheme_id,
           mark_cal_mode,
           installment_frequency,
           total_installment,
           consent_insurance_opt_crop,
           mco_ppno,
           manager_ppno,
           dev_amount,
           prod_amount,
           tot_sanctioned_amount,
           tot_max_credit_limit,
           no_of_products,
           no_of_securities,
           tot_land,
           circle_code,
           loan_case_no,
           app_date,
           app_status_change_date,
           app_status_change_reason,
           sanction_date,
           disb_date,
           l_status,
           valid_until,
           outstanding,
           disbursed_amount)
          select d.loan_disb_id,
                 B.ENG_ID,
                 B.S_ID,
                 c.cnic,
                 c.customername,
                 (case
                   when dp.prod_dev = 'P' then
                    'Production'
                   when dp.prod_dev = 'D' then
                    'Development'
                   else
                    ''
                 end) as Type,
                 sk.description as scheme,
                 gl.glsubname as l_purpose,
                 dp.crop_id as l_crop,
                 gl.glsubcode,
                 dp.cultivated_area,
                 dp.market_values,
                 dp.quantity,
                 dp.total_estimated_cost,
                 dp.prod_dev,
                 dp.loan_app_id,
                 dp.required_item_id,
                 dp.purpose_id,
                 dp.sub_proposal_id,
                 dp.entered_by,
                 dp.entered_date,
                 dp.amount_recommedned,
                 dp.scheme_id,
                 dp.mark_cal_mode,
                 dp.installment_frequency,
                 dp.total_installment,
                 dp.consent_insurance_opt_crop,
                 lp.mco_ppno,
                 lp.manager_ppno,
                 lp.dev_amount,
                 lp.prod_amount,
                 lp.tot_sanctioned_amount,
                 lp.tot_max_credit_limit,
                 lp.no_of_products,
                 lp.no_of_securities,
                 lp.tot_land,
                 lp.circle_code,
                 lp.loan_case_no,
                 lp.app_date,
                 lp.app_status_change_date,
                 lp.app_status_change_reason,
                 lp.sanction_date,
                 d.disb_date,
                 d.disb_statusid as l_status,
                 d.valid_until,
                 ((d.disbursed_amount - d.recovered_principal) +
                 ceil(d.todate_markup - d.recov_markup)) as Outstanding,
                 d.disbursed_amount
          
            from atas.t_ln_loan_app@proddb.l1.local lp
          
           inner join ATAS.t_Ln_Production_Dev_Loan@PRODDB.L1.LOCAL dp
              on dp.loan_app_id = lp.loan_app_id
             and dp.branch_id = lp.branch_id
          
           inner join atas.t_ln_customer_loan_app@proddb.l1.local cl
              on lp.loan_app_id = cl.loan_app_id
             and cl.branch_id = lp.branch_id
          
           inner join atas.t_customer@proddb.l1.local c
              ON cl.customer_id = c.customerid
             and c.branchid = cl.branch_id
          
           inner join atas.t_glsub@proddb.l1.local gl
              on gl.glsubid = dp.gl_sub_id
             and gl.branchid = lp.branch_id
          
           inner join atas.t_ln_disbursement@PRODDB.L1.LOCAL d
              on d.loan_app_id = lp.loan_app_id
             and lp.branch_id = d.org_unitid
             and d.glsubid = dp.gl_sub_id
          
           inner join atas.cm_scheme@proddb.l1.local sk
              on d.scheme_id = sk.scheme_id
          
           inner join T_AU_SAMPLE_BRANCH b
              on D.loan_disb_id = b.disb_id
             and b.eng_id = EID
             and b.s_id = SD
           where not exists (select 'z'
                    from T_SAMPLE_loans_data sd
                   where sd.loan_disb_id = b.disb_id);
        commit;
      
      else
        if (SD in (1, 12)) then
          delete from t_sample_accounts_data d
           where d.eng_id = EID
             and d.s_d = SD;
          commit;
        
          insert into t_sample_accounts_data
            (eng_id,
             accountid,
             oldaccountno,
             name,
             customername,
             dob,
             phonecell,
             cnic,
             cnicexpirydate,
             openingdate,
             bmvs_verified,
             purpose,
             acc_type,
             acc_category,
             risk)
          /*SELECT bb.eng_id,
                a.accountid,
                a.oldaccountno,
                a.name,
                c.customername,
                c.dob,
                c.phonecell,
                c.cnic,
                c.cnicexpirydate,
                a.openingdate,
                c.bmvs_verified,
                p.description as purpose,
                t.description as acc_type,
                ct.description as acc_category,
                (SELECT 'High'
                   from branch.t_account_highrisk@PRODDB.L1.LOCAL ra
                  where ra.accountid = a.accountid) as risk
           from BRANCH.T_CUSTOMERACCOUNTS@PRODDB.L1.LOCAL ca
          inner join branch.t_customer@PRODDB.L1.LOCAL c
             on ca.customerid = c.customerid
          INNER JOIN BRANCH.t_Account@PRODDB.L1.LOCAL a
             on a.accountid = ca.accountid
          inner join branch.t_branch@proddb.l1.local b
             on a.branchid = b.branchid
          inner join branch.T_ACCOUNTCATEGORY@PRODDB.L1.LOCAL ct
             on ct.accountcategoryid = a.accountcategoryid
          inner join BRANCH.T_ACCOUNT_PURPOSE@PRODDB.L1.LOCAL p
             on p.account_purposeid = a.account_purposeid
          inner join BRANCH.T_ACCOUNTTYPE@PRODDB.L1.LOCAL t
             on t.accounttypeid = a.accounttypeid
          inner join T_AU_SAMPLE_BRANCH bb
             on bb.ac_no = a.oldaccountno
          WHERE BB.ENG_ID = eid
            and bb.s_id = sd*/
            select s.eng_id,
                   s.accountid,
                   s.oldaccountno,
                   s.name,
                   s.customername,
                   s.dob,
                   s.phonecell,
                   s.cnic,
                   s.cnicexpirydate,
                   s.openingdate,
                   s.bmvs_verified,
                   s.purpose,
                   s.acc_type,
                   s.acc_category,
                   s.risk
              from v_p_update_Sample s
             where s.eng_id = eid
               and s.s_id = sd
                  
               AND not exists
             (select 'z'
                      from t_sample_accounts_data ds
                     where ds.oldaccountno = s.ac_no);
        
          commit;
        
        end if;
      end if;
    end if;
    open io_cursor for
    
      select S_M || ' Sample has been updated' as remarks from dual;
  
  end p_update_Sample;

  PROCEDURE P_ADD_EXCEPTION_DATA(E_ID      IN NUMBER,
                                 P_NO      IN NUMBER,
                                 R_ID      IN NUMBER, -- report id / ER_ID
                                 ENT_ID    IN NUMBER,
                                 io_cursor OUT t_cursor) IS
  BEGIN
  
    ------------------------------------------------------------------
    -- 0) Always clear old generated data for this engagement/report
    --    (choose one of the two delete strategies below)
    ------------------------------------------------------------------
  
    -- Strategy A: regenerate ALL reports for this ENG_ID (recommended
    -- because your code runs all P_POP_EXCEPTION_* procedures)
  
    DELETE FROM t_exception_accounts_cust e WHERE eng_id = E_ID;
    DELETE FROM t_exception_accounts_txn WHERE eng_id = E_ID;
    DELETE FROM t_exception_eng WHERE eng_id = E_ID;
    Delete from t_exception_eng_branches b where b.engid = E_ID;
    DELETE FROM t_exception_accounts WHERE eng_id = E_ID;
    DELETE FROM t_exception_accounts_data WHERE eng_id = E_ID;
    Delete from T_EXCEPTION_ENG_DATA where eng_id = E_ID;
    commit;
    -- 1. Prepare temp data once
    P_PREP_EXCEPTION_BASE(E_ID);
  
    -- 2. Populate only the requested report
    -- Populate ALL exception reports instead of one specific
    P_POP_EXCEPTION_CTR(E_ID);
    P_POP_EXCEPTION_CNIC_EXPIRY(E_ID);
    P_POP_EXCEPTION_NEG_BAL(E_ID);
    P_POP_EXCEPTION_EMP_ACCOUNTS(E_ID);
    P_POP_EXCEPTION_HIGH_TRUN_OVER(E_ID);
    P_POP_EXCEPTION_DORMANT(E_ID);
    P_POP_EXCEPTION_Cell_Change(E_ID);
    P_POP_EXCEPTION_PROFIT_AMOUNT(E_ID);
    P_POP_EXCEPTION_ZAKAT(E_ID);
  
    -- 3. Optionally return rows just inserted for that ER_ID
    OPEN io_cursor FOR
      SELECT *
        FROM t_exception_accounts_data
       WHERE eng_id = E_ID
         AND er_id = R_ID;
  
  END;

  PROCEDURE P_PREP_EXCEPTION_BASE(E_ID IN NUMBER) IS
    ENT_Type NUMBER := 0;
    B_Code   VARCHAR2(50);
    B_ID     NUMBER := 0;
  BEGIN
    ----------------------------------------------------------------
    -- 1. Get entity type + branch code for this engagement
    ----------------------------------------------------------------
    SELECT e.entity_type, e.branch_code
      INTO ENT_Type, B_CODE
      FROM t_au_plan_eng e
     WHERE e.eng_id = E_ID;
  
    ----------------------------------------------------------------
    -- Only run for branch-type entities
    ----------------------------------------------------------------
    IF ENT_Type IN (6, 28) THEN
    
      ----------------------------------------------------------------
      -- 2. Resolve branchid and (re)build TEMP_BRANCHES for this ENG_ID
      ----------------------------------------------------------------
      SELECT b.branchid
        INTO B_ID
        FROM branch.t_branch@proddb.l1.local b
       WHERE b.branchcode = B_CODE;
    
      -- Clear TEMP_BRANCHES for this session (optional but clean)
      EXECUTE IMMEDIATE 'TRUNCATE TABLE temp_branches';
    
      INSERT INTO t_exception_eng_branches
        (branchid, branchcode, name, engid, audit_startdate, audit_enddate)
        SELECT b.branchid,
               b.branchcode,
               b.name,
               E_ID,
               e.operation_startdate,
               e.operation_enddate
          FROM branch.t_branch@proddb.l1.local b
          JOIN t_au_plan_eng e
            ON e.branch_code = b.branchcode
           AND e.eng_id = E_ID
         WHERE b.branchid = B_ID;
    
      COMMIT; -- GTT rows remain because of ON COMMIT PRESERVE ROWS
    
      ----------------------------------------------------------------
      -- 3. Insert ACCOUNT-LEVEL data into T_EXCEPTION_ACCOUNTS
      --    One row per ACCOUNT_ID, avoid duplicates with NOT EXISTS
      ----------------------------------------------------------------
      INSERT INTO T_EXCEPTION_ACCOUNTS
        (ACCOUNT_ID,
         ACCOUNT_NO,
         TITLE,
         OPENINGDATE_DISP,
         ACCOUNT_PURPOSE,
         ACCOUNT_TYPE,
         ACCOUNT_CATEGORY,
         ACCOUNT_STATUS,
         RISK,
         ACCOUNT_BALANCE,
         LAST_TRAN_DATE_DISP,
         BRANCH_ID,
         ENG_ID)
        select * from v_T_EXCEPTION_ACCOUNTS e where e.engid = E_ID;
    
      COMMIT;
    
      ----------------------------------------------------------------
      -- 4. Insert CUSTOMER-LEVEL data into T_EXCEPTION_ACCOUNTS_CUST
      --    PK is CUSTOMER_ID, so we:
      --      * dedupe per CUSTOMER_ID (rn = 1)
      --      * ensure we don't reinsert existing CUSTOMER_IDs
      ----------------------------------------------------------------
      INSERT INTO T_EXCEPTION_ACCOUNTS_CUST
        (ACCOUNT_ID,
         CUSTOMER_ID,
         CUSTOMER_ACCOUNT_ID,
         CUSTOMERNAME,
         DOB_DISP,
         PHONE_CELL,
         CNIC,
         CNICEXPIRYDATE_DISP,
         BMVS_VERIFIED,
         BANK_EMPLOYEE,
         SOURCE_OF_FUND,
         DATE_OF_CLOSURE_DISP,
         ZAKAT_EXEMPTED,
         CUSTOMER_RISK,
         ENG_ID)
        Select b.accountid,
               b.customerid,
               b.customeraccountid,
               b.customername,
               b.dob,
               b.phonecell,
               b.cnic,
               b.cnicexpirydate,
               b.bmvs_verified,
               b.isbankemployee,
               b.SOURCE_OF_FUND,
               b.DATE_OF_CLOSURE,
               b.ZAKAT_EXEMPTED,
               b.CUSTOMER_RISK,
               E_ID
          from v_T_EXCEPTION_ACCOUNTS_CUST b
         where b.branchid = B_ID;
    
      COMMIT;
    
      ----------------------------------------------------------------
      -- 5. Insert TRANSACTION-LEVEL data into T_EXCEPTION_ACCOUNTS_TXN
      --    Guard with NOT EXISTS to avoid duplicate txn rows
      ----------------------------------------------------------------
      INSERT INTO T_EXCEPTION_ACCOUNTS_TXN
        (TR_ID,
         ACCOUNT_ID,
         TRAN_DATE_DISP,
         TRAN_AUTH_DATE_DISP,
         TRANSACTIONMASTERCODE,
         TRANSACTION_DISCRIPTION,
         NATURE_OF_TRANSACTION,
         DR_AMOUNT,
         CR_AMOUNT,
         TR_AMOUNT,
         BRANCH_ID,
         ENG_ID)
        select a.transactionid,
               a.account_id,
               a.tran_date_disp,
               a.tran_auth_date_disp,
               a.transactionmastercode,
               a.transaction_description,
               a.nature_of_transaction,
               a.dr_amount,
               a.cr_amount,
               a.tr_amount,
               a.branchid,
               E_ID
          from v_T_EXCEPTION_ACCOUNTS_TXN a;
    
      COMMIT;
    END IF;
  END P_PREP_EXCEPTION_BASE;

  PROCEDURE P_POP_EXCEPTION_CTR(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 1;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       DATE_DISP,
       NET_AMOUNT)
      SELECT ac.eng_id,
             tx.ACCOUNT_ID,
             1,
             ac.account_no,
             ac.title,
             c.CNIC,
             c.phone_cell,
             ac.account_type,
             tx.TRAN_DATE_DISP,
             SUM(tx.CR_AMOUNT) AS EXCEPTION_AMOUNT
        FROM T_EXCEPTION_ACCOUNTS_TXN tx
        JOIN T_EXCEPTION_ACCOUNTS ac
          ON ac.ACCOUNT_ID = tx.ACCOUNT_ID
         and ac.eng_id = tx.eng_id
       inner join t_exception_accounts_cust c
          on c.account_id = ac.account_id
        join t_exception_eng_branches b
          on b.engid = ac.eng_id
       where ac.ENG_ID = E_ID
         and trunc(tx.tran_auth_date_disp) between trunc(b.audit_startdate) and
             trunc(b.audit_enddate)
         and tx.transactionmastercode not in (321, 28)
         and tx.transaction_discription like '%CASH%'
         and tx.cr_amount < 2000000
       GROUP BY ac.eng_id,
                tx.ACCOUNT_ID,
                1,
                ac.account_no,
                ac.title,
                c.CNIC,
                c.phone_cell,
                ac.account_type,
                tx.TRAN_DATE_DISP
      HAVING SUM(tx.CR_AMOUNT) > 2000000;
  
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_HIGH_TRUN_OVER(E_ID IN NUMBER) IS
  BEGIN
  
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 6;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       CR_AMOUNT,
       DR_AMOUNT,
       NET_AMOUNT)
      SELECT ac.eng_id,
             tx.account_id,
             6 AS er_id,
             ac.account_no,
             ac.title,
             c.cnic,
             c.phone_cell,
             ac.account_type,
             SUM(NVL(tx.cr_amount, 0)) AS credit_amount,
             SUM(NVL(tx.dr_amount, 0)) AS debit_amount,
             SUM(NVL(tx.cr_amount, 0) + NVL(tx.dr_amount, 0)) AS total_turnover
        FROM t_exception_accounts_txn tx
        JOIN t_exception_accounts ac
          ON ac.account_id = tx.account_id
         AND ac.eng_id = tx.eng_id
        join t_exception_accounts_cust c
          on c.account_id = ac.account_id
        JOIN t_exception_eng_branches b
          ON b.engid = ac.eng_id
       WHERE ac.eng_id = E_ID
         AND tx.transactionmastercode NOT IN (321, 28)
         AND tx.tran_auth_date_disp >= TRUNC(b.audit_startdate)
         AND tx.tran_auth_date_disp < TRUNC(b.audit_enddate) + 1
       GROUP BY ac.eng_id,
                tx.account_id,
                6,
                ac.account_no,
                ac.title,
                c.cnic,
                c.phone_cell,
                ac.account_type
      HAVING SUM(NVL(tx.cr_amount, 0) + NVL(tx.dr_amount, 0)) > 5000000;
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_CNIC_EXPIRY(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 2;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CELL,
       ACCOUNT_TYPE,
       CNIC,
       DATE_DISP)
      SELECT cust.eng_id,
             cust.ACCOUNT_ID,
             2 AS ER_ID,
             ac.account_no,
             ac.title,
             cust.phone_cell,
             ac.account_type,
             cust.cnic,
             cust.CNICEXPIRYDATE_DISP
        FROM T_EXCEPTION_ACCOUNTS_CUST cust
        JOIN T_EXCEPTION_ACCOUNTS ac
          ON ac.ACCOUNT_ID = cust.ACCOUNT_ID
         AND ac.ENG_ID = cust.eng_id
        JOIN T_EXCEPTION_ENG_BRANCHES b
          ON b.branchid = ac.BRANCH_ID
       WHERE trunc(cust.CNICEXPIRYDATE_DISP) between
             trunc(b.audit_startdate) and trunc(b.audit_enddate)
         and ac.account_status != 'CLOSED'
         and b.engid = E_ID;
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_NEG_BAL(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 3;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       DATE_DISP,
       NET_AMOUNT)
    
      SELECT E_ID,
             ac.ACCOUNT_ID,
             3,
             ac.account_no,
             ac.title,
             cust.cnic,
             cust.phone_cell,
             ac.account_type,
             ac.LAST_TRAN_DATE_DISP,
             ac.ACCOUNT_BALANCE
        FROM T_EXCEPTION_ACCOUNTS ac
       inner join T_EXCEPTION_ACCOUNTS_CUST cust
          on cust.account_id = ac.account_id
       WHERE ac.ENG_ID = E_ID
         AND ac.ACCOUNT_BALANCE < 0;
  
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_EMP_ACCOUNTS(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 4;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       CR_AMOUNT,
       DR_AMOUNT,
       NET_AMOUNT)
      SELECT ac.eng_id,
             cust.account_id,
             4 AS er_id,
             ac.account_no,
             ac.title,
             cust.cnic,
             cust.phone_cell,
             ac.account_type,
             sum(t.cr_amount) as cr_amount,
             sum(t.dr_amount) as dr_amount,
             SUM(NVL(t.cr_amount, 0) + NVL(t.dr_amount, 0)) AS total_turnover
        FROM t_exception_accounts ac
        JOIN t_exception_accounts_cust cust
          ON cust.account_id = ac.account_id
        JOIN t_exception_accounts_txn t
          ON t.account_id = ac.account_id
        JOIN t_exception_eng_branches b
          ON b.engid = ac.eng_id
       WHERE ac.eng_id = E_ID
         AND cust.bank_employee = 'Y'
         AND t.tran_auth_date_disp >= TRUNC(b.audit_startdate)
         AND t.tran_auth_date_disp < TRUNC(b.audit_enddate) + 1
       GROUP BY ac.eng_id,
                cust.account_id,
                4,
                ac.account_no,
                ac.title,
                cust.cnic,
                cust.phone_cell,
                ac.account_type;
  
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_DORMANT(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 7;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       DATE_DISP)
      SELECT ac.eng_id,
             l.accountid,
             7,
             ac.account_no,
             ac.title,
             c.cnic,
             c.phone_cell,
             ac.account_type,
             l.histupdatedon
        FROM T_EXCEPTION_ACCOUNTS ac
       inner join t_exception_accounts_cust c
          on c.account_id = ac.account_id
        JOIN v_exp_account_status_log l
          ON ac.ACCOUNT_ID = l.accountid
        join t_exception_eng_branches b
          on b.engid = ac.eng_id
       where ac.ENG_ID = E_ID
         and trunc(l.histupdatedon) between trunc(b.audit_startdate) and
             trunc(b.audit_enddate)
         and l.description like '%(DORMANT) TO (OPERATIVE)%';
  
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_PROFIT_AMOUNT(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 8;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       CR_AMOUNT,
       DR_AMOUNT,
       NET_AMOUNT)
      SELECT ac.eng_id,
             l.accountid,
             8,
             ac.account_no,
             ac.title,
             c.cnic,
             c.phone_cell,
             ac.account_type,
             l.profit_amount,
             l.tax_amount,
             l.net_amount
        FROM T_EXCEPTION_ACCOUNTS ac
        join t_exception_accounts_cust c
          on c.account_id = ac.account_id
        JOIN v_exception_report_tax_amount l
          ON ac.ACCOUNT_ID = l.accountid
        join t_exception_eng_branches b
          on b.engid = ac.eng_id
       where ac.ENG_ID = E_ID
         and trunc(l.transactiondate) between trunc(b.audit_startdate) and
             trunc(b.audit_enddate);
  
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_Cell_Change(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG_DATA e
     where e.eng_id = E_ID
       and e.er_id = 9;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       ACCOUNT_TYPE,
       TEXT_1,
       Text_2,DATE_DISP)
      SELECT ac.eng_id,
             ac.account_id,
             9,
             ac.account_no,
             ac.title,
             c.cnic,
             ac.account_type,
             l.old_number,
             l.new_number,
             l.datecreated
      
        FROM T_EXCEPTION_ACCOUNTS ac
       inner join t_Exception_Accounts_Cust c
          on c.account_id = ac.account_id
        JOIN v_exception_cell_no_change l
          ON l.customerid = c.customer_id
      
        join t_exception_eng_branches b
          on b.engid = ac.eng_id
       where ac.ENG_ID = E_ID
         and trunc(l.datecreated) between trunc(b.audit_startdate) and
             trunc(b.audit_enddate);
  
    COMMIT;
  END;

  PROCEDURE P_POP_EXCEPTION_ZAKAT(E_ID IN NUMBER) IS
  BEGIN
    delete from T_EXCEPTION_ENG e
     where e.eng_id = E_ID
       and e.er_id = 7;
    commit;
    INSERT INTO T_EXCEPTION_ENG_DATA
      (ENG_ID,
       ACCOUNT_ID,
       ER_ID,
       ACCOUNT_NO,
       TITLE,
       CNIC,
       CELL,
       ACCOUNT_TYPE,
       DATE_DISP,
       TEXT_1,
       TEXT_2)
      SELECT ac.eng_id,
             l.accountid,
             7 AS code,
             ac.account_no,
             ac.title,
             c.cnic,
             c.phone_cell,
             ac.account_type,
             l.histupdatedon,
             REGEXP_SUBSTR(l.description, '\(([^)]+)\)', 1, 1, NULL, 1) AS zakat_status_from,
             REGEXP_SUBSTR(l.description, '\(([^)]+)\)', 1, 2, NULL, 1) AS zakat_status_to
        FROM T_EXCEPTION_ACCOUNTS ac
       INNER JOIN t_exception_accounts_cust c
          ON c.account_id = ac.account_id
        JOIN v_exp_account_status_log l
          ON ac.ACCOUNT_ID = l.accountid
        JOIN t_exception_eng_branches b
          ON b.engid = ac.eng_id
       WHERE ac.ENG_ID = E_ID
         AND TRUNC(l.histupdatedon) BETWEEN TRUNC(b.audit_startdate) AND
             TRUNC(b.audit_enddate)
         AND l.description LIKE '%ZAKAT%';
  
    COMMIT;
  END;

  Procedure T_AU_EXCEPTION_REPORT(E_ID      number,
                                  P_NO      number,
                                  ENT_ID    number,
                                  R_ID      number,
                                  io_cursor OUT t_cursor) is
  begin
    if E_ID > 0 then
      open io_cursor for
        select t.R_ID,
               t.r_report_title as report_title,
               t.r_discription as discription,
               t.r_ind ind,
               t.status,
               t.loan_status,
               Trunc(p.operation_startdate) || '-' ||
               trunc(p.operation_enddate) as REPORTING_PERIOD,
               COUNT(distinct r.account_id) AS EXCEPTION_COUNT -- will be 0 if no matching rows
          from T_AU_EXCEPTION_REPORTS t
         inner join T_EXCEPTION_ENG_Data e
            on t.r_id = e.er_id
         inner join t_au_plan_eng p
            on p.eng_id = e.eng_id
         inner JOIN T_EXCEPTION_ENG_DATA r
            ON r.eng_id = e.eng_id
           AND r.er_id = e.er_id
         where p.eng_id = E_ID
         group by t.R_ID,
                  t.r_report_title,
                  t.r_discription,
                  t.r_ind,
                  t.status,
                  t.loan_status,
                  Trunc(p.operation_startdate),
                  trunc(p.operation_enddate)
         order by t.r_id;
    else
      open io_cursor for
        select t.R_ID,
               t.r_report_title as report_title,
               t.r_discription  as discription,
               t.r_ind          ind,
               t.status,
               t.loan_status,
               null             as REPORTING_PERIOD,
               0                AS EXCEPTION_COUNT -- will be 0 if no matching rows
          from T_AU_EXCEPTION_REPORTS t
         where t.status = 'Y';
    end if;
  end T_AU_EXCEPTION_REPORT;

  Procedure P_Add_new_exp_report(IND          varchar2,
                                 REPORT_ID    number,
                                 REPORT_TITLE varchar2,
                                 DESCRIPTION  varchar2,
                                 R_TYPE       varchar2,
                                 L_Status     number,
                                 P_NO         number,
                                 R_ID         number,
                                 ENT_ID       number,
                                 io_cursor    OUT t_cursor) is
  begin
    if (IND = 'A') then
      insert into T_AU_EXCEPTION_REPORTS
        (R_ID, R_REPORT_TITLE, R_DISCRIPTION, R_IND, STATUS, LOAN_STATUS)
      values
        ((SELECT COALESCE(max(PP.R_ID) + 1, 1)
           FROM T_AU_EXCEPTION_REPORTS PP),
         REPORT_TITLE,
         DESCRIPTION,
         R_TYPE,
         'Y',
         0);
      commit;
      open io_cursor for
        select 'Report Added' as remarks from dual;
    else
      if IND = 'U' then
        update T_AU_EXCEPTION_REPORTS s
           set s.R_report_title = REPORT_TITLE,
               s.R_discription  = DESCRIPTION,
               s.R_ind          = R_TYPE
         where s.R_id = REPORT_ID;
        commit;
        open io_cursor for
          select 'Report Updated' as remarks from dual;
      end if;
    end if;
  
  end P_Add_new_exp_report;

  PROCEDURE P_GET_EXCEPTION_REPORT_FORMAT(P_R_ID    IN NUMBER,
                                          IO_CURSOR OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT F.FORMAT_ID,
             F.R_ID,
             COLUMN_NAME,
             COLUMN_HEADER,
             DATA_TYPE,
             COLUMN_ORDER,
             F.IS_ACTIVE
        FROM T_AU_EXCEPTION_REPORTS_FORMAT F
       WHERE R_ID = P_R_ID
         AND IS_ACTIVE = 'Y'
       ORDER BY COLUMN_ORDER;
  END P_GET_EXCEPTION_REPORT_FORMAT;

  PROCEDURE P_INSERT_EXCEPTION_REPORT_FORMAT(P_R_ID          IN NUMBER,
                                             P_COLUMN_NAME   IN VARCHAR2,
                                             P_COLUMN_HEADER IN VARCHAR2,
                                             P_COLUMN_ORDER  IN NUMBER,
                                             P_DATA_TYPE     IN VARCHAR2,
                                             O_FORMAT_ID     OUT NUMBER) AS
  BEGIN
    INSERT INTO T_AU_EXCEPTION_REPORTS_FORMAT
      (R_ID,
       COLUMN_NAME,
       COLUMN_HEADER,
       COLUMN_ORDER,
       DATA_TYPE,
       IS_ACTIVE)
    VALUES
      (P_R_ID,
       P_COLUMN_NAME,
       P_COLUMN_HEADER,
       P_COLUMN_ORDER,
       P_DATA_TYPE,
       'Y')
    RETURNING FORMAT_ID INTO O_FORMAT_ID;
  END P_INSERT_EXCEPTION_REPORT_FORMAT;

  PROCEDURE P_UPDATE_EXCEPTION_REPORT_FORMAT(P_FORMAT_ID     IN NUMBER,
                                             P_COLUMN_HEADER IN VARCHAR2,
                                             P_COLUMN_ORDER  IN NUMBER,
                                             P_DATA_TYPE     IN VARCHAR2,
                                             P_IS_ACTIVE     IN VARCHAR2) AS
  BEGIN
    if (P_IS_ACTIVE = 'N') then
      delete from T_AU_EXCEPTION_REPORTS_FORMAT d
       where d.format_id = P_FORMAT_ID;
    else
      UPDATE T_AU_EXCEPTION_REPORTS_FORMAT
         SET COLUMN_HEADER = P_COLUMN_HEADER,
             COLUMN_ORDER  = P_COLUMN_ORDER,
             DATA_TYPE     = P_DATA_TYPE,
             IS_ACTIVE     = P_IS_ACTIVE
       WHERE FORMAT_ID = P_FORMAT_ID;
    end if;
  
  END;

  PROCEDURE P_GET_EXCEPTION_REPORT_DATA(P_R_ID     IN NUMBER,
                                        P_ENG_ID   IN NUMBER,
                                        IO_CURSOR1 OUT SYS_REFCURSOR, -- FORMAT
                                        IO_CURSOR2 OUT SYS_REFCURSOR -- DATA
                                        ) AS
  BEGIN
    ----------------------------------------------------------------
    -- Cursor 1: Column Format
    ----------------------------------------------------------------
    OPEN IO_CURSOR1 FOR
      SELECT FORMAT_ID,
             R_ID,
             COLUMN_NAME,
             COLUMN_HEADER,
             COLUMN_ORDER,
             DATA_TYPE,
             is_active
        FROM T_AU_EXCEPTION_REPORTS_FORMAT
       WHERE R_ID = P_R_ID
         AND IS_ACTIVE = 'Y'
       ORDER BY COLUMN_ORDER;
  
    ----------------------------------------------------------------
    -- Cursor 2: Actual Data (from unified view)
    ----------------------------------------------------------------
    OPEN IO_CURSOR2 FOR
      SELECT r.exc_eng_id,
             r.eng_id,
             r.er_id,
             r.account_no,
             r.code,
             r.lc_no,
             r.title,
             r.date_disp,
             r.cell,
             r.cnic,
             r.account_purpose,
             r.account_type,
             r.text_1,
             r.text_2,
             r.dr_amount,
             r.cr_amount,
             r.net_amount,
             r.remarks
        FROM T_EXCEPTION_ENG_DATA r
       WHERE r.ER_ID = P_R_ID
         AND r.ENG_ID = P_ENG_ID;
  
  END;

  procedure P_GET_LOAN_STATUS(io_cursor OUT t_cursor) is
  begin
    open io_cursor for
      select s.loan_status_id,
             s.loan_status_code,
             s.description,
             s.active,
             s.fk_rowid
        from v_get_loan_status s;
  
  end P_GET_LOAN_STATUS;

  ---------------------
  PROCEDURE P_LOG_EXCEPTION_EVENT(p_eng_id          IN NUMBER,
                                  p_proc_name       IN VARCHAR2,
                                  p_status          IN VARCHAR2,
                                  p_error_message   IN VARCHAR2,
                                  p_error_stack     IN VARCHAR2,
                                  p_error_backtrace IN VARCHAR2,
                                  p_failed_step     IN VARCHAR2) IS
  BEGIN
    INSERT INTO T_EXCEPTION_ERROR_LOG
      (ENG_ID,
       PROCEDURE_NAME,
       STATUS,
       ERROR_MESSAGE,
       ERROR_STACK,
       ERROR_BACKTRACE,
       FAILED_STEP,
       LOG_DATE)
    VALUES
      (p_eng_id,
       p_proc_name,
       p_status,
       p_error_message,
       p_error_stack,
       p_error_backtrace,
       p_failed_step,
       SYSDATE);
  END P_LOG_EXCEPTION_EVENT;

  PROCEDURE P_SEND_EXCEPTION_ALERT(p_eng_id IN NUMBER,
                                   p_step   IN VARCHAR2,
                                   p_errmsg IN VARCHAR2) IS
    v_subject VARCHAR2(200);
    v_body    VARCHAR2(4000);
  BEGIN
    v_subject := 'Exception job failed for ENG_ID ' || p_eng_id;
  
    v_body := 'Exception job P_RUN_MISSING_EXCEPTIONS failed' || CHR(10) ||
              'ENG_ID : ' || p_eng_id || CHR(10) || 'Step   : ' || p_step ||
              CHR(10) || 'Error  : ' || p_errmsg || CHR(10);
  
    -- TODO: replace with your actual email-sending procedure
    -- Example (adjust to your mail package signature):
    -- PKG_MAIL.P_SEND_MAIL(
    --     p_to      => 'isad.alerts@ztbl.com.pk',
    --     p_cc      => NULL,
    --     p_subject => v_subject,
    --     p_body    => v_body);
  
    NULL; -- keep it compiling until you plug real email
  END P_SEND_EXCEPTION_ALERT;

  PROCEDURE P_GET_EXCEPTION_MONITOR(io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT eng_id,
             period_id,
             branch_code,
             branch_name,
             exc_accounts,
             exc_rows,
             last_status,
             last_error_message,
             last_failed_step,
             last_log_date
        FROM V_EXCEPTION_ERROR_MONITOR;
  END P_GET_EXCEPTION_MONITOR;

  PROCEDURE P_GET_EXCEPTION_MONITOR_ENTITIES(IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT e.eng_id AS eng_id,
             ent.entity_id AS ent_id,
             ent.name AS ent_name,
             COUNT(DISTINCT e.eng_id) AS total_eng,
             COUNT(DISTINCT ex.eng_id) AS eng_with_exc,
             
             MAX(log.log_date) AS last_run_date,
             TO_CHAR(TRUNC(MAX(log.log_date)),
                     'DD-Mon-YYYY',
                     'NLS_DATE_LANGUAGE=ENGLISH') AS last_run_date_disp
      
        FROM t_au_plan_eng e
        JOIN t_auditee_entities ent
          ON e.entity_id = ent.entity_id
       inner JOIN T_EXCEPTION_ENG_data ex
          ON ex.eng_id = e.eng_id
        left JOIN T_EXCEPTION_ERROR_LOG log
          ON log.eng_id = e.eng_id
       WHERE e.period_id > 3
         and e.status < 12
       GROUP BY e.eng_id, ent.entity_id, ent.name
       ORDER BY name;
  END P_GET_EXCEPTION_MONITOR_ENTITIES;

  PROCEDURE P_REGENERATE_EXCEPTION(P_ENG_ID IN NUMBER, P_ER_ID IN NUMBER) IS
    -- for logging
    v_errmsg    VARCHAR2(500);
    v_err_stack CLOB;
    v_err_bt    CLOB;
  BEGIN
    ----------------------------------------------------------------
    -- 1. Remove existing exception markers for this ENG + report
    ----------------------------------------------------------------
    DELETE FROM T_EXCEPTION_ENG
     WHERE ENG_ID = P_ENG_ID
       AND ER_ID = P_ER_ID;
  
    COMMIT;
  
    ----------------------------------------------------------------
    -- 2. Rebuild base data
    ----------------------------------------------------------------
    P_PREP_EXCEPTION_BASE(P_ENG_ID);
  
    ----------------------------------------------------------------
    -- 3. Run the specific exception-populating procedure
    ----------------------------------------------------------------
    IF P_ER_ID = 1 THEN
      P_POP_EXCEPTION_CTR(P_ENG_ID);
    ELSIF P_ER_ID = 2 THEN
      P_POP_EXCEPTION_CNIC_EXPIRY(P_ENG_ID);
    ELSIF P_ER_ID = 3 THEN
      P_POP_EXCEPTION_NEG_BAL(P_ENG_ID);
    ELSIF P_ER_ID = 4 THEN
      P_POP_EXCEPTION_EMP_ACCOUNTS(P_ENG_ID);
      -- ELSIF P_ER_ID = 5 THEN
      --  P_POP_EXCEPTION_HIGH_TRUN_OVER(P_ENG_ID);
    ELSIF P_ER_ID = 6 THEN
      P_POP_EXCEPTION_HIGH_TRUN_OVER(P_ENG_ID);
    
    ELSE
      NULL; -- future exception types
    END IF;
  
    COMMIT;
  
    ----------------------------------------------------------------
    -- 4. Success log
    ----------------------------------------------------------------
    INSERT INTO T_EXCEPTION_ERROR_LOG
      (ENG_ID,
       PROCEDURE_NAME,
       ERROR_MESSAGE,
       ERROR_STACK,
       ERROR_BACKTRACE,
       FAILED_STEP,
       LOG_DATE)
    VALUES
      (P_ENG_ID,
       'P_REGENERATE_EXCEPTION',
       'SUCCESS',
       NULL,
       NULL,
       'ER_ID=' || P_ER_ID,
       SYSDATE);
  
    COMMIT;
  
  EXCEPTION
    WHEN OTHERS THEN
      ----------------------------------------------------------------
      -- Capture values in PL/SQL variables FIRST (critical)
      ----------------------------------------------------------------
      v_errmsg    := SUBSTR(SQLERRM, 1, 500);
      v_err_stack := DBMS_UTILITY.format_error_stack;
      v_err_bt    := DBMS_UTILITY.format_error_backtrace;
    
      INSERT INTO T_EXCEPTION_ERROR_LOG
        (ENG_ID,
         PROCEDURE_NAME,
         ERROR_MESSAGE,
         ERROR_STACK,
         ERROR_BACKTRACE,
         FAILED_STEP,
         LOG_DATE)
      VALUES
        (P_ENG_ID,
         'P_REGENERATE_EXCEPTION',
         v_errmsg,
         v_err_stack,
         v_err_bt,
         'ER_ID=' || P_ER_ID,
         SYSDATE);
    
      COMMIT;
      RAISE;
  END P_REGENERATE_EXCEPTION;

  PROCEDURE P_GET_EXCEPTION_MONITOR_DETAILS(P_ENG_ID  IN NUMBER,
                                            io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT rpt.r_id AS ER_ID, -- always from master table
             e.eng_id AS ENG_ID,
             ent.entity_id AS ENT_ID,
             trunc(e.audit_startdate) || '-' || trunc(e.audit_enddate) AS ExecutionDates,
             Trunc(e.operation_startdate) || '-' ||
             trunc(e.operation_enddate) as ReportingPeriod,
             rpt.r_report_title AS REPORT_TITLE,
             rpt.r_ind as Report_Type,
             COUNT(distinct ep.account_id) AS EXC_COUNT -- will be 0 if no matching rows
        FROM t_au_plan_eng e
        JOIN t_auditee_entities ent
          ON ent.entity_id = e.entity_id
      
      -- get ALL reports (you can add a filter on rpt.ind/status if needed)
        JOIN t_au_exception_reports rpt
          ON 1 = 1
      
      -- exception engine, optional per report
        LEFT JOIN T_EXCEPTION_ENG_data ep
          ON ep.eng_id = e.eng_id
         AND ep.er_id = rpt.r_id
      
      -- detailed exceptions, optional per report
/*        LEFT JOIN V_EXCEPTION_ALL_REPORTS r
          ON r.eng_id = e.eng_id
         AND r.er_id = rpt.r_id*/
      
       WHERE e.eng_id = P_ENG_ID -- or 2744 in your ad-hoc test
       GROUP BY rpt.r_id,
                e.eng_id,
                ent.entity_id,
                e.audit_startdate,
                e.audit_enddate,
                e.operation_startdate,
                e.operation_enddate,
                rpt.r_report_title,
                rpt.r_ind
       order by rpt.r_id;
  
  END P_GET_EXCEPTION_MONITOR_DETAILS;

  PROCEDURE P_GET_LOANS_EXCEPTIONS(LStatus  IN NUMBER,
                                   E_ID     IN NUMBER,
                                   P_NO     IN NUMBER,
                                   R_ID     IN NUMBER,
                                   ENT_ID   IN NUMBER,
                                   T_CURSOR OUT SYS_REFCURSOR) IS
    v_branch_code VARCHAR2(50);
    v_start_date  DATE;
    v_end_date    DATE;
  BEGIN
    /* Get engagement context */
    SELECT e.branch_code, e.audit_startdate, e.audit_enddate
      INTO v_branch_code, v_start_date, v_end_date
      FROM t_au_plan_eng e
     WHERE e.eng_id = E_ID;
  
    /* Return loans for this engagement branch + audit period, filtered by status */
    OPEN T_CURSOR FOR
      SELECT l.loan_disb_id,
             l.TYPE,
             l.SCHEME,
             l.L_PURPOSE,
             l.LC_NO,
             l.CNIC,
             l.CUSTOMERNAME,
             l.APP_DATE,
             l.APP_DATE_DISP,
             l.DISB_DATE,
             l.DISB_DATE_DISP,
             l.DEV_AMOUNT,
             l.OUTSTANDING
        FROM v_P_GET_LOANS_EXCEPTIONS l
       WHERE 1 = 1
            /* IMPORTANT: adjust column name if your view uses BRANCHID instead of BRANCH_CODE */
         AND l.code = v_branch_code
            /* Date range without TRUNC (better for indexes) */
         AND l.disb_date >= v_start_date
         AND l.disb_date < (v_end_date + 1)
            /* Status filter */
         AND (LStatus IS NULL OR LStatus = 0 OR l.l_status = LStatus);
  
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      /* If ENG_ID not found, return empty cursor safely */
      OPEN T_CURSOR FOR
        SELECT CAST(NULL AS NUMBER) AS loan_disb_id,
               CAST(NULL AS VARCHAR2(50)) AS "TYPE",
               CAST(NULL AS VARCHAR2(50)) AS "SCHEME",
               CAST(NULL AS VARCHAR2(200)) AS "L_PURPOSE",
               CAST(NULL AS VARCHAR2(100)) AS "LC_NO",
               CAST(NULL AS VARCHAR2(25)) AS "CNIC",
               CAST(NULL AS VARCHAR2(200)) AS "CUSTOMERNAME",
               CAST(NULL AS DATE) AS "APP_DATE",
               CAST(NULL AS VARCHAR2(20)) AS "APP_DATE_DISP",
               CAST(NULL AS DATE) AS "DISB_DATE",
               CAST(NULL AS VARCHAR2(20)) AS "DISB_DATE_DISP",
               CAST(NULL AS NUMBER) AS "DEV_AMOUNT",
               CAST(NULL AS NUMBER) AS "OUTSTANDING"
          FROM dual
         WHERE 1 = 0;
  END P_GET_LOANS_EXCEPTIONS;

end PKG_SM;

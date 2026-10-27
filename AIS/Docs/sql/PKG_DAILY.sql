create or replace package PKG_DAILY is

  -- Author  : ASAD
  -- Created : 27-10-2026 2:48:25 PM
  -- Purpose : Day end procedures

  TYPE t_cursor IS REF CURSOR;

  procedure Session_Kill_day_end(io_cursor OUT t_cursor);
  
  procedure HR_DATA;
  
  procedure HR_AUDIT_DATA;

end PKG_DAILY;
/
create or replace package body PKG_DAILY is

  procedure Session_Kill_day_end(io_cursor OUT t_cursor) is
  
  begin
    Open io_cursor for
      select count(*)
        from t_user_session s
       where s.session_active = 'Y'
         and trunc(s.logged_in_date) = trunc(sysdate);
  
    update t_user_session s
       set s.session_active = 'N'
     where s.session_active = 'Y';
    commit;
    pkg_daily.HR_DATA;
  

  End Session_Kill_day_end;

  procedure HR_DATA is
  begin
    delete from v_service_employeeinfo;
    commit;
    insert into v_service_employeeinfo
      select * from V_SERVICE_EMPLOYEEINFO_VIEW;
    commit;
  
    delete from V_GET_ACTIVE_EMPLOYEES;
    commit;
  
    insert into V_GET_ACTIVE_EMPLOYEES
      (PPNO,
       CURRENTDIVISIONCODE,
       CURRENTDEPARTMENTCODE,
       CURRENTZONECODE,
       CURRENTBRANCHCODE,
       CURRENTDESIGNATIONCODE,
       EMPLOYEETYPE)
      select * from V_GET_ACTIVE_EMPLOYEES_VIEW;
    commit;
    pkg_daily.HR_AUDIT_DATA;
  end HR_DATA;

  procedure HR_AUDIT_DATA is
  begin
    DELETE FROM t_audit_emp;
    COMMIT;
    INSERT INTO t_audit_emp
      (Ppno,
       Departmentcode,
       Deptarment,
       Employeefirstname,
       Employeelastname,
       Rankcode,
       Current_Rank,
       Designationcode,
       Fun_Designation,
       Type,
       Entity_Id)
      SELECT E.ppno,
             E.departmentcode,
             E.description,
             E.employeefirstname,
             E.employeelastname,
             E.currentrankcode,
             E.CURRENT_RANK,
             E.designationcode,
             E.FUN_DESIGNATION,
             E.type,
             E.ENTITY_ID
        FROM V_AUDIT_EMP E;
    COMMIT;
  
  end HR_AUDIT_DATA;

end PKG_DAILY;

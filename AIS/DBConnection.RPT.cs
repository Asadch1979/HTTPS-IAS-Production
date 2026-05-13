using AIS.Models;
using AIS.Models.Reports;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public List<AuditZoneItem> GetAuditZones()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditZoneItem>();
                }
            var zones = new List<AuditZoneItem>();

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_RPT.P_GET_AUDIT_ZONES";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            zones.Add(new AuditZoneItem
                                {
                                ZoneId = SafeGetInt32(reader, "ZONE_ID"),
                                ZoneName = SafeGetString(reader, "ZONE_NAME")
                                });
                            }
                        }
                    }
                }

            return zones;
            }

        public List<DepartmentModel> GetDepartments(int div_code = 0, bool sessionCheck = true)
            {
            var sessionHandler = CreateSessionHandler();
            var deptList = new List<DepartmentModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<DepartmentModel>();
                }

                var entityId = 0;
                if (div_code == 0)
                    entityId = Convert.ToInt32(loggedInUser.UserEntityID);
                else
                    entityId = div_code;

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.R_GetDepartments";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("EntityId", OracleDbType.Int32).Value = entityId;
                    cmd.Parameters.Add("PPNUM", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    GuardAgainstDynamicSql(cmd);
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        DepartmentModel dept = new DepartmentModel();
                        dept.ID = Convert.ToInt32(rdr["ID"]);
                        dept.DIV_ID = Convert.ToInt32(rdr["DIVISIONID"]);
                        dept.NAME = rdr["NAME"].ToString();
                        dept.CODE = rdr["CODE"].ToString();
                        if (rdr["ISACTIVE"].ToString() == "Y")
                            dept.STATUS = "Active";
                        else if (rdr["ISACTIVE"].ToString() == "N")
                            dept.STATUS = "InActive";
                        else
                            dept.STATUS = rdr["ISACTIVE"].ToString();
                        dept.DIV_NAME = rdr["DIV_NAME"].ToString();
                        if (rdr["AUDITED_BY_DEPID"].ToString() != null && rdr["AUDITED_BY_DEPID"].ToString() != "")
                            {
                            // dept.AUDITED_BY_NAME = rdr["ADUTIED_BY"].ToString();

                            dept.AUDITED_BY_DEPID = Convert.ToInt32(rdr["AUDITED_BY_DEPID"]);
                            cmd.Parameters.Clear();
                            cmd.CommandText = "PKG_RPT.P_GetDepartments";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("E_id", OracleDbType.Int32).Value = 3;
                            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                            GuardAgainstDynamicSql(cmd);
                            OracleDataReader rdr2 = cmd.ExecuteReader();
                            while (rdr2.Read())
                                {
                                dept.AUDITED_BY_NAME = rdr2["NAME"].ToString();
                                }
                            }
                        deptList.Add(dept);
                        }
                    }
                }
            return deptList;
            }


        public List<SettledParasMonitoringModel> GetSettledParasForMonitoring(int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<SettledParasMonitoringModel>();
                }

            List<SettledParasMonitoringModel> list = new List<SettledParasMonitoringModel>();
            using (var con = this.DatabaseConnection())
                {
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_RPT.P_GET_SETTLED_PARA_DETAILS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("AUDITEE_ID", OracleDbType.Int32).Value = ENTITY_ID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        SettledParasMonitoringModel chk = new SettledParasMonitoringModel();
                        chk.REPORTING_OFFICE = rdr["REPORTING_OFFICE"].ToString();
                        chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                        chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                        chk.COM_ID = rdr["COM_ID"].ToString();
                        chk.SETTLED_BY = rdr["SETTLED_BY"].ToString();
                        chk.SETTLED_ON = rdr["SETTLED_ON"].ToString();
                        chk.RISK = rdr["RISK"].ToString();
                        chk.PARA_NO = rdr["PARA_NO"].ToString();
                        chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                        chk.COMPLIANCE_CYCLE = rdr["COMPLIANCE_CYCLE"].ToString();
                        chk.AUDITED_BY = rdr["AUDITEDBY"].ToString();
                        chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                        list.Add(chk);
                        }
                    }
                }

            return list;
            }

        public List<PostComplianceHistoryModel> GetSettledParaComplianceHistory(string COM_ID)
            {
            List<PostComplianceHistoryModel> stList = new List<PostComplianceHistoryModel>();
            using (var con = this.DatabaseConnection())
                {
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_RPT.P_GetParasForCompliancehistory";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_COM_ID", OracleDbType.Int32).Value = COM_ID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        PostComplianceHistoryModel st = new PostComplianceHistoryModel();
                        st.HIST_ID = Convert.ToInt32(rdr["HIST_ID"].ToString());
                        st.COM_ID = Convert.ToInt32(rdr["COM_ID"].ToString());
                        st.COM_CYCLE = rdr["COM_CYCLE"].ToString();
                        st.COM_STAGE = rdr["COM_STAGE"].ToString();
                        st.COM_STATUS = rdr["COM_STATUS"].ToString();
                        st.COMMENT_BY_ROLE = rdr["COMMENT_BY_ROLE"].ToString();
                        st.NAME = rdr["NAME"].ToString();
                        st.DESIGNATION = rdr["DESIGNATION"].ToString();
                        st.PP_NO = rdr["PP_NO"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["PP_NO"]);
                        st.COMMENT_ON = rdr["COMMENT_ON"].ToString();
                        st.COMMENTS = rdr["COMMENTS"].ToString();
                        st.COM_FLOW = rdr["COM_FLOW"].ToString();
                        stList.Add(st);
                        }
                    }
                }

            return stList;
            }

        public GetOldParasBranchComplianceTextModel GetComplianceCycleText(string COM_ID, string C_CYCLE)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new GetOldParasBranchComplianceTextModel();
                }

            GetOldParasBranchComplianceTextModel resp = new GetOldParasBranchComplianceTextModel();
            using (var con = this.DatabaseConnection())
                {
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_RPT.P_GetParasForComplianceforhistory";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_C_CYCLE", OracleDbType.Int32).Value = C_CYCLE;
                    cmd.Parameters.Add("P_COM_ID", OracleDbType.Int32).Value = COM_ID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        resp.PARA_TEXT = rdr["REPLY"].ToString();
                        resp.PARA_TEXT_ID = rdr["TEXT_ID"].ToString();
                        resp.OBS_TEXT = rdr["PARA_TEXT"].ToString();
                        resp.PARA_NO = rdr["PARA_NO"].ToString();
                        resp.GIST_OF_PARA = rdr["GIST_OF_PARAS"].ToString();
                        }
                    }
                }

            return resp;
            }



        public List<UserRelationshipModel> GetchildpostingForParaPositionReport(int e_r_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<UserRelationshipModel>();
                }

            if (e_r_id == 0)
                e_r_id = Convert.ToInt32(loggedInUser.UserEntityID);

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_RPT.P_Getchildposting";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("erid", OracleDbType.Int32).Value = e_r_id;
                    cmd.Parameters.Add("USER_ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        UserRelationshipModel entity = new UserRelationshipModel();
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                        entity.C_NAME = rdr["C_NAME"].ToString();
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }

        public List<UserRelationshipModel> GetparentrepofficeForParaPositionReport(int r_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<UserRelationshipModel>();
                }
            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_Getparentrepoffice";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("rid", OracleDbType.Int32).Value = r_id;
                    cmd.Parameters.Add("user_entity_id", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        UserRelationshipModel entity = new UserRelationshipModel();
                        entity.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"]);
                        entity.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                        entity.ACTIVE = rdr["ACTIVE"].ToString();
                        entity.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                        entity.ENTITYTYPEDESC = rdr["ENTITYTYPEDESC"].ToString();
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }


        public List<UserRelationshipModel> GetrealtionshiptypeForParaPositionReport()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<UserRelationshipModel>();
                }

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_Getrealtionshiptype";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("UserRoleid", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        UserRelationshipModel entity = new UserRelationshipModel();
                        entity.ENTITY_REALTION_ID = Convert.ToInt32(rdr["ENTITY_REALTION_ID"]);
                        entity.FIELD_NAME = rdr["FIELD_NAME"].ToString();
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }



        public List<JoiningCompletionReportModel> GetJoiningCompletion(int DEPT_ID, DateTime AUDIT_STARTDATE, DateTime AUDIT_ENDDATE)
            {
            List<JoiningCompletionReportModel> list = new List<JoiningCompletionReportModel>();
            var effectiveDeptId = DEPT_ID;
            if (effectiveDeptId <= 0)
                {
                var loggedInUser = CreateSessionHandler().GetUser();
                if (loggedInUser == null || loggedInUser.UserEntityID.GetValueOrDefault() <= 0)
                    {
                    return list;
                    }

                effectiveDeptId = loggedInUser.UserEntityID.GetValueOrDefault();
                }

            using (var con = this.DatabaseConnection())
                {
               
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.R_JOININGCOMPLETION";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("DEPT_ID", OracleDbType.Int32).Value = effectiveDeptId;
                    cmd.Parameters.Add("AUDIT_START", OracleDbType.Date).Value = AUDIT_STARTDATE;
                    cmd.Parameters.Add("AUDIT_END", OracleDbType.Date).Value = AUDIT_ENDDATE;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        JoiningCompletionReportModel jc = new JoiningCompletionReportModel();

                        jc.AUDIT_BY = rdr["AUDIT_BY"].ToString();
                        jc.Reporting = rdr["REPORTING"].ToString();
                        jc.CODE = rdr["CODE"].ToString();
                        jc.AUDITEE_NAME = rdr["AUDITEE_NAME"].ToString();
                        jc.Risk = rdr["RISK"].ToString();
                        jc.START_DATE = rdr["START_DATE"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(rdr["START_DATE"]);
                        jc.END_DATE = rdr["END_DATE"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(rdr["END_DATE"]);
                        jc.STATUS = rdr["STATUS"].ToString();
                        jc.Issuancedate = rdr["ISSUANCE_DATE"] == DBNull.Value
                            ? string.Empty
                            : Convert.ToDateTime(rdr["ISSUANCE_DATE"]).ToString("dd-MM-yy");
                        list.Add(jc);
                        }
                    }
                }
            return list;
            }

        public List<AuditeeAddressModel> GetAddress(int ENT_ID)
            {
            var sessionHandler = CreateSessionHandler();

            List<AuditeeAddressModel> list = new List<AuditeeAddressModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditeeAddressModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.r_getauditeeaddress";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("EngId", OracleDbType.Int32).Value = ENT_ID;
                    cmd.Parameters.Add("ppnum", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        AuditeeAddressModel chk = new AuditeeAddressModel();
                        chk.ENG_ID = Convert.ToInt32(rdr["ENG_ID"]);
                        chk.CODE = Convert.ToInt32(rdr["CODE"]);


                        chk.P_NAME = rdr["P_NAME"].ToString();
                        chk.ADDRESS = rdr["ADDRESS"].ToString();
                        chk.NAME = rdr["NAME"].ToString();
                        chk.LICENSE = rdr["LICENSE"].ToString();
                        if (rdr["DATE_OF_OPENING"].ToString() != null && rdr["DATE_OF_OPENING"].ToString() != "")
                            chk.DATE_OF_OPENING = Convert.ToDateTime(rdr["DATE_OF_OPENING"].ToString()).ToString("dd/MM/yyyy");
                        if (rdr["AUDIT_STARTDATE"].ToString() != null && rdr["AUDIT_STARTDATE"].ToString() != "")
                            chk.AUDIT_STARTDATE = Convert.ToDateTime(rdr["AUDIT_STARTDATE"].ToString()).ToString("dd/MM/yyyy");
                        if (rdr["AUDIT_ENDDATE"].ToString() != null && rdr["AUDIT_ENDDATE"].ToString() != "")
                            chk.AUDIT_ENDDATE = Convert.ToDateTime(rdr["AUDIT_ENDDATE"].ToString()).ToString("dd/MM/yyyy");
                        if (rdr["OPERATION_STARTDATE"].ToString() != null && rdr["OPERATION_STARTDATE"].ToString() != "")
                            chk.OPERATION_STARTDATE = Convert.ToDateTime(rdr["OPERATION_STARTDATE"].ToString()).ToString("dd/MM/yyyy");
                        if (rdr["OPERATION_ENDDATE"].ToString() != null && rdr["OPERATION_ENDDATE"].ToString() != "")
                            chk.OPERATION_ENDDATE = Convert.ToDateTime(rdr["OPERATION_ENDDATE"].ToString()).ToString("dd/MM/yyyy");


                        var highValue = rdr["HIGH"].ToString();
                        var mediumValue = rdr["MEDIUM"].ToString();
                        var lowValue = rdr["LOW"].ToString();

                        chk.HIGH = string.IsNullOrEmpty(highValue) ? "0" : highValue;
                        chk.MEDIUM = string.IsNullOrEmpty(mediumValue) ? "0" : mediumValue;
                        chk.LOW = string.IsNullOrEmpty(lowValue) ? "0" : lowValue;

                        var settledHigh = rdr["SETTLE_HIGH"].ToString();
                        var settledMedium = rdr["SETTLE_MEDIUM"].ToString();
                        var settledLow = rdr["SETTLE_LOW"].ToString();

                        chk.SETTLED_HIGH = string.IsNullOrEmpty(settledHigh) ? "0" : settledHigh;
                        chk.SETTLED_MEDIUM = string.IsNullOrEmpty(settledMedium) ? "0" : settledMedium;
                        chk.SETTLED_LOW = string.IsNullOrEmpty(settledLow) ? "0" : settledLow;

                        var openHigh = rdr["OPEN_HIGH"].ToString();
                        var openMedium = rdr["OPEN_MEDIUM"].ToString();
                        var openLow = rdr["OPEN_LOW"].ToString();

                        chk.OPEN_HIGH = string.IsNullOrEmpty(openHigh) ? "0" : openHigh;
                        chk.OPEN_MEDIUM = string.IsNullOrEmpty(openMedium) ? "0" : openMedium;
                        chk.OPEN_LOW = string.IsNullOrEmpty(openLow) ? "0" : openLow;

                        list.Add(chk);
                        }
                    }
                }
            return list;
            }


        public List<GetTeamDetailsModel> GetTeamDetails(int eng_id)
            {
            var sessionHandler = CreateSessionHandler();

            List<GetTeamDetailsModel> list = new List<GetTeamDetailsModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<GetTeamDetailsModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.p_getauditteams";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("userentityid", OracleDbType.Int32).Value = eng_id;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        GetTeamDetailsModel chk = new GetTeamDetailsModel();

                        chk.TEAM_NAME = rdr["TEAM_NAME"].ToString();
                        chk.MEMBER_PPNO = rdr["MEMBER_PPNO"].ToString();
                        chk.MEMBER_NAME = rdr["MEMBER_NAME"].ToString();
                        chk.ISTEAMLEAD = rdr["ISTEAMLEAD"].ToString();
                        chk.AUDIT_START_DATE = Convert.ToDateTime(rdr["AUDIT_START_DATE"].ToString()).ToString("dd/MM/yyyy");
                        chk.AUDIT_END_DATE = Convert.ToDateTime(rdr["AUDIT_END_DATE"].ToString()).ToString("dd/MM/yyyy");

                        list.Add(chk);
                        }
                    }
                }
            return list;
            }

        public List<GetFinalReportModel> GetAuditeeParas(int eng_id)
            {
            var sessionHandler = CreateSessionHandler();

            List<GetFinalReportModel> list = new List<GetFinalReportModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<GetFinalReportModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.r_getauditeeparas";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("engid", OracleDbType.Int32).Value = 1198;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        GetFinalReportModel chk = new GetFinalReportModel();


                        chk.MEMO_NUMBER = rdr["MEMO_NUMBER"].ToString();
                        chk.PARA_NO = rdr["PARA_NO"].ToString();
                        //chk.STATUS = rdr["STATUS"].ToString();
                        chk.V_HEADER = rdr["V_HEADER"].ToString();
                        chk.V_DETAIL = rdr["V_DETAIL"].ToString();
                        chk.RISK = rdr["RISK"].ToString();
                        chk.OBSERVATION = rdr["OBSERVATION"].ToString();
                        chk.MANAGEMENT_REPLY = rdr["MANAGEMENT_REPLY"].ToString();
                        chk.RECOMMENDATION = rdr["RECOMMENDATION"].ToString();
                        // chk.REMARKS = rdr["REMARKS"].ToString();
                        chk.ASSIGNEDTO = rdr["ASSIGNEDTO"].ToString();
                        chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                        chk.REF_OUT = rdr["REF_OUT"].ToString();
                        chk.MESSAGE = rdr["MESSAGE"].ToString();


                        list.Add(chk);
                        }
                    }
                }
            return list;
            }


















        public List<AuditParaReconsillation> GetAuditParaRensillation()
            {

            var sessionHandler = CreateSessionHandler();

            List<AuditParaReconsillation> resp = new List<AuditParaReconsillation>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditParaReconsillation>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_FAD_audit_Para_Reconciliation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        AuditParaReconsillation rd = new AuditParaReconsillation();

                        rd.AUDIT_ZONE = rdr["Audit_zone"].ToString();
                        rd.ENTITY_TYPE_DESC = rdr["entitytypedesc"].ToString();
                        rd.REPORTING_OFFICE = rdr["Reporting_Office"].ToString();
                        rd.ENTITY_NAME = rdr["Auditee"].ToString();
                        rd.OPEN_BALANCE = rdr["Open_balance"].ToString();
                        rd.ADDED = rdr["Added"].ToString();
                        rd.TOTAL = rdr["Total"].ToString();
                        rd.SETTLED_LEGACY = rdr["Settled_Legacy"].ToString();
                        rd.SETTLED_NEW_PARA = rdr["Settled_New_Paras"].ToString();
                        rd.UN_SETTLED = rdr["Un_Settled"].ToString();
                        rd.INDICATOR = rdr["ind"].ToString();
                        rd.PERCENTAGE = rdr["percentage"].ToString();
                        rd.R1 = rdr["r1"].ToString();
                        rd.R2 = rdr["r2"].ToString();
                        rd.R3 = rdr["r3"].ToString();
                        resp.Add(rd);

                        }
                    }
                }
            return resp;

            }





        public List<AuditeeEntitiesModel> GetEntityTypesForEntityWiseOutstandingObsPosition()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditeeEntitiesModel>();
                }

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_ENTITY_TYPE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                        entity.NAME = rdr["ENTITY_TYPE"].ToString();
                        entity.CODE = Convert.ToInt32(rdr["entitycode"].ToString());
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }


        public List<AuditeeEntitiesModel> GetLoanStatus()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditeeEntitiesModel>();
                }

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.p_get_loan_status";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                        entity.NAME = rdr["description"].ToString();
                        entity.CODE = Convert.ToInt32(rdr["accountstatusid"].ToString());
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }



        public List<AuditeeEntitiesModel> GetRBHList(int REGION_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditeeEntitiesModel>();
                }

            List<AuditeeEntitiesModel> entitiesList = new List<AuditeeEntitiesModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.R_get_rbh_list";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("gm", OracleDbType.Int32).Value = REGION_ID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        AuditeeEntitiesModel entity = new AuditeeEntitiesModel();
                        entity.NAME = rdr["name"].ToString();
                        entity.CODE = Convert.ToInt32(rdr["entity_id"].ToString());
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }




        public List<GISTWiseReportParas> GetAuditReportParaByGistKeyword(string GIST)
            {
            var sessionHandler = CreateSessionHandler();

            List<GISTWiseReportParas> resp = new List<GISTWiseReportParas>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<GISTWiseReportParas>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_find_gist";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("GST", OracleDbType.Varchar2).Value = GIST;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        GISTWiseReportParas mp = new GISTWiseReportParas();
                        mp.REGION = rdr["region"].ToString();
                        mp.AUDIT_ZONE = rdr["AUDIT_ZONE"].ToString();
                        mp.BRANCH = rdr["BRANCH"].ToString();
                        mp.BRANCH_CODE = rdr["BRANCH_CODE"].ToString();
                        mp.E_DATE = rdr["entereddate"].ToString();
                        mp.PARA_NO = rdr["para_no"].ToString();
                        mp.ANNEX = rdr["annex"].ToString();
                        mp.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                        mp.NO_OF_INSTANCES = rdr["no_of_instances"].ToString();
                        mp.AMOUNT_INVOLVED = rdr["amount_involved"].ToString();
                        resp.Add(mp);
                        }
                    }
                }
            return resp;
            }

        public List<ComplianceProgressReportModel> GetComplianceProgressReport(string ROLE_TYPE)
            {
            var sessionHandler = CreateSessionHandler();

            List<ComplianceProgressReportModel> resp = new List<ComplianceProgressReportModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<ComplianceProgressReportModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_COM_PROGREE_REPORT ";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("R_TYPE", OracleDbType.Varchar2).Value = ROLE_TYPE;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        ComplianceProgressReportModel cp = new ComplianceProgressReportModel();
                        cp.PPNO = rdr["PP_NO"].ToString();
                        cp.NAME = rdr["Name"].ToString();
                        cp.TOTAL = rdr["Total"].ToString();
                        cp.REFERRED_BACK = rdr["Referred_Back"].ToString();
                        cp.RECOMMENDED = rdr["Recommended"].ToString();
                        cp.PENDING = rdr["Pending"].ToString();
                        if (rdr["last_login"] != DBNull.Value && rdr["last_login"] != null)
                            {
                            cp.LAST_LOGIN_ON = rdr["last_login"].ToString();
                            }
                        resp.Add(cp);
                        }
                    }
                }
            return resp;

            }

        public List<ComplianceProgressReportDetailModel> GetComplianceProgressReportDetails(string ROLE_TYPE, int? PP_NO)
            {
            var sessionHandler = CreateSessionHandler();

            List<ComplianceProgressReportDetailModel> resp = new List<ComplianceProgressReportDetailModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<ComplianceProgressReportDetailModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_COM_PROGREE_REPORT_DETAIL ";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("R_TYPE", OracleDbType.Varchar2).Value = ROLE_TYPE;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = PP_NO ?? (object)DBNull.Value;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        ComplianceProgressReportDetailModel cp = new ComplianceProgressReportDetailModel();
                        cp.COMPLIANCE_UNIT = rdr["Compliance_Unit"].ToString();
                        cp.PARENT_ID = rdr["parent_id"].ToString();
                        cp.PARENT_NAME = rdr["p_name"].ToString();
                        cp.ENTITY_ID = rdr["entity_id"].ToString();
                        cp.ENTITY_NAME = rdr["name"].ToString();
                        cp.ENTITY_CODE = rdr["code"].ToString();
                        cp.COM_KEY = rdr["COM_KEY"].ToString();
                        cp.PP_NO = rdr["PP_NO"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["PP_NO"]);
                        cp.EMP_NAME = rdr["emp_name"].ToString();
                        cp.TOTAL = rdr["Total"].ToString();
                        cp.REFERRED_BACK = rdr["Refered_back"].ToString();
                        cp.RECOMMENDED = rdr["Satisfied"].ToString();
                        cp.PENDING = rdr["Pending"].ToString();

                        resp.Add(cp);
                        }
                    }
                }
            return resp;
            }

        public List<AuditEntitiesModel> GetEntityTypesForSettlementReport()
            {


            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (var con = this.DatabaseConnection())

                {

               
                var sessionHandler = CreateSessionHandler();
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<AuditEntitiesModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_ENTITY_TYPE_FOR_SETTLEMENT";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        AuditEntitiesModel entity = new AuditEntitiesModel();
                        entity.AUTID = Convert.ToInt32(rdr["autid"]);
                        entity.ENTITYTYPEDESC = rdr["entitytypedesc"].ToString();
                        entitiesList.Add(entity);
                        }
                    }
                }
            return entitiesList;

            }

        public List<SettledParasModel> GetSettledParasForComplianceReport(int ENTITY_TYPE_ID, DateTime? DATE_FROM, DateTime? DATE_TO)
            {
            var sessionHandler = CreateSessionHandler();

            List<SettledParasModel> resp = new List<SettledParasModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<SettledParasModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_COMPLIANCE_REPORT";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("S_ENT_ID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                    cmd.Parameters.Add("S_DATE_FROM", OracleDbType.Date).Value = DATE_FROM;
                    cmd.Parameters.Add("S_DATE_TO", OracleDbType.Date).Value = DATE_TO;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        SettledParasModel cp = new SettledParasModel();
                        cp.ENTITY_ID = rdr["entity_id"].ToString();
                        cp.PARENT_ID = rdr["parent_id"].ToString();
                        cp.REPORTING_OFFICE = rdr["p_name"].ToString();
                        cp.PLACE_OF_POSTING = rdr["c_name"].ToString();
                        cp.AUDIT_PERIOD = rdr["period"].ToString();
                        cp.PARA_NO = rdr["para_no"].ToString();
                        cp.GIST = rdr["Gist"].ToString();
                        cp.SETTLED_ON = rdr["setteled_on"].ToString();
                        cp.AUDITED_BY = rdr["auditedby"].ToString();
                        resp.Add(cp);
                        }
                    }
                }
            return resp;
            }


        public List<EngPlanDelayAnalysisReportModel> GetEngagementPlanDelayAnalysisReport()
            {
            var sessionHandler = CreateSessionHandler();

            List<EngPlanDelayAnalysisReportModel> resp = new List<EngPlanDelayAnalysisReportModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<EngPlanDelayAnalysisReportModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.p_delay_audits";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        EngPlanDelayAnalysisReportModel cp = new EngPlanDelayAnalysisReportModel();
                        cp.ENTITY_ID = rdr["entity_id"].ToString();
                        cp.REPORTING_OFFICE = rdr["p_name"].ToString();
                        cp.PLACE_OF_POSTING = rdr["c_name"].ToString();
                        cp.ENTITY_NAME = rdr["name"].ToString();
                        cp.AUDIT_START_DATE = rdr["audit_startdate"].ToString();
                        cp.AUDIT_END_DATE = rdr["audit_enddate"].ToString();
                        cp.STATUS = rdr["status"].ToString();
                        cp.DELAY_DAYS = rdr["no_of_days"].ToString();
                        resp.Add(cp);
                        }
                    }
                }
            return resp;
            }

        public List<FADMonthlyReviewParasModel> GetFADMonthlyReviewParasForEntityTypeId(string ENT_TYPE_ID, DateTime? S_DATE, DateTime? E_DATE)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<FADMonthlyReviewParasModel>();
                }
            var list = new List<FADMonthlyReviewParasModel>();
            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_rpt.R_FAD_MONTHLY_REVIEW";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("R_T", OracleDbType.Int32).Value = ENT_TYPE_ID;
                        cmd.Parameters.Add("S_DATE", OracleDbType.Date).Value = S_DATE;
                        cmd.Parameters.Add("E_DATE", OracleDbType.Date).Value = E_DATE;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                var review = new FADMonthlyReviewParasModel
                                    {
                                    REPORTING_OFFICE = rdr["P_NAME"].ToString(),
                                    PLACE_OF_POSTING = rdr["C_NAME"].ToString(),
                                    CHILD_CODE = rdr["CHILD_CODE"].ToString(),
                                    OPENING_BALANCE = rdr["opening_bal"].ToString(),
                                    PARA_ADDED = rdr["Para_added"].ToString(),
                                    SETTLED_COM = rdr["Settled_com"].ToString(),
                                    SETTLED_AUDIT = rdr["Settled_aud"].ToString(),
                                    OUTSTANDING = rdr["Outstanding"].ToString(),
                                    R1 = rdr["r1"].ToString(),
                                    R2 = rdr["r2"].ToString(),
                                    R3 = rdr["r3"].ToString()
                                    };


                                list.Add(review);
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return list;
            }

        public List<SeriousFraudulentObsGM> GetSeriousFraudulentObsGMOverview()
            {
            var sessionHandler = CreateSessionHandler();

            List<SeriousFraudulentObsGM> list = new List<SeriousFraudulentObsGM>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<SeriousFraudulentObsGM>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_GM_WISE_SERIOUS_PARAS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("R_T", OracleDbType.Int32).Value = 0;
                    cmd.Parameters.Add("S_DATE", OracleDbType.Date).Value = new DateTime();
                    cmd.Parameters.Add("E_DATE", OracleDbType.Date).Value = new DateTime();
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        SeriousFraudulentObsGM chk = new SeriousFraudulentObsGM();
                        chk.PARENT_ID = Convert.ToInt32(rdr["PARENT_ID"].ToString());
                        chk.P_NAME = rdr["P_NAME"].ToString();
                        chk.TOTAL_NO = rdr["total_before_current_year"].ToString();
                        chk.C_TOTAL_NO = rdr["total_in_current_year"].ToString(); // Total no of Serious Observation in CURRENT
                        chk.A1 = rdr["a1_before_current_year"].ToString();
                        chk.C_A1 = rdr["a1_in_current_year"].ToString();
                        chk.AMOUNT = rdr["c_amount_before_current_year"].ToString();
                        chk.C_AMOUNT = rdr["c_amount_in_current_year"].ToString();
                        chk.PER_INV = rdr["PER_INV"].ToString();
                        chk.C_PER_INV = rdr["C_PER_INV"].ToString();

                        list.Add(chk);
                        }
                    }
                }
            return list;
            }

        public List<SeriousFraudulentObsGMDetails> GetSeriousFraudulentObsGMDetails(string INDICATOR, int PARENT_ENT_ID, string ANNEX_IND)
            {
            var sessionHandler = CreateSessionHandler();

            List<SeriousFraudulentObsGMDetails> list = new List<SeriousFraudulentObsGMDetails>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<SeriousFraudulentObsGMDetails>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_GM_WISE_SERIOUS_PARAS_DETAILS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("PARENT_ENT_ID", OracleDbType.Int32).Value = PARENT_ENT_ID;
                    cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                    cmd.Parameters.Add("P_ANNEX", OracleDbType.Varchar2).Value = ANNEX_IND;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;

                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        {
                        SeriousFraudulentObsGMDetails chk = new SeriousFraudulentObsGMDetails();
                        chk.P_NAME = rdr["p_name"].ToString();
                        chk.C_NAME = rdr["c_name"].ToString();
                        chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                        chk.PARA_NO = rdr["para_no"].ToString();
                        chk.ANNEX_HEADING = rdr["heading"].ToString();
                        chk.RISK = rdr["risk"].ToString();
                        chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                        chk.AMOUNT_INVOLVED = rdr["amount_involved"].ToString();

                        list.Add(chk);
                        }
                    }
                }
            return list;
            }



        public List<ParaTextSearchModel> GetAuditParasByText(string SEARCH_KEYWORD)
            {
            var sessionHandler = CreateSessionHandler();

            List<ParaTextSearchModel> list = new List<ParaTextSearchModel>();
            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<ParaTextSearchModel>();
                }
                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_rpt.P_GET_PARA_TEXT_WORDS_V2";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("T_TEXT", OracleDbType.Varchar2).Value = SEARCH_KEYWORD;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        ParaTextSearchModel chk = new ParaTextSearchModel
                            {
                            AUDIT_ZONE = rdr["name"]?.ToString(),
                            PARENT_NAME = rdr["p_name"]?.ToString(),
                            CHILD_NAME = rdr["c_name"]?.ToString(),
                            AUDIT_PERIOD = rdr["audit_period"]?.ToString(),
                            ANNEXURE = rdr["annex"]?.ToString(),
                            PARA_NO = rdr["para_no"]?.ToString(),
                            GIST_OF_PARAS = rdr["gist_of_paras"]?.ToString(),
                            TEXT = rdr["text"]?.ToString()

                            };

                        list.Add(chk);
                        }


                    }
                }
            return list;
            }



        public List<FadDeskOfficerRptModel> GetFadDeskOfficerRptByDateRange(DateTime startDate, DateTime endDate)
            {
            // Session pattern for logged-in user (if required by package)
            var sessionHandler = CreateSessionHandler();



            List<FadDeskOfficerRptModel> results = new List<FadDeskOfficerRptModel>();
            using (var con = this.DatabaseConnection())

                {

               

                using (var cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_RPT.P_GET_FAD_DESK_OFFICER_RPT_BY_PERIOD";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_start_date", OracleDbType.Date).Value = startDate;
                    cmd.Parameters.Add("p_end_date", OracleDbType.Date).Value = endDate;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            FadDeskOfficerRptModel rpt = new FadDeskOfficerRptModel
                                {
                                AuditPeriod = reader["AUDIT_PERIOD"]?.ToString(),
                                ChildCode = reader["CHILD_CODE"]?.ToString(),
                                CName = reader["C_NAME"]?.ToString(),
                                AZ = reader["AZ"]?.ToString(),
                                PName = reader["P_NAME"]?.ToString(),
                                Annex = reader["ANNEX"]?.ToString(),
                                GistOfParas = reader["GIST_OF_PARAS"]?.ToString(),
                                ParaNo = reader["PARA_NO"]?.ToString(),
                                NoOfInstances = reader["NO_OF_INSTANCES"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NO_OF_INSTANCES"]),
                                Risk = reader["RISK"]?.ToString(),
                                Amount = reader["AMOUNT"]?.ToString(),
                                Status = reader["para_status"]?.ToString(),
                                };
                            results.Add(rpt);
                            }
                        }
                    }
                }
            return results;
            }

        private static string SafeGetString(IDataRecord record, string column)
            {
            return record[column] == DBNull.Value ? string.Empty : record[column].ToString();
            }

        private static int SafeGetInt32(IDataRecord record, string column)
            {
            return record[column] == DBNull.Value ? 0 : Convert.ToInt32(record[column]);
            }

        private static decimal SafeGetDecimal(IDataRecord record, string column)
            {
            return record[column] == DBNull.Value ? 0m : Convert.ToDecimal(record[column]);
            }
        }
    }

using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
        public partial class DBConnection : Controller, IDBConnection
            {
            private static string CleanDbMessage(string message)
                {
                if (string.IsNullOrWhiteSpace(message))
                    {
                    return string.Empty;
                    }

                var normalized = message.Replace("\r", "\n");
                var parts = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var combined = string.Join(" ", parts);
                combined = Regex.Replace(combined, @"\s+", " ").Trim();

                var oraMatch = Regex.Match(combined, @"ORA-\d+:\s*(.*)");
                if (oraMatch.Success)
                    {
                    combined = oraMatch.Groups[1].Value;
                    combined = Regex.Replace(combined, @"\s*ORA-\d+:\s*.*", string.Empty).Trim();
                    }

                return combined;
                }
        public List<AuditEntitiesModel> GetAuditEntitiesForOtherEntitySelection()
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_get_auditee_submission_list";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel entity = new AuditEntitiesModel();
                    entity.AUTID = Convert.ToInt32(rdr["entity_id"]);
                    entity.ENTITYTYPEDESC = rdr["name"].ToString();

                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<RiskGroupModel> GetRiskGroup()
            {
            var con = this.DatabaseConnection();
            List<RiskGroupModel> riskgroupList = new List<RiskGroupModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetRiskGroup";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    RiskGroupModel rgm = new RiskGroupModel();
                    rgm.GR_ID = Convert.ToInt32(rdr["GR_ID"]);
                    rgm.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    riskgroupList.Add(rgm);
                    }
                }

            con.Dispose();
            return riskgroupList;
            }

        public List<RiskSubGroupModel> GetRiskSubGroup(int group_id)
            {
            var con = this.DatabaseConnection();
            List<RiskSubGroupModel> risksubgroupList = new List<RiskSubGroupModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetRiskSubGroup";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("group_id", OracleDbType.Int32).Value = group_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    RiskSubGroupModel rsgm = new RiskSubGroupModel();
                    rsgm.S_GR_ID = Convert.ToInt32(rdr["S_GR_ID"]);
                    rsgm.GR_ID = Convert.ToInt32(rdr["GR_ID"]);
                    rsgm.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    rsgm.GROUP_DESC = rdr["GROUP_DESC"].ToString();
                    risksubgroupList.Add(rsgm);
                    }
                }
            con.Dispose();
            return risksubgroupList;
            }

        public List<RiskActivityModel> GetRiskActivities(int Sub_group_id)
            {
            var con = this.DatabaseConnection();
            List<RiskActivityModel> riskActivityList = new List<RiskActivityModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_GetRiskActivities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Sub_group_id", OracleDbType.Int32).Value = Sub_group_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    RiskActivityModel ram = new RiskActivityModel();
                    ram.S_GR_ID = Convert.ToInt32(rdr["S_GR_ID"]);
                    ram.ACTIVITY_ID = Convert.ToInt32(rdr["ACTIVITY_ID"]);
                    ram.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    ram.SUB_GROUP_DESC = rdr["SUB_GROUP_DESC"].ToString();
                    riskActivityList.Add(ram);
                    }
                }
            con.Dispose();
            return riskActivityList;
            }

        public List<AuditVoilationcatModel> GetAuditVoilationcats()
            {
            var con = this.DatabaseConnection();
            List<AuditVoilationcatModel> voilationList = new List<AuditVoilationcatModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetAuditVoilationcats";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditVoilationcatModel voilationcat = new AuditVoilationcatModel();
                    voilationcat.ID = Convert.ToInt32(rdr["ID"]);
                    voilationcat.V_NAME = rdr["V_Name"].ToString();
                    voilationcat.MAX_NUMBER = Convert.ToInt32(rdr["MAX_Number"]);
                    voilationcat.STATUS = rdr["Status"].ToString();
                    voilationList.Add(voilationcat);
                    }
                }
            con.Dispose();
            return voilationList;
            }

        public List<AuditSubVoilationcatModel> GetVoilationSubGroup(int group_id)
            {
            var con = this.DatabaseConnection();
            List<AuditSubVoilationcatModel> voilationsubgroupList = new List<AuditSubVoilationcatModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetVoilationSubGroup";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("group_id", OracleDbType.Int32).Value = group_id;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditSubVoilationcatModel vsgm = new AuditSubVoilationcatModel();
                    vsgm.ID = Convert.ToInt32(rdr["ID"]);
                    if (rdr["V_ID"].ToString() != "")
                        vsgm.V_ID = Convert.ToInt32(rdr["V_ID"]);
                    else
                        vsgm.V_ID = 0;
                    vsgm.SUB_V_NAME = rdr["SUB_V_NAME"].ToString();
                    vsgm.RISK_ID = rdr["RISK_ID"].ToString();
                    vsgm.STATUS = rdr["STATUS"].ToString();
                    voilationsubgroupList.Add(vsgm);
                    }
                }
            con.Dispose();
            return voilationsubgroupList;
            }

        public List<TaskListModel> GetTaskList()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            List<TaskListModel> tasklist = new List<TaskListModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetTaskList";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    TaskListModel tlist = new TaskListModel();
                    tlist.ID = Convert.ToInt32(rdr["ID"]);
                    tlist.ENG_PLAN_ID = Convert.ToInt32(rdr["ENG_PLAN_ID"]);
                    tlist.TEAM_ID = Convert.ToInt32(rdr["TEAM_ID"]);
                    tlist.SEQUENCE_NO = Convert.ToInt32(rdr["SEQUENCE_NO"]);
                    tlist.TEAMMEMBER_PPNO = Convert.ToInt32(rdr["TEAMMEMBER_PPNO"]);
                    tlist.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    tlist.ENTITY_TYPE = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    tlist.REPORTING = rdr["P_NAME"].ToString();
                    tlist.ENTITY_TYPE_DESC = rdr["ENTITY_TYPE_DESC"].ToString();
                    tlist.ENTITY_CODE = Convert.ToInt32(rdr["ENTITY_CODE"]);
                    tlist.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    tlist.TEAM_NAME = rdr["T_NAME"].ToString();
                    tlist.WORKING_PAPER = rdr["WORKING_PAPER"].ToString();
                    tlist.PRE_INFO = rdr["pre_info"].ToString();
                    tlist.EMP_NAME = loggedInUser.Name.ToString();
                    tlist.AUDIT_START_DATE = Convert.ToDateTime(rdr["AUDIT_START_DATE"]).ToString("dd/MM/yyyy"); ;
                    tlist.AUDIT_END_DATE = Convert.ToDateTime(rdr["AUDIT_END_DATE"]).ToString("dd/MM/yyyy"); ;
                    tlist.STATUS_ID = Convert.ToInt32(rdr["STATUS_ID"]);
                    tlist.ENG_STATUS = rdr["ENG_STATUS"].ToString();
                    tlist.ENG_NEXT_STATUS = rdr["ENG_NEXT_STATUS"].ToString();
                    tlist.ISACTIVE = rdr["ISACTIVE"].ToString();
                    tlist.AUDIT_YEAR = rdr["AUDIT_YEAR"].ToString();
                    tlist.ISCLOSE = rdr["CLOSING"].ToString();
                    if (rdr["OPERATION_STARTDATE"].ToString() != null && rdr["OPERATION_STARTDATE"].ToString() != "")
                        tlist.OPERATION_STARTDATE = Convert.ToDateTime(rdr["OPERATION_STARTDATE"]);
                    if (rdr["OPERATION_ENDDATE"].ToString() != null && rdr["OPERATION_ENDDATE"].ToString() != "")
                        tlist.OPERATION_ENDDATE = Convert.ToDateTime(rdr["OPERATION_ENDDATE"]);
                    tasklist.Add(tlist);
                    }
                }
            con.Dispose();
            return tasklist;
            }

        public JoiningModel GetJoiningDetails(int engId = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            JoiningModel jm = new JoiningModel();
            List<JoiningTeamModel> tjlist = new List<JoiningTeamModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (engId == 0)
                engId = this.GetLoggedInUserEngId();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetJoiningDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENG", OracleDbType.Int32).Value = engId;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    jm.ENG_PLAN_ID = engId;
                    jm.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    jm.ENTITY_CODE = Convert.ToInt32(rdr["ENTITY_CODE"]);
                    jm.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    jm.STATUS = "";
                    jm.RISK = rdr["RISK"].ToString();
                    jm.SIZE = rdr["ENT_SIZE"].ToString();
                    jm.START_DATE = Convert.ToDateTime(rdr["AUDIT_START_DATE"]);
                    jm.END_DATE = Convert.ToDateTime(rdr["AUDIT_END_DATE"]);
                    jm.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    jm.TEAM_NAME = rdr["TEAM_NAME"].ToString();
                    jm.AUDITEE_EMAIL = rdr["EMAIL_ADDRESS"].ToString();
                    jm.AUDITEE_PHONE = rdr["TELEPHONE"].ToString();
                    JoiningTeamModel tm = new JoiningTeamModel();
                    tm.EMP_NAME = rdr["MEMBER_NAME"].ToString();
                    tm.PP_NO = Convert.ToInt32(rdr["MEMBER_PPNO"]);
                    tm.IS_TEAM_LEAD = rdr["ISTEAMLEAD"].ToString();
                    tjlist.Add(tm);
                    jm.TEAM_DETAILS = tjlist;
                    }
                }
            con.Dispose();
            return jm;
            }

        public string AddJoiningReport(AddJoiningModel jm)
            {
            var sessionHandler = CreateSessionHandler();

            string response = "";
            string toEmail = "";
            string ccEmail = "";
            string auditEntity = "";
            string teamMembers = "";
            string email = "";
            string teamLead = "";

            using (var con = this.DatabaseConnection())
                {
               
                var loggedInUser = sessionHandler.GetUserOrThrow();
                jm.ENTEREDBY = Convert.ToInt32(loggedInUser.PPNumber);
                jm.ENTEREDDATE = DateTime.Now;

                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ar.P_AddJoiningReport";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = jm.ENG_PLAN_ID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("COMPLETION_DATE", OracleDbType.Date).Value = jm.COMPLETION_DATE;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (OracleDataReader rdr = cmd.ExecuteReader())
                        {
                        while (rdr.Read())
                            {
                            this.SetEngIdOnHold();
                            response = rdr["remarks"].ToString();
                            toEmail = rdr["to_email"].ToString();
                            ccEmail = rdr["cc_email"].ToString();
                            auditEntity = rdr["auditee_name"].ToString();
                            teamLead = rdr["team_lead"]?.ToString();
                            teamMembers = rdr["team_members"]?.ToString();
                            email = rdr["email"]?.ToString();
                            }
                        }
                    }
                }
            
            if (email == "Y")
                {
                var serviceProvider = _httpCon?.HttpContext?.RequestServices;
                Task.Run(() => EmailNotification.SendJoiningNotificationAsync(_configuration, toEmail, ccEmail, auditEntity, teamLead, teamMembers, serviceProvider));
                }
            return response;
            }

        public List<AuditChecklistModel> GetAuditChecklist()
            {
            var con = this.DatabaseConnection();
            List<AuditChecklistModel> list = new List<AuditChecklistModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_GetAuditChecklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistModel chk = new AuditChecklistModel();
                    chk.T_ID = Convert.ToInt32(rdr["T_ID"]);
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.RISK_SEQUENCE = rdr["RISK_SEQUENCE"].ToString();
                    chk.RISK_WEIGHTAGE = rdr["WEIGHT_ASSIGNED"].ToString();
                    chk.ENTITY_TYPE = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    chk.ENTITY_TYPE_NAME = rdr["ENTITY_TYPE_NAME"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistModel> GetAuditChecklistCAD()
            {
            var con = this.DatabaseConnection();
            List<AuditChecklistModel> list = new List<AuditChecklistModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_GetAuditChecklistCAD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistModel chk = new AuditChecklistModel();
                    chk.T_ID = Convert.ToInt32(rdr["T_ID"]);
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.ENTITY_TYPE = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    //chk.ENTITY_TYPE_NAME = rdr["ENTITY_TYPE_NAME"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistSubModel> GetAuditChecklistSub(int t_id = 0, int eng_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditChecklistSubModel> list = new List<AuditChecklistSubModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_GetAuditChecklistSub";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("tid", OracleDbType.Int32).Value = t_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistSubModel chk = new AuditChecklistSubModel();
                    chk.S_ID = Convert.ToInt32(rdr["S_ID"]);
                    chk.T_ID = Convert.ToInt32(rdr["T_ID"]);
                    chk.T_NAME = rdr["T_NAME"].ToString();
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.ENTITY_TYPE = Convert.ToInt32(rdr["ENTITY_TYPE"]);
                    chk.ENTITY_TYPE_NAME = rdr["ENTITY_TYPE_NAME"].ToString();
                    chk.STATUS = "Pending";
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistDetailsModel> GetAuditChecklistDetails(int s_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditChecklistDetailsModel> list = new List<AuditChecklistDetailsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetAuditChecklistDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("sid", OracleDbType.Int32).Value = s_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditChecklistDetailsModel chk = new AuditChecklistDetailsModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.S_ID = Convert.ToInt32(rdr["S_ID"]);
                    chk.S_NAME = rdr["S_NAME"].ToString();
                    chk.V_ID = Convert.ToInt32(rdr["V_ID"]);
                    chk.V_NAME = rdr["V_NAME"].ToString();
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    chk.RISK = rdr["RISK"].ToString();
                    if (rdr["ROLE_RESP_ID"].ToString() != null && rdr["ROLE_RESP_ID"].ToString() != "")
                        {
                        chk.ROLE_RESP_ID = Convert.ToInt32(rdr["ROLE_RESP_ID"]);
                        }
                    if (rdr["PROCESS_OWNER_ID"].ToString() != null && rdr["PROCESS_OWNER_ID"].ToString() != "")
                        {
                        chk.PROCESS_OWNER_ID = Convert.ToInt32(rdr["PROCESS_OWNER_ID"]);

                        }
                    chk.STATUS = rdr["STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string SaveAuditObservation(ObservationModel ob)
            {
            int addedObsId = 0;
            string returnResp = "";
            bool proceed = false;

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var entityId = loggedInUser.UserEntityID.GetValueOrDefault();
            var skipResponsibility = entityId == 112242 || entityId == 112248;  // EXCLUDE these entities from responsibility assignment
            var isSpecialEntity = skipResponsibility;                           // keep your existing “special entity defaults” behavior

            var subChecklistId = ob.SUBCHECKLIST_ID;
            var annexureId = ob.ANNEXURE_ID;
            var checklistDetailId = ob.CHECKLISTDETAIL_ID;
            var noOfInstances = ob.NO_OF_INSTANCES;
            var amountInvolved = ob.AMOUNT_INVOLVED;
            var responsiblePpnoList = ob.RESPONSIBLE_PPNO;

            // Keep your original behavior for these entities (defaults for optional fields)
            if (isSpecialEntity)
                {
                subChecklistId ??= 0;

                if (string.IsNullOrWhiteSpace(annexureId))
                    annexureId = "0";

                checklistDetailId ??= 0;

                if (string.IsNullOrWhiteSpace(noOfInstances))
                    noOfInstances = "0";

                if (string.IsNullOrWhiteSpace(amountInvolved))
                    amountInvolved = "0";

                responsiblePpnoList ??= new List<ObservationResponsiblePPNOModel>();
                }

            var INDICATOR = "A";

            if (ob.ENGPLANID == 0)
                ob.ENGPLANID = this.GetLoggedInUserEngId();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_SaveAuditObservation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("PLANID", OracleDbType.Int32).Value = ob.ENGPLANID;
                cmd.Parameters.Add("STATUS", OracleDbType.Int32).Value = ob.STATUS;
                cmd.Parameters.Add("REPLYDATE", OracleDbType.Date).Value = ob.REPLYDATE;
                cmd.Parameters.Add("ENTEREDBY", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("Severity", OracleDbType.Int32).Value = ob.SEVERITY;

                cmd.Parameters.Add("SUBCHECKLISTID", OracleDbType.Int32).Value = subChecklistId;
                cmd.Parameters.Add("CHECKLISTDETAILID", OracleDbType.Int32).Value = checklistDetailId;

                cmd.Parameters.Add("VCATID", OracleDbType.Int32).Value = ob.V_CAT_ID;
                cmd.Parameters.Add("VCATNATUREID", OracleDbType.Int32).Value = ob.V_CAT_NATURE_ID;

                cmd.Parameters.Add("TEXT_DATA", OracleDbType.Clob).Value = ob.OBSERVATION_TEXT;
                cmd.Parameters.Add("NOINSTANCES", OracleDbType.Int32).Value = noOfInstances;
                cmd.Parameters.Add("AMOUNT_INV", OracleDbType.Int32).Value = amountInvolved;

                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = ob.HEADING;
                cmd.Parameters.Add("OT_ENT_ID", OracleDbType.Int32).Value = ob.OTHER_ENTITY_ID;

                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;

                cmd.Parameters.Add("ANNEX_ID", OracleDbType.Int32).Value = annexureId;

                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    var refVal = rdr["REF"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(refVal) && refVal == "2")
                        {
                        returnResp = CleanDbMessage(rdr["remarks"]?.ToString());
                        }
                    else if (!string.IsNullOrWhiteSpace(refVal) && refVal == "1")
                        {
                        addedObsId = Convert.ToInt32(rdr["ID"]?.ToString());
                        returnResp = CleanDbMessage(rdr["remarks"]?.ToString());
                        proceed = true;
                        }
                    }

                if (proceed)
                    {
                    // IMPORTANT: Skip responsibility assignment for excluded entities
                    if (!skipResponsibility && responsiblePpnoList != null && responsiblePpnoList.Count > 0 && addedObsId > 0)
                        {
                        foreach (ObservationResponsiblePPNOModel pp in responsiblePpnoList)
                            {
                            // int is already non-null; int? may be null
                            int ppNo = pp.PP_NO;
                            int loanCase = pp.LOAN_CASE ?? 0;
                            int accountNumber = pp.ACCOUNT_NUMBER;     // if you keep it non-nullable int
                            int lcAmount = pp.LC_AMOUNT ?? 0;
                            int accAmount = pp.ACC_AMOUNT;             // if you keep it non-nullable int

                            // If you want extra safety (e.g. negative not allowed), clamp:
                            if (ppNo < 0) ppNo = 0;
                            if (loanCase < 0) loanCase = 0;
                            if (accountNumber < 0) accountNumber = 0;
                            if (lcAmount < 0) lcAmount = 0;
                            if (accAmount < 0) accAmount = 0;

                            cmd.CommandText = "pkg_ar.P_responibilityassigned";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();

                            cmd.Parameters.Add("N_ID", OracleDbType.Int32).Value = addedObsId;
                            cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ob.ENGPLANID;
                            cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = 0;
                            cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                            cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;

                            // RES_PP is Int32: bind as int (not string)
                            cmd.Parameters.Add("RES_PP", OracleDbType.Int32).Value = Convert.ToInt32(ppNo);

                            cmd.Parameters.Add("LOANCASE", OracleDbType.Int32).Value = Convert.ToInt32(loanCase);
                            cmd.Parameters.Add("ACCNUMBER", OracleDbType.Int32).Value = Convert.ToInt32(accountNumber);
                            cmd.Parameters.Add("LCAMOUNT", OracleDbType.Int32).Value = Convert.ToInt32(lcAmount);
                            cmd.Parameters.Add("ACAMOUNT", OracleDbType.Int32).Value = Convert.ToInt32(accAmount);

                            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                            cmd.ExecuteReader();
                            }
                        }
                    }
                }

            con.Dispose();
            return returnResp;
            }

        public string SaveAuditObservationCAU(ObservationModel ob)
            {

            int addedObsId = 0;
            string returnResp = "";
            bool proceed = false;
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var INDICATOR = "A";
            if (ob.ENGPLANID == 0)
                ob.ENGPLANID = this.GetLoggedInUserEngId();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_SaveAuditObservationCAD";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PLANID", OracleDbType.Int32).Value = ob.ENGPLANID;
                cmd.Parameters.Add("STATUS", OracleDbType.Int32).Value = ob.STATUS;
                cmd.Parameters.Add("REPLYDATE", OracleDbType.Date).Value = ob.REPLYDATE;
                cmd.Parameters.Add("ENTEREDBY", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("Severity", OracleDbType.Int32).Value = ob.SEVERITY;
                cmd.Parameters.Add("SUBCHECKLISTID", OracleDbType.Varchar2).Value = ob.SUBCHECKLIST_ID;
                cmd.Parameters.Add("CHECKLISTDETAILID", OracleDbType.Int32).Value = ob.CHECKLISTDETAIL_ID;
                cmd.Parameters.Add("TEXT_DATA", OracleDbType.Clob).Value = ob.OBSERVATION_TEXT;
                cmd.Parameters.Add("TITLE", OracleDbType.Varchar2).Value = ob.HEADING;
                cmd.Parameters.Add("AMOUNT_INV", OracleDbType.Int32).Value = ob.AMOUNT_INVOLVED;
                cmd.Parameters.Add("NO_INST", OracleDbType.Int32).Value = ob.NO_OF_INSTANCES;
                cmd.Parameters.Add("BRANCHID", OracleDbType.Int32).Value = ob.BRANCH_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ANNEX_ID", OracleDbType.Int32).Value = ob.ANNEXURE_ID == "" ? 0 : Convert.ToInt32(ob.ANNEXURE_ID);
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "2")
                        {
                        returnResp = CleanDbMessage(rdr["remarks"].ToString());
                        }
                    else if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "1")
                        {
                        addedObsId = Convert.ToInt32(rdr["ID"].ToString());
                        returnResp = CleanDbMessage(rdr["remarks"].ToString());
                        proceed = true;

                        }
                    }
                if (proceed)
                    {
                    if (ob.RESPONSIBLE_PPNO != null)
                        {
                        if (ob.RESPONSIBLE_PPNO.Count > 0 && addedObsId > 0)
                            {
                            foreach (ObservationResponsiblePPNOModel pp in ob.RESPONSIBLE_PPNO)
                                {
                                cmd.CommandText = "pkg_ar.P_responibilityassigned";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("N_ID", OracleDbType.Int32).Value = addedObsId;
                                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ob.ENGPLANID;
                                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = 0;
                                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                                cmd.Parameters.Add("RES_PP", OracleDbType.Int32).Value = pp.PP_NO;

                                cmd.Parameters.Add("LOANCASE", OracleDbType.Int32).Value =
                                    pp.LOAN_CASE ?? 0;

                                cmd.Parameters.Add("ACCNUMBER", OracleDbType.Int32).Value =
                                    pp.ACCOUNT_NUMBER;

                                cmd.Parameters.Add("LCAMOUNT", OracleDbType.Int32).Value =
                                    pp.LC_AMOUNT ?? 0;

                                cmd.Parameters.Add("ACAMOUNT", OracleDbType.Int32).Value =
                                    pp.ACC_AMOUNT;


                                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                                cmd.ExecuteReader();
                                }
                            }
                        }
                    }


                }
            con.Dispose();
            return returnResp;
            }

        public bool SetEngIdOnHold()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            int ENG_ID = this.GetLoggedInUserEngId();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_SetEngIdOnHold";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.ExecuteReader();
                }
            con.Dispose();
            return true;
            }

        public string GetLatestAuditorResponse(int obs_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string response = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetLatestAuditorResponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obs_id", OracleDbType.Int32).Value = obs_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    response = rdr["Recommendation"].ToString();
                    }
                }
            con.Dispose();
            return response;
            }

        public string GetLatestDepartmentalHeadResponse(int obs_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string response = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetLatestDepartmentalHeadResponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obs_id", OracleDbType.Int32).Value = obs_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    response = rdr["audit_reply"].ToString();
                    }
                }
            con.Dispose();
            return response;
            }

        public string GetLatestAuditeeResponse(int obs_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string response = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetLatestAuditeeResponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obs_id", OracleDbType.Int32).Value = obs_id;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    response = rdr["reply"].ToString();
                    }
                }
            con.Dispose();
            return response;
            }

        public List<ObservationSummaryModel> GetManagedObservationsSummaryForSelectedEntity(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationSummaryModel> list = new List<ObservationSummaryModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_details_for_manage_observations_summary";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationSummaryModel chk = new ObservationSummaryModel();

                    chk.ENG_ID = Convert.ToInt32(rdr["ENG_ID"]);
                    chk.PPNO = rdr["PPNO"].ToString();
                    chk.E_NAME = rdr["E_NAME"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    chk.TEAM = rdr["TEAM"].ToString();
                    chk.CREATED = rdr["CREATED"].ToString();
                    chk.SUBMIT_TO_AUDITEE = rdr["SUBMIT_TO_AUDITEE"].ToString();
                    chk.RESPONDED_BY_AUDITEE = rdr["RESPONDED_BY_AUDITEE"].ToString();
                    chk.DROP_RESOLVED_BY_TEAM_HEAD = rdr["Drop_Resolved_by_team_head"].ToString();
                    chk.ADDED_TO_DRAFT = rdr["ADDED_TO_DRAFT"].ToString();
                    chk.ADDED_TO_FINAL = rdr["ADDED_TO_FINAL"].ToString();
                    chk.SETTELED = rdr["SETTELED"].ToString();
                    chk.TOTAL = rdr["TOTAL"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ObservationRevisedModel> GetManagedObservationsForSelectedEntity(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationRevisedModel> list = new List<ObservationRevisedModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_details_for_manage_observations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationRevisedModel chk = new ObservationRevisedModel();

                    chk.OBS_ID = Convert.ToInt32(rdr["OBD_ID"]);
                    if (rdr["MEMO"].ToString() != null && rdr["MEMO"].ToString() != "")
                        chk.MEMO = Convert.ToInt32(rdr["MEMO"]);
                    if (rdr["DRAFT_PARA"].ToString() != null && rdr["DRAFT_PARA"].ToString() != "")
                        chk.DRAFT_PARA = Convert.ToInt32(rdr["DRAFT_PARA"]);
                    if (rdr["FINAL_PARA"].ToString() != null && rdr["FINAL_PARA"].ToString() != "")
                        chk.FINAL_PARA = Convert.ToInt32(rdr["FINAL_PARA"]);
                    chk.IND = rdr["IND"].ToString();
                    chk.T_IND = rdr["T_IND"].ToString();
                    chk.TITLE = rdr["Title"].ToString();
                    chk.E_NAME = rdr["e_name"].ToString();
                    chk.STATUS = rdr["status"].ToString();
                    chk.STATUS_ID = Convert.ToInt32(rdr["status_id"].ToString());

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string UpdateAuditObservationStatus(ManageAuditParasModel pm)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_Update_Audit_Paras";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("COM_ID", OracleDbType.Int32).Value = pm.COM_ID;
                cmd.Parameters.Add("N_PARA_ID", OracleDbType.Int32).Value = pm.NEW_PARA_ID;
                cmd.Parameters.Add("O_PARA_ID", OracleDbType.Int32).Value = pm.OLD_PARA_ID;
                cmd.Parameters.Add("D_PARA_NO", OracleDbType.Varchar2).Value = pm.PARA_NO;
                cmd.Parameters.Add("D_AUDIT_PERIOD", OracleDbType.Varchar2).Value = pm.AUDIT_PERIOD;
                cmd.Parameters.Add("D_GIST", OracleDbType.Varchar2).Value = pm.OBS_GIST;
                cmd.Parameters.Add("D_RISK", OracleDbType.Int32).Value = pm.OBS_RISK_ID;
                cmd.Parameters.Add("D_ANNEX", OracleDbType.Int32).Value = pm.ANNEX_ID;
                cmd.Parameters.Add("D_IND", OracleDbType.Varchar2).Value = pm.INDICATOR;
                cmd.Parameters.Add("D_PARA_TEXT", OracleDbType.Clob).Value = pm.PARA_TEXT;
                cmd.Parameters.Add("D_AMOUNT", OracleDbType.Decimal).Value = decimal.TryParse(pm.AMOUNT_INV?.Replace(",", ""),out var amt) ? amt : 0;
                cmd.Parameters.Add("D_INSTANCES", OracleDbType.Int32).Value = int.TryParse(pm.NO_INSTANCES, out var inst) ? inst : 0;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string ReferredBackAuditObservationStatus(ManageAuditParasModel pm)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_Authorize_Update_Audit_Paras";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = pm.COM_ID;
                cmd.Parameters.Add("N_PARA_ID", OracleDbType.Int32).Value = pm.NEW_PARA_ID;
                cmd.Parameters.Add("O_PARA_ID", OracleDbType.Int32).Value = pm.OLD_PARA_ID;
                cmd.Parameters.Add("D_PARA_ID", OracleDbType.Int32).Value = pm.PARA_ID;
                cmd.Parameters.Add("D_PARA_NO", OracleDbType.Varchar2).Value = pm.PARA_NO;
                cmd.Parameters.Add("D_AUDIT_PERIOD", OracleDbType.Varchar2).Value = pm.AUDIT_PERIOD;
                cmd.Parameters.Add("D_GIST", OracleDbType.Varchar2).Value = pm.OBS_GIST;
                cmd.Parameters.Add("D_RISK", OracleDbType.Int32)
                    .Value = pm.OBS_RISK_ID;
                cmd.Parameters.Add("D_ANNEX", OracleDbType.Int32)
                    .Value = pm.ANNEX_ID;
                cmd.Parameters.Add("D_IND", OracleDbType.Varchar2).Value = pm.INDICATOR;
                cmd.Parameters.Add("D_PARA_TEXT", OracleDbType.Clob).Value = pm.PARA_TEXT ?? "";
                cmd.Parameters.Add("D_AMOUNT", OracleDbType.Decimal).Value = Convert.ToDecimal(pm.AMOUNT_INV ?? "0");
                cmd.Parameters.Add("D_INSTANCES", OracleDbType.Int32).Value = Convert.ToInt32(pm.NO_INSTANCES ?? "0");
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("P_DECISION", OracleDbType.Varchar2).Value = pm.P_DECISION;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }


                }
            con.Dispose();
            return resp;
            }

        public string AuthorizedAuditObservationStatus(ManageAuditParasModel pm)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_Authorize_Update_Audit_Paras";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = pm.COM_ID;
                cmd.Parameters.Add("N_PARA_ID", OracleDbType.Int32).Value = pm.NEW_PARA_ID;
                cmd.Parameters.Add("O_PARA_ID", OracleDbType.Int32).Value = pm.OLD_PARA_ID;
                cmd.Parameters.Add("D_PARA_ID", OracleDbType.Int32).Value = pm.PARA_ID;
                cmd.Parameters.Add("D_PARA_NO", OracleDbType.Varchar2).Value = pm.PARA_NO;
                cmd.Parameters.Add("D_AUDIT_PERIOD", OracleDbType.Varchar2).Value = pm.AUDIT_PERIOD;
                cmd.Parameters.Add("D_GIST", OracleDbType.Varchar2).Value = pm.OBS_GIST;
                cmd.Parameters.Add("D_RISK", OracleDbType.Int32).Value = pm.OBS_RISK_ID;
                cmd.Parameters.Add("D_ANNEX", OracleDbType.Int32).Value = pm.ANNEX_ID;
                cmd.Parameters.Add("D_IND", OracleDbType.Varchar2).Value = pm.INDICATOR;
                cmd.Parameters.Add("D_PARA_TEXT", OracleDbType.Clob).Value = pm.PARA_TEXT;
                cmd.Parameters.Add("D_AMOUNT", OracleDbType.Decimal).Value = pm.AMOUNT_INV;
                cmd.Parameters.Add("D_INSTANCES", OracleDbType.Int32).Value = pm.NO_INSTANCES;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("P_DECISION", OracleDbType.Varchar2).Value = pm.P_DECISION;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }


                }
            con.Dispose();
            return resp;
            }

        public List<ManageObservations> GetManagedObservations(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageObservations> list = new List<ManageObservations>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();

                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"]);
                    chk.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"]);
                    if (rdr["MEMO_NO"].ToString() != null && rdr["MEMO_NO"].ToString() != "")
                        chk.MEMO_NO = Convert.ToInt32(rdr["MEMO_NO"]);
                    chk.NO_OF_INSTANCES = Convert.ToInt32(rdr["NOINSTANCES"]);
                    chk.VIOLATION = rdr["VIOLATION"].ToString();
                    chk.HEADING = rdr["HEADING"].ToString();
                    chk.NATURE = rdr["NATURE"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ManageObservations> GetManagedObservationText(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ManageObservations> list = new List<ManageObservations>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedObservationsText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();
                    chk.VIOLATION = rdr["VIOLATION"].ToString();
                    chk.NATURE = rdr["NATURE"].ToString();
                    chk.OBS_TEXT = rdr["OBS_TEXT"].ToString();
                    chk.HEADING = rdr["OBS_HEADING"].ToString();
                    if (rdr["OBS_RISK_ID"].ToString() != null && rdr["OBS_RISK_ID"].ToString() != "")
                        chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"].ToString());
                    chk.OBS_REPLY = this.GetLatestAuditeeResponse(OBS_ID);
                    chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(OBS_ID, ENG_ID);
                    chk.ATTACHED_EVIDENCES = this.GetRespondedObservationEvidences(OBS_ID);

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeResponseEvidenceModel> GetRespondedObservationEvidences(int OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeResponseEvidenceModel> modellist = new List<AuditeeResponseEvidenceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_by_obs_id";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr3 = cmd.ExecuteReader();
                while (rdr3.Read())
                    {
                    AuditeeResponseEvidenceModel am = new AuditeeResponseEvidenceModel();
                    am.FILE_ID = rdr3["ID"].ToString();
                    am.IMAGE_NAME = rdr3["FILE_NAME"].ToString();
                    am.IMAGE_DATA = "";
                    am.SEQUENCE = Convert.ToInt32(rdr3["SEQUENCE"].ToString());
                    am.IMAGE_TYPE = rdr3["FILE_TYPE"].ToString();
                    modellist.Add(am);
                    }
                }
            con.Dispose();
            return modellist;
            }

        public List<ObservationTextModel> GetManagedAllObservationsText(int OBS_ID = 0, string INDICATOR = "")
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationTextModel> list = new List<ObservationTextModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_details_for_manage_observations_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationTextModel chk = new ObservationTextModel();
                    chk.CP = rdr["CP"].ToString();
                    chk.CP_ID = rdr["CP_ID"].ToString();
                    chk.PSN = rdr["PSN"].ToString();
                    chk.PSN_ID = rdr["PSN_ID"].ToString();
                    chk.CD = rdr["CD"].ToString();
                    chk.CD_ID = rdr["CD_ID"].ToString();
                    chk.TEXT = rdr["TEXT"].ToString();
                    chk.TITLE = rdr["TITLE"].ToString();
                    chk.RISK = rdr["RISK"].ToString();
                    chk.INSTANCES = rdr["Instances"].ToString();
                    chk.AMOUNT = rdr["amount"].ToString();
                    chk.RISK_ID = rdr["RISK_ID"].ToString();
                    chk.ENG_ID = Convert.ToInt32(rdr["engplanid"]);
                    chk.OBS_REPLY = this.GetLatestAuditeeResponse(OBS_ID);
                    chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(OBS_ID, chk.ENG_ID);
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ManageObservations> GetManagedObservationsForBranches(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedObservationsForBranches";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();

                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"]);
                    chk.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"]);
                    if (rdr["MEMO_NO"].ToString() != null && rdr["MEMO_NO"].ToString() != "")
                        chk.MEMO_NO = Convert.ToInt32(rdr["MEMO_NO"]);

                    chk.PROCESS = rdr["PROCESS"].ToString();
                    chk.NO_OF_INSTANCES = Convert.ToInt32(rdr["NOINSTANCES"]);
                    chk.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    chk.Checklist_Details = rdr["Check_List_Detail"].ToString();
                    chk.HEADING = rdr["HEADINGS"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();
                    chk.ANNEXURE_ID = rdr["ANNEX_ID"].ToString();
                    chk.ANNEXURE_CODE = rdr["ANNEX_CODE"].ToString();
                    chk.PPNO_TEST = loggedInUser.PPNumber;
                    chk.DSA = rdr["DSA"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ManageObservations> GetManagedObservationTextForBranches(int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<ManageObservations> list = new List<ManageObservations>();
            if (loggedInUser.UserEntityID == 112242 || loggedInUser.UserEntityID == 112248)
                {

                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ar.P_GetManagedObservationstext";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        ManageObservations chk = new ManageObservations();
                        chk.INDICATOR = "D";
                        //chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                        chk.PROCESS = rdr["Violation"].ToString();
                        //chk.PROCESS_ID = rdr["PROCESS_ID"].ToString();
                        chk.SUB_PROCESS = rdr["NATURE"].ToString();
                        //  chk.SUB_PROCESS_ID = rdr["SUB_PROCESS_ID"].ToString();
                        chk.HEADING = rdr["OBS_HEADING"].ToString();
                        if (rdr["OBS_RISK_ID"].ToString() != null && rdr["OBS_RISK_ID"].ToString() != "")
                            chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"].ToString());
                        chk.OBS_TEXT = rdr["OBS_TEXT"].ToString();
                        chk.OBS_REPLY = this.GetLatestAuditeeResponse(OBS_ID);
                        chk.ENG_ID = Convert.ToInt32(rdr["engplanid"]);
                        chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(OBS_ID, chk.ENG_ID);
                        chk.ATTACHED_EVIDENCES = this.GetRespondedObservationEvidences(OBS_ID);
                        list.Add(chk);
                        }
                    }
                }
            else
                {
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "pkg_ar.P_GetManagedObservationsForBranchesText";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                    cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                    cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    OracleDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        {
                        ManageObservations chk = new ManageObservations();
                        chk.INDICATOR = "Z";
                        chk.ENG_ID = Convert.ToInt32(rdr["engplanid"]);
                        chk.PROCESS = rdr["PROCESS"].ToString();
                        chk.PROCESS_ID = rdr["PROCESS_ID"].ToString();
                        chk.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                        chk.SUB_PROCESS_ID = rdr["SUB_PROCESS_ID"].ToString();
                        chk.HEADING = rdr["HEADINGS"].ToString();
                        if (rdr["RISK_ID"].ToString() != null && rdr["RISK_ID"].ToString() != "")
                            chk.OBS_RISK_ID = Convert.ToInt32(rdr["RISK_ID"].ToString());
                        chk.ANNEXURE_ID = rdr["ANNEXURE_ID"].ToString();
                        chk.Checklist_Details = rdr["Check_List_Detail"].ToString();
                        chk.Checklist_Details_Id = rdr["Check_List_Detail_Id"].ToString();
                        chk.OBS_TEXT = rdr["OBS_TEXT"].ToString();
                        chk.OBS_REPLY = this.GetLatestAuditeeResponse(OBS_ID);
                        chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(OBS_ID, chk.ENG_ID);
                        list.Add(chk);
                        }
                    }
                }


            con.Dispose();
            return list;
            }

        public List<ManageObservations> GetManagedDraftObservations(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedDraftObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();
                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.ENG_ID = Convert.ToInt32(rdr["ENG_ID"]);
                    chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"]);
                    chk.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"]);
                    if (rdr["MEMO_NO"].ToString() != null && rdr["MEMO_NO"].ToString() != "")
                        chk.MEMO_NO = Convert.ToInt32(rdr["MEMO_NO"].ToString());

                    if (rdr["DRAFT_PARA"].ToString() != null && rdr["DRAFT_PARA"].ToString() != "")
                        chk.DRAFT_PARA_NO = Convert.ToInt32(rdr["DRAFT_PARA"]);

                    chk.VIOLATION = rdr["VIOLATION"].ToString();
                    chk.NATURE = rdr["NATURE"].ToString();
                    chk.AUD_REPLY = this.GetLatestAuditorResponse(chk.OBS_ID);
                    chk.HEAD_REPLY = this.GetLatestDepartmentalHeadResponse(chk.OBS_ID);
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();

                    list.Add(chk);

                    }
                }
            con.Dispose();

            return list;
            }

        public List<ManageObservations> GetManagedDraftObservationsBranch(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedDraftObservationsForBranches";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();
                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"]);
                    chk.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"]);
                    if (rdr["MEMO_NO"].ToString() != null && rdr["MEMO_NO"].ToString() != "")
                        chk.MEMO_NO = Convert.ToInt32(rdr["MEMO_NO"]);

                    if (rdr["DRAFT_PARA"].ToString() != null && rdr["DRAFT_PARA"].ToString() != "")
                        chk.DRAFT_PARA_NO = Convert.ToInt32(rdr["DRAFT_PARA"]);


                    chk.PROCESS = rdr["PROCESS"].ToString();
                    chk.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    chk.Checklist_Details = rdr["CHECK_LIST_DETAIL"].ToString();

                    chk.AUD_REPLY = this.GetLatestAuditorResponse(chk.OBS_ID);
                    chk.HEAD_REPLY = this.GetLatestDepartmentalHeadResponse(chk.OBS_ID);
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.HEADING = rdr["HEADINGS"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();
                    // chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(chk.OBS_ID);
                    list.Add(chk);

                    }
                }
            con.Dispose();

            return list;
            }

        public List<ManageObservations> GetEntityReportParasForBranch(int ENG_ID = 0)
            {
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedDraftObservationsbranch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();
                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"]);
                    chk.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"]);
                    if (rdr["MEMO_NO"].ToString() != null && rdr["MEMO_NO"].ToString() != "")
                        chk.MEMO_NO = Convert.ToInt32(rdr["MEMO_NO"]);
                    if (rdr["DRAFT_PARA"].ToString() != null && rdr["DRAFT_PARA"].ToString() != "")
                        chk.DRAFT_PARA_NO = Convert.ToInt32(rdr["DRAFT_PARA"]);

                    if (rdr["FINAL_PARA"].ToString() != null && rdr["FINAL_PARA"].ToString() != "")
                        chk.FINAL_PARA_NO = Convert.ToInt32(rdr["FINAL_PARA"]);

                    chk.PROCESS = rdr["PROCESS"].ToString();
                    chk.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    chk.Checklist_Details = rdr["CHECK_LIST_DETAIL"].ToString();

                    chk.OBS_TEXT =
                    chk.AUD_REPLY = this.GetLatestAuditorResponse(chk.OBS_ID);
                    chk.HEAD_REPLY = this.GetLatestDepartmentalHeadResponse(chk.OBS_ID);
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();
                    // chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(chk.OBS_ID);
                    list.Add(chk);

                    }
                }
            con.Dispose();

            return list;
            }

        public List<ManageObservations> GetManagedDraftObservationsText(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedDraftObservationsText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();
                    chk.OBS_TEXT = rdr["OBS_TEXT"].ToString();
                    chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(OBS_ID, ENG_ID);
                    list.Add(chk);
                    }
                }
            con.Dispose();

            return list;
            }

        public List<ManageObservations> GetManagedDraftObservationsForBranches(int ENG_ID = 0)
            {
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            List<ManageObservations> finalList = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetManagedDraftObservationsForBranches";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;


                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ManageObservations chk = new ManageObservations();
                    chk.OBS_ID = Convert.ToInt32(rdr["OBS_ID"]);
                    chk.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"]);
                    chk.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"]);
                    if (rdr["MEMO_NO"].ToString() != null && rdr["MEMO_NO"].ToString() != "")
                        chk.MEMO_NO = Convert.ToInt32(rdr["MEMO_NO"]);

                    if (rdr["PROCESS"].ToString() != null && rdr["PROCESS"].ToString() != "")
                        chk.PROCESS = rdr["PROCESS"].ToString();

                    if (rdr["SUBPROCESS"].ToString() != null && rdr["SUBPROCESS"].ToString() != "")
                        chk.SUB_PROCESS = rdr["SUBPROCESS"].ToString();



                    chk.OBS_TEXT = rdr["OBS_TEXT"].ToString();
                    chk.OBS_REPLY = this.GetLatestAuditeeResponse(chk.OBS_ID);
                    chk.AUD_REPLY = this.GetLatestAuditorResponse(chk.OBS_ID);
                    chk.HEAD_REPLY = this.GetLatestDepartmentalHeadResponse(chk.OBS_ID);
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();
                    chk.RESPONSIBLE_PPs = this.GetObservationResponsiblePPNOs(chk.OBS_ID, chk.ENG_ID);
                    list.Add(chk);

                    }
                }
            con.Dispose();

            return list;
            }

        public List<SubCheckListStatus> GetSubChecklistStatus(int ENG_ID = 0, int S_ID = 0)
            {
            List<SubCheckListStatus> list = new List<SubCheckListStatus>();
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_getauditeecheckklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PLANID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("SUBCHECKLISTID", OracleDbType.Int32).Value = S_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SubCheckListStatus chk = new SubCheckListStatus();
                    if (rdr["S_ID"].ToString() != null && rdr["S_ID"].ToString() != "")
                        chk.S_ID = Convert.ToInt32(rdr["S_ID"].ToString());
                    if (rdr["CHECKLIST_ID"].ToString() != null && rdr["CHECKLIST_ID"].ToString() != "")
                        chk.CD_ID = Convert.ToInt32(rdr["CHECKLIST_ID"].ToString());
                    if (rdr["STATUS"].ToString() != null && rdr["STATUS"].ToString() != "")
                        chk.Status = rdr["STATUS"].ToString();
                    if (rdr["OBSID"].ToString() != null && rdr["OBSID"].ToString() != "")
                        chk.OBS_ID = Convert.ToInt32(rdr["OBSID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string DropAuditObservation(int OBS_ID)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_DropAuditObservation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }
                }
            return resp;
            }

        public string SubmitAuditObservationToAuditee(int OBS_ID = 0)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_SubmitAuditObservationToAuditee";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }
                }
            return resp;
            }

        public string UpdateAuditObservationStatus(int OBS_ID, int NEW_STATUS_ID, string DRAFT_PARA_NO, string AUDITOR_COMMENT)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            if (NEW_STATUS_ID == 8 || NEW_STATUS_ID == 9)
                {
                if (loggedInUser.UserRoleID != 5 && loggedInUser.UserRoleID != 6 && loggedInUser.UserRoleID != 7 && loggedInUser.UserRoleID != 15)
                    {
                    resp = "Only Departmental Head is authorized to update this observation status";
                    return resp;

                    }
                }

            string Remarks = "";
            if (NEW_STATUS_ID == 4)
                Remarks = "Settle";
            else if (NEW_STATUS_ID == 5)
                Remarks = "Add to Draft Report";
            else if (NEW_STATUS_ID == 8)
                Remarks = "Add to Final Report";
            else if (NEW_STATUS_ID == 9)
                Remarks = "Para settle in discussion";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_UpdateAuditObservationStatus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("NEW_STATUS_ID", OracleDbType.Int32).Value = NEW_STATUS_ID;
                cmd.Parameters.Add("D_PARA_NO", OracleDbType.Varchar2).Value = DRAFT_PARA_NO;
                cmd.Parameters.Add("Remarks", OracleDbType.Varchar2).Value = Remarks;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }

                if (NEW_STATUS_ID == 4 || NEW_STATUS_ID == 5)
                    {
                    cmd.CommandText = "pkg_ar.AUDITOR_RESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("AUDITOR_COMMENT", OracleDbType.Varchar2).Value = AUDITOR_COMMENT;
                    cmd.Parameters.Add("status", OracleDbType.Int32).Value = NEW_STATUS_ID;
                    cmd.ExecuteReader();

                    }
                else if (NEW_STATUS_ID == 8 || NEW_STATUS_ID == 9)
                    {
                    cmd.CommandText = "pkg_ar.AUDITOR_REPLY";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("AUDITOR_COMMENT", OracleDbType.Varchar2).Value = AUDITOR_COMMENT;
                    cmd.Parameters.Add("status", OracleDbType.Int32).Value = NEW_STATUS_ID;
                    cmd.ExecuteReader();
                    }

                }
            con.Dispose();
            return resp;
            }

        public string UpdateAuditObservationStatusByHead(int OBS_ID, int NEW_STATUS_ID, string FINAL_PARA_NO, string AUDITOR_COMMENT)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string Remarks = "";
            if (NEW_STATUS_ID == 4)
                Remarks = "Settled";
            else if (NEW_STATUS_ID == 5)
                Remarks = "Add to Draft Report";
            else if (NEW_STATUS_ID == 8)
                Remarks = "Add to Final Report";
            else if (NEW_STATUS_ID == 9)
                Remarks = "Para settle in discussion";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_UpdateAuditObservationStatusByHead";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("NEW_STATUS_ID", OracleDbType.Int32).Value = NEW_STATUS_ID;
                cmd.Parameters.Add("F_PARA_NO", OracleDbType.Varchar2).Value = FINAL_PARA_NO;
                cmd.Parameters.Add("Remarks", OracleDbType.Varchar2).Value = Remarks;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }

                if (NEW_STATUS_ID == 4 || NEW_STATUS_ID == 5)
                    {
                    cmd.CommandText = "pkg_ar.AUDITOR_RESPONSE";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("AUDITOR_COMMENT", OracleDbType.Varchar2).Value = AUDITOR_COMMENT;
                    cmd.Parameters.Add("status", OracleDbType.Int32).Value = NEW_STATUS_ID;
                    cmd.ExecuteReader();

                    }
                else if (NEW_STATUS_ID == 8 || NEW_STATUS_ID == 9)
                    {
                    cmd.CommandText = "pkg_ar.AUDITOR_REPLY";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
                    cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                    cmd.Parameters.Add("AUDITOR_COMMENT", OracleDbType.Varchar2).Value = AUDITOR_COMMENT;
                    cmd.Parameters.Add("status", OracleDbType.Int32).Value = NEW_STATUS_ID;
                    cmd.ExecuteReader();
                    }

                }
            con.Dispose();
            return resp;
            }

        public string UpdateAuditObservationText(int OBS_ID, string OBS_TEXT, int PROCESS_ID = 0, int SUBPROCESS_ID = 0, int CHECKLIST_ID = 0, string OBS_TITLE = "", int RISK_ID = 0, int ANNEXURE_ID = 0)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_UpdateObservation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("title", OracleDbType.Clob).Value = OBS_TITLE;
                cmd.Parameters.Add("OBTEXT", OracleDbType.Clob).Value = OBS_TEXT;
                cmd.Parameters.Add("SUBPROCESSID", OracleDbType.Int32).Value = SUBPROCESS_ID;
                cmd.Parameters.Add("CHECKLISTID", OracleDbType.Int32).Value = CHECKLIST_ID;
                cmd.Parameters.Add("RiskID", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("AnnexureID", OracleDbType.Int32).Value = ANNEXURE_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<ClosingDraftTeamDetailsModel> GetClosingDraftObservations(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<ClosingDraftTeamDetailsModel> list = new List<ClosingDraftTeamDetailsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_GetClosingDraftObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ClosingDraftTeamDetailsModel chk = new ClosingDraftTeamDetailsModel();
                    //  chk.ENG_PLAN_ID = Convert.ToInt32(ENG_ID);
                    if (rdr["TOTAL_NO_OB"].ToString() != null && rdr["TOTAL_NO_OB"].ToString() != "")
                        chk.TOTAL_NO_OB = Convert.ToInt32(rdr["TOTAL_NO_OB"]);

                    if (rdr["SUBMITTED_TO_AUDITEE"].ToString() != null && rdr["SUBMITTED_TO_AUDITEE"].ToString() != "")
                        chk.SUBMITTED_TO_AUDITEE = Convert.ToInt32(rdr["SUBMITTED_TO_AUDITEE"]);
                    if (rdr["RESOLVED"].ToString() != null && rdr["RESOLVED"].ToString() != "")
                        chk.RESOLVED = Convert.ToInt32(rdr["RESOLVED"]);
                    if (rdr["RESPONDED"].ToString() != null && rdr["RESPONDED"].ToString() != "")
                        chk.RESPONDED = Convert.ToInt32(rdr["RESPONDED"]);
                    if (rdr["DROPPED"].ToString() != null && rdr["DROPPED"].ToString() != "")
                        chk.DROPPED = Convert.ToInt32(rdr["DROPPED"]);
                    if (rdr["ADDED_TO_DRAFT"].ToString() != null && rdr["ADDED_TO_DRAFT"].ToString() != "")
                        chk.ADDED_TO_DRAFT = Convert.ToInt32(rdr["ADDED_TO_DRAFT"]);


                    chk.TEAM_MEM_PPNO = Convert.ToInt32(rdr["MEMBER_PPNO"]);
                    chk.JOINING_DATE = Convert.ToDateTime(rdr["JOINING_DATE"].ToString()).ToString("dd/MM/yyyy");
                    chk.COMPLETION_DATE = Convert.ToDateTime(rdr["COMPLETION_DATE"].ToString()).ToString("dd/MM/yyyy");
                    chk.ISTEAMLEAD = rdr["TEAMLEAD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.MEMBER_NAME = rdr["MEMBER_NAME"].ToString();
                    chk.ENG_PLAN_ID = Convert.ToInt32(rdr["ENG_PLAN_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ClosingDraftTeamDetailsModel> GetClosingDraftTeamDetails(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ClosingDraftTeamDetailsModel> list = new List<ClosingDraftTeamDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_GetClosingDraftObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ClosingDraftTeamDetailsModel chk = new ClosingDraftTeamDetailsModel();
                    chk.ENG_PLAN_ID = Convert.ToInt32(ENG_ID);
                    if (rdr["SUBMITTED_TO_AUDITEE"].ToString() != null && rdr["SUBMITTED_TO_AUDITEE"].ToString() != "")
                        chk.SUBMITTED_TO_AUDITEE = Convert.ToInt32(rdr["SUBMITTED_TO_AUDITEE"]);
                    if (rdr["RESOLVED"].ToString() != null && rdr["RESOLVED"].ToString() != "")
                        chk.RESOLVED = Convert.ToInt32(rdr["RESOLVED"]);
                    if (rdr["RESPONDED"].ToString() != null && rdr["RESPONDED"].ToString() != "")
                        chk.RESPONDED = Convert.ToInt32(rdr["RESPONDED"]);
                    if (rdr["DROPPED"].ToString() != null && rdr["DROPPED"].ToString() != "")
                        chk.DROPPED = Convert.ToInt32(rdr["DROPPED"]);
                    if (rdr["ADDED_TO_DRAFT"].ToString() != null && rdr["ADDED_TO_DRAFT"].ToString() != "")
                        chk.ADDED_TO_DRAFT = Convert.ToInt32(rdr["ADDED_TO_DRAFT"]);


                    chk.TEAM_MEM_PPNO = Convert.ToInt32(rdr["MEMBER_PPNO"]);
                    chk.JOINING_DATE = Convert.ToDateTime(rdr["JOINING_DATE"].ToString()).ToString("dd/MM/yyyy");
                    chk.COMPLETION_DATE = Convert.ToDateTime(rdr["COMPLETION_DATE"].ToString()).ToString("dd/MM/yyyy");
                    chk.ISTEAMLEAD = rdr["TEAMLEAD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.MEMBER_NAME = rdr["MEMBER_NAME"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<SearchChecklistDetailsModel> SearchChecklistDetails()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<SearchChecklistDetailsModel> list = new List<SearchChecklistDetailsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetAuditChecklistDetails_search";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    SearchChecklistDetailsModel cm = new SearchChecklistDetailsModel();
                    cm.ID = Convert.ToInt32(rdr["ID"].ToString());
                    cm.PROCESS = rdr["P_NAME"].ToString();
                    cm.SUB_PROCESS = rdr["S_NAME"].ToString();
                    cm.PROCESS_DETAIL = rdr["C_NAME"].ToString();
                    cm.RISK = rdr["RISK"].ToString();
                    list.Add(cm);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeOldParasModel> GetLegacyParasEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetEntitiesForLegacyPara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ENTITY_NAME = rdr["NAME"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeOldParasModel> GetLegacyParasEntitiesHO()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetEntitiesForLegacyPara_HO";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ENTITY_NAME = rdr["NAME"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeOldParasModel> GetLegacyParasEntitiesReport(int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetEntitiesForLegacyPara_ho_report";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ENTITY_NAME = rdr["NAME"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetLegacyParasForUpdate(int ENTITY_ID, string PARA_REF = "", int PARA_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())

                {
                cmd.CommandText = "pkg_ar.P_GetLeagacyObservations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("entityId", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("paraRef", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();

                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.TYPE_ID = rdr["TYPE_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    if (PARA_REF != null)
                        {
                        chk.ENT_TYPE = rdr["ENT_TYPE"].ToString();
                        if (rdr["PROCESS"].ToString() != null && rdr["PROCESS"].ToString() != "")
                            chk.PROCESS = Convert.ToInt32(rdr["PROCESS"].ToString());
                        if (rdr["SUB_PROCESS"].ToString() != null && rdr["SUB_PROCESS"].ToString() != "")
                            chk.SUB_PROCESS = Convert.ToInt32(rdr["SUB_PROCESS"].ToString());
                        if (rdr["PROCESS_DETAIL"].ToString() != null && rdr["PROCESS_DETAIL"].ToString() != "")
                            chk.PROCESS_DETAIL = Convert.ToInt32(rdr["PROCESS_DETAIL"].ToString());
                        chk.PARA_TEXT = rdr["PARA_TEXT"].ToString();
                        }

                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ANNEXURE = rdr["ANNEXURE"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = rdr["VOL_I_II"].ToString();

                    if (PARA_ID != 0 && chk.ENT_TYPE.ToLower() != "d")
                        chk.PARA_RESP = this.GetLegacyParaResponsiblePersons(PARA_ID);
                    list.Add(chk);

                    }



                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetLegacyParasForUpdateHO(string entityName, string PARA_REF = "", int PARA_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())

                {
                cmd.CommandText = "pkg_ar.P_GetLeagacyObservations_ho";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("entityname", OracleDbType.Varchar2).Value = entityName;
                cmd.Parameters.Add("paraRef", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();

                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.TYPE_ID = rdr["TYPE_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    if (PARA_REF != null)
                        {
                        chk.ENT_TYPE = rdr["ENT_TYPE"].ToString();
                        chk.PROCESS = Convert.ToInt32(rdr["PROCESS"].ToString());
                        chk.SUB_PROCESS = Convert.ToInt32(rdr["SUB_PROCESS"].ToString());
                        chk.PROCESS_DETAIL = Convert.ToInt32(rdr["PROCESS_DETAIL"].ToString());
                        chk.PARA_TEXT = rdr["PARA_TEXT"].ToString();
                        chk.RISK_ID = rdr["riskid"].ToString();
                        }

                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ANNEXURE = rdr["ANNEXURE"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = rdr["VOL_I_II"].ToString();
                    list.Add(chk);

                    }



                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetLegacyParasForGistUpdate(int ENTITY_ID, string PARA_REF = "", int PARA_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())

                {
                cmd.CommandText = "pkg_ar.P_GetLeagacyObservations_for_gist_update";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("entityId", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("paraRef", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();

                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.TYPE_ID = rdr["TYPE_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();

                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ANNEXURE = rdr["ANNEXURE"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = rdr["VOL_I_II"].ToString();
                    list.Add(chk);

                    }



                }
            con.Dispose();
            return list;
            }

        public string UpdateLegacyParasWithResponsibility(AddLegacyParaModel LEGACY_PARA)
            {
            string resp = "";
            string responseRes = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_update_legacy_Para_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ref_id", OracleDbType.Varchar2).Value = LEGACY_PARA.REF_P;
                cmd.Parameters.Add("obtext", OracleDbType.Clob).Value = LEGACY_PARA.PARA_TEXT;
                cmd.Parameters.Add("process_id", OracleDbType.Int32).Value = LEGACY_PARA.PROCESS_ID;
                cmd.Parameters.Add("subprocessid", OracleDbType.Int32).Value = LEGACY_PARA.SUB_PROCESS_ID;
                cmd.Parameters.Add("checklistid", OracleDbType.Int32).Value = LEGACY_PARA.CHECKLIST_DETAIL_ID;
                cmd.Parameters.Add("pp_no", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("risk_id", OracleDbType.Int32).Value = LEGACY_PARA.RISK_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "2")
                        {
                        resp = rdr["REMARKS"].ToString();
                        return resp;
                        }
                    else
                        {
                        resp = rdr["REMARKS"].ToString();
                        }

                    }
                if (LEGACY_PARA.RESP_PP != null)
                    {
                    if (LEGACY_PARA.RESP_PP.Count > 0)
                        {
                        foreach (ObservationResponsiblePPNOModel respRow in LEGACY_PARA.RESP_PP)
                            {
                            responseRes = "";
                            cmd.CommandText = "pkg_ar.p_add_para_responsibility";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("refid", OracleDbType.Int32).Value = LEGACY_PARA.ID;
                            cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = respRow.PP_NO;
                            cmd.Parameters.Add("AZ_Entity_id", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                            cmd.Parameters.Add("user_ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("lC_no", OracleDbType.Int32).Value = respRow.LOAN_CASE.HasValue ? (object)respRow.LOAN_CASE.Value : DBNull.Value;
                            cmd.Parameters.Add("LC_AMOUNT", OracleDbType.Int32).Value = respRow.LC_AMOUNT.HasValue ? (object)respRow.LC_AMOUNT.Value : DBNull.Value;
                            cmd.Parameters.Add("AC_NO", OracleDbType.Int32).Value = respRow.ACCOUNT_NUMBER;
                            cmd.Parameters.Add("AC_AMOUNT", OracleDbType.Int32).Value = respRow.ACC_AMOUNT;
                            cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = LEGACY_PARA.REF_P;
                            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                            OracleDataReader rdr2 = cmd.ExecuteReader();
                            while (rdr2.Read())
                                {
                                responseRes = rdr2["REMARKS"].ToString();

                                }
                            }

                        }

                    }
                }
            con.Dispose();
            return resp + "<br/>" + responseRes;
            }

        public string UpdateLegacyParaGistParaNo(string PARA_REF, string PARA_NO, string GIST_OF_PARA)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_update_legacy_Para_Gist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Ref_Id", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("GIST", OracleDbType.Varchar2).Value = GIST_OF_PARA;
                cmd.Parameters.Add("PARANO", OracleDbType.Varchar2).Value = PARA_NO;
                cmd.Parameters.Add("pp_no", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("u_entity", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();

                    }

                }
            con.Dispose();
            return resp;
            }

        public string AddResponsibilityToLegacyParas(ObservationResponsiblePPNOModel RESP_PP, string REF_P, int P_ID)
            {
            string responseRes = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_add_para_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refid", OracleDbType.Int32).Value = P_ID;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = RESP_PP.PP_NO;
                cmd.Parameters.Add("AZ_Entity_id", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("user_ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("lC_no", OracleDbType.Int32).Value = RESP_PP.LOAN_CASE.HasValue ? (object)RESP_PP.LOAN_CASE.Value : DBNull.Value;
                cmd.Parameters.Add("LC_AMOUNT", OracleDbType.Int32).Value = RESP_PP.LC_AMOUNT.HasValue ? (object)RESP_PP.LC_AMOUNT.Value : DBNull.Value;
                cmd.Parameters.Add("AC_NO", OracleDbType.Int32).Value = RESP_PP.ACCOUNT_NUMBER;
                cmd.Parameters.Add("AC_AMOUNT", OracleDbType.Int32).Value = RESP_PP.ACC_AMOUNT;
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = REF_P;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                    {
                    responseRes = rdr2["REMARKS"].ToString();

                    }

                }
            con.Dispose();
            return responseRes;
            }

        public string DeleteResponsibilityOfLegacyParas(string REF_P, int P_ID, int PP_NO)
            {
            string responseRes = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_delete_para_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refid", OracleDbType.Int32).Value = P_ID;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = PP_NO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                    {
                    responseRes = rdr2["REMARKS"].ToString();

                    }

                }
            con.Dispose();
            return responseRes;
            }

        public string UpdateLegacyParasWithResponsibilityNoChangesAZ(AddLegacyParaModel LEGACY_PARA)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_no_update_legacy_Para_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ref_id", OracleDbType.Varchar2).Value = LEGACY_PARA.REF_P;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("risk_id", OracleDbType.Int32).Value = LEGACY_PARA.RISK_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    if (rdr["REF"].ToString() != "" && rdr["REF"].ToString() != null && rdr["REF"].ToString() == "2")
                        {
                        resp = rdr["REMARKS"].ToString();
                        return resp;
                        }
                    else
                        {
                        resp = rdr["REMARKS"].ToString();
                        }

                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AuditeeEntitiesModel> GetEntitiesDropDownForManageObservations()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_entities_for_manage_observations";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel chk = new AuditeeEntitiesModel();
                    chk.NAME = rdr["name"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    chk.ENG_ID = Convert.ToInt32(rdr["ENG_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeEntitiesModel> GetEntitiesForManageAuditParas()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_GetManageAuditParasEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel chk = new AuditeeEntitiesModel();
                    chk.NAME = rdr["name"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeEntitiesModel> GetObservationEntitiesForManageObservations()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_GetObservationEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeEntitiesModel chk = new AuditeeEntitiesModel();
                    chk.CODE = Convert.ToInt32(rdr["CODE"].ToString());
                    chk.NAME = rdr["entity_name"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    chk.ENG_ID = Convert.ToInt32(rdr["eng_id"].ToString());
                    chk.TYPE_ID = Convert.ToInt32(rdr["TYPE_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public AuditeeResponseEvidenceModel GetAuditeeEvidenceData(string FILE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var resp = new AuditeeResponseEvidenceModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_AUDITEE_OBSERVATION_RESPONSE_evidences_FileData";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("FILE_ID", OracleDbType.Varchar2).Value = FILE_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = new AuditeeResponseEvidenceModel
                        {
                        IMAGE_NAME = rdr["FILE_NAME"].ToString(),
                        SEQUENCE = Convert.ToInt32(rdr["SEQUENCE"]),
                        LENGTH = Convert.ToInt32(rdr["LENGTH"]),
                        FILE_ID = (rdr["id"].ToString())
                        };

                    // Handle CLOB data
                    var clob = rdr.GetOracleClob(rdr.GetOrdinal("FILE_DATA"));
                    if (clob != null)
                        {
                        resp.IMAGE_DATA = clob.Value; // Get the entire CLOB data as a string
                        }

                    }
                }
            con.Dispose();
            return resp;
            }

        public string CloseDraftAuditReport(int ENG_ID)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_CloseAudit";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["REMARKS"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<ObservationResponsiblePPNOModel> GetLegacyParaResponsiblePersons(int PARA_REF)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationResponsiblePPNOModel> list = new List<ObservationResponsiblePPNOModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.p_get_legacy_para_responsibles";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("paraRef", OracleDbType.Int32).Value = PARA_REF;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationResponsiblePPNOModel rp = new ObservationResponsiblePPNOModel();
                    rp.LOAN_CASE = ReadNullableInt(rdr, "LOANCASE");
                    rp.EMP_NAME = rdr["EMP_NAME"].ToString();
                    rp.LC_AMOUNT = ReadNullableInt(rdr, "LCAMOUNT");
                    rp.ACCOUNT_NUMBER = ReadInt(rdr, "ACCNUMBER");
                    rp.ACC_AMOUNT = ReadInt(rdr, "ACAMOUNT");
                    rp.PP_NO = ReadInt(rdr, "PP_NO");
                    list.Add(rp);
                    }
                }
            con.Dispose();
            return list;
            }

        public UserModel GetEmployeeNameFromPPNO(int PP_NO)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            UserModel um = new UserModel();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = " pkg_ar.P_get_employees_information";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = PP_NO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    um.Name = rdr["emp_name"].ToString();
                    um.PPNumber = rdr["ppno"].ToString();
                    }
                }
            con.Dispose();
            return um;
            }

        public List<AddNewLegacyParaModel> GetUpdatedGistParaOfLegacyParaForAuthorize()
            {
            List<AddNewLegacyParaModel> list = new List<AddNewLegacyParaModel>();
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_legacy_para_to_authorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AddNewLegacyParaModel lpara = new AddNewLegacyParaModel();
                    lpara.PARA_REF = rdr["REF_P"].ToString();
                    lpara.ANNEXURE = rdr["ANNEXURE"].ToString();
                    lpara.PARA_NO = rdr["PARA_NO"].ToString();
                    lpara.GIST_OF_PARA = rdr["GIST_OF_PARAS"].ToString();
                    lpara.OLD_GIST_OF_PARA = rdr["OLD_GIST_OF_PARAS"].ToString();
                    lpara.AUDIT_YEAR = rdr["AUDIT_YEAR"].ToString();
                    lpara.E_CODE = rdr["E_CODE"].ToString();
                    lpara.NATURE = rdr["NATURE"].ToString();
                    lpara.E_NAME = rdr["E_NAME"].ToString();
                    list.Add(lpara);
                    }
                }
            con.Dispose();
            return list;

            }

        public string AuthorizeLegacyParaGistParaNoUpdate(string PARA_REF, string GIST_OF_PARA, string PARA_NO)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_Authorize_Para_Gist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("GIST", OracleDbType.Varchar2).Value = GIST_OF_PARA;
                cmd.Parameters.Add("PARANO", OracleDbType.Varchar2).Value = PARA_NO;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public string SettleLegacyParaHO(int NEW_STATUS, string PARA_REF, string SETTLEMENT_NOTES)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_Settel_Legacy_Para_HO";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Int32).Value = PARA_REF;
                cmd.Parameters.Add("NEW_STATUS ", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("REMARK", OracleDbType.Varchar2).Value = SETTLEMENT_NOTES;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string DeleteLegacyParaHO(string PARA_REF)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_Delete_Legacy_Para_HO";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Int32).Value = PARA_REF;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<LoanCaseFileDetailsModel> GetWorkingPaperLoanCases(string ENGID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseFileDetailsModel> resp = new List<LoanCaseFileDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetLoanCaseFile";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCaseFileDetailsModel eng = new LoanCaseFileDetailsModel();
                    eng.LC_ID = rdr["LC_ID"].ToString();
                    eng.LC_NUMBER = rdr["LC_NUMBER"].ToString();
                    eng.CATEGORY = rdr["CATEGORY"].ToString();
                    eng.OBSERVATION = rdr["OBSERVATION"].ToString();
                    eng.DISB_DATE = rdr["DISB_DATE"].ToString();
                    eng.AMOUNT = rdr["AMOUNT"].ToString();
                    eng.PARA_NO = rdr["PARA_NO"].ToString();
                    resp.Add(eng);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddWorkingPaperLoanCases(string ENGID, string LCNUMBER, string LCAMOUNT, DateTime DISBDATE, string LCAT, string OBS, string PARA_NO)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_AddLoanCaseFile";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("LCNUMBER", OracleDbType.Varchar2).Value = LCNUMBER;
                cmd.Parameters.Add("LCAmount", OracleDbType.Int32).Value = LCAMOUNT;
                cmd.Parameters.Add("DISBDATE", OracleDbType.Date).Value = DISBDATE;
                cmd.Parameters.Add("LC", OracleDbType.Varchar2).Value = LCAT;
                cmd.Parameters.Add("OBS", OracleDbType.Varchar2).Value = OBS;
                cmd.Parameters.Add("PARA_NO", OracleDbType.Varchar2).Value = PARA_NO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<VoucherCheckingDetailsModel> GetWorkingPaperVoucherChecking(string ENGID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<VoucherCheckingDetailsModel> resp = new List<VoucherCheckingDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetVoucherChecking";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    VoucherCheckingDetailsModel eng = new VoucherCheckingDetailsModel();
                    eng.V_ID = rdr["V_ID"].ToString();
                    eng.V_NUMBER = rdr["V_NUMBER"].ToString();
                    eng.OBSERVATION = rdr["OBSERVATION"].ToString();
                    eng.PARA_NO = rdr["PARA_NO"].ToString();
                    resp.Add(eng);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddWorkingVoucherChecking(string ENGID, string VNUMBER, string OBS, string PARA_NO)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_AddVoucherChecking";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("VNUMBER", OracleDbType.Varchar2).Value = VNUMBER;
                cmd.Parameters.Add("OBS", OracleDbType.Varchar2).Value = OBS;
                cmd.Parameters.Add("PARA_NO", OracleDbType.Varchar2).Value = PARA_NO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<AccountOpeningDetailsModel> GetWorkingPaperAccountOpening(string ENGID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AccountOpeningDetailsModel> resp = new List<AccountOpeningDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetAccountOpeningDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AccountOpeningDetailsModel eng = new AccountOpeningDetailsModel();
                    eng.A_ID = rdr["A_ID"].ToString();
                    eng.V_NUMBER = rdr["V_NUMBER"].ToString();
                    eng.A_NATURE = rdr["A_NATURE"].ToString();
                    eng.OBSERVATION = rdr["OBSERVATION"].ToString();
                    eng.PARA_NO = rdr["PARA_NO"].ToString();
                    resp.Add(eng);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddWorkingAccountOpening(string ENGID, string VNUMBER, string ANATURE, string OBS, string PARA_NO)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_AddAccountOpeningDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("VNUMBER", OracleDbType.Varchar2).Value = VNUMBER;
                cmd.Parameters.Add("ANATURE", OracleDbType.Varchar2).Value = ANATURE;
                cmd.Parameters.Add("OBS", OracleDbType.Varchar2).Value = OBS;
                cmd.Parameters.Add("PARA_NO", OracleDbType.Varchar2).Value = PARA_NO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<FixedAssetsDetailsModel> GetWorkingPaperFixedAssets(string ENGID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FixedAssetsDetailsModel> resp = new List<FixedAssetsDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetFixedAssetsDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENGID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FixedAssetsDetailsModel eng = new FixedAssetsDetailsModel();
                    eng.FA_ID = rdr["FA_ID"].ToString();
                    eng.ASSET_NAME = rdr["ASSET_NAME"].ToString();
                    eng.PHYSICAL_EXISTANCE = rdr["PHYSICAL_EXISTANCE"].ToString();
                    eng.LOCATION_AS_PER_FAR = rdr["LOCATION_AS_PER_FAR"].ToString();
                    eng.DIFFERENCE = rdr["DIFFERENCE"].ToString();
                    eng.REMARKS = rdr["REMARKS"].ToString();
                    resp.Add(eng);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddWorkingFixedAssets(string ENGID, string A_NAME, string PHY_EX, string FAR, string DIFF, string REM)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_AddFixedAssetsDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("ANAME", OracleDbType.Varchar2).Value = A_NAME;
                cmd.Parameters.Add("PHYEX", OracleDbType.Varchar2).Value = PHY_EX;
                cmd.Parameters.Add("LFAR", OracleDbType.Varchar2).Value = FAR;
                cmd.Parameters.Add("DIFF", OracleDbType.Varchar2).Value = DIFF;
                cmd.Parameters.Add("REM", OracleDbType.Varchar2).Value = REM;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<CashCountDetailsModel> GetWorkingPaperCashCounter(string ENGID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<CashCountDetailsModel> resp = new List<CashCountDetailsModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GetCashCounterDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENGID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    CashCountDetailsModel eng = new CashCountDetailsModel();
                    eng.ID = rdr["ID"].ToString();
                    eng.DENOMINATION_VAULT = rdr["DENOMINATION_VAULT"].ToString();
                    eng.NO_CURRENCY_NOTES_VAULT = rdr["NO_CURRENCY_NOTES_VAULT"].ToString();
                    eng.TOTAL_AMOUNT_VAULT = rdr["TOTAL_AMOUNT_VAULT"].ToString();
                    eng.DENOMINATION_SAFE_REGISTER = rdr["DENOMINATION_SAFE_REGISTER"].ToString();
                    eng.NO_CURRENCY_NOTES_SAFE_REGISTER = rdr["NO_CURRENCY_NOTES_SAFE_REGISTER"].ToString();
                    eng.TOTAL_AMOUNT_SAFE_REGISTER = rdr["TOTAL_AMOUNT_SAFE_REGISTER"].ToString();
                    eng.DIFFERENCE = rdr["DIFFERENCE"].ToString();
                    resp.Add(eng);
                    }
                }
            con.Dispose();
            return resp;

            }

        public string AddWorkingCashCounter(string ENGID, string DVAULT, string NOVAULT, string TOTVAULT, string DSR, string NOSR, string TOTSR, string DIFF)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_AddCashCounterDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ENGID", OracleDbType.Varchar2).Value = ENGID;
                cmd.Parameters.Add("DVAL", OracleDbType.Varchar2).Value = DVAULT;
                cmd.Parameters.Add("CVAL", OracleDbType.Varchar2).Value = NOVAULT;
                cmd.Parameters.Add("AVAL", OracleDbType.Varchar2).Value = TOTVAULT;
                cmd.Parameters.Add("DSR", OracleDbType.Varchar2).Value = DSR;
                cmd.Parameters.Add("DSR", OracleDbType.Varchar2).Value = NOSR;
                cmd.Parameters.Add("ASR", OracleDbType.Varchar2).Value = TOTSR;
                cmd.Parameters.Add("DIFF", OracleDbType.Varchar2).Value = DIFF;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<DraftDSAGuidelines> GetDraftDSAGuidelines()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<DraftDSAGuidelines> resp = new List<DraftDSAGuidelines>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_get_dsa_guidline";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    DraftDSAGuidelines d = new DraftDSAGuidelines();
                    d.ID = Convert.ToInt32(rdr["ID"].ToString());
                    d.PARTICULARS = rdr["PATRICULARS"].ToString();
                    d.STATUS = rdr["STATUS"].ToString();
                    resp.Add(d);
                    }
                }
            con.Dispose();
            return resp;

            }

        public List<Object> DraftDSA(int OBS_ID, string PPNO, string DSA_CONTENT)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            int DSA_ID = 0;
            List<Object> outRec = new List<object>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_draft_dsa";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obs", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("content", OracleDbType.Clob).Value = DSA_CONTENT;
                cmd.Parameters.Add("RES_P_NO", OracleDbType.Int32).Value = PPNO;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    DSA_ID = Convert.ToInt32(rdr["DSA_ID"].ToString());
                    outRec.Add(resp);
                    outRec.Add(DSA_ID);
                    }
                }
            con.Dispose();
            return outRec;

            }

        public void AddDraftDSAGuideline(string D_ID, string G_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_add_dsa_checkilist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("d_id", OracleDbType.Int32).Value = D_ID;
                cmd.Parameters.Add("check_list", OracleDbType.Int32).Value = G_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.ExecuteReader();

                }
            con.Dispose();

            }

        public List<ObservationResponsiblePPNOModel> GetResponsiblePersonsList(int PARA_ID, string INDICATOR)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ObservationResponsiblePPNOModel> list = new List<ObservationResponsiblePPNOModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_Get_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Para_ID", OracleDbType.Int32).Value = PARA_ID;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationResponsiblePPNOModel usr = new ObservationResponsiblePPNOModel();
                    usr.EMP_NAME = rdr["emp_name"].ToString();
                    usr.PP_NO = ReadInt(rdr, "PP_NO");
                    usr.LOAN_CASE = ReadNullableInt(rdr, "loan_case");
                    usr.LC_AMOUNT = ReadNullableInt(rdr, "lc_amount");
                    usr.ACCOUNT_NUMBER = ReadInt(rdr, "account_number");
                    usr.ACC_AMOUNT = ReadInt(rdr, "ac_amount");
                    usr.REMARKS = rdr["remarks"].ToString();
                    usr.INDICATOR = rdr["r_ind"].ToString();
                    list.Add(usr);
                    }
                }
            con.Dispose();
            return list;
            }

        private string AddInitialResponsibilityAssignment(int paraId, int eng_id, int com_id, ObservationResponsiblePPNOModel responsible, string INDICATOR)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
           
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_responibilityassigned";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("N_ID", OracleDbType.Int32).Value = paraId;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = eng_id;
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = com_id;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("RES_PP", OracleDbType.Int32).Value = responsible.PP_NO;
                cmd.Parameters.Add("LOANCASE", OracleDbType.Int32).Value = responsible.LOAN_CASE.HasValue ? (object)responsible.LOAN_CASE.Value : DBNull.Value;
                cmd.Parameters.Add("ACCNUMBER", OracleDbType.Int32).Value = responsible.ACCOUNT_NUMBER;
                cmd.Parameters.Add("LCAMOUNT", OracleDbType.Int32).Value = responsible.LC_AMOUNT.HasValue ? (object)responsible.LC_AMOUNT.Value : DBNull.Value;
                cmd.Parameters.Add("ACAMOUNT", OracleDbType.Int32).Value = responsible.ACC_AMOUNT;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            con.Dispose();
            return resp;
            }

        private string UpdateResponsibilityAssignment(int newParaId, int eng_id, int com_id, string indicator, ObservationResponsiblePPNOModel responsible)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
           
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_Update_responsibility";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = indicator;
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = com_id;
                cmd.Parameters.Add("N_PARA_ID", OracleDbType.Int32).Value = newParaId;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = eng_id;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = responsible.PP_NO;
                cmd.Parameters.Add("L_CASE", OracleDbType.Int32).Value = responsible.LOAN_CASE.HasValue ? (object)responsible.LOAN_CASE.Value : DBNull.Value;
                cmd.Parameters.Add("LC_AMOUNT", OracleDbType.Int32).Value = responsible.LC_AMOUNT.HasValue ? (object)responsible.LC_AMOUNT.Value : DBNull.Value;
                cmd.Parameters.Add("AC_AMOUNT", OracleDbType.Int32).Value = responsible.ACC_AMOUNT;
                cmd.Parameters.Add("NO_ACCOUNT", OracleDbType.Int32).Value = responsible.ACCOUNT_NUMBER;
                cmd.Parameters.Add("Remarks", OracleDbType.Varchar2).Value = responsible.REMARKS;
                cmd.Parameters.Add("U_D_action", OracleDbType.Varchar2).Value = responsible.ACTION;
                cmd.Parameters.Add("E_NAME", OracleDbType.Varchar2).Value = responsible.EMP_NAME;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            con.Dispose();
            return resp;
            }

        public string SubmitDSAToAuditee(int ENTITY_ID, int OBS_ID, int ENG_ID, int RESP_PPNO, int RESP_ROW_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_draft_dsa";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("RESP_PPNO", OracleDbType.Int32).Value = RESP_PPNO;
                cmd.Parameters.Add("RESP_ROW_ID", OracleDbType.Int32).Value = RESP_ROW_ID;
                cmd.Parameters.Add("ENGAGEMENT_ID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<DraftDSAList> GetDraftDSAList()
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<DraftDSAList> resp = new List<DraftDSAList>();

            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_get_dsa_list";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    var dm = new DraftDSAList
                        {
                        ID = rdr["ID"].ToString(),
                        DSA_NO = rdr["D_NO"].ToString(),
                        AUDIT_PERIOD = rdr["A_PERIOD"].ToString(),
                        REPORTING_OFFICE = rdr["REPO_OFFICE"].ToString(),
                        ENTITY_ID = rdr["entity_id"].ToString(),
                        ENTITY_NAME = rdr["ENTITY_NAME"].ToString(),
                        AZ_NAME = rdr["AZ_NAME"].ToString(),
                        HEADING = rdr["HEADING"].ToString(),
                        ENG_ID = rdr["ENG_ID"].ToString(),
                        OBS_ID = rdr["OBS_ID"].ToString(),
                        ROW_RESP_ID = rdr["ROW_RESP_ID"].ToString(),
                        RESP_PP_NO = rdr["pp_no"].ToString(),
                        EMP_NAME = rdr["EMP_NAME"].ToString(),
                        LOAN_CASE = rdr["LOANCASE"].ToString(),
                        LC_AMOUNT = rdr["LCAMOUNT"].ToString(),
                        AC_NUMBER = rdr["ACCNUMBER"].ToString(),
                        AC_AMOUNT = rdr["ACAMOUNT"].ToString(),
                        CREATED_BY_TEAM = rdr["TeamName"].ToString(),
                        DSA_STATUS = rdr["DSA_STATUS"].ToString(),
                        STATUS_UP = rdr["STATUS_UP"].ToString(),
                        STATUS_DOWN = rdr["STATUS_DOWN"].ToString(),
                        };
                    resp.Add(dm);
                    }
                }
            con.Dispose();
            return resp;
            }

        public DSAContentModel GetDraftDSAContent(int DSA_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            DSAContentModel resp = new DSAContentModel();

            using (OracleCommand cmd = con.CreateCommand())
                {

                cmd.CommandText = "pkg_ar.P_get_dsa_content";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = new DSAContentModel
                        {
                        ID = rdr["ID"].ToString(),
                        NO = rdr["D_NO"].ToString(),
                        TEXT = rdr["TEXT"].ToString(),
                        HEADING = rdr["HEADING"].ToString(),
                        };

                    }
                }
            con.Dispose();
            return resp;
            }

        public string SubmitDSAToHeadFAD(int DSA_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_submit_dsa_to_head_fad";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string UpdateDSAHeading(int DSA_ID, string DSA_HEADING)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_update_dsa_heading";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("d_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("U_HEADING", OracleDbType.Varchar2).Value = DSA_HEADING;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string ReferredBackDSAByHeadFad(int DSA_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_reffered_back_dsa_by_head_fad";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string SubmitDSAToDPD(int DSA_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_submit_dsa_by_head_fad_to_dpd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string ReferredBackDSAByDPD(int DSA_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_reffered_back_dsa_by_dpd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string AcknowledgeDSA(int DSA_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_submit_dsa_by_dpd_to_committee";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = DSA_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<LoanCaseDetailModel> GetLoanCaseDetailsWithBRCode(int LC, int BR_CODE)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<LoanCaseDetailModel> list = new List<LoanCaseDetailModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ar.P_GET_LC_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("LC_NO", OracleDbType.Int32).Value = LC;
                cmd.Parameters.Add("B_CODE", OracleDbType.Int32).Value = BR_CODE;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    LoanCaseDetailModel lcm = new LoanCaseDetailModel
                        {
                        Name = rdr["NAME"]?.ToString(),
                        Cnic = rdr["CNIC"]?.ToString(),
                        LoanCaseNo = rdr["LOAN_CASE_NO"]?.ToString(),
                        DisbDate = rdr["DISB_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["DISB_DATE"]),
                        AppDate = rdr["APP_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["APP_DATE"]),
                        CadReceiveDate = rdr["CAD_RECEIVE_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CAD_RECEIVE_DATE"]),
                        SanctionDate = rdr["SANCTION_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["SANCTION_DATE"]),
                        DisbursedAmount = rdr["DISBURSED_AMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["DISBURSED_AMOUNT"]),
                        OutstandingAmount = rdr["OUTSTANDING_AMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(rdr["OUTSTANDING_AMOUNT"]),
                        McoPPNo = rdr["mco_ppno"]?.ToString(),
                        McoName = rdr["mco_name"]?.ToString(),
                        ManagerPPNo = rdr["manager_ppno"]?.ToString(),
                        ManagerName = rdr["manager_name"]?.ToString(),
                        RgmPPNo = rdr["rgm_ppno"]?.ToString(),
                        RgmName = rdr["rgm_name"]?.ToString(),
                        CadReviewerPPNo = rdr["cad_reviewer"]?.ToString(),
                        CadReviewerName = rdr["cad_name"]?.ToString(),
                        CadAuthorizerPPNo = rdr["cad_authorizer"]?.ToString(),
                        CadAuthorizerName = rdr["cad_authorizer_name"]?.ToString(),
                        };

                    list.Add(lcm);


                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditChecklistAnnexureCircularModel> Get_Circulars(string text)
            {
            var result = new List<AuditChecklistAnnexureCircularModel>();
            using (var con = this.DatabaseConnection())
                {
               

                using (var cmd = new OracleCommand("pkg_AR.P_GET_ENB_CIRCULARS", con))
                    {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_text", OracleDbType.Varchar2, 200).Value = text ?? (object)DBNull.Value;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            result.Add(new AuditChecklistAnnexureCircularModel
                                {
                                ID = reader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID"]),
                                DivisionEntId = reader["DIVISION_ENT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DIVISION_ENT_ID"]),
                                ReferenceTypeId = reader["REFERENCE_TYPE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["REFERENCE_TYPE_ID"]),
                                ReferenceType = reader["REFERENCE_TYPE"]?.ToString(),
                                InstructionsDetails = reader["INSTRUCTIONSDETAILS"]?.ToString(),
                                Keywords = reader["KEYWORDS"]?.ToString(),
                                RedirectedPage = reader["REDIRECTEDPAGE"]?.ToString(),
                                Division = reader["DIVISION"]?.ToString(),
                                InstructionsTitle = reader["INSTRUCTIONSTITLE"]?.ToString(),
                                InstructionsDate = reader["INSTRUCTIONSDATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["INSTRUCTIONSDATE"]),
                                DocType = reader["DOCTYPE"]?.ToString()
                                });
                            }
                        }
                    }
                }
            return result;
            }
        }
    }

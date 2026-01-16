using AIS.Models;
using AIS.Models.AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public string GetUserName(string PPNUMBER)
            {
            string userName = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.p_ppno_name";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = PPNUMBER;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    userName = rdr["EMPLOYEE_NAME"].ToString();
                    }
                }
            con.Dispose();
            return userName;

            }

        public List<BranchModel> GetZoneBranches(int zone_code = 0, bool sessionCheck = true)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            var con = this.DatabaseConnection();
            List<BranchModel> branchList = new List<BranchModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasEntityid";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Entityid", OracleDbType.Int32).Value = zone_code;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BranchModel br = new BranchModel();
                    br.BRANCHID = Convert.ToInt32(rdr["branchentityid"]);
                    br.BRANCHNAME = rdr["branchname"].ToString();
                    branchList.Add(br);
                    }
                }
            con.Dispose();
            return branchList;
            }

        public List<PreConcludingModel> GetEntityObservationDetails(int ENG_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<PreConcludingModel> list = new List<PreConcludingModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_get_audit_pre_Concluding";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("engid", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    PreConcludingModel chk = new PreConcludingModel();
                    chk.OBS_ID = rdr["id"].ToString();
                    chk.OBS_STATUS = rdr["ob_status"].ToString();
                    chk.FINAL_PARA_NO = rdr["final_para_no"].ToString();
                    chk.HEADING = rdr["headings"].ToString();
                    chk.STATUS = rdr["STATUS"].ToString();
                    chk.OBS_RISK = rdr["severity"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<ObservationResponsiblePPNOModel> GetOldParasObservationResponsiblePPNOsUpdatedByImp(int PARA_ID, string PARA_CATEGORY, int AU_OBS_ID)
            {
            var con = this.DatabaseConnection();
            List<ObservationResponsiblePPNOModel> list = new List<ObservationResponsiblePPNOModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforfinalsettlement_responsibles";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("paraRef", OracleDbType.Varchar2).Value = PARA_ID;
                cmd.Parameters.Add("OBSID", OracleDbType.Varchar2).Value = AU_OBS_ID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ObservationResponsiblePPNOModel usr = new ObservationResponsiblePPNOModel();
                    usr.EMP_NAME = rdr["EMP_NAME"].ToString();
                    usr.PP_NO = rdr["PP_NO"].ToString();
                    usr.LOAN_CASE = rdr["LOANCASE"].ToString();
                    usr.LC_AMOUNT = rdr["LCAMOUNT"].ToString();
                    usr.ACCOUNT_NUMBER = rdr["ACCNUMBER"].ToString();
                    usr.ACC_AMOUNT = rdr["ACAMOUNT"].ToString();
                    list.Add(usr);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<ManageObservations> GetFinalizedDraftObservations(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetFinalizedDraftObservations";
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

                    if (rdr["FINAL_PARA"].ToString() != null && rdr["FINAL_PARA"].ToString() != "")
                        chk.FINAL_PARA_NO = Convert.ToInt32(rdr["FINAL_PARA"]);

                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.HEADING = rdr["TITLE"].ToString();
                    chk.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    chk.OBS_RISK = rdr["OBS_RISK"].ToString();
                    chk.PERIOD = rdr["PERIOD"].ToString();
                    list.Add(chk);

                    }
                }
            con.Dispose();

            return list;
            }

        public List<ManageObservations> GetFinalizedDraftObservationsBranch(int ENG_ID = 0, int OBS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            List<ManageObservations> list = new List<ManageObservations>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetFinalizedDraftObservationsbranch";
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
                    if (rdr["FINAL_PARA"].ToString() != null && rdr["FINAL_PARA"].ToString() != "")
                        chk.FINAL_PARA_NO = Convert.ToInt32(rdr["FINAL_PARA"]);

                    chk.PROCESS = rdr["PROCESS"].ToString();
                    chk.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    chk.Checklist_Details = rdr["CHECK_LIST_DETAIL"].ToString();
                    chk.HEADING = rdr["HEADINGS"].ToString();

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

        public List<OldParasModel> GetOldParas(string AUDITED_BY, string AUDIT_YEAR)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParas";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = AUDITED_BY;
                cmd.Parameters.Add("AUDITPERIOD", OracleDbType.Int32).Value = AUDIT_YEAR;
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
                    chk.AUDITED_BY = rdr["AUDITED_BY"].ToString();
                    chk.AUDITEDBY = rdr["AUDITEDBY"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetOldSettledParasForResponse(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasForResponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityId", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.NEW_PARA_ID = rdr["AU_OBS_ID"].ToString();
                    chk.OLD_PARA_ID = rdr["REF_P"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = "";
                    chk.IND = rdr["IND"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetCurrentParasForStatusChangeRequest(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetnewParasForResponse";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityId", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_para"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.VOL_I_II = "";
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetCurrentParasForStatusChangeRequestReview()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetnewParasForResponse_reviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityId", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    OldParasModel chk = new OldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ID"]);
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_CODE = rdr["ENTITY_CODE"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_para"].ToString();
                    chk.AMOUNT_INVOLVED = rdr["AMOUNT_INVOLVED"].ToString();
                    chk.MAKER_REMARKS = rdr["remarks"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString() == "6" ? "Settled" : "Un-settled";
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<OldParasModel> GetManageLegacyParas()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<OldParasModel> list = new List<OldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParas";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
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
                    chk.AUDITED_BY = rdr["AUDITED_BY"].ToString();
                    chk.AUDITEDBY = rdr["AUDITEDBY"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditeeOldParasModel> GetCurrentParasEntitiesForStatusChange()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetEntitiesForNewPara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    chk.ENG_ID = rdr["ENG_ID"].ToString();
                    chk.ENTITY_NAME = rdr["NAME"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public bool UpdateOldParasStatus(int ID, int NEW_STATUS)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            bool success = false;
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_UpdateOldParasFadsettleunsettle";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("PID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add(" NEW_STATUS ", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.ExecuteReader();
                success = true;
                }
            con.Dispose();
            return success;
            }

        public List<AuditeeEntitiesModel> GetObservationEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_get_audit_pre_Concluding_entities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("userentityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
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

        public List<AuditeeEntitiesModel> GetObservationEntitiesForPreConcluding(int pageId = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_get_audit_pre_Concluding_entities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("userentityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                //cmd.Parameters.Add("PAGE_ID", OracleDbType.Int32).Value = pageId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditeeEntitiesModel chk = new AuditeeEntitiesModel();
                    chk.NAME = rdr["ENTITY_NAME"].ToString();
                    chk.ENG_ID = Convert.ToInt32(rdr["ENG_ID"].ToString());
                    //  chk.TYPE_ID = Convert.ToInt32(rdr["TYPE_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<SettledPostCompliancesModel> GetSettledPostCompliancesForMonitoring(string MONTH_NAME, string YEAR)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<SettledPostCompliancesModel> list = new List<SettledPostCompliancesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetSettledParasForReview";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("MON", OracleDbType.Varchar2).Value = MONTH_NAME;
                cmd.Parameters.Add("YR", OracleDbType.Varchar2).Value = YEAR;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    SettledPostCompliancesModel chk = new SettledPostCompliancesModel();
                    chk.COM_ID = rdr["COM_ID"].ToString();
                    chk.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    chk.ENTITY_NAME = rdr["NAME"].ToString();
                    chk.COM_KEY = rdr["COM_KEY"].ToString();
                    chk.NEW_PARA_ID = rdr["NEW_PARAID"].ToString() == "" ? 0 : Convert.ToInt32(rdr["NEW_PARAID"].ToString());
                    chk.OLD_PARA_ID = rdr["old_para_id"].ToString() == "" ? 0 : Convert.ToInt32(rdr["old_para_id"].ToString());

                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    chk.INDICATOR = rdr["ind"].ToString();
                    chk.PARA_RISK = rdr["rsk"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    chk.PARA_NO = rdr["para_no"].ToString();
                    chk.SETTLED_ON = rdr["STELLED_ON"].ToString();
                    chk.COM_STAGE = rdr["COM_STAGE"].ToString();
                    chk.COM_STATUS = rdr["COM_STATUS"].ToString();
                    chk.COM_CYCLE = rdr["COM_CYCLE"].ToString();

                    chk.COMPLIANCE_UNIT = rdr["COM_UNIT"].ToString();
                    chk.COMPLIANCE_SETTLEMENT_OFFICER = rdr["SETTLED_BY"].ToString();
                    chk.COMPLIANCE_UNIT_INCHARGE = rdr["REVIEWED_BY"].ToString();

                    chk.NEXT_R_ID = "";
                    chk.PREV_R_ID = "";
                    chk.PREV_ROLE = "";
                    chk.NEXT_ROLE = "";


                    list.Add(chk);

                    }
                }
            con.Dispose();
            return list;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasBranchComplianceTextForImpIncharge(int PID, string Ref_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforsettlementext";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Entityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("pid", OracleDbType.Int32).Value = PID;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("refP", OracleDbType.Varchar2).Value = Ref_P;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    chk.PARA_TEXT = rdr["para_text"].ToString();
                    chk.GIST_OF_PARA = rdr["gist_of_paras"].ToString();
                    //chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(Ref_P, chk.PARA_CATEGORY);
                    //chk.EVIDENCES = this.GetOldParasEvidences(Ref_P, chk.PARA_CATEGORY, REPLY_DATE, OBS_ID);
                    }
                }
            con.Dispose();
            return chk;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasReferredBackBranchComplianceTextForImpIncharge(int PID, string Ref_P, string PARA_CATEGORY, string REPLY_DATE, string OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforsettlementext_referedack";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Entityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("pid", OracleDbType.Int32).Value = PID;
                cmd.Parameters.Add("refP", OracleDbType.Varchar2).Value = Ref_P;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {

                    chk.PARA_TEXT = rdr["para_text"].ToString();
                    chk.GIST_OF_PARA = rdr["gist_of_paras"].ToString();
                    //chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(Ref_P, chk.PARA_CATEGORY);
                    //chk.EVIDENCES = this.GetOldParasEvidences(Ref_P, chk.PARA_CATEGORY, REPLY_DATE, OBS_ID);
                    }
                }
            con.Dispose();
            return chk;
            }

        public GetOldParasBranchComplianceTextModel GetOldParasBranchComplianceTextForHeadAZ(int PID, string Ref_P, string OBS_ID, string PARA_CATEGORY, string REPLY_DATE)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            GetOldParasBranchComplianceTextModel chk = new GetOldParasBranchComplianceTextModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforfinalsettlementext";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Entityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("refP", OracleDbType.Varchar2).Value = Ref_P;
                cmd.Parameters.Add("PID", OracleDbType.Int32).Value = PID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    chk.PARA_TEXT = rdr["para_text"].ToString();
                    chk.GIST_OF_PARA = rdr["gist_of_paras"].ToString();
                    //chk.RESPONSIBLE_PPs = this.GetOldParasObservationResponsiblePPNOs(Ref_P, chk.PARA_CATEGORY);
                    //chk.UPDATED_RESPONSIBLE_PPs_BY_IMP = this.GetOldParasObservationResponsiblePPNOsUpdatedByImp(PID, chk.PARA_CATEGORY, 0);
                    //chk.EVIDENCES = this.GetOldParasEvidences(Ref_P, chk.PARA_CATEGORY, REPLY_DATE, OBS_ID);
                    }
                }
            con.Dispose();
            return chk;
            }

        public List<GetOldParasforComplianceSettlement> GetComplianceForImpZone()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasforComplianceSettlement> list = new List<GetOldParasforComplianceSettlement>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforsettlement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasforComplianceSettlement chk = new GetOldParasforComplianceSettlement();
                    chk.ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.REF_P = rdr["ref_p"].ToString();
                    chk.AU_OBS_ID = rdr["AU_OBS_ID"].ToString();
                    chk.REPORTINGOFFICE = rdr["Reportingoffice"].ToString();
                    chk.AUDITEENAME = rdr["auditeename"].ToString();
                    chk.AUDITPERIOD = rdr["audit_period"].ToString();
                    chk.PARANO = rdr["para_no"].ToString();
                    chk.GISTOFPARA = rdr["headings"].ToString();
                    chk.REPLIEDDATE = rdr["replieddate"].ToString();
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.RISK = rdr["RISK"].ToString();
                    chk.REMARKS = rdr["remarks"].ToString();
                    chk.SEQUENCE = rdr["SEQUENCE"].ToString();
                    chk.AUDITED_BY = rdr["auditedby"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<GetOldParasforComplianceSettlement> GetReferredBackParasComplianceForImpZone()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasforComplianceSettlement> list = new List<GetOldParasforComplianceSettlement>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforsettlement_ref";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasforComplianceSettlement chk = new GetOldParasforComplianceSettlement();
                    chk.ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.REF_P = rdr["ref_p"].ToString();
                    chk.AU_OBS_ID = rdr["AU_OBS_ID"].ToString();
                    chk.REPORTINGOFFICE = rdr["Reportingoffice"].ToString();
                    chk.AUDITEENAME = rdr["auditeename"].ToString();
                    chk.AUDITPERIOD = rdr["audit_period"].ToString();
                    chk.PARANO = rdr["para_no"].ToString();
                    chk.GISTOFPARA = rdr["headings"].ToString();
                    chk.REPLIEDDATE = rdr["replieddate"].ToString();
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.RISK = rdr["risk"].ToString();
                    chk.HEAD_REF_REMARKS = rdr["remarks"].ToString();
                    chk.SEQUENCE = rdr["SEQUENCE"].ToString();
                    chk.AUDITED_BY = rdr["auditedby"].ToString();

                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<GetOldParasforComplianceSettlement> GetOldParasBranchComplianceSubmission()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasforComplianceSettlement> list = new List<GetOldParasforComplianceSettlement>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforsettlement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasforComplianceSettlement chk = new GetOldParasforComplianceSettlement();
                    chk.ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.REF_P = rdr["ref_p"].ToString();
                    chk.REPORTINGOFFICE = rdr["Reportingoffice"].ToString();
                    chk.AUDITEENAME = rdr["auditeename"].ToString();
                    chk.AUDITPERIOD = rdr["audit_period"].ToString();
                    chk.PARANO = rdr["para_no"].ToString();
                    chk.GISTOFPARA = rdr["gistofpara"].ToString();
                    chk.AMOUNT = rdr["amount_involved"].ToString();
                    chk.REPLY = rdr["reply"].ToString();
                    chk.REMARKS = rdr["remarks"].ToString();
                    chk.REVIEWER_REMARKS = rdr["REVIEWER_REMARKS"].ToString();

                    chk.SUBMITTED = rdr["submitted"].ToString();
                    chk.C_STATUS = rdr["c_status"].ToString();
                    chk.VOL_I_II = rdr["c_status"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<GetOldParasForFinalSettlement> GetOldParasForFinalSettlement()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<GetOldParasForFinalSettlement> list = new List<GetOldParasForFinalSettlement>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParasforfinalsettlement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    GetOldParasForFinalSettlement chk = new GetOldParasForFinalSettlement();

                    chk.REF_P = rdr["REF_P"].ToString();
                    chk.AUDITEENAME = rdr["AUDITEENAME"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GISTOFPARA = rdr["GISTOFPARA"].ToString();
                    chk.RISK = rdr["RISK"].ToString();
                    chk.IMP_REMARKS = rdr["REMARKS"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.REPLIEDDATE = rdr["REPLIEDDATE"].ToString();
                    chk.PARA_CATEGORY = rdr["PARA_CATEGORY"].ToString();
                    chk.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"].ToString());
                    chk.ID = Convert.ToInt32(rdr["ID"].ToString());
                    chk.AU_OBS_ID = rdr["AU_OBS_ID"].ToString();
                    chk.SEQUENCE = rdr["SEQUENCE"].ToString();
                    chk.AUDITED_BY = rdr["AUDITEDBY"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddOldParasStatusUpdate(string OBS_ID, string REFID, string REMARKS, int NEW_STATUS, string PARA_CATEGORY, string SETTLE_INDICATOR, string SEQUENCE, string AUDITED_BY)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_AddOldParasImpRemarks";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("REFID", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("REMARK", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = SETTLE_INDICATOR;
                cmd.Parameters.Add("R_STATUS", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("SEQ_ID", OracleDbType.Varchar2).Value = SEQUENCE;
                cmd.Parameters.Add("AUDIT_ID", OracleDbType.Varchar2).Value = AUDITED_BY;
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

        public string AddOldParasStatusPartiallySettle(string OBS_ID, string REFID, string REMARKS, int NEW_STATUS, string PARA_CATEGORY, string SETTLE_INDICATOR, List<ObservationResponsiblePPNOModel> RESPONSIBLES_ARR, string SEQUENCE, string AUDITED_BY, string PARA_TEXT)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_AddOldParasImpRemarks_partial_comp";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("O_B", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("REFID", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("REMARK", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("PARA_T", OracleDbType.Clob).Value = PARA_TEXT;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = "P";
                cmd.Parameters.Add("R_STATUS", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("SEQ_ID", OracleDbType.Varchar2).Value = SEQUENCE;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }

                if (RESPONSIBLES_ARR != null)
                    {
                    if (RESPONSIBLES_ARR.Count > 0)
                        {
                        foreach (ObservationResponsiblePPNOModel pp in RESPONSIBLES_ARR)
                            {
                            cmd.CommandText = "pkg_hd.p_add_para_responsibility_partial_comp";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                            cmd.Parameters.Add("USER_PPNO", OracleDbType.Int32).Value = pp.PP_NO;
                            cmd.Parameters.Add("LC_NO", OracleDbType.Varchar2).Value = pp.LOAN_CASE;
                            cmd.Parameters.Add("LC_AMOUNT", OracleDbType.Varchar2).Value = pp.ACCOUNT_NUMBER;
                            cmd.Parameters.Add("AC_NO", OracleDbType.Varchar2).Value = pp.LC_AMOUNT;
                            cmd.Parameters.Add("AC_AMOUNT", OracleDbType.Varchar2).Value = pp.ACC_AMOUNT;
                            cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = REFID;
                            cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                            cmd.Parameters.Add("A_C", OracleDbType.Varchar2).Value = pp.RESP_ACTIVE;
                            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                            OracleDataReader rdr2 = cmd.ExecuteReader();
                            while (rdr.Read())
                                {
                                resp = resp + "<br/>" + rdr2["remarks"].ToString();
                                }
                            }
                        }

                    }
                }
            con.Dispose();
            return resp;
            }

        public string AddOldParasheadStatusUpdate(int PARA_ID, string REMARKS, int NEW_STATUS, string PARA_REF, string PARA_INDICATOR, string PARA_CATEGORY, int AU_OBS_ID, string SEQUENCE, string AUDITED_BY, string ENTITY_ID)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_AddFinalsettlement";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_C", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = AU_OBS_ID;
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = PARA_REF;
                cmd.Parameters.Add("REMARK", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = PARA_INDICATOR;
                cmd.Parameters.Add("R_STATUS", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("SEQ_ID", OracleDbType.Varchar2).Value = SEQUENCE;
                cmd.Parameters.Add("AUDIT_ID", OracleDbType.Varchar2).Value = AUDITED_BY;
                cmd.Parameters.Add("AUDITEE_ID", OracleDbType.Varchar2).Value = ENTITY_ID;
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

        public List<AuditeeOldParasModel> GetOldParasForMonitoring(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasModel> list = new List<AuditeeOldParasModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_ALL_PARAS_MONITORING";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("EntityID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasModel chk = new AuditeeOldParasModel();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.PARA_CATEGORY = rdr["IND"].ToString();
                    chk.MEMO_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["GIST_OF_PARAS"].ToString();
                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    chk.PARA_ID = rdr["OLD_PARA_ID"].ToString();
                    chk.OBS_ID = rdr["OBS_ID"].ToString();
                    chk.PARA_RISK = rdr["PARA_RISK"].ToString();
                    chk.COM_ID = Convert.ToInt32(rdr["COM_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string GetParaText(string ref_p)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetOldParastext";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("para_ref", OracleDbType.Varchar2).Value = ref_p;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["PARA_TEXT"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string GetAllParaText(int COM_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_ALL_PARA_TEXT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("CM_ID", OracleDbType.Varchar2).Value = COM_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["PARA_TEXT"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AuditeeOldParasPpnoModel> GetOldParasForMonitoringPpno(int ppno)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
           
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeOldParasPpnoModel> list = new List<AuditeeOldParasPpnoModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.p_ppno_para";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = ppno;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditeeOldParasPpnoModel model = new AuditeeOldParasPpnoModel();

                    model.ComId = rdr["COM_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["COM_ID"]);
                    model.OldParaId = rdr["OLD_PARA_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["OLD_PARA_ID"]);
                    model.NewParaId = rdr["NEW_PARA_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["NEW_PARA_ID"]);
                    model.EntityName = rdr["NAME"]?.ToString();
                    model.AuditPeriod = rdr["AUDIT_PERIOD"]?.ToString();
                    model.Amount = rdr["AMOUNT"].ToString();
                    model.Annex = rdr["CODE"]?.ToString();
                    model.ParaStatus = rdr["PARA_STATUS"]?.ToString();
                    model.Ind = rdr["IND"]?.ToString();
                    model.ParaNo = rdr["PARA_NO"]?.ToString();
                    model.GistOfParas = rdr["GIST_OF_PARAS"]?.ToString();

                    list.Add(model);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddChangeStatusRequestForSettledPara(string REFID, string OBS_ID, string INDICATOR, int NEW_STATUS, string REMARKS)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.p_changestatusrequestforsettledpara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("refp", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("au_obs_id", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ind", OracleDbType.Varchar2).Value = INDICATOR;
                cmd.Parameters.Add("NewStatus", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("remarks", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remark"].ToString();
                    }

                }
            con.Dispose();
            return resp;
            }

        public string ReviewerAddChangeStatusRequestForSettledPara(string REFID, string IND, string REMARKS, string Action_IND)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_ChangeStatusRequestForSettledPara_new_reviewer";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obsid", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("P_IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("Remark", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("ind", OracleDbType.Varchar2).Value = Action_IND;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remark"].ToString();
                    }

                }
            con.Dispose();
            return resp;
            }

        public string AddChangeStatusRequestForCurrentPara(string REFID, int NEW_STATUS, string REMARKS)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_ChangeStatusRequestForSettledPara_new";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obs_id", OracleDbType.Varchar2).Value = REFID;
                cmd.Parameters.Add("NewStatus", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("remarks", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remark"].ToString();
                    }

                }
            con.Dispose();
            return resp;
            }

        public string AddObservationGistAndRecommendation(int OBS_ID = 0, string GIST_OF_PARA = "", string AUDITOR_RECOMMENDATION = "")
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditPlanReportModel> planList = new List<AuditPlanReportModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {

                string _sql = "pkg_hd.P_audit_pre_Concluding";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("obsid", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("gist", OracleDbType.Varchar2).Value = GIST_OF_PARA;
                cmd.Parameters.Add("recom", OracleDbType.Varchar2).Value = AUDITOR_RECOMMENDATION;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                cmd.CommandText = _sql;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();

                    }
                }
            con.Dispose();
            return resp;
            }

        public List<AuditConcludingEntitiesModel> GetAuditConcludingEntities()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditConcludingEntitiesModel> list = new List<AuditConcludingEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.p_get_audit_concluding_entities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditConcludingEntitiesModel chk = new AuditConcludingEntitiesModel();

                    chk.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();

                    chk.ENG_ID = Convert.ToInt32(rdr["ENG_ID"].ToString());
                    chk.TYPE_ID = Convert.ToInt32(rdr["TYPE_ID"].ToString());
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string ConcludeDraftAuditReport(int ENG_ID)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            if (ENG_ID == 0)
                ENG_ID = this.GetLoggedInUserEngId();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_Audit_Concluding";
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

        public string SubmitPreConcluding(int ENG_ID)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_audit_pre_submission";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("engid", OracleDbType.Int32).Value = ENG_ID;
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

        public string UpdateAuditParaForFinalization(int OBS_ID, string ANNEX_ID, string PROCESS_ID, int SUB_PROCESS_ID, int PROCESS_DETAIL_ID, int RISK_ID, int FINAL_PARA_NO, string GIST_OF_PARA, string TEXT_PARA, string AMOUNT_INV, string NO_INST)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_audit_para_update_svz_az";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("ANXID", OracleDbType.Int32).Value = ANNEX_ID;
                cmd.Parameters.Add("PROCID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("SUB_PROCID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("PROC_DETID", OracleDbType.Int32).Value = PROCESS_DETAIL_ID;
                cmd.Parameters.Add("RISKID", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("FINAL_PARA", OracleDbType.Int32).Value = FINAL_PARA_NO;
                cmd.Parameters.Add("PARA_GIST", OracleDbType.Varchar2).Value = GIST_OF_PARA;
                cmd.Parameters.Add("TEXT_OF_PARA", OracleDbType.Clob).Value = TEXT_PARA;
                cmd.Parameters.Add("AMOUNT_INV", OracleDbType.Int32).Value = AMOUNT_INV;
                cmd.Parameters.Add("NO_INST", OracleDbType.Int32).Value = NO_INST;
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

        public string UpdateAuditParaForFinalizationHO(int OBS_ID, string VIOLATION_ID, int VIOLATION_NATURE_ID, int RISK_ID, string GIST_OF_PARA, string TEXT_PARA)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_audit_para_update_head_dept";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBID", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("VID", OracleDbType.Int32).Value = VIOLATION_ID;
                cmd.Parameters.Add("VNATURE_ID", OracleDbType.Int32).Value = VIOLATION_NATURE_ID;
                cmd.Parameters.Add("RISKID", OracleDbType.Int32).Value = RISK_ID;
                cmd.Parameters.Add("PARA_GIST", OracleDbType.Varchar2).Value = GIST_OF_PARA;
                cmd.Parameters.Add("TEXT_OF_PARA", OracleDbType.Clob).Value = TEXT_PARA;
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

        public List<AuditEntitiesModel> GetAuditeeEntitiesType()
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetAuditEntitiestype";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel entity = new AuditEntitiesModel();
                    entity.TYPE_ID = Convert.ToInt32(rdr["TYPEID"]);
                    entity.ENTITYTYPEDESC = rdr["E_NAME"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditEntitiesModel> GetAuditEntitiesByTypeId(int ENTITY_TYPE_ID)
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditEntitiesModel> entitiesList = new List<AuditEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetAuditEntities";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("TYPEID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditEntitiesModel entity = new AuditEntitiesModel();
                    entity.TYPE_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    entity.ENTITYTYPEDESC = rdr["E_NAME"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditPeriodModel> GetAuditYearForAddLegacyPara()
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditPeriodModel> entitiesList = new List<AuditPeriodModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetAuditYear";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditPeriodModel entity = new AuditPeriodModel();
                    entity.AUDITPERIODID = Convert.ToInt32(rdr["audit_year"]);
                    entity.DESCRIPTION = rdr["period"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public List<AuditNatureModel> GetAuditNatureForAddLegacyPara()
            {
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditNatureModel> entitiesList = new List<AuditNatureModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetAuditnature";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditNatureModel entity = new AuditNatureModel();
                    entity.N_ID = Convert.ToInt32(rdr["NID"]);
                    entity.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    entitiesList.Add(entity);
                    }
                }
            con.Dispose();
            return entitiesList;

            }

        public string AddNewLegacyPara(AddNewLegacyParaModel LEGACY_PARA)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditNatureModel> entitiesList = new List<AuditNatureModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_add_legacy_Para";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("typeid", OracleDbType.Int32).Value = LEGACY_PARA.ENTITY_TYPE_ID;
                cmd.Parameters.Add("audityear", OracleDbType.Varchar2).Value = LEGACY_PARA.AUDIT_YEAR;
                cmd.Parameters.Add("PARANO", OracleDbType.Varchar2).Value = LEGACY_PARA.PARA_NO;
                cmd.Parameters.Add("GIST", OracleDbType.Varchar2).Value = LEGACY_PARA.GIST_OF_PARA;
                cmd.Parameters.Add("ANEXURE", OracleDbType.Varchar2).Value = LEGACY_PARA.ANNEXURE;
                cmd.Parameters.Add("amount", OracleDbType.Varchar2).Value = LEGACY_PARA.AMOUNT;
                cmd.Parameters.Add("VOL", OracleDbType.Varchar2).Value = LEGACY_PARA.VOL_I_II;
                cmd.Parameters.Add("Entityid", OracleDbType.Int32).Value = LEGACY_PARA.ENTITY_ID;
                cmd.Parameters.Add("USER_ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("nature", OracleDbType.Int32).Value = LEGACY_PARA.NATURE_ID;
                cmd.Parameters.Add("ppno", OracleDbType.Int32).Value = loggedInUser.PPNumber;
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

        public List<AddNewLegacyParaModel> GetAddedLegacyParaForAuthorize()
            {
            List<AddNewLegacyParaModel> list = new List<AddNewLegacyParaModel>();
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_get_legacy_para_to_authorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITY_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AddNewLegacyParaModel lpara = new AddNewLegacyParaModel();
                    lpara.PARA_REF = rdr["REF_P"].ToString();
                    lpara.ANNEXURE = rdr["ANNEXURE"].ToString();
                    lpara.VOL_I_II = rdr["VOL_I_II"].ToString();
                    lpara.PARA_NO = rdr["PARA_NO"].ToString();
                    lpara.GIST_OF_PARA = rdr["GIST_OF_PARAS"].ToString();
                    lpara.AUDIT_YEAR = rdr["AUDIT_YEAR"].ToString();
                    lpara.E_CODE = rdr["E_CODE"].ToString();
                    lpara.NATURE = rdr["NATURE"].ToString();
                    lpara.E_NAME = rdr["E_NAME"].ToString();
                    lpara.AMOUNT = rdr["AMOUNT_INVOLVED"].ToString();
                    list.Add(lpara);
                    }
                }
            con.Dispose();
            return list;

            }

        public string AuthorizeLegacyParaAddition(string PARA_REF)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_Authorize_legacy_para_addition";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = PARA_REF;
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

        public string DeleteLegacyParaAdditionRequest(string PARA_REF)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();

            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_referedback_Del_para";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("REFP", OracleDbType.Varchar2).Value = PARA_REF;
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

        public string GetNewParaText(string OBS_ID)
            {
            string resp = "";
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GetnewParastext";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("OBS_ID", OracleDbType.Varchar2).Value = OBS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["text"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public string RequestDeleteDuplicatePara(int NEW_PARA_ID = 0, int OLD_PARA_ID = 0, string INDICATOR = "", string REMARKS = "")
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_hd.P_ADD_DUPLICATE_PARAS";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("o_para_id", OracleDbType.Int32).Value = OLD_PARA_ID;
                        cmd.Parameters.Add("n_para_id", OracleDbType.Int32).Value = NEW_PARA_ID;
                        cmd.Parameters.Add("p_ind", OracleDbType.Varchar2).Value = INDICATOR;
                        cmd.Parameters.Add("r_remarks", OracleDbType.Varchar2).Value = REMARKS;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return resp;
            }

        public List<AuditeeEntitiesModel> GetDuplicateParasAuthorizationEntityList()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<AuditeeEntitiesModel> list = new List<AuditeeEntitiesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_DUPLICATE_PARAS_ENT_FOR_AUTH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
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

        public List<DuplicateDeleteManageParaModel> GetDuplicateParasForAuthorization()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<DuplicateDeleteManageParaModel> list = new List<DuplicateDeleteManageParaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_DUPLICATE_PARAS_FOR_AUTH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    DuplicateDeleteManageParaModel chk = new DuplicateDeleteManageParaModel();
                    chk.DId = Convert.ToInt32(rdr["d_id"]);
                    chk.OldParaId = Convert.ToInt32(rdr["old_para_id"]);
                    chk.NewParaId = Convert.ToInt32(rdr["new_para_id"]);
                    chk.EntityId = rdr["entity_id"].ToString();
                    chk.EntityName = rdr["EntityName"].ToString();
                    chk.AuditPeriod = rdr["audit_period"].ToString();
                    chk.ParaGist = rdr["gist_of_paras"].ToString();
                    chk.ParaNo = rdr["para_no"].ToString();
                    chk.Ind = rdr["ind"].ToString();
                    chk.Risk = rdr["risk"].ToString();
                    chk.Instances = rdr["instances"].ToString();
                    chk.Amount = rdr["amount"].ToString();
                    chk.Annex = rdr["annex"].ToString();
                    chk.AddedBy = rdr["added_by"].ToString();
                    chk.AddedOn = Convert.ToDateTime(rdr["added_on"]);
                    chk.Remarks = rdr["remarks"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string RejectDeleteDuplicatePara(int D_PARA_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_hd.P_REJECT_DUPLICATE_PARAS";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("did", OracleDbType.Int32).Value = D_PARA_ID;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return resp;
            }

        public string AuthDeleteDuplicatePara(int D_PARA_ID = 0, string INDICATOR = "", string REMARKS = "")
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";

            try
                {
                using (var con = this.DatabaseConnection())
                    {
                   

                    using (var cmd = con.CreateCommand())
                        {
                        cmd.CommandText = "pkg_hd.P_AUTH_DUPLICATE_PARAS";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("did", OracleDbType.Int32).Value = D_PARA_ID;
                        cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                        cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                        cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                        cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var rdr = cmd.ExecuteReader())
                            {
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            }
                        }
                    }
                }
            catch (Exception)
                {

                throw;
                }

            return resp;
            }

        public ObservationModel GetObservationDetailsById(int OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ObservationModel resp = new ObservationModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_OBSERVATION_DETAILS_FROM_ID";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obid", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {

                    resp.ANNEXURE_ID = rdr["annex_id"].ToString();
                    resp.PROCESS_ID = Convert.ToInt32(rdr["t_id"].ToString());
                    resp.SUBCHECKLIST_ID = Convert.ToInt32(rdr["s_id"].ToString());
                    resp.CHECKLISTDETAIL_ID = Convert.ToInt32(rdr["d_id"].ToString());
                    resp.RISKMODEL_ID = Convert.ToInt32(rdr["severity"].ToString());
                    resp.HEADING = rdr["headings"].ToString();
                    resp.OBSERVATION_TEXT = rdr["text"].ToString();
                    resp.AUDITEE_REPLY = rdr["reply"].ToString();
                    resp.AUDITOR_RECOM = rdr["recommendation"].ToString();
                    resp.AMOUNT_INVOLVED = rdr["amount_involved"].ToString();
                    resp.NO_OF_INSTANCES = rdr["no_of_instances"].ToString();
                    resp.DSA_ISSUED = rdr["DSA"].ToString();
                    resp.ENG_ID = Convert.ToInt32(rdr["engplanid"]);
                    resp.RESPONSIBLE_PPNO = this.GetObservationResponsiblePPNOs(OBS_ID, resp.ENG_ID);

                    }
                }
            con.Dispose();
            return resp;
            }

        public ObservationModel GetObservationDetailsByIdHO(int OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ObservationModel resp = new ObservationModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_OBSERVATION_DETAILS_FROM_ID_HO";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obid", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {

                    if (!string.IsNullOrEmpty(rdr["control_violation"].ToString()))
                        resp.PROCESS_ID = Convert.ToInt32(rdr["control_violation"].ToString());
                    if (!string.IsNullOrEmpty(rdr["nature_id"].ToString()))
                        resp.SUBCHECKLIST_ID = Convert.ToInt32(rdr["nature_id"].ToString());
                    resp.RISKMODEL_ID = Convert.ToInt32(rdr["severity"].ToString());
                    resp.HEADING = rdr["headings"].ToString();
                    resp.OBSERVATION_TEXT = rdr["text"].ToString();
                    resp.AUDITEE_REPLY = rdr["reply"].ToString();
                    resp.AUDITOR_RECOM = rdr["recommendation"].ToString();

                    }
                }
            con.Dispose();
            return resp;
            }

        public ObservationModel GetObservationDetailsByIdForPreConcluding(int OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ObservationModel resp = new ObservationModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obid", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {

                    resp.ANNEXURE_ID = rdr["annex_id"].ToString();
                    resp.PROCESS_ID = Convert.ToInt32(rdr["t_id"].ToString());
                    resp.SUBCHECKLIST_ID = Convert.ToInt32(rdr["s_id"].ToString());
                    resp.CHECKLISTDETAIL_ID = Convert.ToInt32(rdr["d_id"].ToString());
                    resp.RISKMODEL_ID = Convert.ToInt32(rdr["severity"].ToString());
                    resp.HEADING = rdr["headings"].ToString();
                    resp.FINAL_PARA_NO = Convert.ToInt32(rdr["Final_PARA_NO"].ToString());
                    resp.OBSERVATION_TEXT = rdr["text"].ToString();
                    resp.AUDITEE_REPLY = rdr["reply"].ToString();
                    resp.AUDITOR_RECOM = rdr["recommendation"].ToString();
                    resp.HEAD_RECOM = rdr["head_recom"].ToString();
                    resp.QA_RECOM = rdr["qa_recom"].ToString();
                    resp.QA_GIST = rdr["qa_gist"].ToString();
                    resp.AMOUNT_INVOLVED = rdr["amount_involved"].ToString();
                    resp.NO_OF_INSTANCES = rdr["no_of_instances"].ToString();
                    resp.ENG_ID = Convert.ToInt32(rdr["engplanid"]);
                    resp.RESPONSIBLE_PPNO = this.GetObservationResponsiblePPNOs(OBS_ID, resp.ENG_ID);

                    }
                }
            con.Dispose();
            return resp;
            }

        public ObservationModel GetObservationDetailsByIdForPreConcludingHO(int OBS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            ObservationModel resp = new ObservationModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_OBSERVATION_DETAILS_FROM_ID_PRE_CON_HO";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("obid", OracleDbType.Int32).Value = OBS_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {

                    resp.ANNEXURE_ID = rdr["annex_id"].ToString();
                    resp.PROCESS_ID = Convert.ToInt32(rdr["t_id"].ToString());
                    resp.SUBCHECKLIST_ID = Convert.ToInt32(rdr["s_id"].ToString());
                    resp.CHECKLISTDETAIL_ID = Convert.ToInt32(rdr["d_id"].ToString());
                    resp.RISKMODEL_ID = Convert.ToInt32(rdr["severity"].ToString());
                    resp.HEADING = rdr["headings"].ToString();
                    resp.OBSERVATION_TEXT = rdr["text"].ToString();
                    resp.AUDITEE_REPLY = rdr["reply"].ToString();
                    resp.AUDITOR_RECOM = rdr["recommendation"].ToString();
                    resp.HEAD_RECOM = rdr["head_recom"].ToString();
                    resp.QA_RECOM = rdr["qa_recom"].ToString();
                    resp.QA_GIST = rdr["qa_gist"].ToString();
                    resp.AMOUNT_INVOLVED = rdr["amount_involved"].ToString();
                    resp.NO_OF_INSTANCES = rdr["no_of_instances"].ToString();
                    resp.ENG_ID = Convert.ToInt32(rdr["engplanid"]);
                    resp.RESPONSIBLE_PPNO = this.GetObservationResponsiblePPNOs(OBS_ID, resp.ENG_ID);

                    }
                }
            con.Dispose();
            return resp;
            }

        public async Task<string> UploadAuditReport(int ENG_ID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            string resp = "";
            List<AuditeeResponseEvidenceModel> AUDIT_REPORT = new List<AuditeeResponseEvidenceModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                AUDIT_REPORT = await this.GetUploadedAuditReportsFromDirectory(ENG_ID.ToString());
                int index = 1;
                if (AUDIT_REPORT != null)
                    {
                    if (AUDIT_REPORT.Count > 0)
                        {
                        foreach (var item in AUDIT_REPORT)
                            {
                            cmd.CommandText = "pkg_hd.P_UPLOAD_AUDIT_REPORT";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                            cmd.Parameters.Add("AREP", OracleDbType.Clob).Value = item.IMAGE_DATA;
                            cmd.Parameters.Add("REP_TYPE", OracleDbType.Varchar2).Value = item.IMAGE_TYPE;
                            cmd.Parameters.Add("REP_NAME", OracleDbType.Varchar2).Value = item.FILE_NAME;
                            cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                            cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                            cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                            OracleDataReader rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                                {
                                resp = rdr["remarks"].ToString();
                                }
                            index++;
                            }
                        }
                    }

                this.DeleteAuditReportSubFolderDirectoryFromServer(ENG_ID.ToString());
                }

            con.Dispose();
            return resp;
            }

        public List<FinalAuditReportModel> GetAuditReports(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            List<FinalAuditReportModel> repList = new List<FinalAuditReportModel>();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_FINAL_AUDIT_REPORT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FinalAuditReportModel z = new FinalAuditReportModel();
                    z.ID = rdr["ID"].ToString();
                    z.ENTITY_ID = rdr["ENTITY_ID"].ToString();
                    z.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    z.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    repList.Add(z);
                    }
                }
            con.Dispose();
            return repList;
            }

        public AuditeeResponseEvidenceModel GetAuditReportContent(string FILE_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var resp = new AuditeeResponseEvidenceModel();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_AUDIT_REPORT_CONTENT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("FILE_ID", OracleDbType.Varchar2).Value = FILE_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = new AuditeeResponseEvidenceModel
                        {

                        FILE_ID = (rdr["id"].ToString()),
                        IMAGE_TYPE = (rdr["doc_type"].ToString()),
                        IMAGE_NAME = (rdr["doc_name"].ToString())
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

        public FinalAuditReportModel GetCheckAuditReportExisits(int ENG_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            FinalAuditReportModel resp = new FinalAuditReportModel();
            var loggedInUser = sessionHandler.GetUserOrThrow();


            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_GET_CHECK_AUDIT_REPORT_UPLOADED";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = ENG_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp.ID = rdr["ID"].ToString();
                    resp.DOC_TYPE = rdr["doc_type"].ToString();
                    resp.DOC_NAME = rdr["doc_name"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<ParaStatusChangeModel> GetParasForStatusChange(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ParaStatusChangeModel> list = new List<ParaStatusChangeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_Get_Paras_For_Status_Change";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ParaStatusChangeModel chk = new ParaStatusChangeModel();
                    chk.COM_ID = rdr["COM_ID"].ToString();
                    chk.OLD_PARA_ID = rdr["OLD_PARA_ID"].ToString();
                    chk.NEW_PARA_ID = rdr["NEW_PARA_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.RISK = rdr["RISK"].ToString();
                    chk.IND = rdr["IND"].ToString();
                    chk.PARA_STATUS = rdr["PARA_STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AddChangeStatusRequestForPara(string COM_ID, int NEW_STATUS, string REMARKS, string IND, String Action_IND)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_Add_Paras_For_Status_Change";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = COM_ID;
                cmd.Parameters.Add("NewStatus", OracleDbType.Int32).Value = NEW_STATUS;
                cmd.Parameters.Add("remarks", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("Action_IND", OracleDbType.Varchar2).Value = Action_IND;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remark"].ToString();
                    }

                }
            con.Dispose();
            return resp;
            }

        public List<ParaStatusChangeModel> GetParasForStatusChangeToAuthorize()
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ParaStatusChangeModel> list = new List<ParaStatusChangeModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_Get_Paras_For_Status_Change_For_Authorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ParaStatusChangeModel chk = new ParaStatusChangeModel();
                    chk.COM_ID = rdr["COM_ID"].ToString();
                    chk.OLD_PARA_ID = rdr["OLD_PARA_ID"].ToString();
                    chk.NEW_PARA_ID = rdr["NEW_PARA_ID"].ToString();
                    chk.AUDIT_PERIOD = rdr["AUDIT_PERIOD"].ToString();
                    chk.PARA_NO = rdr["PARA_NO"].ToString();
                    chk.GIST_OF_PARAS = rdr["gist_of_paras"].ToString();
                    chk.RISK = rdr["RISK"].ToString();
                    chk.IND = rdr["IND"].ToString();
                    chk.PARA_STATUS = rdr["OLD_PARA_STATUS"].ToString();
                    chk.NEW_PARA_STATUS = rdr["NEW_PARA_STATUS"].ToString();
                    list.Add(chk);
                    }
                }
            con.Dispose();
            return list;
            }

        public string AuthorizeChangeStatusRequestForPara(string COM_ID, int NEW_PARA_ID, int OLD_PARA_ID, string REMARKS, string IND, String Action_IND)
            {
            string resp = "";
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_hd.P_Authorize_Paras_For_Status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("C_ID", OracleDbType.Int32).Value = COM_ID;
                cmd.Parameters.Add("N_PARA_ID", OracleDbType.Int32).Value = NEW_PARA_ID;
                cmd.Parameters.Add("O_PARA_ID", OracleDbType.Int32).Value = OLD_PARA_ID;
                cmd.Parameters.Add("remarks", OracleDbType.Varchar2).Value = REMARKS;
                cmd.Parameters.Add("IND", OracleDbType.Varchar2).Value = IND;
                cmd.Parameters.Add("Action_IND", OracleDbType.Varchar2).Value = Action_IND;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["Remark"].ToString();
                    }

                }
            con.Dispose();
            return resp;
            }

        public bool AddOldParas(OldParasModel jm)
            {
            var sessionHandler = CreateSessionHandler();


            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (OracleCommand cmd = con.CreateCommand())
                {
                List<int> PP_NOs = new List<int>();
                jm.STATUS = 1;
                jm.ENTERED_BY = loggedInUser.PPNumber;
                if (jm.RESPONSIBLE_PP_NO != "" && jm.RESPONSIBLE_PP_NO != null)
                    {
                    PP_NOs = jm.RESPONSIBLE_PP_NO.Split(',').Select(int.Parse).ToList();
                    }
                cmd.CommandText = "pkg_hd.P_AddOldParas";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("PROCESS", OracleDbType.Int32).Value = jm.PROCESS;
                cmd.Parameters.Add("SUBPROCESS", OracleDbType.Int32).Value = jm.SUB_PROCESS;
                cmd.Parameters.Add("PROCESSDETAIL", OracleDbType.Int32).Value = jm.PROCESS_DETAIL;
                cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("REPLYTEXT", OracleDbType.Clob).Value = jm.PARA_TEXT;
                cmd.Parameters.Add("PID", OracleDbType.Clob).Value = jm.ID;
                cmd.ExecuteReader();
                foreach (int pp in PP_NOs)
                    {
                    cmd.CommandText = "pkg_ais.P_AddOldParasResponsibilityAssigned";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("REF_P", OracleDbType.Int32).Value = jm.ID;
                    cmd.Parameters.Add("PPNO", OracleDbType.Int32).Value = pp;
                    cmd.ExecuteReader();
                    }
                }
            con.Dispose();
            return true;
            }


        }
    }

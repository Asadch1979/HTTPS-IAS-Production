using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public List<RiskProcessDefinition> GetFunctionalListForDashboard()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_names";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["entity_id"]);
                    proc.P_NAME = rdr["Functional_owner"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<RiskProcessDefinition> GetViolationListForDashboard(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_names_checklist";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();

                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["t_id"]);
                    proc.P_NAME = rdr["heading"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<RiskProcessDefinition> GetSubViolationListForDashboard(int ENTITY_ID = 0, int PROCESS_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_names_checklist_sub";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("PROCESSID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["S_ID"]);
                    proc.P_NAME = rdr["HEADING"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<RiskProcessDefinition> GetHOFunctionalListForDashboard(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_names_ho";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("AUDITEDBY", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["entity_id"]);
                    proc.P_NAME = rdr["Functional_owner"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<RiskProcessDefinition> GetHOViolationListForDashboard(int ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_names_checklist_ho";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["t_id"]);
                    proc.P_NAME = rdr["heading"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<RiskProcessDefinition> GetHOSubViolationListForDashboard(int ENTITY_ID = 0, int PROCESS_ID = 0)
            {
            var con = this.DatabaseConnection();
            List<RiskProcessDefinition> pdetails = new List<RiskProcessDefinition>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_names_checklist_sub_ho";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = ENTITY_ID;
                cmd.Parameters.Add("PROCESSID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RiskProcessDefinition proc = new RiskProcessDefinition();
                    proc.P_ID = Convert.ToInt32(rdr["S_ID"]);
                    proc.P_NAME = rdr["HEADING"].ToString();
                    pdetails.Add(proc);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<UserRelationshipModel> GetchildpostingForDashboardPanel(int e_r_id = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            if (e_r_id == 0)
                e_r_id = Convert.ToInt32(loggedInUser.UserEntityID);

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Getchildposting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("erid", OracleDbType.Int32).Value = e_r_id;
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
            con.Dispose();
            return entitiesList;

            }

        public List<UserRelationshipModel> GetparentrepofficeForDashboardPanel(int r_id = 0)
            {

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Getparentrepoffice";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("rid", OracleDbType.Int32).Value = r_id;
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
            con.Dispose();
            return entitiesList;

            }

        public List<UserRelationshipModel> GetrealtionshiptypeForDashboardPanel()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<UserRelationshipModel> entitiesList = new List<UserRelationshipModel>();
            var con = this.DatabaseConnection();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Getrealtionshiptype";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserRoleid", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
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
            con.Dispose();
            return entitiesList;

            }

        public List<FunctionalResponsibilityWiseParas> GetFunctionalResponsibilityWisePara(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<FunctionalResponsibilityWiseParas> list = new List<FunctionalResponsibilityWiseParas>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GetFunctionalResponsibilityWisePara";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENTITYID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("PROCESSID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("SUB_PROCESSID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("PROCESS_DETAILID", OracleDbType.Int32).Value = PROCESS_DETAIL_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();


                while (rdr.Read())
                    {
                    FunctionalResponsibilityWiseParas para = new FunctionalResponsibilityWiseParas();
                    para.PROCESS_ID = Convert.ToInt32(rdr["PROCESS_ID"].ToString());
                    para.PROCESS = rdr["PROCESS"].ToString();
                    para.SUB_PROCESS_ID = Convert.ToInt32(rdr["SUB_PROCESS_ID"].ToString());
                    para.VIOLATION = rdr["VIOLATION"].ToString();
                    para.CHECK_LIST_DETAIL_ID = Convert.ToInt32(rdr["CHECK_LIST_DETAIL_ID"].ToString());
                    para.PERIOD = rdr["PERIOD"].ToString();
                    para.OBS_ID = Convert.ToInt32(rdr["OBS_ID"].ToString());
                    para.ENTITY_NAME = rdr["ENTITY_NAME"].ToString();
                    para.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();

                    para.MEMO_NO = rdr["MEMO_NO"].ToString();
                    para.OBS_TEXT = rdr["OBS_TEXT"].ToString();
                    para.OBS_RISK_ID = Convert.ToInt32(rdr["OBS_RISK_ID"].ToString());
                    para.OBS_RISK = rdr["OBS_RISK"].ToString();
                    para.OBS_STATUS_ID = Convert.ToInt32(rdr["OBS_STATUS_ID"].ToString());
                    para.OBS_STATUS = rdr["OBS_STATUS"].ToString();
                    list.Add(para);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<FunctionalResponsibilitiesWiseParasModel> GetFunctionalResponsibilityWiseParaForDashboard(int FUNCTIONAL_ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FunctionalResponsibilitiesWiseParasModel> list = new List<FunctionalResponsibilitiesWiseParasModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = FUNCTIONAL_ENTITY_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    FunctionalResponsibilitiesWiseParasModel zb = new FunctionalResponsibilitiesWiseParasModel();
                    zb.OBS_ID = rdr["d_id"].ToString();
                    zb.PARA_REF = rdr["ref_p"].ToString();
                    zb.PARA_CATEGORY = rdr["pc"].ToString();
                    zb.REP_OFFICE = rdr["p_name"].ToString();
                    zb.ENTITY_NAME = rdr["c_name"].ToString();
                    zb.ANNEXURE = rdr["Annex"].ToString();
                    zb.CHECK_LIST = rdr["chlist"].ToString();
                    zb.PARA_NO = rdr["final_para_no"].ToString();
                    zb.RISK = rdr["Risk"].ToString();
                    zb.GIST = rdr["Gist"].ToString();
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<FADNewOldParaPerformanceModel> GetHOFunctionalResponsibilityWiseParaForDashboard(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0, int FUNCTIONAL_ENTITY_ID = 0, int DEPT_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADNewOldParaPerformanceModel> list = new List<FADNewOldParaPerformanceModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_ho";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = FUNCTIONAL_ENTITY_ID;
                cmd.Parameters.Add("PROCESSID", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("SUB_PROCESSID", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("AUDITEDBY", OracleDbType.Int32).Value = DEPT_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    FADNewOldParaPerformanceModel zb = new FADNewOldParaPerformanceModel();
                    zb.Process = rdr["Functional_owner"].ToString();
                    zb.Total_Paras = rdr["Total_Paras"].ToString();
                    zb.Setteled_Para = rdr["Setteled_Para"].ToString();
                    zb.Unsetteled_Para = rdr["Unsetteled_Para"].ToString();
                    zb.Ratio = rdr["Ratio"].ToString();
                    zb.R1 = rdr["R1"].ToString();
                    zb.R2 = rdr["R2"].ToString();
                    zb.R3 = rdr["R3"].ToString();
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<FADNewOldParaPerformanceModel> GetViolationWiseParaForDashboard(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<FADNewOldParaPerformanceModel> list = new List<FADNewOldParaPerformanceModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_v_wise";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("process_id", OracleDbType.Int32).Value = PROCESS_ID;
                cmd.Parameters.Add("sub_id", OracleDbType.Int32).Value = SUB_PROCESS_ID;
                cmd.Parameters.Add("d_id", OracleDbType.Int32).Value = PROCESS_DETAIL_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADNewOldParaPerformanceModel zb = new FADNewOldParaPerformanceModel();
                    zb.Process = rdr["process"].ToString();
                    zb.Total_Paras = rdr["Total_Paras"].ToString();
                    zb.Setteled_Para = rdr["Setteled_Para"].ToString();
                    zb.Unsetteled_Para = rdr["Unsetteled_Para"].ToString();
                    zb.Ratio = rdr["Ratio"].ToString();
                    zb.R1 = rdr["R1"].ToString();
                    zb.R2 = rdr["R2"].ToString();
                    zb.R3 = rdr["R3"].ToString();
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<FADNewOldParaPerformanceModel> GetRelationLegacyObservationForDashboard(int USER_ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADNewOldParaPerformanceModel> list = new List<FADNewOldParaPerformanceModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_old";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = USER_ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADNewOldParaPerformanceModel zb = new FADNewOldParaPerformanceModel();
                    zb.Process = rdr["Process"].ToString();
                    zb.Total_Paras = rdr["Total_Paras"].ToString();
                    zb.Setteled_Para = rdr["Setteled_Para"].ToString();
                    zb.Unsetteled_Para = rdr["Unsetteled_Para"].ToString();
                    zb.Ratio = rdr["Ratio"].ToString();
                    zb.R1 = rdr["R1"].ToString();
                    zb.R2 = rdr["R2"].ToString();
                    zb.R3 = rdr["R3"].ToString();
                    list.Add(zb);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<FADNewOldParaPerformanceModel> GetRelationAISObservationForDashboard(int USER_ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADNewOldParaPerformanceModel> list = new List<FADNewOldParaPerformanceModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_new";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = USER_ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADNewOldParaPerformanceModel zb = new FADNewOldParaPerformanceModel();
                    zb.Process = rdr["Process"].ToString();
                    zb.Total_Paras = rdr["Total_Paras"].ToString();
                    zb.Setteled_Para = rdr["Setteled_Para"].ToString();
                    zb.Unsetteled_Para = rdr["Unsetteled_Para"].ToString();
                    zb.Ratio = rdr["Ratio"].ToString();
                    zb.R1 = rdr["R1"].ToString();
                    zb.R2 = rdr["R2"].ToString();
                    zb.R3 = rdr["R3"].ToString();
                    list.Add(zb);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<FADNewOldParaPerformanceModel> GetRelationObservationForDashboard(int USER_ENTITY_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<FADNewOldParaPerformanceModel> list = new List<FADNewOldParaPerformanceModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = USER_ENTITY_ID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADNewOldParaPerformanceModel zb = new FADNewOldParaPerformanceModel();
                    zb.Process = rdr["Process"].ToString();
                    zb.Total_Paras = rdr["Total_Paras"].ToString();
                    zb.Setteled_Para = rdr["Setteled_Para"].ToString();
                    zb.Unsetteled_Para = rdr["Unsetteled_Para"].ToString();
                    zb.Ratio = rdr["Ratio"].ToString();
                    zb.R1 = rdr["R1"].ToString();
                    zb.R2 = rdr["R2"].ToString();
                    zb.R3 = rdr["R3"].ToString();
                    list.Add(zb);

                    }
                }
            con.Dispose();
            return list;
            }

        public List<NoEntitiesRiskBasePlan> GetEntitiesRiskBasePlanForDashboard(int PROCESS_ID = 0, int SUB_PROCESS_ID = 0, int PROCESS_DETAIL_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<NoEntitiesRiskBasePlan> list = new List<NoEntitiesRiskBasePlan>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.p_get_risk_baseplan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    NoEntitiesRiskBasePlan zb = new NoEntitiesRiskBasePlan();
                    zb.ENTITY_NAME = rdr["name"].ToString();
                    zb.ENTITY_DESC = rdr["entitytypedesc"].ToString();
                    zb.ENTITY_RISK = rdr["risk"].ToString();
                    zb.ENTITY_SIZE = rdr["entity_size"].ToString();
                    zb.ENTITY_NO = Convert.ToInt32(rdr["no_of_enitites"].ToString());
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<FADAuditPerformanceModel> GetAuditPerformanceForDashboard()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<FADAuditPerformanceModel> list = new List<FADAuditPerformanceModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_AUDIT_PERFORMANCE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FADAuditPerformanceModel zb = new FADAuditPerformanceModel();
                    zb.Title = rdr["title"].ToString();
                    zb.Total_Paras = rdr["Total_Paras"].ToString();
                    zb.Setteled_Para = rdr["Setteled_Para"].ToString();
                    zb.Unsetteled_Para = rdr["Unsetteled_Para"].ToString();
                    zb.Ratio = rdr["Ratio"].ToString();
                    zb.R1 = rdr["R1"].ToString();
                    zb.R2 = rdr["R2"].ToString();
                    zb.R3 = rdr["R3"].ToString();
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<AuditPerformanceChartDashboardModel> GetAuditPerformanceChartForDashboard()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<AuditPerformanceChartDashboardModel> list = new List<AuditPerformanceChartDashboardModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.p_get_dashborad_scorecard";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditPerformanceChartDashboardModel zb = new AuditPerformanceChartDashboardModel();
                    zb.HEADING = rdr["heading"].ToString();
                    zb.NO_OF_ENTITIES = rdr["no_of_ent"].ToString();
                    zb.TOTAL_ENTITIES = rdr["tot_ent"].ToString();
                    zb.REMARKS = rdr["remarks"].ToString();
                    zb.DEPARTMENT = rdr["department"].ToString();
                    zb.PERCENTAGE = rdr["pencent"].ToString();
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<RepetativeParaModel> GetRepetativeParaForDashboard(int P_ID = 0, int SP_ID = 0, int PD_ID = 0)

            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            List<RepetativeParaModel> list = new List<RepetativeParaModel>();
            var con = this.DatabaseConnection();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.p_get_dash_repetitive";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_ID", OracleDbType.Int32).Value = P_ID;
                cmd.Parameters.Add("PS_ID", OracleDbType.Int32).Value = SP_ID;
                cmd.Parameters.Add("D_ID", OracleDbType.Int32).Value = PD_ID;
                cmd.Parameters.Add("UserEntityID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    RepetativeParaModel zb = new RepetativeParaModel();
                    zb.PROCESS = rdr["PROCESS"].ToString();
                    zb.SUB_PROCESS = rdr["SUB_PROCESS"].ToString();
                    zb.PROCESS_DETAIL = rdr["CHECKLIST_DETAILS"].ToString();
                    zb.Y2023 = rdr["YEAR_2023"].ToString();
                    zb.Y2022 = rdr["YEAR_2022"].ToString();
                    zb.Y2021 = rdr["YEAR_2021"].ToString();
                    zb.Y2020 = rdr["YEAR_2020"].ToString();
                    zb.Y2019 = rdr["YEAR_2019"].ToString();
                    zb.Y2018 = rdr["YEAR_2018"].ToString();
                    zb.Y2017 = rdr["YEAR_2017"].ToString();
                    zb.Y2016 = rdr["YEAR_2016"].ToString();
                    zb.Y2015 = rdr["YEAR_2015"].ToString();
                    zb.Y2014 = rdr["YEAR_2014"].ToString();
                    zb.Y2013 = rdr["YEAR_2013"].ToString();
                    zb.Y2012 = rdr["YEAR_2012"].ToString();
                    zb.Y2011 = rdr["YEAR_2011"].ToString();
                    zb.Y2010 = rdr["YEAR_2010"].ToString();
                    list.Add(zb);
                    }
                }
            con.Dispose();
            return list;
            }

        public List<EntityWiseObservationModel> GetReportingOfficeWiseObservations()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<EntityWiseObservationModel> pdetails = new List<EntityWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Functional_Reporting_office_WISE_ANALYSIS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntityWiseObservationModel zb = new EntityWiseObservationModel();
                    zb.ENTITY_ID = Convert.ToInt32(rdr["parent_id"].ToString());
                    zb.REPORTING_OFFICE = rdr["name"].ToString();
                    zb.TOTAL = rdr["total"].ToString();
                    zb.OLD_TOTAL = rdr["old_total"].ToString();
                    zb.NEW_TOTAL = rdr["new_total"].ToString();
                    zb.R1 = rdr["r1"].ToString();
                    zb.R2 = rdr["r2"].ToString();
                    zb.R3 = rdr["r3"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<EntityWiseObservationModel> GetEntityWiseObservations()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<EntityWiseObservationModel> pdetails = new List<EntityWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Functional_ENTITY_WISE_ANALYSIS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    EntityWiseObservationModel zb = new EntityWiseObservationModel();
                    zb.ENTITY_ID = Convert.ToInt32(rdr["entity_id"].ToString());
                    zb.REPORTING_OFFICE = rdr["REPORTING_OFFICE"].ToString();
                    zb.ENTITY_NAME = rdr["name"].ToString();
                    zb.TOTAL = rdr["total"].ToString();
                    zb.OLD_TOTAL = rdr["old_total"].ToString();
                    zb.NEW_TOTAL = rdr["new_total"].ToString();
                    zb.R1 = rdr["r1"].ToString();
                    zb.R2 = rdr["r2"].ToString();
                    zb.R3 = rdr["r3"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<AnnexWiseObservationModel> GetAnnexureWiseObservations(string positionDate)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<AnnexWiseObservationModel> pdetails = new List<AnnexWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Functional_ANALYSIS_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                DateTime parsedDate;
                object refDateValue = DBNull.Value;
                if (!string.IsNullOrWhiteSpace(positionDate) && DateTime.TryParse(positionDate, out parsedDate))
                    {
                    refDateValue = parsedDate;
                    }
                cmd.Parameters.Add("P_REF_DATE", OracleDbType.Date).Value = refDateValue;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AnnexWiseObservationModel zb = new AnnexWiseObservationModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.HEADING = rdr["Heading"].ToString();
                    zb.ANNEX = rdr["Annex"].ToString();
                    zb.AUDIT_COMMENTS = rdr["AUDIT_COMMENTS"].ToString();
                    zb.TOTAL = rdr["total"].ToString();
                    zb.OLD_TOTAL = rdr["old_total"].ToString();
                    zb.NEW_TOTAL = rdr["new_total"].ToString();
                    zb.R1 = rdr["r1"].ToString();
                    zb.R2 = rdr["r2"].ToString();
                    zb.R3 = rdr["r3"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<AnnexWiseObservationModel> GetFunctionAnnexures()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<AnnexWiseObservationModel> pdetails = new List<AnnexWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Function_Annexure";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("e_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AnnexWiseObservationModel zb = new AnnexWiseObservationModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.HEADING = rdr["Heading"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<FunctionalAnnexureWiseObservationModel> GetEntityWiseObservationDetail(int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<FunctionalAnnexureWiseObservationModel> pdetails = new List<FunctionalAnnexureWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Functional_ENTITY_WISE_Paras";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("R_ID", OracleDbType.Varchar2).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Varchar2).Value = ENTITY_ID;
                cmd.Parameters.Add("P_NO", OracleDbType.Varchar2).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FunctionalAnnexureWiseObservationModel zb = new FunctionalAnnexureWiseObservationModel();
                    zb.ID = Convert.ToInt32(rdr["entity_id"].ToString());
                    zb.NAME = rdr["name"].ToString();
                    zb.PARA_CATEGORY = rdr["para_category"].ToString();
                    zb.PARA_NO = rdr["para_no"].ToString();
                    zb.PARA_REF = rdr["ref_p"].ToString();
                    zb.OBS_ID = rdr["au_obs_id"].ToString();
                    zb.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    zb.PARA_GIST = rdr["gist_of_paras"].ToString();
                    zb.P_RISK = rdr["risk"].ToString();
                    zb.COM_ID = Convert.ToInt32(rdr["COM_ID"].ToString());
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<FunctionalAnnexureWiseObservationModel> GetFunctionalObservations(int ANNEX_ID, int ENTITY_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<FunctionalAnnexureWiseObservationModel> pdetails = new List<FunctionalAnnexureWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Function_Annexure_Paras";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("a_id", OracleDbType.Varchar2).Value = ANNEX_ID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = ENTITY_ID;
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FunctionalAnnexureWiseObservationModel zb = new FunctionalAnnexureWiseObservationModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.NAME = rdr["name"].ToString();
                    zb.PARA_CATEGORY = rdr["para_category"].ToString();
                    zb.PARA_NO = rdr["para_no"].ToString();
                    zb.PARA_REF = rdr["ref_p"].ToString();
                    zb.OBS_ID = rdr["au_obs_id"].ToString();
                    zb.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public string GetFunctionalObservationText(int PARA_ID, string PARA_CATEGORY)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_Function_Annexure_Paras_text";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("p_id", OracleDbType.Varchar2).Value = PARA_ID;
                cmd.Parameters.Add("p_c", OracleDbType.Varchar2).Value = PARA_CATEGORY;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["para_text"].ToString();
                    }
                }
            con.Dispose();
            return resp;
            }

        public List<FunctionalAnnexureWiseObservationModel> GetFunctionalRespDetailPara(int PROCESS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<FunctionalAnnexureWiseObservationModel> pdetails = new List<FunctionalAnnexureWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_PARA";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("a_id", OracleDbType.Varchar2).Value = PROCESS_ID;
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FunctionalAnnexureWiseObservationModel zb = new FunctionalAnnexureWiseObservationModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.NAME = rdr["name"].ToString();
                    zb.PARA_CATEGORY = rdr["para_category"].ToString();
                    zb.PARA_NO = rdr["para_no"].ToString();
                    zb.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<FunctionalAnnexureWiseObservationModel> GetFunctionalRespSummaryPara(int PROCESS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<FunctionalAnnexureWiseObservationModel> pdetails = new List<FunctionalAnnexureWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_GET_Dash_table_functionwise_PARA_summary";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("a_id", OracleDbType.Varchar2).Value = PROCESS_ID;
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    FunctionalAnnexureWiseObservationModel zb = new FunctionalAnnexureWiseObservationModel();

                    zb.P_NAME = rdr["p_name"].ToString();
                    // zb.NAME = rdr["name"].ToString();
                    zb.PARA_NO = rdr["para_no"].ToString();
                    zb.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<ComplianceSummaryModel> GetComplianceSummary(int entityID)
            {

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            List<ComplianceSummaryModel> resp = new List<ComplianceSummaryModel>();

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_db.P_compliance_summary";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("ENTITY", OracleDbType.Int32).Value = entityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    ComplianceSummaryModel m = new ComplianceSummaryModel();
                    m.ID = rdr["Region_id"].ToString();
                    m.NAME = rdr["Region"].ToString();
                    m.TOTAL_PARA = rdr["Total_para"].ToString();
                    m.TOTAL_COMPLIANCE = rdr["Total_Comp"].ToString();
                    m.AT_REPORTING = rdr["AT_reporting"].ToString();
                    m.UNDER_CONSIDERATION = rdr["Under_consideration"].ToString();
                    m.SETTLED = rdr["settled"].ToString();
                    m.REJECTED = rdr["Rejected"].ToString();
                    m.ROLE_ID = loggedInUser.UserRoleID.ToString();
                    resp.Add(m);
                    }
                }
            con.Dispose();
            return resp;

            }
        }
    }

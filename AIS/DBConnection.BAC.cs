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
          public List<BACAgendaModel> GetBACAgenda(int MEETING_NO)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BACAgendaModel> pdetails = new List<BACAgendaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_BAC_AGENDA";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("Meeting", OracleDbType.Int32).Value = MEETING_NO;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACAgendaModel zb = new BACAgendaModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.MEETING_NO = rdr["meeting_no"].ToString();
                    zb.MEMO_NO = rdr["memo_no"].ToString();
                    zb.SUBJECT = rdr["subject"].ToString();
                    zb.REMARKS = rdr["remarks"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACAgendaModel> GetBACAMeetingSummary(int MEETING_NO)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BACAgendaModel> pdetails = new List<BACAgendaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_Bac_get_actionable_sum";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("User_entityid", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACAgendaModel zb = new BACAgendaModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.MEETING_NO = rdr["meeting_no"].ToString();
                    zb.MEMO_NO = rdr["memo_no"].ToString();
                    zb.SUBJECT = rdr["subject"].ToString();
                    zb.REMARKS = rdr["remarks"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACAgendaActionablesSummaryModel> GetBACAgendaActionablesConsolidatedSummary()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BACAgendaActionablesSummaryModel> pdetails = new List<BACAgendaActionablesSummaryModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_Bac_get_actionable_snap";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACAgendaActionablesSummaryModel zb = new BACAgendaActionablesSummaryModel();
                    zb.TOTAL = Convert.ToInt32(rdr["total"].ToString());
                    zb.COMPLETED = rdr["completed"].ToString();
                    zb.UN_COMPLETED = rdr["un_completed"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACAgendaActionablesSummaryModel> GetBACAgendaActionablesSummary()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BACAgendaActionablesSummaryModel> pdetails = new List<BACAgendaActionablesSummaryModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_Bac_get_actionable_sum";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACAgendaActionablesSummaryModel zb = new BACAgendaActionablesSummaryModel();
                    zb.TOTAL = Convert.ToInt32(rdr["total"].ToString());
                    zb.MEETING_NO = rdr["meeting_number"].ToString();
                    zb.COMPLETED = rdr["completed"].ToString();
                    zb.UN_COMPLETED = rdr["un_completed"].ToString();
                    zb.RESPONSIBLES = rdr["RESPONSIBLE"].ToString();
                    zb.MANAGEMENT_RESPONSE = rdr["RESPONSE"].ToString();
                    zb.REFERENCE = rdr["BAC_DIRECTIVES"].ToString();
                    zb.CIA_REMARKS = rdr["CIA_REMARKS"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACAgendaActionablesModel> GetBACAgendaActionables(string STATUS)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BACAgendaActionablesModel> pdetails = new List<BACAgendaActionablesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_Bac_get_actionable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("status", OracleDbType.Varchar2).Value = STATUS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACAgendaActionablesModel zb = new BACAgendaActionablesModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.MEETING_NO = rdr["meeting_number"].ToString();
                    zb.ITEM_HEADING = rdr["item_heading"].ToString();
                    zb.BAC_DIRECTION = rdr["bac_direction"].ToString();
                    zb.ASSIGN_TO = rdr["assign_to"].ToString();
                    zb.TIMELINE = rdr["time_line"].ToString();
                    zb.OPEN_TIMELINE = rdr["open_time_line"].ToString();
                    zb.DUE_DATE = rdr["due_date"].ToString();
                    zb.REPORT_FREQUENCY = rdr["rpt_frequency"].ToString();
                    zb.ENTERED_BY = rdr["entered_by"].ToString();
                    zb.ENTERED_ON = rdr["entered_on"].ToString();
                    zb.DELAY = rdr["delay"].ToString();
                    zb.STATUS = rdr["status"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACAgendaActionablesModel> GetBACAgendaActionablesWithMeetingNo(string STATUS, string MEETING_NO)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<BACAgendaActionablesModel> pdetails = new List<BACAgendaActionablesModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_Bac_get_actionable_meetings_with_status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("meeting", OracleDbType.Varchar2).Value = MEETING_NO;
                cmd.Parameters.Add("A_Status", OracleDbType.Varchar2).Value = STATUS;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACAgendaActionablesModel zb = new BACAgendaActionablesModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.MEETING_NO = rdr["meeting_number"].ToString();
                    zb.ITEM_HEADING = rdr["item_heading"].ToString();
                    zb.BAC_DIRECTION = rdr["bac_direction"].ToString();
                    zb.ASSIGN_TO = rdr["assign_to"].ToString();
                    zb.TIMELINE = rdr["time_line"].ToString();
                    zb.OPEN_TIMELINE = rdr["open_time_line"].ToString();
                    zb.DUE_DATE = rdr["due_date"].ToString();
                    zb.REPORT_FREQUENCY = rdr["rpt_frequency"].ToString();
                    zb.ENTERED_BY = rdr["entered_by"].ToString();
                    zb.ENTERED_ON = rdr["entered_on"].ToString();
                    zb.DELAY = rdr["delay"].ToString();
                    zb.STATUS = rdr["status"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACCIAAnalysisOptionsModel> GetBACCIAAnalysisOptions()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<BACCIAAnalysisOptionsModel> pdetails = new List<BACCIAAnalysisOptionsModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_CIA_ANALYSIS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACCIAAnalysisOptionsModel zb = new BACCIAAnalysisOptionsModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.HEADING = rdr["heading"].ToString();
                    zb.AUDIT_COMMENTS = rdr["audit_comments"].ToString();
                    zb.MONITORING = rdr["monitoring"].ToString();
                    zb.AUTOMATION = rdr["automation"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<BACCIAAnalysisModel> GetBACCIAAnalysis(int processId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<BACCIAAnalysisModel> pdetails = new List<BACCIAAnalysisModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_CIA_ANALYSIS_DETAILS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("a_id", OracleDbType.Varchar2).Value = processId;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    BACCIAAnalysisModel zb = new BACCIAAnalysisModel();
                    zb.ID = Convert.ToInt32(rdr["id"].ToString());
                    zb.COUNT = rdr["total"].ToString();
                    zb.INDICATOR = rdr["indicator"].ToString();
                    zb.ANNEX = rdr["annex"].ToString();
                    zb.HEADING = rdr["heading"].ToString();
                    zb.OLDCOUNT = rdr["old_total"].ToString();
                    zb.NEWCOUNT = rdr["new_total"].ToString();
                    zb.AUDITCOMMENTS = rdr["audit_comments"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }

        public List<FunctionalAnnexureWiseObservationModel> GetAnalysisDetailPara(int PROCESS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<FunctionalAnnexureWiseObservationModel> pdetails = new List<FunctionalAnnexureWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_CIA_ANALYSIS_DETAILS_PARA";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("a_id", OracleDbType.Varchar2).Value = PROCESS_ID;
                cmd.Parameters.Add("r_id", OracleDbType.Varchar2).Value = loggedInUser.UserGroupID;
                cmd.Parameters.Add("ent_id", OracleDbType.Varchar2).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
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

        public List<FunctionalAnnexureWiseObservationModel> GetAnalysisSummaryPara(int PROCESS_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();

            List<FunctionalAnnexureWiseObservationModel> pdetails = new List<FunctionalAnnexureWiseObservationModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_bac.P_CIA_ANALYSIS_SUMMARY";
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
                    zb.NAME = rdr["name"].ToString();
                    zb.PARA_NO = rdr["para_no"].ToString();
                    zb.AUDIT_PERIOD = rdr["audit_period"].ToString();
                    pdetails.Add(zb);
                    }
                }
            con.Dispose();
            return pdetails;
            }
        }
    }

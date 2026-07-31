using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
    {
    /// <summary>Legacy observation-status operations. New code must use the dedicated draft/finalize methods.</summary>
    public partial class DBConnection : Controller, IDBConnection
        {
        [Obsolete("Legacy status API. Use AddObservationToDraft or FinalizeOrSettleObservation.")]
        public string UpdateAuditObservationStatus(int OBS_ID, int NEW_STATUS_ID, string DRAFT_PARA_NO, string AUDITOR_COMMENT)
            {
            const int SettledByTeamLeadStatus = 4;
            const int DraftReportStatus = 5;
            const int FinalReportStatus = 8;
            const int SettledInDiscussionStatus = 9;
            var loggedInUser = CreateSessionHandler().GetUser();
            if (loggedInUser == null || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber) || loggedInUser.UserRoleID <= 0)
                return string.Empty;

            var paraNumber = (DRAFT_PARA_NO ?? string.Empty).Trim();
            if (NEW_STATUS_ID != SettledByTeamLeadStatus && NEW_STATUS_ID != DraftReportStatus
                && NEW_STATUS_ID != FinalReportStatus && NEW_STATUS_ID != SettledInDiscussionStatus)
                return "Unsupported observation status transition.";
            if (NEW_STATUS_ID == DraftReportStatus && string.IsNullOrWhiteSpace(paraNumber))
                return "Draft para number is required before adding para to draft report.";
            if ((NEW_STATUS_ID == FinalReportStatus || NEW_STATUS_ID == SettledInDiscussionStatus)
                && string.IsNullOrWhiteSpace(paraNumber))
                return "Final para number is required before adding para to final report or settling in discussion.";
            if ((NEW_STATUS_ID == FinalReportStatus || NEW_STATUS_ID == SettledInDiscussionStatus)
                && loggedInUser.UserRoleID != 6 && loggedInUser.UserRoleID != 7 && loggedInUser.UserRoleID != 15)
                return "Only Departmental Head is authorized to update this observation status.";

            var remarks = NEW_STATUS_ID switch
                {
                SettledByTeamLeadStatus => "Settle",
                DraftReportStatus => "Add to Draft Report",
                FinalReportStatus => "Add to Final Report",
                SettledInDiscussionStatus => "Para settle in discussion",
                _ => string.Empty
                };
            var response = string.Empty;
            var statusUpdated = false;
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "pkg_ar.P_UpdateAuditObservationStatus";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
            cmd.Parameters.Add("NEW_STATUS_ID", OracleDbType.Int32).Value = NEW_STATUS_ID;
            cmd.Parameters.Add("D_PARA_NO", OracleDbType.Varchar2).Value = paraNumber;
            cmd.Parameters.Add("Remarks", OracleDbType.Varchar2).Value = remarks;
            cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
            cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
            cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    {
                    response = reader["REMARKS"].ToString();
                    statusUpdated = HasColumn(reader, "REF") && reader["REF"]?.ToString() == "1";
                    }

            // Confirmed legacy mapping: 4/5 => AUDITOR_RESPONSE; 8/9 => AUDITOR_REPLY.
            // The follow-up is deliberately skipped unless the main procedure returns REF = 1.
            if (!statusUpdated)
                return response;
            cmd.Parameters.Clear();
            cmd.CommandText = NEW_STATUS_ID == 4 || NEW_STATUS_ID == 5
                ? "pkg_ar.AUDITOR_RESPONSE"
                : "pkg_ar.AUDITOR_REPLY";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = OBS_ID;
            cmd.Parameters.Add("PPNumber", OracleDbType.Int32).Value = loggedInUser.PPNumber;
            cmd.Parameters.Add("AUDITOR_COMMENT", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(AUDITOR_COMMENT) ? remarks : AUDITOR_COMMENT;
            cmd.Parameters.Add("P_status", OracleDbType.Int32).Value = NEW_STATUS_ID;
            cmd.ExecuteNonQuery();
            return response;
            }

        [Obsolete("Legacy status API. Use AddObservationToDraft or FinalizeOrSettleObservation.")]
        public string UpdateFadAuditObservationStatus(int observationId, int newStatusId, string draftParaNumber, int? finalParaNumber, string auditorComment)
            {
            var paraNumber = newStatusId == 8
                ? finalParaNumber?.ToString()
                : newStatusId == 9 ? null : draftParaNumber;
            return UpdateAuditObservationStatus(observationId, newStatusId, paraNumber, auditorComment);
            }
        }
    }

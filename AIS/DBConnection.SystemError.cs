using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public SystemErrorRecord RegisterSystemError(
            string fingerprint,
            string errorType,
            string errorCode,
            string errorMessage,
            string technicalDetails,
            SystemErrorContext context)
            {
            context ??= new SystemErrorContext();
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.REGISTER_SYSTEM_ERROR";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_FINGERPRINT", OracleDbType.Varchar2).Value = DbValue(fingerprint, 128);
            cmd.Parameters.Add("P_ERROR_TYPE", OracleDbType.Varchar2).Value = DbValue(errorType, 300);
            cmd.Parameters.Add("P_ERROR_CODE", OracleDbType.Varchar2).Value = DbValue(errorCode, 100);
            cmd.Parameters.Add("P_MODULE", OracleDbType.Varchar2).Value = DbValue(context.Module, 100);
            cmd.Parameters.Add("P_CONTROLLER", OracleDbType.Varchar2).Value = DbValue(context.Controller, 100);
            cmd.Parameters.Add("P_ACTION", OracleDbType.Varchar2).Value = DbValue(context.Action, 100);
            cmd.Parameters.Add("P_API_PATH", OracleDbType.Varchar2).Value = DbValue(context.ApiPath, 1000);
            cmd.Parameters.Add("P_STORED_PROCEDURE", OracleDbType.Varchar2).Value = DbValue(context.StoredProcedure, 200);
            cmd.Parameters.Add("P_PPNO", OracleDbType.Varchar2).Value = DbValue(context.Ppno, 50);
            cmd.Parameters.Add("P_ROLE_NAME", OracleDbType.Varchar2).Value = DbValue(context.Role, 150);
            cmd.Parameters.Add("P_ENTITY_NAME", OracleDbType.Varchar2).Value = DbValue(context.Entity, 300);
            cmd.Parameters.Add("P_PAGE_ID", OracleDbType.Int32).Value = context.PageId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = context.EngagementId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_PARA_ID", OracleDbType.Int32).Value = context.ParaId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_COM_ID", OracleDbType.Int32).Value = context.ComId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_TRACE_ID", OracleDbType.Varchar2).Value = DbValue(context.TraceId, 200);
            cmd.Parameters.Add("P_IP_ADDRESS", OracleDbType.Varchar2).Value = DbValue(context.IpAddress, 100);
            cmd.Parameters.Add("P_USER_AGENT", OracleDbType.Varchar2).Value = DbValue(context.UserAgent, 1000);
            cmd.Parameters.Add("P_ERROR_MESSAGE", OracleDbType.Clob).Value = string.IsNullOrWhiteSpace(errorMessage) ? (object)DBNull.Value : errorMessage;
            cmd.Parameters.Add("P_TECHNICAL_DETAILS", OracleDbType.Clob).Value = string.IsNullOrWhiteSpace(technicalDetails) ? (object)DBNull.Value : technicalDetails;
            var errorId = cmd.Parameters.Add("O_ERROR_ID", OracleDbType.Int64);
            errorId.Direction = ParameterDirection.Output;
            var reference = cmd.Parameters.Add("O_ERROR_REFERENCE", OracleDbType.Varchar2, 50);
            reference.Direction = ParameterDirection.Output;
            var first = cmd.Parameters.Add("O_IS_FIRST_OCCURRENCE", OracleDbType.Int32);
            first.Direction = ParameterDirection.Output;
            var firstOccurrence = cmd.Parameters.Add("O_FIRST_OCCURRENCE_UTC", OracleDbType.TimeStamp);
            firstOccurrence.Direction = ParameterDirection.Output;
            var lastOccurrence = cmd.Parameters.Add("O_LAST_OCCURRENCE_UTC", OracleDbType.TimeStamp);
            lastOccurrence.Direction = ParameterDirection.Output;
            var count = cmd.Parameters.Add("O_OCCURRENCE_COUNT", OracleDbType.Int32);
            count.Direction = ParameterDirection.Output;
            var emailSent = cmd.Parameters.Add("O_EMAIL_ALREADY_SENT", OracleDbType.Int32);
            emailSent.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return new SystemErrorRecord
                {
                ErrorId = ToLong(errorId.Value),
                ErrorReference = reference.Value?.ToString() ?? string.Empty,
                Fingerprint = fingerprint ?? string.Empty,
                IsFirstOccurrence = ToInt(first.Value) == 1,
                FirstOccurrenceUtc = ToDateTime(firstOccurrence.Value),
                LastOccurrenceUtc = ToDateTime(lastOccurrence.Value),
                OccurrenceCount = ToInt(count.Value),
                EmailAlreadySent = ToInt(emailSent.Value) == 1
                };
            }

        public void MarkSystemErrorEmailStatus(long errorId, bool emailSent)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.MARK_SYSTEM_ERROR_EMAIL";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ERROR_ID", OracleDbType.Int64).Value = errorId;
            cmd.Parameters.Add("P_EMAIL_SENT", OracleDbType.Int32).Value = emailSent ? 1 : 0;
            cmd.ExecuteNonQuery();
            }

        public SystemErrorNotificationRecipients GetSystemErrorNotificationRecipients()
            {
            var recipients = new SystemErrorNotificationRecipients();
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.GET_SYSTEM_ERROR_RECIPIENTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                var address = reader["EMAIL_ADDRESS"]?.ToString();
                if (string.IsNullOrWhiteSpace(address))
                    {
                    continue;
                    }

                var recipientType = reader["RECIPIENT_TYPE"]?.ToString();
                if (string.Equals(recipientType, "CC", StringComparison.OrdinalIgnoreCase))
                    {
                    recipients.CcRecipients.Add(address);
                    }
                else
                    {
                    recipients.ToRecipients.Add(address);
                    }
                }

            return recipients;
            }

        public List<SystemErrorSummaryModel> GetSystemErrors(SystemErrorFilter filter)
            {
            filter ??= new SystemErrorFilter();
            var result = new List<SystemErrorSummaryModel>();
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.GET_SYSTEM_ERRORS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            AddSystemErrorFilterParameters(cmd, filter);
            cmd.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.Add(MapSystemErrorSummary(reader));
                }

            return result;
            }

        public SystemErrorDetailModel GetSystemErrorDetail(long errorId)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.GET_SYSTEM_ERROR_DETAIL";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ERROR_ID", OracleDbType.Int64).Value = errorId;
            cmd.Parameters.Add("O_MASTER", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_HISTORY", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_STATUS_HISTORY", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            var detail = new SystemErrorDetailModel();
            if (reader.Read())
                {
                var summary = MapSystemErrorSummary(reader);
                detail.ErrorId = summary.ErrorId;
                detail.ErrorReference = summary.ErrorReference;
                detail.ResolutionStatus = summary.ResolutionStatus;
                detail.FirstOccurrenceUtc = summary.FirstOccurrenceUtc;
                detail.LastOccurrenceUtc = summary.LastOccurrenceUtc;
                detail.OccurrenceCount = summary.OccurrenceCount;
                detail.Module = summary.Module;
                detail.Controller = summary.Controller;
                detail.Action = summary.Action;
                detail.ApiPath = summary.ApiPath;
                detail.Ppno = summary.Ppno;
                detail.Role = summary.Role;
                detail.Entity = summary.Entity;
                detail.ErrorType = summary.ErrorType;
                detail.ErrorCode = summary.ErrorCode;
                detail.StoredProcedure = summary.StoredProcedure;
                detail.ClientIp = summary.ClientIp;
                detail.ResolvedBy = summary.ResolvedBy;
                detail.ResolvedOnUtc = summary.ResolvedOnUtc;
                detail.ResolutionRemarks = summary.ResolutionRemarks;
                detail.ErrorMessage = GetString(reader, "ERROR_MESSAGE");
                detail.TechnicalDetails = GetString(reader, "TECHNICAL_DETAILS");
                detail.EmailSent = string.Equals(GetString(reader, "EMAIL_SENT"), "Y", StringComparison.OrdinalIgnoreCase);
                detail.EmailSentUtc = GetNullableDateTime(reader, "EMAIL_SENT_UTC");
                }

            if (reader.NextResult())
                {
                while (reader.Read())
                    {
                    detail.History.Add(new SystemErrorHistoryModel
                        {
                        HistoryId = GetLong(reader, "HISTORY_ID"),
                        OccurredOnUtc = GetDateTime(reader, "OCCURRED_ON_UTC"),
                        TraceId = GetString(reader, "TRACE_ID"),
                        IpAddress = GetString(reader, "IP_ADDRESS"),
                        UserAgent = GetString(reader, "USER_AGENT"),
                        Ppno = GetString(reader, "PPNO"),
                        Role = GetString(reader, "ROLE_NAME"),
                        Entity = GetString(reader, "ENTITY_NAME"),
                        ApiPath = GetString(reader, "API_PATH"),
                        ErrorMessage = GetString(reader, "ERROR_MESSAGE")
                        });
                    }
                }

            if (reader.NextResult())
                {
                while (reader.Read())
                    {
                    detail.StatusHistory.Add(new SystemErrorStatusHistoryModel
                        {
                        ChangedOnUtc = GetDateTime(reader, "CHANGED_ON_UTC"),
                        OldStatus = GetString(reader, "OLD_STATUS"),
                        NewStatus = GetString(reader, "NEW_STATUS"),
                        ChangedByPpno = GetString(reader, "CHANGED_BY_PPNO"),
                        Remarks = GetString(reader, "REMARKS")
                        });
                    }
                }

            return detail;
            }

        public void ResolveSystemError(long errorId, string resolvedByPpno, string remarks)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.RESOLVE_SYSTEM_ERROR";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ERROR_ID", OracleDbType.Int64).Value = errorId;
            cmd.Parameters.Add("P_RESOLVED_BY_PPNO", OracleDbType.Varchar2).Value = DbValue(resolvedByPpno, 50);
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2).Value = DbValue(remarks, 1000);
            cmd.ExecuteNonQuery();
            }

        private static void AddSystemErrorFilterParameters(OracleCommand cmd, SystemErrorFilter filter)
            {
            cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = DbValue(filter.Status, 30);
            cmd.Parameters.Add("P_FROM_DATE", OracleDbType.TimeStamp).Value = filter.FromDate ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_TO_DATE", OracleDbType.TimeStamp).Value = filter.ToDate ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_ERROR_REFERENCE", OracleDbType.Varchar2).Value = DbValue(filter.ErrorReference, 50);
            cmd.Parameters.Add("P_MODULE", OracleDbType.Varchar2).Value = DbValue(filter.Module, 100);
            cmd.Parameters.Add("P_USER_PPNO", OracleDbType.Varchar2).Value = DbValue(filter.UserPpno, 50);
            cmd.Parameters.Add("P_ENTITY", OracleDbType.Varchar2).Value = DbValue(filter.Entity, 300);
            cmd.Parameters.Add("P_ERROR_TYPE_CODE", OracleDbType.Varchar2).Value = DbValue(filter.ErrorTypeOrCode, 300);
            }

        private static SystemErrorSummaryModel MapSystemErrorSummary(OracleDataReader reader)
            {
            return new SystemErrorSummaryModel
                {
                ErrorId = GetLong(reader, "ERROR_ID"),
                ErrorReference = GetString(reader, "ERROR_REFERENCE"),
                ResolutionStatus = GetString(reader, "RESOLUTION_STATUS"),
                FirstOccurrenceUtc = GetDateTime(reader, "FIRST_OCCURRENCE_UTC"),
                LastOccurrenceUtc = GetDateTime(reader, "LAST_OCCURRENCE_UTC"),
                OccurrenceCount = GetInt(reader, "OCCURRENCE_COUNT"),
                Module = GetString(reader, "MODULE"),
                Controller = GetString(reader, "CONTROLLER"),
                Action = GetString(reader, "ACTION"),
                ApiPath = GetString(reader, "API_PATH"),
                Ppno = GetString(reader, "PPNO"),
                Role = GetString(reader, "ROLE_NAME"),
                Entity = GetString(reader, "ENTITY_NAME"),
                ErrorType = GetString(reader, "ERROR_TYPE"),
                ErrorCode = GetString(reader, "ERROR_CODE"),
                StoredProcedure = GetString(reader, "STORED_PROCEDURE"),
                ClientIp = GetString(reader, "LAST_IP_ADDRESS"),
                ResolvedBy = GetString(reader, "RESOLVED_BY"),
                ResolvedOnUtc = GetNullableDateTime(reader, "RESOLVED_ON_UTC"),
                ResolutionRemarks = GetString(reader, "RESOLUTION_REMARKS")
                };
            }

        private static string GetString(OracleDataReader reader, string column)
            {
            return reader[column] == DBNull.Value ? string.Empty : reader[column]?.ToString() ?? string.Empty;
            }

        private static int GetInt(OracleDataReader reader, string column)
            {
            return reader[column] == DBNull.Value ? 0 : Convert.ToInt32(reader[column]);
            }

        private static long GetLong(OracleDataReader reader, string column)
            {
            return reader[column] == DBNull.Value ? 0 : Convert.ToInt64(reader[column]);
            }

        private static DateTime GetDateTime(OracleDataReader reader, string column)
            {
            return reader[column] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader[column]);
            }

        private static DateTime? GetNullableDateTime(OracleDataReader reader, string column)
            {
            return reader[column] == DBNull.Value ? null : Convert.ToDateTime(reader[column]);
            }

        private static long ToLong(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return 0;
                }

            return value is OracleDecimal oracleDecimal ? Convert.ToInt64(oracleDecimal.Value) : Convert.ToInt64(value);
            }

        private static int ToInt(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return 0;
                }

            return value is OracleDecimal oracleDecimal ? Convert.ToInt32(oracleDecimal.Value) : Convert.ToInt32(value);
            }

        private static DateTime ToDateTime(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return DateTime.MinValue;
                }

            if (value is OracleTimeStamp oracleTimeStamp)
                {
                return oracleTimeStamp.Value;
                }

            return Convert.ToDateTime(value);
            }
        }
    }

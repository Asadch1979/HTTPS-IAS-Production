using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

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

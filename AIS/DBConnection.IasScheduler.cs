using AIS.Models.Scheduler;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        public IasSchedulerClaimedJob ClaimIasSchedulerDueJob()
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_CLAIM_DUE_JOB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapIasSchedulerClaimedJob(reader) : null;
        }

        public void CompleteIasSchedulerJob(long executionId, int recordsProcessed, string responseMessage)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_COMPLETE_JOB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_EXECUTION_ID", OracleDbType.Int64).Value = executionId;
            cmd.Parameters.Add("P_RECORDS_PROCESSED", OracleDbType.Int32).Value = recordsProcessed;
            cmd.Parameters.Add("P_RESPONSE_MESSAGE", OracleDbType.Varchar2, 2000).Value =
                string.IsNullOrWhiteSpace(responseMessage) ? DBNull.Value : responseMessage;
            cmd.ExecuteNonQuery();
        }

        public void FailIasSchedulerJob(long executionId, string errorMessage)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_FAIL_JOB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_EXECUTION_ID", OracleDbType.Int64).Value = executionId;
            cmd.Parameters.Add("P_ERROR_MESSAGE", OracleDbType.Varchar2, 4000).Value =
                string.IsNullOrWhiteSpace(errorMessage) ? "Application scheduler handler failed." : errorMessage;
            cmd.ExecuteNonQuery();
        }

        private static IasSchedulerClaimedJob MapIasSchedulerClaimedJob(OracleDataReader reader)
        {
            return new IasSchedulerClaimedJob
            {
                ExecutionId = ReadIasSchedulerInt64(reader, "EXECUTION_ID"),
                ScheduleId = ReadIasSchedulerInt64(reader, "SCHEDULE_ID"),
                JobId = ReadIasSchedulerInt64(reader, "JOB_ID"),
                JobCode = ReadIasSchedulerString(reader, "JOB_CODE"),
                JobName = ReadIasSchedulerString(reader, "JOB_NAME"),
                ExecutionType = ReadIasSchedulerString(reader, "EXECUTION_TYPE"),
                PackageName = ReadIasSchedulerString(reader, "PACKAGE_NAME"),
                ProcedureName = ReadIasSchedulerString(reader, "PROCEDURE_NAME"),
                ApplicationHandler = ReadIasSchedulerString(reader, "APPLICATION_HANDLER"),
                EmailRequired = string.Equals(ReadIasSchedulerString(reader, "EMAIL_REQUIRED"), "Y", StringComparison.OrdinalIgnoreCase),
                ExecutionKey = ReadIasSchedulerString(reader, "EXECUTION_KEY"),
                ScheduledFor = ReadIasSchedulerTimestampTz(reader, "SCHEDULED_FOR"),
                PeriodFrom = ReadIasSchedulerNullableDate(reader, "PERIOD_FROM"),
                PeriodTo = ReadIasSchedulerNullableDate(reader, "PERIOD_TO"),
                RetryNo = ReadIasSchedulerInt32(reader, "RETRY_NO")
            };
        }

        private static long ReadIasSchedulerInt64(OracleDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? 0 : Convert.ToInt64(reader[columnName]);
        }

        private static int ReadIasSchedulerInt32(OracleDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? 0 : Convert.ToInt32(reader[columnName]);
        }

        private static string ReadIasSchedulerString(OracleDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? string.Empty : reader[columnName].ToString();
        }

        private static DateTime? ReadIasSchedulerNullableDate(OracleDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : Convert.ToDateTime(reader[columnName]);
        }

        private static DateTime ReadIasSchedulerTimestampTz(OracleDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            var timestamp = reader.GetOracleTimeStampTZ(ordinal);
            return timestamp.IsNull
                ? DateTime.MinValue
                : DateTime.SpecifyKind(timestamp.Value, DateTimeKind.Local);
        }
    }
}

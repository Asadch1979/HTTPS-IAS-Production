using AIS.Models.Scheduler;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        public List<IasSchedulerJobDefinition> GetIasSchedulerJobDefinitions()
        {
            return ExecuteIasSchedulerCursor("PKG_IAS_SCHEDULER.P_GET_JOB_DEFINITIONS", MapIasSchedulerJobDefinition);
        }

        public List<IasSchedulerSchedule> GetIasSchedulerSchedules()
        {
            return ExecuteIasSchedulerCursor("PKG_IAS_SCHEDULER.P_GET_SCHEDULES", MapIasSchedulerSchedule);
        }

        public List<IasSchedulerExecutionHistory> GetIasSchedulerExecutionHistory(
            long? scheduleId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_GET_EXECUTION_HISTORY";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_SCHEDULE_ID", OracleDbType.Int64).Value = scheduleId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate ?? (object)DBNull.Value;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            return ReadIasSchedulerCursor(cmd, MapIasSchedulerExecutionHistory);
        }

        public List<IasSchedulerRunRequest> GetIasSchedulerRunRequests(long? scheduleId = null)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_GET_RUN_REQUESTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_SCHEDULE_ID", OracleDbType.Int64).Value = scheduleId ?? (object)DBNull.Value;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            return ReadIasSchedulerCursor(cmd, MapIasSchedulerRunRequest);
        }

        public List<IasSchedulerReconciliationExecution> GetIasSchedulerReconciliationRequired()
        {
            return ExecuteIasSchedulerCursor(
                "PKG_IAS_SCHEDULER.P_GET_RECONCILIATION_REQUIRED",
                MapIasSchedulerReconciliationExecution);
        }

        public long SaveIasSchedulerJobDefinition(SaveIasSchedulerJobDefinitionRequest model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_SAVE_JOB_DEFINITION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_JOB_ID", OracleDbType.Int64).Value = model.JobId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_JOB_CODE", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(model.JobCode, 100);
            cmd.Parameters.Add("P_JOB_NAME", OracleDbType.Varchar2, 200).Value = IasSchedulerDbValue(model.JobName, 200);
            cmd.Parameters.Add("P_EXECUTION_TYPE", OracleDbType.Varchar2, 20).Value = IasSchedulerDbValue(model.ExecutionType, 20);
            cmd.Parameters.Add("P_PACKAGE_NAME", OracleDbType.Varchar2, 128).Value = IasSchedulerDbValue(model.PackageName, 128);
            cmd.Parameters.Add("P_PROCEDURE_NAME", OracleDbType.Varchar2, 128).Value = IasSchedulerDbValue(model.ProcedureName, 128);
            cmd.Parameters.Add("P_APPLICATION_HANDLER", OracleDbType.Varchar2, 150).Value = IasSchedulerDbValue(model.ApplicationHandler, 150);
            cmd.Parameters.Add("P_DESCRIPTION", OracleDbType.Varchar2, 1000).Value = IasSchedulerDbValue(model.Description, 1000);
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(model.UpdatedBy, 100);
            var savedId = cmd.Parameters.Add("P_SAVED_JOB_ID", OracleDbType.Int64);
            savedId.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return ReadIasSchedulerOutputInt64(savedId);
        }

        public long SaveIasSchedulerSchedule(SaveIasSchedulerScheduleRequest model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_SAVE_SCHEDULE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_SCHEDULE_ID", OracleDbType.Int64).Value = model.ScheduleId ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_JOB_ID", OracleDbType.Int64).Value = model.JobId;
            cmd.Parameters.Add("P_FREQUENCY_TYPE", OracleDbType.Varchar2, 20).Value = IasSchedulerDbValue(model.FrequencyType, 20);
            cmd.Parameters.Add("P_RUN_HOUR", OracleDbType.Int32).Value = model.RunHour;
            cmd.Parameters.Add("P_RUN_MINUTE", OracleDbType.Int32).Value = model.RunMinute;
            cmd.Parameters.Add("P_DAY_OF_WEEK", OracleDbType.Int32).Value = model.DayOfWeek ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_DAY_OF_MONTH", OracleDbType.Int32).Value = model.DayOfMonth ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_PERIOD_TYPE", OracleDbType.Varchar2, 30).Value = IasSchedulerDbValue(model.PeriodType, 30);
            cmd.Parameters.Add("P_EMAIL_REQUIRED", OracleDbType.Char, 1).Value = model.EmailRequired ? "Y" : "N";
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Char, 1).Value = model.IsActive ? "Y" : "N";
            cmd.Parameters.Add("P_EFFECTIVE_FROM", OracleDbType.Date).Value = model.EffectiveFrom;
            cmd.Parameters.Add("P_EFFECTIVE_TO", OracleDbType.Date).Value = model.EffectiveTo ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_MAX_RETRY", OracleDbType.Int32).Value = model.MaxRetry;
            cmd.Parameters.Add("P_RETRY_INTERVAL_MINUTES", OracleDbType.Int32).Value = model.RetryIntervalMinutes;
            cmd.Parameters.Add("P_NEXT_DUE_ON", OracleDbType.TimeStampTZ).Value =
                model.NextDueOn.HasValue ? ToIasSchedulerOffset(model.NextDueOn.Value) : (object)DBNull.Value;
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2, 1000).Value = IasSchedulerDbValue(model.Remarks, 1000);
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(model.UpdatedBy, 100);
            var savedId = cmd.Parameters.Add("P_SAVED_SCHEDULE_ID", OracleDbType.Int64);
            savedId.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return ReadIasSchedulerOutputInt64(savedId);
        }

        public void SetIasSchedulerScheduleActive(long scheduleId, bool isActive, string updatedBy)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_SET_ACTIVE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_SCHEDULE_ID", OracleDbType.Int64).Value = scheduleId;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Char, 1).Value = isActive ? "Y" : "N";
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(updatedBy, 100);
            cmd.ExecuteNonQuery();
        }

        public long RequestIasSchedulerRunNow(
            long scheduleId,
            DateTime? periodFrom,
            DateTime? periodTo,
            string requestedBy,
            string remarks = null)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_REQUEST_RUN_NOW";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_SCHEDULE_ID", OracleDbType.Int64).Value = scheduleId;
            cmd.Parameters.Add("P_PERIOD_FROM", OracleDbType.Date).Value = periodFrom ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_PERIOD_TO", OracleDbType.Date).Value = periodTo ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_REQUESTED_BY", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(requestedBy, 100);
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2, 1000).Value = IasSchedulerDbValue(remarks, 1000);
            var requestId = cmd.Parameters.Add("P_REQUEST_ID", OracleDbType.Int64);
            requestId.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return ReadIasSchedulerOutputInt64(requestId);
        }

        public void CancelIasSchedulerRunRequest(long requestId, string updatedBy)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_CANCEL_RUN_REQUEST";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_REQUEST_ID", OracleDbType.Int64).Value = requestId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(updatedBy, 100);
            cmd.ExecuteNonQuery();
        }

        public void FlagStaleIasSchedulerExecutions(int staleMinutes)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_FLAG_STALE_EXECUTIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_STALE_MINUTES", OracleDbType.Int32).Value = staleMinutes;
            cmd.ExecuteNonQuery();
        }

        public void ResolveIasSchedulerExecution(
            long executionId,
            string resolution,
            int? recordsProcessed,
            string message,
            string updatedBy)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_IAS_SCHEDULER.P_RESOLVE_EXECUTION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_EXECUTION_ID", OracleDbType.Int64).Value = executionId;
            cmd.Parameters.Add("P_RESOLUTION", OracleDbType.Varchar2, 20).Value = IasSchedulerDbValue(resolution, 20);
            cmd.Parameters.Add("P_RECORDS_PROCESSED", OracleDbType.Int32).Value = recordsProcessed ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_MESSAGE", OracleDbType.Varchar2, 4000).Value = IasSchedulerDbValue(message, 4000);
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = IasSchedulerDbValue(updatedBy, 100);
            cmd.ExecuteNonQuery();
        }

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
                string.IsNullOrWhiteSpace(errorMessage) ? "IAS scheduler handler failed." : errorMessage;
            cmd.ExecuteNonQuery();
        }

        private List<T> ExecuteIasSchedulerCursor<T>(string procedureName, Func<OracleDataReader, T> map)
        {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            return ReadIasSchedulerCursor(cmd, map);
        }

        private static List<T> ReadIasSchedulerCursor<T>(OracleCommand cmd, Func<OracleDataReader, T> map)
        {
            var result = new List<T>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(map(reader));
            }

            return result;
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
                EmailRequired = ReadIasSchedulerFlag(reader, "EMAIL_REQUIRED"),
                ExecutionKey = ReadIasSchedulerString(reader, "EXECUTION_KEY"),
                ScheduledFor = ReadIasSchedulerTimestampTz(reader, "SCHEDULED_FOR"),
                PeriodFrom = ReadIasSchedulerNullableDate(reader, "PERIOD_FROM"),
                PeriodTo = ReadIasSchedulerNullableDate(reader, "PERIOD_TO"),
                RetryNo = ReadIasSchedulerInt32(reader, "RETRY_NO"),
                RunSource = ReadIasSchedulerString(reader, "RUN_SOURCE", "SCHEDULED"),
                RunRequestId = ReadIasSchedulerNullableInt64(reader, "RUN_REQUEST_ID")
            };
        }

        private static IasSchedulerJobDefinition MapIasSchedulerJobDefinition(OracleDataReader reader)
        {
            return new IasSchedulerJobDefinition
            {
                JobId = ReadIasSchedulerInt64(reader, "JOB_ID"),
                JobCode = ReadIasSchedulerString(reader, "JOB_CODE"),
                JobName = ReadIasSchedulerString(reader, "JOB_NAME"),
                ExecutionType = ReadIasSchedulerString(reader, "EXECUTION_TYPE"),
                PackageName = ReadIasSchedulerString(reader, "PACKAGE_NAME"),
                ProcedureName = ReadIasSchedulerString(reader, "PROCEDURE_NAME"),
                ApplicationHandler = ReadIasSchedulerString(reader, "APPLICATION_HANDLER"),
                Description = ReadIasSchedulerString(reader, "DESCRIPTION"),
                CreatedBy = ReadIasSchedulerString(reader, "CREATED_BY"),
                CreatedOn = ReadIasSchedulerNullableDate(reader, "CREATED_ON"),
                UpdatedBy = ReadIasSchedulerString(reader, "UPDATED_BY"),
                UpdatedOn = ReadIasSchedulerNullableDate(reader, "UPDATED_ON"),
                ScheduleCount = ReadIasSchedulerInt32(reader, "SCHEDULE_COUNT")
            };
        }

        private static IasSchedulerSchedule MapIasSchedulerSchedule(OracleDataReader reader)
        {
            return new IasSchedulerSchedule
            {
                JobId = ReadIasSchedulerInt64(reader, "JOB_ID"),
                JobCode = ReadIasSchedulerString(reader, "JOB_CODE"),
                JobName = ReadIasSchedulerString(reader, "JOB_NAME"),
                ExecutionType = ReadIasSchedulerString(reader, "EXECUTION_TYPE"),
                PackageName = ReadIasSchedulerString(reader, "PACKAGE_NAME"),
                ProcedureName = ReadIasSchedulerString(reader, "PROCEDURE_NAME"),
                ApplicationHandler = ReadIasSchedulerString(reader, "APPLICATION_HANDLER"),
                Description = ReadIasSchedulerString(reader, "DESCRIPTION"),
                ScheduleId = ReadIasSchedulerInt64(reader, "SCHEDULE_ID"),
                FrequencyType = ReadIasSchedulerString(reader, "FREQUENCY_TYPE"),
                RunHour = ReadIasSchedulerInt32(reader, "RUN_HOUR"),
                RunMinute = ReadIasSchedulerInt32(reader, "RUN_MINUTE"),
                DayOfWeek = ReadIasSchedulerNullableInt32(reader, "DAY_OF_WEEK"),
                DayOfMonth = ReadIasSchedulerNullableInt32(reader, "DAY_OF_MONTH"),
                PeriodType = ReadIasSchedulerString(reader, "PERIOD_TYPE"),
                EmailRequired = ReadIasSchedulerFlag(reader, "EMAIL_REQUIRED"),
                IsActive = ReadIasSchedulerFlag(reader, "IS_ACTIVE"),
                EffectiveFrom = ReadIasSchedulerNullableDate(reader, "EFFECTIVE_FROM"),
                EffectiveTo = ReadIasSchedulerNullableDate(reader, "EFFECTIVE_TO"),
                LastRunOn = ReadIasSchedulerNullableTimestampTz(reader, "LAST_RUN_ON"),
                LastStatus = ReadIasSchedulerString(reader, "LAST_STATUS"),
                LastPeriodFrom = ReadIasSchedulerNullableDate(reader, "LAST_PERIOD_FROM"),
                LastPeriodTo = ReadIasSchedulerNullableDate(reader, "LAST_PERIOD_TO"),
                NextDueOn = ReadIasSchedulerNullableTimestampTz(reader, "NEXT_DUE_ON"),
                RetryCount = ReadIasSchedulerInt32(reader, "RETRY_COUNT"),
                MaxRetry = ReadIasSchedulerInt32(reader, "MAX_RETRY"),
                RetryIntervalMinutes = ReadIasSchedulerInt32(reader, "RETRY_INTERVAL_MINUTES"),
                LastError = ReadIasSchedulerString(reader, "LAST_ERROR"),
                Remarks = ReadIasSchedulerString(reader, "REMARKS"),
                CreatedBy = ReadIasSchedulerString(reader, "CREATED_BY"),
                CreatedOn = ReadIasSchedulerNullableDate(reader, "CREATED_ON"),
                UpdatedBy = ReadIasSchedulerString(reader, "UPDATED_BY"),
                UpdatedOn = ReadIasSchedulerNullableDate(reader, "UPDATED_ON"),
                IsDue = ReadIasSchedulerFlag(reader, "IS_DUE"),
                OpenRunRequests = ReadIasSchedulerInt32(reader, "OPEN_RUN_REQUESTS")
            };
        }

        private static IasSchedulerExecutionHistory MapIasSchedulerExecutionHistory(OracleDataReader reader)
        {
            return new IasSchedulerExecutionHistory
            {
                ExecutionId = ReadIasSchedulerInt64(reader, "EXECUTION_ID"),
                ScheduleId = ReadIasSchedulerInt64(reader, "SCHEDULE_ID"),
                JobId = ReadIasSchedulerInt64(reader, "JOB_ID"),
                JobCode = ReadIasSchedulerString(reader, "JOB_CODE"),
                JobName = ReadIasSchedulerString(reader, "JOB_NAME"),
                ExecutionType = ReadIasSchedulerString(reader, "EXECUTION_TYPE"),
                RunSource = ReadIasSchedulerString(reader, "RUN_SOURCE"),
                RunRequestId = ReadIasSchedulerNullableInt64(reader, "RUN_REQUEST_ID"),
                ExecutionKey = ReadIasSchedulerString(reader, "EXECUTION_KEY"),
                ScheduledFor = ReadIasSchedulerNullableTimestampTz(reader, "SCHEDULED_FOR"),
                PeriodFrom = ReadIasSchedulerNullableDate(reader, "PERIOD_FROM"),
                PeriodTo = ReadIasSchedulerNullableDate(reader, "PERIOD_TO"),
                Status = ReadIasSchedulerString(reader, "STATUS"),
                StartedOn = ReadIasSchedulerNullableTimestampTz(reader, "STARTED_ON"),
                CompletedOn = ReadIasSchedulerNullableTimestampTz(reader, "COMPLETED_ON"),
                RetryNo = ReadIasSchedulerInt32(reader, "RETRY_NO"),
                RecordsProcessed = ReadIasSchedulerNullableInt32(reader, "RECORDS_PROCESSED"),
                ResponseMessage = ReadIasSchedulerString(reader, "RESPONSE_MESSAGE"),
                ErrorMessage = ReadIasSchedulerString(reader, "ERROR_MESSAGE"),
                CreatedOn = ReadIasSchedulerNullableTimestampTz(reader, "CREATED_ON")
            };
        }

        private static IasSchedulerRunRequest MapIasSchedulerRunRequest(OracleDataReader reader)
        {
            return new IasSchedulerRunRequest
            {
                RequestId = ReadIasSchedulerInt64(reader, "REQUEST_ID"),
                RunRequestId = ReadIasSchedulerInt64(reader, "REQUEST_ID"),
                ScheduleId = ReadIasSchedulerInt64(reader, "SCHEDULE_ID"),
                JobId = ReadIasSchedulerInt64(reader, "JOB_ID"),
                JobCode = ReadIasSchedulerString(reader, "JOB_CODE"),
                JobName = ReadIasSchedulerString(reader, "JOB_NAME"),
                RequestedBy = ReadIasSchedulerString(reader, "REQUESTED_BY"),
                RequestedOn = ReadIasSchedulerNullableTimestampTz(reader, "REQUESTED_ON"),
                PeriodFrom = ReadIasSchedulerNullableDate(reader, "PERIOD_FROM"),
                PeriodTo = ReadIasSchedulerNullableDate(reader, "PERIOD_TO"),
                Status = ReadIasSchedulerString(reader, "STATUS"),
                ExecutionId = ReadIasSchedulerNullableInt64(reader, "EXECUTION_ID"),
                Remarks = ReadIasSchedulerString(reader, "REMARKS"),
                UpdatedBy = ReadIasSchedulerString(reader, "UPDATED_BY"),
                UpdatedOn = ReadIasSchedulerNullableDate(reader, "UPDATED_ON")
            };
        }

        private static IasSchedulerReconciliationExecution MapIasSchedulerReconciliationExecution(OracleDataReader reader)
        {
            return new IasSchedulerReconciliationExecution
            {
                ExecutionId = ReadIasSchedulerInt64(reader, "EXECUTION_ID"),
                ScheduleId = ReadIasSchedulerInt64(reader, "SCHEDULE_ID"),
                JobId = ReadIasSchedulerInt64(reader, "JOB_ID"),
                JobCode = ReadIasSchedulerString(reader, "JOB_CODE"),
                JobName = ReadIasSchedulerString(reader, "JOB_NAME"),
                RunSource = ReadIasSchedulerString(reader, "RUN_SOURCE"),
                RunRequestId = ReadIasSchedulerNullableInt64(reader, "RUN_REQUEST_ID"),
                ExecutionKey = ReadIasSchedulerString(reader, "EXECUTION_KEY"),
                ScheduledFor = ReadIasSchedulerNullableTimestampTz(reader, "SCHEDULED_FOR"),
                PeriodFrom = ReadIasSchedulerNullableDate(reader, "PERIOD_FROM"),
                PeriodTo = ReadIasSchedulerNullableDate(reader, "PERIOD_TO"),
                StartedOn = ReadIasSchedulerNullableTimestampTz(reader, "STARTED_ON"),
                ErrorMessage = ReadIasSchedulerString(reader, "ERROR_MESSAGE"),
                LastError = ReadIasSchedulerString(reader, "ERROR_MESSAGE")
            };
        }

        private static bool HasIasSchedulerColumn(OracleDataReader reader, string columnName)
        {
            for (var index = 0; index < reader.FieldCount; index++)
            {
                if (string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static long ReadIasSchedulerInt64(OracleDataReader reader, string columnName)
        {
            return ReadIasSchedulerNullableInt64(reader, columnName) ?? 0;
        }

        private static long? ReadIasSchedulerNullableInt64(OracleDataReader reader, string columnName)
        {
            return !HasIasSchedulerColumn(reader, columnName) || reader[columnName] == DBNull.Value
                ? null
                : Convert.ToInt64(reader[columnName]);
        }

        private static int ReadIasSchedulerInt32(OracleDataReader reader, string columnName)
        {
            return ReadIasSchedulerNullableInt32(reader, columnName) ?? 0;
        }

        private static int? ReadIasSchedulerNullableInt32(OracleDataReader reader, string columnName)
        {
            return !HasIasSchedulerColumn(reader, columnName) || reader[columnName] == DBNull.Value
                ? null
                : Convert.ToInt32(reader[columnName]);
        }

        private static string ReadIasSchedulerString(OracleDataReader reader, string columnName, string defaultValue = "")
        {
            return !HasIasSchedulerColumn(reader, columnName) || reader[columnName] == DBNull.Value
                ? defaultValue
                : reader[columnName].ToString();
        }

        private static bool ReadIasSchedulerFlag(OracleDataReader reader, string columnName)
        {
            return string.Equals(ReadIasSchedulerString(reader, columnName), "Y", StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime? ReadIasSchedulerNullableDate(OracleDataReader reader, string columnName)
            {
            if (!HasIasSchedulerColumn(reader, columnName) || reader[columnName] == DBNull.Value)
                {
                return null;
                }

            var value = reader[columnName];

            if (value is DateTime dateTime)
                {
                return dateTime;
                }

            if (value is DateTimeOffset dateTimeOffset)
                {
                return dateTimeOffset.DateTime;
                }

            return Convert.ToDateTime(value);
            }
        private static DateTime ReadIasSchedulerTimestampTz(OracleDataReader reader, string columnName)
        {
            return ReadIasSchedulerNullableTimestampTz(reader, columnName) ?? DateTime.MinValue;
        }

        private static DateTime? ReadIasSchedulerNullableTimestampTz(OracleDataReader reader, string columnName)
        {
            if (!HasIasSchedulerColumn(reader, columnName) || reader[columnName] == DBNull.Value)
            {
                return null;
            }

            var ordinal = reader.GetOrdinal(columnName);
            var timestamp = reader.GetOracleTimeStampTZ(ordinal);
            return timestamp.IsNull ? null : DateTime.SpecifyKind(timestamp.Value, DateTimeKind.Local);
        }

        private static object IasSchedulerDbValue(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DBNull.Value;
            }

            value = value.Trim();
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static DateTimeOffset ToIasSchedulerOffset(DateTime value)
        {
            var local = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Local)
                : value;
            return new DateTimeOffset(local);
        }

        private static long ReadIasSchedulerOutputInt64(OracleParameter parameter)
        {
            if (parameter.Value == null || parameter.Value == DBNull.Value)
            {
                return 0;
            }

            if (parameter.Value is OracleDecimal oracleDecimal)
            {
                return oracleDecimal.ToInt64();
            }

            return Convert.ToInt64(parameter.Value.ToString());
        }
    }
}

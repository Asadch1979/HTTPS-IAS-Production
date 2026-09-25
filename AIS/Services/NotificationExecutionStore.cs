using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace AIS.Services
{
    // A committed claim survives HTTP retries and application restarts. Unknown
    // outcomes remain RUNNING for reconciliation, never blindly re-executed.
    public sealed class NotificationExecutionStore
    {
        public sealed record ExecutionRecord(string Fingerprint, string Status, string Response);

        private readonly IConfiguration configuration;
        public NotificationExecutionStore(IConfiguration configuration) => this.configuration = configuration;
        public OracleConnection Open()
        {
            var connection = new OracleConnection(new OracleConnectionStringBuilder
            {
                UserID = configuration["ConnectionStrings:DBUserName"],
                Password = configuration["ConnectionStrings:DBUserPassword"],
                DataSource = configuration["ConnectionStrings:DBDataSource"]
            }.ConnectionString);
            connection.Open();
            return connection;
        }

        public bool Claim(string key, string fingerprint)
        {
            using var connection = Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.BindByName = true;
            command.CommandText = "INSERT INTO T_IAS_NOTIFY_EXECUTION (EXECUTION_KEY,FINGERPRINT,STATUS,CREATED_ON) VALUES (:k,:f,'RUNNING',SYSTIMESTAMP)";
            command.Parameters.Add("k", OracleDbType.Varchar2).Value = key;
            command.Parameters.Add("f", OracleDbType.Varchar2).Value = fingerprint;
            try { command.ExecuteNonQuery(); transaction.Commit(); return true; }
            catch (OracleException e) when (e.Number == 1) { transaction.Rollback(); return false; }
        }

        public void Complete(string key, string status, string response)
        {
            using var connection = Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.BindByName = true;
            command.CommandText = "UPDATE T_IAS_NOTIFY_EXECUTION SET STATUS=:s,RESPONSE=:r,UPDATED_ON=SYSTIMESTAMP WHERE EXECUTION_KEY=:k";
            command.Parameters.Add("s", OracleDbType.Varchar2).Value = status;
            command.Parameters.Add("r", OracleDbType.Clob).Value = response ?? "";
            command.Parameters.Add("k", OracleDbType.Varchar2).Value = key;
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public bool ClaimWeekly(string key)
        {
            if (Claim(key, "WEEKLY")) return true;
            using var connection = Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.BindByName = true;
            command.CommandText = @"UPDATE T_IAS_NOTIFY_EXECUTION SET STATUS='RUNNING',
                RETRY_COUNT=RETRY_COUNT+1,UPDATED_ON=SYSTIMESTAMP WHERE EXECUTION_KEY=:k
                AND STATUS='FAILED' AND RETRY_COUNT<5
                AND UPDATED_ON<SYSTIMESTAMP-INTERVAL '15' MINUTE";
            command.Parameters.Add("k", OracleDbType.Varchar2).Value = key;
            var claimed = command.ExecuteNonQuery() == 1;
            transaction.Commit();
            return claimed;
        }

        public string ExecuteReview(string requestId, string fingerprint, Func<string> action)
        {
            var key = "REVIEW:" + requestId;
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint)));
            return ExecuteReviewCore(key, hash, action, Claim, Complete, ReadExecution);
        }

        public static string ExecuteReviewCore(
            string key,
            string hash,
            Func<string> action,
            Func<string, string, bool> claim,
            Action<string, string, string> complete,
            Func<string, ExecutionRecord> readExecution)
        {
            if (claim(key, hash))
            {
                var response = action();
                // The business operation has committed; tracking failure must
                // not convert it to another business execution on retry.
                try { complete(key, "COMPLETE", response); }
                catch (Exception e) { Console.Error.WriteLine($"Review tracking failed: {key}: {e}"); }
                return response;
            }
            var existing = readExecution(key);
            if (existing != null && existing.Fingerprint == hash && existing.Status == "COMPLETE")
                return string.IsNullOrEmpty(existing.Response) ? JsonSerializer.Serialize(new { Status = false, Message = "This request was already completed." }) : existing.Response;
            return JsonSerializer.Serialize(new { Status = false, Message = "This request is already processing or requires reconciliation. Refresh before taking another action." });
        }

        private ExecutionRecord ReadExecution(string key)
        {
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandText = "SELECT FINGERPRINT,STATUS,RESPONSE FROM T_IAS_NOTIFY_EXECUTION WHERE EXECUTION_KEY=:k";
            command.Parameters.Add("k", OracleDbType.Varchar2).Value = key;
            using var reader = command.ExecuteReader();
            return reader.Read()
                ? new ExecutionRecord(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2))
                : null;
        }
    }
}

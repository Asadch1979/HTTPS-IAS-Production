using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public EmailConfigurationRecord GetActiveEmailConfiguration()
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL_CONFIGURATION.GET_ACTIVE_CONFIGURATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadEmailConfiguration(reader) : null;
            }

        public EmailConfigurationRecord GetEmailConfiguration(long configId)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL_CONFIGURATION.GET_CONFIGURATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_CONFIG_ID", OracleDbType.Int64).Value = configId;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadEmailConfiguration(reader) : null;
            }

        public long SaveEmailConfiguration(EmailConfigurationRecord record, string changedBy, string clientIp)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_EMAIL_CONFIGURATION.UPSERT_CONFIGURATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            var id = cmd.Parameters.Add("P_CONFIG_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.InputOutput;
            id.Value = record.ConfigId > 0 ? record.ConfigId : DBNull.Value;
            cmd.Parameters.Add("P_SMTP_HOST", OracleDbType.Varchar2).Value = record.SmtpHost;
            cmd.Parameters.Add("P_SMTP_PORT", OracleDbType.Int32).Value = record.SmtpPort;
            cmd.Parameters.Add("P_FROM_EMAIL", OracleDbType.Varchar2).Value = record.FromEmail;
            cmd.Parameters.Add("P_SMTP_USERNAME", OracleDbType.Varchar2).Value = DbNull(record.SmtpUsername);
            cmd.Parameters.Add("P_ENCRYPTED_PASSWORD", OracleDbType.Raw).Value = record.EncryptedPassword?.Length > 0 ? record.EncryptedPassword : DBNull.Value;
            cmd.Parameters.Add("P_ENABLE_SSL", OracleDbType.Char).Value = record.EnableSsl ? "Y" : "N";
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Char).Value = record.IsActive ? "Y" : "N";
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2).Value = DbNull(changedBy);
            cmd.Parameters.Add("P_CLIENT_IP", OracleDbType.Varchar2).Value = DbNull(clientIp);
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(id.Value.ToString());
            }

        public void SetEmailConfigurationActive(long configId, bool active, string changedBy, string clientIp)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = active
                ? "PKG_EMAIL_CONFIGURATION.ACTIVATE_CONFIGURATION"
                : "PKG_EMAIL_CONFIGURATION.DEACTIVATE_CONFIGURATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_CONFIG_ID", OracleDbType.Int64).Value = configId;
            cmd.Parameters.Add("P_CHANGED_BY", OracleDbType.Varchar2).Value = DbNull(changedBy);
            cmd.Parameters.Add("P_CLIENT_IP", OracleDbType.Varchar2).Value = DbNull(clientIp);
            cmd.ExecuteNonQuery();
            }

        private static EmailConfigurationRecord ReadEmailConfiguration(OracleDataReader reader)
            {
            return new EmailConfigurationRecord
                {
                ConfigId = Convert.ToInt64(reader["CONFIG_ID"]),
                SmtpHost = reader["SMTP_HOST"]?.ToString() ?? string.Empty,
                SmtpPort = Convert.ToInt32(reader["SMTP_PORT"]),
                FromEmail = reader["FROM_EMAIL"]?.ToString() ?? string.Empty,
                SmtpUsername = reader["SMTP_USERNAME"]?.ToString() ?? string.Empty,
                EncryptedPassword = reader["ENCRYPTED_PASSWORD"] == DBNull.Value ? Array.Empty<byte>() : (byte[])reader["ENCRYPTED_PASSWORD"],
                EnableSsl = string.Equals(reader["ENABLE_SSL"]?.ToString(), "Y", StringComparison.OrdinalIgnoreCase),
                IsActive = string.Equals(reader["IS_ACTIVE"]?.ToString(), "Y", StringComparison.OrdinalIgnoreCase),
                UpdatedBy = reader["UPDATED_BY"]?.ToString() ?? string.Empty,
                UpdatedOn = reader["UPDATED_ON"] == DBNull.Value ? null : Convert.ToDateTime(reader["UPDATED_ON"])
                };
            }

        private static object DbNull(string value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
        }
    }

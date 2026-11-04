using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        // Identities/context are always obtained from the authenticated server session.
        public List<Dictionary<string, object>> LegacyParaRequests(string operation, object input, int page)
        {
            var user = CreateSessionHandler().GetUser();
            if (user == null || user.UserContextAssignmentId.GetValueOrDefault() <= 0
                || user.UserEntityID.GetValueOrDefault() <= 0 || user.UserRoleID <= 0
                || !long.TryParse(user.PPNumber, out var pp))
                throw new UnauthorizedAccessException("An active selected user context is required.");
            using var connection = DatabaseConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.BindByName = true;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PKG_IAS_LEGACY_PARA.P_EXECUTE";
                GuardAgainstDynamicSql(command);
                command.Parameters.Add("p_operation", OracleDbType.Varchar2).Value = operation;
                command.Parameters.Add("p_ppno", OracleDbType.Int64).Value = pp;
                command.Parameters.Add("p_context", OracleDbType.Int32).Value = user.UserContextAssignmentId.Value;
                command.Parameters.Add("p_page", OracleDbType.Int32).Value = page;
                command.Parameters.Add("p_input", OracleDbType.Clob).Value = JsonSerializer.Serialize(input,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                command.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var rows = new List<Dictionary<string, object>>();
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (var i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i).ToLowerInvariant()] = reader.IsDBNull(i) ? null
                                : reader.GetValue(i) is Oracle.ManagedDataAccess.Types.OracleClob clob ? clob.Value
                                : reader.GetValue(i);
                        rows.Add(row);
                    }
                transaction.Commit();
                return rows;
            }
            catch { transaction.Rollback(); throw; }
        }
    }
}

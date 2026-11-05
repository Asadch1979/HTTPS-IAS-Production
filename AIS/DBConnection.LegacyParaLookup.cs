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
        // Typed lookup contract. No requestId, version or free-form JSON paths.
        public List<Dictionary<string, object>> LegacyParaReferenceLookup(LegacyParaLookup input, LegacyParaPage page)
        {
            if (page != LegacyParaPage.Maker)
                throw new UnauthorizedAccessException("Maker page context required.");
            if (input == null || string.IsNullOrWhiteSpace(input.Kind) || input.ParentId < 0 || input.RelationshipId < 0)
                throw new ArgumentException("Invalid lookup request.");
            var user = CreateSessionHandler().GetUser();
            if (user == null || user.UserContextAssignmentId.GetValueOrDefault() <= 0
                || user.UserEntityID.GetValueOrDefault() <= 0 || user.UserRoleID <= 0
                || user.UserGroupID <= 0 || !long.TryParse(user.PPNumber, out var pp))
                throw new UnauthorizedAccessException("An active selected user context is required.");

            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PKG_IAS_LEGACY_PARA_LOOKUP.P_GET_LOOKUP";
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("p_ppno", OracleDbType.Int64).Value = pp;
            command.Parameters.Add("p_context", OracleDbType.Int32).Value = user.UserContextAssignmentId.Value;
            command.Parameters.Add("p_role", OracleDbType.Int32).Value = user.UserRoleID;
            command.Parameters.Add("p_group", OracleDbType.Int32).Value = user.UserGroupID;
            command.Parameters.Add("p_page", OracleDbType.Int32).Value = (int)page;
            command.Parameters.Add("p_kind", OracleDbType.Varchar2).Value = input.Kind;
            command.Parameters.Add("p_parent", OracleDbType.Int64).Value = input.ParentId;
            command.Parameters.Add("p_relationship", OracleDbType.Int64).Value = input.RelationshipId;
            command.Parameters.Add("p_search", OracleDbType.Varchar2).Value =
                string.IsNullOrWhiteSpace(input.Search) ? (object)DBNull.Value : input.Search.Trim();
            command.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            var rows = new List<Dictionary<string, object>>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i).ToLowerInvariant()] = reader.IsDBNull(i) ? null :
                        reader.GetValue(i) is OracleClob clob ? clob.Value : reader.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }
    }
}

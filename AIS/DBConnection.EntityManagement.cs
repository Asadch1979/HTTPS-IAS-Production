using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace AIS.Controllers
{
    // Read-only adapter for the already deployed PKG_IAS_ENTITY_MGMT.
    // Authorization is enforced in AdministrationPanelController.EntityReads.cs
    // BEFORE calling any of these methods. Never expose these unscoped readers
    // to ordinary or reporting-role users.
    public partial class DBConnection
    {
        public sealed class EntityReadResult
        {
            public List<Dictionary<string, string>> Rows { get; set; } = new List<Dictionary<string, string>>();
            public bool Truncated { get; set; }
            public long TotalRows { get; set; }
        }

        private EntityReadResult ReadEntityManagementCursor(string procedure, Action<OracleCommand> bind, int maxRows)
        {
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = procedure;
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            bind(command);
            command.Parameters.Add("O_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            var result = new EntityReadResult();
            while (reader.Read())
            {
                if (result.Rows.Count >= maxRows)
                {
                    result.Truncated = true;
                    break;
                }
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                    {
                        row[reader.GetName(i)] = null;
                        continue;
                    }
                    string text;
                    if (string.Equals(reader.GetDataTypeName(i), "CLOB", StringComparison.OrdinalIgnoreCase))
                    {
                        using var clob = reader.GetOracleClob(i);
                        text = clob.IsNull ? null : clob.Value;
                    }
                    else
                    {
                        var value = reader.GetValue(i);
                        text = value is DateTime timestamp
                            ? timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                            : Convert.ToString(value, CultureInfo.InvariantCulture);
                    }
                    // History payloads may include large source text. Limit browser transfer;
                    // source archival rows remain unchanged and complete in Oracle.
                    row[reader.GetName(i)] = text != null && text.Length > 16000
                        ? text.Substring(0, 16000) + " [display truncated]"
                        : text;
                }
                result.Rows.Add(row);
            }
            if (result.Rows.Count > 0 &&
                result.Rows[0].TryGetValue("TOTAL_ROWS", out var total) &&
                long.TryParse(total, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
                result.TotalRows = count;
            return result;
        }

        public EntityReadResult SearchEntityManagement(int? typeId, string query, string status, int offset, int pageSize)
        {
            return ReadEntityManagementCursor("PKG_IAS_ENTITY_MGMT.P_SEARCH_ENTITIES", cmd =>
            {
                cmd.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value = (object)typeId ?? DBNull.Value;
                cmd.Parameters.Add("P_SEARCH_TEXT", OracleDbType.Varchar2).Value =
                    string.IsNullOrWhiteSpace(query) ? (object)DBNull.Value : query.Trim();
                cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = status;
                cmd.Parameters.Add("P_OFFSET", OracleDbType.Int32).Value = offset;
                cmd.Parameters.Add("P_PAGE_SIZE", OracleDbType.Int32).Value = pageSize;
            }, pageSize);
        }

        public EntityReadResult GetEntityManagementProfile(int entityId)
        {
            return ReadEntityManagementCursor("PKG_IAS_ENTITY_MGMT.P_GET_ENTITY_PROFILE", cmd =>
                cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId, 200);
        }

        public EntityReadResult GetEntityManagementClosurePreview(int entityId)
        {
            return ReadEntityManagementCursor("PKG_IAS_ENTITY_MGMT.P_PREVIEW_ENTITY_CLOSURE", cmd =>
                cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId, 1);
        }

        public EntityReadResult GetEntityManagementChangeLog(int entityId)
        {
            return ReadEntityManagementCursor("PKG_IAS_ENTITY_MGMT.P_GET_ENTITY_CHANGE_LOG", cmd =>
                cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId, 100);
        }

        public EntityReadResult GetEntityManagementParaArchive(int entityId)
        {
            return ReadEntityManagementCursor("PKG_IAS_ENTITY_MGMT.P_GET_ENTITY_PARA_ARCHIVE", cmd =>
                cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId, 100);
        }

        public object GetEntityManagementSummary()
        {
            // The existing approved search cursor exposes the total matching count.
            // The other four cards need separately approved aggregate definitions.
            var active = SearchEntityManagement(null, null, "ACTIVE", 0, 1);
            var inactive = SearchEntityManagement(null, null, "INACTIVE", 0, 1);
            return new { activeEntities = active.TotalRows, inactiveEntities = inactive.TotalRows };
        }
    }
}

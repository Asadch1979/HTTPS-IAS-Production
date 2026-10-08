using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        public List<OrganizationNode> GetOrganizationNodes(int? rootEntityId)
        {
            var results = new List<OrganizationNode>();
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "PKG_ORG_STRUCTURE.P_GET_NODES";
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            command.Parameters.Add("P_ROOT_ENTITY_ID", OracleDbType.Int32).Value = (object)rootEntityId ?? DBNull.Value;
            command.Parameters.Add("O_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new OrganizationNode
                {
                    EntityId = OrgNumber(reader, "ENTITY_ID").Value,
                    EntityName = OrgText(reader, "ENTITY_NAME"),
                    EntityTypeId = OrgNumber(reader, "ENTITY_TYPE_ID"),
                    EntityCategory = OrgText(reader, "ENTITY_CATEGORY"),
                    ParentEntityId = OrgNumber(reader, "PARENT_ENTITY_ID"),
                    ParentEntityName = OrgText(reader, "PARENT_ENTITY_NAME"),
                    Reporting1 = OrgNumber(reader, "REPORTING_1"),
                    Reporting2 = OrgNumber(reader, "REPORTING_2"),
                    Reporting3 = OrgNumber(reader, "REPORTING_3"),
                    Reporting4 = OrgNumber(reader, "REPORTING_4"),
                    Reporting5 = OrgNumber(reader, "REPORTING_5"),
                    Reporting6 = OrgNumber(reader, "REPORTING_6"),
                    DirectChildCount = OrgNumber(reader, "DIRECT_CHILD_COUNT") ?? 0,
                    IsRoot = OrgNumber(reader, "IS_ROOT") == 1,
                    IsOrphan = OrgNumber(reader, "IS_ORPHAN") == 1,
                    IsMissingName = OrgNumber(reader, "IS_MISSING_NAME") == 1,
                    HasTypeConflict = OrgNumber(reader, "HAS_TYPE_CONFLICT") == 1
                });
            }
            return results;
        }

        public List<OrganizationPathEntry> GetOrganizationEntityPath(int entityId)
        {
            var results = new List<OrganizationPathEntry>();
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "PKG_ORG_STRUCTURE.P_GET_ENTITY_PATH";
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            command.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId;
            command.Parameters.Add("O_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new OrganizationPathEntry
                {
                    LevelFromEntity = OrgNumber(reader, "LEVEL_FROM_ENTITY") ?? 0,
                    EntityId = OrgNumber(reader, "ENTITY_ID").Value,
                    EntityName = OrgText(reader, "ENTITY_NAME"),
                    ParentEntityId = OrgNumber(reader, "PARENT_ENTITY_ID"),
                    ParentEntityName = OrgText(reader, "PARENT_ENTITY_NAME")
                });
            }
            return results;
        }

        private static int? OrgNumber(OracleDataReader reader, string column) =>
            reader[column] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[column]);
        private static string OrgText(OracleDataReader reader, string column) =>
            reader[column] == DBNull.Value ? null : Convert.ToString(reader[column]);
    }
}

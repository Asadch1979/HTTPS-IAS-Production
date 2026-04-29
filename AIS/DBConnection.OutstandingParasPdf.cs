using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public List<DepartmentModel> GetAuditDepartments()
            {
            var rows = new List<DepartmentModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_AUDIT_DEPARTMENTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new DepartmentModel
                    {
                    ID = OutstandingPdfGetInt(reader, "ID", "AUDIT_DEPARTMENT_ID", "DEPARTMENT_ID", "CODE"),
                    CODE = OutstandingPdfGetString(reader, "CODE", "AUDIT_DEPARTMENT_CODE", "DEPARTMENT_CODE"),
                    NAME = OutstandingPdfGetString(reader, "NAME", "AUDIT_DEPARTMENT_NAME", "DEPARTMENT_NAME"),
                    STATUS = OutstandingPdfGetString(reader, "STATUS", "ISACTIVE")
                    });
                }

            return rows;
            }

        public List<OutstandingParaEntityPdfModel> GetOutstandingParaEntitiesForPdf(int auditDepartmentId, DateTime executionStartDate, DateTime executionEndDate)
            {
            var rows = new List<OutstandingParaEntityPdfModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARA_ENTITIES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = auditDepartmentId;
            cmd.Parameters.Add("P_EXECUTION_START_DATE", OracleDbType.Date).Value = executionStartDate;
            cmd.Parameters.Add("P_EXECUTION_END_DATE", OracleDbType.Date).Value = executionEndDate;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParaEntityPdfModel
                    {
                    EngagementId = OutstandingPdfGetInt(reader, "ENG_ID", "ENGAGEMENT_ID"),
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                    EntityCode = OutstandingPdfGetString(reader, "ENTITY_CODE", "BRANCH_CODE", "CODE"),
                    AuditDepartment = OutstandingPdfGetString(reader, "AUDIT_DEPARTMENT", "AUDIT_DEPARTMENT_NAME", "DEPARTMENT_NAME"),
                    AuditPeriod = OutstandingPdfGetString(reader, "AUDIT_PERIOD", "AUDIT_PERIOD_NAME"),
                    ExecutionStartDate = OutstandingPdfGetNullableDate(reader, "EXECUTION_START_DATE", "AUDIT_START_DATE"),
                    ExecutionEndDate = OutstandingPdfGetNullableDate(reader, "EXECUTION_END_DATE", "AUDIT_END_DATE"),
                    TeamLead = OutstandingPdfGetString(reader, "TEAM_LEAD", "TEAM_LEAD_NAME"),
                    TeamMembers = OutstandingPdfGetString(reader, "TEAM_MEMBERS", "AUDIT_TEAM_MEMBERS"),
                    OutstandingParasCount = OutstandingPdfGetInt(reader, "OUTSTANDING_PARAS_COUNT", "PARAS_COUNT", "PARA_COUNT")
                    });
                }

            return rows;
            }

        public OutstandingParaEntityPdfModel GetOutstandingParaEntityForPdfByEngId(int engId)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARA_ENTITY_BY_ENG_ID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                {
                return null;
                }

            return new OutstandingParaEntityPdfModel
                {
                EngagementId = OutstandingPdfGetInt(reader, "ENG_ID", "ENGAGEMENT_ID"),
                EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                EntityCode = OutstandingPdfGetString(reader, "ENTITY_CODE", "BRANCH_CODE", "CODE"),
                AuditDepartment = OutstandingPdfGetString(reader, "AUDIT_DEPARTMENT", "AUDIT_DEPARTMENT_NAME", "DEPARTMENT_NAME"),
                AuditPeriod = OutstandingPdfGetString(reader, "AUDIT_PERIOD", "AUDIT_PERIOD_NAME"),
                ExecutionStartDate = OutstandingPdfGetNullableDate(reader, "EXECUTION_START_DATE", "AUDIT_START_DATE"),
                ExecutionEndDate = OutstandingPdfGetNullableDate(reader, "EXECUTION_END_DATE", "AUDIT_END_DATE"),
                TeamLead = OutstandingPdfGetString(reader, "TEAM_LEAD", "TEAM_LEAD_NAME"),
                TeamMembers = OutstandingPdfGetString(reader, "TEAM_MEMBERS", "AUDIT_TEAM_MEMBERS"),
                OutstandingParasCount = OutstandingPdfGetInt(reader, "OUTSTANDING_PARAS_COUNT", "PARAS_COUNT", "PARA_COUNT")
                };
            }

        public List<OutstandingParaPdfModel> GetOutstandingParasForPdf(int auditDepartmentId, DateTime executionStartDate, DateTime executionEndDate)
            {
            var rows = new List<OutstandingParaPdfModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARAS_FOR_PDF";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = auditDepartmentId;
            cmd.Parameters.Add("P_EXECUTION_START_DATE", OracleDbType.Date).Value = executionStartDate;
            cmd.Parameters.Add("P_EXECUTION_END_DATE", OracleDbType.Date).Value = executionEndDate;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParaPdfModel
                    {
                    EngagementId = OutstandingPdfGetInt(reader, "ENG_ID", "ENGAGEMENT_ID"),
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                    ParaNo = OutstandingPdfGetString(reader, "PARA_NO", "PARA_NUMBER"),
                    ParaTitle = OutstandingPdfGetString(reader, "PARA_TITLE", "V_HEADER", "GIST"),
                    RiskCategory = OutstandingPdfGetString(reader, "RISK_CATEGORY", "RISK"),
                    ObservationText = OutstandingPdfGetString(reader, "OBSERVATION_TEXT", "V_DETAIL", "PARA_DETAIL"),
                    LatestManagementResponse = OutstandingPdfGetString(reader, "LATEST_MANAGEMENT_RESPONSE", "MANAGEMENT_REPLY", "MANAGEMENT_COMMENTS"),
                    AuditRemarks = OutstandingPdfGetString(reader, "AUDIT_REMARKS", "AUDITOR_COMMENTS", "AUDIT_COMMENTS"),
                    CurrentComplianceStatus = OutstandingPdfGetString(reader, "CURRENT_COMPLIANCE_STATUS", "COMPLIANCE_STATUS", "STATUS")
                    });
                }

            return rows;
            }

        public List<OutstandingParaPdfModel> GetOutstandingParasForPdfByEngId(int engId)
            {
            var rows = new List<OutstandingParaPdfModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARAS_BY_ENG_ID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParaPdfModel
                    {
                    EngagementId = OutstandingPdfGetInt(reader, "ENG_ID", "ENGAGEMENT_ID"),
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                    ParaNo = OutstandingPdfGetString(reader, "PARA_NO", "PARA_NUMBER"),
                    ParaTitle = OutstandingPdfGetString(reader, "PARA_TITLE", "V_HEADER", "GIST"),
                    RiskCategory = OutstandingPdfGetString(reader, "RISK_CATEGORY", "RISK"),
                    ObservationText = OutstandingPdfGetString(reader, "OBSERVATION_TEXT", "V_DETAIL", "PARA_DETAIL"),
                    LatestManagementResponse = OutstandingPdfGetString(reader, "LATEST_MANAGEMENT_RESPONSE", "MANAGEMENT_REPLY", "MANAGEMENT_COMMENTS"),
                    AuditRemarks = OutstandingPdfGetString(reader, "AUDIT_REMARKS", "AUDITOR_COMMENTS", "AUDIT_COMMENTS"),
                    CurrentComplianceStatus = OutstandingPdfGetString(reader, "CURRENT_COMPLIANCE_STATUS", "COMPLIANCE_STATUS", "STATUS")
                    });
                }

            return rows;
            }

        private static string OutstandingPdfGetString(IDataRecord reader, params string[] columnNames)
            {
            foreach (var columnName in columnNames)
                {
                var ordinal = OutstandingPdfGetOrdinal(reader, columnName);
                if (ordinal >= 0 && !reader.IsDBNull(ordinal))
                    {
                    return Convert.ToString(reader.GetValue(ordinal)) ?? string.Empty;
                    }
                }

            return string.Empty;
            }

        private static int OutstandingPdfGetInt(IDataRecord reader, params string[] columnNames)
            {
            foreach (var columnName in columnNames)
                {
                var ordinal = OutstandingPdfGetOrdinal(reader, columnName);
                if (ordinal >= 0 && !reader.IsDBNull(ordinal))
                    {
                    var value = Convert.ToString(reader.GetValue(ordinal));
                    if (int.TryParse(value, out var parsed))
                        {
                        return parsed;
                        }
                    }
                }

            return 0;
            }

        private static DateTime? OutstandingPdfGetNullableDate(IDataRecord reader, params string[] columnNames)
            {
            foreach (var columnName in columnNames)
                {
                var ordinal = OutstandingPdfGetOrdinal(reader, columnName);
                if (ordinal >= 0 && !reader.IsDBNull(ordinal))
                    {
                    return Convert.ToDateTime(reader.GetValue(ordinal));
                    }
                }

            return null;
            }

        private static int OutstandingPdfGetOrdinal(IDataRecord reader, string columnName)
            {
            for (var i = 0; i < reader.FieldCount; i++)
                {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                    return i;
                    }
                }

            return -1;
            }
        }
    }

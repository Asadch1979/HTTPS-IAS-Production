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

        public List<OutstandingParasSummaryPdfModel> GetOutstandingParasSummaryForPdf(int auditDepartmentId, string risk)
            {
            var rows = new List<OutstandingParasSummaryPdfModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARAS_SUMMARY_PDF";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = auditDepartmentId <= 0 ? DBNull.Value : auditDepartmentId;
            cmd.Parameters.Add("P_RISK", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(risk) || string.Equals(risk.Trim(), "ALL", StringComparison.OrdinalIgnoreCase)
                ? DBNull.Value
                : risk.Trim();
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParasSummaryPdfModel
                    {
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                    AuditDepartment = OutstandingPdfGetString(reader, "AUDIT_DEPARTMENT", "AUDIT_DEPARTMENT_NAME", "DEPARTMENT_NAME"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                    ParaNo = OutstandingPdfGetString(reader, "PARA_NO", "PARANO", "PARA_NUMBER", "COM_ID"),
                    AuditPeriod = OutstandingPdfGetString(reader, "AUDIT_PERIOD", "AUDIT_PERIOD_NAME"),
                    GistHeading = OutstandingPdfGetString(reader, "GIST_HEADING", "PARA_TITLE", "V_HEADER", "GIST"),
                    Risk = OutstandingPdfGetString(reader, "RISK", "RISK_CATEGORY"),
                    ParaText = OutstandingPdfGetString(reader, "PARA_TEXT", "OBSERVATION_TEXT", "V_DETAIL", "PARA_DETAIL"),
                    CurrentComplianceStatus = OutstandingPdfGetString(reader, "CURRENT_COMPLIANCE_STATUS", "COMPLIANCE_STATUS", "STATUS")
                    });
                }

            return rows;
            }

        public List<OutstandingParasSummarySetModel> GetOutstandingParasSummarySetsForPdf(int auditDepartmentId, string risk)
            {
            var rows = new List<OutstandingParasSummarySetModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARAS_SUMMARY_SETS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = auditDepartmentId <= 0 ? DBNull.Value : auditDepartmentId;
            cmd.Parameters.Add("P_RISK", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(risk) || string.Equals(risk.Trim(), "ALL", StringComparison.OrdinalIgnoreCase)
                ? DBNull.Value
                : risk.Trim();
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParasSummarySetModel
                    {
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                    AuditDepartment = OutstandingPdfGetString(reader, "AUDIT_DEPARTMENT", "AUDIT_DEPARTMENT_NAME", "DEPARTMENT_NAME"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                    Risk = OutstandingPdfGetString(reader, "RISK", "RISK_CATEGORY"),
                    RowCount = OutstandingPdfGetInt(reader, "ROW_COUNT", "TOTAL_ROWS", "PARA_COUNT")
                    });
                }

            return rows;
            }

        public List<OutstandingParasSummaryPdfModel> GetOutstandingParasSummaryForPdfSet(int auditDepartmentId, int entityId, string risk)
            {
            var rows = new List<OutstandingParasSummaryPdfModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_FRPT.P_GET_OUTSTANDING_PARAS_SUMMARY_SET_PDF";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = auditDepartmentId <= 0 ? DBNull.Value : auditDepartmentId;
            cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId;
            cmd.Parameters.Add("P_RISK", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(risk) || string.Equals(risk.Trim(), "ALL", StringComparison.OrdinalIgnoreCase)
                ? DBNull.Value
                : risk.Trim();
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParasSummaryPdfModel
                    {
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID", "ENT_ID"),
                    AuditDepartment = OutstandingPdfGetString(reader, "AUDIT_DEPARTMENT", "AUDIT_DEPARTMENT_NAME", "DEPARTMENT_NAME"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME", "BRANCH_NAME", "AUDIT_ENTITY_NAME"),
                    ParaNo = OutstandingPdfGetString(reader, "PARA_NO", "PARANO", "PARA_NUMBER", "COM_ID"),
                    AuditPeriod = OutstandingPdfGetString(reader, "AUDIT_PERIOD", "AUDIT_PERIOD_NAME"),
                    GistHeading = OutstandingPdfGetString(reader, "GIST_HEADING", "PARA_TITLE", "V_HEADER", "GIST"),
                    Risk = OutstandingPdfGetString(reader, "RISK", "RISK_CATEGORY"),
                    ParaText = OutstandingPdfGetString(reader, "PARA_TEXT", "OBSERVATION_TEXT", "V_DETAIL", "PARA_DETAIL"),
                    CurrentComplianceStatus = OutstandingPdfGetString(reader, "CURRENT_COMPLIANCE_STATUS", "COMPLIANCE_STATUS", "STATUS")
                    });
                }

            return rows;
            }

        public OutstandingParasSummaryPdfSaveResult SaveCiaSummaryPdf(
            string batchId,
            int auditDepartmentId,
            string auditDepartmentName,
            int entityId,
            string entityName,
            string risk,
            int partNo,
            string fileName,
            string fileMimeType,
            byte[] pdfBytes,
            string generatedBy,
            DateTime? expiresOn,
            string status,
            string errorMessage)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_CIA_SUMMARY_PDF.P_SAVE_CIA_SUMMARY_PDF";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Varchar2).Value = CiaDbValue(batchId);
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = auditDepartmentId <= 0 ? DBNull.Value : auditDepartmentId;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_NAME", OracleDbType.Varchar2).Value = CiaDbValue(auditDepartmentName);
            cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId <= 0 ? DBNull.Value : entityId;
            cmd.Parameters.Add("P_ENTITY_NAME", OracleDbType.Varchar2).Value = CiaDbValue(entityName);
            cmd.Parameters.Add("P_RISK", OracleDbType.Varchar2).Value = CiaDbValue(risk);
            cmd.Parameters.Add("P_PART_NO", OracleDbType.Int32).Value = partNo <= 0 ? 1 : partNo;
            cmd.Parameters.Add("P_FILE_NAME", OracleDbType.Varchar2).Value = CiaDbValue(fileName);
            cmd.Parameters.Add("P_FILE_MIME_TYPE", OracleDbType.Varchar2).Value = CiaDbValue(fileMimeType);
            cmd.Parameters.Add("P_FILE_SIZE", OracleDbType.Int64).Value = pdfBytes?.LongLength ?? 0;
            cmd.Parameters.Add("P_PDF_BLOB", OracleDbType.Blob).Value = pdfBytes == null || pdfBytes.Length == 0 ? DBNull.Value : pdfBytes;
            cmd.Parameters.Add("P_GENERATED_BY", OracleDbType.Varchar2).Value = CiaDbValue(generatedBy);
            cmd.Parameters.Add("P_EXPIRES_ON", OracleDbType.Date).Value = expiresOn.HasValue ? (object)expiresOn.Value : DBNull.Value;
            cmd.Parameters.Add("P_STATUS", OracleDbType.Varchar2).Value = CiaDbValue(status);
            cmd.Parameters.Add("P_ERROR_MESSAGE", OracleDbType.Varchar2).Value = CiaDbValue(errorMessage);
            cmd.Parameters.Add("O_PDF_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_STATUS", OracleDbType.Varchar2, 50).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return new OutstandingParasSummaryPdfSaveResult
                {
                PdfId = ConvertOracleInt(cmd.Parameters["O_PDF_ID"].Value),
                Status = ConvertOracleString(cmd.Parameters["O_STATUS"].Value),
                Message = ConvertOracleString(cmd.Parameters["O_MESSAGE"].Value)
                };
            }

        public List<OutstandingParasSummaryPdfStoreModel> GetCiaSummaryPdfList()
            {
            var rows = new List<OutstandingParasSummaryPdfStoreModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_CIA_SUMMARY_PDF.P_GET_CIA_SUMMARY_PDF_LIST";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_BATCH_ID", OracleDbType.Varchar2).Value = DBNull.Value;
            cmd.Parameters.Add("P_GENERATED_BY", OracleDbType.Varchar2).Value = DBNull.Value;
            cmd.Parameters.Add("P_AUDIT_DEPARTMENT_ID", OracleDbType.Int32).Value = DBNull.Value;
            cmd.Parameters.Add("P_RISK", OracleDbType.Varchar2).Value = DBNull.Value;
            cmd.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = DBNull.Value;
            cmd.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = DBNull.Value;
            cmd.Parameters.Add("P_LATEST_BATCH_ONLY", OracleDbType.Varchar2).Value = "N";
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new OutstandingParasSummaryPdfStoreModel
                    {
                    PdfId = OutstandingPdfGetInt(reader, "PDF_ID"),
                    BatchId = OutstandingPdfGetString(reader, "BATCH_ID"),
                    AuditDepartmentId = OutstandingPdfGetInt(reader, "AUDIT_DEPARTMENT_ID"),
                    AuditDepartmentName = OutstandingPdfGetString(reader, "AUDIT_DEPARTMENT_NAME"),
                    EntityId = OutstandingPdfGetInt(reader, "ENTITY_ID"),
                    EntityName = OutstandingPdfGetString(reader, "ENTITY_NAME"),
                    Risk = OutstandingPdfGetString(reader, "RISK"),
                    PartNo = OutstandingPdfGetInt(reader, "PART_NO"),
                    FileName = OutstandingPdfGetString(reader, "FILE_NAME"),
                    FileMimeType = OutstandingPdfGetString(reader, "FILE_MIME_TYPE"),
                    FileSize = OutstandingPdfGetLong(reader, "FILE_SIZE"),
                    GeneratedBy = OutstandingPdfGetString(reader, "GENERATED_BY"),
                    GeneratedOn = OutstandingPdfGetNullableDate(reader, "GENERATED_ON"),
                    ExpiresOn = OutstandingPdfGetNullableDate(reader, "EXPIRES_ON"),
                    Status = OutstandingPdfGetString(reader, "STATUS"),
                    ErrorMessage = OutstandingPdfGetString(reader, "ERROR_MESSAGE")
                    });
                }

            return rows;
            }

        public OutstandingParasSummaryPdfDownloadModel DownloadCiaSummaryPdf(int pdfId)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_CIA_SUMMARY_PDF.P_DOWNLOAD_CIA_SUMMARY_PDF";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_PDF_ID", OracleDbType.Int32).Value = pdfId;
            cmd.Parameters.Add("O_FILE_NAME", OracleDbType.Varchar2, 1000).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_FILE_MIME_TYPE", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_FILE_SIZE", OracleDbType.Int64).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_PDF_BLOB", OracleDbType.Blob).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_STATUS", OracleDbType.Varchar2, 50).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return new OutstandingParasSummaryPdfDownloadModel
                {
                PdfId = pdfId,
                FileName = ConvertOracleString(cmd.Parameters["O_FILE_NAME"].Value),
                FileMimeType = ConvertOracleString(cmd.Parameters["O_FILE_MIME_TYPE"].Value),
                FileSize = ConvertOracleLong(cmd.Parameters["O_FILE_SIZE"].Value),
                ContentBytes = ConvertOracleBlob(cmd.Parameters["O_PDF_BLOB"].Value),
                Status = ConvertOracleString(cmd.Parameters["O_STATUS"].Value),
                Message = ConvertOracleString(cmd.Parameters["O_MESSAGE"].Value)
                };
            }

        public OutstandingParasSummaryPdfDeleteResult DeleteCiaSummaryPdf(int pdfId, string deletedBy)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText = "PKG_CIA_SUMMARY_PDF.P_DELETE_CIA_SUMMARY_PDF";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_PDF_ID", OracleDbType.Int32).Value = pdfId;
            cmd.Parameters.Add("P_DELETED_BY", OracleDbType.Varchar2).Value = CiaDbValue(deletedBy);
            cmd.Parameters.Add("O_STATUS", OracleDbType.Varchar2, 50).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("O_MESSAGE", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return new OutstandingParasSummaryPdfDeleteResult
                {
                Status = ConvertOracleString(cmd.Parameters["O_STATUS"].Value),
                Message = ConvertOracleString(cmd.Parameters["O_MESSAGE"].Value)
                };
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

        private static long OutstandingPdfGetLong(IDataRecord reader, params string[] columnNames)
            {
            foreach (var columnName in columnNames)
                {
                var ordinal = OutstandingPdfGetOrdinal(reader, columnName);
                if (ordinal >= 0 && !reader.IsDBNull(ordinal))
                    {
                    var value = Convert.ToString(reader.GetValue(ordinal));
                    if (long.TryParse(value, out var parsed))
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

        private static object CiaDbValue(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
            }

        private static string ConvertOracleString(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return string.Empty;
                }

            return value.ToString();
            }

        private static int ConvertOracleInt(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return 0;
                }

            return int.TryParse(value.ToString(), out var parsed) ? parsed : 0;
            }

        private static long ConvertOracleLong(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return 0;
                }

            return long.TryParse(value.ToString(), out var parsed) ? parsed : 0;
            }

        private static byte[] ConvertOracleBlob(object value)
            {
            if (value == null || value == DBNull.Value)
                {
                return Array.Empty<byte>();
                }

            if (value is byte[] bytes)
                {
                return bytes;
                }

            if (value is OracleBlob blob && !blob.IsNull)
                {
                return blob.Value;
                }

            return Array.Empty<byte>();
            }
        }
    }

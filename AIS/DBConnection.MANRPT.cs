using AIS.Models.FieldAuditReport;
using AIS.Models.ManagementReport;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public ManagementAuditCoverModel GetManagementAuditCover(int engId)
            {
            var cover = new ManagementAuditCoverModel();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_AUDIT_COVER";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                {
                cover.AuditedBy = reader["AUDITED_BY"] == DBNull.Value ? string.Empty : reader["AUDITED_BY"].ToString();
                cover.Reporting = reader["REPORTING"] == DBNull.Value ? string.Empty : reader["REPORTING"].ToString();
                cover.EntityName = reader["ENTITY_NAME"] == DBNull.Value ? string.Empty : reader["ENTITY_NAME"].ToString();
                cover.AuditedOn = reader["AUDITED_ON"] == DBNull.Value ? string.Empty : reader["AUDITED_ON"].ToString();
                }

            return cover;
            }

        public List<GetTeamDetailsModel> GetManagementAuditTeamDetails(int engId)
            {
            return GetFieldAuditTeamDetails(engId);
            }

        public Dictionary<string, string> GetManReportObjectiveScope(int engId)
            {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_MAN_OBJECTIVE_SCOPE";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                {
                fields[ManagementReportSectionCodes.Objective] = reader[ManagementReportSectionCodes.Objective] == DBNull.Value
                    ? string.Empty
                    : reader[ManagementReportSectionCodes.Objective].ToString();
                fields[ManagementReportSectionCodes.Scope] = reader[ManagementReportSectionCodes.Scope] == DBNull.Value
                    ? string.Empty
                    : reader[ManagementReportSectionCodes.Scope].ToString();
                fields[ManagementReportSectionCodes.Methodology] = reader[ManagementReportSectionCodes.Methodology] == DBNull.Value
                    ? string.Empty
                    : reader[ManagementReportSectionCodes.Methodology].ToString();
                fields[ManagementReportSectionCodes.Disclaimer] = reader[ManagementReportSectionCodes.Disclaimer] == DBNull.Value
                    ? string.Empty
                    : reader[ManagementReportSectionCodes.Disclaimer].ToString();
                fields[ManagementReportSectionCodes.Introduction] = reader[ManagementReportSectionCodes.Introduction] == DBNull.Value
                    ? string.Empty
                    : reader[ManagementReportSectionCodes.Introduction].ToString();
                }

            return fields;
            }

        public string GetManReportExecutiveSummary(int engId)
            {
            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_MAN_EXEC_SUMMARY";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                {
                return reader["TEXT_BLOCK"] == DBNull.Value ? string.Empty : reader["TEXT_BLOCK"].ToString();
                }

            return string.Empty;
            }

        public void SaveManReportTextBlock(int engId, string sectionCode, string text)
            {
            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_SAVE_TEXT_BLOCK";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("P_SECTION_CODE", OracleDbType.Varchar2).Value = sectionCode ?? string.Empty;
            cmd.Parameters.Add("P_TEXT_BLOCK", OracleDbType.Clob).Value = text ?? string.Empty;

            cmd.ExecuteNonQuery();
            }

        public List<ManagementReportObservationModel> GetManReportObservations(int engId)
            {
            var list = new List<ManagementReportObservationModel>();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_MAN_AUDIT_OBSERVATIONS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                var status = reader["STATUS"] == DBNull.Value ? string.Empty : reader["STATUS"].ToString();
                list.Add(new ManagementReportObservationModel
                    {
                    ParaNo = reader["PARA_NO"] == DBNull.Value ? string.Empty : reader["PARA_NO"].ToString(),
                    Title = reader["TITLE"] == DBNull.Value ? string.Empty : reader["TITLE"].ToString(),
                    ParaText = reader["PARA_TEXT"] == DBNull.Value ? string.Empty : reader["PARA_TEXT"].ToString(),
                    RiskCategory = reader["RISK_CATEGORY"] == DBNull.Value ? string.Empty : reader["RISK_CATEGORY"].ToString(),
                    Recommendation = reader["RECOMMENDATION"] == DBNull.Value ? string.Empty : reader["RECOMMENDATION"].ToString(),
                    ManagementReply = reader["MANAGEMENT_REPLY"] == DBNull.Value ? string.Empty : reader["MANAGEMENT_REPLY"].ToString(),
                    AuditReply = reader["AUDIT_REPLY"] == DBNull.Value ? string.Empty : reader["AUDIT_REPLY"].ToString(),
                    Status = string.IsNullOrWhiteSpace(status) ? "Un-Settled" : status
                    });
                }

            return list;
            }

        public List<ManagementReportSettledParaModel> GetManReportSettledParas(int engId)
            {
            var list = new List<ManagementReportSettledParaModel>();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_MAN_SETTLED_PARAS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new ManagementReportSettledParaModel
                    {
                    Title = reader["TITLE"] == DBNull.Value ? string.Empty : reader["TITLE"].ToString(),
                    ParaText = reader["PARA_TEXT"] == DBNull.Value ? string.Empty : reader["PARA_TEXT"].ToString(),
                    ManagementReply = reader["MANAGEMENT_REPLY"] == DBNull.Value ? string.Empty : reader["MANAGEMENT_REPLY"].ToString()
                    });
                }

            return list;
            }

        public List<FieldAuditPdfParaModel> GetManReportObservationPdfParas(int engId)
            {
            var list = new List<FieldAuditPdfParaModel>();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_MAN_AUDIT_OBSERVATIONS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                var status = reader["STATUS"] == DBNull.Value ? string.Empty : reader["STATUS"].ToString();
                list.Add(new FieldAuditPdfParaModel
                    {
                    ParaNo = reader["PARA_NO"] == DBNull.Value ? string.Empty : reader["PARA_NO"].ToString(),
                    Gist = reader["TITLE"] == DBNull.Value ? string.Empty : reader["TITLE"].ToString(),
                    ParaDetail = reader["PARA_TEXT"] == DBNull.Value ? string.Empty : reader["PARA_TEXT"].ToString(),
                    Risk = reader["RISK_CATEGORY"] == DBNull.Value ? string.Empty : reader["RISK_CATEGORY"].ToString(),
                    Recommendations = reader["RECOMMENDATION"] == DBNull.Value ? string.Empty : reader["RECOMMENDATION"].ToString(),
                    ManagementComments = reader["MANAGEMENT_REPLY"] == DBNull.Value ? string.Empty : reader["MANAGEMENT_REPLY"].ToString(),
                    AuditorComments = reader["AUDIT_REPLY"] == DBNull.Value ? string.Empty : reader["AUDIT_REPLY"].ToString(),
                    Status = string.IsNullOrWhiteSpace(status) ? "Un-Settled" : status
                    });
                }

            return list;
            }

        public List<FieldAuditPdfParaModel> GetManReportSettledPdfParas(int engId)
            {
            var list = new List<FieldAuditPdfParaModel>();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_MANRPT.P_GET_MAN_SETTLED_PARAS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new FieldAuditPdfParaModel
                    {
                    Gist = reader["TITLE"] == DBNull.Value ? string.Empty : reader["TITLE"].ToString(),
                    ParaDetail = reader["PARA_TEXT"] == DBNull.Value ? string.Empty : reader["PARA_TEXT"].ToString(),
                    ManagementComments = reader["MANAGEMENT_REPLY"] == DBNull.Value ? string.Empty : reader["MANAGEMENT_REPLY"].ToString(),
                    Status = "Settled"
                    });
                }

            return list;
            }

        public List<FieldAuditPdfSectionModel> GetManagementReportPdfSections(int engId)
            {
            var sections = new List<FieldAuditPdfSectionModel>();
            var objectiveScope = GetManReportObjectiveScope(engId);
            var summary = GetManReportExecutiveSummary(engId);

            sections.Add(new FieldAuditPdfSectionModel
                {
                SectionCode = "OBJECTIVE",
                SectionTitle = "Objective",
                HtmlContent = objectiveScope.TryGetValue(ManagementReportSectionCodes.Objective, out var obj) ? obj : string.Empty
                });
            sections.Add(new FieldAuditPdfSectionModel
                {
                SectionCode = "SCOPE",
                SectionTitle = "Scope",
                HtmlContent = objectiveScope.TryGetValue(ManagementReportSectionCodes.Scope, out var scope) ? scope : string.Empty
                });
            sections.Add(new FieldAuditPdfSectionModel
                {
                SectionCode = "METHODOLOGY",
                SectionTitle = "Methodology",
                HtmlContent = objectiveScope.TryGetValue(ManagementReportSectionCodes.Methodology, out var method) ? method : string.Empty
                });
            sections.Add(new FieldAuditPdfSectionModel
                {
                SectionCode = "DISCLAIMER",
                SectionTitle = "Disclaimer",
                HtmlContent = objectiveScope.TryGetValue(ManagementReportSectionCodes.Disclaimer, out var disclaimer) ? disclaimer : string.Empty
                });
            sections.Add(new FieldAuditPdfSectionModel
                {
                SectionCode = "INTRODUCTION",
                SectionTitle = "Introduction",
                HtmlContent = objectiveScope.TryGetValue(ManagementReportSectionCodes.Introduction, out var intro) ? intro : string.Empty
                });
            sections.Add(new FieldAuditPdfSectionModel
                {
                SectionCode = "EXEC_SUMMARY",
                SectionTitle = "Executive Summary",
                HtmlContent = summary ?? string.Empty
                });

            return sections;
            }
        }
    }

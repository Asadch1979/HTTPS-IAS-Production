using AIS.Models.FieldAuditReport;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        /* =========================================================
           MASTER PDF AGGREGATE
        ========================================================= */

        public FieldAuditPdfReportData GetFieldAuditReportPdfData(int engId, int? reportVersion)
            {
            var activeEngagementId = GetActiveEngagementId();
            return new FieldAuditPdfReportData
                {
                Header = GetFieldAuditPdfHeader(activeEngagementId, reportVersion),
                ReportMeta = GetFieldAuditPdfReportMeta(activeEngagementId, reportVersion),
                Sections = GetFieldAuditPdfSections(activeEngagementId, reportVersion),
                KpiRows = GetFieldAuditPdfKpiSnapshot(activeEngagementId, reportVersion),
                NplRows = GetFieldAuditPdfNplSnapshot(activeEngagementId, reportVersion),
                StaffRows = GetFieldAuditPdfStaffSnapshot(activeEngagementId, reportVersion),
                Paras = GetFieldAuditPdfParas(activeEngagementId, reportVersion),
                StatisticsRows = GetFieldAuditPdfStatistics(activeEngagementId, reportVersion),
                IncomeLeakageRows = GetFieldAuditPdfIncomeLeakage(activeEngagementId, reportVersion)
                };
            }

        /* =========================================================
           HEADER
        ========================================================= */

        public FieldAuditPdfHeaderModel GetFieldAuditPdfHeader(int engId, int? reportVersion)
            {
            var header = new FieldAuditPdfHeaderModel();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_HEADER";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                {
                header.BankName = GetString(reader, "BANK_NAME");
                header.InternalAuditDivision = GetString(reader, "INTERNAL_AUDIT_DIVISION");
                header.BranchName = GetString(reader, "BRANCH_NAME");
                header.BranchCode = GetString(reader, "BRANCH_CODE");
                header.AuditPeriod = GetString(reader, "AUDIT_PERIOD");
                header.AuditStartDate = GetNullableDate(reader, "AUDIT_START_DATE");
                header.AuditEndDate = GetNullableDate(reader, "AUDIT_END_DATE");
                header.ReportStatus = GetString(reader, "REPORT_STATUS");
                header.VersionNumber = GetString(reader, "VERSION_NO");
                header.EntityName = GetString(reader, "ENTITY_NAME");
                }

            return header;
            }

        /* =========================================================
           REPORT META
        ========================================================= */

        public FieldAuditPdfReportMetaModel GetFieldAuditPdfReportMeta(int engId, int? reportVersion)
            {
            var meta = new FieldAuditPdfReportMetaModel();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_REPORT_META";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                {
                meta.ReportStatus = GetString(reader, "REPORT_STATUS");
                meta.VersionNumber = GetString(reader, "VERSION_NO");
                meta.GeneratedBy = GetString(reader, "GENERATED_BY");
                meta.GeneratedOn = GetNullableDate(reader, "GENERATED_ON");
                meta.EntityName = GetString(reader, "ENTITY_NAME");
                meta.AuditPeriod = GetString(reader, "AUDIT_PERIOD");
                }

            return meta;
            }

        /* =========================================================
           SECTIONS
        ========================================================= */

        public List<FieldAuditPdfSectionModel> GetFieldAuditPdfSections(int engId, int? reportVersion)
            {
            var sections = new List<FieldAuditPdfSectionModel>();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_SECTIONS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                sections.Add(new FieldAuditPdfSectionModel
                    {
                    SectionCode = GetString(reader, "SECTION_CODE"),
                    SectionTitle = GetString(reader, "SECTION_TITLE"),
                    DisplayOrder = GetInt(reader, "DISPLAY_ORDER"),
                    IsMandatory = IsTruthy(reader["IS_MANDATORY"]),
                    HtmlContent = GetString(reader, "TEXT_BLOCK")
                    });
                }

            return sections.OrderBy(s => s.DisplayOrder).ToList();
            }

        /* =========================================================
           KPI
        ========================================================= */

        public List<FieldAuditPdfKpiRowModel> GetFieldAuditPdfKpiSnapshot(int engId, int? reportVersion)
            {
            var rows = new List<FieldAuditPdfKpiRowModel>();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_KPI";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfKpiRowModel
                    {
                    KpiCode = GetString(reader, "KPI_CODE"),
                    KpiLabel = GetString(reader, "KPI_LABEL"),
                    PeriodEndDate = GetNullableDate(reader, "PERIOD_END_DATE"),
                    ActualValue = GetNullableDecimal(reader, "ACTUAL_VALUE"),
                    TargetValue = GetNullableDecimal(reader, "TARGET_VALUE"),
                    Unit = GetString(reader, "UNIT")
                    });
                }

            return rows;
            }

        /* =========================================================
           NPL
        ========================================================= */

        public List<FieldAuditPdfNplRowModel> GetFieldAuditPdfNplSnapshot(int engId, int? reportVersion)
            {
            var rows = new List<FieldAuditPdfNplRowModel>();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_NPL";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfNplRowModel
                    {
                    Category = GetString(reader, "CATEGORY"),
                    PeriodEndDate = GetNullableDate(reader, "PERIOD_END_DATE"),
                    CaseCount = GetNullableInt(reader, "CASE_COUNT"),
                    OutstandingAmount = GetNullableDecimal(reader, "OUTSTANDING_AMOUNT"),
                    ProvisionAmount = GetNullableDecimal(reader, "PROVISION_AMOUNT")
                    });
                }

            return rows;
            }

        /* =========================================================
           STAFF
        ========================================================= */

        public List<FieldAuditPdfStaffRowModel> GetFieldAuditPdfStaffSnapshot(int engId, int? reportVersion)
            {
            var rows = new List<FieldAuditPdfStaffRowModel>();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_STAFF";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfStaffRowModel
                    {
                    Designation = GetString(reader, "DESIGNATION"),
                    Strength = GetNullableInt(reader, "STRENGTH"),
                    AsOfDate = GetNullableDate(reader, "AS_OF_DATE")
                    });
                }

            return rows;
            }

        /* =========================================================
           PARAS (IAS CORE)
        ========================================================= */

        public List<FieldAuditPdfParaModel> GetFieldAuditPdfParas(int engId, int? reportVersion)
            {
            var rows = new List<FieldAuditPdfParaModel>();
            var sessionHandler = CreateSessionHandler();
            var user = sessionHandler.GetUserOrThrow();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_RPT.R_GETAUDITEEPARAS";

            cmd.Parameters.Add("ENGID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.UserEntityID;
            cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = user.PPNumber;
            cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = user.UserRoleID;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfParaModel
                    {
                    ParaNo = GetString(reader, "PARA_NO"),
                    Gist = GetString(reader, "V_HEADER"),
                    ParaDetail = GetString(reader, "V_DETAIL"),
                    Risk = GetString(reader, "RISK"),
                    ManagementComments = GetString(reader, "MANAGEMENT_REPLY"),
                    Recommendations = GetString(reader, "RECOMMENDATION"),
                    AnnexureCode = GetOptionalString(reader, "ANNEXURE_CODE"),
                    Instances = GetOptionalString(reader, "INSTANCES"),
                    Amount = GetOptionalString(reader, "AMOUNT"),
                    Implications = GetOptionalString(reader, "IMPLICATIONS"),
                    AuditorComments = GetOptionalString(reader, "AUDITOR_COMMENTS"),
                    RemarksInCharge = GetOptionalString(reader, "SVP_REMARKS"),
                    Nature = GetOptionalString(reader, "NATURE"),
                    IsSignificant = GetOptionalString(reader, "IS_SIGNIFICANT") == "1"
                    });
                }

            return rows;
            }

        /* =========================================================
           STATISTICS
        ========================================================= */

        public List<FieldAuditPdfStatisticsRowModel> GetFieldAuditPdfStatistics(int engId, int? reportVersion)
            {
            var rows = new List<FieldAuditPdfStatisticsRowModel>();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_STATISTICS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfStatisticsRowModel
                    {
                    RiskLevel = GetString(reader, "RISK_LEVEL"),
                    ReportedCount = GetNullableInt(reader, "REPORTED_COUNT"),
                    RectifiedCount = GetNullableInt(reader, "RECTIFIED_COUNT"),
                    OutstandingCount = GetNullableInt(reader, "OUTSTANDING_COUNT")
                    });
                }

            return rows;
            }

        /* =========================================================
           INCOME LEAKAGE
        ========================================================= */

        public List<FieldAuditPdfIncomeLeakageRowModel> GetFieldAuditPdfIncomeLeakage(int engId, int? reportVersion)
            {
            var rows = new List<FieldAuditPdfIncomeLeakageRowModel>();
            var activeEngagementId = GetActiveEngagementId();

            using var con = DatabaseConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_INCOME_LEAKAGE";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = reportVersion ?? (object)DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfIncomeLeakageRowModel
                    {
                    CaseReference = GetString(reader, "CASE_REFERENCE"),
                    Description = GetString(reader, "DESCRIPTION"),
                    Amount = GetNullableDecimal(reader, "AMOUNT")
                    });
                }

            return rows;
            }

        /* =========================================================
           LOCAL HELPERS (SELF-CONTAINED)
        ========================================================= */

        private static string GetString(IDataRecord r, string c)
            => r[c] == DBNull.Value ? string.Empty : r[c].ToString();

        private static int GetInt(IDataRecord r, string c)
            => r[c] == DBNull.Value ? 0 : Convert.ToInt32(r[c]);

        private static int? GetNullableInt(IDataRecord r, string c)
            => r[c] == DBNull.Value ? (int?)null : Convert.ToInt32(r[c]);

        private static decimal? GetNullableDecimal(IDataRecord r, string c)
            => r[c] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[c]);

        private static DateTime? GetNullableDate(IDataRecord r, string c)
            => r[c] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[c]);

        private static bool IsTruthy(object v)
            => v != DBNull.Value && (v.ToString() == "Y" || v.ToString() == "1");

        private static string GetOptionalString(IDataRecord r, string c)
            {
            try
                {
                return r[c] == DBNull.Value ? string.Empty : r[c].ToString();
                }
            catch (IndexOutOfRangeException)
                {
                return string.Empty;
                }
            }

        private int GetActiveEngagementId()
            {
            var sessionHandler = CreateSessionHandler();
            return sessionHandler.GetActiveEngagementIdOrThrow();
            }
        }
    }

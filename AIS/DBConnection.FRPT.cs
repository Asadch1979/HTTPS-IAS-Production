using AIS.Models;
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
        private static readonly MenuPagesModel[] FieldAuditReportMenuPages =
        {
            new MenuPagesModel
            {
                PageId = 970101,
                Page_Name = "Report Overview",
                Page_Path = "FieldAuditReport/ReportOverview",
                Sub_Menu = "Field Audit Report",
                Sub_Menu_Name = "Field Audit Report",
                Hide_Menu = 0
            },
            new MenuPagesModel
            {
                PageId = 970102,
                Page_Name = "Narrative Sections",
                Page_Path = "FieldAuditReport/NarrativeSections",
                Sub_Menu = "Field Audit Report",
                Sub_Menu_Name = "Field Audit Report",
                Hide_Menu = 1
            },
            new MenuPagesModel
            {
                PageId = 970103,
                Page_Name = "KPI Snapshot",
                Page_Path = "FieldAuditReport/KpiSnapshot",
                Sub_Menu = "Field Audit Report",
                Sub_Menu_Name = "Field Audit Report",
                Hide_Menu = 1
            },
            new MenuPagesModel
            {
                PageId = 970104,
                Page_Name = "NPL Snapshot",
                Page_Path = "FieldAuditReport/NplSnapshot",
                Sub_Menu = "Field Audit Report",
                Sub_Menu_Name = "Field Audit Report",
                Hide_Menu = 1
            },
            new MenuPagesModel
            {
                PageId = 970105,
                Page_Name = "Staff Snapshot",
                Page_Path = "FieldAuditReport/StaffSnapshot",
                Sub_Menu = "Field Audit Report",
                Sub_Menu_Name = "Field Audit Report",
                Hide_Menu = 1
            },
            new MenuPagesModel
            {
                PageId = 970106,
                Page_Name = "Finalize Report",
                Page_Path = "FieldAuditReport/FinalizeReport",
                Sub_Menu = "Field Audit Report",
                Sub_Menu_Name = "Field Audit Report",
                Hide_Menu = 1
            }
        };

        /* =========================================================
           REPORT STATUS
        ========================================================= */

        public List<FieldAuditEngagementOptionModel> GetReportEntities()
            {
            var list = new List<FieldAuditEngagementOptionModel>();

            var sessionHandler = CreateSessionHandler();
            var con = this.DatabaseConnection();
            var loggedInUser = sessionHandler.GetUserOrThrow();

            using (con)
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FRPT.P_GET_REPORT_ENTITY";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("P_USER_ENT_ID", OracleDbType.Int32)
                              .Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor)
                              .Direction = ParameterDirection.Output;

                using (var dr = cmd.ExecuteReader())
                    {
                    while (dr.Read())
                        {
                        var engId = Convert.ToInt32(dr["ENG_ID"]);

                        if (engId <= 0)
                            continue;

                        list.Add(new FieldAuditEngagementOptionModel
                            {
                            EngId = engId,
                            EngagementId = engId,          // 🔑 THIS WAS MISSING
                            EntityId = Convert.ToInt32(dr["ENTITY_ID"]),
                            Entitytype = Convert.ToInt32(dr["ENTITY_TYPE"]),
                            AuditType = dr["AUDIT_TYPE"]?.ToString(),
                            EntityName = dr["ENTITY_NAME"]?.ToString(),
                            AuditPeriod = dr["AUDIT_PERIOD"]?.ToString()
                            });
                        }
                    }
                }

            return list;
            }

        public bool IsFieldAuditReportFinal(int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_IS_REPORT_FINAL";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            var outParam = cmd.Parameters.Add("O_IS_FINAL", OracleDbType.Int32, ParameterDirection.Output);

            cmd.ExecuteNonQuery();
            if (outParam.Value == null || outParam.Value == DBNull.Value)
                {
                return false;
                }

            var oracleValue = (Oracle.ManagedDataAccess.Types.OracleDecimal)outParam.Value;
            return oracleValue.ToInt32() == 1;

            }

        /* =========================================================
           REPORT OVERVIEW
        ========================================================= */

        public FieldAuditReportOverviewModel GetFieldAuditReportOverview(int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_REPORT_OVERVIEW";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
                {
                columns.Add(reader.GetName(i));
                }

            string GetString(params string[] names)
                {
                foreach (var name in names)
                    {
                    if (!columns.Contains(name))
                        {
                        continue;
                        }

                    var value = reader[name];
                    if (value == DBNull.Value)
                        {
                        return string.Empty;
                        }

                    return value.ToString();
                    }

                return string.Empty;
                }

            DateTime? GetDate(params string[] names)
                {
                foreach (var name in names)
                    {
                    if (!columns.Contains(name))
                        {
                        continue;
                        }

                    var value = reader[name];
                    if (value == DBNull.Value)
                        {
                        return null;
                        }

                    return Convert.ToDateTime(value);
                    }

                return null;
                }

            int? GetInt(params string[] names)
                {
                foreach (var name in names)
                    {
                    if (!columns.Contains(name))
                        {
                        continue;
                        }

                    var value = reader[name];
                    if (value == DBNull.Value)
                        {
                        return null;
                        }

                    return Convert.ToInt32(value);
                    }

                return null;
                }

            return new FieldAuditReportOverviewModel
                {
                EngId = GetInt("ENG_ID") ?? 0,
                EntityId = GetInt("ENTITY_ID") ?? 0,
                EntityCode = GetString("ENTITY_CODE"),
                EntityName = GetString("ENTITY_NAME"),
                AuditPeriod = GetString("AUDIT_PERIOD"),
                AuditStartDate = GetDate("AUDIT_STARTDATE"),
                AuditEndDate = GetDate("AUDIT_ENDDATE"),
                TeamName = GetString("TEAM_NAME"),
                VersionNo = GetString("VERSION_NO"),
                GeneratedOn = GetDate("GENERATED_ON"),
                GeneratedBy = GetString("GENERATED_BY"),
                FinalizedOn = GetDate("FINALIZED_ON"),
                FinalizedBy = GetString("FINALIZED_BY"),
                AUDIT_YEAR = GetString("AUDIT_YEAR", "AUDIT_PERIOD"),
                REPORTING_OFFICE = GetString("REPORTING_OFFICE"),
                ENTITY_NAME = GetString("ENTITY_NAME"),
                OPERATION_STARTDATE = GetDate("OPERATION_STARTDATE", "AUDIT_STARTDATE"),
                OPERATION_ENDDATE = GetDate("OPERATION_ENDDATE", "AUDIT_ENDDATE"),
                AUDIT_STARTED_ON = GetDate("AUDIT_STARTED_ON", "AUDIT_STARTDATE"),
                TEAM_EXIST = GetDate("TEAM_EXIST"),
                TOTAL_MEMBERS = GetInt("TOTAL_MEMBERS"),
                TEAM_LEAD = GetString("TEAM_LEAD", "TEAM_NAME")
                };
            }

        /* =========================================================
           NARRATIVE SECTIONS
        ========================================================= */

        public List<FieldAuditNarrativeSectionModel> GetFieldAuditNarrativeSections(int engId)
            {
            var list = new List<FieldAuditNarrativeSectionModel>();

            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_NARRATIVE_SECTIONS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new FieldAuditNarrativeSectionModel
                    {
                    SectionCode = reader["SECTION_CODE"] == DBNull.Value ? string.Empty : reader["SECTION_CODE"].ToString(),
                    SectionTitle = reader["SECTION_TITLE"] == DBNull.Value ? string.Empty : reader["SECTION_TITLE"].ToString(),
                    DisplayOrder = reader["DISPLAY_ORDER"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DISPLAY_ORDER"]),
                    IsMandatory = reader["IS_MANDATORY"] != DBNull.Value && reader["IS_MANDATORY"].ToString() == "Y",
                    TextBlock = reader["TEXT_BLOCK"] == DBNull.Value ? string.Empty : reader["TEXT_BLOCK"].ToString()
                    });
                }

            return list;
            }

        public void SaveFieldAuditTextBlock(int engId, string sectionCode, string text)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_SAVE_TEXT_BLOCK";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_SECTION_CODE", OracleDbType.Varchar2).Value = sectionCode ?? string.Empty;
            cmd.Parameters.Add("P_TEXT_BLOCK", OracleDbType.Clob).Value = text ?? string.Empty;

            cmd.ExecuteNonQuery();
            }

        /* =========================================================
           KPI SNAPSHOT
        ========================================================= */

        public List<KpiSnapshotRowModel> GetFieldAuditKpiSnapshots(int engId)
            {
            var list = new List<KpiSnapshotRowModel>();

            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_KPI_SNAPSHOT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new KpiSnapshotRowModel
                    {
                    KpiCode = reader["KPI_CODE"] == DBNull.Value ? string.Empty : reader["KPI_CODE"].ToString(),
                    KpiLabel = reader["KPI_LABEL"] == DBNull.Value ? string.Empty : reader["KPI_LABEL"].ToString(),
                    PeriodEnd = reader["PERIOD_END"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PERIOD_END"]),
                    ActualValue = reader["ACTUAL_VALUE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["ACTUAL_VALUE"]),
                    TargetValue = reader["TARGET_VALUE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TARGET_VALUE"]),
                    Unit = reader["UNIT"] == DBNull.Value ? string.Empty : reader["UNIT"].ToString()
                    });
                }

            return list;
            }

        public void SaveFieldAuditKpiSnapshot(int engId, KpiSnapshotRowModel row)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();
            var entityId = GetFieldAuditEntityId(activeEngagementId);

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_SAVE_KPI_SNAPSHOT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = entityId;
            cmd.Parameters.Add("P_KPI_CODE", OracleDbType.Varchar2).Value = row?.KpiCode ?? string.Empty;
            cmd.Parameters.Add("P_KPI_LABEL", OracleDbType.Varchar2).Value = row?.KpiLabel ?? string.Empty;
            cmd.Parameters.Add("P_PERIOD_END", OracleDbType.Date).Value = row?.PeriodEnd ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_ACTUAL_VALUE", OracleDbType.Decimal).Value = row?.ActualValue ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_TARGET_VALUE", OracleDbType.Decimal).Value = row?.TargetValue ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_UNIT", OracleDbType.Varchar2).Value = row?.Unit ?? string.Empty;

            cmd.ExecuteNonQuery();
            }

        public List<FieldAuditPdfStatisticsRowModel> GetFieldAuditStatistics(int engId)
            {
            var rows = new List<FieldAuditPdfStatisticsRowModel>();
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_PDF_STATISTICS";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_REPORT_VERSION", OracleDbType.Int32).Value = DBNull.Value;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new FieldAuditPdfStatisticsRowModel
                    {
                    RiskLevel = reader["RISK_LEVEL"] == DBNull.Value ? string.Empty : reader["RISK_LEVEL"].ToString(),
                    ReportedCount = reader["REPORTED_COUNT"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["REPORTED_COUNT"]),
                    RectifiedCount = reader["RECTIFIED_COUNT"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RECTIFIED_COUNT"]),
                    OutstandingCount = reader["OUTSTANDING_COUNT"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OUTSTANDING_COUNT"])
                    });
                }

            return rows;
            }

        public List<GetTeamDetailsModel> GetFieldAuditTeamDetails(int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();
            return GetTeamDetails(activeEngagementId);
            }

        /* =========================================================
           NPL SNAPSHOT
        ========================================================= */

        public List<NplSnapshotRowModel> GetFieldAuditNplSnapshots(int engId)
            {
            var list = new List<NplSnapshotRowModel>();

            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_NPL_SNAPSHOT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new NplSnapshotRowModel
                    {
                    Category = reader["CATEGORY"] == DBNull.Value ? string.Empty : reader["CATEGORY"].ToString(),
                    PeriodEnd = reader["PERIOD_END"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PERIOD_END"]),
                    CaseCount = reader["CASE_COUNT"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CASE_COUNT"]),
                    OutstandingAmount = reader["OUTSTANDING_AMOUNT"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["OUTSTANDING_AMOUNT"]),
                    ProvisionAmount = reader["PROVISION_AMOUNT"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["PROVISION_AMOUNT"])
                    });
                }

            return list;
            }

        public void SaveFieldAuditNplSnapshot(int engId, NplSnapshotRowModel row)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_SAVE_NPL_SNAPSHOT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_CATEGORY", OracleDbType.Varchar2).Value = row?.Category ?? string.Empty;
            cmd.Parameters.Add("P_PERIOD_END", OracleDbType.Date).Value = row?.PeriodEnd ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_CASE_COUNT", OracleDbType.Int32).Value = row?.CaseCount ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_OUTSTANDING_AMOUNT", OracleDbType.Decimal).Value = row?.OutstandingAmount ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_PROVISION_AMOUNT", OracleDbType.Decimal).Value = row?.ProvisionAmount ?? (object)DBNull.Value;

            cmd.ExecuteNonQuery();
            }

        /* =========================================================
           STAFF SNAPSHOT
        ========================================================= */

        public List<StaffSnapshotRowModel> GetFieldAuditStaffSnapshots(int engId)
            {
            var list = new List<StaffSnapshotRowModel>();

            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_GET_STAFF_SNAPSHOT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                list.Add(new StaffSnapshotRowModel
                    {
                    Designation = reader["DESIGNATION"] == DBNull.Value ? string.Empty : reader["DESIGNATION"].ToString(),
                    StaffCount = reader["STAFF_COUNT"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["STAFF_COUNT"]),
                    AsOfDate = reader["AS_OF_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["AS_OF_DATE"])
                    });
                }

            return list;
            }

        public void SaveFieldAuditStaffSnapshot(int engId, StaffSnapshotRowModel row)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_SAVE_STAFF_SNAPSHOT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.Parameters.Add("P_DESIGNATION", OracleDbType.Varchar2).Value = row?.Designation ?? string.Empty;
            cmd.Parameters.Add("P_STAFF_COUNT", OracleDbType.Int32).Value = row?.StaffCount ?? (object)DBNull.Value;
            cmd.Parameters.Add("P_AS_OF_DATE", OracleDbType.Date).Value = row?.AsOfDate ?? (object)DBNull.Value;

            cmd.ExecuteNonQuery();
            }


        public FieldAuditReportChecklistModel GetFieldAuditReportChecklist(int engId)
            {
            var sections = GetFieldAuditNarrativeSections(engId);
            bool HasCompletionFlag(string sectionCode)
                {
                return sections.Any(section =>
                    string.Equals(section.SectionCode, sectionCode, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(section.TextBlock));
                }

            return new FieldAuditReportChecklistModel
                {
                HasKpiSnapshot = HasCompletionFlag(FieldAuditReportSectionCodes.KpiSnapshot),
                HasNplSnapshot = HasCompletionFlag(FieldAuditReportSectionCodes.NplSnapshot),
                HasStaffSnapshot = HasCompletionFlag(FieldAuditReportSectionCodes.StaffSnapshot),
                MandatoryNarrativesComplete = sections
                    .Where(section => section.IsMandatory)
                    .All(section => !string.IsNullOrWhiteSpace(section.TextBlock))
                };
            }

        public bool HasFieldAuditReportData(int engId)
            {
            var sections = GetFieldAuditNarrativeSections(engId);
            if (sections.Any(section => !string.IsNullOrWhiteSpace(section.TextBlock)))
                {
                return true;
                }

            return HasFieldAuditRows("T_FRPT_KPI_SNAPSHOT", engId)
                || HasFieldAuditRows("T_FRPT_NPL_SNAPSHOT", engId)
                || HasFieldAuditRows("T_FRPT_STAFF_SNAPSHOT", engId);
            }

        private bool HasFieldAuditRows(string tableName, int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = $@"
                                SELECT COUNT(1)
                                FROM {tableName}
                                WHERE ENG_ID = :P_ENG_ID";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
            }

        private int GetFieldAuditEntityId(int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();
            var overview = GetFieldAuditReportOverview(activeEngagementId);
            return overview?.EntityId ?? 0;
            }



        /* =========================================================
           FINALIZE
        ========================================================= */

        public void FinalizeFieldAuditReport(int engId)
            {
            var sessionHandler = CreateSessionHandler();
            var activeEngagementId = sessionHandler.GetActiveEngagementIdOrThrow();

            using var con = DatabaseConnection();
            EnsureConnectionOpen(con);

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_FRPT.P_FINALIZE_REPORT";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = activeEngagementId;
            cmd.ExecuteNonQuery();
            }

        private static void EnsureConnectionOpen(OracleConnection connection)
            {
            if (connection.State != ConnectionState.Open)
                {
                connection.Open();
                }
            }
        }
    }

using AIS.Models.Notifications;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public List<ManagementAuditWeeklyDivisionSummaryModel> GetManagementAuditWeeklyDivisions(
            DateTime fromDate, DateTime toDate)
            {
            var divisions = new List<ManagementAuditWeeklyDivisionSummaryModel>();
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_WEEKLY.P_GET_MGMT_WEEKLY_DIVISIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate;
            cmd.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                divisions.Add(new ManagementAuditWeeklyDivisionSummaryModel
                    {
                    DivisionId = Convert.ToInt32(reader["DIVISION_ID"]),
                    DivisionName = reader["DIVISION_NAME"]?.ToString() ?? string.Empty,
                    ToEmail = reader["TO_EMAIL"]?.ToString() ?? string.Empty,
                    CcEmail = reader["CC_EMAIL"]?.ToString() ?? string.Empty,
                    BccEmail = reader["BCC_EMAIL"]?.ToString() ?? string.Empty,
                    SettledCount = Convert.ToInt32(reader["SETTLED_COUNT"]),
                    RejectedCount = Convert.ToInt32(reader["REJECTED_COUNT"]),
                    NoComplianceCount = Convert.ToInt32(reader["NO_COMPLIANCE_COUNT"]),
                    TotalCount = Convert.ToInt32(reader["TOTAL_COUNT"])
                    });
                }

            return divisions;
            }

        public List<ManagementAuditWeeklyDivisionDetailModel> GetManagementAuditWeeklyDivisionData(
            int divisionId, DateTime fromDate, DateTime toDate)
            {
            var details = new List<ManagementAuditWeeklyDivisionDetailModel>();
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_WEEKLY.P_GET_MGMT_WEEKLY_DIVISION_DATA";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_DIVISION_ID", OracleDbType.Int32).Value = divisionId;
            cmd.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate;
            cmd.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                details.Add(new ManagementAuditWeeklyDivisionDetailModel
                    {
                    DecisionHistoryId = Convert.ToInt32(reader["DECISION_HISTORY_ID"]),
                    ComId = Convert.ToInt32(reader["COM_ID"]),
                    ComCycle = Convert.ToInt32(reader["COM_CYCLE"]),
                    EntityId = Convert.ToInt32(reader["ENTITY_ID"]),
                    AuditedBy = Convert.ToInt32(reader["AUDITED_BY"]),
                    DivisionId = Convert.ToInt32(reader["DIVISION_ID"]),
                    DivisionName = reader["DIVISION_NAME"]?.ToString() ?? string.Empty,
                    EntityName = reader["ENTITY_NAME"]?.ToString() ?? string.Empty,
                    AuditPeriod = reader["AUDIT_PERIOD"]?.ToString() ?? string.Empty,
                    ParaNo = reader["PARA_NO"]?.ToString() ?? string.Empty,
                    Title = reader["TITLE"]?.ToString() ?? string.Empty,
                    SubmittedOn = reader["SUBMITTED_ON"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["SUBMITTED_ON"]),
                    DecisionOn = Convert.ToDateTime(reader["DECISION_ON"]),
                    DecisionStatus = reader["DECISION_STATUS"]?.ToString() ?? string.Empty,
                    ComStatus = Convert.ToInt32(reader["COM_STATUS"]),
                    Reason = reader["REASON"]?.ToString() ?? string.Empty
                    });
                }

            return details;
            }

        public List<ManagementAuditWeeklyNoComplianceModel> GetManagementAuditWeeklyNoCompliance(
            int divisionId, DateTime fromDate, DateTime toDate)
            {
            var details = new List<ManagementAuditWeeklyNoComplianceModel>();
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_WEEKLY.P_GET_MGMT_WEEKLY_NO_COMPLIANCE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_DIVISION_ID", OracleDbType.Int32).Value = divisionId;
            cmd.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate;
            cmd.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                details.Add(new ManagementAuditWeeklyNoComplianceModel
                    {
                    ComId = ReadManagementAuditWeeklyInt(reader, "COM_ID"),
                    ComCycle = ReadManagementAuditWeeklyInt(reader, "COM_CYCLE"),
                    EntityId = ReadManagementAuditWeeklyInt(reader, "ENTITY_ID"),
                    AuditedBy = ReadManagementAuditWeeklyInt(reader, "AUDITED_BY"),
                    DivisionId = ReadManagementAuditWeeklyInt(reader, "DIVISION_ID"),
                    DivisionName = ReadManagementAuditWeeklyString(reader, "DIVISION_NAME"),
                    EntityName = ReadManagementAuditWeeklyString(reader, "ENTITY_NAME"),
                    AuditPeriod = ReadManagementAuditWeeklyString(reader, "AUDIT_PERIOD"),
                    ParaNo = ReadManagementAuditWeeklyString(reader, "PARA_NO"),
                    Title = ReadManagementAuditWeeklyString(reader, "TITLE"),
                    Risk = ReadManagementAuditWeeklyString(reader, "RISK"),                    
                    ParaStatus = ReadManagementAuditWeeklyNullableInt(reader, "PARA_STATUS"),
                    ComStatus = ReadManagementAuditWeeklyNullableInt(reader, "COM_STATUS"),
                    ComStage = ReadManagementAuditWeeklyNullableInt(reader, "COM_STAGE")
                    });
                }

            return details;
            }

        public List<ManagementAuditNotificationDivisionModel> GetManagementAuditNotificationConfigurations()
            {
            var divisions = new List<ManagementAuditNotificationDivisionModel>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_NOTIFY_ADMIN.P_GET_CONFIGURATIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("IO_DIVISIONS", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("IO_RECIPIENTS", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                divisions.Add(new ManagementAuditNotificationDivisionModel
                    {
                    DivisionId = ReadManagementAuditWeeklyInt(reader, "DIVISION_ID"),
                    DivisionName = ReadManagementAuditWeeklyString(reader, "DIVISION_NAME"),
                    ReportingOffice = ReadManagementAuditWeeklyString(reader, "REPORTING_OFFICE"),
                    IsActive = IsManagementAuditNotificationEnabled(reader, "IS_ACTIVE"),
                    WeeklyEmailEnabled = IsManagementAuditNotificationEnabled(reader, "WEEKLY_EMAIL_ENABLED"),
                    EffectiveFrom = ReadManagementAuditWeeklyDate(reader, "EFFECTIVE_FROM") ?? DateTime.Today,
                    EffectiveTo = ReadManagementAuditWeeklyDate(reader, "EFFECTIVE_TO"),
                    Remarks = ReadManagementAuditWeeklyString(reader, "REMARKS"),
                    CreatedBy = ReadManagementAuditWeeklyString(reader, "CREATED_BY"),
                    CreatedOn = ReadManagementAuditWeeklyDate(reader, "CREATED_ON"),
                    UpdatedBy = ReadManagementAuditWeeklyString(reader, "UPDATED_BY"),
                    UpdatedOn = ReadManagementAuditWeeklyDate(reader, "UPDATED_ON")
                    });
                }

            if (reader.NextResult())
                {
                while (reader.Read())
                    {
                    var divisionId = ReadManagementAuditWeeklyInt(reader, "DIVISION_ID");
                    var division = divisions.FirstOrDefault(item => item.DivisionId == divisionId);
                    if (division == null)
                        {
                        continue;
                        }

                    division.Recipients.Add(new ManagementAuditNotificationRecipientModel
                        {
                        RecipientId = ReadManagementAuditWeeklyInt(reader, "RECIPIENT_ID"),
                        DivisionId = divisionId,
                        RecipientType = ReadManagementAuditWeeklyString(reader, "RECIPIENT_TYPE"),
                        EmailAddress = ReadManagementAuditWeeklyString(reader, "EMAIL_ADDRESS"),
                        PersonName = ReadManagementAuditWeeklyString(reader, "PERSON_NAME"),
                        Designation = ReadManagementAuditWeeklyString(reader, "DESIGNATION"),
                        IsActive = IsManagementAuditNotificationEnabled(reader, "IS_ACTIVE"),
                        EffectiveFrom = ReadManagementAuditWeeklyDate(reader, "EFFECTIVE_FROM") ?? DateTime.Today,
                        EffectiveTo = ReadManagementAuditWeeklyDate(reader, "EFFECTIVE_TO"),
                        DisplayOrder = ReadManagementAuditWeeklyInt(reader, "DISPLAY_ORDER"),
                        CreatedBy = ReadManagementAuditWeeklyString(reader, "CREATED_BY"),
                        CreatedOn = ReadManagementAuditWeeklyDate(reader, "CREATED_ON"),
                        UpdatedBy = ReadManagementAuditWeeklyString(reader, "UPDATED_BY"),
                        UpdatedOn = ReadManagementAuditWeeklyDate(reader, "UPDATED_ON")
                        });
                    }
                }

            return divisions;
            }

        public List<ManagementAuditNotificationDivisionOptionModel> GetManagementAuditNotificationDivisionOptions()
            {
            var divisions = new List<ManagementAuditNotificationDivisionOptionModel>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_NOTIFY_ADMIN.P_GET_DIVISION_OPTIONS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("IO_DIVISIONS", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                divisions.Add(new ManagementAuditNotificationDivisionOptionModel
                    {
                    DivisionId = ReadManagementAuditWeeklyInt(reader, "DIVISION_ID"),
                    DivisionName = ReadManagementAuditWeeklyString(reader, "DIVISION_NAME").Trim()
                    });
                }

            return divisions;
            }

        public void SaveManagementAuditNotificationDivision(
            SaveManagementAuditNotificationDivisionModel model, string updatedBy)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_NOTIFY_ADMIN.P_SAVE_DIVISION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_DIVISION_ID", OracleDbType.Int32).Value = model.DivisionId;
            cmd.Parameters.Add("P_DIVISION_NAME", OracleDbType.Varchar2, 200).Value = model.DivisionName.Trim();
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Char, 1).Value = model.IsActive ? "Y" : "N";
            cmd.Parameters.Add("P_WEEKLY_EMAIL_ENABLED", OracleDbType.Char, 1).Value = model.WeeklyEmailEnabled ? "Y" : "N";
            cmd.Parameters.Add("P_EFFECTIVE_FROM", OracleDbType.Date).Value = model.EffectiveFrom.Value;
            cmd.Parameters.Add("P_EFFECTIVE_TO", OracleDbType.Date).Value = model.EffectiveTo.HasValue ? model.EffectiveTo.Value : DBNull.Value;
            cmd.Parameters.Add("P_REMARKS", OracleDbType.Varchar2, 1000).Value = string.IsNullOrWhiteSpace(model.Remarks) ? DBNull.Value : model.Remarks.Trim();
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = updatedBy;
            cmd.ExecuteNonQuery();
            }

        public int SaveManagementAuditNotificationRecipient(
            SaveManagementAuditNotificationRecipientModel model, string updatedBy)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_NOTIFY_ADMIN.P_SAVE_RECIPIENT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_RECIPIENT_ID", OracleDbType.Int32).Value = model.RecipientId.HasValue ? model.RecipientId.Value : DBNull.Value;
            cmd.Parameters.Add("P_DIVISION_ID", OracleDbType.Int32).Value = model.DivisionId;
            cmd.Parameters.Add("P_RECIPIENT_TYPE", OracleDbType.Varchar2, 10).Value = model.RecipientType.Trim().ToUpperInvariant();
            cmd.Parameters.Add("P_EMAIL_ADDRESS", OracleDbType.Varchar2, 500).Value = model.EmailAddress.Trim();
            cmd.Parameters.Add("P_PERSON_NAME", OracleDbType.Varchar2, 200).Value = string.IsNullOrWhiteSpace(model.PersonName) ? DBNull.Value : model.PersonName.Trim();
            cmd.Parameters.Add("P_DESIGNATION", OracleDbType.Varchar2, 200).Value = string.IsNullOrWhiteSpace(model.Designation) ? DBNull.Value : model.Designation.Trim();
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Char, 1).Value = model.IsActive ? "Y" : "N";
            cmd.Parameters.Add("P_EFFECTIVE_FROM", OracleDbType.Date).Value = model.EffectiveFrom.Value;
            cmd.Parameters.Add("P_EFFECTIVE_TO", OracleDbType.Date).Value = model.EffectiveTo.HasValue ? model.EffectiveTo.Value : DBNull.Value;
            cmd.Parameters.Add("P_DISPLAY_ORDER", OracleDbType.Int32).Value = model.DisplayOrder;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = updatedBy;
            var savedId = cmd.Parameters.Add("P_SAVED_RECIPIENT_ID", OracleDbType.Int32);
            savedId.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return Convert.ToInt32(savedId.Value.ToString());
            }

        public void RemoveManagementAuditNotificationRecipient(int recipientId, string updatedBy)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_MGMT_AUDIT_NOTIFY_ADMIN.P_REMOVE_RECIPIENT";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_RECIPIENT_ID", OracleDbType.Int32).Value = recipientId;
            cmd.Parameters.Add("P_UPDATED_BY", OracleDbType.Varchar2, 100).Value = updatedBy;
            cmd.ExecuteNonQuery();
            }

        private static int ReadManagementAuditWeeklyInt(OracleDataReader reader, string columnName)
            {
            return reader[columnName] == DBNull.Value ? 0 : Convert.ToInt32(reader[columnName]);
            }

        private static int? ReadManagementAuditWeeklyNullableInt(OracleDataReader reader, string columnName)
            {
            return reader[columnName] == DBNull.Value ? null : Convert.ToInt32(reader[columnName]);
            }

        private static DateTime? ReadManagementAuditWeeklyDate(OracleDataReader reader, string columnName)
            {
            return reader[columnName] == DBNull.Value ? null : Convert.ToDateTime(reader[columnName]);
            }

        private static string ReadManagementAuditWeeklyString(OracleDataReader reader, string columnName)
            {
            return reader[columnName] == DBNull.Value ? string.Empty : reader[columnName].ToString();
            }

        private static bool IsManagementAuditNotificationEnabled(OracleDataReader reader, string columnName)
            {
            return string.Equals(ReadManagementAuditWeeklyString(reader, columnName), "Y", StringComparison.OrdinalIgnoreCase);
            }
        }
    }

using AIS.Models.Notifications;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

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
                    SettledCount = Convert.ToInt32(reader["SETTLED_COUNT"]),
                    RejectedCount = Convert.ToInt32(reader["REJECTED_COUNT"]),
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
        }
    }

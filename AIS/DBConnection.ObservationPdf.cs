using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public ObservationPdfDataModel GetObservationPdfData(int obsId)
            {
            var details = GetObservationPrintDetails(obsId);
            return details ?? new ObservationPdfDataModel();
            }

        public ObservationPdfDataModel GetObservationPrintDetails(int obsId)
            {
            var result = new ObservationPdfDataModel();
            using var con = DatabaseConnection();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "pkg_ar.P_GET_OBSERVATION_TO_PRINT";

            cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = obsId;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.EntityName = GetString(reader, "ENTITY_NAME");
                result.AuditPeriod = GetString(reader, "AUDIT_PERIOD");
                result.MemoNumber = GetString(reader, "MEMO_NUMBER");
                result.MemoDate = GetNullableDate(reader, "MEMO_DATE");
                result.Annexure = GetString(reader, "ANNEXURE");
                result.Title = GetString(reader, "TITLE");
                result.Risk = GetString(reader, "RISK");
                result.ParaText = GetString(reader, "PARA_TEXT");
                result.ReferenceId = GetNullableLong(reader, "REFERENCE_ID");
                result.TeamLead = GetString(reader, "TEAM_LEAD");
                break;
                }

            return result;
            }

        public List<ObservationPdfResponsibilityModel> GetObservationPrintResponsibilities(int obsId, int engId)
            {
            var results = new List<ObservationPdfResponsibilityModel>();
            using var con = DatabaseConnection();

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "pkg_ae.P_GetObservationResponsible";

            cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = obsId;
            cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                var responsibility = new ObservationPdfResponsibilityModel
                    {
                    PpNo = reader["PP_NO"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PP_NO"]),
                    EmployeeName = GetString(reader, "EMP_NAME"),
                    LoanCase = GetString(reader, "LOANCASE"),
                    LcAmount = GetString(reader, "LCAMOUNT"),
                    AccountNumber = GetString(reader, "ACCNUMBER"),
                    AcAmount = GetString(reader, "ACAMOUNT")
                    };

                if (responsibility.PpNo.HasValue
                    || !string.IsNullOrWhiteSpace(responsibility.EmployeeName)
                    || !string.IsNullOrWhiteSpace(responsibility.LoanCase)
                    || !string.IsNullOrWhiteSpace(responsibility.LcAmount)
                    || !string.IsNullOrWhiteSpace(responsibility.AccountNumber)
                    || !string.IsNullOrWhiteSpace(responsibility.AcAmount))
                    {
                    results.Add(responsibility);
                    }
                }

            return results;
            }

        private static long? GetNullableLong(IDataRecord reader, string columnName)
            {
            for (var i = 0; i < reader.FieldCount; i++)
                {
                if (!string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                    continue;
                    }

                return reader.IsDBNull(i) ? (long?)null : Convert.ToInt64(reader.GetValue(i));
                }

            return null;
            }
        }
    }

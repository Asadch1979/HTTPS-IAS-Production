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
            GuardAgainstDynamicSql(cmd);

            cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = obsId;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                result.EntityName = GetOptionalString(reader, "ENTITY_NAME");
                result.AuditPeriod = GetOptionalString(reader, "AUDIT_PERIOD", "PERIOD");
                result.MemoNumber = GetOptionalString(reader, "MEMO_NUMBER", "MEMO_NO");
                result.MemoDate = GetOptionalDate(reader, "MEMO_DATE");
                result.Annexure = GetOptionalString(reader, "ANNEXURE", "ANNEX");
                result.Title = GetOptionalString(reader, "TITLE", "HEADINGS", "HEADING", "OBS_HEADING");
                result.Risk = GetOptionalString(reader, "RISK", "OBS_RISK", "SEVERITY");
                result.ParaText = GetOptionalString(reader, "PARA_TEXT", "TEXT", "OBS_TEXT");
                result.ReferenceId = GetNullableLong(reader, "REFERENCE_ID");
                result.TeamLead = GetOptionalString(reader, "TEAM_LEAD", "MEMBER_NAME");
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
            cmd.BindByName = true;
            cmd.CommandText = "pkg_ae.P_GetObservationResponsible";
            GuardAgainstDynamicSql(cmd);

            cmd.Parameters.Add("OBSID", OracleDbType.Int32).Value = obsId;
            cmd.Parameters.Add("E_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                var responsibility = new ObservationPdfResponsibilityModel
                    {
                    PpNo = GetOptionalInt(reader, "PP_NO", "PPNO"),
                    EmployeeName = GetOptionalString(reader, "EMP_NAME", "EMPLOYEE_NAME"),
                    LoanCase = GetOptionalString(reader, "LOANCASE", "LOAN_CASE"),
                    LcAmount = GetOptionalString(reader, "LCAMOUNT", "LC_AMOUNT"),
                    AccountNumber = GetOptionalString(reader, "ACCNUMBER", "ACCOUNT_NUMBER"),
                    AcAmount = GetOptionalString(reader, "ACAMOUNT", "ACC_AMOUNT")
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

        private static string GetOptionalString(IDataRecord reader, params string[] columnNames)
            {
            var ordinal = GetOrdinal(reader, columnNames);
            if (!ordinal.HasValue || reader.IsDBNull(ordinal.Value))
                {
                return string.Empty;
                }

            return Convert.ToString(reader.GetValue(ordinal.Value)) ?? string.Empty;
            }

        private static DateTime? GetOptionalDate(IDataRecord reader, params string[] columnNames)
            {
            var ordinal = GetOrdinal(reader, columnNames);
            if (!ordinal.HasValue || reader.IsDBNull(ordinal.Value))
                {
                return null;
                }

            return Convert.ToDateTime(reader.GetValue(ordinal.Value));
            }

        private static int? GetOptionalInt(IDataRecord reader, params string[] columnNames)
            {
            var ordinal = GetOrdinal(reader, columnNames);
            if (!ordinal.HasValue || reader.IsDBNull(ordinal.Value))
                {
                return null;
                }

            return Convert.ToInt32(reader.GetValue(ordinal.Value));
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

        private static int? GetOrdinal(IDataRecord reader, params string[] columnNames)
            {
            if (reader == null || columnNames == null || columnNames.Length == 0)
                {
                return null;
                }

            for (var i = 0; i < reader.FieldCount; i++)
                {
                var fieldName = reader.GetName(i);
                foreach (var columnName in columnNames)
                    {
                    if (string.Equals(fieldName, columnName, StringComparison.OrdinalIgnoreCase))
                        {
                        return i;
                        }
                    }
                }

            return null;
            }
        }
    }

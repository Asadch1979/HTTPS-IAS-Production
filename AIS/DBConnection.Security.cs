using AIS.Models.Security;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public long SaveCspViolation(CspViolationDto dto, long? createdByPpNo)
            {
            if (dto == null)
                {
                throw new ArgumentNullException(nameof(dto));
                }

            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_SEC.P_SAVE_CSP_VIOLATION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_disposition", OracleDbType.Varchar2).Value = ToDbValue(dto.Disposition, 100);
            cmd.Parameters.Add("p_document_uri", OracleDbType.Varchar2).Value = ToDbValue(dto.DocumentUri, 1000);
            cmd.Parameters.Add("p_referrer", OracleDbType.Varchar2).Value = ToDbValue(dto.Referrer, 1000);
            cmd.Parameters.Add("p_blocked_uri", OracleDbType.Varchar2).Value = ToDbValue(dto.BlockedUri, 1000);
            cmd.Parameters.Add("p_effective_directive", OracleDbType.Varchar2).Value = ToDbValue(dto.EffectiveDirective, 200);
            cmd.Parameters.Add("p_violated_directive", OracleDbType.Varchar2).Value = ToDbValue(dto.ViolatedDirective, 200);
            cmd.Parameters.Add("p_original_policy", OracleDbType.Varchar2).Value = ToDbValue(dto.OriginalPolicy, 2000);
            cmd.Parameters.Add("p_source_file", OracleDbType.Varchar2).Value = ToDbValue(dto.SourceFile, 1000);
            cmd.Parameters.Add("p_line_number", OracleDbType.Int64).Value = ToDbNumber(dto.LineNumber);
            cmd.Parameters.Add("p_column_number", OracleDbType.Int64).Value = ToDbNumber(dto.ColumnNumber);
            cmd.Parameters.Add("p_status_code", OracleDbType.Int64).Value = ToDbNumber(dto.StatusCode);
            cmd.Parameters.Add("p_script_sample", OracleDbType.Varchar2).Value = ToDbValue(dto.ScriptSample, 2000);
            cmd.Parameters.Add("p_user_agent", OracleDbType.Varchar2).Value = ToDbValue(dto.UserAgent, 1000);
            cmd.Parameters.Add("p_client_ip", OracleDbType.Varchar2).Value = ToDbValue(dto.ClientIp, 200);
            cmd.Parameters.Add("p_raw_json", OracleDbType.Clob).Value = ToDbValue(dto.RawJson, 32000);
            cmd.Parameters.Add("p_created_by_pp_no", OracleDbType.Int64).Value = createdByPpNo.HasValue && createdByPpNo.Value > 0
                ? createdByPpNo.Value
                : DBNull.Value;

            var outputId = new OracleParameter("o_violation_id", OracleDbType.Int64)
                {
                Direction = ParameterDirection.Output
                };
            cmd.Parameters.Add(outputId);

            cmd.ExecuteNonQuery();

            if (outputId.Value == null || outputId.Value == DBNull.Value)
                {
                return 0;
                }

            return Convert.ToInt64(outputId.Value);
            }

        private static object ToDbValue(string value, int maxLength)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return DBNull.Value;
                }

            var trimmed = value.Trim();
            if (trimmed.Length > maxLength)
                {
                trimmed = trimmed.Substring(0, maxLength);
                }

            return trimmed;
            }

        private static object ToDbNumber(long? value)
            {
            return value.HasValue ? value.Value : DBNull.Value;
            }
        }
    }

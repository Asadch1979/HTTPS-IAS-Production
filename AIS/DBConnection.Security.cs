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

            using (OracleConnection con = DatabaseConnection(requireActiveSession: false))
            using (OracleCommand cmd = new OracleCommand("PKG_SEC.P_SAVE_CSP_VIOLATION", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_disposition", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.Disposition, 200));
                cmd.Parameters.Add("p_document_uri", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.DocumentUri, 2000));
                cmd.Parameters.Add("p_referrer", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.Referrer, 2000));
                cmd.Parameters.Add("p_blocked_uri", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.BlockedUri, 2000));
                cmd.Parameters.Add("p_effective_directive", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.EffectiveDirective, 1000));
                cmd.Parameters.Add("p_violated_directive", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.ViolatedDirective, 1000));
                cmd.Parameters.Add("p_original_policy", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.OriginalPolicy, 2000));
                cmd.Parameters.Add("p_source_file", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.SourceFile, 2000));
                cmd.Parameters.Add("p_line_number", OracleDbType.Int64).Value = DbValue(dto.LineNumber);
                cmd.Parameters.Add("p_column_number", OracleDbType.Int64).Value = DbValue(dto.ColumnNumber);
                cmd.Parameters.Add("p_status_code", OracleDbType.Int64).Value = DbValue(dto.StatusCode);
                cmd.Parameters.Add("p_script_sample", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.ScriptSample, 2000));
                cmd.Parameters.Add("p_user_agent", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.UserAgent, 2000));
                cmd.Parameters.Add("p_client_ip", OracleDbType.Varchar2).Value = DbValue(Truncate(dto.ClientIp, 200));
                cmd.Parameters.Add("p_raw_json", OracleDbType.Clob).Value = DbValue(dto.RawJson);
                cmd.Parameters.Add("p_created_by_pp_no", OracleDbType.Int64).Value = DbValue(createdByPpNo);

                var outViolationId = new OracleParameter("o_violation_id", OracleDbType.Int64)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outViolationId);

                GuardAgainstDynamicSql(cmd);
                cmd.ExecuteNonQuery();

                return outViolationId.Value == DBNull.Value || outViolationId.Value == null
                    ? 0L
                    : Convert.ToInt64(outViolationId.Value);
            }
        }

        private static object DbValue(object value)
        {
            return value ?? DBNull.Value;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength);
        }
    }
}

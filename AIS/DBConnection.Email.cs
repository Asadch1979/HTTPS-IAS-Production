using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public long LogEmailTriggerAttempt(string module, string triggerPoint, string referenceId, string toAddress, string ccAddress, string subject)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO T_AU_EMAIL_TRIGGER_LOG
                (ID, TRIGGER_DATE, MODULE, TRIGGER_POINT, REFERENCE_ID, TO_ADDRESS, CC_ADDRESS, EMAIL_SUBJECT, STATUS)
                VALUES (SEQ_T_AU_EMAIL_TRIGGER_LOG.NEXTVAL, SYSTIMESTAMP, :MODULE, :TRIGGER_POINT, :REFERENCE_ID, :TO_ADDRESS, :CC_ADDRESS, :EMAIL_SUBJECT, 'TRIGGERED')
                RETURNING ID INTO :LOG_ID";
            cmd.CommandType = CommandType.Text;
            cmd.BindByName = true;
            cmd.Parameters.Add("MODULE", OracleDbType.Varchar2).Value = DbValue(module, 100);
            cmd.Parameters.Add("TRIGGER_POINT", OracleDbType.Varchar2).Value = DbValue(triggerPoint, 200);
            cmd.Parameters.Add("REFERENCE_ID", OracleDbType.Varchar2).Value = DbValue(referenceId, 200);
            cmd.Parameters.Add("TO_ADDRESS", OracleDbType.Varchar2).Value = DbValue(toAddress, 2000);
            cmd.Parameters.Add("CC_ADDRESS", OracleDbType.Varchar2).Value = DbValue(ccAddress, 2000);
            cmd.Parameters.Add("EMAIL_SUBJECT", OracleDbType.Varchar2).Value = DbValue(subject, 1000);
            var id = cmd.Parameters.Add("LOG_ID", OracleDbType.Int64);
            id.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return Convert.ToInt64(id.Value.ToString());
            }

        public void CompleteEmailTriggerAttempt(long logId, string status, string errorMessage, bool sent)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"UPDATE T_AU_EMAIL_TRIGGER_LOG
                SET STATUS = :STATUS, ERROR_MESSAGE = :ERROR_MESSAGE,
                    SENT_ON = CASE WHEN :IS_SENT = 1 THEN SYSTIMESTAMP ELSE NULL END
                WHERE ID = :LOG_ID";
            cmd.CommandType = CommandType.Text;
            cmd.BindByName = true;
            cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = status;
            cmd.Parameters.Add("ERROR_MESSAGE", OracleDbType.Varchar2).Value = DbValue(errorMessage, 4000);
            cmd.Parameters.Add("IS_SENT", OracleDbType.Int32).Value = sent ? 1 : 0;
            cmd.Parameters.Add("LOG_ID", OracleDbType.Int64).Value = logId;
            cmd.ExecuteNonQuery();
            }

        private static object DbValue(string value, int maxLength)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return DBNull.Value;
                }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
            }
        }
    }

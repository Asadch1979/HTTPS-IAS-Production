using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        public void AddActivityLog(int entityId, int roleId, int ppNo, int pageId, string action, string unattend = "N")
        {
            using (OracleConnection con = DatabaseConnection(requireActiveSession: false))
            using (OracleCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "P_add_activity_log";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("v_entity_id", OracleDbType.Int32).Value = entityId;
                cmd.Parameters.Add("v_role_id", OracleDbType.Int32).Value = roleId;
                cmd.Parameters.Add("v_ppnum", OracleDbType.Int32).Value = ppNo;
                cmd.Parameters.Add("v_page_id", OracleDbType.Int32).Value = pageId;
                cmd.Parameters.Add("v_action", OracleDbType.Varchar2).Value = action;
                cmd.Parameters.Add("v_unattend", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(unattend) ? "N" : unattend;

                GuardAgainstDynamicSql(cmd);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

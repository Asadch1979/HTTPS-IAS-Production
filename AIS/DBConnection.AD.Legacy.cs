using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        // LEGACY / UNUSED / REPLACED DBCONNECTION METHODS

        // Original DBConnection file: AIS/DBConnection.AD.cs
        // Old package procedure: pkg_ad.P_UPDATE_COMPLIANCE_FLOW
        // Replacement DBConnection method/procedure: AddComplianceFlow -> pkg_ad.P_ADD_UPDATE_COMPLIANCE_FLOW
        // Controller still calls it: Only ApiCallsController_Legacy.update_compliance_flow, marked [NonAction].
        // Reason moved: No active JS/View caller found; active save/update flow is consolidated through add_compliance_flow.
        public string UpdateComplianceFlow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return string.Empty;
                }

            using var con = this.DatabaseConnection();
            string resp = "";

            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_ad.P_UPDATE_COMPLIANCE_FLOW";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                GuardAgainstDynamicSql(cmd);
                cmd.Parameters.Clear();
                cmd.Parameters.Add("F_ID", OracleDbType.Int32).Value = ID;
                cmd.Parameters.Add("TYPE_ID", OracleDbType.Int32).Value = ENTITY_TYPE_ID;
                cmd.Parameters.Add("GROUP_ID", OracleDbType.Int32).Value = GROUP_ID;
                cmd.Parameters.Add("P_GROUP_ID", OracleDbType.Int32).Value = PREV_GROUP_ID;
                cmd.Parameters.Add("N_GROUP_ID", OracleDbType.Int32).Value = NEXT_GROUP_ID;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    resp = rdr["remarks"].ToString();
                    }
                }

            return resp;
            }
        }
    }

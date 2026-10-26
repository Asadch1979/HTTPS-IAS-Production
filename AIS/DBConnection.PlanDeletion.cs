using AIS.Models.Planning;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        private (int EntityId, int PpNo, int RoleId) RequirePlanDeletionSession()
        {
            var user = CreateSessionHandler().GetUser();
            if (user == null || user.UserEntityID.GetValueOrDefault() <= 0 ||
                user.UserRoleID <= 0 || !int.TryParse(user.PPNumber, out var ppNo) || ppNo <= 0)
                throw new UnauthorizedAccessException("Active IAS session is required.");
            return (user.UserEntityID.Value, ppNo, user.UserRoleID);
        }

        public string RequestAuditPlanDeletion(int planId, string reason)
        {
            var actor = RequirePlanDeletionSession();
            if (planId <= 0 || string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length < 10 || reason.Length > 2000)
                throw new ArgumentException("Please enter a valid reason (10 to 2000 characters).");
            return CallPlanDeletionProcedure("PKG_HD.P_REQUEST_PLAN_DELETION",
                "P_PLAN_ID", planId, actor, reason.Trim());
        }

        public string AuthorizeAuditPlanDeletion(int requestId)
        {
            var actor = RequirePlanDeletionSession();
            if (requestId <= 0) throw new ArgumentException("Invalid request.");
            return CallPlanDeletionProcedure("PKG_HD.P_AUTHORIZE_PLAN_DELETION",
                "P_REQUEST_ID", requestId, actor, null);
        }

        private string CallPlanDeletionProcedure(
            string procedure, string idParameter, int id,
            (int EntityId, int PpNo, int RoleId) actor, string reason)
        {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procedure; // Fixed internal names, never a caller-supplied value.
            cmd.BindByName = true;
            cmd.Parameters.Add(idParameter, OracleDbType.Int32).Value = id;
            if (reason != null)
                cmd.Parameters.Add("P_REASON", OracleDbType.Varchar2, 2000).Value = reason;
            cmd.Parameters.Add("P_ENT_ID", OracleDbType.Int32).Value = actor.EntityId;
            cmd.Parameters.Add("P_PPNO", OracleDbType.Int32).Value = actor.PpNo;
            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = actor.RoleId;
            cmd.Parameters.Add("P_RESULT", OracleDbType.Varchar2, 64).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return cmd.Parameters["P_RESULT"].Value?.ToString() ?? "";
        }

        public List<AuditPlanDeletionRequestModel> GetAuditPlanDeletionRequests()
        {
            var actor = RequirePlanDeletionSession();
            var result = new List<AuditPlanDeletionRequestModel>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_HD.P_GET_PLAN_DELETION_REQUESTS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("P_ENT_ID", OracleDbType.Int32).Value = actor.EntityId;
            cmd.Parameters.Add("P_PPNO", OracleDbType.Int32).Value = actor.PpNo;
            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = actor.RoleId;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                result.Add(new AuditPlanDeletionRequestModel
                {
                    RequestId = Convert.ToInt32(rdr["REQUEST_ID"]),
                    PlanId = Convert.ToInt32(rdr["PLAN_ID"]),
                    AuditPeriodId = Convert.ToInt32(rdr["AUDITPERIODID"]),
                    EntityId = Convert.ToInt32(rdr["ENTITY_ID"]),
                    AuditedBy = Convert.ToInt32(rdr["AUDITEDBY"]),
                    EntityName = rdr["ENTITY_NAME"]?.ToString(),
                    PeriodName = rdr["PERIOD_NAME"]?.ToString(),
                    Reason = rdr["REASON"]?.ToString(),
                    RequestedBy = Convert.ToInt32(rdr["REQUESTED_BY"]),
                    RequestedOn = Convert.ToDateTime(rdr["REQUESTED_ON"])
                });
            }
            return result;
        }
    }
}

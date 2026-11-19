using AIS.Models;
using AIS.Session;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        private SessionUser RiskSession()
        {
            var user = sessionHandler.GetUserOrThrow();
            if (sessionHandler.IsSessionRevoked(user)) throw new UnauthenticatedException();
            if (user.ID <= 0 || user.UserRoleID <= 0 || user.UserGroupID <= 0 || user.UserEntityID.GetValueOrDefault() <= 0 || string.IsNullOrWhiteSpace(user.SessionId))
                throw new UnauthorizedAccessException();
            return new SessionUser { ID=user.ID,UserRoleID=user.UserRoleID,UserGroupID=user.UserGroupID,
                UserEntityID=user.UserEntityID,UserContextAssignmentId=user.UserContextAssignmentId,SessionId=user.SessionId };
        }
        private static void RiskParameter(OracleCommand cmd, string name, OracleDbType type, object value)
            => cmd.Parameters.Add(name, type).Value = value ?? DBNull.Value;
        private static void RiskContext(OracleCommand cmd, SessionUser user)
        {
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            RiskParameter(cmd,"p_user",OracleDbType.Int32,user.ID);
            RiskParameter(cmd,"p_role",OracleDbType.Int32,user.UserRoleID);
            RiskParameter(cmd,"p_entity",OracleDbType.Int32,user.UserEntityID);
            RiskParameter(cmd,"p_group",OracleDbType.Int32,user.UserGroupID);
            RiskParameter(cmd,"p_context",OracleDbType.Int32,user.UserContextAssignmentId);
            RiskParameter(cmd,"p_session",OracleDbType.Varchar2,user.SessionId);
        }
        private void RiskSessionUnchanged(SessionUser before)
        {
            var now = RiskSession();
            if (now.ID != before.ID || now.SessionId != before.SessionId || now.UserRoleID != before.UserRoleID ||
                now.UserEntityID != before.UserEntityID || now.UserGroupID != before.UserGroupID || now.UserContextAssignmentId != before.UserContextAssignmentId)
                throw new UnauthenticatedException();
        }
        private Dictionary<string, List<Dictionary<string, object>>> RiskRead(string procedure, string[] cursors, Action<OracleCommand> inputs)
        {
            var user = RiskSession();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            RiskContext(cmd,user);
            cmd.CommandText = "PKG_FAD_RISK_ASSESS." + procedure;
            inputs?.Invoke(cmd);
            foreach (var cursor in cursors) cmd.Parameters.Add("o_"+cursor,OracleDbType.RefCursor).Direction=ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            var result = new Dictionary<string,List<Dictionary<string,object>>>();
            foreach (var name in cursors)
            {
                using var cursor = (OracleRefCursor)cmd.Parameters["o_"+name].Value;
                using var reader = cursor.GetDataReader();
                var rows = new List<Dictionary<string,object>>();
                while (reader.Read())
                {
                    var row = new Dictionary<string,object>();
                    for (int i=0;i<reader.FieldCount;i++)
                    {
                        object value = null;
                        if (!reader.IsDBNull(i))
                        {
                            if (reader.GetDataTypeName(i).Equals("CLOB",StringComparison.OrdinalIgnoreCase))
                            { using var lob=reader.GetOracleClob(i); value=lob.Value; }
                            else value=reader.GetValue(i);
                        }
                        row[reader.GetName(i).ToLowerInvariant()] = value;
                    }
                    rows.Add(row);
                }
                result[name]=rows;
            }
            RiskSessionUnchanged(user);
            return result;
        }
        public Dictionary<string,List<Dictionary<string,object>>> RiskAssessmentCatalog()
            => RiskRead("P_CATALOG",new[]{"versions","assessments"},null);
        public (bool Prepare, bool Review) RiskAssessmentCapabilities()
        {
            var user=RiskSession();
            using var con=DatabaseConnection();
            using var cmd=con.CreateCommand();
            cmd.BindByName=true;
            cmd.CommandText="SELECT can_prepare,can_review FROM T_FAD_RISK_ACCESS WHERE group_id=:grp AND audit_entity_id=:ent";
            RiskParameter(cmd,"grp",OracleDbType.Int32,user.UserGroupID);
            RiskParameter(cmd,"ent",OracleDbType.Int32,user.UserEntityID);
            using var reader=cmd.ExecuteReader();
            var result=reader.Read() ? (reader.GetString(0)=="Y",reader.GetString(1)=="Y") : (false,false);
            RiskSessionUnchanged(user);
            return result;
        }
        public Dictionary<string,List<Dictionary<string,object>>> RiskAssessmentRead(int engagementId,int id,int versionId)
            => RiskRead("P_READ",new[]{"header","details","results","history"},cmd => {
                RiskParameter(cmd,"p_eng",OracleDbType.Int32,engagementId);
                RiskParameter(cmd,"p_id",OracleDbType.Int32,id);
                RiskParameter(cmd,"p_version",OracleDbType.Int32,versionId);
            });
        public int RiskAssessmentSave(FadRiskAssessmentRequest request)
        {
            var user=RiskSession();
            using var con=DatabaseConnection();
            using var tx=con.BeginTransaction();
            using var cmd=con.CreateCommand();
            cmd.Transaction=tx;
            RiskContext(cmd,user);
            cmd.CommandText="PKG_FAD_RISK_ASSESS.P_SAVE";
            RiskParameter(cmd,"p_eng",OracleDbType.Int32,request.EngagementId);
            RiskParameter(cmd,"p_id",OracleDbType.Int32,request.AssessmentId);
            cmd.Parameters["p_id"].Direction=ParameterDirection.InputOutput;
            RiskParameter(cmd,"p_version",OracleDbType.Int32,request.VersionId);
            RiskParameter(cmd,"p_revision",OracleDbType.Int32,request.Revision);
            RiskParameter(cmd,"p_size",OracleDbType.Varchar2,request.BranchSize);
            RiskParameter(cmd,"p_portfolio",OracleDbType.Decimal,request.Portfolio);
            RiskParameter(cmd,"p_reason",OracleDbType.Varchar2,request.SizeReason);
            RiskParameter(cmd,"p_details",OracleDbType.Clob,JsonSerializer.Serialize(request.Details));
            cmd.ExecuteNonQuery();
            var id=int.Parse(cmd.Parameters["p_id"].Value.ToString());
            RiskSessionUnchanged(user);
            tx.Commit();
            return id; // Disposal rolls back uncommitted transactions on every error path.
        }
        public void RiskAssessmentTransition(FadRiskTransitionRequest request)
        {
            var user=RiskSession();
            using var con=DatabaseConnection();
            using var tx=con.BeginTransaction();
            using var cmd=con.CreateCommand();
            cmd.Transaction=tx;
            RiskContext(cmd,user);
            cmd.CommandText="PKG_FAD_RISK_ASSESS.P_TRANSITION";
            RiskParameter(cmd,"p_eng",OracleDbType.Int32,request.EngagementId);
            RiskParameter(cmd,"p_id",OracleDbType.Int32,request.AssessmentId);
            RiskParameter(cmd,"p_revision",OracleDbType.Int32,request.Revision);
            RiskParameter(cmd,"p_action",OracleDbType.Varchar2,request.Action);
            RiskParameter(cmd,"p_remarks",OracleDbType.Varchar2,request.Remarks);
            cmd.ExecuteNonQuery();
            RiskSessionUnchanged(user);
            tx.Commit();
        }
    }
}

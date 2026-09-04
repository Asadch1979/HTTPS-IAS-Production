using AIS.Models;
using AIS.Services;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public void LogApplicationActivity(ApplicationAuditEvent e, ApplicationAuditContext c)
            {
            using var con = DatabaseConnection(requireActiveSession: false);
            using var cmd = con.CreateCommand();
            cmd.CommandText = "PKG_LG.P_LOG_APPLICATION_ACTIVITY";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            AddAuditParameter(cmd, "P_EVENT_TYPE", OracleDbType.Varchar2, e.EventType ?? "BUSINESS_ACTION");
            AddAuditParameter(cmd, "P_ACTION_NAME", OracleDbType.Varchar2, e.ActionName);
            AddAuditParameter(cmd, "P_ACTION_CATEGORY", OracleDbType.Varchar2, e.ActionCategory);
            AddAuditParameter(cmd, "P_MODULE_NAME", OracleDbType.Varchar2, e.ModuleName);
            AddAuditParameter(cmd, "P_PPNO", OracleDbType.Varchar2, c.Ppno);
            AddAuditParameter(cmd, "P_ROLE_ID", OracleDbType.Int32, c.RoleId);
            AddAuditParameter(cmd, "P_GROUP_ID", OracleDbType.Int32, c.GroupId);
            AddAuditParameter(cmd, "P_ENTITY_ID", OracleDbType.Int32, c.EntityId);
            AddAuditParameter(cmd, "P_USER_CONTEXT_ID", OracleDbType.Int32, c.UserContextId);
            AddAuditParameter(cmd, "P_SESSION_ID", OracleDbType.Varchar2, c.SessionId);
            AddAuditParameter(cmd, "P_PAGE_ID", OracleDbType.Int32, c.PageId);
            AddAuditParameter(cmd, "P_CONTROLLER_NAME", OracleDbType.Varchar2, c.ControllerName);
            AddAuditParameter(cmd, "P_CONTROLLER_ACTION", OracleDbType.Varchar2, c.ControllerAction);
            AddAuditParameter(cmd, "P_API_PATH", OracleDbType.Varchar2, c.ApiPath);
            AddAuditParameter(cmd, "P_HTTP_METHOD", OracleDbType.Varchar2, c.HttpMethod);
            AddAuditParameter(cmd, "P_DB_PACKAGE_NAME", OracleDbType.Varchar2, e.DbPackageName);
            AddAuditParameter(cmd, "P_DB_PROCEDURE_NAME", OracleDbType.Varchar2, e.DbProcedureName);
            AddAuditParameter(cmd, "P_ENGAGEMENT_ID", OracleDbType.Int64, e.EngagementId);
            AddAuditParameter(cmd, "P_PARA_ID", OracleDbType.Int64, e.ParaId);
            AddAuditParameter(cmd, "P_OLD_PARA_ID", OracleDbType.Int64, e.OldParaId);
            AddAuditParameter(cmd, "P_NEW_PARA_ID", OracleDbType.Int64, e.NewParaId);
            AddAuditParameter(cmd, "P_COM_ID", OracleDbType.Int64, e.ComId);
            AddAuditParameter(cmd, "P_OBJECT_TYPE", OracleDbType.Varchar2, e.ObjectType);
            AddAuditParameter(cmd, "P_OBJECT_ID", OracleDbType.Varchar2, e.ObjectId);
            AddAuditParameter(cmd, "P_RESULT_STATUS", OracleDbType.Varchar2, "SUCCESS");
            AddAuditParameter(cmd, "P_RESULT_CODE", OracleDbType.Varchar2, e.ResultCode);
            AddAuditParameter(cmd, "P_RESULT_MESSAGE", OracleDbType.Varchar2, ApplicationAuditLogger.Truncate(e.ResultMessage, 1000));
            AddAuditParameter(cmd, "P_CLIENT_IP_ADDRESS", OracleDbType.Varchar2, c.ClientIp);
            AddAuditParameter(cmd, "P_PROXY_IP_ADDRESS", OracleDbType.Varchar2, c.ProxyIp);
            AddAuditParameter(cmd, "P_USER_AGENT", OracleDbType.Varchar2, c.UserAgent);
            AddAuditParameter(cmd, "P_TRACE_ID", OracleDbType.Varchar2, c.TraceId);
            AddAuditParameter(cmd, "P_REQUEST_ID", OracleDbType.Varchar2, c.RequestId);
            AddAuditParameter(cmd, "P_DURATION_MS", OracleDbType.Int64, c.DurationMs);
            AddAuditParameter(cmd, "P_DETAILS", OracleDbType.Clob, e.Details);
            GuardAgainstDynamicSql(cmd);
            cmd.ExecuteNonQuery();
            }

        private static void AddAuditParameter(OracleCommand cmd, string name, OracleDbType type, object value)
            => cmd.Parameters.Add(name, type).Value = value ?? DBNull.Value;
        }
    }

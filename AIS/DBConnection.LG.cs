using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public const int ManageUsersSelectionPageId = 43;

        public List<EntityTypeSelectionDto> GetEntityTypesForSelection(int pageId)
            => ReadManageUsersSelection("PKG_LG.P_Get_Entity_type", pageId, null, null,
                r => new EntityTypeSelectionDto { AUTID = Convert.ToInt32(r["AUTID"]), ENTITYCODE = ReadString(r, "ENTITYCODE"), ENTITYTYPEDESC = ReadString(r, "ENTITYTYPEDESC"), DISPLAY_ORDER = ReadNullableInt(r, "DISPLAY_ORDER") });

        public List<ReportingOfficeSelectionDto> GetReportingOfficesForSelection(int entityTypeId, int pageId)
            => ReadManageUsersSelection("PKG_LG.P_Get_Reporting_Office", pageId, entityTypeId, null,
                r => new ReportingOfficeSelectionDto { ENTITY_ID = Convert.ToInt32(r["ENTITY_ID"]), DESCRIPTION = ReadString(r, "DESCRIPTION"), PARENT_TYPE_ID = ReadNullableInt(r, "PARENT_TYPE_ID") });

        public List<PostingSelectionDto> GetPostingsForSelection(int reportingId, int entityTypeId, int pageId)
            => ReadManageUsersSelection("PKG_LG.P_Get_Posting", pageId, entityTypeId, reportingId,
                r => new PostingSelectionDto { ENTITY_ID = Convert.ToInt32(r["ENTITY_ID"]), C_NAME = ReadString(r, "C_NAME"), TYPEID = Convert.ToInt32(r["TYPEID"]), PARENT_ID = ReadNullableInt(r, "PARENT_ID") });

        private List<T> ReadManageUsersSelection<T>(string procedure, int pageId, int? entityTypeId, int? reportingId, Func<OracleDataReader, T> map)
            {
            if (pageId != ManageUsersSelectionPageId || entityTypeId <= 0 || reportingId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageId));
            var user = CreateSessionHandler().GetUserOrThrow();
            if (CreateSessionHandler().IsSessionRevoked(user))
                throw new AIS.Session.UnauthenticatedException();
            var rows = new List<T>();
            using (var con = DatabaseConnection())
            using (var cmd = con.CreateCommand())
                {
                cmd.CommandText = procedure;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;
                GuardAgainstDynamicSql(cmd);
                if (reportingId.HasValue) cmd.Parameters.Add("Reporting_ID", OracleDbType.Int32).Value = reportingId.Value;
                if (entityTypeId.HasValue) cmd.Parameters.Add("ENT_TYP_ID", OracleDbType.Int32).Value = entityTypeId.Value;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = Convert.ToInt32(user.PPNumber);
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = user.UserRoleID;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.UserEntityID;
                cmd.Parameters.Add("PAGE_ID", OracleDbType.Int32).Value = ManageUsersSelectionPageId;
                cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) rows.Add(map(reader));
                }
            return rows;
            }

        public void LogInfo(string module, string controller, string action, string message, string techDetails, int? pageId, int? engId, string userPpno)
            {
            LogWithLevel("PKG_LG.LOG_INFO", module, controller, action, message, techDetails, pageId, engId, userPpno);
            }

        public void LogWarning(string module, string controller, string action, string message, string techDetails, int? pageId, int? engId, string userPpno)
            {
            LogWithLevel("PKG_LG.LOG_WARNING", module, controller, action, message, techDetails, pageId, engId, userPpno);
            }

        public void LogError(string module, string controller, string action, string message, string techDetails, int? pageId, int? engId, string userPpno)
            {
            LogWithLevel("PKG_LG.LOG_ERROR", module, controller, action, message, techDetails, pageId, engId, userPpno);
            }

        public List<SystemLogModel> GetSystemLogs(DateTime? startTime, DateTime? endTime, string logLevel, string module, string userPpno, int? engId)
            {
            var logs = new List<SystemLogModel>();
            using (var con = DatabaseConnection(requireActiveSession: false))
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_LG.P_GET_SYS_LOGS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    GuardAgainstDynamicSql(cmd);
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_start_time", OracleDbType.TimeStamp).Value = startTime.HasValue ? (object)startTime.Value : DBNull.Value;
                    cmd.Parameters.Add("p_end_time", OracleDbType.TimeStamp).Value = endTime.HasValue ? (object)endTime.Value : DBNull.Value;
                    cmd.Parameters.Add("p_log_level", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(logLevel) ? (object)DBNull.Value : logLevel;
                    cmd.Parameters.Add("p_module", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(module) ? (object)DBNull.Value : module;
                    cmd.Parameters.Add("p_user_ppno", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(userPpno) ? (object)DBNull.Value : userPpno;
                    cmd.Parameters.Add("p_eng_id", OracleDbType.Int32).Value = engId.HasValue ? (object)engId.Value : DBNull.Value;
                    cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            logs.Add(new SystemLogModel
                                {
                                LOGID = ReadInt(reader, "LOG_ID"),
                                LOGLEVEL = ReadString(reader, "LOG_LEVEL"),
                                LOGTIME = ReadDateTime(reader, "LOG_TIME"),
                                MODULE = ReadString(reader, "MODULE"),
                                CONTROLLER = ReadString(reader, "CONTROLLER"),
                                ACTION = ReadString(reader, "ACTION"),
                                MESSAGE = ReadString(reader, "MESSAGE"),
                                TECHDETAILS = ReadClob(reader, "TECH_DETAILS"),
                                PAGEID = ReadNullableInt(reader, "PAGE_ID"),
                                ENGID = ReadNullableInt(reader, "ENG_ID"),
                                USERPPNO = ReadString(reader, "USER_PPNO")
                                });
                            }
                        }
                    }

            return logs;
            }

        public int DeleteOldSystemLogs(DateTime cutoffTime)
            {
            using (var con = DatabaseConnection(requireActiveSession: false))
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_LG.P_DELETE_SYS_LOGS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    GuardAgainstDynamicSql(cmd);
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_cutoff_time", OracleDbType.TimeStamp).Value = cutoffTime;
                    var deletedCountParam = cmd.Parameters.Add("o_deleted_count", OracleDbType.Int32);
                    deletedCountParam.Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();

                    if (deletedCountParam.Value == DBNull.Value || deletedCountParam.Value == null)
                        {
                        return 0;
                        }

                    return Convert.ToInt32(deletedCountParam.Value);
                    }
            }

        private void LogWithLevel(string procedureName, string module, string controller, string action, string message, string techDetails, int? pageId, int? engId, string userPpno)
            {
            using (var con = DatabaseConnection(requireActiveSession: false))
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = procedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_module", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(module) ? (object)DBNull.Value : module;
                    cmd.Parameters.Add("p_controller", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(controller) ? (object)DBNull.Value : controller;
                    cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(action) ? (object)DBNull.Value : action;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(message) ? (object)DBNull.Value : message;
                    cmd.Parameters.Add("p_tech_details", OracleDbType.Clob).Value = string.IsNullOrWhiteSpace(techDetails) ? (object)DBNull.Value : techDetails;
                    cmd.Parameters.Add("p_page_id", OracleDbType.Int32).Value = pageId.HasValue ? (object)pageId.Value : DBNull.Value;
                    cmd.Parameters.Add("p_eng_id", OracleDbType.Int32).Value = engId.HasValue ? (object)engId.Value : DBNull.Value;
                    cmd.Parameters.Add("p_user_ppno", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(userPpno) ? (object)DBNull.Value : userPpno;
                    cmd.ExecuteNonQuery();
                    }
            }

        private static int? ReadNullableInt(IDataRecord reader, string column)
            {
            if (reader == null)
                {
                return null;
                }

            var value = reader[column];
            if (value == null || value == DBNull.Value)
                {
                return null;
                }

            if (value is int intValue)
                {
                return intValue;
                }

            if (value is long longValue)
                {
                return longValue > int.MaxValue || longValue < int.MinValue ? (int?)null : (int)longValue;
                }

            if (value is decimal decimalValue)
                {
                return decimalValue > int.MaxValue || decimalValue < int.MinValue ? (int?)null : decimal.ToInt32(decimalValue);
                }

            var text = value.ToString();
            return int.TryParse(text, out var parsed) ? parsed : (int?)null;
            }

        private static DateTime ReadDateTime(OracleDataReader reader, string column)
            {
            if (reader[column] == DBNull.Value)
                {
                return DateTime.MinValue;
                }

            return Convert.ToDateTime(reader[column]);
            }

        private static string ReadString(OracleDataReader reader, string column)
            {
            if (reader[column] == DBNull.Value)
                {
                return string.Empty;
                }

            return reader[column].ToString();
            }

        private static string ReadClob(OracleDataReader reader, string column)
            {
            if (reader[column] == DBNull.Value)
                {
                return string.Empty;
                }

            using (var clob = reader.GetOracleClob(reader.GetOrdinal(column)))
                {
                return clob?.Value ?? string.Empty;
                }
            }
        }
    }

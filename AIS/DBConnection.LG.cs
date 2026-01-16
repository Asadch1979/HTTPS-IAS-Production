using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
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
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_start_time", OracleDbType.TimeStamp).Value = startTime.HasValue ? (object)startTime.Value : DBNull.Value;
                    cmd.Parameters.Add("p_end_time", OracleDbType.TimeStamp).Value = endTime.HasValue ? (object)endTime.Value : DBNull.Value;
                    cmd.Parameters.Add("p_log_level", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(logLevel) ? (object)DBNull.Value : logLevel;
                    cmd.Parameters.Add("p_module", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(module) ? (object)DBNull.Value : module;
                    cmd.Parameters.Add("p_user_ppno", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(userPpno) ? (object)DBNull.Value : userPpno;
                    cmd.Parameters.Add("p_eng_id", OracleDbType.Int32).Value = engId.HasValue ? (object)engId.Value : DBNull.Value;
                    cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            logs.Add(new SystemLogModel
                                {
                                LOG_ID = ReadInt(reader, "LOG_ID"),
                                LOG_LEVEL = ReadString(reader, "LOG_LEVEL"),
                                LOG_TIME = ReadDateTime(reader, "LOG_TIME"),
                                MODULE = ReadString(reader, "MODULE"),
                                CONTROLLER = ReadString(reader, "CONTROLLER"),
                                ACTION = ReadString(reader, "ACTION"),
                                MESSAGE = ReadString(reader, "MESSAGE"),
                                TECH_DETAILS = ReadString(reader, "TECH_DETAILS"),
                                PAGE_ID = ReadNullableInt(reader, "PAGE_ID"),
                                ENG_ID = ReadNullableInt(reader, "ENG_ID"),
                                USER_PPNO = ReadString(reader, "USER_PPNO")
                                });
                            }
                        }
                    }

            return logs;
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

        private static int ReadInt(OracleDataReader reader, string column)
            {
            if (reader[column] == DBNull.Value)
                {
                return 0;
                }

            return Convert.ToInt32(reader[column]);
            }

        private static int? ReadNullableInt(OracleDataReader reader, string column)
            {
            if (reader[column] == DBNull.Value)
                {
                return null;
                }

            return Convert.ToInt32(reader[column]);
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
        }
    }

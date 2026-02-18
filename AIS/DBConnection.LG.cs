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

        public List<(int pageId, long engId)> GetEngPagePermissionsByPpno(long ppno)
            {
            var permissions = new List<(int pageId, long engId)>();
            if (ppno <= 0)
                {
                return permissions;
                }

            using (var con = DatabaseConnection(requireActiveSession: false))
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_LG.P_GET_ENG_PAGE_PERMISSIONS_BY_PPNO";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_ppno", OracleDbType.Int64).Value = ppno;
                    cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            if (!TryReadInt(reader, "PAGE_ID", out var pageId) || pageId <= 0)
                                {
                                continue;
                                }

                            if (!TryReadLong(reader, "ENG_ID", out var engId) || engId <= 0)
                                {
                                continue;
                                }

                            permissions.Add((pageId, engId));
                            }
                        }
                    }

            return permissions;
            }

        public List<(int pageId, long comId)> GetComPagePermissionsByPpno(long ppno)
            {
            var permissions = new List<(int pageId, long comId)>();
            if (ppno <= 0)
                {
                return permissions;
                }

            using (var con = DatabaseConnection(requireActiveSession: false))
                using (OracleCommand cmd = con.CreateCommand())
                    {
                    cmd.CommandText = "PKG_LG.P_GET_COM_PAGE_PERMISSIONS_BY_PPNO";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("p_ppno", OracleDbType.Int64).Value = ppno;
                    cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            if (!TryReadInt(reader, "PAGE_ID", out var pageId) || pageId <= 0)
                                {
                                continue;
                                }

                            if (!TryReadLong(reader, "COM_ID", out var comId) || comId <= 0)
                                {
                                continue;
                                }

                            permissions.Add((pageId, comId));
                            }
                        }
                    }

            return permissions;
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

        private static bool TryReadInt(OracleDataReader reader, string column, out int value)
            {
            value = 0;
            if (reader == null || string.IsNullOrWhiteSpace(column))
                {
                return false;
                }

            var ordinal = GetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
                {
                return false;
                }

            var raw = reader.GetValue(ordinal);
            switch (raw)
                {
                case int intValue:
                    value = intValue;
                    return true;
                case long longValue when longValue <= int.MaxValue && longValue >= int.MinValue:
                    value = (int)longValue;
                    return true;
                case decimal decimalValue when decimalValue <= int.MaxValue && decimalValue >= int.MinValue:
                    value = decimal.ToInt32(decimalValue);
                    return true;
                default:
                    return int.TryParse(raw.ToString(), out value);
                }
            }

        private static bool TryReadLong(OracleDataReader reader, string column, out long value)
            {
            value = 0;
            if (reader == null || string.IsNullOrWhiteSpace(column))
                {
                return false;
                }

            var ordinal = GetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
                {
                return false;
                }

            var raw = reader.GetValue(ordinal);
            switch (raw)
                {
                case long longValue:
                    value = longValue;
                    return true;
                case int intValue:
                    value = intValue;
                    return true;
                case decimal decimalValue when decimalValue <= long.MaxValue && decimalValue >= long.MinValue:
                    value = decimal.ToInt64(decimalValue);
                    return true;
                default:
                    return long.TryParse(raw.ToString(), out value);
                }
            }

        private static int GetOrdinal(OracleDataReader reader, string column)
            {
            for (var i = 0; i < reader.FieldCount; i++)
                {
                if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                    {
                    return i;
                    }
                }

            return -1;
            }
        }
    }

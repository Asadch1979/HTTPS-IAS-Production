using System;
using System.Collections.Generic;
using System.Data;
using AIS.Models;
using Oracle.ManagedDataAccess.Client;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public List<DashboardLayoutPageModel> GetRoleDashboardPages(int roleId)
            {
            var list = new List<DashboardLayoutPageModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_GET_ROLE_DASHBOARD_PAGES";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = roleId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor)
                .Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                {
                list.Add(new DashboardLayoutPageModel
                    {
                    RoleId = roleId,
                    PageId = ReadInt(reader, "PAGE_ID"),
                    PageName = ReadString(reader, "PAGE_NAME"),
                    PageUrl = ReadString(reader, "PAGE_URL"),
                    PagePath = ReadString(reader, "PAGE_PATH"),
                    PageOrder = ReadInt(reader, "PAGE_ORDER"),
                    DashboardOrder = ReadInt(reader, "DASHBOARD_ORDER")
                    });
                }

            return list;
            }

        public List<DashboardLayoutPageModel> GetRoleDashboardConfig(int roleId)
            {
            var list = new List<DashboardLayoutPageModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_GET_ROLE_DASHBOARD_CONFIG";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = roleId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor)
                .Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                {
                list.Add(new DashboardLayoutPageModel
                    {
                    RoleId = roleId,
                    PageId = ReadInt(reader, "PAGE_ID"),
                    PageName = ReadString(reader, "PAGE_NAME"),
                    DashboardOrder = ReadInt(reader, "DASHBOARD_ORDER"),
                    IsActive = ReadString(reader, "IS_ACTIVE")
                    });
                }

            return list;
            }

        public List<DashboardLayoutPageModel> GetDashboardQuickLinks(int roleId)
            {
            var list = new List<DashboardLayoutPageModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_GET_DASHBOARD_QUICK_LINKS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = roleId;
            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor)
                .Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                {
                var displayOrder = ReadInt(reader, "DISPLAY_ORDER");
                if (displayOrder == 0)
                    {
                    displayOrder = ReadInt(reader, "DASHBOARD_ORDER");
                    }
                if (displayOrder == 0)
                    {
                    displayOrder = ReadInt(reader, "PAGE_ORDER");
                    }

                var pageUrl = ReadString(reader, "PAGE_URL", "PAGE_PATH");

                list.Add(new DashboardLayoutPageModel
                    {
                    RoleId = roleId,
                    PageId = ReadInt(reader, "PAGE_ID"),
                    PageName = ReadString(reader, "PAGE_NAME"),
                    PageUrl = pageUrl,
                    PagePath = pageUrl,
                    DashboardOrder = displayOrder
                    });
                }

            return list;
            }

        public void MaintainRoleDashboardPage(DashboardLayoutSaveRequest item)
            {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_MAINT_ROLE_DASHBOARD_PAGE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = item.RoleId;
            cmd.Parameters.Add("P_PAGE_ID", OracleDbType.Int32).Value = item.PageId;
            cmd.Parameters.Add("P_DASHBOARD_ORDER", OracleDbType.Int32).Value = item.DashboardOrder;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2, 1).Value = item.IsActive;
            cmd.Parameters.Add("P_ACTION_IND", OracleDbType.Varchar2, 1).Value = item.ActionInd;

            cmd.Parameters.Add("O_MESSAGE", OracleDbType.Varchar2, 4000)
                .Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            var msg = cmd.Parameters["O_MESSAGE"].Value?.ToString();

            if (!string.IsNullOrWhiteSpace(msg) && msg.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException(msg);
            }


        public List<ApiMasterModel> GetApiMasterList()
            {
            var list = new List<ApiMasterModel>();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_GET_API_MASTER";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("O_CURSOR", OracleDbType.RefCursor)
                .Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                {
                list.Add(new ApiMasterModel
                    {
                    ApiId = ReadInt(reader, "API_ID"),
                    ApiName = ReadString(reader, "VIEW_NAME", "API_NAME", "ACTION_NAME"),
                    ApiPath = ReadString(reader, "API_PATH"),
                    HttpMethod = ReadString(reader, "HTTP_METHOD"),
                    IsActive = ReadString(reader, "IS_ACTIVE")
                    });
                }

            return list;
            }

        public bool ApiPathExists(string apiPath, string httpMethod, int apiId)
            {
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_CHECK_API_UNIQUE";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("P_API_ID", OracleDbType.Int32).Value = apiId;
            cmd.Parameters.Add("P_API_PATH", OracleDbType.Varchar2).Value = apiPath;
            cmd.Parameters.Add("P_HTTP_METHOD", OracleDbType.Varchar2).Value = httpMethod;
            cmd.Parameters.Add("O_EXISTS", OracleDbType.Int32)
                .Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();
            var count = Convert.ToInt32(cmd.Parameters["O_EXISTS"].Value);
            return count > 0;
            }

        public void InsertApiMaster(ApiMasterModel model)
            {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            MaintainApiMaster(model, "A");
            }

        public void UpdateApiMaster(ApiMasterModel model)
            {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            MaintainApiMaster(model, "U");
            }

        public void MaintainApiMaster(ApiMasterModel model, string actionInd)
            {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(actionInd))
                throw new ArgumentException("Action indicator is required.", nameof(actionInd));

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = "PKG_AD.P_MAINT_API_MASTER";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Clear();

            cmd.Parameters.Add("P_API_ID", OracleDbType.Int32).Value = model.ApiId;
            cmd.Parameters.Add("P_API_NAME", OracleDbType.Varchar2).Value = model.ApiName;
            cmd.Parameters.Add("P_API_PATH", OracleDbType.Varchar2).Value = model.ApiPath;
            cmd.Parameters.Add("P_HTTP_METHOD", OracleDbType.Varchar2).Value = model.HttpMethod;
            cmd.Parameters.Add("P_IS_ACTIVE", OracleDbType.Varchar2, 1).Value = model.IsActive;
            cmd.Parameters.Add("P_ACTION_IND", OracleDbType.Varchar2, 1).Value = actionInd;

            cmd.Parameters.Add("O_MESSAGE", OracleDbType.Varchar2, 4000)
                .Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            var msg = cmd.Parameters["O_MESSAGE"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(msg) && msg.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException(msg);
            }

        private static int ReadInt(IDataRecord reader, string column, int defaultValue = 0)
            {
            var ordinal = GetOrdinal(reader, column);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
                {
                return defaultValue;
                }

            var value = reader.GetValue(ordinal);
            if (value == null || value == DBNull.Value)
                {
                return defaultValue;
                }

            if (value is int intValue)
                {
                return intValue;
                }

            if (value is long longValue)
                {
                return longValue > int.MaxValue || longValue < int.MinValue ? defaultValue : (int)longValue;
                }

            if (value is decimal decimalValue)
                {
                return decimalValue > int.MaxValue || decimalValue < int.MinValue ? defaultValue : decimal.ToInt32(decimalValue);
                }

            var text = value.ToString();
            return int.TryParse(text, out var parsed) ? parsed : defaultValue;
            }

        private static string ReadString(IDataRecord reader, params string[] columns)
            {
            if (columns == null || columns.Length == 0)
                {
                return string.Empty;
                }

            foreach (var column in columns)
                {
                var ordinal = GetOrdinal(reader, column);
                if (ordinal < 0 || reader.IsDBNull(ordinal))
                    {
                    continue;
                    }

                return reader.GetValue(ordinal)?.ToString();
                }

            return string.Empty;
            }

        private static int GetOrdinal(IDataRecord reader, string column)
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

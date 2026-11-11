using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        // The page ID is resolved from the authenticated controller's fixed
        // page path / dashboard scope. It must NEVER come from query parameters.
        // Require exact active GET dependency, not a role-only or TOP-anchor grant.
        internal bool HasActiveExecutivePageApi(int pageId, string method, string requestPath)
        {
            if ((pageId < 1000300 || pageId > 1000305) && pageId != 1000401)
                return false;
            if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(requestPath))
                return false;
            var normalized = requestPath.Trim().Trim('/').Replace('\\', '/');
            if (!normalized.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ||
                normalized.Length > 300 || normalized.Contains("//") ||
                normalized.Contains("?") || normalized.Contains("#"))
                return false;

            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                SELECT COUNT(*)
                  FROM T_IAS_EXEC_PAGE_API d
                  JOIN T_MENU_PAGES p
                    ON p.ID=d.PAGE_ID AND p.STATUS='A'
                  JOIN T_AU_API_MASTER m
                    ON m.API_ID=d.API_ID AND m.IS_ACTIVE='Y'
                 WHERE d.PAGE_ID=:page_id AND d.IS_ACTIVE='Y'
                   AND UPPER(TRIM(m.HTTP_METHOD))='GET'
                   AND LOWER(TRIM(BOTH '/' FROM m.API_PATH))=LOWER(:api_path)";
            command.Parameters.Add("page_id", OracleDbType.Int32).Value = pageId;
            command.Parameters.Add("api_path", OracleDbType.Varchar2, 300).Value = normalized;
            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }
    }
}

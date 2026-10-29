using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        internal List<CcoFunctionPositionRow> GetCcoFunctionPosition(ExecutiveDashboardScope scope, CcoOrganizationFilter filters)
        {
            ValidateCcoOrganizationContext(scope);
            filters.Validate();
            // Never fall back to unscoped Report E when the trusted Oracle scope
            // objects are absent or invalid. The controller checks exact API grants.
            return ReadCcoFunctionPosition(scope, filters);
        }

        // Secured Oracle reader: scope token, source rows and aggregates all stay
        // within one transaction. No browser-selectable root or unscoped fallback.
        private List<CcoFunctionPositionRow> ReadCcoFunctionPosition(ExecutiveDashboardScope scope, CcoOrganizationFilter filters)
        {
            ValidateCcoOrganizationContext(scope); filters.Validate();
            var user=ValidateExecutiveDashboardSession(scope);
            var group=user.UserGroupID;
            if(group<=0 || !scope.ContextAssignmentId.HasValue || !scope.SessionEntityId.HasValue) throw new UnauthorizedAccessException();
            using var connection=DatabaseConnection();
            using var transaction=connection.BeginTransaction();
            OracleCommand Command(string procedure)
            {
                var cmd=connection.CreateCommand();cmd.Transaction=transaction;cmd.CommandType=CommandType.StoredProcedure;
                cmd.BindByName=true;cmd.CommandTimeout=60;cmd.CommandText=procedure;return cmd;
            }
            void End(){using var cmd=Command("PKG_CCO_SCOPE.P_END_SCOPE");cmd.ExecuteNonQuery();}
            string token=null;
            void Assert(){using var cmd=Command("PKG_CCO_SCOPE.P_ASSERT_SCOPE");cmd.Parameters.Add("P_SCOPE_TOKEN",OracleDbType.Varchar2,32).Value=token;cmd.ExecuteNonQuery();}
            try
            {
                using(var begin=Command("PKG_CCO_SCOPE.P_BEGIN_SCOPE"))
                {
                    begin.Parameters.Add("P_USER_ID",OracleDbType.Int32).Value=scope.UserId;
                    begin.Parameters.Add("P_ROLE_ID",OracleDbType.Int32).Value=scope.RoleId;
                    begin.Parameters.Add("P_GROUP_ID",OracleDbType.Int32).Value=group;
                    begin.Parameters.Add("P_ENTITY_ID",OracleDbType.Int32).Value=scope.SessionEntityId.Value;
                    begin.Parameters.Add("P_CONTEXT_ID",OracleDbType.Int32).Value=scope.ContextAssignmentId.Value;
                    begin.Parameters.Add("P_SESSION_ID",OracleDbType.Varchar2).Value=scope.SessionId;
                    var output=begin.Parameters.Add("O_SCOPE_TOKEN",OracleDbType.Varchar2,32);output.Direction=ParameterDirection.Output;
                    begin.ExecuteNonQuery();token=output.Value?.ToString();
                }
                if(token==null || !Regex.IsMatch(token,"\\A[0-9A-F]{32}\\z"))throw new UnauthorizedAccessException();
                Assert();var rows=new List<CcoFunctionPositionRow>();long? expected=null;
                using(var cmd=Command("PKG_CCO_FUNCTION_POSITION.P_GET_POSITION"))
                {
                    cmd.Parameters.Add("P_SCOPE_TOKEN",OracleDbType.Varchar2,32).Value=token;
                    cmd.Parameters.Add("P_START_DATE",OracleDbType.Date).Value=DateTime.ParseExact(filters.StartDate,"yyyy-MM-dd",CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("P_END_DATE",OracleDbType.Date).Value=DateTime.ParseExact(filters.EndDate,"yyyy-MM-dd",CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("P_MAX_ROWS",OracleDbType.Int32).Value=filters.MaxRows;
                    cmd.Parameters.Add("IO_CURSOR",OracleDbType.RefCursor).Direction=ParameterDirection.Output;
                    using var reader=cmd.ExecuteReader();
                    while(reader.Read())
                    {
                        long Count(string key)=>Convert.ToInt64(reader[key],CultureInfo.InvariantCulture);
                        var sourceCount=Count("SOURCE_ROW_COUNT");
                        if(sourceCount>filters.MaxRows || sourceCount<1 || rows.Count>=filters.MaxRows || (expected.HasValue && expected!=sourceCount))throw new InvalidOperationException("Invalid function row count");
                        expected=sourceCount;
                        var row=new CcoFunctionPositionRow {
                            RowKind=Convert.ToString(reader["ROW_KIND"]),GroupId=OrgNumber(reader,"GROUP_ID"),GroupName=OrgText(reader,"GROUP_NAME"),
                            DivisionId=OrgNumber(reader,"DIVISION_ID"),DivisionName=OrgText(reader,"DIVISION_NAME"),
                            OpeningBalance=Count("OPENING_BALANCE"),ParasAdded=Count("PARAS_ADDED"),TotalParas=Count("TOTAL_PARAS"),
                            SettledParas=Count("SETTLED_PARAS"),OutstandingParas=Count("OUTSTANDING_PARAS"),
                            SettlementPercentage=reader["SETTLEMENT_PERCENTAGE"]==DBNull.Value?null:Convert.ToDecimal(reader["SETTLEMENT_PERCENTAGE"],CultureInfo.InvariantCulture)};
                        if(row.TotalParas!=checked(row.OpeningBalance+row.ParasAdded) || row.OpeningBalance<0 || row.ParasAdded<0 || row.SettledParas<0 || row.OutstandingParas<0 ||
                            (row.RowKind!="DETAIL" && row.RowKind!="SUBTOTAL" && row.RowKind!="TOTAL"))throw new InvalidOperationException("Invalid function aggregate");
                        rows.Add(row);
                    }
                }
                if(expected.HasValue && expected!=rows.Count)throw new InvalidOperationException("Incomplete function aggregates");
                Assert();ValidateCcoOrganizationContext(scope);
                if(ValidateExecutiveDashboardSession(scope).UserGroupID!=group)throw new UnauthorizedAccessException();
                End();transaction.Commit();return rows;
            }
            catch
            {
                try{End();}catch{OracleConnection.ClearPool(connection);}
                try{transaction.Rollback();}catch{OracleConnection.ClearPool(connection);}
                throw;
            }
        }
    }
}

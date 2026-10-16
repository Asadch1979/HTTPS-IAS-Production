using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public List<IASParaPositionModel> GetIASParaPosition(int entityTypeId, DateTime startDate, DateTime endDate)
            {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user)) throw new AIS.Session.UnauthenticatedException();
            if (user.UserEntityID.GetValueOrDefault() <= 0 || string.IsNullOrWhiteSpace(user.PPNumber) || user.UserRoleID <= 0)
                throw new InvalidOperationException("A valid user posting is required for IAS Para Position.");

            var rows = new List<IASParaPositionModel>();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "R_IAS_PARA_POSITION";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = user.PPNumber;
            cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = user.UserEntityID;
            cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = user.UserRoleID;
            cmd.Parameters.Add("R_T", OracleDbType.Int32).Value = entityTypeId;
            cmd.Parameters.Add("S_DATE", OracleDbType.Date).Value = startDate;
            cmd.Parameters.Add("E_DATE", OracleDbType.Date).Value = endDate;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                rows.Add(new IASParaPositionModel
                    {
                    REPORTING_OFFICE = reader["P_NAME"].ToString(),
                    PLACE_OF_POSTING = reader["C_NAME"].ToString(),
                    CHILD_CODE = reader["CHILD_CODE"].ToString(),
                    OPENING_BALANCE = reader["OPENING_BAL"].ToString(),
                    ISSUE_IDENTIFIED = Convert.ToInt32(reader["ISSUE_IDENTIFIED"]),
                    ISSUE_RECTIFIED = Convert.ToInt32(reader["ISSUE_RECTIFIED"]),
                    PARA_ADDED = reader["PARA_ADDED"].ToString(),
                    SETTLED_COM = reader["SETTLED_COM"].ToString(),
                    SETTLED_AUDIT = reader["SETTLED_AUD"].ToString(),
                    OUTSTANDING = reader["OUTSTANDING"].ToString(),
                    R1 = reader["R1"].ToString(),
                    R2 = reader["R2"].ToString(),
                    R3 = reader["R3"].ToString()
                    });
                }
            return rows;
            }
        }
    }

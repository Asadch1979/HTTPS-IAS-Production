using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection : Controller, IDBConnection
        {
        public List<AuditPeriodModel> GetAuditPeriodStatus()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditPeriodModel> periodList = new List<AuditPeriodModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GET_Auditperiod_status";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditPeriodModel period = new AuditPeriodModel();
                    period.STATUS_ID = Convert.ToInt32(rdr["ID"]);
                    period.STATUS = rdr["STATUS"].ToString();
                    periodList.Add(period);
                    }
                }
            con.Dispose();
            return periodList;
            }

        public List<AuditPeriodModel> GetAuditPeriods(int dept_code = 0, int AUDIT_PERIOD_ID = 0)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditPeriodModel> periodList = new List<AuditPeriodModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditPeriods";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    {
                    AuditPeriodModel period = new AuditPeriodModel();
                    period.AUDITPERIODID = Convert.ToInt32(rdr["AUDITPERIODID"]);
                    period.DESCRIPTION = rdr["DESCRIPTION"].ToString();
                    period.START_DATE = Convert.ToDateTime(rdr["START_DATE"]);
                    period.END_DATE = Convert.ToDateTime(rdr["END_DATE"]);
                    period.STATUS_ID = Convert.ToInt32(rdr["STATUS_ID"]);
                    period.STATUS = rdr["STATUS"].ToString();
                    periodList.Add(period);
                    }
                }
            con.Dispose();
            return periodList;
            }

        public List<AuditCriteriaModel> GetAuditCriteriasToAuthorize()
            {
            var sessionHandler = CreateSessionHandler();


            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditCriteriaModel> criteriaList = new List<AuditCriteriaModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetAuditCriteriasToAuthorize";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    {
                    AuditCriteriaModel acr = new AuditCriteriaModel();
                    acr.ID = Convert.ToInt32(rdr["ID"]);
                    acr.ENTITY_TYPEID = Convert.ToInt32(rdr["ENTITY_TYPEID"]);
                    if (rdr["ENTITY_ID"].ToString() != null && rdr["ENTITY_ID"].ToString() != "")
                        acr.ENTITY_ID = Convert.ToInt32(rdr["ENTITY_ID"]);
                    if (rdr["SIZE_ID"].ToString() != null && rdr["SIZE_ID"].ToString() != "")
                        acr.SIZE_ID = Convert.ToInt32(rdr["SIZE_ID"]);
                    acr.RISK_ID = Convert.ToInt32(rdr["RISK_ID"]);
                    acr.FREQUENCY_ID = Convert.ToInt32(rdr["FREQUENCY_ID"]);
                    acr.NO_OF_DAYS = Convert.ToInt32(rdr["NO_OF_DAYS"]);
                    acr.APPROVAL_STATUS = Convert.ToInt32(rdr["APPROVAL_STATUS"]);
                    acr.AUDITPERIODID = Convert.ToInt32(rdr["AUDITPERIODID"]);
                    acr.PERIOD = rdr["PERIOD"].ToString();
                    //acr.ENTITY = rdr["ENTITY"].ToString();
                    acr.FREQUENCY = rdr["FREQUENCY"].ToString();
                    acr.SIZE = rdr["BRSIZE"].ToString();
                    acr.RISK = rdr["RISK"].ToString();
                    acr.VISIT = rdr["VISIT"].ToString();
                    acr.ENTITY_NAME = rdr["NAME"].ToString();
                    acr.COMMENTS = rdr["REMARKS"].ToString();// this.GetAuditCriteriaLogLastStatus(acr.ID);
                    acr.ENTITIES_COUNT = this.GetExpectedCountOfAuditEntitiesOnCriteria(acr.ID);
                    criteriaList.Add(acr);
                    }
                }
            con.Dispose();
            return criteriaList;
            }


        public List<AuditRefEngagementPlanModel> GetRefferedBackAuditEngagementPlans()
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUserOrThrow();
            var con = this.DatabaseConnection();
            List<AuditRefEngagementPlanModel> list = new List<AuditRefEngagementPlanModel>();
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "pkg_pg.P_GetRefferedBackAuditEngagementPlans";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("ENT_ID", OracleDbType.Int32).Value = loggedInUser.UserEntityID;
                cmd.Parameters.Add("P_NO", OracleDbType.Int32).Value = loggedInUser.PPNumber;
                cmd.Parameters.Add("R_ID", OracleDbType.Int32).Value = loggedInUser.UserRoleID;
                cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                OracleDataReader ardr = cmd.ExecuteReader();
                while (ardr.Read())
                    {
                    AuditRefEngagementPlanModel eng = new AuditRefEngagementPlanModel();
                    eng.PLAN_ID = Convert.ToInt32(ardr["plan_id"].ToString());
                    eng.ENG_ID = Convert.ToInt32(ardr["eng_id"].ToString());
                    eng.TEAM_NAME = ardr["team_name"].ToString();
                    eng.TEAM_ID = Convert.ToInt32(ardr["team_id"].ToString());
                    eng.ENTITY_NAME = ardr["name"].ToString();
                    eng.COMMENTS = this.GetLatestCommentsOnEngagement(Convert.ToInt32(eng.ENG_ID)).ToString();
                    eng.AUDIT_STARTDATE = Convert.ToDateTime(ardr["audit_startdate"].ToString()).ToString("dd/MM/yyyy");
                    eng.AUDIT_ENDDATE = Convert.ToDateTime(ardr["audit_enddate"].ToString()).ToString("dd/MM/yyyy");
                    eng.OP_STARTDATE = Convert.ToDateTime(ardr["op_startdate"].ToString()).ToString("dd/MM/yyyy");
                    eng.OP_ENDDATE = Convert.ToDateTime(ardr["op_enddate"].ToString()).ToString("dd/MM/yyyy");
                    eng.ENTITY_ID = Convert.ToInt32(ardr["entity_id"].ToString());
                    eng.AUDIT_BY_ID = Convert.ToInt32(ardr["auditby_id"].ToString());
                    list.Add(eng);
                    }
                }
            con.Dispose();
            return list;
            }


        }
    }

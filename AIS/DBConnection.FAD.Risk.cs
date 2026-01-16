using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        /* =========================
           SAVE – MAIN KPI
           ========================= */

        public void P_SAVE_KPI_MAIN(
            ref int kpiMainId,
            string code,
            string name,
            int displayOrder,
            string isActive)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_SAVE_KPI_MAIN";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kpi_main_id", OracleDbType.Int32).Value = kpiMainId;
                cmd.Parameters["p_kpi_main_id"].Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add("p_code", OracleDbType.Varchar2).Value = code;
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                cmd.Parameters.Add("p_display_order", OracleDbType.Int32).Value = displayOrder;
                cmd.Parameters.Add("p_is_active", OracleDbType.Char).Value = isActive;

                cmd.ExecuteNonQuery();

                kpiMainId = int.Parse(cmd.Parameters["p_kpi_main_id"].Value.ToString());
                }
            }

        /* =========================
           SAVE – SUB KPI
           ========================= */

        public void P_SAVE_KPI_SUB(
            ref int kpiSubId,
            int kpiMainId,
            string code,
            string name,
            decimal weightage,
            int displayOrder,
            string isActive)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_SAVE_KPI_SUB";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kpi_sub_id", OracleDbType.Int32).Value = kpiSubId;
                cmd.Parameters["p_kpi_sub_id"].Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add("p_kpi_main_id", OracleDbType.Int32).Value = kpiMainId;
                cmd.Parameters.Add("p_code", OracleDbType.Varchar2).Value = code;
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                cmd.Parameters.Add("p_weightage", OracleDbType.Decimal).Value = weightage;
                cmd.Parameters.Add("p_display_order", OracleDbType.Int32).Value = displayOrder;
                cmd.Parameters.Add("p_is_active", OracleDbType.Char).Value = isActive;

                cmd.ExecuteNonQuery();

                kpiSubId = int.Parse(cmd.Parameters["p_kpi_sub_id"].Value.ToString());
                }
            }

        /* =========================
           SAVE – PROCESS
           ========================= */

        public void P_SAVE_PROCESS(
            ref int processId,
            int kpiSubId,
            string code,
            string name,
            decimal weightage,
            int displayOrder,
            string isActive)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_SAVE_PROCESS";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_process_id", OracleDbType.Int32).Value = processId;
                cmd.Parameters["p_process_id"].Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add("p_kpi_sub_id", OracleDbType.Int32).Value = kpiSubId;
                cmd.Parameters.Add("p_code", OracleDbType.Varchar2).Value = code;
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                cmd.Parameters.Add("p_weightage", OracleDbType.Decimal).Value = weightage;
                cmd.Parameters.Add("p_display_order", OracleDbType.Int32).Value = displayOrder;
                cmd.Parameters.Add("p_is_active", OracleDbType.Char).Value = isActive;

                cmd.ExecuteNonQuery();

                processId = int.Parse(cmd.Parameters["p_process_id"].Value.ToString());
                }
            }

        /* =========================
           SAVE – SUB PROCESS
           ========================= */

        public void P_SAVE_SUB_PROCESS(
            ref int subProcessId,
            int processId,
            string code,
            string name,
            int gravityId,
            string isActive)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_SAVE_SUB_PROCESS";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_sub_process_id", OracleDbType.Int32).Value = subProcessId;
                cmd.Parameters["p_sub_process_id"].Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add("p_process_id", OracleDbType.Int32).Value = processId;
                cmd.Parameters.Add("p_code", OracleDbType.Varchar2).Value = code;
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                cmd.Parameters.Add("p_gravity_id", OracleDbType.Int32).Value = gravityId;
                cmd.Parameters.Add("p_is_active", OracleDbType.Char).Value = isActive;

                cmd.ExecuteNonQuery();

                subProcessId = int.Parse(cmd.Parameters["p_sub_process_id"].Value.ToString());
                }
            }

        /* =========================
           SAVE – ANNEXURE
           ========================= */

        public void P_SAVE_ANNEXURE(
            ref int annexureId,
            string code,
            string name,
            string isActive)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_SAVE_ANNEXURE";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_annexure_id", OracleDbType.Int32).Value = annexureId;
                cmd.Parameters["p_annexure_id"].Direction = ParameterDirection.InputOutput;
                cmd.Parameters.Add("p_code", OracleDbType.Varchar2).Value = code;
                cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                cmd.Parameters.Add("p_is_active", OracleDbType.Char).Value = isActive;

                cmd.ExecuteNonQuery();

                annexureId = int.Parse(cmd.Parameters["p_annexure_id"].Value.ToString());
                }
            }

        /* =========================
           SAVE – SUB PROCESS → ANNEXURE MAP
           ========================= */

        public void P_SAVE_SUBPROC_ANNEX(
            int subProcessId,
            int annexureId,
            string isActive)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_SAVE_SUBPROC_ANNEX";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_sub_process_id", OracleDbType.Int32).Value = subProcessId;
                cmd.Parameters.Add("p_annexure_id", OracleDbType.Int32).Value = annexureId;
                cmd.Parameters.Add("p_is_active", OracleDbType.Char).Value = isActive;

                cmd.ExecuteNonQuery();
                }
            }

        /* =========================
           GET – MAIN KPI
           ========================= */

        public DataTable P_GET_KPI_MAIN()
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_KPI_MAIN";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }

        /* =========================
           GET – SUB KPI (BY MAIN)
           ========================= */

        public DataTable P_GET_KPI_SUB(int kpiMainId)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_KPI_SUB";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kpi_main_id", OracleDbType.Int32).Value = kpiMainId;
                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }

        /* =========================
           GET – PROCESS (BY SUB KPI)
           ========================= */

        public DataTable P_GET_PROCESS(int kpiSubId)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_PROCESS";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kpi_sub_id", OracleDbType.Int32).Value = kpiSubId;
                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }

        /* =========================
           GET – SUB PROCESS (BY PROCESS)
           ========================= */

        public DataTable P_GET_SUB_PROCESS(int processId)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_SUB_PROCESS";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_process_id", OracleDbType.Int32).Value = processId;
                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }

        /* =========================
           GET – ANNEXURE MASTER
           ========================= */

        public DataTable P_GET_ANNEXURE()
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_ANNEXURE";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }

        /* =========================
           GET – SUB PROCESS → ANNEXURE MAP
           ========================= */

        public DataTable P_GET_SUBPROC_ANNEX(int subProcessId)
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_SUBPROC_ANNEX";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_sub_process_id", OracleDbType.Int32).Value = subProcessId;
                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }

        /* =========================
           GET – GRAVITY MASTER
           ========================= */

        public DataTable P_GET_GRAVITY()
            {
            using (var con = this.DatabaseConnection())
            using (OracleCommand cmd = con.CreateCommand())
                {
                cmd.CommandText = "PKG_FAD_RISK.P_GET_GRAVITY";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                    }
                }
            }
        }
    }

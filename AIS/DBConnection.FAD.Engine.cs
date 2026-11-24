using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;
using System.Collections.Generic;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        // IAS pattern: call a stored package procedure, consume its ref cursors.
        // There is no raw SELECT or application-side risk formula here.
        public Dictionary<string, List<Dictionary<string, object>>> EngineRead(
            bool catalog, int period, int type, int entity, int result)
        {
            var user = RiskSession();
            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            RiskContext(cmd, user);
            cmd.CommandText = "PKG_FAD_RISK_ENGINE." + (catalog ? "P_CATALOG" : "P_RESULTS");
            GuardAgainstDynamicSql(cmd);

            if (!catalog)
            {
                RiskParameter(cmd, "p_period", OracleDbType.Int32, period);
                RiskParameter(cmd, "p_type", OracleDbType.Int32, DBNull.Value);
                RiskParameter(cmd, "p_ent", OracleDbType.Int32, entity > 0 ? (object)entity : DBNull.Value);
                RiskParameter(cmd, "p_result", OracleDbType.Int32, result);
            }

            var cursors = catalog
                ? new[] { "cursor" }
                : new[] { "current", "cursor", "detail", "exceptions" };
            foreach (var name in cursors)
                cmd.Parameters.Add("o_" + name, OracleDbType.RefCursor).Direction =
                    ParameterDirection.Output;

            cmd.ExecuteNonQuery();
            var data = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var name in cursors)
            {
                using var cursor = (OracleRefCursor)cmd.Parameters["o_" + name].Value;
                using var reader = cursor.GetDataReader();
                var rows = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object value = null;
                        if (!reader.IsDBNull(i))
                        {
                            if (string.Equals(reader.GetDataTypeName(i), "CLOB",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                using var clob = reader.GetOracleClob(i);
                                value = clob.Value;
                            }
                            else
                            {
                                value = reader.GetValue(i);
                            }
                        }
                        row[reader.GetName(i).ToLowerInvariant()] = value;
                    }
                    rows.Add(row);
                }
                data[name] = rows;
            }
            RiskSessionUnchanged(user);
            return data;
        }

        // Engine procedures return summary through OUT parameters; IAS writes
        // are never followed by an unscoped raw SELECT from DBConnection.
        public object EngineRun(bool singleEntity, int period, int entity = 0)
        {
            var user = RiskSession();
            using var con = DatabaseConnection();
            using var tx = con.BeginTransaction();
            using var cmd = con.CreateCommand();
            cmd.Transaction = tx;
            RiskContext(cmd, user);
            cmd.CommandText = "PKG_FAD_RISK_ENGINE." +
                (singleEntity ? "P_RUN_ENTITY" : "P_RUN_PERIOD");
            GuardAgainstDynamicSql(cmd);
            RiskParameter(cmd, "p_audit_period_id", OracleDbType.Int32, period);
            if (singleEntity)
                RiskParameter(cmd, "p_ent_id", OracleDbType.Int32, entity);

            foreach (var name in new[]
            {
                "p_run_id", "p_processed", "p_calculated",
                "p_pending", "p_skipped", "p_failed"
            })
                cmd.Parameters.Add(name, OracleDbType.Int64).Direction =
                    ParameterDirection.Output;

            cmd.ExecuteNonQuery();
            int number(string key) => int.Parse(cmd.Parameters[key].Value.ToString());
            var summary = new
            {
                runId = number("p_run_id"),
                processed = number("p_processed"),
                calculated = number("p_calculated"),
                pending = number("p_pending"),
                skipped = number("p_skipped"),
                failed = number("p_failed")
            };

            RiskSessionUnchanged(user);
            tx.Commit();
            return summary;
        }
    }
}

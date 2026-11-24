using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;
using System.Collections.Generic;
namespace AIS.Controllers {
 public partial class DBConnection {
  public Dictionary<string,List<Dictionary<string,object>>> EngineRead(bool catalog,int period,int type,int entity,int result) {
   var user=RiskSession();
   using var con=DatabaseConnection();
   using var cmd=con.CreateCommand();
   RiskContext(cmd,user);
   cmd.CommandText="PKG_FAD_RISK_ENGINE."+(catalog?"P_CATALOG":"P_RESULTS");
   if(!catalog) {
    RiskParameter(cmd,"p_period",OracleDbType.Int32,period);
    RiskParameter(cmd,"p_type",OracleDbType.Int32,type>0?(object)type:null);
    RiskParameter(cmd,"p_ent",OracleDbType.Int32,entity>0?(object)entity:null);
    RiskParameter(cmd,"p_result",OracleDbType.Int32,result);
   }
   var names=catalog?new[]{"cursor"}:new[]{"current","cursor","detail","exceptions"};
   foreach(var name in names) cmd.Parameters.Add("o_"+name,OracleDbType.RefCursor).Direction=ParameterDirection.Output;
   cmd.ExecuteNonQuery();
   var output=new Dictionary<string,List<Dictionary<string,object>>>();
   foreach(var name in names) {
    using var cursor=(OracleRefCursor)cmd.Parameters["o_"+name].Value;
    using var reader=cursor.GetDataReader();
    var rows=new List<Dictionary<string,object>>();
    while(reader.Read()) {
     var row=new Dictionary<string,object>();
     for(int i=0;i<reader.FieldCount;i++) {
      object value=null;
      if(!reader.IsDBNull(i)) {
       if(reader.GetDataTypeName(i).Equals("CLOB",StringComparison.OrdinalIgnoreCase)) { using var lob=reader.GetOracleClob(i); value=lob.Value; }
       else value=reader.GetValue(i);
      }
      row[reader.GetName(i).ToLowerInvariant()]=value;
     }
     rows.Add(row);
    }
    output[name]=rows;
   }
   RiskSessionUnchanged(user); return output;
  }
  public object EngineRun(bool bulk,int period,int selection) {
   var user=RiskSession();
   using var con=DatabaseConnection();
   using var tx=con.BeginTransaction(IsolationLevel.Serializable);
   using var cmd=con.CreateCommand();
   cmd.Transaction=tx; RiskContext(cmd,user);
   cmd.CommandText="PKG_FAD_RISK_ENGINE."+(bulk?"P_RUN_PERIOD":"P_RUN_ENTITY");
   RiskParameter(cmd,"p_audit_period_id",OracleDbType.Int32,period);
   RiskParameter(cmd,bulk?"p_entity_type_id":"p_ent_id",OracleDbType.Int32,selection);
   cmd.Parameters.Add("p_run_id",OracleDbType.Int64).Direction=ParameterDirection.Output;
   cmd.ExecuteNonQuery();
   var id=long.Parse(cmd.Parameters["p_run_id"].Value.ToString());
   cmd.Parameters.Clear();
   cmd.CommandType=CommandType.Text;
   cmd.CommandText="SELECT processed,skipped,pending,failed,calculated FROM T_FAD_RISK_ENGINE_RUN WHERE run_id=:id AND audit_office_id=:office";
   RiskParameter(cmd,"id",OracleDbType.Int64,id); RiskParameter(cmd,"office",OracleDbType.Int32,user.UserEntityID);
   object summary;
   using(var reader=cmd.ExecuteReader()) {
    if(!reader.Read()) throw new InvalidOperationException("Engine summary unavailable.");
    summary=new {runId=id,processed=reader.GetDecimal(0),skipped=reader.GetDecimal(1),pending=reader.GetDecimal(2),failed=reader.GetDecimal(3),calculated=reader.GetDecimal(4)};
   }
   RiskSessionUnchanged(user); tx.Commit(); return summary;
  }
 }
}

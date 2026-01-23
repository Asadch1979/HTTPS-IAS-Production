using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public ObservationPdfDataModel GetObservationPdfData(int obsId)
            {
            var result = new ObservationPdfDataModel();
            using var con = DatabaseConnection();            

            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "pkg_ar.P_GET_OBSERVATION_TO_PRINT";

            cmd.Parameters.Add("OBS_ID", OracleDbType.Int32).Value = obsId;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                if (string.IsNullOrWhiteSpace(result.MemoNumber))
                    {
                    result.MemoNumber = GetString(reader, "MEMO_NUMBER");
                    result.MemoDate = GetNullableDate(reader, "MEMO_DATE");
                    result.Annexure = GetString(reader, "ANNEXURE");
                    result.Title = GetString(reader, "TITLE");
                    result.Risk = GetString(reader, "RISK");
                    result.ParaText = GetString(reader, "PARA_TEXT");
                    }

                var responsibility = new ObservationPdfResponsibilityModel
                    {
                    PpNo = GetString(reader, "PP_NO"),
                    LoanCase = GetString(reader, "LOAN_CASE"),
                    LcAmount = GetString(reader, "LC_AMOUNT")
                    };

                if (!string.IsNullOrWhiteSpace(responsibility.PpNo)
                    || !string.IsNullOrWhiteSpace(responsibility.LoanCase)
                    || !string.IsNullOrWhiteSpace(responsibility.LcAmount))
                    {
                    result.Responsibilities.Add(responsibility);
                    }
                }

            return result;
            }
        }
    }

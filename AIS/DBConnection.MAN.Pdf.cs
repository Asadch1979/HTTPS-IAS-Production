using AIS.Models.FieldAuditReport;
using AIS.Models.ManagementReport;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public ManagementAuditPdfReportData GetManagementAuditReportPdfData(int engId)
            {
            return new ManagementAuditPdfReportData
                {
                Cover = GetManagementAuditCover(engId),
                Sections = GetFieldAuditPdfSections(engId, null),
                StaffRows = GetFieldAuditPdfStaffSnapshot(engId, null),
                Paras = GetFieldAuditPdfParas(engId, null),
                AuditTeam = GetFieldAuditTeamDetails(engId)
                };
            }

        public ManagementAuditCoverModel GetManagementAuditCover(int engId)
            {
            var cover = new ManagementAuditCoverModel();

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "P_GET_AUDIT_COVER";

            cmd.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = engId;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                {
                cover.AuditedBy = GetString(reader, "AUDITED_BY");
                cover.Reporting = GetString(reader, "REPORTING");
                cover.EntityName = GetString(reader, "ENTITY_NAME");
                cover.AuditedOn = GetString(reader, "AUDITED_ON");
                }

            return cover;
            }
        }
    }

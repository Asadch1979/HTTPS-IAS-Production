using AIS.Models;
using AIS.Models.FieldAuditReport;
using System.Collections.Generic;

namespace AIS.Models.ManagementReport
    {
    public class ManagementAuditPdfReportData
        {
        public string GeneratedByName { get; set; }
        public string GeneratedByPPNo { get; set; }
        public ManagementAuditCoverModel Cover { get; set; } = new ManagementAuditCoverModel();
        public List<FieldAuditPdfSectionModel> Sections { get; set; } = new List<FieldAuditPdfSectionModel>();
        public List<FieldAuditPdfStaffRowModel> StaffRows { get; set; } = new List<FieldAuditPdfStaffRowModel>();
        public List<FieldAuditPdfParaModel> Observations { get; set; } = new List<FieldAuditPdfParaModel>();
        public List<FieldAuditPdfParaModel> SettledParas { get; set; } = new List<FieldAuditPdfParaModel>();
        public List<GetTeamDetailsModel> AuditTeam { get; set; } = new List<GetTeamDetailsModel>();
        }

    public class ManagementAuditCoverModel
        {
        public string AuditedBy { get; set; }
        public string Reporting { get; set; }
        public string EntityName { get; set; }
        public string AuditedOn { get; set; }
        }
    }

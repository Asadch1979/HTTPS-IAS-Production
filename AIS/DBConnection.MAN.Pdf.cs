using AIS.Models.ManagementReport;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public ManagementAuditPdfReportData GetManagementAuditReportPdfData(int engId)
            {
            return new ManagementAuditPdfReportData
                {
                Cover = GetManagementAuditCover(engId),
                Sections = GetManagementReportPdfSections(engId),
                StaffRows = GetManReportStaffSnapshot(engId),
                Observations = GetManReportObservationPdfParas(engId),
                SettledParas = GetManReportSettledPdfParas(engId),
                AuditTeam = GetManagementAuditTeamDetails(engId)
                };
            }
        }
    }

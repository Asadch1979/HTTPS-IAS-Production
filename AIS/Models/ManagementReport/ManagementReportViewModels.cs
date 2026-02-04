using AIS.Models;
using System;
using System.Collections.Generic;

namespace AIS.Models.ManagementReport
    {
    public static class ManagementReportSectionCodes
        {
        public const string Objective = "MAN_OBJ_OBJECTIVE";
        public const string Scope = "MAN_OBJ_SCOPE";
        public const string Methodology = "MAN_OBJ_METHODOLOGY";
        public const string Disclaimer = "MAN_OBJ_DISCLAIMER";
        public const string Introduction = "MAN_OBJ_INTRODUCTION";
        public const string ExecutiveSummary = "MAN_EXEC_SUMMARY";
        }

    public class ManagementReportHomeViewModel
        {
        public bool HasActiveEngagement { get; set; }
        public string EntityName { get; set; }
        public string AuditPeriod { get; set; }
        }

    public class ManagementReportCoverViewModel
        {
        public bool HasActiveEngagement { get; set; }
        public int EngagementId { get; set; }
        public ManagementAuditCoverModel Cover { get; set; } = new ManagementAuditCoverModel();
        public List<GetTeamDetailsModel> AuditTeam { get; set; } = new List<GetTeamDetailsModel>();
        }

    public class ManagementReportObjectiveScopeViewModel
        {
        public int EngagementId { get; set; }
        public bool IsReadOnly { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

    public class ManagementReportExecutiveSummaryViewModel
        {
        public int EngagementId { get; set; }
        public bool IsReadOnly { get; set; }
        public string ExecutiveSummary { get; set; }
        }

    public class ManagementReportObservationModel
        {
        public string ParaNo { get; set; }
        public string Title { get; set; }
        public string ParaText { get; set; }
        public string RiskCategory { get; set; }
        public string Recommendation { get; set; }
        public string ManagementReply { get; set; }
        public string AuditReply { get; set; }
        public string Status { get; set; }
        }

    public class ManagementReportObservationsViewModel
        {
        public int EngagementId { get; set; }
        public List<ManagementReportObservationModel> Observations { get; set; } = new List<ManagementReportObservationModel>();
        }

    public class ManagementReportSettledParaModel
        {
        public string Title { get; set; }
        public string ParaText { get; set; }
        public string ManagementReply { get; set; }
        }

    public class ManagementReportSettledParasViewModel
        {
        public int EngagementId { get; set; }
        public List<ManagementReportSettledParaModel> Paras { get; set; } = new List<ManagementReportSettledParaModel>();
        }

    public class ManagementReportChecklistModel
        {
        public bool HasCoverData { get; set; }
        public bool HasObjectiveScope { get; set; }
        public bool HasExecutiveSummary { get; set; }
        public bool HasStaffSnapshot { get; set; }
        public bool HasObservations { get; set; }
        public bool HasSettledParas { get; set; }
        public bool IsComplete => HasCoverData && HasObjectiveScope && HasExecutiveSummary && HasStaffSnapshot && HasObservations && HasSettledParas;
        }

    public class ManagementReportFinalizeViewModel
        {
        public int EngagementId { get; set; }
        public bool IsFinal { get; set; }
        public bool CanFinalize { get; set; }
        public ManagementReportChecklistModel Checklist { get; set; } = new ManagementReportChecklistModel();
        }
    }

using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Models
{
    public sealed class ExecutiveDashboardViewDefinition
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Partial { get; set; }
        public string Description { get; set; }
        public string[] Kpis { get; set; }
        public string[] Sections { get; set; }
        public bool UseLifecycleKpis { get; set; }
        public string[] Datasets => (Kpis.Length > 0 && (!UseLifecycleKpis || Sections.Contains("critical"))
                ? new[] { "GetKpiSummary" } : Array.Empty<string>())
            .Concat(UseLifecycleKpis ? new[] { "GetKpiLifecycle" } : Array.Empty<string>())
            .Concat(Sections.Select(section => section switch { "risk" => "GetRiskSummary", "aging" => "GetAgingSummary",
                "cycle" => "GetCycleSummary", "trend" => "GetSettlementTrend", "critical" => "GetCriticalItems",
                _ => throw new InvalidOperationException("Unknown dashboard section.") })).Distinct().ToArray();
        // Separate endpoints, fetched only by the scoped role-dashboard UI.
        // Keep them out of Datasets so the existing executive charts are untouched.
        public string[] RoleDatasets => Id == "command" ? new[] { "GetCiaCommandKpis", "GetCiaIssueNature" }
            : Id == "management" ? new[] { "GetRoleUnitComparison", "GetRoleFollowups" }
            : Id == "regional" ? new[] { "GetRoleEntityHeatmap", "GetRoleFollowups" } : Array.Empty<string>();
        public string[] Reports { get; set; } = Array.Empty<string>();
    }

    // Presentation definitions are chosen only after resolving the authorized perspective.
    public static class ExecutiveDashboardViews
    {
        public static List<ExecutiveDashboardViewDefinition> ForPerspective(string perspective)
        {
            var glanceKpis = perspective switch
            {
                "TOP" => new[] { "openingOpenParas", "issueIdentified", "issueRectified", "paraAdded", "parasSettled", "closingOpenParas", "highRiskOpenParas", "over1YearParas", "over3ComplianceCycles" },
                "GROUP" => new[] { "openParas", "highRiskParas", "over1YearParas", "settledMtd", "settlementRatio", "criticalItems" },
                "CCO" => new[] { "openParas", "zeroCycleParas", "over3CycleParas", "over1YearParas", "settlementRatio", "criticalItems" },
                "CIA" => new[] { "openParas", "highRiskParas", "issueIdentified", "issueRectified", "settledMtd", "settlementRatio", "over1YearParas", "criticalItems" },
                "RLM" => new[] { "openParas", "highRiskParas", "settledMtd", "over1YearParas", "settlementRatio", "criticalItems" },
                _ => throw new ArgumentException("Unknown executive perspective.", nameof(perspective))
            };
            var priority = perspective == "CCO" ? "cycle" : perspective == "GROUP" ? "aging" : "risk";
            var reports = perspective switch
            {
                // Formal executive reports are approved for CIA, Compliance
                // and Group Management only. TOP/RLM must not expose a
                // reports tab, including through a manually crafted URL.
                "GROUP" => new[] { "E" },
                "CCO" => new[] { "B", "C", "D" },
                "CIA" => new[] { "A", "B", "C", "D", "E" },
                _ => Array.Empty<string>()
            };
            var views = new List<ExecutiveDashboardViewDefinition>
            {
                View("glance", "At a Glance", "_Glance", "Executive position and priority observations in your authorized scope.", glanceKpis, new[] { "trend", priority, "critical" }),
                View("position", "Audit Para Position", "_AuditParaPosition", "Current open position and risk mix. Historical opening and additions will follow in the reporting phase.", new[] { "totalCases", "openParas", "settledTotal", "highRiskParas" }, new[] { "risk" }),
                View("settlement", "Settlement & Performance", "_SettlementPerformance", "Existing monthly settlements and settlement ratio. Settlement-source and regional comparisons will follow in the reporting phase.", new[] { "settledMtd", "settlementMtdRate" }, perspective == "CCO" ? new[] { "trend", "cycle" } : new[] { "trend" }),
                View("risk", "Risk", "_Risk", "Risk distribution and authorized critical observations.", new[] { "highRiskParas", "criticalItems" }, new[] { "risk", "critical" }),
                View("aging", "Aging & Compliance", "_AgingCompliance", "Age and compliance cycles of open observations.", new[] { "over1YearParas", "zeroCycleParas", "over3CycleParas", "criticalItems" }, perspective == "CCO" ? new[] { "cycle", "aging", "critical" } : new[] { "aging", "cycle", "critical" }),
                new ExecutiveDashboardViewDefinition { Id = "reports", Title = "Reports", Partial = "_Reports", Description = "Formal reports relevant to this perspective, within the same authorized scope.", Kpis = Array.Empty<string>(), Sections = Array.Empty<string>(), Reports = reports }
            };
            // TOP Audit Para Position uses the existing genuinely dated
            // lifecycle cursor, not current-state total/open/settled counts.
            // Historical snapshot measures remain explicitly estimated.
            if (perspective == "TOP")
            {
                var position = views.First(v => v.Id == "position");
                position.Kpis = new[] { "openingOpenParas", "paraAdded",
                    "parasSettled", "closingOpenParas", "highRiskOpenParas" };
                position.Sections = Array.Empty<string>(); // no current-state risk chart
                position.UseLifecycleKpis = true;
            }
            views[0].UseLifecycleKpis = perspective == "TOP";
            if (perspective != "CIA" && perspective != "CCO" && perspective != "GROUP")
                views.RemoveAll(v => v.Id == "reports");
            if (perspective == "CIA")
                views.Insert(0, View("command", "CIA Audit Command Center", "_CiaCommandCenter",
                    "Plan coverage, delayed reports, A1 observations, pending auditee response, audit teams and ten issue categories.",
                    Array.Empty<string>(), Array.Empty<string>()));
            if (perspective == "GROUP" || perspective == "RLM")
                views.Insert(0, View("management", "ZTBL | Management Audit Dashboard", "_ManagementAudit",
                    "Current management para position, subordinate unit comparisons and bounded follow-up priorities.",
                    new[] { "openParas", "highRiskParas", "settledMtd", "over1YearParas" },
                    new[] { "risk", "trend" }));
            if (perspective == "RLM")
                views.Insert(0, View("regional", "Regional Audit Pulse", "_RegionalPulse", "Current branch risk and follow-up priorities within your reporting descendants.",
                    new[] { "openParas", "highRiskParas", "over1YearParas", "over3CycleParas" }, new[] { "risk", "aging", "cycle", "trend", "critical" }));
            if (perspective == "CCO") return CcoDashboardPresentation.Definitions(views.Single(v => v.Id == "reports"));
            return views;
        }

        private static ExecutiveDashboardViewDefinition View(string id, string title, string partial, string description, string[] kpis, string[] sections)
            => new ExecutiveDashboardViewDefinition { Id = id, Title = title, Partial = partial, Description = description, Kpis = kpis, Sections = sections };

        public static string ReportTitle(string id) => id switch
        {
            "A" => "Annual Field Audit Paras Progress",
            "B" => "Field Audit Position — Field Office",
            "C" => "Field Audit Position — GM / Region",
            "D" => "Top Five and Bottom Five Regions",
            "E" => "Division-wise Management Audit Settlement",
            _ => throw new ArgumentException("Unknown report.", nameof(id))
        };
    }
}

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
        public bool EnableGmPulseRoleDatasets { get; set; }
        public bool EnableGmPulseChartDatasets { get; set; }
        public string[] Datasets => (Kpis.Length > 0 && (!UseLifecycleKpis || Sections.Contains("critical"))
                ? new[] { "GetKpiSummary" } : Array.Empty<string>())
            .Concat(UseLifecycleKpis ? new[] { "GetKpiLifecycle" } : Array.Empty<string>())
            .Concat(Sections.Select(section => section switch { "risk" => "GetRiskSummary", "aging" => "GetAgingSummary",
                "cycle" => "GetCycleSummary", "trend" => "GetSettlementTrend", "critical" => "GetCriticalItems",
                _ => throw new InvalidOperationException("Unknown dashboard section.") }))
            .Concat(EnableGmPulseChartDatasets
                ? new[] { "GetRiskSummary", "GetAgingSummary", "GetCycleSummary", "GetSettlementTrend" }
                : Array.Empty<string>())
            .Distinct().ToArray();
        // Separate endpoints, fetched only by the scoped role-dashboard UI.
        // Keep them out of Datasets so the existing executive charts are untouched.
        public string[] RoleDatasets => Id == "command" ? new[] { "GetCiaCommandKpis", "GetCiaIssueNature" }
            : Id == "management" ? new[] { "GetRoleUnitComparison", "GetRoleFollowups" }
            : Id == "glance" && EnableGmPulseRoleDatasets
                ? new[] { "GetRoleEntityHeatmap", "GetRoleFollowups" }
                : Array.Empty<string>();
        public string[] Reports { get; set; } = Array.Empty<string>();
    }

    // Presentation definitions are chosen only after resolving the authorized perspective.
    public static class ExecutiveDashboardViews
    {
        public static List<ExecutiveDashboardViewDefinition> ForPerspective(string perspective)
        {
            var glanceKpis = perspective switch
            {
                "TOP" => new[] { "openingOpenParas", "issueIdentified", "issueRectified", "paraAdded", "netParaForCompliance", "parasSettled", "closingOpenParas", "highRiskOpenParas", "mediumRiskOpenParas", "lowRiskOpenParas" },
                "GROUP" => new[] { "openingOpenParas", "issueIdentified", "issueRectified", "paraAdded", "netParaForCompliance", "parasSettled", "closingOpenParas", "highRiskOpenParas", "mediumRiskOpenParas", "lowRiskOpenParas" },
                "CCO" => new[] { "openingOpenParas", "issueIdentified", "issueRectified", "paraAdded", "netParaForCompliance", "parasSettled", "closingOpenParas", "highRiskOpenParas", "mediumRiskOpenParas", "lowRiskOpenParas" },
                "CIA" => new[] { "openingOpenParas", "issueIdentified", "issueRectified", "paraAdded", "netParaForCompliance", "parasSettled", "closingOpenParas", "highRiskOpenParas", "mediumRiskOpenParas", "lowRiskOpenParas" },
                "RLM" => new[] { "openingOpenParas", "issueIdentified", "issueRectified", "paraAdded", "netParaForCompliance", "parasSettled", "closingOpenParas", "highRiskOpenParas", "mediumRiskOpenParas", "lowRiskOpenParas" },
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
                View("glance", "At a Glance", "_Glance", "Executive position in the authorized scope.", glanceKpis, perspective == "RLM" ? Array.Empty<string>() : new[] { "trend", priority }),
                View("position", "Audit Para Position", "_AuditParaPosition", "Current open position and risk mix. Historical opening and additions will follow in the reporting phase.", new[] { "totalCases", "openParas", "settledTotal", "highRiskParas" }, new[] { "risk" }),
                View("settlement", "Settlement & Performance", "_SettlementPerformance", "Existing monthly settlements and settlement ratio. Settlement-source and regional comparisons will follow in the reporting phase.", new[] { "settledMtd", "settlementMtdRate" }, perspective == "CCO" ? new[] { "trend", "cycle" } : new[] { "trend" }),
                View("risk", "High Risk", "_Risk", "High-risk observations in the authorized scope.", new[] { "highRiskParas" }, new[] { "critical" }),
                View("audit-paras", "Audit Para’s", "_AuditParas", "Authorized risk distribution and high-risk audit para register.", Array.Empty<string>(), new[] { "risk", "critical" }),
                View("aging", "Aging & Compliance", "_AgingCompliance", "Age and compliance cycles of open observations.", Array.Empty<string>(), perspective == "CCO" ? new[] { "cycle", "aging" } : new[] { "aging", "cycle" }),
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
            if (perspective == "GROUP")
            {
                // Management position is the canonical Group tab; the generic position tab duplicates it.
                views.RemoveAll(v => v.Id == "position");
                views.Single(v => v.Id == "reports").Title = "CCM Reports";
                // The Audit Para’s tab owns High-Risk detail; no duplicate High Risk tab.
                views.RemoveAll(v => v.Id == "risk");
            }
            if (perspective == "GROUP")
                views.Insert(0, View("management", "Audit Para Position", "_ManagementAudit",
                    "Nine dated lifecycle indicators for the Group reporting portfolio, followed by authorized organizational comparisons.",
                    glanceKpis, Array.Empty<string>()));
            if (perspective == "RLM")
            {
                // At a Glance replaces RLM's former five-card and generic Audit Para Position views.
                views.RemoveAll(v => v.Id == "position" || v.Id == "risk");
                // GM-only Regional Audit Pulse is embedded into RLM At a Glance
                // after verified root resolution, never exposed as a separate tab.
                // Admit its existing endpoints at the tab-dataset gate only
                // for RLM; the endpoint independently enforces GM Office type 21.
                var gmGlance = views.Single(v => v.Id == "glance");
                gmGlance.EnableGmPulseRoleDatasets = true;
                gmGlance.EnableGmPulseChartDatasets = true;
            }
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

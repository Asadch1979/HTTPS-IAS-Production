using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Models
{
    // Presentation only. The canonical report definition remains the API allowlist.
    public static class CcoDashboardPresentation
    {
        public static string ReportForView(string view) => view switch
        {
            "field-office" => "B", "gm-region" => "C", "regional-rankings" => "D", _ => null
        };

        public static string ResolveLegacy(string view, string report) => view switch
        {
            "glance" or "position" or "risk" or "aging" => "overview",
            "settlement" => "effectiveness",
            "reports" => string.IsNullOrEmpty(report) || report == "B" ? "field-office"
                : report == "C" ? "gm-region" : report == "D" ? "regional-rankings"
                : throw new ArgumentException("This report is not available for CCO."),
            _ => view
        };

        public static List<ExecutiveDashboardViewDefinition> Definitions(ExecutiveDashboardViewDefinition reports)
        {
            ExecutiveDashboardViewDefinition View(string id, string title, string[] keys, string[] sections, bool lifecycle = false)
                => new ExecutiveDashboardViewDefinition { Id=id, Title=title, Partial="_CcoCompliance",
                    Description="Compliance oversight within the authenticated CCO scope.", Kpis=keys, Sections=sections, UseLifecycleKpis=lifecycle };
            ExecutiveDashboardViewDefinition Report(string id, string title)
                => new ExecutiveDashboardViewDefinition { Id=id, Title=title, Partial="_Reports",
                    Kpis=Array.Empty<string>(), Sections=Array.Empty<string>(), Reports=new[]{ReportForView(id)} };
            return new List<ExecutiveDashboardViewDefinition> {
                View("overview", "Compliance Overview", new[]{"openingCases","paraAdded","totalCases","settledTotal","openParas","highRiskParas","over1YearParas"}, new[]{"trend","risk","aging","cycle","critical"}),
                View("org-position", "Group / Division / GM Position", Array.Empty<string>(), Array.Empty<string>()),
                Report("field-office", "Field Office Position"),
                Report("gm-region", "GM / Region Position"),
                Report("regional-rankings", "Regional Rankings"),
                View("performance", "Audit Performance Analysis", Array.Empty<string>(), Array.Empty<string>()),
                View("effectiveness", "Settlement Effectiveness", new[]{"settledMtd","settlementMtdRate","zeroCycleParas","over3CycleParas","over1YearParas"}, new[]{"trend","cycle","aging"}),
                reports
            };
        }
    }
}

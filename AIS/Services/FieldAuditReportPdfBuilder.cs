using AIS.Models.FieldAuditReport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AIS.Services
    {
    public class FieldAuditReportPdfBuilder
        {
        private static readonly string[] NarrativeSectionOrder =
            {
            "Fraud Prone Indicators",
            "Regulatory Violations",
            "Safety & Security",
            "Non-Addressable Findings",
            "Audit Recommendations",
            "Overall Conclusion",
            "Audit Limitation"
            };

        public string BuildHtml(FieldAuditPdfReportData data)
            {
            if (data == null)
                {
                throw new ArgumentNullException(nameof(data));
                }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: A4 portrait; margin: 36pt 36pt 54pt 36pt; }");
            sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 11pt; color: #000; }");
            sb.AppendLine("h1, h2, h3, h4 { margin: 12pt 0 6pt; }");
            sb.AppendLine("h1 { font-size: 20pt; text-align: center; }");
            sb.AppendLine("h2 { font-size: 15pt; }");
            sb.AppendLine("h3 { font-size: 13pt; }");
            sb.AppendLine("h4 { font-size: 12pt; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 8pt 0; }");
            sb.AppendLine("thead { display: table-header-group; }");
            sb.AppendLine("th, td { border: 1px solid #000; padding: 4pt 6pt; vertical-align: top; }");
            sb.AppendLine("tr { page-break-inside: avoid; }");
            sb.AppendLine(".section { page-break-after: auto; }");
            sb.AppendLine(".section-title { border-bottom: 1px solid #000; padding-bottom: 4pt; }");
            sb.AppendLine(".meta-grid { width: 100%; }");
            sb.AppendLine(".meta-label { width: 35%; font-weight: bold; }");
            sb.AppendLine(".paragraph { margin: 6pt 0; }");
            sb.AppendLine(".page-break { page-break-before: always; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            AppendCoverHeader(sb, data);
            AppendExecutiveSummary(sb, data);
            AppendBranchProfile(sb, data);
            AppendStaffPosition(sb, data);
            AppendKpiSnapshot(sb, data);
            AppendNplAnalysis(sb, data);
            AppendSignificantParas(sb, data);
            AppendAuditStatistics(sb, data);
            AppendIncomeLeakage(sb, data);
            AppendNarrativeSections(sb, data);
            AppendDetailedParas(sb, data);
            AppendStaticClauses(sb, data);
            AppendFooterSection(sb, data);

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
            }

        private static void AppendCoverHeader(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var header = data.Header ?? new FieldAuditPdfHeaderModel();
            var meta = data.ReportMeta ?? new FieldAuditPdfReportMetaModel();
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h1>Field Audit Report</h1>");
            sb.AppendLine("<table class=\"meta-grid\">");
            AppendMetaRow(sb, "Bank Name", header.BankName);
            AppendMetaRow(sb, "Internal Audit Division", header.InternalAuditDivision);
            AppendMetaRow(sb, "Branch Name", header.BranchName);
            AppendMetaRow(sb, "Branch Code", header.BranchCode);
            AppendMetaRow(sb, "Audit Period", header.AuditPeriod ?? meta.AuditPeriod);
            AppendMetaRow(sb, "Audit Dates", FormatDateRange(header.AuditStartDate, header.AuditEndDate));
            AppendMetaRow(sb, "Report Status", meta.ReportStatus ?? header.ReportStatus);
            AppendMetaRow(sb, "Version Number", meta.VersionNumber ?? header.VersionNumber);
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendExecutiveSummary(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Executive Summary</h2>");
            AppendNarrativeBlock(sb, "Executive Summary – Facts", FindSectionContent(data, "EXEC_SUMMARY_FACTS", "Executive Summary – Facts"));
            AppendNarrativeBlock(sb, "Executive Summary – Conclusion &amp; Key Messages", FindSectionContent(data, "EXEC_SUMMARY_CONCLUSION", "Executive Summary – Conclusion & Key Messages"));
            sb.AppendLine("</section>");
            }

        private static void AppendBranchProfile(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var header = data.Header ?? new FieldAuditPdfHeaderModel();
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Branch / Entity Profile</h2>");
            sb.AppendLine("<h3>Branch Profile</h3>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Profile Item</th><th>Details</th></tr></thead>");
            sb.AppendLine("<tbody>");

            var profileRows = header.BranchProfileRows?.Where(row => !string.IsNullOrWhiteSpace(row.Label)).ToList()
                ?? new List<FieldAuditPdfKeyValueModel>();

            if (profileRows.Count == 0)
                {
                AppendKeyValueRow(sb, "Branch Name", header.BranchName);
                AppendKeyValueRow(sb, "Branch Code", header.BranchCode);
                AppendKeyValueRow(sb, "Audit Period", header.AuditPeriod);
                AppendKeyValueRow(sb, "Audit Dates", FormatDateRange(header.AuditStartDate, header.AuditEndDate));
                }
            else
                {
                foreach (var row in profileRows)
                    {
                    AppendKeyValueRow(sb, row.Label, row.Value);
                    }
                }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            AppendNarrativeBlock(sb, "Key Statistics Narrative", FindSectionContent(data, "BRANCH_PROFILE", "Branch Profile"));
            sb.AppendLine("</section>");
            }

        private static void AppendStaffPosition(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Staff Position</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Designation</th><th>Strength</th><th>As-of Date</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in data.StaffRows ?? new List<FieldAuditPdfStaffRowModel>())
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(row.Designation));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", row.Strength?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatDate(row.AsOfDate));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendKpiSnapshot(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">KPI Snapshot</h2>");
            var kpiRows = data.KpiRows ?? new List<FieldAuditPdfKpiRowModel>();
            var periods = kpiRows.Select(row => row.PeriodEndDate)
                .Where(date => date.HasValue)
                .Select(date => date.Value)
                .Distinct()
                .OrderBy(date => date)
                .ToList();

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>KPI</th><th>Unit</th>");
            foreach (var period in periods)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", Encode(period.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
                }
            sb.AppendLine("</tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var group in kpiRows.GroupBy(row => new { row.KpiCode, row.KpiLabel, row.Unit }))
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(group.Key.KpiLabel));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(group.Key.Unit));
                foreach (var period in periods)
                    {
                    var match = group.FirstOrDefault(row => row.PeriodEndDate.HasValue && row.PeriodEndDate.Value.Date == period.Date);
                    var cellText = match == null
                        ? string.Empty
                        : $"{FormatNumber(match.ActualValue)} / {FormatNumber(match.TargetValue)}";
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(cellText));
                    }
                sb.AppendLine("</tr>");
                }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendNplAnalysis(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">NPL Analysis</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Category</th><th>Period End</th><th>Cases</th><th>Outstanding</th><th>Provision</th></tr></thead>");
            sb.AppendLine("<tbody>");
            var rows = data.NplRows ?? new List<FieldAuditPdfNplRowModel>();

            foreach (var row in rows.OrderBy(row => row.Category).ThenBy(row => row.PeriodEndDate))
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(row.Category));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatDate(row.PeriodEndDate));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", row.CaseCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(row.OutstandingAmount));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(row.ProvisionAmount));
                sb.AppendLine("</tr>");
                }

            foreach (var group in rows.GroupBy(row => row.Category))
                {
                var ordered = group.Where(item => item.PeriodEndDate.HasValue)
                    .OrderBy(item => item.PeriodEndDate)
                    .ToList();
                if (ordered.Count >= 2)
                    {
                    var latest = ordered[^1];
                    var previous = ordered[^2];
                    sb.AppendLine("<tr>");
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode($"{group.Key} Movement"));
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode($"{FormatDate(previous.PeriodEndDate)} → {FormatDate(latest.PeriodEndDate)}"));
                    sb.AppendLine("<td></td>");
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber((latest.OutstandingAmount ?? 0m) - (previous.OutstandingAmount ?? 0m)));
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber((latest.ProvisionAmount ?? 0m) - (previous.ProvisionAmount ?? 0m)));
                    sb.AppendLine("</tr>");
                    }
                }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendSignificantParas(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var significant = (data.Paras ?? new List<FieldAuditPdfParaModel>())
                .Where(para => string.Equals(para.Risk, "High", StringComparison.OrdinalIgnoreCase) && para.IsSignificant)
                .ToList();

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Significant Paras (High Risk)</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Para No</th><th>Gist</th><th>Nature</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var para in significant)
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(para.ParaNo));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(para.Gist));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(para.Nature));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendAuditStatistics(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Audit Statistics</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Risk</th><th>Reported</th><th>Rectified</th><th>Outstanding</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in data.StatisticsRows ?? new List<FieldAuditPdfStatisticsRowModel>())
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(row.RiskLevel));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", row.ReportedCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", row.RectifiedCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", row.OutstandingCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendIncomeLeakage(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var rows = data.IncomeLeakageRows ?? new List<FieldAuditPdfIncomeLeakageRowModel>();
            if (rows.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Income Leakage</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Case</th><th>Description</th><th>Amount</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in rows)
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(row.CaseReference));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(row.Description));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(row.Amount));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("<tr>");
            sb.AppendLine("<td colspan=\"2\"><strong>Total</strong></td>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(rows.Sum(row => row.Amount ?? 0m)));
            sb.AppendLine("</tr>");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendNarrativeSections(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Narrative Sections</h2>");
            foreach (var title in NarrativeSectionOrder)
                {
                var content = FindSectionContent(data, null, title);
                if (string.IsNullOrWhiteSpace(content))
                    {
                    continue;
                    }

                AppendNarrativeBlock(sb, title, content);
                }
            sb.AppendLine("</section>");
            }

        private static void AppendDetailedParas(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var paras = data.Paras ?? new List<FieldAuditPdfParaModel>();
            if (paras.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Audit Paras – Detailed</h2>");

            foreach (var group in paras.GroupBy(para => para.Risk).OrderBy(group => group.Key))
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<h3>{0} Risk Paras</h3>", Encode(group.Key));
                foreach (var para in group)
                    {
                    sb.AppendLine("<div class=\"paragraph\">");
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<h4>Para {0}</h4>", Encode(para.ParaNo));
                    AppendKeyValueParagraph(sb, "Annexure Code", para.AnnexureCode);
                    AppendKeyValueParagraph(sb, "Instances", para.Instances);
                    AppendKeyValueParagraph(sb, "Amount", para.Amount);
                    AppendKeyValueParagraph(sb, "Gist", para.Gist);
                    AppendKeyValueParagraph(sb, "Para Detail", para.ParaDetail);
                    AppendKeyValueParagraph(sb, "Implications", para.Implications);
                    AppendKeyValueParagraph(sb, "Recommendations", para.Recommendations);
                    AppendKeyValueParagraph(sb, "Management Comments", para.ManagementComments);
                    AppendKeyValueParagraph(sb, "Auditor Comments", para.AuditorComments);
                    AppendKeyValueParagraph(sb, "Remarks of SVP/In-charge", para.RemarksInCharge);
                    sb.AppendLine("</div>");
                    }
                }

            sb.AppendLine("</section>");
            }

        private static void AppendStaticClauses(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Static Clauses</h2>");
            AppendNarrativeBlock(sb, "Disclaimer", FindSectionContent(data, "DISCLAIMER", "Disclaimer"));
            AppendNarrativeBlock(sb, "Restriction Clause", FindSectionContent(data, "RESTRICTION_CLAUSE", "Restriction Clause"));
            sb.AppendLine("</section>");
            }

        private static void AppendFooterSection(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var meta = data.ReportMeta ?? new FieldAuditPdfReportMetaModel();
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Footer</h2>");
            sb.AppendLine("<table class=\"meta-grid\">");
            AppendMetaRow(sb, "Generated By", meta.GeneratedBy);
            AppendMetaRow(sb, "Generated On", FormatDate(meta.GeneratedOn));
            AppendMetaRow(sb, "System Name", "IAS");
            AppendMetaRow(sb, "Confidentiality Note", "Confidential - Internal Use Only");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendMetaRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"meta-label\">{0}</td>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendKeyValueRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendKeyValueParagraph(StringBuilder sb, string label, string value)
            {
            if (string.IsNullOrWhiteSpace(label))
                {
                return;
                }

            sb.AppendFormat(CultureInfo.InvariantCulture, "<p><strong>{0}:</strong> {1}</p>", Encode(label), Encode(value));
            }

        private static void AppendNarrativeBlock(StringBuilder sb, string title, string htmlContent)
            {
            if (string.IsNullOrWhiteSpace(htmlContent))
                {
                return;
                }

            sb.AppendFormat(CultureInfo.InvariantCulture, "<h3>{0}</h3>", Encode(title));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"paragraph\">{0}</div>", htmlContent);
            }

        private static string FindSectionContent(FieldAuditPdfReportData data, string code, string title)
            {
            var sections = data.Sections ?? new List<FieldAuditPdfSectionModel>();
            FieldAuditPdfSectionModel section = null;
            if (!string.IsNullOrWhiteSpace(code))
                {
                section = sections.FirstOrDefault(item => string.Equals(item.SectionCode, code, StringComparison.OrdinalIgnoreCase));
                }
            if (section == null && !string.IsNullOrWhiteSpace(title))
                {
                section = sections.FirstOrDefault(item => string.Equals(item.SectionTitle, title, StringComparison.OrdinalIgnoreCase));
                }
            return section?.HtmlContent;
            }

        private static string Encode(string input)
            {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : WebUtility.HtmlEncode(input);
            }

        private static string FormatDate(DateTime? date)
            {
            return date.HasValue ? date.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty;
            }

        private static string FormatDateRange(DateTime? start, DateTime? end)
            {
            var startText = FormatDate(start);
            var endText = FormatDate(end);
            if (string.IsNullOrWhiteSpace(startText) && string.IsNullOrWhiteSpace(endText))
                {
                return string.Empty;
                }

            return $"{startText} - {endText}".Trim(' ', '-');
            }

        private static string FormatNumber(decimal? value)
            {
            return value.HasValue ? value.Value.ToString("N2", CultureInfo.InvariantCulture) : string.Empty;
            }
        }
    }

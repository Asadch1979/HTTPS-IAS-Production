using AIS.Models.FieldAuditReport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
            sb.AppendLine("body{ font-family: Arial, Helvetica, sans-serif; font-size: 12px; color:#111; margin: 22px; }");
            sb.AppendLine("h1, h2, h3, h4 { margin: 12pt 0 6pt; }");
            sb.AppendLine("h1 { font-size: 20pt; text-align: center; font-weight: 700; }");
            sb.AppendLine("h2 { font-size: 14pt; }");
            sb.AppendLine("h3 { font-size: 13pt; }");
            sb.AppendLine("h4 { font-size: 12pt; }");
            sb.AppendLine("table, tr, td, th { page-break-inside: avoid; break-inside: avoid; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 8pt 0; }");
            sb.AppendLine("thead { display: table-header-group; }");
            sb.AppendLine("tfoot { display: table-footer-group; }");
            sb.AppendLine("th, td { border: 1px solid #111; padding: 4pt 6pt; vertical-align: top; }");
            sb.AppendLine(".grid{ width:100%; border-collapse:collapse; table-layout:fixed; margin-top:8px; }");
            sb.AppendLine(".grid th,.grid td{ border:1px solid #111; padding:6px; font-size:12px; vertical-align:top; }");
            sb.AppendLine(".grid th{ background:#f6f8fa; font-weight:700; text-align:center; }");
            sb.AppendLine(".grid td{ text-align:center; }");
            sb.AppendLine(".grid td.left{ text-align:left; }");
            sb.AppendLine(".cover-page{ text-align:center; padding-top:24pt; }");
            sb.AppendLine(".cover-logo{ text-align:center; margin-bottom:16pt; }");
            sb.AppendLine(".cover-logo img{ height:72px; }");
            sb.AppendLine(".cover-subtitle{ font-size:14pt; font-weight:600; margin-top:6pt; }");
            sb.AppendLine(".cover-report-title{ font-size:16pt; font-weight:700; margin-top:10pt; }");
            sb.AppendLine(".cover-box{ border:1px solid #111; padding:14pt 18pt; width:68%; margin:20pt auto 18pt; }");
            sb.AppendLine(".cover-confidential{ margin-top:32pt; font-size:12pt; font-weight:600; }");
            sb.AppendLine(".section { page-break-after: auto; }");
            sb.AppendLine(".section-title{ font-size:14px; font-weight:700; margin:18px 0 10px 0; padding:8px 10px; border-left:4px solid #111; background:#f6f8fa; }");
            sb.AppendLine(".meta-grid { width: 100%; }");
            sb.AppendLine(".meta-label { width: 35%; font-weight: bold; }");
            sb.AppendLine(".paragraph { margin: 6pt 0; }");
            sb.AppendLine(".para-box{ border:1px solid #d0d7de; border-radius:8px; padding:12px 14px; margin:10px 0; background:#fff; page-break-inside:avoid; break-inside:avoid; }");
            sb.AppendLine(".para-title{ font-weight:700; font-size:13px; margin:0 0 8px 0; color:#111; }");
            sb.AppendLine(".para-body{ font-size:12px; line-height:1.6; text-align:justify; white-space:normal !important; overflow-wrap:anywhere; color:#212529; width:100%; }");
            sb.AppendLine(".para-body *{ white-space:normal !important; overflow-wrap:anywhere; font-size:12px !important; }");
            sb.AppendLine(".para-body h1, .para-body h2, .para-body h3{ font-size:13px !important; margin:6px 0 !important; }");
            sb.AppendLine(".para-body table{ width:100%; border-collapse:collapse; margin-top:10px; }");
            sb.AppendLine(".para-body table th,.para-body table td{ border:1px solid #222; padding:4px 6px; vertical-align:top; }");
            sb.AppendLine(".para-sep{ border:0; border-top:1px solid #d0d7de; margin:14px 0; }");
            sb.AppendLine(".exec-summary{ line-height:1.8; }");
            sb.AppendLine(".chart-block{ text-align:center; margin:10pt 0 12pt; }");
            sb.AppendLine(".chart-img{ width:100%; max-height:260px; object-fit:contain; }");
            sb.AppendLine(".chart-caption{ font-size:11px; color:#444; margin-top:4pt; }");
            sb.AppendLine(".page-break{ page-break-before: always; break-before: page; }");
            sb.AppendLine(".break-after{ page-break-after: always; break-after: page; }");
            sb.AppendLine(".avoid-break{ page-break-inside: avoid; break-inside: avoid; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            AppendCoverHeader(sb, data);
            sb.AppendLine("<div class=\"page-break\"></div>");
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
            var entityName = header.EntityName ?? meta.EntityName ?? header.BranchName;
            sb.AppendLine("<section class=\"section cover-page\">");
            sb.AppendLine("<div class=\"cover-logo\"><img src=\"/Images/ztbllogo.png\" alt=\"ZTBL logo\" /></div>");
            sb.AppendLine("<div>");
            sb.AppendLine("<h1>Zarai Taraqiati Bank Limited</h1>");
            sb.AppendLine("<div class=\"cover-subtitle\">Internal Audit Division</div>");
            sb.AppendLine("<div class=\"cover-report-title\">Field Audit Report</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"cover-box\">");
            sb.AppendLine("<table class=\"meta-grid\">");
            AppendMetaRow(sb, "Entity", entityName);
            AppendMetaRow(sb, "Branch Code", header.BranchCode);
            AppendMetaRow(sb, "Audit Period", header.AuditPeriod ?? meta.AuditPeriod);
            AppendMetaRow(sb, "Audit Dates", FormatDateRange(header.AuditStartDate, header.AuditEndDate));
            AppendMetaRow(sb, "Status", meta.ReportStatus ?? header.ReportStatus);
            AppendMetaRow(sb, "Version", meta.VersionNumber ?? header.VersionNumber);
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"cover-confidential\">Confidential \u2013 Internal Use Only</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendExecutiveSummary(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var facts = FindSectionContent(data, "EXEC_SUMMARY_FACTS", "Executive Summary – Facts");
            var conclusions = FindSectionContent(data, "EXEC_SUMMARY_CONCLUSION", "Executive Summary – Conclusion & Key Messages");
            var factsParagraphs = NormalizeNarrativeParagraphs(facts ?? string.Empty);
            var conclusionParagraphs = NormalizeNarrativeParagraphs(conclusions ?? string.Empty);
            if (string.IsNullOrWhiteSpace(factsParagraphs) && string.IsNullOrWhiteSpace(conclusionParagraphs))
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Executive Summary</h2>");
            AppendExecutiveSummaryBlock(sb, "Executive Summary – Facts", facts);
            AppendExecutiveSummaryBlock(sb, "Executive Summary – Conclusion &amp; Key Messages", conclusions);
            sb.AppendLine("</section>");
            }

        private static void AppendBranchProfile(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var header = data.Header ?? new FieldAuditPdfHeaderModel();
            var profileRows = header.BranchProfileRows?.Where(row => !string.IsNullOrWhiteSpace(row.Label)).ToList()
                ?? new List<FieldAuditPdfKeyValueModel>();
            var profileNarrative = FindSectionContent(data, "BRANCH_PROFILE", "Branch Profile");
            var hasProfileContent = profileRows.Any(row => !string.IsNullOrWhiteSpace(row.Value))
                || HasAnyValue(header.BranchName, header.BranchCode, header.AuditPeriod, FormatDateRange(header.AuditStartDate, header.AuditEndDate))
                || !string.IsNullOrWhiteSpace(NormalizeHtml(profileNarrative));

            if (!hasProfileContent)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Branch / Entity Profile</h2>");
            sb.AppendLine("<h3>Branch Profile</h3>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Profile Item</th><th>Details</th></tr></thead>");
            sb.AppendLine("<tbody>");

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

            AppendNarrativeBlock(sb, "Key Statistics Narrative", profileNarrative);
            sb.AppendLine("</section>");
            }

        private static void AppendStaffPosition(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var staffRows = data.StaffRows ?? new List<FieldAuditPdfStaffRowModel>();
            if (staffRows.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Staff Position</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Designation</th><th>Strength</th><th>As-of Date</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in staffRows)
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
            var kpiRows = data.KpiRows ?? new List<FieldAuditPdfKpiRowModel>();
            var chartSources = ExtractChartSources(data, "KPI_CHART", "KPI_CHARTS", "KPI Snapshot Chart", "KPI Charts", "KPI Chart");
            if (kpiRows.Count == 0 && chartSources.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">KPI Snapshot</h2>");
            sb.AppendLine("<div class=\"avoid-break\">");
            AppendChartBlocks(sb, chartSources, "KPI Snapshot Chart");
            if (kpiRows.Count > 0)
                {
                var periods = kpiRows.Select(row => row.PeriodEndDate)
                    .Where(date => date.HasValue)
                    .Select(date => date.Value)
                    .Distinct()
                    .OrderBy(date => date)
                    .ToList();

                var startDate = periods.Count > 0 ? periods.First() : (DateTime?)null;
                var endDate = periods.Count > 0 ? periods.Last() : (DateTime?)null;

                sb.AppendLine("<table class=\"grid\">");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th rowspan=\"2\" style=\"width:40px\">Sr. No.</th>");
            sb.AppendLine("<th rowspan=\"2\">KPIs</th>");
            sb.AppendLine("<th colspan=\"1\">As on Date of Audit</th>");
            sb.AppendLine("<th colspan=\"3\">Audit Operation Period Start date (Rs. in Millions)</th>");
            sb.AppendLine("<th colspan=\"3\">Audit Operation Period End Date (Rs. in Millions)</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Actual</th>");
            sb.AppendLine("<th>Target</th><th>Actual</th><th>%age</th>");
            sb.AppendLine("<th>Target</th><th>Actual</th><th>%age</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            var groupedKpis = kpiRows.GroupBy(row =>
                string.IsNullOrWhiteSpace(row.KpiCode) ? row.KpiLabel ?? string.Empty : row.KpiCode);
            var index = 1;
            foreach (var group in groupedKpis.OrderBy(group => group.Key))
                {
                var kpiLabel = group.Select(row => row.KpiLabel).FirstOrDefault(label => !string.IsNullOrWhiteSpace(label))
                    ?? group.Key;
                var startRow = GetKpiRowForDate(group, startDate);
                var endRow = GetKpiRowForDate(group, endDate);
                var latestRow = group.Where(row => row.PeriodEndDate.HasValue)
                    .OrderByDescending(row => row.PeriodEndDate.Value)
                    .FirstOrDefault();

                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", index++);
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(kpiLabel));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber((endRow ?? latestRow)?.ActualValue));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(startRow?.TargetValue));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(startRow?.ActualValue));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatPercent(startRow?.ActualValue, startRow?.TargetValue));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(endRow?.TargetValue));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(endRow?.ActualValue));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatPercent(endRow?.ActualValue, endRow?.TargetValue));
                sb.AppendLine("</tr>");
                }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
                }
            sb.AppendLine("</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendNplAnalysis(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var rows = data.NplRows ?? new List<FieldAuditPdfNplRowModel>();
            var chartSources = ExtractChartSources(data, "NPL_CHART", "NPL_CHARTS", "NPL Composition Chart", "NPL Chart", "NPL Charts");
            if (rows.Count == 0 && chartSources.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">NPL Analysis</h2>");
            sb.AppendLine("<div class=\"avoid-break\">");
            AppendChartBlocks(sb, chartSources, "NPL Composition");
            if (rows.Count > 0)
                {
                sb.AppendLine("<table class=\"grid\">");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th rowspan=\"2\" style=\"width:140px\">NPL Portfolio</th>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th colspan=\"3\">{0}</th>", Encode(GetNplDateLabel(rows, true)));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th colspan=\"3\">{0}</th>", Encode(GetNplDateLabel(rows, false)));
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Cases</th><th>Outstanding Principal (Rs.)</th><th>Provision Amount (Rs.)</th>");
            sb.AppendLine("<th>Cases</th><th>Outstanding Principal (Rs.)</th><th>Provision Amount (Rs.)</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            var orderedDates = rows.Select(row => row.PeriodEndDate)
                .Where(date => date.HasValue)
                .Select(date => date.Value.Date)
                .Distinct()
                .OrderBy(date => date)
                .ToList();
            DateTime? date1 = null;
            DateTime? date2 = null;
            if (orderedDates.Count > 0)
                {
                if (orderedDates.Count > 2)
                    {
                    date1 = orderedDates[^2];
                    date2 = orderedDates[^1];
                    }
                else
                    {
                    date1 = orderedDates.First();
                    date2 = orderedDates.Last();
                    }
                }
            var buckets = new[] { "OAEM", "Substandard", "Doubtful", "Loss" };
            var totalsDate1 = new NplAggregate();
            var totalsDate2 = new NplAggregate();

            foreach (var bucket in buckets)
                {
                var bucketDate1 = AggregateNpl(rows, bucket, date1);
                var bucketDate2 = AggregateNpl(rows, bucket, date2);
                totalsDate1.Add(bucketDate1);
                totalsDate2.Add(bucketDate2);

                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(bucket));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatInteger(bucketDate1.Cases));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(bucketDate1.Outstanding));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(bucketDate1.Provision));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatInteger(bucketDate2.Cases));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(bucketDate2.Outstanding));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatNumber(bucketDate2.Provision));
                sb.AppendLine("</tr>");
                }

            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\"><strong>Total</strong></td>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><strong>{0}</strong></td>", FormatInteger(totalsDate1.Cases));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><strong>{0}</strong></td>", FormatNumber(totalsDate1.Outstanding));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><strong>{0}</strong></td>", FormatNumber(totalsDate1.Provision));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><strong>{0}</strong></td>", FormatInteger(totalsDate2.Cases));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><strong>{0}</strong></td>", FormatNumber(totalsDate2.Outstanding));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><strong>{0}</strong></td>", FormatNumber(totalsDate2.Provision));
            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
                }
            sb.AppendLine("</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendSignificantParas(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var significant = (data.Paras ?? new List<FieldAuditPdfParaModel>())
                .Where(para => string.Equals(para.Risk, "High", StringComparison.OrdinalIgnoreCase) && para.IsSignificant)
                .ToList();

            if (significant.Count == 0)
                {
                return;
                }

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
            var rows = data.StatisticsRows ?? new List<FieldAuditPdfStatisticsRowModel>();
            if (rows.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Audit Statistics</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Risk</th><th>Reported</th><th>Rectified</th><th>Outstanding</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in rows)
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
            var narrativeBlocks = NarrativeSectionOrder
                .Select(title => new
                    {
                    Title = title,
                    Content = FindSectionContent(data, null, title)
                    })
                .Where(item => !string.IsNullOrWhiteSpace(item.Content))
                .ToList();

            if (narrativeBlocks.Count == 0)
                {
                return;
                }

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
                    sb.AppendLine("<div class=\"para-box\">");
                    var paraTitle = BuildParaTitle(para);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"para-title\">{0}</div>", paraTitle);
                    sb.AppendLine("<div class=\"para-body\">");
                    AppendParaDetailContent(sb, para.ParaDetail);
                    AppendResponsibilityTable(sb, para);
                    AppendParaRemarksGrid(sb, para);
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                    sb.AppendLine("<hr class=\"para-sep\" />");
                    }
                }

            sb.AppendLine("</section>");
            }

        private static void AppendStaticClauses(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var disclaimer = FindSectionContent(data, "DISCLAIMER", "Disclaimer");
            var restriction = FindSectionContent(data, "RESTRICTION_CLAUSE", "Restriction Clause");
            if (string.IsNullOrWhiteSpace(disclaimer) && string.IsNullOrWhiteSpace(restriction))
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Static Clauses</h2>");
            AppendNarrativeBlock(sb, "Disclaimer", disclaimer);
            AppendNarrativeBlock(sb, "Restriction Clause", restriction);
            sb.AppendLine("</section>");
            }

        private static void AppendFooterSection(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var meta = data.ReportMeta ?? new FieldAuditPdfReportMetaModel();
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Footer</h2>");
            sb.AppendLine("<table class=\"meta-grid\">");
            if (!string.IsNullOrWhiteSpace(meta.GeneratedBy))
                {
                AppendMetaRow(sb, "Generated By", meta.GeneratedBy);
                }
            var generatedOn = FormatDate(meta.GeneratedOn);
            if (!string.IsNullOrWhiteSpace(generatedOn))
                {
                AppendMetaRow(sb, "Generated On", generatedOn);
                }
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
            var normalizedHtml = NormalizeHtml(htmlContent);
            if (string.IsNullOrWhiteSpace(normalizedHtml))
                {
                return;
                }
            LogSectionHtmlSnippet(title, normalizedHtml);
            sb.Append("<div class=\"paragraph\">");
            sb.Append(normalizedHtml);
            sb.AppendLine("</div>");
            }

        private static void AppendExecutiveSummaryBlock(StringBuilder sb, string title, string htmlContent)
            {
            if (string.IsNullOrWhiteSpace(htmlContent))
                {
                return;
                }

            var paragraphs = NormalizeNarrativeParagraphs(htmlContent);
            if (string.IsNullOrWhiteSpace(paragraphs))
                {
                return;
                }

            sb.AppendFormat(CultureInfo.InvariantCulture, "<h3>{0}</h3>", Encode(title));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"exec-summary\">{0}</div>", paragraphs);
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

        private static List<string> ExtractChartSources(FieldAuditPdfReportData data, params string[] identifiers)
            {
            var sources = new List<string>();
            var sections = data.Sections ?? new List<FieldAuditPdfSectionModel>();
            foreach (var identifier in identifiers)
                {
                var section = sections.FirstOrDefault(item =>
                    string.Equals(item.SectionCode, identifier, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.SectionTitle, identifier, StringComparison.OrdinalIgnoreCase));
                if (section == null || string.IsNullOrWhiteSpace(section.HtmlContent))
                    {
                    continue;
                    }

                sources.AddRange(ExtractImageSources(section.HtmlContent));
                }

            return sources.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

        private static IEnumerable<string> ExtractImageSources(string htmlContent)
            {
            if (string.IsNullOrWhiteSpace(htmlContent))
                {
                return Array.Empty<string>();
                }

            var normalized = NormalizeHtml(htmlContent);
            var matches = Regex.Matches(normalized, "<img[^>]*?src=[\"'](?<src>[^\"']+)[\"'][^>]*?>", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
                {
                return matches.Select(match => match.Groups["src"].Value).Where(src => !string.IsNullOrWhiteSpace(src));
                }

            var trimmed = normalized.Trim();
            return string.IsNullOrWhiteSpace(trimmed)
                ? Array.Empty<string>()
                : new[] { trimmed };
            }

        private static void AppendChartBlocks(StringBuilder sb, List<string> sources, string caption)
            {
            if (sources == null || sources.Count == 0)
                {
                return;
                }

            foreach (var source in sources)
                {
                sb.AppendLine("<div class=\"chart-block\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<img class=\"chart-img\" src=\"{0}\" alt=\"{1}\" />", EncodeAttribute(source), Encode(caption));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"chart-caption\">{0}</div>", Encode(caption));
                sb.AppendLine("</div>");
                }
            }

        private static string Encode(string input)
            {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : WebUtility.HtmlEncode(input);
            }

        private static string EncodeAttribute(string input)
            {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : WebUtility.HtmlAttributeEncode(input);
            }

        private static string NormalizeHtml(string? html)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                return string.Empty;
                }

            string previous;
            var current = html;
            do
                {
                previous = current;
                current = WebUtility.HtmlDecode(previous);
                }
            while (!string.Equals(previous, current, StringComparison.Ordinal));

            return current;
            }

        private static string NormalizeNarrativeParagraphs(string html)
            {
            var normalized = NormalizeHtml(html);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return string.Empty;
                }

            normalized = Regex.Replace(normalized, "<style[\\s\\S]*?</style>", string.Empty, RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, "<script[\\s\\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, "<table[\\s\\S]*?</table>", string.Empty, RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, "<\\s*br\\s*/?>", "\n", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, "</p>", "\n", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, "<[^>]+>", string.Empty);
            normalized = WebUtility.HtmlDecode(normalized);

            var lines = normalized.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count == 0)
                {
                return string.Empty;
                }

            var builder = new StringBuilder();
            foreach (var line in lines)
                {
                builder.AppendFormat(CultureInfo.InvariantCulture, "<p>{0}</p>", Encode(line));
                }

            return builder.ToString();
            }

        private static bool HasAnyValue(params string[] values)
            {
            return values.Any(value => !string.IsNullOrWhiteSpace(value));
            }

        private static string StripParaTextPrefix(string? html)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                return string.Empty;
                }

            const string prefix = "Para Text:";
            var trimmed = html.TrimStart();
            return trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? trimmed.Substring(prefix.Length).TrimStart()
                : html;
            }

        private static void LogSectionHtmlSnippet(string title, string html)
            {
            if (string.IsNullOrEmpty(html))
                {
                return;
                }

            var snippet = html.Length > 200 ? html.Substring(0, 200) : html;
            Trace.WriteLine($"[FieldAuditReportPdfBuilder] Section HTML ({title}) snippet: {snippet}");
            }

        private static void AppendParaDetailContent(StringBuilder sb, string htmlContent)
            {
            if (string.IsNullOrWhiteSpace(htmlContent))
                {
                return;
                }

            var normalizedHtml = NormalizeHtml(htmlContent);
            normalizedHtml = StripParaTextPrefix(normalizedHtml);
            if (string.IsNullOrWhiteSpace(normalizedHtml))
                {
                return;
                }

            sb.Append(normalizedHtml);
            }

        private static void AppendResponsibilityTable(StringBuilder sb, FieldAuditPdfParaModel para)
            {
            var hasResponsibility = HasAnyValue(para.AnnexureCode, para.Instances, para.Amount, para.Gist);
            if (!hasResponsibility)
                {
                return;
                }

            sb.AppendLine("<table class=\"grid\">");
            sb.AppendLine("<thead><tr><th>Annexure Code</th><th>Instances</th><th>Amount</th><th>Title / Gist</th></tr></thead>");
            sb.AppendLine("<tbody>");
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(para.AnnexureCode));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(para.Instances));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(para.Amount));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(para.Gist));
            sb.AppendLine("</tr>");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            }

        private static void AppendParaRemarksGrid(StringBuilder sb, FieldAuditPdfParaModel para)
            {
            var hasRemarks = HasAnyValue(para.Implications, para.Recommendations, para.ManagementComments, para.AuditorComments, para.RemarksInCharge);
            if (!hasRemarks)
                {
                return;
                }

            sb.AppendLine("<table class=\"grid\">");
            sb.AppendLine("<thead><tr><th>Implications</th><th>Recommendations</th><th>Mgmt Comments</th><th>Auditor Comments</th><th>SVP Remarks</th></tr></thead>");
            sb.AppendLine("<tbody>");
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(para.Implications));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(para.Recommendations));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(para.ManagementComments));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(para.AuditorComments));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", Encode(para.RemarksInCharge));
            sb.AppendLine("</tr>");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            }

        private static string BuildParaTitle(FieldAuditPdfParaModel para)
            {
            var number = string.IsNullOrWhiteSpace(para.ParaNo) ? string.Empty : $"Para {para.ParaNo}";
            var title = string.IsNullOrWhiteSpace(para.Gist) ? string.Empty : para.Gist.Trim();
            if (string.IsNullOrWhiteSpace(number))
                {
                return Encode(title);
                }

            return string.IsNullOrWhiteSpace(title)
                ? Encode(number)
                : Encode($"{number} - {title}");
            }

        private static FieldAuditPdfKpiRowModel GetKpiRowForDate(IEnumerable<FieldAuditPdfKpiRowModel> rows, DateTime? date)
            {
            if (!date.HasValue)
                {
                return null;
                }

            return rows.FirstOrDefault(row => row.PeriodEndDate.HasValue && row.PeriodEndDate.Value.Date == date.Value.Date);
            }

        private static string FormatPercent(decimal? actual, decimal? target)
            {
            if (!actual.HasValue || !target.HasValue || target.Value == 0m)
                {
                return "-";
                }

            return Percent(actual.Value, target.Value);
            }

        private static string Percent(decimal actual, decimal target)
            {
            return target == 0m
                ? "-"
                : ((actual / target) * 100m).ToString("0.00", CultureInfo.InvariantCulture);
            }

        private static string GetNplDateLabel(List<FieldAuditPdfNplRowModel> rows, bool isStart)
            {
            var orderedDates = rows.Select(row => row.PeriodEndDate)
                .Where(date => date.HasValue)
                .Select(date => date.Value.Date)
                .Distinct()
                .OrderBy(date => date)
                .ToList();

            if (orderedDates.Count == 0)
                {
                return string.Empty;
                }

            if (orderedDates.Count > 2)
                {
                return orderedDates[isStart ? orderedDates.Count - 2 : orderedDates.Count - 1]
                    .ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                }

            return (isStart ? orderedDates.First() : orderedDates.Last())
                .ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }

        private static NplAggregate AggregateNpl(IEnumerable<FieldAuditPdfNplRowModel> rows, string bucket, DateTime? date)
            {
            if (!date.HasValue)
                {
                return new NplAggregate();
                }

            var matches = rows.Where(row =>
                row.PeriodEndDate.HasValue
                && row.PeriodEndDate.Value.Date == date.Value.Date
                && string.Equals(row.Category, bucket, StringComparison.OrdinalIgnoreCase));

            return new NplAggregate
                {
                Cases = matches.Sum(row => row.CaseCount ?? 0),
                Outstanding = matches.Sum(row => row.OutstandingAmount ?? 0m),
                Provision = matches.Sum(row => row.ProvisionAmount ?? 0m)
                };
            }

        private static string FormatInteger(int? value)
            {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
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

        private sealed class NplAggregate
            {
            public int Cases { get; set; }
            public decimal Outstanding { get; set; }
            public decimal Provision { get; set; }

            public void Add(NplAggregate other)
                {
                if (other == null)
                    {
                    return;
                    }

                Cases += other.Cases;
                Outstanding += other.Outstanding;
                Provision += other.Provision;
                }
            }
        }
    }

using AIS.Models.FieldAuditReport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            sb.AppendLine("h1 { font-size: 18pt; text-align: center; }");
            sb.AppendLine("h2 { font-size: 14pt; }");
            sb.AppendLine("h3 { font-size: 13pt; }");
            sb.AppendLine("h4 { font-size: 12pt; }");
            sb.AppendLine("tr, td, th { page-break-inside: avoid; break-inside: avoid; }");
            sb.AppendLine("h1, h2, h3, h4, .section-title { page-break-after: avoid; break-after: avoid; }");
            sb.AppendLine("p { orphans: 3; widows: 3; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 8pt 0; }");
            sb.AppendLine("thead { display: table-header-group; }");
            sb.AppendLine("tfoot { display: table-footer-group; }");
            sb.AppendLine("th, td { border: 1px solid #111; padding: 4pt 6pt; vertical-align: top; }");
            sb.AppendLine(".grid{ width:100%; border-collapse:collapse; table-layout:fixed; margin-top:8px; }");
            sb.AppendLine(".grid th,.grid td{ border:1px solid #111; padding:6px; font-size:12px; vertical-align:top; }");
            sb.AppendLine(".grid th{ background:#f6f8fa; font-weight:700; text-align:center; }");
            sb.AppendLine(".grid td{ text-align:center; }");
            sb.AppendLine(".grid td.left{ text-align:left; }");
            sb.AppendLine(".grid th.left{ text-align:left; }");
            sb.AppendLine(".section { page-break-after: auto; }");
            sb.AppendLine(".cover-page{ text-align:center; }");
            sb.AppendLine(".cover-logo{ text-align:left; margin-bottom:18px; }");
            sb.AppendLine(".cover-logo img{ height:70px; }");
            sb.AppendLine(".cover-headings{ margin:10px 0 18px; }");
            sb.AppendLine(".cover-headings .bank-name{ font-size:18pt; font-weight:700; margin:0; }");
            sb.AppendLine(".cover-headings .division{ font-size:13pt; margin-top:6px; }");
            sb.AppendLine(".cover-headings .report-title{ font-size:15pt; font-weight:700; margin-top:10px; }");
            sb.AppendLine(".cover-box{ border:1px solid #111; padding:16px 18px; margin:16px auto 0; width:68%; text-align:left; }");
            sb.AppendLine(".cover-box table{ border:none; margin:0; }");
            sb.AppendLine(".cover-box td{ border:none; padding:4px 6px; }");
            sb.AppendLine(".cover-confidential{ margin-top:22px; font-weight:700; }");
            sb.AppendLine(".section-title{ font-size:14px; font-weight:700; margin:18px 0 10px 0; padding:8px 10px; border-left:4px solid #111; background:#f6f8fa; }");
            sb.AppendLine(".exec-summary{ line-height:1.8; text-align:justify; }");
            sb.AppendLine(".exec-summary p{ margin:0 0 10px; }");
            sb.AppendLine(".chart-block{ margin:12px 0; text-align:center; }");
            sb.AppendLine(".chart-img{ width:100%; max-height:260px; object-fit:contain; }");
            sb.AppendLine(".chart-caption{ font-size:11px; color:#444; margin-top:4px; }");
            sb.AppendLine(".chart-html{ width:100%; }");
            sb.AppendLine(".chart-html svg, .chart-html img{ max-width:100%; height:auto; }");
            sb.AppendLine(".meta-grid { width: 100%; }");
            sb.AppendLine(".meta-label { width: 35%; font-weight: bold; }");
            sb.AppendLine(".paragraph { margin: 6pt 0; }");
            sb.AppendLine(".para-box{ border:1px solid #d0d7de; border-radius:8px; padding:12px 14px; margin:10px 0; background:#fff; page-break-inside:avoid; break-inside:avoid; }");
            sb.AppendLine(".para-title{ font-weight:700; font-size:13px; margin:0 0 8px 0; color:#111; }");
            sb.AppendLine(".para-body{ font-size:12px; line-height:1.6; text-align:justify; white-space:normal !important; overflow-wrap:anywhere; color:#212529; width:100%; }");
            sb.AppendLine(".para-body *{ font-size:12px !important; white-space:normal !important; overflow-wrap:anywhere; }");
            sb.AppendLine(".para-body h1,.para-body h2,.para-body h3{ font-size:13px !important; margin:6px 0 !important; }");
            sb.AppendLine(".para-body table{ width:100%; border-collapse:collapse; margin-top:10px; }");
            sb.AppendLine(".para-body table th,.para-body table td{ border:1px solid #222; padding:4px 6px; vertical-align:top; }");
            sb.AppendLine(".para-sep{ border:0; border-top:1px solid #d0d7de; margin:14px 0; }");
            sb.AppendLine(".para-block{ width:100%; border-collapse:collapse; margin:10px 0; page-break-inside:avoid; break-inside:avoid; }");
            sb.AppendLine(".para-block thead th{ background:#f6f8fa; text-align:left; font-size:13px; padding:8px 10px; border:1px solid #d0d7de; }");
            sb.AppendLine(".para-block tbody td{ border:1px solid #d0d7de; padding:12px 14px; }");
            sb.AppendLine(".para-meta th{ background:#f6f8fa; font-weight:700; text-align:left; }");
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
            var logoDataUri = GetLogoDataUri();
            var bankName = string.IsNullOrWhiteSpace(header.BankName) ? "Zarai Taraqiati Bank Limited" : header.BankName;
            var division = string.IsNullOrWhiteSpace(header.InternalAuditDivision) ? "Internal Audit Division" : header.InternalAuditDivision;

            sb.AppendLine("<section class=\"section cover-page\">");
            if (!string.IsNullOrWhiteSpace(logoDataUri))
                {
                sb.AppendLine("<div class=\"cover-logo\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<img src=\"{0}\" alt=\"ZTBL logo\" />", logoDataUri);
                sb.AppendLine("</div>");
                }
            sb.AppendLine("<div class=\"cover-headings\">");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"bank-name\">{0}</div>", Encode(bankName));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"division\">{0}</div>", Encode(division));
            sb.AppendLine("<div class=\"report-title\">Field Audit Report</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"cover-box\">");
            sb.AppendLine("<table class=\"meta-grid\">");
            AppendMetaRow(sb, "Entity", header.EntityName);
            AppendMetaRow(sb, "Branch Code", header.BranchCode);
            AppendMetaRow(sb, "Period", header.AuditPeriod ?? meta.AuditPeriod);
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
            var factsContent = FindSectionContent(data, "EXEC_SUMMARY_FACTS", "Executive Summary – Facts");
            var conclusionContent = FindSectionContent(data, "EXEC_SUMMARY_CONCLUSION", "Executive Summary – Conclusion & Key Messages");
            var summaryContent = FindSectionContent(data, "EXEC_SUMMARY", "Executive Summary")
                ?? FindSectionContent(data, "EXECUTIVE_SUMMARY", "Executive Summary");
            var factsText = NormalizeToParagraphs(factsContent);
            var conclusionText = NormalizeToParagraphs(conclusionContent);
            var summaryText = NormalizeToParagraphs(summaryContent);

            if (string.IsNullOrWhiteSpace(factsText) && string.IsNullOrWhiteSpace(conclusionText)
                && string.IsNullOrWhiteSpace(summaryText))
                {
                return;
                }

            sb.AppendLine("<section class=\"section page-break\">");
            sb.AppendLine("<h2 class=\"section-title\">Executive Summary</h2>");
            sb.AppendLine("<div class=\"exec-summary\">");
            if (string.IsNullOrWhiteSpace(factsText) && string.IsNullOrWhiteSpace(conclusionText))
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<p>{0}</p>", summaryText);
                }
            else if (!string.IsNullOrWhiteSpace(summaryText))
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<p>{0}</p>", summaryText);
                }
            if (!string.IsNullOrWhiteSpace(factsText))
                {
                sb.AppendLine("<p><strong>Executive Summary \u2013 Facts</strong></p>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<p>{0}</p>", factsText);
                }
            if (!string.IsNullOrWhiteSpace(conclusionText))
                {
                sb.AppendLine("<p><strong>Executive Summary \u2013 Conclusion &amp; Key Messages</strong></p>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<p>{0}</p>", conclusionText);
                }
            sb.AppendLine("</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendBranchProfile(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var header = data.Header ?? new FieldAuditPdfHeaderModel();
            var narrative = FindSectionContent(data, "BRANCH_PROFILE", "Branch Profile");

            var hasHeaderData = !string.IsNullOrWhiteSpace(header.BranchName)
                || !string.IsNullOrWhiteSpace(header.BranchCode)
                || !string.IsNullOrWhiteSpace(header.AuditPeriod)
                || header.AuditStartDate.HasValue
                || header.AuditEndDate.HasValue;

            var profileRows = header.BranchProfileRows?.Where(row => !string.IsNullOrWhiteSpace(row.Label)).ToList()
                ?? new List<FieldAuditPdfKeyValueModel>();

            if (profileRows.Count == 0 && !hasHeaderData && !HasMeaningfulContent(narrative))
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

            AppendNarrativeBlock(sb, "Key Statistics Narrative", narrative);
            sb.AppendLine("</section>");
            }

        private static void AppendStaffPosition(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var rows = data.StaffRows ?? new List<FieldAuditPdfStaffRowModel>();
            var displayRows = rows.Where(row => HasMeaningfulContent(row.Designation) || row.Strength.HasValue || row.AsOfDate.HasValue).ToList();
            if (!displayRows.Any())
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Staff Position</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Designation</th><th>Strength</th><th>As-of Date</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in displayRows)
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(row.Designation));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatInteger(row.Strength));
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
            var chartContent = FindSectionContent(data, "KPI_CHART", "KPI Chart") ?? FindSectionContent(data, "FRPT_SECTION_KPI", "KPI Snapshot");
            var chartImages = ExtractChartImages(chartContent);
            if (kpiRows.Count == 0 && chartImages.Count == 0 && !HasMeaningfulContent(chartContent))
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">KPI Snapshot</h2>");
            sb.AppendLine("<div class=\"avoid-break\">");
            AppendChartContent(sb, chartImages, chartContent, "KPI Snapshot Chart");
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
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", EncodeDisplayValue(kpiLabel));
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
            var chartContent = FindSectionContent(data, "NPL_CHART", "NPL Chart") ?? FindSectionContent(data, "FRPT_SECTION_NPL", "NPL Snapshot");
            var chartImages = ExtractChartImages(chartContent);
            if (rows.Count == 0 && chartImages.Count == 0 && !HasMeaningfulContent(chartContent))
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">NPL Analysis</h2>");
            sb.AppendLine("<div class=\"avoid-break\">");
            AppendChartContent(sb, chartImages, chartContent, "NPL Composition Chart");
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
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(para.ParaNo));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(para.Gist));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(para.Nature));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendAuditStatistics(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var rows = data.StatisticsRows ?? new List<FieldAuditPdfStatisticsRowModel>();
            if (!rows.Any(row => HasMeaningfulContent(row.RiskLevel)
                || row.ReportedCount.HasValue
                || row.RectifiedCount.HasValue
                || row.OutstandingCount.HasValue))
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
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(row.RiskLevel));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatInteger(row.ReportedCount));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatInteger(row.RectifiedCount));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatInteger(row.OutstandingCount));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendIncomeLeakage(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var rows = data.IncomeLeakageRows ?? new List<FieldAuditPdfIncomeLeakageRowModel>();
            if (!rows.Any(row => HasMeaningfulContent(row.Description)
                || HasMeaningfulContent(row.CaseReference)
                || row.Amount.HasValue))
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
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(row.CaseReference));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(row.Description));
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
            var hasNarrativeContent = NarrativeSectionOrder.Any(title =>
                HasMeaningfulContent(FindSectionContent(data, null, title)));
            if (!hasNarrativeContent)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Narrative Sections</h2>");
            foreach (var title in NarrativeSectionOrder)
                {
                var content = FindSectionContent(data, null, title);
                if (!HasMeaningfulContent(content))
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
                sb.AppendFormat(CultureInfo.InvariantCulture, "<h3>{0} Risk Paras</h3>", EncodeDisplayValue(group.Key));
                foreach (var para in group)
                    {
                    var paraTitle = string.IsNullOrWhiteSpace(para.Gist)
                        ? $"Para {EncodeDisplayValue(para.ParaNo)}"
                        : $"Para {EncodeDisplayValue(para.ParaNo)}: {EncodeDisplayValue(para.Gist)}";

                    sb.AppendLine("<table class=\"para-block\">");
                    sb.AppendLine("<thead>");
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<tr><th>{0}</th></tr>", paraTitle);
                    sb.AppendLine("</thead>");
                    sb.AppendLine("<tbody>");
                    sb.AppendLine("<tr><td>");
                    sb.AppendLine("<div class=\"para-body\">");
                    AppendParaBodyDetails(sb, para);
                    sb.AppendLine("</div>");
                    sb.AppendLine("</td></tr>");
                    sb.AppendLine("</tbody>");
                    sb.AppendLine("</table>");
                    sb.AppendLine("<hr class=\"para-sep\" />");
                    }
                }

            sb.AppendLine("</section>");
            }

        private static void AppendStaticClauses(StringBuilder sb, FieldAuditPdfReportData data)
            {
            var disclaimer = FindSectionContent(data, "DISCLAIMER", "Disclaimer");
            var restriction = FindSectionContent(data, "RESTRICTION_CLAUSE", "Restriction Clause");
            if (!HasMeaningfulContent(disclaimer) && !HasMeaningfulContent(restriction))
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
            var generatedBy = meta.GeneratedBy;
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Footer</h2>");
            sb.AppendLine("<table class=\"meta-grid\">");
            if (!string.IsNullOrWhiteSpace(generatedBy))
                {
                AppendMetaRow(sb, "Generated By", generatedBy);
                }
            if (meta.GeneratedOn.HasValue)
                {
                AppendMetaRow(sb, "Generated On", FormatDate(meta.GeneratedOn));
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
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendKeyValueRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", EncodeDisplayValue(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendKeyValueParagraph(StringBuilder sb, string label, string value)
            {
            if (string.IsNullOrWhiteSpace(label))
                {
                return;
                }

            sb.AppendFormat(CultureInfo.InvariantCulture, "<p><strong>{0}:</strong> {1}</p>", Encode(label), EncodeDisplayValue(value));
            }

        private static void AppendNarrativeBlock(StringBuilder sb, string title, string htmlContent)
            {
            if (!HasMeaningfulContent(htmlContent))
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

        private static string DisplayValue(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
            }

        private static string EncodeDisplayValue(string value)
            {
            return WebUtility.HtmlEncode(DisplayValue(value));
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

        private static bool HasMeaningfulContent(string htmlContent)
            {
            if (string.IsNullOrWhiteSpace(htmlContent))
                {
                return false;
                }

            var normalized = NormalizeHtml(htmlContent);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return false;
                }

            var textOnly = Regex.Replace(normalized, "<.*?>", string.Empty);
            return !string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(textOnly));
            }

        private static string NormalizeToParagraphs(string htmlContent)
            {
            if (!HasMeaningfulContent(htmlContent))
                {
                return string.Empty;
                }

            var normalized = NormalizeHtml(htmlContent);
            var withNewlines = Regex.Replace(normalized, "<\\s*br\\s*/?>", "\n", RegexOptions.IgnoreCase);
            withNewlines = Regex.Replace(withNewlines, "</p\\s*>", "\n", RegexOptions.IgnoreCase);
            withNewlines = Regex.Replace(withNewlines, "<[^>]+>", string.Empty);
            var lines = withNewlines.Split('\n')
                .Select(line => WebUtility.HtmlDecode(line).Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => WebUtility.HtmlEncode(line));

            return string.Join("</p><p>", lines);
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

        private static void AppendParaTextBlock(StringBuilder sb, string htmlContent)
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

            sb.AppendLine("<div class=\"para-box avoid-break\">");
            sb.AppendLine("<div class=\"para-title\">Para Text:</div>");
            sb.Append("<div class=\"para-body\">");
            sb.Append(normalizedHtml);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<hr class=\"para-sep\" />");
            }

        private static void AppendParaBodyDetails(StringBuilder sb, FieldAuditPdfParaModel para)
            {
            AppendParaMetaTable(sb, para);

            var normalizedHtml = NormalizeHtml(para.ParaDetail);
            normalizedHtml = StripParaTextPrefix(normalizedHtml);
            if (!string.IsNullOrWhiteSpace(normalizedHtml))
                {
                sb.Append(normalizedHtml);
                }

            sb.AppendLine("<table class=\"grid\">");
            sb.AppendLine("<tbody>");
            AppendGridRow(sb, "Implications", para.Implications);
            AppendGridRow(sb, "Recommendations", para.Recommendations);
            AppendGridRow(sb, "Mgmt Comments", para.ManagementComments);
            AppendGridRow(sb, "Auditor Comments", para.AuditorComments);
            AppendGridRow(sb, "SVP Remarks", para.RemarksInCharge);
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            }

        private static void AppendParaMetaTable(StringBuilder sb, FieldAuditPdfParaModel para)
            {
            var metaItems = new[]
                {
                new KeyValuePair<string, string>("Annexure Code", para.AnnexureCode),
                new KeyValuePair<string, string>("Instances", para.Instances),
                new KeyValuePair<string, string>("Amount", para.Amount)
                };

            if (!metaItems.Any(item => !string.IsNullOrWhiteSpace(item.Value)))
                {
                return;
                }

            sb.AppendLine("<table class=\"grid para-meta\">");
            sb.AppendLine("<thead><tr><th class=\"left\">Item</th><th class=\"left\">Details</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in metaItems)
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<th class=\"left\">{0}</th>", Encode(item.Key));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", EncodeDisplayValue(item.Value));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            }

        private static void AppendGridRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th class=\"left\">{0}</th>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"left\">{0}</td>", EncodeDisplayValue(value));
            sb.AppendLine("</tr>");
            }

        private static List<string> ExtractChartImages(string htmlContent)
            {
            var images = new List<string>();
            if (string.IsNullOrWhiteSpace(htmlContent))
                {
                return images;
                }

            var normalized = NormalizeHtml(htmlContent);
            foreach (Match match in Regex.Matches(normalized, "<img[^>]*src=[\"'](?<src>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase))
                {
                var src = match.Groups["src"].Value;
                if (!string.IsNullOrWhiteSpace(src))
                    {
                    images.Add(src);
                    }
                }

            return images;
            }

        private static void AppendChartBlocks(StringBuilder sb, List<string> images, string captionBase)
            {
            if (images == null || images.Count == 0)
                {
                return;
                }

            for (var index = 0; index < images.Count; index++)
                {
                var caption = images.Count > 1 ? $"{captionBase} {index + 1}" : captionBase;
                sb.AppendLine("<div class=\"chart-block\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<img class=\"chart-img\" src=\"{0}\" alt=\"{1}\" />", images[index], Encode(caption));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"chart-caption\">{0}</div>", Encode(caption));
                sb.AppendLine("</div>");
                }
            }

        private static void AppendChartContent(StringBuilder sb, List<string> images, string htmlContent, string captionBase)
            {
            if (images != null && images.Count > 0)
                {
                AppendChartBlocks(sb, images, captionBase);
                return;
                }

            if (!HasMeaningfulContent(htmlContent))
                {
                return;
                }

            var normalizedHtml = NormalizeHtml(htmlContent);
            if (string.IsNullOrWhiteSpace(normalizedHtml))
                {
                return;
                }

            sb.AppendLine("<div class=\"chart-block\">");
            sb.AppendLine("<div class=\"chart-html\">");
            sb.AppendLine(normalizedHtml);
            sb.AppendLine("</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"chart-caption\">{0}</div>", Encode(captionBase));
            sb.AppendLine("</div>");
            }

        private static string GetLogoDataUri()
            {
            try
                {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "ztbllogo.png");
                if (!File.Exists(path))
                    {
                    return string.Empty;
                    }

                var bytes = File.ReadAllBytes(path);
                return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                }
            catch
                {
                return string.Empty;
                }
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
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "-";
            }

        private static string FormatDate(DateTime? date)
            {
            return date.HasValue ? date.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "-";
            }

        private static string FormatDateRange(DateTime? start, DateTime? end)
            {
            if (!start.HasValue && !end.HasValue)
                {
                return "-";
                }

            if (start.HasValue && end.HasValue)
                {
                return $"{FormatDate(start)} - {FormatDate(end)}";
                }

            return start.HasValue ? FormatDate(start) : FormatDate(end);
            }

        private static string FormatNumber(decimal? value)
            {
            return value.HasValue ? value.Value.ToString("N2", CultureInfo.InvariantCulture) : "-";
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

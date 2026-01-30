using AIS.Models.FieldAuditReport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            sb.AppendLine(".grid{ width:100%; border-collapse:collapse; table-layout:fixed; }");
            sb.AppendLine(".grid th,.grid td{ border:1px solid #000; padding:5px; font-size:12px; vertical-align:middle; }");
            sb.AppendLine(".grid th{ font-weight:700; text-align:center; }");
            sb.AppendLine(".grid td{ text-align:center; }");
            sb.AppendLine(".grid td.left{ text-align:left; }");
            sb.AppendLine(".section { page-break-after: auto; }");
            sb.AppendLine(".section-title { border-bottom: 1px solid #000; padding-bottom: 4pt; }");
            sb.AppendLine(".meta-grid { width: 100%; }");
            sb.AppendLine(".meta-label { width: 35%; font-weight: bold; }");
            sb.AppendLine(".paragraph { margin: 6pt 0; }");
            sb.AppendLine(".para-box{ border:1px solid #ced4da; border-radius:4px; padding:12px 14px; margin:10px 0; background:#fff; page-break-inside:avoid; }");
            sb.AppendLine(".para-title{ font-weight:700; text-decoration:underline; font-size:14px; margin-bottom:10px; color:#111; }");
            sb.AppendLine(".para-body{ font-size:13px; line-height:1.6; text-align:justify; white-space:normal !important; overflow-wrap:anywhere; color:#212529; width:100%; }");
            sb.AppendLine(".para-body *{ white-space:normal !important; overflow-wrap:anywhere; }");
            sb.AppendLine(".para-body table{ width:100%; border-collapse:collapse; margin-top:10px; }");
            sb.AppendLine(".para-body table th,.para-body table td{ border:1px solid #222; padding:4px 6px; vertical-align:top; }");
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
            sb.AppendLine("</section>");
            }

        private static void AppendNplAnalysis(StringBuilder sb, FieldAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">NPL Analysis</h2>");
            var rows = data.NplRows ?? new List<FieldAuditPdfNplRowModel>();
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
                    AppendParaTextBlock(sb, para.ParaDetail);
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

            sb.AppendLine("<div class=\"para-box\">");
            sb.AppendLine("<div class=\"para-title\">Para Text:</div>");
            sb.Append("<div class=\"para-body\">");
            sb.Append(normalizedHtml);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
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

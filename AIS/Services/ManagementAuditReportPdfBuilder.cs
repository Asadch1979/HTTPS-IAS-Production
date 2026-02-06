using AIS.Models;
using AIS.Models.FieldAuditReport;
using AIS.Models.ManagementReport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AIS.Services
    {
    public class ManagementAuditReportPdfBuilder
        {
        public string BuildHtml(ManagementAuditPdfReportData data)
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
            sb.AppendLine("@page { size: A4 portrait; margin: 18mm; }");
            sb.AppendLine("body{ font-family: Arial, Helvetica, sans-serif; font-size: 12px; color:#111; margin: 22px; }");
            sb.AppendLine("h1, h2, h3, h4 { margin: 12pt 0 6pt; }");
            sb.AppendLine("h1 { font-size: 18pt; text-align: center; }");
            sb.AppendLine("h2 { font-size: 14pt; }");
            sb.AppendLine("h3 { font-size: 13pt; }");
            sb.AppendLine("h4 { font-size: 12pt; }");
            sb.AppendLine("h2, h3, h4 { page-break-after: avoid; break-after: avoid; }");
            sb.AppendLine(".report-table { width: 100%; border-collapse: collapse; margin: 8pt 0; page-break-inside: auto; break-inside: auto; }");
            sb.AppendLine("tr { page-break-inside: avoid; break-inside: avoid; }");
            sb.AppendLine("thead { display: table-header-group; }");
            sb.AppendLine(".report-table th, .report-table td { border: 1px solid #111; padding: 4pt 6pt; vertical-align: top; }");
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
            sb.AppendLine(".section-title{ font-size:14px; font-weight:700; margin:12px 0 6px 0; padding:6px 10px; border-left:4px solid #111; background:#f6f8fa; page-break-after: avoid; break-after: avoid; }");
            sb.AppendLine(".section-block{ margin-bottom:12px; }");
            sb.AppendLine(".section-block > .section-title + *{ margin-top:0 !important; }");
            sb.AppendLine(".section-body{ line-height:1.7; text-align:justify; }");
            sb.AppendLine(".section-body p{ margin:0 0 10px; }");
            sb.AppendLine(".meta-grid { width: 100%; border-collapse: collapse; }");
            sb.AppendLine(".meta-grid th, .meta-grid td { border: 1px solid #111; padding: 4pt 6pt; vertical-align: top; }");
            sb.AppendLine(".meta-label { width: 35%; font-weight: bold; }");
            sb.AppendLine(".para-box{ border:1px solid #d0d7de; border-radius:8px; padding:12px 14px; margin:10px 0; background:#fff; page-break-inside:avoid; break-inside:avoid; }");
            sb.AppendLine(".para-title{ font-weight:700; font-size:13px; margin:0 0 8px 0; color:#111; }");
            sb.AppendLine(".para-table{ width:100%; border-collapse:collapse; margin:10px 0; table-layout:fixed; }");
            sb.AppendLine(".para-table th{ background:#f6f8fa; text-align:left; border:1px solid #d0d7de; width:20%; }");
            sb.AppendLine(".para-table td{ border:1px solid #d0d7de; padding:10px 12px; overflow:hidden; word-break:break-word; overflow-wrap:anywhere; }");
            sb.AppendLine(".para-body{ font-size:12px; line-height:1.6; text-align:justify; white-space:normal !important; overflow-wrap:anywhere; color:#212529; width:100%; }");
            sb.AppendLine(".para-body *{ font-size:12px !important; white-space:normal !important; overflow-wrap:anywhere; }");
            sb.AppendLine(".page-break{ page-break-before: always; break-before: page; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            AppendCoverHeader(sb, data);
            sb.AppendLine("<div class=\"page-break\"></div>");
            AppendNarrativeInputs(sb, data);
            AppendExecutiveSummary(sb, data);
            AppendStaffDetails(sb, data);
            AppendAuditObservations(sb, data);
            AppendSettledParas(sb, data);

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
            }

        private static void AppendCoverHeader(StringBuilder sb, ManagementAuditPdfReportData data)
            {
            var cover = data.Cover ?? new ManagementAuditCoverModel();
            var logoDataUri = GetLogoDataUri();
            var reportTitle = string.IsNullOrWhiteSpace(cover.AuditedBy)
                ? "Management Audit Report"
                : $"{cover.AuditedBy} Report";

            sb.AppendLine("<section class=\"section cover-page\">");
            if (!string.IsNullOrWhiteSpace(logoDataUri))
                {
                sb.AppendLine("<div class=\"cover-logo\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<img src=\"{0}\" alt=\"ZTBL logo\" />", logoDataUri);
                sb.AppendLine("</div>");
                }
            sb.AppendLine("<div class=\"cover-headings\">");
            sb.AppendLine("<div class=\"bank-name\">Zarai Taraqiati Bank Limited</div>");
            sb.AppendLine("<div class=\"division\">Internal Audit Division</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"report-title\">{0}</div>", Encode(reportTitle));
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"cover-box\">");
            sb.AppendLine("<table class=\"meta-grid\">");
            AppendMetaRow(sb, "Reporting Office", cover.Reporting);
            AppendMetaRow(sb, "Audit Report of:", cover.EntityName);
            AppendMetaRow(sb, "Audited on :", cover.AuditedOn);
            AppendMetaRow(sb, "Audited by :", cover.AuditedBy);
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            AppendTeamDetails(sb, data.AuditTeam);
            sb.AppendLine("<div class=\"cover-confidential\">Confidential \u2013 Internal Use Only</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendTeamDetails(StringBuilder sb, List<GetTeamDetailsModel> auditTeam)
            {
            if (auditTeam == null || auditTeam.Count == 0)
                {
                return;
                }

            var teamLead = auditTeam.FirstOrDefault(member => string.Equals(member.ISTEAMLEAD, "Y", StringComparison.OrdinalIgnoreCase))?.MEMBER_NAME;
            var members = auditTeam
                .Where(member => !string.Equals(member.ISTEAMLEAD, "Y", StringComparison.OrdinalIgnoreCase))
                .Select(member => member.MEMBER_NAME)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            sb.AppendLine("<div class=\"section-block\">");
            sb.AppendLine("<h2 class=\"section-title\">Audit Team</h2>");
            sb.AppendLine("<table class=\"report-table\">");
            sb.AppendLine("<tbody>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th class=\"meta-label\">Team Lead</th>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(teamLead));
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th class=\"meta-label\">Members</th>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", members.Count == 0 ? "-" : Encode(string.Join(", ", members)));
            sb.AppendLine("</tr>");
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            }

        private static void AppendNarrativeInputs(StringBuilder sb, ManagementAuditPdfReportData data)
            {
            sb.AppendLine("<section class=\"section\">");
            AppendNarrativeSection(sb, data, "OBJECTIVE", "Objective");
            AppendNarrativeSection(sb, data, "SCOPE", "Scope");
            AppendNarrativeSection(sb, data, "METHODOLOGY", "Methodology");
            AppendNarrativeSection(sb, data, "DISCLAIMER", "Disclaimer");
            AppendNarrativeSection(sb, data, "INTRODUCTION", "Introduction");
            sb.AppendLine("</section>");
            }

        private static void AppendExecutiveSummary(StringBuilder sb, ManagementAuditPdfReportData data)
            {
            var summary = FindSectionContent(data, "EXEC_SUMMARY", "EXECUTIVE SUMMARY")
                ?? FindSectionContent(data, "EXECUTIVE_SUMMARY", "Executive Summary");

            if (!HasMeaningfulContent(summary))
                {
                return;
                }

            sb.AppendLine("<section class=\"section page-break\">");
            sb.AppendLine("<h2 class=\"section-title\">Executive Summary</h2>");
            AppendSectionBody(sb, summary);
            sb.AppendLine("</section>");
            }

        private static void AppendStaffDetails(StringBuilder sb, ManagementAuditPdfReportData data)
            {
            var rows = data.StaffRows ?? new List<FieldAuditPdfStaffRowModel>();
            if (rows.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Staff Details</h2>");
            sb.AppendLine("<table class=\"report-table\">");
            sb.AppendLine("<thead><tr><th>PP No</th><th>Name</th><th>Rank</th><th>Designation</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var row in rows)
                {
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(row.PpNo?.ToString()));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(row.Name));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(row.Rank));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(row.Designation));
                sb.AppendLine("</tr>");
                }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</section>");
            }

        private static void AppendAuditObservations(StringBuilder sb, ManagementAuditPdfReportData data)
            {
            var paras = data.Observations ?? new List<FieldAuditPdfParaModel>();
            if (paras.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Audit Observations</h2>");
            foreach (var para in paras)
                {
                var status = string.IsNullOrWhiteSpace(para.Status) ? "Un-Settled" : para.Status;
                sb.AppendLine("<div class=\"para-box\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"para-title\">Para No: {0}</div>", FormatCell(para.ParaNo));
                sb.AppendLine("<table class=\"para-table\">");
                AppendParaRow(sb, "Title", para.Gist);
                AppendParaRowHtml(sb, "Para Text", para.ParaDetail);
                AppendParaRow(sb, "Risk Category", para.Risk);
                AppendParaRowHtml(sb, "Recommendation", para.Recommendations);
                AppendParaRowHtml(sb, "Management Reply", para.ManagementComments);
                AppendParaRowHtml(sb, "Audit Reply", para.AuditorComments);
                AppendParaRow(sb, "Status", status);
                sb.AppendLine("</table>");
                sb.AppendLine("</div>");
                }
            sb.AppendLine("</section>");
            }

        private static void AppendSettledParas(StringBuilder sb, ManagementAuditPdfReportData data)
            {
            var settled = data.SettledParas ?? new List<FieldAuditPdfParaModel>();
            if (settled.Count == 0)
                {
                return;
                }

            sb.AppendLine("<section class=\"section\">");
            sb.AppendLine("<h2 class=\"section-title\">Paras Settled During the Course of Audit</h2>");
            foreach (var para in settled)
                {
                sb.AppendLine("<div class=\"para-box\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"para-title\">{0}</div>", FormatCell(para.Gist));
                sb.AppendLine("<table class=\"para-table\">");
                AppendParaRowHtml(sb, "Para Text", para.ParaDetail);
                AppendParaRowHtml(sb, "Management Reply", para.ManagementComments);
                sb.AppendLine("</table>");
                sb.AppendLine("</div>");
                }
            sb.AppendLine("</section>");
            }

        private static void AppendNarrativeSection(StringBuilder sb, ManagementAuditPdfReportData data, string code, string title)
            {
            var content = FindSectionContent(data, code, title);
            if (!HasMeaningfulContent(content))
                {
                return;
                }

            sb.AppendLine("<div class=\"section-block\">");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<h2 class=\"section-title\">{0}</h2>", Encode(title));
            AppendSectionBody(sb, content);
            sb.AppendLine("</div>");
            }

        private static void AppendSectionBody(StringBuilder sb, string htmlContent)
            {
            var normalized = NormalizeHtml(htmlContent);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return;
                }

            sb.AppendLine("<div class=\"section-body\">");
            sb.Append(normalized);
            sb.AppendLine("</div>");
            }

        private static void AppendMetaRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th class=\"meta-label\">{0}</th>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendParaRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendParaRowHtml(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", Encode(label));
            var normalized = NormalizeHtml(value);
            var content = string.IsNullOrWhiteSpace(normalized) ? "-" : normalized;
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td><div class=\"para-body\">{0}</div></td>", content);
            sb.AppendLine("</tr>");
            }

        private static string FindSectionContent(ManagementAuditPdfReportData data, string code, string title)
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

        private static string FormatCell(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? "-" : Encode(value);
            }

        private static string NormalizeHtml(string html)
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

            current = Regex.Replace(current, "\\swidth\\s*=\\s*[\"']?\\s*\\d+%?\\s*[\"']?", string.Empty, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "\\snowrap(\\s*=\\s*[\"']?nowrap[\"']?)?", string.Empty, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "style\\s*=\\s*[\"'][^\"']*[\"']", match =>
                {
                var style = match.Value;
                style = Regex.Replace(style, "width\\s*:\\s*[^;]+;?", string.Empty, RegexOptions.IgnoreCase);
                style = Regex.Replace(style, "white-space\\s*:\\s*nowrap;?", string.Empty, RegexOptions.IgnoreCase);
                style = Regex.Replace(style, "\\s{2,}", " ");
                return style;
                }, RegexOptions.IgnoreCase);

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
        }
    }

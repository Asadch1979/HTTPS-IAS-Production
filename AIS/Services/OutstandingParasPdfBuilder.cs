using AIS.Models;
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
    public class OutstandingParasPdfBuilder
        {
        public string BuildHtml(OutstandingParasPdfReportData data)
            {
            if (data == null)
                {
                throw new ArgumentNullException(nameof(data));
                }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: A4 portrait; margin: 18mm; }");
            sb.AppendLine("body{ font-family: Arial, Helvetica, sans-serif; font-size:12px; color:#111; margin:22px; }");
            sb.AppendLine("h1{ font-size:20pt; text-align:center; margin:14pt 0 8pt; }");
            sb.AppendLine("h2{ font-size:15pt; margin:12pt 0 8pt; page-break-after:avoid; break-after:avoid; }");
            sb.AppendLine("h3{ font-size:13pt; margin:10pt 0 6pt; page-break-after:avoid; break-after:avoid; }");
            sb.AppendLine(".cover-page{ text-align:center; min-height:720px; }");
            sb.AppendLine(".cover-logo{ text-align:left; margin-bottom:22px; }");
            sb.AppendLine(".cover-logo img{ height:70px; }");
            sb.AppendLine(".bank-name{ font-size:18pt; font-weight:700; margin-top:8px; }");
            sb.AppendLine(".division{ font-size:13pt; margin-top:6px; }");
            sb.AppendLine(".report-title{ font-size:18pt; font-weight:700; margin:36px 0 24px; }");
            sb.AppendLine(".cover-box{ border:1px solid #111; padding:16px 18px; margin:18px auto 0; width:82%; text-align:left; }");
            sb.AppendLine(".confidential{ margin-top:24px; font-weight:700; }");
            sb.AppendLine(".page-break{ page-break-before:always; break-before:page; }");
            sb.AppendLine(".break-after{ page-break-after:always; break-after:page; }");
            sb.AppendLine(".entity-divider{ border:0; border-top:2px solid #111; margin:0 0 18px; }");
            sb.AppendLine(".meta-grid{ width:100%; border-collapse:collapse; margin:8pt 0; }");
            sb.AppendLine(".meta-grid th,.meta-grid td{ border:1px solid #111; padding:6px 8px; vertical-align:top; text-align:left; }");
            sb.AppendLine(".meta-grid th{ width:34%; background:#f6f8fa; font-weight:700; }");
            sb.AppendLine(".section-title{ font-size:14px; font-weight:700; margin:12px 0 8px; padding:7px 10px; border-left:4px solid #111; background:#f6f8fa; }");
            sb.AppendLine(".para-card{ border:1px solid #d0d7de; border-radius:6px; padding:12px 14px; margin:12px 0; page-break-inside:avoid; break-inside:avoid; }");
            sb.AppendLine(".para-title{ font-size:13px; font-weight:700; margin-bottom:8px; }");
            sb.AppendLine(".para-meta{ width:100%; border-collapse:collapse; margin:8px 0; table-layout:fixed; }");
            sb.AppendLine(".para-meta th,.para-meta td{ border:1px solid #d0d7de; padding:6px 8px; vertical-align:top; word-break:break-word; overflow-wrap:anywhere; }");
            sb.AppendLine(".para-meta th{ width:28%; background:#f6f8fa; text-align:left; }");
            sb.AppendLine(".rich-html,.rich-html *{ font-size:12px !important; white-space:normal !important; overflow-wrap:anywhere; max-width:100% !important; box-sizing:border-box !important; }");
            sb.AppendLine(".rich-html table{ width:100% !important; max-width:100% !important; table-layout:auto !important; border-collapse:collapse; }");
            sb.AppendLine(".rich-html th,.rich-html td{ border:1px solid #d0d7de; padding:4px 6px; vertical-align:top; word-break:break-word; }");
            sb.AppendLine(".empty{ color:#555; font-style:italic; }");
            sb.AppendLine("</style></head><body>");

            var entities = data.Entities ?? new List<OutstandingParaEntityPdfModel>();
            var paras = data.Paras ?? new List<OutstandingParaPdfModel>();
            if (entities.Count == 0)
                {
                AppendMainCover(sb, data, null);
                sb.AppendLine("<section class=\"page-break\"><h2>No outstanding audit paras found.</h2></section>");
                }

            for (var i = 0; i < entities.Count; i++)
                {
                var entity = entities[i];
                var entityParas = GetParasForEntity(paras, entity).ToList();
                if (i > 0)
                    {
                    sb.AppendLine("<section class=\"page-break\">");
                    sb.AppendLine("<hr class=\"entity-divider\" />");
                    AppendMainCover(sb, data, entity);
                    sb.AppendLine("</section>");
                    }
                else
                    {
                    AppendMainCover(sb, data, entity);
                    }

                sb.AppendLine("<section class=\"page-break\">");
                AppendParas(sb, entity, entityParas);
                sb.AppendLine("</section>");
                }

            sb.AppendLine("</body></html>");
            return sb.ToString();
            }

        public string BuildSummaryHtml(OutstandingParasSummaryPdfReportData data)
            {
            if (data == null)
                {
                throw new ArgumentNullException(nameof(data));
                }

            var rows = data.Paras ?? new List<OutstandingParasSummaryPdfModel>();
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: A4 landscape; margin: 10mm 8mm 14mm; }");
            sb.AppendLine("body{ font-family: Arial, Helvetica, sans-serif; font-size:8px; color:#111; margin:0; }");
            sb.AppendLine("h1{ font-size:15pt; text-align:center; margin:0 0 8pt; }");
            sb.AppendLine(".filters{ font-size:8px; margin:0 0 7pt; }");
            sb.AppendLine("table{ width:100%; border-collapse:collapse; table-layout:fixed; }");
            sb.AppendLine("thead{ display:table-header-group; }");
            sb.AppendLine("th,td{ border:1px solid #333; padding:3px 4px; vertical-align:top; white-space:normal; overflow-wrap:anywhere; word-break:break-word; }");
            sb.AppendLine("th{ background:#eeeeee; font-weight:700; text-align:center; }");
            sb.AppendLine(".entity{ width:10%; }");
            sb.AppendLine(".period{ width:8%; }");
            sb.AppendLine(".para-no{ width:2%; text-align:center; }");
            sb.AppendLine(".gist{ width:10%; }");
            sb.AppendLine(".risk{ width:6%; text-align:center; }");
            sb.AppendLine(".text{ width:74%; }");
            sb.AppendLine(".summary-rich,.summary-rich *{ font-size:8px !important; white-space:normal !important; overflow-wrap:anywhere; max-width:100% !important; box-sizing:border-box !important; }");
            sb.AppendLine(".summary-rich table{ width:100% !important; max-width:100% !important; table-layout:auto !important; border-collapse:collapse; }");
            sb.AppendLine(".summary-rich th,.summary-rich td{ border:1px solid #999; padding:2px 3px; vertical-align:top; word-break:break-word; }");
            sb.AppendLine(".summary-rich p{ margin:0 0 3px; }");
            sb.AppendLine(".summary-rich ul,.summary-rich ol{ margin:0 0 3px 12px; padding:0; }");
            sb.AppendLine(".empty{ text-align:center; color:#555; font-style:italic; }");
            sb.AppendLine("</style></head><body>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<h1>{0}</h1>", Encode(data.ReportTitle));
            sb.AppendLine();
            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                "<div class=\"filters\"><strong>Audit Department:</strong> {0} &nbsp; <strong>Risk:</strong> {1}</div>",
                FormatPlainCell(string.IsNullOrWhiteSpace(data.AuditDepartmentName) ? "All" : data.AuditDepartmentName),
                FormatPlainCell(string.IsNullOrWhiteSpace(data.Risk) ? "All" : data.Risk));
            sb.AppendLine();
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th class=\"entity\">Entity Name</th>");
            sb.AppendLine("<th class=\"period\">Audit Period</th>");
            sb.AppendLine("<th class=\"para-no\">Para No.</th>");
            sb.AppendLine("<th class=\"gist\">Gist / Heading</th>");
            sb.AppendLine("<th class=\"risk\">Risk</th>");
            sb.AppendLine("<th class=\"text\">Para Text</th>");
            sb.AppendLine("</tr></thead><tbody>");

            if (rows.Count == 0)
                {
                sb.AppendLine("<tr><td colspan=\"6\" class=\"empty\">No outstanding audit paras found for the selected filters.</td></tr>");
                }

            for (var i = 0; i < rows.Count; i++)
                {
                var row = rows[i];
                sb.AppendLine("<tr>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"entity\">{0}</td>", FormatPlainCell(row.EntityName));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"period\">{0}</td>", FormatPlainCell(row.AuditPeriod));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"para-no\">{0}</td>", FormatPlainCell(row.ParaNo));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"gist\">{0}</td>", FormatPlainCell(row.GistHeading));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<td class=\"risk\">{0}</td>", FormatPlainCell(row.Risk));
                sb.Append("<td class=\"text\"><div class=\"summary-rich\">");
                sb.Append(NormalizeHtml(row.ParaText));
                sb.Append("</div></td>");
                sb.AppendLine("</tr>");
                }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
            }

        private static void AppendMainCover(StringBuilder sb, OutstandingParasPdfReportData data, OutstandingParaEntityPdfModel entity)
            {
            var logoDataUri = GetLogoDataUri();
            sb.AppendLine("<section class=\"cover-page\">");
            if (!string.IsNullOrWhiteSpace(logoDataUri))
                {
                sb.AppendLine("<div class=\"cover-logo\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<img src=\"{0}\" alt=\"ZTBL logo\" />", logoDataUri);
                sb.AppendLine("</div>");
                }

            sb.AppendLine("<div class=\"bank-name\">Zarai Taraqiati Bank Limited</div>");
            sb.AppendLine("<div class=\"division\">Internal Audit Division</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"report-title\">{0}</div>", Encode(string.IsNullOrWhiteSpace(data.ReportTitle) ? "Outstanding Audit Paras Report" : data.ReportTitle));
            sb.AppendLine("<div class=\"cover-box\"><table class=\"meta-grid\"><tbody>");
            AppendCoverDetails(sb, data, entity);
            AppendMetaRow(sb, "Report Scope", "Outstanding audit paras only");
            AppendMetaRow(sb, "Status As Of / Generated On", FormatDateTime(data.GeneratedOn));
            AppendMetaRow(sb, "Classification", "CONFIDENTIAL");
            sb.AppendLine("</tbody></table></div>");
            sb.AppendLine("<div class=\"confidential\">Confidential - Internal Use Only</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendCoverDetails(StringBuilder sb, OutstandingParasPdfReportData data, OutstandingParaEntityPdfModel entity)
            {
            if (entity == null)
                {
                AppendMetaRow(sb, "Entity Name", "-");
                AppendMetaRow(sb, "Entity Code", "-");
                AppendMetaRow(sb, "Audit Department", data.AuditDepartmentName);
                AppendMetaRow(sb, "Audit Period", "-");
                AppendMetaRow(sb, "Execution Start Date", FormatDate(data.ExecutionStartDate));
                AppendMetaRow(sb, "Execution End Date", FormatDate(data.ExecutionEndDate));
                AppendMetaRow(sb, "Team Lead", "-");
                AppendMetaRow(sb, "Team Members", "-");
                return;
                }

            AppendMetaRow(sb, "Entity Name", entity.EntityName);
            AppendMetaRow(sb, "Entity Code", entity.EntityCode);
            AppendMetaRow(sb, "Audit Department", string.IsNullOrWhiteSpace(entity.AuditDepartment) ? data.AuditDepartmentName : entity.AuditDepartment);
            AppendMetaRow(sb, "Audit Period", entity.AuditPeriod);
            AppendMetaRow(sb, "Execution Start Date", FormatDate(entity.ExecutionStartDate ?? data.ExecutionStartDate));
            AppendMetaRow(sb, "Execution End Date", FormatDate(entity.ExecutionEndDate ?? data.ExecutionEndDate));
            AppendMetaRow(sb, "Team Lead", entity.TeamLead);
            AppendMetaRow(sb, "Team Members", entity.TeamMembers);
            }

        private static void AppendParas(StringBuilder sb, OutstandingParaEntityPdfModel entity, List<OutstandingParaPdfModel> paras)
            {
            sb.AppendLine("<div class=\"section-title\">Para Details</div>");

            if (paras == null || paras.Count == 0)
                {
                sb.AppendLine("<p class=\"empty\">No outstanding paras are available for this entity / engagement.</p>");
                return;
                }

            foreach (var para in paras)
                {
                sb.AppendLine("<div class=\"para-card\">");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"para-title\">Para {0}: {1}</div>", Encode(para.ParaNo), Encode(para.ParaTitle));
                sb.AppendLine("<table class=\"para-meta\"><tbody>");
                AppendMetaRow(sb, "Para No", para.ParaNo);
                AppendMetaRow(sb, "Para Title", para.ParaTitle);
                AppendMetaRow(sb, "Risk Category", para.RiskCategory);
                AppendMetaRow(sb, "Current Compliance Status", para.CurrentComplianceStatus);
                AppendRichRow(sb, "Observation Text", para.ObservationText);
                AppendRichRow(sb, "Latest Management Response", para.LatestManagementResponse);
                AppendRichRow(sb, "Audit Remarks", para.AuditRemarks);
                sb.AppendLine("</tbody></table>");
                sb.AppendLine("</div>");
                }
            }

        private static IEnumerable<OutstandingParaPdfModel> GetParasForEntity(IEnumerable<OutstandingParaPdfModel> paras, OutstandingParaEntityPdfModel entity)
            {
            return paras.Where(para =>
                (entity.EngagementId > 0 && para.EngagementId == entity.EngagementId)
                || (entity.EngagementId <= 0 && entity.EntityId > 0 && para.EntityId == entity.EntityId)
                || (entity.EngagementId <= 0 && entity.EntityId <= 0 && string.Equals(para.EntityName, entity.EntityName, StringComparison.OrdinalIgnoreCase)));
            }

        private static void AppendMetaRow(StringBuilder sb, string label, string value)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", Encode(label));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", FormatCell(value));
            sb.AppendLine("</tr>");
            }

        private static void AppendRichRow(StringBuilder sb, string label, string html)
            {
            sb.AppendLine("<tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", Encode(label));
            sb.Append("<td><div class=\"rich-html\">");
            sb.Append(NormalizeHtml(html));
            sb.AppendLine("</div></td></tr>");
            }

        private static string FormatCell(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? "-" : Encode(value);
            }

        private static string FormatPlainCell(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? "-" : Encode(ToPlainText(value));
            }

        private static string Encode(string value)
            {
            return WebUtility.HtmlEncode(value ?? string.Empty);
            }

        private static string NormalizeHtml(string html)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                return "-";
                }

            var current = html.Trim();
            current = Regex.Replace(current, "<script[\\s\\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "\\son\\w+\\s*=\\s*(['\"]).*?\\1", string.Empty, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "\\sstyle\\s*=\\s*(['\"])(.*?)\\1", string.Empty, RegexOptions.IgnoreCase);

            if (!Regex.IsMatch(current, "<[a-z][\\s\\S]*>", RegexOptions.IgnoreCase))
                {
                current = Encode(current).Replace(Environment.NewLine, "<br />");
                }

            return string.IsNullOrWhiteSpace(current) ? "-" : current;
            }

        private static string ToPlainText(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return string.Empty;
                }

            var current = Regex.Replace(value, "<script[\\s\\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "<style[\\s\\S]*?</style>", string.Empty, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "<br\\s*/?>", Environment.NewLine, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "</p\\s*>", Environment.NewLine, RegexOptions.IgnoreCase);
            current = Regex.Replace(current, "<[^>]+>", " ");
            current = WebUtility.HtmlDecode(current);
            current = Regex.Replace(current, "[ \\t\\r\\f\\v]+", " ");
            current = Regex.Replace(current, "\\n\\s+", Environment.NewLine);
            return current.Trim();
            }

        private static string FormatDate(DateTime? date)
            {
            return date.HasValue ? date.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "-";
            }

        private static string FormatDateRange(DateTime start, DateTime end)
            {
            return $"{FormatDate(start)} to {FormatDate(end)}";
            }

        private static string FormatDateTime(DateTime value)
            {
            return value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
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

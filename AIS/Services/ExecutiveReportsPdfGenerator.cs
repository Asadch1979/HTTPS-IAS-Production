using AIS.Models;
using iText.Html2pdf;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Font.Constants;
using iText.IO.Image;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace AIS.Services
{
    // Render a searchable, server-produced A4 landscape PDF. This avoids browser
    // URL/date print headers, image-only capture, and unwanted trailing pages.
    // The authenticated report controller provides an already-scoped result.
    public static class ExecutiveReportsPdfGenerator
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        private static string H(object v) => WebUtility.HtmlEncode(Convert.ToString(v, Culture) ?? "");
        private static string N(long v) => v.ToString("N0", Culture);
        private static string P(decimal? v) => v.HasValue ? v.Value.ToString("0.00", Culture) + "%" : "N/A";
        private static string Title(string id) => id switch {
            "A" => "Annual Field Audit Paras Progress",
            "B" => "Field Audit Position - Field Office",
            "C" => "Field Audit Position - GM / Region",
            "D" => "Top Five and Bottom Five Regions",
            "E" => "Division-wise Management Audit Settlement",
            _ => "Executive Report"
        };
        private static string Family(string value) => value == "GM Field Offices" ? "Field Audit Division"
            : value == "Other Field Offices" ? "Management Audit Division" : value ?? "Unassigned";
        private static string V(ExecutiveReportRow r, string field) => field switch {
            "opening" => N(r.OpeningBalance), "added" => N(r.ParasAdded),
            "total" => N(r.TotalParas), "settled" => N(r.SettledParas),
            "rate" => P(r.SettlementPercentage), "open" => N(r.OutstandingParas),
            "low" => N(r.LowRisk), "medium" => N(r.MediumRisk),
            "high" => N(r.HighRisk), "compliance" => N(r.SettledByCompliance),
            "audit" => N(r.SettledByAudit), "unknown" => N(r.SettledByUnknown),
            _ => ""
        };
        private static readonly (string Label, string Key)[] Position = {
            ("Opening","opening"), ("Added","added"), ("Total","total"),
            ("Settled","settled"), ("Settlement %","rate"), ("Outstanding","open"),
            ("Low","low"), ("Medium","medium"), ("High","high"),
            ("Compliance","compliance"), ("Audit","audit"), ("Unknown","unknown")
        };

        public static byte[] Create(string reportId, ExecutiveReportResult report, string webRootPath)
        {
            if (report == null || report.Rows == null)
                throw new ArgumentException("Validated report rows are required.", nameof(report));
            if (reportId == null || !"ABCDE".Contains(reportId) || reportId.Length != 1)
                throw new ArgumentException("Unknown executive report.", nameof(reportId));

            var html = BuildHtml(reportId, report);
            using var baseStream = new MemoryStream();
            // iText pdfHTML writes real searchable text and paginates tables;
            // nothing is rasterized or taken from a browser screenshot.
            HtmlConverter.ConvertToPdf(html, baseStream);
            var basePdf = baseStream.ToArray();
            using var input = new MemoryStream(basePdf);
            using var stamped = new MemoryStream();
            using (var pdf = new PdfDocument(new PdfReader(input), new PdfWriter(stamped)))
            {
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var logo = Path.Combine(webRootPath ?? "", "Images", "ztbllogo.png");
                ImageData data = File.Exists(logo) ? ImageDataFactory.Create(logo) : null;
                var total = pdf.GetNumberOfPages();
                for (int i = 1; i <= total; i++)
                {
                    var page = pdf.GetPage(i);
                    var size = page.GetPageSize();
                    var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
                    if (data != null)
                        canvas.AddImageFittedIntoRectangle(data,
                            new Rectangle(26, size.GetHeight() - 38, 22, 22), false);
                    Put(canvas, bold, 10, 54, size.GetHeight() - 29,
                        "ZTBL  |  INTERNAL AUDIT SYSTEM", new DeviceRgb(15, 92, 77));
                    Put(canvas, font, 8, size.GetWidth() - 205, size.GetHeight() - 29,
                        "Report " + reportId + "  |  " + Title(reportId), new DeviceRgb(100, 116, 139));
                    canvas.SetStrokeColor(new DeviceRgb(226, 232, 240))
                        .MoveTo(26, size.GetHeight() - 44).LineTo(size.GetWidth() - 26, size.GetHeight() - 44).Stroke();
                    canvas.MoveTo(26, 34).LineTo(size.GetWidth() - 26, 34).Stroke();
                    Put(canvas, font, 8, 26, 21, "Generated on " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm", Culture), new DeviceRgb(100, 116, 139));
                    Put(canvas, font, 8, size.GetWidth() - 92, 21,
                        "Page " + i + " of " + total, new DeviceRgb(100, 116, 139));
                }
            }
            return stamped.ToArray();
        }
        private static void Put(PdfCanvas c, PdfFont f, float size, float x, float y,
            string value, Color color)
        {
            c.BeginText().SetFontAndSize(f, size).SetFillColor(color)
                .MoveText(x, y).ShowText(value).EndText();
        }

        private static string BuildHtml(string id, ExecutiveReportResult data)
        {
            var rows = data.Rows;
            var html = new StringBuilder();
            html.Append(@"<!doctype html><html><head><meta charset='utf-8'><style>
              @page { size: A4 landscape; margin: 57pt 26pt 45pt; }
              body { font-family: Arial, sans-serif; color: #1f2933; font-size: 8pt; }
              h1 { margin: 0 0 4pt; font-size: 16pt; color: #0f5c4d; }
              .meta { color:#64748b; margin:0 0 12pt; font-size:9pt; }
              h2 { color:#0f5c4d; font-size:11pt; margin:13pt 0 6pt; }
              table { width:100%; border-collapse:collapse; margin:5pt 0 11pt; table-layout:fixed; }
              thead { display:table-header-group; }
              th { background:#e9f3f0; color:#1f2933; font-weight:bold; font-size:7.8pt; padding:5pt 3pt; border-bottom:1pt solid #cbd5e1; }
              td { padding:4pt 3pt; border-bottom:.4pt solid #e2e8f0; font-size:7.8pt; }
              tr { page-break-inside:avoid; break-inside:avoid; }
              .left { text-align:left; width:22%; overflow-wrap:break-word; }
              .num { text-align:right; white-space:nowrap; }
              .group td { background:#eaf3f1; font-weight:bold; }
              .subtotal td { background:#f1f5f9; font-weight:bold; border-top:1pt solid #94a3b8; }
              .total td { background:#dcebe5; font-weight:bold; border-top:1.2pt solid #0f5c4d; }
              tbody tr:nth-child(even) { background:#f8fafc; }
              .low { color:#8a5d08; } .medium { color:#a15e0a; } .high { color:#a52b2b; font-weight:bold; }
              .note { color:#64748b; font-size:8pt; margin-top:10pt; }
              .bar { height:9pt; display:inline-block; background:#2e7d32; }
              .bar-bottom { background:#c62828; }
              .benchmark { border-left:1pt dashed #e39b1b; }
              </style></head><body>");
            html.Append("<h1>Report ").Append(H(id)).Append(" · ").Append(H(Title(id))).Append("</h1>");
            html.Append("<p class='meta'>").Append(data.Filters == null ? "" :
                H(data.Filters.StartDate) + " to " + H(data.Filters.EndDate))
                .Append(" &nbsp;|&nbsp; ").Append(H(data.DashboardType))
                .Append(data.GroupRootId.HasValue ? " / Group " + data.GroupRootId.Value : "")
                .Append("</p>");
            if (id == "A")
            {
                html.Append("<h2>Annual audit position</h2>");
                AddTable(html, "Year", rows, r => r.PeriodStart.ToString("yyyy", Culture) +
                    (r.IsPartialYear ? " (partial)" : ""),
                    new[]{("Settled","settled"),("Settlement %","rate"),("Added","added"),("Outstanding","open")});
            }
            else if (id == "B")
            {
                html.Append("<h2>Field office paras position</h2>");
                AddPosition(html, rows, r => r.ItemName, true);
            }
            else if (id == "C")
            {
                html.Append("<h2>GM paras position</h2>");
                var gm = rows.Where(r => r.RowKind == "SUBTOTAL").ToList();
                var total = rows.FirstOrDefault(r => r.RowKind == "TOTAL");
                if (total != null) gm.Add(total);
                AddPosition(html, gm, r => r.RowKind == "TOTAL" ? "Grand total" : r.GroupName, false);
                html.Append("<h2>GM / region paras position</h2>");
                AddPosition(html, rows, r => r.ItemName, true);
            }
            else if (id == "D")
            {
                foreach (var side in new[]{"TOP","BOTTOM"})
                {
                    var ranked = rows.Where(r => r.RankingGroup == side && r.RankPosition.HasValue)
                        .OrderBy(r => r.RankPosition).ToList();
                    html.Append("<h2>").Append(side == "TOP" ? "Top five regions" : "Bottom five regions").Append("</h2>");
                    html.Append("<table><thead><tr><th class='left'>Region</th><th>Rank</th><th>Total paras</th><th>Settlement %</th><th>Outstanding</th></tr></thead><tbody>");
                    foreach (var r in ranked)
                        html.Append("<tr><td>").Append(H(r.ItemName)).Append("</td><td class='num'>").Append(r.RankPosition)
                            .Append("</td><td class='num'>").Append(N(r.TotalParas))
                            .Append("</td><td class='num'>").Append(P(r.SettlementPercentage))
                            .Append("</td><td class='num'>").Append(N(r.OutstandingParas)).Append("</td></tr>");
                    html.Append("</tbody></table>");
                }
            }
            else
            {
                html.Append("<h2>Division-wise Management Audit Settlement</h2>");
                AddTable(html, "Function / Division", rows, r => r.ItemName,
                    new[]{("Opening","opening"),("Added","added"),("Total","total"),
                          ("Settled","settled"),("Outstanding","open"),("Settlement %","rate")});
            }
            html.Append("<p class='note'>Data note: historical positions use available IAS dates and current organizational mapping; settlement attribution is indicative.</p>");
            html.Append("</body></html>");
            return html.ToString();
        }

        private static void AddPosition(StringBuilder html, IList<ExecutiveReportRow> rows,
            Func<ExecutiveReportRow,string> label, bool group)
        {
            html.Append("<table><thead><tr><th class='left'>Office / Region</th>");
            foreach (var col in Position)
                html.Append("<th>").Append(H(col.Label)).Append("</th>");
            html.Append("</tr></thead><tbody>");
            string last = null;
            foreach (var r in rows)
            {
                var kind = r.RowKind?.ToUpperInvariant();
                if (group && kind != "TOTAL" && r.GroupName != last)
                {
                    html.Append("<tr class='group'><td colspan='13'>")
                        .Append(H(r.GroupName == "GM Field Offices" || r.GroupName == "Other Field Offices"
                            ? Family(r.GroupName) : r.GroupName))
                        .Append("</td></tr>");
                    last = r.GroupName;
                }
                html.Append("<tr class='").Append(kind == "TOTAL" ? "total" : kind == "SUBTOTAL" ? "subtotal" : "")
                    .Append("'><td>").Append(H(kind == "TOTAL" ? "Grand total"
                        : kind == "SUBTOTAL" ? "Subtotal · " + Family(r.GroupName)
                        : label(r))).Append("</td>");
                foreach (var col in Position)
                    html.Append("<td class='num'>").Append(H(V(r, col.Key))).Append("</td>");
                html.Append("</tr>");
            }
            html.Append("</tbody></table>");
        }
        private static void AddTable(StringBuilder html, string identity, IList<ExecutiveReportRow> rows,
            Func<ExecutiveReportRow,string> label, (string Label,string Key)[] columns)
        {
            html.Append("<table><thead><tr><th class='left'>").Append(H(identity)).Append("</th>");
            foreach (var col in columns) html.Append("<th>").Append(H(col.Label)).Append("</th>");
            html.Append("</tr></thead><tbody>");
            foreach (var r in rows)
            {
                var kind = r.RowKind?.ToUpperInvariant();
                html.Append("<tr class='").Append(kind == "TOTAL" ? "total" : kind == "SUBTOTAL" ? "subtotal" : "")
                    .Append("'><td>").Append(H(kind == "TOTAL" ? "Grand total" : label(r))).Append("</td>");
                foreach (var col in columns)
                    html.Append("<td class='num'>").Append(H(V(r, col.Key))).Append("</td>");
                html.Append("</tr>");
            }
            html.Append("</tbody></table>");
        }
    }
}

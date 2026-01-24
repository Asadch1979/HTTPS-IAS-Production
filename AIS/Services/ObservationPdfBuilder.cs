using AIS.Models;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace AIS.Services
    {
    public class ObservationPdfBuilder
        {
        public string BuildHtml(ObservationPdfDataModel data)
            {
            var memoDate = data?.MemoDate?.ToString("dd-MMM-yyyy") ?? string.Empty;
            var memoNumber = Encode(data?.MemoNumber);
            var annexure = Encode(data?.Annexure);
            var title = Encode(data?.Title);
            var risk = Encode(data?.Risk);
            var paraText = data?.ParaText ?? string.Empty;

            var hasResponsibilities = data?.Responsibilities != null
                && data.Responsibilities.Any(item =>
                    !string.IsNullOrWhiteSpace(item.PpNo)
                    || !string.IsNullOrWhiteSpace(item.LoanCase)
                    || !string.IsNullOrWhiteSpace(item.LcAmount));

            var builder = new StringBuilder();
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<style>");
            builder.AppendLine("body { font-family: Arial, Helvetica, sans-serif; font-size: 12px; color: #111; }");
            builder.AppendLine(".header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }");
            builder.AppendLine(".title { font-size: 18px; font-weight: bold; }");
            builder.AppendLine(".memo-date { font-size: 12px; text-align: right; }");
            builder.AppendLine(".label { font-weight: bold; }");
            builder.AppendLine(".section { margin-bottom: 12px; }");
            builder.AppendLine(".responsibility-table { width: 100%; border-collapse: collapse; margin-top: 6px; }");
            builder.AppendLine(".responsibility-table th, .responsibility-table td { border: 1px solid #666; padding: 6px; text-align: left; }");
            builder.AppendLine(".footer { margin-top: 30px; font-size: 11px; }");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<div class=\"header\">");
            builder.AppendLine("  <div class=\"title\">Internal Audit System</div>");
            builder.AppendLine($"  <div class=\"memo-date\">Memo Date: {Encode(memoDate)}</div>");
            builder.AppendLine("</div>");

            builder.AppendLine($"<div class=\"section\"><span class=\"label\">Memo No:</span> {memoNumber}</div>");
            builder.AppendLine($"<div class=\"section\"><span class=\"label\">Title:</span> {title}</div>");
            builder.AppendLine($"<div class=\"section\"><span class=\"label\">Annexure:</span> {annexure}</div>");
            builder.AppendLine($"<div class=\"section\"><span class=\"label\">Risk:</span> {risk}</div>");
            builder.AppendLine("<div class=\"section\">");
            builder.AppendLine("  <div class=\"label\">Body:</div>");
            builder.AppendLine($"  <div>{paraText}</div>");
            builder.AppendLine("</div>");

            if (hasResponsibilities)
                {
                builder.AppendLine("<div class=\"section\">");
                builder.AppendLine("  <div class=\"label\">Responsibility</div>");
                builder.AppendLine("  <table class=\"responsibility-table\">");
                builder.AppendLine("    <thead><tr><th>PP No</th><th>Loan Case</th><th>LC Amount</th></tr></thead>");
                builder.AppendLine("    <tbody>");
                foreach (var responsibility in data.Responsibilities)
                    {
                    if (string.IsNullOrWhiteSpace(responsibility.PpNo)
                        && string.IsNullOrWhiteSpace(responsibility.LoanCase)
                        && string.IsNullOrWhiteSpace(responsibility.LcAmount))
                        {
                        continue;
                        }

                    builder.AppendLine("      <tr>");
                    builder.AppendLine($"        <td>{Encode(responsibility.PpNo)}</td>");
                    builder.AppendLine($"        <td>{Encode(responsibility.LoanCase)}</td>");
                    builder.AppendLine($"        <td>{Encode(responsibility.LcAmount)}</td>");
                    builder.AppendLine("      </tr>");
                    }
                builder.AppendLine("    </tbody>");
                builder.AppendLine("  </table>");
                builder.AppendLine("</div>");
                }

            builder.AppendLine("<div class=\"footer\">Disclaimer: System-generated memo does not require a signature and is subject to change before finalizing the audit report.</div>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            return builder.ToString();
            }

        private static string Encode(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : WebUtility.HtmlEncode(value);
            }
        }
    }

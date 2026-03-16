using AIS.Models.IID;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AIS.Services
    {
    public class IidInquiryReportPdfBuilder
        {
        public string BuildHtml(IidInquiryReportPdfData data)
            {
            if (data == null)
                {
                throw new ArgumentNullException(nameof(data));
                }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size:A4 portrait; margin:18mm; }");
            sb.AppendLine("body{ font-family:'Times New Roman', Times, serif; font-size:12.5px; color:#111; line-height:1.45; }");
            sb.AppendLine(".page-break{ page-break-before:always; }");
            sb.AppendLine(".cover-page{ min-height:250mm; display:flex; flex-direction:column; justify-content:flex-start; align-items:center; text-align:center; padding-top:22mm; }");
            sb.AppendLine(".cover-bank{ font-size:19px; font-weight:700; letter-spacing:.2px; text-transform:uppercase; }");
            sb.AppendLine(".cover-dept{ margin-top:10px; font-size:15px; font-weight:700; text-transform:uppercase; }");
            sb.AppendLine(".cover-title{ margin-top:26px; font-size:30px; font-weight:800; letter-spacing:1.6px; text-transform:uppercase; }");
            sb.AppendLine(".cover-subtitle{ margin-top:24px; font-size:14px; font-weight:700; }");
            sb.AppendLine(".cover-context{ margin-top:18px; width:80%; font-size:13px; }");
            sb.AppendLine(".conducted-by{ margin-top:40px; font-size:14px; }");
            sb.AppendLine(".conducted-by .name{ margin-top:8px; font-size:15px; font-weight:700; }");
            sb.AppendLine(".cover-footer{ margin-top:auto; width:100%; font-size:10.5px; color:#444; display:flex; justify-content:space-between; }");
            sb.AppendLine(".annex-title{ text-align:center; font-size:17px; font-weight:700; margin-bottom:14px; text-transform:uppercase; }");
            sb.AppendLine(".section-block{ margin:10px 0 15px; }");
            sb.AppendLine(".section-heading{ font-weight:700; font-size:13.5px; margin:0 0 6px; }");
            sb.AppendLine(".section-body{ margin-left:12px; text-align:justify; }");
            sb.AppendLine(".section-body ul,.section-body ol{ margin:4px 0 0 20px; padding:0; }");
            sb.AppendLine(".section-body li{ margin:0 0 4px; }");
            sb.AppendLine(".separator-line{ margin-top:8px; color:#666; letter-spacing:.7px; }");
            sb.AppendLine(".signature-block{ margin-top:26px; display:flex; justify-content:space-between; gap:30px; }");
            sb.AppendLine(".signature-cell{ width:45%; text-align:center; }");
            sb.AppendLine(".signature-line{ margin:38px 0 6px; border-top:1px solid #111; }");
            sb.AppendLine(".annex3-table{ width:100%; border-collapse:collapse; margin-top:10px; table-layout:fixed; }");
            sb.AppendLine(".annex3-table th,.annex3-table td{ border:1px solid #222; padding:7px 6px; vertical-align:top; word-wrap:break-word; white-space:pre-wrap; }");
            sb.AppendLine(".annex3-table th{ background:#f1f1f1; font-weight:700; text-align:left; }");
            sb.AppendLine(".muted{ color:#666; }");
            sb.AppendLine("</style></head><body>");

            AppendCover(sb, data);
            AppendAnnexTwo(sb, data);
            AppendAnnexThree(sb, data);

            sb.AppendLine("</body></html>");
            return sb.ToString();
            }

        private static void AppendCover(StringBuilder sb, IidInquiryReportPdfData data)
            {
            var bankName = !string.IsNullOrWhiteSpace(data.Header?.BankName) ? data.Header.BankName : "Zarai Taraqiati Bank Limited";
            var departmentName = !string.IsNullOrWhiteSpace(data.Header?.DepartmentName) ? data.Header.DepartmentName : "Internal Audit Division";
            var reference = Coalesce(data.Header?.ComplaintNo, data.ComplaintSnapshot?.ComplaintNo, "N/A");
            var regionBranch = BuildSlashLine(data.ComplaintSnapshot?.Region, data.ComplaintSnapshot?.Branch);
            var generatedBy = Coalesce(data.Header?.GeneratedByName, "N/A");
            var generatedByPp = Coalesce(data.Header?.GeneratedByPPNo, "N/A");

            sb.AppendLine("<section class='cover-page'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-bank'>{0}</div>", Encode(bankName));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-dept'>{0}</div>", Encode(departmentName));
            sb.AppendLine("<div class='cover-title'>INQUIRY REPORT</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-subtitle'>Inquiry / Complaint Reference: {0}</div>", Encode(reference));
            sb.AppendLine("<div class='cover-context'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Nature / Category:</strong> {0}</div>", Encode(BuildSlashLine(data.ComplaintSnapshot?.Nature, data.ComplaintSnapshot?.Category)));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Region / Branch:</strong> {0}</div>", Encode(regionBranch));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Inquiry Status:</strong> {0}</div>", Encode(Coalesce(data.Header?.InquiryStatus, data.FinalConclusion?.InquiryStatus, "N/A")));
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='conducted-by'><div><strong>Conducted by</strong></div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='name'>{0}</div><div>PP No: {1}</div></div>", Encode(generatedBy), Encode(generatedByPp));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-footer'><span>Confidential - Internal Use Only</span><span>Generated on: {0}</span></div>", Encode(FormatDate(data.Header?.GeneratedOn)));
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexTwo(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='page-break'>");
            sb.AppendLine("<div class='annex-title'>Annex-II</div>");

            AppendNarrativeSection(sb, "1. Complaint Details / Incident Reported",
                BuildComplaintDetails(data));

            AppendNarrativeSection(sb, "2. Complainant Details",
                BuildComplainantDetails(data));

            AppendNarrativeSection(sb, "3. Reference Details and Originating Process Center",
                BuildReferenceDetails(data));

            AppendNarrativeSection(sb, "4. Details of Accusations",
                BuildAccusationDetails(data));

            AppendNarrativeSection(sb, "5. Main Alleged Accused Details",
                BuildMainAccusedDetails(data));

            AppendNarrativeSection(sb, "6. Alleged Co-accused Details",
                BuildCoAccusedDetails(data));

            AppendNarrativeSection(sb, "7. Inquiry Proceedings",
                BuildSingleLineList(data.FinalConclusion?.Proceedings));

            AppendNarrativeSection(sb, "8. Details of Record Scrutinized",
                BuildRecordDetails(data));

            AppendNarrativeSection(sb, "9. Time and place of Recording Statement of Complainant",
                BuildStatementLog(data, true));

            AppendNarrativeSection(sb, "10. Time and place of Recording Statement of Accused",
                BuildStatementLog(data, false));

            AppendNarrativeSection(sb, "11. Critical Points highlighted in statement of complainant",
                BuildStatementPoints(data, true));

            AppendNarrativeSection(sb, "12. Critical Points highlighted in statement of accused",
                BuildStatementPoints(data, false));

            AppendNarrativeSection(sb, "13. Details of material evidence",
                BuildEvidenceDetails(data, false));

            AppendNarrativeSection(sb, "14. Details of circumstantial evidence",
                BuildEvidenceDetails(data, true));

            AppendNarrativeSection(sb, "15. Details of findings with implications / violated policy references",
                BuildFindingsAndViolations(data));

            AppendNarrativeSection(sb, "16. Details of clear recommendations",
                BuildRecommendations(data));

            AppendNarrativeSection(sb, "17. Whether reported in latest audit report",
                BuildSingleLineList("Not specifically captured in source records."));

            AppendNarrativeSection(sb, "18. Root cause of incident",
                BuildSingleLineList(data.FinalConclusion?.Findings));

            AppendNarrativeSection(sb, "19. Name with PP.No. of accused against whom DSAs framed",
                BuildDsaNarrative(data));

            AppendNarrativeSection(sb, "20. Summary of violations statement",
                BuildViolationSummary(data));

            AppendSignature(sb);
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexThree(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='page-break'>");
            sb.AppendLine("<div class='annex-title'>Annex-III</div>");
            sb.AppendLine("<div class='section-heading'>Structured Violation Summary</div>");

            if (!data.Violations.Any())
                {
                sb.AppendLine("<p class='muted'>No violation records are available.</p>");
                sb.AppendLine("</section>");
                return;
                }

            sb.AppendLine("<table class='annex3-table'>");
            sb.AppendLine("<colgroup><col style='width:16%'><col style='width:34%'><col style='width:22%'><col style='width:28%'></colgroup>");
            sb.AppendLine("<thead><tr><th>Category</th><th>Violation Detail</th><th>Policy/Reference</th><th>Recommendation</th></tr></thead><tbody>");
            foreach (var row in data.Violations.OrderBy(x => x.SortOrder))
                {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                    Encode(row.Category),
                    ToParagraphs(row.ViolationDetail),
                    ToParagraphs(row.ReferenceText),
                    ToParagraphs(row.Recommendation));
                }
            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</section>");
            }

        private static void AppendNarrativeSection(StringBuilder sb, string title, IEnumerable<string> lines)
            {
            var sectionLines = lines?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();

            sb.AppendLine("<div class='section-block'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='section-heading'>{0}</div>", Encode(title));
            sb.AppendLine("<div class='section-body'>");

            if (!sectionLines.Any())
                {
                sb.AppendLine("<div>N/A</div>");
                }
            else if (sectionLines.Count == 1)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div>{0}</div>", ToParagraphs(sectionLines[0]));
                }
            else
                {
                sb.AppendLine("<ol>");
                foreach (var line in sectionLines)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<li>{0}</li>", ToParagraphs(line));
                    }
                sb.AppendLine("</ol>");
                }

            sb.AppendLine("</div>");
            sb.AppendLine("<div class='separator-line'>________________</div>");
            sb.AppendLine("</div>");
            }

        private static void AppendSignature(StringBuilder sb)
            {
            sb.AppendLine("<div class='signature-block'>");
            sb.AppendLine("<div class='signature-cell'><div class='signature-line'></div><div><strong>Team Member</strong></div></div>");
            sb.AppendLine("<div class='signature-cell'><div class='signature-line'></div><div><strong>Team Leader / Head</strong></div></div>");
            sb.AppendLine("</div>");
            }

        private static IEnumerable<string> BuildComplaintDetails(IidInquiryReportPdfData data)
            {
            return new[]
                {
                $"Complaint No: {Coalesce(data.ComplaintSnapshot?.ComplaintNo, data.Header?.ComplaintNo, "N/A")}",
                $"Nature / Category: {BuildSlashLine(data.ComplaintSnapshot?.Nature, data.ComplaintSnapshot?.Category)}",
                $"Submitted On: {Coalesce(data.ComplaintSnapshot?.SubmittedOn, "N/A")}",
                $"Action Required: {Coalesce(data.ComplaintSnapshot?.ActionRequired, "N/A")}",
                $"Incident Reported: {Coalesce(data.ComplaintSnapshot?.Contents, "N/A")}".Trim()
                };
            }

        private static IEnumerable<string> BuildComplainantDetails(IidInquiryReportPdfData data)
            {
            return new[]
                {
                $"Name: {Coalesce(data.ComplaintSnapshot?.ComplainantName, "N/A")}",
                $"CNIC: {Coalesce(data.ComplaintSnapshot?.Cnic, "N/A")}",
                $"Cell No: {Coalesce(data.ComplaintSnapshot?.CellularNumber, "N/A")}",
                $"Gender: {Coalesce(data.ComplaintSnapshot?.Gender, "N/A")}",
                $"Mailing Address: {Coalesce(data.ComplaintSnapshot?.MailingAddress, "N/A")}",
                $"Received From: {Coalesce(data.ComplaintSnapshot?.ReceivedFrom, "N/A")}".Trim()
                };
            }

        private static IEnumerable<string> BuildReferenceDetails(IidInquiryReportPdfData data)
            {
            return new[]
                {
                $"Reference Number: {Coalesce(data.Header?.ComplaintNo, data.ComplaintSnapshot?.ComplaintNo, "N/A")}",
                $"Originating Region / Branch: {BuildSlashLine(data.ComplaintSnapshot?.Region, data.ComplaintSnapshot?.Branch)}",
                $"Location Type: {Coalesce(data.ComplaintSnapshot?.LocationType, "N/A")}",
                $"Inquiry Status: {Coalesce(data.Header?.InquiryStatus, data.FinalConclusion?.InquiryStatus, "N/A")}".Trim()
                };
            }

        private static IEnumerable<string> BuildAccusationDetails(IidInquiryReportPdfData data)
            {
            return data.Accusations.OrderBy(x => x.SortOrder).Select(x => x.AccusationText);
            }

        private static IEnumerable<string> BuildMainAccusedDetails(IidInquiryReportPdfData data)
            {
            var mainRow = data.AccusedList.FirstOrDefault();
            if (mainRow == null)
                {
                return Array.Empty<string>();
                }

            return new[]
                {
                $"Name: {Coalesce(mainRow.PersonName, "N/A")}",
                $"Designation: {Coalesce(mainRow.Designation, "N/A")}",
                $"PP No: {Coalesce(mainRow.PpnoNumber, "N/A")}",
                $"CNIC: {Coalesce(mainRow.Cnic, "N/A")}",
                $"Role / Posting: {BuildSlashLine(mainRow.RoleType, mainRow.PostingPlace)}"
                };
            }

        private static IEnumerable<string> BuildCoAccusedDetails(IidInquiryReportPdfData data)
            {
            return data.AccusedList.Skip(1).Select(x =>
                $"{Coalesce(x.PersonName, "N/A")} ({Coalesce(x.Designation, "N/A")}) - PP No: {Coalesce(x.PpnoNumber, "N/A")}, CNIC: {Coalesce(x.Cnic, "N/A")}, Role/Posting: {BuildSlashLine(x.RoleType, x.PostingPlace)}");
            }

        private static IEnumerable<string> BuildStatementLog(IidInquiryReportPdfData data, bool complainant)
            {
            return data.Statements
                .Where(x => IsComplainantRole(x.RoleType) == complainant)
                .Select(x => $"{Coalesce(x.PersonName, "N/A")} | Date/Time: {FormatDate(x.StatementDatetime)} | Place: {Coalesce(x.Place, "N/A")} | Mode: {Coalesce(x.ModeType, "N/A")}");
            }

        private static IEnumerable<string> BuildStatementPoints(IidInquiryReportPdfData data, bool complainant)
            {
            return data.Statements
                .Where(x => IsComplainantRole(x.RoleType) == complainant)
                .Select(x => $"{Coalesce(x.PersonName, "N/A")}: {Coalesce(x.KeyPoints, "N/A")}");
            }

        private static IEnumerable<string> BuildRecordDetails(IidInquiryReportPdfData data)
            {
            return data.RecordsScrutinized
                .OrderBy(x => x.SortOrder)
                .Select(x => $"{Coalesce(x.RecordTitle, "Record")}: {Coalesce(x.RecordDetails, "N/A")}");
            }

        private static IEnumerable<string> BuildEvidenceDetails(IidInquiryReportPdfData data, bool circumstantial)
            {
            return data.EvidenceFiles
                .Where(x => IsCircumstantial(x.EvidenceType) == circumstantial)
                .Select(x => $"{Coalesce(x.EvidenceType, "Evidence")}: {Coalesce(x.Description, "N/A")} (File: {Coalesce(x.FileName, "N/A")}, Uploaded: {FormatDate(x.UploadedOn)})");
            }

        private static IEnumerable<string> BuildFindingsAndViolations(IidInquiryReportPdfData data)
            {
            var findings = data.FindingsRecommendations
                .Where(x => x.AccusationId != 0)
                .Select(x => $"Allegation: {Coalesce(x.AccusationText, "N/A")}; Finding: {Coalesce(x.FindingText, "N/A")}; Outcome: {Coalesce(x.Outcome, "N/A")}");

            var violations = data.Violations
                .OrderBy(x => x.SortOrder)
                .Select(x => $"Violation ({Coalesce(x.Category, "General")}): {Coalesce(x.ViolationDetail, "N/A")}; Reference: {Coalesce(x.ReferenceText, "N/A")}");

            return findings.Concat(violations);
            }

        private static IEnumerable<string> BuildRecommendations(IidInquiryReportPdfData data)
            {
            var recommendations = data.FindingsRecommendations
                .Select(x => x.RecommendationText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim());

            if (!recommendations.Any() && !string.IsNullOrWhiteSpace(data.FinalConclusion?.Recommendation))
                {
                return new[] { data.FinalConclusion.Recommendation };
                }

            return recommendations;
            }

        private static IEnumerable<string> BuildDsaNarrative(IidInquiryReportPdfData data)
            {
            return data.DsaFiles
                .OrderBy(x => x.SortOrder)
                .Select(x => $"{Coalesce(x.PersonName, "N/A")} ({Coalesce(x.Designation, "N/A")}) - PP No: {Coalesce(x.PpnoNumber, "N/A")}, DSA Status: {Coalesce(x.DsaStatus, "N/A")}, Remarks: {Coalesce(x.Remarks, "N/A")}");
            }

        private static IEnumerable<string> BuildViolationSummary(IidInquiryReportPdfData data)
            {
            if (!data.Violations.Any())
                {
                return BuildSingleLineList(data.FinalConclusion?.FinalOutcome);
                }

            return data.Violations
                .OrderBy(x => x.SortOrder)
                .Select(x => $"{Coalesce(x.Category, "General")}: {Coalesce(x.ViolationDetail, "N/A")}");
            }

        private static IEnumerable<string> BuildSingleLineList(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() : new[] { value };
            }

        private static string Encode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

        private static string ToParagraphs(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return "N/A";
                }

            return Encode(value).Replace("\r\n", "\n").Replace("\n", "<br/>");
            }

        private static bool IsComplainantRole(string roleType)
            {
            var value = (roleType ?? string.Empty).Trim().ToLowerInvariant();
            return value.Contains("complain");
            }

        private static bool IsCircumstantial(string evidenceType)
            {
            var value = (evidenceType ?? string.Empty).Trim().ToLowerInvariant();
            return value.Contains("circum");
            }

        private static string FormatDate(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : "N/A";
            }

        private static string BuildSlashLine(string left, string right)
            {
            return $"{Coalesce(left, "N/A")} / {Coalesce(right, "N/A")}";
            }

        private static string Coalesce(params string[] values)
            {
            foreach (var value in values)
                {
                if (!string.IsNullOrWhiteSpace(value))
                    {
                    return value.Trim();
                    }
                }

            return "N/A";
            }
        }
    }

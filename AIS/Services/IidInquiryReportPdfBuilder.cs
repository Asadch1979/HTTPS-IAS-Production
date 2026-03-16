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
            sb.AppendLine("body { font-family: 'Times New Roman', Times, serif; font-size: 12pt; color: #111; line-height: 1.5; }");
            sb.AppendLine(".page-break { page-break-before: always; }");
            sb.AppendLine(".cover { min-height: 245mm; display: flex; flex-direction: column; justify-content: center; text-align: center; }");
            sb.AppendLine(".cover-bank { font-size: 18pt; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6mm; }");
            sb.AppendLine(".cover-dept { font-size: 14pt; font-weight: 700; text-transform: uppercase; margin-bottom: 12mm; }");
            sb.AppendLine(".cover-title { font-size: 24pt; font-weight: 700; letter-spacing: 1px; margin-bottom: 12mm; }");
            sb.AppendLine(".cover-ref { font-size: 12pt; margin: 2mm 0; }");
            sb.AppendLine(".cover-conducted { margin-top: 18mm; }");
            sb.AppendLine(".cover-subtle { margin-top: auto; font-size: 10pt; color: #444; }");
            sb.AppendLine(".annex-heading { text-align: center; font-size: 16pt; font-weight: 700; margin: 0 0 8mm; }");
            sb.AppendLine(".annex-subheading { text-align: center; font-size: 12pt; font-weight: 700; margin: 0 0 8mm; text-transform: uppercase; }");
            sb.AppendLine(".section { margin: 0 0 7mm; }");
            sb.AppendLine(".section-label { font-weight: 700; margin-bottom: 2mm; }");
            sb.AppendLine(".section-body { margin-left: 4mm; text-align: justify; }");
            sb.AppendLine(".section-body ul, .section-body ol { margin: 2mm 0 0 5mm; padding-left: 4mm; }");
            sb.AppendLine(".separator { margin-top: 3mm; margin-left: 4mm; color: #444; }");
            sb.AppendLine(".signature-grid { margin-top: 16mm; width: 100%; border-collapse: collapse; }");
            sb.AppendLine(".signature-grid td { width: 50%; text-align: center; vertical-align: bottom; padding-top: 12mm; }");
            sb.AppendLine(".signature-line { display: inline-block; min-width: 60mm; border-top: 1px solid #222; padding-top: 2mm; font-weight: 700; }");
            sb.AppendLine(".annex-three-title { text-align: center; font-size: 16pt; font-weight: 700; margin: 0 0 6mm; }");
            sb.AppendLine(".annex-three-table { width: 100%; border-collapse: collapse; table-layout: fixed; }");
            sb.AppendLine(".annex-three-table th, .annex-three-table td { border: 1px solid #333; padding: 6px 7px; vertical-align: top; font-size: 10.5pt; }");
            sb.AppendLine(".annex-three-table th { text-align: left; background: #f5f5f5; }");
            sb.AppendLine(".col-sr { width: 6%; } .col-category { width: 16%; } .col-violation { width: 30%; } .col-reference { width: 24%; } .col-recommendation { width: 24%; }");
            sb.AppendLine(".muted { color: #666; }");
            sb.AppendLine("</style></head><body>");

            AppendCover(sb, data);
            AppendAnnexTwo(sb, data);
            AppendAnnexThree(sb, data);

            sb.AppendLine("</body></html>");
            return sb.ToString();
            }

        private static void AppendCover(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='cover'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-bank'>{0}</div>", Encode(GetValue(data.Header?.BankName, "Zarai Taraqiati Bank Limited")));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-dept'>{0}</div>", Encode(GetValue(data.Header?.DepartmentName, "Internal Audit Division")));
            sb.AppendLine("<div class='cover-title'>INQUIRY REPORT</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-ref'><strong>Inquiry / Complaint Reference:</strong> {0}</div>", Encode(GetValue(data.Header?.ComplaintNo, data.ComplaintSnapshot?.ComplaintNo)));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-ref'><strong>Branch / Context:</strong> {0}</div>", Encode(JoinNotEmpty(" | ", data.ComplaintSnapshot?.Region, data.ComplaintSnapshot?.Branch, data.ComplaintSnapshot?.LocationType)));
            sb.AppendLine("<div class='cover-conducted'>");
            sb.AppendLine("<div><strong>Conducted by</strong></div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div>{0}</div>", Encode(GetValue(data.Header?.GeneratedByName, "Inquiry Team")));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div>PP No: {0}</div>", Encode(GetValue(data.Header?.GeneratedByPPNo, "N/A")));
            sb.AppendLine("</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-subtle'>Generated on: {0} &nbsp;&nbsp; | &nbsp;&nbsp; Confidential (Internal Use)</div>", Encode(FormatDate(data.Header?.GeneratedOn)));
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexTwo(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='page-break'>");
            sb.AppendLine("<div class='annex-heading'>Annex-II</div>");
            sb.AppendLine("<div class='annex-subheading'>Detailed Inquiry Narrative</div>");

            AppendSection(sb, "1. Complaint Details / Incident Reported", new[]
                {
                $"Complaint No: {GetValue(data.ComplaintSnapshot?.ComplaintNo)}",
                $"Nature / Category: {JoinNotEmpty(" / ", data.ComplaintSnapshot?.Nature, data.ComplaintSnapshot?.Category)}",
                $"Submitted On: {GetValue(data.ComplaintSnapshot?.SubmittedOn)}",
                $"Action Required: {GetValue(data.ComplaintSnapshot?.ActionRequired)}",
                $"Incident Narrative: {GetValue(data.ComplaintSnapshot?.Contents)}"
                });

            AppendSection(sb, "2. Complainant Details", new[]
                {
                $"Name: {GetValue(data.ComplaintSnapshot?.ComplainantName)}",
                $"CNIC: {GetValue(data.ComplaintSnapshot?.Cnic)}",
                $"Cell Number: {GetValue(data.ComplaintSnapshot?.CellularNumber)}",
                $"Gender: {GetValue(data.ComplaintSnapshot?.Gender)}",
                $"Address: {GetValue(data.ComplaintSnapshot?.MailingAddress)}"
                });

            AppendSection(sb, "3. Reference Details and Originating Process Center", new[]
                {
                $"Received From: {GetValue(data.ComplaintSnapshot?.ReceivedFrom)}",
                $"Region / Branch: {JoinNotEmpty(" / ", data.ComplaintSnapshot?.Region, data.ComplaintSnapshot?.Branch)}",
                $"Location Type: {GetValue(data.ComplaintSnapshot?.LocationType)}",
                $"Referenced Documents: {JoinNotEmpty(", ", data.ComplaintSnapshot?.UploadedComplaint, data.ComplaintSnapshot?.UploadedFFR, data.ComplaintSnapshot?.UploadedEvidence)}"
                });

            AppendSection(sb, "4. Details of Accusations", ToNumberedItems(data.Accusations.OrderBy(a => a.SortOrder).Select(a => a.AccusationText)));

            var accused = data.AccusedList.Where(a => !IsComplainantRole(a.RoleType)).ToList();
            var mainAccused = accused.FirstOrDefault() ?? data.AccusedList.FirstOrDefault();
            AppendSection(sb, "5. Main Alleged Accused Details", mainAccused == null
                ? new[] { "No main alleged accused details available." }
                : new[]
                    {
                    $"Name: {GetValue(mainAccused.PersonName)}",
                    $"Designation: {GetValue(mainAccused.Designation)}",
                    $"Role: {GetValue(mainAccused.RoleType)}",
                    $"PP No: {GetValue(mainAccused.PpnoNumber)}",
                    $"CNIC: {GetValue(mainAccused.Cnic)}",
                    $"Posting Place: {GetValue(mainAccused.PostingPlace)}"
                    });

            AppendSection(sb, "6. Alleged Co-accused Details", ToNumberedItems(accused.Skip(mainAccused == null ? 0 : 1).Select(x =>
                $"{GetValue(x.PersonName)} ({GetValue(x.Designation)}), PP No: {GetValue(x.PpnoNumber)}, Role: {GetValue(x.RoleType)}, Posting: {GetValue(x.PostingPlace)}")));

            AppendSection(sb, "7. Inquiry Proceedings", new[] { GetValue(data.FinalConclusion?.Proceedings) });
            AppendSection(sb, "8. Details of Record Scrutinized", ToNumberedItems(data.RecordsScrutinized.OrderBy(r => r.SortOrder).Select(r =>
                $"{GetValue(r.RecordTitle)}: {GetValue(r.RecordDetails)}")));

            var complainantStatements = data.Statements.Where(x => IsComplainantRole(x.RoleType)).ToList();
            var accusedStatements = data.Statements.Where(x => !IsComplainantRole(x.RoleType)).ToList();

            AppendSection(sb, "9. Time and place of Recording Statement of Complainant", ToNumberedItems(complainantStatements.Select(s =>
                $"{GetValue(s.PersonName)} - Date/Time: {FormatDate(s.StatementDatetime)}, Place: {GetValue(s.Place)}, Mode: {GetValue(s.ModeType)}")));

            AppendSection(sb, "10. Time and place of Recording Statement of Accused", ToNumberedItems(accusedStatements.Select(s =>
                $"{GetValue(s.PersonName)} - Date/Time: {FormatDate(s.StatementDatetime)}, Place: {GetValue(s.Place)}, Mode: {GetValue(s.ModeType)}")));

            AppendSection(sb, "11. Critical Points highlighted in statement of complainant", ToNumberedItems(complainantStatements.Select(s =>
                $"{GetValue(s.PersonName)}: {GetValue(s.KeyPoints)}")));

            AppendSection(sb, "12. Critical Points highlighted in statement of accused", ToNumberedItems(accusedStatements.Select(s =>
                $"{GetValue(s.PersonName)}: {GetValue(s.KeyPoints)}")));

            AppendSection(sb, "13. Details of material evidence", ToNumberedItems(data.EvidenceFiles
                .Where(e => !ContainsIgnoreCase(e.EvidenceType, "circumstantial"))
                .Select(e => $"{GetValue(e.EvidenceType)} - {GetValue(e.Description)} (File: {GetValue(e.FileName)}, Uploaded: {FormatDate(e.UploadedOn)})")));

            AppendSection(sb, "14. Details of circumstantial evidence", ToNumberedItems(data.EvidenceFiles
                .Where(e => ContainsIgnoreCase(e.EvidenceType, "circumstantial"))
                .Select(e => $"{GetValue(e.EvidenceType)} - {GetValue(e.Description)} (File: {GetValue(e.FileName)}, Uploaded: {FormatDate(e.UploadedOn)})")));

            AppendSection(sb, "15. Details of findings with implications / violated policy references", BuildFindingsWithReferences(data));
            AppendSection(sb, "16. Details of clear recommendations", BuildRecommendations(data));
            AppendSection(sb, "17. Whether reported in latest audit report", new[] { "Not specifically available in source records." });
            AppendSection(sb, "18. Root cause of incident", new[] { GetValue(data.FinalConclusion?.Findings, data.FinalConclusion?.Gist) });

            AppendSection(sb, "19. Name with PP.No. of accused against whom DSAs framed", ToNumberedItems(data.DsaFiles.OrderBy(d => d.SortOrder).Select(d =>
                $"{GetValue(d.PersonName)} ({GetValue(d.Designation)}), PP No: {GetValue(d.PpnoNumber)}, Status: {GetValue(d.DsaStatus)}, Remarks: {GetValue(d.Remarks)}")));

            AppendSection(sb, "20. Summary of violations statement", ToNumberedItems(data.Violations.OrderBy(v => v.SortOrder).Select(v =>
                $"{GetValue(v.Category)}: {GetValue(v.ViolationDetail)} | Reference: {GetValue(v.ReferenceText)}")));

            sb.AppendLine("<table class='signature-grid'><tr><td><span class='signature-line'>Team Member</span></td><td><span class='signature-line'>Team Leader / Head</span></td></tr></table>");
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexThree(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='page-break'>");
            sb.AppendLine("<div class='annex-three-title'>Annex-III</div>");
            sb.AppendLine("<div class='annex-subheading'>Violation Summary</div>");

            if (!data.Violations.Any())
                {
                sb.AppendLine("<p class='muted'>No violations recorded for Annex-III.</p></section>");
                return;
                }

            sb.AppendLine("<table class='annex-three-table'>");
            sb.AppendLine("<thead><tr><th class='col-sr'>#</th><th class='col-category'>Category</th><th class='col-violation'>Violation Detail</th><th class='col-reference'>Policy / Reference</th><th class='col-recommendation'>Recommendation</th></tr></thead><tbody>");
            var idx = 1;
            foreach (var row in data.Violations.OrderBy(v => v.SortOrder))
                {
                sb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                    idx++,
                    Encode(GetValue(row.Category)),
                    ToParagraphs(row.ViolationDetail),
                    ToParagraphs(row.ReferenceText),
                    ToParagraphs(row.Recommendation));
                }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</section>");
            }

        private static void AppendSection(StringBuilder sb, string title, IEnumerable<string> lines)
            {
            var items = lines?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
            if (!items.Any())
                {
                items.Add("N/A");
                }

            sb.AppendLine("<div class='section'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='section-label'>{0}</div>", Encode(title));
            sb.AppendLine("<div class='section-body'>");
            if (items.Count == 1)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div>{0}</div>", ToParagraphs(items[0]));
                }
            else
                {
                sb.AppendLine("<ol>");
                foreach (var item in items)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<li>{0}</li>", ToParagraphs(item));
                    }

                sb.AppendLine("</ol>");
                }

            sb.AppendLine("</div>");
            sb.AppendLine("<div class='separator'>________________</div>");
            sb.AppendLine("</div>");
            }

        private static IEnumerable<string> ToNumberedItems(IEnumerable<string> values)
            {
            var items = values?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(GetValue).ToList() ?? new List<string>();
            return items.Any() ? items : new[] { "N/A" };
            }

        private static IEnumerable<string> BuildFindingsWithReferences(IidInquiryReportPdfData data)
            {
            var items = new List<string>();
            foreach (var finding in data.FindingsRecommendations.Where(f => f.AccusationId != 0 || !string.IsNullOrWhiteSpace(f.AccusationText)))
                {
                items.Add($"Accusation: {GetValue(finding.AccusationText)}. Finding: {GetValue(finding.FindingText)}. Outcome: {GetValue(finding.Outcome)}.");
                }

            foreach (var v in data.Violations)
                {
                items.Add($"Violation Category: {GetValue(v.Category)}. Policy / Rule Reference: {GetValue(v.ReferenceText)}.");
                }

            return items.Any() ? items : new[] { "N/A" };
            }

        private static IEnumerable<string> BuildRecommendations(IidInquiryReportPdfData data)
            {
            var items = new List<string>();
            items.AddRange(data.FindingsRecommendations.Where(f => !string.IsNullOrWhiteSpace(f.RecommendationText)).Select(f =>
                $"Against accusation '{GetValue(f.AccusationText)}': {GetValue(f.RecommendationText)}"));
            items.AddRange(data.Violations.Where(v => !string.IsNullOrWhiteSpace(v.Recommendation)).Select(v =>
                $"{GetValue(v.Category)}: {GetValue(v.Recommendation)}"));

            if (!string.IsNullOrWhiteSpace(data.FinalConclusion?.Recommendation))
                {
                items.Add($"Overall Recommendation: {GetValue(data.FinalConclusion.Recommendation)}");
                }

            return items.Any() ? items : new[] { "N/A" };
            }

        private static bool IsComplainantRole(string roleType)
            {
            var value = (roleType ?? string.Empty).Trim().ToLowerInvariant();
            return value.Contains("complain");
            }

        private static bool ContainsIgnoreCase(string value, string contains)
            {
            return (value ?? string.Empty).IndexOf(contains ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
            }

        private static string JoinNotEmpty(string separator, params string[] values)
            {
            var normalized = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();
            return normalized.Any() ? string.Join(separator, normalized) : "N/A";
            }

        private static string GetValue(string value, string fallback = "N/A")
            {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            }

        private static string Encode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

        private static string ToParagraphs(string value)
            {
            return Encode(GetValue(value)).Replace("\r\n", "\n").Replace("\n", "<br/>");
            }

        private static string FormatDate(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : "N/A";
            }

        private static string FormatDate(DateTime value)
            {
            return value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
            }
        }
    }

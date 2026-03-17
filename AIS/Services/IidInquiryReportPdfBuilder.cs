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
        private const string AnnexSeparator = "________________";

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
            sb.AppendLine("body{ font-family:'Times New Roman', Times, serif; font-size:12px; color:#111; line-height:1.45; }");
            sb.AppendLine(".page-break{ page-break-before:always; break-before:page; margin:0; padding:0; height:1px; line-height:0; font-size:0; }");
            sb.AppendLine(".cover{ min-height:245mm; display:flex; flex-direction:column; justify-content:center; text-align:center; }");
            sb.AppendLine(".cover-bank{ font-size:20px; font-weight:700; letter-spacing:.4px; text-transform:uppercase; }");
            sb.AppendLine(".cover-dept{ font-size:15px; padding-top:10px; font-weight:600; text-transform:uppercase; }");
            sb.AppendLine(".cover-title{ font-size:30px; padding-top:44px; font-weight:700; letter-spacing:1.5px; text-transform:uppercase; }");
            sb.AppendLine(".cover-ref{ padding-top:30px; font-size:13px; }");
            sb.AppendLine(".cover-conducted{ padding-top:36px; font-size:13px; line-height:1.8; }");
            sb.AppendLine(".cover-meta{ margin-top:auto; font-size:10px; color:#555; }");
            sb.AppendLine(".annex-page{ padding-top:2px; }");
            sb.AppendLine(".annex-title{ text-align:center; font-weight:700; font-size:18px; padding-bottom:3px; }");
            sb.AppendLine(".annex-subtitle{ text-align:center; font-size:12px; padding-bottom:20px; }");
            sb.AppendLine(".annex-section{ padding-bottom:12px; }");
            sb.AppendLine(".annex-label{ font-size:12.5px; font-weight:700; padding-bottom:6px; }");
            sb.AppendLine(".annex-body{ padding-left:14px; text-align:justify; }");
            sb.AppendLine(".annex-paragraph{ padding-bottom:6px; }");
            sb.AppendLine(".annex-body ul, .annex-body ol{ padding-left:18px; }");
            sb.AppendLine(".annex-body li{ padding-bottom:4px; }");
            sb.AppendLine(".section-separator{ padding-top:8px; color:#666; letter-spacing:1px; }");
            sb.AppendLine(".signature-wrap{ padding-top:26px; display:flex; gap:24px; justify-content:space-between; }");
            sb.AppendLine(".signature-block{ width:47%; text-align:center; }");
            sb.AppendLine(".signature-line{ border-top:1px solid #111; padding-top:46px; font-weight:700; }");
            sb.AppendLine(".violation-table{ width:100%; border-collapse:collapse; table-layout:fixed; }");
            sb.AppendLine(".violation-table th,.violation-table td{ border:1px solid #666; padding:7px; vertical-align:top; word-wrap:break-word; }");
            sb.AppendLine(".violation-table th{ background:#f2f2f2; text-align:left; }");
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
            var header = data.Header ?? new IidInquiryHeaderModel();
            var snapshot = data.ComplaintSnapshot ?? new IidComplaintSnapshotModel();

            sb.AppendLine("<section class='cover'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-bank'>{0}</div>", EncodeOrDefault(header.BankName, "Zarai Taraqiati Bank Limited")).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-dept'>{0}</div>", EncodeOrDefault(header.DepartmentName, "Internal Audit Department")).AppendLine();
            sb.AppendLine("<div class='cover-title'>Inquiry Report</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-ref'><div><strong>Inquiry / Complaint Reference:</strong> {0}</div><div><strong>Branch / Region Context:</strong> {1}</div></div>",
                EncodeOrDefault(header.ComplaintNo ?? snapshot.ComplaintNo),
                EncodeOrDefault(JoinNonEmpty(" / ", snapshot.Branch, snapshot.Region), "N/A")).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-conducted'><div><strong>Conducted by</strong></div><div>{0}</div><div>PP No: {1}</div><div>Inquiry Status: {2}</div></div>",
                EncodeOrDefault(header.GeneratedByName, "Inquiry Team"),
                EncodeOrDefault(header.GeneratedByPPNo),
                EncodeOrDefault(header.InquiryStatus)).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-meta'>Generated on {0} &nbsp;|&nbsp; Confidential - Internal Use Only</div>",
                Encode(FormatDate(header.GeneratedOn))).AppendLine();
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexTwo(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<div class='page-break'>&nbsp;</div>");
            sb.AppendLine("<section class='annex-page'>");
            sb.AppendLine("<div class='annex-title'>Annex-II</div>");
            sb.AppendLine("<div class='annex-subtitle'>Inquiry Narrative and Proceedings</div>");

            var snapshot = data.ComplaintSnapshot ?? new IidComplaintSnapshotModel();
            var accusations = data.Accusations.OrderBy(x => x.SortOrder).ThenBy(x => x.AccusationId).ToList();
            var mainAccused = data.AccusedList.Where(x => !IsCoAccused(x.RoleType)).ToList();
            var coAccused = data.AccusedList.Where(x => IsCoAccused(x.RoleType)).ToList();
            var complainantStatements = data.Statements.Where(x => IsComplainantRole(x.RoleType)).OrderBy(x => x.StatementDatetime).ToList();
            var accusedStatements = data.Statements.Where(x => !IsComplainantRole(x.RoleType)).OrderBy(x => x.StatementDatetime).ToList();
            var materialEvidence = data.EvidenceFiles.Where(x => (x.EvidenceType ?? string.Empty).ToLowerInvariant().Contains("material")).ToList();
            var circumstantialEvidence = data.EvidenceFiles.Where(x => !(x.EvidenceType ?? string.Empty).ToLowerInvariant().Contains("material")).ToList();
            var frRows = data.FindingsRecommendations.Where(x => x.AccusationId != 0).ToList();

            AppendAnnexSection(sb, "1. Complaint Details / Incident Reported",
                BuildParagraphs(
                    BuildPair("Complaint Number", snapshot.ComplaintNo),
                    BuildPair("Nature / Category", JoinNonEmpty(" / ", snapshot.Nature, snapshot.Category)),
                    BuildPair("Submitted On", snapshot.SubmittedOn),
                    BuildPair("Incident Narrative", snapshot.Contents)));

            AppendAnnexSection(sb, "2. Complainant Details",
                BuildParagraphs(
                    BuildPair("Name", snapshot.ComplainantName),
                    BuildPair("CNIC", snapshot.Cnic),
                    BuildPair("Cell Number", snapshot.CellularNumber),
                    BuildPair("Address", snapshot.MailingAddress),
                    BuildPair("Gender", snapshot.Gender)));

            AppendAnnexSection(sb, "3. Reference Details and Originating Process Center",
                BuildParagraphs(
                    BuildPair("Complaint Reference", data.Header?.ComplaintNo ?? snapshot.ComplaintNo),
                    BuildPair("Originating Source", snapshot.ReceivedFrom),
                    BuildPair("Region / Branch", JoinNonEmpty(" / ", snapshot.Region, snapshot.Branch)),
                    BuildPair("Location Type", snapshot.LocationType),
                    BuildPair("Action Required", snapshot.ActionRequired)));

            AppendAnnexSection(sb, "4. Details of Accusations",
                BuildOrderedList(accusations.Select(x => x.AccusationText)));

            AppendAnnexSection(sb, "5. Main Alleged Accused Details",
                BuildNarrativeList(mainAccused.Select(BuildAccusedNarrative)));

            AppendAnnexSection(sb, "6. Alleged Co-accused Details",
                BuildNarrativeList(coAccused.Select(BuildAccusedNarrative)));

            AppendAnnexSection(sb, "7. Inquiry Proceedings",
                BuildParagraphs(data.FinalConclusion?.Proceedings));

            AppendAnnexSection(sb, "8. Details of Record Scrutinized",
                BuildNarrativeList(data.RecordsScrutinized
                    .OrderBy(x => x.SortOrder)
                    .Select(x => JoinNonEmpty(" - ", x.RecordTitle, x.RecordDetails))));

            AppendAnnexSection(sb, "9. Time and place of Recording Statement of Complainant",
                BuildNarrativeList(complainantStatements.Select(BuildStatementTimeline)));

            AppendAnnexSection(sb, "10. Time and place of Recording Statement of Accused",
                BuildNarrativeList(accusedStatements.Select(BuildStatementTimeline)));

            AppendAnnexSection(sb, "11. Critical Points highlighted in statement of complainant",
                BuildNarrativeList(complainantStatements.Select(x => JoinNonEmpty(" - ", x.PersonName, x.KeyPoints))));

            AppendAnnexSection(sb, "12. Critical Points highlighted in statement of accused",
                BuildNarrativeList(accusedStatements.Select(x => JoinNonEmpty(" - ", x.PersonName, x.KeyPoints))));

            AppendAnnexSection(sb, "13. Details of material evidence",
                BuildNarrativeList(materialEvidence.Select(BuildEvidenceNarrative)));

            AppendAnnexSection(sb, "14. Details of circumstantial evidence",
                BuildNarrativeList(circumstantialEvidence.Select(BuildEvidenceNarrative)));

            AppendAnnexSection(sb, "15. Details of findings with implications / violated policy references",
                BuildNarrativeList(frRows.Select(x => JoinNonEmpty(" ",
                    BuildPair("Accusation", x.AccusationText),
                    BuildPair("Finding", x.FindingText),
                    BuildPair("Policy Reference", FindViolationReferenceForAccusation(data, x))))));

            AppendAnnexSection(sb, "16. Details of clear recommendations",
                BuildNarrativeList(frRows.Select(x => JoinNonEmpty(" ", BuildPair("Accusation", x.AccusationText), BuildPair("Recommendation", x.RecommendationText), BuildPair("Outcome", x.Outcome)))));

            AppendAnnexSection(sb, "17. Whether reported in latest audit report",
                BuildParagraphs("No explicit audit report reference was provided in the available IID inquiry dataset."));

            AppendAnnexSection(sb, "18. Root cause of incident",
                BuildParagraphs(data.FinalConclusion?.Gist));

            AppendAnnexSection(sb, "19. Name with PP.No. of accused against whom DSAs framed",
                BuildNarrativeList(data.DsaFiles.OrderBy(x => x.SortOrder).Select(x => JoinNonEmpty(" - ", x.PersonName, "PP No: " + x.PpnoNumber, x.Designation, x.DsaStatus, x.Remarks))));

            AppendAnnexSection(sb, "20. Summary of violations statement",
                BuildParagraphs(data.FinalConclusion?.Findings, data.FinalConclusion?.Recommendation, data.FinalConclusion?.FinalOutcome));

            AppendSignatureBlock(sb);
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexThree(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<div class='page-break'>&nbsp;</div>");
            sb.AppendLine("<section class='annex-page'>");
            sb.AppendLine("<div class='annex-title'>Annex-III</div>");
            sb.AppendLine("<div class='annex-subtitle'>Violation Summary and Recommendations</div>");

            if (!data.Violations.Any())
                {
                sb.AppendLine("<div class='muted'>No violation records available.</div>");
                sb.AppendLine("</section>");
                return;
                }

            sb.AppendLine("<table class='violation-table'>");
            sb.AppendLine("<colgroup><col style='width:18%'/><col style='width:34%'/><col style='width:24%'/><col style='width:24%'/></colgroup>");
            sb.AppendLine("<thead><tr><th>Category</th><th>Violation Detail</th><th>Reference</th><th>Recommendation</th></tr></thead><tbody>");
            foreach (var row in data.Violations.OrderBy(x => x.SortOrder))
                {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                    ToParagraphs(row.Category),
                    ToParagraphs(row.ViolationDetail),
                    ToParagraphs(row.ReferenceText),
                    ToParagraphs(row.Recommendation));
                }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexSection(StringBuilder sb, string title, string bodyHtml)
            {
            sb.AppendLine("<div class='annex-section'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='annex-label'>{0}</div>", Encode(title)).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='annex-body'>{0}</div>", bodyHtml).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='section-separator'>{0}</div>", AnnexSeparator).AppendLine();
            sb.AppendLine("</div>");
            }

        private static void AppendSignatureBlock(StringBuilder sb)
            {
            sb.AppendLine("<div class='signature-wrap'>");
            sb.AppendLine("<div class='signature-block'><div class='signature-line'>Team Member</div></div>");
            sb.AppendLine("<div class='signature-block'><div class='signature-line'>Team Leader / Head</div></div>");
            sb.AppendLine("</div>");
            }

        private static string BuildParagraphs(params string[] values)
            {
            var items = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (!items.Any())
                {
                return "<div class='annex-paragraph'>N/A</div>";
                }

            return string.Join(string.Empty, items.Select(x => $"<div class='annex-paragraph'>{ToParagraphs(x)}</div>"));
            }

        private static string BuildOrderedList(IEnumerable<string> values)
            {
            var items = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (!items.Any())
                {
                return "<div class='annex-paragraph'>N/A</div>";
                }

            var sb = new StringBuilder("<ol>");
            foreach (var item in items)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<li>{0}</li>", ToParagraphs(item));
                }

            sb.Append("</ol>");
            return sb.ToString();
            }

        private static string BuildNarrativeList(IEnumerable<string> values)
            {
            return BuildOrderedList(values);
            }

        private static string BuildAccusedNarrative(IidAccusedRowModel row)
            {
            if (row == null)
                {
                return null;
                }

            return JoinNonEmpty("; ",
                BuildPair("Name", row.PersonName),
                BuildPair("Father Name", row.FatherName),
                BuildPair("Designation", row.Designation),
                BuildPair("Role", row.RoleType),
                BuildPair("PP No", row.PpnoNumber),
                BuildPair("CNIC", row.Cnic),
                BuildPair("Posting Place", row.PostingPlace));
            }

        private static string BuildStatementTimeline(IidStatementRowModel row)
            {
            if (row == null)
                {
                return null;
                }

            return JoinNonEmpty("; ",
                BuildPair("Person", row.PersonName),
                BuildPair("Role", row.RoleType),
                BuildPair("Date/Time", FormatDate(row.StatementDatetime)),
                BuildPair("Place", row.Place),
                BuildPair("Mode", row.ModeType));
            }

        private static string BuildEvidenceNarrative(IidEvidenceFileRowModel row)
            {
            if (row == null)
                {
                return null;
                }

            return JoinNonEmpty("; ",
                BuildPair("Type", row.EvidenceType),
                BuildPair("Description", row.Description),
                BuildPair("File", row.FileName),
                BuildPair("Uploaded On", FormatDate(row.UploadedOn)));
            }

        private static string FindViolationReferenceForAccusation(IidInquiryReportPdfData data, IidFindingRecommendationRowModel row)
            {
            var match = data.Violations
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.ViolationDetail)
                    && !string.IsNullOrWhiteSpace(row.AccusationText)
                    && v.ViolationDetail.IndexOf(row.AccusationText, StringComparison.OrdinalIgnoreCase) >= 0);

            return match?.ReferenceText;
            }

        private static bool IsComplainantRole(string roleType)
            {
            var value = (roleType ?? string.Empty).Trim().ToLowerInvariant();
            return value.Contains("complain");
            }

        private static bool IsCoAccused(string roleType)
            {
            var value = (roleType ?? string.Empty).Trim().ToLowerInvariant();
            return value.Contains("co") && value.Contains("accused");
            }

        private static string BuildPair(string label, string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            return $"<strong>{Encode(label)}:</strong> {ToParagraphs(value)}";
            }

        private static string JoinNonEmpty(string separator, params string[] values)
            {
            var available = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return available.Any() ? string.Join(separator, available) : null;
            }

        private static string Encode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

        private static string EncodeOrDefault(string value, string fallback = "N/A")
        {
            var selected = string.IsNullOrWhiteSpace(value) ? fallback : value;
            return Encode(selected);
        }

        private static string ToParagraphs(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return "N/A";
                }

            return Encode(value).Replace("\r\n", "\n").Replace("\n", "<br/>");
            }

        private static string FormatDate(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : "N/A";
            }

        private static string FormatDate(DateTime value)
            {
            return value == default ? "N/A" : value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
            }
        }
    }

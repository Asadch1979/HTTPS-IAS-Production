using AIS.Models.IID;
using Ganss.Xss;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AIS.Services
    {
    public class IidInquiryReportPdfBuilder
        {
        private static readonly Regex HtmlTagPattern = new Regex(@"<\s*/?\s*[a-zA-Z][^>]*>|&lt;\s*/?\s*[a-zA-Z][^&]*&gt;", RegexOptions.Compiled);

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
            sb.AppendLine(".cover{ text-align:center; padding-top:62mm; padding-bottom:36mm; page-break-after:always; break-after:page; }");
            sb.AppendLine(".cover-bank{ font-size:20px; font-weight:700; letter-spacing:.4px; text-transform:uppercase; }");
            sb.AppendLine(".cover-dept{ font-size:15px; padding-top:10px; font-weight:600; text-transform:uppercase; }");
            sb.AppendLine(".cover-title{ font-size:30px; padding-top:44px; font-weight:700; letter-spacing:1.5px; text-transform:uppercase; }");
            sb.AppendLine(".cover-status{ font-size:20px; padding-top:14px; font-weight:700; letter-spacing:1px; text-transform:uppercase; }");
            sb.AppendLine(".cover-ref{ padding-top:30px; font-size:13px; }");
            sb.AppendLine(".cover-conducted{ padding-top:36px; font-size:13px; line-height:1.8; }");
            sb.AppendLine(".cover-meta{ padding-top:42mm; font-size:10px; color:#555; }");
            sb.AppendLine(".annex-page{ padding-top:2px; }");
            sb.AppendLine(".annex-page.page-break-after{ page-break-after:always; break-after:page; }");
            sb.AppendLine(".annex-title{ text-align:center; font-weight:700; font-size:18px; padding-bottom:3px; }");
            sb.AppendLine(".annex-subtitle{ text-align:center; font-size:12px; padding-bottom:20px; }");
            sb.AppendLine(".annex-section{ padding-bottom:12px; }");
            sb.AppendLine(".annex-label{ font-size:12.5px; font-weight:700; padding-bottom:6px; }");
            sb.AppendLine(".annex-body{ padding-left:14px; text-align:justify; }");
            sb.AppendLine(".annex-paragraph{ padding-bottom:6px; }");
            sb.AppendLine(".annex-body p{ margin:0 0 6px; }");
            sb.AppendLine(".annex-body ul, .annex-body ol{ padding-left:18px; }");
            sb.AppendLine(".annex-body li{ padding-bottom:4px; }");
            sb.AppendLine(".record-block{ margin:0 0 10px; page-break-inside:avoid; }");
            sb.AppendLine(".record-number{ font-weight:700; margin:0 0 4px; }");
            sb.AppendLine(".record-table{ width:100%; border-collapse:collapse; table-layout:fixed; margin:0 0 2px; }");
            sb.AppendLine(".record-table td{ border:1px solid #666; padding:7px; vertical-align:top; word-wrap:break-word; overflow-wrap:anywhere; }");
            sb.AppendLine(".record-label{ width:29%; font-weight:700; background:#f2f2f2; }");
            sb.AppendLine(".summary-table{ width:100%; border-collapse:collapse; table-layout:fixed; margin:0 0 2px; font-size:11px; line-height:1.3; }");
            sb.AppendLine(".summary-table thead{ display:table-header-group; }");
            sb.AppendLine(".summary-table tr{ page-break-inside:avoid; break-inside:avoid; }");
            sb.AppendLine(".summary-table th,.summary-table td{ border:1px solid #666; padding:4px 6px; vertical-align:top; text-align:left; word-wrap:break-word; overflow-wrap:anywhere; }");
            sb.AppendLine(".summary-table th{ background:#f2f2f2; font-weight:700; }");
            sb.AppendLine(".summary-table p{ margin:0 0 4px; }");
            sb.AppendLine(".summary-table ul,.summary-table ol{ margin:0; padding-left:18px; }");
            sb.AppendLine(".compact-record{ margin:0 0 8px; }");
            sb.AppendLine(".compact-line{ margin:0 0 4px; }");
            sb.AppendLine(".summary-block{ margin:0 0 10px; }");
            sb.AppendLine(".summary-heading{ font-weight:700; text-decoration:underline; margin:0 0 3px; }");
            sb.AppendLine(".summary-text{ margin:0 0 6px; }");
            sb.AppendLine(".signature-wrap{ width:100%; padding-top:26px; border-collapse:collapse; }");
            sb.AppendLine(".signature-wrap td{ width:50%; text-align:center; vertical-align:top; padding:0 12px; }");
            sb.AppendLine(".signature-slot{ padding-top:42px; }");
            sb.AppendLine(".signature-line{ border-top:1px solid #111; padding-top:10px; min-height:56px; }");
            sb.AppendLine(".signature-name{ font-weight:700; }");
            sb.AppendLine(".signature-role{ padding-top:4px; font-size:11px; color:#444; }");
            sb.AppendLine(".violation-table{ width:100%; border-collapse:collapse; table-layout:fixed; }");
            sb.AppendLine(".violation-table thead{ display:table-header-group; }");
            sb.AppendLine(".violation-table tr{ page-break-inside:avoid; break-inside:avoid; }");
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
            if (HasMeaningfulValue(header.InquiryStatus))
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-status'>{0}</div>", Encode(header.InquiryStatus)).AppendLine();
                }
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-ref'><div><strong>Inquiry / Complaint Reference:</strong> {0}</div><div><strong>Branch / Region Context:</strong> {1}</div><div><strong>Inquiry Status:</strong> {2}</div></div>",
                EncodeOrDefault(header.ComplaintNo ?? snapshot.ComplaintNo),
                EncodeOrDefault(JoinNonEmpty(" / ", snapshot.Branch, snapshot.Region), "N/A"),
                EncodeOrDefault(header.InquiryStatus)).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-conducted'><div><strong>Conducted by</strong></div><div>{0}</div></div>",
                EncodeOrDefault(header.InspectionUnit, "Inspection Unit")).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='cover-meta'>Generated on {0} &nbsp;|&nbsp; Confidential - Internal Use Only</div>",
                Encode(FormatDate(header.GeneratedOn))).AppendLine();
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexTwo(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='annex-page page-break-after'>");
            sb.AppendLine("<div class='annex-title'>Annex-II</div>");
            sb.AppendLine("<div class='annex-subtitle'>Inquiry Narrative and Proceedings</div>");

            var snapshot = data.ComplaintSnapshot ?? new IidComplaintSnapshotModel();
            var accusations = data.Accusations
                .Where(x => IsVisibleAccusationText(x.AccusationText))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.AccusationId)
                .ToList();
            var proceedings = data.InquiryProceedings.OrderBy(x => x.SortOrder).ThenBy(x => x.VisitDate).ToList();
            var mainAccused = data.AccusedList.Where(x => !IsCoAccused(x.RoleType)).ToList();
            var coAccused = data.AccusedList.Where(x => IsCoAccused(x.RoleType)).ToList();
            var complainantStatements = data.Statements.Where(x => IsComplainantRole(x.RoleType)).OrderBy(x => x.StatementDatetime).ToList();
            var accusedStatements = data.Statements.Where(x => !IsComplainantRole(x.RoleType)).OrderBy(x => x.StatementDatetime).ToList();
            var materialEvidence = data.EvidenceFiles.Where(x => (x.EvidenceType ?? string.Empty).ToLowerInvariant().Contains("material")).ToList();
            var circumstantialEvidence = data.EvidenceFiles.Where(x => !(x.EvidenceType ?? string.Empty).ToLowerInvariant().Contains("material")).ToList();
            var frRows = data.FindingsRecommendations
                .Where(x => x.AccusationId != 0 && IsVisibleAccusationText(x.AccusationText))
                .ToList();

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
                BuildSummaryTable(
                    mainAccused,
                    new[] { "Name", "Father Name", "Designation", "Role", "PP No." },
                    new Func<IidAccusedRowModel, string>[]
                    {
                        x => x.PersonName,
                        x => x.FatherName,
                        x => x.Designation,
                        x => x.RoleType,
                        x => x.PpnoNumber
                    },
                    new[] { "22%", "22%", "22%", "18%", "16%" }));

            AppendAnnexSection(sb, "6. Alleged Co-accused Details",
                BuildRecordTables(coAccused, BuildAccusedFields, true));

            AppendAnnexSection(sb, "7. Inquiry Proceedings",
                BuildProceedingsNarrative(proceedings)
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "8. Details of Record Scrutinized",
                BuildRecordsScrutinizedTable(data.RecordsScrutinized.OrderBy(x => x.SortOrder))
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "9. Statement of Complainant",
                BuildSummaryTable(
                    complainantStatements,
                    new[] { "Name", "Role", "Date", "Place", "Mode" },
                    new Func<IidStatementRowModel, string>[]
                    {
                        x => x.PersonName,
                        x => x.RoleType,
                        x => x.StatementDatetime.HasValue ? FormatDate(x.StatementDatetime) : null,
                        x => x.Place,
                        x => x.ModeType
                    },
                    new[] { "22%", "18%", "20%", "24%", "16%" })
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "10. Statement of Accused",
                BuildSummaryTable(
                    accusedStatements,
                    new[] { "Name", "Role", "Date", "Place", "Mode" },
                    new Func<IidStatementRowModel, string>[]
                    {
                        x => x.PersonName,
                        x => x.RoleType,
                        x => x.StatementDatetime.HasValue ? FormatDate(x.StatementDatetime) : null,
                        x => x.Place,
                        x => x.ModeType
                    },
                    new[] { "22%", "18%", "20%", "24%", "16%" })
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "11. Critical Points of Complainant",
                BuildUnorderedList(
                    complainantStatements
                        .Where(x => HasMeaningfulValue(x.KeyPoints))
                        .Select(x => JoinNonEmpty(": ", x.PersonName, x.KeyPoints)))
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "12. Critical Points of Accused/Co-accused/Witnesses",
                BuildMatrixTable(
                    new[] { "Sr. No.", "Name of accused/Co-accused/Witness", "Critical points observed" },
                    accusedStatements
                        .Where(x => HasMeaningfulValue(x.KeyPoints))
                        .Select((x, index) => (IReadOnlyList<string>)new[]
                            {
                            (index + 1).ToString(CultureInfo.InvariantCulture),
                            JoinNonEmpty(" / ", x.PersonName, x.RoleType),
                            x.KeyPoints
                            }),
                    new[] { "10%", "30%", "60%" })
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "13. Material Evidences",
                !string.IsNullOrWhiteSpace(data.EvidenceSummary?.MaterialEvidenceDetail)
                    ? BuildParagraphs(data.EvidenceSummary.MaterialEvidenceDetail)
                    : BuildNarrativeFieldBlocks(materialEvidence, (row, index) => $"Material Evidence {index}", BuildEvidenceFields)
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "14. Circumstantial Evidences",
                !string.IsNullOrWhiteSpace(data.EvidenceSummary?.CircumstantialEvidenceDetail)
                    ? BuildParagraphs(data.EvidenceSummary.CircumstantialEvidenceDetail)
                    : BuildNarrativeFieldBlocks(circumstantialEvidence, (row, index) => $"Circumstantial Evidence {index}", BuildEvidenceFields)
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "15. Findings with implications",
                BuildFindingsOnlyBlocks(frRows)
                ?? BuildParagraphs(StripLeadingCaption(data.FinalConclusion?.Findings, "Finding with implications", "Findings with implications"))
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "16. Clear Recommendations",
                BuildRecommendationsOnlyBlocks(frRows)
                ?? BuildParagraphs(StripLeadingCaption(data.FinalConclusion?.Recommendation, "Clear recommendations", "Details of clear recommendations"))
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "17. Conclusion",
                BuildParagraphs(data.FinalConclusion?.Conclusion)
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "18. Whether reported in audit report",
                BuildAuditReportSection(data.FinalConclusion)
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "19. Root Cause",
                BuildParagraphs(data.FinalConclusion?.RootCause)
                ?? BuildParagraphs("Nil"));

            AppendAnnexSection(sb, "20. DSA section",
                BuildMatrixTable(
                    new[] { "PP.No.", "Name & designation of accused", "Allegations", "Reference of policy / SOP / circular violation" },
                    data.DsaFiles
                        .OrderBy(x => x.SortOrder)
                        .Select(x => (IReadOnlyList<string>)new[]
                            {
                            x.PpnoNumber,
                            JoinNonEmpty(" / ", x.PersonName, x.Designation),
                            HasMeaningfulValue(x.Remarks) ? x.Remarks : "Nil",
                            "Nil"
                            }),
                    new[] { "16%", "28%", "28%", "28%" })
                ?? BuildParagraphs("Nil"));

            AppendSignatureBlock(sb, data);
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexThree(StringBuilder sb, IidInquiryReportPdfData data)
            {
            var violationRows = (data?.Violations ?? new List<IidViolationRowModel>())
                .Where(HasMeaningfulViolationRow)
                .OrderBy(x => x.SortOrder)
                .ToList();

            sb.AppendLine("<section class='annex-page'>");
            sb.AppendLine("<div class='annex-title'>Annex-III</div>");
            sb.AppendLine("<div class='annex-subtitle'>Summary of Violations Statement</div>");
            sb.AppendLine("<div class='annex-section'>");
            sb.AppendLine("<div class='annex-label'>21. Summary of violations statement</div>");
            sb.AppendLine("<div class='annex-body'>");
            sb.AppendLine("<table class='violation-table'>");
            sb.AppendLine("<colgroup><col style='width:18%'/><col style='width:34%'/><col style='width:24%'/><col style='width:24%'/></colgroup>");
            sb.AppendLine("<thead><tr><th>Category</th><th>Violation Detail</th><th>Reference</th><th>Recommendation</th></tr></thead><tbody>");

            if (violationRows.Any())
                {
                foreach (var row in violationRows)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                        ToTableCellContent(row.Category),
                        ToTableCellContent(row.ViolationDetail),
                        ToTableCellContent(row.ReferenceText),
                        ToTableCellContent(row.Recommendation));
                    }
                }
            else
                {
                sb.AppendLine("<tr><td colspan='4'><div class='summary-block'><div class='summary-heading'>No violations observed</div><div class='summary-text'>No internal, procedural, policy, or control violation was recorded in the Annex-III summary for this inquiry.</div></div></td></tr>");
                }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</section>");
            }

        private static void AppendAnnexSection(StringBuilder sb, string title, string bodyHtml)
            {
            if (string.IsNullOrWhiteSpace(bodyHtml))
                {
                return;
                }

            sb.AppendLine("<div class='annex-section'>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='annex-label'>{0}</div>", Encode(title)).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='annex-body'>{0}</div>", bodyHtml).AppendLine();
            sb.AppendLine("</div>");
            }

        private static void AppendSignatureBlock(StringBuilder sb, IidInquiryReportPdfData data)
            {
            var header = data?.Header ?? new IidInquiryHeaderModel();
            sb.AppendLine("<table class='signature-wrap'><tr>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", BuildSignatureCell(header.TeamMembers, "Team Member", true)).AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", BuildSignatureCell(header.TeamLead, "Team Lead", false)).AppendLine();
            sb.AppendLine("</tr></table>");
            }

        private static string BuildSignatureCell(string names, string roleLabel, bool splitOnComma)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "<div class='signature-slot'><div class='signature-line'><div class='signature-name'>{0}</div><div class='signature-role'>{1}</div></div></div>",
                FormatSignatureNames(names, splitOnComma),
                Encode(roleLabel));
        }

        private static string FormatSignatureNames(string value, bool splitOnComma)
        {
            if (string.IsNullOrWhiteSpace(value))
                {
                return "&nbsp;";
                }

            var separators = splitOnComma
                ? new[] { "\r\n", "\n", ";", "," }
                : new[] { "\r\n", "\n", ";" };

            var parts = value
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Encode)
                .ToList();

            if (!parts.Any())
                {
                return Encode(value.Trim());
                }

            return string.Join("<br/>", parts);
        }

        private static string BuildParagraphs(params string[] values)
            {
            var items = values
                .Select(NormalizeNarrativeValue)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            if (!items.Any())
                {
                return null;
                }

            return string.Join(string.Empty, items.Select(x => $"<div class='annex-paragraph'>{ToParagraphs(x)}</div>"));
            }

        private static string BuildOrderedList(IEnumerable<string> values)
            {
            var items = (values ?? Enumerable.Empty<string>())
                .Select(NormalizeNarrativeValue)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            if (!items.Any())
                {
                return null;
                }

            var sb = new StringBuilder("<ol>");
            foreach (var item in items)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<li>{0}</li>", ToParagraphs(item));
                }

            sb.Append("</ol>");
            return sb.ToString();
            }

        private static string BuildUnorderedList(IEnumerable<string> values)
            {
            var items = (values ?? Enumerable.Empty<string>())
                .Select(NormalizeNarrativeValue)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            if (!items.Any())
                {
                return null;
                }

            var sb = new StringBuilder("<ul>");
            foreach (var item in items)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<li>{0}</li>", ToParagraphs(item));
                }

            sb.Append("</ul>");
            return sb.ToString();
            }

        private static string BuildNarrativeList(IEnumerable<string> values)
            {
            return BuildOrderedList(values);
            }

        private static string BuildRecordTables<T>(IEnumerable<T> rows, Func<T, IEnumerable<KeyValuePair<string, string>>> buildFields, bool showNumbersWhenMultiple)
            {
            if (rows == null || buildFields == null)
                {
                return null;
                }

            var fieldSets = new List<List<KeyValuePair<string, string>>>();
            foreach (var row in rows)
                {
                if (row == null)
                    {
                    continue;
                    }

                var fieldList = (buildFields(row) ?? Enumerable.Empty<KeyValuePair<string, string>>()).ToList();
                if (!fieldList.Any() || !fieldList.Any(x => !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(x.Value))))
                    {
                    continue;
                    }

                fieldSets.Add(fieldList);
                }

            if (!fieldSets.Any())
                {
                return null;
                }

            var showNumbers = showNumbersWhenMultiple && fieldSets.Count > 1;
            var blocks = fieldSets
                .Select((fields, index) => BuildRecordTable(index + 1, fields, showNumbers))
                .ToList();

            return blocks.Any()
                ? string.Join(string.Empty, blocks)
                : null;
            }

        private static string BuildCompactFieldBlocks<T>(IEnumerable<T> rows, Func<T, IEnumerable<KeyValuePair<string, string>>> buildFields)
            {
            if (rows == null || buildFields == null)
                {
                return null;
                }

            var blocks = new List<string>();
            foreach (var row in rows)
                {
                if (row == null)
                    {
                    continue;
                    }

                var fields = (buildFields(row) ?? Enumerable.Empty<KeyValuePair<string, string>>())
                    .Where(x => !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(x.Value)))
                    .ToList();

                if (!fields.Any())
                    {
                    continue;
                    }

                blocks.Add(BuildCompactFieldBlock(fields));
                }

            if (!blocks.Any())
                {
                return null;
                }

            if (blocks.Count == 1)
                {
                return blocks[0];
                }

            return string.Join(string.Empty, blocks);
            }

        private static string BuildSummaryTable<T>(IEnumerable<T> rows, IReadOnlyList<string> headers, IReadOnlyList<Func<T, string>> valueSelectors, IReadOnlyList<string> widths = null)
            {
            if (rows == null || headers == null || valueSelectors == null || headers.Count == 0 || headers.Count != valueSelectors.Count)
                {
                return null;
                }

            var rowValues = new List<List<string>>();
            foreach (var row in rows)
                {
                if (row == null)
                    {
                    continue;
                    }

                var values = valueSelectors
                    .Select(selector => selector == null ? null : selector(row))
                    .ToList();

                if (!values.Any(value => !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(value))))
                    {
                    continue;
                    }

                rowValues.Add(values);
                }

            if (!rowValues.Any())
                {
                return null;
                }

            var sb = new StringBuilder();
            sb.AppendLine("<table class='summary-table'>");
            if (widths != null && widths.Count == headers.Count)
                {
                sb.Append("<colgroup>");
                foreach (var width in widths)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<col style='width:{0}'/>", EncodeOrDefault(width, "auto"));
                    }

                sb.AppendLine("</colgroup>");
                }

            sb.AppendLine("<thead><tr>");
            foreach (var header in headers)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", EncodeOrDefault(header));
                }

            sb.AppendLine("</tr></thead><tbody>");
            foreach (var values in rowValues)
                {
                sb.AppendLine("<tr>");
                foreach (var value in values)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", ToTableCellContent(value));
                    }

                sb.AppendLine("</tr>");
                }

            sb.AppendLine("</tbody></table>");
            return sb.ToString();
            }

        private static string BuildProceedingsNarrative(IEnumerable<IidInquiryProceedingRowModel> rows)
            {
            var items = (rows ?? Enumerable.Empty<IidInquiryProceedingRowModel>())
                .Where(row => row != null && (
                    HasMeaningfulValue(row.NoticeReference)
                    || row.VisitDate.HasValue
                    || HasMeaningfulValue(row.PlaceVisited)
                    || HasMeaningfulValue(row.ParticipantsDetail)
                    || HasMeaningfulValue(row.MissingParticipantsReason)))
                .ToList();

            if (!items.Any())
                {
                return null;
                }

            var blocks = items
                .Select(row => BuildCompactFieldBlock(new[]
                    {
                    BuildField("Notice Reference", HasMeaningfulValue(row.NoticeReference) ? row.NoticeReference : "Nil"),
                    BuildField("Visit Date", row.VisitDate.HasValue ? FormatDate(row.VisitDate) : "Nil"),
                    BuildField("Place Visited", HasMeaningfulValue(row.PlaceVisited) ? row.PlaceVisited : "Nil"),
                    BuildField("Participants Detail", HasMeaningfulValue(row.ParticipantsDetail) ? row.ParticipantsDetail : "Nil"),
                    BuildField("Missing Participants Reason", HasMeaningfulValue(row.MissingParticipantsReason) ? row.MissingParticipantsReason : "Nil")
                    }))
                .Where(block => !string.IsNullOrWhiteSpace(block))
                .ToList();

            return blocks.Any() ? string.Join(string.Empty, blocks) : null;
            }

        private static string BuildRecordsScrutinizedTable(IEnumerable<IidRecordScrutinizedRowModel> rows)
            {
            var items = (rows ?? Enumerable.Empty<IidRecordScrutinizedRowModel>())
                .Where(row => row != null && (
                    HasMeaningfulValue(row.RecordTitle)
                    || HasMeaningfulValue(row.RecordDetails)))
                .ToList();

            if (!items.Any())
                {
                return null;
                }

            return BuildMatrixTable(
                new[] { "Sr. No.", "Record Title", "Details" },
                items.Select((row, index) => (IReadOnlyList<string>)new[]
                    {
                    (index + 1).ToString(CultureInfo.InvariantCulture),
                    row.RecordTitle,
                    row.RecordDetails
                    }),
                new[] { "10%", "30%", "60%" });
            }

        private static string BuildMatrixTable(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows, IReadOnlyList<string> widths = null)
            {
            if (headers == null || headers.Count == 0 || rows == null)
                {
                return null;
                }

            var normalizedRows = rows
                .Where(row => row != null && row.Count == headers.Count && row.Any(value => HasMeaningfulValue(value)))
                .ToList();

            if (!normalizedRows.Any())
                {
                return null;
                }

            var sb = new StringBuilder();
            sb.AppendLine("<table class='summary-table'>");
            if (widths != null && widths.Count == headers.Count)
                {
                sb.Append("<colgroup>");
                foreach (var width in widths)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<col style='width:{0}'/>", EncodeOrDefault(width, "auto"));
                    }

                sb.AppendLine("</colgroup>");
                }

            sb.AppendLine("<thead><tr>");
            foreach (var header in headers)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<th>{0}</th>", EncodeOrDefault(header));
                }

            sb.AppendLine("</tr></thead><tbody>");
            foreach (var row in normalizedRows)
                {
                sb.AppendLine("<tr>");
                foreach (var value in row)
                    {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<td>{0}</td>", ToTableCellContent(value));
                    }

                sb.AppendLine("</tr>");
                }

            sb.AppendLine("</tbody></table>");
            return sb.ToString();
            }

        private static string BuildNarrativeFieldBlocks<T>(IEnumerable<T> rows, Func<T, int, string> headingSelector, Func<T, IEnumerable<KeyValuePair<string, string>>> buildFields)
            {
            if (rows == null || buildFields == null)
                {
                return null;
                }

            var blocks = new List<string>();
            var index = 0;
            foreach (var row in rows)
                {
                if (row == null)
                    {
                    continue;
                    }

                var fields = (buildFields(row) ?? Enumerable.Empty<KeyValuePair<string, string>>())
                    .Where(x => HasMeaningfulValue(x.Value))
                    .ToList();
                if (!fields.Any())
                    {
                    continue;
                    }

                index++;
                var heading = headingSelector?.Invoke(row, index);
                var block = new StringBuilder();
                block.Append("<div class='summary-block'>");
                if (HasMeaningfulValue(heading))
                    {
                    block.AppendFormat(CultureInfo.InvariantCulture, "<div class='summary-heading'>{0}</div>", Encode(heading));
                    }

                foreach (var field in fields)
                    {
                    block.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "<div class='summary-text'><strong>{0}:</strong> {1}</div>",
                        EncodeOrDefault(field.Key, "N/A"),
                        ToParagraphs(field.Value));
                    }

                block.Append("</div>");
                blocks.Add(block.ToString());
                }

            return blocks.Any() ? string.Join(string.Empty, blocks) : null;
            }

        private static string BuildRecordTable(int index, IEnumerable<KeyValuePair<string, string>> fields, bool showNumber)
            {
            var sb = new StringBuilder();
            sb.Append("<div class='record-block'>");
            if (showNumber)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='record-number'>{0}.</div>", index);
                }
            sb.AppendLine("<table class='record-table'><colgroup><col style='width:29%'/><col style='width:71%'/></colgroup><tbody>");
            foreach (var field in fields)
                {
                sb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "<tr><td class='record-label'>{0}</td><td>{1}</td></tr>",
                    EncodeOrDefault(field.Key, "N/A"),
                    ToParagraphs(field.Value));
                }

            sb.AppendLine("</tbody></table></div>");
            return sb.ToString();
            }

        private static string BuildCompactFieldBlock(IEnumerable<KeyValuePair<string, string>> fields)
            {
            var lines = fields
                .Where(x => !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(x.Value)))
                .Select(x => string.Format(
                    CultureInfo.InvariantCulture,
                    "<div class='compact-line'><strong>{0}:</strong> {1}</div>",
                    EncodeOrDefault(x.Key, "N/A"),
                    ToParagraphs(x.Value)))
                .ToList();

            return lines.Any()
                ? $"<div class='compact-record'>{string.Join(string.Empty, lines)}</div>"
                : null;
            }

        private static string BuildKeyPointsBlocks(IEnumerable<string> values)
            {
            var items = (values ?? Enumerable.Empty<string>())
                .Select(NormalizeNarrativeValue)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !string.Equals(x, "N/A", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!items.Any())
                {
                return null;
                }

            if (items.Count == 1)
                {
                return $"<div class='annex-paragraph'>{ToParagraphs(items[0])}</div>";
                }

            return string.Join(string.Empty, items.Select(x => $"<div class='annex-paragraph'>{ToParagraphs(x)}</div>"));
            }

        private static string BuildViolationSummaryBlocks(IEnumerable<IidFindingRecommendationRowModel> rows)
            {
            var items = (rows ?? Enumerable.Empty<IidFindingRecommendationRowModel>())
                .Where(x => x != null && IsVisibleAccusationText(x.AccusationText))
                .ToList();

            if (!items.Any())
                {
                return null;
                }

            var blocks = new List<string>();
            foreach (var row in items)
                {
                blocks.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "<div class='summary-block'><div class='summary-heading'>Allegation:</div><div class='summary-text'>{0}</div><div class='summary-heading'>Finding:</div><div class='summary-text'>{1}</div><div class='summary-heading'>Outcome:</div><div class='summary-text'>{2}</div></div>",
                    ToParagraphs(row.AccusationText),
                    ToParagraphs(row.FindingText),
                    ToParagraphs(row.Outcome)));
                }

            return string.Join(string.Empty, blocks);
            }

        private static string BuildFindingsRecommendationSummary(IidInquiryReportPdfData data, IEnumerable<IidFindingRecommendationRowModel> rows)
            {
            var items = (rows ?? Enumerable.Empty<IidFindingRecommendationRowModel>())
                .Where(x => x != null && (
                    IsVisibleAccusationText(x.AccusationText)
                    || !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(x.FindingText))
                    || !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(x.RecommendationText))
                    || !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(x.Outcome))))
                .ToList();

            var detailsHtml = BuildRecordTables(items, BuildFindingRecommendationSummaryFields, true);
            if (!string.IsNullOrWhiteSpace(detailsHtml))
                {
                return detailsHtml;
                }

            var fallbackFields = BuildFindingRecommendationFallbackFields(data)
                .Where(x => HasMeaningfulValue(x.Value))
                .ToList();

            return fallbackFields.Any()
                ? BuildRecordTable(1, fallbackFields, false)
                : null;
            }

        private static string BuildProceedingHeading(IidInquiryProceedingRowModel row, int index)
            {
            return JoinNonEmpty(" | ",
                $"Proceeding {index}",
                row?.VisitDate.HasValue == true ? FormatDate(row.VisitDate) : null,
                row?.NoticeReference);
            }

        private static string BuildRecordScrutinizedHeading(IidRecordScrutinizedRowModel row, int index)
            {
            return HasMeaningfulValue(row?.RecordTitle) ? row.RecordTitle : $"Record {index}";
            }

        private static string BuildFindingHeading(IidFindingRecommendationRowModel row, int index)
            {
            return IsVisibleAccusationText(row?.AccusationText) ? row.AccusationText : $"Finding {index}";
            }

        private static string BuildRecommendationHeading(IidFindingRecommendationRowModel row, int index)
            {
            return IsVisibleAccusationText(row?.AccusationText) ? row.AccusationText : $"Recommendation {index}";
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildAccusedFields(IidAccusedRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Name", row.PersonName);
            yield return BuildField("Father Name", row.FatherName);
            yield return BuildField("Designation", row.Designation);
            yield return BuildField("Role", row.RoleType);
            yield return BuildField("PP No", row.PpnoNumber);
            yield return BuildField("CNIC", row.Cnic);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildProceedingFields(IidInquiryProceedingRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Notice Reference", row.NoticeReference);
            yield return BuildField("Visit Date", row.VisitDate.HasValue ? FormatDate(row.VisitDate) : null);
            yield return BuildField("Place Visited", row.PlaceVisited);
            yield return BuildField("Participants Detail", row.ParticipantsDetail);
            yield return BuildField("Missing Participants Reason", row.MissingParticipantsReason);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildRecordScrutinizedFields(IidRecordScrutinizedRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Record Title", row.RecordTitle);
            yield return BuildField("Record Details", row.RecordDetails);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildStatementTimelineFields(IidStatementRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Name", row.PersonName);
            yield return BuildField("Role", row.RoleType);
            yield return BuildField("Date / Time", row.StatementDatetime.HasValue ? FormatDate(row.StatementDatetime) : null);
            yield return BuildField("Place", row.Place);
            yield return BuildField("Mode", row.ModeType);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildEvidenceFields(IidEvidenceFileRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Type", row.EvidenceType);
            yield return BuildField("Description", row.Description);
            yield return BuildField("File", row.FileName);
            yield return BuildField("Uploaded On", row.UploadedOn.HasValue ? FormatDate(row.UploadedOn) : null);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildFindingFields(IidInquiryReportPdfData data, IidFindingRecommendationRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Accusation", row.AccusationText);
            yield return BuildField("Finding", row.FindingText);
            yield return BuildField("Policy Reference", FindViolationReferenceForAccusation(data, row));
            yield return BuildField("Last Updated On", row.UpdatedOn.HasValue ? FormatDate(row.UpdatedOn) : null);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildRecommendationFields(IidFindingRecommendationRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Accusation", row.AccusationText);
            yield return BuildField("Recommendation", row.RecommendationText);
            yield return BuildField("Outcome", row.Outcome);
            yield return BuildField("Last Updated On", row.UpdatedOn.HasValue ? FormatDate(row.UpdatedOn) : null);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildFindingNarrativeFields(IidInquiryReportPdfData data, IidFindingRecommendationRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Finding with implications", row.FindingText);
            yield return BuildField("Reference of policy / SOP / circular violation", FindViolationReferenceForAccusation(data, row));
            yield return BuildField("Outcome", row.Outcome);
            yield return BuildField("Last updated on", row.UpdatedOn.HasValue ? FormatDate(row.UpdatedOn) : null);
            }

        private static string BuildFindingsOnlyBlocks(IEnumerable<IidFindingRecommendationRowModel> rows)
            {
            var items = (rows ?? Enumerable.Empty<IidFindingRecommendationRowModel>())
                .Where(row => row != null && HasMeaningfulValue(row.FindingText))
                .Select(row => StripLeadingCaption(row.FindingText, "Finding with implications", "Findings with implications"))
                .Where(HasMeaningfulValue)
                .ToList();

            return BuildRomanOrderedList(items);
            }

        private static string BuildRecommendationsOnlyBlocks(IEnumerable<IidFindingRecommendationRowModel> rows)
            {
            var items = (rows ?? Enumerable.Empty<IidFindingRecommendationRowModel>())
                .Where(row => row != null && HasMeaningfulValue(row.RecommendationText))
                .Select(row => StripLeadingCaption(row.RecommendationText, "Clear recommendations", "Details of clear recommendations"))
                .Where(HasMeaningfulValue)
                .ToList();

            return BuildRomanOrderedList(items);
            }

        private static string BuildRomanOrderedList(IEnumerable<string> values)
            {
            var items = (values ?? Enumerable.Empty<string>())
                .Select(NormalizeNarrativeValue)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            if (!items.Any())
                {
                return null;
                }

            var sb = new StringBuilder("<ol style='list-style-type:lower-roman;'>");
            foreach (var item in items)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<li>{0}</li>", ToParagraphs(item));
                }

            sb.Append("</ol>");
            return sb.ToString();
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildRecommendationNarrativeFields(IidFindingRecommendationRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Clear recommendations", row.RecommendationText);
            yield return BuildField("Outcome", row.Outcome);
            yield return BuildField("Last updated on", row.UpdatedOn.HasValue ? FormatDate(row.UpdatedOn) : null);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildFindingRecommendationSummaryFields(IidFindingRecommendationRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Allegation", IsVisibleAccusationText(row.AccusationText) ? row.AccusationText : null);
            yield return BuildField("Finding", row.FindingText);
            yield return BuildField("Outcome", row.Outcome);
            yield return BuildField("Recommendations", row.RecommendationText);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildFindingRecommendationFallbackFields(IidInquiryReportPdfData data)
            {
            var firstAccusation = data?.Accusations?
                .Where(x => IsVisibleAccusationText(x.AccusationText))
                .OrderBy(x => x.SortOrder)
                .Select(x => x.AccusationText)
                .FirstOrDefault();

            yield return BuildField("Allegation", firstAccusation);
            yield return BuildField("Finding", data?.FinalConclusion?.Findings);
            yield return BuildField("Outcome", data?.FinalConclusion?.FinalOutcome);
            yield return BuildField("Recommendations", data?.FinalConclusion?.Recommendation);
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildDsaFields(IidDsaRowModel row)
            {
            if (row == null)
                {
                yield break;
                }

            yield return BuildField("Name", row.PersonName);
            yield return BuildField("Designation", row.Designation);
            yield return BuildField("PP No", row.PpnoNumber);
            yield return BuildField("CNIC", row.Cnic);
            yield return BuildField("DSA Status", row.DsaStatus);
            yield return BuildField("Remarks", row.Remarks);
            }

        private static string BuildAuditReportSection(IidFinalConclusionModel model)
            {
            if (model == null)
                {
                return null;
                }

            var reportedInAuditReport = NormalizeYesNoValue(model.ReportedInAuditReport);
            if (string.Equals(reportedInAuditReport, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                return BuildParagraphs("Yes", model.AuditReportReferenceDetail);
                }

            if (string.Equals(reportedInAuditReport, "No", StringComparison.OrdinalIgnoreCase))
                {
                return BuildParagraphs("No");
                }

            return BuildParagraphs("Nil");
            }

        private static string StripLeadingCaption(string value, params string[] captions)
            {
            var normalized = NormalizeNarrativeValue(value);
            if (string.IsNullOrWhiteSpace(normalized) || captions == null || captions.Length == 0)
                {
                return normalized;
                }

            var result = normalized;
            foreach (var caption in captions.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                result = Regex.Replace(
                    result,
                    @"^\s*" + Regex.Escape(caption) + @"\s*[:\-]?\s*",
                    string.Empty,
                    RegexOptions.IgnoreCase);
                }

            return result;
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

        private static bool IsVisibleAccusationText(string value)
            {
            var normalized = NormalizeNarrativeValue(value);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return false;
                }

            return !string.Equals(normalized.Trim(), "Additional Charges", StringComparison.OrdinalIgnoreCase);
            }

        private static string BuildPair(string label, string value)
            {
            var normalized = NormalizeNarrativeValue(value);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return null;
                }

            return $"<strong>{Encode(label)}:</strong> {ToParagraphs(normalized)}";
            }

        private static KeyValuePair<string, string> BuildField(string label, string value)
        {
            return new KeyValuePair<string, string>(label, value);
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

        private static string ToTableCellContent(string value)
            {
            var normalized = NormalizeNarrativeValue(value);
            return string.IsNullOrWhiteSpace(normalized) ? "&nbsp;" : ToParagraphs(normalized);
            }

        private static string ToParagraphs(string value)
            {
            var normalized = NormalizeNarrativeValue(value);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return "N/A";
                }

            if (ContainsHtmlMarkup(normalized))
                {
                var sanitized = CreateNarrativeSanitizer().Sanitize(WebUtility.HtmlDecode(normalized));
                return string.IsNullOrWhiteSpace(sanitized) ? "N/A" : sanitized;
                }

            return Encode(normalized).Replace("\n", "<br/>");
            }

        private static string NormalizeNarrativeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                {
                return null;
                }

            var normalized = value.Replace("\r\n", "\n").Trim();
            if (LooksLikeSqlDebugText(normalized) || IsNilLikeText(normalized))
                {
                return null;
                }

            return normalized;
        }

        private static string NormalizeYesNoValue(string value)
            {
            var normalized = NormalizeNarrativeValue(value);
            if (string.IsNullOrWhiteSpace(normalized))
                {
                return null;
                }

            if (normalized.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                return "Yes";
                }

            if (normalized.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                return "No";
                }

            return normalized;
            }

        private static bool HasMeaningfulValue(string value)
            {
            return !string.IsNullOrWhiteSpace(NormalizeNarrativeValue(value));
            }

        private static bool HasMeaningfulViolationRow(IidViolationRowModel row)
            {
            return row != null
                && (HasMeaningfulValue(row.Category)
                    || HasMeaningfulValue(row.ViolationDetail)
                    || HasMeaningfulValue(row.ReferenceText)
                    || HasMeaningfulValue(row.Recommendation));
            }

        private static bool LooksLikeSqlDebugText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                {
                return false;
                }

            var normalized = value.Trim().ToLowerInvariant();
            return normalized.StartsWith("select ", StringComparison.Ordinal)
                && (normalized.Contains(" from t_au_iid_inq_statements ")
                    || normalized.Contains(" t.rowid ")
                    || normalized.Contains("pkg_inq.")
                    || normalized.Contains(" where "));
        }

        private static bool IsNilLikeText(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return true;
                }

            var plainText = HtmlTagPattern.Replace(WebUtility.HtmlDecode(value), " ");
            plainText = Regex.Replace(plainText, @"\s+", " ").Trim().Trim('.', ':', ';');
            if (string.IsNullOrWhiteSpace(plainText))
                {
                return true;
                }

            return plainText.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                || plainText.Equals("NA", StringComparison.OrdinalIgnoreCase)
                || plainText.Equals("NIL", StringComparison.OrdinalIgnoreCase)
                || plainText.Equals("NONE", StringComparison.OrdinalIgnoreCase)
                || plainText.Equals("NOT APPLICABLE", StringComparison.OrdinalIgnoreCase)
                || plainText.Equals("-", StringComparison.OrdinalIgnoreCase)
                || plainText.Equals("--", StringComparison.OrdinalIgnoreCase);
            }

        private static bool ContainsHtmlMarkup(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && HtmlTagPattern.IsMatch(value);
        }

        private static HtmlSanitizer CreateNarrativeSanitizer()
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.UnionWith(new[]
            {
                "b", "blockquote", "br", "code", "div", "em", "i", "li", "ol", "p", "pre",
                "span", "strong", "sub", "sup", "table", "tbody", "td", "tfoot", "th",
                "thead", "tr", "u", "ul"
            });
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.UnionWith(new[] { "colspan", "rowspan", "class" });
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedClasses.Clear();
            return sanitizer;
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

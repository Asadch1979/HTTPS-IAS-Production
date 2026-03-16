using AIS.Models.IID;
using System;
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
            sb.AppendLine("body{ font-family: Arial, Helvetica, sans-serif; font-size:12px; color:#111; }");
            sb.AppendLine("h1,h2,h3{ margin:0 0 8px; }");
            sb.AppendLine(".page-break{ page-break-before:always; }");
            sb.AppendLine(".section-title{ font-size:14px; font-weight:700; margin:12px 0 6px; padding:6px 10px; border-left:4px solid #111; background:#f6f8fa; }");
            sb.AppendLine(".meta-grid{ width:100%; border-collapse:collapse; margin:8px 0; }");
            sb.AppendLine(".meta-grid td,.meta-grid th{ border:1px solid #111; padding:6px; vertical-align:top; }");
            sb.AppendLine(".label{ width:30%; font-weight:700; background:#fafafa; }");
            sb.AppendLine(".narrative{ margin:6px 0 10px; line-height:1.55; text-align:justify; }");
            sb.AppendLine(".para-box{ border:1px solid #d0d7de; border-radius:8px; padding:10px 12px; margin:8px 0; }");
            sb.AppendLine(".muted{ color:#666; }");
            sb.AppendLine("</style></head><body>");

            AppendCover(sb, data);
            AppendComplaintSnapshot(sb, data);
            AppendComplainantDetails(sb, data);
            AppendAccusedDetails(sb, data);
            AppendGist(sb, data);
            AppendProceedings(sb, data);
            AppendStatements(sb, data, true);
            AppendStatements(sb, data, false);
            AppendRecords(sb, data);
            AppendEvidence(sb, data);
            AppendViolations(sb, data);
            AppendFindingsRecommendations(sb, data);
            AppendAdditionalCharges(sb, data);
            AppendDsa(sb, data);
            AppendFinalConclusion(sb, data);
            AppendSignature(sb, data);

            sb.AppendLine("</body></html>");
            return sb.ToString();
            }

        private static void AppendCover(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section>");
            sb.AppendLine("<h1 style='text-align:center;font-size:20px;'>IID Inquiry Report</h1>");
            sb.AppendLine("<p style='text-align:center;font-size:13px;'>Internal Audit Division - ZTBL</p>");
            sb.AppendLine("<table class='meta-grid'>");
            AppendMetaRow(sb, "Complaint No", data.Header?.ComplaintNo);
            AppendMetaRow(sb, "Inquiry Status", data.Header?.InquiryStatus);
            AppendMetaRow(sb, "Generated On", data.Header?.GeneratedOn.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture));
            sb.AppendLine("</table>");
            sb.AppendLine("<p style='text-align:center;font-weight:700;margin-top:20px;'>Confidential - Internal Use Only</p>");
            sb.AppendLine("</section>");
            }

        private static void AppendComplaintSnapshot(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section class='page-break'><div class='section-title'>1. Complaint Snapshot</div><table class='meta-grid'>");
            AppendMetaRow(sb, "Complaint No", data.ComplaintSnapshot?.ComplaintNo);
            AppendMetaRow(sb, "Nature / Category", $"{data.ComplaintSnapshot?.Nature} / {data.ComplaintSnapshot?.Category}");
            AppendMetaRow(sb, "Submitted On", data.ComplaintSnapshot?.SubmittedOn);
            AppendMetaRow(sb, "Region / Branch", $"{data.ComplaintSnapshot?.Region} / {data.ComplaintSnapshot?.Branch}");
            AppendMetaRow(sb, "Action Required", data.ComplaintSnapshot?.ActionRequired);
            AppendMetaRow(sb, "Complaint Background", data.ComplaintSnapshot?.Contents);
            sb.AppendLine("</table></section>");
            }

        private static void AppendComplainantDetails(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>2. Complainant Details</div><table class='meta-grid'>");
            AppendMetaRow(sb, "Name", data.ComplaintSnapshot?.ComplainantName);
            AppendMetaRow(sb, "CNIC", data.ComplaintSnapshot?.Cnic);
            AppendMetaRow(sb, "Cell No", data.ComplaintSnapshot?.CellularNumber);
            AppendMetaRow(sb, "Gender", data.ComplaintSnapshot?.Gender);
            AppendMetaRow(sb, "Address", data.ComplaintSnapshot?.MailingAddress);
            AppendMetaRow(sb, "Received From", data.ComplaintSnapshot?.ReceivedFrom);
            sb.AppendLine("</table></section>");
            }

        private static void AppendAccusedDetails(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>3. Accused Details</div>");
            if (!data.AccusedList.Any()) { sb.AppendLine("<p class='muted'>No accused detail available.</p></section>"); return; }
            foreach (var row in data.AccusedList)
                {
                sb.AppendLine("<div class='para-box'>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>{0}</strong> ({1})</div>", Encode(row.PersonName), Encode(row.Designation));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div>PP No: {0} | CNIC: {1}</div>", Encode(row.PpnoNumber), Encode(row.Cnic));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div>Role: {0} | Posting: {1}</div>", Encode(row.RoleType), Encode(row.PostingPlace));
                sb.AppendLine("</div>");
                }
            sb.AppendLine("</section>");
            }

        private static void AppendGist(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>4. Gist / Complaint Background</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'>{0}</div>", ToParagraphs(data.FinalConclusion?.Gist ?? data.ComplaintSnapshot?.Contents));
            sb.AppendLine("</section>");
            }

        private static void AppendProceedings(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>5. Proceedings of Inquiry</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'>{0}</div>", ToParagraphs(data.FinalConclusion?.Proceedings));
            sb.AppendLine("</section>");
            }

        private static void AppendStatements(StringBuilder sb, IidInquiryReportPdfData data, bool complainant)
            {
            var rows = data.Statements.Where(x => IsComplainantRole(x.RoleType) == complainant).ToList();
            sb.AppendFormat("<section><div class='section-title'>{0}</div>", complainant ? "6. Statement of Complainant" : "7. Statements of Accused / Others");
            if (!rows.Any()) { sb.AppendLine("<p class='muted'>No statement available.</p></section>"); return; }
            foreach (var row in rows)
                {
                sb.AppendLine("<div class='para-box'>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>{0}</strong> ({1})</div>", Encode(row.PersonName), Encode(row.RoleType));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div>Date/Time: {0} | Place: {1}</div>", FormatDate(row.StatementDatetime), Encode(row.Place));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div>Statement Mode: {0} | Attachment: {1}</div>", Encode(row.ModeType), Encode(row.UploadedStatement));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'>{0}</div>", ToParagraphs(row.KeyPoints));
                sb.AppendLine("</div>");
                }
            sb.AppendLine("</section>");
            }

        private static void AppendRecords(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>8. Record Scrutinized</div>");
            if (!data.RecordsScrutinized.Any()) { sb.AppendLine("<p class='muted'>No records added.</p></section>"); return; }
            foreach (var row in data.RecordsScrutinized)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='para-box'><div><strong>{0}</strong></div><div class='narrative'>{1}</div></div>", Encode(row.RecordTitle), ToParagraphs(row.RecordDetails));
                }
            sb.AppendLine("</section>");
            }

        private static void AppendEvidence(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>9. Evidence Reviewed</div>");
            if (!data.EvidenceFiles.Any()) { sb.AppendLine("<p class='muted'>No evidence file uploaded.</p></section>"); return; }
            sb.AppendLine("<table class='meta-grid'><tr><th>Type</th><th>Description</th><th>File</th><th>Uploaded On</th></tr>");
            foreach (var row in data.EvidenceFiles)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", Encode(row.EvidenceType), Encode(row.Description), Encode(row.FileName), FormatDate(row.UploadedOn));
                }
            sb.AppendLine("</table></section>");
            }

        private static void AppendViolations(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>10. Violations / Rules / Policy References</div>");
            if (!data.Violations.Any()) { sb.AppendLine("<p class='muted'>No violation recorded.</p></section>"); return; }
            foreach (var row in data.Violations)
                {
                sb.AppendLine("<div class='para-box'>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Category:</strong> {0}</div>", Encode(row.Category));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'><strong>Violation:</strong> {0}</div>", ToParagraphs(row.ViolationDetail));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Reference:</strong> {0}</div>", Encode(row.ReferenceText));
                sb.AppendLine("</div>");
                }
            sb.AppendLine("</section>");
            }

        private static void AppendFindingsRecommendations(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>11. Findings & Recommendations (Allegation-wise)</div>");
            var rows = data.FindingsRecommendations.Where(x => x.AccusationId != 0).ToList();
            if (!rows.Any()) { sb.AppendLine("<p class='muted'>No findings/recommendation available.</p></section>"); return; }
            foreach (var row in rows)
                {
                sb.AppendLine("<div class='para-box'>");
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Allegation:</strong> {0}</div>", Encode(row.AccusationText));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'><strong>Finding:</strong> {0}</div>", ToParagraphs(row.FindingText));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'><strong>Recommendation:</strong> {0}</div>", ToParagraphs(row.RecommendationText));
                sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Master Outcome:</strong> {0}</div>", Encode(row.Outcome));
                sb.AppendLine("</div>");
                }
            sb.AppendLine("</section>");
            }

        private static void AppendAdditionalCharges(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>12. Additional Charges</div>");
            var row = data.FindingsRecommendations.FirstOrDefault(x => x.AccusationId == 0)
                ?? data.FindingsRecommendations.FirstOrDefault(x => (x.AccusationText ?? string.Empty).Trim().Equals("Additional Charges", StringComparison.OrdinalIgnoreCase));
            if (row == null) { sb.AppendLine("<p class='muted'>No additional charge found.</p></section>"); return; }
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='para-box'><div class='narrative'><strong>Finding:</strong> {0}</div><div class='narrative'><strong>Recommendation:</strong> {1}</div><div><strong>Outcome:</strong> {2}</div></div>", ToParagraphs(row.FindingText), ToParagraphs(row.RecommendationText), Encode(row.Outcome));
            sb.AppendLine("</section>");
            }

        private static void AppendDsa(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>13. DSA Section</div>");
            if (!data.DsaFiles.Any()) { sb.AppendLine("<p class='muted'>No DSA record.</p></section>"); return; }
            sb.AppendLine("<table class='meta-grid'><tr><th>Name</th><th>Designation</th><th>PP No</th><th>Status</th><th>Remarks</th></tr>");
            foreach (var row in data.DsaFiles)
                {
                sb.AppendFormat(CultureInfo.InvariantCulture, "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", Encode(row.PersonName), Encode(row.Designation), Encode(row.PpnoNumber), Encode(row.DsaStatus), Encode(row.Remarks));
                }
            sb.AppendLine("</table></section>");
            }

        private static void AppendFinalConclusion(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>14. Final Outcome / Conclusion</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'><strong>Findings Summary:</strong> {0}</div>", ToParagraphs(data.FinalConclusion?.Findings));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div class='narrative'><strong>Recommendation Summary:</strong> {0}</div>", ToParagraphs(data.FinalConclusion?.Recommendation));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div><strong>Final Outcome:</strong> {0}</div>", Encode(data.FinalConclusion?.FinalOutcome));
            sb.AppendLine("</section>");
            }

        private static void AppendSignature(StringBuilder sb, IidInquiryReportPdfData data)
            {
            sb.AppendLine("<section><div class='section-title'>15. Signature / Generation Info</div>");
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div>Generated By: {0}</div>", Encode(data.Header?.GeneratedByName));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div>PP No: {0}</div>", Encode(data.Header?.GeneratedByPPNo));
            sb.AppendFormat(CultureInfo.InvariantCulture, "<div>Generated On: {0}</div>", Encode(data.Header?.GeneratedOn.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture)));
            sb.AppendLine("</section>");
            }

        private static void AppendMetaRow(StringBuilder sb, string label, string value)
            {
            sb.AppendFormat(CultureInfo.InvariantCulture, "<tr><td class='label'>{0}</td><td>{1}</td></tr>", Encode(label), ToParagraphs(value));
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

        private static string FormatDate(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : "N/A";
            }
        }
    }

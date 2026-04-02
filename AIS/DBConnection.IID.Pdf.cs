using AIS.Models.IID;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public IidInquiryReportPdfData GetIidInquiryReportPdfData(long complaintId)
            {
            var complaint = GetComplaint((int)complaintId);
            if (complaint == null)
                {
                return null;
                }

            var accusations = GetIidInqAccusationsByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqAccusationRow>();
            var accused = GetIidInqAccusedListByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqAccusedRow>();
            var proceedings = GetIidInqProceedingsByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqProceedingRow>();
            var statements = GetIidInqStatementsByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqStatementRow>();
            var records = GetIidInqRecordsByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqRecordRow>();
            var evidence = GetIidInqEvidenceFilesByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqEvidenceFileRow>();
            var evidenceStep = GetIidInqEvidenceStepByComplaintId(complaintId) ?? new Models.IID.InquiryReport.IidInqEvidenceStepModel { ComplaintId = complaintId };
            var violations = GetIidInqViolationsByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqViolationRow>();
            var findingsRows = GetIidInqFindingsRecommByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqFindingsRecommRow>();
            var dsaRows = GetIidInqDsaByComplaintId(complaintId) ?? new List<Models.IID.InquiryReport.IidInqDsaRow>();
            var inquiryNarrative = GetLatestInquiryReportByComplaintId((int)complaintId);
            var planDetails = GetIidPlanDetails((int)complaintId);

            var accusationLookup = accusations.ToDictionary(x => x.AccusationId, x => x.AccusationText ?? string.Empty);

            string GetPlanValue(string key)
                {
                if (planDetails == null || string.IsNullOrWhiteSpace(key))
                    {
                    return string.Empty;
                    }

                return planDetails.TryGetValue(key, out var value)
                    ? value?.ToString() ?? string.Empty
                    : string.Empty;
                }

            return new IidInquiryReportPdfData
                {
                ComplaintId = complaintId,
                Header = new IidInquiryHeaderModel
                    {
                    BankName = "Zarai Taraqiati Bank Limited",
                    DepartmentName = "Internal Audit Division",
                    ReportTitle = "IID Inquiry Report",
                    ComplaintNo = complaint.ComplaintNo,
                    InquiryStatus = complaint.Status,
                    InspectionUnit = complaint.AssignedUnit,
                    TeamLead = GetPlanValue("teamLead"),
                    TeamMembers = GetPlanValue("teamMembers")
                    },
                ComplaintSnapshot = new IidComplaintSnapshotModel
                    {
                    ComplaintNo = complaint.ComplaintNo,
                    Nature = complaint.Nature,
                    Category = complaint.Category,
                    SubmittedOn = complaint.SubmittedOn,
                    Region = complaint.Region,
                    Branch = complaint.Branch,
                    LocationType = complaint.LocationTypeText,
                    ComplainantName = complaint.ComplainantName,
                    Cnic = complaint.CNIC,
                    CellularNumber = complaint.CellularNumber,
                    MailingAddress = complaint.MailingAddress,
                    Gender = complaint.Gender,
                    ReceivedFrom = complaint.ReceivedFrom,
                    ActionRequired = complaint.ActionRequired,
                    Contents = complaint.Contents,
                    UploadedComplaint = complaint.UploadedComplaint,
                    UploadedFFR = complaint.UploadedFFR,
                    UploadedEvidence = complaint.UploadedEvidence
                    },
                Accusations = accusations
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new IidAccusationRowModel
                        {
                        AccusationId = x.AccusationId,
                        AccusationText = x.AccusationText,
                        SortOrder = x.SortOrder
                        })
                    .ToList(),
                InquiryProceedings = proceedings
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.VisitDate)
                    .Select(x => new IidInquiryProceedingRowModel
                        {
                        NoticeReference = x.NoticeReference,
                        VisitDate = x.VisitDate,
                        PlaceVisited = x.PlaceVisited,
                        ParticipantsDetail = x.ParticipantsDetail,
                        MissingParticipantsReason = x.MissingParticipantsReason,
                        SortOrder = x.SortOrder
                        })
                    .ToList(),
                AccusedList = accused
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new IidAccusedRowModel
                        {
                        PersonName = x.PersonName,
                        FatherName = x.FatherName,
                        Designation = x.Designation,
                        RoleType = x.RoleType,
                        PpnoNumber = x.PpnoNumber,
                        Cnic = x.Cnic
                        })
                    .ToList(),
                Statements = statements
                    .OrderBy(x => x.StatementDatetime)
                    .Select(x => new IidStatementRowModel
                        {
                        PersonName = x.PersonName,
                        RoleType = x.RoleType,
                        PpnoNumber = x.PpnoNumber,
                        Cnic = x.Cnic,
                        StatementDatetime = x.StatementDatetime,
                        Place = x.Place,
                        ModeType = x.ModeType,
                        KeyPoints = x.KeyPoints,
                        UploadedStatement = x.UploadedStatement
                        })
                    .ToList(),
                RecordsScrutinized = records
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new IidRecordScrutinizedRowModel
                        {
                        RecordTitle = x.RecordTitle,
                        RecordDetails = x.RecordDetails,
                        SortOrder = x.SortOrder
                        })
                    .ToList(),
                EvidenceFiles = evidence
                    .OrderByDescending(x => x.UploadedOn)
                    .Select(x => new IidEvidenceFileRowModel
                        {
                        EvidenceType = x.EvidenceType,
                        Description = x.Description,
                        FileName = x.FileName,
                        FilePath = x.FilePath,
                        UploadedOn = x.UploadedOn
                        })
                    .ToList(),
                EvidenceSummary = new IidEvidenceSummaryModel
                    {
                    MaterialEvidenceDetail = evidenceStep.MaterialEvidenceDetail,
                    CircumstantialEvidenceDetail = evidenceStep.CircumstantialEvidenceDetail
                    },
                Violations = violations
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new IidViolationRowModel
                        {
                        Category = x.Category,
                        ViolationDetail = x.ViolationDetail,
                        ReferenceText = x.ReferenceText,
                        Recommendation = x.Recommendation,
                        SortOrder = x.SortOrder
                        })
                    .ToList(),
                FindingsRecommendations = findingsRows
                    .Select(x => new IidFindingRecommendationRowModel
                        {
                        AccusationId = x.AccusationId,
                        AccusationText = accusationLookup.TryGetValue(x.AccusationId, out var text) ? text : (x.AccusationId == 0 ? "Additional Charges" : string.Empty),
                        FindingText = x.FindingText,
                        RecommendationText = x.RecommendationText,
                        Outcome = x.Outcome,
                        UpdatedOn = x.UpdatedOn
                        })
                    .OrderBy(x => x.AccusationId == 0 ? int.MaxValue : x.AccusationId)
                    .ToList(),
                DsaFiles = dsaRows
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new IidDsaRowModel
                        {
                        PersonName = x.PersonName,
                        Designation = x.Designation,
                        PpnoNumber = x.PpnoNumber,
                        Cnic = x.Cnic,
                        DsaStatus = x.DsaStatus,
                        Remarks = x.Remarks,
                        SortOrder = x.SortOrder
                        })
                    .ToList(),
                FinalConclusion = new IidFinalConclusionModel
                    {
                    Gist = inquiryNarrative?.Gist,
                    Proceedings = inquiryNarrative?.Proceedings,
                    Findings = inquiryNarrative?.Findings,
                    Recommendation = inquiryNarrative?.Recommendation,
                    FinalOutcome = findingsRows.Select(x => x.Outcome).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                    InquiryStatus = complaint.Status
                    }
                };
            }

        public bool IsComplaintAllowedForIidPdf(long complaintId)
            {
            var sessionHandler = CreateSessionHandler();
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null || loggedInUser.UserRoleID <= 0)
                {
                return false;
                }

            using var con = DatabaseConnection();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PKG_INQ.GET_COMPLAINTS_DD";
            cmd.Parameters.Add("p_page_id", OracleDbType.Int32).Value = 5;
            cmd.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                {
                if (!reader.IsDBNull(reader.GetOrdinal("COMPLAINT_ID")) && Convert.ToInt64(reader["COMPLAINT_ID"]) == complaintId)
                    {
                    return true;
                    }
                }

            return false;
            }
        }
    }

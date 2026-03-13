using System;
using System.Collections.Generic;

namespace AIS.Models.IID
    {
    public class IidInquiryReportPdfData
        {
        public long ComplaintId { get; set; }
        public IidInquiryHeaderModel Header { get; set; } = new IidInquiryHeaderModel();
        public IidComplaintSnapshotModel ComplaintSnapshot { get; set; } = new IidComplaintSnapshotModel();
        public List<IidAccusedRowModel> AccusedList { get; set; } = new List<IidAccusedRowModel>();
        public List<IidAccusationRowModel> Accusations { get; set; } = new List<IidAccusationRowModel>();
        public List<IidStatementRowModel> Statements { get; set; } = new List<IidStatementRowModel>();
        public List<IidRecordScrutinizedRowModel> RecordsScrutinized { get; set; } = new List<IidRecordScrutinizedRowModel>();
        public List<IidEvidenceFileRowModel> EvidenceFiles { get; set; } = new List<IidEvidenceFileRowModel>();
        public List<IidViolationRowModel> Violations { get; set; } = new List<IidViolationRowModel>();
        public List<IidFindingRecommendationRowModel> FindingsRecommendations { get; set; } = new List<IidFindingRecommendationRowModel>();
        public List<IidDsaRowModel> DsaFiles { get; set; } = new List<IidDsaRowModel>();
        public IidFinalConclusionModel FinalConclusion { get; set; } = new IidFinalConclusionModel();
        }

    public class IidInquiryHeaderModel
        {
        public string BankName { get; set; }
        public string DepartmentName { get; set; }
        public string ReportTitle { get; set; }
        public string ComplaintNo { get; set; }
        public string InquiryStatus { get; set; }
        public string GeneratedByName { get; set; }
        public string GeneratedByPPNo { get; set; }
        public DateTime GeneratedOn { get; set; }
        }

    public class IidComplaintSnapshotModel
        {
        public string ComplaintNo { get; set; }
        public string Nature { get; set; }
        public string Category { get; set; }
        public string SubmittedOn { get; set; }
        public string Region { get; set; }
        public string Branch { get; set; }
        public string LocationType { get; set; }
        public string ComplainantName { get; set; }
        public string Cnic { get; set; }
        public string CellularNumber { get; set; }
        public string MailingAddress { get; set; }
        public string Gender { get; set; }
        public string ReceivedFrom { get; set; }
        public string ActionRequired { get; set; }
        public string Contents { get; set; }
        public string UploadedComplaint { get; set; }
        public string UploadedFFR { get; set; }
        public string UploadedEvidence { get; set; }
        }

    public class IidAccusationRowModel
        {
        public long AccusationId { get; set; }
        public string AccusationText { get; set; }
        public int SortOrder { get; set; }
        }

    public class IidAccusedRowModel
        {
        public string PersonName { get; set; }
        public string FatherName { get; set; }
        public string Designation { get; set; }
        public string RoleType { get; set; }
        public string PpnoNumber { get; set; }
        public string Cnic { get; set; }
        public string PostingPlace { get; set; }
        }

    public class IidStatementRowModel
        {
        public string PersonName { get; set; }
        public string RoleType { get; set; }
        public string StatementType { get; set; }
        public string PpnoNumber { get; set; }
        public string Cnic { get; set; }
        public DateTime? StatementDatetime { get; set; }
        public string Place { get; set; }
        public string ModeType { get; set; }
        public string KeyPoints { get; set; }
        public string UploadedStatement { get; set; }
        }

    public class IidRecordScrutinizedRowModel
        {
        public string RecordTitle { get; set; }
        public string RecordDetails { get; set; }
        public int SortOrder { get; set; }
        }

    public class IidEvidenceFileRowModel
        {
        public string EvidenceType { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? UploadedOn { get; set; }
        }

    public class IidViolationRowModel
        {
        public string Category { get; set; }
        public string ViolationDetail { get; set; }
        public string ReferenceText { get; set; }
        public string Recommendation { get; set; }
        public int SortOrder { get; set; }
        }

    public class IidFindingRecommendationRowModel
        {
        public long AccusationId { get; set; }
        public string AccusationText { get; set; }
        public string FindingText { get; set; }
        public string RecommendationText { get; set; }
        public string Outcome { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }

    public class IidDsaRowModel
        {
        public string PersonName { get; set; }
        public string Designation { get; set; }
        public string PpnoNumber { get; set; }
        public string Cnic { get; set; }
        public string DsaStatus { get; set; }
        public string Remarks { get; set; }
        public int SortOrder { get; set; }
        }

    public class IidFinalConclusionModel
        {
        public string Gist { get; set; }
        public string Proceedings { get; set; }
        public string Findings { get; set; }
        public string Recommendation { get; set; }
        public string FinalOutcome { get; set; }
        public string InquiryStatus { get; set; }
        }
    }

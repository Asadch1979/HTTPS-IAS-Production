using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AIS.Models.WorkingPaperV2
    {
    public static class WorkingPaperTypes
        {
        public const string LoanCase = "LCF";
        public const string Voucher = "VCH";
        public const string AccountOpening = "AOF";
        public const string FixedAsset = "FAS";
        public const string CashCount = "CCT";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { LoanCase, Voucher, AccountOpening, FixedAsset, CashCount };
        }

    public static class WorkingPaperStatuses
        {
        public const string Draft = "DRAFT";
        public const string InReview = "IN_REVIEW";
        public const string Returned = "RETURNED";
        public const string Reviewed = "REVIEWED";
        public const string Approved = "APPROVED";
        }

    public sealed class WorkingPaperWorkspaceModel
        {
        public long WorkingPaperId { get; set; }
        public long EngagementId { get; set; }
        public int EntityId { get; set; }
        public string PaperType { get; set; }
        public string ReferenceNo { get; set; }
        public int VersionNo { get; set; }
        public string Status { get; set; }
        public string PreparerPpno { get; set; }
        public string ReviewerPpno { get; set; }
        public DateTime? DueDate { get; set; }
        public long RowVersion { get; set; }
        public WorkingPaperPlanModel Plan { get; set; } = new WorkingPaperPlanModel();
        public WorkingPaperConclusionModel Conclusion { get; set; } = new WorkingPaperConclusionModel();
        public List<WorkingPaperItemModel> Items { get; set; } = new List<WorkingPaperItemModel>();
        public List<WorkingPaperEvidenceModel> Evidence { get; set; } = new List<WorkingPaperEvidenceModel>();
        public List<WorkingPaperExceptionModel> Exceptions { get; set; } = new List<WorkingPaperExceptionModel>();
        public List<WorkingPaperReviewNoteModel> ReviewNotes { get; set; } = new List<WorkingPaperReviewNoteModel>();
        public List<WorkingPaperHistoryModel> History { get; set; } = new List<WorkingPaperHistoryModel>();
        public List<WorkingPaperLegacyRecordModel> LegacyRecords { get; set; } = new List<WorkingPaperLegacyRecordModel>();
        }

    public sealed class CreateWorkingPaperRequest
        {
        [Range(1, long.MaxValue)] public long EngagementId { get; set; }
        [Required, StringLength(3, MinimumLength = 3)] public string PaperType { get; set; }
        [Required, StringLength(30)] public string ReviewerPpno { get; set; }
        public DateTime? DueDate { get; set; }
        }

    public sealed class WorkingPaperPlanModel
        {
        [StringLength(4000)] public string Objectives { get; set; }
        [StringLength(4000)] public string Risks { get; set; }
        [StringLength(4000)] public string Controls { get; set; }
        [StringLength(4000)] public string Scope { get; set; }
        [StringLength(1000)] public string PopulationSource { get; set; }
        public DateTime? PopulationAsOf { get; set; }
        [Range(0, long.MaxValue)] public long? PopulationCount { get; set; }
        [Range(typeof(decimal), "0", "999999999999999999")] public decimal? PopulationValue { get; set; }
        [StringLength(3)] public string CurrencyCode { get; set; }
        [StringLength(30)] public string SamplingMethod { get; set; }
        [Range(0, int.MaxValue)] public int? SampleSize { get; set; }
        [StringLength(4000)] public string SamplingRationale { get; set; }
        [StringLength(200)] public string AuditProgramReference { get; set; }
        }

    public sealed class SavePlanRequest
        {
        [Range(1, long.MaxValue)] public long WorkingPaperId { get; set; }
        [Range(0, long.MaxValue)] public long RowVersion { get; set; }
        [Required] public WorkingPaperPlanModel Plan { get; set; }
        }

    public sealed class WorkingPaperItemModel
        {
        public long ItemId { get; set; }
        public long WorkingPaperId { get; set; }
        [Required, StringLength(120)] public string ItemReference { get; set; }
        [Required] public JsonElement Details { get; set; }
        [Required, RegularExpression("PASS|EXCEPTION|NOT_APPLICABLE|NOT_TESTED|PENDING")] public string Result { get; set; }
        [StringLength(4000)] public string AuditorComment { get; set; }
        public long RowVersion { get; set; }
        }

    public sealed class SaveItemRequest
        {
        [Range(1, long.MaxValue)] public long WorkingPaperId { get; set; }
        public long ItemId { get; set; }
        [Required, StringLength(120)] public string ItemReference { get; set; }
        [Required] public JsonElement Details { get; set; }
        [Required] public string Result { get; set; }
        [StringLength(4000)] public string AuditorComment { get; set; }
        [Range(0, long.MaxValue)] public long RowVersion { get; set; }
        }

    public sealed class WorkingPaperEvidenceModel
        {
        public long EvidenceLinkId { get; set; }
        public long WorkingPaperId { get; set; }
        public long? ItemId { get; set; }
        public long? ExceptionId { get; set; }
        [Required, StringLength(100)] public string ExistingEvidenceId { get; set; }
        [Required, StringLength(300)] public string Title { get; set; }
        [StringLength(100)] public string EvidenceType { get; set; }
        [StringLength(500)] public string Source { get; set; }
        public DateTime? EvidenceDate { get; set; }
        }

    public sealed class WorkingPaperExceptionModel
        {
        public long ExceptionId { get; set; }
        public long WorkingPaperId { get; set; }
        public long ItemId { get; set; }
        [Required, StringLength(4000)] public string Criteria { get; set; }
        [Required, StringLength(4000)] public string Condition { get; set; }
        [StringLength(4000)] public string Cause { get; set; }
        [Required, StringLength(4000)] public string Impact { get; set; }
        [Required, RegularExpression("LOW|MEDIUM|HIGH|CRITICAL")] public string RiskRating { get; set; }
        [StringLength(30)] public string OwnerPpno { get; set; }
        public DateTime? DueDate { get; set; }
        public long? ObservationId { get; set; }
        [StringLength(30)] public string Status { get; set; }
        public long RowVersion { get; set; }
        }

    public sealed class WorkingPaperConclusionModel
        {
        [RegularExpression("ACHIEVED|PARTLY_ACHIEVED|NOT_ACHIEVED|PENDING")] public string ObjectiveAssessment { get; set; }
        [StringLength(4000)] public string OverallConclusion { get; set; }
        [StringLength(4000)] public string UnresolvedIssues { get; set; }
        [StringLength(30)] public string ResidualRisk { get; set; }
        }

    public sealed class SaveConclusionRequest
        {
        [Range(1, long.MaxValue)] public long WorkingPaperId { get; set; }
        [Range(0, long.MaxValue)] public long RowVersion { get; set; }
        [Required] public WorkingPaperConclusionModel Conclusion { get; set; }
        }

    public sealed class WorkingPaperReviewNoteModel
        {
        public long ReviewNoteId { get; set; }
        public long WorkingPaperId { get; set; }
        [Required, StringLength(100)] public string SectionKey { get; set; }
        [Required, StringLength(4000)] public string NoteText { get; set; }
        [StringLength(4000)] public string ResponseText { get; set; }
        public string Status { get; set; }
        public string RaisedBy { get; set; }
        public DateTime? RaisedOn { get; set; }
        }

    public sealed class WorkflowActionRequest
        {
        [Range(1, long.MaxValue)] public long WorkingPaperId { get; set; }
        [Required, RegularExpression("SUBMIT|RETURN|REVIEW|APPROVE|REOPEN")] public string Action { get; set; }
        [StringLength(2000)] public string Remarks { get; set; }
        [Range(0, long.MaxValue)] public long RowVersion { get; set; }
        }

    public sealed class WorkingPaperHistoryModel
        {
        public long HistoryId { get; set; }
        public string Action { get; set; }
        public string ActorPpno { get; set; }
        public DateTime ActionOn { get; set; }
        public string Details { get; set; }
        }

    public sealed class WorkingPaperLegacyRecordModel
        {
        public string SourceTable { get; set; }
        public string SourceId { get; set; }
        public string DisplayReference { get; set; }
        public string LegacyValuesJson { get; set; }
        }

    public sealed class WorkingPaperOperationResult
        {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long WorkingPaperId { get; set; }
        public long RecordId { get; set; }
        public long RowVersion { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
        }
    }

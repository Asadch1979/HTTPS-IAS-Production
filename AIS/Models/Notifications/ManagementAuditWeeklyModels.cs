using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models.Notifications
    {
    public class ManagementAuditWeeklyDivisionSummaryModel
        {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string CcEmail { get; set; } = string.Empty;
        public int SettledCount { get; set; }
        public int RejectedCount { get; set; }
        public int NoComplianceCount { get; set; }
        public int TotalCount { get; set; }
        }

    public class ManagementAuditWeeklyDivisionDetailModel
        {
        public int DecisionHistoryId { get; set; }
        public int ComId { get; set; }
        public int ComCycle { get; set; }
        public int EntityId { get; set; }
        public int AuditedBy { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string AuditPeriod { get; set; } = string.Empty;
        public string ParaNo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? SubmittedOn { get; set; }
        public DateTime DecisionOn { get; set; }
        public string DecisionStatus { get; set; } = string.Empty;
        public int ComStatus { get; set; }
        public string Reason { get; set; } = string.Empty;
        }

    public class ManagementAuditWeeklyNoComplianceModel
        {
        public int ComId { get; set; }
        public int ComCycle { get; set; }
        public int EntityId { get; set; }
        public int AuditedBy { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string AuditPeriod { get; set; } = string.Empty;
        public string ParaNo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Risk { get; set; } = string.Empty;
        public DateTime? LastComplianceSubmittedOn { get; set; }
        public int? ParaStatus { get; set; }
        public int? ComStatus { get; set; }
        public int? ComStage { get; set; }
        }

    public class ManagementAuditNotificationAdminViewModel
        {
        public List<ManagementAuditNotificationDivisionModel> Divisions { get; set; } = new();
        public List<ManagementAuditNotificationDivisionOptionModel> DivisionOptions { get; set; } = new();
        }

    public class ManagementAuditNotificationDivisionOptionModel
        {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        }

    public class ManagementAuditNotificationDivisionModel
        {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string ReportingOffice { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool WeeklyEmailEnabled { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public List<ManagementAuditNotificationRecipientModel> Recipients { get; set; } = new();
        }

    public class ManagementAuditNotificationRecipientModel
        {
        public int RecipientId { get; set; }
        public int DivisionId { get; set; }
        public string RecipientType { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int DisplayOrder { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        }

    public class SaveManagementAuditNotificationDivisionModel
        {
        [Range(1, int.MaxValue, ErrorMessage = "Division is required.")]
        public int DivisionId { get; set; }

        [Required(ErrorMessage = "Division name is required.")]
        [StringLength(200)]
        public string DivisionName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public bool WeeklyEmailEnabled { get; set; }

        [Required(ErrorMessage = "Effective From is required.")]
        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; } = string.Empty;
        }

    public class SaveManagementAuditNotificationRecipientModel
        {
        public int? RecipientId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Division is required.")]
        public int DivisionId { get; set; }

        [Required(ErrorMessage = "Recipient type is required.")]
        [RegularExpression("^(?i:TO|CC)$", ErrorMessage = "Recipient type must be TO or CC.")]
        public string RecipientType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(500)]
        public string EmailAddress { get; set; } = string.Empty;

        [StringLength(200)]
        public string PersonName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Designation { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Effective From is required.")]
        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [Range(1, 999, ErrorMessage = "Display order must be between 1 and 999.")]
        public int DisplayOrder { get; set; } = 1;
        }
    }

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace AIS.Models.Notifications
    {
    public class ManagementAuditWeeklyDivisionSummaryModel
        {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string CcEmail { get; set; } = string.Empty;
        public string BccEmail { get; set; } = string.Empty;
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
        public bool IsConfigured { get; set; }
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

    public class SaveManagementAuditNotificationDivisionModel : IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
            if (EffectiveFrom.HasValue && EffectiveTo.HasValue && EffectiveTo.Value.Date < EffectiveFrom.Value.Date)
                {
                yield return new ValidationResult(
                    "Effective To cannot be earlier than Effective From.",
                    new[] { nameof(EffectiveTo) });
                }
            }
        }

    public class SaveManagementAuditNotificationRecipientModel : IValidatableObject
        {
        public int? RecipientId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Division is required.")]
        public int DivisionId { get; set; }

        [Required(ErrorMessage = "Recipient type is required.")]
        [RegularExpression("^(?i:TO|CC|BCC)$", ErrorMessage = "Recipient type must be TO, CC or BCC.")]
        public string RecipientType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [SemicolonSeparatedEmailList]
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
            if (EffectiveFrom.HasValue && EffectiveTo.HasValue && EffectiveTo.Value.Date < EffectiveFrom.Value.Date)
                {
                yield return new ValidationResult(
                    "Effective To cannot be earlier than Effective From.",
                    new[] { nameof(EffectiveTo) });
                }
            }
        }

    public sealed class SemicolonSeparatedEmailListAttribute : ValidationAttribute
        {
        public SemicolonSeparatedEmailListAttribute()
            : base("Enter valid email addresses separated by semicolons. Empty entries are not allowed.")
            {
            }

        public override bool IsValid(object value)
            {
            return value == null || ManagementAuditRecipientEmailList.TryNormalize(value.ToString(), out _);
            }
        }

    public static class ManagementAuditRecipientEmailList
        {
        public static bool TryNormalize(string value, out string normalized)
            {
            normalized = string.Empty;
            if (string.IsNullOrWhiteSpace(value) || value.Contains(',') || value.Contains('\r') || value.Contains('\n'))
                {
                return false;
                }

            var parts = value.Split(new[] { ';' }, StringSplitOptions.None);
            if (parts.Any(part => string.IsNullOrWhiteSpace(part)))
                {
                return false;
                }

            var addresses = new List<string>();
            foreach (var part in parts)
                {
                var address = part.Trim();
                try
                    {
                    var parsed = new MailAddress(address);
                    if (!string.Equals(parsed.Address, address, StringComparison.OrdinalIgnoreCase))
                        {
                        return false;
                        }
                    }
                catch (FormatException)
                    {
                    return false;
                    }

                if (!addresses.Contains(address, StringComparer.OrdinalIgnoreCase))
                    {
                    addresses.Add(address);
                    }
                }

            normalized = string.Join("; ", addresses);
            return normalized.Length <= 500;
            }
        }
    }

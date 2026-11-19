using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace AIS.Models
{
    public class FadRiskAssessmentRequest : IValidatableObject
    {
        [Range(1, int.MaxValue)] public int EngagementId { get; set; }
        [Range(0, int.MaxValue)] public int AssessmentId { get; set; }
        [Range(1, int.MaxValue)] public int VersionId { get; set; }
        [Range(0, int.MaxValue)] public int Revision { get; set; }
        [Required, RegularExpression("^(Small|Medium|Large)$")] public string BranchSize { get; set; }
        [Required, Range(typeof(decimal), "0", "999999999999999999")] public decimal? Portfolio { get; set; }
        [Required, StringLength(2000)] public string SizeReason { get; set; }
        [Required, MinLength(1), MaxLength(500)] public List<FadRiskDetailInput> Details { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (AssessmentId > 0 && Revision < 1) yield return new ValidationResult("Revision is required.");
            if (Details != null && (Details.Any(d => d == null) || Details.Select(d => d?.Code).Distinct().Count() != Details.Count))
                yield return new ValidationResult("Detail codes must be unique and nonempty.");
        }
    }
    public class FadRiskDetailInput : IValidatableObject
    {
        [Required, StringLength(50), RegularExpression("^[A-Za-z0-9-]+$")]
        [JsonPropertyName("code")] public string Code { get; set; }
        [Range(typeof(decimal), "0", "999999999999999999")]
        [JsonPropertyName("instances")] public decimal? Instances { get; set; }
        [Range(typeof(decimal), "0", "999999999999999999")]
        [JsonPropertyName("base")] public decimal? Base { get; set; }
        [Range(typeof(decimal), "-999999999999999999", "999999999999999999")]
        [JsonPropertyName("actual")] public decimal? Actual { get; set; }
        [RegularExpression("^[0-9]{1,18}$"), StringLength(18)]
        [JsonPropertyName("observation")] public string Observation { get; set; }
        [RegularExpression("^(WP:)?[0-9]{1,18}$"), StringLength(21)]
        [JsonPropertyName("evidence")] public string Evidence { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (Instances.HasValue && (Instances != decimal.Truncate(Instances.Value) || Base.HasValue || Actual.HasValue))
                yield return new ValidationResult("Instances must be whole counts without financial values.");
            if (Instances > 0 && (string.IsNullOrWhiteSpace(Observation) || string.IsNullOrWhiteSpace(Evidence)))
                yield return new ValidationResult("Positive counts require observation and evidence IDs.");
            if (!Instances.HasValue && (!Actual.HasValue || string.IsNullOrWhiteSpace(Evidence)))
                yield return new ValidationResult("Financial values and evidence are required.");
            if (Base.HasValue && Base <= 0) yield return new ValidationResult("Zero denominators require review.");
            if (Evidence?.StartsWith("WP:") == true && (Instances.HasValue || !string.IsNullOrEmpty(Observation)))
                yield return new ValidationResult("Working-paper evidence is for financial inputs without an observation.");
            if (Evidence != null && !Evidence.StartsWith("WP:") && string.IsNullOrWhiteSpace(Observation))
                yield return new ValidationResult("Observation evidence requires an observation ID.");
            if (!string.IsNullOrWhiteSpace(Observation) && string.IsNullOrWhiteSpace(Evidence))
                yield return new ValidationResult("An evidence ID is required with an observation.");
        }
    }
    public class FadRiskTransitionRequest : IValidatableObject
    {
        [Range(1, int.MaxValue)] public int EngagementId { get; set; }
        [Range(1, int.MaxValue)] public int AssessmentId { get; set; }
        [Range(1, int.MaxValue)] public int Revision { get; set; }
        [Required, RegularExpression("^(SUBMIT|APPROVE|RETURN)$")] public string Action { get; set; }
        [StringLength(2000)] public string Remarks { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (Action != "SUBMIT" && string.IsNullOrWhiteSpace(Remarks)) yield return new ValidationResult("Reviewer remarks are required.");
        }
    }
}

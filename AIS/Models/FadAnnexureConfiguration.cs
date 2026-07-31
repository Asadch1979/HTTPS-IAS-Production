using System;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public sealed class FadAnnexureConfiguration
        {
        public int AnnexureId { get; set; }
        public string AnnexureCode { get; set; } = string.Empty;
        public string Description { get; set; }
        public string ShiftApplicable { get; set; } = "N";
        public string Active { get; set; } = "Y";
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }

    public sealed class UpdateFadAnnexureStatusRequest
        {
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Annexure ID.")]
        public int AnnexureId { get; set; }

        [Required]
        [RegularExpression("^[YNyn]$", ErrorMessage = "Status must be Y or N.")]
        public string ShiftApplicable { get; set; } = string.Empty;
        }

    public sealed class FadAnnexureStatusResult
        {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }

using AIS.Validation;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
    {
    public class MemoDraftParaUpdateRow
        {
        public int EngagementId { get; set; }
        public int ObservationId { get; set; }
        public int MemoNumber { get; set; }
        public int DraftParaNumber { get; set; }
        [PlainText]
        public string ObservationTitle { get; set; }
        [PlainText]
        public string EntityName { get; set; }
        public int StatusId { get; set; }
        [PlainText]
        public string StatusName { get; set; }
        public bool IsFinalized { get; set; }
        }

    public sealed class UpdateMemoDraftParaRequest
        {
        public int EngagementId { get; set; }
        public int ObservationId { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Memo Number must contain digits only.")]
        public string MemoNumber { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Draft Para Number must contain digits only.")]
        public string DraftParaNumber { get; set; }
        }
    }

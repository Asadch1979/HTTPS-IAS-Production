using AIS.Validation;

namespace AIS.Models
    {
    public sealed class EngagementEntityShiftRequestModel
        {
        public int EngagementId { get; set; }
        public int NewEntityId { get; set; }

        [PlainText]
        public string Reason { get; set; }
        }

    public sealed class EngagementEntityShiftResponseModel
        {
        public bool Status { get; set; }

        [PlainText]
        public string Remarks { get; set; } = string.Empty;
        }
    }

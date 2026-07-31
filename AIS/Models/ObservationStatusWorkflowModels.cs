namespace AIS.Models
    {
    public sealed class AddObservationToDraftRequest
        {
        public int ObservationId { get; set; }
        public string DraftParaNumber { get; set; }
        public string Remarks { get; set; }
        }

    public sealed class FinalizeOrSettleObservationRequest
        {
        public int ObservationId { get; set; }
        public int NewStatusId { get; set; }
        public int? FinalParaNumber { get; set; }
        public string Remarks { get; set; }
        }

    public sealed class ObservationStatusWorkflowResult
        {
        public bool Success { get; set; }
        public string Reference { get; set; }
        public string Remarks { get; set; }
        }
    }

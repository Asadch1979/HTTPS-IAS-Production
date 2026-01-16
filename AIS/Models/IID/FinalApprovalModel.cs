using AIS.Validation;
namespace AIS.Models.IID
    {
    public class FinalApprovalModel
        {
        public int? ReportId { get; set; }
        [PlainText]
        public string Comments { get; set; }
        [PlainText]
        public string Decision { get; set; }
        }

    }

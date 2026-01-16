using AIS.Validation;
namespace AIS.Models
    {
    public class ErrorViewModel
        {
        [PlainText]
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        }
    }

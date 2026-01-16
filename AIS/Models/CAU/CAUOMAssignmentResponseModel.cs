using AIS.Validation;
namespace AIS.Models
    {
    public class CAUOMAssignmentResponseModel
        {
        public int ID { get; set; }
        [PlainText]
        public string RESPONSE { get; set; }
        }
    }

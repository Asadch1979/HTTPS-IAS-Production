using AIS.Validation;
namespace AIS.Models
    {
    public class DraftDSAGuidelines
        {
        public int ID { get; set; }
        [PlainText]
        public string PARTICULARS { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }

using AIS.Validation;
namespace AIS.Models
    {
    public class DivisionModel
        {
        public int DIVISIONID { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }

        }
    }

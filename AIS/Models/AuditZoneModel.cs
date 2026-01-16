using AIS.Validation;
namespace AIS.Models
    {
    public class AuditZoneModel
        {
        public int ID { get; set; }
        [PlainText]
        public string ZONECODE { get; set; }
        [PlainText]
        public string ZONENAME { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }

        }
    }

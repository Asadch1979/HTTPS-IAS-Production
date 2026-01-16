using AIS.Validation;
namespace AIS.Models
    {
    public class FADAuditManpowerModel
        {
        public int ID { get; set; }
        [PlainText]
        public string COMPANY { get; set; }
        [PlainText]
        public string RANK { get; set; }
        [PlainText]
        public string PLACEMENT { get; set; }
        [PlainText]
        public string EXISTING { get; set; }
        [PlainText]
        public string ADDITIONAL_REQUIRED { get; set; }
        }
    }

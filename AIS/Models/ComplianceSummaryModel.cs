using AIS.Validation;
namespace AIS.Models
    {
    public class ComplianceSummaryModel
        {
        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string TOTAL_PARA { get; set; }
        [PlainText]
        public string TOTAL_COMPLIANCE { get; set; }
        [PlainText]
        public string AT_REPORTING { get; set; }
        [PlainText]
        public string UNDER_CONSIDERATION { get; set; }
        [PlainText]
        public string REJECTED { get; set; }
        [PlainText]
        public string SETTLED { get; set; }
        [PlainText]
        public string ROLE_ID { get; set; }




        }
    }

using AIS.Validation;
namespace AIS.Models
    {
    public class FADAuditBudgetModel
        {
        public int ID { get; set; }
        [PlainText]
        public string GL_CODE { get; set; }
        [PlainText]
        public string GL_HEADING { get; set; }
        [PlainText]
        public string EXISTING { get; set; }
        [PlainText]
        public string UTILIZATION { get; set; }
        [PlainText]
        public string REMAND { get; set; }
        [PlainText]
        public string RATIONALIZATION { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        }
    }

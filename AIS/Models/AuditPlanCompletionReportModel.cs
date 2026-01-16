using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPlanCompletionReportModel
        {
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string AUDITNAME { get; set; }
        public int AUDITS { get; set; }
        public int ENGPLAN { get; set; }
        public int JOINING { get; set; }
        public int COMPLETED { get; set; }
        public int OBSERVATIONS { get; set; }
        public int HIGHRISKPARA { get; set; }
        public int MEDIUMRISKPARA { get; set; }
        public int LOWRISKPARA { get; set; }

        }
    }

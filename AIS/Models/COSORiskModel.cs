using AIS.Validation;
namespace AIS.Models
    {
    public class COSORiskModel
        {
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string DEPT_NAME { get; set; }
        [PlainText]
        public string RATING_FACTORS { get; set; }
        public int SUB_FACTORS { get; set; }
        public int MAX_SCORE { get; set; }
        public int FINAL_SCORE { get; set; }
        public int WEIGHT_ASSIGNED { get; set; }
        public int NO_OF_OBSERVATIONS { get; set; }
        public int WEIGHTED_AVERAGE_SCORE { get; set; }
        [PlainText]
        public string AUDIT_RATING { get; set; }
        [PlainText]
        public string FINAL_AUDIT_RATING { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        }
    }

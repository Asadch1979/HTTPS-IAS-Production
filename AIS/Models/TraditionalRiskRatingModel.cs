using AIS.Validation;
namespace AIS.Models
    {
    public class TraditionalRiskRatingModel
        {

        [PlainText]
        public string MAIN_PROCESS { get; set; }
        [PlainText]
        public string RISK_MODEL { get; set; }
        [PlainText]
        public string MAX_NUMBER { get; set; }
        [PlainText]
        public string WEIGHTAGE_AVERAGE { get; set; }
        [PlainText]
        public string GRAVITY_RISK { get; set; }
        [PlainText]
        public string NO_OF_OBSERVATIONS { get; set; }
        [PlainText]
        public string RISK_BASED_MARKS { get; set; }
        [PlainText]
        public string WEIGHTED_AVERAGE_MARKS { get; set; }

        [PlainText]
        public string CIA_MARKS { get; set; }


        }
    }

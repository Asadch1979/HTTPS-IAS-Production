using AIS.Validation;
namespace AIS.Models
    {
    public class DepttWiseOutstandingParasModel
        {

        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }

        [PlainText]
        public string AGE { get; set; }
        [PlainText]
        public string TOTAL_PARAS { get; set; }


        }
    }

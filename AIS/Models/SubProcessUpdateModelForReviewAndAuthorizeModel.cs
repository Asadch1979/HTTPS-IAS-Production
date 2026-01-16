using AIS.Validation;
namespace AIS.Models
    {
    public class SubProcessUpdateModelForReviewAndAuthorizeModel
        {
        [PlainText]
        public string P_ID { get; set; }
        [PlainText]
        public string NEW_P_ID { get; set; }
        [PlainText]
        public string SP_ID { get; set; }
        [PlainText]
        public string NEW_SP_ID { get; set; }
        [PlainText]
        public string PROCESS_NAME { get; set; }
        [PlainText]
        public string NEW_PROCESS_NAME { get; set; }
        [PlainText]
        public string SUB_PROCESS_NAME { get; set; }
        [PlainText]
        public string NEW_SUB_PROCESS_NAME { get; set; }
        [PlainText]
        public string COMMENTS { get; set; }


        }
    }

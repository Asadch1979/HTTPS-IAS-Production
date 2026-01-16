using AIS.Validation;
namespace AIS.Models
    {
    public class CriteriaIDComment
        {
        public int? ID { get; set; }
        [PlainText]
        public string COMMENT { get; set; }
        }
    }

using AIS.Validation;
namespace AIS.Models
    {
    public class GroupModel
        {
        public int? GROUP_ID { get; set; }
        [PlainText]
        public string GROUP_NAME { get; set; }
        [PlainText]
        public string GROUP_DESCRIPTION { get; set; }
        [PlainText]
        public string ISACTIVE { get; set; }
        public int? GROUP_CODE { get; set; }
        }
    }

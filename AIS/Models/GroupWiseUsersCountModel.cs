using AIS.Validation;
namespace AIS.Models
    {
    public class GroupWiseUsersCountModel
        {
        [PlainText]
        public string G_ID { get; set; }
        [PlainText]
        public string G_NAME { get; set; }
        [PlainText]
        public string U_COUNT { get; set; }


        }
    }

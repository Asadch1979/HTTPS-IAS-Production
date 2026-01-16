using AIS.Validation;
namespace AIS.Models
    {
    public class UserRoleDetailAdminPanelModel
        {
        [PlainText]
        public string ROLE_ID { get; set; }
        [PlainText]
        public string GROUP_ID { get; set; }
        [PlainText]
        public string GROUP_NAME { get; set; }
        }
    }


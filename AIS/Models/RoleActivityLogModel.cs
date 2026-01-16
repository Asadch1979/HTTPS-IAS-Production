using AIS.Validation;
namespace AIS.Models
    {
    public class RoleActivityLogModel
        {

        [PlainText]
        public string USER_PP_NUMBER { get; set; }
        [PlainText]
        public string USER_NAME { get; set; }
        [PlainText]
        public string START_DATE { get; set; }
        [PlainText]
        public string END_DATE { get; set; }
        [PlainText]
        public string ACT_DATE { get; set; }
        [PlainText]
        public string ACTIVITY { get; set; }
        [PlainText]
        public string ACTIONS { get; set; }
        [PlainText]
        public string DURATION { get; set; }

        }
    }

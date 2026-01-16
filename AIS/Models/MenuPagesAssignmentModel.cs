using AIS.Validation;
namespace AIS.Models
    {
    public class MenuPagesAssignmentModel
        {
        [PlainText]
        public string P_ID { get; set; }
        [PlainText]
        public string M_ID { get; set; }
        [PlainText]
        public string SM_ID { get; set; }
        [PlainText]
        public string SM_NAME { get; set; }
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string P_PATH { get; set; }
        [PlainText]
        public string P_ORDER { get; set; }
        [PlainText]
        public string P_STATUS { get; set; }
        [PlainText]
        public string P_HIDE_MENU { get; set; }
        }
    }

using AIS.Validation;
namespace AIS.Models
    {
    public class SubMenuModel
        {
        [PlainText]
        public string SUB_MENU_ID { get; set; }
        [PlainText]
        public string MENU_ID { get; set; }
        [PlainText]
        public string SUB_MENU_NAME { get; set; }
        [PlainText]
        public string SUB_MENU_ORDER { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        }
    }
